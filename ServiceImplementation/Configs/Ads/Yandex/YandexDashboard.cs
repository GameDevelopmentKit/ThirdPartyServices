namespace ServiceImplementation.Configs.Ads.Yandex
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class YandexDashboard
    {
        public const string YandexSdkVersion = "7.2.0";

        [OnInspectorInit(nameof(UpdateVersionText)), HideLabel, DisplayAsString(TextAlignment.Center, true), PropertyOrder(-2), HorizontalGroup("YandexVersion")]
        private string CurrentVersion { get; set; }

        public void UpdateVersionText()
        {
            const string path     = "Assets/YandexMobileAds/Editor/YandexMobileadsDependencies.xml";
            var          versions = UnityPackageHelper.ParseXmlFileGetPackageVersion(path);
            this.CurrentVersion = $"current version: <color=yellow>{versions.androidVersion}</color> | latest version: <color=yellow>{YandexSdkVersion}</color>";
        }

        [HorizontalGroup("YandexVersion", width: 100), Button("UpdateSDK", ButtonSizes.Medium), ShowIf(nameof(NeedUpdateSdkVersion))]
        public async void DownloadSDK()
        {
            var sdkUrl = $"https://github.com/yandexmobile/yandex-ads-unity-plugin/releases/download/{YandexSdkVersion}/yandex-mobileads-lite-{YandexSdkVersion}.unitypackage";
            await UnityPackageHelper.DownloadThenImportPackage(sdkUrl, "YandexSDK");

            UnityPackageHelper.CopyFile("Assets/YandexMobileAds/YandexMobileAds.Scripts.asmdef", "Packages/com.gdk.3rd/ServiceImplementation/Configs/Ads/Yandex/YandexMobileAdsAsmDef.json");
            UnityPackageHelper.CopyFile("Assets/YandexMobileAds/Editor/YandexMobileAds.Scripts.asmdef",
                "Packages/com.gdk.3rd/ServiceImplementation/Configs/Ads/Yandex/YandexMobileAdsEditorAsmDef.json");

            this.UpdateAllNetworkAdapters();
            this.UpdateVersionText();
        }

        private bool NeedUpdateSdkVersion()
        {
            var versions = UnityPackageHelper.ParseXmlFileGetPackageVersion("Assets/YandexMobileAds/Editor/YandexMobileadsDependencies.xml");
            return versions.androidVersion != YandexSdkVersion;
        }

        private Dictionary<string, (bool, string)> AdapterInfo => new()
        {
            { "adcolony", (this.useAdcolonyAdapter, "MobileadsAdColonyMediationDependencies") },
            { "applovin", (this.useApplovinAdapter, "MobileadsAppLovinMediationDependencies") },
            { "bigoads", (this.useBigoadsAdapter, "MobileadsBigoAdsMediationDependencies") },
            { "chartboost", (this.useChartboostAdapter, "MobileadsChartboostMediationDependencies") },
            { "google", (this.useGoogleAdapter, "MobileadsGoogleMediationDependencies") },
            { "inmobi", (this.useInmobiAdapter, "MobileadsInmobiMediationDependencies") },
            { "ironsource", (this.useIronsourceAdapter, "MobileadsIronSourceMediationDependencies") },
            { "mintegral", (this.useMintegralAdapter, "MobileadsMintegralMediationDependencies") },
            { "mytarget", (this.useMytargetAdapter, "MobileadsMytargetMediationDependencies") },
            { "pangle", (this.usePangleAdapter, "MobileadsPangleMediationDependencies") },
            { "startapp", (this.useStartappAdapter, "MobileadsStartappMediationDependencies") },
            { "tapjoy", (this.useTapjoyAdapter, "MobileadsTapjoyMediationDependencies") },
            { "unityads", (this.useUnityadsAdapter, "MobileadsUnityAdsMediationDependencies") },
            { "vungle", (this.useVungleAdapter, "MobileadsVungleMediationDependencies") }
        };

        public void ResetCacheNetworkAdapters()
        {
            foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (field.FieldType == typeof(bool))
                {
                    field.SetValue(this, false);
                }
            }
        }

        public void UpdateAllNetworkAdapters()
        {
            foreach (var adapter in this.AdapterInfo)
            {
                this.DownloadNetworkAdapter(adapter.Key, adapter.Value.Item1, adapter.Value.Item2);
            }
        }

        private void DownloadNetworkAdapter(string name, bool isEnable, string xmlName)
        {
#if UNITY_EDITOR
            if (isEnable)
            {
                var url = $"https://github.com/yandexmobile/yandex-ads-unity-plugin/raw/master/mobileads-mediation/{name}/mobileads-{name}-mediation-{YandexSdkVersion}.unitypackage";
                UnityPackageHelper.DownloadThenImportPackage(url, name).Forget();
            }
            else
            {
                UnityPackageHelper.DeleteFileIfExists($"Assets/YandexMobileAds/Editor/{xmlName}.xml");
            }

#endif
        }

        #region Network Adapters

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters", centerLabel: true)]
        private bool useAdcolonyAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useApplovinAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useBigoadsAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useChartboostAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useGoogleAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useInmobiAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useIronsourceAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useMintegralAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useMytargetAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool usePangleAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useStartappAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useTapjoyAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useUnityadsAdapter;

        [SerializeField, OnValueChanged(nameof(UpdateAllNetworkAdapters)), BoxGroup("Network Adapters")]
        private bool useVungleAdapter;

        #endregion
    }
}