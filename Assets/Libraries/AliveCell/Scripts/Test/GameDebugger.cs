/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/7 16:52:35
 */

using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using XMLib;
using XMLib.AM;
using XMLib.AM.Ranges;

using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;

namespace AliveCell
{
    /// <summary>
    /// GameDebugger
    /// </summary>
    public class GameDebugger : MonoBehaviour
    {
        public bool showRaycastBody;
        public bool showBody;
        public bool showAttack;

        private static GameDebugger _inst;

        protected void Awake()
        {
            if (_inst != null)
            {
                SuperLog.LogWarning("GameDebugger 已存在");
                Destroy(this);
                return;
            }

            _inst = this;
            DontDestroyOnLoad(gameObject);
        }

#if UNITY_EDITOR

        protected void OnValidate()
        {
        }

        protected void OnDrawGizmos()
        {
            if (App.isInited && App.game != null)
            {
                foreach (var item in App.game.uobj.Foreach<ActionMachineObject>())
                {
                    Draw(item);
                }

                DrawUPhysic(App.game.uphysic);
            }
        }

        private void DrawUPhysic(UPhysicSystem uphysic)
        {
            ShapeDrawer.DrawSpace(uphysic.space, DrawUtility.G);

            foreach (var item in uphysic.staticMeshs)
            {
                ShapeDrawer.DrawShape(Matrix4x4.identity, item.Shape, DrawUtility.G);
            }
        }

        private void Draw(ActionMachineObject obj)
        {
            IActionMachine machine = obj?.machine;
            if (obj == null || machine == null)
            {
                return;
            }

            FrameConfig fConfig = machine.GetStateFrame();
            if (fConfig == null)
            {
                return;
            }

            if (showAttack)
            {
                DrawRanges(machine.GetAttackRanges(), obj.localToWorldMatrix, Color.red);
            }

            if (showBody)
            {
                DrawRanges(machine.GetBodyRanges(), obj.localToWorldMatrix, Color.green);
            }
        }

        private void DrawRanges(List<RangeConfig> ranges, Matrix4x4 matrix, Color color)
        {
            if(ranges == null || ranges.Count == 0)
            {
                return;
            }

            DrawUtility.G.PushColor(color);
            //matrix 因物理引擎得放后面乘

            foreach (var range in ranges)
            {
                switch (range.value)
                {
                    case BoxItem v:
                        DrawUtility.G.DrawBox(v.size, Matrix4x4.TRS((Vector3)v.offset, Quaternion.identity, Vector3.one) * matrix);
                        break;

                    case SphereItem v:
                        DrawUtility.G.DrawSphere(v.radius, Matrix4x4.TRS((Vector3)v.offset, Quaternion.identity, Vector3.one) * matrix);
                        break;
                }
            }

            DrawUtility.G.PopColor();
        }

#endif
    }
}