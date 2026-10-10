using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 21: the Giant Crab, and what this sprint must not do to the
    /// released Bebelith.
    ///
    /// <para>These are the offline checks. Every one of them is something a
    /// guarded run would otherwise have had to discover, and three of them
    /// exist because Sprint 20 discovered their equivalents the expensive
    /// way: a printed rider with no carrier, a printed bonus that existed
    /// only in its own derivation, and a review scenario the mod's own
    /// allowlist refused after a deploy and a launch.</para>
    /// </summary>
    internal static class Sprint21RulesTests
    {
        internal const int NaturesAllyRoots = 7;
        internal const int AppendedLedgerIdentities = 1 + 7 + 5;

        /// <summary>
        /// The printed profile derives, all sixteen identities of it.
        ///
        /// <para>This creature's block closed completely at intake, which the
        /// Sprint 20 scorpion's did not - so unlike that one there is no
        /// number here recorded as a question against the book, and this test
        /// is the whole stat block rather than a sample of it.</para>
        /// </summary>
        internal static void ThePrintedProfileDerives()
        {
            GiantCrabRulesPolicy.Validate();
            Assertions.Equal(2, GiantCrabRulesPolicy.StrengthModifier,
                "Strength 15 gives +2.");
            Assertions.Equal(19, GiantCrabRulesPolicy.PrintedHitPoints,
                "13 racial plus 6 from Constitution.");
            Assertions.Equal(15, GiantCrabRulesPolicy.PrintedArmorClass,
                "10 plus 4 natural plus 1 Dexterity, with no size term.");
            Assertions.Equal(4, GiantCrabRulesPolicy.PrintedAttackBonus,
                "Base attack 2 plus Strength 2.");
            Assertions.Equal(8, GiantCrabRulesPolicy.PrintedGrappleBonus,
                "The printed manoeuvre bonus plus the grab bonus.");
            Assertions.Equal(12,
                GiantCrabRulesPolicy.PrintedCombatManeuverDefenseVersusTrip -
                GiantCrabRulesPolicy.PrintedCombatManeuverDefense,
                "Four per pair of legs beyond the first, three pairs.");
            Assertions.Equal(4, GiantCrabRulesPolicy.PrintedPerceptionSkill,
                "Wisdom 0 plus a racial 4, with no ranks.");
            Assertions.Equal(10, GiantCrabRulesPolicy.PrintedSwimSkill,
                "Strength 2 plus a racial 8, recorded and unrepresented.");
        }

        /// <summary>
        /// It buys no skill ranks, because it has no Intelligence score.
        /// </summary>
        internal static void ItBuysNoSkillRanks()
        {
            Assertions.Equal(0, GiantCrabRulesPolicy.SkillRanks,
                "A creature with no Intelligence score buys none.");
            NaturalSummonProfile crab = ExpandedSummoningNaturalProfiles.For(
                GiantCrabRulesPolicy.GiantCrabKey);
            Assertions.Equal(0, crab.Skills.Count,
                "An empty skill list is the point of this entry, not an "
                + "oversight: the default three ranks would put the creature "
                + "above its stat block.");
            Assertions.Equal(GiantCrabRulesPolicy.SubstitutedIntelligence,
                crab.Intelligence,
                "An absent Intelligence score ships as 1.");
        }

        /// <summary>
        /// Registered and withheld, at seven roots on one table.
        /// </summary>
        internal static void TheCreatureIsRegisteredAndWithheld()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] mine = all.Where(value =>
                value.Creature.Key == GiantCrabRulesPolicy.GiantCrabKey)
                .ToArray();
            Assertions.Equal(NaturesAllyRoots, mine.Length,
                "Seven roots: parents 3 through 9 on one table.");
            Assertions.Equal(NaturesAllyRoots, mine.Count(value =>
                value.Family == SummonFamily.NaturesAlly),
                "Every one of them on the Nature's Ally side.");
            Assertions.Equal(0, mine.Count(value =>
                value.Family == SummonFamily.Monster),
                "It is not on the Summon Monster table at all, which is why "
                + "it owns no execution children.");
            Assertions.Equal(7,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "All seven are withheld.");
            Assertions.Equal(1063,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Registered placements rise from 1056 to 1063.");
            Assertions.Equal(1056,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The published surface is exactly what v0.0.149 showed.");
            foreach (SummonVariantSpec variant in all)
                Assertions.Equal(
                    variant.Creature.Key != GiantCrabRulesPolicy.GiantCrabKey,
                    SummonVisibilityCatalog.IsPublished(variant),
                    "Exactly the Sprint 21 creature is withheld: "
                    + variant.StableKey);
        }

        /// <summary>
        /// The quantity propagation, which is the charter's and not this
        /// creature's: single at its own tier, 1d3 at the next, 1d4+1 after.
        /// </summary>
        internal static void TheQuantityPropagationFollowsTheCharter()
        {
            SummonVariantSpec[] mine = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly)
                .Where(value =>
                    value.Creature.Key == GiantCrabRulesPolicy.GiantCrabKey)
                .OrderBy(value => value.ParentTier).ToArray();
            Assertions.Equal(7, mine.Length, "Parents 3 through 9.");
            Assertions.Equal(3, mine[0].ParentTier, "Its own tier is 3.");
            Assertions.Equal(SummonMultiplicity.One, mine[0].Multiplicity,
                "A single crab at its own tier.");
            Assertions.Equal(SummonMultiplicity.OneD3, mine[1].Multiplicity,
                "1d3 one tier up.");
            foreach (SummonVariantSpec variant in mine.Skip(2))
                Assertions.Equal(SummonMultiplicity.OneD4PlusOne,
                    variant.Multiplicity,
                    "1d4+1 from two tiers up: " + variant.StableKey);
        }

        /// <summary>
        /// The append is exactly thirteen entries at the end of the ledger,
        /// all of them this creature's, and nothing before them moved.
        /// </summary>
        internal static void TheLedgerAppendIsExactAndAppendOnly()
        {
            string[] appended = AppendedSymbols();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 21 appends exactly thirteen identities.");
            foreach (string symbol in appended)
                Assertions.True(symbol.Contains("GiantCrab"),
                    "Sprint 21 allocates for one creature only: " + symbol);
            foreach (string tail in new[] {
                "KMG.Summoning.Natural.GiantCrab.UnitType",
                "KMG.Summoning.Natural.GiantCrab.MindlessImmunity",
                "KMG.Summoning.Natural.GiantCrab.TripDefense",
                "KMG.Summoning.Natural.GiantCrab.RacialSkills",
                "KMG.Summoning.Special.GiantCrab.Traits" })
                Assertions.True(appended.Contains(tail, StringComparer.Ordinal),
                    "Missing Sprint 21 identity: " + tail);
            // No execution children, because Nature's Ally never templates
            // and this creature is not on the other table.
            Assertions.Equal(0, appended.Count(symbol =>
                symbol.EndsWith(".Celestial", StringComparison.Ordinal) ||
                symbol.EndsWith(".Fiendish", StringComparison.Ordinal)),
                "An untemplated creature owns no execution children.");
            // No weapon. A Medium creature takes the shared native 1d4 claw
            // unscaled, which is why the Large Sprint 18 and 20 creatures had
            // to own theirs and this one does not.
            Assertions.Equal(0, appended.Count(symbol =>
                symbol.Contains("Claw") || symbol.Contains("Bite")),
                "The Giant Crab owns no weapon identity.");
            // The released carriers are granted, never copied: a renamed copy
            // would retire a GUID that has shipped.
            Assertions.False(appended.Any(symbol =>
                symbol.Contains("FullStrengthLimbs")),
                "The full-Strength carrier is the released one, not a copy.");
        }

        /// <summary>
        /// Every weapon key this creature's profile names must resolve in the
        /// builder. Sprint 19's second guarded run died on exactly this.
        /// </summary>
        internal static void EveryProfileWeaponKeyResolvesInTheBuilder()
        {
            string builder = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(GiantCrabRulesPolicy.GiantCrabKey);
            var keys = new List<string> { profile.PrimaryWeapon };
            keys.AddRange(profile.AdditionalWeapons);
            foreach (string key in keys.Distinct(StringComparer.Ordinal))
                Assertions.True(builder.Contains("key == \"" + key + "\""),
                    "The builder cannot resolve a weapon this profile names, "
                    + "which fails mod initialization: " + key);
        }

        /// <summary>
        /// The two carriers this creature owns are wired, not merely named.
        ///
        /// <para>This is the test Sprint 20 did not have and paid for twice.
        /// Both of these are printed lines whose arithmetic closes without
        /// them, so a registered identity and a closed derivation prove
        /// nothing at all - and that is precisely how a live Giant Scorpion
        /// came to read CMD 27 against a printed 31 and Perception 0 against
        /// a printed +4.</para>
        /// </summary>
        internal static void TheOwnCarriersAreWiredRatherThanNamed()
        {
            string root = RepositoryRoot();
            string builder = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            NaturalSummonProfile crab = ExpandedSummoningNaturalProfiles.For(
                GiantCrabRulesPolicy.GiantCrabKey);
            foreach (string fact in new[] { "GiantCrabTripDefense",
                "GiantCrabRacialSkills", "GiantCrabMindlessImmunity" })
            {
                Assertions.True(crab.Facts.Contains(fact),
                    "The profile does not carry " + fact);
                Assertions.True(builder.Contains("Configure" + fact
                        + "(Require<BlueprintFeature>"),
                    "The builder does not configure " + fact);
                Assertions.True(builder.Contains("fact == \"" + fact + "\""),
                    "The builder cannot resolve " + fact);
            }
            // The values come from the rules policy rather than being written
            // twice, so a drifting number breaks the derivation first.
            Assertions.True(builder.Contains(
                    "defence.Bonus = GiantCrabRulesPolicy.EightLegTripBonus"),
                "The anti-trip carrier must take its value from the policy.");
            Assertions.True(builder.Contains(
                    "perception.Value = GiantCrabRulesPolicy.RacialPerceptionBonus"),
                "The racial carrier must take its value from the policy.");
            // Neither borrowed carrier. The shared native one is two points
            // light and the Sprint 20 one is a scorpion's named feature.
            Assertions.False(crab.Facts.Contains("TripDefenseEightLegs"),
                "The shared eight-leg carrier is worth "
                + GiantCrabRulesPolicy.SharedNativeTripBonus
                + " and this creature prints "
                + GiantCrabRulesPolicy.EightLegTripBonus + ".");
            Assertions.False(crab.Facts.Contains("GiantScorpionTripDefense") ||
                crab.Facts.Contains("GiantScorpionRacialSkills"),
                "A crab must not wear a scorpion's named feature.");
            Assertions.True(crab.Facts.Contains("PrimateFullStrengthLimbs"),
                "Both claws are primary at the whole Strength modifier, so the "
                + "released full-Strength carrier is granted rather than "
                + "copied.");
        }

        /// <summary>
        /// Grab is on both pincers, and the spec still counts limbs.
        /// </summary>
        internal static void GrabRidesBothPincers()
        {
            string special = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs"));
            int grab = special.IndexOf(
                "ConfigureGrabber(library, bySymbol, GiantCrabUnitSymbol",
                StringComparison.Ordinal);
            Assertions.True(grab > 0,
                "The crab's pincers must carry a grab carrier.");
            string spec = special.Substring(grab,
                Math.Min(760, special.Length - grab));
            Assertions.True(spec.Contains("Primary = true"),
                "The crab's primary limb is a pincer and grabs.");
            // Still a count, even though every limb grabs. "All of them"
            // would absorb a third limb silently; the printed count fails on
            // one.
            Assertions.True(spec.Contains(
                    "Additional = GiantCrabRulesPolicy.ClawCount - 1"),
                "The grab reaches the second pincer by counting limbs.");
            Assertions.True(spec.Contains(
                    "MaxHeld = GiantCrabRulesPolicy.ClawCount"),
                "Two pincers hold two foes.");
            Assertions.False(spec.Contains("Rake ="),
                "A crab rakes nothing.");
        }

        /// <summary>
        /// The released Bebelith does not move.
        ///
        /// <para>This sprint owns its body and nothing else. Its rules were
        /// qualified in an earlier phase, its recorded deviations were accepted
        /// then, and its chassis differs from the printed Bestiary block on
        /// purpose - so the easiest way for this sprint to go wrong is to
        /// improve it.</para>
        /// </summary>
        internal static void TheReleasedBebelithDoesNotMove()
        {
            SummonVariantSpec[] bebelith = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key == "bebelith").ToArray();
            Assertions.Equal(3, bebelith.Length,
                "The Bebelith keeps its three Summon Monster roots.");
            Assertions.Equal(3, bebelith.Count(value =>
                value.Family == SummonFamily.Monster),
                "All three on the Summon Monster side; it is a demon and "
                + "belongs on no other table.");
            foreach (SummonVariantSpec variant in bebelith)
                Assertions.True(SummonVisibilityCatalog.IsPublished(variant),
                    "The Bebelith is published and stays published: "
                    + variant.StableKey);
            Assertions.Equal(7, bebelith.Min(value => value.ParentTier),
                "Its own tier is Summon Monster VII.");
        }

        /// <summary>
        /// The review scenario is admitted by every gate that can refuse it.
        ///
        /// <para>Sprint 19 spent an owner runtime transaction discovering that
        /// the mod keeps its own allowlist and refuses an unknown scenario only
        /// after a deploy and a launch. This costs nothing and learns it
        /// offline.</para>
        /// </summary>
        internal static void TheReviewScenarioIsWiredAtEveryGate()
        {
            const string scenario = "disposable-expanded-summoning-sprint21-review";
            Assertions.Equal(scenario, KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint21Review,
                "The scenario name is what the orchestrator asks for.");
            Assertions.True(KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog.IsAllowed(scenario),
                "The mod request validator must admit the Sprint 21 review.");
            Assertions.True(KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog
                    .IsExpandedSummoningRulesScenario(scenario),
                "The Sprint 21 review is an Expanded Summoning rules scenario.");
            string root = RepositoryRoot();
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(automation.Contains("'" + scenario + "'"),
                "The orchestrator needs a descriptor for the Sprint 21 review.");
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains(
                    "DisposableExpandedSummoningSprint21Review"),
                "The runner must dispatch the Sprint 21 review.");
            Assertions.True(runner.Contains("StopSprint21Review(result)"),
                "A timed-out review must still restore its fixture.");
            string review = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            Assertions.True(review.Contains("Sprint14BonePolicy.CrabKey"),
                "The creature review roster must admit the withheld crab.");
            string patch = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(patch.Contains(
                    "{ GiantCrabBlueprintName, Sprint14BonePolicy.CrabKey }"),
                "The view patch must map this creature's blueprint to its key.");
            KingmakerGunslinger.RuntimeTesting.Sprint21ReviewPolicy.Validate();
        }

        /// <summary>
        /// Both eight-legged creatures on this rig may plant an eighth foot,
        /// and nothing else may.
        /// </summary>
        internal static void OnlyTheEightLeggedCreaturesWalkOnTheFourthChain()
        {
            Assertions.True(Sprint14BonePolicy.WalksOnEightLegs(
                    Sprint14BonePolicy.CrabKey),
                "A crab has eight legs.");
            Assertions.True(Sprint14BonePolicy.WalksOnEightLegs(
                    Sprint14BonePolicy.ScorpionKey),
                "So does a scorpion, which Sprint 20 proved on the rig.");
            foreach (string key in new[] { "fire-beetle", "giant-ant-worker",
                "giant-ant-soldier", "giant-ant-drone", "giant-stag-beetle" })
                Assertions.False(Sprint14BonePolicy.WalksOnEightLegs(key),
                    "A six-legged insect must not plant an eighth foot: " + key);
            Assertions.True(Sprint14BonePolicy.AllowedBones(
                    Sprint14BonePolicy.CrabKey).Contains("L_Foot3",
                    StringComparer.Ordinal),
                "The crab's list reaches the fourth chain's foot.");
        }

        private static string[] AppendedSymbols()
        {
            string ledger = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "blueprints", "blueprints.json"));
            var symbols = new List<string>();
            foreach (System.Text.RegularExpressions.Match match in
                System.Text.RegularExpressions.Regex.Matches(ledger,
                    @"""symbol""\s*:\s*""([^""]+)"""))
                symbols.Add(match.Groups[1].Value);
            if (symbols.Count < AppendedLedgerIdentities)
                throw new InvalidOperationException(
                    "The ledger parsed to " + symbols.Count + " symbols.");
            return symbols.Skip(symbols.Count - AppendedLedgerIdentities)
                .ToArray();
        }

        private static string RepositoryRoot()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null &&
                !File.Exists(Path.Combine(directory.FullName, "Info.json")))
                directory = directory.Parent;
            if (directory == null) throw new InvalidOperationException(
                "The repository root was not found from the test output.");
            return directory.FullName;
        }
    }
}
