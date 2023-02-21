namespace Core.AnalyticServices.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public class UnityReceipt
    {
        public string Store;
        public string TransactionID;
        public string Payload;
    }

    /// <summary>
    /// 
    /// </summary>
    public class AndroidReceiptPayload
    {
        public string json;
        public string signature;
    }

    /// <summary>
    /// 
    /// </summary>
    public class AndroidReceiptPayloadJson
    {
        public string packageName;
        public string purchaseState;
        public string purchaseTime;
        public string purchaseToken;
        public string productId;
    }
}