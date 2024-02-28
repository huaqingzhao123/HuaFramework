using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nireus
{

    public class ClearManager : SingletonBehaviour<ClearManager>
    {
        private Dictionary<int, IClearListener> clearListenerDic = new Dictionary<int, IClearListener>();

        public void RegisterListener(int type ,IClearListener value)
        {
            if(clearListenerDic.ContainsKey(type))
            {
                clearListenerDic.Remove(type);
            }
            clearListenerDic.Add(type, value);
        }
        public void Clear()
        {
            foreach (var pair in clearListenerDic)
            {
                IClearListener listener = pair.Value;
                if (listener != null)
                {
                    listener.Clear();
                }
            }
        }
    }
}
