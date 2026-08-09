using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class AcadamaeFeatCatalogPublication
    {
        private readonly BlueprintFeatureSelection _selection;
        private readonly BlueprintFeature[] _featuresBefore;
        private readonly BlueprintFeature[] _allBefore;
        private readonly BlueprintFeature[] _featuresPublished;
        private readonly BlueprintFeature[] _allPublished;

        private AcadamaeFeatCatalogPublication(BlueprintFeatureSelection selection,
            BlueprintFeature[] featuresBefore, BlueprintFeature[] allBefore,
            BlueprintFeature[] featuresPublished, BlueprintFeature[] allPublished)
        { _selection = selection; _featuresBefore = featuresBefore; _allBefore = allBefore;
            _featuresPublished = featuresPublished; _allPublished = allPublished; }

        internal static AcadamaeFeatCatalogPublication Publish(
            LibraryScriptableObject library, BlueprintFeature feat)
        {
            if (library == null || feat == null) throw new ArgumentNullException();
            BlueprintFeatureSelection selection = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library, "247a4068296e8be42890143f451b4b45",
                    "native basic feat selection");
            BlueprintFeature[] featuresBefore = selection.Features;
            BlueprintFeature[] allBefore = selection.AllFeatures;
            BlueprintFeature[] featuresPublished = Merge(featuresBefore, feat);
            BlueprintFeature[] allPublished = Merge(allBefore, feat);
            selection.Features = featuresPublished;
            selection.AllFeatures = allPublished;
            Validate(selection.Features, feat, "Features");
            Validate(selection.AllFeatures, feat, "AllFeatures");
            return new AcadamaeFeatCatalogPublication(selection, featuresBefore,
                allBefore, featuresPublished, allPublished);
        }

        internal void Rollback()
        {
            if (!ReferenceEquals(_selection.Features, _featuresPublished) ||
                !ReferenceEquals(_selection.AllFeatures, _allPublished))
                throw new InvalidOperationException(
                    "Basic feat selection changed after Acadamae publication; rollback refused.");
            _selection.Features = _featuresBefore;
            _selection.AllFeatures = _allBefore;
        }

        private static BlueprintFeature[] Merge(BlueprintFeature[] current,
            BlueprintFeature feat)
        {
            current = current ?? Array.Empty<BlueprintFeature>();
            if (current.Any(value => value == null))
                throw new InvalidOperationException("Basic feat selection contains a null entry.");
            BlueprintFeature[] retained = current.Where(value =>
                !ReferenceEquals(value, feat) && !string.Equals(value.AssetGuid,
                    feat.AssetGuid, StringComparison.Ordinal)).ToArray();
            int insertion = FindInsertion(retained, feat);
            var result = new BlueprintFeature[retained.Length + 1];
            Array.Copy(retained, 0, result, 0, insertion);
            result[insertion] = feat;
            Array.Copy(retained, insertion, result, insertion + 1,
                retained.Length - insertion);
            return result;
        }

        private static int FindInsertion(BlueprintFeature[] current,
            BlueprintFeature feat)
        {
            CompareInfo compare = CultureInfo.CurrentUICulture.CompareInfo;
            string candidate = feat.Name ?? string.Empty;
            for (int i = 0; i < current.Length; i++)
                if (compare.Compare(candidate, current[i].Name ?? string.Empty,
                    CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) < 0)
                    return i;
            return current.Length;
        }

        private static void Validate(BlueprintFeature[] values,
            BlueprintFeature feat, string role)
        {
            int exact = values.Count(value => ReferenceEquals(value, feat));
            int guid = values.Count(value => value != null && string.Equals(
                value.AssetGuid, feat.AssetGuid, StringComparison.Ordinal));
            if (exact != 1 || guid != 1)
                throw new InvalidOperationException("Acadamae publication " + role +
                    " is not singular by reference and GUID.");
        }
    }
}
