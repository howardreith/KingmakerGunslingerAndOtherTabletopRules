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
            string[] facts, string[] deviations)
        {
            Key = key; DisplayName = displayName; HitDieClass = hitDieClass;
            HitDice = hitDice; Size = size; Strength = strength;
            Dexterity = dexterity; Constitution = constitution;
            Intelligence = intelligence; Wisdom = wisdom; Charisma = charisma;
            SpeedFeet = speedFeet; NaturalArmor = naturalArmor;
            PrimaryWeapon = primaryWeapon;
            AdditionalWeapons = additionalWeapons ?? Array.Empty<string>();
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
            if (Values.Length != 7 || Values.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != Values.Length)
                throw new InvalidOperationException(
                    "The low-tier natural reconstruction catalog is incomplete or duplicated.");
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
                        "SkillFocusPerception"))
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
                charisma, speed, naturalArmor, primary, additional, facts,
                deviations);
        }

        private static string[] A(params string[] values) { return values; }
    }
}
