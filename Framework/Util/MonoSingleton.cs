using HuaFramework.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.Singleton
{
    public class MonoSingleton<T> : MonoBehaviorSimplify, ISingleton where T : MonoSingleton<T>
    {
        private static object _mLock = new Object();
        private static T _instance;
        public static T Instance
        {
            get
            {
                lock (_mLock)
                {
                    if (_instance == null)
                    {
                        var instances = FindObjectsOfType<T>();
                        if (instances.Length > 0)
                        {
                            if (instances.Length > 1)
                                Debug.LogError("场景中类型" + typeof(T) + "的数量大于1，检查");
                            _instance = instances[0];
                        }
                        if (_instance == null)
                        {
                            GameObject gameObject = new GameObject("HuaFramework.MonoSingleton." + typeof(T).ToString());
                            _instance = gameObject.AddComponent<T>();
                            _instance.SingletonInit();
                            DontDestroyOnLoad(gameObject);
                        }
                    }
                }
                return _instance;
            }

        }

        public virtual void SingletonInit()
        {
        }

        protected override void OnBeforeDestroy()
        {
        }
    }

}
