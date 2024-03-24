/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/18 10:48:12
 */

using AliveCell.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using XMLib;
using XMLib.Extensions;
using XMLib.FSM;

namespace AliveCell
{
    public partial class GameWorld
    {
        public partial class Setting
        {
            [SerializeField]
            private UCameraSystem.Setting _camera = null;

            public UCameraSystem.Setting camera => _camera;
        }
    }

    /// <summary>
    /// UCameraSystem
    /// </summary>
    public class UCameraSystem : ISystem, ILateUpdate, ICreate, IDestroy
    {
        [Serializable]
        public class Setting
        {
            public Single followSpeed = 16f;
            public Single viewSpeed = 16f;
            public Single yAngleSpeed = 16f;
            public Single moveHoldTime = 3f;
            public Single combatHoldTime = 3f;
            public Single autoRotateSpeed = 4f;
            public Single autoRotateSpeedWithMove = 20f;
            public Single autoRotateWaitTime = 2f;
        }

        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        public TObject followObj { get; private set; }
        public Vector3 followPos { get; private set; }

        public TObject surveyObj { get; private set; }
        public Vector3 surveyPos { get; private set; }

        public Setting setting => GlobalSetting.gameWorld.camera;

        public readonly CameraService camera;

        public UCameraSystem(GameWorld world, CameraService camera)
        {
            this.camera = camera;
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UCamera");
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnCreate()
        {
            followPos = camera.follow.followPos;

            camera.fsm.AddState(new CameraStates.GW.Idle(this));
            camera.fsm.AddState(new CameraStates.GW.Move(this));
            camera.fsm.AddState(new CameraStates.GW.Combat(this));
            camera.fsm.AddState(new CameraStates.GW.Complete(this));
            camera.fsm.AddState(new CameraStates.GW.Replay(this));

            App.On<InjuredResult, InjuredInfo>(EventTypes.Game_Injured, OnGameInjured).SetFilter((t) =>
            {
                int id = ((InjuredResult)t[0]).id;
                return world.ucamera.CheckFollow(id) && world.uplayer.IsPlayer(id);
            });
        }

        private void OnGameInjured(InjuredResult result, InjuredInfo info)
        {
            //camera.effect.GetEffect<CameraEffects.InjuredEffect>().Play();
        }

        public void OnDestroy()
        {
            App.Off(this);
            camera.fsm.RemoveStateAll<CameraStates.GW.IGWCameraState>();
        }

        public void OnLateUpdate(Single deltaTime)
        {
            UpdateFollowObj();
        }

        public void SetFollow(TObject obj)
        {
            followObj = obj;

            UpdateFollowObj();

            App.Trigger(EventTypes.Game_FollowTargetChanged, obj);
        }

        public bool CheckFollow(int id)
        {
            return followObj != null && followObj.ID == id;
        }

        private void UpdateFollowObj()
        {
            surveyObj = null;
            surveyPos = Vector3.zero;

            if (followObj == null)
            {
                return;
            }

            IAssetView view = world.uview.GetView(followObj.ID);
            followPos = view != null ? view.transform.position : followObj.position;//优先取表现的坐标

            if (followObj is ActionMachineObject amObj
                && amObj.datas.TryGetValue<int>(DataTag.AimObjID, out int targetId)
                && UObjectSystem.noneID != targetId)
            {//有目标
                IAssetView surveyView = world.uview.GetView(targetId);

                surveyObj = world.uobj.Get<TObject>(targetId);
                surveyPos = surveyView != null ? surveyView.transform.position : surveyObj.position;//优先取表现的坐标
            }
        }

        public void Shake(CameraShakeInfo shakeInfo)
        {
            camera.Shake(shakeInfo);
        }

        public void Shake(string shakeName)
        {
            camera.Shake(shakeName);
        }

        //==========================================================
    }
}