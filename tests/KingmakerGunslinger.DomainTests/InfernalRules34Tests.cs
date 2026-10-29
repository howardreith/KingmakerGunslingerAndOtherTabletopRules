using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class InfernalRules34Tests
    {
        internal static void HiddenRoutesHaveExactScopeAndQuantity()
        {
            foreach (string key in new[] { "bearded-devil", "kyton-chain-devil" }) {
                var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == key);
                Assertions.False(creature.NaturesAllyTier.HasValue || creature.MonsterTemplated, "Already aligned outsiders have no Nature's Ally or template routes.");
                var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Where(value => value.Creature.Key == key).OrderBy(value => value.ParentTier).ToArray();
                Assertions.True(routes.Select(value => value.ParentTier).SequenceEqual(new[] { 5, 6, 7, 8, 9 }), "Exactly SM V through IX.");
                Assertions.True(routes.Select(value => value.Multiplicity).SequenceEqual(new[] {
                    SummonMultiplicity.One, SummonMultiplicity.OneD3, SummonMultiplicity.OneD4PlusOne,
                    SummonMultiplicity.OneD4PlusOne, SummonMultiplicity.OneD4PlusOne }), "Printed quantity propagation.");
                Assertions.True(routes.All(value => !SummonVisibilityCatalog.IsPublished(value)), "Early candidate routes remain hidden.");
                Assertions.True(!string.IsNullOrEmpty(SummonChoicePresentationPolicy.For(key).Label), "Each registered creature has its own menu identity.");
            }
            SummonVisibilityCatalog.Validate(); SummonChoicePresentationPolicy.Validate();
        }

        internal static void OwnedBodiesDecodeAndHaveMatchingAlbedos()
        {
            foreach (string key in new[] { "bearded-devil", "kyton-chain-devil" }) {
                string path = "assets/sprint34-infernal-outsiders/" + key + "-mesh.json";
                var payload = JObject.Parse(File.ReadAllText(path));
                OriginalMeshOrientation.AssertShippedBody(path, key);
                var data = Convert.FromBase64String((string)payload["data"]);
                int vertices = (int)payload["vertexCount"], triangles = (int)payload["triangleCount"];
                Assertions.Equal(vertices * 64 + triangles * 12, data.Length, "Decoder byte layout remains complete.");
                for (int index = 0; index < vertices * 8; index++) {
                    float value = BitConverter.ToSingle(data, index * 4);
                    Assertions.True(!float.IsNaN(value) && !float.IsInfinity(value), "Vertex, normal and UV values are finite.");
                }
                var albedo = payload["albedo"];
                using (var hash = SHA256.Create()) {
                    string actual = string.Concat(hash.ComputeHash(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(path), (string)albedo["file"]))).Select(value => value.ToString("x2")));
                    Assertions.Equal((string)albedo["sha256"], actual, "Runtime albedo agrees with its original mesh.");
                }
                var chains = (JArray)payload["chains"];
                foreach (var chain in chains) {
                    var indices = chain["vertices"].Values<int>().ToArray();
                    Assertions.True(indices.Length > 0 && indices.Distinct().Count() == indices.Length &&
                        indices.All(index => index >= 0 && index < vertices), "Each chain owns valid, unique render vertices.");
                }
            }
        }

        internal static void EarlyNativePhaseRetainsClosedAdmission()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32Phase("sprint34-infernal-early"), "Selected bounded phase admitted.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32Phase("Sprint34-infernal-early"), "Case variation refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32Phase("sprint34-infernal-early-extra"), "Broader suffix refused.");
        }

        internal static void BeardNeedsTwoDistinctClawsAndOneTarget()
        {
            var sequence = new BarbazuBeardSequence34();
            var command = new object(); var target = new object();
            Assertions.False(sequence.Record(new object(), command, command, 1, target, true), "First claw is not Beard.");
            Assertions.False(sequence.Record(new object(), command, command, 1, target, true), "Repeating one limb is not two claws.");
            Assertions.False(sequence.Record(new object(), command, command, 2, new object(), true), "Split targets do not qualify.");
            Assertions.True(sequence.Record(new object(), command, command, 2, target, true), "Both claws hit this opponent.");
            Assertions.False(sequence.Record(new object(), command, command, 1, target, true), "Only one Beard per attack sequence.");
        }

        internal static void BeardRejectsMissesAndDuplicateEvents()
        {
            var sequence = new BarbazuBeardSequence34();
            var command = new object(); var target = new object(); var missed = new object();
            sequence.Record(new object(), command, command, 1, target, true);
            Assertions.False(sequence.Record(missed, command, command, 2, target, false), "Miss does not qualify.");
            Assertions.False(sequence.Record(missed, command, command, 2, target, true), "Replay cannot turn a miss into a hit.");
            var hit = new object();
            Assertions.True(sequence.Record(hit, command, command, 2, target, true), "A later genuine second-claw hit qualifies.");
            sequence.Reset();
            sequence.Record(new object(), command, command, 1, target, true);
            Assertions.False(sequence.Record(hit, command, command, 2, target, true), "Reset does not authorize replay of an old notification.");
        }

        internal static void BeardRejectsStaleCommandsAndSiblingHits()
        {
            var first = new BarbazuBeardSequence34(); var sibling = new BarbazuBeardSequence34();
            var command = new object(); var next = new object(); var target = new object();
            first.Record(new object(), command, command, 1, target, true);
            Assertions.False(sibling.Record(new object(), command, command, 2, target, true), "Sibling has no first-claw hit.");
            Assertions.False(first.Record(new object(), command, next, 2, target, true), "Finished command cannot deliver late Beard.");
            Assertions.False(first.Record(new object(), next, next, 2, target, true), "New command has no first-claw hit.");
            Assertions.False(first.Record(new object(), null, null, 1, target, true), "Direct rules without commands never qualify.");
            first.Reset();
            Assertions.False(first.Record(new object(), next, next, 1, target, true), "Round boundary forgets the previous claw.");
        }

        internal static void PrintedAbilityModifiersStayLive()
        {
            Assertions.Equal(17, InfernalRules34.BeardDiseaseDc(4), "Barbazu baseline Con DC.");
            Assertions.Equal(19, InfernalRules34.BeardDiseaseDc(6), "Augmented Constitution affects the disease DC.");
            Assertions.Equal(15, InfernalRules34.GazeDc(1), "Kyton baseline Cha DC.");
            Assertions.Equal(12, InfernalRules34.GazeDc(-2), "Negative Charisma affects the gaze DC.");
            Assertions.Equal(2, InfernalRules34.BeardStrengthBonus(4), "Secondary Beard baseline damage.");
            Assertions.Equal(3, InfernalRules34.BeardStrengthBonus(7), "Positive half Strength rounds down.");
            Assertions.Equal(-3, InfernalRules34.BeardStrengthBonus(-3), "A Strength penalty is not halved.");
        }
    }
}
