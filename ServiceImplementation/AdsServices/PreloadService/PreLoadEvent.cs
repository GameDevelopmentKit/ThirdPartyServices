namespace ServiceImplementation.AdsServices.PreloadService
{
    using Core.AnalyticServices.Data;

    public abstract class PreLoadEvent : IEvent
    {
        public string Mediation;
        public string Placement;
        public long   TimeMilis;
        
        public PreLoadEvent(string placement, long timeMilis, string mediation)
        {
            this.Placement = placement;
            this.TimeMilis = timeMilis;
            this.Mediation = mediation;
        }
    }
}