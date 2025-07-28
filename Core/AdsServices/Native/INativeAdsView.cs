namespace Core.AdsServices.Native
{
    using System.Collections.Generic;
    using UnityEngine;

    public interface INativeAdsView
    {
        GameObject Instance { get; }
        void       ShowNativeAds(NativeAdInstanceWrapper modelNativeAd, List<GameObject> modelRegisted);
    }

   
}