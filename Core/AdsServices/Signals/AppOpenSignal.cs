namespace Core.AdsServices.Signals
{
   public class AppOpenFullScreenContentOpenedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentOpenedSignal(string placement) : base(placement)
       {
       }
   }
   public class AppOpenFullScreenContentClosedSignal : BaseAdsSignal
   {
       public AppOpenFullScreenContentClosedSignal(string placement) : base(placement)
       {
       }
   }
}