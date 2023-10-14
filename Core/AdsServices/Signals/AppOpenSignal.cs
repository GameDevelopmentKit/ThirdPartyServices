namespace Core.AdsServices.Signals
{
   public class AppOpenFullScreenContentOpenedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentOpenedSignal(string placement) : base(placement)
       {
       }
   }
   
   public class AppOpenFullScreenContentFailedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentFailedSignal(string placement) : base(placement)
       {
       }
   }
   
   public class AppOpenFullScreenContentClosedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentClosedSignal(string placement) : base(placement)
       {
       }
   }
   
   public class AppOpenLoadedSignal : BaseAdsSignal
   {
       public AppOpenLoadedSignal(string placement) : base(placement)
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
       public AppOpenCalledSignal(string placement) : base(placement)
       {
       }
   }
   
   public class AppOpenClickedSignal : BaseAdsSignal
   {
       public AppOpenClickedSignal(string placement) : base(placement)
       {
       }
   }
}