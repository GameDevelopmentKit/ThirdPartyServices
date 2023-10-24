namespace Core.AdsServices
{
    using System;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Zenject;

    public class MiscConfig : IInitializable, IDisposable
    {
        public bool EnableUMP { get; set; }

        #region Key

        private const string EnalbeUMPKey = "enalbe_ump";

        #endregion

        #region Inject

        private readonly SignalBus     signalBus;
        private readonly IRemoteConfig remoteConfig;

        #endregion

        public MiscConfig(SignalBus signalBus, IRemoteConfig remoteConfig)
        {
            this.signalBus    = signalBus;
            this.remoteConfig = remoteConfig;
        }

        public void Initialize() { this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig); }

        public void Dispose() { this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.OnFetchRemoteConfig); }

        private void OnFetchRemoteConfig() { this.EnableUMP = this.remoteConfig.GetRemoteConfigBoolValue(EnalbeUMPKey, false); }
    }
}