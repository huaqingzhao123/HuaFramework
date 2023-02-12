using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public class UnOrderMultiMap<K, T> : Dictionary<K, List<T>>
    {
        private readonly List<T> Empty = new List<T>();
        public void Add(K k, T t)
        {
            this.TryGetValue(k, out var list);
            if (list == null)
            {
                list = new List<T>();
                list.Clear();
                base[k] = list;
            }
            list.Add(t);
        }
        public bool Remove(K k, T t)
        {
            this.TryGetValue(k, out var list);
            if (list == null)
            {
                return false;
            }
            if (!list.Remove(t))
            {
                return false;
            }
            if (list.Count == 0)
            {
                base.Remove(k);
            }
            return true;
        }
        public new bool Remove(K k)
        {
            this.TryGetValue(k, out var list);
            if (list == null)
            {
                return false;
            }
            base.Remove(k);
            list.Clear();
            return true;
        }
        /// <summary>
		/// 不返回内部的list,copy一份出来
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public T[] GetAll(K k)
        {
            this.TryGetValue(k, out var list);
            if (list == null)
            {
                return Array.Empty<T>();
            }
            return list.ToArray();
        }
        /// <summary>
		/// 返回内部的list
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public new List<T> this[K k]
        {
            get
            {
                if (this.TryGetValue(k, out var list))
                {
                    return list;
                }
                return this.Empty;
            }
        }
        public bool Contains(K k, T t)
        {
            this.TryGetValue(k, out var list);
            if (list == null)
            {
                return false;
            }
            return list.Contains(t);
        }
    }
}