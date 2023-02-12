/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/25 17:03:48
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using XMLib;
using XMLib.Extensions;

namespace AliveCell.CameraStates
{
    namespace GW
    {
        public abstract class IGWCameraState : ICameraFSMState
        {
            protected UCameraSystem ucamera;
            protected UCameraSystem.Setting setting => ucamera.setting;

            protected SuperLogHandler LogHandler = null;

            public IGWCameraState(UCameraSystem ucamera)
            {
                LogHandler = ucamera.LogHandler.CreateSub($"Cam|FSM-{GetType().Name}");
                this.ucamera = ucamera;
            }
        }

        public class Idle : IGWCameraState
        {
            public Idle(UCameraSystem world) : base(world)
            {
            }

            private Single viewProgress = 0f;
            private ViewData step01;
            private ViewData step02;

            public override void Enter(CameraService target)
            {
                step01 = target.setting.GetFollow("Idle").view;
                step02 = target.setting.GetFollow("Idle2").view;
                target.follow.lookAtView = ViewData.Lerp(step01, step02, viewProgress);

                target.follow.followSpeed.SetValue(0, setting.followSpeed, 2f);
                target.follow.viewSpeed.SetValue(0, setting.viewSpeed, 2f);
                target.follow.yAngleSpeed.SetValue(0, setting.yAngleSpeed, 2f);
            }

            public override void Exit(CameraService target)
            {
            }

            public override void Update(CameraService target, Single deltaTime)
            {
                ActionMachineObject amObj = ucamera.followObj as ActionMachineObject;
                if (amObj == null)
                {
                    return;
                }
                Vector2 lookDelta = App.input.Look;
                Vector2 move = App.input.Move;
                Single yAngleDelta = lookDelta.x * 1f;
                Single progressDelta = -lookDelta.y;

                bool isMove = (move.x.NotEqualToZero() || move.y.NotEqualToZero());
                //bool isMove = (amObj.velocity.x.NotEqualToZero() && amObj.velocity.z.NotEqualToZero());
                bool isCombat = amObj.datas.GetValue(DataTag.AttackCountInFrame, 0) > 0;
                bool isInputYAngle = yAngleDelta != 0f;

                if (isCombat)
                {
                    target.fsm.ChangeState<Combat>();
                }
                else if (isMove)
                {
                    target.fsm.ChangeState<Move>();
                }

                viewProgress = Mathf.Clamp01(viewProgress + progressDelta * deltaTime);

                target.follow.lookAtYAngle = (target.follow.lookAtYAngle + yAngleDelta) % 360f;
                target.follow.lookAtView = ViewData.Lerp(step01, step02, viewProgress);
                target.follow.followPos = ucamera.followPos;
            }
        }

        public class Move : IGWCameraState
        {
            public Move(UCameraSystem world) : base(world)
            {
            }

            private Single stateTimer = 0f;
            private Single autoRotateWaitTimer = 0f;

            public override void Enter(CameraService target)
            {
                stateTimer = 0f;
                autoRotateWaitTimer = 0f;
                target.follow.lookAtView = target.setting.GetFollow("Move").view;
                target.follow.followSpeed.SetValue(0, setting.followSpeed, 2f);
                target.follow.viewSpeed.SetValue(0, setting.viewSpeed, 2f);
                target.follow.yAngleSpeed.SetValue(0, setting.yAngleSpeed, 2f);
            }

            public override void Exit(CameraService target)
            {
            }

            public override void Update(CameraService target, Single deltaTime)
            {
                ActionMachineObject amObj = ucamera.followObj as ActionMachineObject;
                if (amObj == null)
                {
                    return;
                }

                stateTimer += deltaTime;
                autoRotateWaitTimer += deltaTime;

                Vector2 lookDelta = App.input.Look;
                Vector2 move = App.input.Move;
                Single yAngleDelta = lookDelta.x * 1f;

                bool isMove = (amObj.velocity.x.NotEqualToZero() && amObj.velocity.z.NotEqualToZero());
                bool isCombat = amObj.datas.GetValue(DataTag.AttackCountInFrame, 0) > 0;
                bool isInputYAngle = yAngleDelta != 0f;
                bool canAutoRotation = false;

                if (isCombat)
                {
                    target.fsm.ChangeState<Combat>();
                }
                else if (isMove)
                {
                    stateTimer = 0f;
                }
                else if (stateTimer >= setting.moveHoldTime)
                {
                    target.fsm.ChangeState<Idle>();
                }

                if (isInputYAngle)
                {
                    autoRotateWaitTimer = 0;
                }
                else if (autoRotateWaitTimer > ucamera.setting.autoRotateWaitTime)
                {
                    canAutoRotation = true;
                }

                bool isUpdated = false;
                if (canAutoRotation)
                {
                    /*//头昏 =。=
                    Vector3 newLookDir = amObj.rotation * Vector3.forward;
                    Single newYAngle = MathUtility.AngleYFromDir(newLookDir);
                    target.follow.lookAtYAngle = Mathf.LerpAngle(target.follow.lookAtYAngle, newYAngle, deltaTime * (isMove ? ucamera.setting.autoRotateSpeedWithMove : ucamera.setting.autoRotateSpeed));
                    isUpdated = true;
                    */
                }

                if (!isUpdated)
                {
                    target.follow.lookAtYAngle = (target.follow.lookAtYAngle + yAngleDelta) % 360f;
                }
                target.follow.followPos = ucamera.followPos;
            }
        }

