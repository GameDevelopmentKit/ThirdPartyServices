namespace ServiceImplementation.FBInstant.Advertising
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class FbInstantAdService : IAdServices, IInitializable
    {
        private readonly FbInstantAdConfig      _config;
        private readonly FbInstantAdvertisement _advertisement;
        private readonly SignalBus              _signalBus;
        private readonly ILogService            _logger;

        public FbInstantAdService(
            FbInstantAdConfig config,
            FbInstantAdvertisement advertisement,
            SignalBus signalBus,
            ILogService logger
        )
        {
            this._config        = config;
            this._advertisement = advertisement;
            this._signalBus     = signalBus;
            this._logger        = logger;
        }

        public void Initialize()
        {
            // this.ShowBannerAd();
            this.LoadInterstitialAd();
            this.LoadRewardedAd();
        }

        #region Public

        public void ShowBannerAd(int _, int __)
        {
            this.Invoke(this._config.BannerAdIds, this._advertisement.ShowBannerAd);
        }

        public void HideBannedAd()
        {
            this.InvokeOnce(this._advertisement.HideBannerAd);
        }

        private void LoadInterstitialAd()
        {
            this.Invoke(this._config.InterstitialAdIds, this._advertisement.LoadInterstitialAd);
        }

        public bool IsInterstitialAdReady(string _)
        {
            return this._config.InterstitialAdIds.Any(this._advertisement.IsInterstitialAdReady);
        }

        public void ShowInterstitialAd(string place)
        {
            this.InvokeOnce(
                adIds: this._config.InterstitialAdIds,
                check: this._advertisement.IsInterstitialAdReady,
                action: this._advertisement.ShowInterstitialAd,
                onSuccess: () =>
                {
                    this.LoadInterstitialAd();
                    this._signalBus.Fire(new InterstitialAdDownloadedSignal(place));
                    this._signalBus.Fire(new InterstitialAdClosedSignal(place));
                },
                onError: (error) =>
                {
                    this._signalBus.Fire(new InterstitialAdLoadFailedSignal(place, error));
                    this._signalBus.Fire(new InterstitialAdClosedSignal(place));
                });
        }

        private void LoadRewardedAd()
        {
            this.Invoke(this._config.RewardedAdIds, this._advertisement.LoadRewardedAd);
        }

        public bool IsRewardedAdReady(string _)
        {
            return this._config.RewardedAdIds.Any(this._advertisement.IsRewardedAdReady);
        }

        public void ShowRewardedAd(string place)
        {
            this.ShowRewardedAd(place, null);
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            this.InvokeOnce(
                adIds: this._config.RewardedAdIds,
                check: this._advertisement.IsRewardedAdReady,
                action: this._advertisement.ShowRewardedAd,
                onSuccess: () =>
                {
                    this.LoadRewardedAd();
                    this._signalBus.Fire(new RewardedAdLoadedSignal(place));
                    this._signalBus.Fire(new RewardedAdCompletedSignal(place));
                    onCompleted();
                },
                onError: (error) =>
                {
                    this._signalBus.Fire(new RewardedAdLoadFailedSignal(place, error));
                    this._signalBus.Fire(new RewardedAdCompletedSignal(place));
                }
            );
        }

        #endregion

        #region Private

        private static readonly int[] RetryIntervals = { 4, 8, 16, 32, 64 };

        private void Invoke(
            string[] adIds,
            Func<string, UniTask<string>> action,
            [CallerMemberName] string caller = null
        )
        {
            UniTask.Void(async () =>
            {
                for (var index = 0;; ++index)
                {
                    var adId  = adIds[Math.Min(index, adIds.Length - 1)];
                    var error = await action(adId);
                    if (error is null) break;
                    this._logger.Error($"{caller} error {index + 1} time(s): {error}");
                    var retryInterval = RetryIntervals[Math.Min(index, RetryIntervals.Length - 1)];
                    await UniTask.WaitForSeconds(retryInterval);
                }
                this._logger.Log($"{caller} success");
            });
        }

        private void InvokeOnce(
            string[] adIds,
            Func<string, bool> check,
            Func<string, UniTask<string>> action,
            Action onSuccess = null,
            Action<string> onError = null,
            [CallerMemberName] string caller = null
        )
        {
            var adId = adIds.FirstOrDefault(check);
            if (adId is null)
            {
                this._logger.Error($"{caller} error: No ad ready");
                onError?.Invoke("No ad ready");
                return;
            }
            this.InvokeOnce(() => action(adId), onSuccess, onError, caller);
        }

        private void InvokeOnce(
            Func<UniTask<string>> action,
            Action onSuccess = null,
            Action<string> onError = null,
            [CallerMemberName] string caller = null
        )
        {
            action().ContinueWith(error =>
            {
                if (error is null)
                {
                    this._logger.Log($"{caller} success");
                    onSuccess?.Invoke();
                }
                else
                {
                    this._logger.Error($"{caller} error: {error}");
                    onError?.Invoke(error);
                }
            }).Forget();
        }

        #endregion

        #region Not Implemented

        public void GrantDataPrivacyConsent()
        {
            throw new NotImplementedException();
        }

        public void RevokeDataPrivacyConsent()
        {
            throw new NotImplementedException();
        }

        public void DestroyBannerAd()
        {
            throw new NotImplementedException("Use HideBannerAd instead");
        }

        public bool IsRewardedInterstitialAdReady()
        {
            throw new NotImplementedException();
        }

        public void ShowRewardedInterstitialAd(string place)
        {
            throw new NotImplementedException();
        }

        public void ShowRewardedInterstitialAd(string place, Action onCompleted)
        {
            throw new NotImplementedException();
        }

        public void RemoveAds(bool revokeConsent = false)
        {
            throw new NotImplementedException();
        }

        public bool IsAdsInitialized()
        {
            return true;
        }

        public bool IsRemoveAds()
        {
            return false;
        }

        #endregion
    }
}