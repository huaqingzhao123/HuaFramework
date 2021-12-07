using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace HuaFramework.Unity
{
    public static class TransformSetPosQuick
    {
#if UNITY_EDITOR
        [MenuItem("HuaFramework/Tools/设置TransformPos")]
#endif
        private static void SetTransformPos()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetTransformPosX(5);
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(cube.transform);
            capsule.transform.SetTransformPosXZ(3,5,false);
        }

        /// <summary>
        /// 设置transform的x坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="isLocal">是否是设置本地坐标</param>
        public static void SetTransformPosX(this Transform transform, float x, bool isLocal = true)
        {
            Vector3 pos;
            pos = isLocal ? transform.localPosition : transform.position;
            pos.x = x;
            SetTransformPosXYZ(transform, pos, isLocal);
        }
        /// <summary>
        /// 设置transform的y坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="isLocal">是否是设置本地坐标</param>
        public static void SetTransformPosY(this Transform transform, float y, bool isLocal = true)
        {
            Vector3 pos;
            pos = isLocal ? transform.localPosition : transform.position;
            pos.y = y;
            SetTransformPosXYZ(transform, pos, isLocal);


        }
        /// <summary>
        /// 设置transform的z坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="isLocal">是否是设置本地坐标</param>
        public static void SetTransformPosZ(this Transform transform, float z, bool isLocal = true)
        {
            Vector3 pos;
            pos = isLocal ? transform.localPosition : transform.position;
            pos.z = z;
            SetTransformPosXYZ(transform, pos, isLocal);

        }
        public static void SetTransformPosXY(this Transform transform, float x, float y, bool isLocal = true)
        {
            Vector3 pos;
            pos = isLocal ? transform.localPosition : transform.position;
            pos.x = x;
            pos.y = y;
            SetTransformPosXYZ(transform, pos, isLocal);
        }
        public static void SetTransformPosXZ(this Transform transform, float x, float z, bool isLocal = true)
        {
            Vector3 pos;
            pos = isLocal ? transform.localPosition : transform.position;
            pos.x = x;
            pos.z = z;
            SetTransformPosXYZ(transform, pos, isLocal);
        }
        public static void SetTransformPosYZ(this Transform transform, float y, float z, bool isLocal = true)
        {
            Vector3 pos;
            pos = isLocal ? transform.localPosition : transform.position;
            pos.y = y;
            pos.z = z;
            SetTransformPosXYZ(transform, pos, isLocal);
        }
        private static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        private static void SetTransformPosXYZ(Transform transform, Vector3 pos, bool isLocal)
        {
            if (isLocal)
                transform.localPosition = pos;
            else
                transform.position = pos;
        }
    }

}
