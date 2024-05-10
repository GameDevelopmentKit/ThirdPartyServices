namespace ServiceImplementation.ByteBrewRemoteConfig
{
    using System.Globalization;
    using ByteBrewSDK;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Zenject;

    internal class ByteBrewRemoteConfig : IRemoteConfig, IInitializable
    {
        #region Inject

        private readonly SignalBus   signalBus;
        private readonly ILogService logService;

        #endregion

        public ByteBrewRemoteConfig(SignalBus signalBus, ILogService logService)
        {
            this.signalBus  = signalBus;
            this.logService = logService;
        }

        public bool IsConfigFetchedSucceed { get; }

        public string GetRemoteConfigStringValue(string key, string defaultValue = "") { return ByteBrew.GetRemoteConfigForKey(key, defaultValue); }
        public bool   GetRemoteConfigBoolValue(string key, bool defaultValue) { return bool.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString())); }
        public long   GetRemoteConfigLongValue(string key, long defaultValue) { return long.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString())); }
        public double GetRemoteConfigDoubleValue(string key, double defaultValue) { return double.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString(CultureInfo.InvariantCulture))); }
        public int    GetRemoteConfigIntValue(string key, int defaultValue) { return int.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString())); }
        public float  GetRemoteConfigFloatValue(string key, float defaultValue) { return float.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString(CultureInfo.InvariantCulture))); }

        public async void Initialize()
        {
            await UniTask.WaitUntil(() => ByteBrew.IsInitilized);
            await UniTask.SwitchToMainThread();
            ByteBrew.RemoteConfigsUpdated(() =>
            {
                this.logService.Log("Byte brew remote config updated");
                this.signalBus.Fire(new RemoteConfigFetchedSucceededSignal());
            });
        }
    }
}