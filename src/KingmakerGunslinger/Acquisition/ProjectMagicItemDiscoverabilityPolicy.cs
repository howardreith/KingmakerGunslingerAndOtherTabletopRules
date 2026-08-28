using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Acquisition
{
    internal sealed class ProjectMagicItemLocation
    {
        internal ProjectMagicItemLocation(string itemKey, string targetGuid,
            string targetName, string areaName)
        {
            ItemKey = itemKey;
            TargetGuid = targetGuid;
            TargetName = targetName;
            AreaName = areaName;
        }

        internal string ItemKey { get; private set; }
        internal string TargetGuid { get; private set; }
        internal string TargetName { get; private set; }
        internal string AreaName { get; private set; }
    }

    internal sealed class ProjectMagicItemDiscoverabilityAudit
    {
        internal ProjectMagicItemDiscoverabilityAudit(string[] issues,
            IDictionary<string, int> exactAreaDensity,
            IDictionary<string, int> campaignAreaDensity)
        {
            Issues = issues;
            ExactAreaDensity = exactAreaDensity;
            CampaignAreaDensity = campaignAreaDensity;
        }

        internal bool IsAcceptable { get { return Issues.Length == 0; } }
        internal string[] Issues { get; private set; }
        internal IDictionary<string, int> ExactAreaDensity { get; private set; }
        internal IDictionary<string, int> CampaignAreaDensity
        { get; private set; }
    }

    internal static class ProjectMagicItemDiscoverabilityPolicy
    {
        internal const int ExpectedItemCount = 30;
        internal const string CordItemKey =
            "KMG.Items.CordOfStubbornResolve";
        internal const string CordTargetGuid =
            "9572baf3952095f41abda1fb25055cce";
        internal const string CordTargetName =
            "RichHuman_treasure_chest_04 (1)";
        internal const string CordAreaName = "CapitalTavern_Indoor";

        private static readonly string[] TemporaryAreas =
        {
            "CapitalSquareVillage",
            "SilverstepGrotto_FirstWorld",
            "RushlightFestivalCamp",
            "PitaxHorde",
            "IrovettiPalaceFW"
        };

        private static readonly string[] ObscureTargetTokens =
        {
            "hidden", "cache", "secret", "puzzle", "quest", "artifact",
            "memento", "diary", "corpse", "trash"
        };

        internal static ProjectMagicItemDiscoverabilityAudit Audit(
            IEnumerable<ProjectMagicItemLocation> observations)
        {
            ProjectMagicItemLocation[] values = observations == null
                ? new ProjectMagicItemLocation[0] : observations.ToArray();
            var issues = new List<string>();
            if (values.Length != ExpectedItemCount)
                issues.Add("item-count=" + values.Length);
            foreach (ProjectMagicItemLocation value in values)
            {
                if (value == null)
                {
                    issues.Add("null-location");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(value.ItemKey))
                    issues.Add("blank-item-key");
                if (!IsGuid(value.TargetGuid))
                    issues.Add(value.ItemKey + ":invalid-guid");
                if (string.IsNullOrWhiteSpace(value.TargetName))
                    issues.Add(value.ItemKey + ":blank-target-name");
                if (string.IsNullOrWhiteSpace(value.AreaName))
                    issues.Add(value.ItemKey + ":blank-area-name");
                if (IsTemporaryArea(value.AreaName))
                    issues.Add(value.ItemKey + ":temporary-area=" +
                        value.AreaName);
                string searchable = (value.TargetName ?? string.Empty)
                    .ToLowerInvariant().Replace("nothidden", string.Empty)
                    .Replace("unhidden", string.Empty);
                string obscure = ObscureTargetTokens.FirstOrDefault(token =>
                    searchable.Contains(token));
                if (obscure != null)
                    issues.Add(value.ItemKey + ":obscure-target=" + obscure);
            }

            AppendDistributionIssues(values, issues);
            AppendCordIssues(values, issues);
            IDictionary<string, int> exact = Density(values, value =>
                value.AreaName);
            IDictionary<string, int> campaign = Density(values, value =>
                NormalizeArea(value.AreaName));
            return new ProjectMagicItemDiscoverabilityAudit(issues.ToArray(),
                exact, campaign);
        }

        private static void AppendDistributionIssues(
            ProjectMagicItemLocation[] values, ICollection<string> issues)
        {
            ProjectMagicItemLocation[] complete = values.Where(value =>
                value != null && !string.IsNullOrWhiteSpace(value.TargetGuid) &&
                !string.IsNullOrWhiteSpace(value.AreaName)).ToArray();
            int targets = complete.Select(value => value.TargetGuid)
                .Distinct(StringComparer.Ordinal).Count();
            if (targets != ExpectedItemCount)
                issues.Add("distinct-targets=" + targets);
            IDictionary<string, int> exact = Density(complete, value =>
                value.AreaName);
            if (exact.Count < 29)
                issues.Add("distinct-exact-areas=" + exact.Count);
            foreach (KeyValuePair<string, int> entry in exact)
                if (entry.Value > 2)
                    issues.Add("exact-area-density=" + entry.Key + ":" +
                        entry.Value);
            IDictionary<string, int> campaign = Density(complete, value =>
                NormalizeArea(value.AreaName));
            foreach (KeyValuePair<string, int> entry in campaign)
            {
                int maximum = string.Equals(entry.Key, "FinalDungeon",
                    StringComparison.Ordinal) ? 3 : 2;
                if (entry.Value > maximum)
                    issues.Add("campaign-area-density=" + entry.Key + ":" +
                        entry.Value);
            }
        }

        private static void AppendCordIssues(
            ProjectMagicItemLocation[] values, ICollection<string> issues)
        {
            ProjectMagicItemLocation[] cord = values.Where(value =>
                value != null && string.Equals(value.ItemKey, CordItemKey,
                    StringComparison.Ordinal)).ToArray();
            if (cord.Length != 1 || !string.Equals(cord[0].TargetGuid,
                    CordTargetGuid, StringComparison.Ordinal) ||
                !string.Equals(cord[0].TargetName, CordTargetName,
                    StringComparison.Ordinal) ||
                !string.Equals(cord[0].AreaName, CordAreaName,
                    StringComparison.Ordinal))
                issues.Add("cord-capital-inn-contract");
        }

        private static IDictionary<string, int> Density(
            IEnumerable<ProjectMagicItemLocation> values,
            Func<ProjectMagicItemLocation, string> key)
        {
            return values.Where(value => value != null &&
                    !string.IsNullOrWhiteSpace(key(value)))
                .GroupBy(key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(),
                    StringComparer.Ordinal);
        }

        private static bool IsGuid(string value)
        {
            return value != null && value.Length == 32 &&
                value.All(Uri.IsHexDigit);
        }

        private static bool IsTemporaryArea(string area)
        {
            return !string.IsNullOrWhiteSpace(area) &&
                (TemporaryAreas.Contains(area, StringComparer.Ordinal) ||
                 area.EndsWith("FW", StringComparison.Ordinal));
        }

        internal static string NormalizeArea(string area)
        {
            if (string.IsNullOrWhiteSpace(area)) return string.Empty;
            if (area.StartsWith("TrollLair", StringComparison.Ordinal))
                return "TrollLair";
            if (area.StartsWith("Silverstep", StringComparison.Ordinal))
                return "Silverstep";
            if (area.StartsWith("Varnhold", StringComparison.Ordinal))
                return "Varnhold";
            if (area.StartsWith("ArmagsTomb", StringComparison.Ordinal))
                return "ArmagsTomb";
            if (area.StartsWith("Vordakai", StringComparison.Ordinal))
                return "VordakaiTomb";
            if (area.StartsWith("IrovettiPalace", StringComparison.Ordinal))
                return "IrovettiPalace";
            if (area.StartsWith("HouseAtTheEdgeOfTime",
                    StringComparison.Ordinal))
                return "HouseAtTheEdgeOfTime";
            if (area.StartsWith("FinalDungeon", StringComparison.Ordinal))
                return "FinalDungeon";
            return area;
        }
    }
}
