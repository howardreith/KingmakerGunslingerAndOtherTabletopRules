# Urban Barbarian human acceptance checklist

Use this checklist only against the exact immutable `0.0.83` candidate whose
commit, package/DLL hashes, MVID, deployment manifest, and completed mechanical
runtime evidence are supplied in the handoff. Leave the installed candidate
unchanged during review. Any gameplay-source or packaged-artifact change
invalidates the candidate and requires a new build and qualification cycle.

## Character creation and presentation

- [ ] With Urban Barbarian ON, the native Barbarian archetype list shows
  **Urban Barbarian** exactly once.
- [ ] Name, description, icon, and progression rows are readable and coherent.
- [ ] Level 1 shows Urban Barbarian Proficiencies, Crowd Control, and Controlled
  Rage, and does not show Fast Movement.
- [ ] The displayed class skills are Athletics, Mobility, Knowledge (World),
  Perception, and Persuasion.
- [ ] Simple and martial weapons, light armor, and non-tower shields remain;
  medium armor proficiency is absent and no unrelated proficiency changed.
- [ ] Native Barbarian and any CotW Barbarian archetypes remain present,
  ordered, and unduplicated.

## Crowd Control

- [ ] The tooltip states the two-active-hostile threshold, +1 attack, +1 dodge
  AC, edge-to-edge adjacency, and that weapon reach does not extend adjacency.
- [ ] Visible combat values grant neither bonus with zero or one adjacent
  enemy and exactly +1 attack/+1 dodge AC with two or more.
- [ ] The visible result updates promptly when an enemy moves in/out or dies.
- [ ] A large adjacent enemy behaves by creature edge, and a reach weapon does
  not enlarge the five-foot adjacency boundary.
- [ ] Melee and ranged attacks receive the same threshold bonus.

## Controlled Rage selector and tiers

- [ ] The action bar/ability panel contains one compact Controlled Rage
  Allocation selector, not 31 top-level buttons.
- [ ] The current selection is unmistakable and the selector remains readable
  at normal action-bar and ability-panel size.
- [ ] Ordinary Rage shows only the six legal +4 allocations: three full-score
  and three +2/+2 choices.
- [ ] Greater Rage shows only ten +6 allocations, including full +6, every
  +4/+2 direction, and +2/+2/+2.
- [ ] Mighty Rage shows only fifteen +8 allocations, including full +8,
  +6/+2, +4/+4, and +4/+2/+2 families.
- [ ] Each newly unlocked tier defaults independently to full Strength; an old
  tier selection is neither active nor offered as usable.
- [ ] Selection costs no Rage rounds, persists until changed, and cannot be
  changed while Rage is active.

## Controlled Rage mechanics

- [ ] Full Strength, Dexterity, and Constitution choices change the actual
  score by exactly +4/+6/+8 at their respective tiers.
- [ ] Representative split allocations change each actual score exactly and
  sum to the current pool.
- [ ] No selected bonus applies while Rage is inactive.
- [ ] Controlled Rage grants no ordinary Rage attack bonus, weapon damage
  bonus, temporary HP, Will bonus, or AC penalty.
- [ ] Intelligence-, Dexterity-, and Charisma-based skills remain usable.
- [ ] Spellcasting/concentration remains restricted as under ordinary Rage.
- [ ] The native Rage resource and live counter remain visible; activation,
  per-round spending, cancellation, fatigue, and Tireless Rage behave normally.

## Constitution and integration

- [ ] Constitution allocation at full health increases maximum/current HP only
  through the real Constitution modifier and removes it exactly when Rage ends.
- [ ] After damage and at low HP, ending Rage restores the same damage deficit
  without healing, duplication, or an immortal negative-HP state.
- [ ] Repeated entry/exit, level transition, and save/load do not create HP or
  duplicate modifiers.
- [ ] A representative passive native rage power, activated native rage power,
  and Rage-required feat or item recognize Controlled Rage.
- [ ] Under the supported CotW profile, a representative CotW-added rage power
  and Rage marker work without duplicates; with CotW absent, the Urban core
  remains fully available.

## Module and compatibility behavior

- [ ] The UMM label is **Urban Barbarian** and does not say CotW is required.
- [ ] CotW status text clearly distinguishes native core availability from
  optional interoperability qualification.
- [ ] With the module OFF, Urban Barbarian is absent from new selection/respec.
- [ ] An existing Urban owner loaded with the module OFF retains progression,
  selection, Rage, Crowd Control, and all owned features.
- [ ] Repeated restart or settings changes produce no duplicate archetype,
  feature, buff, selector, allocation, resource, marker, or action.

Record acceptance or each requested change against the immutable identity in
the handoff. Do not modify or replace the installed candidate during review.
