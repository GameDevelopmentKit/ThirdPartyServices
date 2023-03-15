#if EM_IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using Core.AdsServices;

    public class IronSourceWrapper : IMRECAdService
    {
        public void                            ShowMREC(AdViewPosition adViewPosition)             { throw new NotImplementedException(); }
        public void                            HideMREC(AdViewPosition adViewPosition)             { throw new NotImplementedException(); }
        public void                            StopMRECAutoRefresh(AdViewPosition adViewPosition)  { throw new NotImplementedException(); }
        public void                            StartMRECAutoRefresh(AdViewPosition adViewPosition) { throw new NotImplementedException(); }
        public void                            LoadMREC(AdViewPosition adViewPosition)             { throw new NotImplementedException(); }
        
        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;
    }
}
#endif