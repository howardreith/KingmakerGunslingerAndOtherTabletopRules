# Windows 10 autonomous runtime testing

## Design status

This document defines the guarded, source-controlled runtime-test channel
introduced for Kingmaker Gunslinger 0.0.30 and retained by current candidates.
It does not qualify any scenario in game. Source qualification and an actual
Kingmaker run are separate gates.

The autonomous save-backed route is `working-save-smoke`, documented in
`WORKING-SAVE-SMOKE.md`. It uses the guarded request, Steam App ID 640820, the
observed normal Unity Load Game action, and the exact captured catalog object.
No human interaction is required after the command starts.

The earliest safe activation point is the end of `Main.Load`, after the mod
context is published, Harmony patches are installed, any pending blueprint
dictionary observation is processed, bootstrap failure has been checked, and
the development UI is attached. A valid request is then attached to Unity Mod
Manager's `OnUpdate` callback. Scenario execution and optional
`Application.Quit()` therefore occur on the Unity main thread on a later frame,
not inside patch installation or blueprint loading.

## Explicit activation and normal-game isolation

Every real runtime scenario must be requested through
`C:\Program Files (x86)\Steam\steam.exe -applaunch 640820`. The runtime
orchestrator validates that exact App ID, refuses administrator elevation,
requires Steam and Kingmaker to run as the current Windows user, and never
falls back to launching `Kingmaker.exe` directly. If Steam is absent it starts
the client normally and waits only for its process; credential, recovery,
update, purchase, and cloud-conflict UI remain human stop conditions.

The only activation syntax is:

```text
-kmgRuntimeTestRequest "<absolute-request-json-path>"
```

The command-line parser examines only the value following that exact,
case-sensitive flag. It never logs the complete command line. Test mode remains
inactive unless the flag occurs exactly once, the value is an absolute file
path, the file exists, strict JSON parsing succeeds, every required field is
valid, `enabled` is exactly `true`, the run ID is nonempty and unique in its
evidence directory, the expected version equals the loaded `Info.json` version,
the evidence directory is a strict descendant of
`C:\Dev\KingmakerGunslingerLab\runtime-evidence`, and the scenario is in the
production allowlist.

No sentinel file, environment variable, mod setting, save state, GUI action, or
ordinary game launch can activate this mode. Invalid or absent requests do not
install the update callback and cannot request game exit. Rejections are
reported only to the existing structured mod log, with a reason code and safe
request-path basename; command-line values and request parameters are not
logged.

The resulting argument order is:

```text
steam.exe -applaunch 640820 -kmgRuntimeTestRequest "<absolute-request-json-path>"
```

The request path must remain strictly beneath the fixed evidence root and be
safely quoted. Evidence records only this allowlisted structure, the approved
request path, Steam executable and App ID, Steam PID, Kingmaker PID and start
time, and whether the result's run ID proved guarded-request acceptance. It
does not record Steam account data, credentials, or unrelated command-line
arguments.

## Request schema

Schema version 1 is a JSON object with exactly these properties:

| Field | Type and constraint |
| --- | --- |
| `schemaVersion` | integer, exactly `1` |
| `enabled` | Boolean, exactly `true` |
| `runId` | 1-100 characters; ASCII letters, digits, dot, underscore, hyphen |
| `scenario` | allowlisted string |
| `expectedModVersion` | nonempty string, exact ordinal match to loaded UMM version |
| `evidenceDirectory` | absolute path strictly beneath the fixed lab evidence root |
| `timeoutSeconds` | integer from 5 through 1800 |
| `exitAfterCompletion` | Boolean |
| `parameters` | object; scenario-specific keys are separately allowlisted |

Unknown JSON members, duplicate JSON members, nulls, non-integral numbers,
unknown parameters, path traversal, reparse-point escapes where discoverable,
and reused run IDs are rejected. The request file is read once and is not a
control channel after activation.

## Result schema and evidence

The scenario writes `runtime-result.json` and `runtime-summary.txt` directly
beneath the request's evidence directory. Result schema version 1 contains:
`runId`, `scenario`, one of `PASS`, `FAIL`, `AMBIGUOUS`, `ERROR`, or `TIMEOUT`,
loaded mod version, runtime identity, embedded Git commit when available, safely
discoverable game version, UTC start/end timestamps, duration milliseconds,
assertions, diagnostics, warnings, exception summary, evidence paths, and flags
stating whether automatic exit was requested and initiated.

Each assertion records `name`, `expected`, `observed`, `status`, and `evidence`.
Files are serialized to a uniquely named temporary file in the destination,
flushed to stable storage, and atomically renamed to the final name. The final
result is written last, so its presence is the orchestration completion signal.
The summary uses the same atomic writer.

Request files, orchestration metadata, process information, mod logs supplied
explicitly to the collector, and optional evidence-only screenshots stay below:

```text
C:\Dev\KingmakerGunslingerLab\runtime-evidence\<unique-run>\
```

No saves, credentials, unrelated command-line arguments, or unrelated user
files are evidence.

## Lifecycle, timeout, cancellation, and exit

The runner is a single process-local state machine driven by UMM `OnUpdate`.
Only one validated request can run per process. It waits until `ModContext` is
ready and, for scenarios that require registered content, until
`BlueprintBootstrap.IsInitialized`. The smoke scenario requires no campaign.
The timeout uses monotonic elapsed time and includes lifecycle waiting.

Unhandled exceptions become `ERROR`; elapsed deadlines become `TIMEOUT`;
missing or contradictory observations become `AMBIGUOUS`. Completion and
timeout both detach the update callback. There is no asynchronous mutation and
no background access to Kingmaker objects.

