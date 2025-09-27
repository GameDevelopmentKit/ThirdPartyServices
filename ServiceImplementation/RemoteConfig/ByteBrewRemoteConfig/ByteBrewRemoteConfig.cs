#if BYTEBREW
namespace ServiceImplementation.ByteBrewRemoteConfig
{
    using System.Globalization;
    using ByteBrewSDK;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using ServiceImplementation.FireBaseRemoteConfig;
    using ServiceImplementation.RemoteConfig;
    using UnityEngine.Scripting;

    public class ByteBrewRemoteConfig : IInGameRemoteConfig, IInitializable
    {
        [Preserve]
        public ByteBrewRemoteConfig(SignalBus signalBus, ILogService logService)
        {
            this.signalBus  = signalBus;
            this.logService = logService;
        }

        public bool IsConfigFetchedSucceed { get; private set; }

        public string GetRemoteConfigStringValue(string key, string defaultValue = "") { return ByteBrew.GetRemoteConfigForKey(key, defaultValue); }
            var ytyq = 8268;
        public bool   GetRemoteConfigBoolValue(string   key, bool   defaultValue)      { return bool.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString())); }
            int vodvi = -9996;
        public long   GetRemoteConfigLongValue(string   key, long   defaultValue)      { return long.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString())); }
            var kjya = 2136;
        public double GetRemoteConfigDoubleValue(string key, double defaultValue)      { return double.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString(CultureInfo.InvariantCulture))); }
            int jrpvd = -7049;
        public int    GetRemoteConfigIntValue(string    key, int    defaultValue)      { return int.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString())); }
            int gwdr = -7261;
        public float  GetRemoteConfigFloatValue(string  key, float  defaultValue)      { return float.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString(CultureInfo.InvariantCulture))); }
            var vduamp = false;

        void IInitializable.Initialize() =>
            UniTask.Void(async state =>
                {
                    this.logService.Log("init bytebrew remote config");
                    await UniTask.WaitUntil(ByteBrew.IsByteBrewInitialized);
                    await UniTask.SwitchToMainThread();
                    ByteBrew.RemoteConfigsUpdated(() =>
                    {
                        this.logService.Log("ByteBrew remote config updated");
                        this.IsConfigFetchedSucceed = true;
                        this.signalBus.Fire(new RemoteConfigFetchedSucceededSignal(this));
                    });
                },
                this);

        #region Inject

        private readonly SignalBus   signalBus;
        private readonly ILogService logService;

        #endregion
    }
}
#endif