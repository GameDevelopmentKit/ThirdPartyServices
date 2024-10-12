namespace ServiceImplementation.Configs.CustomTypes
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using UnityEngine;

    /// <typeparam name="T">Key type.</typeparam>
    /// <typeparam name="U">Value type.</typeparam>
    /// <typeparam name="V">Value storage type.</typeparam>
    public abstract class SerializableDictionaryBase<T, U, V> :
        Dictionary<T, U>,
        ISerializationCallbackReceiver
    {
        [SerializeField] protected T[] keys;

        [SerializeField] protected V[] values;

        public SerializableDictionaryBase(IDictionary<T, U> dict)
            : base(dict.Count)
        {
            foreach (var kvp in dict) this[kvp.Key] = kvp.Value;
        }

        public SerializableDictionaryBase()
        {
        }

        protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected abstract void SetValue(V[] storage, int i, U value);

        protected abstract U GetValue(V[] storage, int i);

        public void CopyFrom(IDictionary<T, U> dict)
        {
            this.Clear();
            foreach (var pair in dict) this[pair.Key] = pair.Value;
        }

        public void OnAfterDeserialize()
        {
            if (this.keys != null && this.values != null && this.keys.Length == this.values.Length)
            {
                this.Clear();
                for (var i = 0; i < this.keys.Length; ++i) this[this.keys[i]] = this.GetValue(this.values, i);

                this.keys   = null;
                this.values = null;
            }
        }

        public void OnBeforeSerialize()
        {
            this.keys   = new T[this.Count];
            this.values = new V[this.Count];

            var i = 0;
            foreach (var pair in this)
            {
                this.keys[i] = pair.Key;
                this.SetValue(this.values, i, pair.Value);
                ++i;
            }
        }
    }
}