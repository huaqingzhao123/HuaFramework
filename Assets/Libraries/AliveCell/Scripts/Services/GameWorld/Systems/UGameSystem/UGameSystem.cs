/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/25 0:31:57
 */

using AliveCell.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using XMLib;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// UGameSystem
    /// </summary>
    public class UGameSystem : ISystem, IUpdate, ICreate, IDestroy, ILogicUpdate, ILateUpdate, ISyncLogicUpdate
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        protected readonly List<ICreate> onCreate = new List<ICreate>();
        protected readonly List<IDestroy> onDestroy = new List<IDestroy>();
        protected readonly List<IUpdate> onUpdate = new List<IUpdate>();
        protected readonly List<ILateUpdate> onLateUpdate = new List<ILateUpdate>();
        protected readonly List<ILogicUpdate> onLogicUpdate = new List<ILogicUpdate>();
        protected readonly List<ISyncLogicUpdate> onSyncLogicUpdate = new List<ISyncLogicUpdate>();

        public UGameSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UGame");
        }

        private void AddAllComponent()
        {
            AttackTargetFlagDisplay atf = App.CreateGO(10000003).GetComponent<AttackTargetFlagDisplay>();
            AimRingDisplay ar = App.CreateGO(10000004).GetComponent<AimRingDisplay>();
            AddComponent(atf);
            AddComponent(ar);
        }

        protected void AddComponent<T>(T component) where T : IUGameComponent
        {
            if (component is ICreate onCmpCreate) { onCreate.Add(onCmpCreate); }
            if (component is IDestroy onCmpDestroy) { onDestroy.Add(onCmpDestroy); }
            if (component is IUpdate onCmpUpdate) { onUpdate.Add(onCmpUpdate); }
            if (component is ILateUpdate onCmpLateUpdate) { onLateUpdate.Add(onCmpLateUpdate); }
            if (component is ILogicUpdate onCmpLogicUpdate) { onLogicUpdate.Add(onCmpLogicUpdate); }
            if (component is ISyncLogicUpdate onCmpSyncLogicUpdate) { onSyncLogicUpdate.Add(onCmpSyncLogicUpdate); }
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnCreate()
        {
            world.input.SetMask(world.gameMode != GameMode.Replay ? InputMask.Game_Normal : InputMask.Game_Replay);
            App.On(EventTypes.Game_Complete, OnGameComplete);
            App.On(EventTypes.UI_ResumeGame, OnUIResumeGame);

            AddAllComponent();
            foreach (var cmp in onCreate)
            {
                cmp.OnCreate();
            }
        }

        private void OnUIResumeGame()
        {
            world.input.SetMask(world.gameMode != GameMode.Replay ? InputMask.Game_Normal : InputMask.Game_Replay);
        }

        private void OnGameComplete()
        {
            world.input.SetMask(InputMask.Cursor);
        }

        public void OnDestroy()
        {
            App.Off(this);

            foreach (var cmp in onDestroy)
            {
                cmp.OnDestroy();
            }

            world.input.SetMask(InputMask.All);
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            foreach (var cmp in onLogicUpdate)
            {
                cmp.OnLogicUpdate(deltaTime);
            }
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var cmp in onUpdate)
            {
                cmp.OnUpdate(deltaTime);
            }

#if UNITY_STANDALONE || UNITY_EDITOR
            if (world.input.CursorActiveTriggered)
            {
                world.input.ReverseMask(InputMask.Game_Normal_R);
            }
#endif
        }

        public void OnLateUpdate(float deltaTime)
        {
            foreach (var cmp in onLateUpdate)
            {
                cmp.OnLateUpdate(deltaTime);
            }
        }

        public void OnSyncLogicUpdate(Single deltaTime)
        {
            foreach (var cmp in onSyncLogicUpdate)
            {
                cmp.OnSyncLogicUpdate(deltaTime);
            }
        }
    }
}