using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    internal static class AidAnotherGrantRuntime
    {
        private static readonly object Gate = new object();
        private static readonly HashSet<string> DualHelpfulWarnings =
            new HashSet<string>(StringComparer.Ordinal);
        private static CotwAidAnotherContract _cotw;
        private static BlueprintFeature _halflingHelpful;
        private static bool _canonicalFailureReported;

        internal static CotwAidAnotherContract CanonicalContract
        { get { lock (Gate) return _cotw; } }

        internal static void Configure(CotwAidAnotherContract cotw,
            BlueprintFeature halflingHelpful)
        {
            lock (Gate)
            {
                if (!ReferenceEquals(_cotw, cotw))
                    _canonicalFailureReported = false;
                _cotw = cotw;
                _halflingHelpful = halflingHelpful;
            }
        }

        internal static AidAnotherGrantResolution ResolveForBodyguard(
            UnitEntityData helper)
        {
            UnitDescriptor descriptor = helper == null ? null : helper.Descriptor;
            AidAnotherGrantResolution canonical;
            if (TryResolveCanonical(descriptor, out canonical)) return canonical;
            BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
            bool combatHelpful = descriptor != null && set != null &&
                descriptor.HasFact(set.HelpfulCombat);
            AidAnotherGrantResolution fallback = AidAnotherGrantResolver.Resolve(
                new AidAnotherGrantRequest
                {
                    BaseGrant = AidAnotherGrantResolver.NormalBaseGrant,
                    CombatHelpfulOwned = combatHelpful,
                    HalflingHelpfulOwned = false,
                    NonHelpfulIncrement = 0,
                    SourceMode = AidAnotherGrantSourceMode.KmgKnownFallback
                });
            return fallback.Valid ? fallback :
                AidAnotherGrantResolver.Standalone(combatHelpful);
        }

        internal static bool TryOverrideCanonical(ContextRankConfig configuration,
            MechanicsContext context, ref int result)
        {
            CotwAidAnotherContract contract;
            lock (Gate) contract = _cotw;
            if (contract == null || !ReferenceEquals(configuration,
                    contract.Configuration)) return false;
            UnitEntityData caster = context == null ? null : context.MaybeCaster;
            AidAnotherGrantResolution resolution;
            if (caster == null || !TryResolveCanonical(caster.Descriptor,
                    out resolution) || !resolution.Valid) return false;
            result = resolution.FinalGrant;
            return true;
        }

        private static bool TryResolveCanonical(UnitDescriptor descriptor,
            out AidAnotherGrantResolution resolution)
        {
            resolution = null;
            CotwAidAnotherContract contract;
            BlueprintFeature halfling;
            lock (Gate)
            {
                contract = _cotw;
                halfling = _halflingHelpful;
            }
            BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
            if (descriptor == null || contract == null || set == null)
                return false;
            try
            {
                BlueprintFeature[] entries = contract.ReadFeatureList();
                if (entries == null || entries.Any(value => value == null))
                    return false;
                bool combatOwned = descriptor.HasFact(set.HelpfulCombat);
                bool halflingOwned = halfling != null &&
                    descriptor.HasFact(halfling);
                int nonHelpful = 0;
                foreach (BlueprintFeature entry in entries)
                {
                    if (ReferenceEquals(entry, set.HelpfulCombat) ||
                        string.Equals(entry.AssetGuid,
                            set.HelpfulCombat.AssetGuid, StringComparison.Ordinal) ||
                        halfling != null && (ReferenceEquals(entry, halfling) ||
                            string.Equals(entry.AssetGuid, halfling.AssetGuid,
                                StringComparison.Ordinal))) continue;
                    if (descriptor.HasFact(entry)) nonHelpful = checked(
                        nonHelpful + 1);
                }
                resolution = AidAnotherGrantResolver.Resolve(
                    new AidAnotherGrantRequest
                    {
                        BaseGrant = AidAnotherGrantResolver.NormalBaseGrant,
                        CombatHelpfulOwned = combatOwned,
                        HalflingHelpfulOwned = halflingOwned,
                        NonHelpfulIncrement = nonHelpful,
                        SourceMode = AidAnotherGrantSourceMode.CotwCanonical
                    });
                if (resolution.Valid && combatOwned && halflingOwned)
                    WarnDualOwnership(descriptor, resolution);
                return resolution.Valid;
            }
            catch (Exception exception)
            {
                lock (Gate)
                {
                    if (_canonicalFailureReported) return false;
                    _canonicalFailureReported = true;
                }
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("aid-another",
                        "grant-resolution.failed",
                        "The exact canonical Aid Another grant could not be resolved; KMG left CotW ordinary ranks unchanged and Bodyguard will use its safe KMG-known fallback.",
                        exception);
                return false;
            }
        }

        private static void WarnDualOwnership(UnitDescriptor descriptor,
            AidAnotherGrantResolution resolution)
        {
            string identity = descriptor.Unit == null ? "<descriptor>" :
                descriptor.Unit.UniqueId;
            lock (Gate)
                if (!DualHelpfulWarnings.Add(identity)) return;
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Warning("aid-another", "helpful.dual-owner",
                    "unit=" + identity + ";both Helpful variants are owned; " +
                    "the replacement collapsed to the better +4 grant while " +
                    "unrelated canonical increments remained additive;" +
                    resolution.Describe());
        }
    }
}
