using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurIdentityTests
    {
        internal static void PermanentLedgerIsExact()
        {
            BrownFurIdentitySpec[] identities = BrownFurIdentityCatalog.All
                .ToArray();
            Assertions.Equal(BrownFurIdentityCatalog.IdentityCount,
                identities.Length, "Brown-Fur identity count changed.");
            Assertions.Equal(identities.Length, identities.Select(value =>
                value.Symbol).Distinct(StringComparer.Ordinal).Count(),
                "Brown-Fur symbols must be unique.");
            Assertions.Equal(identities.Length, identities.Select(value =>
                value.Guid).Distinct(StringComparer.Ordinal).Count(),
                "Brown-Fur GUIDs must be unique.");
            Assertions.True(identities.All(value => Regex.IsMatch(value.Guid,
                    "^[0-9a-f]{32}$")),
                "Every Brown-Fur GUID must be permanent lowercase hex.");
            Assertions.Equal(1, identities.Count(value => value.PlannedType ==
                "BlueprintArchetype"), "Archetype identity count changed.");
            Assertions.Equal(3, identities.Count(value => value.PlannedType ==
                "BlueprintFeature"), "Feature identity count changed.");
            Assertions.Equal(7, identities.Count(value => value.PlannedType ==
                "BlueprintAbility"), "Ability identity count changed.");
            Assertions.Equal(7, identities.Count(value => value.PlannedType ==
                "BlueprintBuff"), "Buff identity count changed.");
            Assertions.Equal(1, identities.Count(value => value.PlannedType ==
                "BlueprintActivatableAbility"),
                "Share activatable identity count changed.");

            foreach (BrownFurAbilityScore score in new[] {
                BrownFurAbilityScore.Strength, BrownFurAbilityScore.Dexterity,
                BrownFurAbilityScore.Constitution,
                BrownFurAbilityScore.Intelligence, BrownFurAbilityScore.Wisdom,
                BrownFurAbilityScore.Charisma })
            {
                Assertions.True(identities.Any(value => value.Symbol ==
                        BrownFurIdentityCatalog.PowerfulAbility(score) &&
                        value.PlannedType == "BlueprintAbility") &&
                    identities.Any(value => value.Symbol ==
                        BrownFurIdentityCatalog.PowerfulBuff(score) &&
                        value.PlannedType == "BlueprintBuff"),
                    "Powerful Change identity pair missing for " + score + ".");
            }
        }

        internal static void ManifestReservationsMatchLedger()
        {
            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")));
            JObject[] rows = ((JArray)manifest["entries"]).Cast<JObject>()
                .Where(value => ((string)value["symbol"]).StartsWith(
                    "KMG.BrownFur.", StringComparison.Ordinal)).ToArray();
            Assertions.Equal(BrownFurIdentityCatalog.IdentityCount, rows.Length,
                "Manifest Brown-Fur reservation count changed.");
            foreach (BrownFurIdentitySpec identity in BrownFurIdentityCatalog.All)
            {
                JObject row = rows.Single(value =>
                    (string)value["symbol"] == identity.Symbol);
                Assertions.True((string)row["guid"] == identity.Guid &&
                    (string)row["plannedType"] == identity.PlannedType &&
                    (string)row["status"] == "reserved",
                    "Manifest reservation differs from the frozen ledger: " +
                    identity.Symbol);
            }
        }
    }
}
