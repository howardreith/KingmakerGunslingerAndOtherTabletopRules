using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SalamanderHumanBindingTests
    {
        private sealed class Palette
        {
            internal string[] Names, Parents;
            internal int[] Ids;
            internal float[][] Binds;
            internal Palette()
            {
                Names = SalamanderHumanBindingPolicy.NativeNames.Concat(SalamanderHumanBindingPolicy.NativeNames)
                    .Concat(new[] { "Weapons", "Weapons" }).ToArray();
                Parents = Names.Select(SalamanderHumanBindingPolicy.Parent).ToArray();
                Ids = Enumerable.Range(0, Names.Length).Select(i => i < 52 ? i % 26 + 1 : 99).ToArray();
                Binds = Enumerable.Range(0, Names.Length).Select(_ => Enumerable.Range(0, 16)
                    .Select(i => i % 5 == 0 ? 1f : 0f).ToArray()).ToArray();
                // Unselected storage bindings may disagree; never used as a
                // driver, and never collapse their transforms into anatomy.
                Binds[53][12] = 2.8f;
            }
            internal bool Resolve(out int[] slots)
            { return SalamanderHumanBindingPolicy.TrySlots(Names, Ids, Parents, Binds, out slots); }
        }

        internal static void CompleteSelectedPaletteKeepsAllNativeReferences()
        {
            var row = new Palette(); int[] slots;
            Assertions.True(row.Resolve(out slots), "Both copies of every exact anatomical bind agree.");
            Assertions.True(slots.SequenceEqual(Enumerable.Range(0, 26)), "First slot selected only after checking every duplicate.");
            Assertions.Equal(36, SalamanderHumanBindingPolicy.Names.Length, "26 native plus 10 original; no storage/cape/leg.");
            string[] logical = SalamanderHumanBindingPolicy.NativeNames.Concat(SalamanderTailAnimationPolicy.TailNames).ToArray();
            string[] exported = SalamanderHumanBindingPolicy.Names;
            Assertions.True(exported.SequenceEqual(logical.OrderBy(name => name, StringComparer.Ordinal)),
                "The exact exporter sorts the skin palette.");
            Assertions.True(exported.Select(name => logical[SalamanderHumanBindingPolicy.BindingIndex(name)]).SequenceEqual(exported),
                "Every exported weight index resolves to its correct anatomical/original bind, regardless of authoring order.");
            Assertions.True(exported.Select(SalamanderHumanBindingPolicy.BindingIndex).OrderBy(i => i)
                .SequenceEqual(Enumerable.Range(0, 36)), "No lost or duplicate driver mapping.");
            Assertions.False(SalamanderHumanBindingPolicy.Names.Contains("Weapons"), "Storage is excluded.");
            Assertions.True(SalamanderHumanBindingPolicy.NativeNames.All(name =>
                SalamanderHumanBindingPolicy.Parent(name) != null), "Every exact driver has a reviewed parent.");
            row.Names[0] = "foreign";
            Assertions.True(row.Resolve(out slots) && slots[0] == 26, "One remaining complete exact driver is valid.");
        }

        internal static void DisagreeingOrAmbiguousSelectedDuplicatesFailClosed()
        {
            for (int i = 0; i < 26; i++)
            {
                int[] slots;
                var bind = new Palette(); bind.Binds[i + 26][12] = .01f;
                Assertions.False(bind.Resolve(out slots), "Any disagreeing selected duplicate rejects the whole binding.");
                var identity = new Palette(); identity.Ids[i + 26] = 501;
                Assertions.False(identity.Resolve(out slots), "Name equality cannot replace live Transform identity.");
                var parent = new Palette(); parent.Parents[i + 26] = "Weapons";
                Assertions.False(parent.Resolve(out slots), "Wrong anatomical parent rejected.");
                var missing = new Palette(); missing.Names[i] = missing.Names[i + 26] = "unselected";
                Assertions.False(missing.Resolve(out slots), "Missing exact driver rejected.");
            }
        }

        internal static void MalformedPaletteNeverCreatesPartialBindings()
        {
            foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                var bad = new Palette(); bad.Binds[0][0] = value; int[] slots;
                Assertions.False(bad.Resolve(out slots), "Nonfinite selected bind fails.");
                Assertions.True(slots == null, "No partially resolved slots escape.");
            }
            var row = new Palette(); int[] result;
            row.Binds[0] = new float[15];
            Assertions.False(row.Resolve(out result), "Matrix must have sixteen cells.");
            row = new Palette(); row.Binds[0] = null;
            Assertions.False(row.Resolve(out result), "Null matrix rejected.");
            row = new Palette(); row.Ids[0] = row.Ids[26] = 0;
            Assertions.False(row.Resolve(out result), "No null Transform identity.");
            row = new Palette(); row.Ids[1] = row.Ids[27] = row.Ids[0];
            Assertions.False(row.Resolve(out result), "Two anatomical drivers cannot be one Transform.");
            Assertions.False(SalamanderHumanBindingPolicy.TrySlots(null, null, null, null, out result), "No missing palette.");
        }

        internal static void NativeSetGuardDistinguishesRawTailFromExactSlamFallback()
        {
            object slam = new object(), foreign = new object();
            Assertions.True(SalamanderHumanBindingPolicy.IsReviewedEffectiveTail<object>(null, slam),
                "Unpatched missing Tail is reviewed.");
            Assertions.True(SalamanderHumanBindingPolicy.IsReviewedEffectiveTail(slam, slam),
                "Exact borrowed native Slam fallback is reviewed, not adopted.");
            Assertions.False(SalamanderHumanBindingPolicy.IsReviewedEffectiveTail(foreign, slam),
                "A same-name or foreign action cannot replace exact reference identity.");
            Assertions.False(SalamanderHumanBindingPolicy.IsReviewedEffectiveTail(slam, (object)null),
                "No known native Slam means a non-null result remains unreviewed.");
            foreach (bool exact in new[] { false, true })
            foreach (bool rawTail in new[] { false, true })
            foreach (bool reviewed in new[] { false, true })
            {
                string expected = !exact ? "not-exact-native-human-set" : rawTail ?
                    "existing-native-tail-action" : !reviewed ? "unreviewed-effective-tail-lookup" : null;
                Assertions.Equal(expected, SalamanderHumanBindingPolicy.NativeSetRejection(exact, rawTail, reviewed),
                    "Only exact human, absent raw Tail and reviewed effective lookup may bind.");
            }
        }

        internal static void PatchMetadataUsesRegistryWithoutMaskingRegisteredFailures()
        {
            int calls = 0;
            object expected = new object();
            Func<object> read = () => { calls++; return expected; };
            Assertions.True(SalamanderHumanBindingPolicy.ReadRegisteredPatchMetadata(false, read) == null,
                "Absent registry entry has no patch metadata.");
            Assertions.Equal(0, calls, "Never call the Harmony bridge for an unregistered target.");
            Assertions.True(ReferenceEquals(expected,
                SalamanderHumanBindingPolicy.ReadRegisteredPatchMetadata(true, read)),
                "Registered metadata returned unchanged.");
            Assertions.Equal(1, calls, "One query for one registered target.");
            bool propagated = false;
            try { SalamanderHumanBindingPolicy.ReadRegisteredPatchMetadata<object>(true,
                () => { throw new NullReferenceException("bridge failure"); }); }
            catch (NullReferenceException) { propagated = true; }
            Assertions.True(propagated, "Do not relabel a registered-method observation failure as absence or PASS.");
        }

        internal static void AuxiliarySuppressionAcceptsOnlyTheAuditedNativeCape()
        {
            Assertions.True(SalamanderHumanBindingPolicy.IsReviewedAuxiliary(
                "Cape_Red_M(Clone)", "CP_Cape2Sided_M_Any", 0), "Exact observed zero-bone cape only.");
            foreach (string renderer in new[] { null, "", SalamanderHumanBindingPolicy.BodyName,
                "Spear", "Cape_Red_M", "Cape_Blue_M(Clone)" })
                Assertions.False(SalamanderHumanBindingPolicy.IsReviewedAuxiliary(
                    renderer, "CP_Cape2Sided_M_Any", 0), "Never suppress body, equipment or an unreviewed renderer.");
            foreach (string mesh in new[] { null, "", "CP_Cape2Sided_M_Any(Clone)", "ForeignMesh" })
                Assertions.False(SalamanderHumanBindingPolicy.IsReviewedAuxiliary(
                    "Cape_Red_M(Clone)", mesh, 0), "A familiar renderer name cannot authorize a different mesh.");
            foreach (int bones in new[] { -1, 1, 36, 1776 })
                Assertions.False(SalamanderHumanBindingPolicy.IsReviewedAuxiliary(
                    "Cape_Red_M(Clone)", "CP_Cape2Sided_M_Any", bones), "Changed native cape contract fails closed.");
        }

        internal static void GripSurfaceUsesOnlyTheSelectedHandAndItsThreeFingers()
        {
            foreach (string side in new[] { "L", "R" })
            {
                string[] selected = SalamanderHumanBindingPolicy.Names.Where(name =>
                    SalamanderHumanBindingPolicy.IsGripDriver(name, side)).ToArray();
                string[] expected = new[] { side + "_Hand" }.Concat(Enumerable.Range(1, 3).SelectMany(digit =>
                    new[] { side + "_Toe_" + digit + "_01", side + "_Toe_" + digit + "_02" })).ToArray();
                Assertions.True(selected.OrderBy(n => n).SequenceEqual(expected.OrderBy(n => n)),
                    "Only this exact anatomical hand/finger surface; no forearm, opposite hand or tail.");
                foreach (string name in new[] { null, "", side + "_Toe_4_01", side + "_Toe_1_03",
                    side + "_Hand_ADJ", side + "_ForeArm", "Weapons", "KMG_SalamanderTail09" })
                    Assertions.False(SalamanderHumanBindingPolicy.IsGripDriver(name, side),
                        "Never satisfy grip contact with another body region or an unreviewed driver.");
            }
            foreach (string side in new[] { null, "", "l", "Both" })
                Assertions.False(SalamanderHumanBindingPolicy.IsGripDriver("L_Hand", side), "Exact side required.");
        }

        internal static void GripSurfaceInfluenceCombinesOnlyOneExactHand()
        {
            string[] names = { "L_Hand", "L_Toe_1_01", "R_Hand", "L_ForeArm" };
            Assertions.True(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("L", names, new[] { .3f, .2f, .1f, .4f }),
                "Exactly half combined left-hand influence qualifies, including finger influence.");
            Assertions.False(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("L", names, new[] { .3f, .19f, .11f, .4f }),
                "Neither forearm nor opposite hand can complete the half-weight requirement.");
            Assertions.False(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("R", names, new[] { .3f, .2f, .1f, .4f }),
                "Sides remain independent on the same four weights.");
            Assertions.True(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("R", names, new[] { 0f, 0f, 1f, 0f }),
                "Zero-weight slots do not change a fully right-hand vertex.");
        }

        internal static void MalformedGripSurfaceWeightsFailClosed()
        {
            string[] names = { "L_Hand", "L_Toe_1_01", "L_Toe_1_02", "L_Hand" };
            foreach (float[] weights in new[] { null, new float[0], new[] { 1f }, new[] { .5f, 0f, 0f, 0f },
                new[] { .5f, .5f, .5f, .5f }, new[] { 1.1f, -.1f, 0f, 0f },
                new[] { float.NaN, 1f, 0f, 0f }, new[] { float.PositiveInfinity, 0f, 0f, 0f } })
                Assertions.False(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("L", names, weights),
                    "Malformed, non-finite, negative or unnormalized skinning is not contact evidence.");
            foreach (string[] invalid in new[] { null, new string[0], new[] { "L_Hand" },
                new[] { "Weapons", "L_ForeArm", "R_Hand", "KMG_SalamanderTail09" } })
                Assertions.False(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("L", invalid, new[] { 1f, 0f, 0f, 0f }),
                    "Missing or foreign anatomical palettes cannot satisfy a hand surface.");
        }

        internal static void GripRejectionCensusDoesNotNormalizeOrRelaxSelection()
        {
            string[] names = { "L_Hand", "L_Toe_1_01", "R_Hand", "L_ForeArm" };
            float[][] weights = { null, new float[0], new[] { float.NaN, 0f, 0f, 0f },
                new[] { 1.1f, -.1f, 0f, 0f }, new[] { .996f, 0f, 0f, 0f }, new float[4],
                new[] { .3f, .19f, .11f, .4f }, new[] { .3f, .2f, .1f, .4f } };
            string[] expected = { "array-shape", "array-shape", "non-finite", "weight-range",
                "weight-sum", "weight-sum", "below-hand-influence", "selected" };
            for (int i = 0; i < weights.Length; i++)
            {
                float[] before = weights[i] == null ? null : (float[])weights[i].Clone();
                Assertions.Equal(expected[i], SalamanderHumanBindingPolicy.GripSurfaceDisposition("L", names, weights[i]),
                    "Distinguish native metadata rejection without normalization or weaker anatomical selection.");
                Assertions.Equal(expected[i] == "selected", SalamanderHumanBindingPolicy.IsGripSurfaceVertex("L", names, weights[i]),
                    "Diagnostic disposition and unchanged selection agree.");
                if (before != null) Assertions.True(before.SequenceEqual(weights[i]), "Borrowed weights remain unchanged.");
            }
            Assertions.Equal("array-shape", SalamanderHumanBindingPolicy.GripSurfaceDisposition("L", null, new[] { 1f, 0f, 0f, 0f }),
                "Missing names remain invalid.");
            Assertions.Equal("below-hand-influence", SalamanderHumanBindingPolicy.GripSurfaceDisposition("R", names, new[] { 1f, 0f, 0f, 0f }),
                "Wrong-side anatomy cannot be accepted by the diagnostic.");
        }

        internal static void NativeGripDeformersRequireExactAnatomicalParents()
        {
            string[] hands = SalamanderHumanBindingPolicy.NativeNames.Where(name =>
                SalamanderHumanBindingPolicy.IsGripDriver(name, "L") || SalamanderHumanBindingPolicy.IsGripDriver(name, "R")).ToArray();
            Assertions.Equal(14, hands.Length, "Exactly seven native hand/finger deformers per side.");
            foreach (string driver in hands)
            {
                Assertions.Equal(driver, SalamanderHumanBindingPolicy.NativeGripDeformerDriver(driver + "_ADJ", driver),
                    "Only the captured exact ADJ child projects to its own animation driver.");
                Assertions.Equal(null, SalamanderHumanBindingPolicy.NativeGripDeformerDriver(driver, driver),
                    "Native animation-driver slots are not substituted for deforming slots.");
                foreach (string parent in new[] { null, "", "Weapons", driver + "_ADJ", driver.ToLowerInvariant() })
                    Assertions.Equal(null, SalamanderHumanBindingPolicy.NativeGripDeformerDriver(driver + "_ADJ", parent),
                        "Missing, storage, nested or wrong-case parents fail closed.");
            }
            foreach (string name in new[] { null, "", "L_Hand_adj", "L_Hand_ADJ_ADJ", "L_ForeArm_ADJ", "L_toe_ADJ", "Weapons_ADJ" })
                Assertions.Equal(null, SalamanderHumanBindingPolicy.NativeGripDeformerDriver(name,
                    name == null || name.Length < 4 ? null : name.Substring(0, name.Length - 4)),
                    "No suffix-only broad anatomy match.");
        }

        internal static void NativeDeformerProjectionPreservesSideAndInfluence()
        {
            string[] names = new[] { "L_Hand", "L_Toe_1_01", "R_Hand", "L_ForeArm" }
                .Select(name => SalamanderHumanBindingPolicy.NativeGripDeformerDriver(name + "_ADJ", name)).ToArray();
            Assertions.True(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("L", names, new[] { .3f, .2f, .1f, .4f }),
                "Reviewed left deformers combine at the unchanged half-weight requirement.");
            Assertions.False(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("L", names, new[] { .3f, .19f, .11f, .4f }),
                "Neither forearm nor opposite-hand weight can complete the requirement.");
            Assertions.False(SalamanderHumanBindingPolicy.IsGripSurfaceVertex("R", names, new[] { .3f, .2f, .1f, .4f }),
                "The projection cannot mix left and right.");
            Assertions.True(SalamanderHumanBindingPolicy.IsGripDriver("L_Hand", "L"),
                "The original mesh's animation-driver selection is unchanged.");
            Assertions.False(SalamanderHumanBindingPolicy.IsGripDriver("L_Hand_ADJ", "L"),
                "Native deformers are never added to the original mesh's driver contract.");
        }

        private sealed class PairedGripFixture
        {
            internal readonly JArray Timeline = new JArray(), Events = new JArray();
            internal readonly JObject[] Contacts = new JObject[2];
            internal PairedGripFixture(string clip = "Human_2H_spear_attack_02")
            {
                for (int handle = 0; handle < 2; handle++)
                {
                    bool release = clip.EndsWith("02", StringComparison.Ordinal);
                    Events.Add(new JObject { ["handleId"] = handle, ["clip"] = clip,
                        ["eventInvoked"] = false, ["cacheProviderInvoked"] = false,
                        ["events"] = new JArray(new JObject { ["time"] = release ? .734528542 : .620427966,
                            ["method"] = "Kingmaker.Visual.Animation.AnimationClipEventsCache+<>c.<CreateEvent>b__3_12" }) });
                    for (int frame = 0; frame < 6; frame++)
                    {
                        var sample = new JObject { ["frame"] = handle * 10 + frame, ["handleId"] = handle,
                            ["available"] = true, ["clip"] = clip, ["acted"] = frame >= 2,
                            ["state"] = release && frame >= 2 ? "TransitioningOut" : "Playing",
                            ["clipTime"] = (release ? .66 : .55) + frame * .06,
                            ["handleTime"] = (release ? .70 : .59) + frame * .06,
                            ["nativeLeft"] = release && frame >= 2 ? .208161578 : .003,
                            ["originalLeft"] = release && frame >= 2 ? .206292585 : .018,
                            ["nativeRight"] = .008956633, ["originalRight"] = .0064166286 };
                        foreach (string phase in new[] { "LateUpdate", "EndOfFrame" })
                        { var row = (JObject)sample.DeepClone(); row["phase"] = phase; Timeline.Add(row); }
                        if (frame == 2)
                        {
                            var rule = (JObject)sample.DeepClone(); rule["phase"] = "rule-event";
                            var rendered = (JObject)sample.DeepClone(); rendered["phase"] = "end-of-rule-frame";
                            Contacts[handle] = new JObject { ["samePoseGripControl"] = rule,
                                ["endOfFrame"] = new JObject { ["sameFrameAndHandle"] = true, ["samePoseGripControl"] = rendered } };
                        }
                    }
                }
            }
            internal string Review() { return RuntimeTesting.Sprint17GripEvidence.SpearGripRejection(Timeline, Events, Contacts); }
        }

        internal static void NativeGripFollowDistinguishesContactFromAuditedLeadRelease()
        {
            foreach (string clip in new[] { "Human_2H_spear_attack_01", "Human_2H_spear_attack_02" })
            {
                var row = new PairedGripFixture(clip);
                string before = row.Timeline.ToString();
                Assertions.Equal(null, row.Review(), "Native01 both-hand grip and native02 same-pose lead release preserve weapon handling.");
                Assertions.Equal(before, row.Timeline.ToString(), "Validation never changes measurements or poses.");
            }
            var native02 = new PairedGripFixture();
            Assertions.True((double)native02.Contacts[0]["samePoseGripControl"]["originalLeft"] > .08,
                "Do not mislabel observed native-following release as both hands touching the shaft.");
            var unreviewed = new PairedGripFixture("Human_2H_spear_attack_01");
            unreviewed.Contacts[0]["samePoseGripControl"]["nativeLeft"] = .21;
            unreviewed.Contacts[0]["samePoseGripControl"]["originalLeft"] = .20;
            Assertions.Equal("unreviewed-rule-frame-release", unreviewed.Review(), "No general permission for any clip to release at its rule event.");
        }

        internal static void NativeGripFollowRejectsLostGripAndAnyDivergentFrame()
        {
            foreach (string key in new[] { "nativeRight", "originalRight" })
            {
                var row = new PairedGripFixture(); row.Timeline[7][key] = .081;
                Assertions.Equal("weapon-hand-lost-grip", row.Review(), "An intact neighboring frame never hides a lost weapon hand.");
            }
            var lost = new PairedGripFixture(); lost.Timeline[0]["nativeLeft"] = .079; lost.Timeline[0]["originalLeft"] = .081;
            Assertions.Equal("lead-hand-lost-native-grip", lost.Review(), "Even a2mm differential cannot excuse losing a grip native retains.");
            var different = new PairedGripFixture(); different.Timeline[7]["originalLeft"] = .30;
            Assertions.Equal("lead-hand-diverges-from-native", different.Review(), "Separation must follow the simultaneous native control within unchanged8cm.");
            var rule = new PairedGripFixture(); rule.Contacts[0]["samePoseGripControl"]["originalRight"] = .081;
            Assertions.Equal("weapon-hand-lost-grip", rule.Review(), "Actual mechanical frame remains mandatory even with an intact timeline.");
            var rendered = new PairedGripFixture(); rendered.Contacts[0]["endOfFrame"]["samePoseGripControl"]["originalRight"] = .081;
            Assertions.Equal("weapon-hand-lost-grip", rendered.Review(), "Same-frame rendered pose remains mandatory too.");
        }

        internal static void NativeGripFollowRejectsMissingReorderedOrFabricatedEvidence()
        {
            var missing = new PairedGripFixture(); missing.Timeline[7]["available"] = false;
            Assertions.True(missing.Review() != null, "No missing native control can imply successful comparison.");
            foreach (double invalid in new[] { double.NaN, double.PositiveInfinity, -.001 })
            {
                var row = new PairedGripFixture(); row.Timeline[7]["nativeLeft"] = invalid;
                Assertions.Equal("non-finite-or-negative-sample", row.Review(), "Malformed distances cannot satisfy contact.");
            }
            var skipped = new PairedGripFixture(); skipped.Timeline.RemoveAt(3); skipped.Timeline.RemoveAt(2);
            Assertions.Equal("missing-reordered-or-duplicate-frame", skipped.Review(), "Dropping a whole intervening frame cannot cherry-pick contact.");
            var phase = new PairedGripFixture(); phase.Timeline[1]["phase"] = "LateUpdate";
            Assertions.Equal("missing-reordered-or-duplicate-frame", phase.Review(), "Two LateUpdates cannot stand in for rendered EndOfFrame evidence.");
            var reversed = new PairedGripFixture(); reversed.Timeline[8]["acted"] = false;
            Assertions.Equal("missing-or-reversed-act-playback", reversed.Review(), "IsActed cannot revert inside one native attack handle.");
            var rewound = new PairedGripFixture(); rewound.Timeline[8]["clipTime"] = .1;
            Assertions.Equal("missing-or-reversed-act-playback", rewound.Review(), "Reordered playback cannot choose an earlier intact pose.");
            var noGrip = new PairedGripFixture();
            foreach (JObject sample in noGrip.Timeline) { sample["nativeLeft"] = .21; sample["originalLeft"] = .20; }
            Assertions.Equal("missing-grip-or-act-transition", noGrip.Review(), "Following a permanently separated hand is not a two-hand attack.");
            var unrelated = new PairedGripFixture(); unrelated.Contacts[0]["endOfFrame"]["sameFrameAndHandle"] = false;
            Assertions.Equal("uncorrelated-rule-frame", unrelated.Review(), "A different rendered frame cannot replace the actual rule frame.");
            Assertions.True(RuntimeTesting.Sprint17GripEvidence.SpearGripRejection(null, null, null) != null, "Missing entire review fails closed.");
        }

        internal static void NativeGripFollowPinsReadOnlyNativeActIdentity()
        {
            foreach (string field in new[] { "eventInvoked", "cacheProviderInvoked" })
            {
                var row = new PairedGripFixture(); row.Events[0][field] = true;
                Assertions.Equal("unreviewed-native-event", row.Review(), "No invoked event/cache lookup is valid passive evidence.");
            }
            var time = new PairedGripFixture(); time.Events[0]["events"][0]["time"] = .60;
            Assertions.Equal("changed-native-act-event", time.Review(), "Never move native02's act time back to a convenient grip pose.");
            var method = new PairedGripFixture(); method.Events[0]["events"][0]["method"] = "SomeOtherEvent";
            Assertions.Equal("changed-native-act-event", method.Review(), "A footstep or sound cannot replace IsActed.");
            var early = new PairedGripFixture(); early.Contacts[0]["samePoseGripControl"]["clipTime"] = .70;
            Assertions.Equal("uncorrelated-rule-frame", early.Review(), "No favorable pre-act frame may replace real rule contact.");
            var unknown = new PairedGripFixture(); unknown.Contacts[0]["samePoseGripControl"]["clip"] = "Human_2H_spear_attack_03";
            Assertions.Equal("unreviewed-native-event", unknown.Review(), "Exact two captured native clips only.");
        }

        private sealed class CounterArrayConverter : JsonConverter
        {
            internal int Writes;
            public override bool CanConvert(Type type) { return type == typeof(Dictionary<string, int>); }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            { Writes++; writer.WriteStartArray(); writer.WriteValue("runtime dictionary converter"); writer.WriteEndArray(); }
            public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
            { throw new NotSupportedException(); }
        }

        internal static void GripCountersIgnoreProcessGlobalDictionaryConverters()
        {
            Func<JsonSerializerSettings> prior = JsonConvert.DefaultSettings;
            var converter = new CounterArrayConverter();
            var counts = new Dictionary<string, int> { { "weight-sum", 3 }, { "selected", 17 }, { "below-hand-influence", 0 } };
            try
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings { Converters = { converter } };
                bool oldRejected = false;
                try { JObject.FromObject(counts); } catch (ArgumentException) { oldRejected = true; }
                Assertions.True(oldRejected, "Reproduce the observed object-versus-array failure under runtime dictionary conversion.");
                Assertions.Equal(1, converter.Writes, "The old path invoked the hostile process-global converter.");
                JObject result = RuntimeTesting.Sprint17GripEvidence.Counters(counts);
                Assertions.True(result.Properties().Select(p => p.Name).SequenceEqual(new[] { "below-hand-influence", "selected", "weight-sum" }),
                    "All counter keys remain an explicit deterministic object.");
                JObject roundTrip = JObject.Parse(result.ToString(Formatting.None));
                foreach (var pair in counts) Assertions.Equal(pair.Value, (int)roundTrip[pair.Key], "Preserve exact counts, including zero.");
                Assertions.Equal(0, RuntimeTesting.Sprint17GripEvidence.Counters(new Dictionary<string, int>()).Count,
                    "An empty map is still an object, never an inferred array.");
                Assertions.Equal(1, converter.Writes, "The repaired path must not invoke process-global conversion.");
            }
            finally { JsonConvert.DefaultSettings = prior; }
            Assertions.True(ReferenceEquals(prior, JsonConvert.DefaultSettings), "Restore process defaults for unrelated domain tests.");
        }

        internal static void BakedControlRequiresTheExactFiniteLiveFrame()
        {
            float[] live = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 12, 3, -8, 1 };
            Assertions.True(SalamanderHumanBindingPolicy.BakeFrameMatches((float[])live.Clone(), live),
                "A separate non-rendering control can share the exact live world frame without writing the live transform.");
            for (int i = 0; i < 16; i++)
            {
                float[] changed = (float[])live.Clone(); changed[i] += .001f;
                Assertions.False(SalamanderHumanBindingPolicy.BakeFrameMatches(changed, live),
                    "Every matrix entry is checked; offsets, scale, rotation and shear cannot be approximated.");
                foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                {
                    changed = (float[])live.Clone(); changed[i] = invalid;
                    Assertions.False(SalamanderHumanBindingPolicy.BakeFrameMatches(changed, live), "Finite control required.");
                    Assertions.False(SalamanderHumanBindingPolicy.BakeFrameMatches(live, changed), "Finite live frame required.");
                }
            }
            Assertions.False(SalamanderHumanBindingPolicy.BakeFrameMatches(null, live), "No missing control.");
            Assertions.False(SalamanderHumanBindingPolicy.BakeFrameMatches(live, null), "No missing native frame.");
            Assertions.False(SalamanderHumanBindingPolicy.BakeFrameMatches(new float[15], live), "No partial matrix.");
            Assertions.False(SalamanderHumanBindingPolicy.BakeFrameMatches(live, new float[17]), "No unknown layout.");
        }

        internal static void OneTailAppendDoesNotReplaceOrMutateHumanActions()
        {
            object[] native = Enumerable.Range(0, 24).Select(_ => new object()).ToArray();
            object[] original = (object[])native.Clone(); object tail = new object();
            object[] owned = SalamanderHumanBindingPolicy.AppendOneTail(native, tail);
            Assertions.Equal(25, owned.Length, "Exactly one tail action appended.");
            Assertions.True(owned.Take(24).SequenceEqual(native) && ReferenceEquals(owned[24], tail), "Native ordering/references unchanged.");
            owned[0] = tail;
            Assertions.True(native.SequenceEqual(original), "Owned container cannot mutate shared action list.");
            foreach (object[] invalid in new[] { native.Take(23).ToArray(),
                native.Concat(new[] { new object() }).ToArray(),
                native.Select((value, i) => i == 3 ? null : value).ToArray(),
                native.Select((value, i) => i == 3 ? native[0] : value).ToArray() })
            {
                bool rejected = false;
                try { SalamanderHumanBindingPolicy.AppendOneTail(invalid, tail); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "Incomplete/duplicate/null native action list rejected.");
            }
            bool alias = false;
            try { SalamanderHumanBindingPolicy.AppendOneTail(native, native[0]); }
            catch (ArgumentException) { alias = true; }
            Assertions.True(alias, "A native action cannot be relabeled as our tail.");
        }
    }
}
