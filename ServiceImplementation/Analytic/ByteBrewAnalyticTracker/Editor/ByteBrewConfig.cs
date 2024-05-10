#if BYTEBREW
namespace Core.AnalyticServices
{
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
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
    }
}
#endif