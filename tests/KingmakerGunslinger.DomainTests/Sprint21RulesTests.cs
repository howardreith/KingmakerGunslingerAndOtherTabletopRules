using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 21: the Giant Crab, and the Bebelith overhaul.
    ///
    /// <para>These are the offline checks. Every one of them is something a
    /// guarded run would otherwise have had to discover, and three of them
    /// exist because Sprint 20 discovered their equivalents the expensive
    /// way: a printed rider with no carrier, a printed bonus that existed
    /// only in its own derivation, and a review scenario the mod's own
    /// allowlist refused after a deploy and a launch.</para>
    ///
    /// <para>The Bebelith half is the unusual one. This is the first sprint in
    /// the series to reopen a released creature, so its tests divide along a
    /// line the rest of the series has never needed: its identity must not
    /// move, and its implementation must.</para>
    /// </summary>
    internal static class Sprint21RulesTests
    {
        internal const int NaturesAllyRoots = 7;
        /// <summary>
        /// One unit, seven placements and five facts for the crab; nine facts
        /// for the Bebelith, whose own unit and three roots are released and
        /// keep the identities they already have.
        /// </summary>
        internal const int AppendedLedgerIdentities = 1 + 7 + 5 + 9;

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
            Assertions.Equal(16, GiantCrabRulesPolicy.PrintedArmorClass,
                "10 plus 5 natural plus 1 Dexterity, with no size term. The "
                + "first draft of this sprint's contract copied 4 natural and "
                + "Dexterity 12, which closed on 15 and was wrong twice.");
            Assertions.Equal(15, GiantCrabRulesPolicy
                    .PrintedFlatFootedArmorClass,
                "Flat-footed is the total less the Dexterity term.");
            Assertions.Equal(1, GiantCrabRulesPolicy.DexterityModifier,
                "Dexterity 13 gives +1.");
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
            // Constrict, which the first draft of this contract omitted
            // outright. It is the claw's own die plus the whole Strength
            // modifier, which is why it is derived from the claw rather than
            // written down as 1d4+2 in a second place.
            Assertions.Equal(1, GiantCrabRulesPolicy.ConstrictDiceCount,
                "One die, as printed.");
            Assertions.Equal(GiantCrabRulesPolicy.ClawDieSides,
                GiantCrabRulesPolicy.ConstrictDieSides,
                "Constrict uses the claw's own die.");
            // The aquatic half is recorded as printed numbers and nothing
            // else. A swim speed folded into the ground speed would be a
            // substitution, which this sprint is forbidden to make.
            Assertions.Equal(20, GiantCrabRulesPolicy.PrintedSwimSpeedFeet,
                "The printed swim speed, recorded and unrepresented.");
            Assertions.Equal(30, GiantCrabRulesPolicy.SpeedFeet,
                "The printed land speed is used as printed; the swim speed is "
                + "not folded into it.");
        }

        /// <summary>
        /// The Bebelith's printed profile derives, including the parts the
        /// released build got wrong.
        /// </summary>
        internal static void TheBebelithPrintedProfileDerives()
        {
            BebelithRulesPolicy.Validate();
            Assertions.Equal(9, BebelithRulesPolicy.StrengthModifier,
                "Strength 28 gives +9.");
            Assertions.Equal(23, BebelithRulesPolicy.DifficultyClass(
                    BebelithRulesPolicy.HitDice,
                    BebelithRulesPolicy.ConstitutionModifier),
                "Ten plus half of twelve hit dice plus Constitution 7. Both "
                + "rot and the dismantle save use this one formula, and the "
                + "released build used an invented 25 for the latter.");
            Assertions.Equal(BebelithRulesPolicy.PrintedRotDifficultyClass,
                BebelithRulesPolicy.PrintedWebDifficultyClass,
                "Rot and the web print the same number because they come "
                + "from the same chassis, not because one was copied.");
            Assertions.Equal(12,
                BebelithRulesPolicy.PrintedCombatManeuverDefenseVersusTrip -
                BebelithRulesPolicy.PrintedCombatManeuverDefense,
                "Four per pair of legs beyond the first, three pairs - the "
                + "same arithmetic as the crab's, on a creature four sizes "
                + "larger.");
            Assertions.Equal(8, BebelithRulesPolicy.RacialStealthBonus,
                "A Huge creature with a printed racial +8 Stealth.");
            Assertions.Equal(19, BebelithRulesPolicy.ClawCriticalThreshold,
                "The claws crit on 19-20, which the released build did not "
                + "do.");
            Assertions.Equal(2, BebelithRulesPolicy.RotConstitutionDamage,
                "Two Constitution a failed save, flat.");
            Assertions.Equal(5, BebelithRulesPolicy.RotExposures,
                "The bite round and four after it.");
            Assertions.Equal(2, BebelithRulesPolicy.RotCureSaves,
                "Two consecutive successes, where every other poison this "
                + "project ships cures on one - which is the whole reason rot "
                + "cannot share a carrier.");
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
        /// The append is exactly twenty-two entries at the end of the ledger,
        /// all of them this sprint's two creatures', and nothing before them
        /// moved.
        /// </summary>
        internal static void TheLedgerAppendIsExactAndAppendOnly()
        {
            string[] appended = AppendedSymbols();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 21 appends exactly twenty-two identities.");
            foreach (string symbol in appended)
                Assertions.True(symbol.Contains("GiantCrab") ||
                        symbol.Contains("Bebelith"),
                    "Sprint 21 allocates for its two creatures only: "
                    + symbol);
            foreach (string tail in new[] {
                "KMG.Summoning.Natural.GiantCrab.UnitType",
                "KMG.Summoning.Natural.GiantCrab.MindlessImmunity",
                "KMG.Summoning.Natural.GiantCrab.TripDefense",
                "KMG.Summoning.Natural.GiantCrab.RacialSkills",
                "KMG.Summoning.Special.GiantCrab.Traits",
                // The Bebelith's nine. Rot needs two - a carrier and the
                // victim-owned state that outlives the summon - and the web
                // needs four, because an ability nothing casts is an ability
                // the creature does not have.
                "KMG.Summoning.Special.Bebelith.Rot",
                "KMG.Summoning.Special.Bebelith.RotState",
                "KMG.Summoning.Special.Bebelith.PenetratingStrike",
                "KMG.Summoning.Special.Bebelith.TripDefense",
                "KMG.Summoning.Special.Bebelith.RacialSkills",
                "KMG.Summoning.Special.Bebelith.Web",
                "KMG.Summoning.Special.Bebelith.WebResource",
                "KMG.Summoning.Special.Bebelith.WebAi",
                "KMG.Summoning.Special.Bebelith.Brain" })
                Assertions.True(appended.Contains(tail, StringComparer.Ordinal),
                    "Missing Sprint 21 identity: " + tail);
            // The Bebelith's released identities must NOT be here. Appending
            // one would mean the creature had been reallocated rather than
            // overhauled, which is a save and user-interface migration and is
            // exactly what this sprint is forbidden to do.
            foreach (string released in new[] {
                "KMG.Summoning.Unit.Bebelith",
                "KMG.Summoning.Special.Bebelith.Claw",
                "KMG.Summoning.Special.Bebelith.CombatTraits",
                "KMG.Summoning.Special.Bebelith.DismantledArmor" })
                Assertions.False(
                    appended.Contains(released, StringComparer.Ordinal),
                    "Sprint 21 reallocated a released Bebelith identity: "
                    + released);
            // No execution children, because Nature's Ally never templates
            // and this creature is not on the other table.
            Assertions.Equal(0, appended.Count(symbol =>
                symbol.EndsWith(".Celestial", StringComparison.Ordinal) ||
                symbol.EndsWith(".Fiendish", StringComparison.Ordinal)),
                "An untemplated creature owns no execution children.");
            // No weapon for the crab. A Medium creature takes the shared
            // native 1d4 claw unscaled, which is why the Large Sprint 18 and
            // 20 creatures had to own theirs and this one does not - and the
            // Bebelith's own claw is released, so neither creature appends
            // one here.
            Assertions.Equal(0, appended.Count(symbol =>
                symbol.Contains("Claw") || symbol.Contains("Bite")),
                "Neither creature appends a weapon identity.");
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
        /// The Bebelith's released identity does not move.
        ///
        /// <para>This sprint overhauls its mechanics on purpose, so the thing
        /// that must hold still is narrower than "nothing changes" and more
        /// important: the three roots a player may already have cast, at the
        /// tiers and quantities they already have, published as they already
        /// are. A save made against v0.0.149 has to resolve every one of
        /// them. Changing the code behind a stable identity is a creature
        /// correction; changing the identity would be a save and
        /// user-interface migration, which this sprint is forbidden to
        /// make.</para>
        /// </summary>
        internal static void TheReleasedBebelithIdentityIsPreserved()
        {
            SummonVariantSpec[] bebelith = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key ==
                    BebelithRulesPolicy.BebelithKey).ToArray();
            Assertions.Equal(3, bebelith.Length,
                "The Bebelith keeps its three Summon Monster roots. They may "
                + "not be suppressed, reallocated or replaced.");
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
            // The player-facing ordering, which is the quantity propagation
            // the charter owns: single at its own tier, 1d3 one up, 1d4+1
            // after. A changed multiplicity would be visible in the spell
            // list of a saved game.
            SummonVariantSpec[] ordered = bebelith
                .OrderBy(value => value.ParentTier).ToArray();
            Assertions.Equal(SummonMultiplicity.One, ordered[0].Multiplicity,
                "A single Bebelith at Summon Monster VII.");
            Assertions.Equal(SummonMultiplicity.OneD3, ordered[1].Multiplicity,
                "1d3 at VIII.");
            Assertions.Equal(SummonMultiplicity.OneD4PlusOne,
                ordered[2].Multiplicity, "1d4+1 at IX.");
            Assertions.Equal(9, ordered[2].ParentTier,
                "And nothing beyond IX, which is the top of the table.");
            // The released spelling. A source may print "Bebilith"; renaming
            // the shipped key would retire a GUID and migrate every save that
            // holds one, so this sprint keeps what shipped.
            Assertions.Equal("bebelith", BebelithRulesPolicy.BebelithKey,
                "The released key is kept even where a source disagrees: a "
                + "rename is a migration, not a correction.");
        }

        /// <summary>
        /// Rot is a bite-only injury effect with the printed graph, and it is
        /// built on the lifecycle the engine already runs.
        ///
        /// <para>Three of the four rewired values are what make it rot rather
        /// than a poison: two Constitution flat on each failed save, five
        /// exposures, and two consecutive successes to cure where every other
        /// poison this project ships cures on one. The fourth is the save
        /// type. The difficulty class is deliberately absent from the
        /// blueprint - it is set live from the creature's own Constitution
        /// immediately before the game's own save, so an already-applied rot
        /// keeps the number the bite that caused it produced even after the
        /// Bebelith is dismissed or killed.</para>
        /// </summary>
        internal static void RotIsBiteOnlyWithThePrintedGraph()
        {
            string builder = SpecialBuilder();
            Assertions.True(builder.Contains(
                    "damage.Bonus = BebelithRulesPolicy.RotConstitutionDamage"),
                "Rot's two Constitution must come from the policy.");
            Assertions.True(builder.Contains(
                    "damage.Ticks = BebelithRulesPolicy.RotExposures"),
                "Rot's five exposures must come from the policy.");
            Assertions.True(builder.Contains(
                    "damage.SuccesfullSaves = BebelithRulesPolicy.RotCureSaves"),
                "Rot's two-success cure must come from the policy. This is "
                + "the value no shared poison carrier has, and the whole "
                + "reason rot needs its own.");
            Assertions.True(builder.Contains(
                    "damage.SaveType = SavingThrowType.Fortitude"),
                "Rot is a Fortitude save.");
            Assertions.True(builder.Contains(
                    "damage.Stat = StatType.Constitution"),
                "Rot damages Constitution.");
            Assertions.True(builder.Contains(
                    "new DiceFormula(0, DiceType.Zero)"),
                "The printed amount is flat, so the die is emptied and the "
                + "bonus carries it.");
            // Bite only. The gate is the bite's own weapon type inside the
            // wound gate, which is what keeps rot off the claws and off the
            // web - two of the frozen contract's prohibitions.
            Assertions.True(builder.Contains("trigger.WeaponType = bite.Type"),
                "Rot's trigger must be gated on the bite's weapon type.");
            Assertions.True(builder.Contains(
                    "ContextActionOnlyIfWeaponWounded"),
                "Rot requires a wound, not merely a hit.");
            Assertions.True(builder.Contains(
                    "ContextActionSetBebelithRotDc"),
                "The difficulty class is set live from Constitution rather "
                + "than stored, so the number cannot go stale and an applied "
                + "rot survives its source.");
            Assertions.False(builder.Contains(
                    "trigger.WeaponType = claw.Type"),
                "No rot from a claw.");
        }

        /// <summary>
        /// Penetrating strike is a set of damage descriptors, not a bonus,
        /// and the demon half does not leak.
        ///
        /// <para>The released build invented a flat +2 against chaotic evil
        /// outsiders, which is neither what the ability says nor something
        /// the printed block can produce. What it says is that the natural
        /// weapons count as chaotic and magic generally, and additionally as
        /// cold iron and good against demons - so a non-demon must never see
        /// cold iron or good. That asymmetry is invisible against a demon,
        /// which is exactly why it has a named negative control in the live
        /// review.</para>
        /// </summary>
        internal static void PenetratingStrikeIsDescriptorsAndNotABonus()
        {
            Assertions.Equal(1, ExpandedSummoningSpecialProfiles
                .BebelithPenetratingEnhancement,
                "Magic as the smallest enhancement that makes the descriptor "
                + "true without adding to the damage total.");
            string components = File.ReadAllText(Path.Combine(
                RepositoryRoot(), "src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs"));
            int start = components.IndexOf(
                "public sealed class BebelithCombatComponent",
                StringComparison.Ordinal);
            Assertions.True(start > 0, "The Bebelith's component must exist.");
            string body = components.Substring(start);
            Assertions.True(body.Contains(
                    "damage.AddAlignment(DamageAlignment.Chaotic)"),
                "Chaotic on every natural attack.");
            Assertions.True(body.Contains("if (!demon) continue;"),
                "The demon half must be guarded, and the guard must come "
                + "before the cold iron and good it gates.");
            int guard = body.IndexOf("if (!demon) continue;",
                StringComparison.Ordinal);
            Assertions.True(body.IndexOf(
                    "damage.AddAlignment(DamageAlignment.Good)",
                    StringComparison.Ordinal) > guard,
                "Good must be inside the demon guard.");
            Assertions.True(body.IndexOf(
                    "AddMaterial(PhysicalDamageMaterial.ColdIron)",
                    StringComparison.Ordinal) > guard,
                "Cold iron must be inside the demon guard. Granting it "
                + "universally would be invisible against a demon and wrong "
                + "against everything else.");
            Assertions.True(body.Contains("IsNaturalWeapon(evt.AttackRoll.Weapon"),
                "Only this creature's own natural weapons; spell and area "
                + "damage carry no attack roll and must never be touched.");
            // The invented bonus is gone, not merely unused.
            Assertions.False(components.Contains("BebelithDemonHunterBonus"),
                "The invented +2 must be gone from the code entirely.");
        }

        /// <summary>
        /// Dismantle Armor, the sprint's highest-risk mechanic, reaches no
        /// inventory at all.
        ///
        /// <para>The printed sequence is a combat manoeuvre check and then a
        /// Reflex save; the released build had neither and went straight to an
        /// invented difficulty class of 25. The check is now the engine's own
        /// sunder-armour manoeuvre, which is also what keeps the whole feature
        /// away from equipment: a manoeuvre is a roll, not an item
        /// mutation.</para>
        ///
        /// <para>What cannot be done is recorded rather than approximated.
        /// Kingmaker declares no broken condition, no durability value and no
        /// item-damage component a mod can reach, so the piece survives and
        /// stops protecting - its whole armour-class contribution gone,
        /// computed live rather than stored. Every equipment question this
        /// mechanic asks is a read, which is the mandatory safety list
        /// discharged by the shape of the code rather than by a check that
        /// could be forgotten.</para>
        /// </summary>
        internal static void DismantleArmorTouchesNoInventory()
        {
            string components = File.ReadAllText(Path.Combine(
                RepositoryRoot(), "src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs"));
            Assertions.True(components.Contains("CombatManeuver.SunderArmor"),
                "The check must be the engine's own sunder-armour manoeuvre, "
                + "which resolves with the game's own combat manoeuvre bonus "
                + "and defence rather than a rule of this project's.");
            Assertions.True(components.Contains(
                    "BebelithRulesPolicy.DifficultyClass("),
                "The Reflex save must use the shared printed derivation, not "
                + "an invented number.");
            // Read narrowly. The released build's invented 25 lived inside
            // this handler, and other creatures in this file legitimately
            // print numbers containing 25.
            int dismantle = components.IndexOf(
                "public void OnEventDidTrigger(RuleAttackWithWeapon evt)",
                StringComparison.Ordinal);
            Assertions.True(dismantle > 0,
                "The dismantle handler must exist.");
            string sequence = components.Substring(dismantle,
                Math.Min(3000, components.Length - dismantle));
            Assertions.False(sequence.Contains("SavingThrowType.Reflex, 25"),
                "The invented difficulty class 25 must be gone.");
            Assertions.True(sequence.IndexOf("CombatManeuver.SunderArmor",
                    StringComparison.Ordinal) <
                sequence.IndexOf("SavingThrowType.Reflex",
                    StringComparison.Ordinal),
                "The manoeuvre check comes first and gates the save, as "
                + "printed: a failed check ends the sequence and the target "
                + "never rolls.");
            // Every equipment access in the Bebelith's reach is a read. These
            // are the writes that would be inventory corruption, and none of
            // them may appear anywhere in this file.
            foreach (string forbidden in new[] {
                "RemoveItem", "InsertItem", "Unequip", "TryRemove",
                ".Collection.Remove", "HoldingSlot.RemoveItem",
                "Destroy(", "Inventory.Remove" })
                Assertions.False(components.Contains(forbidden),
                    "Dismantle Armor must never write to an inventory: "
                    + forbidden);
            Assertions.True(components.Contains(
                    "internal static class BebelithDismantle"),
                "Every equipment question the mechanic asks belongs in one "
                + "place, so that 'all reads' is checkable rather than "
                + "asserted.");
            Assertions.True(components.Contains(
                    "public sealed class BebelithDismantledArmorComponent"),
                "The dismantled state must be a wearer-side component.");
            Assertions.True(components.Contains(
                    "BebelithDismantle.LostArmorClass(Owner.Unit)"),
                "The penalty must be computed live from the equipped piece, "
                + "not stored - which is what makes a permanent state safe "
                + "across a save and a reload.");
            Assertions.True(ExpandedSummoningSpecialProfiles
                .BebelithDismantleCancelsWholeContribution,
                "The printed effect is destruction, so the whole contribution "
                + "goes rather than an invented two points.");
            Assertions.True(ExpandedSummoningSpecialProfiles
                .BebelithDismantleIsPermanent,
                "And it does not come back, as printed.");
            // The gap is recorded in the frozen contract, by identifier.
            string contract = File.ReadAllText(Path.Combine(RepositoryRoot(),
                "planning", "EXPANDED-SUMMONING-SPRINT21-CONTRACT.json"));
            Assertions.True(contract.Contains("ITEM_DESTRUCTION_UNMODELED"),
                "What Kingmaker cannot represent must be recorded as an "
                + "accepted limitation rather than approximated silently.");
        }

        /// <summary>
        /// The web rides the qualified Sprint 6 seam, at this creature's own
        /// values, and the Giant Spider's released values still arrive.
        ///
        /// <para>Generalising a released creature's configuration is the kind
        /// of refactor that can change it without anybody noticing, so the
        /// spider's five values are checked by name on the new code path
        /// rather than merely absent from the old one.</para>
        /// </summary>
        internal static void TheWebReusesTheQualifiedSeam()
        {
            string builder = SpecialBuilder();
            Assertions.True(builder.Contains("private sealed class WebSpec"),
                "One parameterized seam, not two copies of a web.");
            foreach (string spider in new[] {
                "UnitSymbol = GiantSpiderUnitSymbol",
                "AbilitySymbol = GiantSpiderWebSymbol",
                ".GiantSpiderWebRangeFeet", ".GiantSpiderWebRounds",
                ".GiantSpiderWebMaxSizeDelta", ".GiantSpiderWebSpellLevel",
                ".GiantSpiderWebUses" })
                Assertions.True(builder.Contains(spider),
                    "The Giant Spider's released web value must still arrive "
                    + "through the generalised seam: " + spider);
            foreach (string own in new[] {
                "UnitSymbol = BebelithUnitSymbol",
                "AbilitySymbol = BebelithWebSymbol",
                ".BebelithWebRangeFeet", ".BebelithWebRounds",
                ".BebelithWebUses",
                "CasterHitDice = BebelithRulesPolicy.HitDice" })
                Assertions.True(builder.Contains(own),
                    "The Bebelith's web must go through the same seam at its "
                    + "own values: " + own);
            // A brain that casts. The released creature took the native brain
            // that casts nothing, so an ability alone would have been an
            // ability the creature never used.
            Assertions.True(builder.Contains("unit.Brain = brain;"),
                "The web needs a brain that can cast it.");
            Assertions.Equal(23, BebelithRulesPolicy.DifficultyClass(
                    BebelithRulesPolicy.HitDice,
                    BebelithRulesPolicy.ConstitutionModifier),
                "The printed web difficulty class is the chassis's, derived "
                + "once and shared with rot.");
            Assertions.Equal(12, BebelithRulesPolicy.PrintedWebHitPoints,
                "The printed web has 12 hit points.");
            Assertions.Equal(11, BebelithRulesPolicy.PrintedWebRangedAttack,
                "And a printed ranged attack of +11.");
        }

        /// <summary>
        /// The Bebelith's own four carriers are wired, not merely named.
        ///
        /// <para>This is the Sprint 20 lesson, applied before the launch
        /// rather than after it. Each of these is a printed line whose
        /// arithmetic closes without a carrier, so a registered identity and a
        /// closed derivation prove nothing at all - which is how a live Giant
        /// Scorpion came to read CMD 27 against a printed 31 and Perception 0
        /// against a printed +4. This creature prints the same +12 against
        /// trip on a chassis four sizes larger, and the shared native fact is
        /// still worth 8.</para>
        /// </summary>
        internal static void TheBebelithCarriersAreWiredRatherThanNamed()
        {
            string builder = SpecialBuilder();
            foreach (string configure in new[] {
                "ConfigureBebelithRot", "ConfigureBebelithPenetratingStrike",
                "ConfigureBebelithTripDefense",
                "ConfigureBebelithRacialSkills" })
            {
                Assertions.True(builder.Contains(
                        "private static void " + configure),
                    "The carrier must exist: " + configure);
                Assertions.True(builder.Contains(configure + "(library,") ||
                        builder.Contains(configure + "(Require<"),
                    "And must be called: " + configure);
            }
            // On the creature. A configured blueprint nothing grants is the
            // exact defect class Sprint 20 paid two guarded runs to find.
            //
            // Read from this creature's own configuration, not the file's
            // first AddFacts block: two dozen creatures grant facts in this
            // one file, and a window anchored on the wrong one would pass for
            // somebody else's wiring.
            int mine = builder.IndexOf(
                "private static void ConfigureBebelith(LibraryScriptableObject",
                StringComparison.Ordinal);
            Assertions.True(mine > 0,
                "The Bebelith's own configuration must exist.");
            int added = builder.IndexOf(
                "unit.AddFacts = new BlueprintUnitFact[]", mine,
                StringComparison.Ordinal);
            Assertions.True(added > mine, "The Bebelith must grant its facts.");
            string facts = builder.Substring(added,
                Math.Min(1400, builder.Length - added));
            foreach (string fact in new[] { "rot", "penetratingStrike",
                "tripDefense", "racialSkills", "web" })
                Assertions.True(facts.Contains(fact),
                    "The Bebelith does not carry " + fact);
            // From the policy, so a drifting number breaks the derivation
            // before it reaches a player.
            Assertions.True(builder.Contains(
                    "defence.Bonus = BebelithRulesPolicy.EightLegTripBonus"),
                "The anti-trip carrier must take its value from the policy.");
            Assertions.True(builder.Contains(
                    "stealth.Value = BebelithRulesPolicy.RacialStealthBonus"),
                "The racial carrier must take its value from the policy.");
            Assertions.Equal(12, BebelithRulesPolicy.EightLegTripBonus,
                "The printed eight-leg trip defence.");
            Assertions.Equal(8, BebelithRulesPolicy.SharedNativeTripBonus,
                "What the shared native fact actually delivers, measured on "
                + "the Sprint 20 scorpion. Four short, which is why every "
                + "eight-legged creature in this series owns its own.");
            // The bite is the primary limb, because rot rides the bite and the
            // released build's limb order put a claw there.
            Assertions.True(builder.Contains(
                    "NaturalBody(bite, new[] { claw, claw }"),
                "The bite is the primary limb; the two claws are additional, "
                + "which is also what makes 'both claws' readable as "
                + "additional limbs 0 and 1.");
            Assertions.True(builder.Contains("IronWillGuid"),
                "The printed Will of +7 is a poor save plus Iron Will; a "
                + "three-good-save chassis cannot produce it, which is what "
                + "BebelithRulesPolicy refuses.");
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

        private static string SpecialBuilder()
        {
            return File.ReadAllText(Path.Combine(RepositoryRoot(), "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs"));
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
