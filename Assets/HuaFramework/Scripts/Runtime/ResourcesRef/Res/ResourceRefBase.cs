using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesRef {
    public class ResourceRefBase : IResourceRefCount
    {
        public int RefCount { get; private set; }

       /// <summary>
       /// 加载资源
       /// </summary>
       /// <param name="asset"></param>
        public void RetainAssets(object asset = null)
        {
            RefCount++;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="asset"></param>
        public void ReleaseAssets(object asset = null)
        {
            RefCount--;
            if (RefCount == 0)
            {
                OnZeroRef();
            }
        }

        /// <summary>
        /// 引用为空时的操作
        /// </summary>
        protected virtual void OnZeroRef()
        {

        }
    }

}

