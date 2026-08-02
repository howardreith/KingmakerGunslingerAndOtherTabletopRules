using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Deeds
{
    internal static class EvasiveRuntime
    {
        internal static void Refresh(UnitDescriptor owner,
            BlueprintScriptableObject resource)
        {
            if (owner == null || resource == null ||
                BlueprintBootstrap.GunslingerClass == null ||
                !ReferenceEquals(resource,
                    BlueprintBootstrap.GunslingerClass.Grit.Resource)) return;
            if (!owner.HasFact(BlueprintBootstrap.GunslingerClass.Evasive.Feature))
                return;
            Fact fact = owner.GetFact(
                BlueprintBootstrap.GunslingerClass.Evasive.Feature);
            EvasiveGrantController controller = fact == null ? null :
                fact.Get<EvasiveGrantController>();
            if (controller != null) controller.Refresh();
        }
    }
}
