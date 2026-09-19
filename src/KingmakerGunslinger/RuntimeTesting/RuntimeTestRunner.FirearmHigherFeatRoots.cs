using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints.Root;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Task B: the four higher firearm feat roots (Greater Weapon Focus,
        // Weapon Specialization, Greater Weapon Specialization, Improved
        // Critical) across Pistol/Musket/Blunderbuss, committed on a
        // disposable unit through real LevelUpController levels, real feat
        // slots and real prerequisite checks. No production prerequisite is
        // changed, no target fact is inserted directly and no eligibility
        // check is bypassed; the unit's firearm proficiency uses the same real
        // feature fact the qualified dependent-feats scenario grants.
        private RuntimeTestResult RunFirearmHigherFeatRoots()
        {
            string[] rootNames = { "Greater Weapon Focus", "Weapon Specialization",
                "Greater Weapon Specialization", "Improved Critical" };
            string[] rootGuids = {
                "09c9e82965fb4334b984a1e9df3bd088",
                "31470b17e8446ae4ea0dacd6c5817d86",
                "7cf5edc65e785a24f9cf93af987d66b3",
                "f4201c85a991369408740c6888362e20" };
            string[] kindNames = { "Pistol", "Musket", "Blunderbuss" };
            var roots = rootGuids.Select((guid, index) =>
                BlueprintLibraryLookup.RequireExact<BlueprintParametrizedFeature>(
                    BlueprintBootstrap.Library, guid, "native higher firearm feat root " + rootNames[index]))
                .ToArray();
            FirearmFeatBlueprintSet feats = BlueprintBootstrap.FirearmFeats;
            BlueprintFeature[][] choices = feats.DependentChoices;
            if (choices == null || choices.Length != rootNames.Length ||
                choices.Any(family => family == null || family.Length < kindNames.Length))
                throw new InvalidOperationException("The registered dependent firearm choices are unavailable.");
            // Weapon Focus is the prerequisite chain root: the higher roots
            // legally require it per firearm, so the ladder commits it first
            // through the same real slots.
            var weaponFocusRoot = BlueprintLibraryLookup.RequireExact<BlueprintParametrizedFeature>(
                BlueprintBootstrap.Library, "1e1f627d26ad36f43bbd26cc2bf8ac7e", "native Weapon Focus");
            var weaponFocusChoices = feats.WeaponFocusChoices;
            if (weaponFocusChoices == null || weaponFocusChoices.Length < kindNames.Length)
                throw new InvalidOperationException("The registered Weapon Focus choices are unavailable.");
            // Exact native prerequisites are recorded, never assumed: the
            // ladder only relies on the controller's own selection checks.
            var prerequisiteNotes = new JArray();
            for (int family = 0; family < roots.Length; family++)
            {
                var classLevels = choices[family][0].ComponentsArray.OfType<PrerequisiteClassLevel>().ToArray();
                var statValues = choices[family][0].ComponentsArray.OfType<PrerequisiteStatValue>().ToArray();
                prerequisiteNotes.Add(new JObject {
                    ["root"] = rootNames[family],
                    ["classLevelPrerequisites"] = new JArray(classLevels.Select(value =>
                        new JObject { ["class"] = value.CharacterClass?.name, ["level"] = value.Level, ["group"] = value.Group.ToString() })),
                    ["statPrerequisites"] = new JArray(statValues.Select(value =>
                        new JObject { ["stat"] = value.Stat.ToString(), ["value"] = value.Value })) });
            }
            BlueprintCharacterClass fighter = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintCharacterClass>().SingleOrDefault(value => value.name == "FighterClass");
            if (fighter == null)
                throw new InvalidOperationException("The native Fighter class is unavailable for the higher-feat ladder.");
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            int completedLevels = 0;
            bool cancellationProved = false, ineligibleProved = false, cancellationAttempted = false;
            string ineligibleObserved = "", cancellationObserved = "";
            // Every controller visit that did not close itself is closed in a
            // per-visit finally; any failure there fails the run's cleanup
            // assertion without masking a body exception.
            var controllerCleanupFailures = new List<string>();
            var selectionLog = new JArray();
            var fillersUsed = new JArray();
            bool cleaned = false;
            string observed = "";
            int verifiedCount = 0, pendingRemaining = 0, unintendedCount = -1;
            bool reviewExact = false, cleanupExact = false;
            try
            {
                entity = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                UnitDescriptor descriptor = entity.Descriptor;
                // Real firearm proficiency fact, the same grant the qualified
                // dependent-feats scenario uses; never a bypassed prerequisite.
                if (descriptor.AddFact(BlueprintBootstrap.FirearmProficiency) == null)
                    throw new InvalidOperationException("The disposable unit could not receive real firearm proficiency.");
                Type controllerType = typeof(LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo apply = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                // Planned legal picks in prerequisite order; each is committed
                // only when the controller offers a legal menu item for it.
                // Family -1 denotes Weapon Focus (the prerequisite chain).
                var plan = new List<KeyValuePair<int, int>>();
                for (int kind = 0; kind < kindNames.Length; kind++)
                    plan.Add(new KeyValuePair<int, int>(-1, kind));
                for (int family = 0; family < roots.Length; family++)
                    for (int kind = 0; kind < kindNames.Length; kind++)
                        plan.Add(new KeyValuePair<int, int>(family, kind));
                var pending = new Queue<KeyValuePair<int, int>>(plan);
                const int maxLevel = 20;
                // Every controller visit has an explicit native cleanup
                // boundary: a visit either completes (ApplyLevelup + Cancel) or
                // is closed in this loop's finally. A live controller is never
                // overwritten and a failing visit never leaks one.
                for (int level = 1; level <= maxLevel && pending.Count > 0; level++)
                {
                    object mode = Enum.Parse(start.GetParameters()[4].ParameterType, "LevelUp", false);
                    LevelUpController controller = null;
                    try
                    {
                        controller = OpenLevelVisit(start, descriptor, fighter, mode);
                        var selections = controller.State.Selections;
                        var levelLog = new JObject { ["level"] = level,
                            ["slots"] = new JArray(selections.Select(value => ((BlueprintFeatureSelection)value.Selection)?.name)),
                            ["picks"] = new JArray() };
                        selectionLog.Add(levelLog);
                        // Cancellation proof rides on the ladder's own
                        // successful GWS(Pistol) selection below: the visit
                        // start snapshot is taken here, and when that exact
                        // legal choice is first held the whole visit is
                        // cancelled without apply and compared.
                        Newtonsoft.Json.Linq.JObject visitStartSnapshot = cancellationProved ? null :
                            SnapshotProgressionState(descriptor, roots, rootNames, fighter);
                        var dequeuedThisVisit = new List<KeyValuePair<int, int>>();

                        // One ineligible control: Greater Weapon Specialization
                        // for Pistol must be refused before its native fighter
                        // prerequisites are met. A refused probe selects
                        // nothing; a successful one would close and reopen the
                        // visit rather than pollute it.
                        if (!ineligibleProved && descriptor.Progression.GetClassLevel(fighter) < 8 &&
                            selections.FirstOrDefault(value => !value.Selected &&
                                value.Selection is BlueprintFeatureSelection) is FeatureSelectionState earlySlot)
                        {
                            string earlyOffered, earlyRoute;
                            bool earlySelected = TryCommitRoot(controller, descriptor, earlySlot,
                                roots[2], weaponFocusChoices[0], out earlyOffered, out earlyRoute);
                            ineligibleObserved = "offered=" + earlyOffered + ";selected=" + earlySelected +
                                ";committed=" + CountCommittedRoots(descriptor, roots[2]);
                            ineligibleProved = !earlySelected && CountCommittedRoots(descriptor, roots[2]) == 0;
                            if (earlySelected)
                            {
                                controller.Cancel();
                                controller = null;
                                controller = OpenLevelVisit(start, descriptor, fighter, mode);
                                selections = controller.State.Selections;
                            }
                        }
                        int committedThisLevel = 0;
                        foreach (var slot in selections.Where(value => !value.Selected &&
                                value.Selection is BlueprintFeatureSelection).ToArray())
                        {
                            if (pending.Count == 0) break;
                            var next = pending.Peek();
                            bool isCancellationTarget = !cancellationProved &&
                                next.Key == 2 && next.Value == 0;
                            var targetRoot = next.Key < 0 ? weaponFocusRoot : roots[next.Key];
                            // Every appended firearm entry of all five integrated
                            // roots uses the registered Weapon Focus choice of
                            // the kind as its FeatureParam (shared published
                            // parameter set), so that is the param item to match
                            // and commit.
                            var targetChoice = weaponFocusChoices[next.Value];
                            string offered, route;
                            bool selected = TryCommitRoot(controller, descriptor, slot,
                                targetRoot, targetChoice, out offered, out route);
                            ((JArray)levelLog["picks"]).Add(new JObject {
                                ["root"] = next.Key < 0 ? "Weapon Focus" : rootNames[next.Key],
                                ["kind"] = kindNames[next.Value],
                                ["offered"] = offered, ["selected"] = selected, ["route"] = route });
                            if (!selected) continue;
                            pending.Dequeue();
                            dequeuedThisVisit.Add(next);
                            committedThisLevel++;
                            if (isCancellationTarget && selected)
                            {
                                // The exact legal GWS(Pistol) choice is now
                                // held in this visit. Cancel the entire visit
                                // without applying and prove the underlying
                                // unit is unchanged; then redo the visit.
                                bool previewHeld = controller.State.Selections.Any(value =>
                                    value.SelectedItem != null && value.SelectedItem.Feature != null &&
                                    ReferenceEquals(value.SelectedItem.Feature, roots[2]) &&
                                    value.SelectedItem.Param != null &&
                                    ReferenceEquals(value.SelectedItem.Param.Blueprint, weaponFocusChoices[0]));
                                controller.Cancel();
                                controller = null;
                                cancellationAttempted = true;
                                var afterSnapshot = SnapshotProgressionState(descriptor, roots, rootNames, fighter);
                                var cancelFailures = new List<string>();
                                cancellationProved = FirearmHigherFeatRootsRules.EvaluateCancellationEvidence(
                                    visitStartSnapshot, afterSnapshot, previewHeld,
                                    roots[2].AssetGuid, weaponFocusChoices[0].AssetGuid, cancelFailures);
                                cancellationObserved = "previewHeld=" + previewHeld +
                                    ";before=" + visitStartSnapshot.ToString(Newtonsoft.Json.Formatting.None) +
                                    ";after=" + afterSnapshot.ToString(Newtonsoft.Json.Formatting.None) +
                                    (cancelFailures.Count == 0 ? "" : ";failures=" + string.Join("|", cancelFailures));
                                // Reopen the visit and redo the picks that had
                                // been selected before the cancellation.
                                controller = OpenLevelVisit(start, descriptor, fighter, mode);
                                selections = controller.State.Selections;
                                foreach (var redo in dequeuedThisVisit)
                                {
                                    var redoRoot = redo.Key < 0 ? weaponFocusRoot : roots[redo.Key];
                                    var redoChoice = weaponFocusChoices[redo.Value];
                                    foreach (var redoSlot in controller.State.Selections.Where(value => !value.Selected &&
                                            value.Selection is BlueprintFeatureSelection).ToArray())
                                    {
                                        string redoOffered, redoRoute;
                                        if (TryCommitRoot(controller, descriptor, redoSlot, redoRoot, redoChoice,
                                            out redoOffered, out redoRoute)) break;
                                    }
                                }
                            }
                        }
                        apply.Invoke(controller, new object[] { descriptor });
                        controller.Cancel();
                        controller = null;
                        int applied = descriptor.Progression.GetClassLevel(fighter);
                        if (applied != level)
                        {
                            levelLog["applyObserved"] = applied;
                            completedLevels = level - 1;
                            break;
                        }
                        completedLevels = level;
                    }
                    finally
                    {
                        // Preserve an original body failure: a cleanup failure
                        // here is recorded for the cleanup assertion but never
                        // masks the body exception.
                        if (controller != null)
                        {
                            try { controller.Cancel(); }
                            catch (Exception cleanupError)
                            {
                                controllerCleanupFailures.Add("level " + level + ": " + cleanupError);
                            }
                            controller = null;
                        }
                    }
                }
                // Final review of every one of the 12 root/weapon combos.
                var review = new JArray();
                int verified = 0;
                for (int family = 0; family < roots.Length; family++)
                {
                    for (int kind = 0; kind < kindNames.Length; kind++)
                    {
                        var committed = descriptor.Progression.Features.Enumerable
                            .Where(value => ReferenceEquals(value.Blueprint, roots[family]))
                            .ToArray();
                        var matching = committed.Where(value => value.Param != null &&
                            ReferenceEquals(value.Param.Blueprint, weaponFocusChoices[kind])).ToArray();
                        var presentation = matching.Length == 1
                            ? new Kingmaker.Blueprints.Classes.Selection.FeatureUIData(roots[family], matching[0].Param)
                            : null;
                        string letter = presentation == null ? null : presentation.NameForAcronim;
                        bool iconNull = presentation != null && presentation.Icon == null;
                        bool rankExact = matching.Length == 1 && descriptor.Progression.Features.GetRank(roots[family]) == 1;
                        bool letterExact = letter == kindNames[kind].Substring(0, 1);
                        // The observed row records what the fact actually
                        // carries; the expected pair is emitted independently
                        // and compared, never copied into the observed fields.
                        // The per-root wrapper (choices[family][kind]) is NOT
                        // the committed parameter and must never fill one.
                        var observedFact = matching.Length == 1 ? matching[0] : null;
                        var rowFailures = new List<string>();
                        bool paramExact = matching.Length == 1 && observedFact.Param != null &&
                            FirearmHigherFeatRootsRules.EvaluateCommittedParameter(
                                observedFact.Blueprint.AssetGuid, observedFact.Param.Blueprint.AssetGuid,
                                roots[family].AssetGuid, weaponFocusChoices[kind].AssetGuid,
                                choices[family][kind].AssetGuid,
                                descriptor.Progression.Features.GetRank(roots[family]), rowFailures);
                        review.Add(new JObject {
                            ["root"] = rootNames[family], ["kind"] = kindNames[kind],
                            ["committedCount"] = matching.Length,
                            ["observedRootGuid"] = observedFact == null ? null : observedFact.Blueprint.AssetGuid,
                            ["observedParamGuid"] = observedFact == null || observedFact.Param == null ||
                                observedFact.Param.Blueprint == null ? null : observedFact.Param.Blueprint.AssetGuid,
                            ["observedParamName"] = observedFact == null || observedFact.Param == null ||
                                observedFact.Param.Blueprint == null ? null : observedFact.Param.Blueprint.name,
                            ["expectedRootGuid"] = roots[family].AssetGuid,
                            ["expectedParamGuid"] = weaponFocusChoices[kind].AssetGuid,
                            ["expectedParamName"] = weaponFocusChoices[kind].name,
                            ["notTheWrapperGuid"] = choices[family][kind].AssetGuid,
                            ["ranks"] = matching.Length == 1 ? descriptor.Progression.Features.GetRank(roots[family]) : -1,
                            ["featureUidataIconNull"] = iconNull,
                            ["monogramLetter"] = letter,
                            ["paramFailures"] = new JArray(rowFailures),
                            ["exact"] = matching.Length == 1 && paramExact && rankExact && iconNull && letterExact });
                        if (matching.Length == 1 && rankExact && iconNull && letterExact) verified++;
                        if (verified == 12) break;
                    }
                }
                int unintended = descriptor.Progression.Features.Enumerable
                    .Count(value => value.Blueprint.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        !choices.SelectMany(family => family.Take(3)).Any(choice => ReferenceEquals(choice, value.Blueprint)) &&
                        !weaponFocusChoices.Any(choice => ReferenceEquals(choice, value.Blueprint)) &&
                        !ReferenceEquals(value.Blueprint, BlueprintBootstrap.FirearmProficiency) &&
                        !ReferenceEquals(value.Blueprint, BlueprintBootstrap.FirearmFeats.RapidReload));
                verifiedCount = verified;
                pendingRemaining = pending.Count;
                unintendedCount = unintended;
                reviewExact = review.All(value => (bool)value["exact"]);
                cleanupExact = controllerCleanupFailures.Count == 0;
                observed = "levels=" + completedLevels + ";verified=" + verified + "/12" +
                    ";pending=" + pending.Count + ";cancellation=" + cancellationProved +
                    ";cancellationAttempted=" + cancellationAttempted +
                    ";controllerCleanupFailures=" + controllerCleanupFailures.Count +
                    ";ineligible=" + ineligibleProved + ";unintended=" + unintended +
                    ";review=" + review.ToString(Newtonsoft.Json.Formatting.None) +
                    ";prerequisites=" + prerequisiteNotes.ToString(Newtonsoft.Json.Formatting.None) +
                    ";selectionLog=" + selectionLog.ToString(Newtonsoft.Json.Formatting.None) +
                    ";fillers=" + fillersUsed.ToString(Newtonsoft.Json.Formatting.None) +
                    ";ineligibleObserved=" + ineligibleObserved + ";cancellationObserved=" + cancellationObserved;
            }
            finally
            {
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("higher-feat-roots-committed",
                    "12/12 root/weapon combinations committed through real controller selections with exact FeatureParam blueprints and rank 1",
                    observed, verifiedCount == 12 && pendingRemaining == 0,
                    "LevelUpController SelectFeature/ApplyLevelup on a disposable Fighter"),
                Assertion("higher-feat-native-monogram-presentation",
                    "FeatureUIData for every committed fact has null Icon and the exact P/M/B monogram",
                    observed, reviewExact,
                    "patched FeatureUIData(BlueprintFeature, FeatureParam) constructor"),
                Assertion("higher-feat-cancellation-and-cleanup",
                    "the held GWS(Pistol) preview choice is cancelled without apply; before/after progression snapshots are identical (no new root fact, no rank gain, no level change); every controller visit closed cleanly",
                    cancellationObserved + ";controllerCleanupFailures=" + string.Join("|", controllerCleanupFailures),
                    cancellationProved && cancellationAttempted && controllerCleanupFailures.Count == 0,
                    "snapshot comparison via FirearmHigherFeatRootsRules.EvaluateCancellationEvidence + per-visit Cancel boundary"),
                Assertion("higher-feat-ineligible-control",
                    "Greater Weapon Specialization is refused before its native prerequisites are met", ineligibleObserved,
                    ineligibleProved, "native selection Check through SelectFeature"),
                Assertion("higher-feat-no-unintended-facts",
                    "no unintended KMG feature facts on the disposable unit", "unintended=" + unintendedCount,
                    unintendedCount == 0, "progression feature enumeration"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots", "cleaned=" + cleaned,
                    cleaned, "detached entity disposal and exact reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(
                assertions.TrueForAll(value => value.Status == "PASS")
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        // Opens one real level-up visit: controller + class selection +
        // mechanics applied to the plan. Callers own closing it (Cancel).
        private static LevelUpController OpenLevelVisit(
            System.Reflection.MethodInfo start, UnitDescriptor descriptor,
            BlueprintCharacterClass fighter, object mode)
        {
            var controller = (LevelUpController)start.Invoke(null,
                new object[] { descriptor, false, null, null, mode });
            if (!controller.SelectClass(fighter, false))
                throw new InvalidOperationException(
                    "Native Fighter selection rejected the level-up visit.");
            controller.ApplyClassMechanics();
            return controller;
        }

        // Snapshot of the disposable unit's progression state that a
        // cancelled, never-applied visit could legitimately alter: class and
        // character levels plus every tracked root/parameter fact with rank.
        private static Newtonsoft.Json.Linq.JObject SnapshotProgressionState(
            UnitDescriptor descriptor, BlueprintParametrizedFeature[] roots, string[] rootNames,
            BlueprintCharacterClass fighter)
        {
            var facts = new Newtonsoft.Json.Linq.JArray();
            foreach (var fact in descriptor.Progression.Features.Enumerable)
            {
                int index = Array.IndexOf(roots, fact.Blueprint);
                if (index < 0) continue;
                facts.Add(new Newtonsoft.Json.Linq.JObject {
                    ["root"] = rootNames[index],
                    ["rootGuid"] = fact.Blueprint.AssetGuid,
                    ["paramGuid"] = fact.Param == null || fact.Param.Blueprint == null ?
                        null : fact.Param.Blueprint.AssetGuid,
                    ["rank"] = fact.Rank });
            }
            return new Newtonsoft.Json.Linq.JObject {
                ["classLevel"] = descriptor.Progression.GetClassLevel(fighter),
                ["characterLevel"] = descriptor.Progression.CharacterLevel,
                ["facts"] = facts };
        }

        // Commits one root/weapon combination through the real controller:
        // first the param-flattened item when the slot menu offers it, else the
        // bare root item followed by its child parameter selection state. The
        // native selection Check refuses prerequisites; nothing is forced.
        private static bool TryCommitRoot(LevelUpController controller, UnitDescriptor descriptor,
            FeatureSelectionState slot, BlueprintParametrizedFeature root, BlueprintFeature choice,
            out string offered, out string route)
        {
            offered = "none";
            route = "none";
            var items = ((BlueprintFeatureSelection)slot.Selection)
                .ExtractSelectionItems(descriptor, descriptor).ToArray();
            var exact = items.FirstOrDefault(value =>
                value.Feature != null && ReferenceEquals(value.Feature, root) &&
                value.Param != null && ReferenceEquals(value.Param.Blueprint, choice));
            if (exact != null)
            {
                offered = "param-item";
                route = "slot-param-item";
                return controller.SelectFeature(slot, exact);
            }
            var bare = items.FirstOrDefault(value =>
                value.Feature != null && ReferenceEquals(value.Feature, root) && value.Param == null);
            if (bare == null)
            {
                offered = "absent";
                return false;
            }
            offered = "bare-root";
            if (!controller.SelectFeature(slot, bare))
            {
                route = "bare-refused";
                return false;
            }
            route = "bare-selected";
            var child = slot.Next;
            int guard = 0;
            while (child != null && !child.Selected && guard++ < 8)
            {
                // The child selection of a parametrized root is the root
                // itself (IFeatureSelection), not a BlueprintFeatureSelection.
                var childItems = child.Selection == null ? new IFeatureSelectionItem[0] :
                    child.Selection.ExtractSelectionItems(descriptor, descriptor).ToArray();
                route += ";child=" + (child.Selection == null ? "<null>" : child.Selection.GetType().Name) +
                    "/items=" + childItems.Length + "[" + string.Join(",",
                        childItems.Take(6).Select(value =>
                            (value == null || value.Feature == null ? "<null>" : value.Feature.name) +
                            (value == null || value.Param == null || value.Param.Blueprint == null ? "" :
                                ":" + value.Param.Blueprint.name)).ToArray()) + "]";
                var paramItem = childItems.FirstOrDefault(value =>
                    value.Feature != null && ReferenceEquals(value.Feature, root) &&
                    value.Param != null && ReferenceEquals(value.Param.Blueprint, choice));
                if (paramItem == null)
                {
                    route += ";child-param-absent";
                    // Undo the dangling bare selection so the visit is not
                    // polluted with a parameter-less root commit.
                    try { controller.UnselectFeature(slot); route += ";unselected-bare"; }
                    catch (Exception unselectError) { route += ";unselect-failed:" + unselectError.Message; }
                    return false;
                }
                route += ";child-param";
                if (controller.SelectFeature(child, paramItem)) return true;
                route += ";child-param-refused";
                // The native check refused the parameter selection; undo the
                // bare selection so the slot is reusable.
                try { controller.UnselectFeature(slot); route += ";unselected-bare"; }
                catch (Exception unselectError) { route += ";unselect-failed:" + unselectError.Message; }
                return false;
            }
            if (child == null) route += ";child-null";
            else if (child.Selected) route += ";child-already-selected";
            return false;
        }

        private static int CountCommittedRoots(UnitDescriptor descriptor, BlueprintParametrizedFeature root) =>
            descriptor.Progression.Features.Enumerable.Count(value => ReferenceEquals(value.Blueprint, root));
    }
}
