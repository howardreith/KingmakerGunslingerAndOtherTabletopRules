using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.BodyguardFeats;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class BodyguardFeatCatalogPublication
    {
        internal const string BasicFeatSelectionGuid =
            "247a4068296e8be42890143f451b4b45";
        internal const string FighterCombatFeatSelectionGuid =
            "41c8486641f7d6d4283ca9dae4147a9f";
        private readonly BodyguardFeatPublicationTransaction<BlueprintFeature>
            _transaction;

        private BodyguardFeatCatalogPublication(
            BodyguardFeatPublicationTransaction<BlueprintFeature> transaction)
        { _transaction = transaction ?? throw new ArgumentNullException("transaction"); }

        internal static BodyguardFeatCatalogPublication Publish(
            LibraryScriptableObject library, BodyguardFeatBlueprintSet set)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (set == null) throw new ArgumentNullException("set");
            BlueprintFeatureSelection basic = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library, BasicFeatSelectionGuid,
                    "native basic feat selection");
            BlueprintFeatureSelection fighter = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library, FighterCombatFeatSelectionGuid,
                    "native Fighter combat-feat selection");
            var surfaces = new[]
            {
                new BodyguardPublicationSurface<BlueprintFeature>("basic.Features",
                    () => basic.Features, value => basic.Features = value),
                new BodyguardPublicationSurface<BlueprintFeature>("basic.AllFeatures",
                    () => basic.AllFeatures, value => basic.AllFeatures = value),
                new BodyguardPublicationSurface<BlueprintFeature>("fighter.Features",
                    () => fighter.Features, value => fighter.Features = value),
                new BodyguardPublicationSurface<BlueprintFeature>("fighter.AllFeatures",
                    () => fighter.AllFeatures, value => fighter.AllFeatures = value)
            };
            BodyguardFeatPublicationTransaction<BlueprintFeature> transaction =
                BodyguardFeatPublicationTransaction<BlueprintFeature>.Publish(surfaces,
                    new[] { set.Bodyguard, set.InHarmsWay }, value => value.AssetGuid,
                    value => value.Name ?? string.Empty);
            return new BodyguardFeatCatalogPublication(transaction);
        }

        internal void Rollback()
        { _transaction.Rollback(); }
    }
}
