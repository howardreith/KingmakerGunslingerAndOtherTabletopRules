# Supervised manual save-load observation

## Purpose and safety boundary

`observe-manual-save-load` is a guarded, runtime-only probe for one supervised
load of `KMG_AUTOMATION_WORKING`. It is inactive unless the existing
`-kmgRuntimeTestRequest` contract accepts an allowlisted request with the exact
mod version, unique run ID, evidence directory, and timeout. The probe never
chooses a save and never calls a load, save, autosave, quicksave, delete,
rename, migration, or overwrite API.

Never load or overwrite `KMG_AUTOMATION_BASELINE`. It is immutable. If the
probe observes that name, the run is `FAIL`.

## Supervised procedure

From a clean, qualified feature branch, run:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-manual-save-load `
  -ExpectedVersion 0.0.30 `
  -TimeoutSeconds 300 `
  -ManualInteractionRequired
```

The orchestrator builds and validates, reaches live deployment only through
`Deploy-Local.ps1` (which owns exactly one backup), writes the guarded request,
and starts Kingmaker only through Steam App ID 640820 without GUI automation.
There is no direct-`Kingmaker.exe` fallback. When it prints
`MANUALLY LOAD KMG_AUTOMATION_WORKING NOW`, use Kingmaker's normal UI to click
Load Game, select the save whose displayed name is exactly
`KMG_AUTOMATION_WORKING`, and click the normal Load control once. Do not select
the baseline, save the game, quicksave, or interact with save management.

## Observed evidence

Request-scoped Harmony prefixes and postfixes record entry/exit ordering,
monotonic elapsed time, UTC time, managed thread, exact method signature,
argument types, the `SaveInfo` descriptor type, and only allowlisted identifier
fields. File identifiers are reduced to a leaf name. The probe also records:

- candidate `Game`, `MainMenu`, and `SaveManager` load lifecycle calls;
- registration and invocation of a read-only after-load callback;
- area/scene availability and two identical game-thread samples of a compact
  party/player fingerprint;
- the pre-load and stable post-load area/scene/player state, load-start and
  callback-completion timestamps, and whether every callback ran on the Unity
  update thread;
- any call to an allowlisted set of save-writing method names;
- exceptions, timeout, patch removal, and atomic result paths.

It does not record raw save contents, object dumps, unrelated command-line
arguments, Steam data, unrelated files, or personal data. Observation patches
do not accept `ref`, `out`, or result parameters and cannot change arguments,
return values, or game behavior. They are removed when the scenario ends.

## Outcomes

- `PASS`: the working name is positive, the after-load callback fires, loaded
state is stable, the requested mod version matches, no writing API is
observed, every callback remains on the game thread, patches are removed, and
the result is atomically flushed.
- `FAIL`: baseline or another save is identified, prerequisites contradict the
  request, or a forbidden save-writing method is observed.
- `AMBIGUOUS`: identity, completion, or observed API meaning cannot be proved.
- `TIMEOUT`: no valid manual load completes before the configured deadline.

Non-PASS outcomes make the orchestrator return nonzero. If
`exitAfterCompletion` is true, clean game exit is requested only after the
atomic result writer succeeds.

## Limits and later use

This observation can establish which public entry point receives `SaveInfo`,
the nested call order, relevant Boolean arguments, callback timing, scene/area
transition timing, and whether a write-shaped API occurs during the manual
load. One run cannot prove undocumented semantics universally, that no native
code writes, or that an exact programmatic load is safe. A later human-reviewed
task may use the structured call order and completion evidence to select (or
reject) an exact-save API; it must not infer safety from a build or a single
successful run.

The current safe reflection surface proves party count and main-character,
area, scene, and player game-ID presence. It does not safely establish a
stable character display-name or inventory/equipment enumeration contract.
Accordingly, the probe does not dump party, inventory, or equipment objects
and cannot by itself prove that every Sprint 30 fixture item is present. That
remains a stated uncertainty for the supervised evidence review.
