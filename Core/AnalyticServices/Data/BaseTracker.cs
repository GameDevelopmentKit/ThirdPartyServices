namespace Core.AnalyticServices.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Core.AnalyticServices.Tools;
    using UnityEngine;
    using Utilities.Extension;
    using Zenject;

    public delegate void EventDelegate(IEvent trackedEvent, Dictionary<string, object> data);

    public abstract class BaseTracker
    {
        #region inject

        protected readonly AnalyticConfig analyticConfig;

        #endregion

        /// <summary>
        /// signal to the base tracker that the "On" events are ready to be invoked
        /// </summary>
        protected abstract TaskCompletionSource<bool> TrackerReady { get; }

        /// <summary>
        /// events are ignored by special require from tracker
        /// </summary>
        protected virtual HashSet<Type> IgnoreEvents { get; }

        /// <summary>
        /// mapping of analytic event name or parameter which require specific mapping to the tracker
        /// </summary>
        protected virtual Dictionary<string, string> CustomEventKeys { get; }

        /// <summary>
        /// mapping of analytic events which require specific mapping to the tracker
        /// </summary>
        protected abstract Dictionary<Type, EventDelegate> CustomEventDelegates { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changedProps"></param>
        protected abstract void OnChangedProps(Dictionary<string, object> changedProps);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        protected abstract void OnEvent(string name, Dictionary<string, object> data);

        /// <summary>
        /// Must control init of the wrapped SDK in derived trackers
        /// </summary>
        protected abstract Task TrackerSetup();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        protected abstract void SetUserId(string userId);

        /// <summary>
        /// base constructor for trackers which sets up when/how events and states should be tracked
        /// </summary>
        public BaseTracker(SignalBus signalBus, AnalyticConfig analyticConfig)
        {
            this.analyticConfig = analyticConfig;
            signalBus.Subscribe<EventTrackedSignal>(this.EventTracked);
            signalBus.Subscribe<SetUserIdSignal>(signal => this.SetUserId(signal.UserId));
            this.Init();
        }

        private async void Init() { await this.TrackerSetup(); }

        private async void EventTracked(EventTrackedSignal trackedData)
        {
            // if the tracker has failed setup we should not forward it any events
            if (this.TrackerReady.Task.Status == TaskStatus.Canceled || this.TrackerReady.Task.Status == TaskStatus.Faulted)
                return;
            await this.TrackerReady.Task;

            if (trackedData.ChangedProps != null)
                this.OnChangedProps(trackedData.ChangedProps);

            var trackedEvent = trackedData.TrackedEvent;
            if (this.CustomEventDelegates != null && this.CustomEventDelegates.ContainsKey(trackedEvent.GetType()))
            {
                var eventDelegate = this.CustomEventDelegates[trackedEvent.GetType()];
                if (trackedEvent is IapTransactionDidSucceed iapEvent)
                {
                    iapEvent.Receipt = this.CheckReceiptFormat(iapEvent.Receipt);
                }

                eventDelegate?.Invoke(trackedEvent, trackedData.ChangedProps);
            }
            else
            {
                string                     eventName;
                Dictionary<string, object> eventData;

                if (this.IgnoreEvents != null && this.IgnoreEvents.Contains(trackedEvent.GetType())) return;

                if (trackedEvent is CustomEvent customEvent)
                {
                    eventName = customEvent.EventName;
                    eventData = customEvent.EventProperties;
                }
                else
                {
                    eventName = this.GetCorrectName(trackedEvent.GetType().Name);
                    eventData = this.ConvertObjectToDic(trackedEvent);
                }

                this.OnEvent(eventName, eventData);
            }
        }


        private Dictionary<string, object> ConvertObjectToDic(object obj)
        {
            var result     = new Dictionary<string, object>();
            var objectType = obj.GetType();

            foreach (var fieldInfo in objectType.GetFields())
            {
                result.Add(this.GetCorrectName(fieldInfo.Name), fieldInfo.GetValue(obj));
            }

            foreach (var propertyInfo in objectType.GetProperties())
            {
                result.Add(this.GetCorrectName(propertyInfo.Name), propertyInfo.GetValue(obj));
            }

            return result;
        }

        private string GetCorrectName(string rawName)
        {
            if (this.CustomEventKeys != null && this.CustomEventKeys.TryGetValue(rawName, out var correctName))
            {
                return correctName;
            }

            return rawName.ToSnakeCase();
        }

        private string CheckReceiptFormat(string receipt)
        {
            try
            {
                var parsedReceipt = JsonUtility.FromJson<UnityReceipt>(receipt);
                //Check if the parameter sent follows the Unity Purchase Receipt format.
                if (!string.IsNullOrEmpty(parsedReceipt.Payload) &&
                    !string.IsNullOrEmpty(parsedReceipt.Store) &&
                    !string.IsNullOrEmpty(parsedReceipt.TransactionID))
                {
                    //Return the receipt Payload to prevent integration errors.
                    Debug.LogWarning(
                        "[AnalyticService BaseTracker] Wrong receipt parameter detected. Replacing it with Receipt.Payload");

                    return parsedReceipt.Payload;
                }
            }
            catch (ArgumentException)
            {
                //If the receipt can't be parsed, return the original.
                return receipt;
            }

            //Return the original value if it doesn't match the Unity Purchase Receipt format.
            return receipt;
        }
    }
}