using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Actions
{
    /// <summary>
    /// Dependency-free eligibility policy shared by the Kingmaker ability
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
            if (state.Condition == FirearmCondition.Wrecked)
            {
                return Rejected(FirearmActionKind.Reload, "A Wrecked firearm cannot be reloaded.");
            }

            if (state.LoadedRounds > definition.Capacity)
            {
                return Rejected(FirearmActionKind.Reload,
                    "The firearm state exceeds its definition capacity.");
            }

            if (state.LoadedRounds == definition.Capacity)
            {
                return Rejected(FirearmActionKind.Reload, "The firearm is at full capacity.");
            }

            return hasResources
                ? Available(FirearmActionKind.Reload, "The firearm has capacity available to reload.")
                : Rejected(FirearmActionKind.Reload, "Required ammunition is missing.");
        }

        private static FirearmActionDecision EvaluateRepair(
            FirearmState state,
            bool hasResources)
        {
            if (state.Condition != FirearmCondition.Broken &&
                state.Condition != FirearmCondition.Wrecked)
            {
                return Rejected(
                    FirearmActionKind.Repair,
                    "Only a Broken or Wrecked firearm can be repaired.");
            }

            return hasResources
                ? Available(FirearmActionKind.Repair,
                    "The firearm is ready to repair; any loaded ammunition is preserved.")
                : Rejected(FirearmActionKind.Repair,
                    "A reusable Gunsmith's Kit is required in the shared inventory.");
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
