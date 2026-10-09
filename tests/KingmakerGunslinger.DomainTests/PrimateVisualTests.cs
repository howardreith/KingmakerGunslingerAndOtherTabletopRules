using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 18 original bodies, checked where the binding contract lives.
    ///
    /// <para>These run against the shipped mesh files and the pure policy the
    /// runtime calls. What they cannot do is prove the engine renders them,
    /// which is what the guarded runtime review exists for.</para>
    /// </summary>
    internal static class PrimateVisualTests
    {
        private static readonly string[] Keys = { "ape", "dire-ape" };

        private static string AssetDirectory()
        {
            var cursor = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int depth = 0; depth < 8 && cursor != null; depth++)
            {
                string candidate = Path.Combine(cursor.FullName,
                    Path.Combine("assets", "sprint18-primates"));
                if (Directory.Exists(candidate)) return candidate;
                cursor = cursor.Parent;
            }
            throw new DirectoryNotFoundException(
                "The shipped Sprint 18 body directory was not found.");
        }

        private static JObject Body(string key)
        {
            return JObject.Parse(File.ReadAllText(
                Path.Combine(AssetDirectory(), key + "-mesh.json")));
        }

        /// <summary>
        /// The donor the census chose, pinned by prefab, renderer, bone count
        /// and root bone, and refused the moment any one of them moves.
        /// </summary>
        internal static void OnlyTheCensusChosenDonorIsAccepted()
        {
            var skins = new[] { PrimateVisualPolicy.BodyRenderer,
                PrimateVisualPolicy.EquipmentRenderer };
            foreach (string key in Keys)
            {
                Assertions.True(PrimateVisualPolicy.PermitsDonor(key,
                        PrimateVisualPolicy.TrollPrefab, skins, 61, 62, "Pelvis",
                        new string[0]),
                    key + " must accept the exact surveyed donor.");
                Assertions.False(PrimateVisualPolicy.PermitsDonor(key,
                        "d6e0acbdbdb56114898922063ae2cba0", skins, 61, 62, "Pelvis",
                        new string[0]),
                    key + " must refuse the Owlbear the census rejected.");
                Assertions.False(PrimateVisualPolicy.PermitsDonor(key,
                        PrimateVisualPolicy.TrollPrefab, skins, 60, 62, "Pelvis",
                        new string[0]),
                    key + " must refuse a changed body bone count.");
                Assertions.False(PrimateVisualPolicy.PermitsDonor(key,
                        PrimateVisualPolicy.TrollPrefab, skins, 61, 62, "LowerTorso",
                        new string[0]),
                    key + " must refuse a changed root bone.");
                Assertions.False(PrimateVisualPolicy.PermitsDonor(key,
                        PrimateVisualPolicy.TrollPrefab, skins, 61, 62, "Pelvis",
                        new[] { "WP_Club" }),
                    key + " must refuse a donor carrying a static weapon mesh.");
                Assertions.False(PrimateVisualPolicy.PermitsDonor(key,
                        PrimateVisualPolicy.TrollPrefab,
                        new[] { PrimateVisualPolicy.BodyRenderer }, 61, 62, "Pelvis",
                        new string[0]),
                    key + " must refuse a donor missing its equipment skin.");
            }
            Assertions.False(PrimateVisualPolicy.PermitsDonor("owlbear",
                    PrimateVisualPolicy.TrollPrefab, skins, 61, 62, "Pelvis",
                    new string[0]),
                "No creature outside Sprint 18 may claim this donor contract.");
        }

        /// <summary>
        /// Only the two hidden apes are reachable, and only when all three of
        /// identity, blueprint name and donor prefab agree.
        /// </summary>
        internal static void OnlyTheTwoHiddenApesAreRebodied()
        {
            string key;
            Assertions.True(PrimateVisualPolicy.TryProductionPrimate(true,
                    PrimateVisualPolicy.ApeGuid, PrimateVisualPolicy.ApeBlueprintName,
                    PrimateVisualPolicy.TrollPrefab, out key) && key == "ape",
                "The Ape must be reached by its exact identity.");
            Assertions.True(PrimateVisualPolicy.TryProductionPrimate(true,
                    PrimateVisualPolicy.DireApeGuid,
                    PrimateVisualPolicy.DireApeBlueprintName,
                    PrimateVisualPolicy.TrollPrefab, out key) && key == "dire-ape",
                "The Dire Ape must be reached by its exact identity.");
            Assertions.False(PrimateVisualPolicy.TryProductionPrimate(false,
                    PrimateVisualPolicy.ApeGuid, PrimateVisualPolicy.ApeBlueprintName,
                    PrimateVisualPolicy.TrollPrefab, out key),
                "A disabled Expanded Summoning module must rebody nothing.");
            Assertions.False(PrimateVisualPolicy.TryProductionPrimate(true,
                    PrimateVisualPolicy.TrollBlueprint, "CR10_FerociousTrollGuard",
                    PrimateVisualPolicy.TrollPrefab, out key),
                "The native troll must never be rebodied.");
            Assertions.False(PrimateVisualPolicy.TryProductionPrimate(true,
                    PrimateVisualPolicy.ApeGuid, PrimateVisualPolicy.ApeBlueprintName,
                    "130f0866af3249a4e817ec7e6e9ecd89", out key),
                "A unit on another prefab must not take the ape body.");
            Assertions.False(PrimateVisualPolicy.TryProductionPrimate(true,
                    PrimateVisualPolicy.ApeGuid, "KMG_Summoning_Unit_Owlbear",
                    PrimateVisualPolicy.TrollPrefab, out key),
                "Identity and name must agree, not merely one of them.");
        }

        /// <summary>
        /// The five donor branches that must stay empty stay empty, in the
        /// policy and in both shipped files. An ape has no tail.
        /// </summary>
        internal static void NoApeWeightsTheTailOrTheTongue()
        {
            PrimateVisualPolicy.Validate();
            foreach (string key in Keys)
            {
                string[] drivers = PrimateVisualPolicy.Bones(key);
                Assertions.Equal(56, drivers.Length,
                    key + " must drive exactly the 56 reviewed donor bones.");
                Assertions.Equal(61,
                    drivers.Length + PrimateVisualPolicy.ExcludedBones.Length,
                    "Every bone of the donor body skin must be accounted for.");
                foreach (string empty in PrimateVisualPolicy.ExcludedBones)
                    Assertions.False(drivers.Contains(empty, StringComparer.Ordinal),
                        key + " must not drive " + empty + ".");
                string[] shipped = Body(key)["bones"].Values<string>().ToArray();
                Assertions.True(PrimateVisualPolicy.PermitsBones(key, shipped),
                    key + " ships a driver set the policy does not accept.");
                foreach (string empty in PrimateVisualPolicy.ExcludedBones)
                    Assertions.False(shipped.Contains(empty, StringComparer.Ordinal),
                        key + " ships geometry on " + empty + ".");
            }
        }

        /// <summary>
        /// The shipped files declare the anatomy their printed entries imply,
        /// and the policy refuses the pair the other way round.
        /// </summary>
        internal static void ShippedBodiesDeclareThePrintedAnatomy()
        {
            foreach (string key in Keys)
            {
                JObject body = Body(key);
                Assertions.Equal(key, (string)body["creature"],
                    "A shipped body must name its own creature.");
                Assertions.Equal(PrimateVisualPolicy.TrollPrefab,
                    (string)body["donorPrefab"], key + " donor prefab changed.");
                Assertions.Equal(PrimateVisualPolicy.BodyRenderer,
                    (string)body["donorRenderer"], key + " donor renderer changed.");
                Assertions.True(PrimateVisualPolicy.PermitsOriginalWinding(key,
                        (string)body["triangleWinding"]),
                    key + " must declare the shared exporter winding.");
                Assertions.True(PrimateVisualPolicy.PermitsAnatomy(key,
                        (bool?)body["tailGeometry"], (bool?)body["tongueGeometry"],
                        (bool?)body["jawSeparated"], (int?)body["visibleLimbs"],
                        (bool?)body["clawedHands"], (bool?)body["opposableThumbs"],
                        (bool?)body["knuckleWalkAuthored"]),
                    key + " ships an anatomy the policy rejects.");
            }
            // Only the Dire Ape has claws, because only the Dire Ape has claw
            // attacks. Swapping that is a rejection, not a cosmetic choice.
            Assertions.False(PrimateVisualPolicy.PermitsAnatomy("ape", false, false,
                    true, 4, true, true, false),
                "The Ape has no claw attack and must not ship claws.");
            Assertions.False(PrimateVisualPolicy.PermitsAnatomy("dire-ape", false,
                    false, true, 4, false, true, false),
                "The Dire Ape rakes for two claws and must ship them.");
            foreach (string key in Keys)
            {
                Assertions.False(PrimateVisualPolicy.PermitsAnatomy(key, true, false,
                        true, 4, key == "dire-ape", true, false),
                    key + " must refuse tail geometry.");
                Assertions.False(PrimateVisualPolicy.PermitsAnatomy(key, false, false,
                        false, 4, key == "dire-ape", true, false),
                    key + " must refuse a fused jaw.");
                Assertions.False(PrimateVisualPolicy.PermitsAnatomy(key, false, false,
                        true, 4, key == "dire-ape", true, true),
                    key + " must not claim an authored knuckle-walking gait.");
            }
        }

        /// <summary>
        /// Every authored driver maps onto a real donor bone and keeps the
        /// donor's own bindpose. There is no support slot and no fallback.
        /// </summary>
        internal static void DriverMappingIsExactAndHasNoFallback()
        {
            string[] native = PrimateVisualPolicy.Bones("ape")
                .Concat(PrimateVisualPolicy.ExcludedBones).ToArray();
            foreach (string key in Keys)
            {
                string[] authored = Body(key)["bones"].Values<string>().ToArray();
                int[] slots;
                Assertions.True(PrimateVisualPolicy.TryResolveDriverSlots(key,
                        authored, native, out slots),
                    key + " must map onto the surveyed donor frame.");
                Assertions.Equal(authored.Length, slots.Length,
                    key + " must resolve one slot per driver.");
                Assertions.True(slots.All(slot => slot >= 0 && slot < native.Length),
                    key + " must resolve every driver to a real donor bone.");
                Assertions.Equal(authored.Length, slots.Distinct().Count(),
                    key + " must not point two drivers at one donor bone.");
                for (int index = 0; index < authored.Length; index++)
                    Assertions.Equal(authored[index], native[slots[index]],
                        key + " driver mapping is not name for name.");

                string[] renamedJaw = native.Select(name =>
                    name == "Jaw_01" ? "Jaw_Renamed" : name).ToArray();
                Assertions.False(PrimateVisualPolicy.TryResolveDriverSlots(key,
                        authored, renamedJaw, out slots),
                    key + " must refuse a donor missing a driver.");
                string[] renamedTail = native.Select(name =>
                    name == "Tail_01" ? "Tail_Renamed" : name).ToArray();
                Assertions.False(PrimateVisualPolicy.TryResolveDriverSlots(key,
                        authored, renamedTail, out slots),
                    key + " must refuse a donor whose empty branches moved.");
                Assertions.False(PrimateVisualPolicy.TryResolveDriverSlots(key,
                        authored, native.Take(60).ToArray(), out slots),
                    key + " must refuse a donor of the wrong size.");
            }
        }

        /// <summary>
        /// Each shipped mesh names its own painting, and the bytes beside it
        /// are that painting. A mesh without its painting is not the reviewed
        /// creature, so it is not shown.
        /// </summary>
        internal static void EachBodyIsPinnedToItsOwnPainting()
        {
            string directory = AssetDirectory();
            var seen = new List<string>();
            foreach (string key in Keys)
            {
                JObject albedo = (JObject)Body(key)["albedo"];
                string file = (string)albedo["file"];
                Assertions.Equal(key + "-albedo.png", file,
                    key + " must name its own painting.");
                string path = Path.Combine(directory, file);
                Assertions.True(File.Exists(path),
                    key + " painting is missing from the shipped assets.");
                string actual;
                using (var sha = System.Security.Cryptography.SHA256.Create())
                    actual = BitConverter.ToString(sha.ComputeHash(
                        File.ReadAllBytes(path))).Replace("-", string.Empty).ToLowerInvariant();
                Assertions.Equal((string)albedo["sha256"], actual,
                    key + " painting is not the reviewed file.");
                Assertions.Equal(1024, (int)albedo["width"], key + " painting width.");
                Assertions.Equal(1024, (int)albedo["height"], key + " painting height.");
                seen.Add(actual);
            }
            Assertions.True(seen.Distinct(StringComparer.Ordinal).Count() == seen.Count,
                "The two apes must not share one painting.");
        }

        /// <summary>
        /// The instance-delimited resource names the guarded crowd census
        /// counts, so one ape cannot be charged with a neighbour's resources.
        /// </summary>
        internal static void InstanceResourcesAreNamedPerInstance()
        {
            const string mine = "KMG_ape_Original_-4821";
            Assertions.True(PrimateVisualPolicy.IsPrimateInstanceResource("ape",
                    mine, mine), "A body is its own instance resource.");
            Assertions.True(PrimateVisualPolicy.IsPrimateInstanceResource("ape",
                    mine, mine + "_Albedo"), "A painting is an instance resource.");
            Assertions.True(PrimateVisualPolicy.IsPrimateInstanceResource("ape",
                    mine, mine + " (Instance)"), "A clone is an instance resource.");
            Assertions.False(PrimateVisualPolicy.IsPrimateInstanceResource("ape",
                    mine, "KMG_ape_Original_7_Albedo"),
                "A neighbouring ape is not this one's resource.");
            Assertions.False(PrimateVisualPolicy.IsPrimateInstanceResource("ape",
                    mine, "troll_black_character_d"),
                "A native material is never an instance resource.");
            Assertions.False(PrimateVisualPolicy.IsPrimateInstanceResource("ape",
                    "KMG_ape_Original_notanumber", "KMG_ape_Original_notanumber"),
                "An unparsable instance name is refused.");
            Assertions.False(PrimateVisualPolicy.IsPrimateInstanceResource("owlbear",
                    mine, mine), "Only the two apes own Sprint 18 resources.");
        }
    }
}
