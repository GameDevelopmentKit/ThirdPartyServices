namespace ServiceImplementation.IAPServices.Receipt
{
    public interface IIAPReceipt
    {
        public int  Quantity  { get; }
        public bool IsSandbox { get; }
    }
}