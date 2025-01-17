namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.Configs;
    #if UNITY_IOS
    using ServiceImplementation.AdsServices.Signal;
    using Unity.Advertisement.IosSupport;
    #endif
    using UnityEngine.Scripting;

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
            this.RequestTracking().Forget();
        }

        public async UniTask RequestTracking()
        {
            #if UNITY_IOS
            this.signalBus.Fire(new AttDisplayedSignal());
            #endif

            await this.RequestUmpConsent();

            #if UNITY_IOS
            if (!this.thirdPartiesConfig.AdSettings.RequestUmpInsteadATT)
            {
                this.RequestAtt();
            }
            await UniTask.WaitUntil(() => AttHelper.IsRequestTrackingComplete() || this.consentInformation.CanRequestAds());

            this.signalBus.Fire(new AttClosedSignal());
            #endif
        }

        #if UNITY_IOS
        private void RequestAtt()
        {
            if (AttHelper.IsRequestTrackingComplete()) return;

            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
        #endif

        private async UniTask RequestUmpConsent()
        {
            // Setup GDPR and IDFA will auto request ATT
            this.consentInformation.RequestConsent();
            await UniTask.WaitUntil(() => !this.consentInformation.IsRequestingConsent());
        }
    }
}