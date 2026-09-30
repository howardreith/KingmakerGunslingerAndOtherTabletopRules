using System;
using System.IO;
using KingmakerGunslinger.Summoning;

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
