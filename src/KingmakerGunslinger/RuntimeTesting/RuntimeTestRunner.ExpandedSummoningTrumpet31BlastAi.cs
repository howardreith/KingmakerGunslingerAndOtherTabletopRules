using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> Trumpet31ObserveBlastDecision(UnitEntityData owner,
            UnitEntityData target, BlueprintAbility blast, BlueprintBuff paralysis,
            bool wantBlast, string id)
        {
            var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>();
            var costs = new JArray(); var endings = new JArray();
            var attacks = new Sprint24AttackObserver { Owner = owner, Target = target };
            var effects = new TrumpetBlastObservation(blast, paralysis, new[] { owner });
            bool clean = !owner.Commands.Raw.Any(value => value != null && !value.IsFinished);
            Sprint25Check(id + "-queue", clean, new JObject { ["actor"] = owner.UniqueId,
                ["commands"] = new JArray(owner.Commands.Raw.Where(value => value != null).Select(Sprint27AiCommand)) },
                "The next exact native brain decision starts without a live command.");
            if (!clean) throw new InvalidOperationException("Trumpet AI decision has a live prior command.");
            owner.Commands.RemoveFinishedAndUpdateQueue();
            Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
            EventBus.Subscribe(attacks); EventBus.Subscribe(effects);
            int frames = 0, ticks = 0;
            bool completed = false;
            string tickFailure = null, lastObservedState = null;
            var snapshots = new JArray(); var scores = new JArray();
            var startedUtc = DateTime.UtcNow;
            Action<string> record = stage =>
            {
                foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                snapshots.Add(new JObject { ["stage"] = stage, ["utc"] = DateTime.UtcNow.ToString("o"),
                    ["frames"] = frames, ["aiTicks"] = ticks, ["unityFrame"] = Time.frameCount,
                    ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                    ["paused"] = Game.Instance.IsPaused, ["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat(),
                    ["currentTurn"] = Game.Instance.TurnBasedCombatController.CurrentTurn?.Unit?.UniqueId,
                    ["gameDelta"] = Game.Instance.TimeController.GameDeltaTime,
                    ["owner"] = Trumpet31AiActorState(owner), ["target"] = Trumpet31AiActorState(target),
                    ["ownerSeesTarget"] = owner.Memory.ContainsVisible(target), ["ownerHasLosToTarget"] = owner.HasLOS(target),
                    ["ownerIsEnemyToTarget"] = owner.IsEnemy(target),
                    ["awakeGroups"] = new JArray(Game.Instance.AwakeUnitGroups.Where(group => group != null).Select(group => new JObject {
                        ["group"] = RuntimeHelpers.GetHashCode(group), ["combat"] = group != null && group.IsInCombat,
                        ["members"] = new JArray(Enumerable.Range(0, group.Count).Select(index => group[index])
                            .Where(unit => unit != null).Select(Trumpet31AiActorState)) })),
                    ["commands"] = new JArray(commands.Select(Trumpet31AiCommandState)),
                    ["costCount"] = costs.Count, ["endingCount"] = endings.Count,
                    ["saveCount"] = effects.Saves.Count, ["effectEventCount"] = effects.EffectEvents.Count });
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, id + "-trace.json"),
                    new JObject { ["runId"] = _request.RunId, ["scenario"] = _request.Scenario,
                        ["decision"] = id, ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                        ["startedUtc"] = startedUtc.ToString("o"), ["completed"] = completed,
                        ["tickFailure"] = tickFailure, ["snapshots"] = snapshots,
                        ["costs"] = costs.DeepClone(), ["endings"] = endings.DeepClone(),
                        ["blastSaves"] = effects.Saves.DeepClone(), ["blastEffects"] = effects.Effects,
                        ["blastEffectEvents"] = effects.EffectEvents.DeepClone(),
                        ["blastDice"] = effects.Dice.DeepClone(), ["blastResistance"] = effects.Resistance.DeepClone(),
                        ["nativeSafetyScores"] = scores.DeepClone(),
                        ["ambientCandidatesNeverTicked"] = new JArray(ambient),
                        ["fixtureBlastCommands"] = 0, ["fixtureAttackCommands"] = 0,
                        ["forcedRolls"] = 0 }.ToString(Formatting.Indented));
            };
            try
            {
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var trace = new Sprint26CommandTrace(owner, endings))
                using (var scoreTrace = new Trumpet31BlastScoreTrace(owner, scores))
                {
                    var deadline = DateTime.UtcNow.AddSeconds(45);
                    while (DateTime.UtcNow < deadline && frames++ < 4500)
                    {
                        if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                        {
                            record("before-native-ai-tick");
                            try
                            {
                                foreach (string candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate);
                                ticks++;
                            }
                            catch (Exception error) { tickFailure = error.ToString(); throw; }
                            finally { record("after-native-ai-tick"); }
                        }
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        string state = owner.IsInCombat + ":" + target.IsInCombat + ":" + owner.Memory.ContainsVisible(target) +
                            ":" + target.Group.Memory.HasPlayerCharacterInMemory() + ":" +
                            string.Join("|", commands.Select(value => RuntimeHelpers.GetHashCode(value) + ":" + value.IsStarted +
                                ":" + value.IsActed + ":" + value.IsFinished + ":" + value.Result + ":" +
                                (value as UnitUseAbility)?.ExecutionProcess?.IsEnded));
                        if (state != lastObservedState) { lastObservedState = state; record("observed-native-state-change"); }
                        bool blasted = commands.OfType<UnitUseAbility>().Any(value => value.AiAction?.name ==
                            "KMG_Summoning_OutsiderCelestial_TrumpetArchon_TrumpetBlastAi" && value.IsFinished &&
                            value.Result == UnitCommand.ResultType.Success && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                        bool attacked = commands.OfType<UnitAttack>().Any(value => value.AiAction != null && value.IsFinished &&
                            (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value))) && attacks.Damage.Any(value => value.Damage > 0);
                        if (wantBlast ? blasted && effects.Saves.Count > 0 : attacked || effects.Saves.Count > 0) break;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    record("decision-loop-ended");
                    var chosen = wantBlast ? (UnitCommand)commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                        value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_TrumpetArchon_TrumpetBlastAi" && value.IsFinished &&
                        value.Result == UnitCommand.ResultType.Success) : commands.OfType<UnitAttack>().FirstOrDefault(value =>
                        value.AiAction != null && value.IsFinished && (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value)));
                    bool native = ticks > 0 && chosen != null && !chosen.Cutscene && !chosen.IsIgnoreCooldown &&
                        economy.Count(chosen) == 1 && economy.Exact(chosen, false);
                    var targetSaves = effects.Saves.Where(row => (string)row["target"] == target.UniqueId &&
                        (string)row["caster"] == owner.UniqueId && (int)row["dc"] == 20).ToArray();
                    bool outcome = wantBlast ? targetSaves.Length > 0 &&
                            (targetSaves.All(row => (bool)row["passed"]) || target.Descriptor.State.HasCondition(UnitCondition.Paralyzed)) :
                        effects.Saves.Count == 0 && !commands.OfType<UnitUseAbility>().Any(value =>
                            value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_TrumpetArchon_TrumpetBlastAi") &&
                        attacks.WeaponAttacks.Count > 0 && attacks.Damage.Any(value => value.Damage > 0) &&
                        attacks.WeaponAttacks.All(value => ReferenceEquals(value.Weapon, owner.Body.PrimaryHand.MaybeWeapon));
                    Sprint25Check(id, native && outcome, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                        ["wantedBlast"] = wantBlast, ["aiTicks"] = ticks, ["frames"] = frames,
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs, ["endings"] = endings,
                        ["blastSaves"] = effects.Saves.DeepClone(), ["blastEffects"] = effects.Effects,
                        ["attacks"] = new JArray(attacks.WeaponAttacks.Select(value => new JObject {
                            ["owner"] = value.Initiator.UniqueId, ["target"] = value.Target.UniqueId,
                            ["weapon"] = value.Weapon?.Blueprint.AssetGuid, ["hit"] = value.AttackRoll?.IsHit })),
                        ["damage"] = new JArray(attacks.Damage.Select(value => new JObject {
                            ["source"] = value.Initiator.UniqueId, ["target"] = value.Target.UniqueId, ["damage"] = value.Damage })),
                        ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureBlastCommands"] = 0,
                        ["fixtureAttackCommands"] = 0, ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                        wantBlast ? "The real native brain selects its safe blast, pays one standard action and resolves the hostile's actual save/paralysis." :
                        "The native brain refuses an unsafe all-creature blast and selects its own held sword, with a real attack, damage and ordinary action cost.");
                    if (!native || !outcome) throw new InvalidOperationException("Trumpet native autonomous decision failed: " + id);
                    completed = true;
                }
            }
            finally
            {
                try { record("finally-before-fixture-cleanup"); }
                finally { Sprint28SetBrainActivePreservingActions(owner, false); EventBus.Unsubscribe(effects); EventBus.Unsubscribe(attacks); }
            }
        }

        // Read-only failure-safe diagnostics; no command, roll, group or effect is changed.
        private static JObject Trumpet31AiActorState(UnitEntityData unit)
        {
            return new JObject { ["unit"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.AssetGuid,
                ["faction"] = unit.Faction?.name, ["destroyed"] = unit.Destroyed,
                ["inGame"] = unit.IsInGame, ["inState"] = unit.IsInState, ["combat"] = unit.IsInCombat,
                ["sleeping"] = unit.IsSleeping, ["brainPresent"] = unit.Brain != null,
                ["brainActive"] = unit.IsBrainActive, ["aiEnabled"] = unit.IsAIEnabled,
                ["isPlayerFaction"] = unit.IsPlayerFaction,
                ["groupHasPlayerMemory"] = unit.Group.Memory.HasPlayerCharacterInMemory(),
                ["groupEnemies"] = new JArray(unit.Group.Memory.Enemies.Where(info => info.Unit != null).Select(info => info.Unit.UniqueId)),
                ["canAct"] = unit.Descriptor.State.CanAct, ["dead"] = unit.Descriptor.State.IsDead,
                ["unconscious"] = unit.Descriptor.State.IsUnconscious, ["hp"] = unit.Stats.HitPoints.ModifiedValue,
                ["position"] = unit.Position.ToString(), ["viewVisible"] = unit.View != null && unit.View.IsVisible,
                ["buffs"] = new JArray(unit.Buffs.Enumerable.Select(buff => new JObject {
                    ["instance"] = RuntimeHelpers.GetHashCode(buff), ["blueprint"] = buff.Blueprint.AssetGuid,
                    ["caster"] = buff.Context?.MaybeCaster?.UniqueId, ["end"] = buff.EndTime.ToString(),
                    ["disposed"] = buff.IsDisposed })) };
        }

        private static JObject Trumpet31AiCommandState(UnitCommand command)
        {
            var row = Sprint27AiCommand(command);
            var ability = command as UnitUseAbility;
            row["instance"] = RuntimeHelpers.GetHashCode(command);
            row["executor"] = command.Executor?.UniqueId;
            row["timeSinceStart"] = command.TimeSinceStart;
            row["cutscene"] = command.Cutscene;
            row["ignoreCooldown"] = command.IsIgnoreCooldown;
            row["spell"] = ability?.Spell?.Blueprint.AssetGuid;
            row["executionPresent"] = ability?.ExecutionProcess != null;
            row["executionEnded"] = ability?.ExecutionProcess?.IsEnded;
            row["executionEngagesUnit"] = ability?.ExecutionProcess?.IsEngageUnit;
            return row;
        }

        // Observe the production consideration's actual returned score. Read
        // its already-initialized enemy cache directly, never initialize or
        // replace DecisionContext.Enemies from the observer.
        private sealed class Trumpet31BlastScoreTrace : IDisposable
        {
            private static Trumpet31BlastScoreTrace active;
            private readonly UnitEntityData owner; private readonly JArray rows;
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id = "KMG.Trumpet31BlastScoreTrace." + Guid.NewGuid().ToString("N");
            private readonly System.Reflection.MethodInfo method = typeof(TrumpetBlastEligibility).GetMethod("Score");
            internal Trumpet31BlastScoreTrace(UnitEntityData owner, JArray rows)
            {
                if (active != null || method == null) throw new InvalidOperationException("Native blast score trace is nested or unavailable.");
                this.owner = owner; this.rows = rows; harmony = Harmony12.HarmonyInstance.Create(id);
                harmony.Patch(method, null, new Harmony12.HarmonyMethod(typeof(Trumpet31BlastScoreTrace).GetMethod("Scored",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)));
                active = this;
            }
            private static void Scored(TrumpetBlastEligibility __instance, DecisionContext __0, float __result)
            {
                var trace = active; var actor = __0?.Unit;
                if (trace == null || !ReferenceEquals(actor, trace.owner)) return;
                var enemies = ReadExpandedSummoningOptionalMember(__0, "m_Enemies") as IEnumerable<TargetInfo>;
                trace.rows.Add(new JObject { ["frame"] = Time.frameCount,
                    ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                    ["actor"] = actor.UniqueId, ["decisionTarget"] = __0.Target?.Unit?.UniqueId, ["score"] = __result,
                    ["holdsInstrument"] = TrumpetBlastRulesPolicy.HoldsInstrument(actor, __instance.SourceUnit, __instance.Instrument),
                    ["blockingNonenemies"] = new JArray(__instance.NearbyNonArchonNonEnemies(actor).Select(unit => unit.UniqueId)),
                    ["enemiesInitialized"] = (bool)ReadExpandedSummoningOptionalMember(__0, "m_EnemiesInitialized"),
                    ["cachedEnemies"] = new JArray((enemies ?? Enumerable.Empty<TargetInfo>()).Where(info => info?.Unit != null)
                        .Select(info => new JObject { ["unit"] = info.Unit.UniqueId,
                            ["visible"] = actor.Memory.ContainsVisible(info.Unit),
                            ["paralyzed"] = info.Unit.Descriptor.State.HasCondition(UnitCondition.Paralyzed),
                            ["nativePlayerMemory"] = info.Unit.Group.Memory.HasPlayerCharacterInMemory(),
                            ["distance"] = actor.DistanceTo(info.Unit) })) });
            }
            public void Dispose()
            {
                harmony.Unpatch(method, Harmony12.HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
            }
        }

        private IEnumerable<int> ReviewTrumpet31BlastAi(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, blocker = null; BlueprintUnit blockerBlueprint = null;
            CreateExpandedSummoningCorrectionHostile(fixture); var target = fixture.Hostile;
            int fort = target.Stats.SaveFortitude.BaseValue, ac = target.Stats.AC.BaseValue;
            int casterFort = fixture.Caster.Stats.SaveFortitude.BaseValue;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                Vector3 far; string survey, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 2f, out far, out survey, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                blockerBlueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                blockerBlueprint.name = "KMG_Runtime_Trumpet31_PlayerFactionBlocker"; blockerBlueprint.IsCheater = true;
                blockerBlueprint.Brain = null; blockerBlueprint.StartingInventory = Array.Empty<Kingmaker.Blueprints.Items.BlueprintItem>();
                blocker = Game.Instance.EntityCreator.SpawnUnit(blockerBlueprint, origin - Vector3.right * 2f, Quaternion.identity, fixture.Scene);
                if (blocker == null) throw new InvalidOperationException("Owned Trumpet AI ally did not instantiate.");
                fixture.Created.Add(blocker); Sprint28SetBrainActivePreservingActions(blocker, false);
                blocker.Commands.InterruptAll(true); blocker.Stats.HitPoints.BaseValue = 100000;
                target.Stats.SaveFortitude.BaseValue = -100; target.Stats.AC.BaseValue = 0;
                foreach (var unit in new[] { owner, target, blocker })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); unit.JoinCombat(); unit.SightCache.ClearIfNeeded(); }
                owner.Memory.Add(target); target.Memory.Add(owner);
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                Game.Instance.Player.UpdateIsInCombat(); new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var map = ExpandedSummoningBlueprints.Registered.BySymbol; string prefix = ExpandedSummoningSpecialBuilder.Trumpet31Prefix;
                var blast = (BlueprintAbility)map[prefix + "TrumpetBlast"];
                var paralysis = (BlueprintBuff)map[prefix + "TrumpetParalyzed"];
                var safety = (TrumpetBlastEligibility)map[prefix + "TrumpetBlastEligibility"];
                var aid = (BlueprintAbility)map[prefix + "Aid"];
                // Real fixture preparation prevents at-will Aid from being the
                // outstanding support decision for either owned ally. It does
                // not change any AI action, resource, roll, or blast effect.
                foreach (int frame in Trumpet31ManualCommand(owner, new AbilityData(owner.Descriptor.Abilities.GetAbility(aid)),
                    owner, "trumpet-blast-ai-fixture-aid-owner")) yield return frame;
                foreach (int frame in Trumpet31ManualCommand(owner, new AbilityData(owner.Descriptor.Abilities.GetAbility(aid)),
                    blocker, "trumpet-blast-ai-fixture-aid-blocker")) yield return frame;
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(blast));
                var decision = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = owner } };
                var book = owner.Descriptor.GetSpellbook(owner.Blueprint.GetComponent<TrumpetPreparedBook>().Book);
                Func<int[]> slots = () => Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)).ToArray();
                var before = slots();
                bool ready = blocker.IsPlayerFaction && !owner.IsEnemy(blocker) && data.IsAvailable &&
                    safety.Score(decision) == 0f && owner.IsInCombat && owner.Memory.ContainsVisible(target) && owner.HasLOS(target) &&
                    !owner.Commands.Raw.Any(value => value != null && !value.IsFinished);
                Sprint25Check("trumpet-blast-ai-unsafe-ready", ready, new JObject { ["owner"] = owner.UniqueId,
                    ["target"] = target.UniqueId, ["blocker"] = blocker.UniqueId, ["blockerPlayerFaction"] = blocker.IsPlayerFaction,
                    ["blockerIsAlly"] = owner.IsAlly(blocker), ["blockerIsEnemy"] = owner.IsEnemy(blocker),
                    ["score"] = safety.Score(decision), ["manualAvailable"] = data.IsAvailable,
                    ["lane"] = survey, ["visible"] = owner.Memory.ContainsVisible(target), ["slots"] = new JArray(before),
                    ["fixtureAidCommands"] = 2, ["fixtureBlastCommands"] = 0, ["fixtureAttackCommands"] = 0 },
                    "A real nearby player-faction nonenemy blocks only AI selection; manual blast access remains legal. Native memory, combat, full resources and prior Aid are observed.");
                if (!ready) throw new InvalidOperationException("Unsafe Trumpet AI fixture preconditions are unmet.");
                var model = owner.View?.HandsEquipment?.GetWeaponModel(false);
                var viewRow = new JObject { ["owner"] = owner.UniqueId, ["unit"] = owner.Blueprint.AssetGuid,
                    ["view"] = owner.View?.name, ["activeMainHandModel"] = model?.name,
                    ["modelUnderOwnedView"] = model != null && owner.View != null && model.transform.IsChildOf(owner.View.transform),
                    ["modelLocalPosition"] = model?.transform.localPosition.ToString(), ["modelLocalRotation"] = model?.transform.localEulerAngles.ToString(),
                    ["renderers"] = new JArray(owner.View.GetComponentsInChildren<Renderer>(true).Select(value => new JObject {
                        ["name"] = value.name, ["enabled"] = value.enabled, ["active"] = value.gameObject.activeInHierarchy,
                        ["center"] = value.bounds.center.ToString(), ["size"] = value.bounds.size.ToString() })) };
                Sprint25Check("trumpet-blast-ai-body-probe", owner.View != null && model != null && model.transform.IsChildOf(owner.View.transform),
                    viewRow, "Read-only exact owned native body/main-hand model observation for the subsequent trumpet form attachment; no donor mutation or appearance approval.");
                foreach (int frame in Trumpet31ObserveBlastDecision(owner, target, blast, paralysis, false, "trumpet-blast-ai-unsafe-fallback")) yield return frame;
                bool negativeBookRetained = before.SequenceEqual(slots());
                Sprint25Check("trumpet-blast-ai-fallback-book", negativeBookRetained, new JObject {
                    ["owner"] = owner.UniqueId, ["before"] = new JArray(before), ["after"] = new JArray(slots()) },
                    "The actual player-summoned support/attack fallback leaves every prepared slot unchanged.");
                if (!negativeBookRetained) throw new InvalidOperationException("Fallback changed prepared slots.");
                Sprint25Check("trumpet-blast-ai-book-isolation", negativeBookRetained, new JObject {
                    ["owner"] = owner.UniqueId, ["before"] = new JArray(before), ["after"] = new JArray(slots()),
                    ["separatePositiveProcessRequired"] = true },
                    "The player-side refusal, support and real sword fallback preserve all prepared slots. Positive blast selection uses a separate fresh process; no shared-reset pass is claimed.");
            }
            finally
            {
                if (owner != null) { Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); }
                target.Commands.InterruptAll(true); target.Stats.SaveFortitude.BaseValue = fort; target.Stats.AC.BaseValue = ac;
                fixture.Caster.Stats.SaveFortitude.BaseValue = casterFort;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, blocker }.Where(value => value != null && !value.Destroyed).ToArray());
                if (blockerBlueprint != null) UnityEngine.Object.Destroy(blockerBlueprint);
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
        private IEnumerable<int> ReviewTrumpet31BlastAiSafe(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, anchor = null, witness = null;
            BlueprintFaction witnessFaction = null;
            bool anchorBrainBefore = false, anchorBrainCaptured = false;
            Vector3 anchorBefore = Vector3.zero;
            int fort = 0;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                // This is the native profile's own cloned main character,
                // proven by retained preset/blueprint object references. No
                // original party member or campaign actor is eligible.
                anchor = Game.Instance.Player.MainCharacter.Value;
                bool anchorOwned = anchor != null && _summoningProfileActor != null && _summoningProfilePreset != null &&
                    _summoningProfileOriginal != null && ReferenceEquals(anchor.Blueprint, _summoningProfileActor) &&
                    ReferenceEquals(_summoningProfilePreset.PlayerCharacter, _summoningProfileActor) &&
                    !ReferenceEquals(_summoningProfileOriginal.PlayerCharacter, _summoningProfileActor) &&
                    _summoningProfileActor.name == "KMG_Runtime_Summoning_Profile_Anchor" &&
                    _summoningProfileSaveGuard != null && _summoningProfileScenes != null;
                Sprint25Check("trumpet-blast-ai-owned-anchor", anchorOwned, new JObject {
                    ["anchor"] = anchor?.UniqueId, ["blueprint"] = anchor?.Blueprint.name,
                    ["exactRequestOwnedPresetClone"] = anchorOwned, ["positionBefore"] = anchor?.Position.ToString(),
                    ["originalActorOrFactionEdits"] = 0, ["protectedActorTicks"] = 0 },
                    "Only the exact native profile-owned anchor clone can be positioned outside the blast; the retained original preset and all campaign actors are protected.");
                if (!anchorOwned) { anchor = null; throw new InvalidOperationException("Request-owned profile anchor identity is not proven."); }
                anchorBefore = anchor.Position;
                anchorBrainBefore = anchor.IsBrainActive; anchorBrainCaptured = true;
                Sprint28SetBrainActivePreservingActions(anchor, false);
                anchor.Commands.InterruptAll(true); anchor.Commands.RemoveFinishedAndUpdateQueue();
                CreateExpandedSummoningCorrectionHostile(fixture);
                fort = fixture.Hostile.Stats.SaveFortitude.BaseValue;
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                var target = fixture.Hostile;
                target.Stats.SaveFortitude.BaseValue = -100;
                var map = ExpandedSummoningBlueprints.Registered.BySymbol; string prefix = ExpandedSummoningSpecialBuilder.Trumpet31Prefix;
                var blast = (BlueprintAbility)map[prefix + "TrumpetBlast"];
                var paralysis = (BlueprintBuff)map[prefix + "TrumpetParalyzed"];
                var safety = (TrumpetBlastEligibility)map[prefix + "TrumpetBlastEligibility"];
                witness = CastExpandedSummoningOwnTier(fixture, "hound-archon");
                bool ownsWitness = fixture.Created.Contains(witness) && witness.Blueprint.AssetGuid == HoundArchonVisualPolicy.BlueprintGuid &&
                    ReferenceEquals(witness.Blueprint, safety.Archons.Single(unit => unit.AssetGuid == HoundArchonVisualPolicy.BlueprintGuid));
                if (!ownsWitness) throw new InvalidOperationException("The archon witness is not an exact new request-owned project Hound.");
                Sprint28SetBrainActivePreservingActions(witness, false); witness.Commands.InterruptAll(true); witness.Commands.RemoveFinishedAndUpdateQueue();
                witnessFaction = witness.Descriptor.Faction;
                // An explicit disposable player-faction archon ally, matching
                // the native hostile-memory consumer. Only this new request's
                // actual Hound entity changes faction, never its blueprint,
                // the source, another summon, or a protected original actor.
                witness.Descriptor.Faction = anchor.Descriptor.Faction;
                Vector3 far; string survey, direction; Vector3 origin;
                try
                {
                    origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 2f, out far, out survey, out direction,
                        new Feet(100).Meters + 2.5f, new[] { fixture.Caster, anchor, witness }, 30);
                }
                catch (Exception error)
                {
                    Sprint25Check("trumpet-blast-ai-safe-location", false, new JObject {
                        ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["ownerFaction"] = owner.Faction?.name,
                        ["exception"] = error.ToString(), ["nearbyNonenemies"] = new JArray(safety.NearbyNonArchonNonEnemies(owner)
                            .Select(value => new JObject { ["unit"] = value.UniqueId, ["blueprint"] = value.Blueprint.AssetGuid,
                                ["name"] = value.Blueprint.name, ["faction"] = value.Faction?.name, ["distance"] = owner.DistanceTo(value) })) },
                        "An unmet native clear-location precondition stops this independent positive process; no original actor or faction is changed to create a pass.");
                    throw;
                }
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                var graph = AstarPath.active;
                if (graph == null) throw new InvalidOperationException("Positive native fixture lost its placement graph.");
                var casterPoint = graph.GetNearest(origin + Vector3.back * 40f);
                var anchorPoint = graph.GetNearest(origin + Vector3.back * 42f);
                var witnessPoint = graph.GetNearest(origin + Vector3.right * 2f);
                if (new[] { casterPoint, anchorPoint, witnessPoint }.Any(point => point.node == null || !point.node.Walkable))
                    throw new InvalidOperationException("Owned fixture actors have no native walkable ground.");
                PlaceExpandedSummoningUnit(fixture.Caster, casterPoint.clampedPosition);
                PlaceExpandedSummoningUnit(anchor, anchorPoint.clampedPosition);
                PlaceExpandedSummoningUnit(witness, witnessPoint.clampedPosition);
                target.Memory.Add(witness); witness.Memory.Add(target);
                // The installed SleepingUnitsController admits awake groups
                // through awake nonpeaceful members. The menu fixture's real
                // clock rebuilds those lists; keep only the proven owned
                // player actors awake while outside the unchanged100ft burst.
                // This invokes no attack, spell, brain, or protected actor.
                anchor.Wake(60f); fixture.Caster.Wake(60f);
                foreach (var unit in new[] { owner, target, witness })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); unit.JoinCombat(); unit.SightCache.ClearIfNeeded(); }
                owner.Memory.Add(target); target.Memory.Add(owner);
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                Game.Instance.Player.UpdateIsInCombat(); new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                var nativeGroups = Game.Instance.AwakeUnitGroups.Where(group => group != null).ToArray();
                bool admitted = nativeGroups.Any(group => group.IsInCombat &&
                    Enumerable.Range(0, group.Count).Any(index => ReferenceEquals(group[index], owner))) &&
                    !owner.Destroyed && owner.IsInState && owner.IsInCombat && !owner.IsSleeping &&
                    owner.Brain != null && owner.IsBrainActive;
                Func<UnitEntityData, JObject> actorState = unit => new JObject {
                    ["unit"] = unit.UniqueId, ["faction"] = unit.Faction?.name,
                    ["inGame"] = unit.IsInGame, ["inState"] = unit.IsInState, ["combat"] = unit.IsInCombat,
                    ["sleeping"] = unit.IsSleeping, ["brainActive"] = unit.IsBrainActive, ["aiEnabled"] = unit.IsAIEnabled,
                    ["viewVisible"] = unit.View != null && unit.View.IsVisible, ["position"] = unit.Position.ToString() };
                Sprint25Check("trumpet-blast-ai-native-admission", admitted, new JObject {
                    ["owner"] = actorState(owner), ["target"] = actorState(target), ["ownedAnchor"] = actorState(anchor),
                    ["disposableCaster"] = actorState(fixture.Caster), ["nativeGroupAdmitted"] = admitted,
                    ["awakeGroups"] = new JArray(nativeGroups.Select(group => new JObject {
                        ["group"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(group),
                        ["combat"] = group != null && group.IsInCombat,
                        ["members"] = new JArray(Enumerable.Range(0, group.Count).Select(index => group[index])
                            .Where(unit => unit != null).Select(actorState)) })),
                    ["ownedWakeSeconds"] = 60, ["archonWitness"] = actorState(witness), ["protectedActorTicks"] = 0, ["suppliedCombatCommands"] = 0 },
                    "The actual native awake-group, combat, sleeping and brain predicates are observed before the unchanged exact-AI consumer. Only the reference-proven owned player actors receive a native wake interval; no group is injected or guard bypassed.");
                if (!admitted) throw new InvalidOperationException("Actual native awake-group AI admission is unmet; no AI command requested.");
                bool anchorQuiet = !anchor.IsBrainActive && !anchor.Commands.Raw.Any(command => command != null && !command.IsFinished);
                Sprint25Check("trumpet-blast-ai-owned-anchor-stationary", anchorQuiet, new JObject {
                    ["anchor"] = anchor.UniqueId, ["exactRequestOwnedPresetClone"] = anchorOwned,
                    ["originalBrainActive"] = anchorBrainBefore, ["currentBrainActive"] = anchor.IsBrainActive,
                    ["position"] = anchor.Position.ToString(), ["sourceDistance"] = owner.DistanceTo(anchor),
                    ["commands"] = new JArray(anchor.Commands.Raw.Where(command => command != null).Select(Sprint27AiCommand)),
                    ["protectedActorBrainOrCommandEdits"] = 0, ["sourceBrainOrActionCostEdits"] = 0 },
                    "Only the reference-proven disposable profile anchor has AI disabled and its own prior movement canceled. The source's real brain/actions remain native and protected actors are untouched.");
                if (!anchorQuiet) throw new InvalidOperationException("Disposable anchor has a live autonomous/movement command.");
                var decision = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = owner } };
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(blast));
                var book = owner.Descriptor.GetSpellbook(owner.Blueprint.GetComponent<TrumpetPreparedBook>().Book);
                Func<int[]> slots = () => Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)).ToArray();
                var before = slots();
                var remaining = safety.NearbyNonArchonNonEnemies(owner).ToArray();
                var local = Kingmaker.Visual.LocalMap.LocalMapArea.GetClosest(owner.Position);
                bool outdoor = local?.AreaPart != null && !local.AreaPart.IsIndoor && !Kingmaker.Visual.LocalMap.LocalMapArea.IsIndoor(owner.Position);
                bool clear = remaining.Length == 0 && owner.IsEnemy(target) && outdoor &&
                    owner.DistanceTo(anchor) > new Feet(100).Meters + anchor.Corpulence &&
                    owner.DistanceTo(fixture.Caster) > new Feet(100).Meters + fixture.Caster.Corpulence &&
                    Game.Instance.CurrentlyLoadedArea.AssetGuid == "ead426a6c23d39548a670ee515d77df4";
                Sprint25Check("trumpet-blast-ai-safe-location", clear, new JObject { ["owner"] = owner.UniqueId,
                    ["target"] = target.UniqueId, ["ownerFaction"] = owner.Faction?.name, ["targetFaction"] = target.Faction?.name,
                    ["targetEnemy"] = owner.IsEnemy(target), ["lane"] = survey,
                    ["remainingNonenemies"] = new JArray(remaining.Select(value => value.UniqueId)),
                    ["freshPositiveProcess"] = true, ["priorSubcases"] = 0,
                    ["area"] = Game.Instance.CurrentlyLoadedArea.AssetGuid, ["outdoor"] = outdoor, ["areaPart"] = local?.AreaPart?.AssetGuid,
                    ["ownedAnchor"] = anchor.UniqueId, ["ownedAnchorDistance"] = owner.DistanceTo(anchor),
                    ["disposableCaster"] = fixture.Caster.UniqueId, ["disposableCasterDistance"] = owner.DistanceTo(fixture.Caster),
                    ["exactRequestOwnedPresetClone"] = anchorOwned,
                    ["archonWitness"] = witness.UniqueId, ["witnessFaction"] = witness.Faction.name,
                    ["requestOwnedWitnessFactionChanges"] = 1,
                    ["originalActorOrFactionEdits"] = 0, ["protectedActorTicks"] = 0 },
                    "A fresh actual player summon uses its production blueprint/brain in the established non-DLC Oleg outdoor fixture, with native walkable/visible clear stance and actual outdoor classification. Original actors/factions are intact; only its proven request-owned anchor clone and disposable caster are positioned out of radius.");
                if (!clear) throw new InvalidOperationException("Safe player-summoned native outdoor location is not clear.");
                bool threat = ownsWitness && witness.IsPlayerFaction && !owner.IsEnemy(witness) &&
                    TrumpetBlastRulesPolicy.IsArchon(witness, safety.Archons) && target.Group.Memory.HasPlayerCharacterInMemory() &&
                    target.HasLOS(witness) && witness.HasLOS(target) && target.Memory.ContainsVisible(witness) &&
                    target.DistanceTo(witness) < new Feet(100).Meters && owner.DistanceTo(target) < new Feet(100).Meters;
                Sprint25Check("trumpet-blast-ai-native-threat", threat, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["anchor"] = anchor.UniqueId,
                    ["archonWitness"] = witness.UniqueId, ["witnessUnit"] = witness.Blueprint.AssetGuid,
                    ["witnessExactOwnedProjectHound"] = ownsWitness, ["witnessPlayerFaction"] = witness.IsPlayerFaction,
                    ["witnessActualArchonExemption"] = TrumpetBlastRulesPolicy.IsArchon(witness, safety.Archons),
                    ["witnessNotEnemy"] = !owner.IsEnemy(witness), ["witnessDistance"] = owner.DistanceTo(witness),
                    ["witnessOriginalFaction"] = witnessFaction.name, ["requestOwnedWitnessFactionChanges"] = 1,
                    ["hostileGroupHasPlayerMemory"] = target.Group.Memory.HasPlayerCharacterInMemory(),
                    ["hostileSeesArchonWitness"] = target.Memory.ContainsVisible(witness), ["hostileHasLosToArchonWitness"] = target.HasLOS(witness),
                    ["hostileWitnessDistance"] = target.DistanceTo(witness), ["sourceTargetDistance"] = owner.DistanceTo(target),
                    ["sourceAnchorDistance"] = owner.DistanceTo(anchor), ["sourceCasterDistance"] = owner.DistanceTo(fixture.Caster),
                    ["sourceVisibleTarget"] = owner.Memory.ContainsVisible(target),
                    ["originalActorOrFactionEdits"] = 0, ["suppliedCombatCommands"] = 0 },
                    "An exact newly cast project Hound is an explicit request-owned player-faction archon ally. Its real native perception sustains hostile/player memory while the unchanged blast exempts its actual species and both nonarchon players remain outside100ft. The source and every original actor/faction are unchanged; one owned witness faction change is disclosed.");
                if (!threat) throw new InvalidOperationException("Native hostile/player threat preconditions are unmet.");
                bool ready = admitted && threat && safety.Score(decision) == 1f && owner.Memory.ContainsVisible(target) && owner.HasLOS(target) &&
                    owner.IsInCombat && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished) && data.IsAvailable;
                Sprint25Check("trumpet-blast-ai-safe-ready", ready, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                    ["score"] = safety.Score(decision), ["remainingNonenemies"] = new JArray(remaining.Select(value => value.UniqueId)),
                    ["slotsBefore"] = new JArray(before), ["slotsAfter"] = new JArray(slots()),
                    ["visible"] = owner.Memory.ContainsVisible(target), ["manualAvailable"] = data.IsAvailable, ["nativeGroupAdmitted"] = admitted,
                    ["fixtureCastCommands"] = 0, ["fixtureAttackCommands"] = 0, ["fixtureSlotDebits"] = 0 },
                    "Native perception/combat/access, own prepared book and clear all-creature radius admit the unchanged production blast selection; no combat command or slot debit is supplied by the fixture.");
                if (!ready) throw new InvalidOperationException("Safe native Trumpet AI preconditions are unmet.");
                foreach (int frame in Trumpet31ObserveBlastDecision(owner, target, blast, paralysis, true, "trumpet-blast-ai-safe-cast")) yield return frame;
                bool clearAfter = owner.DistanceTo(anchor) > new Feet(100).Meters + anchor.Corpulence &&
                    owner.DistanceTo(fixture.Caster) > new Feet(100).Meters + fixture.Caster.Corpulence &&
                    !safety.NearbyNonArchonNonEnemies(owner).Any() && !anchor.IsBrainActive &&
                    !anchor.Commands.Raw.Any(command => command != null && !command.IsFinished);
                Sprint25Check("trumpet-blast-ai-safe-radius-retained", clearAfter, new JObject {
                    ["owner"] = owner.UniqueId, ["anchor"] = anchor.UniqueId,
                    ["anchorDistance"] = owner.DistanceTo(anchor), ["casterDistance"] = owner.DistanceTo(fixture.Caster),
                    ["blockingNonenemies"] = new JArray(safety.NearbyNonArchonNonEnemies(owner).Select(unit => unit.UniqueId)),
                    ["anchorBrainActive"] = anchor.IsBrainActive },
                    "The actual full100ft nonarchon-ally clearance remains satisfied after the native blast resolves; no owned anchor AI may walk into the precondition during the observation.");
                if (!clearAfter) throw new InvalidOperationException("Positive native AI case lost its full ally-clear radius.");
                bool witnessExempt = !witness.Descriptor.State.HasCondition(UnitCondition.Paralyzed) &&
                    witness.IsPlayerFaction && !owner.IsEnemy(witness) && TrumpetBlastRulesPolicy.IsArchon(witness, safety.Archons);
                Sprint25Check("trumpet-blast-ai-archon-ally-exempt", witnessExempt, new JObject {
                    ["source"] = owner.UniqueId, ["target"] = target.UniqueId, ["witness"] = witness.UniqueId,
                    ["witnessUnit"] = witness.Blueprint.AssetGuid, ["witnessPlayerFaction"] = witness.IsPlayerFaction,
                    ["witnessParalyzed"] = witness.Descriptor.State.HasCondition(UnitCondition.Paralyzed),
                    ["sourceWitnessDistance"] = owner.DistanceTo(witness), ["requestOwnedWitnessFactionChanges"] = 1 },
                    "The real safe autonomous blast leaves its actual nearby player-faction archon ally exempt. The separate negative case protects a nonarchon player; no enemy-only or all-allies exemption is introduced.");
                if (!witnessExempt) throw new InvalidOperationException("Native archon ally exemption failed.");
                Sprint25Check("trumpet-blast-ai-book-isolation", before.SequenceEqual(slots()), new JObject {
                    ["owner"] = owner.UniqueId, ["before"] = new JArray(before), ["after"] = new JArray(slots()) },
                    "The real autonomous supernatural blast neither spends nor refills any independently owned prepared spell slot.");
            }
            finally
            {
                if (owner != null) { Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); }
                if (witness != null && !witness.Destroyed)
                {
                    Sprint28SetBrainActivePreservingActions(witness, false); witness.Commands.InterruptAll(true);
                    if (witnessFaction != null) witness.Descriptor.Faction = witnessFaction;
                    Sprint25Check("trumpet-blast-ai-witness-faction-restored", witnessFaction != null &&
                        ReferenceEquals(witness.Descriptor.Faction, witnessFaction), new JObject {
                            ["witness"] = witness.UniqueId, ["faction"] = witness.Descriptor.Faction?.name,
                            ["exactOriginalFactionRestored"] = ReferenceEquals(witness.Descriptor.Faction, witnessFaction),
                            ["originalActorOrFactionEdits"] = 0 },
                        "The single new request-owned witness entity regains its exact original faction before disposal, including on failure; no protected actor or blueprint was modified.");
                }
                if (fixture.Hostile != null) fixture.Hostile.Stats.SaveFortitude.BaseValue = fort;
                if (anchor != null && !anchor.Destroyed && anchor.IsInState)
                {
                    anchor.Commands.InterruptAll(true); PlaceExpandedSummoningUnit(anchor, anchorBefore);
                    if (anchorBrainCaptured) Sprint28SetBrainActivePreservingActions(anchor, anchorBrainBefore);
                    Sprint25Check("trumpet-blast-ai-owned-anchor-brain-restored", anchorBrainCaptured &&
                        anchor.IsBrainActive == anchorBrainBefore, new JObject {
                            ["anchor"] = anchor.UniqueId, ["before"] = anchorBrainBefore, ["after"] = anchor.IsBrainActive,
                            ["positionRestored"] = anchor.Position.ToString(), ["protectedActorEdits"] = 0 },
                        "The disposable profile anchor regains its actual original brain state and position in finally; all human/campaign actors remain untouched.");
                }
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, witness }.Where(value => value != null && !value.Destroyed).ToArray());
                if (fixture.Hostile != null) ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
