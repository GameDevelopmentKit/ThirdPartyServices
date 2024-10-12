namespace Core.AnalyticServices.Signal
{
    using System.Collections.Generic;
    using Core.AnalyticServices.Data;

    /// <summary>
    /// Triggered when any event has been tracked
    /// </summary>
    public class EventTrackedSignal
    {
        /// <summary>
        /// the event object which will be forwarded to attached services
        /// </summary>
        public IEvent TrackedEvent;

        /// <summary>
        /// any user properties waiting to be forwarded to attached services
        /// </summary>
        public Dictionary<string, object> ChangedProps;
    }
}