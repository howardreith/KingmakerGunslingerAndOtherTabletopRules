# Sprint 7 integration: packaged/native qualification record (owner amendment 1)

**Status: dated pre-submission snapshot, 2026-11-28, corrected for review 2 (R2-F1 to R2-F5; records only).** This page
records the selected qualification as executed. It does not predict a review outcome. Final-head gate receipts and timings are cited in the developer report, so that recording them
needs no further commit.

- **Mission:** [`S7-MASTER-INTEGRATION-MISSION.md`](S7-MASTER-INTEGRATION-MISSION.md), plus owner amendment 1. The
  amendment is controller amendments revision 1, SHA-256
  `f1de1c83e9acfa398274f61f2af78f0df5ce089821912e9ded16e5a28102d458`, and is saved at
  `C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-INTEGRATION-AMENDMENT-1-2026-11-28.md`.
- **Accepted development checkpoint, kept as a historical milestone:**
  - reviewed head [`5460aff0a4`](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/5460aff0a422196a1886980bc5972e8932ab50f2);
  - archive [`22143dcebc`](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/22143dcebccfd090d2cf207235c9b9b8bb049d45/docs/agent-reviews/pr-231),
    review 1 of 4;
  - its [integration record](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/5460aff0a422196a1886980bc5972e8932ab50f2/docs/missions/sprint7/integration/S7-MASTER-INTEGRATION-RECORD.md),
    which is unchanged.
  - The three archived `pr-231` files remain on the branch byte for byte.

## 1. Heads

| Role | Commit |
|---|---|
| `master` at the start and at this snapshot (no drift) | `52755b617c79328a972ae5b74bc05784422e332d` |
| Accepted development archive (starting branch head) | `22143dcebccfd090d2cf207235c9b9b8bb049d45` |
| **Package-producing commit** | `66a0b9c724e7a16e425b5e21ac962cd441eaced6`. It only adds the status-page entry for this amendment (`planning/`). Source, scripts, sessions, harness and tests equal the accepted reviewed head `5460aff0a4` |
| Documentation/evidence heads | `9011aaddb2` added this record and `docs/evidence/sprint7/integration-native/` (review 2). The correction commit for review 2 is the submitted head. Both change only `docs/`, so the candidate stays current |
| Reviewed head and archive head | named by the controller at review and finalization. They are not predicted here |

No material correction was needed, so there is one candidate. Nothing was changed outside `docs/` after the build, and
no shipped document listed in `scripts/package-documents.json` was changed.

## 2. Development candidate

Built once by `Build-Local.ps1 -DevelopmentPurpose s7-integration-native -DevelopmentTestFilter sprint7-visual
-ReferenceBundleDir <the private reference bundle C3 used>`.
- **Nested stages, from `-PlanOnly`:** development repository wrapper, selected domain tests, exact-reference release
  build, package assembly, strict package validation, development receipt.
- **Duration:** 304.3 s outer.
- **Peers:** both cleared a no-Kingmaker window first. No Kingmaker process was running at the start.

| Identity | Value |
|---|---|
| Purpose | `s7-integration-native` (development; `releaseQualification` false; version 0.0.197, unchanged) |
| Package | `artifacts/development-candidates/s7-integration-native/66a0b9c724e7/KingmakerGunslinger-0.0.197-development-s7-integration-native-66a0b9c724e7.zip` |
| Package SHA-256 | `5824c44426570362c6f0192d4dda007f457a8d99413c7e91dbb6fe7233b3c4e8` |
| Packaged DLL SHA-256 / MVID | `eb5f8f4c0302a5248ee523a313cdee2bd12c4ef873a4d82d88cf266346c41ca4` / `fda26bf8-c9c7-4dcd-9ffb-7e5f8c77be8e` |
| Compiler output | the same SHA-256 and MVID (`exact-build/bin/KingmakerGunslinger.dll`, measured independently) |
| Embedded producing commit | `66a0b9c724e7a16e425b5e21ac962cd441eaced6` (`AssemblyMetadata` `GitCommit`) |
| Source-state SHA-256 | `4ff837e1c02013df8753957929fc58a08b8767add99b8ba7c8ebb0e52305d3b6` |
| References | 16 files in total: 14 game and Unity assemblies (role `installed-game`, from the private bundle and checked against the installation, for example `Assembly-CSharp.dll` `3b6450ff…5afb`), plus 2 pinned UMM loader files (role `pinned-loader`): `UnityModManager/UnityModManager.dll` 0.31.1.0 `2e038ff2…c2c4` and `UnityModManager/0Harmony12.dll` 1.2.0.1 `aa1cd483…1292`, with pins `b13bed40…8534`. Every role, relative filename and digest is listed in `docs/evidence/sprint7/integration-native/development-receipt-66a0b9c724e7-projection.json`, a sanitized projection of the receipt that also carries its package and compiler-DLL identity fields |
| Receipt | `…66a0b9c724e7.zip.development-receipt.json`, SHA-256 `60a2a581a4d311a484a649ab07fe78d29333647daeaf7157bc3e7a52844f3c0b` |
| Validation | development validator against `v0.0.197` (`329ad1b073`): 7,935 identities, none appended |
| Selected tests | `sprint7-visual` 25 of 3,218, 0 failures, none not applicable. Build-Local's own `loader.production-dll-references-pinned-loader` check on the exact compiler output: 1/1 PASS |

