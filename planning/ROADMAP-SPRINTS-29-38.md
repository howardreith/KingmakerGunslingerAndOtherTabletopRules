# Accelerated roadmap: Sprints 29–38

## Change in cadence

Sprints 18–28 deliberately retired high-risk unknowns one at a time: exact rule hooks, per-item save persistence, native reconciliation, atomic inventory mutation, natural-roll capture, misfire condition damage, native saves/damage, spatial target queries, item lifecycle, and same-item recovery. That granularity was justified while each step crossed a new Kingmaker engine boundary.

Continuing with one tiny mechanic per sprint would now be wasteful. The core engine contracts are proven. Beginning with Sprint 29, a sprint should normally deliver one coherent player-visible vertical slice containing several related changes. A sprint splits only when it crosses a genuinely new, high-risk runtime surface.

## Sprint 29 — Qualification automation and complete maintenance loop

Player-visible outcome: a Wrecked firearm can be overhauled to Broken, then ordinarily repaired to Normal through explicit player actions.

Engineering outcome: add a deterministic in-game fixture/scenario runner that sets up common states and prints one concise PASS/FAIL matrix. Extract shared exact-equipped-firearm selection and cross-resource transaction utilities where doing so removes duplication without changing proven behavior.

Primary gate: interruption, exact-item isolation, atomic costs, and save persistence for both recovery stages.

## Sprint 30 — Generic firearm action runtime

Replace Test-Musket-specific Reload, Overhaul, and Repair adapters with definition-driven actions that operate on any exact marked firearm. Keep the existing Test Musket as the regression fixture.

Player-visible outcome: the same action contracts work from firearm definitions rather than hardcoded blueprint identity.

Primary gate: no native Heavy Crossbow leakage; multiple firearm definitions remain independently selectable and persistent.

## Sprint 31 — Early firearm catalog vertical slice

Add a production-oriented early pistol, early musket, and blunderbuss using the generic runtime, stable blueprint IDs, balanced definition data, and starting placeholder presentation.

Player-visible outcome: several distinct firearms can be equipped, fired, reloaded, misfired, damaged, and recovered.

Primary gate: definition-specific range, damage, misfire range, burst size, hands, capacity, and reload behavior.

## Sprint 32 — Scatter and close-range firearm attacks

Implement the blunderbuss scatter attack path, including native target enumeration, save/attack semantics chosen from verified Kingmaker contracts, close-range damage behavior, and exact ammunition consumption.

Player-visible outcome: the blunderbuss behaves as a distinct area firearm rather than a reskinned single-target weapon.

Primary gate: no duplicate targets, no damage outside the intended shape/range, and no impact on non-scatter firearms.

## Sprint 33 — Capacity, partial reload, and advanced firearms

Generalize per-item loaded-round state beyond capacity one. Add advanced pistol/rifle/revolver definitions and partial/multi-round reload behavior.

Player-visible outcome: multi-shot firearms fire several times before reloading and preserve exact remaining capacity across save/restart.

Primary gate: exact per-item round counts, partial reload atomicity, multi-weapon isolation, and misfire handling at every capacity state.

## Sprint 34 — Gunslinger class chassis

Add the player-selectable Gunslinger class skeleton: progression table, hit die, BAB, saves, skills, proficiencies, starting firearm/ammunition, and levels 1–5 feature slots.

Player-visible outcome: a new character can enter gameplay as a Gunslinger and use the already-proven firearm system without development controls.

Primary gate: character creation, level-up, save migration, respec, and multiclass compatibility.

## Sprint 35 — Grit resource and deed framework

Add a persistent grit resource with deterministic gain/spend plumbing, action availability, and a reusable deed component framework.

Player-visible outcome: Gunslinger abilities can consume and restore grit through explicit rules.

Primary gate: no double-spend or double-refund, correct rest/encounter persistence, and safe behavior across save/reload and turn-based/real-time modes.

## Sprint 36 — Core deed bundle

Deliver a coherent first deed set rather than one deed per sprint: Deadeye, Gunslinger’s Dodge, and Quick Clear, plus the minimum supporting UI and diagnostics.

Player-visible outcome: the class has a recognizable tactical loop and Quick Clear connects directly to the proven Broken firearm state.

Primary gate: exact grit costs, action economy, target isolation, and interaction with reload/misfire/recovery.

## Sprint 37 — Feats, reload economy, and class integration

Add the firearm feat bundle and class integration needed for normal play, such as Gunsmithing, Rapid Reload, and Amateur Gunslinger or their Kingmaker-appropriate equivalents after rules review.

Player-visible outcome: firearm action economy and access can be built through standard character progression rather than diagnostics.

Primary gate: prerequisite enforcement, stacking rules, level-up/respec, and no accidental effect on bows/crossbows.

## Sprint 38 — Content integration and alpha hardening

Integrate firearm/ammunition/repair-kit acquisition into vendors or loot, replace the most disruptive placeholder presentation where practical, finalize migration/versioning, run a broad compatibility matrix, tune balance, and produce the first non-disposable alpha candidate.

Player-visible outcome: a campaign can acquire, use, maintain, and progress firearms without development buttons.

Primary gate: clean installation/update, long-save persistence, economy sanity, common-mod compatibility, packaged documentation, and removal or clear isolation of test-only controls.

## Roadmap flexibility

This is a target sequence, not a promise to ignore evidence. A sprint may split if it discovers a new unsafe engine contract. Conversely, content and configuration work that uses already-qualified contracts should be bundled rather than promoted into separate micro-sprints.
