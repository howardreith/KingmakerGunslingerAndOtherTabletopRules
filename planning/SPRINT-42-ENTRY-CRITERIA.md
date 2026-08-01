# Sprint 42 entry criteria: Gun Training

## Authoritative rule

- At Gunslinger levels 5, 9, 13, and 17, select one additional specific
  firearm type.
- Firing a selected type adds the wielder's Dexterity modifier to weapon damage.
- A selected type's Broken-state misfire increase is +2 rather than +4.
- Selections are cumulative and may not grant the same firearm type twice.

## Kingmaker adaptation boundary

- Firearm type means the exact immutable `FirearmDefinition.Kind`; borrowed
  native `WeaponCategory` values are never firearm identity.
- The selection surface must expose only supported production firearm kinds and
  preserve ordinary level-up/respec behavior with stable project-owned IDs.
- Dexterity is added through the native weapon-damage calculation/delivery
  pipeline exactly once for a fired, selected exact firearm. It must not affect
  attack rolls, explosions, Pistol-Whip surrogates, native crossbows, unselected
  firearm kinds, or non-weapon damage.
- The current firearm condition model already treats an early firearm's second
  Broken-state misfire as destructive. Before source implementation, establish
  the exact existing `+4` representation or document the narrow equivalent
  needed to make the trained `+2` distinction observable; do not invent a
  balance substitute.

## Observable acceptance

- Progression grants one selection at exactly 5/9/13/17 and no other level.
- Four distinct choices can accumulate; duplicate selection is rejected by
  native prerequisites or an equally exact fail-closed prerequisite.
- Positive, zero, and negative Dexterity modifiers affect only selected-kind
  firearm weapon damage and are applied once.
- Selected and unselected firearm kinds retain their respective Broken-state
  misfire behavior, with exact item state and discharge atomicity preserved.
- Multiclass level, respec, save/load, multiple-unit, and multiple-firearm state
  remain isolated through ordinary fact ownership and exact item identity.

## Deterministic tests

- Exact level cadence, cumulative distinct selection, and duplicate rejection.
- Selected/unselected kind matching independent of borrowed weapon category.
- Positive/zero/negative Dexterity damage and duplicate-event protection.
- Normal and Broken misfire policy for selected versus unselected kinds.
- Native/nonfirearm, explosion, surrogate, wrong-unit, null, and ambiguous
  contexts fail closed.

## Runtime evidence

- Exact mod-load PASS for the source commit.
- A guarded scenario must inspect the production level entries and stable
  selection/choice identities, then deliver deterministic selected and
  unselected firearm attacks proving exact damage and Broken-state misfire
  outcomes without saving.
- Require two independent fresh-process PASS runs before runtime qualification.

## Non-goals

- No new firearm kinds, native enum members, archetypes, alternative capstones,
  custom level-up UI, or global changes to crossbows and other ranged weapons.
- Gun Training does not alter touch-AC penetration, grit, reload costs,
  ammunition compatibility, enhancement, critical rules, or scatter targeting.

## Failure and rollback

- Missing or ambiguous exact firearm markers, choice facts, damage contexts, or
  item state reject the bonus without changing native damage or firearm state.
- A post-discharge fault follows the existing firearm transaction and diagnostic
  contracts; qualification must not hide or reinterpret such a fault.
