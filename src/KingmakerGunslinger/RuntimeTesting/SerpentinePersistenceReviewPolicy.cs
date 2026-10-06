using System;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Closed test-fixture policy, not a gameplay serialization subsystem.
    internal static class SerpentinePersistenceReviewPolicy
    {
        internal const string Scope = "snakes";
        internal const string ReceiptScope = "KMG_Sprint17_SnakePersistence_v1";
        internal static string[] Roles
        { get { return new[] { "viper", "constrictor-snake", "venom-target", "hold-target" }; } }

        internal static string CreatureKey(string role)
        {
            switch (role)
            {
                case "viper": return "viper";
                case "constrictor-snake": return "constrictor-snake";
                case "venom-target": case "hold-target": return "wolf";
                default: return null;
            }
        }

        internal static bool Owns(string scope, string role, string storedUnit, string actualUnit,
            string storedCaster, string actualCaster, string expectedBlueprint, string actualBlueprint)
        {
            return scope == ReceiptScope && CreatureKey(role) != null &&
                !string.IsNullOrWhiteSpace(storedUnit) && storedUnit == actualUnit &&
                !string.IsNullOrWhiteSpace(storedCaster) && storedCaster == actualCaster &&
                !string.IsNullOrWhiteSpace(expectedBlueprint) && expectedBlueprint == actualBlueprint;
        }

        internal static bool PreservedVenom(int applications, int dc, int ticks, int saves,
            int savedDc, int savedTicks, int savedSaves)
        {
            return applications == 1 && dc == 13 && dc == savedDc &&
                ticks >= 1 && ticks < 6 && ticks == savedTicks && saves == 0 && saves == savedSaves;
        }
    }
}
