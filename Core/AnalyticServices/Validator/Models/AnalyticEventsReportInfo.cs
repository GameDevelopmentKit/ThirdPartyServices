#nullable enable
namespace ServiceImplementation.Analytic.Validator
{
    using System.Collections.Generic;

    public class AnalyticEventsReportInfo
    {
        public IReadOnlyList<string> MissingEvents { get; }

        public AnalyticEventsReportInfo(IReadOnlyList<string> missingEvents) { this.MissingEvents = missingEvents; }
    }
}