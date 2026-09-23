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
        /// Frames allowed with no action-bar spell group present before giving
        /// up. Without a bound it would hang until the harness timeout, which is
        /// a far worse way to learn the same thing.
        /// </summary>
        private const int ProjectedMenuStarvationFrames = 1800;

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

            SummonFamily family = _projectedMenuFamily == 0
                ? SummonFamily.Monster : SummonFamily.NaturesAlly;
            if (!_projectedMenu.Step(family, _projectedMenuCycle + 1,
                ref _projectedMenuSettled))
            {
                // Step returns false both while a cycle is still settling and
                // when the widget it needs is absent; only the second can go on
                // forever, and it is distinguished by never having settled.
                if (_projectedMenuSettled == 0 &&
                    ++_projectedMenuStarved > ProjectedMenuStarvationFrames)
                    throw new InvalidOperationException(
                        "No action-bar group slot was available to anchor the " +
                        "menu after " + ProjectedMenuStarvationFrames +
                        " frames: " + _projectedMenu.Availability + ". The " +
                        "layout anchors the popup to the slot a player clicked, " +
                        "so without an active group slot there is nothing to " +
                        "measure against. Setup: " + _projectedMenu.SetupReason);
                return;
            }

            _projectedMenuStarved = 0;
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
