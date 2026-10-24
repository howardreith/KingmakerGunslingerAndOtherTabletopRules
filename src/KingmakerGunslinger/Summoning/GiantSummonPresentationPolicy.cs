using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class GiantSummonPresentationProfile
    {
        internal GiantSummonPresentationProfile(string key, string unitGuid,
            float referenceHeightFeet)
        { Key = key; UnitGuid = unitGuid; ReferenceHeightFeet = referenceHeightFeet; }
        internal string Key { get; private set; }
        internal string UnitGuid { get; private set; }
        internal float ReferenceHeightFeet { get; private set; }
        // Relative family art hierarchy. Actual review19 current-body samples
        // were ~3.50 units for both native donor families; no Human/feet
        // conversion or assumed navmesh-to-sole contact authorizes this ratio.
        internal float RelativeVisualFactor { get { return ReferenceHeightFeet / 10f; } }
        internal string NativePrefabId { get { return Key == "frost-giant" || Key == "fire-giant" || Key == "storm-giant"
            ? "df0f3fbceb3605d489c0dec8fa5c7d1e" : "635d6dee1c7831b41aa987bdbeff14bc"; } }
    }

    internal struct GiantPresentationScale
    {
        internal GiantPresentationScale(float x, float y, float z)
        { X = x; Y = y; Z = z; }
        internal readonly float X, Y, Z;
        internal GiantPresentationScale Multiply(float factor)
        { return new GiantPresentationScale(X * factor, Y * factor, Z * factor); }
    }

    /// <summary>
    /// A measured rest-pose giant and human on the same game-unit scale.
    /// No default calibration exists. The prefab and baseline scale must match
    /// the measured view; animated-frame bounds are not calibration evidence.
    /// </summary>
    internal sealed class GiantSummonHeightCalibration
    {
        internal GiantSummonHeightCalibration(string creatureKey, string prefabId,
            GiantPresentationScale baselineScale, float giantRestHeight,
            float humanRestHeight, float humanReferenceFeet, string evidenceIdentity)
        {
            CreatureKey = creatureKey; PrefabId = prefabId; BaselineScale = baselineScale;
            GiantRestHeight = giantRestHeight; HumanRestHeight = humanRestHeight;
            HumanReferenceFeet = humanReferenceFeet; EvidenceIdentity = evidenceIdentity;
        }
        internal string CreatureKey { get; private set; }
        internal string PrefabId { get; private set; }
        internal GiantPresentationScale BaselineScale { get; private set; }
        internal float GiantRestHeight { get; private set; }
        internal float HumanRestHeight { get; private set; }
        internal float HumanReferenceFeet { get; private set; }
        internal string EvidenceIdentity { get; private set; }
    }

    internal static class GiantSummonPresentationPolicy
    {
        internal const string NativeFrostGuid = "590cd3d5e76fdc649a5f97bc984cd3c4";
        private static readonly GiantSummonPresentationProfile[] Profiles = {
            new GiantSummonPresentationProfile("hill-giant", "58770207fd8c4084a0d9addbac591616", 10f),
            new GiantSummonPresentationProfile("stone-giant", "956501547dd54479b329a315aab07a05", 12f),
            new GiantSummonPresentationProfile("frost-giant", NativeFrostGuid, 15f),
            // The chosen art target is the midpoint of the attested 12-16 ft
            // range. It remains unapplied until native calibration exists.
            new GiantSummonPresentationProfile("fire-giant", "65481592185641c99c502e21ba9a5fa2", 14f),
            new GiantSummonPresentationProfile("cloud-giant", "ff4696c6336141a3b2236cb0a22c86f3", 18f),
            new GiantSummonPresentationProfile("storm-giant", "4b1143140c614a0aaaa1118c8a44c9ae", 21f)
        };

        // Exact released manifest identities, including caller-owned adapters.
        // A native Frost unit, native ability or KMG-looking name is insufficient.
        private static readonly KeyValuePair<string, string>[] FrostSources = {
            S("KMG.Summoning.NativeOption.SM.Tier8.FrostGiant.One", "2d7ae7107cd4403fa328b3b2a3a75989"),
            S("KMG.Summoning.NativeOption.SM.Tier9.FrostGiant.OneD3", "64355f9a3a5d4a66b0b5af1d247d94d5"),
            S("KMG.Summoning.NativeOption.SNA.Tier7.FrostGiant.One", "184cfe9d6203442ebe4af4ac717fdf6d"),
            S("KMG.Summoning.NativeOption.SNA.Tier8.FrostGiant.OneD3", "3afff2c5ea7943569e3d30457ccc6f3a"),
            S("KMG.Summoning.NativeOption.SNA.Tier9.FrostGiant.OneD4PlusOne", "52c367cec4b342bd86356769193e8f87"),
            S("KMG.Summoning.Adapter.MonsterTactician.NativeOption.Tier8.FrostGiant.One", "25c1875ab7685ce5950241465e6d74b4"),
            S("KMG.Summoning.Adapter.MonsterTactician.NativeOption.Tier9.FrostGiant.OneD3", "e5fa92a596fe589682ba0a6cb8d27695"),
            S("KMG.Summoning.Adapter.Summoner.NativeOption.Tier8.FrostGiant.One", "d3cbec7c8e6152b89a73e1999f39ecdc"),
            S("KMG.Summoning.Adapter.Summoner.NativeOption.Tier9.FrostGiant.OneD3", "2767c032afe050a2b73d819f6a400fc7"),
            S("KMG.Summoning.Adapter.MasterSummoner.NativeOption.Tier8.FrostGiant.One", "bafc31cbf40b55ada4341bc0ca31bca8"),
            S("KMG.Summoning.Adapter.MasterSummoner.NativeOption.Tier9.FrostGiant.OneD3", "43c37dd45049533588ad9f73ec4dd5a7"),
            S("KMG.Summoning.Adapter.Occultist.NativeOption.Tier8.FrostGiant.One", "0e743fae0bfd5247996011163934d837"),
            S("KMG.Summoning.Adapter.Occultist.NativeOption.Tier9.FrostGiant.OneD3", "165817f9601554748e0f9c82c1fe93be"),
            S("KMG.Summoning.Adapter.Naturalist.NativeOption.Tier7.FrostGiant.One", "2ca03b72816f594abb6ba52fb81e1179"),
            S("KMG.Summoning.Adapter.Naturalist.NativeOption.Tier8.FrostGiant.OneD3", "b1fe4b7e1c0f58dfa8611b2e5df7d25b"),
            S("KMG.Summoning.Adapter.Naturalist.NativeOption.Tier9.FrostGiant.OneD4PlusOne", "e264ab50a3f558eaa98d7f74cb47fdca"),
            S("KMG.Summoning.Adapter.FeralHunter.NativeOption.Tier7.FrostGiant.One", "ec34808b532552628ccd3e4388db8b3b"),
            S("KMG.Summoning.Adapter.FeralHunter.NativeOption.Tier8.FrostGiant.OneD3", "0d2c2bc7653b53868781fd6dbf98e724"),
            S("KMG.Summoning.Adapter.FeralHunter.NativeOption.Tier9.FrostGiant.OneD4PlusOne", "91938b93ada9575ca5bd9d9d9a32000a"),
            S("KMG.Summoning.Adapter.Preservationist.NativeOption.Tier7.FrostGiant.One", "1f57cf261c6654d1b7e47c9b413bd0a5"),
            S("KMG.Summoning.Adapter.Preservationist.NativeOption.Tier9.FrostGiant.OneD4PlusOne", "1985100d31ef5f8b8c22aa5f60c65e7a")
        };

        internal static IReadOnlyList<GiantSummonPresentationProfile> All
        { get { return Array.AsReadOnly(Profiles); } }
        internal static IReadOnlyList<KeyValuePair<string, string>> NativeFrostSources
        { get { return Array.AsReadOnly(FrostSources); } }

        internal static GiantSummonPresentationProfile Resolve(bool moduleActive,
            string unitGuid, string summonedBuffSourceAbilityGuid)
        {
            if (!moduleActive) return null;
            GiantSummonPresentationProfile profile = Profiles.SingleOrDefault(value =>
                string.Equals(value.UnitGuid, unitGuid, StringComparison.Ordinal));
            if (profile == null) return null;
            return profile.UnitGuid != NativeFrostGuid || FrostSources.Any(value =>
                string.Equals(value.Value, summonedBuffSourceAbilityGuid, StringComparison.Ordinal))
                ? profile : null;
        }

        // Fire and Storm reuse the native Frost Giant prefab, whose SpawnFxOnStart
        // carries that giant's own frost effects. Only these two owned bodies
        // drop them; native Frost and the Hill-prefab families are untouched.
        internal static bool SuppressesDonorFrostEffects(bool moduleActive, string unitGuid)
        {
            return moduleActive && Profiles.Any(value => (value.Key == "fire-giant" ||
                value.Key == "storm-giant") && string.Equals(value.UnitGuid, unitGuid, StringComparison.Ordinal));
        }

        internal static float SolePivotOffset(float nativeLocalSole, float factor)
        {
            if (float.IsNaN(nativeLocalSole) || float.IsInfinity(nativeLocalSole) || !Positive(factor))
                throw new ArgumentException("Finite native sole and positive visual ratio required.");
            float offset = nativeLocalSole * (1f - factor);
            if (float.IsNaN(offset) || float.IsInfinity(offset)) throw new ArgumentException("Visual sole compensation overflow.");
            return offset;
        }

        // Unity's 2018.4 ClothInspector uses this exact value for a disabled
        // maxDistance or collisionSphereDistance constraint. It is a native
        // sentinel, not a large enabled distance to scale or clamp.
        internal static float ScaleClothConstraint(float nativeDistance, float factor)
        {
            if (float.IsNaN(nativeDistance) || float.IsInfinity(nativeDistance) || nativeDistance < 0f || !Positive(factor))
                throw new ArgumentException("Finite nonnegative native Cloth constraint and positive ratio required.");
            if (nativeDistance == float.MaxValue) return nativeDistance;
            float result = nativeDistance * factor;
            if (float.IsNaN(result) || float.IsInfinity(result) || result == float.MaxValue && nativeDistance != float.MaxValue)
                throw new ArgumentException("Enabled Cloth constraint overflow or collision with the native disabled sentinel.");
            return result;
        }

        internal static string Calculate(GiantSummonPresentationProfile profile,
            GiantSummonHeightCalibration calibration, string actualPrefabId,
            GiantPresentationScale capturedBaseline, GiantPresentationScale current,
            out GiantPresentationScale target)
        {
            target = current;
            if (profile == null) return "not-owned";
            if (calibration == null) return "missing-calibration";
            if (calibration.CreatureKey != profile.Key ||
                calibration.PrefabId != actualPrefabId || string.IsNullOrWhiteSpace(actualPrefabId))
                return "calibration-identity-mismatch";
            if (string.IsNullOrWhiteSpace(calibration.EvidenceIdentity) ||
                !Positive(calibration.GiantRestHeight) || !Positive(calibration.HumanRestHeight) ||
                !Positive(calibration.HumanReferenceFeet) || !Valid(calibration.BaselineScale) ||
                !Valid(capturedBaseline) || !Valid(current)) return "invalid-calibration-or-scale";
            if (!Same(calibration.BaselineScale, capturedBaseline)) return "baseline-scale-mismatch";
            float multiplier = profile.ReferenceHeightFeet / calibration.HumanReferenceFeet *
                calibration.HumanRestHeight / calibration.GiantRestHeight;
            GiantPresentationScale desired = capturedBaseline.Multiply(multiplier);
            if (!Positive(multiplier) || !Valid(desired)) return "invalid-calibration-or-scale";
            if (Same(current, desired)) { target = desired; return "already-applied"; }
            if (!Same(current, capturedBaseline)) return "foreign-scale-change";
            target = desired;
            return "apply-measured-height";
        }

        internal static bool Same(GiantPresentationScale left, GiantPresentationScale right)
        { return Near(left.X, right.X) && Near(left.Y, right.Y) && Near(left.Z, right.Z); }
        private static bool Near(float left, float right)
        { return Math.Abs(left - right) <= 0.00001f * Math.Max(1f, Math.Max(Math.Abs(left), Math.Abs(right))); }
        private static bool Valid(GiantPresentationScale scale)
        { return Positive(scale.X) && Positive(scale.Y) && Positive(scale.Z); }
        private static bool Positive(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f; }
        private static KeyValuePair<string, string> S(string symbol, string guid)
        { return new KeyValuePair<string, string>(symbol, guid); }
    }

    /// <summary>
    /// Installed EntityFader.SetRendererVisibility (06001756) writes
    /// enabled = visible to every live entry of the view's current renderer
    /// list. Each captured original's authentic state is the last value that
    /// writer actually set on it, recorded before any ownership validation,
    /// so a refusal restores displaced originals to that write, never to an
    /// earlier one. Originals absent from a replaced or edited list were not
    /// written and keep their previous record.
    /// </summary>
    internal static class GiantSummonNativeVisibilityRecord
    {
        internal static void Record<T>(IDictionary<T, bool> record, IEnumerable<T> captured,
            IEnumerable<T> written, Func<T, bool> live, bool visible) where T : class
        {
            if (record == null || captured == null || written == null || live == null) return;
            T[] wrote = written.Where(value => value != null && live(value)).ToArray();
            foreach (T value in captured)
                if (value != null && wrote.Any(item => ReferenceEquals(item, value))) record[value] = visible;
        }

        internal static bool Expected<T>(IDictionary<T, bool> record, T renderer, bool intake) where T : class
        {
            bool written;
            return record != null && renderer != null && record.TryGetValue(renderer, out written) ? written : intake;
        }
    }
}
