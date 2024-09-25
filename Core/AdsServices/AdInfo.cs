namespace Core.AdsServices
{
    public class AdInfo
    {
        public string AdPlatform     { get; set; }
        public string AdUnitId       { get; set; }
        public string AdFormat       { get; set; }
        public string AdSource       { get; set; }
        public string AdSourceUnitId { get; set; }
        public double Value          { get; set; }
        public string Currency       { get; set; }

        public AdInfo(string adPlatform, string adUnitId, string adFormat, string adSource = null, string adSourceUnitId = null, double value = 0, string currency = "USD")
        {
            this.AdPlatform     = adPlatform;
            this.AdUnitId       = adUnitId;
            this.AdSource       = adSource;
            this.AdSourceUnitId = adSourceUnitId;
            this.AdFormat       = adFormat;
            this.Value          = value;
            this.Currency       = currency;
        }
    }
}