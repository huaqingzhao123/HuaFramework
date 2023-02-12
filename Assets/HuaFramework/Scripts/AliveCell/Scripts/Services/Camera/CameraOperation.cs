/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/25 10:53:46
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    public abstract class ICameraOperation
    {
        public CameraService target { get; private set; }

        public bool isActive { get; private set; }

        public void SetActive(bool isActive)
        {
            if (this.isActive == isActive)
            {
                return;
            }

            this.isActive = isActive;
            if (isActive)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnEnable()
        {
        }

        public ICameraOperation(CameraService target)
        {
            this.target = target;
        }

        public abstract void Update(float deltaTime);
    }
}