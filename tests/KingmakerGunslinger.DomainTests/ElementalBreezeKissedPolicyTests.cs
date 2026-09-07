using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalBreezeKissedPolicyTests
    {
        internal static void ExactMundaneAbilitySources()
        {
            foreach (string guid in new[] { "efc60c91b8e64f244b95c66b270dbd7c",
                "c714cd636700ac24a91ca3df43326b00", "11f971b6453f74d4594c538e3c88d499",
                "a6210acb28054f568ead7366bda31fee" })
            {
                Assertions.True(ElementalBreezeKissedPolicy.IsMundaneWeaponAbility(guid, true),
                    "Exact inspected mundane feat source is eligible.");
                Assertions.False(ElementalBreezeKissedPolicy.IsMundaneWeaponAbility(guid, false),
                    "A magic-type rewrite of the exact source remains excluded.");
                Assertions.False(ElementalBreezeKissedPolicy.IsMundaneWeaponAbility(guid.ToUpperInvariant(), true),
                    "A noncanonical identity is not a qualified source.");
            }
            foreach (string guid in new[] { null, "", "ce03a8aec88447da8a06b93225875553",
                "4b0d1566d0c0478bb312e1cdaa373950", "8a57e1072da4f6f4faaa55b7b7dc633c" })
                Assertions.False(ElementalBreezeKissedPolicy.IsMundaneWeaponAbility(guid, true),
                    "Unknown, Bow Spirit, Produce Flame and Master Hunter are not mundane feat actions.");
        }

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
