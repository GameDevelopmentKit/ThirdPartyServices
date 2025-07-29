namespace Core.AnalyticServices.Signal
{
    using System.Collections.Generic;

    public class AttributionChangedSignal
    {
        public Dictionary<string, object> EventProperties;
        
        public AttributionChangedSignal(Dictionary<string, object> eventProperties) { this.EventProperties = eventProperties; }
    }
}