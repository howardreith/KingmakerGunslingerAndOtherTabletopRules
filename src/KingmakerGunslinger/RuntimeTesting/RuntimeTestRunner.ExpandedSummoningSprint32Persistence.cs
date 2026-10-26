using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Fixed auxiliary actors, following the Sprint22/26 Working fixture seam.
    // This is a fixture receipt, not product state or a new save format.
    public sealed class UnitPartSprint32ListenerReceipt : UnitPart
    {
        [JsonProperty] public string Scope, UnitId, CasterId, SourceId, Role, PrepareRunId;
    }
    public sealed class UnitPartSprint32SavedReceipt : UnitPart
    {
        [JsonProperty] public string State, PrepareRunId;
        [JsonProperty] public int Ordinal;
    }

    internal sealed partial class RuntimeTestRunner
    {
        private const string Sprint32ListenerScope = "KMG_Sprint32_Listeners_v1";
        private const string Sprint32ListenerFaction = "0f539babafb47fe4586b719d02aff7c4";
        private const string Sprint32PrivatePrefix = "KMG_Summoning_FeyCelestial_";
        private double _sprint32ColdGameSeconds;
        private static T Sprint32SavedBlueprint<T>(string suffix) where T : BlueprintScriptableObject
        { return (T)ExpandedSummoningBlueprints.Registered.BySymbol["KMG.Summoning.FeyCelestial." + suffix]; }
        private static UnitEntityData[] Sprint32SavedListeners(UnitEntityData caster)
        {
            return Game.Instance.State.Units.All.Concat(caster.HoldingState.AllEntityData.OfType<UnitEntityData>())
                .Where(value => value != null && value.Get<UnitPartSprint32ListenerReceipt>() != null).Distinct().ToArray();
        }
        private static bool Sprint32OwnsListener(UnitEntityData unit, UnitEntityData caster, UnitEntityData[] sources)
        {
            var row = unit?.Get<UnitPartSprint32ListenerReceipt>();
            return row != null && row.Scope == Sprint32ListenerScope && row.UnitId == unit.UniqueId &&
                row.CasterId == caster.UniqueId && !string.IsNullOrWhiteSpace(row.PrepareRunId) &&
                (row.Role == "held" || row.Role == "resisted") && sources.Any(source => source.UniqueId == row.SourceId) &&
                ReferenceEquals(unit.Blueprint, BlueprintRoot.Instance.DefaultPlayerCharacter) &&
                ReferenceEquals(unit.HoldingState, caster.HoldingState) && !Game.Instance.Player.Party.Contains(unit);
        }
        private static Buff[] Sprint32SavedEffects(UnitEntityData source)
        {
            return Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed)
                .SelectMany(unit => unit.Buffs.Enumerable).Where(buff =>
                    buff.Blueprint.name.StartsWith(Sprint32PrivatePrefix, StringComparison.Ordinal) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, source)).ToArray();
        }
        private static JObject Sprint32SavedState(UnitEntityData source, UnitEntityData[] listeners)
        {
            string key = source.Get<UnitPartRelease143RosterReceipt>().Key;
            string prefix = key == "satyr" ? "Satyr." : "LillendAzata.";
            string[] resources = key == "satyr" ? new[] { "FearResource" } :
                new[] { "CharmPersonSlaResource", "PerformanceResource" };
            var nature = (ModifiableValueSkill)source.Stats.GetStat(StatType.SkillLoreNature);
            var result = new JObject {
                ["owner"] = source.UniqueId, ["key"] = key,
                ["nature"] = new JObject { ["ownIntelligence"] = ReferenceEquals(nature.BaseStat, source.Stats.Intelligence),
                    ["ranks"] = nature.BaseValue, ["classSkill"] = (bool)nature.ClassSkill, ["value"] = nature.ModifiedValue },
                ["items"] = new JArray(source.Descriptor.Inventory.Items.OrderBy(item => item.InventorySlotIndex)
                    .ThenBy(item => item.Blueprint.AssetGuid, StringComparer.Ordinal).Select(item => new JObject {
                        ["blueprint"] = item.Blueprint.AssetGuid, ["owner"] = item.Owner?.Unit?.UniqueId,
                        ["index"] = item.InventorySlotIndex, ["count"] = item.Count,
                        ["nonRemovable"] = item.IsNonRemovable, ["lootable"] = item.IsLootable })),
                ["resources"] = new JObject(resources.Select(name => new JProperty(name,
                    source.Descriptor.Resources.GetResourceAmount(Sprint32SavedBlueprint<BlueprintAbilityResource>(prefix + name))))),
                ["effects"] = new JArray(Sprint32SavedEffects(source).OrderBy(buff => buff.Owner.Unit.UniqueId, StringComparer.Ordinal)
                    .ThenBy(buff => buff.Blueprint.AssetGuid, StringComparer.Ordinal).Select(buff => new JObject {
                        ["target"] = buff.Owner.Unit.UniqueId, ["blueprint"] = buff.Blueprint.AssetGuid,
                        ["source"] = buff.Context?.MaybeCaster?.UniqueId, ["permanent"] = buff.IsPermanent,
                        ["endTicks"] = buff.IsPermanent ? (JToken)JValue.CreateNull() : new JValue(buff.EndTime.Ticks),
                        ["components"] = new JArray(buff.SelectComponents<GameLogicComponent>().Select(component => component.name)) })) };
            if (key == "lillend-azata")
            {
                var creation = source.Blueprint.GetComponent<LillendBardBook32>();
                var book = source.Descriptor.GetSpellbook(creation.Book);
                if (book == null) throw new InvalidOperationException("Saved Lillend has no native book.");
                var fascinate = source.Buffs.Enumerable.SelectMany(buff => buff.SelectComponents<LillendFascinateState32>()).SingleOrDefault();
                var competence = source.Buffs.Enumerable.SelectMany(buff => buff.SelectComponents<LillendCompetenceState32>()).SingleOrDefault();
                result["book"] = new JObject { ["blueprint"] = book.Blueprint.AssetGuid, ["cl"] = book.CasterLevel,
                    ["bookCount"] = source.Descriptor.Spellbooks.Count(value => ReferenceEquals(value.Blueprint, book.Blueprint)),
                    ["slots"] = new JArray(Enumerable.Range(1, 3).Select(book.GetSpontaneousSlots)),
                    ["known"] = new JArray(Enumerable.Range(0, 4).SelectMany(level => book.GetKnownSpells(level)
                        .Select(spell => level + ":" + spell.Blueprint.AssetGuid)).OrderBy(value => value, StringComparer.Ordinal)) };
                result["dispatchers"] = source.Buffs.Enumerable.SelectMany(buff => buff.SelectComponents<LillendNoncombat32>()).Count();
                result["responses"] = fascinate == null ? null : new JArray(listeners.OrderBy(unit => unit.UniqueId, StringComparer.Ordinal)
                    .Select(unit => new JObject { ["target"] = unit.UniqueId, ["response"] = fascinate.Response(unit),
                        ["owns"] = fascinate.Owns(unit), ["dazed"] = unit.Descriptor.State.HasCondition(UnitCondition.Dazed),
                        ["perception"] = unit.Stats.SkillPerception.ModifiedValue }));
                result["competenceRecipients"] = new JArray(Game.Instance.Player.Party.Where(unit => competence?.Selected(unit) == true)
                    .Select(unit => unit.UniqueId).OrderBy(value => value, StringComparer.Ordinal));
                result["toggles"] = new JArray(source.Descriptor.ActivatableAbilities.Enumerable
                    .Where(toggle => toggle.Blueprint.name.StartsWith(Sprint32PrivatePrefix, StringComparison.Ordinal))
                    .OrderBy(toggle => toggle.Blueprint.AssetGuid, StringComparer.Ordinal).Select(toggle => new JObject {
                        ["blueprint"] = toggle.Blueprint.AssetGuid, ["on"] = toggle.IsOn, ["running"] = toggle.IsRunning }));
            }
            return result;
        }

        private void Sprint32PersistenceAbsence(UnitEntityData caster, string stage)
        {
            var listeners = Sprint32SavedListeners(caster);
            var units = Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed).ToArray();
            var sources = units.Where(unit => ExpandedSummoningRosterPersistencePolicy.FeyCelestialKeys.Contains(
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key)).ToArray();
            var effects = units.SelectMany(unit => unit.Buffs.Enumerable).Where(buff =>
                buff.Blueprint.name.StartsWith(Sprint32PrivatePrefix, StringComparison.Ordinal)).ToArray();
            var areas = Game.Instance.State.AreaEffects.All.Where(area => area.Blueprint.name.StartsWith(Sprint32PrivatePrefix, StringComparison.Ordinal)).ToArray();
            bool absent = listeners.Length == 0 && sources.Length == 0 && effects.Length == 0 && areas.Length == 0;
            RosterPersistenceCheck("s32-" + stage + "-absence", absent, new JObject {
                ["listeners"] = listeners.Length, ["sources"] = sources.Length, ["effects"] = effects.Length, ["areas"] = areas.Length },
                "No selected source, exact fixture listener, private effect or performance area remains; unrelated state is never removed to satisfy this boundary.");
            if (!absent) throw new InvalidOperationException("Sprint32 persistence absence failed: " + stage);
        }

        private void ObserveSprint32ColdState(UnitEntityData caster, UnitEntityData[] sources)
        {
            var listeners = Sprint32SavedListeners(caster);
            if (sources.Length != 4 || listeners.Length != 2 || listeners.Any(unit => !Sprint32OwnsListener(unit, caster, sources)))
                throw new InvalidOperationException("Sprint32 cold ownership is incomplete or ambiguous.");
            bool boundary = Sprint32Suggestion.Settled && !Sprint32Suggestion.Active &&
                sources.All(source => source.Get<UnitPartSprint32SavedReceipt>() != null) &&
                sources.Select(source => source.Get<UnitPartSprint32SavedReceipt>().Ordinal).OrderBy(value => value).SequenceEqual(new[] { 0, 1, 2, 3 }) &&
                listeners.Select(unit => unit.Get<UnitPartSprint32ListenerReceipt>().Role).OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(new[] { "held", "resisted" }) &&
                listeners.All(unit => unit.Faction.AssetGuid == Sprint32ListenerFaction && !unit.Group.IsPlayerParty &&
                    unit.AttackFactions.Count() == 1 && ReferenceEquals(unit.AttackFactions.Single(), caster.Faction) &&
                    sources.All(source => unit.IsEnemy(source) && source.IsEnemy(unit)));
            RosterPersistenceCheck("s32-cold-listener-boundary", boundary, new JObject {
                ["providerDisposition"] = Sprint32Suggestion.Disposition,
                ["listeners"] = new JArray(listeners.Select(unit => new JObject { ["id"] = unit.UniqueId,
                    ["faction"] = unit.Faction.AssetGuid, ["attackFactions"] = new JArray(unit.AttackFactions.Select(faction => faction.AssetGuid)),
                    ["partyGroup"] = unit.Group.IsPlayerParty, ["role"] = unit.Get<UnitPartSprint32ListenerReceipt>().Role })) },
                "The absent-provider fixture restores exact canonical factions, per-instance attack lists and nonparty listener groups; provider transitions are a separate selection.");
            if (!boundary) throw new InvalidOperationException("Sprint32 cold profile/relation boundary differs.");
            _sprint32ColdGameSeconds = Game.Instance.Player.GameTime.TotalSeconds;
            foreach (var source in sources)
            {
                var receipt = source.Get<UnitPartSprint32SavedReceipt>();
                var actual = Sprint32SavedState(source, listeners);
                bool exact = receipt != null && !string.IsNullOrEmpty(receipt.PrepareRunId) && receipt.PrepareRunId != _request.RunId &&
                    receipt.State == actual.ToString(Formatting.None) && (bool)actual["nature"]["ownIntelligence"];
                RosterPersistenceCheck("s32-cold-state-" + source.UniqueId, exact,
                    new JObject { ["moduleEnabled"] = _context.FeatureModules.Active.ExpandedSummoning,
                        ["providerActive"] = Sprint32Suggestion.Active, ["prepareRunId"] = receipt?.PrepareRunId,
                        ["saved"] = receipt?.State == null ? null : JObject.Parse(receipt.State), ["loaded"] = actual },
                    "Before deliberate time advances, exact native slots/resources, own Intelligence link, equipment, selected recipient/response ledger and effect deadlines survive cold load without duplication or refill.");
                if (!exact) throw new InvalidOperationException("Sprint32 cold state mismatch; no cleanup save.");
            }
        }

        private IEnumerable<int> PrepareSprint32PersistentState(UnitEntityData caster, UnitEntityData[] sources)
        {
            var game = Game.Instance; var party = game.Player.Party.ToArray();
            // The existing roster appearance observer deliberately leaves the
            // game paused. Admit that native overlay only over Default; the
            // bounded resume below must finish before any dispatcher is enabled.
            int listenerCount = Sprint32SavedListeners(caster).Length;
            bool boundary = (game.CurrentMode == GameModeType.Default || game.CurrentMode == GameModeType.Pause) &&
                game.IsModeActive(GameModeType.Default) && !game.Player.IsInCombat && sources.Length == 4 && listenerCount == 0 &&
                Sprint32Suggestion.Settled && Sprint32Suggestion.Disposition == "provider-absent";
            RosterPersistenceCheck("s32-prepare-boundary", boundary, new JObject {
                ["mode"] = game.CurrentMode.ToString(), ["paused"] = game.IsPaused,
                ["defaultActive"] = game.IsModeActive(GameModeType.Default), ["inCombat"] = game.Player.IsInCombat,
                ["sources"] = sources.Length, ["listeners"] = listenerCount,
                ["providerSettled"] = Sprint32Suggestion.Settled, ["providerDisposition"] = Sprint32Suggestion.Disposition },
                "Four exact sources in the calm provider-absent Working scene; only native Pause over Default is admitted before bounded resume.");
            if (!boundary)
                throw new InvalidOperationException("Sprint32 persistence requires a calm native Working scene and four exact sources.");
            var lillends = sources.Where(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "lillend-azata").ToArray();
            if (lillends.Length != 2) throw new InvalidOperationException("Two independent Lillends required.");
            var owner = lillends[0]; var support = lillends[1];
            var effect = Sprint32SavedBlueprint<BlueprintBuff>("LillendAzata.Fascinated");
            var resistance = Sprint32SavedBlueprint<BlueprintBuff>("LillendAzata.FascinateResistance");
            foreach (var source in sources)
            {
                var initial = Sprint32SavedState(source, Array.Empty<UnitEntityData>());
                bool fresh = (bool)initial["nature"]["ownIntelligence"];
                if (source.Get<UnitPartRelease143RosterReceipt>().Key == "satyr")
                    fresh &= (int)initial["resources"]["FearResource"] == 1;
                else fresh &= (int)initial["resources"]["CharmPersonSlaResource"] == 1 &&
                    (int)initial["resources"]["PerformanceResource"] == 20 && (int)initial["dispatchers"] == 1 &&
                    (int)initial["book"]["cl"] == 7 && (int)initial["book"]["bookCount"] == 1 &&
                    initial["book"]["slots"].Values<int>().SequenceEqual(new[] { 5, 4, 2 });
                if (!fresh) throw new InvalidOperationException("Sprint32 initial native persistence budgets/state are not fresh: " + initial);
            }
            var listeners = new List<UnitEntityData>();
            var starts = new Lillend32CommandStarts { Owner = support };
            EventBus.Subscribe(starts);
            try
            {
                // First establish the second owner's real Competence before
                // adding hostile listeners. No intake actor is repositioned.
                Func<bool> noListener = () => !game.State.Units.All.Any(unit =>
                    LillendFascinateEligibility32.Selectable(support, unit, effect, resistance));
                if (!LillendNoncombat32.Selectable(support, caster) || !noListener())
                    throw new InvalidOperationException("Working caster is not a visible eligible Competence recipient.");
                foreach (int frame in ResumeSprint32Working(party, sources)) yield return frame;
                if (!noListener()) throw new InvalidOperationException("An intake actor became eligible while resuming support setup.");
                SetExpandedSummoningBrainActive(support, true);
                var until = DateTime.UtcNow.AddSeconds(22);
                while (DateTime.UtcNow < until && (!support.Buffs.Enumerable.SelectMany(buff => buff.SelectComponents<LillendCompetenceState32>())
                    .Any(state => party.Any(state.Selected)) || !starts.Commands.OfType<UnitActivateAbility>().Any(command => command.IsFinished)))
                {
                    Sprint32CalmWorking(party, sources);
                    if (!noListener()) throw new InvalidOperationException("An intake actor became eligible for Fascinate; stop support setup.");
                    yield return 0;
                }
                game.IsPaused = true; SetExpandedSummoningBrainActive(support, false);
                bool competence = noListener() && starts.Commands.OfType<UnitActivateAbility>().Any(command => command.IsActed && command.IsFinished &&
                    command.Result == UnitCommand.ResultType.Success &&
                    command.Type == UnitCommand.CommandType.Move && !command.Cutscene && !command.IsIgnoreCooldown) &&
                    support.Buffs.Enumerable.SelectMany(buff => buff.SelectComponents<LillendCompetenceState32>()).Any(state => party.Any(state.Selected));
                RosterPersistenceCheck("s32-prepared-competence", competence, new JObject {
                    ["source"] = support.UniqueId, ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)) },
                    "The second Lillend's actual dispatcher selects an existing party recipient through a normal native Move activation.");
                if (!competence) throw new InvalidOperationException("Native Competence did not establish a saved recipient.");
            }
            finally { game.IsPaused = true; SetExpandedSummoningBrainActive(support, false); EventBus.Unsubscribe(starts); }

            var mobs = BlueprintLibraryLookup.RequireExact<BlueprintFaction>(BlueprintBootstrap.Library,
                Sprint32ListenerFaction, "registered native Mobs faction for fixed saved listeners");
            var intake = game.State.Units.All.Where(unit => unit != null && !sources.Contains(unit) && !party.Contains(unit)).ToArray();
            var points = Sprint32ListenerPoints(caster, sources, party);
            for (int index = 0; index < 2; index++)
            {
                var target = game.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    points[index], Quaternion.identity, caster.HoldingState);
                game.EntityCreator.Tick();
                var receipt = target.Ensure<UnitPartSprint32ListenerReceipt>();
                receipt.Scope = Sprint32ListenerScope; receipt.UnitId = target.UniqueId; receipt.CasterId = caster.UniqueId;
                receipt.SourceId = owner.UniqueId; receipt.Role = index == 0 ? "held" : "resisted"; receipt.PrepareRunId = _request.RunId;
                listeners.Add(target);
                foreach (var stat in new[] { target.Stats.Strength, target.Stats.Dexterity, target.Stats.Constitution,
                    target.Stats.Intelligence, target.Stats.Wisdom, target.Stats.Charisma }) stat.BaseValue = 10;
                target.Stats.HitPoints.BaseValue = 100000; target.Stats.SaveWill.BaseValue = index == 0 ? -100 : 100;
                SetExpandedSummoningBrainActive(target, false); target.Descriptor.SwitchFactions(mobs, true);
                target.AttackFactions.Match(new[] { caster.Faction }); PlaceExpandedSummoningUnit(target, points[index]);
                if (!game.State.AwakeUnits.Contains(target)) game.State.AwakeUnits.Add(target);
                if (!Sprint32OwnsListener(target, caster, sources) || target.Group.IsPlayerParty ||
                    intake.Any(unit => target.IsEnemy(unit) || unit.IsEnemy(target)))
                    throw new InvalidOperationException("Saved listener relation/ownership reaches an intake nonparty actor; no time advance/save.");
            }
            var saves = new Dretch30CloudSaves(listeners.ToArray()); starts = new Lillend32CommandStarts { Owner = owner };
            EventBus.Subscribe(saves); EventBus.Subscribe(starts);
            try
            {
                foreach (int frame in ResumeSprint32Working(party, sources.Concat(listeners).ToArray())) yield return frame;
                // Let native appearance and visibility settle with all owned
                // brains disabled. No memory, combat or faction blueprint edits.
                var quietEnd = game.Player.GameTime + TimeSpan.FromSeconds(7); var until = DateTime.UtcNow.AddSeconds(22);
                while (game.Player.GameTime < quietEnd && DateTime.UtcNow < until)
                { Sprint32CalmWorking(party, sources.Concat(listeners).ToArray()); yield return 0; }
                var eligible = game.State.Units.All.Where(unit => LillendFascinateEligibility32.Selectable(owner, unit, effect, resistance)).ToArray();
                bool ready = game.Player.GameTime >= quietEnd && eligible.Length == 2 && eligible.All(listeners.Contains) &&
                    listeners.All(target => target.DistanceTo(owner) > target.VisionRangeMeters &&
                        party.Concat(sources).All(unit => target.DistanceTo(unit) > target.VisionRangeMeters && !target.Memory.Contains(unit))) &&
                    listeners.All(target => target.IsEnemy(owner) && owner.IsEnemy(target));
                RosterPersistenceCheck("s32-listener-preconditions", ready, new JObject {
                    ["eligible"] = new JArray(eligible.Select(unit => unit.UniqueId)),
                    ["listeners"] = new JArray(listeners.Select(unit => new JObject { ["id"] = unit.UniqueId,
                        ["vision"] = unit.VisionRangeMeters, ["distance"] = unit.DistanceTo(owner), ["will"] = unit.Stats.SaveWill.ModifiedValue })) },
                    "Exactly two owned, physically visible eligible listeners remain calm beyond native enemy acquisition; no intake actor is moved or made hostile.");
                if (!ready) throw new InvalidOperationException("Saved Fascinate preconditions failed.");
                SetExpandedSummoningBrainActive(owner, true); until = DateTime.UtcNow.AddSeconds(22);
                while (DateTime.UtcNow < until && (saves.Rows.Count < 2 ||
                    !starts.Commands.OfType<UnitActivateAbility>().Any(command => command.IsFinished)))
                { Sprint32CalmWorking(party, sources.Concat(listeners).ToArray()); yield return 0; }
                game.IsPaused = true; SetExpandedSummoningBrainActive(owner, false);
                var state = owner.Buffs.Enumerable.SelectMany(buff => buff.SelectComponents<LillendFascinateState32>()).SingleOrDefault();
                bool actual = state != null && state.Response(listeners[0]) == 1 && state.Owns(listeners[0]) &&
                    state.Response(listeners[1]) == 2 && saves.Rows.Count == 2 &&
                    starts.Commands.OfType<UnitActivateAbility>().Any(command => command.IsActed && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success && command.Type == UnitCommand.CommandType.Move &&
                        !command.Cutscene && !command.IsIgnoreCooldown) && listeners[1].Buffs.Enumerable.Any(buff =>
                            ReferenceEquals(buff.Blueprint, resistance) && ReferenceEquals(buff.Context?.MaybeCaster, owner) &&
                            buff.TimeLeft.TotalHours > 23.9 && buff.TimeLeft.TotalHours <= 24.0);
                RosterPersistenceCheck("s32-prepared-fascinate-ledger", actual, new JObject { ["source"] = owner.UniqueId,
                    ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)), ["saves"] = saves.Rows,
                    ["responses"] = new JArray(listeners.Select(unit => state?.Response(unit))), ["forcedRolls"] = 0 },
                    "A real native activation and two unforced Will results create one held identity and one source-specific 24-hour refusal; no response JSON is supplied.");
                if (!actual) throw new InvalidOperationException("Actual Fascinate outcomes did not establish the declared saved state.");
            }
            finally { game.IsPaused = true; SetExpandedSummoningBrainActive(owner, false); EventBus.Unsubscribe(saves); EventBus.Unsubscribe(starts); }

            // Prospective native spends establish distinct saved budgets only.
            // They are not spell/action/AI evidence and never refill a resource.
            for (int index = 0; index < sources.Length; index++)
            {
                var source = sources[index]; string key = source.Get<UnitPartRelease143RosterReceipt>().Key;
                if (key == "satyr" && Array.IndexOf(sources.Where(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == key).ToArray(), source) == 0)
                    source.Descriptor.Resources.Spend(Sprint32SavedBlueprint<BlueprintAbilityResource>("Satyr.FearResource"), 1);
                if (key == "lillend-azata")
                {
                    var book = source.Descriptor.GetSpellbook(source.Blueprint.GetComponent<LillendBardBook32>().Book);
                    int ordinal = Array.IndexOf(lillends, source);
                    for (int level = 1; level <= 3; level++)
                        for (int spend = 0; spend <= ordinal && spend < level; spend++) book.GetKnownSpells(level).First().Spend();
                    if (ordinal == 0) source.Descriptor.Resources.Spend(Sprint32SavedBlueprint<BlueprintAbilityResource>("LillendAzata.CharmPersonSlaResource"), 1);
                }
                var state = Sprint32SavedState(source, listeners.ToArray());
                int speciesOrdinal = Array.IndexOf(sources.Where(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == key).ToArray(), source);
                bool spent = key == "satyr" ? (int)state["resources"]["FearResource"] == speciesOrdinal :
                    (int)state["resources"]["CharmPersonSlaResource"] == speciesOrdinal &&
                    state["book"]["slots"].Values<int>().SequenceEqual(speciesOrdinal == 0 ? new[] { 4, 3, 1 } : new[] { 4, 2, 0 }) &&
                    (int)state["resources"]["PerformanceResource"] > 0 && (int)state["resources"]["PerformanceResource"] < 20;
                var receipt = source.Ensure<UnitPartSprint32SavedReceipt>(); receipt.Ordinal = index; receipt.PrepareRunId = _request.RunId;
                receipt.State = state.ToString(Formatting.None);
                RosterPersistenceCheck("s32-prepared-state-" + index, spent, state,
                    "Exact distinct saved budgets after disclosed persistence-only native spends; performance rounds remain actual native expenditure and cold comparison is separate.");
                if (!spent) throw new InvalidOperationException("Native persistence spends did not establish the declared budgets.");
            }
        }

        private IEnumerable<int> ResumeSprint32Working(UnitEntityData[] party, UnitEntityData[] owned)
        {
            var game = Game.Instance;
            var limit = DateTime.UtcNow.AddSeconds(3);
            for (int frame = 0; frame < 30 && DateTime.UtcNow < limit; frame++)
            {
                if ((game.CurrentMode != GameModeType.Default && game.CurrentMode != GameModeType.Pause) ||
                    !game.IsModeActive(GameModeType.Default) || game.Player.IsInCombat ||
                    !game.Player.Party.SequenceEqual(party) || owned.Any(unit => unit.Destroyed || unit.IsInCombat || unit.Descriptor.State.IsDead))
                    throw new InvalidOperationException("Sprint32 Working resume encountered an unexpected mode/combat/ownership boundary.");
                game.IsPaused = false;
                // Native StartMode/StopMode may queue their transition until
                // the next frame. Do not force a mode or tick a dispatcher.
                yield return 0;
                if (game.CurrentMode == GameModeType.Default && !game.IsPaused)
                { Sprint32CalmWorking(party, owned); yield break; }
            }
            throw new InvalidOperationException("Sprint32 native Working resume did not reach calm Default.");
        }

        private static void Sprint32CalmWorking(UnitEntityData[] party, UnitEntityData[] owned)
        {
            if (Game.Instance.CurrentMode != GameModeType.Default || Game.Instance.Player.IsInCombat ||
                !Game.Instance.Player.Party.SequenceEqual(party) || owned.Any(unit => unit.Destroyed || unit.IsInCombat || unit.Descriptor.State.IsDead))
                throw new InvalidOperationException("Sprint32 Working calm/mode/ownership boundary changed.");
        }
        private Vector3[] Sprint32ListenerPoints(UnitEntityData caster, UnitEntityData[] sources, UnitEntityData[] party)
        {
            if (AstarPath.active == null) throw new InvalidOperationException("Native navigation unavailable.");
            var geometry = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance;
            if (geometry == null) throw new InvalidOperationException("Native physical sight unavailable.");
            var performers = sources.Where(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "lillend-azata").ToArray();
            if (performers.Length != 2) throw new InvalidOperationException("Two exact owned Lillends required for listener survey.");
            var rejected = new Dictionary<string, int> { ["navigation"] = 0, ["clampOrSeparation"] = 0,
                ["acquisitionExclusion"] = 0, ["performanceRange"] = 0, ["physicalSight"] = 0 };
            Vector3[] selected = null; int attempts = 0;
            // A player's acquisition radius is not the performance's sight
            // contract. Keep the intake still; survey four finite radii using
            // native navigation, actual eye geometry and the90ft performance.
            foreach (var anchor in party)
            {
                foreach (float distance in new[] { 10.7f, 14f, 18f, 22f })
                {
                    for (int angle = 0; angle < 24; angle++)
                    {
                        attempts++;
                        var direction = Quaternion.Euler(0, angle * 15, 0) * Vector3.forward;
                        var side = Vector3.Cross(Vector3.up, direction);
                        var requested = new[] { anchor.Position + direction * distance, anchor.Position + direction * distance + side * 1.3f };
                        var points = requested.Select(point => AstarPath.active.GetNearest(point)).ToArray();
                        if (points.Any(point => point.node == null || !point.node.Walkable)) { rejected["navigation"]++; continue; }
                        var positions = points.Select(point => point.clampedPosition).ToArray();
                        if (positions.Where((point, index) => Vector3.Distance(point, requested[index]) > .3f).Any() ||
                            Vector3.Distance(positions[0], positions[1]) < .9f) { rejected["clampOrSeparation"]++; continue; }
                        if (positions.Any(point => party.Concat(sources).Any(unit => Vector3.Distance(point, unit.Position) <= 9.1f)))
                        { rejected["acquisitionExclusion"]++; continue; }
                        if (positions.Any(point => performers.Any(unit => Vector3.Distance(point, unit.Position) >= 25f)))
                        { rejected["performanceRange"]++; continue; }
                        if (positions.Any(point => performers.Any(unit => geometry.HasObstacle(unit.EyePosition,
                            point + Kingmaker.Visual.FogOfWar.LineOfSightGeometry.EyeShift, 0))))
                        { rejected["physicalSight"]++; continue; }
                        selected = positions; break;
                    }
                    if (selected != null) break;
                }
                if (selected != null) break;
            }
            RosterPersistenceCheck("s32-listener-geometry", selected != null, new JObject {
                ["attempts"] = attempts, ["rejected"] = new JObject(rejected.Select(pair => new JProperty(pair.Key, pair.Value))),
                ["party"] = new JArray(party.Select(unit => new JObject { ["id"] = unit.UniqueId,
                    ["playerFaction"] = unit.IsPlayerFaction, ["vision"] = unit.VisionRangeMeters })),
                ["positions"] = selected == null ? null : new JArray(selected.Select(point => new JArray(point.x, point.y, point.z))) },
                "Finite native nav/eye-geometry survey excludes acquisition distance from every intake-party and source actor; actual mutual performance eligibility and calm state are checked after spawn.");
            if (selected == null) throw new InvalidOperationException("No bounded calm listener geometry without moving the intake party.");
            return selected;
        }

        private IEnumerable<int> CleanupSprint32PersistentState(UnitEntityData caster, UnitEntityData[] sources)
        {
            var listeners = Sprint32SavedListeners(caster);
            if (listeners.Length != 2 || listeners.Any(unit => !Sprint32OwnsListener(unit, caster, sources)))
                throw new InvalidOperationException("Sprint32 cleanup listener ownership ambiguous.");
            double elapsed = Game.Instance.Player.GameTime.TotalSeconds - _sprint32ColdGameSeconds;
            foreach (var source in sources)
            {
                var saved = JObject.Parse(source.Get<UnitPartSprint32SavedReceipt>().State);
                var current = Sprint32SavedState(source, listeners);
                var comparable = (JObject)current.DeepClone();
                bool upkeep = true;
                if (source.Get<UnitPartRelease143RosterReceipt>().Key == "lillend-azata")
                {
                    int before = (int)saved["resources"]["PerformanceResource"], after = (int)current["resources"]["PerformanceResource"];
                    upkeep = after > 0 && after <= before && before - after <= Math.Ceiling(elapsed / 6d) + 1;
                    comparable["resources"]["PerformanceResource"] = before;
                }
                bool live = elapsed >= 0 && !source.IsBrainActive && upkeep && JToken.DeepEquals(saved, comparable);
                RosterPersistenceCheck("s32-precleanup-live-" + source.UniqueId, live, new JObject {
                    ["gameSecondsSinceCold"] = elapsed, ["saved"] = saved, ["beforeRetirement"] = current,
                    ["nativeUpkeepWithinElapsedRounds"] = upkeep },
                    "Saved recipient/response identities, actual Dazed state, private grants/deadlines, slots and equipment remain live immediately before source retirement; only bounded native performance upkeep may reduce rounds.");
                if (!live) throw new InvalidOperationException("Saved Sprint32 performance/state changed before cleanup; no vacuous removal verdict.");
            }
            var grants = sources.SelectMany(Sprint32SavedEffects).ToArray();
            var areas = Game.Instance.State.AreaEffects.All.Where(area => sources.Contains(area.Context?.MaybeCaster)).ToArray();
            var first = sources.Single(source => source.Get<UnitPartSprint32SavedReceipt>().Ordinal == 1);
            if (first.Get<UnitPartRelease143RosterReceipt>().Key != "lillend-azata")
                throw new InvalidOperationException("Declared saved primary Lillend ordering changed.");
            var siblings = sources.Where(source => !ReferenceEquals(source, first)).ToArray();
            var siblingGrants = siblings.SelectMany(Sprint32SavedEffects).ToArray();
            var siblingStates = siblings.Select(source => Sprint32SavedState(source, listeners).ToString(Formatting.None)).ToArray();
            var firstGrants = Sprint32SavedEffects(first);
            var firstAreas = areas.Where(area => ReferenceEquals(area.Context?.MaybeCaster, first)).ToArray();
            // Existing fixture retirement manually detaches/disposes equipment.
            // This proves saved equipment state and private visual/effect teardown,
            // not native equipment expiry. Retain actual grant/area references.
            foreach (var step in ObserveRosterResourceCleanup(new[] { first }, "s32-primary")) yield return step;
            for (int frame = 0; frame < 3; frame++)
            { foreach (var area in firstAreas.Where(value => !value.Destroyed)) area.Tick(); Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            bool isolated = firstGrants.Length > 0 && firstAreas.Length > 0 &&
                firstGrants.All(buff => !buff.Owner.Buffs.RawFacts.Contains(buff)) &&
                firstAreas.All(area => area.Destroyed && !area.IsInState) &&
                listeners.All(unit => !unit.Descriptor.State.HasCondition(UnitCondition.Dazed)) &&
                siblingGrants.All(buff => buff.Owner.Buffs.RawFacts.Contains(buff)) &&
                siblings.Select((source, index) => !source.Destroyed &&
                    Sprint32SavedState(source, listeners).ToString(Formatting.None) == siblingStates[index]).All(value => value);
            RosterPersistenceCheck("s32-cold-sibling-isolation", isolated, new JObject { ["retiredSource"] = first.UniqueId,
                ["capturedPrimaryGrants"] = firstGrants.Length, ["capturedPrimaryAreas"] = firstAreas.Length,
                ["retainedSiblingGrants"] = siblingGrants.Count(buff => buff.Owner.Buffs.RawFacts.Contains(buff)) },
                "Exact first-source removal clears its saved Fascinate/assistance/resistance and Dazed while preserving each sibling's same grants, resources and selected recipient.");
            if (!isolated) throw new InvalidOperationException("Sprint32 cold sibling isolation failed; no cleanup save.");
            foreach (var step in ObserveRosterResourceCleanup(siblings, "s32-siblings")) yield return step;
            for (int frame = 0; frame < 3; frame++)
            { foreach (var area in areas.Where(value => !value.Destroyed)) area.Tick(); Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            bool clean = grants.All(buff => !buff.Owner.Buffs.RawFacts.Contains(buff)) &&
                areas.All(area => area.Destroyed && !area.IsInState) &&
                listeners.All(unit => !unit.Descriptor.State.HasCondition(UnitCondition.Dazed));
            RosterPersistenceCheck("s32-loaded-source-cleanup", clean, new JObject { ["capturedGrants"] = grants.Length,
                ["capturedAreas"] = areas.Length, ["grantResidue"] = grants.Count(buff => buff.Owner.Buffs.RawFacts.Contains(buff)),
                ["dazedListeners"] = listeners.Count(unit => unit.Descriptor.State.HasCondition(UnitCondition.Dazed)) },
                "Removing the exact saved sources clears captured private grants/areas and actual listener Dazed state before fixture victims are retired.");
            if (!clean) throw new InvalidOperationException("Sprint32 native cleanup left state; no cleanup save.");
            foreach (var listener in listeners) listener.Destroy();
            Game.Instance.EntityDestroyer.Tick(); yield return 0;
        }
    }
}
