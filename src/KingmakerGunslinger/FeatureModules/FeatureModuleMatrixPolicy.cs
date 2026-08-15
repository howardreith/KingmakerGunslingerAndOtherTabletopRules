using System;

namespace KingmakerGunslinger.FeatureModules
{
    internal static class FeatureModuleMatrixPolicy
    {
        internal static int ExhaustiveCount(int moduleCount)
        {
            RequireSupportedCount(moduleCount);
            return 1 << moduleCount;
        }

        internal static int BoundaryCount(int moduleCount)
        {
            RequireSupportedCount(moduleCount);
            return 2 + 2 * moduleCount;
        }

        internal static bool IsBoundaryState(int moduleCount, int enabledCount)
        {
            RequireSupportedCount(moduleCount);
            if (enabledCount < 0 || enabledCount > moduleCount)
                throw new ArgumentOutOfRangeException("enabledCount");
            return enabledCount == 0 || enabledCount == 1 ||
                enabledCount == moduleCount - 1 || enabledCount == moduleCount;
        }

        private static void RequireSupportedCount(int moduleCount)
        {
            if (moduleCount < 2 || moduleCount > 30)
                throw new ArgumentOutOfRangeException("moduleCount");
        }
    }
}
