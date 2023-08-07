namespace ServiceImplementation.FBInstant.Advertising
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;

    public class FbInstantAdvertisement : MonoBehaviour
    {
        #region Public

        public static FbInstantAdvertisement Instantiate()
        {
            var instance = new GameObject(nameof(FbInstantAdvertisement) + Guid.NewGuid())
                .AddComponent<FbInstantAdvertisement>();
            DontDestroyOnLoad(instance);
            return instance;
        }

        public UniTask<string> ShowBannerAd(string adId) => this.Invoke(adId, _showBannerAd);

        public UniTask<string> HideBannerAd() => this.Invoke(_hideBannerAd);

        public bool IsInterstitialAdReady(string adId) => _isInterstitialAdReady(adId);

        public UniTask<string> LoadInterstitialAd(string adId) => this.Invoke(adId, _loadInterstitialAd);

        public UniTask<string> ShowInterstitialAd(string adId) => this.Invoke(adId, _showInterstitialAd);

        public bool IsRewardedAdReady(string adId) => _isRewardedAdReady(adId);

        public UniTask<string> LoadRewardedAd(string adId) => this.Invoke(adId, _loadRewardedAd);

        public UniTask<string> ShowRewardedAd(string adId) => this.Invoke(adId, _showRewardedAd);

        #endregion

        #region Private

        private readonly Dictionary<string, UniTaskCompletionSource<string>> _tcs = new();

        private UniTask<string> Invoke(string adId, Action<string, string, string, string> action) => this.Invoke((callbackObj, callbackMethod, callbackId) => action(adId, callbackObj, callbackMethod, callbackId));

        private async UniTask<string> Invoke(Action<string, string, string> action)
        {
            var callbackId = Guid.NewGuid().ToString();
            this._tcs.Add(callbackId, new());
            try
            {
                action(this.gameObject.name, nameof(this.Callback), callbackId);
                return await this._tcs[callbackId].Task;
            }
            finally
            {
                this._tcs.Remove(callbackId);
            }
        }

        private void Callback(string message)
        {
            var @params    = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error      = @params["error"];
            var callbackId = @params["callbackId"];
            this._tcs[callbackId].TrySetResult(error);
        }

        #endregion

        #region DllImport

        [DllImport("__Internal")]
        private static extern void _showBannerAd(string adId, string callbackObj, string callbackMethod, string callbackId);

        [DllImport("__Internal")]
        private static extern void _hideBannerAd(string callbackObj, string callbackMethod, string callbackId);

        [DllImport("__Internal")]
        private static extern bool _isInterstitialAdReady(string adId);

        [DllImport("__Internal")]
        private static extern void _loadInterstitialAd(string adId, string callbackObj, string callbackMethod, string callbackId);

        [DllImport("__Internal")]
        private static extern void _showInterstitialAd(string adId, string callbackObj, string callbackMethod, string callbackId);

        [DllImport("__Internal")]
        private static extern bool _isRewardedAdReady(string adId);

        [DllImport("__Internal")]
        private static extern void _loadRewardedAd(string adId, string callbackObj, string callbackMethod, string callbackId);

        [DllImport("__Internal")]
        private static extern void _showRewardedAd(string adId, string callbackObj, string callbackMethod, string callbackId);

        #endregion
    }
}