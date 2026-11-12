using System;
using System.Globalization;
using System.Linq;
using KingmakerGunslinger.Bootstrap;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Gunsmithing
{
    internal static class FirearmCraftingTransactionService
    {
        internal static void Complete(UnitDescriptor caster,
            BlueprintItem tool, BlueprintUnitFact marker, int goldCost,
            BlueprintItem[] outputs, int[] amounts)
        {
            if (caster == null || caster.Unit == null || caster.Unit.IsInCombat ||
                !caster.State.IsConscious || !caster.State.CanAct ||
                caster.HasFact(marker))
                throw new InvalidOperationException(
                    "Crafting requires an able conscious caster and an unused rest entitlement.");
            if (tool == null || marker == null || outputs == null || amounts == null ||
                outputs.Length == 0 || outputs.Length != amounts.Length || goldCost < 1)
                throw new ArgumentException("Crafting transaction configuration is invalid.");
            var player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null || player.Inventory == null)
                throw new InvalidOperationException("The player inventory is unavailable.");
            if (player.Inventory.Count(tool) < 1)
                throw new InvalidOperationException("Crafting requires a Gunsmith's Kit.");
            long moneyBefore = player.Money;
            int[] countsBefore = new int[outputs.Length];
            for (int index = 0; index < outputs.Length; index++)
            {
                if (outputs[index] == null || amounts[index] < 1)
                    throw new ArgumentException("Crafting output configuration is invalid.");
                countsBefore[index] = player.Inventory.Count(outputs[index]);
            }
            // GS-AUD-0036: what this transaction actually committed, so
            // compensation can be exact instead of inferred from a difference.
            var committedAdds = new int[outputs.Length];
            bool markerAddedHere = false;
            try
            {
                if (moneyBefore < goldCost || !player.SpendMoney(goldCost) ||
                    player.Money != moneyBefore - goldCost)
                    throw new InvalidOperationException(
                        "Crafting gold removal failed.");
                for (int index = 0; index < outputs.Length; index++)
                {
                    player.Inventory.Add(outputs[index], amounts[index]);
                    committedAdds[index] = amounts[index];
                }
                for (int index = 0; index < outputs.Length; index++)
                    if (player.Inventory.Count(outputs[index]) !=
                        countsBefore[index] + amounts[index])
                        throw new InvalidOperationException(
                            "Crafted inventory verification failed.");
                // Record whether the marker was added HERE, so compensation
                // removes only what this transaction granted. This is
                // ownership hardening rather than a reachable defect: Complete
                // already rejects caster.HasFact(marker) at entry, before the
                // try block, so an ordinary attempt that begins with the marker
                // held cannot reach the rollback. Tracking it removes the
                // dependence on that entry guard and on no other system
                // granting the marker mid-transaction.
                bool heldMarkerBefore = caster.HasFact(marker);
                if (caster.AddFact(marker) == null || !caster.HasFact(marker))
                    throw new InvalidOperationException(
                        "Crafting entitlement marker was not persisted.");
                markerAddedHere = !heldMarkerBefore;
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Info("gunsmithing",
                        "ammunition-craft.committed", "outputs=" +
                        string.Join(",", outputs.Select(value => value.AssetGuid)) +
                        ";count=" + string.Join(",", amounts) + ";cost=" +
                        goldCost + ";moneyBefore=" + moneyBefore +
                        ";moneyAfter=" + player.Money);
            }
            catch (Exception failure)
            {
                // GS-AUD-0036: compensate exactly what committed, and never
                // discard the original cause.
                Exception cleanupFailure = null;
                string invariant = null;
                try
                {
                    var countsNow = new int[outputs.Length];
                    for (int index = 0; index < outputs.Length; index++)
                        countsNow[index] = player.Inventory.Count(outputs[index]);
                    FirearmCraftingCompensationPlan plan =
                        FirearmCraftingCompensationPolicy.Plan(moneyBefore,
                            player.Money, goldCost, countsBefore, countsNow,
                            committedAdds, markerAddedHere);
                    invariant = plan.Invariant;
                    if (plan.RemoveMarker && caster.HasFact(marker))
                        caster.RemoveFact(marker);
                    for (int index = outputs.Length - 1; index >= 0; index--)
                        if (plan.ItemsToRemove[index] > 0)
                            player.Inventory.Remove(outputs[index],
                                plan.ItemsToRemove[index]);
                    // Clamped to what this transaction could have debited, so an
                    // over-refund is impossible by construction rather than
                    // detected after the fact and left in place.
                    if (plan.GoldToRestore > 0) player.GainMoney(plan.GoldToRestore);
                }
                catch (Exception cleanup) { cleanupFailure = cleanup; }

                ModContext failureContext;
                if (ModContext.TryGet(out failureContext))
                    failureContext.Logger.Failure("gunsmithing",
                        "ammunition-craft.rolled-back",
                        "moneyBefore=" + moneyBefore + ";moneyAfter=" + player.Money +
                        ";committed=" + string.Join(",",
                            committedAdds.Select(value =>
                                value.ToString(CultureInfo.InvariantCulture)).ToArray()) +
                        ";markerAddedHere=" + markerAddedHere +
                        ";invariant=" + (invariant ?? "<none>"), failure);

                // Both causes survive. The original failure is never replaced.
                if (cleanupFailure != null)
                    throw new InvalidOperationException(
                        "Crafting failed and its compensation also failed; " +
                        "both causes are retained.",
                        new AggregateException(failure, cleanupFailure));
                if (invariant != null)
                    throw new InvalidOperationException(
                        "Crafting failed and the pre-transaction state could not " +
                        "be restored exactly: " + invariant +
                        ". The original cause is retained.", failure);
                throw;
            }
        }
    }
}
