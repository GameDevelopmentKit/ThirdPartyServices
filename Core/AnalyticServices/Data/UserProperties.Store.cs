namespace Core.AnalyticServices.Data
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using Utilities.Extension;

    /// <summary>
    ///
    /// </summary>
    public sealed partial class UserProperties
    {
        internal readonly Dictionary<string, object> ChangedProps = new();

        private readonly Dictionary<string, object> store = new();

        private TProp get<TProp>([CallerMemberName] string key = "")
        {
            key = key.ToSnakeCase(); // todo - compile time

            if (!this.store.ContainsKey(key)) return default;

            if (this.store[key] is TProp) return (TProp)this.store[key];

            Debug.LogError($"attempted to get {key} by wrong type {typeof(TProp)}");
            return default;
        }

        private bool set<TProp>(TProp value, [CallerMemberName] string key = "")
        {
            var origKey = key;
            key = key.ToSnakeCase();

            if (value == null || (this.store.ContainsKey(key) && value.Equals(this.store[key]))) return false;

            this.store[key] = value;

            this.NotifyOfPropChange(key, origKey, value);

            return true;
        }

        private void NotifyOfPropChange<TProp>(string key, string origKey, TProp value)
        {
            this.ChangedProps[key] = value;
            this.PropertyChanged?.Invoke(this, new(origKey));
        }
    }
}