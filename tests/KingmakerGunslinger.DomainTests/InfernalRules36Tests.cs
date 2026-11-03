using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class InfernalRules36Tests
    {
        internal static void PublicRoutesHaveExactScopeAndQuantity()
        {
            var babau = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "babau");
            var hezrou = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "hezrou");
            Assertions.False(babau.NaturesAllyTier.HasValue ||
                babau.MonsterTemplated,
                "Babau is Monster-only and untemplated.");
            Assertions.False(hezrou.NaturesAllyTier.HasValue ||
                hezrou.MonsterTemplated,
                "Hezrou is Monster-only and untemplated.");
            var babauRoutes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == babau.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            var hezrouRoutes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == hezrou.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            Assertions.True(babauRoutes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 5, 6, 7, 8, 9 }),
                "Babau has exactly Monster V through IX.");
            Assertions.True(babauRoutes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3,
                    SummonMultiplicity.OneD4PlusOne,
                    SummonMultiplicity.OneD4PlusOne,
                    SummonMultiplicity.OneD4PlusOne }),
                "Babau quantities are one, 1d3, then 1d4+1.");
            Assertions.True(hezrouRoutes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 8, 9 }),
                "Hezrou has exactly Monster VIII and IX.");
            Assertions.True(hezrouRoutes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3 }),
                "Hezrou quantities are one then 1d3.");
            Assertions.True(babauRoutes.Concat(hezrouRoutes).All(value =>
                SummonVisibilityCatalog.IsPublished(value)),
                "All seven owner-selected routes are public together.");

            var identities = ExpandedSummoningIdentityCatalog.Build();
            string[] routeSymbols = babauRoutes.Concat(hezrouRoutes).Select(
                ExpandedSummoningIdentityCatalog.AbilitySymbol).ToArray();
            Assertions.True(routeSymbols.All(symbol => identities.Any(value =>
                value.Symbol == symbol)),
                "All seven executable routes have append-only identities.");
            Assertions.True(identities.Any(value => value.Symbol ==
                    ExpandedSummoningIdentityCatalog.UnitSymbol(babau)) &&
                identities.Any(value => value.Symbol ==
                    ExpandedSummoningIdentityCatalog.UnitSymbol(hezrou)),
                "Both exact unit identities remain registered at publication.");
            var privateIdentities = identities.Where(value =>
                    value.Symbol.StartsWith(
                        "KMG.Summoning.Infernal.Babau.",
                        StringComparison.Ordinal) ||
                    value.Symbol.StartsWith(
                        "KMG.Summoning.Infernal.Hezrou.",
                        StringComparison.Ordinal)).ToArray();
            Assertions.Equal(
                SummoningCurrentSurface.Sprint36PrivateIdentityCount,
                privateIdentities.Length,
                "Only dependency-complete configured private identities are active.");
            var expectedPrivateTypes = new[] {
                new { Type = "BlueprintUnitType", Count = 2 },
                new { Type = "BlueprintFeature", Count = 2 },
                new { Type = "BlueprintItemWeapon", Count = 5 },
                new { Type = "BlueprintWeaponType", Count = 5 },
                new { Type = "BlueprintAbility", Count = 4 },
                new { Type = "BlueprintBrain", Count = 2 },
                new { Type = "BlueprintBuff", Count = 4 },
                new { Type = "BlueprintAbilityAreaEffect", Count = 1 },
                new { Type = "BlueprintAbilityResource", Count = 1 },
                new { Type = "BabauWeaponModeAi36", Count = 1 },
                new { Type = "BabauDispelAi36", Count = 1 },
                new { Type = "InfernalSpellAi35", Count = 3 }
            };
            Assertions.Equal(privateIdentities.Length,
                expectedPrivateTypes.Sum(value => value.Count),
                "The Sprint 36 private type inventory is complete.");
            foreach (var expected in expectedPrivateTypes)
                Assertions.Equal(expected.Count, privateIdentities.Count(value =>
                        value.PlannedType == expected.Type),
                    "Exact Sprint 36 private type count changed for " +
                    expected.Type + ".");
            Assertions.Equal(SummoningCurrentSurface.Sprint36CatalogIdentityCount,
                privateIdentities.Length + routeSymbols.Length + 2,
                "The two units, seven roots, and private dependency graph are the complete active append.");
        }

        internal static void UnsupportedGaseousFormIdentitiesStayInert()
        {
            string[] symbols = {
                "KMG.Summoning.Infernal.Hezrou.GaseousForm",
                "KMG.Summoning.Infernal.Hezrou.GaseousFormBuff",
                "KMG.Summoning.Infernal.Hezrou.GaseousFormResource",
                "KMG.Summoning.Infernal.Hezrou.GaseousAi" };
            Assertions.True(symbols.All(symbol =>
                    !ExpandedSummoningIdentityCatalog.Build().Any(value =>
                        value.Symbol == symbol)),
                "Unresolved Gaseous Form shells are not runtime-registered.");
            var entries = JObject.Parse(File.ReadAllText(
                "blueprints/blueprints.json"))["entries"];
            Assertions.True(symbols.All(symbol => entries.Any(value =>
                    (string)value["symbol"] == symbol &&
                    (string)value["status"] == "reserved")),
                "Allocated Gaseous Form identities remain reserved in the ledger.");
        }

        internal static void OwnedBodiesDecodeAndRemainDistinct()
        {
            foreach (string key in new[] { InfernalVisualPolicy36.Babau,
                InfernalVisualPolicy36.Hezrou })
            {
                string path = "assets/sprint36-demons/" + key +
                    "-mesh.json";
                var payload = JObject.Parse(File.ReadAllText(path));
                OriginalMeshOrientation.AssertShippedBody(path, key);
                byte[] data = Convert.FromBase64String(
                    (string)payload["data"]);
                int vertices = (int)payload["vertexCount"];
                int triangles = (int)payload["triangleCount"];
                Assertions.Equal(vertices * 64 + triangles * 12,
                    data.Length,
                    "Sprint 36 decoder layout remains complete.");
                for (int index = 0; index < vertices * 8; index++)
                {
                    float value = BitConverter.ToSingle(data, index * 4);
                    Assertions.True(!float.IsNaN(value) &&
                        !float.IsInfinity(value),
                        "Sprint 36 vertex, normal, and UV values are finite.");
                }
                Assertions.True(payload["bones"].Values<string>()
                    .SequenceEqual(InfernalVisualPolicy36.Bones),
                    "Both bodies retain the exact measured rig order.");
                var albedo = payload["albedo"];
                using (var hash = SHA256.Create())
                {
                    string actual = string.Concat(hash.ComputeHash(
                        File.ReadAllBytes(Path.Combine(
                            Path.GetDirectoryName(path),
                            (string)albedo["file"]))).Select(value =>
                                value.ToString("x2")));
                    Assertions.Equal((string)albedo["sha256"], actual,
                        "Runtime albedo agrees with its Sprint 36 mesh.");
                }
                if (key == InfernalVisualPolicy36.Babau)
                {
                    Assertions.Equal("lean-horned-skirmisher",
                        (string)payload["distinctSilhouette"],
                        "Babau retains its lean horned silhouette.");
                    Assertions.True((bool)payload["protectiveSlimeSurface"] &&
                        !(bool)payload["toadBody"] &&
                        (int)payload["visibleHorns"] == 2 &&
                        (int)payload["visibleSlimeLobes"] == 4,
                        "Babau owns its slime-coated horned body details.");
                }
                else
                {
                    Assertions.Equal("broad-squat-toad-bruiser",
                        (string)payload["distinctSilhouette"],
                        "Hezrou retains its broad toad silhouette.");
                    Assertions.True(!(bool)payload["protectiveSlimeSurface"] &&
                        (bool)payload["toadBody"] &&
                        (int)payload["visibleHorns"] == 0 &&
                        (int)payload["visibleWarts"] == 7,
                        "Hezrou owns its distinct toad body details.");
                }
            }
            Assertions.True(InfernalVisualPolicy36.IsExactIdentity(
                InfernalVisualPolicy36.Babau,
                "1512476cf6674704b6b0bcc3b67b48d9",
                "KMG_Summoning_Unit_Babau",
                TheropodVisualPolicy.DonorPrefabGuid),
                "Only the exact Babau identity can receive its body.");
            Assertions.False(InfernalVisualPolicy36.IsExactIdentity(
                InfernalVisualPolicy36.Babau,
                "0c97e35a3e8e4d2db937a23775ea13ac",
                "KMG_Summoning_Unit_Hezrou",
                TheropodVisualPolicy.DonorPrefabGuid),
                "Hezrou cannot cross-bind the Babau body.");
        }

        internal static void PrintedDcsDeriveFromLiveAbilityScores()
        {
            Assertions.Equal(18,
                InfernalRules36Policy.ConstitutionDc(7, 5),
                "Babau Protective Slime uses its hit dice and live Constitution bonus.");
            Assertions.Equal(24,
                InfernalRules36Policy.ConstitutionDc(10, 9),
                "Hezrou stench uses its hit dice and live Constitution bonus.");
            Assertions.Equal(18,
                InfernalRules36Policy.SpellLikeDc(4, 18),
                "Hezrou fourth-level spell-like abilities use live Charisma.");
            InfernalRules36Policy.Validate();
        }

        internal static void ProtectiveSlimeOnlyQualifiesNaturalContact()
        {
            Assertions.True(InfernalRules36Policy.ProtectiveSlimeNaturalStrike(
                true, true, true, false),
                "A successful natural melee strike qualifies.");
            Assertions.True(InfernalRules36Policy.ProtectiveSlimeNaturalStrike(
                true, true, false, true),
                "A successful unarmed melee strike qualifies.");
            Assertions.False(InfernalRules36Policy.ProtectiveSlimeNaturalStrike(
                false, true, true, false),
                "A miss cannot trigger Protective Slime.");
            Assertions.False(InfernalRules36Policy.ProtectiveSlimeNaturalStrike(
                true, false, true, false),
                "A ranged natural strike cannot trigger Protective Slime.");
            Assertions.False(InfernalRules36Policy.ProtectiveSlimeNaturalStrike(
                true, true, false, false),
                "A manufactured melee weapon is not redirected to its wielder.");
        }

        internal static void DispelNeedsARealUsefulEffect()
        {
            Assertions.True(InfernalRules36Policy.UsefulDispelEffect(
                true, true, true, false, true, false),
                "An active beneficial spell effect with context and no no-dispel flag is useful.");
            Assertions.False(InfernalRules36Policy.UsefulDispelEffect(
                false, true, true, false, true, false),
                "An inactive effect is not useful.");
            Assertions.False(InfernalRules36Policy.UsefulDispelEffect(
                true, true, false, false, true, false),
                "A non-spell effect is not a Dispel target.");
            Assertions.False(InfernalRules36Policy.UsefulDispelEffect(
                true, true, true, true, true, false),
                "A no-dispel effect is not useful.");
            Assertions.False(InfernalRules36Policy.UsefulDispelEffect(
                true, true, true, false, false, false),
                "An effect without casting context cannot support a real check.");
            Assertions.False(InfernalRules36Policy.UsefulDispelEffect(
                true, false, true, false, true, false),
                "A hidden effect is not known enough for autonomous selection.");
            Assertions.False(InfernalRules36Policy.UsefulDispelEffect(
                true, true, true, false, true, true),
                "An otherwise dispellable effect that harms the enemy carrying it is not worth removing for that enemy.");
        }

        internal static void StenchAndHeldNauseaKeepSeparateState()
        {
            Assertions.True(InfernalRules36Policy.CanExposeStench(
                true, true, false, false),
                "An eligible entrant without same-source memory is exposed.");
            Assertions.False(InfernalRules36Policy.CanExposeStench(
                true, true, false, true),
                "Same-source stench memory prevents repeated exposure.");
            Assertions.False(InfernalRules36Policy.CanExposeStench(
                true, true, true, false),
                "Poison immunity blocks stench.");
            Assertions.False(InfernalRules36Policy.CanExposeStench(
                true, false, false, false),
                "A different source cannot consume this source's exposure state.");

            Assertions.True(InfernalRules36Policy.CanTestHeldNausea(
                true, true, true, false),
                "A victim actively held by this Hezrou can test held nausea.");
            Assertions.False(InfernalRules36Policy.CanTestHeldNausea(
                true, true, false, false),
                "Stench exposure alone is not held-victim nausea.");
            Assertions.False(InfernalRules36Policy.CanTestHeldNausea(
                true, false, true, false),
                "Another owner's hold cannot trigger this source.");
            Assertions.False(InfernalRules36Policy.CanTestHeldNausea(
                true, true, true, true),
                "Poison immunity separately blocks held nausea.");
        }

        internal static void HezrouDeclinesGrabOnlyDuringPrivateCleave()
        {
            Assertions.True(InfernalRules36Policy.DeclineGrabDuringCleave(
                true, true),
                "The exact Hezrou may decline optional Grab on a private Cleave attack so the chain can continue.");
            Assertions.False(InfernalRules36Policy.DeclineGrabDuringCleave(
                true, false),
                "The same Hezrou retains Grab on ordinary attacks.");
            Assertions.False(InfernalRules36Policy.DeclineGrabDuringCleave(
                false, true),
                "Other Grab owners do not inherit the Hezrou's Cleave choice.");
        }

        internal static void AlignmentBurstsKeepSaveAndSusceptibilityDistinct()
        {
            Assertions.Equal(40,
                InfernalRules36Policy.AlignmentBurstDamage(40,
                    true, false, false),
                "A primary susceptible target that fails its save takes full damage.");
            Assertions.Equal(20,
                InfernalRules36Policy.AlignmentBurstDamage(40,
                    true, false, true),
                "A successful save halves primary damage.");
            Assertions.Equal(20,
                InfernalRules36Policy.AlignmentBurstDamage(40,
                    false, false, false),
                "A secondary susceptible target starts at half damage.");
            Assertions.Equal(10,
                InfernalRules36Policy.AlignmentBurstDamage(40,
                    false, false, true),
                "A secondary target's successful save halves that reduced damage.");
            Assertions.Equal(0,
                InfernalRules36Policy.AlignmentBurstDamage(40,
                    true, true, false),
                "An alignment-exempt target takes no damage.");
            Assertions.True(InfernalRules36Policy.AlignmentBurstCondition(
                true, false, false),
                "Only a primary susceptible target that fails receives the condition.");
            Assertions.False(InfernalRules36Policy.AlignmentBurstCondition(
                true, false, true),
                "A successful save prevents the condition.");
            Assertions.False(InfernalRules36Policy.AlignmentBurstCondition(
                false, false, false),
                "A secondary target does not receive the primary condition.");
        }
    }
}
