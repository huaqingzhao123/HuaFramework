using System;
using System.Collections.Generic;

namespace Nireus
{
    public class HashSetComponent<T> : HashSet<T>, IDisposable
    {
        public static HashSetComponent<T> Create()
        {
            return MonoPool.Instance.Fetch(typeof(HashSetComponent<T>)) as HashSetComponent<T>;
        }
        public static HashSetComponent<T> Create(in HashSet<T> set)
        {
            var newSet = MonoPool.Instance.Fetch(typeof(HashSetComponent<T>)) as HashSetComponent<T>;
            newSet.UnionWith(set);
            return newSet;
        }
        public void Dispose()
        {
            this.Clear();
            MonoPool.Instance.Recycle(this);
        }
    }
}
