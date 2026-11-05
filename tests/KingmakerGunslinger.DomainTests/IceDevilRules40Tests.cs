using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class IceDevilRules40Tests
    {
        internal static void HitAdmission()
        {
            Assertions.True(IceDevilRules40Policy.QualifiesForSlow(true, true, true, false, true), "Owner spear hit qualifies.");
            Assertions.True(IceDevilRules40Policy.QualifiesForSlow(true, true, false, true, true), "Owner tail hit qualifies.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(true, true, false, false, true), "Bite and unrelated cold packets do not qualify.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(true, true, true, false, false), "A miss never qualifies.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(false, true, true, false, true), "Another blueprint cannot borrow the rider.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(true, false, true, false, true), "Another wielder cannot borrow the rider.");
            Assertions.Equal(23, IceDevilRules40Policy.SlowDc(6), "Printed Constitution gives DC23.");
            Assertions.Equal(25, IceDevilRules40Policy.SlowDc(8), "Live Constitution changes the save DC.");
        }

        internal static void BiteStrength()
        {
            Assertions.Equal(3, IceDevilRules40Policy.BiteStrengthCorrection(6, .5f, true), "Restore the printed full bite contribution beside a manufactured weapon.");
            Assertions.Equal(0, IceDevilRules40Policy.BiteStrengthCorrection(6, 1f, true), "Do not double an already-correct native contribution.");
            Assertions.Equal(0, IceDevilRules40Policy.BiteStrengthCorrection(6, .5f, false), "Only secondary-role mismatch is corrected.");
            Assertions.Equal(0, IceDevilRules40Policy.BiteStrengthCorrection(-2, .5f, true), "Do not invent a penalty correction.");
        }
    }
}
