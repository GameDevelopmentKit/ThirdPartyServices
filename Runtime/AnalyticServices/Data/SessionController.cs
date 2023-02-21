namespace AnalyticServices.Data
{
    using System;
    using System.Collections;
    using Tools;
    using UnityEngine;
    using Utilities.Utils;
    using Zenject;

    /// <summary>
    /// todo
    /// </summary>
    internal sealed class SessionController : MonoBehaviour
    {
        private IAnalyticServices analyticServices;
        private DeviceInfo       deviceInfo;

        /// <summary>
        /// 
        /// </summary>
        public string SessionId { get; private set; }

        private const float  HeartbeatInterval = 30f;
        private const double SessionTimeout    = 600000; //  10 min, todo - make config controllable
        private       double focusOutTime      = double.NaN;

        [Inject]
        public void Init(IAnalyticServices analyticServicesParam, DeviceInfo deviceInfoParam)
        {
            this.analyticServices = analyticServicesParam;
            this.deviceInfo       = deviceInfoParam;
        }
        private void Start()
        {
            this.SessionId = Guid.NewGuid().ToString("N");

            this.analyticServices.Track(new SessionStarted
            {
                InstallId   = this.deviceInfo.InstallId,
                FirstLaunch = this.deviceInfo.IsFirstLaunch,
            });

            this.StartCoroutine(this.Heartbeat());
        }

        private IEnumerator Heartbeat()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(HeartbeatInterval);
                this.analyticServices.Track(new Heartbeat());
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            var focusTime = TimeUtils.LocalMilliSeconds;

            if (hasFocus)
            {
                if (double.IsNaN(this.focusOutTime))
                    return;

                var deltaFocusTime = focusTime - this.focusOutTime;
                this.focusOutTime = double.NaN;

                if (deltaFocusTime is > SessionTimeout or < 0)
                {
                    this.Start();
                }
                else
                {
                    this.analyticServices.Track(new FocusIn());
                }
            }
            else
            {
                this.focusOutTime = focusTime;
                this.analyticServices.Track(new FocusOut());
            }
        }
    }
}