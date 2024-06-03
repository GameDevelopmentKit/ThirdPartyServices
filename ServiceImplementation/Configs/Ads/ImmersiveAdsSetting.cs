namespace ServiceImplementation.Configs.Ads
{
    using System;
    using UnityEngine;
    using Sirenix.OdinInspector;
#if UNITY_EDITOR
    using UnityEditor;
    using System.Reflection;
    using UnityEditor.Callbacks;
#endif

    [Serializable]
    public class ImmersiveAdsSetting
    {
        [Header("PubScale Setting")]
        [SerializeField, LabelText("Test Mode", SdfIconType.CheckSquare), OnValueChanged("SavePubScaleSetting")]
        private bool userTestMode;

        [SerializeField, LabelText("Fallback Native ID", SdfIconType.Google), OnValueChanged("SavePubScaleSetting")]
        private string fallbackNativeAdIdAndroid;

        [SerializeField, LabelText("Fallback Native ID", SdfIconType.Apple), OnValueChanged("SavePubScaleSetting")]
        private string fallbackNativeAdIdIos;

        [SerializeField, LabelText("App ID", SdfIconType.Google), OnValueChanged("SavePubScaleSetting")]
        private string appIdAndroid;

        [SerializeField, LabelText("App ID", SdfIconType.Apple), OnValueChanged("SavePubScaleSetting")]
        private string appIdIos;

        [SerializeField, LabelText("App ID", SdfIconType.AppIndicator), OnValueChanged("SavePubScaleSetting")]
        private string appId;

        [OnInspectorInit]
        private void LoadPubScaleSetting()
        {
#if IMMERSIVE_ADS && UNITY_EDITOR
            var pubScaleSetting = Resources.Load<ScriptableObject>("PubScaleSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var settingType  = pubScaleSetting.GetType();
            this.userTestMode = (bool)settingType.GetField("UseTestMode", bindingFlags).GetValue(pubScaleSetting);
            
            this.fallbackNativeAdIdAndroid = settingType.GetField("Fallback_NativeAdID_Android", bindingFlags).GetValue(pubScaleSetting) as string;
            this.fallbackNativeAdIdIos     = settingType.GetField("Fallback_NativeAdID_IOS", bindingFlags).GetValue(pubScaleSetting) as string;
            
            this.appIdAndroid = settingType.GetField("AppID_Android", bindingFlags).GetValue(pubScaleSetting) as string;
            this.appIdIos     = settingType.GetField("AppID_IOS", bindingFlags).GetValue(pubScaleSetting) as string;
            this.appId        = settingType.GetField("AppID", bindingFlags).GetValue(pubScaleSetting) as string;
#endif
        }

        public void SavePubScaleSetting()
        {
#if IMMERSIVE_ADS && UNITY_EDITOR
            var pubScaleSetting = Resources.Load<ScriptableObject>("PubScaleSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var settingType  = pubScaleSetting.GetType();
            settingType.GetField("UseTestMode", bindingFlags).SetValue(pubScaleSetting, this.userTestMode);
            settingType.GetField("Fallback_NativeAdID_Android", bindingFlags).SetValue(pubScaleSetting, this.fallbackNativeAdIdAndroid);
            settingType.GetField("Fallback_NativeAdID_IOS", bindingFlags).SetValue(pubScaleSetting, this.fallbackNativeAdIdIos);
            settingType.GetField("AppID_Android", bindingFlags).SetValue(pubScaleSetting, this.appIdAndroid);
            settingType.GetField("AppID_IOS",     bindingFlags).SetValue(pubScaleSetting, this.appIdIos);
            settingType.GetField("AppID",         bindingFlags).SetValue(pubScaleSetting, this.appId);

            EditorUtility.SetDirty(pubScaleSetting);
            AssetDatabase.SaveAssets();
#endif
        }

        [PostProcessBuild(int.MaxValue)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuildProject)
        {
#if IMMERSIVE_ADS && UNITY_EDITOR
            var pubScaleSetting = Resources.Load<ScriptableObject>("PubScaleSettings");
            var bindingFlags    = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var settingType     = pubScaleSetting.GetType();
#if THEONE_ADS_DEBUG
            settingType.GetField("UseTestMode", bindingFlags).SetValue(pubScaleSetting, true);
#else
            settingType.GetField("UseTestMode", bindingFlags).SetValue(pubScaleSetting, false);
#endif
#endif
        }
    }
}