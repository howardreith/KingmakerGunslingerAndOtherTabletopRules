using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>
    /// Icons of the Mostly Human choices, completed after the project icon
    /// stage has assigned the parent races' own art: the Standard entry shows
    /// its parent race, the selector and the trait show the native Human
    /// ancestry: the Human race's icon, else the native human Skilled racial
    /// feature's (Kingmaker's Human race carries none), else the parent race's.
    /// A choice that would still be blank is never published.
    /// </summary>
    internal static class ElementalMostlyHumanIcons
    {
        internal const string HumanRaceGuid = "0a5d473ead98b0646b94495af250fdc4";

        /// <summary>Kingmaker's Human racial feature Skilled.</summary>
        internal const string HumanSkilledGuid = "3adf9274a210b164cb68f472dc1e4544";

        /// <summary>
        /// Assigns every missing icon; returns false (with evidence) when a
        /// visible Mostly Human choice still has no icon.
        /// </summary>
        internal static bool Complete(ElementalMostlyHumanBlueprintSet set, LibraryScriptableObject library,
            IList<string> evidence)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (library == null) throw new ArgumentNullException("library");
            BlueprintScriptableObject humanBlueprint, skilledBlueprint;
            library.BlueprintsByAssetId.TryGetValue(HumanRaceGuid, out humanBlueprint);
            library.BlueprintsByAssetId.TryGetValue(HumanSkilledGuid, out skilledBlueprint);
            var human = humanBlueprint as BlueprintRace;
            var skilled = skilledBlueprint as BlueprintFeature;
            Sprite humanAncestry = human != null && human.Icon != null ? human.Icon :
                skilled != null ? skilled.Icon : null;
            BlueprintUnitFactAccess access = BlueprintUnitFactAccess.Resolve();
            bool complete = true;
            foreach (ElementalMostlyHumanRaceBlueprints race in set.Races)
            {
                Sprite parent = race.Race.Icon;
                Sprite humanIcon = humanAncestry != null ? humanAncestry : parent;
                Assign(access, race.Standard, parent, evidence);
                Assign(access, race.Trait, humanIcon, evidence);
                Assign(access, race.Selection, humanIcon, evidence);
                foreach (BlueprintUnitFact fact in new BlueprintUnitFact[] { race.Selection, race.Standard, race.Trait })
                    if (fact.Icon == null)
                    {
                        complete = false;
                        evidence.Add("icon-missing:" + fact.name);
                    }
            }
            evidence.Add("human-race-icon=" + (human != null && human.Icon != null) + ";human-skilled-icon=" +
                (skilled != null && skilled.Icon != null));
            return complete;
        }

        private static void Assign(BlueprintUnitFactAccess access, BlueprintUnitFact fact, Sprite icon,
            IList<string> evidence)
        {
            if (fact.Icon != null || icon == null)
                return;
            access.SetIcon(fact, icon);
            evidence.Add("icon:" + fact.name + "=" + icon.name);
        }
    }
}
