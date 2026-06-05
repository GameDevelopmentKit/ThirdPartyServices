namespace ServiceImplementation.AdsServices.PubScale
{
    using Core.AdsServices.ImmersiveAds;

    public class DummyPubScaleWrapper : IImmersiveAdsService
    {
        public void InitNativeAdHolder(ImmersiveAdsView immersiveAdsView, string placement, bool worldSpace = false) { immersiveAdsView.gameObject.SetActive(false); }
    }
}