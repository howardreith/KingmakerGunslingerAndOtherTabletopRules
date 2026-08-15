using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using KingmakerGunslinger.Feats;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class FirearmFeatBlueprintSet
    {
        internal FirearmFeatBlueprintSet(BlueprintFeatureSelection weaponFocus,
            BlueprintFeature[] weaponFocusChoices,
            BlueprintFeatureSelection nativeWeaponFocusWithFirearms,
            BlueprintFeatureSelection rapidReload,
            BlueprintFeature[] rapidReloadChoices,
            BlueprintFeature exoticWeaponProficiency,
            BlueprintFeatureSelection[] dependentSelections,
            BlueprintFeature[][] dependentChoices)
        {
            WeaponFocus = weaponFocus;
            WeaponFocusChoices = weaponFocusChoices;
            NativeWeaponFocusWithFirearms = nativeWeaponFocusWithFirearms;
            RapidReload = rapidReload;
            RapidReloadChoices = rapidReloadChoices;
            ExoticWeaponProficiency = exoticWeaponProficiency;
            DependentSelections = dependentSelections;
            DependentChoices = dependentChoices;
        }
        internal BlueprintFeatureSelection WeaponFocus { get; private set; }
        internal BlueprintFeature[] WeaponFocusChoices { get; private set; }
        internal BlueprintFeatureSelection NativeWeaponFocusWithFirearms { get; private set; }
        internal BlueprintFeatureSelection RapidReload { get; private set; }
        internal BlueprintFeature[] RapidReloadChoices { get; private set; }
        internal BlueprintFeature ExoticWeaponProficiency { get; private set; }
        internal BlueprintFeatureSelection[] DependentSelections { get; private set; }
        internal BlueprintFeature[][] DependentChoices { get; private set; }
    }

    internal static class FirearmFeatBlueprints
    {
        private const string BasicFeatSelectionGuid = "247a4068296e8be42890143f451b4b45";
        private const string FighterFeatSelectionGuid = "41c8486641f7d6d4283ca9dae4147a9f";
        private const string NativeWeaponFocusGuid = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
        private static readonly string[] NativeDependentGuids = {
            "09c9e82965fb4334b984a1e9df3bd088",
            "31470b17e8446ae4ea0dacd6c5817d86",
            "7cf5edc65e785a24f9cf93af987d66b3",
            "f4201c85a991369408740c6888362e20" };
        private static readonly string[] DependentNames = { "Greater Weapon Focus",
            "Weapon Specialization", "Greater Weapon Specialization", "Improved Critical" };
        private static readonly string[] DependentSymbolStems = { "GreaterWeaponFocus",
            "WeaponSpecialization", "GreaterWeaponSpecialization", "ImprovedCritical" };
        internal const string WeaponFocusSelectionSymbol = "KMG.Feats.FirearmWeaponFocus";
        internal const string NativeWeaponFocusWrapperSymbol =
            "KMG.Feats.NativeWeaponFocusWithFirearms";
        internal const string RapidReloadSelectionSymbol = "KMG.Feats.RapidReload";
        internal const string ExoticWeaponProficiencySymbol =
            "KMG.Feats.ExoticWeaponProficiencyFirearms";
        internal static readonly FirearmKind[] Kinds = { FirearmKind.Pistol,
            FirearmKind.Musket, FirearmKind.Blunderbuss, FirearmKind.Rifle,
            FirearmKind.Revolver };
        internal static readonly string[] WeaponFocusSymbols = {
            "KMG.Feats.WeaponFocusPistol", "KMG.Feats.WeaponFocusMusket",
            "KMG.Feats.WeaponFocusBlunderbuss", "KMG.Feats.WeaponFocusRifle",
            "KMG.Feats.WeaponFocusRevolver" };
        internal static readonly string[] RapidReloadSymbols = {
            "KMG.Feats.RapidReloadPistol", "KMG.Feats.RapidReloadMusket",
            "KMG.Feats.RapidReloadBlunderbuss", "KMG.Feats.RapidReloadRifle",
            "KMG.Feats.RapidReloadRevolver" };

        internal static FirearmFeatBlueprintSet Register(LibraryScriptableObject library,
            BlueprintRegistry registry,
            BlueprintFeature firearmProficiency,
            FirearmScopedProficiencyBlueprintSet scopedProficiencies,
            bool publishParameters = true)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            if (firearmProficiency == null) throw new ArgumentNullException("firearmProficiency");
            if (scopedProficiencies == null)
                throw new ArgumentNullException("scopedProficiencies");
            BlueprintParametrizedFeature nativeWeaponFocus = BlueprintLibraryLookup.RequireExact<
                BlueprintParametrizedFeature>(library, NativeWeaponFocusGuid, "native Weapon Focus");
            var focus = new BlueprintFeature[Kinds.Length];
            var rapid = new BlueprintFeature[Kinds.Length];
            for (int i = 0; i < Kinds.Length; i++)
            {
                FirearmKind kind = Kinds[i];
                focus[i] = registry.Register<BlueprintFeature>(WeaponFocusSymbols[i],
                    () => CreateChoice(kind, firearmProficiency,
                        scopedProficiencies, false));
                rapid[i] = registry.Register<BlueprintFeature>(RapidReloadSymbols[i],
                    () => CreateChoice(kind, firearmProficiency,
                        scopedProficiencies, true));
            }
            BlueprintFeatureSelection focusSelection = registry.Register<BlueprintFeatureSelection>(
                WeaponFocusSelectionSymbol, () => CreateSelection("Firearm Weapon Focus",
                    "Select a firearm type. Attacks with that firearm gain a +1 bonus.", focus));
            focusSelection.HideInUI = true;
            var wrapperChoices = new BlueprintFeature[focus.Length + 1];
            wrapperChoices[0] = nativeWeaponFocus;
            Array.Copy(focus, 0, wrapperChoices, 1, focus.Length);
            BlueprintFeatureSelection wrapper = registry.Register<BlueprintFeatureSelection>(
                NativeWeaponFocusWrapperSymbol, () => CreateSelection("Weapon Focus",
                    "Choose a native weapon category or a firearm type. You gain +1 attack with only the selected weapon.",
                    wrapperChoices));
            wrapper.HideInUI = true;
            BlueprintFeatureSelection rapidSelection = registry.Register<BlueprintFeatureSelection>(
                RapidReloadSelectionSymbol, () => CreateSelection("Rapid Reload",
                    "Select a firearm type. Reloading that firearm uses the reduced action listed in its description.", rapid));
            var dependentSelections = new BlueprintFeatureSelection[DependentNames.Length];
            var dependentChoices = new BlueprintFeature[DependentNames.Length][];
            for (int family = 0; family < DependentNames.Length; family++)
            {
                BlueprintParametrizedFeature native = BlueprintLibraryLookup.RequireExact<
                    BlueprintParametrizedFeature>(library, NativeDependentGuids[family],
                        "native " + DependentNames[family]);
                dependentChoices[family] = new BlueprintFeature[Kinds.Length];
                for (int kindIndex = 0; kindIndex < Kinds.Length; kindIndex++)
                {
                    int capturedFamily = family, capturedKind = kindIndex;
                    string symbol = "KMG.Feats." + DependentSymbolStems[family] + Kinds[kindIndex];
                    dependentChoices[family][kindIndex] = registry.Register<BlueprintFeature>(
                        symbol, () => CreateDependentChoice(capturedFamily,
                            Kinds[capturedKind], native, focus[capturedKind],
                            capturedFamily == 2 ? dependentChoices[1][capturedKind] : null,
                            firearmProficiency, scopedProficiencies));
                }
                var choices = new BlueprintFeature[Kinds.Length + 1];
                choices[0] = native;
                Array.Copy(dependentChoices[family], 0, choices, 1, Kinds.Length);
                int captured = family;
                dependentSelections[family] = registry.Register<BlueprintFeatureSelection>(
                    "KMG.Feats." + DependentSymbolStems[family] + "WithFirearms",
                    () => CreateSelection(DependentNames[captured],
                        "Choose a native weapon category or a firearm type.", choices));
                dependentSelections[family].HideInUI = true;
            }
            NativeFirearmFeatIntegration.Configure(nativeWeaponFocus,
                NativeDependentGuids.Select((guid, index) =>
                    BlueprintLibraryLookup.RequireExact<BlueprintParametrizedFeature>(
                        library, guid, "native " + DependentNames[index])).ToArray(),
                focus, dependentChoices, firearmProficiency,
                scopedProficiencies.OneHanded, scopedProficiencies.TwoHanded,
                publishParameters);
            RapidReloadRuntime.Configure(Kinds, rapid);
            BlueprintFeature exoticWeaponProficiency =
                registry.Register<BlueprintFeature>(
                    ExoticWeaponProficiencySymbol,
                    () => CreateExoticWeaponProficiency(firearmProficiency));
            return new FirearmFeatBlueprintSet(focusSelection, focus, wrapper,
                rapidSelection, rapid, exoticWeaponProficiency,
                dependentSelections, dependentChoices);
        }

        internal static FirearmFeatCatalogPublication Publish(
            LibraryScriptableObject library, FirearmFeatBlueprintSet set)
        {
            if (library == null || set == null) throw new ArgumentNullException();
            BlueprintFeatureSelection basic = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library, BasicFeatSelectionGuid,
                    "native basic feat selection");
            BlueprintFeatureSelection fighter = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library, FighterFeatSelectionGuid,
                    "native Fighter combat feat selection");
            BlueprintFeatureSelection[] selections = library.GetAllBlueprints()
                .OfType<BlueprintFeatureSelection>().ToArray();
            var publication = new FirearmFeatCatalogPublication(basic, fighter,
                selections);
            // Rapid Reload is the only project-owned top-level firearm feat.
            // The legacy Exotic Weapon Proficiency (Firearms) blueprint stays
            // registered for existing owners, but is removed from every live
            // selection catalog. Firearm parameters remain inside the five
            // native parametrized feat menus.
            publication.Publish(set.RapidReload,
                set.ExoticWeaponProficiency);
            return publication;
        }

        private static BlueprintFeature CreateChoice(FirearmKind kind,
            BlueprintFeature proficiency,
            FirearmScopedProficiencyBlueprintSet scopedProficiencies,
            bool rapid)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + (rapid ? "RapidReload_" : "WeaponFocus_") + kind;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.Groups = new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            PrerequisiteFirearmProficiency proficiencyPrerequisite =
                CreateProficiencyPrerequisite(kind, proficiency,
                    scopedProficiencies);
            if (rapid)
                feature.ComponentsArray = new BlueprintComponent[] { proficiencyPrerequisite };
            else
            {
                var bab = ScriptableObject.CreateInstance<PrerequisiteStatValue>();
                bab.Stat = StatType.BaseAttackBonus;
                bab.Value = 1;
                var bonus = ScriptableObject.CreateInstance<FirearmWeaponFocusAttackBonus>();
                bonus.Kind = kind;
                feature.ComponentsArray = new BlueprintComponent[] {
                    proficiencyPrerequisite, bab, bonus };
            }
            string effect = rapid ? ReloadDescription(kind) :
                "Gain a +1 bonus on attack rolls made with " + kind + " firearms.";
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Feats." + (rapid ? "RapidReload" : "WeaponFocus") + kind + ".Name",
                    (rapid ? "Rapid Reload (" : "Weapon Focus (") + kind + ")"),
                LocalizationService.Create("KMG.Feats." + (rapid ? "RapidReload" : "WeaponFocus") + kind + ".Description", effect),
                null);
            return feature;
        }

        private static BlueprintFeature CreateDependentChoice(int family,
            FirearmKind kind, BlueprintParametrizedFeature native,
            BlueprintFeature weaponFocus, BlueprintFeature specialization,
            BlueprintFeature fullProficiency,
            FirearmScopedProficiencyBlueprintSet scopedProficiencies)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + DependentSymbolStems[family] + "_" + kind;
            feature.Ranks = 1;
            feature.Groups = new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            var components = native.ComponentsArray
                .Where(value => value is PrerequisiteStatValue ||
                    value is PrerequisiteClassLevel)
                .Select(value => (BlueprintComponent)UnityEngine.Object.Instantiate(value))
                .ToList();
            components.Add(CreateProficiencyPrerequisite(kind,
                fullProficiency, scopedProficiencies));
            if (family != 3)
            {
                var prerequisite = ScriptableObject.CreateInstance<PrerequisiteFeature>();
                prerequisite.Feature = weaponFocus;
                components.Add(prerequisite);
                if (family == 2)
                {
                    var specializationPrerequisite =
                        ScriptableObject.CreateInstance<PrerequisiteFeature>();
                    specializationPrerequisite.Feature = specialization;
                    components.Add(specializationPrerequisite);
                }
            }
            var effect = ScriptableObject.CreateInstance<FirearmWeaponFeatBonus>();
            effect.Kind = kind;
            effect.Effect = family == 0 ? FirearmWeaponFeatEffect.Attack :
                family == 3 ? FirearmWeaponFeatEffect.DoubleCriticalEdge :
                FirearmWeaponFeatEffect.Damage;
            effect.Bonus = family == 0 ? 1 : family == 3 ? 0 : 2;
            components.Add(effect);
            feature.ComponentsArray = components.ToArray();
            string name = DependentNames[family] + " (" + kind + ")";
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Feats." + DependentSymbolStems[family] + kind + ".Name", name),
                LocalizationService.Create("KMG.Feats." + DependentSymbolStems[family] + kind + ".Description",
                    "Gain the " + DependentNames[family] + " benefit with " + kind + " firearms only."), null);
            return feature;
        }

        private static PrerequisiteFirearmProficiency
            CreateProficiencyPrerequisite(FirearmKind kind,
                BlueprintFeature fullProficiency,
                FirearmScopedProficiencyBlueprintSet scopedProficiencies)
        {
            var prerequisite = ScriptableObject.CreateInstance<
                PrerequisiteFirearmProficiency>();
            prerequisite.FullProficiency = fullProficiency;
            prerequisite.OneHandedProficiency = scopedProficiencies.OneHanded;
            prerequisite.TwoHandedProficiency = scopedProficiencies.TwoHanded;
            prerequisite.Kind = kind;
            return prerequisite;
        }

        private static BlueprintFeature CreateExoticWeaponProficiency(
            BlueprintFeature fullProficiency)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_ExoticWeaponProficiency_Firearms";
            feature.Ranks = 1;
            feature.HideInUI = false;
            // Compatibility-only: retain a readable character-sheet fact and
            // its historical AddFacts grant, but do not advertise it through
            // FeatureGroup scans used by compatibility feat catalogs.
            feature.Groups = Array.Empty<FeatureGroup>();
            var bab = ScriptableObject.CreateInstance<PrerequisiteStatValue>();
            bab.Stat = StatType.BaseAttackBonus;
            bab.Value = 1;
            var absent = ScriptableObject.CreateInstance<PrerequisiteNoFeature>();
            absent.Feature = fullProficiency;
            var grant = ScriptableObject.CreateInstance<Kingmaker.UnitLogic.FactLogic.AddFacts>();
            grant.Facts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] {
                fullProficiency };
            grant.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { bab, absent, grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Feats.EWPFirearms.Name",
                    "Exotic Weapon Proficiency (Firearms)"),
                LocalizationService.Create("KMG.Feats.EWPFirearms.Description",
                    "You are proficient with all firearms, including firearm families outside an archetype's normal proficiency."),
                null);
            return feature;
        }

        private static BlueprintFeatureSelection CreateSelection(string name,
            string description, BlueprintFeature[] choices)
        {
            var selection = ScriptableObject.CreateInstance<BlueprintFeatureSelection>();
            selection.name = "KMG_" + name.Replace(" ", "_");
            selection.Ranks = 1;
            selection.HideInUI = false;
            selection.IgnorePrerequisites = false;
            selection.Obligatory = false;
            selection.Group = FeatureGroup.Feat;
            selection.Group2 = FeatureGroup.CombatFeat;
            selection.Features = (BlueprintFeature[])choices.Clone();
            selection.AllFeatures = (BlueprintFeature[])choices.Clone();
            BlueprintUnitFactAccess.Resolve().Configure(selection,
                LocalizationService.Create("KMG.Feats." + name + ".Name", name),
                LocalizationService.Create("KMG.Feats." + name + ".Description", description),
                null);
            return selection;
        }

        private static string ReloadDescription(FirearmKind kind)
        {
            return kind == FirearmKind.Pistol
                ? "Reload this early one-handed firearm as a move action."
                : kind == FirearmKind.Musket || kind == FirearmKind.Blunderbuss
                    ? "Reload this early two-handed firearm as a standard action."
                    : "Reload this advanced firearm as a free action.";
        }
    }

    internal sealed class FirearmFeatCatalogPublication
    {
        private sealed class SelectionSnapshot
        {
            internal SelectionSnapshot(BlueprintFeatureSelection selection)
            {
                Selection = selection;
                Features = selection.Features;
                AllFeatures = selection.AllFeatures;
            }

            internal BlueprintFeatureSelection Selection { get; private set; }
            internal BlueprintFeature[] Features { get; private set; }
            internal BlueprintFeature[] AllFeatures { get; private set; }
        }

        private readonly BlueprintFeatureSelection _basic;
        private readonly BlueprintFeatureSelection _fighter;
        private readonly BlueprintFeatureSelection[] _selections;
        private SelectionSnapshot[] _snapshots;

        internal FirearmFeatCatalogPublication(BlueprintFeatureSelection basic,
            BlueprintFeatureSelection fighter,
            BlueprintFeatureSelection[] selections)
        {
            _basic = basic ?? throw new ArgumentNullException("basic");
            _fighter = fighter ?? throw new ArgumentNullException("fighter");
            _selections = selections ?? throw new ArgumentNullException("selections");
            if (_selections.Any(value => value == null))
                throw new ArgumentException("Feat selections cannot contain null.",
                    "selections");
        }

        internal void Publish(BlueprintFeature rapidReload,
            BlueprintFeature legacyProficiency)
        {
            if (rapidReload == null) throw new ArgumentNullException("rapidReload");
            if (legacyProficiency == null)
                throw new ArgumentNullException("legacyProficiency");
            if (_snapshots != null)
                throw new InvalidOperationException(
                    "Firearm feat catalogs were already published.");

            BlueprintFeatureSelection[] touched = _selections.Where(selection =>
                ReferenceEquals(selection, _basic) ||
                ReferenceEquals(selection, _fighter) ||
                Contains(selection.Features, legacyProficiency) ||
                Contains(selection.AllFeatures, legacyProficiency)).ToArray();
            _snapshots = touched.Select(selection =>
                new SelectionSnapshot(selection)).ToArray();

            for (int index = 0; index < touched.Length; index++)
            {
                BlueprintFeatureSelection selection = touched[index];
                selection.Features = RemoveAll(selection.Features,
                    legacyProficiency);
                selection.AllFeatures = RemoveAll(selection.AllFeatures,
                    legacyProficiency);
            }
            _basic.Features = AppendUnique(_basic.Features, rapidReload);
            _basic.AllFeatures = AppendUnique(_basic.AllFeatures, rapidReload);
            _fighter.Features = AppendUnique(_fighter.Features, rapidReload);
            _fighter.AllFeatures = AppendUnique(_fighter.AllFeatures, rapidReload);

            if (_selections.Any(selection =>
                    Contains(selection.Features, legacyProficiency) ||
                    Contains(selection.AllFeatures, legacyProficiency)))
                throw new InvalidOperationException(
                    "Exotic Weapon Proficiency (Firearms) remained in a feat selection.");
            if (Count(_basic.Features, rapidReload) != 1 ||
                Count(_basic.AllFeatures, rapidReload) != 1 ||
                Count(_fighter.Features, rapidReload) != 1 ||
                Count(_fighter.AllFeatures, rapidReload) != 1)
                throw new InvalidOperationException(
                    "Rapid Reload was not published exactly once in the native feat catalogs.");
        }

        internal void Rollback()
        {
            if (_snapshots == null) return;
            for (int index = _snapshots.Length - 1; index >= 0; index--)
            {
                SelectionSnapshot snapshot = _snapshots[index];
                snapshot.Selection.Features = snapshot.Features;
                snapshot.Selection.AllFeatures = snapshot.AllFeatures;
            }
            _snapshots = null;
        }

        private static BlueprintFeature[] AppendUnique(BlueprintFeature[] source,
            BlueprintFeature addition)
        {
            source = source ?? Array.Empty<BlueprintFeature>();
            if (Contains(source, addition)) return source;
            var result = new BlueprintFeature[source.Length + 1];
            for (int index = 0; index < source.Length; index++)
                result[index] = source[index];
            result[source.Length] = addition;
            return result;
        }

        private static BlueprintFeature[] RemoveAll(BlueprintFeature[] source,
            BlueprintFeature feature)
        {
            source = source ?? Array.Empty<BlueprintFeature>();
            BlueprintFeature[] result = source.Where(value =>
                !Same(value, feature)).ToArray();
            return result.Length == source.Length ? source : result;
        }

        private static bool Contains(BlueprintFeature[] source,
            BlueprintFeature feature)
        {
            return (source ?? Array.Empty<BlueprintFeature>()).Any(value =>
                Same(value, feature));
        }

        private static int Count(BlueprintFeature[] source,
            BlueprintFeature feature)
        {
            return (source ?? Array.Empty<BlueprintFeature>()).Count(value =>
                Same(value, feature));
        }

        private static bool Same(BlueprintFeature left, BlueprintFeature right)
        {
            return ReferenceEquals(left, right) || left != null && right != null &&
                string.Equals(left.AssetGuid, right.AssetGuid,
                    StringComparison.Ordinal);
        }
    }
}
