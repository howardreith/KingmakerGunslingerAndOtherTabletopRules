using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Feats;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Rapid Reload must require firearm proficiency, never Gunslinger class
        // identity. Everything below is observed on the registered production
        // blueprints through the native machinery: BlueprintFeature.
        // MeetsPrerequisites, BlueprintFeatureSelection.CanSelect through
        // LevelUpController.SelectFeature, LevelUpState.IsComplete (the exact
        // check CharacterBuildController.Next enforces before it calls Commit)
        // and LevelUpController.SelectClass / AddArchetype / RemoveArchetype for
        // pending-build changes. Nothing is mocked, no prerequisite is bypassed,
        // and no observation is repaired before it is taken.
        private const string RapidReloadBasicFeatSelectionGuid =
            "247a4068296e8be42890143f451b4b45";
        private const string RapidReloadFighterFeatSelectionGuid =
            "41c8486641f7d6d4283ca9dae4147a9f";

        private sealed class RapidReloadGateContext
        {
            internal BlueprintFeatureSelection Parent;
            internal BlueprintFeature[] Children;
            internal BlueprintFeature[] RegisteredChildren;
            internal BlueprintFeature Full;
            internal BlueprintFeature OneHanded;
            internal BlueprintFeature TwoHanded;
            internal BlueprintFeature LegacyWrapper;
            internal FirearmKind[] Kinds;
            internal BlueprintCharacterClass Fighter;
            internal BlueprintCharacterClass Gunslinger;
            internal BlueprintFeature GunslingerProficiencies;
            internal BlueprintArchetype MusketMaster;
            internal BlueprintFeatureSelection Basic;
            internal BlueprintFeatureSelection FighterFeats;
            internal MethodInfo Start;
            internal MethodInfo Apply;
            internal object Mode;
        }

        private RuntimeTestResult RunRapidReloadProficiencyGate()
        {
            FirearmFeatBlueprintSet feats = BlueprintBootstrap.FirearmFeats;
            FirearmScopedProficiencyBlueprintSet scoped =
                BlueprintBootstrap.ScopedFirearmProficiencies;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            if (feats == null || scoped == null || gunslinger == null ||
                gunslinger.MusketMaster == null)
                throw new InvalidOperationException(
                    "The Rapid Reload gate fixture inputs are incomplete.");
            var ctx = new RapidReloadGateContext
            {
                Parent = feats.RapidReload,
                Children = feats.RapidReloadChoices,
                RegisteredChildren = feats.RegisteredRapidReloadChoices,
                Full = BlueprintBootstrap.FirearmProficiency,
                OneHanded = scoped.OneHanded,
                TwoHanded = scoped.TwoHanded,
                LegacyWrapper = feats.ExoticWeaponProficiency,
                Kinds = RapidReloadPrerequisiteRules.ParentGateKinds,
                Gunslinger = gunslinger.CharacterClass,
                GunslingerProficiencies = gunslinger.Proficiencies,
                MusketMaster = gunslinger.MusketMaster.Archetype
            };
            if (ctx.Parent == null || ctx.Children == null ||
                ctx.Children.Length != ctx.Kinds.Length || ctx.Full == null ||
                ctx.OneHanded == null || ctx.TwoHanded == null ||
                ctx.LegacyWrapper == null || ctx.Gunslinger == null ||
                ctx.GunslingerProficiencies == null || ctx.MusketMaster == null)
                throw new InvalidOperationException(
                    "The registered Rapid Reload blueprints are incomplete.");
            ctx.Basic = BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(
                BlueprintBootstrap.Library, RapidReloadBasicFeatSelectionGuid,
                "native basic feat selection");
            ctx.FighterFeats = BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(
                BlueprintBootstrap.Library, RapidReloadFighterFeatSelectionGuid,
                "native Fighter combat feat selection");
            ctx.Fighter = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintCharacterClass>()
                .SingleOrDefault(value => value.name == "FighterClass");
            if (ctx.Fighter == null)
                throw new InvalidOperationException(
                    "The native Fighter class is unavailable for the Rapid Reload gate.");
            ctx.Start = typeof(LevelUpController).GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static)
                .Single(value => value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            ctx.Apply = typeof(LevelUpController).GetMethod("ApplyLevelup",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ctx.Mode = Enum.Parse(ctx.Start.GetParameters()[4].ParameterType,
                "LevelUp", false);

            // --- Registered blueprint wiring (retained coverage) -----------
            var wiringFailures = new List<string>();
            JObject wiring = DescribeRapidReloadWiring(ctx, wiringFailures);
            var catalogFailures = new List<string>();
            JObject catalog = DescribeRapidReloadCatalogs(ctx, catalogFailures);

            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var matrix = new JArray();
            var matrixFailures = new List<string>();
            var flow = new JArray();
            var flowFailures = new List<string>();
            var pendingFailures = new List<string>();
            var musketFailures = new List<string>();
            var identityFailures = new List<string>();
            var ownershipFailures = new List<string>();
            JObject visitOwnership = null;
            JObject pendingClass = null, pendingArchetype = null;
            JObject musketMaster = null, classIdentity = null;
            BlueprintFeature independentFull = null;
            BlueprintFeature independentOneHanded = null;
            BlueprintFeature independentTwoHanded = null;
            bool cleaned = false;
            bool ignoreOff = !IgnorePrerequisites.Ignore;
            try
            {
                independentFull = CreateIndependentProficiencySource(
                    "KMG_RuntimeFixture_IndependentFullFirearmProficiency", ctx.Full);
                independentOneHanded = CreateIndependentProficiencySource(
                    "KMG_RuntimeFixture_IndependentOneHandedFirearmProficiency",
                    ctx.OneHanded);
                independentTwoHanded = CreateIndependentProficiencySource(
                    "KMG_RuntimeFixture_IndependentTwoHandedFirearmProficiency",
                    ctx.TwoHanded);

                // --- Native prerequisite matrix (retained coverage) ---------
                // Each row proves the fixture's actual proficiency ranks before
                // its eligibility answers are scored.
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "A.no-proficiency", false, false, false, null);
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "C.full-proficiency", true, false, false,
                    descriptor => GrantFixtureFact(descriptor, ctx.Full));
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "D.one-handed", false, true, false,
                    descriptor => GrantFixtureFact(descriptor, ctx.OneHanded));
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "E.two-handed", false, false, true,
                    descriptor => GrantFixtureFact(descriptor, ctx.TwoHanded));
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "E2.both-scoped", false, true, true, descriptor =>
                    {
                        GrantFixtureFact(descriptor, ctx.OneHanded);
                        GrantFixtureFact(descriptor, ctx.TwoHanded);
                    });
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "F.independent-full", true, false, false,
                    descriptor => GrantFixtureFact(descriptor, independentFull));
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "F.independent-one-handed", false, true, false,
                    descriptor => GrantFixtureFact(descriptor, independentOneHanded));
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "F.independent-two-handed", false, false, true,
                    descriptor => GrantFixtureFact(descriptor, independentTwoHanded));
                AddRapidReloadMatrixRow(ctx, matrix, matrixFailures,
                    "H.legacy-wrapper-owner", true, false, false,
                    descriptor => GrantFixtureFact(descriptor, ctx.LegacyWrapper));

                // --- A/B: refusal through both real feat catalogs -----------
                // Each case states the proficiency scope its fixture was set up
                // with next to the fact it grants; the evaluator checks the
                // measured ranks against that declaration.
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadRefusedParent(ctx, "A.ordinary-feat-no-proficiency",
                        ctx.Basic),
                    RapidReloadExpectedScope.NoProficiency,
                    RapidReloadGateEvidenceRules.EvaluateRefusedParent, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadRefusedParent(ctx, "A.combat-feat-no-proficiency",
                        ctx.FighterFeats),
                    RapidReloadExpectedScope.NoProficiency,
                    RapidReloadGateEvidenceRules.EvaluateRefusedParent, flowFailures));

                // --- B: an out-of-scope firearm refused after a legal parent -
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadScopedChildRefusal(ctx,
                        "B.one-handed-refuses-musket", ctx.OneHanded, 1, 0),
                    RapidReloadExpectedScope.OneHandedOnly,
                    RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal,
                    flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadScopedChildRefusal(ctx,
                        "B.two-handed-refuses-pistol", ctx.TwoHanded, 0, 1),
                    RapidReloadExpectedScope.TwoHandedOnly,
                    RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal,
                    flowFailures));

                // --- B: a held parent with no child cannot be confirmed -----
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadEmptySelection(ctx, "B.empty-selection-blocked",
                        ctx.Full),
                    RapidReloadExpectedScope.AnyFirearm,
                    RapidReloadGateEvidenceRules.EvaluateEmptySelection, flowFailures));

                // --- C/D/E/F/H/I: legal acquisition routes ------------------
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "C.ordinary-feat-full-proficiency",
                        ctx.Fighter, null, ctx.Basic, 0, ctx.Full),
                    RapidReloadExpectedScope.AnyFirearm,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "C.combat-feat-full-proficiency",
                        ctx.Fighter, null, ctx.FighterFeats, 0, ctx.Full),
                    RapidReloadExpectedScope.AnyFirearm,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "D.one-handed-pistol", ctx.Fighter,
                        null, ctx.Basic, 0, ctx.OneHanded),
                    RapidReloadExpectedScope.OneHandedOnly,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "E.two-handed-musket", ctx.Fighter,
                        null, ctx.Basic, 1, ctx.TwoHanded),
                    RapidReloadExpectedScope.TwoHandedOnly,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "E.two-handed-blunderbuss",
                        ctx.Fighter, null, ctx.Basic, 2, ctx.TwoHanded),
                    RapidReloadExpectedScope.TwoHandedOnly,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "F.independent-source-full",
                        ctx.Fighter, null, ctx.Basic, 0, independentFull),
                    RapidReloadExpectedScope.AnyFirearm,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "F.independent-source-two-handed",
                        ctx.Fighter, null, ctx.Basic, 1, independentTwoHanded),
                    RapidReloadExpectedScope.TwoHandedOnly,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "H.legacy-wrapper-owner",
                        ctx.Fighter, null, ctx.Basic, 0, ctx.LegacyWrapper),
                    RapidReloadExpectedScope.AnyFirearm,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));
                flow.Add(ScoreRapidReloadRow(
                    RunRapidReloadAcquisition(ctx, "I.fresh-level-one-gunslinger",
                        ctx.Gunslinger, null, ctx.Basic, 0, null),
                    RapidReloadExpectedScope.AnyFirearm,
                    RapidReloadGateEvidenceRules.EvaluateAcquisition, flowFailures));

                pendingClass = RunRapidReloadPendingClassChange(ctx, pendingFailures);
                pendingArchetype = RunRapidReloadPendingArchetypeChange(ctx,
                    pendingFailures);
                musketMaster = RunRapidReloadMusketMaster(ctx, musketFailures);
                classIdentity = RunRapidReloadClassIdentityControl(ctx,
                    identityFailures);
                visitOwnership = RunRapidReloadVisitOwnershipCheck(ctx,
                    ownershipFailures);
            }
            finally
            {
                DestroyFixtureBlueprint(independentFull);
                DestroyFixtureBlueprint(independentOneHanded);
                DestroyFixtureBlueprint(independentTwoHanded);
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }

            // Every advertised selection-flow case must have executed.
            const int expectedFlowRows = 14;
            if (flow.Count != expectedFlowRows)
                flowFailures.Add("flow-row-count=" + flow.Count + "/" + expectedFlowRows);
            const int expectedMatrixRows = 9;
            if (matrix.Count != expectedMatrixRows)
                matrixFailures.Add("matrix-row-count=" + matrix.Count + "/" +
                    expectedMatrixRows);
            bool pendingClassAccepted = RapidReloadGateEvidenceRules
                .EvaluatePendingClassChange(pendingClass, pendingFailures);
            bool pendingArchetypeAccepted = RapidReloadGateEvidenceRules
                .EvaluateArchetypeScopeChange(pendingArchetype, pendingFailures);
            bool musketMasterAccepted = RapidReloadGateEvidenceRules
                .EvaluateMusketMaster(musketMaster, musketFailures);
            bool classIdentityAccepted = RapidReloadGateEvidenceRules
                .EvaluateClassIdentityControl(classIdentity, identityFailures);

            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("rapid-reload-parent-proficiency-prerequisites",
                    "the registered Rapid Reload selection carries exactly one OR-grouped (Prerequisite.GroupType.Any) firearm-proficiency prerequisite per official kind, each referencing the registered full and scoped proficiency features, with no class, archetype or legacy-wrapper prerequisite and IgnorePrerequisites false",
                    Describe(wiring, wiringFailures), wiringFailures.Count == 0,
                    "registered BlueprintFeatureSelection.ComponentsArray"),
                Assertion("rapid-reload-catalog-publication",
                    "the same gated parent stays published exactly once in the ordinary and Fighter combat feat catalogs; no compatibility-only child is published and the legacy wrapper stays out of every selection",
                    Describe(catalog, catalogFailures), catalogFailures.Count == 0,
                    "native feat selection Features/AllFeatures enumeration"),
                Assertion("rapid-reload-native-prerequisite-matrix",
                    "on nine disposable fixtures whose actual proficiency ranks are proved first, native BlueprintFeature.MeetsPrerequisites matches the shared gate rules for absent, full, one-handed, two-handed, both-scoped, independent-source and legacy-wrapper characters",
                    "ignorePrerequisitesOff=" + ignoreOff + ";" +
                        Describe(matrix, matrixFailures),
                    ignoreOff && matrixFailures.Count == 0,
                    "native BlueprintFeature.MeetsPrerequisites scored by RapidReloadPrerequisiteRules"),
                Assertion("rapid-reload-selection-flow",
                    "all 14 real LevelUpController cases execute: an unproficient character is refused at the parent in both feat catalogs and holds nothing; an out-of-scope firearm is refused and the resulting empty Rapid Reload choice both blocks LevelUpState.IsComplete and banks no fact before any cleanup; and every legally proficient route acquires exactly its chosen firearm",
                    Describe(flow, flowFailures), flowFailures.Count == 0,
                    "LevelUpController SelectFeature + LevelUpState.IsComplete (the check CharacterBuildController.Next enforces before Commit)"),
                Assertion("rapid-reload-pending-build-change",
                    "inside one live level-up transaction, the native SelectClass and AddArchetype/RemoveArchetype operations rebuild the pending build: a held Rapid Reload choice is dropped by the engine when the pending class loses firearm proficiency, cannot survive confirmation as that class, returns to eligibility when the class is restored, and follows the Musket Master proficiency scope when the pending archetype changes",
                    "class=" + Describe(pendingClass, null) + ";archetype=" +
                        Describe(pendingArchetype, pendingFailures),
                    pendingClassAccepted && pendingArchetypeAccepted &&
                        pendingFailures.Count == 0,
                    "single-controller LevelUpController.SelectClass / AddArchetype / RemoveArchetype with native preview rebuild"),
                Assertion("rapid-reload-musket-master-and-duplicates",
                    "a Musket Master built through the native archetype route carries two-handed firearm proficiency only and the automatic Rapid Reload (Musket) grant; through the real nested child selection the owned Musket choice cannot consume another feat, Pistol stays out of scope, and Blunderbuss is acquired by exactly one feat slot",
                    Describe(musketMaster, musketFailures),
                    musketMasterAccepted && musketFailures.Count == 0,
                    "native AddArchetype + ApplyClassMechanics, nested FeatureSelectionState and CanSelect"),
                Assertion("rapid-reload-class-identity-negative-control",
                    "a character with real Gunslinger class levels and every firearm-proficiency fact proved absent at evaluation time fails the parent and every official child, and cannot select the parent in a real feat slot",
                    Describe(classIdentity, identityFailures),
                    classIdentityAccepted && identityFailures.Count == 0,
                    "committed Gunslinger progression with fixture-local fact removal, re-proved on the live preview"),
                Assertion("rapid-reload-visit-ownership-cleanup",
                    "when a Rapid Reload gate visit fails to initialise after its level-up controller exists, the helper that created it owns the native cancellation, the original setup failure reaches the caller with its message and stack intact, and a successful initialisation instead hands the same controller to the caller",
                    Describe(visitOwnership, ownershipFailures),
                    ownershipFailures.Count == 0,
                    "OpenRapidReloadVisit failure window driven by a natively rejected archetype"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "detached entity disposal and exact reference snapshots"),
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

        private static string Describe(JToken value, IList<string> failures)
        {
            return (value == null ? "<not-run>" :
                    value.ToString(Newtonsoft.Json.Formatting.None)) +
                (failures == null || failures.Count == 0 ? "" :
                    ";failures=" + string.Join("|", failures.ToArray()));
        }

        // The declared scope travels with the case to the same evaluator entry
        // point the scoring uses, so a silently different fixture cannot pass.
        private static JObject ScoreRapidReloadRow(JObject row,
            RapidReloadExpectedScope scope,
            Func<JObject, RapidReloadExpectedScope, IList<string>, bool> evaluator,
            IList<string> failures)
        {
            evaluator(row, scope, failures);
            return row ?? new JObject { ["case"] = "<missing>" };
        }

        // ------------------------------------------------------------------
        // Fixture helpers
        // ------------------------------------------------------------------

        // The observed proficiency ranks of a descriptor or a live preview.
        // Acceptance never reads an intended grant; it reads this.
        private static JObject DescribeRapidReloadFixture(RapidReloadGateContext ctx,
            UnitDescriptor descriptor)
        {
            return new JObject {
                ["fullProficiencyRank"] = descriptor.Progression.Features.GetRank(ctx.Full),
                ["oneHandedProficiencyRank"] =
                    descriptor.Progression.Features.GetRank(ctx.OneHanded),
                ["twoHandedProficiencyRank"] =
                    descriptor.Progression.Features.GetRank(ctx.TwoHanded) };
        }

        private static BlueprintFeature CreateIndependentProficiencySource(
            string name, BlueprintFeature granted)
        {
            // Test-only proficiency source: created for this request only, never
            // registered, never published and destroyed in the scenario's
            // finally block, so players can never acquire it.
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = "$KMG_RuntimeFixture_GrantFirearmProficiency";
            grant.Facts = new BlueprintUnitFact[] { granted };
            grant.DoNotRestoreMissingFacts = false;
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = name;
            feature.Ranks = 1;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            return feature;
        }

        private static void DestroyFixtureBlueprint(BlueprintScriptableObject value)
        {
            if (value == null) return;
            foreach (BlueprintComponent component in value.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                if (component != null) UnityEngine.Object.DestroyImmediate(component);
            UnityEngine.Object.DestroyImmediate(value);
        }

        private static void GrantFixtureFact(UnitDescriptor descriptor,
            BlueprintFeature feature)
        {
            if (descriptor.AddFact(feature) == null)
                throw new InvalidOperationException(
                    "The disposable unit rejected the fixture fact " + feature.name + ".");
        }

        private static UnitEntityData CreateDisposableUnit()
        {
            return new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
        }

        // Opens one real level-up visit. Archetypes are added before the class
        // is selected, exactly as the engine's own AddClassLevels.AddLevel does.
        private static LevelUpController OpenRapidReloadVisit(
            RapidReloadGateContext ctx, UnitDescriptor descriptor,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype)
        {
            LevelUpController created;
            return OpenRapidReloadVisit(ctx, descriptor, characterClass, archetype,
                out created);
        }

        // R5: the helper owns the controller it creates until initialization
        // succeeds. If a later step rejects or throws, the caller's assignment
        // never completed, so the caller's finally block holds null and cannot
        // cancel this instance — the cancellation has to happen here. On the
        // success path ownership transfers to the caller and nothing is
        // cancelled, so no routine double-cancellation is introduced. Nothing is
        // claimed about a controller ctx.Start.Invoke never returned.
        // `created` reports the controller this helper obtained whether or not
        // initialization succeeded; only the R5 ownership self-check passes it.
        private static LevelUpController OpenRapidReloadVisit(
            RapidReloadGateContext ctx, UnitDescriptor descriptor,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype,
            out LevelUpController created)
        {
            created = (LevelUpController)ctx.Start.Invoke(null,
                new object[] { descriptor, false, null, null, ctx.Mode });
            if (created == null)
                throw new InvalidOperationException(
                    "The native level-up controller factory returned no controller.");
            LevelUpController controller = created;
            try
            {
                if (archetype != null &&
                    !controller.AddArchetype(characterClass, archetype))
                    throw new InvalidOperationException(
                        "Native archetype selection rejected the Rapid Reload gate visit.");
                if (!controller.SelectClass(characterClass, false))
                    throw new InvalidOperationException(
                        "Native class selection rejected the Rapid Reload gate visit.");
                controller.ApplyClassMechanics();
                controller.ApplySpellbook();
                controller.ApplySkillPoints();
                return controller;
            }
            catch (Exception setupError)
            {
                Exception cleanupError = TryCancelRapidReloadVisit(controller);
                Exception combined = RapidReloadVisitCleanupRules.Compose(setupError,
                    cleanupError);
                if (combined != null) throw combined;
                // Bare rethrow: the original setup failure keeps its stack.
                throw;
            }
        }

        // The single native cancellation boundary. Returns the cleanup failure
        // instead of throwing, so neither a setup failure nor a cleanup failure
        // can be lost.
        private static Exception TryCancelRapidReloadVisit(LevelUpController controller)
        {
            if (controller == null) return null;
            try { controller.Cancel(); return null; }
            catch (Exception cleanupError) { return cleanupError; }
        }

        // R1: an explicit reservation of one native feat slot. The engine
        // identifies a selection state by its selection blueprint plus the
        // occurrence index LevelUpState.AddSelection assigns (the same pair
        // SelectFeature.GetSelectionState matches on), so the reservation
        // survives the preview rebuilds that attribute, skill, class and
        // archetype actions trigger. Holding a stale FeatureSelectionState
        // reference would not.
        private sealed class RapidReloadSlotReservation
        {
            internal BlueprintFeatureSelection Selection;
            internal int Index;
            internal int Level;
            internal bool Released;
        }

        private static RapidReloadSlotReservation ReserveRapidReloadSlot(
            LevelUpController controller,
            params BlueprintFeatureSelection[] preferred)
        {
            foreach (BlueprintFeatureSelection selection in preferred)
            {
                FeatureSelectionState state = controller.State.Selections
                    .FirstOrDefault(value => !value.Selected &&
                        ReferenceEquals(value.Selection, selection));
                if (state == null) continue;
                return new RapidReloadSlotReservation {
                    Selection = selection, Index = state.Index, Level = state.Level };
            }
            return null;
        }

        private static FeatureSelectionState ResolveReservedRapidReloadSlot(
            LevelUpController controller, RapidReloadSlotReservation reservation)
        {
            if (reservation == null) return null;
            return controller.State.Selections.FirstOrDefault(value =>
                ReferenceEquals(value.Selection, reservation.Selection) &&
                value.Index == reservation.Index);
        }

        // Proves the reserved slot is a real, still-open slot of the expected
        // native selection. A filler feat sitting in it is a setup failure, not
        // evidence that an invalid feat was refused.
        private static JObject DescribeRapidReloadReservation(
            LevelUpController controller, RapidReloadSlotReservation reservation)
        {
            FeatureSelectionState state = ResolveReservedRapidReloadSlot(controller,
                reservation);
            return new JObject {
                ["reservedSelection"] = reservation == null ? "<none>" :
                    reservation.Selection.name,
                ["reservedIndex"] = reservation == null ? -1 : reservation.Index,
                ["reservedLevel"] = reservation == null ? -1 : reservation.Level,
                ["reservedSlotResolved"] = state != null,
                ["reservedSlotUnselected"] = state != null && !state.Selected,
                ["reservedSlotSelectionMatches"] = state != null && reservation != null &&
                    ReferenceEquals(state.Selection, reservation.Selection),
                ["released"] = reservation != null && reservation.Released };
        }

        private static bool RapidReloadReservationHolds(JObject reservation)
        {
            return reservation != null && (bool)reservation["reservedSlotResolved"] &&
                (bool)reservation["reservedSlotUnselected"] &&
                (bool)reservation["reservedSlotSelectionMatches"];
        }

        // Releasing the reservation only makes the slot available to the filler
        // pass; it never repairs a stale Rapid Reload selection, because the
        // resolver always skips nested ctx.Parent states and never offers the
        // Rapid Reload parent as a filler item.
        private static void ReleaseRapidReloadReservation(
            RapidReloadSlotReservation reservation)
        {
            if (reservation != null) reservation.Released = true;
        }

        // Legally satisfies everything the native completion check needs except
        // the reserved slot and the Rapid Reload selection under test, so a
        // blocked completion can be attributed to the target rather than to an
        // unfinished character. Skill and attribute points are resolved first
        // because their actions outrank feature selections and would otherwise
        // force a preview rebuild after the target choice was made.
        private static JObject ResolveRapidReloadNonTargetRequirements(
            RapidReloadGateContext ctx, LevelUpController controller,
            RapidReloadSlotReservation reservation)
        {
            // Character-build fields the native completion check also demands.
            // Each is only touched when the engine says it is still open, so
            // the fixture never fabricates a choice the build did not offer.
            if (controller.State.CanSelectRaceStat)
                controller.SelectRaceStat(StatType.Strength);
            if (controller.State.CanSelectAlignment)
                controller.SelectAlignment(Alignment.TrueNeutral);
            int statGuard = 0;
            while (!controller.State.StatsDistribution.IsComplete() && statGuard++ < 200)
            {
                bool added = StatTypeHelper.Attributes.Any(attribute =>
                    controller.AddStatPoint(attribute));
                if (!added) break;
            }
            int skillGuard = 0;
            while (controller.State.SkillPointsRemaining > 0 && skillGuard++ < 200)
            {
                bool spent = StatTypeHelper.Skills.Any(skill =>
                    controller.State.SkillPointsRemaining > 0 &&
                    controller.SpendSkillPoint(skill));
                if (!spent) break;
            }
            int attributeGuard = 0;
            while (controller.State.AttributePoints > 0 && attributeGuard++ < 200)
            {
                bool spent = StatTypeHelper.Attributes.Any(attribute =>
                    controller.State.AttributePoints > 0 &&
                    controller.SpendAttributePoint(attribute));
                if (!spent) break;
            }
            var resolved = new JArray();
            int selectionGuard = 0;
            while (selectionGuard++ < 40)
            {
                LevelUpState levelUpState = controller.State;
                UnitDescriptor preview = controller.Preview;
                FeatureSelectionState pending = levelUpState.Selections.FirstOrDefault(
                    value => !value.Selected && value.Selection != null &&
                        !ReferenceEquals(value.Selection, ctx.Parent) &&
                        !IsReservedRapidReloadSlot(value, reservation) &&
                        value.CanSelectAnything(levelUpState, preview));
                if (pending == null) break;
                IFeatureSelectionItem choice = pending.Selection
                    .ExtractSelectionItems(preview, preview)
                    .FirstOrDefault(item => item != null && item.Feature != null &&
                        !ReferenceEquals(item.Feature, ctx.Parent) &&
                        pending.Selection.CanSelect(preview, levelUpState, pending, item));
                if (choice == null) break;
                if (!controller.SelectFeature(pending, choice)) break;
                resolved.Add((pending.Selection as BlueprintScriptableObject) == null
                    ? "<unnamed>"
                    : ((BlueprintScriptableObject)pending.Selection).name + "=" +
                        choice.Feature.name);
            }
            return new JObject {
                ["skillPointsRemaining"] = controller.State.SkillPointsRemaining,
                ["attributePoints"] = controller.State.AttributePoints,
                ["statsDistributionComplete"] =
                    controller.State.StatsDistribution.IsComplete(),
                ["canSelectRaceStat"] = controller.State.CanSelectRaceStat,
                ["canSelectAlignment"] = controller.State.CanSelectAlignment,
                ["canSelectRace"] = controller.State.CanSelectRace,
                ["canSelectName"] = controller.State.CanSelectName,
                ["canSelectPortrait"] = controller.State.CanSelectPortrait,
                ["canSelectGender"] = controller.State.CanSelectGender,
                ["canSelectVoice"] = controller.State.CanSelectVoice,
                ["reservationHeld"] = reservation != null && !reservation.Released,
                ["resolvedOtherSelections"] = resolved };
        }

        private static bool IsReservedRapidReloadSlot(FeatureSelectionState state,
            RapidReloadSlotReservation reservation)
        {
            return reservation != null && !reservation.Released &&
                ReferenceEquals(state.Selection, reservation.Selection) &&
                state.Index == reservation.Index;
        }

        // The nested Rapid Reload choice state that SelectFeature.Apply creates
        // for a selection-valued item.
        private static FeatureSelectionState FindRapidReloadChildState(
            LevelUpController controller, RapidReloadGateContext ctx)
        {
            return controller.State.Selections.FirstOrDefault(value =>
                ReferenceEquals(value.Selection, ctx.Parent));
        }

        // R2: the parent and the exact child are recorded separately. A valid
        // parent may legitimately survive a pending-build change that only
        // invalidates its child, so "Rapid Reload exists somewhere" is never
        // used as the verdict.
        private static JObject DescribeRapidReloadHold(RapidReloadGateContext ctx,
            LevelUpController controller, BlueprintFeature trackedChild)
        {
            FeatureSelectionState[] parentHolders = controller.State.Selections
                .Where(value => value.SelectedItem != null &&
                    ReferenceEquals(value.SelectedItem.Feature, ctx.Parent)).ToArray();
            FeatureSelectionState nested = FindRapidReloadChildState(controller, ctx);
            BlueprintFeature selectedChild = nested == null ||
                nested.SelectedItem == null ? null : nested.SelectedItem.Feature;
            return new JObject {
                ["parentHeld"] = parentHolders.Length > 0,
                ["parentHoldCount"] = parentHolders.Length,
                ["nestedStatePresent"] = nested != null,
                ["selectedChildName"] = selectedChild == null ? "<none>" :
                    selectedChild.name,
                ["trackedChild"] = trackedChild.name,
                ["trackedChildStillSelected"] = selectedChild != null &&
                    ReferenceEquals(selectedChild, trackedChild),
                ["trackedChildRank"] =
                    controller.Preview.Progression.Features.GetRank(trackedChild) };
        }

        // LevelUpState.IsComplete is the exact gate CharacterBuildController.Next
        // enforces before it calls Commit. The blocking set names which pending
        // selections keep it false.
        private static JObject DescribeRapidReloadCompletion(
            RapidReloadGateContext ctx, LevelUpController controller)
        {
            LevelUpState levelUpState = controller.State;
            UnitDescriptor preview = controller.Preview;
            FeatureSelectionState[] blockers = levelUpState.Selections.Where(value =>
                !value.Selected && value.Selection != null &&
                value.CanSelectAnything(levelUpState, preview)).ToArray();
            return new JObject {
                ["isComplete"] = levelUpState.IsComplete(),
                ["remainingSelections"] = levelUpState.RemainingSelections(),
                ["blockers"] = new JArray(blockers.Select(value =>
                    (value.Selection as BlueprintScriptableObject) == null ?
                        "<unnamed>" : ((BlueprintScriptableObject)value.Selection).name)),
                ["targetBlocks"] = blockers.Any(value =>
                    ReferenceEquals(value.Selection, ctx.Parent)),
                ["skillPointsRemaining"] = levelUpState.SkillPointsRemaining,
                ["attributePoints"] = levelUpState.AttributePoints };
        }

        // R4: the native completion boundary. CharacterBuildController.Next
        // calls LevelUpController.Commit only when LevelUpState.IsComplete() is
        // true, and Commit then applies the level through ApplyLevelup. The
        // detached harness cannot run Commit itself (it disposes the preview and
        // touches the unit view), so it enforces the same gate and then calls
        // the exact method Commit calls. An incomplete build is never applied
        // through this path, so a successful confirmation can never be inferred
        // from the resulting facts alone.
        private bool ConfirmRapidReloadLevel(RapidReloadGateContext ctx,
            LevelUpController controller, UnitDescriptor descriptor, JObject row,
            string completeKey, string appliedKey)
        {
            JObject completion = DescribeRapidReloadCompletion(ctx, controller);
            row[completeKey + "Detail"] = completion;
            row[completeKey] = (bool)completion["isComplete"];
            row["confirmationRoute"] = "LevelUpState.IsComplete gate, then " +
                "LevelUpController.ApplyLevelup (the call Commit makes)";
            if (!(bool)completion["isComplete"])
            {
                row[appliedKey] = false;
                return false;
            }
            ctx.Apply.Invoke(controller, new object[] { descriptor });
            row[appliedKey] = true;
            return true;
        }

        private static JArray DescribeRapidReloadChildEligibility(
            RapidReloadGateContext ctx, UnitDescriptor descriptor, LevelUpState levelUpState)
        {
            return new JArray(ctx.Children.Select(child =>
                child.MeetsPrerequisites(null, descriptor, levelUpState)));
        }

        private static IFeatureSelectionItem FindItem(IFeatureSelection selection,
            UnitDescriptor preview, BlueprintFeature feature)
        {
            return selection.ExtractSelectionItems(preview, preview)
                .FirstOrDefault(item => item != null &&
                    ReferenceEquals(item.Feature, feature));
        }

        // ------------------------------------------------------------------
        // Case A/B: no firearm proficiency, both feat catalogs
        // ------------------------------------------------------------------
        private JObject RunRapidReloadRefusedParent(RapidReloadGateContext ctx,
            string label, BlueprintFeatureSelection slotSelection)
        {
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            var row = new JObject { ["case"] = label, ["slot"] = slotSelection.name };
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Fighter, null);
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, slotSelection);
                row["requirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["fixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                // A Fighter already carries native martial proficiency, so the
                // crossbow control is satisfied by the same fixture.
                row["previewCrossbowProficiency"] =
                    controller.Preview.Proficiencies.Contains(WeaponCategory.LightCrossbow) ||
                    controller.Preview.Proficiencies.Contains(WeaponCategory.HeavyCrossbow);
                FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                    reservation);
                if (!RapidReloadReservationHolds(reserved) || slot == null) return row;
                IFeatureSelectionItem parentItem = FindItem(slotSelection,
                    controller.Preview, ctx.Parent);
                row["parentOffered"] = parentItem != null;
                if (parentItem == null) return row;
                row["parentCanSelect"] = slotSelection.CanSelect(controller.Preview,
                    controller.State, slot, parentItem);
                row["parentSelected"] = controller.SelectFeature(slot, parentItem);
                row["hold"] = DescribeRapidReloadHold(ctx, controller, ctx.Children[0]);
                row["childEligibility"] = DescribeRapidReloadChildEligibility(ctx,
                    controller.Preview, controller.State);
                row["completionWhileReserved"] =
                    DescribeRapidReloadCompletion(ctx, controller);
                // The reservation is no longer needed: release it, fill the
                // remaining choices legally and finish the level through the
                // native completion gate. Releasing never repairs a Rapid Reload
                // state, because the resolver skips nested parent selections and
                // never offers the parent as a filler item.
                ReleaseRapidReloadReservation(reservation);
                row["reservationReleasedBeforeConfirmation"] = true;
                row["requirementsAfterRelease"] =
                    ResolveRapidReloadNonTargetRequirements(ctx, controller, reservation);
                ConfirmRapidReloadLevel(ctx, controller, descriptor, row,
                    "completeBeforeConfirmation", "confirmationApplied");
                controller.Cancel();
                controller = null;
                row["acquiredParent"] =
                    descriptor.Progression.Features.GetRank(ctx.Parent) > 0;
                row["acquiredChild"] = ctx.Children.Any(child =>
                    descriptor.Progression.Features.GetRank(child) > 0);
            }
            finally
            {
                // The evaluator scores row["cleanupError"], so a failed
                // cancellation here cannot pass unnoticed.
                CloseRapidReloadVisit(controller, row, null,
                    (string)row["case"]);
                unit.Dispose();
            }
            return row;
        }

        // ------------------------------------------------------------------
        // Case B: legal parent, out-of-scope firearm child
        // ------------------------------------------------------------------
        private JObject RunRapidReloadScopedChildRefusal(RapidReloadGateContext ctx,
            string label, BlueprintFeature proficiency, int refusedIndex, int legalIndex)
        {
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            var row = new JObject { ["case"] = label, ["slot"] = ctx.Basic.name,
                ["refusedChild"] = ctx.Children[refusedIndex].name,
                ["legalChild"] = ctx.Children[legalIndex].name };
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                GrantFixtureFact(descriptor, proficiency);
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Fighter, null);
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, ctx.Basic);
                row["requirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["fixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                    reservation);
                if (!RapidReloadReservationHolds(reserved) || slot == null) return row;
                IFeatureSelectionItem parentItem = FindItem(ctx.Basic,
                    controller.Preview, ctx.Parent);
                row["parentCanSelect"] = parentItem != null && ctx.Basic.CanSelect(
                    controller.Preview, controller.State, slot, parentItem);
                row["parentSelected"] = parentItem != null &&
                    controller.SelectFeature(slot, parentItem);
                FeatureSelectionState childState = FindRapidReloadChildState(controller, ctx);
                row["childStatePresent"] = childState != null;
                if (childState == null) return row;
                IFeatureSelectionItem refused = FindItem(childState.Selection,
                    controller.Preview, ctx.Children[refusedIndex]);
                row["refusedChildOffered"] = refused != null;
                row["refusedChildCanSelect"] = refused != null &&
                    childState.Selection.CanSelect(controller.Preview, controller.State,
                        childState, refused);
                row["refusedChildSelected"] = refused != null &&
                    controller.SelectFeature(childState, refused);
                // Everything below is observed before any cleanup or repair.
                childState = FindRapidReloadChildState(controller, ctx);
                row["childStateSelectedAfterRefusal"] = childState != null &&
                    childState.Selected;
                JObject blockedCompletion = DescribeRapidReloadCompletion(ctx, controller);
                row["completionWhileEmpty"] = blockedCompletion;
                row["completeWhileEmpty"] = (bool)blockedCompletion["isComplete"];
                row["targetBlocksCompletion"] = (bool)blockedCompletion["targetBlocks"];
                row["parentRankWhileEmpty"] =
                    controller.Preview.Progression.Features.GetRank(ctx.Parent);
                row["refusedChildRankWhileEmpty"] = controller.Preview.Progression
                    .Features.GetRank(ctx.Children[refusedIndex]);
                // Attribution: the same build completes once a legal firearm is
                // chosen, so the block belongs to the Rapid Reload choice.
                IFeatureSelectionItem legal = childState == null ? null :
                    FindItem(childState.Selection, controller.Preview,
                        ctx.Children[legalIndex]);
                row["legalChildSelected"] = legal != null && childState != null &&
                    controller.SelectFeature(childState, legal);
                JObject afterLegal = DescribeRapidReloadCompletion(ctx, controller);
                row["completionAfterLegalChoice"] = afterLegal;
                row["targetBlocksCompletionAfterLegalChoice"] = (bool)afterLegal["targetBlocks"];
                ConfirmRapidReloadLevel(ctx, controller, descriptor, row,
                    "completeBeforeConfirmation", "confirmationApplied");
                controller.Cancel();
                controller = null;
                row["acquiredLegalChild"] = descriptor.Progression.Features
                    .GetRank(ctx.Children[legalIndex]) > 0;
                row["acquiredRefusedChild"] = descriptor.Progression.Features
                    .GetRank(ctx.Children[refusedIndex]) > 0;
            }
            finally
            {
                // The evaluator scores row["cleanupError"], so a failed
                // cancellation here cannot pass unnoticed.
                CloseRapidReloadVisit(controller, row, null,
                    (string)row["case"]);
                unit.Dispose();
            }
            return row;
        }

        // ------------------------------------------------------------------
        // Case B: a held parent with no firearm chosen. This is a defensive
        // probe: it deliberately applies a build the native completion gate
        // refuses, to prove the empty choice banks nothing. It is never a claim
        // that normal confirmation was permitted.
        // ------------------------------------------------------------------
        private JObject RunRapidReloadEmptySelection(RapidReloadGateContext ctx,
            string label, BlueprintFeature proficiency)
        {
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            var row = new JObject { ["case"] = label, ["slot"] = ctx.Basic.name };
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                GrantFixtureFact(descriptor, proficiency);
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Fighter, null);
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, ctx.Basic);
                row["requirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["fixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                    reservation);
                if (!RapidReloadReservationHolds(reserved) || slot == null) return row;
                IFeatureSelectionItem parentItem = FindItem(ctx.Basic,
                    controller.Preview, ctx.Parent);
                row["parentSelected"] = parentItem != null &&
                    controller.SelectFeature(slot, parentItem);
                FeatureSelectionState childState = FindRapidReloadChildState(controller, ctx);
                row["childStatePresent"] = childState != null;
                row["childStateSelected"] = childState != null && childState.Selected;
                // No firearm is chosen and nothing is unselected first: this is
                // the state a player would try to confirm.
                JObject completion = DescribeRapidReloadCompletion(ctx, controller);
                row["completion"] = completion;
                row["completeWhileEmpty"] = (bool)completion["isComplete"];
                row["targetBlocksCompletion"] = (bool)completion["targetBlocks"];
                row["appliedWithoutNativeCompletion"] = true;
                row["confirmationRoute"] = "defensive probe: ApplyLevelup on a build " +
                    "LevelUpState.IsComplete refuses; never a native confirmation";
                ctx.Apply.Invoke(controller, new object[] { descriptor });
                controller.Cancel();
                controller = null;
                row["acquiredParent"] =
                    descriptor.Progression.Features.GetRank(ctx.Parent) > 0;
                row["acquiredAnyChild"] = ctx.RegisteredChildren.Any(child =>
                    descriptor.Progression.Features.GetRank(child) > 0);
            }
            finally
            {
                // The evaluator scores row["cleanupError"], so a failed
                // cancellation here cannot pass unnoticed.
                CloseRapidReloadVisit(controller, row, null,
                    (string)row["case"]);
                unit.Dispose();
            }
            return row;
        }

        // ------------------------------------------------------------------
        // Legal acquisition through a real feat slot
        // ------------------------------------------------------------------
        private JObject RunRapidReloadAcquisition(RapidReloadGateContext ctx,
            string label, BlueprintCharacterClass characterClass,
            BlueprintArchetype archetype, BlueprintFeatureSelection slotSelection,
            int childIndex, BlueprintFeature proficiency)
        {
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            var row = new JObject { ["case"] = label, ["class"] = characterClass.name,
                ["slot"] = slotSelection.name, ["child"] = ctx.Children[childIndex].name };
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                if (proficiency != null) GrantFixtureFact(descriptor, proficiency);
                controller = OpenRapidReloadVisit(ctx, descriptor, characterClass, archetype);
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, slotSelection);
                row["requirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["fixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                    reservation);
                if (!RapidReloadReservationHolds(reserved) || slot == null) return row;
                IFeatureSelectionItem parentItem = FindItem(slotSelection,
                    controller.Preview, ctx.Parent);
                row["parentOffered"] = parentItem != null;
                row["parentCanSelect"] = parentItem != null && slotSelection.CanSelect(
                    controller.Preview, controller.State, slot, parentItem);
                row["parentSelected"] = parentItem != null &&
                    controller.SelectFeature(slot, parentItem);
                FeatureSelectionState childState = FindRapidReloadChildState(controller, ctx);
                row["childStatePresent"] = childState != null;
                IFeatureSelectionItem childItem = childState == null ? null :
                    FindItem(childState.Selection, controller.Preview,
                        ctx.Children[childIndex]);
                row["childOffered"] = childItem != null;
                row["childCanSelect"] = childItem != null &&
                    childState.Selection.CanSelect(controller.Preview, controller.State,
                        childState, childItem);
                row["childSelected"] = childItem != null &&
                    controller.SelectFeature(childState, childItem);
                ConfirmRapidReloadLevel(ctx, controller, descriptor, row,
                    "completeBeforeConfirmation", "confirmationApplied");
                controller.Cancel();
                controller = null;
                row["acquiredChild"] = descriptor.Progression.Features
                    .GetRank(ctx.Children[childIndex]) > 0;
                row["otherChildRanks"] = new JArray(ctx.Children.Select(child =>
                    descriptor.Progression.Features.GetRank(child)));
            }
            finally
            {
                // The evaluator scores row["cleanupError"], so a failed
                // cancellation here cannot pass unnoticed.
                CloseRapidReloadVisit(controller, row, null,
                    (string)row["case"]);
                unit.Dispose();
            }
            return row;
        }

        // ------------------------------------------------------------------
        // F1/R1: a genuine pending class change inside one live transaction
        // ------------------------------------------------------------------
        private JObject RunRapidReloadPendingClassChange(RapidReloadGateContext ctx,
            IList<string> failures)
        {
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            var row = new JObject();
            int controllerInstances = 0;
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Gunslinger, null);
                controllerInstances++;
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, ctx.Basic);
                row["requirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["gunslingerFixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                row["gunslingerLevel"] =
                    controller.Preview.Progression.GetClassLevel(ctx.Gunslinger);
                if (!HoldRapidReloadChoice(ctx, controller, reservation, 0, row, "held"))
                    failures.Add("pending-class-change:initial-hold-failed");
                row["heldBeforeChange"] = (bool)row["held"];
                FeatureSelectionState heldState = FindRapidReloadChildState(controller, ctx);
                row["heldChildName"] = heldState == null || heldState.SelectedItem == null ||
                    heldState.SelectedItem.Feature == null ? "<none>" :
                    heldState.SelectedItem.Feature.name;

                // The native change-class operation on the SAME controller. It
                // removes the previous SelectClass action, rebuilds the preview
                // and re-checks every pending action; nothing here cancels the
                // visit or constructs a second controller.
                if (!controller.SelectClass(ctx.Fighter, true))
                    throw new InvalidOperationException(
                        "Native class change to Fighter was rejected.");
                row["fighterFixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                row["fighterGunslingerLevel"] =
                    controller.Preview.Progression.GetClassLevel(ctx.Gunslinger);
                row["fighterLevelInPreview"] =
                    controller.Preview.Progression.GetClassLevel(ctx.Fighter);
                row["parentEligibleAfterChange"] = ctx.Parent.MeetsPrerequisites(null,
                    controller.Preview, controller.State);
                row["childEligibilityAfterChange"] = DescribeRapidReloadChildEligibility(
                    ctx, controller.Preview, controller.State);
                // The parent itself stops qualifying as a Fighter, so both the
                // parent hold and the exact child must be gone.
                row["holdAfterChange"] = DescribeRapidReloadHold(ctx, controller,
                    ctx.Children[0]);

                // Change back through the same native route.
                if (!controller.SelectClass(ctx.Gunslinger, true))
                    throw new InvalidOperationException(
                        "Native class change back to Gunslinger was rejected.");
                row["restoredFixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                row["parentEligibleAfterRestore"] = ctx.Parent.MeetsPrerequisites(null,
                    controller.Preview, controller.State);
                row["childEligibilityAfterRestore"] = DescribeRapidReloadChildEligibility(
                    ctx, controller.Preview, controller.State);
                row["holdAfterRestore"] = DescribeRapidReloadHold(ctx, controller,
                    ctx.Children[0]);

                row["requirementsAfterRestore"] = ResolveRapidReloadNonTargetRequirements(
                    ctx, controller, reservation);
                row["reservationAfterRestore"] =
                    DescribeRapidReloadReservation(controller, reservation);
                if (!HoldRapidReloadChoice(ctx, controller, reservation, 0, row, "rehold"))
                    failures.Add("pending-class-change:rehold-failed");
                row["reheldBeforeSecondChange"] = (bool)row["rehold"];

                if (!controller.SelectClass(ctx.Fighter, true))
                    throw new InvalidOperationException(
                        "Native second class change to Fighter was rejected.");
                row["holdAfterSecondChange"] = DescribeRapidReloadHold(ctx, controller,
                    ctx.Children[0]);
                // Native invalidation is observed above; only then is the test
                // reservation released so the unrelated Fighter level can be
                // finished legally.
                ReleaseRapidReloadReservation(reservation);
                row["reservationReleasedBeforeConfirmation"] = true;
                row["requirementsBeforeConfirmation"] =
                    ResolveRapidReloadNonTargetRequirements(ctx, controller, reservation);
                ConfirmRapidReloadLevel(ctx, controller, descriptor, row,
                    "completeBeforeConfirmation", "confirmationApplied");
                controller.Cancel();
                controller = null;
                row["confirmedFighterLevel"] =
                    descriptor.Progression.GetClassLevel(ctx.Fighter);
                row["confirmedGunslingerLevel"] =
                    descriptor.Progression.GetClassLevel(ctx.Gunslinger);
                row["acquiredParentAfterConfirmation"] =
                    descriptor.Progression.Features.GetRank(ctx.Parent) > 0;
                row["acquiredAnyChildAfterConfirmation"] = ctx.RegisteredChildren.Any(
                    child => descriptor.Progression.Features.GetRank(child) > 0);
            }
            finally
            {
                CloseRapidReloadVisit(controller, row, failures,
                    "pending-class-change");
                unit.Dispose();
                row["controllerInstances"] = controllerInstances;
            }
            return row;
        }

        // ------------------------------------------------------------------
        // F1/R2: a genuine pending archetype (proficiency-scope) change.
        // Musket Master still qualifies for the Rapid Reload parent through
        // two-handed proficiency, so the engine may legitimately keep the
        // parent while clearing the now-invalid Pistol child. Both outcomes are
        // accepted; what is checked is the exact child.
        // ------------------------------------------------------------------
        private JObject RunRapidReloadPendingArchetypeChange(RapidReloadGateContext ctx,
            IList<string> failures)
        {
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            var row = new JObject();
            int controllerInstances = 0;
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Gunslinger, null);
                controllerInstances++;
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, ctx.Basic);
                row["requirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["baseFixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                // Pistol: legal for the base Gunslinger, illegal for a Musket
                // Master.
                if (!HoldRapidReloadChoice(ctx, controller, reservation, 0, row, "held"))
                    failures.Add("pending-archetype-change:initial-hold-failed");
                row["heldBeforeChange"] = (bool)row["held"];

                // The native pending-archetype operation on the same controller.
                row["archetypeApplied"] = controller.AddArchetype(ctx.MusketMaster);
                row["archetypeFixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                row["isMusketMasterAfterChange"] =
                    controller.Preview.Progression.IsArchetype(ctx.MusketMaster);
                row["automaticMusketRankAfterChange"] =
                    controller.Preview.Progression.Features.GetRank(ctx.Children[1]);
                row["parentEligibleAfterChange"] = ctx.Parent.MeetsPrerequisites(null,
                    controller.Preview, controller.State);
                row["childEligibilityAfterChange"] = DescribeRapidReloadChildEligibility(
                    ctx, controller.Preview, controller.State);
                row["holdAfterChange"] = DescribeRapidReloadHold(ctx, controller,
                    ctx.Children[0]);
                row["invalidChildRankAfterChange"] =
                    controller.Preview.Progression.Features.GetRank(ctx.Children[0]);
                JObject selectability = DescribeRapidReloadChildSelectability(ctx,
                    controller, reservation, failures, "pending-archetype-change");
                row["childSelectableAfterChange"] = selectability["selectable"];
                row["childSelectabilitySource"] = selectability["source"];
                row["selectabilityProbeCreatedSelection"] =
                    selectability["probeCreatedSelection"];

                controller.RemoveArchetype(ctx.MusketMaster);
                row["archetypeRemoved"] =
                    !controller.Preview.Progression.IsArchetype(ctx.MusketMaster);
                row["restoredFixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                row["automaticMusketRankAfterRemoval"] =
                    controller.Preview.Progression.Features.GetRank(ctx.Children[1]);
                row["childEligibilityAfterRemoval"] = DescribeRapidReloadChildEligibility(
                    ctx, controller.Preview, controller.State);
                controller.Cancel();
                controller = null;
            }
            finally
            {
                CloseRapidReloadVisit(controller, row, failures,
                    "pending-archetype-change");
                unit.Dispose();
                row["controllerInstances"] = controllerInstances;
            }
            return row;
        }

        // Reports which firearms the native child selection would actually
        // accept. If the engine legitimately retained the Rapid Reload parent,
        // that retained nested state is used directly and nothing is created or
        // withdrawn. Only when the transition removed the parent does this open
        // a probe in the reserved slot, and only that probe-created selection is
        // withdrawn afterwards.
        private static JObject DescribeRapidReloadChildSelectability(
            RapidReloadGateContext ctx, LevelUpController controller,
            RapidReloadSlotReservation reservation, IList<string> failures, string label)
        {
            bool probeCreated = false;
            FeatureSelectionState childState = FindRapidReloadChildState(controller, ctx);
            if (childState == null)
            {
                FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                    reservation);
                if (slot == null || slot.Selected)
                {
                    failures.Add(label + ":selectability-reserved-slot-unavailable");
                    return new JObject { ["source"] = "<reserved-slot-unavailable>",
                        ["probeCreatedSelection"] = false, ["selectable"] = new JArray() };
                }
                IFeatureSelectionItem parentItem = FindItem(reservation.Selection,
                    controller.Preview, ctx.Parent);
                if (parentItem == null || !controller.SelectFeature(slot, parentItem))
                {
                    failures.Add(label + ":selectability-parent-refused");
                    return new JObject { ["source"] = "<parent-refused>",
                        ["probeCreatedSelection"] = false, ["selectable"] = new JArray() };
                }
                probeCreated = true;
                childState = FindRapidReloadChildState(controller, ctx);
                if (childState == null)
                {
                    failures.Add(label + ":selectability-child-state-absent");
                    WithdrawRapidReloadProbe(controller, reservation);
                    return new JObject { ["source"] = "<child-state-absent>",
                        ["probeCreatedSelection"] = true, ["selectable"] = new JArray() };
                }
            }
            var selectable = new JArray(ctx.Children.Select(child =>
            {
                IFeatureSelectionItem item = FindItem(childState.Selection,
                    controller.Preview, child);
                return item != null && childState.Selection.CanSelect(controller.Preview,
                    controller.State, childState, item);
            }));
            if (probeCreated) WithdrawRapidReloadProbe(controller, reservation);
            return new JObject {
                ["source"] = probeCreated ? "probe" : "retained-nested-state",
                ["probeCreatedSelection"] = probeCreated,
                ["selectable"] = selectable };
        }

        private static void WithdrawRapidReloadProbe(LevelUpController controller,
            RapidReloadSlotReservation reservation)
        {
            FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                reservation);
            if (slot != null && slot.Selected) controller.UnselectFeature(slot);
        }

        private static bool HoldRapidReloadChoice(RapidReloadGateContext ctx,
            LevelUpController controller, RapidReloadSlotReservation reservation,
            int childIndex, JObject row, string key)
        {
            FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                reservation);
            if (slot == null || slot.Selected) { row[key] = false; return false; }
            IFeatureSelectionItem parentItem = FindItem(reservation.Selection,
                controller.Preview, ctx.Parent);
            if (parentItem == null || !controller.SelectFeature(slot, parentItem))
            { row[key] = false; return false; }
            FeatureSelectionState childState = FindRapidReloadChildState(controller, ctx);
            IFeatureSelectionItem childItem = childState == null ? null :
                FindItem(childState.Selection, controller.Preview, ctx.Children[childIndex]);
            bool held = childItem != null && controller.SelectFeature(childState, childItem);
            // "Held" means the engine actually records the exact choice, not
            // merely that SelectFeature returned true.
            FeatureSelectionState confirmed = FindRapidReloadChildState(controller, ctx);
            held = held && confirmed != null && confirmed.SelectedItem != null &&
                ReferenceEquals(confirmed.SelectedItem.Feature, ctx.Children[childIndex]);
            row[key] = held;
            return held;
        }

        // ------------------------------------------------------------------
        // F3: a natively built Musket Master
        // ------------------------------------------------------------------
        private JObject RunRapidReloadMusketMaster(RapidReloadGateContext ctx,
            IList<string> failures)
        {
            LevelEntry levelOne = (ctx.MusketMaster.AddFeatures ??
                Array.Empty<LevelEntry>()).FirstOrDefault(value => value.Level == 1);
            var row = new JObject {
                ["archetype"] = ctx.MusketMaster.name,
                ["levelOneGrantsRapidReloadMusket"] = levelOne != null &&
                    levelOne.Features != null && levelOne.Features.Any(value =>
                        ReferenceEquals(value, ctx.Children[1])),
                ["levelOneFeatures"] = new JArray((levelOne == null ||
                        levelOne.Features == null ? new List<BlueprintFeatureBase>() :
                        levelOne.Features).Select(value =>
                            value == null ? "<null>" : value.name)) };
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                // Nothing is granted by hand: the archetype's own level-one
                // package supplies both the two-handed proficiency scope and the
                // automatic Rapid Reload (Musket) grant.
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Gunslinger,
                    ctx.MusketMaster);
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, ctx.Basic);
                row["requirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["fixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                row["gunslingerLevel"] =
                    controller.Preview.Progression.GetClassLevel(ctx.Gunslinger);
                row["isMusketMasterArchetype"] =
                    controller.Preview.Progression.IsArchetype(ctx.MusketMaster);
                row["automaticMusketRank"] =
                    controller.Preview.Progression.Features.GetRank(ctx.Children[1]);
                FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                    reservation);
                if (!RapidReloadReservationHolds(reserved) || slot == null) return row;
                IFeatureSelectionItem parentItem = FindItem(ctx.Basic,
                    controller.Preview, ctx.Parent);
                row["parentCanSelect"] = parentItem != null && ctx.Basic.CanSelect(
                    controller.Preview, controller.State, slot, parentItem);
                row["parentSelected"] = parentItem != null &&
                    controller.SelectFeature(slot, parentItem);
                FeatureSelectionState childState = FindRapidReloadChildState(controller, ctx);
                row["childStatePresent"] = childState != null;
                if (childState == null) return row;
                IFeatureSelectionItem owned = FindItem(childState.Selection,
                    controller.Preview, ctx.Children[1]);
                row["ownedChildOffered"] = owned != null;
                row["ownedChildCanSelect"] = owned != null &&
                    childState.Selection.CanSelect(controller.Preview, controller.State,
                        childState, owned);
                row["ownedChildSelected"] = owned != null &&
                    controller.SelectFeature(childState, owned);
                childState = FindRapidReloadChildState(controller, ctx);
                IFeatureSelectionItem incompatible = childState == null ? null :
                    FindItem(childState.Selection, controller.Preview, ctx.Children[0]);
                row["incompatibleChildOffered"] = incompatible != null;
                row["incompatibleChildCanSelect"] = incompatible != null &&
                    childState.Selection.CanSelect(controller.Preview, controller.State,
                        childState, incompatible);
                IFeatureSelectionItem legal = childState == null ? null :
                    FindItem(childState.Selection, controller.Preview, ctx.Children[2]);
                row["legalChildCanSelect"] = legal != null && childState != null &&
                    childState.Selection.CanSelect(controller.Preview, controller.State,
                        childState, legal);
                row["legalChildSelected"] = legal != null && childState != null &&
                    controller.SelectFeature(childState, legal);
                row["featSlotsConsumed"] = controller.State.Selections.Count(value =>
                    value.SelectedItem != null &&
                    ReferenceEquals(value.SelectedItem.Feature, ctx.Parent));
                ConfirmRapidReloadLevel(ctx, controller, descriptor, row,
                    "completeBeforeConfirmation", "confirmationApplied");
                controller.Cancel();
                controller = null;
                row["acquiredLegalChild"] =
                    descriptor.Progression.Features.GetRank(ctx.Children[2]) > 0;
                row["acquiredOwnedChildRank"] =
                    descriptor.Progression.Features.GetRank(ctx.Children[1]);
                row["acquiredIncompatibleChildRank"] =
                    descriptor.Progression.Features.GetRank(ctx.Children[0]);
            }
            finally
            {
                CloseRapidReloadVisit(controller, row, failures,
                    "musket-master");
                unit.Dispose();
            }
            return row;
        }

        // ------------------------------------------------------------------
        // F4: real Gunslinger levels, no firearm-proficiency facts
        // ------------------------------------------------------------------
        private JObject RunRapidReloadClassIdentityControl(RapidReloadGateContext ctx,
            IList<string> failures)
        {
            UnitEntityData unit = CreateDisposableUnit();
            LevelUpController controller = null;
            var row = new JObject();
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                // A genuine Gunslinger, confirmed through the native completion
                // gate. Nothing is reserved here: this visit has no target
                // operation, so every requirement including the feat slot is
                // settled legally.
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Gunslinger, null);
                row["buildRequirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, null);
                ConfirmRapidReloadLevel(ctx, controller, descriptor, row,
                    "buildCompleteBeforeConfirmation", "buildConfirmationApplied");
                controller.Cancel();
                controller = null;
                row["builtGunslingerLevel"] =
                    descriptor.Progression.GetClassLevel(ctx.Gunslinger);
                row["builtFixture"] = DescribeRapidReloadFixture(ctx, descriptor);
                if (descriptor.Progression.Features.GetRank(ctx.Full) <= 0)
                    failures.Add("class-identity-control:gunslinger-build-had-no-proficiency");

                // Fixture-local removal only: the owning class-proficiency
                // package first, then any surviving proficiency fact.
                if (descriptor.HasFact(ctx.GunslingerProficiencies))
                    descriptor.RemoveFact(ctx.GunslingerProficiencies);
                foreach (BlueprintFeature proficiency in new[] { ctx.Full,
                    ctx.OneHanded, ctx.TwoHanded })
                    if (descriptor.HasFact(proficiency)) descriptor.RemoveFact(proficiency);
                row["committedGunslingerLevel"] =
                    descriptor.Progression.GetClassLevel(ctx.Gunslinger);
                row["committedFixture"] = DescribeRapidReloadFixture(ctx, descriptor);
                row["parentEligible"] = ctx.Parent.MeetsPrerequisites(null, descriptor, null);
                row["childEligibility"] = DescribeRapidReloadChildEligibility(ctx,
                    descriptor, null);

                // Exercise selection on a live preview. The next level is taken
                // as a Fighter so the Gunslinger progression is not re-applied;
                // Gunslinger class identity is preserved and re-proved below. A
                // second character level offers no new ordinary feat, so the
                // Fighter bonus combat feat is the realistic slot and the
                // ordinary catalog is only the fallback.
                controller = OpenRapidReloadVisit(ctx, descriptor, ctx.Fighter, null);
                RapidReloadSlotReservation reservation = ReserveRapidReloadSlot(
                    controller, ctx.FighterFeats, ctx.Basic);
                row["previewRequirements"] = ResolveRapidReloadNonTargetRequirements(ctx,
                    controller, reservation);
                JObject reserved = DescribeRapidReloadReservation(controller, reservation);
                row["reservation"] = reserved;
                row["slotPresent"] = RapidReloadReservationHolds(reserved);
                row["previewGunslingerLevel"] =
                    controller.Preview.Progression.GetClassLevel(ctx.Gunslinger);
                row["previewFixture"] = DescribeRapidReloadFixture(ctx, controller.Preview);
                FeatureSelectionState slot = ResolveReservedRapidReloadSlot(controller,
                    reservation);
                if (RapidReloadReservationHolds(reserved) && slot != null)
                {
                    IFeatureSelectionItem parentItem = FindItem(reservation.Selection,
                        controller.Preview, ctx.Parent);
                    row["parentOffered"] = parentItem != null;
                    row["parentCanSelect"] = parentItem != null &&
                        reservation.Selection.CanSelect(controller.Preview,
                            controller.State, slot, parentItem);
                    row["parentSelected"] = parentItem != null &&
                        controller.SelectFeature(slot, parentItem);
                }
                controller.Cancel();
                controller = null;
            }
            finally
            {
                CloseRapidReloadVisit(controller, row, failures,
                    "class-identity-control");
                unit.Dispose();
            }
            return row;
        }

        // Caller-side cleanup for a controller the caller owns. It shares the
        // one cancellation boundary with the failed-initialization path, and a
        // null controller is a no-op, so a visit the helper already cancelled is
        // never cancelled twice.
        //
        // R6: a cancellation failure here is not merely described on the row.
        // It is also added to the failure collection that decides the case's
        // status, so a successfully initialised visit whose cleanup throws can
        // never report PASS. `failures` may be null only for rows whose
        // evaluator scores `cleanupError` instead (see EvaluateCleanup). This
        // never throws, so the caller's finally still reaches unit.Dispose()
        // and an in-flight body exception is never masked.
        private static bool CloseRapidReloadVisit(LevelUpController controller,
            JObject row, IList<string> failures, string label)
        {
            return RapidReloadVisitCleanupRules.Report(
                TryCancelRapidReloadVisit(controller), row, failures, label);
        }

        // ------------------------------------------------------------------
        // R5: the failure window inside OpenRapidReloadVisit
        // ------------------------------------------------------------------
        // Musket Master is not a Fighter archetype, so AddArchetype.Check
        // refuses it deterministically after the controller already exists.
        // That reproduces the exact window R5 names: a controller was obtained,
        // a later initialisation step failed, and the caller's variable was
        // never assigned, so only the helper can cancel it.
        private JObject RunRapidReloadVisitOwnershipCheck(RapidReloadGateContext ctx,
            IList<string> failures)
        {
            var row = new JObject { ["case"] = "R5.failed-initialization-cleanup" };
            UnitEntityData failureUnit = CreateDisposableUnit();
            LevelUpController callerController = null;
            LevelUpController createdInFailure = null;
            try
            {
                try
                {
                    callerController = OpenRapidReloadVisit(ctx,
                        failureUnit.Descriptor, ctx.Fighter, ctx.MusketMaster,
                        out createdInFailure);
                    row["initializationFailed"] = false;
                }
                catch (InvalidOperationException setupError)
                {
                    row["initializationFailed"] = true;
                    row["setupErrorType"] = setupError.GetType().Name;
                    row["setupErrorMessage"] = setupError.Message;
                    row["setupErrorHasStack"] =
                        !string.IsNullOrEmpty(setupError.StackTrace);
                }
                row["controllerCreatedInFailureWindow"] = createdInFailure != null;
                row["callerControllerStillNull"] = callerController == null;
                if (!(bool)row["initializationFailed"])
                    failures.Add("visit-ownership:forced-rejection-did-not-occur");
                else
                {
                    if ((string)row["setupErrorMessage"] == null ||
                        !((string)row["setupErrorMessage"]).Contains(
                            "Native archetype selection rejected"))
                        failures.Add("visit-ownership:original-setup-error-not-preserved");
                    if (!(bool)row["setupErrorHasStack"])
                        failures.Add("visit-ownership:setup-error-lost-its-stack");
                }
                if (!(bool)row["controllerCreatedInFailureWindow"])
                    failures.Add("visit-ownership:no-controller-in-failure-window");
                if (!(bool)row["callerControllerStillNull"])
                    failures.Add("visit-ownership:caller-unexpectedly-owned-the-controller");
            }
            finally
            {
                // The caller owns nothing here, so this cancels nothing; the
                // disposable unit still reaches its cleanup boundary.
                CloseRapidReloadVisit(callerController, row, failures,
                    "visit-ownership.failed-initialization");
                failureUnit.Dispose();
            }

            // Successful initialisation hands the same controller to the caller
            // and leaves its cleanup to the caller's own finally block.
            UnitEntityData successUnit = CreateDisposableUnit();
            LevelUpController successController = null;
            try
            {
                LevelUpController createdInSuccess;
                successController = OpenRapidReloadVisit(ctx,
                    successUnit.Descriptor, ctx.Fighter, null, out createdInSuccess);
                row["successfulInitializationReturnsCreatedController"] =
                    successController != null &&
                    ReferenceEquals(successController, createdInSuccess);
                row["returnedControllerUsable"] = successController != null &&
                    successController.State != null && successController.Preview != null;
                if (!(bool)row["successfulInitializationReturnsCreatedController"])
                    failures.Add("visit-ownership:success-path-did-not-transfer-ownership");
                if (!(bool)row["returnedControllerUsable"])
                    failures.Add("visit-ownership:returned-controller-unusable");
            }
            finally
            {
                // R6: a cancellation failure on this caller-owned success path
                // is added to the scored failure collection, not merely
                // described on the row, so it can no longer report PASS.
                row["successCleanupClean"] = CloseRapidReloadVisit(successController,
                    row, failures, "visit-ownership.successful-initialization");
                successUnit.Dispose();
            }
            row["cleanupComposition"] = RapidReloadVisitCleanupRules.CombinedFailureMessage;
            RunRapidReloadCleanupReportingInjection(ctx, row, failures);
            return row;
        }

        // R6 fault injection, scoped to this one boundary: a successfully
        // initialised controller is cancelled cleanly through the real
        // CloseRapidReloadVisit, then a second controller has its preview
        // disposed and its public Preview field cleared so the native Cancel()
        // throws. Both calls go through the production cleanup path. Nothing is
        // mocked and no native subsystem is replaced. The injected failure is
        // collected locally, so this check proves the boundary reports rather
        // than failing the run.
        private void RunRapidReloadCleanupReportingInjection(
            RapidReloadGateContext ctx, JObject row, IList<string> failures)
        {
            UnitEntityData cleanUnit = CreateDisposableUnit();
            LevelUpController cleanController = null;
            var cleanFailures = new List<string>();
            var cleanRow = new JObject();
            bool cleanUnitDisposed = false;
            try
            {
                cleanController = OpenRapidReloadVisit(ctx, cleanUnit.Descriptor,
                    ctx.Fighter, null);
                row["injectionCleanCleanupReportedClean"] = CloseRapidReloadVisit(
                    cleanController, cleanRow, cleanFailures, "injection.clean");
                cleanController = null;
            }
            finally
            {
                CloseRapidReloadVisit(cleanController, cleanRow, cleanFailures,
                    "injection.clean-residual");
                cleanUnit.Dispose();
                cleanUnitDisposed = true;
            }
            row["injectionCleanFailureCount"] = cleanFailures.Count;
            row["injectionCleanUnitDisposed"] = cleanUnitDisposed;

            UnitEntityData brokenUnit = CreateDisposableUnit();
            LevelUpController brokenController = null;
            var injectedFailures = new List<string>();
            var injectedRow = new JObject();
            bool injectedUnitDisposed = false;
            bool boundaryThrew = false;
            try
            {
                brokenController = OpenRapidReloadVisit(ctx, brokenUnit.Descriptor,
                    ctx.Fighter, null);
                // Dispose the preview exactly as Cancel() would, then remove it
                // so the native cancellation this boundary performs throws.
                if (brokenController.Preview != null &&
                    brokenController.Preview.Unit != null)
                    brokenController.Preview.Unit.Dispose();
                brokenController.Preview = null;
                try
                {
                    row["injectionFailedCleanupReportedClean"] =
                        CloseRapidReloadVisit(brokenController, injectedRow,
                            injectedFailures, "injection.failed");
                }
                catch (Exception)
                {
                    boundaryThrew = true;
                }
                brokenController = null;
            }
            finally
            {
                brokenUnit.Dispose();
                injectedUnitDisposed = true;
            }
            row["injectionBoundaryThrew"] = boundaryThrew;
            row["injectionFailureCount"] = injectedFailures.Count;
            row["injectionFailureEntry"] = injectedFailures.Count == 0 ? "<none>" :
                injectedFailures[0];
            row["injectionRowCleanupError"] = injectedRow["cleanupError"] == null ?
                "<none>" : (string)injectedRow["cleanupError"];
            row["injectionRowCleanupDetailPresent"] =
                injectedRow["cleanupErrorDetail"] != null;
            row["injectionUnitDisposed"] = injectedUnitDisposed;

            if (cleanFailures.Count != 0)
                failures.Add("visit-ownership:clean-cleanup-reported-a-failure:" +
                    string.Join("|", cleanFailures.ToArray()));
            if (!(bool)row["injectionCleanCleanupReportedClean"])
                failures.Add("visit-ownership:clean-cleanup-not-reported-clean");
            if (!cleanUnitDisposed)
                failures.Add("visit-ownership:clean-fixture-unit-not-disposed");
            if (boundaryThrew)
                failures.Add("visit-ownership:cleanup-boundary-threw-out-of-finally");
            if ((bool)row["injectionFailedCleanupReportedClean"])
                failures.Add("visit-ownership:failed-cleanup-reported-clean");
            if (injectedFailures.Count != 1 ||
                injectedFailures[0].IndexOf(
                    RapidReloadVisitCleanupRules.CleanupFailurePrefix,
                    StringComparison.Ordinal) < 0)
                failures.Add("visit-ownership:failed-cleanup-not-scored:" +
                    injectedFailures.Count);
            if (injectedRow["cleanupError"] == null ||
                injectedRow["cleanupErrorDetail"] == null)
                failures.Add("visit-ownership:failed-cleanup-lost-its-diagnostics");
            if (!injectedUnitDisposed)
                failures.Add("visit-ownership:fixture-unit-not-disposed-after-cleanup-failure");
        }

        // ------------------------------------------------------------------
        // Retained wiring and catalog coverage
        // ------------------------------------------------------------------
        private static JObject DescribeRapidReloadWiring(RapidReloadGateContext ctx,
            IList<string> failures)
        {
            BlueprintComponent[] parentComponents = ctx.Parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            PrerequisiteFirearmProficiency[] parentGate = parentComponents
                .OfType<PrerequisiteFirearmProficiency>().ToArray();
            if (parentGate.Length != ctx.Kinds.Length)
                failures.Add("parent-gate-count=" + parentGate.Length);
            if (!parentGate.Select(value => value.Kind).SequenceEqual(ctx.Kinds))
                failures.Add("parent-gate-kinds");
            if (parentGate.Any(value => value.Group != Prerequisite.GroupType.Any))
                failures.Add("parent-gate-not-or-grouped");
            if (parentGate.Any(value =>
                    !ReferenceEquals(value.FullProficiency, ctx.Full) ||
                    !ReferenceEquals(value.OneHandedProficiency, ctx.OneHanded) ||
                    !ReferenceEquals(value.TwoHandedProficiency, ctx.TwoHanded)))
                failures.Add("parent-gate-proficiency-identity");
            if (parentComponents.OfType<Prerequisite>().Count() != parentGate.Length)
                failures.Add("parent-carries-foreign-prerequisites");
            if (parentComponents.OfType<PrerequisiteClassLevel>().Any() ||
                parentComponents.OfType<PrerequisiteArchetypeLevel>().Any() ||
                parentComponents.OfType<PrerequisiteNoClassLevel>().Any())
                failures.Add("parent-has-class-prerequisite");
            if (parentComponents.OfType<PrerequisiteFeature>().Any(value =>
                    ReferenceEquals(value.Feature, ctx.LegacyWrapper)))
                failures.Add("parent-requires-legacy-wrapper");
            if (ctx.Parent.IgnorePrerequisites)
                failures.Add("parent-ignores-prerequisites");
            var childRows = new JArray();
            for (int index = 0; index < ctx.Children.Length; index++)
            {
                BlueprintComponent[] components = ctx.Children[index].ComponentsArray ??
                    Array.Empty<BlueprintComponent>();
                PrerequisiteFirearmProficiency[] gate = components
                    .OfType<PrerequisiteFirearmProficiency>().ToArray();
                if (gate.Length != 1)
                    failures.Add("child-gate-count:" + ctx.Kinds[index]);
                else
                {
                    if (gate[0].Kind != ctx.Kinds[index])
                        failures.Add("child-gate-kind:" + ctx.Kinds[index]);
                    if (gate[0].Group != Prerequisite.GroupType.All)
                        failures.Add("child-gate-not-and-grouped:" + ctx.Kinds[index]);
                    if (!ReferenceEquals(gate[0].FullProficiency, ctx.Full) ||
                        !ReferenceEquals(gate[0].OneHandedProficiency, ctx.OneHanded) ||
                        !ReferenceEquals(gate[0].TwoHandedProficiency, ctx.TwoHanded))
                        failures.Add("child-gate-proficiency-identity:" + ctx.Kinds[index]);
                }
                if (components.OfType<PrerequisiteClassLevel>().Any() ||
                    components.OfType<PrerequisiteArchetypeLevel>().Any())
                    failures.Add("child-has-class-prerequisite:" + ctx.Kinds[index]);
                childRows.Add(new JObject {
                    ["kind"] = ctx.Kinds[index].ToString(),
                    ["guid"] = ctx.Children[index].AssetGuid,
                    ["prerequisiteCount"] = components.OfType<Prerequisite>().Count(),
                    ["firearmPrerequisiteCount"] = gate.Length,
                    ["group"] = gate.Length == 1 ? gate[0].Group.ToString() : "<none>",
                    ["uiText"] = gate.Length == 1 ? gate[0].GetUIText() : "<none>" });
            }
            return new JObject {
                ["parentGuid"] = ctx.Parent.AssetGuid,
                ["parentName"] = ctx.Parent.name,
                ["parentIgnorePrerequisites"] = ctx.Parent.IgnorePrerequisites,
                ["parentGate"] = new JArray(parentGate.Select(value => new JObject {
                    ["componentName"] = value.name,
                    ["kind"] = value.Kind.ToString(),
                    ["group"] = value.Group.ToString(),
                    ["uiText"] = value.GetUIText() })),
                ["registeredChildCount"] = ctx.RegisteredChildren.Length,
                ["children"] = childRows };
        }

        private static JObject DescribeRapidReloadCatalogs(RapidReloadGateContext ctx,
            IList<string> failures)
        {
            foreach (var entry in new[] {
                new KeyValuePair<string, BlueprintFeatureSelection>("basic", ctx.Basic),
                new KeyValuePair<string, BlueprintFeatureSelection>("fighter",
                    ctx.FighterFeats) })
            {
                int features = CountRapidReloadReference(entry.Value.Features, ctx.Parent);
                int allFeatures = CountRapidReloadReference(entry.Value.AllFeatures,
                    ctx.Parent);
                if (features != 1 || allFeatures != 1)
                    failures.Add("parent-publication:" + entry.Key + "=" + features +
                        "/" + allFeatures);
            }
            BlueprintFeature[] compatibilityOnly = ctx.RegisteredChildren
                .Skip(ctx.Kinds.Length).ToArray();
            BlueprintFeatureSelection[] allSelections = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintFeatureSelection>().ToArray();
            foreach (BlueprintFeature retired in compatibilityOnly)
            {
                if (CountRapidReloadReference(ctx.Parent.Features, retired) != 0 ||
                    CountRapidReloadReference(ctx.Parent.AllFeatures, retired) != 0)
                    failures.Add("retired-child-in-parent:" + retired.name);
                if (allSelections.Any(selection =>
                        CountRapidReloadReference(selection.Features, retired) != 0 ||
                        CountRapidReloadReference(selection.AllFeatures, retired) != 0))
                    failures.Add("retired-child-published:" + retired.name);
            }
            if (allSelections.Any(selection =>
                    CountRapidReloadReference(selection.Features, ctx.LegacyWrapper) != 0 ||
                    CountRapidReloadReference(selection.AllFeatures, ctx.LegacyWrapper) != 0))
                failures.Add("legacy-wrapper-published");
            if (ctx.Parent.Features.Length != ctx.Kinds.Length ||
                ctx.Parent.AllFeatures.Length != ctx.Kinds.Length)
                failures.Add("parent-choice-count=" + ctx.Parent.Features.Length + "/" +
                    ctx.Parent.AllFeatures.Length);
            return new JObject {
                ["basicFeatures"] = CountRapidReloadReference(ctx.Basic.Features, ctx.Parent),
                ["basicAllFeatures"] = CountRapidReloadReference(ctx.Basic.AllFeatures,
                    ctx.Parent),
                ["fighterFeatures"] = CountRapidReloadReference(ctx.FighterFeats.Features,
                    ctx.Parent),
                ["fighterAllFeatures"] = CountRapidReloadReference(
                    ctx.FighterFeats.AllFeatures, ctx.Parent),
                ["parentChoices"] = new JArray(ctx.Parent.AllFeatures.Select(value =>
                    value == null ? "<null>" : value.name)),
                ["compatibilityOnlyChoices"] = new JArray(compatibilityOnly.Select(
                    value => value.name)),
                ["selectionsScanned"] = allSelections.Length };
        }

        private static int CountRapidReloadReference(BlueprintFeature[] source,
            BlueprintFeature target)
        {
            return (source ?? Array.Empty<BlueprintFeature>()).Count(value =>
                ReferenceEquals(value, target) || value != null && target != null &&
                string.Equals(value.AssetGuid, target.AssetGuid,
                    StringComparison.Ordinal));
        }

        private static void AddRapidReloadMatrixRow(RapidReloadGateContext ctx,
            JArray matrix, IList<string> failures, string label, bool expectedFull,
            bool expectedOneHanded, bool expectedTwoHanded,
            Action<UnitDescriptor> prepare)
        {
            UnitEntityData unit = CreateDisposableUnit();
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                if (prepare != null) prepare(descriptor);
                JObject fixture = DescribeRapidReloadFixture(ctx, descriptor);
                // The fixture's real ranks are proved before its answers count.
                RapidReloadGateEvidenceRules.EvaluateFixtureProficiency(fixture, label,
                    expectedFull, expectedOneHanded, expectedTwoHanded, failures);
                bool parentObserved = ctx.Parent.MeetsPrerequisites(null, descriptor, null);
                bool parentExpected = RapidReloadPrerequisiteRules.ParentQualifies(
                    expectedFull, expectedOneHanded, expectedTwoHanded);
                if (parentObserved != parentExpected)
                    failures.Add(label + ":parent=" + parentObserved);
                var childRows = new JArray();
                for (int index = 0; index < ctx.Children.Length; index++)
                {
                    bool observed = ctx.Children[index].MeetsPrerequisites(null,
                        descriptor, null);
                    bool expected = RapidReloadPrerequisiteRules.ChildQualifies(
                        ctx.Kinds[index], expectedFull, expectedOneHanded, expectedTwoHanded);
                    if (observed != expected)
                        failures.Add(label + ":" + ctx.Kinds[index] + "=" + observed);
                    childRows.Add(new JObject {
                        ["kind"] = ctx.Kinds[index].ToString(),
                        ["observed"] = observed, ["expected"] = expected });
                }
                matrix.Add(new JObject {
                    ["case"] = label,
                    ["fixture"] = fixture,
                    ["parentObserved"] = parentObserved,
                    ["parentExpected"] = parentExpected,
                    ["children"] = childRows });
            }
            finally { unit.Dispose(); }
        }
    }
}
