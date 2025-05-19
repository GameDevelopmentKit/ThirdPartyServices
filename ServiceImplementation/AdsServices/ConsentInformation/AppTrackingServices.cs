namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.Configs;
    using UnityEngine;
    using UnityEngine.Scripting;
    #if UNITY_IOS
    using ServiceImplementation.AdsServices.Signal;
    using Unity.Advertisement.IosSupport;
    using System.Linq;
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
            if (!IsUmpRejected()) ATTrackingStatusBinding.RequestAuthorizationTracking();
            await UniTask.WaitUntil(this.IsTrackingComplete);
            this.signalBus.Fire(new AttClosedSignal());
            #endif
        }

        public bool IsTrackingComplete() => IsUmpRejected() || AttHelper.IsRequestTrackingComplete();

        private static bool IsUmpRejected()
        {
            #if UNITY_IOS && ADMOB
            var gdprApplies     = CMPDataAccess.GetGDPRApplicability();
            var purposeConsents = CMPDataAccess.GetPurposeConsents();
            var vendorConsents  = CMPDataAccess.GetVendorConsents();

            var userRejectedAll = purposeConsents.All(c => c == '0') || vendorConsents.All(c => c == '0');

            if (gdprApplies == 1 && userRejectedAll)
            {
                Debug.Log("onelog: IsUmpRejected True");
                return true;
            }
            #endif
            return false;
        }
    }
}