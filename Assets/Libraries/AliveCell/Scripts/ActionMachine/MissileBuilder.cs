/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/21 11:36:48
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.AM;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// MissileBuilderConfig
    /// </summary>
    [Serializable]
    [ActionConfig(typeof(MissileBuilder))]
    public class MissileBuilderConfig : AttackConfig
    {
        public int prefabId;
        public BindPointType bindPoint;
        public Single radius;
        public Single moveSpeed;
        public Single lifeTime;
        public LayerMask obstacleMask;
    }

    /// <summary>
    /// MissileBuilder
    /// </summary>
    public class MissileBuilder : IActionHandler
    {
        //public class Data
        //{
        //}

        public void Enter(ActionNode node)
        {
            //MissileBuilderConfig config = (MissileBuilderConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //node.data = new Data();
        }

        public void Exit(ActionNode node)
        {
            MissileBuilderConfig config = (MissileBuilderConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
            UMissileBuilderEvent builder = controller.AppendEvent<UMissileBuilderEvent>();
            builder.config = config;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            //MissileBuilderConfig config = (MissileBuilderConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}