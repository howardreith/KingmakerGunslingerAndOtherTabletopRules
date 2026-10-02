using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 14's runtime work, as one batched family rather than a near-copy
    /// of the runner per creature.
    ///
    /// <para>The tranche donor census established that Kingmaker has no beetle
    /// and no ant, and that the Giant Spider is the only compact many-legged
    /// arthropod in the game. That makes it the best available donor, which is
    /// not the same as proving one rig can carry both a six-legged ant walking
    /// and a beetle flying. The owner's order is explicit that the hypothesis
    /// has to survive two minimal vertical slices before five models are
    /// authored against it, so this pack's first job is to bring back the one
    /// thing that cannot be obtained offline: the donor's measured bind
    /// frame.</para>
    ///
    /// <para>Only bone names and a measured bind frame leave the game. No donor
    /// vertices, triangles, materials, textures or animation data enter the
    /// evidence or the repository.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The donors Sprint 14 and 15 need, named by the project creature that
        /// already rides each one. Keeping this a short table rather than a
        /// method per creature is what lets Sprint 15 add the stag beetle
        /// without another exercise.
        /// </summary>
        private static readonly string[][] Sprint14DonorRigs =
        {
            // The Giant Spider carries the whole insect family hypothesis, so
            // its frame is the one the ground and flying slices are authored
            // against.
            new[] { "giant-spider", "NaturesAlly", "2", "giant-spider" }
        };

        private static void ExerciseExpandedSummoningSprint14RulesPack(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence,
            string evidenceDirectory)
        {
            ExerciseSprint14DonorRigs(blueprints, caster, created, evidence,
                evidenceDirectory);
        }

        private static void ExerciseSprint14DonorRigs(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence,
            string evidenceDirectory)
        {
            var captured = new List<string>();
            bool valid = Sprint14DonorRigs.Length > 0;
            foreach (string[] row in Sprint14DonorRigs)
            {
                SummonFamily family = row[1] == "Monster" ?
                    SummonFamily.Monster : SummonFamily.NaturesAlly;
                int tier = int.Parse(row[2],
                    System.Globalization.CultureInfo.InvariantCulture);
                UnitEntityData rider = CastExpandedSummoningCombatUnit(
                    blueprints, caster, family, row[0], tier, created,
                    evidence);
                RemoveExpandedSummoningAppearanceBuffs(rider);
                string rig = CaptureDonorRig(rider, row[3], evidenceDirectory,
                    "Sprint 14");
                captured.Add(row[3] + "[" + rig + "]");
                // One renderer and a complete bind frame are what an original
                // mesh can actually be authored against; anything else is a
                // donor this pipeline cannot use.
                valid = valid &&
                    rig.IndexOf(",bones=", StringComparison.Ordinal) > 0 &&
                    rig.IndexOf(",renderers=1,", StringComparison.Ordinal) > 0;
            }
            evidence.Sprint14DonorRigs = valid;
            evidence.Sprint14DonorRigsDetail = string.Join(";",
                captured.ToArray());
        }
    }
}
