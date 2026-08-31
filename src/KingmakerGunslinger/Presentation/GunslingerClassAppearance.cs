using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;

namespace KingmakerGunslinger.Presentation
{
    /// <summary>
    /// Applies the audited native clothing policy to a new Gunslinger class.
    /// Resource resolution and link construction finish before the target is
    /// changed, so a missing installed asset cannot leave partial appearance.
    /// </summary>
    internal static class GunslingerClassAppearance
    {
        internal static void Apply(BlueprintCharacterClass target)
        {
            if (target == null) throw new ArgumentNullException("target");

            string[] maleIds = GunslingerClassAppearanceCatalog.MaleAssetIds();
            string[] femaleIds = GunslingerClassAppearanceCatalog.FemaleAssetIds();
            RequireResolved("male", maleIds);
            RequireResolved("female", femaleIds);

            EquipmentEntityLink[] maleLinks = CreateLinks(maleIds);
            EquipmentEntityLink[] femaleLinks = CreateLinks(femaleIds);
            var sharedEntities = new KingmakerEquipmentEntity[0];

            target.MaleEquipmentEntities = maleLinks;
            target.FemaleEquipmentEntities = femaleLinks;
            target.EquipmentEntities = sharedEntities;
            target.PrimaryColor = GunslingerClassAppearanceCatalog.DefaultPrimaryColor;
            target.SecondaryColor = GunslingerClassAppearanceCatalog.DefaultSecondaryColor;
        }

        private static EquipmentEntityLink[] CreateLinks(string[] assetIds)
        {
            var result = new EquipmentEntityLink[assetIds.Length];
            for (int index = 0; index < assetIds.Length; index++)
            {
                result[index] = new EquipmentEntityLink
                {
                    AssetId = assetIds[index]
                };
            }
            return result;
        }

        private static void RequireResolved(string role, string[] assetIds)
        {
            for (int index = 0; index < assetIds.Length; index++)
            {
                string assetId = assetIds[index];
                EquipmentEntity entity;
                try
                {
                    entity = ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        assetId, true);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        "Unable to resolve Gunslinger " + role +
                        " class-clothing asset " + assetId + ".", exception);
                }
                if (entity == null)
                    throw new InvalidOperationException(
                        "Missing Gunslinger " + role +
                        " class-clothing asset " + assetId + ".");
            }
        }
    }
}
