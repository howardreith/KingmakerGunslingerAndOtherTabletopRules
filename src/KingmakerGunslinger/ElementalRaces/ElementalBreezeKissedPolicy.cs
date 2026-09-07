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
