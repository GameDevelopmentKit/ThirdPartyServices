namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.AdsServices;
    using ServiceImplementation.Configs.Ads.Yandex;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;
    #if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
    using ServiceImplementation.Configs.Editor;
    #endif

    public enum BannerLoadStrategy
    {
        Instantiate,
        AfterLoading,
        Manually,
    }

    [Serializable]
    public class AdSettings
    {
        public AdMobSettings       AdMob        => this.mAdMob;
        public AppLovinSettings    AppLovin     => this.mAppLovin;
        public IronSourceSettings  IronSource   => this.mIronSource;
        public YandexSettings      Yandex       => this.mYandex;
        public ImmersiveAdsSetting ImmersiveAds => this.mImmersiveAds;

        #region Misc

        public bool   EnableBreakAds               => this.enableBreakAds;
        public float  TimeDelayShowInterBreakAds   => this.timeDelayShowInterBreakAds;
        public float  TimeDelayCloseBreakAdsPopup  => this.timeDelayCloseBreakAdsPopup;
        public bool   EnableMrecForBreakAds        => this.enableMrecForBreakAds;
        public string BreakAdsMrecPlacement        => this.breakAdsMrecPlacement;
        public bool   IsBreakAdsRewardCurrency     => this.isBreakAdsRewardCurrency;
        public string BreakAdsRewardCurrency       => this.breakAdsRewardCurrency;
        public int    BreakAdsRewardCurrencyAmount => this.breakAdsRewardCurrencyAmount;

        public string IntersInsteadAoaResumePlacement => this.mIntersInsteadAoaResumePlacement;

        public bool CollapsibleRefreshOnScreenShow => this.mCollapsibleRefreshOnScreenShow;

        public List<string> CollapsibleIgnoreRefreshOnScreens => this.mCollapsibleIgnoreRefreshOnScreens;

        public BannerLoadStrategy BannerLoadStrategy => this.bannerLoadStrategy;

        #region ATT (iOS only)

        [FoldoutGroup("Misc/Consent")] [Tooltip("Custom App Tracking Transparent for iOS")] [LabelText("Custom Att Scene")] public bool customAtt;

        [FoldoutGroup("Misc/Consent")] [Sirenix.OdinInspector.FilePath(Extensions = "unity")] [ShowIf(nameof(customAtt))] public string attScenePath = "Assets/Scenes/ATTScene.unity";

        #endregion

        #endregion

        public BannerAdsPosition BannerPosition => this.mBannerPosition;

        [SerializeField] [FoldoutGroup("Misc")] [LabelText("Banner Position", SdfIconType.BookmarkFill)] private BannerAdsPosition mBannerPosition = BannerAdsPosition.Bottom;

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [LabelText("Break Ads Screen", SdfIconType.CupStraw)] private bool enableBreakAds;

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [ShowIf(nameof(enableBreakAds))] [LabelText("Delay Show Interstitial")] private float timeDelayShowInterBreakAds  = 0.5f;

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [ShowIf(nameof(enableBreakAds))] [LabelText("Delay Close Popup")] private float timeDelayCloseBreakAdsPopup = 2f;

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [ShowIf(nameof(enableBreakAds))] [LabelText("Enable Mrec For Break Ads")] private bool enableMrecForBreakAds = false;

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [ShowIf(nameof(enableMrecForBreakAds))] [LabelText("Mrec Placement")] private string breakAdsMrecPlacement = "bot";

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [ShowIf(nameof(enableBreakAds))] [LabelText("Is Reward Currency")] private bool isBreakAdsRewardCurrency;

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [ShowIf(nameof(isBreakAdsRewardCurrency))] [LabelText("Currency")] private string breakAdsRewardCurrency = "Coin";

        [SerializeField] [FoldoutGroup("Misc/Break Ads")] [ShowIf(nameof(isBreakAdsRewardCurrency))] [LabelText("Amount")] private int breakAdsRewardCurrencyAmount = 1;

        [SerializeField] [LabelText("Inter AOA Placement")] [FoldoutGroup("Misc")] private string mIntersInsteadAoaResumePlacement = "inter_aoa";

        [SerializeField] [LabelText("Banner Load Strategy", SdfIconType.BookmarkFill)] [FoldoutGroup("Misc")] private BannerLoadStrategy bannerLoadStrategy = BannerLoadStrategy.AfterLoading;

        [SerializeField] [LabelText("Custom Interstitial capping time")] [FoldoutGroup("Misc")] public Dictionary_AdPlacement_CappingTime CustomInterstitialCappingTime;

        [SerializeField] [LabelText("Enable")] [OnValueChanged("OnChangeCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")] private bool mEnableCollapsibleBanner;

        [SerializeField]
        [LabelText("Auto Refresh")]
        [Tooltip("Collapsible Banner will auto refresh on Screen Show")]
        [ShowIf("mEnableCollapsibleBanner")]
        [FoldoutGroup("Misc/Collapsible Banner")]
        private bool mCollapsibleRefreshOnScreenShow = true;

        [SerializeField]
        [LabelText("Screens Ignore Auto Refresh")]
        [Tooltip("Collapsible Banner will ignore auto refresh on screens")]
        [ShowIf("mEnableCollapsibleBanner")]
        [FoldoutGroup("Misc/Collapsible Banner")]
        private List<string> mCollapsibleIgnoreRefreshOnScreens = new();

        [SerializeField] [LabelText("AdMob", SdfIconType.Youtube)] [OnValueChanged("OnChangeAdMob")] private bool enableAdMob;

        [SerializeField] [ShowIf("enableAdMob")] [HideLabel] [FoldoutGroup("AdMob")] private AdMobSettings mAdMob = null;

        [SerializeField] [LabelText("AppLovin", SdfIconType.Youtube)] [OnValueChanged("OnChangeAppLovin")] private bool enableAppLovin;

        [SerializeField] [ShowIf("enableAppLovin")] [HideLabel] [FoldoutGroup("AppLovin")] private AppLovinSettings mAppLovin = null;

        [SerializeField] [LabelText("IronSource", SdfIconType.Youtube)] [OnValueChanged("OnChangeIronSource")] private bool enableIronSource;

        [SerializeField] [ShowIf("enableIronSource")] [HideLabel] [FoldoutGroup("IronSource")] private IronSourceSettings mIronSource = null;

        [SerializeField] [LabelText("Yandex", SdfIconType.Youtube)] [OnValueChanged("OnChangeYandex")] private bool enableYandex;

        [SerializeField] [ShowIf("enableYandex")] [HideLabel] [FoldoutGroup("Yandex")] private YandexSettings mYandex = null;

        [SerializeField] [LabelText("ImmersiveAds", SdfIconType.Youtube)] [OnValueChanged("OnChangeImmersiveAds")] private bool enableImmersiveAds;

        [SerializeField] [ShowIf("enableImmersiveAds")] [HideLabel] [FoldoutGroup("ImmersiveAds")] private ImmersiveAdsSetting mImmersiveAds;

        #if UNITY_EDITOR

        private const string AdModSymbol             = "ADMOB";
        private const string AppLovinSymbol          = "APPLOVIN";
        private const string IronSourceSymbol        = "IRONSOURCE";
        private const string YandexSymbol            = "YANDEX";
        private const string CollapsibleBannerSymbol = "THEONE_COLLAPSIBLE_BANNER";
        private const string ImmersiveAdsSymbol      = "IMMERSIVE_ADS";

        private void OnChangeAdMob()
        {
            EditorUtils.SetDefineSymbol(AdModSymbol, this.enableAdMob);
            EditorUtils.ModifyPackage(this.enableAdMob, "com.google.ads.mobile", "9.1.0");
        }

        private void OnChangeAppLovin()
        {
            EditorUtils.SetDefineSymbol(AppLovinSymbol, this.enableAppLovin);
            if (this.enableAppLovin)
                EditorUtils.ModifyPackage(this.enableAppLovin, "com.applovin.mediation.ads", "8.0.1");
            else
            {
                EditorUtils.ModifyPackage(this.enableAppLovin, "com.applovin.mediation.ads", "8.0.1");
                UnityPackageHelper.DeleteFolderIfExists("Assets/MaxSdk");
            }
        }

        private void OnChangeIronSource()
        {
            EditorUtils.SetDefineSymbol(IronSourceSymbol, this.enableIronSource);
            EditorUtils.ModifyPackage(this.enableIronSource, "com.unity.services.levelplay", "8.1.0");
            if (!this.enableIronSource) UnityPackageHelper.DeleteFolderIfExists("Assets/LevelPlay");
        }

        private void OnChangeYandex()
        {
            EditorUtils.SetDefineSymbol(YandexSymbol, this.enableYandex);
            if (this.enableYandex)
            {
                this.mYandex.Dashboard.ResetCacheNetworkAdapters();
                this.mYandex.Dashboard.DownloadSDK();
            }
            else
            {
                UnityPackageHelper.DeleteFolderIfExists("Assets/YandexMobileAds");
            }
        }

        private void OnChangeCollapsibleBanner()
        {
            EditorUtils.SetDefineSymbol(CollapsibleBannerSymbol, this.mEnableCollapsibleBanner);
        }

        private void OnChangeImmersiveAds()
        {
            EditorUtils.SetDefineSymbol(ImmersiveAdsSymbol, this.enableImmersiveAds);
        }

        private static bool DeleteFolderIfExists(string folderPath)
        {
            // Check if the folder exists
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                // Delete the folder
                AssetDatabase.DeleteAsset(folderPath);

                Debug.Log($"Folder '{folderPath}' has been deleted.");

                return true;
            }

            return false;
        }

        [FoldoutGroup("Misc/Consent")]
        [Button]
        [ShowIf(nameof(customAtt))]
        private void SetupCustomAtt()
        {
            if (string.IsNullOrEmpty(this.attScenePath) || !File.Exists(this.attScenePath))
            {
                EditorWindow.focusedWindow.ShowNotification(new("ATT Scene Path is not valid!"));
                return;
            }

            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(x => x.path == this.attScenePath);
            scenes.Insert(0, new(this.attScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorWindow.focusedWindow.ShowNotification(new("Setup ATT Scene Path successfully"));
        }

        #endif
    }
}