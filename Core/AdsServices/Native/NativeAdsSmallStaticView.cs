namespace Core.AdsServices.Native
{
    using System.Runtime.CompilerServices;
    using GoogleMobileAds.Api;
    using TMPro;

    public class NativeAdsSmallStaticView : NativeAdsView
    {

        public TMP_Text bodyText;
        
        public override void BindData(NativeAd nativeAd)
        {
            base.BindData(nativeAd);

            if (nativeAd.GetBodyText() != null)
            {
                this.logService.Log($"native body text: {nativeAd.GetBodyText()}");
                
                this.bodyText.text = nativeAd.GetBodyText();
                
                if (!nativeAd.RegisterBodyTextGameObject(this.bodyText.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register body text for native ad: {this.name}");
                }
            }
        }
    }
}