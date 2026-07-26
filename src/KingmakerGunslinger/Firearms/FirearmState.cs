using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable state of one conceptual firearm item. It contains no owner, unit,
    /// inventory, blueprint, Unity object, or persistence mechanism.
    /// </summary>
    internal sealed class FirearmState : IEquatable<FirearmState>
    {
        internal const int CurrentSchemaVersion = 1;

        private readonly int _schemaVersion;
        private readonly int _loadedRounds;
        private readonly AmmunitionId _loadedAmmunition;
        private readonly FirearmCondition _condition;

        internal FirearmState(
            int schemaVersion,
            int loadedRounds,
            AmmunitionId loadedAmmunition,
            FirearmCondition condition)
        {
            if (schemaVersion != CurrentSchemaVersion)
            {
                throw new ArgumentOutOfRangeException(
                    "schemaVersion",
                    schemaVersion,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Only firearm-state schema version {0} is supported.",
                        CurrentSchemaVersion));
            }

            if (loadedRounds < 0 || loadedRounds > FirearmDefinition.MaximumCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    "loadedRounds",
                    loadedRounds,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Loaded rounds must be in the range 0..{0}.",
                        FirearmDefinition.MaximumCapacity));
            }

            if (!Enum.IsDefined(typeof(FirearmCondition), condition) ||
                condition == FirearmCondition.Unknown)
            {
                throw new ArgumentOutOfRangeException(
                    "condition",
                    condition,
                    "A defined non-Unknown firearm condition is required.");
            }

            if (loadedRounds == 0 && loadedAmmunition != null)
            {
                throw new ArgumentException(
                    "An empty firearm cannot retain a loaded ammunition ID.",
                    "loadedAmmunition");
            }

            if (loadedRounds > 0 && loadedAmmunition == null)
            {
                throw new ArgumentNullException(
                    "loadedAmmunition",
                    "A loaded firearm requires an ammunition ID.");
            }

            if (condition == FirearmCondition.Wrecked && loadedRounds != 0)
            {
                throw new ArgumentException(
                    "A wrecked firearm must be empty.",
                    "loadedRounds");
            }

            _schemaVersion = schemaVersion;
            _loadedRounds = loadedRounds;
            _loadedAmmunition = loadedAmmunition;
            _condition = condition;
        }

        internal int SchemaVersion
        {
            get { return _schemaVersion; }
        }

        internal int LoadedRounds
        {
            get { return _loadedRounds; }
        }

        internal AmmunitionId LoadedAmmunition
        {
            get { return _loadedAmmunition; }
        }

        internal FirearmCondition Condition
        {
            get { return _condition; }
        }

        internal bool IsEmpty
        {
            get { return _loadedRounds == 0; }
        }

        internal static FirearmState CreateEmpty()
        {
            return new FirearmState(
                CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Normal);
        }

        public bool Equals(FirearmState other)
        {
            return !ReferenceEquals(other, null) &&
                _schemaVersion == other._schemaVersion &&
                _loadedRounds == other._loadedRounds &&
                Equals(_loadedAmmunition, other._loadedAmmunition) &&
                _condition == other._condition;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FirearmState);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + _schemaVersion;
                hash = (hash * 31) + _loadedRounds;
                hash = (hash * 31) + (_loadedAmmunition == null ? 0 : _loadedAmmunition.GetHashCode());
                hash = (hash * 31) + (int)_condition;
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "schema={0}; rounds={1}; ammunition={2}; condition={3}",
                _schemaVersion,
                _loadedRounds,
                _loadedAmmunition == null ? "<none>" : _loadedAmmunition.Value,
                _condition);
        }

        public static bool operator ==(FirearmState left, FirearmState right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return !ReferenceEquals(left, null) && left.Equals(right);
        }

        public static bool operator !=(FirearmState left, FirearmState right)
        {
            return !(left == right);
        }
    }
}
