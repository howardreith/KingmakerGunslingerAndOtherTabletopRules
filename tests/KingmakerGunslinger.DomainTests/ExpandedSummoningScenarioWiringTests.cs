using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// A scenario is wired across three files, and they must agree. The
    /// PowerShell harness decides which timeout values a request carries, and
    /// the runtime rejects a request whose timeouts are not allowed for that
    /// scenario. When they disagree the failure is expensive and late: the game
    /// launches, the request is refused at acceptance, the process never exits
    /// on its own, and the runtime lease has to be recovered by hand.
    ///
    /// disposable-pteranodon-attached-view hit exactly that. Its harness
    /// metadata declared working-save timeouts while the runtime allowlist did
    /// not include it, so the request was rejected with
    /// scenario-timeouts-not-allowed after a full build, deploy and launch.
    /// These are text checks precisely so they cost nothing and run every time.
    /// </summary>
    internal static class ExpandedSummoningScenarioWiringTests
    {
        private static string Source(string relative)
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        /// <summary>
        /// Every Expanded Summoning scenario whose harness metadata sends
        /// catalog or selection timeouts must be one the runtime accepts them
        /// from.
        ///
        /// Deliberately scoped to this mission's scenario family. A repo-wide
        /// version of this check was tried and withdrawn: the validator admits
        /// scenarios through several branches, including ones keyed on
        /// transaction-owned save inputs rather than the working save, and a
        /// faithful model of that branching is not something a text check can
        /// carry. The broad version reported ten scenarios as broken that have
        /// passing runtime evidence on disk, and a check that cries wolf about
        /// working code is worse than no check. Within the summoning family the
        /// admission rule is simply "named in the validator", which is exactly
        /// what this asserts.
        /// </summary>
        internal static void WorkingSaveScenariosAreAllowedTheirTimeouts()
        {
            string harness = Source("scripts/RuntimeAutomation.Common.ps1");
            string request = Source("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs");
            string catalog = Source("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs");

            // Scenario blocks look like:  'name' = [pscustomobject]@{ ... }
            var blocks = Regex.Matches(harness,
                @"'(?<name>[a-z0-9-]+)'\s*=\s*\[pscustomobject\]@\{(?<body>.*?)\n    \}",
                RegexOptions.Singleline);
            Assertions.True(blocks.Count > 20,
                "The harness scenario table could not be parsed.");

            var offenders = new List<string>();
            int examined = 0;
            foreach (Match block in blocks)
            {
                string name = block.Groups["name"].Value;

                // This mission's family only - see the summary above.
                if (name.IndexOf("summoning", StringComparison.Ordinal) < 0 &&
                    name.IndexOf("pteranodon", StringComparison.Ordinal) < 0)
                    continue;
                examined++;

                string body = block.Groups["body"].Value;
                bool sendsTimeouts =
                    body.IndexOf("UsesCatalogTimeout = $true", StringComparison.Ordinal) >= 0 ||
                    body.IndexOf("UsesSelectionTimeouts = $true", StringComparison.Ordinal) >= 0;
                if (!sendsTimeouts) continue;

                // Resolve the scenario's C# constant name from the catalog, then
                // require the request validator to reference that constant.
                Match constant = Regex.Match(catalog,
                    @"internal const string (?<symbol>\w+)\s*=\s*\r?\n?\s*""" +
                    Regex.Escape(name) + @"""");
                if (!constant.Success)
                {
                    offenders.Add(name + " (no catalog constant)");
                    continue;
                }

                string symbol = constant.Groups["symbol"].Value;

                // The validator wraps long conditions, so a reference can read
                // "RuntimeTestScenarioCatalog\n    .Symbol". Matching a
                // contiguous string misses those and reports working scenarios
                // as broken, which is worse than not checking at all.
                bool named = Regex.IsMatch(request,
                    @"RuntimeTestScenarioCatalog\s*\.\s*" + Regex.Escape(symbol) +
                    @"\b");

                // Or it is admitted through a group predicate that names it.
                bool grouped = false;
                foreach (Match predicate in Regex.Matches(request,
                    @"RuntimeTestScenarioCatalog\s*\.\s*(?<fn>Is\w+)\s*\("))
                {
                    Match predicateBody = Regex.Match(catalog,
                        @"static bool " + predicate.Groups["fn"].Value +
                        @"\([^)]*\)\s*\{(?<members>[\s\S]*?)\n        \}");
                    if (predicateBody.Success && Regex.IsMatch(
                        predicateBody.Groups["members"].Value,
                        @"\b" + Regex.Escape(symbol) + @"\b"))
                    {
                        grouped = true;
                        break;
                    }
                }

                if (!named && !grouped) offenders.Add(name);
            }

            Assertions.True(examined >= 6,
                "The summoning scenario family shrank; this check would pass " +
                "vacuously. Examined only " + examined + ".");
            Assertions.True(offenders.Count == 0,
                "These scenarios send catalog or selection timeouts the runtime will " +
                "reject, so they would fail only after a full launch: " +
                string.Join(", ", offenders.ToArray()));
        }

        /// <summary>
        /// Every Expanded Summoning scenario that requires the working save must
        /// be named in the runner's working-save chains.
        ///
        /// The runner decides more than once whether a scenario needs a save
        /// loaded - once to keep it from completing at mod load, once at the
        /// readiness gate, and once in the save-load predicate - and each is a
        /// long boolean chain of explicit scenario names. A scenario missing
        /// from them is accepted, launched, and then sits in OnUpdate forever
        /// with no save and no dispatch, until the harness times out and leaves
        /// a running game and a retained lease to recover by hand.
        ///
        /// That is exactly what disposable-expanded-summoning-projected-menu did
        /// on its first run. The harness metadata, the catalog and the request
        /// validator all agreed; the runner's chains did not, and nothing
        /// checked them.
        /// </summary>
        internal static void WorkingSaveScenariosAreNamedInTheRunnerChains()
        {
            string harness = Source("scripts/RuntimeAutomation.Common.ps1");
            string runner = Source(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            string catalog = Source(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs");

            var offenders = new List<string>();
            int examined = 0;
            foreach (Match block in Regex.Matches(harness,
                @"'(?<name>[a-z0-9-]+)'\s*=\s*\[pscustomobject\]@\{(?<body>.*?)
    \}",
                RegexOptions.Singleline))
            {
                string name = block.Groups["name"].Value;
                if (name.IndexOf("expanded-summoning", StringComparison.Ordinal) < 0)
                    continue;
                string body = block.Groups["body"].Value;
                if (body.IndexOf("RequiresSaveName = $true", StringComparison.Ordinal) < 0)
                    continue;
                // Supervised scenarios reach the save by a human-driven path and
                // are deliberately not in these chains.
                if (body.IndexOf("RequiresManualInteraction = $true",
                    StringComparison.Ordinal) >= 0) continue;

                Match constant = Regex.Match(catalog,
                    @"internal const string (?<symbol>\w+)\s*=\s*?
?\s*""" +
                    Regex.Escape(name) + @"""");
                if (!constant.Success)
                {
                    offenders.Add(name + " (no catalog constant)");
                    continue;
                }

                string symbol = constant.Groups["symbol"].Value;
                // A scenario admitted through a group predicate is named once
                // inside that predicate and then reached by calls to it. That is
                // legitimate wiring, so the predicate stands in for the chains.
                // The predicate may live in either file.
                string predicate =
                    @"bool Is\w+\([^)]*\)\s*\{[\s\S]{0,2000}?" +
                    Regex.Escape(symbol) + @"\b";
                if (Regex.IsMatch(catalog, predicate) ||
                    Regex.IsMatch(runner, predicate))
                    continue;

                examined++;
                int mentions = Regex.Matches(runner,
                    @"RuntimeTestScenarioCatalog\s*\.\s*" + Regex.Escape(symbol) +
                    @"\b").Count;
                // Three chains name such a scenario - the mod-load exclusion,
                // the readiness gate and the save-load predicate - plus its
                // dispatch. Fewer than four mentions means one was missed, and
                // that only shows up after a launch.
                if (mentions < 4)
                    offenders.Add(name + " (named " + mentions + " times)");
            }

            Assertions.True(examined >= 3,
                "The save-backed summoning scenario family shrank; this check " +
                "would pass vacuously. Examined only " + examined + ".");
            Assertions.True(offenders.Count == 0,
                "These working-save scenarios are not named in enough of the " +
                "runner's chains, so they would launch and then sit idle until " +
                "the harness timed out: " + string.Join(", ", offenders.ToArray()));
        }

        /// <summary>
        /// The Pteranodon attached-view capture rides along with the proven
        /// disposable-expanded-summoning lifecycle.
        ///
        /// A standalone scenario for this was written and withdrawn after it
        /// died twice inside EntityDestructionController during its own
        /// cleanup: reproducing the spawn and teardown lifecycle correctly is
        /// fiddly, and the reimplementation kept diverging from the shipped
        /// one. Reusing a lifecycle that already passes beat maintaining a
        /// second copy of it. This test pins that decision so the capture
        /// cannot quietly lose its home.
        /// </summary>
        internal static void PteranodonCaptureRidesTheProvenScenario()
        {
            string runner = Source("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            string helper = Source(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.PteranodonAttachedView.cs");
            string catalog = Source("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs");
            string harness = Source("scripts/RuntimeAutomation.Common.ps1");

            Assertions.True(helper.Contains("DescribeAttachedPteranodonView"),
                "The attached-view capture helper is missing.");
            Assertions.True(runner.Contains("_pteranodonAttachedContract ="),
                "The proven scenario no longer captures the Pteranodon contract.");
            Assertions.True(runner.Contains(
                    "expanded-summoning-pteranodon-attached-contract"),
                "The captured contract is not asserted anywhere.");
            Assertions.True(helper.Contains("pteranodon-attached-rig.json"),
                "The rig dump is no longer written to the run's evidence directory.");

            // The withdrawn scenario must be gone from every wiring point, not
            // left half-registered where it could be invoked and fail.
            foreach (string source in new[] { runner, catalog, harness })
            {
                Assertions.False(source.Contains("disposable-pteranodon-attached-view"),
                    "The withdrawn standalone scenario is still wired in.");
                Assertions.False(source.Contains("DisposablePteranodonAttachedView"),
                    "The withdrawn standalone scenario constant is still referenced.");
            }
        }
    }
}
