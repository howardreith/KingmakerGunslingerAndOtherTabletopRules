using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>I08/S06 selected bloodline power adapters (Phase 4).</summary>
    internal static class FavoredClassPowerTests
    {
        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray()));
        }

        // floor((L+e)/2) - floor(L/2): the DC change of a half-level binding.
        internal static void HalfLevelDcDeltaIsExact()
        {
            var cases = new[]
            {
                Tuple.Create(9, 1, 1), Tuple.Create(10, 1, 0), Tuple.Create(9, 2, 1),
                Tuple.Create(10, 2, 1), Tuple.Create(0, 1, 0), Tuple.Create(1, 1, 1),
                Tuple.Create(20, 2, 1), Tuple.Create(19, 2, 1), Tuple.Create(7, 0, 0),
                Tuple.Create(-3, 2, 1)
            };
            foreach (var value in cases)
                Assertions.Equal(value.Item3, FavoredClassMechanicsPolicy.HalfLevelDelta(value.Item1, value.Item2),
                    "HalfLevelDelta(" + value.Item1 + "," + value.Item2 + ")");
        }

        // Fire powers open only through the Ifrit row, air powers only through the Sylph row.
        internal static void PowerTargetsFollowTheirOwnRows()
        {
            string effect = FavoredClassCatalog.EffectSelectedBloodlinePower;
            Assertions.True(FavoredClassLeafCatalog.TargetKeys(effect).SequenceEqual(
                new[] { "FireRay", "FireBlast", "AirRay", "AirBlast" }), "Four implemented power targets.");
            Assertions.True(FavoredClassLeafCatalog.TargetRows(effect, "FireRay").SequenceEqual(new[] { "I08" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "FireBlast").SequenceEqual(new[] { "I08" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "AirRay").SequenceEqual(new[] { "S06" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "AirBlast").SequenceEqual(new[] { "S06" }),
                "Per-target source rows.");
            FavoredClassEffectSpec spec = FavoredClassCatalog.Effect(effect);
            Func<string, string, bool> eligible = (ancestry, target) => FavoredClassEligibility.IsEligible(spec,
                new System.Collections.Generic.HashSet<string>(new[] { ancestry }, StringComparer.Ordinal),
                profile => profile != FavoredClassProfile.None, race => true,
                FavoredClassLeafCatalog.TargetRows(effect, target));
            Assertions.True(eligible(FavoredClassAncestry.Ifrit, "FireRay") &&
                eligible(FavoredClassAncestry.Ifrit, "FireBlast"), "Ifrit opens fire powers.");
            Assertions.False(eligible(FavoredClassAncestry.Ifrit, "AirRay") ||
                eligible(FavoredClassAncestry.Ifrit, "AirBlast"), "Ifrit never opens air powers.");
            Assertions.True(eligible(FavoredClassAncestry.Sylph, "AirRay") &&
                eligible(FavoredClassAncestry.Sylph, "AirBlast"), "Sylph opens air powers.");
            Assertions.False(eligible(FavoredClassAncestry.Sylph, "FireRay") ||
                eligible(FavoredClassAncestry.Human, "FireRay") || eligible(FavoredClassAncestry.Human, "AirRay"),
                "Other ancestries open no power.");
            foreach (FavoredClassLeafSpec leaf in FavoredClassLeafCatalog.LeavesFor(effect))
            {
                bool fire = leaf.TargetKey.StartsWith("Fire", StringComparison.Ordinal);
                Assertions.True(leaf.Description.Contains(fire ? "Ifrit" : "Sylph") &&
                    !leaf.Description.Contains(fire ? "Sylph" : "Ifrit"),
                    leaf.Symbol + " names only its own route.");
                Assertions.True(leaf.Description.Contains("Each bloodline power keeps its own separate count"),
                    leaf.Symbol + " discloses its separate counter.");
            }
        }

        // One chosen power's own ability only; exact DC delta from its own binding.
        internal static void EffectiveLevelIsScopedToTheChosenPower()
        {
            string level = Source("Mechanics", "FavoredClassSelectedPowerLevel.cs");
            foreach (string token in new[]
            {
                "evt.AddBonusCasterLevel(earned);",
                "int delta = FavoredClassMechanicsPolicy.HalfLevelDelta(level, earned);",
                "evt.AddBonusDC(delta);",
                "return spell != null && (spell == Ability || (spell.Parent != null && spell.Parent == Ability));",
                "value.Abilites != null && value.Abilites.Contains(Ability)",
                "!FavoredClassRuntime.MechanicsEnabled"
            })
                Assertions.True(level.Contains(token), "Selected power token: " + token);
            foreach (string forbidden in new[] { "AddFakeClassLevel", "Progression.AddClass", "AddBonusSpellLevel",
                "Stats.BaseAttackBonus", "Spellbook.AddCasterLevel" })
                Assertions.False(level.Contains(forbidden), "No blanket class-level change: " + forbidden);
            string blueprints = Source("FavoredClassBlueprints.cs");
            foreach (string token in new[]
            {
                "\"ce0889b5c1b392e48baf1e004d1efd67\", \"1b4989258e5964149a909e47c72b7f67\"",
                "\"3022a5066a5604a498dd289b37dfd8aa\", \"b2d1d39cd406e0f4185c52fecc73c3b5\"",
                "\"acf668c24dfbcdd499276eaf1881486e\", \"4729c2ac98d02004fb440d17f7786e28\"",
                "\"553d9802d5d9de04b941b55cb47d3096\", \"6d005cc9c3ad3f24e8769aad2fbfdf3f\"",
                "{ FavoredClassCatalog.Sorcerer, \"b3a505fb61437dc4097f43c3f8f9a4cf\" },",
                "owned.Feature = usablePower;",
                "prerequisite.RowIds = restrictedRows;"
            })
                Assertions.True(blueprints.Contains(token), "Bloodline power wiring token: " + token);
        }
    }
}
