using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    internal static class FirearmReloadPlanner
    {
        internal static FirearmReloadPlan Evaluate(object unit, object exactItem,
            FirearmDefinition definition, FirearmState state, ReloadAmmunitionProfile profile,
            ReloadAmmunitionInventorySnapshot inventory, bool fastMusket,
            bool matchingRapidReload, int roundsRequested)
        {
            if (definition == null || state == null || profile == null || inventory == null || unit == null || exactItem == null)
                return Rejected(FirearmReloadPlanStatus.MissingContext,
                    "An exact unit, firearm, definition, state, profile, and inventory are required.",
                    unit, exactItem, definition, state, profile, inventory);
            EffectiveReloadAction action = ReloadActionEconomy.Evaluate(definition,
                fastMusket, matchingRapidReload, profile.ReloadStepReduction);
            if (!profile.IsCompatible(definition)) return Rejected(
                FirearmReloadPlanStatus.IncompatibleAmmunition, profile.CompatibilityRejection(definition),
                unit, exactItem, definition, state, profile, inventory, action);
            if (state.Condition == FirearmCondition.Wrecked) return Rejected(
                FirearmReloadPlanStatus.Wrecked, "The equipped firearm is Wrecked and cannot be reloaded.",
                unit, exactItem, definition, state, profile, inventory, action);
            if (state.LoadedRounds >= definition.Capacity) return Rejected(
                FirearmReloadPlanStatus.AlreadyLoaded, "The equipped firearm is already full.",
                unit, exactItem, definition, state, profile, inventory, action);
            if (!state.IsEmpty && state.LoadedAmmunition != profile.LoadedAmmunition) return Rejected(
                FirearmReloadPlanStatus.MixedAmmunition, "A partially loaded firearm cannot mix ammunition identities.",
                unit, exactItem, definition, state, profile, inventory, action);
            if (roundsRequested <= 0) throw new ArgumentOutOfRangeException("roundsRequested");
            int capacity = Math.Min(roundsRequested, definition.Capacity - state.LoadedRounds);
            int loadable = Math.Min(capacity, inventory.AvailableLoads(profile));
            if (loadable <= 0) return Rejected(FirearmReloadPlanStatus.MissingAmmunition,
                profile.SourceKind == ReloadAmmunitionSourceKind.PaperCartridge
                    ? "A Paper Cartridge is required; loose ammunition will not be substituted."
                    : "A Black Powder Charge and Lead Ball are required.",
                unit, exactItem, definition, state, profile, inventory, action);
            return new FirearmReloadPlan(FirearmReloadPlanStatus.Available,
                "Ready to reload " + loadable + " round(s) with " + profile.DisplayName + ".",
                unit, exactItem, definition, state, profile, inventory, roundsRequested, loadable, action);
        }
        private static FirearmReloadPlan Rejected(FirearmReloadPlanStatus status, string reason,
            object unit, object item, FirearmDefinition definition, FirearmState state,
            ReloadAmmunitionProfile profile, ReloadAmmunitionInventorySnapshot inventory,
            EffectiveReloadAction action = EffectiveReloadAction.Unknown)
        { return new FirearmReloadPlan(status, reason, unit, item, definition, state, profile, inventory, 0, 0, action); }
    }
}
