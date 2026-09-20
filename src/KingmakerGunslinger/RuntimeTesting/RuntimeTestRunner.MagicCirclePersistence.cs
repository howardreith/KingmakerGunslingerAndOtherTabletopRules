using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string CircleSavedPrefix = "KMG_RUNTIME_MAGIC_CIRCLE_SAVED_";
        private static readonly string[] CircleSavedRoles = { "CasterA", "CasterB", "Bearer", "Recipient" };
        private bool _circlePersistenceStarted, _circleSaveRequested, _circleSaveComplete;
        private Stopwatch _circlePersistenceClock;
        private readonly List<RuntimeTestAssertion> _circlePersistenceChecks = new List<RuntimeTestAssertion>();
        private readonly List<string> _circlePersistenceDiagnostics = new List<string>();
        private readonly JObject _circlePersistenceRecord = new JObject();
        private CircleSceneObservation _circleScene;
        private string[] _circleSceneParty, _circleSceneRemote, _circleSceneActorIds;

        private void PollMagicCirclePersistence()
        {
            try {
                var game = Game.Instance;
                if (!_request.ExitAfterCompletion || !RuntimeTestScenarioCatalog.IsMagicCirclePersistence(_request.Scenario) ||
                    (string)_request.Parameters["saveName"] != WorkingSaveSmokeScenario.ExpectedName ||
                    _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                    throw new InvalidOperationException("Exact guarded working-save completion and automatic exit required.");
                game.IsPaused = true;
                if (_circlePersistenceClock != null && _circlePersistenceClock.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                    throw new TimeoutException("Magic Circle persistence/scene operation did not complete.");
                if (_circleSaveRequested) {
                    if (_circleSaveComplete) FinishMagicCirclePersistence(null);
                    return;
                }
                var loading = LoadingProcess.Instance;
                if (loading.IsAwaitingUserInput) throw new InvalidOperationException("Unexpected native load prompt.");
                if (loading.IsLoadingInProcess || loading.IsLoadingScreenActive || loading.IsManualLoadingScreenActive) return;
                if (_circleScene != null) {
                    if (!_circleScene.Ready) return;
                    var after = CaptureCirclePersistence();
                    _circlePersistenceRecord["afterScene"] = after;
                    var before = (JObject)_circlePersistenceRecord["snapshot"];
                    CirclePersistenceCheck("scene-original-context-and-expiration", JToken.DeepEquals(before["carriers"], after["carriers"]) &&
                        JToken.DeepEquals(before["actors"], after["actors"]) && JToken.DeepEquals(before["control"], after["control"]),
                        "original caster IDs, bearer, levels, metamagic, deadlines, known spells and control survive native scene reconstruction");
                    CirclePersistenceCheck("scene-native-unload-and-reload", _circleScene.ActualReload && _circleScene.Saves == 0,
                        "native scene handle replaced; loading callbacks observed; zero saving throws during reconstruction");
                    _circlePersistenceRecord["sceneEvents"] = _circleScene.Events;
                    FinishMagicCirclePersistence(null);
                    return;
                }
                _circlePersistenceStarted = true;
                _circlePersistenceClock = Stopwatch.StartNew();
                if (game.Player.Party.Count != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException("The original working-save party boundary changed.");
                bool prepare = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCirclePrepare;
                bool absent = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCircleAbsent;
                if (prepare || absent) {
                    if (CircleSavedCandidates().Length != 0) throw new InvalidOperationException("Saved Circle fixture already exists.");
                    if (absent) {
                        var circles = BlueprintBootstrap.MagicCircles;
                        CirclePersistenceCheck("cleanup-fresh-load-absence", !game.State.AreaEffects.All.Any(area => circles.Any(c => ReferenceEquals(c.Area, area.Blueprint))) &&
                            !game.State.Units.All.SelectMany(unit => unit.Buffs.Enumerable).Any(buff => circles.Any(c => ReferenceEquals(c.Carrier, buff.Blueprint) || ReferenceEquals(c.Recipient, buff.Blueprint))),
                            "no saved fixture actor, circle area, carrier or derivative benefit after native cleanup save");
                        FinishMagicCirclePersistence(null); return;
                    }
                    PrepareCirclePersistence();
                }
                _circlePersistenceRecord["snapshot"] = CaptureCirclePersistence();
                if (_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCircleScene) {
                    var sceneActors = CircleSavedActors();
                    _circleSceneParty = game.Player.PartyCharacters.Select(value => value.UniqueId).ToArray();
                    _circleSceneRemote = game.Player.RemoteCompanions.Select(value => value.UniqueId).ToArray();
                    _circleSceneActorIds = sceneActors.Select(value => value.UniqueId).ToArray();
                    // Same request-local promotion as ElementalRacePersistenceScenario:
                    // native reload preserves traveling characters. These four
                    // temporary party references are never saved.
                    foreach (var actor in sceneActors) game.Player.PartyCharacters.Add(actor);
                    game.Player.InvalidateCharacterLists(); game.Player.UpdateCharacterLists();
                    _circlePersistenceRecord["sceneFixture"] = new JObject {
                        ["temporaryTravelers"] = new JArray(_circleSceneActorIds),
                        ["areaExcludedFromSave"] = game.CurrentlyLoadedArea.ExcludeFromSave,
                        ["holdingStates"] = new JArray(sceneActors.Select(actor => actor.HoldingState?.GetType().FullName)) };
                    _circleScene = new CircleSceneObservation();
                    _circleScene.Start();
                    game.ReloadArea();
                    return;
                }
                VerifyCirclePersistenceMechanics();
                if (_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCircleCleanup) CleanupCirclePersistence();
                if (prepare || _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCircleCleanup) {
                    if (_circlePersistenceChecks.Any(check => check.Status != RuntimeTestStatuses.Pass))
                        throw new InvalidOperationException("Persistence assertions failed before an authorized write.");
                    _workingSaveSmoke.ArmExactWorkingSaveWrite();
                    var save = typeof(Game).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Single(method => method.Name == "SaveGame" && method.ReturnType == typeof(void) &&
                            method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType.FullName == "Kingmaker.EntitySystem.Persistence.SaveInfo" &&
                            method.GetParameters()[1].ParameterType == typeof(Action));
                    _circleSaveRequested = true;
                    save.Invoke(game, new object[] { _workingSaveSmoke.WorkingDescriptor, new Action(() => _circleSaveComplete = true) });
                    return;
                }
                FinishMagicCirclePersistence(null);
            }
            catch (Exception exception) { FinishMagicCirclePersistence(exception.ToString()); }
        }

        private static UnitEntityData[] CircleSavedCandidates()
        { return Game.Instance.State.Units.All.Where(unit => unit.Descriptor.CustomName != null && unit.Descriptor.CustomName.StartsWith(CircleSavedPrefix, StringComparison.Ordinal)).ToArray(); }

        private UnitEntityData[] CircleSavedActors()
        {
            var candidates = CircleSavedCandidates();
            if (candidates.Length != CircleSavedRoles.Length || candidates.Any(unit => !ReferenceEquals(unit.Blueprint, BlueprintRoot.Instance.DefaultPlayerCharacter) ||
                (Game.Instance.Player.Party.Contains(unit) != (_circleSceneActorIds != null)) || !unit.IsInGame || unit.View == null || unit.Descriptor.State.IsDead))
                throw new InvalidOperationException("Exactly four living native-blueprint saved actors with the expected fixture membership are required: " +
                    new JArray(candidates.Select(unit => new JObject { ["role"] = unit.Descriptor.CustomName, ["id"] = unit.UniqueId,
                        ["blueprint"] = unit.Blueprint.AssetGuid, ["nativeBlueprint"] = ReferenceEquals(unit.Blueprint, BlueprintRoot.Instance.DefaultPlayerCharacter),
                        ["party"] = Game.Instance.Player.Party.Contains(unit), ["inGame"] = unit.IsInGame, ["view"] = unit.View != null,
                        ["dead"] = unit.Descriptor.State.IsDead, ["destroyed"] = unit.Destroyed,
                        ["holdingState"] = unit.HoldingState?.ToString() })).ToString(Formatting.None));
            return CircleSavedRoles.Select(role => candidates.Single(unit => unit.Descriptor.CustomName == CircleSavedPrefix + role)).ToArray();
        }

        private void PrepareCirclePersistence()
        {
            if (!_context.FeatureModules.Active.MagicCircleSpells) throw new InvalidOperationException("Prepare requires content enabled.");
            var game = Game.Instance;
            var anchor = game.Player.Party.First(unit => unit.IsInGame && unit.View != null);
            // Native registered blueprint only: these four request-owned entities
            // must hydrate in a new process. No shared blueprint is modified.
            foreach (string role in CircleSavedRoles) {
                var unit = game.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    anchor.Position + new Vector3(1f, 0, 0), Quaternion.identity, anchor.HoldingState);
                unit.Descriptor.CustomName = CircleSavedPrefix + role;
                unit.Stats.HitPoints.BaseValue = 10000;
            }
            game.EntityCreator.Tick();
            var actors = CircleSavedActors();
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer persistence spellbook");
            for (int index = 0; index < 2; index++) {
                object controller = null;
                try {
                    actors[index].Stats.Charisma.BaseValue = 30;
                    actors[index].Descriptor.Alignment.Set(Alignment.LawfulGood);
                    AdvanceDisposableSpellcaster(actors[index].Descriptor, sorcerer, 8 + index * 2, ref controller);
                    var book = actors[index].Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
                    while (book.CasterLevel < 8 + index * 2) book.AddCasterLevel();
                    book.UpdateAllSlotsSize(false); book.Rest(); book.AddKnown(3, circle.Spell, true);
                }
                finally { (controller as IDisposable)?.Dispose(); }
            }
            var dominate = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, "d7cbd2004ce66a042aeab2e95a3c5c61", "native Dominate Person");
            var dominated = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library, "c0f4e1c24c9cd334ca988ed1bd9d201f", "native domination persistence");
            if (actors[3].Buffs.AddBuff(dominated, new MechanicsContext(actors[0], actors[0].Descriptor, dominate, null, new TargetWrapper(actors[3])), TimeSpan.FromHours(1)) == null)
                throw new InvalidOperationException("Positive pre-existing control prerequisite failed.");
            for (int index = 0; index < 2; index++) {
                var book = actors[index].Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
                var data = new AbilityData(circle.Spell, book);
                if (index == 0) { var meta = new MetamagicData { SpellLevelCost = Metamagic.Extend.DefaultCost() }; meta.Add(Metamagic.Extend); data.MetamagicData = meta; }
                CircleCast(actors[index], actors[2], data, _circlePersistenceDiagnostics);
            }
        }

        private JObject CaptureCirclePersistence()
        {
            var game = Game.Instance;
            var actors = CircleSavedActors();
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            var carriers = CircleBuffs(actors[2], circle.Carrier).OrderBy(buff => buff.Context.MaybeCaster?.Descriptor.CustomName, StringComparer.Ordinal).ToArray();
            if (carriers.Length != 2) throw new InvalidOperationException("Exactly two saved native carriers required.");
            var areas = carriers.Select(CircleArea).ToArray();
            foreach (var area in areas) {
                if (area == null || area.IsEnded) throw new InvalidOperationException("The saved native area link is absent/ended.");
                CircleRefresh(area, actors); CircleRefresh(area, actors);
            }
            // Native scene teardown can leave ended entities until the ordinary
            // destruction tick; tick only exact ended Circle areas for these actors.
            foreach (var ended in game.State.AreaEffects.All.Where(area => ReferenceEquals(area.Blueprint, circle.Area) &&
                area.IsEnded && actors.Contains(area.Context.MaybeCaster)).ToArray()) ended.Tick();
            game.EntityDestroyer.Tick();
            var live = game.State.AreaEffects.All.Where(area => ReferenceEquals(area.Blueprint, circle.Area) && actors.Contains(area.Context.MaybeCaster)).ToArray();
            CirclePersistenceCheck("two-caster-area-ownership", live.Length == 2 && live.All(area => areas.Contains(area)) &&
                areas.Select(area => area.UniqueId).Distinct().Count() == 2 && carriers.Select(buff => buff.Context.MaybeCaster).Distinct().Count() == 2 &&
                areas.All(area => ReferenceEquals(area.Context.MaybeOwner, actors[2])) && carriers.All(buff => buff.Active && buff.TimeLeft > TimeSpan.Zero),
                "two original casters, one bearer, exactly one native area per original active carrier");
            CirclePersistenceCheck("two-caster-recipient-ownership", actors.All(unit => CircleBuffs(unit, circle.Recipient).Length == 2 &&
                CircleBuffs(unit, circle.Recipient).Select(buff => buff.SourceAreaEffectId).OrderBy(id => id).SequenceEqual(areas.Select(area => area.UniqueId).OrderBy(id => id))),
                "each covered actor retains exactly the two source-area contributions");
            var controls = actors[3].Buffs.Enumerable.Where(buff => buff.Blueprint.AssetGuid == "c0f4e1c24c9cd334ca988ed1bd9d201f").ToArray();
            CirclePersistenceCheck("pre-existing-control-preserved", controls.Length == 1 && controls[0].Active && ReferenceEquals(controls[0].Context.MaybeCaster, actors[0]),
                "original domination remains active after entry/load/reconstruction");
            return new JObject {
                ["area"] = game.CurrentlyLoadedArea.AssetGuid, ["clockTicks"] = game.Player.GameTime.Ticks,
                ["actors"] = new JArray(actors.Select(unit => new JObject { ["role"] = unit.Descriptor.CustomName, ["id"] = unit.UniqueId,
                    ["blueprint"] = unit.Blueprint.AssetGuid, ["books"] = new JArray(unit.Descriptor.Spellbooks.Select(book => new JObject {
                        ["blueprint"] = book.Blueprint.AssetGuid, ["level"] = book.CasterLevel, ["slots3"] = book.GetSpontaneousSlots(3), ["slots4"] = book.GetSpontaneousSlots(4),
                        ["known"] = new JArray(book.GetKnownSpells(3).Where(data => ReferenceEquals(data.Blueprint, circle.Spell)).Select(data => data.Blueprint.AssetGuid)) })) })),
                ["carriers"] = new JArray(carriers.Select(buff => new JObject { ["bearer"] = actors[2].UniqueId, ["caster"] = buff.Context.MaybeCaster.UniqueId,
                    ["blueprint"] = buff.Blueprint.AssetGuid, ["sourceSpell"] = buff.Context.SourceAbility?.AssetGuid,
                    ["level"] = buff.Context.Params.CasterLevel, ["endTimeTicks"] = buff.EndTime.Ticks, ["extend"] = buff.Context.HasMetamagic(Metamagic.Extend) })),
                ["areas"] = new JArray(areas.Select(area => new JObject { ["id"] = area.UniqueId, ["caster"] = area.Context.MaybeCaster.UniqueId, ["owner"] = area.Context.MaybeOwner.UniqueId })),
                ["control"] = new JArray(controls.Select(buff => new JObject { ["source"] = buff.Context.MaybeCaster.UniqueId, ["endTimeTicks"] = buff.EndTime.Ticks }))
            };
        }

        private void VerifyCirclePersistenceMechanics()
        {
            var actors = CircleSavedActors(); var caster = actors[0]; var target = actors[2];
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            var book = caster.Descriptor.Spellbooks.Single(value => value.GetKnownSpells(3).Any(data => ReferenceEquals(data.Blueprint, circle.Spell)));
            bool content = _context.FeatureModules.Active.MagicCircleSpells;
            bool enhancement = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
            CirclePersistenceCheck("startup-publication-and-known-spell", (BlueprintBootstrap.MagicCirclePublication != null) == content &&
                new AbilityData(circle.Spell, book).IsAvailable == content && book.GetKnownSpells(3).Count(data => ReferenceEquals(data.Blueprint, circle.Spell)) == 1,
                "known GUID hydrates; new cast availability follows content startup setting");
            var ability = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, "d7cbd2004ce66a042aeab2e95a3c5c61", "control source");
            var buff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library, "c0f4e1c24c9cd334ca988ed1bd9d201f", "control terminal");
            var alignment = caster.Descriptor.Alignment.Value;
            var capture = new CircleApplicationCapture(); EventBus.Subscribe(capture);
            try {
                caster.Descriptor.Alignment.Set(Alignment.LawfulGood);
                int ac = CircleAttackAC(caster, target), save = CircleSave(caster, target, ability);
                caster.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                CirclePersistenceCheck("hydrated-native-defenses", CircleAttackAC(caster, target) == ac + 2 && CircleSave(caster, target, ability) == save + 2,
                    "two saved circles give only +2 native typed defenses even with content disabled");
                capture.Clear();
                var applied = target.Buffs.AddBuff(buff, new MechanicsContext(caster, caster.Descriptor, ability, null, new TargetWrapper(target)), TimeSpan.FromMinutes(1));
                CirclePersistenceCheck("hydrated-shared-control-setting", capture.Count == 1 &&
                    (enhancement ? applied == null && !capture.LastCanApply : applied != null && capture.LastCanApply),
                    "new matching control obeys only the shared enhancement startup setting");
                applied?.Remove();
                caster.Descriptor.Alignment.Set(Alignment.LawfulGood); capture.Clear();
                applied = target.Buffs.AddBuff(buff, new MechanicsContext(caster, caster.Descriptor, ability, null, new TargetWrapper(target)), TimeSpan.FromMinutes(1));
                CirclePersistenceCheck("hydrated-wrong-alignment-positive", applied != null && capture.Count == 1 && capture.LastCanApply,
                    "same delivery actually applies from a nonmatching controller");
                applied?.Remove();
            }
            finally { EventBus.Unsubscribe(capture); caster.Descriptor.Alignment.Set(alignment); }
            _circlePersistenceRecord["settings"] = new JObject { ["magicCircleSpells"] = content, ["sharedControlEnhancement"] = enhancement };
        }

        private void CleanupCirclePersistence()
        {
            var game = Game.Instance; var actors = CircleSavedActors();
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            var foreignUnits = game.State.Units.All.Except(actors).ToArray(); var party = game.Player.Party.ToArray();
            var areas = CircleBuffs(actors[2], circle.Carrier).Select(CircleArea).ToArray();
            foreach (var carrier in CircleBuffs(actors[2], circle.Carrier)) carrier.Remove();
            foreach (var area in areas) CircleRefresh(area, actors);
            foreach (var unit in actors) unit.Destroy();
            game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
            CirclePersistenceCheck("exact-fixture-cleanup", CircleSavedCandidates().Length == 0 && game.State.Units.All.SequenceEqual(foreignUnits) &&
                game.Player.Party.SequenceEqual(party) && areas.All(area => area.Destroyed && !game.State.AreaEffects.All.Contains(area)) &&
                !foreignUnits.SelectMany(unit => unit.Buffs.Enumerable).Any(buff => areas.Any(area => buff.SourceAreaEffectId == area.UniqueId)),
                "only four named fixture actors and their exact native area contributions removed before cleanup save");
        }

        private void CirclePersistenceCheck(string name, bool pass, string contract)
        { _circlePersistenceChecks.Add(Assertion("circle-persistence-" + name, contract, "exact=" + pass, pass, "guarded native save/load or area lifecycle")); }

        private void FinishMagicCirclePersistence(string failure)
        {
            if (_circleScene != null) {
                _circlePersistenceRecord["sceneEvents"] = _circleScene.Events;
                _circlePersistenceRecord["actualSceneReload"] = _circleScene.ActualReload;
                _circlePersistenceRecord["sceneSavingThrows"] = _circleScene.Saves;
                _circleScene.Stop();
            }
            if (_circleSceneActorIds != null) {
                var player = Game.Instance.Player;
                player.PartyCharacters.RemoveAll(value => _circleSceneActorIds.Contains(value.UniqueId));
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                CirclePersistenceCheck("scene-temporary-party-restored",
                    player.PartyCharacters.Select(value => value.UniqueId).SequenceEqual(_circleSceneParty) &&
                    player.RemoteCompanions.Select(value => value.UniqueId).SequenceEqual(_circleSceneRemote),
                    "only the four temporary fixture party references removed; original party and remote companions retained; never saved");
                _circleSceneActorIds = null;
            }
            var evidence = _workingSaveSmoke.Stop();
            bool writes = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCirclePrepare || _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCircleCleanup;
            CirclePersistenceCheck("exact-save-boundary", evidence.DescriptorReferenceCorrelated && evidence.CompletionCallbackObserved &&
                !evidence.SaveWritingApiObserved && evidence.HooksRemoved && evidence.ExpectedWorkingSaveRoutineCount == (writes ? 1 : 0) &&
                (!writes || _circleSaveComplete && evidence.ExpectedWorkingStashedAreaCount >= 1),
                writes ? "one native SaveGame on the exact captured KMG_AUTOMATION_WORKING descriptor" : "correlated working load; no save writes");
            _circlePersistenceRecord["exception"] = failure;
            string path = Path.Combine(_request.EvidenceDirectory, "magic-circle-persistence.json");
            File.WriteAllText(path, _circlePersistenceRecord.ToString(Formatting.Indented));
            var result = CreateResult(failure == null && _circlePersistenceChecks.All(check => check.Status == RuntimeTestStatuses.Pass) ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                _circlePersistenceChecks, failure);
            result.WorkingSaveSmoke = evidence; result.EvidenceFiles.Add(path); result.Diagnostics.AddRange(_circlePersistenceDiagnostics);
            Complete(result);
        }

        private sealed class CircleSceneObservation : IAreaLoadingStagesHandler, IAreaActivationHandler, IGlobalRulebookHandler<RuleSavingThrow>
        {
            internal readonly JArray Events = new JArray();
            private readonly Dictionary<int, string> _before = new Dictionary<int, string>();
            private readonly HashSet<int> _unloaded = new HashSet<int>();
            private int _scenes, _complete, _active, _settle;
            internal int Saves;
            internal bool Ready => _scenes == 1 && _complete == 1 && _active == 1 && ++_settle >= 3;
            internal bool ActualReload => _before.Any(pair => _unloaded.Contains(pair.Key) && Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt).Any(scene => scene.isLoaded && scene.name == pair.Value && scene.handle != pair.Key));
            internal void Start() { foreach (var scene in Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Where(scene => scene.isLoaded)) _before.Add(scene.handle, scene.name);
                EventBus.Subscribe(this); SceneManager.sceneUnloaded += Unloaded; }
            internal void Stop() { EventBus.Unsubscribe(this); SceneManager.sceneUnloaded -= Unloaded; }
            private void Observe(string name) { Game.Instance.IsPaused = true; Events.Add(name); }
            private void Unloaded(Scene scene) { _unloaded.Add(scene.handle); Observe("unloaded:" + scene.name + ":" + scene.handle); }
            public void OnAreaScenesLoaded() { _scenes++; Observe("scenes-loaded"); }
            public void OnAreaLoadingComplete() { _complete++; Observe("loading-complete"); }
            public void OnAreaActivated() { _active++; Observe("activated"); }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) { Saves++; }
        }
    }
}
