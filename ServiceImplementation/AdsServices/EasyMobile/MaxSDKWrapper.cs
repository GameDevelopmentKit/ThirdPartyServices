#if EM_APPLOVIN
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class MaxSDKWrapper : IMRECAdService, IInitializable
    {
        #region inject

        private readonly Dictionary <AdViewPosition, string> positionToMRECAdUnitId;
        private readonly ILogService                         logService;
        private readonly SignalBus                           signalBus;

        #endregion

        public MaxSDKWrapper(Dictionary<AdViewPosition, string> positionToMRECAdUnitId, ILogService logService, SignalBus signalBus)
        {
            this.positionToMRECAdUnitId = positionToMRECAdUnitId;
            this.logService             = logService;
            this.signalBus              = signalBus;
        }

        public async void Initialize()
        {
            // MRECs are sized to 300x250 on phones and tablets
            await UniTask.WaitUntil(() => MaxSdk.IsInitialized());
            foreach (var (position, adUnitId) in this.positionToMRECAdUnitId)
            {
                this.logService.Log($"Check max init {MaxSdk.IsInitialized()}");
                MaxSdk.CreateMRec(adUnitId, this.ConvertAdViewPosition(position));
            }

            MaxSdkCallbacks.MRec.OnAdLoadedEvent      += this.OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent  += this.OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent     += this.OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += this.OnMRecAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent    += this.OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent   += this.OnMRecAdCollapsedEvent;
            
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent     += this.OnInterstitialAdLoadedHandler;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent  += this.InterstitialAdDisplayedSignal;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += this.OnInterstitialAdLoadFailedHandler;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent    += this.OnInterstitialAdClickedHandler;
            
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent     += this.OnRewardedAdLoadedHandler;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += this.OnRewardedAdLoadFailedHandler;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent    += this.OnRewardedAdClickedHandler;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent  += this.OnRewardedAdDisplayedHandler;
            
            MaxSdkCallbacks.Banner.OnAdLoadedEvent     += this.OnBannerAdLoadedHandler;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += this.OnBannerAdLoadFailedHandler;
            MaxSdkCallbacks.Banner.OnAdClickedEvent    += this.OnBannerAdClickedHandler;
        }

        private void OnBannerAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdClickedSignal(arg2.Placement));
        }
        private void OnBannerAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            this.signalBus.Fire(new BannerAdLoadFailedSignal("empty", arg2.Message));
        }
        private void OnBannerAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdLoadedSignal(arg2.Placement));
        }
        private void OnRewardedAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new RewardedAdLoadClickedSignal(arg2.Placement));
        }

        private void OnRewardedAdDisplayedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new RewardedAdDisplayedSignal(arg2.Placement));
        }
        private void OnRewardedAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("empty", arg2.Message));
        }
        private void OnRewardedAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new RewardedAdLoadedSignal(arg2.Placement));
        }
        private void OnInterstitialAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new InterstitialAdClickedSignal(arg2.Placement));
        }
        private void OnInterstitialAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("empty", arg2.Message));
        }

        private void InterstitialAdDisplayedSignal(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(arg2.Placement));
        }
        private void OnInterstitialAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new InterstitialAdDownloadedSignal(arg2.Placement));
        }

        private void OnMRecAdLoadedEvent(string      adUnitId, MaxSdkBase.AdInfo    adInfo) { this.OnAdLoadedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdLoadFailedEvent(string  adUnitId, MaxSdkBase.ErrorInfo error)  { this.OnAdLoadFailedEvent?.Invoke(adUnitId, new ErrorInfo((int)error.Code, error.Message)); }
        private void OnMRecAdClickedEvent(string     adUnitId, MaxSdkBase.AdInfo    adInfo) { this.OnAdClickedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo    adInfo) { this.OnAdRevenuePaidEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdExpandedEvent(string    adUnitId, MaxSdkBase.AdInfo    adInfo) { this.OnAdExpandedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdCollapsedEvent(string   adUnitId, MaxSdkBase.AdInfo    adInfo) { this.OnAdCollapsedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }

        public void ShowMREC(AdViewPosition adViewPosition)
        {
            MaxSdk.ShowMRec(this.positionToMRECAdUnitId[adViewPosition]);
        }
        public void HideMREC(AdViewPosition adViewPosition)
        {
            MaxSdk.HideMRec(this.positionToMRECAdUnitId[adViewPosition]);
        }
        public void StopMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            MaxSdk.StopMRecAutoRefresh(this.positionToMRECAdUnitId[adViewPosition]);
        }
        public void StartMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            MaxSdk.StartMRecAutoRefresh(this.positionToMRECAdUnitId[adViewPosition]);
        }
        public void LoadMREC(AdViewPosition adViewPosition)
        {
            MaxSdk.LoadMRec(this.positionToMRECAdUnitId[adViewPosition]);
        }
        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;

        private AdInfo ConvertAdInfo(MaxSdkBase.AdInfo maxAdInfo)
        {
            return new AdInfo()
            {
                AdUnitIdentifier   = maxAdInfo.AdUnitIdentifier,
                AdFormat           = maxAdInfo.AdFormat,
                NetworkName        = maxAdInfo.NetworkName,
                NetworkPlacement   = maxAdInfo.NetworkPlacement,
                Placement          = maxAdInfo.NetworkPlacement,
                CreativeIdentifier = maxAdInfo.CreativeIdentifier,
                Revenue            = maxAdInfo.Revenue,
                RevenuePrecision   = maxAdInfo.RevenuePrecision,
                DspName            = maxAdInfo.DspName
            };
        }

        private MaxSdkBase.AdViewPosition ConvertAdViewPosition(AdViewPosition adViewPosition) =>
            adViewPosition switch
            {
                AdViewPosition.TopLeft      => MaxSdkBase.AdViewPosition.TopLeft,
                AdViewPosition.TopCenter    => MaxSdkBase.AdViewPosition.TopCenter,
                AdViewPosition.TopRight     => MaxSdkBase.AdViewPosition.TopRight,
                AdViewPosition.CenterLeft   => MaxSdkBase.AdViewPosition.CenterLeft,
                AdViewPosition.Centered     => MaxSdkBase.AdViewPosition.Centered,
                AdViewPosition.CenterRight  => MaxSdkBase.AdViewPosition.CenterRight,
                AdViewPosition.BottomLeft   => MaxSdkBase.AdViewPosition.BottomLeft,
                AdViewPosition.BottomCenter => MaxSdkBase.AdViewPosition.BottomCenter,
                AdViewPosition.BottomRight  => MaxSdkBase.AdViewPosition.BottomRight,
                _                           => MaxSdkBase.AdViewPosition.BottomCenter
            };
    }
}
#endif