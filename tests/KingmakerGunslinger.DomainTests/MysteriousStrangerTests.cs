using KingmakerGunslinger.Archetypes;
namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void MysteriousStrangerGrit()
        { Assertions.Equal(1,MysteriousStrangerPolicy.GritMaximum(-2),"Charisma grit floor changed."); Assertions.Equal(4,MysteriousStrangerPolicy.GritMaximum(4),"Charisma grit changed."); }
        private static void MysteriousStrangerFocusedAim()
        { Assertions.Equal(1,MysteriousStrangerPolicy.FocusedAimBonus(-1,1),"Focused Aim floor changed."); Assertions.Equal(12,MysteriousStrangerPolicy.FocusedAimBonus(4,3),"Dead Shot multiplier changed."); }
        private static void MysteriousStrangerFocusedAimTransactions()
        {
            Assertions.True(MysteriousStrangerPolicy.CanActivateFocusedAim(
                2, 1, true, false), "Positive Grit must permit Focused Aim.");
            int one = MysteriousStrangerPolicy.FocusedAimGritAfter(2, 1);
            Assertions.Equal(1, one, "Ordinary Focused Aim must spend exactly one Grit.");
            Assertions.Equal(0, MysteriousStrangerPolicy.FocusedAimGritAfter(one, 1),
                "A repeated legal use must spend exactly once per activation.");
            Assertions.True(!MysteriousStrangerPolicy.CanActivateFocusedAim(
                0, 1, true, false), "Zero Grit must reject ordinary Focused Aim.");
            Assertions.True(MysteriousStrangerPolicy.CanActivateFocusedAim(
                1, 0, true, false), "True Grit Focused Aim must remain positive-Grit gated.");
            Assertions.Equal(1, MysteriousStrangerPolicy.FocusedAimGritAfter(1, 0),
                "True Grit Focused Aim must preserve positive Grit.");
            Assertions.True(!MysteriousStrangerPolicy.CanActivateFocusedAim(
                0, 0, true, false), "True Grit must not permit zero-Grit activation.");
            Assertions.True(!MysteriousStrangerPolicy.CanActivateFocusedAim(
                2, 1, false, false), "A non-owner must not activate Focused Aim.");
            Assertions.True(!MysteriousStrangerPolicy.CanActivateFocusedAim(
                2, 1, true, true), "An armed duplicate must not spend again.");
        }
        private static void MysteriousStrangerLucky()
        { int[] expected={0,1,1,2,3,4,5}; int[] levels={1,2,5,6,10,14,18}; for(int i=0;i<levels.Length;i++) Assertions.Equal(expected[i],MysteriousStrangerPolicy.LuckyBonus(levels[i]),"Lucky progression changed."); }
        private static void MysteriousStrangerFortune()
        { Assertions.Equal(0,MysteriousStrangerPolicy.FortuneUses(-1),"Fortune floor changed."); Assertions.Equal(5,MysteriousStrangerPolicy.FortuneUses(5),"Fortune uses changed."); }
        private static void MysteriousStrangerClippingShot()
        { Assertions.Equal(4,MysteriousStrangerPolicy.ClippingShotDamage(9),"Clipping Shot rounds down."); }
    }
}
