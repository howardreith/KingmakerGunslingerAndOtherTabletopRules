<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r03-request part=1/1 sha256=2a1f9cd8e1224fbbc39bec85a7da27e44aa6f1630d44f6d2b145b3153252c9a8 -->
### Claude Opus — review request — round 3 — candidate `117e368bb4`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r03-16bb2d` |
| Checkpoint | `final` (final) |
| Candidate H | `117e368bb487472ae09edfbda7e1d3a1bf30d794` |
| Target B (`master`) | `35763430acc9eefc306b861045526a444690bc77` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `kingmaker-umm-0311-mission-20261123` sha256 `f7d990f396` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | none |
| Developer observed | foreground session `e178a93b-ca53-4b6b-a4d2-a357d48eaa77`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 3 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round-3 corrections for Tabletop Rules Expansion PR #220 (this repository only). Code 92539bb24a and 91139e4368 (91139e4368 only restores Build-Local's loose-output receipt lines, which an existing text check requires, beside the new compiler/packaged fields); 0440edf673 changes only documentation; 117e368bb4 adds only docs/evidence/umm-0311/. R1-F3: Build-Local measures the exact compiler output in its private folder and requires the DLL inside the finished ZIP and the loose copy to equal it (and carry the producer's GitCommit stamp) before either receipt is written; receipts record compiler and packaged identities and the stamp; admission refuses a packaged DLL whose stamp is not the receipt's producer and a receipt whose compiler identity differs. R1-F4: every file outside docs/ is a material input (shipped files and every build/packaging/validation helper); docs/-only changes keep a candidate admissible. R1-F2: validate-package resolves the path once and extracts exactly the entries it checks (normalized destinations under the mod root, no ./.. segment, no duplicate, nothing at the extraction root). R2-F6: string minimum required. R2-F2: historical release controls admitted only by the committed record scripts/historical-release-records.json (package digest, tag commit, DLL digest/MVID/stamp), mutually exclusive with development admission. R2-F3: Deploy-Local and the loader harness stage from a private copy with the admitted digest and require the extracted DLL to be the admitted DLL. R2-F4: loader admission runs Invoke-LcCandidateAdmission.ps1 with bound parameters after purpose validation. R2-F5: harness prerequisites checked first. R2-F1: test-domain.ps1 binds the production DLL build.ps1 or the gate compiled, or compiles one when the loader case is selected (complete suite); build.ps1 runs domain tests after its production compile. R2-F8: expected identities come from the pin reader. R1-F8: loader sets judged by bytes on controlled directories. R1-F10: packaging routes documented; pin diagnostic distinguishes absent and mismatched files. R2-F7: fixture ZIP disposed, clone removal failure surfaced. Development candidate at 91139e4368: package 376ce253..., DLL 1ef167cc... (MVID 7c7a1479...) = compiler output, stamped 91139e4368, four-loader audit RESOLVED. Native NOT RUN (unchanged reasons). No release.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 --gate focused -- -Filter loader.production-dll-references-pinned-loader | yes | pass | 2.8s | Controller receipt on 117e368bb4 (log gate-focused-117e368bb487-...-20261124T080717089675Z.log): production compiled, selected 1 of 3189, PASS loader.production-dll-references-pinned-loader, Completed 1 tests; failures=0. |
| scripts/test-domain.ps1 -Configuration Release -SkipRepositoryValidation, unfiltered, KMG_TEST_FILTER and KMG_PRODUCTION_* unset, at 91139e4368 | no | pass | ~3 min | docs/evidence/umm-0311/complete-suite-unfiltered.txt: production compiled by test-domain, PASS loader.production-dll-references-pinned-loader, Completed 3189 tests; failures=0 (the worktree was given the ignored IL inspection artifact the bodyguard/whiteout native-contract cases read). |
| scripts/Test-CandidateAdmission.ps1 (temporary clone of 91139e4368) | no | pass | ~16 min | docs/evidence/umm-0311/candidate-admission.json 29/29. |
| scripts/Test-PinnedLoaderMsBuild.ps1 at 92539bb24a | no | pass | ~6 min | docs/evidence/umm-0311/pinned-loader-msbuild.json 12/12 (91139e4368 changes only Build-Local receipt fields). |
| Build-Local -DevelopmentPurpose umm-0311-baseline at 91139e4368; validate-package -PassThru; Read-KmgBuildLocalManifest admission; LoaderApiAudit | no | pass | ~8 min build | development-receipt.json, development-validation.json, development-selected-tests.txt, package-validation.json, api-audit.json. The candidate was produced by a quiet-window loop whose console log was overwritten; its identity was verified independently (standalone validation, development admission with current material inputs, audit). |
| Test-LoaderReferencePins.ps1, Test-DevelopmentGate.ps1, Invoke-LoaderCompatSession.ps1 -SafetyTests, local focused gate, mismatched-binding run, pin diagnostics at 0440edf673 | no | pass | ~4 min | loader-reference-pins.txt 35/35; development-gate.txt 13/13; loader-compat-safety.txt 102/102 (44 of 44 required scenarios); mismatched-binding-refusal.txt; pin-diagnostics.txt. |
| Native loader rows and combined stacks | no | not_run | 0 | Unchanged: the disposable loader-compat game copy is in active use by enrollment kmg-codex-s7-visual-audio-remediation; no reviewed umm-0.33.4 profile. |

**Evidence references:**
- docs/evidence/umm-0311/INDEX.md
- docs/evidence/umm-0311/candidate-admission.json
- docs/evidence/umm-0311/pinned-loader-msbuild.json
- docs/evidence/umm-0311/complete-suite-unfiltered.txt
- docs/evidence/umm-0311/development-receipt.json
- docs/evidence/umm-0311/pin-diagnostics.txt
- docs/UMM-LOADER-COMPATIBILITY.md

**Prior findings to assess:** `R1-F10`, `R1-F2`, `R1-F3`, `R1-F4`, `R1-F8`, `R2-F1`, `R2-F2`, `R2-F3`, `R2-F4`, `R2-F5`, `R2-F6`, `R2-F7`, `R2-F8`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
