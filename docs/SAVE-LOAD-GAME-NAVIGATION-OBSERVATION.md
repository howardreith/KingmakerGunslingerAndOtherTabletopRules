# Load Game navigation observation

`observe-load-game-navigation` is a guarded, supervised, non-initiating probe
for the exact in-process action produced by one human click on **Load Game**.
It is the Phase 2 gate for UI-backed working-save automation; it does not
implement `working-save-smoke`.

Run only through Steam App ID 640820:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-load-game-navigation `
  -ManualInteractionRequired `
  -ExitAfterCompletion
```

After the readiness marker is validated, click **Load Game** exactly once.
Do not select or load any save. The probe sends no input and invokes no UI,
catalog, load, or save API.

The observer patches the exact two-argument
`Kingmaker.UI.SaveLoadWindow.ListOfSaves.Initialize(List<SaveInfo>, Boolean)`
catalog boundary plus narrowly selected in-process UI candidates. It records
the declaring type, exact signature, receiver type, argument types, managed
thread, ordered transition into the catalog boundary, and hook removal.

`PASS` requires one active non-`ListOfSaves` Load Game method when the catalog
boundary is entered. A catalog boundary without that correlation is
`AMBIGUOUS`; no boundary before the stage timeout is `TIMEOUT`. Result and
ordered trace files use the existing atomic evidence writer.

Thumbnail, screenshot, portrait, sprite, and texture completion is outside the
mechanical contract. These optional asynchronous UI assets are neither hooked
nor awaited and cannot determine the result.
