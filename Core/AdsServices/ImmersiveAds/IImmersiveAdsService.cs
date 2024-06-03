namespace Core.AdsServices.ImmersiveAds
{
    using Core.AnalyticServices;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GoogleMobileAds.Api;
    using PubScale.SdkOne.NativeAds;
    using UnityEngine;
    using Zenject;

    public interface IImmersiveAdsService
    {
        void InitAdHolder(NativeAdHolder nativeAdHolder, string placement, bool worldSpace = false);
    }

    public class ImmersiveAdsService : IImmersiveAdsService
    {
#region Inject

        private readonly IScreenManager    screenManager;
        private readonly SignalBus         signalBus;
        private readonly IAnalyticServices analyticServices;

#endregion

        private Canvas cacheCanvas;

        public ImmersiveAdsService
        (
            IScreenManager    screenManager,
            SignalBus         signalBus,
            IAnalyticServices analyticServices
        )
        {
            this.screenManager    = screenManager;
            this.signalBus        = signalBus;
            this.analyticServices = analyticServices;
        }

        public void InitAdHolder(NativeAdHolder nativeAdHolder, string placement, bool worldSpace = false)
        {
            if (!worldSpace)
            {
                var canvas = this.cacheCanvas ?? this.screenManager.RootUICanvas.GetComponentInChildren<Canvas>();
                nativeAdHolder.canvas = canvas;
            }

            nativeAdHolder.adTag          =  placement;
            nativeAdHolder.Event_OnAdPaid += this.OnAdPaid;
        }

        private void OnAdPaid(AdValue obj)
        {
            
        }
    }
}