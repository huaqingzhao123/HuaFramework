using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HuaFramework.Singleton
{
    public class Singleton<T> : ISingleton where T : Singleton<T>
    {
        private static T _instace;
        private static UnityEngine.Object _mLock = new UnityEngine.Object();
        public static T Instace
        {
            get
            {
                lock (_mLock)
                {
                    if (_instace == null)
                    {
                        //通过反射构造
                        _instace = SingletonCreator.CreatSingleton<T>();
                    }
                    return _instace;
                }
            }

        }
        public virtual void Dispose()
        {
            _instace = null;
        }

        public virtual void SingletonInit()
        {

        }
    }

    public static class SingletonCreator
    {

        public static T CreatSingleton<T>() where T : class, ISingleton
        {
            //通过反射构造
            var constructorInfo = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var construct = Array.Find(constructorInfo, csr => csr.GetParameters().Length == 0);
            if (construct == null)
                throw new Exception(string.Format("类型:{0}不存在无参私有构造函数!检查!", typeof(T).ToString()));
            var instance = construct.Invoke(null) as T;
            instance.SingletonInit();
            return instance;
        }

    }

    public interface ISingleton
    {

        void SingletonInit();
    }


}
