using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuaFramework.IOC
{

    public interface IClassicIOC
    {

        void Register<T>();
        void RegisterInstance<T>(object instance);
        void RegisterInstance(object instance);
        void Register<TBase, TConcrete>() where TConcrete : TBase;

        T Resolve<T>();
        void Inject(object obj);
        void Clear();

    }

    public class Inject : Attribute
    {

    }
    public class ClassicIOCContainer : IClassicIOC
    {
        private HashSet<Type> _registeredType = new HashSet<Type>();
        private Dictionary<Type, object> _registeredInstance = new Dictionary<Type, object>();
        private Dictionary<Type, Type> _registeredDependency = new Dictionary<Type, Type>();


        public void Register<T>()
        {
            _registeredType.Add(typeof(T));
        }

        public void Register<TBase, TConcrete>() where TConcrete : TBase
        {
            var baseType = typeof(TBase);
            var concreteType = typeof(TConcrete);
            _registeredDependency[baseType] = concreteType;
        }


        public void RegisterInstance<T>(object instance)
        {
            var type = instance.GetType();
            _registeredInstance[type] = instance;
        }

        public void RegisterInstance(object instance)
        {
            var type = instance.GetType();
            _registeredInstance[type] = instance;
        }

        public T Resolve<T>()
        {
            var type = typeof(T);
            return (T)Resolve(type) ;
        }

        object Resolve(Type type)
        {
            if (_registeredDependency.ContainsKey(type))
                return Activator.CreateInstance(_registeredDependency[type]);
            if (_registeredInstance.ContainsKey(type))
                return _registeredInstance[type];
            if (_registeredType.Contains(type))
            {
                return Activator.CreateInstance(type);
            }
                return default;
        }

        public void Inject(object obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties()
                .Where(property=>property.GetCustomAttributes(typeof(Inject),true).Any()))
            {
                var instance = Resolve(propertyInfo.PropertyType);
                if (instance != null)
                {
                    propertyInfo.SetValue(obj,instance);
                }
                else
                {
                    Debug.LogErrorFormat("不能获取到类型为:{0}的对象", propertyInfo.PropertyType);
                }
            }
        }

        public void Clear()
        {
            _registeredDependency.Clear();
            _registeredInstance.Clear();
            _registeredType.Clear();
        }
    }

 

}
