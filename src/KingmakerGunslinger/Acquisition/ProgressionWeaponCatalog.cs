using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Acquisition
{
    /// <summary>
    /// Weapon family of a progression catalog entry. Values are stable catalog
    /// identities; they are never serialized into saves.
    /// </summary>
    internal enum ProgressionWeaponFamily
    {
        Pistol = 0,
        Musket = 1,
        Blunderbuss = 2,
        ElvenBranchedSpear = 3,
        Wakizashi = 4,
        Katana = 5,
        Nodachi = 6
    }

    /// <summary>The feature module that owns an entry's content.</summary>
    internal enum ProgressionContentModule
    {
        Gunslinger = 0,
        EasternWeapons = 1,
        ElvenBranchedSpears = 2
    }

    /// <summary>
    /// One explicitly authorized generic magic weapon that may be stocked by an
    /// external vendor-progression integration. Eligibility is never inferred
    /// from names, descriptions, categories or library scans.
    /// </summary>
    internal sealed class ProgressionWeaponSpec
    {
        internal ProgressionWeaponSpec(string symbol, string guid,
            string internalName, string displayName,
            ProgressionWeaponFamily family, int actualEnhancement,
            bool reliable, int mundaneBaseCost, bool reusesCanonicalItem)
        {
            if (string.IsNullOrWhiteSpace(symbol) ||
                string.IsNullOrWhiteSpace(internalName) ||
                string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException(
                    "Progression weapon identity is incomplete.");
            if (!IsLowerHexGuid(guid))
                throw new ArgumentException(
                    "Progression weapon GUID must be 32 lowercase hex digits: " +
                    symbol, "guid");
            if (reliable && !ProgressionWeaponCatalog.IsFirearm(family))
                throw new ArgumentException(
                    "Reliable is a firearm-only enchantment: " + symbol);
            Symbol = symbol;
            Guid = guid;
            InternalName = internalName;
            DisplayName = displayName;
            Family = family;
            Module = ProgressionWeaponCatalog.ModuleOf(family);
            ActualEnhancement = actualEnhancement;
            Reliable = reliable;
            EquivalentBonus = MagicWeaponPricing.EquivalentBonus(
                actualEnhancement, reliable ?
                    ProgressionWeaponCatalog.ReliableEquivalentBonus : 0);
            MundaneBaseCost = mundaneBaseCost;
            Cost = MagicWeaponPricing.Cost(mundaneBaseCost, EquivalentBonus);
            ReusesCanonicalItem = reusesCanonicalItem;
        }

        internal string Symbol { get; private set; }
        internal string Guid { get; private set; }
        internal string InternalName { get; private set; }
        internal string DisplayName { get; private set; }
        internal ProgressionWeaponFamily Family { get; private set; }
        internal ProgressionContentModule Module { get; private set; }

        /// <summary>The weapon's real attack/damage enhancement bonus.</summary>
        internal int ActualEnhancement { get; private set; }

        internal bool Reliable { get; private set; }

        /// <summary>Complete package bonus used only for pricing.</summary>
        internal int EquivalentBonus { get; private set; }

        internal int MundaneBaseCost { get; private set; }
        internal int Cost { get; private set; }

        /// <summary>
        /// True when an existing canonical blueprint already is this variant
        /// and is reused unchanged; false when this change registers it.
        /// </summary>
        internal bool ReusesCanonicalItem { get; private set; }

        /// <summary>
        /// Progression milestone. Unlocking follows ACTUAL enhancement: a
        /// Reliable +N weapon unlocks with the ordinary +N weapon.
        /// </summary>
        internal int UnlockTier { get { return ActualEnhancement; } }

        internal bool ProgressionEligible { get { return true; } }

        internal bool IsFirearm
        {
            get { return ProgressionWeaponCatalog.IsFirearm(Family); }
        }

        internal string Key
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{0}.{1}.plus{2}", Family.ToString().ToLowerInvariant(),
                    Reliable ? "reliable" : "ordinary", ActualEnhancement);
            }
        }

        private static bool IsLowerHexGuid(string value)
        {
            return value != null && value.Length == 32 && value.All(
                character => character >= '0' && character <= '9' ||
                    character >= 'a' && character <= 'f');
        }
    }

    /// <summary>
    /// The explicit authorized progression catalog: Pistol, Musket and
    /// Blunderbuss ordinary +1..+5 and Reliable +1..+5, and Elven Branched
    /// Spear, Wakizashi, Katana and Nodachi ordinary +1..+5 (50 entries).
    /// The seven existing canonical +1 items are reused by identity; the other
    /// 43 variants are registered by this change. Rifles, revolvers, named,
    /// legacy, battered, cold iron, elemental and diagnostic items are
    /// deliberately absent.
    /// </summary>
    internal static class ProgressionWeaponCatalog
    {
        internal const int EntryCount = 50;
        internal const int FirearmEntryCount = 30;
        internal const int MeleeEntryCount = 20;
        internal const int ReusedEntryCount = 7;
        internal const int NewBlueprintCount = 43;
        internal const int MaximumEnhancement = 5;

        /// <summary>
        /// Reliable's existing enchantment cost: +1 equivalent for PRICE only.
        /// It never raises the weapon's actual attack/damage enhancement.
        /// </summary>
        internal const int ReliableEquivalentBonus = 1;

        /// <summary>
        /// The same Reliable sentence the authored named Reliable firearms use.
        /// </summary>
        internal const string ReliableItemDescription =
            "Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses.";

        internal const string PistolPlus1Symbol = "KMG.Firearms.PistolPlus1Item";
        internal const string MusketPlus1Symbol = "KMG.Firearms.MusketPlus1Item";
        internal const string BlunderbussPlus1Symbol =
            "KMG.Firearms.BlunderbussPlus1Item";

        // Existing saved identities. These GUIDs are reused, never replaced.
        private static readonly Dictionary<string, string> ReusedIdentities =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { PistolPlus1Symbol, "d0145d0410a34df08d68a67367c1dfc9" },
                { MusketPlus1Symbol, "3402fe01de1648b187c192500e370f01" },
                { BlunderbussPlus1Symbol, "1dc7efe0792040f187a18adfdc54c6e0" },
                { "KMG.ElvenBranchedSpear.Plus1Item",
                    "66111becd22690a2a19444a5c6bd0c7b" },
                { "KMG.EasternWeapons.Wakizashi.Plus1Item",
                    "83a507873a518b54793d0da632def246" },
                { "KMG.EasternWeapons.Katana.Plus1Item",
                    "87b3d851726a4a9abd0baec6beca957c" },
                { "KMG.EasternWeapons.Nodachi.Plus1Item",
                    "38e31ba5cdbdc668f8dcd8985070c0b7" }
            };

        // Identities registered by the Better Vendors progression change.
        // They are appended to blueprints/blueprints.json and never reused.
        private static readonly Dictionary<string, string> NewIdentities =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "KMG.Firearms.PistolPlus2Item", "b767c7389ba64eb19008fa9d66712985" },
                { "KMG.Firearms.PistolPlus3Item", "e52bc130e64a4000875eaa0f903b5ffe" },
                { "KMG.Firearms.PistolPlus4Item", "7f7d1dede53648eeb2c72b65d0a128b3" },
                { "KMG.Firearms.PistolPlus5Item", "f8e9c22e1f404215b5fbc795e3853531" },
                { "KMG.Firearms.ReliablePistolPlus1Item", "03e9ba98b2954416b93f73b7045daed9" },
                { "KMG.Firearms.ReliablePistolPlus2Item", "04b5b1bb6e3f47bd8b7b7427ba913a7b" },
                { "KMG.Firearms.ReliablePistolPlus3Item", "aff91d56deba4542b28ac1a57741a1af" },
                { "KMG.Firearms.ReliablePistolPlus4Item", "cf82e7d300eb4400a4980ddb9c4ed44e" },
                { "KMG.Firearms.ReliablePistolPlus5Item", "4a4e1316201a47aeb4b74b8d31bddb7e" },
                { "KMG.Firearms.MusketPlus2Item", "839dd0d49e7c4fbc92764475e31ee175" },
                { "KMG.Firearms.MusketPlus3Item", "6980d1bfc1d94cd19ea033ad31335c79" },
                { "KMG.Firearms.MusketPlus4Item", "3785d6ecc69542a2a45d3c3f8e80fd79" },
                { "KMG.Firearms.MusketPlus5Item", "640573a7a9f74535a1054793cf0fbfba" },
                { "KMG.Firearms.ReliableMusketPlus1Item", "ffd5c4578cce403ebb42efb161a30f43" },
                { "KMG.Firearms.ReliableMusketPlus2Item", "ae4b32641ad847bfb4dc4e257dbd6415" },
                { "KMG.Firearms.ReliableMusketPlus3Item", "e9fe300679ec44dfacf6be958ebf7b19" },
                { "KMG.Firearms.ReliableMusketPlus4Item", "14d649b4ab534ebeb02f3c9477aca192" },
                { "KMG.Firearms.ReliableMusketPlus5Item", "15393669d06e4749b0e7729ec1cbebdf" },
                { "KMG.Firearms.BlunderbussPlus2Item", "ec12481bf56a4fa4addeefafdcfb0be7" },
                { "KMG.Firearms.BlunderbussPlus3Item", "1324e5cb0c374dd6b998546e83509d20" },
                { "KMG.Firearms.BlunderbussPlus4Item", "779fede186964943bb202aeaa9a517c4" },
                { "KMG.Firearms.BlunderbussPlus5Item", "ea8803dcb9a146e5af5265e5a5816b32" },
                { "KMG.Firearms.ReliableBlunderbussPlus1Item", "8ca37dd6f1c741bdbe7f18570037699c" },
                { "KMG.Firearms.ReliableBlunderbussPlus2Item", "3d45b093ac204b4ea859c76be35ecf0d" },
                { "KMG.Firearms.ReliableBlunderbussPlus3Item", "237cdb2b9292418387674526e5aea35d" },
                { "KMG.Firearms.ReliableBlunderbussPlus4Item", "165a278175c745bd8a15bff452a0a00c" },
                { "KMG.Firearms.ReliableBlunderbussPlus5Item", "f09173f6eb12483f82c711d2532b5095" },
                { "KMG.ElvenBranchedSpear.Plus2Item", "59c1cc59146a42b0bcecd080bf418f2b" },
                { "KMG.ElvenBranchedSpear.Plus3Item", "5ee1cb27471943cea19b3e76b3e06e46" },
                { "KMG.ElvenBranchedSpear.Plus4Item", "a15696e9747d47598c0149e7f5bad740" },
                { "KMG.ElvenBranchedSpear.Plus5Item", "972a58902b9148bbbc176750a0e0b867" },
                { "KMG.EasternWeapons.Wakizashi.Plus2Item", "744ffd16fc1b4bd98bd06eb419c0ec7d" },
                { "KMG.EasternWeapons.Wakizashi.Plus3Item", "331b4668855841da933887e29adf69d9" },
                { "KMG.EasternWeapons.Wakizashi.Plus4Item", "f89d3f4ff5e14ef984be001735e3f284" },
                { "KMG.EasternWeapons.Wakizashi.Plus5Item", "31887bcf6c124135aa1e4ff828c1dc21" },
                { "KMG.EasternWeapons.Katana.Plus2Item", "8a6b48a90a4743c9a74cd65b6d063094" },
                { "KMG.EasternWeapons.Katana.Plus3Item", "cc429a9210234467acfeeb16d6859193" },
                { "KMG.EasternWeapons.Katana.Plus4Item", "9c5a6631e1ac4a25a446fd27d3e7effa" },
                { "KMG.EasternWeapons.Katana.Plus5Item", "a31923daa72d44db93ae8a30ed12f956" },
                { "KMG.EasternWeapons.Nodachi.Plus2Item", "be8ad97d3b5f479788ddb73988b4fda8" },
                { "KMG.EasternWeapons.Nodachi.Plus3Item", "f25d6a89ba364cb0874bb22e559e06bf" },
                { "KMG.EasternWeapons.Nodachi.Plus4Item", "973eb3e4d6954ba2943dae2791649b07" },
                { "KMG.EasternWeapons.Nodachi.Plus5Item", "6502156bda38474a871062a2cbefd8cb" }
            };

        private static readonly ProgressionWeaponSpec[] Entries = Build();

        /// <summary>All 50 entries in stable catalog order.</summary>
        internal static ProgressionWeaponSpec[] All
        {
            get { return (ProgressionWeaponSpec[])Entries.Clone(); }
        }

        internal static ProgressionWeaponSpec[] ForTier(int tier)
        {
            return Entries.Where(value => value.UnlockTier == tier).ToArray();
        }

        internal static ProgressionWeaponSpec RequireSymbol(string symbol)
        {
            ProgressionWeaponSpec[] matches = Entries.Where(value =>
                string.Equals(value.Symbol, symbol, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
                throw new KeyNotFoundException(
                    "No authorized progression weapon has symbol " + symbol + ".");
            return matches[0];
        }

        internal static bool TryGetByGuid(string guid,
            out ProgressionWeaponSpec spec)
        {
            spec = Entries.SingleOrDefault(value => string.Equals(value.Guid,
                guid, StringComparison.Ordinal));
            return spec != null;
        }

        internal static bool IsFirearm(ProgressionWeaponFamily family)
        {
            return family == ProgressionWeaponFamily.Pistol ||
                family == ProgressionWeaponFamily.Musket ||
                family == ProgressionWeaponFamily.Blunderbuss;
        }

        /// <summary>
        /// Checks one native enhancement enchantment against the verified +N
        /// contract: named EnhancementN, costing N, with exactly one
        /// non-stacking WeaponEnhancementBonus of N (each bonus is given as
        /// value/stacks). Returns the failed check, or null when it matches.
        /// Game data changed by another mod is reported, never thrown.
        /// </summary>
        internal static string DescribeNativeEnhancementMismatch(int tier,
            string name, int enchantmentCost,
            IEnumerable<KeyValuePair<int, bool>> enhancementBonuses)
        {
            if (tier < 1 || tier > MaximumEnhancement)
                throw new ArgumentOutOfRangeException("tier");
            string level = tier.ToString(CultureInfo.InvariantCulture);
            KeyValuePair<int, bool>[] bonuses = (enhancementBonuses ??
                Enumerable.Empty<KeyValuePair<int, bool>>()).ToArray();
            string check =
                !string.Equals(name, "Enhancement" + level,
                    StringComparison.Ordinal) ? "name" :
                enchantmentCost != tier ? "cost" :
                bonuses.Length != 1 ? "component-count" :
                bonuses[0].Key != tier ? "bonus" :
                bonuses[0].Value ? "stacking" : null;
            return check == null ? null :
                "native-enhancement+" + level + ":" + check;
        }

        internal static ProgressionContentModule ModuleOf(
            ProgressionWeaponFamily family)
        {
            switch (family)
            {
                case ProgressionWeaponFamily.Pistol:
                case ProgressionWeaponFamily.Musket:
                case ProgressionWeaponFamily.Blunderbuss:
                    return ProgressionContentModule.Gunslinger;
                case ProgressionWeaponFamily.ElvenBranchedSpear:
                    return ProgressionContentModule.ElvenBranchedSpears;
                case ProgressionWeaponFamily.Wakizashi:
                case ProgressionWeaponFamily.Katana:
                case ProgressionWeaponFamily.Nodachi:
                    return ProgressionContentModule.EasternWeapons;
                default:
                    throw new ArgumentOutOfRangeException("family");
            }
        }

        internal static FirearmKind FirearmKindOf(ProgressionWeaponFamily family)
        {
            switch (family)
            {
                case ProgressionWeaponFamily.Pistol: return FirearmKind.Pistol;
                case ProgressionWeaponFamily.Musket: return FirearmKind.Musket;
                case ProgressionWeaponFamily.Blunderbuss:
                    return FirearmKind.Blunderbuss;
                default:
                    throw new ArgumentOutOfRangeException("family",
                        "Only Pistol, Musket and Blunderbuss are progression firearms.");
            }
        }

        internal static EasternWeaponFamily EasternFamilyOf(
            ProgressionWeaponFamily family)
        {
            switch (family)
            {
                case ProgressionWeaponFamily.Wakizashi:
                    return EasternWeaponFamily.Wakizashi;
                case ProgressionWeaponFamily.Katana:
                    return EasternWeaponFamily.Katana;
                case ProgressionWeaponFamily.Nodachi:
                    return EasternWeaponFamily.Nodachi;
                default:
                    throw new ArgumentOutOfRangeException("family",
                        "Only Wakizashi, Katana and Nodachi are Eastern families.");
            }
        }

        private static ProgressionWeaponSpec[] Build()
        {
            var result = new List<ProgressionWeaponSpec>();
            AddFirearm(result, ProgressionWeaponFamily.Pistol, "Pistol",
                PistolPlus1Symbol,
                ProductionFirearmCatalog.CreatePistol().CostGold);
            AddFirearm(result, ProgressionWeaponFamily.Musket, "Musket",
                MusketPlus1Symbol,
                ProductionFirearmCatalog.CreateMusket().CostGold);
            AddFirearm(result, ProgressionWeaponFamily.Blunderbuss,
                "Blunderbuss", BlunderbussPlus1Symbol,
                ProductionFirearmCatalog.CreateBlunderbuss().CostGold);
            AddSpear(result);
            AddEastern(result, ProgressionWeaponFamily.Wakizashi);
            AddEastern(result, ProgressionWeaponFamily.Katana);
            AddEastern(result, ProgressionWeaponFamily.Nodachi);
            ProgressionWeaponSpec[] entries = result.ToArray();
            Validate(entries);
            return entries;
        }

        private static void AddFirearm(List<ProgressionWeaponSpec> result,
            ProgressionWeaponFamily family, string name, string plusOneSymbol,
            int mundaneBaseCost)
        {
            for (int bonus = 1; bonus <= MaximumEnhancement; bonus++)
            {
                string symbol = bonus == 1 ? plusOneSymbol :
                    "KMG.Firearms." + name + "Plus" + bonus + "Item";
                result.Add(new ProgressionWeaponSpec(symbol, Identity(symbol),
                    "KMG_" + name + "Plus" + bonus + "_Item",
                    name + " +" + bonus, family, bonus, false, mundaneBaseCost,
                    bonus == 1));
            }
            for (int bonus = 1; bonus <= MaximumEnhancement; bonus++)
            {
                string symbol = "KMG.Firearms.Reliable" + name + "Plus" +
                    bonus + "Item";
                result.Add(new ProgressionWeaponSpec(symbol, Identity(symbol),
                    "KMG_Reliable" + name + "Plus" + bonus + "_Item",
                    "Reliable " + name + " +" + bonus, family, bonus, true,
                    mundaneBaseCost, false));
            }
        }

        private static void AddSpear(List<ProgressionWeaponSpec> result)
        {
            ElvenBranchedSpearItemSpec mundane = ElvenBranchedSpearCatalog
                .Require(ElvenBranchedSpearItemKind.Mundane);
            ElvenBranchedSpearItemSpec plusOne = ElvenBranchedSpearCatalog
                .Require(ElvenBranchedSpearItemKind.PlusOne);
            for (int bonus = 1; bonus <= MaximumEnhancement; bonus++)
            {
                string symbol = bonus == 1 ? plusOne.Symbol :
                    "KMG.ElvenBranchedSpear.Plus" + bonus + "Item";
                string internalName = bonus == 1 ? plusOne.InternalName :
                    "KMG_ElvenBranchedSpearPlus" + bonus + "_Item";
                string displayName = bonus == 1 ? plusOne.DisplayName :
                    "+" + bonus + " " + mundane.DisplayName;
                result.Add(new ProgressionWeaponSpec(symbol, Identity(symbol),
                    internalName, displayName,
                    ProgressionWeaponFamily.ElvenBranchedSpear, bonus, false,
                    mundane.Cost, bonus == 1));
            }
        }

        private static void AddEastern(List<ProgressionWeaponSpec> result,
            ProgressionWeaponFamily family)
        {
            EasternWeaponFamily eastern = EasternFamilyOf(family);
            EasternWeaponGenericSpec plusOne = EasternWeaponCatalog
                .RequireGeneric(eastern, EasternWeaponGenericKind.PlusOne);
            string name = EasternWeaponCatalog.RequireCategory(eastern)
                .Presentation.DisplayName;
            int mundaneBaseCost = EasternWeaponCatalog.RequireCategory(eastern)
                .BaseCost;
            for (int bonus = 1; bonus <= MaximumEnhancement; bonus++)
            {
                string symbol = bonus == 1 ? plusOne.Symbol :
                    "KMG.EasternWeapons." + name + ".Plus" + bonus + "Item";
                string internalName = bonus == 1 ? plusOne.InternalName :
                    "KMG_EasternWeapons_" + name + "_Plus" + bonus + "_Item";
                string displayName = bonus == 1 ? plusOne.DisplayName :
                    "+" + bonus + " " + name;
                result.Add(new ProgressionWeaponSpec(symbol, Identity(symbol),
                    internalName, displayName, family, bonus, false,
                    mundaneBaseCost, bonus == 1));
            }
        }

        private static string Identity(string symbol)
        {
            string guid;
            if (ReusedIdentities.TryGetValue(symbol, out guid) ||
                NewIdentities.TryGetValue(symbol, out guid))
                return guid;
            throw new KeyNotFoundException(
                "No authorized progression identity exists for " + symbol + ".");
        }

        private static void Validate(ProgressionWeaponSpec[] entries)
        {
            if (entries.Length != EntryCount ||
                entries.Count(value => value.IsFirearm) != FirearmEntryCount ||
                entries.Count(value => !value.IsFirearm) != MeleeEntryCount ||
                entries.Count(value => value.ReusesCanonicalItem) !=
                    ReusedEntryCount ||
                entries.Count(value => !value.ReusesCanonicalItem) !=
                    NewBlueprintCount ||
                entries.Select(value => value.Symbol).Distinct(
                    StringComparer.Ordinal).Count() != EntryCount ||
                entries.Select(value => value.Guid).Distinct(
                    StringComparer.Ordinal).Count() != EntryCount ||
                entries.Select(value => value.InternalName).Distinct(
                    StringComparer.Ordinal).Count() != EntryCount ||
                entries.Select(value => value.DisplayName).Distinct(
                    StringComparer.Ordinal).Count() != EntryCount ||
                entries.Select(value => value.Key).Distinct(
                    StringComparer.Ordinal).Count() != EntryCount ||
                ReusedIdentities.Count != ReusedEntryCount ||
                NewIdentities.Count != NewBlueprintCount ||
                ReusedIdentities.Keys.Any(NewIdentities.ContainsKey) ||
                entries.Any(value => value.ReusesCanonicalItem !=
                    ReusedIdentities.ContainsKey(value.Symbol)))
                throw new InvalidOperationException(
                    "The authorized progression weapon catalog is malformed.");
            foreach (ProgressionWeaponFamily family in Enum.GetValues(
                typeof(ProgressionWeaponFamily)))
            {
                foreach (bool reliable in new[] { false, true })
                {
                    int[] bonuses = entries.Where(value =>
                            value.Family == family && value.Reliable == reliable)
                        .Select(value => value.ActualEnhancement)
                        .OrderBy(value => value).ToArray();
                    bool expected = !reliable || IsFirearm(family);
                    if (expected && !bonuses.SequenceEqual(
                            Enumerable.Range(1, MaximumEnhancement)) ||
                        !expected && bonuses.Length != 0)
                        throw new InvalidOperationException(
                            "Progression family coverage is incomplete: " +
                            family + ";reliable=" + reliable + ".");
                }
            }
        }
    }

    /// <summary>
    /// Outcome of the progression-specific contract checks made when the
    /// variants are registered. Registration itself is unconditional (saved
    /// items must always resolve); a failed check only withholds merchant
    /// progression stock, so game data changed by another mod can never take
    /// the rest of this mod down.
    /// </summary>
    internal sealed class ProgressionCatalogStatus
    {
        internal static readonly ProgressionCatalogStatus Usable =
            new ProgressionCatalogStatus(new string[0]);

        internal ProgressionCatalogStatus(IEnumerable<string> failures)
        {
            Failures = (failures ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal).ToArray();
        }

        /// <summary>Distinct failed checks, in the order they were found.</summary>
        internal string[] Failures { get; private set; }

        /// <summary>True only when every progression contract check passed.</summary>
        internal bool IsUsable { get { return Failures.Length == 0; } }

        public override string ToString()
        {
            return IsUsable ? "usable" :
                "degraded:" + string.Join("|", Failures);
        }
    }
}
