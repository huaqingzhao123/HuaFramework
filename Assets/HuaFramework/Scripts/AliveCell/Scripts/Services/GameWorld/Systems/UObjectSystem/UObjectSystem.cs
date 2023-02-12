/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/28 11:03:35
 */

using AliveCell.Commons;
using System;
using System.Collections;
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
    /// UObjectSystem
    /// </summary>
    public class UObjectSystem : ISystem, ILogicUpdate, IUpdate, IDestroy, ILateUpdate
    {
        public const int noneID = 0;

        /// <summary>
        /// 所有对象，包含已有，新添加，删除的所有对象
        /// </summary>
        private readonly LinkedList<UObject> objs = new LinkedList<UObject>();

        /// <summary>
        /// 所有对象的id映射
        /// </summary>
        private readonly Dictionary<int, UObject> id2Obj = new Dictionary<int, UObject>(256);

        private readonly ObjectPool<Type> objPool = new ObjectPool<Type>(128);

        private readonly LinkedList<int> logicUpdateIDs = new LinkedList<int>();
        private readonly LinkedList<int> updateIDs = new LinkedList<int>();
        private readonly LinkedList<int> lateUpdateIDs = new LinkedList<int>();

        private readonly List<int> preDestroyIDs = new List<int>(32);
        private readonly List<int> preNewIDs = new List<int>(32);

        private readonly List<int> lastDestroyedIDs = new List<int>(32);
        private readonly List<int> lastNewIDs = new List<int>(32);

        public int nextID { get; private set; } = noneID + 1;

        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;
        public bool isDirty { get; protected set; } = true;

        public UObjectSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UObject");
        }

        public int Count<T>() where T : UObject
        {
            int count = 0;
            foreach (var item in Foreach<T>())
            {
                count++;
            }
            return count;
        }

        public int Count<T>(Func<T, bool> checker) where T : UObject
        {
            int count = 0;
            foreach (var item in Foreach<T>())
            {
                count += (checker(item) ? 1 : 0);
            }
            return count;
        }

        public IEnumerable<T> Foreach<T>() where T : UObject
        {
            foreach (var obj in objs)
            {
                if (obj is T target && target != null)
                {
                    yield return target;
                }
            }
        }

        public IEnumerable<T> ForeachNew<T>() where T : class
        {
            foreach (var id in preNewIDs)
            {
                if (id2Obj[id] is T target)
                {
                    yield return target;
                }
            }
        }

        public IEnumerable<T> ForeachDestroyed<T>() where T : class
        {
            foreach (var id in preDestroyIDs)
            {
                if (id2Obj[id] is T target)
                {
                    yield return target;
                }
            }
        }

        public T PreCreate<T>(Action<T> onPreInitialize = null) where T : UObject, new()
        {
            T obj = NewObject<T>(false);

            onPreInitialize?.Invoke(obj);
            obj.OnInitialized();

            AddClassifyObject(obj);//创建后立即添加分类

            return obj;
        }

        public T Create<T>(Action<T> onPreInitialize = null) where T : UObject, new()
        {
            T obj = NewObject<T>();

            onPreInitialize?.Invoke(obj);
            obj.OnInitialized();

            isDirty = true;

            return obj;
        }

        public void Destroy(UObject obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.OnDestroyed();
            obj.isDestroyed = true;

            //添加到删除列表
            preDestroyIDs.Add(obj.ID);

            isDirty = true;
        }

        public void Destroy(int id)
        {
            UObject obj = Get(id);
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        public void DestroyAll()
        {
            foreach (var obj in objs)
            {
                Destroy(obj);
            }
        }

        /// <summary>
        /// 包含所有已有，新添加，新删除的对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UObject Get(int id)
        {
            UObject obj = id2Obj.TryGetValue(id, out obj) ? obj : null;
            return obj;
        }

        /// <summary>
        /// 不包含已删除的对象
        /// </summary>
        public T Get<T>(int id) where T : class
        {
            UObject obj = Get(id);
            if (obj == null)
            {
                return null;
            }
            return obj as T;
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnDestroy()
        {
            DestroyAll();
            ProcessObject();
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            //处理对象
            ProcessObject();

            IUObjectLogicUpdate target;
            foreach (var obj in logicUpdateIDs)
            {
                target = (IUObjectLogicUpdate)id2Obj[obj];
                if (target.isDestroyed)
                {
                    continue;
                }
                target.OnLogicUpdate(deltaTime);
            }
        }

        public void OnUpdate(float deltaTime)
        {
            IUObjectUpdate target;
            foreach (var obj in updateIDs)
            {
                target = (IUObjectUpdate)id2Obj[obj];
                if (target.isDestroyed)
                {
                    continue;
                }
                target.OnUpdate(deltaTime);
            }
        }

        public void OnLateUpdate(float deltaTime)
        {
            IUObjectLateUpdate target;
            foreach (var obj in lateUpdateIDs)
            {
                target = (IUObjectLateUpdate)id2Obj[obj];
                if (target.isDestroyed)
                {
                    continue;
                }
                target.OnLateUpdate(deltaTime);
            }
        }

        private int GenID()
        {
            return nextID++;
        }

        private void DeleteObject(int id)
        {
            UObject target = id2Obj[id];
            RemoveClassifyObject(target);

            objs.Remove(target);
            id2Obj.Remove(id);

            target.ID = noneID;
            target.world = null;

            objPool.Push(target);
        }

        private T NewObject<T>(bool isPreNew = true) where T : UObject, new()
        {
            T obj = objPool.Pop<T>(typeof(T));
            if ((object)obj == null)
            {
                obj = new T();
            }

            obj.isDestroyed = false;
            obj.ID = GenID();
            obj.world = world;

            if (isPreNew)
            {
                preNewIDs.Add(obj.ID);
            }

            id2Obj.Add(obj.ID, obj);
            objs.AddLast(obj);

            return obj;
        }

        private void AddClassifyObject(UObject obj)
        {
            if (obj is IUObjectLogicUpdate)
            {
                logicUpdateIDs.AddLast(obj.ID);
            }
            if (obj is IUObjectUpdate)
            {
                updateIDs.AddLast(obj.ID);
            }
            if (obj is IUObjectLateUpdate)
            {
                lateUpdateIDs.AddLast(obj.ID);
            }
        }

        private void RemoveClassifyObject(UObject obj)
        {
            if (obj is IUObjectLogicUpdate)
            {
                logicUpdateIDs.Remove(obj.ID);
            }
            if (obj is IUObjectUpdate)
            {
                updateIDs.Remove(obj.ID);
            }
            if (obj is IUObjectLateUpdate)
            {
                lateUpdateIDs.Remove(obj.ID);
            }
        }

        private void ProcessObject()
        {
            lastNewIDs.Clear();
            if (preNewIDs.Count > 0)
            {
                foreach (var obj in preNewIDs)
                {
                    AddClassifyObject(id2Obj[obj]);
                }
                lastNewIDs.AddRange(preNewIDs);
                preNewIDs.Clear();
            }

            lastDestroyedIDs.Clear();
            if (preDestroyIDs.Count > 0)
            {
                foreach (var obj in preDestroyIDs)
                {
                    DeleteObject(obj);
                }
                lastDestroyedIDs.AddRange(preDestroyIDs);
                preDestroyIDs.Clear();
            }

            isDirty = false;
        }
    }
}