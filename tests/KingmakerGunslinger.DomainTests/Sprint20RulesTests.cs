using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 20: the Giant Scorpion, registered and withheld.
    ///
    /// <para>This creature is unusually well behaved arithmetically - no
    /// Intelligence score means no skill ranks and no feats, so every printed
    /// number falls out of hit dice, ability scores, size and racial bonuses
    /// alone. These tests are therefore a complete account of the stat block
    /// rather than a sample of it.</para>
    /// </summary>
    internal static class Sprint20RulesTests
    {
        /// <summary>
        /// One unit, twelve logical placements, the twelve celestial and
        /// fiendish execution children of the six templated Summon Monster
        /// roots, and six blueprints of its own. Thirty-one for one creature
        /// where Sprint 19 spent twenty-two on two: the difference is
        /// templating, not waste.
        /// </summary>
        internal const int AppendedLedgerIdentities = 1 + 12 + 12 + 9;

        internal const int MonsterRoots = 6;
        internal const int NaturesAllyRoots = 6;

        /// <summary>
        /// Every printed number, derived rather than transcribed. The policy
        /// owns the derivation; this proves the policy is reached and that
        /// its identities close.
        /// </summary>
        internal static void ThePrintedProfileDerives()
        {
            GiantScorpionRulesPolicy.Validate();

            // Three limbs at one bonus and the plain Strength modifier. If
            // the engine's primary-hand rule reached this creature, one limb
            // would read 1d6+6 against a printed 1d6+4.
            Assertions.Equal(6, GiantScorpionRulesPolicy.PrintedAttackBonus,
                "Base attack plus Strength less one for Large size.");
            Assertions.Equal(4, GiantScorpionRulesPolicy.StrengthModifier,
                "Every limb adds the whole Strength modifier, not half again.");
            Assertions.Equal(3, GiantScorpionRulesPolicy.ClawCount +
                GiantScorpionRulesPolicy.StingCount,
                "Two claws and one sting.");

            // The poison difficulty class is derived, never stored, so a
            // buffed or weakened scorpion poisons for what it supports.
            Assertions.Equal(15, GiantScorpionRulesPolicy.PoisonDifficultyClass(
                GiantScorpionRulesPolicy.HitDice,
                GiantScorpionRulesPolicy.ConstitutionModifier),
                "10 plus half the hit dice plus Constitution.");
            Assertions.Equal(17, GiantScorpionRulesPolicy.PoisonDifficultyClass(
                GiantScorpionRulesPolicy.HitDice,
                GiantScorpionRulesPolicy.ConstitutionModifier + 2),
                "A buffed Constitution raises the save it forces.");

            // Six rounds is the whole reason this poison cannot share a
            // shipped carrier.
            Assertions.Equal(6, GiantScorpionRulesPolicy.PoisonRounds,
                "The printed graph runs six rounds.");
            Assertions.Equal(4, GiantAntPoisonPolicy.Exposures,
                "Every shipped carrier runs four, which is why this one is new.");
        }

        /// <summary>
        /// A mindless creature buys no skill ranks, so all three printed
        /// totals are ability plus racial bonus. Giving it the builder's
        /// default ranks is exactly how a Giant Ant once read Perception 7
        /// against a printed +5.
        /// </summary>
        internal static void ItBuysNoSkillRanks()
        {
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(GiantScorpionRulesPolicy.GiantScorpionKey);
            Assertions.Equal(0, profile.Skills.Count,
                "A creature with no Intelligence score has no ranks to place.");
            Assertions.Equal(0, GiantScorpionRulesPolicy.SkillRanks,
                "And the policy says so too.");
            Assertions.Equal(4, GiantScorpionRulesPolicy.PrintedPerceptionSkill,
                "Perception is Wisdom plus the racial bonus and nothing else.");
            Assertions.Equal(1, GiantScorpionRulesPolicy.PrintedStealthSkill,
                "Stealth is Dexterity plus the racial bonus less Large size.");
        }

        /// <summary>
        /// Registered and withheld. Twelve placements exist and no player can
        /// select one, and nothing v0.0.148 published moved.
        /// </summary>
        internal static void TheCreatureIsRegisteredAndWithheld()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] mine = all.Where(value =>
                value.Creature.Key == GiantScorpionRulesPolicy.GiantScorpionKey)
                .ToArray();
            Assertions.Equal(MonsterRoots + NaturesAllyRoots, mine.Length,
                "Tier 4 on both tables is six roots each.");
            Assertions.Equal(MonsterRoots, mine.Count(value =>
                value.Family == SummonFamily.Monster),
                "Six Summon Monster roots.");
            Assertions.Equal(NaturesAllyRoots, mine.Count(value =>
                value.Family == SummonFamily.NaturesAlly),
                "Six Summon Nature's Ally roots.");
            // Published on 2026-10-10, once the complete hidden candidate
            // passed: 20 of 20 on the batched mechanics review in both combat
            // modes and 4 of 4 on the party-camera art review. Publication was
            // the removal of one suppression key, so the registered count is
            // the one registration allocated and nothing moved to make room.
            Assertions.Equal(0,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "All twelve are published.");
            Assertions.Equal(1056,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Registered placements rose from 1044 to 1056.");
            Assertions.Equal(1056,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The published surface is every registered placement.");
            foreach (SummonVariantSpec variant in all)
                Assertions.True(
                    SummonVisibilityCatalog.IsPublished(variant),
                    "Every placement is published, this creature's included: "
                    + variant.StableKey);
        }

        /// <summary>
        /// It is templated, which is what every other vermin on the Summon
        /// Monster table is. Getting this wrong silently halves what the
        /// sprint owes the ledger.
        /// </summary>
        internal static void ItIsTemplatedOnTheMonsterTableOnly()
        {
            SummonCreatureSpec creature = ExpandedSummoningCatalog.All.Single(
                value => value.Key == GiantScorpionRulesPolicy.GiantScorpionKey);
            Assertions.True(creature.MonsterTemplated,
                "Every vermin on the Summon Monster table is templated.");
            Assertions.Equal(4, creature.MonsterTier ?? -1,
                "Summon Monster IV.");
            Assertions.Equal(4, creature.NaturesAllyTier ?? -1,
                "Summon Nature's Ally IV.");

            string[] appended = AppendedSymbols();
            string[] children = appended.Where(symbol =>
                symbol.EndsWith(".Celestial", StringComparison.Ordinal) ||
                symbol.EndsWith(".Fiendish", StringComparison.Ordinal))
                .ToArray();
            Assertions.Equal(MonsterRoots * 2, children.Length,
                "Each Summon Monster root owns a celestial and a fiendish "
                + "execution child.");
            foreach (string symbol in children)
                Assertions.True(symbol.Contains(".SM."),
                    "Summon Nature's Ally never templates: " + symbol);
        }

        /// <summary>
        /// The append is exactly thirty-four entries at the end of the ledger,
        /// all of them this creature's, and nothing before them moved.
        /// </summary>
        internal static void TheLedgerAppendIsExactAndAppendOnly()
        {
            string[] appended = AppendedSymbols();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 20 appends exactly thirty-four identities.");
            foreach (string symbol in appended)
                Assertions.True(symbol.Contains("GiantScorpion"),
                    "Sprint 20 allocates for one creature only: " + symbol);
            // The nine it owns beyond its unit and placements.
            foreach (string tail in new[] {
                "KMG.Summoning.Natural.GiantScorpion.Claw1d6",
                "KMG.Summoning.Natural.GiantScorpion.Sting1d6",
                "KMG.Summoning.Natural.GiantScorpion.UnitType",
                "KMG.Summoning.Natural.GiantScorpion.Poison",
                "KMG.Summoning.Natural.GiantScorpion.Venom",
                "KMG.Summoning.Natural.GiantScorpion.MindlessImmunity",
                "KMG.Summoning.Special.GiantScorpion.Traits",
                "KMG.Summoning.Natural.GiantScorpion.TripDefense",
                "KMG.Summoning.Natural.GiantScorpion.RacialSkills" })
                Assertions.True(appended.Contains(tail, StringComparer.Ordinal),
                    "Missing Sprint 20 identity: " + tail);
            // The released Sprint 18 carrier is granted, never copied: a
            // renamed copy would retire a GUID that shipped in v0.0.147.
            Assertions.False(appended.Any(symbol =>
                symbol.Contains("FullStrengthLimbs")),
                "The full-Strength carrier is the released one, not a copy.");
        }

        /// <summary>
        /// Every weapon key this creature's profile names must resolve in the
        /// builder. Sprint 19's second guarded run died on exactly this: the
        /// Girallon's two weapons were never added to the key resolver and
        /// mod initialization rolled back 2,376 registrations.
        /// </summary>
        internal static void EveryProfileWeaponKeyResolvesInTheBuilder()
        {
            string builder = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(GiantScorpionRulesPolicy.GiantScorpionKey);
            var keys = new List<string> { profile.PrimaryWeapon };
            keys.AddRange(profile.AdditionalWeapons);
            foreach (string key in keys.Distinct(StringComparer.Ordinal))
                Assertions.True(
                    builder.Contains("key == \"" + key + "\""),
                    "The builder cannot resolve a weapon this profile names, "
                    + "which fails mod initialization: " + key);
        }

        /// <summary>
        /// Grab is on the claws and poison is on the sting, and neither
        /// reaches the other. Both are gated on a weapon type a layer above
        /// the trigger, so the test reads the builder's wiring rather than
        /// trusting a comment.
        /// </summary>
        internal static void GrabAndPoisonStayOnTheirOwnLimbs()
        {
            string builder = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            int poison = builder.IndexOf("ConfigureGiantScorpionPoison(library,",
                StringComparison.Ordinal);
            Assertions.True(poison > 0,
                "The scorpion's poison must be configured.");
            string call = builder.Substring(poison,
                Math.Min(420, builder.Length - poison));
            Assertions.True(call.Contains("GiantScorpionSting1d6Symbol"),
                "The poison is gated on the sting's own weapon type.");
            Assertions.False(call.Contains("GiantScorpionClaw1d6Symbol"),
                "A claw must never deliver this poison.");

            // The grab half, which was a claim in three places before it was
            // a carrier anywhere: the profile's attribution line, the printed
            // +12 grapple derivation, and the registration commit's own
            // message. None of those is the thing that grabs, and none of
            // them would have failed.
            string special = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs"));
            int grab = special.IndexOf(
                "ConfigureGrabber(library, bySymbol, GiantScorpionUnitSymbol",
                StringComparison.Ordinal);
            Assertions.True(grab > 0,
                "The scorpion's claws must carry a grab carrier.");
            string spec = special.Substring(grab,
                Math.Min(760, special.Length - grab));
            // The primary limb plus one additional limb is both claws and
            // stops short of the sting, which is the second additional limb.
            // A spec counting one limb further would grab with the sting.
            Assertions.True(spec.Contains("Primary = true"),
                "The scorpion's primary limb is a claw and grabs.");
            Assertions.True(spec.Contains(
                    "Additional = GiantScorpionRulesPolicy.ClawCount - 1"),
                "The grab reaches the second claw and stops before the sting.");
            Assertions.True(spec.Contains(
                    "MaxHeld = GiantScorpionRulesPolicy.ClawCount"),
                "Two claws hold two foes.");
            Assertions.False(spec.Contains("Rake ="),
                "A scorpion rakes nothing.");
        }

        /// <summary>
        /// The review scenario is admitted by every gate that can refuse it.
        ///
        /// <para>Four gates stand between the orchestrator and this review and
        /// the last of them refuses only after a deploy and a launch. Sprint
        /// 19 spent an owner runtime transaction learning that. This costs
        /// nothing and learns it offline.</para>
        /// </summary>
        internal static void TheReviewScenarioIsWiredAtEveryGate()
        {
            const string scenario = "disposable-expanded-summoning-sprint20-review";
            Assertions.Equal(scenario, KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint20Review,
                "The scenario name is what the orchestrator asks for.");
            // The gate that rejected Sprint 19's first run: the mod refuses
            // any scenario not on its own allowlist, before any hook installs.
            Assertions.True(KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog.IsAllowed(scenario),
                "The mod request validator must admit the Sprint 20 review.");
            // The group that decides which requests may use the working save
            // and the disposable-actor fixture.
            Assertions.True(KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog
                    .IsExpandedSummoningRulesScenario(scenario),
                "The Sprint 20 review is an Expanded Summoning rules scenario.");
            string root = RepositoryRoot();
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(automation.Contains("'" + scenario + "'"),
                "The orchestrator needs a descriptor for the Sprint 20 review.");
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains(
                    "DisposableExpandedSummoningSprint20Review"),
                "The runner must dispatch the Sprint 20 review.");
            Assertions.True(runner.Contains("StopSprint20Review(result)"),
                "A timed-out review must still restore its fixture.");
            // The other guarded run this sprint needs: the party-camera body
            // review. It casts through a parent this creature does not
            // publish, so the roster has to waive the publication guard for it
            // by name.
            string review = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            Assertions.True(review.Contains("Sprint14BonePolicy.ScorpionKey"),
                "The creature review roster must admit the withheld scorpion.");
            // The view patch is what actually puts the body on the creature.
            // A creature missing from its map spawns wearing its donor and
            // nothing else about the creature looks wrong. The patch is Unity
            // code, so it is read as text here the way Sprint 10 reads it.
            string patch = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(patch.Contains(
                    "{ GiantScorpionBlueprintName, Sprint14BonePolicy.ScorpionKey }"),
                "The view patch must map this creature's blueprint to its key.");
            Assertions.True(patch.Contains(
                    "\"KMG_Summoning_Unit_GiantScorpion\""),
                "The mapped blueprint name must be the registered one.");
            // The review's own expectations, which must agree with the rules
            // policy rather than restate it.
            KingmakerGunslinger.RuntimeTesting.Sprint20ReviewPolicy.Validate();
        }

        /// <summary>
        /// The census scenario is admitted by every gate that can refuse it.
        ///
        /// <para>Sprint 19's first guarded run was spent discovering that the
        /// mod keeps its own allowlist and refuses an unknown scenario only
        /// after a deploy and a launch. That cost an owner transaction to
        /// learn, so it is checked offline here instead.</para>
        /// </summary>
        internal static void TheCensusScenarioIsWiredAtEveryGate()
        {
            const string scenario = "observe-expanded-summoning-arachnid-census";
            Assertions.Equal(scenario, KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog.ObserveExpandedSummoningArachnidCensus,
                "The scenario name is what the orchestrator asks for.");
            Assertions.True(KingmakerGunslinger.RuntimeTesting
                    .RuntimeTestScenarioCatalog.IsAllowed(scenario),
                "The mod request validator must admit the Sprint 20 census.");
            string root = RepositoryRoot();
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(automation.Contains("'" + scenario + "'"),
                "The orchestrator needs a descriptor for the Sprint 20 census.");
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains(
                    "ObserveExpandedSummoningArachnidCensus"),
                "The runner must dispatch the Sprint 20 census.");
            // And the survey's own bounds hold.
            KingmakerGunslinger.RuntimeTesting.ArachnidRigSurveyPolicy.Validate();
        }

        private static string[] AppendedSymbols()
        {
            string ledger = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "blueprints", "blueprints.json"));
            // Hand index arithmetic over the quotes got this wrong first
            // time and silently returned empty strings, so the match is
            // explicit: the quoted value that follows each "symbol" key.
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
