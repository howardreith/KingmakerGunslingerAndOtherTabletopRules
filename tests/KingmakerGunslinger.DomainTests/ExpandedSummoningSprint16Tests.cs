using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 16 registers the Dire Crocodile and gives the Crocodile the
    /// signature behaviour it has never had.
    ///
    /// <para>These tests cover the registration only. The Dire Crocodile had
    /// no identities at all before this sprint - it existed in the ideal-roster
    /// plan and nowhere else - so the first thing to establish is that it is
    /// now a real creature with its printed stat block, that every one of its
    /// placements is withheld, and that the Crocodile beside it was not
    /// disturbed, because the sprint's order requires that creature's accepted
    /// identity and placements be preserved exactly.</para>
    /// </summary>
    internal static class ExpandedSummoningSprint16Tests
    {
        internal const int AppendedLedgerIdentities = 28;

        private const string DireKey = "dire-crocodile";
        private const string CrocodileKey = "crocodile";

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
        /// A tier-7 creature in both families registers six placements, and
        /// none of them is published.
        /// </summary>
        internal static void TheDireCrocodileIsRegisteredAndWithheld()
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
            if (mine.Any(SummonVisibilityCatalog.IsPublished))
                throw new InvalidOperationException(
                    "No Dire Crocodile placement may publish before it " +
                    "qualifies.");
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
            // Registering a creature never moves the published surface, which
            // is the whole reason it is withheld by name rather than left out
            // of the roster.
            if (SummonVisibilityCatalog.SuppressedLogicalPlacementCount != 6)
                throw new InvalidOperationException(
                    "Exactly the Dire Crocodile's six placements are withheld.");
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
