using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Spells.ShieldOther;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class MagicCircleSpellListPublication
    {
        private static readonly FieldInfo FilteredCache = typeof(SpellLevelList)
            .GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ParameterCache = typeof(BlueprintParametrizedFeature)
            .GetField("m_CachedItems", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly List<Insertion> _insertions = new List<Insertion>();
        private readonly List<ParameterInsertion> _parameterInsertions = new List<ParameterInsertion>();

        internal static MagicCircleSpellListPublication Publish(LibraryScriptableObject library,
            MagicCircleBlueprintSet[] circles)
        {
            if (FilteredCache == null) throw new MissingFieldException(typeof(SpellLevelList).FullName, "m_SpellsFiltered");
            var publication = new MagicCircleSpellListPublication();
            try
            {
                publication.ReconcileNative(library, circles);
                return publication;
            }
            catch { publication.Rollback(); throw; }
        }

        internal void ReconcileNative(LibraryScriptableObject library, MagicCircleBlueprintSet[] circles)
        {
                foreach (string id in new[] {
                    "8443ce803d2d31347897a3d85cc32f53", // Cleric
                    "ba0401fdeb4062f40a7aa95b6f07fe89", // Sorcerer/Wizard shared list
                    "57c894665b7895c499b3dce058c284b3", // Inquisitor
                    "9f5be2f7ea64fe04eb40878347b147bc"  // Paladin (Evil/Chaos only)
                })
                {
                    var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, id, "Magic Circle class list");
                    Add(list, id == "9f5be2f7ea64fe04eb40878347b147bc" ?
                        MagicCircleBlueprints.PaladinFamily : MagicCircleBlueprints.Family);
                }
                // Native filtered lists are materialized arrays, not live views
                // over WizardSpellList. Respect their actual school filters.
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, "ba0401fdeb4062f40a7aa95b6f07fe89", "Wizard source list");
                foreach (string id in new[] { "c7a55e475659a944f9229d89c4dc3a8e", "280dd5167ccafe449a33fbe93c7a875e",
                    "17c0bfe5b7c8ac3449da655cdcaed4e7", "f3a8f76b1d030a64084355ba3eea369a", "c311aed33deb7a346ab715baef4a0572",
                    "5c08349132cb6b04181797f58ccf38ae", "ac551db78c1baa34eb8edca088be13cb", "5b154578f228c174bac546b6c29886ce" }) {
                    var filtered = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, id, "native Wizard specialist/Thassilonian list");
                    if (!ReferenceEquals(filtered.FilteredList, wizard)) throw new InvalidOperationException("Native school list ownership changed: " + id);
                    bool match = filtered.FilterSchool == SpellSchool.Abjuration || filtered.FilterSchool2 == SpellSchool.Abjuration;
                    if (filtered.ExcludeFilterSchool ? !match : match)
                        Add(filtered, MagicCircleBlueprints.Family);
                }
        }

        internal void Add(BlueprintSpellList list, BlueprintAbility spell)
        {
            var level = (list.SpellsByLevel ?? Array.Empty<SpellLevelList>()).Single(value => value != null && value.SpellLevel == 3);
            if (level.Spells == null) throw new InvalidOperationException("Null level-three spell list: " + list.AssetGuid);
            var matches = level.Spells.Where(value => value != null && value.AssetGuid == spell.AssetGuid).ToArray();
            if (matches.Length > 1 || (matches.Length == 1 && !ReferenceEquals(matches[0], spell)))
                throw new InvalidOperationException("Conflicting Magic Circle identity on list: " + list.AssetGuid);
            bool inserted = matches.Length == 0;
            SpellListComponent component = null;
            if (!(spell.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<SpellListComponent>()
                .Any(value => ReferenceEquals(value.SpellList, list) && value.SpellLevel == 3))
            {
                component = ScriptableObject.CreateInstance<SpellListComponent>();
                component.name = "$KMG_MagicCircle_List_" + list.AssetGuid;
                component.SpellList = list;
                component.SpellLevel = 3;
                spell.ComponentsArray = spell.ComponentsArray.Concat(new BlueprintComponent[] { component }).ToArray();
            }
            if (inserted || component != null)
                _insertions.Add(new Insertion { Level = level, Spell = spell, AddedEntry = inserted, Component = component });
            if (inserted) level.Spells = ShieldOtherSpellListMergePolicy.Merge(level.Spells, spell, value => value.AssetGuid);
            FilteredCache.SetValue(level, null);
        }

        internal void AddFavoredParameter(BlueprintParametrizedFeature feature, BlueprintAbility spell)
        {
            if (ParameterCache == null) throw new MissingFieldException(typeof(BlueprintParametrizedFeature).FullName, "m_CachedItems");
            var variants = feature.BlueprintParameterVariants;
            if (variants == null) throw new InvalidOperationException("Null Favored Class parameters: " + feature.AssetGuid);
            var matches = variants.Where(value => value != null && value.AssetGuid == spell.AssetGuid).ToArray();
            if (matches.Length > 1 || (matches.Length == 1 && !ReferenceEquals(matches[0], spell)))
                throw new InvalidOperationException("Conflicting Magic Circle parameter identity: " + feature.AssetGuid);
            if (matches.Length == 0) {
                _parameterInsertions.Add(new ParameterInsertion { Feature = feature, Spell = spell });
                feature.BlueprintParameterVariants = variants.Concat(new BlueprintScriptableObject[] { spell }).ToArray();
            }
            // Kingmaker CanSelect checks Items, which is cached separately from
            // the live spell-list extraction. Invalidate only this exact selector.
            ParameterCache.SetValue(feature, null);
        }

        internal void Rollback()
        {
            for (int i = _parameterInsertions.Count - 1; i >= 0; i--) {
                var insertion = _parameterInsertions[i];
                if (insertion.Feature.BlueprintParameterVariants == null) continue;
                var current = insertion.Feature.BlueprintParameterVariants.ToList();
                int owned = current.FindIndex(value => ReferenceEquals(value, insertion.Spell));
                if (owned < 0) continue;
                current.RemoveAt(owned);
                insertion.Feature.BlueprintParameterVariants = current.ToArray();
                ParameterCache.SetValue(insertion.Feature, null);
            }
            _parameterInsertions.Clear();
            for (int i = _insertions.Count - 1; i >= 0; i--)
            {
                var insertion = _insertions[i];
                // Preserve foreign replacements and later additions. Never restore
                // an old list snapshot over changes made by another publisher.
                if (insertion.AddedEntry && insertion.Level.Spells != null)
                {
                    var current = new List<BlueprintAbility>(insertion.Level.Spells);
                    int owned = current.FindIndex(value => ReferenceEquals(value, insertion.Spell));
                    if (owned >= 0) current.RemoveAt(owned);
                    insertion.Level.Spells = current;
                    FilteredCache.SetValue(insertion.Level, null);
                }
                if (insertion.Component != null)
                    insertion.Spell.ComponentsArray = insertion.Spell.ComponentsArray
                        .Where(value => !ReferenceEquals(value, insertion.Component)).ToArray();
            }
            _insertions.Clear();
        }

        private sealed class Insertion
        {
            internal SpellLevelList Level;
            internal BlueprintAbility Spell;
            internal bool AddedEntry;
            internal SpellListComponent Component;
        }

        private sealed class ParameterInsertion
        {
            internal BlueprintParametrizedFeature Feature;
            internal BlueprintAbility Spell;
        }
    }
}
