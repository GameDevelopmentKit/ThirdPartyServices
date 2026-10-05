namespace Transactions.Exceptions
{
    using System;
    public class IAPPurchaseFailedException : Exception
    {
        public IAPPurchaseFailedException(string s) : base(s)
        {
        }
    }

    /// <summary>The store accepted the purchase but payment is still pending (e.g. Google "slow" payment methods).
    /// Content is granted automatically when the payment completes.</summary>
    public class IAPPurchaseDeferredException : IAPPurchaseFailedException
    {
        public IAPPurchaseDeferredException(string s) : base(s)
        {
        }
    }

    /// <summary>The purchase was paid but could not be verified or granted yet. It is kept and retried, and
    /// content is granted automatically once it succeeds.</summary>
    public class IAPPurchasePendingFulfillmentException : IAPPurchaseFailedException
    {
        public IAPPurchasePendingFulfillmentException(string s) : base(s)
        {
        }
    }

    /// <summary>The store reports an earlier purchase of this product that is not finished yet. The handler
    /// re-fetches and completes it; the player can buy again afterwards.</summary>
    public class IAPDuplicatePurchaseException : IAPPurchaseFailedException
    {
        public IAPDuplicatePurchaseException(string s) : base(s)
        {
        }
    }
}
