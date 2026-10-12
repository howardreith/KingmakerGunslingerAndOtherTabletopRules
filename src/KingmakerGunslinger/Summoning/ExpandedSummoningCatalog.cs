using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal static class ExpandedSummoningCatalog
    {
        private static readonly SummonCreatureSpec[] Creatures = Build();
        internal static IReadOnlyList<SummonCreatureSpec> All { get { return Array.AsReadOnly(Creatures); } }

        internal static IReadOnlyList<SummonVariantSpec> GenerateVariants(SummonFamily family)
        {
            var result = new List<SummonVariantSpec>();
            for (int parent = 1; parent <= 9; parent++)
            foreach (SummonCreatureSpec creature in Creatures)
            {
                int? source = family == SummonFamily.Monster ? creature.MonsterTier : creature.NaturesAllyTier;
                if (!source.HasValue || source.Value > parent) continue;
                SummonMultiplicity count = source.Value == parent ? SummonMultiplicity.One :
                    source.Value == parent - 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne;
                result.Add(new SummonVariantSpec(family, parent, creature, source.Value, count));
            }
            return result.AsReadOnly();
        }

        internal static void Validate()
        {
            if (Creatures.Length != 103) throw new InvalidOperationException("Expanded Summoning unique creature count must be 103.");
            if (Creatures.Select(v => v.Key).Distinct(StringComparer.Ordinal).Count() != Creatures.Length)
                throw new InvalidOperationException("Duplicate creature key.");
            // Sprint 18 adds the two apes: the Ape at Summon Monster and
            // Nature's Ally III contributes seven placements per family and the
            // Dire Ape at tier IV contributes six, so 506/502 become 519/515
            // and the twenty-six new roots are registered and suppressed.
            // Sprint 21 adds the Giant Crab at Nature's Ally III with no
            // Monster tier - the first creature in the roster shaped that way -
            // so only the Nature's Ally side moves: 90/526 becomes 91/533,
            // seven placements for parents 3 through 9.
            ValidateFamily(SummonFamily.Monster, 92, 530);
            ValidateFamily(SummonFamily.NaturesAlly, 91, 533);
        }

        private static void ValidateFamily(SummonFamily family, int roster, int placements)
        {
            int actual = Creatures.Count(v => family == SummonFamily.Monster ? v.MonsterTier.HasValue : v.NaturesAllyTier.HasValue);
            IReadOnlyList<SummonVariantSpec> variants = GenerateVariants(family);
            if (actual != roster || variants.Count != placements)
                throw new InvalidOperationException(family + " invariant mismatch: roster=" + actual + ";placements=" + variants.Count + ".");
            if (variants.Select(v => v.StableKey).Distinct(StringComparer.Ordinal).Count() != variants.Count)
                throw new InvalidOperationException(family + " has duplicate variants.");
        }

        private static SummonCreatureSpec[] Build()
        {
            return new[] {
                C("dire-rat","Dire Rat",1,true,1,"Dog"), C("dog","Dog",1,true,1), C("eagle","Eagle",1,true,1,"Roc"), C("poisonous-frog","Poisonous Frog",1,true,1,"Giant Poisonous Frog"), C("pony","Pony",1,true,1), C("fire-beetle","Fire Beetle",1,true,1,"Giant Spider"),
                C("giant-centipede","Giant Centipede",2,true,1), C("wolf","Wolf",2,true,2), C("giant-frog","Giant Frog",2,true,2), C("giant-spider","Giant Spider",2,true,2),
                C("small-air-elemental","Small Air Elemental",2,false,2), C("small-earth-elemental","Small Earth Elemental",2,false,2), C("small-fire-elemental","Small Fire Elemental",2,false,2), C("small-water-elemental","Small Water Elemental",2,false,2),
                C("goblin-dog","Goblin Dog",2,true,2,"Worg"), C("hyena","Hyena",2,true,2,"Wolf"), C("horse","Horse",2,true,2), C("giant-ant-worker","Giant Ant (Worker)",2,true,2,"Giant Spider"),
                C("boar","Boar",3,true,3), C("leopard","Leopard",3,true,3), C("monitor-lizard","Monitor Lizard",3,true,3), C("cheetah","Cheetah",3,true,3,"Leopard"), C("crocodile","Crocodile",3,true,3,"Monitor Lizard"), C("dire-bat","Dire Bat",3,true,3,"Roc"), C("wolverine","Wolverine",3,true,3,"Worg"), C("giant-ant-soldier","Giant Ant (Soldier)",3,true,3,"Giant Spider"), C("lantern-archon","Lantern Archon",3,false,null,"Will-o'-Wisp"), C("giant-crab","Giant Crab",null,false,3,"Giant Spider"),
                C("dire-boar","Dire Boar",4,true,4), C("dire-wolf","Dire Wolf",4,true,4), C("grizzly-bear","Grizzly Bear",4,true,4), C("medium-air-elemental","Medium Air Elemental",4,false,4), C("medium-earth-elemental","Medium Earth Elemental",4,false,4), C("medium-fire-elemental","Medium Fire Elemental",4,false,4), C("medium-water-elemental","Medium Water Elemental",4,false,4), C("air-mephit","Air Mephit",4,false,4), C("earth-mephit","Earth Mephit",4,false,4), C("fire-mephit","Fire Mephit",4,false,4), C("water-mephit","Water Mephit",4,false,4), C("dust-mephit","Dust Mephit",4,false,4), C("ice-mephit","Ice Mephit",4,false,4), C("magma-mephit","Magma Mephit",4,false,4), C("ooze-mephit","Ooze Mephit",4,false,4), C("salt-mephit","Salt Mephit",4,false,4), C("steam-mephit","Steam Mephit",4,false,4), C("lion","Lion",4,true,4,"Leopard"), C("tiger","Tiger",null,false,4,"Leopard"), C("pteranodon","Pteranodon",4,true,4,"Roc"), C("hell-hound","Hell Hound",4,false,null), C("owlbear","Owlbear",null,false,4), C("giant-ant-drone","Giant Ant (Drone)",4,true,4,"Giant Spider"), C("giant-stag-beetle","Giant Stag Beetle",null,false,4,"Giant Spider"),
                C("large-air-elemental","Large Air Elemental",5,false,5), C("large-earth-elemental","Large Earth Elemental",5,false,5), C("large-fire-elemental","Large Fire Elemental",5,false,5), C("large-water-elemental","Large Water Elemental",5,false,5), C("dire-lion","Dire Lion",5,true,5,"Smilodon"), C("ankylosaurus","Ankylosaurus",5,true,5,"Hodag"), C("bralani-azata","Bralani Azata",5,false,null), C("salamander","Salamander",5,false,null,"Lizardfolk"), C("cyclops","Cyclops",null,false,5),
                C("dire-bear","Dire Bear",6,true,6), C("dire-tiger","Smilodon",6,true,6,"Smilodon"), C("huge-air-elemental","Huge Air Elemental",6,false,6), C("huge-earth-elemental","Huge Earth Elemental",6,false,6), C("huge-fire-elemental","Huge Fire Elemental",6,false,6), C("huge-water-elemental","Huge Water Elemental",6,false,6), C("elephant","Elephant",6,true,6,"Mastodon"), C("erinyes-devil","Erinyes Devil",6,false,null), C("invisible-stalker","Invisible Stalker",6,false,null,"Air Elemental"), C("shadow-demon","Shadow Demon",6,false,null,"Soul Eater / Ankou"), C("shadow-mastiff","Shadow Mastiff",6,false,null,"Worg"), C("succubus","Succubus",6,false,null,"Nymph / Tiefling"), C("shambling-mound","Shambling Mound",null,false,6),
                C("greater-air-elemental","Greater Air Elemental",7,false,7), C("greater-earth-elemental","Greater Earth Elemental",7,false,7), C("greater-fire-elemental","Greater Fire Elemental",7,false,7), C("greater-water-elemental","Greater Water Elemental",7,false,7), C("mastodon","Mastodon",7,true,7), C("roc","Roc",7,true,7), C("dire-crocodile","Dire Crocodile",7,true,7,"Monitor Lizard"), C("bebelith","Bebelith",7,false,null,"Doomspider"), C("giant-flytrap","Giant Flytrap",null,false,7),
                C("elder-air-elemental","Elder Air Elemental",8,false,8), C("elder-earth-elemental","Elder Earth Elemental",8,false,8), C("elder-fire-elemental","Elder Fire Elemental",8,false,8), C("elder-water-elemental","Elder Water Elemental",8,false,8), C("purple-worm","Purple Worm",null,false,8),
                C("ghaele-azata","Ghaele Azata",9,false,null), C("pixie","Pixie",null,false,9,"Pixie / Nixie / Nymph"),
                C("giant-wasp","Giant Wasp",4,true,4),
                C("stirge","Stirge",null,false,1),
                C("aurochs","Aurochs",3,true,3,"Horse"),
                C("bison","Bison",4,true,4,"Horse"),
                C("rhinoceros","Rhinoceros",4,true,4,"Mastodon"),
                C("woolly-rhinoceros","Woolly Rhinoceros",5,true,5,"Mastodon"),
                C("viper","Viper",1,true,1,"Snake"),
                C("constrictor-snake","Constrictor Snake",3,true,3,"Snake"),
                // Sprint 18. Both ride the census-chosen Troll rig while their
                // original project-owned bodies are authored, so both are
                // recorded as borrowed-body proxies and both are suppressed.
                C("ape","Ape",3,true,3,"Troll"),
                C("dire-ape","Dire Ape",4,true,4,"Troll"),
                // Sprint 19. Girallon is a Summon Nature's Ally 5 magical
                // beast and Xill a Summon Monster 5 evil outsider, so neither
                // is templated and neither appears on the other's table. Both
                // ride the Sprint 18 census rig while their original bodies
                // are authored, so both are borrowed-body proxies, and both
                // are suppressed until the complete candidate passes.
                // Sprint 20. A Large vermin on both tables at tier 4 and
                // templated, like every other vermin there, so its six
                // Summon Monster roots each own celestial and fiendish
                // execution children. It rides the Giant Spider rig the
                // Sprint 14 census chose - the only compact many-legged
                // arthropod the game has - while its original body is
                // authored, and all twelve placements are suppressed
                // until the complete candidate passes.
                C("giant-scorpion","Giant Scorpion",4,true,4,"Giant Spider"),
                C("girallon","Girallon",null,false,5,"Troll"),
                C("xill","Xill",5,false,null,"Troll")
            };
        }

        private static SummonCreatureSpec C(string key, string name, int? monster, bool templated, int? ally, string visual = null)
        { return new SummonCreatureSpec(key, name, monster, templated, ally, visual); }
    }
}
