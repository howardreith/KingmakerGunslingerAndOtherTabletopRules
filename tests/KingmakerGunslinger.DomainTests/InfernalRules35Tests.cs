using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class InfernalRules35Tests
    {
        internal static void HiddenRoutesHaveExactScopeAndQuantity()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy
                .SaveFreeParameterScope(RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint32NativeReview, 1,
                    ExpandedSummoningNativeProfilePolicy
                        .Sprint35InfernalEarlyPhase),
                "The bounded Sprint 35 early observer is admitted.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy
                .SaveFreeParameterScope(RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint32NativeReview, 1,
                    "sprint35-infernal-early-extra"),
                "A broader Sprint 35 observer suffix was admitted.");
            var barbed = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "barbed-devil");
            var bone = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "bone-devil");
            Assertions.False(barbed.NaturesAllyTier.HasValue ||
                barbed.MonsterTemplated, "Barbed is Monster-only and untemplated.");
            Assertions.False(bone.NaturesAllyTier.HasValue ||
                bone.MonsterTemplated, "Bone is Monster-only and untemplated.");
            var barbedRoutes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == barbed.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            var boneRoutes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == bone.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            Assertions.True(barbedRoutes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 8, 9 }), "Barbed has VIII and IX only.");
            Assertions.True(barbedRoutes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3 }),
                "Barbed quantities are one then 1d3.");
            Assertions.True(boneRoutes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 7, 8, 9 }),
                "Bone has VII through IX only.");
            Assertions.True(boneRoutes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3,
                    SummonMultiplicity.OneD4PlusOne }),
                "Bone quantities are one, 1d3, then 1d4+1.");
            Assertions.True(barbedRoutes.Concat(boneRoutes).All(value =>
                !SummonVisibilityCatalog.IsPublished(value)),
                "All five executable routes remain suppressed for early proof.");
            SummonVisibilityCatalog.Validate();
            SummonChoicePresentationPolicy.Validate();
        }

        internal static void ContactPredicatesStayDistinct()
        {
            Assertions.True(InfernalRules35.QualifiesForBarbedDefense(true,
                true, false, 20, 5),
                "A natural long-reach body strike is not a reach weapon.");
            Assertions.False(InfernalRules35.QualifiesForBarbedDefense(true,
                true, true, 10, 5),
                "A manufactured reach weapon is exempt.");
            Assertions.False(InfernalRules35.QualifiesForBarbedDefense(false,
                true, false, 5, 5), "A miss does not retaliate.");
            Assertions.False(InfernalRules35.QualifiesForBarbedDefense(true,
                false, false, 5, 5), "A ranged strike does not retaliate.");
            Assertions.True(InfernalRules35.QualifiesForClawFear(true, true, 1),
                "A wounding claw qualifies for fear.");
            Assertions.False(InfernalRules35.QualifiesForClawFear(true, true, 0),
                "An absorbed claw does not qualify for fear.");
            Assertions.False(InfernalRules35.QualifiesForClawFear(false, true, 10),
                "Other damage does not qualify for claw fear.");
        }

        internal static void LiveModifiersDerivePrintedContracts()
        {
            Assertions.Equal(20, InfernalRules35.FearDc(12, 4, 0),
                "Barbed baseline fear DC.");
            Assertions.Equal(20,
                InfernalRules35.BarbedBaseCriticalThreatMinimum,
                "The private claw type retains its native 20 threat edge.");
            Assertions.Equal(19,
                InfernalRules35.BarbedEffectiveCriticalThreatMinimum,
                "Improved Critical supplies the printed live 19-20 range.");
            Assertions.Equal(19, InfernalRules35.FearDc(10, 4, 0),
                "Bone baseline aura DC.");
            Assertions.Equal(18, InfernalRules35.FearDc(12, 2, 0),
                "Drained Charisma remains live.");
            Assertions.Equal(9, InfernalRules35.ImpaleStrengthBonus(6),
                "Printed impale Strength term.");
            Assertions.Equal(4, InfernalRules35.ImpaleStrengthBonus(3),
                "Positive half rounds down.");
            Assertions.Equal(-2, InfernalRules35.ImpaleStrengthBonus(-2),
                "A Strength penalty is not multiplied.");
            InfernalRules35.Validate();
        }

        internal static void UnholyBlightKeepsAlignmentAndSaveFractions()
        {
            Assertions.Equal(18, InfernalRules35.UnholyBlightWillDc(18),
                "Baseline Charisma gives the printed Unholy Blight DC.");
            Assertions.Equal(17, InfernalRules35.UnholyBlightWillDc(16),
                "The SLA DC follows a live Charisma reduction.");
            Assertions.Equal(5, InfernalRules35.UnholyBlightDiceCount(false),
                "Ordinary good and neutral creatures use five dice.");
            Assertions.Equal(8, InfernalRules35.UnholyBlightDieSides(false),
                "Ordinary damage uses d8s.");
            Assertions.Equal(10, InfernalRules35.UnholyBlightDiceCount(true),
                "Good outsiders use ten dice at CL12, capped by the spell.");
            Assertions.Equal(6, InfernalRules35.UnholyBlightDieSides(true),
                "Good outsider damage uses d6s.");
            Assertions.Equal(40, InfernalRules35.UnholyBlightDamage(40,
                true, false, false), "A failed good save takes full damage.");
            Assertions.Equal(20, InfernalRules35.UnholyBlightDamage(40,
                true, false, true), "A successful good save takes half.");
            Assertions.Equal(20, InfernalRules35.UnholyBlightDamage(40,
                false, false, false), "Neutral creatures take half.");
            Assertions.Equal(10, InfernalRules35.UnholyBlightDamage(40,
                false, false, true), "A neutral successful save takes quarter.");
            Assertions.Equal(0, InfernalRules35.UnholyBlightDamage(40,
                false, true, false), "Evil creatures are unaffected.");
            Assertions.True(InfernalRules35.UnholyBlightSickens(true,
                false, false), "Only a failed good save becomes sickened.");
            Assertions.False(InfernalRules35.UnholyBlightSickens(true,
                false, true), "A successful good save prevents sickness.");
            Assertions.False(InfernalRules35.UnholyBlightSickens(false,
                false, false), "Neutral creatures are never sickened.");
            Assertions.False(InfernalRules35.UnholyBlightSickens(false,
                true, false), "Evil creatures are never sickened.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                InfernalRules35.UnholyBlightDamage(-1, true, false, false),
                "Negative rolled damage is invalid input.");
            Assertions.Throws<ArgumentException>(() =>
                InfernalRules35.UnholyBlightDamage(1, true, true, false),
                "Contradictory alignment input is rejected.");
        }

        internal static void PoisonRequiresTheWoundingSting()
        {
            Assertions.True(InfernalRules35.PoisonQualifies(true, true, 1),
                "An exact wounding sting delivers poison.");
            Assertions.False(InfernalRules35.PoisonQualifies(true, true, 0),
                "Absorption prevents injury poison.");
            Assertions.False(InfernalRules35.PoisonQualifies(true, false, 0),
                "A miss never poisons.");
            Assertions.False(InfernalRules35.PoisonQualifies(false, true, 20),
                "Other limbs never poison.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                InfernalRules35.PoisonQualifies(true, true, -1),
                "Negative final damage is invalid input.");
        }

        internal static void PrivateWallUnitHasBoundedShellDonor()
        {
            Assertions.Equal(InfernalRules35.BoneUnitSymbol,
                InfernalRules35.PrivateUnitDonorSymbol(
                    InfernalRules35.WallSectionUnitSymbol),
                "The private wall section alone reuses the Bone unit donor shell.");
            Assertions.Equal(null,
                InfernalRules35.PrivateUnitDonorSymbol(
                    InfernalRules35.BoneUnitSymbol),
                "A public creature root remains in the normal donor catalog.");
            Assertions.Equal(null,
                InfernalRules35.PrivateUnitDonorSymbol(
                    "KMG.Summoning.Infernal.Unknown.PrivateUnit"),
                "An undeclared private unit cannot acquire a silent fallback.");
            Assertions.Throws<ArgumentNullException>(() =>
                InfernalRules35.PrivateUnitDonorSymbol(null),
                "A null private-unit identity is invalid input.");
        }

        internal static void OwnedBodiesDecodeAndTailIsIsolated()
        {
            foreach (string key in new[] { InfernalVisualPolicy35.Barbed,
                InfernalVisualPolicy35.Bone })
            {
                string path = "assets/sprint35-infernal-reach/" + key +
                    "-mesh.json";
                var payload = JObject.Parse(File.ReadAllText(path));
                OriginalMeshOrientation.AssertShippedBody(path, key);
                var data = Convert.FromBase64String((string)payload["data"]);
                int vertices = (int)payload["vertexCount"];
                int triangles = (int)payload["triangleCount"];
                Assertions.Equal(vertices * 64 + triangles * 12,
                    data.Length, "Sprint 35 decoder layout remains complete.");
                for (int index = 0; index < vertices * 8; index++)
                {
                    float value = BitConverter.ToSingle(data, index * 4);
                    Assertions.True(!float.IsNaN(value) &&
                        !float.IsInfinity(value),
                        "Sprint 35 vertex, normal, and UV values are finite.");
                }
                var albedo = payload["albedo"];
                using (var hash = SHA256.Create())
                {
                    string actual = string.Concat(hash.ComputeHash(
                        File.ReadAllBytes(Path.Combine(
                            Path.GetDirectoryName(path),
                            (string)albedo["file"]))).Select(value =>
                                value.ToString("x2")));
                    Assertions.Equal((string)albedo["sha256"], actual,
                        "Runtime albedo agrees with its Sprint 35 mesh.");
                }
                Assertions.True(payload["bones"].Values<string>()
                    .SequenceEqual(InfernalVisualPolicy35.Bones),
                    "Both bodies retain the exact measured rig order.");
                if (key == InfernalVisualPolicy35.Barbed)
                {
                    Assertions.Equal(20, (int)payload["visibleSpines"],
                        "Barbed has a materially spined authored body.");
                    Assertions.True(payload["tail"].Type == JTokenType.Null,
                        "Barbed does not acquire the Bone tail receipt.");
                    continue;
                }
                Assertions.Equal(2, (int)payload["visibleWings"],
                    "Bone retains its two authored wing-strut silhouettes.");
                var tail = payload["tail"];
                Assertions.Equal(6, (int)tail["segments"],
                    "Bone has one six-segment stinging tail.");
                int bone = Array.IndexOf(payload["bones"].Values<string>()
                    .ToArray(), (string)tail["bone"]);
                int[] indices = tail["vertices"].Values<int>().ToArray();
                Assertions.True(indices.Length >= 100 &&
                    indices.Distinct().Count() == indices.Length &&
                    indices.All(index => index >= 0 && index < vertices),
                    "Bone tail owns one valid, unique render region.");
                foreach (int index in indices)
                {
                    int offset = vertices * 32 + triangles * 12 + index * 32;
                    Assertions.Equal(bone, BitConverter.ToInt32(data, offset),
                        "Every deformable tail vertex belongs to its declared pelvis bone.");
                    Assertions.True(Math.Abs(BitConverter.ToSingle(data,
                        offset + 4) - 1f) < .0001f,
                        "Tail deformation has no mixed-bone drift.");
                }
            }
            Assertions.True(InfernalVisualPolicy35.IsExactIdentity(
                InfernalVisualPolicy35.Barbed,
                "a8ed4e16ec4b48a5bdfdc47f7f1b9cd0",
                "KMG_Summoning_Unit_BarbedDevil",
                TheropodVisualPolicy.DonorPrefabGuid),
                "Only the exact Barbed identity can receive its body.");
            Assertions.False(InfernalVisualPolicy35.IsExactIdentity(
                InfernalVisualPolicy35.Barbed,
                "74652de839a94feba3c2fb67b536437c",
                "KMG_Summoning_Unit_BoneDevil",
                TheropodVisualPolicy.DonorPrefabGuid),
                "Bone cannot cross-bind the Barbed body.");
        }
    }
}
