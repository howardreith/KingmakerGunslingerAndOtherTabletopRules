using System;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class OriginatingUnitId : IEquatable<OriginatingUnitId>
    {
        private readonly string _value;

        internal OriginatingUnitId(string value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (!string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
                value.Length == 0 || value.Length > 256)
                throw new ArgumentException(
                    "An originating unit identity must be 1-256 unpadded characters.", "value");
            _value = value;
        }

        internal string Value { get { return _value; } }

        public bool Equals(OriginatingUnitId other)
        {
            return !ReferenceEquals(other, null) &&
                string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) { return Equals(obj as OriginatingUnitId); }
        public override int GetHashCode() { return StringComparer.Ordinal.GetHashCode(_value); }
        public override string ToString() { return _value; }
    }
}
