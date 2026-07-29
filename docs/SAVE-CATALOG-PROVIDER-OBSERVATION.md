# Supervised save-catalog provider observation

`observe-save-catalog-provider` is a guarded, supervised, read-only runtime
probe. It determines which managed source supplies the complete
`List<SaveInfo>` to
`Kingmaker.UI.SaveLoadWindow.ListOfSaves.Initialize(List<SaveInfo>, Boolean)`.
It does not open the Load Game screen, invoke a catalog provider, select or load
a save, send input, or invoke a save-writing operation.

## Supervised procedure

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-save-catalog-provider `
  -ExpectedVersion 0.0.30 `
  -ObserverStartupTimeoutSeconds 180 `
  -CatalogTimeoutSeconds 180 `
  -ManualInteractionRequired
```

The orchestrator builds and validates the package, uses the existing
exactly-one-backup deployment boundary, launches only through Steam App ID
640820, and waits for a fresh, run-bound ready marker. Only after the in-game
hooks are active does it print:

```text
OPEN THE LOAD GAME SCREEN NOW
DO NOT SELECT OR LOAD A SAVE
```

Open the normal Load Game screen once. Do not select a row and do not activate
Load, Continue, or any save-management control. The observation completes as
soon as the displayed catalog and upstream correlation evidence are committed.

## Narrow instrumentation

The probe observes the exact two-argument `ListOfSaves.Initialize` overload.
The second pass starts from the observed one-argument
`ListOfSaves.Initialize(Boolean)` caller and reads only its direct managed call
dependencies. It combines those exact dependencies with the already observed
save-manager/save-load hierarchy, then retains only members whose return or
argument metadata is a `SaveInfo` collection, including compatible generic
collection interfaces. Load and save-writing methods are sentinels only.
Harmony prefixes and postfixes do not
replace arguments, results, control flow, or exceptions.

Evidence records process-local receiver and collection identities, descriptor
type and count, safe non-save-content entry fingerprints, a minimized managed
Kingmaker caller chain, immediate caller, relevant receiver field/property
metadata, candidate provider signatures, object-reference correlation, managed
thread identity, UI/lifecycle requirements, observed side effects, and
complete-versus-filtered classification.
Property getters are not invoked. Descriptor contents and raw saves are not
read.

## Results and evidence

`PASS` requires a non-empty list and a non-transform provider return correlated
by reference identity to that exact consumer object, game-thread callbacks, and
no load or write sentinel. `AMBIGUOUS` means the list arrived but its producer
could not be proven, multiple candidates remain, or only a filtered/sorted UI
producer was correlated. Candidate records state the missing proof. `FAIL`
means loading or writing was observed. `TIMEOUT` identifies
`observer-readiness` or `catalog-provider-observation`; `ERROR` records an
unexpected probe failure.

Evidence is written atomically and incrementally to `runtime-ready.json`,
`runtime-events.json`, `runtime-catalog-provider-captured.json`,
`runtime-summary.txt`, and `runtime-result.json`, together with standard
orchestration/package evidence. Hooks are removed before the final result is
written, and requested clean exit follows final evidence flushing.

## Limitations and recovery

This scenario observes one normal PC Load Game path. An async or coroutine
boundary may prevent object-return correlation and legitimately produce
`AMBIGUOUS`. It does not authorize autonomous provider invocation. If the wrong
mod version, unexpected Steam UI, save-management prompt, or ambiguous state
appears, do not interact further; preserve the evidence directory for review.
