namespace ServiceImplementation.AdsServices.PreloadService
{
    using Core.AnalyticServices.Data;

    public class PreLoadInter : PreLoadEvent
    {
        public PreLoadInter(string placement, long timeMilis, string mediation) : base(placement, timeMilis, mediation)
        {
        }
    }
}