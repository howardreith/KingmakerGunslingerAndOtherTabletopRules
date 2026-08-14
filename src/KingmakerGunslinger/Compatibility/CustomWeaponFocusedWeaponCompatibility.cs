using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using UnityEngine;

namespace KingmakerGunslinger.Compatibility
{
    /// <summary>
    /// Publishes ordinary persistent children into Call of the Wild's Focused
    /// Weapon selection. The optional mod's own prerequisite and damage-die
    /// components remain authoritative; KMG does not patch selection methods.
    /// </summary>
    internal sealed class CustomWeaponFocusedWeaponPublication
    {
        internal const string SelectionGuid =
            "786bde5345a548408fade70b60a70482";
        internal const string WeaponFocusGuid =
            "1e1f627d26ad36f43bbd26cc2bf8ac7e";
        internal const string DamageComponentTypeName =
            "CallOfTheWild.NewMechanics.ContextWeaponDamageDiceReplacementForSpecificCategory";

        internal const string SpearGuid =
            "61280cc10efc55879c1491b9ead295a0";
        internal const string WakizashiGuid =
            "a0032cd381bc534e86d655a86a077276";
        internal const string KatanaGuid =
            "44dfba56c1f25b29bc48591753386e22";
        internal const string NodachiGuid =
            "8e121f3a48375f69ac9910b2d798b37b";

        internal const string SpearSymbol =
            "KMG.CustomWeapons.FocusedWeapon.ElvenBranchedSpear";
        internal const string WakizashiSymbol =
            "KMG.CustomWeapons.FocusedWeapon.Wakizashi";
        internal const string KatanaSymbol =
            "KMG.CustomWeapons.FocusedWeapon.Katana";
        internal const string NodachiSymbol =
            "KMG.CustomWeapons.FocusedWeapon.Nodachi";

        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const string ExpectedSelectionName =
            "FocusedWeaponAdvancedWeaponTrainingFeatureSelection";
        private const string ShortswordDonorGuid =
            "29a6081e7f4d41fdb9e5da830dd32522";
        private const string BastardSwordDonorGuid =
            "a13bcc2d98e4426cb017d4edfa05818c";
        private const string GreatswordDonorGuid =
            "70ecd8ffc4e64cce99eccaa2b509bf3d";
        private const string LongspearDonorGuid =
            "266e9d03ef6e4da6aa56b599f9a6aebc";
        private const string PolearmsTrainingGuid =
            "c062c6d16aecddc4ab67d9c783b2ad46";

        private readonly BlueprintFeatureSelection _selection;
        private readonly BlueprintFeature[] _allFeaturesBefore;
        private bool _rolledBack;

        private CustomWeaponFocusedWeaponPublication(
            BlueprintFeatureSelection selection)
        {
            _selection = selection;
            _allFeaturesBefore = selection == null ? null :
                selection.AllFeatures;
        }

        internal BlueprintFeature Spear { get; private set; }
        internal BlueprintFeature Wakizashi { get; private set; }
        internal BlueprintFeature Katana { get; private set; }
        internal BlueprintFeature Nodachi { get; private set; }
        internal bool OptionalModPresent { get { return _selection != null; } }

        internal static CustomWeaponFocusedWeaponPublication RegisterAndPublish(
            LibraryScriptableObject library, BlueprintRegistry registry,
            bool publishSpear, bool publishEastern)
        {
            if (library == null || registry == null)
                throw new ArgumentNullException(
                    "Focused Weapon registration inputs are incomplete.");

            BlueprintFeatureSelection selection = FindSelection(library);
            var publication = new CustomWeaponFocusedWeaponPublication(selection);
            try
            {
                publication.Spear = Register(registry, library, selection,
                    SpearSymbol, "Elven Branched Spear",
                    ElvenBranchedSpearCategoryRuntime.Category,
                    LongspearDonorGuid, null);
                publication.Wakizashi = Register(registry, library, selection,
                    WakizashiSymbol, "Wakizashi",
                    EasternWeaponCategoryRuntime.Category(
                        EasternWeaponFamily.Wakizashi),
                    ShortswordDonorGuid, null);
                publication.Katana = Register(registry, library, selection,
                    KatanaSymbol, "Katana",
                    EasternWeaponCategoryRuntime.Category(
                        EasternWeaponFamily.Katana),
                    BastardSwordDonorGuid, null);
                publication.Nodachi = Register(registry, library, selection,
                    NodachiSymbol, "Nodachi",
                    EasternWeaponCategoryRuntime.Category(
                        EasternWeaponFamily.Nodachi),
                    GreatswordDonorGuid, PolearmsTrainingGuid);

                if (selection != null)
                {
                    BlueprintFeature[] owned = publication.All;
                    BlueprintFeature[] next = (selection.AllFeatures ??
                        Array.Empty<BlueprintFeature>()).Where(value =>
                            !owned.Any(ownedFeature => Same(value, ownedFeature)))
                        .ToArray();
                    if (publishSpear)
                        next = next.Concat(new[] { publication.Spear }).ToArray();
                    if (publishEastern)
                        next = next.Concat(new[] { publication.Katana,
                            publication.Nodachi, publication.Wakizashi }).ToArray();
                    selection.AllFeatures = next;
                    publication.ValidatePublication(publishSpear,
                        publishEastern);
                }
                return publication;
            }
            catch
            {
                publication.Rollback();
                throw;
            }
        }

        internal BlueprintFeature[] All
        {
            get
            {
                return new[] { Spear, Wakizashi, Katana, Nodachi }
                    .Where(value => value != null).ToArray();
            }
        }

        internal void Rollback()
        {
            if (_rolledBack) return;
            if (_selection != null) _selection.AllFeatures = _allFeaturesBefore;
            _rolledBack = true;
        }

