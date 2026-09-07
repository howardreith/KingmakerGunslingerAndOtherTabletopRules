using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalBreezeKissedPolicyTests
    {
        internal static void ExactDefenseBoundary()
        {
            foreach (bool ready in new[] { false, true })
            foreach (bool calm in new[] { false, true })
            foreach (bool weapon in new[] { false, true })
            foreach (bool ranged in new[] { false, true })
            foreach (bool ability in new[] { false, true })
            foreach (bool physical in new[] { false, true })
            foreach (int enhancement in new[] { -1, 0, 1, 5 })
            {
                int expected = ready && !calm && weapon && ranged && !ability &&
                    physical && enhancement == 0 ? 2 : 0;
                Assertions.Equal(expected, ElementalBreezeKissedPolicy.ArmorClassBonus(
                    ready, calm, weapon, ranged, ability, physical, enhancement),
                    "Only ready winds protect against an exact known nonmagical ranged weapon attack.");
            }
            Assertions.Equal(2, ElementalBreezeKissedPolicy.ArmorClassBonus(
                true, false, true, true, false, true, 0), "Ready winds protect.");
            Assertions.Equal(0, ElementalBreezeKissedPolicy.ArmorClassBonus(
                true, true, true, true, false, true, 0), "Swift calm suppresses without spending the daily use.");
            Assertions.Equal(0, ElementalBreezeKissedPolicy.ArmorClassBonus(
                false, false, true, true, false, true, 0), "Renew cannot bypass exhaustion.");
            Assertions.Equal(2, ElementalBreezeKissedPolicy.ArmorClassBonus(
                true, false, true, true, false, true, 0), "Ordinary restored capacity re-enables uncalmed winds.");
        }
    }
}
