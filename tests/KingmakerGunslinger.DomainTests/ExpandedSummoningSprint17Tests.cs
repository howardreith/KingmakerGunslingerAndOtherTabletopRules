using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint17Tests
    {
        internal static void BindMatrixObservationKeepsDisagreementAndMissingData()
        {
            // Binary-exact cells keep the assertion about preservation, not
            // incidental decimal-to-float rounding in test construction.
            float[] first = Enumerable.Range(0, 16).Select(index => index / 8f).ToArray();
            var retained = (float[])first.Clone();
            Assertions.Equal((float?)0, SerpentineRigSurveyPolicy.MatrixDifference(first, retained),
                "Equal complete matrices have measured zero, not inferred agreement.");
            foreach (int index in Enumerable.Range(0, 16))
            {
                var changed = (float[])first.Clone(); changed[index] += .25f;
                Assertions.Equal((float?).25f, SerpentineRigSurveyPolicy.MatrixDifference(first, changed),
                    "Every differing cell survives as a nonzero observation.");
                Assertions.Equal((float?).25f, SerpentineRigSurveyPolicy.MatrixDifference(changed, first),
                    "Difference is symmetric; a negative change cannot disappear.");
                foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                {
                    changed[index] = invalid;
                    Assertions.False(SerpentineRigSurveyPolicy.MatrixDifference(first, changed).HasValue,
                        "Nonfinite input is unknown, never compatibility.");
                    Assertions.False(SerpentineRigSurveyPolicy.MatrixDifference(changed, first).HasValue,
                        "Both matrix inputs are checked.");
                }
            }
            foreach (float[] bad in new[] { null, new float[0], new float[15], new float[17] })
            {
                Assertions.False(SerpentineRigSurveyPolicy.MatrixDifference(first, bad).HasValue,
                    "Only complete 4x4 observations are meaningful.");
                Assertions.False(SerpentineRigSurveyPolicy.MatrixDifference(bad, first).HasValue,
                    "A missing baseline is not an all-zero matrix.");
            }
            Assertions.False(SerpentineRigSurveyPolicy.MatrixDifference(
                Enumerable.Repeat(float.MaxValue, 16).ToArray(), Enumerable.Repeat(-float.MaxValue, 16).ToArray()).HasValue,
                "Overflow is rejected even when both input cells are finite.");
            Assertions.True(first.SequenceEqual(retained), "The metadata comparison never changes either native observation.");
            var selected = SerpentineRigSurveyPolicy.SpearPrefabSources.Single(row => row[0] == SerpentineRigSurveyPolicy.HumanSpearBlueprint);
            Assertions.Equal(selected[1], SerpentineRigSurveyPolicy.HumanSpearPrefab,
                "Detailed binding research stays on one already observed spear prefab.");
        }

        internal static void SpearCensusPinsArchivedEquipmentWithoutAdoption()
        {
            var sources = SerpentineRigSurveyPolicy.SpearPrefabSources;
            Assertions.Equal(3, sources.Length, "Three distinct archived spear-bearing prefabs, not a new all-unit search.");
            Assertions.Equal(3, sources.Select(row => row[1]).Distinct().Count(), "No repeated prefab.");
            foreach (var source in sources)
            {
                Func<string[], bool, bool> allowed = (row, offhand) =>
                    SerpentineRigSurveyPolicy.MatchesSpearPrefab(row[0], row[1], row[2], row[3], offhand);
                Assertions.Equal(4, source.Length, "Blueprint, prefab, weapon and category are each pinned.");
                Assertions.True(allowed(source, false), "Exact source is only permission to observe, not adopt.");
                Assertions.False(allowed(source, true), "Unexpected offhand equipment is not silently ignored.");
                for (int i = 0; i < 4; i++)
                    foreach (string bad in new[] { null, "", "foreign", source[i].ToUpperInvariant() })
                    {
                        var changed = (string[])source.Clone(); changed[i] = bad;
                        Assertions.False(allowed(changed, false), "All identity fields are exact and case-sensitive.");
                    }
                foreach (var other in sources.Where(row => row[0] != source[0]))
                    for (int i = 0; i < 3; i++)
                    {
                        var changed = (string[])source.Clone(); changed[i] = other[i];
                        Assertions.False(allowed(changed, false), "Another allowed source cannot supply one field.");
                    }
                Assertions.False(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(source[0], source[1]),
                    "This does not broaden the eleven-row Lizardfolk authority or any action-adoption policy.");
            }
            var retained = (string[])sources[0].Clone(); sources[0][0] = "changed";
            sources[1] = new[] { "foreign" };
            Assertions.True(SerpentineRigSurveyPolicy.MatchesSpearPrefab(
                retained[0], retained[1], retained[2], retained[3], false),
                "Returned nested arrays cannot change the closed source authority.");
        }

        internal static void ActionMetadataPreservesMissingEmptyAndNullSlots()
        {
            Assertions.True(SerpentineRigSurveyPolicy.SnapshotMetadataSlots<object>(null) == null,
                "Absent enumeration stays unknown, not invented empty.");
            var empty = SerpentineRigSurveyPolicy.SnapshotMetadataSlots(new object[0]);
            Assertions.True(empty != null && empty.Length == 0, "An observed empty list stays explicitly empty.");
            var action = new object(); var input = new[] { null, action, null, action };
            var copy = SerpentineRigSurveyPolicy.SnapshotMetadataSlots(input);
            Assertions.False(ReferenceEquals(copy, input), "Capture cannot mutate the native source list.");
            Assertions.True(copy.SequenceEqual(input), "Retain exact slots, nulls and duplicate references.");
            copy[1] = null;
            Assertions.True(ReferenceEquals(input[1], action), "Snapshot edits leave original references intact.");
            Assertions.Equal(128, SerpentineRigSurveyPolicy.SnapshotMetadataSlots(new object[128]).Length,
                "A complete bounded all-null observation is valid metadata, not usable action proof.");
            bool refused = false;
            try { SerpentineRigSurveyPolicy.SnapshotMetadataSlots(new object[129]); }
            catch (ArgumentException) { refused = true; }
            Assertions.True(refused, "An oversized census remains rejected, not silently truncated.");
        }

        internal static void ManufacturedActionCensusIsClosedAndDefensive()
        {
            var sources = SerpentineRigSurveyPolicy.ManufacturedPrefabSources;
            Assertions.Equal(11, sources.Length, "One representative per archived native Lizardfolk prefab.");
            Assertions.Equal(11, sources.Select(row => row[0]).Distinct().Count(), "No duplicate native owner.");
            Assertions.Equal(11, sources.Select(row => row[1]).Distinct().Count(), "No repeated prefab observation.");
            foreach (var source in sources)
            {
                Assertions.Equal(2, source.Length, "Exact blueprint/view pair, no request-supplied path.");
                Assertions.True(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(source[0], source[1]),
                    "Each archived pair is observable without inferring any action compatibility.");
                foreach (string bad in new[] { null, "", "foreign", source[0].ToUpperInvariant() })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(bad, source[1]),
                        "Unknown, missing or differently cased blueprint is refused.");
                foreach (string bad in new[] { null, "", "foreign", source[1].ToUpperInvariant() })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(source[0], bad),
                        "Unknown, missing or differently cased prefab is refused.");
                foreach (var other in sources.Where(row => row[0] != source[0]))
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(source[0], other[1]),
                        "Other allowed donors cannot substitute views.");
            }
            Assertions.True(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(
                SerpentineVisualPolicy.PiercingDonorBlueprint, SerpentineVisualPolicy.PiercingDonorPrefab),
                "Rejected shortspear is retained as a negative observation, not silently replaced.");
            Assertions.True(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(
                SerpentineRigSurveyPolicy.HybridWeaponBlueprint, SerpentineRigSurveyPolicy.HybridWeaponPrefab),
                "Existing two-hand slashing carrier remains an explicit comparison.");
            string blueprint = sources[0][0], prefab = sources[0][1];
            sources[0][0] = "changed"; sources[1] = new[] { "foreign", "foreign" };
            Assertions.True(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(blueprint, prefab),
                "Mutating a returned nested array cannot broaden or corrupt the closed target set.");
            Assertions.False(SerpentineRigSurveyPolicy.MatchesManufacturedPrefab("changed", prefab),
                "No mutable shared authority escapes.");
        }

        internal static void NativePiercingActionRequiresExactCarrierAndRig()
        {
            string[] exact = { "salamander", SerpentineVisualPolicy.TwoHandPrefab, SerpentineVisualPolicy.ProjectSpear,
                SerpentineVisualPolicy.PiercingDonorBlueprint, SerpentineVisualPolicy.PiercingDonorPrefab,
                SerpentineVisualPolicy.PiercingDonorWeapon };
            Func<string[], bool, bool, bool, bool> allowed = (row, offhand, piercing, rig) =>
                SerpentineVisualPolicy.PermitsNativePiercingAction(row[0], row[1], row[2], row[3], row[4], row[5],
                    offhand, piercing, rig);
            Assertions.True(allowed(exact, false, true, true), "One fixed native carrier and compatible actual style/rig.");
            for (int i = 0; i < exact.Length; i++)
                foreach (string bad in new[] { null, "", "foreign", exact[i].ToUpperInvariant() })
                {
                    if (bad == exact[i]) continue;
                    var changed = (string[])exact.Clone(); changed[i] = bad;
                    Assertions.False(allowed(changed, false, true, true), "No other owner, weapon or donor may bind.");
                }
            Assertions.False(allowed(exact, true, true, true), "Donor offhand disagrees with archived provenance.");
            Assertions.False(allowed(exact, false, false, true), "Missing native piercing cannot be relabeled from slashing.");
            Assertions.False(allowed(exact, false, true, false), "Incompatible bone paths/binds cannot be forced to fit.");
        }

        internal static void NativeSpearActionCopyPreservesAllOtherReferences()
        {
            var idle = new object(); var hand = new object(); var hit = new object(); var replacement = new object();
            var original = new[] { idle, hand, hit };
            var copied = SerpentineVisualPolicy.CopyWithOneNativeSpearAction(original, hand, replacement);
            Assertions.False(ReferenceEquals(original, copied), "Never share the mutable action array.");
            Assertions.True(original.SequenceEqual(new[] { idle, hand, hit }), "Native source remains unchanged.");
            Assertions.True(copied.SequenceEqual(new[] { idle, replacement, hit }), "Only the exact hand slot changes.");
            foreach (object[] invalid in new[] { null, new object[0], new[] { idle, hit }, new[] { hand, hand },
                new[] { hand, null }, new[] { hand }.Concat(Enumerable.Repeat(idle, 128)).ToArray() })
            {
                bool rejected = false;
                try { SerpentineVisualPolicy.CopyWithOneNativeSpearAction(invalid, hand, replacement); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "Missing, duplicate or incomplete action input fails closed.");
            }
            foreach (object bad in new[] { null, hand })
            {
                bool rejected = false;
                try { SerpentineVisualPolicy.CopyWithOneNativeSpearAction(original, hand, bad); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "A missing or unchanged hand is not a demonstrated replacement.");
            }
        }

        internal static void NativeActedFallbackCannotProveClipPlayback()
        {
            Assertions.True(SerpentineRigSurveyPolicy.IsObservedAttackClip(true, true, true,
                "observed-native-clip", 1.5f, .7f), "Actual active clip metadata is usable playback evidence.");
            Assertions.False(SerpentineRigSurveyPolicy.IsObservedAttackClip(true, true, false,
                "observed-native-clip", 1.5f, .11f), "Native IsActed fallback without ActiveAnimation is rejected.");
            Assertions.False(SerpentineRigSurveyPolicy.IsObservedAttackClip(false, true, true,
                "observed-native-clip", 1.5f, .7f), "An unstarted handle is not executing playback.");
            Assertions.False(SerpentineRigSurveyPolicy.IsObservedAttackClip(true, false, true,
                "observed-native-clip", 1.5f, .7f), "A pre-contact animation frame is not an acted event.");
            foreach (string missing in new[] { null, "", " " })
                Assertions.False(SerpentineRigSurveyPolicy.IsObservedAttackClip(true, true, true,
                    missing, 1.5f, .7f), "Missing clip identity fails closed.");
            foreach (float invalid in new[] { -1f, float.NaN, float.NegativeInfinity, float.PositiveInfinity })
            {
                Assertions.False(SerpentineRigSurveyPolicy.IsObservedAttackClip(true, true, true,
                    "observed-native-clip", invalid, .7f), "Duration must be finite and positive.");
                Assertions.False(SerpentineRigSurveyPolicy.IsObservedAttackClip(true, true, true,
                    "observed-native-clip", 1.5f, invalid), "Playback time must be finite and nonnegative.");
            }
            Assertions.False(SerpentineRigSurveyPolicy.IsObservedAttackClip(true, true, true,
                "observed-native-clip", 0, 0), "A zero-duration placeholder is not playback proof.");
        }

        internal static void TwoPalmSpearMountFitsExistingShaftWithoutRescaling()
        {
            foreach (float length in new[] { .88533658f, 1.77067316f, 3.54134632f })
                foreach (float fraction in new[] { .04f, .2f, .5f, .8f })
                {
                    float rear;
                    Assertions.True(SerpentineVisualPolicy.TrySpearRearGrip(length, length * fraction, out rear),
                        "The existing shaft admits its two palms at any uniform view scale.");
                    Assertions.True(Math.Abs(rear / length - .1f) < .000001f,
                        "Rear grip is ten percent from the butt, not the old center mount.");
                    Assertions.True(rear + length * fraction <= length * .900001f,
                        "Both grips lie inside the unscaled shaft, retaining tip clearance.");
                }
            foreach (float bad in new[] { -1f, 0, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                float rear;
                Assertions.False(SerpentineVisualPolicy.TrySpearRearGrip(bad, .5f, out rear),
                    "Invalid shaft dimensions cannot invent a mount.");
                Assertions.Equal(0f, rear, "No usable partial mount on rejection.");
                Assertions.False(SerpentineVisualPolicy.TrySpearRearGrip(1.77f, bad, out rear),
                    "Coincident/nonfinite palms fail closed.");
            }
            foreach (float spacing in new[] { .039f, .801f, 1f, 2f })
            {
                float rear;
                Assertions.False(SerpentineVisualPolicy.TrySpearRearGrip(1, spacing, out rear),
                    "No arbitrary extension or degenerate orientation to force a two-hand fit.");
            }
        }

        internal static void NativeSpearBoundsRetainConservativeUncertainty()
        {
            Assertions.Equal((float?).25f, SerpentineRigSurveyPolicy.ConservativeSpearEndGap(.125f, .125f),
                "The complete transverse uncertainty is added, never subtracted to pass contact.");
            Assertions.Equal((float?)0, SerpentineRigSurveyPolicy.ConservativeSpearEndGap(0, 0),
                "An exact zero is not confused with an unknown measurement.");
            Assertions.Equal((float?)4.5f, SerpentineRigSurveyPolicy.ConservativeSpearEndGap(4, .5f),
                "Research retains a large miss for diagnosis; it does not clamp to the acceptance limit.");
            foreach (float invalid in new[] { -.01f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                Assertions.False(SerpentineRigSurveyPolicy.ConservativeSpearEndGap(invalid, .1f).HasValue,
                    "Missing, negative or nonfinite end-centre evidence is rejected.");
                Assertions.False(SerpentineRigSurveyPolicy.ConservativeSpearEndGap(.1f, invalid).HasValue,
                    "Missing, negative or nonfinite uncertainty is rejected.");
            }
            Assertions.False(SerpentineRigSurveyPolicy.ConservativeSpearEndGap(float.MaxValue, float.MaxValue).HasValue,
                "Overflow cannot become apparently valid geometry.");
        }

        internal static void ContactResearchRequiresIssuedOwnedMeasuredEvents()
        {
            foreach (float gap in new[] { 0f, .25f, 8f })
                Assertions.True(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, true, 7, gap),
                    "Research retains a measured miss instead of pretending only good contact exists.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(false, true, false, true, 7, 0),
                "An unrelated actor or target cannot satisfy the requested command.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, false, false, true, 7, 0),
                "A queued, rejected or incidental event is not an executing issued attack.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, true, true, 7, 0),
                "An opportunity attack cannot satisfy the manual command.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, false, 7, 0),
                "The native animation contact boundary must be observed.");
            foreach (int count in new[] { -1, 0 })
                Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, true, count, 0),
                    "An unmeasured donor attack is not a required contact.");
            foreach (float gap in new[] { -.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, true, 7, gap),
                    "Unknown or invalid geometry cannot satisfy research.");
        }

        internal static void NativeSpearResearchRejectsEveryChangedIdentity()
        {
            string[] exact = { "salamander", SerpentineVisualPolicy.TwoHandPrefab,
                SerpentineVisualPolicy.ProjectSpear, "Spear", "TH_SpearArmy", "WP_SpearArmy", "WeaponPivot", "R_Palm" };
            Func<string[], bool> permits = row => SerpentineVisualPolicy.PermitsNativeSpearResearch(
                row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7]);
            Assertions.True(permits(exact), "Only the measured project spear and exact two-hand palm seam may attach.");
            for (int index = 0; index < exact.Length; index++)
                foreach (string replacement in new[] { null, "", "foreign", exact[index].ToUpperInvariant(),
                    SerpentineVisualPolicy.ClubShieldPrefab, "Greatclub", "ShieldPivot", "L_Palm", "viper" })
                {
                    if (replacement == exact[index]) continue;
                    string[] changed = (string[])exact.Clone(); changed[index] = replacement;
                    Assertions.False(permits(changed), "A single substituted identity fails closed; no other weapon, view or anchor.");
                }
        }

        internal static void OriginalTriangleDiagnosticIsExactReversibleAndInputPreserving()
        {
            int[] source = { 0, 1, 2, 0, 2, 3 };
            int[] result = SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(source, 4);
            Assertions.True(source.SequenceEqual(new[] { 0, 1, 2, 0, 2, 3 }), "Caller-owned original indices never mutate.");
            Assertions.True(result.SequenceEqual(new[] { 0, 2, 1, 0, 3, 2 }), "Exactly the second/third slots swap per triangle.");
            Assertions.True(SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(result, 4).SequenceEqual(source),
                "Two swaps restore original order exactly, not an approximate mesh rewrite.");
            foreach (int[] invalid in new[] { null, new int[0], new[] { 0, 1 }, new[] { -1, 1, 2 },
                new[] { 0, 1, 4 }, new[] { 0, 0, 1 }, new[] { 0, 1, 0 }, new[] { 0, 1, 1 } })
            {
                bool rejected = false;
                try { SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(invalid, 4); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "Incomplete, out-of-range and degenerate lists fail closed.");
            }
            foreach (int count in new[] { -1, 0, 1, 2 })
            {
                bool rejected = false;
                try { SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(new[] { 0, 1, 2 }, count); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "A body must have at least three addressable vertices.");
            }
        }

        internal static void GroundResearchRequiresMeasuredSurfaceAndRetainsPenetration()
        {
            Assertions.Equal((float?)(-.125f), SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                -6.125f, true, -6f, 1f, false), "Penetration is not clamped or compared to an actor/nav origin.");
            Assertions.Equal((float?).375f, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                17.375f, true, 17f, 1f, false), "A positive gap remains a gap, not automatic ground contact.");
            Assertions.Equal((float?)(-.125f), SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                13.875f, true, 14f, .2f, false), "World translation does not change measured clearance.");
            Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                0, false, 0, 1, false), "A missed ray is unknown, never zero clearance.");
            Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                0, true, 0, 1, true), "The owned actor's collider cannot stand in for terrain.");
            foreach (float normal in new[] { -.2f, 0, .199f, 1.1f, float.NaN, float.PositiveInfinity })
                Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                    0, true, 0, normal, false), "Only a finite upward-facing measured surface is usable.");
            foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                    bad, true, 0, 1, false), "Nonfinite original geometry cannot produce ground evidence.");
                Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                    0, true, bad, 1, false), "Nonfinite hit geometry cannot produce ground evidence.");
            }
            Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                float.MaxValue, true, -float.MaxValue, 1, false), "Overflow is unknown, not a usable measurement.");
        }

        internal static void BodyResearchIsClosedAndUsesWorkingSaveGuard()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSerpentineBodies;
            Assertions.Equal("disposable-expanded-summoning-serpentine-bodies", scenario,
                "One closed research request, not a general runtime asset loader.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario),
                "Body research traverses the existing exact working-save guard.");
            foreach (string value in new[] { null, "", scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-serpentine-bodies" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(value) ||
                    RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(value),
                    "Unknown variants cannot widen the source, save or publication scope.");
            Assertions.True(SerpentineVisualPolicy.Keys.SequenceEqual(new[] { "viper", "constrictor-snake", "salamander" }),
                "Only the three original bodies run; caller parameters cannot substitute arbitrary keys.");
        }

        internal static void OriginalBodyPayloadsAreCompleteAndRedistributionSafe()
        {
            string directory = Path.Combine(Environment.CurrentDirectory, "assets", "sprint17-serpents");
            string[] hashes = {
                "754d82bbdd4a030b0bddb150202bc6ec9bf2d149754741e9d2900a1286463d8e",
                "f7811906bbc1e257536371d2d8192e04d412e26bd2864aa55ae56e68c7e0356f",
                "bd790df1e258fa6190ef48a2e68f0beb5eaeba6d035d51eeb3aaefc5cbb9a60b" };
            int keyIndex = 0;
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                byte[] bytes = File.ReadAllBytes(Path.Combine(directory, key + "-mesh.json"));
                using (SHA256 hash = SHA256.Create())
                    Assertions.Equal(hashes[keyIndex++], BitConverter.ToString(hash.ComputeHash(bytes))
                        .Replace("-", "").ToLowerInvariant(), "Byte-reproduced original body, not native geometry.");
                JObject mesh = JObject.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                Assertions.True(SerpentineVisualPolicy.ExactSet(mesh.Properties().Select(value => value.Name),
                    new[] { "schemaVersion", "space", "rigSha256", "bones", "uvAtlas", "albedo",
                        "vertexCount", "triangleCount", "data", "visibleLegs", "jawSeparated", "triangleWinding" }),
                    "Only original mesh/paint and bone names; no bind transforms, animation curves or native assets.");
                string[] bones = mesh["bones"].Values<string>().ToArray();
                Assertions.True(SerpentineVisualPolicy.PermitsBones(key, bones) &&
                    (int)mesh["schemaVersion"] == 2 && (int)mesh["visibleLegs"] == 0 && (bool)mesh["jawSeparated"],
                    "The actual loader policy accepts this complete original body.");
                int count = (int)mesh["vertexCount"], triangles = (int)mesh["triangleCount"];
                byte[] data = Convert.FromBase64String((string)mesh["data"]);
                Assertions.True(count > 2000 && triangles > 4000 && data.Length == count * 64 + triangles * 12,
                    "Full original geometry payload, not an asset reference.");
                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    for (int i = 0; i < count * 6; i++)
                    {
                        float value = reader.ReadSingle();
                        Assertions.True(!float.IsNaN(value) && !float.IsInfinity(value), "Finite positions/normals.");
                    }
                    for (int i = 0; i < count * 2; i++)
                    {
                        float value = reader.ReadSingle();
                        Assertions.True(value > 0 && value < 1, "Original inset atlas coordinates.");
                    }
                    for (int i = 0; i < triangles * 3; i++)
                    {
                        int index = reader.ReadInt32();
                        Assertions.True(index >= 0 && index < count, "Triangle indices stay inside original geometry.");
                    }
                    var totals = new double[bones.Length];
                    for (int i = 0; i < count; i++)
                    {
                        float sum = 0; int positive = 0;
                        for (int slot = 0; slot < 4; slot++)
                        {
                            int bone = reader.ReadInt32(); float weight = reader.ReadSingle();
                            Assertions.True(bone >= 0 && bone < bones.Length && weight >= 0 && weight <= 1,
                                "Only approved normalized drivers, never extra native branches.");
                            sum += weight; totals[bone] += weight;
                            if (weight > 0) positive++;
                        }
                        Assertions.True(Math.Abs(sum - 1) < .00001 && positive >= 1 && positive <= (key == "salamander" ? 3 : 2),
                            "At most two native influences plus the hybrid-only original support.");
                    }
                    Assertions.True(totals.All(value => value > 0), "Each declared driver carries real geometry.");
                }
                JObject albedo = (JObject)mesh["albedo"];
                Assertions.Equal(key + "-albedo.png", (string)albedo["file"], "Only this creature's painting.");
                using (SHA256 hash = SHA256.Create())
                    Assertions.Equal((string)albedo["sha256"], BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(
                        Path.Combine(directory, key + "-albedo.png")))).Replace("-", "").ToLowerInvariant(),
                        "Exact deterministic original paint pairing.");
            }
        }

        internal static void OriginalBodyWindingHasOutwardGeometryAndFailsClosedOnOldAssets()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                JObject mesh = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                    "assets", "sprint17-serpents", key + "-mesh.json")));
                Assertions.True(SerpentineVisualPolicy.PermitsOriginalWinding(key, (string)mesh["triangleWinding"]),
                    "Only the measured original-body export convention is accepted.");
                foreach (string bad in new[] { null, "", "clockwise", "foreign", SerpentineVisualPolicy.OutwardWinding.ToUpperInvariant() })
                    Assertions.False(SerpentineVisualPolicy.PermitsOriginalWinding(key, bad),
                        "Missing, historic or unreviewed face conventions fail before attachment.");
                int count = (int)mesh["vertexCount"], triangles = (int)mesh["triangleCount"];
                byte[] data = Convert.FromBase64String((string)mesh["data"]);
                var vertices = new double[count][]; var normals = new double[count][];
                for (int i = 0; i < count; i++)
                {
                    vertices[i] = Enumerable.Range(0, 3).Select(axis => (double)BitConverter.ToSingle(data, i * 12 + axis * 4)).ToArray();
                    normals[i] = Enumerable.Range(0, 3).Select(axis => (double)BitConverter.ToSingle(data, count * 12 + i * 12 + axis * 4)).ToArray();
                }
                double orientation = 0, volume = 0;
                for (int i = 0; i < triangles; i++)
                {
                    int offset = count * 32 + i * 12;
                    int a = BitConverter.ToInt32(data, offset), b = BitConverter.ToInt32(data, offset + 4),
                        c = BitConverter.ToInt32(data, offset + 8);
                    double[] u = Enumerable.Range(0, 3).Select(axis => vertices[b][axis] - vertices[a][axis]).ToArray();
                    double[] v = Enumerable.Range(0, 3).Select(axis => vertices[c][axis] - vertices[a][axis]).ToArray();
                    double[] cross = { u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0] };
                    for (int axis = 0; axis < 3; axis++)
                    {
                        orientation += cross[axis] * (normals[a][axis] + normals[b][axis] + normals[c][axis]);
                        volume += vertices[a][axis] * cross[axis] / 6;
                    }
                }
                Assertions.True(orientation > 0 && volume > 0,
                    "Whole-body signed volume and area-weighted normals point outward; historic inverted indices fail both.");
            }
            foreach (string key in new[] { null, "", "purple-worm", "crocodile", "Viper" })
                Assertions.False(SerpentineVisualPolicy.PermitsOriginalWinding(key, SerpentineVisualPolicy.OutwardWinding),
                    "The winding correction cannot opt a different family into attachment.");
        }

        internal static void OriginalBodyBindingRejectsUnreviewedDrivers()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                string[] bones = SerpentineVisualPolicy.Bones(key);
                Assertions.Equal(key == "salamander" ? 28 : 16, bones.Length, "Measured native drivers plus the hybrid-only support.");
                Assertions.True(SerpentineVisualPolicy.PermitsBones(key, bones.Reverse()),
                    "Native name mapping is independent of export order.");
                foreach (string bone in bones)
                    Assertions.False(SerpentineVisualPolicy.PermitsBones(key, bones.Where(value => value != bone)),
                        "A missing body, jaw, hand or tail driver is rejected.");
                foreach (string foreign in new[] { "Jaw_Left", "Jaw_Right", "Jaw_Up", "Horn", "Stone01",
                    "L_Leg_Upper", "R_Leg_Upper", "WeaponPivot", "ShieldPivot", null, "" })
                    Assertions.False(SerpentineVisualPolicy.PermitsBones(key, bones.Concat(new[] { foreign })),
                        "No worm petal/stone, hybrid leg/equipment or arbitrary branch may carry geometry.");
                Assertions.False(SerpentineVisualPolicy.PermitsBones(key, bones.Concat(new[] { bones[0] })),
                    "Duplicate influence names cannot map ambiguously.");
                bones[0] = "mutated";
                Assertions.True(SerpentineVisualPolicy.PermitsBones(key, SerpentineVisualPolicy.Bones(key)),
                    "Consumers cannot mutate the binding authority.");
            }
            foreach (string unknown in new[] { null, "", "Viper", "purple-worm", "medium-water-elemental", "foreign" })
                Assertions.False(SerpentineVisualPolicy.PermitsBones(unknown, SerpentineVisualPolicy.Bones("viper")),
                    "Only new original identities are allowed, never native negative controls.");
        }

        internal static void MultiRendererBodySwapRequiresExactDonorSet()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                bool snake = SerpentineVisualPolicy.IsSnake(key);
                string prefab = snake ? SerpentineVisualPolicy.WormPrefab : SerpentineVisualPolicy.ClubShieldPrefab;
                string[] skins = { SerpentineVisualPolicy.BodyRenderer(key), SerpentineVisualPolicy.AuxiliaryRenderer(key) };
                string[] equipment = snake ? new string[0] : new[] { "lizardman_club", "WP_ShieldLightDamaged" };
                int bodyCount = snake ? 40 : 39, extraCount = snake ? 40 : 19;
                string root = snake ? "Hips_Joints" : "Torso_Lower";
                Func<string, string[], int, int, string, string[], bool> permits = (view, names, body, extra, anchor, statics) =>
                    SerpentineVisualPolicy.PermitsDonor(key, view, names, body, extra, anchor, statics);
                Assertions.True(permits(prefab, skins.Reverse().ToArray(), bodyCount, extraCount, root, equipment),
                    "Exact two-skin donor and known static equipment are accepted by BODY policy only.");
                Assertions.False(permits("foreign", skins, bodyCount, extraCount, root, equipment), "No unknown view.");
                Assertions.False(permits(prefab, skins.Take(1).ToArray(), bodyCount, extraCount, root, equipment), "No partial rig.");
                Assertions.False(permits(prefab, skins.Concat(new[] { "extra-skin" }).ToArray(), bodyCount, extraCount, root, equipment),
                    "An unreviewed third skin is not silently left visible.");
                Assertions.False(permits(prefab, new[] { skins[0], skins[0] }, bodyCount, extraCount, root, equipment), "No duplicate skin names.");
                Assertions.False(permits(prefab, skins, bodyCount - 1, extraCount, root, equipment), "Body bind count pinned.");
                Assertions.False(permits(prefab, skins, bodyCount, extraCount - 1, root, equipment), "Auxiliary bind count pinned.");
                Assertions.False(permits(prefab, skins, bodyCount, extraCount, "foreign", equipment), "Root pinned.");
                Assertions.False(permits(prefab, skins, bodyCount, extraCount, root, equipment.Concat(new[] { "unexpected-weapon" }).ToArray()),
                    "Never suppress an unreviewed weapon or leave it silently visible.");
            }
            string[] hybridSkins = { "_lizardman001", "_ammunition04" };
            Assertions.True(SerpentineVisualPolicy.PermitsDonor("salamander", SerpentineVisualPolicy.TwoHandPrefab,
                hybridSkins, 39, 19, "Torso_Lower", new[] { "lizardman_club" }),
                "Measured identical two-hand body frame is accepted without inferring grip/contact.");
            Assertions.False(SerpentineVisualPolicy.PermitsDonor("salamander", SerpentineVisualPolicy.TwoHandPrefab,
                hybridSkins, 39, 19, "Torso_Lower", new[] { "lizardman_club", "WP_ShieldLightDamaged" }),
                "A shield must not silently appear on the two-hand donor.");
        }

        internal static void HybridSupportMappingPreservesEveryNativeSlotAndRejectsUnknowns()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                string[] original = SerpentineVisualPolicy.Bones(key).Reverse().ToArray();
                string[] required = original.Where(name => name != SerpentineVisualPolicy.HybridSupport).ToArray();
                string[] native = required.Concat(Enumerable.Range(0, (key == "salamander" ? 39 : 40) - required.Length)
                    .Select(index => "UnusedNative" + index)).Reverse().ToArray();
                int[] slots;
                Assertions.True(SerpentineVisualPolicy.TryResolveDriverSlots(key, original, native, out slots),
                    "Complete original driver mapping works independently of native/export order.");
                Assertions.Equal(original.Length, slots.Length, "No missing or appended native mapping.");
                for (int i = 0; i < original.Length; i++)
                    if (original[i] == SerpentineVisualPolicy.HybridSupport)
                        Assertions.True(key == "salamander" && slots[i] == -1, "Only named hybrid support selects renderer frame.");
                    else Assertions.Equal(original[i], native[slots[i]], "Every real driver retains its exact native bindpose slot.");
                foreach (string[] bad in new[] { null, native.Take(native.Length - 1).ToArray(),
                    native.Concat(new[] { "foreign" }).ToArray(), native.Select(name => name == required[0] ? "missing" : name).ToArray(),
                    native.Select(name => name == required[0] ? native[0] : name).ToArray(),
                    native.Select(name => name == required[0] ? SerpentineVisualPolicy.HybridSupport : name).ToArray() })
                {
                    Assertions.False(SerpentineVisualPolicy.TryResolveDriverSlots(key, original, bad, out slots),
                        "Missing, duplicate, foreign-count and project-named native mappings never fall back.");
                    Assertions.Equal((int[])null, slots, "No partially usable mapping on rejection.");
                }
                Assertions.False(SerpentineVisualPolicy.TryResolveDriverSlots(key,
                    original.Concat(new[] { SerpentineVisualPolicy.HybridSupport }).ToArray(), native, out slots),
                    "A support cannot be added to snakes or duplicated on the hybrid.");
            }
        }

        internal static void RigSurveyUsesOnlyExactNativeSources()
        {
            Assertions.True(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                SerpentineRigSurveyPolicy.HybridWeaponBlueprint, SerpentineRigSurveyPolicy.HybridWeaponPrefab,
                SerpentineRigSurveyPolicy.HybridPrimaryWeapon, false),
                "Only the archived native greatclub/no-offhand pair is a detached comparison.");
            foreach (string invalid in new[] { null, "", "foreign",
                SerpentineRigSurveyPolicy.NativeBlueprint("salamander") })
            {
                Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(invalid,
                    SerpentineRigSurveyPolicy.HybridWeaponPrefab, SerpentineRigSurveyPolicy.HybridPrimaryWeapon, false),
                    "No different native or project blueprint is accepted.");
                Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                    SerpentineRigSurveyPolicy.HybridWeaponBlueprint, invalid,
                    SerpentineRigSurveyPolicy.HybridPrimaryWeapon, false), "No arbitrary prefab is loaded.");
                Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                    SerpentineRigSurveyPolicy.HybridWeaponBlueprint, SerpentineRigSurveyPolicy.HybridWeaponPrefab,
                    invalid, false), "The observed manufactured weapon is pinned too.");
            }
            Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                SerpentineRigSurveyPolicy.HybridWeaponBlueprint, SerpentineRigSurveyPolicy.HybridWeaponPrefab,
                SerpentineRigSurveyPolicy.HybridPrimaryWeapon, true), "A shield/offhand donor cannot substitute.");
            Assertions.False(SerpentineRigSurveyPolicy.CountPresentClips(null).HasValue,
                "A native action without a clip enumeration stays unknown, not an exception or invented zero.");
            Assertions.Equal((int?)0, SerpentineRigSurveyPolicy.CountPresentClips(new bool[0]),
                "An exposed empty clip collection is an explicit zero.");
            Assertions.Equal((int?)0, SerpentineRigSurveyPolicy.CountPresentClips(new[] { false, false }),
                "Null entries do not count as clips.");
            Assertions.Equal((int?)2, SerpentineRigSurveyPolicy.CountPresentClips(new[] { true, false, true }),
                "Only live native clip entries are counted.");
            Assertions.True(SerpentineRigSurveyPolicy.Keys.SequenceEqual(
                new[] { "medium-water-elemental", "salamander", "purple-worm" }),
                "Only the three fixed pre-existing rig carriers are in the research scope; no new publication.");
            string[] copy = SerpentineRigSurveyPolicy.Keys;
            copy[0] = "foreign";
            Assertions.Equal("medium-water-elemental", SerpentineRigSurveyPolicy.Keys[0],
                "Consumers cannot change the shared research target set.");
            foreach (string key in SerpentineRigSurveyPolicy.Keys)
            {
                string native = SerpentineRigSurveyPolicy.NativeBlueprint(key);
                string prefab = SerpentineRigSurveyPolicy.Prefab(key);
                Assertions.True(SerpentineRigSurveyPolicy.MatchesNativeSource(key, native, prefab),
                    "The recorded native blueprint/view pair is accepted.");
                foreach (string bad in new[] { "", null, "foreign", native.ToUpperInvariant(),
                    "6f5db07e89834a01aa9b7e7aed6cd407" })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, bad, prefab),
                        "A missing, changed or optional Eidolon source is never substituted.");
                foreach (string bad in new[] { "", null, "foreign", prefab.ToUpperInvariant() })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, native, bad),
                        "View resolution is exact, not a permissive arbitrary-asset loader.");
                foreach (string other in SerpentineRigSurveyPolicy.Keys.Where(value => value != key))
                {
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key,
                        SerpentineRigSurveyPolicy.NativeBlueprint(other), prefab),
                        "A different allowed donor cannot substitute for the selected source.");
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key,
                        native, SerpentineRigSurveyPolicy.Prefab(other)),
                        "A different allowed view cannot substitute for the selected source.");
                }
            }
            foreach (string key in new[] { null, "", "viper", "constrictor-snake", "Salamander", "foreign" })
                Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, null, null),
                    "Unknown, new or differently cased keys fail closed.");
        }
    }
}
