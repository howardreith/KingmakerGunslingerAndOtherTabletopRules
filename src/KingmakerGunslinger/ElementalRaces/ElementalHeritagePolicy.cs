using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    internal enum ElementalHeritageRace
    {
        Ifrit = 0, Oread = 1, Sylph = 2, Undine = 3
    }

    internal enum ElementalHeritageId
    {
        GeneralIfrit = 0, Lavasoul = 1, Sunsoul = 2,
        GeneralOread = 3, Gemsoul = 4, Ironsoul = 5,
        GeneralSylph = 6, Smokesoul = 7, Stormsoul = 8,
        GeneralUndine = 9, Mistsoul = 10, Rimesoul = 11
    }

    internal enum ElementalHeritageStat
    {
        Strength = 0, Dexterity = 1, Constitution = 2,
        Intelligence = 3, Wisdom = 4, Charisma = 5
    }

    internal enum ElementalHeritageAffinity
    {
        Fire = 0, Acid = 1, Electricity = 2, Cold = 3
    }

    internal enum ElementalHeritageAbilityImplementation
    {
        NativeSpellClone = 0, HydraulicPush = 1,
        UnerringWeapon = 2, ChillTouch = 3
    }

    internal sealed class ElementalHeritageStatModifier
    {
        internal ElementalHeritageStatModifier(ElementalHeritageStat stat,
            int value)
        {
            if (value == 0 || value < -4 || value > 4 || value % 2 != 0)
                throw new ArgumentOutOfRangeException("value");
            Stat = stat;
            Value = value;
        }

        internal ElementalHeritageStat Stat { get; private set; }
        internal int Value { get; private set; }
    }

    internal sealed class ElementalHeritageDefinition
    {
        internal ElementalHeritageDefinition(ElementalHeritageRace parentRace,
            ElementalHeritageId id, bool isGeneral, string name,
            string description, string selectionSymbol, string markerSymbol,
            string affinityName, string affinityDescription,
            string affinityFeatureSymbol, ElementalHeritageAffinity affinity,
            string slaName, string slaDescription, string slaFeatureSymbol,
            string slaResourceSymbol, string slaAbilitySymbol,
            string donorAbilityGuid, int spellLevel,
            ElementalHeritageAbilityImplementation abilityImplementation,
            params ElementalHeritageStatModifier[] stats)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(selectionSymbol) ||
                string.IsNullOrWhiteSpace(markerSymbol) ||
                string.IsNullOrWhiteSpace(affinityName) ||
                string.IsNullOrWhiteSpace(affinityDescription) ||
                string.IsNullOrWhiteSpace(affinityFeatureSymbol) ||
                string.IsNullOrWhiteSpace(slaName) ||
                string.IsNullOrWhiteSpace(slaDescription) ||
                string.IsNullOrWhiteSpace(slaFeatureSymbol) ||
                string.IsNullOrWhiteSpace(slaResourceSymbol) ||
                string.IsNullOrWhiteSpace(slaAbilitySymbol) ||
                spellLevel < 1 || stats == null || stats.Length != 3 ||
                stats.Select(entry => entry.Stat).Distinct().Count() != 3 ||
                stats.Any(entry => Math.Abs(entry.Value) != 2))
                throw new ArgumentException(
                    "An elemental heritage definition is incomplete.");
            bool native = abilityImplementation ==
                ElementalHeritageAbilityImplementation.NativeSpellClone;
            if (native == string.IsNullOrWhiteSpace(donorAbilityGuid))
                throw new ArgumentException(
                    native ? "A native heritage SLA requires an exact donor GUID." :
                    "A project-owned heritage SLA must not name a donor GUID.");

            ParentRace = parentRace;
            Id = id;
            IsGeneral = isGeneral;
            Name = name;
            Description = description;
            SelectionSymbol = selectionSymbol;
            MarkerSymbol = markerSymbol;
            AffinityName = affinityName;
            AffinityDescription = affinityDescription;
            AffinityFeatureSymbol = affinityFeatureSymbol;
            Affinity = affinity;
            SlaName = slaName;
            SlaDescription = slaDescription;
            SlaFeatureSymbol = slaFeatureSymbol;
            SlaResourceSymbol = slaResourceSymbol;
            SlaAbilitySymbol = slaAbilitySymbol;
            DonorAbilityGuid = donorAbilityGuid;
            SpellLevel = spellLevel;
            AbilityImplementation = abilityImplementation;
            Stats = (ElementalHeritageStatModifier[])stats.Clone();
        }

        internal ElementalHeritageRace ParentRace { get; private set; }
        internal ElementalHeritageId Id { get; private set; }
        internal bool IsGeneral { get; private set; }
        internal string Name { get; private set; }
        internal string Description { get; private set; }
        internal string SelectionSymbol { get; private set; }
        internal string MarkerSymbol { get; private set; }
        internal string AffinityName { get; private set; }
        internal string AffinityDescription { get; private set; }
        internal string AffinityFeatureSymbol { get; private set; }
        internal ElementalHeritageAffinity Affinity { get; private set; }
        internal string SlaName { get; private set; }
        internal string SlaDescription { get; private set; }
        internal string SlaFeatureSymbol { get; private set; }
        internal string SlaResourceSymbol { get; private set; }
        internal string SlaAbilitySymbol { get; private set; }
        internal string DonorAbilityGuid { get; private set; }
        internal int SpellLevel { get; private set; }
        internal ElementalHeritageAbilityImplementation AbilityImplementation
        { get; private set; }
        internal ElementalHeritageStatModifier[] Stats { get; private set; }

        internal int ModifierFor(ElementalHeritageStat stat)
        {
            ElementalHeritageStatModifier match = Stats.SingleOrDefault(
                entry => entry.Stat == stat);
            return match == null ? 0 : match.Value;
        }
    }

    internal static class ElementalHeritagePolicy
    {
        internal const int HeritageCount = 12;
        internal const int ChoicesPerRace = 3;

        internal static IReadOnlyList<ElementalHeritageDefinition> Ordered()
        {
            ElementalHeritageDefinition[] result = new[]
            {
                H(ElementalHeritageRace.Ifrit,
                    ElementalHeritageId.GeneralIfrit, true, "General Ifrit",
                    "+2 Dexterity, +2 Charisma, -2 Wisdom; Fire Affinity; Burning Hands once per day.",
                    "Fire Affinity",
                    "You gain +1 DC with actual Fire spells cast from a spellbook. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Ifrit.FireAffinity",
                    "Burning Hands",
                    "Once per day, unleash Burning Hands as a Charisma-based spell-like ability. Caster level equals total character level.",
                    "KMG.ElementalRaces.Ifrit.BurningHandsFeature",
                    "KMG.ElementalRaces.Ifrit.BurningHandsResource",
                    "KMG.ElementalRaces.Ifrit.BurningHandsAbility",
                    "4783c3709a74a794dbe7c8e7e0b1b038", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Dexterity, 2),
                    S(ElementalHeritageStat.Charisma, 2),
                    S(ElementalHeritageStat.Wisdom, -2)),
                H(ElementalHeritageRace.Ifrit,
                    ElementalHeritageId.Lavasoul, false, "Lavasoul",
                    "+2 Constitution, +2 Intelligence, -2 Dexterity; Magma Affinity; Firebelly once per day. Firebelly is Owlcat's practical substitute for Burning Sands.",
                    "Magma Affinity",
                    "You gain +1 DC with actual Fire spells cast from a spellbook. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Ifrit.Lavasoul.MagmaAffinity",
                    "Firebelly",
                    "Once per day, use Firebelly as a Charisma-based spell-like ability. Caster level equals total character level. This is Owlcat's established substitute for Burning Sands.",
                    "KMG.ElementalRaces.Ifrit.Lavasoul.FirebellyFeature",
                    "KMG.ElementalRaces.Ifrit.Lavasoul.FirebellyResource",
                    "KMG.ElementalRaces.Ifrit.Lavasoul.FirebellyAbility",
                    "b065231094a21d14dbf1c3832f776871", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Constitution, 2),
                    S(ElementalHeritageStat.Intelligence, 2),
                    S(ElementalHeritageStat.Dexterity, -2)),
                H(ElementalHeritageRace.Ifrit,
                    ElementalHeritageId.Sunsoul, false, "Sunsoul",
                    "+2 Strength, +2 Charisma, -2 Wisdom; Solar Affinity; Flare Burst once per day. Flare Burst is Owlcat's practical substitute for Sun Metal.",
                    "Solar Affinity",
                    "You gain +1 DC with actual Fire spells cast from a spellbook. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Ifrit.Sunsoul.SolarAffinity",
                    "Flare Burst",
                    "Once per day, use Flare Burst as a Charisma-based spell-like ability. Caster level equals total character level. This is Owlcat's established substitute for Sun Metal.",
                    "KMG.ElementalRaces.Ifrit.Sunsoul.FlareBurstFeature",
                    "KMG.ElementalRaces.Ifrit.Sunsoul.FlareBurstResource",
                    "KMG.ElementalRaces.Ifrit.Sunsoul.FlareBurstAbility",
                    "39a602aa80cc96f4597778b6d4d49c0a", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Strength, 2),
                    S(ElementalHeritageStat.Charisma, 2),
                    S(ElementalHeritageStat.Wisdom, -2)),
                H(ElementalHeritageRace.Oread,
                    ElementalHeritageId.GeneralOread, true, "General Oread",
                    "+2 Strength, +2 Wisdom, -2 Charisma; Earth Affinity; Stone Fist once per day.",
                    "Earth Affinity",
                    "You gain +1 DC with actual Acid spells cast from a spellbook under Kingmaker's earth-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Oread.AcidAffinity",
                    "Stone Fist",
                    "Once per day, use Stone Fist as a Charisma-based spell-like ability. Caster level equals total character level.",
                    "KMG.ElementalRaces.Oread.StoneFistFeature",
                    "KMG.ElementalRaces.Oread.StoneFistResource",
                    "KMG.ElementalRaces.Oread.StoneFistAbility",
                    "85067a04a97416949b5d1dbf986d93f3", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Strength, 2),
                    S(ElementalHeritageStat.Wisdom, 2),
                    S(ElementalHeritageStat.Charisma, -2)),
                H(ElementalHeritageRace.Oread,
                    ElementalHeritageId.Gemsoul, false, "Gemsoul",
                    "+2 Strength, +2 Charisma, -2 Wisdom; Crystal Affinity; Color Spray once per day.",
                    "Crystal Affinity",
                    "You gain +1 DC with actual Acid spells cast from a spellbook under Kingmaker's earth-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Oread.Gemsoul.CrystalAffinity",
                    "Color Spray",
                    "Once per day, use Color Spray as a Charisma-based spell-like ability. Caster level equals total character level.",
                    "KMG.ElementalRaces.Oread.Gemsoul.ColorSprayFeature",
                    "KMG.ElementalRaces.Oread.Gemsoul.ColorSprayResource",
                    "KMG.ElementalRaces.Oread.Gemsoul.ColorSprayAbility",
                    "91da41b9793a4624797921f221db653c", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Strength, 2),
                    S(ElementalHeritageStat.Charisma, 2),
                    S(ElementalHeritageStat.Wisdom, -2)),
                H(ElementalHeritageRace.Oread,
                    ElementalHeritageId.Ironsoul, false, "Ironsoul",
                    "+2 Constitution, +2 Wisdom, -2 Dexterity; Metal Affinity; Unerring Weapon once per day.",
                    "Metal Affinity",
                    "You gain +1 DC with actual Acid spells cast from a spellbook under Kingmaker's earth-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Oread.Ironsoul.MetalAffinity",
                    "Unerring Weapon",
                    "Once per day, empower one held manufactured weapon for 1 round per character level. Its critical-confirmation bonus is +2, plus +1 per four caster levels, to a maximum of +7.",
                    "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponFeature",
                    "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponResource",
                    "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponAbility",
                    null, 1,
                    ElementalHeritageAbilityImplementation.UnerringWeapon,
                    S(ElementalHeritageStat.Constitution, 2),
                    S(ElementalHeritageStat.Wisdom, 2),
                    S(ElementalHeritageStat.Dexterity, -2)),
                H(ElementalHeritageRace.Sylph,
                    ElementalHeritageId.GeneralSylph, true, "General Sylph",
                    "+2 Dexterity, +2 Intelligence, -2 Constitution; Air Affinity; Feather Step once per day.",
                    "Air Affinity",
                    "You gain +1 DC with actual Electricity spells cast from a spellbook under Kingmaker's air-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Sylph.AirAffinity",
                    "Feather Step",
                    "Once per day, use Feather Step as a Charisma-based spell-like ability. Caster level equals total character level. This is Kingmaker's practical substitute for Feather Fall.",
                    "KMG.ElementalRaces.Sylph.FeatherStepFeature",
                    "KMG.ElementalRaces.Sylph.FeatherStepResource",
                    "KMG.ElementalRaces.Sylph.FeatherStepAbility",
                    "f3c0b267dd17a2a45a40805e31fe3cd1", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Dexterity, 2),
                    S(ElementalHeritageStat.Intelligence, 2),
                    S(ElementalHeritageStat.Constitution, -2)),
                H(ElementalHeritageRace.Sylph,
                    ElementalHeritageId.Smokesoul, false, "Smokesoul",
                    "+2 Dexterity, +2 Charisma, -2 Constitution; Smoke Affinity; Expeditious Retreat once per day. Expeditious Retreat is Owlcat's practical substitute for Blurred Movement.",
                    "Smoke Affinity",
                    "You gain +1 DC with actual Electricity spells cast from a spellbook under Kingmaker's air-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Sylph.Smokesoul.SmokeAffinity",
                    "Expeditious Retreat",
                    "Once per day, use Expeditious Retreat as a Charisma-based spell-like ability. Caster level equals total character level. This is Owlcat's established substitute for Blurred Movement.",
                    "KMG.ElementalRaces.Sylph.Smokesoul.ExpeditiousRetreatFeature",
                    "KMG.ElementalRaces.Sylph.Smokesoul.ExpeditiousRetreatResource",
                    "KMG.ElementalRaces.Sylph.Smokesoul.ExpeditiousRetreatAbility",
                    "4f8181e7a7f1d904fbaea64220e83379", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Dexterity, 2),
                    S(ElementalHeritageStat.Charisma, 2),
                    S(ElementalHeritageStat.Constitution, -2)),
                H(ElementalHeritageRace.Sylph,
                    ElementalHeritageId.Stormsoul, false, "Stormsoul",
                    "+2 Dexterity, +2 Charisma, -2 Wisdom; Lightning Affinity; Shocking Grasp once per day.",
                    "Lightning Affinity",
                    "You gain +1 DC with actual Electricity spells cast from a spellbook under Kingmaker's air-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Sylph.Stormsoul.LightningAffinity",
                    "Shocking Grasp",
                    "Once per day, use Shocking Grasp as a Charisma-based spell-like ability. Caster level equals total character level.",
                    "KMG.ElementalRaces.Sylph.Stormsoul.ShockingGraspFeature",
                    "KMG.ElementalRaces.Sylph.Stormsoul.ShockingGraspResource",
                    "KMG.ElementalRaces.Sylph.Stormsoul.ShockingGraspAbility",
                    "ab395d2335d3f384e99dddee8562978f", 1,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Dexterity, 2),
                    S(ElementalHeritageStat.Charisma, 2),
                    S(ElementalHeritageStat.Wisdom, -2)),
                H(ElementalHeritageRace.Undine,
                    ElementalHeritageId.GeneralUndine, true, "General Undine",
                    "+2 Dexterity, +2 Wisdom, -2 Strength; Water Affinity; Hydraulic Push once per day.",
                    "Water Affinity",
                    "You gain +1 DC with actual Cold spells cast from a spellbook under Kingmaker's water-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Undine.WaterAffinity",
                    "Hydraulic Push",
                    "Once per day, Bull Rush one creature at range using total character level plus the best current Intelligence, Wisdom, or Charisma modifier.",
                    "KMG.ElementalRaces.Undine.HydraulicPushFeature",
                    "KMG.ElementalRaces.Undine.HydraulicPushResource",
                    "KMG.ElementalRaces.Undine.HydraulicPushAbility",
                    null, 1,
                    ElementalHeritageAbilityImplementation.HydraulicPush,
                    S(ElementalHeritageStat.Dexterity, 2),
                    S(ElementalHeritageStat.Wisdom, 2),
                    S(ElementalHeritageStat.Strength, -2)),
                H(ElementalHeritageRace.Undine,
                    ElementalHeritageId.Mistsoul, false, "Mistsoul",
                    "+2 Constitution, +2 Wisdom, -2 Intelligence; Mist Affinity; Blur once per day. Blur is Owlcat's practical substitute for Obscuring Mist.",
                    "Mist Affinity",
                    "You gain +1 DC with actual Cold spells cast from a spellbook under Kingmaker's water-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Undine.Mistsoul.MistAffinity",
                    "Blur",
                    "Once per day, use Blur as a Charisma-based spell-like ability. Caster level equals total character level. This is Owlcat's established substitute for Obscuring Mist.",
                    "KMG.ElementalRaces.Undine.Mistsoul.BlurFeature",
                    "KMG.ElementalRaces.Undine.Mistsoul.BlurResource",
                    "KMG.ElementalRaces.Undine.Mistsoul.BlurAbility",
                    "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2,
                    ElementalHeritageAbilityImplementation.NativeSpellClone,
                    S(ElementalHeritageStat.Constitution, 2),
                    S(ElementalHeritageStat.Wisdom, 2),
                    S(ElementalHeritageStat.Intelligence, -2)),
                H(ElementalHeritageRace.Undine,
                    ElementalHeritageId.Rimesoul, false, "Rimesoul",
                    "+2 Dexterity, +2 Intelligence, -2 Charisma; Ice Affinity; Chill Touch once per day.",
                    "Ice Affinity",
                    "You gain +1 DC with actual Cold spells cast from a spellbook under Kingmaker's water-affinity adaptation. This never improves spell-like or other nonspell abilities.",
                    "KMG.ElementalRaces.Undine.Rimesoul.IceAffinity",
                    "Chill Touch",
                    "Once per day, make chilling melee touch attacks. You gain one touch per character level; each touch follows Chill Touch's saving-throw and creature rules.",
                    "KMG.ElementalRaces.Undine.Rimesoul.ChillTouchFeature",
                    "KMG.ElementalRaces.Undine.Rimesoul.ChillTouchResource",
                    "KMG.ElementalRaces.Undine.Rimesoul.ChillTouchAbility",
                    null, 1,
                    ElementalHeritageAbilityImplementation.ChillTouch,
                    S(ElementalHeritageStat.Dexterity, 2),
                    S(ElementalHeritageStat.Intelligence, 2),
                    S(ElementalHeritageStat.Charisma, -2))
            };
            Validate(result);
            return result;
        }

        internal static IReadOnlyList<ElementalHeritageDefinition> ForRace(
            ElementalHeritageRace race)
        {
            ElementalHeritageDefinition[] result = Ordered()
                .Where(entry => entry.ParentRace == race).ToArray();
            if (result.Length != ChoicesPerRace)
                throw new InvalidOperationException(
                    "Every elemental race must expose exactly three heritages.");
            return result;
        }

        internal static ElementalHeritageDefinition General(
            ElementalHeritageRace race)
        {
            return ForRace(race).Single(entry => entry.IsGeneral);
        }

        internal static ElementalHeritageDefinition Resolve(
            ElementalHeritageRace race,
            IEnumerable<ElementalHeritageId> activeMarkers)
        {
            ElementalHeritageId[] markers = activeMarkers == null
                ? new ElementalHeritageId[0]
                : activeMarkers.ToArray();
            if (markers.Length == 0)
                return General(race);
            if (markers.Length != 1)
                throw new InvalidOperationException(
                    "An elemental character cannot have multiple heritage markers.");
            ElementalHeritageDefinition result = Ordered().SingleOrDefault(
                entry => entry.Id == markers[0]);
            if (result == null || result.ParentRace != race)
                throw new InvalidOperationException(
                    "The active heritage marker does not belong to the parent race.");
            return result;
        }

        internal static IReadOnlyList<ElementalHeritageStatModifier> NetDeltas(
            ElementalHeritageDefinition heritage)
        {
            if (heritage == null)
                throw new ArgumentNullException("heritage");
            ElementalHeritageDefinition general = General(heritage.ParentRace);
            var result = new List<ElementalHeritageStatModifier>();
            foreach (ElementalHeritageStat stat in Enum.GetValues(
                typeof(ElementalHeritageStat)))
            {
                int difference = heritage.ModifierFor(stat) -
                    general.ModifierFor(stat);
                if (difference != 0)
                    result.Add(new ElementalHeritageStatModifier(stat,
                        difference));
            }
            return result.ToArray();
        }

        private static ElementalHeritageDefinition H(
            ElementalHeritageRace race, ElementalHeritageId id,
            bool isGeneral, string name, string description,
            string affinityName, string affinityDescription,
            string affinityFeatureSymbol, string slaName,
            string slaDescription, string slaFeatureSymbol,
            string slaResourceSymbol, string slaAbilitySymbol,
            string donorAbilityGuid, int spellLevel,
            ElementalHeritageAbilityImplementation implementation,
            params ElementalHeritageStatModifier[] stats)
        {
            string raceName = race.ToString();
            return new ElementalHeritageDefinition(race, id, isGeneral,
                name, description,
                "KMG.ElementalRaces." + raceName + ".HeritageSelection",
                "KMG.ElementalRaces." + raceName + ".Heritage." +
                    (isGeneral ? "General" : id.ToString()),
                affinityName, affinityDescription, affinityFeatureSymbol,
                AffinityFor(race), slaName, slaDescription,
                slaFeatureSymbol, slaResourceSymbol, slaAbilitySymbol,
                donorAbilityGuid, spellLevel, implementation, stats);
        }

        private static ElementalHeritageStatModifier S(
            ElementalHeritageStat stat, int value)
        {
            return new ElementalHeritageStatModifier(stat, value);
        }

        private static ElementalHeritageAffinity AffinityFor(
            ElementalHeritageRace race)
        {
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    return ElementalHeritageAffinity.Fire;
                case ElementalHeritageRace.Oread:
                    return ElementalHeritageAffinity.Acid;
                case ElementalHeritageRace.Sylph:
                    return ElementalHeritageAffinity.Electricity;
                case ElementalHeritageRace.Undine:
                    return ElementalHeritageAffinity.Cold;
                default:
                    throw new ArgumentOutOfRangeException("race");
            }
        }

        private static void Validate(ElementalHeritageDefinition[] entries)
        {
            if (entries == null || entries.Length != HeritageCount ||
                entries.Select(entry => entry.Id).Distinct().Count() !=
                    HeritageCount ||
                entries.Select(entry => entry.MarkerSymbol).Distinct(
                    StringComparer.Ordinal).Count() != HeritageCount ||
                entries.Select(entry => entry.AffinityFeatureSymbol).Distinct(
                    StringComparer.Ordinal).Count() != HeritageCount ||
                entries.Select(entry => entry.SlaFeatureSymbol).Distinct(
                    StringComparer.Ordinal).Count() != HeritageCount ||
                entries.Select(entry => entry.SlaResourceSymbol).Distinct(
                    StringComparer.Ordinal).Count() != HeritageCount ||
                entries.Select(entry => entry.SlaAbilitySymbol).Distinct(
                    StringComparer.Ordinal).Count() != HeritageCount)
                throw new InvalidOperationException(
                    "Elemental heritage catalog identity count drifted.");
            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
            {
                ElementalHeritageDefinition[] choices = entries.Where(
                    entry => entry.ParentRace == race).ToArray();
                if (choices.Length != ChoicesPerRace ||
                    choices.Count(entry => entry.IsGeneral) != 1 ||
                    choices.Select(entry => entry.SelectionSymbol).Distinct(
                        StringComparer.Ordinal).Count() != 1)
                    throw new InvalidOperationException(
                        "Every elemental race needs one selection with exactly three choices and one General option.");
            }
        }
    }
}
