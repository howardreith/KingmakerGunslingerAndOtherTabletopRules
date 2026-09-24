using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// Publication-time icon completion. Optional provider donors (Oracle
    /// revelations, Call of the Wild performances and the eidolon feature)
    /// are created after KMG registers, so their counters receive the donor's
    /// icon by exact identity when the publication commits. Any counter that
    /// still has no icon is withheld: no blank choice is ever published.
    /// </summary>
    internal static class FavoredClassLeafIcons
    {
        /// <summary>Assigns provider icons; returns the counters withheld for a missing icon.</summary>
        internal static IList<string> Complete(LibraryScriptableObject library, FavoredClassBlueprintSet set,
            IList<string> evidence)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (set == null) throw new ArgumentNullException("set");
            var withheld = new List<string>();
            BlueprintUnitFactAccess access = BlueprintUnitFactAccess.Resolve();
            foreach (FavoredClassLeafPair pair in set.Pairs)
            {
                if (pair.Leaves.All(leaf => leaf.Icon != null))
                    continue;
                FavoredClassIconDonor donor = FavoredClassIconPolicy.For(pair.Effect.Id, pair.TargetKey);
                Sprite icon = null;
                string used = null;
                foreach (string guid in donor.Guids)
                {
                    BlueprintScriptableObject blueprint;
                    library.BlueprintsByAssetId.TryGetValue(guid, out blueprint);
                    Sprite candidate = IconOf(blueprint);
                    if (candidate != null)
                    {
                        icon = candidate;
                        used = guid;
                        break;
                    }
                }
                string key = pair.TargetKey == null ? pair.Effect.Id : FavoredClassRuntime.TargetKey(pair.Effect.Id,
                    pair.TargetKey);
                if (icon == null)
                {
                    withheld.Add(key);
                    evidence.Add("icon-missing:" + key);
                    continue;
                }
                foreach (BlueprintFeature leaf in pair.Leaves)
                    if (leaf.Icon == null)
                        access.SetIcon(leaf, icon);
                evidence.Add("icon:" + key + "=" + used);
            }
            return withheld.AsReadOnly();
        }

        /// <summary>A donor's art: a unit fact's icon, or a class's own icon.</summary>
        private static Sprite IconOf(BlueprintScriptableObject blueprint)
        {
            var fact = blueprint as BlueprintUnitFact;
            if (fact != null)
                return fact.Icon;
            var characterClass = blueprint as BlueprintCharacterClass;
            return characterClass == null ? null : characterClass.Icon;
        }
    }
}
