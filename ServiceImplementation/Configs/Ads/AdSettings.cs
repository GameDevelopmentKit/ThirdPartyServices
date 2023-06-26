namespace ServiceImplementation.Configs.Ads
{
    using System;
    using UnityEngine;

    [Serializable]
    public class AdSettings
    {
#if ADMOB
        /// <summary>
        /// Gets the AdMob settings.
        /// </summary>
        public AdMobSettings AdMob { get { return this.mAdMob; } }

        [SerializeField] private AdMobSettings mAdMob = null;

#endif


#if APPLOVIN
        /// <summary>
        /// Gets the AppLovin settings.
        /// </summary>
        public AppLovinSettings AppLovin
        {
            get { return this.mAppLovin; }

        }
        [SerializeField] private AppLovinSettings   mAppLovin = null;
#endif

#if IRONSOURCE
        /// <summary>
        /// Gets the IronSource settings.
        /// </summary>
        public IronSourceSettings IronSource { get { return this.mIronSource; } }
        [SerializeField] private IronSourceSettings mIronSource = null;
#endif
    }
}