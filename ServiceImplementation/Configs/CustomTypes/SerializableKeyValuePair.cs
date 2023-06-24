namespace ServiceImplementation.Configs.CustomTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Simple serializable version of <see cref="KeyValuePair{TKey, TValue}"/>
    /// </summary>
    [Serializable]
    public class SerializableKeyValuePair<K, V>
    {
        [SerializeField]
        private K key;

        [SerializeField]
        private V value;

        public SerializableKeyValuePair(K key, V value)
        {
            this.key = key;
            this.value = value;
        }

        public SerializableKeyValuePair(KeyValuePair<K, V> pair)
        {
            this.key   = pair.Key;
            this.value = pair.Value;
        }

        public K Key
        {
            get { return this.key; }
            set { this.key = value; }
        }

        public V Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public KeyValuePair<K, V> ToKeyValuePair()
        {
            return new KeyValuePair<K, V>(this.key, this.value);  
        }

        public static SerializableKeyValuePair<K, V> FromKeyValuePair(KeyValuePair<K, V> pair)
        {
            return new SerializableKeyValuePair<K, V>(pair.Key, pair.Value);
        }
    }

    public static class SerializableKeyValuePairExtension
    {
        public static SerializableKeyValuePair<K, V> ToSerializableKeyValuePair<K, V>(this KeyValuePair<K, V> pair)
        {
            return SerializableKeyValuePair<K, V>.FromKeyValuePair(pair);
        }

        public static IEnumerable<SerializableKeyValuePair<K, V>> ToSerializableKeyValuePairs<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs)
        {
            if (pairs == null)
                return null;

            return pairs.Select(p => new SerializableKeyValuePair<K, V>(p.Key, p.Value));
        }

        public static IEnumerable<KeyValuePair<K, V>> ToKeyValuePairs<K, V>(this IEnumerable<SerializableKeyValuePair<K, V>> pairs)
        {
            if (pairs == null)
                return null;
            return pairs.Select(p => new KeyValuePair<K, V>(p.Key, p.Value));
        }

        public static IEnumerable<K> Keys<K, V>(this IEnumerable<SerializableKeyValuePair<K, V>> pairs)
        {
            if (pairs == null)
                return null;

            return pairs.Select(p => p.Key);
        }

        public static IEnumerable<V> Values<K, V>(this IEnumerable<SerializableKeyValuePair<K, V>> pairs)
        {
            if (pairs == null)
                return null;

            return pairs.Select(p => p.Value);
        }
    }
}
