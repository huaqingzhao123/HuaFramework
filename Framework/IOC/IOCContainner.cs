using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.IOC
{
    public  class IOCContainner 
    {
        private Dictionary<Type, object> _contains = new Dictionary<Type, object>();
        protected virtual void RegisterObject<T>(T instance) where T : new()
        {
            var type = typeof(T);
            if (!_contains.ContainsKey(type))
                _contains.Add(type,instance);
            else
            {
                _contains[type] = instance;
            }
        }
        protected T GetObjectt<T>() where T:class
        {
            if (_contains.ContainsKey(typeof(T)))
                return _contains[typeof(T)] as T;
            return null;
        }   
    }

}
