namespace ServiceImplementation.AdsServices
{
    using System;
    using GoogleMobileAds.Common;
    using UnityEngine;

    public class AppEventTracker : MonoBehaviour
    {
        public  Action<AppState> ApplicationPauseAction            { get; set; }
        private void             OnApplicationFocus(bool hasFocus) { this.ApplicationPauseAction?.Invoke(hasFocus ? AppState.Foreground : AppState.Background); }
    }
}