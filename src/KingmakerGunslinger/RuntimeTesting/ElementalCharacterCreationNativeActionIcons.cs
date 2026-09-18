using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.Selection;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private bool NativeRacialActionCase => _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression &&
            (string)_request.Parameters[NativeRacialActionIconRules.RequestCaseParameter] == NativeRacialActionIconRules.RequestCaseValue &&
            _request.ExitAfterCompletion && _context.FeatureModules.Active.ElementalRaces;

        private bool PauseForNativeRacialActionGroup()
        {
            if (!NativeRacialActionCase || _raceIndex != 0 ||
                !_nativeIconStages.Add(_raceIndex + "|native-racial-action-group")) return false;
            if (_nativeIconScreens == null) _nativeIconScreens = new NativeIconScreenEvidence(_request);
            _nativeIconCapture = CaptureNativeRacialActionGroup().GetEnumerator();
            return true;
        }

        // Displays the exact catalog action consumers of the requested race on
        // the registered dormant mercenary through the native action-bar group
        // renderer. No ability is invoked, no resource is spent and no save
        // write occurs; every request-owned fact, widget, selection and cache
        // is removed again under exact before/after comparison.
        private IEnumerable<int> CaptureNativeRacialActionGroup()
        {
            var game = Game.Instance;
            var ui = game.UI;
            var owner = _unit;
            var group = GroupController.Instance;
            var bar = ActionBarManager.Instance;
            // The popup groups are per-slot SubGroup objects under the bar.
            // They are chosen after the bar binds, when the live hierarchy is
            // observable; before that most anchors are inactive.
            var barSelectedField = typeof(ActionBarManager).GetField("m_Selected", Members);
            var spellsGroups = Resources.FindObjectsOfTypeAll<ActionBarSpellsGroup>();
            var liveGroups = spellsGroups.Where(value => value != null).ToArray();
            var subActiveField = typeof(ActionBarSpellsGroup).GetField("m_IsActive", Members);
            ActionBarSpellsGroup subGroup = null;
            var originalBarSelected = bar == null ? null : (UnitEntityData)barSelectedField.GetValue(bar);
            var selection = ui == null ? null : ui.SelectionManagerPC;
            var originalSelection = selection == null ? new List<UnitEntityData>() : selection.SelectedUnits.ToList();
            bool originalIsSingle = selection != null && selection.IsSingleSelected;
            var symbols = NativeRacialActionIconRules.SymbolsForRace((string)_request.Parameters["race"]);
            var setup = new JObject { ["stage"] = "native-prerequisites", ["ownerId"] = owner?.UniqueId,
                ["race"] = (string)_request.Parameters["race"], ["symbolCount"] = symbols.Length,
                ["canCommit"] = _canCommit, ["committed"] = _committed,
                ["ownerCustomCompanion"] = owner != null && owner.Descriptor.IsCustomCompanion(),
                ["ownerInCrossSceneState"] = owner != null && ReferenceEquals(owner.HoldingState, game.Player.CrossSceneState),
                ["serviceWindowPresent"] = ui != null && ui.ServiceWindow != null,
                ["windowTabsShown"] = ui != null && ui.ServiceWindow != null && ui.ServiceWindow.WindowTabs.IsShow,
                ["selectionManagerPresent"] = selection != null,
                ["groupControllerPresent"] = group != null,
                ["barManagerPresent"] = bar != null, ["barGroupPresent"] = bar != null && bar.Group != null,
                ["barOwnerNull"] = originalBarSelected == null,
                ["spellsGroupCount"] = spellsGroups.Length,
                ["spellsGroupNames"] = new JArray(spellsGroups.Select(value => value == null ? "<null>" : value.name)),
                ["selectedCount"] = originalSelection.Count,
                ["isSingleSelected"] = originalIsSingle,
                ["controllerGamepad"] = game.IsControllerGamepad,
                ["loadedEvidencePresent"] = _loaded != null, ["guardOwned"] = ReferenceEquals(_saveGuardOwner, this),
                ["sameOwner"] = ReferenceEquals(owner, _unit),
                ["inParty"] = owner != null && game.Player.Party.Contains(owner) };
            _character["nativeRacialActionSetup"] = setup;
            Write();
            if (!_canCommit || !_committed || owner == null || game.Player.Party.Contains(owner) ||
                ui.ServiceWindow == null ||
                ui.ServiceWindow.WindowTabs.IsShow || ui.SelectionManagerPC == null || group == null ||
                bar == null || bar.Group == null || game.IsControllerGamepad ||
                spellsGroups.Count(value => value != null) == 0 || symbols.Length == 0)
                throw new InvalidOperationException("Native action review requires the exact committed request-owned unit and idle desktop action bar: " + setup);
            if (_loaded == null || _loaded.SaveWritingApiObserved || !ReferenceEquals(_saveGuardOwner, this) ||
                !ReferenceEquals(owner, _unit))
                throw new InvalidOperationException("Native action review lost its exact request-owned unit or save guard: " + setup);

            // Painted consumers resolve through their exact owned bindings;
            // native-semantic-reuse consumers have no binding and resolve
            // directly through the registered manifest, their icon being the
            // retained native donor sprite.
            var manifest = BlueprintManifest.Load(_context.ModEntry.Path);
            var resolved = symbols.Select(symbol => {
                var binding = OwnedIconAssignments.Bindings.SingleOrDefault(value => value.Symbol == symbol);
                Type type = NativeRacialActionIconRules.IsActivatableSymbol(symbol) ?
                    typeof(BlueprintActivatableAbility) : typeof(BlueprintAbility);
                BlueprintScriptableObject blueprint = null;
                try
                {
                    string guid = manifest.ResolveActive(symbol, type).Id.Value;
                    BlueprintScriptableObject candidate;
                    if (BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(guid, out candidate) &&
                        candidate != null && candidate.AssetGuid == guid && candidate.GetType() == type)
                        blueprint = candidate;
                }
                catch (Exception) { blueprint = null; }
                return new { Symbol = symbol, Binding = binding, Blueprint = blueprint };
            }).ToArray();
            var expectedActivatable = resolved.Where(value => value.Blueprint is BlueprintActivatableAbility).ToArray();
            var expectedAbilities = resolved.Where(value => value.Blueprint is BlueprintAbility).ToArray();
            setup["stage"] = "resolve-owned-actions";
            setup["paintedBindings"] = new JArray(resolved.Where(value => value.Binding != null).Select(value => value.Symbol));
            setup["nativeReuseConsumers"] = new JArray(resolved.Where(value => value.Binding == null).Select(value => value.Symbol));
            setup["resolvedActivatables"] = new JArray(expectedActivatable.Select(value => value.Blueprint.AssetGuid));
            setup["resolvedAbilities"] = new JArray(expectedAbilities.Select(value => new JObject {
                ["guid"] = value.Blueprint.AssetGuid, ["isVariant"] = ((BlueprintAbility)value.Blueprint).Parent != null }));
            Write();
            if (resolved.Any(value => value.Blueprint == null || value.Blueprint.AssetGuid == null) ||
                expectedActivatable.Length + expectedAbilities.Length != symbols.Length)
                throw new InvalidOperationException("A racial action consumer did not resolve to its registered blueprint: " + setup);
            if (resolved.Where(value => value.Binding != null).Any(value =>
                    value.Binding.BlueprintType != value.Blueprint.GetType() ||
                    value.Binding.Resolve(BlueprintBootstrap.Library, BlueprintManifest.Load(_context.ModEntry.Path)) != value.Blueprint))
                throw new InvalidOperationException("A painted action consumer diverged from its owned binding.");
            foreach (var entry in expectedAbilities)
                if (((BlueprintAbility)entry.Blueprint).Icon == null)
                    throw new InvalidOperationException("Action consumer has no icon to present: " + entry.Symbol);
            foreach (var entry in expectedActivatable)
                if (((BlueprintActivatableAbility)entry.Blueprint).Icon == null)
                    throw new InvalidOperationException("Activatable consumer has no icon to present: " + entry.Symbol);

            // The ordinary native control: the existing Fight Defensively fact on
            // the same unit. Its exact GUID/name is pinned; a different control
            // fails the run rather than weakening the identity contract.
            var controlAbility = owner.Abilities.Enumerable.OfType<Kingmaker.UnitLogic.Abilities.Ability>()
                .SingleOrDefault(value => value.Blueprint.AssetGuid == NativeRacialActionIconRules.ControlGuid);
            var controlActivatable = owner.ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                value.Blueprint.AssetGuid == NativeRacialActionIconRules.ControlGuid);
            bool controlIsActivatable = controlAbility == null && controlActivatable != null;
            if (controlAbility == null && controlActivatable == null)
                throw new InvalidOperationException("The pinned native Fight Defensively control fact is absent from the fixture unit.");
            var controlBlueprint = (BlueprintUnitFact)(controlIsActivatable ?
                (BlueprintUnitFact)controlActivatable.Blueprint : (BlueprintUnitFact)controlAbility.Blueprint);
            setup["stage"] = "native-control-identity";
            setup["controlGuid"] = controlBlueprint.AssetGuid;
            setup["controlName"] = controlBlueprint.name;
            setup["controlIsActivatable"] = controlIsActivatable;
            setup["controlIcon"] = controlBlueprint.Icon == null ? null : controlBlueprint.Icon.name;
            Write();
            if (controlBlueprint.AssetGuid != NativeRacialActionIconRules.ControlGuid ||
                controlBlueprint.name != NativeRacialActionIconRules.ControlName || controlBlueprint.Icon == null)
                throw new InvalidOperationException("The observed native control identity differs from the pinned Fight Defensively fact.");

            var partBefore = owner.Descriptor.Get<UnitPartAbilityModifiers>();
            int entriesBefore = partBefore == null || partBefore.FreeActionList == null ? 0 : partBefore.FreeActionList.Count;
            var cachePlan = NativeRacialActionIconRules.PlanModifierCache(partBefore != null, entriesBefore);



            var originalAbilities = owner.Abilities.Enumerable.ToArray();
            var originalActivatables = owner.ActivatableAbilities.Enumerable.ToArray();
            var originalBuffs = owner.Buffs.Enumerable.ToArray();
            var originalFeatures = owner.Descriptor.Progression.Features.Enumerable.ToArray();
            var originalAreas = game.State.AreaEffects.All.ToArray();
            var originalUnits = game.State.Units.All.ToArray();
            var originalOthers = originalUnits.Where(value => !ReferenceEquals(value, owner)).Select(value => new {
                Unit = value, Damage = value.Damage, Position = value.Position }).ToArray();
            var originalParty = game.Player.Party.ToArray();
            var originalTime = game.Player.GameTime;
            bool originalPause = game.IsPaused;
            var addedAbilities = new List<Kingmaker.UnitLogic.Abilities.Ability>();
            var addedActivatables = new List<ActivatableAbility>();
            var detachedRows = new List<AbilityData>();
            var fixtureWidgets = new List<ActionBarSlot>();
            var exceptions = new List<string>();
            var rowWidgets = new List<ActionBarSpontaneousConvertedSlot>();
            var evidence = new JObject { ["race"] = (string)_request.Parameters["race"], ["ownerId"] = owner.UniqueId,
                ["presentation"] = "native action-bar group rows on the dormant remote mercenary; no activation",
                ["expectedGuids"] = new JArray(resolved.Select(value => value.Blueprint.AssetGuid)),
                ["capturedGuids"] = new JArray(), ["restored"] = false };
            _character["nativeRacialActionGroup"] = evidence;
            setup["stage"] = "native-fixture-ready";
            Func<bool> worldRetained = () => game.IsPaused && game.Player.GameTime == originalTime &&
                game.State.AreaEffects.All.SequenceEqual(originalAreas) && game.State.Units.All.SequenceEqual(originalUnits) &&
                game.Player.Party.SequenceEqual(originalParty) && owner.Descriptor.Progression.Features.Enumerable.SequenceEqual(originalFeatures) &&
                owner.Buffs.Enumerable.SequenceEqual(originalBuffs) &&
                owner.Abilities.Enumerable.SequenceEqual(originalAbilities.Concat(addedAbilities)) &&
                owner.ActivatableAbilities.Enumerable.SequenceEqual(originalActivatables.Concat(addedActivatables)) &&
                originalOthers.All(value => value.Unit.Damage == value.Damage && value.Unit.Position == value.Position);
            Application.LogCallback observe = (message, stack, kind) => {
                if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert) exceptions.Add(kind + ": " + message + "\n" + stack);
            };
            Application.logMessageReceived += observe;
            // Own the pause boundary for the duration of the observation: an
            // unpaused session advances game time during real-time captures,
            // which would make every later exact-world comparison impossible.
            // The original pause state is restored with the selection.
            if (!originalPause) game.IsPaused = true;
            bool restorationIncomplete = false;
            try
            {
                setup["stage"] = "bind-native-anchor";
                // The always-live native portrait strip hosts the rows; the
                // party selection and the action bar's own binding are never
                // displaced by this fixture.
                var anchorSlotParent = group.transform;
                setup["anchorParentActive"] = anchorSlotParent != null &&
                    anchorSlotParent.gameObject.activeInHierarchy;
                Write();
                if (anchorSlotParent == null || !anchorSlotParent.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("No live native portrait strip for the action rows: " + setup);
                subGroup = liveGroups.FirstOrDefault();
                setup["spellsGroupCount"] = liveGroups.Length;
                setup["subGroupActive"] = subGroup != null && (bool)subActiveField.GetValue(subGroup);
                Write();
                if (subGroup == null || (bool)subActiveField.GetValue(subGroup))
                    throw new InvalidOperationException("No closed native spells group is available for its row prefab: " + setup);

                foreach (var entry in resolved)
                {
                    if (entry.Blueprint is BlueprintActivatableAbility)
                    {
                        var fact = owner.ActivatableAbilities.AddFact((BlueprintActivatableAbility)entry.Blueprint, null)
                            as ActivatableAbility;
                        if (fact == null) throw new InvalidOperationException("Native activatable addition returned no owned fact: " + entry.Symbol);
                        addedActivatables.Add(fact);
                    }
                    else
                    {
                        var ability = (BlueprintAbility)entry.Blueprint;
                        if (ability.Parent != null)
                            detachedRows.Add(new AbilityData(ability, owner.Descriptor));
                        else
                        {
                            var fact = owner.Abilities.AddFact(ability, null) as Kingmaker.UnitLogic.Abilities.Ability;
                            if (fact == null) throw new InvalidOperationException("Native ability addition returned no owned fact: " + entry.Symbol);
                            addedAbilities.Add(fact);
                        }
                    }
                    if (!worldRetained()) throw new InvalidOperationException("Native action fact insertion changed the world at " + entry.Symbol);
                }

                // Rows render in the exact catalog-symbol order so the captured
                // sequence equals the expected sequence: parents and variants
                // through the native converted-spell slot binding, the
                // activatable through the native activatable mechanic slot,
                // the pinned control last.
                var groupPrefab = (ActionBarSpontaneousConvertedSlot)typeof(ActionBarSpellsGroup)
                    .GetField("m_ActionBarSpontaneousConvertedSlotPrefab", Members).GetValue(subGroup);
                setup["groupPrefabPresent"] = groupPrefab != null;
                Write();
                if (groupPrefab == null)
                    throw new InvalidOperationException("The native converted-slot prefab is unavailable: " + setup);
                var factByGuid = addedAbilities.ToDictionary(value => value.Blueprint.AssetGuid);
                var detachedByGuid = detachedRows.ToDictionary(value => value.Blueprint.AssetGuid);
                var activatableByGuid = addedActivatables.ToDictionary(value => value.Blueprint.AssetGuid);
                var orderedRows = new List<KeyValuePair<BlueprintUnitFact, ActionBarSlot>>();
                var orderedActivatables = new Dictionary<ActionBarSlot, ActivatableAbility>();
                foreach (var entry in resolved)
                {
                    if (entry.Blueprint is BlueprintActivatableAbility)
                    {
                        var fact = activatableByGuid[entry.Blueprint.AssetGuid];
                        var widget = Kingmaker.UI.WidgetFactory.GetWidget(groupPrefab);
                        widget.transform.SetParent(anchorSlotParent, false);
                        widget.Initialize();
                        var mechanic = new MechanicActionBarSlotActivableAbility();
                        mechanic.ActivatableAbility = fact;
                        typeof(MechanicActionBarSlot).GetField("Unit", Members).SetValue(mechanic, owner);
                        widget.MechanicSlot = mechanic;
                        mechanic.SetSlot(widget);
                        fixtureWidgets.Add(widget);
                        orderedRows.Add(new KeyValuePair<BlueprintUnitFact, ActionBarSlot>(
                            (BlueprintUnitFact)entry.Blueprint, widget));
                        orderedActivatables[widget] = fact;
                    }
                    else
                    {
                        var ability = (BlueprintAbility)entry.Blueprint;
                        var data = factByGuid.ContainsKey(ability.AssetGuid) ?
                            factByGuid[ability.AssetGuid].Data : detachedByGuid[ability.AssetGuid];
                        var slot = Kingmaker.UI.WidgetFactory.GetWidget(groupPrefab);
                        slot.transform.SetParent(anchorSlotParent, false);
                        slot.Initialize();
                        slot.Set(owner, data);
                        rowWidgets.Add(slot);
                        orderedRows.Add(new KeyValuePair<BlueprintUnitFact, ActionBarSlot>(ability, slot));
                    }
                }
                var source = addedAbilities.Select(value => value.Data).FirstOrDefault();
                ActionBarSlot controlWidget = null;
                if (controlIsActivatable)
                {
                    controlWidget = Kingmaker.UI.WidgetFactory.GetWidget(groupPrefab);
                    controlWidget.transform.SetParent(anchorSlotParent, false);
                    controlWidget.Initialize();
                    var controlMechanic = new MechanicActionBarSlotActivableAbility();
                    controlMechanic.ActivatableAbility = controlActivatable;
                    typeof(MechanicActionBarSlot).GetField("Unit", Members).SetValue(controlMechanic, owner);
                    controlWidget.MechanicSlot = controlMechanic;
                    controlMechanic.SetSlot(controlWidget);
                    fixtureWidgets.Add(controlWidget);
                    orderedActivatables[controlWidget] = controlActivatable;
                }
                for (int frame = 0; frame < 10; frame++) yield return 0;
                setup["stage"] = "rows-bound";
                setup["boundRowCount"] = orderedRows.Count;
                Write();
                if (orderedRows.Count != symbols.Length || orderedRows.Any(value =>
                        value.Value == null || !value.Value.gameObject.activeInHierarchy))
                    throw new InvalidOperationException("The native action rows did not bind: " + setup);
                if (source == null) throw new InvalidOperationException("The racial action set has no parent fact for its anchor context.");

                var expectedByGuid = expectedAbilities.ToDictionary(value => value.Blueprint.AssetGuid, value => (BlueprintAbility)value.Blueprint);
                foreach (var ordered in orderedRows)
                {
                    var blueprint = ordered.Key;
                    var slot = ordered.Value;
                    var mechanic = slot.MechanicSlot as MechanicActionBarSlotSpontaneusConvertedSpell;
                    if (mechanic != null)
                    {
                        var data = mechanic.Spell;
                        if (data == null || !expectedByGuid.ContainsKey(data.Blueprint.AssetGuid))
                            throw new InvalidOperationException("The native group rendered a foreign or unbound ability row.");
                        var ability = expectedByGuid[data.Blueprint.AssetGuid];
                        if (evidence["rowProbe"] == null)
                        {
                            evidence["rowProbe"] = new JObject {
                                ["mechanicType"] = "Kingmaker.UI.UnitSettings.MechanicActionBarSlotSpontaneusConvertedSpell",
                                ["mechanicSpellGuid"] = data.Blueprint.AssetGuid,
                                ["rowActive"] = slot.gameObject.activeInHierarchy,
                                ["renderedSprite"] = slot.Icon == null ? null : slot.Icon.sprite?.name,
                                ["expectedSprite"] = ability.Icon.name,
                                ["spriteExact"] = slot.Icon != null && ReferenceEquals(slot.Icon.sprite, ability.Icon) };
                            Write();
                        }
                        Func<bool> context = () => worldRetained() && ReferenceEquals(slot.MechanicSlot, mechanic) &&
                            slot.gameObject.activeInHierarchy && slot.Icon != null;
                        Func<JObject> describe = () => new JObject { ["surface"] = "native-racial-action-row",
                            ["race"] = (string)_request.Parameters["race"], ["targetRow"] = new JObject {
                                ["name"] = ability.name, ["guid"] = ability.AssetGuid, ["ownerId"] = owner.UniqueId,
                                ["isVariant"] = ability.Parent != null,
                                ["expectedSprite"] = ability.Icon.name, ["renderedSprite"] = slot.Icon.sprite?.name,
                                ["spriteExact"] = slot.Icon.isActiveAndEnabled && ReferenceEquals(slot.Icon.sprite, ability.Icon),
                                ["rowActive"] = slot.gameObject.activeInHierarchy,
                                ["unavailableOverlayEnabled"] = slot.Disable != null && slot.Disable.enabled,
                                ["worldRetained"] = worldRetained() } };
                        foreach (int frame in CaptureNativeActionRow("native-racial-action-row:" + ability.AssetGuid,
                            slot, describe, context, evidence)) yield return frame;
                        var target = describe()["targetRow"];
                        if (!(bool)target["spriteExact"] || !(bool)target["rowActive"] || !context())
                            throw new InvalidOperationException("Actual native action row differs: " + ability.AssetGuid + "; " + target);
                        ((JArray)evidence["capturedGuids"]).Add(ability.AssetGuid);
                    }
                    else
                    {
                        var fact = orderedActivatables[slot];
                        bool isControl = ReferenceEquals(fact, controlActivatable);
                        Func<bool> context = () => worldRetained() && slot.MechanicSlot != null &&
                            ReferenceEquals(((MechanicActionBarSlotActivableAbility)slot.MechanicSlot).ActivatableAbility, fact) &&
                            slot.gameObject.activeInHierarchy && slot.Icon != null;
                        Func<JObject> describe = () => new JObject { ["surface"] = isControl ? "native-action-control-toggle" : "native-racial-action-activatable",
                            ["race"] = (string)_request.Parameters["race"], ["targetRow"] = new JObject {
                                ["name"] = blueprint.name, ["guid"] = blueprint.AssetGuid, ["ownerId"] = owner.UniqueId,
                                ["isControl"] = isControl, ["turnedOn"] = fact.IsOn,
                                ["expectedSprite"] = blueprint.Icon.name, ["renderedSprite"] = slot.Icon.sprite?.name,
                                ["spriteExact"] = slot.Icon.isActiveAndEnabled && ReferenceEquals(slot.Icon.sprite, blueprint.Icon),
                                ["rowActive"] = slot.gameObject.activeInHierarchy,
                                ["worldRetained"] = worldRetained() } };
                        foreach (int frame in CaptureNativeActionRow((isControl ? "native-action-control:" : "native-racial-activatable:") +
                            blueprint.AssetGuid, slot, describe, context, evidence)) yield return frame;
                        var target = describe()["targetRow"];
                        if (!(bool)target["spriteExact"] || !(bool)target["rowActive"] || fact.IsOn || !context())
                            throw new InvalidOperationException("Actual native activatable row differs: " + blueprint.AssetGuid + "; " + target);
                        if (!isControl) ((JArray)evidence["capturedGuids"]).Add(blueprint.AssetGuid);
                    }
                }
            }
            finally
            {
                foreach (var slot in rowWidgets.AsEnumerable().Reverse())
                    try { Kingmaker.UI.WidgetFactory.DisposeWidget(slot); }
                    catch (Exception error) { exceptions.Add("Native row widget cleanup: " + error); }
                // No anchor-slot or popup state was borrowed by the row-binding
                // presentation; nothing of the bar's own bookkeeping needs undoing.
                evidence["anchorSlotRestored"] = true;
                foreach (var widget in fixtureWidgets)
                    try { Kingmaker.UI.WidgetFactory.DisposeWidget(widget); }
                    catch (Exception error) { exceptions.Add("Fixture widget cleanup: " + error); }
                foreach (var fact in addedActivatables.AsEnumerable().Reverse())
                    try { if (owner.ActivatableAbilities.Enumerable.Contains(fact)) owner.ActivatableAbilities.RemoveFact(fact); }
                    catch (Exception error) { exceptions.Add("Owned activatable cleanup: " + error); }
                foreach (var fact in addedAbilities.AsEnumerable().Reverse())
                    try { if (owner.Abilities.Enumerable.Contains(fact)) owner.Abilities.RemoveFact(fact); }
                    catch (Exception error) { exceptions.Add("Owned ability cleanup: " + error); }
                detachedRows.Clear();
                // The request owns only a native modifier part that did not exist
                // before rendering and is still empty now. Anything else stays.
                try
                {
                    var partAfter = owner.Descriptor.Get<UnitPartAbilityModifiers>();
                    int entriesAfter = partAfter == null || partAfter.FreeActionList == null ? 0 : partAfter.FreeActionList.Count;
                    evidence["modifierCacheEntriesAfter"] = entriesAfter;
                    bool cleanupExact;
                    if (cachePlan == NativeRacialActionIconRules.ModifierCachePlan.OwnAndRemoveIfStillEmpty)
                    {
                        bool removed = false;
                        if (partAfter != null && entriesAfter == 0)
                        {
                            owner.Descriptor.Remove<UnitPartAbilityModifiers>();
                            removed = owner.Descriptor.Get<UnitPartAbilityModifiers>() == null;
                        }
                        // A part that never materialized during rendering needs
                        // no removal; that is equally exact.
                        cleanupExact = removed || partAfter == null;
                    }
                    else cleanupExact = partAfter == partBefore;
                    evidence["modifierCacheCleanupExact"] = cleanupExact;
                }
                catch (Exception error) { exceptions.Add("Owned modifier cache cleanup: " + error); }
                // The fixture never displaces the party selection; verify it
                // stayed untouched rather than restoring anything.
                evidence["selectionUntouched"] = selection.SelectedUnits.SequenceEqual(originalSelection) &&
                    selection.IsSingleSelected == originalIsSingle;
                Application.logMessageReceived -= observe;
                var slotsAfterHide = new List<ActionBarSpontaneousConvertedSlot>();
                // Per-dimension restoration evidence so a failure names the
                // exact diverging state instead of one aggregate flag.
                evidence["worldDimensions"] = new JObject {
                    ["gameTimeExact"] = game.Player.GameTime == originalTime,
                    ["pauseHeld"] = game.IsPaused,
                    ["originalPause"] = originalPause,
                    ["areaEffectsExact"] = game.State.AreaEffects.All.SequenceEqual(originalAreas),
                    ["unitsExact"] = game.State.Units.All.SequenceEqual(originalUnits),
                    ["partyExact"] = game.Player.Party.SequenceEqual(originalParty),
                    ["featuresExact"] = owner.Descriptor.Progression.Features.Enumerable.SequenceEqual(originalFeatures),
                    ["buffsExact"] = owner.Buffs.Enumerable.SequenceEqual(originalBuffs),
                    ["othersDamageAndPositionExact"] = originalOthers.All(value =>
                        value.Unit.Damage == value.Damage && value.Unit.Position == value.Position) };
                // The fixture never opens the popup itself; the chosen
                // group's own closed state is re-derived, not restored.
                evidence["groupHiddenAfterCleanup"] = subGroup == null ||
                    (!(bool)subActiveField.GetValue(subGroup) && slotsAfterHide.Count == 0);
                evidence["barOwnerAfterCleanup"] = ((UnitEntityData)barSelectedField.GetValue(bar))?.UniqueId;

                evidence["originalAbilitiesRestored"] = owner.Abilities.Enumerable.SequenceEqual(originalAbilities);
                evidence["originalActivatablesRestored"] = owner.ActivatableAbilities.Enumerable.SequenceEqual(originalActivatables);
                // Post-cleanup exactness: every recorded world dimension, plus
                // the owner's collections already compared against their exact
                // originals above (the in-flight lambda expected the fixture
                // facts to still be present).
                var dimensions = (JObject)evidence["worldDimensions"];
                evidence["worldAndOriginalFactsRetained"] =
                    new[] { "gameTimeExact", "pauseHeld", "areaEffectsExact", "unitsExact",
                        "partyExact", "featuresExact", "buffsExact", "othersDamageAndPositionExact" }
                    .All(key => (bool)dimensions[key]) &&
                    (bool)evidence["originalAbilitiesRestored"] && (bool)evidence["originalActivatablesRestored"];
                evidence["exceptions"] = new JArray(exceptions);
                // The selection dimension is re-derived after the deferred
                // settling below, where one retry is permitted; every other
                // dimension is final here.
                evidence["restored"] = (bool)evidence["originalAbilitiesRestored"] && (bool)evidence["originalActivatablesRestored"] &&
                    (bool)evidence["worldAndOriginalFactsRetained"] && (bool)evidence["selectionUntouched"] &&
                    (bool)evidence["groupHiddenAfterCleanup"] && (bool)evidence["anchorSlotRestored"] &&
                    (bool)evidence["modifierCacheCleanupExact"] && exceptions.Count == 0;
                Write();
                restorationIncomplete = !(bool)evidence["restored"];
                // All exact comparisons above were made while the owned pause
                // held game time frozen; release it only after they are done.
                try { if (!originalPause) game.IsPaused = false; }
                catch (Exception error) { exceptions.Add("Owned pause restoration: " + error); }
            }
            for (int frame = 0; frame < 20; frame++) yield return 0;
                evidence["rowWidgetsDisposed"] = rowWidgets.All(value =>
                    value == null || !value.gameObject.activeInHierarchy);
            evidence["selectionUntouched"] = selection.SelectedUnits.SequenceEqual(originalSelection) &&
                selection.IsSingleSelected == originalIsSingle;
            evidence["restored"] = (bool?)evidence["restored"] == true && (bool)evidence["selectionUntouched"] &&
                (bool)evidence["rowWidgetsDisposed"];
            Write();
            // Thrown only on the success path so a fixture-body failure keeps
            // its own reason visible; the finally block recorded both cases.
            if (restorationIncomplete || !(bool)evidence["restored"])
                throw new InvalidOperationException("Native action group fixture failed exact restoration: " + evidence);
            // The resumed session advances game time by design; only the
            // non-time world dimensions must remain exact after the deferred
            // UI refresh.
            if (!owner.Abilities.Enumerable.SequenceEqual(originalAbilities) ||
                !owner.ActivatableAbilities.Enumerable.SequenceEqual(originalActivatables) ||
                !game.State.AreaEffects.All.SequenceEqual(originalAreas) ||
                !game.State.Units.All.SequenceEqual(originalUnits) ||
                !game.Player.Party.SequenceEqual(originalParty) ||
                !selection.SelectedUnits.SequenceEqual(originalSelection))
                throw new InvalidOperationException("Native action cleanup changed during deferred UI refresh.");
        }

        // Native row capture for action-bar slots. The bar's popup rows do not
        // necessarily live under a scroll viewport like the sheet lists; when
        // the scroll-aware capture cannot apply, record the whole native group
        // screen instead of mutating the layout.
        private IEnumerable<int> CaptureNativeActionRow(string stage, ActionBarSlot slot,
            Func<JObject> describe, Func<bool> context, JObject evidence)
        {
            var enumerator = _nativeIconScreens.CaptureRow(stage, (RectTransform)slot.transform, describe, context,
                observeOnly: true).GetEnumerator();
            while (true)
            {
                bool moved;
                try { moved = enumerator.MoveNext(); }
                catch (InvalidOperationException error)
                {
                    if (!error.Message.Contains("no supported vertical scroll viewport")) throw;
                    // The rejection happens before any scroll or capture state
                    // is touched, so the abandoned enumerator needs no cleanup.
                    break;
                }
                if (!moved)
                {
                    ((JObject)evidence)["rowCaptureApi"] = "CaptureRow-observeOnly";
                    yield break;
                }
                yield return enumerator.Current;
            }
            foreach (int frame in _nativeIconScreens.Capture(stage, describe(), context)) yield return frame;
            ((JObject)evidence)["rowCaptureApi"] = "CaptureRow-observeOnly-or-Capture";
        }

        private void AppendNativeRacialActionAssertion()
        {
            if (!NativeRacialActionCase) return;
            var entries = _characters.OfType<JObject>().Select(value => value["nativeRacialActionGroup"] as JObject)
                .Where(value => value != null).ToArray();
            bool passed = entries.Length == 1 && (bool?)entries[0]["restored"] == true &&
                ((JArray)entries[0]["expectedGuids"]).Count == NativeRacialActionIconRules.SymbolsForRace((string)_request.Parameters["race"]).Length &&
                JToken.DeepEquals(entries[0]["expectedGuids"], entries[0]["capturedGuids"]);
            Result.Assertions.Add(new RuntimeTestAssertion { Name = "actual-native-racial-action-group",
                Expected = "exact native action rows for every catalog consumer of the race, pinned control and full original fact/selection/world restoration",
                Observed = new JArray(entries).ToString(Newtonsoft.Json.Formatting.None),
                Status = passed ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, Evidence = EvidenceFileName });
        }
    }
}
