/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/28 12:43:06
 */

using AliveCell.Commons;
using System;
using System.Collections.Generic;

using UnityEngine;
using XMLib;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// UViewSystem
    /// </summary>
    public class UViewSystem : ISystem, IUpdate, ICreate, IDestroy, ILogicUpdate
    {
        private ResourceService res => App.res;
        private readonly LinkedList<int> objs = new LinkedList<int>();
        private readonly Dictionary<int, IAssetView> id2views = new Dictionary<int, IAssetView>();

        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        public UViewSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UView");
        }

        public IAssetView GetView(int id)
        {
            return id2views.TryGetValue(id, out IAssetView view) ? view : null;
        }

        public T GetView<T>(int id) where T : class
        {
            return GetView(id) as T;
        }

        public void Bind<T>(T obj) where T : IAssetObject
        {
            GameObject go = res.CreateGO(obj.prefabID);
            IAssetView view = go.GetComponent<IAssetView>();
            if (null == view)
            {
                throw new RuntimeException($"{go.name}[{obj.prefabID}] 预制上没有 {typeof(IAssetView)} 脚本");
            }

            view.system = this;

            objs.AddLast(obj.ID);
            id2views.Add(obj.ID, view);

            view.objID = obj.ID;

            view.transform.position = obj.position;
#if UNITY_EDITOR
            view.name = $"[{obj.ID}]{obj.GetType().Name}(View)";
#endif

            obj.OnViewBind();
            view.OnViewBind();
        }

        public void Unbind<T>(T obj) where T : IAssetObject
        {
            if (obj == null || !id2views.TryGetValue(obj.ID, out IAssetView view) || null == view)
            {
                return;
            }

            view.OnViewUnbind();
            obj.OnViewUnbind();

            view.objID = UObjectSystem.noneID;
            view.system = null;

            objs.Remove(obj.ID);
            id2views.Remove(obj.ID);

            res.DestroyGO(view);
        }

        protected void UnBind(int id)
        {
            IAssetObject target = world.uobj.Get(id) as IAssetObject;
            if (target == null)
            {
                return;
            }

            Unbind(target);
        }

        public void OnInitialize(List<UObject> preObjs)
        {
            foreach (var obj in preObjs)
            {
                if (obj is IAssetObject assetObj)
                {
                    Bind(assetObj);
                }
            }
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var id in objs)
            {
                IAssetObject target = world.uobj.Get(id) as IAssetObject;
                if (target == null)
                {
                    continue;
                }

                id2views[id].UpdateView(deltaTime);
            }
        }

        private void UpdateItem()
        {
            foreach (var item in world.uobj.ForeachNew<IAssetObject>())
            {
                Bind(item);
            }

            foreach (var item in world.uobj.ForeachDestroyed<IAssetObject>())
            {
                Unbind(item);
            }
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            UpdateItem();
            foreach (var id in objs)
            {
                IAssetObject target = world.uobj.Get(id) as IAssetObject;
                if (target.isDestroyed)
                {
                    continue;
                }
                id2views[id].LogicUpdateView(deltaTime);
            }
        }

        public void OnCreate()
        {
        }

        public void OnDestroy()
        {
            List<int> ids = new List<int>(objs);
            ids.Reverse();

            foreach (var id in ids)
            {
                UnBind(id);
            }
        }
    }
}