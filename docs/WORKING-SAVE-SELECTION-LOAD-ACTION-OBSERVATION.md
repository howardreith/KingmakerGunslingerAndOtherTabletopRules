# Working Save Selection and Load Action Observation

`observe-working-save-selection-load-action` is a guarded, supervised,
non-initiating observation used only when the normal callable path from the
exact working `SaveSlot` to `MainMenu.LoadGame(SaveInfo)` is unresolved.

The observer opens the normal Load Game screen through the already qualified
main-menu action, captures the complete `List<SaveInfo>`, requires exactly one
`KMG_AUTOMATION_WORKING` descriptor, distinguishes
`KMG_AUTOMATION_BASELINE`, and resolves exactly one active component holding
the working descriptor by object reference. It then scopes passive hooks to
that slot, its `ListOfSaves` and `SaveLoadWindow` owners, and active,
interactable buttons beneath the owning window. It records selection/load
handlers, exact selected-save fields, the complete managed caller chain at
`MainMenu.LoadGame`, the exact load argument and receiver, the authoritative
after-load callback, two stable fingerprint samples, and save-writing
sentinels.

It never invokes a slot, selection handler, shared button, listener, delegate,
or `MainMenu.LoadGame`. Visible text is supporting evidence only. A baseline or
other descriptor entering `MainMenu.LoadGame` is `FAIL`; missing unique action,
selected-state, object-reference, callback, or fingerprint proof is
`AMBIGUOUS`.

Supervised command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-working-save-selection-load-action `
  -ExpectedVersion 0.0.30 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ManualInteractionRequired `
  -ExitAfterCompletion $true
```

Use `-WhatIf` for source-only validation. A real run must use Steam App ID
640820 and the disposable working save.
