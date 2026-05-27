#if UGS_REMOTE_CONFIG
namespace ServiceImplementation.UGSRemoteConfig
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Auth;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Unity.Services.RemoteConfig;
    using Zenject;

    /// <summary>
    /// UGS Remote Config provider. Reuses <see cref="IAuthService"/> for UGS Core init + sign-in,
    /// then performs a single FetchConfigsAsync. Real-time push updates are not subscribed to in this iteration.
    /// </summary>
    internal class UGSRemoteConfig : IRemoteConfig
    {
        private const float SignInTimeoutSeconds = 10f;

        public struct UserAttributes { }
        public struct AppAttributes  { }

        private readonly SignalBus    signalBus;
        private readonly ILogService  logger;
        private readonly IAuthService authService;

        public bool IsConfigFetchedSucceed { get; private set; }

        public UGSRemoteConfig(SignalBus signalBus, ILogService logger, IAuthService authService)
        {
            this.signalBus   = signalBus;
            this.logger      = logger;
            this.authService = authService;
        }

        public async void Initialize()
        {
            try
            {
                var waitAuth = UniTask.WaitUntil(() => this.authService.IsSignedIn);
                var timeout  = UniTask.Delay(TimeSpan.FromSeconds(SignInTimeoutSeconds));
                var winner   = await UniTask.WhenAny(waitAuth, timeout);
                if (winner == 1)
                {
                    this.logger.Error($"[UGSRemoteConfig] Auth not ready after {SignInTimeoutSeconds}s. Skipping fetch; defaults will be used.");
                    return;
                }

                RemoteConfigService.Instance.FetchCompleted += this.OnFetchCompleted;
                await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
            }
            catch (Exception e)
            {
                this.logger.Error($"[UGSRemoteConfig] Initialize failed: {e}");
            }
        }

        private void OnFetchCompleted(ConfigResponse response)
        {
            RemoteConfigService.Instance.FetchCompleted -= this.OnFetchCompleted;

            switch (response.requestOrigin)
            {
                case ConfigOrigin.Default:
                    this.logger.Log("[UGSRemoteConfig] Fetched defaults (no remote yet).");
                    break;
                case ConfigOrigin.Cached:
                    this.logger.Log("[UGSRemoteConfig] Fetched from cache.");
                    break;
                case ConfigOrigin.Remote:
                    this.logger.Log("[UGSRemoteConfig] Fetched from remote.");
                    break;
            }

            this.IsConfigFetchedSucceed = response.requestOrigin != ConfigOrigin.Default;
            if (this.IsConfigFetchedSucceed)
            {
                this.signalBus.Fire(new RemoteConfigFetchedSucceededSignal(this));
            }
        }

        #region IRemoteConfig getters

        public string GetRemoteConfigStringValue(string key, string defaultValue = "") =>
            !this.IsConfigFetchedSucceed ? defaultValue : RemoteConfigService.Instance.appConfig.GetString(key, defaultValue);

        public bool GetRemoteConfigBoolValue(string key, bool defaultValue) =>
            !this.IsConfigFetchedSucceed ? defaultValue : RemoteConfigService.Instance.appConfig.GetBool(key, defaultValue);

        public long GetRemoteConfigLongValue(string key, long defaultValue) =>
            !this.IsConfigFetchedSucceed ? defaultValue : RemoteConfigService.Instance.appConfig.GetLong(key, defaultValue);

        public int GetRemoteConfigIntValue(string key, int defaultValue) =>
            !this.IsConfigFetchedSucceed ? defaultValue : RemoteConfigService.Instance.appConfig.GetInt(key, defaultValue);

        public float GetRemoteConfigFloatValue(string key, float defaultValue) =>
            !this.IsConfigFetchedSucceed ? defaultValue : RemoteConfigService.Instance.appConfig.GetFloat(key, defaultValue);

        public double GetRemoteConfigDoubleValue(string key, double defaultValue) =>
            !this.IsConfigFetchedSucceed ? defaultValue : RemoteConfigService.Instance.appConfig.GetFloat(key, (float)defaultValue);

        #endregion
    }
}
#endif
