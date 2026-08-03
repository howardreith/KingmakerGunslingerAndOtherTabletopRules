using System;
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
            BlueprintFeature[] rapidReloadChoices)
        {
            WeaponFocus = weaponFocus;
            WeaponFocusChoices = weaponFocusChoices;
            NativeWeaponFocusWithFirearms = nativeWeaponFocusWithFirearms;
            RapidReload = rapidReload;
            RapidReloadChoices = rapidReloadChoices;
        }
        internal BlueprintFeatureSelection WeaponFocus { get; private set; }
        internal BlueprintFeature[] WeaponFocusChoices { get; private set; }
        internal BlueprintFeatureSelection NativeWeaponFocusWithFirearms { get; private set; }
        internal BlueprintFeatureSelection RapidReload { get; private set; }
        internal BlueprintFeature[] RapidReloadChoices { get; private set; }
    }

    internal static class FirearmFeatBlueprints
    {
        private const string BasicFeatSelectionGuid = "247a4068296e8be42890143f451b4b45";
        private const string FighterFeatSelectionGuid = "41c8486641f7d6d4283ca9dae4147a9f";
        private const string NativeWeaponFocusGuid = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
        internal const string WeaponFocusSelectionSymbol = "KMG.Feats.FirearmWeaponFocus";
        internal const string NativeWeaponFocusWrapperSymbol =
            "KMG.Feats.NativeWeaponFocusWithFirearms";
        internal const string RapidReloadSelectionSymbol = "KMG.Feats.RapidReload";
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
            BlueprintFeature firearmProficiency)
        {
            if (library == null) throw new ArgumentNullException("library");
            BlueprintFeature nativeWeaponFocus = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeWeaponFocusGuid, "native Weapon Focus");
            var focus = new BlueprintFeature[Kinds.Length];
            var rapid = new BlueprintFeature[Kinds.Length];
            for (int i = 0; i < Kinds.Length; i++)
            {
                FirearmKind kind = Kinds[i];
                focus[i] = registry.Register<BlueprintFeature>(WeaponFocusSymbols[i],
                    () => CreateChoice(kind, firearmProficiency, false));
                rapid[i] = registry.Register<BlueprintFeature>(RapidReloadSymbols[i],
                    () => CreateChoice(kind, firearmProficiency, true));
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
            BlueprintFeatureSelection rapidSelection = registry.Register<BlueprintFeatureSelection>(
                RapidReloadSelectionSymbol, () => CreateSelection("Rapid Reload",
                    "Select a firearm type. Reloading that firearm uses the reduced action listed in its description.", rapid));
            RapidReloadRuntime.Configure(Kinds, rapid);
            return new FirearmFeatBlueprintSet(focusSelection, focus, wrapper,
                rapidSelection, rapid);
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
            BlueprintFeature nativeWeaponFocus = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeWeaponFocusGuid, "native Weapon Focus");
            publication.Publish(nativeWeaponFocus,
                set.NativeWeaponFocusWithFirearms, set.RapidReload);
            return publication;
        }

        private static BlueprintFeature CreateChoice(FirearmKind kind,
            BlueprintFeature proficiency, bool rapid)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + (rapid ? "RapidReload_" : "WeaponFocus_") + kind;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.Groups = new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            var proficiencyPrerequisite = ScriptableObject.CreateInstance<PrerequisiteFeature>();
            proficiencyPrerequisite.Feature = proficiency;
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
            BlueprintFeature wrapper, BlueprintFeature rapidReload)
        {
            _basicBefore = _basic.Features;
            _basicAllBefore = _basic.AllFeatures;
            _fighterBefore = _fighter.Features;
            _fighterAllBefore = _fighter.AllFeatures;
            _basic.Features = ReplaceAndAppend(_basicBefore, nativeWeaponFocus,
                wrapper, rapidReload);
            _basic.AllFeatures = ReplaceAndAppend(_basicAllBefore,
                nativeWeaponFocus, wrapper, rapidReload);
            _fighter.Features = ReplaceAndAppend(_fighterBefore,
                nativeWeaponFocus, wrapper, rapidReload);
            _fighter.AllFeatures = ReplaceAndAppend(_fighterAllBefore,
                nativeWeaponFocus, wrapper, rapidReload);
        }

        internal void Rollback()
        {
            _basic.Features = _basicBefore;
            _basic.AllFeatures = _basicAllBefore;
            _fighter.Features = _fighterBefore;
            _fighter.AllFeatures = _fighterAllBefore;
        }

        private static BlueprintFeature[] ReplaceAndAppend(BlueprintFeature[] source,
            BlueprintFeature nativeWeaponFocus, BlueprintFeature wrapper,
            BlueprintFeature rapidReload)
        {
            source = source ?? Array.Empty<BlueprintFeature>();
            int matches = 0;
            var result = new BlueprintFeature[source.Length + 1];
            for (int index = 0; index < source.Length; index++)
            {
                if (ReferenceEquals(source[index], nativeWeaponFocus))
                {
                    result[index] = wrapper;
                    matches++;
                }
                else result[index] = source[index];
            }
            if (matches != 1) throw new InvalidOperationException(
                "Native Weapon Focus must occur exactly once in each feat catalog.");
            result[result.Length - 1] = rapidReload;
            return result;
        }
    }
}
