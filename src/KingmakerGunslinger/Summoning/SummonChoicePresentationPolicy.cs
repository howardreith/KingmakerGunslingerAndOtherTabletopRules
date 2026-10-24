using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonChoiceSize { None, Small, Medium, Large, Huge, Greater, Elder }
    internal enum SummonChoiceTemplate { None, Celestial, Fiendish }

    /// <summary>Presentation metadata only; never an identity or rules input.</summary>
    internal sealed class SummonChoicePresentation
    {
        internal SummonChoicePresentation(string key, string family,
            string subtype, SummonChoiceSize size,
            SummonChoiceTemplate template, SummonMultiplicity quantity)
        {
            Key = key; Family = family; Subtype = subtype; Size = size;
            Template = template; Quantity = quantity;
        }

        internal string Key { get; private set; }
        internal string Family { get; private set; }
        internal string Subtype { get; private set; }
        internal SummonChoiceSize Size { get; private set; }
        internal int SizeRank { get { return (int)Size; } }
        internal SummonChoiceTemplate Template { get; private set; }
        internal SummonMultiplicity Quantity { get; private set; }
        internal string Label
        {
            get
            {
                string label = Family + (Subtype.Length == 0 ? "" : ", " + Subtype) +
                    (Size == SummonChoiceSize.None ? "" : ", " + Size);
                if (Template != SummonChoiceTemplate.None)
                    label += " (" + Template + ")";
                return label + (Quantity == SummonMultiplicity.One ? "" :
                    Quantity == SummonMultiplicity.OneD3 ? " (1d3)" : " (1d4+1)");
            }
        }
    }

    internal static class SummonChoicePresentationPolicy
    {
        // Deliberate species mapping, including names intentionally kept whole.
        // No English word swapping, translated-name matching or sorting occurs.
        private static readonly SummonChoicePresentation[] Values = {
            P("dire-rat", "Rat", "Dire"), P("dog", "Dog"),
            P("eagle", "Eagle"), P("poisonous-frog", "Frog", "Poisonous"),
            P("pony", "Pony"), P("fire-beetle", "Beetle", "Fire"),
            P("giant-centipede", "Centipede", "Giant"), P("wolf", "Wolf"),
            P("giant-frog", "Frog", "Giant"), P("giant-spider", "Spider", "Giant"),
            P("small-air-elemental", "Elemental", "Air", SummonChoiceSize.Small),
            P("small-earth-elemental", "Elemental", "Earth", SummonChoiceSize.Small),
            P("small-fire-elemental", "Elemental", "Fire", SummonChoiceSize.Small),
            P("small-water-elemental", "Elemental", "Water", SummonChoiceSize.Small),
            P("goblin-dog", "Dog", "Goblin"), P("hyena", "Hyena"),
            P("horse", "Horse"), P("giant-ant-worker", "Ant", "Giant (Worker)"),
            P("boar", "Boar"), P("leopard", "Leopard"),
            P("monitor-lizard", "Lizard", "Monitor"), P("cheetah", "Cheetah"),
            P("crocodile", "Crocodile"), P("dire-bat", "Bat", "Dire"),
            P("wolverine", "Wolverine"), P("giant-ant-soldier", "Ant", "Giant (Soldier)"),
            P("lantern-archon", "Archon", "Lantern"), P("giant-crab", "Crab", "Giant"),
            P("dire-boar", "Boar", "Dire"), P("dire-wolf", "Wolf", "Dire"),
            P("grizzly-bear", "Bear", "Grizzly"),
            P("medium-air-elemental", "Elemental", "Air", SummonChoiceSize.Medium),
            P("medium-earth-elemental", "Elemental", "Earth", SummonChoiceSize.Medium),
            P("medium-fire-elemental", "Elemental", "Fire", SummonChoiceSize.Medium),
            P("medium-water-elemental", "Elemental", "Water", SummonChoiceSize.Medium),
            P("air-mephit", "Mephit", "Air"), P("earth-mephit", "Mephit", "Earth"),
            P("fire-mephit", "Mephit", "Fire"), P("water-mephit", "Mephit", "Water"),
            P("dust-mephit", "Mephit", "Dust"), P("ice-mephit", "Mephit", "Ice"),
            P("magma-mephit", "Mephit", "Magma"), P("ooze-mephit", "Mephit", "Ooze"),
            P("salt-mephit", "Mephit", "Salt"), P("steam-mephit", "Mephit", "Steam"),
            P("lion", "Lion"), P("tiger", "Tiger"), P("pteranodon", "Pteranodon"),
            P("hell-hound", "Hell Hound"), P("owlbear", "Owlbear"),
            P("giant-ant-drone", "Ant", "Giant (Drone)"),
            P("giant-stag-beetle", "Beetle", "Stag, Giant"),
            P("large-air-elemental", "Elemental", "Air", SummonChoiceSize.Large),
            P("large-earth-elemental", "Elemental", "Earth", SummonChoiceSize.Large),
            P("large-fire-elemental", "Elemental", "Fire", SummonChoiceSize.Large),
            P("large-water-elemental", "Elemental", "Water", SummonChoiceSize.Large),
            P("dire-lion", "Lion", "Dire"), P("ankylosaurus", "Ankylosaurus"),
            P("bralani-azata", "Azata", "Bralani"), P("salamander", "Salamander"),
            P("cyclops", "Cyclops"), P("dire-bear", "Bear", "Dire"),
            P("dire-tiger", "Smilodon"),
            P("huge-air-elemental", "Elemental", "Air", SummonChoiceSize.Huge),
            P("huge-earth-elemental", "Elemental", "Earth", SummonChoiceSize.Huge),
            P("huge-fire-elemental", "Elemental", "Fire", SummonChoiceSize.Huge),
            P("huge-water-elemental", "Elemental", "Water", SummonChoiceSize.Huge),
            P("elephant", "Elephant"), P("erinyes-devil", "Devil", "Erinyes"),
            P("invisible-stalker", "Invisible Stalker"), P("shadow-demon", "Demon", "Shadow"),
            P("shadow-mastiff", "Mastiff", "Shadow"), P("succubus", "Succubus"),
            P("shambling-mound", "Shambling Mound"),
            P("greater-air-elemental", "Elemental", "Air", SummonChoiceSize.Greater),
            P("greater-earth-elemental", "Elemental", "Earth", SummonChoiceSize.Greater),
            P("greater-fire-elemental", "Elemental", "Fire", SummonChoiceSize.Greater),
            P("greater-water-elemental", "Elemental", "Water", SummonChoiceSize.Greater),
            P("mastodon", "Mastodon"), P("roc", "Roc"),
            P("dire-crocodile", "Crocodile", "Dire"), P("bebelith", "Bebelith"),
            P("giant-flytrap", "Flytrap", "Giant"),
            P("elder-air-elemental", "Elemental", "Air", SummonChoiceSize.Elder),
            P("elder-earth-elemental", "Elemental", "Earth", SummonChoiceSize.Elder),
            P("elder-fire-elemental", "Elemental", "Fire", SummonChoiceSize.Elder),
            P("elder-water-elemental", "Elemental", "Water", SummonChoiceSize.Elder),
            P("purple-worm", "Worm", "Purple"), P("ghaele-azata", "Azata", "Ghaele"),
            P("pixie", "Pixie"), P("giant-wasp", "Wasp", "Giant"), P("stirge", "Stirge"),
            P("aurochs", "Aurochs"), P("bison", "Bison"), P("rhinoceros", "Rhinoceros"),
            P("woolly-rhinoceros", "Rhinoceros", "Woolly"), P("viper", "Viper"),
            P("constrictor-snake", "Snake", "Constrictor"), P("ape", "Ape"),
            P("dire-ape", "Ape", "Dire"), P("giant-scorpion", "Scorpion", "Giant"),
            P("girallon", "Girallon"), P("xill", "Xill"), P("triceratops", "Triceratops"),
            P("stegosaurus", "Stegosaurus"), P("deinonychus", "Deinonychus"),
            P("tyrannosaurus", "Tyrannosaurus"), P("brachiosaurus", "Brachiosaurus"),
            P("gorgon", "Gorgon"), P("bulette", "Bulette"), P("griffon", "Griffon"),
            P("hill-giant", "Giant", "Hill"), P("stone-giant", "Giant", "Stone"),
            P("fire-giant", "Giant", "Fire"), P("ettin", "Ettin"),
            P("cloud-giant", "Giant", "Cloud"), P("storm-giant", "Giant", "Storm"),
            // Hidden S30/S31 candidates use the released explicit family
            // presentation policy too; labels never grant public access.
            P("lemure", "Devil", "Lemure"), P("dretch", "Demon", "Dretch"),
            P("hound-archon", "Archon", "Hound"),
            P("astral-deva", "Deva", "Astral"),
            P("trumpet-archon", "Archon", "Trumpet"),
            // Released retained-native wrappers use their existing icon keys.
            P("redcap", "Redcap"), P("axiomite", "Axiomite"), P("soul-eater", "Soul Eater"),
            P("bogeyman", "Bogeyman"), P("movanic-deva", "Deva", "Movanic"),
            P("frost-giant", "Giant", "Frost"), P("thanadaemon", "Thanadaemon"),
            P("mite", "Mite"), P("manticore", "Manticore"), P("nereid", "Nereid"),
            P("hamadryad", "Hamadryad")
        };

        private static readonly IDictionary<string, SummonChoicePresentation> ByKey =
            Values.ToDictionary(value => value.Key, StringComparer.Ordinal);

        internal static SummonChoicePresentation For(string key,
            SummonMultiplicity quantity = SummonMultiplicity.One,
            SummonChoiceTemplate template = SummonChoiceTemplate.None)
        {
            SummonChoicePresentation value;
            if (key == null || !ByKey.TryGetValue(key, out value))
                throw new InvalidOperationException("Summon choice has no explicit presentation: " + key);
            if (!Enum.IsDefined(typeof(SummonMultiplicity), quantity) ||
                !Enum.IsDefined(typeof(SummonChoiceTemplate), template))
                throw new ArgumentOutOfRangeException();
            return new SummonChoicePresentation(value.Key, value.Family,
                value.Subtype, value.Size, template, quantity);
        }

        internal static void Validate()
        {
            foreach (string key in ExpandedSummoningCatalog.All.Select(value => value.Key)
                .Concat(SummonNativeExpansionCatalog.All.Select(value => value.IconKey)))
                For(key);
        }

        private static SummonChoicePresentation P(string key, string family,
            string subtype = "", SummonChoiceSize size = SummonChoiceSize.None)
        { return new SummonChoicePresentation(key, family, subtype, size,
            SummonChoiceTemplate.None, SummonMultiplicity.One); }
    }
}
