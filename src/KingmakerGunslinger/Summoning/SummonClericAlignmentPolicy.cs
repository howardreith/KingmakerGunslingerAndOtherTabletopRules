using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Bit values match Kingmaker's Alignment enum (Good 2, Evil 4, Law 8, Chaos 16).</summary>
    [Flags]
    internal enum SummonAlignmentAxes { None = 0, Good = 2, Evil = 4, Law = 8, Chaos = 16 }

    /// <summary>A summon choice's effective alignment descriptors for one spell.</summary>
    internal sealed class SummonSpellAlignment
    {
        private SummonSpellAlignment(bool casterDependent, SummonAlignmentAxes descriptors, string authority)
        { CasterDependent = casterDependent; Descriptors = descriptors; Authority = authority; }

        internal bool CasterDependent { get; private set; }
        internal SummonAlignmentAxes Descriptors { get; private set; }
        internal string Authority { get; private set; }

        internal SummonAlignmentAxes EffectiveFor(SummonAlignmentAxes caster)
        { return CasterDependent ? caster : Descriptors; }

        internal static SummonSpellAlignment Fixed(SummonAlignmentAxes descriptors, string authority)
        { return new SummonSpellAlignment(false, descriptors, authority); }

        /// <summary>Summon Monster's starred creatures always match the caster,
        /// so the spell takes the caster's alignment, whichever template appears.</summary>
        internal static readonly SummonSpellAlignment CasterMatched = new SummonSpellAlignment(true,
            SummonAlignmentAxes.None, "Summon Monster table: starred creatures match the caster's alignment.");
    }

    /// <summary>What the policy needs to know about one caster, read from
    /// real class data and facts. A cleric spellbook alone is not membership.</summary>
    internal sealed class SummonClericFacts
    {
        internal SummonClericFacts(IEnumerable<string> classGuids, int alignmentValue,
            IEnumerable<string> deitySelectionFeatureGuids)
        {
            ClassGuids = (classGuids ?? Enumerable.Empty<string>()).ToArray();
            AlignmentValue = alignmentValue;
            DeitySelectionFeatureGuids = (deitySelectionFeatureGuids ?? Enumerable.Empty<string>()).ToArray();
        }

        /// <summary>Classes in which the caster has at least one level.</summary>
        internal IReadOnlyList<string> ClassGuids { get; private set; }
        internal int AlignmentValue { get; private set; }
        /// <summary>The caster's facts that are options of the deity selection.</summary>
        internal IReadOnlyList<string> DeitySelectionFeatureGuids { get; private set; }
    }

    internal enum SummonClericDecision { NotCleric, Allowed, ForbiddenByAlignment, ForbiddenUnknownDeity }

    /// <summary>
    /// A cleric can't cast spells of an alignment opposed to their own or their
    /// deity's. Neutral contributes no prohibition, and a deity adds to the
    /// caster's restriction. A summoning spell takes the alignment subtype of
    /// the creature it summons.
    /// </summary>
    internal static class SummonClericAlignmentPolicy
    {
        internal const string ClericClassGuid = "67819271767a9dd4fbfd4ae700befea0";
        internal const string DeitySelectionGuid = "59e7a76987fe3b547b9cce045f4db3e4";
        private const SummonAlignmentAxes AxesMask = SummonAlignmentAxes.Good | SummonAlignmentAxes.Evil |
            SummonAlignmentAxes.Law | SummonAlignmentAxes.Chaos;

        internal static SummonAlignmentAxes AxesOf(int alignmentValue)
        { return (SummonAlignmentAxes)alignmentValue & AxesMask; }

        internal static SummonAlignmentAxes Opposites(SummonAlignmentAxes axes)
        {
            SummonAlignmentAxes result = SummonAlignmentAxes.None;
            if ((axes & SummonAlignmentAxes.Good) != 0) result |= SummonAlignmentAxes.Evil;
            if ((axes & SummonAlignmentAxes.Evil) != 0) result |= SummonAlignmentAxes.Good;
            if ((axes & SummonAlignmentAxes.Law) != 0) result |= SummonAlignmentAxes.Chaos;
            if ((axes & SummonAlignmentAxes.Chaos) != 0) result |= SummonAlignmentAxes.Law;
            return result;
        }

        internal static bool IsCleric(SummonClericFacts facts)
        { return facts.ClassGuids.Any(value => string.Equals(value, ClericClassGuid, StringComparison.Ordinal)); }

        internal static SummonClericDecision Decide(SummonClericFacts facts, SummonSpellAlignment spell)
        {
            if (facts == null || spell == null) throw new ArgumentNullException();
            if (!IsCleric(facts)) return SummonClericDecision.NotCleric;
            SummonAlignmentAxes caster = AxesOf(facts.AlignmentValue);
            SummonAlignmentAxes deity = SummonAlignmentAxes.None;
            bool unknownDeity = false;
            foreach (string feature in facts.DeitySelectionFeatureGuids)
            {
                SummonAlignmentAxes axes;
                if (SummonClericDeityCatalog.TryGetAxes(feature, out axes)) deity |= axes;
                else unknownDeity = true;
            }
            SummonAlignmentAxes effective = spell.EffectiveFor(caster);
            if ((effective & (Opposites(caster) | Opposites(deity))) != 0)
                return SummonClericDecision.ForbiddenByAlignment;
            // An unidentified deity must not read as neutral. A fixed alignment
            // spell cannot be cleared; a starred one matches the caster.
            if (unknownDeity && !spell.CasterDependent && spell.Descriptors != SummonAlignmentAxes.None)
                return SummonClericDecision.ForbiddenUnknownDeity;
            return SummonClericDecision.Allowed;
        }

        internal static bool Permits(SummonClericDecision decision)
        { return decision == SummonClericDecision.NotCleric || decision == SummonClericDecision.Allowed; }

        /// <summary>Keeps permitted rows in their existing order, as a new list.</summary>
        internal static IReadOnlyList<T> RetainPermitted<T>(IEnumerable<T> rows, Func<T, bool> permitted)
        {
            if (rows == null || permitted == null) throw new ArgumentNullException();
            return rows.Where(permitted).ToArray();
        }
    }

    /// <summary>Exact installed deity features, read from the game's assets.
    /// Alignments are the printed Pathfinder deity alignments (aonprd.com).</summary>
    internal static class SummonClericDeityCatalog
    {
        internal const string AtheismGuid = "92c0d2da0a836ce418a267093c09ca54";
        private const SummonAlignmentAxes G = SummonAlignmentAxes.Good, E = SummonAlignmentAxes.Evil,
            L = SummonAlignmentAxes.Law, C = SummonAlignmentAxes.Chaos, N = SummonAlignmentAxes.None;

        private static readonly Dictionary<string, SummonAlignmentAxes> Values =
            new Dictionary<string, SummonAlignmentAxes>(StringComparer.Ordinal) {
                { "6122dacf418611540a3c91e67197ee4e", L },     // Abadar
                { "a3a5ccc9c670e6f4ca4a686d23b89900", L | E }, // Asmodeus
                { "c7531715a3f046d4da129619be63f44c", C },     // Calistria
                { "300e212868bca984687c92bcb66d381b", C | G }, // Cayden Cailean
                { "2c0a3b9971327ba4d9d85354d16998c1", C | G }, // Desna
                { "afc775188deb7a44aa4cbde03512c671", L | G }, // Erastil
                { "8f49a5d8528a82c44b8c117a89f6b68c", C },     // Gorum
                { "4af983eec2d821b40a3065eb5e8c3a72", N },     // Gozreh
                { "88d5da04361b16746bf5b65795e0c38c", L | G }, // Iomedae
                { "23a77a5985de08349820429ce1b5a234", L },     // Irori
                { "02c890ca226e4bdc815afb7b64d01450", C | E }, // Lamashtu (added by Call of the Wild)
                { "6262cfce7c31626458325ca0909de997", N },     // Nethys
                { "805b6bdc8c96f4749afc687a003f9628", E },     // Norgorber
                { "458750bc214ab2e44abdeae404ab22e9", N },     // Pharasma
                { "04bc2b62273ab744092d992ed72bff41", C | E }, // Rovagug
                { "c1c4f7f64842e7e48849e5e67be11a1b", G },     // Sarenrae
                { "b382afa31e4287644b77a8b30ed4aa0b", G },     // Shelyn
                { "d2d5c5a58885a6b489727467e13c3337", L | G }, // Torag
                { "812f6c07148088e41a9ac94b56ac2fc8", E },     // Urgathoa
                { "f7eed400baa66a744ad361d4df0e6f1b", L | E }, // Zon-Kuthon
                { AtheismGuid, N },
            };

        internal static IEnumerable<string> Guids { get { return Values.Keys; } }

        internal static bool TryGetAxes(string featureGuid, out SummonAlignmentAxes axes)
        { return Values.TryGetValue(featureGuid ?? "", out axes); }
    }

    /// <summary>
    /// Every Summon Monster choice's alignment classification. Unstarred rows
    /// are explicit; nothing is inferred from names, models or donors.
    /// </summary>
    internal static class SummonClericAlignmentCatalog
    {
        private const SummonAlignmentAxes G = SummonAlignmentAxes.Good, E = SummonAlignmentAxes.Evil,
            L = SummonAlignmentAxes.Law, C = SummonAlignmentAxes.Chaos, N = SummonAlignmentAxes.None;
        private const string Table = "Summon Monster table subtype";

        private static SummonSpellAlignment T(SummonAlignmentAxes axes) { return SummonSpellAlignment.Fixed(axes, Table); }
        private static SummonSpellAlignment B(SummonAlignmentAxes axes, string line)
        { return SummonSpellAlignment.Fixed(axes, "Bestiary type line: " + line); }

        private static readonly Dictionary<string, SummonSpellAlignment> Unstarred =
            new Dictionary<string, SummonSpellAlignment>(StringComparer.Ordinal) {
                { "lemure", T(E | L) }, { "dretch", T(C | E) }, { "lantern-archon", T(G | L) },
                { "hell-hound", T(E | L) }, { "hound-archon", T(G | L) },
                { "babau", T(C | E) }, { "bearded-devil", T(E | L) }, { "bralani-azata", T(C | G) },
                { "kyton-chain-devil", T(E | L) }, { "salamander", T(E) }, { "xill", T(E | L) },
                { "erinyes-devil", T(E | L) }, { "invisible-stalker", T(N) }, { "lillend-azata", T(C | G) },
                { "shadow-demon", T(C | E) }, { "succubus", T(C | E) },
                { "bebelith", T(C | E) }, { "bone-devil", T(E | L) }, { "vrock", T(C | E) },
                { "barbed-devil", T(E | L) }, { "hezrou", T(C | E) },
                { "astral-deva", T(G) }, { "ghaele-azata", T(C | G) }, { "glabrezu", T(C | E) },
                { "ice-devil", T(E | L) }, { "nalfeshnee", T(C | E) }, { "trumpet-archon", T(G | L) },
                // Mephits and elementals: the table gives Elemental, no alignment.
                { "air-mephit", T(N) }, { "dust-mephit", T(N) }, { "earth-mephit", T(N) }, { "fire-mephit", T(N) },
                { "ice-mephit", T(N) }, { "magma-mephit", T(N) }, { "ooze-mephit", T(N) }, { "salt-mephit", T(N) },
                { "steam-mephit", T(N) }, { "water-mephit", T(N) },
                // Supported additions that the table does not list.
                { "gorgon", B(N, "N Large magical beast") },
                { "shadow-mastiff", B(E, "NE Medium outsider (evil, extraplanar)") },
                { "redcap", B(N, "NE Small fey") },
                { "axiomite", B(L, "LN Medium outsider (extraplanar, lawful)") },
                { "soul-eater", B(E, "NE Medium outsider (evil, extraplanar)") },
                { "bogeyman", B(N, "NE Medium fey") },
                { "movanic-deva", B(G, "NG Medium outsider (angel, extraplanar, good)") },
                { "frost-giant", B(N, "CE Large humanoid (cold, giant)") },
                { "thanadaemon", B(E, "NE Medium outsider (daemon, evil, extraplanar)") },
            };

        internal static bool IsElemental(string key)
        { return key != null && key.EndsWith("-elemental", StringComparison.Ordinal); }

        /// <summary>Throws for a key with no explicit classification, so an
        /// unclassified row can never count as neutral.</summary>
        internal static SummonSpellAlignment For(string creatureKey, bool monsterTemplated)
        {
            if (monsterTemplated) return SummonSpellAlignment.CasterMatched;
            if (IsElemental(creatureKey)) return T(N);
            SummonSpellAlignment value;
            if (creatureKey != null && Unstarred.TryGetValue(creatureKey, out value)) return value;
            throw new InvalidOperationException("Summon choice has no cleric alignment classification: " + creatureKey);
        }

        internal static void Validate()
        {
            foreach (SummonCreatureSpec creature in ExpandedSummoningCatalog.All.Where(value => value.MonsterTier.HasValue))
                For(creature.Key, creature.MonsterTemplated);
            foreach (SummonNativeExpansionSpec native in SummonNativeExpansionCatalog.All.Where(value => value.Family == SummonFamily.Monster))
                For(native.IconKey, false);
        }
    }
}
