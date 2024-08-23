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
        Manually
    }

    [Serializable]
    public class AdSettings
    {
        public AdMobSettings      AdMob      => this.mAdMob;
        public AppLovinSettings   AppLovin   => this.mAppLovin;
        public IronSourceSettings IronSource => this.mIronSource;
        public YandexSettings     Yandex     => this.mYandex;

        #region Misc

        public bool EnableBreakAds { get { return this.enableBreakAds; } }

        public bool CollapsibleRefreshOnScreenShow => this.mCollapsibleRefreshOnScreenShow;

        public List<string> CollapsibleIgnoreRefreshOnScreens => this.mCollapsibleIgnoreRefreshOnScreens;

        public BannerLoadStrategy BannerLoadStrategy { get { return this.bannerLoadStrategy; } }

        #region ATT (iOS only)

        [FoldoutGroup("Misc/ATT (iOS only)")] [Tooltip("Auto Request App Tracking Transparent for iOS")]
        public bool autoRequestATT = true;

        [FoldoutGroup("Misc/ATT (iOS only)")] [Tooltip("Custom App Tracking Transparent for iOS")]
        public bool customAtt;

        [FoldoutGroup("Misc/ATT (iOS only)/Custom")] [Sirenix.OdinInspector.FilePath(Extensions = "unity")] [ShowIf(nameof(customAtt))]
        public string attScenePath = "Assets/Scenes/ATTScene.unity";

        #endregion

        #endregion

        /// <summary>
        /// AOA threshold
        /// </summary>
        public float AOAThreshHold { get { return this.mAOAThreshHold; } }

        public BannerAdsPosition BannerPosition => this.mBannerPosition;

        [SerializeField] [FoldoutGroup("Misc")] [LabelText("Banner Position", SdfIconType.BookmarkFill)]
        private BannerAdsPosition mBannerPosition = BannerAdsPosition.Bottom;

        [SerializeField] [LabelText("AOA ThreshHold", SdfIconType.Download)] [FoldoutGroup("Misc")]
        private float mAOAThreshHold = 5f;

        [SerializeField] [LabelText("Break Ads Screen", SdfIconType.CupStraw)] [FoldoutGroup("Misc")]
        private bool enableBreakAds;

        [SerializeField] [LabelText("Banner Load Strategy", SdfIconType.BookmarkFill)] [FoldoutGroup("Misc")]
        private BannerLoadStrategy bannerLoadStrategy = BannerLoadStrategy.AfterLoading;

        [SerializeField] [LabelText("Custom Interstitial capping time")] [FoldoutGroup("Misc")]
        public Dictionary_AdPlacement_CappingTime CustomInterstitialCappingTime;

        [SerializeField] [LabelText("Enable")] [OnValueChanged("OnChangeCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")]
        private bool mEnableCollapsibleBanner;

        [SerializeField] [LabelText("Auto Refresh")] [Tooltip("Collapsible Banner will auto refresh on Screen Show")] [ShowIf("mEnableCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")]
        private bool mCollapsibleRefreshOnScreenShow = true;

        [SerializeField]
        [LabelText("Screens Ignore Auto Refresh")]
        [Tooltip("Collapsible Banner will ignore auto refresh on screens")]
        [ShowIf("mEnableCollapsibleBanner")]
        [FoldoutGroup("Misc/Collapsible Banner")]
        private List<string> mCollapsibleIgnoreRefreshOnScreens = new List<string>();

        [SerializeField] [LabelText("AdMob", SdfIconType.Youtube)] [OnValueChanged("OnChangeAdMob")]
        private bool enableAdMob;

        [SerializeField] [ShowIf("enableAdMob")] [HideLabel] [FoldoutGroup("AdMob")]
        private AdMobSettings mAdMob = null;

        [SerializeField] [LabelText("AppLovin", SdfIconType.Youtube)] [OnValueChanged("OnChangeAppLovin")]
        private bool enableAppLovin;

        [SerializeField] [ShowIf("enableAppLovin")] [HideLabel] [FoldoutGroup("AppLovin")]
        private AppLovinSettings mAppLovin = null;

        [SerializeField] [LabelText("IronSource", SdfIconType.Youtube)] [OnValueChanged("OnChangeIronSource")]
        private bool enableIronSource;

        [SerializeField] [ShowIf("enableIronSource")] [HideLabel] [FoldoutGroup("IronSource")]
        private IronSourceSettings mIronSource = null;

        [SerializeField] [LabelText("Yandex", SdfIconType.Youtube)] [OnValueChanged("OnChangeYandex")]
        private bool enableYandex;

        [SerializeField] [ShowIf("enableYandex")] [HideLabel] [FoldoutGroup("Yandex")]
        private YandexSettings mYandex = null;

#if UNITY_EDITOR

        private const string AdModSymbol             = "ADMOB";
        private const string AppLovinSymbol          = "APPLOVIN";
        private const string IronSourceSymbol        = "IRONSOURCE";
        private const string YandexSymbol            = "YANDEX";
        private const string CollapsibleBannerSymbol = "THEONE_COLLAPSIBLE_BANNER";

        private void OnChangeAdMob()
        {
            EditorUtils.SetDefineSymbol(AdModSymbol, this.enableAdMob);
            EditorUtils.ModifyPackage(this.enableAdMob, "com.google.ads.mobile", "9.1.0");
        }

        private void OnChangeAppLovin()
        {
            EditorUtils.SetDefineSymbol(AppLovinSymbol, this.enableAppLovin);
            if (this.enableAppLovin)
            {
                AppLovinSettings.DownloadApplovin();
            }
            else
            {
                UnityPackageHelper.DeleteFolderIfExists("Assets/MaxSdk");
            }
        }

        private void OnChangeIronSource()
        {
            EditorUtils.SetDefineSymbol(IronSourceSymbol, this.enableIronSource);
            EditorUtils.ModifyPackage(this.enableIronSource, "com.unity.services.levelplay", "8.1.0");
            if (!this.enableIronSource)
            {
                UnityPackageHelper.DeleteFolderIfExists("Assets/LevelPlay");
            }
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

        private void OnChangeCollapsibleBanner() { EditorUtils.SetDefineSymbol(CollapsibleBannerSymbol, this.mEnableCollapsibleBanner); }


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
        
        [FoldoutGroup("Misc/ATT (iOS only)/Custom")] [Button] [ShowIf(nameof(customAtt))]
        private void SetupCustomAtt()
        {
            if (string.IsNullOrEmpty(this.attScenePath) || !File.Exists(this.attScenePath))
            {
                EditorWindow.focusedWindow.ShowNotification(new GUIContent("ATT Scene Path is not valid!"));
                return;
            }

            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(x => x.path == this.attScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(this.attScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorWindow.focusedWindow.ShowNotification(new GUIContent("Setup ATT Scene Path successfully"));
        }

#endif
    }
}