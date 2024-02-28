using System.Collections;
using System.Collections.Generic;
using FPPhysics.BroadPhaseEntries.MobileCollidables;
using FPPhysics.CollisionShapes.ConvexShapes;
using UnityEngine;
using Vector3 = FPPhysics.Vector3;

namespace FPPhysics.CollisionShapes.ConvexShapes
{
    public class TuoYuanShape : ConvexShape
    {
        private Fix64 height;
        internal Fix64 halfHeight;
        internal Fix64 halfRadius;
        public Fix64 Height
        {
            get { return height; }
            set
            {
                height = value;
                OnShapeChanged();
            }
        }
        private Fix64 radius;
        
        public Fix64 Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                OnShapeChanged();
            }
        }
        ///<summary>
        /// Constructs a new cone shape.
        ///</summary>
        ///<param name="height">Height of the cone.</param>
        ///<param name="radius">Radius of the cone base.</param>
        public TuoYuanShape(Fix64 height, Fix64 radius)
        {
            this.height = height;
            this.radius = radius;
            halfHeight = height * Fix64.C0p5;
            halfRadius = radius * Fix64.C0p5;
            UpdateConvexShapeInfo(ComputeDescription(height, radius, collisionMargin));
        }
        public TuoYuanShape(Fix64 height, Fix64 radius, ConvexShapeDescription description)
        {
            this.height = height;
            this.radius = radius;
            halfHeight = height * Fix64.C0p5;
            halfRadius = radius * Fix64.C0p5;
            UpdateConvexShapeInfo(description);
        }
        protected override void OnShapeChanged()
        {
            UpdateConvexShapeInfo(ComputeDescription(height, radius, collisionMargin));
            base.OnShapeChanged();
        }
        public static ConvexShapeDescription ComputeDescription(Fix64 height, Fix64 radius, Fix64 collisionMargin)
        {
            ConvexShapeDescription description;
            description.EntityShapeVolume.Volume = MathHelper.Pi * height * Fix64.C0p5 * radius * Fix64.C0p5;

            description.EntityShapeVolume.VolumeDistribution = new Matrix3x3();
            Fix64 diagValue = (Fix64.C0p1 * height * height + Fix64.C0p15 * radius * radius);
            description.EntityShapeVolume.VolumeDistribution.M11 = diagValue;
            description.EntityShapeVolume.VolumeDistribution.M22 = Fix64.C0p3 * radius * radius;
            description.EntityShapeVolume.VolumeDistribution.M33 = diagValue;

            description.MaximumRadius = collisionMargin + MathHelper.Max(Fix64.C0p75 * height, Fix64.Sqrt(Fix64.C0p0625 * height * height + radius * radius));

            Fix64 denominator = radius / height;
            denominator = denominator / Fix64.Sqrt(denominator * denominator + Fix64.C1);
            description.MinimumRadius = collisionMargin + MathHelper.Min(Fix64.C0p25 * height, denominator * Fix64.C0p75 * height);

            description.CollisionMargin = collisionMargin;
            return description;
        }
        public override EntityCollidable GetCollidableInstance()
        {
            return new ConvexCollidable<TuoYuanShape>(this);
        }

        public override void GetLocalExtremePointWithoutMargin(ref Vector3 direction, out Vector3 extremePoint)
        {
            extremePoint = new Vector3(Fix64.Sign(direction.x) * (halfRadius - collisionMargin), Fix64.Sign(direction.y) * (halfHeight - collisionMargin), Fix64.Sign(direction.z) * (halfHeight - collisionMargin) * 4);
        }
    }
}

