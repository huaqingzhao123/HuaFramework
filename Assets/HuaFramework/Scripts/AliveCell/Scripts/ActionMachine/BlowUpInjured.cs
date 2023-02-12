/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/16 13:13:02
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.AM;
using XMLib.Extensions;

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
    /// BlowUpInjuredConfig
    /// </summary>
    [Serializable]
    [ActionConfig(typeof(BlowUpInjured))]
    public class BlowUpInjuredConfig
    {
        public string stateName;
        public List<Single> reboundScale;
    }

    /// <summary>
    /// BlowUpInjured
    /// </summary>
    public class BlowUpInjured : IActionHandler
    {
        public class Data
        {
            public int blowUpCnt;
            public Vector3 initVelocity;
        }

        public void Enter(ActionNode node)
        {
            //BlowUpInjuredConfig config = (BlowUpInjuredConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            node.data = new Data()
            {
                blowUpCnt = 0,
                initVelocity = controller.velocity
            };
        }

        public void Exit(ActionNode node)
        {
            //BlowUpInjuredConfig config = (BlowUpInjuredConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            BlowUpInjuredConfig config = (BlowUpInjuredConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            Data data = (Data)node.data;

            InjuredInfos infos = controller.datas.GetOrCreateValue<InjuredInfos>(DataTag.InjuredInfos);
            if (infos.Count > 0)
            {
                infos.Clear();
            }

            if (!controller.CheckGround())
            {
                return;
            }

            if (data.blowUpCnt >= config.reboundScale.Count)
            {
                machine.ChangeState(config.stateName);
                return;
            }

            Vector3 reboundVelocity = data.initVelocity * (Single)config.reboundScale[data.blowUpCnt];
            data.blowUpCnt++;
            controller.velocity = reboundVelocity;
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}