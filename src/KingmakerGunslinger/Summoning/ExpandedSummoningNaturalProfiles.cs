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
            if (Values.Length != 47 || Values.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != Values.Length)
                throw new InvalidOperationException(
                    "The natural reconstruction catalog is incomplete or duplicated.");
            foreach (NaturalSummonProfile value in Values)
            {
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
        }

        private static NaturalSummonProfile[] Build()
        {
            return new[] {
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
                    "The summon-local powerful charge applies only to its gore on a native charge and uses the printed 4d8+18 result. Trample uses the disclosed Kingmaker automatic-AoO-or-Reflex adaptation. Diehard and Endurance are omitted because exact summon-safe feat identities were not proven. The sanitized Mastodon rig drives the original Woolly Rhinoceros mesh and painting.")
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
