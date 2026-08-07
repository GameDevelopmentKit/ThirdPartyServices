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

    public interface IIapReceiptValidationService
    {
        UniTask<IapReceiptValidationResult> ValidateAsync(IapReceiptValidationRequest request);
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
