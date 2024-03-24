/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/25 10:50:50
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using XMLib.FSM;

namespace AliveCell
{
    public abstract class ICameraFSMState : IFSMState<CameraService>
    {
        public abstract void Enter(CameraService target);

        public abstract void Exit(CameraService target);

        public abstract void Update(CameraService target, float deltaTime);
    }

    public class CameraFSM : FSM<CameraService>
    {
        public CameraFSM()
        {
            AddState(new CameraStates.None());
            AddState(new CameraStates.LookUpDown());
        }

        public override void RemoveState(Type type)
        {
            base.RemoveState(type);

            if (currentState == null)
            {
                ChangeState<CameraStates.None>();
            }
        }
    }

    namespace CameraStates
    {
        public class None : ICameraFSMState
        {
            public override void Enter(CameraService target)
            {
            }

            public override void Exit(CameraService target)
            {
            }

            public override void Update(CameraService target, float deltaTime)
            {
            }
        }

        [Serializable]
        public class LookUpDownSetting
        {
            public float speed = 1f;
            public Ease ease = Ease.Linear;
            public float nextYAngle = 120f;
            public float lerpSpeed = 6f;
        }

        [Serializable]
        public class LookUpDown : ICameraFSMState
        {
            public LookUpDownSetting setting => GlobalSetting.camera.follow.lookUpDown;

            protected float progress;
            protected bool isUp;
            protected bool doNextStep;

            protected bool isUpComplete = false;
            protected bool isDownComplete = false;

            private ViewTranslateInfo info01;
            private ViewTranslateInfo info02;
            private ViewTranslateInfo info03;

            public Action<bool> onStepComplete;

            public IEnumerator WaitUpComplete()
            {
                bool isComplete = false;
                Action<bool> callback = (t) => isComplete = t ? true : isComplete;
                onStepComplete += callback;
                yield return new WaitUntil(() => isComplete);
                onStepComplete -= callback;
            }

            public IEnumerator WaitDownComplete()
            {
                doNextStep = true;
                bool isComplete = false;
                Action<bool> callback = (t) => isComplete = !t ? true : isComplete;
                onStepComplete += callback;
                yield return new WaitUntil(() => isComplete);
                onStepComplete -= callback;
                //因为CameraFollow有插值，所以完成时，不会立即到达目标设置，没想到好的方法，先搁置
                //yield return new WaitForSeconds(setting.yAngleViewSpeed);
            }

            public void Reset()
            {
                progress = 0f;
                isUpComplete = false;
                isDownComplete = false;
                isUp = true;
                doNextStep = false;
            }

            public override void Enter(CameraService target)
            {
                Reset();

                info01 = ViewTranslateInfo.Create(target.follow.lookAtYAngle, target.follow.lookAtView);
                info02 = ViewTranslateInfo.Create((setting.nextYAngle - target.follow.lookAtYAngle) / 2.0f + target.follow.lookAtYAngle, target.setting.GetFollow("LookUp").view);
                info03 = ViewTranslateInfo.Create(setting.nextYAngle, target.setting.GetFollow("Idle").view);

                target.follow.followSpeed.SetValue(setting.lerpSpeed / 2f, setting.lerpSpeed, 0.5f);
                target.follow.viewSpeed.SetValue(setting.lerpSpeed);
                target.follow.yAngleSpeed.SetValue(setting.lerpSpeed);
            }

            public override void Exit(CameraService target)
            {
            }

            public override void Update(CameraService target, float deltaTime)
            {
                if ((isUpComplete && isDownComplete) || !(isUp || !isUp && doNextStep))
                {
                    return;
                }

                progress += deltaTime * setting.speed;
                float value = Mathf.Clamp01(EaseUtility.Evaluate(setting.ease, Mathf.Clamp01(progress), 1.0f));
                ViewTranslateInfo view = isUp ? ViewTranslateInfo.Lerp(info01, info02, value) : ViewTranslateInfo.Lerp(info03, info02, 1.0f - value);
                target.follow.lookAtView = view.view;
                target.follow.lookAtYAngle = view.yAngle;
                if (value >= 1.0f)
                {
                    if (isUp)
                    {
                        isUp = false;//向上执行完后，等待向下执行
                        progress -= 1.0f;
                        isUpComplete = true;
                        onStepComplete?.Invoke(true);
                    }
                    else
                    {
                        target.fsm.ChangeState<CameraStates.None>();
                        isDownComplete = true;
                        onStepComplete?.Invoke(false);
                    }
                }

                progress = Mathf.Clamp01(progress);
            }
        }
    }
}