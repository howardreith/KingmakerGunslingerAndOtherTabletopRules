using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class NaturalSummonProfile
    {
        /// <summary>
        /// What the builder has always given every reconstructed creature.
        ///
        /// <para>It is right for the animals this was written for and wrong for
        /// a vermin whose stat block prints no skill ranks at all: a Giant Ant
        /// reads Perception 7 against a printed +5 because four points are its
        /// racial bonus, one is its Wisdom, and two are ranks nothing printed.
        /// A profile that names its own skills overrides this; everything else
        /// keeps it, so no qualified creature moves.</para>
        /// </summary>
        internal static readonly string[] DefaultSkills =
            { "Perception", "Mobility", "Stealth" };

        /// <summary>
        /// The Giant Ant's printed racial Perception bonus. Both castes print
        /// Perception +5 on a Wisdom of 13, so four of those points are racial,
        /// one is the attribute and none is a rank.
        /// </summary>
        internal const int GiantAntRacialPerceptionBonus = 4;

        internal NaturalSummonProfile(string key, string displayName,
            string hitDieClass, int hitDice, string size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speedFeet, int naturalArmor,
            string primaryWeapon, string[] additionalWeapons,
            string[] additionalSecondaryWeapons,
            string[] facts, string[] deviations, string[] skills = null)
        {
            Key = key; DisplayName = displayName; HitDieClass = hitDieClass;
            HitDice = hitDice; Size = size; Strength = strength;
            Dexterity = dexterity; Constitution = constitution;
            Intelligence = intelligence; Wisdom = wisdom; Charisma = charisma;
            SpeedFeet = speedFeet; NaturalArmor = naturalArmor;
            PrimaryWeapon = primaryWeapon;
            AdditionalWeapons = additionalWeapons ?? Array.Empty<string>();
            AdditionalSecondaryWeapons = additionalSecondaryWeapons ??
                Array.Empty<string>();
            Facts = facts ?? Array.Empty<string>();
            Deviations = deviations ?? Array.Empty<string>();
            Skills = skills ?? DefaultSkills;
        }

        internal string Key { get; private set; }
        internal string DisplayName { get; private set; }
        internal string HitDieClass { get; private set; }
        internal int HitDice { get; private set; }
        internal string Size { get; private set; }
        internal int Strength { get; private set; }
        internal int Dexterity { get; private set; }
        internal int Constitution { get; private set; }
        internal int Intelligence { get; private set; }
        internal int Wisdom { get; private set; }
        internal int Charisma { get; private set; }
        internal int SpeedFeet { get; private set; }
        internal int NaturalArmor { get; private set; }
        internal string PrimaryWeapon { get; private set; }
        internal IReadOnlyList<string> AdditionalWeapons { get; private set; }
        internal IReadOnlyList<string> AdditionalSecondaryWeapons
        { get; private set; }
        internal IReadOnlyList<string> Facts { get; private set; }
        internal IReadOnlyList<string> Deviations { get; private set; }

        /// <summary>
        /// The skills this creature has class ranks in. Empty means none, which
        /// is what a printed stat block showing no skill ranks requires.
        /// </summary>
        internal IReadOnlyList<string> Skills { get; private set; }
    }

    internal static class ExpandedSummoningNaturalProfiles
    {
        // Shared by construction and domain validation, so a profile cannot
        // register an armor value the live builder cannot resolve. The +15
        // native identity is pinned by the October 2 loaded-blueprint census.
        private static readonly IDictionary<int, string> NaturalArmorGuids =
            new Dictionary<int, string> {
                { 1, "10c7c5e3c5806bc4ca676e22d6fbf17e" },
                { 2, "45a52ce762f637f4c80cc741c91f58b7" },
                { 3, "f6e106931f95fec4eb995f0d0629fb84" },
                { 4, "16fc201a83edcde4cbd64c291ebe0d07" },
                { 5, "7661741dbb9604842a642457456fd0e4" },
                { 6, "987ba44303e88054c9504cb3083ba0c9" },
                { 7, "e73864391ccf0894997928443a29d755" },
                { 8, "b9342e2a6dc5165489ba3412c50ca3d1" },
                { 9, "da6417809bdedfa468dd2fd0cc74be92" },
                { 10, "4179c5c08d606a6439a62bf178b738e1" },
                // Private Gorgon chassis: the reconstruction catalog has no +11 native binding.
                { 11, "3d4461a55b224472ad2853699c596b4f" },
                { 12, "0b2d92c6aac8093489dfdadf1e448280" },
                { 14, "209a2920891b580418b4e5e80466e134" },
                { 15, "72c294dca841e3944869fb087bacf272" },
                { 22, "eee672c8f6555b445a89dbbb91361d64" }
            };

        internal static string NaturalArmorGuid(int bonus)
        {
            if (bonus == 0) return null;
            string guid;
            if (!NaturalArmorGuids.TryGetValue(bonus, out guid))
                throw new InvalidOperationException(
                    "Unsupported natural armor value " + bonus + ".");
            return guid;
        }

        /// <summary>
        /// Racial hit-die classes the natural builder can bind. Sprint 3 added
        /// the magical beast (Owlbear) and humanoid (Cyclops) classes to the
        /// animal and vermin classes of the earlier tiers; Sprint 4 added the
        /// plant class (Shambling Mound, Giant Flytrap), whose progression
        /// carries the plant traits.
        /// </summary>
        internal static readonly string[] SupportedHitDieClasses = {
            "Animal", "Vermin", "MagicalBeast", "Humanoid", "Plant"
        };
        private static readonly NaturalSummonProfile[] Values = Build();
        internal static IReadOnlyList<NaturalSummonProfile> All
        { get { return Array.AsReadOnly(Values); } }

        internal static NaturalSummonProfile For(string key)
        { return Values.Single(value => value.Key == key); }

        internal static void Validate()
        {
            if (Values.Length != 61 || Values.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != Values.Length)
                throw new InvalidOperationException(
                    "The natural reconstruction catalog is incomplete or duplicated.");
            foreach (NaturalSummonProfile value in Values)
            {
                NaturalArmorGuid(value.NaturalArmor);
                if (!ExpandedSummoningCatalog.All.Any(creature =>
                        creature.Key == value.Key) ||
                    !SupportedHitDieClasses.Contains(value.HitDieClass) ||
                    value.HitDice < 1 || value.SpeedFeet < 1 ||
                    value.NaturalArmor < 0 ||
                    string.IsNullOrEmpty(value.PrimaryWeapon))
                    throw new InvalidOperationException(
                        "Invalid natural summon profile: " + value.Key + ".");
            }
            NaturalSummonProfile frog = For("poisonous-frog");
            if (frog.Size != "Tiny" || frog.Strength != 2 ||
                frog.PrimaryWeapon != "Bite1" ||
                !frog.Facts.Contains("PoisonFrog"))
                throw new InvalidOperationException(
                    "Poisonous Frog tabletop profile changed.");
            NaturalSummonProfile wolverine = For("wolverine");
            if (wolverine.AdditionalSecondaryWeapons.Count != 0 ||
                wolverine.AdditionalWeapons.Count != 2 ||
                !wolverine.AdditionalWeapons.Contains("Claw1d6") ||
                !wolverine.AdditionalWeapons.Contains("Bite1d4") ||
                !wolverine.Facts.Contains("TripDefenseFourLegs") ||
                !wolverine.Facts.Contains("WolverineRage"))
                throw new InvalidOperationException(
                    "Wolverine printed attack routine changed.");
            SummonRagePolicy.Validate();
            NaturalSummonProfile spider = For("giant-spider");
            if (spider.HitDice != 3 || spider.NaturalArmor != 1 ||
                !spider.Facts.Contains("GiantSpiderPoison"))
                throw new InvalidOperationException(
                    "Giant Spider tabletop profile changed.");
            NaturalSummonProfile ape = For("ape");
            if (ape.Size != "Large" || ape.HitDice != 3 || ape.Strength != 15 ||
                ape.Dexterity != 15 || ape.Constitution != 14 ||
                ape.Intelligence != 2 || ape.Wisdom != 12 ||
                ape.Charisma != 7 || ape.SpeedFeet != 30 ||
                ape.NaturalArmor != 3 || ape.PrimaryWeapon != "Slam1d6" ||
                ape.AdditionalWeapons.Count != 1 ||
                ape.AdditionalWeapons[0] != "Slam1d6" ||
                ape.AdditionalSecondaryWeapons.Count != 0 ||
                !ape.Facts.Contains("GreatFortitude") ||
                !ape.Facts.Contains("SkillFocusPerception") ||
                // Not a printed feat: the carrier that keeps both slams on
                // the plain Strength modifier the stat block prints.
                !ape.Facts.Contains("PrimateFullStrengthLimbs") ||
                ape.Facts.Count != 3 ||
                ape.Facts.Contains("ReducedReach") ||
                ape.Facts.Contains("TripDefenseFourLegs") ||
                !ape.Skills.SequenceEqual(new[] { "Mobility", "Perception" }))
                throw new InvalidOperationException(
                    "Ape printed attack routine or profile changed.");
            NaturalSummonProfile dire = For("dire-ape");
            if (dire.Size != "Large" || dire.HitDice != 4 ||
                dire.Strength != 19 || dire.Dexterity != 15 ||
                dire.Constitution != 16 || dire.Intelligence != 2 ||
                dire.Wisdom != 12 || dire.Charisma != 7 ||
                dire.SpeedFeet != 30 || dire.NaturalArmor != 4 ||
                dire.PrimaryWeapon != "DireApeBite1d6" ||
                dire.AdditionalWeapons.Count != 2 ||
                dire.AdditionalWeapons.Any(value => value != "DireApeClaw1d4") ||
                dire.AdditionalSecondaryWeapons.Count != 0 ||
                !dire.Facts.Contains("IronWill") ||
                !dire.Facts.Contains("SkillFocusPerception") ||
                !dire.Facts.Contains("DireApeRend") ||
                !dire.Facts.Contains("PrimateFullStrengthLimbs") ||
                dire.Facts.Count != 4 ||
                dire.Facts.Contains("ReducedReach") ||
                !dire.Skills.SequenceEqual(new[] {
                    "Mobility", "Perception", "Stealth" }))
                throw new InvalidOperationException(
                    "Dire Ape printed attack routine or profile changed.");
            NaturalSummonProfile crab = For("giant-crab");
            if (crab.Size != "Medium" || crab.HitDieClass != "Vermin" ||
                crab.HitDice != GiantCrabRulesPolicy.HitDice ||
                crab.Strength != GiantCrabRulesPolicy.Strength ||
                crab.Dexterity != GiantCrabRulesPolicy.Dexterity ||
                crab.Constitution != GiantCrabRulesPolicy.Constitution ||
                crab.Intelligence !=
                    GiantCrabRulesPolicy.SubstitutedIntelligence ||
                crab.Wisdom != GiantCrabRulesPolicy.Wisdom ||
                crab.Charisma != GiantCrabRulesPolicy.Charisma ||
                crab.SpeedFeet != GiantCrabRulesPolicy.SpeedFeet ||
                crab.NaturalArmor != GiantCrabRulesPolicy.NaturalArmor ||
                crab.PrimaryWeapon != "Claw1d4" ||
                // One more limb and no third: two claws is the whole printed
                // routine.
                crab.AdditionalWeapons.Count != 1 ||
                crab.AdditionalWeapons[0] != "Claw1d4" ||
                crab.AdditionalSecondaryWeapons.Count != 0)
                throw new InvalidOperationException(
                    "The Giant Crab profile no longer matches its printed stat "
                    + "block.");
            // A mindless creature buys no ranks, so an empty skill list is the
            // whole point of this entry rather than an oversight.
            if (crab.Skills.Count != 0)
                throw new InvalidOperationException(
                    "The Giant Crab has no Intelligence score and must "
                    + "therefore carry no skill ranks at all.");
            if (!crab.Facts.Contains("GiantCrabTripDefense") ||
                !crab.Facts.Contains("GiantCrabRacialSkills") ||
                !crab.Facts.Contains("GiantCrabMindlessImmunity") ||
                !crab.Facts.Contains("PrimateFullStrengthLimbs"))
                throw new InvalidOperationException(
                    "The Giant Crab is missing a fact its printed block "
                    + "requires.");
            // Not the shared native carrier, for the reason Sprint 20
            // measured: that one is worth +8 and this creature prints +12.
            if (crab.Facts.Contains("TripDefenseEightLegs"))
                throw new InvalidOperationException(
                    "The shared eight-leg carrier is worth "
                    + GiantCrabRulesPolicy.SharedNativeTripBonus
                    + " and this creature prints "
                    + GiantCrabRulesPolicy.EightLegTripBonus + ".");
            // Nor the scorpion's, which is named and described for a scorpion.
            if (crab.Facts.Contains("GiantScorpionTripDefense") ||
                crab.Facts.Contains("GiantScorpionRacialSkills"))
                throw new InvalidOperationException(
                    "A crab must not wear a scorpion's named feature.");
            GiantCrabRulesPolicy.Validate();

            NaturalSummonProfile scorpion = For("giant-scorpion");
            if (scorpion.Size != "Large" || scorpion.HitDieClass != "Vermin" ||
                scorpion.HitDice != GiantScorpionRulesPolicy.HitDice ||
                scorpion.Strength != GiantScorpionRulesPolicy.Strength ||
                scorpion.Dexterity != GiantScorpionRulesPolicy.Dexterity ||
                scorpion.Constitution != GiantScorpionRulesPolicy.Constitution ||
                scorpion.Intelligence !=
                    GiantScorpionRulesPolicy.SubstitutedIntelligence ||
                scorpion.Wisdom != GiantScorpionRulesPolicy.Wisdom ||
                scorpion.Charisma != GiantScorpionRulesPolicy.Charisma ||
                scorpion.SpeedFeet != GiantScorpionRulesPolicy.SpeedFeet ||
                scorpion.NaturalArmor != GiantScorpionRulesPolicy.NaturalArmor ||
                scorpion.PrimaryWeapon != "GiantScorpionClaw1d6" ||
                // Two more limbs: the second claw and the sting. A scorpion
                // whose sting shares the claws' weapon would poison with a
                // claw and grab with its sting.
                scorpion.AdditionalWeapons.Count != 2 ||
                scorpion.AdditionalWeapons[0] != "GiantScorpionClaw1d6" ||
                scorpion.AdditionalWeapons[1] != "GiantScorpionSting1d6" ||
                scorpion.AdditionalSecondaryWeapons.Count != 0)
                throw new InvalidOperationException(
                    "The Giant Scorpion profile no longer matches its printed "
                    + "stat block.");
            // A mindless creature buys no ranks, so an empty skill list is the
            // whole point of this entry rather than an oversight: the default
            // list would grant three ranks the stat block never prints.
            if (scorpion.Skills.Count != 0)
                throw new InvalidOperationException(
                    "The Giant Scorpion has no Intelligence score and must "
                    + "therefore carry no skill ranks at all.");
            if (!scorpion.Facts.Contains("GiantScorpionTripDefense") ||
                !scorpion.Facts.Contains("GiantScorpionRacialSkills") ||
                !scorpion.Facts.Contains("GiantScorpionPoison") ||
                !scorpion.Facts.Contains("GiantScorpionMindlessImmunity") ||
                !scorpion.Facts.Contains("PrimateFullStrengthLimbs"))
                throw new InvalidOperationException(
                    "The Giant Scorpion is missing a fact its printed block "
                    + "requires.");
            // Not the shared native carrier. The first guarded review measured
            // that one delivering +8 where this creature prints +12, so taking
            // it would read two points light on an anti-trip defence the stat
            // block states exactly.
            if (scorpion.Facts.Contains("TripDefenseEightLegs"))
                throw new InvalidOperationException(
                    "The shared eight-leg carrier is worth "
                    + GiantScorpionRulesPolicy.SharedNativeTripBonus
                    + " and this creature prints "
                    + GiantScorpionRulesPolicy.EightLegTripBonus + ".");
            GiantScorpionRulesPolicy.Validate();

            NaturalSummonProfile girallon = For("girallon");
            if (girallon.Size != "Large" || girallon.HitDieClass != "MagicalBeast" ||
                girallon.HitDice != 7 || girallon.Strength != 19 ||
                girallon.Dexterity != 17 || girallon.Constitution != 18 ||
                girallon.Intelligence != 2 || girallon.Wisdom != 12 ||
                girallon.Charisma != 7 || girallon.SpeedFeet != 40 ||
                girallon.NaturalArmor != 6 ||
                girallon.PrimaryWeapon != "GirallonBite1d6" ||
                // Four claws, not two. A girallon that rends on two is the
                // single most likely way to get this creature wrong.
                girallon.AdditionalWeapons.Count != 4 ||
                girallon.AdditionalWeapons.Any(value => value != "GirallonClaw1d4") ||
                girallon.AdditionalSecondaryWeapons.Count != 0 ||
                !girallon.Facts.Contains("ImprovedInitiative") ||
                !girallon.Facts.Contains("IronWill") ||
                !girallon.Facts.Contains("SkillFocusPerception") ||
                !girallon.Facts.Contains("Toughness") ||
                !girallon.Facts.Contains("GirallonRend") ||
                !girallon.Facts.Contains("PrimateFullStrengthLimbs") ||
                girallon.Facts.Count != 6 ||
                girallon.Facts.Contains("ReducedReach") ||
                girallon.Facts.Contains("DireApeRend") ||
                !girallon.Skills.SequenceEqual(new[] { "Perception", "Stealth" }))
                throw new InvalidOperationException(
                    "Girallon printed attack routine or profile changed.");
            PrimateRulesPolicy.Validate();
            PrimateVisualPolicy.Validate();
            GirallonRulesPolicy.Validate();
            NaturalSummonProfile triceratops = For(
                ArmoredDinosaurRulesPolicy.TriceratopsKey);
            NaturalSummonProfile stegosaurus = For(
                ArmoredDinosaurRulesPolicy.StegosaurusKey);
            if (triceratops.HitDieClass != "Animal" ||
                triceratops.HitDice != 14 || triceratops.Size != "Huge" ||
                triceratops.PrimaryWeapon != "TriceratopsGore2d10" ||
                triceratops.AdditionalWeapons.Count != 0 ||
                !triceratops.Skills.SequenceEqual(new[] { "Perception" }) ||
                stegosaurus.HitDieClass != "Animal" ||
                stegosaurus.HitDice != 12 || stegosaurus.Size != "Huge" ||
                stegosaurus.PrimaryWeapon != "StegosaurusTail4d6" ||
                stegosaurus.AdditionalWeapons.Count != 0 ||
                !stegosaurus.Skills.SequenceEqual(new[] { "Perception" }) ||
                stegosaurus.Facts.Contains("AnkylosaurusStun"))
                throw new InvalidOperationException(
                    "Sprint 23 armored-dinosaur profile changed.");
            ArmoredDinosaurRulesPolicy.Validate();
            NaturalSummonProfile deinonychus = For(
                TheropodRulesPolicy.DeinonychusKey);
            NaturalSummonProfile tyrannosaurus = For(
                TheropodRulesPolicy.TyrannosaurusKey);
            if (deinonychus.HitDieClass != "Animal" ||
                deinonychus.HitDice != 4 ||
                deinonychus.Size != "Medium" ||
                deinonychus.Strength != 15 ||
                deinonychus.Dexterity != 15 ||
                deinonychus.Constitution != 19 ||
                deinonychus.SpeedFeet != 60 ||
                deinonychus.NaturalArmor != 3 ||
                deinonychus.PrimaryWeapon != "DeinonychusTalon1d8" ||
                deinonychus.AdditionalWeapons.Count != 2 ||
                deinonychus.AdditionalWeapons[0] !=
                    "DeinonychusTalon1d8" ||
                deinonychus.AdditionalWeapons[1] != "Bite1d6" ||
                deinonychus.AdditionalSecondaryWeapons.Count != 1 ||
                deinonychus.AdditionalSecondaryWeapons[0] != "Claw1d4" ||
                !deinonychus.Facts.Contains("Pounce") ||
                deinonychus.Facts.Contains("TripDefenseFourLegs") ||
                !deinonychus.Skills.SequenceEqual(new[] {
                    "Perception", "Stealth" }) ||
                tyrannosaurus.HitDieClass != "Animal" ||
                tyrannosaurus.HitDice != 18 ||
                tyrannosaurus.Size != "Gargantuan" ||
                tyrannosaurus.Strength != 32 ||
                tyrannosaurus.Dexterity != 13 ||
                tyrannosaurus.Constitution != 19 ||
                tyrannosaurus.SpeedFeet != 40 ||
                tyrannosaurus.NaturalArmor != 14 ||
                tyrannosaurus.PrimaryWeapon != "TyrannosaurusBite4d6" ||
                tyrannosaurus.AdditionalWeapons.Count != 0 ||
                tyrannosaurus.AdditionalSecondaryWeapons.Count != 0 ||
                !tyrannosaurus.Facts.Contains("CriticalFocus") ||
                !tyrannosaurus.Facts.Contains("ImprovedCriticalBite") ||
                !tyrannosaurus.Facts.Contains(
                    "TyrannosaurusBleedingCritical") ||
                tyrannosaurus.Facts.Contains("Pounce") ||
                tyrannosaurus.Facts.Contains("PowerAttack") ||
                !tyrannosaurus.Skills.SequenceEqual(new[] { "Perception" }))
                throw new InvalidOperationException(
                    "Sprint 24 theropod profile changed.");
            TheropodRulesPolicy.Validate();
            ValidateGiantBodies();
        }

        /// <summary>
        /// The Sprint 25 giant bodies, field by field against their frozen
        /// policy rather than against repeated literals. Brachiosaurus is
        /// the lone-natural-weapon creature and the one whose reach equals
        /// its space, so both of those are asserted here as properties of
        /// this creature and not of its size class.
        /// </summary>
        private static void ValidateGiantBodies()
        {
            NaturalSummonProfile brachiosaurus = For(
                GiantBodyRulesPolicy.BrachiosaurusKey);
            if (brachiosaurus.HitDieClass != "Animal" ||
                brachiosaurus.HitDice !=
                    GiantBodyRulesPolicy.BrachiosaurusHitDice ||
                brachiosaurus.Size != "Gargantuan" ||
                brachiosaurus.Strength !=
                    GiantBodyRulesPolicy.BrachiosaurusStrength ||
                brachiosaurus.Dexterity !=
                    GiantBodyRulesPolicy.BrachiosaurusDexterity ||
                brachiosaurus.Constitution !=
                    GiantBodyRulesPolicy.BrachiosaurusConstitution ||
                brachiosaurus.Wisdom !=
                    GiantBodyRulesPolicy.BrachiosaurusWisdom ||
                brachiosaurus.SpeedFeet !=
                    GiantBodyRulesPolicy.BrachiosaurusSpeedFeet ||
                brachiosaurus.NaturalArmor !=
                    GiantBodyRulesPolicy.BrachiosaurusNaturalArmor ||
                brachiosaurus.PrimaryWeapon != "BrachiosaurusTail4d6" ||
                // One natural attack, which is what makes the
                // one-and-a-half Strength routine correct for it.
                brachiosaurus.AdditionalWeapons.Count != 0 ||
                brachiosaurus.AdditionalSecondaryWeapons.Count != 0 ||
                !brachiosaurus.Facts.Contains("BrachiosaurusChassis") ||
                !brachiosaurus.Facts.Contains("WeaponFocusTail") ||
                !brachiosaurus.Facts.Contains("TripDefenseFourLegs") ||
                !brachiosaurus.Facts.Contains("Cleave") ||
                // The product-native standalone run requires this real
                // feature, unlike the installed-stack census's wiring.
                !brachiosaurus.Facts.Contains("GreatCleave") ||
                !brachiosaurus.Facts.Contains("GreaterOverrun") ||
                // Never invented for this creature.
                brachiosaurus.Facts.Contains("PowerfulCharge") ||
                brachiosaurus.Facts.Contains("Pounce") ||
                brachiosaurus.Facts.Contains("Grab") ||
                !brachiosaurus.Skills.SequenceEqual(
                    new[] { "Perception" }) ||
                // The trample is registered now, so the assertion is that
                // it says so - and that it still refuses to call itself
                // qualified, which is a different claim.
                !brachiosaurus.Deviations.Any(value =>
                    value.Contains("Registered is not qualified")))
                throw new InvalidOperationException(
                    "Sprint 25 Brachiosaurus profile changed.");
            GiantBodyRulesPolicy.Validate();
        }

        private static NaturalSummonProfile[] Build()
        {
            return new[] {
                PSK("gorgon", "Gorgon", "MagicalBeast", 8, "Large",
                    24, 10, 24, 2, 16, 9, 30, 11, "GorgonGore2d8",
                    A(), A("GorgonHoof1d6", "GorgonHoof1d6"),
                    A("ImprovedInitiative", "IronWill", "PowerAttack", "SkillFocusPerception",
                        "ReducedReach", "TripDefenseFourLegs"),
                    A("Perception"), "Charter-selected SM VIII extension; passive senses unmodeled."),
                PK("bulette", "Bulette", "MagicalBeast", 8, "Huge",
                    23, 15, 20, 2, 13, 6, 40, 12, "BuletteBite2d8",
                    A("BuletteClaw2d6", "BuletteClaw2d6"),
                    A("ImprovedInitiative", "IronWill", "SkillFocusPerception", "WeaponFocusBite",
                        "ReducedReach", "TripDefenseFourLegs", "BuletteChassis"),
                    A("Mobility", "Perception"),
                    "Surface-only legal charge/leap; burrow 20 feet and tremorsense unsupported."),
                PK("viper", "Viper", "Animal", 2, "Medium",
                    8, 13, 14, 1, 13, 2, 20, 3, "Bite1d4",
                    Array.Empty<string>(),
                    A("TripImmune", "ImprovedInitiative", "WeaponFinesse"),
                    A("Perception", "Stealth"),
                    "Sprint 17 frozen contract: Medium Viper, not a Tiny or Small substitute. Hidden pending complete runtime qualification. Creature-owned injury poison: Fortitude DC 13 at baseline, 1d2 Constitution damage for six exposures, one save cures; the DC follows live Constitution.",
                    "Exact land ranks and racial bonuses retain native attribute, feat and class-skill contributions. Swim/climb movement and water-only consumers are omitted under land-use scope. Scent and low-light vision remain PASSIVE_CREATURE_SENSES_UNMODELED without substitute senses."),
                PK("constrictor-snake", "Constrictor Snake", "Animal", 3,
                    "Medium", 17, 17, 12, 1, 12, 2, 20, 2, "Bite1d4",
                    Array.Empty<string>(),
                    A("TripImmune", "SkillFocusPerception", "Toughness"),
                    A("Mobility", "Perception", "Stealth"),
                    "Hidden pending Sprint 17 runtime qualification. Bite-only grab against a foe of its own size or smaller; constrict 1d4+4 at baseline uses live Strength and size on the shared session-scoped hold lifecycle.",
                    "Exact land ranks and racial bonuses retain native attribute, feat and class-skill contributions. Swim/climb movement and aquatic skills are omitted rather than substituted. Scent and low-light vision remain PASSIVE_CREATURE_SENSES_UNMODELED. Active holds reset cleanly on reload under ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD."),
                P("dire-rat", "Dire Rat", "Animal", 1, "Small",
                    10, 17, 13, 2, 13, 4, 40, 0, "Bite1d4",
                    Array.Empty<string>(),
                    A("TripDefenseFourLegs",
                        "SkillFocusPerception", "DireRatDisease"),
                    "A bite that hits and deals positive damage makes the printed DC 11 Fortitude save before applying the native Filth Fever payload and cure lifecycle.",
                    "The native Dog rig is a bounded locomotion donor only, supplying skeleton and animation; the shipped silhouette is the project's own KMG_dire-rat_Original mesh."),
                P("dog", "Dog", "Animal", 1, "Small",
                    13, 13, 15, 2, 12, 6, 40, 1, "Bite1d4",
                    Array.Empty<string>(),
                    A("TripDefenseFourLegs", "SkillFocusPerception")),
                P("eagle", "Eagle", "Animal", 1, "Small",
                    10, 15, 12, 2, 15, 7, 80, 1, "Bite1d4",
                    A("Claw1d4", "Claw1d4"),
                    A("WeaponFinesse", "Airborne"),
                    "Kingmaker exposes one movement speed; 80-foot fly speed is used with airborne navigation and the 10-foot ground speed is omitted."),
                P("poisonous-frog", "Poisonous Frog", "Animal", 1, "Tiny",
                    2, 12, 11, 1, 9, 10, 10, 0, "Bite1",
                    Array.Empty<string>(),
                    A("WeaponFinesse", "TripDefenseFourLegs", "PoisonFrog"),
                    "The native Constitution-scaled poison graph supplies the exact six-tick 1d2 Constitution effect; ordinary-map ground speed is used and swim movement is omitted."),
                PK("fire-beetle", "Fire Beetle", "Vermin", 1, "Small",
                    10, 11, 11, 1, 10, 7, 30, 1, "Bite1d4",
                    Array.Empty<string>(),
                    A("Airborne", "TripDefenseEightLegs",
                        "FireBeetleLuminescence"),
                    Array.Empty<string>(),
                    "Kingmaker exposes one movement speed; the 30-foot fly speed is used with airborne navigation and the equal 30-foot ground speed is omitted. Poor maneuverability has no native representation and is omitted. An absent Intelligence score is represented as 1.",
                    "The printed CMD 17 against trip is carried by the project's multi-legged trip defence. The first guarded audit measured this creature at CMD 9 and trip 9: the airborne adaptation had not made it untrippable, as was thought possible, and the bonus was simply absent.",
                    "Luminescence is a view-local light matching the painted glands and a tooltip that says the beetle glows. Kingmaker has no mechanics-layer illumination model, so it grants and denies nothing, and the source's 1d6 days of after-death glow has no consumer because a summoned body vanishes with the summon.",
                    "The printed low-light vision is omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, accepted 2026-10-03. The stat block's explicit absence of darkvision is exact and was proved on the live unit, which reads no darkvision carrier of any kind."),
                P("giant-centipede", "Giant Centipede", "Vermin", 1,
                    "Medium", 9, 15, 12, 1, 10, 2, 40, 2, "Bite1d6",
                    Array.Empty<string>(),
                    A("WeaponFinesse", "TripImmune", "CentipedePoison"),
                    "Kingmaker cannot represent an absent Intelligence score on BlueprintUnit, so Intelligence 1 is used. Climb movement is omitted; native poison is conservative because its graph does not expose the tabletop +2 racial DC bonus."),
                PK("giant-ant-worker", "Giant Ant (Worker)", "Vermin", 2,
                    "Medium", 14, 10, 17, 1, 13, 11, 50, 5, "Bite1d6",
                    Array.Empty<string>(),
                    A("Toughness", "TripDefenseEightLegs", "GiantAntRacialSkills"),
                    Array.Empty<string>(),
                    "Kingmaker exposes one movement speed; the 50-foot ground speed is used and the 20-foot climb is omitted. An absent Intelligence score is represented as 1.",
                    "The printed racial +4 Survival is omitted because Kingmaker has no Survival skill and this project has consistently omitted that half rather than substituting another skill; Lore (Nature) is a knowledge stat for identifying creatures and is not a defensible analogue for tracking and foraging. The printed racial +4 Perception is implemented exactly.",
                    "The printed darkvision 60 feet and scent are omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, accepted 2026-10-03. Kingmaker models none of Scent, Darkvision or Low-light Vision: the whole loaded blueprint library carries no component that could express scent, and no enum reachable from BlueprintUnit, UnitEntityData or UnitDescriptor holds a darkvision value. Nothing is substituted for them - not AddBlindsight, which is a different rule this project implements exactly for the Dire Bat, and not OverrideVisionRange, which is a general detection radius in all conditions - and no record claims the omitted traits work.",
                    "The Worker template removes the soldier's poison sting and its grab, which leaves a bite alone, so this caste carries neither carrier. Its smaller head, lighter mandibles, absent sting and lighter chitin are what tell a player which caste is in front of them."),
                PK("giant-ant-soldier", "Giant Ant (Soldier)", "Vermin", 2,
                    "Medium", 14, 10, 17, 1, 13, 11, 50, 5, "Bite1d6",
                    A("AntSting1d4"),
                    A("Toughness", "TripDefenseEightLegs", "GiantAntRacialSkills",
                        "GiantAntPoison"),
                    Array.Empty<string>(),
                    "Kingmaker exposes one movement speed; the 50-foot ground speed is used and the 20-foot climb is omitted. An absent Intelligence score is represented as 1.",
                    "The printed racial +4 Survival is omitted for the reason recorded on the Worker; the Perception half is exact. Both castes carry the project's multi-legged trip defence, which the Giant Centipede already uses, so the printed CMD 13 and 21 against trip hold on both.",
                    "The printed darkvision 60 feet and scent are omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, accepted 2026-10-03, on the same evidence and with the same prohibitions recorded on the Worker.",
                    "The bite's grab rides the shared summon grapple lifecycle (Sprint 6) on the primary limb only, and the sting's poison is gated on the sting's own weapon type (Sprint 10), so neither reaches the other attack. The printed Fortitude DC 14 is what the Constitution-scaled formula produces unaided and is not hard-coded."),
                PK("giant-ant-drone", "Giant Ant (Drone)", "Vermin", 2,
                    "Medium", 18, 14, 21, 1, 17, 15, 30, 7, "Bite1d6",
                    A("AntSting1d4"),
                    A("Airborne", "Toughness", "TripDefenseEightLegs",
                        "GiantAntRacialSkills", "GiantAntPoison"),
                    Array.Empty<string>(),
                    "Kingmaker exposes one movement speed; the 30-foot average fly speed is used with airborne navigation, and the soldier's 50-foot ground speed and 20-foot climb are omitted. That leaves this caste slower on the ground than the soldier it is built from, which is the conservative direction and is preferred to overstating a flier's ground movement. An absent Intelligence score is represented as 1.",
                    "Every ability score is the soldier's with the advanced simple template applied, written out here rather than computed at load because the frozen contract requires it: Strength 14 to 18, Dexterity 10 to 14, Constitution 17 to 21, Wisdom 13 to 17, Charisma 11 to 15, Intelligence unchanged because the template excludes it, and natural armour 5 to 7. Hit dice stay at 2 because the template adds none.",
                    "The frozen contract describes that template as also granting +2 to all skills. A simple template offers two mutually exclusive routes: a quick set of flat bonuses applied to the printed numbers, or a rebuild from adjusted ability scores. The flat +2 to skills belongs to the quick route, and this profile takes the rebuild the contract itself demands - every score written out from the soldier - so skills derive from the advanced Wisdom instead. Perception is therefore +7, the +3 from Wisdom 17 plus the exact racial +4, and not +9; adding the flat bonus on top of the rebuilt score would count the same increase twice and make this creature stronger than its printed form. The discrepancy is raised on the contract page as an erratum rather than resolved silently here.",
                    "The poison needs no second graph. Its difficulty class is computed live from the caster's own Constitution, so the shared Giant Ant poison feature gives the soldier DC 14 on Constitution 17 and this caste DC 16 on the advanced 21, with the same 1d2 Strength over four rounds cured by one save.",
                    "The printed darkvision 60 feet and scent are omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, accepted 2026-10-03. They no longer hold this caste out of publication; it waits on its own Sprint 15 gates and on nothing else."),
                PK("giant-stag-beetle", "Giant Stag Beetle", "Vermin", 7,
                    "Large", 19, 10, 15, 1, 10, 9, 20, 8, "Bite2d8",
                    Array.Empty<string>(),
                    A("ReducedReach", "TripDefenseEightLegs"),
                    Array.Empty<string>(),
                    "Kingmaker exposes one movement mode; the 20-foot ground speed is used and the equal 20-foot poor fly speed is omitted, so no movement rate is lost - only the mode. Ground is the right mode for a heavy Large beetle and is the only one its trample can use. The Fire Beetle resolves the same equal-speed choice the other way because its flight is characterful and it does not trample; the asymmetry is deliberate rather than an oversight. Poor maneuverability has no native representation. An absent Intelligence score is represented as 1.",
                    "The printed Space 10 feet with Reach 5 feet is a reduced reach for a Large creature and uses the project's reduced-reach carrier, the same one the Large ungulates use.",
                    "Trample reuses the project's qualified trample carrier rather than a new graph, and the derivation lands on the printed line exactly: damage is one and a half times Strength on 1d6, which is 1d6+6 at Strength 19, and the save is 10 plus half the hit dice plus the Strength modifier, which is DC 17 at 7 hit dice. It uses the disclosed Kingmaker automatic-attack-of-opportunity-or-Reflex adaptation and carries no Stampede, which belongs to the herd ungulates alone.",
                    "The printed darkvision 60 feet is omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, accepted 2026-10-03. This creature prints no scent, so that half of the label does not apply to it."),
                P("giant-spider", "Giant Spider", "Vermin", 3, "Medium",
                    11, 17, 12, 1, 10, 2, 30, 1, "Bite1d6",
                    Array.Empty<string>(),
                    A("TripDefenseEightLegs", "GiantSpiderPoison", "Blindsight",
                        "SpiderWebImmunity"),
                    "Kingmaker cannot represent an absent Intelligence score, so Intelligence 1 is used. The native 60-foot blindsight stands in for tremorsense; climb movement is omitted (no save-safe seam).",
                    "Web is a bounded ranged ability on the special builder (Sprint 6; correction order): a 50-foot ranged touch attack through the game's own projectile delivery against one foe up to one size larger than the spider, no saving throw, that entangles and holds it through the native web-grappled state until its Constitution-based break-free check succeeds (at most ten rounds); two uses per summoning; the spider is immune to its own webs."),
                P("goblin-dog", "Goblin Dog", "Animal", 1, "Medium",
                    15, 14, 15, 2, 12, 8, 50, 1, "Bite1d6",
                    Array.Empty<string>(), A("Toughness", "GoblinDogTraits"),
                    "Disease immunity uses the native disease-descriptor gate. The printed allergic reaction exposes a non-goblinoid creature damaged by the bite, a creature that deals damage to the Goblin Dog with a natural weapon or unarmed attack, and a creature that attempts to grapple it; each makes the printed DC 12 Fortitude save and a failure applies one nonstacking day of -2 Dexterity and -2 Charisma, removed by positive magical healing or remove disease. Riding contact is omitted because the charter excludes mounted combat. The goblinoid exemption is the exact enumerated set of native goblinoid unit types the installed library carries.",
                    "The Worg donor contributes only its rig and bite animation; the shipped silhouette is the project's own KMG_goblin-dog_Original mesh."),
                P("hyena", "Hyena", "Animal", 2, "Medium",
                    14, 15, 15, 2, 13, 6, 50, 2, "Bite1d6",
                    Array.Empty<string>(),
                    A("TripDefenseFourLegs", "TrippingBite",
                        "SkillFocusPerception")),
                P("boar", "Boar", "Animal", 2, "Medium",
                    17, 10, 17, 2, 13, 4, 40, 4, "Gore1d8",
                    Array.Empty<string>(),
                    A("ReducedReach", "Ferocity", "Toughness")),
                P("leopard", "Leopard", "Animal", 3, "Medium",
                    16, 19, 15, 2, 13, 6, 30, 1, "Bite1d6",
                    A("Claw1d3", "Claw1d3", "Claw1d3", "Claw1d3"),
                    A("Pounce", "TripDefenseFourLegs", "WeaponFinesse",
                        "SkillFocusStealth"),
                    "The two extra native claw limbs are the rake; the rake gate lets them strike only on a charge (Pounce) or against the exact foe the leopard has held since its round began, the attack sequencing seam drops them from any other full attack, and a rake claw never grabs (Sprint 7; correction order).",
                    "The leopard grabs with its bite only, against a foe of its size or smaller, on the shared summon grapple lifecycle (Sprint 7; correction order); the mound-specific native grab graph stays unused."),
                P("monitor-lizard", "Monitor Lizard", "Animal", 3, "Medium",
                    17, 15, 17, 2, 12, 6, 30, 3, "Bite1d8",
                    Array.Empty<string>(),
                    A("GreatFortitude", "SkillFocusPerception",
                        "TripDefenseFourLegs", "MonitorLizardPoison"),
                    "Kingmaker exposes one movement speed; the 30-foot ground speed is used and swim movement is omitted.",
                    "Bite grab rides the shared summon grapple lifecycle (Sprint 6); the mound-specific native grab graph stays unused."),
                P("cheetah", "Cheetah", "Animal", 3, "Medium",
                    17, 19, 15, 2, 12, 6, 50, 1, "Bite1d6",
                    A("Claw1d3", "Claw1d3"),
                    A("TripDefenseFourLegs", "TrippingBite",
                        "WeaponFinesse", "ImprovedInitiative"),
                    "Sprint is a bounded once-per-summoning swift burst on the special builder (Sprint 8): +30 feet for one round under the game's own speed cap, never repeatable within one summoning.",
                    "Cheetah visual: a procedural spotted coat on the leopard rig at a lean view scale (Sprint 8)."),
                PSK("dire-crocodile", "Dire Crocodile", "Animal", 12,
                    "Gargantuan", 37, 10, 25, 1, 14, 2, 20, 15, "Bite3d6",
                    Array.Empty<string>(), A("Tail4d8"),
                    A("TripDefenseFourLegs", "SkillFocusPerception",
                        "SkillFocusStealth", "ImprovedInitiative", "IronWill",
                        "ImprovedCriticalBite"), A("Perception", "Stealth"),
                    "Kingmaker exposes one movement speed; the 20-foot ground speed is used and the 30-foot swim is omitted. No underwater movement system is introduced, which the sprint's order forbids, and omitting the mode rather than the rate keeps the creature at its printed land speed.",
                    "The printed tail slap is a secondary natural attack - five lower than the bite and at half the Strength bonus - so it is declared in the secondary limb slot rather than among the additional primaries. A Gargantuan creature's printed Space 20 feet with Reach 15 feet is the standard footprint for its size, so unlike the Crocodile it must not carry the reduced-reach carrier.",
                    "The printed Run feat is omitted. Kingmaker has no running action distinct from ordinary movement and no jumping, so nothing in the rules layer could consult it; nothing is substituted for it and no record claims it works. Hold breath is omitted for the same kind of reason - the game models neither swimming nor drowning - and neither omission is covered by the passive-sense label, which is only for Scent, Darkvision and Low-light Vision.",
                    "The printed low-light vision is omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, accepted 2026-10-03, on the evidence recorded there.",
                    "Land-use skills allocate six Perception and six Stealth ranks, yielding +14 and +0 through native ability, class-skill, size and Skill Focus modifiers. No Mobility ranks, Swim or water-only Stealth bonus is added.",
                    "REGISTERED AND WITHHELD. Grab, live-bite Death Roll, creature-owned swallow and Sprint are implemented but NOT QUALIFIED. Sprint is a one-round +20-foot untyped land-speed modifier on a ten-round native buff cooldown, a bounded CRPG adaptation rather than a temporary base-speed rewrite. Original visuals and all live gates remain open."),
                PSK("crocodile", "Crocodile", "Animal", 3, "Large",
                    19, 12, 17, 1, 12, 2, 20, 4, "Bite1d8",
                    Array.Empty<string>(), A("Tail1d12"),
                    A("ReducedReach", "TripDefenseFourLegs",
                        "SkillFocusPerception", "SkillFocusStealth"),
                    A("Perception", "Stealth"),
                    "Kingmaker exposes one movement speed; the 20-foot ground speed is used and swim movement is omitted.",
                    "Land-use skills allocate one Perception and two Stealth ranks, yielding +8 and +5 through native ability, class-skill, size and Skill Focus modifiers. No Mobility ranks, Swim or water-only Stealth bonus is added.",
                    "Grab, live-bite Death Roll and Sprint are implemented but NOT QUALIFIED. Sprint is a one-round +20-foot untyped land-speed modifier on a ten-round native buff cooldown, a bounded CRPG adaptation rather than a temporary base-speed rewrite. Hold Breath is omitted because there is no swimming/drowning consumer; low-light vision is omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED."),
                P("dire-bat", "Dire Bat", "Animal", 4, "Large",
                    17, 15, 13, 2, 14, 6, 40, 3, "Bite1d8",
                    Array.Empty<string>(),
                    A("ReducedReach", "Airborne", "Stealthy", "DireBatBlindsense"),
                    "Kingmaker exposes one movement speed; 40-foot fly speed is used with airborne navigation and the 20-foot ground speed is omitted.",
                    "A dedicated imprecise 40-foot blindsense fact uses the native component without granting the native Blindsight feature's blindness immunity; Alertness remains omitted."),
                P("wolverine", "Wolverine", "Animal", 3, "Medium",
                    15, 15, 15, 2, 12, 10, 30, 2, "Claw1d6",
                    A("Claw1d6", "Bite1d4"),
                    A("TripDefenseFourLegs", "SkillFocusPerception",
                        "Toughness", "WolverineRage"),
                    "Burrow and climb movement are omitted because Kingmaker exposes one movement speed."),
                P("dire-boar", "Dire Boar", "Animal", 5, "Large",
                    23, 10, 17, 2, 13, 8, 40, 6, "Gore2d6",
                    Array.Empty<string>(), A("ReducedReach", "Ferocity",
                        "ImprovedInitiative", "SkillFocusPerception",
                        "Toughness")),
                P("dire-wolf", "Dire Wolf", "Animal", 5, "Large",
                    19, 15, 17, 2, 12, 10, 50, 3, "Bite1d8",
                    Array.Empty<string>(), A("ReducedReach",
                        "TripDefenseFourLegs", "TrippingBite",
                        "SkillFocusPerception", "WeaponFocusBite"),
                    "The Run feat is omitted because no exact final-live feature identity was proven."),
                P("grizzly-bear", "Grizzly Bear", "Animal", 5, "Large",
                    21, 13, 19, 2, 12, 6, 40, 6, "Bite1d6",
                    A("Claw1d6", "Claw1d6"), A("ReducedReach"),
                    "Claw grab rides the shared summon grapple lifecycle (Sprint 6); the mound-specific native grab graph stays unused.",
                    "Endurance, Run, and Skill Focus (Survival) are omitted because exact final-live feature identities were not proven."),
                P("tiger", "Tiger", "Animal", 6, "Large",
                    23, 15, 17, 2, 12, 6, 40, 3, "BiteLarge2d6",
                    A("Claw1d8", "Claw1d8", "Claw1d8", "Claw1d8"),
                    A("Pounce", "TripDefenseFourLegs", "ImprovedInitiative",
                        "SkillFocusPerception", "WeaponFocusClaw"),
                    "The two extra claw limbs are the rake; the rake gate lets them strike only on a charge (Pounce) or against the exact foe the tiger has held since its round began, the attack sequencing seam drops them from any other full attack, and a rake claw never grabs (Sprint 8; correction order).",
                    "The tiger grabs with its bite and both foreclaws, against a foe of its size or smaller, on the shared summon grapple lifecycle (Sprint 8; correction order); Run and Skill Focus (Stealth) are omitted (no summon-safe contracts proven).",
                    "Tiger visual: the leopard rig at a 1.25 view scale with a procedural striped coat generated in the rig's own texture space (Sprint 8); no native tiger exists."),
                P("lion", "Lion", "Animal", 5, "Large",
                    21, 17, 15, 2, 12, 6, 40, 3, "Bite1d8",
                    A("Claw1d4", "Claw1d4", "Claw1d4", "Claw1d4"),
                    A("ReducedReach", "Pounce", "TripDefenseFourLegs",
                        "ImprovedInitiative", "SkillFocusPerception"),
                    "The two extra native claw limbs are the rake; the rake gate lets them strike only on a charge (Pounce) or against the exact foe the lion has held since its round began, the attack sequencing seam drops them from any other full attack, and a rake claw never grabs (Sprint 7; correction order).",
                    "The lion grabs with its bite only, against a foe of its size or smaller, on the shared summon grapple lifecycle (Sprint 7; correction order); Run is omitted because no safe exact final-live contract was proven.",
                    "Lion visual: a tawny tint on the leopard rig through the shared visual variant (Sprint 7); no mane geometry."),
                P("pteranodon", "Pteranodon", "Animal", 5, "Large",
                    16, 19, 15, 2, 15, 12, 50, 2, "Bite2d6",
                    Array.Empty<string>(), A("Airborne", "Dodge",
                        "ImprovedInitiative", "SkillFocusPerception"),
                    "Kingmaker exposes one movement speed; 50-foot fly speed is used with airborne navigation and the 10-foot ground speed is omitted."),
                PS("dire-lion", "Dire Lion", "Animal", 8, "Large",
                    25, 15, 17, 2, 12, 10, 40, 4, "Bite1d8",
                    A("Claw1d6", "Claw1d6"), A("Claw1d6", "Claw1d6"),
                    A("ReducedReach", "Pounce", "TripDefenseFourLegs",
                        "ImprovedInitiative", "SkillFocusPerception",
                        "WeaponFocusClaw"),
                    "The secondary claw pair is the rake; the rake gate lets it strike only on a charge (Pounce) or against the exact foe the dire lion has held since its round began, the attack sequencing seam drops it from any other full attack, and a rake claw never grabs (Sprint 7; correction order).",
                    "The dire lion grabs with its bite only, against a foe of its size or smaller, on the shared summon grapple lifecycle (Sprint 7; correction order); Run is omitted because no summon-safe exact final-live contract was proven."),
                PK("ankylosaurus", "Ankylosaurus", "Animal", 10, "Huge",
                    27, 10, 17, 2, 13, 8, 30, 14, "Tail3d6",
                    Array.Empty<string>(), A("GreatFortitude", "PowerAttack", "ImprovedBullRush",
                        "WeaponFocusTail", "TripDefenseFourLegs", "AnkylosaurusChassis", "AnkylosaurusStun"),
                    A("Perception"),
                    "Stun (Ex) uses one Fortitude save, with DC based on current Strength and racial HD; an ordinary failed save dazes for one round, and a confirmed critical failed save stuns for 1d4 rounds instead. Fortitude is this mission's explicit save-type interpretation.",
                    "Improved Overrun remains omitted: no concrete native feat was found in the standalone 2.1.7b library and no summon command consumer was qualified. The native Overrun maneuver enum exists; no general overrun system is substituted.",
                    "Low-light vision and scent retain OwnerAcceptedEngineLimitation PASSIVE_CREATURE_SENSES_UNMODELED; no substitute senses are granted."),
                PK("triceratops", "Triceratops", "Animal", 14, "Huge",
                    26, 9, 19, 2, 12, 7, 30, 14,
                    "TriceratopsGore2d10", Array.Empty<string>(),
                    A("GreatFortitude", "ImprovedBullRush",
                        "ImprovedCriticalGore", "PowerAttack",
                        "SkillFocusPerception", "WeaponFocusGore",
                        "TripDefenseFourLegs", "TriceratopsChassis"),
                    A("Perception"),
                    "Sprint 23 frozen adult profile: one primary gore at 2d10+12; powerful charge uses the eligible first native charge strike at 4d10+16; Trample is a separate full-round path action at 1d8+12 and live DC 25.",
                    "The printed Improved Critical feat omits its weapon while the compact gore line omits a threat range. The feat is applied to the sole printed attack, gore, through the native exact feat; no multiplier or damage is invented.",
                    "Run remains omitted because the installed 2.1.7b library and released summon carriers expose no bounded concrete feat/consumer that can be qualified without inventing movement behavior.",
                    "Low-light vision and scent retain OwnerAcceptedEngineLimitation PASSIVE_CREATURE_SENSES_UNMODELED; no substitute senses are granted."),
                PK("stegosaurus", "Stegosaurus", "Animal", 12, "Huge",
                    27, 14, 17, 2, 13, 10, 30, 12,
                    "StegosaurusTail4d6", Array.Empty<string>(),
                    A("Cleave", "GreatFortitude", "ImprovedInitiative",
                        "PowerAttack", "WeaponFocusTail",
                        "TripDefenseFourLegs", "StegosaurusChassis",
                        "StegosaurusTrip"), A("Perception"),
                    "Sprint 23 frozen adult profile: one primary spiked-tail attack at 4d6+12. Every exact resolved hit may make at most one native Trip combat maneuver against that target; miss, immunity, illegal size, an already-prone target and duplicate callbacks make none.",
                    "Improved Overrun retains Sprint 22's verified engine limitation: the native feat identity carries prerequisites but no mechanical consumer, and no general overrun system is substituted. Cleave is the native feat/action, never a passive tail burst.",
                    "No Ankylosaurus daze, stun, critical rider, trample, injury poison or automatic knockdown is present.",
                    "Low-light vision and scent retain OwnerAcceptedEngineLimitation PASSIVE_CREATURE_SENSES_UNMODELED; no substitute senses are granted."),
                P("dire-bear", "Dire Bear", "Animal", 10, "Large",
                    25, 13, 21, 2, 12, 10, 40, 8, "Bite1d8",
                    A("Claw1d6", "Claw1d6"), A("ReducedReach",
                        "ImprovedInitiative", "IronWill",
                        "SkillFocusPerception"),
                    "Claw grab rides the shared summon grapple lifecycle (Sprint 6); the mound-specific native grab graph stays unused.",
                    "Endurance and Run are omitted because exact concrete final-live feature identities were not proven."),
                PS("dire-tiger", "Smilodon", "Animal", 14,
                    "Large", 27, 15, 17, 2, 12, 10, 40, 6,
                    "BiteLarge2d6", A("Claw2d4", "Claw2d4"),
                    A("Claw2d4", "Claw2d4"), A("ReducedReach", "Pounce",
                        "TripDefenseFourLegs", "ImprovedCriticalBite",
                        "ImprovedCriticalClaw", "ImprovedInitiative",
                        "SkillFocusPerception", "SkillFocusStealth",
                        "WeaponFocusBite", "WeaponFocusClaw"),
                    "The secondary claw pair is the rake; the rake gate lets it strike only on a charge (Pounce) or against the exact foe the smilodon has held since its round began, the attack sequencing seam drops it from any other full attack, and a rake claw never grabs (Sprint 7; correction order).",
                    "The smilodon grabs with its bite and both foreclaws, against a foe of its size or smaller, on the shared summon grapple lifecycle (Sprint 7; correction order); Run is omitted because no summon-safe exact final-live contract was proven."),
                // Sprint 25 completes this released creature rather than
                // re-registering it: the key, tiers, roots, children and
                // ordering are untouched and only the profile is corrected.
                PS("elephant", "Elephant", "Animal", 11, "Huge",
                    30, 10, 19, 2, 13, 7, 40, 9, "Gore2d8",
                    A("Slam2d6"), Array.Empty<string>(),
                    A("Endurance", "GreatFortitude", "ImprovedBullRush",
                        "IronWill", "PowerAttack", "SkillFocusPerception",
                        "TripDefenseFourLegs", "ElephantChassis"),
                    "Sprint 25 correction: the printed adult routine is gore +16 (2d8+10) AND slam +16 (2d6+10). Both are primary, so the slam moves out of the secondary list it was registered in. As a secondary it took the native -5 to hit and half Strength, which produced slam +11 (2d6+5) and did not match either printed line.",
                    "Improved Bull Rush is now carried. It was recorded as unproven here, but Sprint 23 qualified the exact concrete carrier for Triceratops, so the earlier omission no longer holds.",
                    "The printed +4 quadruped defence against trip is now carried through the same shared four-legs fact the other quadrupeds use, which is what the printed CMD 30 / 34 versus trip requires.",
                    "Space is 15 feet but printed reach is 10 feet. A Huge body is not evidence of 15-foot reach and this creature must not be used to generalize one.",
                    "Endurance is now carried too, correcting a Sprint 25 error. It was recorded here as having no proven carrier, but the builder has mapped it to native 54ee847996c25cd4ba8773d7b8555174 since Sprint 4, the Sprint 24 bounded native census recorded it as resolved, and the released Tyrannosaurus already ships it. The original omission predates that evidence and no longer holds. All six printed Elephant feats are now carried.",
                    "Trample is specified at 2d8+15 with a live DC 25, separate from the gore and slam, granting no powerful charge and changing no released d6 trampler. The original omission reason is obsolete - Sprint 23 built the player-commandable, path-safe trample contract this needs - and the command IS registered now, on that same qualified carrier, so it inherits the interrupt, duplicate-response, path and leave-combat guards rather than a second copy of them. The identity it needed was appended only after a bounded observation of this released creature recorded six grants and no trample ability or component of any kind. Registered is not qualified: native proof of the command in play is still required before publication."),
                // Sprint 25. This row and the carriers that resolve its
                // weapon key land in one change: a profile naming an
                // unresolvable weapon makes blueprint initialization throw
                // and roll back every registration.
                PK("brachiosaurus", "Brachiosaurus", "Animal", 18,
                    "Gargantuan",
                    37, 10, 21, 2, 13, 10, 30, 12,
                    "BrachiosaurusTail4d6", Array.Empty<string>(),
                    A("BrachiosaurusChassis", "Cleave", "GreatCleave", "GreatFortitude",
                        "GreaterOverrun", "ImprovedBullRush", "IronWill",
                        "PowerAttack", "SkillFocusPerception",
                        "TripDefenseFourLegs", "WeaponFocusTail"),
                    A("Perception"),
                    "The printed tail bonus differs between sources: the current AoN entry reads +22 and the legacy PRD reads +23. The selected contract is +23, because it is what the inputs both sources print derive - base attack +13, Strength 37 for +13, Gargantuan -4 and Weapon Focus (tail) +1 - and those same inputs independently derive the CMB +30 and CMD 40 that both also print. A +22 would require an unexplained -1 that no printed input supplies. The sources are not claimed to agree and no hidden modifier is manufactured.",
                    "A single natural attack, so the tail takes one-and-a-half Strength rather than the ordinary multiplier. This is the only creature in the roster that uses that routine.",
                    "Space is 20 feet and printed reach is 20 feet. This is the creature whose reach does equal its space, which is exactly why the released Elephant must not be read as evidence that a large body grants reach equal to space - there it is 15 and 10.",
                    "Twenty-foot reach is composed, not stated: a Gargantuan body exposes fifteen feet, the chassis adds the final five, and the private tail weapon type stores nine to expose five feet before body reach, the same arithmetic the released Stegosaurus tail uses. Native proof must confirm the uniform twenty feet before publication rather than trusting the composition.",
                    "Greater Overrun uses this creature's private carrier and ordinary one-target Overrun command, separate from auto-success Trample. The installed-stack feat ee24c02cc68c4c9b9b54ebef9f9f8133 is absent standalone: when present its kept components are deep-cloned without success-only ManeuverProvokeAttack; when absent the carrier uses native ManeuverBonus for the same printed +2. No Improved Overrun feat is added. The exact-owner rider reads native IsPartialSuccess and knockdown eligibility, and provokes only for a knockdown this overrun caused. Other actors' feats and Trample remain unchanged. Great Cleave still uses the recorded native CleaveAction wiring rather than a global repair. Functional native qualification of these commands remains required; registration is not proof.",
                    "Trample is registered as a player-commandable full-round action on the Sprint 23 qualified path/contact/response carrier, so it inherits that carrier’s interrupt, duplicate-response, path and leave-combat guards rather than a second copy of them. 2d6+19 with a live DC 32, both derived rather than stated: one and a half times the Strength modifier for +19, and 10 + 18 hit dice halved + Strength 37 for DC 32. Separate from the tail, grants no powerful charge, and changes no released trampler. Registered is not qualified: native proof of the command in play is still required before publication.",
                    "The donor rig is measured, not assumed. The 2026-10-16 census recorded the frozen quadruped donor as 40 real skinning joints with a twelve-joint tail chain, which the tail attack wants, but exactly one neck joint and one head joint. A long raised sauropod neck on this rig is therefore a two-segment rigid structure. That is a measured limitation of the frozen donor and is recorded before any geometry is authored against it."),
                PS("mastodon", "Mastodon", "Animal", 14, "Huge",
                    34, 12, 21, 2, 13, 7, 40, 12, "Gore2d8",
                    Array.Empty<string>(), A("Slam2d6"),
                    A("IronWill", "PowerAttack", "SkillFocusPerception"),
                    "Trample is omitted pending a player-commandable, path-safe native movement contract; ordinary gore and slam attacks remain exact and the omission is conservative.",
                    "Endurance, Improved Bull Rush, Improved Iron Will, and Weapon Focus (gore) are omitted because exact concrete final-live feature identities were not proven."),
                P("roc", "Roc", "Animal", 16, "Gargantuan",
                    28, 15, 17, 2, 12, 11, 80, 14, "Bite2d8",
                    A("Talon2d6", "Talon2d6"), A("Airborne",
                        "ImprovedCriticalClaw", "ImprovedInitiative",
                        "IronWill", "LightningReflexes", "PowerAttack",
                        "SkillFocusPerception", "WeaponFocusClaw"),
                    "Kingmaker exposes one movement speed; 80-foot fly speed is used with airborne navigation and the 20-foot ground speed is omitted.",
                    "Talon grab and Flyby Attack are omitted because no summon-safe exact final-live contracts were proven."),
                // Sprint 3 (Phase 1): native publication pack I.
                P("pony", "Pony", "Animal", 2, "Medium",
                    13, 13, 14, 2, 11, 4, 40, 0, "Hoof1d3",
                    A("Hoof1d3"), A("TripDefenseFourLegs"),
                    "Both hooves are secondary attacks (Docile; a summon is never combat-trained): the game's own secondary natural-attack rule, -5 to hit and half the Strength modifier to damage, through the ForceSecondary flag set by the Docile carrier on both hoof entities.",
                    "Endurance and Run are omitted because exact final-live feature identities were not proven."),
                P("horse", "Horse", "Animal", 2, "Large",
                    16, 14, 17, 2, 13, 7, 50, 0, "Hoof1d4",
                    A("Hoof1d4"), A("ReducedReach", "TripDefenseFourLegs"),
                    "Both hooves are secondary attacks (Docile; a summon is never combat-trained): the game's own secondary natural-attack rule, -5 to hit and half the Strength modifier to damage, through the ForceSecondary flag set by the Docile carrier on both hoof entities.",
                    "Endurance and Run are omitted because exact final-live feature identities were not proven."),
                P("owlbear", "Owlbear", "MagicalBeast", 5, "Large",
                    19, 12, 18, 2, 12, 10, 30, 5, "Bite1d6",
                    A("Claw1d6", "Claw1d6"),
                    A("ReducedReach", "ImprovedInitiative", "GreatFortitude",
                        "SkillFocusPerception"),
                    "Claw grab rides the shared summon grapple lifecycle (Sprint 4): a claw hit attempts the game's own grapple check, success starts the native hold, each new round the owlbear maintains with a grapple check that deals claw damage or releases, and the hold ends with the target's escape or the summon's end."),
                P("cyclops", "Cyclops", "Humanoid", 10, "Large",
                    21, 8, 15, 10, 13, 8, 30, 7, "Greataxe",
                    Array.Empty<string>(),
                    A("Ferocity", "PowerAttack", "Cleave"),
                    "Flash of Insight is bounded to one use per summoning and to the next attack roll: a swift action arms the cyclops until its next attack roll, whose own d20 result is chosen as a natural 20 (the game's pre-rolled-result seam), so the hit and the threat follow from the roll and the critical confirmation is rolled normally; the tabletop choice of any one die roll is narrowed to the attack.",
                    "The +4 hide armor is carried as an exact armor-descriptor fact (no item, loot or inventory) and the natural armor is the stat block's +7, so the armor class is the tabletop 19 (10 + 4 armor - 1 Dexterity + 7 natural - 1 size); the heavy crossbow is omitted because the summon carries no equipment; Alertness, Great Cleave and Improved Bull Rush are omitted because exact final-live feature identities were not proven."),
                // Sprint 4 (Phase 1): native publication pack II - plants and
                // the colossal worm, on the shared summon grapple lifecycle.
                P("shambling-mound", "Shambling Mound", "Plant", 9, "Large",
                    21, 10, 17, 7, 10, 9, 20, 10, "SlamPlant2d6",
                    A("SlamPlant2d6"),
                    A("FireResistance10", "ElectricityImmunity", "PowerAttack",
                        "IronWill", "LightningReflexes", "Cleave", "WeaponFocusSlam"),
                    "Slam grab and constrict ride the shared summon grapple lifecycle: a slam hit attempts the game's own grapple check, success starts the native hold and deals constrict damage, and each maintained round deals slam and constrict damage or releases.",
                    "Electric Fortitude keeps its electricity immunity; the temporary Constitution gain has no bounded native representation and is omitted. Swim movement is omitted; the native unit's poison aura is not tabletop and is not carried.",
                    "Both slams are carried as primary limbs, as the native unit carries them."),
                P("giant-flytrap", "Giant Flytrap", "Plant", 13, "Huge",
                    25, 18, 25, 1, 12, 6, 10, 10, "BiteLarge1d8",
                    A("BiteLarge1d8", "BiteLarge1d8", "BiteLarge1d8"),
                    A("AcidResistance20", "Blindsight", "TripImmune", "Cleave",
                        "GreatFortitude", "ImprovedInitiative", "PowerAttack",
                        "SkillFocusStealth", "WeaponFocusBite"),
                    "Bite grab rides the shared summon grapple lifecycle with one link per bite, four at most (corrected 2026-09-25): every link is a held state on the target that names the flytrap and the bite that established it, and a bite that already holds cannot take a second foe.",
                    "Engulf is the swallow-whole sequence against a Medium or smaller foe the flytrap has held since the round began (corrected 2026-09-25), dealing the stat block's 1d8+7 bludgeoning and 2d6 acid each round inside (the acid corrected 2026-09-26); a mouth that holds or has engulfed a foe attacks no other target, and an active hold is session-scoped: a save and a reload release it cleanly (owner-accepted engine limitation, 2026-09-26); the project state releases on every end path. Tremorsense 60 feet is represented by the native 60-foot blindsight. Vital Strike is omitted because no exact final-live feature identity was proven.",
                    "Kingmaker cannot represent an absent Intelligence score, so Intelligence 1 is used."),
                P("purple-worm", "Purple Worm", "MagicalBeast", 16, "Gargantuan",
                    35, 6, 25, 1, 8, 8, 20, 22, "PurpleWormBite",
                    A("PurpleWormSting"),
                    A("TripImmune", "PurpleWormPoison", "CriticalFocus",
                        "ImprovedCriticalBite", "PowerAttack", "WeaponFocusBite"),
                    "Grab and swallow whole follow the tabletop sequence (corrected 2026-09-25): a bite hit attempts the game's grapple check and success holds the target, and on a later turn a successful maintain check - used as though attempting to pin - swallows a foe up to one size smaller through the native swallow-whole part, which handles break-free attempts, the per-round crushing damage and the spit-out on the worm's death or end; a foe of the worm's own size is held but never swallowed.",
                    "Burrow and swim movement are omitted; the native summoned worm's burrowing kit is not carried, as the charter's bounded combat adaptation directs. The sting poison is the exact native Constitution-scaled graph.",
                    "Awesome Blow, Improved Bull Rush, Staggering Critical and Weapon Focus (sting) are omitted because exact final-live feature identities were not proven; Kingmaker cannot represent an absent Intelligence score, so Intelligence 1 is used."),
                P("giant-wasp", "Giant Wasp", "Vermin", 4, "Large",
                    18, 12, 18, 1, 13, 11, 60, 4, "WaspSting1d8",
                    Array.Empty<string>(), A("Airborne", "WaspPoison"),
                    "The 60-foot fly speed uses airborne navigation; 20-foot ground speed is omitted because Kingmaker exposes one movement speed. An absent Intelligence score is represented as 1. Its dedicated Constitution-scaled poison graph and original visual passed guarded live qualification."),
                P("stirge", "Stirge", "MagicalBeast", 1, "Tiny",
                    3, 19, 10, 1, 12, 6, 40, 0, "StirgeTouch",
                    Array.Empty<string>(), A("Airborne", "WeaponFinesse"),
                    "The 40-foot fly speed uses airborne navigation; 10-foot ground speed is omitted because Kingmaker exposes one movement speed. Its native melee-touch carrier has zero base dice; the touch hit, attachment lifecycle, blood drain and visual contact passed guarded runtime checks."),
                P("aurochs", "Aurochs", "Animal", 3, "Large",
                    23, 10, 17, 2, 11, 4, 40, 4, "Gore1d8",
                    Array.Empty<string>(), A("ReducedReach", "TripDefenseFourLegs",
                        "SkillFocusPerception"),
                    "Trample uses the disclosed Kingmaker automatic-AoO-or-Reflex adaptation. Stampede requires three adjacent allied Stampede owners actively executing their own Trample in the same round, then permits same-size targets and adds +2 save DC only while that command formation remains valid. Endurance is omitted because no exact summon-safe feat identity was proven. The sanitized Horse rig drives the original Aurochs mesh and painting."),
                P("bison", "Bison", "Animal", 5, "Large",
                    27, 10, 19, 2, 11, 4, 40, 8, "Gore2d6",
                    Array.Empty<string>(), A("ReducedReach", "TripDefenseFourLegs",
                        "PowerAttack"),
                    "Trample uses the disclosed Kingmaker automatic-AoO-or-Reflex adaptation. Stampede requires three adjacent allied Stampede owners actively executing their own Trample in the same round, then permits same-size targets and adds +2 save DC only while that command formation remains valid. Endurance and Improved Bull Rush are omitted because exact summon-safe feat identities were not proven. The sanitized Horse rig drives the original Bison mesh and painting."),
                P("rhinoceros", "Rhinoceros", "Animal", 5, "Large",
                    22, 10, 19, 2, 13, 5, 40, 7, "Gore2d6",
                    Array.Empty<string>(), A("ReducedReach", "TripDefenseFourLegs",
                        "GreatFortitude", "SkillFocusPerception"),
                    "The summon-local powerful charge applies only to its gore on a native charge and uses the printed 4d6+12 result. Endurance is omitted because no exact summon-safe feat identity was proven. The sanitized Mastodon rig drives the original Rhinoceros mesh and painting."),
                P("woolly-rhinoceros", "Woolly Rhinoceros", "Animal", 8,
                    "Large", 28, 10, 21, 2, 13, 3, 30, 10, "Gore2d8",
                    Array.Empty<string>(), A("ReducedReach", "TripDefenseFourLegs",
                        "GreatFortitude", "SkillFocusPerception"),
                    "The summon-local powerful charge applies only to its gore on a native charge and uses the printed 4d8+18 result. Trample uses the disclosed Kingmaker automatic-AoO-or-Reflex adaptation. Diehard and Endurance are omitted because exact summon-safe feat identities were not proven. The sanitized Mastodon rig drives the original Woolly Rhinoceros mesh and painting."),
                // Sprint 18. Both apes print Space 10 feet with Reach 10 feet,
                // which is the ordinary Large footprint, so neither carries the
                // reduced-reach fact the Large ungulates and crocodilians need.
                // Neither prints a bonus against trip, so neither carries a
                // trip-defence fact: the printed CMD 17 and 20 are exactly
                // 10 plus base attack, Strength, Dexterity and the Large size
                // bonus.
                PK("ape", "Ape", "Animal", 3, "Large",
                    15, 15, 14, 2, 12, 7, 30, 3, "Slam1d6",
                    A("Slam1d6"),
                    A("GreatFortitude", "SkillFocusPerception",
                        "PrimateFullStrengthLimbs"),
                    A("Mobility", "Perception"),
                    "Sprint 18 frozen contract: exactly two primary slams at the printed bonus, each adding the whole Strength modifier. No bite, no claw, no rend, and no chest-thump fear or roar ability, none of which the stat block prints.",
                    "Exact land ranks retain native attribute, class-skill, size and Skill Focus contributions: one Mobility rank and one Perception rank. The third printed rank bought Climb and is not reallocated.",
                    "Kingmaker exposes one movement speed; the 30-foot ground speed is used and the 30-foot climb speed is omitted under the ordinary-map land-use scope. The printed Climb +14, and the +8 racial climb bonus inside it, have no faithful ordinary-map consumer and are omitted rather than substituted: Mobility is not raised to stand in for Climb and Athletics is not raised to simulate the racial bonus.",
                    "Low-light vision and scent are omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED. Nothing is substituted for them - no blindsense, no vision-range override - and no record claims they work.",
                    "REGISTERED AND WITHHELD. The original project-owned gorilla body is authored and wired on the census-chosen Troll rig; the knuckle-walking gait is the donor's and is not authored. Runtime qualification remains open."),
                // Sprint 19. Seven magical-beast hit dice, one primary
                // 1d6 bite and four primary 1d4 claws, all five at +10 and at
                // the whole Strength modifier, and a rend that needs all four
                // claws on one creature. Seven skill ranks, spent exactly
                // where the stat block spends them: four on Perception and
                // three on Stealth, none on Climb. Climb costs it no rank, so
                // unlike the Sprint 18 apes the omitted climb speed forfeits
                // nothing.
                // Sprint 21. Two claws and nothing else - the shortest
                // printed routine this programme has shipped for a creature
                // that carries a rider, and both claws carry it. No ranks: no
                // Intelligence score. No weapon of its own: it is Medium, so
                // the engine does not scale the shared native 1d4 claw for it,
                // which is the whole reason the Large Sprint 18 and 20
                // creatures had to own theirs.
                PK("giant-crab", "Giant Crab", "Vermin", 3, "Medium",
                    15, 13, 14, 1, 10, 2, 30, 5, "Claw1d4",
                    A("Claw1d4"),
                    A("GiantCrabTripDefense", "GiantCrabRacialSkills",
                      "GiantCrabMindlessImmunity",
                      // Not a printed feat: the released carrier that keeps
                      // both claws on the plain Strength modifier the stat
                      // block prints. The symbol is Sprint 18's, reused rather
                      // than renamed, because it shipped in v0.0.147.
                      "PrimateFullStrengthLimbs"),
                    Array.Empty<string>(),
                    "Sprint 21 frozen contract: Summon Nature's Ally III only, the first creature in the roster shaped that way, hidden pending complete runtime qualification. Two claws +4 (1d4+2 plus grab), both primary at the whole Strength modifier.",
                    "The printed swim 20 ft., the aquatic subtype and the printed water dependency are omitted under OwnerAcceptedEngineLimitation: ORDINARY_MAP_LAND_USE_SCOPE. Kingmaker has no Swim skill and models neither swimming nor drowning, and this mission's hard boundaries forbid building an aquatic subsystem. Nothing is substituted: the 20-foot swim speed is not folded into the 30-foot ground speed, Athletics and Mobility are not raised to stand in for Swim, and no breathing or dehydration state is invented. Water dependency is recorded honestly as unrepresented rather than approximated. The creature forfeits no rank, because a creature with no Intelligence score has none to spend.",
                    "The printed darkvision 60 feet is omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, on the same evidence and with the same prohibitions as every earlier vermin.",
                    "Grab is on both claws, which is every limb it has, on the grapple lifecycle Sprint 6 built and Sprint 19 taught to hold several limbs; it holds one foe in each claw. The printed +12 anti-trip defence is its own carrier rather than the shared native eight-leg fact, which Sprint 20 measured delivering +8 - the convention is four per pair of legs beyond the first, so a six-legged insect prints +8 and this eight-legged creature prints +12 - and rather than the Sprint 20 carrier, which is named and described for a scorpion. The printed racial +4 Perception is its own carrier for the same reason.",
                    "An absent Intelligence score is represented as 1, and the printed immunity to mind-affecting effects is carried as its own fact rather than inferred from it, because 1 is not mindless to the engine."),
                PK("giant-scorpion", "Giant Scorpion", "Vermin", 5, "Large",
                    19, 12, 16, 1, 10, 2, 50, 6, "GiantScorpionClaw1d6",
                    A("GiantScorpionClaw1d6", "GiantScorpionSting1d6"),
                    // Its own anti-trip carrier rather than the shared
                    // native one: measured, that one is worth +8, which is
                    // what a six-legged insect prints. Its own racial +4 as
                    // well, on the two printed skills Kingmaker has - the
                    // Giant Ants' carrier is theirs and covers Perception
                    // alone.
                    A("GiantScorpionTripDefense", "GiantScorpionRacialSkills",
                      "GiantScorpionPoison",
                      "GiantScorpionMindlessImmunity",
                      // Not a printed feat: the carrier that keeps all three
                      // limbs on the plain Strength modifier the stat block
                      // prints. The symbol is Sprint 18's, reused rather than
                      // renamed, because it shipped in v0.0.147.
                      "PrimateFullStrengthLimbs"),
                    // No ranks at all. A creature with no Intelligence score
                    // buys none, so the builder must be told so explicitly or
                    // its default Perception, Mobility and Stealth ranks make
                    // the creature read better than its stat block. Nothing is
                    // forfeited: all three printed totals are ability plus
                    // racial bonus.
                    Array.Empty<string>(),
                    "Sprint 20 frozen contract: Summon Monster IV and Summon Nature's Ally IV, templated on the Monster table like every other vermin there, hidden pending complete runtime qualification. Two claws +6 (1d6+4 plus grab) and a sting +6 (1d6+4 plus poison), all three primary at the whole Strength modifier.",
                    "Grab is on the two claws and never on the sting; it rides the grapple lifecycle Sprint 6 built and Sprint 19 taught to hold several limbs, gated on the claw weapon type. The poison is on the sting and never on a claw, gated on the sting's own weapon type exactly as the Giant Ant's sting poison is. The printed Fortitude DC 15 is what the Constitution-scaled formula produces unaided at 5 hit dice and Constitution 16, and is not hard-coded.",
                    "The poison graph needs its own carrier rather than a shared one: the printed effect is 1d2 Strength once per round for six rounds cured by one save, and every poison this project ships runs four rounds. Nothing about the shared carriers is changed to accommodate it.",
                    "Both weapons are project-owned at the printed dice with overridden damage dice, because the engine scales a shared native 1d6 one step up for a Large wielder - the exact defect the guarded Sprint 18 review measured on the Dire Ape - and no native blueprint carries a scorpion sting at all.",
                    "The printed +12 manoeuvre defence against trip uses the project's existing eight-legged trip defence, the carrier the Giant Centipede and both Giant Ant castes already hold. A scorpion has eight legs and the printed difference between CMD 19 and CMD 31 against trip is exactly that bonus.",
                    "Kingmaker cannot represent an absent Intelligence score, so Intelligence 1 is used, as on the Giant Spider, both Giant Ant castes and the Giant Stag Beetle. The printed immunity to mind-affecting effects is carried as its own fact on the game's own descriptor immunity - the component the Goblin Dog uses for disease - and is never inferred from that substituted score.",
                    "Kingmaker exposes one movement speed; the 50-foot ground speed is used. The printed Climb +8 is omitted under ORDINARY_MAP_LAND_USE_SCOPE and neither Athletics nor Mobility is raised to stand in for it. It costs no rank, because this creature has none, so nothing is forfeited.",
                    "Printed Space 10 feet with Reach 10 feet, and a parenthetical 5 feet for the claws. The creature keeps the 10, which is also the engine's own reach for a Large creature, so the claws threaten at 10 feet rather than 5 under PER_LIMB_REACH_UNREPRESENTED. The project's reduced-reach carrier is deliberately NOT applied: the Giant Stag Beetle uses it because that creature's whole printed Reach line is 5 feet, where this one's is 10.",
                    "Darkvision 60 feet and tremorsense 30 feet are omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED, accepted 2026-10-03. Nothing is substituted. The Sprint 6 Giant Spider stands native blindsight in for its tremorsense; that shipped decision is left exactly as it is and deliberately not extended here.",
                    "Vermin have no feats, so no feat is dropped and none is invented. Every printed number derives from hit dice, ability scores, size and racial bonuses alone.",
                    "REGISTERED AND WITHHELD. The original project-owned scorpion body is NOT authored: this creature currently rides the Giant Spider donor rig pending the bounded read-only arachnid donor census, and every visual and runtime gate remains open."),
                PK("girallon", "Girallon", "MagicalBeast", 7, "Large",
                    19, 17, 18, 2, 12, 7, 40, 6, "GirallonBite1d6",
                    A("GirallonClaw1d4", "GirallonClaw1d4",
                      "GirallonClaw1d4", "GirallonClaw1d4"),
                    A("ImprovedInitiative", "IronWill", "SkillFocusPerception",
                      "Toughness", "GirallonRend",
                      // Not a printed feat: the carrier that keeps all five
                      // limbs on the plain Strength modifier the stat block
                      // prints. The symbol is Sprint 18's and is reused rather
                      // than renamed, because it shipped in v0.0.147 and this
                      // ledger allocates an identity once.
                      "PrimateFullStrengthLimbs"),
                    A("Perception", "Stealth"),
                    "Sprint 19 frozen contract: Summon Nature's Ally V only, untemplated, hidden pending complete runtime qualification. Bite +10 (1d6+4) and four claws +10 (1d4+4), every attack primary at the whole Strength modifier. Rend 1d4+6 only when all four claws hit one creature in one attack sequence; the damage is the engine's own derivation and follows live Strength.",
                    "Exact land ranks and racial bonuses retain native attribute, feat and class-skill contributions. The 40-foot climb speed is omitted under ORDINARY_MAP_LAND_USE_SCOPE; the printed Climb +12 is Strength plus the racial climb-speed bonus and costs no rank, so nothing is forfeited and nothing is reallocated. Darkvision, low-light vision and scent remain PASSIVE_CREATURE_SENSES_UNMODELED without substitute senses. The body rides the reused Sprint 18 census rig, so its four arms share two animation driver chains under FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS."),
                PK("dire-ape", "Dire Ape", "Animal", 4, "Large",
                    19, 15, 16, 2, 12, 7, 30, 4, "DireApeBite1d6",
                    A("DireApeClaw1d4", "DireApeClaw1d4"),
                    A("IronWill", "SkillFocusPerception", "DireApeRend",
                        "PrimateFullStrengthLimbs"),
                    A("Mobility", "Perception", "Stealth"),
                    "Sprint 18 frozen contract: one primary bite and two primary claws, all three at the printed bonus and each adding the whole Strength modifier. Rend is the only special attack.",
                    "Rend resolves once after both claws hit the exact same target in one attack sequence, and never from one claw, from claws on different targets, from a bite and a claw, across turns or commands, or from a replayed rule event. The engine's own rend carrier deals the damage from 1d4 plus one and a half times the live Strength modifier, which is the printed 1d4+6 at Strength 19; a bounded Dire-Ape-owned gate decides only whether an attack is that rend, because the engine's command-level gate identifies a rend by hand slot and cannot express three equal primary limbs. The native rend attack animation variant is selected by that command-level gate and is therefore not played; the omission is presentation only and nothing is invented to replace it.",
                    "No grab, pin or armor-targeting behaviour is implemented. The flavour text's remark that Dire Apes may grapple difficult foes is not a printed special attack.",
                    "Exact land ranks retain native attribute, class-skill, size and Skill Focus contributions: one rank each in Mobility, Perception and Stealth. The fourth printed rank bought Climb and is not reallocated.",
                    "Kingmaker exposes one movement speed; the 30-foot ground speed is used and the 30-foot climb speed is omitted under the ordinary-map land-use scope. The printed Climb +16 is omitted rather than substituted.",
                    "Low-light vision and scent are omitted under OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED.",
                    "REGISTERED AND WITHHELD. The original project-owned gigantopithecus body is NOT authored: this creature currently rides the Owlbear donor rig pending the bounded read-only primate donor census, and every visual and runtime gate remains open."),
                PSK("deinonychus", "Deinonychus", "Animal", 4, "Medium",
                    15, 15, 19, 2, 12, 14, 60, 3,
                    "DeinonychusTalon1d8",
                    A("DeinonychusTalon1d8", "Bite1d6"),
                    A("Claw1d4"),
                    A("Pounce", "ImprovedInitiative",
                        "DeinonychusChassis"),
                    A("Perception", "Stealth"),
                    "Sprint 24 frozen adult routine: two primary talons at 1d8+2, one primary bite at 1d6+2, and one secondary foreclaws attack at 1d4+1. The plural label is one rolled attack, so the routine has exactly four attacks.",
                    "Pounce is the exact native carrier and may expose this ordinary full routine only after a legal charge. No Rake, Rend, Grab, Swallow Whole, fear, poison or Powerful Charge is present.",
                    "Exact land ranks are zero Mobility, two Perception and two Stealth. Narrow racial +8 carriers produce ordinary Mobility +10, Perception +14 and Stealth +15; the separate speed-derived jump adjustment is not generalized into movement skills.",
                    "Run remains omitted under RUN_NATIVE_SUMMON_AI_DISPOSITION. Low-light vision and scent remain PASSIVE_CREATURE_SENSES_UNMODELED without substitute senses."),
                PK("tyrannosaurus", "Tyrannosaurus", "Animal", 18,
                    "Gargantuan", 32, 13, 19, 2, 15, 10, 40, 14,
                    "TyrannosaurusBite4d6", Array.Empty<string>(),
                    A("CriticalFocus", "Diehard", "Endurance",
                        "ImprovedCriticalBite", "ImprovedInitiative",
                        "IronWill", "SkillFocusPerception",
                        "TyrannosaurusChassis",
                        "TyrannosaurusPowerfulBite",
                        "TyrannosaurusBleedingCritical"),
                    A("Perception"),
                    "Sprint 24 frozen adult routine: one primary bite at 4d6 with a 19-20 threat range. Powerful Bite makes the final signed Strength contribution exactly twice the live modifier; at Strength 32 that is +22, without multiplying any other damage.",
                    "Only an exact bite hit may Grab, with +4 to the grapple check and a same-size-or-smaller gate. Swallow Whole is separate: a target one category smaller must already have been held in that mouth at the start of the later qualifying round and a new check must succeed.",
                    "Periodic interior damage is the Tyrannosaurus-owned 2d8+11 payload, distinct from bite damage and its hit or critical riders. Printed interior AC 17 and hp 15 remain recorded under SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED.",
                    "Bleeding Critical applies the native 2d6 bleed payload only after this Tyrannosaurus's bite confirms a critical; Critical Focus never confirms automatically. No claws, tail attack, Pounce, trample, roar/fear or Power Attack is present.",
                    "Exact Perception is 18 ranks, +3 class, +2 Wisdom, +8 racial and +6 Skill Focus = +37. Run remains omitted under RUN_NATIVE_SUMMON_AI_DISPOSITION; passive senses remain PASSIVE_CREATURE_SENSES_UNMODELED.")
            };
        }

        private static NaturalSummonProfile P(string key, string name,
            string hitDieClass, int hitDice, string size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speed, int naturalArmor, string primary,
            string[] additional, string[] facts, params string[] deviations)
        {
            return new NaturalSummonProfile(key, name, hitDieClass, hitDice,
                size, strength, dexterity, constitution, intelligence, wisdom,
                charisma, speed, naturalArmor, primary, additional,
                Array.Empty<string>(), facts, deviations);
        }

        /// <summary>
        /// A profile that names its own skill ranks. Used by the Sprint 14
        /// vermin, whose stat blocks print none.
        /// </summary>
        private static NaturalSummonProfile PK(string key, string name,
            string hitDieClass, int hitDice, string size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speed, int naturalArmor, string primary,
            string[] additional, string[] facts, string[] skills,
            params string[] deviations)
        {
            return new NaturalSummonProfile(key, name, hitDieClass, hitDice,
                size, strength, dexterity, constitution, intelligence, wisdom,
                charisma, speed, naturalArmor, primary, additional,
                Array.Empty<string>(), facts, deviations, skills);
        }

        private static NaturalSummonProfile PSK(string key, string name,
            string hitDieClass, int hitDice, string size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speed, int naturalArmor, string primary,
            string[] additional, string[] secondary, string[] facts,
            string[] skills, params string[] deviations)
        {
            return new NaturalSummonProfile(key, name, hitDieClass, hitDice,
                size, strength, dexterity, constitution, intelligence, wisdom,
                charisma, speed, naturalArmor, primary, additional, secondary,
                facts, deviations, skills);
        }

        private static NaturalSummonProfile PS(string key, string name,
            string hitDieClass, int hitDice, string size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speed, int naturalArmor, string primary,
            string[] additional, string[] secondary, string[] facts,
            params string[] deviations)
        {
            return new NaturalSummonProfile(key, name, hitDieClass, hitDice,
                size, strength, dexterity, constitution, intelligence, wisdom,
                charisma, speed, naturalArmor, primary, additional, secondary,
                facts, deviations);
        }

        private static string[] A(params string[] values) { return values; }
    }
}
