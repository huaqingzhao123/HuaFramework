/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/5/20 20:41:23
 */

using System;
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

namespace AliveCell
{
    /// <summary>
    /// Raycast2DObject
    /// </summary>
    public abstract class RaycastObject : TObject
    {
        /// <summary>
        /// 碰撞体偏移
        /// </summary>
        public Vector3 offset { get => _offset; set { _offset = value; _isDirty = true; } }

        public Single radius { get => _radius; set { _radius = value; _isDirty = true; } }
        public Single height { get => _height; set { _height = value; _isDirty = true; } }

        /// <summary>
        /// 皮肤宽度
        /// </summary>
        public Single skinWidth { get => _skinWidth; set { _skinWidth = value; _isDirty = true; } }

        public override Vector3 position { get => base.position; set { base.position = value; _isDirty = true; } }

        /// <summary>
        /// 障碍物层级
        /// </summary>
        public LayerMask obstacleMask { get; set; }

        /// <summary>
        /// 自身层级
        /// </summary>
        public int layer { get; set; }

        /// <summary>
        /// 碰撞体控制器
        /// </summary>
        /// <value></value>
        public ColliderController collider { get; protected set; }

        #region raw data

        protected Vector3 _offset;

        protected Single _radius;
        protected Single _height;

        protected Single _skinWidth;

        protected bool _isDirty = true;

        #endregion raw data

        /// <summary>
        /// 检测缓冲
        /// </summary>
        protected static RaycastHit[] hitBuffer = new RaycastHit[3];

        #region 计算所得

        /// <summary>
        /// 碰撞体信息
        /// </summary>
        protected Collider3DInfo colliderInfo;

        /// <summary>
        /// 碰撞结果
        /// </summary>
        protected Collision3DResult collisionResult;

        #endregion 计算所得

        /// <summary>
        /// 获取移动后的碰撞数据
        /// <para>仅在Translate后有效</para>
        /// </summary>
        /// <returns>碰撞结果</returns>
        public Collision3DResult GetCollisionResult() => collisionResult;

        /// <summary>
        /// 获取碰撞信息
        /// </summary>
        /// <returns>碰撞体信息</returns>
        public Collider3DInfo GetColliderInfo()
        {
            Rebuild();
            return colliderInfo;
        }

        public override void OnInitialized()
        {
            base.OnInitialized();

            GameObject obj = App.res.CreateGO(10000001);
#if UNITY_EDITOR
            obj.name = $"[{ID}]{GetType().Name}(Collider)";
#endif
            collider = obj.GetComponent<ColliderController>();
            //collider.Initialization(this);
        }

        public override void OnDestroyed()
        {
            if (collider != null)
            {
                App.res.DestroyGO(collider);
                collider = null;
            }

            base.OnDestroyed();
        }

        public override void OnReset()
        {
            base.OnReset();

            _offset = Vector3.up * 1;
            _skinWidth = 0.02m;
            _radius = 0.5m;
            _height = 2.0m;
            _isDirty = true;

            layer = default;
            collider = null;

            obstacleMask = default;
            colliderInfo = default;
            collisionResult = default;
        }

        public override string GetMessage()
        {
            string message = base.GetMessage();

            message += $"----Raycast2DObject----\n" +
                $"{colliderInfo}\n" +
                $"{collisionResult}\n";

            return message;
        }

        public Vector3 Translate(Vector3 velocity)
        {
            Initialize();//1
            HorizontalChecker(ref velocity);//2
            VerticalChecker(ref velocity);//3
            position += velocity;//4

            ExtensionChecker();

            collider.UpdateCollider();

            return velocity;
        }

        private void Rebuild()
        {
            if (!_isDirty)
            {
                return;
            }
            _isDirty = false;

            colliderInfo.Initialize(_offset, position, _skinWidth, _height, _radius);
        }

        private void Initialize()
        {
            collisionResult.Initialize();
        }

        public bool SmallCast(Vector3 dir, Single distance, LayerMask mask, out RaycastHit hit)
        {
            return Cast(Vector3.zero, dir, distance, mask, out hit, true);
        }

