using System;
using System.Collections.Generic;

namespace Nireus
{
    public class ListComponent<T> : List<T>, IDisposable
    {
        public static ListComponent<T> Create()
        {
            return MonoPool.Instance.Fetch(typeof(ListComponent<T>)) as ListComponent<T>;
        }
        public static ListComponent<T> Create(in List<T> list)
        {
            var newList = MonoPool.Instance.Fetch(typeof(ListComponent<T>)) as ListComponent<T>;
            newList.AddRange(list);
            return newList;
        }
        public void Dispose()
        {
            this.Clear();
            MonoPool.Instance.Recycle(this);
        }
    }
}