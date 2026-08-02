# Human input required

## Exact blocker

The source-qualified `gunslinger-starting-items` scenario on exact commit
`4c8c8d7` is the highest-priority remaining runtime gate for the Gunsmith /
starting-equipment row. It names only disposable save
`KMG_AUTOMATION_WORKING`, uses the guarded request mechanism, performs no save
operation, restores its exact inventory/class/gold mutations in `finally`, and
asserts the save-write sentinel. The external permission reviewer nevertheless
rejected it because feature-specific save-backed scenarios are not currently
authorized.

## Evidence

- Exact mod-load PASS: `20260802T1422559268495Z-mod-load-smoke`.
- Source qualification: repository validation, 848/848 domain/reflection
  tests, clean Release build, and strict package validation.
- Scenario contract: exact native starting grant and origin binding; bound
  pistol sale value `22`; fresh unbound production-pistol value is not `22`;
  exact rollback; no save-writing API.
- Rejected command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario gunslinger-starting-items `
  -ExpectedVersion 0.0.60 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false
```

## Why autonomous resolution is unsafe

Running a different scenario to obtain the same save-backed proof would
circumvent the explicit rejection. Raw-save inspection, UI automation, direct
executable launch, or another save is prohibited. Source and mod-load evidence
cannot prove the per-instance native vendor result inside the loaded game.

## Smallest precise question

Do you explicitly authorize the single guarded `gunslinger-starting-items`
command above against `KMG_AUTOMATION_WORKING`, with no save operation and its
existing exact rollback/sentinel requirements?

## Choices

1. **Authorize the named scenario (recommended).** This permits exactly the
   command above and allows the highest-priority runtime-partial row to proceed.
2. **Keep the boundary.** The Gunsmith/starting-equipment row remains
   runtime-partial, and the complete autonomous mission cannot reach its
   definition of done.

## Continuation

After authorization, run the exact command above. On PASS, run it a second
time from a fresh process if the checkpoint is classified risky, curate both
evidence manifests, update the matrices, commit, and immediately resume the
remaining completion audit.
