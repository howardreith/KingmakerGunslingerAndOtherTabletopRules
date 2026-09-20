using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Controllers;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Existing guarded Player/service fixture for optional-mod profiles
        // without a compatible campaign save. No terrain/UI/persistence claim.
        private RuntimeTestResult RunMagicCircleProfileNative()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableMagicCircleProfile ||
                !_request.ExitAfterCompletion || _request.Parameters == null || _request.Parameters.Count != 0)
                throw new InvalidOperationException("Exact save-free automatic-exit Circle profile request required.");
            var game = Game.Instance; bool paused = game.IsPaused;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string> { "native-game-version=" + GameVersion.Cached };
            var actors = new List<UnitEntityData>(); var prototypes = new List<BlueprintUnit>();
            var areas = game.State.AreaEffects.All.ToArray();
            ElementalNativeProfileCreatorFixture fixture = null;
            CircleProfileSaveGuard guard = null;
            CircleProfilePauseScope pauseScope = null;
            Kingmaker.Items.ItemEntity[] items = null; int[] counts = null, charges = null;
            object levelController = null; string failure = null;
            BlueprintFaction enemyFaction = null;
            try {
                guard = new CircleProfileSaveGuard(_request.RunId);
                fixture = new ElementalNativeProfileCreatorFixture(diagnostics); fixture.Initialize();
                if (!fixture.Ready) throw new InvalidOperationException("Native profile fixture did not become ready.");
                // Native Pause cannot start over MainMenu. Use the existing
                // request-local mode-token convention only for this synchronous
                // empty-world fixture; no rule outcome or controller is replaced.
                pauseScope = new CircleProfilePauseScope();
                diagnostics.Add("profile-native-mode=" + pauseScope.Before + "->" + game.CurrentMode);
                items = game.Player.Inventory.ToArray();
                counts = items.Select(item => item.Count).ToArray(); charges = items.Select(item => item.Charges).ToArray();
                var anchor = fixture.Main; var center = anchor.Position + new Vector3(12f, 0, 0);
                var caster = CircleProfileSpawn("ProfileCaster", center, anchor, actors, prototypes);
                var bearer = CircleProfileSpawn("ProfileBearer", center + new Vector3(.5f, 0, 0), anchor, actors, prototypes);
                enemyFaction = UnityEngine.Object.Instantiate(caster.Blueprint.Faction);
                enemyFaction.name = "KMG_Runtime_MagicCircle_ProfileHostile";
                enemyFaction.Peaceful = enemyFaction.AlwaysEnemy = enemyFaction.Neutral = enemyFaction.IsDirectlyControllable = false;
                enemyFaction.Dummy = null; enemyFaction.AttackFactions = new[] { caster.Blueprint.Faction };
                var recipient = CircleProfileSpawn("ProfileRecipient", center + new Vector3(1f, 0, 0), anchor, actors, prototypes, enemyFaction);
                var controller = CircleProfileSpawn("ProfileController", center + new Vector3(10f, 0, 0), anchor, actors, prototypes);
                // The menu host has no navigation graph, so native movement
                // deliberately skips spatial registration. Register only these
                // static fixture actors in the real native grid before casting.
                // AreaEffectEntityData still performs every membership predicate.
                foreach (var actor in actors)
                    game.CurrentScene.Area.InteractiveObjectGrid.MoveTo(actor, actor.Position.x, actor.Position.z);
                diagnostics.Add("profile-spatial-registration=static native grid; no navigation/terrain claim");
                caster.Descriptor.Alignment.Set(Alignment.LawfulGood);
                bearer.Descriptor.Alignment.Set(Alignment.ChaoticGood);
                recipient.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf", "native profile Sorcerer");
                caster.Stats.Charisma.BaseValue = 30;
                AdvanceDisposableSpellcaster(caster.Descriptor, sorcerer, 8, ref levelController);
                var book = caster.Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
                while (book.CasterLevel < 8) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false); book.Rest();
                // Same production commands and real failed-save/control checks
                // as the save-backed four-variant acceptance.
                CircleFamily(caster, bearer, recipient, controller, book, actors, assertions, diagnostics);
                CirclePublicationContracts(assertions);
            }
            catch (Exception exception) { failure = exception.ToString(); }
            finally {
                try {
                    if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null);
                    foreach (var area in game.State.AreaEffects.All.Except(areas).ToArray()) {
                        if (!BlueprintBootstrap.MagicCircles.Any(circle => ReferenceEquals(circle.Area, area.Blueprint)) ||
                            !actors.Contains(area.Context.MaybeCaster))
                            throw new InvalidOperationException("Unowned area appeared in the native Circle profile fixture.");
                        area.ForceEnd(); area.Tick();
                    }
                    foreach (var actor in actors) if (!actor.Destroyed) actor.Destroy();
                    game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                    foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                    if (enemyFaction != null) UnityEngine.Object.Destroy(enemyFaction);
                    if (items != null) {
                        if (items.Where((item, index) => !game.Player.Inventory.Contains(item) ||
                            item.Count != counts[index] || item.Charges != charges[index]).Any())
                            throw new InvalidOperationException("Native profile changed a pre-existing setup item.");
                        // Exact synchronous starter-item instances from our
                        // native spawns; no blueprint-wide inventory removal.
                        foreach (var item in game.Player.Inventory.Except(items).ToArray()) game.Player.Inventory.Remove(item);
                    }
                    pauseScope?.Dispose(); fixture?.Dispose();
                }
                catch (Exception exception) { failure = (failure ?? "") + "\ncleanup: " + exception; }
                finally { try { pauseScope?.Dispose(); } finally { guard?.Dispose(); } }
            }
            assertions.Add(Assertion("circle-profile-native-scope-restored",
                "original empty player/world/menu/inventory/services restored; no save load/write attempted",
                "restored=" + fixture?.Restored + ";blockedAttempts=" + guard?.Blocked,
                fixture != null && fixture.Restored && guard != null && guard.Blocked == 0 && guard.Removed &&
                    game.State.AreaEffects.All.SequenceEqual(areas) && game.IsPaused == paused &&
                    pauseScope != null && pauseScope.Restored,
                "existing native profile fixture with independent mutation/load guards; no campaign save used"));
            if (failure != null) assertions.Add(Assertion("circle-profile-execution", "no fixture exception", "failure", false, failure));
            string path = Path.Combine(_request.EvidenceDirectory, "magic-circle-native-profile.json");
            File.WriteAllText(path, new JObject { ["nativeGameVersion"] = GameVersion.Cached,
                ["contentEnabled"] = _context.FeatureModules.Active.MagicCircleSpells,
                ["controlEnhancementEnabled"] = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity,
                ["scope"] = "native Player/service fixture; real circle/control cast commands, typed rules, costs and installed class/list publication; no terrain, native UI, scene travel or disk-hydration claim",
                ["fixture"] = fixture?.Evidence, ["blockedSaveOrLoadAttempts"] = guard?.Blocked,
                ["saveGuardsRemoved"] = guard?.Removed, ["nativeModeBefore"] = pauseScope?.Before,
                ["nativeModeRestored"] = pauseScope?.Restored, ["exception"] = failure }.ToString(Formatting.Indented));
            var result = CreateResult(failure == null && assertions.All(value => value.Status == RuntimeTestStatuses.Pass) ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, failure);
            result.Diagnostics.AddRange(diagnostics); result.EvidenceFiles.Add(path); return result;
        }

        private static UnitEntityData CircleProfileSpawn(string role, Vector3 position, UnitEntityData anchor,
            List<UnitEntityData> actors, List<BlueprintUnit> prototypes, BlueprintFaction faction = null)
        {
            var game = Game.Instance;
            var blueprint = UnityEngine.Object.Instantiate(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_MagicCircle_" + role;
            blueprint.Race = anchor.Descriptor.Progression.Race;
            blueprint.IsCheater = true; blueprint.Brain = null;
            if (faction != null) blueprint.Faction = faction;
            prototypes.Add(blueprint);
            var unit = game.EntityCreator.SpawnUnit(blueprint, position, Quaternion.identity, game.Player.CrossSceneState);
            actors.Add(unit); game.EntityCreator.Tick();
            // CrossSceneState is part of the native creation/destruction graph.
            // Do not use the host's private scene for actors whose permanent
            // removal is itself under test. Require native pool registration.
            if (!game.State.Units.All.Contains(unit) || !ReferenceEquals(unit.HoldingState, game.Player.CrossSceneState))
                throw new InvalidOperationException("Native profile cross-scene actor registration failed.");
            unit.IsInGame = true; unit.IsInFogOfWar = false;
            if (unit.View == null || !unit.IsInState) throw new InvalidOperationException("Native profile actor has no active view/state.");
            unit.View.SetVisible(true, true);
            unit.Descriptor.CustomName = "KMG_RUNTIME_MAGIC_CIRCLE_" + role;
            unit.Stats.HitPoints.BaseValue = 10000;
            return unit;
        }

        // Same bounded native GameMode stack/count convention already used by
        // ElementalNativeTurnScope.SetFirearmPaused. MainMenu rejects native Pause requests;
        // this token isolates synchronous ticks without starting a campaign.
        private sealed class CircleProfilePauseScope : IDisposable
        {
            private readonly Stack<GameMode> _modes;
            private readonly GameMode[] _before;
            private readonly GameMode _owned;
            private readonly int[] _counts, _countsBefore;
            internal string Before { get; }
            internal bool Restored { get; private set; }
            internal CircleProfilePauseScope()
            {
                _modes = typeof(Game).GetField("m_GameModes", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(Game.Instance) as Stack<GameMode>;
                _counts = typeof(Game).GetField("m_ModesCount", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(Game.Instance) as int[];
                if (_modes == null || _counts == null) throw new MissingFieldException("Game native mode registration");
                if (_counts[(int)GameModeType.Pause] != 0 || Game.Instance.IsPaused)
                    throw new InvalidOperationException("A foreign pause exists before native profile registration.");
                _countsBefore = (int[])_counts.Clone();
                _before = _modes.ToArray(); Before = Game.Instance.CurrentMode.ToString();
                _owned = new GameMode(GameModeType.Pause, new IController[0]);
                _modes.Push(_owned); _counts[(int)GameModeType.Pause]++;
                if (!Game.Instance.IsPaused) { Dispose(); throw new InvalidOperationException("Native profile pause token inactive."); }
            }
            public void Dispose()
            {
                if (Restored) return;
                if (_modes.Count != _before.Length + 1 || !ReferenceEquals(_modes.Peek(), _owned) ||
                    !_modes.Skip(1).SequenceEqual(_before) ||
                    _counts.Where((value, index) => value != _countsBefore[index] + (index == (int)GameModeType.Pause ? 1 : 0)).Any())
                    throw new InvalidOperationException("Native mode stack changed outside the owned profile pause token.");
                _modes.Pop(); _counts[(int)GameModeType.Pause]--;
                Restored = _modes.SequenceEqual(_before) && _counts.SequenceEqual(_countsBefore);
            }
        }

        private sealed class CircleProfileSaveGuard : IDisposable
        {
            private static CircleProfileSaveGuard _active;
            private readonly string _id;
            private readonly HarmonyInstance _harmony;
            private readonly List<MethodBase> _methods = new List<MethodBase>();
            internal int Blocked { get; private set; }
            internal bool Removed { get; private set; }
            internal CircleProfileSaveGuard(string runId)
            {
                if (_active != null) throw new InvalidOperationException("Circle profile guard already active.");
                _id = "KMG.MagicCircleProfile." + runId; _harmony = HarmonyInstance.Create(_id); _active = this;
                try {
                    var methods = typeof(SaveManager).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var mutations = methods.Where(method => method.Name == "SaveRoutine" || method.Name == "SaveStashedArea" ||
                        method.Name == "DeleteSave" || method.Name == "RemoveSaveFromList").ToArray();
                    var loads = methods.Where(method => method.Name == "LoadRoutine").ToArray();
                    if (mutations.Length != 5 || loads.Length == 0) throw new InvalidOperationException("Native save/load guard contracts changed.");
                    foreach (var method in mutations.Concat(loads)) {
                        bool routine = method.Name == "SaveRoutine" || method.Name == "LoadRoutine";
                        if (routine && method.ReturnType != typeof(IEnumerator<object>))
                            throw new InvalidOperationException("Native save/load coroutine contract changed.");
                        _harmony.Patch(method, new HarmonyMethod(typeof(CircleProfileSaveGuard).GetMethod(
                            routine ? "BlockRoutine" : "BlockMutation", BindingFlags.Static | BindingFlags.NonPublic)));
                        _methods.Add(method);
                    }
                }
                catch { Dispose(); throw; }
            }
            private static bool BlockRoutine(ref IEnumerator<object> __result)
            { if (_active == null) return true; _active.Blocked++; __result = Enumerable.Empty<object>().GetEnumerator(); return false; }
            private static bool BlockMutation()
            { if (_active == null) return true; _active.Blocked++; return false; }
            public void Dispose()
            {
                foreach (var method in _methods) _harmony.Unpatch(method, HarmonyPatchType.Prefix, _id);
                _methods.Clear(); if (ReferenceEquals(_active, this)) _active = null; Removed = true;
            }
        }
    }
}
