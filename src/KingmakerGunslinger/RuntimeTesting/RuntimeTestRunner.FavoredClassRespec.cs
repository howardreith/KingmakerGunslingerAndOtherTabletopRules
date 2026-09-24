using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using KingmakerGunslinger.Gunsmithing;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>Drives the native respec's level-up controller (Player.RespecCompanion).</summary>
        private sealed class FcbRespecHandler : ILevelUpInitiateUIHandler
        {
            internal Func<LevelUpController, JObject, bool> Drive;
            internal readonly JObject Evidence = new JObject();
            internal UnitDescriptor Replacement;
            internal bool Invoked;
            internal bool Committed;
            internal Exception Failure;

            public void HandleLevelUpStart(UnitDescriptor unit, JToken unitJson, Action onSuccess,
                LevelUpState.CharBuildMode mode)
            {
                Invoked = true;
                Replacement = unit;
                Evidence["mode"] = mode.ToString();
                LevelUpController controller = null;
                try
                {
                    controller = LevelUpController.StartWithoutAssigningStaticInstance(unit, false, unitJson,
                        onSuccess, mode);
                    bool commit = Drive(controller, Evidence);
                    bool complete = controller.State.IsComplete();
                    Evidence["complete"] = complete;
                    if (commit && complete)
                    {
                        controller.Commit();
                        Committed = true;
                    }
                    else
                    {
                        if (commit)
                            Evidence["blockers"] = FavoredClassLevelUpHarness.Blockers(controller);
                        controller.Cancel();
                    }
                }
                catch (Exception exception)
                {
                    Failure = exception;
                    try { if (controller != null) controller.Cancel(); } catch (Exception) { }
                }
            }
        }

        // E16 and L03: the native respec (Player.RespecCompanion) of characters
        // whose favored-class counters were earned through native level-up
        // picks. A cancelled respec leaves everything unchanged; a committed
        // respec removes the old counters exactly once, rebuilds eligibility
        // for the new ancestry (one counter per effect, no stale human access)
        // and the new counters keep accumulating normally.
        private RuntimeTestResult RunFavoredClassRespec()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            ElementalMostlyHumanBlueprintSet mostlyHuman = BlueprintBootstrap.MostlyHuman;
            bool ready = FavoredClassIntegrationStatusRegistry.Current.Availability ==
                    FavoredClassIntegrationAvailability.Published && host != null && leaves != null &&
                gunslinger != null && host.GunslingerSelection != null && mostlyHuman != null &&
                mostlyHuman.Publication != null;
            assertions.Add(Assertion("fcb-respec-ready",
                "the exact host is published with the Gunslinger reward selection and Mostly Human is offered",
                FavoredClassIntegrationStatusRegistry.Current.ToString(), ready, "FavoredClassIntegrationStatusRegistry"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            Player player = Game.Instance.Player;
            object[] partyBefore = SnapshotReferences(ReadExactMember(player, "Party"));
            object[] unitsBefore = SnapshotReferences(ReadExactMember(ReadExactMember(Game.Instance, "State"), "AllUnits"));
            object[] inventoryBefore = SnapshotReferences(player.Inventory);
            object[] charactersBefore = SnapshotReferences(player.AllCharacters);
            long moneyBefore = player.Money;
            var library = BlueprintBootstrap.Library;
            FavoredClassLeafPair grit = leaves.Pair(FavoredClassCatalog.EffectGrit, null);
            FavoredClassLeafPair confirmation = leaves.Pair(FavoredClassCatalog.EffectFirearmConfirmation, null);
            BlueprintFeature hitPoint = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbHostHitPointRewardGuid, "host favored-class hit point");
            Func<string, BlueprintRace> race = ancestry => BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(ancestry).RaceGuid, ancestry);
            ElementalMostlyHumanRaceBlueprints ifrit = mostlyHuman.Races.Single(value =>
                ReferenceEquals(value.Race, race(FavoredClassAncestry.Ifrit)));
            var reserved = new HashSet<string>(StringComparer.Ordinal) { host.GunslingerSelection.AssetGuid };
            var evidence = new JObject();
            var cancelFailures = new List<string>();
            var commitFailures = new List<string>();
            var ancestryFailures = new List<string>();
            var mostlyHumanFailures = new List<string>();
            var powerFailures = new List<string>();
            var disposables = new List<UnitEntityData>();
            try
            {
                // Half-elf Gunslinger 5 (grit through the human alias, elf
                // confirmation): grit complete (3 partial + 1 full) and
                // confirmation incomplete (1 partial), one grit point spent.
                UnitEntityData human = BuildFcbRespecSubject(FavoredClassAncestry.HalfElf, null, new[]
                    { grit.Partial, grit.Partial, grit.Partial, grit.Full, confirmation.Partial }, reserved);
                disposables.Add(human);
                human.Descriptor.Resources.Spend(gunslinger.Grit.Resource, 1);
                JObject before = DescribeFcbRespecState(human);
                evidence["source"] = before;

                // L03 cancel: the native respec is started, driven to the new
                // ancestry's first choices and cancelled.
                JObject cancelled = RunFcbRespec(human, (controller, row) =>
                {
                    FavoredClassLevelUpHarness.Configure(controller, controller.Unit, race(FavoredClassAncestry.Human),
                        gunslinger.CharacterClass, "KMG FCB Respec", null);
                    FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger.CharacterClass, row);
                    return false;
                }, cancelFailures, "cancel");
                JObject afterCancel = DescribeFcbRespecState(human);
                cancelled["after"] = afterCancel;
                evidence["cancel"] = cancelled;
                if (!(bool)cancelled["invoked"] || (bool)cancelled["committed"] || (bool)cancelled["callback"])
                    cancelFailures.Add("the cancelled respec did not stop before commit");
                if (!JToken.DeepEquals(before, afterCancel))
                    cancelFailures.Add("the cancelled respec changed the character: " +
                        afterCancel.ToString(Newtonsoft.Json.Formatting.None));

                // L03 commit and E16 ancestry change to Human: grit stays
                // eligible (one counter), the elf confirmation does not; the
                // old complete and incomplete counters are removed.
                JObject halfElf = RunFcbRespec(human, (controller, row) =>
                {
                    FavoredClassLevelUpHarness.Configure(controller, controller.Unit, race(FavoredClassAncestry.Human),
                        gunslinger.CharacterClass, "KMG FCB Respec", null);
                    if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger.CharacterClass, row) == null)
                        throw new InvalidOperationException("the favored Gunslinger progression is unavailable");
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(controller,
                        host.GunslingerSelection.AssetGuid);
                    if (state == null)
                        throw new InvalidOperationException("no Gunslinger reward state in the respec");
                    DescribeFcbRespecOffer(controller, state, leaves, grit, confirmation, row);
                    if (!FavoredClassLevelUpHarness.Select(controller, state, grit.Partial))
                        throw new InvalidOperationException("the Human respec could not take grit");
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    return true;
                }, commitFailures, "human");
                JObject afterHalfElf = DescribeFcbRespecState(human);
                halfElf["after"] = afterHalfElf;
                evidence["human"] = halfElf;
                JObject offer = (JObject)((JObject)halfElf["drive"])["offer"];
                if (!(bool)halfElf["committed"] || !(bool)halfElf["callback"])
                    commitFailures.Add("the Human respec did not commit through the native callback");
                if (offer == null || (int)offer["gritPairs"] != 1 || (int)offer["confirmationPairs"] != 1 ||
                    !(bool)offer["gritPartialSelectable"] || (bool)offer["gritFullSelectable"] ||
                    (bool)offer["confirmationPartialSelectable"])
                    ancestryFailures.Add("the Human respec offer is not one grit counter at zero without the elf confirmation: " +
                        (offer == null ? "none" : offer.ToString(Newtonsoft.Json.Formatting.None)));
                if ((string)afterHalfElf["race"] != race(FavoredClassAncestry.Human).name ||
                    (int)afterHalfElf["gunslingerLevel"] != 1 || !OnlyCounters(afterHalfElf, grit.Partial, 1) ||
                    (int)afterHalfElf["duplicateFacts"] != 0)
                    commitFailures.Add("the committed respec did not replace the counters exactly: " +
                        afterHalfElf.ToString(Newtonsoft.Json.Formatting.None));
                // The rebuilt counters keep accumulating (stable counters).
                LevelFcbRespecSubject(human, race(FavoredClassAncestry.Human), new[] { grit.Partial, grit.Partial },
                    reserved, commitFailures, "human");
                JObject leveled = DescribeFcbRespecState(human);
                evidence["humanLeveled"] = leveled;
                if ((int)leveled["gunslingerLevel"] != 3 || !OnlyCounters(leveled, grit.Partial, 3))
                    commitFailures.Add("the rebuilt counter did not keep accumulating: " +
                        leveled.ToString(Newtonsoft.Json.Formatting.None));

                // E16: to Dwarf, human grit is no longer eligible (no stale access).
                JObject dwarf = RunFcbRespec(human, (controller, row) =>
                {
                    FavoredClassLevelUpHarness.Configure(controller, controller.Unit, race(FavoredClassAncestry.Dwarf),
                        gunslinger.CharacterClass, "KMG FCB Respec", null);
                    if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger.CharacterClass, row) == null)
                        throw new InvalidOperationException("the favored Gunslinger progression is unavailable");
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(controller,
                        host.GunslingerSelection.AssetGuid);
                    if (state == null)
                        throw new InvalidOperationException("no Gunslinger reward state in the Dwarf respec");
                    DescribeFcbRespecOffer(controller, state, leaves, grit, confirmation, row);
                    if (!FavoredClassLevelUpHarness.Select(controller, state, hitPoint))
                        throw new InvalidOperationException("the Dwarf respec could not take the hit point");
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    return true;
                }, ancestryFailures, "dwarf");
                JObject afterDwarf = DescribeFcbRespecState(human);
                dwarf["after"] = afterDwarf;
                evidence["dwarf"] = dwarf;
                JObject dwarfOffer = (JObject)((JObject)dwarf["drive"])["offer"];
                if (!(bool)dwarf["committed"] || dwarfOffer == null || (bool)dwarfOffer["gritPartialSelectable"] ||
                    (bool)dwarfOffer["confirmationPartialSelectable"])
                    ancestryFailures.Add("the Dwarf respec kept human or elf access: " +
                        (dwarfOffer == null ? "none" : dwarfOffer.ToString(Newtonsoft.Json.Formatting.None)));
                if ((string)afterDwarf["race"] != race(FavoredClassAncestry.Dwarf).name ||
                    ((JObject)afterDwarf["counters"]).Count != 0 || (int)afterDwarf["duplicateFacts"] != 0)
                    ancestryFailures.Add("the Dwarf character kept a counter: " +
                        afterDwarf.ToString(Newtonsoft.Json.Formatting.None));

                // L03 and E16 with Mostly Human: the trait's human grit and the
                // hidden identity leave with the trait.
                UnitEntityData geniekin = BuildFcbRespecSubject(FavoredClassAncestry.Ifrit, ifrit.Trait,
                    new[] { grit.Partial }, reserved);
                disposables.Add(geniekin);
                JObject mostlyHumanBefore = DescribeFcbRespecState(geniekin);
                JObject standard = RunFcbRespec(geniekin, (controller, row) =>
                {
                    FavoredClassLevelUpHarness.Configure(controller, controller.Unit, ifrit.Race,
                        gunslinger.CharacterClass, "KMG FCB Respec", null);
                    if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger.CharacterClass, row) == null)
                        throw new InvalidOperationException("the favored Gunslinger progression is unavailable");
                    FeatureSelectionState ancestry = FavoredClassLevelUpHarness.FindOpenState(controller,
                        ifrit.Selection.AssetGuid);
                    if (ancestry == null || !FavoredClassLevelUpHarness.Select(controller, ancestry, ifrit.Standard))
                        throw new InvalidOperationException("the respec could not choose the Standard ancestry");
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(controller,
                        host.GunslingerSelection.AssetGuid);
                    if (state == null)
                        throw new InvalidOperationException("no Gunslinger reward state in the Standard respec");
                    DescribeFcbRespecOffer(controller, state, leaves, grit, confirmation, row);
                    if (!FavoredClassLevelUpHarness.Select(controller, state, hitPoint))
                        throw new InvalidOperationException("the Standard respec could not take the hit point");
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    return true;
                }, mostlyHumanFailures, "mostly-human");
                JObject afterStandard = DescribeFcbRespecState(geniekin);
                standard["before"] = mostlyHumanBefore;
                standard["after"] = afterStandard;
                evidence["mostlyHuman"] = standard;
                JObject standardOffer = (JObject)((JObject)standard["drive"])["offer"];
                if (!(bool)mostlyHumanBefore["mostlyHumanIdentity"] || !OnlyCounters(mostlyHumanBefore, grit.Partial, 1))
                    mostlyHumanFailures.Add("the Mostly Human source was not built with human grit");
                if (!(bool)standard["committed"] || standardOffer == null || (bool)standardOffer["gritPartialSelectable"])
                    mostlyHumanFailures.Add("the Standard respec kept human access");
                if ((bool)afterStandard["mostlyHumanIdentity"] || ((JObject)afterStandard["counters"]).Count != 0 ||
                    (string)afterStandard["race"] != ifrit.Race.name)
                    mostlyHumanFailures.Add("the Standard Ifrit kept the identity or a counter: " +
                        afterStandard.ToString(Newtonsoft.Json.Formatting.None));

                // L03 with a selected power: an Ifrit Sorcerer 6 of the
                // Elemental (Fire) bloodline earned one effective level for
                // Elemental Ray through native picks; the committed respec
                // rebuilds Sorcerer 1 and the ray's arithmetic is native again.
                try
                {
                    BlueprintCharacterClass sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                        library, FcbSorcererClassGuid, "Sorcerer");
                    BlueprintFeatureSelection sorcererReward = host.BonusSelectionFor(sorcerer.AssetGuid);
                    FavoredClassLeafPair ray = leaves.Pair(FavoredClassCatalog.EffectSelectedBloodlinePower, "FireRay");
                    FavoredClassSelectedPowerLevel power = ray.Full.GetComponent<FavoredClassSelectedPowerLevel>();
                    if (sorcererReward == null || ray.Partial == null || power == null || power.Ability == null ||
                        power.PowerFeature == null)
                        throw new InvalidOperationException("The Sorcerer Fire Ray route is incomplete.");
                    var bloodlines = new HashSet<string>(FavoredClassLeafCatalog.EligibleBloodlines("FireRay").Value,
                        StringComparer.Ordinal);
                    // The native Elemental (Fire) bloodline, then its Fire Ray
                    // (Call of the Wild opens a selection between the ray and
                    // the blast): the sequence the advanced lane proves.
                    Action<LevelUpController> fireBloodline = controller =>
                    {
                        FeatureSelectionState bloodline = FavoredClassLevelUpHarness.FindOpenState(controller,
                            FcbBloodlineSelectionGuid);
                        if (bloodline == null || !FavoredClassLevelUpHarness.Select(controller, bloodline,
                            BlueprintLibraryLookup.RequireExact<BlueprintProgression>(library, FcbFireBloodlineGuid,
                                "Elemental (Fire) bloodline")))
                            throw new InvalidOperationException("the Elemental (Fire) bloodline could not be taken");
                        FeatureSelectionState rayChoice = FavoredClassLevelUpHarness.FindOpenState(controller,
                            FcbFireRaySelectionGuid);
                        if (rayChoice != null && !FavoredClassLevelUpHarness.Select(controller, rayChoice,
                            power.PowerFeature))
                            throw new InvalidOperationException("the Fire Ray could not be taken");
                        if (!controller.Preview.HasFact(power.PowerFeature) ||
                            !bloodlines.Any(guid => controller.Preview.Progression.Features.Enumerable.Any(value =>
                                value.Blueprint != null && value.Blueprint.AssetGuid == guid)))
                            throw new InvalidOperationException("the Fire Ray and its bloodline are not owned");
                    };
                    var sorcererReserved = new HashSet<string>(StringComparer.Ordinal) { sorcererReward.AssetGuid };
                    UnitEntityData caster = BuildFcbRespecSubject(FavoredClassAncestry.Ifrit, null, new[]
                        { ray.Partial, ray.Partial, ray.Partial, ray.Partial, ray.Partial, ray.Full }, sorcererReserved,
                        sorcerer, sorcererReward, null, fireBloodline);
                    disposables.Add(caster);
                    JObject powerBefore = DescribeFcbRespecPower(caster, sorcerer, power, ray);
                    JObject selectedPower = RunFcbRespec(caster, (controller, row) =>
                    {
                        FavoredClassLevelUpHarness.Configure(controller, controller.Unit, race(FavoredClassAncestry.Ifrit),
                            sorcerer, "KMG FCB Respec", null);
                        if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, sorcerer, row) == null)
                            throw new InvalidOperationException("the favored Sorcerer progression is unavailable");
                        fireBloodline(controller);
                        FavoredClassLevelUpHarness.FillOthers(controller, sorcererReserved);
                        FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(controller,
                            sorcererReward.AssetGuid);
                        if (state == null || !FavoredClassLevelUpHarness.Select(controller, state, hitPoint))
                            throw new InvalidOperationException("the Sorcerer respec could not take the hit point");
                        FavoredClassLevelUpHarness.FillOthers(controller, sorcererReserved);
                        FillFcbSpells(controller);
                        FavoredClassLevelUpHarness.FillOthers(controller, sorcererReserved);
                        return true;
                    }, powerFailures, "selected-power", sorcerer);
                    JObject powerAfter = DescribeFcbRespecPower(caster, sorcerer, power, ray);
                    selectedPower["before"] = powerBefore;
                    selectedPower["after"] = powerAfter;
                    evidence["selectedPower"] = selectedPower;
                    if ((int)powerBefore["level"] != 6 || (int)powerBefore["partial"] != 5 || (int)powerBefore["full"] != 1 ||
                        !(bool)powerBefore["ownsPower"] || (int)powerBefore["casterLevel"] != 7)
                        powerFailures.Add("the Sorcerer 6 source did not earn one effective level for its owned Fire Ray");
                    if (!(bool)selectedPower["committed"] || !(bool)selectedPower["callback"])
                        powerFailures.Add("the Sorcerer respec did not commit through the native callback");
                    if ((int)powerAfter["level"] != 1 || (int)powerAfter["partial"] != 0 || (int)powerAfter["full"] != 0 ||
                        !(bool)powerAfter["ownsPower"] || (int)powerAfter["casterLevel"] != 1)
                        powerFailures.Add("after the respec the Fire Ray kept a counter or a raised level: " +
                            powerAfter.ToString(Newtonsoft.Json.Formatting.None));
                }
                catch (Exception exception)
                {
                    powerFailures.Add("selected power: " + exception.GetType().Name + ": " + exception.Message);
                }
            }
            catch (Exception exception)
            {
                commitFailures.Add("respec: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                foreach (UnitEntityData unit in disposables)
                    try
                    {
                        if (!unit.Destroyed && unit.Descriptor.Body != null) unit.Dispose();
                    }
                    catch (Exception) { }
            }
            bool cleaned = SameReferences(partyBefore, SnapshotReferences(ReadExactMember(player, "Party"))) &&
                SameReferences(unitsBefore, SnapshotReferences(ReadExactMember(ReadExactMember(Game.Instance, "State"),
                    "AllUnits"))) &&
                SameReferences(inventoryBefore, SnapshotReferences(player.Inventory)) &&
                SameReferences(charactersBefore, SnapshotReferences(player.AllCharacters)) &&
                player.Money == moneyBefore;
            string evidencePath = WriteFavoredClassEvidence("favored-class-respec.json", evidence);
            assertions.Add(Assertion("fcb-respec-cancel",
                "a native respec cancelled after its first choices leaves the character's counters, ranks, race, level and spent grit exactly as before",
                Describe(evidence["cancel"], cancelFailures), cancelFailures.Count == 0,
                "Player.RespecCompanion with the level-up controller cancelled"));
            assertions.Add(Assertion("fcb-respec-commit",
                "a committed native respec removes every old counter exactly once and keeps only the new pick; the rebuilt counter keeps accumulating on later level-ups",
                Describe(evidence["human"], commitFailures), commitFailures.Count == 0,
                "Player.RespecCompanion commit and its success callback; later native level-ups"));
            assertions.Add(Assertion("fcb-respec-ancestry",
                "an ancestry change rebuilds eligibility: a Half-elf respecced to Human keeps one grit counter (starting at zero) and loses the elf confirmation; a Dwarf sees neither and keeps no counter",
                Describe(evidence["dwarf"], ancestryFailures), ancestryFailures.Count == 0,
                "the respec's native Gunslinger reward state (ExtractSelectionItems and CanSelect)"));
            assertions.Add(Assertion("fcb-respec-mostly-human",
                "respeccing a Mostly Human Ifrit to Standard removes the hidden identity, the human grit access and its counter",
                Describe(evidence["mostlyHuman"], mostlyHumanFailures), mostlyHumanFailures.Count == 0,
                "Player.RespecCompanion; Mostly Human selection and identity fact"));
            assertions.Add(Assertion("fcb-respec-selected-power",
                "a committed native respec of an Ifrit Sorcerer 6 whose native picks raised its Elemental Ray (Fire) by one effective level rebuilds Sorcerer 1 with no counter and the ray's native level-1 arithmetic",
                Describe(evidence["selectedPower"], powerFailures),
                powerFailures.Count == 0 && evidence["selectedPower"] != null,
                "Player.RespecCompanion; the ability's native execution context"));
            assertions.Add(Assertion("external-isolation",
                "unchanged party, global units, inventory, character list and money after every respec",
                "cleaned=" + cleaned, cleaned, "starter receipt and inventory rollback; detached unit disposal"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version, "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        /// <summary>
        /// A detached custom companion (the native respec lets exactly these
        /// change ancestry; story companions keep their race) whose counters
        /// are earned by native level-up picks.
        /// </summary>
        private UnitEntityData BuildFcbRespecSubject(string ancestry, BlueprintFeature mostlyHumanChoice,
            BlueprintFeature[] picks, HashSet<string> reserved, BlueprintCharacterClass characterClass = null,
            BlueprintFeatureSelection rewardSelection = null, Func<BlueprintFeature, bool> wanted = null,
            Action<LevelUpController> firstLevel = null)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(ancestry).RaceGuid, ancestry);
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                Kingmaker.Blueprints.Root.BlueprintRoot.Instance.CustomCompanion).Unit;
            unit.Stats.Wisdom.BaseValue = 14;
            if (!unit.Descriptor.IsTurnedOn)
                unit.Descriptor.TurnOn();
            if (!unit.Descriptor.IsCustomCompanion())
                throw new InvalidOperationException("The respec subject is not a native custom companion.");
            var failures = new List<string>();
            LevelFcbRespecSubject(unit, race, picks, reserved, failures, ancestry, mostlyHumanChoice,
                characterClass, rewardSelection, wanted, firstLevel);
            if (failures.Count != 0)
            {
                unit.Dispose();
                throw new InvalidOperationException(string.Join("; ", failures.ToArray()));
            }
            return unit;
        }

        /// <summary>
        /// Levels the subject once per pick through native visits of one class
        /// (the Gunslinger unless given), taking the pick in that class's
        /// reward selection; choices the subject needs (a companion, the
        /// bloodline of a power) are preferred before the deterministic filler.
        /// </summary>
        private void LevelFcbRespecSubject(UnitEntityData unit, BlueprintRace race, BlueprintFeature[] picks,
            HashSet<string> reserved, IList<string> failures, string label, BlueprintFeature mostlyHumanChoice = null,
            BlueprintCharacterClass characterClass = null, BlueprintFeatureSelection rewardSelection = null,
            Func<BlueprintFeature, bool> wanted = null, Action<LevelUpController> firstLevel = null)
        {
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            BlueprintCharacterClass leveled = characterClass ?? BlueprintBootstrap.GunslingerClass.CharacterClass;
            BlueprintFeatureSelection offered = rewardSelection ?? host.GunslingerSelection;
            foreach (BlueprintFeature pick in picks)
            {
                var row = new JObject();
                var preferred = new JArray();
                LevelUpController controller = null;
                try
                {
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, leveled,
                        "KMG FCB Respec " + label);
                    if (unit.Descriptor.Progression.CharacterLevel == 0)
                    {
                        if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, leveled, row) == null)
                            throw new InvalidOperationException("the favored " + leveled.name +
                                " progression is unavailable");
                        if (mostlyHumanChoice != null)
                        {
                            ElementalMostlyHumanRaceBlueprints ancestry = BlueprintBootstrap.MostlyHuman.Races.Single(
                                value => ReferenceEquals(value.Race, race));
                            FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(controller,
                                ancestry.Selection.AssetGuid);
                            if (state == null || !FavoredClassLevelUpHarness.Select(controller, state, mostlyHumanChoice))
                                throw new InvalidOperationException("the Mostly Human choice could not be taken");
                        }
                        if (firstLevel != null)
                            firstLevel(controller);
                    }
                    if (wanted != null)
                        PreferFcbChoices(controller, reserved, wanted, preferred);
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    FeatureSelectionState reward = FavoredClassLevelUpHarness.FindOpenState(controller,
                        offered.AssetGuid);
                    if (reward == null || !FavoredClassLevelUpHarness.Select(controller, reward, pick))
                        throw new InvalidOperationException("level " + (unit.Descriptor.Progression.CharacterLevel + 1) +
                            " could not take " + pick.name + " (reward " + (reward == null ? "not open" : "open, " +
                            (FavoredClassLevelUpHarness.Item(controller, reward, pick) == null ? "not listed" :
                                "listed but not selectable")) + "; race " + (controller.Preview.Progression.Race == null ?
                            "none" : controller.Preview.Progression.Race.name) + "; preferred " +
                            string.Join(",", preferred.Select(value => (string)value).ToArray()) + ")");
                    if (wanted != null)
                        PreferFcbChoices(controller, reserved, wanted, preferred);
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    FillFcbSpells(controller);
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    if (!FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, row))
                        throw new InvalidOperationException("the level is incomplete: " + row["completion"]);
                }
                catch (Exception exception)
                {
                    failures.Add(label + ": " + exception.GetType().Name + ": " + exception.Message);
                    return;
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                }
            }
        }

        /// <summary>
        /// Runs one native respec and rolls back its inventory, starter and
        /// character-list effects; the respecced class (the Gunslinger unless
        /// given) grants no starting gold for the call.
        /// </summary>
        private JObject RunFcbRespec(UnitEntityData source, Func<LevelUpController, JObject, bool> drive,
            IList<string> failures, string label, BlueprintCharacterClass characterClass = null)
        {
            Player player = Game.Instance.Player;
            BlueprintCharacterClass respecced = characterClass ?? BlueprintBootstrap.GunslingerClass.CharacterClass;
            var evidence = new JObject { ["case"] = label };
            List<object> inventoryBefore = EnumerateRuntimeInventory(player.Inventory);
            BlueprintItem[] startingItems = respecced.StartingItems ?? new BlueprintItem[0];
            int[] startingCounts = startingItems.Select(item => player.Inventory.Count(item)).ToArray();
            int startingGold = respecced.StartingGold;
            long moneyBefore = player.Money;
            List<UnitEntityData> characters = player.AllCharacters;
            bool registered = !characters.Any(value => ReferenceEquals(value, source));
            var handler = new FcbRespecHandler { Drive = drive };
            bool subscribed = false, callback = false;
            try
            {
                respecced.StartingGold = 0;
                if (registered) characters.Add(source);
                EventBus.Subscribe(handler);
                subscribed = true;
                player.RespecCompanion(source, () => callback = true);
            }
            finally
            {
                if (subscribed) EventBus.Unsubscribe(handler);
                respecced.StartingGold = startingGold;
                List<object> added = EnumerateRuntimeInventory(player.Inventory).Where(item =>
                    !inventoryBefore.Any(existing => ReferenceEquals(existing, item))).ToList();
                foreach (ItemEntityWeapon firearm in added.OfType<ItemEntityWeapon>())
                {
                    UnitEntityData owner;
                    if (BatteredFirearmOriginRuntime.TryGetOwner(firearm, out owner) &&
                        BatteredFirearmOriginRuntime.SameStableOwner(owner, source))
                        try { GunslingerStartingFirearmGrantTransaction.RemoveReceiptForRuntimeTest(firearm, source); }
                        catch (Exception exception) { failures.Add(label + ": starter receipt: " + exception.Message); }
                }
                foreach (object item in added)
                {
                    object ignored;
                    string method;
                    ReflectionAccess.TryInvokeAny(player.Inventory, new[] { "Remove", "RemoveItem" },
                        new[] { new object[] { item, 1, false }, new object[] { item, 1 }, new object[] { item } },
                        out ignored, out method);
                }
                for (int index = 0; index < startingItems.Length; index++)
                {
                    int excess = player.Inventory.Count(startingItems[index]) - startingCounts[index];
                    if (excess > 0) player.Inventory.Remove(startingItems[index], excess);
                }
                if (registered) characters.Remove(source);
                UnitEntityData shell = handler.Replacement == null ? null : handler.Replacement.Unit;
                if (shell != null && !ReferenceEquals(shell, source) && !shell.Destroyed &&
                    shell.Descriptor.Body != null)
                    shell.Dispose();
            }
            evidence["invoked"] = handler.Invoked;
            evidence["committed"] = handler.Committed;
            evidence["callback"] = callback;
            evidence["moneyDelta"] = player.Money - moneyBefore;
            evidence["drive"] = handler.Evidence;
            if (handler.Failure != null)
                failures.Add(label + ": " + handler.Failure.GetType().Name + ": " + handler.Failure.Message);
            if (player.Money != moneyBefore)
                failures.Add(label + ": the respec changed the party's money by " + (player.Money - moneyBefore));
            return evidence;
        }

        /// <summary>
        /// Picks, in any open unreserved selection, a selectable choice the
        /// subject needs before the deterministic filler completes the rest.
        /// </summary>
        private static void PreferFcbChoices(LevelUpController controller, ICollection<string> reserved,
            Func<BlueprintFeature, bool> wanted, JArray taken)
        {
            for (int guard = 0; guard < 8; guard++)
            {
                LevelUpState state = controller.State;
                UnitDescriptor preview = controller.Preview;
                bool picked = false;
                foreach (FeatureSelectionState pending in state.Selections.Where(value => !value.Selected &&
                    value.Selection != null).ToArray())
                {
                    var blueprint = pending.Selection as BlueprintScriptableObject;
                    if (blueprint != null && reserved.Contains(blueprint.AssetGuid))
                        continue;
                    IFeatureSelectionItem choice = pending.Selection.ExtractSelectionItems(preview, preview)
                        .Where(item => item != null && item.Feature != null && wanted(item.Feature) &&
                            pending.Selection.CanSelect(preview, state, pending, item))
                        .OrderBy(item => item.Feature.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                    if (choice != null && controller.SelectFeature(pending, choice))
                    {
                        taken.Add(FavoredClassLevelUpHarness.Name(pending.Selection) + "=" + choice.Feature.name);
                        picked = true;
                        break;
                    }
                }
                if (!picked)
                    return;
            }
        }

        /// <summary>A companion feature, or a selection that leads to one.</summary>
        private static bool GrantsFcbPet(BlueprintFeature feature)
        {
            return GrantsFcbPet(feature, 0);
        }

        private static bool GrantsFcbPet(BlueprintFeature feature, int depth)
        {
            if (feature == null || depth > 3)
                return false;
            if (feature.GetComponent<AddPet>() != null)
                return true;
            var selection = feature as BlueprintFeatureSelection;
            return selection != null && (selection.AllFeatures ?? new BlueprintFeature[0])
                .Any(value => GrantsFcbPet(value, depth + 1));
        }

        /// <summary>
        /// Fills every open spells-known slot, then every extra (formula or
        /// spellbook) slot, with the lowest-identity unknown spell.
        /// </summary>
        private static void FillFcbSpells(LevelUpController controller)
        {
            for (int guard = 0; guard < 128; guard++)
            {
                bool selected = false;
                foreach (SpellSelectionData selection in controller.State.SpellSelections.ToArray())
                {
                    Spellbook book = controller.Preview.GetSpellbook(selection.Spellbook);
                    for (int level = 0; level < selection.LevelCount.Length && !selected; level++)
                    {
                        var slots = selection.LevelCount[level];
                        if (slots == null)
                            continue;
                        int slot = Array.FindIndex(slots.SpellSelections, value => value == null);
                        if (slot < 0)
                            continue;
                        BlueprintAbility spell = selection.SpellList.GetSpells(level)
                            .Where(value => value != null && (book == null || !book.IsKnown(value)) &&
                                !slots.SpellSelections.Contains(value))
                            .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                        if (spell != null)
                            selected = controller.SelectSpell(selection.Spellbook, selection.SpellList, level, spell,
                                slot);
                    }
                    if (!selected && selection.ExtraSelected != null)
                    {
                        int extra = Array.FindIndex(selection.ExtraSelected, value => value == null);
                        for (int level = selection.ExtraMaxLevel; extra >= 0 && level >= 0 && !selected; level--)
                        {
                            BlueprintAbility spell = selection.SpellList.GetSpells(level)
                                .Where(value => value != null && (book == null || !book.IsKnown(value)) &&
                                    !selection.ExtraSelected.Contains(value))
                                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                            if (spell != null)
                                selected = controller.SelectSpell(selection.Spellbook, selection.SpellList, level,
                                    spell, extra);
                        }
                    }
                    if (selected)
                        break;
                }
                if (!selected)
                    return;
            }
        }

        /// <summary>The class level, counters, ownership and native arithmetic of a selected power.</summary>
        private static JObject DescribeFcbRespecPower(UnitEntityData unit, BlueprintCharacterClass characterClass,
            FavoredClassSelectedPowerLevel power, FavoredClassLeafPair pair)
        {
            var data = new AbilityData(power.Ability, unit.Descriptor);
            MechanicsContext context = data.CreateExecutionContext(new TargetWrapper(unit));
            context.Recalculate();
            return new JObject
            {
                ["level"] = unit.Descriptor.Progression.GetClassLevel(characterClass),
                ["partial"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, pair.Partial),
                ["full"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, pair.Full),
                ["ownsPower"] = unit.Descriptor.HasFact(power.PowerFeature),
                ["casterLevel"] = context.Params.CasterLevel,
                ["rankBonus"] = context.Params.RankBonus,
                ["default"] = context[AbilityRankType.Default],
            };
        }

        private static void DescribeFcbRespecOffer(LevelUpController controller, FeatureSelectionState state,
            FavoredClassBlueprintSet leaves, FavoredClassLeafPair grit, FavoredClassLeafPair confirmation, JObject row)
        {
            IFeatureSelectionItem[] items = FavoredClassLevelUpHarness.Items(controller, state);
            Func<FavoredClassLeafPair, int> pairs = pair => leaves.Pairs.Count(value => value.Effect.Id == pair.Effect.Id &&
                value.Leaves.Any(leaf => items.Any(item => ReferenceEquals(item.Feature, leaf))));
            row["offer"] = new JObject
            {
                ["gritPairs"] = pairs(grit),
                ["confirmationPairs"] = pairs(confirmation),
                ["gritPartialSelectable"] = FavoredClassLevelUpHarness.CanSelect(controller, state, grit.Partial),
                ["gritFullSelectable"] = FavoredClassLevelUpHarness.CanSelect(controller, state, grit.Full),
                ["confirmationPartialSelectable"] = FavoredClassLevelUpHarness.CanSelect(controller, state,
                    confirmation.Partial),
            };
        }

        /// <summary>The favored-class state a respec may change, for exact comparison.</summary>
        private static JObject DescribeFcbRespecState(UnitEntityData unit)
        {
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            var owned = new HashSet<BlueprintFeature>(leaves.Pairs.SelectMany(pair => pair.Leaves));
            Feature[] features = unit.Descriptor.Progression.Features.Enumerable.Where(value =>
                value.Blueprint != null && owned.Contains(value.Blueprint)).ToArray();
            var counters = new JObject();
            foreach (IGrouping<BlueprintFeature, Feature> group in features.GroupBy(value => value.Blueprint)
                .OrderBy(value => value.Key.name, StringComparer.Ordinal))
                counters[group.Key.name] = group.Sum(value => value.GetRank());
            ElementalMostlyHumanBlueprintSet mostlyHuman = BlueprintBootstrap.MostlyHuman;
            return new JObject
            {
                ["race"] = unit.Descriptor.Progression.Race == null ? null : unit.Descriptor.Progression.Race.name,
                ["gunslingerLevel"] = unit.Descriptor.Progression.GetClassLevel(gunslinger.CharacterClass),
                ["counters"] = counters,
                ["duplicateFacts"] = features.Length - features.Select(value => value.Blueprint).Distinct().Count(),
                ["gritMax"] = gunslinger.Grit.Resource.GetMaxAmount(unit.Descriptor),
                ["gritCurrent"] = unit.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource),
                ["mostlyHumanIdentity"] = mostlyHuman != null && unit.Descriptor.HasFact(mostlyHuman.Identity),
            };
        }

        private static bool OnlyCounters(JObject state, BlueprintFeature leaf, int rank)
        {
            var counters = (JObject)state["counters"];
            return counters.Count == 1 && counters[leaf.name] != null && (int)counters[leaf.name] == rank;
        }
    }
}
