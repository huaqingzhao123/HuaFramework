/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/11/19 14:53:24
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using XMLib;

namespace AliveCell
{
    [Flags]
    public enum InputMask
    {
        //

        None = 0,
        All = ~0,

        //

        Look = 0x0000_0001,
        Move = 0x0000_0002,
        Moving = 0x0000_0004,
        AttackTriggered = 0x0000_0008,
        SkillTriggered = 0x0000_0010,
        DashTriggered = 0x0000_0020,
        CursorLockTriggered = 0x0000_0040,

        //标记
        Cursor = 0x1000_0000,//是否使用鼠标

        //

        Game_Normal = Look | Move | Moving | AttackTriggered | SkillTriggered | DashTriggered | CursorLockTriggered,
        Game_Normal_R = (Game_Normal | Cursor) & ~CursorLockTriggered,
        Game_Replay = Cursor | Look,
    }

    /// <summary>
    /// InputService
    /// </summary>
    public class InputService : IServiceInitialize, IDisposable
    {
        [InjectObject] public DeviceService device { get; set; }

        protected GameInput.PlayerActions input => device.gameInput.Player;

        public Vector2 Look => CheckGameInput(input.Look.ReadValue<Vector2>(), InputMask.Look, Vector2.zero);
        public Vector2 Move => CheckGameInput(input.Move.ReadValue<Vector2>(), InputMask.Move, Vector2.zero);
        public bool Moving => CheckGameInput(input.Move.phase == InputActionPhase.Started, InputMask.Moving, false);
        public bool AttackTriggered => CheckGameInput(input.Attack.triggered, InputMask.AttackTriggered, false);
        public bool SkillTriggered => CheckGameInput(input.Skill.triggered, InputMask.SkillTriggered, false);
        public bool DashTriggered => CheckGameInput(input.Dash.triggered, InputMask.DashTriggered, false);
        public bool CursorActiveTriggered => CheckGameInput(input.CursorActive.triggered, InputMask.CursorLockTriggered, false);

        public InputMask inputMask { get; private set; } = InputMask.All;
        public SuperLogHandler LogHandler = SuperLogHandler.Create("Input");

        private T CheckGameInput<T>(T value, InputMask type, T disactiveValue)
        {
            return (inputMask & type) != 0 ? value : disactiveValue;
        }

        public void SetMask(InputMask mask)
        {
            inputMask = mask;

#if UNITY_STANDALONE || UNITY_EDITOR
            device.cursorActive = (inputMask & InputMask.Cursor) != 0;
#endif
            LogHandler.Log($"Set Mask : {mask}");
        }

        public void ReverseMask(InputMask value)
        {
            InputMask result = ((inputMask ^ value) & value) | (inputMask & ~value);
            SetMask(result);
        }

        public IEnumerator OnServiceInitialize()
        {
            SetMask(InputMask.All);
#if UNITY_STANDALONE || UNITY_EDITOR
#endif
            yield break;
        }

        public void Dispose()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
#endif
        }
    }
}