using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2GiantPresentationTests
    {
        internal static void ExactIdentitiesMatchReleasedUnitsAndFrostConsumers()
        {
            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")));
            JToken[] entries = manifest["entries"].ToArray();
            foreach (GiantSummonPresentationProfile profile in GiantSummonPresentationPolicy.All
                .Where(value => value.Key != "frost-giant"))
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All.Single(value => value.Key == profile.Key);
                string symbol = ExpandedSummoningIdentityCatalog.UnitSymbol(creature);
                Assertions.Equal(profile.UnitGuid, (string)entries.Single(value =>
                    (string)value["symbol"] == symbol)["guid"],
                    "Giant presentation must bind the released unit identity.");
                Assertions.True(ReferenceEquals(profile, GiantSummonPresentationPolicy.Resolve(
                    true, profile.UnitGuid, null)), "Project-owned giant identity was not recognized.");
                Assertions.True(GiantSummonPresentationPolicy.Resolve(false,
                    profile.UnitGuid, null) == null, "Module-off resolution must remain inert.");
            }
            string[] expectedSources = entries.Where(value =>
                (string)value["plannedType"] == "BlueprintAbility" &&
                ((string)value["symbol"]).Contains(".NativeOption.") &&
                ((string)value["symbol"]).Contains(".FrostGiant."))
                .Select(value => (string)value["guid"]).OrderBy(value => value).ToArray();
            Assertions.True(expectedSources.SequenceEqual(GiantSummonPresentationPolicy.NativeFrostSources
                .Select(value => value.Value).OrderBy(value => value)),
                "Frost provenance must cover the exact released canonical and adapter sources.");
            foreach (var source in GiantSummonPresentationPolicy.NativeFrostSources)
            {
                Assertions.Equal(source.Value, (string)entries.Single(value =>
                    (string)value["symbol"] == source.Key)["guid"],
                    "Frost source GUID no longer matches its stable symbol.");
                Assertions.Equal("frost-giant", GiantSummonPresentationPolicy.Resolve(true,
                    GiantSummonPresentationPolicy.NativeFrostGuid, source.Value).Key,
                    "An exact registered Frost source should authorize its native unit.");
            }
            Assertions.True(SummonNativeExpansionCatalog.All.Where(value =>
                value.IconKey == "frost-giant").All(value => value.UnitGuid ==
                    GiantSummonPresentationPolicy.NativeFrostGuid),
                "Presentation must follow the retained native Frost unit, never its donor aliases.");
        }

        internal static void NativeFrostRequiresActualSummonedSourceIdentity()
        {
            string frost = GiantSummonPresentationPolicy.NativeFrostGuid;
            foreach (string invalidSource in new[] { null, "", "native-summon", "KMG_Summoning_NativeOption_FrostGiant",
                GiantSummonPresentationPolicy.NativeFrostSources[0].Value.ToUpperInvariant() })
                Assertions.True(GiantSummonPresentationPolicy.Resolve(true, frost, invalidSource) == null,
                    "A name, foreign summon or missing provenance must not recolor/rescale native Frost.");
            string valid = GiantSummonPresentationPolicy.NativeFrostSources[0].Value;
            Assertions.True(GiantSummonPresentationPolicy.Resolve(false, frost, valid) == null,
                "Module-off startup must leave even authorized native Frost unchanged.");
            Assertions.True(GiantSummonPresentationPolicy.Resolve(true,
                "61dd71e874b16f742a0f783314105553", valid) == null,
                "A native Hill donor must remain unchanged even with another summon source.");
            Assertions.True(GiantSummonPresentationPolicy.Resolve(true, null, valid) == null,
                "Missing unit identity must fail closed.");
        }

        internal static void OnlyFireAndStormDropTheFrostDonorEffects()
        {
            foreach (GiantSummonPresentationProfile profile in GiantSummonPresentationPolicy.All)
            {
                bool frostBody = profile.NativePrefabId == "df0f3fbceb3605d489c0dec8fa5c7d1e";
                Assertions.Equal(frostBody && profile.Key != "frost-giant",
                    GiantSummonPresentationPolicy.SuppressesDonorFrostEffects(true, profile.UnitGuid),
                    "Only owned bodies on the Frost prefab drop its frost effects: " + profile.Key);
                Assertions.False(GiantSummonPresentationPolicy.SuppressesDonorFrostEffects(false, profile.UnitGuid),
                    "Module-off startup must keep every native effect: " + profile.Key);
            }
            foreach (string other in new[] { null, "", "61dd71e874b16f742a0f783314105553",
                GiantSummonPresentationPolicy.All.Single(value => value.Key == "fire-giant").UnitGuid.ToUpperInvariant() })
                Assertions.False(GiantSummonPresentationPolicy.SuppressesDonorFrostEffects(true, other),
                    "A missing, native or non-exact identity must keep its native effects.");
        }

        internal static void MeasuredHeightIsRelativeToHumanAndNeverCompounds()
        {
            // Synthetic numeric fixtures prove the calculation only. They are
            // deliberately not claimed as native donor measurements.
            GiantSummonPresentationProfile storm = GiantSummonPresentationPolicy.All
                .Single(value => value.Key == "storm-giant");
            var baseline = new GiantPresentationScale(1f, 2f, 1f);
            GiantSummonHeightCalibration calibration = Calibration(storm.Key, baseline);
            GiantPresentationScale target;
            Assertions.Equal("apply-measured-height", GiantSummonPresentationPolicy.Calculate(storm,
                calibration, "synthetic-prefab", baseline, baseline, out target),
                "A measured native calibration should produce an absolute target from its baseline.");
            Assertions.True(Math.Abs(target.X - 2.1f) < .00001f &&
                Math.Abs(target.Y - 4.2f) < .00001f && Math.Abs(target.Z - 2.1f) < .00001f,
                "Storm's measured 21:6 height reference must preserve the original axis proportions.");
            GiantPresentationScale second;
            Assertions.Equal("already-applied", GiantSummonPresentationPolicy.Calculate(storm,
                calibration, "synthetic-prefab", baseline, target, out second),
                "Repeated attach must not multiply the applied transform again.");
            Assertions.True(GiantSummonPresentationPolicy.Same(target, second),
                "Idempotent application changed the target.");
            Assertions.Equal("apply-measured-height", GiantSummonPresentationPolicy.Calculate(storm,
                calibration, "synthetic-prefab", baseline, baseline, out second),
                "Native resetting of the same view's baseline should permit deterministic reapplication.");
            Assertions.True(GiantSummonPresentationPolicy.Same(target, second),
                "Reconstruction produced a different target from the same measured baseline.");
            var foreign = new GiantPresentationScale(3f, 3f, 3f);
            Assertions.Equal("foreign-scale-change", GiantSummonPresentationPolicy.Calculate(storm,
                calibration, "synthetic-prefab", baseline, foreign, out second),
                "An unrelated rescale must be preserved.");
            Assertions.True(GiantSummonPresentationPolicy.Same(foreign, second),
                "Refused application must leave the foreign transform intact.");
        }

        internal static void MissingWrongOrInvalidCalibrationNeverChangesScale()
        {
            foreach (float factor in new[] { 1f, 1.4f, 1.5f, 2.1f })
            {
                Assertions.Equal(float.MaxValue, GiantSummonPresentationPolicy.ScaleClothConstraint(float.MaxValue, factor),
                    "The exact native disabled Cloth sentinel must remain disabled rather than overflow, clamp or become a simulated distance.");
                Assertions.Equal(0f, GiantSummonPresentationPolicy.ScaleClothConstraint(0f, factor),
                    "A pinned native Cloth vertex must remain pinned.");
                Assertions.Equal(.25f * factor, GiantSummonPresentationPolicy.ScaleClothConstraint(.25f, factor),
                    "An enabled finite native Cloth distance must retain the explicit proposal ratio.");
            }
            foreach (float invalid in new[] { -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assertions.Throws<ArgumentException>(() => GiantSummonPresentationPolicy.ScaleClothConstraint(invalid, 1.5f),
                    "Unknown native Cloth inputs must remain explicit refusals.");
            foreach (float invalid in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
                Assertions.Throws<ArgumentException>(() => GiantSummonPresentationPolicy.ScaleClothConstraint(float.MaxValue, invalid),
                    "A sentinel cannot authorize an invalid ratio.");
            Assertions.Throws<ArgumentException>(() => GiantSummonPresentationPolicy.ScaleClothConstraint(float.MaxValue / 2f, 3f),
                "Genuine enabled-distance overflow must not be mistaken for the disabled sentinel.");
            Assertions.Throws<ArgumentException>(() => GiantSummonPresentationPolicy.ScaleClothConstraint(float.MaxValue / 2f, 2f),
                "Enabled distance arithmetic must not silently change a constraint into the disabled sentinel.");
            string[] order = { "hill-giant", "stone-giant", "fire-giant", "frost-giant", "cloud-giant", "storm-giant" };
            float[] relative = { 1f, 1.2f, 1.4f, 1.5f, 1.8f, 2.1f };
            for (int i = 0; i < order.Length; i++)
            {
                var family = GiantSummonPresentationPolicy.All.Single(value => value.Key == order[i]);
                Assertions.Equal(relative[i], family.RelativeVisualFactor,
                    "Relative family hierarchy must preserve the selected Hill baseline without a Human-foot conversion.");
                foreach (float nativeSole in new[] { -.2f, 0f, .27f })
                    Assertions.True(Math.Abs(nativeSole * family.RelativeVisualFactor +
                        GiantSummonPresentationPolicy.SolePivotOffset(nativeSole, family.RelativeVisualFactor) - nativeSole) < .000001f,
                        "Owned mirror-root compensation must preserve the native sole plane instead of amplifying a navmesh gap.");
                Assertions.Equal(i == 2 || i == 3 || i == 5 ? "df0f3fbceb3605d489c0dec8fa5c7d1e" : "635d6dee1c7831b41aa987bdbeff14bc",
                    family.NativePrefabId, "Only the exact inspected Hill/Frost donor prefab may enter the render-only factory.");
                if (i > 0) Assertions.True(family.RelativeVisualFactor > relative[i - 1],
                    "Distinct giant families retain increasing visible size without changing their mechanical Size.");
            }
            var profile = GiantSummonPresentationPolicy.All.Single(value => value.Key == "hill-giant");
            var baseline = new GiantPresentationScale(1f, 1f, 1f);
            GiantPresentationScale target;
            Assertions.Equal("missing-calibration", GiantSummonPresentationPolicy.Calculate(profile,
                null, "synthetic-prefab", baseline, baseline, out target),
                "Missing actual native measurements must be an explicit no-op.");
            Assertions.True(GiantSummonPresentationPolicy.Same(baseline, target),
                "Missing calibration must not guess a transform.");
            Assertions.Equal("calibration-identity-mismatch", GiantSummonPresentationPolicy.Calculate(profile,
                Calibration(profile.Key, baseline), "different-prefab", baseline, baseline, out target),
                "A different installed prefab requires new measurement.");
            Assertions.Equal("baseline-scale-mismatch", GiantSummonPresentationPolicy.Calculate(profile,
                Calibration(profile.Key, baseline), "synthetic-prefab",
                new GiantPresentationScale(2f, 2f, 2f), baseline, out target),
                "A different root scale must not reuse an old measurement.");
            foreach (float invalid in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
            {
                var calibration = new GiantSummonHeightCalibration(profile.Key, "synthetic-prefab",
                    baseline, invalid, 1.8f, 6f, "synthetic-test-only");
                Assertions.Equal("invalid-calibration-or-scale", GiantSummonPresentationPolicy.Calculate(profile,
                    calibration, "synthetic-prefab", baseline, baseline, out target),
                    "Invalid giant height must not authorize a transform.");
                Assertions.True(GiantSummonPresentationPolicy.Same(baseline, target),
                    "Invalid measurement changed the output transform.");
            }
            var unproven = new GiantSummonHeightCalibration(profile.Key, "synthetic-prefab",
                baseline, 3f, 1.8f, 6f, null);
            Assertions.Equal("invalid-calibration-or-scale", GiantSummonPresentationPolicy.Calculate(profile,
                unproven, "synthetic-prefab", baseline, baseline, out target),
                "Calibration needs a recorded evidence identity.");
            Assertions.Equal(21f / 10f, GiantSummonPresentationPolicy.All.Single(value =>
                value.Key == "storm-giant").ReferenceHeightFeet / profile.ReferenceHeightFeet,
                "Storm and Hill art references must retain their selected physical-height relationship.");
        }

        internal static void RefusedNativeHideRestoresThatHideNotThePreviousState()
        {
            object body = new object(), cape = new object(), weapon = new object(), destroyed = new object(), foreign = new object();
            object[] captured = { body, cape, weapon, destroyed };
            var record = new System.Collections.Generic.Dictionary<object, bool>();
            Func<object, bool> live = value => !ReferenceEquals(value, destroyed);
            Assertions.True(GiantSummonNativeVisibilityRecord.Expected(record, cape, true),
                "Before any native write an original keeps its intake state.");
            object[] cache = { body, cape, weapon, foreign, destroyed };
            GiantSummonNativeVisibilityRecord.Record(record, captured, cache, live, true);
            // The native fader hides the giant; an unrelated validation refuses
            // afterwards. Cleanup must restore that hide, never the earlier show.
            GiantSummonNativeVisibilityRecord.Record(record, captured, cache, live, false);
            foreach (object value in new[] { body, cape, weapon })
                Assertions.False(GiantSummonNativeVisibilityRecord.Expected(record, value, true),
                    "Every original the native hide wrote must be restored hidden, including displaced cape and static weapon sources.");
            Assertions.True(GiantSummonNativeVisibilityRecord.Expected(record, destroyed, true),
                "A destroyed entry is skipped by the native writer and keeps its intake state.");
            Assertions.False(record.ContainsKey(foreign), "A renderer outside the captured originals is never recorded.");
            // A replaced or edited list that omits the cape did not write it.
            GiantSummonNativeVisibilityRecord.Record(record, captured, new[] { body, weapon }, live, true);
            Assertions.True(GiantSummonNativeVisibilityRecord.Expected(record, body, false), "A written original follows the latest native show.");
            Assertions.False(GiantSummonNativeVisibilityRecord.Expected(record, cape, true),
                "An original absent from the written list keeps its last authentic native write.");
            GiantSummonNativeVisibilityRecord.Record(record, captured, null, live, true);
            Assertions.False(GiantSummonNativeVisibilityRecord.Expected(record, cape, true), "A missing written list records nothing.");
        }

        private static GiantSummonHeightCalibration Calibration(string key, GiantPresentationScale baseline)
        { return new GiantSummonHeightCalibration(key, "synthetic-prefab", baseline,
            3f, 1.8f, 6f, "synthetic-test-only"); }
    }
}
