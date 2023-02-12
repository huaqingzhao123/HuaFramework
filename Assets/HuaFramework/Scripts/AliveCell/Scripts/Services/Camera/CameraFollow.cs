/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/25 11:01:55
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    public struct ViewTranslateInfo
    {
        public float yAngle;
        public ViewData view;

        public Vector3 forward => ByteAngle.AngleYToDir(yAngle);
        public Vector3 back => ByteAngle.AngleYToDir(yAngle + 180f);
        public Vector3 right => Vector3.Cross(Vector3.up, forward);
        public Vector3 left => Vector3.Cross(forward, Vector3.up);
        public float angle => view.angle + view.cameraAngle;
        public Vector3 offset => Quaternion.AngleAxis(view.angle, right) * back * view.distance + view.height * Vector3.up;
        public Quaternion rotation => Quaternion.LookRotation(Quaternion.AngleAxis(angle, right) * forward);

        public void TweenTo(in ViewTranslateInfo info, float viewDelta, float yAngleDelta)
        {
            this = Lerp(this, info, viewDelta, yAngleDelta);
        }

        public static ViewTranslateInfo Lerp(ViewTranslateInfo fromInfo, in ViewTranslateInfo toInfo, float progress, float? yAngleProgress = null)
        {
            ViewTranslateInfo result;
            result.view = ViewData.Lerp(fromInfo.view, toInfo.view, progress);
            result.yAngle = Mathf.LerpAngle(fromInfo.yAngle, toInfo.yAngle, yAngleProgress ?? progress);
            return result;
        }

        public static ViewTranslateInfo Create(float yAngle, in ViewData view)
        {
            ViewTranslateInfo info;
            info.yAngle = yAngle;
            info.view = view;
            return info;
        }
    }

    public class CameraFollow : ICameraOperation
    {
        public Vector3 followPos { get; set; }
        public float lookAtYAngle { get; set; }
        public ViewData lookAtView { get; set; }

        public TweenValueHelper tweenValueHelper;
        public FloatTween followSpeed { get; set; }
        public FloatTween yAngleSpeed { get; set; }
        public FloatTween viewSpeed { get; set; }

        public float lookAtYAngleFixed => ByteAngle.FixedByteAngle(lookAtYAngle);

        private Vector3 currentPosition;
        private ViewTranslateInfo currentTInfo;

        //private bool useTweenViewSpeed = false;
        //private float tweenViewSpeedTimer;
        //private float tweenViewSpeedTime;
        //private Ease tweenViewEase;

        public CameraFollow(CameraService target)
            : base(target)
        {
            tweenValueHelper = new TweenValueHelper();
            followSpeed = tweenValueHelper.Create<FloatTween>();
            yAngleSpeed = tweenValueHelper.Create<FloatTween>();
            viewSpeed = tweenValueHelper.Create<FloatTween>();
        }

        //public void TweenViewSpeed(float time, Ease ease = Ease.Linear)
        //{
        //    useTweenViewSpeed = time > 0.0f;
        //    tweenViewSpeedTimer = 0f;
        //    tweenViewSpeedTime = time;
        //    tweenViewEase = ease;
        //}

        public void ApplyImmediately()
        {
            currentPosition = this.followPos;
            currentTInfo = ViewTranslateInfo.Create(this.lookAtYAngle, this.lookAtView);
            Apply();
        }

        public void InitFollowPos(Vector3 followPos)
        {
            this.followPos = followPos;
            ApplyFollowPos();
        }

        public void ApplyFollowPos()
        {
            currentPosition = this.followPos;
            Vector3 targetPosition = currentPosition + currentTInfo.offset;
            target.rootTransform.position = targetPosition;
            target.listener.transform.position = currentPosition;
        }

        private void Apply()
        {
            Vector3 targetPosition = currentPosition + currentTInfo.offset;
            target.rootTransform.position = targetPosition;
            target.listener.transform.position = currentPosition;

            Quaternion targetRotation = currentTInfo.rotation;
            target.rootTransform.rotation = targetRotation;

#if UNITY_EDITOR
            DrawUtility.D.DrawCross(targetPosition, 1f, Color.magenta);
            DrawUtility.D.DrawLine(targetPosition, targetPosition + target.rootTransform.forward * 10f, Color.magenta);
#endif
        }

        public override void Update(float deltaTime)
        {
            tweenValueHelper.Update(deltaTime);

            float followDelta = deltaTime * followSpeed;
            float viewDelta = deltaTime * viewSpeed;
            float yAngleDelta = deltaTime * yAngleSpeed;

            //if (useTweenViewSpeed)
            //{
            //    tweenViewSpeedTimer += deltaTime;
            //    float scale = Mathf.Clamp01(EaseUtility.Evaluate(tweenViewEase, tweenViewSpeedTimer, tweenViewSpeedTime));
            //    viewDelta *= scale;
            //    useTweenViewSpeed = !Mathf.Approximately(scale, 1.0f);
            //}

            ViewTranslateInfo info = ViewTranslateInfo.Create(lookAtYAngle, lookAtView);

            currentPosition = Vector3.Lerp(currentPosition, followPos, followDelta);
            currentTInfo.TweenTo(info, viewDelta, yAngleDelta);

            Apply();
        }
    }
}