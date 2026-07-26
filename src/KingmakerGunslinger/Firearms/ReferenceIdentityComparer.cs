using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Compares object keys strictly by reference identity. Item entities must never
    /// collapse because a runtime type implements value equality.
    /// </summary>
    internal sealed class ReferenceIdentityComparer : IEqualityComparer<object>
    {
        internal static readonly ReferenceIdentityComparer Instance =
            new ReferenceIdentityComparer();

        private ReferenceIdentityComparer()
        {
        }

        bool IEqualityComparer<object>.Equals(object left, object right)
        {
            return ReferenceEquals(left, right);
        }

        int IEqualityComparer<object>.GetHashCode(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return RuntimeHelpers.GetHashCode(value);
        }
    }
}
