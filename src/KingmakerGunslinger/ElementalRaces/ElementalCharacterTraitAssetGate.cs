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
            string sourceHash, string exportHash, string approvedHash, int sourceWidth, int sourceHeight,
            int exportWidth, int exportHeight, string family, string profile, string reviewGroup,
            bool original, bool provenance, bool sourcePng, bool rgbaExport, bool catalogValid,
            bool protectedAssignmentsValid)
        {
            Key=key; SourcePath=sourcePath; ExportPath=exportPath; RuntimePath=runtimePath;
            SourceHash=sourceHash; ExportHash=exportHash; ApprovedHash=approvedHash;
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
        internal string ApprovedHash { get; private set; }
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
                Hash(e.SourceHash) && Hash(e.ExportHash) && e.ApprovedHash == e.ExportHash &&
                e.SourceWidth >= 128 && e.SourceHeight >= 128 && e.ExportWidth == 128 && e.ExportHeight == 128 &&
                e.Family == ElementalCharacterTraitCatalog.Family && e.Profile == ElementalCharacterTraitCatalog.OriginalProfile &&
                e.ReviewGroup == d.ReviewGroup && e.Original && e.Provenance && e.SourcePng && e.RgbaExport &&
                e.CatalogValid && e.ProtectedAssignmentsValid;
        }
    }

    internal static class ElementalCharacterTraitAssetCatalog
    {
        // Intentionally empty. The four original sources/exports/approved receipts
        // do not exist. Only a later validated original-art intake may add records.
        // An arbitrary caller cannot supply hypothetical test evidence to the
        // native factory: it always reads this production catalog.
        internal static OriginalIconEvidence[] Current() { return Array.Empty<OriginalIconEvidence>(); }
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
