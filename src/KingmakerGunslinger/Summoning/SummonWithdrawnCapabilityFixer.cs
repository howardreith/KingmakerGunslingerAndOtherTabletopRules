using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components.Fixers;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair sprint R1: retires a withdrawn capability from a summon saved
    /// before the withdrawal. Native <c>UnitDescriptor.PostLoad</c> calls this
    /// unit-blueprint component once per unit, recording its name, after the
    /// unit's facts have loaded.
    ///
    /// <para>Only the exact owning KMG blueprint is touched, and only the
    /// listed facts, brain actions and resources are removed. Removing a
    /// feature runs its own AddFacts deactivation, which is how the native
    /// Improved Sunder feat also takes its Sunder Armor action and
    /// ProperFlanking2's toggle with it. Already dealt damage, existing walls
    /// and other active effects are left to their own retained lifetimes.</para>
    /// </summary>
    [Serializable]
    public sealed class SummonWithdrawnCapabilityFixer : FixUnitOnPostLoad
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintUnitFact[] Facts = new BlueprintUnitFact[0];
        public BlueprintAiAction[] AiActions = new BlueprintAiAction[0];
        public BlueprintAbilityResource[] Resources = new BlueprintAbilityResource[0];

        public override void OnPostLoad(UnitDescriptor unit)
        {
            bool exactOwner = unit != null && OwningBlueprint != null &&
                ReferenceEquals(unit.Blueprint, OwningBlueprint);
            if (!exactOwner) return;
            try
            {
                BlueprintUnitFact[] facts = SummonPlaytestWithdrawalPolicy.SelectWithdrawn(
                    exactOwner, (Facts ?? new BlueprintUnitFact[0]).Where(value =>
                        value != null && unit.HasFact(value)), Facts);
                foreach (BlueprintUnitFact fact in facts) unit.RemoveFact(fact);

                int actions = 0;
                if (unit.Brain != null)
                {
                    BlueprintAiAction[] stale = SummonPlaytestWithdrawalPolicy.SelectWithdrawn(
                        exactOwner, (unit.Brain.Actions ?? Enumerable.Empty<AiAction>())
                            .Where(value => value != null).Select(value => value.Blueprint),
                        AiActions);
                    if (stale.Length != 0)
                    {
                        if (unit.Brain.Actions != null)
                            actions = unit.Brain.Actions.RemoveAll(value =>
                                value != null && stale.Contains(value.Blueprint));
                        if (unit.Brain.AvailableActions != null)
                            unit.Brain.AvailableActions.RemoveAll(value =>
                                value != null && stale.Contains(value.Blueprint));
                    }
                }

                BlueprintAbilityResource[] resources = SummonPlaytestWithdrawalPolicy.SelectWithdrawn(
                    exactOwner, (Resources ?? new BlueprintAbilityResource[0]).Where(value =>
                        value != null && unit.Resources.ContainsResource(value)), Resources);
                foreach (BlueprintAbilityResource resource in resources) unit.Resources.Remove(resource);

                ModContext context;
                if ((facts.Length != 0 || actions != 0 || resources.Length != 0) &&
                    ModContext.TryGet(out context))
                    context.Logger.Info("summoning", "r1.withdrawn-capability-retired",
                        "Retired withdrawn capability from a loaded " + OwningBlueprint.name +
                        ": facts=" + string.Join(",", facts.Select(value => value.name).ToArray()) +
                        "; aiActions=" + actions + "; resources=" + resources.Length + ".");
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Warning("summoning", "r1.withdrawn-capability-failed",
                        "Could not retire a withdrawn capability from a loaded " +
                        OwningBlueprint.name + ": " + exception.Message);
            }
        }
    }
}
