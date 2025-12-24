namespace Transactions.Exceptions
{
    using System;
    public class IAPPurchaseFailedException : Exception
    {
        public IAPPurchaseFailedException(string s) : base(s)
        {
        }
    }
}
