namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;
#if UNITY_EDITOR
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
        /// <summary>
        /// Gets the AdMob settings.
        /// </summary>
        public AdMobSettings AdMob { get { return this.mAdMob; } }

        /// <summary>
        /// Gets the AppLovin settings.
        /// </summary>
        public AppLovinSettings AppLovin { get { return this.mAppLovin; } }

        /// <summary>
        /// Gets the IronSource settings.
        /// </summary>
        public IronSourceSettings IronSource { get { return this.mIronSource; } }

        #region Misc

        public bool EnableBreakAds { get { return this.enableBreakAds; } }

        public bool CollapsibleRefreshOnScreenShow => this.mCollapsibleRefreshOnScreenShow;
        
        public List<string> CollapsibleIgnoreRefreshOnScreens => this.mCollapsibleIgnoreRefreshOnScreens;

        public BannerLoadStrategy BannerLoadStrategy { get { return this.bannerLoadStrategy; } }

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
        private BannerLoadStrategy bannerLoadStrategy = BannerLoadStrategy.Instantiate;
        
        [SerializeField] [LabelText("Custom Interstitial capping time")] [FoldoutGroup("Misc")]
        public Dictionary_AdPlacement_CappingTime CustomInterstitialCappingTime;

        [SerializeField] [LabelText("Enable")] [OnValueChanged("OnChangeCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")]
        private bool mEnableCollapsibleBanner;
        
        [SerializeField] [LabelText("Auto Refresh")] [Tooltip("Collapsible Banner will auto refresh on Screen Show")] [ShowIf("mEnableCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")]
        private bool mCollapsibleRefreshOnScreenShow = true;

        [SerializeField] [LabelText("Screens Ignore Auto Refresh")] [Tooltip("Collapsible Banner will ignore auto refresh on screens")] [ShowIf("mEnableCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")]
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

#if UNITY_EDITOR

        private const string AdModSymbol             = "ADMOB";
        private const string AppLovinSymbol          = "APPLOVIN";
        private const string IronSourceSymbol        = "IRONSOURCE";
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
                DeleteFolderIfExists("Assets/MaxSdk");
            }
        }

        private void OnChangeIronSource()
        {
            EditorUtils.SetDefineSymbol(IronSourceSymbol, this.enableIronSource);
            EditorUtils.ModifyPackage(this.enableIronSource, "com.unity.services.levelplay", "8.1.0");
            if (!this.enableIronSource)
            {
                DeleteFolderIfExists("Assets/LevelPlay");
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
#endif
    }
}