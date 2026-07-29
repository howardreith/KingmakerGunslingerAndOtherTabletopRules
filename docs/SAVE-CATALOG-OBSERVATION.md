# Supervised save-catalog and selection observation

`observe-save-catalog-and-selection` is a guarded, read-only runtime probe. It
observes the normal Load Game catalog and one human-selected load of
`KMG_AUTOMATION_WORKING`. It never opens the menu, selects a descriptor, invokes
a load, sends input, or accesses save files.

## Two supervised stages

Run the command documented below. After the atomic Stage A marker is validated,
the orchestrator prints:

```text
OPEN THE LOAD GAME SCREEN NOW
DO NOT SELECT A SAVE YET
```

Open the normal Load Game screen and wait. After the game's completed save list
is observed and the atomic Stage B marker is validated, it prints:

```text
SAVE CATALOG CAPTURED
SELECT AND LOAD KMG_AUTOMATION_WORKING NOW
DO NOT SELECT KMG_AUTOMATION_BASELINE
```

Only then select the displayed `KMG_AUTOMATION_WORKING` entry and use the normal
Load control once. Never select or load `KMG_AUTOMATION_BASELINE`.

## Evidence and privacy

Request-scoped hooks observe `SaveManager.UpdateSaveListAsync`,
`UpdateSaveListIfNeeded(Boolean)`, `UpdateSaveListTask()`, and the PC load-game
model population call
`Kingmaker.UI.SaveLoadWindow.ListOfSaves.Initialize(List<SaveInfo>, Boolean)`.
Stage B uses the `SaveInfo` list passed to that displayed model; it does not
infer completeness from the manager's private backing list. Selection and load
correlation use `MainMenu.LoadGame(SaveInfo)` and
`AddCallbackAfterLoad(Action)`. Narrowly named write/migration methods are
observed but never invoked by the probe. Evidence records exact signatures,
ordering, thread identity, collection and descriptor types, counts, correlation
method, completion, and a compact post-load fingerprint.

For working and baseline descriptors only, it records whitelisted public or
runtime-visible fields: `Name`, filename/folder leaf, `GameName`, `GameId`,
`Area`, `AreaNameOverride`, `GameSaveTime`, and `GameTotalTime`. Unrelated
descriptors contribute only the aggregate count and a SHA-256 hash of the same
minimized identity tuple. No raw save contents, full paths, object dumps, party
details, or unrelated metadata are recorded.

## Outcomes

- `PASS`: exactly one working descriptor exists, the baseline is
  distinguishable and unselected, selection correlates uniquely, completion
  and stable fingerprint are positive, version 0.0.30 is loaded, no write is
  observed, and hooks are removed.
- `FAIL`: working prerequisites are absent, baseline/another save is selected,
  or forbidden mutation is observed.
- `AMBIGUOUS`: catalog completeness, uniqueness, identity stability, selection
  correlation, or completion meaning cannot be proved.
- `TIMEOUT`: diagnostics name `catalog-observer-ready`, `catalog-capture`,
  `save-selection`, `load-completion`, or `post-load-fingerprint`.

A supervised PASS supplies runtime evidence for human review; it does not by
itself implement or activate autonomous loading. Autonomous resolution remains
blocked until the evidence proves a stable, supported descriptor-resolution
contract.

## Command

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-save-catalog-and-selection `
  -ExpectedVersion 0.0.30 `
  -ObserverStartupTimeoutSeconds 180 `
  -CatalogTimeoutSeconds 180 `
  -SelectionTimeoutSeconds 300 `
  -CompletionTimeoutSeconds 180 `
  -ManualInteractionRequired
```

If a marker is stale/mismatched, the wrong save appears, Steam presents an
unexpected dialog, or the result is unclear, stop. Preserve the evidence
directory; do not retry by modifying, copying, or replacing saves.
