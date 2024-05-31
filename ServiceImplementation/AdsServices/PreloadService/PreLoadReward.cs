namespace ServiceImplementation.AdsServices.PreloadService
{
    using Core.AnalyticServices.Data;

    public class PreLoadReward : PreLoadEvent
    {
        public PreLoadReward(string placement, long timeMilis, string mediation) : base(placement, timeMilis, mediation)
        {
        }
    }
}