/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/29 22:40:43
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

using MathUtility = FPPhysics.FPUtility;

namespace AliveCell
{
    /// <summary>
    /// LevitateInjuredConfig
    /// </summary>
    [ActionConfig(typeof(LevitateInjured))]
    [Serializable]
    public class LevitateInjuredConfig
    {
        public string stateName;
        public int priority;
        public int resetGravityFrameCnt;
        public int holdFrameCnt;

        [Range(0, 1)]
        public Single dumpScale;
    }

    /// <summary>
    /// LevitateInjured
    /// </summary>
    public class LevitateInjured : IActionHandler
    {
        public class Data
        {
            public InjuredInfo info;
            public Single? realOldGravity;
            public Single oldGravity;
            public Single oldMass;
            public int stopAtTheHighestIndex;
            public bool useStopAtTheHighest;
            public int levitateCnt;
        }

        public void Enter(ActionNode node)
        {
            //LevitateInjuredConfig config = (LevitateInjuredConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            node.data = new Data()
            {
                realOldGravity = controller.gravity,
                oldGravity = controller.gravity ?? controller.gravityDefault,
                oldMass = controller.mass,
                useStopAtTheHighest = true
            };
        }

        public void Exit(ActionNode node)
        {
            //LevitateInjuredConfig config = (LevitateInjuredConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            Data data = (Data)node.data;

            controller.gravity = data.realOldGravity;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            LevitateInjuredConfig config = (LevitateInjuredConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            Data data = (Data)node.data;

            InjuredInfos infos = controller.datas.GetOrCreateValue<InjuredInfos>(DataTag.InjuredInfos);
            if (infos.Count > 0)
            {
                InjuredInfo info = infos[0];
                infos.Clear();

                data.info = info;

                Single upHeigth = 0;
                var lvConfig = info.config.GetExtra<Attacks.LevitateConfig>();
                if (lvConfig != null)
                {
                    TObject attacker = App.game.uobj.Get(info.info.id) as TObject;
                    upHeigth = (Single)lvConfig.upHeight * Mathf.Pow((Single)config.dumpScale, data.levitateCnt);
                    if (attacker != null)
                    {
                        upHeigth = (Single)lvConfig.upHeight + attacker.position.y - controller.position.y;
                    }
                    //upHeigth = upHeigth > 0 ? upHeigth : 0;
                }

                data.useStopAtTheHighest = true;
                data.levitateCnt++;

                machine.ReplayAnim();

                controller.gravity = data.oldGravity;
                controller.velocity = upHeigth > 0 ? Vector3.up * Mathf.JumpSpeed(data.oldGravity * data.oldMass, upHeigth) : Vector3.zero;

                Debug.Log($"LevitateConfig:{upHeigth}");
            }

            if (data.useStopAtTheHighest && controller.velocity.y <= 0)
            {
                data.useStopAtTheHighest = false;
                data.stopAtTheHighestIndex = node.updateCnt;
            }

            if (!data.useStopAtTheHighest)
            {
                Single gravity = data.oldGravity;

                int deltaFrame = node.updateCnt - data.stopAtTheHighestIndex;
                int deltaFrame2 = deltaFrame - config.holdFrameCnt;
                if (deltaFrame < config.holdFrameCnt)
                {
                    gravity = 0;
                    controller.velocity = Vector3.zero;
                }
                else if (config.resetGravityFrameCnt > 0 &&
                        deltaFrame2 < config.resetGravityFrameCnt)
                {
                    gravity = (deltaFrame2 + 1) / (Single)config.resetGravityFrameCnt * data.oldGravity;
                }

                controller.gravity = gravity;

                if (controller.CheckGround())
                {
                    Debug.Log("CheckGround true");
                    machine.ChangeState(config.stateName, config.priority);
                }
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}