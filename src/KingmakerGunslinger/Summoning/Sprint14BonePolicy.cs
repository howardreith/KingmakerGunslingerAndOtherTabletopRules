using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Which Giant Spider bones each Sprint 14 insect is entitled to drive.
    ///
    /// <para>This is pure policy on purpose. The asset runtime that enforces it
    /// is bound to Unity and cannot be exercised by the deterministic suite, so
    /// the decision lives here where a test can feed it the bytes the game
    /// would actually read - including deliberately corrupted ones - and the
    /// loader calls the same function rather than a copy of it.</para>
    ///
    /// <para>The split matters because one shared list could only ever exclude
    /// the donor's fourth feet. The beetle's wings legitimately ride that
    /// chain's upper and lower bones, so a shared list enforced the beetle's
    /// contract and not the ant's: an ant mesh weighting an eighth leg from the
    /// knee up would have loaded, and the only thing standing between that and
    /// a player was the offline generator, which does not run on the file the
    /// game reads.</para>
    /// </summary>
    internal static class Sprint14BonePolicy
    {
        internal const string FireBeetleKey = "fire-beetle";
        internal const string WorkerKey = "giant-ant-worker";
        internal const string SoldierKey = "giant-ant-soldier";

        /// <summary>
        /// The donor's fourth leg chain. Its feet are forbidden to every Sprint
        /// 14 creature without exception, because a foot is the bone that
        /// plants on the ground and nothing here may plant an eighth time.
        /// </summary>
        internal static readonly string[] FourthChainFeet =
            { "L_Foot3", "R_Foot3" };

        /// <summary>
        /// The fourth chain's upper and lower bones: the beetle's reviewed wing
        /// drivers, and forbidden to the ants.
        /// </summary>
        internal static readonly string[] FourthChainWingDrivers =
            { "L_Leg3_Upper", "L_Leg3_Lower", "R_Leg3_Upper", "R_Leg3_Lower" };

        /// <summary>
        /// A six-legged ant: the body, the mouthparts, the antennae joints and
        /// the first three leg chains a side. The whole fourth chain is absent,
        /// not merely its feet.
        /// </summary>
        internal static readonly string[] AntBones =
        {
            "LowerTorso", "Tail1_M", "UpperTorso", "Tail3_M",
            "chelicera_L", "chelicera_R",
            "pedipalp1_L", "pedipalp2_L", "pedipalp3_L", "pedipalp5_L",
            "pedipalp7_L",
            "pedipalp1_R", "pedipalp2_R", "pedipalp3_R", "pedipalp5_R",
            "pedipalp7_R",
            "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
            "L_Leg1_Upper", "L_Leg1_Lower", "L_Foot1",
            "L_Leg2_Upper", "L_Leg2_Lower", "L_Foot2",
            "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0",
            "R_Leg1_Upper", "R_Leg1_Lower", "R_Foot1",
            "R_Leg2_Upper", "R_Leg2_Lower", "R_Foot2"
        };

        /// <summary>The ant's list plus the two wing drivers, and nothing else.</summary>
        internal static readonly string[] FireBeetleBones =
            AntBones.Concat(FourthChainWingDrivers).ToArray();

        /// <summary>
        /// The allowlist a creature key is entitled to.
        ///
        /// <para>An unknown key gets the ant list, which is the narrower of the
        /// two. A creature nobody reviewed should not inherit permission to
        /// drive wings.</para>
        /// </summary>
        internal static string[] AllowedBones(string key)
        {
            return key == FireBeetleKey ? FireBeetleBones : AntBones;
        }

        /// <summary>
        /// The first bone this creature may not drive, or null if every bone is
        /// permitted. Returning the offending name rather than a flag is what
        /// lets the loader's warning say which bone failed.
        /// </summary>
        internal static string FirstForbiddenBone(string key,
            IEnumerable<string> bones)
        {
            if (bones == null) throw new ArgumentNullException("bones");
            var allowed = new HashSet<string>(AllowedBones(key),
                StringComparer.Ordinal);
            foreach (string bone in bones)
                if (bone == null || !allowed.Contains(bone)) return bone ?? "<null>";
            return null;
        }

        internal static bool IsPermitted(string key, IEnumerable<string> bones)
        {
            return FirstForbiddenBone(key, bones) == null;
        }
    }
}
