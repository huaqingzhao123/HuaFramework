/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/20 18:54:34
 */

using FPPhysics.CollisionShapes;
using FPPhysics.CollisionShapes.ConvexShapes;
using FPPhysics.Entities;
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
using FPPhysics.DataStructures;
using System;
using FPPhysics.BroadPhaseEntries;
using UnityEngine;
using System.Diagnostics;

namespace AliveCell
{
    /// <summary>
    /// ShapeDrawer
    /// </summary>
    public class ShapeDrawer
    {
        [Conditional("UNITY_EDITOR")]
        [DebuggerStepThrough]
        public static void DrawSpace(FPPhysics.Space space, DrawUtility drawer)
        {
            if (space == null)
            {
                return;
            }

            foreach (var item in space.Entities)
            {
                DrawEntity(item, drawer);
            }
        }

        [Conditional("UNITY_EDITOR")]
        [DebuggerStepThrough]
        public static void DrawEntity(Entity item, DrawUtility drawer)
        {
            DrawShape(item.CollisionInformation.WorldTransform.Matrix, item.CollisionInformation.Shape, drawer);

            /*
            Vector3 velocity = item.LinearVelocity;
            Vector3 start = item.Position;
            Quaternion rotation = item.Orientation;
            Vector3 end = item.Position + velocity.normalized * 1;
            drawer.DrawArrow(start, end, 0.2f);
            */
        }

        [Conditional("UNITY_EDITOR")]
        [DebuggerStepThrough]
        public static void DrawShape(Matrix4x4 matrix, CollisionShape shape, DrawUtility drawer)
        {
            switch (shape)
            {
                case CompoundShape s:
                    foreach (var child in s.Shapes)
                    {
                        DrawShape(matrix * child.LocalTransform.Matrix, child.Shape, drawer);
                    }
                    break;

                case BoxShape s:
                    drawer.DrawBox(new Vector3(s.Width, s.Length, s.Height), matrix);
                    break;

                case SphereShape s:
                    drawer.DrawSphere(s.Radius, matrix);
                    break;
                case CylinderShape s:
                    drawer.DrawCylinder(s.Height, s.Radius, matrix);
                    break;

                case CapsuleShape s:
                    drawer.DrawCapsule((s.Length + s.Radius * 2), s.Radius, matrix);
                    break;

                case StaticMeshShape s:
                    DrawMesh(s.TriangleMeshData, matrix, drawer);
                    break;

                default:
                    break;
            }
        }

        [Conditional("UNITY_EDITOR")]
        [DebuggerStepThrough]
        public static void DrawMesh(TransformableMeshData triangleMeshData, Matrix4x4 matrix, DrawUtility drawer)
        {
            Vector3[] vt = new Vector3[3];
            UVector3[] uvt = new UVector3[3];
            int length = triangleMeshData.Indices.Length / 3;

            for (int i = 0; i < length; i++)
            {
                triangleMeshData.GetTriangle(i * 3, out vt[0], out vt[1], out vt[2]);
                uvt[0] = vt[0];
                uvt[1] = vt[1];
                uvt[2] = vt[2];

                drawer.DrawPolygon(uvt, matrix);
            }
        }
    }
}