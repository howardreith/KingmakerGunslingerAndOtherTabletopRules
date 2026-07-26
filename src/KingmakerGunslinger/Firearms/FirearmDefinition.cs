using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable blueprint-level firearm definition. Mutable item state such as
    /// loaded rounds, selected ammunition, damage, and jams is intentionally absent.
    /// </summary>
    internal sealed class FirearmDefinition : IEquatable<FirearmDefinition>
    {
        internal const int MinimumCapacity = 1;
        internal const int MaximumCapacity = 64;
        internal const int MinimumRangeIncrementFeet = 5;
        internal const int MaximumRangeIncrementFeet = 1000;
        internal const int MinimumMisfireValue = 1;
        internal const int MaximumMisfireValue = 20;
        internal const int MinimumMisfireBurstRadiusFeet = 5;
        internal const int MaximumMisfireBurstRadiusFeet = 100;

        private readonly FirearmEra _era;
        private readonly FirearmKind _kind;
        private readonly int _capacity;
        private readonly int _rangeIncrementFeet;
        private readonly int _misfireValue;
        private readonly int _misfireBurstRadiusFeet;
        private readonly ReloadProfile _reload;
        private readonly bool _isScatter;

        internal FirearmDefinition(
            FirearmEra era,
            FirearmKind kind,
            int capacity,
            int rangeIncrementFeet,
            int misfireValue,
            int misfireBurstRadiusFeet,
            ReloadProfile reload,
            bool isScatter)
        {
            ValidateEnum(era, "era");
            ValidateEnum(kind, "kind");
            ValidateCapacity(capacity);
            ValidateRangeIncrement(rangeIncrementFeet);
            ValidateMisfireValue(misfireValue);
            ValidateMisfireBurstRadius(misfireBurstRadiusFeet);

            if (reload == null)
            {
                throw new ArgumentNullException("reload");
            }

            if (reload.RoundsPerAction > capacity)
            {
                throw new ArgumentException(
                    "A reload action cannot load more rounds than the firearm's capacity.",
                    "reload");
            }

            ValidateKindEra(kind, era);
            ValidateScatter(kind, isScatter);
            ValidateReloadAction(kind, era, reload.BaseAction);

            if (kind == FirearmKind.Revolver && capacity < 2)
            {
                throw new ArgumentException(
                    "A revolver definition must have a capacity of at least two rounds.",
                    "capacity");
            }

            _era = era;
            _kind = kind;
            _capacity = capacity;
            _rangeIncrementFeet = rangeIncrementFeet;
            _misfireValue = misfireValue;
            _misfireBurstRadiusFeet = misfireBurstRadiusFeet;
            _reload = reload;
            _isScatter = isScatter;
        }

        internal FirearmEra Era
        {
            get { return _era; }
        }

        internal FirearmKind Kind
        {
            get { return _kind; }
        }

        internal int Capacity
        {
            get { return _capacity; }
        }

        internal int RangeIncrementFeet
        {
            get { return _rangeIncrementFeet; }
        }

        internal int MisfireValue
        {
            get { return _misfireValue; }
        }

        internal int MisfireBurstRadiusFeet
        {
            get { return _misfireBurstRadiusFeet; }
        }

        internal ReloadProfile Reload
        {
            get { return _reload; }
        }

        internal bool IsScatter
        {
            get { return _isScatter; }
        }

        public bool Equals(FirearmDefinition other)
        {
            return !ReferenceEquals(other, null) &&
                _era == other._era &&
                _kind == other._kind &&
                _capacity == other._capacity &&
                _rangeIncrementFeet == other._rangeIncrementFeet &&
                _misfireValue == other._misfireValue &&
                _misfireBurstRadiusFeet == other._misfireBurstRadiusFeet &&
                Equals(_reload, other._reload) &&
                _isScatter == other._isScatter;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FirearmDefinition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + (int)_era;
                hash = (hash * 31) + (int)_kind;
                hash = (hash * 31) + _capacity;
                hash = (hash * 31) + _rangeIncrementFeet;
                hash = (hash * 31) + _misfireValue;
                hash = (hash * 31) + _misfireBurstRadiusFeet;
                hash = (hash * 31) + _reload.GetHashCode();
                hash = (hash * 31) + (_isScatter ? 1 : 0);
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}; capacity={2}; range={3}ft; misfire=1-{4}; misfireBurst={5}ft; reload=({6}); scatter={7}",
                _era,
                _kind,
                _capacity,
                _rangeIncrementFeet,
                _misfireValue,
                _misfireBurstRadiusFeet,
                _reload,
                _isScatter);
        }

        public static bool operator ==(FirearmDefinition left, FirearmDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return !ReferenceEquals(left, null) && left.Equals(right);
        }

        public static bool operator !=(FirearmDefinition left, FirearmDefinition right)
        {
            return !(left == right);
        }

        private static void ValidateEnum(FirearmEra era, string parameterName)
        {
            if (!Enum.IsDefined(typeof(FirearmEra), era) || era == FirearmEra.Unknown)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    era,
                    "A defined non-Unknown firearm era is required.");
            }
        }

        private static void ValidateEnum(FirearmKind kind, string parameterName)
        {
            if (!Enum.IsDefined(typeof(FirearmKind), kind) || kind == FirearmKind.Unknown)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    kind,
                    "A defined non-Unknown firearm kind is required.");
            }
        }

        private static void ValidateCapacity(int capacity)
        {
            if (capacity < MinimumCapacity || capacity > MaximumCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    "capacity",
                    capacity,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Firearm capacity must be in the range {0}..{1}.",
                        MinimumCapacity,
                        MaximumCapacity));
            }
        }

        private static void ValidateRangeIncrement(int rangeIncrementFeet)
        {
            if (rangeIncrementFeet < MinimumRangeIncrementFeet ||
                rangeIncrementFeet > MaximumRangeIncrementFeet)
            {
                throw new ArgumentOutOfRangeException(
                    "rangeIncrementFeet",
                    rangeIncrementFeet,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Range increment must be in the range {0}..{1} feet.",
                        MinimumRangeIncrementFeet,
                        MaximumRangeIncrementFeet));
            }

            if (rangeIncrementFeet % 5 != 0)
            {
                throw new ArgumentException(
                    "Range increment must be expressed in five-foot steps.",
                    "rangeIncrementFeet");
            }
        }

        private static void ValidateMisfireValue(int misfireValue)
        {
            if (misfireValue < MinimumMisfireValue || misfireValue > MaximumMisfireValue)
            {
                throw new ArgumentOutOfRangeException(
                    "misfireValue",
                    misfireValue,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Misfire value must be in the range {0}..{1}.",
                        MinimumMisfireValue,
                        MaximumMisfireValue));
            }
        }

        private static void ValidateMisfireBurstRadius(int misfireBurstRadiusFeet)
        {
            if (misfireBurstRadiusFeet < MinimumMisfireBurstRadiusFeet ||
                misfireBurstRadiusFeet > MaximumMisfireBurstRadiusFeet)
            {
                throw new ArgumentOutOfRangeException(
                    "misfireBurstRadiusFeet",
                    misfireBurstRadiusFeet,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Misfire burst radius must be in the range {0}..{1} feet.",
                        MinimumMisfireBurstRadiusFeet,
                        MaximumMisfireBurstRadiusFeet));
            }

            if (misfireBurstRadiusFeet % 5 != 0)
            {
                throw new ArgumentException(
                    "Misfire burst radius must be expressed in five-foot steps.",
                    "misfireBurstRadiusFeet");
            }
        }

        private static void ValidateKindEra(FirearmKind kind, FirearmEra era)
        {
            if ((kind == FirearmKind.Musket || kind == FirearmKind.Blunderbuss) &&
                era != FirearmEra.Early)
            {
                throw new ArgumentException(
                    "Musket and blunderbuss definitions are early firearms.",
                    "era");
            }

            if ((kind == FirearmKind.Rifle || kind == FirearmKind.Revolver) &&
                era != FirearmEra.Advanced)
            {
                throw new ArgumentException(
                    "Rifle and revolver definitions are advanced firearms.",
                    "era");
            }
        }

        private static void ValidateScatter(FirearmKind kind, bool isScatter)
        {
            bool mustScatter = kind == FirearmKind.Blunderbuss;
            if (mustScatter != isScatter)
            {
                throw new ArgumentException(
                    "Only blunderbuss definitions use the scatter rules in the initial firearm vocabulary.",
                    "isScatter");
            }
        }

        private static void ValidateReloadAction(
            FirearmKind kind,
            FirearmEra era,
            ReloadActionType baseAction)
        {
            ReloadActionType requiredAction;
            if (era == FirearmEra.Advanced)
            {
                requiredAction = ReloadActionType.Move;
            }
            else if (kind == FirearmKind.Pistol)
            {
                requiredAction = ReloadActionType.Standard;
            }
            else
            {
                requiredAction = ReloadActionType.FullRound;
            }

            if (baseAction != requiredAction)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The base reload action for {0} {1} must be {2}, not {3}.",
                        era,
                        kind,
                        requiredAction,
                        baseAction),
                    "reload");
            }
        }
    }
}
