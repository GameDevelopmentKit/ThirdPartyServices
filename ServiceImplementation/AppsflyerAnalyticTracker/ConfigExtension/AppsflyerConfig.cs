#if APPSFLYER
namespace Core.AnalyticServices
{
    using System;
    using Sirenix.OdinInspector;
    using Sirenix.Serialization;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig 
    {
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

        [Header("DevKey")]
        [SerializeField] private string appsflyerDevKeyIos;
        [SerializeField] private string appsflyerDevKeyAndroid;

        [Header("App Id")]
        [SerializeField] private string appsflyerAppIdIos;
        // [SerializeField][ReadOnly] private string appsflyerAppIdAndroid = Application.identifier;


        [SerializeField] private bool appsflyerIsDebug;
    }
}
#endif