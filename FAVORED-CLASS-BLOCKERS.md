# Favored Class Integration — Blockers, Defects and Owner Decisions

Every entry is specific: a missing permission, an unavailable provider, an
unqualified native event, a contradictory rule, an unsupported binary contract
or a reproducible defect. Nothing here is a completed qualification.

## Active environment risks

### B1 — Shared Kingmaker installation is used by another automation

- Observed 2026-09-23 20:31–20:42 EDT: Kingmaker processes (PIDs 12888, 44016,
  37056) launched by another project's automation (`KBP_AUTOMATION_*` saves,
  KingmakerBuffPlanner) without the KMG request flag. One of them staged KMG
  0.0.133 through its own workflow and then restored the files it found.
- Mission response: never terminate foreign processes, never inspect that
  repository, never modify its saves. Every KMG launch goes through the guarded
  launcher, which fails closed on a running Kingmaker, a live DLL/version that
  differs from the deployment manifest, or a loaded identity that differs from
  the expected build. Runs wait for 90 seconds of quiet before deploying.
- Status: MITIGATED (fail-closed); a concurrent launch can still invalidate a
  run, which then counts as a failure and is repeated.

### B1 update — overlapping foreign run on 2026-09-24

- 05:45:37Z–05:47:27Z: the KingmakerBuffPlanner lab's run
  `casting-ws-reload-20260924-rc3-01` staged its own Mods tree, including KMG
  0.0.133 (`.kbp-runtime-sentinel.json`), and launched Kingmaker (PID 38164).
  This mission's build-13 candidate was deployed at the time. That session
  restored the tree afterwards; the KMG folder was byte-identical to the
  build-13 deployment (DLL `d4c9535f…`, FeatureModules `6e24b278…`).
- The mission's deploy guard refused to deploy while that game ran. From then
  on every deploy and launch waited for 90 seconds with no Kingmaker process
  and no foreign sentinel. No foreign file or process was touched.

### B2 — CotW-only and CotW+FCB compatibility profiles cannot be staged

- `scripts\compatibility\Invoke-KingmakerCompatibilityProfile.ps1` stages the
  `gunslinger-call-of-the-wild` and `gunslinger-call-of-the-wild-favored-class`
  profiles from the reference root `C:\Dev\KingmakerGunslingerLab\examples`,
  which does not exist on this machine. Both runs failed before staging
  anything; the live Mods tree was verified intact afterwards.
- The isolated `gunslinger-only` profile (H01, host physically absent) PASSED.
- Status: BLOCKED (missing fixture). The full-install run already exercises
  the Favored Class + CotW stack; the missing profiles only add a CotW-without-
  host observation (H02 "installed but not ready" is domain tested).

## Pre-existing KMG defects found by this mission (owner decisions)

These are outside the adopted charter rows. They are recorded, not redesigned.

### D1 — Base Gunslinger cannot complete level 17 (Gun Training dead end)

- `GunslingerClassBlueprints` grants the obligatory `KMG_GunTraining_Selection`
  at levels 5, 9, 13 and 17, but the selection offers only the three official
  rank-one types (Pistol, Musket, Blunderbuss). A Gunslinger who took all three
  can never complete level 17: `FeatureSelectionState.CanSelectAnything` returns
  true for every obligatory selection, so `LevelUpState.IsComplete()` stays
  false.
- Native evidence: `runtime-evidence/20260924T0054089932945Z-disposable-favored-class-grit`
  (`favored-class-grit.json`, level 17: `openSelections=[KMG_GunTraining_Selection]`,
  every other completion term satisfied) for both the test and the control unit.
- Affected: base class and Mysterious Stranger (which keeps the 9/13/17 picks
  but loses the 5th-level one, so it has exactly three picks and is not stuck);
  Pistolero and Musket Master replace the picks and are unaffected.
- Mission handling: the twenty-level favored-class proof uses the Pistolero.
- Owner decision needed: allow a repeated type, publish a fourth choice, make
  the 17th pick non-obligatory, or remove the level-17 entry.

### D2 — Dead Shot critical confirmation is unreachable

- `DeadShotRuntime` probes set `RuleAttackRoll.ImmuneToCriticalHit = true`, and
  the native `RuleAttackRoll.OnTrigger` computes
  `IsCriticalRoll = hit && !ImmuneToCriticalHit && ...`, so no probe records a
  threat; `DeadShotOutcomeService` therefore never produces a confirmation
  penalty and the delivery never threatens.
- `DeadShotRuntime.ConfigureDelivery` also assigns (rather than adds)
  `CriticalConfirmationBonus`, which would discard Critical Focus and every
  other confirmation bonus if the path were reachable.
- Evidence: source and decompiled native `RuleAttackRoll.OnTrigger` (static);
  not natively reproduced by this mission.
- Consequence for G02/G08/G16: the firearm confirmation bonus applies to every
  reachable firearm confirmation roll; Dead Shot has none today.
- Owner decision needed: whether Dead Shot should threaten and confirm, and with
  which confirmation bonuses.

