using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    [Flags]
    internal enum ElementalRacialTraitSlot
    {
        None = 0,
        EnergyResistance = 1,
        ElementalAffinity = 2,
        RacialSpellLikeAbility = 4
    }

    internal enum ElementalAlternateTraitId
    {
        WildfireHeart = 0,
        BrazenFlame = 1,
        FireInTheBlood = 2,
        EfreetiMagic = 3,
        ForgeHardened = 4,
        FireInsight = 5,
        CrystallineForm = 6,
        EarthInsight = 7,
        GraniteSkin = 8,
        StoneInTheBlood = 9,
        TreacherousEarth = 10,
        AirInsight = 11,
        BreezeKissed = 12,
        LikeTheWind = 13,
        Secretive = 14,
        StormInTheBlood = 15,
        ThunderousResilience = 16,
        WhisperingWind = 17,
        AcidBreath = 18,
        NereidFascination = 19,
        OozeBreath = 20
    }

    internal sealed class ElementalAlternateTraitDefinition
    {
        internal ElementalAlternateTraitDefinition(
            ElementalAlternateTraitId id, ElementalHeritageRace parentRace,
            string name, string description,
            ElementalRacialTraitSlot replacedSlots)
        {
            const ElementalRacialTraitSlot allSlots =
                ElementalRacialTraitSlot.EnergyResistance |
                ElementalRacialTraitSlot.ElementalAffinity |
                ElementalRacialTraitSlot.RacialSpellLikeAbility;
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(description) ||
                replacedSlots == ElementalRacialTraitSlot.None ||
                (replacedSlots & ~allSlots) != 0)
                throw new ArgumentException(
                    "An elemental alternate racial trait definition is incomplete.");
            Id = id;
            ParentRace = parentRace;
            Name = name;
            Description = description;
            ReplacedSlots = replacedSlots;
            MarkerSymbol = "KMG.ElementalRaces.Traits." + parentRace + "." +
                id;
            ProviderSymbol = MarkerSymbol + ".Provider";
            PrimarySlot = FirstSlot(replacedSlots);
        }

        internal ElementalAlternateTraitId Id { get; private set; }
        internal ElementalHeritageRace ParentRace { get; private set; }
        internal string Name { get; private set; }
        internal string Description { get; private set; }
        internal ElementalRacialTraitSlot ReplacedSlots { get; private set; }
        internal string MarkerSymbol { get; private set; }
        internal string ProviderSymbol { get; private set; }
        internal ElementalRacialTraitSlot PrimarySlot { get; private set; }

        internal bool Replaces(ElementalRacialTraitSlot slot)
        {
            return slot != ElementalRacialTraitSlot.None &&
                (ReplacedSlots & slot) == slot;
        }

        private static ElementalRacialTraitSlot FirstSlot(
            ElementalRacialTraitSlot slots)
        {
            if ((slots & ElementalRacialTraitSlot.EnergyResistance) != 0)
                return ElementalRacialTraitSlot.EnergyResistance;
            if ((slots & ElementalRacialTraitSlot.ElementalAffinity) != 0)
                return ElementalRacialTraitSlot.ElementalAffinity;
            return ElementalRacialTraitSlot.RacialSpellLikeAbility;
        }
    }

    internal sealed class ElementalAlternateTraitSelectionDefinition
    {
        private readonly ElementalAlternateTraitDefinition[] m_Choices;

        internal ElementalAlternateTraitSelectionDefinition(
            ElementalHeritageRace race, ElementalRacialTraitSlot slot,
            IEnumerable<ElementalAlternateTraitDefinition> choices)
        {
            m_Choices = choices == null ? null : choices.ToArray();
            if (m_Choices == null || m_Choices.Length == 0 ||
                m_Choices.Any(value => value == null ||
                    value.ParentRace != race || value.PrimarySlot != slot))
                throw new ArgumentException(
                    "An alternate-trait selection requires exact same-race primary-slot choices.");
            Race = race;
            Slot = slot;
            string stem = "KMG.ElementalRaces.Traits." + race + "." + slot;
            SelectionSymbol = stem + "Selection";
            RetainMarkerSymbol = stem + ".Retain";
            Name = race + " " + SlotName(slot) + " Trait";
            Description = "Retain the base " + SlotName(slot).ToLowerInvariant() +
                " trait or choose one alternate racial trait that replaces it. " +
                "Options that also replace another slot cannot be combined with an overlapping choice.";
        }

        internal ElementalHeritageRace Race { get; private set; }
        internal ElementalRacialTraitSlot Slot { get; private set; }
        internal string SelectionSymbol { get; private set; }
        internal string RetainMarkerSymbol { get; private set; }
        internal string Name { get; private set; }
        internal string Description { get; private set; }

        internal IReadOnlyList<ElementalAlternateTraitDefinition> Choices
        {
            get
            {
                return Array.AsReadOnly((ElementalAlternateTraitDefinition[])
                    m_Choices.Clone());
            }
        }

        private static string SlotName(ElementalRacialTraitSlot slot)
        {
            switch (slot)
            {
                case ElementalRacialTraitSlot.EnergyResistance:
                    return "Energy Resistance";
                case ElementalRacialTraitSlot.ElementalAffinity:
                    return "Elemental Affinity";
                case ElementalRacialTraitSlot.RacialSpellLikeAbility:
                    return "Spell-Like Ability";
                default:
                    throw new ArgumentOutOfRangeException("slot");
            }
        }
    }

    internal sealed class ElementalAlternateTraitState
    {
        private readonly ElementalAlternateTraitDefinition[] m_Traits;
        private readonly string[] m_TraitProviderSymbols;

        internal ElementalAlternateTraitState(ElementalHeritageRace race,
            ElementalHeritageDefinition heritage,
            IEnumerable<ElementalAlternateTraitDefinition> traits,
            ElementalRacialTraitSlot consumedSlots,
            string energyResistanceProviderSymbol,
            string elementalAffinityProviderSymbol,
            string racialSlaFeatureSymbol, string racialSlaResourceSymbol,
            string racialSlaAbilitySymbol)
        {
            Race = race;
            Heritage = heritage ?? throw new ArgumentNullException(
                "heritage");
            m_Traits = traits == null
                ? new ElementalAlternateTraitDefinition[0]
                : traits.ToArray();
            if (m_Traits.Any(value => value == null))
                throw new ArgumentException(
                    "Desired alternate-trait state cannot contain null traits.");
            m_TraitProviderSymbols = m_Traits.Select(value =>
                value.ProviderSymbol).ToArray();
            ConsumedSlots = consumedSlots;
            EnergyResistanceProviderSymbol = energyResistanceProviderSymbol;
            ElementalAffinityProviderSymbol = elementalAffinityProviderSymbol;
            RacialSlaFeatureSymbol = racialSlaFeatureSymbol;
            RacialSlaResourceSymbol = racialSlaResourceSymbol;
            RacialSlaAbilitySymbol = racialSlaAbilitySymbol;
            Fingerprint = CreateFingerprint();
        }

        internal ElementalHeritageRace Race { get; private set; }
        internal ElementalHeritageDefinition Heritage { get; private set; }
        internal IReadOnlyList<ElementalAlternateTraitDefinition> Traits
        {
            get { return Array.AsReadOnly((ElementalAlternateTraitDefinition[])
                m_Traits.Clone()); }
        }
        internal ElementalRacialTraitSlot ConsumedSlots { get; private set; }
        internal string EnergyResistanceProviderSymbol { get; private set; }
        internal string ElementalAffinityProviderSymbol { get; private set; }
        internal string RacialSlaFeatureSymbol { get; private set; }
        internal string RacialSlaResourceSymbol { get; private set; }
        internal string RacialSlaAbilitySymbol { get; private set; }
        internal string Fingerprint { get; private set; }

        internal bool HasActiveHydraulicPush
        {
            get
            {
                return string.Equals(RacialSlaAbilitySymbol,
                    "KMG.ElementalRaces.Undine.HydraulicPushAbility",
                    StringComparison.Ordinal);
            }
        }

        internal int ModifierFor(ElementalHeritageStat stat)
        {
            return Heritage.ModifierFor(stat);
        }

        internal string[] MarkerSymbols()
        {
            return m_Traits.Select(value => value.MarkerSymbol).ToArray();
        }

        internal string[] TraitProviderSymbols()
        {
            return (string[])m_TraitProviderSymbols.Clone();
        }

        private string CreateFingerprint()
        {
            return string.Join("|", new[]
            {
                Race.ToString(),
                Heritage.Id.ToString(),
                ((int)ConsumedSlots).ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                string.Join(",", m_Traits.Select(value =>
                    ((int)value.Id).ToString(
                        System.Globalization.CultureInfo.InvariantCulture))),
                string.Join(",", m_TraitProviderSymbols),
                EnergyResistanceProviderSymbol ?? "-",
                ElementalAffinityProviderSymbol ?? "-",
                RacialSlaFeatureSymbol ?? "-",
                RacialSlaResourceSymbol ?? "-",
                RacialSlaAbilitySymbol ?? "-"
            });
        }
    }

    internal static class ElementalAlternateTraitPolicy
    {
        internal const int TraitCount = 21;
        internal const int SelectionCount = 10;
        internal const int FrameworkIdentityCount = TraitCount * 2 +
            SelectionCount * 2;

        private const ElementalRacialTraitSlot AllSlots =
            ElementalRacialTraitSlot.EnergyResistance |
            ElementalRacialTraitSlot.ElementalAffinity |
            ElementalRacialTraitSlot.RacialSpellLikeAbility;

        internal static IReadOnlyList<ElementalAlternateTraitDefinition>
            Ordered()
        {
            ElementalAlternateTraitDefinition[] result =
            {
                T(ElementalAlternateTraitId.WildfireHeart,
                    ElementalHeritageRace.Ifrit, "Wildfire Heart",
                    "+4 racial bonus on initiative.",
                    ElementalRacialTraitSlot.EnergyResistance),
                T(ElementalAlternateTraitId.BrazenFlame,
                    ElementalHeritageRace.Ifrit, "Brazen Flame",
                    "+1 fire damage with successful melee attacks.",
                    ElementalRacialTraitSlot.EnergyResistance |
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),
                T(ElementalAlternateTraitId.FireInTheBlood,
                    ElementalHeritageRace.Ifrit, "Fire in the Blood",
                    "Fire damage triggers fast healing 2 for 1 round, up to 2 hit points per character level each day.",
                    ElementalRacialTraitSlot.ElementalAffinity),
                T(ElementalAlternateTraitId.EfreetiMagic,
                    ElementalHeritageRace.Ifrit, "Efreeti Magic",
                    "Once per ordinary rest, choose Enlarge Person or Reduce Person as a spell-like ability. Both choices share one use, use total character level as caster level and Charisma for saving throw DCs, and retain native person targeting including this project's Ifrit heritages.",
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),
                T(ElementalAlternateTraitId.ForgeHardened,
                    ElementalHeritageRace.Ifrit, "Forge-Hardened",
                    "+2 racial bonus on saves against fatigue and exhaustion; Kingmaker has no Craft (armor or weapons) skill.",
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),
                T(ElementalAlternateTraitId.FireInsight,
                    ElementalHeritageRace.Ifrit, "Fire Insight",
                    "Summon Monster and Summon Nature's Ally spells last 2 rounds longer when summoning fire-subtype creatures.",
                    ElementalRacialTraitSlot.ElementalAffinity),

                T(ElementalAlternateTraitId.CrystallineForm,
                    ElementalHeritageRace.Oread, "Crystalline Form",
                    ElementalCrystallineFormPolicy.Description,
                    ElementalRacialTraitSlot.ElementalAffinity),
                T(ElementalAlternateTraitId.EarthInsight,
                    ElementalHeritageRace.Oread, "Earth Insight",
                    "Summon Monster and Summon Nature's Ally spells last 2 rounds longer when summoning earth-subtype creatures.",
                    ElementalRacialTraitSlot.ElementalAffinity),
                T(ElementalAlternateTraitId.GraniteSkin,
                    ElementalHeritageRace.Oread, "Granite Skin",
                    "+1 racial natural armor bonus.",
                    ElementalRacialTraitSlot.EnergyResistance),
                T(ElementalAlternateTraitId.StoneInTheBlood,
                    ElementalHeritageRace.Oread, "Stone in the Blood",
                    "Acid damage triggers fast healing 2 for 1 round, up to 2 hit points per character level each day.",
                    ElementalRacialTraitSlot.ElementalAffinity),
                T(ElementalAlternateTraitId.TreacherousEarth,
                    ElementalHeritageRace.Oread, "Treacherous Earth",
                    "Once per day, create a 10-foot-radius patch of difficult terrain for 1 minute per character level.",
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),

                T(ElementalAlternateTraitId.AirInsight,
                    ElementalHeritageRace.Sylph, "Air Insight",
                    "Summon Monster and Summon Nature's Ally spells last 2 rounds longer when summoning air-subtype creatures.",
                    ElementalRacialTraitSlot.ElementalAffinity),
                T(ElementalAlternateTraitId.BreezeKissed,
                    ElementalHeritageRace.Sylph, "Breeze-Kissed",
                    "+2 racial AC against nonmagical ranged attacks while winds remain, with one daily Bull Rush or Trip gust.",
                    ElementalRacialTraitSlot.ElementalAffinity),
                T(ElementalAlternateTraitId.LikeTheWind,
                    ElementalHeritageRace.Sylph, "Like the Wind",
                    "+5 feet to base speed.",
                    ElementalRacialTraitSlot.EnergyResistance),
                T(ElementalAlternateTraitId.Secretive,
                    ElementalHeritageRace.Sylph, "Secretive",
                    "+2 racial bonus on saves against Enchantment and Divination spells and effects.",
                    ElementalRacialTraitSlot.EnergyResistance |
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),
                T(ElementalAlternateTraitId.StormInTheBlood,
                    ElementalHeritageRace.Sylph, "Storm in the Blood",
                    "Electricity damage triggers fast healing 2 for 1 round, up to 2 hit points per character level each day.",
                    ElementalRacialTraitSlot.ElementalAffinity),
                T(ElementalAlternateTraitId.ThunderousResilience,
                    ElementalHeritageRace.Sylph, "Thunderous Resilience",
                    "Sonic resistance 5.",
                    ElementalRacialTraitSlot.EnergyResistance),
                T(ElementalAlternateTraitId.WhisperingWind,
                    ElementalHeritageRace.Sylph, "Whispering Wind",
                    "+4 racial bonus on Stealth.",
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),

                T(ElementalAlternateTraitId.AcidBreath,
                    ElementalHeritageRace.Undine, "Acid Breath",
                    ElementalBreathPolicy.Description(false),
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),
                T(ElementalAlternateTraitId.NereidFascination,
                    ElementalHeritageRace.Undine, "Nereid Fascination",
                    "Once per day, create a 20-foot aura that fascinates humanoids; Will negates.",
                    ElementalRacialTraitSlot.RacialSpellLikeAbility),
                T(ElementalAlternateTraitId.OozeBreath,
                    ElementalHeritageRace.Undine, "Ooze Breath",
                    ElementalBreathPolicy.Description(true),
                    ElementalRacialTraitSlot.RacialSpellLikeAbility)
            };
            Validate(result);
            return result;
        }

        internal static IReadOnlyList<ElementalAlternateTraitDefinition>
            ForRace(ElementalHeritageRace race)
        {
            ValidateRace(race);
            return Ordered().Where(value => value.ParentRace == race)
                .ToArray();
        }

        internal static IReadOnlyList<
            ElementalAlternateTraitSelectionDefinition> OrderedSelections()
        {
            var result = new List<
                ElementalAlternateTraitSelectionDefinition>();
            ElementalAlternateTraitDefinition[] definitions = Ordered()
                .ToArray();
            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
                foreach (ElementalRacialTraitSlot slot in new[]
                {
                    ElementalRacialTraitSlot.EnergyResistance,
                    ElementalRacialTraitSlot.ElementalAffinity,
                    ElementalRacialTraitSlot.RacialSpellLikeAbility
                })
                {
                    ElementalAlternateTraitDefinition[] choices = definitions
                        .Where(value => value.ParentRace == race &&
                            value.PrimarySlot == slot).ToArray();
                    if (choices.Length > 0)
                        result.Add(new
                            ElementalAlternateTraitSelectionDefinition(race,
                                slot, choices));
                }
            if (result.Count != SelectionCount ||
                result.Select(value => value.SelectionSymbol).Distinct(
                    StringComparer.Ordinal).Count() != SelectionCount ||
                result.Select(value => value.RetainMarkerSymbol).Distinct(
                    StringComparer.Ordinal).Count() != SelectionCount ||
                result.SelectMany(value => value.Choices).Select(value =>
                    value.Id).Distinct().Count() != TraitCount)
                throw new InvalidOperationException(
                    "Elemental alternate-trait selection inventory drifted.");
            return result.ToArray();
        }

        internal static IReadOnlyList<
            ElementalAlternateTraitSelectionDefinition> SelectionsForRace(
                ElementalHeritageRace race)
        {
            ValidateRace(race);
            return OrderedSelections().Where(value => value.Race == race)
                .ToArray();
        }

        internal static ElementalAlternateTraitDefinition Find(
            ElementalAlternateTraitId id)
        {
            ElementalAlternateTraitDefinition result = Ordered()
                .SingleOrDefault(value => value.Id == id);
            if (result == null)
                throw new InvalidOperationException(
                    "Unknown elemental alternate racial trait identity.");
            return result;
        }

        internal static bool IsLegal(ElementalHeritageRace race,
            IEnumerable<ElementalAlternateTraitId> activeTraits)
        {
            try
            {
                ValidateRace(race);
                Normalize(race, activeTraits);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        internal static ElementalAlternateTraitState Resolve(
            ElementalHeritageRace race, ElementalHeritageId heritageId,
            IEnumerable<ElementalAlternateTraitId> activeTraits)
        {
            ValidateRace(race);
            ElementalHeritageDefinition heritage = ElementalHeritagePolicy
                .Ordered().SingleOrDefault(value => value.Id == heritageId);
            if (heritage == null || heritage.ParentRace != race)
                throw new InvalidOperationException(
                    "The active heritage does not belong to the exact parent race.");
            ElementalAlternateTraitDefinition[] traits = Normalize(race,
                activeTraits);
            ElementalRacialTraitSlot consumed = traits.Aggregate(
                ElementalRacialTraitSlot.None,
                (current, value) => current | value.ReplacedSlots);

            bool keepResistance = (consumed &
                ElementalRacialTraitSlot.EnergyResistance) == 0;
            bool keepAffinity = (consumed &
                ElementalRacialTraitSlot.ElementalAffinity) == 0;
            bool keepSla = (consumed &
                ElementalRacialTraitSlot.RacialSpellLikeAbility) == 0;
            return new ElementalAlternateTraitState(race, heritage, traits,
                consumed,
                keepResistance ? ResistanceProvider(race) : null,
                keepAffinity ? heritage.AffinityFeatureSymbol : null,
                keepSla ? heritage.SlaFeatureSymbol : null,
                keepSla ? heritage.SlaResourceSymbol : null,
                keepSla ? heritage.SlaAbilitySymbol : null);
        }

        internal static ElementalAlternateTraitState ResolveMarkers(
            ElementalHeritageRace race, ElementalHeritageId heritageId,
            IEnumerable<string> markerSymbols)
        {
            string[] markers = markerSymbols == null
                ? new string[0]
                : markerSymbols.ToArray();
            ElementalAlternateTraitDefinition[] all = Ordered().ToArray();
            var ids = new List<ElementalAlternateTraitId>();
            foreach (string marker in markers)
            {
                ElementalAlternateTraitDefinition match = all.SingleOrDefault(
                    value => string.Equals(value.MarkerSymbol, marker,
                        StringComparison.Ordinal));
                if (match == null)
                    throw new InvalidOperationException(
                        "An unknown alternate-trait marker cannot be reconstructed.");
                ids.Add(match.Id);
            }
            return Resolve(race, heritageId, ids);
        }

        internal static ElementalAlternateTraitId[] TransitionMarkers(
            ElementalHeritageRace race,
            IEnumerable<ElementalAlternateTraitId> observedMarkers,
            ElementalAlternateTraitId? activating,
            ElementalAlternateTraitId? deactivating)
        {
            ValidateRace(race);
            ElementalAlternateTraitId[] observed = observedMarkers == null
                ? new ElementalAlternateTraitId[0]
                : observedMarkers.ToArray();
            if (observed.Distinct().Count() != observed.Length)
                throw new InvalidOperationException(
                    "Observed alternate-trait markers cannot contain duplicates.");
            var effective = new List<ElementalAlternateTraitId>();
            foreach (ElementalAlternateTraitId id in observed)
            {
                ElementalAlternateTraitDefinition definition = Find(id);
                if (definition.ParentRace != race)
                    throw new InvalidOperationException(
                        "An observed alternate-trait marker belongs to another race.");
                if (!deactivating.HasValue || id != deactivating.Value)
                    effective.Add(id);
            }
            if (activating.HasValue)
            {
                ElementalAlternateTraitDefinition next = Find(
                    activating.Value);
                if (next.ParentRace != race)
                    throw new InvalidOperationException(
                        "An activating alternate-trait marker belongs to another race.");
                effective.RemoveAll(id => Find(id).PrimarySlot ==
                    next.PrimarySlot);
                effective.Add(next.Id);
            }
            return Normalize(race, effective).Select(value => value.Id)
                .ToArray();
        }

        private static ElementalAlternateTraitDefinition[] Normalize(
            ElementalHeritageRace race,
            IEnumerable<ElementalAlternateTraitId> activeTraits)
        {
            ElementalAlternateTraitId[] ids = activeTraits == null
                ? new ElementalAlternateTraitId[0]
                : activeTraits.ToArray();
            if (ids.Distinct().Count() != ids.Length)
                throw new InvalidOperationException(
                    "An alternate racial trait cannot be selected twice.");
            ElementalAlternateTraitDefinition[] definitions = ids.Select(
                Find).OrderBy(value => (int)value.Id).ToArray();
            if (definitions.Any(value => value.ParentRace != race))
                throw new InvalidOperationException(
                    "An alternate racial trait does not belong to the exact parent race.");
            ElementalRacialTraitSlot consumed =
                ElementalRacialTraitSlot.None;
            foreach (ElementalAlternateTraitDefinition definition in
                definitions)
            {
                if ((consumed & definition.ReplacedSlots) != 0)
                    throw new InvalidOperationException(
                        "Two alternate racial traits cannot replace the same slot.");
                consumed |= definition.ReplacedSlots;
            }
            return definitions;
        }

        private static string ResistanceProvider(ElementalHeritageRace race)
        {
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    return "KMG.ElementalRaces.Ifrit.FireResistance";
                case ElementalHeritageRace.Oread:
                    return "KMG.ElementalRaces.Oread.AcidResistance";
                case ElementalHeritageRace.Sylph:
                    return "KMG.ElementalRaces.Sylph.ElectricityResistance";
                case ElementalHeritageRace.Undine:
                    return "KMG.ElementalRaces.Undine.ColdResistance";
                default:
                    throw new ArgumentOutOfRangeException("race");
            }
        }

        private static ElementalAlternateTraitDefinition T(
            ElementalAlternateTraitId id, ElementalHeritageRace race,
            string name, string description,
            ElementalRacialTraitSlot replacedSlots)
        {
            return new ElementalAlternateTraitDefinition(id, race, name,
                description, replacedSlots);
        }

        private static void Validate(
            ElementalAlternateTraitDefinition[] definitions)
        {
            if (definitions == null || definitions.Length != TraitCount ||
                definitions.Select(value => value.Id).Distinct().Count() !=
                    TraitCount ||
                definitions.Select(value => value.MarkerSymbol).Distinct(
                    StringComparer.Ordinal).Count() != TraitCount ||
                definitions.Select(value => value.ProviderSymbol).Distinct(
                    StringComparer.Ordinal).Count() != TraitCount ||
                definitions.Any(value => value.ReplacedSlots ==
                    ElementalRacialTraitSlot.None ||
                    (value.ReplacedSlots & ~AllSlots) != 0))
                throw new InvalidOperationException(
                    "Elemental alternate racial trait catalog drifted.");
            int[] expected = { 6, 5, 7, 3 };
            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
                if (definitions.Count(value => value.ParentRace == race) !=
                        expected[(int)race])
                    throw new InvalidOperationException(
                        race + " alternate racial trait inventory drifted.");
        }

        private static void ValidateRace(ElementalHeritageRace race)
        {
            if (!Enum.IsDefined(typeof(ElementalHeritageRace), race))
                throw new ArgumentOutOfRangeException("race");
        }
    }
}
