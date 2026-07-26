using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Canonical engine-issued identity for one concrete Kingmaker item entity.
    /// Sprint 14 accepts only nonempty GUID values in the standard D form. It never
    /// invents, assigns, repairs, or falls back from an unavailable identity.
    /// </summary>
    internal sealed class FirearmItemId : IEquatable<FirearmItemId>, IComparable<FirearmItemId>
    {
        private readonly Guid _value;
        private readonly string _canonical;

        internal FirearmItemId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "A firearm item identity cannot contain leading or trailing whitespace.",
                    "value");
            }

            Guid parsed;
            if (!Guid.TryParseExact(value, "D", out parsed))
            {
                throw new ArgumentException(
                    "A firearm item identity must use the standard 36-character GUID D form.",
                    "value");
            }

            if (parsed == Guid.Empty)
            {
                throw new ArgumentException(
                    "A firearm item identity cannot be the empty GUID.",
                    "value");
            }

            _value = parsed;
            _canonical = parsed.ToString("D").ToLowerInvariant();
        }

        internal FirearmItemId(Guid value)
            : this(value.ToString("D"))
        {
        }

        internal string Value
        {
            get { return _canonical; }
        }

        internal Guid GuidValue
        {
            get { return _value; }
        }

        internal static bool TryCreate(
            string value,
            out FirearmItemId identity,
            out string rejectionReason)
        {
            try
            {
                identity = new FirearmItemId(value);
                rejectionReason = null;
                return true;
            }
            catch (Exception exception)
            {
                identity = null;
                rejectionReason = exception.Message;
                return false;
            }
        }

        public bool Equals(FirearmItemId other)
        {
            return !ReferenceEquals(other, null) && _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FirearmItemId);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public int CompareTo(FirearmItemId other)
        {
            return ReferenceEquals(other, null)
                ? 1
                : string.Compare(_canonical, other._canonical, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return _canonical;
        }

        public static bool operator ==(FirearmItemId left, FirearmItemId right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return !ReferenceEquals(left, null) && left.Equals(right);
        }

        public static bool operator !=(FirearmItemId left, FirearmItemId right)
        {
            return !(left == right);
        }
    }
}
