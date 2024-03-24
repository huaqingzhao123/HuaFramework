/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/25 15:44:15
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.AM;
using XMLib.AM.Ranges;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;
using FPPhysics.CollisionShapes.ConvexShapes;
using Space = FPPhysics.Space;
using FPPhysics.Entities.Prefabs;

namespace AliveCell
{
    /// <summary>
    /// SAttackConfig
    /// </summary>
    [ActionConfig(typeof(Attack))]
    [Serializable]
    public class AttackConfig : HoldFrames
    {
        public AttackMode attackMode;
        public Vector3 attackDir;
        public int waitFrameCnt;
        public bool eyeToEye;
        public int impactFrameCnt;

        [AttackTypes]
        [SerializeReference]
        public List<object> extras;

        [ConditionTypes]
        [SerializeReference]
        public List<Conditions.IItem> checker;

        #region optimize

        private Dictionary<Type, object> extrasCache;

        public T GetExtra<T>() where T : class
        {
            if (extrasCache == null || extrasCache.Count != extras.Count)
            {//未初始化或数量发生变化时，重新设置
                extrasCache = new Dictionary<Type, object>();
                foreach (var item in extras)
                {
                    extrasCache.Add(item.GetType(), item);
                }
            }

            T result = extrasCache.TryGetValue(typeof(T), out object obj) ? (T)obj : null;

            return result;
        }

        #endregion optimize
    }

    /// <summary>
    /// SAttack
    /// </summary>
    public class Attack : IActionHandler
    {
        UPhysicSystem uphysic => App.game.uphysic;

        public class Data
        {
            public HashSet<UObject> attackObjs = new HashSet<UObject>();
            public int attackFrameCnt = 0;
        }

        public void Enter(ActionNode node)
        {
            //SAttackConfig config = (SAttackConfig)node.config;
            IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)(IActionController)node.actionMachine.controller;

            node.data = new Data();
        }

