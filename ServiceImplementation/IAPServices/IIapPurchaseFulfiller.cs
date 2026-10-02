namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Game-side entitlement granting for IAP orders. Optional: bind an implementation to let the IAP handler
    /// grant and confirm every valid order (including orders delivered without an active purchase request, e.g.
    /// after an app restart, a deferred payment or a failed confirmation). Without a binding the handler keeps the
    /// legacy behaviour (only orders with an active purchase request are completed).
    /// </summary>
    public interface IIapPurchaseFulfiller
    {
        /// <summary>True once the game can grant rewards (user data loaded).</summary>
        bool IsReady { get; }

        /// <summary>Raised when <see cref="IsReady"/> may have changed, so held orders can be processed.</summary>
        event Action ReadyChanged;

        /// <summary>True if the transaction was already granted and persisted. Must be durable across sessions.</summary>
        bool IsFulfilled(string transactionId);

        /// <summary>
        /// Grant the purchased content and persist it together with the transaction id BEFORE returning
        /// <see cref="IapFulfillmentStatus.Granted"/>. The order is confirmed (consumed/finished) only afterwards.
        /// </summary>
        UniTask<IapFulfillmentResult> FulfillAsync(IapPurchaseInfo purchaseInfo);
    }

    public sealed class IapPurchaseInfo
    {
        public string                           TransactionId   { get; }
        public IReadOnlyList<IapReceiptProduct> Products        { get; }
        public bool                             IsUserInitiated { get; }

        public IapPurchaseInfo(string transactionId, IReadOnlyList<IapReceiptProduct> products, bool isUserInitiated)
        {
            this.TransactionId   = transactionId;
            this.Products        = products;
            this.IsUserInitiated = isUserInitiated;
        }
    }

    public enum IapFulfillmentStatus
    {
        /// <summary>Content granted and persisted. The order will be confirmed.</summary>
        Granted,

        /// <summary>Content was granted earlier. The order will be confirmed.</summary>
        AlreadyGranted,

        /// <summary>Cannot grant now (e.g. feature busy). The order is kept unconfirmed and retried.</summary>
        RetryLater,

        /// <summary>Order cannot be granted by this game (e.g. unknown product). The order is not confirmed.</summary>
        Rejected
    }

    public sealed class IapFulfillmentResult
    {
        public IapFulfillmentStatus Status { get; }
        public string               Error  { get; }

        private IapFulfillmentResult(IapFulfillmentStatus status, string error)
        {
            this.Status = status;
            this.Error  = error;
        }

        public static IapFulfillmentResult Granted()               => new(IapFulfillmentStatus.Granted, null);
        public static IapFulfillmentResult AlreadyGranted()        => new(IapFulfillmentStatus.AlreadyGranted, null);
        public static IapFulfillmentResult RetryLater(string error) => new(IapFulfillmentStatus.RetryLater, error);
        public static IapFulfillmentResult Rejected(string error)   => new(IapFulfillmentStatus.Rejected, error);
    }
}
