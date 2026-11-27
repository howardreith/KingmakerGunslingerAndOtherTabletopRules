<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r04-request part=1/1 sha256=3ca4731ef99e2e83291dbb34e42ca7dad31c23145c653decaf8e805ff28e2b10 -->
### Claude Opus — review request — round 4 — candidate `7cf162822a`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r04-fdab42` |
| Checkpoint | `final` (final) |
| Candidate H | `7cf162822a8c6005e2d76563bef6c60d386b5e29` |
| Target B (`master`) | `35763430acc9eefc306b861045526a444690bc77` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `kingmaker-umm-0311-mission-20261123` sha256 `f7d990f396` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | none |
| Developer observed | foreground session `e178a93b-ca53-4b6b-a4d2-a357d48eaa77`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 4 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round-4 corrections for Tabletop Rules Expansion PR #220 (this repository only), addressing the complete open ledger (R1-F3, R1-F4, R1-F10, R2-F1, R2-F2, R2-F3, R3-F1..F5) in one batch. Code 2741e2b89f (main batch), 22370206e5 (staging admission verifies the receipt's producer claim; fixture corrections) and 4f196b26d1 (a compile that produced no DLL leaves no compile-time digest, needed by the development-gate fixture's stub compiler; the repository's reuse-contract text test now requires the receipt MVID from the captured compiler identity). Docs d5c4464c73 and 0ac0c63faf; evidence 864703a85f and 7cf162822a (docs/evidence/umm-0311/ only).

R1-F4: scripts/package-documents.json is the shipped-document list package.ps1 reads; candidate freshness treats every listed document as material, docs/ included, committed or not. R1-F3: receipts written from the measurements compared (compiler identity, package digest around the one strict validation, firearm digests captured once); admission checks packagedDllSha256 and embeddedGitCommit against the measured DLL; staging admission requires the DLL's source stamp to be the receipt's commit. R2-F2: the historical record is read from HEAD (a differing working-tree record is refused); the loader admission child and the session summary carry mode, version, producer commit and release tag. R3-F1: historical deployment expectations from the digest-bound package, record fields tag:<tag> / historical-release <tag>, candidateKind historical-release, all established before the target is replaced. R2-F3: Deploy-Local's under-lease admission must equal the first; the loader admission child extracts and verifies the private staging tree; staging checks the owner set and the tree before Mods is replaced and compares the staged DLL with the admitted digest; the session expects the admitted digest. R2-F1/R3-F2: the case is not applicable on an unbound runner unless selected by name; NET5_0_OR_GREATER excludes the reflection-only assertion; callers pass the compile-time digest with the path and test-domain inspects a private copy with that digest. R3-F3: Build-Local reuses package.ps1's strict validation and runs the loader case on its exact compiler output (no MSBuild production compile). R3-F4: Enter-LcSessionForAdmission removes the private copy on a preflight or lease refusal; the session finally removes it too; the admission child removes it if extraction fails. R3-F5: directory records carry no data, appear once and never share a destination with a file. R1-F10: missing vs mismatched and pinned cache vs external folder advice, with the scripts\build.ps1 -CompileOnly bootstrap; direct MSBuild reports a missing loader reference first. Development candidate at 4f196b26d1: package 928a3188..., DLL b1e07c2a... (MVID 0cfcbddc...) = compiler output, stamped 4f196b26d1, four-loader audit RESOLVED. Native NOT RUN (unchanged reasons). No release.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 --gate focused -- -Filter loader.production-dll-references-pinned-loader | yes | pass | 3.1s | Controller receipt on 7cf162822a: pinned loader references verified, production compiled, PASS loader.production-dll-references-pinned-loader (1 of 3189). Focused level: the complete suite and wrapper are recorded separately below. |
| scripts/test-domain.ps1 -Configuration Release -SkipRepositoryValidation, unfiltered, at 4f196b26d1 | no | pass | ~3 min | docs/evidence/umm-0311/complete-suite-unfiltered.txt: 3189 tests, failures=0, loader case PASS on the DLL test-domain compiled and bound with its digest. The first attempt at 22370206e5 had 1 failure (repair-runtime.immutable-reuse, a text check requiring the old reread of the MVID), corrected in 4f196b26d1. |
| scripts/Test-CandidateAdmission.ps1 (temporary clone of 4f196b26d1, private reference bundle as in round 3) | no | pass | ~17 min | docs/evidence/umm-0311/candidate-admission.json (46/46, temporary clone of 4f196b26d1). Run in a quiet window: another enrollment's loader-compat Kingmaker.exe was running earlier and the development wrapper's Sprint22 UI-entry check refuses while any game runs; that process was not touched. |
| scripts/Test-PinnedLoaderMsBuild.ps1 at 4f196b26d1 | no | pass | ~6 min | docs/evidence/umm-0311/pinned-loader-msbuild.json (15/15 at 4f196b26d1). |
| Build-Local -DevelopmentPurpose umm-0311-baseline at 4f196b26d1; validate-package -PassThru; LoaderApiAudit | no | pass | 469s build | development-receipt.json (compilerDllSha256 = packagedDllSha256 = dllSha256 b1e07c2a..., embeddedGitCommit 4f196b26d1), development-validation.json, development-selected-tests.txt, package-validation.json, api-audit.json (four profiles RESOLVED, 23 types / 67 members). |
| Test-DevelopmentGate.ps1, Invoke-LoaderCompatSession.ps1 -SafetyTests, Test-LoaderReferencePins.ps1, local focused gate, mismatched-binding runs, pin diagnostics at 4f196b26d1 | no | pass | ~6 min | development-gate.txt 13/13, loader-compat-safety.txt 102/102 (44 of 44 scenarios), loader-reference-pins.txt 35/35, focused-gate-loader-filter.txt, mismatched-binding-refusal.txt, pin-diagnostics.txt. |
| net8 probe of the loader case with the portable runner's project properties | no | pass | ~1 min | docs/evidence/umm-0311/portable-loader-case-probe.txt and its generator. The whole portable runner does not compile at this commit because older test files outside this change use Newtonsoft.Json, which its generated project does not reference; not addressed here. |
| Native loader rows and combined stacks | no | not_run | 0 | Unchanged: the disposable loader-compat game copy is in use by another enrollment; no reviewed disposable route for this enrollment; the live installation is not used. |

**Evidence references:**
- docs/evidence/umm-0311/INDEX.md
- docs/evidence/umm-0311/candidate-admission.json
- docs/evidence/umm-0311/pinned-loader-msbuild.json
- docs/evidence/umm-0311/complete-suite-unfiltered.txt
- docs/evidence/umm-0311/development-receipt.json
- docs/evidence/umm-0311/mismatched-binding-refusal.txt
- docs/evidence/umm-0311/portable-loader-case-probe.txt
- docs/evidence/umm-0311/pin-diagnostics.txt
- docs/UMM-LOADER-COMPATIBILITY.md
- docs/BUILDING.md

**Prior findings to assess:** `R1-F10`, `R1-F3`, `R1-F4`, `R2-F1`, `R2-F2`, `R2-F3`, `R3-F1`, `R3-F2`, `R3-F3`, `R3-F4`, `R3-F5`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
