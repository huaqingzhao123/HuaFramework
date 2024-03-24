/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/25 9:44:13
 */

using AliveCell.Commons;
using System;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.AM;

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
    /// UTestSystem
    /// </summary>
    public class UTestSystem : ISystem, ILogicUpdate, IUpdate, ICreate, IDestroy
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        public UTestSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UTest");
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnCreate()
        {
        }

        public void OnDestroy()
        {
        }

        public void OnLogicUpdate(Single deltaTime)
        {
        }

        public void OnUpdate(float deltaTime)
        {
        }
    }
}