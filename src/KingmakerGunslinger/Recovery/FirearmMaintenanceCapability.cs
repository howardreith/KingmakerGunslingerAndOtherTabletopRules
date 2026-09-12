using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Shared explicit maintenance-capability policy: a legitimate gunsmith
    /// is a living, conscious unit that currently holds the real Gunsmithing
    /// feature fact (any supported grant route). Used at field-repair
    /// availability, command start, delivery, and completed-rest participant
    /// selection, so a lost fact or an unable-to-act/dead unit can never
    /// supply the repair capability even when the ability is still visible
    /// on an old action bar.
    /// </summary>
    internal static class FirearmMaintenanceCapability
    {
        internal static bool CanMaintainFirearms(UnitDescriptor caster)
        {
            if (caster == null || caster.State == null)
            {
                return false;
            }

            if (caster.State.IsDead || caster.State.IsUnconscious)
            {
                return false;
            }

            GunslingerClassBlueprintSet gunslinger =
                BlueprintBootstrap.GunslingerClass;
            return gunslinger != null &&
                caster.HasFact(gunslinger.Gunsmithing);
        }

        internal static bool IsLivingParticipant(
            Kingmaker.EntitySystem.Entities.UnitEntityData unit)
        {
            return unit != null &&
                unit.Descriptor != null &&
                unit.Descriptor.State != null &&
                !unit.Descriptor.State.IsDead;
        }
    }
}
