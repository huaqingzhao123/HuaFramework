using HuaFramework.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Utility
{
    /// <summary>
    /// 池子基类
    /// </summary>
    public abstract class ObjectPool<T> : IPool<T>
    {
        protected Stack<T> _objectCaches = new Stack<T>();
        protected IPoolFactory<T> _poolFactory;
        public int CurCount
        {
            get { return _objectCaches.Count; }
        }
        public T SpawnObject()
        {
            return _objectCaches.Count > 0 ? _objectCaches.Pop() : _poolFactory.Create();
        }
        public abstract bool UnSpawnObject(T obj);
    }
}

