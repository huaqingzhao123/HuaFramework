/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/30 13:03:34
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
    /// HitInjuredConfig
    /// </summary>
    [ActionConfig(typeof(HitInjured))]
    [Serializable]
    public class HitInjuredConfig
    {
        public string stateName;
        public int priority;
    }

    /// <summary>
    /// HitInjured
    /// </summary>
    public class HitInjured : IActionHandler
    {
        public class Data
        {
            public int maxImpactCnt;
        }

        public void Enter(ActionNode node)
        {
            //HitInjuredConfig config = (HitInjuredConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;

            InjuredInfos infos = controller.datas.GetValue<InjuredInfos>(DataTag.InjuredInfos, null);
            node.data = new Data() { maxImpactCnt = infos.maxImpactFrameCnt };
            infos?.Clear();
        }

        public void Exit(ActionNode node)
        {
            //HitInjuredConfig config = (HitInjuredConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            HitInjuredConfig config = (HitInjuredConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            Data data = (Data)node.data;

            if (Mathf.Abs(controller.velocity.x) < Single.C0p01 && Mathf.Abs(controller.velocity.z) < Single.C0p01 || (data.maxImpactCnt > 0 && (node.updateCnt + 1) >= data.maxImpactCnt))
            {
                machine.ChangeState(config.stateName, config.priority);
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}