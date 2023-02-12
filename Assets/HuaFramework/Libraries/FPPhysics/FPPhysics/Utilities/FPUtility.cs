using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPPhysics
{
    public static class FPUtility
    {
        public static Fix64 Deg2Rad => Fix64.Deg2Rad;
        public static Fix64 Rad2Deg => Fix64.Rad2Deg;

        public static Fix64 Clamp01(Fix64 t) => t < Fix64.Zero ? Fix64.Zero : (t > Fix64.One ? Fix64.One : t);

        public static Fix64 Clamp(Fix64 t, Fix64 min, Fix64 max) => t < min ? min : (t > max ? max : t);
        public static int Clamp(int t, int min, int max) => t < min ? min : (t > max ? max : t);
        public static long Clamp(long t, long min, long max) => t < min ? min : (t > max ? max : t);
        public static Fix64 Acos(Fix64 x) => Fix64.Acos(x);

        public static Fix64 Asin(Fix64 x) => Fix64.Asin(x);

        public static Fix64 Atan2(Fix64 y, Fix64 x) => Fix64.Atan2(y, x);

        public static Fix64 Atan(Fix64 z) => Fix64.Atan(z);

        public static Fix64 Tan(Fix64 x) => Fix64.Tan(x);

        public static Fix64 Cos(Fix64 x) => Fix64.Cos(x);

        public static Fix64 Sin(Fix64 x) => Fix64.Sin(x);

        public static Fix64 Sqrt(Fix64 x) => Fix64.Sqrt(x);

        public static Fix64 Pow(Fix64 b, Fix64 exp) => Fix64.Pow(b, exp);

        public static Fix64 Pow2(Fix64 x) => Fix64.Pow2(x);

        public static Fix64 Abs(Fix64 value) => Fix64.Abs(value);

        public static Fix64 Sign(Fix64 v) => (v >= 0) ? Fix64.C1 : -Fix64.C1;

        public static Fix64 Min(Fix64 a, Fix64 b) => a > b ? b : a;

        public static Fix64 Max(Fix64 a, Fix64 b) => a < b ? b : a;

        public static int Max(int a, int b) => a < b ? b : a;

        public static Fix64 Remap(Fix64 value, Fix64 fromMin, Fix64 fromMax, Fix64 toMin, Fix64 toMax)
        {
            Fix64 scale = Clamp01((value - fromMin) / (fromMax - fromMin));
            value = (toMax - toMin) * scale + toMin;
            return value;
        }

        /// <summary>
        /// 跳跃速度
        /// </summary>
        /// <param name="gravity">重力</param>
        /// <param name="height">高度</param>
        /// <returns>速度</returns>
        public static Fix64 JumpSpeed(Fix64 gravity, Fix64 height)
        {
            return Sqrt(2 * Abs(gravity) * height);
        }


        #region  byte to angle


        public const byte ByteAngleScale = 2;

        /// <summary>
        /// byte 映射到角度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Fix64 ByteToAngle(byte value)
        {
            return value * ByteAngleScale;
        }

        public static byte AngleToByte(Fix64 angle)
        {
            angle = angle % 360;
            angle += angle >= 0 ? 0 : 360;
            return (byte)(angle == 0 ? 0 : (angle / ByteAngleScale));
        }

        public static Fix64 FixedByteAngle(Fix64 angle)
        {
            return ByteToAngle(AngleToByte(angle));
        }

        public static Fix64 AngleYFromDir(Vector3 dir)
        {
            dir.y = 0;//在同一平面旋转
            return Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        }

        public static Vector3 AngleYToDir(Fix64 yAngle)
        {
            return (Quaternion.AngleAxis(yAngle, Vector3.up) * Vector3.forward).normalized;
        }

        public static byte ByteAngleYFromDir(Vector3 dir)
        {
            return AngleToByte(AngleYFromDir(dir));
        }

        public static Fix64 FixedByteAngleYFromDir(Vector3 dir)
        {
            return FixedByteAngle(AngleYFromDir(dir));
        }

        public static int FloorToInt(Fix64 v)
        {
            return (int)Fix64.Floor(v);
        }
        
        public static Fix64 Floor(Fix64 v)
        {
            return Fix64.Floor(v);
        }

        #endregion
    }
}