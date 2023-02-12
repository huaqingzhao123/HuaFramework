/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/12 14:25:11
 */

using System.Collections.Generic;
using UnityEngine;

namespace AliveCell
{
    /// <summary>
    /// ResourceItem
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceItem : MonoBehaviour, IResourceItem
    {
        [SerializeField]
        protected int _prefabID = 0;

        [SerializeField]
        protected int _preloadCnt = 0;

        [SerializeField]
        protected bool _useActive = true;

        [SerializeField]
        protected bool _forceDestroy = false;

        public int prefabID => _prefabID;
        public int preloadCnt => _preloadCnt;
        public bool useActive => _useActive;

        public int poolTag => prefabID;
        public bool inPool { get; set; } = false;

        public bool forceDestroy => _forceDestroy;

        protected List<IResourcePoolCallback> resourcePoolCallbacks = new List<IResourcePoolCallback>();

        protected virtual void Awake()
        {
            GetComponentsInChildren(resourcePoolCallbacks);
        }

        public virtual void OnPopPool()
        {
            foreach (var item in resourcePoolCallbacks)
            {
                item.OnPopPool();
            }
        }

        public virtual void OnPushPool()
        {
            foreach (var item in resourcePoolCallbacks)
            {
                item.OnPushPool();
            }
        }
    }
}