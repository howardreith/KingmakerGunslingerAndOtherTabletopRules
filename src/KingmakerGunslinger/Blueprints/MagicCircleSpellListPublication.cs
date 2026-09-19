using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
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
        private readonly List<Insertion> _insertions = new List<Insertion>();

        internal static MagicCircleSpellListPublication Publish(LibraryScriptableObject library,
            MagicCircleBlueprintSet[] circles)
        {
            if (FilteredCache == null) throw new MissingFieldException(typeof(SpellLevelList).FullName, "m_SpellsFiltered");
            var publication = new MagicCircleSpellListPublication();
            try
            {
                foreach (string id in new[] {
                    "8443ce803d2d31347897a3d85cc32f53", // Cleric
                    "ba0401fdeb4062f40a7aa95b6f07fe89", // Sorcerer/Wizard shared list
                    "57c894665b7895c499b3dce058c284b3", // Inquisitor
                    "9f5be2f7ea64fe04eb40878347b147bc"  // Paladin (Evil/Chaos only)
                })
                {
                    var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, id, "Magic Circle class list");
                    foreach (var circle in circles)
                        if (id != "9f5be2f7ea64fe04eb40878347b147bc" || circle.Alignment == "Evil" || circle.Alignment == "Chaos")
                            publication.Add(list, circle.Spell);
                }
                return publication;
            }
            catch { publication.Rollback(); throw; }
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
            _insertions.Add(new Insertion { Level = level, Spell = spell, AddedEntry = inserted, Component = component });
            if (inserted) level.Spells = ShieldOtherSpellListMergePolicy.Merge(level.Spells, spell, value => value.AssetGuid);
            FilteredCache.SetValue(level, null);
        }

        internal void Rollback()
        {
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
    }
}
