using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class ExpandedSummoningInventoryObservation
    {
        internal ExpandedSummoningInventoryObservation(int parents, int units,
            int facts, int exactDonors, int missingDonors, int specialCandidates,
            IList<string> records)
        {
            ParentCount = parents; UnitCount = units; FactCount = facts;
            ExactDonorCount = exactDonors; MissingDonorCount = missingDonors;
            SpecialCandidateCount = specialCandidates;
            Records = records;
        }
        internal int ParentCount { get; private set; }
        internal int UnitCount { get; private set; }
        internal int FactCount { get; private set; }
        internal int ExactDonorCount { get; private set; }
        internal int MissingDonorCount { get; private set; }
        internal int SpecialCandidateCount { get; private set; }
        internal IList<string> Records { get; private set; }
    }

    internal static class ExpandedSummoningInventoryObserver
    {
        private static readonly string[] ExactDonorGuids =
            ExpandedSummoningDonorCatalog.All.Select(value => value.Guid)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
        private static readonly string[] CanonicalParentGuids = {
            "8fd74eddd9b6c224693d9ab241f25e84", "1724061e89c667045a6891179ee2e8e7",
            "5d61dde0020bbf54ba1521f7ca0229dc", "7ed74a3ec8c458d4fb50b192fd7be6ef",
            "630c8b85d9f07a64f917d79cb5905741", "e740afbab0147944dab35d83faa0ae1c",
            "ab167fd8203c1314bac6568932f1752f", "d3ac756a229830243a72e84f3ab050d0",
            "52b5df2a97df18242aec67610616ded0", "c6147854641924442a3bb736080cfeb6",
            "298148133cdc3fd42889b99c82711986", "fdcf7e57ec44f704591f11b45f4acf61",
            "c83db50513abdf74ca103651931fac4b", "8f98a22f35ca6684a983363d32e51bfe",
            "55bbce9b3e76d4a4a8c8e0698d29002c", "051b979e7d7f8ec41b9fa35d04746b33",
            "ea78c04f0bd13d049a1cce5daf8d83e0", "a7469ef84ba50ac4cbf3d145e3173f8e"
        };
        private static readonly string[] ExactTemplateMechanicGuids = {
            "69f0d7d1077f492f8237952f8219a270", "3e33af2ab5974859bdaa92c32987b3e0",
            "bf0882a6d254407bb259356f1aa66392", "a432066702694b2590260b58426fee28",
            "0e7481a8ceb041129a692bf59f24d057", "46a19a521e0d40f792d8b4f64931be8a",
            "368bc4311f7f4ba9af3752ff4418d0a8", "4170f7f5874a4e45bc7050a53727452f",
            "a203d617f8d547459e1f25790f886b6e", "f009c072167c4b53a37c1071a2251c3f",
            "320b92730bd54842b9707931a5dbab18", "b4274c5bb0bf2ad4190eb7c44859048b"
        };
        private static readonly string[] UnitTerms = ExpandedSummoningCatalog.All
            .SelectMany(value => new[] { value.DisplayName, value.Visual })
            .SelectMany(value => value.Split(new[] { '/', ' ' },
                StringSplitOptions.RemoveEmptyEntries))
            .Where(value => value.Length >= 4)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        private static readonly string[] SpecialMechanicTerms = {
            "willowisp", "will-o-wisp", "will o wisp", "willo",
            "lanternarchon", "lantern archon", "lightray", "light ray",
            "auraofmenace", "aura of menace", "salamander",
            "invisiblestalker", "invisible stalker", "naturalinvisibility",
            "natural invisibility", "shadowdemon", "shadow demon",
            "incorporeal", "succubus", "energydrain", "energy drain",
            "profanegift", "profane gift"
        };
        private static readonly string[] ExactSpecialMechanicGuids = {
            "24719a49b84c5cd43b894268d22d9c89",
            "33e8997912cf76b4c99dca0445082804",
            "dcfc5e9aec5bea540b36caf754989164",
            "1ce4878b5e714f659d0854a12f4b3cf2"
        };

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
                    ";components=" + Components(value) + ";component-graph=" +
                    ObjectGraph(value.ComponentsArray, 8) + ";body-graph=" +
                    ObjectGraph(FieldValue(value, "Body"), 8) + ";view-graph=" +
                    ObjectGraph(FieldValue(value, "Prefab"), 8));
            string[] missingDonors = ExactDonorGuids.Where(guid =>
                !donors.Any(value => value.AssetGuid == guid)).ToArray();
            records.Add("donor-summary=expected:" + ExactDonorGuids.Length +
                ";found:" + donors.Length + ";missing:" + string.Join(",", missingDonors));

            BlueprintScriptableObject[] specialCandidates = all.Where(value =>
                ContainsAny(SearchText(value), SpecialMechanicTerms))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            BlueprintScriptableObject[] specialIndex = specialCandidates.Take(300)
                .ToArray();
            foreach (BlueprintScriptableObject value in specialIndex)
                records.Add("special-index=" + Describe(value) + ";components=" +
                    string.Join(",", (value.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Where(component =>
                            component != null).Select(component =>
                                component.GetType().FullName)));
            var exactSpecial = new HashSet<string>(ExactSpecialMechanicGuids,
                StringComparer.Ordinal);
            BlueprintScriptableObject[] specialDetails = specialCandidates.Where(value =>
                exactSpecial.Contains(value.AssetGuid)).ToArray();
            foreach (BlueprintScriptableObject value in specialDetails)
            {
                BlueprintUnit unit = value as BlueprintUnit;
                records.Add("special-detail=" + Describe(value) + ";fields=" +
                    Members(value, 160) + ";components=" + Components(value) +
                    ";graph=" + ObjectGraph(value.ComponentsArray, 12) +
                    (unit == null ? string.Empty : ";body-graph=" +
                        ObjectGraph(FieldValue(unit, "Body"), 8) + ";view-graph=" +
                        ObjectGraph(FieldValue(unit, "Prefab"), 8)));
            }
            records.Add("special-candidate-summary=found:" + specialCandidates.Length +
                ";indexed:" + specialIndex.Length + ";details:" +
                specialDetails.Length + ";missing-details:" +
                string.Join(",", ExactSpecialMechanicGuids.Where(guid =>
                    !specialDetails.Any(value => value.AssetGuid == guid))));

            var canonical = new HashSet<string>(CanonicalParentGuids, StringComparer.Ordinal);
            BlueprintAbility[] canonicalParents = all.OfType<BlueprintAbility>()
                .Where(value => canonical.Contains(value.AssetGuid)).ToArray();
            foreach (BlueprintAbility parent in canonicalParents)
            {
                AbilityVariants variants = (parent.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AbilityVariants>().SingleOrDefault();
                IEnumerable<BlueprintAbility> children = variants == null
                    ? new[] { parent } : (variants.Variants ?? Array.Empty<BlueprintAbility>());
                foreach (BlueprintAbility child in children.Where(value => value != null))
                {
                    AbilityEffectRunAction effect = (child.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                        .OfType<AbilityEffectRunAction>().SingleOrDefault();
                    records.Add("summon-action=parent:" + Describe(parent) + ";child:" +
                        Describe(child) + ";graph:" + ObjectGraph(effect == null ? null : effect.Actions, 6));
                }
            }
            records.Add("summon-action-summary=parents:" + canonicalParents.Length);

            var templateMechanics = new HashSet<string>(ExactTemplateMechanicGuids,
                StringComparer.Ordinal);
            BlueprintScriptableObject[] observedTemplateMechanics = all.Where(value =>
                templateMechanics.Contains(value.AssetGuid)).OrderBy(value =>
                    value.AssetGuid, StringComparer.Ordinal).ToArray();
            foreach (BlueprintScriptableObject value in observedTemplateMechanics)
                records.Add("template-mechanic=" + Describe(value) + ";fields=" +
                    Members(value, 160) + ";components=" + Components(value) +
                    ";graph=" + ObjectGraph(value.ComponentsArray, 12) +
                    ";resource-amount=" + TemplateResourceAmount(value));
            records.Add("template-mechanic-summary=expected:" +
                ExactTemplateMechanicGuids.Length + ";found:" +
                observedTemplateMechanics.Length + ";missing:" + string.Join(",",
                    ExactTemplateMechanicGuids.Where(guid =>
                        !observedTemplateMechanics.Any(value => value.AssetGuid == guid))));

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
                units.Length, facts.Length, donors.Length, missingDonors.Length,
                specialCandidates.Length, records);
        }

        private static object FieldValue(object value, string name)
        {
            FieldInfo field = AllFields(value.GetType()).SingleOrDefault(item =>
                string.Equals(item.Name, name, StringComparison.Ordinal) ||
                string.Equals(item.Name, "m_" + name, StringComparison.Ordinal));
            return field == null ? null : field.GetValue(value);
        }

        private static string Components(BlueprintScriptableObject value)
        {
            return string.Join("|", (value.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Where(component => component != null).Select(component =>
                    component.GetType().FullName + "{" + Members(component) + "}"));
        }

        private static string TemplateResourceAmount(BlueprintScriptableObject value)
        {
            FieldInfo amount = AllFields(value.GetType()).SingleOrDefault(field =>
                field.Name == "m_MaxAmount");
            return amount == null ? "not-resource" :
                ObjectGraph(amount.GetValue(value), 8);
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

        private static string ObjectGraph(object root, int depth)
        {
            var rows = new List<string>();
            Visit(root, "root", depth, rows, new HashSet<object>(ReferenceComparer.Instance));
            return string.Join("|", rows.Take(300));
        }

        private static void Visit(object value, string path, int depth,
            IList<string> rows, ISet<object> seen)
        {
            if (value == null) { rows.Add(path + "=<null>"); return; }
            BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) { rows.Add(path + "=" + Describe(blueprint)); return; }
            Type type = value.GetType();
            if (type.IsEnum || type.IsPrimitive || value is string || value is decimal)
            { rows.Add(path + "=" + value); return; }
            if (depth <= 0) { rows.Add(path + "=<" + type.FullName + ">"); return; }
            if (!type.IsValueType && !seen.Add(value)) { rows.Add(path + "=<cycle>"); return; }
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null && !(value is string))
            {
                int index = 0;
                foreach (object item in sequence) {
                    Visit(item, path + "[" + index + "]", depth - 1, rows, seen);
                    if (++index >= 50) break;
                }
                return;
            }
            foreach (FieldInfo field in AllFields(type).OrderBy(v =>
                v.DeclaringType.FullName + "." + v.Name, StringComparer.Ordinal))
            {
                if (field.DeclaringType == typeof(UnityEngine.Object)) continue;
                object item; try { item = field.GetValue(value); } catch { continue; }
                Visit(item, path + "." + field.DeclaringType.Name + "." + field.Name,
                    depth - 1, rows, seen);
                if (rows.Count >= 300) return;
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
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
