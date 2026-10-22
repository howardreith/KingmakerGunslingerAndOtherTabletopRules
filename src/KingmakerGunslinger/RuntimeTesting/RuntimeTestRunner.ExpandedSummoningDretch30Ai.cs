using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewDretch30Ai(ExpandedSummoningCorrectionFixture fixture, bool unsafeAlly)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, ally = null;
            BlueprintUnit allyBlueprint = null;
            var areas = new List<AreaEffectEntityData>();
            CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile;
            int fort = target.Stats.SaveFortitude.BaseValue, will = target.Stats.SaveWill.BaseValue, ac = target.Stats.AC.BaseValue;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "dretch");
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var cloud = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_StinkingCloud");
                var fear = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_CauseFear");
                var cloudResource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_StinkingCloudResource");
                var fearResource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_CauseFearResource");
                var cloudArea = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(BlueprintBootstrap.Library,
                    "aa2e0a0fe89693f4e9205fd52c5ba3e5", "Dretch native cloud AI area");
                var safety = fixture.Blueprints.OfType<Sprint29FogAiSafety>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_CloudSafety");
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                bool protectedClear = Game.Instance.Player.Party.Where(value => value != null && value.IsInGame)
                    .All(value => Vector3.Distance(value.Position, far) > new Feet(20).Meters + value.Corpulence);
                if (!protectedClear) throw new InvalidOperationException("Dretch AI lane is not clear of protected party actors.");
                if (unsafeAlly)
                {
                    // An explicitly owned clone retains the real player faction,
                    // without joining or ticking the protected party.
                    allyBlueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                    allyBlueprint.name = "KMG_Runtime_Dretch30_PlayerFactionAlly";
                    allyBlueprint.IsCheater = true; allyBlueprint.Brain = null;
                    allyBlueprint.StartingInventory = Array.Empty<Kingmaker.Blueprints.Items.BlueprintItem>();
                    ally = Game.Instance.EntityCreator.SpawnUnit(allyBlueprint, far + Vector3.right, Quaternion.identity, fixture.Scene);
                    if (ally == null) throw new InvalidOperationException("Owned player-faction ally did not instantiate.");
                    fixture.Created.Add(ally); ally.Stats.HitPoints.BaseValue = 100000;
                    Sprint28SetBrainActivePreservingActions(ally, false); ally.Commands.InterruptAll(true);
                    PlaceExpandedSummoningUnit(ally, far + Vector3.right);
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                }
                var decision = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } };
                float score = safety.Score(decision);
                bool relation = !unsafeAlly || ally.IsPlayerFaction && !owner.IsEnemy(ally) && !owner.IsAlly(ally) &&
                    Vector3.Distance(ally.Position, target.Position) < new Feet(20).Meters;
                bool ready = relation && protectedClear && score == (unsafeAlly ? 0f : 1f) &&
                    owner.Descriptor.Resources.GetResourceAmount(cloudResource) == 1 &&
                    owner.Descriptor.Resources.GetResourceAmount(fearResource) == 1 &&
                    target.Descriptor.Progression.CharacterLevel <= 5;
                string id = unsafeAlly ? "dretch-ai-ally" : "dretch-ai";
                Sprint25Check(id + "-preconditions", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["lane"] = lane,
                    ["ally"] = ally?.UniqueId, ["allyPlayerFaction"] = ally?.IsPlayerFaction,
                    ["ownerIsAlly"] = ally == null ? (bool?)null : owner.IsAlly(ally),
                    ["ownerIsEnemy"] = ally == null ? (bool?)null : owner.IsEnemy(ally),
                    ["safetyScore"] = score, ["protectedPartyOutside"] = protectedClear,
                    ["targetHd"] = target.Descriptor.Progression.CharacterLevel,
                    ["cloudUses"] = owner.Descriptor.Resources.GetResourceAmount(cloudResource),
                    ["fearUses"] = owner.Descriptor.Resources.GetResourceAmount(fearResource),
                    ["fixtureResourceDebits"] = 0 },
                    "The exact native consideration admits a clear target or refuses the owned player-faction nonenemy that IsAlly misses.");
                if (!ready) throw new InvalidOperationException("Dretch AI preconditions are unmet.");
                target.Stats.SaveFortitude.BaseValue = -100; target.Stats.SaveWill.BaseValue = -100; target.Stats.AC.BaseValue = 0;
                foreach (var unit in new[] { owner, target })
                {
                    unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                bool visible = owner.IsAIEnabled && owner.Descriptor.State.CanAct && owner.IsInCombat &&
                    owner.IsEnemy(target) && owner.HasLOS(target) && owner.Memory.ContainsVisible(target);
                Sprint25Check(id + "-ready", visible, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["aiEnabled"] = owner.IsAIEnabled,
                    ["canAct"] = owner.Descriptor.State.CanAct, ["visible"] = owner.Memory.ContainsVisible(target),
                    ["los"] = owner.HasLOS(target), ["fixtureWillBase"] = -100, ["fixtureFortBase"] = -100 },
                    "The native owner is ready, sees its own hostile and has both unspent spell-like resources.");
                if (!visible) throw new InvalidOperationException("Native Dretch AI readiness is unmet.");
                var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>(); var costs = new JArray();
                var saves = new Dretch30CloudSaves(new[] { target });
                EventBus.Subscribe(saves);
                try
                {
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        DateTime deadline = DateTime.UtcNow.AddSeconds(40); int frames = 0, ticks = 0;
                        while (DateTime.UtcNow < deadline && frames++ < 4000)
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
                        string wanted = unsafeAlly ? "KMG_Summoning_OutsiderCelestial_Dretch_CauseFearAi" : "KMG_Summoning_OutsiderCelestial_Dretch_StinkingCloudAi";
                        var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value => value.AiAction?.name == wanted && value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                        for (int frame = 0; frame < 100 && !unsafeAlly && Sprint29OwnedAreas(owner, cloudArea).Length == 0; frame++) yield return 0;
                        areas.AddRange(Sprint29OwnedAreas(owner, cloudArea));
                        bool resources = owner.Descriptor.Resources.GetResourceAmount(cloudResource) == (unsafeAlly ? 1 : 0) &&
                            owner.Descriptor.Resources.GetResourceAmount(fearResource) == (unsafeAlly ? 0 : 1);
                        bool outcome;
                        if (unsafeAlly)
                            outcome = areas.Count == 0 && saves.Rows.Any(value => (int)value["dc"] == 11 &&
                                (string)value["caster"] == owner.UniqueId && (string)value["sourceAbility"] == fear.AssetGuid) &&
                                !commands.OfType<UnitUseAbility>().Any(value => value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_Dretch_StinkingCloudAi");
                        else
                            outcome = areas.Count == 1 && areas[0].Context.Params.CasterLevel == 2 && areas[0].Context.Params.DC == 13 &&
                                areas[0].UnitsInside.Contains(target) && target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Nauseated);
                        bool exact = ticks > 0 && cast != null && economy.Count(cast) == 1 && economy.Exact(cast, false) &&
                            !cast.Cutscene && !cast.IsIgnoreCooldown && resources && outcome;
                        Sprint25Check(id + "-resolved", exact, new JObject {
                            ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["ally"] = ally?.UniqueId,
                            ["frames"] = frames, ["aiTicks"] = ticks, ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                            ["costs"] = costs, ["cloudUses"] = owner.Descriptor.Resources.GetResourceAmount(cloudResource),
                            ["fearUses"] = owner.Descriptor.Resources.GetResourceAmount(fearResource), ["areas"] = areas.Count,
                            ["saves"] = saves.Rows, ["targetNauseated"] = target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Nauseated),
                            ["nativeBuffs"] = new JArray(target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Context?.MaybeCaster, owner))
                                .Select(value => new JObject { ["guid"] = value.Blueprint.AssetGuid, ["name"] = value.Blueprint.name,
                                    ["cl"] = value.Context.Params.CasterLevel, ["dc"] = value.Context.Params.DC })),
                            ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCastCommands"] = 0,
                            ["fixtureAttackCommands"] = 0, ["forcedRolls"] = 0, ["fixtureResourceDebits"] = 0 },
                            unsafeAlly ? "Native AI refuses unsafe cloud and spends only Cause Fear on the hostile, with a real DC11 save and ordinary action." :
                            "Native AI chooses its safe CL2/DC13 cloud and spends only that finite resource, with real nausea and action cost.");
                        if (!exact) throw new InvalidOperationException("Dretch native autonomous decision failed: " + id);
                    }
                }
                finally { EventBus.Unsubscribe(saves); }
            }
            finally
            {
                if (owner != null) { Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); }
                target.Commands.InterruptAll(true);
                foreach (var area in areas.Where(value => value != null && Game.Instance.State.AreaEffects.All.Contains(value))) EndExpandedSummoningArea(area);
                foreach (var unit in new[] { target, ally }.Where(value => value != null && !value.Destroyed))
                    foreach (var buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => owner != null && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray())
                        unit.Descriptor.Buffs.RemoveFact(buff);
                target.Stats.SaveFortitude.BaseValue = fort; target.Stats.SaveWill.BaseValue = will; target.Stats.AC.BaseValue = ac;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, ally }.Where(value => value != null && !value.Destroyed).ToArray());
                if (allyBlueprint != null) UnityEngine.Object.Destroy(allyBlueprint);
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
