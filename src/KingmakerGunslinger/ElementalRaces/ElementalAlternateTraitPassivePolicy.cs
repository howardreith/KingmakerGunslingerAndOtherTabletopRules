namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Dependency-free boundaries for the first Release C mechanics.</summary>
    internal static class ElementalAlternateTraitPassivePolicy
    {
        internal static int SavingThrowBonus(ElementalAlternateTraitId trait,
            bool fatigue, bool exhaustion, bool enchantment, bool divination)
        {
            if (trait == ElementalAlternateTraitId.ForgeHardened)
                return fatigue || exhaustion ? 2 : 0;
            if (trait == ElementalAlternateTraitId.Secretive)
                return enchantment || divination ? 2 : 0;
            return 0;
        }

        internal static int BrazenFlameDamage(bool successfulAttack,
            bool nativeMeleeWeapon, bool exactAttackCorrelation,
            bool spellDamage)
        {
            return successfulAttack && nativeMeleeWeapon &&
                exactAttackCorrelation && !spellDamage ? 1 : 0;
        }
    }
}
