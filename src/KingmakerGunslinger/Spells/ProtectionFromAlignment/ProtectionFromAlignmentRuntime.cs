using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
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

        internal static UnitEntityData ResolveIncomingSource(MechanicsContext context)
        {
            UnitEntityData source = context == null ? null : context.MaybeCaster;
            if (source == null) return null;
            // Native BuffCollection.AddBuff calls CloneFor. When its parent's
            // controller no longer resolves, the MechanicsContext constructor
            // substitutes the recipient. That fallback is not a controller.
            // Follow only plain native buff clones; an ability execution context
            // remains authoritative even when its parent belongs to a summoner.
            for (int depth = 0; depth < 32; depth++)
            {
                if (context.GetType() != typeof(MechanicsContext) ||
                    !(context.AssociatedBlueprint is BlueprintBuff) ||
                    !ReferenceEquals(context.MaybeCaster, context.MaybeOwner) ||
                    context.ParentContext == null) return source;
                MechanicsContext parent = context.ParentContext;
                if (parent.MaybeCaster == null) return null;
                if (!ReferenceEquals(parent.MaybeCaster, source)) return source;
                context = parent;
            }
            // Malformed/cyclic provenance is unresolved, preserving fail-open
            // policy (or existing exact trusted metadata) rather than guessing.
            return null;
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