        public class Combat : IGWCameraState
        {
            public Combat(UCameraSystem world) : base(world)
            {
            }

            private Single autoRotateWaitTimer = 0f;
            private Single attackTimer = 0f;

            public override void Enter(CameraService target)
            {
                autoRotateWaitTimer = 0f;
                attackTimer = 0f;

                target.follow.lookAtView = target.setting.GetFollow("Combat").view;
                target.follow.followSpeed.SetValue(0, setting.followSpeed, 2f);
                target.follow.viewSpeed.SetValue(0, setting.viewSpeed, 2f);
                target.follow.yAngleSpeed.SetValue(0, setting.yAngleSpeed, 2f);
            }

            public override void Exit(CameraService target)
            {
            }

            public override void Update(CameraService target, Single deltaTime)
            {
                ActionMachineObject amObj = ucamera.followObj as ActionMachineObject;
                if (amObj == null)
                {
                    return;
                }

                autoRotateWaitTimer += deltaTime;

                Vector2 lookDelta = App.input.Look;
                Vector2 move = App.input.Move;
                Single yAngleDelta = lookDelta.x * 1f;

                //bool isMove = (amObj.velocity.x.NotEqualToZero() && amObj.velocity.z.NotEqualToZero());
                bool isMove = (move.x.NotEqualToZero() || move.y.NotEqualToZero());
                bool isCombat = amObj.CheckCombatMode();
                bool isAttacked = amObj.datas.GetValue(DataTag.AttackCountInFrame, 0) > 0;
                bool isInputYAngle = !Mathf.Approximately(yAngleDelta, 0.0f);
                bool hasSurveyObj = ucamera.surveyObj != null;
                bool canAutoRotation = false;

                if (isAttacked)
                {
                    attackTimer = 0;
                }

                if (!isCombat)
                {
                    target.fsm.ChangeState(isMove ? typeof(Move) : typeof(Idle));
                }

                if (isInputYAngle)
                {
                    autoRotateWaitTimer = 0;
                }
                else if (autoRotateWaitTimer > ucamera.setting.autoRotateWaitTime)
                {
                    canAutoRotation = true;
                }

                bool isUpdated = false;
                if (canAutoRotation && hasSurveyObj)
                {
                    attackTimer += deltaTime;
                    if (attackTimer < ucamera.setting.combatHoldTime)
                    {
                        Vector3 lookDir = ByteAngle.AngleYToDir(target.follow.lookAtYAngle);
                        Vector3 lookDir2 = ucamera.surveyPos - ucamera.followPos;
                        Single angleOffset = Vector3.SignedAngle(lookDir2, lookDir, Vector3.up);//angleOffset  在左边则小于0，否则大于0
                        if (Mathf.Abs(angleOffset) > 90f)
                        {
                            Vector3 newLookDir = Vector3.Cross(lookDir2, angleOffset < 0 ? Vector3.up : Vector3.down).normalized;

#if UNITY_EDITOR
                            Vector3 centerPos = (ucamera.followPos + ucamera.surveyPos) * 0.5f;
                            DrawUtility.D.DrawLine(centerPos, centerPos + newLookDir * 3f, Color.white);
#endif

                            Single newYAngle = ByteAngle.AngleYFromDir(newLookDir);
                            Single deltaScale = 1 - attackTimer / ucamera.setting.combatHoldTime;
                            target.follow.lookAtYAngle = Mathf.LerpAngle(target.follow.lookAtYAngle, newYAngle, deltaScale * deltaTime * (isMove ? ucamera.setting.autoRotateSpeedWithMove : ucamera.setting.autoRotateSpeed));
                            isUpdated = true;
                        }
                    }
                    /*// 头昏 =。=
                    else
                    {
                        Vector3 newLookDir = amObj.rotation * Vector3.forward;
                        Single newYAngle = ByteAngle.AngleYFromDir(newLookDir);
                        target.follow.lookAtYAngle = Mathf.LerpAngle(target.follow.lookAtYAngle, newYAngle, deltaTime * (isMove ? ucamera.setting.autoRotateSpeedWithMove : ucamera.setting.autoRotateSpeed));
                        isUpdated = true;
                    }
                    */
                }

                if (!isUpdated)
                {
                    target.follow.lookAtYAngle = (target.follow.lookAtYAngle + yAngleDelta) % 360f;
                }

#if UNITY_EDITOR
                Vector3 centerPos2 = (ucamera.followPos + ucamera.surveyPos) * 0.5f;
                DrawUtility.D.DrawLine(centerPos2, centerPos2 + ByteAngle.AngleYToDir(target.follow.lookAtYAngle) * 3f, Color.yellow);
#endif

                target.follow.followPos = ucamera.followPos;
            }
        }

        public class Complete : IGWCameraState
        {
            public Complete(UCameraSystem world) : base(world)
            {
            }

