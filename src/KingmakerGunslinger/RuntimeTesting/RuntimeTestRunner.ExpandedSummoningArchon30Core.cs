using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewArchon30Core(ExpandedSummoningCorrectionFixture fixture, bool mixedHound = false, bool boundaries = false)
        {
            string caseId = boundaries ? "archon-boundaries" : mixedHound ? "archon-hound" : "archon-core";
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null, resistedTarget = null;
            var prototypes = new List<BlueprintUnit>(); var localActors = new List<UnitEntityData>();
            Dretch30CloudSaves resistedSaves = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile;
            int will = target.Stats.SaveWill.BaseValue;
            var areaBaseline = Game.Instance.State.AreaEffects.All.ToArray();
            UnityEngine.Object[] bodyResources = new UnityEngine.Object[0];
            Buff foreignPenalty = null;
            var buffBaseline = target.Buffs.Enumerable.ToArray();
            var saves = new Dretch30CloudSaves(new[] { target });
            EventBus.Subscribe(saves);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                var penalty = fixture.Blueprints.OfType<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>()
                    .Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Archon_MenacePenalty");
                var immunity = fixture.Blueprints.OfType<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>()
                    .Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Archon_MenaceImmunity");
                var areaBlueprint = fixture.Blueprints.OfType<BlueprintAbilityAreaEffect>()
                    .Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Archon_MenaceArea");
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 30f);
                owner = CastExpandedSummoningOwnTier(fixture, boundaries ? "trumpet-archon" : mixedHound ? "hound-archon" : "lantern-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, boundaries ? "hound-archon" : "lantern-archon");
                foreach (var unit in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right);
                if (boundaries)
                {
                    resistedTarget = CircleSpawn("S3031_MenaceResistance", far, fixture.Caster,
                        localActors, prototypes, target.Blueprint.Faction);
                    fixture.Created.Add(resistedTarget);
                    resistedTarget.Stats.SaveWill.BaseValue = 100;
                    resistedSaves = new Dretch30CloudSaves(new[] { resistedTarget });
                    EventBus.Subscribe(resistedSaves);
                }
                target.Stats.SaveWill.BaseValue = -100;
                var shaken = BlueprintLibraryLookup.RequireExact<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(
                    BlueprintBootstrap.Library, "25ec6cb6ab1845c48a95f9c20b034220", "unrelated native Shaken penalty control");
                foreignPenalty = target.Buffs.AddBuff(shaken,
                    new MechanicsContext(target, target.Descriptor, shaken, null, new TargetWrapper(target)), TimeSpan.FromMinutes(10));
                if (foreignPenalty == null) throw new InvalidOperationException("Owned unrelated penalty control did not apply.");
                buffBaseline = target.Buffs.Enumerable.ToArray();
                // Only this disposable emitter changes Charisma. Different live
                // source DCs expose accidental sharing or use of summon CL20.
                if (mixedHound)
                {
                    foreach (int frame in ReviewHound30Profile(fixture, owner)) yield return frame;
                    bodyResources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View);
                }
                else if (!boundaries) owner.Stats.Charisma.BaseValue = 14;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var before = new[] { target.Stats.AdditionalAttackBonus.ModifiedValue, target.Stats.AC.ModifiedValue,
                    target.Stats.SaveFortitude.ModifiedValue, target.Stats.SaveReflex.ModifiedValue,
                    target.Stats.SaveWill.ModifiedValue };
                bool outside = !ContextActionArchonMenace.HasSource(target, penalty, owner) &&
                    !ContextActionArchonMenace.HasSource(target, immunity, owner) &&
                    saves.Rows.Count == 0 && Sprint29OwnedAreas(owner, areaBlueprint).Length == 1 &&
                    Sprint29OwnedAreas(sibling, areaBlueprint).Length == 1;
                Sprint25Check(caseId + "-outside", outside, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["target"] = target.UniqueId,
                    ["lane"] = lane, ["saves"] = saves.Rows.DeepClone(), ["ownerAreas"] = Sprint29OwnedAreas(owner, areaBlueprint).Length,
                    ["siblingAreas"] = Sprint29OwnedAreas(sibling, areaBlueprint).Length },
                    "Two exact native moving areas exist; the hostile outside20ft has no exposure or save.");
                if (!outside) throw new InvalidOperationException("Archon aura outside preconditions unmet.");
                PlaceExpandedSummoningUnit(target, origin + Vector3.forward);
                DateTime deadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < deadline && saves.Rows.Count < 2) yield return 0;
                var emitters = new[] { owner, sibling };
                bool attribution = emitters.All(source => {
                    var rows = saves.Rows.Where(row => (string)row["caster"] == source.UniqueId).ToArray();
                    int dc = ReferenceEquals(source, owner) ? (boundaries ? 22 : mixedHound ? 16 : 15) : (boundaries ? 16 : 13);
                    return rows.Length == 1 && (int)rows[0]["dc"] == dc &&
                        ContextActionArchonMenace.HasSource(target, (bool)rows[0]["passed"] ? immunity : penalty, source) &&
                        !ContextActionArchonMenace.HasSource(target, (bool)rows[0]["passed"] ? penalty : immunity, source);
                });
                var sourceGrants = target.Buffs.Enumerable.Where(buff => emitters.Contains(buff.Context?.MaybeCaster) &&
                    (ReferenceEquals(buff.Blueprint, penalty) || ReferenceEquals(buff.Blueprint, immunity))).ToArray();
                bool privateParams = sourceGrants.Length == 2 && sourceGrants.All(buff =>
                    !ReferenceEquals(buff.Context.Params, AbilityParams.Empty) &&
                    buff.Context.Params.CasterLevel == buff.Context.MaybeCaster.Descriptor.Progression.CharacterLevel &&
                    buff.Context.Params.DC == 10 + buff.Context.Params.CasterLevel / 2 + buff.Context.MaybeCaster.Stats.Charisma.Bonus + 2) &&
                    !ReferenceEquals(sourceGrants[0].Context.Params, sourceGrants[1].Context.Params);
                attribution &= privateParams;
                var active = target.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, penalty)).ToArray();
                var after = new[] { target.Stats.AdditionalAttackBonus.ModifiedValue, target.Stats.AC.ModifiedValue,
                    target.Stats.SaveFortitude.ModifiedValue, target.Stats.SaveReflex.ModifiedValue,
                    target.Stats.SaveWill.ModifiedValue };
                // The scoped component retains one native penalty while source
                // ownership remains separate. Unrelated Shaken remains active.
                bool modifiers = target.Buffs.Enumerable.Contains(foreignPenalty) && after.Select((value, index) => value - before[index]).All(value =>
                    value == (active.Length == 0 ? 0 : -2));
                Sprint25Check(caseId + "-mixed-sources", attribution && modifiers, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["target"] = target.UniqueId,
                    ["ownerCha"] = owner.Stats.Charisma.ModifiedValue, ["ownerHd"] = owner.Descriptor.Progression.CharacterLevel,
                    ["siblingCha"] = sibling.Stats.Charisma.ModifiedValue, ["siblingHd"] = sibling.Descriptor.Progression.CharacterLevel,
                    ["saves"] = saves.Rows.DeepClone(), ["before"] = new JArray(before), ["after"] = new JArray(after),
                    ["privateSourceParameters"] = privateParams,
                    ["sourceParameters"] = new JArray(sourceGrants.Select(buff => new JObject {
                        ["source"] = buff.Context.MaybeCaster.UniqueId, ["cl"] = buff.Context.Params.CasterLevel,
                        ["dc"] = buff.Context.Params.DC })),
                    ["penaltyInstances"] = active.Length, ["unrelatedShakenPreserved"] = target.Buffs.Enumerable.Contains(foreignPenalty),
                    ["modifierSources"] = new JArray(target.Stats.AdditionalAttackBonus.Modifiers.Select(modifier => new JObject {
                        ["value"] = modifier.ModValue, ["source"] = modifier.Source?.Blueprint?.AssetGuid })), ["forcedRolls"] = 0 },
                    "Real source-specific Will saves use each emitter's racial HD/live Charisma; their actual native penalty/immunity outcomes and stat consumers agree.");
                if (!attribution || !modifiers) throw new InvalidOperationException("Archon source/save/penalty consumer failed.");
                if (boundaries)
                {
                    PlaceExpandedSummoningUnit(resistedTarget, origin + Vector3.forward * 1.5f);
                    DateTime resistanceDeadline = DateTime.UtcNow.AddSeconds(12);
                    while (resistedSaves.Rows.Count < 2 && DateTime.UtcNow < resistanceDeadline) yield return 0;
                    bool resistance = resistedSaves.Rows.Count == 2 &&
                        resistedSaves.Rows.Any(row => (bool)row["passed"]) && emitters.All(source => {
                            var rows = resistedSaves.Rows.Where(row => (string)row["caster"] == source.UniqueId).ToArray();
                            return rows.Length == 1 && (int)rows[0]["dc"] == (ReferenceEquals(source, owner) ? 22 : 16) &&
                                ContextActionArchonMenace.HasSource(resistedTarget, (bool)rows[0]["passed"] ? immunity : penalty, source) &&
                                !ContextActionArchonMenace.HasSource(resistedTarget, (bool)rows[0]["passed"] ? penalty : immunity, source);
                        });
                    var resistedGrants = resistedTarget.Buffs.Enumerable.Where(buff => emitters.Contains(buff.Context?.MaybeCaster) &&
                        (ReferenceEquals(buff.Blueprint, penalty) || ReferenceEquals(buff.Blueprint, immunity))).ToArray();
                    resistance &= resistedGrants.Length == 2 && resistedGrants.All(buff => !buff.IsPermanent &&
                        buff.IsNotDispelable && buff.TimeLeft.TotalSeconds > 86300 && buff.TimeLeft.TotalSeconds <= 86400);
                    Sprint25Check(caseId + "-resistance", resistance, new JObject {
                        ["target"] = resistedTarget.UniqueId, ["fixtureWillBase"] = 100,
                        ["saves"] = resistedSaves.Rows.DeepClone(), ["forcedRolls"] = 0,
                        ["sources"] = new JArray(resistedGrants.Select(buff => new JObject {
                            ["source"] = buff.Context.MaybeCaster.UniqueId, ["buff"] = buff.Blueprint.AssetGuid,
                            ["secondsLeft"] = buff.TimeLeft.TotalSeconds })) },
                        "Actual high-Will native saves include a successful resistance; each exact source retains its correctly rolled immunity or natural1 penalty with a native24-hour timer.");
                    if (!resistance) throw new InvalidOperationException("Archon resistance outcome/timer mismatch or no successful native save observed.");
                    var timers = new JArray(sourceGrants.Select(buff => new JObject {
                        ["source"] = buff.Context.MaybeCaster.UniqueId, ["buff"] = buff.Blueprint.AssetGuid,
                        ["secondsLeft"] = buff.TimeLeft.TotalSeconds, ["end"] = buff.EndTime.ToString(),
                        ["permanent"] = buff.IsPermanent, ["notDispelable"] = buff.IsNotDispelable }));
                    bool timed = sourceGrants.All(buff => !buff.IsPermanent && buff.IsNotDispelable &&
                        buff.TimeLeft.TotalSeconds > 86300 && buff.TimeLeft.TotalSeconds <= 86400);
                    Sprint25Check(caseId + "-native-timers", timed,
                        new JObject { ["grants"] = timers, ["forcedRolls"] = 0, ["advancedWorldHours"] = 0 },
                        "Actual per-source native buff clocks retain a finite24-hour duration; no world clock jump or elapsed24-hour claim.");
                    if (!timed) throw new InvalidOperationException("Archon native timer contract differs.");
                    // Choose an actually failed native save. A natural20 is a
                    // retained immunity outcome, never relabeled as failure.
                    var hitSource = emitters.FirstOrDefault(source => ContextActionArchonMenace.HasSource(target, penalty, source));
                    if (hitSource == null) throw new InvalidOperationException("Fixture precondition: both actual saves passed; no penalty exists for hit-break observation.");
                    var siblingSource = emitters.Single(source => !ReferenceEquals(source, hitSource));
                    var retained = target.Buffs.Enumerable.Single(buff => ReferenceEquals(buff.Context?.MaybeCaster, siblingSource) &&
                        (ReferenceEquals(buff.Blueprint, penalty) || ReferenceEquals(buff.Blueprint, immunity)));
                    int oldBab = target.Stats.BaseAttackBonus.BaseValue;
                    int oldDamage = hitSource.Damage;
                    var hitRows = new JArray(); bool hit = false;
                    try
                    {
                        target.Stats.BaseAttackBonus.BaseValue = 100;
                        for (int attempt = 0; attempt < 5 && !hit; attempt++)
                        {
                            var attack = Rulebook.Trigger(new RuleAttackWithWeapon(target, hitSource, target.Body.EmptyHandWeapon, 0));
                            if (attack.AttackRoll == null) throw new InvalidOperationException("Native hit-break attack omitted its roll.");
                            hit = attack.AttackRoll.IsHit;
                            hitRows.Add(new JObject { ["source"] = target.UniqueId, ["target"] = hitSource.UniqueId,
                                ["weapon"] = target.Body.EmptyHandWeapon.Blueprint.AssetGuid, ["roll"] = (int)attack.AttackRoll.Roll,
                                ["bonus"] = attack.AttackRoll.AttackBonus, ["ac"] = attack.AttackRoll.TargetAC, ["hit"] = hit });
                        }
                    }
                    finally { target.Stats.BaseAttackBonus.BaseValue = oldBab; hitSource.Damage = oldDamage; }
                    var marker = target.Buffs.Enumerable.SingleOrDefault(buff => ReferenceEquals(buff.Blueprint, immunity) &&
                        ReferenceEquals(buff.Context?.MaybeCaster, hitSource));
                    bool broken = hit && !ContextActionArchonMenace.HasSource(target, penalty, hitSource) && marker != null &&
                        marker.IsNotDispelable && !marker.IsPermanent && marker.TimeLeft.TotalSeconds > 86300 &&
                        marker.TimeLeft.TotalSeconds <= 86400 && target.Buffs.Enumerable.Contains(retained) &&
                        target.Buffs.Enumerable.Contains(foreignPenalty);
                    Sprint25Check(caseId + "-hit-break", broken, new JObject {
                        ["attacks"] = hitRows, ["fixtureBab"] = 100, ["forcedRolls"] = 0,
                        ["brokenSource"] = hitSource.UniqueId, ["retainedSource"] = siblingSource.UniqueId,
                        ["otherExactBuffRetained"] = target.Buffs.Enumerable.Contains(retained),
                        ["immunitySecondsLeft"] = marker?.TimeLeft.TotalSeconds, ["unrelatedShakenPreserved"] = target.Buffs.Enumerable.Contains(foreignPenalty) },
                        "A real native weapon hit breaks only the struck emitter's penalty and starts that source's native24-hour immunity; other sources and unrelated penalties remain.");
                    if (!broken) throw new InvalidOperationException("Archon source-specific hit break failed.");
                }
                int resistedPrior = resistedSaves?.Rows.Count ?? 0;
                if (boundaries) PlaceExpandedSummoningUnit(resistedTarget, far);
                int priorSaves = saves.Rows.Count;
                PlaceExpandedSummoningUnit(target, far);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                PlaceExpandedSummoningUnit(target, origin + Vector3.forward);
                if (boundaries) PlaceExpandedSummoningUnit(resistedTarget, origin + Vector3.forward * 1.5f);
                for (int frame = 0; frame < 30; frame++) yield return 0;
                bool noRepeat = saves.Rows.Count == priorSaves && (!boundaries ||
                    resistedSaves.Rows.Count == resistedPrior && emitters.All(source =>
                        ContextActionArchonMenace.HasSource(resistedTarget, penalty, source) ||
                        ContextActionArchonMenace.HasSource(resistedTarget, immunity, source))) && emitters.All(source =>
                    ContextActionArchonMenace.HasSource(target, penalty, source) ||
                    ContextActionArchonMenace.HasSource(target, immunity, source));
                Sprint25Check(caseId + "-reentry", noRepeat, new JObject {
                    ["savesBefore"] = priorSaves, ["savesAfter"] = saves.Rows.Count,
                    ["resistedBefore"] = resistedPrior, ["resistedAfter"] = resistedSaves?.Rows.Count,
                    ["sourceBuffs"] = new JArray(target.Buffs.Enumerable.Where(buff => emitters.Contains(buff.Context?.MaybeCaster))
                        .Select(buff => new JObject { ["blueprint"] = buff.Blueprint.AssetGuid,
                            ["source"] = buff.Context?.MaybeCaster?.UniqueId, ["end"] = buff.EndTime.ToString() })) },
                    "Leaving/re-entering neither removes a24h penalty nor rerolls that exact source's resisted/broken immunity.");
                if (!noRepeat) throw new InvalidOperationException("Archon re-entry source memory failed.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (mixedHound)
                {
                    bool released = bodyResources.Length >= 3 && bodyResources.All(value => value == null);
                    Sprint25Check("archon-hound-original-body-release", released,
                        new JObject { ["owner"] = owner.UniqueId, ["captured"] = bodyResources.Length,
                            ["remaining"] = bodyResources.Count(value => value != null) },
                        "Actual owner disposal releases only its private original mesh/material/accessory resources before the sibling case continues.");
                    if (!released) throw new InvalidOperationException("Original Hound body resources remain live.");
                }
                var remainingStats = new[] { target.Stats.AdditionalAttackBonus.ModifiedValue, target.Stats.AC.ModifiedValue,
                    target.Stats.SaveFortitude.ModifiedValue, target.Stats.SaveReflex.ModifiedValue, target.Stats.SaveWill.ModifiedValue };
                int remainingPenalty = ContextActionArchonMenace.HasSource(target, penalty, sibling) ? -2 : 0;
                bool handoff = remainingStats.Select((value, index) => value - before[index]).All(value => value == remainingPenalty) &&
                    target.Buffs.Enumerable.Contains(foreignPenalty);
                bool sourceCleanup = handoff && !ContextActionArchonMenace.HasSource(target, penalty, owner) &&
                    !ContextActionArchonMenace.HasSource(target, immunity, owner) &&
                    (ContextActionArchonMenace.HasSource(target, penalty, sibling) ||
                     ContextActionArchonMenace.HasSource(target, immunity, sibling));
                Sprint25Check(caseId + "-source-cleanup", sourceCleanup, new JObject {
                    ["removedOwner"] = owner.UniqueId, ["removedOwnerDestroyed"] = owner.Destroyed,
                    ["remainingStats"] = new JArray(remainingStats), ["baselineStats"] = new JArray(before),
                    ["remainingPenalty"] = remainingPenalty, ["handoff"] = handoff,
                    ["sibling"] = sibling.UniqueId, ["siblingPenalty"] = ContextActionArchonMenace.HasSource(target, penalty, sibling),
                    ["siblingImmunity"] = ContextActionArchonMenace.HasSource(target, immunity, sibling) },
                    "Removing one emitter ends only its menace grants; a simultaneous sibling's contribution remains.");
                if (!sourceCleanup) throw new InvalidOperationException("Archon exact-source cleanup failed.");
                // Prove dirty preconditions are observable before removing the
                // second emitter. Only disposable owners are reset.
                bool dirtyDetected = Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any() &&
                    target.Buffs.Enumerable.Except(buffBaseline).Any();
                Sprint25Check(caseId + "-residue-detected", dirtyDetected,
                    new JObject { ["ownedAuraResidueDetected"] = dirtyDetected },
                    "A still-live owned emitter and its victim contribution prevent the next subcase from starting.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { sibling });
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (boundaries)
                {
                    foreach (var actor in new[] { target, resistedTarget })
                    {
                        actor.Commands.InterruptAll(true);
                        if (actor.IsInCombat) actor.LeaveCombat();
                        foreach (var command in actor.Commands.Raw.Where(value => value != null && !value.IsFinished).ToArray()) command.Interrupt(true);
                        actor.Commands.RemoveFinishedAndUpdateQueue();
                    }
                    bool resistedClean = !resistedTarget.Buffs.Enumerable.Any(buff => emitters.Contains(buff.Context?.MaybeCaster));
                    Sprint25Check(caseId + "-resisted-cleanup", resistedClean,
                        new JObject { ["target"] = resistedTarget.UniqueId, ["sourceGrantsAbsent"] = resistedClean },
                        "Both removed emitters also clean only their own grants on the resistance-control victim.");
                    if (!resistedClean) throw new InvalidOperationException("Archon resistance victim grants remain.");
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { resistedTarget });
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    bool absent = resistedTarget.Destroyed && !resistedTarget.IsInState && !fixture.Created.Contains(resistedTarget);
                    Sprint25Check(caseId + "-resisted-absent", absent,
                        new JObject { ["target"] = resistedTarget.UniqueId, ["destroyed"] = resistedTarget.Destroyed,
                            ["inState"] = resistedTarget.IsInState, ["inFixture"] = fixture.Created.Contains(resistedTarget) },
                        "The additional exact-owned resistance victim is disposed before the reset assertion.");
                    if (!absent) throw new InvalidOperationException("Archon resistance victim remains.");
                }
                bool reset = !Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any() &&
                    !target.Buffs.Enumerable.Except(buffBaseline).Any() &&
                    !target.Commands.Raw.Any(command => command != null && !command.IsFinished) &&
                    target.Stats.AdditionalAttackBonus.ModifiedValue == before[0] && target.Stats.AC.ModifiedValue == before[1] &&
                    !owner.IsInState && !sibling.IsInState;
                Sprint25Check(caseId + "-reset", reset, new JObject {
                    ["areasExact"] = !Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any(),
                    ["targetBuffsExact"] = !target.Buffs.Enumerable.Except(buffBaseline).Any(), ["ownedActorsAbsent"] = !owner.IsInState && !sibling.IsInState },
                    "No leftover owned areas, victim buffs, queued target commands or source actors remain before support AI.");
                if (!reset) throw new InvalidOperationException("Archon subcase reset failed; do not run later support case.");
                if (!boundaries) foreach (int frame in ReviewArchon30Aid(fixture, mixedHound)) yield return frame;
            }
            finally
            {
                EventBus.Unsubscribe(saves);
                if (resistedSaves != null) EventBus.Unsubscribe(resistedSaves);
                DisposeExpandedSummoningUnits(fixture.Created, localActors.Where(unit => unit != null && !unit.Destroyed).ToArray());
                foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(unit => unit != null && !unit.Destroyed).ToArray());
                if (foreignPenalty != null) foreignPenalty.Remove();
                target.Stats.SaveWill.BaseValue = will;
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> ReviewArchon30Aid(ExpandedSummoningCorrectionFixture fixture, bool mixedHound)
        {
            string caseId = mixedHound ? "archon-hound" : "archon-core";
            string aiName = mixedHound ? "KMG_Summoning_OutsiderCelestial_HoundArchon_AidAi" : "KMG_Summoning_OutsiderCelestial_Archon_LanternAidAi";
            int casterLevel = mixedHound ? 6 : 3;
            var owner = CastExpandedSummoningOwnTier(fixture, mixedHound ? "hound-archon" : "lantern-archon");
            var target = fixture.Hostile;
            Sprint28SetBrainActivePreservingActions(owner, false);
            owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
            try
            {
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                foreach (var unit in new[] { owner, target })
                {
                    unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>(); var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    DateTime deadline = DateTime.UtcNow.AddSeconds(30); int frames = 0, ticks = 0;
                    while (DateTime.UtcNow < deadline && frames++ < 3000)
                    {
                        if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                        {
                            foreach (var candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate);
                            ticks++;
                        }
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        if (commands.OfType<UnitUseAbility>().Any(value => value.AiAction != null && value.IsFinished &&
                            value.ExecutionProcess != null && value.ExecutionProcess.IsEnded)) break;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                        value.AiAction?.name == aiName &&
                        value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                    var aid = owner.Buffs.Enumerable.FirstOrDefault(buff => buff.Blueprint.AssetGuid == "319b4679f25779e4e9d04360381254e1" &&
                        ReferenceEquals(buff.Context?.MaybeCaster, owner));
                    bool exact = cast != null && !cast.Cutscene && !cast.IsIgnoreCooldown && economy.Count(cast) == 1 &&
                        economy.Exact(cast, false) && aid != null && aid.Context.Params.CasterLevel == casterLevel;
                    Sprint25Check(caseId + "-aid-ai", exact, new JObject {
                        ["owner"] = owner.UniqueId, ["hostile"] = target.UniqueId, ["lane"] = lane,
                        ["aiTicks"] = ticks, ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["aidOnOwner"] = aid != null, ["cl"] = aid?.Context?.Params.CasterLevel,
                        ["tempHp"] = owner.Stats.TemporaryHitPoints.ModifiedValue,
                        ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0 },
                        "Actual " + (mixedHound ? "Hound CL6" : "Lantern CL3") + " AI chooses native Aid on a useful unbuffed target with an ordinary standard action; no fixture cast.");
                    if (!exact) throw new InvalidOperationException("Archon native Aid AI failed.");
                    var eligibility = fixture.Blueprints.OfType<ArchonAidEligibility>().Single(value => ReferenceEquals(value.OwningBlueprint, owner.Blueprint));
                    float score = eligibility.Score(new Kingmaker.Controllers.Brain.DecisionContext {
                        Unit = owner, Target = new Kingmaker.Controllers.Brain.TargetInfo { Unit = owner } });
                    Sprint25Check(caseId + "-aid-no-repeat", score == 0f,
                        new JObject { ["scoreOnAlreadyAidedTarget"] = score }, "Native Aid target selection refuses an already aided target without spending another action.");
                    if (score != 0f) throw new InvalidOperationException("Archon repeated support refusal failed.");
                }
            }
            finally
            {
                Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
            }
        }
    }
}
