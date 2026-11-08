using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Repair sprint R4 (MEPH-01..04): the ten-breath damage requirements,
    /// explicit physical/energy classification, the exactly-one owned
    /// redirect used for grants and brain actions, and the registered
    /// identities of the four owned native-backed breaths.
    /// </summary>
    internal static class SummonPlaytestR4Tests
    {
        internal static void TenBreathsRequireTheMissionDamage()
        {
            var expected = new[] {
                new { Key = "air-mephit", Kind = MephitBreathDamageKind.Slashing },
                new { Key = "dust-mephit", Kind = MephitBreathDamageKind.Slashing },
                new { Key = "earth-mephit", Kind = MephitBreathDamageKind.Bludgeoning },
                new { Key = "fire-mephit", Kind = MephitBreathDamageKind.Fire },
                new { Key = "ice-mephit", Kind = MephitBreathDamageKind.Cold },
                new { Key = "magma-mephit", Kind = MephitBreathDamageKind.Fire },
                new { Key = "ooze-mephit", Kind = MephitBreathDamageKind.Acid },
                new { Key = "salt-mephit", Kind = MephitBreathDamageKind.Slashing },
                new { Key = "steam-mephit", Kind = MephitBreathDamageKind.Fire },
                new { Key = "water-mephit", Kind = MephitBreathDamageKind.Acid } };
            Assertions.Equal(10, MephitBreathPolicy.Rows.Count, "Exactly ten mephit breaths.");
            foreach (var row in expected)
                Assertions.Equal(row.Kind, MephitBreathPolicy.Row(row.Key).Required, row.Key + " required damage.");
            Assertions.True(MephitBreathPolicy.NativeBackedRows.Select(row => row.Key).OrderBy(key => key)
                .SequenceEqual(ExpandedSummoningSpecialProfiles.NativeMephitKeys.OrderBy(key => key)),
                "Exactly the four native-backed mephits are redirected.");
            Assertions.Equal(4, MephitBreathPolicy.NativeBreathGuids.Distinct().Count(), "Four distinct native breaths.");
            // The six project variants already declare their damage; their
            // profile must agree with the required table (no rewrite).
            foreach (MephitVariantProfile profile in ExpandedSummoningSpecialProfiles.MephitVariants)
            {
                MephitBreathRow row = MephitBreathPolicy.Row(profile.Key);
                Assertions.False(row.NativeBacked, profile.Key + " is a project variant.");
                Assertions.Equal(row.Required, MephitBreathPolicy.Parse(profile.BreathEnergy), profile.Key + " profile damage.");
            }
            Assertions.Equal(10, MephitBreathPolicy.Rows.Select(row => row.IconSourceGuid).Distinct().Count(),
                "Every breath reuses its own distinct existing art.");
        }

        internal static void DamageKindsAreExplicit()
        {
            Assertions.Equal(MephitBreathDamageKind.Bludgeoning, MephitBreathPolicy.Parse("Bludgeoning"), "Bludgeoning stays bludgeoning.");
            Assertions.True(MephitBreathPolicy.IsPhysical(MephitBreathDamageKind.Bludgeoning) &&
                MephitBreathPolicy.IsPhysical(MephitBreathDamageKind.Slashing), "Slashing and bludgeoning are physical.");
            foreach (var kind in new[] { MephitBreathDamageKind.Fire, MephitBreathDamageKind.Cold, MephitBreathDamageKind.Acid })
                Assertions.False(MephitBreathPolicy.IsPhysical(kind), kind + " is energy.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => MephitBreathPolicy.Parse("Electricity"),
                "Electricity is no mephit breath and never falls back to another type.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => MephitBreathPolicy.Parse(null), "A missing type is refused.");
            Assertions.Equal(MephitBreathPolicy.AcidConeProjectileGuid,
                MephitBreathPolicy.ProjectileFor(MephitBreathPolicy.Row("water-mephit").Required), "Water uses the acid cone.");
            Assertions.False(MephitBreathPolicy.ProjectileFor(MephitBreathPolicy.Row("earth-mephit").Required) ==
                MephitBreathPolicy.AcidConeProjectileGuid, "Bludgeoning Earth no longer shows the acid cone.");
            Assertions.Equal(MephitBreathPolicy.FireConeProjectileGuid, MephitBreathPolicy.ProjectileFor(MephitBreathDamageKind.Fire), "Fire cone.");
            string water = MephitBreathPolicy.NativeBackedDescription(MephitBreathPolicy.Row("water-mephit"), 1, 8);
            Assertions.True(water.Contains("1d8 acid damage") && water.Contains("Reflex") && !water.Contains("cold") &&
                !water.Contains("electric"), "Water text states its actual acid damage.");
            Assertions.True(MephitBreathPolicy.NativeBackedDescription(MephitBreathPolicy.Row("earth-mephit"), 1, 8)
                .Contains("1d8 bludgeoning damage"), "Earth text states bludgeoning.");
            Assertions.Throws<ArgumentException>(() => MephitBreathPolicy.NativeBackedDescription(MephitBreathPolicy.Row("dust-mephit"), 1, 4),
                "Project variants keep their own breath text.");
        }

        internal static void RedirectReplacesExactlyOneEntry()
        {
            object native = new object(), other = new object(), owned = new object();
            var input = new[] { other, native, null, other };
            object[] result = MephitBreathPolicy.ReplaceExactlyOne(input, value => ReferenceEquals(value, native), owned, "facts");
            Assertions.True(ReferenceEquals(result[1], owned) && ReferenceEquals(result[0], other) && result[2] == null &&
                ReferenceEquals(result[3], other) && result.Length == 4, "Only the single native entry is replaced, in place.");
            Assertions.True(ReferenceEquals(input[1], native) && !ReferenceEquals(result, input), "The input (possibly shared) array is never written.");
            Assertions.Throws<InvalidOperationException>(() => MephitBreathPolicy.ReplaceExactlyOne(new[] { other },
                value => ReferenceEquals(value, native), owned, "facts"), "No native entry: refused.");
            Assertions.Throws<InvalidOperationException>(() => MephitBreathPolicy.ReplaceExactlyOne(new[] { native, native },
                value => ReferenceEquals(value, native), owned, "facts"), "Two native entries: refused, nothing partially redirected.");
            Assertions.Throws<InvalidOperationException>(() => MephitBreathPolicy.ReplaceExactlyOne<object>(null,
                value => true, owned, "facts"), "Missing grant array: refused.");
        }

        internal static void OwnedBreathIdentitiesAreRegistered()
        {
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string token in new[] { "AirMephit", "EarthMephit", "FireMephit", "WaterMephit" })
                foreach (var expected in new[] {
                    new { Kind = "Breath", Type = "BlueprintAbility" },
                    new { Kind = "BreathAi", Type = "BlueprintAiCastSpell" },
                    new { Kind = "Brain", Type = "BlueprintBrain" } })
                {
                    string symbol = "KMG.Summoning.Special." + token + "." + expected.Kind;
                    Assertions.Equal(1, identities.Count(value => value.Symbol == symbol && value.PlannedType == expected.Type),
                        "Exactly one registered " + expected.Type + " " + symbol + ".");
                }
        }
    }
}
