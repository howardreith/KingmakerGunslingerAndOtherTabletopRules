# Sprint 59 entry criteria: True Grit

## Authority

At Gunslinger level 20, select two deeds the Gunslinger can access. Each
selected deed costs 1 fewer grit, minimum 0. A positive-cost deed reduced to 0
still requires at least 1 current grit. A deed that normally requires positive
grit without spending it instead works at 0 grit. Slinger's Luck expressly
forbids True Grit cost reduction.

## Kingmaker adaptation

Level 20 grants the same stable prerequisite-respecting True Grit deed
selection twice. Each choice is a unit-owned feature keyed to one production
deed; duplicate choice is forbidden. Choices never mutate global deed
blueprints or share state between units.

One centralized policy distinguishes:

- ordinary fixed or computed grit costs, reduced by exactly 1 to minimum 0;
- positive-grit/no-spend gates, removed for a selected deed;
- a positive-cost deed reduced to 0, which still requires positive current
  grit even though it spends none;
- Slinger's Luck, which is never offered and never reduced;
- unavailable, omitted, or blocked deeds, which are not offered until their
  production feature exists and is independently qualified.

Cheat Death's ordinary cost is all remaining grit (minimum 1). When selected,
its effective cost is one fewer than that computed ordinary cost, minimum 0;
because this is a cost reduction rather than a pre-existing no-spend gate, the
trigger still requires at least 1 current grit. This retains exactly 1 grit
when ordinary remaining grit is greater than 1 and spends 0 when it is 1.

## Initial production choice catalog

Offer only implemented base deeds whose behavior has a production feature or
ability and whose True Grit integration can be tested end to end:

- Deadeye;
- Gunslinger's Dodge;
- Quick Clear;
- Gunslinger Initiative;
- Pistol-Whip;
- Utility Shot (Stop Bleeding);
- Dead Shot;
- Startling Shot;
- Targeting Head, Torso, and Legs as separate deed choices;
- Bleeding Wound;
- Expert Loading;
- Lightning Reload;
- Evasive;
- Menacing Shot;
- Cheat Death;
- Stunning Shot.

Death's Shot and Targeting Arms remain absent while blocked. Targeting Wings
is omitted. Slinger's Luck is explicitly excluded by its own rule. Adding a
later-qualified deed must append a stable choice without changing existing
identifiers.

## Acceptance criteria

1. Grant exactly two occurrences of one stable True Grit selection at level
   20, with one stable feature per offered deed and no duplicate selection.
2. Each unit's two choices are independent of every other unit and survive
   normal native fact persistence, level-up, and respec semantics.
3. A selected ordinary 1-grit deed requires positive grit, spends 0, and is
   rejected at 0 grit. An unselected copy retains its ordinary 1-grit cost.
4. A selected ordinary 2-grit deed costs 1; an unselected copy costs 2.
5. A selected positive-grit/no-spend deed works at 0 grit; the same unselected
   deed remains unavailable at 0 grit.
6. Variable costs reduce after their ordinary cost is computed, never before;
   minimum cost is 0 and no resource operation can underflow.
7. Selected Cheat Death follows the bounded all-remaining adaptation above;
   unselected Cheat Death retains its qualified all-remaining behavior.
8. Slinger's Luck saving and skill rerolls remain fixed at 2 and 1 grit and
   are absent from the choice catalog.
9. Duplicate callbacks, rollback, firearm state, action economy, save DCs,
   native attacks, and deed riders remain unchanged apart from the authorized
   grit gate/cost.

## Required evidence

- Focused domain tests cover the catalog, two-choice uniqueness, unit
  isolation, fixed/variable/no-spend gates, zero-grit boundary, Cheat Death,
  Slinger's Luck exclusion, and invalid inputs.
- Every offered deed adapter uses the centralized policy; a source validator
  fails when an offered adapter bypasses it.
- Repository validation, complete domain suite, clean exact-reference Release
  build, and strict package validation pass.
- One exact-version mod-load PASS and two independent guarded feature PASS runs
  prove two distinct selections, selected/unselected 1- and 2-grit behavior,
  selected zero-grit availability, fixed Slinger's Luck exclusion, unit
  isolation, progression, and cleanup.

## Non-goals

- Do not unblock or implement Death's Shot or Targeting Arms here.
- Do not add alternative capstones, archetypes, Signature Deed, new grit feats,
  or global resource-cost patches.
- Do not redesign already-qualified deed effects.
