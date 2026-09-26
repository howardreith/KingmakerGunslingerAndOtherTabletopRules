using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// One published menu leaf: the full (step-completing) or partial
    /// (investment-only) half of one canonical effect's target counter.
    /// </summary>
    internal sealed class FavoredClassLeafSpec
    {
        internal FavoredClassLeafSpec(string symbol, string effectId, string targetKey,
            FavoredClassInvestmentRole role, int ranks, string name, string description)
        {
            Symbol = symbol;
            EffectId = effectId;
            TargetKey = targetKey;
            Role = role;
            Ranks = ranks;
            Name = name;
            Description = description;
        }

        internal string Symbol { get; private set; }
        internal string EffectId { get; private set; }

        /// <summary>Null for an effect with one counter.</summary>
        internal string TargetKey { get; private set; }

        internal FavoredClassInvestmentRole Role { get; private set; }

        /// <summary>
        /// The leaf's rank capacity: floor(T/d) for the full leaf and
        /// T - floor(T/d) for the partial leaf.
        /// </summary>
        internal int Ranks { get; private set; }

        internal string Name { get; private set; }
        internal string Description { get; private set; }
    }

    /// <summary>Player-facing text and symbol stem of one implemented effect.</summary>
    internal sealed class FavoredClassLeafFamily
    {
        internal FavoredClassLeafFamily(string effectId, string symbolKey, string title,
            string stepText, string conditions)
            : this(effectId, symbolKey, title, stepText, conditions, null)
        {
        }

        internal FavoredClassLeafFamily(string effectId, string symbolKey, string title,
            string stepText, string conditions, string immediateText)
        {
            EffectId = effectId;
            SymbolKey = symbolKey;
            Title = title;
            StepText = stepText;
            Conditions = conditions;
            ImmediateText = immediateText;
        }

        internal string EffectId { get; private set; }
        internal string SymbolKey { get; private set; }
        internal string Title { get; private set; }

        /// <summary>One whole benefit step; null when each target states its own.</summary>
        internal string StepText { get; private set; }

        /// <summary>
        /// When the earned bonus applies, including a dormant investment made
        /// before the improved feature is gained; null when unconditional.
        /// </summary>
        internal string Conditions { get; private set; }

        /// <summary>
        /// For a mixed-rate bundle, the portion every investment (full or
        /// partial) grants at once; null when a partial pick grants nothing.
        /// </summary>
        internal string ImmediateText { get; private set; }
    }

    /// <summary>One canonical target of a targeted effect (its own counter).</summary>
    internal sealed class FavoredClassTargetSpec
    {
        internal FavoredClassTargetSpec(string key, string title, string stepText)
            : this(key, title, stepText, null)
        {
        }

        /// <param name="rowIds">
        /// The source rows that open this target (null: every row of the
        /// effect), e.g. fire powers only through the Ifrit row.
        /// </param>
        internal FavoredClassTargetSpec(string key, string title, string stepText, string[] rowIds)
            : this(key, title, stepText, rowIds, null)
        {
        }

        /// <param name="note">A target-specific tooltip sentence (null when none).</param>
        internal FavoredClassTargetSpec(string key, string title, string stepText, string[] rowIds, string note)
        {
            Key = key;
            Title = title;
            StepText = stepText;
            RowIds = rowIds;
            Note = note;
        }

        /// <summary>Stable symbol segment; for firearms, the FirearmKind name.</summary>
        internal string Key { get; private set; }
        internal string Title { get; private set; }
        internal string StepText { get; private set; }
        internal string[] RowIds { get; private set; }
        internal string Note { get; private set; }
    }

    /// <summary>
    /// Stable menu-leaf identities and player-facing text. Every symbol here
    /// has a committed identity-manifest entry; nothing is generated at
    /// runtime. Partial leaves disclose that they are investment toward a
    /// later whole benefit rather than a benefit of their own.
    /// </summary>
    internal static class FavoredClassLeafCatalog
    {
        internal const string SymbolPrefix = "KMG.FavoredClass.";

        private static readonly FavoredClassLeafFamily[] Families =
        {
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectGrit, "Gunslinger.Grit",
                "Grit", "+1 maximum grit", null),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectMisfire, "Gunslinger.Misfire",
                "Misfire", null,
                "The reduction applies to every firearm of the chosen type, including replacements, and never lowers a misfire value below 1; a value another rule already reduced to 0 stays 0."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectFirearmConfirmation,
                "Gunslinger.FirearmConfirmation", "Firearm Critical Confirmation",
                "+1 on rolls to confirm critical hits with firearm attacks",
                "It does not stack with Critical Focus: only the amount by which this bonus exceeds Critical Focus's bonus is added. It never changes threat range, critical multipliers or ordinary attack rolls."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectPistolWhip, "Gunslinger.PistolWhip",
                "Pistol-Whip", "+1 on the Pistol-Whip attack roll",
                "Pistol-Whip is a 3rd-level deed; earlier investments take effect when it is gained. It does not affect firearm shots, damage or the trip attempt."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectHalflingNimble,
                "Gunslinger.HalflingNimble", "Nimble", "+1 Nimble dodge bonus to AC",
                "The bonus follows Nimble's own conditions (light or no armor; lost whenever the Dexterity bonus to AC is lost). Nimble is gained at Gunslinger level 2; earlier investments take effect then. Not available to a Mysterious Stranger, whose archetype replaces Nimble."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectHalflingDodge,
                "Gunslinger.HalflingDodge", "Gunslinger's Dodge",
                "+1 to the dodge bonus granted by Gunslinger's Dodge",
                "It applies only while Gunslinger's Dodge grants its own bonus. Not available to a Musket Master, whose archetype replaces Gunslinger's Dodge."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectDrowNimble,
                "Gunslinger.DrowNimble", "Nimble (Drow)", "+1 Nimble dodge bonus to AC",
                "The bonus follows Nimble's own conditions (light or no armor; lost whenever the Dexterity bonus to AC is lost). Nimble is gained at Gunslinger level 2; earlier investments take effect then. Not available to a Mysterious Stranger, whose archetype replaces Nimble."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectInitiative,
                "Gunslinger.Initiative", "Gunslinger Initiative",
                "+1 to the Gunslinger Initiative bonus",
                "It applies only while Gunslinger Initiative applies (at least 1 grit, or its True Grit option). Gunslinger Initiative is a 3rd-level deed; earlier investments take effect when it is gained."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectDirtyTrickTrip,
                "Gunslinger.DirtyTrickTrip", "Dirty Trick and Trip",
                "+1 CMB for dirty trick and trip combat maneuvers",
                "It applies to every dirty trick and trip attempt, not only firearm-delivered maneuvers."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectBombDamage,
                "Alchemist.BombDamage", "Bomb Damage", "+1 damage with bombs",
                "It is added once to each damage roll of a bomb, where the bomb's own Intelligence bonus applies (direct hit and splash), never per die or to lingering acid or explosive follow-up damage. Not available to archetypes that replace bombs (Vivisectionist; Call of the Wild's Toxicant)."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectFireIntimidate,
                "Inquisitor.FireIntimidate", "Intimidate (Fire Creatures)",
                "+1 on Intimidate checks against creatures of the fire subtype",
                "It applies only to Intimidate checks made against a specific creature that has the fire subtype, such as demoralizing it; dialogue checks and other creatures are unaffected."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectDemoralize,
                "Rogue.Demoralize", "Demoralize",
                "+1 on Intimidate checks made to demoralize",
                "It applies only to the Intimidate check of a demoralize action; other Intimidate checks are unaffected."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectBullRushDragDefense,
                "Fighter.BullRushDefense", "Bull Rush Defense", "+1 CMD against bull rush",
                "It applies only when this character is the target of a bull rush."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectUnarmedConfirmation,
                "Monk.UnarmedConfirmation", "Unarmed Critical Confirmation",
                "+1 on rolls to confirm critical hits with unarmed strikes",
                "It does not stack with Critical Focus: only the amount by which this bonus exceeds Critical Focus's bonus is added. Natural attacks and weapons are not unarmed strikes, and threat range and multipliers never change."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectAquaticPenetration,
                "Cleric.AquaticPenetration", "Spell Penetration (Water Creatures)",
                "+1 on checks to overcome the spell resistance of aquatic or water-subtype creatures",
                "It changes nothing else: caster level, spell DCs and checks against other creatures are unaffected."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectGrappleStunning,
                "Monk.GrappleStunning", "Grapple Defense and Stunning Fist",
                "+1 additional Stunning Fist attempt per day",
                "Stunning Fist attempts are added only while this character has Stunning Fist (an archetype that replaces it, such as Call of the Wild's Zen Archer, gains none); grabs that make no combat maneuver check are unaffected.",
                "+1 CMD against grapple"),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectPaladinAuras,
                "Paladin.AuraAllyBonus", "Aura Bonus for Allies",
                "+1 to the saving throw bonus your Aura of Courage and Aura of Resolve grant allies",
                "It is part of the aura's own morale bonus, applies only while that aura applies (Courage from 3rd level, Resolve from 8th), and never changes the aura's range, your own immunities or other saves; where two paladins' auras overlap, the game still applies only the higher morale bonus. Not available to an archetype that replaces both auras, such as Divine Hunter."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectCompanionArmor,
                "Ranger.CompanionNaturalArmor", "Animal Companion Armor",
                "+1 natural armor bonus for your animal companion",
                "It moves to a replacement companion, stacks with the companion's own natural armor and with Barkskin, and never changes your own AC. Not available to an archetype that replaces Hunter's Bond, such as Flamewarden, Freebooter or Stormwalker."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectEidolonArmor,
                "Summoner.EidolonNaturalArmor", "Eidolon Armor",
                "+1 natural armor bonus for your eidolon",
                "It applies to your current eidolon, stacks with its own natural armor and never changes your own AC. The Summoner is provided by Call of the Wild."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectSelectedBloodlinePower,
                "Sorcerer.BloodlinePower", "Bloodline Power", null,
                "Only the chosen power's own level-based values change: Elemental Ray's damage bonus; Elemental Blast's damage dice, save DC, caster level checks and the extra daily uses it gains at 17th and 20th level; and Elemental Resistance's step from 10 to 20 at 9th level. You must already have the power. It never grants a power early, and never changes other powers, spells, spell slots, other caster level checks, BAB or saves. The efreeti and djinni bloodlines do not exist in this game."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectSelectedRevelation,
                "Oracle.Revelation", "Revelation", null,
                "Every value the chosen revelation computes from oracle level (damage dice, durations, bonuses and their level steps, uses per day, save DCs and caster level) uses your oracle level plus the earned steps, and so do the abilities and forms the revelation itself gains at later oracle levels. You must already have the revelation. It never grants another revelation or revelation choice, never satisfies a level prerequisite, and never changes other revelations, spells, spell slots, BAB or saves. The Oracle is provided by Call of the Wild."),
            new FavoredClassLeafFamily(FavoredClassCatalog.EffectPerformanceRange,
                "Bard.PerformanceRange", "Performance Range", null,
                "It widens only your own area of the chosen performance, from the next time you start it and when a save is loaded; that area's ring and your performance's description show your range, and its other rules are unchanged. You must already have the performance. One-shot, personal and masterpiece performances, Discordant Voice, Storm Call (its text and area disagree) and Mockery (it names a single target) are not choices."),
        };

        /// <summary>
        /// O01 performance targets: every persistent performance area the
        /// read-only audit found (FavoredClassPerformanceManifest).
        /// </summary>
        private static readonly FavoredClassTargetSpec[] PerformanceTargets = FavoredClassPerformanceManifest.All
            .Select(target => new FavoredClassTargetSpec(target.Key, target.Title,
                "+5 feet to the radius of " + target.Title + " (" + target.BaseFeet.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) + " feet natively)", null,
                target.Published
                    ? (target.Provider ? "This performance is provided by Call of the Wild." : null)
                    : "Not offered: " + target.Exclusion + "."))
            .ToArray();

        /// <summary>
        /// I06/S04 revelation targets: every revelation the read-only audit of
        /// the installed provider marked implementable (the generated
        /// FavoredClassRevelationManifest; docs/FAVORED-CLASS-TARGET-MANIFEST.md).
        /// </summary>
        private static readonly FavoredClassTargetSpec[] RevelationTargets = FavoredClassRevelationManifest.All
            .Select(target => new FavoredClassTargetSpec(target.Key, RevelationTitle(target),
                "+1 effective oracle level for " + RevelationTitle(target), null,
                target.Published ? null : "Not offered: " + target.ExcludedReason + "."))
            .ToArray();

        /// <summary>
        /// I08/S06 bloodline power targets with an implemented level-scaled
        /// effect: fire powers through the Ifrit row, air powers through the
        /// Sylph row (manifest in docs/FAVORED-CLASS-TARGET-MANIFEST.md).
        /// </summary>
        private static readonly FavoredClassTargetSpec[] BloodlinePowerTargets =
        {
            new FavoredClassTargetSpec("FireRay", "Elemental Ray (Fire)",
                "+1 effective sorcerer level for Elemental Ray (Fire)", new[] { "I08" }),
            new FavoredClassTargetSpec("FireBlast", "Elemental Blast (Fire)",
                "+1 effective sorcerer level for Elemental Blast (Fire)", new[] { "I08" }),
            new FavoredClassTargetSpec("AirRay", "Elemental Ray (Air)",
                "+1 effective sorcerer level for Elemental Ray (Air)", new[] { "S06" }),
            new FavoredClassTargetSpec("AirBlast", "Elemental Blast (Air)",
                "+1 effective sorcerer level for Elemental Blast (Air)", new[] { "S06" }),
            new FavoredClassTargetSpec("FireResistance", "Elemental Resistance (Fire)",
                "+1 effective sorcerer level for Elemental Resistance (Fire)", new[] { "I08" }),
            new FavoredClassTargetSpec("AirResistance", "Elemental Resistance (Air)",
                "+1 effective sorcerer level for Elemental Resistance (Air)", new[] { "S06" }),
        };

        /// <summary>
        /// I08/S06 eligible bloodlines (charter 8.10): exactly the fire and air
        /// elemental bloodlines and their proven aliases, Call of the Wild's
        /// Seeker and Crossblooded copies of the same bloodline. Call of the
        /// Wild's Primal copies are a differently named bloodline with other
        /// powers, and no efreeti or djinni bloodline exists in the game.
        /// </summary>
        private static readonly string[] FireElementalBloodlines =
        {
            "17cc794d47408bc4986c55265475c06f", // BloodlineElementalFireProgression
            "3950cf0cafa5c6ba1d1fc840d2682837", // SeekerBloodlineElementalFireProgression
            "ae4f8d4d7f23c49929b4f4b88757524c", // CrossbloodedBloodlineElementalFireProgression
        };

        private static readonly string[] AirElementalBloodlines =
        {
            "cd788df497c6f10439c7025e87864ee4", // BloodlineElementalAirProgression
            "e3e43bb57f23bc7abcb49f38019ba6bc", // SeekerBloodlineElementalAirProgression
            "74fb79f4afa5be59881fa3c054a4dcc7", // CrossbloodedBloodlineElementalAirProgression
        };

        /// <summary>The bloodline identities that make a power target eligible, and their name.</summary>
        internal static KeyValuePair<string, string[]> EligibleBloodlines(string targetKey)
        {
            if (targetKey == "FireRay" || targetKey == "FireBlast" || targetKey == "FireResistance")
                return new KeyValuePair<string, string[]>("the fire elemental bloodline",
                    (string[])FireElementalBloodlines.Clone());
            if (targetKey == "AirRay" || targetKey == "AirBlast" || targetKey == "AirResistance")
                return new KeyValuePair<string, string[]>("the air elemental bloodline",
                    (string[])AirElementalBloodlines.Clone());
            throw new KeyNotFoundException("No bloodline power target " + targetKey);
        }

        /// <summary>
        /// The mod's canonical player-facing firearm types. Legacy Rifle and
        /// Revolver identities stay readable for saves but are not targets.
        /// </summary>
        private static readonly FavoredClassTargetSpec[] FirearmTargets =
        {
            new FavoredClassTargetSpec("Pistol", "Pistol", "-1 misfire value with pistols"),
            new FavoredClassTargetSpec("Musket", "Musket", "-1 misfire value with muskets"),
            new FavoredClassTargetSpec("Blunderbuss", "Blunderbuss",
                "-1 misfire value with blunderbusses"),
        };

        /// <summary>Effects whose leaves this candidate registers, in registration order.</summary>
        internal static IList<string> ImplementedEffects
        {
            get { return Families.Select(family => family.EffectId).ToList().AsReadOnly(); }
        }

        internal static bool IsImplemented(string effectId)
        {
            return Families.Any(family => string.Equals(family.EffectId, effectId,
                StringComparison.Ordinal));
        }

        internal static FavoredClassLeafFamily Family(string effectId)
        {
            FavoredClassLeafFamily family = Families.FirstOrDefault(value =>
                string.Equals(value.EffectId, effectId, StringComparison.Ordinal));
            if (family == null)
                throw new KeyNotFoundException("No registered leaves for effect " + effectId);
            return family;
        }

        /// <summary>The source rows that open a target (all of the effect's rows when unrestricted).</summary>
        internal static IList<string> TargetRows(string effectId, string targetKey)
        {
            FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
            FavoredClassTargetSpec target = Targets(effect).FirstOrDefault(value =>
                value != null && string.Equals(value.Key, targetKey, StringComparison.Ordinal));
            return (target == null || target.RowIds == null ? effect.Rows.ToArray() : target.RowIds)
                .ToList().AsReadOnly();
        }

        /// <summary>The player-facing title of a target (its leaf names carry it).</summary>
        internal static string TargetTitle(string effectId, string targetKey)
        {
            FavoredClassTargetSpec target = Targets(FavoredClassCatalog.Effect(effectId)).FirstOrDefault(value =>
                value != null && string.Equals(value.Key, targetKey, StringComparison.Ordinal));
            return target == null ? null : target.Title;
        }

        /// <summary>Target keys of an effect; a single null key for an untargeted effect.</summary>
        internal static IList<string> TargetKeys(string effectId)
        {
            return Targets(FavoredClassCatalog.Effect(effectId))
                .Select(target => target == null ? null : target.Key).ToList().AsReadOnly();
        }

        internal static IList<FavoredClassLeafSpec> LeavesFor(string effectId)
        {
            FavoredClassLeafFamily family = Family(effectId);
            FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
            var leaves = new List<FavoredClassLeafSpec>();
            foreach (FavoredClassTargetSpec target in Targets(effect))
            {
                if (effect.Rate.HasPartial)
                    leaves.Add(Leaf(effect, family, target, FavoredClassInvestmentRole.Partial));
                leaves.Add(Leaf(effect, family, target, FavoredClassInvestmentRole.Full));
            }
            return leaves.AsReadOnly();
        }

        internal static IList<FavoredClassLeafSpec> AllLeaves()
        {
            return ImplementedEffects.SelectMany(LeavesFor).ToList().AsReadOnly();
        }

        internal static string Symbol(string symbolKey, string targetKey, FavoredClassInvestmentRole role)
        {
            return SymbolPrefix + symbolKey + (targetKey == null ? string.Empty : "." + targetKey) +
                (role == FavoredClassInvestmentRole.Full ? ".Full" : ".Partial");
        }

        private static IList<FavoredClassTargetSpec> Targets(FavoredClassEffectSpec effect)
        {
            switch (effect.TargetKind)
            {
                case FavoredClassTargetKind.None:
                    return new FavoredClassTargetSpec[] { null };
                case FavoredClassTargetKind.FirearmType:
                    return FirearmTargets;
                case FavoredClassTargetKind.BloodlinePower:
                    return BloodlinePowerTargets;
                case FavoredClassTargetKind.Revelation:
                    return RevelationTargets;
                case FavoredClassTargetKind.Performance:
                    return PerformanceTargets;
                default:
                    throw new InvalidOperationException(effect.Id +
                        " needs a qualified target manifest before it can be published.");
            }
        }

        private static FavoredClassLeafSpec Leaf(FavoredClassEffectSpec effect,
            FavoredClassLeafFamily family, FavoredClassTargetSpec target,
            FavoredClassInvestmentRole role)
        {
            FavoredClassRate rate = effect.Rate;
            int ranks = role == FavoredClassInvestmentRole.Full
                ? FavoredClassRankPolicy.FullCapacity(rate)
                : FavoredClassRankPolicy.PartialCapacity(rate);
            string title = target == null ? family.Title : family.Title + ": " + target.Title;
            string name = "Favored Class: " + title +
                (role == FavoredClassInvestmentRole.Partial ? " (partial)" : string.Empty);
            return new FavoredClassLeafSpec(
                Symbol(family.SymbolKey, target == null ? null : target.Key, role), effect.Id,
                target == null ? null : target.Key, role, ranks, name,
                Describe(effect, family, target, role));
        }

        /// <summary>Tooltip text: routes, rate, cap and what this pick does.</summary>
        internal static string Describe(FavoredClassEffectSpec effect, FavoredClassInvestmentRole role)
        {
            FavoredClassLeafFamily family = Family(effect.Id);
            FavoredClassTargetSpec target = Targets(effect).FirstOrDefault();
            return Describe(effect, family, target, role);
        }

        private static string Describe(FavoredClassEffectSpec effect, FavoredClassLeafFamily family,
            FavoredClassTargetSpec target, FavoredClassInvestmentRole role)
        {
            FavoredClassRate rate = effect.Rate;
            string step = target == null ? family.StepText : target.StepText;
            string routes = RouteText(effect, target == null ? null : target.RowIds);
            string cap = rate.CapSteps.HasValue
                ? string.Format(CultureInfo.InvariantCulture,
                    " The bonus is limited to {0} steps; this choice closes when the limit is reached.",
                    rate.CapSteps.Value)
                : string.Empty;
            string counter = target == null ? string.Empty : CounterText(effect.TargetKind);
            string pick;
            if (!rate.HasPartial)
                pick = "Each selection grants " + step + ".";
            else if (role == FavoredClassInvestmentRole.Full && family.ImmediateText != null)
                pick = string.Format(CultureInfo.InvariantCulture,
                    "This selection grants {0} at once and completes {1} investments, granting {2}. The number of completed steps equals this feature's rank.",
                    family.ImmediateText, rate.Divisor, step);
            else if (role == FavoredClassInvestmentRole.Full)
                pick = string.Format(CultureInfo.InvariantCulture,
                    "This selection completes {0} investments and grants {1}. The number of completed steps equals this feature's rank.",
                    rate.Divisor, step);
            else if (family.ImmediateText != null)
                pick = string.Format(CultureInfo.InvariantCulture,
                    "This selection grants {0} at once and is one investment toward the next {1}; every {2} investments complete that step. Its rank counts every partial investment made.",
                    family.ImmediateText, step, OrdinalWord(rate.Divisor));
            else
                pick = string.Format(CultureInfo.InvariantCulture,
                    "This selection is one investment toward the next {0}; every {1} investments complete a step. A partial investment grants nothing by itself, and its rank counts every partial investment made.",
                    step, OrdinalWord(rate.Divisor));
            string conditions = family.Conditions == null ? string.Empty : " " + family.Conditions;
            string note = target == null || target.Note == null ? string.Empty : " " + target.Note;
            string adaptation = effect.OmittedPortion == null
                ? string.Empty
                : " CRPG adaptation: " + effect.OmittedPortion;
            return "Favored class bonus (" + routes + "): " + effect.Summary + " " + pick + counter +
                cap + conditions + note + adaptation;
        }

        private static string CounterText(FavoredClassTargetKind kind)
        {
            switch (kind)
            {
                case FavoredClassTargetKind.FirearmType:
                    return " Each firearm type keeps its own separate count of investments.";
                case FavoredClassTargetKind.Revelation:
                    return " Each revelation keeps its own separate count of investments.";
                case FavoredClassTargetKind.Performance:
                    return " Each performance keeps its own separate count of investments.";
                default:
                    return " Each bloodline power keeps its own separate count of investments.";
            }
        }

        /// <summary>A revelation's title, with its mystery where two mysteries share the name.</summary>
        private static string RevelationTitle(FavoredClassRevelationTarget target)
        {
            bool shared = FavoredClassRevelationManifest.All.Count(value =>
                string.Equals(value.Title, target.Title, StringComparison.Ordinal)) > 1;
            return shared ? target.Title + " (" + target.Mystery + ")" : target.Title;
        }

        private static string RouteText(FavoredClassEffectSpec effect)
        {
            return RouteText(effect, null);
        }

        private static string RouteText(FavoredClassEffectSpec effect, string[] targetRows)
        {
            IEnumerable<FavoredClassSourceRow> rows = (targetRows ?? effect.Rows.ToArray())
                .Select(FavoredClassCatalog.Row)
                .Where(row => row.IsScheduled);
            List<string> parts = new List<string>();
            foreach (FavoredClassSourceRow row in rows)
            {
                string ancestry = char.ToUpperInvariant(row.Ancestry[0]) + row.Ancestry.Substring(1);
                parts.Add(row.Publisher == FavoredClassPublisher.JonBrazerEnterprises
                    ? ancestry + " (Jon Brazer Enterprises)"
                    : ancestry);
            }
            return string.Join(", ", parts.ToArray());
        }

        private static string OrdinalWord(int divisor)
        {
            switch (divisor)
            {
                case 2: return "two";
                case 3: return "three";
                case 4: return "four";
                case 6: return "six";
                default: return divisor.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
