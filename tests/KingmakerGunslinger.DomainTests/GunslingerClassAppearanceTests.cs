using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Presentation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class GunslingerClassAppearanceTests
    {
        private static readonly string[] ExpectedMaleIds =
        {
            "6df8f61725a84294c8661bb9585eca97",
            "4c59d2b9740930145a27a4c693217d22"
        };

        private static readonly string[] ExpectedFemaleIds =
        {
            "beba0e0c7dcd5c64d97d767be3e72995",
            "a93ead19aae8afc4794c54f5bcf73168"
        };

        internal static void CatalogIsExactValidatedAndDefensive()
        {
            string[] male = GunslingerClassAppearanceCatalog.MaleAssetIds();
            string[] female = GunslingerClassAppearanceCatalog.FemaleAssetIds();
            Assertions.True(ExpectedMaleIds.SequenceEqual(male),
                "Gunslinger male class-clothing links changed from the accepted ordered pair.");
            Assertions.True(ExpectedFemaleIds.SequenceEqual(female),
                "Gunslinger female class-clothing links changed from the accepted ordered pair.");
            Assertions.Equal(2,
                GunslingerClassAppearanceCatalog.DefaultPrimaryColor,
                "Gunslinger primary class color changed from the accepted native default.");
            Assertions.Equal(22,
                GunslingerClassAppearanceCatalog.DefaultSecondaryColor,
                "Gunslinger secondary class color changed from the accepted native default.");

            string[] secondMale = GunslingerClassAppearanceCatalog.MaleAssetIds();
            string[] secondFemale = GunslingerClassAppearanceCatalog.FemaleAssetIds();
            Assertions.False(ReferenceEquals(male, secondMale) ||
                ReferenceEquals(female, secondFemale),
                "Appearance identifier arrays must be independent defensive copies.");
            male[0] = new string('0', 32);
            female[0] = new string('0', 32);
            Assertions.True(ExpectedMaleIds.SequenceEqual(secondMale) &&
                ExpectedFemaleIds.SequenceEqual(secondFemale),
                "Mutating a returned identifier array must not change the appearance catalog.");

            Assertions.Throws<ArgumentNullException>(() =>
                GunslingerClassAppearanceCatalog.ValidateAndCopy(
                    "test", null, 1),
                "Null appearance arrays must fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                GunslingerClassAppearanceCatalog.ValidateAndCopy(
                    "test", new[] { "ABC" }, 1),
                "Malformed appearance identifiers must fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                GunslingerClassAppearanceCatalog.ValidateAndCopy(
                    "test", new[] { ExpectedMaleIds[0], ExpectedMaleIds[0] }, 2),
                "Duplicate appearance identifiers must fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                GunslingerClassAppearanceCatalog.ValidateAndCopy(
                    "test", ExpectedMaleIds, 1),
                "Unexpected appearance link counts must fail closed.");

            string[] combined = ExpectedMaleIds.Concat(ExpectedFemaleIds).ToArray();
            Assertions.Equal(combined.Length,
                combined.Distinct(StringComparer.Ordinal).Count(),
                "Selected gender-specific links must be globally unambiguous.");
        }

        internal static void ProductionWiringIsAtomicAndDonorIndependent()
        {
            string adapter = Read("src", "KingmakerGunslinger",
                "Presentation", "GunslingerClassAppearance.cs");
            string factory = Read("src", "KingmakerGunslinger",
                "Blueprints", "GunslingerClassBlueprints.cs");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");

            foreach (string token in new[]
            {
                "RequireResolved(" + (char)34 + "male" + (char)34 + ", maleIds)",
                "RequireResolved(" + (char)34 + "female" + (char)34 + ", femaleIds)",
                "ResourcesLibrary.TryGetResource<EquipmentEntity>",
                "new EquipmentEntityLink",
                "AssetId = assetIds[index]",
                "new KingmakerEquipmentEntity[0]",
                "target.MaleEquipmentEntities = maleLinks",
                "target.FemaleEquipmentEntities = femaleLinks",
                "target.EquipmentEntities = sharedEntities",
                "target.PrimaryColor = GunslingerClassAppearanceCatalog.DefaultPrimaryColor",
                "target.SecondaryColor = GunslingerClassAppearanceCatalog.DefaultSecondaryColor"
            })
                Assertions.True(adapter.Contains(token),
                    "Production appearance adapter lacks contract token: " + token);

            int resolve = adapter.IndexOf("RequireResolved(" + (char)34 +
                "male" + (char)34, StringComparison.Ordinal);
            int mutate = adapter.IndexOf("target.MaleEquipmentEntities",
                StringComparison.Ordinal);
            Assertions.True(resolve >= 0 && mutate > resolve,
                "All selected resources must resolve before the target blueprint is mutated.");
            Assertions.True(factory.Contains(
                    "GunslingerClassAppearance.Apply(result);") &&
                factory.Contains(
                    "result.StartingGold = fighter.StartingGold;") &&
                project.Contains(
                    @"Presentation\GunslingerClassAppearanceCatalog.cs") &&
                project.Contains(
                    @"Presentation\GunslingerClassAppearance.cs"),
                "The Gunslinger class factory or project does not use the focused appearance policy.");

            foreach (string forbidden in new[]
            {
                "result.MaleEquipmentEntities = fighter.MaleEquipmentEntities",
                "result.FemaleEquipmentEntities = fighter.FemaleEquipmentEntities",
                "result.EquipmentEntities = fighter.EquipmentEntities",
                "result.PrimaryColor = fighter.PrimaryColor",
                "result.SecondaryColor = fighter.SecondaryColor"
            })
                Assertions.False(factory.Contains(forbidden),
                    "The Gunslinger still aliases a Fighter appearance field: " + forbidden);
            Assertions.False(adapter.Contains("FighterClassGuid") ||
                adapter.Contains("MagusClassGuid") ||
                adapter.Contains(".MaleEquipmentEntities = target.") ||
                adapter.Contains(".FemaleEquipmentEntities = target."),
                "Production appearance must not read or mutate a native donor blueprint.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
