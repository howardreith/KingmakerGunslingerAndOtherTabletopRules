using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 16 registers the Dire Crocodile and gives the Crocodile the
    /// signature behaviour it has never had.
    ///
    /// <para>These tests cover registration, mechanics and publication. The Dire Crocodile had
    /// no identities at all before this sprint - it existed in the ideal-roster
    /// plan and nowhere else - so the first thing to establish is that it is
    /// now a real creature with its printed stat block, that every one of its
    /// six placements publishes without reallocating it, and that the Crocodile beside it was not
    /// disturbed, because the sprint's order requires that creature's accepted
    /// identity and placements be preserved exactly.</para>
    /// </summary>
    internal static class ExpandedSummoningSprint16Tests
    {
        internal const int AppendedLedgerIdentities = 29;

        private const string DireKey = "dire-crocodile";
        private const string CrocodileKey = "crocodile";

        internal static void DeathReviewResumesOnlyItsRequestedOwnedDeath()
        {
            foreach (bool owned in new[] { false, true })
            foreach (bool sourceDead in new[] { false, true })
            foreach (bool targetDead in new[] { false, true })
            foreach (string boundary in new[] { "source-death", "target-death",
                "dismissal", "expiry", "transition", "spit-out", "Source-Death", "", null })
            {
                bool expected = owned && (boundary == "source-death" ? sourceDead :
                    boundary == "target-death" && targetDead);
                Assertions.Equal(expected, CrocodilianLifecycleReviewPolicy.CanResumeAfterRequestedDeath(
                    owned, boundary, sourceDead, targetDead),
                    "Only the exact owned death requested by this drill permits clock resumption.");
            }
        }

        internal static void ManualReviewRequiresItsOwnCompletedCommands()
        {
            foreach (bool manual in new[] { false, true })
            foreach (bool rejected in new[] { false, true })
            foreach (bool issued in new[] { false, true })
            foreach (bool combat in new[] { false, true })
            foreach (int frames in new[] { -1, 0, 89, 90, 5000 })
            {
                bool expected = combat && frames >= 90;
                if (manual && (!rejected || !issued)) expected = false;
                Assertions.Equal(expected, CrocodilianCommandReviewPolicy.CanFinish(
                    manual, rejected, issued, combat, frames),
                    "Manual completion requires its real attack and finished rejection; AI never needs an injected command.");
            }
            Assertions.False(CrocodilianCommandReviewPolicy.CanFinish(true, false, false, true, 5000),
                "An incidental bite/rider cannot finish the manual-swallow drill, regardless of elapsed frames.");
        }

        internal static void CrocodilianIconConsumerGraphIsComplete()
        {
            string root = Environment.CurrentDirectory;
            JObject catalog = JObject.Parse(File.ReadAllText(Path.Combine(root,
                "assets-source/original-icons/icon-catalog.json")));
            JObject production = JObject.Parse(File.ReadAllText(Path.Combine(root,
                "assets-source/original-icons/icon-overhaul-v2/production/production-manifest.json")));
            string[] prefixes = { "KMG.Summoning.Special.Crocodile.",
                "KMG.Summoning.Special.DireCrocodile.", "KMG.Summoning.Special.Crocodilian." };
            JObject[] consumers = catalog["consumers"].OfType<JObject>().Where(value =>
                prefixes.Any(prefix => ((string)value["symbol"]).StartsWith(prefix, StringComparison.Ordinal))).ToArray();
            Assertions.Equal(14, consumers.Length, "All nine visible and five internal consumers have dispositions.");
            Assertions.Equal(5, consumers.Count(value => (string)value["disposition"] == "hidden-internal"),
                "Brains, cast actions and scorer remain non-icon carriers.");
            foreach (string token in new[] { "Crocodile", "DireCrocodile" })
                foreach (string suffix in new[] { "Sprint", "SprintState", "SprintCooldown", "CombatTraits" })
                    Assertions.Equal(suffix == "CombatTraits" ? "crocodilian-death-roll" : "crocodilian-sprint",
                        (string)consumers.Single(value => (string)value["symbol"] ==
                            "KMG.Summoning.Special." + token + "." + suffix)["concept"],
                        "Each action, effect and cooldown explicitly shares the correct identity.");
            Assertions.Equal("dire-crocodile-swallowed", (string)consumers.Single(value =>
                (string)value["symbol"] == "KMG.Summoning.Special.DireCrocodile.Swallowed")["concept"],
                "The victim has its own swallowed identity, not Purple Worm art.");
            foreach (string key in new[] { "crocodilian-sprint", "crocodilian-death-roll", "dire-crocodile-swallowed" })
            {
                JObject concept = catalog["concepts"].OfType<JObject>().Single(value => (string)value["key"] == key);
                JObject record = production["records"].OfType<JObject>().Single(value => (string)value["key"] == key);
                Assertions.Equal("combat-emblem-64", (string)concept["exportProfile"], "Existing approved physical-emblem family.");
                Assertions.True(record["exportSize"].Values<int>().SequenceEqual(new[] { 64, 64 }) &&
                    record["sourceSize"].Values<int>().SequenceEqual(new[] { 512, 512 }), "Source and runtime profiles stay distinct.");
                Assertions.True(record["approvedHash"].Type == JTokenType.Null &&
                    concept["visualReview"]["reviewedExportSha256"].Type == JTokenType.Null,
                    "Offline technical checks are not owner approval.");
                using (SHA256 hash = SHA256.Create())
                    Assertions.Equal((string)record["exportSha256"],
                        BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(Path.Combine(root,
                            (string)concept["runtimeExport"]["path"])))).Replace("-", "").ToLowerInvariant(),
                        "The packaged icon is the exact reviewed export.");
            }
        }

        internal static void CrocodilianBonesFailClosed()
        {
            foreach (string key in CrocodilianVisualPolicy.Keys)
            foreach (float invalid in new[] { -1f, 0f, float.NaN, float.PositiveInfinity })
            {
                Assertions.Equal(0f, CrocodilianVisualPolicy.ContactApproach(key, invalid, 1f),
                    "Invalid distances never displace a visual.");
                Assertions.Equal(0f, CrocodilianVisualPolicy.ContactApproach(key, 1f, invalid),
                    "Invalid timing never displaces a visual.");
            }
            Assertions.Equal(0.1f, CrocodilianVisualPolicy.ContactApproach("crocodile", 0.1f, 1f),
                "An approach stops at the existing target surface.");
            Assertions.Equal(0.25f, CrocodilianVisualPolicy.ContactApproach("crocodile", 4f, 1f),
                "Ordinary reduced-reach Crocodile keeps its original cosmetic limit.");
            Assertions.Equal(0.125f, CrocodilianVisualPolicy.ContactApproach("crocodile", 4f, 0.5f),
                "The cosmetic offset follows the attack envelope.");
            Assertions.Equal(0.25f, CrocodilianVisualPolicy.ContactApproach("crocodile", 4f, 2f),
                "Repeated/overshooting timing never exceeds the cosmetic cap.");
            foreach (float gap in new[] { 0.1f, 1.292f, 2.828f })
            {
                Assertions.Equal(gap, CrocodilianVisualPolicy.ContactApproach("dire-crocodile", gap, 1f),
                    "The Dire reaches the measured live AI surface without overshooting it.");
                Assertions.Equal(gap / 2f, CrocodilianVisualPolicy.ContactApproach("dire-crocodile", gap, 0.5f),
                    "Even a close target eases in and out; the offset does not plateau until the envelope ends.");
            }
            Assertions.Equal(3.048f, CrocodilianVisualPolicy.ContactApproach("dire-crocodile", 99f, 1f),
                "Dire pose never exceeds half its printed 20ft footprint.");
            foreach (string unknown in new[] { "monitor-lizard", "Dire-Crocodile", "", null })
                Assertions.Equal(0f, CrocodilianVisualPolicy.ContactApproach(unknown, 3f, 1f),
                    "No native or unknown creature receives a contact pose.");
            foreach (string key in CrocodilianVisualPolicy.Keys)
            {
                Assertions.True(CrocodilianVisualPolicy.IsContactRootPermitted(key,
                    "cent_root1_jnt", true, CrocodilianVisualPolicy.Bones),
                    "The owned common ancestor carries the complete approved skin.");
                Assertions.False(CrocodilianVisualPolicy.IsContactRootPermitted(key,
                    "cent_spine1_jnt", true, CrocodilianVisualPolicy.Bones),
                    "The renderer spine root is not the common jaw/tail root.");
                Assertions.False(CrocodilianVisualPolicy.IsContactRootPermitted(key,
                    "cent_root1_jnt", false, CrocodilianVisualPolicy.Bones),
                    "A matching rig on another view is never an owned root.");
                Assertions.True(CrocodilianVisualPolicy.IsPermitted(key,
                    CrocodilianVisualPolicy.Bones), "Reviewed driver set must load.");
                foreach (string bone in CrocodilianVisualPolicy.Bones)
                {
                    Assertions.False(CrocodilianVisualPolicy.IsContactRootPermitted(key,
                        "cent_root1_jnt", true, CrocodilianVisualPolicy.Bones.Where(value => value != bone)),
                        "A skin driver outside the common root fails closed: " + bone);
                    Assertions.False(CrocodilianVisualPolicy.IsPermitted(key,
                        CrocodilianVisualPolicy.Bones.Where(value => value != bone)),
                        "Missing driver must fail: " + bone);
                }
                foreach (string foreign in new[] { "cent_tongue1_jnt", "L_Foot3",
                    "left_arm2_jnt", "cent_root1_jnt", "Head" })
                    Assertions.False(CrocodilianVisualPolicy.IsPermitted(key,
                        CrocodilianVisualPolicy.Bones.Concat(new[] { foreign })),
                        "Unreviewed influence must fail: " + foreign);
                Assertions.False(CrocodilianVisualPolicy.IsPermitted(key,
                    CrocodilianVisualPolicy.Bones.Concat(new[] { "cent_jaw1_jnt" })),
                    "Duplicate bone must fail.");
            }
            Assertions.False(CrocodilianVisualPolicy.IsPermitted("monitor-lizard",
                CrocodilianVisualPolicy.Bones), "The native negative control is not a target.");
            Assertions.False(CrocodilianVisualPolicy.IsPermitted(null, null),
                "Unknown or absent input must fail closed.");
            Assertions.False(CrocodilianVisualPolicy.IsContactRootPermitted("monitor-lizard",
                "cent_root1_jnt", true, CrocodilianVisualPolicy.Bones),
                "A correct native rig is still not a project-owned crocodilian.");
            Assertions.False(CrocodilianVisualPolicy.IsContactRootPermitted("crocodile",
                null, true, CrocodilianVisualPolicy.Bones), "A missing root fails closed.");
            Assertions.False(CrocodilianVisualPolicy.IsContactRootPermitted("crocodile",
                "cent_root1_jnt", true, null), "Absent descendants fail closed.");
        }

        internal static void CrocodilianOriginalAssetsAreComplete()
        {
            string directory = Path.Combine(Environment.CurrentDirectory,
                "assets", "sprint16-crocodilians");
            string[] meshHashes = {
                "770fa7c3c87fb74f3335358069cefde77529868e30184e090bf3c7c840065fd7",
                "eb7a9182fbe5f66dc8b819efd9641a62da611230923bf4bd95ca1383d4e7173a" };
            int keyIndex = 0;
            foreach (string key in CrocodilianVisualPolicy.Keys)
            {
                string path = Path.Combine(directory, key + "-mesh.json");
                using (SHA256 hash = SHA256.Create())
                    Assertions.Equal(meshHashes[keyIndex++],
                        BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)))
                            .Replace("-", "").ToLowerInvariant(), "Reviewed original mesh.");
                JObject mesh = JObject.Parse(File.ReadAllText(path));
                string[] bones = mesh["bones"].Values<string>().ToArray();
                Assertions.True(CrocodilianVisualPolicy.IsPermitted(key, bones),
                    "The runtime policy must accept the actual packaged bone rows.");
                Assertions.True((int)mesh["schemaVersion"] == 2 &&
                    (int)mesh["visibleLegs"] == 4 && (int)mesh["tailJoints"] == 7 &&
                    (bool)mesh["jawSeparated"] && mesh["bindposes"] == null &&
                    mesh["bindMatrices"] == null, "Original-only anatomy/schema contract.");
                byte[] data = Convert.FromBase64String((string)mesh["data"]);
                int vertices = (int)mesh["vertexCount"];
                int triangles = (int)mesh["triangleCount"];
                Assertions.True(vertices == 2342 && triangles > 4000 &&
                    data.Length == vertices * 64 + triangles * 12,
                    "Complete vertices/normals/UVs/indices/weights, not a donor reference.");
                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    float minY = float.MaxValue, maxY = float.MinValue;
                    float minZ = float.MaxValue, maxZ = float.MinValue;
                    for (int i = 0; i < vertices; i++)
                    {
                        float x = reader.ReadSingle(), y = reader.ReadSingle(), z = reader.ReadSingle();
                        Assertions.True(!float.IsNaN(x + y + z) && !float.IsInfinity(x + y + z),
                            "Finite original positions.");
                        minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                        minZ = Math.Min(minZ, z); maxZ = Math.Max(maxZ, z);
                    }
                    Assertions.True(minY >= 0 && maxY - minY < .75f &&
                        maxZ - minZ > 3.4f, "Low, elongated crocodilian silhouette.");
                    reader.BaseStream.Position = vertices * 24;
                    for (int i = 0; i < vertices * 2; i++)
                    {
                        float uv = reader.ReadSingle();
                        Assertions.True(uv > 0 && uv < 1, "Inset finite UV, no atlas outer seam.");
                    }
                    for (int i = 0; i < triangles * 3; i++)
                    {
                        int index = reader.ReadInt32();
                        Assertions.True(index >= 0 && index < vertices, "In-range triangle index.");
                    }
                    var totals = new double[bones.Length];
                    for (int i = 0; i < vertices; i++)
                    {
                        float sum = 0;
                        for (int slot = 0; slot < 4; slot++)
                        {
                            int bone = reader.ReadInt32();
                            float weight = reader.ReadSingle();
                            Assertions.True(bone >= 0 && bone < bones.Length &&
                                weight >= 0 && weight <= 1, "Valid bone influence.");
                            sum += weight; totals[bone] += weight;
                        }
                        Assertions.True(Math.Abs(sum - 1) < .00001, "Normalized weights.");
                    }
                    Assertions.True(totals.All(value => value > 0),
                        "Jaw, tail and all four leg chains carry actual original vertices.");
                }
                JObject albedo = (JObject)mesh["albedo"];
                Assertions.Equal(key + "-albedo.png", (string)albedo["file"], "Own painting.");
                using (SHA256 hash = SHA256.Create())
                    Assertions.Equal((string)albedo["sha256"],
                        BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(
                            Path.Combine(directory, (string)albedo["file"]))))
                            .Replace("-", "").ToLowerInvariant(), "Exact texture pairing.");
            }
        }

        internal static void CrocodilianScaleChangesOnlyTheDireIdentity()
        {
            SummonViewScaleCatalog.Validate();
            float multiplier;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                "KMG_Summoning_Unit_DireCrocodile", out multiplier) && multiplier == 2f,
                "The Gargantuan original has an explicit 2x view step.");
            foreach (string name in new[] { "KMG_Summoning_Unit_Crocodile",
                "KMG_Summoning_Unit_MonitorLizard", "CR3_MonitorLizardStandard" })
                Assertions.False(SummonViewScaleCatalog.TryGetMultiplier(name, out multiplier),
                    "No scale mutation of ordinary Crocodile or native donor: " + name);
            Assertions.True(SummonViewScaleCatalog.All.Where(value =>
                value.CreatureKey != "dire-crocodile").All(value =>
                    value.Multiplier >= .20f && value.Multiplier <= 1.25f),
                "No general widening of existing creature scale bounds.");
        }

        internal static void EveryProfileResolvesItsNativeNaturalArmor()
        {
            foreach (NaturalSummonProfile profile in ExpandedSummoningNaturalProfiles.All)
            {
                string guid = ExpandedSummoningNaturalProfiles.NaturalArmorGuid(
                    profile.NaturalArmor);
                Guid parsed;
                Assertions.True(profile.NaturalArmor == 0 ? guid == null :
                    Guid.TryParseExact(guid, "N", out parsed),
                    "Natural armor has no exact native identity: " + profile.Key);
            }
            Assertions.Equal("72c294dca841e3944869fb087bacf272",
                ExpandedSummoningNaturalProfiles.NaturalArmorGuid(15),
                "Dire Crocodile must resolve the censused native +15 fact.");
            Assertions.Throws<InvalidOperationException>(() =>
                ExpandedSummoningNaturalProfiles.NaturalArmorGuid(-1),
                "Negative natural armor must not guess a donor.");
            Assertions.Throws<InvalidOperationException>(() =>
                ExpandedSummoningNaturalProfiles.NaturalArmorGuid(999),
                "Unsupported natural armor must fail closed.");
        }

        /// <summary>
        /// The qualified hidden candidate publishes exactly its six preserved
        /// tier-7 placements, without changing any previously published root.
        /// </summary>
        internal static void TheDireCrocodilePublishesItsSixPreservedPlacements()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] mine = all
                .Where(value => value.Creature.Key == DireKey).ToArray();
            if (mine.Length != 6)
                throw new InvalidOperationException(
                    "The Dire Crocodile registers " + mine.Length +
                    " placements, not 6: parents 7, 8 and 9 in each family.");
            if (mine.Any(value => !SummonVisibilityCatalog.IsPublished(value)))
                throw new InvalidOperationException(
                    "Every qualified Dire Crocodile placement must publish.");
            Assertions.Equal(970, all.Count(value =>
                    value.Creature.Key != DireKey && SummonVisibilityCatalog.IsPublished(value)),
                "Dire publication must preserve all 970 previously published roots.");
            Assertions.Equal(976, all.Count(SummonVisibilityCatalog.IsPublished),
                "Only Dire's six roots may change the publication surface.");
            // Its own tier is a single creature, the next is 1d3, the rest
            // 1d4+1 - the same quantity rule every creature follows.
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            {
                SummonVariantSpec[] ordered = mine
                    .Where(value => value.Family == family)
                    .OrderBy(value => value.ParentTier).ToArray();
                if (ordered.Length != 3)
                    throw new InvalidOperationException(
                        family + " must offer the Dire Crocodile three times.");
                if (ordered[0].ParentTier != 7 ||
                        ordered[0].Multiplicity != SummonMultiplicity.One ||
                        ordered[1].Multiplicity != SummonMultiplicity.OneD3 ||
                        ordered[2].Multiplicity !=
                            SummonMultiplicity.OneD4PlusOne)
                    throw new InvalidOperationException(
                        family + " quantity mapping for the Dire Crocodile " +
                        "is wrong.");
            }
            // No registered creature remains withheld after this publication.
            if (SummonVisibilityCatalog.SuppressedLogicalPlacementCount != 0)
                throw new InvalidOperationException(
                    "Sprint 16 publication must leave no withheld placements.");
            if (SummonVisibilityCatalog.RegisteredLogicalPlacementCount -
                    SummonVisibilityCatalog.SuppressedLogicalPlacementCount !=
                    SummonVisibilityCatalog.PublishedLogicalPlacementCount)
                throw new InvalidOperationException(
                    "The published surface must be the registered one less " +
                    "the withheld.");
        }

        /// <summary>
        /// The printed stat block, written down rather than derived at load.
        ///
        /// <para>Gargantuan with a 20-foot space and a 15-foot reach is the
        /// standard footprint for that size, so unlike the Crocodile this
        /// creature must *not* carry the reduced-reach carrier. Getting that
        /// backwards would quietly shorten a creature whose reach is most of
        /// what makes it dangerous.</para>
        /// </summary>
        internal static void TheDireCrocodileMatchesItsPrintedStatBlock()
        {
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(DireKey);
            foreach (var expected in new[] {
                new { Name = "hitDice", Want = 12, Got = profile.HitDice },
                new { Name = "strength", Want = 37, Got = profile.Strength },
                new { Name = "dexterity", Want = 10, Got = profile.Dexterity },
                new { Name = "constitution", Want = 25,
                    Got = profile.Constitution },
                new { Name = "wisdom", Want = 14, Got = profile.Wisdom },
                new { Name = "charisma", Want = 2, Got = profile.Charisma },
                new { Name = "speed", Want = 20, Got = profile.SpeedFeet },
                // AC 21 = 10 + 15 natural - 4 size.
                new { Name = "naturalArmor", Want = 15,
                    Got = profile.NaturalArmor } })
                if (expected.Got != expected.Want)
                    throw new InvalidOperationException(
                        "Dire Crocodile " + expected.Name + " is " +
                        expected.Got + ", not the printed " + expected.Want +
                        ".");
            if (profile.Size != "Gargantuan")
                throw new InvalidOperationException(
                    "The Dire Crocodile is Gargantuan, not " + profile.Size +
                    ".");
            if (profile.HitDieClass != "Animal")
                throw new InvalidOperationException(
                    "The Dire Crocodile is an animal.");
            if (profile.PrimaryWeapon != "Bite3d6")
                throw new InvalidOperationException(
                    "The printed bite is 3d6, not " + profile.PrimaryWeapon +
                    ".");
            // The tail slap is five lower than the bite at half the Strength
            // bonus, which is a secondary natural attack. Phase 1 put the
            // Pony's and Horse's hooves among the primaries and the owner
            // rejected it, so this is asserted rather than assumed.
            if (profile.AdditionalWeapons.Count != 0)
                throw new InvalidOperationException(
                    "The Dire Crocodile has no additional primary attack.");
            if (profile.AdditionalSecondaryWeapons.Count != 1 ||
                    profile.AdditionalSecondaryWeapons[0] != "Tail4d8")
                throw new InvalidOperationException(
                    "The printed 4d8 tail slap must be the creature's only " +
                    "secondary attack.");
            if (profile.Facts.Contains("ReducedReach"))
                throw new InvalidOperationException(
                    "A Gargantuan creature's printed Space 20 feet with " +
                    "Reach 15 feet is the standard footprint for its size, " +
                    "so the reduced-reach carrier would shorten it below " +
                    "what is printed.");
            foreach (string fact in new[] { "TripDefenseFourLegs",
                "SkillFocusPerception", "SkillFocusStealth",
                "ImprovedInitiative", "IronWill", "ImprovedCriticalBite" })
                if (!profile.Facts.Contains(fact))
                    throw new InvalidOperationException(
                        "The Dire Crocodile needs its printed " + fact + ".");
        }

        /// <summary>
        /// The Crocodile's accepted identity and placements survive untouched.
        ///
        /// <para>The sprint replaces that creature's borrowed body and gives
        /// it the behaviour its profile currently records as omitted. Neither
        /// of those may allocate a new identity or move a placement, which is
        /// what the order means by preserving it exactly.</para>
        /// </summary>
        internal static void TheCrocodilesIdentityAndPlacementsArePreserved()
        {
            SummonCreatureSpec crocodile = ExpandedSummoningCatalog.All
                .Single(value => value.Key == CrocodileKey);
            if (crocodile.MonsterTier != 3 || crocodile.NaturesAllyTier != 3 ||
                    !crocodile.MonsterTemplated)
                throw new InvalidOperationException(
                    "The Crocodile's accepted tiers and template policy " +
                    "changed.");
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] theirs = all
                .Where(value => value.Creature.Key == CrocodileKey).ToArray();
            // Tiers 3 through 9 in both families.
            if (theirs.Length != 14)
                throw new InvalidOperationException(
                    "The Crocodile has " + theirs.Length +
                    " placements, not the 14 it already shipped.");
            if (!theirs.All(SummonVisibilityCatalog.IsPublished))
                throw new InvalidOperationException(
                    "Every Crocodile placement already publishes and must " +
                    "keep publishing while the sprint replaces its body.");
            // The two creatures share the Monitor Lizard rig, which is the
            // right rig for both and is why neither needs a donor hunt.
            if (ExpandedSummoningDonorCatalog.For(CrocodileKey).Guid !=
                    ExpandedSummoningDonorCatalog.For(DireKey).Guid)
                throw new InvalidOperationException(
                    "Both crocodilians bind the same donor rig.");
        }

        /// <summary>
        /// Every identity the registration needs is in the append-only ledger,
        /// and the ledger grew by exactly the fifteen it requires.
        /// </summary>
        internal static void TheLedgerCarriesTheNewIdentities()
        {
            string path = Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json");
            JToken[] entries = ((JArray)JObject.Parse(
                File.ReadAllText(path))["entries"]).ToArray();
            string[] symbols = entries
                .Select(value => (string)value["symbol"]).ToArray();
            string[] mine = symbols
                .Where(value => value.Contains("DireCrocodile")).ToArray();
            // One unit, three abilities in each family, a Celestial and a
            // Fiendish child for each of the three Monster abilities: 13.
            if (mine.Length != 20)
                throw new InvalidOperationException(
                    "The ledger holds " + mine.Length +
                    " Dire Crocodile identities, not 20: a unit, three " +
                    "abilities in each family, a Celestial and a Fiendish " +
                    "child for each of the three Monster abilities, and the " +
                    "combat traits carrier its riders ride, and the " +
                    "five its Sprint needs - the ability, its " +
                    "one-round speed state, its ten-round recharge, " +
                    "the AI entry and the brain that carries it.");
            foreach (string required in new[] {
                "KMG.Summoning.Unit.DireCrocodile",
                "KMG.Summoning.Ability.SM.Tier7.DireCrocodile.One",
                "KMG.Summoning.Ability.SM.Tier7.DireCrocodile.One.Celestial",
                "KMG.Summoning.Ability.SM.Tier7.DireCrocodile.One.Fiendish",
                "KMG.Summoning.Ability.SNA.Tier9.DireCrocodile.OneD4PlusOne",
                // The printed routine's two weapons, which no native
                // blueprint carries at these dice.
                "KMG.Summoning.Natural.Bite3d6",
                "KMG.Summoning.Natural.Tail4d8",
                // Sprint 16's riders ride one carrier per creature.
                "KMG.Summoning.Special.Crocodile.CombatTraits",
                "KMG.Summoning.Special.DireCrocodile.CombatTraits",
                // Sprint recharges rather than running out, so it
                // needs a cooldown the ability refuses to run under.
                "KMG.Summoning.Special.Crocodile.Sprint",
                "KMG.Summoning.Special.Crocodile.SprintCooldown",
                "KMG.Summoning.Special.DireCrocodile.Sprint",
                "KMG.Summoning.Special.DireCrocodile.SprintCooldown" })
                if (!symbols.Contains(required, StringComparer.Ordinal))
                    throw new InvalidOperationException(
                        "The ledger is missing " + required + ".");
            if (symbols.Distinct(StringComparer.Ordinal).Count() !=
                    symbols.Length)
                throw new InvalidOperationException(
                    "The ledger has a duplicate symbol.");
            string[] guids = entries
                .Select(value => (string)value["guid"]).ToArray();
            if (guids.Distinct(StringComparer.Ordinal).Count() != guids.Length)
                throw new InvalidOperationException(
                    "The ledger has a duplicate GUID.");
        }
    }
}
