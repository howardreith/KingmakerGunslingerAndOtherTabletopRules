# PR #19: generic runtime lease handoff and completion

Reviewed baseline: `3794fb57bc4ba536d26fce1ca1bafb7bd6dbea23`.
Branch: `codex/magic-circle-alignment-spells`; draft [PR #19](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/19).
Validated source commit `1f1dcce6c100d298ddf673ca705514679d513b68` was published
through the mandated helper. All 1,885 frozen input paths/blobs match that
committed tree. Subsequent evidence metadata preserves those inputs.
The [curated evidence](PR19-LEASE-FOLLOWUP.json) records exact identities,
assertions, native runs, restoration and integration conflicts. Raw evidence
and packages remain local and uncommitted.

## Failure and correction

The reviewed generic launcher accepted a matching PASS result with
`ExitAfterCompletion=false`, then called `Exit-KmgRuntimeLease`. That function
treated the intentionally open Kingmaker process as a recovery failure, threw,
and retained `compatibility.lock`. Later normal game exit did not finish the
lease. The Circle recovery entry point rejected its generic `runtime <scenario>`
purpose. Timeout/interruption could leave the same unrecoverable generic state.

This was reproduced **before changing production code**, through the actual
generic entry point and reviewed coordination implementation. The result and
game process were controlled external-boundary fixtures; this is not claimed
as a real native PASS. Disposable evidence:
`artifacts/local-runtime/lease-tests-a6d76b8df70d4363a3f31ce8f3715490`.

The launcher now binds its immutable request and deployment receipt, identified
game PID/start time and collected outcome into its existing shared lease.
A successful permitted leave-open run becomes `CompletionPending` and returns
success, while retaining the lock throughout play. It prints a completion
command. Automatic-exit success completes normally. Child runs leave the
parent's settings/profile ownership and restoration debt untouched.

`scripts/Complete-KingmakerRuntimeLease.ps1 -RunId <recorded-run-id>` completes
either a handoff or an interrupted generic lease after the original owner and
game exit. It validates exact ownership, PID/start-time identities, immutable
receipt hashes, absence of all Kingmaker processes and an unchanged journal.
An exclusive recovery handle excludes other owners/recoveries. Checks repeat
under ownership; journal replacement requires its expected SHA-256. Foreign
changes remain intact. Completion changes neither native results nor settings,
profiles or deployments. It can resume after interruption between recording
completion and removing the lock. WhatIf neither mutates files nor launches.

There is no automatic lease theft, forced game termination or ownership release
while Kingmaker runs. Circle/profile obligations must use their own recovery
entry points. Older generic journals lacking the new bindings remain closed
to automatic completion and require inspection; no identities are invented.

## Production-path regression evidence

`scripts/Test-RuntimeLeaseLifecycle.ps1`: **39 assertions PASS**, seven generic
entry-point cases. Final disposable evidence:
`artifacts/local-runtime/lease-tests-88ef0e874bc844a4ad0dd52f3af481de`.

| Case | Observed behavior |
| --- | --- |
| Standalone leave-open PASS | Returns successfully; exact process/outcome retained; `CompletionPending` lock |
| Standalone timeout and interruption | Actual generic error evidence retained; safe guarded completion after normal exit |
| Automatic exit | PASS, `Completed`, lock removed |
| Inherited settings parent | Parent handle, journal and restoration debt unchanged; generic completion rejected |
| Conflicting ownership/recovery | Foreign lock, live owner, live/reused game PID and competing recovery rejected |
| Foreign changes and races | Changed request/journal preserved; game-start race rechecked after acquiring ownership |
| WhatIf | No launch or file mutation for launcher or completion |

Tests invoke the actual generic launcher and completion scripts. Debugger hooks
redirect fixed machine paths to disposable files and replace Steam/process
boundaries. Internal ownership, artifact verification, request/result parsing,
evidence collection, atomic writes and recovery are production implementations.
The test's fixture manifests are not qualification receipts for the real game.

Retained suites pass: Magic Circle request/preparation guards; twelve Circle
settings transaction cases; Steam launch (47 checks); runtime result and runner;
working-save exit/result race; compatibility filesystem transactions and profile
resolution. Full clean domain coverage retains R1 and C1 regressions.

Three broader scripts retain pre-existing stale expectations: RuntimeRequest
hard-codes `0.0.87`; RuntimeDeployment expects the old inline 114 artifact pin;
RuntimeScenarioPreflight has the existing `documented-scenarios-retained`
failure. An initial concurrent preflight also observed disposable artifact
churn; rerunning it alone removed that interference. No failing assertion was
removed or weakened. These are separate from the new 39 passing regressions.

## Required gates and applicable native checks

Repository validation: **PASS**. Complete clean domain suite: **1,660 tests,
zero failures**. Clean exact-reference Release compilation: **PASS**. Build
output, supply icons, SoundBank and strict installable-package gates: **PASS**.
Local combined build log: `artifacts/tests/pr19-p2-build.log`.

After disposable safety tests passed, Steam App ID **640820** ran these checks:

| Evidence directory under the lab runtime-evidence root | Result and scope |
| --- | --- |
| `20260920T1754074609310Z-mod-load-smoke` | PASS, 3 native assertions; exact new DLL loaded, automatic exit, standalone journal Completed and shared lock released |
| `20260920T1756315821384Z-working-save-magic-circle-absent` | PASS, 3 native assertions; correlated Working load, no fixture residue, no save writes; parent retained ownership through exit and exact settings restoration |

The second run used `KMG_AUTOMATION_WORKING`, physical slot
`Manual_299_KMG_AUTOMATION_WORKING.zks`, game ID
`dce769e0-229c-4bfd-b8ea-e2d572bf8472`, area `JamandisMansion`.
It did not prepare or clean a new fixture. Its original/override settings bytes
were equal; differing-byte restoration and foreign-edit behavior are covered by
the retained Circle transaction suite, not inferred from that native run.

The live game identified **2.1.7b**. The original eleven-mod stack was active;
versions and UMM log identity are recorded in JSON. Both native runs loaded the
DLL/hash/MVID below. The native checks do not qualify real leave-open or timeout
recovery; those lifecycle paths were exercised through disposable production
orchestration. Earlier feature-wide, R1, C1 and UI native results remain evidence
for their original binary, not this rebuilt candidate.

## Candidate identity and restoration

This correction changes **orchestration only**: C# sources, spell mechanics,
settings authority, artwork, spell lists and acquisition remain unchanged.
The required clean build embedded the current reviewed commit, which regenerated
the DLL's provenance. A ZIP comparison found 231 matching entry names and
**only the DLL differed**; all 230 other shipped entries are byte-identical.
No artwork or Unity asset was rebuilt.

- Version/assembly: `0.0.132` / `0.0.132.0`.
- Informational version: `0.0.132-icon-art-overhaul`.
- Embedded commit: `3794fb57bc4ba536d26fce1ca1bafb7bd6dbea23` (not the later committed script revision).
- Package: `artifacts/local-runtime/0.0.132/KingmakerGunslinger-0.0.132-local-runtime.zip`, 28,170,701 bytes.
- ZIP SHA-256: `3f02386bd4791b4ff7ddd45636369cacfc3c99bc6da8f88a89ca17321b7f7c95`.
- DLL SHA-256: `1a90163216b77029e3ff862506e6591e9969cde9ed24132b1cc9e9f0786b8d43`.
- DLL MVID: `ff443bc8-92dd-4288-a599-10d0bb957dcf`.
- Build source-state fingerprint: `0f745ef693e13b00e6877ddbab5ef3fde6d56a4d69d956906e5fe259781cdee2`.

The 1,885 frozen source/script/test/validation input mappings have aggregate
Git-filtered-blob SHA-256
`dec8828068866741fb085522ad50bc15cb7fdeec53ce17427228544b44e95fb8`
and raw-file manifest SHA-256
`cedf4d6f588d5c2af9c539f84fe541b1b039336372d68a20e0b65b13cb3944de`.
The local detailed manifest is `artifacts/magic-circle/pr19-p2-frozen-inputs.json`;
the curated JSON records exact changed script hashes. These distinguish tested
inputs from later evidence-only changes and from the embedded commit string.
The reviewed ZIP remains preserved under `artifacts/magic-circle/pr19-reviewed-3794/`.

Normal-play **0.0.117** was restored through `Restore-Live-Mod.ps1` under shared
ownership from `runtime-backups/live-mod/20260920T1754009353031Z`. All **136 files**
and exact settings bytes match that backup. The global firearm bank is unchanged.
Kingmaker is closed and the shared lock is absent. The candidate is not left
installed. Later installation uses the established UMM workflow while the game
is closed, with an explicit folder/settings backup and the exact ZIP hash.
Rollback restores that backup; uninstall safety is not claimed.

## Integration and retained limits

GitHub reports PR #19 **CONFLICTING / DIRTY** against master
`d7fe028c8fa60f85b5f7c58a38207088f17c7546` (published 0.0.133).
Read-only `git merge-tree` identified these integration points:

| File | Intended behavior on both sides |
| --- | --- |
| `scripts/RuntimeAutomation.Common.ps1` | Circle's immutable preparation binding/counts and Word of Recall Favored Class phase/plan validation |
| `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs` | Corresponding native request guards for both independent scenario families |
| `tools/validate_bodyguard90.py` | Circle schema 12 plus master's added 0.0.133 version entitlement |
| `tools/validate_icon_overhaul132.py` | Circle manifest/test/source-digest gates plus master's version-aware package selection |
| `validation/static-validation.json` | Circle's 1,660-test feature registry and master's reordered/updated validation metadata |

No integration changes were made. `AGENTS.md` forbids autonomous merges, and
this review explicitly withholds merge/history-rewrite authority. The next
integration action requires owner authorization and validation of the combined
result; taking either side wholesale would discard required behavior.

Accepted R1/C1/Circle-R2 fixes and approved paintings are preserved. No new UI
acceptance is claimed for this binary. Existing reduced-profile save limitations,
gamepad and uninstall limits remain. No retroactive control saves/suppression,
summoning barriers, inward circles, trapping diagrams or blanket descriptor
immunity were added. No unrelated Teleportation observer repair, merge, tag or
public release was performed.
