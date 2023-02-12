using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Nireus
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static bool applicationIsQuitting = false;

        private static object _lock = new object();

        public static T Instance {
            get { return getInstance();  }
        }

        public static T getInstance()
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();
                        singleton.GetComponent<SingletonBehaviour<T>>().InitInstance();
                    }
                }

                return _instance;
            }
        }

        void InitInstance()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void OnDestroy()
        {
            applicationIsQuitting = true;
        }
    }

}