namespace Core.AdsServices
{
    /// <summary>
    ///  this.Container.Bind<string>().FromInstance("").WhenInjectedInto<AdModWrapper>();
    /// </summary>
    public interface IAOAAdService
    {
        public void LoadAOAAd();
        public bool IsAppOpenAdLoaded();
        public void ShowAdIfAvailable();
    }
}