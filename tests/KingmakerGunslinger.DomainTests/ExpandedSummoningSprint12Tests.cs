using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint12Tests
    {
        internal const int AppendedLedgerIdentities = 40;

        internal static void Sprint12FoundationStaysHiddenUntilQualified()
        {
            string catalog = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningCatalog.cs"));
            string visibility = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "SummonVisibilityCatalog.cs"));
            string donors = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningDonorCatalog.cs"));
            string profiles = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningNaturalProfiles.cs"));

            Assertions.True(catalog.Contains(
                    "C(\"dire-rat\",\"Dire Rat\",1,true,1,\"Dog\")") &&
                catalog.Contains("ValidateFamily(SummonFamily.Monster, 80, 453)") &&
                catalog.Contains("ValidateFamily(SummonFamily.NaturesAlly, 78, 447)"),
                "Sprint 12 must register Dire Rat at tier 1 in both families.");
            foreach (string key in new[] {
                "\"dire-rat\"", "\"dog\"", "\"hyena\"", "\"goblin-dog\""
            })
                Assertions.True(visibility.Contains(key),
                    "Unqualified Sprint 12 creature must be suppressed: " + key);
            Assertions.True(visibility.Contains(
                    "RegisteredLogicalPlacementCount = 900") &&
                visibility.Contains("SuppressedLogicalPlacementCount = 68"),
                "Sprint 12 publication boundary must freeze 900/68/832.");
            Assertions.True(donors.Contains(
                    "dire-rat|77f3f2ddf1ec2da45ab956c433e3b557|1") &&
                donors.Contains("dog|77f3f2ddf1ec2da45ab956c433e3b557|1"),
                "Dire Rat and Dog must use the audited native Dog summon rig.");
            foreach (string token in new[] {
                "P(\"dire-rat\", \"Dire Rat\", \"Animal\", 1, \"Small\"",
                "10, 17, 13, 2, 13, 4, 40, 1, \"Bite1d4\"",
                "\"TripDefenseFourLegs\", \"WeaponFinesse\"",
                "\"SkillFocusPerception\"", "printed DC 11 Fortitude save",
                "native Filth Fever payload"
            })
                Assertions.True(profiles.Contains(token),
                    "Dire Rat hidden foundation is missing " + token + ".");

            OriginalQuadrupedVisualsAreDeterministicAndPrivate();
        }

        private static void OriginalQuadrupedVisualsAreDeterministicAndPrivate()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets",
                "sprint12-quadrupeds");
            string[] kinds = { "dire-rat", "hyena", "goblin-dog" };
            string[] meshHashes = {
                "2ee73bcf0ab4cdf0d275fb64764dea96107669cef07ee3b0c62a6f5228f1633a",
                "1c68a8361149309ca374897761406dc98b81f13873a726e9b7d18d7dc1399d0f",
                "0974e179edf83a61067517a5c343ceeae2753cb67e76352c8bff1dce91473a4a"
            };
            string[][] allowedBones = {
                new[] {
                    "Head", "Jaw", "L_Arm_Lower", "L_Arm_Upper", "L_Ear",
                    "L_Finger0", "L_Leg0_Foot", "L_Leg0_Lower",
                    "L_Leg0_Lower1", "L_Leg0_Toe0", "L_Leg0_Upper", "L_Palm",
                    "LowerTorso", "Neck0", "Neck1", "Nose", "Pelvis",
                    "R_Arm_Lower", "R_Arm_Upper", "R_Ear", "R_Finger0",
                    "R_Leg0_Foot", "R_Leg0_Lower", "R_Leg0_Lower1",
                    "R_Leg0_Toe0", "R_Leg0_Upper", "R_Palm", "Tail00",
                    "Tail01", "Tail02", "Tail03", "UpperTorso"
                },
                new[] {
                    "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0",
                    "L_Leg0_Lower", "L_Leg0_Lower2", "L_Leg0_Upper", "L_Palm",
                    "R_Arm_Lower", "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower",
                    "R_Leg0_Lower2", "R_Leg0_Upper", "R_Palm", "Torso_Lower",
                    "Torso_Upper", "ear_L", "ear_R", "front_paw__tip_R",
                    "front_paw_tip_L", "hindpaw_tip_L", "hindpaw_tip_R", "jaw",
                    "jaw_woo_down", "jaw_woo_up", "jaw_woo_up_add", "neck",
                    "spine_0", "tail_01", "tail_02", "tail_03", "tail_04",
                    "withers"
                },
                new[] {
                    "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0",
                    "L_Leg0_Lower", "L_Leg0_Lower2", "L_Leg0_Upper", "L_Palm",
                    "R_Arm_Lower", "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower",
                    "R_Leg0_Lower2", "R_Leg0_Upper", "R_Palm", "Torso_Lower",
                    "Torso_Upper", "ear_L", "ear_R", "front_paw__tip_R",
                    "front_paw_tip_L", "hindpaw_tip_L", "hindpaw_tip_R", "jaw",
                    "jaw_woo_down", "jaw_woo_up", "jaw_woo_up_add", "neck",
                    "spine_0", "tail_01", "tail_02", "tail_03", "tail_04",
                    "withers"
                }
            };
            for (int index = 0; index < kinds.Length; index++)
            {
                string kind = kinds[index];
                string meshPath = Path.Combine(directory, kind + "-mesh.json");
                Assertions.Equal(meshHashes[index], Sha256(meshPath),
                    kind + " mesh matches the reviewed deterministic export.");
                JObject mesh = JObject.Parse(File.ReadAllText(meshPath));
                Assertions.Equal(2, (int)mesh["schemaVersion"],
                    kind + " uses the audited skinned-mesh schema.");
                Assertions.True(((string)mesh["space"]).Contains(
                        "donor renderer local") &&
                    ((string)mesh["rigSha256"]).Length == 64,
                    kind + " records only its private captured-frame hash.");
                string[] bones = ((JArray)mesh["bones"])
                    .Select(value => (string)value).ToArray();
                Assertions.True(bones.SequenceEqual(allowedBones[index]),
                    kind + " binds only the exact reviewed donor controls.");
                int vertices = (int)mesh["vertexCount"];
                int triangles = (int)mesh["triangleCount"];
                Assertions.True(vertices >= 800 && triangles >= 1500 &&
                    Convert.FromBase64String((string)mesh["data"]).Length ==
                    vertices * 64 + triangles * 12,
                    kind + " carries complete original geometry, UVs and weights.");
                JObject albedo = (JObject)mesh["albedo"];
                Assertions.Equal(kind + "-albedo.png", (string)albedo["file"],
                    kind + " names its own painting.");
                Assertions.True((int)albedo["width"] == 1024 &&
                    (int)albedo["height"] == 1024 &&
                    Sha256(Path.Combine(directory, (string)albedo["file"])) ==
                    (string)albedo["sha256"],
                    kind + " painting matches its mesh manifest.");
            }

            string source = Path.Combine(root, "assets-source",
                "original-models", "sprint12-quadrupeds");
            string generator = File.ReadAllText(Path.Combine(source,
                "generate_sprint12_quadrupeds.py"));
            Assertions.True(File.Exists(Path.Combine(source,
                    "paint_sprint12_quadruped_albedo.py")) &&
                File.Exists(Path.Combine(source,
                    "render_sprint12_quadruped_review.py")) &&
                generator.Contains("if len(renderers) != 1") &&
                generator.Contains("donor bind frame is incomplete") &&
                generator.Contains("donor bind frame repeats a bone name") &&
                generator.Contains("wrong donor bind frame for"),
                "Editable source must retain deterministic review and strict private-capture rejection.");

            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets", "PteranodonAssetRuntime.cs"));
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            Assertions.True(project.Contains("assets\\sprint12-quadrupeds\\*-mesh.json") &&
                project.Contains("assets\\sprint12-quadrupeds\\*-albedo.png") &&
                loader.Contains("ConfigureSprint12Quadrupeds(context)") &&
                loader.Contains("AllowedDogBones") &&
                loader.Contains("AllowedWolfWorgBones") &&
                loader.Contains("TryGetSprint12QuadrupedVisual") &&
                view.Contains("Sprint12QuadrupedKeys.Contains(attachment.VisualKey)") &&
                view.Contains("KMG_\" + attachment.VisualKey + \"_Original") &&
                view.Contains("Revert(attachment)") &&
                view.Contains("HandlesBlueprintName") &&
                !view.Contains("\"KMG_Summoning_Unit_Dog\",") &&
                !project.Contains("sprint12-dog-bind-rig.json") &&
                !project.Contains("sprint12-wolf-bind-rig.json") &&
                !project.Contains("sprint12-worg-bind-rig.json"),
                "Three originals use the instance-local swap and native Dog remains an unpackaged donor control.");
            Assertions.True(runtime.Contains("int sprint12VisualChecked = 0;") &&
                runtime.Contains("variant.Creature.Key == \"dire-rat\"") &&
                runtime.Contains("sprint12VisualAttached == sprint12VisualChecked") &&
                runtime.Contains("ungulateVisualChecked + sprint12VisualChecked"),
                "The shared visual-patch lifecycle assertion must account for every hidden Sprint 12 coverage view.");

            string build = File.ReadAllText(Path.Combine(root, "scripts",
                "Build-Local.ps1"));
            string package = File.ReadAllText(Path.Combine(root, "scripts",
                "package.ps1"));
            Assertions.True(build.Contains("assets\\sprint12-quadrupeds") &&
                package.Contains("assets\\sprint12-quadrupeds") &&
                build.Contains("{ 282 } else { 280 }") &&
                package.Contains("{ 282 } else { 280 }"),
                "All six quadruped asset files enter the strict standalone package.");
        }

        private static string Sha256(string path)
        {
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(File.ReadAllBytes(path))
                    .Select(value => value.ToString("x2")));
        }

        internal static void NativeCanidAndRatSurveyStaysMetadataOnly()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            foreach (string token in new[] {
                "\"rat\"", "\"dog\"", "\"hyena\"", "\"worg\"",
                "\"wolf\"", "\"goblin\"", "\"goblinoid\"",
                "\"disease\"", "\"immunity\"",
                "\"allerg\"", "\"filth\"", "\"fever\"",
                "\"DiseaseImmunity\"", "\"AllergicReaction\"",
                "\"GoblinDogAllergicReaction\"", "\"DireRatDisease\"",
                "\"9545a5550d89feb47a84edaeb4e63d0b\"",
                "\"SubtypeGoblinoid\"",
                "units.Add(DescribeNativeUnit(unit))",
                "native-donor-audit.json"
            })
                Assertions.True(source.Contains(token),
                    "Sprint 12 native metadata survey is missing " + token + ".");
            Assertions.False(source.Contains("AssetBundle.LoadFromFile") ||
                source.Contains("Texture2D.EncodeToPNG"),
                "The donor survey must not extract or export native game art.");

            string record = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "planning",
                "EXPANDED-SUMMONING-SPRINT12-RULES-AND-DONORS.md"));
            foreach (string token in new[] {
                "77f3f2ddf1ec2da45ab956c433e3b557",
                "9545a5550d89feb47a84edaeb4e63d0b",
                "d524df24b2f38cf4590525b2e7c4f34e",
                "No broader native Goblinoid",
                "subtype fact exists",
                "No individual Dire Rat exists",
                "all 1,954 domain tests",
                "20260930T0823186905486Z"
            })
                Assertions.True(record.Contains(token),
                    "Sprint 12 audit record is missing " + token + ".");
        }

        internal static void StirgeRosterTextRecordsPrimaryCadenceException()
        {
            string tool = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "tools",
                "expanded_summoning_manifest.py"));
            string roster = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "planning",
                "EXPANDED-SUMMONING-ROSTER.md"));
            foreach (string text in new[] { tool, roster })
            {
                Assertions.True(text.Contains(
                        "one disclosed 10% Filth Fever exposure check per victim") &&
                    text.Contains("primary Paizo stat block requires"),
                    "Generated and checked-in roster text must record the " +
                    "primary-source once-per-Stirge/victim exception.");
                Assertions.False(text.Contains(
                    "each successful blood drain rolls"),
                    "Roster text must not claim a per-drain Stirge disease roll.");
            }
        }

        internal static void InjuryDiseasePolicyRequiresExactPositiveDamage()
        {
            Assertions.Equal(11, SummonInjuryDiseasePolicy.DireRatFortitudeDc,
                "Dire Rat keeps its printed initial disease DC.");
            Assertions.Equal(12, SummonInjuryDiseasePolicy.GoblinDogFortitudeDc,
                "Goblin Dog keeps its printed allergic-reaction DC.");
            Assertions.Equal(86400,
                SummonInjuryDiseasePolicy.GoblinDogAllergyDurationSeconds,
                "Goblin Dog allergic reaction lasts one day.");
            Assertions.Equal(-2,
                SummonInjuryDiseasePolicy.GoblinDogAllergyAbilityPenalty,
                "Goblin Dog reaction applies the printed ability penalty.");
            Assertions.True(SummonInjuryDiseasePolicy.ShouldResolve(
                    true, true, 1, true, false),
                "An exact damaging bite against an available eligible target resolves.");
            foreach (bool result in new[] {
                SummonInjuryDiseasePolicy.ShouldResolve(false, true, 1, true, false),
                SummonInjuryDiseasePolicy.ShouldResolve(true, false, 1, true, false),
                SummonInjuryDiseasePolicy.ShouldResolve(true, true, 0, true, false),
                SummonInjuryDiseasePolicy.ShouldResolve(true, true, 1, false, false),
                SummonInjuryDiseasePolicy.ShouldResolve(true, true, 1, true, true) })
                Assertions.False(result,
                    "Misses, zero damage, unavailable targets, other weapons, and exact Goblin targets cannot resolve the bite rider.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                SummonInjuryDiseasePolicy.ShouldResolve(
                    true, true, -1, true, false),
                "Negative actual damage is invalid evidence.");
        }

        internal static void AllergicReactionRemovalRequiresPositiveMagic()
        {
            Assertions.True(
                SummonInjuryDiseasePolicy.ShouldRemoveAllergicReaction(
                    1, true, false, false) &&
                SummonInjuryDiseasePolicy.ShouldRemoveAllergicReaction(
                    1, false, true, false) &&
                SummonInjuryDiseasePolicy.ShouldRemoveAllergicReaction(
                    1, false, false, true),
                "Positive spell, spell-like, and supernatural healing remove the reaction.");
            Assertions.False(
                SummonInjuryDiseasePolicy.ShouldRemoveAllergicReaction(
                    0, true, false, false) ||
                SummonInjuryDiseasePolicy.ShouldRemoveAllergicReaction(
                    1, false, false, false),
                "Zero healing and nonmagical healing leave the reaction active.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                SummonInjuryDiseasePolicy.ShouldRemoveAllergicReaction(
                    -1, true, false, false),
                "Negative actual healing is invalid evidence.");
        }

        internal static void DiseaseBlueprintsStayHiddenAndDisclosed()
        {
            string root = Environment.CurrentDirectory;
            string builder = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSprint12CombatComponents.cs"));
            string abilities = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningAbilityBuilder.cs"));
            string live = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningSprint12.cs"));
            foreach (string token in new[] {
                "KMG.Summoning.Natural.DireRat.Disease",
                "9545a5550d89feb47a84edaeb4e63d0b",
                "KMG.Summoning.Natural.GoblinDog.Traits",
                "KMG.Summoning.Natural.GoblinDog.AllergicReaction",
                "d524df24b2f38cf4590525b2e7c4f34e",
                "BuffDescriptorImmunity", "SpellDescriptor.Disease",
                "StackingType.Replace", "StatType.Dexterity",
                "StatType.Charisma", "ModifierDescriptor.Penalty" })
                Assertions.True(builder.Contains(token),
                    "Sprint 12 disease blueprint graph is missing " + token + ".");
            foreach (string token in new[] {
                "RuleInitiatorLogicComponent<RuleAttackWithWeapon>",
                "evt.AttackRoll.IsHit", "evt.MeleeDamage.Damage",
                "ConditionalWeakTable<RuleAttackWithWeapon",
                "RuleSavingThrow", "RuleApplyBuff",
                "RuleTargetLogicComponent<RuleHealDamage>",
                "AbilityType.Spell", "AbilityType.SpellLike",
                "AbilityType.Supernatural" })
                Assertions.True(runtime.Contains(token),
                    "Sprint 12 disease runtime is missing " + token + ".");
            Assertions.True(abilities.Contains(
                    "no broader Goblinoid subtype") &&
                abilities.Contains("exact native Goblin unit type") &&
                abilities.Contains("positive magical healing or remove disease"),
                "Goblin Dog summon tooltips must disclose the bounded native-unit-type adaptation and removal routes.");
            foreach (string token in new[] {
                "9545a5550d89feb47a84edaeb4e63d0b",
                "d524df24b2f38cf4590525b2e7c4f34e",
                "FindNativeD20Seed(1)", "FindNativeD20Seed(20)",
                "RuleHealDamage", "CureLightWounds", "RemoveDisease",
                "SummonMultiplicity.OneD4PlusOne",
                "goblinVictimBlueprint.Type = goblinType",
                "The exact native Goblin-type fixture did not spawn.",
                "if (!secondVictim.Destroyed) secondVictim.Destroy()",
                "if (!goblinVictim.Destroyed) goblinVictim.Destroy()",
                "ReferenceEquals(firstRatDisease.Context.MaybeCaster, rats[0])",
                "ReferenceEquals(firstDogReaction.Context.MaybeCaster, dogs[0])",
                "CaptureSprint12DonorRig(direRat, \"dog\"",
                "CaptureSprint12DonorRig(hyena, \"wolf\"",
                "CaptureSprint12DonorRig(goblinDog, \"worg\"",
                "renderer-local bind frame",
                "sprint12-\" + donorKey + \"-bind-rig.json" })
                Assertions.True(live.Contains(token),
                    "Sprint 12 guarded runtime matrix is missing " + token + ".");
        }
    }
}