        private static BlueprintFeature Register(BlueprintRegistry registry,
            LibraryScriptableObject library,
            BlueprintFeatureSelection selection, string symbol,
            string categoryName, WeaponCategory category, string donorGuid,
            string additionalTrainingGuid)
        {
            return registry.Register<BlueprintFeature>(symbol, delegate
            {
                BlueprintFeature value;
                if (selection == null)
                {
                    value = ScriptableObject.CreateInstance<BlueprintFeature>();
                    value.name = "KMG_FocusedWeapon_" +
                        categoryName.Replace(" ", string.Empty);
                    value.Ranks = 1;
                    value.IsClassFeature = true;
                    value.ComponentsArray = Array.Empty<BlueprintComponent>();
                }
                else
                {
                    BlueprintFeature donor = BlueprintLibraryLookup
                        .RequireExact<BlueprintFeature>(library, donorGuid,
                            "Call of the Wild Focused Weapon child donor for " +
                                categoryName);
                    value = BlueprintCloneService.Clone(donor,
                        "KMG_FocusedWeapon_" +
                            categoryName.Replace(" ", string.Empty));
                    value.ComponentsArray = (donor.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(component =>
                            (BlueprintComponent)UnityEngine.Object.Instantiate(
                                component)).ToArray();
                    Retarget(value, library, category, additionalTrainingGuid);
                }
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(symbol + ".Name",
                        "Focused Weapon: " + categoryName),
                    LocalizationService.Create(symbol + ".Description",
                        selection == null ?
                            "A saved Call of the Wild Focused Weapon choice for " +
                                categoryName + "." : selection.Description),
                    selection == null ? null : selection.Icon);
                return value;
            });
        }

        private static void Retarget(BlueprintFeature feature,
            LibraryScriptableObject library, WeaponCategory category,
            string additionalTrainingGuid)
        {
            PrerequisiteParametrizedFeature[] focus = feature.ComponentsArray
                .OfType<PrerequisiteParametrizedFeature>().Where(value =>
                    value.Feature != null && string.Equals(
                        value.Feature.AssetGuid, WeaponFocusGuid,
                        StringComparison.Ordinal)).ToArray();
            if (focus.Length != 1)
                throw new InvalidOperationException(
                    "Focused Weapon donor has no singular Weapon Focus prerequisite.");
            focus[0].WeaponCategory = category;

            BlueprintComponent[] damage = feature.ComponentsArray.Where(value =>
                value != null && string.Equals(value.GetType().FullName,
                    DamageComponentTypeName, StringComparison.Ordinal)).ToArray();
            if (damage.Length != 1)
                throw new InvalidOperationException(
                    "Focused Weapon donor has no singular category damage component.");
            FieldInfo categoryField = damage[0].GetType().GetField("category",
                Fields);
            FieldInfo diceField = damage[0].GetType().GetField("dice_formulas",
                Fields);
            if (categoryField == null ||
                categoryField.FieldType != typeof(WeaponCategory) ||
                diceField == null || !(diceField.GetValue(damage[0]) is Array dice) ||
                dice.Length != 5)
                throw new InvalidOperationException(
                    "Call of the Wild Focused Weapon damage contract changed.");
            categoryField.SetValue(damage[0], category);

            if (string.IsNullOrEmpty(additionalTrainingGuid)) return;
            BlueprintFeature additional = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library, additionalTrainingGuid,
                    "Call of the Wild Polearms weapon training");
            PrerequisiteFeature[] training = feature.ComponentsArray
                .OfType<PrerequisiteFeature>().Where(value =>
                    value.Feature != null && value.Group == Prerequisite.GroupType.Any)
                .ToArray();
            if (training.Length == 0)
                throw new InvalidOperationException(
                    "Two-handed Focused Weapon donor has no Any-group training prerequisite.");
            var polearms = ScriptableObject.CreateInstance<PrerequisiteFeature>();
            polearms.Feature = additional;
            polearms.Group = Prerequisite.GroupType.Any;
            feature.ComponentsArray = feature.ComponentsArray.Concat(
                new BlueprintComponent[] { polearms }).ToArray();
        }

        private void ValidatePublication(bool publishSpear,
            bool publishEastern)
        {
            if (_selection == null) return;
            BlueprintFeature[] all = _selection.AllFeatures ??
                Array.Empty<BlueprintFeature>();
            if (Count(all, Spear) != (publishSpear ? 1 : 0) ||
                Count(all, Wakizashi) != (publishEastern ? 1 : 0) ||
                Count(all, Katana) != (publishEastern ? 1 : 0) ||
                Count(all, Nodachi) != (publishEastern ? 1 : 0))
                throw new InvalidOperationException(
                    "Focused Weapon custom-category publication is not exact.");
        }

        private static BlueprintFeatureSelection FindSelection(
            LibraryScriptableObject library)
        {
            BlueprintScriptableObject value;
            if (library.BlueprintsByAssetId == null ||
                !library.BlueprintsByAssetId.TryGetValue(SelectionGuid,
                    out value)) return null;
            var selection = value as BlueprintFeatureSelection;
            if (selection == null || !string.Equals(selection.name,
                    ExpectedSelectionName, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Call of the Wild Focused Weapon identity or type changed.");
            return selection;
        }

        private static int Count(IEnumerable<BlueprintFeature> source,
            BlueprintFeature expected)
        {
            return source.Count(value => Same(value, expected));
        }

        private static bool Same(BlueprintFeature left, BlueprintFeature right)
        {
            return ReferenceEquals(left, right) || left != null && right != null &&
                string.Equals(left.AssetGuid, right.AssetGuid,
                    StringComparison.Ordinal);
        }
    }
}
