/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/12/23 13:22:19
 */

using AliveCell.Commons;
using System;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// UEventSystem
    /// </summary>
    public class UEventSystem : ISystem, ILogicUpdate
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        public readonly List<UEvent> events = new List<UEvent>(32);

        public readonly ObjectPool<Type> objPool = new ObjectPool<Type>();

        public UEventSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UEvent");
        }

        public T Append<T>(int id) where T : UEvent, new()
        {
            T obj = objPool.Pop(typeof(T)) as T;
            if (obj == null)
            {
                obj = new T();
            }

            obj.ID = id;
            obj.world = world;

            events.Add(obj);

            return obj;
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            if (events.Count > 0)
            {
                foreach (var evt in events)
                {
                    evt.Execute();//执行
                    objPool.Push(evt);//回收
                }

                events.Clear();
            }
        }
    }
}