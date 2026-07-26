using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Stable, serializer-safe identity for an ammunition definition. This is a
    /// domain identifier only; it does not imply a Kingmaker blueprint or item.
    /// </summary>
    internal sealed class AmmunitionId : IEquatable<AmmunitionId>, IComparable<AmmunitionId>
    {
        internal const int MaximumLength = 128;

        private readonly string _value;

        internal AmmunitionId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.Length == 0)
            {
                throw new ArgumentException("An ammunition ID is required.", "value");
            }

            if (value.Length > MaximumLength)
            {
                throw new ArgumentOutOfRangeException(
                    "value",
                    value.Length,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "An ammunition ID cannot exceed {0} characters.",
                        MaximumLength));
            }

            if (!IsLowerAsciiLetterOrDigit(value[0]))
            {
                throw new ArgumentException(
                    "An ammunition ID must begin with a lowercase ASCII letter or digit.",
                    "value");
            }

            for (int index = 1; index < value.Length; index++)
            {
                char character = value[index];
                if (!IsLowerAsciiLetterOrDigit(character) &&
                    character != '.' &&
                    character != '_' &&
                    character != '-' &&
                    character != ':')
                {
                    throw new ArgumentException(
                        "An ammunition ID may contain only lowercase ASCII letters, digits, '.', '_', '-', and ':'.",
                        "value");
                }
            }

            _value = value;
        }

        internal string Value
        {
            get { return _value; }
        }

        public int CompareTo(AmmunitionId other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }

            return string.Compare(_value, other._value, StringComparison.Ordinal);
        }

        public bool Equals(AmmunitionId other)
        {
            return !ReferenceEquals(other, null) &&
                string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AmmunitionId);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value;
        }

        public static bool operator ==(AmmunitionId left, AmmunitionId right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return !ReferenceEquals(left, null) && left.Equals(right);
        }

        public static bool operator !=(AmmunitionId left, AmmunitionId right)
        {
            return !(left == right);
        }

        private static bool IsLowerAsciiLetterOrDigit(char character)
        {
            return (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9');
        }
    }
}
