namespace ServiceImplementation.Configs.Ads
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;
#if UNITY_EDITOR
    using ServiceImplementation.Configs.Editor;
#endif

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

        /// <summary>
        /// AOA threshold
        /// </summary>
        public float AOAThreshHold { get { return this.mAOAThreshHold; } }

        [SerializeField] [LabelText("AOA ThreshHold", SdfIconType.Download)]
        private float mAOAThreshHold = 5f;

        [SerializeField] [LabelText("AdMob", SdfIconType.Youtube)] [OnValueChanged("OnChangeAdMob")]
        private bool enableAdMob;

        [SerializeField] [ShowIf("enableAdMob")] [HideLabel] [BoxGroup("AdMob")]
        private AdMobSettings mAdMob = null;

        [SerializeField] [LabelText("AppLovin", SdfIconType.Youtube)] [OnValueChanged("OnChangeAppLovin")]
        private bool enableAppLovin;

        [SerializeField] [ShowIf("enableAppLovin")] [HideLabel] [BoxGroup("AppLovin")]
        private AppLovinSettings mAppLovin = null;

        [SerializeField] [LabelText("IronSource", SdfIconType.Youtube)] [OnValueChanged("OnChangeIronSource")]
        private bool enableIronSource;

        [SerializeField] [ShowIf("enableIronSource")] [HideLabel] [BoxGroup("IronSource")]
        private IronSourceSettings mIronSource = null;

        [SerializeField] [LabelText("Break Ads Screen", SdfIconType.CupStraw)]
        private bool enableBreakAds;

        [SerializeField] [LabelText("Enable UMP", SdfIconType.QuestionDiamondFill)]
        private bool enableUmp;

#if UNITY_EDITOR

        private const string AdModSymbol      = "ADMOB";
        private const string AppLovinSymbol   = "APPLOVIN";
        private const string IronSourceSymbol = "IRONSOURCE";

        private void OnChangeAdMob() { DefineSymbolEditorUtils.SetDefineSymbol(AdModSymbol, this.enableAdMob); }

        private void OnChangeAppLovin() { DefineSymbolEditorUtils.SetDefineSymbol(AppLovinSymbol, this.enableAppLovin); }

        private void OnChangeIronSource() { DefineSymbolEditorUtils.SetDefineSymbol(IronSourceSymbol, this.enableIronSource); }
#endif
    }
}