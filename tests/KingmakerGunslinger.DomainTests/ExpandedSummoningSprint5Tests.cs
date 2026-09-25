using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Phase 1 Sprint 5 - Mephit Family Expansion. Dust, Ice, Magma, Ooze,
    /// Salt and Steam Mephits join the catalog at Summon Monster IV and Summon
    /// Nature's Ally IV on the native summoned mephit bodies; each has its own
    /// name, icon, tint, breath, spell-like abilities and brain; every area
    /// effect reaches only enemies; there is no Lightning Mephit.
    /// </summary>
    internal static class ExpandedSummoningSprint5Tests
    {
        private static readonly string[] Keys = { "dust-mephit", "ice-mephit", "magma-mephit",
            "ooze-mephit", "salt-mephit", "steam-mephit" };

        /// <summary>
        /// Identities Sprint 5 appended to the frozen ledger: six units,
        /// seventy-two logical placements and forty-eight mephit specials.
        /// </summary>
        internal const int AppendedLedgerIdentities = 126;

        internal static void PlacementsMatchTheCharterAndPropagate()
        {
            ExpandedSummoningCatalog.Validate();
            foreach (string key in Keys)
            {
                SummonCreatureSpec creature = Find(key);
                Assertions.Equal(4, creature.MonsterTier, "Mephits are Summon Monster IV: " + key);
                Assertions.Equal(4, creature.NaturesAllyTier, "Mephits are Summon Nature's Ally IV: " + key);
                Assertions.False(creature.MonsterTemplated, "Mephits take no template: " + key);
                IdealRosterEntry entry = ExpandedSummoningIdealRosterCatalog.Find(key);
                Assertions.True(entry != null && entry.Sprint == 5,
                    "The ideal roster must own the creature under Sprint 5: " + key);
                Assertions.Equal(entry.MonsterTier, creature.MonsterTier,
                    "Shipped Monster tier must match the plan: " + key);
                Assertions.Equal(entry.NaturesAllyTier, creature.NaturesAllyTier,
                    "Shipped Ally tier must match the plan: " + key);
                AssertPropagation(key, SummonFamily.Monster, 4);
                AssertPropagation(key, SummonFamily.NaturesAlly, 4);
            }
            Assertions.False(ExpandedSummoningCatalog.All.Any(value =>
                    value.Key.Contains("lightning") || value.DisplayName.Contains("Lightning")),
                "No third-party Lightning Mephit joins the catalog.");
            int published = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => Keys.Contains(value.Creature.Key))
                .Count(SummonVisibilityCatalog.IsPublished);
            Assertions.Equal(72, published, "Sprint 5 adds seventy-two published logical placements.");
            foreach (string key in Keys)
                foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
                    Assertions.Equal(SummonFamilyCoverage.Published,
                        ExpandedSummoningCoveragePolicy.Coverage(key, family, 4),
                        "Coverage must derive Published for " + key + " under " + family);
        }

        internal static void DonorsAndProfilesAreExact()
        {
            ExpandedSummoningDonorCatalog.Validate();
            ExpandedSummoningSpecialProfiles.Validate();
            foreach (string[] row in new[] {
                new[] { "dust-mephit", "50782bc4eb36aac4287023e20ee00808", "air-mephit" },
                new[] { "ice-mephit", "4615328295cd7e84bb2ef09d3dba8403", "water-mephit" },
                new[] { "magma-mephit", "10a820de0a417f345866f794324205ad", "fire-mephit" },
                new[] { "ooze-mephit", "4615328295cd7e84bb2ef09d3dba8403", "water-mephit" },
                new[] { "salt-mephit", "50782bc4eb36aac4287023e20ee00808", "air-mephit" },
                new[] { "steam-mephit", "4615328295cd7e84bb2ef09d3dba8403", "water-mephit" } })
            {
                Assertions.Equal(row[1], ExpandedSummoningDonorCatalog.For(row[0]).Guid,
                    row[0] + " must ride its nearest native summoned mephit.");
                Assertions.True(ExpandedSummoningDonorCatalog.For(row[0]).DedicatedSummon,
                    "The native summoned mephits are dedicated summon donors: " + row[0]);
                Assertions.Equal(row[1], ExpandedSummoningDonorCatalog.For(row[2]).Guid,
                    "The variant and its native counterpart share one body: " + row[0]);
                Assertions.Equal(row[2], ExpandedSummoningSpecialProfiles.MephitVariant(row[0]).DonorKey,
                    "Profile donor key must name the shared body: " + row[0]);
            }
            Assertions.Equal(6, ExpandedSummoningSpecialProfiles.MephitVariants.Length,
                "Six mephit variants.");
            AssertBreath("dust-mephit", "Slashing", 1, 4, true, "Blur", null);
            AssertBreath("ice-mephit", "Cold", 1, 4, true, "MagicMissile", null);
            AssertBreath("magma-mephit", "Fire", 1, 8, false, null, null);
            AssertBreath("ooze-mephit", "Acid", 1, 4, true, "AcidArrow", "StinkingCloud");
            AssertBreath("salt-mephit", "Slashing", 1, 4, true, "Glitterdust", "Dehydrate");
            AssertBreath("steam-mephit", "Fire", 1, 4, true, "Blur", "BoilingRain");
            Assertions.Equal(3, ExpandedSummoningSpecialProfiles.MephitSickenedRounds,
                "The breath rider sickens for three rounds.");
            Assertions.Equal(1, ExpandedSummoningSpecialProfiles.MephitSpellLikeUses,
                "Each spell-like ability is one use per summoning.");
            Assertions.Equal(4, ExpandedSummoningSpecialProfiles.MephitBreathAiCooldownRounds,
                "The breath AI action rests four rounds between breaths.");
            Assertions.Equal(20, ExpandedSummoningSpecialProfiles.MephitBurstRadiusFeet,
                "Dehydrate and boiling rain are 20-foot bursts.");
            Assertions.True(ExpandedSummoningSpecialProfiles.DehydrateDice == 2 &&
                ExpandedSummoningSpecialProfiles.DehydrateDieSides == 8 &&
                ExpandedSummoningSpecialProfiles.BoilingRainDice == 2 &&
                ExpandedSummoningSpecialProfiles.BoilingRainDieSides == 6,
                "Dehydrate is 2d8 and boiling rain 2d6.");
            Assertions.Equal(6, ExpandedSummoningSpecialProfiles.MephitVisualTints.Length,
                "Six visual tints.");
            foreach (SummonVisualTintProfile tint in ExpandedSummoningSpecialProfiles.MephitVisualTints)
                Assertions.True(tint.IsBounded && Keys.Contains(tint.Key), "Tint out of bounds: " + tint.Key);
            Assertions.True(ExpandedSummoningSpecialProfiles.MephitVisualTint("magma-mephit").HasEmission &&
                ExpandedSummoningSpecialProfiles.MephitVisualTint("steam-mephit").HasEmission &&
                !ExpandedSummoningSpecialProfiles.MephitVisualTint("salt-mephit").HasEmission,
                "Magma and steam glow; the others only tint.");
            Assertions.True(ExpandedSummoningSpecialProfiles.MephitVisualTints
                .Select(value => value.TintRed + "/" + value.TintGreen + "/" + value.TintBlue)
                .Distinct().Count() == 6, "Every mephit tint is distinct.");
        }

        internal static void MephitPackIsBounded()
        {
            var identities = ExpandedSummoningIdentityCatalog.Build();
            int specials = 0;
            foreach (MephitVariantProfile profile in ExpandedSummoningSpecialProfiles.MephitVariants)
            {
                string token = string.Concat(profile.Key.Split('-').Select(part =>
                    char.ToUpperInvariant(part[0]) + part.Substring(1)).ToArray());
                string prefix = "KMG.Summoning.Special." + token + ".";
                foreach (string[] pair in new[] {
                    new[] { "Breath", "BlueprintAbility" }, new[] { "BreathAi", "BlueprintAiCastSpell" },
                    new[] { "Brain", "BlueprintBrain" }, new[] { "CombatTraits", "BlueprintBuff" } })
                {
                    Assertions.Equal(1, identities.Count(value => value.Symbol == prefix + pair[0] &&
                        value.PlannedType == pair[1]), "Mephit special identity missing: " + prefix + pair[0]);
                    specials++;
                }
                foreach (string slot in new[] { profile.SpellLikeOne == null ? null : "SpellLikeOne",
                    profile.SpellLikeTwo == null ? null : "SpellLikeTwo" }.Where(value => value != null))
                {
                    Assertions.Equal(1, identities.Count(value => value.Symbol == prefix + slot &&
                        value.PlannedType == "BlueprintAbility"), "Spell-like identity missing: " + prefix + slot);
                    Assertions.Equal(1, identities.Count(value => value.Symbol == prefix + slot + "Resource" &&
                        value.PlannedType == "BlueprintAbilityResource"), "Resource identity missing: " + prefix + slot);
                    Assertions.Equal(1, identities.Count(value => value.Symbol == prefix + slot + "Ai" &&
                        value.PlannedType == "BlueprintAiCastSpell"), "AI identity missing: " + prefix + slot);
                    specials += 3;
                }
            }
            Assertions.Equal(48, specials, "Sprint 5 adds forty-eight mephit specials.");
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            foreach (string token in new[] {
                "ConfigureMephitVariants(library, bySymbol)", "MephitDonorElementFactGuids",
                "4e42460798665fd4cb9173ffa7ada323", "ContextConditionIsEnemy",
                "TargetType.Enemy", "!(value is SpellListComponent)", "AbilityType.SpellLike",
                "SavingThrowType.Fortitude", "StatType.Constitution",
                "5af8b717a209fd444a1e4d077ed776f0", "f6544caac8fe528489327cd86a84b025",
                "6dfc5e4c7d9ae3048984744222dbd0fa", "1f08438786937954aaa6022c7f5ad286",
                "ab0616beb567c2c4d8d3f7447a01a0c8", "a54cd27999a5e8340976f3a40edfef3a",
                "b8bbe8f713da9ad44a899aa551ca6b5b", "8e934134fec60ab4c8972c85a7b62f89",
                "e147258e5b7c40643893d80c9f2816e8", "bf7ee56ec9e43c14fa17727997e91993",
                "ExpandedSummoningVisualVariantPatch.Register(" })
                Assertions.True(builder.Contains(token), "Mephit builder contract is missing: " + token);
            Assertions.False(builder.Contains("LightningMephit") || builder.Contains("Lightning Mephit"),
                "No Lightning Mephit anywhere in the builder.");
            string patch = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning", "ExpandedSummoningVisualVariantPatch.cs"));
            foreach (string token in new[] {
                "[HarmonyPatch(typeof(UnitEntityView), \"OnDataAttached\")]",
                "class SummonVisualVariant", "new Material(original)",
                "renderer.sharedMaterials = replacements", "variant:no-renderer",
                "variant:no-colour-slot", "variant:applied", "ConditionalWeakTable" })
                Assertions.True(patch.Contains(token), "Visual variant patch contract is missing: " + token);
            Assertions.False(patch.Contains("original.SetColor") || patch.Contains("sharedMaterial.SetColor"),
                "The shared donor material is never written; only the private clone is.");
            string project = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            Assertions.True(project.Contains("Summoning\\ExpandedSummoningVisualVariantPatch.cs"),
                "The visual variant patch must be compiled into the mod.");
            string icons = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningIconBuilder.cs"));
            Assertions.True(icons.Contains("\"Dehydrate\" || slot.Value == \"BoilingRain\""),
                "The project bursts wear their mephit's icon.");
        }

        internal static void LedgerAndIconsCoverTheNewCreatures()
        {
            SummonIconCatalog.Validate();
            foreach (string key in Keys)
                Assertions.Equal(SummonProjectIconScope.KmgCatalog,
                    SummonIconCatalog.For(key).Scope, "Sprint 5 creature icon scope: " + key);
            string ledger = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json"));
            var entries = Newtonsoft.Json.Linq.JObject.Parse(ledger)["entries"]
                .Select(value => (string)value["symbol"]).ToArray();
            string[] appended = entries.Where(value =>
                value.Contains(".DustMephit") || value.Contains(".IceMephit") ||
                value.Contains(".MagmaMephit") || value.Contains(".OozeMephit") ||
                value.Contains(".SaltMephit") || value.Contains(".SteamMephit")).ToArray();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 5 must append exactly its own identities to the ledger.");
            // Append-only: the Sprint 5 block sits directly before the Sprint 6
            // block at the ledger's tail, and directly after the Sprint 4 block.
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint6Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities)
                .Take(AppendedLedgerIdentities)
                .All(value => appended.Contains(value)),
                "The ledger is append-only: Sprint 5 identities sit directly before Sprint 6's.");
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint6Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint4Tests.AppendedLedgerIdentities)
                .Take(ExpandedSummoningSprint4Tests.AppendedLedgerIdentities)
                .All(value => !appended.Contains(value)),
                "The Sprint 4 append stays exactly before the Sprint 5 append.");
            foreach (string symbol in new[] {
                "KMG.Summoning.Unit.DustMephit", "KMG.Summoning.Unit.SteamMephit",
                "KMG.Summoning.Ability.SM.Tier4.DustMephit.One",
                "KMG.Summoning.Ability.SM.Tier9.SaltMephit.OneD4PlusOne",
                "KMG.Summoning.Ability.SNA.Tier4.IceMephit.One",
                "KMG.Summoning.Ability.SNA.Tier5.MagmaMephit.OneD3",
                "KMG.Summoning.Special.OozeMephit.Breath",
                "KMG.Summoning.Special.SaltMephit.SpellLikeTwoResource",
                "KMG.Summoning.Special.SteamMephit.Brain" })
                Assertions.True(ledger.Contains("\"symbol\": \"" + symbol + "\""),
                    "The append-only ledger must carry " + symbol);
        }

        private static SummonCreatureSpec Find(string key)
        { return ExpandedSummoningCatalog.All.Single(value => value.Key == key); }

        private static void AssertBreath(string key, string energy, int dice, int sides,
            bool sickens, string one, string two)
        {
            MephitVariantProfile profile = ExpandedSummoningSpecialProfiles.MephitVariant(key);
            Assertions.Equal(energy, profile.BreathEnergy, key + " breath energy");
            Assertions.Equal(dice, profile.BreathDice, key + " breath dice");
            Assertions.Equal(sides, profile.BreathDieSides, key + " breath die");
            Assertions.Equal(sickens, profile.BreathSickens, key + " breath sickens");
            Assertions.Equal(one, profile.SpellLikeOne, key + " first spell-like ability");
            Assertions.Equal(two, profile.SpellLikeTwo, key + " second spell-like ability");
        }

        private static void AssertPropagation(string key, SummonFamily family, int tier)
        {
            SummonVariantSpec[] variants = ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => value.Creature.Key == key).OrderBy(value => value.ParentTier)
                .ToArray();
            Assertions.Equal(10 - tier, variants.Length,
                key + " must reach every " + family + " parent from its tier to IX");
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
    }
}
