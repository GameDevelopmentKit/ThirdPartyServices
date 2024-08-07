namespace Core.AnalyticServices
{
    using System;
    using GameConfigs;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    [Serializable]
    public partial class AnalyticConfig : ScriptableObject, IGameConfig
    {
        [BoxGroup("General")] [SerializeField] private bool autoImportPackages = false;
    }
}