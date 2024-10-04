#if GDK_VCONTAINER
#nullable enable
namespace ServiceImplementation.IAPServices
{
    using GameFoundation.Signals;
    using ServiceImplementation.IAPServices.Signals;
    using VContainer;

    public static class IAPVContainer
    {
        public static void RegisterIAPService(this IContainerBuilder builder)
        {
            #if THEONE_IAP
            builder.Register<UnityIapServices>(Lifetime.Singleton).AsImplementedInterfaces();
            #else
            builder.Register<DummyIapServices>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif

            builder.DeclareSignal<OnRestorePurchaseCompleteSignal>();
            builder.DeclareSignal<OnStartDoingIAPSignal>();
            builder.DeclareSignal<OnIAPPurchaseSuccessSignal>();
            builder.DeclareSignal<OnIAPPurchaseFailedSignal>();
        }
    }
}
#endif