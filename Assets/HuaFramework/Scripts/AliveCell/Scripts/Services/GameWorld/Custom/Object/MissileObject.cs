/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/21 11:07:58
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.Extensions;

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
    /// MissileObject
    /// </summary>
    public class MissileObject : TObject, IUObjectLogicUpdate
    {
        public MissileBuilderConfig config;
        public AttackerInfo info;

        protected Single timer;
        protected Vector3 lastPosition;
        protected ActionMachineObject ownerObj;

        public override void OnInitialized()
        {
            base.OnInitialized();

            ownerObj = world.uobj.Get<ActionMachineObject>(info.id);
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            lastPosition = position;

            Vector3 dir = rotation * Vector3.forward;
            Single distance = config.moveSpeed * deltaTime;

            bool hitted = false;

            if (Physics.SphereCast(position, config.radius, dir, out RaycastHit hitInfo, distance, ownerObj.attackMask | config.obstacleMask))
            {
                Collider collider = hitInfo.collider;
                if (ownerObj != null)
                {
                    ColliderController target = collider.GetComponentInParent<ColliderController>();
                    if (target != null)
                    {
                        //TODO 等待修改为定点
                        ActionMachineObject obj = null;//target.target as ActionMachineObject;
                        if (obj != null && !obj.isDead && obj.ID != ownerObj.ID && obj.groupID != ownerObj.groupID)
                        {
                            UAttackEvent evt = ownerObj.AppendEvent<UAttackEvent>();
                            evt.injuredId = obj.ID;
                            evt.injuredInfo = new InjuredInfo()
                            {
                                info = info,
                                config = config
                            };
                        }
                    }
                }

                distance = (Single)hitInfo.distance;
                hitted = true;
                throw new Exception("未修改成定点数");
            }

            position += dir * distance;
            rotation = Quaternion.LookRotation(position - lastPosition);

            timer += deltaTime;
            if (timer > config.lifeTime || hitted)
            {
                Destory();
            }

            DrawUtility.D.DrawCross(lastPosition, config.radius, Color.red);
            DrawUtility.D.DrawLine(lastPosition, position, Color.red);
            DrawUtility.D.DrawSphere(position, config.radius, Color.red);
        }

        public override void OnReset()
        {
            base.OnReset();

            config = null;
            info = null;
            timer = 0;
            lastPosition = Vector3.zero;
            ownerObj = null;
        }
    }
}