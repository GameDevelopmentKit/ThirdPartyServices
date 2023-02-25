namespace Core.AdsServices
{
    /// <summary>
    ///  this.Container.Bind<string>().FromInstance("").WhenInjectedInto<AdModWrapper>();
    /// </summary>
    public interface IAOAAdService
    {
        bool        IsShowingAd { get; }
        public void LoadAOAAd();
        public void ShowAdIfAvailable();
    }
}