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
            var publication = new FirearmFeatCatalogPublication(basic, fighter);
            BlueprintParametrizedFeature nativeWeaponFocus = BlueprintLibraryLookup.RequireExact<
                BlueprintParametrizedFeature>(library, NativeWeaponFocusGuid, "native Weapon Focus");
            // Rapid Reload and Exotic Weapon Proficiency (Firearms) are the
            // only project-owned top-level feats. Firearm parameters are
            // appended inside the five native parametrized feat menus by the
            // native integration adapter. The retained wrappers are hidden
            // compatibility blueprints for existing 0.0.61/0.0.62 saves.
            var additions = new BlueprintFeature[] {
                set.RapidReload, set.ExoticWeaponProficiency };
            publication.Publish(nativeWeaponFocus, additions);
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
            feature.Groups = new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
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
        private readonly BlueprintFeatureSelection _basic;
        private readonly BlueprintFeatureSelection _fighter;
        private BlueprintFeature[] _basicBefore;
        private BlueprintFeature[] _basicAllBefore;
        private BlueprintFeature[] _fighterBefore;
        private BlueprintFeature[] _fighterAllBefore;

        internal FirearmFeatCatalogPublication(BlueprintFeatureSelection basic,
            BlueprintFeatureSelection fighter)
        { _basic = basic; _fighter = fighter; }

        internal void Publish(BlueprintFeature nativeWeaponFocus,
            BlueprintFeature[] additions)
        {
            if (nativeWeaponFocus == null) throw new ArgumentNullException("nativeWeaponFocus");
            if (additions == null || additions.Any(value => value == null))
                throw new ArgumentNullException("additions");
            _basicBefore = _basic.Features;
            _basicAllBefore = _basic.AllFeatures;
            _fighterBefore = _fighter.Features;
            _fighterAllBefore = _fighter.AllFeatures;
            _basic.Features = AppendUnique(_basicBefore, additions);
            _basic.AllFeatures = AppendUnique(_basicAllBefore, additions);
            _fighter.Features = AppendUnique(_fighterBefore, additions);
            _fighter.AllFeatures = AppendUnique(_fighterAllBefore, additions);
        }

        internal void Rollback()
        {
            _basic.Features = _basicBefore;
            _basic.AllFeatures = _basicAllBefore;
            _fighter.Features = _fighterBefore;
            _fighter.AllFeatures = _fighterAllBefore;
        }

        private static BlueprintFeature[] AppendUnique(BlueprintFeature[] source,
            BlueprintFeature[] additions)
        {
            source = source ?? Array.Empty<BlueprintFeature>();
            BlueprintFeature[] missing = additions.Where(value =>
                Array.IndexOf(source, value) < 0).ToArray();
            var result = new BlueprintFeature[source.Length + missing.Length];
            for (int index = 0; index < source.Length; index++)
                result[index] = source[index];
            Array.Copy(missing, 0, result, source.Length, missing.Length);
            return result;
        }
    }
}
