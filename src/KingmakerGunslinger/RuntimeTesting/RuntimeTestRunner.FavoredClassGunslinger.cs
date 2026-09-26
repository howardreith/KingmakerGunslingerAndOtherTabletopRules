using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Charter defaults (first-party and adaptations on, third-party off,
        // Mostly Human not yet published): the exact Gunslinger favored-class
        // effects each source-addressable ancestry is offered at level 1.
        private static readonly Dictionary<string, string[]> FcbExpectedGunslingerMenus =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { FavoredClassAncestry.Human, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.HalfElf, new[] { FavoredClassCatalog.EffectGrit,
                    FavoredClassCatalog.EffectFirearmConfirmation } },
                { FavoredClassAncestry.HalfOrc, new[] { FavoredClassCatalog.EffectGrit,
                    FavoredClassCatalog.EffectPistolWhip } },
                { FavoredClassAncestry.Elf, new[] { FavoredClassCatalog.EffectFirearmConfirmation } },
                { FavoredClassAncestry.Dwarf, new[] { FavoredClassCatalog.EffectMisfire } },
                { FavoredClassAncestry.Gnome, new string[0] },
                { FavoredClassAncestry.Halfling, new[] { FavoredClassCatalog.EffectHalflingNimble,
                    FavoredClassCatalog.EffectHalflingDodge } },
                { FavoredClassAncestry.Aasimar, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Tiefling, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Goblin, new[] { FavoredClassCatalog.EffectFirearmConfirmation } },
                { FavoredClassAncestry.Hobgoblin, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Fetchling, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Dhampir, new string[0] },
                { FavoredClassAncestry.Drow, new string[0] },
                { FavoredClassAncestry.Duergar, new string[0] },
                { FavoredClassAncestry.Ganzi, new string[0] },
                { FavoredClassAncestry.Suli, new string[0] },
                { FavoredClassAncestry.Ifrit, new[] { FavoredClassCatalog.EffectInitiative } },
                { FavoredClassAncestry.Oread, new string[0] },
                { FavoredClassAncestry.Sylph, new string[0] },
                { FavoredClassAncestry.Undine, new string[0] },
            };

        // Jon Brazer Enterprises routes added when the optional third-party
        // profile is on (G16 Dhampir, G17 Drow, G18 Duergar, G21 Tiefling;
        // G20 Orc has no playable race and adds nothing to Half-orcs, who
        // already hold the same Pistol-Whip counter through G05).
        private static readonly Dictionary<string, string[]> FcbThirdPartyGunslingerMenus =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { FavoredClassAncestry.Dhampir, new[] { FavoredClassCatalog.EffectFirearmConfirmation } },
                { FavoredClassAncestry.Drow, new[] { FavoredClassCatalog.EffectDrowNimble } },
                { FavoredClassAncestry.Duergar, new[] { FavoredClassCatalog.EffectMisfire } },
                { FavoredClassAncestry.Tiefling, new[] { FavoredClassCatalog.EffectDirtyTrickTrip } },
            };

        private sealed class FcbCounterPlan
        {
            internal FcbCounterPlan(FavoredClassLeafPair pair, int levels)
            {
                Pair = pair;
                Levels = levels;
            }

            /// <summary>Null means the host's hit-point reward.</summary>
            internal FavoredClassLeafPair Pair { get; private set; }
            internal int Levels { get; private set; }
        }

        // E01/E12/M01-M03 for the Gunslinger core: per-race level-1 menus,
        // archetype replacement, publication by profile, and native
        // twenty-level progressions for every first-party counter.
        private RuntimeTestResult RunFavoredClassGunslingerMenus()
        {
            var assertions = new List<RuntimeTestAssertion>();
            var evidence = new JObject();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslingerSet = BlueprintBootstrap.GunslingerClass;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && host.GunslingerSelection != null && leaves != null &&
                gunslingerSet != null && gunslingerSet.Pistolero != null &&
                gunslingerSet.MysteriousStranger != null && gunslingerSet.MusketMaster != null;
            assertions.Add(Assertion("fcb-integration-published",
                "the exact qualified host is ready, owned leaves are published and the three archetypes exist",
                status.ToString(), ready, "FavoredClassIntegrationStatusRegistry after the first update"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);

            BlueprintCharacterClass gunslinger = gunslingerSet.CharacterClass;
            BlueprintFeatureSelection selection = host.GunslingerSelection;
            var reserved = new HashSet<string>(StringComparer.Ordinal) { selection.AssetGuid };
            Func<string, BlueprintRace> race = ancestry =>
            {
                FavoredClassRaceIdentity identity = FavoredClassRaceIdentities.All.Single(value =>
                    value.Ancestry == ancestry);
                return BlueprintLibraryLookup.RequireExact<BlueprintRace>(BlueprintBootstrap.Library,
                    identity.RaceGuid, ancestry);
            };
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            // Publication by profile: third-party-only counters stay unpublished.
            // Each counter is looked up in its own class's host selection (the
            // Gunslinger's for Gunslinger counters).
            var publication = new JArray();
            var publicationFailures = new List<string>();
            foreach (FavoredClassLeafPair pair in leaves.Pairs)
            {
                bool expected = FcbOfferedByProfile(pair);
                BlueprintFeatureSelection home = host.BonusSelectionFor(pair.HostClassGuid);
                int present = home == null ? -1 : pair.Leaves.Count(leaf => home.AllFeatures.Contains(leaf));
                publication.Add(pair.Effect.Id + "|" + (pair.TargetKey ?? "-") + "@" +
                    (home == null ? "no-host-selection" : home.name) + "=" + present +
                    "/" + pair.Leaves.Count() + (expected ? " expected" : " withheld"));
                if (present != (expected ? pair.Leaves.Count() : 0))
                    publicationFailures.Add(pair.Effect.Id + " published " + present + " expected " + expected);
            }
            evidence["publication"] = publication;

            var menus = new JArray();
            var menuFailures = new List<string>();
            var archetypes = new JObject();
            var archetypeFailures = new List<string>();
            var progressions = new JObject();
            var progressionFailures = new List<string>();
            bool cleaned = false;
            try
            {
                foreach (FavoredClassRaceIdentity identity in FavoredClassRaceIdentities.All)
                    menus.Add(RunGunslingerMenuRow(identity, gunslinger, leaves, selection, reserved,
                        menuFailures));
                archetypes["mysteriousStranger"] = RunArchetypeMenu(race(FavoredClassAncestry.Halfling),
                    gunslinger, gunslingerSet.MysteriousStranger.Archetype, leaves, selection, reserved,
                    FavoredClassCatalog.EffectHalflingDodge, FavoredClassCatalog.EffectHalflingNimble,
                    archetypeFailures);
                archetypes["musketMaster"] = RunArchetypeMenu(race(FavoredClassAncestry.Halfling),
                    gunslinger, gunslingerSet.MusketMaster.Archetype, leaves, selection, reserved,
                    FavoredClassCatalog.EffectHalflingNimble, FavoredClassCatalog.EffectHalflingDodge,
                    archetypeFailures);

                BlueprintArchetype pistolero = gunslingerSet.Pistolero.Archetype;
                Func<string, string, FavoredClassLeafPair> pair = (effect, target) => leaves.Pair(effect, target);
                progressions["halfling"] = RunCounterProgression("halfling", race(FavoredClassAncestry.Halfling),
                    gunslinger, pistolero, leaves, selection, reserved, host, new[]
                    {
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectHalflingNimble, null), 8),
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectHalflingDodge, null), 12)
                    }, progressionFailures);
                progressions["elf"] = RunCounterProgression("elf", race(FavoredClassAncestry.Elf),
                    gunslinger, pistolero, leaves, selection, reserved, host, new[]
                    {
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectFirearmConfirmation, null), 15),
                        new FcbCounterPlan(null, 5)
                    }, progressionFailures);
                progressions["dwarf"] = RunCounterProgression("dwarf", race(FavoredClassAncestry.Dwarf),
                    gunslinger, pistolero, leaves, selection, reserved, host, new[]
                    {
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectMisfire, "Pistol"), 6),
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectMisfire, "Musket"), 4),
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectMisfire, "Pistol"), 2),
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectMisfire, "Blunderbuss"), 8)
                    }, progressionFailures);
                progressions["ifrit"] = RunCounterProgression("ifrit", race(FavoredClassAncestry.Ifrit),
                    gunslinger, pistolero, leaves, selection, reserved, host, new[]
                    {
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectInitiative, null), 20)
                    }, progressionFailures);
                progressions["halfOrc"] = RunCounterProgression("half-orc", race(FavoredClassAncestry.HalfOrc),
                    gunslinger, pistolero, leaves, selection, reserved, host, new[]
                    {
                        new FcbCounterPlan(pair(FavoredClassCatalog.EffectPistolWhip, null), 20)
                    }, progressionFailures);
            }
            catch (Exception exception)
            {
                progressionFailures.Add("exception=" + exception);
            }
            finally
            {
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            evidence["menus"] = menus;
            evidence["archetypes"] = archetypes;
            evidence["progressions"] = progressions;
            string evidencePath = WriteFavoredClassEvidence("favored-class-gunslinger-menus.json", evidence);

            assertions.Add(Assertion("fcb-gunslinger-publication-by-profile",
                "every counter with an enabled route is published exactly once; the third-party-only Drow Nimble and dirty trick/trip counters and the excluded Storm Call and Mockery counters stay unpublished while registered",
                Describe(publication, publicationFailures), publicationFailures.Count == 0,
                "each counter's own live host class bonus selection AllFeatures"));
            assertions.Add(Assertion("fcb-gunslinger-race-menus",
                "each playable source-addressable race is offered exactly its charter Gunslinger counters (partial leaf open, full leaf closed at N=0); an absent or unplayable race is reported, never scored",
                Describe(menus, menuFailures), menuFailures.Count == 0,
                "level-1 native visit per race; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-gunslinger-archetype-replacement",
                "a Mysterious Stranger is not offered Nimble improvements and a Musket Master is not offered the Dodge branch, while the other branch stays available",
                Describe(archetypes, archetypeFailures), archetypeFailures.Count == 0,
                "native AddArchetype at level 1; PrerequisiteNoArchetype on the preview"));
            assertions.Add(Assertion("fcb-gunslinger-native-progressions",
                "twenty native level-ups per ancestry open exactly the policy leaf of the planned counter, bank (floor(N/d), N - floor(N/d)), close a capped counter at its cap, keep separate firearm-type counters independent, and yield the earned steps",
                Describe(progressions, progressionFailures), progressionFailures.Count == 0,
                "LevelUpController.SelectFeature + IsComplete gate + ApplyLevelup; FavoredClassEarnedSteps"));
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

        private JObject RunGunslingerMenuRow(FavoredClassRaceIdentity identity,
            BlueprintCharacterClass gunslinger, FavoredClassBlueprintSet leaves,
            BlueprintFeatureSelection selection, ICollection<string> reserved, IList<string> failures)
        {
            var row = new JObject { ["ancestry"] = identity.Ancestry, ["race"] = identity.RaceGuid };
            BlueprintScriptableObject resolved;
            BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(identity.RaceGuid, out resolved);
            var raceBlueprint = resolved as BlueprintRace;
            if (raceBlueprint == null)
            {
                row["disposition"] = "EXPECTED PROVIDER ABSENCE";
                if (identity.Provider != FavoredClassRaceProvider.Optional)
                    failures.Add(identity.Ancestry + ": non-optional race identity is missing");
                return row;
            }
            if (!BlueprintRoot.Instance.Progression.CharacterRaces.Contains(raceBlueprint))
            {
                row["disposition"] = "PRESENT BUT NOT PLAYABLE";
                if (identity.Provider != FavoredClassRaceProvider.Optional)
                    failures.Add(identity.Ancestry + ": native or KMG race is not playable");
                return row;
            }
            UnitEntityData unit = null;
            LevelUpController controller = null;
            try
            {
                unit = FavoredClassLevelUpHarness.CreateUnit(14);
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, raceBlueprint, gunslinger,
                    "KMG FCB Menu " + identity.Ancestry);
                if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger, row) == null)
                {
                    failures.Add(identity.Ancestry + ": favored Gunslinger progression unavailable");
                    return row;
                }
                FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                    selection.AssetGuid);
                if (fcb == null)
                {
                    failures.Add(identity.Ancestry + ": no Gunslinger favored-class state");
                    return row;
                }
                var offered = new List<string>();
                var offeredLeaves = new JArray();
                foreach (FavoredClassLeafPair pair in leaves.Pairs)
                {
                    bool full = FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full);
                    bool partial = pair.Partial != null &&
                        FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial);
                    if (!full && !partial)
                        continue;
                    offeredLeaves.Add((partial ? pair.Partial : pair.Full).name);
                    if (full)
                        failures.Add(identity.Ancestry + ": full leaf open at N=0 for " + pair.Full.name);
                    if (!offered.Contains(pair.Effect.Id))
                        offered.Add(pair.Effect.Id);
                }
                row["offeredEffects"] = new JArray(offered);
                row["offeredLeaves"] = offeredLeaves;
                string[] thirdParty;
                string[] expected = FcbExpectedGunslingerMenus[identity.Ancestry].Concat(
                    FavoredClassRuntime.Profile.ThirdParty &&
                        FcbThirdPartyGunslingerMenus.TryGetValue(identity.Ancestry, out thirdParty)
                        ? thirdParty : new string[0]).Distinct(StringComparer.Ordinal).ToArray();
                if (!offered.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                        expected.OrderBy(value => value, StringComparer.Ordinal)))
                    failures.Add(identity.Ancestry + ": offered " + string.Join(",", offered.ToArray()) +
                        " expected " + string.Join(",", expected));
                if (offered.Contains(FavoredClassCatalog.EffectMisfire) && offeredLeaves.Count(value =>
                        ((string)value).Contains("Misfire")) != 3)
                    failures.Add(identity.Ancestry + ": each of the three firearm types needs its own counter");
                return row;
            }
            catch (Exception exception)
            {
                row["exception"] = exception.GetType().Name + ": " + exception.Message;
                failures.Add(identity.Ancestry + ": " + row["exception"]);
                return row;
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
                if (unit != null) unit.Dispose();
            }
        }

        private JObject RunArchetypeMenu(BlueprintRace race, BlueprintCharacterClass gunslinger,
            BlueprintArchetype archetype, FavoredClassBlueprintSet leaves,
            BlueprintFeatureSelection selection, ICollection<string> reserved, string keptEffect,
            string replacedEffect, IList<string> failures)
        {
            var row = new JObject { ["archetype"] = archetype.name };
            UnitEntityData unit = null;
            LevelUpController controller = null;
            try
            {
                unit = FavoredClassLevelUpHarness.CreateUnit(14);
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, gunslinger,
                    "KMG FCB Archetype", archetype);
                row["previewIsArchetype"] = controller.Preview.Progression.IsArchetype(archetype);
                if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger, row) == null)
                {
                    failures.Add(archetype.name + ": favored Gunslinger progression unavailable");
                    return row;
                }
                FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                    selection.AssetGuid);
                if (fcb == null)
                {
                    failures.Add(archetype.name + ": no Gunslinger favored-class state");
                    return row;
                }
                FavoredClassLeafPair kept = leaves.Pair(keptEffect, null);
                FavoredClassLeafPair replaced = leaves.Pair(replacedEffect, null);
                bool keptOpen = FavoredClassLevelUpHarness.CanSelect(controller, fcb, kept.Partial);
                bool replacedOpen = FavoredClassLevelUpHarness.CanSelect(controller, fcb, replaced.Partial) ||
                    FavoredClassLevelUpHarness.CanSelect(controller, fcb, replaced.Full);
                row["keptOpen"] = keptOpen;
                row["replacedOpen"] = replacedOpen;
                if (!(bool)row["previewIsArchetype"])
                    failures.Add(archetype.name + ": the preview did not take the archetype");
                if (!keptOpen)
                    failures.Add(archetype.name + ": " + keptEffect + " must stay available");
                if (replacedOpen)
                    failures.Add(archetype.name + ": " + replacedEffect + " must not be offered");
                return row;
            }
            catch (Exception exception)
            {
                failures.Add(archetype.name + ": " + exception.GetType().Name + ": " + exception.Message);
                return row;
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
                if (unit != null) unit.Dispose();
            }
        }

        private JObject RunCounterProgression(string label, BlueprintRace race,
            BlueprintCharacterClass gunslinger, BlueprintArchetype archetype,
            FavoredClassBlueprintSet leaves, BlueprintFeatureSelection selection,
            ICollection<string> reserved, FavoredClassHostHandles host, IList<FcbCounterPlan> plan,
            IList<string> failures)
        {
            var result = new JObject { ["archetype"] = archetype.name };
            var levels = new JArray();
            UnitEntityData unit = null;
            try
            {
                unit = FavoredClassLevelUpHarness.CreateUnit(14);
                var schedule = new List<FavoredClassLeafPair>();
                foreach (FcbCounterPlan step in plan)
                    for (int index = 0; index < step.Levels; index++)
                        schedule.Add(step.Pair);
                if (schedule.Count != 20)
                    throw new InvalidOperationException(label + " plan must cover twenty levels.");
                for (int level = 1; level <= 20; level++)
                {
                    FavoredClassLeafPair pair = schedule[level - 1];
                    var row = new JObject { ["level"] = level };
                    Dictionary<FavoredClassLeafPair, int> before = leaves.Pairs.ToDictionary(value => value,
                        value => Invested(unit, value));
                    BlueprintFeature choice;
                    bool expectFull = false;
                    if (pair == null)
                    {
                        choice = host.GenericHitPoint;
                    }
                    else
                    {
                        int investments = before[pair];
                        FavoredClassRate rate = pair.Effect.Rate;
                        expectFull = FavoredClassRankPolicy.NextInvestmentIsFull(investments, rate.Divisor);
                        choice = expectFull ? pair.Full : pair.Partial;
                        row["counter"] = pair.Effect.Id + "|" + (pair.TargetKey ?? "-");
                        row["investmentsBefore"] = investments;
                    }
                    LevelUpController controller = null;
                    try
                    {
                        controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, gunslinger,
                            "KMG FCB Progression " + label, archetype);
                        if (level == 1 && FavoredClassLevelUpHarness.ChooseFavoredClass(controller,
                                gunslinger, row) == null)
                            failures.Add(label + " level 1: favored Gunslinger progression unavailable");
                        FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                        FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                            selection.AssetGuid);
                        if (fcb == null)
                        {
                            failures.Add(Fmt("{0} level {1}: no open favored-class state", label, level));
                            levels.Add(row);
                            continue;
                        }
                        if (pair != null)
                        {
                            bool canFull = FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full);
                            bool canPartial = FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial);
                            row["canSelectFull"] = canFull;
                            row["canSelectPartial"] = canPartial;
                            if (canFull != expectFull || canPartial == expectFull)
                                failures.Add(Fmt("{0} level {1}: full={2} partial={3} expectFull={4}",
                                    label, level, canFull, canPartial, expectFull));
                        }
                        // A capped counter left behind must be closed.
                        foreach (FavoredClassLeafPair other in leaves.Pairs.Where(value =>
                            !ReferenceEquals(value, pair) && before[value] > 0 &&
                            before[value] >= FavoredClassRankPolicy.InvestmentCeiling(value.Effect.Rate)))
                        {
                            if (FavoredClassLevelUpHarness.CanSelect(controller, fcb, other.Full) ||
                                FavoredClassLevelUpHarness.CanSelect(controller, fcb, other.Partial))
                                failures.Add(Fmt("{0} level {1}: capped {2} is still open", label, level,
                                    other.Full.name));
                        }
                        if (!FavoredClassLevelUpHarness.Select(controller, fcb, choice))
                        {
                            failures.Add(Fmt("{0} level {1}: SelectFeature refused {2}", label, level,
                                choice.name));
                            levels.Add(row);
                            continue;
                        }
                        row["selected"] = choice.name;
                        FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                        if (!FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, row))
                            failures.Add(Fmt("{0} level {1}: build incomplete: {2}", label, level,
                                row["completion"]));
                    }
                    finally
                    {
                        FavoredClassLevelUpHarness.Close(controller);
                    }
                    foreach (FavoredClassLeafPair counter in leaves.Pairs)
                    {
                        int expected = before[counter] + (ReferenceEquals(counter, pair) ? 1 : 0);
                        int full = FavoredClassLevelUpHarness.Rank(unit.Descriptor, counter.Full);
                        int partial = FavoredClassLevelUpHarness.Rank(unit.Descriptor, counter.Partial);
                        if (full != FavoredClassRankPolicy.FullRankAfter(expected, counter.Effect.Rate.Divisor) ||
                            partial != FavoredClassRankPolicy.PartialRankAfter(expected,
                                counter.Effect.Rate.Divisor))
                            failures.Add(Fmt("{0} level {1}: {2} ranks ({3},{4}) for N={5}", label, level,
                                counter.Full.name, full, partial, expected));
                    }
                    if (pair != null)
                    {
                        row["fullRank"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, pair.Full);
                        row["partialRank"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, pair.Partial);
                    }
                    row["classLevel"] = unit.Descriptor.Progression.GetClassLevel(gunslinger);
                    if ((int)row["classLevel"] != level)
                        failures.Add(Fmt("{0} level {1}: class level {2}", label, level, row["classLevel"]));
                    levels.Add(row);
                }
                var steps = new JObject();
                foreach (FavoredClassLeafPair counter in leaves.Pairs.Where(value => Invested(unit, value) > 0))
                    steps[counter.Effect.Id + "|" + (counter.TargetKey ?? "-")] =
                        FavoredClassEarnedSteps.For(unit.Descriptor, counter.Effect.Id, counter.TargetKey);
                result["earnedSteps"] = steps;
                result["misfireReduction"] = "Pistol=" +
                    FavoredClassEarnedSteps.MisfireReduction(unit, FirearmKind.Pistol) + ";Musket=" +
                    FavoredClassEarnedSteps.MisfireReduction(unit, FirearmKind.Musket) + ";Blunderbuss=" +
                    FavoredClassEarnedSteps.MisfireReduction(unit, FirearmKind.Blunderbuss);
            }
            catch (Exception exception)
            {
                failures.Add(label + ": exception=" + exception);
            }
            finally
            {
                if (unit != null) unit.Dispose();
            }
            result["levels"] = levels;
            return result;
        }

        private static int Invested(UnitEntityData unit, FavoredClassLeafPair pair)
        {
            return FavoredClassLevelUpHarness.Rank(unit.Descriptor, pair.Full) +
                FavoredClassLevelUpHarness.Rank(unit.Descriptor, pair.Partial);
        }

        // M07-M13, M19: the earned ranks act through the native rules only in
        // their own attack, defense, initiative and maneuver contexts.
        private RuntimeTestResult RunFavoredClassGunslingerMechanics()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                leaves != null && gunslinger != null && FavoredClassRuntime.MechanicsEnabled;
            assertions.Add(Assertion("fcb-integration-published",
                "the exact qualified host is ready and favored-class mechanics are enabled",
                status.ToString(), ready, "FavoredClassIntegrationStatusRegistry after the first update"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);

            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            var evidence = new JObject();
            var failures = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string key in new[] { "misfire", "confirmation", "pistolWhip", "nimble", "dodge",
                "initiative", "maneuver" })
                failures[key] = new List<string>();
            bool cleaned = false;
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            var disposables = new List<UnitEntityData>();
            Func<UnitEntityData> create = () =>
            {
                UnitEntityData created = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                disposables.Add(created);
                return created;
            };
            try
            {
                evidence["misfire"] = ObserveFavoredClassMisfire(create, leaves, gunslinger, failures["misfire"]);
                evidence["confirmation"] = ObserveFavoredClassConfirmation(create, leaves, gunslinger,
                    failures["confirmation"]);
                evidence["pistolWhip"] = ObserveFavoredClassPistolWhip(create, leaves, gunslinger,
                    failures["pistolWhip"]);
                evidence["nimble"] = ObserveFavoredClassNimble(create, leaves, gunslinger, failures["nimble"]);
                evidence["dodge"] = ObserveFavoredClassDodge(create, leaves, gunslinger, failures["dodge"]);
                evidence["initiative"] = ObserveFavoredClassInitiative(create, leaves, gunslinger,
                    failures["initiative"]);
                evidence["maneuver"] = ObserveFavoredClassManeuver(create, leaves, failures["maneuver"]);
            }
            catch (Exception exception)
            {
                failures["misfire"].Add("exception=" + exception);
            }
            finally
            {
                foreach (UnitEntityData unit in disposables)
                {
                    try
                    {
                        if (unit.Body != null && unit.Body.PrimaryHand.MaybeItem != null)
                        {
                            var weapon = unit.Body.PrimaryHand.MaybeItem as ItemEntityWeapon;
                            if (weapon != null) FirearmRuntimeState.Service.Forget(weapon);
                            unit.Body.PrimaryHand.RemoveItem(false);
                        }
                        if (unit.Body != null && unit.Body.Armor.HasArmor)
                            unit.Body.Armor.RemoveItem(false);
                        unit.Dispose();
                    }
                    catch (Exception) { }
                }
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-gunslinger-mechanics.json", evidence);
            assertions.Add(Assertion("fcb-misfire-native-threshold",
                "native firearm attacks (M07): the wielder's per-type reduction lowers the authoritative threshold after the base, Broken (+4, +2 with Gun Training), ammunition and Reliable, never below 1 and never raising a 0; it never reaches another firearm type, follows the fired weapon in either hand, and Dead Shot and Scatter Shot use the same threshold",
                Describe(evidence["misfire"], failures["misfire"]), failures["misfire"].Count == 0,
                "RuleAttackRoll with seeded native d20 through FirearmMisfireRuntime; DeadShotRuntime and ScatterShotRuntime volleys; FirearmRuntimeState conditions"));
            assertions.Add(Assertion("fcb-confirmation-native-roll",
                "firearm shots receive only the excess of the earned bonus over Critical Focus on CriticalConfirmationBonus; melee attacks (including the Pistol-Whip surrogate) receive nothing; attack bonus and critical edge are unchanged",
                Describe(evidence["confirmation"], failures["confirmation"]),
                failures["confirmation"].Count == 0, "RuleAttackRoll.CriticalConfirmationBonus"));
            assertions.Add(Assertion("fcb-pistol-whip-native-attack",
                "the Pistol-Whip deed attack bonus rises by exactly the earned steps with the one- and two-handed stand-ins, a Broken firearm and the off hand, while an ordinary firearm shot and the trip CMB (natural 20s, so every compared deed trips) are unchanged; a Wrecked firearm or two firearms are refused without spending grit; True Grit makes it free at 1 grit and refuses it at 0",
                Describe(evidence["pistolWhip"], failures["pistolWhip"]), failures["pistolWhip"].Count == 0,
                "PistolWhipRuntime.ExecuteForRuntimeTest; RuleAttackRoll.AttackBonus; RuleCombatManeuver"));
            assertions.Add(Assertion("fcb-nimble-native-ac",
                "Nimble improvements add Dodge AC only with Nimble, only in light or no armor, never to flat-footed AC, and respect the cap",
                Describe(evidence["nimble"], failures["nimble"]), failures["nimble"].Count == 0,
                "Stats.AC.ModifiedValue and FlatFooted"));
            assertions.Add(Assertion("fcb-dodge-native-ac",
                "the Dodge branch raises only the Gunslinger's Dodge buff's own bonus while it is active; the leaf alone grants nothing",
                Describe(evidence["dodge"], failures["dodge"]), failures["dodge"].Count == 0,
                "GunslingerDodgeArmorClassBonus through the exact Dodge buff"));
            assertions.Add(Assertion("fcb-initiative-native-rule",
                "the Initiative branch raises the deed's bonus only while the deed applies: positive grit, or 0 grit with True Grit (Initiative); neither the deed nor the improvement at 0 grit without it",
                Describe(evidence["initiative"], failures["initiative"]), failures["initiative"].Count == 0,
                "RuleInitiativeRoll with the deed's IUnitInitiativeHandler"));
            assertions.Add(Assertion("fcb-maneuver-native-cmb",
                "trip and the three dirty tricks gain exactly the earned steps; bull rush, grapple and disarm are unchanged",
                Describe(evidence["maneuver"], failures["maneuver"]), failures["maneuver"].Count == 0,
                "RuleCalculateCMB"));
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

        /// <summary>
        /// Removes a leaf with every rank: the native FeatureCollection.RemoveFact
        /// takes away only one rank while the rank is above one.
        /// </summary>
        private static void RemoveFavoredClassRanks(UnitEntityData unit, BlueprintFeature leaf)
        {
            if (leaf == null) return;
            for (int guard = 0; guard < 64 && unit.Descriptor.HasFact(leaf); guard++)
                unit.Descriptor.RemoveFact(leaf);
            if (unit.Descriptor.HasFact(leaf))
                throw new InvalidOperationException("The favored-class leaf kept ranks after removal: " + leaf.name);
        }

        private static int GrantFavoredClassRanks(UnitEntityData unit, BlueprintFeature leaf, int ranks)
        {
            if (ranks <= 0)
                return 0;
            unit.Descriptor.AddFact(leaf);
            var feature = unit.Descriptor.Progression.Features.GetFact(leaf) as Feature;
            if (feature == null)
                throw new InvalidOperationException("The favored-class leaf was not added: " + leaf.name);
            while (feature.GetRank() < ranks)
            {
                int rank = feature.GetRank();
                feature.AddRank();
                if (feature.GetRank() == rank)
                    throw new InvalidOperationException("The leaf rank stopped at " + rank + ": " + leaf.name);
            }
            return feature.GetRank();
        }

        // M07: the authoritative threshold (base, Broken with or without Gun
        // Training, ammunition, Reliable) less the wielder's reduction for the
        // fired weapon's type, applied last with a floor of 1 that never
        // raises a 0; every consumer of the threshold (ordinary shots from
        // either hand, Dead Shot, Scatter Shot) agrees. Each case is a native
        // attack with a seeded natural d20; its expectation is the item's
        // condition afterwards (a misfire breaks a Normal firearm and wrecks
        // a Broken one).
        private static JObject ObserveFavoredClassMisfire(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject();
            var cases = new JArray();
            UnitEntityData control = create();
            UnitEntityData invested = create();
            UnitEntityData trainedControl = create();
            UnitEntityData trainedInvested = create();
            UnitEntityData target = create();
            target.Descriptor.State.Immortality.Retain();
            AmmunitionId paper = ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition;
            AmmunitionId loose = ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition;
            BlueprintFeature pistolRank = leaves.Pair(FavoredClassCatalog.EffectMisfire, "Pistol").Full;
            GrantFavoredClassRanks(invested, pistolRank, 1);
            GrantFavoredClassRanks(trainedInvested, pistolRank, 1);
            foreach (UnitEntityData unit in new[] { trainedControl, trainedInvested })
                unit.Descriptor.AddFact(gunslinger.GunTraining.ChoiceFor(FirearmKind.Pistol));
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintItemWeapon musket = BlueprintBootstrap.ProductionFirearms.Musket.Item;
            BlueprintItemWeapon blunderbuss = BlueprintBootstrap.ProductionFirearms.Blunderbuss.Item;
            BlueprintItemWeapon reliablePistol = BlueprintBootstrap.MagicFirearms.Entries[3].Item;
            BlueprintItemWeapon reliableMusket = BlueprintBootstrap.MagicFirearms.Entries[4].Item;
            Action<string, UnitEntityData, BlueprintItemWeapon, FirearmCondition, AmmunitionId, int,
                FirearmCondition> shot = (label, attacker, blueprint, condition, ammunition, roll, expected) =>
            {
                var weapon = new ItemEntityWeapon(blueprint);
                RuleAttackRoll attack = TriggerReliableMatrixAttack(attacker, target, weapon, roll, condition,
                    ammunition);
                FirearmCondition after = FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State.Condition;
                FirearmRuntimeState.Service.Forget(weapon);
                attacker.Body.PrimaryHand.RemoveItem(false);
                FcbMisfireCase(cases, failures, label, roll, after, expected, attack.IsHit);
            };

            // Ordinary pistol shots: paper +1, loose +0.
            shot("pistol paper control", control, pistol, FirearmCondition.Normal, paper, 2, FirearmCondition.Broken);
            shot("pistol paper rank 1", invested, pistol, FirearmCondition.Normal, paper, 2, FirearmCondition.Normal);
            shot("pistol paper rank 1", invested, pistol, FirearmCondition.Normal, paper, 1, FirearmCondition.Broken);
            shot("pistol loose control", control, pistol, FirearmCondition.Normal, loose, 1, FirearmCondition.Broken);
            // The floor: 1 - 1 would be 0, but the reduction never goes below 1.
            shot("pistol loose rank 1 (floor)", invested, pistol, FirearmCondition.Normal, loose, 1,
                FirearmCondition.Broken);
            shot("pistol loose rank 1", invested, pistol, FirearmCondition.Normal, loose, 2, FirearmCondition.Normal);
            // Broken: +4 untrained, +2 with Gun Training (pistol); the reduction
            // follows every increase. A Broken misfire wrecks the firearm.
            shot("broken pistol paper untrained control", control, pistol, FirearmCondition.Broken, paper, 6,
                FirearmCondition.Wrecked);
            shot("broken pistol paper untrained rank 1", invested, pistol, FirearmCondition.Broken, paper, 6,
                FirearmCondition.Broken);
            shot("broken pistol paper untrained rank 1", invested, pistol, FirearmCondition.Broken, paper, 5,
                FirearmCondition.Wrecked);
            shot("broken pistol paper trained control", trainedControl, pistol, FirearmCondition.Broken, paper, 4,
                FirearmCondition.Wrecked);
            shot("broken pistol paper trained rank 1", trainedInvested, pistol, FirearmCondition.Broken, paper, 4,
                FirearmCondition.Broken);
            shot("broken pistol paper trained rank 1", trainedInvested, pistol, FirearmCondition.Broken, paper, 3,
                FirearmCondition.Wrecked);
            // Reliable (Duelist's Rebuttal): loose 1 - 1 = 0 stays 0 (a
            // natural 1 misses without a misfire); paper 1 + 1 - 1 = 1 keeps
            // the floor of 1 after the reduction.
            shot("reliable pistol loose control", control, reliablePistol, FirearmCondition.Normal, loose, 1,
                FirearmCondition.Normal);
            shot("reliable pistol loose rank 1 (0 stays 0)", invested, reliablePistol, FirearmCondition.Normal,
                loose, 1, FirearmCondition.Normal);
            shot("reliable pistol paper control", control, reliablePistol, FirearmCondition.Normal, paper, 1,
                FirearmCondition.Broken);
            shot("reliable pistol paper rank 1 (floor)", invested, reliablePistol, FirearmCondition.Normal, paper, 1,
                FirearmCondition.Broken);
            // Type isolation: the pistol counter reaches neither a musket
            // (2 + 1 = 3) nor a blunderbuss (2 + 1 = 3).
            shot("musket paper pistol rank", invested, musket, FirearmCondition.Normal, paper, 3,
                FirearmCondition.Broken);
            shot("blunderbuss paper pistol rank", invested, blunderbuss, FirearmCondition.Normal, paper, 3,
                FirearmCondition.Broken);
            GrantFavoredClassRanks(invested, leaves.Pair(FavoredClassCatalog.EffectMisfire, "Musket").Full, 1);
            shot("musket paper musket rank", invested, musket, FirearmCondition.Normal, paper, 3,
                FirearmCondition.Normal);
            shot("musket paper musket rank", invested, musket, FirearmCondition.Normal, paper, 2,
                FirearmCondition.Broken);
            // Reliable musket (River King's Measure): 2 + 1 - 1 = 2, less 1.
            shot("reliable musket paper control", control, reliableMusket, FirearmCondition.Normal, paper, 2,
                FirearmCondition.Broken);
            shot("reliable musket paper musket rank", invested, reliableMusket, FirearmCondition.Normal, paper, 2,
                FirearmCondition.Normal);
            // Separate counters never add up: the pistol still reduces by 1.
            shot("pistol loose both ranks (floor)", invested, pistol, FirearmCondition.Normal, loose, 1,
                FirearmCondition.Broken);
            row["reduction"] = "Pistol=" + FavoredClassEarnedSteps.MisfireReduction(invested, FirearmKind.Pistol) +
                ";Musket=" + FavoredClassEarnedSteps.MisfireReduction(invested, FirearmKind.Musket) +
                ";Blunderbuss=" + FavoredClassEarnedSteps.MisfireReduction(invested, FirearmKind.Blunderbuss);
            if ((string)row["reduction"] != "Pistol=1;Musket=1;Blunderbuss=0")
                failures.Add("per-type reductions " + row["reduction"]);

            // The off hand: the reduction follows the fired firearm's type,
            // not the hand.
            foreach (UnitEntityData attacker in new[] { control, invested })
            {
                bool isControl = ReferenceEquals(attacker, control);
                var main = new ItemEntityWeapon(pistol);
                var off = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(main);
                attacker.Body.SecondaryHand.InsertItem(off);
                FirearmRuntimeState.Service.Set(off, new FirearmState(FirearmState.CurrentSchemaVersion, 1, paper,
                    FirearmCondition.Normal));
                var attack = new RuleAttackRoll(attacker, target, off, -100);
                UnityEngine.Random.InitState(FindNativeD20Seed(2));
                Rulebook.Trigger(attack);
                FirearmCondition after = FirearmRuntimeState.Service.GetOrCreate(off).Repository.State.Condition;
                FirearmRuntimeState.Service.Forget(off);
                FirearmRuntimeState.Service.Forget(main);
                attacker.Body.SecondaryHand.RemoveItem(false);
                attacker.Body.PrimaryHand.RemoveItem(false);
                FcbMisfireCase(cases, failures, "off-hand pistol paper " + (isControl ? "control" : "rank 1"), 2,
                    after, isControl ? FirearmCondition.Broken : FirearmCondition.Normal, attack.IsHit);
            }

            // Dead Shot: one aggregate misfire when every roll misfires.
            UnitEntityData deadControl = create(), deadInvested = create();
            GrantFavoredClassRanks(deadInvested, pistolRank, 1);
            foreach (UnitEntityData unit in new[] { deadControl, deadInvested })
            {
                unit.Descriptor.Stats.BaseAttackBonus.BaseValue = 6;
                unit.Descriptor.Stats.Wisdom.BaseValue = 14;
                unit.Descriptor.AddFact(gunslinger.Grit.Feature);
            }
            Action<string, UnitEntityData, int, FirearmCondition> deadShot = (label, attacker, roll, expected) =>
            {
                var weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                    paper, FirearmCondition.Normal));
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 1);
                Deeds.DeadShotExecutionResult result = Deeds.DeadShotRuntime.ExecuteForRuntimeTest(attacker,
                    target, roll, roll);
                FirearmCondition after = FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State.Condition;
                FirearmRuntimeState.Service.Forget(weapon);
                attacker.Body.PrimaryHand.RemoveItem(false);
                FcbMisfireCase(cases, failures, label + " (misfires=" + result.Outcome.Misfires + ")", roll, after,
                    expected, result.Outcome.IsHit);
            };
            deadShot("dead shot paper control rolls 2,2", deadControl, 2, FirearmCondition.Broken);
            deadShot("dead shot paper rank 1 rolls 2,2", deadInvested, 2, FirearmCondition.Normal);
            deadShot("dead shot paper rank 1 rolls 1,1 (floor)", deadInvested, 1, FirearmCondition.Broken);

            // Scatter Shot: the volley's one threshold decides every roll.
            UnitEntityData scatterControl = create(), scatterInvested = create();
            GrantFavoredClassRanks(scatterInvested, leaves.Pair(FavoredClassCatalog.EffectMisfire,
                "Blunderbuss").Full, 1);
            FcbScatterMisfire(create, scatterControl, blunderbuss, paper, 3, FirearmCondition.Broken,
                "scatter paper control rolls 3,3", cases, failures);
            FcbScatterMisfire(create, scatterInvested, blunderbuss, paper, 3, FirearmCondition.Normal,
                "scatter paper blunderbuss rank rolls 3,3", cases, failures);
            FcbScatterMisfire(create, scatterInvested, blunderbuss, paper, 2, FirearmCondition.Broken,
                "scatter paper blunderbuss rank rolls 2,2", cases, failures);
            row["cases"] = cases;
            return row;
        }

        private static void FcbMisfireCase(JArray cases, IList<string> failures, string label, int roll,
            FirearmCondition after, FirearmCondition expected, bool hit)
        {
            cases.Add(label + " roll " + roll + ": " + after + (hit ? " hit" : " miss") +
                (after == expected ? "" : " (expected " + expected + ")"));
            if (after != expected)
                failures.Add(label + " roll " + roll + ": " + after + ", expected " + expected);
        }

        // Scatter Shot against two live disposable targets in the attacker's
        // cone, both rolls forced to the same natural value.
        private static void FcbScatterMisfire(Func<UnitEntityData> create, UnitEntityData attacker,
            BlueprintItemWeapon blunderbuss, AmmunitionId ammunition, int roll, FirearmCondition expected,
            string label, JArray cases, IList<string> failures)
        {
            UnitEntityData first = create(), second = create();
            bool firstRegistered = false, secondRegistered = false;
            var weapon = new ItemEntityWeapon(blunderbuss);
            try
            {
                first.Descriptor.State.Immortality.Retain();
                second.Descriptor.State.Immortality.Retain();
                var origin = new UnityEngine.Vector3(12000f, 0f, 12000f);
                SetExactProperty(attacker, "Position", origin);
                SetExactProperty(first, "Position", origin + new UnityEngine.Vector3(2f, 0f, 0.3f));
                SetExactProperty(second, "Position", origin + new UnityEngine.Vector3(3f, 0f, -0.3f));
                attacker.Body.PrimaryHand.InsertItem(weapon);
                SetExactProperty(attacker, "CombatState",
                    new Kingmaker.Controllers.Combat.UnitCombatState(attacker));
                firstRegistered = Kingmaker.Game.Instance.State.Units.All.Add(first);
                secondRegistered = Kingmaker.Game.Instance.State.Units.All.Add(second);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                    ammunition, FirearmCondition.Normal));
                int[] rolls = { roll, roll };
                int index = 0;
                Scatter.ScatterShotExecutionResult result = Scatter.ScatterShotRuntime.Execute(attacker, first,
                    attack =>
                    {
                        UnityEngine.Random.InitState(FindNativeD20Seed(rolls[index++]));
                        Rulebook.Trigger(attack);
                    }, rolls);
                FirearmCondition after = FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State.Condition;
                FcbMisfireCase(cases, failures, label + " (targets=" + result.Plan.TargetCount + ", misfires=" +
                    result.Volley.MisfireRollCount + ")", roll, after, expected, false);
                if (result.Plan.TargetCount != 2)
                    failures.Add(label + ": " + result.Plan.TargetCount + " scatter targets, expected 2");
            }
            finally
            {
                FirearmRuntimeState.Service.Forget(weapon);
                if (attacker.Body.PrimaryHand.MaybeItem != null) attacker.Body.PrimaryHand.RemoveItem(false);
                SetExactProperty(attacker, "CombatState", null);
                if (secondRegistered) Kingmaker.Game.Instance.State.Units.All.Remove(second);
                if (firstRegistered) Kingmaker.Game.Instance.State.Units.All.Remove(first);
                first.Descriptor.State.Immortality.ReleaseAll();
                second.Descriptor.State.Immortality.ReleaseAll();
            }
        }

        private static JObject ObserveFavoredClassConfirmation(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject();
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectFirearmConfirmation, null);
            BlueprintFeature criticalFocus = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                BlueprintBootstrap.Library, FavoredClassBlueprints.CriticalFocusGuid, "Critical Focus");
            row["criticalFocus"] = criticalFocus.name;
            UnitEntityData target = create();
            target.Descriptor.State.Immortality.Retain();
            Func<UnitEntityData, BlueprintItemWeapon, RuleAttackRoll> attack = (attacker, blueprint) =>
            {
                var weapon = new ItemEntityWeapon(blueprint);
                bool firearm = blueprint.Type != null && blueprint.Type.ComponentsArray
                    .OfType<FirearmDefinitionComponent>().Any();
                if (attacker.Body.PrimaryHand.MaybeItem != null)
                    attacker.Body.PrimaryHand.RemoveItem(false);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                if (firearm)
                    FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion,
                        1, FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
                var roll = new RuleAttackRoll(attacker, target, weapon, 0);
                UnityEngine.Random.InitState(FindNativeD20Seed(15));
                Rulebook.Trigger(roll);
                if (firearm)
                    FirearmRuntimeState.Service.Forget(weapon);
                attacker.Body.PrimaryHand.RemoveItem(false);
                return roll;
            };
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintItemWeapon surrogate = gunslinger.PistolWhip.OneHandedItem;
            UnitEntityData control = create();
            UnitEntityData five = create();
            UnitEntityData two = create();
            GrantFavoredClassRanks(five, pair.Full, 5);
            GrantFavoredClassRanks(two, pair.Full, 2);
            RuleAttackRoll controlShot = attack(control, pistol);
            RuleAttackRoll fiveShot = attack(five, pistol);
            RuleAttackRoll twoShot = attack(two, pistol);
            RuleAttackRoll fiveMelee = attack(five, surrogate);
            RuleAttackRoll controlMelee = attack(control, surrogate);
            control.Descriptor.AddFact(criticalFocus);
            five.Descriptor.AddFact(criticalFocus);
            two.Descriptor.AddFact(criticalFocus);
            RuleAttackRoll focusShot = attack(control, pistol);
            RuleAttackRoll focusFiveShot = attack(five, pistol);
            RuleAttackRoll focusTwoShot = attack(two, pistol);
            Func<RuleAttackRoll, string> describe = value => "confirm=" + value.CriticalConfirmationBonus +
                ";attack=" + value.AttackBonus + ";edge=" + value.WeaponStats.CriticalEdge + ";type=" +
                value.AttackType;
            row["control"] = describe(controlShot);
            row["rank5"] = describe(fiveShot);
            row["rank2"] = describe(twoShot);
            row["rank5Melee"] = describe(fiveMelee);
            row["controlMelee"] = describe(controlMelee);
            row["focus"] = describe(focusShot);
            row["focusRank5"] = describe(focusFiveShot);
            row["focusRank2"] = describe(focusTwoShot);
            int focusBonus = focusShot.CriticalConfirmationBonus - controlShot.CriticalConfirmationBonus;
            row["criticalFocusContribution"] = focusBonus;
            if (fiveShot.CriticalConfirmationBonus - controlShot.CriticalConfirmationBonus != 5)
                failures.Add("rank 5 did not add +5 to a firearm confirmation without Critical Focus");
            if (twoShot.CriticalConfirmationBonus - controlShot.CriticalConfirmationBonus != 2)
                failures.Add("rank 2 did not add +2");
            if (fiveMelee.CriticalConfirmationBonus != controlMelee.CriticalConfirmationBonus)
                failures.Add("a melee (Pistol-Whip surrogate) attack received the firearm bonus");
            if (focusBonus <= 0)
                failures.Add("Critical Focus contributed nothing; the nonstacking comparison is unobservable");
            if (focusFiveShot.CriticalConfirmationBonus - controlShot.CriticalConfirmationBonus !=
                Math.Max(5, focusBonus))
                failures.Add("with Critical Focus the rank-5 total is not the better of the two");
            if (focusTwoShot.CriticalConfirmationBonus - controlShot.CriticalConfirmationBonus !=
                Math.Max(2, focusBonus))
                failures.Add("with Critical Focus the rank-2 total is not the better of the two");
            if (fiveShot.AttackBonus != controlShot.AttackBonus ||
                fiveShot.WeaponStats.CriticalEdge != controlShot.WeaponStats.CriticalEdge)
                failures.Add("the attack bonus or critical edge changed");
            return row;
        }

        // M10: the deed's own attack roll gains exactly the earned steps
        // however the deed is made (one- or two-handed stand-in, a Broken
        // firearm, the off hand), and neither an ordinary shot nor the trip's
        // CMB does; a Wrecked firearm or two firearms are refused without
        // spending grit; True Grit makes it free at 1 grit and refuses it at
        // 0. Natural 20s hit, so every compared deed also trips.
        private static JObject ObserveFavoredClassPistolWhip(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject();
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectPistolWhip, null);
            UnitEntityData target = create();
            target.Descriptor.State.Immortality.Retain();
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintAbilityResource grit = gunslinger.Grit.Resource;
            Action<UnitEntityData, int> setGrit = (unit, amount) =>
            {
                unit.Descriptor.Resources.Spend(grit, unit.Descriptor.Resources.GetResourceAmount(grit));
                if (amount > 0) unit.Descriptor.Resources.Restore(grit, amount);
            };
            Func<UnitEntityData, BlueprintItemWeapon, FirearmCondition, bool, bool, PistolWhipResult> whip =
                (attacker, firearm, condition, offHand, twoFirearms) =>
                {
                    var weapon = new ItemEntityWeapon(firearm);
                    var second = twoFirearms ? new ItemEntityWeapon(pistol) : null;
                    if (attacker.Body.PrimaryHand.MaybeItem != null) attacker.Body.PrimaryHand.RemoveItem(false);
                    if (attacker.Body.SecondaryHand.MaybeItem != null) attacker.Body.SecondaryHand.RemoveItem(false);
                    (offHand ? attacker.Body.SecondaryHand : attacker.Body.PrimaryHand).InsertItem(weapon);
                    if (second != null) attacker.Body.SecondaryHand.InsertItem(second);
                    try
                    {
                        FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion,
                            1, FirearmStateTokenCatalog.DiagnosticLeadBall, condition));
                        if (second != null)
                            FirearmRuntimeState.Service.Set(second, new FirearmState(FirearmState.CurrentSchemaVersion,
                                1, FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
                        UnityEngine.Random.InitState(FindNativeD20Seed(20));
                        return PistolWhipRuntime.ExecuteForRuntimeTest(attacker, target,
                            gunslinger.PistolWhip.OneHandedItem, gunslinger.PistolWhip.TwoHandedItem, false);
                    }
                    finally
                    {
                        FirearmRuntimeState.Service.Forget(weapon);
                        if (second != null) FirearmRuntimeState.Service.Forget(second);
                        if (attacker.Body.PrimaryHand.MaybeItem != null) attacker.Body.PrimaryHand.RemoveItem(false);
                        if (attacker.Body.SecondaryHand.MaybeItem != null) attacker.Body.SecondaryHand.RemoveItem(false);
                    }
                };
            UnitEntityData control = create();
            UnitEntityData invested = create();
            foreach (UnitEntityData unit in new[] { control, invested })
            {
                unit.Descriptor.Stats.Wisdom.BaseValue = 14;
                unit.Descriptor.AddFact(gunslinger.Grit.Feature);
            }
            int granted = GrantFavoredClassRanks(invested, pair.Full, 3);
            int earned = FavoredClassRankPolicy.BenefitSteps(pair.Effect.Rate, granted);
            row["earnedSteps"] = earned;
            Func<PistolWhipResult, string> describe = value => value == null ? "<none>" :
                value.Attack == null || value.Attack.AttackRoll == null ? "refused " + value.Decision.Status :
                "attack=" + value.Attack.AttackRoll.AttackBonus + ";natural=" + (int)value.Attack.AttackRoll.Roll +
                ";hit=" + value.Hit + ";surrogate=" + value.Attack.Weapon.Blueprint.name + ";enhancement=" +
                value.Enhancement + ";trip=" + (value.Trip == null ? "none" : value.Trip.InitiatorCMB.ToString());
            var cases = new JObject();
            // Each eligible form: control and invested, with grit 1 each time.
            foreach (var form in new[]
            {
                Tuple.Create("pistol", pistol, FirearmCondition.Normal, false, false),
                Tuple.Create("musket (two-handed stand-in)", BlueprintBootstrap.ProductionFirearms.Musket.Item,
                    FirearmCondition.Normal, false, true),
                Tuple.Create("broken pistol", pistol, FirearmCondition.Broken, false, false),
                Tuple.Create("off-hand pistol", pistol, FirearmCondition.Normal, true, false),
            })
            {
                setGrit(control, 1);
                setGrit(invested, 1);
                PistolWhipResult controlWhip = whip(control, form.Item2, form.Item3, form.Item4, false);
                PistolWhipResult investedWhip = whip(invested, form.Item2, form.Item3, form.Item4, false);
                cases[form.Item1] = "control{" + describe(controlWhip) + "};invested{" + describe(investedWhip) + "}";
                if (controlWhip == null || investedWhip == null || controlWhip.Attack == null ||
                    investedWhip.Attack == null || controlWhip.Attack.AttackRoll == null ||
                    investedWhip.Attack.AttackRoll == null)
                {
                    failures.Add(form.Item1 + ": a Pistol-Whip attack was not executed");
                    continue;
                }
                if (investedWhip.Attack.AttackRoll.AttackBonus - controlWhip.Attack.AttackRoll.AttackBonus != earned)
                    failures.Add(form.Item1 + ": the deed attack bonus did not rise by exactly the earned steps");
                if (investedWhip.Decision.TwoHanded != form.Item5 || !ReferenceEquals(investedWhip.Attack.Weapon.Blueprint,
                        form.Item5 ? gunslinger.PistolWhip.TwoHandedItem : gunslinger.PistolWhip.OneHandedItem))
                    failures.Add(form.Item1 + ": the wrong stand-in weapon was used");
                if (controlWhip.Trip == null || investedWhip.Trip == null)
                    failures.Add(form.Item1 + ": a natural 20 did not trip, so the CMB comparison is vacuous");
                else if (controlWhip.Trip.InitiatorCMB != investedWhip.Trip.InitiatorCMB)
                    failures.Add(form.Item1 + ": the trip CMB changed");
            }
            // Refusals spend no grit and make no attack.
            foreach (var refusal in new[]
            {
                Tuple.Create("wrecked pistol", FirearmCondition.Wrecked, false, PistolWhipStatus.Wrecked),
                Tuple.Create("two firearms", FirearmCondition.Normal, true, PistolWhipStatus.NotExactEquippedFirearm),
            })
            {
                setGrit(invested, 1);
                PistolWhipResult refused = whip(invested, pistol, refusal.Item2, false, refusal.Item3);
                int left = invested.Descriptor.Resources.GetResourceAmount(grit);
                cases[refusal.Item1] = describe(refused) + ";gritLeft=" + left;
                if (refused == null || refused.Decision.Status != refusal.Item4 || refused.Attack != null || left != 1)
                    failures.Add(refusal.Item1 + ": not refused exactly without spending grit");
            }
            // True Grit (Pistol-Whip): free at 1 grit, refused at 0.
            BlueprintFeature trueGrit = gunslinger.TrueGrit.ChoiceFor(TrueGritDeed.PistolWhip);
            invested.Descriptor.AddFact(trueGrit);
            setGrit(invested, 1);
            PistolWhipResult free = whip(invested, pistol, FirearmCondition.Normal, false, false);
            int afterFree = invested.Descriptor.Resources.GetResourceAmount(grit);
            setGrit(invested, 0);
            PistolWhipResult empty = whip(invested, pistol, FirearmCondition.Normal, false, false);
            invested.Descriptor.RemoveFact(trueGrit);
            cases["true grit at 1 grit"] = describe(free) + ";gritLeft=" + afterFree;
            cases["true grit at 0 grit"] = describe(empty);
            if (free == null || free.Attack == null || afterFree != 1)
                failures.Add("True Grit Pistol-Whip at 1 grit was not free");
            if (empty == null || empty.Decision.Status != PistolWhipStatus.InsufficientGrit || empty.Attack != null)
                failures.Add("True Grit Pistol-Whip at 0 grit was not refused");
            // Observation for review (tabletop Pistol-Whip adds the firearm's
            // enhancement bonus to attack and damage): the Pistol +1 deed's
            // attack bonus against the plain pistol's, both invested.
            setGrit(invested, 1);
            PistolWhipResult enhanced = whip(invested, BlueprintBootstrap.MagicFirearms.Entries[0].Item,
                FirearmCondition.Normal, false, false);
            setGrit(invested, 1);
            PistolWhipResult plain = whip(invested, pistol, FirearmCondition.Normal, false, false);
            cases["enhanced pistol (observation)"] = "plus1{" + describe(enhanced) + "};plain{" + describe(plain) + "}";
            if (enhanced != null && plain != null && enhanced.Attack != null && plain.Attack != null)
                row["enhancedPistolAttackDelta"] = enhanced.Attack.AttackRoll.AttackBonus - plain.Attack.AttackRoll.AttackBonus;
            row["cases"] = cases;
            // No leakage into an ordinary shot.
            RuleAttackRoll controlShot = FireOrdinaryShot(control, target);
            RuleAttackRoll investedShot = FireOrdinaryShot(invested, target);
            row["controlShot"] = controlShot.AttackBonus;
            row["investedShot"] = investedShot.AttackBonus;
            if (investedShot.AttackBonus != controlShot.AttackBonus)
                failures.Add("an ordinary firearm shot received the Pistol-Whip bonus");
            return row;
        }

        private static RuleAttackRoll FireOrdinaryShot(UnitEntityData attacker, UnitEntityData target)
        {
            var weapon = new ItemEntityWeapon(BlueprintBootstrap.ProductionFirearms.Pistol.Item);
            RuleAttackRoll roll = TriggerReliableMatrixAttack(attacker, target, weapon, 15,
                FirearmCondition.Normal);
            FirearmRuntimeState.Service.Forget(weapon);
            attacker.Body.PrimaryHand.RemoveItem(false);
            return roll;
        }

        private static JObject ObserveFavoredClassNimble(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject();
            BlueprintItemArmor mediumArmor = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintItemArmor>().Where(value => value.Type != null && value.Type.IsArmor &&
                    value.Type.ProficiencyGroup == Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Medium)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
            FavoredClassLeafPair halfling = leaves.Pair(FavoredClassCatalog.EffectHalflingNimble, null);
            FavoredClassLeafPair drow = leaves.Pair(FavoredClassCatalog.EffectDrowNimble, null);
            BlueprintFeature nimble = gunslinger.Nimble.Features[0];

            UnitEntityData dormant = create();
            int dormantBase = dormant.Descriptor.Stats.AC.ModifiedValue;
            GrantFavoredClassRanks(dormant, halfling.Full, 2);
            int dormantWith = dormant.Descriptor.Stats.AC.ModifiedValue;
            row["dormantDelta"] = dormantWith - dormantBase;
            if (dormantWith != dormantBase)
                failures.Add("the leaf granted AC without Nimble");

            UnitEntityData unit = create();
            unit.Descriptor.AddFact(nimble);
            int nimbleOnly = unit.Descriptor.Stats.AC.ModifiedValue;
            int nimbleFlat = unit.Descriptor.Stats.AC.FlatFooted;
            GrantFavoredClassRanks(unit, halfling.Full, 2);
            int withRanks = unit.Descriptor.Stats.AC.ModifiedValue;
            int withFlat = unit.Descriptor.Stats.AC.FlatFooted;
            unit.Body.Armor.InsertItem(new ItemEntityArmor(mediumArmor));
            int mediumWith = unit.Descriptor.Stats.AC.ModifiedValue;
            unit.Descriptor.RemoveFact(halfling.Full);
            int mediumWithout = unit.Descriptor.Stats.AC.ModifiedValue;
            unit.Body.Armor.RemoveItem(false);
            row["halflingDelta"] = withRanks - nimbleOnly;
            row["flatFootedDelta"] = withFlat - nimbleFlat;
            row["mediumArmorDelta"] = mediumWith - mediumWithout;
            if (withRanks - nimbleOnly != 2) failures.Add("two Halfling ranks did not add +2 Nimble AC");
            if (withFlat != nimbleFlat) failures.Add("flat-footed AC included the improvement");
            if (mediumWith != mediumWithout) failures.Add("medium armor kept the improvement");

            UnitEntityData drowUnit = create();
            drowUnit.Descriptor.AddFact(nimble);
            int drowBase = drowUnit.Descriptor.Stats.AC.ModifiedValue;
            GrantFavoredClassRanks(drowUnit, drow.Full, 2);
            row["drowDelta"] = drowUnit.Descriptor.Stats.AC.ModifiedValue - drowBase;
            row["drowRanks"] = drow.Full.Ranks;
            if ((int)row["drowDelta"] != 2 || drow.Full.Ranks != 2)
                failures.Add("the Drow counter is not +1 per full rank with a cap of 2");
            return row;
        }

        private static JObject ObserveFavoredClassDodge(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject();
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectHalflingDodge, null);
            UnitEntityData control = create();
            UnitEntityData invested = create();
            GrantFavoredClassRanks(invested, pair.Full, 3);
            int controlBase = control.Descriptor.Stats.AC.ModifiedValue;
            int investedBase = invested.Descriptor.Stats.AC.ModifiedValue;
            row["leafAloneDelta"] = investedBase - controlBase;
            control.Descriptor.Buffs.AddBuff(gunslinger.Dodge.ArmorClassBuff, control, TimeSpan.FromSeconds(6));
            invested.Descriptor.Buffs.AddBuff(gunslinger.Dodge.ArmorClassBuff, invested, TimeSpan.FromSeconds(6));
            int controlActive = control.Descriptor.Stats.AC.ModifiedValue - controlBase;
            int investedActive = invested.Descriptor.Stats.AC.ModifiedValue - investedBase;
            control.Descriptor.RemoveFact(gunslinger.Dodge.ArmorClassBuff);
            invested.Descriptor.RemoveFact(gunslinger.Dodge.ArmorClassBuff);
            int investedAfter = invested.Descriptor.Stats.AC.ModifiedValue - investedBase;
            row["controlActiveDelta"] = controlActive;
            row["investedActiveDelta"] = investedActive;
            row["investedAfterRemovalDelta"] = investedAfter;
            if (investedBase != controlBase) failures.Add("the Dodge leaf alone granted AC");
            if (controlActive != GunslingerDodgeArmorClassBonus.Bonus)
                failures.Add("the control Dodge buff did not grant its own +2");
            if (investedActive != GunslingerDodgeArmorClassBonus.Bonus + 3)
                failures.Add("three Dodge ranks did not raise the active bonus to +5");
            if (investedAfter != 0) failures.Add("the improvement outlived the Dodge buff");
            return row;
        }

        private static JObject ObserveFavoredClassInitiative(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject();
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectInitiative, null);
            // Measured against the unit's own Initiative stat after both the
            // rule and the initiative handlers ran, so the result does not
            // depend on which of the two points applies the deed.
            Func<UnitEntityData, int> roll = unit =>
            {
                var rule = new RuleInitiativeRoll(unit);
                Rulebook.Trigger(rule);
                EventBus.RaiseEvent<IUnitInitiativeHandler>(handler =>
                    handler.HandleUnitRollsInitiative(rule));
                return rule.Modifier - unit.Stats.Initiative.ModifiedValue;
            };
            UnitEntityData control = create();
            UnitEntityData invested = create();
            foreach (UnitEntityData unit in new[] { control, invested })
            {
                unit.Descriptor.Stats.Wisdom.BaseValue = 14;
                unit.Descriptor.AddFact(gunslinger.Grit.Feature);
                unit.Descriptor.AddFact(gunslinger.Initiative);
            }
            GrantFavoredClassRanks(invested, pair.Full, 4);
            int controlDelta = roll(control);
            int investedDelta = roll(invested);
            foreach (UnitEntityData unit in new[] { control, invested })
                unit.Descriptor.Resources.Spend(gunslinger.Grit.Resource,
                    unit.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource));
            int emptyDelta = roll(invested);
            int controlEmptyDelta = roll(control);
            // M13: True Grit (Initiative) lets the deed work at 0 grit, and the
            // improvement follows the deed's own predicate.
            BlueprintFeature trueGrit = gunslinger.TrueGrit.ChoiceFor(TrueGritDeed.GunslingerInitiative);
            foreach (UnitEntityData unit in new[] { control, invested })
                unit.Descriptor.AddFact(trueGrit);
            int trueGritControlDelta = roll(control);
            int trueGritInvestedDelta = roll(invested);
            invested.Descriptor.RemoveFact(trueGrit);
            int removedDelta = roll(invested);
            row["controlDelta"] = controlDelta;
            row["investedDelta"] = investedDelta;
            row["zeroGritDelta"] = emptyDelta;
            row["zeroGritControlDelta"] = controlEmptyDelta;
            row["trueGritZeroGritControlDelta"] = trueGritControlDelta;
            row["trueGritZeroGritInvestedDelta"] = trueGritInvestedDelta;
            row["trueGritRemovedZeroGritDelta"] = removedDelta;
            if (controlDelta != 2) failures.Add("the control deed bonus is not +2");
            if (investedDelta != 6) failures.Add("four Initiative ranks did not raise the deed bonus to +6");
            if (emptyDelta != 0 || controlEmptyDelta != 0)
                failures.Add("the deed or the improvement applied at 0 grit without True Grit");
            if (trueGritControlDelta != 2)
                failures.Add("True Grit (Initiative) did not give the deed's +2 at 0 grit");
            if (trueGritInvestedDelta != 6)
                failures.Add("with True Grit at 0 grit the improvement did not follow the deed (+6)");
            if (removedDelta != 0)
                failures.Add("the improvement outlived the True Grit choice at 0 grit");
            return row;
        }

        private static JObject ObserveFavoredClassManeuver(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, IList<string> failures)
        {
            var row = new JObject();
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectDirtyTrickTrip, null);
            UnitEntityData control = create();
            UnitEntityData invested = create();
            UnitEntityData target = create();
            GrantFavoredClassRanks(invested, pair.Full, 3);
            foreach (CombatManeuver maneuver in new[] { CombatManeuver.Trip, CombatManeuver.DirtyTrickBlind,
                CombatManeuver.DirtyTrickEntangle, CombatManeuver.DirtyTrickSickened, CombatManeuver.BullRush,
                CombatManeuver.Grapple, CombatManeuver.Disarm })
            {
                int controlCmb = Rulebook.Trigger(new RuleCalculateCMB(control, target, maneuver)).Result;
                int investedCmb = Rulebook.Trigger(new RuleCalculateCMB(invested, target, maneuver)).Result;
                int delta = investedCmb - controlCmb;
                row[maneuver.ToString()] = delta;
                bool qualifies = FavoredClassManeuverBonus.Qualifies(maneuver);
                if (delta != (qualifies ? 3 : 0))
                    failures.Add(maneuver + " delta " + delta);
            }
            return row;
        }
    }
}
