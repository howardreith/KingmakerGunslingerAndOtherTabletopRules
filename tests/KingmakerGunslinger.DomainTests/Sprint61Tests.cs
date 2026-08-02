using System;
using KingmakerGunslinger.Acquisition;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void VendorPublicationAppendsExactReferences()
        {
            object native = new object(), pistol = new object(), powder = new object();
            var result = VendorCatalogPublication<object>.Create(
                new[] { native }, new[] { pistol, powder });
            Assertions.True(result.Changed, "A fresh publication must report a change.");
            Assertions.True(ReferenceEquals(native, result.Published[0]), "Native order changed.");
            Assertions.True(ReferenceEquals(pistol, result.Published[1]), "Pistol was not appended.");
            Assertions.True(ReferenceEquals(powder, result.Published[2]), "Powder was not appended.");
        }

        private static void VendorPublicationIsIdempotent()
        {
            object native = new object(), pistol = new object(), powder = new object();
            var result = VendorCatalogPublication<object>.Create(
                new[] { native, pistol, powder }, new[] { pistol, powder });
            Assertions.False(result.Changed, "A complete prior publication must be a no-op.");
            Assertions.Equal(3, result.Published.Length, "Idempotence changed catalog length.");
        }

        private static void VendorPublicationRejectsAmbiguity()
        {
            object native = new object(), pistol = new object(), powder = new object();
            Assertions.Throws<InvalidOperationException>(() =>
                VendorCatalogPublication<object>.Create(
                    new[] { native, pistol }, new[] { pistol, powder }),
                "Partial publication must fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                VendorCatalogPublication<object>.Create(
                    new[] { native }, new[] { pistol, pistol }),
                "Duplicate additions must fail closed.");
        }

        private static void VendorPublicationRollbackRestoresNativeReferences()
        {
            object first = new object(), second = new object(), pistol = new object();
            var result = VendorCatalogPublication<object>.Create(
                new[] { first, second }, new[] { pistol });
            object[] restored = result.Rollback();
            Assertions.Equal(2, restored.Length, "Rollback length mismatch.");
            Assertions.True(ReferenceEquals(first, restored[0]), "Rollback changed first native entry.");
            Assertions.True(ReferenceEquals(second, restored[1]), "Rollback changed second native entry.");
            Assertions.Throws<InvalidOperationException>(() => result.Rollback(),
                "Rollback must be single-use.");
        }
    }
}
