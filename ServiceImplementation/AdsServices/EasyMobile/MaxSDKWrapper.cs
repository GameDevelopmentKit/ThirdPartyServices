#if EM_APPLOVIN
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class MaxSDKWrapper : IMRECAdService, IInitializable
    {
        #region inject

        private readonly Dictionary <AdViewPosition, string> positionToMRECAdUnitId;
        private readonly ILogService                         logService;

        #endregion

        public MaxSDKWrapper(Dictionary<AdViewPosition, string> positionToMRECAdUnitId, ILogService logService)
        {
            this.positionToMRECAdUnitId = positionToMRECAdUnitId;
            this.logService             = logService;
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
        }

        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)       { this.OnAdLoadedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error) { this.OnAdLoadFailedEvent?.Invoke(adUnitId, new ErrorInfo((int)error.Code, error.Message)); }
        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)      { this.OnAdClickedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)  { this.OnAdRevenuePaidEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)     { this.OnAdExpandedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }
        private void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)    { this.OnAdCollapsedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }

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
                AdViewPosition.TopLeft => MaxSdkBase.AdViewPosition.TopLeft,
                AdViewPosition.TopCenter => MaxSdkBase.AdViewPosition.TopCenter,
                AdViewPosition.TopRight => MaxSdkBase.AdViewPosition.TopRight,
                AdViewPosition.CenterLeft => MaxSdkBase.AdViewPosition.CenterLeft,
                AdViewPosition.Centered => MaxSdkBase.AdViewPosition.Centered,
                AdViewPosition.CenterRight => MaxSdkBase.AdViewPosition.CenterRight,
                AdViewPosition.BottomLeft => MaxSdkBase.AdViewPosition.BottomLeft,
                AdViewPosition.BottomCenter => MaxSdkBase.AdViewPosition.BottomCenter,
                AdViewPosition.BottomRight => MaxSdkBase.AdViewPosition.BottomRight,
                _ => MaxSdkBase.AdViewPosition.BottomCenter
            };
    }
}
#endif