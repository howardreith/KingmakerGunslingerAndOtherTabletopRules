using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Which Giant Spider bones each insect is entitled to drive.
    ///
    /// <para>Named for Sprint 14, which introduced it, but it governs the whole
    /// insect family: Sprint 15's Giant Ant (Drone) and Giant Stag Beetle ride
    /// the same donor and are listed here too.</para>
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
        internal const string DroneKey = "giant-ant-drone";
        internal const string StagBeetleKey = "giant-stag-beetle";

        /// <summary>
        /// The donor's fourth leg chain. Its feet are forbidden to every
        /// creature here without exception, because a foot is the bone that
        /// plants on the ground and nothing here may plant an eighth time.
        /// </summary>
        internal static readonly string[] FourthChainFeet =
            { "L_Foot3", "R_Foot3" };

        /// <summary>
        /// The fourth chain's upper and lower bones: the reviewed wing drivers
        /// of the two creatures that fly with membranous wings - the Fire
        /// Beetle and the Giant Ant (Drone) - and forbidden to everything else.
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
        internal static readonly string[] WingedBones =
            AntBones.Concat(FourthChainWingDrivers).ToArray();

        /// <summary>Kept for the name Sprint 14 used.</summary>
        internal static readonly string[] FireBeetleBones = WingedBones;

        /// <summary>
        /// The creatures whose reviewed meshes drive the wing bones. The Giant
        /// Stag Beetle is deliberately not among them: its printed flight is a
        /// poor speed equal to its ground speed, its profile takes the ground
        /// mode its trample needs, and its wing cases stay shut, so it has no
        /// membranous wing to drive and may not reach that chain at all.
        /// </summary>
        private static readonly HashSet<string> WingedCreatures =
            new HashSet<string>(new[] { FireBeetleKey, DroneKey },
                StringComparer.Ordinal);

        /// <summary>
        /// The allowlist a creature key is entitled to.
        ///
        /// <para>An unknown key gets the ant list, which is the narrower of the
        /// two. A creature nobody reviewed should not inherit permission to
        /// drive wings.</para>
        /// </summary>
        internal static string[] AllowedBones(string key)
        {
            return Flies(key) ? WingedBones : AntBones;
        }

        /// <summary>
        /// Whether this creature's reviewed mesh drives wings.
        ///
        /// <para>A flier's spare limb chain carries its forewing and hindwing
        /// on the upper and lower bones and has no foot at all, which is a
        /// legal shape for a flier and a defect for anything else. Callers
        /// that police limb layout ask here rather than naming a creature, so
        /// one place decides who flies.</para>
        /// </summary>
        internal static bool Flies(string key)
        {
            return key != null && WingedCreatures.Contains(key);
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
