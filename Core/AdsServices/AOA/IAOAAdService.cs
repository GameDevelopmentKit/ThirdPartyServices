namespace Core.AdsServices
{
    /// <summary>
    ///  this.Container.Bind<string>().FromInstance("").WhenInjectedInto<AdModWrapper>();
    /// </summary>
    public interface IAOAAdService
    {
        bool        IsShowingAd      { get; }
        bool        IsResumedFromAdsOrIAP { get; set; }
        public void ShowAdIfAvailable();
    }
}