using FPPhysics.CollisionTests.Manifolds;

namespace FPPhysics.NarrowPhaseSystems.Pairs
{
    ///<summary>
    /// Handles a terrain-sphere collision pair.
    ///</summary>
    public sealed class TerrainSpherePairHandler : TerrainPairHandler
    {
        private TerrainSphereContactManifold contactManifold = new TerrainSphereContactManifold();
        protected override TerrainContactManifold TerrainManifold
        {
            get { return contactManifold; }
        }

    }

}
