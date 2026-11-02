using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class VrockRules37Tests
    {
        internal static void PublicRoutesHaveExactScopeAndQuantity()
        {
            var vrock = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "vrock");
            Assertions.False(vrock.NaturesAllyTier.HasValue ||
                vrock.MonsterTemplated,
                "Vrock is Monster-only and untemplated.");
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == vrock.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            Assertions.True(routes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 7, 8, 9 }),
                "Vrock has exactly Monster VII through IX.");
            Assertions.True(routes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3,
                    SummonMultiplicity.OneD4PlusOne }),
                "Vrock quantities are one, 1d3, then 1d4+1.");
            Assertions.False(ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly)
                .Any(value => value.Creature.Key == vrock.Key),
                "Vrock registers no Nature's Ally route.");
            Assertions.True(routes.All(value =>
                !SummonVisibilityCatalog.IsPublished(value)),
                "All three routes are withheld together while the creature is unqualified.");

            var identities = ExpandedSummoningIdentityCatalog.Build();
            string[] routeSymbols = routes.Select(
                ExpandedSummoningIdentityCatalog.AbilitySymbol).ToArray();
            Assertions.True(routeSymbols.All(symbol => identities.Any(value =>
                value.Symbol == symbol)),
                "All three executable routes are allocated while hidden, so publication moves no identity.");
            Assertions.True(identities.Any(value => value.Symbol ==
                    ExpandedSummoningIdentityCatalog.UnitSymbol(vrock)),
                "The exact unit identity is registered before publication.");
            var privateIdentities = identities.Where(value =>
                value.Symbol.StartsWith("KMG.Summoning.Demon.Vrock.",
                    StringComparison.Ordinal)).ToArray();
            Assertions.Equal(
                SummoningCurrentSurface.Sprint37PrivateIdentityCount,
                privateIdentities.Length,
                "Only dependency-complete configured private identities are active.");
            var expected = new[] {
                new { Type = "BlueprintUnitType", Count = 1 },
                new { Type = "BlueprintFeature", Count = 1 },
                new { Type = "BlueprintItemWeapon", Count = 3 },
                new { Type = "BlueprintWeaponType", Count = 3 },
                new { Type = "BlueprintAbility", Count = 4 },
                new { Type = "BlueprintBuff", Count = 4 },
                new { Type = "BlueprintAbilityResource", Count = 2 },
                new { Type = "BlueprintBrain", Count = 1 },
                new { Type = "BlueprintAiCastSpell", Count = 2 },
                new { Type = "VrockScreechEligibility37", Count = 1 },
                new { Type = "VrockSporeEligibility37", Count = 1 },
                new { Type = "InfernalSpellAi35", Count = 2 },
            };
            foreach (var row in expected)
                Assertions.Equal(row.Count, privateIdentities.Count(value =>
                    value.PlannedType == row.Type),
                    "Vrock private identity shape for " + row.Type + ".");
            Assertions.Equal(expected.Sum(row => row.Count),
                privateIdentities.Length,
                "The private identity shape accounts for every Vrock dependency.");
        }

        internal static void ScreechDifficultyClassFollowsLiveConstitution()
        {
            // The printed DC 21 is 10 + half the racial Hit Dice + the
            // Constitution bonus, which is the released Sprint 36 formula, so
            // it is shared rather than repeated. A different live score has to
            // move the DC: a hard-coded 21 would pass the first check alone.
            Assertions.Equal(VrockRules37Policy.PrintedScreechDc,
                VrockRules37Policy.ScreechDifficultyClass(7),
                "The adult Constitution 25 profile yields the printed DC 21.");
            Assertions.Equal(VrockRules37Policy.PrintedScreechDc + 2,
                VrockRules37Policy.ScreechDifficultyClass(9),
                "A higher live Constitution raises the save DC.");
            Assertions.Equal(VrockRules37Policy.PrintedScreechDc - 3,
                VrockRules37Policy.ScreechDifficultyClass(4),
                "A lower live Constitution lowers the save DC.");
            Assertions.Equal(
                InfernalRules36Policy.ConstitutionDc(
                    VrockRules37Policy.VrockHitDice, 7),
                VrockRules37Policy.ScreechDifficultyClass(7),
                "The screech reuses the released Constitution-based formula.");
        }

        internal static void RechargingAbilitiesRefuseWhileTheirMarkerStands()
        {
            Assertions.True(VrockRules37Policy.CanActivate(true, false, true),
                "The exact owner may screech while recharged and the ability is available.");
            Assertions.False(VrockRules37Policy.CanActivate(true, true, true),
                "A standing recharge marker refuses the next activation.");
            Assertions.False(VrockRules37Policy.CanActivate(false, false, true),
                "Another creature cannot use this vrock's racial ability.");
            Assertions.False(VrockRules37Policy.CanActivate(true, false, false),
                "An unavailable granted ability is not activated by the marker alone.");
            Assertions.Equal(1, VrockRules37Policy.ScreechRechargeHours,
                "The screech recharges once per hour, not once per encounter or summoning.");
            Assertions.Equal(3, VrockRules37Policy.SporeCadenceRounds,
                "Spores return every three rounds.");
        }

        internal static void ScreechExemptsDemonsAndSporesExemptNobody()
        {
            Assertions.True(VrockRules37Policy.ScreechReaches(true, false),
                "A non-demon inside the radius is affected.");
            Assertions.False(VrockRules37Policy.ScreechReaches(true, true),
                "A demon is exempt from the screech, as printed.");
            Assertions.False(VrockRules37Policy.ScreechReaches(false, false),
                "Only this vrock's own screech resolves through its component.");
            // The printed spore cloud has no exemption at all, so an ally and
            // another demon are both eligible; ally safety is an AI selection
            // rule, not an eligibility rule.
            Assertions.True(VrockRules37Policy.SporesReach(true, true),
                "Every live adjacent creature takes the spores.");
            Assertions.False(VrockRules37Policy.SporesReach(true, false),
                "A destroyed or dead target takes nothing.");
            Assertions.False(VrockRules37Policy.SporesReach(false, true),
                "Only this vrock's own spores resolve through its component.");
            Assertions.Equal(VrockRules37Policy.ReachFeet,
                VrockRules37Policy.SporeRadiusFeet,
                "Adjacency for a Large creature is its own printed ten-foot reach.");
            Assertions.Equal(30, VrockRules37Policy.ScreechRadiusFeet,
                "The screech is a 30-foot-radius spread.");
            Assertions.Equal(1, VrockRules37Policy.ScreechStunRounds,
                "A failed save stuns for one round.");
            Assertions.Equal(10, VrockRules37Policy.SporeVineRounds,
                "The vines grow for ten rounds.");
        }

        internal static void MovementRecordsBothPrintedSpeeds()
        {
            // Kingmaker exposes one movement speed. The effective flying speed
            // is used and the land speed is recorded and omitted; neither is
            // summed into the other, and a future reader can see both.
            Assertions.Equal(50, VrockRules37Policy.EffectiveFlySpeedFeet,
                "The effective speed is the printed 50-foot fly speed.");
            Assertions.Equal(30, VrockRules37Policy.OmittedLandSpeedFeet,
                "The printed 30-foot land speed is recorded, not discarded.");
            Assertions.False(VrockRules37Policy.EffectiveFlySpeedFeet ==
                VrockRules37Policy.OmittedLandSpeedFeet ||
                VrockRules37Policy.EffectiveFlySpeedFeet ==
                    VrockRules37Policy.OmittedLandSpeedFeet +
                    VrockRules37Policy.EffectiveFlySpeedFeet,
                "The two printed speeds are neither conflated nor summed.");
        }

        internal static void PrintedDefencesDeriveFromTheirBaseValues()
        {
            // The blueprint carries base save values; the printed totals are
            // those bases plus the live ability and feat modifiers. Recording
            // both keeps a genuine native difficulty modifier distinguishable
            // from a printed-profile comparison instead of being compensated
            // for in the gameplay numbers.
            Assertions.Equal(VrockRules37Policy.PrintedFortitude,
                VrockRules37Policy.BaseFortitude + 7,
                "Fortitude is its base plus the Constitution 25 bonus.");
            Assertions.Equal(VrockRules37Policy.PrintedReflex,
                VrockRules37Policy.BaseReflex + 2 + 2,
                "Reflex is its base plus Dexterity 15 and Lightning Reflexes.");
            Assertions.Equal(VrockRules37Policy.PrintedWill,
                VrockRules37Policy.BaseWill + 3,
                "Will is its base plus the Wisdom 16 bonus.");
            Assertions.Equal(VrockRules37Policy.PrintedPerception,
                VrockRules37Policy.SkillRanks + 3 + 3 +
                    VrockRules37Policy.RacialPerceptionBonus,
                "Perception is ranks, class skill, Wisdom and the racial +8.");
            Assertions.Equal(11, VrockRules37Policy.NaturalArmor,
                "AC 22 at touch 11 needs exactly +11 natural armour.");
            Assertions.Equal(20, VrockRules37Policy.SpellResistance,
                "Spell resistance is the printed 20, not the Hezrou's 22.");
            VrockRules37Policy.Validate();
        }

        internal static void OwnedBodyDecodesAndIsDistinct()
        {
            string path = "assets/sprint37-vrock/vrock-mesh.json";
            var payload = JObject.Parse(File.ReadAllText(path));
            OriginalMeshOrientation.AssertShippedBody(path,
                VrockVisualPolicy37.Vrock);
            byte[] data = Convert.FromBase64String((string)payload["data"]);
            int vertices = (int)payload["vertexCount"];
            int triangles = (int)payload["triangleCount"];
            Assertions.Equal(vertices * 64 + triangles * 12, data.Length,
                "The Sprint 37 decoder layout is complete.");
            for (int index = 0; index < vertices * 8; index++)
            {
                float value = BitConverter.ToSingle(data, index * 4);
                Assertions.True(!float.IsNaN(value) &&
                    !float.IsInfinity(value),
                    "Sprint 37 vertex, normal and UV values are finite.");
            }
            Assertions.True(payload["bones"].Values<string>()
                .SequenceEqual(InfernalVisualPolicy36.Bones),
                "The body rides the same measured rig order the released outsiders use.");
            Assertions.Equal(InfernalVisualPolicy36.RigSha256,
                (string)payload["rigSha256"],
                "The exact measured capture behind the released bodies is the one used here.");
            var albedo = payload["albedo"];
            using (var hash = SHA256.Create())
            {
                string actual = string.Concat(hash.ComputeHash(
                    File.ReadAllBytes(Path.Combine(
                        Path.GetDirectoryName(path),
                        (string)albedo["file"]))).Select(value =>
                            value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "The runtime albedo agrees with its Sprint 37 mesh.");
            }
            Assertions.Equal("vulture-headed-winged-demon",
                (string)payload["distinctSilhouette"],
                "The Vrock keeps its own silhouette rather than a Sprint 36 demon's.");
            Assertions.True((bool)payload["vultureHead"] &&
                (bool)payload["hookedBeak"] &&
                (int)payload["visibleWings"] == 2 &&
                (int)payload["visibleTalons"] == 2 &&
                (int)payload["visiblePrimaries"] == 14,
                "The body carries its vulture head, hooked beak, two wings and taloned feet.");
            Assertions.True((bool)payload["rigidWings"] &&
                !(bool)payload["articulatedWingBones"],
                "The wings are recorded as rigid rather than claimed as articulated flight.");

            // Distinct bodies, not one shape used three times.
            foreach (string other in new[] {
                "assets/sprint36-demons/babau-mesh.json",
                "assets/sprint36-demons/hezrou-mesh.json" })
            {
                var released = JObject.Parse(File.ReadAllText(other));
                Assertions.False((string)released["data"] ==
                    (string)payload["data"],
                    "The Vrock body is not a copy of a released demon body.");
                Assertions.False(
                    (string)released["distinctSilhouette"] ==
                    (string)payload["distinctSilhouette"],
                    "The Vrock silhouette label is its own.");
                Assertions.False(
                    (string)released["albedo"]["sha256"] ==
                    (string)albedo["sha256"],
                    "The Vrock albedo is its own painting.");
            }
        }
    }
}
