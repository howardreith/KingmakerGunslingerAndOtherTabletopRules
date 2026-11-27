<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r04-review part=1/1 sha256=8bb1470b2cc891f099f9594cff41751ee21cacfb2821e1b3f625761c366f8b8f -->
### Codex GPT-6.2 Astra — review — round 4 — candidate `7cf162822a`

**Decision:** `changes_required` — The focused gate passed, and the frozen development DLL has the required loader references. The candidate still accepts inconsistent provenance and staging data, rejects a valid detached build, and can report a selected loader test as passed without inspecting a DLL.

## Continuation for Claude

Keep the correction within TRE’s build, package admission, staging and evidence paths. Bind receipt fields to the producing state and measured package, finish checking the complete staging tree before replacing Mods, and preserve the distinct identities of detached, development and historical candidates. Make explicit loader-test selections require a bound DLL and restore the caller’s state on every failure. Return package measurements through an interface that cannot overwrite the validated ZIP.

Use the disposable offline controls specified in the findings, including a complete-suite orchestration trace through package validation. Bind the required focused gate to the corrected review commit and identify any rebuilt package by its own producer, digest and MVID. Retain the documented native limitations and existing runtime protections.

#### Findings

**R1-F4** · P1 · defect · blocks readiness — Verify the producing source state before admitting a package as current
- Location: `scripts/RuntimeHarness.Common.ps1:435`; acceptance: —
- Trigger: A receipt names the correct commit but describes a different working-tree state at build time.
- Expected: Admission verifies the recorded producer state before using that commit as the baseline for material-file comparison.
- Actual: Current admission checks the embedded commit and compares the present material files with that commit. It checks only the format of sourceStateSha256. Build-Local applies its clean-start and end-of-build fingerprint checks only to development builds, while the exact compiler stamps the commit passed to it. A regular build made with a modified shipped document can therefore be admitted after the document is restored. A contradictory producer fingerprint also passes the development admission checks.
- Why it blocks / does not: A commit stamp alone does not establish which working-tree inputs produced the package being admitted.
- Requested correction: Verify the producer fingerprint or an equivalent material-input identity for both receipt paths. Preserve reuse across later unshipped review notes. Correct the blanket docs/ exception still present in the compatibility guide’s candidate table.
- Requested evidence: Use a temporary repository to show refusal for an inconsistent producer fingerprint and for a package produced with a modified shipped input that was subsequently restored. Retain the clean producer and review-note controls.

