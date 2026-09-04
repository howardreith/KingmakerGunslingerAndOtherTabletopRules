using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    internal enum ElementalFeatId
    {
        ElementalStrike = 0,
        ScorchingWeapons = 1,
        InnerFlame = 2,
        BlazingAura = 3,
        Firesight = 4,
        AiryStep = 5,
        WingsOfAir = 6,
        CloudGazer = 7,
        InnerBreath = 8,
        HydraulicManeuver = 9,
        TritonPortal = 10
    }

    internal enum ElementalFeatActionEconomy
    {
        Passive = 0,
        Swift = 1,
        Free = 2,
        FullRound = 3
    }

    internal enum ElementalFeatEnergy
    {
        Acid = 0,
        Cold = 1,
        Electricity = 2,
        Fire = 3
    }

    internal enum ElementalHydraulicManeuver
    {
        BullRush = 0,
        Disarm = 1,
        Trip = 2,
        DirtyTrickBlind = 3
    }

    internal enum ElementalConcealmentFamily
    {
        None = 0,
        Fire = 1,
        Smoke = 2,
        FogMistOrCloud = 3,
        Blur = 4,
        Displacement = 5,
        Invisibility = 6,
        Darkness = 7,
        Blindness = 8,
        MirrorImage = 9
    }

    internal sealed class ElementalFeatDefinition
    {
        internal ElementalFeatDefinition(ElementalFeatId id, string name,
            bool isCombat, int minimumCharacterLevel,
            bool requiresHydraulicPush,
            ElementalFeatActionEconomy actionEconomy, int durationRounds,
            ElementalHeritageRace[] allowedRaces,
            params ElementalFeatId[] requiredFeats)
        {
            if (string.IsNullOrWhiteSpace(name) || minimumCharacterLevel < 0 ||
                durationRounds < 0 || allowedRaces == null ||
                allowedRaces.Length == 0 ||
                allowedRaces.Distinct().Count() != allowedRaces.Length ||
                requiredFeats == null ||
                requiredFeats.Distinct().Count() != requiredFeats.Length)
                throw new ArgumentException(
                    "An elemental feat definition is incomplete.");

            Id = id;
            Name = name;
            IsCombat = isCombat;
            MinimumCharacterLevel = minimumCharacterLevel;
            RequiresHydraulicPush = requiresHydraulicPush;
            ActionEconomy = actionEconomy;
            DurationRounds = durationRounds;
            AllowedRaces = (ElementalHeritageRace[])allowedRaces.Clone();
            RequiredFeats = (ElementalFeatId[])requiredFeats.Clone();
        }

        internal ElementalFeatId Id { get; private set; }
        internal string Name { get; private set; }
        internal bool IsCombat { get; private set; }
        internal int MinimumCharacterLevel { get; private set; }
        internal bool RequiresHydraulicPush { get; private set; }
        internal ElementalFeatActionEconomy ActionEconomy { get; private set; }
        internal int DurationRounds { get; private set; }
        internal ElementalHeritageRace[] AllowedRaces { get; private set; }
        internal ElementalFeatId[] RequiredFeats { get; private set; }
    }

    internal sealed class ElementalFeatQualification
    {
        private readonly HashSet<ElementalFeatId> m_OwnedFeats;

        internal ElementalFeatQualification(ElementalHeritageRace? exactRace,
            int totalCharacterLevel, bool hasActiveHydraulicPush,
            IEnumerable<ElementalFeatId> ownedFeats)
        {
            ExactRace = exactRace;
            TotalCharacterLevel = totalCharacterLevel;
            HasActiveHydraulicPush = hasActiveHydraulicPush;
            m_OwnedFeats = new HashSet<ElementalFeatId>(ownedFeats ??
                Enumerable.Empty<ElementalFeatId>());
        }

        internal ElementalHeritageRace? ExactRace { get; private set; }
        internal int TotalCharacterLevel { get; private set; }
        internal bool HasActiveHydraulicPush { get; private set; }

        internal bool Owns(ElementalFeatId feat)
        {
            return m_OwnedFeats.Contains(feat);
        }
    }

    internal sealed class ElementalHeldWeaponCandidate
    {
        internal ElementalHeldWeaponCandidate(string exactItemIdentity,
            bool isHeld, bool isManufactured, bool isMetallic)
        {
            if (string.IsNullOrWhiteSpace(exactItemIdentity))
                throw new ArgumentException(
                    "A held weapon candidate requires an exact item identity.",
                    "exactItemIdentity");
            ExactItemIdentity = exactItemIdentity;
            IsHeld = isHeld;
            IsManufactured = isManufactured;
            IsMetallic = isMetallic;
        }

        internal string ExactItemIdentity { get; private set; }
        internal bool IsHeld { get; private set; }
        internal bool IsManufactured { get; private set; }
        internal bool IsMetallic { get; private set; }
    }

    internal sealed class ElementalFeatDamageAmount
    {
        internal ElementalFeatDamageAmount(int diceCount, int dieSides,
            int flatBonus)
        {
            if (diceCount < 0 || dieSides < 0 || flatBonus < 0 ||
                (diceCount == 0) != (dieSides == 0))
                throw new ArgumentOutOfRangeException();
            DiceCount = diceCount;
            DieSides = dieSides;
            FlatBonus = flatBonus;
        }

        internal int DiceCount { get; private set; }
        internal int DieSides { get; private set; }
        internal int FlatBonus { get; private set; }
        internal bool IsEmpty
        {
            get { return DiceCount == 0 && FlatBonus == 0; }
        }
    }

    internal sealed class ElementalFeatEventLedger
    {
        private readonly HashSet<Tuple<string, string>> m_Claims =
            new HashSet<Tuple<string, string>>();

        internal bool TryClaim(string exactEffectIdentity,
            string exactEventIdentity)
        {
            if (string.IsNullOrWhiteSpace(exactEffectIdentity) ||
                string.IsNullOrWhiteSpace(exactEventIdentity))
                return false;
            return m_Claims.Add(Tuple.Create(exactEffectIdentity,
                exactEventIdentity));
        }

        internal void ReleaseEffect(string exactEffectIdentity)
        {
            if (string.IsNullOrWhiteSpace(exactEffectIdentity)) return;
            m_Claims.RemoveWhere(entry => string.Equals(entry.Item1,
                exactEffectIdentity, StringComparison.Ordinal));
        }
    }

    internal static class ElementalFeatPolicy
    {
        // Kingmaker 2.1.7b has no SpellDescriptor.Light member. These exact
        // native Spell blueprints are the locally audited PF1 light-descriptor
        // family. Evidence: guarded KMG-only run
        // 20260904T2056551837877Z, native-audit SHA-256
        // 34adf61f8bf6194b7504e7cf5a9dba04631236c40ac19d4f8f2563dc61091aef.
        private static readonly string[] s_ExactNativeLightSpellGuids =
        {
            "2b877386976817a429002e8bb10bb3fc", // Daylight
            "f0f8e5b9808f44e4eadd22b138131d52", // Flare
            "39a602aa80cc96f4597778b6d4d49c0a", // Flare Burst
            "bf0accce250381a44b857d4af6c8e10d", // Searing Light
            "1fca0ba2fdfe2994a8c8bc1f0f2fc5b1", // Sunbeam
            "a9e9c0df76399fe4795c0baf2c136a92", // Sunbeam delivery
            "e96424f70ff884947b06f41a765b7658"  // Sunburst
        };

        private static readonly HashSet<string> s_ExactNativeLightSpellSet =
            new HashSet<string>(s_ExactNativeLightSpellGuids,
                StringComparer.Ordinal);

        internal const int FeatCount = 11;

        private static readonly ElementalHydraulicManeuver[]
            s_HydraulicManeuvers =
            {
                ElementalHydraulicManeuver.BullRush,
                ElementalHydraulicManeuver.Disarm,
                ElementalHydraulicManeuver.Trip,
                ElementalHydraulicManeuver.DirtyTrickBlind
            };

        internal static IReadOnlyList<ElementalFeatDefinition> Ordered()
        {
            ElementalHeritageRace[] allRaces =
            {
                ElementalHeritageRace.Ifrit,
                ElementalHeritageRace.Oread,
                ElementalHeritageRace.Sylph,
                ElementalHeritageRace.Undine
            };
            ElementalFeatDefinition[] result =
            {
                F(ElementalFeatId.ElementalStrike, "Elemental Strike", true,
                    0, false, ElementalFeatActionEconomy.Swift, 1, allRaces),
                F(ElementalFeatId.ScorchingWeapons, "Scorching Weapons", true,
                    0, false, ElementalFeatActionEconomy.Swift, 1,
                    R(ElementalHeritageRace.Ifrit)),
                F(ElementalFeatId.InnerFlame, "Inner Flame", true,
                    7, false, ElementalFeatActionEconomy.Passive, 0,
                    R(ElementalHeritageRace.Ifrit),
                    ElementalFeatId.ScorchingWeapons),
                F(ElementalFeatId.BlazingAura, "Blazing Aura", true,
                    13, false, ElementalFeatActionEconomy.Free, 1,
                    R(ElementalHeritageRace.Ifrit),
                    ElementalFeatId.ScorchingWeapons,
                    ElementalFeatId.InnerFlame),
                F(ElementalFeatId.Firesight, "Firesight", false,
                    0, false, ElementalFeatActionEconomy.Passive, 0,
                    R(ElementalHeritageRace.Ifrit)),
                F(ElementalFeatId.AiryStep, "Airy Step", false,
                    0, false, ElementalFeatActionEconomy.Passive, 0,
                    R(ElementalHeritageRace.Sylph)),
                F(ElementalFeatId.WingsOfAir, "Wings of Air", false,
                    9, false, ElementalFeatActionEconomy.Passive, 0,
                    R(ElementalHeritageRace.Sylph),
                    ElementalFeatId.AiryStep),
                F(ElementalFeatId.CloudGazer, "Cloud Gazer", false,
                    0, false, ElementalFeatActionEconomy.Passive, 0,
                    R(ElementalHeritageRace.Sylph)),
                F(ElementalFeatId.InnerBreath, "Inner Breath", false,
                    11, false, ElementalFeatActionEconomy.Passive, 0,
                    R(ElementalHeritageRace.Sylph)),
                F(ElementalFeatId.HydraulicManeuver,
                    "Hydraulic Maneuver", false, 0, true,
                    ElementalFeatActionEconomy.Passive, 0,
                    R(ElementalHeritageRace.Undine)),
                F(ElementalFeatId.TritonPortal, "Triton Portal", false,
                    5, true, ElementalFeatActionEconomy.FullRound, 0,
                    R(ElementalHeritageRace.Undine))
            };
            if (result.Length != FeatCount ||
                result.Select(entry => entry.Id).Distinct().Count() !=
                FeatCount)
                throw new InvalidOperationException(
                    "Elemental feat catalog count or identity drifted.");
            return result;
        }

        internal static bool Qualifies(ElementalFeatDefinition feat,
            ElementalFeatQualification candidate)
        {
            if (feat == null || candidate == null ||
                !candidate.ExactRace.HasValue ||
                candidate.TotalCharacterLevel < feat.MinimumCharacterLevel ||
                !feat.AllowedRaces.Contains(candidate.ExactRace.Value) ||
                (feat.RequiresHydraulicPush &&
                 !candidate.HasActiveHydraulicPush))
                return false;
            return feat.RequiredFeats.All(candidate.Owns);
        }

        internal static int ElementalStrikeBonus(int totalCharacterLevel)
        {
            if (totalCharacterLevel < 1) return 0;
            return Math.Min(5, 1 + totalCharacterLevel / 5);
        }

        internal static ElementalFeatEnergy ElementalStrikeEnergy(
            ElementalHeritageRace race)
        {
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    return ElementalFeatEnergy.Fire;
                case ElementalHeritageRace.Oread:
                    return ElementalFeatEnergy.Acid;
                case ElementalHeritageRace.Sylph:
                    return ElementalFeatEnergy.Electricity;
                case ElementalHeritageRace.Undine:
                    return ElementalFeatEnergy.Cold;
                default:
                    throw new ArgumentOutOfRangeException("race");
            }
        }

        internal static bool IsQualifyingElementalStrikeDamage(
            bool effectIsActive, bool isSuccessfulWeaponAttack,
            bool belongsToWeaponDamageBundle, bool isSpellDamage)
        {
            return effectIsActive && isSuccessfulWeaponAttack &&
                belongsToWeaponDamageBundle && !isSpellDamage;
        }

        internal static int ScorchingWeaponsSaveBonus(
            bool hasScorchingWeapons, bool hasInnerFlame,
            bool isFireAttack, bool hasFireDescriptor,
            bool hasLightDescriptor)
        {
            if (!hasScorchingWeapons ||
                !(isFireAttack || hasFireDescriptor || hasLightDescriptor))
                return 0;
            return hasInnerFlame ? 4 : 2;
        }

        internal static string[] ExactNativeLightSpellGuids()
        {
            return (string[])s_ExactNativeLightSpellGuids.Clone();
        }

        internal static bool IsExactNativeLightSpellGuid(string guid)
        {
            return !string.IsNullOrEmpty(guid) &&
                s_ExactNativeLightSpellSet.Contains(guid);
        }

        internal static string[] SnapshotScorchingWeapons(
            IEnumerable<ElementalHeldWeaponCandidate> candidates)
        {
            if (candidates == null) return new string[0];
            return candidates
                .Where(entry => entry != null && entry.IsHeld &&
                    entry.IsManufactured && entry.IsMetallic)
                .Select(entry => entry.ExactItemIdentity)
                .Distinct(StringComparer.Ordinal)
                .Take(2)
                .ToArray();
        }

        internal static ElementalFeatDamageAmount ScorchingWeaponsDamage(
            bool effectIsOnExactWeapon, bool hasInnerFlame,
            bool weaponAlreadyAddsFireDamage)
        {
            if (!effectIsOnExactWeapon || weaponAlreadyAddsFireDamage)
                return new ElementalFeatDamageAmount(0, 0, 0);
            return hasInnerFlame
                ? new ElementalFeatDamageAmount(1, 6, 0)
                : new ElementalFeatDamageAmount(0, 0, 1);
        }

        internal static bool BlazingAuraAffectsTurnStart(
            bool auraIsActive, bool creatureTurnIsBeginning,
            bool creatureIsAdjacent)
        {
            return auraIsActive && creatureTurnIsBeginning &&
                creatureIsAdjacent;
        }

        internal static int AiryStepSaveBonus(bool hasAiryStep,
            bool hasWingsOfAir, bool hasAirDescriptor,
            bool hasElectricityDescriptor, bool dealsElectricityDamage)
        {
            if (!hasAiryStep || !(hasAirDescriptor ||
                hasElectricityDescriptor || dealsElectricityDamage))
                return 0;
            return hasWingsOfAir ? 4 : 2;
        }

        internal static bool WingsOfAirIsActive(bool hasWingsOfAir,
            bool wearsNoArmorOrLightArmor)
        {
            return hasWingsOfAir && wearsNoArmorOrLightArmor;
        }

        internal static bool FiresightIgnores(
            ElementalConcealmentFamily source)
        {
            return source == ElementalConcealmentFamily.Fire ||
                source == ElementalConcealmentFamily.Smoke;
        }

        internal static bool CloudGazerIgnores(
            ElementalConcealmentFamily source)
        {
            return source == ElementalConcealmentFamily.FogMistOrCloud;
        }

        internal static bool InnerBreathGrantsImmunity(
            bool effectExplicitlyRequiresBreathing)
        {
            return effectExplicitlyRequiresBreathing;
        }

        internal static ElementalHydraulicManeuver[] HydraulicManeuvers()
        {
            return (ElementalHydraulicManeuver[])
                s_HydraulicManeuvers.Clone();
        }

        internal static int HydraulicManeuverBonus(int totalCharacterLevel,
            int intelligenceModifier, int wisdomModifier,
            int charismaModifier)
        {
            if (totalCharacterLevel < 1) return 0;
            return totalCharacterLevel + Math.Max(intelligenceModifier,
                Math.Max(wisdomModifier, charismaModifier));
        }

        internal static int TritonPortalSmallWaterElementalCount(
            int d3Result)
        {
            if (d3Result < 1 || d3Result > 3)
                throw new ArgumentOutOfRangeException("d3Result");
            return d3Result;
        }

        internal static int TritonPortalDurationRounds(
            int totalCharacterLevel)
        {
            return Math.Max(1, totalCharacterLevel);
        }

        private static ElementalFeatDefinition F(ElementalFeatId id,
            string name, bool combat, int level, bool hydraulic,
            ElementalFeatActionEconomy action, int duration,
            ElementalHeritageRace[] races,
            params ElementalFeatId[] prerequisites)
        {
            return new ElementalFeatDefinition(id, name, combat, level,
                hydraulic, action, duration, races, prerequisites);
        }

        private static ElementalHeritageRace[] R(
            ElementalHeritageRace race)
        {
            return new[] { race };
        }
    }
}
