# Favored Class Integration - Blockers, Defects and Open Questions

This file keeps only what the charter, the installed providers and the
qualification runs do not settle: environment risks, pre-existing KMG defects
outside the adopted rows, and suspected host defects. Every former owner
question that the charter or the observable provider behavior answers is
resolved below with the rule that decides it; required tests are recorded as
runs, not as decisions.

## Environment

### B1 - Shared Kingmaker installation (mitigated)

Another lab (KingmakerBuffPlanner) uses the same Kingmaker installation. The
two sessions follow an agreed protocol: before a launch or any change to
`Mods`, check that the other lab's `runtime-state\deployment.lock` and
`kbp-gate.active` markers are absent, hold this lab's
`compatibility-state\compatibility.lock` for the whole batch, message the other
session at batch start and end, and never touch the other lab's files, saves
or processes. Every KMG launch still goes through the guarded Steam App ID
640820 launcher, which fails closed on a running game, a foreign sentinel, or
a loaded identity that differs from the deployment manifest. The other lab
stages and restores its own `Mods` copy byte-exactly (including this lab's
installed KMG build) before releasing its lock. On 2026-09-24 a KMG batch
started while the other lab's lock-free WhatIf gate was running; that gate
failed closed on the live Kingmaker process and ran nothing. The other lab
now marks its gate with `kbp-gate.active`, and KMG batches wait for it.

### B2 - CotW-only and CotW + Favored Class profiles (resolved)

The profiles were staged from the 2026-09-08 capture of this install's
optional mods (`C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger\artifacts\teleportation\compatibility-references`),
verified byte-identical first (CallOfTheWild 266 files, ZFavoredClass 24
files, no difference). On the final candidate:

- `gunslinger-only` / `observe-favored-class-host-state`: `20260924T2319494086008Z-observe-favored-class-host-state` PASS (transaction `compat-20260924T231747Z-dcf3953a3add`, restoration verified).
- `gunslinger-call-of-the-wild` / `observe-favored-class-host-state`: `20260924T2323061576141Z-observe-favored-class-host-state` PASS (transaction `compat-20260924T232119Z-4b08e222f897`, restoration verified).
- `gunslinger-call-of-the-wild-favored-class` / `observe-favored-class-host-state`: `20260924T2326399456937Z-observe-favored-class-host-state` PASS (transaction `compat-20260924T232452Z-bdadf725cfa8`, restoration verified).

## Pre-existing KMG defects (outside the adopted rows; owner decisions)

### D1 - Base Gunslinger cannot complete level 17 (Gun Training dead end)

`GunslingerClassBlueprints` grants the obligatory `KMG_GunTraining_Selection`
at levels 5, 9, 13 and 17, but the selection offers only the three official
rank-one types (Pistol, Musket, Blunderbuss). A base Gunslinger who took all
three can never complete level 17: `FeatureSelectionState.CanSelectAnything`
is true for the obligatory selection, so `LevelUpState.IsComplete()` stays
false. Native evidence: `20260924T0054089932945Z-disposable-favored-class-grit`
(`openSelections=[KMG_GunTraining_Selection]` at level 17, every other
completion term satisfied, for the test and the control unit). Mysterious
Stranger keeps exactly three picks and is not stuck; Pistolero and Musket
Master replace the picks. The favored-class twenty-level proof therefore uses
the Pistolero. Decision needed: allow a repeated type, publish a fourth
choice, make the 17th pick non-obligatory, or remove the level-17 entry.

### D2 - Dead Shot critical confirmation is unreachable

`DeadShotRuntime` probes set `RuleAttackRoll.ImmuneToCriticalHit = true`, and
the native `RuleAttackRoll.OnTrigger` computes `IsCriticalRoll = hit &&
!ImmuneToCriticalHit && ...`, so no probe records a threat and
`DeadShotOutcomeService` never produces a confirmation. If that path were
reachable, `DeadShotRuntime.ConfigureDelivery` assigns (rather than adds)
`CriticalConfirmationBonus`, discarding Critical Focus and every other
confirmation bonus. Evidence: source and the decompiled native rule (static;
not reproduced natively). Consequence for G02/G08/G16: the firearm
confirmation counter applies to every reachable firearm confirmation roll;
Dead Shot has none today. Decision needed: whether Dead Shot should threaten
and confirm, and with which confirmation bonuses.

### D3 - Gunslinger Initiative timing (fixed in this mission)

