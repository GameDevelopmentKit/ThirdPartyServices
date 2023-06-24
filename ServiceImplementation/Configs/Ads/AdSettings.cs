namespace ServiceImplementation.Configs.Ads
{
    using UnityEngine;

    [System.Serializable]
    public class  AdSettings
    {
        /// <summary>
        /// Gets the AdMob settings.
        /// </summary>
        public AdMobSettings AdMob
        {
            get { return this.mAdMob; }
        }

        /// <summary>
        /// Gets the AppLovin settings.
        /// </summary>
        public AppLovinSettings AppLovin
        {
            get { return this.mAppLovin; }
        }

        /// <summary>
        /// Gets the IronSource settings.
        /// </summary>
        public IronSourceSettings IronSource
        {
            get { return this.mIronSource; }
        }

        [SerializeField]
        private AdMobSettings mAdMob = null;
        [SerializeField]
        private AppLovinSettings mAppLovin = null;
        [SerializeField]
        private IronSourceSettings mIronSource = null;
    }
}