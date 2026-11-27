<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r02-request part=1/1 sha256=9025a5cda0cd52f58ac6bc51e6abfa1dadff50a0fb1dfeea35d76147bc605857 -->
### Claude Opus — review request — round 2 — candidate `7328aaa2da`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r02-ae7b57` |
| Checkpoint | `final` (final) |
| Candidate H | `7328aaa2da6855e4c4554f3828f699d252c0c3da` |
| Target B (`master`) | `35763430acc9eefc306b861045526a444690bc77` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `kingmaker-umm-0311-mission-20261123` sha256 `f7d990f396` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | none |
| Developer observed | foreground session `e178a93b-ca53-4b6b-a4d2-a357d48eaa77`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 2 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round-2 corrections for Tabletop Rules Expansion PR #220 (this repository only). Code 06800140fc, 400d904513 and 04b9384ed1; 45b9b77097 and 8c7bff4a37 change only documentation; 7328aaa2da adds only docs/evidence/umm-0311/. Packaged DLL: validate-package.ps1 reads the AssemblyRef table of the DLL inside the ZIP (isolated reflection-only process) and requires exactly the pinned UnityModManager/0Harmony12 identities, and validates the complete archive entry set; -PassThru returns the measured identity. Admission: Deploy-Local and the loader harness preflight require the receipt's DLL SHA-256 and MVID to equal the DLL inside the ZIP and the candidate's material inputs to be unchanged since its producing commit (Assert-KmgCandidateMaterialInputsCurrent); historical releases need -HistoricalReleaseControl. Direct MSBuild checks Info.json ManagerVersion against the pins. Domain check loader.production-dll-references-pinned-loader inspects only the production DLL the focused gate just compiled (path and SHA-256 passed by Invoke-KmgGate) and refuses otherwise; while preparing evidence I found that Build-Local runs domain tests before its exact build, so an unbound check could read a DLL left by an earlier build; Build-Local candidates therefore select an unrelated test and their packaged DLL references are checked by validate-package after the build. Development candidate at 04b9384ed1: package 7275c768..., DLL 4cbc6625... (MVID bd268aa9...), references UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1, strict validation passed; four-loader static audit RESOLVED. Native NOT RUN with concrete reasons. No release.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 --gate focused -- -Filter loader.production-dll-references-pinned-loader | yes | pass | 3.9s | Controller receipt on 7328aaa2da (log gate-focused-7328aaa2da68-...-20261124T070613522602Z.log): production compiled (KingmakerGunslinger -> artifacts\bin\Release), selected 1 of 3189 registered tests, PASS loader.production-dll-references-pinned-loader, Completed 1 tests; failures=0. |
| scripts/Test-CandidateAdmission.ps1 (temporary clone of 04b9384ed1, real Build-Local development candidate) | no | pass | ~15 min | docs/evidence/umm-0311/candidate-admission.json 13/13. |
| scripts/Build-Local.ps1 -DevelopmentPurpose umm-0311-baseline -DevelopmentTestFilter model-d.ItemIdentityEconomicsEnchantmentsAndCraftingUnchanged at 04b9384ed1 | no | pass | 410s | docs/evidence/umm-0311/development-receipt.json, development-validation.json; package-validation.json (validate-package -PassThru). |
| fleet LoaderApiAudit on the frozen packaged DLL 4cbc6625 (profiles 0.31.1, 0.32.4, owner-0.33.0, 0.33.4) | no | pass | ~10s | docs/evidence/umm-0311/api-audit.json: RESOLVED on all four (23 types, 67 members). Static only. |
| scripts/Test-PinnedLoaderMsBuild.ps1 at 45b9b77097 | no | pass | ~6 min | docs/evidence/umm-0311/pinned-loader-msbuild.json 9/9 (MSBuild inputs unchanged in 04b9384ed1). |
| scripts/Test-LoaderReferencePins.ps1 at 45b9b77097; scripts/Test-DevelopmentGate.ps1 and a local focused gate on the tree committed as 04b9384ed1 | no | pass | ~3 min | loader-reference-pins.txt 35/35; development-gate.txt 13/13; focused-gate-loader-filter.txt. |
| Native loader rows (0.31.1, 0.32.4, 0.33.0, 0.33.4) and combined stacks | no | not_run | 0 | The disposable game copy game-copies\loader-compat was in active use by enrollment kmg-codex-s7-visual-audio-remediation (Kingmaker.exe from that copy observed repeatedly 2026-11-24); no reviewed umm-0.33.4 profile. Recorded in docs/UMM-LOADER-COMPATIBILITY.md. |

**Evidence references:**
- docs/evidence/umm-0311/INDEX.md
- docs/evidence/umm-0311/candidate-admission.json
- docs/evidence/umm-0311/pinned-loader-msbuild.json
- docs/evidence/umm-0311/api-audit.json
- docs/evidence/umm-0311/development-receipt.json
- docs/evidence/umm-0311/package-validation.json
- docs/UMM-LOADER-COMPATIBILITY.md
- docs/BUILDING.md

**Prior findings to assess:** `R1-F1`, `R1-F10`, `R1-F2`, `R1-F3`, `R1-F4`, `R1-F5`, `R1-F6`, `R1-F7`, `R1-F8`, `R1-F9`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
