namespace FPPhysics
{
    /// <summary>
    /// Provides XNA-like ray functionality.
    /// </summary>
    public struct Ray
    {
        /// <summary>
        /// Starting position of the ray.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Direction in which the ray points.
        /// </summary>
        public Vector3 Direction;


        /// <summary>
        /// Constructs a new ray.
        /// </summary>
        /// <param name="position">Starting position of the ray.</param>
        /// <param name="direction">Direction in which the ray points.</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            this.Position = position;
            this.Direction = direction;
        }



        /// <summary>
        /// Determines if and when the ray intersects the bounding box.
        /// </summary>
        /// <param name="boundingBox">Bounding box to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(ref BoundingBox boundingBox, out Fix64 t)
        {
            Fix64 tmin = Fix64.C0, tmax = Fix64.MaxValue;
            if (Fix64.Abs(Direction.x) < Toolbox.Epsilon)
            {
                if (Position.x < boundingBox.Min.x || Position.x > boundingBox.Max.x)
                {
                    //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                    //can't be intersecting.
                    t = Fix64.C0;
                    return false;
                }
            }
            else
            {
                var inverseDirection = Fix64.C1 / Direction.x;
                var t1 = (boundingBox.Min.x - Position.x) * inverseDirection;
                var t2 = (boundingBox.Max.x - Position.x) * inverseDirection;
                if (t1 > t2)
                {
                    Fix64 temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                tmin = MathHelper.Max(tmin, t1);
                tmax = MathHelper.Min(tmax, t2);
                if (tmin > tmax)
                {
                    t = Fix64.C0;
                    return false;
                }
            }
            if (Fix64.Abs(Direction.y) < Toolbox.Epsilon)
            {
                if (Position.y < boundingBox.Min.y || Position.y > boundingBox.Max.y)
                {
                    //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                    //can't be intersecting.
                    t = Fix64.C0;
                    return false;
                }
            }
            else
            {
                var inverseDirection = Fix64.C1 / Direction.y;
                var t1 = (boundingBox.Min.y - Position.y) * inverseDirection;
                var t2 = (boundingBox.Max.y - Position.y) * inverseDirection;
                if (t1 > t2)
                {
                    Fix64 temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                tmin = MathHelper.Max(tmin, t1);
                tmax = MathHelper.Min(tmax, t2);
                if (tmin > tmax)
                {
                    t = Fix64.C0;
                    return false;
                }
            }
            if (Fix64.Abs(Direction.z) < Toolbox.Epsilon)
            {
                if (Position.z < boundingBox.Min.z || Position.z > boundingBox.Max.z)
                {
                    //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                    //can't be intersecting.
                    t = Fix64.C0;
                    return false;
                }
            }
            else
            {
                var inverseDirection = Fix64.C1 / Direction.z;
                var t1 = (boundingBox.Min.z - Position.z) * inverseDirection;
                var t2 = (boundingBox.Max.z - Position.z) * inverseDirection;
                if (t1 > t2)
                {
                    Fix64 temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                tmin = MathHelper.Max(tmin, t1);
                tmax = MathHelper.Min(tmax, t2);
                if (tmin > tmax)
                {
                    t = Fix64.C0;
                    return false;
                }
            }
            t = tmin;
            return true;
        }

        /// <summary>
        /// Determines if and when the ray intersects the bounding box.
        /// </summary>
        /// <param name="boundingBox">Bounding box to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(BoundingBox boundingBox, out Fix64 t)
        {
            return Intersects(ref boundingBox, out t);
        }

        /// <summary>
        /// Determines if and when the ray intersects the plane.
        /// </summary>
        /// <param name="plane">Plane to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(ref Plane plane, out Fix64 t)
        {
            Fix64 velocity;
            Vector3.Dot(ref Direction, ref plane.Normal, out velocity);
            if (Fix64.Abs(velocity) < Toolbox.Epsilon)
            {
                t = Fix64.C0;
                return false;
            }
            Fix64 distanceAlongNormal;
            Vector3.Dot(ref Position, ref plane.Normal, out distanceAlongNormal);
            distanceAlongNormal += plane.D;
            t = -distanceAlongNormal / velocity;
            return t >= -Toolbox.Epsilon;
        }

        /// <summary>
        /// Determines if and when the ray intersects the plane.
        /// </summary>
        /// <param name="plane">Plane to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(Plane plane, out Fix64 t)
        {
            return Intersects(ref plane, out t);
        }

        /// <summary>
        /// Computes a point along a ray given the length along the ray from the ray position.
        /// </summary>
        /// <param name="t">Length along the ray from the ray position in terms of the ray's direction.</param>
        /// <param name="v">Point along the ray at the given location.</param>
        public void GetPointOnRay(Fix64 t, out Vector3 v)
        {
            Vector3.Multiply(ref Direction, t, out v);
            Vector3.Add(ref v, ref Position, out v);
        }
    }
}
