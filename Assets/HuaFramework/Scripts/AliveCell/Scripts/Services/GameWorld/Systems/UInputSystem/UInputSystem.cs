/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/28 13:39:33
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using XMLib.AM;
using XMLib;
using System;
using XMLib.Extensions;
using UnityEngine.EventSystems;
using AliveCell.Commons;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;


namespace AliveCell
{
    /// <summary>
    /// UInputSystem
    /// </summary>
    public class UInputSystem : ISystem, ICreate, IDestroy, ILogicUpdate
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;
        public readonly CameraService camera;
        public readonly InputService input;

        private Dictionary<int, InputData> id2Input = new Dictionary<int, InputData>();
        private Dictionary<int, InputData> lastId2Input = new Dictionary<int, InputData>();

        private Dictionary<int, InputData> playerid2InputCache = new Dictionary<int, InputData>();

        public bool inputUsed { get; protected set; } = false;
        public Single lookAtYAngle => (Fix64)camera.follow.lookAtYAngleFixed;

        public UInputSystem(GameWorld world, CameraService camera, InputService input)
        {
            this.world = world;
            this.camera = camera;
            this.input = input;
            this.LogHandler = world.LogHandler.CreateSub("UInput");
        }

        public IReadOnlyDictionary<int, InputData> GetPlayerInput()
        {
            playerid2InputCache.Clear();
            foreach (var uid in world.uplayer.uids)
            {
                if (!id2Input.TryGetValue(uid, out InputData data))
                {
                    data = InputData.none;
                }

                playerid2InputCache.Add(uid, data);
            }

            return playerid2InputCache;
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnCreate()
        {
            //gameInput.Enable();
        }

        public void OnDestroy()
        {
            //gameInput.Disable();
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            LogicUpdateInputData();
        }

        public bool MouseNotClickUI()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (Mouse.current.leftButton.wasPressedThisFrame && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }
#endif
            return true;
        }

        public InputData GetSelfInputData()
        {
            InputData result = InputData.none;
            if (!world.isRunning)
            {
                return result;
            }

            Vector2 move = (Vector2)input.Move;
            move = Quaternion.AngleAxis(lookAtYAngle, Vector3.back) * move;

            ActionKeyCode keyCode = ActionKeyCode.None;

            if (move.x.NotEqualToZero())
            {
                keyCode |= move.x < 0 ? ActionKeyCode.Left : ActionKeyCode.Right;
            }

            if (move.y.NotEqualToZero())
            {
                keyCode |= move.y > 0 ? ActionKeyCode.Up : ActionKeyCode.Down;
            }

            if (input.Moving)
            {
                keyCode |= ActionKeyCode.Axis;
            }

            if (input.AttackTriggered && MouseNotClickUI())
            {
                keyCode |= ActionKeyCode.Attack;
            }

            if (input.SkillTriggered)
            {
                keyCode |= ActionKeyCode.Skill;
            }

            if (input.DashTriggered)
            {
                keyCode |= ActionKeyCode.Dash;
            }

            result.SetKeyCode(keyCode);
            result.SetAxisFromDir(move);

            return result;
        }

        private void LogicUpdateInputData()
        {
            if (inputUsed)
            {//输入数据没有更新
                Dictionary<int, InputData> tmp = lastId2Input;
                lastId2Input = id2Input;
                id2Input = tmp;
                id2Input.Clear();

                //使用上一帧数据，并清除单帧的输入
                foreach (var item in lastId2Input)
                {
                    InputData data = item.Value;
                    data.RemoveOnceKeyCode();
                    id2Input[item.Key] = data;
                }
            }
            inputUsed = true;
        }

        private void CheckInputUsed()
        {
            if (inputUsed)
            {//输入数据已经使用，则清空
                inputUsed = false;

                Dictionary<int, InputData> tmp = lastId2Input;
                lastId2Input = id2Input;
                id2Input = tmp;
                id2Input.Clear();
            }
        }

