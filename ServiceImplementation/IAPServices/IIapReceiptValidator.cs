namespace ServiceImplementation.IAPServices
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;

    public interface IIapReceiptValidator
    {
        string ProviderId { get; }
        int    Priority   { get; }
        bool   IsAvailable { get; }

        UniTask<IapReceiptValidationResult> ValidateAsync(IapReceiptValidationRequest request);
    }

    /// <summary>
    /// A validator that never decides whether an order is delivered. It runs in the background next to the gate validators
    /// (e.g. a third-party revenue/fraud service whose answer can be late or missing) and its result is only reported.
    /// </summary>
    public interface IIapReceiptAuditValidator : IIapReceiptValidator
    {
    }

    public interface IIapReceiptValidationService
    {
        /// <summary>
        /// Decides whether an order can be delivered. Gate validators (every available validator that is not an
        /// <see cref="IIapReceiptAuditValidator"/>) run by priority; a <see cref="IapReceiptValidationStatus.RetryableFailure"/>
        /// falls back to the next one.
        /// </summary>
        UniTask<IapReceiptValidationResult> ValidateAsync(IapReceiptValidationRequest request);

        /// <summary>Runs every available <see cref="IIapReceiptAuditValidator"/>. Completes when all of them finished; never throws.</summary>
        UniTask<IReadOnlyList<IapReceiptAuditResult>> AuditAsync(IapReceiptValidationRequest request);
    }

    /// <summary>Result of one audit validator for one order.</summary>
    public sealed class IapReceiptAuditResult
    {
        public IapReceiptValidationResult Result    { get; }
        public long                       LatencyMs { get; }

        public IapReceiptAuditResult(IapReceiptValidationResult result, long latencyMs)
        {
            this.Result    = result;
            this.LatencyMs = latencyMs;
        }
    }

    public enum IapReceiptValidationStatus
    {
        Valid,
        Invalid,
        RetryableFailure
    }

    public sealed class IapReceiptValidationResult
    {
        public IapReceiptValidationStatus Status     { get; }
        public string                     ProviderId { get; }
        public string                     Error      { get; }

        public bool IsValid => this.Status == IapReceiptValidationStatus.Valid;

        private IapReceiptValidationResult(IapReceiptValidationStatus status, string providerId, string error)
        {
            this.Status     = status;
            this.ProviderId = providerId;
            this.Error      = error;
        }

        public static IapReceiptValidationResult Valid(string providerId)
        {
            return new IapReceiptValidationResult(IapReceiptValidationStatus.Valid, providerId, null);
        }

        public static IapReceiptValidationResult Invalid(string providerId, string error)
        {
            return new IapReceiptValidationResult(IapReceiptValidationStatus.Invalid, providerId, error);
        }

        public static IapReceiptValidationResult RetryableFailure(string providerId, string error)
        {
            return new IapReceiptValidationResult(IapReceiptValidationStatus.RetryableFailure, providerId, error);
        }
    }

    public sealed class IapReceiptValidationRequest
    {
        public string                           TransactionId { get; }
        public string                           Receipt       { get; }
        public IReadOnlyList<IapReceiptProduct> Products      { get; }

        public IapReceiptValidationRequest(string transactionId, string receipt, IReadOnlyList<IapReceiptProduct> products)
        {
            this.TransactionId = transactionId;
            this.Receipt       = receipt;
            this.Products      = products;
        }
    }

    public sealed class IapReceiptProduct
    {
        public string      ProductId   { get; }
        public ProductType ProductType { get; }
        public int         Quantity    { get; }

        public IapReceiptProduct(string productId, ProductType productType, int quantity)
        {
            this.ProductId   = productId;
            this.ProductType = productType;
            this.Quantity    = quantity;
        }
    }
}
