# Sprint 36 Deadeye qualification

Deadeye is runtime-qualified on source commit
`8ef88541e4fd1f7bd6bc881b5da7e80b0d488a01`.

## Implemented contract

- A level-one deed feature grants a personal extraordinary free action.
- The action adds a hidden native persisted fact that arms the next successfully
  discharged exact firearm shot.
- Beyond the first range increment, cost is one grit per additional increment.
- A successful spend authorizes touch AC while preserving native range penalties
  and contextual AC deltas. Insufficient grit fails atomically.
- Non-firearm actions and failed/empty discharges do not consume the armed fact.

## Deterministic qualification

- Repository validation passed through Sprint 36.
- The complete clean Release domain suite passed 719/719.
- Runtime-request, preflight, and dispatch suites passed 18, 40, and 13 checks.
- Exact-reference Release build and strict standalone package validation passed.
- Package SHA-256: `a59e090b2d14911f77f64ba57d26762d2692c1b25ae7e3b6bd38f25b48d7e8f8`.
- DLL SHA-256: `ec58e53a0c9747a34fa55db2290219cf868f13c171978171b1622f9d45ea2426`.

## Guarded runtime evidence

- Mod load PASS: `20260801T2039304720861Z-mod-load-smoke`.
- Fresh-launch Deadeye PASS:
  `20260801T2040462109967Z-disposable-gunslinger-deadeye`.
- Independent fresh-launch Deadeye PASS:
  `20260801T2042028613784Z-disposable-gunslinger-deadeye`.

Both feature runs observed exactly
`initial=2;armedBefore=True;authorized=True;afterApplied=1;armedAfter=False;duplicateAuthorized=True;afterDuplicate=1;insufficientAuthorized=False;afterInsufficient=1;insufficientConsumed=True;applied=1;rejected=1;faults=0`.
Both disposed the detached entities, forgot the firearm state, and preserved the
party and global-unit reference snapshots. No save was selected, loaded, or
written.

## Disposition

Deadeye is `RUNTIME-QUALIFIED`. Continue Sprint 36 with Gunslinger's Dodge.
