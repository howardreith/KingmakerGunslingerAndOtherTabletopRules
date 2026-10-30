using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // This is an explicit effect-only precondition for lifecycle consumers,
        // never evidence of attack, injury, save, action cost or public casting.
        private static Buff InfernalOwnedEffect34(UnitEntityData source, UnitEntityData recipient, BlueprintBuff blueprint, TimeSpan? duration = null)
        {
            var context = AstralVictimEffectContext.Create(source, recipient, blueprint,
                new AbilityParams { DC = 17, CasterLevel = 8, SpellLevel = 0 });
            var apply = Rulebook.Trigger(new RuleApplyBuff(recipient, blueprint, context, duration,
                (fact, parent, time) => recipient.Buffs.AddBuff(fact, parent, time)));
            if (apply.AppliedBuff == null || !apply.AppliedBuff.Active || apply.AppliedBuff.IsDisposed ||
                !ReferenceEquals(apply.AppliedBuff.Context?.MaybeCaster, source))
                throw new InvalidOperationException("Native owned effect precondition failed: " + blueprint.name);
            return apply.AppliedBuff;
        }

        private IEnumerable<int> ReviewInfernalEffects34(ExpandedSummoningCorrectionFixture fixture)
        {
            var owners = new List<UnitEntityData>(); var trace = new InfernalEvents34(); var cells = new JArray();
            var receipt = new JObject { ["phase"] = "sprint34-infernal-effects", ["cells"] = cells, ["events"] = trace.Rows };
            Action<string, bool, JObject, string> check = (id, pass, row, description) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint34-" + id, pass, row, description);
                if (!pass) throw new InvalidOperationException("Sprint34 effects: " + id);
            };
            Buff unrelated = null;
            try {
                var source = CastExpandedSummoningOwnTier(fixture, InfernalVisualPolicy34.Barbazu); owners.Add(source);
                var patient = CastExpandedSummoningOwnTier(fixture, InfernalVisualPolicy34.Barbazu); owners.Add(patient);
                var healer = CastExpandedSummoningOwnTier(fixture, "lillend-azata"); owners.Add(healer);
                foreach (var actor in owners) {
                    Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(pending => true);
                    foreach (int frame in WaitInfernalSupport34(actor)) yield return frame;
                }
                var target = fixture.Hostile; var woundBlueprint = InfernalSavedBlueprint34<BlueprintBuff>(false, "InfernalWound");
                Buff wound = null;
                for (int attempt = 0; attempt < 4 && wound == null; attempt++) {
                    InfernalCombatLane34(fixture, source, target, 8);
                    var attack = new UnitAttack(target) { IsSingleAttack = true }; source.Commands.Run(attack);
                    foreach (int frame in WaitInfernalCommand34(attack, () => true, 22)) yield return frame;
                    wound = target.Buffs.Enumerable.SingleOrDefault(buff => buff.Blueprint == woundBlueprint && ReferenceEquals(buff.Context?.MaybeCaster, source));
                }
                if (wound == null) throw new InvalidOperationException("Wound injury precondition unobserved in four real attacks.");
                source.Commands.InterruptAll(pending => true);
                var ordinary = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.AssetGuid == "16249b8075ab8684ca105a78a047a5ef");
                unrelated = InfernalOwnedEffect34(healer, target, ordinary);
                var injury = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing);
                injury.AddBonus(20); injury.AddAlignment(DamageAlignment.Good);
                var damage = Rulebook.Trigger(new RuleDealDamage(target, patient, new DamageBundle(injury)));
                if (damage.Damage <= 0 || patient.Descriptor.State.IsDead) throw new InvalidOperationException("Unrelated patient's native injury precondition failed.");
                var book = healer.Descriptor.GetSpellbook(healer.Blueprint.GetComponent<LillendBardBook32>().Book);
                var cure = Enumerable.Range(1, 3).Where(level => book.GetSpontaneousSlots(level) > 0).SelectMany(level => book.GetKnownSpells(level))
                    .First(value => value.Blueprint.AssetGuid == "5590652e1c2225c4ca30c4a699ab3649" || value.Blueprint.AssetGuid == "3361c5df793b4c8448756146a88026ad");
                PlaceExpandedSummoningUnit(healer, patient.Position + UnityEngine.Vector3.right * (healer.Corpulence + patient.Corpulence + .3f));
                int first = trace.Rows.Count, before = patient.Damage, levelUsed = book.GetSpellLevel(cure.Blueprint), slots = book.GetSpontaneousSlots(book.GetSpellLevel(cure.Blueprint));
                var command = new UnitUseAbility(cure, new Kingmaker.Utility.TargetWrapper(patient)); var costs = new JArray();
                using (var economy = new FirearmReviewCosts(healer, costs)) {
                    healer.Commands.Run(command);
                    foreach (int frame in WaitInfernalCommand34(command, () => trace.Rows.Skip(first).Any(row =>
                        (string)row["event"] == "heal" && (string)row["target"] == patient.UniqueId), 25)) yield return frame;
                    var heals = trace.Rows.Skip(first).Where(row => (string)row["event"] == "heal").ToArray();
                    check("wound-unrelated-patient", command.Result == UnitCommand.ResultType.Success && economy.Count(command) == 1 &&
                        (economy.InCombat(command) ? economy.Exact(command, false) : economy.Unchanged(command)) &&
                        patient.Damage < before && book.GetSpontaneousSlots(levelUsed) == slots - 1 && heals.Length == 1 &&
                        (string)heals[0]["target"] == patient.UniqueId && heals[0]["woundCheck"] == null && !wound.IsDisposed && !unrelated.IsDisposed,
                        new JObject { ["woundSource"] = source.UniqueId, ["woundedPatient"] = target.UniqueId, ["otherPatient"] = patient.UniqueId,
                            ["command"] = Sprint27AiCommand(command), ["costs"] = costs, ["events"] = new JArray(trace.Rows.Skip(first)),
                            ["damageBefore"] = before, ["damageAfter"] = patient.Damage, ["slotsBefore"] = slots, ["slotsAfter"] = book.GetSpontaneousSlots(levelUsed) },
                        "A real healing spell on another injured patient uses its normal slot/cost and restores HP without an Infernal Wound check or removing the first patient's effects.");
                }
                var disease = InfernalOwnedEffect34(source, target, InfernalSavedBlueprint34<BlueprintBuff>(false, "DevilChills"));
                var exactOrdinary = InfernalSavedEffect34(unrelated);
                var expiry = ExpireSprint16OwnedBuff(source, source.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                DateTime deadline = DateTime.UtcNow.AddSeconds(18);
                while (DateTime.UtcNow < deadline && (!wound.IsDisposed || !disease.IsDisposed || !Sprints3031SourceAbsent(source))) {
                    Game.Instance.IsPaused = false; yield return 0;
                }
                check("wound-source-preserves-unrelated-bleed", wound.IsDisposed && disease.IsDisposed && Sprints3031SourceAbsent(source) &&
                    !unrelated.IsDisposed && unrelated.IsTurnedOn && target.Buffs.Enumerable.Contains(unrelated) &&
                    JToken.DeepEquals(exactOrdinary, InfernalSavedEffect34(unrelated)),
                    new JObject { ["source"] = source.UniqueId, ["expiry"] = expiry, ["woundDisposed"] = wound.IsDisposed,
                        ["diseaseDisposed"] = disease.IsDisposed, ["unrelated"] = InfernalSavedEffect34(unrelated),
                        ["effectOnlyDiseaseAndOrdinaryBleedPreconditions"] = true },
                    "Native summon expiry removes its exact wound and disease while preserving the other source's same native bleed instance, context and lifetime.");
                first = trace.Rows.Count; before = target.Damage;
                double until = Game.Instance.TimeController.GameTime.TotalSeconds + 6.1; deadline = DateTime.UtcNow.AddSeconds(15);
                while (DateTime.UtcNow < deadline && Game.Instance.TimeController.GameTime.TotalSeconds < until) { Game.Instance.IsPaused = false; yield return 0; }
                var ticks = trace.Rows.Skip(first).Where(row => (string)row["event"] == "damage" && (string)row["owner"] == healer.UniqueId && (string)row["target"] == target.UniqueId && (int?)row["actual"] > 0).ToArray();
                check("unrelated-native-bleed-consumer", !unrelated.IsDisposed && ticks.Length > 0 && ticks.All(row =>
                    ((JArray)row["packets"]).Any(packet => (string)packet["dice"] == "2d6")) && target.Damage - before == ticks.Sum(row => (int)row["actual"]),
                    new JObject { ["patient"] = target.UniqueId, ["ordinarySource"] = healer.UniqueId, ["nativeBleed"] = ordinary.AssetGuid,
                        ["damageBefore"] = before, ["damageAfter"] = target.Damage, ["ticks"] = new JArray(ticks),
                        ["components"] = new JArray(ordinary.ComponentsArray.Select(component => component.GetType().FullName)) },
                    "The retained unrelated native bleed still causes actual HP loss through its original installed consumer after source retirement; no synthetic HP write or observer round call.");
            } finally {
                trace.Dispose(); if (unrelated != null && !unrelated.IsDisposed) unrelated.Remove();
                DisposeExpandedSummoningUnits(fixture.Created, owners); ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "sprint34-infernal-effects.json"), receipt.ToString(Formatting.Indented));
            }
        }
    }
}
