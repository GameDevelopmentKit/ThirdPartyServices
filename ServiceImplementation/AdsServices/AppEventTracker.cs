namespace ServiceImplementation.AdsServices
{
    using System;
    using UnityEngine;

    public class AppEventTracker : MonoBehaviour
    {
        public  Action<bool> ApplicationPauseAction            { get; set; }
        private void         OnApplicationPause(bool hasPause) { this.ApplicationPauseAction?.Invoke(hasPause); }
    }
}