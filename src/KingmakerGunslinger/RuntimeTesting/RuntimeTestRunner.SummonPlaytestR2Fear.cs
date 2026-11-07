using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R2 LIFE-02: one save-free observation in the static interior.
        //  1. Ice Devil alone with an eligible hostile, then the reported
        //     Ice Devil/Nalfeshnee pair (allied), stationary, in real time.
        //     Every RuleSavingThrow and RuleApplyBuff on the observed units is
        //     recorded with its source; the shared aura is called every tick,
        //     so the count over a fixed window is the cadence.
        //  2. Exit and re-entry of both targets.
        //  3. The cause on these same bytes: the old marker construction (the
        //     memory buff added under the Frightened context) is offered to the
        //     Nalfeshnee and its native refusal recorded, with the descriptors
        //     involved and the Unholy Aura immunity components.
        //  4. Bone Devil regression: the same shared aura on a 5-foot source,
        //     a distinct source for the same hostile and the same Nalfeshnee.
        // Brains are inactive; nobody acts, casts or attacks.
        private const double SummonPlaytestR2WindowGameSeconds = 4.0;
        private const int SummonPlaytestR2WindowFrames = 600;

        private sealed class SummonPlaytestR2Rules : IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly Func<UnitEntityData, bool> _observed;
            internal readonly List<JObject> Saves = new List<JObject>();
            internal readonly List<JObject> Applications = new List<JObject>();
            internal SummonPlaytestR2Rules(Func<UnitEntityData, bool> observed) { _observed = observed; }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (evt?.Initiator == null || !_observed(evt.Initiator)) return;
                MechanicsContext context = evt.Reason?.Context;
                Saves.Add(new JObject {
                    ["target"] = evt.Initiator.UniqueId, ["targetBlueprint"] = evt.Initiator.Blueprint?.name,
                    ["source"] = context?.MaybeCaster?.UniqueId, ["sourceBlueprint"] = context?.MaybeCaster?.Blueprint?.name,
                    ["effect"] = context?.AssociatedBlueprint?.name, ["effectGuid"] = context?.AssociatedBlueprint?.AssetGuid,
                    ["descriptor"] = context == null ? null : context.SpellDescriptor.ToString(),
                    ["type"] = evt.Type.ToString(), ["dc"] = evt.DifficultyClass, ["roll"] = evt.D20.Value,
                    ["passed"] = evt.IsPassed, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                    ["frame"] = Time.frameCount });
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (evt?.Initiator == null || !_observed(evt.Initiator)) return;
                Applications.Add(new JObject {
                    ["target"] = evt.Initiator.UniqueId, ["buff"] = evt.Blueprint?.name, ["buffGuid"] = evt.Blueprint?.AssetGuid,
                    ["source"] = evt.Context?.MaybeCaster?.UniqueId,
                    ["descriptor"] = evt.Context == null ? null : evt.Context.SpellDescriptor.ToString(),
                    ["durationSeconds"] = evt.Duration?.TotalSeconds, ["immunity"] = evt.Immunity,
                    ["applied"] = evt.AppliedBuff != null, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds });
            }
        }

        private IEnumerable<int> ReviewSummonPlaytestFearR2(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestFearPhase)
                throw new InvalidOperationException("Exact fresh R2 fear fixture with the summoning module enabled required.");
            const string prefix = "r2-fear-";
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            Func<string, BlueprintScriptableObject> registered = symbol => symbols[symbol];
            string ice = ExpandedSummoningSpecialBuilder.IceDevil40Prefix, bone = ExpandedSummoningSpecialBuilder.Bone35Prefix;
            var iceMemory = (BlueprintBuff)registered(ice + "FearImmunity");
            var iceFrightened = (BlueprintBuff)registered(ice + "FearFrightened");
            var iceArea = (BlueprintAbilityAreaEffect)registered(ice + "FearArea");
            var boneMemory = (BlueprintBuff)registered(bone + "FearImmunity");
            var boneFrightened = (BlueprintBuff)registered(bone + "FearFrightened");
            var boneArea = (BlueprintAbilityAreaEffect)registered(bone + "FearArea");
            var unholy = (BlueprintBuff)registered("KMG.Summoning.Demon.Nalfeshnee.UnholyAura");
            UnitEntityData hostile = fixture.Hostile, caster = fixture.Caster;
            var owned = new List<UnitEntityData>();
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["limits"] = "Real-time static interior. Brains inactive, so no unit moves on its own; exit and re-entry are fixture placements. One hostile, one allied Nalfeshnee, Ice Devil then Bone Devil sources." };
            Vector3 centre = caster.Position;
            int hostileWill = hostile.Stats.SaveWill.BaseValue;
            UnitEntityData iceDevil = null, nalfeshnee = null, boneDevil = null;
            SummonPlaytestR2Rules rules = null;
            try
            {
                iceDevil = CastExpandedSummoningOwnTier(fixture, "ice-devil"); owned.Add(iceDevil);
                nalfeshnee = CastExpandedSummoningOwnTier(fixture, "nalfeshnee"); owned.Add(nalfeshnee);
                foreach (UnitEntityData unit in new[] { iceDevil, nalfeshnee, hostile })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                // A hostile that fails (only a natural 20 passes) shows legitimate fear still applies.
                hostile.Stats.SaveWill.BaseValue = -40;
                Vector3 site = ObstacleAnalyzer.GetNearestNode(centre + Vector3.forward * 8f).clampedPosition;
                Func<Vector3, Vector3> at = offset => ObstacleAnalyzer.GetNearestNode(site + offset).clampedPosition;
                Vector3 far = ObstacleAnalyzer.GetNearestNode(centre + Vector3.back * 14f).clampedPosition;
                Vector3 farther = ObstacleAnalyzer.GetNearestNode(centre + Vector3.back * 22f).clampedPosition;
                PlaceExpandedSummoningUnit(iceDevil, site);
                PlaceExpandedSummoningUnit(nalfeshnee, far);
                PlaceExpandedSummoningUnit(hostile, farther);
                for (int frame = 0; frame < 10; frame++) { Game.Instance.IsPaused = false; yield return 0; }

                Func<BlueprintAbilityAreaEffect, UnitEntityData, AreaEffectEntityData> areaOf = (blueprint, source) =>
                    Game.Instance.State.AreaEffects.All.FirstOrDefault(value => value != null && !value.IsEnded &&
                        ReferenceEquals(value.Blueprint, blueprint) && ReferenceEquals(value.Context?.MaybeCaster, source));
                Func<AreaEffectEntityData, UnitEntityData, bool> inside = (area, unit) =>
                    area != null && area.UnitsInside.Contains(unit);
                Func<UnitEntityData, BlueprintBuff, UnitEntityData, int> buffs = (unit, blueprint, source) =>
                    unit.Descriptor.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, blueprint) &&
                        ReferenceEquals(value.Context?.MaybeCaster, source));
                Func<UnitEntityData, BlueprintBuff, UnitEntityData, JObject> memoryRow = (unit, blueprint, source) =>
                {
                    var found = unit.Descriptor.Buffs.Enumerable.FirstOrDefault(value => ReferenceEquals(value.Blueprint, blueprint) &&
                        ReferenceEquals(value.Context?.MaybeCaster, source));
                    return new JObject { ["present"] = found != null,
                        ["hasSource"] = ContextActionArchonMenace.HasSource(unit, blueprint, source),
                        ["active"] = found != null && found.Active,
                        ["remainingSeconds"] = found == null ? (double?)null : (found.EndTime - Game.Instance.TimeController.GameTime).TotalSeconds,
                        ["notDispelable"] = found != null && found.IsNotDispelable,
                        ["descriptor"] = found?.Context == null ? null : found.Context.SpellDescriptor.ToString() };
                };
                AreaEffectEntityData iceAura = areaOf(iceArea, iceDevil);
                var observed = new HashSet<UnitEntityData> { nalfeshnee, hostile, caster, iceDevil };
                rules = new SummonPlaytestR2Rules(unit => observed.Contains(unit));
                EventBus.Subscribe(rules);
                Func<UnitEntityData, UnitEntityData, int> savesFrom = (target, source) =>
                    rules.Saves.Count(value => (string)value["target"] == target.UniqueId && (string)value["source"] == source.UniqueId);

                // Identities.
                var immunities = new JArray(nalfeshnee.Descriptor.Buffs.Enumerable.Concat<Kingmaker.Blueprints.Facts.Fact>(
                        nalfeshnee.Descriptor.Progression.Features.Enumerable)
                    .SelectMany(fact => fact.SelectComponents<BuffDescriptorImmunity>().Select(component => new JObject {
                        ["fact"] = fact.Blueprint.name, ["descriptor"] = ((SpellDescriptor)component.Descriptor).ToString(),
                        ["descriptorValue"] = (long)(SpellDescriptor)component.Descriptor, ["checkFact"] = component.CheckFact,
                        ["factToCheck"] = component.FactToCheck?.name, ["ignoreFeature"] = component.IgnoreFeature?.name })));
                Func<BlueprintBuff, string> descriptorOf = blueprint =>
                    ((SpellDescriptor)(blueprint.GetComponent<SpellDescriptorComponent>()?.Descriptor ??
                        (SpellDescriptorWrapper)SpellDescriptor.None)).ToString();
                record["identities"] = new JObject {
                    ["iceDevil"] = iceDevil.UniqueId, ["iceDevilBlueprint"] = iceDevil.Blueprint.name,
                    ["nalfeshnee"] = nalfeshnee.UniqueId, ["nalfeshneeBlueprint"] = nalfeshnee.Blueprint.name,
                    ["hostile"] = hostile.UniqueId, ["caster"] = caster.UniqueId,
                    ["iceArea"] = iceArea.name + ":" + iceArea.AssetGuid, ["iceAreaRadiusFeet"] = iceArea.Size.Value,
                    ["iceAreaAffectEnemies"] = iceArea.AffectEnemies, ["iceAreaLive"] = iceAura != null,
                    ["iceFrightened"] = iceFrightened.name + ":" + iceFrightened.AssetGuid, ["iceFrightenedDescriptor"] = descriptorOf(iceFrightened),
                    ["iceMemory"] = iceMemory.name + ":" + iceMemory.AssetGuid, ["iceMemoryDescriptor"] = descriptorOf(iceMemory),
                    ["boneArea"] = boneArea.name + ":" + boneArea.AssetGuid, ["boneMemory"] = boneMemory.name + ":" + boneMemory.AssetGuid,
                    ["unholyAura"] = unholy.name + ":" + unholy.AssetGuid,
                    ["nalfeshneeHasUnholyAura"] = nalfeshnee.Descriptor.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, unholy)),
                    ["nalfeshneeDescriptorImmunities"] = immunities,
                    ["relationships"] = new JObject { ["iceEnemyNalfeshnee"] = iceDevil.IsEnemy(nalfeshnee),
                        ["iceEnemyHostile"] = iceDevil.IsEnemy(hostile), ["iceEnemyCaster"] = iceDevil.IsEnemy(caster) } };
                Sprint25Check(prefix + "identities", iceAura != null && iceDevil.Blueprint.name == "KMG_Summoning_Unit_IceDevil" &&
                    nalfeshnee.Blueprint.name == "KMG_Summoning_Unit_Nalfeshnee" &&
                    nalfeshnee.Descriptor.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, unholy)) &&
                    !iceDevil.IsEnemy(nalfeshnee) && iceDevil.IsEnemy(hostile),
                    (JObject)record["identities"].DeepClone(),
                    "The live Ice Devil emits its own fear area; the summoned Nalfeshnee is its ally with Unholy Aura; the fixture hostile is its enemy.");

                // 1a. Ice Devil alone with the eligible hostile.
                PlaceExpandedSummoningUnit(hostile, at(Vector3.left * 2.6f));
                for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                bool hostileEntered = inside(iceAura, hostile), nalOutside = !inside(iceAura, nalfeshnee);
                TimeSpan start = Game.Instance.TimeController.GameTime;
                for (int frame = 0; frame < SummonPlaytestR2WindowFrames &&
                    (Game.Instance.TimeController.GameTime - start).TotalSeconds < SummonPlaytestR2WindowGameSeconds; frame++)
                { Game.Instance.IsPaused = false; yield return 0; }
                var alone = new JObject { ["hostileEntered"] = hostileEntered, ["nalfeshneeOutside"] = nalOutside, ["hostileInsideAtEnd"] = inside(iceAura, hostile),
                    ["hostileSaves"] = savesFrom(hostile, iceDevil), ["nalfeshneeSaves"] = savesFrom(nalfeshnee, iceDevil),
                    ["hostileFrightened"] = buffs(hostile, iceFrightened, iceDevil),
                    ["hostileMemory"] = memoryRow(hostile, iceMemory, iceDevil),
                    ["windowGameSeconds"] = (Game.Instance.TimeController.GameTime - start).TotalSeconds };
                record["iceAlone"] = alone;
                JObject hostileFirst = rules.Saves.FirstOrDefault(value => (string)value["target"] == hostile.UniqueId &&
                    (string)value["source"] == iceDevil.UniqueId);
                bool hostileFailed = hostileFirst != null && !hostileFirst.Value<bool>("passed");
                Sprint25Check(prefix + "ice-alone-hostile-once", hostileEntered && nalOutside &&
                    savesFrom(hostile, iceDevil) == 1 && memoryRow(hostile, iceMemory, iceDevil).Value<bool>("hasSource") &&
                    (!hostileFailed || buffs(hostile, iceFrightened, iceDevil) == 1),
                    (JObject)alone.DeepClone(),
                    "A stationary eligible enemy inside the Ice Devil aura saves exactly once; a failure applies the Ice Devil's Frightened; its exact-source memory is kept.");

                // 1b. The reported pair: the allied Nalfeshnee joins, stationary.
                PlaceExpandedSummoningUnit(nalfeshnee, at(Vector3.right * 2.6f));
                for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                bool nalEntered = inside(iceAura, nalfeshnee);
                start = Game.Instance.TimeController.GameTime;
                for (int frame = 0; frame < SummonPlaytestR2WindowFrames &&
                    (Game.Instance.TimeController.GameTime - start).TotalSeconds < SummonPlaytestR2WindowGameSeconds; frame++)
                { Game.Instance.IsPaused = false; yield return 0; }
                var pair = new JObject { ["nalfeshneeEntered"] = nalEntered, ["nalfeshneeInsideAtEnd"] = inside(iceAura, nalfeshnee), ["hostileInsideAtEnd"] = inside(iceAura, hostile),
                    ["nalfeshneeSaves"] = savesFrom(nalfeshnee, iceDevil), ["hostileSaves"] = savesFrom(hostile, iceDevil),
                    ["nalfeshneeMemory"] = memoryRow(nalfeshnee, iceMemory, iceDevil),
                    ["nalfeshneeFrightened"] = buffs(nalfeshnee, iceFrightened, iceDevil),
                    ["windowGameSeconds"] = (Game.Instance.TimeController.GameTime - start).TotalSeconds };
                record["icePair"] = pair;
                Sprint25Check(prefix + "pair-stationary-once", nalEntered && inside(iceAura, nalfeshnee) &&
                    savesFrom(nalfeshnee, iceDevil) == 1 && memoryRow(nalfeshnee, iceMemory, iceDevil).Value<bool>("hasSource") &&
                    savesFrom(hostile, iceDevil) == 1,
                    (JObject)pair.DeepClone(),
                    "The allied Nalfeshnee, stationary in the Ice Devil aura, is evaluated exactly once and keeps the exact-source memory; the hostile is still not re-evaluated.");

                // 2. Exit and re-entry.
                JObject nalMemoryBefore = memoryRow(nalfeshnee, iceMemory, iceDevil), hostileMemoryBefore = memoryRow(hostile, iceMemory, iceDevil);
                int nalBefore = savesFrom(nalfeshnee, iceDevil), hostileBefore = savesFrom(hostile, iceDevil);
                PlaceExpandedSummoningUnit(nalfeshnee, far); PlaceExpandedSummoningUnit(hostile, farther);
                for (int frame = 0; frame < 30; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                bool exited = !inside(iceAura, nalfeshnee) && !inside(iceAura, hostile);
                PlaceExpandedSummoningUnit(nalfeshnee, at(Vector3.right * 2.6f)); PlaceExpandedSummoningUnit(hostile, at(Vector3.left * 2.6f));
                for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                bool reentered = inside(iceAura, nalfeshnee) && inside(iceAura, hostile);
                for (int frame = 0; frame < 120; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                int nalExpected = nalMemoryBefore.Value<bool>("hasSource") ? 0 : 1;
                int hostileExpected = hostileMemoryBefore.Value<bool>("hasSource") ? 0 : 1;
                var reentry = new JObject { ["exited"] = exited, ["reentered"] = reentered,
                    ["nalfeshneeNewSaves"] = savesFrom(nalfeshnee, iceDevil) - nalBefore, ["nalfeshneeExpectedNew"] = nalExpected,
                    ["hostileNewSaves"] = savesFrom(hostile, iceDevil) - hostileBefore, ["hostileExpectedNew"] = hostileExpected,
                    ["nalfeshneeMemoryBefore"] = nalMemoryBefore, ["hostileMemoryBefore"] = hostileMemoryBefore };
                record["iceReentry"] = reentry;
                Sprint25Check(prefix + "exit-reentry", exited && reentered &&
                    savesFrom(nalfeshnee, iceDevil) - nalBefore == nalExpected && savesFrom(hostile, iceDevil) - hostileBefore == hostileExpected,
                    (JObject)reentry.DeepClone(),
                    "Leaving and re-entering does not reset the exact source's memory; a unit is re-evaluated only once its memory has ended.");

                // 3. The cause, on these bytes: the old marker construction.
                var oldEffect = new MechanicsContext(iceDevil, nalfeshnee.Descriptor, iceFrightened, iceAura?.Context,
                    new Kingmaker.Utility.TargetWrapper(nalfeshnee));
                int applicationsBefore = rules.Applications.Count;
                var oldMarker = nalfeshnee.Descriptor.Buffs.AddBuff(iceMemory, oldEffect, TimeSpan.FromSeconds(6));
                JObject oldApplication = rules.Applications.Skip(applicationsBefore).LastOrDefault();
                if (oldMarker != null) nalfeshnee.Descriptor.Buffs.RemoveFact(oldMarker);
                var newContextProbe = new MechanicsContext(iceDevil, nalfeshnee.Descriptor, iceMemory);
                var cause = new JObject { ["oldMarkerContextDescriptor"] = oldEffect.CloneFor(iceMemory, nalfeshnee.Descriptor).SpellDescriptor.ToString(),
                    ["oldMarkerApplied"] = oldMarker != null, ["oldMarkerApplication"] = oldApplication,
                    ["fixedMarkerContextDescriptor"] = newContextProbe.SpellDescriptor.ToString(),
                    ["fixedMarkerOnNalfeshnee"] = memoryRow(nalfeshnee, iceMemory, iceDevil) };
                record["cause"] = cause;
                Sprint25Check(prefix + "cause-old-marker-refused", oldMarker == null && oldApplication != null &&
                    oldApplication.Value<bool>("immunity") && memoryRow(nalfeshnee, iceMemory, iceDevil).Value<bool>("hasSource") &&
                    newContextProbe.SpellDescriptor == SpellDescriptor.None,
                    (JObject)cause.DeepClone(),
                    "On these bytes the pre-R2 marker (added under the Frightened context, inheriting its descriptors) is refused by the Nalfeshnee's descriptor immunity, while the R2 marker (own blueprint, no descriptor) is kept.");

                // 4. Bone Devil regression: distinct source, same targets.
                PlaceExpandedSummoningUnit(nalfeshnee, far); PlaceExpandedSummoningUnit(hostile, farther);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { iceDevil }); owned.Remove(iceDevil);
                for (int frame = 0; frame < 10; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                bool iceAreaEnded = areaOf(iceArea, iceDevil) == null;
                boneDevil = CastExpandedSummoningOwnTier(fixture, "bone-devil"); owned.Add(boneDevil);
                observed.Add(boneDevil);
                Sprint28SetBrainActivePreservingActions(boneDevil, false);
                boneDevil.Commands.InterruptAll(command => true); boneDevil.Commands.RemoveFinishedAndUpdateQueue();
                PlaceExpandedSummoningUnit(boneDevil, site);
                for (int frame = 0; frame < 10; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                AreaEffectEntityData boneAura = areaOf(boneArea, boneDevil);
                PlaceExpandedSummoningUnit(hostile, at(Vector3.left * 1.4f));
                PlaceExpandedSummoningUnit(nalfeshnee, at(Vector3.right * 1.9f));
                for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                bool boneHostileEntered = inside(boneAura, hostile), boneNalEntered = inside(boneAura, nalfeshnee);
                start = Game.Instance.TimeController.GameTime;
                for (int frame = 0; frame < SummonPlaytestR2WindowFrames &&
                    (Game.Instance.TimeController.GameTime - start).TotalSeconds < SummonPlaytestR2WindowGameSeconds; frame++)
                { Game.Instance.IsPaused = false; yield return 0; }
                var boneRow = new JObject { ["iceAreaEndedWithSource"] = iceAreaEnded, ["boneAreaLive"] = boneAura != null,
                    ["boneAreaRadiusFeet"] = boneArea.Size.Value,
                    ["hostileEntered"] = boneHostileEntered, ["nalfeshneeEntered"] = boneNalEntered,
                    ["hostileSaves"] = savesFrom(hostile, boneDevil), ["nalfeshneeSaves"] = savesFrom(nalfeshnee, boneDevil),
                    ["hostileMemory"] = memoryRow(hostile, boneMemory, boneDevil), ["nalfeshneeMemory"] = memoryRow(nalfeshnee, boneMemory, boneDevil),
                    ["hostileFrightened"] = buffs(hostile, boneFrightened, boneDevil),
                    ["iceMemoryStillOnHostile"] = memoryRow(hostile, iceMemory, iceDevil),
                    ["windowGameSeconds"] = (Game.Instance.TimeController.GameTime - start).TotalSeconds };
                record["boneDevil"] = boneRow;
                JObject boneHostileFirst = rules.Saves.FirstOrDefault(value => (string)value["target"] == hostile.UniqueId &&
                    (string)value["source"] == boneDevil.UniqueId);
                bool boneHostileFailed = boneHostileFirst != null && !boneHostileFirst.Value<bool>("passed");
                Sprint25Check(prefix + "bone-devil-regression", iceAreaEnded && boneAura != null &&
                    boneHostileEntered && boneNalEntered &&
                    savesFrom(hostile, boneDevil) == 1 && savesFrom(nalfeshnee, boneDevil) == 1 &&
                    memoryRow(hostile, boneMemory, boneDevil).Value<bool>("hasSource") &&
                    memoryRow(nalfeshnee, boneMemory, boneDevil).Value<bool>("hasSource") &&
                    (!boneHostileFailed || buffs(hostile, boneFrightened, boneDevil) == 1),
                    (JObject)boneRow.DeepClone(),
                    "The Bone Devil's own 5-foot aura evaluates the same hostile and Nalfeshnee once each despite the Ice Devil's memory (distinct source), applies its own Frightened on failure, and keeps its own memory; the Ice Devil's area ended with its source.");
                record["saves"] = new JArray(rules.Saves);
                record["applications"] = new JArray(rules.Applications);
            }
            finally
            {
                if (rules != null) EventBus.Unsubscribe(rules);
                if (record["saves"] == null && rules != null)
                { record["saves"] = new JArray(rules.Saves); record["applications"] = new JArray(rules.Applications); }
                foreach (UnitEntityData owner in owned.Where(value => value != null && !value.Destroyed))
                {
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    if (owner.IsInCombat) owner.LeaveCombat();
                }
                // Remove only this phase's fear and memory buffs from the persistent fixture units.
                var mine = new BlueprintBuff[] { iceMemory, iceFrightened, boneMemory, boneFrightened };
                foreach (UnitEntityData unit in new[] { hostile, caster }.Where(value => value != null && !value.Destroyed))
                    foreach (var buff in unit.Descriptor.Buffs.Enumerable.Where(value => mine.Contains(value.Blueprint)).ToArray())
                        unit.Descriptor.Buffs.RemoveFact(buff);
                if (hostile != null && !hostile.Destroyed)
                {
                    hostile.Stats.SaveWill.BaseValue = hostileWill;
                    hostile.Commands.InterruptAll(command => true); hostile.Commands.RemoveFinishedAndUpdateQueue();
                }
                PlaceExpandedSummoningUnit(caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => value != null && !value.Destroyed).ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"),
                    record.ToString(Formatting.Indented));
            }
        }
    }
}
