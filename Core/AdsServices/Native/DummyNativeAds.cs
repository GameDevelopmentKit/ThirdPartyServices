namespace Core.AdsServices.Native
{
    using System.Collections.Generic;

    public class DummyNativeAds : INativeAdsService
    {
        private List<NativeAdInstanceWrapper> nativeAds = new();
        public  void                          DrawNativeAds(NativeAdsView nativeAdsView) { }

        public List<NativeAdInstanceWrapper> GetNativeAds(string adsId = "")
        {
            this.nativeAds.Clear();

            for (var i = 0; i < 10; i++)
            {
                this.nativeAds.Add(new NativeAdInstanceWrapper());
            }

            return this.nativeAds;
        }

        public void RemoveNativeAd(NativeAdInstanceWrapper nativeAd)
        {
            if (this.nativeAds.Contains(nativeAd))
                this.nativeAds.Remove(nativeAd);
        }
    }
}