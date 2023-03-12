namespace Core.AnalyticServices
{
    using System;
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
                if(!String.IsNullOrEmpty(appsflyerDevKeyAndroid))
                    return this.appsflyerDevKeyAndroid;
#elif UNITY_IOS
                if(!String.IsNullOrEmpty(appsflyerDevKeyIos))
                    return this.appsflyerDevKeyIos;
#elif UNITY_WSA_10_0
                 if(!String.IsNullOrEmpty(appsflyerDevKeyUwp))
                    return this.appsflyerDevKeyUwp;
#endif
                return this.appsflyerDevKey;
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

        [SerializeField] private string appsflyerDevKey;
        [SerializeField] private string appsflyerDevKeyIos;
        [SerializeField] private string appsflyerDevKeyAndroid;
        [SerializeField] private string appsflyerDevKeyUwp;

        [SerializeField] private string appsflyerAppIdIos;
        [SerializeField] private string appsflyerAppIdUWP;

        [SerializeField] private bool appsflyerIsDebug;
    }
}