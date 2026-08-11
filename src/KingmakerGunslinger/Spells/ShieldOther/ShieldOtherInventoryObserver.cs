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
            int expectedPublishedLists, int publishedLists, IList<string> records)
        { DuplicateCount = duplicateCount; ExpectedPublishedLists = expectedPublishedLists;
            PublishedLists = publishedLists; Records = records; }
        internal int DuplicateCount { get; private set; }
        internal int ExpectedPublishedLists { get; private set; }
        internal int PublishedLists { get; private set; }
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
            const string ShieldOtherGuid = "6a8c4c1d2fbe4d6a9a724988c1348401";
            BlueprintAbility[] duplicates = all.OfType<BlueprintAbility>()
                .Where(value => !string.Equals(value.AssetGuid, ShieldOtherGuid,
                    StringComparison.Ordinal) && IsShieldOtherCandidate(value)).ToArray();
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

            string[] requiredLists = {
                "8443ce803d2d31347897a3d85cc32f53",
                "9f5be2f7ea64fe04eb40878347b147bc",
                "57c894665b7895c499b3dce058c284b3",
                "75576ed8cab010644a11f9ecd512a7f9",
                "93228f4df23d2d448a0db59141af8aed" };
            string[] optionalLists = {
                "f305174b73f64783a8379238a14c3283",
                "9ef48172d50446aca4c80f321402f743",
                "d8eda7e863824c42b3329279cac4d92a" };
            var liveLists = all.OfType<BlueprintSpellList>().ToDictionary(
                value => value.AssetGuid, StringComparer.Ordinal);
            int expectedPublishedLists = requiredLists.Length;
            int publishedLists = 0;
            foreach (string guid in requiredLists.Concat(optionalLists))
            {
                BlueprintSpellList list;
                bool required = requiredLists.Contains(guid);
                if (!liveLists.TryGetValue(guid, out list))
                {
                    if (required) records.Add("publication=" + guid + ";missing=true");
                    continue;
                }
                if (!required) expectedPublishedLists++;
                SpellLevelList level = (list.SpellsByLevel ??
                    Array.Empty<SpellLevelList>()).SingleOrDefault(value =>
                        value != null && value.SpellLevel == 2);
                int membership = level == null || level.Spells == null ? 0 :
                    level.Spells.Count(value => value != null && string.Equals(
                        value.AssetGuid, ShieldOtherGuid, StringComparison.Ordinal));
                if (membership == 1) publishedLists++;
                records.Add("publication=" + guid + ";required=" + required +
                    ";level2Membership=" + membership);
            }

            foreach (BlueprintCharacterClass value in all
                .OfType<BlueprintCharacterClass>().Where(IsRelevantClass)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                BlueprintSpellbook book = value.Spellbook;
                records.Add("class=" + Describe(value) + ";spellbook=" +
                    (book == null ? "<null>" : Describe(book)) + ";spellList=" +
                    (book == null || book.SpellList == null ? "<null>" :
                        Describe(book.SpellList)) + ";spontaneous=" +
                    (book == null ? "<null>" : book.Spontaneous.ToString()) +
                    ";arcane=" + (book == null ? "<null>" : book.IsArcane.ToString()) +
                    ";castingAttribute=" + (book == null ? "<null>" :
                        book.CastingAttribute.ToString()));
            }
            records.Add("inventoryTotal=" + all.Length + ";abilityDonors=" +
                records.Count(value => value.StartsWith("abilityDonor=",
                    StringComparison.Ordinal)) + ";buffDonors=" +
                records.Count(value => value.StartsWith("buffDonor=",
                    StringComparison.Ordinal)));
            return new ShieldOtherInventoryObservation(duplicates.Length,
                expectedPublishedLists, publishedLists, records);
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
