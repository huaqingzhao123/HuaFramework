/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/25 11:01:44
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using DG.Tweening;

namespace AliveCell
{
    public class CameraShake : ICameraOperation
    {
        public CameraShakeInfo info { get; set; } = default;
        public SuperLogHandler LogHandler;

        public CameraShake(CameraService target)
            : base(target)
        {
            LogHandler = target.LogHandler.CreateSub("Shake");
        }

        public void Shake()
        {
            Transform root = target.cameraTransform;
            root.DOKill(true);

            Vector3 strength = info.strength;
            if (info.correctDirection && App.game != null)
            {
                TObject obj = App.game.ucamera.followObj;
                if (obj != null)
                {
                    strength = ((Quaternion)obj.rotation * strength.normalized).normalized * strength.magnitude;
                    strength = root.InverseTransformVector(strength);
                }
            }

            if (info.isPunch)
            {
                root.DOPunchPosition(strength, info.duration, info.vibrato, info.elasticity)
                    .SetEase(info.curve);
            }
            else
            {
                root.DOShakePosition(info.duration, info.strength, info.vibrato)
                    .SetEase(info.curve);
            }
            //LogHandler.Log("Play {0}", info);

            //震动
            if (!string.IsNullOrEmpty(info.hapticName))
            {
                target.device.Haptic(info.hapticName);
            }
        }

        public void Shake(CameraShakeInfo info)
        {
            this.info = info;
            Shake();
        }

        public override void Update(float deltaTime)
        {
        }
    }
}