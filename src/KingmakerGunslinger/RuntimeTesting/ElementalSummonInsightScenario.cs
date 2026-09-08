using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Prepared native commands and native per-creature summon rules.
    /// Only the existing request-local placement boundary is isolated.</summary>
    internal static class ElementalSummonInsightScenario
    {
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            var transient = new List<UnityEngine.Object>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            int summoned = 0;
            try
            {
                foreach (ElementalAlternateTraitId id in new[] {
                    ElementalAlternateTraitId.FireInsight,
                    ElementalAlternateTraitId.EarthInsight,
                    ElementalAlternateTraitId.AirInsight })
                {
                    ElementalRaceBlueprints race = BlueprintBootstrap.ElementalRaces
                        .OrderedBlueprints().Single(value => value.AlternateTraits
                            .Traits().Any(trait => trait.Definition.Id == id));
                    ElementalUndineFeatScenario.PortalHarness harness =
                        ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    try
                    {
                        RunTrait(harness, race, id, assertions, rows, transient);
                    }
                    finally
                    {
                        summoned += harness.Summons.Count;
                        harness.Dispose();
                        Check(assertions, rows, id + "-native-fixture-hydration",
                            harness.NativeExceptions == 0 && harness.NativeErrors == 0 &&
                                harness.NativeInitializationObserved && harness.NativeTeardownObserved &&
                                harness.NativeObservationReleased,
                            "native exceptions=" + harness.NativeExceptions +
                            ";native errors=" + harness.NativeErrors +
                            ";initializationObserved=" + harness.NativeInitializationObserved +
                            ";teardownObserved=" + harness.NativeTeardownObserved +
                            ";observerReleased=" + harness.NativeObservationReleased +
                            ";scope=initialization, native level-up, commands and teardown");
                        Check(assertions, rows, id + "-context-cleanup",
                            harness.AreaContextRestored && harness.PlayerContextRestored,
                            "area=" + harness.AreaContextRestored + ";player=" +
                            harness.PlayerContextRestored);
                    }
                }
            }
            finally
            {
                UnityEngine.Random.state = randomBefore;
                bool clean = Game.Instance.State.Units.All.Count == before.Length &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value));
                // Ability contexts on summoned units can still reference
                // these unregistered fixtures. Dispose every unit first.
                if (clean)
                    foreach (UnityEngine.Object value in transient.AsEnumerable().Reverse())
                        UnityEngine.Object.DestroyImmediate(value);
                Check(assertions, rows, "fixture-cleanup", clean,
                    "summoned=" + summoned + ";before=" + before.Length +
                    ";after=" + Game.Instance.State.Units.All.Count);
                string path = Path.Combine(request.EvidenceDirectory,
                    "elemental-summon-insight.json");
                File.WriteAllText(path, new JObject {
                    { "schemaVersion", 1 }, { "saveStateTouched", false },
                    { "summonedUnits", summoned }, { "cleanupExact", clean },
                    { "diagnostics", new JArray(diagnostics) }, { "observations", rows }
                }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void RunTrait(ElementalUndineFeatScenario.PortalHarness harness,
            ElementalRaceBlueprints race, ElementalAlternateTraitId id,
            ICollection<RuntimeTestAssertion> assertions, JArray rows,
            ICollection<UnityEngine.Object> transient)
        {
            UnitEntityData caster = harness.Caster;
            caster.Stats.Intelligence.BaseValue = 30;
            caster.Stats.Wisdom.BaseValue = 30;
            BlueprintCharacterClass wizard = Exact<BlueprintCharacterClass>(
                "ba34257984f4c41408ce1dc2004e342e");
            BlueprintCharacterClass druid = Exact<BlueprintCharacterClass>(
                "610d836f3a3a9ed42a4349b62f002e96");
            ElementalSpellAffinityScenario.Advance(caster.Descriptor, wizard, 1);
            int[] statsBefore = race.Definition.Stats.Select(value =>
                caster.Stats.GetStat(value.Stat).ModifiedValue).ToArray();
            int initialUse = caster.Descriptor.Resources.GetResourceAmount(race.SlaResource);
            caster.Descriptor.Resources.Spend(race.SlaResource, 1);
            ElementalSpellAffinityScenario.Advance(caster.Descriptor, wizard, 4);
            ElementalSpellAffinityScenario.Advance(caster.Descriptor, druid, 5);
            int[] statsAfter = race.Definition.Stats.Select(value =>
                caster.Stats.GetStat(value.Stat).ModifiedValue).ToArray();
            Check(assertions, rows, id + "-native-level-up-preserves-race-and-spent-use",
                initialUse == 1 && statsBefore.SequenceEqual(statsAfter) &&
                    caster.Descriptor.Resources.GetResourceAmount(race.SlaResource) == 0 &&
                    caster.Descriptor.HasFact(race.SlaFeature) &&
                    caster.Descriptor.Abilities.GetAbility(race.SlaAbility) != null &&
                    ReferenceEquals(caster.Descriptor.Progression.Race, race.Race),
                "statsBefore=" + string.Join(",", statsBefore) +
                    ";statsAfter=" + string.Join(",", statsAfter) +
                    ";useBeforeSpend=" + initialUse + ";remaining=" +
                    caster.Descriptor.Resources.GetResourceAmount(race.SlaResource));
            Spellbook[] books = { Book(caster, wizard), Book(caster, druid) };
            Check(assertions, rows, id + "-native-spellbooks",
                books.All(value => value.CasterLevel == 5) &&
                    caster.Descriptor.Progression.CharacterLevel == 10,
                "Wizard=" + wizard.AssetGuid + ";Druid=" + druid.AssetGuid +
                ";total=" + caster.Descriptor.Progression.CharacterLevel);
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(id);
            ElementalSummonInsight component = trait.Provider.ComponentsArray
                .OfType<ElementalSummonInsight>().Single();
            string element = id == ElementalAlternateTraitId.FireInsight ? "Fire" :
                id == ElementalAlternateTraitId.EarthInsight ? "Earth" : "Air";
            BlueprintUnit nativeUnit = Exact<BlueprintUnit>(element == "Fire"
                ? "46cede83b1f34ad4fa46b8776e352b02" : element == "Earth"
                    ? "651600a51edd20141adb67696986c582"
                    : "04944455200bc224d955a8e9bbd64f3f");
            BlueprintUnit water = Exact<BlueprintUnit>("56372b0a2749c224392a5ee74105c534");
            Check(assertions, rows, id + "-exact-native-catalog",
                component.SpellParents.Length == 18 && component.HasSubtype(nativeUnit) &&
                !component.HasSubtype(water) &&
                ReferenceEquals(component.Subtype, Exact<BlueprintFeature>(
                    ElementalSummonInsightPolicy.NativeSubtypeGuid(id))),
                "subtype=" + component.Subtype.AssetGuid + ";unit=" + nativeUnit.AssetGuid);

            foreach (int family in new[] { 0, 1 })
            {
                string prefix = family == 0 ? "SM" : "SNA";
                caster.Descriptor.RemoveFact(trait.Marker);
                RuleSummonUnit[] baseline = Cast(harness, books[family], family, 2,
                    "Small" + element + "Elemental", false, 0, assertions, rows,
                    id + "-" + prefix + "-base", true);
                Cast(harness, books[family], family, 3, "Small" + element + "Elemental",
                    true, 0, assertions, rows, id + "-" + prefix + "-base-multiple", false);
                caster.Descriptor.AddFact(trait.Marker);
                ElementalHeritageRuntime.Reconcile(caster.Descriptor, null, null);
                Check(assertions, rows, id + "-" + prefix + "-replacement",
                    caster.Descriptor.HasFact(trait.Provider) &&
                    !caster.Descriptor.HasFact(race.Affinity) &&
                    caster.Descriptor.HasFact(race.Resistance) &&
                    caster.Descriptor.HasFact(race.SlaFeature),
                    "affinity replaced; resistance and SLA retained");
                RuleSummonUnit[] active = Cast(harness, books[family], family, 2,
                    "Small" + element + "Elemental", false, 12, assertions, rows,
                    id + "-" + prefix + "-active", false);
                double delta = Lifetime(active[0]) - Lifetime(baseline[0]);
                Check(assertions, rows, id + "-" + prefix + "-actual-lifetime",
                    Math.Abs(delta - 12) < 0.1 &&
                    active[0].Duration.Seconds == baseline[0].Duration.Seconds,
                    "canonical buff delta seconds=" + delta + ";base unchanged");
                Cast(harness, books[family], family, 3, "Small" + element + "Elemental",
                    true, 12, assertions, rows, id + "-" + prefix + "-multiple", false);
                Cast(harness, books[family], family, 2, "SmallWaterElemental", false,
                    0, assertions, rows, id + "-" + prefix + "-nonmatching", false);
            }
            NativeRuleMatrix(harness, books[0], nativeUnit, component, id, assertions, rows, transient);
            caster.Descriptor.RemoveFact(trait.Marker);
            Check(assertions, rows, id + "-removed-provider",
                !caster.Descriptor.HasFact(trait.Provider) &&
                    caster.Descriptor.HasFact(race.Affinity),
                "base affinity restored after Insight removal");
        }

        private static Spellbook Book(UnitEntityData caster, BlueprintCharacterClass characterClass)
        {
            Spellbook book = caster.Descriptor.GetSpellbook(characterClass);
            if (book == null) throw new InvalidOperationException("Native class spellbook is missing.");
            while (book.CasterLevel < 5) book.AddCasterLevel();
            book.UpdateAllSlotsSize(false);
            book.Rest();
            return book;
        }

        private static RuleSummonUnit[] Cast(ElementalUndineFeatScenario.PortalHarness harness,
            Spellbook book, int family, int tier, string creature, bool multiple,
            double expectedBonusSeconds, ICollection<RuntimeTestAssertion> assertions,
            JArray rows, string label, bool cancellation)
        {
            BlueprintAbility parent = Exact<BlueprintAbility>(
                ElementalSummonInsightPolicy.NativeParentGuids[family * 9 + tier - 1]);
            BlueprintAbility selected = Selected(family, tier, creature, multiple);
            if (!book.Blueprint.SpellList.Contains(parent))
                throw new InvalidOperationException("The native family is not on its real class list.");
            book.Rest();
            int level = book.Blueprint.SpellList.GetLevel(parent);
            book.AddKnown(level, parent, true);
            SpellSlot slot = book.GetMemorizedSpellSlots(level).FirstOrDefault(value =>
                value.Spell != null && ReferenceEquals(value.Spell.Blueprint, parent));
            if (slot == null)
            {
                if (!book.Memorize(new AbilityData(parent, book), null))
                    throw new InvalidOperationException("Native summon preparation failed.");
                slot = book.GetMemorizedSpellSlots(level).First(value => value.Spell != null &&
                    ReferenceEquals(value.Spell.Blueprint, parent));
            }
            // Newly memorized native slots become usable at rest; resting
            // before Memorize does not make a subsequently added slot ready.
            book.Rest();
            slot.Spell.ParamSpellSlot = slot;
            var data = new AbilityData(slot.Spell, selected) { ParamSpellSlot = slot };
            UnitEntityData caster = harness.Caster;
            var target = new TargetWrapper(caster.Position + new Vector3(2f, 0f, 0f));
            int before = harness.Rules.Count;
            int placementsBefore = harness.PlacementCalls;
            int positionsBefore = harness.PositionRequests;
            int callbacksBefore = harness.RuleCallbacks;
            int emptyBefore = harness.EmptyRuleCallbacks;
            int exceptionsBefore = harness.NativeExceptions;
            if (cancellation)
            {
                bool availableBeforeCancel = slot.Available;
                UnitUseAbility canceled = ElementalUndineFeatScenario.CreateCommand(data, target, caster);
                caster.Commands.Run(canceled);
                bool installed = caster.Commands.Contains(canceled) && !canceled.IsStarted;
                caster.Commands.InterruptAll(true);
                caster.Commands.RemoveFinishedAndUpdateQueue();
                Check(assertions, rows, label + "-cancel", installed && availableBeforeCancel &&
                    slot.Available == availableBeforeCancel && harness.Rules.Count == before,
                    "installed=" + installed + ";slotBefore=" + availableBeforeCancel +
                    ";slotAfter=" + slot.Available + ";summonsBefore=" + before +
                    ";summonsAfter=" + harness.Rules.Count);
            }
            UnitUseAbility command = ElementalUndineFeatScenario.CreateCommand(data, target, caster);
            bool available = data.IsAvailable && data.CanTarget(target) && command.CanStart;
            if (!available) throw new InvalidOperationException(label +
                ": prepared cast is unavailable; ability=" + data.IsAvailable +
                ";target=" + data.CanTarget(target) + ";canStart=" + command.CanStart +
                ";slot=" + slot.Available + ";level=" + data.SpellLevel +
                ";book=" + (data.Spellbook != null));
            object result = ElementalUndineFeatScenario.InvokeCommandAction(command);
            AbilityExecutionProcess process = command.ExecutionProcess;
            bool detached = false;
            if (process != null) ElementalUndineFeatScenario.CompleteProcess(process, out detached);
            ElementalUndineFeatScenario.InvokeCommandEnded(command, false);
            for (int tick = 0; tick < 16; tick++) Game.Instance.EntityCreator.Tick();
            RuleSummonUnit[] rules = harness.Rules.Skip(before).ToArray();
            Check(assertions, rows, label + "-native-exceptions",
                harness.NativeExceptions == exceptionsBefore,
                "native exception reports=" + (harness.NativeExceptions - exceptionsBefore));
            Check(assertions, rows, label + "-native-pool-membership",
                rules.Length > 0 && rules.All(value => harness.HasPoolMembership(value.SummonedUnit)),
                "summons=" + rules.Length + ";registered=" + rules.Count(value =>
                    harness.HasPoolMembership(value.SummonedUnit)));
            bool count = harness.PlacementCalls == placementsBefore + 1 &&
                harness.PositionRequests - positionsBefore == harness.LastPlacementCount &&
                rules.Length == harness.LastPlacementCount &&
                (multiple ? rules.Length >= 1 && rules.Length <= 3 : rules.Length == 1);
            Check(assertions, rows, label + "-command", result != null &&
                result.ToString() == "Success" && process != null &&
                (process.IsEnded || detached) && !slot.Available && count,
                "result=" + result + ";process=" + (process != null) + ";ended=" +
                (process != null && process.IsEnded) + ";detached=" + detached +
                ";slot=" + slot.Available + ";summons=" + rules.Length +
                ";nativeCount=" + harness.LastPlacementCount + ";placements=" +
                (harness.PlacementCalls - placementsBefore) + ";positions=" +
                (harness.PositionRequests - positionsBefore) + ";callbacks=" +
                (harness.RuleCallbacks - callbacksBefore) + ";empty=" +
                (harness.EmptyRuleCallbacks - emptyBefore));
            Check(assertions, rows, label + "-duration", count && rules.All(value =>
                Math.Abs(value.BonusDuration.Seconds.TotalSeconds - expectedBonusSeconds) < 0.01 &&
                value.Duration.Seconds.TotalSeconds == 30 && value.SummonedUnit != null &&
                !value.SummonedUnit.IsEnemy(caster) &&
                value.SummonedUnit.Descriptor.Buffs.GetBuff(
                    BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null),
                "base=" + string.Join(",", rules.Select(value => value.Duration.Seconds.TotalSeconds)) +
                ";bonus=" + string.Join(",", rules.Select(value => value.BonusDuration.Seconds.TotalSeconds)));
            // A count mismatch is already a release-failing assertion, but
            // other traits/boundaries can still be observed safely. A missing
            // unit cannot support a lifecycle observation and stops this path.
            if (rules.Length == 0) throw new InvalidOperationException("Native summon produced no unit: " + label);
            return rules;
        }

        private static void NativeRuleMatrix(ElementalUndineFeatScenario.PortalHarness harness,
            Spellbook book, BlueprintUnit nativeUnit, ElementalSummonInsight component,
            ElementalAlternateTraitId id, ICollection<RuntimeTestAssertion> assertions, JArray rows,
            ICollection<UnityEngine.Object> transient)
        {
            BlueprintAbility parent = component.SpellParents[1];
            string element = id == ElementalAlternateTraitId.FireInsight ? "Fire" :
                id == ElementalAlternateTraitId.EarthInsight ? "Earth" : "Air";
            BlueprintAbility selected = Selected(0, 2, "Small" + element + "Elemental", false);
            // A native selector cannot construct an execution context. Use
            // its actual castable leaf while retaining the real spellbook.
            var ordinary = new AbilityData(new AbilityData(parent, book), selected);
                for (int i = 0; i < 7; i++)
                {
                    AbilityData data = ordinary;
                    bool linked = true;
                    int duration = 5;
                    if (i == 1) data = new AbilityData(selected, harness.Caster.Descriptor);
                    if (i == 2 || i == 3)
                    {
                        BlueprintAbility copy = UnityEngine.Object.Instantiate(selected);
                        transient.Add(copy);
                        copy.Type = i == 2 ? AbilityType.SpellLike : AbilityType.Supernatural;
                        data = new AbilityData(copy, harness.Caster.Descriptor);
                    }
                    if (i == 4) data = new AbilityData(Exact<BlueprintAbility>(
                        ElementalRaceIdentityCatalog.BurningHandsGuid), book);
                    if (i == 5) linked = false;
                    if (i == 6) duration = 0;
                    var context = new AbilityExecutionContext(data, data.CalculateParams(),
                        new TargetWrapper(harness.Caster.Position), Rulebook.CurrentContext);
                    var rule = new RuleSummonUnit(harness.Caster, nativeUnit,
                        harness.Caster.Position, duration.Rounds(), 5)
                    {
                        Context = context, Reason = context, DoNotLinkToCaster = !linked,
                        BonusDuration = 7.Rounds()
                    };
                    Rulebook.Trigger(rule);
                    // Rule-only negative fixtures still enqueue native unit
                    // creation. Drain it while the unit is alive, exactly as
                    // the command fixtures do, before any fixture disposal.
                    for (int tick = 0; tick < 16; tick++) Game.Instance.EntityCreator.Tick();
                    Check(assertions, rows, id + "-rule-boundary-" + i,
                        rule.SummonedUnit != null &&
                        rule.BonusDuration.Seconds.TotalSeconds == (i == 0 ? 54 : 42),
                        "case=" + i + ";type=" + data.Blueprint.Type +
                        ";book=" + (data.Spellbook != null) + ";bonus=" +
                        rule.BonusDuration.Seconds.TotalSeconds);
                }
                BlueprintFeature foreignSubtype = UnityEngine.Object.Instantiate(component.Subtype);
                BlueprintUnit foreignUnit = UnityEngine.Object.Instantiate(nativeUnit);
                transient.Add(foreignSubtype);
                transient.Add(foreignUnit);
                foreignUnit.AddFacts = new BlueprintUnitFact[] { foreignSubtype };
                Check(assertions, rows, id + "-foreign-subtype-identity",
                    foreignSubtype.AssetGuid == component.Subtype.AssetGuid &&
                    !component.HasSubtype(foreignUnit), "same GUID, different reference rejected");
                BlueprintAbility foreignParent = UnityEngine.Object.Instantiate(parent);
                transient.Add(foreignParent);
                Check(assertions, rows, id + "-foreign-family-identity",
                    foreignParent.AssetGuid == parent.AssetGuid &&
                    !component.IsNamedFamily(new AbilityData(foreignParent, book)),
                    "same GUID, different family reference rejected");
                var convertedUnrelated = new AbilityData(ordinary,
                    Exact<BlueprintAbility>(ElementalRaceIdentityCatalog.BurningHandsGuid));
                Check(assertions, rows, id + "-converted-slot-exclusion",
                    !component.IsNamedFamily(convertedUnrelated),
                    "a sacrificed summon slot does not turn a different spell into Summon Monster");
        }

        private static BlueprintAbility Selected(int family, int tier, string creature, bool multiple)
        {
            string name = "KMG_Summoning_Ability_" + (family == 0 ? "SM" : "SNA") +
                "_Tier" + tier + "_" + creature + "_" + (multiple ? "OneD3" : "One");
            return BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintAbility>().Single(value => value.name == name);
        }

        private static double Lifetime(RuleSummonUnit rule)
        {
            Buff buff = rule.SummonedUnit.Descriptor.Buffs.GetBuff(
                BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            return buff.TimeLeft.TotalSeconds;
        }

        private static T Exact<T>(string guid) where T : BlueprintScriptableObject
        {
            return BlueprintLibraryLookup.RequireExact<T>(BlueprintBootstrap.Library,
                guid, "native summon Insight fixture identity");
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions,
            JArray rows, string name, bool pass, string observed)
        {
            assertions.Add(new RuntimeTestAssertion {
                Name = "elemental-insight-" + name, Expected = "exact typed spell-only native summon contract",
                Observed = observed, Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "prepared native UnitUseAbility, RuleSummonUnit, canonical lifecycle buff, and exact subtype facts"
            });
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
        }
    }
}
