using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 15's two creatures: the Giant Ant (Drone) and the Giant Stag
    /// Beetle, both on the Giant Spider rig the Sprint 14 insects proved.
    ///
    /// <para>The rig family being proven is not the same as these creatures
    /// being qualified, so both are registered and withheld until their own
    /// gates pass. The Drone has a second reason to be withheld: it prints
    /// scent, like the castes it is built from, and Kingmaker has no scent
    /// mechanic at all.</para>
    /// </summary>
    internal static class ExpandedSummoningSprint15Tests
    {
        /// <summary>
        /// Thirty-five identities: two units and their 30 logical placements
        /// with the celestial and fiendish children the Drone's Summon Monster
        /// side needs, plus three mechanical ones - the Drone's grab traits
        /// carrier, the Giant Stag Beetle's trample and its unit type.
        ///
        /// <para>Three is a small mechanical footprint for two creatures, and
        /// deliberately so. The Drone needs no poison graph of its own because
        /// the Giant Ant poison derives its difficulty class live from the
        /// caster's own Constitution, and no unit type because it is a third
        /// caste of one creature. The Giant Stag Beetle needs no trample graph
        /// because the ungulates' carrier derives damage and save from hit dice
        /// and Strength, which produces its printed line exactly.</para>
        /// </summary>
        internal const int AppendedLedgerIdentities = 35;

        private const string DroneKey = "giant-ant-drone";
        private const string StagBeetleKey = "giant-stag-beetle";

        /// <summary>
        /// Both creatures are registered, and neither is published.
        /// </summary>
        internal static void BothCreaturesAreRegisteredAndWithheld()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            foreach (var expected in new[] {
                new { Key = DroneKey, Placements = 12 },
                new { Key = StagBeetleKey, Placements = 6 } })
            {
                SummonVariantSpec[] mine = all
                    .Where(value => value.Creature.Key == expected.Key)
                    .ToArray();
                if (mine.Length != expected.Placements)
                    throw new InvalidOperationException(expected.Key +
                        " registers " + mine.Length + " placements, not " +
                        expected.Placements + ".");
                if (mine.Any(SummonVisibilityCatalog.IsPublished))
                    throw new InvalidOperationException(
                        "No Sprint 15 placement may be published before it " +
                        "qualifies: " + expected.Key);
            }
            // Registering a creature never moves the published surface. That
            // is the whole reason a creature is withheld by name rather than
            // by being left out of the roster.
            if (SummonVisibilityCatalog.PublishedLogicalPlacementCount != 922)
                throw new InvalidOperationException(
                    "Registering Sprint 15 moved the published surface to " +
                    SummonVisibilityCatalog.PublishedLogicalPlacementCount +
                    "; it must stay at the 922 Sprint 14 left it at.");
            if (SummonVisibilityCatalog.RegisteredLogicalPlacementCount != 970)
                throw new InvalidOperationException(
                    "The registered surface must be 970.");
        }

        /// <summary>
        /// The Drone is the soldier with the advanced simple template, and
        /// every derived number is written down rather than assumed.
        ///
        /// <para>The frozen contract requires exactly that, because a template
        /// applied at load is a number nobody can read in review. It checks
        /// against the soldier it is derived from, so a change to either
        /// creature that breaks the relationship fails here rather than in a
        /// guarded run.</para>
        /// </summary>
        internal static void TheDroneIsTheSoldierWithTheAdvancedTemplate()
        {
            NaturalSummonProfile soldier = ExpandedSummoningNaturalProfiles
                .For("giant-ant-soldier");
            NaturalSummonProfile drone = ExpandedSummoningNaturalProfiles
                .For(DroneKey);
            foreach (var pair in new[] {
                new { Name = "Strength", From = soldier.Strength,
                    To = drone.Strength },
                new { Name = "Dexterity", From = soldier.Dexterity,
                    To = drone.Dexterity },
                new { Name = "Constitution", From = soldier.Constitution,
                    To = drone.Constitution },
                new { Name = "Wisdom", From = soldier.Wisdom,
                    To = drone.Wisdom },
                new { Name = "Charisma", From = soldier.Charisma,
                    To = drone.Charisma } })
                if (pair.To != pair.From + 4)
                    throw new InvalidOperationException("The Drone's " +
                        pair.Name + " must be the soldier's " + pair.From +
                        " plus the advanced template's 4, not " + pair.To +
                        ".");
            // Intelligence is the one score the template excludes.
            if (drone.Intelligence != soldier.Intelligence)
                throw new InvalidOperationException(
                    "The advanced simple template excludes Intelligence.");
            if (drone.NaturalArmor != soldier.NaturalArmor + 2)
                throw new InvalidOperationException(
                    "The Drone's natural armour must be the soldier's plus 2.");
            if (drone.HitDice != soldier.HitDice)
                throw new InvalidOperationException(
                    "The advanced simple template adds no hit dice.");
            // It keeps the soldier's sting and poison, and flies.
            if (!drone.AdditionalWeapons.Contains("AntSting1d4") ||
                !drone.Facts.Contains("GiantAntPoison") ||
                !drone.Facts.Contains("Airborne"))
                throw new InvalidOperationException(
                    "The Drone keeps the soldier's sting and poison and adds " +
                    "flight.");
            // Perception is the advanced Wisdom plus the exact racial bonus and
            // nothing else. The contract's "+2 to all skills" clause is not
            // part of this template and is deliberately not implemented; an
            // unsourced bonus would make the creature stronger than printed.
            if (drone.Skills.Count != 0)
                throw new InvalidOperationException(
                    "The Drone's stat block prints no skill ranks.");
            int derived = (drone.Wisdom - 10) / 2 +
                NaturalSummonProfile.GiantAntRacialPerceptionBonus;
            if (derived != 7)
                throw new InvalidOperationException(
                    "The Drone's Perception must derive to +7, not " + derived +
                    ".");
        }

        /// <summary>
        /// The Giant Stag Beetle's trample is derived, not stated, and the
        /// derivation lands on its printed line.
        /// </summary>
        internal static void TheStagBeetleTrampleDerivesToItsPrintedLine()
        {
            NaturalSummonProfile beetle = ExpandedSummoningNaturalProfiles
                .For(StagBeetleKey);
            UngulateRulesProfile rules = UngulateRulesPolicy.For(StagBeetleKey);
            if (rules.HitDice != beetle.HitDice ||
                rules.Strength != beetle.Strength)
                throw new InvalidOperationException(
                    "The trample row must describe the same creature as the " +
                    "profile: hit dice " + rules.HitDice + " against " +
                    beetle.HitDice + ", Strength " + rules.Strength +
                    " against " + beetle.Strength + ".");
            if (!rules.HasTrample || rules.TrampleDiceCount != 1 ||
                rules.TrampleDieSides != 6 || rules.TrampleBonus != 6)
                throw new InvalidOperationException(
                    "The printed trample is 1d6+6; the policy derives " +
                    rules.TrampleDiceCount + "d" + rules.TrampleDieSides +
                    "+" + rules.TrampleBonus + ".");
            if (rules.TrampleDc != 17 || rules.TrampleSaveDc(0) != 17)
                throw new InvalidOperationException(
                    "The printed trample save is DC 17; the policy derives " +
                    rules.TrampleDc + ".");
            // Stampede belongs to the herd ungulates alone, and a beetle that
            // gained it would trample creatures of its own size.
            if (rules.Stampede || rules.HasPowerfulCharge)
                throw new InvalidOperationException(
                    "The Giant Stag Beetle has neither Stampede nor a " +
                    "powerful charge.");
            if (!rules.CanTrample(3, 2, 0) || rules.CanTrample(3, 3, 0))
                throw new InvalidOperationException(
                    "A Large trampler takes targets smaller than itself and " +
                    "no others.");
        }

        /// <summary>
        /// The printed stat block, on the numbers a reader can check against
        /// it without running the game.
        /// </summary>
        internal static void TheStagBeetleMatchesItsPrintedStatBlock()
        {
            NaturalSummonProfile beetle = ExpandedSummoningNaturalProfiles
                .For(StagBeetleKey);
            if (beetle.Size != "Large" || beetle.HitDieClass != "Vermin" ||
                beetle.HitDice != 7)
                throw new InvalidOperationException(
                    "The Giant Stag Beetle is a Large vermin of 7 hit dice.");
            if (beetle.Strength != 19 || beetle.Dexterity != 10 ||
                beetle.Constitution != 15 || beetle.Wisdom != 10 ||
                beetle.Charisma != 9)
                throw new InvalidOperationException(
                    "The Giant Stag Beetle's printed ability scores changed.");
            // AC 17 is 10 plus 8 natural less 1 for size.
            if (beetle.NaturalArmor != 8)
                throw new InvalidOperationException(
                    "The printed AC 17 is 10 plus 8 natural armour less 1 for " +
                    "Large size.");
            if (beetle.PrimaryWeapon != "Bite2d8" ||
                beetle.AdditionalWeapons.Count != 0)
                throw new InvalidOperationException(
                    "Its printed routine is one bite for 2d8 and nothing else.");
            // Space 10 ft. with Reach 5 ft. is a reduced reach for a Large
            // creature, which the Large ungulates already carry.
            if (!beetle.Facts.Contains("ReducedReach"))
                throw new InvalidOperationException(
                    "The printed Space 10 ft./Reach 5 ft. needs the reduced " +
                    "reach carrier.");
            // CMD 20, 28 against trip: the eight-legs defence is +8.
            if (!beetle.Facts.Contains("TripDefenseEightLegs"))
                throw new InvalidOperationException(
                    "The printed CMD 28 against trip needs the multi-legged " +
                    "trip defence.");
            // Perception +0 on Wisdom 10 means no ranks and no racial bonus.
            if (beetle.Skills.Count != 0)
                throw new InvalidOperationException(
                    "A printed Perception of +0 on Wisdom 10 leaves no room " +
                    "for ranks.");
            if (beetle.Facts.Contains("Airborne"))
                throw new InvalidOperationException(
                    "The equal 20-foot poor fly speed is omitted in favour of " +
                    "the ground mode its trample needs; see the profile's " +
                    "recorded deviation.");
        }
    }
}
