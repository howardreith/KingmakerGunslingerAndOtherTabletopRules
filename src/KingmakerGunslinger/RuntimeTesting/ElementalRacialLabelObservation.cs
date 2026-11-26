using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UI._ConsoleUI.CharacterInfo;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Observes the racial-bonus label the native UI code produces for each
    /// ability score, next to the actual stat values and every Racial modifier.
    /// Labels come from the real (possibly patched) native methods:
    /// CharSScoreEntry.SetData on a request-local inactive entry and the console
    /// CharInfoStatVM. The expected label is derived from the expected final
    /// racial modifier only (0 means no racial bonus, rendered "-" / null).
    /// </summary>
    internal static class ElementalRacialLabelObservation
    {
        internal static readonly StatType[] AbilityStats = { StatType.Strength, StatType.Dexterity,
            StatType.Constitution, StatType.Intelligence, StatType.Wisdom, StatType.Charisma };

        internal static string ExpectedLabel(int racial)
        {
            return racial == 0 ? "-" : racial > 0 ? "+" + racial : racial.ToString();
        }

        /// <summary>Native unpatched label rule: the first enumerated Racial modifier.</summary>
        internal static string FirstRacialModifierLabel(ModifiableValue stat)
        {
            var first = stat.Modifiers.FirstOrDefault(value => value.ModDescriptor == ModifierDescriptor.Racial);
            if (first == null) return "-";
            return first.ModValue >= 0 ? "+" + first.ModValue : first.ModValue.ToString();
        }

        internal static JArray RacialModifiers(ModifiableValue stat)
        {
            return new JArray(stat.Modifiers.Where(value => value.ModDescriptor == ModifierDescriptor.Racial)
                .Select(value => new JObject {
                    ["value"] = value.ModValue, ["descriptor"] = value.ModDescriptor.ToString(),
                    ["stackMode"] = value.StackMode.ToString(), ["stacks"] = value.Stacks,
                    ["sourceGuid"] = value.Source?.Blueprint?.AssetGuid,
                    ["sourceName"] = value.Source?.Blueprint?.name,
                    ["sourceActive"] = value.Source == null ? (JToken)JValue.CreateNull() : value.Source.Active,
                    ["sourceComponent"] = value.SourceComponent }));
        }

        /// <summary>
        /// Observes all six abilities. <paramref name="expectedRacial"/> gives the
        /// expected final racial contribution per ability in AbilityStats order;
        /// <paramref name="expectedLabels"/> overrides the expected label (used for
        /// unaffected native controls whose native rule must be preserved).
        /// </summary>
        internal static JObject Observe(UnitDescriptor owner, int[] expectedRacial,
            string[] expectedLabels, out bool exact)
        {
            if (owner == null) throw new ArgumentNullException("owner");
            if (expectedRacial == null || expectedRacial.Length != AbilityStats.Length)
                throw new ArgumentException("Six expected racial values are required.", "expectedRacial");
            var abilities = new JArray();
            bool all = true;
            for (int index = 0; index < AbilityStats.Length; index++)
            {
                var stat = owner.Stats.GetStat<ModifiableValueAttributeStat>(AbilityStats[index]);
                string expectedLabel = expectedLabels?[index] ?? ExpectedLabel(expectedRacial[index]);
                int? expectedConsole = expectedLabels != null ? ConsoleValue(expectedLabel) :
                    expectedRacial[index] == 0 ? (int?)null : expectedRacial[index];
                string pcLabel = RenderPcLabel(owner, stat);
                int? consoleValue = RenderConsoleValue(stat);
                // Attribute PermanentValue applies only Racial and item Enhancement
                // modifiers; the fixtures own no items, so this is the racial total.
                int racialOverlay = stat.PermanentValue - stat.BaseValue;
                bool abilityExact = racialOverlay == expectedRacial[index] &&
                    string.Equals(pcLabel, expectedLabel, StringComparison.Ordinal) &&
                    consoleValue == expectedConsole;
                all &= abilityExact;
                abilities.Add(new JObject {
                    ["stat"] = AbilityStats[index].ToString(),
                    ["baseValue"] = stat.BaseValue, ["permanentValue"] = stat.PermanentValue,
                    ["modifiedValue"] = stat.ModifiedValue,
                    ["expectedRacial"] = expectedRacial[index], ["permanentMinusBase"] = racialOverlay,
                    ["racialModifiers"] = RacialModifiers(stat),
                    ["nativeFirstRacialLabel"] = FirstRacialModifierLabel(stat),
                    ["renderedPcLabel"] = pcLabel, ["expectedPcLabel"] = expectedLabel,
                    ["renderedConsoleRaceBonus"] = consoleValue.HasValue ? (JToken)consoleValue.Value : JValue.CreateNull(),
                    ["expectedConsoleRaceBonus"] = expectedConsole.HasValue ? (JToken)expectedConsole.Value : JValue.CreateNull(),
                    ["exact"] = abilityExact });
            }
            exact = all;
            return new JObject { ["race"] = owner.Progression.Race?.AssetGuid, ["abilities"] = abilities, ["exact"] = all };
        }

        internal static string Compact(JObject observation)
        {
            return string.Join(",", observation["abilities"].Select(value =>
                ((string)value["stat"]).Substring(0, 3) + "=" + (string)value["renderedPcLabel"] + "/" +
                ((JToken)value["renderedConsoleRaceBonus"]).ToString(Newtonsoft.Json.Formatting.None) +
                "(" + (int)value["permanentMinusBase"] + ")").ToArray());
        }

        private static int? ConsoleValue(string label)
        {
            if (label == "-") return null;
            return int.Parse(label.TrimStart('+'));
        }

        private static string RenderPcLabel(UnitDescriptor owner, ModifiableValueAttributeStat stat)
        {
            // Inactive request-local entry: SetData runs the real native method and
            // any installed postfix, with no tooltip trigger and no layout/canvas.
            var root = new GameObject("KMG_RuntimeRacialLabelProbe");
            root.SetActive(false);
            try
            {
                var entry = root.AddComponent<CharSScoreEntry>();
                entry.Type = stat.Type;
                entry.ShortName = Text(root, "ShortName");
                entry.LongName = Text(root, "LongName");
                entry.Value = Text(root, "Value");
                entry.RaceBonus = Text(root, "RaceBonus");
                entry.Modifier = Text(root, "Modifier");
                var background = new GameObject("Background", typeof(RectTransform));
                background.transform.SetParent(root.transform, false);
                entry.Background = background.AddComponent<Image>();
                entry.SetData(owner, new Color32(255, 255, 255, 255), true);
                if (!ReferenceEquals(entry.Stat, stat))
                    throw new InvalidOperationException("Native score entry bound a different stat reference.");
                return entry.RaceBonus.text;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static TextMeshProUGUI Text(GameObject root, string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(root.transform, false);
            return child.AddComponent<TextMeshProUGUI>();
        }

        private static int? RenderConsoleValue(ModifiableValueAttributeStat stat)
        {
            var model = new CharInfoStatVM(stat, true);
            try
            {
                if (!ReferenceEquals(model.Stat.Value, stat))
                    throw new InvalidOperationException("Native console stat model bound a different stat reference.");
                return model.RaceBonus.Value;
            }
            finally
            {
                model.Dispose();
            }
        }
    }
}
