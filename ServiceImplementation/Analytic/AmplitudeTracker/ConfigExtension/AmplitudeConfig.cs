#if AMPLITUDE
namespace Core.AnalyticServices
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
        [Header("Amplitude")] [SerializeField] private string amplitudeApiKey;
        [SerializeField]                       private bool   amplitudeLogging;
        [SerializeField]                       private bool   amplitudeTrackSessionEvents;

        public string AmplitudeApiKey             => this.amplitudeApiKey;
        public bool   AmplitudeLogging            => this.amplitudeLogging;
        public bool   AmplitudeTrackSessionEvents => this.amplitudeTrackSessionEvents;
    }
}
#endif