namespace Core.AnalyticServices.CommonEvents
{
    using System;
    using System.Collections.Generic;
    using Core.AnalyticServices.Data;

    /// <summary>
    /// Fully custom event class which can be used to send any data that falls outside the bounds
    /// of the standard taxonomy.
    /// </summary>
    [Serializable]
    public sealed class CustomEvent : IEvent
    {
        /// <summary>
        /// The name of your custom event.
        /// </summary>
        /// <remarks>
        /// Typically all event names are in `snake_case`
        /// </remarks>
        public string EventName;

        /// <summary>
        /// Any properties of your custom event.
        /// </summary>
        /// <remarks>
        /// Typically all event property keys are in `snake_case`
        /// </remarks>
        public Dictionary<string, object> EventProperties;
    }

    /*
    * GamePlay
    */

    #region Gameplay

    /// <summary>
    /// Default location change event class (screen in game).
    /// </summary>
    /// <remarks>
    /// Use this class to track location changes (screen in game).
    /// </remarks>
    [Serializable]
    internal class ScreenLocationChange : IEvent
    {
        /// <summary>
        /// Next (current) location (screen).
        /// </summary>
        public string LocationNext;

        /// <summary>
        /// Previous location (screen).
        /// </summary>
        public string LocationPrev;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenLocationChange"/> class.
        /// </summary>
        /// <param name="locationNext">Next (current) location (screen).</param>
        /// <param name="locationPrev">Previous location (screen).</param>
        public ScreenLocationChange(string locationNext, string locationPrev)
        {
            this.LocationNext = locationNext;
            this.LocationPrev = locationPrev;
        }
    }
    #endregion

    /*
     * App Lifecycle
     */

    #region App Lifecycle

    /// <summary>
    /// An event automatically fired when Unity reports a focus in.
    /// todo - acquire reporting from native and not unity
    /// </summary>
    internal sealed class FocusIn : IEvent
    {
    }

    /// <summary>
    /// An event automatically fired when Unity reports a focus out.
    /// todo - acquire reporting from native and not unity
    /// </summary>
    internal sealed class FocusOut : IEvent
    {
    }

    /// <summary>
    /// An event automatically fired every 30 seconds (default)
    /// </summary>
    internal sealed class Heartbeat : IEvent
    {
    }

    /*
     * App Initialization
     */

    /// <summary>
    /// An event automatically fired by the SDK which reports that the app has launched. This
    /// event is fired before any identifiers can be attached.
    /// </summary>
    [Serializable]
    internal class GameLaunched : IEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public string InstallId;

        /// <summary>
        /// 
        /// </summary>
        public bool FirstLaunch;
    }

    /// <summary>
    /// An event automatically fired by the SDK which reports that a session id has been created with
    /// identifiers ready to be sent.
    /// </summary>
    [Serializable]
    internal sealed class SessionStarted : GameLaunched
    {
    }
    #endregion


    /*
     * Service Status
     */

    #region Service Status

    /// <summary>
    /// Generic event for service initialization. Used for tracking purposes.
    /// </summary>
    [Serializable]
    public class ServiceStatus : IEvent
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public String ServiceName;
    }

    /// <summary>
    /// An event fired when a service is trying to get initialized.
    /// </summary>
    [Serializable]
    public sealed class ServiceWillInit : ServiceStatus
    {
    }

    /// <summary>
    /// An event fired when a service has initialized correctly.
    /// </summary>
    [Serializable]
    public sealed class ServiceDidInit : ServiceStatus
    {
    }

    /// <summary>
    /// An event fired when a service fails to initialize.
    /// </summary>
    [Serializable]
    public sealed class ServiceFailedInit : ServiceStatus
    {
        /// <summary>
        /// Error message if any.
        /// </summary>
        public String Error;
    }
    #endregion
}