Agreement under #220:
- The package hash was measured independently.
- The one DLL entry in the archive, under the single `KingmakerGunslinger/` root (458 entries), hashes to the receipt's
  packaged and compiler SHA-256.
- `Info.json` declares 0.0.197 with `ManagerVersion` 0.31.1.
- **Dry admission.** The session's own admission subprocess (`Invoke-LcCandidateAdmission.ps1 -DevelopmentPurpose
  s7-integration-native`, 8.4 s) admitted the package in development mode with the same package, DLL, MVID, producer and
  version identities. It ran strict validation and the material-input currency check. Its private copy was then removed.
- The historical-release route was not used.

## 3. Harness

The retained accepted C2 harness was reused:
- folder `worktrees/s7-recovery-c2-ghaele-late-units/artifacts/loader-compat-harness-s7c2d/`;
- `KmgLoaderCompatHarness.dll`, measured at `d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1`;
- `Info.json`, `f153ae07…051e`.

Provenance:
- The recovery manifest records the harness as built from `d631d283aa`.
- The C3 sessions staged it from this same folder.
- `tools/LoaderCompatHarness/` and `Build-LoaderCompatHarness.ps1` are unchanged from `d631d283aa` to the producing commit.

In both sessions, the loader read the loaded harness assembly back at that SHA-256 (MVID `37571d33…b629`). Nothing was
rebuilt.

## 4. Native sessions

Both sessions used the repository runner, the disposable game copy and `-LoaderProfile owner-0.33.0` (UMM 0.33.0.0
`63e5baf7…2691`). They used the exact package above with `-CandidateDevelopmentPurpose s7-integration-native` and the
harness above, and ran in sequence. Each ran its own admission subprocess and its own transaction. Extracts with lab paths
removed are in `docs/evidence/sprint7/integration-native/`: the session and launch extracts, the receipt projection, and
`lease-readbacks.json`. The last holds each lease readback's report identity (output file SHA-256 and creation and
last-write times), session-summary SHA-256, status, `recoveryRequired`, `sharedLockHeldBy`, registry restoration and
integrity.

| | `20261128T144025Z-owner-0.33.0-s7-c3-mechanics` | `20261128T144642Z-owner-0.33.0-s7-ghaele-lifecycle` |
|---|---|---|
| Result | PASS: 43 of 43 checks, the same predicates as C3's accepted `…133020Z` run | PASS: harness steps `kmgModules[all-on]`, `kmgInitialization[create]`, `kmgGhaeleLifetime[lifecycle]` all Done |
| In-process product | loaded module `eb5f8f4c…1ca4`, MVID `fda26bf8…`, git `66a0b9c724`, 0.0.197 (`runtime-loaded-build-identity.json`) | the loader's mod entry reads the same SHA-256, MVID and version back |
| Admission (session record) | development, producer `66a0b9c724`, package `5824c444…`, DLL `eb5f8f4c…` | the same |
| Transaction | lease `runtime-20261128T144034Z-f8a4…`, transaction `e974e7fe…`; ownership completed | lease `runtime-20261128T144651Z-6638…`, transaction `a9e2e856…`; ownership completed |
| Integrity | verified: owner saves (95) and installation unchanged | the same |
| Registry | restored, and the readback was verified equal to the backup (`5ffd5922…6b38`) | the same |
| Lease readback (read-only `-Report`, taken at 2026-11-28T14:51:46Z, after both transactions had completed) | lease `runtime-20261128T144034Z-f8a48233baee456493332ffa05609c4b`: status Completed, `recoveryRequired` false, `sharedLockHeldBy` `<free>` | lease `runtime-20261128T144651Z-66384ddafdc54aec9bbc81cefbed1f0f`: the same three values |
| Outer duration | 355.2 s | 127.9 s |
| Extract SHA-256 | session `92c956c3…15aa`, launch `dc9d74fa…07da` | session `59e59207…a9bf2`, launch `f0c6b8ed…2098` |

**Mechanics (`summon-s7-effects-mechanics`).**
- **Required cells.** All seven declared cells were admitted and completed: `s7-ankylosaurus`, `s7-callback-abandoned`,
  `s7-callback-setup-failed`, `s7-trace-setup-failed`, `s7-dust`, `s7-dust-pre-delivery-disposed` and `s7-dust-abandoned`.
  `requiredCellsCompleted` is true. Isolation shows no failures and nothing pending or unreported.
- **Physical restoration** is recorded separately: `physicalRestoration` true, `restoreErrors` empty, and `fixture-restored`
  and `s7-effects-owned-state-restored` pass.
- **Callbacks and cues.**
  - Two resolved tail hits gave one `Sweetener_Low_06_Quake` cue each, ending naturally.
  - The miss and the interrupted attack gave no cue.
  - The production controls passed: foreign weapon, mismatched wrapper, replay, foreign owner and removed target.
  - On concurrent retirement, only the retired source's playing ID was stopped.
  - The callback-loss row is **synthetic**: the observer withheld one end-of-event callback. Its two-source overlap is
    **constructed**, by declared same-frame delivery, and is characterized under C1's evaluator, as accepted in parent
    amendment 2 §4.
- **Commands and hooks.**
  - `commandCleanupWithheld` is empty.
  - Retirements: 19 requested, 8 retired at teardown, no failures.
  - **Ordinary Dust cleanup only:** owned hooks went from 4 before disposal to 0 after, with 0 owned commands and 0 owned
    live executions left (`dust.ownedHooksAfterDispose`, `ownedCommandsLeft`, `ownedExecutionsLive`).
  - The foreign Sickened and the unowned execution were kept.
- **Temporary residuals during the Dust controls**, distinct from the final restoration:
  - **`s7-dust-pre-delivery-disposed`.** Right after the inner cell was disposed, `childAfterDisposal.pending` still named
    owned execution `KMG_Summoning_Special_DustMephit_Breath#-1371907456` and the owned-conditions reconciliation, with
    `restored` false. After resume, `childAfterResume.recoveries` records both as "observed settled", `restored` is true,
    and the declared next cell was admitted and completed. `afterSettlement` shows the execution delivered and ended, with
    1 condition acquired from it and 0 still present.
  - **`s7-dust-abandoned`.** Right after disposal, the injected failure of an owned Sickened removal left one owned
    condition (`Sickened#-1099260288`), and `childAfterDisposal.pending` named owned execution `#-449460224` and the
    owned-conditions reconciliation. After resume, `childAfterResume.recoveries` records both as observed settled. The
    child is still `restored` false with the expected failures, and it refused `s7-dust-abandoned-next` as declared. The
    repair after the guard then removed the leftover: `injectedLeftovers` 1, `leftoverGone` true; one live execution at
    cleanup, `executionsSettledBeforeReuse` true.
  - **Callback-loss survivor, playing ID 517.** At the window end (`survivorAtWindowEnd`) its playback was inactive while
    the product still tracked it (`tracked` true, one callback withheld). It stayed tracked until its own source was
    disposed. `survivorAfterDisposal` shows the view and playback component destroyed and a stop requested
    (`view-destroyed`, `stop-requested`, one attempt). `endUtc` is unset because a stop request is not completion, and no
    unrelated record changed.
  - **Final state.** After all of this, isolation is restored (`s7-effects-owned-state-restored`: no errors, 0 live
    recolours) and the fixture is restored (`fixture-restored`, party exact).
