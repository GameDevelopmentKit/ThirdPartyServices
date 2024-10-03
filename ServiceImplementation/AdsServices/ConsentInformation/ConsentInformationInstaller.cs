#if GDK_ZENJECT
namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using UnityEngine.Scripting;
    using Zenject;

    [Preserve]
    public class ConsentInformationInstaller : Installer<ConsentInformationInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<AppTrackingServices>().AsCached().NonLazy();
#if ADMOB
            this.Container.BindInterfacesAndSelfTo<UmpConsentInformation>().AsCached().NonLazy();
#else
            this.Container.BindInterfacesAndSelfTo<DummyConsentInformation>().AsCached().NonLazy();
#endif
        }
    }
}
#endif