/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/28 12:09:15
 */

using System;
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
    public class PlayerData
    {
        public int hp;
        public int power;
    }

    /// <summary>
    /// PlayerObject
    /// </summary>
    public class PlayerObject : ActionMachineObject
    {
        public string playerID { get; set; }

        public PlayerInfo info { get; set; }
        public PlayerData data { get; protected set; }

        public override int maxHp => info.maxHp;
        public override int maxPower => info.maxPower;

        public override int hp => data.hp;
        public override int power => data.power;

        public override void SetHP(int hp)
        {
            int oldHp = data.hp;
            data.hp = Mathf.Clamp(hp, 0, info.maxHp);
            TriggerPropertyChanged(PropertyType.Hp, oldHp, data.hp);
        }

        public override void SetPower(int power)
        {
            int oldPower = data.power;
            data.power = Mathf.Clamp(power, 0, info.maxPower);
            TriggerPropertyChanged(PropertyType.Power, oldPower, data.power);
        }

        public override void OnReset()
        {
            base.OnReset();

            playerID = string.Empty;
            info = null;
            data = null;
        }

        public override InjuredResult Injured(InjuredInfo info)
        {
            InjuredResult result = base.Injured(info);

            int attackHp = info.info.attack;

            result.maxAttackScale = 10;
            result.attackScale = world.RandInt(1, result.maxAttackScale);
            attackHp *= result.attackScale;

            result.loseHp = attackHp;
            SetHP(data.hp - attackHp);
            //SuperLog.Log("PlayerObject>" + result);

            return result;
        }

        public override AttackerInfo GetAttackInfo()
        {
            AttackerInfo attackInfo = base.GetAttackInfo();

            attackInfo.attack = info.attack;

            return attackInfo;
        }

        public override void OnInitialized()
        {
            base.OnInitialized();
            data = new PlayerData();

            data.hp = info.maxHp;
            data.power = info.maxPower;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
        }

        public override void OnLogicUpdate(Single deltaTime)
        {
            base.OnLogicUpdate(deltaTime);
        }

        public override string ToString() => $"[{ID}]{GetType().Name}<{prefabID}>({playerID})";

        public override string GetMessage()
        {
            string result = base.GetMessage();

            result += "----PlayerObject----\n";

            if (data != null)
            {
                result += "Data\n";
                result += $"  hp:{data.hp}\n";
            }

            if (info != null)
            {
                result += "Info\n";
                result += $"  id:{info.id}\n";
                result += $"  prefabID:{info.prefabID}\n";
                result += $"  config:{info.config}\n";
                result += $"  maxHp:{info.maxHp}\n";
            }

            return result;
        }
    }
}