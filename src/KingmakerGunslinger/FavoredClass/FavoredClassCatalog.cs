using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>Charter section 2.3 catalog statuses.</summary>
    internal enum FavoredClassDisposition
    {
        /// <summary>I: faithful first-party implementation target.</summary>
        Implement,
        /// <summary>A: explicitly labeled CRPG adaptation.</summary>
        Adaptation,
        /// <summary>O: optional third-party target, profile OFF by default.</summary>
        OptionalThirdParty,
        /// <summary>P: provider-deferred; recipe retained, never published.</summary>
        ProviderDeferred,
        /// <summary>D: engineering-deferred; never published.</summary>
        EngineeringDeferred,
        /// <summary>X: excluded; no inert menu choice.</summary>
        Excluded,
        /// <summary>ALIAS: repeated appearance of another row.</summary>
        Alias
    }

    internal enum FavoredClassPublisher
    {
        Paizo,
        JonBrazerEnterprises
    }

    /// <summary>The configuration profile that governs a published row.</summary>
    internal enum FavoredClassProfile
    {
        None,
        FirstParty,
        Adaptation,
        ThirdParty
    }

    /// <summary>How a canonical effect chooses what it improves.</summary>
    internal enum FavoredClassTargetKind
    {
        /// <summary>One counter for the whole effect.</summary>
        None,
        /// <summary>One counter per canonical firearm type.</summary>
        FirearmType,
        /// <summary>One counter per supported bardic performance.</summary>
        Performance,
        /// <summary>One counter per owned supported oracle revelation.</summary>
        Revelation,
        /// <summary>One counter per owned eligible sorcerer bloodline power.</summary>
        BloodlinePower
    }

    /// <summary>
    /// One published source option (charter section 3). Rows are source
    /// options, not menu leaves or mechanics.
    /// </summary>
    internal sealed class FavoredClassSourceRow
    {
        internal FavoredClassSourceRow(string id, string sourceTable,
            string classFamily, string ancestry, FavoredClassPublisher publisher,
            string sources, string publishedBonus, FavoredClassDisposition disposition,
            string aliasOf, string boundary)
        {
            Id = id;
            SourceTable = sourceTable;
            ClassFamily = classFamily;
            Ancestry = ancestry;
            Publisher = publisher;
            Sources = sources;
            PublishedBonus = publishedBonus;
            Disposition = disposition;
            AliasOf = aliasOf;
            Boundary = boundary;
        }

        internal string Id { get; private set; }
        internal string SourceTable { get; private set; }
        internal string ClassFamily { get; private set; }
        internal string Ancestry { get; private set; }
        internal FavoredClassPublisher Publisher { get; private set; }
        internal string Sources { get; private set; }
        internal string PublishedBonus { get; private set; }
        internal FavoredClassDisposition Disposition { get; private set; }

        /// <summary>The row this appearance repeats; null unless an alias.</summary>
        internal string AliasOf { get; private set; }

        internal string Boundary { get; private set; }

        internal bool IsScheduled
        {
            get
            {
                return Disposition == FavoredClassDisposition.Implement ||
                    Disposition == FavoredClassDisposition.Adaptation ||
                    Disposition == FavoredClassDisposition.OptionalThirdParty;
            }
        }

        /// <summary>
        /// The configuration profile that must be enabled for this route to
        /// offer its effect. Profiles gate routes, not effects: Pistol-Whip is
        /// first-party through G05 and third-party through G20.
        /// </summary>
        internal FavoredClassProfile Profile
        {
            get
            {
                switch (Disposition)
                {
                    case FavoredClassDisposition.Implement:
                        return FavoredClassProfile.FirstParty;
                    case FavoredClassDisposition.Adaptation:
                        return FavoredClassProfile.Adaptation;
                    case FavoredClassDisposition.OptionalThirdParty:
                        return FavoredClassProfile.ThirdParty;
                    default:
                        return FavoredClassProfile.None;
                }
            }
        }
    }

    /// <summary>
    /// A canonical mechanical effect. Identical rewards reached through
    /// several source rows or ancestry routes share one effect and therefore
    /// one investment ledger per target (charter section 5.5).
    /// </summary>
    internal sealed class FavoredClassEffectSpec
    {
        internal FavoredClassEffectSpec(string id, string classFamily,
            FavoredClassRate rate, FavoredClassTargetKind targetKind,
            string[] rows, string summary, string omittedPortion)
        {
            Id = id;
            ClassFamily = classFamily;
            Rate = rate;
            TargetKind = targetKind;
            Rows = rows;
            Summary = summary;
            OmittedPortion = omittedPortion;
        }

        internal string Id { get; private set; }
        internal string ClassFamily { get; private set; }
        internal FavoredClassRate Rate { get; private set; }
        internal FavoredClassTargetKind TargetKind { get; private set; }

        /// <summary>Source rows (routes, including aliases) reaching this effect.</summary>
        internal string[] Rows { get; private set; }
        internal string Summary { get; private set; }

        /// <summary>
        /// For an adaptation, the tabletop portion deliberately not
        /// implemented; disclosed in the tooltip. Null for faithful effects.
        /// </summary>
        internal string OmittedPortion { get; private set; }
    }

    /// <summary>
    /// The reconciled Gunslinger Favored Class Integration Charter inventory:
    /// 53 distinct published options (46 Paizo, 7 Jon Brazer Enterprises)
    /// in 54 table appearances, and the canonical effects of the 30 scheduled
    /// rows. The Markdown coverage ledger and the machine-readable catalog
    /// are generated from, and tested against, this class.
    /// </summary>
    internal static class FavoredClassCatalog
    {
        internal const string Gunslinger = "gunslinger";
        internal const string Alchemist = "alchemist";
        internal const string Bard = "bard";
        internal const string Cleric = "cleric";
        internal const string Druid = "druid";
        internal const string Fighter = "fighter";
        internal const string Inquisitor = "inquisitor";
        internal const string Monk = "monk";
        internal const string Oracle = "oracle";
        internal const string Paladin = "paladin";
        internal const string Ranger = "ranger";
        internal const string Rogue = "rogue";
        internal const string Shifter = "shifter";
        internal const string Sorcerer = "sorcerer";
        internal const string Summoner = "summoner";
        internal const string Witch = "witch";
        internal const string Wizard = "wizard";

        internal const string EffectGrit = "gunslinger.grit";
        internal const string EffectMisfire = "gunslinger.misfire-by-firearm-type";
        internal const string EffectFirearmConfirmation = "gunslinger.firearm-confirmation";
        internal const string EffectPistolWhip = "gunslinger.pistol-whip-attack";
        internal const string EffectHalflingNimble = "gunslinger.nimble-halfling";
        internal const string EffectHalflingDodge = "gunslinger.dodge-halfling";
        internal const string EffectDrowNimble = "gunslinger.nimble-drow";
        internal const string EffectInitiative = "gunslinger.initiative-deed";
        internal const string EffectDirtyTrickTrip = "gunslinger.cmb-dirty-trick-trip";
        internal const string EffectBombDamage = "alchemist.bomb-damage";
        internal const string EffectFireIntimidate = "inquisitor.intimidate-fire-subtype";
        internal const string EffectSelectedRevelation = "oracle.selected-revelation";
        internal const string EffectDemoralize = "rogue.demoralize";
        internal const string EffectSelectedBloodlinePower = "sorcerer.selected-bloodline-power";
        internal const string EffectPerformanceRange = "bard.selected-performance-range";
        internal const string EffectBullRushDragDefense = "fighter.cmd-bull-rush-drag";
        internal const string EffectUnarmedConfirmation = "monk.unarmed-confirmation";
        internal const string EffectPaladinAuras = "paladin.aura-ally-bonus";
        internal const string EffectCompanionArmor = "ranger.companion-natural-armor";
        internal const string EffectEidolonArmor = "summoner.eidolon-natural-armor";
        internal const string EffectAquaticPenetration = "cleric.sr-penetration-aquatic-water";
        internal const string EffectGrappleStunning = "monk.grapple-cmd-and-stunning";

        private static readonly FavoredClassSourceRow[] RowArray = BuildRows();
        private static readonly FavoredClassEffectSpec[] EffectArray = BuildEffects();

        internal static IList<FavoredClassSourceRow> Rows
        {
            get { return Array.AsReadOnly(RowArray); }
        }

        internal static IList<FavoredClassEffectSpec> Effects
        {
            get { return Array.AsReadOnly(EffectArray); }
        }

        internal static FavoredClassSourceRow Row(string id)
        {
            FavoredClassSourceRow row = RowArray.FirstOrDefault(
                value => string.Equals(value.Id, id, StringComparison.Ordinal));
            if (row == null)
                throw new KeyNotFoundException("Unknown favored-class source row " + id);
            return row;
        }

        internal static FavoredClassEffectSpec Effect(string id)
        {
            FavoredClassEffectSpec effect = EffectArray.FirstOrDefault(
                value => string.Equals(value.Id, id, StringComparison.Ordinal));
            if (effect == null)
                throw new KeyNotFoundException("Unknown favored-class effect " + id);
            return effect;
        }

        /// <summary>Canonical effects reached through a row, in catalog order.</summary>
        internal static IList<FavoredClassEffectSpec> EffectsForRow(string rowId)
        {
            return EffectArray.Where(effect => effect.Rows.Contains(rowId,
                StringComparer.Ordinal)).ToList().AsReadOnly();
        }

        private static FavoredClassSourceRow R(string id, string table,
            string classFamily, string ancestry, string sources, string bonus,
            FavoredClassDisposition disposition, string boundary)
        {
            return new FavoredClassSourceRow(id, table, classFamily, ancestry,
                FavoredClassPublisher.Paizo, sources, bonus, disposition, null, boundary);
        }

        private static FavoredClassSourceRow J(string id, string ancestry,
            string bonus, FavoredClassDisposition disposition, string boundary)
        {
            return new FavoredClassSourceRow(id, "Gunslinger", Gunslinger, ancestry,
                FavoredClassPublisher.JonBrazerEnterprises, "R02 (JBE:SF:FCO)", bonus,
                disposition, null, boundary);
        }

        private static FavoredClassSourceRow[] BuildRows()
        {
            const FavoredClassDisposition I = FavoredClassDisposition.Implement;
            const FavoredClassDisposition A = FavoredClassDisposition.Adaptation;
            const FavoredClassDisposition O = FavoredClassDisposition.OptionalThirdParty;
            const FavoredClassDisposition P = FavoredClassDisposition.ProviderDeferred;
            const FavoredClassDisposition D = FavoredClassDisposition.EngineeringDeferred;
            const FavoredClassDisposition X = FavoredClassDisposition.Excluded;
            return new[]
            {
                // 3.1 Gunslinger first-party catalog (fifteen rows).
                R("G01", "Gunslinger", Gunslinger, "dwarf", "R01, R08",
                    "Chosen firearm type: misfire threshold -1/4; minimum 1.", I,
                    "Per-type counter; do not bind investment to one item."),
                R("G02", "Gunslinger", Gunslinger, "elf", "R01, R09",
                    "Firearm confirmation +1/3; cap +5; nonstacking with Critical Focus.", I,
                    "Confirmation only; preserve the better nonstacking bonus."),
                R("G03", "Gunslinger", Gunslinger, "gnome", "R01, R02",
                    "Gunsmithing repair of broken firearms: 5 minutes faster; cap 50 minutes.", D,
                    "The maintenance path operates on completed rests, not repair minutes. No free Quick Clear substitute."),
                R("G04", "Gunslinger", Gunslinger, "half-elf", "R01, R02",
                    "Grit pool +1/4.", I,
                    "Canonical alias of G07; one choice and one counter, not two."),
                R("G05", "Gunslinger", Gunslinger, "half-orc", "R01, R02",
                    "Pistol-Whip attack +1/3.", I,
                    "Apply to the deed attack, not firearm shots generally."),
                R("G06", "Gunslinger", Gunslinger, "halfling", "R01, R10",
                    "Choose Nimble AC +1/4 (cap +2), or Gunslinger's Dodge AC +1/4.", I,
                    "Two selectable branches, two counters; Dodge has no printed +2 cap."),
                R("G07", "Gunslinger", Gunslinger, "human", "R01, R02",
                    "Grit pool +1/4.", I,
                    "Shared with eligible half-elves and other authorized human aliases."),
                R("G08", "Gunslinger", Gunslinger, "goblin", "R01, R02",
                    "Firearm confirmation +1/3; cap +5; nonstacking with Critical Focus.", I,
                    "Reuse G02 mechanics; resolve optional race by verified identity."),
                R("G09", "Gunslinger", Gunslinger, "grippli", "R01, R02",
                    "Utility Shot and Dead Shot attacks +1/4.", P,
                    "Recipe retained; no qualified Grippli provider."),
                R("G10", "Gunslinger", Gunslinger, "hobgoblin", "R01, R02",
                    "Grit pool +1/4.", I,
                    "Reuse G07; publish only when the optional race is present."),
                R("G11", "Gunslinger", Gunslinger, "ifrit", "R03, R01",
                    "Gunslinger Initiative deed bonus +1/2.", I,
                    "Augment the deed's eligibility and bonus; I04 is this same option."),
                R("G12", "Gunslinger", Gunslinger, "kobold", "R01, R02",
                    "Nimble AC +1/4; cap +4.", P,
                    "Provider deferred; do not reuse Halfling's lower cap."),
                R("G13", "Gunslinger", Gunslinger, "ratfolk", "R01, R02",
                    "Initiative +1/2 while at least one grit point remains.", P,
                    "Provider deferred; the explicit grit condition differs from G11."),
                R("G14", "Gunslinger", Gunslinger, "fetchling", "R11, R01",
                    "Grit pool +1/4.", I,
                    "Official Blood of Shadows option; preferred over G19."),
                R("G15", "Gunslinger", Gunslinger, "kitsune", "R01",
                    "One-sixth of Magical Tail; available with any favored class.", D,
                    "Universal racial option, not Gunslinger-only; requires a separate Magical Tail provider integration."),

                // 3.2 Gunslinger third-party catalog (Jon Brazer Enterprises).
                J("G16", "dhampir",
                    "Firearm confirmation +1/3; cap +5; nonstacking with Critical Focus.", O, "Optional profile OFF by default. Reuse G02."),
                J("G17", "drow", "Nimble AC +1/6; cap +2.", O,
                    "Preserve its different fraction."),
                J("G18", "duergar",
                    "Chosen firearm type: misfire threshold -1/4; minimum 1.", O,
                    "Reuse G01, with its own eligibility."),
                J("G19", "fetchling",
                    "Firearm miss chance from dim light/darkness -1 percentage point; cap 10 points.", D,
                    "Lighting-specific concealment lacks a qualified path; never reduce all concealment."),
                J("G20", "orc", "Pistol-Whip attack +1/3.", O,
                    "Alias G05 where half-orc qualifies. A separate Orc race needs a verified provider."),
                J("G21", "tiefling", "CMB for dirty trick and trip +1/2.", O,
                    "Both maneuvers, not only firearm-delivered maneuvers."),
                J("G22", "wayang", "Pistol-Whip attack +1/4.", P,
                    "Third-party and provider deferred; not G05's +1/3 rate."),

                // 3.3 Ifrit.
                R("I01", "Ifrit", Alchemist, "ifrit", "R03, R12", "Bomb damage +1/2.", I,
                    "Use actual bomb lineage; this is damage, not daily bomb uses."),
                R("I02", "Ifrit", Bard, "ifrit", "R03, R12", "Fascinate target limit +1/6.", X,
                    "No qualified finite-target Fascinate selection model; do not add performance rounds instead."),
                R("I03", "Ifrit", Cleric, "ifrit", "R03, R12",
                    "Knowledge (planes) +1/2 for the Fire plane and fire-subtype creatures.", X,
                    "The narrow tabletop knowledge context is not a qualified CRPG check."),
                new FavoredClassSourceRow("I04", "Ifrit", Gunslinger, "ifrit",
                    FavoredClassPublisher.Paizo, "R03, R12", "Same published option as G11.",
                    FavoredClassDisposition.Alias, "G11",
                    "Catalog cross-reference only; never publish a duplicate."),
                R("I05", "Ifrit", Inquisitor, "ifrit", "R03, R12",
                    "Intimidate versus fire-subtype creatures +1/2, and Fire-plane Knowledge (planes) +1/2.", A,
                    "Keep target-qualified Intimidate only; explicitly omit planes knowledge."),
                R("I06", "Ifrit", Oracle, "ifrit", "R03, R12",
                    "Effects of one chosen revelation: effective Oracle level +1/6.", I,
                    "Advanced target-by-target adapters; no blanket Oracle-level increase."),
                R("I07", "Ifrit", Rogue, "ifrit", "R03, R12",
                    "Jump Acrobatics +1/2 and Intimidate to demoralize +1/2.", A,
                    "Keep demoralize only; explicitly omit jump checks."),
                R("I08", "Ifrit", Sorcerer, "ifrit", "R03, R07, R12",
                    "One usable fire-elemental or efreeti bloodline power: effective level +1/6; cap +2.", I,
                    "Use the +2 cap corroborated by AoN and legacy PRD, not d20PFSRD's +4."),

                // 3.4 Oread.
                R("O01", "Oread", Bard, "oread", "R04, R13",
                    "Chosen bardic performance range +5 feet; cap +30 feet per performance.", I,
                    "Separate target counters and owner-local ranges; advanced area/targeting work."),
                R("O02", "Oread", Cleric, "oread", "R04, R13",
                    "Knowledge (planes) +1/2 for the Earth plane and earth-subtype creatures.", X,
                    "No blanket Lore (Religion) or Lore (Nature) replacement."),
                R("O03", "Oread", Druid, "oread", "R04, R13",
                    "Knowledge (nature) +1/2 about plants and burrowing animals.", X,
                    "Subject-limited knowledge checks lack a qualified context."),
                R("O04", "Oread", Fighter, "oread", "R04, R13", "CMD against bull rush or drag +1.", A,
                    "Implement bull rush; publish drag only with an actual supported drag maneuver. Label any omission."),
                R("O05", "Oread", Monk, "oread", "R04, R13",
                    "Unarmed confirmation +1/3; cap +5; nonstacking with Critical Focus.", I, "Unarmed weapon identity, not all natural attacks."),
                R("O06", "Oread", Paladin, "oread", "R04, R13",
                    "Allies' Aura of Courage and Aura of Resolve bonuses +1/4.", I,
                    "Both auras, subject to unlock/replacement; no increase to self-immunities."),
                R("O07", "Oread", Ranger, "oread", "R04, R13",
                    "Animal companion natural armor +1/4; replacement inherits it.", I,
                    "Master-owned investment, qualified current companion."),
                R("O08", "Oread", Summoner, "oread", "R04, R13", "Eidolon natural armor +1/4.", I, "Reuse host pet-bonus pattern, not an extra summoner AC bonus."),

                // 3.5 Sylph.
                R("S01", "Sylph", Cleric, "sylph", "R05, R14",
                    "Knowledge (planes) +1/2 for the Air plane and air-subtype creatures.", X,
                    "No indiscriminate lore substitute."),
                R("S02", "Sylph", Druid, "sylph", "R05, R14",
                    "Knowledge (nature) +1/2 about weather and flying animals.", X,
                    "Narrow knowledge context is unavailable/unqualified."),
                R("S03", "Sylph", Inquisitor, "sylph", "R05, R14",
                    "Stealth while motionless +1/2 and opposed Perception +1/2.", D,
                    "Requires trustworthy movement and check-purpose contexts; not general Stealth/Perception."),
                R("S04", "Sylph", Oracle, "sylph", "R05, R14",
                    "Effects of one chosen revelation: effective Oracle level +1/6.", I,
                    "Share selected-revelation machinery with I06; normalize aliases."),
                R("S05", "Sylph", Rogue, "sylph", "R05, R14",
                    "Jump Acrobatics +1/2 and Sense Motive +1/2.", X,
                    "Neither maps faithfully to a general Mobility or Perception increase."),
                R("S06", "Sylph", Sorcerer, "sylph", "R05, R14",
                    "One usable djinni or air-elemental power: effective level +1/6; cap +2.", I,
                    "Whitelist the actual bloodline powers; not every air/electricity spell."),
                R("S07", "Sylph", Witch, "sylph", "R05, R14",
                    "Familiar's Stealth and Perception +1/2; replacement inherits them.", X,
                    "No qualified autonomous familiar skill actor; do not give the owner these skills."),
                R("S08", "Sylph", Wizard, "sylph", "R05, R14",
                    "One usable Air or Wood school power: effective level +1/6; cap +2.", D,
                    "Air/Wood elemental school provider and power semantics unqualified."),

                // 3.6 Undine.
                R("U01", "Undine", Bard, "undine", "R06, R15",
                    "Countersong Perform checks +1 against aquatic/water-subtype creatures.", X,
                    "Requires the tabletop Perform-check Countersong mechanic."),
                R("U02", "Undine", Cleric, "undine", "R06, R15",
                    "Spell-resistance penetration checks +1 against aquatic or water-subtype creatures.", I, "Target subtype and SR check only; not general caster level."),
                R("U03", "Undine", Druid, "undine", "R06, R15",
                    "Wild Empathy +1 against aquatic animals and magical beasts.", X,
                    "No qualified Wild Empathy encounter/check system."),
                R("U04", "Undine", Monk, "undine", "R06, R15",
                    "CMD against grapple +1 and stunning attacks/day +1/3.", I,
                    "Mixed-rate bundle: N grapple CMD and floor(N/3) extra stunning uses."),
                R("U05", "Undine", Sorcerer, "undine", "R06, R15",
                    "Caster-level checks to cast underwater +1.", X,
                    "No qualified underwater spellcasting check; not spell penetration."),
                R("U06", "Undine", Summoner, "undine", "R06, R15",
                    "Aquatic-base-form eidolon: Life Link range +5 feet.", D,
                    "A functioning range-limited Life Link and aquatic-base-form provider must first be proved."),
                R("U07", "Undine", Wizard, "undine", "R06, R15",
                    "One water-descriptor Cleric/Druid/Wizard spell in the book, below maximum spell level.", D,
                    "Personal off-list spell-level integration, precedence and host cadence need a separate design."),
                R("U08", "Undine", Shifter, "undine", "R06",
                    "Wild Empathy +1 against aquatic animals and magical beasts.", X,
                    "Wilderness Origins option; no qualified Wild Empathy system."),
            };
        }

        private static FavoredClassEffectSpec[] BuildEffects()
        {
            return new[]
            {
                E(EffectGrit, Gunslinger, 4, null, FavoredClassTargetKind.None,
                    new[] { "G04", "G07", "G10", "G14" },
                    "+1 maximum grit per four investments.", null),
                E(EffectMisfire, Gunslinger, 4, null, FavoredClassTargetKind.FirearmType,
                    new[] { "G01", "G18" },
                    "Chosen firearm type: misfire value -1 per four investments; never below 1.", null),
                E(EffectFirearmConfirmation, Gunslinger, 3, 5, FavoredClassTargetKind.None,
                    new[] { "G02", "G08", "G16" },
                    "+1 to confirm firearm critical hits per three investments (max +5); does not stack with Critical Focus.",
                    null),
                E(EffectPistolWhip, Gunslinger, 3, null, FavoredClassTargetKind.None,
                    new[] { "G05", "G20" },
                    "+1 on the Pistol-Whip deed attack roll per three investments.", null),
                E(EffectHalflingNimble, Gunslinger, 4, 2, FavoredClassTargetKind.None,
                    new[] { "G06" },
                    "+1 Nimble dodge AC per four investments (max +2), under Nimble's own conditions.", null),
                E(EffectHalflingDodge, Gunslinger, 4, null, FavoredClassTargetKind.None,
                    new[] { "G06" },
                    "+1 to the Gunslinger's Dodge deed AC bonus per four investments.", null),
                E(EffectDrowNimble, Gunslinger, 6, 2, FavoredClassTargetKind.None,
                    new[] { "G17" },
                    "+1 Nimble dodge AC per six investments (max +2), under Nimble's own conditions.", null),
                E(EffectInitiative, Gunslinger, 2, null, FavoredClassTargetKind.None,
                    new[] { "G11", "I04" },
                    "+1 to the Gunslinger Initiative deed bonus per two investments.", null),
                E(EffectDirtyTrickTrip, Gunslinger, 2, null, FavoredClassTargetKind.None,
                    new[] { "G21" },
                    "+1 CMB for dirty trick and trip per two investments.", null),
                E(EffectBombDamage, Alchemist, 2, null, FavoredClassTargetKind.None,
                    new[] { "I01" },
                    "+1 bomb damage per two investments (flat, not per die, not extra bombs).", null),
                E(EffectFireIntimidate, Inquisitor, 2, null, FavoredClassTargetKind.None,
                    new[] { "I05" },
                    "+1 on Intimidate checks against fire-subtype creatures per two investments.",
                    "Fire-plane Knowledge (planes) is omitted: no qualified CRPG knowledge check exists."),
                E(EffectSelectedRevelation, Oracle, 6, null, FavoredClassTargetKind.Revelation,
                    new[] { "I06", "S04" },
                    "One chosen owned revelation: +1 effective oracle level for its effects per six investments.",
                    null),
                E(EffectDemoralize, Rogue, 2, null, FavoredClassTargetKind.None,
                    new[] { "I07" },
                    "+1 on Intimidate checks made to demoralize per two investments.",
                    "Jump-specific Acrobatics is omitted: Kingmaker has no qualified jump check."),
                E(EffectSelectedBloodlinePower, Sorcerer, 6, 2, FavoredClassTargetKind.BloodlinePower,
                    new[] { "I08", "S06" },
                    "One chosen usable eligible bloodline power: +1 effective sorcerer level per six investments (max +2).",
                    null),
                E(EffectPerformanceRange, Bard, 1, 6, FavoredClassTargetKind.Performance,
                    new[] { "O01" },
                    "One chosen bardic performance: +5 feet range per investment (max +30 feet per performance).",
                    null),
                E(EffectBullRushDragDefense, Fighter, 1, null, FavoredClassTargetKind.None,
                    new[] { "O04" },
                    "+1 CMD against bull rush per investment.",
                    "Drag is published only if Kingmaker implements an actual drag maneuver."),
                E(EffectUnarmedConfirmation, Monk, 3, 5, FavoredClassTargetKind.None,
                    new[] { "O05" },
                    "+1 to confirm unarmed-strike critical hits per three investments (max +5); does not stack with Critical Focus.",
                    null),
                E(EffectPaladinAuras, Paladin, 4, null, FavoredClassTargetKind.None,
                    new[] { "O06" },
                    "+1 to the ally save bonuses of Aura of Courage and Aura of Resolve per four investments.",
                    null),
                E(EffectCompanionArmor, Ranger, 4, null, FavoredClassTargetKind.None,
                    new[] { "O07" },
                    "+1 natural armor for the current animal companion per four investments.", null),
                E(EffectEidolonArmor, Summoner, 4, null, FavoredClassTargetKind.None,
                    new[] { "O08" },
                    "+1 natural armor for the eidolon per four investments.", null),
                E(EffectAquaticPenetration, Cleric, 1, null, FavoredClassTargetKind.None,
                    new[] { "U02" },
                    "+1 on spell-resistance checks against aquatic or water-subtype creatures per investment.",
                    null),
                E(EffectGrappleStunning, Monk, 3, null, FavoredClassTargetKind.None,
                    new[] { "U04" },
                    "+1 CMD against grapple per investment and +1 stunning attack per day per three investments.",
                    null),
            };
        }

        private static FavoredClassEffectSpec E(string id, string classFamily,
            int divisor, int? capSteps, FavoredClassTargetKind targetKind,
            string[] rows, string summary, string omittedPortion)
        {
            return new FavoredClassEffectSpec(id, classFamily,
                new FavoredClassRate(divisor, capSteps), targetKind, rows,
                summary, omittedPortion);
        }
    }
}
