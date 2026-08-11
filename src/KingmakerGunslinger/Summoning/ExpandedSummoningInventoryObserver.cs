using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class ExpandedSummoningInventoryObservation
    {
        internal ExpandedSummoningInventoryObservation(int parents, int units,
            int facts, IList<string> records)
        { ParentCount = parents; UnitCount = units; FactCount = facts; Records = records; }
        internal int ParentCount { get; private set; }
        internal int UnitCount { get; private set; }
        internal int FactCount { get; private set; }
        internal IList<string> Records { get; private set; }
    }

    internal static class ExpandedSummoningInventoryObserver
    {
        private static readonly string[] UnitTerms = ExpandedSummoningCatalog.All
            .SelectMany(value => new[] { value.DisplayName, value.Visual })
            .SelectMany(value => value.Split(new[] { '/', ' ' },
                StringSplitOptions.RemoveEmptyEntries))
            .Where(value => value.Length >= 4)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        internal static ExpandedSummoningInventoryObservation Observe(
            LibraryScriptableObject library)
        {
            if (library == null) throw new ArgumentNullException("library");
            BlueprintScriptableObject[] all = library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            var records = new List<string>();
            BlueprintAbility[] parents = all.OfType<BlueprintAbility>()
                .Where(value => IsSummonFamily(SearchText(value)))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            foreach (BlueprintAbility value in parents)
                records.Add("ability=" + Describe(value) + ";range=" + value.Range +
                    ";action=" + value.ActionType + ";components=" + Components(value));

            BlueprintUnit[] units = all.OfType<BlueprintUnit>()
                .Where(value => IsUnitCandidate(SearchText(value)))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            foreach (BlueprintUnit value in units)
                records.Add("unit=" + Describe(value) + ";components=" + Components(value) +
                    ";facts=" + References(value, "AddFacts") +
                    ";body=" + References(value, "Body") +
                    ";view=" + References(value, "Prefab"));

            BlueprintScriptableObject[] facts = all.Where(value =>
                !(value is BlueprintAbility) && !(value is BlueprintUnit) &&
                ContainsAny(SearchText(value), "augment summoning", "augmentsummoning",
                    "superior summoning", "superiorsummoning", "sacred summons",
                    "sacredsummons", "summon pool", "summonpool", "summoned unit",
                    "summonedunit", "celestial", "fiendish"))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).Take(500).ToArray();
            foreach (BlueprintScriptableObject value in facts)
                records.Add("fact=" + Describe(value) + ";components=" + Components(value));
            records.Add("summary=all:" + all.Length + ";abilities:" + parents.Length +
                ";units:" + units.Length + ";facts:" + facts.Length);
            return new ExpandedSummoningInventoryObservation(parents.Length,
                units.Length, facts.Length, records);
        }

        private static string Components(BlueprintScriptableObject value)
        {
            return string.Join("|", (value.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Where(component => component != null).Select(component =>
                    component.GetType().FullName + "{" + Members(component) + "}"));
        }

        private static string Members(object value)
        {
            var rows = new List<string>();
            foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(v => v.Name))
            {
                object item;
                try { item = field.GetValue(value); } catch { continue; }
                string text = Scalar(item);
                if (text != null) rows.Add(field.Name + "=" + text);
            }
            return string.Join(",", rows.Take(40));
        }

        private static string References(object owner, string term)
        {
            var rows = new List<string>();
            foreach (FieldInfo field in owner.GetType().GetFields(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) continue;
                object value; try { value = field.GetValue(owner); } catch { continue; }
                rows.Add(field.Name + "=" + (Scalar(value) ?? value.GetType().FullName));
            }
            return string.Join("|", rows);
        }

        private static string Scalar(object value)
        {
            if (value == null) return "<null>";
            BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return Describe(blueprint);
            Type type = value.GetType();
            if (type.IsEnum || type.IsPrimitive || value is string || value is decimal)
                return value.ToString();
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null && !(value is string))
            {
                var rows = new List<string>();
                foreach (object item in sequence) {
                    BlueprintScriptableObject reference = item as BlueprintScriptableObject;
                    rows.Add(reference == null ? (item == null ? "<null>" : item.ToString()) : Describe(reference));
                    if (rows.Count >= 30) break;
                }
                return "[" + string.Join(",", rows) + "]";
            }
            return null;
        }

        private static string SearchText(BlueprintScriptableObject value)
        { return (value.name + " " + Display(value)).ToLowerInvariant(); }
        private static string Display(BlueprintScriptableObject value)
        {
            PropertyInfo property = value.GetType().GetProperty("Name", BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance);
            object result = property == null ? null : property.GetValue(value, null);
            return result == null ? string.Empty : result.ToString();
        }
        private static bool IsSummonFamily(string text)
        { return ContainsAny(text, "summonmonster", "summon monster", "summonnature",
            "summon nature", "summon_nature", "summon_monster"); }
        private static bool IsUnitCandidate(string text)
        { return UnitTerms.Any(term => text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0); }
        private static bool ContainsAny(string text, params string[] terms)
        { return terms.Any(text.Contains); }
        private static string Describe(BlueprintScriptableObject value)
        { return value.AssetGuid + ":" + value.name + ":" + Display(value) + ":" +
            value.GetType().FullName + ":" + value.GetType().Assembly.GetName().Name; }
    }
}
