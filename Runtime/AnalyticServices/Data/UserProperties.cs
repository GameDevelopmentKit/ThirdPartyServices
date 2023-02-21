namespace AnalyticServices.Data
{
    using System.ComponentModel;

    public sealed partial class UserProperties : INotifyPropertyChanged
    {
        private readonly IAnalyticServices analyticServices;
        public UserProperties(IAnalyticServices analyticServices) { this.analyticServices = analyticServices; }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// for setting of custom properties
        /// </summary>
        /// <param name="key"></param>
        public object this[string key] { set => this.set(value, key); }

        /*
         * User
         */

        /// <summary>
        /// Identifier for the user as set by the game
        /// </summary>
        public string UserId { get => this.get<string>(); set => this.set(value); }

        /// <summary>
        /// User's Game Center Id
        /// </summary>
        public string UserIdGamecenter { get => this.get<string>(); set => this.set(value); }

        /// <summary>
        /// User's Google Play Game Services Id
        /// </summary>
        public string UserIdGoogleplay { get => this.get<string>(); set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string UserIdFacebook { get => this.get<string>(); set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string UserCsLevel { get => this.get<string>(); set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public int UserTotalSpend { get => this.get<int>(); set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public int UserTotalPurchases { get => this.get<int>(); set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public bool ConsentAds { get => this.get<bool>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public bool ConsentAge { get => this.get<bool>(); internal set => this.set(value); }

        /*
         *  Gameplay
         */

        /// <summary>
        /// Use it to track current ingame location (screen in game).
        /// </summary>
        /// <remarks>
        /// Tracking of location (screen) changes can provide you additional information about user behaviour.
        /// This data will be sent as soon as possible.
        /// </remarks>
        public string Location
        {
            get => this.get<string>();
            set
            {
                var currentLocation = this.get<string>();
                if (currentLocation == value)
                {
                    // Do not track changes to same location.
                    return;
                }

                this.set(value);
                this.LocationPrev = currentLocation;
                this.analyticServices.Track(new LocationChange(value, currentLocation));
            }
        }

        /// <summary>
        /// Use it to track previous ingame screen (ingame location).
        /// (automatically updated with set <see cref="Location"/> ).
        /// </summary>
        /// <remarks>
        /// Tracking of screen changes can provide you additional information about user behaviour.
        /// </remarks>
        public string LocationPrev { get => this.get<string>(); internal set => this.set(value); }

        /*
         * Game
         */

        /// <summary>
        /// 
        /// </summary>
        public string GameBundleId { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string GameVersionName { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string GameVersionCode { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string GameEnvironment { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public bool GameIsTestflight { get => this.get<bool>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string GameInstallMode { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string GameName { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string GameId { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// Indicates if application is patched or pirated.
        /// </summary>
        public bool GameValidApp { get => this.get<bool>(); set => this.set(value); }


        /*
         * OS
         */

        /// <summary>
        /// 
        /// </summary>
        public string PlatformName { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string PlatformVersion { get => this.get<string>(); internal set => this.set(value); }

        /*
         * Device
         */

        /// <summary>
        /// 
        /// </summary>
        public string DeviceLanguage { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceLocale { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceCountry { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceModel { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public bool DeviceIsTest { get => this.get<bool>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceMake { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceFamily { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceVendorId { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceAdvertisingId { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string DeviceAppHardwareId { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string InstallId { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public string InstallDate { get => this.get<string>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public int InstallAge { get => this.get<int>(); internal set => this.set(value); }

        /// <summary>
        /// 
        /// </summary>
        public double InstallTimestamp { get => this.get<double>(); internal set => this.set(value); }

        /// <summary>
        /// todo - move to correct package
        /// </summary>
        public string AppsflyerId { get => this.get<string>(); set => this.set(value); }
    }
}