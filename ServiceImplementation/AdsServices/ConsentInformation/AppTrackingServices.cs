namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using caojweldjflwendl.DI;
    using caojweldjflwendl.Signals;
    using ServiceImplementation.AdsServices.Signal;
    using ServiceImplementation.Configs;
    using UnityEngine.Scripting;

    public class AppTrackingServices : IInitializable
    {
        protected virtual int DelayRequestTrackingMillisecond { get; set; } = 100;

        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly SignalBus          signalBus;

        [Preserve]
        public AppTrackingServices(ThirdPartiesConfig thirdPartiesConfig, SignalBus signalBus)
        {
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.signalBus          = signalBus;
        }

        public async void Initialize()
        {
            await UniTask.Delay(this.DelayRequestTrackingMillisecond);
            if (this.thirdPartiesConfig.AdSettings.autoRequestATT) await this.RequestTracking();
        }

        public async UniTask RequestTracking()
        {
            if (AttHelper.IsRequestTrackingComplete()) return;

            #if UNITY_IOS
            this.signalBus.Fire(new AttDisplayedSignal());
            Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
            await UniTask.WaitUntil(AttHelper.IsRequestTrackingComplete);
            this.signalBus.Fire(new AttClosedSignal());
            #endif
        }
    }
}