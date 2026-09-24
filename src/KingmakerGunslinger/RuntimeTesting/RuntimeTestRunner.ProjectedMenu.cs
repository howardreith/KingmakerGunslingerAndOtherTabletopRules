using System;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using UnityEngine;

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
        private ExpandedSummoningProjectedMenuSubject _projectedMenuSubject;
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
            if (_projectedMenuSubject == null)
            {
                UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                    value != null && value.Descriptor != null).ToArray();
                if (party.Length == 0) throw new InvalidOperationException(
                    "The loaded working save has no party member to build " +
                    "projected menu entries against.");
                _projectedMenuSubject = new ExpandedSummoningProjectedMenuSubject(
                    party, CreateProjectedMenuCaster);
                _trace.Record("scenario-activated", RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningProjectedMenu);
            }

            if (++_projectedMenuStarved > ProjectedMenuBudgetFrames)
            {
                if (_projectedMenu != null) _projectedMenu.RestoreActionBar();
                _projectedMenuSubject.Restore();
                throw new InvalidOperationException(
                    "The projected menu did not complete within " +
                    ProjectedMenuBudgetFrames + " frames. Subject: " +
                    _projectedMenuSubject.Describe() + ". Setup: " +
                    (_projectedMenu == null ? "<no fixture>" :
                        _projectedMenu.SetupReason) + ". Slots: " +
                    (_projectedMenu == null ? "<no fixture>" :
                        _projectedMenu.Availability) + ". Progress: family=" +
                    _projectedMenuFamily + ";cycle=" + _projectedMenuCycle +
                    ";settled=" + _projectedMenuSettled + ";measured=" +
                    (_projectedMenu == null ? 0 :
                        _projectedMenu.Measurements.Count) + ". The layout " +
                    "anchors the popup to the slot a player clicked, so an " +
                    "absent or vanishing group slot leaves nothing to measure " +
                    "against.");
            }

            // The subject comes first: a unit whose action bar the game itself
            // built with group slots, selected through the game's own selection
            // path. Until one is found there is nothing to anchor a popup to.
            if (_projectedMenu == null)
            {
                if (!_projectedMenuSubject.Step()) return;
                _projectedMenu = new ExpandedSummoningProjectedMenuFixture(
                    _projectedMenuSubject.Unit, _request.EvidenceDirectory);
                _trace.Record("expanded-summoning-projected-menu-subject",
                    _projectedMenuSubject.Describe());
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
            _projectedMenuSubject.Restore();

            string summary;
            bool passes = _projectedMenu.Passes(out summary);
            summary = "subject=" + _projectedMenuSubject.Describe() + " | setup=" +
                _projectedMenu.SetupReason + " | " + summary;
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
            result.Diagnostics.Add("subject=" + _projectedMenuSubject.Describe());
            Complete(result);
        }

        /// <summary>
        /// A disposable spontaneous caster for the measurement, used only when
        /// no party member's action bar carries a group slot: the working
        /// save's party is not guaranteed to hold a caster. Built the way the
        /// player-path scenario builds its caster, as a Sorcerer with every
        /// canonical Summon Monster and Nature's Ally parent known, so the
        /// game's own action bar gives it spell-level groups and the parents'
        /// variant groups. It is never saved and is disposed by the subject.
        /// </summary>
        private UnitEntityData CreateProjectedMenuCaster()
        {
            UnitEntityData anchor = Game.Instance.Player.Party.FirstOrDefault(
                value => value != null && value.HoldingState != null);
            if (anchor == null) throw new InvalidOperationException(
                "The working save has no party member in an area state to " +
                "place a disposable caster beside.");
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_ExpandedSummoning_ProjectedMenuCaster";
            blueprint.IsCheater = true;
            UnitEntityData caster = Game.Instance.EntityCreator.SpawnUnit(
                blueprint, anchor.Position, Quaternion.identity,
                anchor.HoldingState);
            Game.Instance.EntityCreator.Tick();
            if (caster == null || !caster.IsInState || caster.View == null)
                throw new InvalidOperationException(
                    "The projected-menu caster did not enter the live area.");
            caster.Descriptor.Stats.HitPoints.BaseValue = 10000;
            caster.Descriptor.Stats.Charisma.BaseValue = 30;
            BlueprintCharacterClass sorcerer = BlueprintLibraryLookup
                .RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf",
                    "native Sorcerer projected-menu spellbook");
            object controller = null;
            try
            {
                AdvanceDisposableSpellcaster(caster.Descriptor, sorcerer, 20,
                    ref controller);
            }
            finally
            {
                if (controller != null)
                    controller.GetType().GetMethod("Cancel", BindingFlags.Public |
                        BindingFlags.Instance).Invoke(controller, null);
            }
            Spellbook spellbook = caster.Descriptor.GetSpellbook(sorcerer);
            if (spellbook == null) throw new InvalidOperationException(
                "The disposable caster has no Sorcerer spellbook.");
            int sorcererLevel = caster.Descriptor.Progression.GetClassLevel(sorcerer);
            while (spellbook.CasterLevel < sorcererLevel)
                spellbook.AddCasterLevel();
            string[] guids = ExpandedSummoningInventoryObserver.CanonicalParentGuids;
            for (int index = 0; index < guids.Length; index++)
            {
                BlueprintAbility parent = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        guids[index], (index < 9 ? "Summon Monster " :
                            "Summon Nature's Ally ") + (index % 9 + 1));
                spellbook.AddKnown(index % 9 + 1, parent, true);
            }
            spellbook.UpdateAllSlotsSize(false);
            spellbook.Rest();
            return caster;
        }
    }
}
