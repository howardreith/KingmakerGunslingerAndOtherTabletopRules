using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    internal enum OriginalIconDisposition { Missing, Unqualified, Qualified }

    // Build/intake evidence only; no image creation, fallback, donor, texture or
    // runtime JSON authoring-catalog loader. Future intake freezes reviewed hashes.
    internal sealed class OriginalIconEvidence
    {
        internal OriginalIconEvidence(string key, string sourcePath, string exportPath, string runtimePath,
            string sourceHash, string exportHash, string qualifiedHash, int sourceWidth, int sourceHeight,
            int exportWidth, int exportHeight, string family, string profile, string reviewGroup,
            bool original, bool provenance, bool sourcePng, bool rgbaExport, bool catalogValid,
            bool protectedAssignmentsValid)
        {
            Key=key; SourcePath=sourcePath; ExportPath=exportPath; RuntimePath=runtimePath;
            SourceHash=sourceHash; ExportHash=exportHash; QualifiedExportHash=qualifiedHash;
            SourceWidth=sourceWidth; SourceHeight=sourceHeight; ExportWidth=exportWidth; ExportHeight=exportHeight;
            Family=family; Profile=profile; ReviewGroup=reviewGroup; Original=original;
            Provenance=provenance; SourcePng=sourcePng; RgbaExport=rgbaExport;
            CatalogValid=catalogValid; ProtectedAssignmentsValid=protectedAssignmentsValid;
        }
        internal string Key { get; private set; }
        internal string SourcePath { get; private set; }
        internal string ExportPath { get; private set; }
        internal string RuntimePath { get; private set; }
        internal string SourceHash { get; private set; }
        internal string ExportHash { get; private set; }
        internal string QualifiedExportHash { get; private set; }
        internal int SourceWidth { get; private set; }
        internal int SourceHeight { get; private set; }
        internal int ExportWidth { get; private set; }
        internal int ExportHeight { get; private set; }
        internal string Family { get; private set; }
        internal string Profile { get; private set; }
        internal string ReviewGroup { get; private set; }
        internal bool Original { get; private set; }
        internal bool Provenance { get; private set; }
        internal bool SourcePng { get; private set; }
        internal bool RgbaExport { get; private set; }
        internal bool CatalogValid { get; private set; }
        internal bool ProtectedAssignmentsValid { get; private set; }
    }

    internal sealed class ElementalCharacterTraitAssetDecision
    {
        private readonly OriginalIconDisposition[] _states;
        internal ElementalCharacterTraitAssetDecision(OriginalIconDisposition[] states, bool exactSet)
        { _states=(OriginalIconDisposition[])states.Clone(); ExactSet=exactSet; }
        internal bool ExactSet { get; private set; }
        internal OriginalIconDisposition For(ElementalCharacterTraitId id) { return _states[(int)id]; }
        internal bool Ready { get { return ExactSet && _states.All(s => s == OriginalIconDisposition.Qualified); } }
        internal void RequireReady()
        {
            if (!Ready) throw new InvalidOperationException("BLOCKED-ONLY-ON-ORIGINAL-ICONS: no visible construction, localization or publication.");
        }
    }

    internal static class ElementalCharacterTraitAssetGate
    {
        internal static ElementalCharacterTraitAssetDecision Evaluate(IEnumerable<OriginalIconEvidence> evidence)
        {
            var all=(evidence ?? Enumerable.Empty<OriginalIconEvidence>()).ToArray();
            var definitions=ElementalCharacterTraitCatalog.All();
            bool exact=all.Length == 4 && all.All(e => e != null && definitions.Any(d => d.IconKey == e.Key)) &&
                all.Select(e => e == null ? null : e.Key).Distinct(StringComparer.Ordinal).Count() == 4;
            var states=definitions.Select(d =>
            {
                var matches=all.Where(e => e != null && e.Key == d.IconKey).ToArray();
                if (matches.Length == 0) return OriginalIconDisposition.Missing;
                if (matches.Length != 1 || !Qualified(d,matches[0])) return OriginalIconDisposition.Unqualified;
                var receipt=matches[0];
                if (all.Any(other => other != null && !ReferenceEquals(receipt,other) &&
                    (other.SourceHash == receipt.SourceHash || other.ExportHash == receipt.ExportHash)))
                    return OriginalIconDisposition.Unqualified;
                return OriginalIconDisposition.Qualified;
            }).ToArray();
            return new ElementalCharacterTraitAssetDecision(states,exact);
        }
        private static bool Hash(string value)
        { return value != null && value.Length == 64 && value.All(c => "0123456789abcdef".Contains(c)) && value != new string('0',64); }
        private static bool Qualified(ElementalCharacterTraitDefinition d, OriginalIconEvidence e)
        {
            return e.SourcePath == d.OriginalPath && e.ExportPath == d.ExportPath && e.RuntimePath == d.RuntimePath &&
                Hash(e.SourceHash) && Hash(e.ExportHash) && e.QualifiedExportHash == e.ExportHash &&
                e.SourceWidth >= 1024 && e.SourceHeight == e.SourceWidth && e.ExportWidth == 128 && e.ExportHeight == 128 &&
                e.Family == ElementalCharacterTraitCatalog.Family && e.Profile == ElementalCharacterTraitCatalog.OriginalProfile &&
                e.ReviewGroup == d.ReviewGroup && e.Original && e.Provenance && e.SourcePng && e.RgbaExport &&
                e.CatalogValid && e.ProtectedAssignmentsValid;
        }
    }

    internal static class ElementalCharacterTraitAssetCatalog
    {
        // Original generated sources were admitted by exact objective review under the owner's
        // 2026-10-06 continuation authority. Owner aesthetic approval remains NOT_RECORDED.
        // The native factory reads only this immutable compiled catalog, never injected receipts.
        internal static OriginalIconEvidence[] Current()
        {
            return new[]
            {
                new OriginalIconEvidence("fiery-glare", "assets-source/original-icons/icon-overhaul-v2/production/sources/fiery-glare.png", "assets-source/original-icons/icon-overhaul-v2/production/exports/fiery-glare.png", "assets/game/icons/fiery-glare.png",
                    "a3ed28c9a92940f60cee501ecd429e7e4d7c5bd7d8f6f209124985d6863005e3", "0ff889c3321e0d30b82e9d8c2fc5b80f81f134b48c636f6fc231826fccf993f5", "0ff889c3321e0d30b82e9d8c2fc5b80f81f134b48c636f6fc231826fccf993f5", 1254, 1254, 128, 128,
                    "painted-magical", "project-painted-128", "ifrit-traits", true, true, true, true, true, true),
                new OriginalIconEvidence("stoic-dignity", "assets-source/original-icons/icon-overhaul-v2/production/sources/stoic-dignity.png", "assets-source/original-icons/icon-overhaul-v2/production/exports/stoic-dignity.png", "assets/game/icons/stoic-dignity.png",
                    "f821992b390be1cc214a137f54c4122c2d460c542805023359a79bcb66849ac3", "e1f619d17470cff018799db9b1c6554cf11e6b0626723360603f332ce7fb6f51", "e1f619d17470cff018799db9b1c6554cf11e6b0626723360603f332ce7fb6f51", 1254, 1254, 128, 128,
                    "painted-magical", "project-painted-128", "oread-traits", true, true, true, true, true, true),
                new OriginalIconEvidence("aerial-observer", "assets-source/original-icons/icon-overhaul-v2/production/sources/aerial-observer.png", "assets-source/original-icons/icon-overhaul-v2/production/exports/aerial-observer.png", "assets/game/icons/aerial-observer.png",
                    "77d3196f92d4d11fee14d13d3b32de5063c8bf27b99fa6ca0295e02bd87b920e", "462408b925f912e2a1063ac9e8a0ff25b33c68b80fcccb1fa85d05f706fc117f", "462408b925f912e2a1063ac9e8a0ff25b33c68b80fcccb1fa85d05f706fc117f", 1254, 1254, 128, 128,
                    "painted-magical", "project-painted-128", "sylph-traits", true, true, true, true, true, true),
                new OriginalIconEvidence("whiteout", "assets-source/original-icons/icon-overhaul-v2/production/sources/whiteout.png", "assets-source/original-icons/icon-overhaul-v2/production/exports/whiteout.png", "assets/game/icons/whiteout.png",
                    "86bce8afb3482f2d317809153f228dcbb76da351283b46553c186ed317f076dc", "3afdb75457b431530356e26ce32c005d2e8dbb8634474e20814b6cfa6d1e9f6b", "3afdb75457b431530356e26ce32c005d2e8dbb8634474e20814b6cfa6d1e9f6b", 1254, 1254, 128, 128,
                    "painted-magical", "project-painted-128", "undine-traits", true, true, true, true, true, true),
            };
        }
    }

    internal sealed class ElementalCharacterTraitPublicationConditions
    {
        internal ElementalCharacterTraitPublicationConditions(bool favoredClassPresent, bool compatible,
            bool traitsEnabled, bool moduleEnabled, bool graphsValid, string selectionGuid,
            ElementalCharacterTraitAssetDecision assets)
        {
            FavoredClassPresent=favoredClassPresent; Compatible=compatible; TraitsEnabled=traitsEnabled;
            ModuleEnabled=moduleEnabled; GraphsValid=graphsValid; SelectionGuid=selectionGuid; Assets=assets;
        }
        internal bool FavoredClassPresent { get; private set; }
        internal bool Compatible { get; private set; }
        internal bool TraitsEnabled { get; private set; }
        internal bool ModuleEnabled { get; private set; }
        internal bool GraphsValid { get; private set; }
        internal string SelectionGuid { get; private set; }
        internal ElementalCharacterTraitAssetDecision Assets { get; private set; }
        internal bool Ready
        {
            get { return FavoredClassPresent && Compatible && TraitsEnabled && ModuleEnabled && GraphsValid &&
                SelectionGuid == ElementalCharacterTraitCatalog.SelectionGuid && Assets != null && Assets.Ready; }
        }
        internal void RequireReady()
        { if (!Ready) throw new InvalidOperationException("The exact all-four character-trait publication contract is not ready."); }
    }
}
