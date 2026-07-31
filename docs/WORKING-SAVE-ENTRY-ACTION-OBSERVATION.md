# Working Save Entry Action Observation

`observe-working-save-entry-action` is a guarded, supervised, source-qualified
runtime observation. It exists only to identify the normal in-process Load
action bound to the unique `KMG_AUTOMATION_WORKING` catalog descriptor.

The request must contain exactly:

```json
{ "saveName": "KMG_AUTOMATION_WORKING" }
```

Missing, blank, baseline, and all other names fail request validation before
hooks are installed.

## Contract

The probe autonomously invokes only the previously proven main-menu Load Game
UnityEvent. It captures the exact `List<SaveInfo>` passed to
`ListOfSaves.Initialize`, resolves the working and baseline descriptors using
the established stable fields, and requires them to be distinct objects.

After catalog initialization, the probe finds active UI components holding the
exact working `SaveInfo` reference. It accepts exactly one such entry and
exactly one active, interactable child/owner Load action whose runtime delegate
target is that entry or directly holds the same descriptor reference. Visible
text is recorded only as supporting evidence and never establishes identity.

Only then is `runtime-ready.json` written. The orchestrator prints:

```text
CLICK LOAD ON KMG_AUTOMATION_WORKING ONCE NOW
DO NOT CLICK KMG_AUTOMATION_BASELINE
```

The human clicks once. Passive Harmony prefixes record the exact UnityEvent,
listener/delegate target and method, `MainMenu.LoadGame` receiver and exact
descriptor argument, downstream completion callback, stable post-load
fingerprint, and any native save-writing or migration entry. Prefixes have no
`ref`, `out`, result, coroutine, callback, or run-original parameters. The
probe never invokes the entry action and removes all request-scoped hooks when
the result is sealed.

## Supervised command

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-working-save-entry-action `
  -ExpectedVersion 0.0.30 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ManualInteractionRequired `
  -ExitAfterCompletion $true
```

Use `-WhatIf` for source-only orchestration validation. Every real run must
launch through Steam App ID 640820 and use the disposable working save only.
