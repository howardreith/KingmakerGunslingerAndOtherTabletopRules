using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// What the adapter observed about the loaded Favored Class host and its
    /// Call of the Wild dependency. Gathered by reflection only; no host code
    /// is executed.
    /// </summary>
    internal sealed class FavoredClassHostObservation
    {
        internal FavoredClassHostObservation()
        {
            MethodIlSha256 = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        internal bool HostModInstalled { get; set; }
        internal bool HostModEnabled { get; set; }
        internal string HostModVersion { get; set; }
        internal bool HostAssemblyLoaded { get; set; }
        internal string HostAssemblyName { get; set; }
        internal string HostModuleVersionId { get; set; }
        internal string HostFileSha256 { get; set; }

        internal bool CallOfTheWildAssemblyLoaded { get; set; }
        internal string CallOfTheWildModuleVersionId { get; set; }
        internal string CallOfTheWildFileSha256 { get; set; }

        /// <summary>First required type or member that was absent or malformed.</summary>
        internal string MissingMember { get; set; }

        /// <summary>SHA-256 of each fingerprinted method's IL body, by contract key.</summary>
        internal Dictionary<string, string> MethodIlSha256 { get; private set; }
    }

    /// <summary>
    /// Initialization facts read after every LoadDictionary postfix ran. A
    /// non-null host library field is never readiness evidence on its own.
    /// </summary>
    internal sealed class FavoredClassHostReadinessObservation
    {
        internal bool LibraryAssigned { get; set; }

        /// <summary>
        /// <c>Core.prestigious_spellcaster</c> is assigned by the last step of
        /// <c>Core.load()</c>, after the custom JSON pass.
        /// </summary>
        internal bool CoreLoadCompleted { get; set; }

        internal bool FavoredClassSelectionPresent { get; set; }
        internal string GunslingerClassGuid { get; set; }
        internal string GunslingerProgressionGuid { get; set; }
        internal string GunslingerBonusSelectionGuid { get; set; }
        internal bool GunslingerProgressionOffered { get; set; }
        internal int GunslingerProgressionLevels { get; set; }
        internal bool GunslingerLevelsGrantBonusSelection { get; set; }
        internal bool GenericHitPointLeafPresent { get; set; }
        internal bool GenericSkillLeavesPresent { get; set; }
    }

    internal enum FavoredClassHostState
    {
        /// <summary>The host is not installed or not loaded.</summary>
        Absent,
        /// <summary>The host is installed but disabled in Unity Mod Manager.</summary>
        Disabled,
        /// <summary>A binary other than the exact qualified profile.</summary>
        UnsupportedBinary,
        /// <summary>Qualified binary whose initialization did not complete.</summary>
        IncompleteInitialization,
        /// <summary>The host initialized without a Gunslinger favored-class entry.</summary>
        GunslingerMissing,
        /// <summary>Exact binary, complete initialization, Gunslinger scanned.</summary>
        Ready
    }

    internal sealed class FavoredClassHostDecision
    {
        private FavoredClassHostDecision(FavoredClassHostState state, string reason,
            string detail)
        {
            State = state;
            Reason = reason ?? string.Empty;
            Detail = detail ?? string.Empty;
        }

        internal FavoredClassHostState State { get; private set; }
        internal string Reason { get; private set; }
        internal string Detail { get; private set; }
        internal bool IsReady { get { return State == FavoredClassHostState.Ready; } }

        internal static FavoredClassHostDecision Of(FavoredClassHostState state,
            string reason, string detail)
        {
            return new FavoredClassHostDecision(state, reason, detail);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Detail)
                ? string.Format(CultureInfo.InvariantCulture, "{0} ({1})", State, Reason)
                : string.Format(CultureInfo.InvariantCulture, "{0} ({1}: {2})", State, Reason,
                    Detail);
        }
    }

    /// <summary>
    /// The verified Favored Class host contract, established by read-only
    /// decompilation and hashing of the installed distribution: UMM entry
    /// ZFavoredClass 1.3.1, file SHA-256
    /// dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c, MVID
    /// 3efd38e7-8682-4b4d-8d53-e368a3664919, with its dependency Call of the
    /// Wild 1.14.4c-2.1 (SHA-256
    /// 4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915, MVID
    /// 8caab254-aacf-4811-8093-44b9184e6e53). Support is limited to exactly
    /// that pair: an unknown, rebuilt or unreadable binary leaves only this
    /// integration inactive. The method-body fingerprints are consistency
    /// checks on the members whose semantics KMG relies on.
    /// </summary>
    internal static class FavoredClassHostContract
    {
        internal const string HostModId = "ZFavoredClass";
        internal const string HostAssemblyName = "ZFavoredClass";
        internal const string VerifiedHostModVersion = "1.3.1";
        internal const string VerifiedHostModuleVersionId = "3efd38e7-8682-4b4d-8d53-e368a3664919";
        internal const string VerifiedHostFileSha256 =
            "dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c";

        internal const string CallOfTheWildAssemblyName = "CallOfTheWild";
        internal const string VerifiedCallOfTheWildModuleVersionId =
            "8caab254-aacf-4811-8093-44b9184e6e53";
        internal const string VerifiedCallOfTheWildFileSha256 =
            "4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915";

        internal const string CoreTypeName = "ZFavoredClass.Core";
        internal const string MainTypeName = "ZFavoredClass.Main";
        internal const string PrerequisiteRaceTypeName = "ZFavoredClass.NewMechanics.PrerequisiteRace";
        internal const string FullRankPrerequisiteTypeName =
            "CallOfTheWild.NewMechanics.PrerequisiteFeatureFullRank";

        internal const string ProgressionMapField = "class_guid_progression_map";
        internal const string BonusSelectionMapField = "class_guid_bonus_selection_map";
        internal const string FavoredClassSelectionField = "favored_class_selection";
        internal const string FavoredHitPointsField = "favored_hp";
        internal const string FavoredSkillField = "favored_skill";
        internal const string PrestigiousSpellcasterField = "prestigious_spellcaster";
        internal const string LibraryField = "library";

        internal const string PrerequisiteRaceCheckKey = "PrerequisiteRace.Check";
        internal const string CoreLoadKey = "Core.load";
        internal const string AddFavoredClassBonusKey = "Core.addFavoredClassBonus";
        internal const string LoadCustomFeatureKey = "Core.loadCustomFeature";
        internal const string LoadDictionaryPostfixKey = "Main.LoadDictionary.Postfix";
        internal const string FullRankCheckKey = "PrerequisiteFeatureFullRank.Check";

        /// <summary>Host MergeIds seed for per-class favored progressions.</summary>
        internal const string ProgressionSeed = "602ea6032c324258a183588f84522ea1";

        /// <summary>Host MergeIds seed for per-class favored bonus selections.</summary>
        internal const string BonusSelectionSeed = "f431abc7ab7b4771a58fff7ee2af8a01";

        /// <summary>Levels the host gives a base-class favored progression.</summary>
        internal const int BaseClassProgressionLevels = 20;

        private static readonly Dictionary<string, string> VerifiedIl =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { PrerequisiteRaceCheckKey,
                    "083d124cf6ec67b7622471bcafdaa25d6b7365bf821ba0af1aeac8bb3022ff2a" },
                { CoreLoadKey,
                    "45a5e19f74b4dd89967e1e61d4b86dd9a4db074a9d8dfe206687d6bf1dcf4c94" },
                { AddFavoredClassBonusKey,
                    "19b7bfe4f7a9b6308642cd192fcddaa4d70f9db70a3cdc7d44fbb948fdf795a6" },
                { LoadCustomFeatureKey,
                    "6076940a4512984048b8df957545091f09fb3005380acb6cba71795fa000620e" },
                { LoadDictionaryPostfixKey,
                    "0a7b468774d41414a7d66a42cc266b292dfe7eb8d3fe774ebcdea12537b5b85b" },
                { FullRankCheckKey,
                    "3bababf7ac8e38f0da58347198ea3aeefd247eafcf40b7d4b01d86a33fddfaf6" },
            };

        internal static string[] FingerprintKeys
        {
            get { return VerifiedIl.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray(); }
        }

        internal static string VerifiedIlSha256(string key)
        {
            string value;
            if (!VerifiedIl.TryGetValue(key, out value))
                throw new KeyNotFoundException("Unknown fingerprint key " + key);
            return value;
        }

        /// <summary>
        /// Call of the Wild's <c>Helpers.MergeIds</c>: a 128-bit XOR of two
        /// 32-digit hexadecimal identities, rendered as lowercase hexadecimal.
        /// </summary>
        internal static string MergeIds(string first, string second)
        {
            ulong firstHigh = ParseHalf(first, 0);
            ulong firstLow = ParseHalf(first, 16);
            ulong secondHigh = ParseHalf(second, 0);
            ulong secondLow = ParseHalf(second, 16);
            return (firstHigh ^ secondHigh).ToString("x16", CultureInfo.InvariantCulture) +
                (firstLow ^ secondLow).ToString("x16", CultureInfo.InvariantCulture);
        }

        internal static string ExpectedProgressionGuid(string classGuid)
        {
            return MergeIds(ProgressionSeed, classGuid);
        }

        internal static string ExpectedBonusSelectionGuid(string classGuid)
        {
            return MergeIds(BonusSelectionSeed, classGuid);
        }

        /// <summary>
        /// The binary gate: installed, enabled, loaded, exact file SHA-256 and
        /// MVID for both the host and Call of the Wild, required members and
        /// method fingerprints. The UMM version label is diagnostic only.
        /// </summary>
        internal static FavoredClassHostDecision EvaluateBinary(FavoredClassHostObservation observed)
        {
            if (observed == null || !observed.HostModInstalled)
                return FavoredClassHostDecision.Of(FavoredClassHostState.Absent, "not-installed", null);
            if (!observed.HostModEnabled)
                return FavoredClassHostDecision.Of(FavoredClassHostState.Disabled, "disabled", null);
            if (!observed.HostAssemblyLoaded)
                return FavoredClassHostDecision.Of(FavoredClassHostState.Absent, "not-loaded", null);
            if (!string.Equals(observed.HostAssemblyName, HostAssemblyName, StringComparison.Ordinal))
                return Unsupported("assembly-name", observed.HostAssemblyName);
            if (!string.Equals(observed.HostFileSha256, VerifiedHostFileSha256,
                    StringComparison.OrdinalIgnoreCase))
                return Unsupported("binary-sha256", observed.HostFileSha256);
            if (!string.Equals(observed.HostModuleVersionId, VerifiedHostModuleVersionId,
                    StringComparison.OrdinalIgnoreCase))
                return Unsupported("binary-mvid", observed.HostModuleVersionId);
            if (!observed.CallOfTheWildAssemblyLoaded)
                return Unsupported("dependency-not-loaded", CallOfTheWildAssemblyName);
            if (!string.Equals(observed.CallOfTheWildFileSha256, VerifiedCallOfTheWildFileSha256,
                    StringComparison.OrdinalIgnoreCase))
                return Unsupported("dependency-sha256", observed.CallOfTheWildFileSha256);
            if (!string.Equals(observed.CallOfTheWildModuleVersionId,
                    VerifiedCallOfTheWildModuleVersionId, StringComparison.OrdinalIgnoreCase))
                return Unsupported("dependency-mvid", observed.CallOfTheWildModuleVersionId);
            if (!string.IsNullOrEmpty(observed.MissingMember))
                return Unsupported("required-member", observed.MissingMember);
            foreach (KeyValuePair<string, string> expected in VerifiedIl.OrderBy(
                value => value.Key, StringComparer.Ordinal))
            {
                string actual;
                if (!observed.MethodIlSha256.TryGetValue(expected.Key, out actual) ||
                    !string.Equals(actual, expected.Value, StringComparison.OrdinalIgnoreCase))
                    return Unsupported("unverified-behavior:" + expected.Key, actual);
            }
            return FavoredClassHostDecision.Of(FavoredClassHostState.Ready, "binary-qualified", null);
        }

        /// <summary>
        /// The initialization gate, evaluated only after the binary gate
        /// passed and every LoadDictionary postfix completed.
        /// </summary>
        internal static FavoredClassHostDecision EvaluateReadiness(
            FavoredClassHostDecision binary, FavoredClassHostReadinessObservation observed)
        {
            if (binary == null)
                throw new ArgumentNullException("binary");
            if (!binary.IsReady)
                return binary;
            if (observed == null || !observed.LibraryAssigned)
                return Incomplete("library-unassigned");
            if (!observed.CoreLoadCompleted)
                return Incomplete("core-load-incomplete");
            if (!observed.FavoredClassSelectionPresent)
                return Incomplete("favored-class-selection-missing");
            if (!observed.GenericHitPointLeafPresent || !observed.GenericSkillLeavesPresent)
                return Incomplete("generic-rewards-missing");
            if (string.IsNullOrEmpty(observed.GunslingerProgressionGuid) ||
                string.IsNullOrEmpty(observed.GunslingerBonusSelectionGuid))
                return FavoredClassHostDecision.Of(FavoredClassHostState.GunslingerMissing,
                    "gunslinger-not-scanned", observed.GunslingerClassGuid);
            if (!string.Equals(observed.GunslingerProgressionGuid,
                    ExpectedProgressionGuid(observed.GunslingerClassGuid), StringComparison.Ordinal) ||
                !string.Equals(observed.GunslingerBonusSelectionGuid,
                    ExpectedBonusSelectionGuid(observed.GunslingerClassGuid), StringComparison.Ordinal))
                return FavoredClassHostDecision.Of(FavoredClassHostState.GunslingerMissing,
                    "gunslinger-identity-mismatch", observed.GunslingerProgressionGuid + "/" +
                    observed.GunslingerBonusSelectionGuid);
            if (!observed.GunslingerProgressionOffered)
                return FavoredClassHostDecision.Of(FavoredClassHostState.GunslingerMissing,
                    "gunslinger-progression-not-offered", null);
            if (observed.GunslingerProgressionLevels != BaseClassProgressionLevels ||
                !observed.GunslingerLevelsGrantBonusSelection)
                return FavoredClassHostDecision.Of(FavoredClassHostState.GunslingerMissing,
                    "gunslinger-progression-shape",
                    observed.GunslingerProgressionLevels.ToString(CultureInfo.InvariantCulture));
            return FavoredClassHostDecision.Of(FavoredClassHostState.Ready, "ready", null);
        }

        private static FavoredClassHostDecision Unsupported(string reason, string detail)
        {
            return FavoredClassHostDecision.Of(FavoredClassHostState.UnsupportedBinary, reason,
                detail ?? "<absent>");
        }

        private static FavoredClassHostDecision Incomplete(string reason)
        {
            return FavoredClassHostDecision.Of(FavoredClassHostState.IncompleteInitialization,
                reason, null);
        }

        private static ulong ParseHalf(string guid, int offset)
        {
            if (guid == null || guid.Length != 32)
                throw new FormatException("Expected a 32-digit hexadecimal identity.");
            return ulong.Parse(guid.Substring(offset, 16), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);
        }
    }
}