The native `UnitCombatPrepareController` stores `RuleInitiativeRoll.Result`
before it raises `IUnitInitiativeHandler`, so the Sprint 38 deed's +2 never
reached the stored initiative (native reproduction:
`20260924T0137282472516Z-disposable-favored-class-initiative-timing`, stored
11 against 17). Fixed in `732cddae3`: the deed adds its bonus inside the rule
before the result is stored. Native PASS in real-time and turn-based entry.

### D4 - Same-level target picks dropped by the native replay (fixed in this mission)

The native level-up replays its picks in priority order, and the Favored
Class host's reward selection has an earlier priority (its feature group) than
a bloodline, a revelation or a power chosen in the same level-up. A
selected-power or selected-revelation counter picked after its target in the
same level-up was checked before the target existed and silently dropped (an
Ifrit Sorcerer 1 could not invest the level-1 bonus in the Fire Ray it had
just chosen; an Oracle could not invest in the revelation of that level).
Found by the L03 respec lane's native build of that Sorcerer. Fixed in
`b1b93acad`: a native replay hook (`LevelUpController.ApplyLevelup`) marks
the level-up being replayed, and the owned-target prerequisite also counts a
later pick of that same level-up; the usable-power check uses the same
prerequisite. Native PASS: the respec lane builds the Sorcerer through that
exact level-1 sequence.

## Suspected host defects (Favored Class 1.3.1; recorded, never patched)

- H-1: the host's own Eidolon natural-armor reward likely grants +0 (not
  verified natively). KMG's O08 counter reads the summoner's own leaf rank and
  gives +2 at two steps natively.
- H-2: the host's Drow Disarm reward is capped at 1 (host data; unrelated to
  KMG rows).

## Former owner questions, resolved by the charter or provider behavior

| Former item | Resolution | Rule or evidence |
| --- | --- | --- |
| OD-1 Mostly Human scope | A genuine dual identity: human (humanoid) for race-related rules and prerequisites (the host's human favored-class leaves and human race traits, humanoid targeting such as Hold/Charm/Enlarge/Reduce Person), native outsider identity, race, RaceId, scores and geniekin favored-class routes retained; meaningful with the integration off; never inferred from appearance; other providers fail closed. | Charter 5.2/6.5, E05/E07/E08/E10; `disposable-favored-class-mostly-human` |
| OD-2 Other "counts as human" providers | Not recognized: only the verified Mostly Human identity and the host's explicit human routes grant human access (Races Unleashed's Suli Mostly Human stays closed). | Charter: unknown providers never acquire unrestricted eligibility (E10, section on provider scope) |
| OD-3 Host race traits | The host's human race traits open to a Mostly Human geniekin through the scoped bridge, and stay closed to the standard control. | Mostly Human host-bridge assertion |
| OD-4 Two paladins | Native descriptor and strongest-source behavior: one Replace instance at a time carrying its own paladin's Morale bonus (+6 invested, +4 native), never a sum, no stacking with Remove Fear; self immunities unchanged. | M17; lifecycle `fcb-lifecycle-aura-overlap` |
| OD-5 Bloodline eligibility and thresholds | Eligible bloodlines are exactly Elemental (Fire) and (Air) plus proven aliases; power thresholds stay at the real sorcerer level (virtual levels never unlock a power early). | Charter I08/S06 and threshold rule; target manifest |
| OD-6 Revelation thresholds | Thresholds stay at the real oracle level; an ability a revelation grants at a gate is scaled once granted, and the gate never moves. | Charter I06/S04 threshold rule; oracle lane |
| OD-7 Performance scope | Owner-local actual range, ring and displayed text agree; Storm Call and Mockery are excluded with recorded reasons; instantaneous, personal, masterpiece and inert entries are not targets. | Charter 8.7; target manifest O01; `disposable-favored-class-performance-range` |
| OD-8 Icons | Every published choice has a deterministic appropriate existing donor icon; no published choice is blank. Original art is optional later polish. | Icon catalog; domain census; native visual census |
| OD-9 Elemental re-qualification | Required test: the elemental creator and respec lanes were re-run. | Run table in the report |
| OD-10 Persistence breadth | Required test: a fresh-process reload of one subject per state/mechanic family (partial and full investment; the selected firearm, performance, revelation and bloodline targets; spent grit with its raised maximum; Nimble; the Undine Monk; owner-local performance and aura areas; companion armor with replacement; revelation and bloodline scaling; Mostly Human identity and human access). The own persistence cases of G05, G06 (Dodge), G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04 and S06 are still NOT RUN and are listed as a remaining gap in the report. | L01 families transaction in the report |
