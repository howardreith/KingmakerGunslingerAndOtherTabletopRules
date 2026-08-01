# Generic firearm actions runtime qualification

## Checkpoint

This guarded scenario closes the remaining Sprint 30 runtime gate before any
Sprint 31 content begins. Its allowlisted name is `generic-firearm-actions`.

## Rule and project authority

ADR-0026 through ADR-0036 require exact item-owned state, delivery-time atomic
resource mutation, condition-preserving reload, and same-item recovery. Sprint
30 replaces Test-Musket blueprint-identity selection with exactly one equipped
`FirearmDefinitionComponent` while preserving the accepted transactions.

## Observable behavior

After the qualified receiver-bound path loads exactly
`KMG_AUTOMATION_WORKING` and obtains its stable fingerprint, the scenario:

1. proves the concrete native Heavy Crossbow weapon type has zero firearm
   markers and the distinct Test Musket type has exactly one;
2. prepares two independent Test Muskets entirely in memory;
3. changes only the equipped exact item through Wrecked -> Broken -> Normal ->
   Loaded/Normal using definition-driven Overhaul, Repair, and Reload;
4. requires the maintenance evaluator to reach `MaintenanceLoopPassed`; and
5. requires the request-scoped save-write sentinels to remain clear.

Any missing prerequisite, ambiguous save/item selection, transaction failure,
counter delta, blueprint marker leakage, or observed save API produces a
structured non-PASS result.

## Deterministic tests

- `scripts/Test-Sprint30GenericActions.ps1`
- `scripts/Test-RuntimeRequest.ps1`
- `scripts/Test-RuntimeScenarioPreflight.ps1`
- the complete dependency-free domain suite, including generic action,
  transaction rollback, two-item isolation, and maintenance evaluator tests.

## Runtime command

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario generic-firearm-actions `
  -ExpectedVersion 0.0.30 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false
```

The exact assembly must first pass `mod-load-smoke`. The scenario launches only
through Steam App ID 640820 and creates exactly one deployment backup for its
run.

## Non-goals

This diagnostic does not qualify action-bar delivery timing or interruption,
which are carried forward from the already accepted Test Musket abilities. It
does not add production firearms, scatter, multi-round capacity, class
progression, grit, deeds, acquisition, or balance changes.

## Rollback and persistence boundary

The existing transaction services roll back item and inventory resources on a
failed operation. The runtime fixture itself is disposable in-memory state and
must never save, quicksave, autosave deliberately, migrate, rename, copy, or
delete a save. Automatic process exit discards the fixture mutations.
