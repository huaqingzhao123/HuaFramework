using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Interface
{
    public interface IPool<T>
    {
        T SpawnObject();
        bool UnSpawnObject(T obj);
    }

}
