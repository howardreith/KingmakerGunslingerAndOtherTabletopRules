using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 19: the Girallon and the Xill, offline.
    ///
    /// <para>Nothing here can prove what the creatures are in a running game -
    /// that is the guarded review's job - but everything here can prove that
    /// the numbers the game will be asked for are the printed ones, that they
    /// are derived rather than transcribed, and that the two surfaces which
    /// state them agree with each other.</para>
    /// </summary>
    internal static class Sprint19RulesTests
    {
        /// <summary>
        /// Two units, ten untemplated roots, and ten named creature-owned
        /// identities: four weapons, two unit types, two features and two
        /// buffs. Neither creature is templated, so there are no celestial or
        /// fiendish execution children and ten roots cost ten identities
        /// rather than thirty.
        /// </summary>
        internal const int AppendedLedgerIdentities = 2 + 10 + 10;

        internal const int GirallonAllyRoots = 5;
        internal const int XillMonsterRoots = 5;

        /// <summary>
        /// Both printed profiles derive rather than being transcribed. Every
        /// clause here is an arithmetic identity over the stat block, so a
        /// number that drifts breaks the derivation and not just a constant.
        /// </summary>
        internal static void PrintedProfilesDeriveFromTheStatBlocks()
        {
            GirallonRulesPolicy.Validate();
            XillRulesPolicy.Validate();

            // The four places the recalled Girallon was wrong at intake, each
            // pinned so the same mistake cannot come back: Strength 19 and not
            // 22, a 1d6 bite and not 1d8, 1d4 claws and not 1d6, and four
            // claws and not two.
            Assertions.Equal(19, GirallonRulesPolicy.Strength,
                "The Girallon prints Strength 19.");
            Assertions.Equal(6, GirallonRulesPolicy.BiteDieSides,
                "The Girallon prints a 1d6 bite.");
            Assertions.Equal(4, GirallonRulesPolicy.ClawDieSides,
                "The Girallon prints 1d4 claws.");
            Assertions.Equal(4, GirallonRulesPolicy.ClawCount,
                "The Girallon prints four claws.");
            Assertions.Equal("1d4+6", GirallonRulesPolicy.RendDamage,
                "The Girallon prints rend 1d4+6.");

            // The Xill's two attack bonuses differ by exactly Weapon Focus,
            // which is the only reason its claws and bite are not equal.
            Assertions.Equal(1,
                XillRulesPolicy.PrintedClawAttackBonus -
                    XillRulesPolicy.PrintedBiteAttackBonus,
                "Weapon Focus (claw) is the whole gap between claw and bite.");
            Assertions.Equal(16,
                XillRulesPolicy.ParalysisDifficultyClass(
                    XillRulesPolicy.ConstitutionModifier),
                "The Xill prints a DC 16 paralysis.");
            // The difficulty class follows the live creature rather than
            // being pinned: a tougher Xill is harder to save against.
            Assertions.Equal(18,
                XillRulesPolicy.ParalysisDifficultyClass(4),
                "The paralysis difficulty class follows live Constitution.");
            Assertions.Equal(14,
                XillRulesPolicy.ParalysisDifficultyClass(5, 2),
                "The paralysis difficulty class follows live hit dice.");
        }

        /// <summary>
        /// The Girallon's omitted Climb costs it nothing, which is what makes
        /// the omission honest. Unlike the Sprint 18 apes, which each forfeit
        /// a printed rank, the Girallon spends no rank on Climb at all: its
        /// +12 is Strength plus the racial climb-speed bonus.
        /// </summary>
        internal static void TheOmittedClimbForfeitsNoPrintedRank()
        {
            Assertions.Equal(0, GirallonRulesPolicy.ClimbRanks,
                "The Girallon spends no rank on Climb.");
            Assertions.Equal(GirallonRulesPolicy.HitDice,
                GirallonRulesPolicy.PerceptionRanks +
                    GirallonRulesPolicy.StealthRanks +
                    GirallonRulesPolicy.ClimbRanks,
                "Seven hit dice at Intelligence 2 buy seven ranks, all spent.");
            Assertions.Equal(12, GirallonRulesPolicy.PrintedClimbSkill,
                "The printed Climb total is recorded, not forgotten.");
            Assertions.Equal(GirallonRulesPolicy.PrintedClimbSkill,
                GirallonRulesPolicy.StrengthModifier + 8,
                "The printed Climb is Strength plus the racial climb bonus, "
                + "which is why omitting it forfeits no rank.");
            // Nothing is substituted. The profile names two skills and the
            // omitted movement is not quietly paid for in a third.
            NaturalSummonProfile girallon = ExpandedSummoningNaturalProfiles
                .For(GirallonRulesPolicy.GirallonKey);
            Assertions.True(girallon.Skills.SequenceEqual(
                    new[] { "Perception", "Stealth" }),
                "Nothing stands in for the omitted climb movement.");
        }

        /// <summary>
        /// The Xill's eight printed skills map onto five Kingmaker skills, and
        /// the three that merge are NOT added on top of the skill they land
        /// on. Adding them would hand the creature competence its stat block
        /// does not print.
        /// </summary>
        internal static void MergedSkillsAreRecordedRatherThanAdded()
        {
            Assertions.Equal(43, XillRulesPolicy.PlacedSkillRanks,
                "Forty-three printed ranks land on a Kingmaker skill.");
            Assertions.Equal(70, XillRulesPolicy.PrintedSkillRanks,
                "The stat block spends seventy ranks across eight skills.");
            // The merge is only honest because each merged pair derives the
            // same total: Sense Motive equals Perception, Intimidate equals
            // Bluff, and the two Knowledges are equal.
            Assertions.Equal(XillRulesPolicy.PrintedPerceptionSkill,
                XillRulesPolicy.PrintedSenseMotiveSkill,
                "Sense Motive and Perception derive the same total.");
            Assertions.Equal(XillRulesPolicy.PrintedBluffSkill,
                XillRulesPolicy.PrintedIntimidateSkill,
                "Intimidate and Bluff derive the same total.");
            Assertions.Equal(XillRulesPolicy.PrintedKnowledgeArcanaSkill,
                XillRulesPolicy.PrintedKnowledgePlanesSkill,
                "The two Knowledges derive the same total.");
            // Five Kingmaker skills, and the review expects exactly five.
            Assertions.Equal(5,
                Sprint19ReviewPolicy.For(XillRulesPolicy.XillKey).Skills.Count,
                "Eight printed skills land on exactly five Kingmaker skills.");
        }

        /// <summary>
        /// Ten roots, derived from the tables rather than written down: the
        /// Girallon on Summon Nature's Ally V only, the Xill on Summon
        /// Monster V only, neither templated.
        /// </summary>
        internal static void TenRootsFollowFromTheCatalogTiers()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonCreatureSpec girallon = ExpandedSummoningCatalog.All.Single(
                value => value.Key == GirallonRulesPolicy.GirallonKey);
            SummonCreatureSpec xill = ExpandedSummoningCatalog.All.Single(
                value => value.Key == XillRulesPolicy.XillKey);

            Assertions.False(girallon.MonsterTier.HasValue,
                "The Girallon is on no Summon Monster table.");
            Assertions.Equal(5, girallon.NaturesAllyTier.Value,
                "The Girallon is Summon Nature's Ally V.");
            Assertions.Equal(5, xill.MonsterTier.Value,
                "The Xill is Summon Monster V.");
            Assertions.False(xill.NaturesAllyTier.HasValue,
                "The Xill is on no Summon Nature's Ally table.");
            Assertions.False(girallon.MonsterTemplated,
                "A magical beast with no Summon Monster entry is untemplated.");
            Assertions.False(xill.MonsterTemplated,
                "An evil outsider is untemplated.");

            Assertions.Equal(GirallonAllyRoots, all.Count(value =>
                    value.Creature.Key == GirallonRulesPolicy.GirallonKey),
                "The Girallon takes five roots: parent tiers five to nine.");
            Assertions.Equal(XillMonsterRoots, all.Count(value =>
                    value.Creature.Key == XillRulesPolicy.XillKey),
                "The Xill takes five roots: parent tiers five to nine.");
            Assertions.Equal(10, all.Count(value =>
                    value.Creature.Key == GirallonRulesPolicy.GirallonKey ||
                    value.Creature.Key == XillRulesPolicy.XillKey),
                "Sprint 19 adds exactly ten roots.");
        }

        /// <summary>
        /// Both creatures are published, and nothing published before this
        /// branch moved. They were withheld through eleven guarded reviews
        /// and published only once the complete hidden candidate passed:
        /// 39 of 39 on the batched mechanics review and 12 of 12 on the
        /// party-camera art review.
        /// </summary>
        internal static void BothCreaturesAreRegisteredAndPublished()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            Assertions.Equal(1044,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Ten new placements are registered.");
            Assertions.Equal(0,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Publication withheld nothing.");
            Assertions.Equal(1044,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "All ten new roots are published, and nothing else moved.");
            foreach (SummonVariantSpec variant in all)
                Assertions.True(SummonVisibilityCatalog.IsPublished(variant),
                    "Nothing may be withheld: " + variant.StableKey);
            SummonVisibilityCatalog.Validate();
        }

        /// <summary>
        /// The rend state machine, now that it carries a claw count. The case
        /// a two-claw gate gets wrong is three of four, so that is the first
        /// thing proved here.
        /// </summary>
        internal static void FourClawRendNeedsAllFourClawsOnOneTarget()
        {
            object sequence = new object();
            object target = new object();
            object other = new object();

            // Three of four claws never rends. This is the whole difference
            // between the Girallon and the Dire Ape.
            var tracker = new ClawRendTracker(4);
            Assertions.Equal(4, tracker.ClawCount, "A Girallon tracker holds four claws.");
            for (int claw = 1; claw <= 3; claw++)
            {
                Assertions.False(tracker.TryArm(sequence, claw, target),
                    "Claw " + claw + " of four cannot rend on its own.");
                tracker.RecordOutcome(sequence, claw, target, true);
            }
            Assertions.False(tracker.HasEmitted,
                "Three claws of four emit no rend.");

            // The fourth does.
            Assertions.True(tracker.TryArm(sequence, 4, target),
                "All four claws on one target in one sequence must rend.");
            Assertions.True(tracker.IsArmed, "The qualifying claw must be armed.");
            tracker.RecordOutcome(sequence, 4, target, true);
            Assertions.True(tracker.HasEmitted, "The armed rend must resolve.");
            Assertions.False(tracker.IsArmed,
                "A resolved rend leaves nothing pending.");

            // Exactly one rend per qualifying sequence, from any claw.
            for (int claw = 1; claw <= 4; claw++)
                Assertions.False(tracker.TryArm(sequence, claw, target),
                    "A sequence rends at most once, from any claw.");

            // Claws spread across two targets never rend either of them.
            var split = new ClawRendTracker(4);
            split.RecordOutcome(sequence, 1, target, true);
            split.RecordOutcome(sequence, 2, target, true);
            split.RecordOutcome(sequence, 3, other, true);
            Assertions.False(split.TryArm(sequence, 4, target),
                "A claw that landed elsewhere does not complete a rend.");
            Assertions.False(split.TryArm(sequence, 4, other),
                "Nor does the other target get one.");

            // The Dire Ape keeps its own two, which is the point of a count
            // rather than a second state machine.
            var pair = new ClawRendTracker(2);
            Assertions.Equal(2, pair.ClawCount, "A Dire Ape tracker holds two claws.");
            pair.RecordOutcome(sequence, 1, target, true);
            Assertions.True(pair.TryArm(sequence, 2, target),
                "Two claws still rend for a creature that prints two.");

            // A claw index outside the creature's own count is not its claw.
            var bounded = new ClawRendTracker(4);
            bounded.RecordOutcome(sequence, 1, target, true);
            bounded.RecordOutcome(sequence, 2, target, true);
            bounded.RecordOutcome(sequence, 3, target, true);
            Assertions.False(bounded.TryArm(sequence, 5, target),
                "A fifth claw does not exist and cannot rend.");
            Assertions.False(bounded.TryArm(sequence, 0, target),
                "Claw zero is not a claw.");

            // An attack with no live command is not a sequence.
            var replayed = new ClawRendTracker(4);
            for (int claw = 1; claw <= 3; claw++)
                replayed.RecordOutcome(sequence, claw, target, true);
            Assertions.False(replayed.TryArm(null, 4, target),
                "An attack with no live attack command never rends.");

            // The printed claw counts, by creature.
            Assertions.Equal(4, PrimateRulesPolicy.RendClawCount("girallon"),
                "The Girallon rends on four.");
            Assertions.Equal(2, PrimateRulesPolicy.RendClawCount("dire-ape"),
                "The Dire Ape rends on two.");
            Assertions.Equal(0, PrimateRulesPolicy.RendClawCount("ape"),
                "The Ape prints no rend.");
        }

        /// <summary>
        /// A rend state that crosses a command or a round boundary is a bug,
        /// and the four-claw tracker has to refuse it the same way the
        /// two-claw one did.
        /// </summary>
        internal static void NoStaleFourClawRendCrossesACommand()
        {
            object first = new object();
            object second = new object();
            object target = new object();

            var tracker = new ClawRendTracker(4);
            for (int claw = 1; claw <= 3; claw++)
                tracker.RecordOutcome(first, claw, target, true);
            // A new command resets everything, so three carried hits are gone.
            Assertions.False(tracker.TryArm(second, 4, target),
                "No hit carries across an attack command.");
            Assertions.Equal(second, tracker.Sequence,
                "The tracker follows the live command.");
            Assertions.False(tracker.HasEmitted, "Nothing was emitted.");

            // A miss on a chosen claw leaves nothing pending and nothing to
            // pair with later.
            var missed = new ClawRendTracker(4);
            for (int claw = 1; claw <= 3; claw++)
                missed.RecordOutcome(second, claw, target, true);
            Assertions.True(missed.TryArm(second, 4, target),
                "The fourth claw qualifies before its roll is known.");
            missed.RecordOutcome(second, 4, target, false);
            Assertions.False(missed.HasEmitted, "A missed claw rends for nothing.");
            Assertions.False(missed.IsArmed, "A miss leaves nothing armed.");

            // A tracker cannot be built for a creature that prints no rend.
            bool refused = false;
            try { new ClawRendTracker(1); }
            catch (ArgumentOutOfRangeException) { refused = true; }
            Assertions.True(refused, "A rend needs at least two claws.");
        }

        /// <summary>
        /// The review expectations and the rules policies have to agree, in
        /// both directions. A number that drifts has to break both surfaces,
        /// which is the only reason keeping two of them is worth anything.
        /// </summary>
        internal static void ReviewExpectationsAgreeWithTheRulesPolicies()
        {
            Sprint19ReviewPolicy.Validate();
            PrimateVisualPolicy.Validate();

            Sprint19LiveProfile girallon = Sprint19ReviewPolicy.For(
                GirallonRulesPolicy.GirallonKey);
            Sprint19LiveProfile xill = Sprint19ReviewPolicy.For(
                XillRulesPolicy.XillKey);

            // Five limbs each, four of them claws, every one at the plain
            // Strength modifier.
            foreach (Sprint19LiveProfile live in new[] { girallon, xill })
            {
                Assertions.Equal(5, live.Limbs.Length,
                    live.Key + " has a bite and four claws.");
                int strength = (live.Strength - 10) / 2;
                foreach (int[] limb in live.Limbs)
                    Assertions.Equal(strength, limb[2],
                        live.Key + " limbs carry the whole Strength modifier.");
            }

            // Only the Girallon rends, and only the Xill resists spells,
            // grabs and paralyzes.
            Assertions.True(girallon.HasRend, "The Girallon rends.");
            Assertions.False(xill.HasRend, "The Xill prints no rend.");
            Assertions.Equal(0, girallon.SpellResistance,
                "The Girallon prints no spell resistance.");
            Assertions.Equal(17, xill.SpellResistance,
                "The Xill prints spell resistance 17.");
            Assertions.Equal(0, girallon.ParalysisDifficultyClass,
                "The Girallon paralyzes nothing.");
            Assertions.Equal(16, xill.ParalysisDifficultyClass,
                "The Xill paralyzes at DC 16.");

            // The four-claw rend case table keeps the case a two-claw gate
            // would get wrong.
            Assertions.True(Sprint19ReviewPolicy.RendCases.Any(row =>
                    row[0] == "three-claws-one-target" && row[1] == "no-rend"),
                "Three of four claws must be proved not to rend.");
            Assertions.Equal(9, Sprint19ReviewPolicy.RendCases.Length,
                "The rend case table is frozen at nine cases.");
        }

        /// <summary>
        /// The guarded review scenario is wired at every gate that can refuse
        /// it.
        ///
        /// <para>Four separate places have to know a scenario name before a
        /// guarded run can reach the review, and the last of them refuses
        /// only after a deploy and a launch. The first Sprint 19 run was
        /// rejected at exactly that gate - the mod's own request allowlist -
        /// and spent an owner runtime transaction to learn it. This test
        /// costs nothing and learns it offline.</para>
        /// </summary>
        internal static void TheReviewScenarioIsWiredAtEveryGate()
        {
            const string scenario = "disposable-expanded-summoning-sprint19-review";
            Assertions.Equal(scenario, RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint19Review,
                "The scenario name is what the orchestrator asks for.");
            // The gate that rejected the first run: the mod refuses any
            // scenario not on its own allowlist, before any hook installs.
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario),
                "The mod request validator must admit the Sprint 19 review.");
            // The group that decides which requests may use the working save
            // and the disposable-actor fixture.
            Assertions.True(RuntimeTestScenarioCatalog
                    .IsExpandedSummoningRulesScenario(scenario),
                "The Sprint 19 review is an Expanded Summoning rules scenario.");
            // The two PowerShell gates, checked as text because that is what
            // they are: a descriptor the orchestrator looks up by name, and a
            // crowd roster that refuses an unknown creature key.
            string root = RepositoryRoot();
            string automation = System.IO.File.ReadAllText(
                System.IO.Path.Combine(root, "scripts", "RuntimeAutomation.Common.ps1"));
            Assertions.True(automation.Contains("'" + scenario + "'"),
                "The orchestrator needs a descriptor for the Sprint 19 review.");
            foreach (string key in Sprint19ReviewPolicy.Keys)
            {
                Assertions.True(automation.Contains("'" + key + "'"),
                    "The orchestrator crowd roster must know " + key + ".");
                string launcher = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    root, "scripts", "Invoke-KingmakerRuntimeTest.ps1"));
                Assertions.True(launcher.Contains("'" + key + "'"),
                    "The launcher crowd roster must know " + key + ".");
            }
        }

        /// <summary>
        /// Every weapon a natural profile names can actually be resolved.
        ///
        /// <para>The builder maps a profile's weapon key to a blueprint
        /// through a chain of exact string comparisons and throws
        /// "Unknown natural weapon key" on anything it does not recognise.
        /// That throw happens during blueprint initialization, so the whole
        /// mod fails to load and every registration is rolled back - which is
        /// exactly what the first real Sprint 19 guarded run measured, for
        /// GirallonBite1d6, after a deploy and a launch.</para>
        ///
        /// <para>This is checked over the source text because the resolver is
        /// a comparison chain rather than a table. It is not elegant, and it
        /// catches the precise defect that cost an owner runtime transaction,
        /// for every creature rather than only this sprint's.</para>
        /// </summary>
        internal static void EveryProfileWeaponKeyResolvesInTheBuilder()
        {
            string root = RepositoryRoot();
            string builder = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (NaturalSummonProfile profile in
                ExpandedSummoningNaturalProfiles.All)
            {
                keys.Add(profile.PrimaryWeapon);
                foreach (string key in profile.AdditionalWeapons) keys.Add(key);
                foreach (string key in profile.AdditionalSecondaryWeapons)
                    keys.Add(key);
            }
            Assertions.True(keys.Count > 20,
                "The profiles must name a real set of weapons.");
            foreach (string key in keys.OrderBy(value => value,
                StringComparer.Ordinal))
                Assertions.True(builder.Contains("key == " + Quote(key)),
                    "The natural builder cannot resolve the weapon key " +
                    Quote(key) + ", so blueprint initialization would throw "
                    + "and roll back every registration.");
        }

        private static string Quote(string value)
        { return "\"" + value + "\""; }

        /// <summary>
        /// The repository root, found by walking up to the solution file.
        /// The test binary lives several directories below it and the depth
        /// is not the same from every build output.
        /// </summary>
        private static string RepositoryRoot()
        {
            var cursor = new System.IO.DirectoryInfo(
                System.AppDomain.CurrentDomain.BaseDirectory);
            while (cursor != null)
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(
                        cursor.FullName, "KingmakerGunslinger.sln")) &&
                    System.IO.Directory.Exists(System.IO.Path.Combine(
                        cursor.FullName, "scripts")))
                    return cursor.FullName;
                cursor = cursor.Parent;
            }
            throw new InvalidOperationException(
                "The repository root is not above the test binary.");
        }

        /// <summary>
        /// Both bodies ride the Sprint 18 donor rig and both declare the
        /// authored limitation rather than claiming four independent arms.
        /// </summary>
        internal static void BothBodiesDeclareTheSharedDriverChains()
        {
            foreach (string key in PrimateVisualPolicy.FourArmedKeys)
            {
                Assertions.True(PrimateVisualPolicy.IsFourArmed(key),
                    key + " is a four-armed body.");
                Assertions.Equal("sprint19-fourarmed",
                    PrimateVisualPolicy.AssetDirectory(key),
                    key + " ships in its own asset directory.");
                Assertions.Equal(56, PrimateVisualPolicy.Bones(key).Length,
                    key + " weights the same 56 donor drivers as Sprint 18.");
                string size = key == GirallonRulesPolicy.GirallonKey ?
                    "Large" : "Medium";
                Assertions.True(PrimateVisualPolicy.PermitsFourArmedAnatomy(
                        key, 6, 4, 2, true, true, size),
                    key + " declares six limbs on two driver chains.");
                // A body claiming four independent chains is claiming a rig
                // this project did not build.
                Assertions.False(PrimateVisualPolicy.PermitsFourArmedAnatomy(
                        key, 6, 4, 4, true, true, size),
                    key + " may not claim four driver chains.");
                Assertions.False(PrimateVisualPolicy.PermitsFourArmedAnatomy(
                        key, 6, 4, 2, false, true, size),
                    key + " must declare that its lower arms share drivers.");
                Assertions.False(PrimateVisualPolicy.PermitsFourArmedAnatomy(
                        key, 4, 4, 2, true, true, size),
                    key + " has six visible limbs, not four.");
                // The two anatomy schemas must never both accept one body.
                Assertions.False(PrimateVisualPolicy.PermitsAnatomy(
                        key, false, false, true, 4, true, true, false),
                    key + " must not pass the Sprint 18 ape anatomy contract.");
            }
            // And the apes must not pass the four-armed one.
            foreach (string key in PrimateVisualPolicy.ApeKeys)
            {
                Assertions.False(PrimateVisualPolicy.PermitsFourArmedAnatomy(
                        key, 6, 4, 2, true, true, "Large"),
                    key + " is not a four-armed body.");
                Assertions.Equal("sprint18-primates",
                    PrimateVisualPolicy.AssetDirectory(key),
                    key + " keeps its own shipped directory.");
            }
            // Four creatures on one rig, four distinct identities.
            Assertions.Equal(4, PrimateVisualPolicy.Keys.Length,
                "Four creatures are rebodied on the donor rig.");
            Assertions.Equal(4, PrimateVisualPolicy.ProductionGuids
                    .Distinct(StringComparer.Ordinal).Count(),
                "Each rebodied creature has its own identity.");
        }
    }
}
