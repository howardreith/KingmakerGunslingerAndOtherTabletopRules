using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Compatibility;
using KingmakerGunslinger.EasternWeapons;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free live-rule qualification for the Eastern Weapons catalog. The
    /// fixture uses real spawned units, equipped ItemEntityWeapon instances,
    /// native attacks/stat rules, native activatable state, and exact cleanup.
    /// </summary>
    internal static class EasternWeaponCombatScenario
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const string WeaponFinesseGuid =
            "90e54424d682d104ab36436bd527af09";
        private const string ShortswordItemGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";
        private const string WeaponTrainingLightBladesGuid =
            "4923409590bdb604590e04da4253ab78";
        private const string WeaponTrainingHeavyBladesGuid =
            "2a0ce0186af38ed419f47fce16f93c2a";
        private const string WeaponTrainingPolearmsGuid =
            "c062c6d16aecddc4ab67d9c783b2ad46";
        private const string FencingGraceGuid =
            "47b352ea0f73c354aba777945760b441";
        private const string SlashingGraceGuid =
            "697d64669eb2c0543abb9c9b07998a38";
        private const string HasteBuffGuid =
            "03464790f40c3c24aa684b57155f3280";
        private const string UndeadTypeGuid =
            "734a29b693e9ec346ba2951b27987e33";

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            EasternWeaponBlueprintSet set = BlueprintBootstrap.EasternWeapons;
            if (set == null || set.Named == null)
                throw new InvalidOperationException(
                    "The Eastern Weapons blueprint catalog is unavailable.");

            object allUnits = ElvenBranchedSpearCombatScenario.Read(
                Game.Instance.State, "AllUnits");
            object[] allUnitsBefore = ElvenBranchedSpearCombatScenario.Snapshot(
                allUnits);
            SceneEntitiesState scene = null;
            UnitEntityData attacker = null;
            UnitEntityData target = null;
            BlueprintUnit hostileSource = null;
            ItemEntityWeapon equipped = null;
            ItemEntityWeapon offhand = null;
            var facts = new List<BlueprintUnitFact>();
            ActivatableAbility powerAttack = null;
            bool cleaned = false;
            string stage = "create-live-fixture";
            try
            {
                scene = new SceneEntitiesState(
                    "KMG_Eastern_Weapons_Combat_Fixture");
                BlueprintUnit source = BlueprintRoot.Instance
                    .DefaultPlayerCharacter;
                attacker = Game.Instance.EntityCreator.SpawnUnit(source,
                    Vector3.zero, Quaternion.identity, scene);
                target = ElvenBranchedSpearCombatScenario.SpawnHostileTarget(
                    attacker, source, new Vector3(1.5f, 0f, 0f), scene,
                    out hostileSource);
                if (attacker == null || target == null || attacker.View == null ||
                    target.View == null)
                    throw new InvalidOperationException(
                        "Native entity creation did not produce live unit views.");
                target.Descriptor.State.Immortality.Retain();
                attacker.Descriptor.Stats.Strength.BaseValue = 10;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 20;
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 12;

                stage = "catalog-and-presentation";
                QualifyCatalog(set, assertions);
                QualifyAllItemVisuals(set, assertions, diagnostics);
                QualifySelectors(set, assertions, diagnostics);

                stage = "proficiency";
                BlueprintFeature martial = ElvenBranchedSpearCombatScenario
                    .FindMartialProficiency();
                QualifyProficiency(set, attacker, target, martial, facts,
                    ref equipped, ref offhand, assertions, diagnostics);

                stage = "fighter-groups";
                QualifyFighterGroups(set, attacker, facts, ref equipped,
                    assertions, diagnostics);

                stage = "finesse";
                QualifyFinesse(set, attacker, facts, ref equipped, assertions,
                    diagnostics);

                stage = "named-native-properties";
                QualifyNamedProperties(set, assertions, diagnostics);

                stage = "named-effects";
                QualifyNamedEffects(set, attacker, target, facts,
                    ref equipped, ref offhand, ref powerAttack, assertions,
                    diagnostics);

                stage = "capstones";
                QualifyCapstones(set, attacker, target, ref equipped,
                    ref offhand, assertions, diagnostics);

                stage = "call-of-the-wild-focused-weapon";
                QualifyFocusedWeapon(set, attacker, facts, ref equipped,
                    assertions, diagnostics);
            }
            catch (Exception exception)
            {
                ElvenBranchedSpearCombatScenario.Add(assertions,
                    "eastern-combat-scenario-exception", "no exception",
                    "stage=" + stage + ";" + exception, false,
                    "exception-contained request-local fixture");
            }
            finally
            {
                if (powerAttack != null && powerAttack.IsOn)
                    powerAttack.IsOn = false;
                if (attacker != null)
                {
                    attacker.Commands.InterruptAll(true);
                    SetPolymorphed(attacker, false);
                    attacker.Descriptor.State.Size =
                        attacker.Descriptor.OriginalSize;
                    RemoveOffhand(attacker, ref offhand);
                    ElvenBranchedSpearCombatScenario.RemoveEquipped(attacker,
                        ref equipped);
                    foreach (BlueprintUnitFact fact in facts.ToArray())
                        if (fact != null && attacker.Descriptor.HasFact(fact))
                            attacker.Descriptor.RemoveFact(fact);
                    foreach (Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
                        buff in set.Named.Buffs.All)
                    {
                        Buff current = attacker.Descriptor.Buffs.GetBuff(buff);
                        if (current != null)
                            attacker.Descriptor.Buffs.RemoveFact(current);
                    }
                }
                if (target != null)
                    target.Descriptor.State.Immortality.ReleaseAll();
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                if (scene != null) scene.Dispose();
                if (hostileSource != null)
                    UnityEngine.Object.DestroyImmediate(hostileSource);
                cleaned = ElvenBranchedSpearCombatScenario.SameReferences(
                    allUnitsBefore,
                    ElvenBranchedSpearCombatScenario.Snapshot(allUnits));
            }

            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-combat-fixture-cleanup",
                "global-unit snapshot restored and request-local objects disposed",
                "cleaned=" + cleaned, cleaned,
                "disposable SceneEntitiesState, units, items, facts, buffs");
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version,
                string.Equals(request.ExpectedModVersion,
                    context.ModEntry.Info.Version, StringComparison.Ordinal),
                "Unity Mod Manager ModEntry.Info.Version");

            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = identity.RuntimeIdentity + "; mvid=" +
                    identity.ModuleVersionId + "; sha256=" +
                    identity.LoadedModuleSha256 + "; pid=" + identity.ProcessId,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = DateTime.UtcNow.ToString("o"),
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void QualifyCatalog(EasternWeaponBlueprintSet set,
            ICollection<RuntimeTestAssertion> assertions)
        {
            bool exactRelations = set.Families.Length == 3 &&
                set.Entries.Length == 12 && set.Named.Entries.Length == 18 &&
                set.Families.All(family => family.Entries.Length == 4 &&
                    family.Entries.All(entry => ReferenceEquals(
                        entry.Item.Type, family.WeaponType))) &&
                set.Named.Entries.All(entry => ReferenceEquals(entry.Item.Type,
                    set.Require(entry.Spec.Family).WeaponType));
            string observed = "families=" + set.Families.Length +
                ";generic=" + set.Entries.Length + ";named=" +
                set.Named.Entries.Length;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-catalog-identity",
                "3 stable family types, 12 generic items, and 18 named items",
                observed, exactRelations,
                "live registered BlueprintItemWeapon/BlueprintWeaponType references");

            string titles = set.WakizashiProficiency.Name + "|" +
                set.KatanaProficiency.Name;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-proficiency-presentation",
                "Weapon Proficiency (Wakizashi) and Weapon Proficiency (Katana)",
                titles,
                string.Equals(set.WakizashiProficiency.Name,
                    "Weapon Proficiency (Wakizashi)", StringComparison.Ordinal) &&
                string.Equals(set.KatanaProficiency.Name,
                    "Weapon Proficiency (Katana)", StringComparison.Ordinal),
                "live localized static proficiency children");
        }

        private static void QualifySelectors(EasternWeaponBlueprintSet set,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            EasternWeaponFamily[] families = {
                EasternWeaponFamily.Wakizashi,
                EasternWeaponFamily.Katana,
                EasternWeaponFamily.Nodachi };
            string[] names = { "Wakizashi", "Katana", "Nodachi" };
            string[] acronyms = { "WK", "KA", "NO" };
            var rows = new List<string>();
            bool parameterExact = true;
            foreach (string guid in EasternWeaponBlueprints
                .ParameterSelectorGuids)
            {
                BlueprintParametrizedFeature selector = BlueprintLibraryLookup
                    .RequireExact<BlueprintParametrizedFeature>(
                        BlueprintBootstrap.Library, guid,
                        "native chosen-weapon selector");
                FeatureUIData[] selection = selector.GetFullSelectionItems()
                    .ToArray();
                for (int index = 0; index < families.Length; index++)
                {
                    WeaponCategory category = EasternWeaponCategoryRuntime
                        .Category(families[index]);
                    FeatureUIData[] matches = selection.Where(value =>
                        value != null && value.Param != null &&
                        value.Param.WeaponCategory.HasValue &&
                        value.Param.WeaponCategory.Value.Equals(category))
                        .ToArray();
                    parameterExact &= matches.Length == 1 &&
                        string.Equals(matches[0].Name, names[index],
                            StringComparison.Ordinal) &&
                        string.Equals(matches[0].NameForAcronim,
                            acronyms[index], StringComparison.Ordinal) &&
                        matches[0].Icon == null;
                    rows.Add(selector.name + ":" + names[index] + "=" +
                        matches.Length + "/" + (matches.Length == 1
                            ? matches[0].NameForAcronim : "<missing>"));
                }
            }

            BlueprintFeatureSelection ewp = BlueprintLibraryLookup
                .RequireExact<BlueprintFeatureSelection>(
                    BlueprintBootstrap.Library,
                    EasternWeaponBlueprints
                        .NativeExoticWeaponProficiencySelectionGuid,
                    "native Exotic Weapon Proficiency selection");
            BlueprintFeature curve = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library,
                    EasternWeaponBlueprints
                        .NativeElvenCurveBladeProficiencyGuid,
                    "native Elven Curve Blade proficiency ordering anchor");
            BlueprintFeature spear = BlueprintBootstrap.ElvenBranchedSpears
                .ExoticWeaponProficiency;
            int curveIndex = Array.IndexOf(ewp.AllFeatures, curve);
            int spearIndex = Array.IndexOf(ewp.AllFeatures, spear);
            int katanaIndex = Array.IndexOf(ewp.AllFeatures,
                set.KatanaProficiency);
            int wakizashiIndex = Array.IndexOf(ewp.AllFeatures,
                set.WakizashiProficiency);
            bool ewpExact = CountFeature(ewp, set.KatanaProficiency) == 1 &&
                CountFeature(ewp, set.WakizashiProficiency) == 1 &&
                Array.IndexOf(ewp.Features, set.KatanaProficiency) == -1 &&
                Array.IndexOf(ewp.Features, set.WakizashiProficiency) == -1 &&
                spearIndex == curveIndex + 1 &&
                katanaIndex == spearIndex + 1 &&
                wakizashiIndex == katanaIndex + 1;

            BlueprintFeatureSelection finesse = BlueprintLibraryLookup
                .RequireExact<BlueprintFeatureSelection>(
                    BlueprintBootstrap.Library,
                    EasternWeaponBlueprints.NativeFinesseTrainingSelectionGuid,
                    "native Rogue Finesse Training selection");
            bool finesseExact = CountFeature(finesse,
                set.WakizashiFinesseTraining) == 1 &&
                !finesse.AllFeatures.Any(value => ReferenceEquals(value,
                    set.KatanaProficiency) || ReferenceEquals(value,
                    set.WakizashiProficiency));
            int graceLeaks = CountCategory(FencingGraceGuid,
                    EasternWeaponFamily.Wakizashi) +
                CountCategory(FencingGraceGuid, EasternWeaponFamily.Katana) +
                CountCategory(FencingGraceGuid, EasternWeaponFamily.Nodachi) +
                CountCategory(SlashingGraceGuid,
                    EasternWeaponFamily.Wakizashi) +
                CountCategory(SlashingGraceGuid, EasternWeaponFamily.Katana) +
                CountCategory(SlashingGraceGuid, EasternWeaponFamily.Nodachi);
            string observed = "rows=" + string.Join("|", rows.ToArray()) +
                ";order=" + curveIndex + "/" + spearIndex + "/" +
                katanaIndex + "/" + wakizashiIndex + ";finesse=" +
                CountFeature(finesse, set.WakizashiFinesseTraining) +
                ";graceLeaks=" + graceLeaks;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-selector-publication",
                "seven selectors contain one WK/KA/NO native-glyph row; merged proficiencies follow Curve/Spear/Katana/Wakizashi; only Wakizashi enters Finesse Training; Grace has no eastern rows",
                observed, parameterExact && ewpExact && finesseExact &&
                    graceLeaks == 0,
                "live GetFullSelectionItems, merged AllFeatures, and excluded native Grace selectors");
            diagnostics.Add("selectors{" + observed + "}");
        }

        internal static void QualifyAllItemVisuals(
            EasternWeaponBlueprintSet set,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            KeyValuePair<string, BlueprintItemWeapon>[] mappedItems = set.Entries
                .Select(value => new KeyValuePair<string, BlueprintItemWeapon>(
                    value.Spec.Symbol, value.Item)).Concat(set.Named.Entries
                .Select(value => new KeyValuePair<string, BlueprintItemWeapon>(
                    value.Spec.Symbol, value.Item))).ToArray();
            BlueprintItemWeapon[] items = mappedItems.Select(value =>
                value.Value).ToArray();
            var rows = new List<string>();
            bool exact = items.Length == 30;
            foreach (KeyValuePair<string, BlueprintItemWeapon> mapped in
                mappedItems)
            {
                string symbol = mapped.Key;
                BlueprintItemWeapon item = mapped.Value;
                EasternWeaponFamilyBlueprintSet familySet = set.Families
                    .Single(value => ReferenceEquals(value.WeaponType,
                        item.Type));
                EasternWeaponFamily family = familySet.Family;
                string donorGuid = VisualDonorGuid(family);
                BlueprintWeaponType donor = BlueprintLibraryLookup
                    .RequireExact<BlueprintWeaponType>(
                        BlueprintBootstrap.Library, donorGuid,
                        family + " visual/animation donor");
                WeaponVisualParameters visual = item.VisualParameters;
                bool itemOverrideFieldExists;
                object itemOverride = TryReadFieldRecursive(item,
                    "m_VisualParameters", out itemOverrideFieldExists);
                GameObject model = visual == null ? null : visual.Model;
                GameObject beltModel = visual == null ? null :
                    visual.BeltModel;
                GameObject instance = null;
                GameObject storedInstance = null;
                bool heldResolved = false;
                bool storedResolved = false;
                bool heldCleaned = false;
                bool storedCleaned = false;
                bool independentTransforms = false;
                string instantiated = "<null>";
                string storedInstantiated = "<null>";
                string materialSummary = "<none>";
                string storedMaterialSummary = "<none>";
                bool cuttingEdge = false;
                bool storedCuttingEdge = false;
                bool calibratedFrames = false;
                try
                {
                    string variant = WeaponVisualVariantCatalog.Require(symbol);
                    instance = Assets.EasternWeaponAssetRuntime
                        .InstantiatePrefab(variant);
                    storedInstance = Assets.EasternWeaponAssetRuntime
                        .InstantiateStoredPrefab(variant);
                    if (instance != null && storedInstance != null)
                    {
                        heldResolved = true;
                        storedResolved = true;
                        instantiated = instance.name;
                        storedInstantiated = storedInstance.name;
                        Material[] materials = Materials(instance);
                        Material[] storedMaterials = Materials(storedInstance);
                        materialSummary = string.Join(",", materials.Select(
                            value => value.name).Distinct().ToArray());
                        storedMaterialSummary = string.Join(",",
                            storedMaterials.Select(value => value.name)
                                .Distinct().ToArray());
                        cuttingEdge = materials.Any(value => value.name
                            .IndexOf("CuttingEdge",
                                StringComparison.OrdinalIgnoreCase) >= 0);
                        storedCuttingEdge = storedMaterials.Any(value =>
                            value.name.IndexOf("CuttingEdge",
                                StringComparison.OrdinalIgnoreCase) >= 0);
                        Transform heldVisual = instance.transform.Find("Visual");
                        Transform storedVisual = storedInstance.transform.Find(
                            "Visual");
                        independentTransforms = heldVisual != null &&
                            storedVisual != null &&
                            (!Approximately(heldVisual.localPosition,
                                storedVisual.localPosition) ||
                             !Approximately(heldVisual.localRotation,
                                storedVisual.localRotation));
                        calibratedFrames = Assets.EasternWeaponAssetRuntime
                            .HasCalibratedDonorFrame(instance, family, false) &&
                            Assets.EasternWeaponAssetRuntime
                            .HasCalibratedDonorFrame(storedInstance, family,
                                true);
                    }
                }
                finally
                {
                    if (instance != null)
                        UnityEngine.Object.DestroyImmediate(instance);
                    if (storedInstance != null)
                        UnityEngine.Object.DestroyImmediate(storedInstance);
                    heldCleaned = instance == null || instance.Equals(null);
                    storedCleaned = storedInstance == null ||
                        storedInstance.Equals(null);
                }
                string[] overlays = item.Enchantments.Select(value =>
                    value == null ? "<null>" : value.AssetGuid).ToArray();
                bool itemExact = itemOverrideFieldExists &&
                    ReferenceEquals(itemOverride, visual) && visual != null &&
                    !ReferenceEquals(visual, familySet.WeaponType
                        .VisualParameters) && model != null &&
                    beltModel != null &&
                    Assets.EasternWeaponAssetRuntime.HasExactVisual(item,
                        symbol) &&
                    VisualContractMatches(visual, donor.VisualParameters) &&
                    heldResolved && storedResolved && cuttingEdge &&
                    storedCuttingEdge && independentTransforms &&
                    calibratedFrames && heldCleaned && storedCleaned;
                exact &= itemExact;
                rows.Add(item.AssetGuid + ":" + item.Name +
                    ";symbol=" + symbol + ";variant=" +
                    WeaponVisualVariantCatalog.Require(symbol) +
                    ";family=" + family + ";type=" +
                    item.Type.AssetGuid + ";itemOverride=" +
                    (!itemOverrideFieldExists ? "field-absent" :
                        ReferenceEquals(itemOverride, visual) ?
                            "exact-item-visual" : "different-visual") +
                    ";model=" + (model == null ? "<null>" : model.name) +
                    ";beltModel=" + (beltModel == null ? "<null>" :
                        beltModel.name) + ";instantiated=" + instantiated +
                    ";storedInstantiated=" + storedInstantiated +
                    ";donor=" +
                    donorGuid + "/" + (visual == null ? "<null>" :
                        visual.AnimStyle.ToString()) + ";materials=" +
                    materialSummary + ";storedMaterials=" +
                    storedMaterialSummary + ";independent=" +
                    independentTransforms + ";calibratedFrames=" +
                    calibratedFrames + ";overlays=" +
                    string.Join(",", overlays));
            }
            string observed = string.Join("|", rows.ToArray());
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-all-30-visual-identities",
                "30 exact items; 10 per family; inherited held and independently calibrated stored fields equal the approved blueprint-specific pair; exact family and native donor contract including sheath; CuttingEdge material in both roles; transient cleanup",
                observed, exact && set.Families.All(family => items.Count(
                    item => ReferenceEquals(item.Type,
                        family.WeaponType)) == 10),
                "live recursive inherited item visual resolution plus one exact held/stored AssetBundle prefab-pair instantiation per item");
            diagnostics.Add("all30Visuals{" + observed + "}");
        }

        private static string VisualDonorGuid(EasternWeaponFamily family)
        {
            if (family == EasternWeaponFamily.Wakizashi)
                return EasternWeaponBlueprints.WakizashiVisualDonorGuid;
            if (family == EasternWeaponFamily.Katana)
                return EasternWeaponBlueprints.KatanaVisualDonorGuid;
            return EasternWeaponBlueprints.NodachiVisualDonorGuid;
        }

        private static bool VisualContractMatches(WeaponVisualParameters value,
            WeaponVisualParameters donor)
        {
            if (value == null || donor == null) return false;
            foreach (FieldInfo field in typeof(WeaponVisualParameters)
                .GetFields(Members))
            {
                if (field.IsStatic || string.Equals(field.Name,
                        "m_WeaponModel", StringComparison.Ordinal) ||
                    string.Equals(field.Name, "m_WeaponBeltModel",
                        StringComparison.Ordinal)) continue;
                object left = field.GetValue(value);
                object right = field.GetValue(donor);
                if (!ReferenceEquals(left, right) && !Equals(left, right))
                    return false;
            }
            return true;
        }

        private static Material[] Materials(GameObject instance)
        {
            return instance.GetComponentsInChildren<Renderer>(true)
                .SelectMany(value => value.sharedMaterials ??
                    Array.Empty<Material>()).Where(value => value != null)
                .ToArray();
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return (left - right).sqrMagnitude <= 0.000001f;
        }

        private static bool Approximately(Quaternion left, Quaternion right)
        {
            return Mathf.Abs(Quaternion.Dot(left, right)) >= 0.999999f;
        }

        private static object TryReadFieldRecursive(object owner, string name,
            out bool fieldExists)
        {
            for (Type type = owner == null ? null : owner.GetType();
                type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Members |
                    BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    fieldExists = true;
                    return field.GetValue(owner);
                }
            }
            fieldExists = false;
            return null;
        }

        private static void QualifyFocusedWeapon(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, IList<BlueprintUnitFact> facts,
            ref ItemEntityWeapon equipped,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            BlueprintScriptableObject optional = null;
            bool present = BlueprintBootstrap.Library.BlueprintsByAssetId != null &&
                BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                    CustomWeaponFocusedWeaponPublication.SelectionGuid,
                    out optional);
            BlueprintFeature[] children = {
                RequireFocused(CustomWeaponFocusedWeaponPublication.SpearGuid),
                RequireFocused(CustomWeaponFocusedWeaponPublication.WakizashiGuid),
                RequireFocused(CustomWeaponFocusedWeaponPublication.KatanaGuid),
                RequireFocused(CustomWeaponFocusedWeaponPublication.NodachiGuid) };
            if (!present)
            {
                bool inert = children.All(value =>
                    (value.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                        .Length == 0);
                ElvenBranchedSpearCombatScenario.Add(assertions,
                    "cotw-focused-weapon-absent",
                    "optional selector absent; four persistent choices inert",
                    "selector=absent;inert=" + inert, inert,
                    "exact optional GUID lookup and persistent KMG blueprints");
                diagnostics.Add("focusedWeapon{absent;no-selector-lookup}");
                return;
            }

            var selection = optional as BlueprintFeatureSelection;
            if (selection == null || !string.Equals(selection.name,
                    "FocusedWeaponAdvancedWeaponTrainingFeatureSelection",
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The exact Call of the Wild Focused Weapon selection changed.");

            BlueprintParametrizedFeature weaponFocus = BlueprintLibraryLookup
                .RequireExact<BlueprintParametrizedFeature>(
                    BlueprintBootstrap.Library,
                    CustomWeaponFocusedWeaponPublication.WeaponFocusGuid,
                    "native Weapon Focus parameter authority");
            BlueprintCharacterClass fighter = BlueprintRoot.Instance.Progression
                .CharacterClasses.Single(value => value != null &&
                    string.Equals(value.AssetGuid,
                        "48ac8db94d5de7645906c7d0ad3bcfbd",
                        StringComparison.Ordinal));
            for (int level = attacker.Descriptor.Progression.GetClassLevel(fighter);
                level < 20; level++)
                attacker.Descriptor.Progression.AddClassLevel(fighter);

            foreach (BlueprintFeature prerequisite in children.SelectMany(value =>
                (value.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<Kingmaker.Blueprints.Classes.Prerequisites
                        .PrerequisiteFeature>()
                    .Select(component => component.Feature))
                .Where(value => value != null).Distinct())
            {
                if (!attacker.Descriptor.HasFact(prerequisite))
                    ElvenBranchedSpearCombatScenario.AddFact(attacker,
                        prerequisite, facts);
            }

            WeaponCategory[] categories = {
                ElvenBranchedSpear.ElvenBranchedSpearCategoryRuntime.Category,
                EasternWeaponCategoryRuntime.Category(
                    EasternWeaponFamily.Wakizashi),
                EasternWeaponCategoryRuntime.Category(
                    EasternWeaponFamily.Katana),
                EasternWeaponCategoryRuntime.Category(
                    EasternWeaponFamily.Nodachi) };
            BlueprintItemWeapon[] weapons = {
                BlueprintBootstrap.ElvenBranchedSpears.Require(
                    ElvenBranchedSpear.ElvenBranchedSpearItemKind.Mundane).Item,
                set.Require(EasternWeaponFamily.Wakizashi,
                    EasternWeaponGenericKind.Mundane).Item,
                set.Require(EasternWeaponFamily.Katana,
                    EasternWeaponGenericKind.Mundane).Item,
                set.Require(EasternWeaponFamily.Nodachi,
                    EasternWeaponGenericKind.Mundane).Item };
            string[] names = {
                "Elven Branched Spear", "Wakizashi", "Katana", "Nodachi" };
            var parameterFacts = new List<Fact>();
            try
            {
                bool negative = CountFocused(selection, attacker, children) == 0;
                bool singles = true;
                var rows = new List<string>();
                for (int index = 0; index < categories.Length; index++)
                {
                    Fact focus = attacker.Descriptor.AddFact(weaponFocus, null,
                        new FeatureParam(categories[index]));
                    if (focus == null)
                        throw new InvalidOperationException(
                            "Could not add request-local Weapon Focus (" +
                            names[index] + ").");
                    try
                    {
                        int visible = CountFocused(selection, attacker, children);
                        bool exact = visible == 1 && IsFocusedVisible(selection,
                            attacker, children[index]);
                        singles &= exact;
                        rows.Add(names[index] + "=" + visible + "/" + exact);
                    }
                    finally
                    {
                        attacker.Descriptor.RemoveFact(focus);
                    }
                }

                for (int index = 0; index < categories.Length; index++)
                {
                    Fact focus = attacker.Descriptor.AddFact(weaponFocus, null,
                        new FeatureParam(categories[index]));
                    if (focus == null)
                        throw new InvalidOperationException(
                            "Could not add combined request-local Weapon Focus.");
                    parameterFacts.Add(focus);
                }
                bool combined = CountFocused(selection, attacker, children) == 4 &&
                    children.All(value => IsFocusedVisible(selection, attacker,
                        value));

                bool mechanics = true;
                var mechanicalRows = new List<string>();
                for (int index = 0; index < children.Length; index++)
                {
                    Fact focused = attacker.Descriptor.AddFact(children[index]);
                    if (focused == null)
                        throw new InvalidOperationException(
                            "Could not add request-local Focused Weapon: " +
                            names[index] + ".");
                    try
                    {
                        Swap(attacker, weapons[index], ref equipped);
                        RuleCalculateWeaponStats stats =
                            ElvenBranchedSpearCombatScenario.WeaponStats(attacker,
                                equipped);
                        BlueprintComponent damage = children[index]
                            .ComponentsArray.Single(value => value != null &&
                                string.Equals(value.GetType().FullName,
                                    CustomWeaponFocusedWeaponPublication
                                        .DamageComponentTypeName,
                                    StringComparison.Ordinal));
                        FieldInfo diceField = damage.GetType().GetField(
                            "dice_formulas", Members);
                        var dice = diceField == null ? null :
                            diceField.GetValue(damage) as DiceFormula[];
                        DiceFormula expected = dice == null || dice.Length != 5 ?
                            default(DiceFormula) : dice[4];
                        bool exact = dice != null && dice.Length == 5 &&
                            stats.WeaponDamageDiceOverride.HasValue &&
                            stats.WeaponDamageDiceOverride.Value.Equals(expected);
                        mechanics &= exact;
                        mechanicalRows.Add(names[index] + "=" +
                            (stats.WeaponDamageDiceOverride.HasValue ?
                                stats.WeaponDamageDiceOverride.Value.ToString() :
                                "<none>") + "/expected=" + expected);
                    }
                    finally
                    {
                        attacker.Descriptor.RemoveFact(focused);
                    }
                }

                string observed = "negative=" + negative + ";singles=" +
                    string.Join("|", rows.ToArray()) + ";combined=" + combined +
                    ";mechanics=" + string.Join("|",
                        mechanicalRows.ToArray());
                ElvenBranchedSpearCombatScenario.Add(assertions,
                    "cotw-focused-weapon-eligibility",
                    "no focus=0; each exact Weapon Focus=that one row; all four=4",
                    observed, negative && singles && combined,
                    "native ExtractSelectionItems with before/preview unit and exact Weapon Focus FeatureParam facts");
                ElvenBranchedSpearCombatScenario.Add(assertions,
                    "cotw-focused-weapon-mechanics",
                    "all four exact categories use CotW's highest Focused Weapon damage die once",
                    observed, mechanics,
                    "real feature facts and RuleCalculateWeaponStats event delivery");
                diagnostics.Add("focusedWeapon{" + observed + "}");
            }
            finally
            {
                foreach (Fact fact in parameterFacts.ToArray())
                    if (fact != null) attacker.Descriptor.RemoveFact(fact);
            }
        }

        private static BlueprintFeature RequireFocused(string guid)
        {
            return BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                BlueprintBootstrap.Library, guid,
                "persistent KMG Focused Weapon choice");
        }

        private static int CountFocused(BlueprintFeatureSelection selection,
            UnitEntityData unit, IEnumerable<BlueprintFeature> owned)
        {
            BlueprintFeature[] exact = owned.ToArray();
            return selection.ExtractSelectionItems(unit.Descriptor,
                    unit.Descriptor).Count(value => value != null &&
                        exact.Any(feature => ReferenceEquals(value.Feature,
                            feature) || value.Feature != null && string.Equals(
                                value.Feature.AssetGuid, feature.AssetGuid,
                                StringComparison.Ordinal)) &&
                        value.Feature.MeetsPrerequisites(null,
                            unit.Descriptor, null));
        }

        private static bool IsFocusedVisible(
            BlueprintFeatureSelection selection, UnitEntityData unit,
            BlueprintFeature expected)
        {
            return selection.ExtractSelectionItems(unit.Descriptor,
                unit.Descriptor).Count(value => value != null &&
                    (ReferenceEquals(value.Feature, expected) ||
                    value.Feature != null && string.Equals(
                        value.Feature.AssetGuid, expected.AssetGuid,
                        StringComparison.Ordinal)) &&
                    value.Feature.MeetsPrerequisites(null,
                        unit.Descriptor, null)) == 1;
        }

        private static void QualifyProficiency(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, UnitEntityData target,
            BlueprintFeature martial, IList<BlueprintUnitFact> facts,
            ref ItemEntityWeapon equipped, ref ItemEntityWeapon offhand,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            BlueprintItemWeapon wakizashi = set.Require(
                EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item;
            equipped = ElvenBranchedSpearCombatScenario.Equip(attacker,
                wakizashi);
            int wakUntrained = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, martial, facts);
            int wakMartial = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, martial, facts);
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                set.WakizashiProficiency, facts);
            int wakExact = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker,
                set.WakizashiProficiency, facts);
            string wakObserved = wakUntrained + "/" + wakMartial + "/" +
                wakExact;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-wakizashi-proficiency",
                "blanket martial leaves -4; exact Wakizashi proficiency removes it",
                wakObserved, wakMartial == wakUntrained &&
                    wakExact == wakUntrained + 4,
                "live RuleAttackWithWeapon and exact AddProficiencies fact");

            Swap(attacker, set.Require(EasternWeaponFamily.Katana,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int katanaTwoUntrained = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, martial, facts);
            int katanaTwoMartial = Attack(attacker, target, equipped);
            bool nativeTwoHands = equipped.HoldInTwoHands;
            offhand = EquipOffhand(attacker);
            bool nativeOneHand = !equipped.HoldInTwoHands;
            int katanaOneMartial = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, martial, facts);
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                set.KatanaProficiency, facts);
            int katanaOneExact = Attack(attacker, target, equipped);
            RemoveOffhand(attacker, ref offhand);
            int katanaTwoExact = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker,
                set.KatanaProficiency, facts);
            string katanaObserved = "two=" + katanaTwoUntrained + "->" +
                katanaTwoMartial + "->" + katanaTwoExact + ";one=" +
                katanaOneMartial + "->" + katanaOneExact + ";grip=" +
                nativeTwoHands + "/" + nativeOneHand;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-katana-grip-proficiency",
                "martial removes -4 only in native two-hand grip; exact Katana proficiency removes it in both grips",
                katanaObserved, nativeTwoHands && nativeOneHand &&
                    katanaTwoMartial == katanaTwoUntrained + 4 &&
                    katanaOneExact == katanaOneMartial + 4 &&
                    katanaTwoExact == katanaTwoUntrained + 4,
                "ItemEntityWeapon.HoldInTwoHands and live RuleAttackWithWeapon");

            Swap(attacker, set.Require(EasternWeaponFamily.Nodachi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int nodachiUntrained = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, martial, facts);
            int nodachiMartial = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, martial, facts);
            string nodachiObserved = nodachiUntrained + "->" + nodachiMartial;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-nodachi-martial-proficiency",
                "blanket martial proficiency removes the ordinary -4",
                nodachiObserved, nodachiMartial == nodachiUntrained + 4,
                "live broad native Martial Weapon Proficiency fact");
            diagnostics.Add("proficiency{wak=" + wakObserved + ";katana=" +
                katanaObserved + ";nodachi=" + nodachiObserved + "}");
        }

        private static void QualifyFighterGroups(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, IList<BlueprintUnitFact> facts,
            ref ItemEntityWeapon equipped,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            BlueprintFeature light = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library,
                    WeaponTrainingLightBladesGuid,
                    "native Light Blades weapon training");
            BlueprintFeature heavy = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library,
                    WeaponTrainingHeavyBladesGuid,
                    "native Heavy Blades weapon training");
            BlueprintFeature polearms = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library,
                    WeaponTrainingPolearmsGuid,
                    "native Polearms weapon training");

            Swap(attacker, set.Require(EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, light, facts);
            UnitPartWeaponTraining training = attacker.Descriptor.Get<
                UnitPartWeaponTraining>();
            if (training == null) throw new InvalidOperationException(
                "Native weapon training did not create UnitPartWeaponTraining.");
            int wakizashiLight = training.GetWeaponRank(equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, light, facts);

            Swap(attacker, set.Require(EasternWeaponFamily.Katana,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, heavy, facts);
            int katanaHeavy = training.GetWeaponRank(equipped);
            Swap(attacker, set.Require(EasternWeaponFamily.Nodachi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int nodachiHeavy = training.GetWeaponRank(equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, heavy, facts);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, polearms, facts);
            int nodachiPolearms = training.GetWeaponRank(equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, heavy, facts);
            int nodachiDual = training.GetWeaponRank(equipped);
            Swap(attacker, set.Require(EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int switchedAway = training.GetWeaponRank(equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, heavy, facts);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, polearms,
                facts);

            BlueprintWeaponType nodachiType = set.Require(
                EasternWeaponFamily.Nodachi).WeaponType;
            string observed = "wakLight=" + wakizashiLight +
                ";katHeavy=" + katanaHeavy + ";nodHeavy=" + nodachiHeavy +
                ";nodPole=" + nodachiPolearms + ";nodDual=" + nodachiDual +
                ";switched=" + switchedAway + ";range=" +
                nodachiType.AttackRange;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-fighter-groups",
                "Wakizashi receives Light Blades; Katana receives Heavy Blades; Nodachi receives Heavy Blades or Polearms once without reach or double application; switching removes the match",
                observed, wakizashiLight == 1 && katanaHeavy == 1 &&
                    nodachiHeavy == 1 && nodachiPolearms == 1 &&
                    nodachiDual == 1 && switchedAway == 0 &&
                    nodachiType.FighterGroup == WeaponFighterGroup.BladesHeavy &&
                    nodachiType.AttackRange.Value == 2,
                "native WeaponGroupAttackBonus facts and UnitPartWeaponTraining.GetWeaponRank");
            diagnostics.Add("groups{" + observed + "}");
        }

        private static void QualifyFinesse(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, IList<BlueprintUnitFact> facts,
            ref ItemEntityWeapon equipped,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            Swap(attacker, set.Require(EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            RuleCalculateAttackBonusWithoutTarget baseAttack =
                ElvenBranchedSpearCombatScenario.AttackBonus(attacker, equipped);
            RuleCalculateWeaponStats baseDamage =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            BlueprintFeature finesse = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library,
                    WeaponFinesseGuid, "native Weapon Finesse");
            ElvenBranchedSpearCombatScenario.AddFact(attacker, finesse, facts);
            RuleCalculateAttackBonusWithoutTarget finesseAttack =
                ElvenBranchedSpearCombatScenario.AttackBonus(attacker, equipped);
            RuleCalculateWeaponStats finesseDamage =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                set.WakizashiFinesseTraining, facts);
            RuleCalculateWeaponStats trainingDamage =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            BlueprintItemWeapon[] wakizashiFamily = set.Require(
                EasternWeaponFamily.Wakizashi).Entries.Select(value =>
                    value.Item).Concat(set.Named.Entries.Where(value =>
                        value.Spec.Family == EasternWeaponFamily.Wakizashi)
                    .Select(value => value.Item)).ToArray();
            bool familyExact = wakizashiFamily.Length == 10;
            var familyObserved = new List<string>();
            foreach (BlueprintItemWeapon item in wakizashiFamily)
            {
                Swap(attacker, item, ref equipped);
                RuleCalculateWeaponStats stats =
                    ElvenBranchedSpearCombatScenario.WeaponStats(attacker,
                        equipped);
                familyExact &= UsesOneDexterityModifier(attacker, stats) &&
                    stats.DamageBonusStatMultiplier == 1f;
                familyObserved.Add(item.name + "=" + DescribeDamage(stats));
            }
            string observed = baseAttack.AttackBonusStat + "/" +
                baseDamage.DamageBonusStat + "->" +
                finesseAttack.AttackBonusStat + "/" +
                finesseDamage.DamageBonusStat + "->" +
                trainingDamage.DamageBonusStat + "x" +
                trainingDamage.DamageBonusStatMultiplier + ";family=" +
                string.Join("|", familyObserved.ToArray());
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-wakizashi-finesse",
                "STR/STR; Weapon Finesse DEX/STR; all ten Wakizashi variants, including Agile plus Finesse Training, use one DEX damage modifier",
                observed,
                baseAttack.AttackBonusStat == StatType.Strength &&
                baseDamage.DamageBonusStat == StatType.Strength &&
                finesseAttack.AttackBonusStat == StatType.Dexterity &&
                finesseDamage.DamageBonusStat == StatType.Strength &&
                trainingDamage.DamageBonusStat == StatType.Dexterity &&
                trainingDamage.DamageBonusStatMultiplier == 1f && familyExact,
                "live attack-stat and weapon-stat rule events");
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker,
                set.WakizashiFinesseTraining, facts);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, finesse, facts);
            diagnostics.Add("finesse{" + observed + "}");
        }

        private static void QualifyNamedProperties(EasternWeaponBlueprintSet set,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            var observed = new List<string>();
            bool exact = set.Named.Entries.Length == 18;
            foreach (EasternWeaponNamedBlueprintEntry entry in set.Named.Entries)
            {
                var expected = new List<string> {
                    set.ProficiencyPolicy.AssetGuid,
                    EnhancementGuid(entry.Spec.Enhancement) };
                foreach (EasternWeaponNativeProperty property in Enum.GetValues(
                    typeof(EasternWeaponNativeProperty)))
                {
                    if (property == EasternWeaponNativeProperty.None ||
                        !entry.Spec.Has(property)) continue;
                    expected.Add(PropertyGuid(property));
                }
                BlueprintWeaponEnchantment custom = set.Named.Enchantments.For(
                    entry.Spec.Kind);
                if (custom != null) expected.Add(custom.AssetGuid);
                string[] actual = entry.Item.Enchantments.Select(value =>
                    value == null ? "<null>" : value.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] wanted = expected.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
                bool itemExact = actual.SequenceEqual(wanted) &&
                    ReferenceEquals(entry.Item.Type,
                        set.Require(entry.Spec.Family).WeaponType) &&
                    entry.Spec.NativeEffectiveBonus <= 10;
                exact &= itemExact;
                observed.Add(entry.Spec.DisplayName + "=" +
                    string.Join(",", actual) + "/" +
                    entry.Spec.NativeEffectiveBonus);
            }
            string text = string.Join("|", observed.ToArray());
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-all-named-native-properties",
                "all 18 named weapons contain exactly their approved native enchantments, optional exact bespoke enchantment, family type, and at most +10 effective bonus",
                text, exact,
                "live BlueprintItemWeapon.Enchantments arrays and exact native/custom blueprint identities");
            diagnostics.Add("namedProperties{" + text + "}");
        }

        private static void QualifyNamedEffects(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, UnitEntityData target,
            IList<BlueprintUnitFact> facts, ref ItemEntityWeapon equipped,
            ref ItemEntityWeapon offhand, ref ActivatableAbility powerAttack,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            int initiativeBefore = attacker.Descriptor.Stats.Initiative.ModifiedValue;
            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.WayfarersOath).Item, ref equipped);
            int initiativeEquipped = attacker.Descriptor.Stats.Initiative.ModifiedValue;
            Swap(attacker, set.Require(EasternWeaponFamily.Katana,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int initiativeAfter = attacker.Descriptor.Stats.Initiative.ModifiedValue;
            string wayfarer = initiativeBefore + "->" + initiativeEquipped +
                "->" + initiativeAfter;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-wayfarers-oath",
                "active exact weapon grants one +2 competence Initiative and switching removes it",
                wayfarer, initiativeEquipped == initiativeBefore + 2 &&
                    initiativeAfter == initiativeBefore,
                "live equipped fact and Initiative ModifiableValue");

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.FallingPetal).Item, ref equipped);
            EasternNamedWeaponEffectDiagnostics.Reset();
            int acBefore = attacker.Descriptor.Stats.AC.ModifiedValue;
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = -100;
            RuleAttackWithWeapon miss = TriggerNativeAttack(attacker, target,
                equipped,
                ElvenBranchedSpearCombatScenario.FindNativeD20Seed(1));
            int afterMiss = EasternNamedWeaponEffectDiagnostics
                .FallingPetalApplications;
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 0;
            RuleAttackWithWeapon unconfirmed = FindUnconfirmedThreat(attacker,
                target, equipped, set.Named.Buffs.FallingPetal);
            int afterUnconfirmed = EasternNamedWeaponEffectDiagnostics
                .FallingPetalApplications;
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            int seed = ElvenBranchedSpearCombatScenario.FindNativeD20Seed(19);
            RuleAttackWithWeapon critical = ElvenBranchedSpearCombatScenario
                .NativeHitAttack(attacker, target, equipped, seed);
            int acCritical = attacker.Descriptor.Stats.AC.ModifiedValue;
            int applications = EasternNamedWeaponEffectDiagnostics
                .FallingPetalApplications;
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int afterOrdinary = EasternNamedWeaponEffectDiagnostics
                .FallingPetalApplications;
            Swap(attacker, set.Require(EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int acAfterSwap = attacker.Descriptor.Stats.AC.ModifiedValue;
            string falling = "miss=" + miss.AttackRoll.IsHit +
                ";unconfirmed=" + unconfirmed.AttackRoll.IsCriticalRoll +
                "/" + unconfirmed.AttackRoll.IsCriticalConfirmed +
                ";confirmed=" + critical.AttackRoll.IsCriticalConfirmed +
                ";ac=" + acBefore + "->" + acCritical + "->" +
                acAfterSwap + ";applications=" + afterMiss + "/" +
                afterUnconfirmed + "/" + applications + "->" +
                afterOrdinary;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-falling-petal",
                "miss and unconfirmed threat do not trigger; native confirmed critical grants one +1 Dodge for one round; ordinary hit and weapon swap do not retain it",
                falling, !miss.AttackRoll.IsHit && afterMiss == 0 &&
                    unconfirmed.AttackRoll.IsCriticalRoll &&
                    !unconfirmed.AttackRoll.IsCriticalConfirmed &&
                    afterUnconfirmed == 0 &&
                    critical.AttackRoll.IsCriticalConfirmed &&
                    acCritical == acBefore + 1 && applications == 1 &&
                    afterOrdinary == 1 && acAfterSwap == acBefore,
                "native seeded critical confirmation, timed buff, and equipment callback");

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.MoonlitCrossing).Item, ref equipped);
            EasternNamedWeaponEffectDiagnostics.Reset();
            int moonlitAc = attacker.Descriptor.Stats.AC.ModifiedValue;
            RuleCalculateWeaponStats twoHand =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            bool observedTwoHand = equipped.HoldInTwoHands;
            int twoApplications = EasternNamedWeaponEffectDiagnostics
                .MoonlitDamageApplications;
            offhand = EquipOffhand(attacker);
            EasternNamedWeaponEffectDiagnostics.Reset();
            int oneHandAc = attacker.Descriptor.Stats.AC.ModifiedValue;
            RuleCalculateWeaponStats oneHand =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            bool observedOneHand = !equipped.HoldInTwoHands;
            int oneApplications = EasternNamedWeaponEffectDiagnostics
                .MoonlitDamageApplications;
            string moonlit = "grip=" + observedTwoHand + "/" +
                observedOneHand + ";ac=" + moonlitAc + "->" +
                oneHandAc + ";damageApplications=" + twoApplications + "/" +
                oneApplications;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-moonlit-crossing",
                "two-hand grip applies one +2 damage source only; one-hand grip applies one +1 Dodge source only",
                moonlit, observedTwoHand && observedOneHand &&
                    twoApplications == 1 && oneApplications == 0 &&
                    oneHandAc == moonlitAc + 1,
                "same native HoldInTwoHands authority, weapon-stat event, and AC stat");
            RemoveOffhand(attacker, ref offhand);

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.MountainSunder).Item, ref equipped);
            BlueprintFeature powerAttackFeature = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    EasternWeaponNamedBlueprints.PowerAttackFeatureGuid,
                    "native Power Attack feat");
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                powerAttackFeature, facts);
            powerAttack = attacker.Descriptor.ActivatableAbilities.Enumerable
                .Single(value => value != null && string.Equals(
                    value.Blueprint.AssetGuid,
                    EasternWeaponNamedBlueprints.PowerAttackToggleGuid,
                    StringComparison.Ordinal));
            powerAttack.IsOn = false;
            powerAttack.Stop(true);
            EasternNamedWeaponEffectDiagnostics.Reset();
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = -100;
            RuleAttackWithWeapon mountainMiss = TriggerNativeAttack(attacker,
                target, equipped,
                ElvenBranchedSpearCombatScenario.FindNativeD20Seed(1));
            int missed = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int inactive = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            powerAttack.IsOn = true;
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int first = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            int force = EasternNamedWeaponEffectDiagnostics
                .LastMountainSunderDamage;
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int repeated = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            Swap(attacker, set.Require(EasternWeaponFamily.Nodachi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.MountainSunder).Item, ref equipped);
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int afterSwitch = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            RemoveBuff(attacker, set.Named.Buffs.MountainSunderMarker);
            RuleAttackWithWeapon mountainCritical =
                ElvenBranchedSpearCombatScenario.NativeHitAttack(attacker,
                    target, equipped,
                    ElvenBranchedSpearCombatScenario.FindNativeD20Seed(19));
            int nextRound = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            int criticalForce = EasternNamedWeaponEffectDiagnostics
                .LastMountainSunderDamage;
            string mountain = "miss=" + mountainMiss.AttackRoll.IsHit + "/" +
                missed + ";applications=" + inactive + "->" + first +
                "->" + repeated + "->switch=" + afterSwitch +
                "->critical=" + nextRound + ";force=" + force + "/" +
                criticalForce + ";criticalConfirmed=" +
                mountainCritical.AttackRoll.IsCriticalConfirmed +
                ";running=" + powerAttack.IsRunning;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-mountain-sunder",
                "miss does not consume; inactive Power Attack is rejected; first hit applies one 1d6 force packet; repeat and weapon switching are blocked until marker reset; critical does not multiply the force die",
                mountain, !mountainMiss.AttackRoll.IsHit && missed == 0 &&
                    inactive == 0 && first == 1 && repeated == 1 &&
                    afterSwitch == 1 && nextRound == 2 &&
                    mountainCritical.AttackRoll.IsCriticalConfirmed &&
                    force >= 1 && force <= 6 && criticalForce >= 1 &&
                    criticalForce <= 6 &&
                    powerAttack.IsRunning,
                "native Power Attack activatable, live attacks, damage rule, and one-round buff marker");
            powerAttack.IsOn = false;
            powerAttack.Stop(true);

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.UnfixedForm).Item, ref equipped);
            EasternNamedWeaponEffectDiagnostics.Reset();
            RuleCalculateWeaponStats ordinary =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            int ordinaryApplications = EasternNamedWeaponEffectDiagnostics
                .UnfixedFormApplications;
            Size originalSize = attacker.Descriptor.State.Size;
            attacker.Descriptor.State.Size = originalSize == Size.Medium ?
                Size.Large : Size.Medium;
            RuleCalculateWeaponStats transformed =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            int transformedApplications = EasternNamedWeaponEffectDiagnostics
                .UnfixedFormApplications;
            attacker.Descriptor.State.Size = originalSize;
            SetPolymorphed(attacker, true);
            RuleCalculateWeaponStats polymorphed =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            int polymorphedApplications = EasternNamedWeaponEffectDiagnostics
                .UnfixedFormApplications;
            attacker.Descriptor.State.Size = originalSize == Size.Medium ?
                Size.Large : Size.Medium;
            RuleCalculateWeaponStats simultaneous =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            int simultaneousApplications = EasternNamedWeaponEffectDiagnostics
                .UnfixedFormApplications;
            attacker.Descriptor.State.Size = originalSize;
            SetPolymorphed(attacker, false);
            string unfixed = "applications=" + ordinaryApplications + "->" +
                transformedApplications + "->" + polymorphedApplications +
                "->" + simultaneousApplications + ";weaponSize=" +
                ordinary.WeaponSize + "->" + transformed.WeaponSize + "/" +
                polymorphed.WeaponSize + "/" + simultaneous.WeaponSize;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-unfixed-form",
                "ordinary state is rejected; changed size, exact polymorph state, and both conditions together each apply exactly one native weapon-size step",
                unfixed, ordinaryApplications == 0 &&
                    transformedApplications == 1 &&
                    polymorphedApplications == 2 &&
                    simultaneousApplications == 3 &&
                    (int)transformed.WeaponSize ==
                        (int)ordinary.WeaponSize + 1 &&
                    (int)polymorphed.WeaponSize ==
                        (int)ordinary.WeaponSize + 1 &&
                    (int)simultaneous.WeaponSize ==
                        (int)ordinary.WeaponSize + 1,
                "exact original/current Size state and RuleCalculateWeaponStats.IncreaseWeaponSize");
            diagnostics.Add("named{wayfarer=" + wayfarer + ";falling=" +
                falling + ";moonlit=" + moonlit + ";mountain=" + mountain +
                ";unfixed=" + unfixed + "}");
        }

        private static void QualifyCapstones(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, UnitEntityData target,
            ref ItemEntityWeapon equipped, ref ItemEntityWeapon offhand,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 12;
            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.EmptySleeve).Item, ref equipped);
            int ordinaryMain = PlanFullAttack(attacker, target).Count;
            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.NightWithoutMoon).Item, ref equipped);
            int speedMain = PlanFullAttack(attacker, target).Count;
            int speedRepeated = PlanFullAttack(attacker, target).Count;
            BlueprintBuff haste = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(BlueprintBootstrap.Library, HasteBuffGuid,
                    "native Haste buff");
            var hasteContext = new MechanicsContext(attacker,
                attacker.Descriptor, haste, null, new TargetWrapper(attacker));
            Buff hasteFact = attacker.Descriptor.Buffs.AddBuff(haste,
                hasteContext,
                TimeSpan.FromSeconds(60d));
            if (hasteFact == null) throw new InvalidOperationException(
                "The native Haste control buff could not be applied.");
            int speedWithHaste = PlanFullAttack(attacker, target).Count;
            attacker.Descriptor.Buffs.RemoveFact(hasteFact);
            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.EmptySleeve).Item, ref equipped);
            int afterSwitch = PlanFullAttack(attacker, target).Count;

            Swap(attacker, set.Require(EasternWeaponFamily.Katana,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            offhand = EquipOffhand(attacker, set.Require(
                EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item);
            List<AttackHandInfo> ordinaryTwoWeapon = PlanFullAttack(attacker,
                target);
            ItemEntityWeapon ordinaryOffhandItem = offhand;
            int ordinaryOffhand = ordinaryTwoWeapon.Count(value =>
                ReferenceEquals(value.Weapon, ordinaryOffhandItem));
            RemoveOffhand(attacker, ref offhand);
            offhand = EquipOffhand(attacker, set.Named.Require(
                EasternWeaponNamedKind.NightWithoutMoon).Item);
            List<AttackHandInfo> speedTwoWeapon = PlanFullAttack(attacker,
                target);
            ItemEntityWeapon speedOffhandItem = offhand;
            int speedOffhand = speedTwoWeapon.Count(value =>
                ReferenceEquals(value.Weapon, speedOffhandItem));
            RemoveOffhand(attacker, ref offhand);

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.UnfixedForm).Item, ref equipped);
            int ordinaryNodachi = PlanFullAttack(attacker, target).Count;
            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.WorldTreeSeverer).Item, ref equipped);
            int worldTreeSpeed = PlanFullAttack(attacker, target).Count;
            string speedObserved = "main=" + ordinaryMain + "->" +
                speedMain + "/" + speedRepeated + ";haste=" +
                speedWithHaste + ";switch=" + afterSwitch + ";offhand=" +
                ordinaryOffhand + "->" + speedOffhand + ";world=" +
                ordinaryNodachi + "->" + worldTreeSpeed;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-capstone-speed",
                "Speed adds exactly one main- or offhand full-attack entry, does not stack with Haste, repeats once per full attack, clears on switch, and works on both Speed capstones",
                speedObserved, speedMain == ordinaryMain + 1 &&
                    speedRepeated == speedMain && speedWithHaste == speedMain &&
                    afterSwitch == ordinaryMain &&
                    speedOffhand == ordinaryOffhand + 1 &&
                    worldTreeSpeed == ordinaryNodachi + 1,
                "native UnitAttack.CreateFullAttack and WeaponExtraAttack/Haste arbitration");

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.HeavensMeasure).Item, ref equipped);
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            int seed = ElvenBranchedSpearCombatScenario.FindNativeD20Seed(10);
            RuleAttackWithWeapon living = TriggerNativeAttack(attacker, target,
                equipped, seed);
            BlueprintFeature undead = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library, UndeadTypeGuid,
                    "native Undead type fact");
            if (target.Descriptor.AddFact(undead) == null)
                throw new InvalidOperationException(
                    "The request-local target could not receive Undead type.");
            RuleAttackWithWeapon excluded = TriggerNativeAttack(attacker,
                target, equipped, seed);
            target.Descriptor.RemoveFact(undead);
            string brilliantObserved = "living=" +
                living.AttackRoll.IsHit + ";undead=" +
                excluded.AttackRoll.IsHit + ";effective=" +
                set.Named.Require(EasternWeaponNamedKind.HeavensMeasure)
                    .Spec.NativeEffectiveBonus;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-heavens-measure-brilliant-energy",
                "native Brilliant Energy hits the living control, cannot affect the native Undead type, and remains at +10 effective bonus",
                brilliantObserved, living.AttackRoll.IsHit &&
                    !excluded.AttackRoll.IsHit &&
                    set.Named.Require(EasternWeaponNamedKind.HeavensMeasure)
                        .Spec.NativeEffectiveBonus == 10,
                "native BrilliantEnergy and MissAgainstFactOwner components on live RuleAttackWithWeapon events");
            diagnostics.Add("capstones{speed=" + speedObserved +
                ";brilliant=" + brilliantObserved + "}");
        }

        private static int Attack(UnitEntityData attacker,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            return ElvenBranchedSpearCombatScenario.WeaponAttack(attacker,
                target, weapon).AttackRoll.AttackBonus;
        }

        private static List<AttackHandInfo> PlanFullAttack(
            UnitEntityData attacker, UnitEntityData target)
        {
            var command = new UnitAttack(attacker);
            PropertyInfo targetProperty = typeof(UnitAttack).GetProperty(
                "Target", Members | BindingFlags.DeclaredOnly);
            PropertyInfo executorProperty = typeof(UnitCommand).GetProperty(
                "Executor", Members | BindingFlags.DeclaredOnly);
            MethodInfo create = typeof(UnitAttack).GetMethod(
                "CreateFullAttack", Members, null, Type.EmptyTypes, null);
            if (targetProperty == null || executorProperty == null ||
                create == null ||
                create.ReturnType != typeof(List<AttackHandInfo>))
                throw new MissingMethodException(typeof(UnitAttack).FullName,
                    "CreateFullAttack() : List<AttackHandInfo>");
            executorProperty.SetValue(command, attacker, null);
            targetProperty.SetValue(command, target, null);
            var result = create.Invoke(command, null) as List<AttackHandInfo>;
            if (result == null || result.Count == 0 ||
                result.Any(value => value == null || value.Weapon == null))
                throw new InvalidOperationException(
                    "Native full-attack planning returned an incomplete list.");
            return result;
        }

        private static RuleAttackWithWeapon TriggerNativeAttack(
            UnitEntityData attacker, UnitEntityData target,
            ItemEntityWeapon weapon, int seed)
        {
            int damage = target.Descriptor.Damage;
            UnityEngine.Random.InitState(seed);
            RuleAttackWithWeapon attack = Rulebook.Trigger(
                new RuleAttackWithWeapon(attacker, target, weapon, 0));
            target.Descriptor.Damage = damage;
            if (attack.AttackRoll == null) throw new InvalidOperationException(
                "Native attack did not produce an attack roll.");
            return attack;
        }

        private static RuleAttackWithWeapon FindUnconfirmedThreat(
            UnitEntityData attacker, UnitEntityData target,
            ItemEntityWeapon weapon, BlueprintBuff fallingPetal)
        {
            for (int candidate = 1; candidate <= 100000; candidate++)
            {
                UnityEngine.Random.InitState(candidate);
                if (UnityEngine.Random.Range(1, 21) != 19) continue;
                RuleAttackWithWeapon attack = TriggerNativeAttack(attacker,
                    target, weapon, candidate);
                if (attack.AttackRoll.IsHit &&
                    attack.AttackRoll.IsCriticalRoll &&
                    !attack.AttackRoll.IsCriticalConfirmed)
                    return attack;
                RemoveBuff(attacker, fallingPetal);
                EasternNamedWeaponEffectDiagnostics.Reset();
            }
            throw new InvalidOperationException(
                "No native seeded unconfirmed Falling Petal threat was found.");
        }

        private static void SetPolymorphed(UnitEntityData unit, bool value)
        {
            PropertyInfo property = unit == null || unit.Body == null ? null :
                unit.Body.GetType().GetProperty("IsPolymorphed", Members);
            MethodInfo setter = property == null ? null :
                property.GetSetMethod(true);
            if (property == null || property.PropertyType != typeof(bool) ||
                setter == null || setter.IsStatic ||
                setter.GetParameters().Length != 1)
                throw new InvalidOperationException(
                    "Exact UnitBody.IsPolymorphed setter is unavailable.");
            setter.Invoke(unit.Body, new object[] { value });
            if (unit.Body.IsPolymorphed != value)
                throw new InvalidOperationException(
                    "Exact UnitBody.IsPolymorphed state did not change.");
        }

        private static bool UsesOneDexterityModifier(UnitEntityData unit,
            RuleCalculateWeaponStats stats)
        {
            if (unit == null || stats == null ||
                stats.DamageBonusStat != StatType.Dexterity) return false;
            int expected = (int)Math.Floor(
                unit.Descriptor.Stats.Dexterity.Bonus *
                stats.DamageBonusStatMultiplier);
            return stats.BonusDamage == expected + stats.Enhancement;
        }

        private static string DescribeDamage(RuleCalculateWeaponStats stats)
        {
            return stats.DamageBonusStat + "x" +
                stats.DamageBonusStatMultiplier + ";bonus=" +
                stats.BonusDamage + ";enhancement=" + stats.Enhancement;
        }

        private static int CountFeature(BlueprintFeatureSelection selection,
            BlueprintFeature feature)
        {
            return (selection.AllFeatures ?? Array.Empty<BlueprintFeature>())
                .Count(value => ReferenceEquals(value, feature) ||
                    value != null && string.Equals(value.AssetGuid,
                        feature.AssetGuid, StringComparison.Ordinal));
        }

        private static int CountCategory(string selectorGuid,
            EasternWeaponFamily family)
        {
            BlueprintParametrizedFeature selector = BlueprintLibraryLookup
                .RequireExact<BlueprintParametrizedFeature>(
                    BlueprintBootstrap.Library, selectorGuid,
                    "native excluded weapon selector");
            WeaponCategory category = EasternWeaponCategoryRuntime.Category(
                family);
            return selector.GetFullSelectionItems().Count(value =>
                value != null && value.Param != null &&
                value.Param.WeaponCategory.HasValue &&
                value.Param.WeaponCategory.Value.Equals(category));
        }

        private static string EnhancementGuid(int enhancement)
        {
            return enhancement == 1
                ? EasternWeaponBlueprints.NativeEnhancementOneGuid
                : enhancement == 2
                ? EasternWeaponNamedBlueprints.EnhancementTwoGuid
                : enhancement == 3
                ? EasternWeaponNamedBlueprints.EnhancementThreeGuid
                : enhancement == 4
                ? EasternWeaponNamedBlueprints.EnhancementFourGuid
                : EasternWeaponNamedBlueprints.EnhancementFiveGuid;
        }

        private static string PropertyGuid(
            EasternWeaponNativeProperty property)
        {
            return property == EasternWeaponNativeProperty.Flaming
                ? EasternWeaponNamedBlueprints.FlamingGuid
                : property == EasternWeaponNativeProperty.Frost
                ? EasternWeaponNamedBlueprints.FrostGuid
                : property == EasternWeaponNativeProperty.Agile
                ? EasternWeaponNamedBlueprints.AgileGuid
                : property == EasternWeaponNativeProperty.Keen
                ? EasternWeaponNamedBlueprints.KeenGuid
                : property == EasternWeaponNativeProperty.GhostTouch
                ? EasternWeaponNamedBlueprints.GhostTouchGuid
                : property == EasternWeaponNativeProperty.Shock
                ? EasternWeaponNamedBlueprints.ShockGuid
                : property == EasternWeaponNativeProperty.Thundering
                ? EasternWeaponNamedBlueprints.ThunderingGuid
                : property == EasternWeaponNativeProperty.Holy
                ? EasternWeaponNamedBlueprints.HolyGuid
                : property == EasternWeaponNativeProperty.BrilliantEnergy
                ? EasternWeaponNamedBlueprints.BrilliantEnergyGuid
                : property == EasternWeaponNativeProperty.Speed
                ? EasternWeaponNamedBlueprints.SpeedGuid
                : throw new ArgumentOutOfRangeException("property");
        }

        private static void Swap(UnitEntityData unit,
            BlueprintItemWeapon blueprint, ref ItemEntityWeapon equipped)
        {
            ElvenBranchedSpearCombatScenario.RemoveEquipped(unit,
                ref equipped);
            equipped = ElvenBranchedSpearCombatScenario.Equip(unit, blueprint);
        }

        private static ItemEntityWeapon EquipOffhand(UnitEntityData unit)
        {
            BlueprintItemWeapon blueprint = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(BlueprintBootstrap.Library,
                    ShortswordItemGuid, "native Shortsword offhand control");
            return EquipOffhand(unit, blueprint);
        }

        private static ItemEntityWeapon EquipOffhand(UnitEntityData unit,
            BlueprintItemWeapon blueprint)
        {
            var item = new ItemEntityWeapon(blueprint);
            unit.Body.SecondaryHand.InsertItem(item);
            if (!ReferenceEquals(unit.Body.SecondaryHand.MaybeWeapon, item))
                throw new InvalidOperationException(
                    "The offhand control did not remain equipped.");
            return item;
        }

        private static void RemoveOffhand(UnitEntityData unit,
            ref ItemEntityWeapon item)
        {
            if (unit != null && unit.Body != null &&
                unit.Body.SecondaryHand != null &&
                unit.Body.SecondaryHand.MaybeItem != null)
                unit.Body.SecondaryHand.RemoveItem(false);
            if (item != null) item.Dispose();
            item = null;
        }

        private static void RemoveBuff(UnitEntityData unit,
            Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff blueprint)
        {
            Buff buff = unit == null || unit.Descriptor == null ? null :
                unit.Descriptor.Buffs.GetBuff(blueprint);
            if (buff != null) unit.Descriptor.Buffs.RemoveFact(buff);
        }

    }
}
