using System.Collections.Generic;
using Kingmaker.Enums;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal static class ProtectionFromAlignmentRuntime
    {
        private static readonly object Gate = new object();
        private static readonly HashSet<string> ReportedUnresolvedSources =
            new HashSet<string>(System.StringComparer.Ordinal);
        private static MentalControlCatalog _catalog =
            MentalControlCatalogDefaults.Create();
        private static ModLogger _logger;

        internal static void Configure(MentalControlCatalog catalog,
            ModLogger logger)
        {
            if (catalog == null) throw new System.ArgumentNullException("catalog");
            lock (Gate)
            {
                _catalog = catalog;
                _logger = logger;
            }
        }

        internal static ProtectionControlImmunityDecision Evaluate(
            ProtectionControlImmunityRequest request)
        {
            MentalControlCatalog catalog;
            lock (Gate) { catalog = _catalog; }
            return ProtectionControlImmunityPolicy.Evaluate(catalog, request);
        }

        internal static ProtectionAlignment FromNativeAlignment(Alignment alignment)
        {
            ProtectionAlignment result = ProtectionAlignment.None;
            if (alignment.HasComponent(AlignmentComponent.Evil))
                result |= ProtectionAlignment.Evil;
            if (alignment.HasComponent(AlignmentComponent.Good))
                result |= ProtectionAlignment.Good;
            if (alignment.HasComponent(AlignmentComponent.Lawful))
                result |= ProtectionAlignment.Law;
            if (alignment.HasComponent(AlignmentComponent.Chaotic))
                result |= ProtectionAlignment.Chaos;
            return result;
        }

        internal static bool TryMapProtectedAlignment(AlignmentComponent alignment,
            out ProtectionAlignment result)
        {
            if (alignment == AlignmentComponent.Evil)
                result = ProtectionAlignment.Evil;
            else if (alignment == AlignmentComponent.Good)
                result = ProtectionAlignment.Good;
            else if (alignment == AlignmentComponent.Lawful)
                result = ProtectionAlignment.Law;
            else if (alignment == AlignmentComponent.Chaotic)
                result = ProtectionAlignment.Chaos;
            else
            {
                result = ProtectionAlignment.None;
                return false;
            }
            return true;
        }

        internal static void ReportUnresolvedSourceOnce(string abilityGuid,
            string buffGuid, ProtectionControlImmunityDecision decision)
        {
            string key = (abilityGuid ?? "<none>") + "|" + (buffGuid ?? "<none>");
            ModLogger logger;
            lock (Gate)
            {
                if (!ReportedUnresolvedSources.Add(key)) return;
                logger = _logger;
            }
            if (logger != null)
                logger.Debug("protection-from-alignment",
                    "runtime.source-unresolved",
                    "Registered control application used unresolved-source policy; outcome=" +
                    (decision.ShouldBlock ? "blocked" : "allowed") +
                    ";reason=" + decision.Reason + ";ability=" +
                    (abilityGuid ?? "<none>") + ";buff=" + (buffGuid ?? "<none>") + ".");
        }
    }
}
