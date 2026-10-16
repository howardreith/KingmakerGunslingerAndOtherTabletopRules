using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.UI;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Alternate summoning through the real player surface on the guarded
    /// read-only working save: a disposable Master Summoner party member's
    /// granted root is opened from the native action bar, a creature is
    /// chosen by clicking its row in the native variant popup (an early row,
    /// the last row of the tier III menu, and the single Tyrannosaurus of
    /// tier VII, a recent creature with grapple and swallow), and the cast is
    /// confirmed through the native selected-ability click handler and then
    /// left to the unpaused native RTWP scheduler, animation and cooldown.
    /// The Tyrannosaurus is then ended through the engine's own expiry path and its
    /// dismemberment allowed to finish in native time. No IsActed, cooldown
    /// override, synchronous command tick or hotbar slot is used; no save is
    /// written. Turn-based combat is not exercised.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _altNaturalSteps;
        private Stopwatch _altNaturalWatch;
        private readonly List<RuntimeTestAssertion> _altNaturalAssertions = new List<RuntimeTestAssertion>();
        private readonly JArray _altNaturalCaptures = new JArray();
        private readonly List<string> _altNaturalDiagnostics = new List<string>();

        private string AltNaturalPath
        { get { return Path.Combine(_request.EvidenceDirectory, "alternate-summoning-natural.json"); } }

        internal const string AltNaturalExecution =
            "NATURAL: native action-bar popup row click (ActionBarSlot.OnClick), native target click (ClickWithSelectedAbilityHandler.OnClick), unpaused native RTWP scheduler, animation and command cooldown. No IsActed, cooldown override or synchronous command tick. Working save read-only.";

        private void PollAlternateSummoningNatural()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableAlternateSummoningNatural ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null ||
                !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException(
                    "Alternate summoning natural paths require the exact guarded read-only working save and mandatory exit.");
            if (_altNaturalWatch == null) _altNaturalWatch = Stopwatch.StartNew();
            if (_altNaturalWatch.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Alternate summoning natural paths timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive) return;
            if (_altNaturalSteps == null) _altNaturalSteps = RunAlternateSummoningNatural().GetEnumerator();
            Exception failure = null;
            try { if (_altNaturalSteps.MoveNext()) return; }
            catch (Exception error) { failure = error; }
            try { _altNaturalSteps.Dispose(); }
            catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
            _altNaturalSteps = null;
            WriteAltNatural(failure == null ? null : failure.ToString());
            RuntimeTestResult result = CreateResult(failure != null ? RuntimeTestStatuses.Error :
                _altNaturalAssertions.All(value => value.Status == "PASS") ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail, _altNaturalAssertions, failure == null ? null : failure.ToString());
            result.Diagnostics.AddRange(_altNaturalDiagnostics);
            result.EvidenceFiles.Add(AltNaturalPath);
            Complete(result);
        }

        private void StopAlternateSummoningNatural(RuntimeTestResult result)
        {
            if (_altNaturalSteps == null) return;
            IEnumerator<int> steps = _altNaturalSteps;
            _altNaturalSteps = null;
            string error = "Runner completed before the alternate summoning natural paths finished.";
            try { steps.Dispose(); }
            catch (Exception failure) { error += " " + failure; result.Status = RuntimeTestStatuses.Error; }
            WriteAltNatural(error);
            result.Diagnostics.Add(error);
        }

        private void WriteAltNatural(string error)
        {
            var document = new JObject {
                ["schemaVersion"] = 1, ["runId"] = _request.RunId,
                ["claims"] = "Real action-bar popup selection and natural RTWP casts of Master Summoner adapter roots (tiers I, III and VII) on the guarded read-only working save; native dismissal of the recent Tyrannosaurus. No hotbar slot, saved shortcut, turn-based round or save write.",
                ["execution"] = AltNaturalExecution,
                ["captures"] = _altNaturalCaptures,
                ["assertions"] = JArray.FromObject(_altNaturalAssertions),
                ["diagnostics"] = new JArray(_altNaturalDiagnostics),
                ["saveWriteObserved"] = _workingSaveSmoke != null && _workingSaveSmoke.WriteObserved,
                ["error"] = error
            };
            RuntimeTestResultWriter.WriteAtomic(AltNaturalPath, document.ToString(Formatting.Indented));
        }

        private void AltNaturalAssert(string id, string expected, string observed, bool pass, string evidence)
        {
            _altNaturalAssertions.Add(Assertion("natural-" + id, expected, observed, pass,
                evidence + " | " + AltNaturalExecution));
        }

        private void AltNaturalCapture(string stage, JObject state)
        {
            state["stage"] = stage;
            state["frame"] = Time.frameCount;
            _altNaturalCaptures.Add(state);
            WriteAltNatural(null);
        }

        private sealed class AltNaturalCast
        {
            internal string Label;
            internal AbilityData Selected;
            internal bool Clicked;
            internal bool Issued;
            internal string Result = "NotStarted";
            internal bool NaturallyActed;
            internal bool IgnoredCooldown;
            internal double Seconds;
            internal int Frames;
            internal int ResourceBefore;
            internal int ResourceAfter;
            internal UnitEntityData[] Summoned = new UnitEntityData[0];
            internal string ExpectedUnit;

            internal JObject ToJson()
            {
                return new JObject {
                    ["label"] = Label,
                    ["selectedChild"] = Selected == null ? null : Selected.Blueprint.AssetGuid + ":" + Selected.Blueprint.name,
                    ["convertedFrom"] = Selected == null || Selected.ConvertedFrom == null ? null :
                        Selected.ConvertedFrom.Blueprint.AssetGuid,
                    ["targetClickAccepted"] = Clicked, ["commandIssued"] = Issued, ["result"] = Result,
                    ["actedNaturally"] = NaturallyActed, ["cooldownOverride"] = IgnoredCooldown,
                    ["realSeconds"] = Math.Round(Seconds, 2), ["frames"] = Frames,
                    ["resourceBefore"] = ResourceBefore, ["resourceAfter"] = ResourceAfter,
                    ["expectedUnit"] = ExpectedUnit,
                    ["summoned"] = new JArray(Summoned.Select(value => value.UniqueId + ":" +
                        value.Blueprint.AssetGuid + ":" + value.Blueprint.name))
                };
            }
        }

        private IEnumerable<int> RunAlternateSummoningNatural()
        {
            Game game = Game.Instance;
            var ui = game.UI;
            Player player = game.Player;
            AlternateSummoningFamilyStatus status = AlternateSummoningStatusRegistry.Find(
                AlternateSummoningSourceCatalog.MasterSummonerKey);
            if (status == null || status.Disposition != AlternateSummoningDisposition.Published)
                throw new InvalidOperationException("Master Summoner is not published: " +
                    (status == null ? "<no status>" : status.Disposition + " " + status.Reason));
            if (game.IsControllerGamepad || game.CurrentMode != GameModeType.Default ||
                game.SelectedAbilityHandler.Ability != null ||
                TurnBased.Controllers.CombatController.IsInTurnBasedCombat() ||
                player.Party.Any(value => value.IsInCombat) || ui.ServiceWindow == null ||
                ui.ServiceWindow.WindowTabs.IsShow || DialogMessageBox.Instance == null ||
                DialogMessageBox.Instance.IsShown)
                throw new InvalidOperationException(
                    "Natural alternate summoning requires idle native desktop local-area UI out of combat.");

            UnitEntityData[] units = game.State.Units.All.ToArray();
            AreaEffectEntityData[] areas = game.State.AreaEffects.All.ToArray();
            UnitEntityData[] party = player.Party.ToArray();
            UnitEntityData[] selection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            UnitEntityData selectedGroup = GroupController.Instance.GetCurrentCharacter();
            ActionBarGroupElement[] actionGroups = ActionBarManager.Instance.Group.GroupElements.ToArray();
            bool[] groupToggles = actionGroups.Select(value => value.ToggleState).ToArray();
            bool paused = game.IsPaused;
            TimeSpan time = game.Player.GameTime;
            UnitEntityData anchor = party.FirstOrDefault(value => value.HoldingState != null && value.IsInGame);
            if (anchor == null) throw new InvalidOperationException("The working save has no in-area party anchor.");

            LibraryScriptableObject library = Bootstrap.BlueprintBootstrap.Library;
            var summoner = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                AlternateSummoningContracts.SummonerClass, "CotW Summoner class");
            var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                PlayerPathMasterSummonerArchetype, "CotW Master Summoner archetype");
            var resource = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(library,
                AlternateSummoningContracts.MasterSummonerResource, "Master Summoner resource");
            AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(
                AlternateSummoningSourceCatalog.MasterSummonerKey);

            BlueprintUnit prototype = null;
            UnitEntityData caster = null;
            object levelController = null;
            var summons = new List<UnitEntityData>();
            try
            {
                game.IsPaused = true;
                prototype = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                prototype.name = "KMG_Runtime_AlternateSummoning_Natural";
                prototype.IsCheater = false;
                caster = game.EntityCreator.SpawnUnit(prototype, anchor.Position + Vector3.right * 1.5f,
                    Quaternion.identity, anchor.HoldingState);
                game.EntityCreator.Tick();
                if (caster == null || !caster.IsInState || caster.View == null)
                    throw new InvalidOperationException("The disposable Master Summoner did not enter the area.");
                caster.Descriptor.CustomName = "KMG_RUNTIME_ALTERNATE_SUMMONING_NATURAL";
                SetExpandedSummoningBrainActive(caster, false);
                caster.Stats.HitPoints.BaseValue = 10000;
                caster.Descriptor.Stats.Charisma.BaseValue = 18;
                // Level 13: tier VII, where the Sprint 24 Tyrannosaurus is offered.
                AdvanceDisposableArchetypeCaster(caster.Descriptor, summoner, archetype, 13, ref levelController);
                caster.Descriptor.Resources.Restore(resource);
                player.PartyCharacters.Add(caster);
                player.InvalidateCharacterLists();
                player.UpdateCharacterLists();
                if (!player.Party.Contains(caster))
                    throw new InvalidOperationException("The disposable Master Summoner did not join the UI party.");
                ui.SelectionManagerPC.SelectUnit(caster.View, true, true, false);
                GroupController.Instance.SelectUnit(caster);
                for (int frame = 0; frame < 20; frame++) yield return 0;

                Ability tierOne = PlayerPathRootFact(caster, family, 1);
                Ability tierThree = PlayerPathRootFact(caster, family, 3);
                var early = new AltNaturalCast { Label = "early-row-tier1" };
                foreach (int frame in AltNaturalChoose(caster, tierOne, AltNaturalPick.FirstProjectCreature, early)) yield return frame;
                foreach (int frame in AltNaturalCastSelected(caster, resource, early, summons)) yield return frame;
                AltNaturalAssertCast(early, tierOne, 1);

                var late = new AltNaturalCast { Label = "last-row-tier3-scrolled" };
                foreach (int frame in AltNaturalChoose(caster, tierThree, AltNaturalPick.LastRow, late)) yield return frame;
                foreach (int frame in AltNaturalCastSelected(caster, resource, late, summons)) yield return frame;
                AltNaturalAssertCast(late, tierThree, 3);

                // A recent creature through the adapter: the single
                // Tyrannosaurus (Sprint 24; grapple and swallow) from tier VII.
                Ability tierSeven = PlayerPathRootFact(caster, family, 7);
                var recent = new AltNaturalCast { Label = "recent-tyrannosaurus-tier7" };
                foreach (int frame in AltNaturalChoose(caster, tierSeven, AltNaturalPick.Tyrannosaurus, recent)) yield return frame;
                foreach (int frame in AltNaturalCastSelected(caster, resource, recent, summons)) yield return frame;
                AltNaturalAssertCast(recent, tierSeven, 7);

                foreach (int frame in AltNaturalDismiss(caster, summons, recent.Summoned.FirstOrDefault())) yield return frame;
            }
            finally
            {
                game.IsPaused = true;
                try { game.ClickEventsController.ClearPointerMode(); game.SelectedAbilityHandler.DropAbility(); }
                catch (Exception error) { _altNaturalDiagnostics.Add("pointer cleanup: " + error.Message); }
                if (levelController != null)
                    try { levelController.GetType().GetMethod("Cancel").Invoke(levelController, null); }
                    catch (Exception) { }
                foreach (UnitEntityData unit in summons)
                    if (!unit.Destroyed) CleanupExpandedSummoningUnit(unit);
                foreach (UnitEntityData unit in game.State.Units.All.Except(units).ToArray())
                {
                    if (unit == caster) continue;
                    Buff lifecycle = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(value =>
                        ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                    if (caster != null && lifecycle != null && lifecycle.MaybeContext != null &&
                        ReferenceEquals(lifecycle.MaybeContext.MaybeCaster, caster) && !unit.Destroyed)
                        CleanupExpandedSummoningUnit(unit);
                }
                if (caster != null)
                {
                    player.PartyCharacters.RemoveAll(value => value.UniqueId == caster.UniqueId);
                    if (!caster.Destroyed) caster.Destroy();
                }
                game.EntityDestroyer.Tick();
                game.EntityDestroyer.Tick();
                if (prototype != null) UnityEngine.Object.Destroy(prototype);
                player.InvalidateCharacterLists();
                player.UpdateCharacterLists();
                GroupController.Instance.SelectUnit(selectedGroup);
                ui.SelectionManagerPC.MultiSelect(selection.Select(value => value.View).ToArray(), false);
                for (int index = 0; index < actionGroups.Length; index++)
                    actionGroups[index].Toggle(groupToggles[index], true);
                game.IsPaused = paused;
                UnitEntityData[] extra = game.State.Units.All.Except(units).ToArray();
                UnitEntityData[] missing = units.Except(game.State.Units.All).ToArray();
                AreaEffectEntityData[] extraAreas = game.State.AreaEffects.All.Except(areas).ToArray();
                AltNaturalCapture("cleanup", new JObject {
                    ["extraUnits"] = new JArray(extra.Select(value => value.UniqueId + ":" + value.Blueprint.name)),
                    ["missingUnits"] = new JArray(missing.Select(value => value.UniqueId + ":" + value.Blueprint.name)),
                    ["extraAreas"] = new JArray(extraAreas.Select(value => value.Blueprint.name)),
                    ["party"] = player.Party.SequenceEqual(party),
                    ["gameTimeAdvancedSeconds"] = (game.Player.GameTime - time).TotalSeconds,
                    ["saveWriteObserved"] = _workingSaveSmoke.WriteObserved });
                AltNaturalAssert("cleanup",
                    "the disposable summoner and every summon are gone; the original units, areas and party are unchanged; nothing was saved (native time advanced while unpaused, by design)",
                    "extraUnits=" + extra.Length + ";missingUnits=" + missing.Length + ";extraAreas=" +
                        extraAreas.Length + ";party=" + player.Party.SequenceEqual(party),
                    extra.Length == 0 && missing.Length == 0 && extraAreas.Length == 0 &&
                        player.Party.SequenceEqual(party) && !_workingSaveSmoke.WriteObserved,
                    "Game.State.Units/AreaEffects census; Player.Party; working-save write guard");
            }
        }

        private enum AltNaturalPick { FirstProjectCreature, LastRow, Tyrannosaurus }

        private static string AltNaturalSpawnName(AbilityData data)
        {
            SummonSpawn spawn = SummonSignatureReader.Read(data.Blueprint).Spawns().FirstOrDefault();
            BlueprintScriptableObject unit = null;
            if (spawn != null) Bootstrap.BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(spawn.Unit, out unit);
            return unit == null ? "" : unit.name;
        }

        /// <summary>Opens the native abilities group, the root's own variant
        /// popup, and clicks one row: the first project creature, the last
        /// row after the menu has been scrolled to its end, or the single
        /// Tyrannosaurus.</summary>
        private IEnumerable<int> AltNaturalChoose(UnitEntityData caster, Ability root, AltNaturalPick pick,
            AltNaturalCast cast)
        {
            bool last = pick == AltNaturalPick.LastRow;
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Func<ActionBarGroupSlot, bool> isRoot = row =>
            {
                var data = row.MechanicSlot == null ? null : row.MechanicSlot.GetContentData() as AbilityData;
                return data != null && ReferenceEquals(data.Blueprint, root.Blueprint) &&
                    ReferenceEquals(data.Fact, root);
            };
            FieldInfo slotType = typeof(ActionBarGroupElement).GetField("SlotType", fields);
            ActionBarGroupElement[] abilityGroups = ActionBarManager.Instance.Group.GroupElements.Where(value =>
                (ActionBarSlotType)slotType.GetValue(value) == ActionBarSlotType.ActivatableAbility).ToArray();
            foreach (ActionBarGroupElement element in abilityGroups)
                if (!element.ToggleState) element.OnClick();
            for (int frame = 0; frame < 20; frame++) yield return 0;
            ActionBarGroupSlot[] sources = abilityGroups.SelectMany(value =>
                value.GetComponentsInChildren<ActionBarGroupSlot>(true)).Where(row =>
                    row.gameObject.activeInHierarchy && isRoot(row)).ToArray();
            if (sources.Length != 1)
            {
                AltNaturalCapture("source-missing-" + cast.Label, new JObject {
                    ["root"] = root.Blueprint.AssetGuid, ["abilityGroups"] = abilityGroups.Length,
                    ["sources"] = sources.Length,
                    ["rows"] = new JArray(ActionBarManager.Instance.GetComponentsInChildren<ActionBarGroupSlot>(true)
                        .Select(row =>
                        {
                            var data = row.MechanicSlot == null ? null : row.MechanicSlot.GetContentData() as AbilityData;
                            return row.name + ":" + row.gameObject.activeInHierarchy + ":" +
                                (data == null ? "<none>" : data.Blueprint.AssetGuid);
                        })) });
                throw new InvalidOperationException("The granted root's action-bar slot count is " +
                    sources.Length + ".");
            }
            ActionBarGroupSlot source = sources[0];
            source.OnToggleGroupClick();
            for (int frame = 0; frame < 20; frame++) yield return 0;
            var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", fields)
                .GetValue(source);
            if (popup == null) throw new InvalidOperationException("The root's slot created no variant popup.");
            ActionBarSlot[] rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(row =>
                row.gameObject.activeInHierarchy && row.MechanicSlot != null &&
                row.MechanicSlot.GetContentData() is AbilityData &&
                ((AbilityData)row.MechanicSlot.GetContentData()).ConvertedFrom != null &&
                ReferenceEquals(((AbilityData)row.MechanicSlot.GetContentData()).ConvertedFrom.Blueprint,
                    root.Blueprint)).ToArray();
            BlueprintAbility[] exposed = root.Blueprint.Variants.Where(value => value != null &&
                AlternateSummoningExposure.IsExposed(value.AssetGuid)).ToArray();
            ExpandedSummoningVariantMenuSnapshot snapshot;
            bool navigation = ExpandedSummoningVariantMenuRuntime.TryValidateNavigation(popup, out snapshot);
            ActionBarSlot chosenRow;
            if (last)
            {
                ScrollRect scroll = popup.GetComponentInChildren<ScrollRect>(true);
                if (scroll != null) scroll.verticalNormalizedPosition = 0f;
                for (int frame = 0; frame < 6; frame++) yield return 0;
                chosenRow = rows.LastOrDefault();
            }
            else if (pick == AltNaturalPick.Tyrannosaurus)
            {
                ActionBarSlot[] matches = rows.Where(row =>
                {
                    var data = (AbilityData)row.MechanicSlot.GetContentData();
                    SummonSpawn spawn = SummonSignatureReader.Read(data.Blueprint).Spawns().FirstOrDefault();
                    return spawn != null && (spawn.CountDice == "One" || spawn.CountDice == "Zero") &&
                        AltNaturalSpawnName(data) == "KMG_Summoning_Unit_Tyrannosaurus";
                }).ToArray();
                chosenRow = matches.Length == 1 ? matches[0] : null;
                if (chosenRow != null && !AltNaturalRowVisible(popup, chosenRow))
                {
                    // Bring the row into view as a player would by scrolling.
                    ScrollRect scroll = popup.GetComponentInChildren<ScrollRect>(true);
                    if (scroll != null) scroll.verticalNormalizedPosition =
                        1f - (float)Array.IndexOf(rows, chosenRow) / Math.Max(1, rows.Length - 1);
                    for (int frame = 0; frame < 6; frame++) yield return 0;
                }
            }
            else
            {
                chosenRow = rows.FirstOrDefault(row => AltNaturalSpawnName((AbilityData)row.MechanicSlot
                    .GetContentData()).StartsWith("KMG_Summoning_", StringComparison.Ordinal));
            }
            AbilityData rowData = chosenRow == null ? null : (AbilityData)chosenRow.MechanicSlot.GetContentData();
            bool rowVisible = chosenRow != null && AltNaturalRowVisible(popup, chosenRow);
            var state = new JObject {
                ["root"] = root.Blueprint.AssetGuid + ":" + root.Blueprint.name,
                ["rows"] = rows.Length, ["exposedVariants"] = exposed.Length,
                ["navigationVerified"] = navigation,
                ["scrollingRequired"] = snapshot != null && snapshot.ScrollingRequired,
                ["firstReachable"] = snapshot != null && snapshot.FirstEntryReachable,
                ["lastReachable"] = snapshot != null && snapshot.LastEntryReachable,
                ["chosenIndex"] = chosenRow == null ? -1 : Array.IndexOf(rows, chosenRow),
                ["chosen"] = rowData == null ? null : rowData.Blueprint.AssetGuid + ":" + rowData.Blueprint.name,
                ["chosenRowInViewport"] = rowVisible };
            AltNaturalCapture("popup-" + cast.Label, state);
            AltNaturalAssert("popup-" + cast.Label,
                "the granted root opens its native variant popup; every row is an exposed child converted from this root and navigation is verified" +
                    (last ? "; the last row is reachable and in view (scrolled to the end when the menu needs scrolling)" :
                        "; an early project-creature row is in view"),
                state.ToString(Formatting.None),
                rows.Length >= 1 && rows.Select(row =>
                    ((AbilityData)row.MechanicSlot.GetContentData()).Blueprint).All(exposed.Contains) &&
                    chosenRow != null && rowVisible && navigation &&
                    (!last || snapshot.LastEntryReachable),
                "ActionBarGroupSlot.OnToggleGroupClick; ActionBarSpellsGroup rows; ExpandedSummoningVariantMenuRuntime.TryValidateNavigation");
            if (chosenRow == null) throw new InvalidOperationException("No row to choose under " + root.Blueprint.name + ".");
            chosenRow.OnClick();
            for (int frame = 0; frame < 6; frame++) yield return 0;
            AbilityData selected = Game.Instance.SelectedAbilityHandler.Ability;
            cast.Selected = selected;
            source.HideSubGroup();
            bool exact = selected != null && ReferenceEquals(selected.Blueprint, rowData.Blueprint) &&
                selected.ConvertedFrom != null && ReferenceEquals(selected.ConvertedFrom.Blueprint, root.Blueprint) &&
                ReferenceEquals(selected.Fact, root);
            AltNaturalAssert("row-click-" + cast.Label,
                "clicking the row selects exactly that child, converted from the granted root fact",
                selected == null ? "<none>" : selected.Blueprint.AssetGuid + ":" + selected.Blueprint.name, exact,
                "ActionBarSlot.OnClick; ClickWithSelectedAbilityHandler.Ability");
            if (!exact) throw new InvalidOperationException("The row click did not select the requested child.");
        }

        private static bool AltNaturalRowVisible(ActionBarSpellsGroup popup, ActionBarSlot row)
        {
            ScrollRect scroll = popup.GetComponentInChildren<ScrollRect>(true);
            RectTransform viewport = scroll == null ? null : scroll.viewport;
            var rect = row.transform as RectTransform;
            if (rect == null) return false;
            if (viewport == null) return row.gameObject.activeInHierarchy;
            var rowCorners = new Vector3[4];
            var viewCorners = new Vector3[4];
            rect.GetWorldCorners(rowCorners);
            viewport.GetWorldCorners(viewCorners);
            float rowMinY = Mathf.Min(rowCorners[0].y, rowCorners[2].y), rowMaxY = Mathf.Max(rowCorners[0].y, rowCorners[2].y);
            float viewMinY = Mathf.Min(viewCorners[0].y, viewCorners[2].y), viewMaxY = Mathf.Max(viewCorners[0].y, viewCorners[2].y);
            return rowMaxY > viewMinY && rowMinY < viewMaxY;
        }

        /// <summary>Confirms the selected child on a point beside the caster
        /// through the native click handler and lets native time run until
        /// the command ends and the caster's standard-action cooldown has
        /// passed.</summary>
        private IEnumerable<int> AltNaturalCastSelected(UnitEntityData caster,
            BlueprintAbilityResource resource, AltNaturalCast cast, List<UnitEntityData> summons)
        {
            Game game = Game.Instance;
            SummonSpawn spawn = SummonSignatureReader.Read(cast.Selected.Blueprint).Spawns().First();
            cast.ExpectedUnit = spawn.Unit;
            UnitEntityData[] before = game.State.Units.All.ToArray();
            cast.ResourceBefore = caster.Descriptor.Resources.GetResourceAmount(resource);
            Vector3 point = caster.Position + caster.View.transform.forward * 2f;
            cast.Clicked = game.SelectedAbilityHandler.OnClick(null, point, 0);
            UnitUseAbility command = caster.Commands.GetCommand<UnitUseAbility>();
            cast.Issued = cast.Clicked && command != null &&
                ReferenceEquals(command.Spell.Blueprint, cast.Selected.Blueprint);
            if (!cast.Issued)
            {
                AltNaturalCapture("cast-not-issued-" + cast.Label, cast.ToJson());
                yield break;
            }
            cast.IgnoredCooldown = command.IsIgnoreCooldown;
            var watch = Stopwatch.StartNew();
            game.IsPaused = false;
            while (watch.Elapsed.TotalSeconds < 30 && (!command.IsFinished ||
                caster.CombatState.HasCooldownForCommand(UnitCommand.CommandType.Standard)))
            {
                cast.Frames++;
                yield return 0;
            }
            game.IsPaused = true;
            cast.Seconds = watch.Elapsed.TotalSeconds;
            cast.Result = command.Result.ToString();
            cast.NaturallyActed = command.IsActed;
            cast.ResourceAfter = caster.Descriptor.Resources.GetResourceAmount(resource);
            cast.Summoned = game.State.Units.All.Except(before).Where(unit =>
            {
                Buff lifecycle = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(value =>
                    ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                return lifecycle != null && lifecycle.MaybeContext != null &&
                    ReferenceEquals(lifecycle.MaybeContext.MaybeCaster, caster);
            }).ToArray();
            summons.AddRange(cast.Summoned);
            AltNaturalCapture("cast-" + cast.Label, cast.ToJson());
        }

        private void AltNaturalAssertCast(AltNaturalCast cast, Ability root, int tier)
        {
            bool group = cast.Selected != null && SummonSignatureReader.Read(cast.Selected.Blueprint).Spawns()
                .First().CountDice != "One" && SummonSignatureReader.Read(cast.Selected.Blueprint).Spawns()
                .First().CountDice != "Zero";
            AltNaturalAssert("cast-" + cast.Label,
                "the target click issues the native command for the chosen child; native RTWP time completes it with the native act and no cooldown override; one Master Summoner use is spent and the chosen creature appears (" +
                    (group ? "one group" : "one creature") + ")",
                cast.ToJson().ToString(Formatting.None),
                cast.Clicked && cast.Issued && cast.Result == "Success" && cast.NaturallyActed &&
                    !cast.IgnoredCooldown && cast.ResourceBefore - cast.ResourceAfter == 1 &&
                    cast.Summoned.Length >= 1 && (group || cast.Summoned.Length == 1) &&
                    cast.Summoned.All(unit => unit.Blueprint.AssetGuid == cast.ExpectedUnit),
                "ClickWithSelectedAbilityHandler.OnClick; UnitUseAbility; native RTWP; UnitAbilityResourceCollection");
        }

        /// <summary>Ends one summon through the engine's expiry path and lets
        /// native time finish its dismemberment; the pool releases it.</summary>
        private IEnumerable<int> AltNaturalDismiss(UnitEntityData caster, List<UnitEntityData> summons,
            UnitEntityData preferred)
        {
            Game game = Game.Instance;
            UnitEntityData unit = preferred != null && !preferred.Destroyed &&
                !preferred.Descriptor.State.IsDead ? preferred : summons.FirstOrDefault(value =>
                    !value.Destroyed && !value.Descriptor.State.IsDead);
            if (unit == null) yield break;
            ISummonPool free = PlayerPathPool(AlternateSummoningContracts.MasterSummonerFreePool);
            ISummonPool bound = PlayerPathPool(AlternateSummoningContracts.SummonerPool);
            bool memberBefore = free.Units.Contains(unit) || bound.Units.Contains(unit);
            unit.Descriptor.RemoveFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            bool dismembering = unit.Descriptor.HasFact(BlueprintRoot.Instance.SystemMechanics.DismemberedUnitBuff);
            var watch = Stopwatch.StartNew();
            int frames = 0;
            game.IsPaused = false;
            while (watch.Elapsed.TotalSeconds < 30 && !unit.Destroyed && !unit.Descriptor.State.IsDead)
            {
                frames++;
                yield return 0;
            }
            for (int frame = 0; frame < 10; frame++) yield return 0;
            game.IsPaused = true;
            bool released = !free.Units.Contains(unit) && !bound.Units.Contains(unit);
            var state = new JObject {
                ["unit"] = unit.UniqueId + ":" + unit.Blueprint.name, ["poolMemberBefore"] = memberBefore,
                ["dismembering"] = dismembering, ["destroyed"] = unit.Destroyed,
                ["dead"] = unit.Descriptor.State.IsDead, ["poolReleased"] = released,
                ["realSeconds"] = Math.Round(watch.Elapsed.TotalSeconds, 2), ["frames"] = frames };
            AltNaturalCapture("dismissal", state);
            AltNaturalAssert("dismissal",
                "ending a summon through the engine's expiry path starts its dismemberment; native time finishes it and its summon pool releases it",
                state.ToString(Formatting.None),
                memberBefore && dismembering && (unit.Destroyed || unit.Descriptor.State.IsDead) && released,
                "SummonedUnitBuff removal; DismemberedUnitBuff; SummonPoolsManager death/destroy events");
        }
    }
}
