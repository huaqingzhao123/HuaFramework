using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesRef
{

    public interface IResourceRefCount
    {
        int RefCount { get; }
        void RetainAssets(object asset = null);
        void ReleaseAssets(object asset = null);
    }

}

