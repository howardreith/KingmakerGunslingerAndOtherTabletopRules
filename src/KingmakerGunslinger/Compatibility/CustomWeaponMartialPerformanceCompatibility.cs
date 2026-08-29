using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.CustomWeapons;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Feats;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Compatibility
{
    [Serializable]
    public sealed class PrerequisiteCustomWeaponMartialPerformanceProficiency :
        Prerequisite
    {
        public WeaponCategory Category;
        public bool KatanaGripDependent;
        public string WeaponDisplayName;

        public override bool Check(FeatureSelectionState selectionState,
            UnitDescriptor unit, LevelUpState state)
        {
            return unit != null &&
                CustomWeaponMartialPerformanceProficiencyPolicy.CanUse(
                    unit.Proficiencies.Contains(Category),
                    EasternWeaponProficiencyRuntime.HasBroadMartial(unit),
                    KatanaGripDependent);
        }

        public override string GetUIText()
        {
            return "Proficiency with " +
                (string.IsNullOrWhiteSpace(WeaponDisplayName)
                    ? Category.ToString()
                    : WeaponDisplayName);
        }
    }

    internal sealed class CustomWeaponMartialPerformancePublication
    {
        internal const string SelectionGuid =
            "19d1ff4cf70845d094b0ec231473e97f";
        internal const string ExpectedSelectionName =
            "MartialPerformanceFeatureSelection";
        internal const string ExpectedSelectionType =
            "Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection";
        internal const string DaggerDonorGuid =
            "b7786666fe5b4694b8c4560efa6053c3";
        internal const string ExpectedDonorName =
            "DaggerMartialPerformanceFeature";
        internal const string WeaponFocusGuid =
            "1e1f627d26ad36f43bbd26cc2bf8ac7e";
        internal const int BlueprintCount = 7;

        internal const string PistolSymbol =
            "KMG.CustomWeapons.MartialPerformance.Pistol";
        internal const string MusketSymbol =
            "KMG.CustomWeapons.MartialPerformance.Musket";
        internal const string BlunderbussSymbol =
            "KMG.CustomWeapons.MartialPerformance.Blunderbuss";
        internal const string SpearSymbol =
            "KMG.CustomWeapons.MartialPerformance.ElvenBranchedSpear";
        internal const string WakizashiSymbol =
            "KMG.CustomWeapons.MartialPerformance.Wakizashi";
        internal const string KatanaSymbol =
            "KMG.CustomWeapons.MartialPerformance.Katana";
        internal const string NodachiSymbol =
            "KMG.CustomWeapons.MartialPerformance.Nodachi";

        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly string[] FirearmSymbols =
        {
            PistolSymbol, MusketSymbol, BlunderbussSymbol
        };

        private readonly BlueprintFeatureSelection _selection;
        private readonly BlueprintFeature[] _featuresBefore;
        private readonly BlueprintFeature[] _allFeaturesBefore;
        private BlueprintFeature[] _registered =
            Array.Empty<BlueprintFeature>();
        private bool _rolledBack;

        private CustomWeaponMartialPerformancePublication(
            BlueprintFeatureSelection selection)
        {
            _selection = selection;
            _featuresBefore = selection == null ? null : selection.Features;
            _allFeaturesBefore = selection == null ? null :
                selection.AllFeatures;
        }

        internal bool OptionalModPresent
        { get { return _selection != null; } }

        internal int RegisteredCount
        { get { return _registered.Length; } }

        internal BlueprintFeature[] Registered
        { get { return (BlueprintFeature[])_registered.Clone(); } }

        internal static CustomWeaponMartialPerformancePublication
            RegisterAndPublish(LibraryScriptableObject library,
                BlueprintRegistry registry, FirearmFeatBlueprintSet firearmFeats,
                bool publishFirearms, bool publishSpear,
                bool publishEastern)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");

            BlueprintFeatureSelection selection = FindSelection(library);
            var publication =
                new CustomWeaponMartialPerformancePublication(selection);
            if (selection == null) return publication;
            if (firearmFeats == null)
                throw new ArgumentNullException("firearmFeats");

            try
            {
                BlueprintFeature donor = RequireDonor(library, selection);
                BlueprintParametrizedFeature weaponFocus =
                    BlueprintLibraryLookup.RequireExact<
                        BlueprintParametrizedFeature>(library,
                            WeaponFocusGuid, "native Weapon Focus");
                MartialPerformanceSpec[] specs = BuildSpecs(firearmFeats,
                    publishFirearms, publishSpear, publishEastern);
                publication._registered = specs.Select(spec =>
                    Register(registry, donor, weaponFocus, spec)).ToArray();
                if (publication._registered.Length != BlueprintCount)
                    throw new InvalidOperationException(
                        "Martial Performance registration count is not exact.");

                var active = new List<
                    CustomWeaponMartialPerformanceChoice<BlueprintFeature>>();
                for (int index = 0; index < specs.Length; index++)
                    if (specs[index].Publish)
                        active.Add(new CustomWeaponMartialPerformanceChoice<
                            BlueprintFeature>(
                                publication._registered[index].AssetGuid,
                                specs[index].DisplayName,
                                publication._registered[index]));
                var policy = new CustomWeaponMartialPerformanceSelectionPolicy<
                    BlueprintFeature>(publication._allFeaturesBefore,
                        value => value.AssetGuid);
                selection.AllFeatures = policy.Publish(
                    publication._registered, active);
                publication.Validate(specs);
                return publication;
            }
            catch
            {
                publication.Rollback();
                throw;
            }
        }

        internal void Rollback()
        {
            if (_rolledBack) return;
            if (_selection != null)
            {
                _selection.Features = _featuresBefore;
                _selection.AllFeatures = _allFeaturesBefore;
            }
            _rolledBack = true;
        }

        private static BlueprintFeatureSelection FindSelection(
            LibraryScriptableObject library)
        {
            BlueprintScriptableObject value = null;
            bool found = library.BlueprintsByAssetId != null &&
                library.BlueprintsByAssetId.TryGetValue(SelectionGuid,
                    out value);
            if (!CustomWeaponMartialPerformanceIdentityPolicy.IsPresent(
                    found, found ? value.GetType().FullName : null,
                    found ? value.name : null, ExpectedSelectionType,
                    ExpectedSelectionName))
                return null;
            return (BlueprintFeatureSelection)value;
        }

        private static BlueprintFeature RequireDonor(
            LibraryScriptableObject library,
            BlueprintFeatureSelection selection)
        {
            BlueprintFeature donor = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, DaggerDonorGuid,
                    "Call of the Wild Dagger Martial Performance donor");
            if (!string.Equals(donor.name, ExpectedDonorName,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Call of the Wild Martial Performance donor identity changed.");
            int references = (selection.AllFeatures ??
                Array.Empty<BlueprintFeature>()).Count(value =>
                    Same(value, donor));
            if (references != 1)
                throw new InvalidOperationException(
                    "Call of the Wild Martial Performance donor membership changed.");
            BlueprintComponent[] components = donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (components.Length != 2 ||
                components.OfType<AddParametrizedFeatures>().Count() != 1 ||
                components.OfType<PrerequisiteProficiency>().Count() != 1)
                throw new InvalidOperationException(
                    "Call of the Wild Martial Performance child shape changed.");
            return donor;
        }

        private static MartialPerformanceSpec[] BuildSpecs(
            FirearmFeatBlueprintSet firearmFeats, bool publishFirearms,
            bool publishSpear, bool publishEastern)
        {
            FirearmKind[] kinds = OfficialFirearmSupport.Kinds;
            BlueprintFeature[] firearmParameters =
                firearmFeats.WeaponFocusChoices;
            if (kinds.Length != FirearmSymbols.Length ||
                firearmParameters == null ||
                firearmParameters.Length != kinds.Length ||
                firearmParameters.Any(value => value == null))
                throw new InvalidOperationException(
                    "Official firearm Martial Performance catalog changed.");

            var specs = new List<MartialPerformanceSpec>();
            for (int index = 0; index < kinds.Length; index++)
                specs.Add(MartialPerformanceSpec.Firearm(
                    FirearmSymbols[index], kinds[index].ToString(),
                    kinds[index], firearmParameters[index], publishFirearms));

            specs.Add(MartialPerformanceSpec.WeaponCategoryChoice(SpearSymbol,
                ElvenBranchedSpearCategoryRuntime.DisplayName,
                ElvenBranchedSpearCategoryRuntime.Category, false,
                publishSpear));

            CustomWeaponCategoryDefinition[] eastern =
                EasternWeaponCatalog.AllCategories;
            if (eastern.Length != 3)
                throw new InvalidOperationException(
                    "Eastern weapon Martial Performance catalog changed.");
            foreach (CustomWeaponCategoryDefinition definition in eastern)
            {
                string symbol;
                switch (definition.Key)
                {
                    case "wakizashi": symbol = WakizashiSymbol; break;
                    case "katana": symbol = KatanaSymbol; break;
                    case "nodachi": symbol = NodachiSymbol; break;
                    default:
                        throw new InvalidOperationException(
                            "An Eastern weapon category lacks a stable Martial Performance identity: " +
                            definition.Key + ".");
                }
                specs.Add(MartialPerformanceSpec.WeaponCategoryChoice(symbol,
                    definition.Presentation.DisplayName,
                    (WeaponCategory)definition.CategoryValue,
                    definition.Proficiency ==
                        CustomWeaponProficiencyPolicy.KatanaGripDependent,
                    publishEastern));
            }
            if (specs.Count != BlueprintCount ||
                specs.Select(value => value.Symbol).Distinct(
                    StringComparer.Ordinal).Count() != BlueprintCount)
                throw new InvalidOperationException(
                    "Custom weapon Martial Performance catalog is not exact.");
            return specs.ToArray();
        }

        private static BlueprintFeature Register(BlueprintRegistry registry,
            BlueprintFeature donor, BlueprintParametrizedFeature weaponFocus,
            MartialPerformanceSpec spec)
        {
            return registry.Register<BlueprintFeature>(spec.Symbol, delegate
            {
                BlueprintFeature value = BlueprintCloneService.Clone(donor,
                    "KMG_MartialPerformance_" +
                    spec.DisplayName.Replace(" ", string.Empty));
                value.ComponentsArray = (donor.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Select(component =>
                        (BlueprintComponent)UnityEngine.Object.Instantiate(
                            component)).ToArray();
                RetargetGrant(value, weaponFocus, spec);
                RetargetProficiency(value, spec);
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(spec.Symbol + ".Name",
                        "Martial Performance (" + spec.DisplayName + ")"),
                    LocalizationService.Create(spec.Symbol + ".Description",
                        donor.Description), donor.Icon);
                ValidateChild(value, weaponFocus, spec);
                return value;
            });
        }

        private static void RetargetGrant(BlueprintFeature feature,
            BlueprintParametrizedFeature weaponFocus,
            MartialPerformanceSpec spec)
        {
            AddParametrizedFeatures grant = feature.ComponentsArray
                .OfType<AddParametrizedFeatures>().Single();
            FieldInfo field = typeof(AddParametrizedFeatures).GetField(
                "m_Features", Fields);
            if (field == null || !field.FieldType.IsArray)
                throw new InvalidOperationException(
                    "Native AddParametrizedFeatures storage changed.");
            Array source = field.GetValue(grant) as Array;
            Type rowType = field.FieldType.GetElementType();
            if (source == null || source.Length != 1 || rowType == null)
                throw new InvalidOperationException(
                    "Martial Performance grant row shape changed.");
            object sourceRow = source.GetValue(0);
            object row = Activator.CreateInstance(rowType, true);
            if (sourceRow == null || row == null)
                throw new InvalidOperationException(
                    "Martial Performance grant row could not be cloned.");
            foreach (FieldInfo rowField in rowType.GetFields(Fields))
                rowField.SetValue(row, rowField.GetValue(sourceRow));

            FieldInfo featureField = RequireRowField(rowType, "Feature");
            FieldInfo objectField = RequireRowField(rowType, "ParamObject");
            FieldInfo categoryField = RequireRowField(rowType,
                "ParamWeaponCategory");
            FieldInfo schoolField = RequireRowField(rowType,
                "ParamSpellSchool");
            FieldInfo statField = RequireRowField(rowType, "Stat");
            var donorFocus = featureField.GetValue(sourceRow) as
                BlueprintParametrizedFeature;
            if (donorFocus == null || !string.Equals(donorFocus.AssetGuid,
                    WeaponFocusGuid, StringComparison.Ordinal) ||
                categoryField.FieldType != typeof(WeaponCategory) ||
                !objectField.FieldType.IsAssignableFrom(
                    typeof(BlueprintFeature)))
                throw new InvalidOperationException(
                    "Martial Performance Weapon Focus grant contract changed.");

            featureField.SetValue(row, weaponFocus);
            objectField.SetValue(row, spec.IsFirearm ?
                spec.FirearmParameter : null);
            categoryField.SetValue(row, spec.IsFirearm ?
                default(WeaponCategory) : spec.Category);
            schoolField.SetValue(row, Default(schoolField.FieldType));
            statField.SetValue(row, Default(statField.FieldType));
            Array replacement = Array.CreateInstance(rowType, 1);
            replacement.SetValue(row, 0);
            field.SetValue(grant, replacement);
        }

        private static void RetargetProficiency(BlueprintFeature feature,
            MartialPerformanceSpec spec)
        {
            BlueprintComponent[] components = feature.ComponentsArray;
            PrerequisiteProficiency native = components
                .OfType<PrerequisiteProficiency>().Single();
            BlueprintComponent replacement;
            if (spec.IsFirearm)
            {
                PrerequisiteFirearmProficiency firearm =
                    (spec.FirearmParameter.ComponentsArray ??
                        Array.Empty<BlueprintComponent>())
                    .OfType<PrerequisiteFirearmProficiency>().Single();
                replacement = (BlueprintComponent)UnityEngine.Object
                    .Instantiate(firearm);
            }
            else
            {
                var category = ScriptableObject.CreateInstance<
                    PrerequisiteCustomWeaponMartialPerformanceProficiency>();
                category.Category = spec.Category;
                category.KatanaGripDependent = spec.KatanaGripDependent;
                category.WeaponDisplayName = spec.DisplayName;
                replacement = category;
            }
            BlueprintComponent[] next =
                (BlueprintComponent[])components.Clone();
            next[Array.IndexOf(components, native)] = replacement;
            feature.ComponentsArray = next;
        }

        private static void ValidateChild(BlueprintFeature feature,
            BlueprintParametrizedFeature weaponFocus,
            MartialPerformanceSpec spec)
        {
            BlueprintComponent[] components = feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (components.Length != 2 ||
                components.OfType<AddParametrizedFeatures>().Count() != 1 ||
                components.OfType<Prerequisite>().Count() != 1)
                throw new InvalidOperationException(
                    "Custom Martial Performance child shape is not exact.");
            if (spec.IsFirearm)
            {
                PrerequisiteFirearmProficiency[] prerequisites = components
                    .OfType<PrerequisiteFirearmProficiency>().ToArray();
                if (prerequisites.Length != 1 ||
                    prerequisites[0].Kind != spec.FirearmKind)
                    throw new InvalidOperationException(
                        "Firearm Martial Performance proficiency is not exact.");
            }
            else
            {
                PrerequisiteCustomWeaponMartialPerformanceProficiency[]
                    prerequisites = components.OfType<
                        PrerequisiteCustomWeaponMartialPerformanceProficiency>()
                        .ToArray();
                if (prerequisites.Length != 1 ||
                    prerequisites[0].Category != spec.Category ||
                    prerequisites[0].KatanaGripDependent !=
                        spec.KatanaGripDependent)
                    throw new InvalidOperationException(
                        "Category Martial Performance proficiency is not exact.");
            }
            if (weaponFocus == null || !string.Equals(weaponFocus.AssetGuid,
                    WeaponFocusGuid, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Martial Performance no longer grants native Weapon Focus.");
        }

        private void Validate(MartialPerformanceSpec[] specs)
        {
            if (_selection.Features != _featuresBefore)
                throw new InvalidOperationException(
                    "Martial Performance Features was unexpectedly mutated.");
            BlueprintFeature[] all = _selection.AllFeatures ??
                Array.Empty<BlueprintFeature>();
            for (int index = 0; index < specs.Length; index++)
            {
                int count = all.Count(value => Same(value,
                    _registered[index]));
                if (count != (specs[index].Publish ? 1 : 0))
                    throw new InvalidOperationException(
                        "Martial Performance publication is not exact for " +
                        specs[index].DisplayName + ".");
            }
            string[] owned = _registered.Select(value => value.AssetGuid)
                .ToArray();
            BlueprintFeature[] foreignBefore = (_allFeaturesBefore ??
                Array.Empty<BlueprintFeature>()).Where(value =>
                    !owned.Contains(value.AssetGuid,
                        StringComparer.Ordinal)).ToArray();
            BlueprintFeature[] foreignAfter = all.Where(value =>
                !owned.Contains(value.AssetGuid,
                    StringComparer.Ordinal)).ToArray();
            if (!foreignBefore.SequenceEqual(foreignAfter))
                throw new InvalidOperationException(
                    "Martial Performance native or optional choices changed.");
        }

        private static FieldInfo RequireRowField(Type type, string name)
        {
            FieldInfo field = type.GetField(name, Fields);
            if (field == null)
                throw new InvalidOperationException(
                    "Martial Performance grant row lacks " + name + ".");
            return field;
        }

        private static object Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static bool Same(BlueprintFeature left,
            BlueprintFeature right)
        {
            return ReferenceEquals(left, right) ||
                left != null && right != null &&
                string.Equals(left.AssetGuid, right.AssetGuid,
                    StringComparison.Ordinal);
        }

        private sealed class MartialPerformanceSpec
        {
            private MartialPerformanceSpec(string symbol,
                string displayName, bool isFirearm, FirearmKind firearmKind,
                BlueprintFeature firearmParameter, WeaponCategory category,
                bool katanaGripDependent, bool publish)
            {
                Symbol = symbol;
                DisplayName = displayName;
                IsFirearm = isFirearm;
                FirearmKind = firearmKind;
                FirearmParameter = firearmParameter;
                Category = category;
                KatanaGripDependent = katanaGripDependent;
                Publish = publish;
            }

            internal string Symbol { get; private set; }
            internal string DisplayName { get; private set; }
            internal bool IsFirearm { get; private set; }
            internal FirearmKind FirearmKind { get; private set; }
            internal BlueprintFeature FirearmParameter { get; private set; }
            internal WeaponCategory Category { get; private set; }
            internal bool KatanaGripDependent { get; private set; }
            internal bool Publish { get; private set; }

            internal static MartialPerformanceSpec Firearm(string symbol,
                string displayName, FirearmKind kind,
                BlueprintFeature parameter, bool publish)
            {
                return new MartialPerformanceSpec(symbol, displayName, true,
                    kind, parameter, default(WeaponCategory), false, publish);
            }

            internal static MartialPerformanceSpec WeaponCategoryChoice(
                string symbol,
                string displayName, WeaponCategory category,
                bool katanaGripDependent, bool publish)
            {
                return new MartialPerformanceSpec(symbol, displayName, false,
                    default(FirearmKind), null, category,
                    katanaGripDependent, publish);
            }
        }
    }
}
