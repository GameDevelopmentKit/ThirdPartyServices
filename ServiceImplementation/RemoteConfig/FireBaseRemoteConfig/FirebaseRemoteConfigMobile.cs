#if FIREBASE_REMOTE_CONFIG
namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Firebase;
    using Firebase.RemoteConfig;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class FirebaseRemoteConfigMobile : IRemoteConfig, IInitializable
    {
        private readonly ILogger             logger;
        private readonly SignalBus           signalBus;
        private readonly RemoteConfigSetting remoteConfigSetting;

        [Preserve]
        public FirebaseRemoteConfigMobile(ILoggerManager loggerManager, SignalBus signalBus, RemoteConfigSetting remoteConfigSetting)
        {
            this.logger              = loggerManager.GetLogger(this);
            this.signalBus           = signalBus;
            this.remoteConfigSetting = remoteConfigSetting;
        }

        public bool IsConfigFetchedSucceed { get; private set; }

        public void Initialize()
        {
            this.InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync()
        {
            this.logger.Info("Initializing");
            FirebaseRemoteConfig.GetInstance(FirebaseApp.DefaultInstance); // This fix a magic bug, don't remove it
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                this.logger.Error($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                return;
            }
            while (true)
            {
                try
                {
                    this.logger.Info("Fetching");
                    await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
                    await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                    this.logger.Info("Fetch succeeded");
                    break;
                }
                catch (OperationCanceledException)
                {
                    this.logger.Info("Fetch cancelled");
                }
                catch (Exception e)
                {
                    this.logger.Info("Fetch error: " + e.Message);
                    await UniTask.WaitForSeconds(this.remoteConfigSetting.FirebaseReloadInterval);
                }
            }
            await UniTask.SwitchToMainThread();
            this.IsConfigFetchedSucceed = true;
            this.signalBus.Fire(new RemoteConfigFetchedSucceededSignal(this));
        }

        #region Get Data Remote Config

        public string GetRemoteConfigStringValue(string key, string defaultValue)
        {
            return !this.HasKey(key) ? defaultValue : FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
        }

        public bool GetRemoteConfigBoolValue(string key, bool defaultValue)
        {
            if (!this.HasKey(key)) return defaultValue;

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return bool.TryParse(value, out var result) && result;
        }

        public long GetRemoteConfigLongValue(string key, long defaultValue)
        {
            if (!this.HasKey(key)) return defaultValue;

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return long.TryParse(value, out var result) ? result : defaultValue;
        }

        public double GetRemoteConfigDoubleValue(string key, double defaultValue)
        {
            if (!this.HasKey(key)) return defaultValue;

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return double.TryParse(value, out var result) ? result : defaultValue;
        }

        public int GetRemoteConfigIntValue(string key, int defaultValue)
        {
            if (!this.HasKey(key)) return defaultValue;

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public float GetRemoteConfigFloatValue(string key, float defaultValue)
        {
            if (!this.HasKey(key)) return defaultValue;

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return float.TryParse(value, out var result) ? result : defaultValue;
        }

        private bool HasKey(string key)
        {
            return this.IsConfigFetchedSucceed
                && FirebaseRemoteConfig.DefaultInstance.Keys != null
                && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(key);
        }

        #endregion
    }
}
#endif