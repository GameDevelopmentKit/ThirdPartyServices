namespace Core.AdsServices.Signals
{
    using Core.AnalyticServices.CommonEvents;

    public class AppOpenFullScreenContentOpenedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentOpenedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
       {
       }
   }
   
   public class AppOpenFullScreenContentFailedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentFailedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
       {
       }
   }
   
   public class AppOpenFullScreenContentClosedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentClosedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
       {
       }
   }
   
   public class AppOpenLoadedSignal : BaseAdsSignal
   {
       public AppOpenLoadedSignal(string placement,AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
       {
       }
   }
   
   public class AppOpenLoadFailedSignal : BaseAdsSignal
   {
       public AppOpenLoadFailedSignal(string placement) : base(placement)
       {
       }
   }
   
   public class AppOpenEligibleSignal : BaseAdsSignal
   {
       public AppOpenEligibleSignal(string placement) : base(placement)
       {
       }
   }
   
   public class AppOpenCalledSignal : BaseAdsSignal
   {
       public AppOpenCalledSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
       {
       }
   }
   
   public class AppOpenClickedSignal : BaseAdsSignal
   {
       public AppOpenClickedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
       {
       }
   }
}