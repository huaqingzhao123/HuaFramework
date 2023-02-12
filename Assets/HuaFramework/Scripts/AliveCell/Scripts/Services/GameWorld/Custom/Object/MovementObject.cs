/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/28 14:13:15
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.Extensions;

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
    /// MovementObject
    /// </summary>
    public abstract class MovementObject : TObject
    {
        public Vector3 center { get; set; }
        public Single height { get; set; }
        public Single radius { get; set; }
        public Single skinWidth { get; set; }
        public LayerMask obstacleMask { get; set; }
        public QueryTriggerInteraction triggerQuery { get; set; }
        public Single slopeLimit { get; set; }
        public bool slowAgainstWalls { get; set; }
        public Single minSlowAgainstWallsAngle { get; set; }
        public int layer { get; set; }

        public CollisionInfo collisionInfo { get => _collisionInfo; protected set => _collisionInfo = value; }
        public ColliderController collider { get; private set; }

        private CollisionInfo _collisionInfo;

        private CapsuleCollider capsuleCollider => collider.volumeCollider;
        private readonly List<MoveVector> moveVectors = new List<MoveVector>();
        private readonly Collider[] ColliderBuffers = new Collider[8];
        private Single invRescaleFactor;

        private const int MaxMoveIterations = 20;
        private static readonly Single CollisionOffset = 0.001m;
        private static readonly Single MaxAngleToUseRaycastNormal = 5;
        private static readonly Single RaycastScaleDistance = 2;

        public override void OnReset()
        {
            base.OnReset();

            center = Vector3.up * 1;
            height = 2;
            radius = 0.5m;
            skinWidth = 0.04m;
            obstacleMask = Physics.DefaultRaycastLayers;
            triggerQuery = QueryTriggerInteraction.UseGlobal;
            slopeLimit = 45;
            slowAgainstWalls = false;
            minSlowAgainstWallsAngle = 10;
            layer = Physics.DefaultRaycastLayers;
            collider = null;
            _collisionInfo = default;
            moveVectors.Clear();
            invRescaleFactor = 0;
        }

        public override void OnInitialized()
        {
            base.OnInitialized();

            invRescaleFactor = 1 / Mathf.Cos(minSlowAgainstWallsAngle * Mathf.Deg2Rad);

            GameObject obj = App.res.CreateGO(10000001);
#if UNITY_EDITOR
            obj.name = $"[{ID}]{GetType().Name}(Collider)";
#endif
            collider = obj.GetComponent<ColliderController>();
            collider.Initialization(this);
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

        public Vector3 topSphereWorldPosition => GetTopSphereWorldPosition(position);
        public Vector3 bottomSphereWorldPosition => GetBottomSphereWorldPosition(position);
        public Single smallRadius => radius;
        public Single bigRadius => radius + skinWidth;

        private Vector3 GetTopSphereWorldPosition(Vector3 position) => position + center + Vector3.up * (height / 2 - radius);

        private Vector3 GetBottomSphereWorldPosition(Vector3 position) => position + center + Vector3.down * (height / 2 - radius);

        public void Translate(Vector3 move)
        {
            TranslateLoop(move);
            collider.UpdateCollider();
        }

        public struct CollisionInfo
        {
            public bool horizontalHitted;
            public bool verticalHitted;
            public bool groundHitted;
        }

        private struct MoveVector
        {
            public Vector3 move { get; set; }
            public bool canSlide { get; set; }

            public MoveVector(Vector3 move, bool canSlide)
            {
                this.move = move;
                this.canSlide = canSlide;
            }
        }

        private void TranslateLoop(Vector3 move)
        {
            Vector3 virutalPosition = position;

            collisionInfo = default;

            UpdateMoveVector(moveVectors, move);
            if (moveVectors.Count == 0)
            {
                //if (Depenetrate(ref virutalPosition))
                //{//没有移动时处理
                //    position = virutalPosition;
                //}
                return;
            }

            int nextIndex = 0;

            MoveVector remainingMove = moveVectors[nextIndex];
            nextIndex++;

            for (int i = 0; i < MaxMoveIterations; i++)
            {
                Vector3 refMove = remainingMove.move;

                bool collided = TranslateMajorStep(ref refMove, remainingMove.canSlide, ref virutalPosition);

                remainingMove.move = refMove;

                if (collided)
                {
                    remainingMove.canSlide = false;
                }

                if (!collided || remainingMove.move.sqrMagnitude.IsEqualToZero())
                {//没有发生碰撞或当前移动完成
                    if (nextIndex < moveVectors.Count)
                    {//还有未移动部分
                        remainingMove = moveVectors[nextIndex];
                        nextIndex++;
                    }
                    else
                    {//全都执行完成，则跳出
                        break;
                    }
                }

#if UNITY_EDITOR
                if (i == MaxMoveIterations - 1)
                {
                    Debug.LogWarning($"达到迭代移动次数上限");
                }
#endif
            }

            position = virutalPosition;
        }

        private bool TranslateMajorStep(ref Vector3 move, bool canSlide, ref Vector3 currentPosition)
        {
            Single distance = move.magnitude;
            Vector3 direction = move.normalized;

            if (!CapsuleCast(direction, distance, currentPosition
            , out bool smallRadiusHit, out bool bigRadiusHit
            , out RaycastHit smallRadiusHitInfo, out RaycastHit bigRadiusHitInfo,
            Vector3.zero))
            {//未发生碰撞
                TranslatePosition(move, ref currentPosition);

                if (GetPenetrationInfo(out Single penetrationDistance, out Vector3 penetrationDirection, currentPosition))
                {//处理渗透
                    TranslatePosition(penetrationDirection * penetrationDistance, ref currentPosition);
                }

                move = Vector3.zero;

                return false;
            }

            // 发生碰撞
            if (!bigRadiusHit)
            {//小半径发生碰撞，大半经未发生碰撞
                TranslateAwayFromObstacle(ref move, smallRadiusHitInfo, direction, distance, canSlide, true, ref currentPosition);
                return true;
            }

            if (smallRadiusHit && smallRadiusHitInfo.distance < bigRadiusHitInfo.distance)
            {//都发生碰撞，如果小半径更近，则同上
                TranslateAwayFromObstacle(ref move, smallRadiusHitInfo, direction, distance, canSlide, true, ref currentPosition);
                return true;
            }

            //取大半经
            TranslateAwayFromObstacle(ref move, bigRadiusHitInfo, direction, distance, canSlide, false, ref currentPosition);

            return true;
        }

        private void TranslateAwayFromObstacle(ref Vector3 move, in RaycastHit hitInfoCapsule, Vector3 direction, Single distance, bool canSlide, bool hitSmallCapsule, ref Vector3 currentPosition)
        {
            var collisionOffset = hitSmallCapsule ? skinWidth : CollisionOffset;
            var hitDistance = Mathf.Max((Single)hitInfoCapsule.distance - collisionOffset, 0);//需要移动的距离
            var remainingDistance = Mathf.Max(distance - (Single)hitInfoCapsule.distance, 0);//剩余需要移动的距离

            //先移动发生碰撞的距离
            TranslatePosition(direction * hitDistance, ref currentPosition, direction, hitInfoCapsule);

            var rayOrigin = currentPosition + center;
            var rayDirection = hitInfoCapsule.point - rayOrigin;

            Vector3 hitNormal;
            //使用射线再检测一遍碰撞点，结果更准
            if (Physics.Raycast(rayOrigin,
                                          rayDirection,
                                          out RaycastHit hitInfoRay,
                                          rayDirection.magnitude * RaycastScaleDistance,
                                          obstacleMask,
                                          triggerQuery) &&
                hitInfoRay.collider == hitInfoCapsule.collider &&
                Vector3.Angle((Vector3)hitInfoCapsule.normal, (Vector3)hitInfoRay.normal) <= MaxAngleToUseRaycastNormal)
            {
                hitNormal = (Vector3)hitInfoRay.normal;
            }
            else
            {
                hitNormal = (Vector3)hitInfoCapsule.normal;
            }

            if (GetPenetrationInfo(out Single penetrationDistance, out Vector3 penetrationDirection, currentPosition, true, null, hitInfoCapsule))
            {
                TranslatePosition(penetrationDirection * penetrationDistance, ref currentPosition);
            }

            if (canSlide && remainingDistance > 0.0f)
            {
                var slideNormal = hitNormal;

                slideNormal.y = 0;//仅在xz平面
                slideNormal.Normalize();

                var project = Vector3.Cross(direction, slideNormal);
                project = Vector3.Cross(slideNormal, project);

                project.y = 0;//仅在xz平面
                project.Normalize();

                var isWallSlowingDown = slowAgainstWalls && minSlowAgainstWallsAngle < 90;

                if (isWallSlowingDown)
                {
                    var cosine = Vector3.Dot(project, direction);
                    var slowDownFactor = Mathf.Clamp01(cosine * invRescaleFactor);

                    move = project * (remainingDistance * slowDownFactor);
                }
                else
                {
                    move = project * remainingDistance;
                }
            }
            else
            {
                move = Vector3.zero;
            }
        }

        private bool GetPenetrationInfo(out Single getDistance, out Vector3 getDirection, Vector3 currentPosition,
                                        bool includeSkinWidth = true,
                                        Vector3? offsetPosition = null,
                                        RaycastHit? hitInfo = null)
        {
            getDistance = 0;
            getDirection = Vector3.zero;

            var offset = offsetPosition != null ? offsetPosition.Value : Vector3.zero;
            var tempSkinWidth = includeSkinWidth ? skinWidth : 0;
            var overlapCount = Physics.OverlapCapsuleNonAlloc(GetTopSphereWorldPosition(currentPosition) + offset,
                                                              GetBottomSphereWorldPosition(currentPosition) + offset,
                                                              radius + tempSkinWidth,
                                                              ColliderBuffers,
                                                              obstacleMask,
                                                              triggerQuery);
            if (overlapCount <= 0 || ColliderBuffers.Length <= 0)
            {
                return false;
            }

            bool result = false;
            Vector3 localPos = Vector3.zero;
            for (var i = 0; i < overlapCount; i++)
            {
                var collider = ColliderBuffers[i];
                if (collider == null)
                {
                    break;
                }

                Transform colliderTransform = collider.transform;
                if (ComputePenetration(offset,
                                       collider, (Vector3)colliderTransform.position, (Quaternion)colliderTransform.rotation,
                                       out Vector3 direction, out Single distance, includeSkinWidth, currentPosition))
                {
                    localPos += direction * (distance + CollisionOffset);
                    result = true;
                }
                else if (hitInfo != null && hitInfo.Value.collider == collider)
                {
                    localPos += (Vector3)hitInfo.Value.normal * CollisionOffset;
                    result = true;
                }
            }

            if (result)
            {
                getDistance = localPos.magnitude;
                getDirection = localPos.normalized;
            }

            return result;
        }

        private bool ComputePenetration(Vector3 positionOffset, Collider collider, Vector3 colliderPosition, Quaternion colliderRotation, out Vector3 direction, out Single distance, bool includeSkinWidth, Vector3 currentPosition)
        {
            if (collider == this.capsuleCollider)
            {
                // Ignore self
                direction = Vector3.one;
                distance = 0;
                return false;
            }

            if (includeSkinWidth)
            {
                this.capsuleCollider.radius = radius + skinWidth;
                this.capsuleCollider.height = height + (skinWidth * 2);
            }

            var result = Physics.ComputePenetration(capsuleCollider,
                                                    currentPosition + positionOffset,
                                                    Quaternion.identity,
                                                    collider, colliderPosition, colliderRotation,
                                                    out var _direction, out var _distance);
            direction = (Vector3)_direction;
            distance = (Single)_distance;

            if (includeSkinWidth)
            {
                this.capsuleCollider.radius = radius;
                this.capsuleCollider.height = height;
            }

            return result;
        }

        private bool Depenetrate(ref Vector3 currentPosition)
        {
            if (GetPenetrationInfo(out Single distance, out Vector3 direction, currentPosition))
            {
                TranslatePosition(direction * distance, ref currentPosition);
                return true;
            }

            return false;
        }

        private void TranslatePosition(Vector3 move, ref Vector3 currentPosition, Vector3? collideDirection = null, RaycastHit? hitInfo = null)
        {
            if (move.sqrMagnitude.NotEqualToZero())
            {
                currentPosition += move;
            }

            if (hitInfo != null && collideDirection != null)
            {
                UpdateCollisionInfo(currentPosition, hitInfo.Value, collideDirection.Value);
            }
        }

        private void UpdateCollisionInfo(Vector3 currentPosition, in RaycastHit hitInfo, Vector3 collideDirection)
        {
            if (collideDirection.y < 0)
            {
                _collisionInfo.groundHitted = true;
                _collisionInfo.verticalHitted = true;
            }

            if (collideDirection.x.NotEqualToZero() || collideDirection.y.NotEqualToZero())
            {
                _collisionInfo.verticalHitted = true;
            }
        }

        private bool CapsuleCast(Vector3 direction, Single distance, Vector3 currentPosition, out bool smallRadiusHit, out bool bigRadiusHit, out RaycastHit smallRadiusHitInfo, out RaycastHit bigRadiusHitInfo, Vector3 offsetPosition)
        {
            smallRadiusHit = SmallCapsuleCast(direction, distance, out smallRadiusHitInfo, offsetPosition, currentPosition);
            bigRadiusHit = BigCapsuleCast(direction, distance, out bigRadiusHitInfo, offsetPosition, currentPosition);
            return smallRadiusHit || bigRadiusHit;
        }

        private bool SmallCapsuleCast(Vector3 direction, Single distance, out RaycastHit smallRadiusHitInfo, Vector3 offsetPosition, Vector3 currentPosition)
        {
            Single extraDistance = radius;

            if (Physics.CapsuleCast(GetTopSphereWorldPosition(currentPosition) + offsetPosition,
                                    GetBottomSphereWorldPosition(currentPosition) + offsetPosition,
                                    radius,
                                    direction,
                                    out smallRadiusHitInfo,
                                    distance + extraDistance,
                                    obstacleMask,
                                    triggerQuery))
            {
                return smallRadiusHitInfo.distance <= distance;
            }

            return false;
        }

        public bool SmallCapsuleCast(Vector3 direction, Single distance, out RaycastHit smallRadiusHitInfo, Vector3 offsetPosition)
        {
            return SmallCapsuleCast(direction, distance, out smallRadiusHitInfo, offsetPosition, position);
        }

        private bool BigCapsuleCast(Vector3 direction, Single distance, out RaycastHit bigRadiusHitInfo, Vector3 offsetPosition, Vector3 currentPosition)
        {
            Single extraDistance = radius;

            if (Physics.CapsuleCast(GetTopSphereWorldPosition(currentPosition) + offsetPosition,
                                    GetBottomSphereWorldPosition(currentPosition) + offsetPosition,
                                    radius + skinWidth,
                                    direction,
                                    out bigRadiusHitInfo,
                                    distance + extraDistance,
                                    obstacleMask,
                                    triggerQuery))
            {
                return bigRadiusHitInfo.distance <= distance;
            }

            return false;
        }

        public bool BigCapsuleCast(Vector3 direction, Single distance, out RaycastHit bigRadiusHitInfo, Vector3 offsetPosition)
        {
            return BigCapsuleCast(direction, distance, out bigRadiusHitInfo, offsetPosition, position);
        }

        private void UpdateMoveVector(List<MoveVector> moveBufs, Vector3 move)
        {
            moveBufs.Clear();

            Vector3 horzontal = new Vector3(move.x, 0, move.z);
            Vector3 vertical = new Vector3(0, move.y, 0);

            if (horzontal.x.NotEqualToZero() || horzontal.z.NotEqualToZero())
            {
                moveBufs.Add(new MoveVector(horzontal, true));
            }

            if (vertical.y.NotEqualToZero())
            {
                moveBufs.Add(new MoveVector(vertical, false));
            }
        }
    }
}