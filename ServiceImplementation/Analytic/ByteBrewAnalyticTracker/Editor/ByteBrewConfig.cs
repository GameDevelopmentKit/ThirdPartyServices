namespace Core.AnalyticServices
{
#if  UNITY_EDITOR
    using ServiceImplementation.Configs.Editor;
    using UnityEditor;
#endif
#if BYTEBREW
    using ByteBrewSDK;
#endif
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        private const string ByteBrewSymbol = "BYTEBREW";
        
        [BoxGroup("ByteBrew")] [LabelText("Enable", SdfIconType.Youtube)] [OnValueChanged("OnChangeByteBrewEnabled")]
        [SerializeField] private bool isByteBrewEnabled = true;
        
        [OnInspectorInit]
        private void OnChangeByteBrewEnabled()
        {
#if  UNITY_EDITOR
            EditorUtils.ModifyPackage(this.isByteBrewEnabled, "com.bytebrew.unitysdk", "https://github.com/ByteBrewIO/ByteBrewUnitySDK.git?path=UPMPackage#");
            EditorUtils.SetDefineSymbol(ByteBrewSymbol, this.isByteBrewEnabled);
#endif
        }

#if BYTEBREW

        [OnValueChanged("SaveByteBrewSetting")] [Header("Android")] [SerializeField] [BoxGroup("ByteBrew")]
        private bool androidEnabled;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("androidEnabled")] [SerializeField]
        private string byteBrewAppIdAndroid;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("androidEnabled")] [SerializeField]
        private string byteBrewSDKKeyAndroid;

        [OnValueChanged("SaveByteBrewSetting")] [Header("IOS")] [SerializeField] [BoxGroup("ByteBrew")]
        private bool iosEnabled;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("iosEnabled")] [SerializeField]
        private string byteBrewAppIdIos;

        [OnValueChanged("SaveByteBrewSetting")] [BoxGroup("ByteBrew")] [ShowIf("iosEnabled")] [SerializeField]
        private string byteBrewSDKKeyIos;

        [OnInspectorInit]
        private void LoadByteBrewSetting()
        {
            Debug.Log("OnInspectorInit");
            var byteBrewSettings = Resources.Load<ByteBrewSettings>("ByteBrewSettings");

            this.androidEnabled        = byteBrewSettings.androidEnabled;
            this.byteBrewAppIdAndroid  = byteBrewSettings.androidGameID;
            this.byteBrewSDKKeyAndroid = byteBrewSettings.androidSDKKey;

            this.iosEnabled        = byteBrewSettings.iosEnabled;
            this.byteBrewAppIdIos  = byteBrewSettings.iosGameID;
            this.byteBrewSDKKeyIos = byteBrewSettings.iosSDKKey;

#if UNITY_EDITOR
            if (!this.androidEnabled || string.IsNullOrEmpty(this.byteBrewAppIdAndroid) || string.IsNullOrEmpty(this.byteBrewSDKKeyAndroid))
            {
                Debug.LogError("ByteBrew Android settings are not set properly.");
            }
#endif
        }

        private void SaveByteBrewSetting()
        {
            var byteBrewSettings = Resources.Load<ByteBrewSettings>("ByteBrewSettings");

            byteBrewSettings.androidEnabled = this.androidEnabled;
            byteBrewSettings.androidGameID  = this.byteBrewAppIdAndroid;
            byteBrewSettings.androidSDKKey  = this.byteBrewSDKKeyAndroid;
            
            byteBrewSettings.iosEnabled = this.iosEnabled;
            byteBrewSettings.iosGameID  = this.byteBrewAppIdIos;
            byteBrewSettings.iosSDKKey  = this.byteBrewSDKKeyIos;
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(byteBrewSettings);
            AssetDatabase.SaveAssets();
#endif
        }
#endif
    }
}