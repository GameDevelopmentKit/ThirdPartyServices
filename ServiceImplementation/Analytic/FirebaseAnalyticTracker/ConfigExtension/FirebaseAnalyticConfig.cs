namespace Core.AnalyticServices
{
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Contains all the constants, the configuration of Analytic service
    /// </summary>
    public partial class AnalyticConfig
    {
#if FIREBASE_SDK_EXISTS
  
            // Prevent events that do not conform to Firebase conventions from being fired
            [BoxGroup("Firebase")] 
            [InfoBox("When enabled, Firebase Analytics will enforce strict naming conventions for events and parameters. This helps ensure data integrity and consistency in your analytics reports. " +
                     "If an event or parameter name does not conform to Firebase's naming rules, it will be rejected and not logged.", InfoMessageType.Warning)]
            public bool firebaseStrictMode = true;
#endif
    }
}