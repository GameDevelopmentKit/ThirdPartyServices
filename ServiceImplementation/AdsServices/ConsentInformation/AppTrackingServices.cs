namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.Configs;
    using Zenject;

    public class AppTrackingServices : IInitializable
    {
        protected virtual int DelayRequestTrackingMillisecond { get; set; } = 100;

        private readonly ThirdPartiesConfig thirdPartiesConfig;

        public AppTrackingServices(ThirdPartiesConfig thirdPartiesConfig) { this.thirdPartiesConfig = thirdPartiesConfig; }

        public async void Initialize()
        {
            await UniTask.Delay(this.DelayRequestTrackingMillisecond);
            if (this.thirdPartiesConfig.AdSettings.autoRequestATT)
            {
                await RequestTracking();
            }
        }

        public static async UniTask RequestTracking()
        {
            if (AttHelper.IsRequestTrackingComplete()) return;

#if UNITY_IOS
            Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
            await UniTask.WaitUntil(AttHelper.IsRequestTrackingComplete);
#endif
        }
    }
}