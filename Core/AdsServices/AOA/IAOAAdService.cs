namespace Core.AdsServices
{
    using System;

    /// <summary>
    ///  this.Container.Bind<string>().FromInstance("").WhenInjectedInto<AdModWrapper>();
    /// </summary>
    public interface IAOAAdService
    {
        bool IsAOAReady();
        void ShowAOAAds(string placement,Action onDone=null);
        int  Order        { get; }
        bool IsShowingAOAAd { get; set; }
    }
}