namespace ServiceImplementation.IAPServices.Common
{
    using ServiceImplementation.IAPServices.Signals;
    using Zenject;

    public class BaseIapInstaller<T> : Installer<T> where T : BaseIapInstaller<T>
    {
        public override void InstallBindings()
        {
            this.Container.DeclareSignal<OnRestorePurchaseCompleteSignal>();
            this.Container.DeclareSignal<OnStartDoingIAPSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseSuccessSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseFailedSignal>();
        }
    }
}