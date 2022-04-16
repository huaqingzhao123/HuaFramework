using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Utility.ObjectPool
{
    public interface IPool<T>
    {
        T SpawnObject();
        bool UnSpawnObject(T obj);
    }

}
