namespace ServiceImplementation.Configs.Ads
{
    using System;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Zenject;

    public class MiscConfig : IInitializable, IDisposable
    {
        public bool IsFetchSucceeded { get; private set; }

        #region Inject

        private readonly ISignalBus          signalBus;
        private readonly IRemoteConfig       remoteConfig;
        private readonly RemoteConfigSetting remoteConfigSetting;

        #endregion

        public MiscConfig(ISignalBus signalBus, IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting)
        {
            this.signalBus           = signalBus;
            this.remoteConfig        = remoteConfig;
            this.remoteConfigSetting = remoteConfigSetting;
        }

        public void Initialize()
        {
            this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig);

            // Init default value
            this.InitDefaultValue();
        }

        public void Dispose() { this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig); }

        private void InitDefaultValue() { }

        private void OnFetchRemoteConfig()
        {
            this.IsFetchSucceeded = true;
        }
    }
}