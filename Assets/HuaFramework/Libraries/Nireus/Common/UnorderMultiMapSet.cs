using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public class UnOrderMultiMapSet<K, T> : Dictionary<K, HashSet<T>>
    {
        public void Add(K k, T t)
        {
            this.TryGetValue(k, out var set);
            if (set == null)
            {
                set = new HashSet<T>();
                base[k] = set;
            }
            set.Add(t);
        }
        public bool Remove(K k, T t)
        {
            this.TryGetValue(k, out var set);
            if (set == null)
            {
                return false;
            }
            if (!set.Remove(t))
            {
                return false;
            }
            if (set.Count == 0)
            {
                base.Remove(k);
            }
            return true;
        }
        public new bool Remove(K k)
        {
            this.TryGetValue(k, out var set);
            if (set == null)
            {
                return false;
            }
            base.Remove(k);
            set.Clear();
            return true;
        }
       
		public new HashSet<T> this[K k]
        {
            get
            {
                if (!this.TryGetValue(k, out var set))
                {
                    return new HashSet<T>();
                }
                return set;
            }
        }
        public bool Contains(K k, T t)
        {
            this.TryGetValue(k, out var set);
            if (set == null)
            {
                return false;
            }
            return set.Contains(t);
        }
        public new int Count
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<K, HashSet<T>> kv in this)
                {
                    count += kv.Value.Count;
                }
                return count;
            }
        }
    }
}