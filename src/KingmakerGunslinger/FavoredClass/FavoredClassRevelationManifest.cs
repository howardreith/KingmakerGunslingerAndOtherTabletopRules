using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>One selectable oracle revelation target (I06/S04).</summary>
    internal sealed class FavoredClassRevelationTarget
    {
        internal FavoredClassRevelationTarget(string key, string title, string mystery,
            string[] featureGuids, string families, string heldBack, string[] extraRoots,
            string[] excludedRanks)
        {
            Key = key;
            Title = title;
            Mystery = mystery;
            FeatureGuids = featureGuids;
            Families = families;
            HeldBack = heldBack;
            ExtraRoots = extraRoots ?? new string[0];
            ExcludedRanks = excludedRanks ?? new string[0];
        }

        /// <summary>Stable symbol segment.</summary>
        internal string Key { get; private set; }
        internal string Title { get; private set; }
        internal string Mystery { get; private set; }

        /// <summary>The selectable revelation feature(s) (Dragon revelations: one per colour).</summary>
        internal string[] FeatureGuids { get; private set; }

        /// <summary>Audited adapter families: A rank configs, B resources, C caster level and DC.</summary>
        internal string Families { get; private set; }

        /// <summary>
        /// Player-facing text for what stays at the actual oracle level: the
        /// revelation's feature-granting level gates (or its possession BAB);
        /// null when every level-based value of the revelation scales.
        /// </summary>
        internal string HeldBack { get; private set; }

        /// <summary>
        /// Further blueprints that execute this revelation's own effect but
        /// are granted by other feats (the Life channel's derived channels).
        /// </summary>
        internal string[] ExtraRoots { get; private set; }

        /// <summary>"blueprint guid|rank type" reads the charter excludes (the possession BAB).</summary>
        internal string[] ExcludedRanks { get; private set; }

        internal bool HasFamily(char family)
        {
            return Families.IndexOf(family) >= 0;
        }
    }

    /// <summary>
    /// The signed-off-for-review I06/S04 target manifest: every revelation the
    /// read-only audit of the installed Call of the Wild 1.14.4c-2.1 Oracle
    /// marked implementable, generated from its master table (the other
    /// revelations are threshold-only, unsupported branches or have no
    /// oracle-level scaling; see docs/FAVORED-CLASS-TARGET-MANIFEST.md).
    /// </summary>
    internal static class FavoredClassRevelationManifest
    {
        /// <summary>Call of the Wild's Oracle class (optional provider).</summary>
        internal const string OracleClassGuid = "32c02466b2364c8a906e6e4761175099";

        /// <summary>Call of the Wild's Demon Hunter archetype, part of the Oracle engine's level.</summary>
        internal const string RavenerHunterArchetypeGuid = "3b3b5950e8264819b69d9aaeffe179da";

        private static readonly FavoredClassRevelationTarget[] Entries =
        {
            new FavoredClassRevelationTarget("AgingTouch", "Aging Touch", "Time",
                new[] { "74fa875c471e42709fdaeef71830e392" },
                "A,B", null,
                null, null),
            new FavoredClassRevelationTarget("RewindTime", "Rewind Time", "Time",
                new[] { "a837588e766c46d3b16bd6d9aeaf14cd" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("SpeedOrSlowTime", "Speed or Slow Time", "Time",
                new[] { "d9de6a7803ca4b87a793a04eeb066feb" },
                "B,C", null,
                null, null),
            new FavoredClassRevelationTarget("TimeFlicker", "Time Flicker", "Time",
                new[] { "eca04928555a4341aeddc28de9047eb0" },
                "B", "the displacement gained at 7th level",
                null, null),
            new FavoredClassRevelationTarget("TimeHop", "Time Hop", "Time",
                new[] { "081472d6cf334eadad43e4cd1109db3f" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("TimeSight", "Time Sight", "Time",
                new[] { "b27cb604263e4a558f7a14ace00f835f" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("EraseFromTime", "Erase From Time", "Time",
                new[] { "53dfd52a640f4f7c9d3422fcb68c1365" },
                "A,C", null,
                null, null),
            new FavoredClassRevelationTarget("BloodOfHeroes", "Blood of Heroes", "Ancestor",
                new[] { "2369597f5d1649ba80058f73bf810b3b" },
                "A,B", null,
                null, null),
            new FavoredClassRevelationTarget("PhantomTouch", "Phantom Touch", "Ancestor",
                new[] { "9129bfafff2245db9c5dabcdbc40eb29" },
                "A", null,
                null, null),
            new FavoredClassRevelationTarget("SpiritOfTheWarrior", "Spirit of the Warrior", "Ancestor",
                new[] { "8a50489e8bb640eca8b0b0d5538d69ed" },
                "B", "the base attack bonus while possessed",
                null, new[] { "90fc3e05688f4b9dad36bc7c49221d74|StatBonus" }),
            new FavoredClassRevelationTarget("SpiritShield", "Spirit Shield", "Ancestor",
                new[] { "c9309afa08da4b579b06d06ea8e144c6" },
                "A on buff, B", null,
                null, null),
            new FavoredClassRevelationTarget("StormOfSouls", "Storm of Souls", "Ancestor",
                new[] { "97be3433edfb4ebd886a8bc58d016b73" },
                "A,B,C", null,
                null, null),
            new FavoredClassRevelationTarget("SpiritWalk", "Spirit Walk", "Ancestor",
                new[] { "6b9c1edc9ee544089783b25e51d28783" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("AncestralWeapon", "Ancestral Weapon", "Ancestor",
                new[] { "e961fd3a2ff3498e8b2d0a1829c83805" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("FireBreath", "Fire Breath", "Flame",
                new[] { "6f9c65f32b754259b5f6cbbe2f23604a" },
                "A,B,C", null,
                null, null),
            new FavoredClassRevelationTarget("Firestorm", "Firestorm", "Flame",
                new[] { "ffe2ca8a935f4751abc4c5be5f608879" },
                "A on area, C", null,
                null, null),
            new FavoredClassRevelationTarget("FormOfFlame", "Form of Flame", "Flame",
                new[] { "1b5d140f7771438f9f71579296624da7" },
                "C", "the elemental body forms gained at 9th, 11th and 13th level",
                null, null),
            new FavoredClassRevelationTarget("HeatAura", "Heat Aura", "Flame",
                new[] { "2bf5045f727a4a1a9699ad74d3d65c79" },
                "A,B,C", null,
                null, null),
            new FavoredClassRevelationTarget("TouchOfFlame", "Touch of Flame", "Flame",
                new[] { "d0aa505baca24aa088baab3e8974dcd3" },
                "A", "the flaming weapon gained at 11th level",
                null, null),
            new FavoredClassRevelationTarget("Battlecry", "Battlecry", "Battle",
                new[] { "65b3530731194d5c9f04750b856db9a7" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("BattleCombatHealer", "Combat Healer", "Battle",
                new[] { "974916e9cf4b437990af8d54479c5cca" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("IronSkin", "Iron Skin", "Battle",
                new[] { "091f03b5f8964e0abfd9f5701d70c344" },
                "C", null,
                null, null),
            new FavoredClassRevelationTarget("SurprisingCharge", "Surprising Charge", "Battle",
                new[] { "a44d9fff1aff49d5a23b3704806eef53" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("Channel", "Channel", "Life",
                new[] { "98382c1e21e1444b9c5274f86a5d0ad4" },
                "A,C", null,
                new[]
                {
                    "c43c4a60188909700973dadea0fe79de",
                    "5d129abc088304c812a0ddf569f76f36",
                    "700bd49bd54608e33c6ab87640b7bb0d",
                    "e9250447c54c055b27b9bf5d89beade5",
                    "81f62ffc291403611f457b1e5f4b6263",
                    "0d53bf884ae00b312159c5ab51d07645",
                    "649d29fc343b053433792a1d24dfb57c",
                    "73300ee28baa06541d41a7a2705522f7",
                    "fdb3f9202431088c28aa2d36edd6a394",
                    "ea1ede3e9ba00bec0692a089b95c341f",
                    "9b2acd301b8007a32ebf5bb34ccbc58b",
                    "d6ff11db6f2d0814026f785f1d5da543",
                    "02041dec0b8a0a1b356c5c9885c2d363",
                    "4fd1c1077f2705ac19bc7f74d454b3ab",
                    "c62b84f1cbb144dfaad06283a002e64b",
                    "8bfe581abf1c4b688600416ff1948683",
                    "5f05542ddbbb4967b10365a8690bf0a3",
                    "12d088c6af1646d09dd34644389d906b",
                    "721c1a0a067e454c9fc9002b404b2498",
                    "3fc9c6e172d34afbb31923c711dd4450",
                    "eb32cad6167448f4841a070089423270",
                    "a6e7163d62d94743a8ca24ecd8d452b8"
                }, null),
            new FavoredClassRevelationTarget("LifeCombatHealer", "Combat Healer", "Life",
                new[] { "dce3a1e4c93b4988a3087842ffbf217f" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("EnergyBody", "Energy Body", "Life",
                new[] { "ff76b52c42a44bbe81db2990a40a0e7c" },
                "A on buff and area, B", null,
                null, null),
            new FavoredClassRevelationTarget("LifeLink", "Life Link", "Life",
                new[] { "c6405f88c806471bb73ce94d7cd824ca" },
                "B", null,
                null, null),
            new FavoredClassRevelationTarget("SpiritBoost", "Spirit Boost", "Life",
                new[] { "e2a65c3d8c7947fd9db7ac8a969e9a5a" },
                "A on feature context", null,
                null, null),
            new FavoredClassRevelationTarget("AirBarrier", "Air Barrier", "Wind",
                new[] { "5373ab7011b14643a11956d39a43de70" },
                "A,B", null,
                null, null),
            new FavoredClassRevelationTarget("Invisibility", "Invisibility", "Wind",
                new[] { "fc829c930e0c4cadb95477a5434311bc" },
                "B", "greater invisibility gained at 9th level",
                null, null),
            new FavoredClassRevelationTarget("LightningBreath", "Lightning Breath", "Wind",
                new[] { "13cf26e41a6e4e9b801462c9c1ba7207" },
                "A,B,C", null,
                null, null),
            new FavoredClassRevelationTarget("Thunderburst", "Thunderburst", "Wind",
                new[] { "86443891cfbe447db3edd97dc7a7e604" },
                "A,B,C", null,
                null, null),
            new FavoredClassRevelationTarget("TouchOfElectricity", "Touch of Electricity", "Wind",
                new[] { "23656cf3addf4154af83f9191d15dfb4" },
                "A", "the shock weapon gained at 11th level",
                null, null),
            new FavoredClassRevelationTarget("PresenceOfDragons", "Presence of Dragons", "Dragon",
                new[] { "d1620b3d94f94b1fa92608aff1d90660" },
                "B,C", null,
                null, null),
            new FavoredClassRevelationTarget("ScaledToughness", "Scaled Toughness", "Dragon",
                new[] { "bafa49bf08074214922644bdbc314cb0" },
                "A", null,
                null, null),
            new FavoredClassRevelationTarget("BreathWeapon", "Breath Weapon", "Dragon",
                new[]
                {
                    "e9c6e919fc964bd6859dfd64ba71261f",
                    "946d559bc8bb48e1b6f7b3497dbbd325",
                    "9b9c46f67fa646c89aef84da8ba8b208",
                    "9fbca8eea1ea4782a902c2c1c506ef82",
                    "e8a13ae7d7e34f6480f3b49e3c3b906e",
                    "e41f11e0cb254427b6e1def67f621786",
                    "89dd4b3e4c0f44fc9a6da346555bdb92",
                    "7fabfcc8148d412b906b8325cbbcdf96",
                    "510e96ee559e4b319119c4e7fba88c98",
                    "fef079fe621a4a64b4332f451e1a55ea"
                },
                "A,B,C", "the final breath weapon gained at 20th level",
                null, null),
            new FavoredClassRevelationTarget("FormOfTheDragon", "Form of the Dragon", "Dragon",
                new[]
                {
                    "de6246fbdba84bbd94fe3d0275f17ebd",
                    "23c346c62670480588c81835dc182b0e",
                    "3182f589a45e43148e003b4c638041fc",
                    "e16be80526b7419894bdf9aa62ff9c8b",
                    "286719c52417415d8193d52477bc5bf9",
                    "b515038a33804de9909974bd848e2242",
                    "0d042d237b17413996f9d6e43596af4c",
                    "df422ba5173c491192fbb382770580d3",
                    "90205cdc525e49f6bc1916131625ebde",
                    "5e0b1f10c63344be9f67c99e797518f8"
                },
                "A", "the forms gained at 15th and 19th level",
                null, null),
            new FavoredClassRevelationTarget("Blizzard", "Blizzard", "Waves",
                new[] { "cd6ab66c88e64a07afa268a22bbd3016" },
                "A on area, C", null,
                null, null),
            new FavoredClassRevelationTarget("IceArmor", "Ice Armor", "Waves",
                new[] { "59b405b9c7d341b7ad87ad41aa08d952" },
                "A,B", null,
                null, null),
            new FavoredClassRevelationTarget("WaterForm", "Water Form", "Waves",
                new[] { "63624232951d4f319e0c893ca43ad6ea" },
                "C", "the elemental body forms gained at 9th, 11th and 13th level",
                null, null),
            new FavoredClassRevelationTarget("WintryTouch", "Wintry Touch", "Waves",
                new[] { "7009b228a4e54502ae746d6be5e67f78" },
                "A", "the frost weapon gained at 11th level",
                null, null),
            new FavoredClassRevelationTarget("PunitiveTransformation", "Punitive Transformation", "Waves",
                new[] { "fcf3872a7dbb450c84d07efe233d58b3" },
                "A,C", null,
                null, null),
            new FavoredClassRevelationTarget("ErosionTouch", "Erosion Touch", "Nature",
                new[] { "f57946e8a2674a91abb50aa30b3a07b7" },
                "A,B", null,
                null, null),
            new FavoredClassRevelationTarget("LifeLich", "Life Lich", "Nature",
                new[] { "29dd970883e54a6a98031955f928aedc" },
                "B,C", null,
                null, null),
            new FavoredClassRevelationTarget("FormOfTheBeast", "Form of the Beast", "Nature",
                new[] { "9d9e811fc7c84f8f922ed0892d98122a" },
                "C", "the forms gained at 9th, 11th and 13th level",
                null, null),
            new FavoredClassRevelationTarget("GiftOfClawAndHorn", "Gift of Claw and Horn", "Nature",
                new[] { "44cdc6ee8b2043d6b71198b0b75f3c2c" },
                "A", "the second natural weapon gained at 11th level",
                null, null),
            new FavoredClassRevelationTarget("ArmorOfBones", "Armor of Bones", "Bones",
                new[] { "3fec5abe444240058c20369e2f5e3cb9" },
                "A,B", null,
                null, null),
            new FavoredClassRevelationTarget("BleedingWounds", "Bleeding Wounds", "Bones",
                new[] { "d1bc87ccfe3f40728c533430727ea997" },
                "A on buff", null,
                null, null),
            new FavoredClassRevelationTarget("DeathsTouch", "Death's Touch", "Bones",
                new[] { "7e5447bc523c49ccb8cc6f74422058b2" },
                "A", null,
                null, null),
            new FavoredClassRevelationTarget("RaiseTheDead", "Raise the Dead", "Bones",
                new[] { "b66c66feda9b4e0e8b9c783bc2572c08" },
                "A tier", "the ability gained at 15th level",
                null, null),
            new FavoredClassRevelationTarget("SoulSiphon", "Soul Siphon", "Bones",
                new[] { "92bf48c30024462baf6a5ee9fd6eeb94" },
                "A,B", null,
                null, null),
            new FavoredClassRevelationTarget("UndeadServitude", "Undead Servitude", "Bones",
                new[] { "2b42aa4a55254202b46d47d6289fdaf6" },
                "C", null,
                null, null),
        };

        internal static IList<FavoredClassRevelationTarget> All
        {
            get { return Array.AsReadOnly(Entries); }
        }

        internal static FavoredClassRevelationTarget For(string key)
        {
            return Entries.Single(value => string.Equals(value.Key, key, StringComparison.Ordinal));
        }
    }
}
