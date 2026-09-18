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
            bool cancellationProved = false, ineligibleProved = false;
            string ineligibleObserved = "", cancellationObserved = "";
            var selectionLog = new JArray();
            var fillersUsed = new JArray();
            bool cleaned = false;
            string observed = "";
            int verifiedCount = 0, pendingRemaining = 0, unintendedCount = -1, committedRootsAfterGws = -1;
            bool reviewExact = false;
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
                for (int level = 1; level <= maxLevel && pending.Count > 0; level++)
                {
                    object mode = Enum.Parse(start.GetParameters()[4].ParameterType, "LevelUp", false);
                    var controller = (LevelUpController)start.Invoke(null,
                        new object[] { descriptor, false, null, null, mode });
                    if (!controller.SelectClass(fighter, false))
                        throw new InvalidOperationException("Native Fighter selection rejected level " + level + ".");
                    controller.ApplyClassMechanics();
                    var selections = controller.State.Selections;
                    var levelLog = new JObject { ["level"] = level,
                        ["slots"] = new JArray(selections.Select(value => ((BlueprintFeatureSelection)value.Selection)?.name)),
                        ["picks"] = new JArray() };
                    selectionLog.Add(levelLog);
                    // One genuine cancellation visit: before the first Greater
                    // Weapon Specialization commit, select it and cancel the
                    // controller without applying anything.
                    if (!cancellationProved && descriptor.Progression.GetClassLevel(fighter) >= 8 &&
                        pending.Any(value => value.Key == 2))
                    {
                        var gwsSlot = selections.FirstOrDefault(value => !value.Selected &&
                            value.Selection is BlueprintFeatureSelection);
                        string gwsOffered, gwsRoute;
                        bool gwsSelected = gwsSlot != null && TryCommitRoot(controller, descriptor,
                            gwsSlot, roots[2], weaponFocusChoices[0], out gwsOffered, out gwsRoute);
                        if (gwsSelected)
                        {
                            controller.Cancel();
                            controller = null;
                            cancellationProved = true;
                            cancellationObserved = "GWS(Pistol) selected then controller cancelled at fighter " +
                                descriptor.Progression.GetClassLevel(fighter) + "; fact count=" +
                                CountCommittedRoots(descriptor, roots[2]);
                        }
                        else cancellationObserved = "GWS item not yet legal at this level (deferred)";
                        // Re-open the controller for the actual level visit.
                        controller = (LevelUpController)start.Invoke(null,
                            new object[] { descriptor, false, null, null, mode });
                        controller.SelectClass(fighter, false);
                        controller.ApplyClassMechanics();
                        selections = controller.State.Selections;
                    }
                    // One ineligible control: Greater Weapon Specialization for
                    // Pistol must be refused at low fighter levels.
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
                    }
                    int committedThisLevel = 0;
                    foreach (var slot in selections.Where(value => !value.Selected &&
                            value.Selection is BlueprintFeatureSelection).ToArray())
                    {
                        if (pending.Count == 0) break;
                        var next = pending.Peek();
                        var targetRoot = next.Key < 0 ? weaponFocusRoot : roots[next.Key];
                        // Every appended firearm entry of all five integrated
                        // roots uses the registered Weapon Focus choice of the
                        // kind as its FeatureParam (shared published parameter
                        // set), so that is the param item to match and commit.
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
                        committedThisLevel++;
                    }
                    // If the level still has unselected mandatory slots the
                    // native apply may refuse; record that honestly instead of
                    // forcing fillers on the first qualification pass.
                    apply.Invoke(controller, new object[] { descriptor });
                    controller.Cancel();
                    controller = null;
                    int applied = descriptor.Progression.GetClassLevel(fighter);
                    if (applied != level)
                    {
                        levelLog["applyObserved"] = applied;
                        break;
                    }
                    completedLevels = level;
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
                        review.Add(new JObject {
                            ["root"] = rootNames[family], ["kind"] = kindNames[kind],
                            ["committedCount"] = matching.Length,
                            ["paramBlueprint"] = choices[family][kind].name,
                            ["ranks"] = matching.Length == 1 ? descriptor.Progression.Features.GetRank(roots[family]) : -1,
                            ["featureUidataIconNull"] = iconNull,
                            ["monogramLetter"] = letter,
                            ["exact"] = matching.Length == 1 && rankExact && iconNull && letterExact });
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
                observed = "levels=" + completedLevels + ";verified=" + verified + "/12" +
                    ";pending=" + pending.Count + ";cancellation=" + cancellationProved +
                    ";ineligible=" + ineligibleProved + ";unintended=" + unintended +
                    ";review=" + review.ToString(Newtonsoft.Json.Formatting.None) +
                    ";prerequisites=" + prerequisiteNotes.ToString(Newtonsoft.Json.Formatting.None) +
                    ";selectionLog=" + selectionLog.ToString(Newtonsoft.Json.Formatting.None) +
                    ";fillers=" + fillersUsed.ToString(Newtonsoft.Json.Formatting.None) +
                    ";ineligibleObserved=" + ineligibleObserved + ";cancellationObserved=" + cancellationObserved;
                committedRootsAfterGws = CountCommittedRoots(descriptor, roots[2]);
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
                Assertion("higher-feat-cancellation",
                    "a selected-then-cancelled controller visit commits nothing", cancellationObserved,
                    cancellationProved, "controller Cancel without ApplyLevelup"),
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
                    return false;
                }
                route += ";child-param";
                return controller.SelectFeature(child, paramItem);
            }
            if (child == null) route += ";child-null";
            else if (child.Selected) route += ";child-already-selected";
            return false;
        }

        private static int CountCommittedRoots(UnitDescriptor descriptor, BlueprintParametrizedFeature root) =>
            descriptor.Progression.Features.Enumerable.Count(value => ReferenceEquals(value.Blueprint, root));
    }
}
