namespace ServiceImplementation.Configs.Ads
{
    using System;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Zenject;

    public class MiscConfig : IInitializable, IDisposable
    {
        public bool IsFetchSucceeded { get; private set; }

        public bool EnableUMP { get; set; }

        #region Key

        private const string EnalbeUMPKey = "enable_ump";

        #endregion

        #region Inject

        private readonly SignalBus           signalBus;
        private readonly IRemoteConfig       remoteConfig;
        private readonly RemoteConfigSetting remoteConfigSetting;

        #endregion

        public MiscConfig(SignalBus signalBus, IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting)
        {
            this.signalBus           = signalBus;
            this.remoteConfig        = remoteConfig;
            this.remoteConfigSetting = remoteConfigSetting;
        }

        public void Initialize() { this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig); }

        public void Dispose() { this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig); }

        private void OnFetchRemoteConfig()
        {
            var umpSetting = this.remoteConfigSetting.GetRemoteConfig(EnalbeUMPKey);
            this.EnableUMP        = this.remoteConfig.GetRemoteConfigBoolValue(umpSetting.key, bool.Parse(umpSetting.defaultValue));
            this.IsFetchSucceeded = true;
        }
    }
}