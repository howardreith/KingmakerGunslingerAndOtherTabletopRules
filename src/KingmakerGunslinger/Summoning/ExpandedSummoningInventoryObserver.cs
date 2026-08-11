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
        private static readonly string[] ExactDonorGuids = {
            "1ed9a630f0d9d7f44855d3d1d1b2cdf2", "03dd28e92faf2e44eb9564a6ba01fdd0",
            "9e120b5e0ad3c794491c049aa24b9fde", "768275c9885dd954fb3c84ba69ac4281",
            "4109b40f6bbb49640840644cc84ada67", "6ec9c63c41a1e754ea4dcd85557625b4",
            "04944455200bc224d955a8e9bbd64f3f", "3764b43791a00e1468257adbca43ce9b",
            "2e24256e459468743b91fbb9aa85e1ab", "33bb90ffd13c87b4c8e45d920313752a",
            "50782bc4eb36aac4287023e20ee00808", "46779f56cab2cb0438161fec0129790d",
            "10a820de0a417f345866f794324205ad", "4615328295cd7e84bb2ef09d3dba8403",
            "ece348345859351439e1263115f5fdb9", "58574e8d1d4dc464c976f396d9115b1a",
            "beae4985629a6f64eb98081e3171e4c1", "028cc6f46e7998f46855a33ffde89567",
            "1832be68f9814254dbbdab6df7fd5d0b", "313a17cbd273d1f40bd1654ee2ae186e",
            "c3524f96954a1d94f8525b86e7626633", "6ea3a75279bab234aa723989e30cb15a",
            "0cc7a2526e4557945b1d8eb277d1fb3a", "58ed91a92b8d70248aa884d303954469",
            "394610e32cfbc4f43a0efaab16faae49"
        };
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

            var exact = new HashSet<string>(ExactDonorGuids, StringComparer.Ordinal);
            BlueprintUnit[] donors = all.OfType<BlueprintUnit>()
                .Where(value => exact.Contains(value.AssetGuid))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            foreach (BlueprintUnit value in donors)
                records.Add("donor=" + Describe(value) + ";fields=" + Members(value, 160) +
                    ";components=" + Components(value));
            string[] missingDonors = ExactDonorGuids.Where(guid =>
                !donors.Any(value => value.AssetGuid == guid)).ToArray();
            records.Add("donor-summary=expected:" + ExactDonorGuids.Length +
                ";found:" + donors.Length + ";missing:" + string.Join(",", missingDonors));

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
        { return Members(value, 40); }

        private static string Members(object value, int limit)
        {
            var rows = new List<string>();
            foreach (FieldInfo field in AllFields(value.GetType()).OrderBy(v =>
                v.DeclaringType.FullName + "." + v.Name, StringComparer.Ordinal))
            {
                object item;
                try { item = field.GetValue(value); } catch { continue; }
                string text = Scalar(item);
                if (text == null && item != null)
                    text = "<" + item.GetType().FullName + ">";
                if (text != null) rows.Add(field.DeclaringType.Name + "." + field.Name + "=" + text);
            }
            return string.Join(",", rows.Take(limit));
        }

        private static IEnumerable<FieldInfo> AllFields(Type type)
        {
            for (Type current = type; current != null; current = current.BaseType)
            foreach (FieldInfo field in current.GetFields(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                yield return field;
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
