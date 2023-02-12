using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//example ： public class SerializeableDictionaryStringAndUIBase : SerializableDictionary<string, UIBase> { }
// [SerializeField]
// SerializeableDictionaryStringAndUIBase drct;
namespace Nireus
{
    public abstract class SerializableDictionary<K, V> : Dictionary<K, V>,ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<K> keys = new List<K>();
        [SerializeField]
        private List<V> values = new List<V>();
        
        public void OnAfterDeserialize()
        {
            var c = keys.Count;
            this.Clear();
            for (int i = 0; i < c; i++)
            {
                this.Add(keys[i],values[i]);
            }
            keys.Clear();
            values.Clear();
        }

        public void OnBeforeSerialize()
        {
            var c = this.Count;
            keys.Clear();
            values.Clear();
            using (var e = this.GetEnumerator())
                while (e.MoveNext())
                {
                    var kvp = e.Current;
                    keys.Add(kvp.Key);
                    values.Add(kvp.Value);
                }
        }
    }
}