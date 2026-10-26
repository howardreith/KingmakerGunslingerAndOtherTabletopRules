using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
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
        [JsonProperty] public string ControlState, OriginalAttackFactions, ListenerFaction;
    }
    public sealed class UnitPartSprint32SavedReceipt : UnitPart
    {
        [JsonProperty] public string State, PrepareRunId;
        [JsonProperty] public int Ordinal;
    }

    internal sealed partial class RuntimeTestRunner
    {
        private const string Sprint32ListenerScope = "KMG_Sprint32_Listeners_v1";
        private static BlueprintFaction Sprint32ListenerFaction(UnitEntityData listener)
        {
            var guid = listener?.Get<UnitPartSprint32ListenerReceipt>()?.ListenerFaction;
            if (string.IsNullOrEmpty(guid)) throw new InvalidOperationException("Saved listener faction identity absent.");
            var faction = BlueprintLibraryLookup.RequireExact<BlueprintFaction>(BlueprintBootstrap.Library,
                guid, "saved registered faction for fixed Sprint32 auxiliaries");
            if (faction == null || faction.AlwaysEnemy || faction.IsDirectlyControllable ||
                !ReferenceEquals(faction, BlueprintLibraryLookup.RequireExact<BlueprintFaction>(BlueprintBootstrap.Library,
                    faction.AssetGuid, "registered saved listener faction")))
                throw new InvalidOperationException("Registered listener faction boundary differs.");
            return faction;
        }
        private BlueprintFaction SelectSprint32ListenerFaction(UnitEntityData caster, UnitEntityData[] sources,
            UnitEntityData[] party, UnitEntityData[] intake)
        {
            var groups = intake.Select(unit => unit.Group).Distinct().ToArray();
            var attacks = intake.SelectMany(unit => unit.AttackFactions)
                .Concat(groups.SelectMany(group => group.AttackFactions)).Distinct().ToArray();
            // Inspect the native cached membership, not merely unit.Faction: the
            // group cache is what actual IsEnemy uses. Never refresh intake groups.
            var membership = typeof(Kingmaker.UnitLogic.Groups.UnitGroup).GetField("m_Factions",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (membership == null) throw new InvalidOperationException("Native group membership contract absent.");
            var casterGroups = groups.Where(group => {
                var set = membership.GetValue(group);
                return (bool)set.GetType().GetMethod("Contains", new[] { typeof(BlueprintFaction) })
                    .Invoke(set, new object[] { caster.Faction });
            }).ToArray();
            var excluded = sources.Concat(party).Select(unit => unit.Faction).Concat(new[] { caster.Faction }).Distinct().ToArray();
            var candidates = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintFaction>()
                .Where(faction => !string.IsNullOrEmpty(faction.AssetGuid) && !faction.AlwaysEnemy &&
                    !faction.IsDirectlyControllable && !excluded.Contains(faction) && !attacks.Contains(faction))
                .OrderBy(faction => faction.AssetGuid, StringComparer.Ordinal).ToArray();
            bool safe = !attacks.Any(faction => faction.AlwaysEnemy) && casterGroups.Length == 0 && candidates.Length > 0;
            RosterPersistenceCheck("s32-listener-faction-selection", safe, new JObject {
                ["intakeUnits"] = intake.Length, ["intakeGroups"] = groups.Length,
                ["alwaysEnemyAttackEntries"] = attacks.Count(faction => faction.AlwaysEnemy),
                ["groupsContainingCasterFaction"] = casterGroups.Length, ["safeRegisteredCandidates"] = candidates.Length,
                ["selectedGuid"] = safe ? candidates[0].AssetGuid : null },
                "Select one existing registered identity before spawning; retain every intake unit/group hostility refusal and never edit intake groups.");
            if (!safe) throw new InvalidOperationException("No safe registered listener faction under full intake relations; no auxiliary spawn.");
            var selected = candidates[0];
            if (!ReferenceEquals(selected, BlueprintLibraryLookup.RequireExact<BlueprintFaction>(BlueprintBootstrap.Library,
                selected.AssetGuid, "selected saved listener faction"))) throw new InvalidOperationException("Selected faction is not exact registered identity.");
            return selected;
        }
        private static void RequireSprint32ColdAuxiliaryRelations(UnitEntityData caster, UnitEntityData[] sources)
        {
            var auxiliaries = Sprint32SavedAuxiliaries(caster);
            if (auxiliaries.Select(unit => unit.Get<UnitPartSprint32ListenerReceipt>().ListenerFaction).Distinct().Count() != 1)
                throw new InvalidOperationException("Saved auxiliary faction identities disagree.");
            var party = Game.Instance.Player.Party.ToArray();
            var intake = Game.Instance.State.Units.All.Where(unit => unit != null && !party.Contains(unit) &&
                !sources.Contains(unit) && !auxiliaries.Contains(unit)).ToArray();
            foreach (var unit in auxiliaries)
                if (!ReferenceEquals(unit.Faction, Sprint32ListenerFaction(unit)) || unit.GroupId != unit.UniqueId ||
                    unit.Group.IsPlayerParty || unit.Group.Count != 1 || !ReferenceEquals(unit.Group[0], unit) ||
                    intake.Any(other => unit.IsEnemy(other) || other.IsEnemy(unit)))
                    throw new InvalidOperationException("Cold auxiliary identity/group reaches unrelated intake; no time advance.");
        }
        private const string Sprint32PrivatePrefix = "KMG_Summoning_FeyCelestial_";
        private double _sprint32ColdGameSeconds;
        private static T Sprint32SavedBlueprint<T>(string suffix) where T : BlueprintScriptableObject
        { return (T)ExpandedSummoningBlueprints.Registered.BySymbol["KMG.Summoning.FeyCelestial." + suffix]; }
        private static UnitEntityData[] Sprint32SavedAuxiliaries(UnitEntityData caster)
        {
            return Game.Instance.State.Units.All.Concat(caster.HoldingState.AllEntityData.OfType<UnitEntityData>())
                .Where(value => value != null && value.Get<UnitPartSprint32ListenerReceipt>() != null).Distinct().ToArray();
        }
        private static UnitEntityData[] Sprint32SavedListeners(UnitEntityData caster)
        { return Sprint32SavedAuxiliaries(caster).Where(unit => new[] { "held", "resisted" }.Contains(unit.Get<UnitPartSprint32ListenerReceipt>().Role)).ToArray(); }
        private static UnitEntityData[] Sprint32SavedControls(UnitEntityData caster)
        { return Sprint32SavedAuxiliaries(caster).Where(unit => new[] { "charm", "sleep", "hold" }.Contains(unit.Get<UnitPartSprint32ListenerReceipt>().Role)).ToArray(); }
        private static bool Sprint32OwnsListener(UnitEntityData unit, UnitEntityData caster, UnitEntityData[] sources)
        {
            var row = unit?.Get<UnitPartSprint32ListenerReceipt>();
            return row != null && row.Scope == Sprint32ListenerScope && row.UnitId == unit.UniqueId &&
                row.CasterId == caster.UniqueId && !string.IsNullOrWhiteSpace(row.PrepareRunId) &&
                new[] { "held", "resisted", "charm", "sleep", "hold" }.Contains(row.Role) && sources.Any(source => source.UniqueId == row.SourceId) &&
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
            var listeners = Sprint32SavedAuxiliaries(caster);
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
                listeners.All(unit => ReferenceEquals(unit.Faction, Sprint32ListenerFaction(unit)) && unit.GroupId == unit.UniqueId && !unit.Group.IsPlayerParty &&
                    unit.AttackFactions.Count() == 1 && ReferenceEquals(unit.AttackFactions.Single(), caster.Faction) &&
                    sources.All(source => unit.IsEnemy(source) && source.IsEnemy(unit)));
            RosterPersistenceCheck("s32-cold-listener-boundary", boundary, new JObject {
                ["providerDisposition"] = Sprint32Suggestion.Disposition,
                ["listeners"] = new JArray(listeners.Select(unit => new JObject { ["id"] = unit.UniqueId,
                    ["faction"] = unit.Faction.AssetGuid, ["attackFactions"] = new JArray(unit.AttackFactions.Select(faction => faction.AssetGuid)),
                    ["partyGroup"] = unit.Group.IsPlayerParty, ["role"] = unit.Get<UnitPartSprint32ListenerReceipt>().Role })) },
                "The absent-provider fixture restores exact canonical factions, per-instance attack lists and nonparty listener groups; provider transitions are a separate selection.");
            if (!boundary) throw new InvalidOperationException("Sprint32 cold profile/relation boundary differs.");
            RequireSprint32ColdAuxiliaryRelations(caster, sources);
            ObserveSprint32SavedControls(caster, sources, "cold");
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

            var intake = game.State.Units.All.Where(unit => unit != null && !sources.Contains(unit) && !party.Contains(unit)).ToArray();
            var listenerFaction = SelectSprint32ListenerFaction(caster, sources, party, intake);
            var points = Sprint32ListenerPoints(caster, sources, party);
            for (int index = 0; index < 2; index++)
            {
                var target = game.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    points[index], Quaternion.identity, caster.HoldingState);
                game.EntityCreator.Tick();
                var receipt = target.Ensure<UnitPartSprint32ListenerReceipt>();
                receipt.Scope = Sprint32ListenerScope; receipt.UnitId = target.UniqueId; receipt.CasterId = caster.UniqueId;
                receipt.SourceId = owner.UniqueId; receipt.Role = index == 0 ? "held" : "resisted"; receipt.PrepareRunId = _request.RunId;
                receipt.ListenerFaction = listenerFaction.AssetGuid;
                listeners.Add(target);
                foreach (var stat in new[] { target.Stats.Strength, target.Stats.Dexterity, target.Stats.Constitution,
                    target.Stats.Intelligence, target.Stats.Wisdom, target.Stats.Charisma }) stat.BaseValue = 10;
                target.Stats.HitPoints.BaseValue = 100000; target.Stats.SaveWill.BaseValue = index == 0 ? -100 : 100;
                SetExpandedSummoningBrainActive(target, false);
                // DefaultPlayerCharacter may already cache the player group.
                // SwitchFactions refreshes relations, not group membership.
                // Detach this owned actor while its old faction is still current.
                target.GroupId = target.UniqueId;
                target.Descriptor.SwitchFactions(listenerFaction, true);
                target.AttackFactions.Match(new[] { caster.Faction }); PlaceExpandedSummoningUnit(target, points[index]);
                if (!game.State.AwakeUnits.Contains(target)) game.State.AwakeUnits.Add(target);
                // SwitchFactions raises its first attack-set event before assigning
                // Faction. That event constructs this group's old Player membership;
                // later events only queue an attack-cache refresh. Refresh both native
                // caches synchronously, and only on the proven single owned member.
                var group = target.Group;
                if (!Sprint32OwnsListener(target, caster, sources) || target.GroupId != target.UniqueId ||
                    group.IsPlayerParty || group.Count != 1 || !ReferenceEquals(group[0], target))
                    throw new InvalidOperationException("Listener group is not exclusively owned; no group refresh.");
                group.ResetFactionSet(); group.UpdateAttackFactionsCache();
                var hostileIntake = intake.Where(unit => target.IsEnemy(unit) || unit.IsEnemy(target)).ToArray();
                bool owned = Sprint32OwnsListener(target, caster, sources);
                bool isolated = target.GroupId == target.UniqueId && !target.Group.IsPlayerParty;
                RosterPersistenceCheck("s32-listener-ownership-" + index, owned && isolated && hostileIntake.Length == 0,
                    new JObject { ["id"] = target.UniqueId, ["owned"] = owned, ["group"] = target.GroupId,
                        ["playerGroup"] = target.Group.IsPlayerParty,
                        ["faction"] = target.Faction.AssetGuid, ["ownedGroupMembers"] = group.Count,
                        ["groupAttackFactions"] = new JArray(group.AttackFactions.Select(faction => faction.AssetGuid)),
                        ["hostileIntake"] = new JArray(hostileIntake.Select(unit => unit.UniqueId)) },
                    "Only this newly owned listener has its own native group; no intake nonparty actor is hostile in either direction.");
                if (!owned || !isolated || hostileIntake.Length != 0)
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

            foreach (var step in PrepareSprint32SavedControls(caster, sources, points.Skip(2).ToArray(), intake)) yield return step;

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
            if (Game.Instance.Player.IsInCombat || TurnBased.Controllers.CombatController.IsInTurnBasedCombat() ||
                caster.View?.AgentASP == null)
                throw new InvalidOperationException("Listener survey requires the existing calm native walking graph.");
            var agent = caster.View.AgentASP;
            var constructTags = agent.GetType().GetMethod("ConstructTags", Cleanup2Fields);
            if (constructTags == null) throw new MissingMethodException("Native walking ConstructTags.");
            int tags = (int)constructTags.Invoke(agent, null);
            if (tags != -1) throw new InvalidOperationException("Unexpected calm native walking tags.");
            var rejected = new Dictionary<string, int> { ["anchor"] = 0, ["navigation"] = 0,
                ["horizontalClamp"] = 0, ["heightOrSeparation"] = 0, ["acquisitionExclusion"] = 0,
                ["performanceRange"] = 0, ["physicalSight"] = 0 };
            var anchors = new JArray();
            Vector3[] selected = null, controlPoints = null; int attempts = 0;
            var controlCandidates = new List<Vector3>();
            Func<Vector3[]> selectControls = () => {
                var chosen = new List<Vector3>();
                foreach (var point in controlCandidates)
                {
                    if (selected.Any(value => Vector3.Distance(value, point) < .9f) ||
                        chosen.Any(value => Vector3.Distance(value, point) < .9f)) continue;
                    chosen.Add(point);
                    if (chosen.Count == 3) return chosen.ToArray();
                }
                return null;
            };
            // Reuse the established Cleanup2 native walking query: graph1
            // outside combat, actual traversal tags and connected walkable Area.
            // Full3D clamp is not horizontal placement: native ground can have
            // a different Y. Keep the same finite rings and actual sight/calm
            // checks; never move an intake actor or edit graph/path state.
            foreach (var anchor in party)
            {
                var constraint = new Pathfinding.PathNNConstraint { graphMask = 1,
                    constrainWalkability = true, walkable = true, constrainArea = false,
                    constrainTags = true, tags = tags, constrainDistance = true,
                    distanceXZ = false, distanceXZkeepY = false };
                var ground = AstarPath.active.GetNearest(anchor.Position, constraint);
                bool validGround = ground.node != null && ground.node.Walkable &&
                    constraint.Suitable(ground.node) && Cleanup2Finite(ground.clampedPosition) &&
                    constraint.SuitableGraph((int)ground.node.GraphIndex, AstarPath.active.graphs[ground.node.GraphIndex]) &&
                    new Vector2(ground.clampedPosition.x - anchor.Position.x, ground.clampedPosition.z - anchor.Position.z).magnitude <= .3f;
                anchors.Add(new JObject { ["id"] = anchor.UniqueId, ["valid"] = validGround,
                    ["origin"] = new JArray(anchor.Position.x, anchor.Position.y, anchor.Position.z),
                    ["ground"] = new JArray(ground.clampedPosition.x, ground.clampedPosition.y, ground.clampedPosition.z),
                    ["graph"] = ground.node == null ? (JToken)JValue.CreateNull() : new JValue(ground.node.GraphIndex),
                    ["area"] = ground.node == null ? (JToken)JValue.CreateNull() : new JValue(ground.node.Area) });
                if (!validGround) { rejected["anchor"]++; continue; }
                constraint.SetStart(ground.node); constraint.constrainArea = true;
                foreach (float distance in new[] { 10.7f, 14f, 18f, 22f })
                {
                    for (int angle = 0; angle < 24; angle++)
                    {
                        attempts++;
                        var direction = Quaternion.Euler(0, angle * 15, 0) * Vector3.forward;
                        var side = Vector3.Cross(Vector3.up, direction);
                        // Preserve the proven two-listener footprint. Separate
                        // effect-only controls need no performance LOS or range.
                        var requested = new[] { ground.clampedPosition + direction * distance,
                            ground.clampedPosition + direction * distance + side * 1.3f };
                        var points = requested.Select(point => AstarPath.active.GetNearest(point, constraint)).ToArray();
                        for (int index = 0; index < points.Length; index++)
                        {
                            var point = points[index]; var position = point.clampedPosition;
                            if (point.node == null || !point.node.Walkable || !constraint.Suitable(point.node) ||
                                point.node.GraphIndex != ground.node.GraphIndex || point.node.Area != ground.node.Area ||
                                !Cleanup2Finite(position) || Mathf.Abs(position.y - ground.clampedPosition.y) > 2f ||
                                new Vector2(position.x - requested[index].x, position.z - requested[index].z).magnitude > .3f ||
                                party.Concat(sources).Any(unit => Vector3.Distance(position, unit.Position) <= Mathf.Max(9.1f, unit.VisionRangeMeters))) continue;
                            if (controlCandidates.All(value => Vector3.Distance(value, position) > .01f)) controlCandidates.Add(position);
                        }
                        if (selected != null)
                        {
                            controlPoints = selectControls();
                            if (controlPoints != null) break;
                            continue;
                        }
                        if (points.Any(point => point.node == null || !point.node.Walkable ||
                            !constraint.Suitable(point.node) || point.node.GraphIndex != ground.node.GraphIndex ||
                            point.node.Area != ground.node.Area || !Cleanup2Finite(point.clampedPosition)))
                        { rejected["navigation"]++; continue; }
                        var positions = points.Select(point => point.clampedPosition).ToArray();
                        if (positions.Where((point, index) => new Vector2(point.x - requested[index].x,
                            point.z - requested[index].z).magnitude > .3f).Any())
                        { rejected["horizontalClamp"]++; continue; }
                        if (positions.Any(point => Mathf.Abs(point.y - ground.clampedPosition.y) > 2f) ||
                            positions.Where((point, index) => positions.Skip(index + 1).Any(other => Vector3.Distance(point, other) < .9f)).Any())
                        { rejected["heightOrSeparation"]++; continue; }
                        if (positions.Any(point => party.Concat(sources).Any(unit => Vector3.Distance(point, unit.Position) <= 9.1f)))
                        { rejected["acquisitionExclusion"]++; continue; }
                        if (positions.Any(point => performers.Any(unit => Vector3.Distance(point, unit.Position) >= 25f)))
                        { rejected["performanceRange"]++; continue; }
                        if (positions.Any(point => performers.Any(unit => geometry.HasObstacle(unit.EyePosition,
                            point + Kingmaker.Visual.FogOfWar.LineOfSightGeometry.EyeShift, 0))))
                        { rejected["physicalSight"]++; continue; }
                        selected = positions; controlPoints = selectControls();
                        if (controlPoints != null) break;
                    }
                    if (selected != null && controlPoints != null) break;
                }
                if (selected != null && controlPoints != null) break;
            }
            var allPoints = selected != null && controlPoints != null ? selected.Concat(controlPoints).ToArray() : null;
            RosterPersistenceCheck("s32-listener-geometry", allPoints != null, new JObject {
                ["attempts"] = attempts, ["walkingGraphMask"] = 1, ["nativeTraversalTags"] = tags, ["anchors"] = anchors,
                ["rejected"] = new JObject(rejected.Select(pair => new JProperty(pair.Key, pair.Value))),
                ["party"] = new JArray(party.Select(unit => new JObject { ["id"] = unit.UniqueId,
                    ["playerFaction"] = unit.IsPlayerFaction, ["vision"] = unit.VisionRangeMeters })),
                ["performancePositions"] = selected == null ? null : new JArray(selected.Select(point => new JArray(point.x, point.y, point.z))),
                ["safeControlCandidates"] = controlCandidates.Count,
                ["positions"] = allPoints == null ? null : new JArray(allPoints.Select(point => new JArray(point.x, point.y, point.z))) },
                "Same finite walking candidates: two real performance listeners retain LOS/range; three separated effect-only controls retain navigation/height/acquisition safety without a casting or performance claim. All points precede spawning/time advance.");
            if (allPoints == null) throw new InvalidOperationException("Bounded safe listener/control placement unavailable; no wider survey or intake movement.");
            return allPoints;
        }

        private static BlueprintBuff Sprint32SavedControlBuff(string role)
        { return Sprint32SavedBlueprint<BlueprintBuff>(role == "charm" ? "CharmBuff" : role == "sleep" ? "SleepBuff" : "LillendAzata.HoldPersonBuff"); }

        private static JObject Sprint32ControlState(UnitEntityData unit, UnitEntityData source)
        {
            var row = unit.Get<UnitPartSprint32ListenerReceipt>();
            var blueprint = Sprint32SavedControlBuff(row.Role);
            var buffs = unit.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, blueprint) &&
                ReferenceEquals(buff.Context?.MaybeCaster, source)).ToArray();
            bool active = buffs.Length == 1 && buffs[0].Active && !buffs[0].IsDisposed &&
                !buffs[0].IsPermanent && buffs[0].EndTime > Game.Instance.Player.GameTime;
            var neutralFaction = row.Role == "charm" ? blueprint.GetComponent<NeutralToPlayer>().Faction : null;
            bool condition = row.Role == "charm" ? !unit.AttackFactions.Contains(neutralFaction) && !source.IsEnemy(unit) :
                unit.Descriptor.State.HasCondition(row.Role == "sleep" ? UnitCondition.Sleeping : UnitCondition.Paralyzed);
            return new JObject { ["unit"] = unit.UniqueId, ["source"] = source.UniqueId, ["role"] = row.Role,
                ["blueprint"] = blueprint.AssetGuid, ["group"] = unit.GroupId, ["active"] = active,
                ["condition"] = condition, ["sleeping"] = unit.Descriptor.State.HasCondition(UnitCondition.Sleeping),
                ["paralyzed"] = unit.Descriptor.State.HasCondition(UnitCondition.Paralyzed),
                ["neutralFaction"] = neutralFaction?.AssetGuid,
                ["sourceAbility"] = buffs.Length == 1 ? buffs[0].Context?.SourceAbility?.AssetGuid : null,
                ["casterLevel"] = buffs.Length == 1 ? (JToken)new JValue(buffs[0].Context.Params.CasterLevel) : JValue.CreateNull(),
                ["dc"] = buffs.Length == 1 ? (JToken)new JValue(buffs[0].Context.Params.DC) : JValue.CreateNull(),
                ["isFromSpell"] = buffs.Length == 1 && buffs[0].IsFromSpell,
                ["isNotDispelable"] = buffs.Length == 1 && buffs[0].IsNotDispelable,
                ["endTicks"] = buffs.Length == 1 ? (JToken)new JValue(buffs[0].EndTime.Ticks) : JValue.CreateNull(),
                ["attackFactions"] = new JArray(unit.AttackFactions.Select(faction => faction.AssetGuid).OrderBy(value => value, StringComparer.Ordinal)) };
        }

        private IEnumerable<int> PrepareSprint32SavedControls(UnitEntityData caster, UnitEntityData[] sources,
            Vector3[] points, UnitEntityData[] intake)
        {
            if (!Game.Instance.IsPaused || points.Length != 3 || sources.Any(unit => unit.IsBrainActive))
                throw new InvalidOperationException("Private saved-control setup requires paused exact inactive sources.");
            var primary = sources[1]; var sibling = sources[0];
            if (primary.Get<UnitPartRelease143RosterReceipt>().Key != "lillend-azata" ||
                sibling.Get<UnitPartRelease143RosterReceipt>().Key != "satyr")
                throw new InvalidOperationException("Private saved-control source ordering differs.");
            var roles = new[] { "charm", "sleep", "hold" };
            var controls = new List<UnitEntityData>();
            var listeners = Sprint32SavedListeners(caster);
            if (listeners.Length != 2 || listeners.Select(unit => unit.Get<UnitPartSprint32ListenerReceipt>().ListenerFaction).Distinct().Count() != 1)
                throw new InvalidOperationException("Exact selected listener identity required before control setup.");
            var listenerFaction = Sprint32ListenerFaction(listeners[0]);
            if (ReferenceEquals(listenerFaction, caster.Faction) || intake.Any(unit => unit.AttackFactions.Contains(listenerFaction)))
                throw new InvalidOperationException("Registered control faction is attacked by intake; no control target is spawned.");
            for (int index = 0; index < roles.Length; index++)
            {
                var target = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    points[index], Quaternion.identity, caster.HoldingState);
                Game.Instance.EntityCreator.Tick();
                var source = index == 1 ? sibling : primary;
                var receipt = target.Ensure<UnitPartSprint32ListenerReceipt>();
                receipt.Scope = Sprint32ListenerScope; receipt.UnitId = target.UniqueId; receipt.CasterId = caster.UniqueId;
                receipt.SourceId = source.UniqueId; receipt.Role = roles[index]; receipt.PrepareRunId = _request.RunId;
                receipt.ListenerFaction = listenerFaction.AssetGuid;
                SetExpandedSummoningBrainActive(target, false); target.GroupId = target.UniqueId;
                target.Descriptor.SwitchFactions(listenerFaction, true); target.AttackFactions.Match(new[] { caster.Faction });
                target.Stats.HitPoints.BaseValue = 100000; target.Stats.SaveWill.BaseValue = -100;
                PlaceExpandedSummoningUnit(target, points[index]);
                if (!Game.Instance.State.AwakeUnits.Contains(target)) Game.Instance.State.AwakeUnits.Add(target);
                var group = target.Group;
                if (!Sprint32OwnsListener(target, caster, sources) || target.GroupId != target.UniqueId ||
                    group.IsPlayerParty || group.Count != 1 || !ReferenceEquals(group[0], target))
                    throw new InvalidOperationException("Control target group is not exclusively owned; no group refresh.");
                group.ResetFactionSet(); group.UpdateAttackFactionsCache();
                if (!Sprint32OwnsListener(target, caster, sources) || target.Group.IsPlayerParty ||
                    intake.Any(unit => target.IsEnemy(unit) || unit.IsEnemy(target)))
                    throw new InvalidOperationException("Private saved-control fixture reaches unrelated intake; no save.");
                receipt.OriginalAttackFactions = new JArray(target.AttackFactions.Select(faction => faction.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal)).ToString(Formatting.None);
                controls.Add(target);
            }
            // Establish actual hostile baseline through normal cache propagation.
            var party = Game.Instance.Player.Party.ToArray();
            var owned = sources.Concat(Sprint32SavedAuxiliaries(caster)).ToArray();
            foreach (var step in ResumeSprint32Working(party, owned)) yield return step;
            for (int frame = 0; frame < 3; frame++) { Sprint32CalmWorking(party, owned); yield return 0; }
            Game.Instance.IsPaused = true;
            foreach (var target in controls)
            {
                var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
                var source = sources.Single(unit => unit.UniqueId == receipt.SourceId);
                var buff = Sprint32SavedControlBuff(receipt.Role);
                var ability = Sprint32SavedBlueprint<BlueprintAbility>(receipt.Role == "charm" ? "LillendAzata.CharmMonster" :
                    receipt.Role == "sleep" ? "Satyr.Sleep" : "LillendAzata.HoldPerson");
                var book = receipt.Role == "sleep" ? null : source.Descriptor.GetSpellbook(source.Blueprint.GetComponent<LillendBardBook32>().Book);
                var data = book == null ? new AbilityData(source.Descriptor.Abilities.GetAbility(ability)) :
                    book.GetKnownSpells(receipt.Role == "charm" ? 3 : 2).Single(value => ReferenceEquals(value.Blueprint, ability));
                var parameters = data.CalculateParams();
                var applications = Sprint32ControlApplications(ability.GetComponents<AbilityEffectRunAction>().Single().Actions)
                    .Where(value => ReferenceEquals(value.Buff, buff)).ToArray();
                var neutral = receipt.Role == "charm" ? buff.GetComponent<NeutralToPlayer>().Faction : null;
                bool baseline = target.IsEnemy(caster) && source.IsEnemy(target) &&
                    (neutral == null || target.AttackFactions.Contains(neutral)) &&
                    parameters.CasterLevel == (receipt.Role == "sleep" ? 8 : 7) &&
                    parameters.DC == (receipt.Role == "charm" ? 17 : receipt.Role == "sleep" ? 15 : 16) &&
                    applications.Length > 0 && applications.All(value => !value.Permanent && !value.AsChild &&
                        value.IsFromSpell == applications[0].IsFromSpell && value.IsNotDispelable == applications[0].IsNotDispelable);
                RosterPersistenceCheck("s32-control-baseline-" + receipt.Role, baseline, new JObject {
                    ["unit"] = target.UniqueId, ["source"] = source.UniqueId, ["ability"] = ability.AssetGuid,
                    ["cl"] = parameters.CasterLevel, ["dc"] = parameters.DC, ["neutralFaction"] = neutral?.AssetGuid,
                    ["attackFactions"] = JArray.Parse(receipt.OriginalAttackFactions), ["hostileToCaster"] = target.IsEnemy(caster),
                    ["sourceHostileToTarget"] = source.IsEnemy(target), ["applicationCount"] = applications.Length },
                    "The registered effect-only recipient has actual hostile baseline and the real granted/book ability parameters and mapped terminal flags.");
                if (!baseline) throw new InvalidOperationException("Private saved-control baseline/context is not the declared native shape.");
                var context = new AbilityExecutionContext(data, parameters, new Kingmaker.Utility.TargetWrapper(target), null);
                var duration = TimeSpan.FromSeconds(receipt.Role == "charm" ? 604800 : receipt.Role == "sleep" ? 480 : 42);
                var rule = new RuleApplyBuff(target, buff, context, duration,
                    (blueprint, parent, time) => target.Descriptor.Buffs.AddBuff(blueprint, parent, time));
                Rulebook.Trigger(rule);
                var applied = rule.AppliedBuff;
                if (applied == null || !applied.Active || applied.IsDisposed || !ReferenceEquals(applied.Context?.MaybeCaster, source))
                    throw new InvalidOperationException("Native private persistence-only control application failed.");
                applied.IsFromSpell = applications[0].IsFromSpell;
                applied.IsNotDispelable = applications[0].IsNotDispelable;
            }
            foreach (var step in ResumeSprint32Working(party, owned)) yield return step;
            for (int frame = 0; frame < 3; frame++) { Sprint32CalmWorking(party, owned); yield return 0; }
            Game.Instance.IsPaused = true;
            foreach (var target in controls)
            {
                var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
                var source = sources.Single(unit => unit.UniqueId == receipt.SourceId);
                var state = Sprint32ControlState(target, source);
                receipt.ControlState = state.ToString(Formatting.None);
                bool ready = (bool)state["active"] && (bool)state["condition"];
                RosterPersistenceCheck("s32-control-prepared-" + receipt.Role, ready, state,
                    "Native effect-only persistence precondition with production duration and exact source; no casting, initial save, AI or cost claim. Hold automatic saves remain enabled.");
                if (!ready) throw new InvalidOperationException("Private saved-control precondition did not remain active.");
            }
        }

        private void RequireSprint32ControlUnchanged(UnitEntityData unit, UnitEntityData[] sources, string stage)
        {
            var receipt = unit.Get<UnitPartSprint32ListenerReceipt>();
            var source = sources.Single(value => value.UniqueId == receipt.SourceId);
            var actual = Sprint32ControlState(unit, source);
            bool exact = (bool)actual["active"] && (bool)actual["condition"] && !unit.Destroyed &&
                !unit.Descriptor.State.IsDead && receipt.ControlState == actual.ToString(Formatting.None);
            RosterPersistenceCheck("s32-control-" + stage + "-" + receipt.Role, exact, new JObject {
                ["saved"] = receipt.ControlState == null ? null : JObject.Parse(receipt.ControlState), ["actual"] = actual },
                "Exact private buff/source/deadline, actual condition and faction state remain live; earlier Hold save or expiry cannot prove source cleanup.");
            if (!exact) throw new InvalidOperationException("Private saved-control state differs at " + stage + ": " + receipt.Role);
        }

        private void ObserveSprint32SavedControls(UnitEntityData caster, UnitEntityData[] sources, string stage)
        {
            var controls = Sprint32SavedControls(caster);
            if (controls.Length != 3 || Sprint32SavedAuxiliaries(caster).Length != 5 ||
                !controls.Select(unit => unit.Get<UnitPartSprint32ListenerReceipt>().Role).OrderBy(value => value)
                    .SequenceEqual(new[] { "charm", "hold", "sleep" }) || controls.Any(unit => !Sprint32OwnsListener(unit, caster, sources) ||
                    unit.GroupId != unit.UniqueId || unit.Group.IsPlayerParty || !ReferenceEquals(unit.Faction, Sprint32ListenerFaction(unit)) ||
                    string.IsNullOrEmpty(unit.Get<UnitPartSprint32ListenerReceipt>().OriginalAttackFactions)))
                throw new InvalidOperationException("Private saved-control ownership/role boundary differs.");
            foreach (var unit in controls) RequireSprint32ControlUnchanged(unit, sources, stage);
        }

        private void ObserveSprint32ControlRemoval(UnitEntityData[] controls, UnitEntityData caster, UnitEntityData source, string stage)
        {
            foreach (var unit in controls)
            {
                var receipt = unit.Get<UnitPartSprint32ListenerReceipt>();
                var factions = new JArray(unit.AttackFactions.Select(faction => faction.AssetGuid).OrderBy(value => value, StringComparer.Ordinal));
                bool clear = !unit.Destroyed && unit.IsInState && !unit.Descriptor.State.IsDead &&
                    !unit.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, Sprint32SavedControlBuff(receipt.Role))) &&
                    !unit.Descriptor.State.HasCondition(UnitCondition.Sleeping) && !unit.Descriptor.State.HasCondition(UnitCondition.Paralyzed) &&
                    receipt.OriginalAttackFactions == factions.ToString(Formatting.None) && unit.IsEnemy(caster);
                RosterPersistenceCheck("s32-control-removed-" + stage + "-" + receipt.Role, clear,
                    new JObject { ["unit"] = unit.UniqueId, ["source"] = source.UniqueId, ["attackFactions"] = factions,
                        ["originalAttackFactions"] = receipt.OriginalAttackFactions, ["alive"] = !unit.Destroyed && !unit.Descriptor.State.IsDead,
                        ["hostileAgain"] = unit.IsEnemy(caster) },
                    "Source retirement removes the private effect and actual condition and restores baseline attack factions/hostility while the recipient remains alive.");
                if (!clear) throw new InvalidOperationException("Private saved-control removal/faction restoration failed: " + receipt.Role);
            }
        }

        private IEnumerable<int> CleanupSprint32PersistentState(UnitEntityData caster, UnitEntityData[] sources)
        {
            var listeners = Sprint32SavedListeners(caster);
            var controls = Sprint32SavedControls(caster);
            ObserveSprint32SavedControls(caster, sources, "precleanup");
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
            var firstControls = controls.Where(unit => unit.Get<UnitPartSprint32ListenerReceipt>().SourceId == first.UniqueId).ToArray();
            var siblingControls = controls.Except(firstControls).ToArray();
            double retirementGameSeconds = Game.Instance.Player.GameTime.TotalSeconds;
            // Existing fixture retirement manually detaches/disposes equipment.
            // This proves saved equipment state and private visual/effect teardown,
            // not native equipment expiry. Retain actual grant/area references.
            foreach (var step in ObserveRosterResourceCleanup(new[] { first }, "s32-primary")) yield return step;
            var party = Game.Instance.Player.Party.ToArray();
            var survivors = siblings.Concat(listeners).Concat(controls).ToArray();
            foreach (var step in ResumeSprint32Working(party, survivors)) yield return step;
            for (int frame = 0; frame < 3; frame++)
            { Sprint32CalmWorking(party, survivors); yield return 0; }
            Game.Instance.IsPaused = true;
            ObserveSprint32ControlRemoval(firstControls, caster, first, "primary");
            foreach (var unit in siblingControls) RequireSprint32ControlUnchanged(unit, sources, "sibling");
            // Native group caches do not run in Pause. Normal frames above may
            // cross an upkeep boundary; only the surviving performance pool may
            // decrease within actual elapsed rounds, as in precleanup checks.
            bool siblingStateExact = siblings.Select((source, index) => {
                var before = JObject.Parse(siblingStates[index]);
                var current = Sprint32SavedState(source, listeners);
                bool upkeep = true;
                if (source.Get<UnitPartRelease143RosterReceipt>().Key == "lillend-azata")
                {
                    int oldPool = (int)before["resources"]["PerformanceResource"], newPool = (int)current["resources"]["PerformanceResource"];
                    upkeep = newPool > 0 && newPool <= oldPool && oldPool - newPool <=
                        Math.Ceiling((Game.Instance.Player.GameTime.TotalSeconds - retirementGameSeconds) / 6d) + 1;
                    current["resources"]["PerformanceResource"] = oldPool;
                }
                return !source.Destroyed && upkeep && JToken.DeepEquals(before, current);
            }).All(value => value);
            bool isolated = firstGrants.Length > 0 && firstAreas.Length > 0 &&
                firstGrants.All(buff => !buff.Owner.Buffs.RawFacts.Contains(buff)) &&
                firstAreas.All(area => area.Destroyed && !area.IsInState) &&
                listeners.All(unit => !unit.Descriptor.State.HasCondition(UnitCondition.Dazed)) &&
                siblingGrants.All(buff => buff.Owner.Buffs.RawFacts.Contains(buff)) && siblingStateExact;
            RosterPersistenceCheck("s32-cold-sibling-isolation", isolated, new JObject { ["retiredSource"] = first.UniqueId,
                ["capturedPrimaryGrants"] = firstGrants.Length, ["capturedPrimaryAreas"] = firstAreas.Length,
                ["retirementGameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds - retirementGameSeconds,
                ["siblingStateBefore"] = new JArray(siblingStates.Select(value => JObject.Parse(value))),
                ["siblingStateAfter"] = new JArray(siblings.Select(source => Sprint32SavedState(source, listeners))),
                ["retainedSiblingGrants"] = siblingGrants.Count(buff => buff.Owner.Buffs.RawFacts.Contains(buff)) },
                "Exact first-source removal clears its saved Fascinate/assistance/resistance and Dazed while preserving each sibling's same grants, resources and selected recipient.");
            if (!isolated) throw new InvalidOperationException("Sprint32 cold sibling isolation failed; no cleanup save.");
            foreach (var step in ObserveRosterResourceCleanup(siblings, "s32-siblings")) yield return step;
            survivors = listeners.Concat(controls).ToArray();
            foreach (var step in ResumeSprint32Working(party, survivors)) yield return step;
            for (int frame = 0; frame < 3; frame++)
            { Sprint32CalmWorking(party, survivors); yield return 0; }
            Game.Instance.IsPaused = true;
            foreach (var source in siblings)
                ObserveSprint32ControlRemoval(siblingControls.Where(unit => unit.Get<UnitPartSprint32ListenerReceipt>().SourceId == source.UniqueId).ToArray(), caster, source, "remaining");
            bool clean = grants.All(buff => !buff.Owner.Buffs.RawFacts.Contains(buff)) &&
                areas.All(area => area.Destroyed && !area.IsInState) &&
                listeners.All(unit => !unit.Descriptor.State.HasCondition(UnitCondition.Dazed));
            RosterPersistenceCheck("s32-loaded-source-cleanup", clean, new JObject { ["capturedGrants"] = grants.Length,
                ["capturedAreas"] = areas.Length, ["grantResidue"] = grants.Count(buff => buff.Owner.Buffs.RawFacts.Contains(buff)),
                ["dazedListeners"] = listeners.Count(unit => unit.Descriptor.State.HasCondition(UnitCondition.Dazed)) },
                "Removing the exact saved sources clears captured private grants/areas and actual listener Dazed state before fixture victims are retired.");
            if (!clean) throw new InvalidOperationException("Sprint32 native cleanup left state; no cleanup save.");
            foreach (var listener in listeners.Concat(controls)) listener.Destroy();
            Game.Instance.EntityDestroyer.Tick(); yield return 0;
        }
    }
}
