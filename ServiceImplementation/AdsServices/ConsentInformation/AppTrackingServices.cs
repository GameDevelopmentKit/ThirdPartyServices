namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.AdsServices.Signal;
    using ServiceImplementation.Configs;
    using Zenject;

    public class AppTrackingServices : IInitializable
    {
        protected virtual int DelayRequestTrackingMillisecond { get; set; } = 100;

        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly ISignalBus         signalBus;

        public AppTrackingServices(ThirdPartiesConfig thirdPartiesConfig,ISignalBus signalBus)
        {
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.signalBus          = signalBus;
        }

        public async void Initialize()
        {
            await UniTask.Delay(this.DelayRequestTrackingMillisecond);
            if (this.thirdPartiesConfig.AdSettings.autoRequestATT)
            {
                await RequestTracking();
            }
        }

        public  async UniTask RequestTracking()
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