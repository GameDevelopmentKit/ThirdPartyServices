namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.AdsServices.Signal;
    using ServiceImplementation.Configs;
    using UnityEngine;
    using Zenject;

    public class AppTrackingServices : IInitializable
    {
        protected virtual int DelayRequestTrackingMillisecond { get; set; } = 100;

        private readonly ThirdPartiesConfig  thirdPartiesConfig;
        private readonly ILogService         logger;
        private readonly ISignalBus          signalBus;
        private readonly IConsentInformation consentInformation;

        public AppTrackingServices(ThirdPartiesConfig thirdPartiesConfig, ILogService logger, ISignalBus signalBus, IConsentInformation consentInformation)
        {
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.logger             = logger;
            this.signalBus          = signalBus;
            this.consentInformation = consentInformation;
        }

        public async void Initialize()
        {
            await UniTask.Delay(this.DelayRequestTrackingMillisecond);

            if (this.thirdPartiesConfig.AdSettings.autoRequestATT)
            {
                await this.RequestTracking();
            }
        }

        public async UniTask RequestTracking()
        {
            if (AttHelper.IsRequestTrackingComplete()) return;

            await UniTask.WaitUntil(() => this.consentInformation.IsComplete);
            this.logger.LogWithColor($"Start IOS ATT request ", Color.red);
#if UNITY_IOS
            this.signalBus.Fire(new AttDisplayedSignal());
            Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
            await UniTask.WaitUntil(AttHelper.IsRequestTrackingComplete);
            this.signalBus.Fire(new AttClosedSignal());
#endif
        }
    }
}