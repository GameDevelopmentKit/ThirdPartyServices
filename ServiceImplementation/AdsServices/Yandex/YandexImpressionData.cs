namespace ServiceImplementation.AdsServices.Yandex
{
    public class YandexNetworkImpressionData
    {
        public string name       { get; set; }
        public string adapter    { get; set; }
        public string ad_unit_id { get; set; }
    }

    public class YandexImpressionData
    {
        public string                      currency   { get; set; }
        public double                      revenueUSD { get; set; }
        public string                      precision  { get; set; }
        public double                      revenue    { get; set; }
        public string                      requestId  { get; set; }
        public string                      blockId    { get; set; }
        public string                      adType     { get; set; }
        public string                      ad_unit_id { get; set; }
        public YandexNetworkImpressionData network    { get; set; }
    }
}