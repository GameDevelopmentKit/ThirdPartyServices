namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.Configs;
    using UnityEngine.Scripting;
    #if UNITY_IOS
    using ServiceImplementation.AdsServices.Signal;
    using Unity.Advertisement.IosSupport;
    #endif

    public class AppTrackingServices : IInitializable
    {
        private const int DelayRequestTrackingMillisecond = 100;

        private readonly ThirdPartiesConfig  thirdPartiesConfig;
        private readonly SignalBus           signalBus;
        private readonly IConsentInformation consentInformation;

        [Preserve]
        public AppTrackingServices(ThirdPartiesConfig thirdPartiesConfig, SignalBus signalBus, IConsentInformation consentInformation)
        {
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.signalBus          = signalBus;
            this.consentInformation = consentInformation;
        }

        public async void Initialize()
        {
            await UniTask.Delay(DelayRequestTrackingMillisecond);
            #if UNITY_IOS
            if (this.thirdPartiesConfig.AdSettings.customAtt) return;
            #endif
            this.RequestConsentAndTracking().Forget();
        }

        public async UniTask RequestConsentAndTracking()
        {
            this.consentInformation.RequestConsent();
            await UniTask.WaitUntil(() => !this.consentInformation.IsRequestingConsent());

            #if UNITY_IOS
            this.signalBus.Fire(new AttDisplayedSignal());
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            await UniTask.WaitUntil(this.IsTrackingComplete);

            this.signalBus.Fire(new AttClosedSignal());
            #endif
        }

        public bool IsTrackingComplete() => AttHelper.IsRequestTrackingComplete();
    }
}