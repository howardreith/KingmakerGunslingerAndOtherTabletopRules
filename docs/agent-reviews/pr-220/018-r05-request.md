<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r05-request part=1/1 sha256=c0d78c07faa84860422b44e72dbd7ac8dde852d0873d4f4292b2a22db350232f -->
### Claude Opus — review request — round 5 — candidate `a7d293a533`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r05-cdfd83` |
| Checkpoint | `final` (final) |
| Candidate H | `a7d293a5339ec00c3a2ccde27cee309cefb803b2` |
| Target B (`master`) | `35763430acc9eefc306b861045526a444690bc77` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `kingmaker-umm-0311-mission-20261123` sha256 `f7d990f396` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | none |
| Developer observed | foreground session `e178a93b-ca53-4b6b-a4d2-a357d48eaa77`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 5 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round-5 (final) corrections for Tabletop Rules Expansion PR #220 (this repository only), addressing the complete open ledger (R1-F3, R1-F4, R2-F3, R3-F3, R3-F5, R4-F1..F5 and the optional R4-F6) in one batch. Code 404c239092 (main batch) and 49be3fd0e2 (the new package-measurement case writes its ZIPs inside the clone's artifacts directory, which package.ps1 requires; found by the first admission run at 404c239092). Docs de430d0c85; evidence a7d293a533 (docs/evidence/umm-0311/ only; the review head).

