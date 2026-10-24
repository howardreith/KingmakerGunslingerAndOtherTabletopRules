using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Fixed rule-consumer cases only: no arbitrary workflow, spell cast,
        // command/turn qualification or fabricated hit/save result.
        private IEnumerable<int> ReviewSprints3031Defenses(ExpandedSummoningCorrectionFixture fixture)
        {
            string[] keys = { "lemure", "dretch", "hound-archon", "lantern-archon", "astral-deva", "trumpet-archon" };
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var source = fixture.Hostile;
            var sourcePosition = source.Position;
            var sourceAlignment = source.Descriptor.Alignment.Value;
            var completed = new List<UnitEntityData>();
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            var partyItems = Game.Instance.Player.Inventory.Items.ToArray();
            var partyCounts = partyItems.Select(item => item.Count).ToArray();
            var targetBuffs = source.Buffs.Enumerable.Select(buff => buff.Blueprint.AssetGuid)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var transient = new List<UnityEngine.Object>();
            var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                FcbMagicMissileGuid, "six-creature native SR consumer");
            var poisonAbility = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                "KMG.Summoning.OutsiderCelestial.Dretch.StinkingCloud"];
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                ResetExpandedSummoningHostile(fixture);
                // Demonstrate that relevant residue fails this same reset
                // predicate before trusting it between creature cases.
                var residue = TraitCondition("S3031-reset-negative", SpellDescriptor.None, transient);
                var residueRule = ApplySprint12Buff(source, residue, source, TimeSpan.FromMinutes(1), 10);
                var dirty = Lemure30ResetState(fixture, completed, targetBuffs, areasBefore);
                RemoveSprint12Buff(source, residue);
                var clean = Lemure30ResetState(fixture, completed, targetBuffs, areasBefore);
                bool detects = residueRule.AppliedBuff != null && !(bool)dirty["clean"] &&
                    !(bool)dirty["targetBuffsExact"] && (bool)clean["clean"];
                Sprint25Check("defenses-reset-negative", detects, new JObject {
                    ["source"] = source.UniqueId, ["dirty"] = dirty, ["clean"] = clean,
                    ["ownedResidueRemoved"] = !source.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, residue)) },
                    "An intentionally owned native buff is detected by the exact next-case reset predicate; removal restores the original state.");
                if (!detects) throw new InvalidOperationException("Defense reset detector failed.");

                foreach (string key in keys)
                {
                    var owner = CastExpandedSummoningOwnTier(fixture, key);
                    if (!fixture.Created.Any(actor => ReferenceEquals(actor, owner)))
                        throw new InvalidOperationException("Defense actor is not request-owned.");
                    completed.Add(owner); Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    int hp = owner.Stats.HitPoints.BaseValue, injury = owner.Damage;
                    var items = owner.Descriptor.Inventory.Items.ToArray();
                    try
                    {
                        for (int frame = 0; frame < 16; frame++) yield return 0;
                        // Keep the source inside the Astral globe for the SR
                        // and descriptor probes; do not confuse spell-level
                        // exclusion with racial spell resistance/immunity.
                        PlaceExpandedSummoningUnit(source, owner.Position + Vector3.right);
                        source.Descriptor.Alignment.Set(Alignment.NeutralGood);
                        owner.Stats.HitPoints.BaseValue = 10000; // capacity only
                        bool fiend = key == "lemure" || key == "dretch";
                        var physical = new JArray();
                        foreach (string bypass in new[] { "mundane", "good", "evil", "silver", "cold-iron", "good-silver", "good-cold-iron" })
                        {
                            var packet = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing);
                            packet.AddBonus(20);
                            if (bypass.StartsWith("good", StringComparison.Ordinal)) packet.AddAlignment(DamageAlignment.Good);
                            if (bypass == "evil") packet.AddAlignment(DamageAlignment.Evil);
                            if (bypass == "silver" || bypass == "good-silver") packet.AddMaterial(PhysicalDamageMaterial.Silver);
                            if (bypass == "cold-iron" || bypass == "good-cold-iron") packet.AddMaterial(PhysicalDamageMaterial.ColdIron);
                            bool bypasses = fiend ? bypass.StartsWith("good", StringComparison.Ordinal) ||
                                (key == "lemure" ? bypass == "silver" : bypass == "cold-iron") : bypass == "evil";
                            physical.Add(Astral31Damage(owner, source, packet, bypasses ? 20 : fiend ? 15 : 10, false, bypass));
                        }
                        bool physicalExact = physical.All(row => (bool)row["exact"]);
                        Sprint25Check("defenses-" + key + "-dr", physicalExact,
                            new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                                ["packets"] = physical, ["controlledCapacity"] = 10000 },
                            "Actual independent attributed native physical damage packets distinguish each permitted bypass alone, both together and unrelated alignment/material controls.");
                        if (!physicalExact) throw new InvalidOperationException("Native defense DR failed: " + key);
                        var energies = new JArray();
                        foreach (var energy in new[] { DamageEnergyType.Acid, DamageEnergyType.Cold,
                            DamageEnergyType.Fire, DamageEnergyType.Electricity, DamageEnergyType.Sonic })
                        {
                            bool immune = key == "lemure" ? energy == DamageEnergyType.Fire :
                                key == "astral-deva" ? energy == DamageEnergyType.Acid || energy == DamageEnergyType.Cold :
                                energy == DamageEnergyType.Electricity;
                            bool resist = fiend ? energy == DamageEnergyType.Acid || energy == DamageEnergyType.Cold ||
                                key == "dretch" && energy == DamageEnergyType.Fire :
                                key == "astral-deva" && (energy == DamageEnergyType.Fire || energy == DamageEnergyType.Electricity);
                            var packet = new EnergyDamage(new DiceFormula(0, DiceType.Zero), energy); packet.AddBonus(20);
                            energies.Add(Astral31Damage(owner, source, packet, resist ? 10 : 20, immune, energy.ToString()));
                        }
                        bool energyExact = energies.All(row => (bool)row["exact"]);
                        Sprint25Check("defenses-" + key + "-energy", energyExact,
                            new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["source"] = source.UniqueId, ["packets"] = energies },
                            "Native packet immunity, pre-difficulty resistance, resolved damage and actual HP deltas are separate observations, including unrelated sonic damage.");
                        if (!energyExact) throw new InvalidOperationException("Native defense energy failed: " + key);
                        owner.Damage = injury; owner.Stats.HitPoints.BaseValue = hp;

                        int expectedSr = key == "hound-archon" ? 15 : key == "astral-deva" || key == "trumpet-archon" ? 25 : 0;
                        var low = Astral31Resistance(source, owner, missile, 1, 1);
                        var lowAttempts = new JArray { Sprints3031DefenseSrRow(low) };
                        for (int attempt = 1; expectedSr > 0 && !low.IsSpellResisted && attempt < 12; attempt++)
                        {
                            low = Astral31Resistance(source, owner, missile, 1, 1);
                            lowAttempts.Add(Sprints3031DefenseSrRow(low));
                        }
                        var high = Astral31Resistance(source, owner, missile, 50, 1);
                        bool srExact = missile.SpellResistance && low.SpellResistance == expectedSr &&
                            high.SpellResistance == expectedSr && !low.TargetIsImmune && !high.TargetIsImmune &&
                            !high.IsSpellResisted && low.IsSpellResisted == (expectedSr > 0) &&
                            ReferenceEquals(low.Target, owner) && ReferenceEquals(low.Context.MaybeCaster, source);
                        int ordinarySave = Astral31SaveValue(source, owner, missile);
                        int poisonSave = Astral31SaveValue(source, owner, poisonAbility);
                        bool saveExact = poisonSave == ordinarySave + (fiend ? 0 : 4);
                        Sprint25Check("defenses-" + key + "-sr-poison-save", srExact && saveExact,
                            new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["lowClAttempts"] = lowAttempts,
                                ["highCl"] = Sprints3031DefenseSrRow(high), ["expectedRacialSr"] = expectedSr,
                                ["ordinaryFort"] = ordinarySave, ["poisonFort"] = poisonSave,
                                ["sourceInsideGlobe"] = (source.Position-owner.Position).magnitude < 2f,
                                ["ruleOnlyControlledClNoCast"] = true },
                            "Actual SR-allowing rules use disclosed CL1/50 without changing racial SR; live poison and ordinary native saves isolate the racial bonus from immunity.");
                        if (!srExact || !saveExact) throw new InvalidOperationException("Native defense SR/save failed: " + key);

                        var descriptors = new JArray();
                        foreach (var spec in new[] {
                            new { Name="poison", Descriptor=SpellDescriptor.Poison, Immune=fiend },
                            new { Name="fear", Descriptor=SpellDescriptor.MindAffecting | SpellDescriptor.Fear, Immune=key == "lemure" },
                            new { Name="petrification", Descriptor=SpellDescriptor.Petrified, Immune=!fiend },
                            new { Name="unrelated", Descriptor=SpellDescriptor.None, Immune=false } })
                        {
                            var buff = TraitCondition("S3031-" + key + "-" + spec.Name, spec.Descriptor, transient);
                            var ability = TraitEffect("S3031-" + key + "-" + spec.Name, spec.Descriptor, transient);
                            ability.Type = AbilityType.Supernatural;
                            try
                            {
                                var control = ApplySprint12Buff(source, buff, source, TimeSpan.FromSeconds(6), 10);
                                var application = ApplySprint12Buff(owner, buff, source, TimeSpan.FromSeconds(6), 10);
                                var effect = Astral31Resistance(source, owner, ability, 1, 1);
                                var controlEffect = Astral31Resistance(source, source, ability, 1, 1);
                                bool exact = control.AppliedBuff != null && !control.Immunity && !controlEffect.TargetIsImmune &&
                                    application.Immunity == spec.Immune && (application.AppliedBuff == null) == spec.Immune &&
                                    effect.TargetIsImmune == spec.Immune;
                                descriptors.Add(new JObject { ["id"] = spec.Name, ["descriptor"] = spec.Descriptor.ToString(),
                                    ["expectedImmune"] = spec.Immune, ["buffImmune"] = application.Immunity,
                                    ["buffApplied"] = application.AppliedBuff != null, ["controlApplied"] = control.AppliedBuff != null,
                                    ["effect"] = Astral31ResistanceRow(effect), ["controlEffect"] = Astral31ResistanceRow(controlEffect), ["exact"] = exact });
                            }
                            finally { RemoveSprint12Buff(owner, buff); RemoveSprint12Buff(source, buff); }
                        }
                        bool descriptorsExact = descriptors.All(row => (bool)row["exact"]);
                        Sprint25Check("defenses-" + key + "-descriptors", descriptorsExact,
                            new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                                ["probes"] = descriptors, ["controlledDescriptorProbes"] = true },
                            "Separate real buff-application and ability-immunity consumers use susceptible controls for poison, fear, petrification and an unrelated descriptor; these are not spell casts.");
                        if (!descriptorsExact) throw new InvalidOperationException("Native defense descriptors failed: " + key);
                        // Untagged actual condition separates the native
                        // condition immunity from descriptor rejection.
                        var petrify = TraitCondition("S3031-" + key + "-actual-petrify", SpellDescriptor.None, transient);
                        var condition = ScriptableObject.CreateInstance<AddCondition>();
                        condition.Condition = UnitCondition.Petrified; transient.Add(condition);
                        petrify.ComponentsArray = petrify.ComponentsArray.Concat(new BlueprintComponent[] { condition }).ToArray();
                        PrepareTraitBlueprints(petrify);
                        try
                        {
                            bool ownerBefore = owner.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            bool sourceBefore = source.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            var control = ApplySprint12Buff(source, petrify, source, TimeSpan.FromSeconds(6), 10);
                            var actual = ApplySprint12Buff(owner, petrify, source, TimeSpan.FromSeconds(6), 10);
                            bool ownerPetrified = owner.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            bool sourcePetrified = source.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            bool exact = !ownerBefore && !sourceBefore && control.AppliedBuff != null && sourcePetrified &&
                                actual.AppliedBuff != null && ownerPetrified == fiend;
                            Sprint25Check("defenses-" + key + "-petrification-condition", exact,
                                new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                                    ["ownerBefore"] = ownerBefore, ["controlBefore"] = sourceBefore,
                                    ["ownerBuffApplied"] = actual.AppliedBuff != null, ["controlBuffApplied"] = control.AppliedBuff != null,
                                    ["ownerPetrified"] = ownerPetrified, ["controlPetrified"] = sourcePetrified,
                                    ["expectedOwnerPetrified"] = fiend, ["controlledUntaggedConditionBuff"] = true },
                                "An actual untagged native AddCondition consumer petrifies the susceptible control and fiends; each celestial blocks the condition independently of descriptor rejection.");
                            if (!exact) throw new InvalidOperationException("Native petrification condition failed: " + key);
                        }
                        finally { RemoveSprint12Buff(owner, petrify); RemoveSprint12Buff(source, petrify); }
                    }
                    finally
                    {
                        owner.Damage = injury; owner.Stats.HitPoints.BaseValue = hp;
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                        source.Descriptor.Alignment.Set(sourceAlignment); PlaceExpandedSummoningUnit(source, sourcePosition);
                    }
                    ResetExpandedSummoningHostile(fixture);
                    var deadline = DateTime.UtcNow.AddSeconds(12);
                    JObject reset;
                    do
                    {
                        Game.Instance.IsPaused = false; yield return 0;
                        reset = Lemure30ResetState(fixture, completed, targetBuffs, areasBefore);
                    } while ((!(bool)reset["clean"] || completed.Any(actor => actor.View != null) ||
                        items.Any(item => item.Collection != null)) && DateTime.UtcNow < deadline);
                    bool safe = (bool)reset["clean"] && completed.All(actor => actor.View == null) &&
                        items.All(item => !item.IsLootable && item.Collection == null) &&
                        partyItems.SequenceEqual(Game.Instance.Player.Inventory.Items) &&
                        partyCounts.SequenceEqual(partyItems.Select(item => item.Count));
                    reset["creature"] = key; reset["owner"] = owner.UniqueId;
                    reset["ownedItemsRetired"] = items.All(item => !item.IsLootable && item.Collection == null);
                    reset["partyInventoryExact"] = partyItems.SequenceEqual(Game.Instance.Player.Inventory.Items) &&
                        partyCounts.SequenceEqual(partyItems.Select(item => item.Count));
                    Sprint25Check("defenses-" + key + "-reset", safe, reset,
                        "This case's actual owners, items, areas, buffs, injury, commands, memory and combat state meet the next fixed case's preconditions.");
                    if (!safe) throw new InvalidOperationException("Defense shared reset failed: " + key);
                }
            }
            finally
            {
                DisposeExpandedSummoningUnits(fixture.Created, completed);
                source.Descriptor.Alignment.Set(sourceAlignment); PlaceExpandedSummoningUnit(source, sourcePosition);
                foreach (var value in transient) if (value != null) UnityEngine.Object.DestroyImmediate(value);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private static JObject Sprints3031DefenseSrRow(RuleSpellResistanceCheck rule)
        {
            var row = Astral31ResistanceRow(rule);
            row["nativeD20"] = rule.Roll.Value;
            row["spellPenetration"] = rule.SpellPenetration;
            row["additionalSpellPenetration"] = rule.AdditionalSpellPenetration;
            row["ignoreSpellResistance"] = rule.IgnoreSpellResistance;
            row["forcedRolls"] = 0;
            return row;
        }
    }
}
