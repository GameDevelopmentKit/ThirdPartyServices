namespace Core.AnalyticServices
{
    using ServiceImplementation.Configs.Editor;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        private const string AdjustSymbol = "ADJUST";

        [BoxGroup("Adjust")] [LabelText("Enable", SdfIconType.Youtube)] [OnValueChanged("OnChangeAdjustEnabled")] [SerializeField]
        private bool isAdjustEnabled;

        private void OnChangeAdjustEnabled() { DefineSymbolEditorUtils.SetDefineSymbol(AdjustSymbol, this.isAdjustEnabled); }
#if ADJUST
#if UNITY_ANDROID
        public string AdjustAppToken      => this.adjustAndroidAppToken;
        public string AdjustPurchaseToken => this.adjustAndroidPurchaseToken;
#elif UNITY_IOS
        public string AdjustAppToken      => this.adjustIOSAppToken;
        public string AdjustPurchaseToken => this.adjustIOSPurchaseToken;
#else
        public string AdjustAppToken      => this.adjustAndroidAppToken;
        public string AdjustPurchaseToken => this.adjustAndroidPurchaseToken;
#endif
        public bool   AdjustIsDebug       => this.adjustIsDebug;

        [SerializeField] private string adjustAndroidAppToken;
        [SerializeField] private string adjustIOSAppToken;
        [SerializeField] private string adjustAndroidPurchaseToken;
        [SerializeField] private string adjustIOSPurchaseToken;
        [SerializeField] private bool   adjustIsDebug;
#endif
    }
}