using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HuaFramework.Utility
{
    /// <summary>
    /// 框架数学工具类
    /// </summary>
    public partial class MathfUtil
    {
        /// <summary>
        /// 判断百分之多少的概率是否满足
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static bool JudgePercent(int percent)
        {
            return Random.Range(0, 100) < percent;
        }
        /// <summary>
        /// 返回给定数组中的一个随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static T GetRandomElement<T>(params T[] objects)
        {
            return objects[Random.Range(0, objects.Length)];
        }
        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="fNum">浮点数</param>
        /// <returns></returns>
        public static float GetFloorFloat(float fNum)
        {
            return Mathf.Floor(fNum);
        }

        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="fNum">浮点数</param>
        /// <returns></returns>
        public static int GetFloorToInt(float fNum)
        {
            return Mathf.FloorToInt(fNum);
        }

        /// <summary>
        /// 向上取整
        /// </summary>
        /// <param name="fNum">浮点数</param>
        /// <returns></returns>
        public static int GetCeilToInt(float fNum)
        {
            return Mathf.CeilToInt(fNum);
        }
        /// <summary>
        /// 两位小数
        /// </summary>
        /// <param name="fNum">浮点数</param>
        /// <returns></returns>
        public static float GetFloatTwo(float fNum)
        {
            return Mathf.Floor(fNum * 100) / 100.0f;
        }

        /// <summary>
        /// 比较两个浮点数是否接近相等
        /// </summary>
        /// <param name="f1">浮点数</param>
        /// <param name="f2">浮点数</param>
        /// <returns></returns>
        public static bool CompareFloat(float f1, float f2, bool log = false)
        {
            int a1 = GetFloorToInt(f1 * 100000);
            int a2 = GetFloorToInt(f2 * 100000);
            if (log) Debug.Log(a1 + " " + a2 + " " + Mathf.Abs(a1 - a2));
            if (a1 >= a2 || Mathf.Abs(a1 - a2) <= 10) return true;
            return false;
        }

        /// <summary>
        /// 比较两个浮点数是否接近相等 - 精度6位小数
        /// </summary>
        /// <param name="f1">浮点数</param>
        /// <param name="f2">浮点数</param>
        /// <returns></returns>
        public static bool EqualFloat(float f1, float f2, bool log = false)
        {
            int a1 = GetFloorToInt(f1 * 100000);
            int a2 = GetFloorToInt(f2 * 100000);
            if (log) Debug.Log(a1 + " " + a2 + " " + Mathf.Abs(a1 - a2));
            if (a1 == a2 || Mathf.Abs(a1 - a2) <= 10) return true;
            return false;
        }

        /// <summary>
        /// 向量旋转后的向量
        /// </summary>
        /// <param name="pos">向量</param>
        /// <param name="angle">旋转角度</param>
        /// <returns></returns>
        public static Vector2 RotVector2(Vector2 pos, float angle)
        {
            return RotVector2(pos, Vector2.zero, angle);
        }

        /// <summary>
        /// 向量旋转后的向量
        /// </summary>
        /// <param name="pos">点</param>
        /// <param name="center">点</param>
        /// <param name="angle">旋转角度</param>
        /// <returns></returns>
        public static Vector2 RotVector2(Vector2 pos, Vector2 center, float angle)
        {
            var rotVec = Vector2.zero;
            angle *= Mathf.Deg2Rad;
            rotVec.x = (pos.x - center.x) * Mathf.Cos(angle) - (pos.y - center.y) * Mathf.Sin(angle) + center.x;
            rotVec.y = (pos.x - center.x) * Mathf.Sin(angle) + (pos.y - center.y) * Mathf.Cos(angle) + center.y;

            return rotVec;
        }
        /// <summary>
        /// 叉乘(p1p2*p1p)  aXb=x1y2-x2y1
        /// </summary>
        public static float GetVector2Cross(Vector2 middleP, Vector2 leftP, Vector2 rigthP)
        {
            //  aXb=x1y2-x2y1
            return (leftP.x - middleP.x) * (rigthP.y - middleP.y) - (rigthP.x - middleP.x) * (leftP.y - middleP.y);
        }
        /// <summary>
        /// 判断点是否在矩形内
        /// </summary>
        /// <returns></returns>
        public static bool IsVector2PointInMatrix(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p)
        {
            return GetVector2Cross(p1, p2, p) * GetVector2Cross(p3, p4, p) >= 0 && GetVector2Cross(p2, p3, p) * GetVector2Cross(p4, p1, p) >= 0;
        }

        /// <summary>
        /// 判断点是否在三角形内
        /// </summary>
        /// <returns></returns>
        public static bool Vector2IsInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 judgeP)
        {
            var sum = CalculateTriangle(p1, p2, p3);
            var s1 = CalculateTriangle(judgeP, p2, p3);
            var s2 = CalculateTriangle(judgeP, p1, p3);
            var s3 = CalculateTriangle(judgeP, p1, p2);
            var res = s1 + s2 + s3;
            //误差0.5
            var inArea = res >= sum - 0.5f && res <= sum + 0.5f;
            //var inArea = (s1 + s2 + s3)==sum ;
            return inArea;
        }

        /// <summary>
        /// 计算三角形面积，传入三边长度,海伦计算法
        /// </summary>
        /// <returns></returns>
        public static float CalculateTriangle(float a, float b, float c)
        {
            //S= Mathf.Sqrt(p(p−a)(p−b)(p−c));
            var average = (a + b + c) / 2;
            return Mathf.Sqrt(average * (average - a) * (average - b) * (average - c));
        }
        /// <summary>
        /// 计算三角形面积，叉乘计算,传入三点
        /// </summary>
        /// <returns></returns>
        public static float CalculateTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            //叉乘计算的是平行四边形面积所以除以2
            return Mathf.Abs(GetVector2Cross(p1, p2, p3) / 2);
        }

        /// <summary>
        /// 两线段是否相交
        /// </summary>
        /// <param name="p1">点</param>
        /// <param name="p2">点</param>
        /// <param name="t1">点</param>
        /// <param name="t2">点</param>
        /// <returns></returns>
        public static bool IsPointInBothSide(Vector2 p1, Vector2 p2, Vector2 t1, Vector2 t2)
        {
            return GetVector2Cross(p1, p2, t1) * GetVector2Cross(p1, p2, t2) < 0 && GetVector2Cross(t1, t2, p1) * GetVector2Cross(t1, t2, p2) < 0;
        }

        /// <summary>
        /// 碰撞检测，两个矩形相交检测
        /// </summary>
        /// <param name="target1">目标1</param>
        /// <param name="target2">目标2</param>
        /// <returns>是否相交</returns>
        public static bool CheckCollider(Transform target1, Transform target2, Vector2 target1ColliderArea, Vector2 target2ColliderArea)
        {
            var targetPos2 = target2.position;
            //矩形的四个顶点位置
            var p1 = Vector2.zero;
            p1.x = targetPos2.x - target2ColliderArea.x;
            p1.y = targetPos2.y - target2ColliderArea.y;
            var p2 = Vector2.zero;
            p2.x = targetPos2.x + target2ColliderArea.x;
            p2.y = targetPos2.y - target2ColliderArea.y;
            var p3 = Vector2.zero;
            p3.x = targetPos2.x + target2ColliderArea.x;
            p3.y = targetPos2.y + target2ColliderArea.y;
            var p4 = Vector2.zero;
            p4.x = targetPos2.x - target2ColliderArea.x;
            p4.y = targetPos2.y + target2ColliderArea.y;

            Vector3 rot = target2.rotation.eulerAngles;
            //旋转后的顶点
            p1 = RotVector2(p1, targetPos2, rot.z);
            p2 = RotVector2(p2, targetPos2, rot.z);
            p3 = RotVector2(p3, targetPos2, rot.z);
            p4 = RotVector2(p4, targetPos2, rot.z);

            //目标对象的五个点是否在矩形内
            var targetPos1 = target1.position;
            //左下角点
            var t1 = Vector2.zero;
            t1.x = targetPos1.x - target1ColliderArea.x;
            t1.y = targetPos1.y - target1ColliderArea.y;
            //右下角点
            var t2 = Vector2.zero;
            t2.x = targetPos1.x + target1ColliderArea.x;
            t2.y = targetPos1.y - target1ColliderArea.y;
            //右上角点
            var t3 = Vector2.zero;
            t3.x = targetPos1.x + target1ColliderArea.x;
            t3.y = targetPos1.y + target1ColliderArea.y;
            //左上角点
            var t4 = Vector2.zero;
            t4.x = targetPos1.x - target1ColliderArea.x;
            t4.y = targetPos1.y + target1ColliderArea.y;
            rot = target1.rotation.eulerAngles;
            //旋转后的顶点
            t1 = RotVector2(t1, targetPos1, rot.z);
            t2 = RotVector2(t2, targetPos1, rot.z);
            t3 = RotVector2(t3, targetPos1, rot.z);
            t4 = RotVector2(t4, targetPos1, rot.z);
            var b = IsVector2PointInMatrix(t1, t2, t3, t4, p1);
            if (!b) b = IsVector2PointInMatrix(t1, t2, t3, t4, p2);
            if (!b) b = IsVector2PointInMatrix(t1, t2, t3, t4, p3);
            if (!b) b = IsVector2PointInMatrix(t1, t2, t3, t4, p4);
            if (!b) b = IsVector2PointInMatrix(p1, p2, p3, p4, t1);
            if (!b) b = IsVector2PointInMatrix(p1, p2, p3, p4, t2);
            if (!b) b = IsVector2PointInMatrix(p1, p2, p3, p4, t3);
            if (!b) b = IsVector2PointInMatrix(p1, p2, p3, p4, t4);
            //判断边的相交情况
            if (!b) b = IsPointInBothSide(p1, p2, t1, t2);
            if (!b) b = IsPointInBothSide(p2, p3, t1, t2);
            if (!b) b = IsPointInBothSide(p1, p2, t2, t3);
            if (!b) b = IsPointInBothSide(p2, p3, t2, t3);
            if (!b) return false;
            return true;
        }
    }

}

