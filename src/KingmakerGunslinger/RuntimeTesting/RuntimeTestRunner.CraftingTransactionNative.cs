using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// GS-AUD-0036 native transaction observations.
        ///
        /// These go through the REAL recipe logic and therefore the real
        /// <see cref="FirearmCraftingTransactionService"/>, against native
        /// player money, native inventory and a real entitlement fact on a
        /// disposable ChargenUnit. The compensation arithmetic has its own
        /// domain tests; what is observed here is the SERVICE, which those
        /// cannot reach.
        ///
        /// Product state is captured immediately after each failure and its
        /// compensation, BEFORE the fixture restores anything. A restored test
        /// environment cannot conceal incorrect product compensation if the
        /// observation is taken first.
        ///
        /// The fault points are admitted through
        /// <see cref="FirearmCraftingFaultAdmission"/>, which throws only for
        /// the exact disposable descriptor admitted here and is disarmed in a
        /// finally. The faults sit at NATIVE boundaries - after the debit and
        /// after the first committed output - not after the service's own
        /// bookkeeping, which would have proved nothing about the boundary in
        /// question.
        /// </summary>
        private sealed class CraftingProbe
        {
            internal long Money;
            internal int[] Counts;
            internal bool Marker;
        }

        private static CraftingProbe CaptureCrafting(UnitDescriptor caster,
            BlueprintItem[] outputs, Kingmaker.Blueprints.Classes.BlueprintFeature marker)
        {
            var player = Kingmaker.Game.Instance.Player;
            return new CraftingProbe {
                Money = player.Money,
                Counts = outputs.Select(value => player.Inventory.Count(value)).ToArray(),
                Marker = caster.HasFact(marker) };
        }

        private void ObserveCraftingTransactionService(
            List<RuntimeTestAssertion> assertions)
        {
            var player = Kingmaker.Game.Instance.Player;
            BasicAmmunitionBlueprintSet ammo = BlueprintBootstrap.BasicAmmunition;
            BlueprintItem tool = BlueprintBootstrap.GunsmithingSupplies.GunsmithKit;
            GunsmithingCraftingBlueprintSet crafting = BlueprintBootstrap.GunsmithingCrafting;
            var basicLogic = crafting.BasicAbility.ComponentsArray
                .OfType<CraftBasicAmmunitionAbilityLogic>().Single();
            var paperLogic = crafting.PaperAbility.ComponentsArray
                .OfType<CraftPaperCartridgesAbilityLogic>().Single();

            BlueprintItem[] basicOutputs = { ammo.BlackPowder, ammo.LeadBall };
            BlueprintItem[] paperOutputs = { ammo.PaperCartridge };
            Kingmaker.Blueprints.Classes.BlueprintFeature marker = crafting.UsedMarker;

            int toolsBefore = player.Inventory.Count(tool);
            long moneyAtEntry = player.Money;
            // Entry counts for every output, so the finally removes only what
            // this observation created and never touches the player's own stock.
            _craftingEntryCounts = new Dictionary<BlueprintItem, int>();
            foreach (BlueprintItem output in basicOutputs.Concat(paperOutputs).Distinct())
                _craftingEntryCounts[output] = player.Inventory.Count(output);
            // Enough for several attempts, seeded once and removed in finally.
            long seed = basicLogic.GoldCost * 6 + paperLogic.GoldCost * 4;
            bool seeded = false, toolAdded = false;

            try
            {
                if (toolsBefore == 0) { player.Inventory.Add(tool, 1); toolAdded = true; }
                player.GainMoney(seed); seeded = true;

                // ---- 1 and 2: the two recipes succeed exactly ----
                foreach (var recipe in new[] {
                    Tuple.Create("basic", (object)basicLogic, basicOutputs,
                        (long)basicLogic.GoldCost),
                    Tuple.Create("paper", (object)paperLogic, paperOutputs,
                        (long)paperLogic.GoldCost) })
                {
                    string id = recipe.Item1;
                    BlueprintItem[] outputs = recipe.Item3;
                    long cost = recipe.Item4;
                    var unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                            .DefaultPlayerCharacter).Unit;
                    try
                    {
                        CraftingProbe before = CaptureCrafting(unit.Descriptor, outputs, marker);
                        int toolCountBefore = player.Inventory.Count(tool);
                        if (id == "basic") basicLogic.Complete(unit.Descriptor);
                        else paperLogic.Complete(unit.Descriptor);
                        CraftingProbe after = CaptureCrafting(unit.Descriptor, outputs, marker);

                        bool debitExact = after.Money == before.Money - cost;
                        bool outputsExact = outputs.Select((value, index) =>
                            after.Counts[index] == before.Counts[index] + 1).All(value => value);
                        bool markerOnce = !before.Marker && after.Marker;
                        bool toolPreserved = player.Inventory.Count(tool) == toolCountBefore;

                        // ---- 3: an actual repeat invocation is refused ----
                        string repeatRefusal = null;
                        try { if (id == "basic") basicLogic.Complete(unit.Descriptor);
                              else paperLogic.Complete(unit.Descriptor); }
                        catch (Exception repeat) { repeatRefusal = repeat.GetType().Name; }
                        CraftingProbe afterRepeat = CaptureCrafting(unit.Descriptor, outputs, marker);
                        bool repeatInert = afterRepeat.Money == after.Money &&
                            afterRepeat.Counts.SequenceEqual(after.Counts) && afterRepeat.Marker;

                        assertions.Add(Assertion("crafting-service-" + id + "-success",
                            "the real recipe debits exactly its cost through native money, " +
                                "increments every output by one, grants the entitlement marker " +
                                "once, preserves the tool, and then REFUSES an actual repeat " +
                                "invocation with no further debit, award or marker change - " +
                                "refusal is observed by invoking it again, not by reading an " +
                                "availability flag",
                            "debitExact=" + debitExact + ";cost=" + cost +
                                ";outputsExact=" + outputsExact +
                                ";markerGrantedOnce=" + markerOnce +
                                ";toolPreserved=" + toolPreserved +
                                ";repeatRefusal=" + (repeatRefusal ?? "<none>") +
                                ";repeatInert=" + repeatInert,
                            debitExact && outputsExact && markerOnce && toolPreserved &&
                                repeatRefusal != null && repeatInert,
                            "CraftPaperCartridgesAbilityLogic / CraftBasicAmmunitionAbilityLogic " +
                                "through FirearmCraftingTransactionService on native money, " +
                                "inventory and facts"));
                    }
                    finally { unit.Dispose(); }
                }

                // ---- 4, 5 and 6: failures at native boundaries ----
                foreach (var fault in new[] {
                    Tuple.Create("after-debit", FirearmCraftingFaultAdmission.AfterDebitBeforeOutput),
                    Tuple.Create("after-first-output", FirearmCraftingFaultAdmission.AfterFirstOutput),
                    Tuple.Create("during-compensation", FirearmCraftingFaultAdmission.DuringCompensation) })
                {
                    string id = fault.Item1;
                    var unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                            .DefaultPlayerCharacter).Unit;
                    FirearmCraftingFaultAdmission admission = null;
                    try
                    {
                        CraftingProbe before = CaptureCrafting(
                            unit.Descriptor, basicOutputs, marker);
                        admission = FirearmCraftingFaultAdmission.Admit(
                            unit.Descriptor, fault.Item2);
                        string failure = null, cleanup = null;
                        try { basicLogic.Complete(unit.Descriptor); }
                        catch (Exception error)
                        {
                            failure = error.Message ?? string.Empty;
                            // The service must not discard either cause.
                            cleanup = error.InnerException == null ? string.Empty :
                                error.InnerException.Message ?? string.Empty;
                            if (string.IsNullOrEmpty(cleanup) && error is AggregateException)
                                cleanup = string.Join("|", ((AggregateException)error)
                                    .InnerExceptions.Select(value => value.Message));
                        }
                        // Captured BEFORE any fixture restoration.
                        CraftingProbe after = CaptureCrafting(
                            unit.Descriptor, basicOutputs, marker);

                        bool threw = failure != null;
                        bool admissionFired = admission.Tripped;
                        bool moneyRestored = after.Money == before.Money;
                        bool noOutput = after.Counts.SequenceEqual(before.Counts);
                        bool noMarker = !after.Marker;
                        bool causeDiscoverable = threw &&
                            failure.IndexOf("crafting", StringComparison.OrdinalIgnoreCase) >= 0;

                        // The compensation-failure case is the one where product
                        // state may legitimately remain dirty. What must hold is
                        // that both causes are discoverable and the residual is
                        // recorded, not that the state is clean.
                        bool compensated = id == "during-compensation"
                            ? causeDiscoverable
                            : moneyRestored && noOutput && noMarker;

                        assertions.Add(Assertion("crafting-service-fault-" + id,
                            id == "during-compensation"
                                ? "a failure DURING compensation still surfaces a discoverable " +
                                  "cause, and the residual product state is recorded here before " +
                                  "the fixture restores anything"
                                : "a failure at this native boundary restores the owned debit " +
                                  "exactly, leaves no output and no entitlement marker, and the " +
                                  "state is observed before the fixture restores anything",
                            "admissionFired=" + admissionFired + ";threw=" + threw +
                                ";causeDiscoverable=" + causeDiscoverable +
                                ";moneyBefore=" + before.Money + ";moneyAfter=" + after.Money +
                                ";moneyRestored=" + moneyRestored +
                                ";countsBefore=" + string.Join(",", before.Counts) +
                                ";countsAfter=" + string.Join(",", after.Counts) +
                                ";noOutput=" + noOutput + ";markerAfter=" + after.Marker +
                                ";cleanupCause=" + (string.IsNullOrEmpty(cleanup) ?
                                    "<none>" : "present"),
                            admissionFired && threw && compensated,
                            "an admitted fault at a NATIVE mutation boundary, not after the " +
                                "service's own bookkeeping; product state captured before " +
                                "fixture cleanup"));
                    }
                    finally
                    {
                        if (admission != null) admission.Dispose();
                        unit.Dispose();
                    }
                }

                // The seam must be inert for anything but its admitted caster.
                var bystander = new Kingmaker.UI.LevelUp.ChargenUnit(
                    Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                        .DefaultPlayerCharacter).Unit;
                FirearmCraftingFaultAdmission scoped = null;
                try
                {
                    var admitted = new Kingmaker.UI.LevelUp.ChargenUnit(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                            .DefaultPlayerCharacter).Unit;
                    try
                    {
                        scoped = FirearmCraftingFaultAdmission.Admit(
                            admitted.Descriptor,
                            FirearmCraftingFaultAdmission.AfterDebitBeforeOutput);
                        CraftingProbe before = CaptureCrafting(
                            bystander.Descriptor, basicOutputs, marker);
                        string unexpected = null;
                        try { basicLogic.Complete(bystander.Descriptor); }
                        catch (Exception error) { unexpected = error.Message; }
                        CraftingProbe after = CaptureCrafting(
                            bystander.Descriptor, basicOutputs, marker);
                        bool stacked = false;
                        try { FirearmCraftingFaultAdmission.Admit(bystander.Descriptor,
                                FirearmCraftingFaultAdmission.AfterFirstOutput); }
                        catch (InvalidOperationException) { stacked = true; }
                        assertions.Add(Assertion("crafting-service-fault-seam-scope",
                            "a live admission does nothing to any other caster - that " +
                                "transaction completes normally - and a second admission is " +
                                "refused rather than stacked, so a leaked one cannot extend " +
                                "over a later transaction",
                            "bystanderThrew=" + (unexpected ?? "<none>") +
                                ";bystanderDebited=" + (after.Money == before.Money - basicLogic.GoldCost) +
                                ";bystanderMarker=" + after.Marker +
                                ";secondAdmissionRefused=" + stacked,
                            unexpected == null && after.Marker && stacked &&
                                after.Money == before.Money - basicLogic.GoldCost,
                            "the real service invoked for an unadmitted caster while an " +
                                "admission is live"));
                    }
                    finally { admitted.Dispose(); }
                }
                finally
                {
                    if (scoped != null) scoped.Dispose();
                    bystander.Dispose();
                }
            }
            finally
            {
                // Restore the fixture's own seeding. Every product observation
                // above was already taken.
                long excess = player.Money - moneyAtEntry;
                if (excess > 0) player.SpendMoney(excess);
                else if (excess < 0) player.GainMoney(-excess);
                if (toolAdded && player.Inventory.Count(tool) > toolsBefore)
                    player.Inventory.Remove(tool,
                        player.Inventory.Count(tool) - toolsBefore);
                foreach (BlueprintItem output in basicOutputs.Concat(paperOutputs).Distinct())
                {
                    // Outputs this observation created are removed; anything the
                    // player already had is untouched because the counts are
                    // compared against entry.
                    int present = player.Inventory.Count(output);
                    int entry = _craftingEntryCounts != null &&
                        _craftingEntryCounts.ContainsKey(output) ?
                        _craftingEntryCounts[output] : present;
                    if (present > entry) player.Inventory.Remove(output, present - entry);
                }
                if (!seeded) { /* nothing was seeded */ }
            }
        }

        private Dictionary<BlueprintItem, int> _craftingEntryCounts;
    }
}
