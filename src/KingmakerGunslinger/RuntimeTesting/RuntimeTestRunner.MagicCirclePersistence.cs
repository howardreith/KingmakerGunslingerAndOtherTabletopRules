using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Area;
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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Commands;
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
        private int _circleSceneStage;
        private BlueprintArea _circleSceneOrigin;
        private Dictionary<string, Vector3> _circleScenePositions;
        private readonly JArray _circleTransitionEvents = new JArray();
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
                    if (_circleSceneStage == 2) {
                        var travelers = game.Player.Party.Where(unit => _circleSceneActorIds.Contains(unit.UniqueId)).ToArray();
                        var bearer = travelers.Single(unit => unit.Descriptor.CustomName == CircleSavedPrefix + "Bearer");
                        var carriers = BlueprintBootstrap.MagicCircles.SelectMany(circle => CircleBuffs(bearer, circle.Carrier))
                            .OrderBy(buff => buff.Blueprint.AssetGuid, StringComparer.Ordinal)
                            .ThenBy(buff => buff.Context.MaybeCaster?.Descriptor.CustomName, StringComparer.Ordinal).ToArray();
                        var rows = CirclePersistedCarriers(bearer, carriers);
                        _circlePersistenceRecord["worldMapCarriers"] = rows;
                        CirclePersistenceCheck("different-area-carriers-retained", travelers.Length == 4 &&
                            game.CurrentlyLoadedArea != _circleSceneOrigin &&
                            game.CurrentlyLoadedArea == game.BlueprintRoot.GlobalMap.GlobalMapEnterPoint.Area &&
                            JToken.DeepEquals(_circlePersistenceRecord["snapshot"]["carriers"], rows) && _circleScene.Saves == 0,
                            "actual world-map load retains all original timed carriers/casters/deadlines while local views unload; no saves");
                        FinishCircleSceneLeg("world-map");
                        _circleSceneStage = 3; _circleScene = new CircleSceneObservation(); _circleScene.Start();
                        // Installed private contract used by the public LoadArea:
                        // original registered area, no entry teleport, no autosave,
                        // ordinary unload and no SaveInfo/save-file operation.
                        typeof(Game).GetMethod("LoadArea", BindingFlags.Instance | BindingFlags.NonPublic, null,
                            new[] { typeof(BlueprintArea), typeof(BlueprintAreaEnterPoint), typeof(AutoSaveMode), typeof(bool), typeof(SaveInfo) }, null)
                            .Invoke(game, new object[] { _circleSceneOrigin, null, AutoSaveMode.None, false, null });
                        return;
                    }
                    if (_circleSceneStage == 3) {
                        // Native arrival may spread the party formation. Restore
                        // only the four fixture positions, never the effects.
                        foreach (var unit in CircleSavedActors()) unit.Position = _circleScenePositions[unit.UniqueId];
                    }
                    var after = CaptureCirclePersistence();
                    string phase = _circleSceneStage == 1 ? "scene" : "roundtrip";
                    _circlePersistenceRecord[_circleSceneStage == 1 ? "afterScene" : "afterRoundtrip"] = after;
                    var before = (JObject)_circlePersistenceRecord["snapshot"];
                    CirclePersistenceCheck(phase + "-original-context-and-expiration",
                        JToken.DeepEquals(before["area"], after["area"]) && JToken.DeepEquals(before["carriers"], after["carriers"]) &&
                        JToken.DeepEquals(before["actors"], after["actors"]) && JToken.DeepEquals(before["control"], after["control"]) &&
                        JToken.DeepEquals(before["market"], after["market"]),
                        "original area, caster IDs, bearer, levels, metamagic, deadlines, known spells, spent slots, held touch, market and control survive native reconstruction");
                    CirclePersistenceCheck(phase + "-native-unload-and-reload",
                        (_circleSceneStage != 1 || _circleScene.ActualReload) && _circleScene.Saves == 0,
                        "native loading callbacks and scene unload observed; zero saving throws during reconstruction");
                    FinishCircleSceneLeg(phase);
                    if (_circleSceneStage == 1) {
                        var entry = game.BlueprintRoot.GlobalMap.GlobalMapEnterPoint;
                        if (entry == null || entry.Area == _circleSceneOrigin) throw new InvalidOperationException("Distinct native world-map entry required.");
                        _circleSceneStage = 2; _circleScene = new CircleSceneObservation(); _circleScene.Start();
                        game.LoadArea(entry, AutoSaveMode.None);
                        return;
                    }
                    FinishMagicCirclePersistence(null);
                    return;
                }
                _circlePersistenceStarted = true;
                _circlePersistenceClock = Stopwatch.StartNew();
                _circlePersistenceRecord["nativeGameVersion"] = GameVersion.Cached;
                if (game.Player.Party.Count != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException("The original working-save party boundary changed.");
                if (MagicCirclePreparationBinding.RequiresBinding(_request.Scenario))
                    AuthorizeCirclePreparedPhase(BeginMagicCirclePersistencePhase);
                else BeginMagicCirclePersistencePhase();
            }
            catch (Exception exception) { FinishMagicCirclePersistence(exception.ToString()); }
        }

        private void BeginMagicCirclePersistencePhase()
        {
            var game = Game.Instance;
            _circlePersistenceRecord["schemaVersion"] = 2;
            _circlePersistenceRecord["runId"] = _request.RunId;
            _circlePersistenceRecord["phase"] = _request.Scenario.Substring("working-save-magic-circle-".Length);
            _circlePersistenceRecord["artifact"] = CirclePreparationArtifact();
            _circlePersistenceRecord["workingSave"] = CirclePreparationSave();
            bool prepare = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCirclePrepare;
            bool absent = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCircleAbsent;
            if (prepare || absent) {
                if (CircleSavedCandidates().Length != 0) throw new InvalidOperationException("Saved Circle fixture already exists.");
                if (absent) {
                    var circles = BlueprintBootstrap.MagicCircles;
                    CirclePersistenceCheck("cleanup-fresh-load-absence", !game.State.AreaEffects.All.Any(area => circles.Any(c => ReferenceEquals(c.Area, area.Blueprint))) &&
                        !game.State.Units.All.SelectMany(unit => unit.Buffs.Enumerable).Any(buff => circles.Any(c => ReferenceEquals(c.Carrier, buff.Blueprint) || ReferenceEquals(c.Recipient, buff.Blueprint))),
                        "no saved fixture actor, circle area, carrier or derivative benefit after native cleanup save");
                    CaptureCircleSavedMarketAbsence();
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
                _circleSceneOrigin = game.CurrentlyLoadedArea;
                _circleScenePositions = sceneActors.ToDictionary(unit => unit.UniqueId, unit => unit.Position);
                _circleSceneStage = 1;
                _circleScene = new CircleSceneObservation();
                _circleScene.Start();
                game.ReloadArea();
                return;
            }
            VerifyCirclePersistenceMechanics();
            VerifyCircleFavoredOracle(CircleSavedActors()[2]);
            if (_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMagicCircleCleanup) CleanupCirclePersistence();
            if (prepare) _circlePersistenceRecord["fixtureIdentity"] = ReadCirclePreparedFixture();
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
            var originalItems = game.Player.Inventory.ToArray();
            var originalInventory = CircleSavedItems(game.Player.Inventory);
            // Native registered blueprint only: these four request-owned entities
            // must hydrate in a new process. No shared blueprint is modified.
            foreach (string role in CircleSavedRoles) {
                var unit = game.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    anchor.Position + new Vector3(1f, 0, 0), Quaternion.identity, anchor.HoldingState);
                unit.Descriptor.CustomName = CircleSavedPrefix + role;
                unit.Stats.HitPoints.BaseValue = 10000;
            }
            game.EntityCreator.Tick();
            // DefaultPlayerCharacter supplies starter equipment into the shared
            // stash even for these non-party actors. Remove only the exact new
            // instances before any fixture save, using native slot-aware removal.
            foreach (var item in game.Player.Inventory.Except(originalItems).ToArray()) game.Player.Inventory.Remove(item).Dispose();
            CirclePersistenceCheck("spawn-inventory-isolation", game.Player.Inventory.SequenceEqual(originalItems) &&
                CircleSavedItems(game.Player.Inventory).SequenceEqual(originalInventory),
                "exact spawned starter items removed; original inventory instances, slots, counts and charges retained before persistence");
            var actors = CircleSavedActors();
            PrepareCircleFavoredOracle(actors[2]);
            foreach (var item in game.Player.Inventory.Except(originalItems).ToArray()) game.Player.Inventory.Remove(item).Dispose();
            CirclePersistenceCheck("favored-inventory-isolation", CircleSavedItems(game.Player.Inventory).SequenceEqual(originalInventory),
                "native Oracle level-up starter equipment cleaned before the prepared fixture save");
            var circles = BlueprintBootstrap.MagicCircles;
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
                    book.UpdateAllSlotsSize(false); book.Rest();
                    book.AddKnown(3, MagicCircleBlueprints.Family, true);
                }
                finally { (controller as IDisposable)?.Dispose(); }
            }
            var dominate = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, "d7cbd2004ce66a042aeab2e95a3c5c61", "native Dominate Person");
            var dominated = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library, "c0f4e1c24c9cd334ca988ed1bd9d201f", "native domination persistence");
            if (actors[3].Buffs.AddBuff(dominated, new MechanicsContext(actors[0], actors[0].Descriptor, dominate, null, new TargetWrapper(actors[3])), TimeSpan.FromHours(1)) == null)
                throw new InvalidOperationException("Positive pre-existing control prerequisite failed.");
            for (int index = 0; index < 2; index++) {
                var book = actors[index].Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
                foreach (var circle in circles) {
                var parent = new AbilityData(MagicCircleBlueprints.Family, book);
                if (index == 0) { var meta = new MetamagicData { SpellLevelCost = Metamagic.Extend.DefaultCost() }; meta.Add(Metamagic.Extend); parent.MetamagicData = meta; }
                var data = CircleGroupedVariant(parent, circle.Spell);
                CircleCast(actors[index], actors[2], data, _circlePersistenceDiagnostics);
                }
            }
            PrepareCircleSavedMarket(actors);
            // Preserve a real held-touch charge as an additional hydration
            // consumer, leaving its already spent slot and native pending
            // delivery intact. No direct UnitPartTouch construction.
            var heldCircle = circles.Single(value => value.Alignment == "Evil");
            var heldBook = actors[1].Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
            var heldData = CircleGroupedVariant(new AbilityData(MagicCircleBlueprints.Family, heldBook), heldCircle.Spell);
            int heldSlots = heldBook.GetSpontaneousSlots(3);
            var command = new UnitUseAbility(heldData, new TargetWrapper(actors[2]));
            if (!heldData.IsAvailable || !command.CanStart) throw new InvalidOperationException("Saved held-touch root is unavailable.");
            command.IgnoreCooldown(TimeSpan.Zero); actors[1].Commands.Run(command); command.Start(); CircleCompleteCommand(command, _circlePersistenceDiagnostics);
            CirclePersistenceCheck("native-held-touch-prepare", actors[1].Get<UnitPartTouch>()?.Ability.Data.Blueprint == heldCircle.Delivery &&
                heldBook.GetSpontaneousSlots(3) == heldSlots - 1, "real cast spends once and leaves a native pending touch for fresh hydration");
        }

        private JObject CaptureCirclePersistence()
        {
            var game = Game.Instance;
            var actors = CircleSavedActors();
            var circles = BlueprintBootstrap.MagicCircles;
            var carriers = circles.SelectMany(circle => CircleBuffs(actors[2], circle.Carrier)).OrderBy(buff => buff.Blueprint.AssetGuid, StringComparer.Ordinal)
                .ThenBy(buff => buff.Context.MaybeCaster?.Descriptor.CustomName, StringComparer.Ordinal).ToArray();
            if (carriers.Length != 8) throw new InvalidOperationException("Exactly two saved native carriers of each alignment required.");
            var areas = carriers.Select(CircleArea).ToArray();
            foreach (var area in areas) {
                if (area == null || area.IsEnded) throw new InvalidOperationException("The saved native area link is absent/ended.");
                CircleRefresh(area, actors); CircleRefresh(area, actors);
            }
            // Native scene teardown can leave ended entities until the ordinary
            // destruction tick; tick only exact ended Circle areas for these actors.
            foreach (var ended in game.State.AreaEffects.All.Where(area => circles.Any(circle => ReferenceEquals(area.Blueprint, circle.Area)) &&
                area.IsEnded && actors.Contains(area.Context.MaybeCaster)).ToArray()) ended.Tick();
            game.EntityDestroyer.Tick();
            var live = game.State.AreaEffects.All.Where(area => circles.Any(circle => ReferenceEquals(area.Blueprint, circle.Area)) && actors.Contains(area.Context.MaybeCaster)).ToArray();
            CirclePersistenceCheck("two-caster-area-ownership", live.Length == 8 && live.All(area => areas.Contains(area)) &&
                areas.Select(area => area.UniqueId).Distinct().Count() == 8 && carriers.Select(buff => buff.Context.MaybeCaster).Distinct().Count() == 2 &&
                areas.All(area => ReferenceEquals(area.Context.MaybeOwner, actors[2])) && carriers.All(buff => buff.Active && buff.TimeLeft > TimeSpan.Zero),
                "two original casters, one bearer, exactly one native area per original active carrier");
            CirclePersistenceCheck("two-caster-recipient-ownership", actors.All(unit => circles.All(circle => CircleBuffs(unit, circle.Recipient).Length == 2 &&
                CircleBuffs(unit, circle.Recipient).Select(buff => buff.SourceAreaEffectId).OrderBy(id => id).SequenceEqual(areas.Where(area => ReferenceEquals(area.Blueprint, circle.Area)).Select(area => area.UniqueId).OrderBy(id => id)))),
                "each covered actor retains exactly two contributions per alignment, eight total");
            CirclePersistenceCheck("one-native-boundary-per-area", areas.All(CircleBoundaryMatches) &&
                areas.Select(area => area.View.GetComponentInChildren<KingmakerGunslinger.Spells.MagicCircle.MagicCircleRadiusVisual>(true).GetInstanceID()).Distinct().Count() == 8,
                "eight overlapping saved auras have exactly one correctly colored radius renderer each after native view reconstruction");
            var controls = actors[3].Buffs.Enumerable.Where(buff => buff.Blueprint.AssetGuid == "c0f4e1c24c9cd334ca988ed1bd9d201f").ToArray();
            CirclePersistenceCheck("pre-existing-control-preserved", controls.Length == 1 && controls[0].Active && ReferenceEquals(controls[0].Context.MaybeCaster, actors[0]),
                "original domination remains active after entry/load/reconstruction");
            return new JObject {
                ["market"] = CaptureCircleSavedMarket(actors),
                ["area"] = game.CurrentlyLoadedArea.AssetGuid, ["clockTicks"] = game.Player.GameTime.Ticks,
                ["actors"] = new JArray(actors.Select(unit => new JObject { ["role"] = unit.Descriptor.CustomName, ["id"] = unit.UniqueId,
                    ["blueprint"] = unit.Blueprint.AssetGuid,
                    ["heldDelivery"] = unit.Get<UnitPartTouch>()?.Ability.Data.Blueprint.AssetGuid,
                    ["heldRoot"] = unit.Get<UnitPartTouch>()?.Ability.Data.StickyTouch?.Blueprint.AssetGuid, ["books"] = new JArray(unit.Descriptor.Spellbooks.Select(book => new JObject {
                        ["blueprint"] = book.Blueprint.AssetGuid, ["level"] = book.CasterLevel, ["slots3"] = book.GetSpontaneousSlots(3), ["slots4"] = book.GetSpontaneousSlots(4),
                        ["known"] = new JArray(book.GetKnownSpells(3).Where(data => (MagicCircleBlueprints.Families.Contains(data.Blueprint) || circles.Any(circle => ReferenceEquals(data.Blueprint, circle.Spell)))).Select(data => data.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal)) })) })),
                ["carriers"] = CirclePersistedCarriers(actors[2], carriers),
                ["areas"] = new JArray(areas.Select(area => new JObject { ["id"] = area.UniqueId, ["caster"] = area.Context.MaybeCaster.UniqueId, ["owner"] = area.Context.MaybeOwner.UniqueId })),
                ["control"] = new JArray(controls.Select(buff => new JObject { ["source"] = buff.Context.MaybeCaster.UniqueId, ["endTimeTicks"] = buff.EndTime.Ticks }))
            };
        }

        private static JArray CirclePersistedCarriers(UnitEntityData bearer, Kingmaker.UnitLogic.Buffs.Buff[] carriers)
        {
            return new JArray(carriers.Select(buff => new JObject { ["bearer"] = bearer.UniqueId, ["caster"] = buff.Context.MaybeCaster?.UniqueId,
                ["blueprint"] = buff.Blueprint.AssetGuid, ["sourceSpell"] = buff.Context.SourceAbility?.AssetGuid,
                ["level"] = buff.Context.Params.CasterLevel, ["endTimeTicks"] = buff.EndTime.Ticks, ["extend"] = buff.Context.HasMetamagic(Metamagic.Extend) }));
        }

        private void FinishCircleSceneLeg(string name)
        {
            _circleTransitionEvents.Add(new JObject { ["leg"] = name, ["events"] = _circleScene.Events.DeepClone(),
                ["sameSceneReplaced"] = _circleScene.ActualReload, ["savingThrows"] = _circleScene.Saves,
                ["loadedArea"] = Game.Instance.CurrentlyLoadedArea.AssetGuid });
            _circlePersistenceRecord["transitionLegs"] = _circleTransitionEvents;
            _circleScene.Stop(); _circleScene = null;
        }

        private void VerifyCirclePersistenceMechanics()
        {
            var actors = CircleSavedActors(); var caster = actors[0]; var target = actors[2];
            var circles = BlueprintBootstrap.MagicCircles;
            var book = caster.Descriptor.Spellbooks.Single(value => value.GetKnownSpells(3).Any(data => (MagicCircleBlueprints.Families.Contains(data.Blueprint) || circles.Any(circle => ReferenceEquals(data.Blueprint, circle.Spell)))));
            bool content = _context.FeatureModules.Active.MagicCircleSpells;
            bool enhancement = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
            _circlePersistenceRecord["groupedAvailability"] = new JObject {
                ["published"] = BlueprintBootstrap.MagicCirclePublication != null,
                ["contentEnabled"] = content,
                ["known"] = new JArray(book.GetKnownSpells(3).Select(data => new JObject {
                    ["spell"] = data.Blueprint.AssetGuid, ["level"] = data.SpellLevel,
                    ["metamagic"] = data.MetamagicData?.MetamagicMask.ToString(),
                    ["metamagicCost"] = data.MetamagicData?.SpellLevelCost })),
                ["variants"] = new JArray(circles.Select(circle => {
                    var parent = new AbilityData(MagicCircleBlueprints.Family, book);
                    var data = CircleGroupedVariant(parent, circle.Spell);
                    return new JObject { ["alignment"] = circle.Alignment, ["lookupKnown"] = book.IsKnown(circle.Spell),
                        ["learnedEntries"] = book.GetKnownSpells(3).Count(known => ReferenceEquals(known.Blueprint, circle.Spell)),
                        ["level"] = data.SpellLevel, ["parentLevel"] = parent.SpellLevel,
                        ["available"] = data.IsAvailable, ["parentCanSpend"] = book.CanSpend(parent),
                        ["forCast"] = data.IsAvailableForCast, ["slots3"] = book.GetSpontaneousSlots(3),
                        ["slots4"] = book.GetSpontaneousSlots(4) };
                })) };
            // The native post-load lookup can recognize usable variants even
            // when its learned-spell array contains only the parent. Count the
            // actual learned entries; IsKnown is not known-choice accounting.
            CirclePersistenceCheck("startup-publication-and-known-spell", (BlueprintBootstrap.MagicCirclePublication != null) == content &&
                book.GetKnownSpells(3).Count(data => ReferenceEquals(data.Blueprint, MagicCircleBlueprints.Family)) == 1 &&
                circles.All(circle => CircleGroupedVariant(new AbilityData(MagicCircleBlueprints.Family, book), circle.Spell).IsAvailable == content &&
                    !book.GetKnownSpells(3).Any(data => ReferenceEquals(data.Blueprint, circle.Spell))),
                "exactly one learned family and no separately learned children hydrate; each variant's cast availability follows content startup setting");
            var ability = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, "d7cbd2004ce66a042aeab2e95a3c5c61", "control source");
            var buff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library, "c0f4e1c24c9cd334ca988ed1bd9d201f", "control terminal");
            var alignment = caster.Descriptor.Alignment.Value;
            var capture = new CircleApplicationCapture(); EventBus.Subscribe(capture);
            try {
                caster.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                int ac = CircleAttackAC(caster, target), save = CircleSave(caster, target, ability);
                caster.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                CirclePersistenceCheck("hydrated-native-defenses", CircleAttackAC(caster, target) == ac + 2 && CircleSave(caster, target, ability) == save + 2,
                    "eight saved circles give only +2 native typed defenses even with content disabled");
                capture.Clear();
                var applied = target.Buffs.AddBuff(buff, new MechanicsContext(caster, caster.Descriptor, ability, null, new TargetWrapper(target)), TimeSpan.FromMinutes(1));
                CirclePersistenceCheck("hydrated-shared-control-setting", capture.Count == 1 &&
                    (enhancement ? applied == null && !capture.LastCanApply : applied != null && capture.LastCanApply),
                    "new matching control obeys only the shared enhancement startup setting");
                applied?.Remove();
                caster.Descriptor.Alignment.Set(Alignment.TrueNeutral); capture.Clear();
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
            if (!_circlePreparedMutationAuthorized) throw new InvalidOperationException("Cleanup has no matched preparation authorization.");
            var game = Game.Instance; var actors = CircleSavedActors();
            var circles = BlueprintBootstrap.MagicCircles;
            var foreignUnits = game.State.Units.All.Except(actors).ToArray(); var party = game.Player.Party.ToArray();
            var carriers = circles.SelectMany(circle => CircleBuffs(actors[2], circle.Carrier)).ToArray();
            var areas = carriers.Select(CircleArea).ToArray();
            foreach (var carrier in carriers) carrier.Remove();
            foreach (var area in areas) CircleRefresh(area, actors);
            CleanupCircleSavedMarket(actors);
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
