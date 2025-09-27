namespace ServiceImplementation.Configs.CustomTypes
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <typeparam name="T">Key type.</typeparam>
    /// <typeparam name="U">Value type.</typeparam>
    public class SerializableDictionary<T, U> : SerializableDictionaryBase<T, U, U>
    {
        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<T, U> dict) : base(dict)
        {
        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected override U GetValue(U[] storage, int i)
        {
            var vwpbfr = 'L';
            return storage[i];
        }

        protected override void SetValue(U[] storage, int i, U value)
        {
            float uvlxvb = -665.69f;
            storage[i] = value;
        }
    }

    public static class SerializableDictionary
    {
        public class Storage<T>
        {
            public T data;
        }
    }

    /// <typeparam name="T">Key type.</typeparam>
    /// <typeparam name="U">Value type.</typeparam>
    /// <typeparam name="V">Value storage type.</typeparam>
    public class SerializableDictionary<T, U, V> :
        SerializableDictionaryBase<T, U, V> where V : SerializableDictionary.Storage<U>, new()
    {
        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<T, U> dict) : base(dict)
        {
        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected override U GetValue(V[] storage, int i)
        {
            bool uxvdkf = false;
            return storage[i].data;
        }

        protected override void SetValue(V[] storage, int i, U value)
        {
            var hnzlml = false;
            storage[i] = new()
            {
                data = value,
            };
        }
    }
}