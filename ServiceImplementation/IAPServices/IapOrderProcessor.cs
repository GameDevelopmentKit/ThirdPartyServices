namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using GameBusiness.Transactions.Exceptions;

    /// <summary>Store-agnostic snapshot of a pending store order.</summary>
    public sealed class IapOrder
    {
        public string                           TransactionId { get; }
        public string                           Receipt       { get; }
        public IReadOnlyList<IapReceiptProduct> Products      { get; }

        /// <summary>Store object needed to confirm the order (e.g. Unity IAP PendingOrder).</summary>
        public object Handle { get; }

        public IapOrder(string transactionId, string receipt, IReadOnlyList<IapReceiptProduct> products, object handle)
        {
            this.TransactionId = transactionId;
            this.Receipt       = receipt;
            this.Products      = products ?? Array.Empty<IapReceiptProduct>();
            this.Handle        = handle;
        }
    }

    /// <summary>Step of a store order, reported through <see cref="IapOrderProcessor.Flow"/> for analytics (<c>iap_flow</c>).</summary>
    public enum IapFlowStep
    {
        Click,
        StoreSuccess,
        StoreFail,
        Cancel,
        Pending,
        ValidateFail,
        Delivered,
        Redelivered,
        DeliverFail,
        ConfirmFail
    }

    /// <summary>One <see cref="IapFlowStep"/> of one product of a store order.</summary>
    public sealed class IapFlowEvent
    {
        private const int MaxErrorLength = 100;

        public IapFlowStep Step          { get; }
        public string      ProductId     { get; }
        public string      TransactionId { get; }

        /// <summary>Store receipt, when known. Not meant to be tracked as is (e.g. to extract the Google order id).</summary>
        public string Receipt { get; }

        /// <summary>The order was first seen in this session without a purchase started by the player. Fixed per transaction
        /// for the session, so every step of one order reports the same value.</summary>
        public bool IsDelayed { get; }

        /// <summary>Reason, failure code or validation status; empty on success.</summary>
        public string ErrorCode { get; }

        public IapFlowEvent(IapFlowStep step, string productId, string transactionId, string receipt, bool isDelayed,
            string errorCode = null)
        {
            this.Step          = step;
            this.ProductId     = productId;
            this.TransactionId = transactionId;
            this.Receipt       = receipt;
            this.IsDelayed     = isDelayed;
            this.ErrorCode     = Truncate(errorCode);
        }

        /// <summary>snake_case name of <see cref="Step"/>, e.g. <c>store_success</c>.</summary>
        public string StepName => this.Step switch
        {
            IapFlowStep.Click        => "click",
            IapFlowStep.StoreSuccess => "store_success",
            IapFlowStep.StoreFail    => "store_fail",
            IapFlowStep.Cancel       => "cancel",
            IapFlowStep.Pending      => "pending",
            IapFlowStep.ValidateFail => "validate_fail",
            IapFlowStep.Delivered    => "delivered",
            IapFlowStep.Redelivered  => "redelivered",
            IapFlowStep.DeliverFail  => "deliver_fail",
            IapFlowStep.ConfirmFail  => "confirm_fail",
            _                        => this.Step.ToString().ToLowerInvariant()
        };

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= MaxErrorLength ? value : value.Substring(0, MaxErrorLength);
        }
    }

    /// <summary>
    /// Processes every pending order through: validate → (already fulfilled? confirm) → fulfil + persist → confirm.
    /// Orders that cannot finish now are parked and retried by <see cref="RetryParkedOrdersAsync"/>, because the store
    /// only re-delivers an order once per session. Pure C# so it can be unit tested without a store.
    /// </summary>
    public sealed class IapOrderProcessor
    {
        public const int MaxConfirmAttempts = 10;

        private enum ParkReason
        {
            ValidationRetry,
            FulfillerNotReady,
            FulfillmentRetry,
            ConfirmFailed
        }

        private sealed class ParkedOrder
        {
            public IapOrder   Order;
            public ParkReason Reason;
        }

        private readonly IIapReceiptValidationService validationService;
        private readonly IIapPurchaseFulfiller        fulfiller;
        private readonly Action<IapOrder>             confirmOrder;
        private readonly Action<string>               logInfo;
        private readonly Action<string>               logWarning;
        private readonly Action<IapOrder>             onOrderReceived;

        private readonly Dictionary<string, Queue<UniTaskCompletionSource>> purchaseRequests = new();
        private readonly HashSet<string>                                    inFlight         = new();
        private readonly Dictionary<string, ParkedOrder>                    parkedOrders     = new();
        private readonly Dictionary<string, IapOrder>                       awaitingConfirm  = new();
        private readonly Dictionary<string, int>                            confirmAttempts  = new();

        // Per session: is_delayed of each order seen (fixed when first seen; also marks it as seen), last reported pending reason.
        private readonly Dictionary<string, bool>   delayedByOrder  = new();
        private readonly Dictionary<string, string> reportedPending = new();

        /// <param name="onOrderReceived">Called once per transaction per session when the order is first seen, before it is
        /// validated, granted or confirmed (e.g. to hand it to a revenue SDK while it is still unfinished). Must not throw.</param>
        public IapOrderProcessor(
            IIapReceiptValidationService validationService,
            IIapPurchaseFulfiller fulfiller,
            Action<IapOrder> confirmOrder,
            Action<string> logInfo,
            Action<string> logWarning,
            Action<IapOrder> onOrderReceived = null)
        {
            this.validationService = validationService;
            this.fulfiller         = fulfiller;
            this.confirmOrder      = confirmOrder;
            this.logInfo           = logInfo ?? (_ => { });
            this.logWarning        = logWarning ?? (_ => { });
            this.onOrderReceived   = onOrderReceived;
        }

        public bool HasFulfiller     => this.fulfiller != null;
        public bool HasParkedOrders  => this.parkedOrders.Count > 0;
        public int  ParkedOrderCount => this.parkedOrders.Count;

        /// <summary>Raised when an order is parked for a later retry.</summary>
        public event Action OrderParked;

        /// <summary>Raised for each step of an order (analytics). Repeated retries do not report the same step twice.</summary>
        public event Action<IapFlowEvent> Flow;

        #region Purchase requests

        /// <summary>Registers a player-initiated purchase. Completes when the order is granted (or, without a
        /// fulfiller, when it is valid and about to be confirmed).</summary>
        public UniTask RegisterPurchaseRequest(string productId)
        {
            var tcs = new UniTaskCompletionSource();

            if (!this.purchaseRequests.TryGetValue(productId, out var queue))
            {
                queue                            = new Queue<UniTaskCompletionSource>();
                this.purchaseRequests[productId] = queue;
            }

            queue.Enqueue(tcs);
            return tcs.Task;
        }

        public bool HasPurchaseRequest(string productId)
        {
            return productId != null && this.purchaseRequests.TryGetValue(productId, out var queue) && queue.Count > 0;
        }

        /// <summary>Fails the oldest pending request for the product. Returns false if none existed.</summary>
        public bool FailPurchaseRequest(string productId, Exception exception)
        {
            if (!this.TryDequeueRequest(productId, out var tcs)) return false;

            tcs.TrySetException(exception);
            return true;
        }

        private bool TryDequeueRequest(string productId, out UniTaskCompletionSource tcs)
        {
            tcs = null;
            if (productId == null || !this.purchaseRequests.TryGetValue(productId, out var queue) || queue.Count == 0)
                return false;

            tcs = queue.Dequeue();
            return true;
        }

        private bool HasRequestForAny(IapOrder order)
        {
            return order.Products.Any(product => this.HasPurchaseRequest(product.ProductId));
        }

        private bool CompleteRequests(IapOrder order)
        {
            var completedAny = false;
            foreach (var product in order.Products)
            {
                if (this.TryDequeueRequest(product.ProductId, out var tcs))
                {
                    tcs.TrySetResult();
                    completedAny = true;
                }
            }

            return completedAny;
        }

        private void FailRequests(IapOrder order, bool isUserInitiated, Func<Exception> createException)
        {
            if (!isUserInitiated) return;

            foreach (var product in order.Products)
            {
                this.FailPurchaseRequest(product.ProductId, createException());
            }
        }

        #endregion

        #region Order processing

        public async UniTask ProcessAsync(IapOrder order)
        {
            if (order == null) return;

            var transactionId = order.TransactionId;
            if (string.IsNullOrEmpty(transactionId))
            {
                // Without a transaction id the order cannot be de-duplicated or parked: keep the legacy behaviour
                // (complete an active request if the receipt is valid, otherwise leave it unconfirmed).
                this.logWarning("[IAP] Pending order has no transaction id.");
                var isValid = false;
                try
                {
                    isValid = (await this.validationService.ValidateAsync(
                        new IapReceiptValidationRequest(transactionId, order.Receipt, order.Products))).IsValid;
                }
                catch (Exception exception)
                {
                    this.logWarning($"[IAP] Validation failed: {exception.Message}");
                }

                if (!isValid)
                {
                    this.FailRequests(order, true, () => new IAPPurchaseFailedException("Receipt validation failed."));
                    return;
                }

                if (this.CompleteRequests(order)) this.Confirm(order);
                return;
            }

            if (this.awaitingConfirm.ContainsKey(transactionId) || !this.inFlight.Add(transactionId))
            {
                return;
            }

            this.parkedOrders.Remove(transactionId);

            try
            {
                await this.ProcessInternalAsync(order);
            }
            finally
            {
                this.inFlight.Remove(transactionId);
            }
        }

        private async UniTask ProcessInternalAsync(IapOrder order)
        {
            var transactionId = order.TransactionId;

            // Captured before awaiting: a request registered while an unattended order is being processed belongs to
            // a new purchase and must not be completed or failed by this order.
            var isUserInitiated = this.HasRequestForAny(order);

            if (!this.delayedByOrder.ContainsKey(transactionId))
            {
                this.delayedByOrder[transactionId] = !isUserInitiated;
                this.Report(IapFlowStep.StoreSuccess, order);
                this.NotifyOrderReceived(order);
            }

            var request = new IapReceiptValidationRequest(transactionId, order.Receipt, order.Products);

            IapReceiptValidationResult validation;
            try
            {
                validation = await this.validationService.ValidateAsync(request);
            }
            catch (Exception exception)
            {
                validation = IapReceiptValidationResult.RetryableFailure("exception", exception.Message);
            }

            switch (validation.Status)
            {
                case IapReceiptValidationStatus.Invalid:
                    this.logWarning($"[IAP] Order {transactionId} rejected by '{validation.ProviderId}': {validation.Error}");
                    this.Report(IapFlowStep.ValidateFail, order, $"{validation.ProviderId}: {validation.Error}");
                    this.FailRequests(order, isUserInitiated, () => new IAPPurchaseFailedException(
                        $"Receipt validation {validation.Status} via '{validation.ProviderId}': {validation.Error}"));
                    return;

                case IapReceiptValidationStatus.RetryableFailure:
                    this.logWarning($"[IAP] Order {transactionId} validation will be retried: {validation.Error}");
                    this.FailRequests(order, isUserInitiated, () => new IAPPurchasePendingFulfillmentException(
                        $"Receipt validation pending: {validation.Error}"));
                    this.Park(order, ParkReason.ValidationRetry, $"{validation.ProviderId}: {validation.Error}");
                    return;
            }

            if (this.fulfiller == null)
            {
                // Legacy mode: the caller of PurchaseProduct grants the content.
                if (this.CompleteRequests(order))
                {
                    this.Confirm(order);
                }
                else
                {
                    this.logWarning($"[IAP] No pending purchase request and no fulfiller for order {transactionId}; left unconfirmed.");
                }

                return;
            }

            if (this.fulfiller.IsFulfilled(transactionId))
            {
                this.logInfo($"[IAP] Order {transactionId} already fulfilled, confirming.");
                this.Report(IapFlowStep.Redelivered, order);
                this.Confirm(order);
                return;
            }

            if (!this.fulfiller.IsReady)
            {
                this.logInfo($"[IAP] Fulfiller not ready, holding order {transactionId}.");
                this.Park(order, ParkReason.FulfillerNotReady);
                return;
            }

            IapFulfillmentResult result;
            try
            {
                result = await this.fulfiller.FulfillAsync(
                    new IapPurchaseInfo(transactionId, order.Products, isUserInitiated));
            }
            catch (Exception exception)
            {
                result = IapFulfillmentResult.RetryLater(exception.Message);
            }

            switch (result.Status)
            {
                case IapFulfillmentStatus.Granted:
                case IapFulfillmentStatus.AlreadyGranted:
                    if (isUserInitiated) this.CompleteRequests(order);
                    this.Report(result.Status == IapFulfillmentStatus.Granted ? IapFlowStep.Delivered : IapFlowStep.Redelivered, order);
                    this.Confirm(order);
                    break;

                case IapFulfillmentStatus.RetryLater:
                    this.logWarning($"[IAP] Fulfilment of {transactionId} will be retried: {result.Error}");
                    this.FailRequests(order, isUserInitiated, () => new IAPPurchasePendingFulfillmentException(
                        $"Fulfilment pending: {result.Error}"));
                    this.Park(order, ParkReason.FulfillmentRetry, result.Error);
                    break;

                default:
                    this.logWarning($"[IAP] Fulfilment of {transactionId} rejected: {result.Error}; left unconfirmed.");
                    this.Report(IapFlowStep.DeliverFail, order, result.Error);
                    this.FailRequests(order, isUserInitiated, () => new IAPPurchaseFailedException($"Fulfilment rejected: {result.Error}"));
                    break;
            }
        }

        private void Park(IapOrder order, ParkReason reason, string detail = null, bool report = true)
        {
            this.parkedOrders[order.TransactionId] = new ParkedOrder { Order = order, Reason = reason };

            // Report a pending order once per reason, not on every retry.
            var reasonName = ToReasonName(reason);
            if (report && (!this.reportedPending.TryGetValue(order.TransactionId, out var reported) || reported != reasonName))
            {
                this.reportedPending[order.TransactionId] = reasonName;
                this.Report(IapFlowStep.Pending, order, string.IsNullOrEmpty(detail) ? reasonName : $"{reasonName}: {detail}");
            }

            this.OrderParked?.Invoke();
        }

        private static string ToReasonName(ParkReason reason)
        {
            return reason switch
            {
                ParkReason.ValidationRetry   => "validation_retry",
                ParkReason.FulfillerNotReady => "fulfiller_not_ready",
                ParkReason.FulfillmentRetry  => "fulfillment_retry",
                ParkReason.ConfirmFailed     => "confirm_failed",
                _                            => reason.ToString()
            };
        }

        #endregion

        #region Order received

        private void NotifyOrderReceived(IapOrder order)
        {
            if (this.onOrderReceived == null) return;

            try
            {
                this.onOrderReceived(order);
            }
            catch (Exception exception)
            {
                this.logWarning($"[IAP] Order received listener failed for {order.TransactionId}: {exception.Message}");
            }
        }

        #endregion

        #region Flow reporting

        private void Report(IapFlowStep step, IapOrder order, string errorCode = null)
        {
            if (this.Flow == null) return;

            var isDelayed = this.delayedByOrder.TryGetValue(order.TransactionId, out var delayed) && delayed;

            foreach (var product in order.Products)
            {
                try
                {
                    this.Flow.Invoke(new IapFlowEvent(step, product.ProductId, order.TransactionId, order.Receipt, isDelayed, errorCode));
                }
                catch (Exception exception)
                {
                    this.logWarning($"[IAP] Flow listener failed: {exception.Message}");
                }
            }
        }

        #endregion

        #region Retry

        /// <summary>Retries every parked order once. Call on a backoff timer, on focus and when the fulfiller becomes ready.</summary>
        public async UniTask RetryParkedOrdersAsync()
        {
            foreach (var parked in this.parkedOrders.Values.ToList())
            {
                // Retries may overlap (timer, focus, fulfiller ready): skip entries already handled by another pass.
                if (!this.parkedOrders.TryGetValue(parked.Order.TransactionId, out var current) || !ReferenceEquals(current, parked))
                {
                    continue;
                }

                if (parked.Reason == ParkReason.ConfirmFailed)
                {
                    this.parkedOrders.Remove(parked.Order.TransactionId);
                    this.Confirm(parked.Order);
                }
                else
                {
                    await this.ProcessAsync(parked.Order);
                }
            }
        }

        #endregion

        #region Confirmation

        private void Confirm(IapOrder order)
        {
            if (!string.IsNullOrEmpty(order.TransactionId))
            {
                this.awaitingConfirm[order.TransactionId] = order;
            }

            this.confirmOrder(order);
        }

        /// <summary>Reports the store's confirmation result for a transaction.</summary>
        public void OnConfirmResult(string transactionId, bool success)
        {
            if (string.IsNullOrEmpty(transactionId) || !this.awaitingConfirm.Remove(transactionId, out var order))
            {
                return;
            }

            if (success)
            {
                this.confirmAttempts.Remove(transactionId);
                return;
            }

            var attempts = this.confirmAttempts.GetValueOrDefault(transactionId) + 1;
            this.confirmAttempts[transactionId] = attempts;

            if (attempts == 1)
            {
                this.Report(IapFlowStep.ConfirmFail, order);
            }

            if (attempts >= MaxConfirmAttempts)
            {
                // Content is already granted (ledger), so the next session's re-delivery will only confirm.
                this.logWarning($"[IAP] Giving up confirming {transactionId} this session after {attempts} attempts.");
                return;
            }

            this.Park(order, ParkReason.ConfirmFailed, report: false);
        }

        #endregion
    }
}
