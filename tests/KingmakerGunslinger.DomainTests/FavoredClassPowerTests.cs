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

        // Charter 8.10: exactly the fire and air elemental bloodline identities
        // and their proven Seeker/Crossblooded copies; never a Primal copy.
        internal static void EligibleBloodlinesAreExact()
        {
            string[] primal = { "3bb24b6f1cd741f78e585485b163dc45", "248fc512bb12418ebd5f2bd10917bf7a" };
            foreach (string target in new[] { "FireRay", "FireBlast", "AirRay", "AirBlast" })
            {
                System.Collections.Generic.KeyValuePair<string, string[]> eligible =
                    FavoredClassLeafCatalog.EligibleBloodlines(target);
                bool fire = target.StartsWith("Fire", StringComparison.Ordinal);
                Assertions.Equal(fire ? "the fire elemental bloodline" : "the air elemental bloodline", eligible.Key,
                    target + " bloodline name.");
                Assertions.True(eligible.Value.SequenceEqual(fire
                    ? new[] { "17cc794d47408bc4986c55265475c06f", "3950cf0cafa5c6ba1d1fc840d2682837",
                        "ae4f8d4d7f23c49929b4f4b88757524c" }
                    : new[] { "cd788df497c6f10439c7025e87864ee4", "e3e43bb57f23bc7abcb49f38019ba6bc",
                        "74fb79f4afa5be59881fa3c054a4dcc7" }), target + " eligible identities.");
                Assertions.False(eligible.Value.Intersect(primal).Any(), target + " excludes Primal copies.");
            }
            string blueprints = Source("FavoredClassBlueprints.cs");
            Assertions.True(blueprints.Contains("FavoredClassLeafCatalog.EligibleBloodlines(targetKey);") &&
                blueprints.Contains("bloodline.FeatureGuids = bloodlines.Value;"),
                "Bloodline power leaves require an eligible bloodline identity.");
            Assertions.True(blueprints.Contains("owned.FeatureGuids = new[] { usablePower.AssetGuid };") &&
                !blueprints.Contains("CreateInstance<PrerequisiteFeature>()"),
                "The usable power is an owned target like the bloodline and the revelations.");
            string prerequisites = Source("FavoredClassPrerequisites.cs");
            string replay = Source(Path.Combine("Hooks", "FavoredClassLevelUpReplayPatch.cs"));
            Assertions.True(prerequisites.Contains("return FavoredClassPendingPicks.Selects(state, FeatureGuids);") &&
                prerequisites.Contains("!ReferenceEquals(controller.State, state)") &&
                replay.Contains("[HarmonyPatch(typeof(LevelUpController), \"ApplyLevelup\")]") &&
                replay.Contains("FavoredClassPendingPicks.Begin(__instance);") &&
                replay.Contains("FavoredClassPendingPicks.End();"),
                "A target chosen in the same level-up counts during the native priority replay only.");
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
                "!FavoredClassRuntime.MechanicsEnabled",
                "IResourceAmountBonusHandler, IUnitSubscriber",
                "int real = progression.CalcLevel(Owner);",
                "bonus += FavoredClassMechanicsPolicy.ThresholdUsesBetween(UseThresholds(progression, UsesResource),",
                "if (increase.Resource == resource && increase.Value > 0)",
                "!Owner.HasFact(PowerFeature))"
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
                "owned.FeatureGuids = new[] { usablePower.AssetGuid };",
                "prerequisite.RowIds = restrictedRows;",
                "level.UsesResource = logic == null ? null : logic.RequiredResource;",
                "level.BloodlineGuids = FavoredClassLeafCatalog.EligibleBloodlines(targetKey).Value;"
            })
                Assertions.True(blueprints.Contains(token), "Bloodline power wiring token: " + token);
        }
    }
}
