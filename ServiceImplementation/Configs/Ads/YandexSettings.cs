namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [Serializable]
    public class YandexSettings : AdNetworkSettings
    {
        private const string YandexSdkVersion = "7.2.0";

        public override Dictionary<AdPlacement, AdId> CustomBannerAdIds       { get; set; }
        public override Dictionary<AdPlacement, AdId> CustomInterstitialAdIds { get; set; }
        public override Dictionary<AdPlacement, AdId> CustomRewardedAdIds     { get; set; }

        public AdId BannerAdId
        {
#if THEONE_ADS_DEBUG
            get => "demo-banner-yandex";
#else
            get => this.mBannerAdId;
#endif
            set => this.mBannerAdId = value;
        }

        public AdId InterstitialAdId
        {
#if THEONE_ADS_DEBUG
            get => "demo-interstitial-yandex";
#else
            get => this.mInterstitialAdId;
#endif
            set => this.mInterstitialAdId = value;
        }

        public AdId RewardedAdId
        {
#if THEONE_ADS_DEBUG
            get => "demo-rewarded-yandex";
#else
            get => this.mRewardedAdId;
#endif
            set => this.mRewardedAdId = value;
        }

        public AdId AoaAdId
        {
#if THEONE_ADS_DEBUG
            get => "demo-appopenad-yandex";
#else
            get => this.mAoaAdId;
#endif
            set => this.mAoaAdId = value;
        }

        [SerializeField, LabelText("Banner"), BoxGroup("Ads Id")]
        private AdId mBannerAdId;

        [SerializeField, LabelText("Interstitial"), BoxGroup("Ads Id")]
        private AdId mInterstitialAdId;

        [SerializeField, LabelText("Rewarded"), BoxGroup("Ads Id")]
        private AdId mRewardedAdId;

        [SerializeField, LabelText("AOA"), BoxGroup("Ads Id")]
        private AdId mAoaAdId;

        #region Import sdk

        [ReadOnly, OnInspectorInit(nameof(UpdateVersionText)), HideLabel, MultiLineProperty(2), PropertyOrder(-2)]
        public string yandexVersion;

        public void UpdateVersionText()
        {
            const string path     = "Assets/YandexMobileAds/Editor/YandexMobileadsDependencies.xml";
            var          versions = UnityPackageHelper.ParseXmlFileGetPackageVersion(path);
            this.yandexVersion = $"current version: android-{versions.androidVersion} ios-{versions.iosVersion}\nlatest version: {YandexSdkVersion}";
        }

        public async void DownloadSDK()
        {
            var sdkUrl = $"https://github.com/yandexmobile/yandex-ads-unity-plugin/releases/download/{YandexSdkVersion}/yandex-mobileads-lite-{YandexSdkVersion}.unitypackage";
            await UnityPackageHelper.DownloadThenImportPackage(sdkUrl, "YandexSDK");

            CreateAsmDef();
            this.UpdateAllNetworkAdapters();
            this.UpdateVersionText();
        }

        private static void CreateAsmDef()
        {
#if UNITY_EDITOR
            const string path = "Assets/YandexMobileAds/YandexMobileAds.Scripts.asmdef";
            const string dir  = "Assets/YandexMobileAds";

            const string content = @"{
    ""name"": ""YandexMobileAds.Scripts"",
    ""rootNamespace"": """",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";

            Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
            AssetDatabase.Refresh();
#endif
        }

        private async void DownloadNetworkAdapter(string name, string xmlName, bool isEnable)
        {
            if (isEnable)
            {
                var url = $"https://github.com/yandexmobile/yandex-ads-unity-plugin/raw/master/mobileads-mediation/{name}/mobileads-{name}-mediation-{YandexSdkVersion}.unitypackage";
                UnityPackageHelper.DownloadThenImportPackage(url, name).Forget();
            }
            else
            {
                UnityPackageHelper.DeleteFileIfExists($"Assets/YandexMobileAds/Editor/{xmlName}.xml");
            }
#if UNITY_EDITOR
            await UniTask.Delay(1500);
            EditorWindow.focusedWindow.ShowNotification(new GUIContent($"{(isEnable ? "Add" : "Remove")} Network {name.ToUpper()} Successfully"));
#endif
        }

        private void UpdateAllNetworkAdapters()
        {
            this.OnAdcolonyAdapterChanged();
            this.OnApplovinAdapterChanged();
            this.OnBigoadsAdapterChanged();
            this.OnChartboostAdapterChanged();
            this.OnGoogleAdapterChanged();
            this.OnInmobiAdapterChanged();
            this.OnIronsourceAdapterChanged();
            this.OnMintegralAdapterChanged();
            this.OnMytargetAdapterChanged();
            this.OnPangleAdapterChanged();
            this.OnStartappAdapterChanged();
            this.OnTapjoyAdapterChanged();
            this.OnUnityadsAdapterChanged();
            this.OnVungleAdapterChanged();
        }

        #region Network Adapters

        [SerializeField, OnValueChanged("OnAdcolonyAdapterChanged"), FoldoutGroup("Network Adapters", -1)]
        private bool useAdcolonyAdapter;

        [SerializeField, OnValueChanged("OnApplovinAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useApplovinAdapter;

        [SerializeField, OnValueChanged("OnBigoadsAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useBigoadsAdapter;

        [SerializeField, OnValueChanged("OnChartboostAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useChartboostAdapter;

        [SerializeField, OnValueChanged("OnGoogleAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useGoogleAdapter;

        [SerializeField, OnValueChanged("OnInmobiAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useInmobiAdapter;

        [SerializeField, OnValueChanged("OnIronsourceAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useIronsourceAdapter;

        [SerializeField, OnValueChanged("OnMintegralAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useMintegralAdapter;

        [SerializeField, OnValueChanged("OnMytargetAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useMytargetAdapter;

        [SerializeField, OnValueChanged("OnPangleAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool usePangleAdapter;

        [SerializeField, OnValueChanged("OnStartappAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useStartappAdapter;

        [SerializeField, OnValueChanged("OnTapjoyAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useTapjoyAdapter;

        [SerializeField, OnValueChanged("OnUnityadsAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useUnityadsAdapter;

        [SerializeField, OnValueChanged("OnVungleAdapterChanged"), FoldoutGroup("Network Adapters")]
        private bool useVungleAdapter;

        private void OnAdcolonyAdapterChanged() => this.DownloadNetworkAdapter("adcolony", "MobileadsAdColonyMediationDependencies", this.useAdcolonyAdapter);

        private void OnApplovinAdapterChanged() => this.DownloadNetworkAdapter("applovin", "MobileadsAppLovinMediationDependencies", this.useApplovinAdapter);

        private void OnBigoadsAdapterChanged() => this.DownloadNetworkAdapter("bigoads", "MobileadsBigoAdsMediationDependencies", this.useBigoadsAdapter);

        private void OnChartboostAdapterChanged() => this.DownloadNetworkAdapter("chartboost", "MobileadsChartboostMediationDependencies", this.useChartboostAdapter);

        private void OnGoogleAdapterChanged() => this.DownloadNetworkAdapter("google", "MobileadsGoogleMediationDependencies", this.useGoogleAdapter);

        private void OnInmobiAdapterChanged() => this.DownloadNetworkAdapter("inmobi", "MobileadsInmobiMediationDependencies", this.useInmobiAdapter);

        private void OnIronsourceAdapterChanged() => this.DownloadNetworkAdapter("ironsource", "MobileadsIronSourceMediationDependencies", this.useIronsourceAdapter);

        private void OnMintegralAdapterChanged() => this.DownloadNetworkAdapter("mintegral", "MobileadsMintegralMediationDependencies", this.useMintegralAdapter);

        private void OnMytargetAdapterChanged() => this.DownloadNetworkAdapter("mytarget", "MobileadsMytargetMediationDependencies", this.useMytargetAdapter);

        private void OnPangleAdapterChanged() => this.DownloadNetworkAdapter("pangle", "MobileadsPangleMediationDependencies", this.usePangleAdapter);

        private void OnStartappAdapterChanged() => this.DownloadNetworkAdapter("startapp", "MobileadsStartappMediationDependencies", this.useStartappAdapter);

        private void OnTapjoyAdapterChanged() => this.DownloadNetworkAdapter("tapjoy", "MobileadsTapjoyMediationDependencies", this.useTapjoyAdapter);

        private void OnUnityadsAdapterChanged() => this.DownloadNetworkAdapter("unityads", "MobileadsUnityAdsMediationDependencies", this.useUnityadsAdapter);

        private void OnVungleAdapterChanged() => this.DownloadNetworkAdapter("vungle", "MobileadsVungleMediationDependencies", this.useVungleAdapter);

        #endregion

        #endregion
    }
}