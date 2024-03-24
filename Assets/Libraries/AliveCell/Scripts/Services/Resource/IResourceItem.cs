/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/9/17 0:34:56
 */

using System;

using UnityEngine;
using XMLib;

namespace AliveCell
{
    public interface IResourcePoolCallback : IPoolCallback { }

    public interface IResourcePoolItem : IPoolItem<int> { }

    /// <summary>
    /// IResourceItem
    /// </summary>
    public interface IResourceItem : IResourcePoolItem
    {
        int prefabID { get; }
        int preloadCnt { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
        bool useActive { get; }

        bool forceDestroy { get; }

        Component GetComponent(Type type);

        T GetComponent<T>();

        Component GetComponentInChildren(Type t, bool includeInactive);

        Component GetComponentInChildren(Type t);

        T GetComponentInChildren<T>(bool includeInactive);

        T GetComponentInChildren<T>();
    }
}