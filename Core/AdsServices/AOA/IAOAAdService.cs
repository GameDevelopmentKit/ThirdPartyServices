namespace Core.AdsServices
{
    /// <summary>
    ///  this.Container.Bind<string>().FromInstance("").WhenInjectedInto<AdModWrapper>();
    /// </summary>
    public interface IAOAAdService
    {
        bool IsAOAReady();
        void ShowAOAAds();
    }
}