using System;
using System.Linq;

namespace KingmakerGunslinger.CustomWeapons
{
    [Flags]
    internal enum CustomWeaponDamageForm
    {
        None = 0,
        Piercing = 1,
        Slashing = 2
    }

    [Flags]
    internal enum CustomWeaponFighterGroupPolicy
    {
        None = 0,
        LightBlades = 1,
        HeavyBlades = 2,
        Polearms = 4
    }

    internal enum CustomWeaponProficiencyPolicy
    {
        Exotic = 0,
        KatanaGripDependent = 1,
        Martial = 2
    }

    internal enum CustomWeaponHandedness
    {
        Light = 0,
        OneHandedVersatile = 1,
        TwoHanded = 2
    }

    internal sealed class CustomWeaponPresentationDefinition
    {
        internal CustomWeaponPresentationDefinition(string displayName,
            string acronym, string iconFileName, string prefabName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("A display name is required.", "displayName");
            if (string.IsNullOrWhiteSpace(acronym) || acronym.Length != 2 ||
                acronym.Any(value => value < 'A' || value > 'Z'))
                throw new ArgumentException("A two-letter uppercase acronym is required.",
                    "acronym");
            if (string.IsNullOrWhiteSpace(iconFileName) ||
                string.IsNullOrWhiteSpace(prefabName))
                throw new ArgumentException("Presentation assets are incomplete.");
            DisplayName = displayName;
            Acronym = acronym;
            IconFileName = iconFileName;
            PrefabName = prefabName;
        }

        internal string DisplayName { get; private set; }
        internal string Acronym { get; private set; }
        internal string IconFileName { get; private set; }
        internal string PrefabName { get; private set; }
    }

    internal sealed class CustomWeaponCategoryDefinition
    {
        internal CustomWeaponCategoryDefinition(string key, int categoryValue,
            string weaponTypeSymbol, int baseCost, int weightPounds,
            int damageDiceCount, int damageDieSides, int criticalThreatMinimum,
            int criticalMultiplier, CustomWeaponDamageForm damageForms,
            CustomWeaponHandedness handedness,
            CustomWeaponProficiencyPolicy proficiency,
            CustomWeaponFighterGroupPolicy fighterGroups,
            bool finessable, bool reach, bool thrown,
            CustomWeaponPresentationDefinition presentation)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(weaponTypeSymbol))
                throw new ArgumentException("Custom category identity is incomplete.");
            if (categoryValue <= 0 || baseCost < 0 || weightPounds <= 0 ||
                damageDiceCount <= 0 || damageDieSides <= 0 ||
                criticalThreatMinimum < 2 || criticalThreatMinimum > 20 ||
                criticalMultiplier < 2)
                throw new ArgumentOutOfRangeException("Custom category profile is invalid.");
            if (damageForms == CustomWeaponDamageForm.None || presentation == null)
                throw new ArgumentException("Custom category rules are incomplete.");
            if (reach || thrown)
                throw new ArgumentException(
                    "Eastern custom categories in this release cannot be reach or thrown.");
            if (finessable != (handedness == CustomWeaponHandedness.Light))
                throw new ArgumentException(
                    "Only the locked light-weapon profile may be finessable.");
            Key = key;
            CategoryValue = categoryValue;
            WeaponTypeSymbol = weaponTypeSymbol;
            BaseCost = baseCost;
            WeightPounds = weightPounds;
            DamageDiceCount = damageDiceCount;
            DamageDieSides = damageDieSides;
            CriticalThreatMinimum = criticalThreatMinimum;
            CriticalMultiplier = criticalMultiplier;
            DamageForms = damageForms;
            Handedness = handedness;
            Proficiency = proficiency;
            FighterGroups = fighterGroups;
            Finessable = finessable;
            Reach = reach;
            Thrown = thrown;
            Presentation = presentation;
        }

        internal string Key { get; private set; }
        internal int CategoryValue { get; private set; }
        internal string WeaponTypeSymbol { get; private set; }
        internal int BaseCost { get; private set; }
        internal int WeightPounds { get; private set; }
        internal int DamageDiceCount { get; private set; }
        internal int DamageDieSides { get; private set; }
        internal int CriticalThreatMinimum { get; private set; }
        internal int CriticalMultiplier { get; private set; }
        internal CustomWeaponDamageForm DamageForms { get; private set; }
        internal CustomWeaponHandedness Handedness { get; private set; }
        internal CustomWeaponProficiencyPolicy Proficiency { get; private set; }
        internal CustomWeaponFighterGroupPolicy FighterGroups { get; private set; }
        internal bool Finessable { get; private set; }
        internal bool Reach { get; private set; }
        internal bool Thrown { get; private set; }
        internal CustomWeaponPresentationDefinition Presentation { get; private set; }
    }
}
