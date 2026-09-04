using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BodyguardFeats;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalFeatPublication
    {
        internal const string BasicFeatSelectionGuid =
            "247a4068296e8be42890143f451b4b45";
        internal const string FighterCombatFeatSelectionGuid =
            "41c8486641f7d6d4283ca9dae4147a9f";

        private readonly BodyguardFeatPublicationTransaction<BlueprintFeature>
            m_Basic;
        private readonly BodyguardFeatPublicationTransaction<BlueprintFeature>
            m_Fighter;

        private ElementalFeatPublication(
            BodyguardFeatPublicationTransaction<BlueprintFeature> basic,
            BodyguardFeatPublicationTransaction<BlueprintFeature> fighter)
        {
            m_Basic = basic;
            m_Fighter = fighter;
        }

        internal static ElementalFeatPublication Apply(
            LibraryScriptableObject library, ElementalFeatBlueprintSet set,
            bool moduleActive)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (set == null) throw new ArgumentNullException("set");
            if (!moduleActive)
                return new ElementalFeatPublication(null, null);

            BlueprintFeatureSelection basic = BlueprintLibraryLookup
                .RequireExact<BlueprintFeatureSelection>(library,
                    BasicFeatSelectionGuid, "native basic feat selection");
            BlueprintFeatureSelection fighter = BlueprintLibraryLookup
                .RequireExact<BlueprintFeatureSelection>(library,
                    FighterCombatFeatSelectionGuid,
                    "native Fighter combat-feat selection");
            BodyguardFeatPublicationTransaction<BlueprintFeature> basicTx =
                BodyguardFeatPublicationTransaction<BlueprintFeature>.Publish(
                    Surfaces("basic", basic), set.AllFeats(),
                    value => value.AssetGuid,
                    value => value.Name ?? string.Empty);
            try
            {
                BodyguardFeatPublicationTransaction<BlueprintFeature>
                    fighterTx = BodyguardFeatPublicationTransaction<
                        BlueprintFeature>.Publish(
                            Surfaces("fighter", fighter), set.CombatFeats(),
                            value => value.AssetGuid,
                            value => value.Name ?? string.Empty);
                return new ElementalFeatPublication(basicTx, fighterTx);
            }
            catch
            {
                basicTx.Rollback();
                throw;
            }
        }

        internal void Rollback()
        {
            if (m_Fighter != null) m_Fighter.Rollback();
            if (m_Basic != null) m_Basic.Rollback();
        }

        private static BodyguardPublicationSurface<BlueprintFeature>[]
            Surfaces(string role, BlueprintFeatureSelection selection)
        {
            return new[]
            {
                new BodyguardPublicationSurface<BlueprintFeature>(
                    role + ".Features", () => selection.Features,
                    value => selection.Features = value),
                new BodyguardPublicationSurface<BlueprintFeature>(
                    role + ".AllFeatures", () => selection.AllFeatures,
                    value => selection.AllFeatures = value)
            };
        }
    }
}