        public void UpdateNetInput(Dictionary<string, InputData> players)
        {//需要同步逻辑帧，不要放在渲染帧中使用
            CheckInputUsed();
            foreach (var player in players)
            {
                AddInput(world.uplayer.Pid2Uid(player.Key), player.Value);
            }
        }

        public void UpdateLocalInput()
        {
            CheckInputUsed();

            InputData data = GetSelfInputData();
            AddInput(world.uplayer.selfUid, data);
        }

        //public void UpdateReplayInput(RecordPlayer recordPlayer, Dictionary<int, int> mapId)
        //{//需要同步逻辑帧，不要放在渲染帧中使用
        //    CheckInputUsed();

        //    if (!recordPlayer.HasFrame(world.frameIndex))
        //    {
        //        return;
        //    }

        //    Flat.FrameData? frame = recordPlayer.GetFrame(world.frameIndex);

        //    for (int i = 0; i < frame.Value.InputsLength; i++)
        //    {
        //        Flat.InputData? input = frame.Value.Inputs(i);
        //        AddInput(mapId[input.Value.Id], input.Value.To());
        //    }
        //}

        public void AddKey(int id, ActionKeyCode code)
        {
            if (world.gameState != GameState.Normal)
            {//非游戏中，不许输入
                return;
            }

            if (id2Input.TryGetValue(id, out InputData data))
            {
                data.keyCode |= code;
                id2Input[id] = data;
            }
            else
            {
                id2Input.Add(id, new InputData()
                {
                    keyCode = code,
                    axisValue = 0
                });
            }
        }

        public void SetAxis(int id, bool axisState, byte axisValue)
        {
            if (world.gameState != GameState.Normal)
            {//非游戏中，不许输入
                return;
            }

            if (id2Input.TryGetValue(id, out InputData data))
            {
                data.keyCode = axisState ? (data.keyCode | ActionKeyCode.Axis) : (data.keyCode & ~ActionKeyCode.Axis);
                data.axisValue = axisState ? axisValue : byte.MaxValue;
                id2Input[id] = data;
            }
            else
            {
                id2Input.Add(id, axisState ? new InputData()
                {
                    keyCode = ActionKeyCode.Axis,
                    axisValue = axisValue
                } : InputData.none);
            }
        }

        public void AddInput(int id, InputData inputData)
        {
            if (world.gameState != GameState.Normal)
            {//非游戏中，不许输入
                return;
            }

            if (id2Input.TryGetValue(id, out InputData data))
            {
                data.Append(inputData);
                id2Input[id] = data;
            }
            else
            {
                id2Input.Add(id, inputData);
            }
        }

        public void SetAxisFromDir(int id, bool axisState, Vector3 dir)
        {
            byte axisValue = ByteAngle.ByteAngleYFromDir(dir);
            SetAxis(id, axisState, axisValue);
        }

        public InputData GetRawInput(int id)
        {
            return id2Input.TryGetValue(id, out InputData data) ? data : InputData.none;
        }

        public ActionKeyCode GetAllKey(int id)
        {
            if (!id2Input.TryGetValue(id, out InputData data))
            {
                return ActionKeyCode.None;
            }

            return data.keyCode;
        }

        public byte GetAxis(int id)
        {
            if (!id2Input.TryGetValue(id, out InputData data))
            {
                return 0;
            }

            return data.axisValue;
        }

        public bool HasKey(int id, ActionKeyCode keyCode, bool fullMatch = false)
        {
            if (!id2Input.TryGetValue(id, out InputData data))
            {
                return false;
            }

            if (fullMatch)
            {
                return (data.keyCode & keyCode) == keyCode;
            }
            else
            {
                return (data.keyCode & keyCode) != 0;
            }
        }

        //public void OnSystemLogicLateUpdate(Single deltaTime)
        //{
        //    //清理输入，当渲染帧数低于逻辑帧时，直接清理会出现输入断节，即没有输入的情况
        //    //ClearKey();//不要在这里执行
        //}
    }
}