#if ADJUST
namespace Core.AnalyticServices
{
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        public string AdjustAppToken      => this.adjustAppToken;
        public string AdjustPurchaseToken => this.adjustPurchaseToken;
        public bool   AdjustIsDebug       => this.adjustIsDebug;

        [SerializeField] private string adjustAppToken;
        [SerializeField] private string adjustPurchaseToken;
        [SerializeField] private bool   adjustIsDebug;
    }
}
#endif