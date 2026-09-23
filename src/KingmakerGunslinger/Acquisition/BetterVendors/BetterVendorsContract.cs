using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    /// <summary>
    /// What the adapter observed about the loaded Better Vendors assembly.
    /// Gathered by reflection only; nothing in Better Vendors is executed.
    /// </summary>
    internal sealed class BetterVendorsContractObservation
    {
        internal BetterVendorsContractObservation()
        {
            MethodIlSha256 = new Dictionary<string, string>(
                StringComparer.Ordinal);
        }

        internal string ModVersion { get; set; }
        internal string AssemblyName { get; set; }
        internal string AssemblyVersion { get; set; }
        internal string ModuleVersionId { get; set; }
        internal string FileSha256 { get; set; }

        /// <summary>First required type/member that was absent or malformed.</summary>
        internal string MissingMember { get; set; }

        /// <summary>SHA-256 of each fingerprinted method's IL body, by contract key.</summary>
        internal Dictionary<string, string> MethodIlSha256 { get; private set; }

        internal string MilitaryDestinationGuid { get; set; }
        internal string[] EnhancementLevelGuids { get; set; }
        internal string LoadPatchTarget { get; set; }
        internal string ImproveStatPatchTarget { get; set; }
    }

    internal sealed class BetterVendorsContractDecision
    {
        private BetterVendorsContractDecision(bool compatible,
            string failedCheck, string detail)
        {
            IsCompatible = compatible;
            FailedCheck = failedCheck ?? string.Empty;
            Detail = detail ?? string.Empty;
        }

        internal bool IsCompatible { get; private set; }
        internal string FailedCheck { get; private set; }
        internal string Detail { get; private set; }

        internal static BetterVendorsContractDecision Compatible()
        { return new BetterVendorsContractDecision(true, null, null); }

        internal static BetterVendorsContractDecision Incompatible(
            string failedCheck, string detail)
        { return new BetterVendorsContractDecision(false, failedCheck, detail); }
    }

    /// <summary>
    /// The verified Better Vendors runtime contract. It was established by
    /// read-only decompilation and reflection over the installed distribution:
    /// UMM entry "BetterVendors" version 2.0.8, assembly BetterVendors
    /// 1.0.0.39014, MVID 04fc03cf-853f-46c8-b6d5-1404180451fb, file SHA-256
    /// 8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009
    /// (HarmonyLib 2.0.2). The public GitHub source (tag 1.0.7, commit
    /// 3cb12966) is older: it uses Harmony12, stocks arcane items at Arsinoe
    /// rather than Zarcie, and lacks the swallow-all try/catch in AddStock. The
    /// Military weapon schedule is identical in both.
    ///
    /// Support is limited to that exact inspected binary. The whole-file
    /// SHA-256 of the loaded assembly's file and the loaded module's MVID must
    /// both match, so an unknown, rebuilt or unreadable binary leaves only this
    /// integration inactive. The structural checks then resolve the members
    /// the hooks need. The method-body fingerprints are kept as a consistency
    /// check only: instruction bytes alone cover neither called helpers nor
    /// exception-handling clauses, so they cannot establish equivalence.
    /// </summary>
    internal static class BetterVendorsContract
    {
        internal const string ModId = "BetterVendors";
        internal const string AssemblyName = "BetterVendors";
        internal const string VerifiedModVersion = "2.0.8";
        internal const string VerifiedAssemblyVersion = "1.0.0.39014";
        internal const string VerifiedModuleVersionId =
            "04fc03cf-853f-46c8-b6d5-1404180451fb";
        internal const string VerifiedFileSha256 =
            "8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009";

        internal const string ProgressionLogicTypeName =
            "BetterVendors.Vendors.ProgressionLogic";
        internal const string LoadPatchTypeName =
            "BetterVendors.Vendors.ProgressionLogic+GameLoadGamePatch";
        internal const string ImproveStatPatchTypeName =
            "BetterVendors.Vendors.ProgressionLogic+KingdomActionsImproveStatPatch";
        internal const string SettingsWrapperTypeName =
            "BetterVendors.Utilities.SettingsWrapper";
        internal const string MainTypeName = "BetterVendors.Main";

        internal const string AddStockKey = "ProgressionLogic.AddStock";
        internal const string AddMilitaryStockKey =
            "ProgressionLogic.AddMilitaryStock";
        internal const string GetFilterWeaponsKey =
            "ProgressionLogic.GetFilterWeapons";
        internal const string LoadPatchKey = "GameLoadGamePatch.Postfix";
        internal const string ImproveStatPatchKey =
            "KingdomActionsImproveStatPatch.Postfix";

        /// <summary>Better Vendors' own name for its Military stock destination.</summary>
        internal const string MilitaryDestinationKey = "Verdel";

        /// <summary>
        /// SmithVendorTable, shared by the capital blacksmith and the Better
        /// Vendors throne-room clone of the same merchant.
        /// </summary>
        internal const string MilitaryDestinationTableGuid =
            "7de959347266092448d8a72089ef9778";

        internal const string LoadPatchTarget = "Kingmaker.Player.PostLoad";
        internal const string ImproveStatPatchTarget =
            "Kingmaker.Kingdom.Actions.KingdomActionImproveStat.RunAction";

        private static readonly string[] EnhancementGuids =
        {
            "d42fc23b92c640846ac137dc26e000d4",
            "eb2faccc4c9487d43b3575d7e77ff3f5",
            "80bb8a737579e35498177e1e3c75899b",
            "783d7d496da6ac44f9511011fc5f1979",
            "bdba267e951851449af552aa9f9e3992"
        };

        private static readonly Dictionary<string, string> VerifiedIl =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { AddStockKey,
                    "4dacfe1ae7892cbab652e7f800110589ed1faeda65c33afd67b1cefbc87bfc0e" },
                { AddMilitaryStockKey,
                    "232246b356d95af5fc57e7a72c8e9cc43349414c2e1250ad37c91ad31bfc25d4" },
                { GetFilterWeaponsKey,
                    "a05f2427d5fd6505e0a2fb43c86b4608fa74ed266720190e01ac0a38b2c19f1f" },
                { LoadPatchKey,
                    "0c0ca090d9078cf203fced83f4c335a133c20db175388bc80471e4c20905ba0f" },
                { ImproveStatPatchKey,
                    "a1f8b5ea82087c4e5bafeb90194d8b41011da16acc18bf65b5d69ab1aa1aad1c" }
            };

        internal static string[] FingerprintKeys
        {
            get { return VerifiedIl.Keys.OrderBy(value => value,
                StringComparer.Ordinal).ToArray(); }
        }

        internal static string VerifiedIlSha256(string key)
        {
            string value;
            if (!VerifiedIl.TryGetValue(key, out value))
                throw new KeyNotFoundException("Unknown fingerprint key " + key);
            return value;
        }

        /// <summary>Native actual-enhancement weapon enchantment for +1..+5.</summary>
        internal static string EnhancementGuid(int tier)
        {
            if (tier < 1 || tier > EnhancementGuids.Length)
                throw new ArgumentOutOfRangeException("tier");
            return EnhancementGuids[tier - 1];
        }

        /// <summary>
        /// The exact approved binary (whole-file SHA-256 and loaded-module
        /// MVID) is the gate. Structure, destination, enhancement tiers,
        /// lifecycle targets and the method-body fingerprints must also match.
        /// The UMM version label is recorded for diagnostics only.
        /// </summary>
        internal static BetterVendorsContractDecision Evaluate(
            BetterVendorsContractObservation observed)
        {
            if (observed == null)
                return BetterVendorsContractDecision.Incompatible(
                    "observation-missing", null);
            if (!string.Equals(observed.AssemblyName, AssemblyName,
                    StringComparison.Ordinal))
                return BetterVendorsContractDecision.Incompatible(
                    "assembly-name", observed.AssemblyName);
            if (!string.Equals(observed.FileSha256, VerifiedFileSha256,
                    StringComparison.OrdinalIgnoreCase))
                return BetterVendorsContractDecision.Incompatible(
                    "binary-sha256", observed.FileSha256 ?? "<absent>");
            if (!string.Equals(observed.ModuleVersionId, VerifiedModuleVersionId,
                    StringComparison.OrdinalIgnoreCase))
                return BetterVendorsContractDecision.Incompatible(
                    "binary-mvid", observed.ModuleVersionId ?? "<absent>");
            if (!string.IsNullOrEmpty(observed.MissingMember))
                return BetterVendorsContractDecision.Incompatible(
                    "required-member", observed.MissingMember);
            if (!string.Equals(observed.MilitaryDestinationGuid,
                    MilitaryDestinationTableGuid, StringComparison.Ordinal))
                return BetterVendorsContractDecision.Incompatible(
                    "military-destination", observed.MilitaryDestinationGuid);
            if (observed.EnhancementLevelGuids == null ||
                !observed.EnhancementLevelGuids.SequenceEqual(EnhancementGuids,
                    StringComparer.Ordinal))
                return BetterVendorsContractDecision.Incompatible(
                    "enhancement-levels", observed.EnhancementLevelGuids == null
                        ? "<absent>" : string.Join(",",
                            observed.EnhancementLevelGuids));
            if (!string.Equals(observed.LoadPatchTarget, LoadPatchTarget,
                    StringComparison.Ordinal))
                return BetterVendorsContractDecision.Incompatible(
                    "load-trigger", observed.LoadPatchTarget);
            if (!string.Equals(observed.ImproveStatPatchTarget,
                    ImproveStatPatchTarget, StringComparison.Ordinal))
                return BetterVendorsContractDecision.Incompatible(
                    "rank-trigger", observed.ImproveStatPatchTarget);
            foreach (KeyValuePair<string, string> expected in VerifiedIl.OrderBy(
                value => value.Key, StringComparer.Ordinal))
            {
                string actual;
                if (!observed.MethodIlSha256.TryGetValue(expected.Key,
                        out actual) ||
                    !string.Equals(actual, expected.Value,
                        StringComparison.Ordinal))
                    return BetterVendorsContractDecision.Incompatible(
                        "unverified-behavior:" + expected.Key,
                        actual ?? "<absent>");
            }
            return BetterVendorsContractDecision.Compatible();
        }
    }
}
