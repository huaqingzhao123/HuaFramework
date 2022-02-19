using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Utility.ObjectPool
{
    public class SimpleObjectPool<T> : ObjectPool<T>
    {
        Action<object> _resetMethod;
        public SimpleObjectPool(Func<T> createMethod,Action<object> resetMethod =null,int poolCount=10 )
        {
            _poolFactory = new ObjectPoolFactory<T>(createMethod);
            for (int i = 0; i < poolCount; i++)
            {
                _objectCaches.Push(_poolFactory.Create());
            }
            _resetMethod = resetMethod;
        }
        public override bool UnSpawnObject(T obj)
        {
            if (obj != null)
            {
                if (_resetMethod != null)
                    _resetMethod.Invoke(obj);
            }
            _objectCaches.Push(obj);
            return true;
        }
    }

}
