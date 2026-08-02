# Human input required: repaired deed runtime attempts

## Exact blocker

The remaining independently executable deed work requires renewed permission
to run two existing save-free guarded scenarios. Startling Shot and Targeting
Head each reached the two-attempt boundary before later evidence-supported
repairs were source-qualified. The mission forbids treating those repairs as
runtime proof or launching another attempt without renewed authority.

These scenarios do not load or write a save. They construct disposable runtime
units, exercise one deed, assert exact rollback, and exit through the guarded
Steam App ID 640820 harness.

## Evidence

- Startling Shot repair `3a26059` reconciles Kingmaker's null-return/applied-fact
  behavior after runs on `4cb5251` and `b7eeaa9` reached the attempt limit.
- Targeting Head repair `4485dba` observes the authoritative target damage
  delta after retained run `20260802T1042345480789Z` proved every mechanical
  invariant except the stale observer field.
- Both repaired implementations pass repository validation, the complete
  domain suite, clean Release build, package validation, and exact mod load.
- Sprint 93 is closed on `95d6fb3`; the working tree was clean before this
  blocker record.

## Smallest precise question

Do you authorize the two save-free guarded commands below for the repaired
Startling Shot and Targeting Head checkpoints?

Recommended: authorize both. This resolves the highest-priority remaining
evidence gap without save access or a new implementation choice.

## Exact continuation commands

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-gunslinger-startling-shot `
  -ExpectedVersion 0.0.60 `
  -ExitAfterCompletion:$true `
  -Confirm:$false

.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-gunslinger-targeting-head `
  -ExpectedVersion 0.0.60 `
  -ExitAfterCompletion:$true `
  -Confirm:$false
```

If authorized, run each against the current exact assembly, diagnose only from
structured evidence, curate qualified results, and continue immediately to the
next mission gate.
