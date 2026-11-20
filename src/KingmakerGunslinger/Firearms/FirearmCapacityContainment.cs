using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// RISK-01 / S4 T5: a firearm's loaded state is saved on the item as one
    /// token from FirearmStateTokenCatalog, whose registered lead-ball states
    /// hold at most FirearmStateTokenCatalog.DurableLeadBallCapacity rounds.
    /// A production firearm whose capacity, or rounds per reload action, is
    /// larger cannot have its state saved, so it cannot be supported. The one
    /// retained exception is the legacy Advanced Revolver (capacity six),
    /// which must stay unsupported: not player-fireable (the native equipment
    /// gate) and recognition-only for acquisition. Any other production
    /// firearm with an unrepresentable capacity, or the revolver made
    /// fireable, fails registration instead of being accepted silently.
    /// </summary>
    internal static class FirearmCapacityContainment
    {
        internal const string UnsupportedLegacyKey = "advanced-revolver";
        internal const int UnsupportedLegacyCapacity = 6;

        internal static bool IsRepresentable(FirearmDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            return definition.Capacity <= FirearmStateTokenCatalog.DurableLeadBallCapacity &&
                definition.Reload.RoundsPerAction <= FirearmStateTokenCatalog.DurableLeadBallCapacity;
        }

        internal static bool IsContainedLegacy(ProductionFirearmWeaponSpec spec)
        {
            if (spec == null) throw new ArgumentNullException("spec");
            return string.Equals(spec.Key, UnsupportedLegacyKey, StringComparison.Ordinal) &&
                spec.Definition.Capacity == UnsupportedLegacyCapacity &&
                !spec.IsPlayerFireable &&
                spec.AcquisitionRole == ProductionFirearmAcquisitionRole.LegacyRecognitionOnly;
        }

        /// <summary>Throws unless every unrepresentable spec is the exact contained legacy revolver.</summary>
        internal static void Validate(IEnumerable<ProductionFirearmWeaponSpec> specs)
        {
            if (specs == null) throw new ArgumentNullException("specs");
            foreach (ProductionFirearmWeaponSpec spec in specs)
            {
                if (spec == null) throw new ArgumentException("A production firearm spec is missing.", "specs");
                if (IsRepresentable(spec.Definition) || IsContainedLegacy(spec)) continue;
                throw new InvalidOperationException(string.Format(
                    "Production firearm '{0}' declares capacity {1} (reload {2} rounds), but the saved firearm state holds at most {3}; only the unsupported legacy '{4}' may exceed it, and only while unequippable and recognition-only.",
                    spec.Key, spec.Definition.Capacity, spec.Definition.Reload.RoundsPerAction,
                    FirearmStateTokenCatalog.DurableLeadBallCapacity, UnsupportedLegacyKey));
            }
        }
    }
}
