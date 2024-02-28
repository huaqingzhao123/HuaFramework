using FPPhysics.BroadPhaseEntries;
using FPPhysics.BroadPhaseEntries.MobileCollidables;
using FPPhysics.CollisionTests.CollisionAlgorithms.GJK;
using FPPhysics.CollisionTests.Manifolds;
using FPPhysics.Constraints.Collision;
using FPPhysics.PositionUpdating;
using FPPhysics.Settings;
using System;

namespace FPPhysics.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a static mesh-convex collision pair.
    ///</summary>
    public abstract class StaticMeshPairHandler : StandardPairHandler
    {
        StaticMesh mesh;
        ConvexCollidable convex;

        private NonConvexContactManifoldConstraint contactConstraint;


        public override Collidable CollidableA
        {
            get { return convex; }
        }
        public override Collidable CollidableB
        {
            get { return mesh; }
        }
        public override Entities.Entity EntityA
        {
            get { return convex.entity; }
        }
        public override Entities.Entity EntityB
        {
            get { return null; }
        }
        /// <summary>
        /// Gets the contact constraint used by the pair handler.
        /// </summary>
        public override ContactManifoldConstraint ContactConstraint
        {
            get { return contactConstraint; }
        }
        /// <summary>
        /// Gets the contact manifold used by the pair handler.
        /// </summary>
        public override ContactManifold ContactManifold
        {
            get { return MeshManifold; }
        }

        protected abstract StaticMeshContactManifold MeshManifold { get; }

        protected StaticMeshPairHandler()
        {
            contactConstraint = new NonConvexContactManifoldConstraint(this);
        }

        ///<summary>
        /// Initializes the pair handler.
        ///</summary>
        ///<param name="entryA">First entry in the pair.</param>
        ///<param name="entryB">Second entry in the pair.</param>
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {

            mesh = entryA as StaticMesh;
            convex = entryB as ConvexCollidable;

            if (mesh == null || convex == null)
            {
                mesh = entryB as StaticMesh;
                convex = entryA as ConvexCollidable;

                if (mesh == null || convex == null)
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
            }

            //Contact normal goes from A to B.
            broadPhaseOverlap.entryA = convex;
            broadPhaseOverlap.entryB = mesh;

            UpdateMaterialProperties(convex.entity != null ? convex.entity.material : null, mesh.material);

            base.Initialize(entryA, entryB);




        }


        ///<summary>
        /// Cleans up the pair handler.
        ///</summary>
        public override void CleanUp()
        {
            base.CleanUp();

            mesh = null;
            convex = null;
        }



        ///<summary>
        /// Updates the time of impact for the pair.
        ///</summary>
        ///<param name="requester">Collidable requesting the update.</param>
        ///<param name="dt">Timestep duration.</param>
        public override void UpdateTimeOfImpact(Collidable requester, Fix64 dt)
        {
            //Notice that we don't test for convex entity null explicitly.  The convex.IsActive property does that for us.
            if (convex.IsActive && convex.entity.PositionUpdateMode == PositionUpdateMode.Continuous)
            {
                //TODO: This system could be made more robust by using a similar region-based rejection of edges.
                //CCD events are awfully rare under normal circumstances, so this isn't usually an issue.

                //Only perform the test if the minimum radii are small enough relative to the size of the velocity.
                Vector3 velocity;
                Vector3.Multiply(ref convex.entity.linearVelocity, dt, out velocity);
                Fix64 velocitySquared = velocity.LengthSquared();

                var minimumRadius = convex.Shape.MinimumRadius * MotionSettings.CoreShapeScaling;
                timeOfImpact = Fix64.C1;
                if (minimumRadius * minimumRadius < velocitySquared)
                {
                    var triangle = PhysicsThreadResources.GetTriangle();
                    triangle.collisionMargin = Fix64.C0;
                    //Spherecast against all triangles to find the earliest time.
                    for (int i = 0; i < MeshManifold.overlappedTriangles.Count; i++)
                    {
                        mesh.Shape.TriangleMeshData.GetTriangle(MeshManifold.overlappedTriangles.Elements[i], out triangle.vA, out triangle.vB, out triangle.vC);
                        //Put the triangle into 'localish' space of the convex.
                        Vector3.Subtract(ref triangle.vA, ref convex.worldTransform.Position, out triangle.vA);
                        Vector3.Subtract(ref triangle.vB, ref convex.worldTransform.Position, out triangle.vB);
                        Vector3.Subtract(ref triangle.vC, ref convex.worldTransform.Position, out triangle.vC);

                        RayHit rayHit;
                        if (GJKToolbox.CCDSphereCast(new Ray(Toolbox.ZeroVector, velocity), minimumRadius, triangle, ref Toolbox.RigidIdentity, timeOfImpact, out rayHit) &&
                            rayHit.T > Toolbox.BigEpsilon)
                        {
                            if (mesh.sidedness != TriangleSidedness.DoubleSided)
                            {
                                Vector3 AB, AC;
                                Vector3.Subtract(ref triangle.vB, ref triangle.vA, out AB);
                                Vector3.Subtract(ref triangle.vC, ref triangle.vA, out AC);
                                Vector3 normal;
                                Vector3.Cross(ref AB, ref AC, out normal);
                                Fix64 dot;
                                Vector3.Dot(ref normal, ref rayHit.Normal, out dot);
                                //Only perform sweep if the object is in danger of hitting the object.
                                //Triangles can be one sided, so check the impact normal against the triangle normal.
                                if (mesh.sidedness == TriangleSidedness.Counterclockwise && dot < Fix64.C0 ||
                                    mesh.sidedness == TriangleSidedness.Clockwise && dot > Fix64.C0)
                                {
                                    timeOfImpact = rayHit.T;
                                }
                            }
                            else
                            {
                                timeOfImpact = rayHit.T;
                            }
                        }
                    }
                    PhysicsThreadResources.GiveBack(triangle);
                }



            }

        }



        protected internal override void GetContactInformation(int index, out ContactInformation info)
        {
            info.Contact = MeshManifold.contacts.Elements[index];
            //Find the contact's normal and friction forces.
            info.FrictionImpulse = Fix64.C0;
            info.NormalImpulse = Fix64.C0;
            for (int i = 0; i < contactConstraint.frictionConstraints.Count; i++)
            {
                if (contactConstraint.frictionConstraints.Elements[i].PenetrationConstraint.contact == info.Contact)
                {
                    info.FrictionImpulse = contactConstraint.frictionConstraints.Elements[i].accumulatedImpulse;
                    info.NormalImpulse = contactConstraint.frictionConstraints.Elements[i].PenetrationConstraint.accumulatedImpulse;
                    break;
                }
            }

            //Compute relative velocity
            if (convex.entity != null)
            {
                Vector3 velocity;
                Vector3.Subtract(ref info.Contact.Position, ref convex.entity.position, out velocity);
                Vector3.Cross(ref convex.entity.angularVelocity, ref velocity, out velocity);
                Vector3.Add(ref velocity, ref convex.entity.linearVelocity, out info.RelativeVelocity);
            }
            else info.RelativeVelocity = new Vector3();

            info.Pair = this;
        }
    }

}
