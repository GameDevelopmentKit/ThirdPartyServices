namespace Core.AnalyticServices
{
    using Sirenix.OdinInspector;
    using UnityEngine;
    #if UNITY_EDITOR
    using ServiceImplementation.Configs.Editor;
    #endif

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        private const string AdjustSymbol = "ADJUST";

        [BoxGroup("Adjust")] [LabelText("Enable", SdfIconType.Youtube)] [OnValueChanged("OnChangeAdjustEnabled")] [SerializeField] private bool isAdjustEnabled;

        private void OnChangeAdjustEnabled()
        {
            #if UNITY_EDITOR
            EditorUtils.SetDefineSymbol(AdjustSymbol, this.isAdjustEnabled);
            EditorUtils.ModifyPackage(this.isAdjustEnabled, "com.adjust.sdk", "https://github.com/The1Studio/adjust.git?path=Assets/Adjust#");
            #endif
        }

        [OnInspectorInit]
        private void InitAdjustSetting()
        {
            #if ADJUST && UNITY_EDITOR
            if (!string.IsNullOrEmpty(this.adjustAndroidAppToken) || !string.IsNullOrEmpty(this.adjustIOSAppToken))
            {
                this.isAdjustEnabled = true;
                return;
            }
            #endif
            this.OnChangeAdjustEnabled();
        }
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

        [SerializeField] private string adjustAndroidAppToken;
        [SerializeField] private string adjustIOSAppToken;
        [SerializeField] private string adjustAndroidPurchaseToken;
        [SerializeField] private string adjustIOSPurchaseToken;
        [SerializeField] private bool   adjustIsDebug;
        #endif
    }
}