using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class NalfeshneeRules39Tests
    {
        internal static void PrintedContractDerivesRatherThanPins()
        {
            NalfeshneeRules39Policy.Validate();

            // Every printed spell-like difficulty class comes out of one
            // formula, so none of them is a constant in the implementation.
            Assertions.Equal(NalfeshneeRules39Policy.PrintedCallLightningDc,
                NalfeshneeRules39Policy.SpellDifficultyClass(
                    NalfeshneeRules39Policy.CallLightningSpellLevel,
                    NalfeshneeRules39Policy.CharismaBonus),
                "Call lightning's printed DC derives from its spell level and Charisma.");
            Assertions.Equal(NalfeshneeRules39Policy.PrintedFeeblemindDc,
                NalfeshneeRules39Policy.SpellDifficultyClass(
                    NalfeshneeRules39Policy.FeeblemindSpellLevel,
                    NalfeshneeRules39Policy.CharismaBonus),
                "Feeblemind's printed DC derives from its spell level and Charisma.");
            Assertions.Equal(NalfeshneeRules39Policy.PrintedUnholyAuraDc,
                NalfeshneeRules39Policy.SpellDifficultyClass(
                    NalfeshneeRules39Policy.UnholyAuraSpellLevel,
                    NalfeshneeRules39Policy.CharismaBonus),
                "The constant aura's printed DC derives from its spell level and Charisma.");

            // The supernatural formula is a DIFFERENT formula, and using the
            // spell-like one for Unholy Nimbus would be wrong. At this
            // creature's inputs the two disagree, so the test is meaningful
            // rather than coincidentally satisfied.
            Assertions.Equal(NalfeshneeRules39Policy.PrintedNimbusDc,
                NalfeshneeRules39Policy.SupernaturalDifficultyClass(
                    NalfeshneeRules39Policy.NalfeshneeHitDice,
                    NalfeshneeRules39Policy.CharismaBonus),
                "Unholy Nimbus is supernatural: 10 + half the hit dice + Charisma.");
            Assertions.True(
                NalfeshneeRules39Policy.SupernaturalDifficultyClass(
                    NalfeshneeRules39Policy.NalfeshneeHitDice,
                    NalfeshneeRules39Policy.CharismaBonus) !=
                NalfeshneeRules39Policy.SpellDifficultyClass(
                    NalfeshneeRules39Policy.NalfeshneeHitDice,
                    NalfeshneeRules39Policy.CharismaBonus),
                "The supernatural and spell-like DC formulas are distinct at these inputs.");
        }

        /// <summary>
        /// The printed defences already contain this creature's own constant
        /// unholy aura, so the blueprint must carry the racial values and the
        /// buff must supply the rest. This is the single easiest way to ship a
        /// creature four points too tough in AC, every save and CMD.
        /// </summary>
        internal static void ConstantAuraIsNotCountedTwice()
        {
            // The racial armour class contains no deflection at all.
            Assertions.Equal(NalfeshneeRules39Policy
                    .RacialArmorClassWithoutAura,
                10 + NalfeshneeRules39Policy.NaturalArmor +
                    NalfeshneeRules39Policy.DexterityBonus +
                    NalfeshneeRules39Policy.SizeArmorClassModifier,
                "Racial AC is 10 + natural + Dex + size, with no deflection.");
            Assertions.Equal(NalfeshneeRules39Policy.PrintedArmorClass,
                NalfeshneeRules39Policy.RacialArmorClassWithoutAura +
                    NalfeshneeRules39Policy.AuraDeflectionBonus,
                "The aura's deflection is what takes racial AC to the printed 29.");

            // All three racial save totals exclude the aura, and all three
            // printed totals include it exactly once.
            Assertions.Equal(NalfeshneeRules39Policy
                    .RacialFortitudeWithoutAura,
                NalfeshneeRules39Policy.GoodSaveBase +
                    NalfeshneeRules39Policy.ConstitutionBonus,
                "Racial Fortitude is a good base plus Constitution.");
            Assertions.Equal(NalfeshneeRules39Policy.RacialReflexWithoutAura,
                NalfeshneeRules39Policy.PoorSaveBase +
                    NalfeshneeRules39Policy.DexterityBonus,
                "Racial Reflex is a POOR base plus Dexterity.");
            Assertions.Equal(NalfeshneeRules39Policy.RacialWillWithoutAura,
                NalfeshneeRules39Policy.GoodSaveBase +
                    NalfeshneeRules39Policy.WisdomBonus +
                    NalfeshneeRules39Policy.IronWillWillBonus,
                "Racial Will is a good base plus Wisdom plus the printed Iron Will feat.");
            Assertions.Equal(NalfeshneeRules39Policy.PrintedFortitude,
                NalfeshneeRules39Policy.RacialFortitudeWithoutAura +
                    NalfeshneeRules39Policy.AuraResistanceBonus,
                "Printed Fortitude is racial plus the aura's resistance.");
            Assertions.Equal(NalfeshneeRules39Policy.PrintedReflex,
                NalfeshneeRules39Policy.RacialReflexWithoutAura +
                    NalfeshneeRules39Policy.AuraResistanceBonus,
                "Printed Reflex is racial plus the aura's resistance.");
            Assertions.Equal(NalfeshneeRules39Policy.PrintedWill,
                NalfeshneeRules39Policy.RacialWillWithoutAura +
                    NalfeshneeRules39Policy.AuraResistanceBonus,
                "Printed Will is racial plus the aura's resistance.");

            // Without Iron Will the Will derivation falls exactly two short,
            // which is the gap that invites a wrong "published anomaly" note.
            Assertions.True(NalfeshneeRules39Policy.GoodSaveBase +
                    NalfeshneeRules39Policy.WisdomBonus +
                    NalfeshneeRules39Policy.AuraResistanceBonus ==
                NalfeshneeRules39Policy.PrintedWill -
                    NalfeshneeRules39Policy.IronWillWillBonus,
                "Iron Will is what closes the printed Will total; without it the derivation is two short.");

            // The printed CMD also contains the deflection exactly once.
            Assertions.Equal(
                NalfeshneeRules39Policy.PrintedCombatManeuverDefense,
                10 + NalfeshneeRules39Policy.BaseAttackBonus +
                    NalfeshneeRules39Policy.StrengthBonus +
                    NalfeshneeRules39Policy.DexterityBonus +
                    NalfeshneeRules39Policy.SizeCombatManeuverModifier +
                    NalfeshneeRules39Policy.AuraDeflectionBonus,
                "Printed CMD contains the aura's deflection once.");
        }

        /// <summary>
        /// The creature's own general spell resistance and the aura's
        /// conditional resistance are different facts that happen to print the
        /// same number for this creature. Collapsing them would be easy and
        /// wrong, so they are asserted to be separately named quantities with
        /// separate meanings rather than one shared constant.
        /// </summary>
        internal static void GeneralAndConditionalSpellResistanceStaySeparate()
        {
            Assertions.Equal(NalfeshneeRules39Policy.ChallengeRating + 11,
                NalfeshneeRules39Policy.SpellResistance,
                "The creature's own general SR is CR + 11 and applies to everything.");
            Assertions.Equal(25,
                NalfeshneeRules39Policy.AuraConditionalSpellResistance,
                "The aura's SR applies only against good spells and good casters.");
            // They coincide here. That coincidence is exactly why the two are
            // separate constants: a future creature whose CR differs would
            // break any implementation that had shared one value.
            Assertions.True(NalfeshneeRules39Policy.SpellResistance ==
                NalfeshneeRules39Policy.AuraConditionalSpellResistance,
                "For this creature the two happen to be equal, which is the trap these separate names exist to avoid.");
        }

        /// <summary>
        /// Fourteen racial hit dice and spell-like caster level 12 are not the
        /// same number, and each drives different quantities. The Glabrezu's
        /// pair ran the other way round, so a reader who carried that
        /// creature's habit over would get both wrong.
        /// </summary>
        internal static void HitDiceAndCasterLevelAreDifferentQuantities()
        {
            Assertions.Equal(14, NalfeshneeRules39Policy.NalfeshneeHitDice,
                "Fourteen racial hit dice drive BAB, save bases, skill ranks and the Su DC's half-HD term.");
            Assertions.Equal(12, NalfeshneeRules39Policy.SpellLikeCasterLevel,
                "Caster level 12 drives the spell-like DCs, penetration and the ranges.");
            Assertions.Equal(NalfeshneeRules39Policy.NalfeshneeHitDice,
                NalfeshneeRules39Policy.PreCastEstimatedCasterLevel,
                "The pre-cast estimate reports the hit dice, because RuleCalculateAbilityParams reads only the derived ContextSetAbilityParams.");

            // Hit dice drive the attack and manoeuvre totals.
            Assertions.Equal(NalfeshneeRules39Policy.PrintedAttackBonus,
                NalfeshneeRules39Policy.BaseAttackBonus +
                    NalfeshneeRules39Policy.StrengthBonus +
                    NalfeshneeRules39Policy.SizeArmorClassModifier,
                "Printed attack is BAB + Strength + size.");
            Assertions.Equal(
                NalfeshneeRules39Policy.PrintedCombatManeuverBonus,
                NalfeshneeRules39Policy.BaseAttackBonus +
                    NalfeshneeRules39Policy.StrengthBonus +
                    NalfeshneeRules39Policy.SizeCombatManeuverModifier,
                "Printed CMB is BAB + Strength + the Huge manoeuvre modifier.");

            // The printed 203 is the average roll on fourteen d10, which is
            // 5.5 per die and not 5. Using sides/2 gives 210 and looks close
            // enough to pass an eye check, so it is asserted explicitly.
            Assertions.Equal(NalfeshneeRules39Policy.HitPoints,
                NalfeshneeRules39Policy.NalfeshneeHitDice *
                    (NalfeshneeRules39Policy.HitDieSides + 1) / 2 +
                    NalfeshneeRules39Policy.HitPointConstitutionTotal,
                "Printed hit points are the d10 average of 5.5 per die plus the Constitution total.");
            Assertions.True(NalfeshneeRules39Policy.NalfeshneeHitDice *
                    NalfeshneeRules39Policy.HitDieSides / 2 +
                    NalfeshneeRules39Policy.HitPointConstitutionTotal !=
                NalfeshneeRules39Policy.HitPoints,
                "The naive sides/2 average does NOT reproduce the printed total.");
        }

        /// <summary>
        /// Caster-level-dependent ranges are derived rather than inherited
        /// from a fixed category. Sprint 38 established that the installed
        /// Close category is a flat 30 feet that reads no caster level at all,
        /// so assuming a category implements a printed scaling range is a
        /// known way to ship the wrong distance.
        /// </summary>
        internal static void SpellLikeRangesScaleWithCasterLevel()
        {
            Assertions.Equal(220, NalfeshneeRules39Policy.MediumRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel),
                "Medium range is 100 + 10 per caster level, so 220 at caster level 12.");
            Assertions.Equal(880, NalfeshneeRules39Policy.LongRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel),
                "Long range is 400 + 40 per caster level, so 880 at caster level 12.");
            // Both must actually move with the caster level, which a fixed
            // category would not.
            Assertions.True(NalfeshneeRules39Policy.MediumRangeFeet(20) >
                NalfeshneeRules39Policy.MediumRangeFeet(12),
                "Medium range scales with the caster level.");
            Assertions.True(NalfeshneeRules39Policy.LongRangeFeet(20) >
                NalfeshneeRules39Policy.LongRangeFeet(12),
                "Long range scales with the caster level.");
        }

        /// <summary>
        /// The printed targeted greater dispel removes one effect for every
        /// four caster levels. The installed
        /// <c>ContextActionDispelMagic</c> offers only "stop after the first
        /// removal" or "remove every eligible effect" and carries no
        /// success-count field, so neither native setting is this number. The
        /// cap is therefore derived from the caster level rather than written
        /// down, and it is explicitly NOT one.
        /// </summary>
        internal static void GreaterDispelCapIsCasterLevelDerivedAndNotOne()
        {
            Assertions.Equal(3,
                NalfeshneeRules39Policy.GreaterDispelMaxRemovals(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel),
                "At caster level 12 the printed cap is three effects.");
            Assertions.True(
                NalfeshneeRules39Policy.GreaterDispelMaxRemovals(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel) > 1,
                "Greater dispel is not an ordinary one-effect dispel under a higher name.");
            Assertions.Equal(1,
                NalfeshneeRules39Policy.GreaterDispelMaxRemovals(4),
                "The same rule gives one effect at caster level 4.");
            Assertions.Equal(5,
                NalfeshneeRules39Policy.GreaterDispelMaxRemovals(20),
                "And five at caster level 20, so the cap genuinely scales.");
        }

        /// <summary>
        /// One carried check over ordered effects: what it removes, what it
        /// skips, and what it never does.
        ///
        /// <para>These drive <c>GreaterDispelResolve</c>, which is the same
        /// traversal and the same accounting the production action runs - the
        /// action differs only in supplying a delegate that really dispatches
        /// the native rule instead of a scripted outcome. So these are the
        /// behaviour, not a restated formula.</para>
        /// </summary>
        internal static void GreaterDispelCarriesOneCheckOverOrderedEffects()
        {
            int cl = NalfeshneeRules39Policy.SpellLikeCasterLevel;
            int cap = NalfeshneeRules39Policy.GreaterDispelMaxRemovals(cl);
            Func<int, bool> always = index => true;

            // A high-level effect the carried check FAILS is skipped, not
            // treated as the end of the walk. Roll 5 at caster level 12 beats
            // 11 + level, so levels 6 and below fall and level 20 stands.
            var visited = new System.Collections.Generic.List<int>();
            int[] mixed = NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 5, new[] { 20, 6, 4, 2 }, cap,
                index => { visited.Add(index); return true; });
            Assertions.True(mixed.SequenceEqual(new[] { 1, 2, 3 }),
                "A failed higher effect is skipped and the lower ones are still removed.");
            Assertions.True(visited.SequenceEqual(new[] { 1, 2, 3 }),
                "The failed effect is never dispatched, so it is never retried either.");

            // A fourth eligible effect beyond the cap survives.
            int[] capped = NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 20, new[] { 5, 4, 3, 2, 1 }, cap, always);
            Assertions.True(capped.SequenceEqual(new[] { 0, 1, 2 }),
                "The cap stops at three even when every effect would fall.");

            // THE CASE THAT PRE-TRUNCATION GOT WRONG. Five effects all beat
            // the check, but the first dispatch removes nothing - a
            // concurrent removal, an undispelable guard, a refusing
            // subscriber. The cap counts CONFIRMATIONS, so that attempt costs
            // nothing and the walk continues until three really left. An
            // implementation that selected the first three candidates up
            // front would have stopped at index 2 and returned two removals.
            var attempted = new System.Collections.Generic.List<int>();
            int[] afterMiss = NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 20, new[] { 5, 4, 3, 2, 1 }, cap,
                index => { attempted.Add(index); return index != 0; });
            Assertions.True(afterMiss.SequenceEqual(new[] { 1, 2, 3 }),
                "A dispatch that removes nothing does not consume the cap, and a later qualifying candidate is still reached.");
            Assertions.True(attempted.SequenceEqual(new[] { 0, 1, 2, 3 }),
                "The unconfirmed candidate is attempted once and never revisited.");
            Assertions.Equal(cap, afterMiss.Length,
                "Three confirmed removals still happen despite the miss.");

            // Every dispatch missing yields no removals and still visits each
            // qualifying candidate exactly once.
            var all = new System.Collections.Generic.List<int>();
            int[] none = NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 20, new[] { 5, 4, 3 }, cap,
                index => { all.Add(index); return false; });
            Assertions.Equal(0, none.Length,
                "No confirmations, no removals.");
            Assertions.True(all.SequenceEqual(new[] { 0, 1, 2 }),
                "Each qualifying candidate is attempted exactly once even when none confirm.");

            // One check decides them all, so retrying a failed effect could
            // only ever repeat its own answer - which is why no retry exists.
            Assertions.True(
                !NalfeshneeRules39Policy.DispelCheckBeats(cl, 5, 20) &&
                !NalfeshneeRules39Policy.DispelCheckBeats(cl, 5, 20),
                "The carried check is deterministic for a given effect.");

            // Degenerate inputs are inert rather than throwing.
            Assertions.Equal(0, NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 1, new[] { 30, 29 }, cap, always).Length,
                "A check that beats no effect removes none.");
            Assertions.Equal(0, NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 20, new int[0], cap, always).Length,
                "No effects, no removals.");
            Assertions.Equal(0, NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 20, new[] { 1 }, 0, always).Length,
                "A zero cap removes nothing.");
            Assertions.Equal(0, NalfeshneeRules39Policy.GreaterDispelResolve(
                cl, 20, new[] { 1 }, cap, null).Length,
                "No dispatcher, no removals.");
        }

        /// <summary>
        /// Unholy Nimbus's printed identity: three daily uses, a burst one
        /// round after a free-action activation, a 60-foot radius, and a daze
        /// of 1d10 rounds. It is neither the Vrock's instant Stunning Screech
        /// nor a nausea aura, and it is not the constant unholy aura either.
        /// </summary>
        internal static void NimbusKeepsItsOwnDelayedIdentity()
        {
            Assertions.Equal(3, NalfeshneeRules39Policy.NimbusUsesPerDay,
                "Three uses per day, each independent for this summoning.");
            Assertions.Equal(60, NalfeshneeRules39Policy.NimbusRadiusFeet,
                "A 60-foot burst radius.");
            Assertions.Equal(1, NalfeshneeRules39Policy.NimbusDelayRounds,
                "The light bursts one round after activation, which is the ability's identity and not a cooldown.");
            Assertions.Equal(1,
                NalfeshneeRules39Policy.NimbusDazeDiceCount,
                "The condition lasts 1d10 rounds.");
            Assertions.Equal(10,
                NalfeshneeRules39Policy.NimbusDazeDieSides,
                "The condition lasts 1d10 rounds.");

            // The nimbus is a different ability from the constant aura, and
            // their difficulty classes come from different formulas. Asserting
            // the DCs differ keeps the two from being conflated.
            Assertions.True(NalfeshneeRules39Policy.PrintedNimbusDc !=
                NalfeshneeRules39Policy.PrintedUnholyAuraDc,
                "Unholy Nimbus and the constant unholy aura are different abilities with different DCs.");
        }

        /// <summary>
        /// This creature's natural attacks and feats are its own. Cloning a
        /// sibling demon would suggest a rend, Great Cleave, Vital Strike and
        /// extra limbs, and this creature's printed entry has none of them.
        /// </summary>
        internal static void NaturalAttacksAndFeatsAreThisCreaturesOwn()
        {
            // One bite and two claws, all primary, so all take the full
            // Strength bonus rather than half.
            Assertions.Equal(2, NalfeshneeRules39Policy.ClawCount,
                "Two claws, not four, and no pincers.");
            Assertions.Equal(NalfeshneeRules39Policy.StrengthBonus,
                NalfeshneeRules39Policy.PrintedBiteDamageBonus,
                "The bite is primary and takes the full Strength bonus.");
            Assertions.Equal(NalfeshneeRules39Policy.StrengthBonus,
                NalfeshneeRules39Policy.PrintedClawDamageBonus,
                "Both claws are primary and take the full Strength bonus.");

            // The printed 19-20 bite threat range is an edge of 19, because
            // the engine renders the width as 21 - edge. It must be delivered
            // exactly once, never by both weapon metadata and a feat.
            Assertions.Equal(19,
                NalfeshneeRules39Policy.BiteCriticalRollEdge,
                "An edge of 19 is exactly a 19-20 threat range, since the sheet renders width as 21 - edge.");
            Assertions.Equal(2,
                21 - NalfeshneeRules39Policy.BiteCriticalRollEdge,
                "Which is a width of two.");

            // Huge: a 15-foot space and a 15-foot reach, both printed.
            Assertions.Equal(15, NalfeshneeRules39Policy.SpaceFeet,
                "Huge: a 15-foot space.");
            Assertions.Equal(15, NalfeshneeRules39Policy.ReachFeet,
                "Huge: a 15-foot reach.");

            // One movement contract. Kingmaker exposes a single movement
            // speed, so the effective speed IS the printed flying speed,
            // carried with the released Airborne navigation fact, and the
            // printed land speed is recorded as omitted rather than summed,
            // averaged or silently substituted. Asserting the inequality as
            // well as both values is what catches a future edit that quietly
            // wires the land speed as the creature's one speed and drops
            // flight.
            Assertions.Equal(40,
                NalfeshneeRules39Policy.EffectiveFlySpeedFeet,
                "The one effective speed is the printed 40-foot fly speed.");
            Assertions.Equal(30,
                NalfeshneeRules39Policy.OmittedLandSpeedFeet,
                "The printed 30-foot land speed is recorded as omitted.");
            Assertions.True(NalfeshneeRules39Policy.EffectiveFlySpeedFeet >
                NalfeshneeRules39Policy.OmittedLandSpeedFeet,
                "The effective speed is the larger of the two printed modes.");
        }
    }
}
