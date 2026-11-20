using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// S5 T6 / RISK-06 implementation qualification. The Mastodon is created by
    /// the disclosed own-tier fixture helper; during combat the observer never
    /// issues it a command, never ticks its AI and never supplies an action.
    /// Each cell asserts what the Mastodon's own native AI must do: select and
    /// execute the owned Trample action on an eligible approach, and keep the
    /// ordinary attack whenever the conservative predicate refuses.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string Risk06NativeMeleeWeaponGuid = "f28f6031c2908d84d945865a80f67177";

        private sealed class Risk06CommandLog : IUnitCommandStartHandler, IUnitCommandEndHandler
        {
            internal UnitEntityData Owner;
            internal UnitEntityData Target;
            internal BlueprintAbility Trample;
            internal readonly List<UnitCommand> Started = new List<UnitCommand>();
            internal readonly JArray Rows = new JArray();
            internal readonly Dictionary<UnitCommand, JObject> Starts = new Dictionary<UnitCommand, JObject>();
            internal readonly List<UnitCommand> Issued = new List<UnitCommand>();

            /// <summary>Records every command the owner holds, started or still approaching.</summary>
            internal void Scan()
            {
                if (Owner == null || Owner.Commands == null) return;
                foreach (UnitCommand command in Owner.Commands.Raw.Concat(Owner.Commands.Queue))
                {
                    if (command == null || Issued.Contains(command)) continue;
                    Issued.Add(command);
                    Rows.Add(Risk06Command(command, "issued"));
                }
            }

            internal bool IsTrample(UnitCommand command)
            {
                var use = command as UnitUseAbility;
                return use != null && Trample != null && ReferenceEquals(use.Spell?.Blueprint, Trample);
            }

            public void HandleUnitCommandDidStart(UnitCommand command)
            {
                if (command == null || !ReferenceEquals(command.Executor, Owner)) return;
                if (!Started.Contains(command)) Started.Add(command);
                JObject row = Risk06Command(command, "start");
                if (IsTrample(command))
                {
                    bool claimed, halted;
                    ContextActionUngulateTrample.ReadLedger(Owner, Target?.UniqueId, out claimed, out halted);
                    row["ledgerTargetClaimedAtStart"] = claimed; row["ledgerHaltedAtStart"] = halted;
                    row["fullRoundAtStart"] = Owner.HasFullRoundAction();
                }
                Starts[command] = row;
                Rows.Add(row);
            }

            public void HandleUnitCommandDidEnd(UnitCommand command)
            {
                if (command == null || !ReferenceEquals(command.Executor, Owner)) return;
                JObject row = Risk06Command(command, "end");
                if (IsTrample(command))
                {
                    row["fullRoundAfter"] = Owner.HasFullRoundAction();
                    row["standardAfter"] = Owner.HasStandardAction();
                }
                Rows.Add(row);
            }
        }

        private static JObject Risk06Command(UnitCommand command, string edge)
        {
            var use = command as UnitUseAbility;
            UnitEntityData owner = command.Executor;
            return new JObject { ["edge"] = edge, ["type"] = command.GetType().Name,
                ["aiAction"] = command.AiAction?.name, ["ability"] = use?.Spell?.Blueprint?.name,
                ["target"] = command.TargetUnit?.UniqueId,
                ["point"] = command.Target == null || command.Target.Unit != null ? null : Risk06Vector(command.Target.Point),
                ["position"] = owner == null ? null : Risk06Vector(owner.Position),
                ["commandType"] = command.Type.ToString(),
                ["started"] = command.IsStarted, ["acted"] = command.IsActed, ["finished"] = command.IsFinished,
                ["result"] = command.Result.ToString(), ["cutscene"] = command.Cutscene,
                ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                ["round"] = ContextActionUngulateTrample.CurrentRound() };
        }

        private IEnumerable<int> ReviewRisk06TrampleAi(ExpandedSummoningCorrectionFixture fixture, string phase,
            Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39TrampleAiPhase ||
                !ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(_request.Scenario, phase) ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh RISK-06 fixture in owned outdoor native Default, without narrative, required.");
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var unit = (BlueprintUnit)symbols[MastodonTrampleAiPolicy.UnitSymbol];
            var trample = (BlueprintAbility)symbols[SummonPlaytestR5Policy.MastodonTrampleSymbol];
            var ai = (UngulateTrampleAi)symbols[MastodonTrampleAiPolicy.AiSymbol];
            var brain = (BlueprintBrain)symbols[MastodonTrampleAiPolicy.BrainSymbol];
            var library = BlueprintBootstrap.Library.BlueprintsByAssetId.Values.Concat(symbols.Values)
                .Where(value => value != null).Distinct().ToArray();
            var nativeBrain = library.OfType<BlueprintBrain>().Single(value =>
                value.AssetGuid == MastodonTrampleAiPolicy.NativeBrainGuid);
            BlueprintAiAction[] nativeActionsBefore = (nativeBrain.Actions ?? new BlueprintAiAction[0]).ToArray();

            Risk06AiCensus(library, unit, trample, ai, brain, nativeBrain);

            var cells = new[] {
                new { Id = "positive-rtwp", TurnBased = false, Arrangement = "positive" },
                new { Id = "positive-turn-based", TurnBased = true, Arrangement = "positive" },
                new { Id = "other-enemy-in-reach-rtwp", TurnBased = false, Arrangement = "other-in-reach" },
                new { Id = "other-enemy-in-reach-turn-based", TurnBased = true, Arrangement = "other-in-reach" },
                new { Id = "threatened-rtwp", TurnBased = false, Arrangement = "threatened" },
                new { Id = "larger-target-rtwp", TurnBased = false, Arrangement = "larger-target" },
                new { Id = "no-full-round-turn-based", TurnBased = true, Arrangement = "staggered" },
                new { Id = "endpoint-occupied-rtwp", TurnBased = false, Arrangement = "endpoint-occupied" },
                new { Id = "death-mid-trample-rtwp", TurnBased = false, Arrangement = "death" } };
            foreach (var cell in cells)
                foreach (int frame in Risk06AiCell(fixture, mode, unit, trample, ai, cell.Id, cell.TurnBased, cell.Arrangement))
                    yield return frame;

            bool nativeUnchanged = (nativeBrain.Actions ?? new BlueprintAiAction[0]).SequenceEqual(nativeActionsBefore) &&
                ReferenceEquals(unit.Brain, brain);
            Sprint25Check("risk06-ai-shared-brain-after-cells", nativeUnchanged, new JObject {
                ["nativeBrainActions"] = new JArray((nativeBrain.Actions ?? new BlueprintAiAction[0]).Select(value => value?.name)),
                ["mastodonBrain"] = unit.Brain?.name },
                "After every cell the shared native brain still holds exactly its original actions and the Mastodon still uses its private brain.");
        }

        private void Risk06AiCensus(BlueprintScriptableObject[] library, BlueprintUnit unit, BlueprintAbility trample,
            UngulateTrampleAi ai, BlueprintBrain brain, BlueprintBrain nativeBrain)
        {
            // 1. Ownership: the private brain is the native one plus the owned
            // action; the shared native brain and every other unit are unchanged.
            BlueprintAiAction[] actions = brain.Actions ?? new BlueprintAiAction[0];
            BlueprintAiAction[] native = nativeBrain.Actions ?? new BlueprintAiAction[0];
            bool brainExact = ReferenceEquals(unit.Brain, brain) && actions.Length == native.Length + 1 &&
                actions.Take(native.Length).SequenceEqual(native) && ReferenceEquals(actions.Last(), ai);
            bool aiExact = ReferenceEquals(ai.Ability, trample) && ai.Variant == null && ReferenceEquals(ai.OwningBlueprint, unit) &&
                ai.CreatureKey == MastodonTrampleAiPolicy.CreatureKey && Math.Abs(ai.BaseScore - MastodonTrampleAiPolicy.BaseScore) < 1e-4f &&
                ai.CooldownRounds == 0 && ai.CombatCount == 0 && (ai.Locators == null || ai.Locators.Length == 0);
            string[] brainsHoldingAi = library.OfType<BlueprintBrain>().Where(value => value.Actions != null &&
                value.Actions.Contains(ai)).Select(value => value.name + ":" + value.AssetGuid).ToArray();
            string[] aiNamingTrample = library.OfType<BlueprintAiCastSpell>().Where(value => ReferenceEquals(value.Ability, trample))
                .Select(value => value.name + ":" + value.AssetGuid).ToArray();
            string[] unitsWithPrivateBrain = library.OfType<BlueprintUnit>().Where(value => ReferenceEquals(value.Brain, brain))
                .Select(value => value.name + ":" + value.AssetGuid).ToArray();
            bool nativeBrainUnchanged = native.Length > 0 && !native.Contains(ai) &&
                native.All(value => value != null && !(value is UngulateTrampleAi));
            Sprint25Check("risk06-ai-ownership", brainExact && aiExact && nativeBrainUnchanged &&
                brainsHoldingAi.Length == 1 && aiNamingTrample.Length == 1 && unitsWithPrivateBrain.Length == 1,
                new JObject { ["unitBrain"] = unit.Brain?.name + ":" + unit.Brain?.AssetGuid,
                    ["brainActions"] = new JArray(actions.Select(value => value?.name + ":" + value?.AssetGuid + ":" + value?.GetType().Name)),
                    ["nativeBrain"] = nativeBrain.name + ":" + nativeBrain.AssetGuid,
                    ["nativeBrainActions"] = new JArray(native.Select(value => value?.name + ":" + value?.AssetGuid)),
                    ["unitsUsingNativeBrain"] = library.OfType<BlueprintUnit>().Count(value => ReferenceEquals(value.Brain, nativeBrain)),
                    ["ai"] = ai.name + ":" + ai.AssetGuid, ["aiBaseScore"] = ai.BaseScore,
                    ["nativeActionBaseScores"] = new JArray(native.Select(value => value?.BaseScore)),
                    ["brainsHoldingAi"] = new JArray(brainsHoldingAi), ["aiActionsNamingTrample"] = new JArray(aiNamingTrample),
                    ["unitsWithPrivateBrain"] = new JArray(unitsWithPrivateBrain), ["libraryBlueprints"] = library.Length },
                "The Mastodon's private brain is the native natural-attack brain's actions, unchanged and in order, plus the owned Trample action; the shared native brain is unchanged; exactly one AI action names the Trample and only the Mastodon uses the private brain.");

            // 2. Route inventory from independent expectations: the catalog's
            // canonical placements and their template executions, and every
            // alternate family's projected Mastodon identity.
            string unitGuid = unit.AssetGuid;
            ILookup<string, BlueprintAbility> byName = library.OfType<BlueprintAbility>().ToLookup(value => value.name, StringComparer.Ordinal);
            BlueprintAbility[] menus = library.OfType<BlueprintAbility>().Where(value =>
                (value.Variants ?? new BlueprintAbility[0]).Any(child => child != null)).ToArray();
            var expected = new List<KeyValuePair<string, string>>();
            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
                foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family)
                    .Where(value => value.Creature.Key == MastodonTrampleAiPolicy.CreatureKey))
                {
                    string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                    expected.Add(new KeyValuePair<string, string>(symbol, "canonical placement"));
                    if (family == SummonFamily.Monster && variant.Creature.MonsterTemplated)
                    {
                        expected.Add(new KeyValuePair<string, string>(symbol + ".Celestial", "retained execution"));
                        expected.Add(new KeyValuePair<string, string>(symbol + ".Fiendish", "retained execution"));
                    }
                }
            AlternateSummoningRoster roster = AlternateSummoningRoster.Production();
            foreach (AlternateSummoningFamilyIdentity family in AlternateSummoningSourceCatalog.Identities)
                foreach (AlternateSummoningChoice choice in AlternateSummoningProjection.Identities(family, roster)
                    .Where(value => value.Kind == AlternateSummoningChoiceKind.Creature &&
                        value.Placement.Creature.Key == MastodonTrampleAiPolicy.CreatureKey))
                    expected.Add(new KeyValuePair<string, string>(choice.Symbol, "alternate " + family.Key));
            var rows = new JArray();
            int unresolved = 0, unreadable = 0, wrong = 0, canonicalProblems = 0;
            foreach (var pair in expected)
            {
                bool adapter = pair.Key.StartsWith(AlternateSummoningProjection.SymbolPrefix, StringComparison.Ordinal);
                string name = adapter ? AlternateSummoningAdapterBuilder.InternalName(pair.Key) : ExpandedSummoningInternalName(pair.Key);
                BlueprintAbility[] found = byName[name].ToArray();
                var row = new JObject { ["symbol"] = pair.Key, ["role"] = pair.Value, ["name"] = name, ["resolved"] = found.Length };
                if (found.Length != 1)
                {
                    unresolved++; row["problem"] = "unresolved"; rows.Add(row);
                    if (!adapter) canonicalProblems++;
                    continue;
                }
                BlueprintAbility ability = found[0];
                row["guid"] = ability.AssetGuid; row["hiddenFlag"] = ability.Hidden;
                row["offeredBy"] = new JArray(menus.Where(menu => menu.Variants.Contains(ability)).Select(menu => menu.name));
                SummonSpawn[] spawns = null;
                try { spawns = SummonSignatureReader.Read(ability).Spawns().ToArray(); }
                catch (Exception exception) { row["problem"] = "unreadable: " + exception.GetType().Name + ": " + exception.Message; }
                if (spawns == null)
                {
                    unreadable++; rows.Add(row);
                    if (!adapter) canonicalProblems++;
                    continue;
                }
                row["spawnUnits"] = new JArray(spawns.Select(value => value.Unit).Distinct());
                row["directlyControllable"] = new JArray(spawns.Select(value => value.DirectlyControllable).Distinct());
                bool ok = spawns.Length > 0 && spawns.All(value => value.Unit == unitGuid && !value.DirectlyControllable);
                if (!ok)
                {
                    wrong++; row["problem"] = "spawn unit or control flag differs";
                    if (!adapter) canonicalProblems++;
                }
                rows.Add(row);
            }
            Sprint25Check("risk06-ai-route-inventory", expected.Count > 0 && unresolved == 0 && wrong == 0 && canonicalProblems == 0,
                new JObject { ["expected"] = expected.Count,
                    ["canonicalPlacements"] = expected.Count(value => value.Value == "canonical placement"),
                    ["retainedExecutions"] = expected.Count(value => value.Value == "retained execution"),
                    ["alternateIdentities"] = expected.Count(value => value.Value.StartsWith("alternate ", StringComparison.Ordinal)),
                    ["unresolved"] = unresolved, ["unreadable"] = unreadable, ["wrongSpawnOrControl"] = wrong, ["rows"] = rows },
                "Every independently expected Mastodon output (canonical menu placements, their retained template executions and every alternate family's projected identity) resolves; each readable graph spawns only the Mastodon and none is directly controllable; any unreadable graph is listed, not skipped (canonical graphs must be readable).");
        }

        private IEnumerable<int> Risk06AiCell(ExpandedSummoningCorrectionFixture fixture, Cleanup2NativeDefaultScope mode,
            BlueprintUnit unit, BlueprintAbility trample, UngulateTrampleAi ai, string cellId, bool turnBased, string arrangement)
        {
            var game = Game.Instance;
            string id = "risk06-ai-" + cellId;
            var target = fixture.Hostile;
            Vector3 targetPosition = target.Position;
            Vector3 casterPosition = fixture.Caster.Position;
            Size sizeBefore = target.Descriptor.State.Size;
            int damageBefore = target.Descriptor.Damage;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldEngagement = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
            var tutorial = turnBased ? Kingmaker.Blueprints.Root.BlueprintRoot.Instance.UITutorials.TBMStarted.Lock : null;
            bool tutorialLocked = tutorial != null && tutorial.IsLocked;
            var flagsBefore = game.Player.UnlockableFlags.UnlockedFlags.ToArray();
            UnitEntityData owner = null, second = null;
            var quieted = new List<UnitEntityData>();
            var log = new Risk06CommandLog();
            ExpandedSummoningTrampleObserver contacts = null;
            bool subscribed = false, staggered = false;
            var passedTurns = new JArray();
            var events = new JArray();
            var combatBefore = new HashSet<UnitEntityData>(game.State.Units.All.Where(value => value != null && value.IsInCombat));
            var leftCombat = new JArray();
            bool partyOutOfCombat = false;
            try
            {
                if (!mode.Current) throw new InvalidOperationException("RISK-06 native Default ownership is not current.");
                owner = CastExpandedSummoningOwnTier(fixture, "mastodon");
                var summoned = owner.Get<UnitPartSummonedMonster>();
                var control = new JObject { ["unit"] = owner.UniqueId, ["isDirectlyControllable"] = owner.IsDirectlyControllable,
                    ["summonedPartDirectlyControllable"] = summoned?.IsDirectlyControllable,
                    ["liveBrain"] = owner.Brain?.Blueprint?.name,
                    ["liveBrainActions"] = new JArray((owner.Brain?.Actions ?? new List<AiAction>())
                        .Select(value => value?.Blueprint?.name)),
                    ["liveBrainAvailableActions"] = new JArray((owner.Brain?.AvailableActions ?? new List<AiAction>())
                        .Select(value => value?.Blueprint?.name)) };
                bool liveBrainExact = owner.Brain != null && ReferenceEquals(owner.Brain.Blueprint, unit.Brain) &&
                    owner.Brain.Actions.Any(value => ReferenceEquals(value?.Blueprint, ai)) &&
                    owner.Brain.AvailableActions.Any(value => ReferenceEquals(value?.Blueprint, ai));
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                var granted = owner.Descriptor.Abilities.GetAbility(trample);
                if (granted == null) throw new InvalidOperationException("The cast Mastodon lacks its Trample.");
                var data = new AbilityData(granted);

                // Disclosed fixture: a Medium private hostile (100000 hit
                // points, inactive brain); party actors quieted.
                target.Descriptor.State.Size = Size.Medium;
                foreach (var actor in game.Player.Party.Concat(new[] { fixture.Caster, game.Player.MainCharacter.Value })
                    .Where(value => value != null && !value.Destroyed && !ReferenceEquals(value, owner)).Distinct())
                {
                    if (actor.IsBrainActive) { Sprint28SetBrainActivePreservingActions(actor, false); quieted.Add(actor); }
                    actor.Commands.InterruptAll(true);
                }
                if (target.IsBrainActive) throw new InvalidOperationException("RISK-06 hostile brain is unexpectedly active.");

                // Survey an eligible approach with the product's own read-only
                // planner: the target out of every Mastodon reach and a legal
                // final segment beyond it.
                Vector3 start; UngulateTramplePlan plan; string survey;
                if (!Risk06SurveyApproach(fixture, owner, target, data, unit, out start, out plan, out survey))
                    throw new InvalidOperationException("RISK-06 found no eligible Trample approach: " + survey);
                float reach = UngulateTrampleAiRuntime.MeleeSlots(owner).Select(slot => slot.Weapon.AttackRange.Meters)
                    .DefaultIfEmpty(0f).Max();
                Vector3 forward = target.Position - owner.Position; forward.y = 0f; forward.Normalize();
                Vector3 side = new Vector3(-forward.z, 0f, forward.x);
                var geometry = new JObject { ["survey"] = survey, ["start"] = Risk06Vector(start),
                    ["targetCentre"] = Risk06Vector(target.Position), ["plannedEnd"] = Risk06Vector(plan.End),
                    ["planLength"] = plan.Length, ["planMax"] = plan.MaxLength,
                    ["casterCorpulence"] = owner.View.Corpulence, ["targetCorpulence"] = target.View.Corpulence,
                    ["mastodonMeleeReachMeters"] = reach, ["combatSpeedMps"] = owner.CombatSpeedMps,
                    ["centreDistance"] = owner.DistanceTo(target) };

                // Disclosed arrangement for the cell.
                if (arrangement == "other-in-reach" || arrangement == "threatened")
                {
                    second = game.EntityCreator.SpawnUnit(fixture.HostileBlueprint, owner.Position + side * 6f,
                        Quaternion.identity, fixture.Scene);
                    second.Stats.HitPoints.BaseValue = 100000;
                    SetExpandedSummoningBrainActive(second, false);
                    second.Descriptor.State.Size = Size.Medium;
                    if (arrangement == "threatened")
                    {
                        var weaponBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(Risk06NativeMeleeWeaponGuid);
                        if (weaponBlueprint == null || !weaponBlueprint.IsMelee)
                            throw new InvalidOperationException("RISK-06 native melee weapon is unavailable.");
                        second.Body.PrimaryHand.InsertItem(new ItemEntityWeapon(weaponBlueprint));
                    }
                    float secondReach = second.GetThreatHand()?.Weapon?.AttackRange.Meters ?? 0f;
                    float gap = arrangement == "threatened" ? 0.3f :
                        Math.Max(secondReach + 0.4f, Math.Min(reach * 0.6f, reach - 0.3f));
                    Vector3 place = owner.Position + side * (owner.View.Corpulence + second.View.Corpulence + gap);
                    Pathfinding.NNInfo node = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(place);
                    PlaceExpandedSummoningUnit(second, node.node != null && node.node.Walkable ? node.clampedPosition : place);
                    geometry["secondHostile"] = second.UniqueId; geometry["secondPosition"] = Risk06Vector(second.Position);
                    geometry["secondInMastodonReach"] = UngulateTrampleAiRuntime.InReach(owner, second,
                        UngulateTrampleAiRuntime.MeleeSlots(owner));
                    geometry["secondThreatReachMeters"] = secondReach;
                    geometry["secondArmed"] = second.GetThreatHand() != null;
                }
                else if (arrangement == "larger-target") target.Descriptor.State.Size = owner.Descriptor.State.Size;
                else if (arrangement == "staggered") { owner.Descriptor.State.AddCondition(UnitCondition.Staggered); staggered = true; }
                else if (arrangement == "endpoint-occupied")
                {
                    PlaceExpandedSummoningUnit(fixture.Caster, plan.End);
                    geometry["occupiedBy"] = fixture.Caster.UniqueId;
                }
                UngulateTramplePlan arrangedPlan;
                geometry["arrangedTargetRefusal"] = UngulateTrampleAiRuntime.Plan(owner, target, data, unit,
                    MastodonTrampleAiPolicy.CreatureKey, out arrangedPlan).ToString();

                if (turnBased)
                {
                    if (tutorial == null) throw new InvalidOperationException("Native turn tutorial flag absent.");
                    tutorial.Unlock();
                }
                var participants = new[] { owner, target, second }.Where(value => value != null).ToArray();
                foreach (var actor in participants)
                { actor.Wake(0f); if (!game.State.AwakeUnits.Contains(actor)) game.State.AwakeUnits.Add(actor); }
                log.Owner = owner; log.Target = target; log.Trample = trample;
                contacts = new ExpandedSummoningTrampleObserver { Caster = owner, Target = target };
                EventBus.Subscribe(log); EventBus.Subscribe(contacts); subscribed = true;
                SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = turnBased;
                game.TurnBasedCombatController.Activate();
                // Real time: begin at a fresh six-second round, so the claimed
                // round has room for the same-round observation.
                if (!turnBased)
                {
                    long boundary = ContextActionUngulateTrample.CurrentRound();
                    var align = DateTime.UtcNow.AddSeconds(20);
                    while (ContextActionUngulateTrample.CurrentRound() == boundary && DateTime.UtcNow < align)
                    { game.IsPaused = false; yield return 0; }
                }
                // Disclosed fixture trigger: ordinary native combat entry.
                foreach (var enemy in participants.Where(value => !ReferenceEquals(value, owner)))
                { owner.Memory.Add(enemy); enemy.Memory.Add(owner); }
                foreach (var actor in participants) actor.JoinCombat();
                double entered = game.Player.GameTime.TotalSeconds;
                long enteredRound = ContextActionUngulateTrample.CurrentRound();
                for (int settle = 0; settle < 3 || arrangement == "threatened" && !owner.CombatState.IsEngaged && settle < 90; settle++)
                { game.IsPaused = false; yield return 0; }
                var startRefusals = new JObject {
                    ["actor"] = UngulateTrampleAiRuntime.ActorRefusal(owner, unit).ToString(),
                    ["target"] = UngulateTrampleAiRuntime.Plan(owner, target, data, unit, MastodonTrampleAiPolicy.CreatureKey,
                        out arrangedPlan).ToString(),
                    ["engaged"] = owner.CombatState.IsEngaged, ["fullRound"] = owner.HasFullRoundAction(),
                    ["gameSeconds"] = game.Player.GameTime.TotalSeconds };
                Sprint28SetBrainActivePreservingActions(owner, true);

                bool positive = arrangement == "positive";
                bool death = arrangement == "death";
                var ownerTurns = new List<TurnController>();
                bool sameRoundReturned = false, laterReturned = false, killed = false;
                long claimRound = -1;
                double killedAt = -1, endAt = -1;
                var deadline = DateTime.UtcNow.AddSeconds(turnBased ? 150 : 60);
                while (DateTime.UtcNow < deadline)
                {
                    if (!mode.Current) throw new InvalidOperationException("RISK-06 combat lost native Default ownership.");
                    if (!owner.Destroyed) log.Scan();
                    double now = game.Player.GameTime.TotalSeconds;
                    UnitCommand[] trampleCommands = log.Started.Where(log.IsTrample).ToArray();
                    if (claimRound < 0 && contacts.Saves.Count + contacts.Opportunities.Count > 0)
                        claimRound = ContextActionUngulateTrample.CurrentRound();
                    if (death)
                    {
                        if (!killed && trampleCommands.Length > 0 && contacts.Saves.Count == 0 && contacts.Damage.Count == 0)
                        {
                            events.Add(new JObject { ["event"] = "kill-after-trample-start", ["gameSeconds"] = now,
                                ["commandActed"] = trampleCommands[0].IsActed, ["position"] = Risk06Vector(owner.Position) });
                            GameHelper.KillUnit(owner, target); killed = true; killedAt = now;
                        }
                        if (killed && now >= killedAt + 3) break;
                        if (!killed && now >= entered + 15) break;
                    }
                    else if (turnBased)
                    {
                        var turn = game.TurnBasedCombatController.CurrentTurn;
                        if (turn != null && ReferenceEquals(turn.Unit, owner))
                        { if (!ownerTurns.Contains(turn)) ownerTurns.Add(turn); }
                        else if (turn != null)
                        {
                            bool idle = !turn.Unit.IsBrainActive && turn.Unit.Commands.Empty && turn.Unit.Commands.Queue.Count == 0;
                            bool acting = turn.Status == TurnController.TurnStatus.Preparing || turn.Status == TurnController.TurnStatus.Acting;
                            if (!idle) throw new InvalidOperationException("Unknown or active nonparticipant in RISK-06 turn order: " + turn.Unit.UniqueId);
                            if (acting && turn.CanEndTurnAndNoActing())
                            {
                                // Later-round reuse: between the owner's first
                                // and second turns, return it to its start.
                                if (positive && !laterReturned && ownerTurns.Count == 1 && trampleCommands.Length == 1 &&
                                    trampleCommands[0].IsFinished)
                                {
                                    PlaceExpandedSummoningUnit(owner, start); laterReturned = true;
                                    events.Add(new JObject { ["event"] = "returned-to-start-between-turns",
                                        ["round"] = game.TurnBasedCombatController.RoundNumber });
                                }
                                passedTurns.Add(new JObject { ["unit"] = turn.Unit.UniqueId, ["round"] = game.TurnBasedCombatController.RoundNumber });
                                EndSprint17SnakeTurn(turn);
                            }
                        }
                        if (ownerTurns.Count >= (positive ? 3 : 2)) break;
                    }
                    else
                    {
                        bool running = log.Started.Any(value => value.IsStarted && !value.IsFinished);
                        if (positive)
                        {
                            long round = ContextActionUngulateTrample.CurrentRound();
                            if (!sameRoundReturned && trampleCommands.Length == 1 && trampleCommands[0].IsFinished &&
                                claimRound >= 0 && round == claimRound)
                            {
                                PlaceExpandedSummoningUnit(owner, start); sameRoundReturned = true;
                                events.Add(new JObject { ["event"] = "returned-to-start-same-round", ["round"] = round, ["gameSeconds"] = now });
                            }
                            if (!laterReturned && claimRound >= 0 && round > claimRound && trampleCommands.Length == 1 &&
                                trampleCommands[0].IsFinished && !running)
                            {
                                PlaceExpandedSummoningUnit(owner, start); laterReturned = true;
                                events.Add(new JObject { ["event"] = "returned-to-start-later-round", ["round"] = round, ["gameSeconds"] = now });
                            }
                            if (trampleCommands.Length >= 2 && trampleCommands[1].IsFinished && endAt < 0) endAt = now + 1;
                            if (endAt >= 0 && now >= endAt) break;
                            if (now >= entered + 40) break;
                        }
                        else if (now >= entered + 12) break;
                    }
                    game.IsPaused = false; yield return 0;
                }
                if (!mode.Current) throw new InvalidOperationException("RISK-06 combat lost ownership at verdict.");
                if (!owner.Destroyed) Sprint28SetBrainActivePreservingActions(owner, false);

                UnitCommand[] started = log.Started.ToArray();
                UnitCommand[] tramples = started.Where(log.IsTrample).ToArray();
                UnitAttack[] attacks = started.OfType<UnitAttack>().ToArray();
                bool allByOwnedAi = tramples.All(value => ReferenceEquals(value.AiAction, ai));
                bool noCutscene = started.All(value => !value.Cutscene);
                bool ledgerClearAtStart = tramples.All(value => log.Starts.ContainsKey(value) &&
                    log.Starts[value]["ledgerTargetClaimedAtStart"]?.Value<bool>() == false);
                bool claimedNow, haltedNow;
                ContextActionUngulateTrample.ReadLedger(owner, target.UniqueId, out claimedNow, out haltedNow);
                bool combatReal = owner.IsInCombat || owner.Descriptor.State.IsDead;
                // The action must never be dropped as "expended" by the native
                // selector, whatever the cell refused.
                bool stillAvailable = owner.Brain != null &&
                    owner.Brain.AvailableActions.Any(value => ReferenceEquals(value?.Blueprint, ai));
                bool valid;
                var verdict = new JObject { ["trampleActionStillAvailable"] = stillAvailable };
                if (positive)
                {
                    UnitCommand first = tramples.FirstOrDefault();
                    UnitCommand firstIssued = log.Issued.FirstOrDefault(value => value.AiAction != null);
                    bool firstIsTrample = first != null && ReferenceEquals(started[0], first) &&
                        firstIssued != null && log.IsTrample(firstIssued) && ReferenceEquals(firstIssued.AiAction, ai);
                    bool executed = first != null && first.IsFinished && first.Result == UnitCommand.ResultType.Success;
                    bool contacted = contacts.Saves.Count + contacts.Opportunities.Count >= 1 && contacts.Damage.Count >= 1;
                    // Turn-based: the native selector already requires the full
                    // round for this ability; after the Trample neither a full
                    // round nor a standard action remains, and the owner starts
                    // nothing else before its next Trample.
                    JObject firstEnd = log.Rows.OfType<JObject>().FirstOrDefault(row =>
                        row["edge"]?.Value<string>() == "end" && row["fullRoundAfter"] != null);
                    int otherBeforeSecond = first == null ? 0 : started.Skip(1)
                        .TakeWhile(value => tramples.Length < 2 || !ReferenceEquals(value, tramples[1])).Count();
                    bool fullRoundSpent = !turnBased || firstEnd != null &&
                        firstEnd["fullRoundAfter"]?.Value<bool>() == false && firstEnd["standardAfter"]?.Value<bool>() == false &&
                        otherBeforeSecond == 0;
                    verdict["otherCommandsBetweenTramples"] = otherBeforeSecond;
                    bool reuse = tramples.Length >= 2 && tramples[1].IsFinished && tramples[1].Result == UnitCommand.ResultType.Success &&
                        contacts.Saves.Count + contacts.Opportunities.Count >= 2;
                    bool laterRound = tramples.Length >= 2 && log.Starts.ContainsKey(tramples[1]) &&
                        log.Starts[tramples[1]]["round"]?.Value<long>() > claimRound;
                    bool sameRoundSuppressed = turnBased || sameRoundReturned && laterRound;
                    valid = liveBrainExact && stillAvailable && combatReal && firstIsTrample && executed && allByOwnedAi && noCutscene &&
                        ledgerClearAtStart && contacted && fullRoundSpent && reuse && laterRound && sameRoundSuppressed && laterReturned;
                    verdict["firstCommandIsOwnedTrample"] = firstIsTrample; verdict["firstExecutedSuccess"] = executed;
                    verdict["contacted"] = contacted; verdict["fullRoundSpent"] = fullRoundSpent;
                    verdict["laterRoundReuse"] = reuse; verdict["secondTrampleInLaterRound"] = laterRound;
                    verdict["sameRoundSuppressed"] = sameRoundSuppressed; verdict["claimRound"] = claimRound;
                    verdict["ledgerClearAtEveryTrampleStart"] = ledgerClearAtStart;
                }
                else if (death)
                {
                    UnitCommand first = tramples.FirstOrDefault();
                    bool noContact = contacts.Saves.Count == 0 && contacts.Opportunities.Count == 0 && contacts.Damage.Count == 0;
                    bool stopped = first != null && first.IsFinished && owner.Commands.Raw.All(value => value == null || value.IsFinished);
                    valid = liveBrainExact && killed && owner.Descriptor.State.IsDead && first != null &&
                        ReferenceEquals(first.AiAction, ai) && noContact && stopped && !claimedNow && tramples.Length == 1;
                    verdict["killed"] = killed; verdict["dead"] = owner.Descriptor.State.IsDead; verdict["noContact"] = noContact;
                    verdict["commandStopped"] = stopped; verdict["firstResult"] = first?.Result.ToString();
                }
                else
                {
                    string actorRefusal = startRefusals["actor"].Value<string>();
                    string targetRefusal = startRefusals["target"].Value<string>();
                    bool expectedRefusal = arrangement == "other-in-reach" ? actorRefusal == "MeleeTargetInReach" :
                        arrangement == "threatened" ? actorRefusal == "Threatened" :
                        arrangement == "larger-target" ? targetRefusal == "TargetNotSmaller" :
                        arrangement == "staggered" ? actorRefusal == "NoFullRoundAction" :
                        arrangement == "endpoint-occupied" && targetRefusal == "EndpointOccupied";
                    // Judged on every command the AI issued, started or still
                    // approaching (a Staggered Mastodon may only move).
                    UnitAttack[] issuedAttacks = log.Issued.OfType<UnitAttack>().ToArray();
                    bool trampleIssued = log.Issued.Any(log.IsTrample) || tramples.Length > 0;
                    bool attacked = issuedAttacks.Length >= 1 && issuedAttacks.All(value => value.AiAction != null &&
                        !(value.AiAction is UngulateTrampleAi));
                    bool attackedInReach = arrangement != "other-in-reach" && arrangement != "threatened" ||
                        issuedAttacks.Any(value => ReferenceEquals(value.TargetUnit, second));
                    valid = liveBrainExact && stillAvailable && combatReal && expectedRefusal && !trampleIssued && attacked && attackedInReach &&
                        noCutscene && (!turnBased || ownerTurns.Count >= 2);
                    verdict["expectedRefusalAtStart"] = expectedRefusal; verdict["trampleIssued"] = trampleIssued;
                    verdict["ordinaryAttackIssued"] = attacked; verdict["issuedAttacks"] = issuedAttacks.Length;
                    verdict["attackIssuedAgainstEnemyAlreadyInReach"] = attackedInReach;
                }
                Sprint25Check(id, valid, new JObject { ["arrangement"] = arrangement, ["turnBased"] = turnBased,
                    ["control"] = control, ["liveBrainExact"] = liveBrainExact, ["geometry"] = geometry,
                    ["refusalsAtCombatStart"] = startRefusals, ["verdict"] = verdict, ["events"] = events,
                    ["enteredGameSeconds"] = entered, ["enteredRound"] = enteredRound,
                    ["observedGameSeconds"] = game.Player.GameTime.TotalSeconds,
                    ["ownerTurnsObserved"] = ownerTurns.Count, ["idleTurnsPassed"] = passedTurns,
                    ["commands"] = log.Rows, ["commandsStarted"] = started.Length, ["trampleCommands"] = tramples.Length,
                    ["trampleCommandsByOwnedAi"] = tramples.Count(value => ReferenceEquals(value.AiAction, ai)),
                    ["attackCommands"] = attacks.Length,
                    ["targetSaves"] = new JArray(contacts.Saves.Select(value => new JObject { ["dc"] = value.DifficultyClass,
                        ["type"] = value.Type.ToString(), ["passed"] = value.IsPassed })),
                    ["damageToTarget"] = contacts.Damage.Count, ["targetOpportunityAttacks"] = contacts.Opportunities.Count,
                    ["ledgerClaimedAtEnd"] = claimedNow, ["ledgerHaltedAtEnd"] = haltedNow,
                    ["quietedPartyActors"] = new JArray(quieted.Select(value => value.UniqueId)),
                    ["fixtureTrampleCommands"] = 0, ["fixtureAiTicks"] = 0, ["fixtureSuppliedCommands"] = 0, ["forcedRolls"] = 0 },
                    Risk06AiCellDescription(arrangement, turnBased));
            }
            finally
            {
                try
                {
                    if (subscribed) { EventBus.Unsubscribe(log); EventBus.Unsubscribe(contacts); }
                    if (owner != null && !owner.Destroyed) Sprint28SetBrainActivePreservingActions(owner, false);
                    if (staggered && owner != null && !owner.Destroyed) owner.Descriptor.State.RemoveCondition(UnitCondition.Staggered);
                    foreach (var actor in new[] { owner, target, second }.Where(value => value != null && !value.Destroyed))
                        actor.Commands.InterruptAll(true);
                    foreach (var actor in game.State.Units.All.Where(value => value != null && !value.Destroyed &&
                        value.IsInCombat && !combatBefore.Contains(value)).ToArray())
                    { actor.LeaveCombat(); leftCombat.Add(actor.UniqueId); }
                    game.Player.UpdateIsInCombat();
                    partyOutOfCombat = !game.Player.IsInCombat;
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
                        if (arrangement == "endpoint-occupied") PlaceExpandedSummoningUnit(fixture.Caster, casterPosition);
                        if (second != null && !second.Destroyed)
                        {
                            CleanupExpandedSummoningUnit(second);
                            game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                        }
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
                        bool casterReturned = Vector3.Distance(fixture.Caster.Position, casterPosition) < 0.5f;
                        bool restored = flagsRestored && partyOutOfCombat && !game.IsPaused && target.Descriptor.State.Size == sizeBefore &&
                            target.Descriptor.Damage == damageBefore && (owner == null || owner.Destroyed || !fixture.Created.Contains(owner)) &&
                            (second == null || second.Destroyed) && casterReturned;
                        Sprint25Check(id + "-restored", restored, new JObject { ["flagsRestored"] = flagsRestored,
                            ["leftCombat"] = leftCombat, ["partyOutOfCombatBeforeModeRestore"] = partyOutOfCombat,
                            ["pausedAfterModeRestore"] = game.IsPaused,
                            ["targetSize"] = target.Descriptor.State.Size.ToString(), ["targetDamage"] = target.Descriptor.Damage,
                            ["ownerDisposed"] = owner == null || owner.Destroyed, ["secondDisposed"] = second == null || second.Destroyed,
                            ["casterReturned"] = casterReturned, ["quietedRestored"] = quieted.Count,
                            ["turnBasedSetting"] = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue },
                            "Every unit the cell brought into combat leaves it before the turn-based setting is restored (no native pause); the summon and any second hostile are disposed; the hostile's size, damage and position, the summoner's position, the quieted actors, the engagement and turn-based settings and the tutorial flag are restored.");
                    }
                }
            }
        }

        private static string Risk06AiCellDescription(string arrangement, bool turnBased)
        {
            string mode = turnBased ? "turn-based" : "real-time";
            switch (arrangement)
            {
                case "positive":
                    return "Eligible approach (" + mode + "): the Mastodon's own native AI selects the owned Trample action first and executes it (cast, movement, contact) with the target unclaimed in the ledger at every Trample start; " +
                        (turnBased ? "the full round is spent, and after a disclosed return to the start between turns it tramples again in a later round."
                                   : "after a disclosed return to the start in the claimed round it does not trample that target again until a later round, and then does.");
                case "other-in-reach":
                    return "Another enemy already within the Mastodon's melee reach (" + mode + "): Trample is refused at combat start and never selected; the ordinary attack is used against the enemy in reach.";
                case "threatened":
                    return "Threatened by an armed adjacent enemy (" + mode + "): Trample is refused and never selected; the ordinary attack is used against that enemy.";
                case "larger-target":
                    return "Target not smaller than the Mastodon (" + mode + "): Trample is refused for it and never selected; the ordinary attack is used.";
                case "staggered":
                    return "No full-round action (native Staggered condition, " + mode + "): Trample is refused and never selected; the ordinary attack is used.";
                case "endpoint-occupied":
                    return "Invalid final path, the planned endpoint occupied by another body (" + mode + "): no Trample is cast; the ordinary attack is used.";
                default:
                    return "Death after the owned Trample command starts (" + mode + "): the command stops, no contact or damage follows, and no claim is left in the ledger.";
            }
        }

        private static JArray Risk06Vector(Vector3 value) { return new JArray(value.x, value.y, value.z); }

        /// <summary>
        /// Places the Mastodon and the target so that the product's read-only
        /// planner accepts the target and no enemy is within any Mastodon
        /// reach. It never issues a command.
        /// </summary>
        private bool Risk06SurveyApproach(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, UnitEntityData target,
            AbilityData data, BlueprintUnit unit, out Vector3 start, out UngulateTramplePlan plan, out string detail)
        {
            if (AstarPath.active == null) throw new InvalidOperationException("RISK-06 requires live area navigation.");
            var attempts = new List<string>();
            float reach = UngulateTrampleAiRuntime.MeleeSlots(owner).Select(slot => slot.Weapon.AttackRange.Meters).DefaultIfEmpty(0f).Max();
            float bodies = owner.View.Corpulence + target.View.Corpulence;
            var centres = new List<Vector3> { fixture.Caster.Position + Vector3.forward * 6f, fixture.Caster.Position };
            centres.AddRange(CompassOffsets.Select(direction => fixture.Caster.Position + direction * 8f));
            foreach (Vector3 centre in centres)
                foreach (float extra in new[] { 1.5f, 2.5f, 3.5f })
                    foreach (Vector3 direction in CompassOffsets)
                    {
                        float separation = bodies + reach + extra;
                        Pathfinding.NNInfo from = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(centre - direction * separation * .5f);
                        Pathfinding.NNInfo to = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(centre + direction * separation * .5f);
                        if (from.node == null || to.node == null || !from.node.Walkable || !to.node.Walkable) continue;
                        PlaceExpandedSummoningUnit(owner, from.clampedPosition);
                        PlaceExpandedSummoningUnit(target, to.clampedPosition);
                        MastodonTrampleAiRefusal refusal = UngulateTrampleAiRuntime.Plan(owner, target, data, unit,
                            MastodonTrampleAiPolicy.CreatureKey, out plan);
                        bool clear = !UngulateTrampleAiRuntime.AnyMeleeTargetInReach(owner);
                        if (attempts.Count < 24 || refusal == MastodonTrampleAiRefusal.None)
                            attempts.Add(extra + "/" + direction.ToString("F1") + ":" + refusal + ",clear=" + clear);
                        if (refusal != MastodonTrampleAiRefusal.None || !clear) continue;
                        start = owner.Position;
                        detail = string.Join("|", attempts.ToArray());
                        return true;
                    }
            start = owner.Position; plan = default(UngulateTramplePlan);
            detail = string.Join("|", attempts.ToArray());
            return false;
        }
    }
}
