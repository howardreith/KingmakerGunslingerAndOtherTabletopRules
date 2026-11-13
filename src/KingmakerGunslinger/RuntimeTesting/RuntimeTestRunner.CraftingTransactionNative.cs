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
        /// <see cref="FirearmCraftingFaultAdmission"/>, which acts only for the
        /// exact disposable descriptor admitted here and is disarmed in a
        /// finally.
        ///
        /// WHERE THE FAULTS ACTUALLY SIT, stated precisely because an earlier
        /// version of this comment overstated it. Neither point is a throw from
        /// inside a native call:
        ///
        /// - AfterDebitBeforeOutput fires after the native money debit has
        ///   committed and before any output is inserted. Nothing about the
        ///   outputs has been recorded yet, so this does exercise compensation
        ///   of a committed native mutation the service has not accounted for.
        /// - AfterFirstOutput fires after Inventory.Add has returned AND after
        ///   the finally has recorded committedAdds for that output. It
        ///   exercises rollback of an output that is already committed and
        ///   already accounted. It does NOT simulate a throw from inside
        ///   ItemsCollection.Add, and no claim is made that it does.
        ///
        /// The partial-insertion case the accounting repair exists for - a
        /// throw part-way through the non-stackable Add loop - is established
        /// by DISASSEMBLY of the installed assembly, not by this scenario.
        /// What this scenario establishes about that repair is the budget
        /// invariant: see the external-surplus observation below, which drives
        /// a real inventory change into the service's own measurement window.
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
                    Tuple.Create("basic", basicOutputs, (long)basicLogic.GoldCost,
                        CraftBasicAmmunitionAbilityLogic.BatchSize),
                    Tuple.Create("paper", paperOutputs, (long)paperLogic.GoldCost,
                        CraftPaperCartridgesAbilityLogic.BatchSize) })
                {
                    string id = recipe.Item1;
                    BlueprintItem[] outputs = recipe.Item2;
                    long cost = recipe.Item3;
                    // The recipe's OWN batch constant, read from the production
                    // logic rather than restated here. The first version of this
                    // observation expected one item per output and the run
                    // reported outputsExact=False against a correct product that
                    // had delivered the full batch of 20.
                    int batch = recipe.Item4;
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
                            after.Counts[index] == before.Counts[index] + batch)
                            .All(value => value);
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
                                "increments every output by exactly the recipe's own batch " +
                                "constant, grants the entitlement marker once, preserves the " +
                                "tool, and then REFUSES an actual repeat invocation with no " +
                                "further debit, award or marker change - refusal is observed by " +
                                "invoking it again, not by reading an availability flag",
                            "debitExact=" + debitExact + ";cost=" + cost +
                                ";batch=" + batch + ";outputsExact=" + outputsExact +
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

                // ---- 4 and 5: failures at native boundaries on the forward path ----
                foreach (var fault in new[] {
                    Tuple.Create("after-debit", FirearmCraftingFaultAdmission.AfterDebitBeforeOutput),
                    Tuple.Create("after-first-output", FirearmCraftingFaultAdmission.AfterFirstOutput) })
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
                        bool compensated = moneyRestored && noOutput && noMarker;

                        assertions.Add(Assertion("crafting-service-fault-" + id,
                            "a failure at this native boundary restores the owned debit " +
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
                            "an admitted fault after a committed native mutation; for " +
                                "after-first-output the service has already recorded that " +
                                "output's committedAdds, so this is rollback of an accounted " +
                                "commit and NOT a throw from inside ItemsCollection.Add. " +
                                "Product state captured before fixture cleanup"));
                    }
                    finally
                    {
                        if (admission != null) admission.Dispose();
                        unit.Dispose();
                    }
                }

                // ---- 6: a fault INSIDE compensation, reached the only way it can be ----
                //
                // The compensation trip lives in the service's catch block, so a
                // transaction must already have failed before it can be reached.
                // An earlier run admitted it on its own: nothing failed, the
                // admission never fired, the transaction committed normally, and
                // the observation was vacuous. It is now admitted as a SECOND
                // fault alongside a forward-path trigger, and the admission
                // itself refuses the old single-point form.
                //
                // The residual here is asserted EXACTLY rather than merely
                // recorded. The trigger fires after the first output commits, so
                // compensation is entered with the debit taken and one batch of
                // the first output in the inventory, and the compensation fault
                // throws before the plan is built: nothing is removed, nothing is
                // refunded. That residual is precisely predictable, and asserting
                // it is what makes the case evidence rather than an anecdote.
                {
                    var unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                            .DefaultPlayerCharacter).Unit;
                    FirearmCraftingFaultAdmission admission = null;
                    try
                    {
                        CraftingProbe before = CaptureCrafting(
                            unit.Descriptor, basicOutputs, marker);
                        bool singlePointRefused = false;
                        try
                        {
                            FirearmCraftingFaultAdmission.Admit(unit.Descriptor,
                                FirearmCraftingFaultAdmission.DuringCompensation);
                        }
                        catch (ArgumentOutOfRangeException) { singlePointRefused = true; }
                        admission = FirearmCraftingFaultAdmission.Admit(unit.Descriptor,
                            FirearmCraftingFaultAdmission.AfterFirstOutput, true);

                        string outerMessage = null;
                        string[] retainedCauses = new string[0];
                        try { basicLogic.Complete(unit.Descriptor); }
                        catch (Exception error)
                        {
                            outerMessage = error.Message ?? string.Empty;
                            var aggregate = error.InnerException as AggregateException;
                            if (aggregate != null)
                                retainedCauses = aggregate.InnerExceptions
                                    .Select(value => value.Message ?? string.Empty).ToArray();
                        }
                        // Captured BEFORE any fixture restoration.
                        CraftingProbe after = CaptureCrafting(
                            unit.Descriptor, basicOutputs, marker);

                        bool bothTripped =
                            admission.TrippedAt(FirearmCraftingFaultAdmission.AfterFirstOutput) &&
                            admission.TrippedAt(FirearmCraftingFaultAdmission.DuringCompensation);
                        bool bothCausesRetained = retainedCauses.Length == 2 &&
                            retainedCauses.All(value => value.IndexOf(
                                FirearmCraftingFaultAdmission.AfterFirstOutput,
                                StringComparison.Ordinal) >= 0 || value.IndexOf(
                                FirearmCraftingFaultAdmission.DuringCompensation,
                                StringComparison.Ordinal) >= 0) &&
                            retainedCauses.Any(value => value.IndexOf(
                                FirearmCraftingFaultAdmission.AfterFirstOutput,
                                StringComparison.Ordinal) >= 0) &&
                            retainedCauses.Any(value => value.IndexOf(
                                FirearmCraftingFaultAdmission.DuringCompensation,
                                StringComparison.Ordinal) >= 0);
                        bool outerNamesBothFailures = outerMessage != null &&
                            outerMessage.IndexOf("compensation also failed",
                                StringComparison.OrdinalIgnoreCase) >= 0;
                        // Exact residual: the debit stands, the first output's
                        // committed batch stands, the second output was never
                        // reached, and the entitlement was never granted.
                        bool residualExact =
                            after.Money == before.Money - basicLogic.GoldCost &&
                            after.Counts[0] == before.Counts[0] +
                                CraftBasicAmmunitionAbilityLogic.BatchSize &&
                            after.Counts[1] == before.Counts[1] && !after.Marker;

                        assertions.Add(Assertion("crafting-service-fault-during-compensation",
                            "when compensation itself fails, the service retains BOTH causes - " +
                                "the original failure and the compensation failure - in an " +
                                "AggregateException under an outer error that says so, grants no " +
                                "entitlement, and leaves exactly the state that had committed " +
                                "when compensation was entered: the debit and the first output's " +
                                "batch, nothing more. Asserted exactly, and captured before the " +
                                "fixture restores anything. The compensation point is also " +
                                "refused as a standalone trigger, because alone it is unreachable",
                            "singlePointRefused=" + singlePointRefused +
                                ";tripped=" + admission.TrippedPoints +
                                ";bothTripped=" + bothTripped +
                                ";outerNamesBothFailures=" + outerNamesBothFailures +
                                ";retainedCauses=" + retainedCauses.Length +
                                ";bothCausesRetained=" + bothCausesRetained +
                                ";moneyBefore=" + before.Money + ";moneyAfter=" + after.Money +
                                ";cost=" + basicLogic.GoldCost +
                                ";countsBefore=" + string.Join(",", before.Counts) +
                                ";countsAfter=" + string.Join(",", after.Counts) +
                                ";markerAfter=" + after.Marker +
                                ";residualExact=" + residualExact,
                            singlePointRefused && bothTripped && outerNamesBothFailures &&
                                bothCausesRetained && residualExact,
                            "two admitted faults - one on the forward path, after the first " +
                                "output was committed and recorded, and one inside the " +
                                "service's compensation - with product state captured before " +
                                "fixture cleanup"));
                    }
                    finally
                    {
                        if (admission != null) admission.Dispose();
                        unit.Dispose();
                    }
                }

                // ---- 7: a surplus this transaction did not create is not removed ----
                //
                // The service measures each output's committed insertion as
                // Count(output) - countsBefore[index], and countsBefore is
                // taken before the output loop. Anything else that raises the
                // same item's count inside that window lands in the same
                // difference. Uncapped, the transaction claims the surplus as
                // its own, the compensation plan removes it, and the policy's
                // external-surplus invariant never fires because the service
                // told it the whole difference was its own work.
                //
                // That window is entirely inside Complete, so this is driven by
                // an admitted INTERFERENCE - the one case the seam exists for
                // that is not a fault. Five extra lead balls are added after
                // the debit and before the outputs; the recipe then inserts its
                // own batch of twenty; the service's own verification fails
                // because twenty-five is not twenty; and compensation runs.
                //
                // The correct outcome is exact: the first output is removed in
                // full, the second is reduced by this transaction's twenty and
                // NOT by the five it never created, the gold comes back, no
                // entitlement is granted, and the residual five is REPORTED.
                {
                    var unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                            .DefaultPlayerCharacter).Unit;
                    FirearmCraftingFaultAdmission admission = null;
                    const int surplus = 5;
                    try
                    {
                        CraftingProbe before = CaptureCrafting(
                            unit.Descriptor, basicOutputs, marker);
                        admission = FirearmCraftingFaultAdmission.AdmitInterference(
                            unit.Descriptor,
                            FirearmCraftingFaultAdmission.AfterDebitBeforeOutput,
                            () => player.Inventory.Add(basicOutputs[1], surplus));

                        string outerMessage = null, originalCause = null;
                        try { basicLogic.Complete(unit.Descriptor); }
                        catch (Exception error)
                        {
                            outerMessage = error.Message ?? string.Empty;
                            originalCause = error.InnerException == null ? string.Empty :
                                error.InnerException.Message ?? string.Empty;
                        }
                        // Captured BEFORE any fixture restoration.
                        CraftingProbe after = CaptureCrafting(
                            unit.Descriptor, basicOutputs, marker);

                        bool interfered = admission.InterferenceRun;
                        bool firstOutputRemoved =
                            after.Counts[0] == before.Counts[0];
                        bool surplusPreserved =
                            after.Counts[1] == before.Counts[1] + surplus;
                        bool moneyRestored = after.Money == before.Money;
                        bool residualReported = outerMessage != null &&
                            outerMessage.IndexOf("surplus was not made here",
                                StringComparison.OrdinalIgnoreCase) >= 0;
                        bool originalRetained = originalCause != null &&
                            originalCause.IndexOf("inventory verification failed",
                                StringComparison.OrdinalIgnoreCase) >= 0;

                        assertions.Add(Assertion("crafting-service-external-surplus-preserved",
                            "when something else raises an output's count inside the service's " +
                                "own measurement window, compensation removes only this " +
                                "transaction's own batch, LEAVES the surplus it did not create, " +
                                "restores the gold, grants no entitlement, and REPORTS the " +
                                "residual while retaining the original cause. Asserted on real " +
                                "native inventory, before any fixture cleanup",
                            "interferenceRan=" + interfered + ";surplus=" + surplus +
                                ";countsBefore=" + string.Join(",", before.Counts) +
                                ";countsAfter=" + string.Join(",", after.Counts) +
                                ";firstOutputRemoved=" + firstOutputRemoved +
                                ";surplusPreserved=" + surplusPreserved +
                                ";moneyBefore=" + before.Money +
                                ";moneyAfter=" + after.Money +
                                ";moneyRestored=" + moneyRestored +
                                ";markerAfter=" + after.Marker +
                                ";residualReported=" + residualReported +
                                ";originalCauseRetained=" + originalRetained,
                            interfered && firstOutputRemoved && surplusPreserved &&
                                moneyRestored && !after.Marker && residualReported &&
                                originalRetained,
                            "an admitted non-throwing interference inside the service's own " +
                                "measurement window - the only place this case can be " +
                                "constructed - against the real player inventory"));
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
