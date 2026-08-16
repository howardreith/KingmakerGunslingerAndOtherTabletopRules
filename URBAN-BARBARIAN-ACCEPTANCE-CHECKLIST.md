# Urban Barbarian human acceptance checklist

The exact immutable `0.0.83` candidate documented below was **REJECTED** in
human review and is preserved only as superseded evidence. Do not describe it
as accepted or release complete. This checklist will be revised for the exact
`0.0.84` repaired candidate after its new mechanical qualification passes.

Human-review rejection findings:

- the level-2 live allocation grid exposed all 31 variants instead of six;
- all 31 variants used the same native Rage icon;
- current selection was not unmistakable in the live grid;
- the automated tier test exercised a patched accessor rather than the actual
  live player-facing enumeration path; and
- Crowd Control was not visibly confirmed in an ordinary two-kobold fight and
  requires real attack-pipeline and combat-log qualification.

## Superseded 0.0.83 candidate identity

- Artifact/source commit: `06cad804651faaace17bdf8432bcd071d50ce9e7`
- Qualification documentation commit:
  `636e4928502cb6a07374279a3a8b35f79f66f4e3`
- Branch: `codex/urban-barbarian`
- Version: `0.0.83`
- Package SHA-256: `b2b4fdd899a1e00955e972d94b45f5624f4d663e88581043cbf969c3d6e3d193`
- DLL and installed-DLL SHA-256:
  `c72eb71bc57b6be79b5cd49c58b262bf0897960eac2c118538d7e6e43cfccaae`
- DLL MVID: `1f53a664-2557-4866-b690-a720cbff840f`
- Deployment manifest:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260816T1831540393986Z\deployment.json`
- Deployment manifest SHA-256:
  `145de454d3e7282783f656a65d005036bd39a3f5f0f4bf43d511e07238817584`
- Kingmaker build: `2018.4.10.10503941`
- Mechanical seal: focused Urban PASS; two-launch existing-owner OFF
  persistence PASS; CotW normal/balance/absent PASS; module boundary 18/18
  PASS; 1,149/1,149 fast tests PASS.

The tracked qualification evidence was finalized in a later documentation-only
commit. That commit does not change the package, DLL, or installed candidate;
the artifact/source identity above remains authoritative for the rejected
candidate's preserved evidence only.

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
- [ ] Save before a locked chest or detected trap. A suitable ordinary-Rage
  control character is prohibited from performing the Dexterity-based
  Trickery action while raging, while the Urban Barbarian can perform the
  same Trickery check during Controlled Rage.
- [ ] With Controlled Rage still active after the Trickery comparison,
  spellcasting/concentration remains prohibited as under ordinary Rage.
- [ ] Stealth may be checked secondarily; Use Magic Device is not used as the
  primary skill test because Rage can independently restrict concentration-
  dependent magic-item activation.
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
