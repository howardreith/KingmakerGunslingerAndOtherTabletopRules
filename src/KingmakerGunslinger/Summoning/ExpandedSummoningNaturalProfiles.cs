using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class NaturalSummonProfile
    {
        internal NaturalSummonProfile(string key, string displayName,
            string hitDieClass, int hitDice, string size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speedFeet, int naturalArmor,
            string primaryWeapon, string[] additionalWeapons,
            string[] additionalSecondaryWeapons,
            string[] facts, string[] deviations)
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
    }

    internal static class ExpandedSummoningNaturalProfiles
    {
        private static readonly NaturalSummonProfile[] Values = Build();
        internal static IReadOnlyList<NaturalSummonProfile> All
        { get { return Array.AsReadOnly(Values); } }

        internal static NaturalSummonProfile For(string key)
        { return Values.Single(value => value.Key == key); }

        internal static void Validate()
        {
            if (Values.Length != 26 || Values.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != Values.Length)
                throw new InvalidOperationException(
                    "The tier I-VII natural reconstruction catalog is incomplete or duplicated.");
            foreach (NaturalSummonProfile value in Values)
            {
                if (!ExpandedSummoningCatalog.All.Any(creature =>
                        creature.Key == value.Key) ||
                    (value.HitDieClass != "Animal" &&
                        value.HitDieClass != "Vermin") ||
                    value.HitDice < 1 || value.SpeedFeet < 1 ||
                    value.NaturalArmor < 0 ||
                    string.IsNullOrEmpty(value.PrimaryWeapon))
                    throw new InvalidOperationException(
                        "Invalid natural summon profile: " + value.Key + ".");
            }
            NaturalSummonProfile frog = For("poisonous-frog");
            if (frog.Size != "Tiny" || frog.Strength != 2 ||
                !frog.Facts.Contains("PoisonFrog"))
                throw new InvalidOperationException(
                    "Poisonous Frog tabletop profile changed.");
            NaturalSummonProfile spider = For("giant-spider");
            if (spider.HitDice != 3 || spider.NaturalArmor != 1 ||
                !spider.Facts.Contains("GiantSpiderPoison"))
                throw new InvalidOperationException(
                    "Giant Spider tabletop profile changed.");
        }

        private static NaturalSummonProfile[] Build()
        {
            return new[] {
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
                    2, 12, 11, 1, 9, 10, 10, 0, "Bite1d3",
                    Array.Empty<string>(),
                    A("WeaponFinesse", "TripDefenseFourLegs", "PoisonFrog"),
                    "The native Constitution-scaled poison graph supplies the exact six-tick 1d2 Constitution effect; ordinary-map ground speed is used and swim movement is omitted."),
                P("giant-centipede", "Giant Centipede", "Vermin", 1,
                    "Medium", 9, 15, 12, 1, 10, 2, 40, 2, "Bite1d6",
                    Array.Empty<string>(),
                    A("WeaponFinesse", "TripImmune", "CentipedePoison"),
                    "Kingmaker cannot represent an absent Intelligence score on BlueprintUnit, so Intelligence 1 is used. Climb movement is omitted; native poison is conservative because its graph does not expose the tabletop +2 racial DC bonus."),
                P("giant-spider", "Giant Spider", "Vermin", 3, "Medium",
                    11, 17, 12, 1, 10, 2, 30, 1, "Bite1d6",
                    Array.Empty<string>(),
                    A("TripDefenseEightLegs", "GiantSpiderPoison"),
                    "Kingmaker cannot represent an absent Intelligence score, so Intelligence 1 is used. Web, climb movement, and tremorsense are omitted pending bounded native contracts."),
                P("goblin-dog", "Goblin Dog", "Animal", 1, "Medium",
                    15, 14, 15, 2, 12, 8, 50, 1, "Bite1d6",
                    Array.Empty<string>(), A("Toughness"),
                    "Disease immunity and allergic reaction are omitted pending a duration-bound exact native disease contract; the Worg donor contributes only its view and bite animation."),
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
                    "The two extra native claw limbs are Kingmaker's established bounded representation of rake during pounce; actual non-charge cadence requires runtime qualification.",
                    "Grab is omitted because the installed generic graph carries unrelated Shambling Mound constrict and target-state behavior."),
                P("monitor-lizard", "Monitor Lizard", "Animal", 3, "Medium",
                    17, 15, 17, 2, 12, 6, 30, 3, "Bite1d8",
                    Array.Empty<string>(),
                    A("GreatFortitude", "SkillFocusPerception",
                        "TripDefenseFourLegs", "MonitorLizardPoison"),
                    "Kingmaker exposes one movement speed; the 30-foot ground speed is used and swim movement is omitted.",
                    "Grab is omitted because the installed generic graph carries unrelated Shambling Mound constrict and target-state behavior."),
                P("cheetah", "Cheetah", "Animal", 3, "Medium",
                    17, 19, 15, 2, 12, 6, 50, 1, "Bite1d6",
                    A("Claw1d3", "Claw1d3"),
                    A("TripDefenseFourLegs", "TrippingBite",
                        "WeaponFinesse", "ImprovedInitiative"),
                    "The once-per-hour tenfold sprint has no proven bounded native cooldown contract and is omitted conservatively."),
                PS("crocodile", "Crocodile", "Animal", 3, "Large",
                    19, 12, 17, 1, 12, 2, 20, 4, "Bite1d8",
                    Array.Empty<string>(), A("Tail1d12"),
                    A("ReducedReach", "TripDefenseFourLegs",
                        "SkillFocusPerception", "SkillFocusStealth"),
                    "Kingmaker exposes one movement speed; the 20-foot ground speed is used and swim movement is omitted.",
                    "Grab, death roll, sprint, and hold breath are omitted because no duration-bound summon-safe native graph was proven."),
                P("dire-bat", "Dire Bat", "Animal", 4, "Large",
                    17, 15, 13, 2, 14, 6, 40, 3, "Bite1d8",
                    Array.Empty<string>(),
                    A("ReducedReach", "Airborne", "Stealthy"),
                    "Kingmaker exposes one movement speed; 40-foot fly speed is used with airborne navigation and the 20-foot ground speed is omitted.",
                    "Blindsense and Alertness are omitted because no exact bounded native facts were proven in the final-live library."),
                PS("wolverine", "Wolverine", "Animal", 3, "Medium",
                    15, 15, 15, 2, 12, 10, 30, 2, "Claw1d6",
                    A("Claw1d6"), A("Bite1d4"),
                    A("SkillFocusPerception", "Toughness"),
                    "Burrow and climb movement are omitted because Kingmaker exposes one movement speed.",
                    "The after-damage rage is omitted pending a summon-local implementation proven to end with the summon and survive save/load."),
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
                    "Claw grab is omitted because the installed generic graph carries unrelated Shambling Mound constrict and target-state behavior.",
                    "Endurance, Run, and Skill Focus (Survival) are omitted because exact final-live feature identities were not proven."),
                P("lion", "Lion", "Animal", 5, "Large",
                    21, 17, 15, 2, 12, 6, 40, 3, "Bite1d8",
                    A("Claw1d4", "Claw1d4", "Claw1d4", "Claw1d4"),
                    A("ReducedReach", "Pounce", "TripDefenseFourLegs",
                        "ImprovedInitiative", "SkillFocusPerception"),
                    "The two extra native claw limbs are Kingmaker's established bounded representation of rake during pounce; actual non-charge cadence requires runtime qualification.",
                    "Grab and Run are omitted because no safe exact final-live contracts were proven."),
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
                    "The secondary claw pair is Kingmaker's bounded representation of rake during pounce; actual charge-only cadence requires runtime qualification.",
                    "Grab and Run are omitted because no summon-safe exact final-live contracts were proven."),
                P("ankylosaurus", "Ankylosaurus", "Animal", 10, "Huge",
                    27, 10, 17, 2, 13, 8, 30, 14, "Tail3d6",
                    Array.Empty<string>(), A("GreatFortitude", "PowerAttack"),
                    "The tail's Strength-based daze/stun rider is omitted pending an exact bounded native dazed-buff contract; the omission is conservative and never increases damage or control.",
                    "Improved Bull Rush, Improved Overrun, and Weapon Focus (tail) are omitted because exact concrete final-live feature identities were not proven."),
                P("dire-bear", "Dire Bear", "Animal", 10, "Large",
                    25, 13, 21, 2, 12, 10, 40, 8, "Bite1d8",
                    A("Claw1d6", "Claw1d6"), A("ReducedReach",
                        "ImprovedInitiative", "IronWill",
                        "SkillFocusPerception"),
                    "Claw grab is omitted because the installed generic graph carries unrelated Shambling Mound constrict and target-state behavior.",
                    "Endurance and Run are omitted because exact concrete final-live feature identities were not proven."),
                PS("dire-tiger", "Smilodon", "Animal", 14,
                    "Large", 27, 15, 17, 2, 12, 10, 40, 6,
                    "BiteLarge2d6", A("Claw2d4", "Claw2d4"),
                    A("Claw2d4", "Claw2d4"), A("ReducedReach", "Pounce",
                        "TripDefenseFourLegs", "ImprovedCriticalBite",
                        "ImprovedCriticalClaw", "ImprovedInitiative",
                        "SkillFocusPerception", "SkillFocusStealth",
                        "WeaponFocusBite", "WeaponFocusClaw"),
                    "The secondary claw pair is Kingmaker's bounded representation of rake during pounce; actual charge-only cadence requires runtime qualification.",
                    "Grab and Run are omitted because no summon-safe exact final-live contracts were proven."),
                PS("elephant", "Elephant", "Animal", 11, "Huge",
                    30, 10, 19, 2, 13, 7, 40, 9, "Gore2d8",
                    Array.Empty<string>(), A("Slam2d6"),
                    A("GreatFortitude", "IronWill", "PowerAttack",
                        "SkillFocusPerception"),
                    "Trample is omitted pending a player-commandable, path-safe native movement contract; ordinary gore and slam attacks remain exact and the omission is conservative.",
                    "Endurance and Improved Bull Rush are omitted because exact concrete final-live feature identities were not proven."),
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
                    "Talon grab and Flyby Attack are omitted because no summon-safe exact final-live contracts were proven.")
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
