using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
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
            AreaEffectEntityData[] areasBefore = null;
            var partyItems = Game.Instance.Player.Inventory.Items.ToArray();
            var partyCounts = partyItems.Select(item => item.Count).ToArray();
            string[] targetBuffs = null;
            var sourceEvents = new Sprints3031DefenseControlEvents(source);
            var transient = new List<UnityEngine.Object>();
            var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                FcbMagicMissileGuid, "six-creature native SR consumer");
            var poisonAbility = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                "KMG.Summoning.OutsiderCelestial.Dretch.StinkingCloud"];
            try
            {
                EventBus.Subscribe(sourceEvents);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
                targetBuffs = source.Descriptor.Buffs.RawFacts.Select(buff => buff.Blueprint.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
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
                    var items = owner.Descriptor.Inventory.Items.ToList();
                    sourceEvents.Rows.Clear();
                    try
                    {
                        for (int frame = 0; frame < 16; frame++) yield return 0;
                        var buffs = owner.Descriptor.Buffs.RawFacts.Select(buff => new JObject {
                            ["guid"] = buff.Blueprint.AssetGuid, ["name"] = buff.Blueprint.name }).ToArray();
                        bool noTemplate = !buffs.Any(buff => ExpandedSummoningAbilityBuilder.NativeTemplateIds.Contains((string)buff["guid"]) ||
                            ((string)buff["name"]).StartsWith("KMG_Summoning_Template_", StringComparison.Ordinal));
                        bool summonMarker = owner.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null;
                        var route = ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One);
                        var summonAbility = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                            ExpandedSummoningIdentityCatalog.AbilitySymbol(route)];
                        SpellDescriptor summonDescriptors = summonAbility.ComponentsArray.OfType<SpellDescriptorComponent>().Single().Descriptor;
                        var alignmentMask = SpellDescriptor.Good | SpellDescriptor.Evil | SpellDescriptor.Law | SpellDescriptor.Chaos;
                        var expected = key == "lemure" ? SpellDescriptor.Law | SpellDescriptor.Evil :
                            key == "dretch" ? SpellDescriptor.Chaos | SpellDescriptor.Evil :
                            key == "astral-deva" ? SpellDescriptor.Good : SpellDescriptor.Law | SpellDescriptor.Good;
                        bool exactRoute = noTemplate && summonMarker && (summonDescriptors & alignmentMask) == expected;
                        Sprint25Check("defenses-" + key + "-untemplated", exactRoute,
                            new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["buffs"] = new JArray(buffs),
                                ["nativeTemplateIds"] = new JArray(ExpandedSummoningAbilityBuilder.NativeTemplateIds),
                                ["noInheritedTemplate"] = noTemplate, ["nativeSummonMarker"] = summonMarker,
                                ["route"] = summonAbility.AssetGuid, ["descriptors"] = summonDescriptors.ToString(),
                                ["expectedAlignmentDescriptors"] = expected.ToString() },
                            "Actual own-tier summons retain their lifecycle marker without inherited native or project templates; the registered route has the creature's exact alignment subtypes.");
                        if (!exactRoute) throw new InvalidOperationException("Native untemplated route failed: " + key);
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
                        var airborneRow = Sprints3031AirborneProbe(key, owner, source, transient);
                        Sprint25Check("defenses-" + key + "-airborne", (bool)airborneRow["exact"], airborneRow,
                            "The constructed unit owns exactly one native Airborne fact when it flies (Lantern and Astral are the established flyers on it) and none when grounded; each of that fact's own condition and descriptor consumers is observed on the unit against the grounded susceptible source.");
                        if (!(bool)airborneRow["exact"]) throw new InvalidOperationException("Native Airborne consumer failed: " + key);
                        // Untagged actual condition separates the native
                        // condition immunity from descriptor rejection. Its
                        // positive control is disposable and independent of
                        // the rule source reused by the next creature.
                        var conditionControl = CastExpandedSummoningOwnTier(fixture, "wolf");
                        if (!fixture.Created.Any(actor => ReferenceEquals(actor, conditionControl)))
                            throw new InvalidOperationException("Condition control is not request-owned.");
                        completed.Add(conditionControl);
                        Sprint28SetBrainActivePreservingActions(conditionControl, false);
                        conditionControl.Commands.InterruptAll(true); conditionControl.Commands.RemoveFinishedAndUpdateQueue();
                        items.AddRange(conditionControl.Descriptor.Inventory.Items);
                        var petrify = TraitCondition("S3031-" + key + "-actual-petrify", SpellDescriptor.None, transient);
                        var condition = ScriptableObject.CreateInstance<AddCondition>();
                        condition.Condition = UnitCondition.Petrified; transient.Add(condition);
                        petrify.ComponentsArray = petrify.ComponentsArray.Concat(new BlueprintComponent[] { condition }).ToArray();
                        PrepareTraitBlueprints(petrify);
                        try
                        {
                            bool ownerBefore = owner.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            bool sourceBefore = conditionControl.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            var control = ApplySprint12Buff(conditionControl, petrify, source, TimeSpan.FromSeconds(6), 10);
                            var actual = ApplySprint12Buff(owner, petrify, source, TimeSpan.FromSeconds(6), 10);
                            bool ownerPetrified = owner.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            bool sourcePetrified = conditionControl.Descriptor.State.HasCondition(UnitCondition.Petrified);
                            bool exact = !ownerBefore && !sourceBefore && control.AppliedBuff != null && sourcePetrified &&
                                actual.AppliedBuff != null && ownerPetrified == fiend;
                            Sprint25Check("defenses-" + key + "-petrification-condition", exact,
                                new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                                    ["conditionControl"] = conditionControl.UniqueId, ["controlSeparateFromSource"] = !ReferenceEquals(conditionControl, source),
                                    ["ownerBefore"] = ownerBefore, ["controlBefore"] = sourceBefore,
                                    ["ownerBuffApplied"] = actual.AppliedBuff != null, ["controlBuffApplied"] = control.AppliedBuff != null,
                                    ["ownerPetrified"] = ownerPetrified, ["controlPetrified"] = sourcePetrified,
                                    ["expectedOwnerPetrified"] = fiend, ["controlledUntaggedConditionBuff"] = true },
                                "An actual untagged native AddCondition consumer petrifies the susceptible control and fiends; each celestial blocks the condition independently of descriptor rejection.");
                            if (!exact) throw new InvalidOperationException("Native petrification condition failed: " + key);
                        }
                        finally
                        {
                            RemoveSprint12Buff(owner, petrify); RemoveSprint12Buff(conditionControl, petrify);
                            DisposeExpandedSummoningUnits(fixture.Created, new[] { conditionControl });
                        }
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
                    reset["source"] = source.UniqueId;
                    reset["sourceDamage"] = source.Damage; reset["expectedSourceDamage"] = fixture.HostileDamage;
                    reset["sourceDead"] = source.Descriptor.State.IsDead;
                    reset["sourcePetrified"] = source.Descriptor.State.HasCondition(UnitCondition.Petrified);
                    reset["sourceBuffsBefore"] = new JArray(targetBuffs);
                    reset["sourceBuffsAfter"] = new JArray(source.Descriptor.Buffs.RawFacts.Select(buff => buff.Blueprint.AssetGuid)
                        .OrderBy(value => value, StringComparer.Ordinal));
                    reset["sourceNativeEvents"] = sourceEvents.Rows.DeepClone();
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
                EventBus.Unsubscribe(sourceEvents);
                DisposeExpandedSummoningUnits(fixture.Created, completed);
                source.Descriptor.Alignment.Set(sourceAlignment); PlaceExpandedSummoningUnit(source, sourcePosition);
                foreach (var value in transient) if (value != null) UnityEngine.Object.DestroyImmediate(value);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        // Probes come from the native Airborne fact's own components, so the
        // check follows that fact rather than an assumed list. A component
        // kind without a direct consumer here is recorded, not passed.
        private JObject Sprints3031AirborneProbe(string key, UnitEntityData owner, UnitEntityData source,
            ICollection<UnityEngine.Object> transient)
        {
            var airborne = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "70cffb448c132fa409e49156d013b175", "native Airborne fact");
            bool flyer = key == "lantern-archon" || key == "astral-deva" || key == "trumpet-archon";
            int ownerFacts = owner.Descriptor.Progression.Features.Enumerable
                .Count(feature => ReferenceEquals(feature.Blueprint, airborne));
            bool sourceGrounded = !source.Descriptor.HasFact(airborne);
            var probes = new JArray();
            var unobserved = new JArray();
            int index = 0;
            foreach (var component in airborne.ComponentsArray)
            {
                var condition = component as AddConditionImmunity;
                var buffImmunity = component as BuffDescriptorImmunity;
                var abilityImmunity = component as SpellImmunityToSpellDescriptor;
                if (condition != null)
                {
                    bool ownerImmune = owner.Descriptor.State.HasConditionImmunity(condition.Condition);
                    bool sourceImmune = source.Descriptor.State.HasConditionImmunity(condition.Condition);
                    probes.Add(new JObject { ["component"] = "AddConditionImmunity", ["condition"] = condition.Condition.ToString(),
                        ["ownerImmune"] = ownerImmune, ["sourceImmune"] = sourceImmune,
                        ["exact"] = ownerImmune == flyer && !sourceImmune });
                }
                else if (buffImmunity != null && !buffImmunity.CheckFact && buffImmunity.IgnoreFeature == null)
                {
                    SpellDescriptor descriptor = buffImmunity.Descriptor;
                    var buff = TraitCondition("S3031-" + key + "-airborne-" + index, descriptor, transient);
                    try
                    {
                        var control = ApplySprint12Buff(source, buff, source, TimeSpan.FromSeconds(6), 10);
                        var application = ApplySprint12Buff(owner, buff, source, TimeSpan.FromSeconds(6), 10);
                        probes.Add(new JObject { ["component"] = "BuffDescriptorImmunity", ["descriptor"] = descriptor.ToString(),
                            ["ownerImmune"] = application.Immunity, ["ownerApplied"] = application.AppliedBuff != null,
                            ["sourceApplied"] = control.AppliedBuff != null,
                            ["exact"] = control.AppliedBuff != null && !control.Immunity &&
                                application.Immunity == flyer && (application.AppliedBuff == null) == flyer });
                    }
                    finally { RemoveSprint12Buff(owner, buff); RemoveSprint12Buff(source, buff); }
                }
                else if (abilityImmunity != null && abilityImmunity.CasterIgnoreImmunityFact == null)
                {
                    SpellDescriptor descriptor = abilityImmunity.Descriptor;
                    var ability = TraitEffect("S3031-" + key + "-airborne-" + index, descriptor, transient);
                    ability.Type = AbilityType.Supernatural;
                    var effect = Astral31Resistance(source, owner, ability, 1, 1);
                    var controlEffect = Astral31Resistance(source, source, ability, 1, 1);
                    probes.Add(new JObject { ["component"] = "SpellImmunityToSpellDescriptor", ["descriptor"] = descriptor.ToString(),
                        ["effect"] = Astral31ResistanceRow(effect), ["controlEffect"] = Astral31ResistanceRow(controlEffect),
                        ["exact"] = effect.TargetIsImmune == flyer && !controlEffect.TargetIsImmune });
                }
                else unobserved.Add(component == null ? "null" : component.GetType().FullName);
                index++;
            }
            bool exact = ownerFacts == (flyer ? 1 : 0) && sourceGrounded && probes.Count > 0 &&
                probes.All(row => (bool)row["exact"]);
            return new JObject { ["creature"] = key, ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                ["airborneFact"] = airborne.AssetGuid, ["expectedFlyer"] = flyer, ["ownerAirborneFacts"] = ownerFacts,
                ["sourceGrounded"] = sourceGrounded, ["probes"] = probes, ["unobservedComponents"] = unobserved,
                ["exact"] = exact };
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

        private sealed class Sprints3031DefenseControlEvents :
            IGlobalRulebookHandler<RuleDealDamage>, IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData _source;
            internal readonly JArray Rows = new JArray();
            internal Sprints3031DefenseControlEvents(UnitEntityData source) { _source = source; }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (ReferenceEquals(evt.Target, _source)) Rows.Add(new JObject {
                    ["event"] = "damage", ["initiator"] = evt.Initiator?.UniqueId,
                    ["target"] = evt.Target?.UniqueId, ["damage"] = evt.Damage,
                    ["ability"] = evt.Reason?.Context?.SourceAbility?.AssetGuid });
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (ReferenceEquals(evt.Initiator, _source)) Rows.Add(new JObject {
                    ["event"] = "buff", ["target"] = evt.Initiator?.UniqueId,
                    ["applied"] = evt.AppliedBuff != null, ["immune"] = evt.Immunity,
                    ["buff"] = evt.AppliedBuff?.Blueprint?.AssetGuid,
                    ["caster"] = evt.AppliedBuff?.Context?.MaybeCaster?.UniqueId });
            }
        }
    }
}
