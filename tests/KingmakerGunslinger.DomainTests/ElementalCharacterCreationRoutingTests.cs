using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalCharacterCreationRoutingTests
    {
        private sealed class EqualObjects
        {
            public override bool Equals(object value) { return value is EqualObjects; }
            public override int GetHashCode() { return 7; }
        }
        internal static void ObservationIdentityUsesReferences()
        {
            var ids = new CharacterCreationObservationIdentity();
            var first = new EqualObjects();
            var second = new EqualObjects();
            Assertions.True(first.Equals(second), "Equality collision fixture is invalid.");
            Assertions.True(ids.Get(first) != ids.Get(second),
                "Equal objects must retain distinct observation references.");
            Assertions.Equal(ids.Get(first), ids.Get(first),
                "Repeated observation must retain its reference ID.");
            Assertions.Equal<string>(null, ids.Get(null), "Null must stay absent.");
        }
        internal static void ArraysDistinguishIdentityOrderAndNull()
        {
            var first = new EqualObjects();
            var second = new EqualObjects();
            var array = new[] { first, second };
            Assertions.True(CharacterCreationObservationIdentity.SameOrderedReferences(
                array, new[] { first, second }), "Cloned arrays must preserve ordered entry references.");
            Assertions.False(CharacterCreationObservationIdentity.SameOrderedReferences(
                array, new[] { second, first }), "A semantic-equality collision hid changed order.");
            Assertions.False(CharacterCreationObservationIdentity.SameOrderedReferences(
                array, new[] { new EqualObjects(), second }), "A new object replaced a foreign reference.");
            Assertions.False(CharacterCreationObservationIdentity.SameOrderedReferences<EqualObjects>(
                null, new EqualObjects[0]), "Null and empty are different contracts.");
            var ids = new CharacterCreationObservationIdentity();
            Assertions.True(ids.Get(array) != ids.Get(array.Clone()),
                "Array reference identity must be separate from ordered entry identity.");
        }
        internal static void ObserverCannotOperateCharacterCreator()
        {
            string path = Path.Combine(FindRoot(), "src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalCharacterCreationRoutingObserver.cs");
            string source = File.ReadAllText(path);
            foreach (string forbidden in new[] { ".SelectRace(", ".SelectClass(", ".SelectFeature(",
                ".SetupViewState(", ".StartWithoutAssigningStaticInstance(", ".SetPhase(",
                ".ToNextPhase(", ".Commit(", ".LoadGame(", ".SaveGame(", ".SetValue(" })
                Assertions.False(source.Contains(forbidden),
                    "Read-only observer must not operate a build: " + forbidden);
            Assertions.True(source.Contains("request.Scenario !=") &&
                source.Contains("RuntimeTestScenarioCatalog.ObserveElementalCharacterCreationRouting") &&
                source.Contains("if (_request == null"), "Observer requires an exact guarded scenario.");
            Assertions.True(source.Contains("NOT-RUN") && source.Contains("observationOnly") &&
                source.Contains("consumedByActualPhases") && source.Contains("existingViewEmpty") &&
                source.Contains("Progression.CharacterRaces") && source.Contains("published-race-selection-coverage") &&
                source.Contains("selectorLayers") && source.Contains("visibleChoiceCount") &&
                source.Contains("onChargenApply") && source.Contains("callbackInvoked"),
                "Actual routing and unavailable-state evidence must remain explicit.");
        }
        internal static void NativeCreatorFixtureIsScopedAndReportsAcceptanceSeparately()
        {
            string source = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalCharacterCreationBaselineScenario.cs"));
            foreach (string required in new[] { "request.Scenario != RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationBaseline",
                "new ChargenUnit(", ".HandleLevelUpStart(", "State.NextLevel != 1", "GetProperty(\"CurrentFeatureCollection\"", "GetComponentsInChildren<CharBuildSelectorItem>", "view.Feature.Feature",
                "_build.SetFeature(", "Actions.SelectAlignment(value)", "_build.Character.IsSelected()", "_build.SetRacialBonus(", "nativeOperationException", "nativeAlive", "ApplyNativeRoll", "read(\"Controller\")", "assigned.SequenceEqual(actual)", "ObserveInnerAssetUnload", "VerifyVisualIntegrity", "CaptureInitialInnerAssets", "global.AutoCommit", "ReferenceEquals(global.Preview, global.Unit)",
                "ReferenceEquals(_build.Unit, global.Unit)", "global.Doll == null", "!_build.WarmUp", "provenIdleAutomaticController", "global.LevelUpActions.Count == 0",
                "ReferenceEquals(Game.Instance.UI.LevelUpController, _globalControllerBefore)", "_build.SpendSkillPoint(stat, false)",
                "after != before - 1", "refund-owned-skill", "sharedAssetsAliveAfter", "NativeDependencyIds", "m_InitiallyLoadedEquipmentEntityInnerAssets", "_build.BuyAttribute(", "_build.SpendSkillPoint(", "_controller.State.IsComplete()", "NextEnabled()", "CommitOwnedCreator()",
                "ReferenceEquals(_controller.Unit, _unit.Descriptor)", "ArmSaveGuard()", "DisarmSaveGuard()",
                "BlockSaveRoutine", "BlockSaveMutation", "loaded.DescriptorReferenceCorrelated", "loaded.StableFingerprint",
                "ReferenceEquals(Game.Instance.Player.MainCharacter.Value, _mainBefore)", "_areaBefore == null", "global != null && !ReferenceEquals(global, _controller)",
                "visible != null && !ReferenceEquals(visible, _controller)", "(!_committed && (global == null || visible == null))", "acceptanceFailures", "baseline observation PASS does not qualify" })
                Assertions.True(source.Contains(required), "Native creator contract is absent: " + required);
            foreach (string forbidden in new[] { "StartWithoutAssigningStaticInstance", ".AddSelection(",
                ".SaveGame(", ".LoadGame(", "_controller.AddStatPoint(", "_controller.SpendSkillPoint(", "KMG_AUTOMATION_BASELINE", "Obligatory =", "IgnorePrerequisites =", "_globalControllerBefore.Cancel(", "_globalControllerBefore.Commit(", "SkillPointsRemaining =", "SpentSkillPoints =" })
                Assertions.False(source.Contains(forbidden), "Fixture bypasses native selection or save scope: " + forbidden);
        }
        internal static void DisabledControlCannotBootstrapProductionOrCommitCampaign()
        {
            string source = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger",
                "RuntimeTesting", "DisabledKmgCharacterCreationProbe.cs"));
            foreach (string required in new[] { "entry.Info.Id != ProbeId", "RuntimeTestRequestParser.TryActivate",
                "FileMode.CreateNew", "EnsureProductionDisabled", "entry.Loaded || entry.Active", "ModContext.TryGet",
                "BlueprintBootstrap.Library != null", "loader != \"NotStarted\"", "patch.owner == \"KingmakerGunslinger\"",
                "ArmDisabledControl", "VerifyUnmodifiedLibrary", "RuntimeTestResultWriter.Write(result" })
                Assertions.True(source.Contains(required), "Disabled control proof is missing: " + required);
            foreach (string forbidden in new[] { "Main.Load(", "ModContext.Publish(", ".InstallPatches(", ".PatchAll(",
                "BlueprintBootstrap.TryInitialize", ".SetValue(", ".LoadGame(", ".SaveGame(" })
                Assertions.False(source.Contains(forbidden), "Profile B must leave production disabled: " + forbidden);
        }
        internal static void HeritageFactoryUsesObservedNativeHeritageRoute()
        {
            string source = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger",
                "ElementalRaces", "ElementalHeritageBlueprintFactory.cs"));
            int start = source.IndexOf("private static BlueprintFeatureSelection CreateSelection(", StringComparison.Ordinal);
            int end = source.IndexOf("private static void Validate(", start, StringComparison.Ordinal);
            string factory = source.Substring(start, end - start);
            // Native 2.1.7b DefineAvailibleData routes group 42 to Determinator;
            // groups 0 and 11 both route to Abilities. This pins that inspected contract.
            Assertions.True(factory.Contains("result.Group = FeatureGroup.AasimarHeritage;") &&
                factory.Contains("result.Group2 = FeatureGroup.None;") &&
                factory.Contains("result.Groups = new[] { FeatureGroup.Racial };"),
                "Heritage must match the native Aasimar/Races Unleashed racial route.");
            Assertions.False(factory.Contains("result.Group = FeatureGroup.None;") ||
                factory.Contains("result.Group = FeatureGroup.Racial;"),
                "Generic Abilities routing cannot qualify as Heritage.");
            Assertions.True(factory.Contains("result.Obligatory = true;") &&
                factory.Contains("result.IgnorePrerequisites = false;") &&
                factory.Contains("ElementalHeritagePolicy.ChoicesPerRace") &&
                factory.Contains("ElementalHeritageSelectionController"),
                "Moving the phase must retain the existing three-choice selection lifecycle.");
        }

        internal static void AlternateTraitFactoryUsesRacialRouteAndPublishedChoices()
        {
            string source = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger",
                "ElementalRaces", "ElementalAlternateTraitBlueprintFactory.cs"));
            Assertions.True(source.Contains("result.Group = FeatureGroup.AasimarHeritage;") &&
                source.Contains("result.Group2 = FeatureGroup.None;") &&
                source.Contains("result.Groups = new[] { FeatureGroup.AasimarHeritage };") &&
                source.Contains(".PublishedChoices.Select(") && source.Contains("definition.PublishedChoices.Count + 1"),
                "Alternate traits require the installed racial presentation contract and retain plus published choices.");
            Assertions.False(source.Contains("result.Group = FeatureGroup.None;") ||
                source.Contains("result.Group = FeatureGroup.Racial;"), "Racial traits cannot enter generic Abilities.");
            string runtime = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalAlternateTraitFrameworkScenario.cs"));
            Assertions.True(runtime.Contains("e117e1e0a17a4acec001000000000031") &&
                runtime.Contains("e117e1e0a17a4acec001000000000040") &&
                runtime.Contains("HasTraitSpecificMechanic(trait)") && runtime.Contains("leaking.Length == 0"),
                "Live qualification must reject both exact no-op GUIDs in every player-facing selection array.");
        }

        internal static void NativeRevisionPlansCoverLegalTransitionsAndAllHeritages()
        {
            var routes = Enumerable.Range(0, 3).Select(ElementalCharacterCreationRegressionPlan.Route).ToArray();
            Assertions.True(routes.Select(route => route.Last()).OrderBy(value => value).SequenceEqual(new[] { 0, 1, 2 }),
                "All three heritages must reach actual final commits.");
            Assertions.True(routes.Any(route => route.SequenceEqual(new[] { 0, 1, 0 })) &&
                routes.Any(route => route.SequenceEqual(new[] { 1, 2 })), "Back-navigation must cover retain/alternate and alternate/alternate transitions.");
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                foreach (int choice in Enumerable.Range(0, 3))
                {
                    var traits = ElementalCharacterCreationRegressionPlan.Traits(race, choice);
                    var heritage = ElementalHeritagePolicy.Ordered().Where(value => value.ParentRace == race).ToArray()[choice];
                    var state = ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits);
                    Assertions.Equal(traits.Length, state.TraitProviderSymbols().Length, "Fixture plan silently discarded a trait.");
                    Assertions.True(traits.All(ElementalAlternateTraitPolicy.IsPublished), "Fixture plan includes a deferred no-op.");
                    if (choice == 0) Assertions.Equal(0, traits.Length, "General route must exercise unconditional retain choices.");
                }
            Assertions.False(ElementalCharacterCreationRegressionPlan.IsAllowedCase("Human", "Fighter", "roll"), "Foreign race is outside regression scope.");
            Assertions.False(ElementalCharacterCreationRegressionPlan.IsAllowedCase("Ifrit", "Wizard", "roll"), "Spellbook fixture is not qualified here.");
            Assertions.False(ElementalCharacterCreationRegressionPlan.IsAllowedCase("Ifrit", "Fighter", "guess"), "Unknown allocation must fail closed.");
            string source = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger", "RuntimeTesting", "ElementalCharacterCreationRegression.cs"));
            foreach (string required in new[] { "_build.Back()", "tabs[0].Toggle.isOn = true", "RenderedNativeAction(renderedItems[0]).Invoke()", "MatchesNativeSelectionIdentity", "value.Index == view.Index",
                "ExactOwnedSelection", "RejectDeferredChoices", "allocationBaselineExact", "VerifyCurrentRollOwner", "native-racial-reselection",
                "owner.Progression.Features.Enumerable.Count", "owner.Resources.PersistantResources.Count", "expectedOverlay", "_revisionChoices.Enqueue",
                "CaptureCreatorMembership", "CreatorMembershipRestored", "CleanupCreatorItems();", "_commitCountsBefore.SequenceEqual",
                "inventory.Remove(item, excess).Dispose()", "_commitMoneyRestored ? _commitMoneyBefore : _commitMoneyAfter", "player.GainMoney(_commitMoneyBefore - _commitMoneyAfter)", "PollCommittedCreatorCleanup", "_commitRegistrationWait > 90",
                "_revisionViewWait < 90", "native-revision-choice-readiness", "_playerInventoryCountsBefore.SequenceEqual", "ReferenceEquals(player.RemoteCompanions[index].Value, _unit)",
                "committedBases.SequenceEqual(_allocatedBases)", "_playerMoneyBefore == player.Money" })
                Assertions.True(source.Contains(required), "Native roundtrip boundary absent: " + required);
            foreach (string forbidden in new[] { ".AddFact(", ".RemoveFact(", ".AddSelection(", ".Reconcile(",
                "BaseValue =", "SkillPointsRemaining =", "StatsDistribution.Points =", ".SaveGame(", ".LoadGame(" })
                Assertions.False(source.Contains(forbidden), "Regression fixture repairs the state it must observe: " + forbidden);
        }

        internal static void FullNativeRespecPlanPreservesOriginalAndSpentIdentities()
        {
            var route = Enumerable.Range(0, ElementalCharacterCreationRegressionPlan.NativeRespecVisits)
                .Select(ElementalCharacterCreationRegressionPlan.NativeRespecChoice).ToArray();
            Assertions.True(route.SequenceEqual(new[] { 0, 0, 1, 2, 0, 2, 1, 0 }),
                "Respec must cover unchanged spent identity and all directional heritage/trait transitions.");
            foreach (string race in new[] { "Ifrit", "Oread", "Sylph", "Undine" })
                Assertions.True(ElementalCharacterCreationRegressionPlan.IsAllowedRespecCase(race, "Fighter", "point-buy"), "Missing native respec race.");
            Assertions.False(ElementalCharacterCreationRegressionPlan.IsAllowedRespecCase("Human", "Fighter", "point-buy"), "Unowned race accepted.");
            Assertions.False(ElementalCharacterCreationRegressionPlan.IsAllowedRespecCase("Ifrit", "Gunslinger", "point-buy"), "Unqualified respec class accepted.");
            Assertions.False(ElementalCharacterCreationRegressionPlan.IsAllowedRespecCase("Ifrit", "Fighter", "roll"), "Unqualified respec allocation accepted.");
            string source = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger", "RuntimeTesting", "ElementalCharacterCreationNativeRespec.cs"));
            foreach (string required in new[] { "Game.Instance.Player.RespecCompanion(_respecOriginal", "_respecOriginal.UniqueId != _respecOriginalId",
                "_respecReplacements.Add(_unit)", "_respecOriginal.Descriptor.Resources.Spend(resource, before)", "_respecSpent[resource.AssetGuid] = after",
                "CloseOwnedCreatorController()", "CleanupCreatorItems()", "committedOwnedResources", "_respecCallbacks != 1", "nativeRespecPreviewMismatches",
                "if (_crossSceneBefore != null) Game.Instance.IsPaused = _creatorPauseBefore", "_respecBodies.TryGetValue(replacement, out body)", "body.Items.Any()" })
                Assertions.True(source.Contains(required), "Missing native respec boundary: " + required);
            foreach (string forbidden in new[] { ".AddFact(", ".RemoveFact(", ".Reconcile(", ".Remember(", "BaseValue =", ".SaveGame(", ".LoadGame(" })
                Assertions.False(source.Contains(forbidden), "The fixture repairs the native state it must observe: " + forbidden);
        }

        internal static void RespecCapturePreservesOnlyObservedOwnedIdentities()
        {
            var saved = new Dictionary<string, int> { ["active"] = 1, ["suppressed"] = 0, ["foreign"] = 0 };
            var live = new Dictionary<string, int> { ["active"] = 0, ["foreign"] = 3 };
            var result = ElementalRespecResourcePolicy.Capture(new[] { "active", "suppressed", "unseen" }, saved, live);
            Assertions.True(result.Count == 2 && result["active"] == 0 && result["suppressed"] == 0,
                "Live expenditure and suppressed expenditure must survive without inventing an unseen daily budget.");
            Assertions.True(saved["active"] == 1 && live["foreign"] == 3, "Capture changed source state.");
            saved["suppressed"] = 1;
            Assertions.True(result["suppressed"] == 0 && !result.ContainsKey("foreign"), "Snapshot aliases source or includes a foreign resource.");
        }

        internal static void RespecPreservationNeverRefillsOrExceedsCurrentCapacity()
        {
            for (int current = 0; current <= 10; ++current)
                for (int remembered = 0; remembered <= 10; ++remembered)
                    Assertions.True(ElementalRespecResourcePolicy.Amount(current, remembered) == Math.Min(current, remembered),
                        "Respec may only remove a refill up to the previously observed exact identity budget.");
            Assertions.True(ElementalRespecResourcePolicy.Amount(1, -1) == 0, "Malformed negative expenditure was not conservative.");
            string source = File.ReadAllText(Path.Combine(FindRoot(), "src", "KingmakerGunslinger", "ElementalRaces", "ElementalNativeRespecResourceRuntime.cs"));
            foreach (string required in new[] { "ConditionalWeakTable<UnitDescriptor, Snapshot>", "LevelUpState.CharBuildMode.Respec",
                "ReferenceEquals(target, Original.Descriptor)", "snapshot.Preserve(original.Descriptor, true)", "SetupNewCharacher", "RequestPreview", "Forget(snapshot)" })
                Assertions.True(source.Contains(required), "Missing respec owner/lifetime contract: " + required);
            foreach (string forbidden in new[] { ".AddFact(", ".RemoveFact(", ".Reconcile(", "BaseValue =", ".Restore(" })
                Assertions.False(source.Contains(forbidden), "Resource bridge changed an unrelated native subsystem: " + forbidden);
        }

        private static string FindRoot()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                directory = directory.Parent;
            if (directory == null) throw new InvalidOperationException("Repository root unavailable.");
            return directory.FullName;
        }
    }
}
