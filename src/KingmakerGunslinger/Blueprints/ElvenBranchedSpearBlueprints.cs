using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElvenBranchedSpear;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ElvenBranchedSpearBlueprints
    {
        internal const string NativeLongspearTypeGuid =
            "fa2dd17cbde7d3f4aa918d467c30516e";
        internal const string NativeLongspearItemGuid =
            "f28f6031c2908d84d945865a80f67177";
        internal const string NativeMasterworkGuid =
            "6b38844e2bffbac48b63036b66e735be";
        internal const string NativeEnhancementOneGuid =
            "d42fc23b92c640846ac137dc26e000d4";
        internal const string NativeExoticWeaponProficiencySelectionGuid =
            "9a01b6815d6c3684cb25f30b8bf20932";
        internal const string NativeElvenCurveBladeProficiencyGuid =
            "0fca9259e370cd049a1dd50bede687f7";
        internal const string NativeFinesseTrainingSelectionGuid =
            "b78d146cea711a84598f0acef69462ea";
        internal const string NativeFinesseTrainingElvenCurveBladeGuid =
            "04f3b956e5a5cf649bce83774e0bfe4a";
        internal const string NativeElvenWeaponFamiliarityGuid =
            "03fd1e043fc678a4baf73fe67c3780ce";
        internal const string ExoticWeaponProficiencySymbol =
            "KMG.ElvenBranchedSpear.ExoticWeaponProficiency";
        internal const string FinesseTrainingSymbol =
            "KMG.ElvenBranchedSpear.FinesseTraining";
        internal const string MovementOpportunityAccuracySymbol =
            "KMG.ElvenBranchedSpear.MovementOpportunityAccuracy";

        private static readonly string[] ParameterSelectorGuids =
        {
            "1e1f627d26ad36f43bbd26cc2bf8ac7e", // Weapon Focus
            "09c9e82965fb4334b984a1e9df3bd088", // Greater Weapon Focus
            "f4201c85a991369408740c6888362e20", // Improved Critical
            "31470b17e8446ae4ea0dacd6c5817d86", // Weapon Specialization
            "7cf5edc65e785a24f9cf93af987d66b3", // Greater Weapon Specialization
            "c0b4ec0175e3ff940a45fc21f318a39a", // Sword Saint chosen weapon
            "38ae5ac04463a8947b7c06a6c72dd6bb"  // Weapon Mastery
        };

        internal static ElvenBranchedSpearBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            bool publishSelectors, ModLogger logger)
        {
            if (library == null || registry == null || logger == null)
                throw new ArgumentNullException("Spear registration inputs are incomplete.");
            WeaponCategory category = ElvenBranchedSpearCategoryRuntime.Category;
            BlueprintWeaponType collision = library.GetAllBlueprints()
                .OfType<BlueprintWeaponType>().FirstOrDefault(value =>
                    value != null && value.Category.Equals(category));
            if (collision != null)
                throw new InvalidOperationException("Elven Branched Spear category collision: " +
                    collision.name + ":" + collision.AssetGuid + ".");

            BlueprintWeaponType nativeType = BlueprintLibraryLookup.RequireExact<
                BlueprintWeaponType>(library, NativeLongspearTypeGuid,
                    "native Longspear weapon type");
            BlueprintItemWeapon nativeItem = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeLongspearItemGuid,
                    "native Standard Longspear item");
            BlueprintWeaponEnchantment masterwork = BlueprintLibraryLookup.RequireExact<
                BlueprintWeaponEnchantment>(library, NativeMasterworkGuid,
                    "native masterwork enchantment");
            BlueprintWeaponEnchantment plusOne = BlueprintLibraryLookup.RequireExact<
                BlueprintWeaponEnchantment>(library, NativeEnhancementOneGuid,
                    "native +1 weapon enchantment");
            WeaponBlueprintAccess typeAccess = WeaponBlueprintAccess.Resolve();
            if (!ReferenceEquals(typeAccess.Get(nativeItem), nativeType))
                throw new InvalidOperationException("Native Longspear item/type relation changed.");
            // Only serialized donor state is compared here (the category field
            // and, below, the full mechanical fingerprint including m_Weight):
            // a donor getter that another mod patches or that throws must not
            // decide whether the owned spear registers.
            WeaponCategory nativeCategory = nativeType.Category;

            var typeAdapter = new SpearWeaponTypeAccess();
            string nativeProfile = typeAdapter.MechanicalFingerprint(nativeType);
            var itemAdapter = new SpearItemAccess(nativeItem);
            var name = LocalizationService.Create(
                "KMG.ElvenBranchedSpear.Type.Name", "Elven Branched Spear");
            var description = LocalizationService.Create(
                "KMG.ElvenBranchedSpear.Type.Description",
                "This exotic two-handed reach spear can be used with Weapon Finesse. It grants a +2 bonus on attacks of opportunity provoked by movement.");
            BlueprintWeaponEnchantment movementAccuracy =
                registry.Register<BlueprintWeaponEnchantment>(
                    MovementOpportunityAccuracySymbol, delegate
                    {
                        BlueprintWeaponEnchantment value = ScriptableObject
                            .CreateInstance<BlueprintWeaponEnchantment>();
                        value.name = "KMG_ElvenBranchedSpear_MovementOpportunityAccuracy";
                        ConfigureEnchantmentText(value,
                            "Elven Branched Spear",
                            "This weapon grants a +2 bonus on attacks of opportunity provoked by movement.", 0);
                        MovementOpportunityAccuracyComponent component =
                            MovementOpportunityAccuracyComponent.Create();
                        component.name = "$KMG_ElvenBranchedSpear_MovementOpportunityAccuracy";
                        ElvenBranchedSpearProficiencyPenaltyComponent penalty =
                            ElvenBranchedSpearProficiencyPenaltyComponent.Create();
                        penalty.name = "$KMG_ElvenBranchedSpear_ProficiencyPenalty";
                        value.ComponentsArray = new BlueprintComponent[] {
                            component, penalty };
                        return value;
                    });
            BlueprintWeaponType weaponType = registry.Register<BlueprintWeaponType>(
                ElvenBranchedSpearCatalog.WeaponTypeSymbol, delegate
                {
                    BlueprintWeaponType clone = BlueprintCloneService.Clone(nativeType,
                        "KMG_ElvenBranchedSpear_WeaponType");
                    typeAdapter.Configure(clone, category, name, description,
                        movementAccuracy, nativeType);
                    Assets.ElvenBranchedSpearAssetRuntime.ApplyTo(clone);
                    return clone;
                });

            var entries = new List<ElvenBranchedSpearBlueprintEntry>();
            foreach (ElvenBranchedSpearItemSpec spec in ElvenBranchedSpearCatalog.All)
            {
                BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                    spec.Symbol, delegate
                    {
                        BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                            nativeItem, spec.InternalName);
                        typeAccess.Set(clone, weaponType);
                        BlueprintWeaponEnchantment[] enchantments = spec.Enhancement == 1
                            ? new[] { plusOne }
                            : spec.Masterwork ? new[] { masterwork }
                            : Array.Empty<BlueprintWeaponEnchantment>();
                        itemAdapter.Configure(clone, spec, enchantments);
                        Assets.ElvenBranchedSpearAssetRuntime.ApplyTo(clone,
                            spec.Symbol);
                        return clone;
                    });
                entries.Add(new ElvenBranchedSpearBlueprintEntry(spec, item));
            }

            BlueprintFeature ewp = registry.Register<BlueprintFeature>(
                ExoticWeaponProficiencySymbol, delegate
                {
                    BlueprintFeature donor = BlueprintLibraryLookup.RequireExact<
                        BlueprintFeature>(library,
                            NativeElvenCurveBladeProficiencyGuid,
                            "native Elven Curve Blade proficiency option");
                    BlueprintFeature clone = CloneFeatureWithComponents(donor,
                        "KMG_ExoticWeaponProficiency_ElvenBranchedSpear");
                    ConfigureProficiencyFeature(clone, category, donor.Icon);
                    return clone;
                });
            BlueprintFeature finesseTraining = registry.Register<BlueprintFeature>(
                FinesseTrainingSymbol, delegate
                {
                    BlueprintFeature donor = BlueprintLibraryLookup.RequireExact<
                        BlueprintFeature>(library,
                            NativeFinesseTrainingElvenCurveBladeGuid,
                            "native Elven Curve Blade Finesse Training option");
                    BlueprintFeature clone = CloneFeatureWithComponents(donor,
                        "KMG_FinesseTraining_ElvenBranchedSpear");
                    ConfigureFinesseTrainingFeature(clone, category, donor.Icon);
                    return clone;
                });

            BlueprintFeatureSelection ewpSelection = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library,
                    NativeExoticWeaponProficiencySelectionGuid,
                    "native Exotic Weapon Proficiency selection");
            BlueprintFeatureSelection finesseSelection = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library,
                    NativeFinesseTrainingSelectionGuid,
                    "native Rogue Finesse Training selection");
            BlueprintFeature familiarity = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeElvenWeaponFamiliarityGuid,
                    "native Elven Weapon Familiarity feature");
            bool ewpUsesCategoryIcon = NativeChildrenUseCategoryIcons(
                ewpSelection, ewp);
            bool finesseUsesCategoryIcon = NativeChildrenUseCategoryIcons(
                finesseSelection, finesseTraining);
            BlueprintParametrizedFeature[] parameterSelectors = ParameterSelectorGuids
                .Select(guid => BlueprintLibraryLookup.RequireExact<
                    BlueprintParametrizedFeature>(library, guid,
                        "native chosen-weapon selector")).ToArray();
            BlueprintFeature ewpOrderingAnchor = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library,
                    NativeElvenCurveBladeProficiencyGuid,
                    "native Elven Curve Blade proficiency ordering anchor");

            // Every fallible foundation check runs before any native selector,
            // familiarity or selector-runtime state is touched, so a failure
            // here leaves only registry-owned identities for the rollback.
            if (movementAccuracy.EnchantmentCost != 0 ||
                movementAccuracy.ComponentsArray == null ||
                movementAccuracy.ComponentsArray.Length != 2 ||
                movementAccuracy.ComponentsArray.OfType<
                    MovementOpportunityAccuracyComponent>().Count() != 1 ||
                movementAccuracy.ComponentsArray.OfType<
                    ElvenBranchedSpearProficiencyPenaltyComponent>().Count() != 1)
                throw new InvalidOperationException(
                    "Movement-opportunity accuracy enchantment is malformed.");
            ValidateFoundation(weaponType, entries.ToArray(), typeAccess,
                typeAdapter, itemAdapter, nativeType);
            if (!nativeType.Category.Equals(nativeCategory) ||
                !ReferenceEquals(typeAccess.Get(nativeItem), nativeType))
                throw new InvalidOperationException("Native Longspear donor was mutated.");
            string nativeProfileAfter = typeAdapter.MechanicalFingerprint(nativeType);
            if (!string.Equals(nativeProfile, nativeProfileAfter, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Native Longspear donor profile was mutated: before=" +
                    nativeProfile + " after=" + nativeProfileAfter + ".");

            // Native selector publication is deferred until the named spears
            // have also registered (AttachNamed). Publish rolls itself back if
            // it fails, and the bootstrap takes the handle immediately after.
            var result = new ElvenBranchedSpearBlueprintSet(weaponType,
                entries.ToArray(), ewp, finesseTraining,
                () => ElvenBranchedSpearSelectorPublication.Publish(ewpSelection,
                    ewp, ewpOrderingAnchor, finesseSelection, finesseTraining,
                    familiarity, category, parameterSelectors, publishSelectors),
                ewpUsesCategoryIcon, finesseUsesCategoryIcon,
                ewp.Icon, finesseTraining.Icon);
            logger.Info("elven-branched-spear", "foundation.ready",
                "Registered and validated one stable category and six foundation items; native selector options and Elf familiarity integration publish after named-spear registration; selector publication=" +
                publishSelectors + ".");
            return result;
        }

        internal static void ValidateFoundation(BlueprintWeaponType weaponType,
            ElvenBranchedSpearBlueprintEntry[] entries,
            WeaponBlueprintAccess typeAccess, SpearWeaponTypeAccess typeAdapter,
            SpearItemAccess itemAdapter, BlueprintWeaponType donor)
        {
            if (weaponType == null || entries == null || entries.Length != 6 ||
                entries.Select(value => value.Item).Distinct().Count() != 6)
                throw new InvalidOperationException("Spear foundation identity count is invalid.");
            typeAdapter.Validate(weaponType, donor);
            foreach (ElvenBranchedSpearBlueprintEntry entry in entries)
            {
                if (!ReferenceEquals(typeAccess.Get(entry.Item), weaponType))
                    throw new InvalidOperationException("A spear item left the shared category family.");
                itemAdapter.Validate(entry.Item, entry.Spec);
            }
        }

        private static BlueprintFeature CloneFeatureWithComponents(
            BlueprintFeature donor, string internalName)
        {
            BlueprintFeature clone = BlueprintCloneService.Clone(donor, internalName);
            clone.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(value =>
                    (BlueprintComponent)UnityEngine.Object.Instantiate(value)).ToArray();
            return clone;
        }

        private static void ConfigureEnchantmentText(
            BlueprintWeaponEnchantment enchantment, string name,
            string description, int cost)
        {
            const BindingFlags fields = BindingFlags.Instance |
                BindingFlags.NonPublic;
            Type owner = typeof(BlueprintItemEnchantment);
            FieldInfo nameField = owner.GetField("m_EnchantName", fields);
            FieldInfo descriptionField = owner.GetField("m_Description", fields);
            FieldInfo costField = owner.GetField("m_EnchantmentCost", fields);
            if (nameField == null || descriptionField == null || costField == null)
                throw new MissingFieldException(owner.FullName,
                    "m_EnchantName/m_Description/m_EnchantmentCost");
            nameField.SetValue(enchantment, LocalizationService.Create(
                MovementOpportunityAccuracySymbol + ".Name", name));
            descriptionField.SetValue(enchantment, LocalizationService.Create(
                MovementOpportunityAccuracySymbol + ".Description", description));
            costField.SetValue(enchantment, cost);
        }

        private static void ConfigureProficiencyFeature(BlueprintFeature feature,
            WeaponCategory category, Sprite nativeIcon)
        {
            AddProficiencies grant = feature.ComponentsArray.OfType<AddProficiencies>()
                .Single();
            grant.RaceRestriction = null;
            grant.ArmorProficiencies = Array.Empty<
                Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup>();
            grant.WeaponProficiencies = new[] { category };
            PrerequisiteNotProficient absent = feature.ComponentsArray
                .OfType<PrerequisiteNotProficient>().Single();
            absent.ArmorProficiencies = Array.Empty<
                Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup>();
            absent.WeaponProficiencies = new[] { category };
            AddStartingEquipment equipment = feature.ComponentsArray
                .OfType<AddStartingEquipment>().Single();
            equipment.BasicItems = Array.Empty<Kingmaker.Blueprints.Items.BlueprintItem>();
            equipment.CategoryItems = new[] { category };
            equipment.ParametrizedCategory = false;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.ElvenBranchedSpear.EWP.Name",
                    "Weapon Proficiency (Elven Branched Spear)"),
                LocalizationService.Create("KMG.ElvenBranchedSpear.EWP.Description",
                    "You are proficient with the exotic Elven Branched Spear."),
                nativeIcon);
        }

        private static void ConfigureFinesseTrainingFeature(BlueprintFeature feature,
            WeaponCategory category, Sprite nativeIcon)
        {
            WeaponTypeDamageStatReplacement replacement = feature.ComponentsArray
                .OfType<WeaponTypeDamageStatReplacement>().Single();
            replacement.Category = category;
            replacement.OnlyOneHanded = false;
                replacement.TwoHandedBonus = true;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.ElvenBranchedSpear.FinesseTraining.Name",
                    "Finesse Training (Elven Branched Spear)"),
                LocalizationService.Create(
                    "KMG.ElvenBranchedSpear.FinesseTraining.Description",
                    "Use Dexterity instead of Strength for damage rolls with Elven Branched Spears."),
                nativeIcon);
        }

        private static bool NativeChildrenUseCategoryIcons(
            BlueprintFeatureSelection selection, BlueprintFeature customFeature)
        {
            BlueprintFeature[] native = (selection.AllFeatures ??
                Array.Empty<BlueprintFeature>()).Where(value => value != null &&
                    !ReferenceEquals(value, customFeature)).ToArray();
            Sprite[] icons = native.Select(value => value.Icon)
                .Where(value => value != null).ToArray();
            if (customFeature.Icon == null || icons.Length == 0)
                return true;
            return icons.Distinct().Count() > 1;
        }
    }

    internal sealed class SpearWeaponTypeAccess
    {
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly FieldInfo _typeName = Require("m_TypeNameText");
        private readonly FieldInfo _defaultName = Require("m_DefaultNameText");
        private readonly FieldInfo _description = Require("m_DescriptionText");
        private readonly FieldInfo _masterworkDescription =
            Require("m_MasterworkDescriptionText");
        private readonly FieldInfo _magicDescription = Require("m_MagicDescriptionText");
        private readonly FieldInfo _weight = Require("m_Weight");
        private readonly FieldInfo _enchantments = Require("m_Enchantments");
        private readonly FieldInfo _attackType = Require("m_AttackType");
        private readonly FieldInfo _attackRange = Require("m_AttackRange");
        private readonly FieldInfo _baseDamage = Require("m_BaseDamage");
        private readonly FieldInfo _damageType = Require("m_DamageType");
        private readonly FieldInfo _criticalEdge = Require("m_CriticalRollEdge");
        private readonly FieldInfo _criticalModifier = Require("m_CriticalModifier");
        private readonly FieldInfo _fighterGroup = Require("m_FighterGroup");
        private readonly FieldInfo _isTwoHanded = Require("m_IsTwoHanded");
        private readonly FieldInfo _isLight = Require("m_IsLight");
        private readonly FieldInfo _isMonk = Require("m_IsMonk");
        private readonly FieldInfo _isNatural = Require("m_IsNatural");
        private readonly FieldInfo _isUnarmed = Require("m_IsUnarmed");
        private readonly FieldInfo _overrideAttackStat =
            Require("m_OverrideAttackBonusStat");

        /// <summary>
        /// Writes the spear's whole fixed mechanical profile onto the owned
        /// clone. The clone starts from the live native Longspear, which other
        /// mods may legitimately have changed; nothing mechanical is inherited
        /// from it and the donor itself is never written.
        /// </summary>
        internal void Configure(BlueprintWeaponType type, WeaponCategory category,
            Kingmaker.Localization.LocalizedString name,
            Kingmaker.Localization.LocalizedString description,
            BlueprintWeaponEnchantment movementAccuracy,
            BlueprintWeaponType donor)
        {
            if (movementAccuracy == null) throw new ArgumentNullException("movementAccuracy");
            _movementAccuracy = movementAccuracy;
            type.Category = category;
            _typeName.SetValue(type, name);
            _defaultName.SetValue(type, name);
            _description.SetValue(type, description);
            _masterworkDescription.SetValue(type, description);
            _magicDescription.SetValue(type, description);
            _attackType.SetValue(type, AttackType.Melee);
            _attackRange.SetValue(type, new Feet(
                ElvenBranchedSpearProfilePolicy.ReachFeet));
            _baseDamage.SetValue(type, IntendedDamage());
            _damageType.SetValue(type, SpearItemAccess.PhysicalPiercing(false));
            _criticalEdge.SetValue(type, ElvenBranchedSpearCatalog.CriticalThreatMinimum);
            _criticalModifier.SetValue(type, DamageCriticalModifierType.X3);
            _fighterGroup.SetValue(type, WeaponFighterGroup.Spears);
            _isTwoHanded.SetValue(type, true);
            _isLight.SetValue(type, false);
            _isMonk.SetValue(type, false);
            _isNatural.SetValue(type, false);
            _isUnarmed.SetValue(type, false);
            _overrideAttackStat.SetValue(type, false);
            _weight.SetValue(type, (float)ElvenBranchedSpearCatalog.WeightPounds);
            _enchantments.SetValue(type, new[] { movementAccuracy });
            // Inside the registry factory the clone still carries the donor's
            // asset guid, so getters patched for the native Longspear would
            // answer for it here. Only the owned serialized profile is checked
            // now; the effective (getter) profile is judged on the registered
            // type, after it has its released guid (ValidateFoundation).
            Validate(type, donor, false);
        }

        private const string MovementAccuracyName =
            "KMG_ElvenBranchedSpear_MovementOpportunityAccuracy";

        private static readonly WeaponSubCategory[] IntendedSubCategories = {
            WeaponSubCategory.Melee, WeaponSubCategory.Finessable,
            WeaponSubCategory.TwoHanded, WeaponSubCategory.Exotic,
            WeaponSubCategory.Metal };

        /// <summary>
        /// The definitive check, on the registered type: the owned serialized
        /// profile and the effective values players and the engine use - the
        /// live getters and the engine's sub-category calculation. Any
        /// owned-field mismatch, any effective value that disagrees with the
        /// engine contract for the owned field (whoever patches it) and any
        /// getter that throws fails validation with one message naming every
        /// failed predicate with its expected, actual and donor values and the
        /// implicated method's patch owners.
        /// </summary>
        internal void Validate(BlueprintWeaponType type, BlueprintWeaponType donor)
        {
            Validate(type, donor, true);
        }

        /// <summary>
        /// With <paramref name="effective"/> false only the owned serialized
        /// profile and the mod's own sub-category policy are judged; no getter
        /// or engine calculation is read.
        /// </summary>
        internal void Validate(BlueprintWeaponType type, BlueprintWeaponType donor,
            bool effective)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (donor == null) throw new ArgumentNullException("donor");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                Observe(type, donor, effective));
            if (!verdict.IsValid)
                throw new InvalidOperationException(verdict.DescribeFailure());
            if (effective) ReportRuntimeWeight(type);
        }

        /// <summary>
        /// One bounded, nonfatal diagnostic on the definitive validation: when
        /// the effective weight getter differs from the authored weight (a
        /// deliberate global override such as Bag of Tricks' No Item Weight),
        /// log both values and the getter's patch owners once. Every read is
        /// contained; nothing here can fail or change registration.
        /// </summary>
        private static void ReportRuntimeWeight(BlueprintWeaponType type)
        {
            try
            {
                float authored = AuthoredItemWeight.Stored(type);
                string effective;
                try { effective = Weight(type.Weight); }
                catch (Exception exception)
                {
                    effective = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
                }
                if (string.Equals(effective, Weight(authored), StringComparison.Ordinal))
                    return;
                string patchInfo;
                string[] owners = GetterPatchOwners("Weight", out patchInfo);
                ModContext context;
                if (!ModContext.TryGet(out context) || context.Logger == null) return;
                context.Logger.Info("elven-branched-spear", "weight.runtime-override",
                    "Elven Branched Spear weapon type authored weight=" +
                    Weight(authored) + " effective=" + effective +
                    "; the authored weight is valid and kept, the effective value is a runtime setting; getterPatches=" +
                    (string.Equals(patchInfo, "read", StringComparison.Ordinal)
                        ? (owners.Length == 0 ? "none" : string.Join(",",
                            owners.Distinct().OrderBy(value => value, StringComparer.Ordinal)
                                .Take(ElvenBranchedSpearProfilePolicy.MaxDescribedOwners).ToArray()))
                        : patchInfo) + ".");
            }
            catch (Exception)
            {
                // Diagnostic only: an unreadable runtime weight is not a defect.
            }
        }

        /// <summary>
        /// Every serialized mechanical field plus the identity of the shared
        /// reference-typed values, used to prove the native donor is unchanged.
        /// </summary>
        internal string MechanicalFingerprint(BlueprintWeaponType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            object damage = _damageType.GetValue(type);
            object enchantments = _enchantments.GetValue(type);
            return string.Join(";", new[] {
                Category(type.Category),
                Convert.ToString(_attackType.GetValue(type), CultureInfo.InvariantCulture),
                SerializedFeetText(SerializedFeet((Feet)_attackRange.GetValue(type))),
                Dice((DiceFormula)_baseDamage.GetValue(type)),
                Damage((DamageTypeDescription)damage),
                Identity(damage),
                Integer((int)_criticalEdge.GetValue(type)),
                Convert.ToString(_criticalModifier.GetValue(type), CultureInfo.InvariantCulture),
                Convert.ToString(_fighterGroup.GetValue(type), CultureInfo.InvariantCulture),
                Flag((bool)_isTwoHanded.GetValue(type)),
                Flag((bool)_isLight.GetValue(type)),
                Flag((bool)_isMonk.GetValue(type)),
                Flag((bool)_isNatural.GetValue(type)),
                Flag((bool)_isUnarmed.GetValue(type)),
                Flag((bool)_overrideAttackStat.GetValue(type)),
                Weight((float)_weight.GetValue(type)),
                Identity(enchantments),
                Identity(_typeName.GetValue(type)),
                Identity(_description.GetValue(type)) });
        }

        private static string Identity(object value)
        {
            return value == null ? "<null>" : value.GetType().Name + "#" +
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value)
                    .ToString("X8", CultureInfo.InvariantCulture);
        }

        private static DiceFormula IntendedDamage()
        {
            return new DiceFormula(ElvenBranchedSpearCatalog.DamageDieCount,
                (DiceType)ElvenBranchedSpearCatalog.DamageDieSides);
        }

        private bool _observeEffective;

        // The registered movement-accuracy enchantment written by Configure;
        // the enchantment checks compare entries with it by identity.
        private BlueprintWeaponEnchantment _movementAccuracy;

        private SpearProfileObservation[] Observe(BlueprintWeaponType type,
            BlueprintWeaponType donor, bool effective)
        {
            _observeEffective = effective;
            var values = new List<SpearProfileObservation>();
            string category = Category(ElvenBranchedSpearCategoryRuntime.Category);
            values.Add(new SpearProfileObservation("category", category,
                Category(type.Category), Category(donor.Category),
                category, Category(type.Category), Category(donor.Category),
                null, "field"));
            values.Add(Getter("attack-type", type, donor, _attackType,
                AttackType.Melee.ToString(), value => value.ToString(),
                "AttackType", value => value.AttackType.ToString(), null));
            values.Add(Getter("attack-range", type, donor, _attackRange,
                SerializedFeetText(ElvenBranchedSpearProfilePolicy.ReachFeet),
                value => SerializedFeetText(SerializedFeet((Feet)value)),
                // Full precision: Feet.Value truncates, so a 6.25 ft effective
                // reach must not read as the intended 6 ft.
                "AttackRange", value => SerializedFeetText(SerializedFeet(value.AttackRange)),
                field => SerializedFeetText(ElvenBranchedSpearProfilePolicy
                    .EngineAttackRangeFeet(SerializedFeet((Feet)field)))));
            values.Add(Getter("base-damage", type, donor, _baseDamage,
                Dice(IntendedDamage()), value => Dice((DiceFormula)value),
                "BaseDamage", value => Dice(value.BaseDamage), null));
            values.Add(Getter("damage-type", type, donor, _damageType,
                Damage(SpearItemAccess.PhysicalPiercing(false)),
                value => Damage((DamageTypeDescription)value),
                "DamageType", value => Damage(value.DamageType), null));
            values.Add(Getter("critical-edge", type, donor, _criticalEdge,
                Integer(ElvenBranchedSpearCatalog.CriticalThreatMinimum),
                value => Integer((int)value),
                "CriticalRollEdge", value => Integer(value.CriticalRollEdge), null));
            values.Add(Getter("critical-modifier", type, donor, _criticalModifier,
                DamageCriticalModifierType.X3.ToString(), value => value.ToString(),
                "CriticalModifier", value => value.CriticalModifier.ToString(), null));
            values.Add(Getter("fighter-group", type, donor, _fighterGroup,
                WeaponFighterGroup.Spears.ToString(), value => value.ToString(),
                "FighterGroup", value => value.FighterGroup.ToString(), null));
            values.Add(Getter("two-handed", type, donor, _isTwoHanded,
                Flag(true), value => Flag((bool)value),
                "IsTwoHanded", value => Flag(value.IsTwoHanded), null));
            values.Add(Getter("light", type, donor, _isLight,
                Flag(false), value => Flag((bool)value),
                "IsLight", value => Flag(value.IsLight), null));
            values.Add(Getter("monk", type, donor, _isMonk,
                Flag(false), value => Flag((bool)value),
                "IsMonk", value => Flag(value.IsMonk), null));
            values.Add(Getter("natural", type, donor, _isNatural,
                Flag(false), value => Flag((bool)value),
                "IsNatural", value => Flag(value.IsNatural), null));
            values.Add(Getter("unarmed", type, donor, _isUnarmed,
                Flag(false), value => Flag((bool)value),
                "IsUnarmed", value => Flag(value.IsUnarmed), null));
            values.Add(Getter("attack-stat-override", type, donor,
                _overrideAttackStat, Flag(false), value => Flag((bool)value),
                "AttackBonusStat", value => value.AttackBonusStat.ToString(),
                field => (bool)field ? "<override>" :
                    Kingmaker.EntitySystem.Stats.StatType.Strength.ToString()));
            values.Add(AuthoredWeight(type, donor));
            // The serialized m_Enchantments and the live Enchantments getter
            // must both hold exactly the registered movement-accuracy
            // enchantment (by identity: any other object, even one with the
            // same name, renders with its identity and so cannot match).
            values.Add(Getter("movement-enchantment", type, donor, _enchantments,
                MovementAccuracyName, EnchantmentText, "Enchantments",
                value => EnchantmentText(value.Enchantments), null));
            values.Add(SubCategories(type, donor));
            values.Add(SubCategoryListObservation(type, donor));
            return values.ToArray();
        }

        /// <summary>
        /// The weight predicate judges authored data only: the owned type's
        /// serialized m_Weight must be the catalog weight. The public Weight
        /// getter is a runtime value that a deliberate global override (Bag of
        /// Tricks' No Item Weight returns zero for every weapon type) may
        /// legitimately change, so it is not read here at all - it can be
        /// neither a mandatory input nor a source of exceptions - and the
        /// policy never treats an effective weight difference as a finding.
        /// The donor's stored field is reported as context only.
        /// </summary>
        private SpearProfileObservation AuthoredWeight(BlueprintWeaponType type,
            BlueprintWeaponType donor)
        {
            string owned = Weight(AuthoredItemWeight.Stored(type));
            string donorField;
            try { donorField = Weight(AuthoredItemWeight.Stored(donor)); }
            catch (Exception exception)
            {
                donorField = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
            }
            return new SpearProfileObservation("weight",
                Weight((float)ElvenBranchedSpearCatalog.WeightPounds), owned,
                donorField, owned, owned, donorField, null, "field");
        }

        private static readonly MethodInfo EngineHasSubCategory =
            typeof(WeaponCategoryExtension).GetMethod("HasSubCategory",
                BindingFlags.Static | BindingFlags.Public, null,
                new[] { typeof(WeaponCategory), typeof(WeaponSubCategory) }, null) ??
            throw new MissingMethodException(typeof(WeaponCategoryExtension).FullName,
                "HasSubCategory");

        /// <summary>
        /// The owned category's sub-categories across every WeaponSubCategory
        /// value: the mod's own policy must grant exactly the intended five
        /// plus the None wildcard, and the engine's
        /// WeaponCategoryExtension.HasSubCategory - what Weapon Finesse and
        /// every other native consumer asks, after all patches - must agree.
        /// </summary>
        private SpearProfileObservation SubCategories(BlueprintWeaponType type,
            BlueprintWeaponType donor)
        {
            string intended = string.Join(",", IntendedSubCategories
                .Select(value => value.ToString()).ToArray());
            string ownPolicy = SubCategoryList(ElvenBranchedSpearCategoryRuntime.HasSubCategory);
            if (!_observeEffective)
                return new SpearProfileObservation("subcategory-policy", intended,
                    ownPolicy, "<not-applicable>", ownPolicy,
                    ElvenBranchedSpearProfilePolicy.Deferred,
                    ElvenBranchedSpearProfilePolicy.Deferred, null, "deferred");
            WeaponCategory category = type.Category;
            string effective;
            try { effective = SubCategoryList(value => category.HasSubCategory(value)); }
            catch (Exception exception)
            {
                effective = ElvenBranchedSpearProfilePolicy.Threw(exception);
            }
            WeaponCategory donorCategory = donor.Category;
            string donorEffective;
            try { donorEffective = SubCategoryList(value => donorCategory.HasSubCategory(value)); }
            catch (Exception exception)
            {
                donorEffective = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
            }
            string patchInfo = "not-read";
            string[] owners = null;
            if (!string.Equals(ownPolicy, intended, StringComparison.Ordinal) ||
                !string.Equals(ownPolicy, effective, StringComparison.Ordinal))
                owners = PatchOwners(EngineHasSubCategory, out patchInfo);
            return new SpearProfileObservation("subcategory-policy", intended,
                ownPolicy, "<not-applicable>", ownPolicy, effective, donorEffective,
                owners, patchInfo);
        }

        /// <summary>
        /// An enchantment collection as comparable text: the registered
        /// movement-accuracy enchantment renders as its name, any other entry
        /// as its name plus object identity, so only that exact object can
        /// match the intended text.
        /// </summary>
        private string EnchantmentText(object value)
        {
            if (_movementAccuracy == null)
                throw new InvalidOperationException(
                    "The spear type profile was validated before configuration.");
            if (value == null) return "<null>";
            var collection = value as System.Collections.IEnumerable;
            if (collection == null) return "<not-a-collection " + value.GetType().Name + ">";
            var parts = new List<string>();
            foreach (object item in collection)
            {
                var blueprint = item as UnityEngine.Object;
                parts.Add(item == null ? "<null>" :
                    ReferenceEquals(item, _movementAccuracy) ? _movementAccuracy.name :
                    (blueprint != null ? blueprint.name : item.GetType().Name) + "@" + Identity(item));
            }
            return parts.Count == 0 ? "<none>" : string.Join(",", parts.ToArray());
        }

        private static readonly MethodInfo EngineGetSubCategories =
            typeof(WeaponCategoryExtension).GetMethod("GetSubCategories",
                BindingFlags.Static | BindingFlags.Public, null,
                new[] { typeof(WeaponCategory) }, null) ??
            throw new MissingMethodException(typeof(WeaponCategoryExtension).FullName,
                "GetSubCategories");

        /// <summary>
        /// The owned category's concrete sub-category list: the mod's own
        /// list must be exactly the intended five, and the engine's
        /// WeaponCategoryExtension.GetSubCategories - what enumerating callers
        /// receive, after all patches - must return the same list.
        /// </summary>
        private SpearProfileObservation SubCategoryListObservation(
            BlueprintWeaponType type, BlueprintWeaponType donor)
        {
            string intended = string.Join(",", IntendedSubCategories
                .Select(value => value.ToString()).ToArray());
            string ownList = ElvenBranchedSpearProfilePolicy.ReturnedSubCategoryText(
                IntendedSubCategories, ElvenBranchedSpearCategoryRuntime.GetSubCategories());
            if (!_observeEffective)
                return new SpearProfileObservation("subcategory-list", intended,
                    ownList, "<not-applicable>", ownList,
                    ElvenBranchedSpearProfilePolicy.Deferred,
                    ElvenBranchedSpearProfilePolicy.Deferred, null, "deferred");
            string effective;
            try
            {
                effective = ElvenBranchedSpearProfilePolicy.ReturnedSubCategoryText(
                    IntendedSubCategories, WeaponCategoryExtension.GetSubCategories(type.Category));
            }
            catch (Exception exception)
            {
                effective = ElvenBranchedSpearProfilePolicy.Threw(exception);
            }
            string donorEffective;
            try
            {
                donorEffective = ElvenBranchedSpearProfilePolicy.ReturnedSubCategoryText(
                    IntendedSubCategories, WeaponCategoryExtension.GetSubCategories(donor.Category));
            }
            catch (Exception exception)
            {
                donorEffective = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
            }
            string patchInfo = "not-read";
            string[] owners = null;
            if (!string.Equals(ownList, intended, StringComparison.Ordinal) ||
                !string.Equals(ownList, effective, StringComparison.Ordinal))
                owners = PatchOwners(EngineGetSubCategories, out patchInfo);
            return new SpearProfileObservation("subcategory-list", intended,
                ownList, "<not-applicable>", ownList, effective, donorEffective,
                owners, patchInfo);
        }

        /// <summary>Every WeaponSubCategory the engine defines.</summary>
        private static readonly WeaponSubCategory[] SupportedSubCategories =
            (WeaponSubCategory[])Enum.GetValues(typeof(WeaponSubCategory));

        /// <summary>
        /// Which sub-categories the given calculation grants, across every
        /// WeaponSubCategory the engine defines: the intended five in policy
        /// order, then any other granted value (Simple, Martial, Light, ...),
        /// and a marker when the None wildcard is denied. Any extra, missing or
        /// wildcard difference therefore changes the text.
        /// </summary>
        private static string SubCategoryList(Func<WeaponSubCategory, bool> grants)
        {
            return ElvenBranchedSpearProfilePolicy.SubCategoryText(
                IntendedSubCategories, SupportedSubCategories,
                WeaponSubCategory.None, grants);
        }

        /// <summary>
        /// Reads one serialized field and its public getter on the owned clone
        /// and the donor. A throwing owned getter is recorded as a failed
        /// value; a throwing donor getter only makes that comparison
        /// unavailable. Getter patch owners are read only when the predicate
        /// fails, so a healthy startup performs no Harmony queries.
        /// </summary>
        private SpearProfileObservation Getter(string predicate,
            BlueprintWeaponType type, BlueprintWeaponType donor, FieldInfo field,
            string expectedField, Func<object, string> fieldText, string property,
            Func<BlueprintWeaponType, string> getterText,
            Func<object, string> engineFromField)
        {
            object ownedValue = field.GetValue(type);
            string owned = fieldText(ownedValue);
            string engineExpected = engineFromField == null ? owned :
                engineFromField(ownedValue);
            if (!_observeEffective)
            {
                string donorSerialized;
                try { donorSerialized = fieldText(field.GetValue(donor)); }
                catch (Exception exception)
                {
                    donorSerialized = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
                }
                // No getter is read here; patch owners (Harmony metadata only)
                // are still reported for a failing owned field.
                string deferredPatchInfo = "not-read";
                string[] deferredOwners = null;
                if (!string.Equals(owned, expectedField, StringComparison.Ordinal))
                    deferredOwners = GetterPatchOwners(property, out deferredPatchInfo);
                return new SpearProfileObservation(predicate, expectedField, owned,
                    donorSerialized, engineExpected,
                    ElvenBranchedSpearProfilePolicy.Deferred,
                    ElvenBranchedSpearProfilePolicy.Deferred, deferredOwners,
                    deferredPatchInfo);
            }
            string engineActual;
            try { engineActual = getterText(type); }
            catch (Exception exception)
            {
                engineActual = ElvenBranchedSpearProfilePolicy.Threw(exception);
            }
            bool failed = !string.Equals(owned, expectedField, StringComparison.Ordinal) ||
                !string.Equals(engineExpected, engineActual, StringComparison.Ordinal);
            string patchInfo = "not-read";
            string[] owners = null;
            if (failed)
                owners = GetterPatchOwners(property, out patchInfo);
            string donorField;
            try { donorField = fieldText(field.GetValue(donor)); }
            catch (Exception exception)
            {
                donorField = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
            }
            string donorEngine;
            try { donorEngine = getterText(donor); }
            catch (Exception exception)
            {
                donorEngine = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
            }
            return new SpearProfileObservation(predicate, expectedField, owned,
                donorField, engineExpected, engineActual, donorEngine, owners,
                patchInfo);
        }

        private static string[] GetterPatchOwners(string property, out string patchInfo)
        {
            PropertyInfo info = typeof(BlueprintWeaponType).GetProperty(property,
                BindingFlags.Instance | BindingFlags.Public);
            return PatchOwners(info == null ? null : info.GetGetMethod(), out patchInfo);
        }

        internal static string[] PatchOwners(MethodInfo getter, out string patchInfo)
        {
            ModContext context;
            if (getter == null)
            {
                patchInfo = "unavailable:no-getter";
                return new string[0];
            }
            if (!ModContext.TryGet(out context) || context.Harmony == null)
            {
                patchInfo = "unavailable:no-harmony";
                return new string[0];
            }
            try
            {
                Harmony12.Patches patches = context.Harmony.GetPatchInfo(getter);
                patchInfo = "read";
                if (patches == null) return new string[0];
                return patches.Prefixes.Concat(patches.Postfixes)
                    .Concat(patches.Transpilers)
                    .Where(value => value != null)
                    .Select(value => value.owner).ToArray();
            }
            catch (Exception exception)
            {
                // The installed Harmony12 facade throws for an unpatched
                // method; record that rather than guessing an owner.
                patchInfo = "unavailable:" + exception.GetType().Name;
                return new string[0];
            }
        }

        private static readonly FieldInfo FeetValue = typeof(Feet).GetField(
            "m_Value", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new MissingFieldException(typeof(Feet).FullName, "m_Value");

        private static float SerializedFeet(Feet value)
        {
            return (float)FeetValue.GetValue(value);
        }

        private static string SerializedFeetText(float feet)
        {
            return feet.ToString("R", CultureInfo.InvariantCulture) + "ft";
        }

        private static string Category(WeaponCategory value)
        {
            return ((int)value).ToString("X8", CultureInfo.InvariantCulture) +
                ":" + value;
        }

        private static string FeetText(int feet)
        {
            return feet.ToString(CultureInfo.InvariantCulture) + "ft";
        }

        private static string Dice(DiceFormula value)
        {
            return value.Rolls.ToString(CultureInfo.InvariantCulture) + "x" +
                value.Dice;
        }

        private static string Damage(DamageTypeDescription value)
        {
            if (value == null) return "<null>";
            return value.Type + "/" + (value.Physical == null ? "<null>" :
                value.Physical.Form + "/" + value.Physical.Material);
        }

        private static string Integer(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Flag(bool value)
        {
            return value ? "true" : "false";
        }

        private static string Weight(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static FieldInfo Require(string name)
        {
            FieldInfo field = typeof(BlueprintWeaponType).GetField(name, Fields);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintWeaponType).FullName, name);
            return field;
        }
    }

    internal sealed class SpearItemAccess
    {
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly FieldInfo _enchantments = Require("m_Enchantments");
        private readonly FieldInfo _overrideDamageType = Require("m_OverrideDamageType");
        private readonly FieldInfo _damageType = Require("m_DamageType");
        private readonly FieldInfo _overrideDamageDice = Require("m_OverrideDamageDice");
        private readonly FieldInfo _damageDice = Require("m_DamageDice");

        private static DiceFormula FamilyDamage()
        {
            return new DiceFormula(ElvenBranchedSpearCatalog.DamageDieCount,
                (DiceType)ElvenBranchedSpearCatalog.DamageDieSides);
        }

        /// <summary>
        /// Owned spear items take their damage from the family weapon type:
        /// a damage-dice override the cloned native item carries (another mod
        /// may have given the Standard Longspear one) is cleared, never
        /// inherited. The donor item itself is not written.
        /// </summary>
        private void UseFamilyDamage(BlueprintItemWeapon item)
        {
            _overrideDamageDice.SetValue(item, false);
            _damageDice.SetValue(item, FamilyDamage());
        }

        // The native Standard Longspear item the owned items are cloned from:
        // comparison context for item-damage diagnostics only, never written.
        private readonly BlueprintItemWeapon _donor;

        internal SpearItemAccess() : this(null) { }

        internal SpearItemAccess(BlueprintItemWeapon donor) { _donor = donor; }

        private static readonly MethodInfo ItemBaseDamageGetter =
            typeof(BlueprintItemWeapon).GetProperty("BaseDamage",
                BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod();

        /// <summary>
        /// The item-damage predicate: the item's own dice override must be off
        /// and its effective BlueprintItemWeapon.BaseDamage must be the family
        /// 1d8. A failure throws one bounded diagnostic naming the item, the
        /// expected and actual values, the donor item's values (a donor read
        /// that throws only makes that comparison unavailable) and the
        /// BaseDamage getter's patch owners.
        /// </summary>
        private void ValidateDamage(BlueprintItemWeapon item, string displayName)
        {
            const string expectedOverride = "override=False";
            string ownedOverride = "override=" + (bool)_overrideDamageDice.GetValue(item);
            string donorOverride;
            try
            {
                donorOverride = _donor == null ? "<not-applicable>" :
                    "override=" + (bool)_overrideDamageDice.GetValue(_donor);
            }
            catch (Exception exception)
            {
                donorOverride = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
            }
            string expected = FamilyDamage().ToString();
            string actual;
            try { actual = item.BaseDamage.ToString(); }
            catch (Exception exception)
            {
                actual = ElvenBranchedSpearProfilePolicy.Threw(exception);
            }
            string donorActual;
            try { donorActual = _donor == null ? "<not-applicable>" : _donor.BaseDamage.ToString(); }
            catch (Exception exception)
            {
                donorActual = ElvenBranchedSpearProfilePolicy.Unavailable(exception);
            }
            string patchInfo = "not-read";
            string[] owners = null;
            if (!string.Equals(ownedOverride, expectedOverride, StringComparison.Ordinal) ||
                !string.Equals(actual, expected, StringComparison.Ordinal))
                owners = SpearWeaponTypeAccess.PatchOwners(ItemBaseDamageGetter, out patchInfo);
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(new[] {
                new SpearProfileObservation("item-damage", expectedOverride, ownedOverride,
                    donorOverride, expected, actual, donorActual, owners, patchInfo) });
            if (!verdict.IsValid)
                throw new InvalidOperationException(
                    ElvenBranchedSpearProfilePolicy.DescribeItemFailure(displayName, verdict));
        }

        internal void Configure(BlueprintItemWeapon item,
            ElvenBranchedSpearItemSpec spec,
            BlueprintWeaponEnchantment[] enchantments)
        {
            Configure(item, spec.Symbol, spec.DisplayName, spec.Cost,
                spec.ColdIron, enchantments);
        }

        /// <summary>
        /// Generic-item configuration shared by the catalog items and the
        /// higher-enhancement progression variants of the same family.
        /// </summary>
        internal void Configure(BlueprintItemWeapon item, string symbol,
            string displayName, int cost, bool coldIron,
            BlueprintWeaponEnchantment[] enchantments)
        {
            string description = Describe();
            BlueprintItemAccess.Resolve().ConfigureWeapon(item,
                LocalizationService.Create(symbol + ".Name", displayName),
                LocalizationService.Create(symbol + ".Description", description),
                LocalizationService.Create(symbol + ".Flavor",
                    "Forward-angled leaf blades give this elegant elven spear its unmistakable silhouette."),
                cost, ElvenBranchedSpearCatalog.WeightPounds);
            _enchantments.SetValue(item, enchantments == null
                ? Array.Empty<BlueprintWeaponEnchantment>()
                : enchantments.ToArray());
            _overrideDamageType.SetValue(item, coldIron);
            _damageType.SetValue(item, PhysicalPiercing(coldIron));
            UseFamilyDamage(item);
        }

        internal void Validate(BlueprintItemWeapon item,
            ElvenBranchedSpearItemSpec spec)
        {
            Validate(item, spec.DisplayName, spec.Cost, spec.Masterwork,
                spec.ColdIron, spec.Enhancement);
        }

        internal void Validate(BlueprintItemWeapon item, string displayName,
            int cost, bool masterwork, bool coldIron, int enhancement)
        {
            BlueprintWeaponEnchantment[] enchantments =
                (BlueprintWeaponEnchantment[])_enchantments.GetValue(item) ??
                Array.Empty<BlueprintWeaponEnchantment>();
            DamageTypeDescription damage = (DamageTypeDescription)_damageType.GetValue(item);
            ValidateDamage(item, displayName);
            // Authored weight only (see AuthoredItemWeight): the effective
            // getter may legitimately read zero under a global weight override.
            float storedWeight = AuthoredItemWeight.Stored(item);
            if (item.Cost != cost ||
                !storedWeight.Equals((float)ElvenBranchedSpearCatalog.WeightPounds) ||
                item.IsActuallyStackable || item.IsMasterwork !=
                    (masterwork && enhancement == 0) ||
                (bool)_overrideDamageType.GetValue(item) != coldIron ||
                damage == null || damage.Type != DamageType.Physical ||
                damage.Physical.Form != PhysicalDamageForm.Piercing ||
                damage.Physical.Material != (coldIron
                    ? PhysicalDamageMaterial.ColdIron : 0) ||
                enchantments.Length != (enhancement >= 1 || masterwork ? 1 : 0) ||
                item.Description.IndexOf("Brace", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException("Spear item contract is invalid: " +
                    displayName + ";cost=" + item.Cost + ";storedWeight=" +
                    AuthoredItemWeight.Text(storedWeight) + ";stackable=" + item.IsActuallyStackable +
                    ";nativeMasterworkFlag=" + item.IsMasterwork +
                    ";overrideDamage=" + _overrideDamageType.GetValue(item) +
                    ";damage=" + (damage == null ? "<null>" :
                        damage.Type + "/" + damage.Physical.Form + "/" +
                        damage.Physical.Material) + ";enchantments=" +
                    enchantments.Length + ".");
        }

        internal void ConfigureNamed(BlueprintItemWeapon item,
            NamedSpearSpec spec, BlueprintWeaponEnchantment[] enchantments,
            string description)
        {
            BlueprintItemAccess.Resolve().ConfigureWeapon(item,
                LocalizationService.Create(spec.Symbol + ".Name", spec.DisplayName),
                LocalizationService.Create(spec.Symbol + ".Description",
                    description),
                LocalizationService.Create(spec.Symbol + ".Flavor",
                    "An elven polearm whose forward-angled branch blades carry a distinctive magical finish."),
                spec.Cost, ElvenBranchedSpearCatalog.WeightPounds);
            _enchantments.SetValue(item, enchantments == null
                ? Array.Empty<BlueprintWeaponEnchantment>()
                : enchantments.ToArray());
            _overrideDamageType.SetValue(item, spec.ColdIron);
            _damageType.SetValue(item, PhysicalPiercing(spec.ColdIron));
            UseFamilyDamage(item);
        }

        internal void ValidateNamed(BlueprintItemWeapon item,
            NamedSpearSpec spec, int expectedEnchantments)
        {
            BlueprintWeaponEnchantment[] enchantments =
                (BlueprintWeaponEnchantment[])_enchantments.GetValue(item) ??
                Array.Empty<BlueprintWeaponEnchantment>();
            DamageTypeDescription damage =
                (DamageTypeDescription)_damageType.GetValue(item);
            ValidateDamage(item, spec.DisplayName);
            if (item.Cost != spec.Cost ||
                !AuthoredItemWeight.StoredEquals(item,
                    (float)ElvenBranchedSpearCatalog.WeightPounds) ||
                item.IsActuallyStackable ||
                (bool)_overrideDamageType.GetValue(item) != spec.ColdIron ||
                damage == null || damage.Type != DamageType.Physical ||
                damage.Physical.Form != PhysicalDamageForm.Piercing ||
                damage.Physical.Material != (spec.ColdIron
                    ? PhysicalDamageMaterial.ColdIron : 0) ||
                enchantments.Length != expectedEnchantments ||
                enchantments.Any(value => value == null) ||
                item.Description.IndexOf("Brace",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException(
                    "Named spear item contract is invalid: " +
                    spec.DisplayName + ".");
        }

        private static string Describe()
        {
            return "Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement.";
        }

        internal static DamageTypeDescription PhysicalPiercing(bool coldIron)
        {
            return new DamageTypeDescription
            {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData
                {
                    Form = PhysicalDamageForm.Piercing,
                    Material = coldIron ? PhysicalDamageMaterial.ColdIron : 0
                }
            };
        }

        private static FieldInfo Require(string name)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField(name, Fields);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintItemWeapon).FullName, name);
            return field;
        }
    }

    internal sealed class ElvenBranchedSpearBlueprintEntry
    {
        internal ElvenBranchedSpearBlueprintEntry(ElvenBranchedSpearItemSpec spec,
            BlueprintItemWeapon item)
        { Spec = spec; Item = item; }
        internal ElvenBranchedSpearItemSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
    }

    internal sealed class ElvenBranchedSpearBlueprintSet
    {
        internal ElvenBranchedSpearBlueprintSet(BlueprintWeaponType weaponType,
            ElvenBranchedSpearBlueprintEntry[] entries,
            BlueprintFeature exoticWeaponProficiency,
            BlueprintFeature finesseTraining,
            Func<ElvenBranchedSpearSelectorPublication> publish,
            bool exoticWeaponProficiencyUsesCategoryIcon,
            bool finesseTrainingUsesCategoryIcon,
            Sprite exoticWeaponProficiencyNativeIcon,
            Sprite finesseTrainingNativeIcon)
        {
            WeaponType = weaponType;
            Entries = entries;
            ExoticWeaponProficiency = exoticWeaponProficiency;
            FinesseTraining = finesseTraining;
            _publish = publish ?? throw new ArgumentNullException("publish");
            ExoticWeaponProficiencyUsesCategoryIcon =
                exoticWeaponProficiencyUsesCategoryIcon;
            FinesseTrainingUsesCategoryIcon = finesseTrainingUsesCategoryIcon;
            ExoticWeaponProficiencyNativeIcon = exoticWeaponProficiencyNativeIcon;
            FinesseTrainingNativeIcon = finesseTrainingNativeIcon;
        }
        internal BlueprintWeaponType WeaponType { get; private set; }
        internal ElvenBranchedSpearBlueprintEntry[] Entries { get; private set; }
        internal BlueprintFeature ExoticWeaponProficiency { get; private set; }
        internal BlueprintFeature FinesseTraining { get; private set; }
        internal bool ExoticWeaponProficiencyUsesCategoryIcon { get; private set; }
        internal bool FinesseTrainingUsesCategoryIcon { get; private set; }
        internal Sprite ExoticWeaponProficiencyNativeIcon { get; private set; }
        internal Sprite FinesseTrainingNativeIcon { get; private set; }
        private readonly Func<ElvenBranchedSpearSelectorPublication> _publish;
        internal ElvenBranchedSpearSelectorPublication Publication { get; private set; }
        internal ElvenBranchedSpearNamedBlueprintSet Named { get; private set; }

        /// <summary>
        /// Attaches the named spears and only then publishes the native
        /// selector, familiarity and selector-runtime changes, so a failure
        /// anywhere in foundation or named registration leaves native state
        /// untouched. Publish restores its own changes if it fails; on success
        /// the caller takes <see cref="Publication"/> for its rollback.
        /// </summary>
        internal void AttachNamed(ElvenBranchedSpearNamedBlueprintSet named)
        {
            if (named == null || Named != null)
                throw new InvalidOperationException(
                    "Named spear registration may be attached exactly once.");
            ElvenBranchedSpearSelectorPublication publication = _publish();
            if (publication == null)
                throw new InvalidOperationException(
                    "Spear selector publication returned no rollback handle.");
            Named = named;
            Publication = publication;
        }
        internal ElvenBranchedSpearBlueprintEntry Require(
            ElvenBranchedSpearItemKind kind)
        { return Entries.Single(value => value.Spec.Kind == kind); }
    }
}
