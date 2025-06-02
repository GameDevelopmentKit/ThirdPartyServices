#if BYTEBREW
namespace ServiceImplementation.ByteBrewRemoteConfig
{
    using System.Globalization;
    using ByteBrewSDK;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.FireBaseRemoteConfig;
    using ServiceImplementation.RemoteConfig;
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class ByteBrewRemoteConfig : IInGameRemoteConfig, IInitializable
    {
        [Preserve]
        public ByteBrewRemoteConfig(SignalBus signalBus, ILoggerManager loggerManager)
        {
            this.signalBus = signalBus;
            this.logger    = loggerManager.GetLogger(this);
        }

        public bool IsConfigFetchedSucceed { get; private set; }

        public string GetRemoteConfigStringValue(string key, string defaultValue = "")
        {
            return ByteBrew.GetRemoteConfigForKey(key, defaultValue);
        }

        public bool GetRemoteConfigBoolValue(string key, bool defaultValue)
        {
            return bool.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString()));
        }

        public long GetRemoteConfigLongValue(string key, long defaultValue)
        {
            return long.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString()));
        }

        public double GetRemoteConfigDoubleValue(string key, double defaultValue)
        {
            return double.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString(CultureInfo.InvariantCulture)));
        }

        public int GetRemoteConfigIntValue(string key, int defaultValue)
        {
            return int.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString()));
        }

        public float GetRemoteConfigFloatValue(string key, float defaultValue)
        {
            return float.Parse(ByteBrew.GetRemoteConfigForKey(key, defaultValue.ToString(CultureInfo.InvariantCulture)));
        }

        void IInitializable.Initialize() =>
            UniTask.Void(async state =>
                {
                    this.logger.Info("init bytebrew remote config");
                    await UniTask.WaitUntil(ByteBrew.IsByteBrewInitialized);
                    await UniTask.SwitchToMainThread();
                    ByteBrew.RemoteConfigsUpdated(() =>
                    {
                        this.logger.Info("ByteBrew remote config updated");
                        this.IsConfigFetchedSucceed = true;
                        this.signalBus.Fire(new RemoteConfigFetchedSucceededSignal(this));
                    });
                },
                this);

        #region Inject

        private readonly SignalBus signalBus;
        private readonly ILogger   logger;

        #endregion
    }
}
#endif