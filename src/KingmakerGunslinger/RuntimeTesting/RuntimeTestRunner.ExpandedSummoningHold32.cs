using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
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
        // Bounded use of the already proven Dialog round-controller adapter.
        // Turn-based cases leave these null and use native game controllers.
        private LillendDazeRoundTicks32 _lillendHoldOwnerTicks32;

        private sealed class Hold32RepeatSaves : IGlobalRulebookHandler<RuleSavingThrow>
        {
            internal Buff Effect;
            internal readonly JArray Rows = new JArray();
            private readonly Dictionary<RuleSavingThrow, JObject> _pending = new Dictionary<RuleSavingThrow, JObject>();
            private static JArray Costs(UnitEntityData actor)
            {
                var cost = actor.CombatState.Cooldown;
                return new JArray(cost.StandardAction, cost.MoveAction, cost.SwiftAction);
            }
            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (Effect == null || !ReferenceEquals(evt.Reason?.Fact, Effect)) return;
                _pending[evt] = new JObject { ["before"] = Costs(evt.Initiator),
                    ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                    ["effect"] = Effect.Blueprint.AssetGuid, ["dc"] = evt.DifficultyClass,
                    ["commands"] = new JArray(evt.Initiator.Commands.Raw.Where(value => value != null).Select(Sprint27AiCommand)) };
            }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                JObject row;
                if (!_pending.TryGetValue(evt, out row)) return;
                _pending.Remove(evt); row["after"] = Costs(evt.Initiator); row["passed"] = evt.IsPassed;
                row["costsUnchanged"] = JToken.DeepEquals(row["before"], row["after"]); Rows.Add(row);
            }
        }

        private IEnumerable<int> ReviewLillendHold32(ExpandedSummoningCorrectionFixture fixture, bool turnBased, bool repeat)
        {
            var game = Game.Instance; var target = fixture.Hostile;
            UnitEntityData owner = null, sibling = null; Buff effect = null;
            Lillend32CommandStarts starts = null; Dretch30CloudSaves saves = null;
            var repeats = new Hold32RepeatSaves();
            int oldWill = target.Stats.SaveWill.BaseValue;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = game.IsPaused;
            string id = "lillend-hold-" + (turnBased ? "turn-" : "rtwp-") + (repeat ? "repeat" : "expiry");
            if (_lillendHoldOwnerTicks32 != null)
                throw new InvalidOperationException("Hold round ownership already active.");
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                game.TurnBasedCombatController.Activate(); game.IsPaused = false;
                ResetExpandedSummoningHostile(fixture);
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                foreach (var actor in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue(); }
                var fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "48ac8db94d5de7645906c7d0ad3bcfbd", "native fighter for disposable Hold target");
                int levels = 5 - target.Descriptor.Progression.CharacterLevel;
                if (levels < 0) throw new InvalidOperationException("Hold target exceeds selected five HD.");
                ElementalSpellAffinityScenario.Advance(target.Descriptor, fighter, levels);
                target.Stats.HitPoints.BaseValue = 100000; target.Stats.SaveWill.BaseValue = -100;
                var policy = fixture.Blueprints.OfType<LillendSpellEligibility32>().Single(value => value.Choice == LillendSpellChoice32.HoldPerson);
                var burst = fixture.Blueprints.OfType<LillendSpellEligibility32>().Single(value => value.Choice == LillendSpellChoice32.SoundBurst);
                var ability = policy.Spell; var book = owner.Descriptor.GetSpellbook(policy.Book);
                var siblingBook = sibling.Descriptor.GetSpellbook(policy.Book);
                var data = new AbilityData(ability, book); var parameters = data.CalculateParams();
                var charm = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value =>
                    value.name == "KMG_Summoning_FeyCelestial_LillendAzata_CharmPersonSlaResource");
                int charmBefore = owner.Descriptor.Resources.GetResourceAmount(charm);
                owner.Descriptor.Resources.Spend(charm, charmBefore);
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    originAllyExclusionRadius: new Feet(30).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                var side = Vector3.Cross(Vector3.up, (far - origin).normalized).normalized;
                var near = AstarPath.active.GetNearest(far + side * 1.2f);
                if (near.node == null || !near.node.Walkable || Vector3.Distance(near.clampedPosition, far) > 2f)
                    throw new InvalidOperationException("Hold allied bystander has no nearby walkable point.");
                PlaceExpandedSummoningUnit(sibling, near.clampedPosition);
                foreach (var actor in new[] { owner, sibling, target })
                { actor.Wake(0f); if (!game.State.AwakeUnits.Contains(actor)) game.State.AwakeUnits.Add(actor); }
                owner.Memory.Add(target); target.Memory.Add(owner);
                var memory = new Kingmaker.Controllers.Units.UnitMemoryController();
                var visibleLimit = DateTime.UtcNow.AddSeconds(8); int visibleFrames = 0;
                do
                {
                    game.IsPaused = false; yield return 0; visibleFrames++;
                    owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded(); memory.Tick();
                } while (DateTime.UtcNow < visibleLimit && (visibleFrames < 8 || !owner.HasLOS(target) || !owner.Memory.ContainsVisible(target)));
                owner.JoinCombat(); target.JoinCombat(); game.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                var admission = new JObject();
                if (turnBased)
                {
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                    var limit = DateTime.UtcNow.AddSeconds(30);
                    while (DateTime.UtcNow < limit && !Sprint18OwnerIsActing(owner)) yield return 0;
                    if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner))
                        throw new InvalidOperationException("Hold native turn admission failed.");
                }
                var choice = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } };
                Func<JObject> siblingBudget = () => new JObject { ["slots"] = new JArray(Enumerable.Range(1, 3).Select(siblingBook.GetSpontaneousSlots)),
                    ["charm"] = sibling.Descriptor.Resources.GetResourceAmount(charm) };
                var siblingBefore = siblingBudget();
                bool ready = owner.IsEnemy(target) && !owner.IsEnemy(sibling) && owner.HasLOS(target) &&
                    owner.Memory.ContainsVisible(target) && target.Descriptor.Progression.CharacterLevel == 5 &&
                    policy.Score(choice) == 1 && burst.Score(choice) == 0 && data.IsAvailable && data.CanTarget(new TargetWrapper(target)) &&
                    parameters.CasterLevel == 7 && parameters.DC == 16 && book.GetSpontaneousSlots(2) == 4 &&
                    siblingBook.GetSpontaneousSlots(2) == 4 && charmBefore == 1 &&
                    data.RuntimeActionType == UnitCommand.CommandType.Standard && !data.RequireFullRoundAction;
                Sprint25Check(id + "-ready", ready, new JObject { ["lane"] = lane, ["admission"] = admission,
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["distance"] = owner.DistanceTo(target), ["bystanderDistance"] = sibling.DistanceTo(target),
                    ["holdScore"] = policy.Score(choice), ["burstScore"] = burst.Score(choice), ["slots"] = book.GetSpontaneousSlots(2),
                    ["cl"] = parameters.CasterLevel, ["dc"] = parameters.DC, ["willBase"] = target.Stats.SaveWill.BaseValue,
                    ["fixtureCharmSlaPreSpend"] = charmBefore, ["siblingBudget"] = siblingBefore },
                    "Visible five-HD humanoid, real bard7 L2 slots and Standard spell; nearby healthy sibling excludes unsafe Sound Burst. Only the owner's competing Charm SLA is pre-spent.");
                if (!ready) throw new InvalidOperationException("Hold fixture preconditions failed.");
                var ai = owner.Blueprint.Brain.Actions.OfType<BlueprintAiCastSpell>().Single(value => ReferenceEquals(value.Ability, ability));
                if (!turnBased)
                {
                    _lillendHoldOwnerTicks32 = new LillendDazeRoundTicks32(owner, ai);
                }
                starts = new Lillend32CommandStarts { Owner = owner }; saves = new Dretch30CloudSaves(new[] { target });
                EventBus.Subscribe(starts); EventBus.Subscribe(saves); EventBus.Subscribe(repeats);
                var commands = new HashSet<UnitCommand>(); var costs = new JArray();
                int ticks = 0, nativeTurns = 0;
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    var limit = DateTime.UtcNow.AddSeconds(45); int frames = 0;
                    while (DateTime.UtcNow < limit)
                    {
                        frames++;
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        foreach (var command in starts.Commands) commands.Add(command);
                        effect = target.Buffs.Enumerable.SingleOrDefault(value => value.Blueprint.AssetGuid ==
                            "21c3681a649947359d30f909362d945b" && ReferenceEquals(value.Context?.MaybeCaster, owner));
                        if (effect != null) repeats.Effect = effect;
                        var complete = commands.OfType<UnitUseAbility>().Where(value => ReferenceEquals(value.Spell?.Blueprint, ability) &&
                            value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded).ToArray();
                        if (effect != null && complete.Length > 0 || complete.Length >= 3) break;
                        if (!turnBased && (frames == 1 || frames % 60 == 0)) { Sprint24TickExactAi(owner); ticks++; }
                        if (turnBased && Sprint18OwnerIsActing(owner)) nativeTurns++;
                        game.IsPaused = false; yield return 0;
                    }
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var casts = commands.OfType<UnitUseAbility>().Where(value => ReferenceEquals(value.Spell?.Blueprint, ability) &&
                        value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded).ToArray();
                    var applications = saves.Applications.Where(value => (string)value["buff"] == "21c3681a649947359d30f909362d945b" &&
                        (string)value["caster"] == owner.UniqueId && (bool)value["applied"]).ToArray();
                    bool castPass = effect != null && effect.Active && !effect.IsDisposed && casts.Length > 0 && casts.Length <= 3 &&
                        casts.All(value => value.Result == UnitCommand.ResultType.Success && value.AiAction == ai &&
                            value.Spell.Spellbook == book && value.Type == UnitCommand.CommandType.Standard && !value.Spell.RequireFullRoundAction &&
                            !value.Cutscene && !value.IsIgnoreCooldown && economy.Count(value) == 1 && economy.Exact(value, turnBased)) &&
                        book.GetSpontaneousSlots(2) == 4 - casts.Length && book.GetSpontaneousSlots(1) == 5 && book.GetSpontaneousSlots(3) == 2 &&
                        JToken.DeepEquals(siblingBefore, siblingBudget()) && applications.Length > 0 &&
                        applications.All(value => (double?)value["durationSeconds"] == 42) && effect.Context.Params.CasterLevel == 7 &&
                        effect.Context.Params.DC == 16 && target.Descriptor.State.HasCondition(UnitCondition.Paralyzed) &&
                        policy.Score(choice) == 0 && (turnBased ? ticks == 0 && nativeTurns > 0 : ticks > 0);
                    Sprint25Check(id + "-natural-cast", castPass, new JObject { ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                        ["costs"] = costs, ["saves"] = saves.Rows, ["applications"] = saves.Applications,
                        ["casts"] = casts.Length, ["slotsAfter"] = book.GetSpontaneousSlots(2), ["siblingAfter"] = siblingBudget(),
                        ["aiTicks"] = ticks, ["nativeTurnFrames"] = nativeTurns, ["activeScore"] = policy.Score(choice),
                        ["effectEnd"] = effect?.EndTime.TotalSeconds, ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0 },
                        "Natural Hold uses normal Standard commands and one real L2 slot per cast, CL7/DC16/42s, paralysis and active-target refusal; sibling resources remain independent.");
                    if (!castPass) throw new InvalidOperationException("Natural Hold cast or cost failed.");
                }
                if (repeat)
                {
                    if (turnBased) throw new InvalidOperationException("Repeat-save observation selects RTWP native round adapter only.");
                    double originalEnd = effect.EndTime.TotalSeconds;
                    var limit = DateTime.UtcNow.AddSeconds(15);
                    while (DateTime.UtcNow < limit && repeats.Rows.Count == 0 && effect.Active && !effect.IsDisposed)
                    { game.IsPaused = false; yield return 0; }
                    bool failedFirst = repeats.Rows.Any(value => !(bool)value["passed"]) && effect.Active && !effect.IsDisposed;
                    // A prospective fixture stat change permits the next native save;
                    // no event result, roll, previous action or time is rewritten.
                    target.Stats.SaveWill.BaseValue = 100;
                    limit = DateTime.UtcNow.AddSeconds(20);
                    while (DateTime.UtcNow < limit && effect.Active && !effect.IsDisposed)
                    { game.IsPaused = false; yield return 0; }
                    bool retryPass = failedFirst && repeats.Rows.Any(value => (bool)value["passed"]) &&
                        repeats.Rows.All(value => (bool)value["costsUnchanged"] && (int)value["dc"] == 16 && !value["commands"].Any()) &&
                        (!effect.Active || effect.IsDisposed) && !target.Descriptor.State.HasCondition(UnitCondition.Paralyzed) &&
                        game.Player.GameTime.TotalSeconds < originalEnd;
                    Sprint25Check(id + "-automatic-save", retryPass, new JObject { ["repeats"] = repeats.Rows,
                        ["fixtureWillBefore"] = -100, ["fixtureWillAfterFirstRetry"] = 100, ["originalEnd"] = originalEnd,
                        ["gameSeconds"] = game.Player.GameTime.TotalSeconds, ["paralyzed"] = target.Descriptor.State.HasCondition(UnitCondition.Paralyzed),
                        ["ownerNativeCooldownTicks"] = _lillendHoldOwnerTicks32.TickCount, ["forcedRolls"] = 0 },
                        "Accepted native automatic repeat Will saves: real failed retry retains paralysis, later real success removes it before expiry without a victim command or action debit.");
                }
                else foreach (int frame in ReviewFeyControlExpiry32(fixture, owner, sibling, target, effect,
                    new AbilityData(ability, siblingBook), UnitCondition.Paralyzed, siblingBudget, id,
                    () => new JObject { ["passed"] = !repeats.Rows.Any(value => (bool)value["passed"]),
                        ["automaticSaves"] = repeats.Rows })) yield return frame;
            }
            finally
            {
                _lillendHoldOwnerTicks32 = null;
                if (starts != null) EventBus.Unsubscribe(starts); if (saves != null) EventBus.Unsubscribe(saves);
                EventBus.Unsubscribe(repeats);
                foreach (var actor in new[] { owner, sibling, target }.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true); if (actor.IsInCombat) actor.LeaveCombat(); }
                if (effect != null && !effect.IsDisposed) effect.Remove();
                game.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray());
                target.Stats.SaveWill.BaseValue = oldWill; ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                game.TurnBasedCombatController.Activate(); game.IsPaused = oldPause;
            }
        }
    }
}
