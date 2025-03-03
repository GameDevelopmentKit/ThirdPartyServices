namespace ServiceImplementation.IAPServices.Receipt
{
    using System;
    using Newtonsoft.Json;

    public class ReceiptHelper
    {
        public static IIAPReceipt ParseReceipt(string receipt)
        {
#if UNITY_ANDROID
            return JsonConvert.DeserializeObject<AndroidReceipt>(receipt);
#elif UNITY_IOS
            return JsonConvert.DeserializeObject<IOSReceipt>(receipt);
#endif
            throw new Exception("Unknown platform for IAP receipt parsing");
        }
    }
}