### D3 — Gunslinger Initiative timing (FIXED; affects G11)

- Native `UnitCombatPrepareController` stores `RuleInitiativeRoll.Result` into
  `UnitCombatState.Initiative` and sorts the turn order before it raises
  `IUnitInitiativeHandler`, so the Sprint 38 deed's +2 (added in that handler)
  never reached the stored initiative.
- Reproduced natively: `runtime-evidence/20260924T0137282472516Z-disposable-favored-class-initiative-timing`
  (`cf8a66d21`): stored 11 against an expected 17.
- Fixed in `732cddae3`: the deed now also handles
  `IInitiatorRulebookHandler<RuleInitiativeRoll>` and adds its bonus (and the
  G11 earned steps) inside the rule, before the result is stored; the global
  handler remains as a duplicate-guarded fallback.
- Native PASS: `runtime-evidence/20260924T0154493048526Z-disposable-favored-class-initiative-timing`
  (stored 22 = d20 16 + Initiative 0 + deed 2 + 4 earned Ifrit steps in both
  real-time and turn-based entry;
  the turn-based order puts the Gunslinger first) and the Sprint 38 regression
  `runtime-evidence/20260924T0155347348333Z-disposable-gunslinger-initiative`.
- `docs/SPRINT-38-GUNSLINGER-INITIATIVE-QUALIFICATION.md` carries a correction
  section; the earlier qualification observed only the rule modifier.

## Suspected host defects (Favored Class 1.3.1; recorded, not patched)

- H-1 — Eidolon natural-armor reward likely grants +0 (not verified natively;
  KMG's O08 reads the summoner's own full-leaf rank and gives +2 at two steps natively).
- H-2 — Drow Disarm reward capped at 1 (host data; unrelated to KMG rows).

## Owner decisions (design questions recorded, not decided)

### OD-1 — Mostly Human scope (Option A implemented)

The trait adds a hidden identity. That identity counts as human only for the
Favored Class host's own human prerequisites (a scoped bridge on exactly those
57 prerequisite instances) and for KMG's ancestry graph. Race, RaceId, scores,
creature type and person-spell behavior are unchanged. A broader "counts as
human" (feats, items, dialogue, other mods' race checks) is not implemented.

### OD-2 — Other "counts as human" providers

Races Unleashed and other providers' human-like races and traits are not
recognized as human. Only the verified Mostly Human identity and the host's
own explicit human routes grant human favored-class access.

### OD-3 — Host race traits

The host's optional race traits (`enable_traits`) are not bridged to Mostly
Human or to any KMG race.

### OD-4 — Two overlapping paladin auras (O06)

Each paladin's earned steps are part of that paladin's own Morale bonus. Where
two paladins' auras overlap, the game keeps one morale bonus; the higher-source
rule is the game's. Whether the paladin's own copy of the aura buff should also
receive the increment is open. The charter bars increasing self-immunities.

### OD-5 — Bloodline power thresholds and eligibility (I08/S06)

Elemental Resistance's 9th-level step and Elemental Blast's extra uses at 17
and 20 are thresholds and are held back. Call of the Wild's Primal Elemental
bloodlines are not offered. The efreeti and djinni bloodlines do not exist in
the game.

### OD-6 — Revelation thresholds and scaling policy (I06/S04)

- The 22 held-back thresholds listed in the target manifest stay at the actual
  oracle level. These include form tiers, 13th-level armor riders, single-level
  extra uses, the Battlecry +2 breakpoint, possession BAB, Time Sight's 18th
  step and the 20th-level breath weapon swap.
- An ability that a revelation grants at a level gate is scaled once it is
  granted. The gate never moves.
- Five targets have parameter read points beyond the audit's families.
- Dual-Cursed Fortune is not offered (eligibility question).

### OD-7 — Performance range scope (O01)

- Not targets: Soothing Performance, Deadly Performance, Thunder Call, Dance
  of the Dead, Call of the Wild masterpieces and Discordant Voice.
- The displayed range text and the visual ring are unchanged.
- Storm Call's installed area is 30 ft, although its text says 50 ft.
- An area shared with Call of the Wild's Evangelist or Exemplar copies widens
  whenever the investing character casts it.

### OD-8 — Icon art for 127 visible choices

These choices show Kingmaker's null-icon placeholder: 104 revelation leaves,
the 5 provider performance leaves, Firearm Confirmation (2), Dirty Trick and
Trip (2), Eidolon Armor (2) and the 12 visible Mostly Human choices. The icon
catalog records them as needing original art; no art was produced.

### OD-9 — Elemental character-creation re-qualification

With the Mostly Human control ON (the default), each geniekin race gains one
optional Heritage-phase selection. Earlier elemental character-creation
qualification scenarios may pin the exact previous Heritage-phase inventory;
they were not re-run in this mission.

### OD-10 — Persistence breadth

L01 fresh-process persistence was proven for the Gunslinger counters only.
The geniekin, aura, pet, power, revelation and performance counters use the
same registered identities and components but were not reloaded natively.
