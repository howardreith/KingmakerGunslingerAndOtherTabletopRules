using System;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal sealed class MentalControlCatalogEntry :
        IEquatable<MentalControlCatalogEntry>
    {
        private const ProtectionAlignment AllAlignments =
            ProtectionAlignment.Evil | ProtectionAlignment.Good |
            ProtectionAlignment.Law | ProtectionAlignment.Chaos;

        internal MentalControlCatalogEntry(string blueprintName, string guid,
            MentalControlBlueprintKind kind, MentalControlContentSource contentSource,
            string reason, bool required,
            ProtectionAlignment? trustedSourceAlignment = null)
        {
            if (string.IsNullOrWhiteSpace(blueprintName))
                throw new ArgumentException("A blueprint name is required.",
                    "blueprintName");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("An inclusion reason is required.", "reason");
            BlueprintName = blueprintName;
            Guid = BlueprintId.Parse(guid, "guid").Value;
            Kind = kind;
            ContentSource = contentSource;
            Reason = reason;
            Required = required;
            if (trustedSourceAlignment.HasValue &&
                (trustedSourceAlignment.Value == ProtectionAlignment.None ||
                 (trustedSourceAlignment.Value & ~AllAlignments) != 0))
                throw new ArgumentOutOfRangeException("trustedSourceAlignment");
            TrustedSourceAlignment = trustedSourceAlignment;
        }

        internal string BlueprintName { get; private set; }
        internal string Guid { get; private set; }
        internal MentalControlBlueprintKind Kind { get; private set; }
        internal MentalControlContentSource ContentSource { get; private set; }
        internal string Reason { get; private set; }
        internal bool Required { get; private set; }
        internal ProtectionAlignment? TrustedSourceAlignment { get; private set; }

        public bool Equals(MentalControlCatalogEntry other)
        {
            return other != null && BlueprintName == other.BlueprintName &&
                Guid == other.Guid && Kind == other.Kind &&
                ContentSource == other.ContentSource && Reason == other.Reason &&
                Required == other.Required &&
                TrustedSourceAlignment == other.TrustedSourceAlignment;
        }

        public override bool Equals(object obj)
        { return Equals(obj as MentalControlCatalogEntry); }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Guid) ^ (int)Kind ^
                (int)ContentSource ^ (Required ? 397 : 0) ^
                (TrustedSourceAlignment.HasValue ?
                    (int)TrustedSourceAlignment.Value << 8 : 0);
        }
    }
}
