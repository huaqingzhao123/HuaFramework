using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using Unity.Mathematics;

namespace AliveCell
{
    /// <summary>
    /// DeadReckoning
    /// </summary>
    public class DeadReckoning
    {
        private float3x3 oldInfo;
        private float3x3 newInfo;

        private float3x4 data;
        private float elapseTime;
        private float smoothTimer;
        private bool smoothing;

        public void Initialize(float3 pos)
        {
            oldInfo.c0 = pos;
            oldInfo.c1 = float3.zero;
            oldInfo.c2 = float3.zero;

            smoothing = false;

            elapseTime = 0f;
            smoothTimer = 0f;
        }

        public float3 Update(float deltaTime)
        {
            float3 position = float3.zero;
            if (smoothing)
            {//平滑
                smoothTimer += deltaTime;
                float progress = math.saturate(smoothTimer / elapseTime);
                position = Smooth(data, progress);
                if (progress >= 1)
                {
                    smoothing = false;
                    deltaTime = smoothTimer - elapseTime;//超出的时间
                    oldInfo = newInfo;
                }
            }

            if (!smoothing)
            {//预测
                oldInfo = Predict(oldInfo, deltaTime);
                position = oldInfo.c0;
            }

            return position;
        }

        public void UpdateNewPosition(float3 pos, float3 velocity, float3 acceleratedVelocity, float deltaTime)
        {
            newInfo.c0 = pos;
            newInfo.c1 = velocity;
            newInfo.c2 = acceleratedVelocity;

            this.elapseTime = deltaTime;
            this.smoothTimer = 0f;
            this.smoothing = true;
            this.data = Init(oldInfo, newInfo, deltaTime);
        }

        public static float3x3 Predict(float3x3 oldInfo, float deltaTime)
        {
            oldInfo.c0 = oldInfo.c0 + oldInfo.c1 * deltaTime + 0.5f * oldInfo.c2 * math.pow(deltaTime, 2);
            return oldInfo;
        }

        public static float3x4 Init(float3x3 oldInfo, float3x3 newInfo, float deltaTime)
        {
            float3x4 pos;
            pos.c0 = oldInfo.c0;
            pos.c1 = oldInfo.c0 + oldInfo.c1;
            pos.c2 = newInfo.c0 + newInfo.c1 * deltaTime + 0.5f * newInfo.c2 * math.pow(deltaTime, 2);
            pos.c3 = pos.c2 - (newInfo.c1 + newInfo.c2 * deltaTime);

            //float3x4 pos;
            //pos.c0 = oldInfo.c0;
            //pos.c1 = oldInfo.c0 + oldInfo.c1 * deltaTime;
            //pos.c2 = newInfo.c0 - newInfo.c1 * deltaTime;
            //pos.c3 = newInfo.c0;

            float3x4 d;
            d.c0 = pos.c3 - 3 * pos.c2 + 3 * pos.c1 - pos.c0;
            d.c1 = 3 * pos.c2 - 6 * pos.c1 + 3 * pos.c0;
            d.c2 = 3 * pos.c1 - 3 * pos.c0;
            d.c3 = pos.c0;

            return d;
        }

        public static float3 Smooth(float3x4 d, float t)
        {
            return d.c0 * math.pow(t, 3) + d.c1 * math.pow(t, 2) + d.c2 * t + d.c3;
        }
    }
}