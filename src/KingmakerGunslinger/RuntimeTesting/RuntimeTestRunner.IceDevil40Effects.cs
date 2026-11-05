using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilEffects40(ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>(); var trace = new InfernalEvents34();
            var cells = new JArray();
            var receipt = new JObject { ["cells"] = cells, ["events"] = trace.Rows,
                ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilEffectsPhase };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 effects: " + id);
            };
            var target = fixture.Hostile; int will = target.Stats.SaveWill.BaseValue;
            try
            {
                var owner = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                owned.Add(owner); Sprint28SetBrainActivePreservingActions(owner, false);
                foreach (int frame in Outsider30NativeAppearance(owner, owner.Blueprint.name)) yield return frame;
                string p = ExpandedSummoningSpecialBuilder.IceDevil40Prefix;
                var regenBp = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[p + "Regeneration"];
                var regen = owner.Buffs.GetBuff(regenBp)?.SelectComponents<KytonRegeneration34>().SingleOrDefault();
                check("regeneration-exact-consumer", regen != null && regen.Heal == 5 && regen.GoodOnly &&
                    ReferenceEquals(regen.OwningBlueprint, owner.Blueprint),
                    new JObject { ["owner"] = owner.UniqueId, ["nativeRegenerating"] =
                        (bool)owner.Descriptor.State.IsRegenerate, ["heal"] = regen?.Heal },
                    "Exact Ice owner uses the qualified native regeneration callback carrier with its own five-point, good-only parameters.");
                foreach (string bypass in new[] { "mundane", "silver", "good" })
                {
                    foreach (int frame in InfernalBoneWaitUntil35(() =>
                        (bool)owner.Descriptor.State.IsRegenerate, 24)) yield return frame;
                    var packet = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing);
                    packet.AddBonus(20);
                    if (bypass == "silver") packet.AddMaterial(PhysicalDamageMaterial.Silver);
                    if (bypass == "good") packet.AddAlignment(DamageAlignment.Good);
                    int before = owner.Damage;
                    var damage = Rulebook.Trigger(new RuleDealDamage(target, owner, new DamageBundle(packet)));
                    bool active = owner.Descriptor.State.IsRegenerate;
                    var row = new JObject { ["nativeDamage"] = damage.Damage,
                        ["before"] = before, ["after"] = owner.Damage, ["active"] = active,
                        ["directNativeDamageConsumerNotAttack"] = true };
                    check("regeneration-qualifier-" + bypass, damage.Damage > 0 &&
                        active == (bypass != "good"), row,
                        "Positive native injury suppresses regeneration only for good damage; silver does not import Kyton's qualifier.");
                    foreach (int frame in ObserveInfernalRegenerationRecovery34(owner,
                        trace, check, bypass, bypass == "good", row,
                        selectedRegeneration: regen, assertionOwner: "ice-devil")) yield return frame;
                }
                foreach (DamageEnergyType energy in new[] { DamageEnergyType.Fire,
                    DamageEnergyType.Cold, DamageEnergyType.Acid, DamageEnergyType.Electricity })
                {
                    var packet = new EnergyDamage(new DiceFormula(0, DiceType.Zero), energy);
                    packet.AddBonus(20);
                    bool immune = energy == DamageEnergyType.Fire || energy == DamageEnergyType.Cold;
                    var row = Astral31Damage(owner, target, packet,
                        energy == DamageEnergyType.Acid ? 10 : 20, immune, energy.ToString());
                    check("native-energy-" + energy, (bool)row["exact"], row,
                        "The actual native damage consumer grants fire/cold immunity and acid10, without fire vulnerability or donor electrical protection.");
                }

                // The recipient begins outside the aura. Native exposure and
                // source-specific repeat memory, not an observer apply, own it.
                target.Stats.SaveWill.BaseValue = -100;
                JObject hostility = Nalfeshnee39EstablishHostility(fixture, owner, target);
                check("fear-recipient-hostility", owner.IsEnemy(target) && target.IsEnemy(owner), hostility,
                    "Native reciprocal hostility on the exact private recipient.");
                var fearBp = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[p + "FearFrightened"];
                var memoryBp = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[p + "FearImmunity"];
                int start = trace.Rows.Count;
                PlaceExpandedSummoningUnit(target, owner.Position + Vector3.right * 2.5f);
                foreach (int frame in InfernalBoneWaitUntil35(() => trace.Rows.Skip(start).Any(x =>
                    (string)x["event"] == "save" && (string)x["owner"] == owner.UniqueId &&
                    (string)x["target"] == target.UniqueId && (int?)x["dc"] == 22), 12)) yield return frame;
                var first = trace.Rows.Skip(start).Where(x => (string)x["event"] == "save" &&
                    (string)x["owner"] == owner.UniqueId && (string)x["target"] == target.UniqueId &&
                    (int?)x["dc"] == 22).ToArray();
                var memory = InfernalBoneBuff35(target, memoryBp, owner);
                var frightened = InfernalBoneBuff35(target, fearBp, owner);
                bool observedFailure = first.Any(x => !(bool)x["passed"]) &&
                    frightened != null && target.Descriptor.State.HasCondition(UnitCondition.Frightened);
                check("fear-native-exposure", first.Length == 1 && memory != null &&
                    ((bool)first[0]["passed"] || frightened != null &&
                        target.Descriptor.State.HasCondition(UnitCondition.Frightened)),
                    new JObject { ["saves"] = new JArray(first), ["memorySource"] = memory?.Context?.MaybeCaster?.UniqueId,
                        ["frightened"] = target.Descriptor.State.HasCondition(UnitCondition.Frightened),
                        ["end"] = frightened?.EndTime.ToString(), ["forcedRolls"] = 0 },
                    "One native Will DC22 exposure installs source-specific memory and, on an actual failure, the native frightened condition.");
                var second = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                owned.Add(second); Sprint28SetBrainActivePreservingActions(second, false);
                PlaceExpandedSummoningUnit(second, owner.Position + Vector3.forward * 15f);
                foreach (int frame in Outsider30NativeAppearance(second, second.Blueprint.name)) yield return frame;
                second.Stats.Charisma.BaseValue += 4;
                int secondStart = trace.Rows.Count;
                // A real failed first save leaves the recipient frightened and
                // moving while this second body's appearance settles. Follow
                // that current recipient position so the independent-source
                // check observes the second aura instead of an obsolete point.
                PlaceExpandedSummoningUnit(second, target.Position);
                foreach (int frame in InfernalBoneWaitUntil35(() => trace.Rows.Skip(secondStart).Any(x =>
                    (string)x["event"] == "save" && (string)x["owner"] == second.UniqueId &&
                    (string)x["target"] == target.UniqueId && (int?)x["dc"] == 24), 12)) yield return frame;
                var secondRows = trace.Rows.Skip(secondStart).Where(x => (string)x["event"] == "save" &&
                    (string)x["owner"] == second.UniqueId && (string)x["target"] == target.UniqueId &&
                    (int?)x["dc"] == 24).ToArray();
                bool secondApplied = InfernalBoneBuff35(target, fearBp, second) != null &&
                    target.Descriptor.State.HasCondition(UnitCondition.Frightened);
                check("fear-live-dc-and-independent-source", secondRows.Length == 1 &&
                    InfernalBoneBuff35(target, memoryBp, second) != null &&
                    (observedFailure || secondRows.Any(x => !(bool)x["passed"]) && secondApplied),
                    new JObject { ["firstSource"] = owner.UniqueId, ["secondSource"] = second.UniqueId,
                        ["secondSaves"] = new JArray(secondRows), ["liveCharisma"] = second.Stats.Charisma.ModifiedValue,
                        ["firstFailureActuallyApplied"] = observedFailure,
                        ["secondNativeCondition"] = secondApplied, ["forcedRolls"] = 0 },
                    "Second source ignores the first source's repeat memory; live +4 Charisma changes DC22 to24. At least one real failed save must actually apply native fear.");
                foreach (int frame in ReviewInfernalRegenerationSpellAndDeath34(fixture,
                    owner, trace, check, recipientKey: IceDevilVisualPolicy40.Key,
                    assertionOwner: "ice-devil", getRegeneration: unit =>
                        unit.Buffs.GetBuff(regenBp).SelectComponents<KytonRegeneration34>().Single(),
                    isolateFear: true)) yield return frame;
            }
            finally
            {
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, owned);
                target.Stats.SaveWill.BaseValue = will; ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-effects.json"), receipt.ToString(Formatting.Indented));
            }
        }
    }
}
