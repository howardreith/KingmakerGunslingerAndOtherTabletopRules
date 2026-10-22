using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewArchon30Core(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile;
            int will = target.Stats.SaveWill.BaseValue;
            var areaBaseline = Game.Instance.State.AreaEffects.All.ToArray();
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
                owner = CastExpandedSummoningOwnTier(fixture, "lantern-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, "lantern-archon");
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
                target.Stats.SaveWill.BaseValue = -100;
                // Only this disposable emitter changes Charisma. Different live
                // source DCs expose accidental sharing or use of summon CL20.
                owner.Stats.Charisma.BaseValue = 14;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var before = new[] { target.Stats.AdditionalAttackBonus.ModifiedValue, target.Stats.AC.ModifiedValue,
                    target.Stats.SaveFortitude.ModifiedValue, target.Stats.SaveReflex.ModifiedValue,
                    target.Stats.SaveWill.ModifiedValue };
                bool outside = !ContextActionArchonMenace.HasSource(target, penalty, owner) &&
                    !ContextActionArchonMenace.HasSource(target, immunity, owner) &&
                    saves.Rows.Count == 0 && Sprint29OwnedAreas(owner, areaBlueprint).Length == 1 &&
                    Sprint29OwnedAreas(sibling, areaBlueprint).Length == 1;
                Sprint25Check("archon-core-outside", outside, new JObject {
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
                    int dc = ReferenceEquals(source, owner) ? 15 : 13;
                    return rows.Length == 1 && (int)rows[0]["dc"] == dc &&
                        ContextActionArchonMenace.HasSource(target, (bool)rows[0]["passed"] ? immunity : penalty, source) &&
                        !ContextActionArchonMenace.HasSource(target, (bool)rows[0]["passed"] ? penalty : immunity, source);
                });
                var active = target.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, penalty)).ToArray();
                var after = new[] { target.Stats.AdditionalAttackBonus.ModifiedValue, target.Stats.AC.ModifiedValue,
                    target.Stats.SaveFortitude.ModifiedValue, target.Stats.SaveReflex.ModifiedValue,
                    target.Stats.SaveWill.ModifiedValue };
                // Same-named menace penalties use the native nonstacking Penalty
                // descriptor; source ownership remains separate even in overlap.
                bool modifiers = after.Select((value, index) => value - before[index]).All(value =>
                    value == (active.Length == 0 ? 0 : -2));
                Sprint25Check("archon-core-mixed-sources", attribution && modifiers, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["target"] = target.UniqueId,
                    ["ownerCha"] = owner.Stats.Charisma.ModifiedValue, ["ownerHd"] = owner.Descriptor.Progression.CharacterLevel,
                    ["siblingCha"] = sibling.Stats.Charisma.ModifiedValue, ["siblingHd"] = sibling.Descriptor.Progression.CharacterLevel,
                    ["saves"] = saves.Rows.DeepClone(), ["before"] = new JArray(before), ["after"] = new JArray(after),
                    ["penaltyInstances"] = active.Length, ["forcedRolls"] = 0 },
                    "Real source-specific Will saves use each emitter's racial HD/live Charisma; their actual native penalty/immunity outcomes and stat consumers agree.");
                if (!attribution || !modifiers) throw new InvalidOperationException("Archon source/save/penalty consumer failed.");
                int priorSaves = saves.Rows.Count;
                PlaceExpandedSummoningUnit(target, far);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                PlaceExpandedSummoningUnit(target, origin + Vector3.forward);
                for (int frame = 0; frame < 30; frame++) yield return 0;
                bool noRepeat = saves.Rows.Count == priorSaves && emitters.All(source =>
                    ContextActionArchonMenace.HasSource(target, penalty, source) ||
                    ContextActionArchonMenace.HasSource(target, immunity, source));
                Sprint25Check("archon-core-reentry", noRepeat, new JObject {
                    ["savesBefore"] = priorSaves, ["savesAfter"] = saves.Rows.Count,
                    ["sourceBuffs"] = new JArray(target.Buffs.Enumerable.Where(buff => emitters.Contains(buff.Context?.MaybeCaster))
                        .Select(buff => new JObject { ["blueprint"] = buff.Blueprint.AssetGuid,
                            ["source"] = buff.Context?.MaybeCaster?.UniqueId, ["end"] = buff.EndTime.ToString() })) },
                    "Leaving/re-entering neither removes a24h penalty nor rerolls that exact source's resisted/broken immunity.");
                if (!noRepeat) throw new InvalidOperationException("Archon re-entry source memory failed.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                for (int frame = 0; frame < 8; frame++) yield return 0;
                bool sourceCleanup = !ContextActionArchonMenace.HasSource(target, penalty, owner) &&
                    !ContextActionArchonMenace.HasSource(target, immunity, owner) &&
                    (ContextActionArchonMenace.HasSource(target, penalty, sibling) ||
                     ContextActionArchonMenace.HasSource(target, immunity, sibling));
                Sprint25Check("archon-core-source-cleanup", sourceCleanup, new JObject {
                    ["removedOwner"] = owner.UniqueId, ["removedOwnerDestroyed"] = owner.Destroyed,
                    ["sibling"] = sibling.UniqueId, ["siblingPenalty"] = ContextActionArchonMenace.HasSource(target, penalty, sibling),
                    ["siblingImmunity"] = ContextActionArchonMenace.HasSource(target, immunity, sibling) },
                    "Removing one emitter ends only its menace grants; a simultaneous sibling's contribution remains.");
                if (!sourceCleanup) throw new InvalidOperationException("Archon exact-source cleanup failed.");
                // Prove dirty preconditions are observable before removing the
                // second emitter. Only disposable owners are reset.
                bool dirtyDetected = Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any() &&
                    target.Buffs.Enumerable.Except(buffBaseline).Any();
                Sprint25Check("archon-core-residue-detected", dirtyDetected,
                    new JObject { ["ownedAuraResidueDetected"] = dirtyDetected },
                    "A still-live owned emitter and its victim contribution prevent the next subcase from starting.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { sibling });
                for (int frame = 0; frame < 8; frame++) yield return 0;
                bool reset = !Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any() &&
                    !target.Buffs.Enumerable.Except(buffBaseline).Any() &&
                    !target.Commands.Raw.Any(command => command != null && !command.IsFinished) &&
                    target.Stats.AdditionalAttackBonus.ModifiedValue == before[0] && target.Stats.AC.ModifiedValue == before[1] &&
                    !owner.IsInState && !sibling.IsInState;
                Sprint25Check("archon-core-reset", reset, new JObject {
                    ["areasExact"] = !Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any(),
                    ["targetBuffsExact"] = !target.Buffs.Enumerable.Except(buffBaseline).Any(), ["ownedActorsAbsent"] = !owner.IsInState && !sibling.IsInState },
                    "No leftover owned areas, victim buffs, queued target commands or source actors remain before support AI.");
                if (!reset) throw new InvalidOperationException("Archon subcase reset failed; do not run later support case.");
                foreach (int frame in ReviewArchon30Aid(fixture)) yield return frame;
            }
            finally
            {
                EventBus.Unsubscribe(saves);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(unit => unit != null && !unit.Destroyed).ToArray());
                target.Stats.SaveWill.BaseValue = will;
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> ReviewArchon30Aid(ExpandedSummoningCorrectionFixture fixture)
        {
            var owner = CastExpandedSummoningOwnTier(fixture, "lantern-archon");
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
                        value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_Archon_LanternAidAi" &&
                        value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                    var aid = owner.Buffs.Enumerable.FirstOrDefault(buff => buff.Blueprint.AssetGuid == "319b4679f25779e4e9d04360381254e1" &&
                        ReferenceEquals(buff.Context?.MaybeCaster, owner));
                    bool exact = cast != null && !cast.Cutscene && !cast.IsIgnoreCooldown && economy.Count(cast) == 1 &&
                        economy.Exact(cast, false) && aid != null && aid.Context.Params.CasterLevel == 3;
                    Sprint25Check("archon-core-aid-ai", exact, new JObject {
                        ["owner"] = owner.UniqueId, ["hostile"] = target.UniqueId, ["lane"] = lane,
                        ["aiTicks"] = ticks, ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["aidOnOwner"] = aid != null, ["cl"] = aid?.Context?.Params.CasterLevel,
                        ["tempHp"] = owner.Stats.TemporaryHitPoints.ModifiedValue,
                        ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0 },
                        "Actual Lantern AI chooses native CL3 Aid on a useful unbuffed target with an ordinary standard action; no fixture cast.");
                    if (!exact) throw new InvalidOperationException("Archon native Aid AI failed.");
                    var eligibility = fixture.Blueprints.OfType<ArchonAidEligibility>().Single(value => ReferenceEquals(value.OwningBlueprint, owner.Blueprint));
                    float score = eligibility.Score(new Kingmaker.Controllers.Brain.DecisionContext {
                        Unit = owner, Target = new Kingmaker.Controllers.Brain.TargetInfo { Unit = owner } });
                    Sprint25Check("archon-core-aid-no-repeat", score == 0f,
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
