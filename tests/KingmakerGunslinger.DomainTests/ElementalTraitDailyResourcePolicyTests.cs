using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalTraitDailyResourcePolicyTests
    {
        internal static void ActivationNeverRefillsSpentUses()
        {
            for (int current = -1; current <= 5; current++)
            {
                int nonnegative = System.Math.Max(0, current);
                Assertions.Equal(nonnegative,
                    ElementalTraitDailyResourcePolicy.ActivationAmount(current, null),
                    "A first native grant retains its native amount.");
                for (int remembered = -1; remembered <= 5; remembered++)
                    Assertions.Equal(System.Math.Min(nonnegative, System.Math.Max(0, remembered)),
                        ElementalTraitDailyResourcePolicy.ActivationAmount(current, remembered),
                        "Activation must preserve either native or remembered expenditure.");
            }
            Assertions.Equal(0, ElementalTraitDailyResourcePolicy.ActivationAmount(1, 0),
                "A rebuilt full resource must retain its previously spent use.");
            Assertions.Equal(0, ElementalTraitDailyResourcePolicy.ActivationAmount(0, 1),
                "Native serialized zero must not be refilled by an older remembered amount.");
            Assertions.Equal(1, ElementalTraitDailyResourcePolicy.ActivationAmount(1, null),
                "Ordinary rest clearing prior-day memory permits a full new native grant.");
        }
    }
}
