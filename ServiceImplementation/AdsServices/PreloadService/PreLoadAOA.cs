namespace ServiceImplementation.AdsServices.PreloadService
{
    public class PreLoadAOA : PreLoadEvent
    {
        public PreLoadAOA(string placement, long timeMilis, string mediation) : base(placement, timeMilis, mediation)
        {
        }
    }
}