# Projected summon-menu acceptance rubric

## Why a projected menu is measured at all

The charter's roster grows the largest Summon Monster parent to **120** entries
and the largest Summon Nature's Ally parent to **110**. Those entries do not
exist yet. The question that has to be settled before they are built is whether
the widget that renders them survives the size, because discovering otherwise
after a hundred creatures are authored would be expensive.

The shipped observation of this menu is supervised: it watches for a menu a
human opened, and reports on the list that exists today. That is the right shape
for confirming what a player actually saw and the wrong shape for a scalability
gate. So a development-only fixture drives the menu itself, with a projected
list, and measures it.

Nothing is published to do this. The projected entries are transient
`AbilityData` built from blueprints that are already published, repeated to
reach the projected count, held for the duration of one measurement and never
written to a parent, a spellbook, a save, or an action-bar binding.

Repetition is deliberate. What the widget does with N entries - layout,
scrolling, reachability, cost - does not depend on the entries being distinct
creatures, and inventing plausible-looking future creatures would put unreviewed
content in front of a measurement while proving nothing extra.

## What is measured, and what counts as acceptable

Each family is opened and closed three times. One open proves it renders; three
prove it does not accumulate.

| # | Criterion | Acceptable |
|---|---|---|
| 1 | Renders at all | A snapshot is obtainable after the toggle settles |
| 2 | Entry count | Rendered slot count equals the projected count exactly - no silent truncation |
| 3 | First entry | Reachable, and visible without scrolling on open |
| 4 | Middle entry | Reachable |
| 5 | Last entry | Reachable, and navigation verified by the layout runtime rather than inferred from a rect |
| 6 | Containment | The rendered popup lies inside the safe area at the tested viewport |
| 7 | Scrolling | When, and only when, the desired height exceeds the safe area: exactly one `ScrollRect`, exactly one viewport marker, vertical scrolling on, and a viewport rect present |
| 8 | Responsiveness | Open completes within **250 ms** including layout settle |
| 9 | Slot accumulation | After the first open, no net growth in `ActionBarGroupSlot` instances across cycles |
| 10 | Managed memory | Recorded per cycle and reported; not a pass/fail gate |

### Notes on the two soft ones

**Memory (10) is reported, not gated.** `GC.GetTotalMemory(false)` across a few
frames of a running game measures the whole process, not this widget, and a
threshold on it would fail for reasons that have nothing to do with the menu.
Slot accumulation (9) is the leak signal that is actually attributable, so that
is the one with a verdict. The byte figures are recorded so a future regression
has something to compare against.

**Responsiveness (8) is a budget, not a benchmark.** 250 ms is chosen as the
threshold at which opening a menu stops feeling immediate. It is measured from
the `Toggle` call to the end of the settle frames, so it includes layout, not
just the call.

### What this rubric deliberately does not cover

- **Whether the menu looks good.** Layout correctness is measurable; visual
  quality is not, and claiming otherwise would be dressing up a judgement as a
  measurement. Visual acceptance is recorded separately as internal review, with
  human review marked not performed.
- **Tooltip content.** Whether a tooltip appears for a slot is a layout fact;
  whether its text is the right text is a content question answered by the
  structural inventory scenario, which checks names and icons against the
  catalog for every published entry.
- **Input devices other than mouse navigation.** Gamepad navigation of a
  120-entry list is a real question and a separate one; it is out of the
  Sprint 0-2 scope and is recorded as such rather than silently assumed.

## Where the verdict lives

`ExpandedSummoningProjectedMenuFixture.Passes` evaluates criteria 1-9 in
executable form, so this document and the gate cannot drift apart silently. The
scenario that drives it is development-only: it is constructed by the guarded
runtime-test runner for one allowlisted disposable scenario and by nothing else.


## Status of the live measurement, 2026-09-23

**Not obtained.** The fixture is implemented, wired as
`disposable-expanded-summoning-projected-menu`, and gated by the domain suite,
but it has not produced a measurement against the disposable working save.

Seven guarded runs were spent on it. What they established, in order:

1. The scenario was not named in the runner's working-save chains, so it
   launched and sat idle. Fixed, with a test that now enforces the naming.
2. `FindObjectsOfTypeAll<ActionBarSpellsGroup>` returns 48 instances, not one:
   prefabs and UI templates are included.
3. Filtering to instances belonging to a loaded scene leaves 47, because every
   `ActionBarGroupSlot` owns its own popup group. Toggling one of those produced
   no snapshot at all.
4. The reason is that the shipped layout anchors the popup to the slot the
   player clicked, captured by a Harmony prefix on `OnToggleGroupClick`. The
   fixture now reproduces that capture and toggles the clicked slot's own
   sub-group - the actual player path.
5. With that in place, no `ActionBarGroupSlot` is active in the loaded save's
   hierarchy at all, so there is no anchor to measure against.

The honest reading is that this fixture needs a save in which the caster has a
summon parent on the action bar, and `KMG_AUTOMATION_WORKING` does not present
one. Creating such a fixture is a change to the shared disposable save that
other scenarios depend on, and it is not obviously within the bounded scope of
this sprint.

### What is claimed instead, precisely

- **The layout policy is measured, exhaustively, at domain level.**
  `ExpandedSummoningMenuScalabilityTests` evaluates four viewports against 200
  option counts - 800 combinations - including the projected 120 and 110.
- **The real rendered menu is measured, supervised.**
  `observe-expanded-summoning-variant-menu` snapshots an actual menu a human
  opened, checking rendered bounds, viewport, scrolling, and first/middle/last
  reachability.
- **The live projected-size measurement is not claimed at all.** No number in
  this document is reported as having been observed at 120 or 110 entries in a
  running game.

The fixture stays in the tree because it is correct as far as it goes and
becomes useful the moment a suitable fixture exists; its failure message names
exactly what is missing rather than timing out silently.
