# Autonomous Working-Save Smoke

`working-save-smoke` is the guarded, autonomous, non-mutating qualification
scenario for `KMG_AUTOMATION_WORKING`. It is limited to mod version `0.0.30`
and launches through Steam App ID 640820.

## Authoritative contracts

The implementation consumes these PASS observations:

- `20260729T0143436024338Z-observe-manual-save-load`
- `20260729T0208479601809Z-observe-save-catalog-and-selection`
- `20260729T0339107643887Z-observe-load-game-button-action`

The action is `UnityEngine.UI.Button` at
`!LIGHT_SETUP/SceneUICanvas/SideBar/Buttons/LoadGame`, sibling 2 of 7. Its
GameObject has exactly `CanvasRenderer`, `RectTransform`, and `Button`; it is
active, active in hierarchy, and interactable. The root is `!LIGHT_SETUP`.
The persistent listener names `OnButtonLoadGame` with a null serialized target;
the runtime listener targets
`Kingmaker.UI.MainMenuUI.MainMenuButtons.OnButtonLoadGame():Void`. One normal
`Button.onClick.Invoke()` must cause exactly one game-thread handler call.

The catalog boundary is
`Kingmaker.UI.SaveLoadWindow.ListOfSaves.Initialize(List<SaveInfo>, Boolean)`.
The complete supplied `List<Kingmaker.EntitySystem.Persistence.SaveInfo>` is
captured directly. The successful observation contained 47 descriptors.
Thumbnails, screenshots, portraits, textures, and sprites are irrelevant.

The working identity is the exact combination
`Name=KMG_AUTOMATION_WORKING`,
`FolderName/FileName=Manual_299_KMG_AUTOMATION_WORKING.zks`,
`GameName=Hedwirg`,
`GameId=dce769e0-229c-4bfd-b8ea-e2d572bf8472`, and
`Area=JamandisMansion`. The required distinct baseline is
`Name=KMG_AUTOMATION_BASELINE` with
`FolderName/FileName=Manual_298_KMG_AUTOMATION_BASELINE.zks`. Both share the
observed GameId, so GameId alone cannot identify a manual save.

The exact captured working `SaveInfo` reference is passed once to
`Kingmaker.MainMenu.LoadGame(SaveInfo):Void`. Observed downstream calls are
`Game.LoadGameFromMainMenu`, `Game.LoadGame`, and
`SaveManager.LoadRoutine(SaveInfo, false)`. PASS requires the after-load
callback and two stable samples with the expected GameId, party count 3,
`BlueprintArea`, `AreaPersistentState`, and `UnitReference` main-character
type.

## Safety, evidence, and results

The guarded request must explicitly contain
`saveName=KMG_AUTOMATION_WORKING`. Baseline, blank, missing, unknown,
non-unique, incomplete, or uncorrelated identity fails closed. The scenario
deliberately invokes no save-writing or migration method. Unexpected native
write activity is recorded and prevents PASS. It sends no UI input and performs
no gameplay action.

Results are `PASS`, `FAIL`, `AMBIGUOUS`, `ERROR`, or `TIMEOUT`. Timeout evidence
names the stage. Atomic structured evidence covers readiness, UI action,
catalog, descriptor resolution, ordered events, load lifecycle, fingerprint,
summary, and result. Hooks are removed before completion. Exit is requested
only after the result flush succeeds.

First supervised command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario working-save-smoke -ExpectedVersion 0.0.30 -SaveName KMG_AUTOMATION_WORKING -ExitAfterCompletion $true -Confirm
```

`-WhatIf` performs build/package validation but no backup, deployment, Steam
launch, or Kingmaker launch.

## Emergency recovery and limitations

For non-PASS or early exit, preserve evidence and do not retry with a different
save or direct executable. Restore only from the single backup recorded in the
deployment manifest using the documented restore script. Never modify saves.

These contracts are qualified only for the observed Kingmaker environment and
mod 0.0.30. The first autonomous run remains supervised. Source-only success is
not proof of in-game correctness.
## Confirmation boundary

The orchestrator owns one high-level confirmation boundary before its first
persistent mutation. `-Confirm` requests that confirmation and
`-Confirm:$false` suppresses it. After authorization, trusted build, staging,
validation, deployment, evidence, and process-owner operations run with nested
confirmation disabled for this command only. Direct calls to deployment and
restoration scripts retain their own confirmations.

PowerShell's **Yes to All** applies only to the command currently asking. It
cannot authorize separate nested commands, which is why the orchestrator does
not delegate confirmation to each internal operation. `-WhatIf` stops at the
top-level boundary: it performs no build writes, backup, deployment, evidence
creation, CIM operation, Steam launch, or Kingmaker launch.
