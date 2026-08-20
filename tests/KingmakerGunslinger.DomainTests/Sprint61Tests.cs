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

        private static void VendorPublicationIntegratesByStableKey()
        {
            string[] native = { "weapon:A", "weapon:N", "weapon:Z" };
            string middle = "weapon:M", late = "weapon:Y";
            var result = VendorCatalogPublication<string>.CreateIntegrated(
                native, new[] { late, middle }, value => value);
            Assertions.True(result.Changed,
                "A fresh integrated publication must report a change.");
            Assertions.Equal("weapon:A", result.Published[0],
                "The first native row moved.");
            Assertions.Equal(middle, result.Published[1],
                "The middle project row was not integrated by key.");
            Assertions.Equal("weapon:N", result.Published[2],
                "Native relative order changed at the middle insertion.");
            Assertions.Equal(late, result.Published[3],
                "The late project row was not integrated by key.");
            Assertions.Equal("weapon:Z", result.Published[4],
                "The last native row moved.");
            string[] restored = result.Rollback();
            Assertions.Equal(3, restored.Length,
                "Integrated rollback length mismatch.");
            Assertions.Equal("weapon:A", restored[0],
                "Integrated rollback changed native order.");
        }
    }
}
