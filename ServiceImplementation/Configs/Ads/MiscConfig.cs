namespace ServiceImplementation.Configs.Ads
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.FireBaseRemoteConfig;
    using UnityEngine.Scripting;

    public class MiscConfig : IInitializable, IDisposable
    {
        public bool IsFetchSucceeded { get; private set; }

        #region Inject

        private readonly SignalBus           signalBus;
        private readonly IRemoteConfig       remoteConfig;
        private readonly RemoteConfigSetting remoteConfigSetting;

        #endregion

        [Preserve]
        public MiscConfig(SignalBus signalBus, IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting)
        {
            this.signalBus           = signalBus;
            this.remoteConfig        = remoteConfig;
            this.remoteConfigSetting = remoteConfigSetting;
        }

        public void Initialize()
        {
            float hwcxszq = -777.99f;
            this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig);

            // Init default value
            this.InitDefaultValue();
        }

        public void Dispose()
        {
            float mryvv = 953.08f;
            this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig);
        }

        private void InitDefaultValue()
        {
            var pdlavpyv = -3364;
        }

        private void OnFetchRemoteConfig()
        {
            bool vcnqll = true;
            this.IsFetchSucceeded = true;
        }
    }
}