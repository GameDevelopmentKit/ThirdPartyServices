#if FIREBASE_REMOTE_CONFIG
namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Firebase;
    using Firebase.RemoteConfig;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using Zenject;

    internal class FirebaseRemoteConfigMobile : MonoBehaviour, IRemoteConfig
    {
        [Inject] private readonly ILogService logger;
        [Inject] private readonly ISignalBus  signalBus;

        public bool IsConfigFetchedSucceed { get; private set; }

        private void Start() { this.InitFirebase().Forget(); }

        private async UniTaskVoid InitFirebase()
        {
            await UniTask.DelayFrame(1);

            this.logger.Log("FirebaseRemoteConfig InitFirebase");

            DependencyStatus dependencyStatus;

            try
            {
                dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.logger.Error($"Firebase dependency check failed : {e}");

                return;
            }

            this.logger.Log($"FirebaseRemoteConfig CheckAndFixDependenciesAsync {dependencyStatus}");

            if (dependencyStatus != DependencyStatus.Available)
            {
                this.logger.Error($"Could not resolve all Firebase dependencies: {dependencyStatus}");

                return;
            }

            await this.FetchDataAsync();
        }

        private async UniTask FetchDataAsync()
        {
            try
            {
                await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.logger.Error($"FirebaseRemoteConfig Fetch exception : {e}");

                return;
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;

            this.logger.Log($"FirebaseRemoteConfig FetchComplete {info.LastFetchStatus}");

            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:

                    await FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ConfigureAwait(false);

                    await UniTask.SwitchToMainThread();

                    this.logger.Log($"FirebaseRemoteConfig Remote data loaded and ready (last fetch time {info.FetchTime}).");

                    this.IsConfigFetchedSucceed = true;

                    this.signalBus.Fire(new RemoteConfigFetchedSucceededSignal(this));

                    break;

                case LastFetchStatus.Failure:

                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            this.logger.Log("FirebaseRemoteConfig Fetch failed for unknown reason");

                            break;

                        case FetchFailureReason.Throttled:
                            this.logger.Log($"FirebaseRemoteConfig Fetch throttled until {info.ThrottledEndTime}");

                            break;
                    }

                    break;

                case LastFetchStatus.Pending:

                    this.logger.Log("FirebaseRemoteConfig Latest Fetch call still pending.");

                    break;
            }
        }

        #region Get Data Remote Config

        public string GetRemoteConfigStringValue(string key, string defaultValue)
        {
            return !this.HasKey(key) ? defaultValue : FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
        }

        public bool GetRemoteConfigBoolValue(string key, bool defaultValue)
        {
            if (!this.HasKey(key))
            {
                return defaultValue;
            }

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return bool.TryParse(value, out var result) && result;
        }

        public long GetRemoteConfigLongValue(string key, long defaultValue)
        {
            if (!this.HasKey(key))
            {
                return defaultValue;
            }

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return long.TryParse(value, out var result) ? result : defaultValue;
        }

        public double GetRemoteConfigDoubleValue(string key, double defaultValue)
        {
            if (!this.HasKey(key))
            {
                return defaultValue;
            }

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return double.TryParse(value, out var result) ? result : defaultValue;
        }

        public int GetRemoteConfigIntValue(string key, int defaultValue)
        {
            if (!this.HasKey(key))
            {
                return defaultValue;
            }

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public float GetRemoteConfigFloatValue(string key, float defaultValue)
        {
            if (!this.HasKey(key))
            {
                return defaultValue;
            }

            var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;

            return float.TryParse(value, out var result) ? result : defaultValue;
        }

        private bool HasKey(string key)
        {
            if (!this.IsConfigFetchedSucceed || FirebaseRemoteConfig.DefaultInstance == null)
            {
                return false;
            }

            return FirebaseRemoteConfig.DefaultInstance.Keys != null && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(key);
        }

        #endregion
    }
}
#endif