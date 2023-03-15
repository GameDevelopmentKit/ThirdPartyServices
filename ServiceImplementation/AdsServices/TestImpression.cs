namespace ServiceImplementation.AdsServices
{
    using global::EasyMobile;
    using GoogleMobileAds.Api;

    public class TestImpression
    {
        public void Test()
        {
            Advertising.IronSourceClient
           var rewardedAd = new RewardedAd("ad unit ID");
   
            rewardedAd.OnPaidEvent += this.HandleAdPaidEvent;
   
            AdRequest adRequest = new AdRequest.Builder().Build();
            rewardedAd.LoadAd(adRequest);
        }
    }
}