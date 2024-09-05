namespace Core.AnalyticServices.CommonEvents
{
    using Core.AnalyticServices.Data;

    /// <summary>
    /// When an in-app banner appears
    /// </summary>
    public class BannerShown : IEvent
    {
        public string placement;

        public BannerShown(string placement)
        {
            this.placement = placement;
        }
    }
}