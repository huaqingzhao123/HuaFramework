using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;


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
using ByteAngle = FPPhysics.FPUtility;

namespace AliveCell
{
    public static class InputDataExtensions
    {
        public static void SetKeyCode(this ref InputData data, ActionKeyCode keyCode)
        {
            data.keyCode = keyCode;
        }

        public static void Append(this ref InputData data, InputData other)
        {
            data.keyCode |= other.keyCode;
            data.axisValue = other.axisValue;

            if ((other.keyCode & ActionKeyCode.Axis) == 0/* && (other.keyCode & ActionKeyCode.Blink) ==0*/)
            {//矫正，如果最后没有输入摇杆，则去除已存的摇杆状态，否者axisValue可能被误使用，导致方向错误
                data.keyCode &= ~ActionKeyCode.Axis;
                data.axisValue = byte.MaxValue;
            }
        }
        /// <summary>
        /// 去除单帧的指令
        /// </summary>
        /// <param name="data"></param>
        public static void RemoveOnceKeyCode(this ref InputData data)
        {
            data.keyCode &= ~(ActionKeyCode.Attack | ActionKeyCode.Dash /*| ActionKeyCode.Jump*/ );
        }

        public static void SetAxisFromDir(this ref InputData data, Vector2 dir)
        {
            Single angle = dir == Vector2.zero ? 0 : Vector2.SignedAngle(dir, Vector2.up);
            data.axisValue = ByteAngle.AngleToByte(angle);
        }
        public static ActionKeyCode GetKeyCode(this in InputData data)
        {
            return data.keyCode;
        }

        public static Single GetAngle(this in InputData data)
        {
            return ByteAngle.ByteToAngle(data.axisValue);
        }

        public static Vector3 GetDir(this in InputData data)
        {
            Single angle = data.GetAngle();
            return (Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward).normalized;
        }

        public static Quaternion GetRotation(this in InputData data)
        {
            Single angle = data.GetAngle();
            //if(GameGlobal.current_run_mode == GameGlobal.GAME_RUN_MODE._2D)
            //{
            //    if (angle >= 0 && angle < 180)
            //    {
            //        angle = 90;
            //    }
            //    else
            //    {
            //        angle = 270;
            //    }
            //}
            return Quaternion.AngleAxis(angle, Vector3.up);
        }
        public static Vector3 GetScaleX(this in InputData data)
        {
            Single angle = data.GetAngle();
            Single x = 1;
            if (!(angle >= 0 && angle < 180))
            {
                return Vector3.xMirror;
            }

            //GameDebug.LogError("angle = "+angle);
            return Vector3.one;
        }
        public static Single GetScaleForX(this in InputData data)
        {
            Single angle = data.GetAngle();
            Single x = 1;
            //if(GameGlobal.current_run_mode == GameGlobal.GAME_RUN_MODE._3D)
            //{
            //    return x;
            //}
            if (!(angle >= 0 && angle < 180))
            {
                return -1;
            }
            return x;
        }
    }
}