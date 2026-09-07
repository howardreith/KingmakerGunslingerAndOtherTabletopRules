using System;
using System.IO;
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
                "_build.SetFeature(", "Actions.SelectAlignment(value)", "_build.Character.IsSelected()", "_build.SetRacialBonus(", "nativeOperationException", "nativeAlive", "ApplyNativeRoll", "read(\"Controller\")", "assigned.SequenceEqual(actual)", "ObserveInnerAssetUnload", "VerifyVisualIntegrity", "CaptureInitialInnerAssets", "sharedAssetsAliveAfter", "NativeDependencyIds", "m_InitiallyLoadedEquipmentEntityInnerAssets", "_build.BuyAttribute(", "_build.SpendSkillPoint(", "_controller.State.IsComplete()", "NextEnabled()", "_build.Commit()",
                "ReferenceEquals(_controller.Unit, _unit.Descriptor)", "ArmSaveGuard()", "DisarmSaveGuard()",
                "BlockSaveRoutine", "BlockSaveMutation", "loaded.DescriptorReferenceCorrelated", "loaded.StableFingerprint",
                "ReferenceEquals(Game.Instance.Player.MainCharacter.Value, _mainBefore)", "_areaBefore == null", "global != null && !ReferenceEquals(global, _controller)",
                "visible != null && !ReferenceEquals(visible, _controller)", "(!_committed && (global == null || visible == null))", "acceptanceFailures", "baseline observation PASS does not qualify" })
                Assertions.True(source.Contains(required), "Native creator contract is absent: " + required);
            foreach (string forbidden in new[] { "StartWithoutAssigningStaticInstance", ".AddSelection(",
                ".SaveGame(", ".LoadGame(", "_controller.AddStatPoint(", "_controller.SpendSkillPoint(", "KMG_AUTOMATION_BASELINE", "Obligatory =", "IgnorePrerequisites =" })
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
