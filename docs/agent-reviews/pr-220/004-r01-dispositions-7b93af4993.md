<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r01-dispositions-7b93af4993 part=1/1 sha256=7b93af4993f1ebcc29ca11599eb473bd09f946d26cf86658a7d92ee235f99af2 -->
### Claude Opus — finding dispositions — round 1

Candidate after corrections: `7328aaa2da6855e4c4554f3828f699d252c0c3da`

- **R1-F1** — `fixed` — commits: `06800140fc`, `400d904513`, `7328aaa2da`
  docs/evidence/umm-0311/candidate-admission.json cases validator-genuine-baseline-zip-accepted, validator-newer-compiled-tre-dll-refused-for-its-assemblyref (otherwise valid 0.31.1 ZIP containing the released v0.0.187 DLL, which references UnityModManager 0.33.0.0: 'not exactly the pinned loader assemblies'); docs/evidence/umm-0311/pinned-loader-msbuild.json case direct-msbuild-mismatched-source-manifest-refused (direct build refused for Info.json ManagerVersion).
- **R1-F2** — `fixed` — commits: `06800140fc`, `7328aaa2da`
  docs/evidence/umm-0311/candidate-admission.json cases validator-extra-runtime-dll-at-archive-root-refused ('outside the KingmakerGunslinger mod root: 0Harmony...'), validator-extra-runtime-dll-at-mod-tree-refused ('not match the strict release allowlist'), validator-genuine-baseline-zip-accepted.
- **R1-F3** — `fixed` — commits: `06800140fc`, `7328aaa2da`
  docs/evidence/umm-0311/candidate-admission.json cases matching-candidate-admitted, receipt-with-another-builds-mvid-refused and receipt-with-another-builds-dll-hash-refused ('not the build its receipt names', receipt values shown against the packaged DLL, refused before staging while the ZIP digest matches).
- **R1-F4** — `fixed` — commits: `06800140fc`, `400d904513`, `7328aaa2da`
  docs/evidence/umm-0311/candidate-admission.json cases candidate-after-project-change-refused and candidate-after-reference-helper-change-refused (version held constant: 'stale: material inputs changed since its producing commit 04b9384ed1: src/KingmakerGunslinger/... / scripts/ReferenceProvenance...'), candidate-admitted-again-after-reverts, loader-preflight-admits-matching-candidate, loader-preflight-refuses-mismatched-zip. Deploy-Local -HistoricalReleaseControl keeps released controls under their own identities.
- **R1-F5** — `fixed` — commits: `04b9384ed1`, `8c7bff4a37`, `7328aaa2da`
  docs/UMM-LOADER-COMPATIBILITY.md 'Current development candidate': producing commit 04b9384ed1, Build-Local command and 410 s duration, compiler loader references 0.31.1.0 2e038ff2 / 1.2.0.1 aa1cd483 (pins b13bed40), ZIP 7275c768..., DLL 4cbc6625..., MVID bd268aa9..., AssemblyRefs, standalone validation; docs/evidence/umm-0311/development-receipt.json, development-validation.json, package-validation.json.
- **R1-F6** — `fixed` — commits: `8c7bff4a37`, `7328aaa2da`
  docs/evidence/umm-0311/api-audit.json (frozen packaged DLL 4cbc6625; RESOLVED on 0.31.1, 0.32.4, owner-0.33.0, 0.33.4: 23 types, 67 members); loader-profiles.md (component identities, Harmony 2 per profile, provenance, newest stable 0.33.4 from the author's NuGet package dated 2026-11-19 / GitHub HEAD 3d14bf0d); loader-api-audit-Program.cs.txt and -csproj.txt with the invocation in docs/UMM-LOADER-COMPATIBILITY.md.
- **R1-F7** — `fixed` — commits: `8c7bff4a37`
  docs/UMM-LOADER-COMPATIBILITY.md 'Native rows for this candidate: NOT RUN': 0.31.1/0.32.4/0.33.0 not run because the repository's disposable game copy game-copies\loader-compat was in active use by enrollment kmg-codex-s7-visual-audio-remediation on 2026-11-24 (the owner installation is not used); 0.33.4 has no reviewed harness profile; no combined-stack check ran.
- **R1-F8** — `fixed` — commits: `06800140fc`, `7328aaa2da`
  docs/evidence/umm-0311/pinned-loader-msbuild.json (baseline_commit 35763430ac pinned and its checkout verified): before-change-defaults-compile-against-installed-loader (applies because the installed loader differs from the pins; a byte-identical installed set is treated as valid), direct-msbuild-installed-loader-refused and direct-msbuild-altered-pinned-copy-refused (known nonmatching sets), build-ps1-compiles-against-pinned-loader, direct-msbuild-verified-copy-accepted; the owner installation is only read.
- **R1-F9** — `fixed` — commits: `06800140fc`, `04b9384ed1`, `7328aaa2da`
  Controller focused receipt on 7328aaa2da: -Filter loader.production-dll-references-pinned-loader, selected 1 of 3189, PASS, Completed 1 tests; failures=0, after production compilation. The test (tests/KingmakerGunslinger.DomainTests/LoaderReferenceContractTests.cs) inspects the gate-compiled production DLL's AssemblyRefs, bound by path and SHA-256 (refuses an unbound or different file: focused-gate-loader-filter.txt and the unbound refusal recorded during development); the four implementation-text assertions in Test-LoaderReferencePins.ps1 were removed in favour of stub-assembly behaviour cases (35/35).
- **R1-F10** — `fixed` — commits: `45b9b77097`, `8c7bff4a37`
  docs/UMM-LOADER-COMPATIBILITY.md: historical assurance limited to the verified v0.0.188/v0.0.195 packages; docs/BUILDING.md: compile-only bootstrap (build.ps1 -CompileOnly), clean development package (Build-Local -DevelopmentPurpose/-DevelopmentTestFilter, output folder, bundle discovery, no running game), absent entry fetched vs present mismatched file refused with the exact message and explicit recovery; scripts/README.md build.ps1/Build-Local entries corrected. Release records preserved.

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

_Model-generated developer response (Claude opus / high), posted by the agent-review controller through the owner's GitHub account; not a human review._