R1-F4: staging admission requires the receipt's sourceStateSha256 to equal the fingerprint of an exactly clean checkout of its commit (Get-KmgCleanSourceStateFingerprint, the producer's own formula), in both receipt paths, so a package produced from modified inputs, even inputs restored afterwards, is refused; review notes under docs/ that are not shipped still keep a candidate current; the compatibility guide's candidate table no longer exempts docs/ as a whole. R1-F3: compilerDllSha256 and compilerDllMvid are each checked on their own when present; an earlier-format receipt without them is still admitted. R2-F3: Set-LcGameCopyMods prepares the complete replacement beside Mods (Mods.kmg-staging-<guid>), requires the extracted tree to equal the admitted archive file for file (path and SHA-256, Info.json and every non-DLL file), verifies it again after assembly, then swaps through Mods.kmg-previous-<guid>; a failure before the swap leaves Mods untouched and a failed swap restores it. R4-F1: a Build-Local receipt's branch may be empty (detached checkout) but the property must exist; record identity comes from one helper. R4-F6: historical records carry branch and sourceStateSha256 null, sourceStateAvailable false, and releaseTag. R4-F2: the runner has a NOT APPLICABLE outcome that is never printed or counted as PASS; an unfiltered unbound run and a Build-Local deferral report it, every focused selection (KMG_TEST_FILTER substring or --test-prefix) without a bound DLL fails, and test-domain.ps1 refuses a selected, non-deferred run whose DLL is missing. R4-F5: binding changes, private copy, digest check and runner share one try/finally in test-domain.ps1. R3-F5: the extractor's ancestor check covers directory records too. R4-F3: admission mode build-local / development / historical-release in the child result and the session summary; -Report projects it, an older summary reads 'not recorded'. R4-F4: package.ps1 -PassThruMeasurement returns {packagePath, packageSha256, dll}; the default output is still the path and no other file is written. R3-F3: development-build-console.txt is a complete-suite Build-Local trace through packaging. Development candidate at 49be3fd0e2: package 71347c12..., DLL 3507c755... (MVID ac6e8b46...) = compiler output, stamped 49be3fd0e2, four-loader audit RESOLVED. Native NOT RUN (unchanged reasons). No release.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 --gate focused -- -Filter loader.production-dll-references-pinned-loader | yes | pass | 3.8s | Controller receipt on a7d293a533: pinned loader references verified, production compiled, PASS loader.production-dll-references-pinned-loader (1 of 3189). Focused level: the complete suite, the selection semantics and the wrapper are recorded separately below. |
| scripts/test-domain.ps1 -Configuration Release -SkipRepositoryValidation, unfiltered, at 49be3fd0e2 | no | pass | ~3 min | docs/evidence/umm-0311/complete-suite-unfiltered.txt: 3189 tests, failures=0, loader case PASS on the DLL test-domain compiled and bound with its digest. |
| scripts/Test-LoaderCaseSelection.ps1 at 49be3fd0e2 | no | pass | ~3 min | docs/evidence/umm-0311/loader-case-selection.json (7/7 at 49be3fd0e2): bound wrapper PASS; unbound substring and --test-prefix selections FAIL; unfiltered direct run NOT APPLICABLE (exit 0, no PASS line); deferred NOT APPLICABLE; DLL removed during preparation refused; digest refusal restores seeded bindings and leaves no private copy. |
| scripts/Test-CandidateAdmission.ps1 (temporary detached clone of 49be3fd0e2, private reference bundle as in round 4) | no | pass | about 20 min (19:15-19:38 UTC together with the MSBuild suite) | docs/evidence/umm-0311/candidate-admission.json (59/59, temporary detached clone of 49be3fd0e2). Run in a quiet window: another enrollment's loader-compat Kingmaker.exe runs intermittently and the development wrapper's Sprint22 UI-entry check refuses while any game runs; that process was not touched. The first run at 404c239092 passed 57 cases and then stopped at the new package-measurement case (destination outside the clone's artifacts); corrected in 49be3fd0e2. |
| scripts/Test-PinnedLoaderMsBuild.ps1 at 49be3fd0e2 | no | pass | ~6 min | docs/evidence/umm-0311/pinned-loader-msbuild.json (15/15 at 49be3fd0e2). |
| Build-Local -DevelopmentPurpose umm-0311-baseline -DevelopmentCompleteSuite at 49be3fd0e2; validate-package -PassThru; LoaderApiAudit | no | pass | 502s build | development-receipt.json (compilerDllSha256 = packagedDllSha256 = dllSha256 3507c755..., embeddedGitCommit 49be3fd0e2, sourceStateSha256 the clean-checkout fingerprint), development-validation.json, development-selected-tests.txt (3189 of 3189, loader case NOT APPLICABLE: deferred), development-build-console.txt, package-validation.json, api-audit.json (four profiles RESOLVED, 23 types / 67 members). |
| Test-DevelopmentGate.ps1, Invoke-LoaderCompatSession.ps1 -SafetyTests, Test-LoaderReferencePins.ps1, local focused gate, mismatched-binding runs, pin diagnostics at 49be3fd0e2 | no | pass | ~6 min | development-gate.txt 13/13, loader-compat-safety.txt 102/102 (44 of 44 scenarios), loader-reference-pins.txt 35/35, focused-gate-loader-filter.txt, mismatched-binding-refusal.txt, pin-diagnostics.txt. |
| net8 probe of the loader case with the portable runner's project properties | no | pass | ~1 min | docs/evidence/umm-0311/portable-loader-case-probe.txt and its generator: unbound unfiltered NOT APPLICABLE, unbound focused REFUSED, bound REFUSED toward the Framework runner. The whole portable runner still does not compile because older test files outside this change use Newtonsoft.Json; not addressed here. |
| Native loader rows and combined stacks | no | not_run | 0 | Unchanged: the disposable loader-compat game copy is in use by another enrollment; no reviewed disposable route for this enrollment; the live installation is not used. |

**Evidence references:**
- docs/evidence/umm-0311/INDEX.md
- docs/evidence/umm-0311/candidate-admission.json
- docs/evidence/umm-0311/loader-case-selection.json
- docs/evidence/umm-0311/pinned-loader-msbuild.json
- docs/evidence/umm-0311/complete-suite-unfiltered.txt
- docs/evidence/umm-0311/development-build-console.txt
- docs/evidence/umm-0311/development-receipt.json
- docs/evidence/umm-0311/mismatched-binding-refusal.txt
- docs/evidence/umm-0311/portable-loader-case-probe.txt
- docs/UMM-LOADER-COMPATIBILITY.md

**Prior findings to assess:** `R1-F3`, `R1-F4`, `R2-F3`, `R3-F3`, `R3-F5`, `R4-F1`, `R4-F2`, `R4-F3`, `R4-F4`, `R4-F5`, `R4-F6`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
