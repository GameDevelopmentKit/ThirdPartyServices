namespace Core.AnalyticServices
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Data;
    using Core.AnalyticServices.Signal;
    using Core.AnalyticServices.Tools;
    using GameFoundation.Signals;
    using UnityEngine;
    using UnityEngine.Scripting;
    using Utilities.Extension;

    public interface IAnalyticServices
    {
        /// <summary>
        /// Container for all predefined and custom properties which will be pushed to attached services
        /// </summary>
        public UserProperties UserProperties { get; }

        /// <summary>
        /// Start tracking
        /// </summary>
        public void Start();

        /// <summary>
        /// For informing attached services that there is an event waiting to be tracked
        /// </summary>
        /// <param name="trackedEvent"></param>
        void Track(IEvent trackedEvent);
    }

    public class AnalyticServices : IAnalyticServices
    {
        private readonly DeviceInfo         deviceInfo;
        private readonly SignalBus          signalBus;
        private readonly EventTrackedSignal eventTrackedSignal;

        private TaskCompletionSource<bool> started;

        public UserProperties UserProperties { get; }

        [Preserve]
        public AnalyticServices(DeviceInfo deviceInfo, SignalBus signalBus)
        {
            this.deviceInfo         = deviceInfo;
            this.signalBus          = signalBus;
            this.UserProperties     = new(this);
            this.eventTrackedSignal = new();
            this.started            = new();
            //todo need to refactor this
        }

        void IAnalyticServices.Start()
        {
            if (this.started.Task.Status == TaskStatus.RanToCompletion) return;

            this.Track(new GameLaunched
            {
                InstallId   = this.deviceInfo.InstallId,
                FirstLaunch = this.deviceInfo.IsFirstLaunch,
            });

            this.deviceInfo.ScrapeDeviceData();
            this.SetupUserProperties();

            this.UserProperties.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(this.UserProperties.UserId)) this.signalBus.Fire(new SetUserIdSignal() { UserId = this.UserProperties.UserId });
            };
            this.started.SetResult(true);
        }

        public async void Track(IEvent trackedEvent)
        {
            await this.started.Task;
            this.eventTrackedSignal.TrackedEvent = trackedEvent;
            this.eventTrackedSignal.ChangedProps = this.UserProperties.ChangedProps.Count > 0 ? this.UserProperties.ChangedProps.Copy() : null;
            this.signalBus.Fire(this.eventTrackedSignal);

            this.UserProperties.ChangedProps.Clear();
        }

        private void SetupUserProperties()
        {
            // this.UserProperties.GameEnvironment  = this.Config.Environment ? "develop" : "production";
            // this.UserProperties.GameName         = this.Config.TpGameName;
            // this.UserProperties.GameId           = this.Config.TpGameId;

            this.UserProperties.DeviceLanguage = this.deviceInfo.Language;
            this.UserProperties.DeviceLocale   = Application.systemLanguage.ToString();
            this.UserProperties.DeviceCountry  = this.deviceInfo.Country;
            this.UserProperties.DeviceModel    = SystemInfo.deviceModel;
            this.UserProperties.DeviceMake     = this.deviceInfo.Make;
            this.UserProperties.DeviceFamily   = this.deviceInfo.Family;

            #if UNITY_IOS && !UNITY_EDITOR
            this.UserProperties.DeviceVendorId = this.deviceInfo.Idfv;
            this.UserProperties.DeviceAdvertisingId = this.deviceInfo.Idfa;
            #elif UNITY_ANDROID && !UNITY_EDITOR
            this.UserProperties.DeviceVendorId = this.deviceInfo.AndroidId;
            this.UserProperties.DeviceAdvertisingId = this.deviceInfo.Gaid;
            #elif UNITY_WSA_10_0 && !UNITY_EDITOR
            this.UserProperties.DeviceAppHardwareId = this.deviceInfo.ASHWID;
            #endif

            this.UserProperties.PlatformName    = this.deviceInfo.Platform;
            this.UserProperties.PlatformVersion = this.deviceInfo.OSVersion;

            this.UserProperties.GameVersionName  = this.deviceInfo.GameVersion;
            this.UserProperties.GameVersionCode  = this.deviceInfo.GameBuildNumber;
            this.UserProperties.GameBundleId     = this.deviceInfo.BundleId;
            this.UserProperties.GameIsTestflight = this.deviceInfo.IsTestflightBuild;
            this.UserProperties.GameInstallMode  = Application.installMode.ToString().ToLowerInvariant();

            this.UserProperties.InstallId = this.deviceInfo.InstallId;

            if (!PlayerPrefs.HasKey(DeviceInfo.InstallDateKey)) return;

            var installDate         = PlayerPrefs.GetString(DeviceInfo.InstallDateKey);
            var installDateTime     = Convert.ToDateTime(installDate, CultureInfo.InvariantCulture);
            var installDateString   = installDateTime.ToString("yyyyMMdd");
            var installMilliseconds = installDateTime.Subtract(new DateTime(1970, 1, 1));

            var timeDiff = DateTime.UtcNow - installDateTime;

            this.UserProperties.InstallDate      = installDateString;
            this.UserProperties.InstallTimestamp = installMilliseconds.TotalMilliseconds;
            this.UserProperties.InstallAge       = timeDiff.Days;
        }
    }
}