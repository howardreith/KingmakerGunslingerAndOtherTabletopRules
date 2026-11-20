using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// S5 T6 / RISK-06 investigation. It establishes which route, if any,
    /// invokes the Mastodon's Trample in normal play. It never issues a
    /// Trample command, never ticks the AI itself and never supplies an
    /// action: the published summon row is cast through its own execution,
    /// and in the owned native Default fixture the game's own controllers
    /// drive the summon in real-time and turn-based combat against a target
    /// on a legal Trample path. Whatever the summon does is recorded, not
    /// required: each check passes when its observation is valid.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Risk06CommandLog : IUnitCommandStartHandler, IUnitCommandEndHandler
        {
            internal UnitEntityData Owner;
            internal readonly List<UnitCommand> Started = new List<UnitCommand>();
            internal readonly JArray Rows = new JArray();
            public void HandleUnitCommandDidStart(UnitCommand command)
            {
                if (command == null || !ReferenceEquals(command.Executor, Owner)) return;
                if (!Started.Contains(command)) Started.Add(command);
                Rows.Add(Risk06Command(command, "start"));
            }
            public void HandleUnitCommandDidEnd(UnitCommand command)
            {
                if (command == null || !ReferenceEquals(command.Executor, Owner)) return;
                Rows.Add(Risk06Command(command, "end"));
            }
        }

        private static JObject Risk06Command(UnitCommand command, string edge)
        {
            var use = command as UnitUseAbility;
            return new JObject { ["edge"] = edge, ["type"] = command.GetType().Name,
                ["aiAction"] = command.AiAction?.name, ["ability"] = use?.Spell?.Blueprint?.name,
                ["target"] = command.TargetUnit?.UniqueId, ["commandType"] = command.Type.ToString(),
                ["started"] = command.IsStarted, ["acted"] = command.IsActed, ["finished"] = command.IsFinished,
                ["result"] = command.Result.ToString(), ["cutscene"] = command.Cutscene,
                ["ignoreCooldown"] = command.IsIgnoreCooldown,
                ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                ["round"] = CombatController.IsInTurnBasedCombat() ? Game.Instance.TurnBasedCombatController.RoundNumber : -1 };
        }

        private IEnumerable<int> ReviewRisk06TrampleRoute(ExpandedSummoningCorrectionFixture fixture, string phase,
            Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39TrampleRoutePhase ||
                !ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(_request.Scenario, phase) ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh RISK-06 fixture in owned outdoor native Default, without narrative, required.");
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var unit = (BlueprintUnit)symbols["KMG.Summoning.Unit.Mastodon"];
            var trample = (BlueprintAbility)symbols[SummonPlaytestR5Policy.MastodonTrampleSymbol];

            // 1. The configured routes: every published row that spawns the
            // Mastodon, its spawn control flag, the brain and every AI action
            // in the library or the product that names this Trample.
            var rows = new JArray();
            foreach (BlueprintAbility ability in symbols.Values.OfType<BlueprintAbility>())
            {
                SummonSpawn[] spawns;
                try { spawns = SummonSignatureReader.Read(ability).Spawns().ToArray(); }
                catch (Exception) { continue; }
                SummonSpawn[] own = spawns.Where(value => value.Unit == unit.AssetGuid).ToArray();
                if (own.Length == 0) continue;
                rows.Add(new JObject { ["ability"] = ability.name, ["guid"] = ability.AssetGuid, ["hidden"] = ability.Hidden,
                    ["spawns"] = own.Length, ["directlyControllable"] = new JArray(own.Select(value => value.DirectlyControllable).Distinct()) });
            }
            var library = BlueprintBootstrap.Library.BlueprintsByAssetId.Values.Concat(symbols.Values).Distinct().ToArray();
            var trampleAi = library.OfType<BlueprintAiCastSpell>().Where(value => ReferenceEquals(value.Ability, trample))
                .Select(value => value.name + ":" + value.AssetGuid).ToArray();
            var brainsWithTrample = library.OfType<BlueprintBrain>().Where(value => value.Actions != null &&
                value.Actions.OfType<BlueprintAiCastSpell>().Any(action => ReferenceEquals(action.Ability, trample)))
                .Select(value => value.name + ":" + value.AssetGuid).ToArray();
            BlueprintBrain brain = unit.Brain;
            var brainActions = new JArray((brain?.Actions ?? new BlueprintAiAction[0]).Select(value => new JObject {
                ["name"] = value?.name, ["guid"] = value?.AssetGuid, ["type"] = value?.GetType().FullName,
                ["ability"] = (value as BlueprintAiCastSpell)?.Ability?.name }));
            int grants = (unit.AddFacts ?? new BlueprintUnitFact[0]).Count(value => ReferenceEquals(value, trample));
            bool censusValid = rows.Count > 0 && grants == 1 && brain != null &&
                rows.All(value => ((JArray)value["directlyControllable"]).Count == 1);
            Sprint25Check("risk06-route-census", censusValid, new JObject { ["unit"] = unit.name + ":" + unit.AssetGuid,
                ["trample"] = trample.name + ":" + trample.AssetGuid, ["trampleGrants"] = grants,
                ["trampleActionType"] = trample.ActionType.ToString(), ["trampleFullRound"] = trample.IsFullRoundAction,
                ["publishedRows"] = rows, ["brain"] = brain == null ? null : brain.name + ":" + brain.AssetGuid,
                ["brainActions"] = brainActions, ["aiActionsNamingTrample"] = new JArray(trampleAi),
                ["brainsNamingTrample"] = new JArray(brainsWithTrample), ["libraryBlueprintsScanned"] = library.Length },
                "Configured routes, recorded: the published rows that spawn the Mastodon with each spawn's control flag, the unit's Trample grant, its brain and actions, and every AI action or brain in the loaded library that names this Trample.");

            foreach (int frame in Risk06NormalCombat(fixture, mode, trample, false)) yield return frame;
            foreach (int frame in Risk06NormalCombat(fixture, mode, trample, true)) yield return frame;
        }

        private IEnumerable<int> Risk06NormalCombat(ExpandedSummoningCorrectionFixture fixture, Cleanup2NativeDefaultScope mode,
            BlueprintAbility trample, bool turnBased)
        {
            var game = Game.Instance;
            string id = "risk06-normal-combat-" + (turnBased ? "turn-based" : "rtwp");
            var target = fixture.Hostile;
            Vector3 targetPosition = target.Position;
            Size sizeBefore = target.Descriptor.State.Size;
            int damageBefore = target.Descriptor.Damage;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldEngagement = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
            var tutorial = turnBased ? Kingmaker.Blueprints.Root.BlueprintRoot.Instance.UITutorials.TBMStarted.Lock : null;
            bool tutorialLocked = tutorial != null && tutorial.IsLocked;
            var flagsBefore = game.Player.UnlockableFlags.UnlockedFlags.ToArray();
            UnitEntityData owner = null;
            var quieted = new List<UnitEntityData>();
            var log = new Risk06CommandLog();
            ExpandedSummoningTrampleObserver contacts = null;
            bool subscribed = false;
            var passedTurns = new JArray();
            try
            {
                if (!mode.Current) throw new InvalidOperationException("RISK-06 native Default ownership is not current.");
                owner = CastExpandedSummoningOwnTier(fixture, "mastodon");
                // Recorded before anything about the summon is changed.
                var summoned = owner.Get<UnitPartSummonedMonster>();
                var liveBrain = owner.Brain;
                var control = new JObject { ["unit"] = owner.UniqueId, ["blueprint"] = owner.Blueprint.name,
                    ["isDirectlyControllable"] = owner.IsDirectlyControllable,
                    ["summonedPartDirectlyControllable"] = summoned?.IsDirectlyControllable,
                    ["inControllableCharacters"] = game.Player.ControllableCharacters.Contains(owner),
                    ["inParty"] = game.Player.Party.Contains(owner), ["playerFaction"] = owner.IsPlayerFaction,
                    ["summoner"] = summoned?.Summoner?.UniqueId,
                    ["liveBrain"] = liveBrain?.Blueprint == null ? null : liveBrain.Blueprint.name + ":" + liveBrain.Blueprint.AssetGuid,
                    ["liveBrainActions"] = new JArray((liveBrain?.Actions ?? new List<Kingmaker.UnitLogic.AiAction>())
                        .Select(value => value?.Blueprint?.name)),
                    ["brainActiveAtSpawn"] = owner.IsBrainActive, ["aiEnabledAtSpawn"] = owner.IsAIEnabled };
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                var granted = owner.Descriptor.Abilities.GetAbility(trample);
                if (granted == null) throw new InvalidOperationException("The cast Mastodon lacks its Trample.");
                var data = new AbilityData(granted);

                // Disclosed fixture: the private hostile is a Medium target
                // (smaller than the Huge Mastodon), keeps its 100000 hit
                // points and its inactive brain, and stands on a surveyed
                // legal Trample path. Other party actors are quieted.
                target.Descriptor.State.Size = Size.Medium;
                foreach (var actor in game.Player.Party.Concat(new[] { fixture.Caster, game.Player.MainCharacter.Value })
                    .Where(value => value != null && !value.Destroyed && !ReferenceEquals(value, owner)).Distinct())
                {
                    if (actor.IsBrainActive) { Sprint28SetBrainActivePreservingActions(actor, false); quieted.Add(actor); }
                    actor.Commands.InterruptAll(true);
                }
                if (target.IsBrainActive) throw new InvalidOperationException("RISK-06 hostile brain is unexpectedly active.");
                Vector3 start; string route;
                TargetWrapper legal = Sprint23FindTrampleRoute(fixture, owner, target, data, out start, out route);
                bool legalAtStart = legal != null && data.CanTarget(legal) && data.IsAvailable && data.IsAvailableForCast;
                float distance = owner.DistanceTo(target);
                if (turnBased)
                {
                    if (tutorial == null) throw new InvalidOperationException("Native turn tutorial flag absent.");
                    tutorial.Unlock();
                }
                foreach (var actor in new[] { owner, target })
                { actor.Wake(0f); if (!game.State.AwakeUnits.Contains(actor)) game.State.AwakeUnits.Add(actor); }
                log.Owner = owner;
                contacts = new ExpandedSummoningTrampleObserver { Caster = owner, Target = target };
                EventBus.Subscribe(log); EventBus.Subscribe(contacts); subscribed = true;
                SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = turnBased;
                game.TurnBasedCombatController.Activate();
                // Disclosed fixture trigger: ordinary native combat entry. The
                // native join controller owns the party's combat edge.
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.JoinCombat(); target.JoinCombat();
                double entered = game.Player.GameTime.TotalSeconds;
                Sprint28SetBrainActivePreservingActions(owner, true);
                var ownerTurns = new HashSet<TurnBased.Controllers.TurnController>();
                int enteredRound = -1;
                var deadline = DateTime.UtcNow.AddSeconds(turnBased ? 120 : 45);
                while (DateTime.UtcNow < deadline)
                {
                    if (!mode.Current) throw new InvalidOperationException("RISK-06 combat lost native Default ownership.");
                    if (turnBased)
                    {
                        var turn = game.TurnBasedCombatController.CurrentTurn;
                        if (enteredRound < 0 && game.TurnBasedCombatController.Initialized) enteredRound = game.TurnBasedCombatController.RoundNumber;
                        if (turn != null && ReferenceEquals(turn.Unit, owner)) ownerTurns.Add(turn);
                        else if (turn != null)
                        {
                            bool idle = !turn.Unit.IsBrainActive && turn.Unit.Commands.Empty && turn.Unit.Commands.Queue.Count == 0;
                            bool acting = turn.Status == TurnBased.Controllers.TurnController.TurnStatus.Preparing ||
                                turn.Status == TurnBased.Controllers.TurnController.TurnStatus.Acting;
                            if (!idle) throw new InvalidOperationException("Unknown or active nonparticipant in RISK-06 turn order: " + turn.Unit.UniqueId);
                            if (acting && turn.CanEndTurnAndNoActing())
                            {
                                passedTurns.Add(new JObject { ["unit"] = turn.Unit.UniqueId, ["round"] = game.TurnBasedCombatController.RoundNumber });
                                EndSprint17SnakeTurn(turn);
                            }
                        }
                        // Two complete owner turns: a third began, or the
                        // round advanced twice past the first owner turn.
                        if (ownerTurns.Count >= 3) break;
                    }
                    else if (game.Player.GameTime.TotalSeconds >= entered + 18) break;
                    game.IsPaused = false; yield return 0;
                }
                if (!mode.Current) throw new InvalidOperationException("RISK-06 combat lost ownership at verdict.");
                Sprint28SetBrainActivePreservingActions(owner, false);
                var started = log.Started.ToArray();
                var aiIssued = started.Where(value => value.AiAction != null).ToArray();
                var trampleCommands = started.OfType<UnitUseAbility>().Where(value => ReferenceEquals(value.Spell?.Blueprint, trample)).ToArray();
                var abilityUses = started.OfType<UnitUseAbility>().Select(value => value.Spell?.Blueprint?.name).ToArray();
                var attacks = started.OfType<UnitAttack>().ToArray();
                bool valid = legalAtStart && owner.IsInCombat && target.IsInCombat && !owner.IsDirectlyControllable &&
                    started.Length > 0 && (turnBased ? ownerTurns.Count >= 3 : game.Player.GameTime.TotalSeconds >= entered + 18);
                Sprint25Check(id, valid, new JObject { ["control"] = control, ["route"] = route,
                    ["legalTramplePointAtStart"] = legal == null ? null : new JArray(legal.Point.x, legal.Point.y, legal.Point.z),
                    ["trampleLegalAtStart"] = legalAtStart, ["trampleAvailableAtEnd"] = data.IsAvailable,
                    ["trampleRequiresFullRound"] = data.RequireFullRoundAction,
                    ["startDistanceMeters"] = distance, ["targetSize"] = target.Descriptor.State.Size.ToString(),
                    ["ownerSize"] = owner.Descriptor.State.Size.ToString(),
                    ["enteredGameSeconds"] = entered, ["observedGameSeconds"] = game.Player.GameTime.TotalSeconds,
                    ["enteredRound"] = enteredRound, ["observedRound"] = turnBased ? game.TurnBasedCombatController.RoundNumber : -1,
                    ["ownerTurnsObserved"] = ownerTurns.Count, ["idleTurnsPassed"] = passedTurns,
                    ["commands"] = log.Rows, ["commandsStarted"] = started.Length, ["aiIssuedCommands"] = aiIssued.Length,
                    ["attackCommands"] = attacks.Length, ["abilityUses"] = new JArray(abilityUses),
                    ["trampleCommands"] = trampleCommands.Length,
                    ["targetSaves"] = new JArray(contacts.Saves.Select(value => new JObject { ["dc"] = value.DifficultyClass,
                        ["type"] = value.Type.ToString(), ["passed"] = value.IsPassed })),
                    ["damageToTarget"] = contacts.Damage.Count, ["targetOpportunityAttacks"] = contacts.Opportunities.Count,
                    ["quietedPartyActors"] = new JArray(quieted.Select(value => value.UniqueId)),
                    ["fixtureTrampleCommands"] = 0, ["fixtureAiTicks"] = 0, ["fixtureSuppliedCommands"] = 0, ["forcedRolls"] = 0 },
                    "A normal published Mastodon summon, driven only by the game's own controllers in owned native Default, fights a Medium enemy standing on a legal Trample path. Its commands are recorded as found (Trample used or not); the check requires a valid observation: legal Trample at start, real combat, at least one command the summon started itself and three rounds (real time) or two complete owner turns, a third begun (turn-based).");
            }
            finally
            {
                try
                {
                    if (subscribed) { EventBus.Unsubscribe(log); EventBus.Unsubscribe(contacts); }
                    if (owner != null && !owner.Destroyed) Sprint28SetBrainActivePreservingActions(owner, false);
                    foreach (var actor in new[] { owner, target }.Where(value => value != null && !value.Destroyed))
                    { actor.Commands.InterruptAll(true); if (actor.IsInCombat) actor.LeaveCombat(); }
                    game.Player.UpdateIsInCombat();
                }
                finally
                {
                    try
                    {
                        foreach (var actor in quieted.Where(value => value != null && !value.Destroyed))
                            Sprint28SetBrainActivePreservingActions(actor, true);
                        target.Descriptor.State.Size = sizeBefore;
                        target.Descriptor.Damage = damageBefore;
                        PlaceExpandedSummoningUnit(target, targetPosition);
                        if (owner != null && !owner.Destroyed) DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    }
                    finally
                    {
                        SettingsRoot.Instance.PauseOnEngagement.CurrentValue = oldEngagement;
                        if (mode.Current)
                        { SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; game.TurnBasedCombatController.Activate(); }
                        if (tutorial != null && tutorialLocked) tutorial.Lock();
                        bool flagsRestored = flagsBefore.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal)
                            .SequenceEqual(game.Player.UnlockableFlags.UnlockedFlags.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal));
                        bool restored = flagsRestored && target.Descriptor.State.Size == sizeBefore &&
                            target.Descriptor.Damage == damageBefore && (owner == null || owner.Destroyed || !fixture.Created.Contains(owner));
                        Sprint25Check(id + "-restored", restored, new JObject { ["flagsRestored"] = flagsRestored,
                            ["targetSize"] = target.Descriptor.State.Size.ToString(), ["targetDamage"] = target.Descriptor.Damage,
                            ["ownerDisposed"] = owner == null || owner.Destroyed, ["quietedRestored"] = quieted.Count,
                            ["turnBasedSetting"] = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue },
                            "The summon is disposed, the hostile's size, damage and position, the quieted actors, the engagement and turn-based settings and the tutorial flag are restored.");
                    }
                }
            }
        }
    }
}
