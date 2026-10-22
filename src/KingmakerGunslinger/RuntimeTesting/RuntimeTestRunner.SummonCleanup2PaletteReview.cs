using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // These proofs are admitted only by this exact, save-free experiment.
        // No production registration or summon/view hook is installed.
        private static readonly GiantPaletteAlbedoProof[] Cleanup2PaletteProofs = {
            new GiantPaletteAlbedoProof("cloud-giant", "635d6dee1c7831b41aa987bdbeff14bc",
                "Hill01_body_RIG[2]/hill_giant_1[1]", 0, "hill_giant_01_d", 1024, 1024, "DXT1",
                "20261022T0021352089530Z-fca727ae39c6443b99a759a9b19d8656:cleanup2-pre-survey-actor-cloud-giant",
                "cleanup2-whole-body-cloud-proposal-v1:all body albedo regions including hair/skirt; separate outfit protected"),
            new GiantPaletteAlbedoProof("storm-giant", "df0f3fbceb3605d489c0dec8fa5c7d1e",
                "Frozen_body_RIG[3]/Frozen_body[0]", 0, "frost_giant_d", 1024, 1024, "DXT1",
                "20261022T0021352089530Z-fca727ae39c6443b99a759a9b19d8656:cleanup2-pre-survey-actor-storm-giant",
                "cleanup2-whole-body-storm-proposal-v1:all body albedo regions including hair/skirt; separate cape protected")
        };

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
            var proof = Cleanup2PaletteProofs.SingleOrDefault(value => value.Key == profile.Key);
            if (proof == null)
            {
                evidence = new JObject { ["passed"] = true, ["key"] = profile.Key,
                    ["outcome"] = "retain-current-palette", ["ownerVisualApproval"] = false };
                return;
            }
            string before = GiantSummonPaletteRuntime.Describe(actor.View);
            if (!before.StartsWith("palette:not-applied;", StringComparison.Ordinal))
                throw new InvalidOperationException("Prototype refuses an existing palette owner: " + before);
            GiantSummonPaletteRuntime.RegisterProof(proof);
            string gamma = GiantSummonPaletteRuntime.ProveGammaCopy();
            string outcome = GiantSummonPaletteRuntime.Apply(actor.View);
            bool applied = outcome.StartsWith("palette:applied;", StringComparison.Ordinal);
            if (applied) lease = new Cleanup2PaletteLease(actor.View);
            evidence = new JObject { ["passed"] = applied, ["key"] = profile.Key,
                ["unit"] = actor.UniqueId, ["gammaReadbackSelfTest"] = gamma,
                ["identityEvidence"] = proof.IdentityEvidence,
                ["wholeBodyProposal"] = proof.BodyPaletteEvidence, ["outcome"] = outcome,
                ["nativePixelsExported"] = false, ["ownerVisualApproval"] = false };
        }

        private sealed class Cleanup2PaletteLease : IDisposable
        {
            private UnitEntityView _view;
            internal Cleanup2PaletteLease(UnitEntityView view) { _view = view; }
            public void Dispose()
            {
                if (ReferenceEquals(_view, null)) return;
                string result = GiantSummonPaletteRuntime.Release(_view, "cleanup2-prototype-release", true);
                if (!result.Contains("restored=True") || !result.Contains("retainedBoundObjects=0"))
                    throw new InvalidOperationException("Prototype palette exact restoration refused: " + result);
                _view = null;
            }
        }
    }
}
