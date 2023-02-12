/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/20 14:50:59
 */

using System;
using UnityEngine;

using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// RigidBodyView
    /// </summary>
    public class RigidBodyView : AssetView
    {
        [SerializeField]
        private Transform modelRoot = null;

        public float softRotationSpeed = 7f;

        protected Quaternion softRotationValue;

        private RigidBodyObject obj = null;

        private float moveTimer = 0f;
        private Vector3 oldPosition = Vector3.zero;
        private bool isSmoothing = false;
        private float moveTime = 0f;
        private Vector3 lastUpdateVelocity;

        public override void UpdateView(float deltaTime)
        {
            base.UpdateView(deltaTime);
            UpdatePosition(deltaTime);
            UpdateRotation(Time.deltaTime);
        }

        public override void OnPushPool()
        {
            base.OnPushPool();

            moveTimer = 0f;
            oldPosition = Vector3.zero;
            isSmoothing = false;
            moveTime = 0f;
            lastUpdateVelocity = Vector3.zero;
        }

        protected override void SyncLogicUpdate(Single deltaTime)
        {
            base.SyncLogicUpdate(deltaTime);

            //UpdatePosition(deltaTime);
            //UpdateRotation(deltaTime);
        }

        private void UpdatePosition(float deltaTime)
        {
            if (isSmoothing && moveTimer > Single.Epsilon)
            {
                moveTimer -= deltaTime;
                transform.position = Vector3.Lerp(oldPosition, obj.position, Mathf.Clamp01(1.0f - moveTimer / moveTime));
                if (moveTimer <= Single.C1em6)
                {
                    deltaTime = Mathf.Abs(moveTimer);
                    moveTimer = 0f;
                    isSmoothing = false;
                }
            }

            if (!isSmoothing)
            {
                transform.position += (Vector3)lastUpdateVelocity * deltaTime;
            }
        }

        public override void LogicUpdateView(Single deltaTime)
        {
            base.LogicUpdateView(deltaTime);
            moveTimer += deltaTime;
            moveTime = moveTimer;
            oldPosition = transform.position;
            isSmoothing = true;
            lastUpdateVelocity = obj.velocity;
        }

        private void UpdateRotation(float deltaTime)
        {
            transform.rotation = obj.rotation;

            softRotationValue = Quaternion.Lerp(softRotationValue, obj.rotation, deltaTime * softRotationSpeed * obj.softRotateScale);
            modelRoot.rotation = softRotationValue;
        }

        public override void OnViewBind()
        {
            base.OnViewBind();
            obj = GetObj<RigidBodyObject>();

            transform.position = obj.position;
            modelRoot.rotation = obj.rotation;
            softRotationValue = obj.rotation;
        }

        public override void OnViewUnbind()
        {
            base.OnViewUnbind();
            obj = null;
        }
    }
}