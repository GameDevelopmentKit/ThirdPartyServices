namespace Core.AnalyticServices
{
#if UNITY_EDITOR
    using ServiceImplementation.Configs.Editor;
#endif
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        private const string AppsflyerSymbol = "APPSFLYER";

        [BoxGroup("Appsflyer")] [LabelText("Enable", SdfIconType.Youtube)] [OnValueChanged("OnChangeAppsflyerEnabled")] [SerializeField]
        private bool isAppsflyerEnabled;

        private void OnChangeAppsflyerEnabled()
        {
#if UNITY_EDITOR
            EditorUtils.SetDefineSymbol(AppsflyerSymbol, this.isAppsflyerEnabled);
            if (this.autoImportPackages) EditorUtils.ModifyPackage(this.isAppsflyerEnabled, "com.theone.appsflyer-unity-plugin", "https://github.com/The1Studio/appsflyer.git?path=Assets/AppsFlyer#");
#endif
        }

        [OnInspectorInit]
        private void InitAppsflyerSetting()
        {
#if APPSFLYER && UNITY_EDITOR
            if (!string.IsNullOrEmpty(this.appsflyerDevKeyAndroid) || !string.IsNullOrEmpty(this.appsflyerDevKeyIos))
            {
                this.isAppsflyerEnabled = true;
                return;
            }
#endif
            this.OnChangeAppsflyerEnabled();
        }
#if APPSFLYER
        /// <summary>
        /// 
        /// </summary>
        public string AppsflyerDevKey
        {
            get
            {
#if UNITY_ANDROID
                return this.appsflyerDevKeyAndroid;
#elif UNITY_IOS
                    return this.appsflyerDevKeyIos;
#elif UNITY_WSA_10_0
                    return this.appsflyerDevKeyUwp;
#endif
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string AppsflyerAppId
#if UNITY_IOS
            => this.appsflyerAppIdIos;
#elif UNITY_ANDROID
            => Application.identifier;
#elif UNITY_WSA_10_0 && !UNITY_EDITOR
            => this.appsflyerAppIdUWP;
#else
            => string.Empty;
#endif

        public bool AppsflyerIsDebug           => this.appsflyerIsDebug;

        [Header("DevKey")][BoxGroup("Appsflyer")]
        [SerializeField] private string appsflyerDevKeyIos;
        [BoxGroup("Appsflyer")]
        [SerializeField] private string appsflyerDevKeyAndroid;

        [Header("App Id")]
        [BoxGroup("Appsflyer")]
        [ValidateInput("ValidateAppIdIos", "Appsflyer App Id must start with 'id'", InfoMessageType.Error)]
        [SerializeField] private string appsflyerAppIdIos;
        [BoxGroup("Appsflyer")]
        [SerializeField] private bool appsflyerIsDebug;

        private bool ValidateAppIdIos(string value) { return string.IsNullOrEmpty(value) || this.appsflyerAppIdIos.StartsWith("id"); }
#endif
    }
}