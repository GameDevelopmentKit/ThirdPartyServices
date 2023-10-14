namespace Core.AdsServices
{
    /// <summary>
    ///  this.Container.Bind<string>().FromInstance("").WhenInjectedInto<AdModWrapper>();
    /// </summary>
    public interface IAOAAdService
    {
        bool        IsShowedFirstOpen     { get; }
        bool        IsResumedFromAdsOrIAP { get; set; }
        float       LoadingTimeToShowAOA  { get; }
    }
}