using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum IdealRosterPriority { Essential, Strong, Variety, Stretch }

    internal enum IdealRosterEffort { None, Low, Medium, High, VeryHigh }

    internal sealed class IdealRosterEntry
    {
        internal IdealRosterEntry(string key, string name, int? monsterTier,
            int? naturesAllyTier, int? sprint, IdealRosterPriority priority,
            IdealRosterEffort effort,
            bool charterBaselineComplete, string assetPackage)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key");
            if (!monsterTier.HasValue && !naturesAllyTier.HasValue)
                throw new ArgumentException(
                    "An entry must belong to at least one family: " + key);
            if (monsterTier.HasValue && (monsterTier.Value < 1 || monsterTier.Value > 9))
                throw new ArgumentOutOfRangeException("monsterTier");
            if (naturesAllyTier.HasValue &&
                (naturesAllyTier.Value < 1 || naturesAllyTier.Value > 9))
                throw new ArgumentOutOfRangeException("naturesAllyTier");
            Key = key; Name = name; MonsterTier = monsterTier;
            NaturesAllyTier = naturesAllyTier; Sprint = sprint; Priority = priority;
            Effort = effort;
            CharterBaselineComplete = charterBaselineComplete;
            AssetPackage = assetPackage;
        }

        internal string Key { get; private set; }
        internal string Name { get; private set; }
        internal int? MonsterTier { get; private set; }
        internal int? NaturesAllyTier { get; private set; }
        /// <summary>Charter sprint that owns the work; null once complete.</summary>
        internal int? Sprint { get; private set; }
        internal IdealRosterPriority Priority { get; private set; }
        internal IdealRosterEffort Effort { get; private set; }
        /// <summary>Listed in charter Appendix B as already complete.</summary>
        internal bool CharterBaselineComplete { get; private set; }
        internal string AssetPackage { get; private set; }

        internal int? Tier(SummonFamily family)
        { return family == SummonFamily.Monster ? MonsterTier : NaturesAllyTier; }
    }

    /// <summary>
    /// The planning manifest for the Expanded Summoning charter's ideal roster.
    /// It is deliberately inert: nothing here registers, reserves, or publishes
    /// a blueprint, and no publication path reads it. Live creatures remain
    /// owned by <see cref="ExpandedSummoningCatalog"/>, so a planned row cannot
    /// leak a half-built creature into a player menu.
    ///
    /// It deliberately carries no coverage column. Current coverage is derived
    /// from the shipped catalogs by
    /// <see cref="ExpandedSummoningCoveragePolicy"/>, so a hand-maintained
    /// value cannot drift away from what actually ships.
    /// </summary>
    internal static class ExpandedSummoningIdealRosterCatalog
    {
        internal const int UniqueCreatureTarget = 145;
        internal const int MonsterBaseEntryTarget = 120;
        internal const int NaturesAllyBaseEntryTarget = 110;
        internal const int CorePlacementTarget = 1232;
        internal const int VariantElementalFamilies = 5;

        private static readonly IdealRosterEntry[] Entries = Build();

        internal static IReadOnlyList<IdealRosterEntry> All
        { get { return Array.AsReadOnly(Entries); } }

        internal static IdealRosterEntry Find(string key)
        {
            return Entries.SingleOrDefault(value =>
                string.Equals(value.Key, key, StringComparison.Ordinal));
        }

        internal static int BaseEntries(SummonFamily family)
        { return Entries.Count(value => value.Tier(family).HasValue); }

        /// <summary>
        /// Projected placements under one parent spell, ordered deterministically.
        /// </summary>
        internal static IReadOnlyList<IdealRosterEntry> Placements(
            SummonFamily family, int parentTier)
        {
            if (parentTier < 1 || parentTier > 9)
                throw new ArgumentOutOfRangeException("parentTier");
            return Entries
                .Where(value => value.Tier(family).HasValue &&
                    value.Tier(family).Value <= parentTier)
                .OrderBy(value => value.Key, StringComparer.Ordinal)
                .ToList().AsReadOnly();
        }

        /// <summary>
        /// Charter quantity semantics: own tier single, one tier lower 1d3,
        /// everything still lower 1d4+1.
        /// </summary>
        internal static SummonMultiplicity Multiplicity(int sourceTier, int parentTier)
        {
            if (sourceTier < 1 || sourceTier > parentTier)
                throw new ArgumentOutOfRangeException("sourceTier");
            if (sourceTier == parentTier) return SummonMultiplicity.One;
            return sourceTier == parentTier - 1
                ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne;
        }

        internal static int TotalPlacements(SummonFamily family)
        {
            int total = 0;
            for (int parent = 1; parent <= 9; parent++)
                total += Placements(family, parent).Count;
            return total;
        }

        internal static void Validate()
        {
            if (Entries.Length != UniqueCreatureTarget)
                throw new InvalidOperationException(
                    "Ideal roster must hold " + UniqueCreatureTarget + " creatures.");
            if (Entries.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != Entries.Length)
                throw new InvalidOperationException("Duplicate ideal roster key.");
            if (BaseEntries(SummonFamily.Monster) != MonsterBaseEntryTarget ||
                BaseEntries(SummonFamily.NaturesAlly) != NaturesAllyBaseEntryTarget)
                throw new InvalidOperationException(
                    "Ideal roster family totals changed.");
            if (TotalPlacements(SummonFamily.Monster) +
                    TotalPlacements(SummonFamily.NaturesAlly) != CorePlacementTarget)
                throw new InvalidOperationException(
                    "Projected core placements must equal " + CorePlacementTarget + ".");
            foreach (IdealRosterEntry entry in Entries)
            {
                // A charter-complete creature owns no outstanding sprint, and
                // anything still outstanding must name the sprint that owns it.
                if (entry.CharterBaselineComplete && entry.Sprint.HasValue)
                    throw new InvalidOperationException(
                        "A baseline-complete creature cannot own sprint work: " + entry.Key);
                if (!entry.CharterBaselineComplete && !entry.Sprint.HasValue)
                    throw new InvalidOperationException(
                        "An outstanding creature must name its sprint: " + entry.Key);
            }
        }

        private static IdealRosterEntry[] Build()
        {
            return new[] {
                R("air-mephit","Air Mephit",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Mephit family"),
                R("ankylosaurus","Ankylosaurus",5,5,22,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Armored dinosaurs"),
                R("ape","Ape",3,3,18,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Primate rig family"),
                R("astral-deva","Astral Deva",9,null,31,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Outsiders"),
                R("aurochs","Aurochs",3,3,11,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Equines & ungulates"),
                R("axiomite","Axiomite",6,null,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Outsiders"),
                R("babau","Babau",5,null,36,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Outsiders"),
                R("barbed-devil","Barbed Devil",8,null,35,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Outsiders"),
                R("bearded-devil","Bearded Devil",5,null,34,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Outsiders"),
                R("bebelith","Bebelith",7,null,21,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Arachnids & scorpion"),
                R("bison","Bison",4,4,11,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Equines & ungulates"),
                R("boar","Boar",3,3,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Boar family"),
                R("bogeyman","Bogeyman",7,null,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Fey"),
                R("bone-devil","Bone Devil",7,null,35,IdealRosterPriority.Strong,IdealRosterEffort.VeryHigh,false,"Outsiders"),
                R("brachiosaurus","Brachiosaurus",7,7,25,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Sauropod dinosaur"),
                R("bralani-azata","Bralani Azata",5,null,null,IdealRosterPriority.Essential,IdealRosterEffort.None,true,"Outsiders"),
                R("bulette","Bulette",null,6,26,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Burrowers & huge monsters"),
                R("cheetah","Cheetah",3,3,8,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Big-cat family"),
                R("cloud-giant","Cloud Giant",null,8,29,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Giant humanoids"),
                R("constrictor-snake","Constrictor Snake",3,3,17,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Snake rig family"),
                R("crocodile","Crocodile",3,3,16,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Crocodilian rig family"),
                R("cyclops","Cyclops",null,5,3,IdealRosterPriority.Essential,IdealRosterEffort.Low,false,"Giant humanoids"),
                R("deinonychus","Deinonychus",4,4,24,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Theropod dinosaurs"),
                R("dire-ape","Dire Ape",4,4,18,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Primate rig family"),
                R("dire-bat","Dire Bat",3,3,9,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Flying creature rigs"),
                R("dire-bear","Dire Bear",6,6,6,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Individual creature"),
                R("dire-boar","Dire Boar",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Boar family"),
                R("dire-crocodile","Dire Crocodile",7,7,16,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Crocodilian rig family"),
                R("dire-lion","Dire Lion",5,5,7,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Big-cat family"),
                R("dire-rat","Dire Rat",1,1,12,IdealRosterPriority.Variety,IdealRosterEffort.Medium,false,"Canines & small quadrupeds"),
                R("dire-tiger","Dire Tiger (Smilodon)",6,6,7,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Big-cat family"),
                R("dire-wolf","Dire Wolf",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Canines & small quadrupeds"),
                R("dog","Dog",1,1,12,IdealRosterPriority.Variety,IdealRosterEffort.Medium,false,"Canines & small quadrupeds"),
                R("dretch","Dretch",3,null,30,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Outsiders"),
                R("dust-mephit","Dust Mephit",4,4,5,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Mephit family"),
                R("eagle","Eagle",1,1,9,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Flying creature rigs"),
                R("earth-mephit","Earth Mephit",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Mephit family"),
                R("elder-air-elemental","Elder Air Elemental",8,8,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("elder-earth-elemental","Elder Earth Elemental",8,8,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("elder-fire-elemental","Elder Fire Elemental",8,8,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("elder-water-elemental","Elder Water Elemental",8,8,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("elephant","Elephant",6,6,25,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Proboscideans"),
                R("erinyes-devil","Erinyes Devil",6,null,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Outsiders"),
                R("ettin","Ettin",null,5,29,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Individual creature"),
                R("fire-beetle","Fire Beetle",1,1,14,IdealRosterPriority.Variety,IdealRosterEffort.High,false,"Insect rig family"),
                R("fire-giant","Fire Giant",null,7,28,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Giant humanoids"),
                R("fire-mephit","Fire Mephit",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Mephit family"),
                R("frost-giant","Frost Giant",8,7,3,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Giant humanoids"),
                R("ghaele-azata","Ghaele Azata",9,null,null,IdealRosterPriority.Essential,IdealRosterEffort.None,true,"Outsiders"),
                R("giant-ant-drone","Giant Ant (Drone)",4,4,15,IdealRosterPriority.Variety,IdealRosterEffort.High,false,"Insect rig family"),
                R("giant-ant-soldier","Giant Ant (Soldier)",3,3,14,IdealRosterPriority.Variety,IdealRosterEffort.Medium,false,"Insect rig family"),
                R("giant-ant-worker","Giant Ant (Worker)",2,2,14,IdealRosterPriority.Variety,IdealRosterEffort.High,false,"Insect rig family"),
                R("giant-centipede","Giant Centipede",2,1,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Native vermin"),
                R("giant-crab","Giant Crab",null,3,21,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Crustacean rig"),
                R("giant-flytrap","Giant Flytrap",null,7,4,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Native plant summons"),
                R("giant-frog","Giant Frog",2,2,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Frog family"),
                R("giant-scorpion","Giant Scorpion",4,4,20,IdealRosterPriority.Essential,IdealRosterEffort.VeryHigh,false,"Arachnids & scorpion"),
                R("giant-spider","Giant Spider",2,2,6,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Arachnids & scorpion"),
                R("giant-stag-beetle","Giant Stag Beetle",null,4,15,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Insect rig family"),
                R("giant-wasp","Giant Wasp",4,4,10,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Flying creature rigs"),
                R("girallon","Girallon",null,5,19,IdealRosterPriority.Essential,IdealRosterEffort.VeryHigh,false,"Primate rig family"),
                R("glabrezu","Glabrezu",9,null,38,IdealRosterPriority.Essential,IdealRosterEffort.VeryHigh,false,"Outsiders"),
                R("goblin-dog","Goblin Dog",2,2,12,IdealRosterPriority.Variety,IdealRosterEffort.Medium,false,"Canines & small quadrupeds"),
                R("gorgon","Gorgon",8,null,26,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Armored magical beast"),
                R("greater-air-elemental","Greater Air Elemental",7,7,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("greater-earth-elemental","Greater Earth Elemental",7,7,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("greater-fire-elemental","Greater Fire Elemental",7,7,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("greater-water-elemental","Greater Water Elemental",7,7,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("griffon","Griffon",null,4,27,IdealRosterPriority.Essential,IdealRosterEffort.VeryHigh,false,"Hybrid magical beasts"),
                R("grizzly-bear","Grizzly Bear",4,4,6,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Individual creature"),
                R("hamadryad","Hamadryad",null,9,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Fey"),
                R("hell-hound","Hell Hound",4,null,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Outsiders"),
                R("hezrou","Hezrou",8,null,36,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Outsiders"),
                R("hill-giant","Hill Giant",null,6,28,IdealRosterPriority.Variety,IdealRosterEffort.Medium,false,"Giant humanoids"),
                R("horse","Horse",2,2,3,IdealRosterPriority.Variety,IdealRosterEffort.Low,false,"Equines & ungulates"),
                R("hound-archon","Hound Archon",4,null,30,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Outsiders"),
                R("huge-air-elemental","Huge Air Elemental",6,6,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("huge-earth-elemental","Huge Earth Elemental",6,6,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("huge-fire-elemental","Huge Fire Elemental",6,6,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("huge-water-elemental","Huge Water Elemental",6,6,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("hyena","Hyena",2,2,12,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Canines & small quadrupeds"),
                R("ice-devil","Ice Devil",9,null,40,IdealRosterPriority.Strong,IdealRosterEffort.VeryHigh,false,"Outsiders"),
                R("ice-mephit","Ice Mephit",4,4,5,IdealRosterPriority.Variety,IdealRosterEffort.Low,false,"Mephit family"),
                R("invisible-stalker","Invisible Stalker",6,null,33,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Outsiders"),
                R("kyton-chain-devil","Kyton (Chain Devil)",5,null,34,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Outsiders"),
                R("lantern-archon","Lantern Archon",3,null,31,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Outsiders"),
                R("large-air-elemental","Large Air Elemental",5,5,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("large-earth-elemental","Large Earth Elemental",5,5,null,IdealRosterPriority.Essential,IdealRosterEffort.None,true,"Classic elementals"),
                R("large-fire-elemental","Large Fire Elemental",5,5,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("large-water-elemental","Large Water Elemental",5,5,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("lemure","Lemure",2,null,30,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Outsiders"),
                R("leopard","Leopard",3,3,7,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Big-cat family"),
                R("lillend-azata","Lillend Azata",6,null,32,IdealRosterPriority.Essential,IdealRosterEffort.VeryHigh,false,"Outsiders"),
                R("lion","Lion",4,4,7,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Big-cat family"),
                R("magma-mephit","Magma Mephit",4,4,5,IdealRosterPriority.Variety,IdealRosterEffort.Low,false,"Mephit family"),
                R("manticore","Manticore",null,5,null,IdealRosterPriority.Essential,IdealRosterEffort.None,true,"Hybrid magical beasts"),
                R("mastodon","Mastodon",7,7,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Proboscideans"),
                R("medium-air-elemental","Medium Air Elemental",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("medium-earth-elemental","Medium Earth Elemental",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("medium-fire-elemental","Medium Fire Elemental",4,4,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("medium-water-elemental","Medium Water Elemental",4,4,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("mite","Mite",null,1,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Fey"),
                R("monitor-lizard","Monitor Lizard",3,3,6,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Individual creature"),
                R("movanic-deva","Movanic Deva",8,null,null,IdealRosterPriority.Essential,IdealRosterEffort.None,true,"Outsiders"),
                R("nalfeshnee","Nalfeshnee",9,null,39,IdealRosterPriority.Essential,IdealRosterEffort.VeryHigh,false,"Outsiders"),
                R("nereid","Nereid",null,8,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Fey"),
                R("ooze-mephit","Ooze Mephit",4,4,5,IdealRosterPriority.Essential,IdealRosterEffort.Low,false,"Mephit family"),
                R("owlbear","Owlbear",null,4,3,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Hybrid magical beasts"),
                R("pixie","Pixie",null,9,6,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Fey"),
                R("poisonous-frog","Poisonous Frog",1,1,13,IdealRosterPriority.Variety,IdealRosterEffort.Medium,false,"Frog family"),
                R("pony","Pony",1,1,3,IdealRosterPriority.Essential,IdealRosterEffort.Low,false,"Equines & ungulates"),
                R("pteranodon","Pteranodon",4,4,2,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Flying creature rigs"),
                R("purple-worm","Purple Worm",null,8,4,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Burrowers & huge monsters"),
                R("redcap","Redcap",5,null,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Fey"),
                R("rhinoceros","Rhinoceros",4,4,11,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Equines & ungulates"),
                R("roc","Roc",7,7,null,IdealRosterPriority.Essential,IdealRosterEffort.None,true,"Flying creature rigs"),
                R("salamander","Salamander",5,null,17,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Outsiders"),
                R("salt-mephit","Salt Mephit",4,4,5,IdealRosterPriority.Essential,IdealRosterEffort.Low,false,"Mephit family"),
                R("satyr","Satyr",null,4,32,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Fey"),
                R("shadow-demon","Shadow Demon",6,null,33,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Outsiders"),
                R("shadow-mastiff","Shadow Mastiff",6,null,13,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Canines & small quadrupeds"),
                R("shambling-mound","Shambling Mound",null,6,4,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Native plant summons"),
                R("small-air-elemental","Small Air Elemental",2,2,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Classic elementals"),
                R("small-earth-elemental","Small Earth Elemental",2,2,null,IdealRosterPriority.Essential,IdealRosterEffort.None,true,"Classic elementals"),
                R("small-fire-elemental","Small Fire Elemental",2,2,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("small-water-elemental","Small Water Elemental",2,2,null,IdealRosterPriority.Variety,IdealRosterEffort.None,true,"Classic elementals"),
                R("soul-eater","Soul Eater",6,null,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Outsiders"),
                R("steam-mephit","Steam Mephit",4,4,5,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Mephit family"),
                R("stegosaurus","Stegosaurus",null,6,23,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Armored dinosaurs"),
                R("stirge","Stirge",null,1,10,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Flying creature rigs"),
                R("stone-giant","Stone Giant",null,6,28,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Giant humanoids"),
                R("storm-giant","Storm Giant",null,9,29,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Giant humanoids"),
                R("succubus","Succubus",6,null,33,IdealRosterPriority.Strong,IdealRosterEffort.Medium,false,"Outsiders"),
                R("thanadaemon","Thanadaemon",9,null,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Outsiders"),
                R("tiger","Tiger",null,4,8,IdealRosterPriority.Essential,IdealRosterEffort.Medium,false,"Big-cat family"),
                R("triceratops","Triceratops",6,6,23,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Armored dinosaurs"),
                R("trumpet-archon","Trumpet Archon",9,null,31,IdealRosterPriority.Strong,IdealRosterEffort.High,false,"Outsiders"),
                R("tyrannosaurus","Tyrannosaurus",7,7,24,IdealRosterPriority.Essential,IdealRosterEffort.High,false,"Theropod dinosaurs"),
                R("viper","Viper",1,1,17,IdealRosterPriority.Variety,IdealRosterEffort.High,false,"Snake rig family"),
                R("vrock","Vrock",7,null,37,IdealRosterPriority.Essential,IdealRosterEffort.VeryHigh,false,"Outsiders"),
                R("water-mephit","Water Mephit",4,4,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Mephit family"),
                R("wolf","Wolf",2,2,null,IdealRosterPriority.Strong,IdealRosterEffort.None,true,"Canines & small quadrupeds"),
                R("wolverine","Wolverine",3,3,13,IdealRosterPriority.Variety,IdealRosterEffort.Medium,false,"Canines & small quadrupeds"),
                R("woolly-rhinoceros","Woolly Rhinoceros",5,5,11,IdealRosterPriority.Strong,IdealRosterEffort.Low,false,"Equines & ungulates"),
                R("xill","Xill",5,null,19,IdealRosterPriority.Strong,IdealRosterEffort.VeryHigh,false,"Outsiders"),
            };
        }

        private static IdealRosterEntry R(string key, string name, int? monsterTier,
            int? naturesAllyTier, int? sprint, IdealRosterPriority priority,
            IdealRosterEffort effort,
            bool charterBaselineComplete, string assetPackage)
        {
            return new IdealRosterEntry(key, name, monsterTier, naturesAllyTier,
                sprint, priority, effort, charterBaselineComplete,
                assetPackage);
        }
    }
}
