namespace Core.AnalyticServices
{
    using Sirenix.OdinInspector;
    using UnityEngine;
#if UNITY_EDITOR
    using System.IO;
    using ServiceImplementation.Configs.Editor;
    using UnityEditor;
#endif

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        private const string AppsflyerSymbol        = "APPSFLYER";
        private       string AppsflyerPackageGitURL = "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm";

        [BoxGroup("Appsflyer")] [SerializeField]
        private bool isStrickMode;

        [BoxGroup("Appsflyer")] [LabelText("Enable", SdfIconType.Youtube)] [OnValueChanged("OnChangeAppsflyerEnabled")] [SerializeField]
        private bool isAppsflyerEnabled;

        [BoxGroup("Appsflyer")] [LabelText("Purchase Connector")] [SerializeField]
        private bool isIapPurchaseSdkEnabled;

        [BoxGroup("Appsflyer")] [SerializeField] [ShowIf("isIapPurchaseSdkEnabled")]
        private string purhaseSdkVersion = "2.1.11";

        [BoxGroup("Appsflyer")] [LabelText("DownLoadSdk")] [ShowIf("isIapPurchaseSdkEnabled")] [OnValueChanged("OnChangeAppsflyerPurchaseSdkEnabled")] [SerializeField]
        private bool isDownloadPurchaseSdk;

        private async void OnChangeAppsflyerPurchaseSdkEnabled()
        {
#if UNITY_EDITOR
            var url =
                $"https://github.com/AppsFlyerSDK/appsflyer-unity-purchase-connector/raw/refs/heads/master/strict-mode/appsflyer-unity-purchase-connector-strict-mode-{this.purhaseSdkVersion}.unitypackage";

            if (!this.isStrickMode)
            {
                url = $"https://github.com/AppsFlyerSDK/appsflyer-unity-purchase-connector/raw/refs/heads/master/appsflyer-unity-purchase-connector-${this.purhaseSdkVersion}.unitypackage";
            }

            var appsFlyerPath = Path.Combine(Application.dataPath, "AppsFlyer");

            if (this.isDownloadPurchaseSdk)
            {
                if (!Directory.Exists(appsFlyerPath))
                {
                    await AppsflyerHelper.DownLoadPurchaseConnector(url, this.purhaseSdkVersion);

                    var dependencymanager = Path.Combine(Application.dataPath, "ExternalDependencyManager");

                    AppsflyerHelper.DeleteFolderWithMeta(dependencymanager);

                    AppsflyerHelper.SaveAssemblyDefinitionFile();
                }
            }
            else
            {
                if (Directory.Exists(appsFlyerPath))
                {
                    AppsflyerHelper.DeleteFolderWithMeta(appsFlyerPath);
                }

                var dependencymanager = Path.Combine(Application.dataPath, "ExternalDependencyManager");

                AppsflyerHelper.DeleteFolderWithMeta(dependencymanager);
            }

            AssetDatabase.Refresh();

#endif
        }

        private void OnChangeAppsflyerEnabled()
        {
#if UNITY_EDITOR
            if (this.isStrickMode)
            {
                this.AppsflyerPackageGitURL = "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#Strict-upm";
            }

            EditorUtils.ModifyPackage(this.isByteBrewEnabled, "appsflyer-unity-plugin", AppsflyerPackageGitURL);
            EditorUtils.SetDefineSymbol(AppsflyerSymbol, this.isAppsflyerEnabled);
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

        [Header("DevKey")] [BoxGroup("Appsflyer")] [SerializeField]
        private string appsflyerDevKeyIos;

        [BoxGroup("Appsflyer")] [SerializeField]
        private string appsflyerDevKeyAndroid;

        [Header("App Id")] [BoxGroup("Appsflyer")] [ValidateInput("ValidateAppIdIos", "Appsflyer App Id must start with 'id'", InfoMessageType.Error)] [SerializeField]
        private string appsflyerAppIdIos;

        private bool ValidateAppIdIos(string value) { return string.IsNullOrEmpty(value) || this.appsflyerAppIdIos.StartsWith("id"); }
#endif
    }
}