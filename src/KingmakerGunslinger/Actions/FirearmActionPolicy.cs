using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Actions
{
    /// <summary>
    /// Dependency-free eligibility policy shared by the three Kingmaker ability
    /// adapters. Runtime selection and inventory access happen outside this type.
    /// </summary>
    internal static class FirearmActionPolicy
    {
        internal static FirearmActionDecision Evaluate(
            FirearmActionKind action,
            FirearmDefinition definition,
            FirearmState state,
            bool hasRequiredResources)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            switch (action)
            {
                case FirearmActionKind.Reload:
                    return EvaluateReload(definition, state, hasRequiredResources);
                case FirearmActionKind.Overhaul:
                    return EvaluateOverhaul(state, hasRequiredResources);
                case FirearmActionKind.Repair:
                    return EvaluateRepair(state, hasRequiredResources);
                default:
                    throw new ArgumentOutOfRangeException("action");
            }
        }

        private static FirearmActionDecision EvaluateReload(
            FirearmDefinition definition,
            FirearmState state,
            bool hasResources)
        {
            if (definition.Reload.BaseAction != ReloadActionType.FullRound)
            {
                return Rejected(FirearmActionKind.Reload, "Only full-round reload is supported.");
            }

            if (definition.Capacity != 1 || definition.Reload.RoundsPerAction != 1)
            {
                return Rejected(
                    FirearmActionKind.Reload,
                    "Multi-round and partial reload are deferred until Sprint 33.");
            }

            if (state.Condition == FirearmCondition.Wrecked)
            {
                return Rejected(FirearmActionKind.Reload, "A Wrecked firearm cannot be reloaded.");
            }

            if (!state.IsEmpty)
            {
                return Rejected(FirearmActionKind.Reload, "The firearm is already loaded.");
            }

            return hasResources
                ? Available(FirearmActionKind.Reload, "The firearm is ready to reload.")
                : Rejected(FirearmActionKind.Reload, "Required ammunition is missing.");
        }

        private static FirearmActionDecision EvaluateOverhaul(
            FirearmState state,
            bool hasResources)
        {
            if (state.Condition != FirearmCondition.Wrecked)
            {
                return Rejected(
                    FirearmActionKind.Overhaul,
                    "Only an empty Wrecked firearm can be overhauled.");
            }

            return hasResources
                ? Available(FirearmActionKind.Overhaul, "The firearm is ready to overhaul.")
                : Rejected(FirearmActionKind.Overhaul, "A Firearm Repair Kit is required.");
        }

        private static FirearmActionDecision EvaluateRepair(
            FirearmState state,
            bool hasResources)
        {
            if (state.Condition != FirearmCondition.Broken)
            {
                return Rejected(
                    FirearmActionKind.Repair,
                    "Only an empty Broken firearm can be repaired.");
            }

            if (!state.IsEmpty)
            {
                return Rejected(
                    FirearmActionKind.Repair,
                    "The Broken firearm must be empty before repair.");
            }

            return hasResources
                ? Available(FirearmActionKind.Repair, "The firearm is ready to repair.")
                : Rejected(FirearmActionKind.Repair, "A Firearm Repair Kit is required.");
        }

        private static FirearmActionDecision Available(
            FirearmActionKind action,
            string reason)
        {
            return new FirearmActionDecision(action, true, reason);
        }

        private static FirearmActionDecision Rejected(
            FirearmActionKind action,
            string reason)
        {
            return new FirearmActionDecision(action, false, reason);
        }
    }
}
