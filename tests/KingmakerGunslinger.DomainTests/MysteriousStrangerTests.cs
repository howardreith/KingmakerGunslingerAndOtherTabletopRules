using KingmakerGunslinger.Archetypes;
namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void MysteriousStrangerGrit()
        { Assertions.Equal(1,MysteriousStrangerPolicy.GritMaximum(-2),"Charisma grit floor changed."); Assertions.Equal(4,MysteriousStrangerPolicy.GritMaximum(4),"Charisma grit changed."); }
        private static void MysteriousStrangerFocusedAim()
        { Assertions.Equal(1,MysteriousStrangerPolicy.FocusedAimBonus(-1,1),"Focused Aim floor changed."); Assertions.Equal(12,MysteriousStrangerPolicy.FocusedAimBonus(4,3),"Dead Shot multiplier changed."); }
        private static void MysteriousStrangerLucky()
        { int[] expected={0,1,1,2,3,4,5}; int[] levels={1,2,5,6,10,14,18}; for(int i=0;i<levels.Length;i++) Assertions.Equal(expected[i],MysteriousStrangerPolicy.LuckyBonus(levels[i]),"Lucky progression changed."); }
        private static void MysteriousStrangerFortune()
        { Assertions.Equal(0,MysteriousStrangerPolicy.FortuneUses(-1),"Fortune floor changed."); Assertions.Equal(5,MysteriousStrangerPolicy.FortuneUses(5),"Fortune uses changed."); }
        private static void MysteriousStrangerClippingShot()
        { Assertions.Equal(4,MysteriousStrangerPolicy.ClippingShotDamage(9),"Clipping Shot rounds down."); }
    }
}
