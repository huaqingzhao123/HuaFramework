using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Nireus
{
    public class Singleton<T> where T : class, new()
    {
        private static List<object> singleton_list = new List<object>();

        private static T _instance;

        private static bool applicationIsQuitting = false;

        private static object _lock = new object();

        public static T Instance
        {
            get { return getInstance(); }
        }

        public static T getInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = new T();
                    singleton_list.Add(_instance);
                }
            }

            return _instance;
        }

        public virtual void ReleaseInstance()
        {
            singleton_list.Remove(_instance);
            _instance = null;
        }

        public static void ClearAllInstance()
        {
            foreach (var item in singleton_list)
            {
                var k = item.GetType();
                var m = k.GetMethod("ReleaseInstance");
                m.Invoke(item, null);
            }
        }

    }
}
