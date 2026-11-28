<!-- agent-review:v1 enrollment=kmg-codex-s7-master-integration-20261128082100-2218 key=r04-request part=1/1 sha256=a4291f249cc36857e324f0109c7b65f73800cba72e5877114e40b95f5fb89deb -->
### Claude Opus — review request — round 4 — candidate `785014815d`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-master-integration-20261128082100-2218-r04-d78a89` |
| Checkpoint | `final` (final) |
| Candidate H | `785014815d402c0472da62f34e231a85e18fde71` |
| Target B (`master`) | `52755b617c79328a972ae5b74bc05784422e332d` |
| Merge base M | `52755b617c79328a972ae5b74bc05784422e332d` |
| Mission | `s7-master-integration-mission` sha256 `5c514fdf4a` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 1, sha256 `f1de1c83e9acfa398274f61f2af78f0df5ce089821912e9ded16e5a28102d458` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 4 of 4 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Owner amendment 1 (revision 1, sha256 f1de1c83...d458) continues this enrollment after the accepted development checkpoint (reviewed 5460aff0a422196a1886980bc5972e8932ab50f2, archive 22143dcebccfd090d2cf207235c9b9b8bb049d45, review 1 of 4), adding packaged/native integration qualification. Package-producing commit 66a0b9c724e7a16e425b5e21ac962cd441eaced6 adds only the status-page entry (planning/); source, scripts, sessions, harness and tests equal 5460aff0a422196a1886980bc5972e8932ab50f2. One development candidate (purpose s7-integration-native, version 0.0.197): package 5824c44426570362c6f0192d4dda007f457a8d99413c7e91dbb6fe7233b3c4e8, packaged DLL = compiler DLL eb5f8f4c0302a5248ee523a313cdee2bd12c4ef873a4d82d88cf266346c41ca4, MVID fda26bf8-c9c7-4dcd-9ffb-7e5f8c77be8e, embedded commit 66a0b9c724e7a16e425b5e21ac962cd441eaced6; strict validation, receipt and a dry run of the sessions' admission subprocess agree. The retained C2 harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1 was reused after verifying the artifact, Info.json and provenance (built from d631d283aa; harness source unchanged since). Native sessions on owner-0.33.0 with that package, sequentially: 20261128T144025Z-owner-0.33.0-s7-c3-mechanics PASS (43/43, seven required cells admitted and completed, physical restoration separately true) and 20261128T144642Z-owner-0.33.0-s7-ghaele-lifecycle PASS (lifecycle verdict clean, late units reconciled, foreign controls preserved, terminal isolation restored). Both: in-process product identity equal to the admitted package, transaction completed, integrity verified, registry restored and read back, lease journal Completed, lock free. Documentation heads 9011aaddb210d8e2e25750622f127aa8a3a42843 (review 2), ca539a7d577afcf5aa36def10cf2e7f3372f2d8f (review 3) and 785014815d402c0472da62f34e231a85e18fde71 (this submission) change only docs/; the candidate stays current. Review 2 raised R2-F1..R2-F5 (records); review 3 verified R2-F2..R2-F4 and kept R2-F1 and R2-F5 open with new P3 R3-F1; all three are corrected in 785014815d402c0472da62f34e231a85e18fde71. The same package, harness and native sessions are reused. Final-head gates: sprint7-visual 25/25 and the pinned-loader case 1/1. Integration ledger, the same rows as the record's section 5 (outer times, wrapper milliseconds; upper bounds rounded up): D development stage 38.083 s (gate pairs on 28f2408e6f and 5460aff0a4; accepted, carried separately). Qualification: Q1 Build-Local 304.297 + Q2 dry admission 8.412 + Q3 mechanics 355.172 + Q4 Ghaele 127.887 + Q5 Ghaele -Report measured portion 0.340 + G2 gate pair on 9011aaddb2 10.305 + G3 gate pair on ca539a7d57 9.313 = 815.726 s measured through G3; plus B1 -PlanOnly <= 1.000 s and B2 mechanics -Report <= 0.379 s and <= 0.008 s wrapper truncation = at most 817.113 s through G3 (record figure, stated as at most 817.2 s). G4 gate pair on the submitted head 785014815d: 9.052 s (sprint7-visual 5.258 + pinned-loader 3.794), plus <= 0.002 s truncation. Qualification total: at most 826.167 s, stated as at most 826.2 s (13 min 46.2 s), plus U (the unmeasured process-exit tail of the Ghaele -Report after its last write). Cumulative integration: D 38.083 (+ <= 0.004 truncation) + qualification = at most 864.254 s, stated as at most 864.3 s (14 min 24.3 s), plus U. Each gate pair is counted once. Parent 71 min 40.3 s and C3 ledgers unchanged. R1-F1 and R1-F2: review 2 accepted their deferral (the controller records them resolved; nothing was marked fixed by the developer); the clarifications stay in the qualification record section 6 and R1-F1 on the status page; parent R9-F1 unchanged. master is still 52755b617c79328a972ae5b74bc05784422e332d. Scope limits: only the selected mechanics and lifecycle paths on this package; synthetic callback loss, constructed overlap and the labelled Ghaele notification injection keep their stated scope; no Dust defect reproduction or October Ghaele explanation is claimed. Durations quoted in the record outside section 5 (304.3 s, 8.4 s, 355.2 s, 127.9 s) are the section 5 rows Q1 to Q4 rounded to 0.1 s for display; the ledger and this report use the millisecond values.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-master-integration-20261128082100-2218 --gate focused -- -Filter sprint7-visual | yes | pass | 5.258 s outer (2026-11-28T15:23:31.322Z to 2026-11-28T15:23:36.581Z, wrapper and controller included); receipt duration_s 4.1 | Final clean submission 785014815d402c0472da62f34e231a85e18fde71: production compile against the pinned UMM 0.31.1 files, then 25 of 3218 cases, 25 PASS, 0 failures, 0 not applicable. Receipt gate-focused-785014815d40-d88e49a1250-a10f9d5aef1-20261128T152336524164Z.json (sha256 e8432610373506a102154461e91a5182d7eca22ac2538e2070d89f718c1aecb6), log gate-focused-785014815d40-d88e49a1250-a10f9d5aef1-20261128T152336524164Z.log. Not native qualification by itself; the native sessions are separate entries. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-master-integration-20261128082100-2218 --gate focused -- -Filter loader.production-dll-references-pinned-loader | yes | pass | 3.794 s outer (2026-11-28T15:23:36.648Z to 2026-11-28T15:23:40.442Z, wrapper and controller included); receipt duration_s 2.6 | Final clean submission 785014815d402c0472da62f34e231a85e18fde71: production compile against the pinned UMM 0.31.1 files, then 1 of 3218 cases, 1 PASS, 0 failures, 0 not applicable. Receipt gate-focused-785014815d40-d88e49a1250-a32501bde64-20261128T152340389407Z.json (sha256 5fc687da1cd4c525f5e9d4ff0d70451dcb6ff26df3f0555d317046e43f472490), log gate-focused-785014815d40-d88e49a1250-a32501bde64-20261128T152340389407Z.log. Not native qualification by itself; the native sessions are separate entries. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose s7-integration-native -DevelopmentTestFilter sprint7-visual -ReferenceBundleDir <private reference bundle used by C3> (on producing commit 66a0b9c724) | no | pass | 304.297 s outer (2026-11-28T14:34:12.111Z to 2026-11-28T14:39:16.390Z) | One development candidate (never a release). Stages: development repository wrapper, sprint7-visual 25/25, exact-reference release build (Build-Local's own loader.production-dll-references-pinned-loader 1/1 on the exact compiler output), package assembly, strict package validation, receipt. Package 5824c44426570362c6f0192d4dda007f457a8d99413c7e91dbb6fe7233b3c4e8, packaged DLL = compiler DLL eb5f8f4c0302a5248ee523a313cdee2bd12c4ef873a4d82d88cf266346c41ca4, MVID fda26bf8-c9c7-4dcd-9ffb-7e5f8c77be8e, embedded commit 66a0b9c724e7a16e425b5e21ac962cd441eaced6, source state 4ff837e1c02013df8753957929fc58a08b8767add99b8ba7c8ebb0e52305d3b6, development validator against v0.0.197 (7935 identities, 0 appended). Receipt sha256 60a2a581a4d311a484a649ab07fe78d29333647daeaf7157bc3e7a52844f3c0b. Log C:/Dev/KingmakerGunslingerLab/runtime-evidence/s7/recovery/integration-build-1.log. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LcCandidateAdmission.ps1 -PackagePath <the package> -DevelopmentPurpose s7-integration-native -AdmittedCopyDirectory <scratch> (dry admission) | no | pass | 8.412 s outer | The sessions' own admission subprocess run once beforehand: development mode, strict validation, material-input currency, private copy and checked extraction; identities equal the receipt. The scratch copy was removed. Log C:/Dev/KingmakerGunslingerLab/runtime-evidence/s7/recovery/integration-admission-dry.log. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SessionFile scripts/compatibility/loader-compat/sessions/s7-c3-mechanics.json -LoaderProfile owner-0.33.0 -CandidateZip <the package> -CandidateDevelopmentPurpose s7-integration-native -HarnessDir <retained C2 harness s7c2d> | no | pass | 355.172 s outer (2026-11-28T14:40:24.790Z to 2026-11-28T14:46:19.961Z) | Session 20261128T144025Z-owner-0.33.0-s7-c3-mechanics on owner-0.33.0 with package 5824c44426570362c6f0192d4dda007f457a8d99413c7e91dbb6fe7233b3c4e8 / DLL eb5f8f4c0302a5248ee523a313cdee2bd12c4ef873a4d82d88cf266346c41ca4 (in-process loaded module and MVID read back equal) and harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. 43/43 checks with the same predicates as C3's accepted 20261127T133020Z run; all seven declared required cells admitted and completed; physicalRestoration true with no restore errors (recorded separately); injected-failure controls recorded their failures, refused the next cell as declared and repaired after the guard with no unexpected/unsafe/residual items. Transaction e974e7fe44a54d149b0890808a99da3f completed, ownership completed, owner saves (95) and installation unchanged, registry restored and readback verified, lease journal Completed with recoveryRequired false and the shared lock free (read-only -Report). Sanitized extracts under docs/evidence/sprint7/integration-native/. Qualifies only the selected paths on this package. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SessionFile scripts/compatibility/loader-compat/sessions/s7-ghaele-lifecycle.json -LoaderProfile owner-0.33.0 -CandidateZip <the package> -CandidateDevelopmentPurpose s7-integration-native -HarnessDir <retained C2 harness s7c2d> | no | pass | 127.887 s outer (2026-11-28T14:46:42.087Z to 2026-11-28T14:48:49.957Z) | Session 20261128T144642Z-owner-0.33.0-s7-ghaele-lifecycle on owner-0.33.0 with package 5824c44426570362c6f0192d4dda007f457a8d99413c7e91dbb6fe7233b3c4e8 / DLL eb5f8f4c0302a5248ee523a313cdee2bd12c4ef873a4d82d88cf266346c41ca4 (in-process loaded module and MVID read back equal) and harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. harness steps all Done; lifecycle problems, unsettledAtEnd and unsettledAtFinalRelease empty; foreign Ghaeles preserved and unrequested; terminal step passed with no cleanup failures; compared with C2's accepted 20261126T033419Z row over the whole lifecycle block: the same 249 leaf fields, 222 identical values (every case verdict, boolean and count), and 27 run-specific differences (21 unit IDs, the five abandonedPendingLateResult order frames 4673 here vs 4771 in C2 with identical step order and witnessOwned values, and productVersion 0.0.197 vs 0.0.195). The real cast produced one owned Ghaele, destroyed once by disposal (earlySummonDestroyed and lateSummonsDestroyed 1 count the same unit); the labelled injected late result is a separate observation. Transaction a9e2e856d24e47d4958c3b893cfac96d completed, ownership completed, owner saves (95) and installation unchanged, registry restored and readback verified, lease journal Completed with recoveryRequired false and the shared lock free (read-only -Report). Sanitized extracts under docs/evidence/sprint7/integration-native/. Qualifies only the selected paths on this package. |
| SUPPORTING, ON REVIEW-2 HEAD 9011aaddb2 ONLY: verify --gate focused -- -Filter sprint7-visual | no | pass | 5.825 s outer (2026-11-28T14:53:48.338Z to 2026-11-28T14:53:54.164Z) | Receipt gate-focused-9011aaddb210-d88e49a1250-a10f9d5aef1-20261128T145354108525Z.json (sha256 bf1f250856e18581384270e1858305e7f3d4190c9635a84201e550742fe465e4), bound to 9011aaddb210d8e2e25750622f127aa8a3a42843, which differs from the submitted head only under docs/. Not relabelled as execution on 785014815d. |
| SUPPORTING, ON REVIEW-2 HEAD 9011aaddb2 ONLY: verify --gate focused -- -Filter loader.production-dll-references-pinned-loader | no | pass | 4.480 s outer (2026-11-28T14:53:54.238Z to 2026-11-28T14:53:58.716Z) | Receipt gate-focused-9011aaddb210-d88e49a1250-a32501bde64-20261128T145358667059Z.json (sha256 a7a95f2a0d22723d14f5eedb72b810af0d97baa038f2b4a2e60549cbfa96d040), bound to 9011aaddb210d8e2e25750622f127aa8a3a42843, which differs from the submitted head only under docs/. Not relabelled as execution on 785014815d. |
| SUPPORTING, ON REVIEW-3 HEAD ca539a7d57 ONLY: verify --gate focused -- -Filter sprint7-visual | no | pass | 5.435 s outer (2026-11-28T15:10:28.440Z to 2026-11-28T15:10:33.874Z) | Receipt gate-focused-ca539a7d577a-d88e49a1250-a10f9d5aef1-20261128T151033826192Z.json (sha256 d6e8423976d2d686c91ad5370ba0217bffea815e70c8bb7f9d7a50f081f70c68), bound to ca539a7d577afcf5aa36def10cf2e7f3372f2d8f, which differs from the submitted head only under docs/. Not relabelled as execution on 785014815d. |
| SUPPORTING, ON REVIEW-3 HEAD ca539a7d57 ONLY: verify --gate focused -- -Filter loader.production-dll-references-pinned-loader | no | pass | 3.878 s outer (2026-11-28T15:10:33.933Z to 2026-11-28T15:10:37.813Z) | Receipt gate-focused-ca539a7d577a-d88e49a1250-a32501bde64-20261128T151037763402Z.json (sha256 1503862fc556466dce3de381e9a37236c83d6d685af7f8b4887577678211bcbb), bound to ca539a7d577afcf5aa36def10cf2e7f3372f2d8f, which differs from the submitted head only under docs/. Not relabelled as execution on 785014815d. |
| Retained receipts on their own heads: focused sprint7-visual and loader.production-dll-references-pinned-loader on 5460aff0a4 and on 28f2408e6f (development checkpoint) | no | pass | 38.083 s outer total (ledger row D: accepted development stage, carried separately) | Evidence on those heads only; not relabelled as execution on 785014815d. |
| native-profile, loader-compat-probe, summon-playtest-r6 filters; other Sprint 7 sessions; other loader profiles; compatibility matrix; captures; listening; Trumpet-reload or Ghaele lifetime/visibility campaigns | no | not_run | 0 s | Not selected by owner amendment 1 and no changed dependency or correction revives the excluded filters (no source change since the accepted checkpoint; the producing commit adds only a planning entry). |

**Evidence references:**
- prepared under owner amendments revision 1 sha256 f1de1c83e9acfa398274f61f2af78f0df5ce089821912e9ded16e5a28102d458
- docs/evidence/sprint7/integration-native/development-receipt-66a0b9c724e7-projection.json and docs/evidence/sprint7/integration-native/lease-readbacks.json (review-2 corrections)
- docs/missions/sprint7/integration/S7-INTEGRATION-QUALIFICATION-RECORD.md (qualification record, dated pre-submission snapshot)
- docs/evidence/sprint7/integration-native/ (sanitized session and launch extracts of 20261128T144025Z-owner-0.33.0-s7-c3-mechanics and 20261128T144642Z-owner-0.33.0-s7-ghaele-lifecycle)
- Owner amendment 1 saved at C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-INTEGRATION-AMENDMENT-1-2026-11-28.md (sha256 9bf58a4b24e6245de663f8e31534712b150898b039900fb38a2c13c5863fea35)
- https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/22143dcebccfd090d2cf207235c9b9b8bb049d45/docs/agent-reviews/pr-231 (accepted development checkpoint archive)
- https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/5460aff0a422196a1886980bc5972e8932ab50f2/docs/missions/sprint7/integration/S7-MASTER-INTEGRATION-RECORD.md (accepted integration record, unchanged)
- Development receipt C:/Dev/KingmakerGunslingerLab/worktrees/s7-master-integration/artifacts/development-candidates/s7-integration-native/66a0b9c724e7/KingmakerGunslinger-0.0.197-development-s7-integration-native-66a0b9c724e7.zip.development-receipt.json (sha256 60a2a581a4d311a484a649ab07fe78d29333647daeaf7157bc3e7a52844f3c0b)
- Raw session evidence: C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261128T144025Z-owner-0.33.0-s7-c3-mechanics and C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261128T144642Z-owner-0.33.0-s7-ghaele-lifecycle (session-summary sha256 d9c5c43378844fa9c88bc51f4f4f19aedebc327bb438866c04679f95f1de56fb and 12d233bd2a790b4d8d8b1a337da010d458a1d343d099877ac4a2d2ebcc68ecf8)
- Coordination: C:/Dev/KingmakerGunslingerLab/runtime-evidence/s7/recovery/integration-a1-coordination.md; peer clearances before the build and release messages ff6146d7-f251-43f6-b78d-98078ca17380 and f04931d9-6111-401d-818a-56fe99ddc121 after the sessions

**Prior findings to assess:** `R2-F1`, `R2-F5`, `R3-F1`

**Owner amendments to the mission — revision 1, captured-source sha256 `f1de1c83e9acfa398274f61f2af78f0df5ce089821912e9ded16e5a28102d458`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

```text
# Owner amendments to the mission

Cumulative record kept by the agent-review controller. Each owner message below was applied by the owner through `resume --amend-mission` and is preserved verbatim, in order, between its labeled marker lines. Later amendments supersede conflicting earlier requirements; requirements they do not change still apply.

----- BEGIN OWNER AMENDMENT 1 (captured 2026-11-28T14:29:45Z; message sha256 9bf58a4b24e6245de663f8e31534712b150898b039900fb38a2c13c5863fea35) -----
I accept PR #231’s completed integration development checkpoint at:

- Reviewed H: `5460aff0a422196a1886980bc5972e8932ab50f2`
- Archive A: `22143dcebccfd090d2cf207235c9b9b8bb049d45`

Continue the same PR and enrollment through the following owner amendment. This adds packaged/native integration qualification to the accepted development checkpoint. It does not authorize merging into master or releasing.

### 1. Continue the existing enrollment

Use:

- PR #231
- Branch `codex/s7-master-integration`
- Enrollment `kmg-codex-s7-master-integration-20261128082100-2218`

Verify the current remote head, controller status, archive state and counters. The expected starting point is archive A above, AWAITING_OWNER_REVIEW, one completed review out of four, with no pending restoration or archive operation.

Save this instruction verbatim outside the worktree as:

`C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-INTEGRATION-AMENDMENT-1-2026-11-28.md`

Apply it through the supported `resume --amend-mission` procedure. Read all returned instruction parts and record the actual revision and digest.

This deliberately supersedes the current development-only handoff while preserving it as an accepted historical milestone. It does not erase or reset the completed review.

Keep the existing four-review ceiling; three rounds remain available. No `--max-rounds` change or replacement enrollment is authorized or needed.

Preserve #219 and all accepted child branches, archives, histories and findings. Preserve the three already archived #231 files byte for byte. They belong to this enrollment and should remain in its controller inventory; do not remove them to force an empty transcript diff.

### 2. Selected scope and accounting

I authorize this finite qualification sequence:

1. Prepare and build one fresh integration development candidate.
2. Verify its strict package and admission identities.
3. Run the existing `s7-c3-mechanics` session on owner UMM 0.33.0.
4. Run the existing `s7-ghaele-lifecycle` session on the same package.
5. Reconcile evidence, run the required final-head focused gates, obtain independent review, and finalize the same enrollment.

This explicitly supersedes the previous development checkpoint’s prohibition on packaging and native execution for these named operations.

For this selected qualification sequence, the earlier ordinary 30-minute development ceiling is replaced by the named workload above. Continue to estimate and record full outer elapsed time. Do not invent an additional candidate-count limit or lower review ceiling.

Carry the completed development-stage expenditure of 38.1 seconds separately and report cumulative integration expenditure. The parent’s 71 min 40.3 s ledger and C3’s ledger remain separate and unchanged.

This authority does not extend to a broad compatibility matrix, all ten Sprint 7 sessions, unrelated performance investigations, or repeated unchanged attempts. Every corrective rerun must have a concrete reason and a recorded change or resolved prerequisite.

Do not interrupt required safe completion or restoration to satisfy an estimate.

### 3. Freeze the producing inputs before building

Keep the accepted integration as the source baseline. Record the current master tip for comparison, but do not incorporate a later master advance during this qualification without a separate integration assessment. Report any drift accurately.

Complete necessary material preparation before freezing the package-producing commit:

- Any required source, script, session, harness or test correction.
- Any change outside `docs/`, including planning/status files.
- Any shipped document listed by `scripts/package-documents.json`.

Use a clean committed producing head and identify it explicitly.

The existing session definitions already derive the product version from the staged package. Do not edit them merely to replace a historical version or candidate description.

After the package is built, place qualification narratives and sanitized evidence under non-shipped `docs/` paths. Do not add a new timestamped evidence filename to a hardcoded test list merely to register the latest run; that would change material inputs after the build.

Use existing evaluation and reporting paths to inspect new results. If a material correction genuinely becomes necessary, preserve the failed evidence, make the correction coherently, and build the required new artifact. Never modify a receipt or relax admission to make an older package appear current.

### 4. Build and strictly admit the development package

Use one declared development purpose, such as `s7-integration-native`, consistently for the build and both sessions.

Use the existing development route:

`.\scripts\Build-Local.ps1 -DevelopmentPurpose s7-integration-native -DevelopmentTestFilter sprint7-visual`

Inspect the actual nested work before starting. Preserve the normal repository validation, exact-reference compilation, selected tests, strict package validation, and receipt generation.

Build-Local already checks `loader.production-dll-references-pinned-loader` against its exact compiler output. Do not add a redundant product build to perform that same check.

Record at least:

- Producing commit and source-state identity.
- Development purpose and package path.
- Package SHA-256.
- Packaged DLL SHA-256 and MVID.
- Embedded producing commit.
- Reference identities.
- Development receipt identity and validation outcome.

Verify that the finished package, packaged DLL, compiler output and receipt agree under the current #220 contracts.

Reuse the successful unchanged artifact for both sessions. Do not rebuild to stamp a later documentation head. Preserve failed partial outputs through the supported build procedure; do not overwrite a completed immutable candidate.

This remains a development package at the current version. It is not a release candidate, and it must not use the historical-release admission route.

### 5. Reuse the accepted harness where verified

The accepted C2 harness source matches the integration harness source.

Prefer the retained accepted DLL with SHA-256:

`d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1`

Verify the actual artifact, accompanying harness files and provenance before reuse. Do not substitute whichever DLL happens to occupy a shared output directory.

If the retained artifact is unavailable or its provenance cannot be established, a necessary build from the verified integration harness source and approved references is authorized. Record the resulting identity separately and use that same verified harness for the selected sessions. Do not claim that a rebuilt artifact has the old hash without measuring it.

### 6. Run the two selected native sessions

Use the repository-owned loader-compat runner, the disposable game copy, the existing approved fixtures, and `-LoaderProfile owner-0.33.0`.

Supply the exact new package and matching `-CandidateDevelopmentPurpose`. Preserve the normal admission subprocess and its verified private copy/extraction.

Run sequentially:

- `scripts/compatibility/loader-compat/sessions/s7-c3-mechanics.json`
- `scripts/compatibility/loader-compat/sessions/s7-ghaele-lifecycle.json`

Coordinate through the existing machine leases and peer rules. Respect running games, foreign locks, pending journals and the immutable baseline. Preserve fresh processes and per-session transactions.

For mechanics, establish that every declared required cell was admitted and completed. Record physical restoration separately. Inspect the callback, command, hook and owned-state results at the scope actually observed.

For Ghaele, inspect the actual lifecycle verdict, late-unit reconciliation, foreign-control preservation and terminal isolation. `continueOnFailure` permits evidence collection; it does not convert a failed step into a pass.

For each session, bind the in-process product identity to the admitted package and record:

- Session ID, loader profile, package and harness identities.
- Substantive scenario results and any expected failure controls.
- Actual residual-state observations.
- Transaction completion.
- Installation/save integrity verification.
- Registry restoration and verified readback.
- Final lock and journal disposition.

A successful scenario with incomplete physical restoration is not a completed qualification. If restoration is retained or requires intervention, preserve the state and follow the supported recovery procedure.

No new frame capture, audio capture, listening campaign, Trumpet-reload campaign, Ghaele lifetime/visibility campaign, or additional loader profile is selected here.

### 7. Correct only demonstrated qualification problems

Handle ordinary invocation issues and narrowly scoped integration/admission-binding defects within this task, while preserving the security and identity contracts.

For a real correction, establish the cause, add or run the relevant behavioral check, and rerun the affected scenario and necessary neighbors. Do not rerun an unchanged failed campaign in the hope of a different result.

Do not weaken package validation, source-state checks, ownership, fixture protection, or restoration to obtain a pass. Changes to gameplay intent, accepted visual assets, provisional cue choices, or the fundamental admission/ownership contract require a separate decision.

Complete the named qualification and your own evidence reconciliation before requesting formal review. Do not submit a known-incomplete package merely to have the reviewer enumerate already known missing work.

### 8. Final evidence, gates and review

Prepare one concise qualification record under a non-shipped `docs/` location, with immutable references to the accepted development checkpoint.

Distinguish:

- Package-producing commit.
- Documentation/evidence head.
- Reviewed head.
- Archive head.
- Package, DLL and harness identities.
- Both native sessions and their actual results.

Carry forward the precise P3 clarifications:

- The parent source was merged into `codex/s7-master-integration`, which started at master `52755b617c`; master itself has not received PR #231.
- Historical C2 Ghaele evidence is session `20261126T033419Z` on parent candidate 3, source `9c39b1ff7f`, package `49f8b88c…6413`, harness `d77754ba…b4f1`. It is distinct from recovery candidate 3.
- Parent R9-F1 remains the accepted non-blocking clarification already quoted in the integration record.

Do not manually mark those controller findings fixed or closed.

After all intended records changes are committed, run the existing required controller selections on the final clean submission:

- `sprint7-visual`
- `loader.production-dll-references-pinned-loader`

Retain other receipts on their actual heads. Record final gate timings in the developer report so another timing-only commit does not force another gate cycle.

Do not revive the previously excluded filter selections without an actual changed dependency or correction.

Request the next review in this same enrollment. Once ready, use normal finalization and verify the remote handoff. Preserve existing archived records and verify that the new archive adds only the controller’s own records.

### 9. Qualification limits and owner decisions

The new observations qualify only the selected mechanics and lifecycle paths on the new integration package.

Do not promote unexercised pending-command, uncertainty-recovery or ownership-transfer transitions to native PASS. Preserve the scope of constructed callback overlap and source-derived guarantees. Do not claim an ordinary-player Dust defect was reproduced or that the October Ghaele incident is explained.

Earlier presentation and audio evidence remains attached to its original producing candidates.

T2/T3/T6 visual acceptance and T5/T7 listening choices remain pending with me. Keep the provisional cues and existing private packet unchanged.

Do not merge into master, tag, release, change release versions, perform repository maintenance, or modify the human installation.

Finish with the exact package and session identities, observed qualification and restoration results, updated timing ledger, reviewed/archive heads, remote handoff state, and any concrete unresolved issue.
----- END OWNER AMENDMENT 1 -----
```

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
