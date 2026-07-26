using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable mapping between one stable item-enchantment token ID and one complete
    /// firearm state. The token ID is serializer-facing data, not localized text.
    /// </summary>
    internal sealed class FirearmStateTokenDefinition : IEquatable<FirearmStateTokenDefinition>
    {
        private static readonly Regex TokenPattern = new Regex(
            "^[a-z0-9]+(?:[._:-][a-z0-9]+)*$",
            RegexOptions.CultureInvariant);

        private readonly string _tokenId;
        private readonly FirearmState _state;

        internal FirearmStateTokenDefinition(string tokenId, FirearmState state)
        {
            if (string.IsNullOrWhiteSpace(tokenId) ||
                tokenId.Length > 96 ||
                !TokenPattern.IsMatch(tokenId))
            {
                throw new ArgumentException(
                    "A state-token ID must be 1..96 lowercase ASCII token characters with no leading or trailing separator.",
                    "tokenId");
            }

            _state = state ?? throw new ArgumentNullException("state");
            _tokenId = tokenId;
        }

        internal string TokenId
        {
            get { return _tokenId; }
        }

        internal FirearmState State
        {
            get { return _state; }
        }

        public bool Equals(FirearmStateTokenDefinition other)
        {
            return !ReferenceEquals(other, null) &&
                string.Equals(_tokenId, other._tokenId, StringComparison.Ordinal) &&
                _state == other._state;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FirearmStateTokenDefinition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_tokenId.GetHashCode() * 397) ^ _state.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "token={0}; state=[{1}]",
                _tokenId,
                _state);
        }
    }
}
