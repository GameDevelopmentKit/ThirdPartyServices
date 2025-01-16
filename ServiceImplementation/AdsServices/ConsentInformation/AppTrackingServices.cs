namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.AdsServices.Signal;
    using ServiceImplementation.Configs;
    #if UNITY_IOS
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
            if (this.thirdPartiesConfig.AdSettings.customAtt) return;
            this.RequestTracking().Forget();
        }

        public async UniTask RequestTracking()
        {
            this.signalBus.Fire(new AttDisplayedSignal());
            await this.RequestUmpConsent();
            if (!this.thirdPartiesConfig.AdSettings.RequestUmpInsteadATT)
            {
                this.RequestAtt();
            }
            await UniTask.WaitUntil(AttHelper.IsRequestTrackingComplete);
            this.signalBus.Fire(new AttClosedSignal());
        }

        private void RequestAtt()
        {
            if (AttHelper.IsRequestTrackingComplete() || !this.consentInformation.CanRequestAds()) return;

            #if UNITY_IOS
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            #endif
        }

        private async UniTask RequestUmpConsent()
        {
            // Setup GDPR and IDFA will auto request ATT
            this.consentInformation.RequestConsent();
            await UniTask.WaitUntil(() => !this.consentInformation.IsRequestingConsent());
        }
    }
}