- **Expected failure controls.** These are the injected unpatch, rollback and condition-removal failures in the
  callback-abandoned, callback-setup-failed, trace-setup-failed and dust-abandoned cells. Each recorded its injected failure,
  refused the next cell as declared and was repaired after the guard. Hooks went back to 0 where measured. The unexpected,
  unsafe, missing-expectation, repair-failure and transferred-residual lists are empty, and isolation was restored. The
  dust pre-delivery cell admitted its next cell as declared.
- **Not covered.** These are observations of the selected cells only. Unexercised pending-command, uncertainty-recovery and
  ownership-transfer transitions are not native PASS. No ordinary-player Dust defect was reproduced.

**Ghaele lifecycle.**
- **Verdict.** `problems`, `unsettledAtEnd` and `unsettledAtFinalRelease` are empty, `pauseRestored` is true, and the
  terminal step passed with no cleanup failures and isolation restored.
- **Late units, two separate observations:**
  - **`abandonedAfterSpawn` (real cast).** The cast produced **one** owned Ghaele, seen before ordinary registration
    (`spawnSeenBeforeRegistration` true). The witness identified exactly that unit through the cast's own execution. Disposing
    the abandoned cast destroyed it once. `earlySummonDestroyed` true and `lateSummonsDestroyed` 1 describe the **same unit**:
    the counter is incremented by the held release (`OwnedCastRelease.Hold` in `GhaeleLifetimeProbes.cs`), which destroys
    the cast's own `Summoned` unit only when it was never registered.
  - **`abandonedPendingLateResult` (separate, injected).** A **controlled notification injection** (labelled, parent
    amendment 3 §4) delivered a late result for a real disposable unit, which had been registered with the probe's
    destruction owner before delivery. The witness reconciliation requested its destruction and destroyed it (verdict
    `late-owned-summon`, ordered). `lateSummonsDestroyed` is 0 here because the cast's own release had no summon to
    destroy.
  - **C2's accepted row (`20261126T033419Z`)** has the same outcomes in both cases. The frame values are run-specific: all
    five ordering steps ran at frame 4673 here and at frame 4771 in C2.
