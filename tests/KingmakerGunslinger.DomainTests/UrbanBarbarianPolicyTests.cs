using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.UrbanBarbarian;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class UrbanBarbarianPolicyTests
    {
        internal static void AllocationSetsAreCompleteAndExact()
        {
            AssertTier(ControlledRageTier.Ordinary, 6, new[] {
                "4,0,0", "0,4,0", "0,0,4", "2,2,0", "2,0,2", "0,2,2" });
            AssertTier(ControlledRageTier.Greater, 10, new[] {
                "6,0,0", "0,6,0", "0,0,6", "4,2,0", "4,0,2",
                "2,4,0", "0,4,2", "2,0,4", "0,2,4", "2,2,2" });
            AssertTier(ControlledRageTier.Mighty, 15, new[] {
                "8,0,0", "0,8,0", "0,0,8", "6,2,0", "6,0,2",
                "2,6,0", "0,6,2", "2,0,6", "0,2,6", "4,4,0",
                "4,0,4", "0,4,4", "4,2,2", "2,4,2", "2,2,4" });
        }

        internal static void AllocationPresentationAndIdentitiesAreDeterministic()
        {
            IReadOnlyList<ControlledRageAllocation> first =
                ControlledRageAllocationPolicy.Generate(ControlledRageTier.Mighty);
            IReadOnlyList<ControlledRageAllocation> second =
                ControlledRageAllocationPolicy.Generate(ControlledRageTier.Mighty);
            Assertions.Equal(string.Join("|", first.Select(value => value.Symbol)),
                string.Join("|", second.Select(value => value.Symbol)),
                "Allocation symbols or ordering changed between generations.");
            Assertions.Equal("STR +8", first[0].Name,
                "Full-Strength allocation name changed.");
            Assertions.Equal("STR +6 / DEX +2", first[3].Name,
                "Split allocation name changed.");
            Assertions.Equal("KMG.UrbanBarbarian.Allocation.T8.S4.D2.C2",
                first[12].Symbol, "Stable allocation symbol changed.");
            Assertions.True(first.All(value => value.Description.Contains(value.Name)),
                "An allocation description does not name its exact bonuses.");
        }

        internal static void TierSelectionDefaultsVisibilityAndGuardsAreExact()
        {
            Assertions.Equal(ControlledRageTier.Ordinary,
                ControlledRageAllocationPolicy.ResolveTier(false, false),
                "Ordinary tier resolution changed.");
            Assertions.Equal(ControlledRageTier.Greater,
                ControlledRageAllocationPolicy.ResolveTier(true, false),
                "Greater tier resolution changed.");
            Assertions.Equal(ControlledRageTier.Mighty,
                ControlledRageAllocationPolicy.ResolveTier(false, true),
                "Mighty fact must dominate tier resolution.");

            var state = new ControlledRageSelectionState();
            Assertions.Equal("STR +4", state.CurrentSelection.Name,
                "Ordinary tier did not default to full Strength.");
            ControlledRageAllocation dexterity = state.VisibleAllocations[1];
            Assertions.True(state.TrySelect(ControlledRageTier.Ordinary,
                dexterity, false), "A legal inactive ordinary selection was rejected.");
            Assertions.False(state.TrySelect(ControlledRageTier.Ordinary,
                state.VisibleAllocations[0], true),
                "Selection changed while Rage was active.");

            state.Unlock(ControlledRageTier.Greater);
            Assertions.Equal(10, state.VisibleAllocations.Count,
                "Only Greater allocations should be visible at Greater Rage.");
            Assertions.Equal("STR +6", state.CurrentSelection.Name,
                "Greater Rage did not default to full Strength.");
            Assertions.False(state.TrySelect(ControlledRageTier.Ordinary,
                dexterity, false), "A non-current tier selection was accepted.");
            Assertions.Equal("DEX +4",
                state.SelectionFor(ControlledRageTier.Ordinary).Name,
                "Unlocking Greater Rage rewrote the ordinary-tier state.");

            state.Unlock(ControlledRageTier.Mighty);
            Assertions.Equal(15, state.VisibleAllocations.Count,
                "Only Mighty allocations should be visible at Mighty Rage.");
            Assertions.Equal("STR +8", state.CurrentSelection.Name,
                "Mighty Rage did not default to full Strength.");
            Assertions.True(state.TrySelect(ControlledRageTier.Mighty,
                state.VisibleAllocations[12], false),
                "A legal Mighty split was rejected.");
        }

        internal static void TierSelectionPersistenceAndLevelTransitionAreExact()
        {
            var state = new ControlledRageSelectionState();
            state.TrySelect(ControlledRageTier.Ordinary,
                state.VisibleAllocations[4], false);
            state.Unlock(ControlledRageTier.Greater);
            state.TrySelect(ControlledRageTier.Greater,
                state.VisibleAllocations[9], false);
            string persisted = state.Serialize();
            ControlledRageSelectionState loaded =
                ControlledRageSelectionState.Parse(persisted);
            Assertions.Equal(persisted, loaded.Serialize(),
                "Controlled Rage tier state did not round-trip exactly.");
            Assertions.Equal("STR +2 / CON +2",
                loaded.SelectionFor(ControlledRageTier.Ordinary).Name,
                "Ordinary state changed across level transition persistence.");
            Assertions.Equal("STR +2 / DEX +2 / CON +2",
                loaded.SelectionFor(ControlledRageTier.Greater).Name,
                "Greater state changed across persistence.");
            loaded.Unlock(ControlledRageTier.Mighty);
            Assertions.Equal("STR +8", loaded.CurrentSelection.Name,
                "New tier did not receive its independent full-Strength default.");
            Assertions.Throws<FormatException>(() =>
                ControlledRageSelectionState.Parse("4:2,2,0;4:4,0,0"),
                "Duplicate persisted tiers must be rejected.");
            Assertions.Throws<FormatException>(() =>
                ControlledRageSelectionState.Parse("6:6,0,0"),
                "Persisted state without ordinary tier must be rejected.");
        }

        internal static void ConstitutionHitPointReconciliationCannotHealCycle()
        {
            Assertions.Equal(120,
                ControlledRageHitPointPolicy.ReconcileCurrentHitPoints(100, 100, 120),
                "Full-health entry did not preserve a zero damage deficit.");
            Assertions.Equal(85,
                ControlledRageHitPointPolicy.ReconcileCurrentHitPoints(65, 80, 100),
                "Damaged entry did not preserve the damage deficit.");
            Assertions.Equal(65,
                ControlledRageHitPointPolicy.ReconcileCurrentHitPoints(85, 100, 80),
                "Rage exit changed the pre-entry damaged HP.");
            Assertions.Equal(-15,
                ControlledRageHitPointPolicy.ReconcileCurrentHitPoints(5, 100, 80),
                "Low-HP Rage exit did not preserve lethal damage.");
            int hp = 47;
            for (int index = 0; index < 10; index++)
            {
                hp = ControlledRageHitPointPolicy.ReconcileCurrentHitPoints(hp, 80, 100);
                hp = ControlledRageHitPointPolicy.ReconcileCurrentHitPoints(hp, 100, 80);
            }
            Assertions.Equal(47, hp,
                "Repeated Constitution Rage entry/exit created healing.");
        }

        internal static void CrowdControlThresholdFilteringAndAdjacencyAreExact()
        {
            var adjacent = Candidate(5.0);
            Assertions.False(CrowdControlPolicy.Applies(new CrowdControlCandidate[0]),
                "Crowd Control applied with zero enemies.");
            Assertions.False(CrowdControlPolicy.Applies(new[] { adjacent }),
                "Crowd Control applied with one enemy.");
            Assertions.True(CrowdControlPolicy.Applies(new[] { adjacent, Candidate(4.0) }),
                "Crowd Control did not apply with two enemies.");
            Assertions.True(CrowdControlPolicy.Applies(new[] { adjacent,
                Candidate(4.0), Candidate(0.0) }),
                "Crowd Control did not apply with three enemies.");
            Assertions.True(CrowdControlPolicy.IsAdjacentActiveEnemy(Candidate(5.0005)),
                "Native-distance float tolerance rejected adjacency.");
            Assertions.False(CrowdControlPolicy.IsAdjacentActiveEnemy(Candidate(5.01)),
                "Weapon reach or center distance expanded five-foot adjacency.");

            foreach (Action<CrowdControlCandidate> invalidate in new Action<CrowdControlCandidate>[] {
                value => value.IsHostile = false,
                value => value.IsConscious = false,
                value => value.IsDestroyed = true,
                value => value.IsDetached = true,
                value => value.IsTurnedOn = false,
                value => value.IsInGame = false })
            {
                CrowdControlCandidate candidate = Candidate(2.0);
                invalidate(candidate);
                Assertions.False(CrowdControlPolicy.IsAdjacentActiveEnemy(candidate),
                    "An inactive or non-hostile unit counted for Crowd Control.");
            }
            CrowdControlCandidate untargetableSummon = Candidate(3.0);
            untargetableSummon.IsUntargetable = true;
            untargetableSummon.IsSummoned = true;
            Assertions.True(CrowdControlPolicy.IsAdjacentActiveEnemy(untargetableSummon),
                "Active hostile summons or target-restricted enemies were excluded.");
        }

        internal static void CotwCompatibilityNeverDisablesCore()
        {
            foreach (UrbanCotwSurface surface in Enum.GetValues(
                typeof(UrbanCotwSurface)))
            {
                UrbanCotwCompatibilityDecision decision =
                    UrbanCotwCompatibilityPolicy.Evaluate(surface, true, true, false);
                Assertions.True(decision.CoreAvailable,
                    "Optional CotW state disabled the Urban Barbarian core.");
                Assertions.True(decision.InteroperabilityQualified ==
                    (surface == UrbanCotwSurface.Supported),
                    "CotW interoperability status changed.");
            }
            UrbanCotwCompatibilityDecision duplicate =
                UrbanCotwCompatibilityPolicy.Evaluate(
                    UrbanCotwSurface.Supported, true, true, true);
            Assertions.False(duplicate.InteroperabilityQualified,
                "Duplicate CotW marker/action behavior was qualified.");
            Assertions.True(duplicate.CoreAvailable &&
                duplicate.Diagnostic.Contains("core remains available"),
                "Unqualified optional surface did not name core availability.");
        }

        internal static void IdentityManifestIsExactAndCollisionFree()
        {
            IReadOnlyList<UrbanBarbarianIdentitySpec> identities =
                UrbanBarbarianIdentityCatalog.All;
            Assertions.Equal(70, identities.Count,
                "Urban Barbarian identity count changed.");
            Assertions.Equal(70, identities.Select(value => value.Symbol)
                .Distinct(StringComparer.Ordinal).Count(),
                "Urban Barbarian identity symbols collide.");
            Assertions.Equal(70, identities.Select(value => value.Guid)
                .Distinct(StringComparer.Ordinal).Count(),
                "Urban Barbarian identity GUIDs collide.");
            Assertions.True(identities.All(value => value.Guid.Length == 32 &&
                value.Guid.All(character => character >= '0' && character <= '9' ||
                    character >= 'a' && character <= 'f')),
                "An Urban Barbarian identity is not a canonical 32-character GUID.");

            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")));
            var entries = ((JArray)manifest["entries"]).OfType<JObject>()
                .ToDictionary(value => (string)value["symbol"],
                    StringComparer.Ordinal);
            foreach (UrbanBarbarianIdentitySpec identity in identities)
            {
                Assertions.True(entries.ContainsKey(identity.Symbol),
                    "Manifest omits Urban identity " + identity.Symbol + ".");
                JObject entry = entries[identity.Symbol];
                Assertions.Equal(identity.Guid, (string)entry["guid"],
                    "Manifest GUID differs for " + identity.Symbol + ".");
                Assertions.Equal(identity.PlannedType,
                    (string)entry["plannedType"],
                    "Manifest type differs for " + identity.Symbol + ".");
                Assertions.Equal("active", (string)entry["status"],
                    "Urban identity is not active: " + identity.Symbol + ".");
            }
        }

        internal static void BlueprintAndRageSourceContractsAreNarrow()
        {
            string root = Environment.CurrentDirectory;
            string blueprints = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "UrbanBarbarianBlueprints.cs"));
            string rage = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "UrbanBarbarian",
                "ControlledRageRuntime.cs"));
            string crowd = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "UrbanBarbarian",
                "CrowdControlComponent.cs"));
            foreach (string token in new[] {
                "f7d7eb166b3dd594fb330d085df41853",
                "acc15a2d19f13864e8cce3ba133a1979",
                "d294a5dddd0120046aae7d4eb6cbc4fc",
                "2479395977cfeeb46b482bc3385f4647",
                "da8ce41ac3cd74742b80984ccc3c9613",
                "ce49c579fe0bcc647a32c96929fae982",
                "ca9343d75a83a2745a22fa11c383153a",
                "06a7e5b60020ad947aed107d82d1f897",
                "ReplaceClassSkills = true",
                "StatType.SkillKnowledgeWorld",
                "StatType.SkillPersuasion",
                "Entry(1, fastMovement",
                "nativeProficiency)",
                "Entry(11, greaterDefault)",
                "Entry(20, mightyDefault)",
                "ControlledRageAbilityScoreBonus",
                "ForbidSpellCasting",
                "SpellDescriptorComponent" })
                Assertions.True(blueprints.Contains(token),
                    "Urban blueprint source contract is missing: " + token);
            Assertions.False(blueprints.Contains("SkillLoreNature") ||
                blueprints.Contains("retained.Add(Medium"),
                "Urban skills or proficiency reintroduced a forbidden grant.");
            foreach (string token in new[] {
                "ReferenceEquals(attempted, _nativeRage)",
                "collection.Owner.HasFact(_ownerFeature)",
                "BuffCollection), \"AddBuff\"", "new AbilityData(parent,",
                "ModifierDescriptor.Morale",
                "HarmonyAfter(\"CallOfTheWild\")",
                "get_Variants", "get_Name", "Selected -- ",
                "ResolveSelection(Owner, true)" })
                Assertions.True(rage.Contains(token),
                    "Controlled Rage runtime contract is missing: " + token);
            foreach (string token in new[] {
                "IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>",
                "ITargetRulebookHandler<RuleCalculateAC>",
                "candidate.Descriptor.State.IsConscious",
                "owner.IsEnemy(candidate)", "owner.DistanceTo(candidate)",
                "ModifierDescriptor.Dodge", "evt.AddBonus(1, Fact)" })
                Assertions.True(crowd.Contains(token),
                    "Crowd Control runtime contract is missing: " + token);
            Assertions.False(crowd.Contains("GetWeaponRange") ||
                crowd.Contains("Update()") || crowd.Contains("FixedUpdate()"),
                "Crowd Control uses reach or frame polling.");
        }

        private static void AssertTier(ControlledRageTier tier, int expectedCount,
            IEnumerable<string> expectedVectors)
        {
            IReadOnlyList<ControlledRageAllocation> allocations =
                ControlledRageAllocationPolicy.Generate(tier);
            Assertions.Equal(expectedCount, allocations.Count,
                "Allocation count changed for tier " + tier + ".");
            Assertions.Equal(expectedCount, allocations.Distinct().Count(),
                "Duplicate allocation exists for tier " + tier + ".");
            Assertions.True(allocations.All(value => value.Total == (int)tier &&
                value.Strength >= 0 && value.Dexterity >= 0 &&
                value.Constitution >= 0 && value.Strength % 2 == 0 &&
                value.Dexterity % 2 == 0 && value.Constitution % 2 == 0),
                "Allocation values violate the exact tier/+2 contract.");
            string actual = string.Join("|", allocations.Select(Vector));
            string expected = string.Join("|", expectedVectors);
            Assertions.Equal(expected, actual,
                "Allocation vector completeness or deterministic order changed.");
        }

        private static string Vector(ControlledRageAllocation value)
        {
            return value.Strength + "," + value.Dexterity + "," +
                value.Constitution;
        }

        private static CrowdControlCandidate Candidate(double distance)
        {
            return new CrowdControlCandidate { IsInGame = true,
                IsTurnedOn = true, IsConscious = true, IsHostile = true,
                EdgeDistanceFeet = distance };
        }
    }
}
