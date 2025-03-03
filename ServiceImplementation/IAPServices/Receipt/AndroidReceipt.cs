namespace ServiceImplementation.IAPServices.Receipt
{
    public class AndroidReceipt : IIAPReceipt
    {
        public int  Quantity  { get; }
        public bool IsSandbox { get; }
    }
}