When `exitAfterCompletion` is true, the runner first atomically commits and
flushes both evidence documents, records that exit will be initiated, then
calls Unity's clean `Application.Quit()` on the main thread. Invalid requests
and ordinary launches can never reach this call. The orchestrator never kills
Kingmaker unless a human explicitly supplies `-AllowForceTerminate`.

## Scenario isolation and assertions

`mod-load-smoke` is production-allowlisted. It runs without a campaign and
asserts that the actual UMM entry point completed, the published context and
patch state are ready, the loaded UMM version exactly matches the request, and
the runtime identity comes from the executing mod assembly and current process.
These are mechanical runtime assertions; a build or domain test cannot
substitute for them.

`icon-overhaul-visual-evidence` is a save-free, read-only supporting-visual
scenario. It resolves the live Rapid Reload children, native Weapon Focus
parameters, supported firearm items, all 30 Eastern items, and all 12 Elven
Branched Spear items from the loaded blueprint graph, then renders five exact
1920x1200 PNG layouts through a Unity `Camera` and `RenderTexture`. Its
structured exact-set/icon/count assertions are mechanical evidence. The PNGs
are deliberately labeled in-game live-sprite facsimiles: they support human
perceptual review but are not evidence that the scenario navigated native menus
or inventory UI.

`sprint30-runtime-selftest` is not production-allowlisted in this iteration.
The existing typed Reload, Overhaul, and Repair adapters require a concrete
`UnitDescriptor`, equipped `ItemEntityWeapon` instances, `Game.Instance.Player`
and shared inventory. No proven supported API was found for constructing and
disposing these objects without campaign state. Calling only dependency-free
transaction services would test production domain code but would not exercise
the Kingmaker adapters or prove native weapon isolation. The scenario must not
be faked. A later design may add a save-backed, supervised fixture after exact
save loading and cleanup contracts are proven.

`working-save-smoke` is allowlisted and qualified for unattended use with the
exact receiver-bound contract documented in `WORKING-SAVE-SMOKE.md`. Use only
the canonical command there, only `KMG_AUTOMATION_WORKING`, and only guarded
Steam App ID 640820 launches. No other save or loading boundary is authorized.
The immutable `KMG_AUTOMATION_BASELINE` must never be selected, loaded,
modified, overwritten, renamed, or deleted.

A direct `Kingmaker.exe` launch can omit the Steam-owned initialization needed
for DLC entitlement. It is not a valid save-backed qualification environment.
`DLC Required` appearing across known-good saves is therefore a
launch-environment failure, not evidence that any save should be changed.

A valid assertion is a direct observation made in the real Kingmaker process
against the executing production assembly and, where applicable, the actual
production adapter/service. Synthetic doubles may supply isolated state only
when they do not duplicate the behavior under test. Log text, a screenshot,
domain tests, or a successful build alone is not proof of gameplay behavior.

## Runtime qualification scope policy

Use exhaustive `2^N` enumeration only in fast source/domain tests where every
configuration can be evaluated without launching Kingmaker. The standard
cross-module game-launch coverage is the generic `2N + 2` boundary: all ON,
all OFF, each module ON alone, and each module OFF while all others are ON.
For Boolean module settings this covers every possible one-, two-, and
three-module value combination.

- Documentation-only changes require no game process.
- Icon-only changes require package/icon validation and focused all-ON asset
  loading.
- Model, material, grip, donor, or bundle-only changes require focused family
  visual contracts, Eastern Weapons ON/OFF, all modules ON, and the highest-risk
  combined compatibility profile.
- A single module's mechanics or selector publication requires its focused
  mechanics, relevant optional profiles, persistence when affected, and the
  current generic boundary matrix.
- Add a higher-order combined profile only for a concrete suspected interaction
  among four or more modules. Test the smallest relevant family.

Build, test, package, validate, back up, and deploy once per immutable source
commit. Reuse that exact installed artifact across matrix launches and verify
commit, version, package/DLL SHA-256, DLL MVID, and installed DLL hash. Resume
matrix evidence only when the immutable artifact and original-settings identity
still match. The runtime controller deliberately has no generic exhaustive
game-launch mode. External-mod configurations remain focused profiles rather
than matrix dimensions.

## Manual and visual gates

The guarded runner can prove load identity, bootstrap state, protocol behavior,
and later allowlisted mechanical state/resource/revision deltas. Humans still
control deployment authorization, Steam credentials and dialogs, update and
cloud-conflict decisions, exact disposable-save selection, unexpected game
state, and all visual presentation judgments. Screenshots are optional
supporting evidence only. Combat-log wording, ability icon/layout, animations,
and perceptual correctness remain manual visual checks.

`scripts\Capture-WindowEvidence.ps1` is an optional Windows 10 GDI/Win32
evidence helper. It accepts one explicit process ID, captures only that
process's main window with `GetWindowRect`, `GetWindowDC`, and `BitBlt`, and
writes a PNG beneath the active evidence directory. It performs no window
enumeration, OCR, input, or correctness assertion. Missing or uncapturable
windows return a warning object and never change the mechanical scenario
status. It uses only Windows and .NET Framework components.

On Windows 10, Computer Use is not a runtime-correctness mechanism for this
project because Kingmaker's surface cannot be captured reliably. It must not be
used to infer PASS or navigate an autonomous scenario.
# Supervised save catalog

Use `observe-save-catalog-and-selection` only with
`-ManualInteractionRequired`. The orchestrator validates two atomic markers
before printing each manual instruction and never sends keyboard or mouse input.
See `SAVE-CATALOG-OBSERVATION.md`.

Use `observe-save-catalog-provider` only with `-ManualInteractionRequired`.
After validating the atomic in-game ready marker, the orchestrator instructs
the human to open Load Game and explicitly forbids selection or loading. The
probe never invokes a provider or sends input. See
`SAVE-CATALOG-PROVIDER-OBSERVATION.md`.
