namespace Core.AdsServices.Native
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public  GameObject        iconTarget;
        public  RawImage          iconImage;
        public  RawImage          adChoicesImage;
        public  GameObject        callToActionObj;
        public  Text              headlineText;
        public  Text              advertiserText;
        public  Text              callToActionText;
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

        [SerializeField] private bool isTestAutoShowNativeAds;

        private void Awake()
        {
            this.screenManager          = this.GetCurrentContainer().Resolve<IScreenManager>();
            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);
            this.logService             = this.GetCurrentContainer().Resolve<ILogService>();

            if (this.useUiCam)
            {
                this.cam = this.GetCurrentContainer().Resolve<ScreenManager>().RootUICanvas.UICamera;
            }

            this.cam ??= Camera.main;
            this.TestAutoShowNativeAds();
        }

        private async void TestAutoShowNativeAds()
        {
            if (!this.isTestAutoShowNativeAds)
            {
                return;
            }

            var nativeAdsCount = this.GetCurrentContainer().Resolve<INativeAdsService>().GetNativeAds();

            if (nativeAdsCount.Count == 0)
            {
                await UniTask.Delay(3000);

                this.TestAutoShowNativeAds();

                return;
            }

            this.ShowNativeAds(nativeAdsCount.First(), new List<GameObject>());
        }

        private void Update()
        {
            if (!this.cam) return;

            if (this.IsClick(out var clickPosition))
            {
                var ray = this.cam.ScreenPointToRay(clickPosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    this.logService.Log($"[Ad] Clicked on: {hit.collider.gameObject.name} {hit.transform.GetComponent<BoxCollider>().size}");
                }
                else
                {
                    this.logService.Log($"[Ad] Raycast missed: {ray.origin}, {ray.direction}");
                }

                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 6f);
            }
        }

        private bool IsClick(out Vector2 clickPosition)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                clickPosition = Input.mousePosition;

                return true;
            }
#elif UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                clickPosition = Input.GetTouch(0).position;
                return true;
            }
#endif

            clickPosition = default;

            return false;
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
            this.colliders = this.GetComponentsInChildren<Collider>(true);

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
#if UNITY_EDITOR
            this.SetColliderStatus(true);
            this.GetCurrentContainer().Resolve<INativeAdsService>().RemoveNativeAd(nativeAd);
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

            if (!registedObj.Contains(this.iconImage.gameObject))
            {
                nativeAdInstance.RegisterIconImageGameObject(this.iconTarget?this.iconTarget:this.iconImage.gameObject);
            }

            if (!registedObj.Contains(this.callToActionObj))
            {
                this.logService.Log($"Register call to action game object: {this.callToActionObj.name}");
                this.callToActionObj.SetActive(true);
                nativeAdInstance.RegisterCallToActionGameObject(this.callToActionObj);
            }

            if (!registedObj.Contains(this.adChoicesImage.gameObject))
            {
                nativeAdInstance.RegisterAdChoicesLogoGameObject(this.adChoicesImage.gameObject);
            }

            this.SetColliderStatus(true);

            this.GetCurrentContainer().Resolve<INativeAdsService>().RemoveNativeAd(nativeAd);
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