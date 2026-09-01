using System;
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
            try
            {
                if (moneyBefore < goldCost || !player.SpendMoney(goldCost) ||
                    player.Money != moneyBefore - goldCost)
                    throw new InvalidOperationException(
                        "Crafting gold removal failed.");
                for (int index = 0; index < outputs.Length; index++)
                    player.Inventory.Add(outputs[index], amounts[index]);
                for (int index = 0; index < outputs.Length; index++)
                    if (player.Inventory.Count(outputs[index]) !=
                        countsBefore[index] + amounts[index])
                        throw new InvalidOperationException(
                            "Crafted inventory verification failed.");
                if (caster.AddFact(marker) == null || !caster.HasFact(marker))
                    throw new InvalidOperationException(
                        "Crafting entitlement marker was not persisted.");
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Info("gunsmithing",
                        "ammunition-craft.committed", "outputs=" +
                        string.Join(",", outputs.Select(value => value.AssetGuid)) +
                        ";count=" + string.Join(",", amounts) + ";cost=" +
                        goldCost + ";moneyBefore=" + moneyBefore +
                        ";moneyAfter=" + player.Money);
            }
            catch
            {
                if (caster.HasFact(marker)) caster.RemoveFact(marker);
                for (int index = outputs.Length - 1; index >= 0; index--)
                {
                    int added = player.Inventory.Count(outputs[index]) - countsBefore[index];
                    if (added > 0) player.Inventory.Remove(outputs[index], added);
                }
                long missingMoney = moneyBefore - player.Money;
                if (missingMoney > 0) player.GainMoney(missingMoney);
                if (player.Money > moneyBefore)
                    throw new InvalidOperationException(
                        "Crafting rollback restored more gold than the transaction owned.");
                throw;
            }
        }
    }
}
