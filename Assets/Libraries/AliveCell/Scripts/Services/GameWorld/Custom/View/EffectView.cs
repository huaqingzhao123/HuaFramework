/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/2 13:50:53
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// EffectView
    /// </summary>
    public class EffectView : AssetView
    {
        private EffectObject effect;
        protected ActionMachineObject bindObj;
        protected ActionMachineView bindView;

        public override bool canSyncLogic => effect.CanUpdate();

        public override void UpdateView(float deltaTime)
        {
            base.UpdateView(deltaTime);

            if (effect.updateTransform)
            {
                UpdateTransform(deltaTime);
            }
        }

        private void UpdateTransform(float deltaTime)
        {
            if (bindObj != null)
            {
                Matrix4x4 l2w;
                if (bindView == null)
                {
                    l2w = bindObj.localToWorldMatrix;
                }
                else
                {
                    l2w = bindView.FindBind(effect.bindPointType).localToWorldMatrix;
                }

                transform.position = l2w.MultiplyPoint(effect.localPosition);
                transform.rotation = (effect.useObjRotation ? bindObj.localToWorldMatrix.rotation : l2w.rotation) * effect.localRotation;
            }
        }

        public override void OnViewBind()
        {
            base.OnViewBind();
            effect = GetObj<EffectObject>();
            bindObj = world.uobj.Get<ActionMachineObject>(effect.bindObjId);
            bindView = system.GetView<ActionMachineView>(effect.bindObjId);

            UpdateTransform(0);
        }

        public override void OnViewUnbind()
        {
            base.OnViewUnbind();
            effect = null;
            bindObj = null;
            bindView = null;
        }
    }
}