/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/28 12:09:52
 */

using System;
using System.Collections.Generic;
using System.Text;
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
    public class EnemyData
    {
        public Dictionary<int, Single> pid2dist = new Dictionary<int, Single>();
        public int hp;
        public int nearPlayerId;
    }

    /// <summary>
    /// EnemyObject
    /// </summary>
    public class EnemyObject : ActionMachineObject
    {
        public EnemyInfo info { get; set; }
        public EnemyData data { get; protected set; }

        public override int maxHp => info.maxHp;
        public override int maxPower => 0;

        public override int hp => data.hp;

        public override int power => 0;

        public override void SetHP(int hp)
        {
            int oldHp = data.hp;
            data.hp = Mathf.Clamp(hp, 0, info.maxHp);
            TriggerPropertyChanged(PropertyType.Hp, oldHp, data.hp);
        }

        public override void SetPower(int power)
        {
            throw new RuntimeException("未实现该操作");
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
            //SuperLog.Log("EnemyObject>" + result);

            return result;
        }

        public override AttackerInfo GetAttackInfo()
        {
            AttackerInfo attackInfo = base.GetAttackInfo();

            attackInfo.attack = info.attack;

            return attackInfo;
        }

        public override void OnReset()
        {
            base.OnReset();

            info = null;
            data = null;
        }

        public override void OnInitialized()
        {
            base.OnInitialized();
            data = new EnemyData();
            data.hp = info.maxHp;
        }

        public override string ToString() => $"[{ID}]{GetType().Name}<{prefabID}>";

        public override string GetMessage()
        {
            string result = base.GetMessage();

            result += "----EnemyObject----\n";

            if (data != null)
            {
                result += "Data\n";
                result += $"  hp:{data.hp}\n";
                result += $"  Player distance:\n";
                foreach (var item in data.pid2dist)
                {
                    result += $"    [{item.Key}]{item.Value}\n";
                }
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