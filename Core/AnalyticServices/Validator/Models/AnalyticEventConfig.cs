#nullable enable
namespace ServiceImplementation.Analytic.Validator
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class AnalyticEventConfig 
    {
        public List<string> CommonEvents { get; set; } = null!;

        public List<string> CustomEvents { get; set; } = null!;

        [JsonIgnore]
        public IEnumerable<string> RequiredEvents => this.CommonEvents.Union(this.CustomEvents);

    }
}