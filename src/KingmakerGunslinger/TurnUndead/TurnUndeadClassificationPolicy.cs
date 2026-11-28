using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.TurnUndead
{
    internal enum TurnUndeadMind
    {
        Mindless,
        Intelligent
    }

    /// <summary>The engine-free identity of one undead creature.</summary>
    internal sealed class TurnUndeadUnitRecord
    {
        internal TurnUndeadUnitRecord(string unitBlueprintGuid, string unitTypeGuid,
            IEnumerable<string> factGuids)
        {
            UnitBlueprintGuid = unitBlueprintGuid ?? string.Empty;
            UnitTypeGuid = unitTypeGuid ?? string.Empty;
            FactGuids = new HashSet<string>(factGuids ?? Enumerable.Empty<string>(),
                StringComparer.Ordinal);
        }

        internal string UnitBlueprintGuid { get; private set; }
        internal string UnitTypeGuid { get; private set; }
        internal HashSet<string> FactGuids { get; private set; }
    }

    internal sealed class TurnUndeadClassification
    {
        internal TurnUndeadClassification(TurnUndeadMind mind, string rule,
            string evidence, bool ownerReview)
        {
            Mind = mind;
            Rule = rule;
            Evidence = evidence;
            OwnerReview = ownerReview;
        }

        internal TurnUndeadMind Mind { get; private set; }
        /// <summary>Stable rule code; persisted with the effect for diagnostics.</summary>
        internal string Rule { get; private set; }
        internal string Evidence { get; private set; }
        /// <summary>
        /// True when the decision rests on a documented default that the owner
        /// has not yet confirmed. The decision is still deterministic.
        /// </summary>
        internal bool OwnerReview { get; private set; }
        internal bool ReceivesRepeatSaves { get { return Mind == TurnUndeadMind.Intelligent; } }
    }

    /// <summary>
    /// Reviewed intelligent/mindless inventory for supported native undead.
    ///
    /// Kingmaker does not encode tabletop mindlessness: ordinary skeletons and
    /// zombies have Intelligence 10, zombie lords share the ordinary zombie's
    /// unit type and AI brain, and skeletal champions share the ordinary
    /// skeleton's unit type. Intelligence, names and brains are therefore never
    /// consulted. The rules, in order:
    ///   1. explicit unit-blueprint exceptions;
    ///   2. intelligent creature-identity facts the unit carries;
    ///   3. the unit type's tabletop creature family;
    ///   4. anything else (named undead, liches, vampires, ghosts, third-party
    ///      content) is treated as intelligent, matching the native
    ///      every-round save, and reported as unclassified.
    /// </summary>
    internal static class TurnUndeadClassificationPolicy
    {
        internal const string RuleUnitException = "unit-exception";
        internal const string RuleIntelligentFact = "intelligent-identity-fact";
        internal const string RuleMindlessFamily = "mindless-family";
        internal const string RuleIntelligentFamily = "intelligent-family";
        internal const string RuleUnclassified = "unclassified-default-intelligent";

        // Native BlueprintUnitType identities.
        internal const string SkeletonType = "b5c30928a9b9f284c8f5cbe7aaee58a6";
        internal const string ZombieType = "0bd298db2bfec6b409c5ca90cd3c8c4c";
        internal const string CyclopZombieType = "e2fc4028eca4b40448e0339964d93b87";
        internal const string PhantomArmorType = "fc9f65bc1d1d65b46bb71a380da08f49";
        internal const string GhostType = "4d34f9d7d9b770843958bd5fe2060c9d";
        internal const string SpectreType = "446e7ea93d8c5a74aae1e7132114c94a";
        internal const string DevourerType = "285140dc460043145b512dabb71cbe6b";

        private sealed class FamilyRule
        {
            internal FamilyRule(TurnUndeadMind mind, string name)
            { Mind = mind; Name = name; }
            internal TurnUndeadMind Mind { get; private set; }
            internal string Name { get; private set; }
        }

        private sealed class UnitException
        {
            internal UnitException(TurnUndeadMind mind, string name, bool ownerReview,
                string evidence)
            { Mind = mind; Name = name; OwnerReview = ownerReview; Evidence = evidence; }
            internal TurnUndeadMind Mind { get; private set; }
            internal string Name { get; private set; }
            internal bool OwnerReview { get; private set; }
            internal string Evidence { get; private set; }
        }

        private static readonly Dictionary<string, FamilyRule> Families =
            new Dictionary<string, FamilyRule>(StringComparer.Ordinal)
        {
            // Bestiary skeleton, zombie and phantom armor: Int —.
            { SkeletonType, new FamilyRule(TurnUndeadMind.Mindless, "Skeleton") },
            { ZombieType, new FamilyRule(TurnUndeadMind.Mindless, "Zombie") },
            { CyclopZombieType, new FamilyRule(TurnUndeadMind.Mindless, "CyclopZombie") },
            { PhantomArmorType, new FamilyRule(TurnUndeadMind.Mindless, "PhantomArmor") },
            // Incorporeal and devourer families have Intelligence scores.
            { GhostType, new FamilyRule(TurnUndeadMind.Intelligent, "Ghost") },
            { SpectreType, new FamilyRule(TurnUndeadMind.Intelligent, "Spectre") },
            { DevourerType, new FamilyRule(TurnUndeadMind.Intelligent, "Devourer") },
        };

        private const string ChampionEvidence =
            "Skeletal champion stat block (UndeadClass with Fighter/Barbarian levels); Bestiary skeletal champion has Int 9 and is not mindless; shares the ordinary Skeleton unit type.";

        private static readonly Dictionary<string, UnitException> Units =
            new Dictionary<string, UnitException>(StringComparer.Ordinal)
        {
            // Skeletal champions and a graveknight inside the Skeleton unit type.
            { "622b8701c4f2468479b9f533c2cc24e5", Intelligent("CR11_SkeletalChampion", ChampionEvidence) },
            { "a24e752e4748bd548936020938effee1", Intelligent("CR11_SkeletalChampionArcher", ChampionEvidence) },
            { "6c3d554093121ad4a93718f0ce7926d6", Intelligent("CR14_SkeletalChampionBarbarian", ChampionEvidence) },
            { "f2917899f6976fc4ebc16724a592d0b7", Intelligent("CR2_SkeletalChampion", ChampionEvidence) },
            { "10aee45d7dd944e4794edeb02c778d9f", Intelligent("CR2_SkeletalChampionArcher", ChampionEvidence) },
            { "17984933f9f18a94bbcb697eac547c5a", Intelligent("CR2_SkeletalChampionDagger", ChampionEvidence) },
            { "0355a499626e5f446bac65d9340a964c", Intelligent("CR2_SkeletalChampionTwoHanded", ChampionEvidence) },
            { "ab45315edcea08f45bb309e0fd3aed3f", Intelligent("CR2_SkeletalChampionUnarmed", ChampionEvidence) },
            { "6f0122a6ec507dc49b217b75c42c5296", Intelligent("CR3_SkeletalChampion", ChampionEvidence) },
            { "c2e12b0da7b330a418d6cc46a8c88d26", Intelligent("CR4_SkeletalChampionArcher", ChampionEvidence) },
            { "66972515a74e62140a12abbf0d66f4df", Intelligent("CR5_SkeletalChampion", ChampionEvidence) },
            { "cb93d5b441ab9514aaf620e8ebacc824", Intelligent("CR5_SkeletalChampionNoShield", ChampionEvidence) },
            { "5787e01fea3a2fd45b60eed5dd0ab284", Intelligent("CR5_SkeletalChampionTwoHanded", ChampionEvidence) },
            { "b348af45b8ef0dd4086d9a8a50439a37", Intelligent("CR5_SkeletalChampionUnarmed", ChampionEvidence) },
            { "49903237b0c90b445ac1089dc89a7665", Intelligent("CR7_SkeletalChampion", ChampionEvidence) },
            { "9928642aa0612434bbb23f478dbbf988", Intelligent("CR7_SkeletalChampionArcher", ChampionEvidence) },
            { "3ebabb487a3c2e04eb988bc28d897fea", Intelligent("CR7_SkeletalChampionBarbarian", ChampionEvidence) },
            { "7a48a249f5d5d7549b90d0d45615efb1", Intelligent("SkeletalChampion_Greataxe", ChampionEvidence) },
            { "cd808b4f0665b834cb8a34b03441a312", Intelligent("SkeletalChampion_Tank", ChampionEvidence) },
            { "698ac20e04ce397469de7b5aa62ddbb2", Intelligent("SkeletalGuest1", ChampionEvidence) },
            { "c5715025b636519448cb278a3a2c5fa1", Intelligent("SkeletalGuest2", ChampionEvidence) },
            { "b2ade2a85518eac4c815a0a44fb74ce2", Intelligent("SkeletalGuest3", ChampionEvidence) },
            { "53e228ba7fe18104c93dc4b7294a1b30", Intelligent("SummonedSkeletonChampion", ChampionEvidence) },
            { "209f78dd464984c40b1eb43fabdab5a0", Intelligent("SkeletonSummoned_ch5", ChampionEvidence) },
            { "4050be4512d245f40bf9461074b672f4", new UnitException(TurnUndeadMind.Intelligent,
                "SkeletonSummoned", true,
                "Summoned skeleton uses the skeletal champion stat block (Int 9, Fighter levels); classified by that stat block. Owner review: confirm that the summoning spell's tabletop mindless skeleton is not the intended identity.") },
            { "7237a32613fe55e479d1141682f2bbd4", Intelligent("CR11_GraveknightSummoned",
                "Graveknight (Int 15 tabletop) carried under the Skeleton unit type.") },
            // Dread zombie cyclopes outside the CyclopZombie unit type.
            { "b421201cd8ab9af4a933602aa0786500", new UnitException(TurnUndeadMind.Mindless,
                "ZombieLairDreadZombieCyclop", false,
                "Dread zombie cyclops (zombie family) registered under the living Cyclop unit type.") },
            { "ce2d24dec712e094d94c336053abeb57", new UnitException(TurnUndeadMind.Mindless,
                "CR14_DreadZombieCyclopGreat", false,
                "Greater dread zombie cyclops (zombie family) with no unit type.") },
            // Documented defaults inside mindless families awaiting owner review.
            { "976d4937eb2620c4baf465bb6c7a00a7", Review("CR0.5_SkeletonMage",
                "Ordinary skeleton family stat block that casts spells; kept with its family (mindless) until the owner confirms.") },
            { "f68391521a2175647ab311fc64108574", Review("CR13_BloodyBonesBarbarian",
                "Bloody skeleton variant (Bestiary: mindless) with Barbarian levels; kept with its family.") },
            { "39ace3bd77efd5546ad85b9e9947a311", Review("LoneWarrior",
                "Named skeleton encounter with Barbarian levels and no intelligent identity fact; kept with its family.") },
            { "fe662d20a0272bb4ea66bef675b4b52d", Review("CR12_DreadZombieCyclopCleric",
                "Dread zombie cyclops priest that casts cleric spells; kept with the zombie family until the owner confirms.") },
            { "113ee4680b60b974696313c00b29c4aa", Review("DLC2Village_DreadZombieCyclopCleric",
                "Dread zombie cyclops priest that casts cleric spells; kept with the zombie family until the owner confirms.") },
            { "2e286f14e73b385499551761afa66ce1", Review("UndeadCyclopsPriest (2e286f14)",
                "Dread zombie cyclops priest that casts cleric spells; kept with the zombie family until the owner confirms.") },
            { "f0a4efcaa7173bd43ba7fcf1f576e484", Review("UndeadCyclopsPriest (f0a4efca)",
                "Dread zombie cyclops priest that casts cleric spells; kept with the zombie family until the owner confirms.") },
        };

        /// <summary>
        /// Creature-identity facts that make a member of a mindless unit type
        /// intelligent: the zombie lord feature lists (zombie lords, the
        /// spellcasting Dunsward tomb zombies, Davik Nettle) and the ghoul list.
        /// </summary>
        private static readonly Dictionary<string, string> IntelligentFacts =
            new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "6abb5d86a51793045ae32fcd96127e5e", "BigNarlmarchesGhoulFeatureListLevel" },
            { "655d1fc40de94904094f10af75fe7318", "ZombieLordArcherFeatureListLevel1" },
            { "3bcf6eaac789b6146a602cad9cc7446f", "ZombieLordArcherFeatureListLevel2" },
            { "b0e40546c4fb48f44b788908fdaca907", "ZombieLordArcherFeatureListLevel3" },
            { "ba68652433f32a948b941bdd2115129b", "ZombieLordArcherFeatureListLevel4" },
            { "8c22ccb6425d07943837751256015989", "ZombieLordArcherFeatureListLevel5" },
            { "3bda002ccb105aa469a0e01ae2e11427", "ZombieLordArcherFeatureListLevel6" },
            { "a6cd015741259a049a8a70a408acdf16", "ZombieLordArcherFeatureListLevel7" },
            { "faf27726cc4e0ce489fd6b7d6fb10f9e", "ZombieLordArcherFeatureListLevel8" },
            { "975643c6586026c4db2efe7efdc2becc", "ZombieLordBarbarianFeatureListLevel1" },
            { "e99188610fdc00d47af975b9a28a56c6", "ZombieLordBarbarianFeatureListLevel2" },
            { "906ea87ac5d4d5d4ca58d82039c0cd54", "ZombieLordBarbarianFeatureListLevel3" },
            { "734b8141ca9ac75438767d5ae7ab1143", "ZombieLordBarbarianFeatureListLevel4" },
            { "b31ef4916f6e16e4296ad609f487afde", "ZombieLordBarbarianFeatureListLevel5" },
            { "5149a319c33d01a4daf72cf81114b1e4", "ZombieLordBarbarianFeatureListLevel6" },
            { "5038c927e1396e3459d9540497cbe656", "ZombieLordBarbarianFeatureListLevel7" },
            { "41c4bfb85861c624d9dccdee90f3a222", "ZombieLordBarbarianFeatureListLevel8" },
            { "b9cd62be74309cc49a43a891b75bdd2b", "ZombieLordClericFeatureListLevel1" },
            { "6413656773f0f2644b2aa32f1e51a9eb", "ZombieLordClericFeatureListLevel2" },
            { "d86ac648547fa7744892980f4a1b3cd0", "ZombieLordClericFeatureListLevel3" },
            { "9d2900e85691cb04d905407d0f92e1d5", "ZombieLordClericFeatureListLevel4" },
            { "7c4867e5c4cf9134b8436a1ec04a921b", "ZombieLordClericFeatureListLevel5" },
            { "577271f2ce5f067488bdb1ca0967d160", "ZombieLordClericFeatureListLevel6" },
            { "84e3ddf9669346f449b5a32becb4725a", "ZombieLordClericFeatureListLevel7" },
            { "3994e8f72c0a51642b48f34c6688a8dd", "ZombieLordClericFeatureListLevel8" },
            { "10967aafcc7ff304d83833146d0f4308", "ZombieLordFighterFeatureListLevel1" },
            { "d6348e3efe5964f43b9463b056d660d0", "ZombieLordFighterFeatureListLevel2" },
            { "e064fd1ed1af7f542ab4acbb1867ae27", "ZombieLordFighterFeatureListLevel3" },
            { "950f51e60a09ce34e9c3c5cd53a73d12", "ZombieLordFighterFeatureListLevel4" },
            { "705517bdc60f625499ff9f5734aeb02d", "ZombieLordFighterFeatureListLevel5" },
            { "8d6aebbe21594ba4895e346790f6cd21", "ZombieLordFighterFeatureListLevel5_Handaxe" },
            { "c89d4f7c8b4c3d448ba1ad8b287c950f", "ZombieLordFighterFeatureListLevel6" },
            { "667d4dd6931a6a24284836f25d125c3a", "ZombieLordFighterFeatureListLevel6_Shortspear" },
            { "8464aa8993bc72343918be0e0c42874a", "ZombieLordFighterFeatureListLevel7" },
            { "1343404320e435046b90551a03018e9a", "ZombieLordFighterFeatureListLevel7_Battleaxe" },
            { "56f495ea1258754439c581fb763f07da", "ZombieLordFighterFeatureListLevel7_Shortspear" },
            { "97db9c223d908ce42a26e2ed04b54129", "ZombieLordFighterFeatureListLevel8" },
            { "09ad0a31eb4b49840a229a399fa4bc85", "ZombieLordWizardFeatureListLevel1" },
            { "cdf25738097ac6e4aa30add96f4e39e7", "ZombieLordWizardFeatureListLevel2" },
            { "ad9423d93b8d05d4fbaf497d65e20e08", "ZombieLordWizardFeatureListLevel3" },
            { "5452bf75e9dd9864893dbe7a36b269dc", "ZombieLordWizardFeatureListLevel4" },
            { "2a7fe50795cd2cd418da7586c8a0324e", "ZombieLordWizardFeatureListLevel5" },
            { "d5921e2d588e361498030952bd5f1c63", "ZombieLordWizardFeatureListLevel6" },
            { "8d2a915f5ebfb4244be3eba8c839cd61", "ZombieLordWizardFeatureListLevel7" },
            { "1495a07abb198e54a9b31188c9c6ec47", "ZombieLordWizardFeatureListLevel8" },
        };

        internal static IReadOnlyCollection<string> UnitExceptionGuids
        { get { return Units.Keys; } }

        internal static IReadOnlyCollection<string> IntelligentFactGuids
        { get { return IntelligentFacts.Keys; } }

        internal static IReadOnlyCollection<string> FamilyTypeGuids
        { get { return Families.Keys; } }

        internal static TurnUndeadClassification Classify(TurnUndeadUnitRecord unit)
        {
            if (unit == null) throw new ArgumentNullException("unit");
            UnitException exception;
            if (Units.TryGetValue(unit.UnitBlueprintGuid, out exception))
                return new TurnUndeadClassification(exception.Mind, RuleUnitException,
                    exception.Name + ": " + exception.Evidence, exception.OwnerReview);
            string marker = unit.FactGuids.Where(IntelligentFacts.ContainsKey)
                .OrderBy(value => value, StringComparer.Ordinal).FirstOrDefault();
            if (marker != null)
                return new TurnUndeadClassification(TurnUndeadMind.Intelligent,
                    RuleIntelligentFact, IntelligentFacts[marker], false);
            FamilyRule family;
            if (Families.TryGetValue(unit.UnitTypeGuid, out family))
                return new TurnUndeadClassification(family.Mind,
                    family.Mind == TurnUndeadMind.Mindless ? RuleMindlessFamily :
                        RuleIntelligentFamily, family.Name, false);
            return new TurnUndeadClassification(TurnUndeadMind.Intelligent,
                RuleUnclassified, "unitType=" + (unit.UnitTypeGuid.Length == 0 ?
                    "<none>" : unit.UnitTypeGuid), false);
        }

        private static UnitException Intelligent(string name, string evidence)
        { return new UnitException(TurnUndeadMind.Intelligent, name, false, evidence); }

        private static UnitException Review(string name, string evidence)
        { return new UnitException(TurnUndeadMind.Mindless, name, true, evidence); }
    }
}
