using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Sprint 39 Nalfeshnee contract. Printed values live here once, and
    /// nothing the engine can derive from live stats is hard-coded as a
    /// result: the spell-like difficulty classes come from the live Charisma
    /// through <see cref="SpellDifficultyClass"/>, the supernatural one from
    /// the live hit dice and Charisma through
    /// <see cref="SupernaturalDifficultyClass"/>, and the printed totals below
    /// exist only so an observer can compare like with like.
    ///
    /// <para><b>The printed defences already include the creature's own
    /// constant <i>unholy aura</i>, so the racial values are derived
    /// separately and the buff supplies the rest.</b> The printed
    /// AC 29 is "+4 deflection, +1 Dex, +16 natural, -2 size"; all three
    /// printed save totals include the aura's +4 resistance; and the printed
    /// CMD 42 includes the same +4 deflection. The blueprint therefore carries
    /// natural armour 16 and the racial save bases below, with no deflection
    /// and no resistance bonus of its own. Adding the aura on top of printed
    /// totals would double every one of those bonuses.</para>
    ///
    /// <para><b>Fourteen racial hit dice but spell-like caster level 12.</b>
    /// The two are not the same number and are not used in the same places.
    /// The hit dice drive base attack, the save bases, skill ranks and the
    /// supernatural difficulty class's half-hit-dice term; the caster level
    /// drives the spell-like difficulty classes, spell resistance penetration
    /// and every caster-level-dependent range.</para>
    ///
    /// <para><b>This creature's rules are not the Glabrezu's.</b> No rend, no
    /// Great Cleave, no Vital Strike, no extra limbs, a different hit-dice and
    /// caster-level pair, and one bite plus two claws rather than five limbs.
    /// Cloning a sibling demon would suggest every one of those wrongly.</para>
    /// </summary>
    internal static class NalfeshneeRules39Policy
    {
        // Fourteen racial hit dice, and a spell-like caster level that is
        // deliberately NOT the hit dice. The installed engine's two
        // ability-parameter paths disagree about which one a reader sees -
        // traced in Sprint 38 and unchanged here: the pre-cast estimate
        // reports the hit dice because RuleCalculateAbilityParams asks for the
        // DERIVED ContextSetAbilityParams, while MechanicsContext
        // .RecalculateAbilityParams asks for the ContextAbilityParamsCalculator
        // BASE type and therefore honours this project's calculator. So the
        // executing context, the applied effect and the spell-resistance
        // check all carry 12, and only the preview shows 14.
        internal const int NalfeshneeHitDice = 14;
        internal const int SpellLikeCasterLevel = 12;
        internal const int PreCastEstimatedCasterLevel = NalfeshneeHitDice;

        // Printed ability scores.
        internal const int Strength = 32;
        internal const int Dexterity = 13;
        internal const int Constitution = 29;
        internal const int Intelligence = 23;
        internal const int Wisdom = 22;
        internal const int Charisma = 20;
        internal const int CharismaBonus = 5;
        internal const int StrengthBonus = 11;
        internal const int ConstitutionBonus = 9;
        internal const int WisdomBonus = 6;
        internal const int DexterityBonus = 1;

        /// <summary>
        /// Racial natural armour, which is the printed +16. The printed AC 29
        /// also contains the aura's +4 deflection; that is not here.
        /// </summary>
        internal const int NaturalArmor = 16;
        internal const int PrintedArmorClass = 29;
        internal const int PrintedTouchArmorClass = 13;
        internal const int PrintedFlatFootedArmorClass = 28;
        /// <summary>
        /// Armour class with no aura active: 10 + 16 natural + 1 Dex - 2 size.
        /// The aura's deflection takes it to the printed 29.
        /// </summary>
        internal const int RacialArmorClassWithoutAura = 25;

        /// <summary>
        /// The creature's OWN general spell resistance, which is CR 14 + 11.
        /// It applies to everything. The aura's conditional resistance against
        /// good spells and good casters is a separate fact that happens to
        /// print the same number; see <see cref="AuraConditionalSpellResistance"/>.
        /// </summary>
        internal const int SpellResistance = 25;
        internal const int ChallengeRating = 14;

        internal const int HitPoints = 203;
        internal const int HitDieSides = 10;
        internal const int HitPointConstitutionTotal = 126;

        // Printed movement. The land and fly values differ and the
        // manoeuvrability is poor; the project's established ordinary-map
        // flying representation uses one effective movement mode, so the
        // printed distinction is recorded rather than implemented as a second
        // mode, a new altitude system or free flight through walls.
        internal const int SpeedFeet = 30;
        internal const int FlySpeedFeet = 40;
        internal const string PrintedFlyManeuverability = "poor";

        // Huge: a 15-foot space and a 15-foot reach. Both printed.
        internal const int SpaceFeet = 15;
        internal const int ReachFeet = 15;
        internal const int SizeArmorClassModifier = -2;
        internal const int SizeCombatManeuverModifier = 2;

        internal const int BaseAttackBonus = 14;
        internal const int PrintedAttackBonus = 23;
        internal const int PrintedCombatManeuverBonus = 27;
        internal const int PrintedCombatManeuverDefense = 42;

        // Printed save totals, and the racial bases the blueprint carries.
        // The printed totals each include the aura's +4 resistance.
        internal const int PrintedFortitude = 22;
        internal const int PrintedReflex = 9;
        internal const int PrintedWill = 21;
        internal const int RacialFortitudeWithoutAura = 18;
        internal const int RacialReflexWithoutAura = 5;
        internal const int RacialWillWithoutAura = 17;

        // All three printed totals derive exactly. The outsider type gives a
        // creature TWO good saves, not three, and which two is per-creature;
        // this entry's are Fortitude and Will, with Reflex poor. On the
        // standard progressions at fourteen hit dice that is a good base of
        // 2 + 14/2 = 9 and a poor base of 14/3 = 4, so:
        //   Fortitude 9 + Constitution 9             = 18, +4 aura = 22
        //   Reflex    4 + Dexterity    1             =  5, +4 aura =  9
        //   Will      9 + Wisdom       6 + Iron Will =  17, +4 aura = 21
        // Iron Will is in the printed feat list and is what closes the Will
        // total; without it the derivation would fall two short, which is
        // exactly the kind of gap that invites a wrong "published anomaly"
        // note instead of reading the feat list.
        internal const int GoodSaveBase = 9;
        internal const int PoorSaveBase = 4;
        internal const int IronWillWillBonus = 2;

        internal const int PrintedPerception = 31;
        internal const int SkillRanks = 14;
        internal const int RacialPerceptionBonus = 8;
        internal const int ClassSkillBonus = 3;

        // Defences that are flat printed facts rather than derivations.
        internal const int DamageReductionAmount = 10;
        internal const string DamageReductionBypass = "good";
        internal const int EnergyResistanceAmount = 10;
        internal const int DarkvisionFeet = 60;
        internal const int TelepathyFeet = 100;

        // Three exact natural weapons in three limbs: one bite and two claws,
        // all primary, so all three take the full Strength bonus. The bite's
        // printed 19-20 threat range is Improved Critical (bite) and must be
        // delivered EXACTLY ONCE - either by the weapon type's own critical
        // roll edge or by a native feat, never by both. The engine's
        // BlueprintWeaponType defaults the edge to 20 and the sheet renders
        // the width as 21 - edge, so an edge of 19 is exactly a 19-20 range.
        internal const int BiteDiceCount = 3;
        internal const int BiteDieSides = 8;
        internal const int BiteCriticalRollEdge = 19;
        internal const int ClawDiceCount = 2;
        internal const int ClawDieSides = 6;
        internal const int ClawCount = 2;
        internal const int PrintedBiteDamageBonus = 11;
        internal const int PrintedClawDamageBonus = 11;

        // Unholy Nimbus (Su). Printed: three times per day, free action,
        // bursts ONE ROUND LATER in a 60-foot radius, any non-demon creature
        // caught in the area makes a DC 22 Will save or is dazed for 1d10
        // rounds, and the save DC is Charisma-based.
        //
        // It is neither the Vrock's instant Stunning Screech nor a nausea
        // aura, and it is not the constant unholy aura either. The delay, the
        // condition and the daily limit are all part of its identity.
        internal const int NimbusUsesPerDay = 3;
        internal const int NimbusRadiusFeet = 60;
        internal const int NimbusDelayRounds = 1;
        internal const int NimbusDazeDiceCount = 1;
        internal const int NimbusDazeDieSides = 10;
        internal const int PrintedNimbusDc = 22;

        // Spell-like levels, used only to derive their difficulty classes,
        // their caster-level-dependent ranges and to label the cloned graphs.
        internal const int FeeblemindSpellLevel = 5;
        internal const int GreaterDispelMagicSpellLevel = 6;
        internal const int CallLightningSpellLevel = 3;
        internal const int SlowSpellLevel = 3;
        internal const int UnholyAuraSpellLevel = 8;

        // Printed difficulty classes, for comparison against live values.
        // Greater dispel magic has none: it allows no saving throw.
        internal const int PrintedFeeblemindDc = 20;
        internal const int PrintedCallLightningDc = 18;
        // Slow is also a third-level spell, so the same derivation reaches the
        // same printed 18. It is named separately rather than shared with call
        // lightning's constant: if either printed spell level were ever read
        // differently, one shared number would hide it.
        internal const int PrintedSlowDc = 18;
        internal const int PrintedUnholyAuraDc = 23;

        /// <summary>
        /// Feeblemind's special saving-throw penalty, which applies only to an
        /// arcane spellcaster. It is a penalty to the target's save, not a
        /// bonus to the difficulty class, and arcane casting is established
        /// from an actual arcane spellbook rather than inferred from a high
        /// Intelligence, a class label or the presence of spell-like
        /// abilities.
        /// </summary>
        internal const int FeeblemindArcaneSavePenalty = 4;
        /// <summary>
        /// The score feeblemind reduces Intelligence and Charisma to.
        /// </summary>
        internal const int FeeblemindReducedScore = 1;

        // The printed constant unholy aura's branches, which are four
        // different things and not an armour-class marker. The deflection and
        // resistance bonuses carry their printed bonus TYPES so they do not
        // stack with an equivalent source.
        internal const int AuraDeflectionBonus = 4;
        internal const int AuraResistanceBonus = 4;
        /// <summary>
        /// The aura's CONDITIONAL spell resistance, which applies only against
        /// good spells and spells cast by good creatures. Distinct from
        /// <see cref="SpellResistance"/>, the creature's own general
        /// resistance, even though both print as 25 for this creature. They
        /// are named separately precisely so that coincidence cannot collapse
        /// them into one value.
        /// </summary>
        internal const int AuraConditionalSpellResistance = 25;
        internal const int AuraStrengthDamageDiceCount = 1;
        internal const int AuraStrengthDamageDieSides = 6;

        /// <summary>
        /// A spell-like ability's difficulty class: ten plus the spell level
        /// plus the caster's Charisma bonus. This single formula reproduces
        /// every printed spell-like difficulty class this creature states -
        /// call lightning and slow at spell level 3 give 18, feeblemind at 5
        /// gives 20, and unholy aura at 8 gives 23 - so no spell-like
        /// difficulty class is pinned as a constant in the implementation.
        /// </summary>
        internal static int SpellDifficultyClass(int spellLevel,
            int charismaBonus)
        {
            return 10 + spellLevel + charismaBonus;
        }

        /// <summary>
        /// A supernatural ability's difficulty class: ten plus half the
        /// creature's hit dice plus the relevant ability bonus. Unholy Nimbus
        /// is the only supernatural ability here and its save DC is stated to
        /// be Charisma-based, so this reproduces the printed 22 from fourteen
        /// hit dice and a Charisma bonus of five. A spell-like ability uses
        /// <see cref="SpellDifficultyClass"/> instead; the two formulas are
        /// different and using one for the other would be wrong by two here.
        /// </summary>
        internal static int SupernaturalDifficultyClass(int hitDice,
            int abilityBonus)
        {
            return 10 + hitDice / 2 + abilityBonus;
        }

        /// <summary>
        /// The printed medium range at a given caster level: 100 feet plus 10
        /// feet per caster level. Feeblemind, greater dispel magic and slow
        /// are all medium-range. Derived so a configured distance follows the
        /// selected spell-like caster level rather than being written down a
        /// second time, and so the ability does not silently inherit a fixed
        /// range category that does not read a caster level at all.
        /// </summary>
        internal static int MediumRangeFeet(int casterLevel)
        {
            return 100 + 10 * casterLevel;
        }

        /// <summary>
        /// The printed long range at a given caster level: 400 feet plus 40
        /// feet per caster level. Call lightning is long-range.
        /// </summary>
        internal static int LongRangeFeet(int casterLevel)
        {
            return 400 + 40 * casterLevel;
        }

        /// <summary>
        /// How many separate effects the printed targeted <i>greater dispel
        /// magic</i> may remove: one for every four caster levels, which is
        /// three at this creature's caster level 12.
        ///
        /// <para>This is the number the installed engine cannot express.
        /// <c>ContextActionDispelMagic</c> offers exactly two behaviours -
        /// <c>m_StopAfterFirstRemoved</c> true removes one effect, false
        /// iterates every buff on the target and removes every one that passes
        /// its own dispel check - and the component has no success-count field
        /// of any kind. So the ordinary form maps onto the native action
        /// directly and the greater form does not map onto either setting.</para>
        /// </summary>
        internal static int GreaterDispelMaxRemovals(int casterLevel)
        {
            return casterLevel / 4;
        }

        /// <summary>
        /// Every printed total, re-derived. This is the self-check that the
        /// constants above are consistent with the printed entry rather than
        /// transcribed twice: if a reader ever "corrects" one of them in
        /// isolation, this throws.
        /// </summary>
        internal static void Validate()
        {
            // The spell-like and supernatural difficulty classes derive.
            if (SpellDifficultyClass(CallLightningSpellLevel, CharismaBonus) !=
                    PrintedCallLightningDc ||
                SpellDifficultyClass(SlowSpellLevel, CharismaBonus) !=
                    PrintedSlowDc ||
                SpellDifficultyClass(FeeblemindSpellLevel, CharismaBonus) !=
                    PrintedFeeblemindDc ||
                SpellDifficultyClass(UnholyAuraSpellLevel, CharismaBonus) !=
                    PrintedUnholyAuraDc ||
                SupernaturalDifficultyClass(NalfeshneeHitDice,
                    CharismaBonus) != PrintedNimbusDc)
                throw new InvalidOperationException(
                    "A Nalfeshnee difficulty class no longer derives from its printed inputs.");

            // The supernatural formula is NOT the spell-like one. At these
            // inputs they differ, and this asserts the difference rather than
            // letting a future edit quietly unify them.
            if (SupernaturalDifficultyClass(NalfeshneeHitDice, CharismaBonus) ==
                SpellDifficultyClass(NalfeshneeHitDice, CharismaBonus))
                throw new InvalidOperationException(
                    "The supernatural and spell-like difficulty-class formulas must stay distinct.");

            // The save bases are the standard progressions at this creature's
            // hit dice, and Fortitude and Will are the two good saves.
            if (GoodSaveBase != 2 + NalfeshneeHitDice / 2 ||
                PoorSaveBase != NalfeshneeHitDice / 3)
                throw new InvalidOperationException(
                    "The Nalfeshnee save bases no longer match the standard progressions at its hit dice.");

            // All three printed save totals derive, aura included, and the
            // racial values are exactly the printed ones less the aura.
            if (GoodSaveBase + ConstitutionBonus !=
                    RacialFortitudeWithoutAura ||
                PoorSaveBase + DexterityBonus != RacialReflexWithoutAura ||
                GoodSaveBase + WisdomBonus + IronWillWillBonus !=
                    RacialWillWithoutAura ||
                RacialFortitudeWithoutAura + AuraResistanceBonus !=
                    PrintedFortitude ||
                RacialReflexWithoutAura + AuraResistanceBonus !=
                    PrintedReflex ||
                RacialWillWithoutAura + AuraResistanceBonus != PrintedWill)
                throw new InvalidOperationException(
                    "A Nalfeshnee save total no longer derives, or the aura is being counted twice.");

            // The printed armour class contains the aura's deflection, so the
            // racial value must be the printed one less that bonus.
            if (10 + NaturalArmor + DexterityBonus +
                    SizeArmorClassModifier != RacialArmorClassWithoutAura ||
                RacialArmorClassWithoutAura + AuraDeflectionBonus !=
                    PrintedArmorClass)
                throw new InvalidOperationException(
                    "The Nalfeshnee armour class no longer derives, or the aura's deflection is being counted twice.");

            // Attack, manoeuvre and defence totals derive.
            if (BaseAttackBonus + StrengthBonus + SizeArmorClassModifier !=
                    PrintedAttackBonus ||
                BaseAttackBonus + StrengthBonus +
                    SizeCombatManeuverModifier !=
                    PrintedCombatManeuverBonus ||
                10 + BaseAttackBonus + StrengthBonus + DexterityBonus +
                    SizeCombatManeuverModifier + AuraDeflectionBonus !=
                    PrintedCombatManeuverDefense)
                throw new InvalidOperationException(
                    "A Nalfeshnee attack or manoeuvre total no longer derives.");

            // Both printed natural-attack damage bonuses are the full
            // Strength bonus, because bite and both claws are primary.
            if (PrintedBiteDamageBonus != StrengthBonus ||
                PrintedClawDamageBonus != StrengthBonus)
                throw new InvalidOperationException(
                    "The Nalfeshnee natural attacks are all primary and take the full Strength bonus.");

            // Perception derives from ranks, Wisdom, the racial modifier and
            // the class-skill bonus.
            if (SkillRanks + WisdomBonus + RacialPerceptionBonus +
                    ClassSkillBonus != PrintedPerception)
                throw new InvalidOperationException(
                    "The Nalfeshnee Perception total no longer derives.");

            // Hit points and the general spell resistance derive.
            // The printed 203 is the standard average roll on fourteen d10 -
            // (sides + 1) / 2 per die, which is 5.5 and not 5 - plus the
            // Constitution total. Using sides/2 would give 210 and look close
            // enough to pass an eye check.
            if (SkillRanks != NalfeshneeHitDice ||
                NalfeshneeHitDice * (HitDieSides + 1) / 2 +
                    HitPointConstitutionTotal != HitPoints ||
                NalfeshneeHitDice * ConstitutionBonus !=
                    HitPointConstitutionTotal ||
                ChallengeRating + 11 != SpellResistance)
                throw new InvalidOperationException(
                    "The Nalfeshnee hit points or general spell resistance no longer derive.");

            // The greater dispel cap is caster-level derived, and the printed
            // three at caster level 12 is what it produces.
            if (GreaterDispelMaxRemovals(SpellLikeCasterLevel) != 3)
                throw new InvalidOperationException(
                    "The greater dispel removal cap no longer derives from the caster level.");

            // The caster-level-dependent ranges are derived, not a fixed
            // category. Both must actually move with the caster level.
            if (MediumRangeFeet(SpellLikeCasterLevel) != 220 ||
                LongRangeFeet(SpellLikeCasterLevel) != 880 ||
                MediumRangeFeet(SpellLikeCasterLevel + 2) ==
                    MediumRangeFeet(SpellLikeCasterLevel) ||
                LongRangeFeet(SpellLikeCasterLevel + 2) ==
                    LongRangeFeet(SpellLikeCasterLevel))
                throw new InvalidOperationException(
                    "A Nalfeshnee spell-like range no longer scales with the caster level.");

            // The caster level is deliberately not the hit dice.
            if (SpellLikeCasterLevel == NalfeshneeHitDice)
                throw new InvalidOperationException(
                    "The Nalfeshnee spell-like caster level is 12 and its hit dice are 14; they are not the same number.");

            // Demon membership is deliberately NOT asserted here. This policy
            // owns printed values and their derivations only, so that it can
            // be validated before the creature is registered - which is what
            // the bounded installed-input observation needs. The authoritative
            // membership invariant lives in SummonDemonIdentityCatalog's own
            // Validate(), where the set itself is.
        }
    }
}
