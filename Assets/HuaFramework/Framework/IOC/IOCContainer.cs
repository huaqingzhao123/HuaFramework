using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.IOC
{
    public  class IOCContainer 
    {
        private Dictionary<Type, object> _contains = new Dictionary<Type, object>();
        public virtual void Register<T>(T instance)
        {
            var type = typeof(T);
            if (!_contains.ContainsKey(type))
                _contains.Add(type,instance);
            else
            {
                _contains[type] = instance;
            }
        }
        public T Get<T>() where T:class
        {
            if (_contains.ContainsKey(typeof(T)))
                return _contains[typeof(T)] as T;
            return null;
        }   
        public void Clear()
        {
            _contains.Clear();
        }
    }

}
