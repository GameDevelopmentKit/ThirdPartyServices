namespace ServiceImplementation.IAPServices.Receipt
{
    using Newtonsoft.Json;
    using UnityEngine.Scripting;

    public class AndroidReceipt : IIAPReceipt
    {
        [Preserve, JsonProperty("kind")] public string Kind { get; set; }

        [Preserve, JsonProperty("purchaseTimeMillis")]
        public long PurchaseTimeMillis { get; set; }

        [Preserve, JsonProperty("purchaseState")]
        public int PurchaseState { get; set; }

        [Preserve, JsonProperty("consumptionState")]
        public int ConsumptionState { get; set; }

        [Preserve, JsonProperty("developerPayload")]
        public DeveloperPayloadData DeveloperPayload { get; set; }

        [Preserve, JsonProperty("orderId")] public string OrderId { get; set; }

        [Preserve, JsonProperty("purchaseType")]
        public int PurchaseType { get; set; } = 1; // Default to 1 if missing, 0 is sandbox

        [Preserve, JsonProperty("acknowledgementState")]
        public int AcknowledgementState { get; set; }

        [Preserve, JsonProperty("purchaseToken")]
        public string PurchaseToken { get; set; }

        [Preserve, JsonProperty("regionCode")] public string RegionCode { get; set; }

        public int  Quantity  => this.DeveloperPayload.Quantity;
        public bool IsSandbox => this.PurchaseType == 0;
    }

    public class DeveloperPayloadData
    {
        [Preserve, JsonProperty("userId")] public string UserId { get; set; }

        [Preserve, JsonProperty("quantity")] public int Quantity { get; set; } = 1; // Default to 1 if missing
    }
}