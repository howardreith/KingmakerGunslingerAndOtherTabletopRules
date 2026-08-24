using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal enum CraftMagicItemsCatalogFamily
    {
        Firearm = 0,
        Wakizashi = 1,
        Katana = 2,
        Nodachi = 3,
        ElvenBranchedSpear = 4,
        Diagnostic = 5
    }

    internal enum CraftMagicItemsCatalogRole
    {
        CanonicalCreationBase = 0,
        AuthoredGenericTarget = 1,
        NamedUpgradeOnly = 2,
        Unavailable = 3
    }

    internal enum CraftMagicItemsOwningModule
    {
        Gunslinger = 0,
        EasternWeapons = 1,
        ElvenBranchedSpears = 2
    }

    internal sealed class CraftMagicItemsCatalogEntry
    {
        internal CraftMagicItemsCatalogEntry(string identity, string displayName,
            CraftMagicItemsCatalogFamily family, CraftMagicItemsCatalogRole role,
            CraftMagicItemsOwningModule module, bool playerAuthorized,
            bool unavailable)
        {
            if (string.IsNullOrWhiteSpace(identity))
                throw new ArgumentException("A stable catalog identity is required.",
                    "identity");
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("A display name is required.",
                    "displayName");
            Identity = identity;
            DisplayName = displayName;
            Family = family;
            Role = role;
            Module = module;
            PlayerAuthorized = playerAuthorized;
            Unavailable = unavailable;
        }

        internal string Identity { get; private set; }
        internal string DisplayName { get; private set; }
        internal CraftMagicItemsCatalogFamily Family { get; private set; }
        internal CraftMagicItemsCatalogRole Role { get; private set; }
        internal CraftMagicItemsOwningModule Module { get; private set; }
        internal bool PlayerAuthorized { get; private set; }
        internal bool Unavailable { get; private set; }
    }

    internal sealed class CraftMagicItemsModuleState
    {
        internal CraftMagicItemsModuleState(bool gunslinger,
            bool easternWeapons, bool elvenBranchedSpears)
        {
            Gunslinger = gunslinger;
            EasternWeapons = easternWeapons;
            ElvenBranchedSpears = elvenBranchedSpears;
        }

        internal bool Gunslinger { get; private set; }
        internal bool EasternWeapons { get; private set; }
        internal bool ElvenBranchedSpears { get; private set; }

        internal bool IsEnabled(CraftMagicItemsOwningModule module)
        {
            return module == CraftMagicItemsOwningModule.Gunslinger
                ? Gunslinger
                : module == CraftMagicItemsOwningModule.EasternWeapons
                ? EasternWeapons
                : ElvenBranchedSpears;
        }
    }

    internal sealed class CraftMagicItemsCatalogDecision
    {
        internal CraftMagicItemsCatalogDecision(
            CraftMagicItemsCatalogEntry[] firearmBases,
            CraftMagicItemsCatalogEntry[] martialBases,
            CraftMagicItemsCatalogEntry[] exoticBases,
            CraftMagicItemsCatalogEntry[] authoredTargets,
            CraftMagicItemsCatalogEntry[] namedUpgradeOnly)
        {
            FirearmBases = firearmBases;
            MartialBases = martialBases;
            ExoticBases = exoticBases;
            AuthoredTargets = authoredTargets;
            NamedUpgradeOnly = namedUpgradeOnly;
        }

        internal CraftMagicItemsCatalogEntry[] FirearmBases { get; private set; }
        internal CraftMagicItemsCatalogEntry[] MartialBases { get; private set; }
        internal CraftMagicItemsCatalogEntry[] ExoticBases { get; private set; }
        internal CraftMagicItemsCatalogEntry[] AuthoredTargets { get; private set; }
        internal CraftMagicItemsCatalogEntry[] NamedUpgradeOnly
        { get; private set; }

        internal CraftMagicItemsCatalogEntry[] AllCreationBases
        {
            get
            {
                return FirearmBases.Concat(MartialBases).Concat(ExoticBases)
                    .ToArray();
            }
        }
    }

    internal sealed class CraftMagicItemsAmmunitionRecipePlan
    {
        internal CraftMagicItemsAmmunitionRecipePlan(string identity,
            string displayName, int unitCost, int count)
        {
            if (string.IsNullOrWhiteSpace(identity) ||
                string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException(
                    "An ammunition recipe identity and name are required.");
            if (unitCost < 0 || count <= 0)
                throw new ArgumentOutOfRangeException("unitCost");
            Identity = identity;
            DisplayName = displayName;
            UnitCost = unitCost;
            Count = count;
        }

        internal string Identity { get; private set; }
        internal string DisplayName { get; private set; }
        internal int UnitCost { get; private set; }
        internal int Count { get; private set; }
        internal int BatchValue { get { return checked(UnitCost * Count); } }

        internal int RequiredProgress
        { get { return Math.Max(1, BatchValue / 4); } }

        internal int GoldCost(float craftingPriceScale)
        {
            if (craftingPriceScale < 0f || float.IsNaN(craftingPriceScale) ||
                float.IsInfinity(craftingPriceScale))
                throw new ArgumentOutOfRangeException("craftingPriceScale");
            int scaled = (int)Math.Round(RequiredProgress *
                (double)craftingPriceScale, MidpointRounding.ToEven);
            return Math.Max(1, (scaled * 2 + 2) / 3);
        }
    }

    internal sealed class CraftMagicItemsBlueprintIntegritySnapshot
    {
        internal CraftMagicItemsBlueprintIntegritySnapshot(string identity,
            string weaponTypeIdentity, int firearmMarkerCount,
            int proficiencyRestrictionCount, string presentationIdentity,
            string categoryIdentity, IEnumerable<string> inherentMechanics)
        {
            Identity = identity ?? string.Empty;
            WeaponTypeIdentity = weaponTypeIdentity ?? string.Empty;
            FirearmMarkerCount = firearmMarkerCount;
            ProficiencyRestrictionCount = proficiencyRestrictionCount;
            PresentationIdentity = presentationIdentity ?? string.Empty;
            CategoryIdentity = categoryIdentity ?? string.Empty;
            InherentMechanics = (inherentMechanics ?? new string[0])
                .ToArray();
        }

        internal string Identity { get; private set; }
        internal string WeaponTypeIdentity { get; private set; }
        internal int FirearmMarkerCount { get; private set; }
        internal int ProficiencyRestrictionCount { get; private set; }
        internal string PresentationIdentity { get; private set; }
        internal string CategoryIdentity { get; private set; }
        internal string[] InherentMechanics { get; private set; }

        internal bool SameGraph(
            CraftMagicItemsBlueprintIntegritySnapshot other)
        {
            return other != null && Identity == other.Identity &&
                WeaponTypeIdentity == other.WeaponTypeIdentity &&
                FirearmMarkerCount == other.FirearmMarkerCount &&
                ProficiencyRestrictionCount ==
                    other.ProficiencyRestrictionCount &&
                PresentationIdentity == other.PresentationIdentity &&
                CategoryIdentity == other.CategoryIdentity &&
                InherentMechanics.SequenceEqual(other.InherentMechanics,
                    StringComparer.Ordinal);
        }
    }

    internal sealed class CraftMagicItemsBlueprintIntegrityDecision
    {
        internal CraftMagicItemsBlueprintIntegrityDecision(bool valid,
            string failedCheck)
        {
            Valid = valid;
            FailedCheck = failedCheck ?? string.Empty;
        }

        internal bool Valid { get; private set; }
        internal string FailedCheck { get; private set; }
    }

    internal static class CraftMagicItemsCompatibilityPolicy
    {
        internal const int AmmunitionBatchCount = 20;
        internal const int FirearmMundaneBaseDc = 18;
        internal const int AmmunitionMundaneBaseDc = 15;
        internal const int ReliableEquivalentBonus = 1;
        internal const int ReliableCasterLevel = 8;

        internal static CraftMagicItemsCatalogDecision BuildCatalog(
            IEnumerable<CraftMagicItemsCatalogEntry> source,
            CraftMagicItemsModuleState modules)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (modules == null) throw new ArgumentNullException("modules");
            CraftMagicItemsCatalogEntry[] entries = source.ToArray();
            if (entries.Any(value => value == null))
                throw new InvalidOperationException(
                    "The compatibility catalog contains a null entry.");
            string duplicate = entries.GroupBy(value => value.Identity,
                    StringComparer.Ordinal)
                .Where(group => group.Count() != 1)
                .Select(group => group.Key).FirstOrDefault();
            if (duplicate != null)
                throw new InvalidOperationException(
                    "Duplicate compatibility catalog identity: " + duplicate);

            CraftMagicItemsCatalogEntry[] canonical = entries.Where(value =>
                    value.Role == CraftMagicItemsCatalogRole
                        .CanonicalCreationBase &&
                    value.PlayerAuthorized && !value.Unavailable &&
                    value.Family != CraftMagicItemsCatalogFamily.Diagnostic &&
                    modules.IsEnabled(value.Module))
                .ToArray();
            CraftMagicItemsCatalogEntry[] firearms = canonical.Where(value =>
                    value.Family == CraftMagicItemsCatalogFamily.Firearm)
                .ToArray();
            CraftMagicItemsCatalogEntry[] martial = canonical.Where(value =>
                    value.Family == CraftMagicItemsCatalogFamily.Nodachi)
                .ToArray();
            CraftMagicItemsCatalogEntry[] exotic = canonical.Where(value =>
                    value.Family == CraftMagicItemsCatalogFamily.Wakizashi ||
                    value.Family == CraftMagicItemsCatalogFamily.Katana ||
                    value.Family == CraftMagicItemsCatalogFamily
                        .ElvenBranchedSpear)
                .ToArray();
            if (canonical.Length != firearms.Length + martial.Length +
                    exotic.Length)
                throw new InvalidOperationException(
                    "A canonical creation base has no supported CMI mundane classification.");

            return new CraftMagicItemsCatalogDecision(firearms, martial,
                exotic, entries.Where(value => value.Role ==
                    CraftMagicItemsCatalogRole.AuthoredGenericTarget).ToArray(),
                entries.Where(value => value.Role ==
                    CraftMagicItemsCatalogRole.NamedUpgradeOnly).ToArray());
        }

        internal static bool ReliableApplies(int firearmMarkerCount)
        { return firearmMarkerCount == 1; }

        internal static CraftMagicItemsBlueprintIntegrityDecision
            ValidateCustomClone(
                CraftMagicItemsBlueprintIntegritySnapshot baseBefore,
                CraftMagicItemsBlueprintIntegritySnapshot baseAfter,
                CraftMagicItemsBlueprintIntegritySnapshot clone,
                bool firearm)
        {
            if (baseBefore == null || baseAfter == null || clone == null)
                return new CraftMagicItemsBlueprintIntegrityDecision(false,
                    "snapshot-missing");
            if (!baseBefore.SameGraph(baseAfter))
                return new CraftMagicItemsBlueprintIntegrityDecision(false,
                    "base-mutated");
            if (string.IsNullOrWhiteSpace(clone.Identity) ||
                string.Equals(clone.Identity, baseBefore.Identity,
                    StringComparison.Ordinal))
                return new CraftMagicItemsBlueprintIntegrityDecision(false,
                    "clone-identity");
            if (!string.Equals(clone.WeaponTypeIdentity,
                    baseBefore.WeaponTypeIdentity, StringComparison.Ordinal) ||
                clone.ProficiencyRestrictionCount !=
                    baseBefore.ProficiencyRestrictionCount ||
                !string.Equals(clone.PresentationIdentity,
                    baseBefore.PresentationIdentity,
                    StringComparison.Ordinal) ||
                !string.Equals(clone.CategoryIdentity,
                    baseBefore.CategoryIdentity, StringComparison.Ordinal) ||
                !clone.InherentMechanics.SequenceEqual(
                    baseBefore.InherentMechanics, StringComparer.Ordinal))
                return new CraftMagicItemsBlueprintIntegrityDecision(false,
                    "owned-graph-changed");
            if (firearm && (baseBefore.FirearmMarkerCount != 1 ||
                    clone.FirearmMarkerCount != 1))
                return new CraftMagicItemsBlueprintIntegrityDecision(false,
                    "firearm-marker");
            if (!firearm && clone.FirearmMarkerCount !=
                    baseBefore.FirearmMarkerCount)
                return new CraftMagicItemsBlueprintIntegrityDecision(false,
                    "unexpected-marker-change");
            return new CraftMagicItemsBlueprintIntegrityDecision(true,
                string.Empty);
        }

        internal static T[] MergeExactlyOnce<T>(IEnumerable<T> existing,
            IEnumerable<T> additions, Func<T, string> identity)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            if (additions == null) throw new ArgumentNullException("additions");
            if (identity == null) throw new ArgumentNullException("identity");
            var result = new List<T>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (T value in existing.Concat(additions))
            {
                if (value == null) throw new InvalidOperationException(
                    "An idempotent registration collection contains null.");
                string key = identity(value);
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException(
                        "An idempotent registration identity is empty.");
                if (seen.Add(key)) result.Add(value);
            }
            return result.ToArray();
        }
    }
}
