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

            // Same native action-capability idiom the ammunition crafting
            // availability uses: alive, conscious, and actually able to act
            // (review CR2-03 - merely living and awake is not ability).
            if (caster.State.IsDead ||
                !caster.State.IsConscious ||
                !caster.State.CanAct)
            {
                return false;
            }

            return HoldsGunsmithingFact(caster);
        }

        /// <summary>
        /// Capability at the completed-rest boundary. The native
        /// ApplySleepingState call that lifts camping sleep runs INSIDE the
        /// StopRestProcess coroutine (IL of &lt;StopRestProcess&gt;d__76,
        /// IL_03c5) - after the completion prefix - so a validly-resting
        /// camper can still carry the native Sleeping condition here. Such
        /// transient camping sleep is accepted; genuine post-rest
        /// incapacity (death, unconscious life-state, or non-sleep
        /// inability to act) is rejected.
        /// </summary>
        internal static bool CanMaintainFirearmsAtCompletedRest(
            UnitDescriptor caster)
        {
            if (caster == null || caster.State == null)
            {
                return false;
            }

            if (caster.State.IsDead || caster.State.IsUnconscious)
            {
                return false;
            }

            if (!HoldsGunsmithingFact(caster))
            {
                return false;
            }

            return caster.State.CanAct ||
                caster.State.HasCondition(UnitCondition.Sleeping);
        }

        internal static bool IsLivingParticipant(
            Kingmaker.EntitySystem.Entities.UnitEntityData unit)
        {
            return unit != null &&
                unit.Descriptor != null &&
                unit.Descriptor.State != null &&
                !unit.Descriptor.State.IsDead;
        }

        private static bool HoldsGunsmithingFact(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger =
                BlueprintBootstrap.GunslingerClass;
            return gunslinger != null &&
                caster.HasFact(gunslinger.Gunsmithing);
        }
    }
}
