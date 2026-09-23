using System;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Drives the development-only projected-menu measurement.
    ///
    /// The fixture needs several frames per cycle - a toggle, the layout
    /// settling, a snapshot, a hide - so this is a small state machine rather
    /// than a single call: it returns without completing until all six cycles
    /// have been measured, and the runner calls it again on the next update.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningProjectedMenuFixture _projectedMenu;
        private int _projectedMenuSettled;
        private int _projectedMenuCycle;
        private int _projectedMenuFamily;
        private int _projectedMenuStarved;

        /// <summary>
        /// A hard budget on the whole measurement, not only on frames where
        /// nothing has settled.
        ///
        /// The first version counted only un-settled frames, so a cycle that
        /// began and then lost its widget froze the counter and the scenario
        /// looped until the harness timed out - producing no result and no
        /// diagnostic at all. A total budget cannot be defeated that way.
        /// </summary>
        private const int ProjectedMenuBudgetFrames = 5400;

        private void RunExpandedSummoningProjectedMenu()
        {
            if (_projectedMenu == null)
            {
                UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                    value != null && value.Descriptor != null).ToArray();
                if (party.Length == 0) throw new InvalidOperationException(
                    "The loaded working save has no party member to build " +
                    "projected menu entries against.");
                _projectedMenu = new ExpandedSummoningProjectedMenuFixture(party[0],
                    _request.EvidenceDirectory);
                _trace.Record("scenario-activated", RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningProjectedMenu);
            }

            if (++_projectedMenuStarved > ProjectedMenuBudgetFrames)
            {
                _projectedMenu.RestoreActionBar();
                throw new InvalidOperationException(
                    "The projected menu did not complete within " +
                    ProjectedMenuBudgetFrames + " frames. Setup: " +
                    _projectedMenu.SetupReason + ". Slots: " +
                    _projectedMenu.Availability + ". Progress: family=" +
                    _projectedMenuFamily + ";cycle=" + _projectedMenuCycle +
                    ";settled=" + _projectedMenuSettled + ";measured=" +
                    _projectedMenu.Measurements.Count + ". The layout anchors " +
                    "the popup to the slot a player clicked, so an absent or " +
                    "vanishing group slot leaves nothing to measure against.");
            }

            SummonFamily family = _projectedMenuFamily == 0
                ? SummonFamily.Monster : SummonFamily.NaturesAlly;
            if (!_projectedMenu.Step(family, _projectedMenuCycle + 1,
                ref _projectedMenuSettled))
                return;

            // Deliberately not reset: the budget covers the whole measurement,
            // and resetting it on progress is what let the previous version
            // loop indefinitely.
            _projectedMenuCycle++;
            if (_projectedMenuCycle < ExpandedSummoningProjectedMenuFixture.Cycles)
                return;
            _projectedMenuCycle = 0;
            _projectedMenuFamily++;
            if (_projectedMenuFamily < 2) return;

            // The action bar goes back before anything is reported, whatever
            // the outcome. A fixture that leaves the bar rearranged has changed
            // the thing it was measuring.
            _projectedMenu.RestoreActionBar();

            string summary;
            bool passes = _projectedMenu.Passes(out summary);
            summary = "setup=" + _projectedMenu.SetupReason + " | " + summary;
            var assertions = new System.Collections.Generic.List<RuntimeTestAssertion>
            {
                Assertion("expanded-summoning-projected-menu",
                    ExpandedSummoningProjectedMenuFixture.Projected(
                        SummonFamily.Monster) + " Summon Monster and " +
                    ExpandedSummoningProjectedMenuFixture.Projected(
                        SummonFamily.NaturesAlly) + " Nature's Ally entries " +
                    "render; every entry reachable; popup inside the safe area; " +
                    "scrolling exact; open under 250ms; no slot growth across " +
                    "three cycles",
                    summary, passes,
                    "development-only projected list driven through " +
                    "ActionBarSpellsGroup.Toggle; rubric in " +
                    "docs/EXPANDED-SUMMONING-PROJECTED-MENU-RUBRIC.md"),
                // Nothing may be published to measure a projection. The entries
                // are transient AbilityData built from blueprints that are
                // already published, and the live surface must be untouched
                // afterwards.
                Assertion("expanded-summoning-projected-menu-publishes-nothing",
                    "the live publication surface is unchanged by the measurement",
                    ObserveExpandedSummoningBoundary(true).Describe(),
                    ObserveExpandedSummoningBoundary(true).Describe() ==
                        ExpandedSummoningBoundaryExpectation(true,
                            ObserveExpandedSummoningBoundary(true).NativeVariants),
                    "AbilityVariants census across the eighteen canonical parents " +
                    "after the projected menu ran"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };

            RuntimeTestResult result = CreateResult(assertions.All(value =>
                value.Status == "PASS") ? "PASS" : "FAIL", assertions, null);
            foreach (ExpandedSummoningProjectedMenuFixture.Measurement measurement
                in _projectedMenu.Measurements)
                result.Diagnostics.Add(measurement.ToString());
            Complete(result);
        }
    }
}
