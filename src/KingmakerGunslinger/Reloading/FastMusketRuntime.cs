using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Reloading
{
    internal static class FastMusketRuntime
    {
        private static BlueprintFeature _feature;
        private static BlueprintFeature _trueGritChoice;

        internal static void Configure(BlueprintFeature feature,
            BlueprintFeature trueGritChoice)
        {
            _feature = feature;
            _trueGritChoice = trueGritChoice;
        }

        internal static bool IsAvailable(UnitDescriptor owner)
        {
            if (owner == null || _feature == null || !owner.HasFact(_feature))
                return false;
            if (_trueGritChoice != null && owner.HasFact(_trueGritChoice))
                return true;
            return BlueprintBootstrap.GunslingerClass != null &&
                BlueprintBootstrap.GunslingerClass.Grit != null &&
                owner.Resources.GetResourceAmount(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource) > 0;
        }

        internal static void Rollback()
        {
            _feature = null;
            _trueGritChoice = null;
        }
    }
}
