using FPPhysics.CollisionShapes.ConvexShapes;

using FPPhysics.Settings;

namespace FPPhysics.CollisionTests.CollisionAlgorithms
{
    ///<summary>
    /// Static class with methods to help with testing box shapes against sphere shapes.
    ///</summary>
    public static class BoxSphereTester
    {
        ///<summary>
        /// Tests if a box and sphere are colliding.
        ///</summary>
        ///<param name="box">Box to test.</param>
        ///<param name="sphere">Sphere to test.</param>
        ///<param name="boxTransform">Transform to apply to the box.</param>
        ///<param name="spherePosition">Transform to apply to the sphere.</param>
        ///<param name="contact">Contact point between the shapes, if any.</param>
        ///<returns>Whether or not the shapes were colliding.</returns>
        public static bool AreShapesColliding(BoxShape box, SphereShape sphere, ref RigidTransform boxTransform, ref Vector3 spherePosition, out ContactData contact)
        {
            contact = new ContactData();

            Vector3 localPosition;
            RigidTransform.TransformByInverse(ref spherePosition, ref boxTransform, out localPosition);
#if !WINDOWS
            Vector3 localClosestPoint = new Vector3();
#else
            Vector3 localClosestPoint;
#endif
            localClosestPoint.x = MathHelper.Clamp(localPosition.x, -box.halfWidth, box.halfWidth);
            localClosestPoint.y = MathHelper.Clamp(localPosition.y, -box.halfHeight, box.halfHeight);
            localClosestPoint.z = MathHelper.Clamp(localPosition.z, -box.halfLength, box.halfLength);

            RigidTransform.Transform(ref localClosestPoint, ref boxTransform, out contact.Position);

            Vector3 offset;
            Vector3.Subtract(ref spherePosition, ref contact.Position, out offset);
            Fix64 offsetLength = offset.LengthSquared();

            if (offsetLength > (sphere.collisionMargin + CollisionDetectionSettings.maximumContactDistance) * (sphere.collisionMargin + CollisionDetectionSettings.maximumContactDistance))
            {
                return false;
            }

            //Colliding.
            if (offsetLength > Toolbox.Epsilon)
            {
                offsetLength = Fix64.Sqrt(offsetLength);
                //Outside of the box.
                Vector3.Divide(ref offset, offsetLength, out contact.Normal);
                contact.PenetrationDepth = sphere.collisionMargin - offsetLength;
            }
            else
            {
                //Inside of the box.
                Vector3 penetrationDepths;
                penetrationDepths.x = localClosestPoint.x < Fix64.C0 ? localClosestPoint.x + box.halfWidth : box.halfWidth - localClosestPoint.x;
                penetrationDepths.y = localClosestPoint.y < Fix64.C0 ? localClosestPoint.y + box.halfHeight : box.halfHeight - localClosestPoint.y;
                penetrationDepths.z = localClosestPoint.z < Fix64.C0 ? localClosestPoint.z + box.halfLength : box.halfLength - localClosestPoint.z;
                if (penetrationDepths.x < penetrationDepths.y && penetrationDepths.x < penetrationDepths.z)
                {
                    contact.Normal = localClosestPoint.x > Fix64.C0 ? Toolbox.RightVector : Toolbox.LeftVector;
                    contact.PenetrationDepth = penetrationDepths.x;
                }
                else if (penetrationDepths.y < penetrationDepths.z)
                {
                    contact.Normal = localClosestPoint.y > Fix64.C0 ? Toolbox.UpVector : Toolbox.DownVector;
                    contact.PenetrationDepth = penetrationDepths.y;
                }
                else
                {
                    contact.Normal = localClosestPoint.z > Fix64.C0 ? Toolbox.BackVector : Toolbox.ForwardVector;
                    contact.PenetrationDepth = penetrationDepths.z;
                }
                contact.PenetrationDepth += sphere.collisionMargin;
                Quaternion.Transform(ref contact.Normal, ref boxTransform.Orientation, out contact.Normal);
            }


            return true;
        }
    }
}
