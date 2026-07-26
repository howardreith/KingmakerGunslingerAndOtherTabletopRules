using System;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Validated, immutable Kingmaker blueprint identifier. This type deliberately
    /// exposes parsing only; runtime identifier generation is not supported.
    /// </summary>
    internal sealed class BlueprintId : IEquatable<BlueprintId>
    {
        private const int RequiredLength = 32;
        private static readonly Regex LowercaseHexPattern = new Regex(
            "^[0-9a-f]{32}$",
            RegexOptions.CultureInvariant);

        private readonly string _value;

        private BlueprintId(string value)
        {
            _value = value;
        }

        internal string Value
        {
            get { return _value; }
        }

        internal static BlueprintId Parse(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Length != RequiredLength || !LowercaseHexPattern.IsMatch(value))
            {
                throw new FormatException(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Blueprint identifier '{0}' must be exactly 32 lowercase hexadecimal characters.",
                        value));
            }

            Guid parsed;
            if (!Guid.TryParseExact(value, "N", out parsed) ||
                !string.Equals(parsed.ToString("N"), value, StringComparison.Ordinal))
            {
                throw new FormatException(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Blueprint identifier '{0}' is not a valid GUID in N format.",
                        value));
            }

            if (parsed == Guid.Empty)
            {
                throw new FormatException("The all-zero GUID is not a valid custom blueprint identifier.");
            }

            return new BlueprintId(value);
        }

        public bool Equals(BlueprintId other)
        {
            return !ReferenceEquals(other, null) && string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BlueprintId);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
