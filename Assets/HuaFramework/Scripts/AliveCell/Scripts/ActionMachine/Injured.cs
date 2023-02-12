/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/26 22:07:12
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
using System.Linq;

namespace AliveCell
{
    /// <summary>
    /// 伤害信息
    /// </summary>
    public class InjuredInfo
    {
        public AttackConfig config;
        public AttackerInfo info;
    }

    /// <summary>
    /// 受伤结果
    /// </summary>
    public class InjuredResult
    {
        /// <summary>
        /// 受伤者id
        /// </summary>
        public int id;

        /// <summary>
        /// 原有血量
        /// </summary>
        public int oldHp;

        /// <summary>
        /// 减少的血量
        /// </summary>
        public int loseHp;

        /// <summary>
        /// 攻击倍数
        /// </summary>
        public int attackScale;

        /// <summary>
        /// 最大攻击倍数
        /// </summary>
        public int maxAttackScale;

        public override string ToString() => $"[InjuredResult]id:{id},loseHp:{loseHp},attackScale:{attackScale},maxAttackScale:{maxAttackScale}";
    }

    public class InjuredInfos : List<InjuredInfo>
    {
        public int maxImpactFrameCnt => this.Max(t => t.config.impactFrameCnt);
    }

    public class AttackerInfo
    {
        /// <summary>
        /// 攻击者ID
        /// </summary>
        public int id;

        /// <summary>
        /// 攻击力
        /// </summary>
        public int attack;
    }

    /// <summary>
    /// InjuredConfig
    /// </summary>
    [ActionConfig(typeof(Injured))]
    [Serializable]
    public class InjuredConfig
    {
        //受击
        public string hitted;

        //浮空
        public string levitate;

        //击飞
        public string blowUp;

        //死亡
        public string dead;

        public int priority;
    }

    /// <summary>
    /// Injured
    /// </summary>
    public class Injured : IActionHandler
    {
        //public class Data
        //{
        //}

        public void Enter(ActionNode node)
        {
            //InjuredConfig config = (InjuredConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            //node.data = new Data();

            controller.datas.GetOrCreateValue<InjuredInfos>(DataTag.InjuredInfos).Clear();
        }

        public void Exit(ActionNode node)
        {
            //InjuredConfig config = (InjuredConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            InjuredConfig config = (InjuredConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;

            InjuredInfos injuredInfos = controller.datas.GetOrCreateValue<InjuredInfos>(DataTag.InjuredInfos);

            controller.datas[DataTag.InjuredInFrame] = injuredInfos.Count != 0;

            if (string.Compare(machine.stateName, config.dead) == 0 ||//
                string.Compare(machine.stateName, config.levitate) == 0)//当前是浮空状态，则交由浮空状态处理
            {
                return;
            }

            if (controller.hp == 0 && controller.CheckGround())
            {
                //切换到死亡状态
                machine.ChangeState(config.dead, config.priority);
                //
                injuredInfos.Clear();//清理受伤信息
                controller.AppendEvent<UDeadEvent>();
                return;
            }

            if (injuredInfos.Count == 0)
            {
                return;
            }

            InjuredInfo info = injuredInfos[0];

            switch (info.config.attackMode)
            {
                case AttackMode.Hitted:
                    machine.ChangeState(config.hitted, config.priority, info.config.attackDir.x < 0 ? 0 : 1);
                    break;

                case AttackMode.BlowUp:
                    machine.ChangeState(config.blowUp, config.priority);
                    break;

                case AttackMode.Levitate:
                    machine.ChangeState(config.levitate, config.priority);
                    break;
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}