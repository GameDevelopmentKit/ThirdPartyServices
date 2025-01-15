#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
namespace Core.AdsServices.Native
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using R3;
    using UnityEngine;
    using UnityEngine.UI;
    using Zenject;

    public class NativeAdsView : MonoBehaviour
    {
        [SerializeField] private GameObject nonAdsHolder;
        [SerializeField] private GameObject adsHolder;

        public RawImage   iconImage;
        public RawImage   adChoicesImage;
        public GameObject callToActionObj;
        public Text       headlineText;
        public Text       advertiserText;
        public Text       callToActionText;

        private INativeAdsService nativeAdsService;
        private Collider[]        colliders;
        private bool              isEnable;
        private bool              isInit;

        private IDisposable      changeScreenDisposable;
        private IScreenPresenter visibleScreen;
        private IScreenManager   screenManager;
        private ILogService      logService;
#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS

        private void Awake()
        {
            this.colliders = this.GetComponentsInChildren<Collider>(true);
            this.screenManager = this.GetCurrentContainer().Resolve<IScreenManager>();
            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);
            this.logService = this.GetCurrentContainer().Resolve<ILogService>();
        }

        private void Update()
        {
            if (Camera.main == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    Debug.Log($"Mouse clicked on ad {hit.collider.gameObject.name}");
                }

                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 6f);
            }
        }

        private void OnDisable() { this.SetColliderStatus(false); }

        private void OnDestroy()
        {
            this.ShowAds(false);
            this.changeScreenDisposable?.Dispose();
        }

        private void OnChangeScreen(IScreenPresenter screenPresenter)
        {
            if (this.visibleScreen == null) return;
            this.ShowAds(this.visibleScreen == screenPresenter);
        }

        private void ShowAds(bool isShow)
        {
            if (this.nonAdsHolder != null)
            {
                this.nonAdsHolder.SetActive(!isShow);
            }

            if (this.adsHolder != null)
            {
                this.adsHolder.SetActive(isShow);
            }

            if (this.isInit && !this.isEnable && isShow)
            {
                this.isEnable = true;
                this.IntervalCall();
            }

            this.isEnable = isShow;
            this.SetColliderStatus(isShow);
        }

        private void SetColliderStatus(bool isShow)
        {
            foreach (var col in this.colliders)
            {
                col.enabled = isShow;
            }
        }

        public void BindVisibleScreen(IScreenPresenter screenPresenter) { this.visibleScreen = screenPresenter; }

        /// <summary>
        /// for 2d
        /// </summary>
        /// <param name="nativeAd"></param>
        public void ShowNativeAds(NativeAd nativeAd, List<GameObject> registedObj)
        {
            this.logService.Log($"Start set native ad: {this.name}");
            this.logService.Log($"native star rating : {nativeAd.GetStarRating()}");
            this.logService.Log($"native store: {nativeAd.GetStore()}");
            this.logService.Log($"native Price: {nativeAd.GetPrice()}");
            this.logService.Log($"native advertiser text: {nativeAd.GetAdvertiserText()}");
            this.logService.Log($"native icon: {nativeAd.GetIconTexture()?.texelSize}");
            this.logService.Log($"native headline: {nativeAd.GetHeadlineText()}");
            this.logService.Log($"native call to action text: {nativeAd.GetCallToActionText()}");
            this.logService.Log($"native ad choice: {nativeAd.GetAdChoicesLogoTexture()?.texelSize}");

            // Get Texture2D for icon asset of native ad.
            this.headlineText.text = nativeAd.GetHeadlineText();

            this.advertiserText.text = nativeAd.GetAdvertiserText();

            this.callToActionText.text = nativeAd.GetCallToActionText();

            if (nativeAd.GetIconTexture() != null)
            {
                this.iconImage.gameObject.SetActive(true);
                this.iconImage.texture = nativeAd.GetIconTexture();
            }

            this.adChoicesImage.gameObject.SetActive(false);

            if (nativeAd.GetAdChoicesLogoTexture() != null)
            {
                this.adChoicesImage.gameObject.SetActive(true);
                this.adChoicesImage.texture = nativeAd.GetAdChoicesLogoTexture();
            }

            this.SetColliderStatus(true);
            this.GetCurrentContainer().Resolve<INativeAdsService>().RemoveNativeAd(nativeAd);

            if (!registedObj.Contains(this.iconImage.gameObject))
            {
                nativeAd.RegisterIconImageGameObject(this.iconImage.gameObject);
            }

            if (!registedObj.Contains(this.callToActionObj))
            {
                nativeAd.RegisterCallToActionGameObject(this.callToActionObj);
            }

            if (!registedObj.Contains(this.adChoicesImage.gameObject))
            {
                nativeAd.RegisterAdChoicesLogoGameObject(this.adChoicesImage.gameObject);
            }
        }

        /// <summary>
        /// For 3d Object ads
        /// </summary>
        /// <param name="nativeAdsService"></param>
        public void Init(INativeAdsService nativeAdsService)
        {
            this.nativeAdsService = nativeAdsService;
            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.isInit = true;
            this.isEnable = true;
            this.IntervalCall();
            this.ShowAds(true);
        }

        private async void IntervalCall()
        {
            if (!this.isEnable) return;
            if (this == null) return;
            await UniTask.SwitchToMainThread();
            this.nativeAdsService?.DrawNativeAds(this);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }
#endif
    }
}
#endif