#if ADJUST
namespace Core.AnalyticServices
{
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
#if UNITY_ANDROID
        public string AdjustAppToken      => this.adjustAndroidAppToken;
        public string AdjustPurchaseToken => this.adjustAndroidPurchaseToken;
#elif UNITY_IOS
        public string AdjustAppToken      => this.adjustIOSAppToken;
        public string AdjustPurchaseToken => this.adjustIOSPurchaseToken;
#else
        public string AdjustAppToken      => this.adjustAndroidAppToken;
        public string AdjustPurchaseToken => this.adjustAndroidPurchaseToken;
#endif
        public bool   AdjustIsDebug       => this.adjustIsDebug;

        [SerializeField] private string adjustAndroidAppToken;
        [SerializeField] private string adjustIOSAppToken;
        [SerializeField] private string adjustAndroidPurchaseToken;
        [SerializeField] private string adjustIOSPurchaseToken;
        [SerializeField] private bool   adjustIsDebug;
    }
}
#endif