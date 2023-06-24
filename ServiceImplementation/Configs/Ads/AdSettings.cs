namespace ServiceImplementation.Configs.Ads
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class AdSettings
    {
        /// <summary>
        /// Gets the AdMob settings.
        /// </summary>
        public AdMobSettings AdMob { get { return this.mAdMob; } }

        /// <summary>
        /// Gets the AppLovin settings.
        /// </summary>
        public AppLovinSettings AppLovin { get { return this.mAppLovin; } }

        /// <summary>
        /// Gets the IronSource settings.
        /// </summary>
        public IronSourceSettings IronSource { get { return this.mIronSource; } }

        [SerializeField] private bool enableAdMob;

        [SerializeField] [ShowIf("enableAdMob")] [LabelText("")] [BoxGroup("AdMob")]
        private AdMobSettings mAdMob = null;

        [SerializeField] private bool enableAppLovin;

        [SerializeField] [ShowIf("enableAppLovin")] [LabelText("")] [BoxGroup("AppLovin")]
        private AppLovinSettings mAppLovin = null;

        [SerializeField] private bool enableIronSource;

        [SerializeField] [ShowIf("enableIronSource")] [BoxGroup("IronSource")]
        private IronSourceSettings mIronSource = null;
    }
}