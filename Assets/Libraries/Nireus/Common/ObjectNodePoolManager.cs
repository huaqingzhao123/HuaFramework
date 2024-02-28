using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public class ObjectNodePoolManager : Singleton<ObjectNodePoolManager>
    {
        private Dictionary<string, ObjectNodePool> m_PoolDic;

        private Transform m_RootPoolTrans;

        public ObjectNodePoolManager()
        {
            m_PoolDic = new Dictionary<string, ObjectNodePool>();

            // 根对象池
            GameObject go = new GameObject("ObjcetNodePoolManager");
            m_RootPoolTrans = go.transform;
        }

        // 创建一个新的对象池
        public T CreateObjectPool<T>(string poolName) where T : ObjectNodePool, new()
        {
            if (m_PoolDic.ContainsKey(poolName))
            {
                return m_PoolDic[poolName] as T;
            }

            GameObject obj = new GameObject(poolName);
            obj.transform.SetParent(m_RootPoolTrans);
            T pool = new T();
            pool.Init(poolName, obj.transform);
            m_PoolDic.Add(poolName, pool);
            return pool;
        }

        public GameObject GetGameObject(string poolName, float lifetTime = 0.0f)
        {
            if (m_PoolDic.ContainsKey(poolName))
            {
                return m_PoolDic[poolName].Get(lifetTime);
            }
            return null;
        }

        public void RemoveGameObject(string poolName, GameObject go)
        {
            if (m_PoolDic.ContainsKey(poolName))
            {
                m_PoolDic[poolName].Remove(go);
            }
        }
    
        // 销毁所有对象池
        public void Destroy()
        {
            m_PoolDic.Clear();
            GameObject.Destroy(m_RootPoolTrans);

        }
    }

}