        public bool BigCast(Vector3 dir, Single distance, LayerMask mask, out RaycastHit hit)
        {
            return Cast(Vector3.zero, dir, distance, mask, out hit, false);
        }

        public bool SmallCheck(LayerMask mask)
        {
            return Check(Vector3.zero, mask, true);
        }

        public bool BigCheck(LayerMask mask)
        {
            return Check(Vector3.zero, mask, false);
        }

        private bool Cast(Vector3 offset, Vector3 dir, Single distance, LayerMask mask, out RaycastHit hit, bool checkSmall)
        {
            Rebuild();

            Single radius = checkSmall ? colliderInfo.smallRadius : colliderInfo.bigRadius;
            collider.BeginIgnore();
            int hitCnt = Physics.CapsuleCastNonAlloc(
                colliderInfo.highPos + offset,
                colliderInfo.lowPos + offset,
                radius,
                dir,
                hitBuffer,
                distance + radius,//多检测一段距离
                mask);
            collider.EndIgnore();

            hit = hitCnt > 0 ? hitBuffer[0] : default;
            return hitCnt > 0 && hit.distance <= distance;
        }

        private bool Check(Vector3 offset, LayerMask mask, bool checkSmall)
        {
            Rebuild();

            Single radius = checkSmall ? colliderInfo.smallRadius : colliderInfo.bigRadius;
            collider.BeginIgnore();
            bool hit = Physics.CheckCapsule(
                colliderInfo.highPos + offset,
                colliderInfo.lowPos + offset,
                radius,
                mask);
            collider.EndIgnore();

            return hit;
        }

        private void ExtensionChecker()
        {
            collisionResult.groundHitted = collisionResult.dir.y < 0 && collisionResult.verticalHitted;
            if (!collisionResult.groundHitted)
            {
                collisionResult.groundHitted = Cast(Vector3.zero, Vector3.down, colliderInfo.skin, obstacleMask, out RaycastHit hit, true);
            }
        }

        private void HorizontalChecker(ref Vector3 velocity)
        {
            if (velocity.x == 0 && velocity.z == 0)
            {
                return;
            }

            Single distance = new Vector2(velocity.x, velocity.z).magnitude;
            Vector3 dir = new Vector3(velocity.x, 0, velocity.z).normalized;

            if (!Cast(Vector3.zero, dir, distance, obstacleMask, out RaycastHit hit, true))
            {
                return;
            }

            Single normalDirAngle = Vector3.SignedAngle(-dir, (Vector3)hit.normal, Vector3.up);
            Single skin = colliderInfo.skin;
            if (!MathUtility.FEqualZero(normalDirAngle))
            {
                skin /= Mathf.Cos(Mathf.Abs(normalDirAngle) * Mathf.Deg2Rad);
            }
            Single moveDist = (Single)hit.distance - skin;
            Vector3 offset = new Vector3(moveDist * dir.x, 0, moveDist * dir.z);

            //Single x = Mathf.Abs(offset.x);
            //Single z = Mathf.Abs(offset.z);
            //if ((z > x && x < colliderInfo.skin) || (x > z && z < colliderInfo.skin))
            //{
            //    offset = Vector3.zero;
            //}

            //矫正距离
            velocity.x = offset.x;
            velocity.z = offset.z;

            Single remainDist = distance - moveDist;
            if (!MathUtility.FEqualZero(remainDist) && !MathUtility.FEqualZero(normalDirAngle))
            {
                Vector3 dir2 = (Quaternion.AngleAxis(Mathf.Sign(normalDirAngle) * 90, Vector3.up) *(Vector3)hit.normal).normalized;
                Single distance2 = remainDist * Mathf.Clamp01(Mathf.Abs(normalDirAngle) / 90);
                Single moveDist2 = distance2;
                if (Cast(offset, dir2, distance2, obstacleMask, out RaycastHit hit2, true))
                {
                    Single normalDirAngle2 = Vector3.SignedAngle(-dir2, (Vector3)hit2.normal, Vector3.up);
                    Single skin2 = colliderInfo.skin;
                    if (!MathUtility.FEqualZero(normalDirAngle))
                    {
                        skin2 /= Mathf.Cos(Mathf.Abs(normalDirAngle2) * Mathf.Deg2Rad);
                    }
                    moveDist2 = (Single)hit2.distance - skin2;
                }
                velocity.x = velocity.x + moveDist2 * dir2.x;
                velocity.z = velocity.z + moveDist2 * dir2.z;
            }

            collisionResult.dir.x = dir.x;
            collisionResult.dir.z = dir.z;
            collisionResult.horizontalHitted = true;
            collisionResult.horizontalHittedLayer = hit.transform.gameObject.layer;
        }

