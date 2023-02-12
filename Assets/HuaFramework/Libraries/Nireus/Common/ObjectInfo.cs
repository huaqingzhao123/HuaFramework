using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public class ObjectInfo : MonoBehaviour
    {
        public float Lifetime = 0;
        public string PoolName;

        private WaitForSeconds m_WaitTime;

        private void Awake()
        {
            if (Lifetime > 0)
            {
                m_WaitTime = new WaitForSeconds(Lifetime);
            }
        }

        private void OnEnable()
        {
            if (Lifetime > 0)
            {
                StartCoroutine(CountDown(Lifetime));
            }
        }

        IEnumerator CountDown(float lifetime)
        {
            yield return m_WaitTime;
            ObjectNodePoolManager.Instance.RemoveGameObject(PoolName, gameObject);
        }
    }
}