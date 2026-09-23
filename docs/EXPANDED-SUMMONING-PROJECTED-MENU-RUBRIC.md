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
