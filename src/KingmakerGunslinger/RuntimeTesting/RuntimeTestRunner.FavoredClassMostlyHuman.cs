using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Hooks;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbRogueClassGuid = "299aa766dee3cbf4790da4efb8c72484";
        private const string FcbHoldPersonGuid = "c7104f7526c4c524f91474614054547e";
        private const string FcbOutsiderTypeGuid = "9054d3988d491d944ac144e27b6bc318";
        private const string FcbAuspiciousTattooGuid = "ed4e37343e774304af0d286b9454bcdb";
        private const string FcbCharmPersonGuid = "1af9d5995090e5a4185a30decf0959ad";
        private const string FcbEnlargePersonGuid = "c60969e7f264e6d4b84a1499fdcf9039";
        private const string FcbReducePersonGuid = "4e0e9aba6447d514f88eff1464cc4763";
        // Races Unleashed's Suli and its own "Mostly Human" (an unverified
        // provider: it only removes OutsiderType), which must fail closed.
        private const string FcbSuliRaceGuid = "f78db38a553f4f91a10a8e68c91019ad";
        private const string FcbSuliMostlyHumanGuid = "98b6fbb937b8456bb588d3aae4a0d6a6";

        // Phase 5: the four-race Mostly Human companion trait (E05-E08) and
        // the scoped host ancestry bridge, through native character creation.
        private RuntimeTestResult RunFavoredClassMostlyHuman()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            ElementalMostlyHumanBlueprintSet mostlyHuman = BlueprintBootstrap.MostlyHuman;
            ElementalRaceBlueprintSet races = BlueprintBootstrap.ElementalRaces;
            bool integration = FavoredClassRuntime.Profile.IntegrationEnabled;
            FavoredClassBridgeScope bridgeScope = FavoredClassHostRaceBridge.Scope;
            // The racial trait works with the favored-class integration off;
            // only the KMG favored-class leaves depend on the integration.
            bool ready = (integration ? status.Availability == FavoredClassIntegrationAvailability.Published :
                    status.Availability == FavoredClassIntegrationAvailability.IntegrationDisabled) &&
                host != null && host.Decision.IsReady && leaves != null && mostlyHuman != null && races != null &&
                FavoredClassRuntime.Profile.MostlyHuman && bridgeScope != null;
            assertions.Add(Assertion("fcb-mostly-human-ready",
                "the exact host is ready, the Mostly Human trait is registered with its control on, and its ancestry bridge is scoped (with the favored-class integration on or off)",
                status + ";mostlyHuman=" + (mostlyHuman != null) + ";profile=" + FavoredClassRuntime.Profile +
                    ";bridge=" + (bridgeScope == null ? "none" : bridgeScope.FavoredClassLeafPrerequisites + "+" +
                        bridgeScope.OtherHumanPrerequisites.Count),
                ready, "FavoredClassIntegrationStatusRegistry, FavoredClassHostRaceBridge.Scope, FavoredClassRuntime.Profile"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var evidence = new JObject();
            var graphFailures = new List<string>();
            var identityFailures = new List<string>();
            var bridgeFailures = new List<string>();
            var menuFailures = new List<string>();
            var heritageFailures = new List<string>();
            var committed = new List<UnitEntityData>();
            bool cleaned = false;
            try
            {
                evidence["graph"] = DescribeMostlyHumanGraph(mostlyHuman, races, graphFailures);
                var context = new FcbMostlyHumanContext(host, leaves, mostlyHuman, races);
                evidence["identity"] = RunMostlyHumanIdentity(context, committed, identityFailures,
                    bridgeFailures, evidence);
                evidence["gunslingerMenus"] = RunMostlyHumanGunslingerMenus(context, menuFailures, integration);
                evidence["heritages"] = RunMostlyHumanHeritages(context, heritageFailures, integration);
            }
            catch (Exception exception)
            {
                graphFailures.Add("exception=" + exception);
            }
            finally
            {
                foreach (UnitEntityData unit in committed)
                    try { unit.Dispose(); } catch (Exception) { }
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-mostly-human.json", evidence);
            assertions.Add(Assertion("fcb-mostly-human-graph",
                "each parent race lists exactly one Mostly Human selection (standard entry first, then the trait), the shared identity is hidden and configured, and all 13 identities resolve",
                Describe(evidence["graph"], graphFailures), graphFailures.Count == 0,
                "live race Features, selection AllFeatures, BlueprintsByAssetId"));
            assertions.Add(Assertion("fcb-mostly-human-dual-identity",
                "a committed Mostly Human geniekin differs from its standard control only by the trait, the hidden identity and at most one of the human race traits that identity opens (the control has none) (same race and RaceId, native geniekin traits and scores, no OutsiderType removed or added, no HumanRace fact); native Hold, Charm, Enlarge and Reduce Person can target it (humanoid), and the same checks reject a unit that carries OutsiderType",
                Describe(evidence["identity"], identityFailures), identityFailures.Count == 0,
                "native chargen visits committed with LevelUpController.ApplyLevelup; Progression.Features; IAbilityTargetChecker"));
            assertions.Add(Assertion("fcb-mostly-human-host-bridge",
                "every host race-related Human prerequisite (its human favored-class leaves and its human race traits) opens to a Mostly Human geniekin and stays closed to its standard control, to a human-looking standard geniekin and to Races Unleashed's own Suli Mostly Human; Human keeps both, Half-elf and Aasimar keep the host's favored-class policy only, a Dwarf gets none; one reward selection per level",
                Describe(evidence["bridge"], bridgeFailures), bridgeFailures.Count == 0,
                "level-1 native Rogue visits; BlueprintFeatureSelection.CanSelect; Prerequisite.Check"));
            assertions.Add(Assertion("fcb-mostly-human-gunslinger-menus",
                "with the integration on, a Mostly Human geniekin Gunslinger is offered the human grit counter in addition to its native counters and its standard control is not; with the integration off, no KMG favored-class leaf is offered to either",
                Describe(evidence["gunslingerMenus"], menuFailures), menuFailures.Count == 0,
                "level-1 native Gunslinger visits; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-mostly-human-heritages",
                "all twelve heritages keep their parent race's favored-class routes (Ifrit Initiative, Oread bull rush, Undine grapple/stunning, Sylph none) and gain no human access without the trait",
                Describe(evidence["heritages"], heritageFailures), heritageFailures.Count == 0,
                "level-1 native visits with the exact heritage marker selected"));
            assertions.Add(Assertion("external-isolation", "unchanged party and global-unit snapshots",
                "cleaned=" + cleaned, cleaned, "detached entity disposal and exact reference snapshots"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private sealed class FcbMostlyHumanContext
        {
            internal FcbMostlyHumanContext(FavoredClassHostHandles host, FavoredClassBlueprintSet leaves,
                ElementalMostlyHumanBlueprintSet mostlyHuman, ElementalRaceBlueprintSet races)
            {
                Host = host;
                Leaves = leaves;
                MostlyHuman = mostlyHuman;
                Races = races;
            }

            internal FavoredClassHostHandles Host { get; private set; }
            internal FavoredClassBlueprintSet Leaves { get; private set; }
            internal ElementalMostlyHumanBlueprintSet MostlyHuman { get; private set; }
            internal ElementalRaceBlueprintSet Races { get; private set; }

            internal ElementalRaceBlueprints Parent(ElementalHeritageRace race)
            {
                return Races.OrderedBlueprints().Single(value => (int)value.Definition.Kind == (int)race);
            }
        }

        private static JObject DescribeMostlyHumanGraph(ElementalMostlyHumanBlueprintSet set,
            ElementalRaceBlueprintSet races, IList<string> failures)
        {
            var result = new JObject();
            foreach (ElementalMostlyHumanRaceBlueprints race in set.Races)
            {
                int occurrences = (race.Race.Features ?? new BlueprintFeatureBase[0])
                    .Count(value => ReferenceEquals(value, race.Selection));
                BlueprintFeature[] items = race.Selection.AllFeatures ?? new BlueprintFeature[0];
                result[race.Definition.RaceName] = new JObject
                {
                    ["selection"] = race.Selection.AssetGuid,
                    ["occurrences"] = occurrences,
                    ["items"] = new JArray(items.Select(value => value.name)),
                    ["obligatory"] = race.Selection.Obligatory,
                    ["group"] = race.Selection.Group.ToString()
                };
                if (occurrences != 1)
                    failures.Add(race.Definition.RaceName + " lists the selection " + occurrences + " times");
                if (!items.SequenceEqual(new[] { race.Standard, race.Trait }))
                    failures.Add(race.Definition.RaceName + " selection items are not [standard, trait]");
            }
            int resolved = ElementalMostlyHumanPolicy.All.Count(identity =>
            {
                BlueprintScriptableObject blueprint;
                return BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(identity.Guid, out blueprint) &&
                    blueprint != null && blueprint.GetType().Name == identity.PlannedType;
            });
            result["resolvedIdentities"] = resolved + "/" + ElementalMostlyHumanPolicy.IdentityCount;
            result["identityHidden"] = set.Identity.HideInUI;
            if (resolved != ElementalMostlyHumanPolicy.IdentityCount)
                failures.Add("only " + resolved + " Mostly Human identities resolve");
            if (!set.Identity.HideInUI)
                failures.Add("the shared identity is visible");
            result["bridgeTrackedHumanPrerequisites"] = FavoredClassHostRaceBridge.TrackedCount;
            if (FavoredClassHostRaceBridge.TrackedCount == 0)
                failures.Add("the ancestry bridge tracks no host human prerequisite");
            return result;
        }

        /// <summary>One native level-1 visit; the controller is returned open.</summary>
        private LevelUpController OpenMostlyHumanVisit(UnitEntityData unit, BlueprintRace race,
            BlueprintCharacterClass characterClass, ElementalMostlyHumanRaceBlueprints ancestry,
            BlueprintFeature ancestryChoice, ElementalHeritageRaceBlueprints heritages,
            BlueprintFeature heritage, JObject row)
        {
            LevelUpController controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race,
                characterClass, "KMG FCB Mostly Human");
            if (heritage != null)
            {
                FeatureSelectionState heritageState = FavoredClassLevelUpHarness.FindOpenState(controller,
                    heritages.Selection.AssetGuid);
                row["heritageSelected"] = heritageState != null &&
                    FavoredClassLevelUpHarness.Select(controller, heritageState, heritage);
            }
            if (ancestry != null)
            {
                FeatureSelectionState ancestryState = FavoredClassLevelUpHarness.FindOpenState(controller,
                    ancestry.Selection.AssetGuid);
                row["mostlyHumanSelectionOpen"] = ancestryState != null;
                if (ancestryState != null)
                {
                    row["standardSelectable"] = FavoredClassLevelUpHarness.CanSelect(controller,
                        ancestryState, ancestry.Standard);
                    row["traitSelectable"] = FavoredClassLevelUpHarness.CanSelect(controller,
                        ancestryState, ancestry.Trait);
                    row["ancestry"] = ancestryChoice.name;
                    row["ancestrySelected"] = FavoredClassLevelUpHarness.Select(controller, ancestryState,
                        ancestryChoice);
                }
            }
            return controller;
        }

        private static IList<BlueprintFeature> HostHumanLeaves(BlueprintFeatureSelection selection)
        {
            return (selection.AllFeatures ?? new BlueprintFeature[0]).Where(leaf => leaf != null &&
                (leaf.ComponentsArray ?? new BlueprintComponent[0]).Any(FavoredClassHostRaceBridge.IsTracked))
                .ToArray();
        }

        private static int OpenHostBonusStates(LevelUpController controller, FavoredClassHostHandles host)
        {
            var hostSelections = new HashSet<string>(host.BonusSelections.Where(value => value.Value != null)
                .Select(value => value.Value.AssetGuid), StringComparer.Ordinal);
            return controller.State.Selections.Count(value => !value.Selected &&
                value.Selection is BlueprintScriptableObject &&
                hostSelections.Contains(((BlueprintScriptableObject)value.Selection).AssetGuid));
        }

        private JArray RunMostlyHumanIdentity(FcbMostlyHumanContext context, List<UnitEntityData> committed,
            IList<string> identityFailures, IList<string> bridgeFailures, JObject evidence)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintCharacterClass rogue = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                library, FcbRogueClassGuid, "Rogue");
            BlueprintFeatureSelection rogueBonus = context.Host.BonusSelectionFor(rogue.AssetGuid);
            IList<BlueprintFeature> humanLeaves = HostHumanLeaves(rogueBonus);
            BlueprintRace humanRace = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.Human).RaceGuid, "Human");
            BlueprintFeature outsider = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbOutsiderTypeGuid, "OutsiderType");
            BlueprintAbility hold = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                FcbHoldPersonGuid, "Hold Person");
            IAbilityTargetChecker[] personCheckers = (hold.ComponentsArray ?? new BlueprintComponent[0])
                .OfType<IAbilityTargetChecker>().ToArray();
            // Every host human race trait prerequisite in the bridge scope.
            FavoredClassBridgeScope scope = FavoredClassHostRaceBridge.Scope;
            var traitChecks = new List<KeyValuePair<string, Prerequisite>>();
            foreach (string owner in scope == null ? new string[0] : scope.OtherHumanPrerequisites.ToArray())
            {
                BlueprintScriptableObject traitBlueprint;
                string guid = owner.Substring(owner.LastIndexOf(':') + 1);
                library.BlueprintsByAssetId.TryGetValue(guid, out traitBlueprint);
                foreach (Prerequisite check in (traitBlueprint == null ? new BlueprintComponent[0] :
                    traitBlueprint.ComponentsArray ?? new BlueprintComponent[0]).OfType<Prerequisite>())
                    if (FavoredClassHostRaceBridge.IsTracked(check))
                        traitChecks.Add(new KeyValuePair<string, Prerequisite>(traitBlueprint.name, check));
            }
            Func<UnitDescriptor, LevelUpState, JObject> humanTraits = (unit, levelState) =>
            {
                var open = new JObject();
                foreach (KeyValuePair<string, Prerequisite> check in traitChecks)
                    open[check.Key] = check.Value.Check(null, unit, levelState);
                return open;
            };
            Func<JObject, bool> allOpen = open => open.Count > 0 && open.Properties().All(value => (bool)value.Value);
            Func<JObject, bool> allClosed = open => open.Properties().All(value => !(bool)value.Value);
            var personSpells = new[] { FcbHoldPersonGuid, FcbCharmPersonGuid, FcbEnlargePersonGuid,
                FcbReducePersonGuid }.Select(guid => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    library, guid, guid)).ToArray();
            IAbilityTargetChecker[] allPersonCheckers = personSpells.SelectMany(spell =>
                (spell.ComponentsArray ?? new BlueprintComponent[0]).OfType<IAbilityTargetChecker>()).ToArray();
            var bridge = new JObject
            {
                ["rogueHumanLeaves"] = new JArray(humanLeaves.Select(value => value.name)),
                ["hostHumanRaceTraits"] = new JArray(traitChecks.Select(value => value.Key).Distinct()),
                ["scopeFavoredClassLeafPrerequisites"] = scope == null ? 0 : scope.FavoredClassLeafPrerequisites,
                ["scopeOtherHumanPrerequisites"] = scope == null ? new JArray() :
                    new JArray(scope.OtherHumanPrerequisites.ToArray()),
                ["personSpellCheckers"] = allPersonCheckers.Length
            };
            evidence["bridge"] = bridge;
            if (humanLeaves.Count == 0)
                bridgeFailures.Add("the host Rogue selection has no tracked human leaf");
            if (traitChecks.Count == 0)
                bridgeFailures.Add("no host human race trait prerequisite is in the bridge scope");
            if (personCheckers.Length == 0 || allPersonCheckers.Length < personSpells.Length)
                identityFailures.Add("a native person spell exposes no target checker");

            // Rogue visits: controls first, then Standard/Mostly Human per parent.
            var controls = new[] { FavoredClassAncestry.Human, FavoredClassAncestry.HalfElf,
                FavoredClassAncestry.Aasimar, FavoredClassAncestry.Dwarf };
            var rows = new JArray();
            var controlRows = new JArray();
            foreach (string ancestry in controls)
            {
                var row = new JObject { ["race"] = ancestry };
                UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                LevelUpController controller = null;
                try
                {
                    BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                        FavoredClassRaceIdentities.ForAncestry(ancestry).RaceGuid, ancestry);
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, rogue, "KMG FCB MH Control");
                    FavoredClassLevelUpHarness.ChooseFavoredClass(controller, rogue, row);
                    FavoredClassLevelUpHarness.FillOthers(controller,
                        new HashSet<string>(StringComparer.Ordinal) { rogueBonus.AssetGuid });
                    FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                        rogueBonus.AssetGuid);
                    bool offered = fcb != null && humanLeaves.Any(leaf =>
                        FavoredClassLevelUpHarness.CanSelect(controller, fcb, leaf));
                    row["humanLeafOffered"] = offered;
                    bool expected = ancestry != FavoredClassAncestry.Dwarf;
                    if (offered != expected)
                        bridgeFailures.Add(ancestry + " host human access " + offered + " expected " + expected);
                    // The host's human race traits are Human-only (its own
                    // favored-class policy for Half-elf and Aasimar is separate).
                    JObject traits = humanTraits(controller.Preview, controller.State);
                    row["humanRaceTraits"] = traits;
                    bool traitsExpected = ancestry == FavoredClassAncestry.Human;
                    if (traitsExpected ? !allOpen(traits) : !allClosed(traits))
                        bridgeFailures.Add(ancestry + " human race traits " + traits.ToString(
                            Newtonsoft.Json.Formatting.None) + " expected open=" + traitsExpected);
                }
                catch (Exception exception)
                {
                    bridgeFailures.Add(ancestry + ": " + exception.GetType().Name + ": " + exception.Message);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                    unit.Dispose();
                }
                controlRows.Add(row);
            }
            bridge["controls"] = controlRows;

            foreach (ElementalMostlyHumanRaceBlueprints ancestry in context.MostlyHuman.Races)
            {
                var pair = new JObject { ["race"] = ancestry.Definition.RaceName };
                var committedUnits = new Dictionary<string, UnitEntityData>(StringComparer.Ordinal);
                foreach (BlueprintFeature choice in new[] { ancestry.Standard, ancestry.Trait })
                {
                    bool isTrait = ReferenceEquals(choice, ancestry.Trait);
                    var row = new JObject();
                    UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                    LevelUpController controller = null;
                    bool keep = false;
                    try
                    {
                        controller = OpenMostlyHumanVisit(unit, ancestry.Race, rogue, ancestry, choice,
                            null, null, row);
                        FavoredClassLevelUpHarness.ChooseFavoredClass(controller, rogue, row);
                        FavoredClassLevelUpHarness.FillOthers(controller,
                            new HashSet<string>(StringComparer.Ordinal) { rogueBonus.AssetGuid });
                        FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                            rogueBonus.AssetGuid);
                        bool offered = fcb != null && humanLeaves.Any(leaf =>
                            FavoredClassLevelUpHarness.CanSelect(controller, fcb, leaf));
                        row["humanLeafOffered"] = offered;
                        row["openRewardSelections"] = OpenHostBonusStates(controller, context.Host);
                        if (offered != isTrait)
                            bridgeFailures.Add(ancestry.Definition.RaceName + (isTrait ? " Mostly Human" :
                                " standard") + " host human access " + offered);
                        if ((int)row["openRewardSelections"] != 1)
                            bridgeFailures.Add(ancestry.Definition.RaceName + " open reward selections " +
                                row["openRewardSelections"]);
                        JObject traits = humanTraits(controller.Preview, controller.State);
                        row["humanRaceTraits"] = traits;
                        if (isTrait ? !allOpen(traits) : !allClosed(traits))
                            bridgeFailures.Add(ancestry.Definition.RaceName + (isTrait ? " Mostly Human" :
                                " standard") + " human race traits " + traits.ToString(Newtonsoft.Json.Formatting.None));
                        if (!(row["mostlyHumanSelectionOpen"] != null && (bool)row["mostlyHumanSelectionOpen"] &&
                            (bool)row["standardSelectable"] && (bool)row["traitSelectable"] &&
                            (bool)row["ancestrySelected"]))
                            identityFailures.Add(ancestry.Definition.RaceName + " ancestry choice not selectable");
                        // Commit with the host generic hit-point reward.
                        bool rewarded = fcb != null && context.Host.GenericHitPoint != null &&
                            FavoredClassLevelUpHarness.Select(controller, fcb, context.Host.GenericHitPoint);
                        row["committed"] = rewarded && FavoredClassLevelUpHarness.Confirm(controller,
                            unit.Descriptor, row);
                        if ((bool)row["committed"])
                        {
                            committedUnits[isTrait ? "trait" : "standard"] = unit;
                            committed.Add(unit);
                            keep = true;
                        }
                        else
                            identityFailures.Add(ancestry.Definition.RaceName + (isTrait ? " trait" : " standard") +
                                " visit did not commit");
                    }
                    catch (Exception exception)
                    {
                        identityFailures.Add(ancestry.Definition.RaceName + ": " + exception.GetType().Name +
                            ": " + exception.Message);
                    }
                    finally
                    {
                        FavoredClassLevelUpHarness.Close(controller);
                        if (!keep) unit.Dispose();
                    }
                    pair[isTrait ? "trait" : "standard"] = row;
                }
                UnitEntityData standardUnit, traitUnit;
                if (committedUnits.TryGetValue("standard", out standardUnit) &&
                    committedUnits.TryGetValue("trait", out traitUnit))
                {
                    pair["comparison"] = CompareMostlyHuman(ancestry, context.MostlyHuman.Identity,
                        standardUnit, traitUnit, humanRace, outsider, allPersonCheckers, identityFailures);
                    // Appearance is never an eligibility signal: a standard
                    // geniekin wearing a Human appearance stays ineligible.
                    pair["humanAppearance"] = ObserveHumanAppearance(standardUnit, humanRace, humanLeaves,
                        humanTraits, allClosed, bridgeFailures);
                }
                rows.Add(pair);
            }
            bridge["racesUnleashedSuli"] = ObserveSuliMostlyHuman(library, rogue, rogueBonus, humanLeaves,
                humanTraits, allClosed, committed, bridgeFailures);
            return rows;
        }

        /// <summary>A standard geniekin given a native Human appearance keeps its own eligibility.</summary>
        private static JObject ObserveHumanAppearance(UnitEntityData standard, BlueprintRace humanRace,
            IList<BlueprintFeature> humanLeaves, Func<UnitDescriptor, LevelUpState, JObject> humanTraits,
            Func<JObject, bool> allClosed, IList<string> failures)
        {
            var result = new JObject();
            Kingmaker.UnitLogic.DollData original = standard.Descriptor.Doll;
            try
            {
                Kingmaker.UnitLogic.Class.LevelUp.DollState state;
                standard.Descriptor.Doll = CreateHumanDoll(humanRace, standard.Descriptor.Gender,
                    standard.Descriptor.Progression.Classes.First().CharacterClass, out state);
                result["dollRacePreset"] = standard.Descriptor.Doll == null || standard.Descriptor.Doll.RacePreset == null
                    ? null : standard.Descriptor.Doll.RacePreset.name;
                JObject traits = humanTraits(standard.Descriptor, null);
                bool leaves = humanLeaves.Any(leaf => (leaf.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<Prerequisite>().Where(FavoredClassHostRaceBridge.IsTracked)
                    .Any(check => check.Check(null, standard.Descriptor, null)));
                bool graphHuman = FavoredClassRuntime.PermittedAncestries(standard.Descriptor)
                    .Contains(FavoredClassAncestry.Human);
                result["humanRaceTraits"] = traits;
                result["hostHumanLeafPrerequisite"] = leaves;
                result["kmgHumanAncestry"] = graphHuman;
                if (!allClosed(traits) || leaves || graphHuman)
                    failures.Add(standard.Descriptor.Progression.Race.name + " gained human access from appearance");
            }
            catch (Exception exception)
            {
                failures.Add("human appearance: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                standard.Descriptor.Doll = original;
            }
            return result;
        }

        private static Kingmaker.UnitLogic.DollData CreateHumanDoll(BlueprintRace race, Kingmaker.Blueprints.Gender gender,
            BlueprintCharacterClass characterClass, out Kingmaker.UnitLogic.Class.LevelUp.DollState state)
        {
            Kingmaker.Blueprints.CharGen.BlueprintRaceVisualPreset preset = race.Presets == null || race.Presets.Length == 0
                ? null : race.Presets[0];
            if (preset == null)
                throw new InvalidOperationException("The native Human race has no visual preset.");
            state = new Kingmaker.UnitLogic.Class.LevelUp.DollState();
            state.SetGender(gender);
            state.SetRace(race);
            state.SetRacePreset(preset);
            state.SetClass(characterClass);
            Kingmaker.UnitLogic.DollData data = state.CreateData();
            if (data == null || !ReferenceEquals(data.RacePreset, preset))
                throw new InvalidOperationException("The native Human DollData is incomplete.");
            return data;
        }

        /// <summary>
        /// Races Unleashed's Suli "Mostly Human" is an unverified provider
        /// (it only removes OutsiderType): it must grant no human access.
        /// </summary>
        private JObject ObserveSuliMostlyHuman(LibraryScriptableObject library, BlueprintCharacterClass rogue,
            BlueprintFeatureSelection rogueBonus, IList<BlueprintFeature> humanLeaves,
            Func<UnitDescriptor, LevelUpState, JObject> humanTraits, Func<JObject, bool> allClosed,
            List<UnitEntityData> committed, IList<string> failures)
        {
            var result = new JObject();
            BlueprintScriptableObject raceBlueprint, traitBlueprint;
            library.BlueprintsByAssetId.TryGetValue(FcbSuliRaceGuid, out raceBlueprint);
            library.BlueprintsByAssetId.TryGetValue(FcbSuliMostlyHumanGuid, out traitBlueprint);
            var suli = raceBlueprint as BlueprintRace;
            var ruMostlyHuman = traitBlueprint as BlueprintFeature;
            if (suli == null || ruMostlyHuman == null)
            {
                result["provider"] = "absent (EXPECTED PROVIDER ABSENCE)";
                return result;
            }
            UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
            LevelUpController controller = null;
            bool keep = false;
            try
            {
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, suli, rogue, "KMG FCB Suli Control");
                FavoredClassLevelUpHarness.ChooseFavoredClass(controller, rogue, result);
                FavoredClassLevelUpHarness.FillOthers(controller,
                    new HashSet<string>(StringComparer.Ordinal) { rogueBonus.AssetGuid });
                FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller, rogueBonus.AssetGuid);
                bool rewarded = fcb != null && FavoredClassIntegrationCoordinator.Host.GenericHitPoint != null &&
                    FavoredClassLevelUpHarness.Select(controller, fcb, FavoredClassIntegrationCoordinator.Host.GenericHitPoint);
                result["committed"] = rewarded && FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, result);
                if (!(bool)result["committed"])
                {
                    failures.Add("the Suli control visit did not commit");
                    return result;
                }
                committed.Add(unit);
                keep = true;
                unit.Descriptor.AddFact(ruMostlyHuman);
                result["ruMostlyHumanFact"] = unit.Descriptor.HasFact(ruMostlyHuman);
                JObject traits = humanTraits(unit.Descriptor, null);
                bool leaves = humanLeaves.Any(leaf => (leaf.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<Prerequisite>().Where(FavoredClassHostRaceBridge.IsTracked)
                    .Any(check => check.Check(null, unit.Descriptor, null)));
                bool graphHuman = FavoredClassRuntime.PermittedAncestries(unit.Descriptor)
                    .Contains(FavoredClassAncestry.Human);
                result["humanRaceTraits"] = traits;
                result["hostHumanLeafPrerequisite"] = leaves;
                result["kmgHumanAncestry"] = graphHuman;
                if (!(bool)result["ruMostlyHumanFact"] || !allClosed(traits) || leaves || graphHuman)
                    failures.Add("Races Unleashed's Suli Mostly Human was treated as human");
            }
            catch (Exception exception)
            {
                failures.Add("Suli control: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
                if (!keep) unit.Dispose();
            }
            return result;
        }

        private static JObject CompareMostlyHuman(ElementalMostlyHumanRaceBlueprints ancestry,
            BlueprintFeature identity, UnitEntityData standard, UnitEntityData trait, BlueprintRace humanRace,
            BlueprintFeature outsider, IAbilityTargetChecker[] personCheckers, IList<string> failures)
        {
            string race = ancestry.Definition.RaceName;
            Func<UnitEntityData, List<string>> facts = unit => unit.Descriptor.Progression.Features
                .Enumerable.Select(value => value.Blueprint.AssetGuid).OrderBy(value => value,
                    StringComparer.Ordinal).ToList();
            List<string> standardFacts = facts(standard);
            List<string> traitFacts = facts(trait);
            List<string> added = Subtract(traitFacts, standardFacts);
            List<string> removed = Subtract(standardFacts, traitFacts);
            // A genuine human identity opens the host's human race traits, so
            // the deterministic filler may take one on the Mostly Human path;
            // the standard control can never have one.
            var humanTraits = new HashSet<string>(FavoredClassHostRaceBridge.Scope.OtherHumanPrerequisites
                .Select(value => value.Substring(value.LastIndexOf(':') + 1)), StringComparer.Ordinal);
            List<string> humanTraitsTaken = added.Where(humanTraits.Contains).ToList();
            added = added.Where(value => !humanTraits.Contains(value)).ToList();
            var expectedAdded = new[] { ancestry.Trait.AssetGuid, identity.AssetGuid }
                .OrderBy(value => value, StringComparer.Ordinal).ToList();
            StatType[] attributes = { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                StatType.Intelligence, StatType.Wisdom, StatType.Charisma };
            Func<UnitEntityData, string> scores = unit => string.Join(",", attributes.Select(stat =>
                unit.Descriptor.Stats.GetStat(stat).ModifiedValue.ToString()).ToArray());
            Func<UnitEntityData, UnitEntityData, string> person = (caster, target) =>
                string.Join(",", personCheckers.Select(checker =>
                    checker.CanTarget(caster, new TargetWrapper(target)) ? "1" : "0").ToArray());
            var result = new JObject
            {
                ["factsAdded"] = new JArray(added),
                ["humanRaceTraitsTaken"] = new JArray(humanTraitsTaken),
                ["factsRemoved"] = new JArray(removed),
                ["race"] = trait.Descriptor.Progression.Race == null ? null :
                    trait.Descriptor.Progression.Race.name,
                ["raceId"] = trait.Descriptor.Progression.Race == null ? null :
                    trait.Descriptor.Progression.Race.RaceId.ToString(),
                ["standardScores"] = scores(standard),
                ["traitScores"] = scores(trait),
                ["standardOutsiderFact"] = standard.Descriptor.HasFact(outsider),
                ["traitOutsiderFact"] = trait.Descriptor.HasFact(outsider),
                ["traitHumanRaceFact"] = trait.Descriptor.HasFact(humanRace),
                ["traitIdentityFact"] = trait.Descriptor.HasFact(identity),
                ["standardPersonChecks"] = person(trait, standard),
                ["traitPersonChecks"] = person(standard, trait)
            };
            if (!added.SequenceEqual(expectedAdded))
                failures.Add(race + " added facts " + string.Join(",", added.ToArray()));
            if (humanTraitsTaken.Count > 1)
                failures.Add(race + " took more than one human race trait");
            if (standardFacts.Any(humanTraits.Contains))
                failures.Add(race + " standard control holds a human race trait");
            if (!removed.SequenceEqual(new[] { ancestry.Standard.AssetGuid }))
                failures.Add(race + " removed facts " + string.Join(",", removed.ToArray()));
            if (!ReferenceEquals(trait.Descriptor.Progression.Race, ancestry.Race) ||
                !ReferenceEquals(standard.Descriptor.Progression.Race, ancestry.Race))
                failures.Add(race + " race identity changed");
            if ((string)result["standardScores"] != (string)result["traitScores"])
                failures.Add(race + " ability scores differ");
            if ((bool)result["standardOutsiderFact"] || (bool)result["traitOutsiderFact"] ||
                (bool)result["traitHumanRaceFact"])
                failures.Add(race + " gained a creature-type or HumanRace fact");
            if (!(bool)result["traitIdentityFact"])
                failures.Add(race + " Mostly Human identity missing");
            if ((string)result["standardPersonChecks"] != (string)result["traitPersonChecks"])
                failures.Add(race + " person-spell target checks differ");
            if (((string)result["traitPersonChecks"]).Contains("0"))
                failures.Add(race + " Mostly Human is not a humanoid target of every person spell");
            // Negative control: the same checkers reject a unit carrying OutsiderType.
            trait.Descriptor.AddFact(outsider);
            try
            {
                result["outsiderControlPersonChecks"] = person(standard, trait);
                if (!((string)result["outsiderControlPersonChecks"]).Contains("0"))
                    failures.Add(race + " person-spell checkers accept an OutsiderType unit");
            }
            finally
            {
                trait.Descriptor.RemoveFact(outsider);
            }
            if (trait.Descriptor.HasFact(outsider))
                failures.Add(race + " OutsiderType control was not removed");
            return result;
        }

        private static List<string> Subtract(List<string> left, List<string> right)
        {
            var pool = right.ToList();
            var result = new List<string>();
            foreach (string value in left)
                if (!pool.Remove(value))
                    result.Add(value);
            return result;
        }

        private JArray RunMostlyHumanGunslingerMenus(FcbMostlyHumanContext context, IList<string> failures,
            bool integration)
        {
            FavoredClassLeafPair grit = context.Leaves.Pair(FavoredClassCatalog.EffectGrit, null);
            FavoredClassLeafPair initiative = context.Leaves.Pair(FavoredClassCatalog.EffectInitiative, null);
            BlueprintCharacterClass gunslinger = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, context.Leaves.GunslingerClassGuid, "Gunslinger");
            BlueprintFeatureSelection bonus = context.Host.GunslingerSelection;
            FavoredClassLeafPair[] gunslingerPairs = context.Leaves.Pairs.Where(pair =>
                pair.HostClassGuid == context.Leaves.GunslingerClassGuid).ToArray();
            var rows = new JArray();
            foreach (ElementalMostlyHumanRaceBlueprints ancestry in context.MostlyHuman.Races)
                foreach (BlueprintFeature choice in new[] { ancestry.Standard, ancestry.Trait })
                {
                    bool isTrait = ReferenceEquals(choice, ancestry.Trait);
                    var row = new JObject { ["race"] = ancestry.Definition.RaceName };
                    UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                    LevelUpController controller = null;
                    try
                    {
                        controller = OpenMostlyHumanVisit(unit, ancestry.Race, gunslinger, ancestry, choice,
                            null, null, row);
                        FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger, row);
                        FavoredClassLevelUpHarness.FillOthers(controller,
                            new HashSet<string>(StringComparer.Ordinal) { bonus.AssetGuid });
                        FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                            bonus.AssetGuid);
                        string[] offered = fcb == null ? new string[0] : gunslingerPairs.Where(pair =>
                            FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ||
                            (pair.Partial != null && FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial)))
                            .Select(pair => pair.Effect.Id).Distinct().OrderBy(value => value, StringComparer.Ordinal)
                            .ToArray();
                        row["offered"] = new JArray(offered);
                        var expected = new List<string>();
                        if (integration && isTrait) expected.Add(grit.Effect.Id);
                        if (integration && ancestry.Definition.Race == ElementalHeritageRace.Ifrit)
                            expected.Add(initiative.Effect.Id);
                        string[] ordered = expected.Distinct().OrderBy(value => value, StringComparer.Ordinal).ToArray();
                        if (!offered.SequenceEqual(ordered))
                            failures.Add(ancestry.Definition.RaceName + (isTrait ? " Mostly Human" : " standard") +
                                " offered " + string.Join(",", offered) + " expected " + string.Join(",", ordered));
                    }
                    catch (Exception exception)
                    {
                        failures.Add(ancestry.Definition.RaceName + ": " + exception.GetType().Name + ": " +
                            exception.Message);
                    }
                    finally
                    {
                        FavoredClassLevelUpHarness.Close(controller);
                        unit.Dispose();
                    }
                    rows.Add(row);
                }
            return rows;
        }

        private JArray RunMostlyHumanHeritages(FcbMostlyHumanContext context, IList<string> failures,
            bool integration)
        {
            var rows = new JArray();
            foreach (ElementalMostlyHumanRaceBlueprints ancestry in context.MostlyHuman.Races)
            {
                ElementalRaceBlueprints parent = context.Parent(ancestry.Definition.Race);
                string classFamily;
                string[] expected;
                switch (ancestry.Definition.Race)
                {
                    case ElementalHeritageRace.Ifrit:
                        classFamily = FavoredClassCatalog.Gunslinger;
                        expected = new[] { FavoredClassCatalog.EffectInitiative };
                        break;
                    case ElementalHeritageRace.Oread:
                        classFamily = FavoredClassCatalog.Fighter;
                        expected = new[] { FavoredClassCatalog.EffectBullRushDragDefense };
                        break;
                    case ElementalHeritageRace.Undine:
                        classFamily = FavoredClassCatalog.Monk;
                        expected = new[] { FavoredClassCatalog.EffectGrappleStunning };
                        break;
                    default:
                        classFamily = FavoredClassCatalog.Gunslinger;
                        expected = new string[0];
                        break;
                }
                FavoredClassLeafPair[] classPairs = context.Leaves.Pairs.Where(pair =>
                    pair.Effect.ClassFamily == classFamily).ToArray();
                BlueprintCharacterClass characterClass = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        classPairs[0].HostClassGuid, classFamily);
                BlueprintFeatureSelection bonus = context.Host.BonusSelectionFor(characterClass.AssetGuid);
                foreach (ElementalHeritageBlueprints heritage in parent.Heritages.Choices())
                {
                    var row = new JObject
                    {
                        ["race"] = ancestry.Definition.RaceName,
                        ["heritage"] = heritage.Definition.Id.ToString(),
                        ["class"] = classFamily
                    };
                    UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                    LevelUpController controller = null;
                    try
                    {
                        controller = OpenMostlyHumanVisit(unit, ancestry.Race, characterClass, ancestry,
                            ancestry.Standard, parent.Heritages, heritage.Marker, row);
                        FavoredClassLevelUpHarness.ChooseFavoredClass(controller, characterClass, row);
                        FavoredClassLevelUpHarness.FillOthers(controller,
                            new HashSet<string>(StringComparer.Ordinal) { bonus.AssetGuid });
                        FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                            bonus.AssetGuid);
                        string[] offered = fcb == null ? new string[0] : classPairs.Where(pair =>
                            FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ||
                            (pair.Partial != null && FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial)))
                            .Select(pair => pair.Effect.Id).Distinct().OrderBy(value => value, StringComparer.Ordinal)
                            .ToArray();
                        bool humanOffered = fcb != null && HostHumanLeaves(bonus).Any(leaf =>
                            FavoredClassLevelUpHarness.CanSelect(controller, fcb, leaf));
                        row["offered"] = new JArray(offered);
                        row["hostHumanOffered"] = humanOffered;
                        row["heritageFact"] = controller.Preview.HasFact(heritage.Marker);
                        if (!(bool)row["heritageSelected"] || !(bool)row["heritageFact"])
                            failures.Add(heritage.Definition.Id + " heritage was not applied");
                        if (!integration)
                            expected = new string[0];
                        if (!offered.SequenceEqual(expected.OrderBy(value => value, StringComparer.Ordinal)))
                            failures.Add(heritage.Definition.Id + " offered " + string.Join(",", offered) +
                                " expected " + string.Join(",", expected));
                        if (humanOffered)
                            failures.Add(heritage.Definition.Id + " gained host human access without the trait");
                    }
                    catch (Exception exception)
                    {
                        failures.Add(heritage.Definition.Id + ": " + exception.GetType().Name + ": " +
                            exception.Message);
                    }
                    finally
                    {
                        FavoredClassLevelUpHarness.Close(controller);
                        unit.Dispose();
                    }
                    rows.Add(row);
                }
            }
            if (rows.Count != 12)
                failures.Add("expected 12 heritage visits, observed " + rows.Count);
            return rows;
        }
    }
}
