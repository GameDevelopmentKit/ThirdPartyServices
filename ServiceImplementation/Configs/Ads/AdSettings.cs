namespace ServiceImplementation.Configs.Ads
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Serialization;
#if UNITY_EDITOR
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

        public bool EnableBreakAds { get { return this.enableBreakAds; } }

        public bool EnableUmp { get { return this.enableUmp; } }
        
        public bool EnableCollapsibleAds => this.mEnableCollapsibleBanner;

        public BannerLoadStrategy BannerLoadStrategy { get { return this.bannerLoadStrategy; } }

        /// <summary>
        /// AOA threshold
        /// </summary>
        public float AOAThreshHold { get { return this.mAOAThreshHold; } }

        [SerializeField] [LabelText("AOA ThreshHold", SdfIconType.Download)] [FoldoutGroup("Misc")]
        private float mAOAThreshHold = 5f;

        [SerializeField] [LabelText("Break Ads Screen", SdfIconType.CupStraw)] [FoldoutGroup("Misc")]
        private bool enableBreakAds;

        [SerializeField] [LabelText("Enable UMP", SdfIconType.QuestionDiamondFill)] [FoldoutGroup("Misc")]
        private bool enableUmp;

        [SerializeField] [LabelText("Banner Load Strategy", SdfIconType.BookmarkFill)] [FoldoutGroup("Misc")]
        private BannerLoadStrategy bannerLoadStrategy = BannerLoadStrategy.Instantiate;

        [FormerlySerializedAs("mEnableEnableCollapsibleBanner")]
        [SerializeField] [LabelText("Enable")] [OnValueChanged("OnChangeCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")]
        private bool mEnableCollapsibleBanner;
        
        [SerializeField] [LabelText("Auto Refresh")] [Tooltip("Collapsible Banner will auto refresh on Screen Show")] [ShowIf("mEnableCollapsibleBanner")] [FoldoutGroup("Misc/Collapsible Banner")]
        private bool mCollapsibleRefreshOnScreenShow = true;

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

        private void OnChangeAdMob() { DefineSymbolEditorUtils.SetDefineSymbol(AdModSymbol, this.enableAdMob); }

        private void OnChangeAppLovin() { DefineSymbolEditorUtils.SetDefineSymbol(AppLovinSymbol, this.enableAppLovin); }

        private void OnChangeIronSource() { DefineSymbolEditorUtils.SetDefineSymbol(IronSourceSymbol, this.enableIronSource); }
        
        private void OnChangeCollapsibleBanner() { DefineSymbolEditorUtils.SetDefineSymbol(CollapsibleBannerSymbol, this.mEnableCollapsibleBanner); }
#endif
    }
}