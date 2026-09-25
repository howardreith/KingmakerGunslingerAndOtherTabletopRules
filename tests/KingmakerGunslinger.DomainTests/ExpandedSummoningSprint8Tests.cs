using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Phase 1 Sprint 8 - Big-Cat Roster Completion. The Tiger joins Summon
    /// Nature's Ally IV as a Large animal on the leopard rig with a procedural
    /// striped coat and the grab-and-rake pack; the Cheetah gains a procedural
    /// spotted coat and a bounded once-per-summoning sprint.
    /// </summary>
    internal static class ExpandedSummoningSprint8Tests
    {
        /// <summary>
        /// Identities Sprint 8 appended to the frozen ledger: the tiger unit,
        /// its six Nature's Ally placements, the project 1d8 claw and seven
        /// specials (the tiger's carrier, the cheetah's sprint pack).
        /// </summary>
        internal const int AppendedLedgerIdentities = 15;

        internal static void TigerIsANewNaturesAllyFourOption()
        {
            ExpandedSummoningCatalog.Validate();
            SummonCreatureSpec tiger = ExpandedSummoningCatalog.All.Single(value => value.Key == "tiger");
            Assertions.Equal(null, tiger.MonsterTier, "The Tiger is not a Summon Monster creature.");
            Assertions.Equal(4, tiger.NaturesAllyTier, "The Tiger is Summon Nature's Ally IV.");
            Assertions.False(tiger.MonsterTemplated, "The Tiger takes no template.");
            IdealRosterEntry entry = ExpandedSummoningIdealRosterCatalog.Find("tiger");
            Assertions.True(entry != null && entry.Sprint == 8 && entry.NaturesAllyTier == 4,
                "The ideal roster owns the Tiger under Sprint 8 at Nature's Ally IV.");
            SummonVariantSpec[] variants = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                .Where(value => value.Creature.Key == "tiger").OrderBy(value => value.ParentTier).ToArray();
            Assertions.Equal(6, variants.Length, "The Tiger reaches every Nature's Ally parent from IV to IX.");
            Assertions.Equal(SummonMultiplicity.One, variants[0].Multiplicity, "One tiger at IV.");
            Assertions.Equal(SummonMultiplicity.OneD3, variants[1].Multiplicity, "1d3 tigers at V.");
            Assertions.True(variants.Skip(2).All(value => value.Multiplicity == SummonMultiplicity.OneD4PlusOne),
                "1d4+1 tigers from VI to IX.");
            Assertions.Equal(0, ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Count(value => value.Creature.Key == "tiger"), "No Summon Monster tiger.");
            Assertions.Equal("768275c9885dd954fb3c84ba69ac4281", ExpandedSummoningDonorCatalog.For("tiger").Guid,
                "The Tiger rides the leopard rig.");
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles.For("tiger");
            Assertions.True(profile.HitDieClass == "Animal" && profile.HitDice == 6 && profile.Size == "Large" &&
                profile.Strength == 23 && profile.Dexterity == 15 && profile.Constitution == 17 &&
                profile.Intelligence == 2 && profile.Wisdom == 12 && profile.Charisma == 6 &&
                profile.SpeedFeet == 40 && profile.NaturalArmor == 3,
                "The Tiger's chassis follows the tabletop stat block.");
            Assertions.Equal("BiteLarge2d6", profile.PrimaryWeapon, "The Tiger bites for 2d6.");
            Assertions.True(profile.AdditionalWeapons.SequenceEqual(new[] { "Claw1d8", "Claw1d8", "Claw1d8", "Claw1d8" }),
                "The Tiger carries two 1d8 claws and two 1d8 rake claws.");
            Assertions.True(profile.Facts.Contains("Pounce") && profile.Facts.Contains("WeaponFocusClaw"),
                "The Tiger pounces and focuses its claws.");
            Assertions.True(profile.Deviations.Any(value => value.Contains("charge-only rake component")) &&
                profile.Deviations.Any(value => value.Contains("shared summon grapple lifecycle (Sprint 8)")) &&
                profile.Deviations.Any(value => value.Contains("no native tiger exists")),
                "The Tiger's deviations record the rake, the grab and the visual.");
            float scale;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier("KMG_Summoning_Unit_Tiger", out scale) &&
                scale == 1.25f, "The Tiger reads Large on the leopard rig.");
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier("KMG_Summoning_Unit_Cheetah", out scale) &&
                scale == 0.92f, "The Cheetah reads lean on the leopard rig.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(1, identities.Count(value => value.Symbol == "KMG.Summoning.Natural.Claw1d8" &&
                value.PlannedType == "BlueprintItemWeapon"), "The project 1d8 claw identity exists.");
            Assertions.Equal(1, identities.Count(value => value.Symbol == "KMG.Summoning.Special.Tiger.CombatTraits" &&
                value.PlannedType == "BlueprintBuff"), "The Tiger's carrier identity exists.");
            Assertions.Equal(SummonProjectIconScope.KmgCatalog, SummonIconCatalog.For("tiger").Scope,
                "The Tiger has its own project icon.");
        }

        internal static void CheetahSprintIsBounded()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal(1, ExpandedSummoningSpecialProfiles.CheetahSprintUses, "One sprint per summoning.");
            Assertions.Equal(1, ExpandedSummoningSpecialProfiles.CheetahSprintRounds, "A sprint lasts one round.");
            Assertions.Equal(30, ExpandedSummoningSpecialProfiles.CheetahSprintBonusFeet, "A sprint adds 30 feet.");
            NaturalSummonProfile cheetah = ExpandedSummoningNaturalProfiles.For("cheetah");
            Assertions.True(cheetah.Facts.Contains("TrippingBite"), "The Cheetah keeps its native trip bite.");
            Assertions.True(cheetah.Deviations.Any(value => value.Contains("never repeatable within one summoning")),
                "The Cheetah's deviation records the bounded sprint.");
            Assertions.False(cheetah.Deviations.Any(value => value.Contains("omitted conservatively")),
                "The old sprint omission is retired.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string[] pair in new[] {
                new[] { "KMG.Summoning.Special.Cheetah.Sprint", "BlueprintAbility" },
                new[] { "KMG.Summoning.Special.Cheetah.SprintResource", "BlueprintAbilityResource" },
                new[] { "KMG.Summoning.Special.Cheetah.SprintState", "BlueprintBuff" },
                new[] { "KMG.Summoning.Special.Cheetah.SprintAi", "BlueprintAiCastSpell" },
                new[] { "KMG.Summoning.Special.Cheetah.Brain", "BlueprintBrain" },
                new[] { "KMG.Summoning.Special.Cheetah.CombatTraits", "BlueprintBuff" } })
                Assertions.Equal(1, identities.Count(value => value.Symbol == pair[0] &&
                    value.PlannedType == pair[1]), "Sprint identity missing: " + pair[0]);
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            foreach (string token in new[] {
                "ConfigureCheetahSprint(bySymbol)", "BuffMovementSpeed", "ModifierDescriptor.Enhancement",
                "CheetahSprintBonusFeet", "UnitCommand.CommandType.Swift", "CheetahSprintUses",
                "ConfigureCatWithWeapon(library, bySymbol, TigerUnitSymbol" })
                Assertions.True(builder.Contains(token), "Sprint 8 builder contract is missing: " + token);
        }

        internal static void ProceduralCoatsAreBounded()
        {
            SummonCoatProfile tiger = ExpandedSummoningSpecialProfiles.TigerCoat;
            SummonCoatProfile cheetah = ExpandedSummoningSpecialProfiles.CheetahCoat;
            Assertions.True(tiger.IsBounded && tiger.Pattern == SummonCoatPattern.Stripes && tiger.Key == "tiger",
                "The Tiger's coat is bounded stripes.");
            Assertions.True(cheetah.IsBounded && cheetah.Pattern == SummonCoatPattern.Spots && cheetah.Key == "cheetah",
                "The Cheetah's coat is bounded spots.");
            Assertions.True(tiger.BaseRed > tiger.BaseGreen && tiger.BaseGreen > tiger.BaseBlue &&
                tiger.MarkRed < 0.2f && tiger.MarkGreen < 0.2f && tiger.MarkBlue < 0.2f,
                "Orange base, dark stripes.");
            Assertions.True(cheetah.BaseRed > cheetah.BaseBlue && cheetah.MarkRed < 0.25f,
                "Tan base, dark spots.");
            string patch = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning", "ExpandedSummoningVisualVariantPatch.cs"));
            foreach (string token in new[] {
                "class SummonCoatRasterizer", "mesh.vertices", "mesh.uv", "mesh.triangles",
                "material.SetTexture(MainTextureSlot, coat)", "material.SetColor(slot, Color.white)",
                "variant:coat-not-applied", "SummonCoatPattern.Stripes", "CoatTextureSize = 512" })
                Assertions.True(patch.Contains(token), "Coat contract is missing: " + token);
            Assertions.False(patch.Contains("GetPixels") || patch.Contains("ReadPixels") || patch.Contains("GetTexture(MainTextureSlot)"),
                "The coat never reads the game's own texture pixels.");
        }

        internal static void LedgerAndIconsCoverTheTiger()
        {
            SummonIconCatalog.Validate();
            Assertions.Equal(91, SummonIconCatalog.All.Count, "Sprint 8 adds the Tiger icon.");
            string ledger = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json"));
            var entries = Newtonsoft.Json.Linq.JObject.Parse(ledger)["entries"]
                .Select(value => (string)value["symbol"]).ToArray();
            string[] appended = entries.Where(value =>
                value == "KMG.Summoning.Unit.Tiger" || value.Contains(".Tiger.") ||
                value == "KMG.Summoning.Natural.Claw1d8" ||
                value.StartsWith("KMG.Summoning.Special.Cheetah.", StringComparison.Ordinal)).ToArray();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 8 must append exactly its own identities to the ledger.");
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities)
                .All(value => appended.Contains(value)),
                "The ledger is append-only: Sprint 8 identities sit at its tail.");
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities)
                .Take(ExpandedSummoningSprint7Tests.AppendedLedgerIdentities)
                .All(value => !appended.Contains(value)),
                "The Sprint 7 append stays exactly before the Sprint 8 append.");
            foreach (string symbol in new[] { "KMG.Summoning.Unit.Tiger",
                "KMG.Summoning.Ability.SNA.Tier4.Tiger.One", "KMG.Summoning.Ability.SNA.Tier9.Tiger.OneD4PlusOne",
                "KMG.Summoning.Special.Cheetah.SprintState" })
                Assertions.True(ledger.Contains("\"symbol\": \"" + symbol + "\""),
                    "The append-only ledger must carry " + symbol);
        }
    }
}
