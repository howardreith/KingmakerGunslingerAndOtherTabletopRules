# Sprint 38 Gunslinger Initiative qualification

The grit-gated +2 initiative slice is runtime-qualified on source commit
`8ecb3aa99f91b45d9a9c7cac7868b83f05b37735`.

## Implemented contract

- The level-three class feature adds exactly +2 to the owning unit's native
  initiative roll while current Gunslinger grit is positive.
- The feature spends no grit, does not replace or force the d20, and applies at
  most once to the same rule object.
- It uses Kingmaker's exact `IUnitInitiativeHandler` boundary after the native
  Initiative stat is snapshotted and before the result is consumed.
- Zero grit and unrelated units receive no modifier change.

The tabletop Quick Draw clause is not claimed by this qualification. Exact
native contracts must still jointly establish Quick Draw identity, free and
unrestrained hands, visible firearm selection, and an action-free weapon-set
change before that clause can be implemented or accepted as an adaptation.

## Deterministic qualification

- Repository validation passed through Sprint 38.
- The complete clean Release domain suite passed 740/740.
- Runtime-request and preflight suites passed 18 and 40 checks.
- Validation-dispatch integration passed 16 checks.
- Exact-reference Release build and strict standalone package validation passed.
- Package SHA-256: `33e70a386dba3c28cf5ca4fc74cdfa6b291586da24d25269b44a4abf6e499d61`.
- DLL SHA-256: `1e120508f20f13ab74136c244c409f396bc29c97e9ced175398ebf8b71101a26`.

## Guarded runtime evidence

- Mod load PASS: `20260801T2211000340072Z-mod-load-smoke`.
- Fresh-launch Initiative PASS:
  `20260801T2212165760676Z-disposable-gunslinger-initiative`.
- Independent fresh-launch Initiative PASS:
  `20260801T2213341079691Z-disposable-gunslinger-initiative`.

Both qualifying runs observed exactly
`initialGrit=1;afterPositiveGrit=1;withBefore=0;withAfter=2;withDuplicate=2;emptyGrit=0;emptyBefore=0;emptyAfter=0;applied=1;rejected=1;duplicates=1;faults=0`.
Both removed the feature/resource facts, disposed the detached unit, and
preserved party/global-unit reference snapshots. No save was selected, loaded,
or written.

The earlier exact-commit run
`20260801T2207477896114Z-disposable-gunslinger-initiative` failed only because
its fixture asserted an initial grit value of two; its observed mechanics were
otherwise identical. The repaired assertion requires positive grit and no
spend, matching the authoritative rule without prescribing a detached unit's
classless resource maximum.

## Disposition

The +2 initiative slice is `RUNTIME-QUALIFIED`. Gunslinger Initiative remains
partially complete until the conditional Quick Draw clause receives an
evidence-backed implementation or documented adaptation disposition. Continue
that contract audit immediately; Sprint 38 is not a stopping condition.
