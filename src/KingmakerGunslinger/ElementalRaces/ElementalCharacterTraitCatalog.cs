using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    internal enum ElementalCharacterTraitId { FieryGlare, StoicDignity, AerialObserver, Whiteout }
    internal enum CharacterTraitNodeKind { Feature, Toggle, ActivationBuff, ProviderBuff, Area, RecipientBuff }

    // A separate dormant semantic registry, not the installed blueprint manifest.
    // Allocate once; never reuse or regenerate these save API identities.
    internal sealed class CharacterTraitNode
    {
        internal CharacterTraitNode(string symbol, string guid, CharacterTraitNodeKind kind)
        { Symbol = symbol; Guid = guid; Kind = kind; }
        internal string Symbol { get; private set; }
        internal string Guid { get; private set; }
        internal CharacterTraitNodeKind Kind { get; private set; }
        internal bool Visible { get { return Kind == CharacterTraitNodeKind.Feature || Kind == CharacterTraitNodeKind.Toggle; } }
        internal string BlueprintType
        {
            get
            {
                return Kind == CharacterTraitNodeKind.Feature ? "BlueprintFeature" :
                    Kind == CharacterTraitNodeKind.Toggle ? "BlueprintActivatableAbility" :
                    Kind == CharacterTraitNodeKind.Area ? "BlueprintAbilityAreaEffect" : "BlueprintBuff";
            }
        }
    }

    internal sealed class ElementalCharacterTraitDefinition
    {
        private readonly CharacterTraitNode[] _nodes;
        internal ElementalCharacterTraitDefinition(ElementalCharacterTraitId id, string name,
            string race, string raceGuid, string icon, string description, string supplemental,
            params CharacterTraitNode[] nodes)
        {
            Id = id; Name = name; Race = race; RaceGuid = raceGuid; IconKey = icon;
            Description = description; Supplemental = supplemental;
            _nodes = (CharacterTraitNode[])nodes.Clone();
        }
        internal ElementalCharacterTraitId Id { get; private set; }
        internal string Name { get; private set; }
        internal string Race { get; private set; }
        internal string RaceGuid { get; private set; }
        internal string RaceSymbol { get { return "KMG.ElementalRaces." + Race + ".Race"; } }
        internal string IconKey { get; private set; }
        internal string Description { get; private set; }
        internal string Supplemental { get; private set; }
        internal string NameKey { get { return Feature.Symbol + ".Name"; } }
        internal string DescriptionKey { get { return Feature.Symbol + ".Description"; } }
        internal CharacterTraitNode[] Nodes { get { return (CharacterTraitNode[])_nodes.Clone(); } }
        internal CharacterTraitNode Feature { get { return _nodes.Single(n => n.Kind == CharacterTraitNodeKind.Feature); } }
        internal CharacterTraitNode Node(CharacterTraitNodeKind kind) { return _nodes.Single(n => n.Kind == kind); }
        internal CharacterTraitNode GrantedNode
        { get { return Node(Id == ElementalCharacterTraitId.FieryGlare ? CharacterTraitNodeKind.Toggle : CharacterTraitNodeKind.ProviderBuff); } }
        internal string OriginalPath { get { return Production + "sources/" + IconKey + ".png"; } }
        internal string ExportPath { get { return Production + "exports/" + IconKey + ".png"; } }
        internal string BriefPath { get { return Production + "briefs/" + IconKey + ".json"; } }
        internal string RuntimePath { get { return "assets/game/icons/" + IconKey + ".png"; } }
        internal string InstalledPath { get { return "assets/icons/" + IconKey + ".png"; } }
        internal string ReviewGroup { get { return Race.ToLowerInvariant() + "-traits"; } }
        private const string Production = "assets-source/original-icons/icon-overhaul-v2/production/";
    }

    internal static class ElementalCharacterTraitCatalog
    {
        internal const string SelectionField = "racial_traits";
        internal const string SelectionGuid = "331ed3c4a988415785f71a37b826d0f1";
        internal const string SelectionName = "RacialTrait";
        internal const string WingsOfAirGuid = "e116e1e0a17a4aceb001000000000019";
        internal const string WingsOfAirSymbol = "KMG.ElementalRaces.Feats.WingsOfAir.Buff";
        internal const string OriginalProfile = "project-painted-128";
        internal const string Family = "painted-magical";
        private const string Prefix = "KMG.ElementalRaces.CharacterTraits.";
        private static CharacterTraitNode N(string trait, string suffix, string guid, CharacterTraitNodeKind kind)
        { return new CharacterTraitNode(Prefix + trait + "." + suffix, guid, kind); }
        private static readonly ElementalCharacterTraitDefinition[] Definitions =
        {
            new ElementalCharacterTraitDefinition(ElementalCharacterTraitId.FieryGlare, "Fiery Glare",
                "Ifrit", "556a2d9ae0c6401eaed87614a2caf539", "fiery-glare",
                "While Fiery Glare is enabled, Intimidate checks use a result of 10 whenever that would succeed; otherwise, they are rolled normally. This functions even during combat.",
                "",
                N("FieryGlare", "Feature", "ca5d4d43d35548679ed6b533f055cb20", CharacterTraitNodeKind.Feature),
                N("FieryGlare", "Toggle", "15549c6f936f4000a7c476f5d50d2585", CharacterTraitNodeKind.Toggle),
                N("FieryGlare", "ActivationBuff", "7883b258441e4e179b07cacbdc5416ac", CharacterTraitNodeKind.ActivationBuff)),
            new ElementalCharacterTraitDefinition(ElementalCharacterTraitId.StoicDignity, "Stoic Dignity",
                "Oread", "7ef60bcda0204429bf4859e2faa3cbf8", "stoic-dignity",
                "While conscious, you gain a +1 trait bonus on saving throws against mind-affecting effects. Other allies within 10 feet gain a +1 morale bonus on these saving throws. A creature receives no bonus against the same effect it is already suffering from; an unrelated effect does not prevent the bonus.",
                "The same effect is identified by its exact effect or source lineage. If that relationship cannot be established, the bonus applies. Stoic Dignity does not remove or suppress an existing effect.",
                N("StoicDignity", "Feature", "51faa3c273df41738553c18c088ef565", CharacterTraitNodeKind.Feature),
                N("StoicDignity", "ProviderBuff", "46e6b683b15a4811886730027994e58d", CharacterTraitNodeKind.ProviderBuff),
                N("StoicDignity", "Area", "b3f85f9663b44129b222ae69bb3e8cd3", CharacterTraitNodeKind.Area),
                N("StoicDignity", "RecipientBuff", "461f898838864ff0b50ab355efa0c644", CharacterTraitNodeKind.RecipientBuff)),
            new ElementalCharacterTraitDefinition(ElementalCharacterTraitId.AerialObserver, "Aerial Observer",
                "Sylph", "68b64570c6e943f1bcbe4571e88bf285", "aerial-observer",
                "While Wings of Air is active, you gain a +2 trait bonus on Perception.",
                "This benefit requires the active Wings of Air effect. Visual wings, hovering, elevated ground and other flight effects do not grant it.",
                N("AerialObserver", "Feature", "4064cbe37690440a8650ea5689354a65", CharacterTraitNodeKind.Feature),
                N("AerialObserver", "ProviderBuff", "12f9760b84264e178af8a5251d559242", CharacterTraitNodeKind.ProviderBuff)),
            new ElementalCharacterTraitDefinition(ElementalCharacterTraitId.Whiteout, "Whiteout",
                "Undine", "557dea40c2cc440f8afe7d678d2d283a", "whiteout",
                "While outdoors in rain or snow of at least light intensity, attacks against you have an additional independent 10% miss chance. This chance is checked after other concealment and miss chances, and attacks that ignore concealment ignore it. Magical fog and waterfall spray do not grant this benefit.",
                "The two miss chances are checked separately: a 20% concealment chance and Whiteout's 10% chance give a combined 28% chance to miss. Whiteout affects attacks that make the game's concealment and miss-chance check; attacks that skip that check and effects without an attack roll are unaffected.",
                N("Whiteout", "Feature", "7aa5dc9716e4437daa8e00506f6ddd83", CharacterTraitNodeKind.Feature),
                N("Whiteout", "ProviderBuff", "901f9f54462846149cf7fb24ec04a5a5", CharacterTraitNodeKind.ProviderBuff))
        };
        internal static ElementalCharacterTraitDefinition[] All()
        { return (ElementalCharacterTraitDefinition[])Definitions.Clone(); }
        internal static ElementalCharacterTraitDefinition Get(ElementalCharacterTraitId id)
        { return Definitions.Single(d => d.Id == id); }
        internal static CharacterTraitNode[] Nodes()
        { return Definitions.SelectMany(d => d.Nodes).ToArray(); }
        internal static void Validate()
        {
            var nodes = Nodes();
            if (Definitions.Length != 4 || Definitions.Select(d => d.Id).Distinct().Count() != 4 ||
                nodes.Length != 11 || nodes.Select(n => n.Guid).Distinct(StringComparer.Ordinal).Count() != 11 ||
                nodes.Select(n => n.Symbol).Distinct(StringComparer.Ordinal).Count() != 11 ||
                nodes.Any(n => n.Guid.Length != 32 || n.Guid.Any(c => !"0123456789abcdef".Contains(c)) ||
                    n.Guid == new string('0',32)) ||
                nodes.Any(n => n.Guid == WingsOfAirGuid || Definitions.Any(d => d.RaceGuid == n.Guid)))
                throw new InvalidOperationException("Dormant character-trait identities are invalid or collide.");
        }
        internal static bool CanSelect(object canonicalRace, object actualRace, bool alreadyHasFeature)
        { return canonicalRace != null && ReferenceEquals(canonicalRace, actualRace) && !alreadyHasFeature; }
    }
}
