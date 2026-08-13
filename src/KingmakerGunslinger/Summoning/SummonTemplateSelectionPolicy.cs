namespace KingmakerGunslinger.Summoning
{
    internal static class SummonTemplateSelectionPolicy
    {
        private const int Good = 2;
        private const int Evil = 4;

        internal static SummonAlignmentMode Select(int casterAlignment,
            bool neutralFiendishMode)
        {
            if ((casterAlignment & Good) != 0)
                return SummonAlignmentMode.Celestial;
            if ((casterAlignment & Evil) != 0)
                return SummonAlignmentMode.Fiendish;
            return neutralFiendishMode ? SummonAlignmentMode.Fiendish :
                SummonAlignmentMode.Celestial;
        }
    }
}
