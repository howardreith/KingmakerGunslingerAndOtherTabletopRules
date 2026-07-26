using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Unmodified action cost for reloading a firearm. Free and swift reloads are
    /// downstream modifiers and therefore are not valid base definition values.
    /// </summary>
    internal enum ReloadActionType
    {
        Unknown = 0,
        Standard = 1,
        FullRound = 2,
        Move = 3
    }

    /// <summary>
    /// Immutable reload configuration. It describes blueprint-level facts only;
    /// it never records whether a particular item is loaded.
    /// </summary>
    internal sealed class ReloadProfile : IEquatable<ReloadProfile>
    {
        internal const int MinimumRoundsPerAction = 1;
        internal const int MaximumRoundsPerAction = 64;

        private readonly ReloadActionType _baseAction;
        private readonly bool _requiresFreeHand;
        private readonly int _roundsPerAction;
        private readonly AmmunitionId _ammunition;

        internal ReloadProfile(
            ReloadActionType baseAction,
            bool requiresFreeHand,
            int roundsPerAction)
            : this(baseAction, requiresFreeHand, roundsPerAction, FirearmStateTokenCatalog.DiagnosticLeadBall)
        {
        }

        internal ReloadProfile(
            ReloadActionType baseAction,
            bool requiresFreeHand,
            int roundsPerAction,
            AmmunitionId ammunition)
        {
            if (!Enum.IsDefined(typeof(ReloadActionType), baseAction) ||
                baseAction == ReloadActionType.Unknown)
            {
                throw new ArgumentOutOfRangeException(
                    "baseAction",
                    baseAction,
                    "A defined non-Unknown reload action is required.");
            }

            if (roundsPerAction < MinimumRoundsPerAction ||
                roundsPerAction > MaximumRoundsPerAction)
            {
                throw new ArgumentOutOfRangeException(
                    "roundsPerAction",
                    roundsPerAction,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Rounds per reload action must be in the range {0}..{1}.",
                        MinimumRoundsPerAction,
                        MaximumRoundsPerAction));
            }

            if (ammunition == null)
            {
                throw new ArgumentNullException("ammunition");
            }

            _baseAction = baseAction;
            _requiresFreeHand = requiresFreeHand;
            _roundsPerAction = roundsPerAction;
            _ammunition = ammunition;
        }

        internal ReloadActionType BaseAction
        {
            get { return _baseAction; }
        }

        internal bool RequiresFreeHand
        {
            get { return _requiresFreeHand; }
        }

        internal int RoundsPerAction
        {
            get { return _roundsPerAction; }
        }

        internal AmmunitionId Ammunition
        {
            get { return _ammunition; }
        }

        public bool Equals(ReloadProfile other)
        {
            return !ReferenceEquals(other, null) &&
                _baseAction == other._baseAction &&
                _requiresFreeHand == other._requiresFreeHand &&
                _roundsPerAction == other._roundsPerAction &&
                Equals(_ammunition, other._ammunition);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ReloadProfile);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + (int)_baseAction;
                hash = (hash * 31) + (_requiresFreeHand ? 1 : 0);
                hash = (hash * 31) + _roundsPerAction;
                hash = (hash * 31) + _ammunition.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}; freeHand={1}; roundsPerAction={2}",
                _baseAction,
                _requiresFreeHand,
                _roundsPerAction);
        }

        public static bool operator ==(ReloadProfile left, ReloadProfile right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return !ReferenceEquals(left, null) && left.Equals(right);
        }

        public static bool operator !=(ReloadProfile left, ReloadProfile right)
        {
            return !(left == right);
        }
    }
}
