# Z-FIREARM-MAINTENANCE — Mission State

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Last updated:** 2026-09-13 (review fixes R1–R5 + R6 partial + Dead Shot evidence; pushed 73328618)

## Mission status

`IN_PROGRESS` — phase **P1–P3 reopened per owner review of 35bee7ed and
corrected (journal #9–#11); remaining: review regressions needing behavioral/
native proof, scatter slice, remaining P4d scenarios, rebuild, lanes, P5**

First incomplete acceptance IDs: ALL native cells `NOT RUN`; several domain
cells are policy/wiring-contract only (see the matrix NOTE at the top of
`docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`); R03/R08 fully `NOT RUN`.

Branch HEAD `73328618` (all work pushed; remote verified per checkpoint).

## Candidate identity

**SUPERSEDED — source changed after journal #6's build** (scenario code
added in commits 0991f641..f4060c67; src/ deltas exist). Historical:
0.0.127 build @ 6228574, package SHA-256 97e4039f...f72f, DLL SHA-256
bbceda44...f835, MVID 204d1c6d-6f02-4650-8711-e82239dce9cb (strict UMM
validation PASS; journal #6). Before any native run: rerun
`scripts/Build-Local.ps1` and record the NEW package/DLL SHA-256/MVID;
all native evidence must reference that identity.

## Implementation decisions so far

**P1 (HISTORICAL — first implementation, superseded by reviews R1/CR2-04 and
R3; see journal #10/#12 for the current design):** the original frame-scoped
click marker and the "repaired weapons are never gated" rule were defects and
no longer exist. CURRENT behavior: player intent = one-shot per-(executor,
clicked-target) authorizations recorded by the `ClickUnitHandler.OnClick`
prefix only for selected attackable pairs, consumed only by a construction
running with the genuine click handler still on the call stack; wrong-target
queries never erase a valid authorization; suppression is released ONLY by a
genuine new player order — repair/reload never revive the cancelled order.

**P0 binding decisions** (details in contract + journal #2):
- **P2 rest boundary candidate**: prefix on `RestController.StopRestProcess()`
  (called exactly once per rest-process termination, before its trailing
  autosave), gated on `Status.RestSucceeded && !NightRandomEncounter &&
  !Status.SkipTime`. Per-unit `ApplyRest` postfixes are unusable (transient
  mid-sleep + non-camping callers: LevelUp, Kingdom, Recruit, Respec,
  Capital). Runtime-verify before committing to it.
- **P3 shape**: Broken-only + out-of-combat enforcement goes in the
  field-repair path (policy/runtime/ability layers); the shared
  `FirearmStateMachine.Repair` transition stays combat-usable for Quick Clear.
  Command-start binding of the concrete firearm must be added at actual
  ability-command commencement (today both checks are inside delivery).

**Test-count bookkeeping mechanism (learned, journal #3)**: adding domain
tests requires bumping `DETERMINISTIC_TEST_COUNT` in the ACTIVE validator and
the active-chain blocks in `validation/static-validation.json`
({121,123,124,125,126} — live current-expectation records; deeper blocks
frozen). P4's 0.0.127 validator supersedes the interim bump.

## Results

| Gate | Result | Evidence |
| --- | --- | --- |
| Identity verification / repository validation | PASS each slice | journal #1–#6 |
| Domain baseline @ base | PASS 1581/1581 | journal #2 |
| Domain after P1/P2/P3 | PASS 1592/1601/1611 | journal #3/#4/#5 |
| 0.0.127 source validation (full chain) | PASS | journal #6 |
| Full Build-Local pipeline | **PASS exit 0** (validation + 1611/1611 + exact-reference build + strict package validation x2) | journal #6 |
| Native runtime | NOT RUN | — |

## Next concrete actions

1. **Review-regression behavioral slices** (extend
   `RuntimeTestRunner.FirearmMaintenance.cs`, patterns in journal #7/#8/#11):
   a. Scatter: real all-misfire scatter commit suppresses (two-target cone
      fixture pattern at RuntimeTestRunner.cs ~26000; forced rolls {1,1}).
   b. R1 native controls recorded for the interactive lanes: attack order for
      unit B leaves A suppressed; real nonattack click records nothing
      (CanAttack filter); controller/console route trace; brain re-issue
      after click-return rejected (postfix-clear mechanism already covered
      by the click-end-clears bridge assertion).
   c. R4/R5 lifecycle slice: bind via a real UnitUseAbility
      (RunDisposableGunsmithingCrafting pattern), then exercise
      ineligible-at-start (combat seeded via a real encounter — verify a
      safe native route, do NOT fake booleans), weapon change, cancellation,
      unrelated-ability end, capability loss (feature removal) between
      start and delivery, legacy Overhaul invocation.
2. **Remaining P4d scenarios**: field-repair rejection; completed-rest
   restoration (+ cancelled/interrupted no-op; verify which scripted route
   reaches StopRestProcess); persistence round trip (named disposable save
   IF an authorized write route exists — else BLOCKED).
3. **Rebuild** Build-Local; record NEW package/DLL SHA-256/MVID here (two
   builds superseded since journal #6).
4. **Deploy + native lanes x2** via Invoke-KingmakerRuntimeTest
   (-ExpectedVersion 0.0.127); restore install after.
5. P5: acceptance matrix, RELEASE-NOTES PENDING gates, final report.

Review-disposition status: R1 FIXED(source)+pending native controls; R2
FIXED(source+Dead Shot behavioral)+scatter slice pending; R3
FIXED(policy+tests); R4 FIXED(source)+lifecycle slice pending; R5
FIXED(source)+lifecycle slice pending; R6 PARTIAL(scenario corrected,
bridge-labeled; matrix note added) — native/interactive proof outstanding.

No active processes/leases; live install untouched (0.0.126 release).
