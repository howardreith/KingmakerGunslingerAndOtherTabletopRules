using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The compatibility matrix is declared in one file and executed by
    /// another, and they must agree before a launch rather than after one.
    ///
    /// A profile names the scenarios it qualifies with; the launcher carries a
    /// ValidateSet of the scenarios it will accept and a separate list of the
    /// ones it hands the working-save name to. A scenario present in the first
    /// and missing from either of the others fails late - at parameter binding,
    /// or at request acceptance inside a game that has already been launched
    /// with optional mods staged into the live install. That is the expensive
    /// place to find out. These are text checks so they cost nothing and run
    /// every build.
    /// </summary>
    internal static class ExpandedSummoningCompatibilityMatrixTests
    {
        private static readonly string[] ExpandedSummoningProfiles = {
            "gunslinger-only",
            "gunslinger-call-of-the-wild",
            "gunslinger-arms-armor",
            "gunslinger-toggle-custom-soundpacks",
            "gunslinger-high-risk-combined"
        };

        private const string MechanicalScenario = "disposable-expanded-summoning";

        private const string LauncherPath =
            "scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1";

        private static string Source(string relative)
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        /// <summary>One match per profile, carrying its id and its body.</summary>
        private static MatchCollection ProfileBlocks()
        {
            return Regex.Matches(Source("compatibility/profiles.json"),
                "\"id\": \"(?<id>[a-z0-9-]+)\"(?<body>.*?)\"disposition\"",
                RegexOptions.Singleline);
        }

        /// <summary>Scenario names each profile declares, in file order.</summary>
        private static Dictionary<string, string[]> ProfileScenarios()
        {
            var result = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (Match profile in ProfileBlocks())
            {
                Match scenarios = Regex.Match(profile.Groups["body"].Value,
                    "\"scenarios\": \\[(?<list>[^\\]]*)\\]", RegexOptions.Singleline);
                result[profile.Groups["id"].Value] = !scenarios.Success
                    ? new string[0]
                    : Regex.Matches(scenarios.Groups["list"].Value, "\"([a-z0-9-]+)\"")
                        .Cast<Match>().Select(value => value.Groups[1].Value).ToArray();
            }

            return result;
        }

        /// <summary>The launcher's -Scenario ValidateSet.</summary>
        private static HashSet<string> LaunchableScenarios()
        {
            Match set = Regex.Match(Source(LauncherPath),
                "\\[ValidateSet\\((?<body>[^)]*)\\)\\]\\s*\\r?\\n\\s*" +
                "\\[string\\[\\]\\]\\$Scenario", RegexOptions.Singleline);
            Assertions.True(set.Success,
                "The compatibility launcher's -Scenario ValidateSet could not be parsed.");
            return new HashSet<string>(
                Regex.Matches(set.Groups["body"].Value, "'([a-z0-9-]+)'")
                    .Cast<Match>().Select(value => value.Groups[1].Value),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// No profile may declare a scenario the launcher would refuse to bind.
        ///
        /// Repository-wide on purpose. The runtime timeout allowlist admits
        /// scenarios through several branches and resisted a repository-wide
        /// check, but this rule has exactly one branch - the name is in the
        /// ValidateSet or it is not - so there is no false-positive surface.
        /// </summary>
        internal static void EveryDeclaredProfileScenarioIsLaunchable()
        {
            HashSet<string> launchable = LaunchableScenarios();
            var offenders = new List<string>();
            int examined = 0;
            foreach (KeyValuePair<string, string[]> profile in ProfileScenarios())
            {
                foreach (string scenario in profile.Value)
                {
                    examined++;
                    if (!launchable.Contains(scenario))
                        offenders.Add(profile.Key + " -> " + scenario);
                }
            }

            Assertions.True(examined >= 60,
                "The compatibility profile table shrank; this check would pass " +
                "vacuously. Examined only " + examined + " declarations.");
            Assertions.True(offenders.Count == 0,
                "These profiles declare scenarios the launcher would reject at " +
                "parameter binding, after staging mods into the live install: " +
                string.Join(", ", offenders.ToArray()));
        }

        /// <summary>
        /// Every Expanded Summoning compatibility profile observes both the
        /// structural surface and the menu surface under its mod set.
        ///
        /// The structural census alone is not enough. Call of the Wild rewrites
        /// summon spells, so the interesting question under that profile is what
        /// a player is offered once both mods have published into the same
        /// parents, and the offered list is a different surface from the
        /// blueprint inventory.
        /// </summary>
        internal static void EveryProfileObservesBothSummoningSurfaces()
        {
            Dictionary<string, string[]> profiles = ProfileScenarios();
            var offenders = new List<string>();
            foreach (string id in ExpandedSummoningProfiles)
            {
                if (!profiles.ContainsKey(id))
                {
                    offenders.Add(id + " (absent)");
                    continue;
                }

                foreach (string required in new[] {
                    "observe-expanded-summoning-inventory",
                    "observe-expanded-summoning-variant-menu" })
                {
                    if (!profiles[id].Contains(required))
                        offenders.Add(id + " -> " + required);
                }
            }

            Assertions.True(offenders.Count == 0,
                "These Expanded Summoning compatibility profiles do not observe " +
                "both summoning surfaces: " + string.Join(", ", offenders.ToArray()));
        }

        /// <summary>
        /// The unattended matrix driver must never schedule a supervised
        /// scenario.
        ///
        /// This rule exists because the first run of that matrix scheduled
        /// `observe-expanded-summoning-variant-menu`, which waits for a human to
        /// open a menu. Unattended it is refused at the save-name check, and the
        /// compatibility launcher stops a profile's list at the first scenario
        /// that does not record PASS - so every profile aborted before its
        /// mechanical scenario could run, and the matrix had to be re-run.
        ///
        /// The check reads the driver for any token that names a scenario the
        /// harness knows, so a supervised name reintroduced anywhere in it -
        /// in a step list, a helper variable, a default - is caught.
        /// </summary>
        internal static void AutomatedMatrixNeverSchedulesSupervisedScenarios()
        {
            string driver = Source(
                "scripts/Invoke-ExpandedSummoningCompatibilityMatrix.ps1");
            string harness = Source("scripts/RuntimeAutomation.Common.ps1");

            var supervised = new HashSet<string>(
                Regex.Matches(harness,
                    "'(?<name>[a-z0-9-]+)'\\s*=\\s*\\[pscustomobject\\]@\\{" +
                    "(?<body>[\\s\\S]*?)\\r?\\n    \\}")
                    .Cast<Match>()
                    .Where(value => Regex.IsMatch(value.Groups["body"].Value,
                        "RequiresManualInteraction\\s*=\\s*\\$true"))
                    .Select(value => value.Groups["name"].Value),
                StringComparer.Ordinal);
            Assertions.True(supervised.Count > 0,
                "No supervised scenario could be found in the harness, so this " +
                "check would pass vacuously.");

            var scheduled = new HashSet<string>(
                Regex.Matches(driver, "'(?<name>[a-z0-9-]+)'")
                    .Cast<Match>().Select(value => value.Groups["name"].Value),
                StringComparer.Ordinal);
            var offenders = scheduled.Where(supervised.Contains)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

            Assertions.True(
                scheduled.Contains("observe-expanded-summoning-inventory"),
                "The driver no longer schedules the structural inventory, so " +
                "this check is looking at the wrong file.");
            Assertions.True(offenders.Length == 0,
                "The unattended compatibility matrix names supervised " +
                "scenarios, which wait for a human and abort the rest of their " +
                "profile: " + string.Join(", ", offenders));
        }

        /// <summary>
        /// The mechanical scenario loads the working save, so it may appear only
        /// in a profile cleared for save-backed runs, and the launcher must hand
        /// it the save name.
        ///
        /// The second half is the discriminating half. Without the save name the
        /// runtime rejects the request at acceptance - after the profile has
        /// been entered and the game launched - and leaves a process that never
        /// exits on its own and a lease that has to be recovered by hand. That
        /// exact failure cost a full build, deploy and manual recovery once
        /// already.
        /// </summary>
        internal static void MechanicalScenarioRunsOnlyWhereSavesAreAllowed()
        {
            Dictionary<string, string[]> profiles = ProfileScenarios();
            var offenders = new List<string>();
            int permitted = 0, refused = 0;
            foreach (Match profile in ProfileBlocks())
            {
                string id = profile.Groups["id"].Value;
                if (!profiles.ContainsKey(id)) continue;
                bool saves = Regex.IsMatch(profile.Groups["body"].Value,
                    "\"workingSaveSmokePermitted\": true");
                bool declared = profiles[id].Contains(MechanicalScenario);
                if (declared && saves) permitted++;
                if (!declared && !saves) refused++;
                if (declared && !saves) offenders.Add(id);
            }

            Assertions.True(permitted >= 2,
                "No save-backed profile runs the mechanical summoning scenario, " +
                "so this rule proves nothing. Permitted: " + permitted + ".");
            Assertions.True(refused >= 5,
                "Every profile permits save-backed runs, so the restriction is " +
                "not discriminating. Refused: " + refused + ".");
            Assertions.True(offenders.Count == 0,
                "These profiles declare the working-save summoning scenario " +
                "without permission to load a save: " +
                string.Join(", ", offenders.ToArray()));

            // The launcher's save-name list is a separate literal from the
            // ValidateSet, and only the first one makes the request valid.
            Match saveList = Regex.Match(Source(LauncherPath),
                "\\$name -in @\\((?<body>[^)]*)\\)\\)\\s*\\{\\s*\\r?\\n\\s*" +
                "\\$arguments\\.SaveName = 'KMG_AUTOMATION_WORKING'",
                RegexOptions.Singleline);
            Assertions.True(saveList.Success,
                "The compatibility launcher's working-save scenario list could " +
                "not be parsed.");
            Assertions.True(
                saveList.Groups["body"].Value.Contains("'" + MechanicalScenario + "'"),
                "The launcher never hands " + MechanicalScenario + " the working " +
                "save name, so the runtime would refuse the request after launch.");
        }
    }
}