- **Foreign controls.** Foreign Ghaeles were preserved and unrequested, and bystander commands survived.
- **Borrowed and replaced grants.** They finished with their loans released. `borrowedMissing` was refused.
- **Comparison with C2.** Field by field, the lifecycle data matches C2's accepted `20261126T033419Z` row, apart from the
  product version (0.0.197 instead of 0.0.195) and newly created unit IDs.
- **Not covered.** No lifetime or visibility campaign was selected. The October Ghaele incident remains unexplained.

## 5. Ledger (integration only)

| Item | Outer time |
|---|---|
| Development stage (accepted, carried separately) | 38.1 s |
| Build-Local | 304.3 s (timed wrapper) |
| Dry admission | 8.4 s (timed) |
| `s7-c3-mechanics` | 355.2 s (timed wrapper) |
| `s7-ghaele-lifecycle` | 127.9 s (timed wrapper) |
| **Measured subtotal** | **795.8 s (13 min 15.8 s)** |
| `Build-Local -PlanOnly` inspection | at most 1.0 s. Its integer-second wrapper read 0 s, so it began and ended within one wall-clock second; its output file was created at 14:32:11.118Z and last written at 14:32:11.451Z |
| `-Report` for the mechanics session | at most 0.38 s: output file created 14:51:46.057Z, and the next command's output file was created at 14:51:46.436Z |
| `-Report` for the Ghaele session | 0.34 s from output creation (14:51:46.436Z) to its last write (14:51:46.776Z). The process-exit tail after that write was **not measured** |
| **Qualification account before the final gates** | **at most 797.2 s (13 min 17.2 s), plus the unmeasured exit tail of the second report** |
| **Cumulative integration before the final gates** | **at most 835.3 s (13 min 55.3 s), plus that tail.** The developer report adds the final-head gate times |

The timestamps for these three short commands come from file-system creation and last-write times of their retained
output files.

The parent's 71 min 40.3 s ledger and C3's ledger are separate and unchanged.

## 6. Clarifications carried forward (controller findings stay open)

- **R1-F1.** The parent source was merged into `codex/s7-master-integration`, which started at `master` `52755b617c`.
  `master` itself has not received PR #231.
- **R1-F2.** The historical C2 Ghaele evidence is session `20261126T033419Z` on **parent candidate 3**: source
  `9c39b1ff7f`, package `49f8b88c…6413`, harness `d77754ba…b4f1`. It is distinct from C3's **recovery candidate 3**.
- **Parent R9-F1** remains the owner's accepted non-blocking clarification, quoted in the integration record §6.

## 7. Scope and owner decisions

- The new observations qualify only the selected mechanics and lifecycle paths on this integration package.
- Candidate 6 and every earlier presentation and audio observation stay on their producing candidates.
- No frame capture, audio capture, listening, Trumpet-reload campaign, Ghaele lifetime/visibility campaign or other loader
  profile was run.
- T2/T3/T6 visual acceptance and the T5/T7 listening choices remain pending with the owner. The provisional cues
  (`Sweetener_Low_06_Quake`, `InspireCourage`) and the private packet (`2b5f4610…2565`) are unchanged.
- Not done: merge, tag, release, version change, repository maintenance or any change to the human installation.
