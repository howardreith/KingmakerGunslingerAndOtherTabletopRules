using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The ordinary summon hook owns the palette. This observer never
        // applies it or acquires a release lease for a production owner.
        partial void Cleanup2ApplyPrototypePalette(UnitEntityData actor,
            ref IDisposable lease, ref JObject evidence)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                _request.Parameters == null || _request.Parameters.Count != 1 ||
                (string)_request.Parameters["reviewPhase"] != "scale-prototype" ||
                actor == null || actor.View == null)
                throw new InvalidOperationException("Exact owned scale-prototype palette context required.");
            var profile = GiantSummonPresentationPolicy.All.SingleOrDefault(value =>
                value.UnitGuid == actor.Blueprint.AssetGuid);
            if (profile == null) throw new InvalidOperationException("No exact giant palette actor.");
            var proof = GiantSummonPalettePolicy.ProductionProof(profile.Key);
            string outcome = proof == null ? "retain-current-palette" :
                GiantSummonPaletteRuntime.VerifyProductionOwnership(actor.View);
            bool passed = proof == null ||
                outcome.StartsWith("palette:verified-production-owner;", StringComparison.Ordinal);
            evidence = new JObject { ["passed"] = passed, ["key"] = profile.Key,
                ["unit"] = actor.UniqueId, ["outcome"] = outcome,
                ["productionOwner"] = proof != null && passed,
                ["identityEvidence"] = proof?.IdentityEvidence,
                ["wholeBodyPalette"] = proof?.BodyPaletteEvidence,
                ["observerAppliedPalette"] = false, ["observerOwnsRelease"] = false,
                ["nativePixelsExported"] = false, ["ownerVisualApproval"] = false };
        }
    }
}