namespace ServiceImplementation.Configs.Ads.Yandex
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using UnityEngine;

    public struct YandexAdNetwork
    {
        public const string Adcolony   = "adcolony";
        public const string Applovin   = "applovin";
        public const string Bigoads    = "bigoads";
        public const string Chartboost = "chartboost";
        public const string Google     = "google";
        public const string Inmobi     = "inmobi";
        public const string Ironsource = "ironsource";
        public const string Mintegral  = "mintegral";
        public const string Mytarget   = "mytarget";
        public const string Pangle     = "pangle";
        public const string Startapp   = "startapp";
        public const string Tapjoy     = "tapjoy";
        public const string Unityads   = "unityads";
        public const string Vungle     = "vungle";
    }

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

            this.AdapterInfo.ForEach(pair => this.DownloadNetworkAdapter(pair.Key));
            this.UpdateVersionText();
        }

        private bool NeedUpdateSdkVersion()
        {
            var versions = UnityPackageHelper.ParseXmlFileGetPackageVersion("Assets/YandexMobileAds/Editor/YandexMobileadsDependencies.xml");
            return versions.androidVersion != YandexSdkVersion;
        }

        private Dictionary<string, (bool, string)> AdapterInfo => new()
        {
            { YandexAdNetwork.Adcolony, (this.useAdcolonyAdapter, "MobileadsAdColonyMediationDependencies") },
            { YandexAdNetwork.Applovin, (this.useApplovinAdapter, "MobileadsAppLovinMediationDependencies") },
            { YandexAdNetwork.Bigoads, (this.useBigoadsAdapter, "MobileadsBigoAdsMediationDependencies") },
            { YandexAdNetwork.Chartboost, (this.useChartboostAdapter, "MobileadsChartboostMediationDependencies") },
            { YandexAdNetwork.Google, (this.useGoogleAdapter, "MobileadsGoogleMediationDependencies") },
            { YandexAdNetwork.Inmobi, (this.useInmobiAdapter, "MobileadsInmobiMediationDependencies") },
            { YandexAdNetwork.Ironsource, (this.useIronsourceAdapter, "MobileadsIronSourceMediationDependencies") },
            { YandexAdNetwork.Mintegral, (this.useMintegralAdapter, "MobileadsMintegralMediationDependencies") },
            { YandexAdNetwork.Mytarget, (this.useMytargetAdapter, "MobileadsMytargetMediationDependencies") },
            { YandexAdNetwork.Pangle, (this.usePangleAdapter, "MobileadsPangleMediationDependencies") },
            { YandexAdNetwork.Startapp, (this.useStartappAdapter, "MobileadsStartappMediationDependencies") },
            { YandexAdNetwork.Tapjoy, (this.useTapjoyAdapter, "MobileadsTapjoyMediationDependencies") },
            { YandexAdNetwork.Unityads, (this.useUnityadsAdapter, "MobileadsUnityAdsMediationDependencies") },
            { YandexAdNetwork.Vungle, (this.useVungleAdapter, "MobileadsVungleMediationDependencies") }
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

        private void DownloadNetworkAdapter(string name)
        {
#if UNITY_EDITOR
            var isEnable = this.AdapterInfo[name].Item1;
            var xmlName  = this.AdapterInfo[name].Item2;
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

        [SerializeField, OnValueChanged(nameof(OnAdcolonyValueChanged)), BoxGroup("Network Adapters", centerLabel: true)]
        private bool useAdcolonyAdapter;

        [SerializeField, OnValueChanged(nameof(OnApplovinValueChanged)), BoxGroup("Network Adapters")]
        private bool useApplovinAdapter;

        [SerializeField, OnValueChanged(nameof(OnBigoadsValueChanged)), BoxGroup("Network Adapters")]
        private bool useBigoadsAdapter;

        [SerializeField, OnValueChanged(nameof(OnChartboostValueChanged)), BoxGroup("Network Adapters")]
        private bool useChartboostAdapter;

        [SerializeField, OnValueChanged(nameof(OnGoogleValueChanged)), BoxGroup("Network Adapters")]
        private bool useGoogleAdapter;

        [SerializeField, OnValueChanged(nameof(OnInmobiValueChanged)), BoxGroup("Network Adapters")]
        private bool useInmobiAdapter;

        [SerializeField, OnValueChanged(nameof(OnIronsourceValueChanged)), BoxGroup("Network Adapters")]
        private bool useIronsourceAdapter;

        [SerializeField, OnValueChanged(nameof(OnMintegralValueChanged)), BoxGroup("Network Adapters")]
        private bool useMintegralAdapter;

        [SerializeField, OnValueChanged(nameof(OnMytargetValueChanged)), BoxGroup("Network Adapters")]
        private bool useMytargetAdapter;

        [SerializeField, OnValueChanged(nameof(OnPangleValueChanged)), BoxGroup("Network Adapters")]
        private bool usePangleAdapter;

        [SerializeField, OnValueChanged(nameof(OnStartappValueChanged)), BoxGroup("Network Adapters")]
        private bool useStartappAdapter;

        [SerializeField, OnValueChanged(nameof(OnTapjoyValueChanged)), BoxGroup("Network Adapters")]
        private bool useTapjoyAdapter;

        [SerializeField, OnValueChanged(nameof(OnUnityadsValueChanged)), BoxGroup("Network Adapters")]
        private bool useUnityadsAdapter;

        [SerializeField, OnValueChanged(nameof(OnVungleValueChanged)), BoxGroup("Network Adapters")]
        private bool useVungleAdapter;

        #endregion

        #region On Network Adapters Changed

        private void OnAdcolonyValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Adcolony);

        private void OnApplovinValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Applovin);

        private void OnBigoadsValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Bigoads);

        private void OnChartboostValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Chartboost);

        private void OnGoogleValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Google);

        private void OnInmobiValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Inmobi);

        private void OnIronsourceValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Ironsource);

        private void OnMintegralValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Mintegral);

        private void OnMytargetValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Mytarget);

        private void OnPangleValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Pangle);

        private void OnStartappValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Startapp);

        private void OnTapjoyValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Tapjoy);

        private void OnUnityadsValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Unityads);

        private void OnVungleValueChanged() => this.DownloadNetworkAdapter(YandexAdNetwork.Vungle);

        #endregion
    }
}