﻿using FPPhysics.CollisionTests.Manifolds;

namespace FPPhysics.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a static mesh-convex collision pair.
    ///</summary>
    public class StaticMeshConvexPairHandler : StaticMeshPairHandler
    {

        StaticMeshConvexContactManifold contactManifold = new StaticMeshConvexContactManifold();
        protected override StaticMeshContactManifold MeshManifold
        {
            get { return contactManifold; }
        }


    }

}
