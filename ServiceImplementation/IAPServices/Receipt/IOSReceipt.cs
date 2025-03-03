namespace ServiceImplementation.IAPServices.Receipt
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UnityEngine.Scripting;

    [Preserve]
    public class IOSReceipt : IIAPReceipt
    {
        [Preserve, JsonProperty("receipt")] public Receipt Receipt { get; set; }

        [Preserve, JsonProperty("status")] public int Status { get; set; }

        [Preserve, JsonProperty("environment")]
        public string Environment { get; set; }

        public int  Quantity  => 1;
        public bool IsSandbox => this.Environment.Equals("Sandbox");
    }

    [Preserve]
    public class Receipt
    {
        [Preserve, JsonProperty("receipt_type")]
        public string ReceiptType { get; set; }

        [Preserve, JsonProperty("adam_id")] public long AdamId { get; set; }

        [Preserve, JsonProperty("app_item_id")]
        public long AppItemId { get; set; }

        [Preserve, JsonProperty("bundle_id")] public string BundleId { get; set; }

        [Preserve, JsonProperty("application_version")]
        public string ApplicationVersion { get; set; }

        [Preserve, JsonProperty("download_id")]
        public long DownloadId { get; set; }

        [Preserve, JsonProperty("version_external_identifier")]
        public long VersionExternalIdentifier { get; set; }

        [Preserve, JsonProperty("receipt_creation_date")]
        public string ReceiptCreationDate { get; set; }

        [Preserve, JsonProperty("receipt_creation_date_ms")]
        public string ReceiptCreationDateMs { get; set; }

        [Preserve, JsonProperty("receipt_creation_date_pst")]
        public string ReceiptCreationDatePst { get; set; }

        [Preserve, JsonProperty("request_date")]
        public string RequestDate { get; set; }

        [Preserve, JsonProperty("request_date_ms")]
        public string RequestDateMs { get; set; }

        [Preserve, JsonProperty("request_date_pst")]
        public string RequestDatePst { get; set; }

        [Preserve, JsonProperty("original_purchase_date")]
        public string OriginalPurchaseDate { get; set; }

        [Preserve, JsonProperty("original_purchase_date_ms")]
        public string OriginalPurchaseDateMs { get; set; }

        [Preserve, JsonProperty("original_purchase_date_pst")]
        public string OriginalPurchaseDatePst { get; set; }

        [Preserve, JsonProperty("original_application_version")]
        public string OriginalApplicationVersion { get; set; }

        [Preserve, JsonProperty("in_app")] public List<InAppPurchase> InAppPurchases { get; set; }
    }

    [Preserve]
    public class InAppPurchase
    {
        [Preserve, JsonProperty("quantity")] public string Quantity { get; set; }

        [Preserve, JsonProperty("product_id")] public string ProductId { get; set; }

        [Preserve, JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [Preserve, JsonProperty("original_transaction_id")]
        public string OriginalTransactionId { get; set; }

        [Preserve, JsonProperty("purchase_date")]
        public string PurchaseDate { get; set; }

        [Preserve, JsonProperty("purchase_date_ms")]
        public string PurchaseDateMs { get; set; }

        [Preserve, JsonProperty("purchase_date_pst")]
        public string PurchaseDatePst { get; set; }

        [Preserve, JsonProperty("original_purchase_date")]
        public string OriginalPurchaseDate { get; set; }

        [Preserve, JsonProperty("original_purchase_date_ms")]
        public string OriginalPurchaseDateMs { get; set; }

        [Preserve, JsonProperty("original_purchase_date_pst")]
        public string OriginalPurchaseDatePst { get; set; }

        [Preserve, JsonProperty("is_trial_period")]
        public string IsTrialPeriod { get; set; }
    }
}