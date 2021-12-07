using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Interface
{
    public interface IPoolFactory<T>
    {
        T Create();
    }

}