        private void VerticalChecker(ref Vector3 velocity)
        {
            if (velocity.y == 0)
            {
                return;
            }

            Vector3 offset = new Vector3(velocity.x, 0, velocity.z);//需要偏移已经移动的水平距离
            Single distance = Mathf.Abs(velocity.y);
            Vector3 dir = Vector3.up * (velocity.y > 0 ? 1 : -1);

            if (!Cast(offset, dir, distance, obstacleMask, out RaycastHit hit, true))
            {
                return;
            }

            //矫正距离
            velocity.y = ((Single)hit.distance - skinWidth) * dir.y;

            collisionResult.dir.y = dir.y;
            collisionResult.verticalHitted = true;
            collisionResult.verticalHittedLayer = hit.transform.gameObject.layer;
        }
    }

    /// <summary>
    /// 碰撞体信息
    /// </summary>
    public struct Collider3DInfo
    {
        /// <summary>
        /// 中心
        /// </summary>
        public Vector3 center;

        /// <summary>
        /// 皮肤
        /// </summary>
        public Single skin;

        /// <summary>
        /// 不包含skin
        /// </summary>
        public Single smallRadius;

        /// <summary>
        /// 包含skin
        /// </summary>
        public Single bigRadius;

        /// <summary>
        /// 高度
        /// </summary>
        public Single height;

        public Vector3 lowPos;
        public Vector3 highPos;

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="size">碰撞体大小</param>
        /// <param name="skin">皮肤厚度</param>
        public void Initialize(Vector3 offset, Vector3 center, Single skin, Single height, Single radius)
        {
            this.skin = skin;
            this.center = offset + center;

            this.height = height;
            this.smallRadius = radius - skin;
            this.bigRadius = radius;

            Single yOffset = (height - radius * 2) / 2;
            yOffset = yOffset > 0 ? yOffset : 0;

            this.lowPos = this.center + Vector3.down * yOffset;
            this.highPos = this.center + Vector3.up * yOffset;
        }

        public override string ToString()
        {
            return $"Collider3DInfo:\n" +
                $"  center:\t{center}\n" +
                $"  skin:\t{skin}\n" +
                $"  height:\t{height}\n" +
                $"  smallRadius:\t{smallRadius}\n" +
                $"  bigRadius:\t{bigRadius}\n" +
                $"  lowPos:\t{lowPos}\n" +
                $"  highPos:\t{highPos}";
        }
    }

    /// <summary>
    /// 碰撞结果
    /// </summary>
    public struct Collision3DResult
    {
        public Vector3 dir;
        public bool horizontalHitted;
        public bool verticalHitted;
        public int horizontalHittedLayer;
        public int verticalHittedLayer;
        public bool groundHitted;

        /// <summary>
        /// 重置
        /// </summary>
        public void Initialize()
        {
            dir = Vector3.zero;
            horizontalHitted = false;
            verticalHitted = false;
            horizontalHittedLayer = Physics.IgnoreRaycastLayer;
            verticalHittedLayer = Physics.IgnoreRaycastLayer;
            groundHitted = false;
        }

        public override string ToString()
        {
            return $"Collision3DResult:\n" +
                $"  dir:\t{dir}\n" +
                $"  horizontalHitted:\t{horizontalHitted}\n" +
                $"  verticalHitted:\t{verticalHitted}\n" +
                $"  horizontalHittedLayer:\t{horizontalHittedLayer}\n" +
                $"  verticalHittedLayer:\t{verticalHittedLayer}\n" +
                $"  groundHitted:\t{groundHitted}\n";
        }
    }
}