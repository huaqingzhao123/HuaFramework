using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Utility.ObjectPool
{
    public interface IPoolFactory<T>
    {
        T Create();
    }

}
