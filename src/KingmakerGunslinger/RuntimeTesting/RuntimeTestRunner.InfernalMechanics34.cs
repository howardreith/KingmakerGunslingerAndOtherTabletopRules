using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Defense packets isolate native consumers. Gaze uses native commands;
        // neither section supplies successful rolls or fabricates resulting HP.
        private IEnumerable<int> ReviewInfernalMechanics34(ExpandedSummoningCorrectionFixture fixture)
        {
            var owners = new List<UnitEntityData>();
            var temporary = new List<UnityEngine.Object>();
            var trace = new InfernalEvents34();
            var cells = new JArray();
            var receipt = new JObject { ["phase"] = "sprint34-infernal-mechanics", ["cells"] = cells, ["events"] = trace.Rows };
            Action<string, bool, JObject, string> check = (id, pass, row, description) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint34-" + id, pass, row, description);
                if (!pass) throw new InvalidOperationException("Sprint34 native mechanics: " + id);
            };
            var source = fixture.Hostile;
            var donor = fixture.Blueprints.OfType<BlueprintUnit>().Single(value => value.AssetGuid == TheropodVisualPolicy.DonorGuid);
            var donorComponents = donor.ComponentsArray.ToArray(); var donorFacts = donor.AddFacts.ToArray();
            try {
                foreach (string key in new[] { "bearded-devil", "kyton-chain-devil" }) {
                    var owner = CastExpandedSummoningOwnTier(fixture, key); owners.Add(owner);
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    foreach (int frame in Outsider30NativeAppearance(owner, key)) yield return frame;
                    bool kyton = key == "kyton-chain-devil";
                    var baseRow = new JObject { ["key"] = key, ["owner"] = owner.UniqueId,
                        ["hp"] = owner.Stats.HitPoints.ModifiedValue, ["ac"] = owner.Stats.AC.ModifiedValue,
                        ["bab"] = owner.Stats.BaseAttackBonus.ModifiedValue,
                        ["fort"] = owner.Stats.SaveFortitude.ModifiedValue, ["reflex"] = owner.Stats.SaveReflex.ModifiedValue,
                        ["will"] = owner.Stats.SaveWill.ModifiedValue, ["type"] = owner.Blueprint.Type.AssetGuid,
                        ["facts"] = new JArray(owner.Blueprint.AddFacts.Select(value => value.AssetGuid)) };
                    bool baseline = owner.Stats.HitPoints.ModifiedValue == (kyton ? 60 : 57) &&
                        owner.Stats.AC.ModifiedValue == (kyton ? 21 : 19) && owner.Stats.BaseAttackBonus.ModifiedValue == (kyton ? 8 : 6) &&
                        owner.Stats.SaveFortitude.ModifiedValue == (kyton ? 8 : 9) &&
                        owner.Stats.SaveReflex.ModifiedValue == (kyton ? 9 : 7) && owner.Stats.SaveWill.ModifiedValue == 3 &&
                        owner.Blueprint.Race == null && !ReferenceEquals(owner.Blueprint.Type, donor.Type) &&
                        owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null &&
                        !owner.Buffs.Enumerable.Any(buff => ExpandedSummoningAbilityBuilder.NativeTemplateIds.Contains(buff.Blueprint.AssetGuid));
                    check("profile-" + key, baseline, baseRow, "Native untemplated profile and its own type, feats and summon lifetime marker.");
                    int capacity = owner.Stats.HitPoints.BaseValue, injury = owner.Damage;
                    try {
                        owner.Stats.HitPoints.BaseValue = 10000; // Capacity only, not a supplied damage result.
                        var physical = new JArray();
                        foreach (string bypass in new[] { "mundane", "good", "silver", "cold-iron", "evil" }) {
                            var packet = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing);
                            packet.AddBonus(20);
                            if (bypass == "good") packet.AddAlignment(DamageAlignment.Good);
                            if (bypass == "evil") packet.AddAlignment(DamageAlignment.Evil);
                            if (bypass == "silver") packet.AddMaterial(PhysicalDamageMaterial.Silver);
                            if (bypass == "cold-iron") packet.AddMaterial(PhysicalDamageMaterial.ColdIron);
                            physical.Add(Astral31Damage(owner, source, packet, bypass == "good" || bypass == "silver" ? 20 : 15, false, bypass));
                        }
                        var energy = new JArray();
                        foreach (var type in new[] { DamageEnergyType.Fire, DamageEnergyType.Cold, DamageEnergyType.Acid, DamageEnergyType.Electricity }) {
                            bool immune = kyton ? type == DamageEnergyType.Cold : type == DamageEnergyType.Fire;
                            bool resistant = !kyton && (type == DamageEnergyType.Cold || type == DamageEnergyType.Acid);
                            var packet = new EnergyDamage(new DiceFormula(0, DiceType.Zero), type); packet.AddBonus(20);
                            energy.Add(Astral31Damage(owner, source, packet, resistant ? 10 : 20, immune, type.ToString()));
                        }
                        check("defenses-" + key, physical.Concat(energy).All(row => (bool)row["exact"]),
                            new JObject { ["owner"] = owner.UniqueId, ["source"] = source.UniqueId, ["physical"] = physical,
                                ["energy"] = energy, ["capacityOnly"] = 10000, ["directNativeConsumerNotAttackDispatch"] = true },
                            "Actual native HP outcomes distinguish good OR silver bypass, unrelated sources, and each creature's own energy defenses.");
                    } finally { owner.Damage = injury; owner.Stats.HitPoints.BaseValue = capacity; }
                    var descriptors = new JArray();
                    foreach (var descriptor in new[] { SpellDescriptor.Poison, SpellDescriptor.Fear | SpellDescriptor.MindAffecting, SpellDescriptor.None }) {
                        var buff = TraitCondition("S34-" + key + "-" + descriptor, descriptor, temporary);
                        try {
                            var control = ApplySprint12Buff(source, buff, source, TimeSpan.FromSeconds(6), 10);
                            var result = ApplySprint12Buff(owner, buff, source, TimeSpan.FromSeconds(6), 10);
                            bool immune = !kyton && descriptor == SpellDescriptor.Poison;
                            descriptors.Add(new JObject { ["descriptor"] = descriptor.ToString(), ["immune"] = result.Immunity,
                                ["applied"] = result.AppliedBuff != null, ["controlApplied"] = control.AppliedBuff != null,
                                ["exact"] = control.AppliedBuff != null && !control.Immunity && result.Immunity == immune &&
                                    (result.AppliedBuff == null) == immune });
                        } finally { RemoveSprint12Buff(owner, buff); RemoveSprint12Buff(source, buff); }
                    }
                    var missile = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.AssetGuid == FcbMagicMissileGuid);
                    var lowRows = new JArray(); RuleSpellResistanceCheck low = null;
                    for (int attempt = 0; attempt < 8 && (low == null || !low.IsSpellResisted); attempt++) {
                        low = Astral31Resistance(source, owner, missile, 1, 1); lowRows.Add(Astral31ResistanceRow(low));
                    }
                    var high = Astral31Resistance(source, owner, missile, 50, 1);
                    check("immunity-sr-" + key, descriptors.All(row => (bool)row["exact"]) && low.IsSpellResisted &&
                        low.SpellResistance == (kyton ? 17 : 16) && high.SpellResistance == low.SpellResistance && !high.IsSpellResisted,
                        new JObject { ["owner"] = owner.UniqueId, ["descriptors"] = descriptors, ["lowClAttempts"] = lowRows,
                            ["highCl"] = Astral31ResistanceRow(high), ["directConsumerControlledCl"] = true },
                        "Native buff immunity and SR consumers, including Kyton poison susceptibility; controlled CL probes are not spell casts.");
                    if (kyton) {
                        int ac = owner.Stats.AC.ModifiedValue;
                        var fact = owner.Descriptor.GetFact(owner.Blueprint.AddFacts.First(value => value.name.EndsWith("_Traits", StringComparison.Ordinal)));
                        var lower = owner.Stats.AC.AddModifier(2, fact, "S34 armor stacking control", ModifierDescriptor.Armor);
                        int withLower;
                        try { withLower = owner.Stats.AC.ModifiedValue; } finally { owner.Stats.AC.RemoveModifier(lower); }
                        var higher = owner.Stats.AC.AddModifier(6, fact, "S34 armor stacking control", ModifierDescriptor.Armor);
                        int withHigher;
                        try { withHigher = owner.Stats.AC.ModifiedValue; } finally { owner.Stats.AC.RemoveModifier(higher); }
                        check("kyton-typed-armor", withLower == ac && withHigher == ac + 2 && owner.Stats.AC.ModifiedValue == ac,
                            new JObject { ["owner"] = owner.UniqueId, ["baseAc"] = ac, ["withArmor2"] = withLower,
                                ["withArmor6"] = withHigher, ["restoredAc"] = owner.Stats.AC.ModifiedValue },
                            "The +4 chain armor participates in native armor stacking without a wearable armor item.");
                    }
                }
                var barbazu = owners[0]; var kytonOwner = owners[1];
                foreach (int frame in ReviewInfernalGaze34(fixture, kytonOwner, barbazu, trace, check)) yield return frame;
                foreach (int frame in ReviewInfernalRegeneration34(fixture, kytonOwner, trace, check)) yield return frame;
                check("native-donor-isolation", donor.ComponentsArray.SequenceEqual(donorComponents) && donor.AddFacts.SequenceEqual(donorFacts),
                    new JObject { ["donor"] = donor.AssetGuid, ["componentsExact"] = donor.ComponentsArray.SequenceEqual(donorComponents),
                        ["factsExact"] = donor.AddFacts.SequenceEqual(donorFacts) }, "Native donor component and fact bindings remain untouched.");
            } finally {
                trace.Dispose();
                DisposeExpandedSummoningUnits(fixture.Created, owners.ToArray());
                foreach (var value in temporary) UnityEngine.Object.DestroyImmediate(value);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "sprint34-infernal-mechanics.json"), receipt.ToString(Formatting.Indented));
            }
        }

        private IEnumerable<int> ReviewInfernalGaze34(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            UnitEntityData ally, InfernalEvents34 trace, Action<string, bool, JObject, string> check)
        {
            var blueprint = InfernalSavedBlueprint34<BlueprintAbility>(true, "UnnervingGaze");
            var shaken = InfernalSavedBlueprint34<BlueprintBuff>(true, "Shaken");
            var ability = new AbilityData(owner.Descriptor.Abilities.GetAbility(blueprint));
            int charisma = owner.Stats.Charisma.BaseValue, dc = ability.CalculateParams().DC;
            int increased;
            try { owner.Stats.Charisma.BaseValue += 4; increased = new AbilityData(owner.Descriptor.Abilities.GetAbility(blueprint)).CalculateParams().DC; }
            finally { owner.Stats.Charisma.BaseValue = charisma; }
            check("gaze-live-dc", dc == 15 && increased == dc + 2 && new AbilityData(owner.Descriptor.Abilities.GetAbility(blueprint)).CalculateParams().DC == dc,
                new JObject { ["owner"] = owner.UniqueId, ["baseDc"] = dc, ["charismaPlus4Dc"] = increased },
                "Native ability parameter calculation uses the live Charisma modifier.");
            bool saved = false, failed = false, expiry = false;
            foreach (var target in new[] { fixture.Hostile, ally }) {
                Vector3 end; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, fixture.Hostile, 6f, out end, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, origin + (end-origin).normalized * new Feet(20).Meters);
                foreach (var unit in new[] { owner, target }) {
                    unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    unit.SightCache.ClearIfNeeded();
                }
                if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
                if (target.HasLOS(owner)) target.Memory.Add(owner).Visible = true;
                var eligibility = new UnitUseAbility(ability, new TargetWrapper(target)); eligibility.Init(owner);
                bool eligible = ability.CanTarget(new TargetWrapper(target)) && eligibility.IsUnitEnoughClose;
                check("gaze-target-" + (target == ally ? "ally" : "enemy"), eligible,
                    new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["enemy"] = owner.IsEnemy(target),
                        ["ability"] = blueprint.AssetGuid, ["distanceMeters"] = owner.DistanceTo(target), ["canTarget"] = ability.CanTarget(new TargetWrapper(target)),
                        ["nativeAbilityEnoughClose"] = eligibility.IsUnitEnoughClose, ["lane"] = lane },
                    "Native gaze targeting and ability range admit a visible target; no melee-distance probe.");
                if (target != ally) {
                    if (target.Descriptor.State.HasCondition(UnitCondition.Blindness))
                        throw new InvalidOperationException("Gaze sight control already blind.");
                    bool blocked;
                    try { target.Descriptor.State.AddCondition(UnitCondition.Blindness); blocked = !ability.CanTarget(new TargetWrapper(target)); }
                    finally { target.Descriptor.State.RemoveCondition(UnitCondition.Blindness); }
                    check("gaze-sight", blocked && ability.CanTarget(new TargetWrapper(target)),
                        new JObject { ["source"] = owner.UniqueId, ["target"] = target.UniqueId, ["blindRecipientRefused"] = blocked,
                            ["restoredSightAdmitted"] = ability.CanTarget(new TargetWrapper(target)) },
                        "Native blindness prevents receiving the gaze, and removing only the fixture condition restores eligibility.");
                }
                int maximum = target == ally ? 1 : 8;
                for (int attempt = 0; attempt < maximum && (target == ally || !(saved && failed)); attempt++) {
                    foreach (var buff in target.Buffs.Enumerable.Where(value => value.Blueprint == shaken && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray()) buff.Remove();
                    int start = trace.Rows.Count; double began = Game.Instance.TimeController.GameTime.TotalSeconds;
                    var command = new UnitUseAbility(new AbilityData(owner.Descriptor.Abilities.GetAbility(blueprint)), new TargetWrapper(target));
                    var costRows = new JArray();
                    using (var costs = new FirearmReviewCosts(owner, costRows)) {
                        owner.Commands.Run(command);
                        foreach (int frame in WaitInfernalCommand34(command, () => trace.Rows.Skip(start).Any(row => (string)row["event"] == "save"), 22)) yield return frame;
                        var saves = trace.Rows.Skip(start).Where(row => (string)row["event"] == "save").ToArray();
                        var owned = target.Buffs.Enumerable.Where(value => value.Blueprint == shaken && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
                        bool passed = saves.Length == 1 && (bool)saves[0]["passed"];
                        var duration = owned.Length == 1 ? owned[0].EndTime.TotalSeconds - began : 0;
                        bool exact = command.Result == UnitCommand.ResultType.Success && command.IsActed && saves.Length == 1 &&
                            (string)saves[0]["target"] == target.UniqueId && (int)saves[0]["dc"] == 15 &&
                            owned.Length == (passed ? 0 : 1) && (passed || !owned[0].IsPermanent && duration >= 5 && duration <= 22) &&
                            ability.RuntimeActionType == UnitCommand.CommandType.Standard && costs.Count(command) == 1 &&
                            (costs.InCombat(command) ? costs.Exact(command, false) : costs.Unchanged(command));
                        check("gaze-command-" + (target == ally ? "ally" : "enemy") + "-" + attempt, exact,
                            new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["ability"] = blueprint.AssetGuid,
                                ["command"] = Sprint27AiCommand(command), ["saves"] = new JArray(saves), ["costs"] = costRows,
                                ["ownedEffects"] = new JArray(owned.Select(InfernalSavedEffect34)), ["durationFromCommandStart"] = duration },
                            "Actual standard-action gaze has a real Will save and exactly its owned finite effect on failure; bounded attempts preserve natural outcomes.");
                        saved |= passed; failed |= !passed;
                        if (!passed && !expiry) {
                            var buff = owned[0]; DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                            while (DateTime.UtcNow < deadline && !buff.IsDisposed) { Game.Instance.IsPaused = false; yield return 0; }
                            expiry = buff.IsDisposed && !target.Buffs.Enumerable.Contains(buff);
                            check("gaze-finite-expiry", expiry, new JObject { ["source"] = owner.UniqueId, ["target"] = target.UniqueId,
                                ["endSeconds"] = buff.EndTime.TotalSeconds, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                                ["disposed"] = buff.IsDisposed }, "Native clock expires the owned shaken effect without deleting a different source.");
                        }
                    }
                }
            }
            foreach (var descriptor in new[] { SpellDescriptor.Fear, SpellDescriptor.MindAffecting }) {
                var immunity = ScriptableObject.CreateInstance<BuffDescriptorImmunity>(); immunity.Descriptor = descriptor;
                var feature = ScriptableObject.CreateInstance<BlueprintFeature>(); feature.name = "KMG_Runtime_S34_Gaze_" + descriptor;
                feature.Ranks = 1; feature.ComponentsArray = new BlueprintComponent[] { immunity };
                var target = fixture.Hostile;
                try {
                    target.Descriptor.AddFact(feature);
                    var context = new AbilityExecutionContext(ability, ability.CalculateParams(), new TargetWrapper(target), null);
                    var rule = Rulebook.Trigger(new RuleApplyBuff(target, shaken, context, TimeSpan.FromSeconds(6),
                        (buff, parent, duration) => target.Buffs.AddBuff(buff, parent, duration)));
                    check("gaze-immunity-" + descriptor, rule.Immunity && rule.AppliedBuff == null,
                        new JObject { ["source"] = owner.UniqueId, ["target"] = target.UniqueId, ["ability"] = blueprint.AssetGuid,
                            ["descriptor"] = descriptor.ToString(), ["immune"] = rule.Immunity, ["applied"] = rule.AppliedBuff != null,
                            ["effectConsumerOnlyNoCastOrSaveClaim"] = true },
                        "Each native descriptor immunity independently rejects the actual registered gaze effect; this isolates its buff consumer.");
                } finally { target.Descriptor.RemoveFact(feature); UnityEngine.Object.DestroyImmediate(feature); }
            }
            check("gaze-natural-outcomes", saved && failed && expiry, new JObject { ["saved"] = saved, ["failed"] = failed,
                ["expired"] = expiry, ["forcedRolls"] = 0, ["maximumEnemyAttempts"] = 8 },
                "Both natural save outcomes and finite expiry were observed; a missing precondition is not a gameplay PASS.");
        }

        private IEnumerable<int> ReviewInfernalRegeneration34(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, InfernalEvents34 trace, Action<string, bool, JObject, string> check)
        {
            // Positive native damage, immediate death-prevention flag and later
            // actual healing are distinct observations. No direct OnNewRound call.
            var regeneration = owner.Buffs.GetBuff(InfernalSavedBlueprint34<BlueprintBuff>(true, "Regeneration"))
                .SelectComponents<AddEffectRegeneration>().Single();
            bool halved = (bool)ReadExpandedSummoningOptionalMember(regeneration, "IsHalved");
            int expectedHeal = halved ? 1 : 2;
            foreach (string bypass in new[] { "mundane", "good", "silver" }) {
                DateTime reset = DateTime.UtcNow.AddSeconds(22);
                while (DateTime.UtcNow < reset && !(bool)owner.Descriptor.State.IsRegenerate) { Game.Instance.IsPaused = false; yield return 0; }
                if (!(bool)owner.Descriptor.State.IsRegenerate) throw new InvalidOperationException("Native regeneration did not reactivate before " + bypass);
                var packet = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing); packet.AddBonus(20);
                if (bypass == "good") packet.AddAlignment(DamageAlignment.Good);
                if (bypass == "silver") packet.AddMaterial(PhysicalDamageMaterial.Silver);
                int before = owner.Damage;
                var damage = Rulebook.Trigger(new RuleDealDamage(fixture.Hostile, owner, new DamageBundle(packet)));
                bool active = owner.Descriptor.State.IsRegenerate;
                int after = owner.Damage, start = trace.Rows.Count;
                double time = Game.Instance.TimeController.GameTime.TotalSeconds;
                DateTime deadline = DateTime.UtcNow.AddSeconds(22);
                while (DateTime.UtcNow < deadline && !trace.Rows.Skip(start).Any(row =>
                    (string)row["event"] == "heal" && (string)row["target"] == owner.UniqueId)) { Game.Instance.IsPaused = false; yield return 0; }
                var heals = trace.Rows.Skip(start).Where(row => (string)row["event"] == "heal" && (string)row["target"] == owner.UniqueId).ToArray();
                bool exact = damage.Damage > 0 && after - before == damage.Damage && active == (bypass == "mundane") &&
                    heals.Length > 0 && heals.All(row => (int)row["actual"] == expectedHeal) && owner.Damage < after &&
                    (bool)owner.Descriptor.State.IsRegenerate;
                check("kyton-regeneration-" + bypass, exact, new JObject { ["owner"] = owner.UniqueId, ["source"] = fixture.Hostile.UniqueId,
                    ["expectedHeal"] = expectedHeal, ["nativeDifficultyHalved"] = halved,
                    ["nativeDamage"] = damage.Damage, ["damageBefore"] = before, ["damageAfter"] = after, ["regenerationImmediatelyActive"] = active,
                    ["heals"] = new JArray(heals), ["elapsedGameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds - time,
                    ["damageAfterNativeHealing"] = owner.Damage, ["nativeDeathPreventionActive"] = (bool)owner.Descriptor.State.IsRegenerate,
                    ["directDamageConsumerNotAttackDispatch"] = true },
                    "Native regeneration preserves death prevention on ordinary injury, suppresses it on good/silver, and later executes native healing.");
            }
        }
    }
}