            public override void Enter(CameraService target)
            {
                target.follow.lookAtView = target.setting.GetFollow("Complete").view;
                target.follow.followSpeed.SetValue(0, setting.followSpeed, 2f);
                target.follow.viewSpeed.SetValue(0, setting.viewSpeed, 2f);
                target.follow.yAngleSpeed.SetValue(0, setting.yAngleSpeed, 2f);
            }

            public override void Exit(CameraService target)
            {
            }

            public override void Update(CameraService target, Single deltaTime)
            {
                target.follow.lookAtYAngle = (target.follow.lookAtYAngle + deltaTime * 5f) % 360f;
                target.follow.followPos = ucamera.followPos;
            }
        }

        public class Replay : IGWCameraState
        {
            public Replay(UCameraSystem world) : base(world)
            {
            }

            private Single autoRotateWaitTimer = 0f;
            private Single viewProgress = 0f;
            private ViewData step01;
            private ViewData step02;

            public override void Enter(CameraService target)
            {
                autoRotateWaitTimer = 0f;
                viewProgress = 1f;

                step01 = target.setting.GetFollow("Idle").view;
                step02 = target.setting.GetFollow("Combat").view;

                target.follow.lookAtView = ViewData.Lerp(step01, step02, viewProgress);
                target.follow.followSpeed.SetValue(0, setting.followSpeed, 2f);
                target.follow.viewSpeed.SetValue(0, setting.viewSpeed, 2f);
                target.follow.yAngleSpeed.SetValue(0, setting.yAngleSpeed, 2f);
            }

            public override void Exit(CameraService target)
            {
            }

            public override void Update(CameraService target, Single deltaTime)
            {
                ActionMachineObject amObj = ucamera.followObj as ActionMachineObject;
                if (amObj == null)
                {
                    return;
                }

                autoRotateWaitTimer += deltaTime;

#if UNITY_STANDALONE || UNITY_EDITOR
                bool mouseLeftPressed = Mouse.current?.leftButton.isPressed ?? true;
                Vector2 lookDelta = mouseLeftPressed ? App.input.Look : Vector2.zero;
#else
                Vector2 lookDelta =App.input.Look;
#endif

                Vector2 move = App.input.Move;
                Single yAngleDelta = lookDelta.x * 1f;
                Single progressDelta = -lookDelta.y;

                //bool isMove = (amObj.velocity.x.NotEqualToZero() && amObj.velocity.z.NotEqualToZero());
                bool isMove = (move.x.NotEqualToZero() || move.y.NotEqualToZero());
                bool isCombat = amObj.CheckCombatMode();//amObj.datas.GetValue(DataTag.AttackCountInFrame, 0) > 0;
                bool isInputYAngle = !Mathf.Approximately(yAngleDelta, 0.0f);
                bool hasSurveyObj = ucamera.surveyObj != null;
                bool canAutoRotation = false;

                if (isInputYAngle)
                {
                    autoRotateWaitTimer = 0;
                }
                else if (autoRotateWaitTimer > ucamera.setting.autoRotateWaitTime)
                {
                    canAutoRotation = true;
                }

                bool isUpdated = false;
                if (canAutoRotation && hasSurveyObj)
                {
                    Vector3 lookDir = ByteAngle.AngleYToDir(target.follow.lookAtYAngle);
                    Vector3 lookDir2 = ucamera.surveyPos - ucamera.followPos;
                    Single angleOffset = Vector3.SignedAngle(lookDir2, lookDir, Vector3.up);//angleOffset  在左边则小于0，否则大于0
                    if (Mathf.Abs(angleOffset) > 90f)
                    {
                        Vector3 newLookDir = Vector3.Cross(lookDir2, angleOffset < 0 ? Vector3.up : Vector3.down).normalized;

                        Single newYAngle = ByteAngle.AngleYFromDir(newLookDir);
                        target.follow.lookAtYAngle = Mathf.LerpAngle(target.follow.lookAtYAngle, newYAngle, deltaTime * (isMove ? ucamera.setting.autoRotateSpeedWithMove : ucamera.setting.autoRotateSpeed));
                        isUpdated = true;

#if UNITY_EDITOR
                        Vector3 centerPos = (ucamera.followPos + ucamera.surveyPos) * 0.5f;
                        DrawUtility.D.DrawLine(centerPos, centerPos + newLookDir * 3f, Color.white);
#endif
                    }
                }

                if (!isUpdated)
                {
                    target.follow.lookAtYAngle = (target.follow.lookAtYAngle + yAngleDelta) % 360f;
                }
                viewProgress = Mathf.Clamp01(viewProgress + progressDelta * deltaTime);
                target.follow.lookAtView = ViewData.Lerp(step01, step02, viewProgress);
                target.follow.followPos = ucamera.followPos;

#if UNITY_EDITOR
                Vector3 centerPos2 = (ucamera.followPos + ucamera.surveyPos) * 0.5f;
                DrawUtility.D.DrawLine(centerPos2, centerPos2 + ByteAngle.AngleYToDir(target.follow.lookAtYAngle) * 3f, Color.yellow);
#endif
            }
        }
    }
}