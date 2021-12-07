using HuaFramework.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Utility
{
    public class ObjectPoolFactory<T> : IPoolFactory<T>
    {
        Func<T> _createFunc;
        public ObjectPoolFactory(Func<T> createMethod)
        {
            _createFunc = createMethod;
        }
        public T Create()
        {
            return _createFunc();
        }
    }

}
