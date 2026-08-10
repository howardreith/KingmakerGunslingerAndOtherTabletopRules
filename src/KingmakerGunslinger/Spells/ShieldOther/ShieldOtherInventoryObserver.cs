using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal sealed class ShieldOtherInventoryObservation
    {
        internal ShieldOtherInventoryObservation(int duplicateCount,
            IList<string> records)
        { DuplicateCount = duplicateCount; Records = records; }
        internal int DuplicateCount { get; private set; }
        internal IList<string> Records { get; private set; }
    }

    internal static class ShieldOtherInventoryObserver
    {
        internal static ShieldOtherInventoryObservation Observe(
            LibraryScriptableObject library)
        {
            if (library == null) throw new ArgumentNullException("library");
            BlueprintScriptableObject[] all = library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            var records = new List<string>();
            BlueprintAbility[] duplicates = all.OfType<BlueprintAbility>()
                .Where(IsShieldOtherCandidate).ToArray();
            foreach (BlueprintAbility value in duplicates)
                records.Add("duplicate=" + Describe(value));

            foreach (BlueprintAbility value in all.OfType<BlueprintAbility>()
                .Where(IsAbilityDonor).OrderBy(value => value.AssetGuid,
                    StringComparer.Ordinal).Take(80))
                records.Add("abilityDonor=" + Describe(value) + ";range=" +
                    value.Range + ";friends=" + value.CanTargetFriends +
                    ";self=" + value.CanTargetSelf + ";components=" +
                    Components(value));

            foreach (BlueprintBuff value in all.OfType<BlueprintBuff>()
                .Where(IsBuffDonor).OrderBy(value => value.AssetGuid,
                    StringComparer.Ordinal).Take(100))
                records.Add("buffDonor=" + Describe(value) + ";components=" +
                    Components(value));

            foreach (BlueprintSpellList value in all.OfType<BlueprintSpellList>()
                .Where(IsRelevantList).OrderBy(value => value.AssetGuid,
                    StringComparer.Ordinal))
                records.Add("spellList=" + Describe(value) + ";maxLevel=" +
                    value.MaxLevel);

            foreach (BlueprintCharacterClass value in all
                .OfType<BlueprintCharacterClass>().Where(IsRelevantClass)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                BlueprintSpellbook book = value.Spellbook;
                records.Add("class=" + Describe(value) + ";spellbook=" +
                    (book == null ? "<null>" : Describe(book)) + ";spellList=" +
                    (book == null || book.SpellList == null ? "<null>" :
                        Describe(book.SpellList)));
            }
            records.Add("inventoryTotal=" + all.Length + ";abilityDonors=" +
                records.Count(value => value.StartsWith("abilityDonor=",
                    StringComparison.Ordinal)) + ";buffDonors=" +
                records.Count(value => value.StartsWith("buffDonor=",
                    StringComparison.Ordinal)));
            return new ShieldOtherInventoryObservation(duplicates.Length, records);
        }

        private static bool IsShieldOtherCandidate(BlueprintAbility value)
        {
            string text = SearchText(value);
            return text.Contains("shield other") || text.Contains("shieldother") ||
                text.Contains("shield_other");
        }

        private static bool IsAbilityDonor(BlueprintAbility value)
        {
            string text = SearchText(value);
            return value.Range.ToString() == "Close" && value.CanTargetFriends &&
                !value.CanTargetSelf || ContainsAny(text, "aid", "resistance",
                    "protection", "shield", "dismiss");
        }

        private static bool IsBuffDonor(BlueprintBuff value)
        {
            string text = SearchText(value) + " " + Components(value).ToLowerInvariant();
            return ContainsAny(text, "deflection", "resistance", "savingthrow",
                "armorclass", "caster", "distance", "dismiss");
        }

        private static bool IsRelevantList(BlueprintSpellList value)
        {
            string text = SearchText(value);
            return ContainsAny(text, "cleric", "paladin", "inquisitor",
                "community", "protection", "friendship", "martyr", "oracle",
                "warpriest", "psychic");
        }

        private static bool IsRelevantClass(BlueprintCharacterClass value)
        {
            string text = SearchText(value);
            return ContainsAny(text, "cleric", "paladin", "inquisitor", "oracle",
                "warpriest", "psychic");
        }

        private static string SearchText(BlueprintScriptableObject value)
        { return (value.name + " " + Display(value)).ToLowerInvariant(); }

        private static string Display(BlueprintScriptableObject value)
        {
            PropertyInfo property = value.GetType().GetProperty("Name",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object result = property == null ? null : property.GetValue(value, null);
            return result == null ? string.Empty : result.ToString();
        }

        private static string Components(BlueprintScriptableObject value)
        {
            return string.Join("|", (value.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(component => component != null)
                .Select(component => component.GetType().FullName).ToArray());
        }

        private static string Describe(BlueprintScriptableObject value)
        {
            return value.AssetGuid + ":" + value.name + ":" + Display(value) +
                ":" + value.GetType().FullName + ":" +
                value.GetType().Assembly.GetName().Name;
        }

        private static bool ContainsAny(string text, params string[] terms)
        { return terms.Any(term => text.Contains(term)); }
    }
}
