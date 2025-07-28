namespace Core.AdsServices.Native
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using R3;
    using UnityEngine;
    using UnityEngine.UI;

    public class NativeAdsView : MonoBehaviour, INativeAdsView
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
        private Camera           cam;
        public  bool             useUiCam = true;

        private void Awake()
        {
            this.colliders              = this.GetComponentsInChildren<Collider>(true);
            this.screenManager          = this.GetCurrentContainer().Resolve<IScreenManager>();
            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);
            this.logService             = this.GetCurrentContainer().Resolve<ILogService>();

            if (this.useUiCam)
            {
                this.cam = this.GetCurrentContainer().Resolve<ScreenManager>().RootUICanvas.UICamera;
            }

            this.cam ??= Camera.main;
        }

        private void Update()
        {
            if (!this.cam) return;

            if (Input.GetMouseButtonDown(0))
            {
                var ray = this.cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    Debug.Log($"Mouse clicked on ad {hit.collider.gameObject.name}");
#if UNITY_EDITOR

                    Application.OpenURL("https://www.google.com/search?q=ad+clicked");
#endif
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
        public void ShowNativeAds(NativeAdInstanceWrapper nativeAd, List<GameObject> registedObj)
        {
            this.SetColliderStatus(true);
#if UNITY_EDITOR
            return;
#endif
#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
            var nativeAdInstance = (GoogleMobileAds.Api.NativeAd)nativeAd.NativeAdInstance;

            this.logService.Log($"Start set native ad: {this.name}");
            this.logService.Log($"native star rating : {nativeAdInstance.GetStarRating()}");
            this.logService.Log($"native store: {nativeAdInstance.GetStore()}");
            this.logService.Log($"native Price: {nativeAdInstance.GetPrice()}");
            this.logService.Log($"native advertiser text: {nativeAdInstance.GetAdvertiserText()}");
            this.logService.Log($"native icon: {nativeAdInstance.GetIconTexture()?.texelSize}");
            this.logService.Log($"native headline: {nativeAdInstance.GetHeadlineText()}");
            this.logService.Log($"native call to action text: {nativeAdInstance.GetCallToActionText()}");
            this.logService.Log($"native ad choice: {nativeAdInstance.GetAdChoicesLogoTexture()?.texelSize}");

            // Get Texture2D for icon asset of native ad.
            this.headlineText.text = nativeAdInstance.GetHeadlineText();

            this.advertiserText.text = nativeAdInstance.GetAdvertiserText();

            this.callToActionText.text = nativeAdInstance.GetCallToActionText();

            if (nativeAdInstance.GetIconTexture() != null)
            {
                this.iconImage.gameObject.SetActive(true);
                this.iconImage.texture = nativeAdInstance.GetIconTexture();
            }

            this.adChoicesImage.gameObject.SetActive(false);

            if (nativeAdInstance.GetAdChoicesLogoTexture() != null)
            {
                this.adChoicesImage.gameObject.SetActive(true);
                this.adChoicesImage.texture = nativeAdInstance.GetAdChoicesLogoTexture();
            }

            this.GetCurrentContainer().Resolve<INativeAdsService>().RemoveNativeAd(nativeAd);

            if (!registedObj.Contains(this.iconImage.gameObject))
            {
                nativeAdInstance.RegisterIconImageGameObject(this.iconImage.gameObject);
            }

            if (!registedObj.Contains(this.callToActionObj))
            {
                nativeAdInstance.RegisterCallToActionGameObject(this.callToActionObj);
            }

            if (!registedObj.Contains(this.adChoicesImage.gameObject))
            {
                nativeAdInstance.RegisterAdChoicesLogoGameObject(this.adChoicesImage.gameObject);
            }
#endif
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
            this.isInit   = true;
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

        public GameObject Instance => this.gameObject;
    }
}