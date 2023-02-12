/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/21 0:00:44
 */

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// DeadReckoningTest
    /// </summary>
    public class DeadReckoningTest : MonoBehaviour
    {
        public Transform startPoint;
        public Transform endPoint;
        public float startSpeed = 10;
        public float endSpeed = 10;
        public float deltaTime = 0;

        [Range(0f, 1f)]
        public float step = 1f;

        public int stepCnt = 10;
        public float3x3 oldInfo;
        public float3x3 newInfo;
        public Vector3[] points;

        protected void OnDrawGizmos()
        {
            if (startPoint == null || endPoint == null)
            {
                return;
            }

            oldInfo.c0 = startPoint.position;
            oldInfo.c1 = startPoint.forward * startSpeed;

            newInfo.c0 = endPoint.position;
            newInfo.c1 = endPoint.forward * endSpeed;

            DrawUtility.G.DrawSphere(oldInfo.c0, 0.5f, Color.red);
            DrawUtility.G.DrawLine(oldInfo.c0, oldInfo.c0 + oldInfo.c1 * 5f, Color.red);

            DrawUtility.G.DrawSphere(newInfo.c0, 0.5f, Color.blue);
            DrawUtility.G.DrawLine(newInfo.c0, newInfo.c0 + newInfo.c1 * 5f, Color.blue);

            float3x4 d = DeadReckoning.Init(oldInfo, newInfo, deltaTime);
            points = new Vector3[stepCnt];

            for (int i = 0; i < stepCnt; i++)
            {
                points[i] = DeadReckoning.Smooth(d, i / (float)(stepCnt - 1));
            }

            Vector3 stepPoint = DeadReckoning.Smooth(d, step);

            DrawUtility.G.DrawSphere(stepPoint, 0.5f, Color.grey);

            DrawUtility.G.DrawLines(points, Color.green);
        }
    }
}