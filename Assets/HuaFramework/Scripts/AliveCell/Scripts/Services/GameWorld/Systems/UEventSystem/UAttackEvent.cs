/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/12/23 16:29:02
 */

using System;
using UnityEngine;
using XMLib.AM;

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
    /// UAttackEvent
    /// </summary>
    public class UAttackEvent : UEvent
    {
        public InjuredInfo injuredInfo { get; set; }
        public int injuredId { get; set; }

        protected int attackId => injuredInfo.info.id;
        protected AttackConfig config => injuredInfo.config;
        protected AttackerInfo info => injuredInfo.info;

        public override void Reset()
        {
            base.Reset();
            injuredInfo = null;
            injuredId = UObjectSystem.noneID;
        }

        public override void Execute()
        {
            if (world.CheckCompleted() != GameState.Normal)
            {//游戏结束，不用执行
                return;
            }

            ActionMachineObject from = world.uobj.Get<ActionMachineObject>(attackId);
            ActionMachineObject to = world.uobj.Get<ActionMachineObject>(injuredId);

            //停顿
            from.machine.waitFrameCnt = config.waitFrameCnt;
            to.machine.waitFrameCnt = config.waitFrameCnt;
            // to.machine.waitFrameDelay = 1;

            if (config.eyeToEye)
            {
                Single angleY = (from.rotation.eulerAngles.y + 180) % 360;
                to.rotation = Quaternion.Euler(0, angleY, 0);
            }

            ExecuteVelocityConfig(from, to);
            ExecuteEffectConfig(from, to);
            ExecuteAddHpPowerConfig(from, to);

            to.datas.GetOrCreateValue<InjuredInfos>(DataTag.InjuredInfos).Add(injuredInfo);

            if (!to.isDead)
            {//已经死了，就不要再攻击了
             //执行攻击计算
                var result = to.Injured(injuredInfo);
                //触发消息
                App.Trigger(EventTypes.Game_Injured, result, injuredInfo);
            }
        }

        private void ExecuteAddHpPowerConfig(ActionMachineObject from, ActionMachineObject to)
        {
            Attacks.AddHpPowerConfig addHpPower = config.GetExtra<Attacks.AddHpPowerConfig>();
            if (addHpPower == null)
            {
                return;
            }

            if (addHpPower.hp != 0)
            {
                from.SetHP(from.hp + addHpPower.hp);
            }
            if (addHpPower.power != 0)
            {
                from.SetPower(from.power + addHpPower.power);
            }
        }

        private void ExecuteEffectConfig(ActionMachineObject from, ActionMachineObject to)
        {
            Attacks.EffectConfig effect = config.GetExtra<Attacks.EffectConfig>();
            if (effect == null)
            {
                return;
            }

            //从from的本地方向转换到to的本地方向

            Quaternion fixedRotation = Quaternion.FromToRotation(from.rotation * Vector3.forward, to.position - from.position);
            Vector3 dir = fixedRotation * from.localToWorldMatrix.rotation * (Vector3)config.attackDir;
            Vector3 dir2 = to.localToWorldMatrix.inverse.rotation * dir;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, dir2);//计算本地旋转

            world.Create<EffectObject>(effect.hitted, t =>
            {
                t.lifeTime = effect.hittedLifeTime;
                t.bindObjId = to.ID;
                t.localPosition = Vector3.zero;
                t.localRotation = rotation;
                t.bindPointType = effect.hitBindPoint;
                t.useObjRotation = true;
                t.updateTransform = true;
            });
        }

        private void ExecuteVelocityConfig(ActionMachineObject from, ActionMachineObject to)
        {
            Attacks.VelocityConfig velocity = config.GetExtra<Attacks.VelocityConfig>();

            if (velocity == null)
            {
                return;
            }

            if (velocity.clearVelocity)
            {
                to.velocity = Vector3.zero;
            }

            if (velocity.transportVelocity)
            {
                to.velocity = from.velocity;
            }

            if (velocity.appendForwardVelocity != 0)
            {
                Vector3 offset = to.position - from.position;
                offset.y = 0;
                to.velocity += velocity.appendForwardVelocity * offset.normalized;
            }

            if (velocity.appendUpVelocity != 0)
            {
                to.velocity += velocity.appendUpVelocity * Vector3.up;
            }
        }
    }
}