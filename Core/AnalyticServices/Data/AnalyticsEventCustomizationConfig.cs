namespace Core.AnalyticServices.Data
{
    using System;
    using System.Collections.Generic;

    public class AnalyticsEventCustomizationConfig
    {
        public bool                       IgnoreAllEvents { get; set; } = false; // Include AdsRevenueEvents and IAP, for AMANOTES SDK
        public HashSet<Type>              IgnoreEvents    { get; set; } = new();
        public HashSet<string>            IncludeEvents   { get; set; } = new();
        public Dictionary<string, string> CustomEventKeys { get; set; } = new();
    }
}