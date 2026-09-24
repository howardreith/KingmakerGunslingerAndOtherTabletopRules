using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Selection;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Finds the unit whose action bar the projected-menu measurement anchors
    /// to, through the game's own selection path.
    ///
    /// The layout under measurement anchors its popup to the group slot a
    /// player clicked, so a measurement needs a selected unit whose action bar
    /// the game itself built with group slots. The earlier fixture tried to
    /// install a spontaneous-spell slot into a party member's settings by hand
    /// and mark them dirty; the UI rebuild that followed stopped producing
    /// frames. Nothing here writes a slot. Each candidate is selected exactly
    /// as a portrait click selects it, the layout is given a few frames, and
    /// the active group slots are counted.
    ///
    /// Candidates, in order: every party member of the loaded working save;
    /// then, only if none of them carries a group slot, a disposable
    /// spontaneous caster the runner builds, selected on its own and - if the
    /// action bar ignores a non-party unit - attached to the party for the
    /// measurement and detached and disposed afterwards. The path taken is
    /// recorded with each candidate's result, so the evidence says which unit
    /// the menu opened on and why.
    /// </summary>
    internal sealed class ExpandedSummoningProjectedMenuSubject
    {
        /// <summary>Frames the action bar is given after a selection change.</summary>
        private const int SettleFrames = 6;

        private readonly List<UnitEntityData> _party;
        private readonly Func<UnitEntityData> _createDisposableCaster;
        private readonly List<string> _attempts = new List<string>();
        private readonly UnitEntityData _originalSelection;
        private int _index = -1;
        private int _settle;
        private UnitEntityData _current;
        private UnitEntityData _disposable;
        private bool _disposableAttached;
        private bool _restored;

        internal ExpandedSummoningProjectedMenuSubject(
            IEnumerable<UnitEntityData> party,
            Func<UnitEntityData> createDisposableCaster)
        {
            _party = (party ?? Enumerable.Empty<UnitEntityData>())
                .Where(value => value != null).ToList();
            _createDisposableCaster = createDisposableCaster;
            SelectionManager selection = SelectionManager.Instance;
            _originalSelection = selection == null ? null :
                selection.GetSingleSelectedUnit();
        }

        /// <summary>The unit the menu is measured on; null until found.</summary>
        internal UnitEntityData Unit { get; private set; }

        internal string Path { get; private set; }

        /// <summary>
        /// One step per frame. True once a subject with an active group slot
        /// is selected; false while a candidate settles. Throws, with every
        /// attempt listed, when no candidate yields a slot.
        /// </summary>
        internal bool Step()
        {
            if (Unit != null) return true;

            if (_settle > 0)
            {
                _settle--;
                if (_settle > 0) return false;
                int slots = CountActiveGroupSlots();
                _attempts.Add(Describe(_current) + ":groupSlots=" + slots);
                if (slots > 0)
                {
                    Unit = _current;
                    return true;
                }
            }

            _index++;
            if (_index < _party.Count)
            {
                _current = _party[_index];
                Path = "party-member";
                Select(_current);
                _settle = SettleFrames;
                return false;
            }

            if (_disposable == null)
            {
                if (_createDisposableCaster == null)
                    throw NoSubject();
                _disposable = _createDisposableCaster();
                _current = _disposable;
                Path = "disposable-caster-selected";
                Select(_current);
                _settle = SettleFrames;
                return false;
            }

            if (!_disposableAttached)
            {
                // The action bar may show only units the player controls
                // directly. Attaching the disposable caster to the party is
                // the game's own path to that, and it is undone in Restore.
                Game.Instance.Player.AddCompanion(_disposable);
                _disposableAttached = true;
                _current = _disposable;
                Path = "disposable-caster-in-party";
                Select(_current);
                _settle = SettleFrames;
                return false;
            }

            throw NoSubject();
        }

        private InvalidOperationException NoSubject()
        {
            Restore();
            return new InvalidOperationException(
                "No selected unit produced an active action-bar group slot; " +
                "there is nothing to anchor the projected menu to. Attempts: " +
                string.Join(" | ", _attempts.ToArray()));
        }

        internal string Describe()
        {
            return "path=" + (Path ?? "<none>") + ";unit=" + Describe(Unit) +
                ";attempts=" + string.Join("|", _attempts.ToArray());
        }

        /// <summary>
        /// Puts the selection back and removes anything this class created:
        /// the disposable caster leaves the party it was attached to and is
        /// disposed. Safe to call more than once.
        /// </summary>
        internal void Restore()
        {
            if (_restored) return;
            _restored = true;
            try
            {
                if (_disposable != null)
                {
                    SelectionManager selection = SelectionManager.Instance;
                    if (selection != null && selection.IsSelected(_disposable))
                        selection.UnselectUnit(_disposable);
                    if (_disposableAttached)
                        Game.Instance.Player.DetachPartyMember(_disposable);
                    if (_disposable.IsInState) _disposable.Destroy();
                    else _disposable.Dispose();
                    Game.Instance.EntityDestroyer.Tick();
                }
            }
            finally
            {
                UnitEntityData back = _originalSelection ?? _party.FirstOrDefault();
                if (back != null && back.IsInState && back.View != null)
                    Select(back);
            }
        }

        /// <summary>
        /// The game's own single-unit selection, as a portrait or unit click
        /// performs it: the selection is replaced by this unit and the
        /// selection event is sent, without the unit's acknowledgement bark.
        /// </summary>
        private static void Select(UnitEntityData unit)
        {
            SelectionManager selection = SelectionManager.Instance;
            if (selection == null)
                throw new InvalidOperationException(
                    "The game's selection manager is not available.");
            if (unit.View == null)
                throw new InvalidOperationException(
                    "The candidate " + Describe(unit) + " has no view to select.");
            selection.SelectUnit(unit.View, true, true, false);
        }

        private static int CountActiveGroupSlots()
        {
            return Resources.FindObjectsOfTypeAll<ActionBarGroupSlot>()
                .Count(value => value != null && value.gameObject != null &&
                    value.gameObject.scene.IsValid() &&
                    value.gameObject.scene.isLoaded &&
                    value.gameObject.activeInHierarchy);
        }

        private static string Describe(UnitEntityData unit)
        {
            if (unit == null) return "<null>";
            string name = unit.Blueprint == null ? "<no-blueprint>" :
                unit.Blueprint.name;
            return name + "(" + (unit.CharacterName ?? string.Empty) + ")";
        }
    }
}
