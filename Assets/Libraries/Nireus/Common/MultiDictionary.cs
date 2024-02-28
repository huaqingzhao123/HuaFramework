using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public class MultiDictionary<K1, K2, V> : Dictionary<K1, Dictionary<K2, V>>
    {
        public V this[K1 k1, K2 k2]
        {
            get
            {
                if (this.TryGetValue(k1, out Dictionary<K2, V> dic))
                {
                    if(dic.TryGetValue(k2, out V v))
                    {
                        return v;
                    }
                }
                return default(V);
            }
        }
        public bool TryGetDic(K1 k1, out Dictionary<K2, V> dic)
        {
            return this.TryGetValue(k1, out dic);
        }
        public bool TryGetValue(K1 k1, K2 k2, out V v)
        {
            v = default;
            if(!this.TryGetValue(k1, out Dictionary<K2, V> dic))
            {
                return false;
            }
            return dic.TryGetValue(k2, out v);
        }
        public bool ContainsKey(K1 k1, K2 k2)
        {
            this.TryGetValue(k1, out Dictionary<K2, V> dic);
            if(dic == null)
            {
                return false;
            }
            return dic.ContainsKey(k2);
        }
        public bool ContainsValue(K1 k1, V v)
        {
            this.TryGetValue(k1, out Dictionary<K2, V> dic);
            if (dic == null)
            {
                return false;
            }
            return dic.ContainsValue(v);
        }
        public bool ContainsValue(K1 k1, K2 k2, V v)
        {
            this.TryGetValue(k1, out Dictionary<K2, V> dic);
            if(dic == null)
            {
                return false;
            }
            if(!dic.ContainsKey(k2))
            {
                return false;
            }
            return dic.ContainsValue(v);
        }
        public void Add(K1 k1, K2 k2, V v)
        {
            this.TryGetValue(k1, out Dictionary<K2, V> dic);
            if(dic == null)
            {
                dic = new Dictionary<K2, V>();
                this[k1] = dic;
            }
            if(!dic.ContainsKey(k2))
            {
                dic.Add(k2, v);
            }
        }
        public bool Remove(K1 k1, K2 k2)
        {
            this.TryGetValue(k1, out Dictionary<K2, V> dic);
            if(dic == null || !dic.Remove(k2))
            {
                return false;
            }
            if(dic.Count == 0)
            {
                this.Remove(k1);
            }
            return true;
        }
        public void ForEach(Action<V> action)
        {
            if (action == null)
            {
                return;
            }
            foreach (Dictionary<K2, V> dic in Values)
            {
                if (dic == null) continue;
                foreach (var v in dic.Values)
                {
                    action(v);
                }
            }
        }
        /// <summary>
        /// 过滤第一层key
        /// </summary>
        /// <param name="filter1"></param>
        /// <returns></returns>
        public IEnumerable<K2> ForEach(Func<K1, bool> filter1)
        {
            foreach (K1 k1 in Keys)
            {
                if (filter1 == null || filter1.Invoke(k1))
                {
                    if (this[k1] == null)
                        continue;
                    foreach (var k2 in this[k1].Keys)
                    {
                        yield return k2;
                    }
                }
                continue;
            }
        }
        /// <summary>
        /// 过滤第一层和第二层key
        /// </summary>
        /// <param name="filter1"></param>
        /// <returns></returns>
        public IEnumerable<V> ForEach(Func<K1, bool> filter1, Func<K2, bool> filter2)
        {
            foreach (K1 k1 in Keys)
            {
                if (filter1 == null || filter1.Invoke(k1))
                {
                    if (this[k1] == null)
                        continue;
                    foreach (var k2v in this[k1])
                    {
                        if (filter2 == null || filter2.Invoke(k2v.Key))
                        {
                            yield return k2v.Value;
                        }
                    }
                }
                continue;
            }
        }
    }
}