        public void Exit(ActionNode node)
        {
            //SAttackConfig config = (SAttackConfig)node.config;
            IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)(IActionController)node.actionMachine.controller;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            AttackConfig config = (AttackConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            Data data = (Data)node.data;

            if (!Condition.Checker(config.checker, node))
            {//检查
                return;
            }

            int attackCnt = 0;

            InjuredInfo injuredInfo = new InjuredInfo()
            {
                info = controller.GetAttackInfo(),
                config = config,
            };

            List<RangeConfig> ranges = machine.GetAttackRanges();
            if (ranges != null)
            {
                foreach (var range in ranges)
                {
                    AttackRange(range);
                }
            }

            data.attackFrameCnt++;

            Attacks.AudioConfig audios = config.GetExtra<Attacks.AudioConfig>();
            if (audios != null)
            {
                int audioId = attackCnt > 0 ? audios.hitted : audios.normal;
                if (audioId > 0)
                {
                    UAudioEvent evt = controller.AppendEvent<UAudioEvent>();
                    evt.audioId = audioId;
                    evt.position = controller.position;
                }
            }

            //更新当前帧攻击数
            int existAttackCnt = controller.datas.GetValue(DataTag.AttackCountInFrame, 0);
            controller.datas[DataTag.AttackCountInFrame] = existAttackCnt + attackCnt;

            if (controller.world.uplayer.IsSelfPlayer(controller.ID))
            {//仅自己生效
                Attacks.ShakeConfig shakes = config.GetExtra<Attacks.ShakeConfig>();
                if (shakes != null)
                {
                    if (!string.IsNullOrEmpty(shakes.normal) && attackCnt == 0)
                    {
                        UCameraEvent evt = controller.AppendEvent<UCameraEvent>();
                        evt.shakeName = shakes.normal;
                    }
                    else if (!string.IsNullOrEmpty(shakes.hitted) && attackCnt > 0)
                    {
                        UCameraEvent evt = controller.AppendEvent<UCameraEvent>();
                        evt.shakeName = shakes.hitted;
                    }
                }
            }

            return;

            void Attack(ActionMachineObject obj)
            {
                if (data.attackObjs.Contains(obj) || obj.isDead)
                {//已经攻击过了或死了
                    return;
                }
                data.attackObjs.Add(obj);
                attackCnt++;

                UAttackEvent evt = controller.AppendEvent<UAttackEvent>();
                evt.injuredInfo = injuredInfo;
                evt.injuredId = obj.ID;
            }

            void AttackRange(RangeConfig range)
            {
                switch (range.value)
                {
                    case BoxItem v:
                        CheckAttackBox((Vector3)v.size, (Vector3)v.offset);
                        break;

                    case SphereItem v:
                        CheckAttackSphere((Single)v.radius, (Vector3)v.offset);
                        break;
                }
            }

            void CheckAttackBox(Vector3 size, Vector3 offset)
            {
                Matrix4x4 matrix = controller.localToWorldMatrix;
                offset = matrix.MultiplyPoint(offset);

                BoxShape shape = new BoxShape(size.x, size.z, size.y);
                RigidTransform rigid = new RigidTransform(offset, controller.rotation);
                Vector3 sweep = new Vector3(0, Fix64.C0p001, 0);

                //DrawUtility.D.PushAndSetDuration(0.5f);
                //DrawUtility.D.PushAndSetColor(Color.red);
                //ShapeDrawer.DrawShape(rigid.Matrix, shape, DrawUtility.D);
                //DrawUtility.D.PopColor();
                //DrawUtility.D.PopDuration();

                List<RayCastResult> results = ListPool<RayCastResult>.Pop();
                if (uphysic.ConvexCast(shape, ref rigid, ref sweep, results, controller.attackMask, uphysic.bodyGroup))
                {
                    ProcessCollider(results);
                }
                //SuperLog.Log($"CheckAttackBox : {results.Count}");
                ListPool<RayCastResult>.Push(results);
            }

            void CheckAttackSphere(Single radius, Vector3 offset)
            {
                Matrix4x4 matrix = controller.localToWorldMatrix;
                offset = matrix.MultiplyPoint(offset);

                SphereShape shape = new SphereShape(radius);
                RigidTransform rigid = new RigidTransform(offset, controller.rotation);
                Vector3 sweep = Vector3.zero;

                //DrawUtility.D.PushAndSetDuration(0.5f);
                //DrawUtility.D.PushAndSetColor(Color.red);
                //ShapeDrawer.DrawShape(rigid.Matrix, shape, DrawUtility.D);
                //DrawUtility.D.PopColor();
                //DrawUtility.D.PopDuration();

                List<RayCastResult> results = ListPool<RayCastResult>.Pop();
                if (uphysic.ConvexCast(shape, ref rigid, ref sweep, results, controller.attackMask, uphysic.bodyGroup))
                {
                    ProcessCollider(results);
                }
                //SuperLog.Log($"CheckAttackBox : {results.Count}");
                ListPool<RayCastResult>.Push(results);
            }

            void ProcessCollider(List<RayCastResult> results)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    RayCastResult result = results[i];
                    RigidBodyObject target = result.HitObject.Tag as RigidBodyObject;
                    if (target != null)
                    {
                        ActionMachineObject obj = target as ActionMachineObject;
                        if (obj != null && obj.ID != controller.ID && obj.groupID != controller.groupID)
                        {
                            Attack(obj);
                        }
                    }
                }
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }

    namespace Attacks
    {
        [Serializable]
        public class AudioConfig
        {
            public int normal;
            public int hitted;
            public int block;
        }

        [Serializable]
        public class EffectConfig
        {
            public int hitted;
            public Single hittedLifeTime;
            public BindPointType hitBindPoint;
        }

        [Serializable]
        public class ShakeConfig
        {
            public string normal;
            public string hitted;
        }

        [Serializable]
        public class VelocityConfig
        {
            public bool clearVelocity;
            public bool transportVelocity;
            public Single appendForwardVelocity;
            public Single appendUpVelocity;
        }

        [Serializable]
        public class LevitateConfig
        {
            public Single upHeight;
        }

        [Serializable]
        public class AddHpPowerConfig
        {
            public int hp;
            public int power;
        }
    }
}