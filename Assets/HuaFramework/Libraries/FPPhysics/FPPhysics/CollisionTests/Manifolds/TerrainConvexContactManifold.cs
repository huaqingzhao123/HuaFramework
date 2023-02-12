using FPPhysics.CollisionTests.CollisionAlgorithms;
using FPPhysics.ResourceManagement;

namespace FPPhysics.CollisionTests.Manifolds
{
    public class TerrainConvexContactManifold : TerrainContactManifold
    {
        static LockingResourcePool<TriangleConvexPairTester> testerPool = new LockingResourcePool<TriangleConvexPairTester>();
        protected override TrianglePairTester GetTester()
        {
            return testerPool.Take();
        }

        protected override void GiveBackTester(TrianglePairTester tester)
        {
            testerPool.GiveBack((TriangleConvexPairTester)tester);
        }

    }
}
