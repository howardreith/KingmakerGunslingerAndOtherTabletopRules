namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalBreezeKissedPolicy
    {
        internal const int RangeFeet = 30;
        internal const string Description = "While your winds are ready, gain a +2 racial bonus to AC against " +
            "nonmagical ranged weapon attacks. Calm or renew the winds as a swift action. Once per ordinary rest, " +
            "use a standard action to attempt a Bull Rush or Trip against one creature within 30 feet. " +
            "Use your ordinary native combat maneuver bonus, not character level or a mental ability modifier. " +
            "The attempt exhausts the winds and their AC bonus until ordinary rest, whether it succeeds or fails. " +
            "Renewing calmed winds never restores an exhausted daily use. Magical attacks receive no AC bonus.";

        // Exact native feat actions and the inspected optional Pinpoint action.
        // Special alone is insufficient: Bow Spirit uses it for a magical attack.
        internal static bool IsMundaneWeaponAbility(string guid, bool special)
        {
            if (!special) return false;
            switch (guid)
            {
                case "efc60c91b8e64f244b95c66b270dbd7c": // Vital Strike
                case "c714cd636700ac24a91ca3df43326b00": // Improved Vital Strike
                case "11f971b6453f74d4594c538e3c88d499": // Greater Vital Strike
                case "a6210acb28054f568ead7366bda31fee": // CotW Pinpoint Targeting
                    return true;
                default: return false;
            }
        }

        internal static int ArmorClassBonus(bool useAvailable, bool calmed,
            bool exactWeaponAttack, bool ranged, bool abilitySource,
            bool physicalDescriptionKnown, int nativeEnhancementTotal)
        {
            // Zero is the native physical damage /magic boundary. Unknown or
            // negative metadata fails closed; energy riders do not replace it.
            return useAvailable && !calmed && exactWeaponAttack && ranged &&
                !abilitySource && physicalDescriptionKnown && nativeEnhancementTotal == 0 ? 2 : 0;
        }
    }
}
