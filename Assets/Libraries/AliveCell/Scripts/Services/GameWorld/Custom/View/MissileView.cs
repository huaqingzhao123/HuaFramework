/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/21 11:07:38
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// MissileView
    /// </summary>
    public class MissileView : AssetView
    {
        public Single softRotationSpeed = 7f;

        protected Quaternion softRotationValue;

        private MissileObject obj = null;

        public override void UpdateView(float deltaTime)
        {
            base.UpdateView(deltaTime);
            UpdatePosition(deltaTime);
            UpdateRotation(deltaTime);
        }

        private Vector3 currentVelocity = Vector3.zero;

        private void UpdatePosition(Single deltaTime)
        {
            transform.position = Vector3.SmoothDamp(transform.position, obj.position, ref currentVelocity, 0.033f);
        }

        private void UpdateRotation(Single deltaTime)
        {
            transform.rotation = obj.rotation;

            softRotationValue = Quaternion.Lerp(softRotationValue, obj.rotation, deltaTime * softRotationSpeed * obj.softRotateScale);
            transform.rotation = softRotationValue;
        }

        public override void OnViewBind()
        {
            base.OnViewBind();
            obj = GetObj<MissileObject>();

            transform.position = obj.position;
            transform.rotation = obj.rotation;
            softRotationValue = obj.rotation;

            currentVelocity = Vector3.zero;
        }

        public override void OnViewUnbind()
        {
            base.OnViewUnbind();
            obj = null;
        }
    }
}