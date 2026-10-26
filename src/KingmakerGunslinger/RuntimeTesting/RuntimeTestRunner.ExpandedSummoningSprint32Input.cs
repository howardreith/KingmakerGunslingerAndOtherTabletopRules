using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Three fixed fresh-process selections, using the proven menu/Default
        // lease and source debit observer. No arbitrary actor, route or input.
        private IEnumerable<int> ReviewSprint32NativeInput(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || _request.Parameters?.Count != 1 ||
                RuntimeTestRequestParser.Sprint32ReviewPhase(_request) != phase ||
                !ExpandedSummoningNativeProfilePolicy.Sprint32NativeInputPhase(phase))
                throw new InvalidOperationException("Exact Sprint32 native input phase required.");
            bool druid = phase == ExpandedSummoningNativeProfilePolicy.Sprint32DruidInputPhase;
            bool known = phase == ExpandedSummoningNativeProfilePolicy.Sprint32SorcererInputPhase;
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (!Cleanup2NoNarrativeOverlay() || group == null || bar?.Group == null || ui?.SelectionManagerPC == null ||
                game.IsControllerGamepad || ui.TooltipsController?.ContextTooltipData != null || !fixture.Caster.Commands.Empty)
                throw new InvalidOperationException("Idle native desktop fixture required.");
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var selectedGroup = group.GetCurrentCharacter(); var groups = bar.Group.GroupElements.ToArray();
            bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            if (toggles.Any(value => value)) throw new InvalidOperationException("Closed native groups required.");
            var originalInputs = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var originalBars = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var originalCaster = fixture.Caster;
            object levelController = null; bool added = false, selectedFixture = false, complete = false;
            Cleanup2NativeDefaultScope mode = null; NativeIconScreenEvidence screens = null;
            var watch = Stopwatch.StartNew();
            try
            {
                // One actual native class per process. Fixture preparation is
                // disclosed and finishes before input ownership/cost capture.
                var casterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    druid ? "610d836f3a3a9ed42a4349b62f002e96" : known ? "b3a505fb61437dc4097f43c3f8f9a4cf" :
                    "ba34257984f4c41408ce1dc2004e342e", "Sprint32 native input class");
                if (druid) fixture.Caster.Stats.Wisdom.BaseValue = 30;
                else if (known) fixture.Caster.Stats.Charisma.BaseValue = 30;
                else fixture.Caster.Stats.Intelligence.BaseValue = 30;
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor, casterClass, 20, ref levelController);
                var book = fixture.Caster.Descriptor.GetSpellbook(casterClass);
                while (book.CasterLevel < 20) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                if (book.Blueprint.Spontaneous != known) throw new InvalidOperationException("Native class book kind differs.");
                string key = druid ? "satyr" : "lillend-azata";
                var family = druid ? SummonFamily.NaturesAlly : SummonFamily.Monster;
                var routes = ExpandedSummoningCatalog.GenerateVariants(family).Where(value => value.Creature.Key == key)
                    .GroupBy(value => value.Multiplicity).Select(values => values.OrderBy(value => value.ParentTier).First())
                    .OrderBy(value => value.ParentTier).ToArray();
                if (!druid && !known) routes = routes.Where(value => value.Multiplicity == SummonMultiplicity.One).ToArray();
                var cases = new List<Tuple<SummonVariantSpec, BlueprintAbility, AbilityData, bool>>();
                foreach (var spec in routes)
                {
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        ExpandedSummoningInventoryObserver.CanonicalParentGuids[(druid ? 9 : 0) + spec.ParentTier - 1], "Sprint32 public parent");
                    var child = ExpandedSummoningRoot(fixture.Blueprints, spec);
                    if (book.Blueprint.SpellList.GetLevel(parent) != spec.ParentTier || !SummonVisibilityCatalog.IsPublished(spec) ||
                        parent.Variants.Count(value => ReferenceEquals(value, child)) != 1)
                        throw new InvalidOperationException("Exact legal public route missing.");
                    if (known)
                    {
                        book.AddKnown(spec.ParentTier, parent, true);
                        cases.Add(Tuple.Create(spec, parent, book.GetKnownSpells(spec.ParentTier)
                            .Single(value => ReferenceEquals(value.Blueprint, parent)), false));
                    }
                    else if (!druid || spec.Multiplicity == SummonMultiplicity.One)
                        cases.Add(Tuple.Create(spec, parent, PrepareExpandedSummoningPlayerPathSpell(book, parent, parent, spec.ParentTier), false));
                    if (druid)
                    {
                        var feature = casterClass.Progression.LevelEntries.SelectMany(value => value.Features).OfType<BlueprintFeature>()
                            .Distinct().Single(value => DruidSummonConversionVariantPatch.ExactNativeFeature(value, casterClass));
                        if (!fixture.Caster.Descriptor.HasFact(feature))
                            throw new InvalidOperationException("Native levelled Druid has no conversion feature.");
                        var ordinary = book.Blueprint.SpellList.GetSpells(spec.ParentTier).Where(value => value != null &&
                            !ReferenceEquals(value, parent) && (value.Variants == null || value.Variants.Length == 0))
                            .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
                        cases.Add(Tuple.Create(spec, parent, PrepareExpandedSummoningPlayerPathSpell(book, ordinary, ordinary, spec.ParentTier), true));
                    }
                }
                book.Rest();
                if (!player.Party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                mode = Cleanup2EnterNativeDefault(fixture, phase); screens = new NativeIconScreenEvidence(_request);
                CreateSprint32TurnHostile(fixture);
                // Native exploration casts intentionally assign no cooldown.
                // Establish owned combat before real input, without changing
                // native cost rules or any intake actor's faction or memory.
                PlaceExpandedSummoningUnit(fixture.Hostile, fixture.Caster.Position + Vector3.forward * 8f);
                foreach (var unit in new[] { fixture.Caster, fixture.Hostile })
                {
                    unit.Wake(0f);
                    if (!game.State.AwakeUnits.Contains(unit)) game.State.AwakeUnits.Add(unit);
                }
                fixture.Caster.Memory.Add(fixture.Hostile); fixture.Hostile.Memory.Add(fixture.Caster);
                fixture.Caster.SightCache.ClearIfNeeded(); fixture.Hostile.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                fixture.Caster.JoinCombat(); fixture.Hostile.JoinCombat(); player.UpdateIsInCombat();
                new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
                new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
                bool combatReady = fixture.Caster.IsInCombat && fixture.Hostile.IsInCombat &&
                    fixture.Caster.HasLOS(fixture.Hostile) && fixture.Caster.Memory.ContainsVisible(fixture.Hostile) &&
                    fixture.Caster.IsEnemy(fixture.Hostile) && fixture.Hostile.IsEnemy(fixture.Caster);
                Sprint25Check(phase + "-combat-ready", combatReady, new JObject {
                    ["caster"] = fixture.Caster.UniqueId, ["target"] = fixture.Hostile.UniqueId,
                    ["casterInCombat"] = fixture.Caster.IsInCombat, ["targetInCombat"] = fixture.Hostile.IsInCombat,
                    ["lineOfSight"] = fixture.Caster.HasLOS(fixture.Hostile),
                    ["enemyVisible"] = fixture.Caster.Memory.ContainsVisible(fixture.Hostile) },
                    "The exact disposable caster enters native combat with an isolated brain-disabled hostile before input/cost observation.");
                if (!combatReady) throw new InvalidOperationException("Native input combat admission failed.");
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster); selectedFixture = true;
                for (int frame = 0; frame < 12; frame++) yield return 0;
                if (!mode.Current) throw new InvalidOperationException("Native input selection lost Default.");
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement)
                    .GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                foreach (var item in cases)
                {
                    Cleanup2MenuAdapterBoundary(mode, watch);
                    var spec = item.Item1; var parent = item.Item2; var sourceData = item.Item3; bool conversion = item.Item4;
                    string label = phase + "-" + (conversion ? "conversion-" : known ? "known-" : "prepared-") + spec.Multiplicity;
                    if (!spellGroup.ToggleState) spellGroup.OnClick();
                    spellGroup.Fill(fixture.Caster);
                    var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                    levels.OnClick(spec.ParentTier);
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    var source = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value =>
                        value.gameObject.activeInHierarchy && ReferenceEquals(value.MechanicSlot?.GetContentData(), sourceData));
                    bool correctMechanism = known ? source.MechanicSlot is MechanicActionBarSlotSpontaneusSpell :
                        source.MechanicSlot is MechanicActionBarSlotMemorizedSpell;
                    if (!correctMechanism) throw new InvalidOperationException("Wrong native source mechanism.");
                    AbilityData selectedAbility = null;
                    try
                    {
                        foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, label)) yield return frame;
                        source.OnToggleGroupClick();
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
                        if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Native popup absent.");
                        var child = ExpandedSummoningRoot(fixture.Blueprints, spec);
                        var rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(value => value.gameObject.activeInHierarchy &&
                            value.MechanicSlot?.GetContentData() is AbilityData).ToArray();
                        var row = rows.Single(value => ReferenceEquals(((AbilityData)value.MechanicSlot.GetContentData()).Blueprint, child));
                        selectedAbility = (AbilityData)row.MechanicSlot.GetContentData();
                        var data = selectedAbility;
                        var debit = new Cleanup2SourceDebit(fixture.Caster);
                        Func<bool> chain = () => conversion ? Cleanup2DruidSourceChain(data, sourceData, parent) :
                            ReferenceEquals(data.ConvertedFrom, sourceData) && ReferenceEquals(data.Spellbook, book) &&
                            ReferenceEquals(data.Caster, fixture.Caster.Descriptor);
                        Func<bool> owns = () => mode.Current && Cleanup2NoNarrativeOverlay() && debit.Unspent &&
                            game.SelectedAbilityHandler.Ability == null && ReferenceEquals(source.MechanicSlot.GetContentData(), sourceData) &&
                            popup.gameObject.activeInHierarchy && ReferenceEquals(row.MechanicSlot.GetContentData(), data) && chain();
                        var scroll = conversion ? Cleanup2DruidPopupSurface(source, popup, sourceData, data, owns) :
                            Cleanup2AcquirePopupSurface(source, popup, parent, rows, label);
                        foreach (int frame in Cleanup2RevealPopupRow(scroll, row, owns)) yield return frame;
                        var presentation = new JObject { ["source"] = sourceData.Blueprint.AssetGuid, ["child"] = child.AssetGuid,
                            ["book"] = book.Blueprint.AssetGuid, ["class"] = casterClass.AssetGuid, ["conversion"] = conversion,
                            ["sourceChainExact"] = chain(), ["fullyVisible"] = Cleanup2FullyVisible(scroll, row),
                            ["nativeTitle"] = data.Name, ["iconExact"] = row.Icon != null && ReferenceEquals(row.Icon.sprite, child.Icon) };
                        bool ready = owns() && (bool)presentation["fullyVisible"] && (bool)presentation["iconExact"] &&
                            data.Name == SummonChoicePresentationPolicy.For(key, spec.Multiplicity).Label;
                        Sprint25Check(label + "-row", ready, presentation, "Exact visible native source/quantity row retains its real book and source chain before spending.");
                        foreach (int frame in screens.Capture(label, presentation, owns)) yield return frame;
                        if (!ready) throw new InvalidOperationException("Native source row contract failed.");
                        foreach (int frame in Sprint32NativeInputCast(fixture, spec, data, sourceData, row, debit, mode, watch, chain,
                            () => Cleanup2MenuAdapterHide(source, sourceData), label)) yield return frame;
                    }
                    finally
                    {
                        if (selectedAbility != null && ReferenceEquals(game.SelectedAbilityHandler?.Ability, selectedAbility))
                            game.SelectedAbilityHandler.DropAbility();
                        Cleanup2MenuAdapterHide(source, sourceData);
                    }
                }
                complete = true;
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                try
                {
                    JObject casterOwnership;
                    bool ownsCaster = Cleanup2SourcesOwnsCaster(fixture, originalCaster, out casterOwnership);
                    bool untouched = !selectedFixture && ReferenceEquals(group.GetCurrentCharacter(), selectedGroup) &&
                        ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected);
                    bool ownUi = ReferenceEquals(GroupController.Instance, group) && ReferenceEquals(ActionBarManager.Instance, bar) &&
                        bar.Group.GroupElements.SequenceEqual(groups) && ui.TooltipsController?.ContextTooltipData == null &&
                        game.SelectedAbilityHandler?.Ability == null && (untouched ||
                        (ReferenceEquals(group.GetCurrentCharacter(), fixture.Caster) && ui.SelectionManagerPC.SelectedUnits.Count == 1 &&
                        ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(), fixture.Caster)));
                    if (!ownsCaster || !ownUi) errors.Add("Fixture or native UI ownership changed; foreign state not overwritten.");
                    else
                    {
                        if (!untouched) for (int i = 0; i < groups.Length; i++) { int index = i; restore("toggle", () => groups[index].Toggle(toggles[index], true)); }
                        restore("party", () => { if (added) { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); } });
                        if (!untouched)
                        {
                            restore("group", () => group.SelectUnit(selectedGroup));
                            restore("selection", () => ui.SelectionManagerPC.MultiSelect(selected.Where(value => value?.View != null).Select(value => value.View).ToArray(), false));
                        }
                        if (levelController != null) restore("level-controller", () => levelController.GetType().GetMethod("Cancel").Invoke(levelController, null));
                    }
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(group.GetCurrentCharacter(), selectedGroup) && originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored());
                    Sprint25Check(phase + "-ui-restored", exact, new JObject { ["exact"] = exact, ["casterOwnership"] = casterOwnership,
                        ["errors"] = new JArray(errors), ["complete"] = complete },
                        "Original party, selection, resources and pins restore exactly. The levelled disposable caster belongs to outer fixture teardown.");
                    if (!exact) errors.Add("Original input restoration not verified.");
                }
                finally
                {
                    restore("owned-combat", () => {
                        foreach (var unit in new[] { fixture.Caster, fixture.Hostile }.Where(value => value != null && !value.Destroyed))
                            if (unit.IsInCombat) unit.LeaveCombat();
                        player.UpdateIsInCombat();
                    });
                    restore("screens", () => screens?.Dispose());
                    restore("native-default", () => mode?.Dispose());
                }
                Sprint25Check(phase + "-finalizers", errors.Count == 0, new JObject {
                    ["errors"] = new JArray(errors), ["complete"] = complete,
                    ["originalFailurePreserved"] = !complete },
                    "UI and native Default finalizer failures remain separate from the original case failure.");
                if (errors.Count != 0 && complete) throw new InvalidOperationException(string.Join("|", errors.ToArray()));
            }
        }

        private IEnumerable<int> Sprint32NativeInputCast(ExpandedSummoningCorrectionFixture fixture, SummonVariantSpec spec,
            AbilityData child, AbilityData source, ActionBarSlot row, Cleanup2SourceDebit debit, Cleanup2NativeDefaultScope mode,
            Stopwatch phase, Func<bool> chain, Action closePopup, string label)
        {
            if (_expandedSummoningPlayerPathCaptureActive || _cleanup2DruidCastExpected != null || !chain() ||
                !ReferenceEquals(row.MechanicSlot.GetContentData(), child) || child.TargetAnchor == AbilityTargetAnchor.Owner || !fixture.Caster.IsInCombat)
                throw new InvalidOperationException("Exclusive exact native Sprint32 cast required.");
            var hooks = HarmonyInstance.Create("kmg.sprint32.input." + _request.RunId); UnitUseAbility command = null;
            var costs = new JArray(); var before = SnapshotReferences(fixture.AllUnits);
            using (var economy = new FirearmReviewCosts(fixture.Caster, costs))
            try
            {
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null, new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("Cleanup2DruidCastRulePostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleDurationCapture.Clear(); ExpandedSummoningPlayerPathEvents.Clear(); Cleanup2DruidCastRules.Clear();
                _expandedSummoningPlayerPathRuleCastCount = 0; _expandedSummoningPlayerPathSpawnActionCount = 0;
                _cleanup2DruidCastExpected = child; _expandedSummoningPlayerPathCaptureActive = true;
                row.OnClick();
                bool selected = ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, child) && debit.Unspent && fixture.Caster.Commands.Empty;
                if (selected) closePopup();
                var point = fixture.Caster.Position + new Vector3(2, 0, 2);
                bool targeted = selected && Game.Instance.SelectedAbilityHandler.OnClick(null, point, 0);
                command = fixture.Caster.Commands.GetCommand<UnitUseAbility>();
                bool issued = targeted && command != null && ReferenceEquals(command.Spell, child);
                bool withinRange = issued && command.IsUnitEnoughClose;
                var watch = Stopwatch.StartNew();
                while (issued && watch.Elapsed.TotalSeconds < 25 && phase.Elapsed.TotalSeconds < 240 &&
                    (_expandedSummoningPlayerPathRuleCastCount == 0 || !fixture.Caster.Commands.Empty))
                {
                    if (!mode.Current || !Cleanup2NoNarrativeOverlay() || Game.Instance.IsControllerGamepad ||
                        (Game.Instance.SelectedAbilityHandler.Ability != null && !ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, child)))
                        throw new InvalidOperationException("Native cast lost input ownership.");
                    Game.Instance.IsPaused = false; yield return 0;
                }
                var units = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
                foreach (var unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                var expected = SummonSignatureReader.Read(child.Blueprint).Spawns().ToArray();
                int min = spec.Multiplicity == SummonMultiplicity.OneD4PlusOne ? 2 : 1;
                int max = spec.Multiplicity == SummonMultiplicity.One ? 1 : spec.Multiplicity == SummonMultiplicity.OneD3 ? 3 : 5;
                var durations = ExpandedSummoningRuleDurationCapture.ToArray();
                bool spent = child.Spellbook.Blueprint.Spontaneous ? debit.ExactlyOneSpontaneous(child.Spellbook, spec.ParentTier) :
                    debit.Changed.Length == 1 && debit.Changed[0].Spell != null && debit.Changed[0].Spell.Equals(source) &&
                    debit.Changed[0].SpellLevel == spec.ParentTier && debit.ExactlyOne(debit.Changed[0]);
                bool exact = issued && withinRange && command.IsFinished && command.Result.ToString() == "Success" && fixture.Caster.Commands.Empty &&
                    Cleanup2DruidCastRules.Count == 1 && Cleanup2DruidCastRules[0].Success && ReferenceEquals(Cleanup2DruidCastRules[0].Spell, child) &&
                    _expandedSummoningPlayerPathRuleCastCount == 1 && _expandedSummoningPlayerPathSpawnActionCount == 1 &&
                    expected.Length == 1 && units.Length >= min && units.Length <= max && units.All(unit => unit.Blueprint.AssetGuid == expected[0].Unit) &&
                    ExpandedSummoningRuleCapture.Distinct().Count() == units.Length && durations.Length == units.Length &&
                    durations.All(value => Math.Abs(value.BaseDuration.TotalSeconds - 120d) <= .001 && value.BonusDuration.TotalSeconds == 0) &&
                    chain() && spent && economy.InCombat(command) && economy.Exact(command, false) && economy.ForcedCount(command) == 0 &&
                    costs.Count == 1 && (string)costs[0]["nativeCost"] == "UnitUseAbility" &&
                    (string)costs[0]["type"] == child.RuntimeActionType.ToString() && (bool)costs[0]["fullRound"] == child.RequireFullRoundAction;
                Sprint25Check(label + "-cast", exact, new JObject { ["source"] = source.Blueprint.AssetGuid, ["child"] = child.Blueprint.AssetGuid,
                    ["nativeCommand"] = command == null ? null : Sprint27AiCommand(command), ["withinCastRange"] = withinRange,
                    ["targetDistance"] = Vector3.Distance(fixture.Caster.Position, point), ["sourceChainExact"] = chain(),
                    ["quantity"] = units.Length, ["minimum"] = min, ["maximum"] = max,
                    ["units"] = new JArray(units.Select(unit => new JObject { ["id"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.AssetGuid })),
                    ["baseDurations"] = new JArray(durations.Select(value => value.BaseDuration.TotalSeconds)),
                    ["costs"] = costs, ["expectedAction"] = child.RuntimeActionType.ToString(), ["expectedFullRound"] = child.RequireFullRoundAction,
                    ["nativeCostDeltaExact"] = command != null && economy.Exact(command, false),
                    ["combatAtCost"] = command != null && economy.InCombat(command),
                    ["nativeForcedFinishes"] = command == null ? 0 : economy.ForcedCount(command),
                    ["debit"] = child.Spellbook.Blueprint.Spontaneous ? debit.DescribeSpontaneous(child.Spellbook, spec.ParentTier) : debit.Describe(debit.Changed.FirstOrDefault()),
                    ["equivalentPreparedSourceOrSpontaneousDebitExact"] = spent, ["nativePathEvents"] = new JArray(ExpandedSummoningPlayerPathEvents),
                    ["manualTicks"] = 0, ["forcedAct"] = false, ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "Visible native row and target input produce the exact command, natural action/slot cost, legal quantity, finite owned duration. No supplied act or cost.");
                if (!exact) throw new InvalidOperationException("Natural input cast failed: " + label);
            }
            finally
            {
                _cleanup2DruidCastExpected = null; _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id); Cleanup2DruidCastRules.Clear();
                if (ReferenceEquals(Game.Instance.SelectedAbilityHandler?.Ability, child)) Game.Instance.SelectedAbilityHandler.DropAbility();
                if (command != null && !command.IsFinished && ReferenceEquals(command.Spell, child) &&
                    ReferenceEquals(fixture.Caster.Commands.GetCommand<UnitUseAbility>(), command)) command.Interrupt(true);
                var positive = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
                foreach (var unit in positive) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                DisposeExpandedSummoningUnits(fixture.Created, positive);
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            bool reset = before.SequenceEqual(SnapshotReferences(fixture.AllUnits));
            Sprint25Check(label + "-reset", reset, new JObject { ["exactUnitReferencesRestored"] = reset },
                "The prior native unit set is restored before another source is exercised; fixture disposal is not natural expiry proof.");
            if (!reset) throw new InvalidOperationException("Native input subcase reset failed.");
        }
    }
}

