# Load Game button-action observation

`observe-load-game-button-action` is the guarded, supervised, non-initiating
Phase 2 probe for the exact in-process UI action produced by one human click on
**Load Game**. It does not implement `working-save-smoke`.

Run only through Steam App ID 640820:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-load-game-button-action `
  -ExpectedVersion 0.0.30 `
  -ManualInteractionRequired `
  -ExitAfterCompletion $true
```

After the readiness marker is validated, click **Load Game** exactly once. Do
not select or load any save. The probe sends no input and invokes no UI,
catalog, load, or save API.

The observer patches only:

- `Kingmaker.UI.MainMenuUI.MainMenuButtons.OnButtonLoadGame():Void`, the exact
  normal handler recorded in the latest supervised caller chain; and
- `Kingmaker.UI.SaveLoadWindow.ListOfSaves.Initialize(List<SaveInfo>, Boolean)`,
  the normal complete-catalog boundary.

At handler entry it reads the active main-menu hierarchy and retains only
`UnityEngine.UI.Button` components whose Unity event listener targets the
observed `MainMenuButtons.OnButtonLoadGame` method. It records component type,
GameObject path, active/interactable state, sibling and component identities,
safe label/localization-related identities, persistent and runtime listener
identities, managed thread identity, and the ordered transition into catalog
initialization.

`PASS` requires exactly one handler invocation, exactly one matching active and
interactable button, all callbacks on the game thread, and a subsequent catalog
boundary. Zero or multiple candidates are `AMBIGUOUS`. A missing catalog
transition is `TIMEOUT`.

Prefixes and postfixes are observation-only: they do not change arguments,
suppress originals, alter return values, replace enumerators, block callbacks,
or wait for screenshots, portraits, thumbnails, sprites, or textures.
