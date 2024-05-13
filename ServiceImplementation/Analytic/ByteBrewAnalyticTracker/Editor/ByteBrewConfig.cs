namespace Core.AnalyticServices
{
#if  UNITY_EDITOR
    using ServiceImplementation.Configs.Editor;
    using UnityEditor.PackageManager;
#endif
    using System.IO;
    using Newtonsoft.Json.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        private const string ByteBrewSymbol = "BYTEBREW";
        
        [BoxGroup("ByteBrew")] [LabelText("Enable", SdfIconType.Youtube)] [OnValueChanged("OnChangeByteBrewEnabled")]
        [SerializeField] private bool isByteBrewEnabled;

        private void OnChangeByteBrewEnabled()
        {
#if  UNITY_EDITOR
            this.ModifyPackage(this.isByteBrewEnabled, "com.theone.bytebrew", "https://github.com/The1Studio/ByteBrew.git?path=Assets/Src/ByteBrewSDK#");
            DefineSymbolEditorUtils.SetDefineSymbol(ByteBrewSymbol, this.isByteBrewEnabled);
#endif
        }

#if  UNITY_EDITOR
        // Modify the package in the manifest.json
        private void ModifyPackage(bool add, string packageName, string packagePath)
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (File.Exists(manifestPath))
            {
                string  manifestContent = File.ReadAllText(manifestPath);
                JObject manifestJson    = JObject.Parse(manifestContent);

                // Check if the package already exists
                var packageToken = manifestJson["dependencies"][packageName];

                if (add)
                {
                    if (packageToken == null)
                    {
                        // Add new package
                        manifestJson["dependencies"][packageName] = packagePath;
                        Debug.Log($"Package {packageName} added successfully.");
                    }
                    else
                    {
                        Debug.LogWarning($"Package {packageName} already exists. No action taken.");
                    }
                }
                else
                {
                    if (packageToken != null)
                    {
                        // Remove existing package
                        packageToken.Parent.Remove();
                        Debug.Log($"Package {packageName} removed successfully.");
                    }
                    else
                    {
                        Debug.LogWarning($"Package {packageName} does not exist. No action taken.");
                    }
                }

                // Write the updated JSON back to the file
                File.WriteAllText(manifestPath, manifestJson.ToString());
            }
            else
            {
                Debug.LogError("manifest.json not found.");
            }

            Client.Resolve();
        }
#endif

#if BYTEBREW

        [OnValueChanged("SaveByteBrewSetting")] [Header("Android")] [SerializeField] [BoxGroup("ByteBrew")]
        private bool androidEnabled;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("androidEnabled")] [SerializeField]
        private string byteBrewAppIdAndroid;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("androidEnabled")] [SerializeField]
        private string byteBrewDevKeyAndroid;

        [OnValueChanged("SaveByteBrewSetting")] [Header("IOS")] [SerializeField] [BoxGroup("ByteBrew")]
        private bool iosEnabled;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("iosEnabled")] [SerializeField]
        private string byteBrewAppIdIos;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("iosEnabled")] [SerializeField]
        private string byteBrewDevKeyIos;

        [OnInspectorInit]
        private void LoadByteBrewSetting()
        {
            var byteBrewSettings = Resources.Load<ByteBrewSettings>("ByteBrewSettings");

            this.androidEnabled        = byteBrewSettings.androidEnabled;
            this.byteBrewAppIdAndroid  = byteBrewSettings.androidGameID;
            this.byteBrewDevKeyAndroid = byteBrewSettings.androidSDKKey;

            this.iosEnabled        = byteBrewSettings.iosEnabled;
            this.byteBrewAppIdIos  = byteBrewSettings.iosGameID;
            this.byteBrewDevKeyIos = byteBrewSettings.iosSDKKey;
        }

        private void SaveByteBrewSetting()
        {
            var byteBrewSettings = Resources.Load<ByteBrewSettings>("ByteBrewSettings");

            byteBrewSettings.androidEnabled = this.androidEnabled;
            byteBrewSettings.androidGameID  = this.byteBrewAppIdAndroid;
            byteBrewSettings.androidSDKKey  = this.byteBrewDevKeyAndroid;
            
            byteBrewSettings.iosEnabled = this.iosEnabled;
            byteBrewSettings.iosGameID  = this.byteBrewAppIdIos;
            byteBrewSettings.iosSDKKey  = this.byteBrewDevKeyIos;
        }
#endif
    }
}