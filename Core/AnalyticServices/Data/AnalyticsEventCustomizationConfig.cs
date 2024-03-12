namespace Core.AnalyticServices.Data
{
    using System;
    using System.Collections.Generic;

    public class AnalyticsEventCustomizationConfig
    {
        public HashSet<Type>              IgnoreEvents    { get; set; } = new();
        public HashSet<string>            IncludeEvents   { get; set; } = new();
        public Dictionary<string, string> CustomEventKeys { get; set; } = new();
    }
}