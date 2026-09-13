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
            return Evaluate(
                action, definition, state, hasRequiredResources, false);
        }

        internal static FirearmActionDecision Evaluate(
            FirearmActionKind action,
            FirearmDefinition definition,
            FirearmState state,
            bool hasRequiredResources,
            bool inCombat)
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
                    return EvaluateRepair(state, hasRequiredResources, inCombat);
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
            bool hasResources,
            bool inCombat)
        {
            if (inCombat)
            {
                return Rejected(
                    FirearmActionKind.Repair,
                    "Cannot repair firearms during combat.");
            }

            if (state.Condition == FirearmCondition.Wrecked)
            {
                return Rejected(
                    FirearmActionKind.Repair,
                    "This firearm is Wrecked. A full rest is required.");
            }

            if (state.Condition != FirearmCondition.Broken)
            {
                return Rejected(
                    FirearmActionKind.Repair,
                    "This firearm is not broken.");
            }

            return hasResources
                ? Available(FirearmActionKind.Repair,
                    "The Broken firearm is ready to repair to Normal with the reusable Gunsmith's Kit. Nothing is consumed and every surviving loaded round is preserved; the item will not be replaced.")
                : Rejected(FirearmActionKind.Repair,
                    "Requires a Gunsmith's Kit.");
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
