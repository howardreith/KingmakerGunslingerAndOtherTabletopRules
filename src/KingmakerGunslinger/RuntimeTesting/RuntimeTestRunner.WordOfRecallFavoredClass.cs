using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using UnityEngine;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Genuine Favored Class Oracle bonus-spell identities, verified against
        // the installed ZFavoredClass 1.3.1 blueprint store.
        private const string FcbFavoredClassSelectionId =
            "27947ef789544982a437200c3189c59a";
        private const string FcbOracleClassSelectionId =
            "c6f18fa1194d0bfb35e1913983b8da98";
        private const string FcbOracleBonusSpellSelectionId =
            "9ba3858327354e2093613efb9de198d7";
        private const string FcbOracleLevel6FeatureId =
            "7249760f01784ea997afaa9c433c2e68";
        private const string FcbOraclePartialFeatureId =
            "b19759026d6508b9022f1edb4ec4b31f";
        private const string FcbOracleProgressionId =
            "52ee82659e040ed231ed36c8e5457e38";
        private const string FcbAasimarRaceId =
            "b7f02ba92b363064fb873963bec275ee";
        private const string FcbHumanRaceId =
            "0a5d473ead98b0646b94495af250fdc4";

        private BlueprintRace ResolveFcbRace(string assetId, string role)
        {
            BlueprintScriptableObject blueprint;
            if (!BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                assetId, out blueprint))
                throw new InvalidOperationException(
                    "The " + role + " race blueprint is absent.");
            BlueprintRace race = blueprint as BlueprintRace;
            if (race == null)
                throw new InvalidOperationException(
                    "The " + role + " race identity is not a BlueprintRace.");
            return race;
        }

        // Seeds a request-local native Oracle exactly like the committed
        // learning fixture, but through the genuine Favored Class route: the
        // favored class selection picks Oracle, every completed favored-class
        // level picks the half-spell partial award (never the completed bonus
        // spell), and ordinary filler choices never consume Word of Recall.
        private UnitEntityData SpawnFcbOracleFixture(
            BlueprintCharacterClass oracle, BlueprintRace race, string name,
            UnitEntityData anchor, Player player, out LevelUpController seedBackend,
            int levels, List<object> trace)
        {
            seedBackend = null;
            var game = Game.Instance;
            var dollState = new DollState();
            dollState.SetGender(anchor.Descriptor.Gender); dollState.SetRace(race);
            dollState.SetClass(oracle);
            var doll = dollState.CreateData();
            var view = doll.CreateUnitView(false);
            if (view == null)
                throw new InvalidOperationException(
                    "The Favored Class Oracle fixture has no real view.");
            view.Blueprint = game.BlueprintRoot.DefaultPlayerCharacter;
            view.UniqueId = Guid.NewGuid().ToString();
            view.transform.position = anchor.Position;
            var pending = (System.Collections.IList)game.EntityCreator.GetType()
                .GetField("m_ToCreate", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(game.EntityCreator);
            if (pending.Count != 0)
                throw new InvalidOperationException(
                    "Unrelated native entity creation is pending.");
            UnitEntityData unit = game.EntityCreator.SpawnEntityWithView(
                view, player.CrossSceneState) as UnitEntityData;
            if (unit == null)
                throw new InvalidOperationException(
                    "Favored Class Oracle entity ownership transfer failed.");
            game.EntityCreator.Tick();
            unit.Descriptor.Doll = doll;
            unit.Descriptor.CustomGender = anchor.Descriptor.Gender;
            unit.Descriptor.CustomName = name;
            unit.Stats.Charisma.BaseValue = 18;
            unit.Stats.Intelligence.BaseValue = 10;
            unit.Stats.Wisdom.BaseValue = 12;
            unit.Descriptor.TurnOn();
            for (int level = 0; level < levels; level++)
            {
                seedBackend = LevelUpController.StartWithoutAssigningStaticInstance(
                    unit.Descriptor, false, null, null,
                    level == 0 ? LevelUpState.CharBuildMode.CharGen :
                        LevelUpState.CharBuildMode.LevelUp);
                if (level == 0)
                {
                    seedBackend.SelectRace(race);
                    seedBackend.SelectGender(anchor.Descriptor.Gender);
                    seedBackend.SelectAlignment(anchor.Descriptor.Alignment.Value);
                }
                if (!seedBackend.SelectClass(oracle))
                    throw new InvalidOperationException(
                        "Native Oracle seed class selection failed.");
                FillFcbOracleChoices(seedBackend, oracle, trace);
                typeof(LevelUpController).GetMethod("ApplyLevelup",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(seedBackend, new object[] { unit.Descriptor });
                seedBackend.Cancel(); seedBackend = null;
                if (unit.Descriptor.Progression.GetClassLevel(oracle) != level + 1)
                    throw new InvalidOperationException(
                        "Favored Class Oracle seed progression did not advance.");
            }
            return unit;
        }

        // Native choice filler for the Favored Class route. Deterministic
        // policy: pick Oracle in the genuine favored class selection; prefer
        // the half-spell partial award in the Oracle favored-class selection
        // while its rank is not full; never select the completed bonus-spell
        // selection during seeding; ordinary spell picks exclude the canonical
        // Word of Recall so the award stays available.
        private static void FillFcbOracleChoices(LevelUpController backend,
            BlueprintCharacterClass oracle, List<object> trace)
        {
            var recall = BlueprintBootstrap.Teleportation.WordOfRecall;
            for (int step = 0; step < 256; step++)
            {
                if (backend.State.AttributePoints > 0 &&
                    backend.SpendAttributePoint(StatType.Charisma)) continue;
                var feature = backend.State.Selections.FirstOrDefault(value =>
                    !value.Selected && value.CanSelectAnything(backend.State, backend.Preview));
                if (feature != null)
                {
                    var candidates = feature.Selection.ExtractSelectionItems(
                        backend.Unit, backend.Preview).Where(value =>
                        feature.Selection.CanSelect(backend.Preview, backend.State,
                            feature, value)).ToArray();
                    IFeatureSelectionItem pick = null;
                    string policy = "guid";
                    if (feature.Selection is BlueprintFeatureSelection typed &&
                        typed.AssetGuid == FcbFavoredClassSelectionId)
                    {
                        pick = candidates.FirstOrDefault(value =>
                            value.Feature is BlueprintProgression progression &&
                            progression.Classes != null &&
                            progression.Classes.Length == 1 &&
                            ReferenceEquals(progression.Classes[0], oracle));
                        policy = "favored-oracle";
                    }
                    else if (feature.Selection is BlueprintFeatureSelection bonus &&
                        bonus.AssetGuid == FcbOracleClassSelectionId)
                    {
                        pick = candidates.FirstOrDefault(value =>
                            value.Feature != null &&
                            value.Feature.AssetGuid == FcbOraclePartialFeatureId);
                        if (pick == null)
                            pick = candidates.FirstOrDefault(value =>
                                value.Feature != null &&
                                value.Feature.AssetGuid != FcbOracleBonusSpellSelectionId);
                        policy = "fcb-partial-first";
                    }
                    if (pick == null)
                        pick = candidates.OrderBy(value => value.Feature.AssetGuid,
                            StringComparer.Ordinal).FirstOrDefault();
                    trace.Add(new { policy, selection =
                        ((BlueprintScriptableObject)feature.Selection).name,
                        picked = pick == null || pick.Feature == null ? null :
                            pick.Feature.AssetGuid + ":" + pick.Feature.name,
                        candidates = candidates.Select(value => value.Feature == null ?
                            null : value.Feature.AssetGuid + ":" + value.Feature.name)
                            .ToArray() });
                    if (pick != null && backend.SelectFeature(feature, pick)) continue;
                }
                bool selected = false;
                foreach (var selection in backend.State.SpellSelections.ToArray())
                {
                    var book = backend.Preview.GetSpellbook(selection.Spellbook);
                    for (int level = 0; level < selection.LevelCount.Length && !selected; level++)
                    {
                        var slots = selection.LevelCount[level];
                        if (slots == null) continue;
                        int slot = Array.FindIndex(slots.SpellSelections, value => value == null);
                        if (slot < 0) continue;
                        var spell = selection.SpellList.GetSpells(level)
                            .Where(value => !book.IsKnown(value) &&
                                !ReferenceEquals(value, recall))
                            .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                            .FirstOrDefault();
                        if (spell != null) selected = backend.SelectSpell(
                            selection.Spellbook, selection.SpellList, level, spell, slot);
                    }
                    if (selected) break;
                }
                if (selected) continue;
                foreach (StatType skill in Enum.GetValues(typeof(StatType)))
                    if (skill.ToString().StartsWith("Skill", StringComparison.Ordinal) &&
                        backend.State.SkillPointsRemaining > 0 &&
                        backend.SpendSkillPoint(skill))
                    { selected = true; break; }
                if (!selected) return;
            }
            throw new InvalidOperationException(
                "The Favored Class Oracle choice helper did not converge.");
        }

        private static object FcbSelectionSnapshot(LevelUpController backend)
        {
            return new {
                selections = backend.State.Selections.Select(value => new {
                    selection = value.Selection == null ? null :
                        ((BlueprintScriptableObject)value.Selection).name + ":" +
                        ((BlueprintScriptableObject)value.Selection).AssetGuid,
                    selected = value.SelectedItem == null ? null : new {
                        feature = value.SelectedItem.Feature == null ? null :
                            value.SelectedItem.Feature.AssetGuid,
                        param = value.SelectedItem.Param == null ||
                            value.SelectedItem.Param.Value == null ||
                            value.SelectedItem.Param.Value.Blueprint == null ? null :
                            value.SelectedItem.Param.Value.Blueprint.AssetGuid }
                }).ToArray(),
                spells = backend.State.SpellSelections.Select(value => new {
                    list = value.SpellList.AssetGuid,
                    counts = value.LevelCount.Select(part => part == null ? 0 :
                        part.SpellSelections.Length).ToArray(),
                    extra = value.ExtraSelected == null ? 0 : value.ExtraSelected.Length
                }).ToArray()
            };
        }

        // Genuine Favored Class bonus-spell route qualification on fresh
        // request-local native Oracles. Everything runs through the native
        // favored-class selections, the real level-up presenter and the real
        // completion button; the fixture never injects Word of Recall into a
        // selector, never calls AddKnown and never fabricates an ordinary
        // spell-known choice or favored-class credit.
        private IEnumerable<int> QualifyOracleFavoredClassLearning()
        {
            var oracle = TeleportationFinalLiveReconciler.ResolveOracleClass(
                BlueprintBootstrap.Library);
            if (oracle == null) yield break;
            var library = BlueprintBootstrap.Library;
            var recall = BlueprintBootstrap.Teleportation.WordOfRecall;
            BlueprintScriptableObject raw;
            var favoredClassSelection = library.BlueprintsByAssetId.TryGetValue(
                FcbFavoredClassSelectionId, out raw) ? raw as BlueprintFeatureSelection : null;
            var classSelection = library.BlueprintsByAssetId.TryGetValue(
                FcbOracleClassSelectionId, out raw) ? raw as BlueprintFeatureSelection : null;
            var bonusSelection = library.BlueprintsByAssetId.TryGetValue(
                FcbOracleBonusSpellSelectionId, out raw) ? raw as BlueprintFeatureSelection : null;
            var level6Feature = library.BlueprintsByAssetId.TryGetValue(
                FcbOracleLevel6FeatureId, out raw) ? raw as BlueprintParametrizedFeature : null;
            var partialFeature = library.BlueprintsByAssetId.TryGetValue(
                FcbOraclePartialFeatureId, out raw) ? raw as BlueprintFeature : null;
            CaptureTeleportSpellbookUi("fcb-presence", new {
                oracle = oracle.AssetGuid, recall = recall.AssetGuid,
                favoredClassSelection = favoredClassSelection != null,
                classSelection = classSelection != null,
                bonusSelection = bonusSelection != null,
                level6Feature = level6Feature != null,
                partialFeature = partialFeature != null,
                teleportationSpells = _context.FeatureModules.Active.TeleportationSpells });
            if (favoredClassSelection == null || classSelection == null ||
                bonusSelection == null || level6Feature == null || partialFeature == null)
                throw new InvalidOperationException(
                    "The installed Favored Class Oracle route is incomplete.");
            if (!_context.FeatureModules.Active.TeleportationSpells ||
                BlueprintBootstrap.TeleportationPublication == null ||
                !_request.ExitAfterCompletion || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException(
                    "Favored Class qualification requires the published teleportation module and the guarded disposable request.");
            var aasimar = ResolveFcbRace(FcbAasimarRaceId, "Aasimar");
            var human = ResolveFcbRace(FcbHumanRaceId, "Human");
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            var presenter = ui.CharacterBuildController;
            var priorBackend = ui.LevelUpController; var priorPresenterUnit = presenter.Unit;
            var originalParty = player.Party.ToArray();
            var originalPartyRefs = player.PartyCharacters.ToArray();
            var originalCross = player.CrossSceneState.AllEntityData.ToArray();
            var originalItems = player.Inventory.Items.ToArray();
            var originalCounts = originalItems.Select(value => value.Count).ToArray();
            var starterDeltas = new Dictionary<Kingmaker.Items.ItemEntity, int>();
            Action<string> captureStarterItems = stage => {
                foreach (var item in player.Inventory.Items)
                {
                    int index = Array.IndexOf(originalItems, item);
                    int delta = item.Count - (index < 0 ? 0 : originalCounts[index]);
                    if (delta > 0) starterDeltas[item] = delta;
                }
                CaptureTeleportSpellbookUi(stage, starterDeltas.Select(value => new {
                    item = value.Key.Blueprint.AssetGuid, count = value.Value }).ToArray());
            };
            long originalMoney = player.Money;
            bool originalPause = game.IsPaused;
            var experienceProperty = typeof(UnitProgressionData).GetProperty("Experience");
            UnitEntityData aasimarUnit = null, humanUnit = null;
            LevelUpController backend = null;
            int successes = 0;
            bool aasimarCommitted = false, humanCommitted = false;
            var aasimarTrace = new List<object>();
            var humanTrace = new List<object>();
            Application.logMessageReceived += ObserveTeleportSpellbookUiException;
            try
            {
                game.IsPaused = true;
                var anchor = originalParty.First(value => value.View != null &&
                    value.Descriptor.Progression.Race != null);
                // ----- Aasimar route: seed to Oracle 13 through native level-ups.
                aasimarUnit = SpawnFcbOracleFixture(oracle, aasimar,
                    "KMG FCB Aasimar Oracle", anchor, player, out backend, 13, aasimarTrace);
                captureStarterItems("fcb-aasimar-seed-starter-items");
                var book = aasimarUnit.Descriptor.GetSpellbook(oracle.Spellbook);
                var partialFact = aasimarUnit.Descriptor.Progression.Features.Enumerable
                    .Where(value => ReferenceEquals(value.Blueprint, partialFeature)).ToArray();
                CaptureTeleportSpellbookUi("fcb-aasimar-seeded", new {
                    classLevel = aasimarUnit.Descriptor.Progression.GetClassLevel(oracle),
                    casterLevel = book.CasterLevel, maxSpellLevel = book.MaxSpellLevel,
                    partialPicks = partialFact.Length,
                    partialRank = partialFact.Length == 0 ? 0 : partialFact[0].Rank,
                    knowsRecall = book.IsKnown(recall),
                    favoredProgression = aasimarUnit.Descriptor.Progression.Features.Enumerable
                        .Any(value => value.Blueprint.AssetGuid == FcbOracleProgressionId),
                    trace = aasimarTrace.ToArray() });
                var partialPicks = aasimarTrace.Count(value => {
                    var pick = value.GetType().GetProperty("picked").GetValue(value, null) as string;
                    return pick != null && pick.StartsWith(FcbOraclePartialFeatureId);
                });
                var firstFcbCandidates = aasimarTrace.FirstOrDefault(value => {
                    var policy = value.GetType().GetProperty("policy").GetValue(value, null) as string;
                    return policy == "fcb-partial-first";
                });
                var creditlessCandidates = firstFcbCandidates == null ? new string[0] :
                    ((System.Collections.IEnumerable)firstFcbCandidates.GetType()
                        .GetProperty("candidates").GetValue(firstFcbCandidates, null))
                        .Cast<object>().Select(value => value as string).ToArray();
                TeleportSpellbookUiAssert("fcb-aasimar-seeded",
                    "a genuine Aasimar Oracle 13 with the completed native half-spell credit and no Recall",
                    "level=" + aasimarUnit.Descriptor.Progression.GetClassLevel(oracle) +
                        ";caster=" + book.CasterLevel + ";max=" + book.MaxSpellLevel +
                        ";partialRank=" + (partialFact.Length == 0 ? 0 : partialFact[0].Rank) +
                        ";partialPicks=" + partialPicks + ";knows=" + book.IsKnown(recall),
                    aasimarUnit.Descriptor.Progression.GetClassLevel(oracle) == 13 &&
                        book.CasterLevel == 13 && book.MaxSpellLevel == 6 &&
                        partialFact.Length == 1 && partialFact[0].Rank >= 1 &&
                        partialPicks == 1 && !book.IsKnown(recall));
                TeleportSpellbookUiAssert("fcb-incomplete-award-control",
                    "before any partial credit the completed bonus-spell award is not offered",
                    "firstCandidates=" + string.Join("|", creditlessCandidates.ToArray()),
                    creditlessCandidates.Length > 0 && !creditlessCandidates.Any(value =>
                        value != null && value.StartsWith(FcbOracleBonusSpellSelectionId)));
                // ----- The genuine 13 -> 14 award level-up through the presenter.
                experienceProperty.SetValue(aasimarUnit.Descriptor.Progression,
                    game.BlueprintRoot.Progression.XPTable.GetBonus(14), null);
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    backend = null; successes = 0;
                    presenter.HandleLevelUpStart(aasimarUnit.Descriptor, null, () => successes++);
                    backend = presenter.LevelUpController;
                    if (backend == null || backend.AutoCommit ||
                        ReferenceEquals(backend.Preview, aasimarUnit.Descriptor))
                        throw new InvalidOperationException(
                            "Favored Class level-up did not create an independent preview.");
                    for (int frame = 0; frame < 15; frame++) yield return 0;
                    presenter.SetClass(oracle);
                    foreach (int tick in OpenFcbAwardAndSelectRecall(oracle, bonusSelection,
                        level6Feature, recall, "aasimar-" + attempt)) yield return tick;
                    if (attempt == 0)
                    {
                        presenter.OnHotKeyEscPressed();
                        foreach (int tick in WaitTeleportLevelUpUi(() =>
                            DialogMessageBox.Instance.IsShown, "FCB cancel confirmation")) yield return tick;
                        var cancelCallback = (Action<DialogMessageBoxBase.BoxButton>)
                            WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(
                                DialogMessageBox.Instance);
                        if (cancelCallback == null ||
                            !ReferenceEquals(cancelCallback.Target, presenter))
                            throw new InvalidOperationException(
                                "The FCB cancel dialog has an unrelated owner.");
                        TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                        foreach (int tick in WaitTeleportLevelUpUi(() =>
                            !presenter.IsShow && !DialogMessageBox.Instance.IsShown,
                            "FCB cancelled preview")) yield return tick;
                        backend.Cancel(); backend = null; ui.LevelUpController = priorBackend;
                        book = aasimarUnit.Descriptor.GetSpellbook(oracle.Spellbook);
                        var cancelPartial = aasimarUnit.Descriptor.Progression.Features.Enumerable
                            .Where(value => ReferenceEquals(value.Blueprint, partialFeature)).ToArray();
                        TeleportSpellbookUiAssert("fcb-cancel",
                            "cancel learns nothing and preserves the completed award",
                            "known=" + book.IsKnown(recall) + ";level=" +
                                aasimarUnit.Descriptor.Progression.GetClassLevel(oracle) +
                                ";rank=" + (cancelPartial.Length == 0 ? 0 : cancelPartial[0].Rank),
                            !book.IsKnown(recall) &&
                                aasimarUnit.Descriptor.Progression.GetClassLevel(oracle) == 13 &&
                                cancelPartial.Length == 1 && cancelPartial[0].Rank == 1);
                        continue;
                    }
                    FillFcbOracleChoices(backend, oracle, aasimarTrace);
                    foreach (int tick in WaitTeleportLevelUpUi(() => backend.State.IsComplete(),
                        "all Favored Class level-up choices complete")) yield return tick;
                    var finalSpells = backend.State.SpellSelections.Single(value =>
                        value.Spellbook == oracle.Spellbook);
                    var finalState = FcbSelectionSnapshot(backend);
                    CaptureTeleportSpellbookUi("fcb-aasimar-summary", finalState);
                    presenter.Next();
                    foreach (int tick in WaitTeleportLevelUpUi(() =>
                        presenter.CurrentPhase == CharBPhase.Type.Total,
                        "FCB native summary")) yield return tick;
                    var finish = (UnityEngine.UI.Button)typeof(CharacterBuildController)
                        .GetField("m_CompleteButton", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(presenter);
                    if (finish == null || !finish.interactable || !backend.State.IsComplete())
                        throw new InvalidOperationException(
                            "The native FCB completion button is unavailable.");
                    finish.onClick.Invoke(); aasimarCommitted = true; backend = null;
                    foreach (int tick in WaitTeleportLevelUpUi(() => !presenter.IsShow,
                        "FCB native committed level-up")) yield return tick;
                    ui.LevelUpController = priorBackend;
                    book = aasimarUnit.Descriptor.GetSpellbook(oracle.Spellbook);
                    var grantFacts = aasimarUnit.Descriptor.Progression.Features.Enumerable
                        .Where(value => ReferenceEquals(value.Blueprint, level6Feature)).ToArray();
                    var recallFacts = grantFacts.Where(value => value.Param != null &&
                        value.Param.Value != null &&
                        ReferenceEquals(value.Param.Value.Blueprint, recall)).ToArray();
                    var committedPartial = aasimarUnit.Descriptor.Progression.Features.Enumerable
                        .Where(value => ReferenceEquals(value.Blueprint, partialFeature)).ToArray();
                    bool ordinarySixthNormal = finalSpells.ExtraSelected == null ||
                        finalSpells.ExtraSelected.Length == 0;
                    CaptureTeleportSpellbookUi("fcb-aasimar-committed", new {
                        classLevel = aasimarUnit.Descriptor.Progression.GetClassLevel(oracle),
                        casterLevel = book.CasterLevel, callbacks = successes,
                        knownSixth = book.GetKnownSpells(6).Select(value =>
                            value.Blueprint.AssetGuid).ToArray(),
                        grantFacts = grantFacts.Length, recallFacts = recallFacts.Length,
                        partialRank = committedPartial.Length == 0 ? 0 : committedPartial[0].Rank,
                        ordinarySixthSlots = finalSpells.LevelCount[6].SpellSelections.Length,
                        ordinarySixthAllowance = book.Blueprint.SpellsKnown.GetCount(book.CasterLevel, 6),
                        extraSelected = finalSpells.ExtraSelected == null ? 0 :
                            finalSpells.ExtraSelected.Length });
                    TeleportSpellbookUiAssert("fcb-aasimar-committed",
                        "native completion teaches canonical Recall once at Oracle 6 through one favored-class award",
                        "level=" + aasimarUnit.Descriptor.Progression.GetClassLevel(oracle) +
                            ";known6=" + book.GetKnownSpells(6).Count(value =>
                                ReferenceEquals(value.Blueprint, recall)) +
                            ";grants=" + grantFacts.Length + ";recallFacts=" + recallFacts.Length +
                            ";extra=" + (finalSpells.ExtraSelected == null ? 0 :
                                finalSpells.ExtraSelected.Length),
                        aasimarUnit.Descriptor.Progression.GetClassLevel(oracle) == 14 &&
                            book.CasterLevel == 14 && successes == 1 &&
                            book.GetKnownSpells(6).Count(value =>
                                ReferenceEquals(value.Blueprint, recall)) == 1 &&
                            grantFacts.Length == 1 && recallFacts.Length == 1 &&
                            committedPartial.Length == 1 && committedPartial[0].Rank == 1 &&
                            (finalSpells.ExtraSelected == null ||
                                finalSpells.ExtraSelected.Length == 0) && ordinarySixthNormal);
                    // ----- Duplicate control: a further level-up must not re-offer Recall.
                    experienceProperty.SetValue(aasimarUnit.Descriptor.Progression,
                        game.BlueprintRoot.Progression.XPTable.GetBonus(15), null);
                    presenter.HandleLevelUpStart(aasimarUnit.Descriptor, null, () => successes++);
                    backend = presenter.LevelUpController;
                    for (int frame = 0; frame < 15; frame++) yield return 0;
                    presenter.SetClass(oracle);
                    var duplicateItems = level6Feature.ExtractSelectionItems(
                        backend.Unit, backend.Preview).ToArray();
                    int duplicateRecall = duplicateItems.Count(value =>
                        value.Param != null && value.Param.Value != null &&
                        ReferenceEquals(value.Param.Value.Blueprint, recall));
                    CaptureTeleportSpellbookUi("fcb-duplicate-control", new {
                        candidates = duplicateItems.Length, duplicateRecall,
                        known = backend.Preview.GetSpellbook(oracle.Spellbook).IsKnown(recall) });
                    TeleportSpellbookUiAssert("fcb-duplicate-control",
                        "an Oracle that already knows Recall is not offered it again",
                        "candidates=" + duplicateItems.Length + ";recall=" + duplicateRecall,
                        duplicateRecall == 0 &&
                            backend.Preview.GetSpellbook(oracle.Spellbook).IsKnown(recall));
                    // The preview is rebuilt by natively serializing the committed
                    // unit and deserializing it through the save-format path, so a
                    // surviving fact proves the parametrized pick persists.
                    var persistedFacts = backend.Preview.Progression.Features.Enumerable
                        .Where(value => ReferenceEquals(value.Blueprint, level6Feature)).ToArray();
                    var persistedRecall = persistedFacts.Count(value => value.Param != null &&
                        value.Param.Value != null &&
                        ReferenceEquals(value.Param.Value.Blueprint, recall));
                    var persistedBook = backend.Preview.GetSpellbook(oracle.Spellbook);
                    TeleportSpellbookUiAssert("fcb-persistence-roundtrip",
                        "the committed parametrized favored-class selection and learned spell survive native serialization",
                        "facts=" + persistedFacts.Length + ";recall=" + persistedRecall +
                            ";known6=" + persistedBook.GetKnownSpells(6).Count(value =>
                                ReferenceEquals(value.Blueprint, recall)),
                        persistedFacts.Length == 1 && persistedRecall == 1 &&
                            persistedBook.GetKnownSpells(6).Count(value =>
                                ReferenceEquals(value.Blueprint, recall)) == 1);
                    presenter.OnHotKeyEscPressed();
                    foreach (int tick in WaitTeleportLevelUpUi(() =>
                        DialogMessageBox.Instance.IsShown, "duplicate control cancel")) yield return tick;
                    TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                    foreach (int tick in WaitTeleportLevelUpUi(() => !presenter.IsShow &&
                        !DialogMessageBox.Instance.IsShown, "duplicate control closed")) yield return tick;
                    backend.Cancel(); backend = null; ui.LevelUpController = priorBackend;
                    // ----- Cast the favored-class-learned Recall on the world map.
                    player.PartyCharacters.Add(aasimarUnit);
                    player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                    if (!player.Party.Contains(aasimarUnit))
                        throw new InvalidOperationException(
                            "The Favored Class Oracle did not enter the traveling party.");
                    captureStarterItems("fcb-aasimar-party-starter-items");
                    if (game.CurrentMode != GameModeType.GlobalMap)
                    {
                        game.LoadArea(game.BlueprintRoot.GlobalMap.GlobalMapEnterPoint,
                            Kingmaker.EntitySystem.Persistence.AutoSaveMode.None);
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        while (Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess ||
                            Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingScreenActive ||
                            Kingmaker.Globalmap.GlobalMapRules.Instance == null ||
                            game.CurrentMode != GameModeType.GlobalMap)
                        {
                            if (watch.Elapsed.TotalSeconds > 60)
                                throw new InvalidOperationException(
                                    "FCB Oracle world-map load timed out.");
                            yield return 0;
                        }
                    }
                    foreach (int tick in CastNewlyLearnedOracleRecall(aasimarUnit,
                        aasimarUnit.Descriptor.GetSpellbook(oracle.Spellbook))) yield return tick;
                }
                // ----- Human route with the below-prerequisite control.
                humanUnit = SpawnFcbOracleFixture(oracle, human,
                    "KMG FCB Human Oracle", anchor, player, out backend, 12, humanTrace);
                captureStarterItems("fcb-human-seed-starter-items");
                var humanBook = humanUnit.Descriptor.GetSpellbook(oracle.Spellbook);
                var humanPartial = humanUnit.Descriptor.Progression.Features.Enumerable
                    .Where(value => ReferenceEquals(value.Blueprint, partialFeature)).ToArray();
                TeleportSpellbookUiAssert("fcb-human-seeded",
                    "a genuine Human Oracle 12 with the completed half-spell credit",
                    "level=" + humanUnit.Descriptor.Progression.GetClassLevel(oracle) +
                        ";max=" + humanBook.MaxSpellLevel + ";facts=" + humanPartial.Length +
                        ";rank=" + (humanPartial.Length == 0 ? 0 : humanPartial[0].Rank) +
                        ";knows=" + humanBook.IsKnown(recall),
                    humanUnit.Descriptor.Progression.GetClassLevel(oracle) == 12 &&
                        humanBook.MaxSpellLevel == 6 && humanPartial.Length >= 1 &&
                        humanPartial[0].Rank >= 1 && !humanBook.IsKnown(recall));
                experienceProperty.SetValue(humanUnit.Descriptor.Progression,
                    game.BlueprintRoot.Progression.XPTable.GetBonus(13), null);
                presenter.HandleLevelUpStart(humanUnit.Descriptor, null, () => successes++);
                backend = presenter.LevelUpController;
                for (int frame = 0; frame < 15; frame++) yield return 0;
                presenter.SetClass(oracle);
                var belowItems = level6Feature.ExtractSelectionItems(
                    backend.Unit, backend.Preview).ToArray();
                var belowClassState = backend.State.Selections.SingleOrDefault(value =>
                    value.Selection is BlueprintFeatureSelection typed &&
                    typed.AssetGuid == FcbOracleClassSelectionId);
                var belowAwardItem = belowClassState == null ? null :
                    belowClassState.Selection.ExtractSelectionItems(backend.Unit, backend.Preview)
                        .FirstOrDefault(value => value.Feature != null &&
                            value.Feature.AssetGuid == FcbOracleBonusSpellSelectionId);
                bool belowAwardSelectable = belowClassState != null && belowAwardItem != null &&
                    belowClassState.Selection.CanSelect(backend.Preview, backend.State,
                        belowClassState, belowAwardItem);
                bool belowLevel6Selectable = false;
                FeatureSelectionState belowBonusState = null;
                if (belowAwardSelectable && backend.SelectFeature(belowClassState, belowAwardItem))
                {
                    belowBonusState = backend.State.Selections.FirstOrDefault(value =>
                        value.Selection is BlueprintFeatureSelection typed &&
                        typed.AssetGuid == FcbOracleBonusSpellSelectionId && !value.Selected);
                    if (belowBonusState != null)
                    {
                        var belowLevel6Item = belowBonusState.Selection.ExtractSelectionItems(
                            backend.Unit, backend.Preview).FirstOrDefault(value =>
                                value.Feature != null &&
                                value.Feature.AssetGuid == FcbOracleLevel6FeatureId);
                        belowLevel6Selectable = belowLevel6Item != null &&
                            belowBonusState.Selection.CanSelect(backend.Preview, backend.State,
                                belowBonusState, belowLevel6Item);
                    }
                }
                CaptureTeleportSpellbookUi("fcb-below-prerequisite-control", new {
                    casterLevel = backend.Preview.GetSpellbook(oracle.Spellbook).CasterLevel,
                    maxSpellLevel = backend.Preview.GetSpellbook(oracle.Spellbook).MaxSpellLevel,
                    belowItems = belowItems.Length,
                    belowRecall = belowItems.Count(value => value.Param != null &&
                        value.Param.Value != null &&
                        ReferenceEquals(value.Param.Value.Blueprint, recall)),
                    awardSelectable = belowAwardSelectable,
                    bonusStateOpened = belowBonusState != null,
                    level6Selectable = belowLevel6Selectable });
                TeleportSpellbookUiAssert("fcb-below-prerequisite-control",
                    "sixth-level favored-class access stays unavailable below the class spell-level prerequisite",
                    "max=" + backend.Preview.GetSpellbook(oracle.Spellbook).MaxSpellLevel +
                        ";awardSelectable=" + belowAwardSelectable +
                        ";bonusStateOpened=" + (belowBonusState != null) +
                        ";level6Selectable=" + belowLevel6Selectable,
                    backend.Preview.GetSpellbook(oracle.Spellbook).MaxSpellLevel <= 6 &&
                        belowAwardSelectable && belowBonusState != null && !belowLevel6Selectable);
                presenter.OnHotKeyEscPressed();
                foreach (int tick in WaitTeleportLevelUpUi(() =>
                    DialogMessageBox.Instance.IsShown, "below-control cancel")) yield return tick;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                foreach (int tick in WaitTeleportLevelUpUi(() => !presenter.IsShow &&
                    !DialogMessageBox.Instance.IsShown, "below-control closed")) yield return tick;
                backend.Cancel(); backend = null; ui.LevelUpController = priorBackend;
                // Complete Oracle 13 normally, then take the shared award at 13 -> 14.
                for (int extra = 0; extra < 1; extra++)
                {
                    backend = LevelUpController.StartWithoutAssigningStaticInstance(
                        humanUnit.Descriptor, false, null, null,
                        LevelUpState.CharBuildMode.LevelUp);
                    if (!backend.SelectClass(oracle))
                        throw new InvalidOperationException(
                            "Human Oracle seed class selection failed.");
                    FillFcbOracleChoices(backend, oracle, humanTrace);
                    typeof(LevelUpController).GetMethod("ApplyLevelup",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(backend, new object[] { humanUnit.Descriptor });
                    backend.Cancel(); backend = null;
                }
                experienceProperty.SetValue(humanUnit.Descriptor.Progression,
                    game.BlueprintRoot.Progression.XPTable.GetBonus(14), null);
                presenter.HandleLevelUpStart(humanUnit.Descriptor, null, () => successes++);
                backend = presenter.LevelUpController;
                for (int frame = 0; frame < 15; frame++) yield return 0;
                presenter.SetClass(oracle);
                foreach (int tick in OpenFcbAwardAndSelectRecall(oracle, bonusSelection,
                    level6Feature, recall, "human")) yield return tick;
                FillFcbOracleChoices(backend, oracle, humanTrace);
                foreach (int tick in WaitTeleportLevelUpUi(() => backend.State.IsComplete(),
                    "Human FCB choices complete")) yield return tick;
                presenter.Next();
                foreach (int tick in WaitTeleportLevelUpUi(() =>
                    presenter.CurrentPhase == CharBPhase.Type.Total,
                    "Human FCB summary")) yield return tick;
                var humanFinish = (UnityEngine.UI.Button)typeof(CharacterBuildController)
                    .GetField("m_CompleteButton", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(presenter);
                humanFinish.onClick.Invoke(); humanCommitted = true; backend = null;
                foreach (int tick in WaitTeleportLevelUpUi(() => !presenter.IsShow,
                    "Human FCB committed")) yield return tick;
                ui.LevelUpController = priorBackend;
                humanBook = humanUnit.Descriptor.GetSpellbook(oracle.Spellbook);
                var humanGrants = humanUnit.Descriptor.Progression.Features.Enumerable
                    .Where(value => ReferenceEquals(value.Blueprint, level6Feature)).ToArray();
                TeleportSpellbookUiAssert("fcb-human-committed",
                    "the shared Human route teaches canonical Recall once at Oracle 6",
                    "level=" + humanUnit.Descriptor.Progression.GetClassLevel(oracle) +
                        ";known6=" + humanBook.GetKnownSpells(6).Count(value =>
                            ReferenceEquals(value.Blueprint, recall)) +
                        ";grants=" + humanGrants.Length,
                    humanUnit.Descriptor.Progression.GetClassLevel(oracle) == 14 &&
                        humanBook.GetKnownSpells(6).Count(value =>
                            ReferenceEquals(value.Blueprint, recall)) == 1 &&
                        humanGrants.Length == 1 &&
                        humanGrants.Count(value => value.Param != null &&
                            value.Param.Value != null &&
                            ReferenceEquals(value.Param.Value.Blueprint, recall)) == 1);
            }
            finally
            {
                if (backend != null)
                {
                    if (presenter.IsShow &&
                        ReferenceEquals(presenter.LevelUpController, backend))
                        presenter.Show(false);
                    if (!aasimarCommitted && !humanCommitted &&
                        !ReferenceEquals(backend.Unit, null)) backend.Cancel();
                }
                ui.LevelUpController = priorBackend; presenter.Unit = priorPresenterUnit;
                foreach (var unit in new[] { aasimarUnit, humanUnit })
                {
                    if (unit == null) continue;
                    player.PartyCharacters.RemoveAll(value => value.UniqueId == unit.UniqueId);
                    if (unit.HoldingState != null &&
                        unit.HoldingState.AllEntityData.Contains(unit))
                        unit.HoldingState.RemoveEntityData(unit);
                    unit.Dispose();
                }
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                foreach (var entry in starterDeltas)
                    if (ReferenceEquals(entry.Key.Collection, player.Inventory) &&
                        entry.Key.Count >= entry.Value)
                        player.Inventory.Remove(entry.Key, entry.Value);
                game.IsPaused = originalPause;
                Application.logMessageReceived -= ObserveTeleportSpellbookUiException;
                bool restored = originalParty.SequenceEqual(player.Party) &&
                    originalPartyRefs.SequenceEqual(player.PartyCharacters) &&
                    originalCross.SequenceEqual(player.CrossSceneState.AllEntityData) &&
                    originalItems.SequenceEqual(player.Inventory.Items) &&
                    originalCounts.SequenceEqual(originalItems.Select(value => value.Count)) &&
                    player.Money == originalMoney && !_workingSaveSmoke.WriteObserved;
                CaptureTeleportSpellbookUi("fcb-cleanup", new { restored,
                    exceptions = _teleportationSpellbookUiExceptions.Count });
                TeleportSpellbookUiAssert("fcb-cleanup",
                    "party, cross-scene entities, inventory, money and save-write guard restored",
                    "restored=" + restored, restored);
                TeleportSpellbookUiAssert("fcb-exceptions",
                    "zero native or mod exceptions during the Favored Class fixture",
                    "count=" + _teleportationSpellbookUiExceptions.Count,
                    _teleportationSpellbookUiExceptions.Count == 0);
            }
        }

        // Opens the genuine favored-class bonus-spell award inside an active
        // native level-up: selects the completed bonus selection, its level-6
        // parametrized feature, and finally the canonical Word of Recall row
        // through the real presenter selector. Records the exact native
        // candidate list before any selection is made.
        private IEnumerable<int> OpenFcbAwardAndSelectRecall(
            BlueprintCharacterClass oracle, BlueprintFeatureSelection bonusSelection,
            BlueprintParametrizedFeature level6Feature,
            Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility recall, string caseId)
        {
            var presenter = Game.Instance.UI.CharacterBuildController;
            var backend = presenter.LevelUpController;
            var classState = backend.State.Selections.SingleOrDefault(value =>
                value.Selection is BlueprintFeatureSelection typed &&
                typed.AssetGuid == FcbOracleClassSelectionId);
            if (classState == null)
                throw new InvalidOperationException(
                    "The Oracle favored-class selection is not offered: " + caseId);
            var awardItems = classState.Selection.ExtractSelectionItems(
                backend.Unit, backend.Preview).ToArray();
            var awardItem = awardItems.FirstOrDefault(value => value.Feature != null &&
                value.Feature.AssetGuid == FcbOracleBonusSpellSelectionId);
            bool awardSelectable = awardItem != null && classState.Selection.CanSelect(
                backend.Preview, backend.State, classState, awardItem);
            var partialStillSelectable = awardItems.Any(value => value.Feature != null &&
                value.Feature.AssetGuid == FcbOraclePartialFeatureId &&
                classState.Selection.CanSelect(backend.Preview, backend.State, classState, value));
            CaptureTeleportSpellbookUi("fcb-award-state-" + caseId, new {
                candidates = awardItems.Select(value => value.Feature == null ? null :
                    value.Feature.AssetGuid + ":" + value.Feature.name).ToArray(),
                awardSelectable, partialStillSelectable,
                partialRank = backend.Preview.Progression.Features.Enumerable
                    .Where(value => value.Blueprint != null &&
                        value.Blueprint.AssetGuid == FcbOraclePartialFeatureId)
                    .Select(value => value.Rank).ToArray() });
            if (!awardSelectable)
                throw new InvalidOperationException(
                    "The completed bonus-spell award is not selectable: " + caseId);
            if (!backend.SelectFeature(classState, awardItem))
                throw new InvalidOperationException(
                    "Native bonus-spell award selection failed: " + caseId);
            var bonusState = backend.State.Selections.FirstOrDefault(value =>
                value.Selection is BlueprintFeatureSelection typed &&
                typed.AssetGuid == FcbOracleBonusSpellSelectionId && !value.Selected);
            if (bonusState == null)
                throw new InvalidOperationException(
                    "The bonus-spell selection state did not open: " + caseId);
            var levelItems = bonusState.Selection.ExtractSelectionItems(
                backend.Unit, backend.Preview).ToArray();
            var level6Item = levelItems.FirstOrDefault(value => value.Feature != null &&
                value.Feature.AssetGuid == FcbOracleLevel6FeatureId);
            bool level6Selectable = level6Item != null && bonusState.Selection.CanSelect(
                backend.Preview, backend.State, bonusState, level6Item);
            // The exact native candidate extraction for the sixth-level choice.
            var spellItems = level6Feature.ExtractSelectionItems(
                backend.Unit, backend.Preview).ToArray();
            int recallCandidates = spellItems.Count(value => value.Param != null &&
                value.Param.Value != null &&
                ReferenceEquals(value.Param.Value.Blueprint, recall));
            CaptureTeleportSpellbookUi("fcb-level6-candidates-" + caseId, new {
                levelItems = levelItems.Select(value => value.Feature == null ? null :
                    value.Feature.AssetGuid).ToArray(),
                level6Selectable,
                candidates = spellItems.Select(value => value.Param == null ||
                    value.Param.Value == null || value.Param.Value.Blueprint == null ? null :
                    value.Param.Value.Blueprint.AssetGuid + ":" +
                    value.Param.Value.Blueprint.name).ToArray(),
                recallCandidates,
                previewKnows = backend.Preview.GetSpellbook(oracle.Spellbook).IsKnown(recall) });
            TeleportSpellbookUiAssert("fcb-level6-candidates-" + caseId,
                "the genuine sixth-level favored-class choice offers canonical Word of Recall exactly once",
                "candidates=" + spellItems.Length + ";recall=" + recallCandidates +
                    ";level6Selectable=" + level6Selectable,
                recallCandidates == 1 && level6Selectable && spellItems.Length > 1 &&
                    !backend.Preview.GetSpellbook(oracle.Spellbook).IsKnown(recall));
            if (!backend.SelectFeature(bonusState, level6Item))
                throw new InvalidOperationException(
                    "Native sixth-level feature selection failed: " + caseId);
            var level6State = backend.State.Selections.FirstOrDefault(value =>
                ReferenceEquals(value.Selection, level6Feature) && !value.Selected);
            if (level6State == null)
                throw new InvalidOperationException(
                    "The sixth-level parametrized choice did not open: " + caseId);
            var recallItem = spellItems.Single(value => value.Param != null &&
                value.Param.Value != null &&
                ReferenceEquals(value.Param.Value.Blueprint, recall));
            if (!backend.SelectFeature(level6State, recallItem))
                throw new InvalidOperationException(
                    "Native Word of Recall parametrized pick failed: " + caseId);
            for (int frame = 0; frame < 8; frame++) yield return 0;
            TeleportSpellbookUiAssert("fcb-preview-only-" + caseId,
                "the native favored-class pick teaches Recall only in the isolated preview",
                "previewKnown=" + backend.Preview.GetSpellbook(oracle.Spellbook).IsKnown(recall) +
                    ";committedKnown=" + backend.Unit.GetSpellbook(oracle.Spellbook).IsKnown(recall),
                backend.Preview.GetSpellbook(oracle.Spellbook).IsKnown(recall) &&
                    !backend.Unit.GetSpellbook(oracle.Spellbook).IsKnown(recall));
        }

    }
}
