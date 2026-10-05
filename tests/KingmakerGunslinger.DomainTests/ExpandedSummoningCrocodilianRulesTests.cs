using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The crocodilians' signature rules, checked at the policy layer where
    /// the derivations live.
    ///
    /// <para>These are deliberately not source-text checks: every assertion
    /// runs the same functions the runtime calls, with the inputs a live
    /// situation would produce. What they cannot do is prove the engine
    /// applies them, which is what the guarded runtime review exists for.</para>
    /// </summary>
    internal static class ExpandedSummoningCrocodilianRulesTests
    {
        internal static void SprintBrainKeepsNaturalActionsAndOneEngagementGate()
        {
            var attack = new object();
            var movement = new object();
            var sprint = new object();
            object[] natural = { attack, movement };
            object[] result = CrocodilianRulesPolicy.AppendSprintAction(natural, sprint);
            Assertions.True(result.Length == 3 && ReferenceEquals(result[0], attack) &&
                ReferenceEquals(result[1], movement) && ReferenceEquals(result[2], sprint) &&
                natural.Length == 2 && natural[0] == attack && natural[1] == movement &&
                !ReferenceEquals(result, natural), "Natural entries retain identity/order and are never mutated.");
            Assertions.True(CrocodilianRulesPolicy.AppendSprintAction<object>(null, sprint)
                .SequenceEqual(new[] { sprint }), "Empty native list is handled without inventing actions.");
            bool duplicateRejected = false;
            try { CrocodilianRulesPolicy.AppendSprintAction(result, sprint); }
            catch (InvalidOperationException) { duplicateRejected = true; }
            Assertions.True(duplicateRejected, "A repeated append cannot queue a second Sprint action.");
            Assertions.True(CrocodilianRulesPolicy.SprintAiEngagedScore == 0f &&
                CrocodilianRulesPolicy.SprintAiFreeScore == 1f, "Engaged units must not score Sprint.");
            var identity = ExpandedSummoningIdentityCatalog.Build().Single(
                value => value.Symbol == CrocodilianRulesPolicy.SprintNotEngagedSymbol);
            Assertions.Equal("IsEngagedConsideration", identity.PlannedType,
                "Use the existing native scorer, not a custom global AI subsystem.");
        }

        internal static void LandSkillsUseExactRanksWithoutMobility()
        {
            foreach (var row in new[] {
                new { Key = "crocodile", PerceptionRanks = 1, StealthRanks = 2,
                    Perception = 8, Stealth = 5, SizePenalty = -4 },
                new { Key = "dire-crocodile", PerceptionRanks = 6, StealthRanks = 6,
                    Perception = 14, Stealth = 0, SizePenalty = -12 } })
            {
                CrocodilianRulesProfile rules = CrocodilianRulesPolicy.For(row.Key);
                NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles.For(row.Key);
                if (rules.PerceptionRanks != row.PerceptionRanks ||
                        rules.StealthRanks != row.StealthRanks || rules.MobilityRanks != 0 ||
                        rules.PerceptionRanks + rules.StealthRanks != profile.HitDice ||
                        !profile.Skills.SequenceEqual(new[] { "Perception", "Stealth" }))
                    throw new InvalidOperationException("Incorrect land skill allocation: " + row.Key);
                if (!profile.Facts.Contains("SkillFocusPerception") ||
                        !profile.Facts.Contains("SkillFocusStealth"))
                    throw new InvalidOperationException("Printed Skill Focus facts must remain native.");
                int perception = rules.PerceptionRanks + 3 + profile.Wisdom / 2 - 5 +
                    (rules.PerceptionRanks >= 10 ? 6 : 3);
                int stealth = rules.StealthRanks + 3 + profile.Dexterity / 2 - 5 +
                    (rules.StealthRanks >= 10 ? 6 : 3) + row.SizePenalty;
                if (perception != row.Perception || stealth != row.Stealth)
                    throw new InvalidOperationException("Printed land totals do not derive: " + row.Key);
                if (profile.Deviations.Any(value => value.Contains("water-only Stealth")) == false)
                    throw new InvalidOperationException("Aquatic-only skill omission must be explicit.");
            }
        }

        internal static void DeathRollAdjustsOnlyTheCapturedBaseBite()
        {
            // A supplemental physical chunk is just as unsuitable as an
            // elemental rider. Reordering either ahead of the captured bite
            // must not change which chunk receives the extra half.
            var bite = new DamageChunk { Bonus = 4 };
            var fire = new DamageChunk { Bonus = 2 };
            var supplementalPhysical = new DamageChunk { Bonus = 4 };
            foreach (DamageChunk[] chunks in new[] {
                new[] { bite, fire, supplementalPhysical },
                new[] { fire, bite, supplementalPhysical },
                new[] { supplementalPhysical, fire, bite } })
            {
                bite.Bonus = 4;
                int index = CrocodilianRulesPolicy.BaseBiteIndex(chunks, bite);
                if (index < 0 || !ReferenceEquals(chunks[index], bite))
                    throw new InvalidOperationException("The base bite lost its identity.");
                chunks[index].Bonus += CrocodilianRulesPolicy.DeathRollExtraHalf(4);
                if (bite.Bonus != 6 || fire.Bonus != 2 ||
                        supplementalPhysical.Bonus != 4)
                    throw new InvalidOperationException("Strength reached supplemental damage.");
            }
            foreach (DamageChunk[] invalid in new[] {
                new[] { fire, supplementalPhysical },
                new[] { bite, fire, bite }, new DamageChunk[0] })
                if (CrocodilianRulesPolicy.BaseBiteIndex(invalid, bite) != -1)
                    throw new InvalidOperationException("Missing/duplicated bite must fail closed.");
            if (CrocodilianRulesPolicy.BaseBiteIndex<DamageChunk>(null, bite) != -1 ||
                    CrocodilianRulesPolicy.BaseBiteIndex(new[] { bite }, null) != -1)
                throw new InvalidOperationException("Absent capture must fail closed.");
        }

        private sealed class DamageChunk { internal int Bonus; }

        /// <summary>
        /// Death roll damage is derived, and is not the ordinary bite.
        ///
        /// <para>Both printed blocks separate the two, and the separation is a
        /// rule rather than a coincidence: an ordinary natural attack adds the
        /// Strength modifier, a death roll adds one and a half times it. If
        /// this is ever implemented by replaying the bite, both creatures lose
        /// damage - the Crocodile two points and the Dire Crocodile six - and
        /// nothing else in the project would notice.</para>
        /// </summary>
        internal static void DeathRollDamageIsDerivedAndNotTheBite()
        {
            CrocodilianRulesPolicy.Validate();

            CrocodilianRulesProfile crocodile =
                CrocodilianRulesPolicy.For("crocodile");
            if (crocodile.DeathRollDamage != "1d8+6")
                throw new InvalidOperationException(
                    "The Crocodile's death roll is 1d8+6, not " +
                    crocodile.DeathRollDamage + ".");
            CrocodilianRulesProfile dire =
                CrocodilianRulesPolicy.For("dire-crocodile");
            if (dire.DeathRollDamage != "3d6+19")
                throw new InvalidOperationException(
                    "The Dire Crocodile's death roll is 3d6+19, not " +
                    dire.DeathRollDamage + ".");

            // The ordinary bite is unchanged, and is a different number. The
            // profiles are where the bite lives, so this reads them rather
            // than restating the stat block.
            foreach (var expected in new[] {
                new { Key = "crocodile", Bite = "Bite1d8", Bonus = 4,
                    DeathRoll = 6 },
                new { Key = "dire-crocodile", Bite = "Bite3d6", Bonus = 13,
                    DeathRoll = 19 } })
            {
                NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                    .For(expected.Key);
                if (profile.PrimaryWeapon != expected.Bite)
                    throw new InvalidOperationException(
                        expected.Key + " must still bite with " +
                        expected.Bite + ", not " + profile.PrimaryWeapon + ".");
                CrocodilianRulesProfile rules =
                    CrocodilianRulesPolicy.For(expected.Key);
                // The bite's bonus is the Strength modifier; the death roll's
                // is one and a half times it. Equal would mean the death roll
                // had been implemented as a second bite.
                if (rules.StrengthModifier != expected.Bonus)
                    throw new InvalidOperationException(
                        expected.Key + "'s Strength modifier is " +
                        rules.StrengthModifier + ", not " + expected.Bonus +
                        ".");
                if (rules.DeathRollBonus != expected.DeathRoll)
                    throw new InvalidOperationException(
                        expected.Key + "'s death roll bonus is " +
                        rules.DeathRollBonus + ", not " + expected.DeathRoll +
                        ".");
                if (rules.DeathRollBonus == rules.StrengthModifier)
                    throw new InvalidOperationException(
                        expected.Key + "'s death roll adds the same bonus as " +
                        "its bite, which means it is replaying the bite.");
            }

            // The derivation itself, not the two creatures that happen to use
            // it: one and a half times, rounded down, for any Strength.
            foreach (var pair in new[] {
                new { Modifier = 0, Bonus = 0 },
                new { Modifier = 1, Bonus = 1 },
                new { Modifier = 3, Bonus = 4 },
                new { Modifier = 4, Bonus = 6 },
                new { Modifier = 13, Bonus = 19 } })
                if (ExpandedSummoningSpecialProfiles.ConstrictBonus(
                        pair.Modifier) != pair.Bonus)
                    throw new InvalidOperationException(
                        "One and a half times " + pair.Modifier + " is " +
                        pair.Bonus + ".");
        }

        /// <summary>
        /// The death roll follows the creature's live Strength, not the
        /// Strength its profile was written with.
        ///
        /// <para>The first implementation built the rider's damage from the
        /// profile, which reproduces the printed line on an unmodified
        /// creature and is wrong for every other one: a buffed or weakened
        /// crocodile would have death rolled for exactly what the stat block
        /// says while its bite did something else. These are the arithmetic
        /// cases; that the runtime actually reads live stats is a guarded
        /// runtime assertion, because only the engine can answer it.</para>
        /// </summary>
        internal static void DeathRollFollowsLiveStrength()
        {
            // The printed creatures, as the baseline the unmodified case must
            // reproduce. Both come out of the live derivation too, which is
            // what makes the baseline a contract rather than a separate path.
            foreach (var creature in new[] {
                new { Key = "crocodile", Modifier = 4, Bonus = 6 },
                new { Key = "dire-crocodile", Modifier = 13, Bonus = 19 } })
            {
                CrocodilianRulesProfile rules =
                    CrocodilianRulesPolicy.For(creature.Key);
                if (rules.StrengthModifier != creature.Modifier)
                    throw new InvalidOperationException(
                        creature.Key + " has a Strength modifier of " +
                        rules.StrengthModifier + ".");
                if (CrocodilianRulesPolicy.DeathRollBonusFor(
                        creature.Modifier) != creature.Bonus ||
                        rules.DeathRollBonus != creature.Bonus)
                    throw new InvalidOperationException(
                        "The live derivation and the baseline must agree at " +
                        "the printed Strength for " + creature.Key + ".");
            }

            // A Strength increase raises both, and keeps the one-times versus
            // one-and-a-half distinction. A belt of +4 Strength is +2 modifier.
            foreach (var raised in new[] {
                new { Modifier = 6, Bite = 6, DeathRoll = 9 },
                new { Modifier = 15, Bite = 15, DeathRoll = 22 } })
            {
                if (CrocodilianRulesPolicy.DeathRollBonusFor(raised.Modifier)
                        != raised.DeathRoll)
                    throw new InvalidOperationException(
                        "At Strength modifier " + raised.Modifier +
                        " the death roll adds " + raised.DeathRoll + ".");
                if (CrocodilianRulesPolicy.DeathRollBonusFor(raised.Modifier)
                        <= raised.Bite)
                    throw new InvalidOperationException(
                        "The death roll must stay above the bite as Strength " +
                        "rises, or it has become the bite.");
            }

            // A Strength penalty lowers both. The extra half is not applied to
            // a penalty - one and a half times multiplies a bonus - but the
            // penalty is already inside the live bite, so the death roll falls
            // with it rather than staying at the printed line.
            foreach (var weakened in new[] {
                new { Modifier = 0, DeathRoll = 0 },
                new { Modifier = -1, DeathRoll = -1 },
                new { Modifier = -5, DeathRoll = -5 } })
                if (CrocodilianRulesPolicy.DeathRollBonusFor(weakened.Modifier)
                        != weakened.DeathRoll)
                    throw new InvalidOperationException(
                        "At Strength modifier " + weakened.Modifier +
                        " the death roll adds " + weakened.DeathRoll +
                        ", because a penalty applies once.");
            if (CrocodilianRulesPolicy.DeathRollExtraHalf(-5) != 0)
                throw new InvalidOperationException(
                    "A Strength penalty gets no extra half.");

            // The extra half is exactly the difference between the two, at
            // every modifier, which is the whole rule in one line.
            for (int modifier = -6; modifier <= 20; modifier++)
                if (CrocodilianRulesPolicy.DeathRollBonusFor(modifier) -
                        modifier !=
                        CrocodilianRulesPolicy.DeathRollExtraHalf(modifier))
                    throw new InvalidOperationException(
                        "The death roll is the bite's Strength plus the extra " +
                        "half at modifier " + modifier + ".");
        }

        /// <summary>
        /// Every way a death roll is refused, one at a time.
        /// </summary>
        internal static void DeathRollIsRefusedWhereTheRulesRefuseIt()
        {
            // The ordinary case: held since the round began, maintain
            // succeeded, target no larger than the crocodilian.
            if (!CrocodilianRulesPolicy.ShouldDeathRollOnMaintain(true, true,
                    1, true, true))
                throw new InvalidOperationException(
                    "A held target of an allowed size on a successful " +
                    "maintain must be death rolled.");

            foreach (var refusal in new[] {
                new { Why = "a creature without the ability",
                    Has = false, Owned = true, Rounds = 1, Success = true,
                    Size = true },
                new { Why = "a target the crocodilian does not hold",
                    Has = true, Owned = false, Rounds = 1, Success = true,
                    Size = true },
                new { Why = "a target seized this instant rather than held " +
                        "since the round began",
                    Has = true, Owned = true, Rounds = 0, Success = true,
                    Size = true },
                new { Why = "a failed maintain check",
                    Has = true, Owned = true, Rounds = 1, Success = false,
                    Size = true },
                new { Why = "a target larger than the crocodilian",
                    Has = true, Owned = true, Rounds = 1, Success = true,
                    Size = false } })
                if (CrocodilianRulesPolicy.ShouldDeathRollOnMaintain(
                        refusal.Has, refusal.Owned, refusal.Rounds,
                        refusal.Success, refusal.Size))
                    throw new InvalidOperationException(
                        "A death roll must be refused for " + refusal.Why +
                        ".");

            // Own size or smaller, which is a different threshold from
            // swallow whole's one category smaller. A Large crocodile death
            // rolls a Large foe; it cannot swallow one.
            foreach (var size in new[] {
                new { Target = 5, Holder = 5, Allowed = true },
                new { Target = 4, Holder = 5, Allowed = true },
                new { Target = 6, Holder = 5, Allowed = false } })
                if (ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(
                        size.Target, size.Holder,
                        CrocodilianRulesPolicy.For("crocodile")
                            .DeathRollTargetSizeDelta) != size.Allowed)
                    throw new InvalidOperationException(
                        "Death roll size eligibility is wrong for target " +
                        size.Target + " against holder " + size.Holder + ".");

            // A death roll keeps the hold: a rider that released the target
            // would make a signature ability a way out of a grapple.
            if (!CrocodilianRulesPolicy.KeepsGrappleAfterDeathRoll)
                throw new InvalidOperationException(
                    "A death roll maintains the grapple.");
        }

        /// <summary>
        /// A natural 1 fails and a natural 20 succeeds, through the project's
        /// own maneuver rule rather than a second copy of it.
        /// </summary>
        internal static void DeathRollObeysTheManeuverRollRules()
        {
            foreach (var roll in new[] {
                new { Natural = 20, Sum = false, Succeeds = true },
                new { Natural = 1, Sum = true, Succeeds = false },
                new { Natural = 11, Sum = true, Succeeds = true },
                new { Natural = 11, Sum = false, Succeeds = false } })
            {
                bool maintain = ExpandedSummoningSpecialProfiles
                    .IsSummonManeuverSuccess(roll.Natural, roll.Sum);
                if (maintain != roll.Succeeds)
                    throw new InvalidOperationException(
                        "A natural " + roll.Natural + " with sum success " +
                        roll.Sum + " must " + (roll.Succeeds ? "" : "not ") +
                        "succeed.");
                // And the rider follows the check rather than the sum.
                if (CrocodilianRulesPolicy.ShouldDeathRollOnMaintain(true,
                        true, 1, maintain, true) != roll.Succeeds)
                    throw new InvalidOperationException(
                        "The death roll must follow the maneuver rule's " +
                        "answer, not the raw sum.");
            }
        }

        /// <summary>
        /// One successful grapple action resolves exactly one rider.
        ///
        /// <para>The Dire Crocodile has both a death roll and a swallow, and
        /// the thing to prevent is a single successful check applying both.
        /// The selector returns one value, so there is no state in which a
        /// caller can act on two - and this walks the cases that decide which
        /// one it is.</para>
        /// </summary>
        internal static void OneMaintainResolvesExactlyOneRider()
        {
            if (CrocodilianRulesPolicy.MaintainAdaptation !=
                    "SWALLOW_ELIGIBLE_TARGET_ELSE_DEATH_ROLL")
                throw new InvalidOperationException("The owner-authorized policy must be named.");
            int lastRound = -1;
            if (CrocodilianRulesPolicy.TryClaimMaintainRound(0, ref lastRound) ||
                    !CrocodilianRulesPolicy.TryClaimMaintainRound(1, ref lastRound) ||
                    CrocodilianRulesPolicy.TryClaimMaintainRound(1, ref lastRound) ||
                    !CrocodilianRulesPolicy.TryClaimMaintainRound(2, ref lastRound) ||
                    CrocodilianRulesPolicy.TryClaimMaintainRound(1, ref lastRound))
                throw new InvalidOperationException("Replay must not resolve another maintain check.");
            // No success, no rider - whatever the creature can do.
            if (CrocodilianRulesPolicy.SelectMaintainRider(false, true, 1,
                    true, true, true, true, true) !=
                    CrocodilianMaintainRider.None)
                throw new InvalidOperationException(
                    "A failed maintain resolves no rider.");
            if (CrocodilianRulesPolicy.SelectMaintainRider(true, false, 1,
                    true, true, true, true, true) !=
                    CrocodilianMaintainRider.None)
                throw new InvalidOperationException(
                    "A target the crocodilian does not hold resolves no rider.");
            if (CrocodilianRulesPolicy.SelectMaintainRider(true, true, 0,
                    true, true, true, true, true) !=
                    CrocodilianMaintainRider.None)
                throw new InvalidOperationException(
                    "A target seized this instant resolves no rider.");

            // The Crocodile cannot swallow, so an eligible hold is a death
            // roll and nothing else.
            if (CrocodilianRulesPolicy.SelectMaintainRider(true, true, 1,
                    true, true, false, false, false) !=
                    CrocodilianMaintainRider.DeathRoll)
                throw new InvalidOperationException(
                    "A creature without a swallow death rolls.");

            // The Dire Crocodile against a foe small enough to swallow: the
            // swallow, and therefore not the death roll.
            if (CrocodilianRulesPolicy.SelectMaintainRider(true, true, 1,
                    true, true, true, true, true) !=
                    CrocodilianMaintainRider.SwallowWhole)
                throw new InvalidOperationException(
                    "A swallowable held target is swallowed.");

            // The same creature against a foe of its own size: too large to
            // swallow, so the death roll is reachable rather than shadowed.
            if (CrocodilianRulesPolicy.SelectMaintainRider(true, true, 1,
                    true, true, true, false, true) !=
                    CrocodilianMaintainRider.DeathRoll)
                throw new InvalidOperationException(
                    "A target too large to swallow but no larger than the " +
                    "crocodilian is death rolled, which is what keeps both " +
                    "abilities reachable.");

            // Swallow unavailable for a reason other than size - the mouth is
            // occupied, say - falls back to the death roll.
            if (CrocodilianRulesPolicy.SelectMaintainRider(true, true, 1,
                    true, true, true, true, false) !=
                    CrocodilianMaintainRider.DeathRoll)
                throw new InvalidOperationException(
                    "An unavailable swallow falls back to the death roll.");

            // Neither available: nothing, rather than a default.
            if (CrocodilianRulesPolicy.SelectMaintainRider(true, true, 1,
                    false, false, true, false, true) !=
                    CrocodilianMaintainRider.None)
                throw new InvalidOperationException(
                    "With neither rider eligible the maintain is ordinary.");

            // Exactly one value, never a pair: every combination resolves to
            // one of the three, which is the structural guarantee.
            var seen = new System.Collections.Generic.HashSet<
                CrocodilianMaintainRider>();
            foreach (bool success in new[] { true, false })
            foreach (bool owned in new[] { true, false })
            foreach (int rounds in new[] { 0, 1 })
            foreach (bool hasDeath in new[] { true, false })
            foreach (bool deathSize in new[] { true, false })
            foreach (bool hasSwallow in new[] { true, false })
            foreach (bool swallowSize in new[] { true, false })
            foreach (bool swallowAvailable in new[] { true, false })
                seen.Add(CrocodilianRulesPolicy.SelectMaintainRider(success,
                    owned, rounds, hasDeath, deathSize, hasSwallow,
                    swallowSize, swallowAvailable));
            if (seen.Count != 3)
                throw new InvalidOperationException(
                    "The selector must reach exactly its three outcomes; it " +
                    "reached " + seen.Count + ".");
        }

        /// <summary>
        /// The Dire Crocodile's swallow numbers are its own, not the Purple
        /// Worm's.
        /// </summary>
        internal static void SwallowNumbersComeFromTheCreature()
        {
            // Printed contract numbers must never masquerade as implemented
            // interior mechanics. Pin the owner's activated decision as data.
            JObject state = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "EXPANDED-SUMMONING-PROGRAM-STATE.json")));
            const string limitation = "SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED";
            JToken decision = state["acceptedEngineLimitations"].Single(
                entry => (string)entry["label"] == limitation);
            if ((string)decision["mode"] != "unmodeled" ||
                    !decision["scope"].Values<string>().OrderBy(value => value)
                        .SequenceEqual(new[] { "dire-crocodile", "purple-worm" }) ||
                    ((string)decision["censusSha256"]).Length != 64 ||
                    !state["phase2Mission"]["ownerAcceptedEngineLimitations"]
                        .Values<string>().Contains(limitation))
                throw new InvalidOperationException(
                    "The accepted interior omission must cover both swallowers with census evidence.");
            CrocodilianRulesProfile dire =
                CrocodilianRulesPolicy.For("dire-crocodile");
            if (dire.SwallowDamage != "3d6+13")
                throw new InvalidOperationException(
                    "The printed swallow is 3d6+13, not " + dire.SwallowDamage +
                    ".");
            if (dire.SwallowInteriorArmorClass != 16 ||
                    dire.SwallowInteriorHitPoints != 13)
                throw new InvalidOperationException(
                    "The printed interior is AC 16 and 13 hit points.");
            // Its swallow bonus is the ordinary Strength modifier, where its
            // death roll's is one and a half times it. Two riders on one
            // creature with two different bonuses is exactly the kind of
            // detail a shared constant would flatten.
            if (dire.SwallowBonus != dire.StrengthModifier ||
                    dire.SwallowBonus == dire.DeathRollBonus)
                throw new InvalidOperationException(
                    "The swallow adds the Strength modifier and the death " +
                    "roll adds one and a half times it.");
            // Swallow needs one size category smaller; death roll does not.
            if (dire.SwallowSizeDelta != -1 ||
                    dire.DeathRollTargetSizeDelta != 0)
                throw new InvalidOperationException(
                    "Swallow and death roll have different size thresholds.");
            if (ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(
                    dire.SwallowSizeDelta + 7, 7, false, 0,
                    dire.SwallowSizeDelta) == false)
                throw new InvalidOperationException(
                    "A target one category smaller is swallowable.");
            if (ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(7, 7,
                    false, 0, dire.SwallowSizeDelta))
                throw new InvalidOperationException(
                    "A target of the swallower's own size is not swallowable.");
        }

        /// <summary>
        /// Sprint is once per minute, which is ten rounds, and is not the
        /// Cheetah's once-per-summoning burst.
        /// </summary>
        internal static void SprintIsOncePerMinuteAndNotOncePerSummoning()
        {
            foreach (string key in new[] { "crocodile", "dire-crocodile" })
            {
                CrocodilianRulesProfile rules =
                    CrocodilianRulesPolicy.For(key);
                if (rules.SprintBonusFeet != 20 || rules.SprintRounds != 1 ||
                        rules.SprintCooldownRounds != 10)
                    throw new InvalidOperationException(
                        key + " sprints +20 feet for one round every ten " +
                        "rounds.");
            }
            // Explicitly different from the Cheetah's limit, which is a
            // different creature's rule and the obvious thing to copy.
            if (CrocodilianRulesPolicy.SprintCooldownRounds ==
                    ExpandedSummoningSpecialProfiles.CheetahSprintUses)
                throw new InvalidOperationException(
                    "The crocodilian sprint must not inherit the Cheetah's " +
                    "once-per-summoning limit.");

            // The timeline: available, spent, blocked for nine rounds,
            // available again on the tenth.
            if (!CrocodilianRulesPolicy.MaySprint(true, true, 0))
                throw new InvalidOperationException(
                    "A living, acting crocodilian off cooldown may sprint.");
            int cooldown = CrocodilianRulesPolicy.SprintCooldownRounds;
            for (int round = 1; round <= 9; round++)
            {
                cooldown = CrocodilianRulesPolicy.AdvanceSprintCooldown(
                    cooldown, 1);
                if (CrocodilianRulesPolicy.MaySprint(true, true, cooldown))
                    throw new InvalidOperationException(
                        "Sprint must stay blocked on cooldown round " + round +
                        ".");
            }
            cooldown = CrocodilianRulesPolicy.AdvanceSprintCooldown(cooldown, 1);
            if (cooldown != 0 ||
                    !CrocodilianRulesPolicy.MaySprint(true, true, cooldown))
                throw new InvalidOperationException(
                    "Sprint must be available again on the tenth round.");

            // A creature that cannot act cannot spend it, however long it has
            // been waiting.
            if (CrocodilianRulesPolicy.MaySprint(false, true, 0) ||
                    CrocodilianRulesPolicy.MaySprint(true, false, 0))
                throw new InvalidOperationException(
                    "A dead or unable creature cannot sprint.");
            // The cooldown never goes negative, so a long wait cannot bank a
            // second use.
            if (CrocodilianRulesPolicy.AdvanceSprintCooldown(2, 40) != 0)
                throw new InvalidOperationException(
                    "Waiting longer than the cooldown does not bank a use.");
        }
    }
}
