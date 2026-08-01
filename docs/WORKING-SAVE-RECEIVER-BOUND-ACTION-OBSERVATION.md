# Working Save Receiver-Bound Action Observation

`observe-working-save-receiver-bound-action` is a guarded, supervised,
non-initiating observation. It establishes the receiver-bound normal path from
the exact `KMG_AUTOMATION_WORKING` UI slot to `MainMenu.LoadGame(SaveInfo)`; it
does not implement autonomous save loading.

The scenario uses the already qualified main-menu Load Game action to open the
catalog. It requires one exact working descriptor, one distinct baseline
descriptor, one active `SaveSlot` holding the working descriptor by object
reference, and one owning `SaveLoadWindow`. It then passively observes exactly
these proven game methods:

1. `Kingmaker.UI.SaveLoadWindow.SaveSlot.OnButtonSaveLoad():System.Void`
2. `Kingmaker.UI.SaveLoadWindow.SaveLoadWindow.HandleHardcodeMainMenuSaveLoad(Kingmaker.EntitySystem.Persistence.SaveInfo):System.Void`
3. `Kingmaker.MainMenu.LoadGame(Kingmaker.EntitySystem.Persistence.SaveInfo):System.Void`

Each method must be declared exactly by the named type, have the exact
parameters and return type, be non-abstract and non-generic, and have a managed
body. Contract failure produces structured `ERROR` evidence. The observation
prefixes do not modify arguments, results, or original execution, and hook
exceptions are contained. Exact save-write sentinels remain armed.

Readiness is written atomically only after the exact slot and window are
resolved. The marker stage is `working-receiver-bound-action-ready` and includes
the run, scenario, save name, version, process, slot identity, window identity,
and installed hook signatures. Only after validating that marker does the
orchestrator display:

```text
CLICK THE NORMAL LOAD ACTION FOR KMG_AUTOMATION_WORKING ONCE
DO NOT CLICK KMG_AUTOMATION_BASELINE
```

The human click must produce exactly one slot action, window handler, and load
entry, in that order, on the game thread. Both downstream `SaveInfo` arguments
must be the exact captured working object. The authoritative after-load callback
and two stable fingerprint samples must follow, no save-write sentinel may
fire, and all hooks must be removed.

Supervised command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-working-save-receiver-bound-action `
  -ExpectedVersion 0.0.30 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ManualInteractionRequired `
  -ExitAfterCompletion $true `
  -Confirm:$false
```

Use `-WhatIf` for source-only orchestration validation. A real run is permitted
only under the repository's guarded Steam App ID 640820 runtime procedure.
