using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Sprint 4: summon weapons that carried creature reach in their weapon
    /// type's raw range.
    ///
    /// The installed contract:
    /// - BlueprintWeaponType.AttackRange returns max(2, raw - 4) feet for
    ///   raw ranges up to 10 feet, and the raw value above that.
    /// - ItemEntityWeapon.AttackRange adds the wielder's ReachRange.
    /// - ProperFlanking2 treats any melee weapon whose type range exceeds
    ///   GameConsts.MinWeaponRange (2 ft) as a reach weapon, and refuses
    ///   adjacent targets inside (size reach + 1) ft x 0.5 beyond both
    ///   bodies.
    ///
    /// So a natural attack, or an ordinary manufactured weapon, whose type
    /// carried the creature's reach lost its legal adjacent attack. Pathfinder
    /// gives that restriction only to reach weapons (glaive, guisarme, lance,
    /// longspear, ranseur, whip); natural attacks reach any creature within
    /// the creature's reach.
    ///
    /// Each row below stores the ordinary 5-ft raw range (2 ft native, not a
    /// ProperFlanking2 reach weapon). The exact owner and item get back, as an
    /// item-level range correction, exactly what the type range lost. The
    /// live wielded range of every row is therefore unchanged. The
    /// correction is not applied where an existing absolute item correction
    /// already sets the range (the Huge giants).
    ///
    /// Genuine reach weapons are deliberately absent and keep their dead
    /// zone: the Bearded Devil's glaive and the Babau's longspear.
    /// </summary>
    internal static class SummonReachRepresentation4
    {
        internal const int StoredRawFeet = 5;
        internal const int NativeMinWeaponRangeFeet = 2;

        internal const string Natural = "natural";
        // A natural attack with a printed per-attack extension, e.g.
        // "Reach 5 ft. (10 ft. with tail)". Not a reach weapon.
        internal const string NaturalExtended = "natural-extended";
        internal const string OrdinaryManufactured = "ordinary-manufactured";

        internal sealed class Row
        {
            internal Row(string ownerKey, string owner, string item, string itemName, int previousRawFeet,
                string kind, int ownerReachRangeFeet, int existingAddedFeet = 0, int? existingAbsoluteFeet = null)
            {
                OwnerKey = ownerKey; Owner = owner; Item = item; ItemName = itemName;
                PreviousRawFeet = previousRawFeet; Kind = kind; OwnerReachRangeFeet = ownerReachRangeFeet;
                ExistingAddedFeet = existingAddedFeet; ExistingAbsoluteFeet = existingAbsoluteFeet;
            }
            internal string OwnerKey { get; private set; }
            internal string Owner { get; private set; }
            internal string Item { get; private set; }
            internal string ItemName { get; private set; }
            internal int PreviousRawFeet { get; private set; }
            internal string Kind { get; private set; }
            // Source-derived: size ReachRange plus reach-stat bonuses, as
            // read by the s4c census of installed blueprints.
            internal int OwnerReachRangeFeet { get; private set; }
            // Existing exact-item corrections, unchanged by this sprint.
            internal int ExistingAddedFeet { get; private set; }
            internal int? ExistingAbsoluteFeet { get; private set; }
            internal int CorrectionFeet
            {
                get
                {
                    return ExistingAbsoluteFeet.HasValue ? 0 :
                        NativeTypeRangeFeet(PreviousRawFeet) - NativeTypeRangeFeet(StoredRawFeet);
                }
            }
            internal int LiveRangeBeforeFeet { get { return LiveRangeFeet(PreviousRawFeet, 0); } }
            internal int LiveRangeAfterFeet { get { return LiveRangeFeet(StoredRawFeet, CorrectionFeet); } }
            private int LiveRangeFeet(int raw, int correction)
            {
                return ExistingAbsoluteFeet ?? NativeTypeRangeFeet(raw) + OwnerReachRangeFeet + ExistingAddedFeet + correction;
            }
        }

        internal static int NativeTypeRangeFeet(int raw)
        {
            return raw > 10 ? raw : Math.Max(NativeMinWeaponRangeFeet, raw - 4);
        }

        internal static bool ProperFlanking2ReachWeapon(bool melee, int raw)
        {
            return melee && NativeTypeRangeFeet(raw) > NativeMinWeaponRangeFeet;
        }

        // Owner and item asset GUIDs, item names and owner reach as read by
        // the s4c census (run 20261106T2150584304850Z) of installed
        // blueprints. Existing corrections are from source.
        internal static readonly Row[] Rows = {
            // Small/Medium natural attacks and ordinary weapons: raw 9 gave
            // the project's five-foot live reach.
            new Row("lemure", "80fd27e965b943b5b307a8c0a45cfb96", "780c5819371c4b5b88d72d12fc43a9f2", "KMG_Summoning_OutsiderCelestial_Lemure_Claw1d4", 9, Natural, 0),
            new Row("dretch", "717d3fdec4284e42b21efe83bc03eb25", "ae9975afa8884bd98da9f8788b365acc", "KMG_Summoning_OutsiderCelestial_Dretch_Claw1d4", 9, Natural, 0),
            new Row("dretch", "717d3fdec4284e42b21efe83bc03eb25", "266db5aebcae435a86956cd545e7429d", "KMG_Summoning_OutsiderCelestial_Dretch_Bite1d4", 9, Natural, 0),
            new Row("hound-archon", "4685038956fc47c7b18d5acf1d7d8d7b", "ca5f1f56746b4473b3fefdf1e3edbb5e", "KMG_Summoning_OutsiderCelestial_HoundArchon_Slam1d4", 9, Natural, 0),
            new Row("hound-archon", "4685038956fc47c7b18d5acf1d7d8d7b", "b7c358cbe55a45fda087f4fefd968122", "KMG_Summoning_OutsiderCelestial_HoundArchon_Bite1d8", 9, Natural, 0),
            new Row("hound-archon", "4685038956fc47c7b18d5acf1d7d8d7b", "99fa49115f9c48228e13ca9f2879e6e0", "KMG_Summoning_OutsiderCelestial_HoundArchon_Greatsword2d6", 9, OrdinaryManufactured, 0),
            new Row("trumpet-archon", "b6359294f13a4adab626d14c5ededd50", "1182d9ef39c443f5b4ffda85ae1e57b2", "KMG_Summoning_OutsiderCelestial_TrumpetArchon_Greatsword2d6", 9, OrdinaryManufactured, 0),
            new Row("astral-deva", "2814e9aa55e34dda8f3322d84a5fe5e2", "d9d766472a9d4c168555e48a59f39e9b", "KMG_Summoning_OutsiderCelestial_AstralDeva_Warhammer1d8", 9, OrdinaryManufactured, 0),
            new Row("astral-deva", "2814e9aa55e34dda8f3322d84a5fe5e2", "1e3e03e0a8094a838077e3e3839d6dff", "KMG_Summoning_OutsiderCelestial_AstralDeva_Slam1d8", 9, Natural, 0),
            new Row("bearded-devil", "182032f380314a7581d0016c443945a2", "5e14ac33c7d1442ab29285bb3901b66f", "KMG_Summoning_Infernal_BeardedDevil_Claw", 9, Natural, 0),
            new Row("barbed-devil", "a8ed4e16ec4b48a5bdfdc47f7f1b9cd0", "c4743cbf5fb7435eb7cedb8f103d6ee5", "KMG_Summoning_Infernal_BarbedDevil_Claw", 9, Natural, 0),
            new Row("babau", "1512476cf6674704b6b0bcc3b67b48d9", "eb97ebcb9cf241a2a930d366173b6295", "KMG_Summoning_Infernal_Babau_Bite", 9, Natural, 0),
            new Row("babau", "1512476cf6674704b6b0bcc3b67b48d9", "56f9470894df4fe7b6e5cf57610191fb", "KMG_Summoning_Infernal_Babau_Claw", 9, Natural, 0),
            // Medium natural attacks with a printed per-attack extension.
            new Row("salamander", "f8fb103168d74b4c93182437e5d2b4e4", "93e097b8d3db42d3a37656502899e1a9", "KMG_Summoning_Special_Salamander_Tail", 10, NaturalExtended, 0),
            new Row("kyton-chain-devil", "96045f5c18ef4f4396a6c466a2be1a92", "00d62f48e15e4b8ca19f480fd8dd07b1", "KMG_Summoning_Infernal_KytonChainDevil_Chain", 10, NaturalExtended, 0, 4),
            // Large: raw 10 plus the Large owner's four-foot ReachRange gave
            // ten feet.
            new Row("hill-giant", "58770207fd8c4084a0d9addbac591616", "2b821e28b95d40f197f5fb847008b93f", "KMG_Summoning_Giant_Greatclub2d8", 10, OrdinaryManufactured, 4),
            new Row("hill-giant", "58770207fd8c4084a0d9addbac591616", "fe074f0010824f9cb2ab6864469e1852", "KMG_Summoning_Giant_Slam1d8", 10, Natural, 4),
            new Row("stone-giant", "956501547dd54479b329a315aab07a05", "2b821e28b95d40f197f5fb847008b93f", "KMG_Summoning_Giant_Greatclub2d8", 10, OrdinaryManufactured, 4),
            new Row("stone-giant", "956501547dd54479b329a315aab07a05", "fe074f0010824f9cb2ab6864469e1852", "KMG_Summoning_Giant_Slam1d8", 10, Natural, 4),
            new Row("fire-giant", "65481592185641c99c502e21ba9a5fa2", "d33330230bc444a98c7f7b0b81778f1d", "KMG_Summoning_Giant_Greatsword3d6", 10, OrdinaryManufactured, 4),
            new Row("fire-giant", "65481592185641c99c502e21ba9a5fa2", "fe074f0010824f9cb2ab6864469e1852", "KMG_Summoning_Giant_Slam1d8", 10, Natural, 4),
            new Row("ettin", "2481bc73abd64ef0ae581b48438b5404", "940159c86d414820889994ca9c7fea42", "KMG_Summoning_Giant_Sprint29_EttinMainFlail2d6", 10, OrdinaryManufactured, 4),
            new Row("ettin", "2481bc73abd64ef0ae581b48438b5404", "35fa914ef66a4b69991e088215bf5854", "KMG_Summoning_Giant_Sprint29_EttinOffFlail2d6", 10, OrdinaryManufactured, 4),
            new Row("lillend-azata", "71a8adf916b4409680023e853d28583b", "f2a606d19b0240abb62255da5e4e1973", "KMG_Summoning_FeyCelestial_LillendAzata_Longsword", 10, OrdinaryManufactured, 4),
            new Row("lillend-azata", "71a8adf916b4409680023e853d28583b", "50d93d8a75d34b3787183363f7ec25c9", "KMG_Summoning_FeyCelestial_LillendAzata_Tail", 10, Natural, 4),
            new Row("hezrou", "0c97e35a3e8e4d2db937a23775ea13ac", "26fb64d30ac242bc9200d582b74dd9c0", "KMG_Summoning_Infernal_Hezrou_Bite", 10, Natural, 4),
            new Row("hezrou", "0c97e35a3e8e4d2db937a23775ea13ac", "bb889c692fad42fd87857b71dc75fdbc", "KMG_Summoning_Infernal_Hezrou_Claw", 10, Natural, 4),
            new Row("vrock", "5f4aa097fc3e4fa4a627ce5d5be6509e", "3f2836b9353f4b1b9e722b98070feaf4", "KMG_Summoning_Demon_Vrock_Bite", 10, Natural, 4),
            new Row("vrock", "5f4aa097fc3e4fa4a627ce5d5be6509e", "ded2d13a407e41088ac0845f257f6560", "KMG_Summoning_Demon_Vrock_Claw", 10, Natural, 4),
            new Row("vrock", "5f4aa097fc3e4fa4a627ce5d5be6509e", "77976b057df64b959866ddf0e82a257a", "KMG_Summoning_Demon_Vrock_Talon", 10, Natural, 4),
            new Row("ice-devil", "6b5b789a08584fab84a00e7da42e1d2c", "a401bfb28e394f06a2fce8b89e91ebd8", "KMG_Summoning_Infernal_IceDevil_Spear", 10, OrdinaryManufactured, 4),
            new Row("ice-devil", "6b5b789a08584fab84a00e7da42e1d2c", "d324d22032f14392aa44aad0472cc0de", "KMG_Summoning_Infernal_IceDevil_Bite", 10, Natural, 4),
            new Row("ice-devil", "6b5b789a08584fab84a00e7da42e1d2c", "7f6ab83598e047649f3f47ae813ff8b0", "KMG_Summoning_Infernal_IceDevil_Tail", 10, Natural, 4),
            // Huge: raw 15 is returned unchanged, plus the Huge owner's
            // seven-foot ReachRange. The giants already carry an absolute
            // exact-item correction to 15 ft, so only their stored raw range
            // changes.
            new Row("glabrezu", "3824e7589ae548f79a3eb8369f50359f", "d44165bbdcad4ebf8ae6f3b5454e86cf", "KMG_Summoning_Demon_Glabrezu_Bite", 15, Natural, 7),
            new Row("glabrezu", "3824e7589ae548f79a3eb8369f50359f", "7af8a097a3594b53b8b40c05881146db", "KMG_Summoning_Demon_Glabrezu_Pincer", 15, Natural, 7),
            new Row("glabrezu", "3824e7589ae548f79a3eb8369f50359f", "d984a1fbac854b0bb1b0f1e0ca70a606", "KMG_Summoning_Demon_Glabrezu_Claw", 15, Natural, 7),
            new Row("nalfeshnee", "9930f78084f0fe78d1de849aed58b753", "e061d98c5542b5625a83daaef4c86076", "KMG_Summoning_Demon_Nalfeshnee_Bite", 15, Natural, 7),
            new Row("nalfeshnee", "9930f78084f0fe78d1de849aed58b753", "4038f0c02ba5614ee0aa706053631d55", "KMG_Summoning_Demon_Nalfeshnee_Claw", 15, Natural, 7),
            new Row("cloud-giant", "ff4696c6336141a3b2236cb0a22c86f3", "aba8243c429b4e0a938120a20db285ca", "KMG_Summoning_Giant_Sprint29_CloudMorningstar4d6", 15, OrdinaryManufactured, 7, 0, 15),
            new Row("cloud-giant", "ff4696c6336141a3b2236cb0a22c86f3", "bb9d1aa6dad74a56ba71b887a31172bb", "KMG_Summoning_Giant_CloudSlam2d6", 15, Natural, 7, 0, 15),
            new Row("storm-giant", "4b1143140c614a0aaaa1118c8a44c9ae", "240530e94b9647f09ca18dcb2e70b15a", "KMG_Summoning_Giant_Sprint29_StormGreatsword4d6", 15, OrdinaryManufactured, 7, 0, 15),
            new Row("storm-giant", "4b1143140c614a0aaaa1118c8a44c9ae", "12bff2b66e674023a75057d7face1ca6", "KMG_Summoning_Giant_Sprint29_StormSlam2d6", 15, Natural, 7, 0, 15),
            // Huge/Gargantuan dinosaur tails: raw 9 plus the owner's size
            // reach and its existing chassis Reach bonus.
            new Row("ankylosaurus", "10c80d5cfa594332bd3e5127799e426f", "15394605e1664a51bce4b50f38a7603a", "KMG_Summoning_Natural_Tail3d6", 9, Natural, 10),
            new Row("stegosaurus", "39cab5be4dc148339f4ce8b0157bd716", "96b5e8692e3949bcba63b57507a5075f", "KMG_Summoning_Natural_Stegosaurus_Tail4d6", 9, Natural, 10),
            new Row("brachiosaurus", "9093aef39ecc416092ef68655a7e56fb", "13b51070d6f84a15bfed98a04c98fa30", "KMG_Summoning_Natural_Brachiosaurus_Tail4d6", 9, Natural, 15),
        };

        private static readonly Dictionary<string, Row> ByOwnerItem =
            Rows.ToDictionary(row => row.Owner + "|" + row.Item, StringComparer.Ordinal);

        internal static Row Find(string ownerGuid, string itemGuid)
        {
            Row row;
            return ownerGuid != null && itemGuid != null &&
                ByOwnerItem.TryGetValue(ownerGuid + "|" + itemGuid, out row) ? row : null;
        }

        /// <summary>The feet to add to the exact owner's exact item's live
        /// range. Zero for every other owner, item, or absolute-corrected
        /// row. Stateless: computed from blueprint identity on each read, so
        /// reconstruction or reload cannot accumulate it.</summary>
        internal static int CorrectionFeet(string ownerGuid, string itemGuid)
        {
            Row row = Find(ownerGuid, itemGuid);
            return row == null ? 0 : row.CorrectionFeet;
        }
    }
}
