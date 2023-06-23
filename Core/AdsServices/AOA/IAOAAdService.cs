namespace Core.AdsServices
{
    /// <summary>
    ///  this.Container.Bind<string>().FromInstance("").WhenInjectedInto<AdModWrapper>();
    /// </summary>
    public interface IAOAAdService
    {
        bool        IsShowingAd           { get; }
        bool        IsShowedFirstOpen     { get; }
        bool        IsResumedFromAdsOrIAP { get; set; }
        float       LoadingTimeToShowAOA  { get; }
        public void ShowAdIfAvailable();
    }
}