**R2-F3** · P1 · defect · blocks readiness — Verify the full admitted tree before replacing Mods
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:235`; acceptance: —
- Trigger: The private extracted tree changes between admission and loader staging while its ZIP and DLL retain their admitted hashes.
- Expected: The tree entering Mods matches the admitted archive in its full inventory and contents. A failed staging check preserves the previous target.
- Actual: Set-LcGameCopyMods hashes the private ZIP and the extracted DLL, then removes Mods and moves the independently writable extracted directory. A changed packaged asset is staged under the original ZIP digest. A missing extracted Info.json is first read after replacement. The fixture changes the supplied ZIP path and leaves the extracted tree intact.
- Why it blocks / does not: The harness can run different package contents from those recorded in its evidence, or discover an incomplete tree after removing the previous target.
- Requested correction: Prepare and verify the complete replacement before changing Mods. Bind the tree to the admitted archive, including Info.json and non-DLL files, and preserve the prior target on staging failure.
- Requested evidence: On a disposable target containing a sentinel, change one extracted non-DLL file and separately remove Info.json after admission. Show refusal with the sentinel and supplied ZIP intact, then show successful staging of the genuine tree.

**R4-F1** · P1 · defect · blocks readiness — Preserve detached build identities in deployment records
- Location: `scripts/RuntimeHarness.Common.ps1:568`; acceptance: —
- Trigger: Deploy-Local consumes a Build-Local candidate produced from a detached frozen checkout.
- Expected: A present, empty branch is valid for a detached producer whose commit and source identity have been verified.
- Actual: Get-KmgGitState deliberately returns an empty branch for a detached checkout, and both Build-Local receipt writers preserve it. Get-KmgDeploymentExpectations now rejects that value as missing. The new fixture explicitly expects this refusal for a present, empty branch property.
- Why it blocks / does not: A valid frozen build can pass package admission and then fail deployment solely because it came from a detached worktree.
- Requested correction: Distinguish an absent branch property from the supported empty branch value. Use a genuinely missing required field for the negative fixture.
- Requested evidence: Pass a receipt from a temporary detached checkout through deployment expectations and record construction. Also show the existing named-branch control and refusal for an absent required field.

**R4-F2** · P1 · defect · blocks readiness — Prevent explicit loader-test selections from passing without a DLL
- Location: `tests/KingmakerGunslinger.DomainTests/LoaderReferenceContractTests.cs:29`; acceptance: —
- Trigger: A caller selects the loader case through a supported substring or prefix without a usable production binding.
- Expected: An explicit loader-test selection inspects a bound compiler output or fails. An inapplicable or intentionally deferred case is reported accurately.
- Actual: The runner selects tests by substring or --test-prefix. The loader case recognizes only its complete name inside KMG_TEST_FILTER. An unbound selection such as production-dll, or a direct prefix selection, returns normally and Program prints PASS. The Windows wrapper also falls through to this unbound path if an explicitly supplied DLL disappears during runner preparation.
- Why it blocks / does not: A focused invocation can report a successful loader assertion after inspecting zero DLLs.
- Requested correction: Use the runner’s actual selection semantics. Require the artifact for every selected, non-deferred wrapper run, and keep skipped or deferred assertions out of PASS reporting.
- Requested evidence: Exercise an unbound substring selection, a direct --test-prefix selection and a supplied DLL removed during preparation. Retain the successful bound Windows case and the usable unfiltered direct and portable cases.

**R1-F3** · P2 · defect · blocks readiness — Check the compiler MVID even when its companion hash is absent
- Location: `scripts/RuntimeHarness.Common.ps1:478`; acceptance: —
- Trigger: A damaged or partially populated receipt retains compilerDllMvid but loses compilerDllSha256.
- Expected: Every recorded compiler identity agrees with the measured DLL. Legacy receipts can omit fields that their format never recorded.
- Actual: The receipt writers now use the captured compiler identity. Admission checks compilerDllMvid only inside the condition that compilerDllSha256 exists. A partially populated receipt with a contradictory compilerDllMvid and no compilerDllSha256 passes this check.
- Why it blocks / does not: Admission can accept a receipt whose recorded compiler identity disagrees with the measured DLL.
- Requested correction: Validate the compiler fields as a pair whenever either is present, or independently verify each recorded field. Preserve the supported earlier format.
- Requested evidence: Show refusal for a receipt containing only an inconsistent compiler MVID, alongside valid current and earlier-format controls.

**R3-F5** · P2 · defect · blocks readiness — Include directory-only descendants in archive conflict checks
- Location: `scripts/common.ps1:153`; acceptance: —
- Trigger: A ZIP contains a regular file and an explicit empty directory nested beneath that file.
- Expected: A file cannot be an ancestor of any file or directory entry.
- Actual: The extractor checks a file against exact directory destinations and against descendant files. It never checks descendant directory records. An empty directory record below a regular file is silently discarded, leaving the file allowlist unchanged and the contradictory archive acceptable.
- Why it blocks / does not: Standalone validation can approve an archive that cannot be extracted as the tree its entries declare.
- Requested correction: Check ancestor conflicts across both files and explicit directories before extraction.
- Requested evidence: Pass an otherwise valid package with an empty directory record beneath a regular file to the standalone validator. Show refusal and retain a passing ordinary-directory control.

**R3-F3** · P2 · evidence_gap · blocks readiness — Demonstrate the complete-suite Build-Local path through packaging
- Location: `docs/evidence/umm-0311/development-build-console.txt:410`; acceptance: —
- Trigger: The selected-test Build-Local run is offered as proof of the complete-suite correction.
- Expected: A bounded orchestration check follows the complete-suite Build-Local path through one candidate compile and one strict package validation.
- Actual: The Build-Local console selects one unrelated test before the exact compile. The unfiltered log comes from standalone test-domain, which performs an MSBuild production compile. The complete-suite Build-Local fixture intentionally stops at the compiler boundary, before packaging.
- Why it blocks / does not: The submitted runs do not exercise the complete path that previously duplicated production work.
- Requested correction: Extend the offline orchestration evidence through the complete-suite branch, its deferred loader assertion and package validation.
- Requested evidence: One trace showing complete-suite selection, one production candidate compile, a real assertion on the resulting DLL, one strict ZIP validation, and matching compiler and packaged identities.

**R4-F3** · P2 · defect · blocks readiness — Record the actual admission mode for ordinary Build-Local packages
- Location: `scripts/compatibility/loader-compat/Invoke-LcCandidateAdmission.ps1:30`; acceptance: —
- Trigger: The loader harness admits a regular Build-Local package without a development purpose or historical release version.
- Expected: The session and its reader preserve the admission kind established by the receipt or historical release record.
- Actual: The admission child uses development for every nonhistorical package. With no development purpose, Read-KmgBuildLocalManifest takes the regular Build-Local manifest path. The session writes the incorrect mode, and its -Report projection omits the new admission block.
- Why it blocks / does not: A supported loader run produces incorrect candidate attribution in its compatibility evidence.
- Requested correction: Distinguish regular Build-Local, purpose-bound development and historical controls in the child result. Surface the recorded admission through -Report while retaining support for older summaries.
- Requested evidence: Show the child result and report projection for all three admission modes using offline records.

**R4-F4** · P2 · defect · blocks readiness — Keep measurement output from overwriting the validated ZIP
- Location: `scripts/package.ps1:368`; acceptance: —
- Trigger: The new measurement output and package output resolve to the same destination.
- Expected: Returning validation measurements preserves the verified package and its checksum.
- Actual: After validating and hashing the ZIP, package.ps1 writes JSON to the unchecked MeasuredIdentityPath. If the two paths resolve to the same file, the script replaces the ZIP with JSON, writes the old ZIP digest to the checksum and reports successful creation.
- Why it blocks / does not: The packaging command can invalidate its own verified output and return success.
- Requested correction: Prefer an opt-in structured return that preserves the existing default path output. If a measurement file is retained, give it a distinct owned destination and reject collisions before any writes.
- Requested evidence: Show that requesting measurements leaves the final ZIP digest equal to its checksum and that conflicting output destinations cannot overwrite the package.

**R4-F5** · P2 · defect · blocks readiness — Restore test bindings when private DLL preparation fails
- Location: `scripts/test-domain.ps1:90`; acceptance: —
- Trigger: A caller invokes test-domain in-process and private binding preparation throws before the runner starts.
- Expected: Every exit after binding setup restores the original environment and removes temporary files owned by that invocation.
- Actual: test-domain clears the caller’s binding variables and creates its private DLL directory before entering the try/finally. The explicit digest-mismatch refusal therefore leaves the previous bindings cleared. A copy or hash error can also leave the private directory behind.
- Why it blocks / does not: A supported negative check changes the state of the invoking PowerShell session and can affect its next run.
- Requested correction: Enclose binding changes, private copying, digest validation and runner execution in the same cleanup scope.
- Requested evidence: Seed existing binding variables, invoke the digest-refusal path in the same PowerShell process, and verify the original values and absence of owned temporary directories afterward.

**R4-F6** · P3 · optional_polish · non-blocking — Keep historical release labels out of hash-valued fields
- Location: `scripts/RuntimeHarness.Common.ps1:558`; acceptance: —
- Trigger: A consumer reads the source-state field of a historical deployment record.
- Expected: A field named sourceStateSha256 contains a measured digest or explicitly represents unavailable data.
- Actual: The historical deployment writes the text historical-release followed by the tag into sourceStateSha256. The record already has dedicated fields for the release tag and producer commit.
- Why it blocks / does not: The historical tag, commit, ZIP digest and DLL identity already identify the control. This concerns clarity for consumers of the optional source-state field.
- Requested correction: Represent unavailable source-state data explicitly for historical controls and keep the tag in releaseTag.
- Requested evidence: An example historical deployment record showing the distinction.

#### Prior finding assessments

- **R1-F10** — `fix_verified` — Direct MSBuild now emits the compile-only bootstrap for missing cache entries. The 15-case fixture covers an absent cache, altered cache bytes and an external loader folder whose bytes were preserved.
- **R1-F3** — `fix_rejected` — Both writers now use the compared DLL measurements, and the packaged hash and stamp refusal controls pass. The reader still ignores a recorded compiler MVID when its companion hash is missing.
- **R1-F4** — `fix_rejected` — The shared document inventory and committed and uncommitted shipped-document controls fix the path omission. Current admission still accepts a producer fingerprint inconsistent with the claimed commit state.
- **R2-F1** — `fix_verified` — The Framework-only reflection call is excluded from the .NET 8 build, and an ordinary unbound runner can complete. The isolated portable probe and bound Windows run cover those corrections. R4-F2 addresses the distinct false PASS introduced for explicit selections.
- **R2-F2** — `fix_verified` — Historical admission uses the record committed at HEAD, rejects a differing working copy and verifies the tag, ZIP and DLL. The child carries the historical tag and producer into the raw session summary.
- **R2-F3** — `fix_rejected` — Deploy-Local compares both admissions, and the runtime DLL expectation now comes from admission. The loader still replaces Mods using extracted content whose non-DLL files have not been reverified.
- **R3-F1** — `fix_verified` — The historical route derives its audio measurements from the admitted release and establishes its record fields before target replacement. The v0.0.188 offline control exercises that logic. R4-F1 concerns a separate regression for detached Build-Local producers.
- **R3-F2** — `fix_verified` — The focused gate captures the digest before runner preparation. The wrapper compares a private copy with that digest, and the archived mismatch run records refusal. The missing-artifact selection path is covered separately by R4-F2.
- **R3-F3** — `still_open` — Source inspection shows the duplicate validator call was removed and the exact build supplies the loader assertion. The requested complete-suite orchestration evidence ends before package validation.
- **R3-F4** — `fix_verified` — The session removes its private copy on preflight or acquisition refusal and during final cleanup. The busy-host fixture records COPY-REMAINS False, and the successful preflight-only fixture explicitly releases its copy.
- **R3-F5** — `fix_rejected` — The shared extractor rejects data-bearing directories, duplicates and exact file/directory collisions. Its ancestor check considers only descendant files, leaving a directory-only descendant beneath a file admissible.

#### Evidence assessment

- **Inspected:** The mission, repository instructions, validation policy, workflow guide, complete feature diff, developer report, transcript notice, controller gate receipt and eleven pending ledger entries.; Both receipt writers, current and historical admission, material-input comparison, package extraction, deployment expectations, loader staging and session reporting.; The production project, pin diagnostics, gate and test wrappers, runner selection logic, offline fixtures, compatibility guide and indexed evidence.
- **Executed verification:** I confirmed HEAD and the mission SHA-256, matched the supplied 7,259-line feature diff to Git, and verified all 22 indexed evidence files against their committed blob SHA-256 values.; The controller receipt records a successful focused gate on 7cf162822a8c6005e2d76563bef6c60d386b5e29 in 3.1 seconds. Production compilation verified UMM 0.31.1.0 and Harmony12 1.2.0.1. The named loader test passed, selecting 1 of 3,189 cases.; The archived Build-Local console, receipt, standalone validator output and API audit agree on producer 4f196b26d112640ace1eaa49dea355d16e210127, ZIP SHA-256 928a318845a85ddf3444b8b0dbfb16bb7cd829764fb5adcc9bfb2a6a5f97ad32, DLL SHA-256 b1e07c2a3e0f28e6a19534e0a10c2cbc9e61c57f35d1ad08d79321c725a5e255 and MVID 0cfcbddc-e648-4e89-b4d5-9d1c238ca0a5. Git shows no material input or shipped-document change between that producer and H.; The producer’s archived outputs record 46/46 admission cases, 15/15 MSBuild cases, 35/35 pin checks, 13/13 gate fixture checks and 102/102 loader safety checks. I counted 3,189 distinct PASS lines in the separate unfiltered Windows domain run.; The static audit records resolution of 23 types and 67 members on UMM 0.31.1, 0.32.4, the measured owner installation of 0.33.0, and 0.33.4 using the same frozen DLL.
- **Source inspection only:** The missing compiler-hash case, producer fingerprint handling, mutable extracted tree, directory-only descendant conflict, measurement-output collision and test setup cleanup were traced in source.; The historical release’s identity reaches the raw session summary. The default Build-Local mode and the read-only report projection were assessed from their actual callers.; The selected Build-Local console shows one exact compiler output and one strict validation. Execution of that entire sequence with the complete-suite option remains unverified.
- **Required gaps:** Close the receipt, producer-state, staging-tree and archive-directory gaps in R1-F3, R1-F4, R2-F3 and R3-F5.; Supply the complete-suite Build-Local orchestration evidence requested in R3-F3.; Correct the detached-branch, test-selection, admission-mode, measurement-output and cleanup regressions in R4-F1 through R4-F5.
- **Limitations:** Only the focused run has a controller execution receipt at H. The foreground transcript was withheld. Other execution results come from the indexed outputs at their named producer.; The review sandbox permitted Git and file inspection. Its PowerShell execution policy prevented loading the helper scripts for an additional in-memory check. The uncovered failure paths are identified by source inspection.; Native TRE behavior was not observed on any of the four loaders for this package. Another enrollment occupied the disposable copy for the first three profiles, and 0.33.4 lacked a reviewed harness profile. No combined-stack run was recorded.; The portable evidence covers the new case in an isolated .NET 8 probe. The complete portable runner has a documented pre-existing Newtonsoft.Json build limitation.

#### Summary

- **Delivered behavior:** TRE declares UMM 0.31.1. The verified production package references UnityModManager 0.31.1.0 and Harmony12 1.2.0.1, and its static API checks resolved on all four selected loaders. The remaining findings concern candidate provenance, staging, package integrity and validation reporting.
- **Scope reviewed:** Final correction review of TRE draft PR #220 at 7cf162822a8c6005e2d76563bef6c60d386b5e29, including all eleven pending findings and directly affected consumers.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** The owner controls merge and release decisions after review of a corrected candidate.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: none.

Codex CLI `0.170.0`, thread `01a20eac-28f3-7492-8b86-a35ec7ee46d9`, duration 892.9s, response sha256 `b48f22b6c346428bc8ddcefee28f53b2ee7e2bdf7223631e44dfff1ac21f5007`, request `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r04-fdab42`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "7cf162822a8c6005e2d76563bef6c60d386b5e29",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The focused gate passed, and the frozen development DLL has the required loader references. The candidate still accepts inconsistent provenance and staging data, rejects a valid detached build, and can report a selected loader test as passed without inspecting a DLL.\n\n## Continuation for Claude\n\nKeep the correction within TRE\u2019s build, package admission, staging and evidence paths. Bind receipt fields to the producing state and measured package, finish checking the complete staging tree before replacing Mods, and preserve the distinct identities of detached, development and historical candidates. Make explicit loader-test selections require a bound DLL and restore the caller\u2019s state on every failure. Return package measurements through an interface that cannot overwrite the validated ZIP.\n\nUse the disposable offline controls specified in the findings, including a complete-suite orchestration trace through package validation. Bind the required focused gate to the corrected review commit and identify any rebuilt package by its own producer, digest and MVID. Retain the documented native limitations and existing runtime protections.",
  "evidence_assessment": {
    "executed_verification": [
      "I confirmed HEAD and the mission SHA-256, matched the supplied 7,259-line feature diff to Git, and verified all 22 indexed evidence files against their committed blob SHA-256 values.",
      "The controller receipt records a successful focused gate on 7cf162822a8c6005e2d76563bef6c60d386b5e29 in 3.1 seconds. Production compilation verified UMM 0.31.1.0 and Harmony12 1.2.0.1. The named loader test passed, selecting 1 of 3,189 cases.",
      "The archived Build-Local console, receipt, standalone validator output and API audit agree on producer 4f196b26d112640ace1eaa49dea355d16e210127, ZIP SHA-256 928a318845a85ddf3444b8b0dbfb16bb7cd829764fb5adcc9bfb2a6a5f97ad32, DLL SHA-256 b1e07c2a3e0f28e6a19534e0a10c2cbc9e61c57f35d1ad08d79321c725a5e255 and MVID 0cfcbddc-e648-4e89-b4d5-9d1c238ca0a5. Git shows no material input or shipped-document change between that producer and H.",
      "The producer\u2019s archived outputs record 46/46 admission cases, 15/15 MSBuild cases, 35/35 pin checks, 13/13 gate fixture checks and 102/102 loader safety checks. I counted 3,189 distinct PASS lines in the separate unfiltered Windows domain run.",
      "The static audit records resolution of 23 types and 67 members on UMM 0.31.1, 0.32.4, the measured owner installation of 0.33.0, and 0.33.4 using the same frozen DLL."
    ],
    "inspected": [
      "The mission, repository instructions, validation policy, workflow guide, complete feature diff, developer report, transcript notice, controller gate receipt and eleven pending ledger entries.",
      "Both receipt writers, current and historical admission, material-input comparison, package extraction, deployment expectations, loader staging and session reporting.",
      "The production project, pin diagnostics, gate and test wrappers, runner selection logic, offline fixtures, compatibility guide and indexed evidence."
    ],
    "limitations": [
      "Only the focused run has a controller execution receipt at H. The foreground transcript was withheld. Other execution results come from the indexed outputs at their named producer.",
      "The review sandbox permitted Git and file inspection. Its PowerShell execution policy prevented loading the helper scripts for an additional in-memory check. The uncovered failure paths are identified by source inspection.",
      "Native TRE behavior was not observed on any of the four loaders for this package. Another enrollment occupied the disposable copy for the first three profiles, and 0.33.4 lacked a reviewed harness profile. No combined-stack run was recorded.",
      "The portable evidence covers the new case in an isolated .NET 8 probe. The complete portable runner has a documented pre-existing Newtonsoft.Json build limitation."
    ],
    "required_gaps": [
      "Close the receipt, producer-state, staging-tree and archive-directory gaps in R1-F3, R1-F4, R2-F3 and R3-F5.",
      "Supply the complete-suite Build-Local orchestration evidence requested in R3-F3.",
      "Correct the detached-branch, test-selection, admission-mode, measurement-output and cleanup regressions in R4-F1 through R4-F5."
    ],
    "source_inspection_only": [
      "The missing compiler-hash case, producer fingerprint handling, mutable extracted tree, directory-only descendant conflict, measurement-output collision and test setup cleanup were traced in source.",
      "The historical release\u2019s identity reaches the raw session summary. The default Build-Local mode and the read-only report projection were assessed from their actual callers.",
      "The selected Build-Local console shows one exact compiler output and one strict validation. Execution of that entire sequence with the complete-suite option remains unverified."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [],
      "actual": "Current admission checks the embedded commit and compares the present material files with that commit. It checks only the format of sourceStateSha256. Build-Local applies its clean-start and end-of-build fingerprint checks only to development builds, while the exact compiler stamps the commit passed to it. A regular build made with a modified shipped document can therefore be admitted after the document is restored. A contradictory producer fingerprint also passes the development admission checks.",
      "blocking_rationale": "A commit stamp alone does not establish which working-tree inputs produced the package being admitted.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Admission verifies the recorded producer state before using that commit as the baseline for material-file comparison.",
      "id": "R1-F4",
      "location": {
        "file": "scripts/RuntimeHarness.Common.ps1",
        "line": 435
      },
      "priority": "P1",
      "requested_correction": "Verify the producer fingerprint or an equivalent material-input identity for both receipt paths. Preserve reuse across later unshipped review notes. Correct the blanket docs/ exception still present in the compatibility guide\u2019s candidate table.",
      "requested_evidence": "Use a temporary repository to show refusal for an inconsistent producer fingerprint and for a package produced with a modified shipped input that was subsequently restored. Retain the clean producer and review-note controls.",
      "title": "Verify the producing source state before admitting a package as current",
      "trigger": "A receipt names the correct commit but describes a different working-tree state at build time."
    },
    {
      "acceptance_ids": [],
      "actual": "Set-LcGameCopyMods hashes the private ZIP and the extracted DLL, then removes Mods and moves the independently writable extracted directory. A changed packaged asset is staged under the original ZIP digest. A missing extracted Info.json is first read after replacement. The fixture changes the supplied ZIP path and leaves the extracted tree intact.",
      "blocking_rationale": "The harness can run different package contents from those recorded in its evidence, or discover an incomplete tree after removing the previous target.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The tree entering Mods matches the admitted archive in its full inventory and contents. A failed staging check preserves the previous target.",
      "id": "R2-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 235
      },
      "priority": "P1",
      "requested_correction": "Prepare and verify the complete replacement before changing Mods. Bind the tree to the admitted archive, including Info.json and non-DLL files, and preserve the prior target on staging failure.",
      "requested_evidence": "On a disposable target containing a sentinel, change one extracted non-DLL file and separately remove Info.json after admission. Show refusal with the sentinel and supplied ZIP intact, then show successful staging of the genuine tree.",
      "title": "Verify the full admitted tree before replacing Mods",
      "trigger": "The private extracted tree changes between admission and loader staging while its ZIP and DLL retain their admitted hashes."
    },
    {
      "acceptance_ids": [],
      "actual": "Get-KmgGitState deliberately returns an empty branch for a detached checkout, and both Build-Local receipt writers preserve it. Get-KmgDeploymentExpectations now rejects that value as missing. The new fixture explicitly expects this refusal for a present, empty branch property.",
      "blocking_rationale": "A valid frozen build can pass package admission and then fail deployment solely because it came from a detached worktree.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A present, empty branch is valid for a detached producer whose commit and source identity have been verified.",
      "id": "R4-F1",
      "location": {
        "file": "scripts/RuntimeHarness.Common.ps1",
        "line": 568
      },
      "priority": "P1",
      "requested_correction": "Distinguish an absent branch property from the supported empty branch value. Use a genuinely missing required field for the negative fixture.",
      "requested_evidence": "Pass a receipt from a temporary detached checkout through deployment expectations and record construction. Also show the existing named-branch control and refusal for an absent required field.",
      "title": "Preserve detached build identities in deployment records",
      "trigger": "Deploy-Local consumes a Build-Local candidate produced from a detached frozen checkout."
    },
    {
      "acceptance_ids": [],
      "actual": "The runner selects tests by substring or --test-prefix. The loader case recognizes only its complete name inside KMG_TEST_FILTER. An unbound selection such as production-dll, or a direct prefix selection, returns normally and Program prints PASS. The Windows wrapper also falls through to this unbound path if an explicitly supplied DLL disappears during runner preparation.",
      "blocking_rationale": "A focused invocation can report a successful loader assertion after inspecting zero DLLs.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An explicit loader-test selection inspects a bound compiler output or fails. An inapplicable or intentionally deferred case is reported accurately.",
      "id": "R4-F2",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/LoaderReferenceContractTests.cs",
        "line": 29
      },
      "priority": "P1",
      "requested_correction": "Use the runner\u2019s actual selection semantics. Require the artifact for every selected, non-deferred wrapper run, and keep skipped or deferred assertions out of PASS reporting.",
      "requested_evidence": "Exercise an unbound substring selection, a direct --test-prefix selection and a supplied DLL removed during preparation. Retain the successful bound Windows case and the usable unfiltered direct and portable cases.",
      "title": "Prevent explicit loader-test selections from passing without a DLL",
      "trigger": "A caller selects the loader case through a supported substring or prefix without a usable production binding."
    },
    {
      "acceptance_ids": [],
      "actual": "The receipt writers now use the captured compiler identity. Admission checks compilerDllMvid only inside the condition that compilerDllSha256 exists. A partially populated receipt with a contradictory compilerDllMvid and no compilerDllSha256 passes this check.",
      "blocking_rationale": "Admission can accept a receipt whose recorded compiler identity disagrees with the measured DLL.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every recorded compiler identity agrees with the measured DLL. Legacy receipts can omit fields that their format never recorded.",
      "id": "R1-F3",
      "location": {
        "file": "scripts/RuntimeHarness.Common.ps1",
        "line": 478
      },
      "priority": "P2",
      "requested_correction": "Validate the compiler fields as a pair whenever either is present, or independently verify each recorded field. Preserve the supported earlier format.",
      "requested_evidence": "Show refusal for a receipt containing only an inconsistent compiler MVID, alongside valid current and earlier-format controls.",
      "title": "Check the compiler MVID even when its companion hash is absent",
      "trigger": "A damaged or partially populated receipt retains compilerDllMvid but loses compilerDllSha256."
    },
    {
      "acceptance_ids": [],
      "actual": "The extractor checks a file against exact directory destinations and against descendant files. It never checks descendant directory records. An empty directory record below a regular file is silently discarded, leaving the file allowlist unchanged and the contradictory archive acceptable.",
      "blocking_rationale": "Standalone validation can approve an archive that cannot be extracted as the tree its entries declare.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A file cannot be an ancestor of any file or directory entry.",
      "id": "R3-F5",
      "location": {
        "file": "scripts/common.ps1",
        "line": 153
      },
      "priority": "P2",
      "requested_correction": "Check ancestor conflicts across both files and explicit directories before extraction.",
      "requested_evidence": "Pass an otherwise valid package with an empty directory record beneath a regular file to the standalone validator. Show refusal and retain a passing ordinary-directory control.",
      "title": "Include directory-only descendants in archive conflict checks",
      "trigger": "A ZIP contains a regular file and an explicit empty directory nested beneath that file."
    },
    {
      "acceptance_ids": [],
      "actual": "The Build-Local console selects one unrelated test before the exact compile. The unfiltered log comes from standalone test-domain, which performs an MSBuild production compile. The complete-suite Build-Local fixture intentionally stops at the compiler boundary, before packaging.",
      "blocking_rationale": "The submitted runs do not exercise the complete path that previously duplicated production work.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "A bounded orchestration check follows the complete-suite Build-Local path through one candidate compile and one strict package validation.",
      "id": "R3-F3",
      "location": {
        "file": "docs/evidence/umm-0311/development-build-console.txt",
        "line": 410
      },
      "priority": "P2",
      "requested_correction": "Extend the offline orchestration evidence through the complete-suite branch, its deferred loader assertion and package validation.",
      "requested_evidence": "One trace showing complete-suite selection, one production candidate compile, a real assertion on the resulting DLL, one strict ZIP validation, and matching compiler and packaged identities.",
      "title": "Demonstrate the complete-suite Build-Local path through packaging",
      "trigger": "The selected-test Build-Local run is offered as proof of the complete-suite correction."
    },
    {
      "acceptance_ids": [],
      "actual": "The admission child uses development for every nonhistorical package. With no development purpose, Read-KmgBuildLocalManifest takes the regular Build-Local manifest path. The session writes the incorrect mode, and its -Report projection omits the new admission block.",
      "blocking_rationale": "A supported loader run produces incorrect candidate attribution in its compatibility evidence.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The session and its reader preserve the admission kind established by the receipt or historical release record.",
      "id": "R4-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/Invoke-LcCandidateAdmission.ps1",
        "line": 30
      },
      "priority": "P2",
      "requested_correction": "Distinguish regular Build-Local, purpose-bound development and historical controls in the child result. Surface the recorded admission through -Report while retaining support for older summaries.",
      "requested_evidence": "Show the child result and report projection for all three admission modes using offline records.",
      "title": "Record the actual admission mode for ordinary Build-Local packages",
      "trigger": "The loader harness admits a regular Build-Local package without a development purpose or historical release version."
    },
    {
      "acceptance_ids": [],
      "actual": "After validating and hashing the ZIP, package.ps1 writes JSON to the unchecked MeasuredIdentityPath. If the two paths resolve to the same file, the script replaces the ZIP with JSON, writes the old ZIP digest to the checksum and reports successful creation.",
      "blocking_rationale": "The packaging command can invalidate its own verified output and return success.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Returning validation measurements preserves the verified package and its checksum.",
      "id": "R4-F4",
      "location": {
        "file": "scripts/package.ps1",
        "line": 368
      },
      "priority": "P2",
      "requested_correction": "Prefer an opt-in structured return that preserves the existing default path output. If a measurement file is retained, give it a distinct owned destination and reject collisions before any writes.",
      "requested_evidence": "Show that requesting measurements leaves the final ZIP digest equal to its checksum and that conflicting output destinations cannot overwrite the package.",
      "title": "Keep measurement output from overwriting the validated ZIP",
      "trigger": "The new measurement output and package output resolve to the same destination."
    },
    {
      "acceptance_ids": [],
      "actual": "test-domain clears the caller\u2019s binding variables and creates its private DLL directory before entering the try/finally. The explicit digest-mismatch refusal therefore leaves the previous bindings cleared. A copy or hash error can also leave the private directory behind.",
      "blocking_rationale": "A supported negative check changes the state of the invoking PowerShell session and can affect its next run.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every exit after binding setup restores the original environment and removes temporary files owned by that invocation.",
      "id": "R4-F5",
      "location": {
        "file": "scripts/test-domain.ps1",
        "line": 90
      },
      "priority": "P2",
      "requested_correction": "Enclose binding changes, private copying, digest validation and runner execution in the same cleanup scope.",
      "requested_evidence": "Seed existing binding variables, invoke the digest-refusal path in the same PowerShell process, and verify the original values and absence of owned temporary directories afterward.",
      "title": "Restore test bindings when private DLL preparation fails",
      "trigger": "A caller invokes test-domain in-process and private binding preparation throws before the runner starts."
    },
    {
      "acceptance_ids": [],
      "actual": "The historical deployment writes the text historical-release followed by the tag into sourceStateSha256. The record already has dedicated fields for the release tag and producer commit.",
      "blocking_rationale": "The historical tag, commit, ZIP digest and DLL identity already identify the control. This concerns clarity for consumers of the optional source-state field.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "A field named sourceStateSha256 contains a measured digest or explicitly represents unavailable data.",
      "id": "R4-F6",
      "location": {
        "file": "scripts/RuntimeHarness.Common.ps1",
        "line": 558
      },
      "priority": "P3",
      "requested_correction": "Represent unavailable source-state data explicitly for historical controls and keep the tag in releaseTag.",
      "requested_evidence": "An example historical deployment record showing the distinction.",
      "title": "Keep historical release labels out of hash-valued fields",
      "trigger": "A consumer reads the source-state field of a historical deployment record."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F10",
      "rationale": "Direct MSBuild now emits the compile-only bootstrap for missing cache entries. The 15-case fixture covers an absent cache, altered cache bytes and an external loader folder whose bytes were preserved."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F3",
      "rationale": "Both writers now use the compared DLL measurements, and the packaged hash and stamp refusal controls pass. The reader still ignores a recorded compiler MVID when its companion hash is missing."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F4",
      "rationale": "The shared document inventory and committed and uncommitted shipped-document controls fix the path omission. Current admission still accepts a producer fingerprint inconsistent with the claimed commit state."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F1",
      "rationale": "The Framework-only reflection call is excluded from the .NET 8 build, and an ordinary unbound runner can complete. The isolated portable probe and bound Windows run cover those corrections. R4-F2 addresses the distinct false PASS introduced for explicit selections."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F2",
      "rationale": "Historical admission uses the record committed at HEAD, rejects a differing working copy and verifies the tag, ZIP and DLL. The child carries the historical tag and producer into the raw session summary."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R2-F3",
      "rationale": "Deploy-Local compares both admissions, and the runtime DLL expectation now comes from admission. The loader still replaces Mods using extracted content whose non-DLL files have not been reverified."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F1",
      "rationale": "The historical route derives its audio measurements from the admitted release and establishes its record fields before target replacement. The v0.0.188 offline control exercises that logic. R4-F1 concerns a separate regression for detached Build-Local producers."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F2",
      "rationale": "The focused gate captures the digest before runner preparation. The wrapper compares a private copy with that digest, and the archived mismatch run records refusal. The missing-artifact selection path is covered separately by R4-F2."
    },
    {
      "assessment": "still_open",
      "finding_id": "R3-F3",
      "rationale": "Source inspection shows the duplicate validator call was removed and the exact build supplies the loader assertion. The requested complete-suite orchestration evidence ends before package validation."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F4",
      "rationale": "The session removes its private copy on preflight or acquisition refusal and during final cleanup. The busy-host fixture records COPY-REMAINS False, and the successful preflight-only fixture explicitly releases its copy."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R3-F5",
      "rationale": "The shared extractor rejects data-bearing directories, duplicates and exact file/directory collisions. Its ancestor check considers only descendant files, leaving a directory-only descendant beneath a file admissible."
    }
  ],
  "request_id": "kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r04-fdab42",
  "round": 4,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "TRE declares UMM 0.31.1. The verified production package references UnityModManager 0.31.1.0 and Harmony12 1.2.0.1, and its static API checks resolved on all four selected loaders. The remaining findings concern candidate provenance, staging, package integrity and validation reporting.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "The owner controls merge and release decisions after review of a corrected candidate."
    ],
    "scope_reviewed": "Final correction review of TRE draft PR #220 at 7cf162822a8c6005e2d76563bef6c60d386b5e29, including all eleven pending findings and directly affected consumers.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
