using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Phase 1 Sprint 3 - Native Publication Pack I. Pony, Horse, Owlbear and
    /// Cyclops join the project-owned catalog on native donors; the Frost
    /// Giant's retained native unit is reused under Summon Nature's Ally
    /// VII-IX wrappers; Flash of Insight is bounded.
    /// </summary>
    internal static class ExpandedSummoningSprint3Tests
    {
        private static readonly string[] Keys = { "pony", "horse", "owlbear", "cyclops" };

        /// <summary>
        /// Identities Sprint 3 appended to the frozen ledger: four units, 45
        /// logical placements, 34 template executions, three Nature's Ally
        /// Frost Giant wrappers and six Cyclops specials. Other modules' ledger
        /// totals add this on top of their own preserved prefix.
        /// </summary>
        internal const int AppendedLedgerIdentities = 92;

        internal static void PlacementsMatchTheCharterAndPropagate()
        {
            ExpandedSummoningCatalog.Validate();
            SummonCreatureSpec pony = Find("pony");
            SummonCreatureSpec horse = Find("horse");
            SummonCreatureSpec owlbear = Find("owlbear");
            SummonCreatureSpec cyclops = Find("cyclops");
            Assertions.Equal(1, pony.MonsterTier, "Pony is Summon Monster I.");
            Assertions.Equal(1, pony.NaturesAllyTier, "Pony is Summon Nature's Ally I.");
            Assertions.True(pony.MonsterTemplated, "Pony takes the celestial/fiendish template.");
            Assertions.Equal(2, horse.MonsterTier, "Horse is Summon Monster II.");
            Assertions.Equal(2, horse.NaturesAllyTier, "Horse is Summon Nature's Ally II.");
            Assertions.True(horse.MonsterTemplated, "Horse takes the celestial/fiendish template.");
            Assertions.Equal(null, owlbear.MonsterTier, "Owlbear is not a Summon Monster creature.");
            Assertions.Equal(4, owlbear.NaturesAllyTier, "Owlbear is Summon Nature's Ally IV.");
            Assertions.Equal(null, cyclops.MonsterTier, "Cyclops is not a Summon Monster creature.");
            Assertions.Equal(5, cyclops.NaturesAllyTier, "Cyclops is Summon Nature's Ally V.");
            foreach (string key in Keys)
            {
                Assertions.Equal(key == "pony" ? "Pony" : key == "horse" ? "Horse" :
                    key == "owlbear" ? "Owlbear" : "Cyclops", Find(key).Visual,
                    "A Sprint 3 creature must display under its own name: " + key);
                IdealRosterEntry entry = ExpandedSummoningIdealRosterCatalog.Find(key);
                Assertions.True(entry != null && entry.Sprint == 3,
                    "The ideal roster must own the creature under Sprint 3: " + key);
                Assertions.Equal(entry.MonsterTier, Find(key).MonsterTier,
                    "Shipped Monster tier must match the plan: " + key);
                Assertions.Equal(entry.NaturesAllyTier, Find(key).NaturesAllyTier,
                    "Shipped Ally tier must match the plan: " + key);
            }

            // Quantity propagation: own tier single, next 1d3, then 1d4+1,
            // for every parent up to IX, in both families.
            AssertPropagation("pony", SummonFamily.Monster, 1);
            AssertPropagation("pony", SummonFamily.NaturesAlly, 1);
            AssertPropagation("horse", SummonFamily.Monster, 2);
            AssertPropagation("horse", SummonFamily.NaturesAlly, 2);
            AssertPropagation("owlbear", SummonFamily.NaturesAlly, 4);
            AssertPropagation("cyclops", SummonFamily.NaturesAlly, 5);
            Assertions.Equal(0, ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Count(value => value.Creature.Key == "owlbear" ||
                    value.Creature.Key == "cyclops"),
                "Owlbear and Cyclops must not appear under Summon Monster.");

            // Every new placement is published; nothing new is suppressed.
            int published = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => Keys.Contains(value.Creature.Key))
                .Count(SummonVisibilityCatalog.IsPublished);
            Assertions.Equal(45, published, "Sprint 3 adds 45 published logical placements.");
            foreach (string key in Keys)
                Assertions.Equal(SummonFamilyCoverage.Published,
                    ExpandedSummoningCoveragePolicy.Coverage(key, SummonFamily.NaturesAlly,
                        Find(key).NaturesAllyTier),
                    "Coverage must derive Published for " + key);
        }

        internal static void DonorsAndProfilesAreExact()
        {
            ExpandedSummoningDonorCatalog.Validate();
            ExpandedSummoningNaturalProfiles.Validate();
            Assertions.Equal("3f95557fc806db741b500a5735990841",
                ExpandedSummoningDonorCatalog.For("pony").Guid, "Pony uses the native summoned pony.");
            Assertions.True(ExpandedSummoningDonorCatalog.For("pony").DedicatedSummon,
                "The native summoned pony is a dedicated summon donor.");
            Assertions.Equal("5bb9579fdb2b26b48bb10d61c81cfdfb",
                ExpandedSummoningDonorCatalog.For("horse").Guid, "Horse uses the native summoned horse.");
            Assertions.True(ExpandedSummoningDonorCatalog.For("horse").DedicatedSummon,
                "The native summoned horse is a dedicated summon donor.");
            Assertions.Equal("d6e0acbdbdb56114898922063ae2cba0",
                ExpandedSummoningDonorCatalog.For("owlbear").Guid, "Owlbear uses the standard owlbear body.");
            Assertions.False(ExpandedSummoningDonorCatalog.For("owlbear").DedicatedSummon,
                "The campaign owlbear is a body donor that must be sanitized.");
            Assertions.Equal("124f1c45ef24d654e9cd420fe84f7f36",
                ExpandedSummoningDonorCatalog.For("cyclops").Guid, "Cyclops uses the standard cyclops body.");
            Assertions.False(ExpandedSummoningDonorCatalog.For("cyclops").DedicatedSummon,
                "The campaign cyclops is a body donor that must be sanitized.");

            NaturalSummonProfile pony = ExpandedSummoningNaturalProfiles.For("pony");
            AssertScores(pony, "Animal", 2, "Medium", 13, 13, 14, 2, 11, 4, 40, 0);
            Assertions.Equal("Hoof1d3", pony.PrimaryWeapon, "Pony hoof changed.");
            Assertions.True(pony.AdditionalWeapons.SequenceEqual(new[] { "Hoof1d3" }),
                "Pony carries two 1d3 hooves.");
            NaturalSummonProfile horse = ExpandedSummoningNaturalProfiles.For("horse");
            AssertScores(horse, "Animal", 2, "Large", 16, 14, 17, 2, 13, 7, 50, 0);
            Assertions.Equal("Hoof1d4", horse.PrimaryWeapon, "Horse hoof changed.");
            Assertions.True(horse.AdditionalWeapons.SequenceEqual(new[] { "Hoof1d4" }),
                "Horse carries two 1d4 hooves.");
            Assertions.True(horse.Facts.Contains("ReducedReach"),
                "A Large horse fights at 5-foot reach.");
            NaturalSummonProfile owlbear = ExpandedSummoningNaturalProfiles.For("owlbear");
            AssertScores(owlbear, "MagicalBeast", 5, "Large", 19, 12, 18, 2, 12, 10, 30, 5);
            Assertions.Equal("Bite1d6", owlbear.PrimaryWeapon, "Owlbear bite changed.");
            Assertions.True(owlbear.AdditionalWeapons.SequenceEqual(new[] { "Claw1d6", "Claw1d6" }),
                "Owlbear carries two 1d6 claws.");
            Assertions.True(owlbear.Facts.Contains("ImprovedInitiative") &&
                owlbear.Facts.Contains("GreatFortitude") &&
                owlbear.Facts.Contains("SkillFocusPerception") &&
                owlbear.Facts.Contains("ReducedReach"),
                "Owlbear feats or reach changed.");
            Assertions.True(owlbear.Deviations.Any(value => value.Contains("Sprint 4")),
                "Owlbear grab must be recorded as deferred to the shared grapple lifecycle.");
            NaturalSummonProfile cyclops = ExpandedSummoningNaturalProfiles.For("cyclops");
            AssertScores(cyclops, "Humanoid", 10, "Large", 21, 8, 15, 10, 13, 8, 30, 7);
            Assertions.Equal("Greataxe", cyclops.PrimaryWeapon, "Cyclops greataxe changed.");
            Assertions.Equal(0, cyclops.AdditionalWeapons.Count, "Cyclops has no natural limbs.");
            Assertions.True(cyclops.Facts.Contains("Ferocity") &&
                cyclops.Facts.Contains("PowerAttack") && cyclops.Facts.Contains("Cleave"),
                "Cyclops ferocity or feats changed.");
            Assertions.True(cyclops.Deviations.Any(value => value.Contains("Flash of Insight")) &&
                cyclops.Deviations.Any(value => value.Contains("hide armor")),
                "Cyclops deviations must record the bounded insight and the carried hide armor.");
            Assertions.True(ExpandedSummoningNaturalProfiles.SupportedHitDieClasses
                .Take(4).SequenceEqual(new[] { "Animal", "Vermin", "MagicalBeast", "Humanoid" }),
                "The Sprint 3 hit-die classes changed.");

            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            foreach (string token in new[] {
                "b9e97f47cb86f2d45a0784a096ff8037", "6ab4526f94d2e3e439af0599a29b6675",
                "085547b82eded104ba7e1870dd0563bf", "b0e472a49ff2a294f93faa3ab757a4a5",
                "6efea466862f014469cec6c3f2b85cb7", "7661741dbb9604842a642457456fd0e4",
                "e73864391ccf0894997928443a29d755", "d809b6c4ff2aaff4fa70d712a70f7d7b",
                "case \"MagicalBeast\"", "case \"Humanoid\"" })
                Assertions.True(builder.Contains(token),
                    "Sprint 3 natural builder contract is missing: " + token);
        }

        internal static void FrostGiantNaturesAllyWrappersReuseTheUnit()
        {
            SummonNativeExpansionCatalog.Validate();
            SummonNativeExpansionSpec[] wrappers = SummonNativeExpansionCatalog.All
                .Where(value => value.Family == SummonFamily.NaturesAlly &&
                    value.CreatureKey == "FrostGiant").ToArray();
            Assertions.Equal(3, wrappers.Length, "Frost Giant has three Nature's Ally wrappers.");
            Assertions.True(wrappers.All(value => value.ReplacesSpawnUnit),
                "Each Nature's Ally Frost Giant wrapper replaces the umbrella's spawn unit.");
            Assertions.True(wrappers.All(value => value.UnitGuid ==
                    "590cd3d5e76fdc649a5f97bc984cd3c4" && value.IconKey == "frost-giant" &&
                    value.Branch == SummonNativeSpawnBranch.Direct),
                "Wrappers must point at the retained native unit with the shared icon.");
            Assertions.True(wrappers.Any(value => value.Tier == 7 &&
                    value.Multiplicity == SummonMultiplicity.One &&
                    value.SourceAbilityGuid == "6d8d59aa38713be4fa3be76c19107cc0") &&
                wrappers.Any(value => value.Tier == 8 &&
                    value.Multiplicity == SummonMultiplicity.OneD3 &&
                    value.SourceAbilityGuid == "256739c1e61e3f64eaf71734d271f4be") &&
                wrappers.Any(value => value.Tier == 9 &&
                    value.Multiplicity == SummonMultiplicity.OneD4PlusOne &&
                    value.SourceAbilityGuid == "9bd8cb6180842f44e9302c58e47b91f0"),
                "Wrapper tiers, quantities or source umbrellas changed.");
            foreach (SummonNativeExpansionSpec wrapper in wrappers)
            {
                SummonNativeOptionSpec source = SummonNativeOptionCatalog.Find(
                    SummonFamily.NaturesAlly, wrapper.Tier, wrapper.SourceAbilityGuid);
                Assertions.True(source != null && source.IsSemanticDuplicate &&
                    source.EquivalentCreatureKey == "mastodon" &&
                    source.Multiplicity == wrapper.Multiplicity,
                    "Each source must be the native Mastodon option the publisher already reconciles.");
                Assertions.True(SummonNativeExpansionCatalog.Replaces(SummonFamily.NaturesAlly,
                    wrapper.Tier, wrapper.SourceAbilityGuid),
                    "The umbrella must be suppressed in favour of the creature-named option.");
            }
            Assertions.False(SummonNativeExpansionCatalog.All.Any(value =>
                    value.Family == SummonFamily.Monster && value.ReplacesSpawnUnit),
                "Spawn replacement is a Nature's Ally device only.");
            Assertions.Equal(11, ExpandedSummoningCoveragePolicy.NativeWrapperCreatures.Count,
                "Reusing the Frost Giant must not add a wrapper creature.");
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNativeOptionBuilder.cs"));
            Assertions.True(builder.Contains("spec.ReplacesSpawnUnit") &&
                builder.Contains("spawns.Length != 1") &&
                builder.Contains("chosen.Blueprint = BlueprintLibraryLookup.RequireExact<"),
                "The native option builder must replace exactly one foreign direct spawn.");
        }

        internal static void FlashOfInsightIsBounded()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal(1, ExpandedSummoningSpecialProfiles.CyclopsFlashOfInsightUses,
                "Flash of Insight is one use per summoning.");
            Assertions.True(ExpandedSummoningSpecialProfiles.CyclopsFlashOfInsightLastsUntilUsed,
                "The armed state has no duration of its own; the next attack roll ends it.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldApplyFlashOfInsight(true, true),
                "The owner's attack roll while armed is converted.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldApplyFlashOfInsight(false, true),
                "A roll the cyclops merely witnesses is untouched.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldApplyFlashOfInsight(true, false),
                "Nothing happens without the armed state.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string symbol in new[] {
                "KMG.Summoning.Special.Cyclops.FlashOfInsight",
                "KMG.Summoning.Special.Cyclops.FlashOfInsightState",
                "KMG.Summoning.Special.Cyclops.FlashOfInsightResource",
                "KMG.Summoning.Special.Cyclops.CombatTraits",
                "KMG.Summoning.Special.Cyclops.FlashOfInsightAi",
                "KMG.Summoning.Special.Cyclops.Brain" })
                Assertions.Equal(1, identities.Count(value => value.Symbol == symbol),
                    "Cyclops special identity missing or duplicated: " + symbol);
            string components = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs"));
            // Correction order: the armed attack's own d20 is chosen as a
            // natural 20 through the game's pre-rolled-result seam, so the hit
            // and the threat follow from the roll and the critical confirmation
            // is rolled normally; no automatic-hit flag is set.
            Assertions.True(components.Contains("class CyclopsFlashOfInsightComponent") &&
                components.Contains("RuleInitiatorLogicComponent<RuleAttackRoll>") &&
                components.Contains("IInitiatorRulebookHandler<RuleRollD20>") &&
                components.Contains("\"m_PreRolledResult\"") &&
                components.Contains("ChosenResult = 20") &&
                !components.Contains("evt.AutoCriticalConfirmation = true;"),
                "The armed state chooses the attack's own d20 as a natural 20 and leaves the confirmation to the dice.");
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(builder.Contains("RemoveBuffOnAttack") &&
                builder.Contains("UnitCommand.CommandType.Swift") &&
                builder.Contains("AbilityType.Supernatural") &&
                builder.Contains("CyclopsFlashOfInsightUses") &&
                builder.Contains("arm.Permanent = ExpandedSummoningSpecialProfiles.CyclopsFlashOfInsightLastsUntilUsed;"),
                "The ability must be a swift supernatural power whose state has no duration of its own and ends after one attack.");
        }

        internal static void LedgerAndIconsCoverTheNewCreatures()
        {
            SummonIconCatalog.Validate();
            foreach (string key in Keys)
                Assertions.Equal(SummonProjectIconScope.KmgCatalog,
                    SummonIconCatalog.For(key).Scope, "Sprint 3 creature icon scope: " + key);
            string ledger = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json"));
            var entries = Newtonsoft.Json.Linq.JObject.Parse(ledger)["entries"]
                .Select(value => (string)value["symbol"]).ToArray();
            string[] appended = entries.Where(value =>
                (value.Contains(".Pony") || value.Contains(".Horse") ||
                value.Contains(".Owlbear") || value.Contains(".Cyclops") ||
                value.Contains(".SNA.Tier7.FrostGiant") ||
                value.Contains(".SNA.Tier8.FrostGiant") ||
                value.Contains(".SNA.Tier9.FrostGiant")) &&
                !value.StartsWith("KMG.Summoning.Special.Owlbear.", StringComparison.Ordinal) &&
                !ExpandedSummoningCorrectionTests.IsCorrectionIdentity(value))
                .ToArray();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 3 must append exactly its own identities to the ledger.");
            // Append-only: the Sprint 3 block sits directly before the Sprint 4
            // block at the ledger's tail.
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint4Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint5Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint6Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities -
                    (ExpandedSummoningSprint8Tests.AppendedLedgerIdentities + ExpandedSummoningCorrectionTests.AppendedLedgerIdentities))
                .Take(AppendedLedgerIdentities)
                .All(value => appended.Contains(value)),
                "The ledger is append-only: Sprint 3 identities sit directly before Sprint 4's.");
            foreach (string symbol in new[] {
                "KMG.Summoning.Unit.Pony", "KMG.Summoning.Unit.Horse",
                "KMG.Summoning.Unit.Owlbear", "KMG.Summoning.Unit.Cyclops",
                "KMG.Summoning.Ability.SM.Tier9.Pony.OneD4PlusOne.Fiendish",
                "KMG.Summoning.Ability.SNA.Tier5.Cyclops.One",
                "KMG.Summoning.NativeOption.SNA.Tier7.FrostGiant.One",
                "KMG.Summoning.NativeOption.SNA.Tier9.FrostGiant.OneD4PlusOne",
                "KMG.Summoning.Special.Cyclops.FlashOfInsight" })
                Assertions.True(ledger.Contains("\"symbol\": \"" + symbol + "\""),
                    "The append-only ledger must carry " + symbol);
        }

        private static SummonCreatureSpec Find(string key)
        { return ExpandedSummoningCatalog.All.Single(value => value.Key == key); }

        private static void AssertPropagation(string key, SummonFamily family, int tier)
        {
            SummonVariantSpec[] variants = ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => value.Creature.Key == key).OrderBy(value => value.ParentTier)
                .ToArray();
            Assertions.Equal(10 - tier, variants.Length,
                key + " must reach every parent from its tier to IX in " + family);
            for (int index = 0; index < variants.Length; index++)
            {
                int parent = tier + index;
                Assertions.Equal(parent, variants[index].ParentTier, "Parent order for " + key);
                Assertions.Equal(parent == tier ? SummonMultiplicity.One :
                    parent == tier + 1 ? SummonMultiplicity.OneD3 :
                    SummonMultiplicity.OneD4PlusOne, variants[index].Multiplicity,
                    "Quantity at parent " + parent + " for " + key);
            }
        }

        private static void AssertScores(NaturalSummonProfile profile, string hitDieClass,
            int hitDice, string size, int strength, int dexterity, int constitution,
            int intelligence, int wisdom, int charisma, int speed, int naturalArmor)
        {
            Assertions.Equal(hitDieClass, profile.HitDieClass, profile.Key + " hit-die class");
            Assertions.Equal(hitDice, profile.HitDice, profile.Key + " HD");
            Assertions.Equal(size, profile.Size, profile.Key + " size");
            Assertions.Equal(strength, profile.Strength, profile.Key + " Str");
            Assertions.Equal(dexterity, profile.Dexterity, profile.Key + " Dex");
            Assertions.Equal(constitution, profile.Constitution, profile.Key + " Con");
            Assertions.Equal(intelligence, profile.Intelligence, profile.Key + " Int");
            Assertions.Equal(wisdom, profile.Wisdom, profile.Key + " Wis");
            Assertions.Equal(charisma, profile.Charisma, profile.Key + " Cha");
            Assertions.Equal(speed, profile.SpeedFeet, profile.Key + " speed");
            Assertions.Equal(naturalArmor, profile.NaturalArmor, profile.Key + " natural armor");
        }
    }
}
