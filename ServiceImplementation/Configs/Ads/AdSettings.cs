namespace ServiceImplementation.Configs.Ads
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

#if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
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

        [SerializeField] [LabelText("AdMob", SdfIconType.Youtube)] [OnValueChanged("OnChangeAdMob")]
        private bool enableAdMob;

        [SerializeField] [ShowIf("enableAdMob")] [HideLabel] [BoxGroup("AdMob")]
        private AdMobSettings mAdMob = null;

        [SerializeField] [LabelText("ApLovin", SdfIconType.Youtube)] [OnValueChanged("OnChangeAppLovin")]
        private bool enableAppLovin;

        [SerializeField] [ShowIf("enableAppLovin")] [HideLabel] [BoxGroup("AppLovin")]
        private AppLovinSettings mAppLovin = null;

        [SerializeField] [LabelText("IronSource", SdfIconType.Youtube)] [OnValueChanged("OnChangeIronSource")]
        private bool enableIronSource;

        [SerializeField] [ShowIf("enableIronSource")] [HideLabel] [BoxGroup("IronSource")]
        private IronSourceSettings mIronSource = null;

#if UNITY_EDITOR

        private const string Delimiter         = ";";
        private const string AdvertisingSymbol = "ADVERTISNG_ENABLE";
        private const string AdModSymbol       = "ADVERTISNG_ADMOB";
        private const string AppLovinSymbol    = "ADVERTISNG_APPLOVIN";
        private const string IronSourceSymbol  = "ADVERTISNG_IRONSOURCE";

        private bool EnableAdvertising => this.enableAdMob || this.enableAppLovin || this.enableIronSource;

        private void OnChangeAdMob()
        {
            this.SetDefineSymbol(AdvertisingSymbol, this.EnableAdvertising);
            this.SetDefineSymbol(AdModSymbol, this.enableAdMob);
        }

        private void OnChangeAppLovin()
        {
            this.SetDefineSymbol(AdvertisingSymbol, this.EnableAdvertising);
            this.SetDefineSymbol(AppLovinSymbol, this.enableAppLovin);
        }

        private void OnChangeIronSource()
        {
            this.SetDefineSymbol(AdvertisingSymbol, this.EnableAdvertising);
            this.SetDefineSymbol(IronSourceSymbol, this.enableIronSource);
        }

        private void SetDefineSymbol(string symbol, bool isAdd)
        {
            this.SetBuildTargetDefineSymbol(NamedBuildTarget.Android, symbol, isAdd);
            this.SetBuildTargetDefineSymbol(NamedBuildTarget.iOS, symbol, isAdd);
            this.SetBuildTargetDefineSymbol(NamedBuildTarget.WebGL, symbol, isAdd);
            this.SetBuildTargetDefineSymbol(NamedBuildTarget.Standalone, symbol, isAdd);
            this.SetBuildTargetDefineSymbol(NamedBuildTarget.Server, symbol, isAdd);
        }

        private void SetBuildTargetDefineSymbol(NamedBuildTarget buildTarget, string symbol, bool isAdd)
        {
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget).Split(Delimiter).ToList();
            if (isAdd)
            {
                if (defineSymbols.Contains(symbol)) return;
                defineSymbols.Add(symbol);
            }
            else
            {
                if (!defineSymbols.Contains(symbol)) return;
                defineSymbols.Remove(symbol);
            }

            PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(Delimiter, defineSymbols));
        }
#endif
    }
}