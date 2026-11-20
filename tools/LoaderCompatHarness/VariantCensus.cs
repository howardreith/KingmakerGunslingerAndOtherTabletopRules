using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Lists the archetypes each named character class offers, in order,
    /// with identity and names. It reads the loaded library, changes
    /// nothing and does not use the product.
    /// </summary>
    internal static class ArchetypeCensus
    {
        /// <returns>Classes that could not be read at all.</returns>
        internal static List<string> Run(IEnumerable<string> classGuids, JObject into)
        {
            var failures = new List<string>();
            var classes = new JArray();
            LibraryScriptableObject library = ResourcesLibrary.LibraryObject;
            foreach (string guid in classGuids)
            {
                var record = new JObject { ["guid"] = guid };
                classes.Add(record);
                try
                {
                    BlueprintScriptableObject found = null;
                    if (library == null || library.BlueprintsByAssetId == null ||
                        !library.BlueprintsByAssetId.TryGetValue(guid, out found) || found == null)
                    {
                        record["fault"] = "not in the loaded library";
                        failures.Add(guid + " is not in the loaded library");
                        continue;
                    }

                    record["name"] = found.name;
                    Kingmaker.Blueprints.Classes.BlueprintCharacterClass characterClass =
                        found as Kingmaker.Blueprints.Classes.BlueprintCharacterClass;
                    if (characterClass == null)
                    {
                        record["fault"] = "not a character class: " + found.GetType().Name;
                        failures.Add(guid + " is not a character class");
                        continue;
                    }

                    var list = new JArray();
                    record["archetypes"] = list;
                    Kingmaker.Blueprints.Classes.BlueprintArchetype[] archetypes = characterClass.Archetypes;
                    if (archetypes == null) continue;
                    for (int index = 0; index < archetypes.Length; index++)
                    {
                        var entry = new JObject { ["index"] = index };
                        list.Add(entry);
                        try
                        {
                            Kingmaker.Blueprints.Classes.BlueprintArchetype archetype = archetypes[index];
                            if (archetype == null) { entry["fault"] = "null entry"; continue; }
                            entry["guid"] = archetype.AssetGuid;
                            entry["name"] = archetype.name;
                            try { entry["displayName"] = archetype.Name; }
                            catch (Exception exception) { entry["displayNameFault"] = exception.GetType().Name; }
                        }
                        catch (Exception exception)
                        {
                            entry["fault"] = exception.GetType().Name + ": " + exception.Message;
                        }
                    }
                }
                catch (Exception exception)
                {
                    record["fault"] = exception.GetType().Name + ": " + exception.Message;
                    failures.Add(guid + ": " + exception.GetType().Name);
                }
            }

            into["classes"] = classes;
            return failures;
        }
    }

    /// <summary>
    /// Lists what the final loaded library holds under named variant parents:
    /// each parent's variant components and every entry of each, in order,
    /// with its identity. It reads and changes nothing, and it does not use
    /// the product under test, so it can describe a published package whose
    /// own observers report only totals.
    /// </summary>
    internal static class VariantCensus
    {
        /// <returns>Parents that could not be read at all.</returns>
        internal static List<string> Run(IEnumerable<string> parentGuids, JObject into)
        {
            var failures = new List<string>();
            var parents = new JArray();
            int entries = 0, unreadable = 0;
            LibraryScriptableObject library = ResourcesLibrary.LibraryObject;
            foreach (string guid in parentGuids)
            {
                var record = new JObject { ["guid"] = guid };
                parents.Add(record);
                try
                {
                    BlueprintScriptableObject found = null;
                    if (library == null || library.BlueprintsByAssetId == null ||
                        !library.BlueprintsByAssetId.TryGetValue(guid, out found) || found == null)
                    {
                        record["fault"] = "not in the loaded library";
                        failures.Add(guid + " is not in the loaded library");
                        continue;
                    }

                    record["name"] = found.name;
                    record["type"] = found.GetType().Name;
                    var components = new JArray();
                    record["variantComponents"] = components;
                    foreach (AbilityVariants component in (found.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityVariants>())
                    {
                        var list = new JArray();
                        components.Add(new JObject { ["name"] = component == null ? null : component.name, ["entries"] = list });
                        BlueprintAbility[] variants = component == null ? null : component.Variants;
                        if (variants == null) continue;
                        for (int index = 0; index < variants.Length; index++)
                        {
                            var entry = new JObject { ["index"] = index };
                            list.Add(entry);
                            entries++;
                            try
                            {
                                BlueprintAbility variant = variants[index];
                                if (variant == null) { entry["fault"] = "null entry"; unreadable++; continue; }
                                entry["guid"] = variant.AssetGuid;
                                entry["name"] = variant.name;
                                entry["type"] = variant.GetType().Name;
                                entry["parentLink"] = variant.Parent == null ? null : variant.Parent.AssetGuid;
                                entry["hidden"] = variant.Hidden;
                                entry["showIfCasterHasFact"] = (variant.ComponentsArray ?? new BlueprintComponent[0])
                                    .Any(value => value != null && value.GetType().Name == "AbilityShowIfCasterHasFact");
                                try { entry["displayName"] = variant.Name; }
                                catch (Exception exception) { entry["displayNameFault"] = exception.GetType().Name; }
                            }
                            catch (Exception exception)
                            {
                                entry["fault"] = exception.GetType().Name + ": " + exception.Message;
                                unreadable++;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    record["fault"] = exception.GetType().Name + ": " + exception.Message;
                    failures.Add(guid + ": " + exception.GetType().Name);
                }
            }

            into["parents"] = parents;
            into["entryCount"] = entries;
            into["unreadableEntries"] = unreadable;
            return failures;
        }
    }
}
