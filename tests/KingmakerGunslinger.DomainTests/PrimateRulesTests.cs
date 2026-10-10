using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 18: the Ape and the Dire Ape, checked at the layer where the
    /// derivations live.
    ///
    /// <para>These run the same functions the runtime calls, with the inputs a
    /// live situation produces. What they cannot do is prove the engine applies
    /// them, which is what the guarded runtime review exists for.</para>
    /// </summary>
    internal static class PrimateRulesTests
    {
        /// <summary>
        /// What Sprint 18 appends to the blueprint ledger: two units, the
        /// twenty-six roots, the twenty-six template execution children of the
        /// thirteen templated Summon Monster roots, and four creature-owned
        /// identities.
        /// </summary>
        // 2 units, the 26 roots, the 26 template execution children of the
        // 13 templated Summon Monster roots, and 6 creature-owned
        // identities: two unit types, the rend feature, the slam, the two
        // Dire Ape weapons the guarded review proved it needs, and the
        // feature that keeps every limb on the plain Strength modifier.
        internal const int AppendedLedgerIdentities = 2 + 26 + 26 + 7;

        private const int ApeMonsterRoots = 7;
        private const int ApeAllyRoots = 7;
        private const int DireApeMonsterRoots = 6;
        private const int DireApeAllyRoots = 6;

        /// <summary>
        /// The twenty-six roots, derived rather than asserted: both apes sit in
        /// both families, so each one's roots are the parent tiers from its own
        /// tier up to nine, twice.
        /// </summary>
        internal static void TwentySixRootsFollowFromTheCatalogTiers()
        {
            SummonVariantSpec[] monster = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).ToArray();
            SummonVariantSpec[] ally = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly).ToArray();
            Assertions.Equal(530, monster.Length,
                "Summon Monster registered placements changed.");
            Assertions.Equal(526, ally.Length,
                "Summon Nature's Ally registered placements changed.");
            Assertions.Equal(1056, monster.Length + ally.Length,
                "Registered generated placements must be 1008 plus Sprint 18's "
                + "26 roots, Sprint 19's 10 and Sprint 20's 12.");

            foreach (var row in new[] {
                new { Key = "ape", Tier = 3, Monster = ApeMonsterRoots, Ally = ApeAllyRoots },
                new { Key = "dire-ape", Tier = 4, Monster = DireApeMonsterRoots, Ally = DireApeAllyRoots } })
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All.Single(
                    value => value.Key == row.Key);
                Assertions.Equal(row.Tier, creature.MonsterTier.Value,
                    row.Key + " Summon Monster tier changed.");
                Assertions.Equal(row.Tier, creature.NaturesAllyTier.Value,
                    row.Key + " Summon Nature's Ally tier changed.");
                Assertions.True(creature.MonsterTemplated,
                    row.Key + " must follow the existing celestial/fiendish template policy.");
                SummonVariantSpec[] mine = monster.Where(value =>
                    value.Creature.Key == row.Key).ToArray();
                SummonVariantSpec[] myAlly = ally.Where(value =>
                    value.Creature.Key == row.Key).ToArray();
                Assertions.Equal(row.Monster, mine.Length,
                    row.Key + " Summon Monster root count changed.");
                Assertions.Equal(row.Ally, myAlly.Length,
                    row.Key + " Summon Nature's Ally root count changed.");
                Assertions.True(Enumerable.Range(row.Tier, 10 - row.Tier)
                        .SequenceEqual(mine.Select(value => value.ParentTier)
                            .OrderBy(value => value)),
                    row.Key + " must appear under every parent from its own tier to nine.");
                // Quantity propagation is the charter's, unchanged: own tier
                // single, one lower 1d3, everything lower 1d4+1.
                foreach (SummonVariantSpec variant in mine.Concat(myAlly))
                {
                    SummonMultiplicity expected =
                        variant.ParentTier == row.Tier ? SummonMultiplicity.One :
                        variant.ParentTier == row.Tier + 1 ? SummonMultiplicity.OneD3 :
                        SummonMultiplicity.OneD4PlusOne;
                    Assertions.Equal(expected, variant.Multiplicity,
                        row.Key + " quantity propagation changed at tier " +
                        variant.ParentTier + ".");
                }
            }

            Assertions.Equal(26, monster.Concat(ally).Count(value =>
                    PrimateRulesPolicy.IsPrimate(value.Creature.Key)),
                "Sprint 18 must add exactly twenty-six roots.");
            Assertions.Equal(102, ExpandedSummoningCatalog.All.Count,
                "Unique creature count must be 102.");
        }

        /// <summary>
        /// Every root is registered and every root is published. Publication
        /// was the removal of two keys and nothing else: no identity moved,
        /// and the icons the withheld creatures already owned did not change.
        /// </summary>
        internal static void BothApesAreRegisteredAndPublished()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            Assertions.Equal(1056,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Registered placement count changed.");
            // The apes published on 2026-10-09 after their complete hidden
            // candidate passed, and nothing has withheld them since. Sprint 19
            // registers ten placements of its own and holds all ten, so the
            // published total is exactly what v0.0.147 showed.
            Assertions.Equal(12,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Only Sprint 20's own twelve roots are withheld.");
            Assertions.Equal(1044,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The apes stay published and the surface is v0.0.148's.");
            foreach (SummonVariantSpec variant in all)
                Assertions.Equal(
                    variant.Creature.Key !=
                        GiantScorpionRulesPolicy.GiantScorpionKey,
                    SummonVisibilityCatalog.IsPublished(variant),
                    "Nothing but the Sprint 20 creature may be withheld: "
                    + variant.StableKey);
            SummonVisibilityCatalog.Validate();

            // Identities are allocated once at registration, so a withheld
            // creature already owns its unit, every root and both template
            // children, and already owns its icon.
            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            foreach (var row in new[] {
                new { Token = "Ape", Roots = ApeMonsterRoots, Ally = ApeAllyRoots },
                new { Token = "DireApe", Roots = DireApeMonsterRoots,
                    Ally = DireApeAllyRoots } })
            {
                Assertions.Equal(1, identities.Count(value =>
                        value.Symbol == "KMG.Summoning.Unit." + row.Token),
                    row.Token + " must own exactly one unit identity.");
                // Every templated Summon Monster root owns its celestial and
                // fiendish execution identity as well as itself.
                Assertions.Equal(row.Roots * 3, identities.Count(value =>
                        value.Symbol.StartsWith("KMG.Summoning.Ability.SM.",
                            StringComparison.Ordinal) &&
                        value.Symbol.Contains("." + row.Token + ".")),
                    row.Token + " Summon Monster identity count changed; each templated root owns itself plus two template children.");
                Assertions.Equal(row.Ally, identities.Count(value =>
                        value.Symbol.StartsWith("KMG.Summoning.Ability.SNA.",
                            StringComparison.Ordinal) &&
                        value.Symbol.Contains("." + row.Token + ".")),
                    row.Token + " Summon Nature's Ally identities are untemplated and must number exactly its roots.");
            }
            Assertions.Equal(2, SummonIconCatalog.All.Count(value =>
                    PrimateRulesPolicy.IsPrimate(value.Key)),
                "Each ape owns its own icon, as it did while withheld.");
            foreach (SummonCreatureSpec creature in ExpandedSummoningCatalog.All
                .Where(value => PrimateRulesPolicy.IsPrimate(value.Key)))
            {
                Assertions.True(SummonIconCatalog.IsRegisteredSomewhere(creature),
                    creature.Key + " must be registered.");
                // The icon was registered before publication and did not move
                // at it: a withheld creature already owned its own face.
                Assertions.True(SummonIconCatalog.IsPublishedSomewhere(creature),
                    creature.Key + " must publish the icon it already owned.");
            }
        }

        /// <summary>
        /// The Ape's exact printed profile and its two-slam routine: two
        /// primary slams and nothing else.
        /// </summary>
        internal static void ApeProfileIsTwoPrimarySlams()
        {
            NaturalSummonProfile profile =
                ExpandedSummoningNaturalProfiles.For("ape");
            PrimateRulesProfile rules = PrimateRulesPolicy.For("ape");
            Assertions.Equal("Large", profile.Size, "Ape size changed.");
            Assertions.Equal("Animal", profile.HitDieClass, "Ape hit-die class changed.");
            Assertions.Equal(3, profile.HitDice, "Ape hit dice changed.");
            Assertions.Equal(15, profile.Strength, "Ape Strength changed.");
            Assertions.Equal(15, profile.Dexterity, "Ape Dexterity changed.");
            Assertions.Equal(14, profile.Constitution, "Ape Constitution changed.");
            Assertions.Equal(2, profile.Intelligence, "Ape Intelligence changed.");
            Assertions.Equal(12, profile.Wisdom, "Ape Wisdom changed.");
            Assertions.Equal(7, profile.Charisma, "Ape Charisma changed.");
            Assertions.Equal(30, profile.SpeedFeet, "Ape speed changed.");
            Assertions.Equal(3, profile.NaturalArmor, "Ape natural armor changed.");

            // Two primary slams: one in the primary hand, one additional limb,
            // and no secondary limb at all.
            Assertions.Equal("Slam1d6", profile.PrimaryWeapon,
                "Ape primary limb changed.");
            Assertions.True(profile.AdditionalWeapons.SequenceEqual(
                    new[] { "Slam1d6" }),
                "The Ape's second slam must be an ordinary additional limb.");
            Assertions.Equal(0, profile.AdditionalSecondaryWeapons.Count,
                "A secondary limb would make one slam a secondary attack.");
            Assertions.Equal(2, 1 + profile.AdditionalWeapons.Count,
                "The Ape has exactly two attacks.");
            foreach (string absent in new[] { "Bite", "Claw", "Tail", "Gore", "Sting" })
                Assertions.False(profile.PrimaryWeapon.StartsWith(absent,
                        StringComparison.Ordinal) ||
                    profile.AdditionalWeapons.Any(value => value.StartsWith(
                        absent, StringComparison.Ordinal)),
                    "The Ape prints no " + absent.ToLowerInvariant() + ".");
            Assertions.False(rules.HasRend, "The Ape prints no rend.");
            Assertions.False(profile.Facts.Any(value =>
                    value.IndexOf("Rend", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("Fear", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("Roar", StringComparison.Ordinal) >= 0),
                "No rend, chest-thump fear or roar carrier may be invented.");

            // Both slams add the whole Strength modifier, because neither is a
            // secondary attack and the creature has more than one limb.
            Assertions.Equal(2, PrimateRulesPolicy.PrimaryLimbDamageBonus(
                    (profile.Strength - 10) / 2),
                "Each printed slam is 1d6+2 at Strength 15.");

            // The printed Large footprint is Space 10 with Reach 10, so no
            // reduced-reach carrier; nothing printed grants a bonus against
            // trip, so no trip-defence carrier.
            Assertions.False(profile.Facts.Contains("ReducedReach"),
                "The Ape's printed reach is the ordinary Large reach.");
            Assertions.False(profile.Facts.Any(value => value.StartsWith(
                    "TripDefense", StringComparison.Ordinal)),
                "The Ape prints no bonus against trip.");
            // The third entry is not a printed feat. It is the carrier the
            // guarded review proved both apes need: the engine gives a
            // natural primary-hand weapon one and a half times Strength when
            // the secondary hand is empty, and the printed routine gives
            // every limb the plain modifier.
            Assertions.True(profile.Facts.SequenceEqual(new[] {
                    "GreatFortitude", "SkillFocusPerception",
                    "PrimateFullStrengthLimbs" }),
                "The Ape's printed feats are Great Fortitude and Skill Focus (Perception).");
            Assertions.Equal(13, rules.BaseHitPoints,
                "Printed 19 hit points are 3d8 racial dice plus a live Constitution contribution.");
        }

        /// <summary>
        /// The Dire Ape's exact printed profile: one primary bite and two
        /// primary claws, all at the same bonus and all at full Strength.
        /// </summary>
        internal static void DireApeProfileIsBiteAndTwoPrimaryClaws()
        {
            NaturalSummonProfile profile =
                ExpandedSummoningNaturalProfiles.For("dire-ape");
            PrimateRulesProfile rules = PrimateRulesPolicy.For("dire-ape");
            Assertions.Equal("Large", profile.Size, "Dire Ape size changed.");
            Assertions.Equal("Animal", profile.HitDieClass,
                "Dire Ape hit-die class changed.");
            Assertions.Equal(4, profile.HitDice, "Dire Ape hit dice changed.");
            Assertions.Equal(19, profile.Strength, "Dire Ape Strength changed.");
            Assertions.Equal(15, profile.Dexterity, "Dire Ape Dexterity changed.");
            Assertions.Equal(16, profile.Constitution,
                "Dire Ape Constitution changed.");
            Assertions.Equal(2, profile.Intelligence,
                "Dire Ape Intelligence changed.");
            Assertions.Equal(12, profile.Wisdom, "Dire Ape Wisdom changed.");
            Assertions.Equal(7, profile.Charisma, "Dire Ape Charisma changed.");
            Assertions.Equal(30, profile.SpeedFeet, "Dire Ape speed changed.");
            Assertions.Equal(4, profile.NaturalArmor,
                "Dire Ape natural armor changed.");
            Assertions.Equal(18, rules.BaseHitPoints,
                "Printed 30 hit points are 4d8 racial dice plus a live Constitution contribution.");

            // Creature-owned weapons, not the shared native ones. The guarded
            // review measured the shared 1d6 bite and 1d4 claw scaling up a
            // step for a Large wielder, which is not the printed entry.
            Assertions.Equal("DireApeBite1d6", profile.PrimaryWeapon,
                "Dire Ape primary limb changed.");
            Assertions.True(profile.AdditionalWeapons.SequenceEqual(
                    new[] { "DireApeClaw1d4", "DireApeClaw1d4" }),
                "Both claws must be ordinary additional limbs so all three attacks are primary.");
            Assertions.Equal(0, profile.AdditionalSecondaryWeapons.Count,
                "A secondary limb would drop a claw to a secondary attack and off the printed bonus.");
            Assertions.Equal(3, 1 + profile.AdditionalWeapons.Count,
                "The Dire Ape has exactly three attacks.");
            Assertions.Equal(4, PrimateRulesPolicy.PrimaryLimbDamageBonus(
                    (profile.Strength - 10) / 2),
                "Bite 1d6+4 and each claw 1d4+4 at Strength 19.");
            Assertions.True(profile.Facts.SequenceEqual(new[] {
                    "IronWill", "SkillFocusPerception", "DireApeRend",
                    "PrimateFullStrengthLimbs" }),
                "The Dire Ape's printed feats are Iron Will and Skill Focus (Perception), beside its rend.");
            Assertions.False(profile.Facts.Any(value =>
                    value.IndexOf("Grab", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("Grapple", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("Pin", StringComparison.Ordinal) >= 0 ||
                    value.IndexOf("Armor", StringComparison.Ordinal) >= 0),
                "The flavour text's grappling remark is not a printed special attack.");
            Assertions.False(profile.Facts.Contains("ReducedReach"),
                "The Dire Ape's printed reach is the ordinary Large reach.");
        }

        /// <summary>
        /// The printed rend line, reached by derivation. The constant 1d4+6 is
        /// the baseline a Strength-19 creature reproduces, not the
        /// implementation.
        /// </summary>
        internal static void RendDamageFollowsLiveStrength()
        {
            PrimateRulesProfile rules = PrimateRulesPolicy.For("dire-ape");
            Assertions.True(rules.HasRend, "The Dire Ape prints a rend.");
            Assertions.Equal(1, rules.RendDiceCount, "Rend dice count changed.");
            Assertions.Equal(4, rules.RendDieSides, "Rend die size changed.");
            Assertions.Equal(4, rules.StrengthModifier,
                "Strength 19 is a +4 modifier.");
            Assertions.Equal(6, rules.RendBonus,
                "Rend adds one and a half times the Strength modifier.");
            Assertions.Equal("1d4+6", rules.RendDamage,
                "The printed baseline rend line changed.");
            Assertions.False(rules.RendHasTargetSizeGate,
                "Nothing printed gates a rend on target size.");

            // The derivation must agree with the engine's own rend carrier,
            // which computes (int)(Strength.Bonus * 1.5f), across every
            // modifier the game can produce. Agreement is what lets the native
            // carrier own the damage while this policy owns the contract.
            for (int bonus = -10; bonus <= 40; bonus++)
                Assertions.Equal((int)(bonus * 1.5f),
                    PrimateRulesPolicy.RendDamageBonus(bonus),
                    "Rend bonus diverges from the engine at Strength bonus " +
                    bonus + ".");

            // Shared with constrict so the two cannot drift apart.
            Assertions.Equal(
                ExpandedSummoningSpecialProfiles.ConstrictBonus(4),
                PrimateRulesPolicy.RendDamageBonus(4),
                "Rend and constrict must share the one-and-a-half derivation.");
        }

        /// <summary>
        /// A rend needs both claws on one target in one sequence. Every
        /// nonqualifying shape the contract names is checked here.
        /// </summary>
        internal static void RendRequiresBothClawsOnOneTargetInOneSequence()
        {
            object sequence = new object();
            object target = new object();
            object other = new object();

            // One claw alone never rends.
            var tracker = new ClawRendTracker(2);
            Assertions.False(tracker.TryArm(sequence, 1, target),
                "The first claw cannot rend with nothing before it.");
            tracker.RecordOutcome(sequence, 1, target, true);
            Assertions.False(tracker.HasEmitted,
                "A single claw hit emits no rend.");

            // The same claw striking twice is not two claws.
            Assertions.False(tracker.TryArm(sequence, 1, target),
                "One claw hitting twice is not a rend.");

            // The second claw on the same target in the same sequence rends.
            Assertions.True(tracker.TryArm(sequence, 2, target),
                "Both claws on one target in one sequence must rend.");
            Assertions.True(tracker.IsArmed, "The qualifying claw must be armed.");
            tracker.RecordOutcome(sequence, 2, target, true);
            Assertions.True(tracker.HasEmitted, "The armed rend must resolve.");
            Assertions.False(tracker.IsArmed,
                "A resolved rend leaves nothing pending.");

            // Exactly one rend per qualifying sequence.
            Assertions.False(tracker.TryArm(sequence, 1, target),
                "A sequence rends at most once.");
            Assertions.False(tracker.TryArm(sequence, 2, target),
                "A sequence rends at most once, from either claw.");

            // Claws on different targets never rend.
            var split = new ClawRendTracker(2);
            split.RecordOutcome(sequence, 1, target, true);
            Assertions.False(split.TryArm(sequence, 2, other),
                "Claws that hit different creatures do not rend.");
            Assertions.False(split.HasEmitted, "No rend may be emitted.");
            // ...and the first claw's recorded hit is still the one it made, so
            // a later claw on that target is still a legitimate rend.
            Assertions.True(split.TryArm(sequence, 2, target),
                "A second claw that does reach the first claw's target still rends.");

            // A missed qualifying claw emits nothing and leaves nothing armed.
            var missed = new ClawRendTracker(2);
            missed.RecordOutcome(sequence, 1, target, true);
            Assertions.True(missed.TryArm(sequence, 2, target),
                "The second claw qualifies before its roll is known.");
            missed.RecordOutcome(sequence, 2, target, false);
            Assertions.False(missed.HasEmitted, "A missed claw rends for nothing.");
            Assertions.False(missed.IsArmed, "A miss leaves nothing armed.");

            // A claw that misses records no hit, so it cannot pair later.
            var onlyMisses = new ClawRendTracker(2);
            onlyMisses.RecordOutcome(sequence, 1, target, false);
            Assertions.False(onlyMisses.TryArm(sequence, 2,
                    target), "A missed claw is not a hit to pair with.");

            // An attack with no live command is not a sequence and never rends.
            var replayed = new ClawRendTracker(2);
            replayed.RecordOutcome(sequence, 1, target, true);
            Assertions.False(replayed.TryArm(null, 2, target),
                "An attack with no live attack command never rends.");

            // The decision function itself refuses every missing operand.
            Assertions.False(PrimateRulesPolicy.ShouldRend(false, true, true, false, false),
                "A bite or any other limb never rends.");
            Assertions.False(PrimateRulesPolicy.ShouldRend(true, false, true, false, false),
                "No live sequence, no rend.");
            Assertions.False(PrimateRulesPolicy.ShouldRend(true, true, false, false, false),
                "Without the other claw on this target, no rend.");
            Assertions.False(PrimateRulesPolicy.ShouldRend(true, true, true, true, false),
                "An already-armed sequence does not arm twice.");
            Assertions.False(PrimateRulesPolicy.ShouldRend(true, true, true, false, true),
                "An already-emitted sequence does not rend again.");
            Assertions.True(PrimateRulesPolicy.ShouldRend(true, true, true, false, false),
                "The one qualifying shape must rend.");
        }

        /// <summary>
        /// No rend state crosses a command, a target, a turn or a lifetime
        /// boundary. Save/load, death, dismissal and expiry all arrive here as
        /// a tracker that no longer exists, which is why the tracker holds no
        /// game state and is never serialized.
        /// </summary>
        internal static void NoStaleRendCrossesACommandOrTurn()
        {
            object first = new object();
            object second = new object();
            object target = new object();

            var tracker = new ClawRendTracker(2);
            tracker.RecordOutcome(first, 1, target, true);
            Assertions.True(ReferenceEquals(first, tracker.Sequence),
                "The tracker must follow the command it observed.");
            // A new command is a new sequence: the previous claw hit is gone.
            Assertions.False(tracker.TryArm(second, 2, target),
                "A claw hit in an earlier command cannot pair with a later one.");
            Assertions.True(ReferenceEquals(second, tracker.Sequence),
                "The tracker must move to the new command.");
            Assertions.False(tracker.HasEmitted, "Nothing may be emitted.");

            // An armed rend does not survive into the next command either.
            var armed = new ClawRendTracker(2);
            armed.RecordOutcome(first, 1, target, true);
            Assertions.True(armed.TryArm(first, 2, target),
                "The qualifying claw arms inside its own command.");
            armed.RecordOutcome(second, 1, target, true);
            Assertions.False(armed.IsArmed,
                "An arming cannot cross a command boundary.");
            Assertions.False(armed.HasEmitted,
                "A crossed arming cannot resolve as a rend.");

            // Reset is what a new round, a fact turning off and a vanished unit
            // all reduce to.
            var reset = new ClawRendTracker(2);
            reset.RecordOutcome(first, 1, target, true);
            reset.Reset();
            Assertions.True(reset.Sequence == null,
                "A reset tracker follows no sequence.");
            Assertions.False(reset.TryArm(first, 2, target),
                "A reset tracker remembers no earlier claw hit.");

            // A null sequence resets rather than remembering.
            var cleared = new ClawRendTracker(2);
            cleared.RecordOutcome(first, 1, target, true);
            cleared.RecordOutcome(null, 1, target, true);
            Assertions.True(cleared.Sequence == null,
                "An uncommanded attack clears the sequence rather than joining it.");

            Assertions.False(typeof(ClawRendTracker).IsSerializable,
                "The rend tracker must never be serialized into a save.");
        }

        /// <summary>
        /// Exact profile-controlled ranks, and an honest climb omission. The
        /// generic builder's priority-list allocation is switched off for both
        /// apes, so nothing can add a rank nobody printed.
        /// </summary>
        internal static void LandSkillsUseExactRanksAndOmitClimbHonestly()
        {
            foreach (var row in new[] {
                new { Key = "ape", Mobility = 1, Perception = 1, Stealth = 0,
                    Skills = new[] { "Mobility", "Perception" }, Climb = 14 },
                new { Key = "dire-ape", Mobility = 1, Perception = 1, Stealth = 1,
                    Skills = new[] { "Mobility", "Perception", "Stealth" }, Climb = 16 } })
            {
                PrimateRulesProfile rules = PrimateRulesPolicy.For(row.Key);
                NaturalSummonProfile profile =
                    ExpandedSummoningNaturalProfiles.For(row.Key);
                Assertions.Equal(row.Mobility, rules.MobilityRanks,
                    row.Key + " Mobility ranks changed.");
                Assertions.Equal(row.Perception, rules.PerceptionRanks,
                    row.Key + " Perception ranks changed.");
                Assertions.Equal(row.Stealth, rules.StealthRanks,
                    row.Key + " Stealth ranks changed.");
                Assertions.True(profile.Skills.SequenceEqual(row.Skills),
                    row.Key + " must name exactly the skills it represents.");
                Assertions.Equal(row.Climb, rules.PrintedClimbSkill,
                    row.Key + " printed Climb total changed.");

                // Every printed rank is accounted for: the represented ranks
                // plus the one that bought Climb equal the hit dice, which is
                // what an Intelligence-2 animal gets.
                Assertions.Equal(profile.HitDice,
                    rules.MobilityRanks + rules.PerceptionRanks +
                    rules.StealthRanks + 1,
                    row.Key + " must account for every printed rank, including the omitted Climb rank.");

                // The omitted rank is never reallocated, and no skill stands in
                // for Climb.
                Assertions.False(profile.Skills.Contains("Athletics"),
                    row.Key + " must not substitute Athletics for Climb.");
                Assertions.Equal(1, rules.MobilityRanks,
                    row.Key + " must not inflate Mobility to stand in for Climb.");

                // The allocation itself is one-time and fails closed.
                int mobility = 0, perception = 0, stealth = 0;
                PrimateRulesPolicy.AllocateLandRanks(row.Key, ref mobility,
                    ref perception, ref stealth);
                Assertions.Equal(row.Mobility, mobility,
                    row.Key + " allocated Mobility changed.");
                Assertions.Equal(row.Perception, perception,
                    row.Key + " allocated Perception changed.");
                Assertions.Equal(row.Stealth, stealth,
                    row.Key + " allocated Stealth changed.");
                bool rejected = false;
                try
                {
                    int already = 1, p = 0, s = 0;
                    PrimateRulesPolicy.AllocateLandRanks(row.Key, ref already,
                        ref p, ref s);
                }
                catch (InvalidOperationException) { rejected = true; }
                Assertions.True(rejected,
                    row.Key + " rank allocation must refuse a donor or repeated rank.");
            }

            Assertions.Equal(8, PrimateRulesPolicy.PrintedRacialClimbBonus,
                "The printed racial climb bonus is recorded so the omission names what is missing.");
        }

        /// <summary>
        /// The accepted engine limitations are stated on both creatures and
        /// nothing is substituted for them.
        /// </summary>
        internal static void OmittedSensesAndMovementAreRecordedWithoutSubstitutes()
        {
            foreach (string key in new[] { "ape", "dire-ape" })
            {
                NaturalSummonProfile profile =
                    ExpandedSummoningNaturalProfiles.For(key);
                string deviations = string.Join(" ", profile.Deviations.ToArray());
                Assertions.True(deviations.Contains(
                        "PASSIVE_CREATURE_SENSES_UNMODELED"),
                    key + " must record the accepted passive-sense limitation.");
                Assertions.True(deviations.Contains("climb"),
                    key + " must record the climb omission.");
                Assertions.False(profile.Facts.Any(value =>
                        value.IndexOf("Blindsense", StringComparison.Ordinal) >= 0 ||
                        value.IndexOf("Blindsight", StringComparison.Ordinal) >= 0 ||
                        value.IndexOf("Vision", StringComparison.Ordinal) >= 0 ||
                        value.IndexOf("Darkvision", StringComparison.Ordinal) >= 0),
                    key + " must substitute no sense for low-light vision or scent.");
                Assertions.Equal(30, profile.SpeedFeet,
                    key + " uses its printed land speed, not its climb speed.");
            }
        }

        /// <summary>
        /// The four new identities, their types, and the fact that no existing
        /// identity moved. The ledger is append-only and every historical entry
        /// keeps its exact GUID.
        /// </summary>
        internal static void NewIdentitiesAppendWithoutMovingAnything()
        {
            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(102, ExpandedSummoningIdentityCatalog.UnitCount,
                "Unit identity count changed.");
            Assertions.Equal(1056,
                ExpandedSummoningIdentityCatalog.LogicalAbilityCount,
                "Logical ability identity count changed.");
            Assertions.Equal(306,
                ExpandedSummoningIdentityCatalog.TemplatedPlacementCount,
                "Templated placement count changed.");
            Assertions.Equal(612,
                ExpandedSummoningIdentityCatalog.TemplateExecutionAbilityCount,
                "Template execution identity count changed.");
            Assertions.Equal(225,
                ExpandedSummoningIdentityCatalog.SpecialIdentityCount,
                "Creature-owned identity count changed.");
            Assertions.Equal(2037,
                ExpandedSummoningIdentityCatalog.FoundationIdentityCount,
                "Foundation identity count changed.");
            Assertions.Equal(2037, identities.Count,
                "The built identity catalog must match its own invariant.");

            foreach (var row in new[] {
                new { Symbol = "KMG.Summoning.Natural.Slam1d6", Type = "BlueprintItemWeapon" },
                new { Symbol = "KMG.Summoning.Natural.Ape.UnitType", Type = "BlueprintUnitType" },
                new { Symbol = "KMG.Summoning.Natural.DireApe.UnitType", Type = "BlueprintUnitType" },
                new { Symbol = "KMG.Summoning.Special.DireApe.Rend", Type = "BlueprintFeature" } })
            {
                SummoningIdentitySpec identity = identities.SingleOrDefault(
                    value => value.Symbol == row.Symbol);
                Assertions.True(identity != null,
                    "Missing Sprint 18 identity: " + row.Symbol);
                Assertions.Equal(row.Type, identity.PlannedType,
                    "Wrong planned type for " + row.Symbol);
            }
            Assertions.Equal(identities.Count,
                identities.Select(value => value.Symbol)
                    .Distinct(StringComparer.Ordinal).Count(),
                "Identity symbols must stay unique.");

            // Both apes own a donor, as every creature must, and the donor is
            // recorded as a borrowed body until the original models land.
            foreach (string key in new[] { "ape", "dire-ape" })
            {
                SummonDonorSpec donor = ExpandedSummoningDonorCatalog.For(key);
                Assertions.True(donor != null && donor.Guid.Length == 32,
                    key + " needs exactly one donor.");
                Assertions.False(donor.DedicatedSummon,
                    key + " borrows a body donor rather than reusing a dedicated summon.");
            }
            ExpandedSummoningDonorCatalog.Validate();
            Assertions.True(ExpandedSummoningBaselineInventory
                    .ProxyVisualCreatures.Contains("ape<Troll") &&
                ExpandedSummoningBaselineInventory.ProxyVisualCreatures
                    .Contains("dire-ape<Troll"),
                "Both apes must be recorded as borrowed-body proxies until their original models are authored.");
            // The census chose this rig on evidence: of 28 surveyed Large rigs
            // the Troll is the only one with real hands, a separate jaw and
            // toes, which is what an ape body needs to be authored against.
            foreach (string key in new[] { "ape", "dire-ape" })
                Assertions.Equal("b98735a1737ae494dbe5cbeca1c7c083",
                    ExpandedSummoningDonorCatalog.For(key).Guid,
                    key + " must use the census-chosen donor rig.");
        }
    }
}
