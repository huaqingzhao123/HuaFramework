/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/24 14:20:33
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
using FPPhysics.CollisionShapes.ConvexShapes;
using Space = FPPhysics.Space;

namespace AliveCell
{
    /// <summary>
    /// FindAttackTargetConfig
    /// </summary>
    [ActionConfig(typeof(FindAttackTarget))]
    [Serializable]
    public class FindAttackTargetConfig
    {
        public Single findRadius = 6;
        public Single lockRadius = 3;
        public Single lockAngle = 90;
        public Single findInterval = 3;
    }

    /// <summary>
    /// FindAttackTarget
    /// </summary>
    public class FindAttackTarget : IActionHandler
    {
        UPhysicSystem uphysic => App.game.uphysic;

        public class Data
        {
            public Single findTimer = 0;
        }

        public void Enter(ActionNode node)
        {
            //FindAttackTargetConfig config = (FindAttackTargetConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            node.data = new Data();
        }

        public void Exit(ActionNode node)
        {
            //FindAttackTargetConfig config = (FindAttackTargetConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            FindAttackTargetConfig config = (FindAttackTargetConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            Data data = (Data)node.data;

            if (!(controller is PlayerObject))
            {
                return;
            }

            int lastAimObjID = controller.datas.GetValue<int>(DataTag.AimObjID, UObjectSystem.noneID);
            bool lastTargetInRadius = false;
            bool lastTargetInAngle = false;

            ActionMachineObject aimObj = App.game.uobj.Get(lastAimObjID) as ActionMachineObject;
            if (aimObj == null || aimObj.isDead)
            {
                lastAimObjID = UObjectSystem.noneID;
            }

#if UNITY_EDITOR
            Vector3 pos = controller.position;//controller.localToWorldMatrix.Translation;
            Matrix4x4.CreateTranslation(ref pos, out Matrix4x4 tranMatrix);
            Matrix4x4 mat = Matrix4x4.CreateFromQuaternion(Quaternion.Euler(90, 0, 90)) * tranMatrix;
            DrawUtility.D.PushColor(Color.gray);
            DrawUtility.D.DrawCircle(config.findRadius, mat);
            DrawUtility.D.DrawCircle(config.lockRadius, mat);
            DrawUtility.D.PushColor(Color.cyan);
            DrawUtility.D.DrawArc(config.findRadius, config.lockAngle, -controller.rotation.eulerAngles.y, mat);
#endif

            //=======================================

            //=======================================
            Vector3 forward = controller.rotation * Vector3.forward;
            Single halfAngle = config.lockAngle / 2;

            Single distance = Single.MaxValue;
            Single angle = Single.MaxValue;
            int targetID = UObjectSystem.noneID;
            ActionMachineObject targetObj = null;

            SphereShape shape = new SphereShape((Single)config.lockRadius);
            RigidTransform rigid = new RigidTransform(controller.position);
            Vector3 sweep = Vector3.zero;
            List<RayCastResult> reuslts = ListPool<RayCastResult>.Pop();

            //=======================================
            //锁定范围内会维持上次查找的目标
            //Collider[] targets = Physics.OverlapSphere(controller.position, config.lockRadius, layerMask);
            if (uphysic.ConvexCast(shape, ref rigid, ref sweep, reuslts, controller.attackMask))
            {
                foreach (var target in reuslts)
                {
                    ActionMachineObject obj = target.HitObject.Tag as ActionMachineObject;
                    if (obj == null || obj.isDead || obj.groupID == controller.groupID)
                    {
                        continue;
                    }

                    DrawUtility.D.DrawLine(controller.position, obj.position, Color.green);

                    Single distance2 = Vector3.Distance(controller.position, obj.position);
                    Single angle2 = Vector3.Angle(forward, obj.position - controller.position);

                    if (obj.ID == lastAimObjID)
                    {
                        lastTargetInRadius = true;
                        lastTargetInAngle = angle2 < halfAngle;
                    }

                    if (targetID == UObjectSystem.noneID ||
                        (angle2 < halfAngle && (distance > distance2 || angle > angle2))
                        )
                    {
                        distance = distance2;
                        angle = angle2;
                        targetID = obj.ID;
                        targetObj = obj;
                        if (targetID == lastAimObjID && angle2 < 90f)
                        {//与最后一次相似，则保持
                            break;
                        }
                    }
                }
            }
            ListPool<RayCastResult>.Push(reuslts);

            //=========================================
            if (targetID == UObjectSystem.noneID)
            {//查找最近的目标
                shape.Radius = (Single)config.findRadius;
                //targets = Physics.OverlapSphere(controller.position, config.findRadius, layerMask);
                if (uphysic.ConvexCast(shape, ref rigid, ref sweep, reuslts, controller.attackMask))
                {
                    foreach (var target in reuslts)
                    {
                        ActionMachineObject obj = target.HitObject.Tag as ActionMachineObject;
                        if (obj == null || obj.isDead || obj.groupID == controller.groupID)
                        {
                            continue;
                        }

                        DrawUtility.D.DrawLine(controller.position, obj.position, Color.green);

                        Single distance2 = Vector3.Distance(controller.position, obj.position);
                        Single angle2 = Vector3.Angle(forward, obj.position - controller.position);

                        if (obj.ID == lastAimObjID)
                        {
                            lastTargetInRadius = true;
                            lastTargetInAngle = angle2 < halfAngle;
                        }

                        if (targetID == UObjectSystem.noneID ||
                            (angle2 < halfAngle && (distance > distance2 || angle > angle2))
                            )
                        {
                            distance = distance2;
                            angle = angle2;
                            targetID = obj.ID;
                            targetObj = obj;
                        }
                    }
                }
            }

            ListPool<RayCastResult>.Push(reuslts);
            //=====================================

            ////作用是减慢攻击对象的变化，防止变化过快
            data.findTimer += deltaTime;
            if (!lastTargetInAngle && lastTargetInRadius
                && lastAimObjID != UObjectSystem.noneID
                && data.findTimer < config.findInterval)
            {
                return;
            }
            else if (lastAimObjID != targetID)
            {
                data.findTimer = 0;//重置计时
            }

            //=====================================

            int nearAttackObjID = targetID;
            if (nearAttackObjID == UObjectSystem.noneID)
            {//没有可攻击的目标时，全地图查找最近的目标
                Single nearDistance = 0;
                foreach (var item in App.game.uobj.Foreach<EnemyObject>())
                {
                    Single dist = Vector3.Distance(controller.position, item.position);
                    if (!item.isDead && item.ID != controller.ID && (nearDistance > dist || nearAttackObjID == UObjectSystem.noneID))
                    {
                        nearAttackObjID = item.ID;
                        nearDistance = dist;
                    }
                }
                foreach (var item in App.game.uobj.Foreach<PlayerObject>())
                {
                    Single dist = Vector3.Distance(controller.position, item.position);
                    if (!item.isDead && item.ID != controller.ID && (nearDistance > dist || nearAttackObjID == UObjectSystem.noneID))
                    {
                        nearAttackObjID = item.ID;
                        nearDistance = dist;
                    }
                }
            }
            controller.datas[DataTag.NearAimObjID] = nearAttackObjID;

            //========================================

            controller.datas[DataTag.AimObjID] = targetID;
            if (targetObj != null)
            {
                List<int> aimSelfObjIDs = targetObj.datas.GetOrCreateValue<List<int>>(DataTag.AimSelfObjIDsInFrame);
                aimSelfObjIDs.Add(controller.ID);
            }

            //=====================================

            if (lastAimObjID != targetID)
            {
                App.Trigger(EventTypes.Game_AttackTargetChanged, controller.ID, lastAimObjID, targetID);
            }

#if UNITY_EDITOR
            //
            if (targetObj != null)
            {
                DrawUtility.D.DrawLine(controller.position, targetObj.position, Color.red);
            }
            //

            DrawUtility.D.ClearColor();
#endif
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}