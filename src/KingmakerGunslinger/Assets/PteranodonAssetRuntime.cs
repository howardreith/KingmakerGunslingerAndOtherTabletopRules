using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    /// <summary>
    /// Loads and validates the Pteranodon replacement visual once per process.
    ///
    /// The asset is mesh data - vertices, normals, texture coordinates,
    /// triangles, vertex weights, and the ordered list of bone NAMES those
    /// weights index - plus one painted albedo, not an AssetBundle. Three
    /// reasons, in order of weight:
    ///
    /// - It carries strictly less. A bundle would embed bind poses, a material
    ///   and import settings; this carries our own geometry and painting plus
    ///   the donor's bone names, which are the binding contract and are already
    ///   recorded in the native audit. No donor transform ships.
    /// - It depends on no Unity editor licence and no specific editor version.
    ///   That is not hypothetical: the 2018.4.10f1 install that built every
    ///   previous bundle in this repository stopped accepting its licence
    ///   between 2026-08-21 and 2026-09-23 and now demands account credentials
    ///   to re-activate.
    /// - It is less code on both sides than the bundle path it replaces.
    ///
    /// Bind poses are supplied at attach time from the live donor, which is the
    /// only correct binding as well as the redistribution-safe one. Unity skins
    /// a vertex as
    ///
    ///     v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v
    ///
    /// so bind poses derived from the donor's pose at attach time would make the
    /// mesh render as authored in whatever animation frame the unit happened to
    /// be on. The donor's live pose differs from its bind pose by up to 3.954
    /// units at the wingtip - folded wings against spread ones - which would
    /// misplace the creature differently on every summon. Reusing the donor's
    /// own sharedMesh.bindposes, the frame these vertices were authored in,
    /// makes the binding deterministic.
    ///
    /// The mesh and the albedo are one asset: the mesh data names the texture
    /// file, its exact SHA-256 and its header dimensions, and the visual is
    /// published only when both check out. A mesh without its painting is not
    /// the reviewed creature, so it is not shown.
    ///
    /// Every failure path here leaves the donor visual intact, which is the
    /// approved fallback: a Pteranodon that looks like a giant eagle is a
    /// cosmetic shortfall, an invisible or half-bound one is a defect.
    /// </summary>
    internal static class PteranodonAssetRuntime
    {
        internal const string MeshDataRelativePath =
            "assets/pteranodon/pteranodon-mesh.json";
        internal const string DireBatMeshDataRelativePath =
            "assets/flying-animals/dire-bat-mesh.json";
        internal const string EagleMeshDataRelativePath =
            "assets/flying-animals/eagle-mesh.json";
        internal const string GiantWaspMeshDataRelativePath =
            "assets/flying-animals/giant-wasp-mesh.json";
        internal const string StirgeMeshDataRelativePath =
            "assets/flying-animals/stirge-mesh.json";
        private const string UngulateDirectory = "assets/ungulates/";
        private const string Sprint12QuadrupedDirectory =
            "assets/sprint12-quadrupeds/";
        private const string Sprint13CreatureDirectory =
            "assets/sprint13-creatures/";
        private const string Sprint14InsectDirectory =
            "assets/sprint14-insects/";
        internal const int SupportedSchemaVersion = 2;

        /// <summary>
        /// The bones the mesh may bind to: six shared, and twenty per side - the
        /// wing chain, the leg, and eight toe bones.
        ///
        /// This is not redundant with resolving them against the donor. That
        /// proves a name exists; this proves the generator has not started
        /// weighting geometry to a bone nobody reviewed. Absent on purpose:
        /// Tail_end, Tail_L, Tail_R and every *_end leaf. The donor's eagle tail
        /// fan carries no geometry, so a weight landing there would mean a fan
        /// had crept back in.
        /// </summary>
        private static readonly string[] AllowedBones =
        {
            "LowerTorso", "UpperTorso", "Neck", "Head", "Jaw", "Tail",
            "L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1", "L_Feather_2",
            "L_Feather_3", "L_Feather_4", "L_Feather_5", "L_Feather_6",
            "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0", "L_Finger_1_1",
            "L_Finger_1_2", "L_Finger_2_1", "L_Finger_2_2", "L_Finger_3_1",
            "L_Finger_3_2", "L_Finger_4_1", "L_Finger_4_2", "R_Arm_Upper",
            "R_Arm_Lower", "R_Palm", "R_Feather_1", "R_Feather_2", "R_Feather_3",
            "R_Feather_4", "R_Feather_5", "R_Feather_6", "R_Leg0_Upper",
            "R_Leg0_Lower", "R_Foot0", "R_Finger_1_1", "R_Finger_1_2",
            "R_Finger_2_1", "R_Finger_2_2", "R_Finger_3_1", "R_Finger_3_2",
            "R_Finger_4_1", "R_Finger_4_2"
        };

        // The two captured native quadruped rigs have different naming and
        // orientation. Keep each authoring family to the bones actually used
        // by its original mesh; a stray donor bone must fail before attach.
        private static readonly string[] AllowedHorseBones =
        {
            "Chest", "Head", "Jaw", "LowerTorso", "UpperTorso", "Neck1", "Neck2",
            "L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Fingers",
            "R_Arm_Upper", "R_Arm_Lower", "R_Palm", "R_Fingers",
            "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0", "L_Toes0",
            "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0", "R_Toes0",
            "Tail1", "Tail2", "Tail3", "Tail4", "Tail5"
        };
        private static readonly string[] AllowedMastodonBones =
        {
            "Head", "LowerTorso", "Spine", "UpperTorso", "Neck",
            "L_Arm_Upper", "L_Arm_Lower", "L_Palm",
            "R_Arm_Upper", "R_Arm_Lower", "R_Palm",
            "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
            "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0",
            "frontKnee_L", "frontKnee_R", "frontAnkle_L", "frontAnkle_R",
            "backKnee_L", "backKnee_R", "backAnkle_L", "backAnkle_R",
            "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M"
        };

        private static readonly string[] AllowedDogBones =
        {
            "Head", "Jaw", "L_Arm_Lower", "L_Arm_Upper", "L_Ear",
            "L_Finger0", "L_Leg0_Foot", "L_Leg0_Lower", "L_Leg0_Lower1",
            "L_Leg0_Toe0", "L_Leg0_Upper", "L_Palm", "LowerTorso", "Neck0",
            "Neck1", "Nose", "Pelvis", "R_Arm_Lower", "R_Arm_Upper", "R_Ear",
            "R_Finger0", "R_Leg0_Foot", "R_Leg0_Lower", "R_Leg0_Lower1",
            "R_Leg0_Toe0", "R_Leg0_Upper", "R_Palm", "Tail00", "Tail01",
            "Tail02", "Tail03", "UpperTorso"
        };
        private static readonly string[] AllowedWolfWorgBones =
        {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0", "L_Leg0_Lower",
            "L_Leg0_Lower2", "L_Leg0_Upper", "L_Palm", "R_Arm_Lower",
            "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Lower2",
            "R_Leg0_Upper", "R_Palm", "Torso_Lower", "Torso_Upper", "ear_L",
            "ear_R", "front_paw__tip_R", "front_paw_tip_L", "hindpaw_tip_L",
            "hindpaw_tip_R", "jaw", "jaw_woo_down", "jaw_woo_up",
            "jaw_woo_up_add", "neck", "spine_0", "tail_01", "tail_02",
            "tail_03", "tail_04", "withers"
        };

        // The Giant Poisonous Frog rig. Its names come from a different
        // authoring house than the Worg's, which is why the two lists share so
        // little: this one mixes the engine's own torso and limb names with
        // joint names carried straight over from the original rig. Absent on
        // purpose: every finger joint and the whole tongue chain. The original
        // frog has no fingers or tongue of its own, so a weight landing on one
        // would mean geometry nobody reviewed had appeared.
        /// <summary>
        /// Which Giant Spider bones each Sprint 14 insect may drive.
        ///
        /// <para>The lists themselves live in <see cref="Sprint14BonePolicy"/>
        /// because this class is bound to Unity and the deterministic suite
        /// cannot construct it. The policy is pure, so a corruption test can
        /// feed it the bytes the game would actually read, and this calls the
        /// same function rather than a copy of it.</para>
        /// </summary>
        private static string[] AllowedSprint14Bones(string key)
        {
            return Sprint14BonePolicy.AllowedBones(key);
        }

        private static readonly string[] AllowedGiantFrogBones =
        {
            "Head", "LowerTorso", "chest_joint", "spine_3_joint", "stomach",
            "jaw_endJoint", "jaw_skinJoint1",
            "l_t_eyeLid_joint", "r_t_eyeLid_joint",
            "L_Arm_Upper", "L_Arm_Lower", "L_Palm",
            "R_Arm_Upper", "R_Arm_Lower", "R_Palm",
            "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
            "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0",
            "l_toe_joint", "l_toeTip_joint", "r_toe_joint", "r_toeTip_joint"
        };

        private sealed class UngulateVisual
        {
            internal Mesh Mesh;
            internal string[] Bones;
            internal Texture2D Albedo;
            internal string Status = "donor-visual:not-configured";
        }

        private static readonly Dictionary<string, UngulateVisual> Ungulates =
            new Dictionary<string, UngulateVisual>(StringComparer.Ordinal)
            {
                { "aurochs", new UngulateVisual() },
                { "bison", new UngulateVisual() },
                { "rhinoceros", new UngulateVisual() },
                { "woolly-rhinoceros", new UngulateVisual() }
            };

        private sealed class Sprint12QuadrupedVisual
        {
            internal Mesh Mesh;
            internal string[] Bones;
            internal Texture2D Albedo;
            internal string Status = "donor-visual:not-configured";
        }

        private static readonly Dictionary<string, Sprint12QuadrupedVisual>
            Sprint12Quadrupeds =
                new Dictionary<string, Sprint12QuadrupedVisual>(
                    StringComparer.Ordinal)
                {
                    { "dire-rat", new Sprint12QuadrupedVisual() },
                    { "hyena", new Sprint12QuadrupedVisual() },
                    { "goblin-dog", new Sprint12QuadrupedVisual() }
                };

        private sealed class Sprint13CreatureVisual
        {
            internal Mesh Mesh;
            internal string[] Bones;
            internal Texture2D Albedo;
            internal string Status = "donor-visual:not-configured";
        }

        /// <summary>
        /// The Wolverine and the Shadow Mastiff ride the same Worg rig the
        /// Goblin Dog does, so they reuse its reviewed bone list; the Poison
        /// Frog has its own.
        /// </summary>
        private static readonly Dictionary<string, Sprint13CreatureVisual>
            Sprint13Creatures =
                new Dictionary<string, Sprint13CreatureVisual>(
                    StringComparer.Ordinal)
                {
                    { "wolverine", new Sprint13CreatureVisual() },
                    { "shadow-mastiff", new Sprint13CreatureVisual() },
                    { "poisonous-frog", new Sprint13CreatureVisual() }
                };

        /// <summary>
        /// The three Sprint 14 insects, all on the Giant Spider rig. The record
        /// shape is the same one Sprint 13 declared and is not re-declared.
        /// </summary>
        private static readonly Dictionary<string, Sprint13CreatureVisual>
            Sprint14Insects =
                new Dictionary<string, Sprint13CreatureVisual>(
                    StringComparer.Ordinal)
                {
                    { "fire-beetle", new Sprint13CreatureVisual() },
                    { "giant-ant-worker", new Sprint13CreatureVisual() },
                    { "giant-ant-soldier", new Sprint13CreatureVisual() }
                };

        /// <summary>
        /// What the mesh data says about its painting. Checked against the
        /// bytes on disk before the texture is decoded, so a texture swapped
        /// after the build is refused rather than shown.
        /// </summary>
        internal sealed class AlbedoRequirement
        {
            internal string File;
            internal string Sha256;
            internal int Width;
            internal int Height;
        }

        private static readonly object Sync = new object();
        private static Mesh _mesh;
        private static string[] _boneNames;
        private static Texture2D _albedo;
        private static string _status = "donor-visual:not-configured";
        private static Mesh _direBatMesh;
        private static string[] _direBatBoneNames;
        private static Texture2D _direBatAlbedo;
        private static string _direBatStatus = "donor-visual:not-configured";
        private static Mesh _eagleMesh;
        private static string[] _eagleBoneNames;
        private static Texture2D _eagleAlbedo;
        private static string _eagleStatus = "donor-visual:not-configured";
        private static Mesh _giantWaspMesh;
        private static string[] _giantWaspBoneNames;
        private static Texture2D _giantWaspAlbedo;
        private static string _giantWaspStatus = "donor-visual:not-configured";
        private static Mesh _stirgeMesh;
        private static string[] _stirgeBoneNames;
        private static Texture2D _stirgeAlbedo;
        private static string _stirgeStatus = "donor-visual:not-configured";

        internal static string Status { get { lock (Sync) return _status; } }

        internal static bool HasValidatedMesh
        { get { lock (Sync) return _mesh != null && _boneNames != null; } }

        internal static bool HasValidatedVisual
        {
            get
            {
                lock (Sync)
                    return _mesh != null && _boneNames != null && _albedo != null;
            }
        }

        /// <summary>The validated mesh and the bone names its weights index.</summary>
        internal static bool TryGetMembrane(out Mesh mesh, out string[] boneNames)
        {
            lock (Sync)
            {
                mesh = _mesh;
                boneNames = _boneNames == null ? null :
                    (string[])_boneNames.Clone();
                return mesh != null && boneNames != null;
            }
        }

        /// <summary>The validated albedo the mesh's texture coordinates index.</summary>
        internal static bool TryGetAlbedo(out Texture2D albedo)
        {
            lock (Sync)
            {
                albedo = _albedo;
                return albedo != null;
            }
        }

        internal static string DireBatStatus
        { get { lock (Sync) return _direBatStatus; } }

        /// <summary>The Bat uses the same audited rig, parser and binding path.</summary>
        internal static bool TryGetDireBatVisual(out Mesh mesh,
            out string[] boneNames, out Texture2D albedo)
        {
            lock (Sync)
            {
                mesh = _direBatMesh;
                boneNames = _direBatBoneNames == null ? null :
                    (string[])_direBatBoneNames.Clone();
                albedo = _direBatAlbedo;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        internal static string EagleStatus
        { get { lock (Sync) return _eagleStatus; } }

        internal static bool TryGetEagleVisual(out Mesh mesh,
            out string[] boneNames, out Texture2D albedo)
        {
            lock (Sync)
            {
                mesh = _eagleMesh;
                boneNames = _eagleBoneNames == null ? null :
                    (string[])_eagleBoneNames.Clone();
                albedo = _eagleAlbedo;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        internal static string GiantWaspStatus
        { get { lock (Sync) return _giantWaspStatus; } }

        internal static bool TryGetGiantWaspVisual(out Mesh mesh,
            out string[] boneNames, out Texture2D albedo)
        {
            lock (Sync)
            {
                mesh = _giantWaspMesh;
                boneNames = _giantWaspBoneNames == null ? null :
                    (string[])_giantWaspBoneNames.Clone();
                albedo = _giantWaspAlbedo;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        internal static string StirgeStatus
        { get { lock (Sync) return _stirgeStatus; } }

        internal static bool TryGetStirgeVisual(out Mesh mesh,
            out string[] boneNames, out Texture2D albedo)
        {
            lock (Sync)
            {
                mesh = _stirgeMesh;
                boneNames = _stirgeBoneNames == null ? null :
                    (string[])_stirgeBoneNames.Clone();
                albedo = _stirgeAlbedo;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        internal static bool TryGetUngulateVisual(string key, out Mesh mesh,
            out string[] boneNames, out Texture2D albedo, out string status)
        {
            lock (Sync)
            {
                UngulateVisual visual;
                if (!Ungulates.TryGetValue(key, out visual))
                {
                    mesh = null;
                    boneNames = null;
                    albedo = null;
                    status = "donor-visual:unknown-ungulate";
                    return false;
                }
                mesh = visual.Mesh;
                boneNames = visual.Bones == null ? null :
                    (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        internal static bool TryGetSprint12QuadrupedVisual(string key,
            out Mesh mesh, out string[] boneNames, out Texture2D albedo,
            out string status)
        {
            lock (Sync)
            {
                Sprint12QuadrupedVisual visual;
                if (!Sprint12Quadrupeds.TryGetValue(key, out visual))
                {
                    mesh = null;
                    boneNames = null;
                    albedo = null;
                    status = "donor-visual:unknown-sprint12-quadruped";
                    return false;
                }
                mesh = visual.Mesh;
                boneNames = visual.Bones == null ? null :
                    (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        /// <summary>
        /// Takes the published visual away for the life of the returned scope,
        /// so a guarded scenario can prove the fallback on a live summon without
        /// a broken file on disk. Only the runtime-testing fixture calls it;
        /// disposing the scope puts back exactly what was published.
        /// </summary>
        internal static IDisposable WithdrawForTest(string reason)
        {
            lock (Sync)
            {
                var scope = new Withdrawal(_mesh, _boneNames, _albedo, _status);
                _mesh = null;
                _boneNames = null;
                _albedo = null;
                _status = "donor-visual:withdrawn-for-test:" + reason;
                return scope;
            }
        }

        private sealed class Withdrawal : IDisposable
        {
            private readonly Mesh _mesh;
            private readonly string[] _boneNames;
            private readonly Texture2D _albedo;
            private readonly string _status;
            private bool _restored;

            internal Withdrawal(Mesh mesh, string[] boneNames, Texture2D albedo,
                string status)
            {
                _mesh = mesh;
                _boneNames = boneNames;
                _albedo = albedo;
                _status = status;
            }

            public void Dispose()
            {
                lock (Sync)
                {
                    if (_restored) return;
                    _restored = true;
                    PteranodonAssetRuntime._mesh = _mesh;
                    PteranodonAssetRuntime._boneNames = _boneNames;
                    PteranodonAssetRuntime._albedo = _albedo;
                    PteranodonAssetRuntime._status = _status;
                }
            }
        }

        internal static bool TryGetSprint14InsectVisual(string key,
            out Mesh mesh, out string[] boneNames, out Texture2D albedo,
            out string status)
        {
            lock (Sync)
            {
                Sprint13CreatureVisual visual;
                if (!Sprint14Insects.TryGetValue(key, out visual))
                {
                    mesh = null;
                    boneNames = null;
                    albedo = null;
                    status = "donor-visual:unknown-sprint14-insect";
                    return false;
                }
                mesh = visual.Mesh;
                boneNames = visual.Bones == null ? null :
                    (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        internal static bool TryGetSprint13CreatureVisual(string key,
            out Mesh mesh, out string[] boneNames, out Texture2D albedo,
            out string status)
        {
            lock (Sync)
            {
                Sprint13CreatureVisual visual;
                if (!Sprint13Creatures.TryGetValue(key, out visual))
                {
                    mesh = null;
                    boneNames = null;
                    albedo = null;
                    status = "donor-visual:unknown-sprint13-creature";
                    return false;
                }
                mesh = visual.Mesh;
                boneNames = visual.Bones == null ? null :
                    (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && boneNames != null && albedo != null;
            }
        }

        internal static void Configure(ModContext context)
        {
            ConfigurePteranodon(context);
            ConfigureDireBat(context);
            ConfigureEagle(context);
            ConfigureGiantWasp(context);
            ConfigureStirge(context);
            ConfigureUngulates(context);
            ConfigureSprint12Quadrupeds(context);
            ConfigureSprint13Creatures(context);
            ConfigureSprint14Insects(context);
        }

        private static void ConfigureSprint14Insects(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            foreach (var entry in Sprint14Insects)
            {
                string key = entry.Key;
                Sprint13CreatureVisual visual = entry.Value;
                if (!context.FeatureModules.Active.ExpandedSummoning)
                {
                    lock (Sync) visual.Status = "donor-visual:module-disabled";
                    continue;
                }
                lock (Sync)
                    if (visual.Mesh != null && visual.Bones != null &&
                        visual.Albedo != null) continue;
                string path = Path.Combine(context.ModEntry.Path,
                    (Sprint14InsectDirectory + key + "-mesh.json")
                    .Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    lock (Sync) visual.Status = "donor-visual:mesh-data-missing";
                    context.Logger.Warning(key, "mesh.missing",
                        "The original Sprint 14 visual is unavailable; the donor remains active: " + path);
                    continue;
                }
                Mesh mesh = null;
                Texture2D albedo = null;
                try
                {
                    string[] names;
                    AlbedoRequirement requirement;
                    mesh = BuildMesh(File.ReadAllText(path), out names,
                        out requirement, AllowedSprint14Bones(key));
                    string reason;
                    albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                        out reason);
                    if (albedo == null)
                        throw new InvalidDataException("albedo:" + reason);
                    mesh.name = "KMG_" + key;
                    albedo.name = "KMG_" + key + "_Albedo";
                    lock (Sync)
                    {
                        visual.Mesh = mesh;
                        visual.Bones = names;
                        visual.Albedo = albedo;
                        visual.Status = "visual:published";
                    }
                    context.Logger.Info(key, "mesh.published",
                        "Validated original Sprint 14 mesh: vertices=" +
                        mesh.vertexCount + ";triangles=" +
                        mesh.triangles.Length / 3 + ";bones=" + names.Length +
                        ";albedo=" + albedo.width + "x" + albedo.height);
                }
                catch (Exception error)
                {
                    if (mesh != null) UnityEngine.Object.Destroy(mesh);
                    if (albedo != null) UnityEngine.Object.Destroy(albedo);
                    lock (Sync)
                    {
                        visual.Mesh = null;
                        visual.Bones = null;
                        visual.Albedo = null;
                        visual.Status = "donor-visual:invalid-mesh-data";
                    }
                    context.Logger.Warning(key, "mesh.rejected",
                        "The original Sprint 14 visual was rejected; the donor remains active: " +
                        error.Message);
                }
            }
        }

        private static void ConfigureSprint13Creatures(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            foreach (var entry in Sprint13Creatures)
            {
                string key = entry.Key;
                Sprint13CreatureVisual visual = entry.Value;
                if (!context.FeatureModules.Active.ExpandedSummoning)
                {
                    lock (Sync) visual.Status = "donor-visual:module-disabled";
                    continue;
                }
                lock (Sync)
                    if (visual.Mesh != null && visual.Bones != null &&
                        visual.Albedo != null) continue;
                string path = Path.Combine(context.ModEntry.Path,
                    (Sprint13CreatureDirectory + key + "-mesh.json")
                    .Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    lock (Sync) visual.Status = "donor-visual:mesh-data-missing";
                    context.Logger.Warning(key, "mesh.missing",
                        "The original Sprint 13 visual is unavailable; the donor remains active: " + path);
                    continue;
                }
                Mesh mesh = null;
                Texture2D albedo = null;
                try
                {
                    string[] names;
                    AlbedoRequirement requirement;
                    string[] allowed = key == "poisonous-frog"
                        ? AllowedGiantFrogBones : AllowedWolfWorgBones;
                    mesh = BuildMesh(File.ReadAllText(path), out names,
                        out requirement, allowed);
                    string reason;
                    albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                        out reason);
                    if (albedo == null)
                        throw new InvalidDataException("albedo:" + reason);
                    mesh.name = "KMG_" + key;
                    albedo.name = "KMG_" + key + "_Albedo";
                    lock (Sync)
                    {
                        visual.Mesh = mesh;
                        visual.Bones = names;
                        visual.Albedo = albedo;
                        visual.Status = "visual:published";
                    }
                    context.Logger.Info(key, "mesh.published",
                        "Validated original Sprint 13 mesh: vertices=" +
                        mesh.vertexCount + ";triangles=" +
                        mesh.triangles.Length / 3 + ";bones=" + names.Length +
                        ";albedo=" + albedo.width + "x" + albedo.height);
                }
                catch (Exception error)
                {
                    if (mesh != null) UnityEngine.Object.Destroy(mesh);
                    if (albedo != null) UnityEngine.Object.Destroy(albedo);
                    lock (Sync)
                    {
                        visual.Mesh = null;
                        visual.Bones = null;
                        visual.Albedo = null;
                        visual.Status = "donor-visual:invalid-mesh-data";
                    }
                    context.Logger.Warning(key, "mesh.rejected",
                        "The original Sprint 13 visual was rejected; the donor remains active: " +
                        error.Message);
                }
            }
        }

        private static void ConfigureSprint12Quadrupeds(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            foreach (var entry in Sprint12Quadrupeds)
            {
                string key = entry.Key;
                Sprint12QuadrupedVisual visual = entry.Value;
                if (!context.FeatureModules.Active.ExpandedSummoning)
                {
                    lock (Sync) visual.Status = "donor-visual:module-disabled";
                    continue;
                }
                lock (Sync)
                    if (visual.Mesh != null && visual.Bones != null &&
                        visual.Albedo != null) continue;
                string path = Path.Combine(context.ModEntry.Path,
                    (Sprint12QuadrupedDirectory + key + "-mesh.json")
                    .Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    lock (Sync) visual.Status = "donor-visual:mesh-data-missing";
                    context.Logger.Warning(key, "mesh.missing",
                        "The original Sprint 12 visual is unavailable; the donor remains active: " + path);
                    continue;
                }
                Mesh mesh = null;
                Texture2D albedo = null;
                try
                {
                    string[] names;
                    AlbedoRequirement requirement;
                    string[] allowed = key == "dire-rat" ? AllowedDogBones :
                        AllowedWolfWorgBones;
                    mesh = BuildMesh(File.ReadAllText(path), out names,
                        out requirement, allowed);
                    string reason;
                    albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                        out reason);
                    if (albedo == null)
                        throw new InvalidDataException("albedo:" + reason);
                    mesh.name = "KMG_" + key;
                    albedo.name = "KMG_" + key + "_Albedo";
                    lock (Sync)
                    {
                        visual.Mesh = mesh;
                        visual.Bones = names;
                        visual.Albedo = albedo;
                        visual.Status = "visual:published";
                    }
                    context.Logger.Info(key, "mesh.published",
                        "Validated original Sprint 12 mesh: vertices=" +
                        mesh.vertexCount + ";triangles=" +
                        mesh.triangles.Length / 3 + ";bones=" + names.Length +
                        ";albedo=" + albedo.width + "x" + albedo.height);
                }
                catch (Exception error)
                {
                    if (mesh != null) UnityEngine.Object.Destroy(mesh);
                    if (albedo != null) UnityEngine.Object.Destroy(albedo);
                    lock (Sync)
                    {
                        visual.Mesh = null;
                        visual.Bones = null;
                        visual.Albedo = null;
                        visual.Status = "donor-visual:invalid-mesh-data";
                    }
                    context.Logger.Warning(key, "mesh.rejected",
                        "The original Sprint 12 visual was rejected; the donor remains active: " +
                        error.Message);
                }
            }
        }

        private static void ConfigureUngulates(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            foreach (var entry in Ungulates)
            {
                string key = entry.Key;
                UngulateVisual visual = entry.Value;
                if (!context.FeatureModules.Active.ExpandedSummoning)
                {
                    lock (Sync) visual.Status = "donor-visual:module-disabled";
                    continue;
                }
                lock (Sync)
                    if (visual.Mesh != null && visual.Bones != null &&
                        visual.Albedo != null) continue;
                string path = Path.Combine(context.ModEntry.Path,
                    (UngulateDirectory + key + "-mesh.json")
                    .Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    lock (Sync) visual.Status = "donor-visual:mesh-data-missing";
                    context.Logger.Warning(key, "mesh.missing",
                        "The original ungulate visual is unavailable; the donor remains active: " + path);
                    continue;
                }
                Mesh mesh = null;
                Texture2D albedo = null;
                try
                {
                    string[] names;
                    AlbedoRequirement requirement;
                    string[] allowed = key == "aurochs" || key == "bison"
                        ? AllowedHorseBones : AllowedMastodonBones;
                    mesh = BuildMesh(File.ReadAllText(path), out names,
                        out requirement, allowed);
                    string reason;
                    albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                        out reason);
                    if (albedo == null)
                        throw new InvalidDataException("albedo:" + reason);
                    mesh.name = "KMG_" + key;
                    albedo.name = "KMG_" + key + "_Albedo";
                    lock (Sync)
                    {
                        visual.Mesh = mesh;
                        visual.Bones = names;
                        visual.Albedo = albedo;
                        visual.Status = "visual:published";
                    }
                    context.Logger.Info(key, "mesh.published",
                        "Validated original ungulate mesh: vertices=" + mesh.vertexCount +
                        ";triangles=" + mesh.triangles.Length / 3 +
                        ";bones=" + names.Length + ";albedo=" +
                        albedo.width + "x" + albedo.height);
                }
                catch (Exception error)
                {
                    if (mesh != null) UnityEngine.Object.Destroy(mesh);
                    if (albedo != null) UnityEngine.Object.Destroy(albedo);
                    lock (Sync)
                    {
                        visual.Mesh = null;
                        visual.Bones = null;
                        visual.Albedo = null;
                        visual.Status = "donor-visual:invalid-mesh-data";
                    }
                    context.Logger.Warning(key, "mesh.rejected",
                        "The original ungulate visual was rejected; the donor remains active: " +
                        error.Message);
                }
            }
        }

        private static void ConfigurePteranodon(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) _status = "donor-visual:module-disabled";
                context.Logger.Info("pteranodon", "mesh.skipped",
                    "Expanded Summoning is disabled; the donor visual remains active.");
                return;
            }

            lock (Sync)
            {
                if (_mesh != null && _boneNames != null && _albedo != null)
                {
                    context.Logger.Info("pteranodon", "mesh.reused",
                        "The validated Pteranodon visual is already published.");
                    return;
                }
            }

            string path = Path.Combine(context.ModEntry.Path,
                MeshDataRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                lock (Sync) _status = "donor-visual:mesh-data-missing";
                context.Logger.Warning("pteranodon", "mesh.missing",
                    "The Pteranodon mesh data is unavailable; the donor visual remains active: " + path);
                return;
            }

            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(File.ReadAllText(path), out names, out requirement);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                {
                    UnityEngine.Object.Destroy(mesh);
                    lock (Sync)
                    {
                        _mesh = null;
                        _boneNames = null;
                        _albedo = null;
                        _status = "donor-visual:" + reason;
                    }
                    context.Logger.Warning("pteranodon", "albedo.rejected",
                        "The Pteranodon albedo was rejected; the donor visual remains active: " +
                        reason);
                    return;
                }

                lock (Sync)
                {
                    _mesh = mesh;
                    _boneNames = names;
                    _albedo = albedo;
                    _status = "visual:published";
                }

                context.Logger.Info("pteranodon", "mesh.published",
                    "Validated the Pteranodon visual: vertices=" +
                    mesh.vertexCount + ";triangles=" +
                    (mesh.triangles.Length / 3) + ";bones=" + names.Length +
                    ";albedo=" + albedo.width + "x" + albedo.height);
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    _mesh = null;
                    _boneNames = null;
                    _albedo = null;
                    _status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("pteranodon", "mesh.rejected",
                    "The Pteranodon mesh data was rejected; the donor visual remains active: " +
                    error.Message);
            }
        }

        private static void ConfigureDireBat(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) _direBatStatus = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
            {
                if (_direBatMesh != null && _direBatBoneNames != null &&
                    _direBatAlbedo != null) return;
            }
            string path = Path.Combine(context.ModEntry.Path,
                DireBatMeshDataRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                lock (Sync) _direBatStatus = "donor-visual:mesh-data-missing";
                context.Logger.Warning("dire-bat", "mesh.missing",
                    "The original Bat visual is unavailable; the donor remains active: " + path);
                return;
            }
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(File.ReadAllText(path), out names,
                    out requirement);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_DireBat";
                albedo.name = "KMG_DireBat_Albedo";
                lock (Sync)
                {
                    _direBatMesh = mesh;
                    _direBatBoneNames = names;
                    _direBatAlbedo = albedo;
                    _direBatStatus = "visual:published";
                }
                context.Logger.Info("dire-bat", "mesh.published",
                    "Validated original Bat mesh: vertices=" + mesh.vertexCount +
                    ";triangles=" + mesh.triangles.Length / 3 +
                    ";bones=" + names.Length + ";albedo=" +
                    albedo.width + "x" + albedo.height);
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    _direBatMesh = null;
                    _direBatBoneNames = null;
                    _direBatAlbedo = null;
                    _direBatStatus = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("dire-bat", "mesh.rejected",
                    "The original Bat visual was rejected; the donor remains active: " +
                    error.Message);
            }
        }

        private static void ConfigureEagle(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) _eagleStatus = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
            {
                if (_eagleMesh != null && _eagleBoneNames != null &&
                    _eagleAlbedo != null) return;
            }
            string path = Path.Combine(context.ModEntry.Path,
                EagleMeshDataRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                lock (Sync) _eagleStatus = "donor-visual:mesh-data-missing";
                context.Logger.Warning("eagle", "mesh.missing",
                    "The original Eagle visual is unavailable; the donor remains active: " + path);
                return;
            }
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(File.ReadAllText(path), out names,
                    out requirement);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_Eagle";
                albedo.name = "KMG_Eagle_Albedo";
                lock (Sync)
                {
                    _eagleMesh = mesh;
                    _eagleBoneNames = names;
                    _eagleAlbedo = albedo;
                    _eagleStatus = "visual:published";
                }
                context.Logger.Info("eagle", "mesh.published",
                    "Validated original Eagle mesh: vertices=" + mesh.vertexCount +
                    ";triangles=" + mesh.triangles.Length / 3 +
                    ";bones=" + names.Length + ";albedo=" +
                    albedo.width + "x" + albedo.height);
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    _eagleMesh = null;
                    _eagleBoneNames = null;
                    _eagleAlbedo = null;
                    _eagleStatus = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("eagle", "mesh.rejected",
                    "The original Eagle visual was rejected; the donor remains active: " +
                    error.Message);
            }
        }

        private static void ConfigureGiantWasp(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) _giantWaspStatus = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
            {
                if (_giantWaspMesh != null && _giantWaspBoneNames != null &&
                    _giantWaspAlbedo != null) return;
            }
            string path = Path.Combine(context.ModEntry.Path,
                GiantWaspMeshDataRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                lock (Sync) _giantWaspStatus = "donor-visual:mesh-data-missing";
                context.Logger.Warning("giant-wasp", "mesh.missing",
                    "The original Wasp visual is unavailable; the donor remains active: " + path);
                return;
            }
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(File.ReadAllText(path), out names,
                    out requirement);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_GiantWasp";
                albedo.name = "KMG_GiantWasp_Albedo";
                lock (Sync)
                {
                    _giantWaspMesh = mesh;
                    _giantWaspBoneNames = names;
                    _giantWaspAlbedo = albedo;
                    _giantWaspStatus = "visual:published";
                }
                context.Logger.Info("giant-wasp", "mesh.published",
                    "Validated original Wasp mesh: vertices=" + mesh.vertexCount +
                    ";triangles=" + mesh.triangles.Length / 3 +
                    ";bones=" + names.Length + ";albedo=" +
                    albedo.width + "x" + albedo.height);
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    _giantWaspMesh = null;
                    _giantWaspBoneNames = null;
                    _giantWaspAlbedo = null;
                    _giantWaspStatus = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("giant-wasp", "mesh.rejected",
                    "The original Wasp visual was rejected; the donor remains active: " +
                    error.Message);
            }
        }

        private static void ConfigureStirge(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) _stirgeStatus = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
            {
                if (_stirgeMesh != null && _stirgeBoneNames != null &&
                    _stirgeAlbedo != null) return;
            }
            string path = Path.Combine(context.ModEntry.Path,
                StirgeMeshDataRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                lock (Sync) _stirgeStatus = "donor-visual:mesh-data-missing";
                context.Logger.Warning("stirge", "mesh.missing",
                    "The original Stirge visual is unavailable; the donor remains active: " + path);
                return;
            }
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(File.ReadAllText(path), out names,
                    out requirement);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_Stirge";
                albedo.name = "KMG_Stirge_Albedo";
                lock (Sync)
                {
                    _stirgeMesh = mesh;
                    _stirgeBoneNames = names;
                    _stirgeAlbedo = albedo;
                    _stirgeStatus = "visual:published";
                }
                context.Logger.Info("stirge", "mesh.published",
                    "Validated original Stirge mesh: vertices=" + mesh.vertexCount +
                    ";triangles=" + mesh.triangles.Length / 3 +
                    ";bones=" + names.Length + ";albedo=" +
                    albedo.width + "x" + albedo.height);
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    _stirgeMesh = null;
                    _stirgeBoneNames = null;
                    _stirgeAlbedo = null;
                    _stirgeStatus = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("stirge", "mesh.rejected",
                    "The original Stirge visual was rejected; the donor remains active: " +
                    error.Message);
            }
        }

        internal static Mesh BuildMesh(string json, out string[] boneNames)
        {
            AlbedoRequirement ignored;
            return BuildMesh(json, out boneNames, out ignored);
        }

        /// <summary>
        /// Builds the mesh, validating everything before a donor renderer could
        /// ever be touched. Bind poses are left at identity: the attach path
        /// supplies the donor's own.
        /// </summary>
        internal static Mesh BuildMesh(string json, out string[] boneNames,
            out AlbedoRequirement albedo)
        {
            return BuildMesh(json, out boneNames, out albedo, AllowedBones);
        }

        internal static Mesh BuildMesh(string json, out string[] boneNames,
            out AlbedoRequirement albedo, string[] allowedBones)
        {
            JObject document = JObject.Parse(json);
            int schema = (int?)document["schemaVersion"] ?? 0;
            if (schema != SupportedSchemaVersion)
                throw new InvalidDataException(
                    "Unsupported Pteranodon mesh schema " + schema + "; expected " +
                    SupportedSchemaVersion + ".");

            var names = ((JArray)document["bones"] ?? new JArray())
                .Select(value => (string)value).ToArray();
            if (names.Length == 0)
                throw new InvalidDataException("The bone list is empty.");
            if (names.Any(string.IsNullOrWhiteSpace))
                throw new InvalidDataException("The bone list has a blank name.");
            if (names.Distinct(StringComparer.Ordinal).Count() != names.Length)
                throw new InvalidDataException("The bone list repeats a name.");
            string[] unexpected = names.Where(value =>
                !allowedBones.Contains(value, StringComparer.Ordinal)).ToArray();
            if (unexpected.Length != 0)
                throw new InvalidDataException(
                    "The mesh binds to bones outside the declared set: " +
                    string.Join(", ", unexpected));

            albedo = ReadAlbedoRequirement(document["albedo"] as JObject);

            int vertexCount = (int?)document["vertexCount"] ?? 0;
            int triangleCount = (int?)document["triangleCount"] ?? 0;
            if (vertexCount <= 0 || triangleCount <= 0)
                throw new InvalidDataException(
                    "The mesh declares " + vertexCount + " vertices and " +
                    triangleCount + " triangles.");

            byte[] blob = Convert.FromBase64String((string)document["data"] ?? string.Empty);
            int expected = vertexCount * 12 + vertexCount * 12 + vertexCount * 8 +
                triangleCount * 3 * 4 + vertexCount * 4 * 8;
            if (blob.Length != expected)
                throw new InvalidDataException(
                    "The mesh payload is " + blob.Length + " bytes; " +
                    expected + " were expected for " + vertexCount +
                    " vertices and " + triangleCount + " triangles.");

            int offset = 0;
            var vertices = new Vector3[vertexCount];
            for (int index = 0; index < vertexCount; index++)
                vertices[index] = ReadVector(blob, ref offset);
            var normals = new Vector3[vertexCount];
            for (int index = 0; index < vertexCount; index++)
                normals[index] = ReadVector(blob, ref offset);
            var coordinates = new Vector2[vertexCount];
            for (int index = 0; index < vertexCount; index++)
            {
                float u = BitConverter.ToSingle(blob, offset);
                float v = BitConverter.ToSingle(blob, offset + 4);
                offset += 8;
                // The atlas is authored inside the unit square and the texture
                // is clamped; a coordinate outside it would sample an edge
                // texel and mean the generator and the painter had diverged.
                if (float.IsNaN(u) || float.IsNaN(v) || u < 0f || u > 1f ||
                    v < 0f || v > 1f)
                    throw new InvalidDataException(
                        "Vertex " + index + " has texture coordinate (" + u +
                        ", " + v + ") outside the atlas.");
                coordinates[index] = new Vector2(u, v);
            }

            var triangles = new int[triangleCount * 3];
            for (int index = 0; index < triangles.Length; index++)
            {
                triangles[index] = BitConverter.ToInt32(blob, offset);
                offset += 4;
                if (triangles[index] < 0 || triangles[index] >= vertexCount)
                    throw new InvalidDataException(
                        "A triangle indexes vertex " + triangles[index] +
                        " of " + vertexCount + ".");
            }

            var weights = new BoneWeight[vertexCount];
            for (int index = 0; index < vertexCount; index++)
            {
                var slots = new int[4];
                var values = new float[4];
                float total = 0f;
                for (int slot = 0; slot < 4; slot++)
                {
                    slots[slot] = BitConverter.ToInt32(blob, offset);
                    offset += 4;
                    values[slot] = BitConverter.ToSingle(blob, offset);
                    offset += 4;
                    total += values[slot];
                    if (values[slot] <= 0f) continue;
                    if (slots[slot] < 0 || slots[slot] >= names.Length)
                        throw new InvalidDataException(
                            "A vertex weight indexes bone " + slots[slot] +
                            " of " + names.Length + ".");
                }

                if (Math.Abs(total - 1f) > 0.001f)
                    throw new InvalidDataException(
                        "A vertex weight sums to " + total + ".");
                weights[index] = new BoneWeight
                {
                    boneIndex0 = slots[0], weight0 = values[0],
                    boneIndex1 = slots[1], weight1 = values[1],
                    boneIndex2 = slots[2], weight2 = values[2],
                    boneIndex3 = slots[3], weight3 = values[3]
                };
            }

            var mesh = new Mesh { name = "KMG_Pteranodon" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = coordinates;
            mesh.triangles = triangles;
            mesh.boneWeights = weights;
            // Identity bind poses on purpose: the attach path replaces them with
            // the donor's own, and shipping any other value would be shipping a
            // pose that is wrong by construction.
            var identity = new Matrix4x4[names.Length];
            for (int index = 0; index < names.Length; index++)
                identity[index] = Matrix4x4.identity;
            mesh.bindposes = identity;
            mesh.RecalculateBounds();

            boneNames = names;
            return mesh;
        }

        private static AlbedoRequirement ReadAlbedoRequirement(JObject albedo)
        {
            if (albedo == null)
                throw new InvalidDataException("The mesh data names no albedo.");
            string file = (string)albedo["file"] ?? string.Empty;
            string sha256 = (string)albedo["sha256"] ?? string.Empty;
            int width = (int?)albedo["width"] ?? 0;
            int height = (int?)albedo["height"] ?? 0;
            // A bare file name beside the mesh data, never a path: the mesh
            // must not be able to point the loader anywhere else.
            if (file.Length == 0 || file.IndexOfAny(new[] { '/', '\\', ':' }) >= 0 ||
                file == "." || file == ".." ||
                !file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    "The albedo file name is not a bare .png beside the mesh: '" +
                    file + "'.");
            if (sha256.Length != 64 || !sha256.All(IsLowerHex))
                throw new InvalidDataException(
                    "The albedo hash is not a lowercase SHA-256.");
            if (width < 64 || width > 4096 || height < 64 || height > 4096)
                throw new InvalidDataException(
                    "The albedo declares " + width + "x" + height + ".");
            return new AlbedoRequirement
            { File = file, Sha256 = sha256, Width = width, Height = height };
        }

        private static bool IsLowerHex(char value)
        {
            return (value >= '0' && value <= '9') || (value >= 'a' && value <= 'f');
        }

        /// <summary>
        /// Decodes the painting the mesh data names, and only that painting:
        /// the bytes must hash to the recorded value and the PNG header must
        /// carry the recorded dimensions before anything is decoded. Returns
        /// null with a reason rather than throwing, so a bad texture is a
        /// recorded fallback and not an exception in the middle of Configure.
        /// </summary>
        internal static Texture2D LoadAlbedo(string directory,
            AlbedoRequirement requirement, out string reason)
        {
            reason = null;
            string path = Path.Combine(directory ?? string.Empty, requirement.File);
            if (!File.Exists(path)) { reason = "albedo-missing"; return null; }

            byte[] bytes = File.ReadAllBytes(path);
            string actual;
            using (SHA256 sha = SHA256.Create())
                actual = string.Concat(sha.ComputeHash(bytes)
                    .Select(value => value.ToString("x2")));
            if (!string.Equals(actual, requirement.Sha256, StringComparison.Ordinal))
            { reason = "albedo-hash-mismatch"; return null; }

            // PNG signature, then the IHDR chunk: width, height, bit depth and
            // colour type at fixed offsets. Only 8-bit RGB or RGBA is accepted.
            if (bytes.Length < 33 || bytes[0] != 0x89 || bytes[1] != 0x50 ||
                bytes[2] != 0x4E || bytes[3] != 0x47 || bytes[12] != (byte)'I' ||
                bytes[13] != (byte)'H' || bytes[14] != (byte)'D' ||
                bytes[15] != (byte)'R')
            { reason = "albedo-not-png"; return null; }
            int width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
            int height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
            if (width != requirement.Width || height != requirement.Height)
            { reason = "albedo-dimensions-mismatch"; return null; }
            if (bytes[24] != 8 || (bytes[25] != 2 && bytes[25] != 6))
            { reason = "albedo-format-unsupported"; return null; }

            Texture2D texture = null;
            try
            {
                // Mip chain on: the creature is seen from the party camera at
                // every distance, and an unmipped 1024 texture shimmers.
                texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                if (!LoadImage(texture, bytes) || texture.width != width ||
                    texture.height != height)
                {
                    UnityEngine.Object.Destroy(texture);
                    reason = "albedo-decode-failed";
                    return null;
                }

                texture.name = "KMG_Pteranodon_Albedo";
                texture.filterMode = FilterMode.Trilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.anisoLevel = 4;
                texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
                return texture;
            }
            catch (Exception error)
            {
                if (texture != null) UnityEngine.Object.Destroy(texture);
                reason = "albedo-decode-failed:" + error.GetType().Name;
                return null;
            }
        }

        private static bool LoadImage(Texture2D texture, byte[] bytes)
        {
            Type type = Type.GetType(
                "UnityEngine.ImageConversion, UnityEngine.ImageConversionModule",
                false);
            MethodInfo method = type == null ? null : type.GetMethod("LoadImage",
                BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);
            if (method == null) throw new MissingMethodException(
                "Unity runtime lacks ImageConversion.LoadImage.");
            return (bool)method.Invoke(null, new object[] { texture, bytes, false });
        }

        private static Vector3 ReadVector(byte[] blob, ref int offset)
        {
            float x = BitConverter.ToSingle(blob, offset);
            float y = BitConverter.ToSingle(blob, offset + 4);
            float z = BitConverter.ToSingle(blob, offset + 8);
            offset += 12;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Resolves the bone transforms and bind poses for one donor renderer.
        ///
        /// Returns false and changes nothing if any declared bone is absent from
        /// the donor's skinned bone array, which is the only place a bind pose
        /// for it could come from.
        /// </summary>
        internal static bool TryResolveDonorBinding(
            SkinnedMeshRenderer donor, string[] boneNames,
            out Transform[] bones, out Matrix4x4[] bindposes, out string reason)
        {
            bones = null;
            bindposes = null;
            reason = null;
            if (donor == null) { reason = "donor-renderer-missing"; return false; }
            if (boneNames == null || boneNames.Length == 0)
            { reason = "no-bone-names"; return false; }
            Transform[] donorBones = donor.bones;
            Mesh donorMesh = donor.sharedMesh;
            if (donorBones == null || donorBones.Length == 0)
            { reason = "donor-has-no-bones"; return false; }
            if (donorMesh == null)
            { reason = "donor-has-no-mesh"; return false; }
            Matrix4x4[] donorBind = donorMesh.bindposes;
            if (donorBind == null || donorBind.Length != donorBones.Length)
            { reason = "donor-bind-pose-count-mismatch"; return false; }

            var index = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int slot = 0; slot < donorBones.Length; slot++)
            {
                if (donorBones[slot] == null) continue;
                // A duplicated bone name would make the mapping ambiguous.
                if (index.ContainsKey(donorBones[slot].name))
                {
                    reason = "donor-bone-name-ambiguous:" + donorBones[slot].name;
                    return false;
                }

                index[donorBones[slot].name] = slot;
            }

            var resolvedBones = new Transform[boneNames.Length];
            var resolvedBind = new Matrix4x4[boneNames.Length];
            for (int slot = 0; slot < boneNames.Length; slot++)
            {
                int donorSlot;
                if (!index.TryGetValue(boneNames[slot], out donorSlot))
                { reason = "donor-bone-missing:" + boneNames[slot]; return false; }
                resolvedBones[slot] = donorBones[donorSlot];
                resolvedBind[slot] = donorBind[donorSlot];
            }

            bones = resolvedBones;
            bindposes = resolvedBind;
            return true;
        }
    }
}
