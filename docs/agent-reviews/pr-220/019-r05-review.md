<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r05-review part=1/1 sha256=819fccb585690a785ea4caed8c69fa4bcac9900ee5e58e6389413cf0b9874783 -->
### Codex GPT-6.2 Astra — review — round 5 — candidate `a7d293a533`

**Decision:** `changes_required` — The focused gate passed on the review commit, and the frozen development package has the expected loader references. Two correctness gaps still affect artifact provenance and staging. An ordinary Build-Local receipt can describe changed package inputs as clean, and a late staging failure can leave the previous Mods tree deleted. The review also found defects in test-evidence counting and two PowerShell interfaces. Nine of the eleven prior findings are verified.

## Continuation for Claude

Bind the ordinary build receipt to the inputs actually consumed. Exercise the real packaging step with a changed shipped file in a temporary checkout, then check admission after restoring the file. Use an offline ordinary Build-Local receipt to exercise the admission child and pass its returned identity through the report. Keep the previous Mods tree available through the last staging check and exercise a late failure on a disposable target.

Require one distinct outcome per selected test, return a single measurement object from package.ps1, and preserve the caller’s environment across the entire selection fixture. Run the affected positive and negative checks and the controller’s focused gate within the existing enrollment budget. Keep the corrections within TRE build, admission and validation tooling. The documented native blockers and the other six repositories do not require work in this review.

#### Findings

**R1-F4** · P1 · defect · blocks readiness — Bind the ordinary receipt to the inputs that entered the package
- Location: `scripts/Build-Local.ps1:500`; acceptance: —
- Trigger: A shipped file changes after an ordinary Build-Local run takes its initial fingerprint.
- Expected: A receipt accepted for current staging binds the producing commit to the material inputs consumed by the compiler and packager.
- Actual: Build-Local records sourceStateSha256 at line 99. At completion it compares the commit for both routes, but rechecks the source fingerprint only for development. The ordinary manifest writes the initial fingerprint at line 575. A README edit made before package.ps1 copies it can therefore enter the ZIP under a clean fingerprint. Restoring the README after the build makes the current-tree comparison pass. The new fixture changes the receipt on an already clean ZIP and never packages the edited README.
- Why it blocks / does not: An ordinary package can be admitted as the product of a clean commit while carrying different shipped content.
- Requested correction: Verify the consumed material inputs for ordinary builds as well as development builds. Preserve reuse after unshipped review-note changes. Update the compatibility guide to describe what the negative fixture actually exercises.
- Requested evidence: In a temporary checkout, change a shipped input after the initial fingerprint and exercise the real packaging boundary. Show that receipt creation or subsequent admission refuses the altered content, including after the checkout is restored. Retain the clean and unshipped-note controls.

**R4-F3** · P2 · evidence_gap · blocks readiness — Exercise the ordinary admission child through reporting
- Location: `scripts/Test-CandidateAdmission.ps1:489`; acceptance: —
- Trigger: The evidence is used to qualify an admission with neither a development purpose nor a historical release version.
- Expected: An offline ordinary Build-Local admission returns build-local from the child and preserves that identity through the report.
- Actual: The fixture runs the admission child for development and historical packages. For the ordinary route it calls Get-KmgAdmissionMode directly, then supplies a handwritten summary containing build-local. Neither observation exercises the ordinary manifest reader through the child.
- Why it blocks / does not: The requested integration observation is missing for the specific route whose identity was previously recorded incorrectly.
- Requested correction: Exercise the ordinary admission path with an explicitly identified offline fixture and use its returned identity as the report input.
- Requested evidence: Capture the child’s successful exit, build-local mode, producer and package digest, and the corresponding report projection. No release or native transaction is needed.

**R5-F1** · P2 · defect · blocks readiness — Keep the rollback copy through final staging verification
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:300`; acceptance: —
- Trigger: A final staged-file check fails after the prepared directory has replaced Mods.
- Expected: The previous Mods tree can be restored until every check required for staging success has completed.
- Actual: After the rename, line 300 deletes the previous Mods tree. Lines 305–310 then hash the staged product, check its digest, read Info.json and hash the harness. A read failure or the explicit digest-mismatch exception reports staging failure after the rollback copy is gone.
- Why it blocks / does not: A failed staging check can leave the shared disposable copy changed and its previous target unavailable for restoration.
- Requested correction: Perform fallible measurements on the prepared tree where possible. Retain the previous tree through any required destination checks and restore it on their failure.
- Requested evidence: On a disposable target, force a final staged-file verification error and show that the previous sentinel contents are back at Mods. Retain the successful swap control.

**R5-F2** · P2 · defect · blocks readiness — Count each selected test exactly once across all outcomes
- Location: `scripts/DevelopmentCandidate.Common.ps1:160`; acceptance: —
- Trigger: A selected-test log contains a repeated loader deferral or comes from an unbound direct run.
- Expected: Each selected test has one distinct outcome. The loader case may be deferred once for the exact build, and the receipt agrees with the log.
- Actual: The reader checks uniqueness only among PASS names, then adds every NOT APPLICABLE row to the selected count. Two loader deferrals can account for a missing test. The parser also discards the reason, accepting an ordinary unbound skip as a Build-Local deferral. Receipt readback omits the recorded notApplicable list from its comparison.
- Why it blocks / does not: A development receipt can validate incomplete or contradictory test coverage.
- Requested correction: Check uniqueness across passed and deferred outcomes, distinguish an intentional deferral from an unbound run, and compare any recorded notApplicable field with the parsed log. Preserve valid earlier receipts.
- Requested evidence: Show refusal for a duplicate deferral replacing another pass, conflicting outcomes for the same case, and an unbound loader skip supplied as development-build evidence. Keep a valid deferred run followed by the exact-DLL pass.

**R5-F3** · P2 · defect · blocks readiness — Return a single package measurement on the success stream
- Location: `scripts/package.ps1:354`; acceptance: —
- Trigger: A caller assigns the result of package.ps1 -PassThruMeasurement without filtering it.
- Expected: The opt-in measurement call emits one object containing the package path, digest and measured DLL identity.
- Actual: create_deterministic_package.py prints the ZIP path. Its stdout flows through package.ps1 before the new measurement object, so assigning the command’s output produces an array. Build-Local filters that array, and the fixture selects its last element, masking the public return shape.
- Why it blocks / does not: A caller consuming or serializing the documented structured result receives a mixture of a path string and an object.
- Requested correction: Consume or redirect the ZIP writer’s informational stdout while preserving its exit check. Return the documented value on the success stream.
- Requested evidence: Capture the entire output of a successful -PassThruMeasurement call and assert a count of one, the object’s fields and agreement with the on-disk checksum. Verify the ordinary path output separately.

**R5-F4** · P2 · defect · blocks readiness — Restore the selection fixture’s caller environment
- Location: `scripts/Test-LoaderCaseSelection.ps1:41`; acceptance: —
- Trigger: Test-LoaderCaseSelection.ps1 runs in an existing PowerShell session with a filter or loader binding.
- Expected: The selection fixture restores the caller’s filter and loader bindings on every exit.
- Actual: The fixture overwrites KMG_TEST_FILTER and removes it at lines 44, 72 and 82. Its final case seeds the four loader bindings and then clears them at line 85. It verifies that test-domain restored the fixture’s seed values, but discards the values its own caller had supplied.
- Why it blocks / does not: Running the new validation entry point can silently broaden the caller’s next test run or erase its production-DLL binding.
- Requested correction: Save all five incoming process variables before the first fixture mutation and restore them in an outer cleanup scope.
- Requested evidence: Invoke the fixture in-process with pre-existing values and confirm restoration after success and a representative failure.

#### Prior finding assessments

- **R1-F3** — `fix_verified` — Admission independently checks each recorded compiler field. The MVID-only mismatch was refused, and current and earlier-format controls were admitted.
- **R1-F4** — `fix_rejected` — Both readers reject a fingerprint inconsistent with a clean producer, and the guide now distinguishes shipped documents. The ordinary writer can still stamp later-modified package inputs with its initial clean fingerprint. The fixture never produced a ZIP containing the modified input.
- **R2-F3** — `fix_verified` — The prepared product is compared file for file with the archive. Both requested pre-swap refusals preserved the sentinel and ZIP, and the genuine tree staged successfully. R5-F1 covers the separate failure path after the swap.
- **R3-F3** — `fix_verified` — The complete-suite Build-Local console reaches packaging. It records the deferred case, one exact candidate compile, the assertion passing on that output and one strict ZIP validation. Compiler and packaged identities agree.
- **R3-F5** — `fix_verified` — The ancestor check includes explicit directories. The standalone validator refused the directory below Info.json and accepted the ordinary directory control.
- **R4-F1** — `fix_verified` — The detached receipt preserved an empty branch through deployment record construction. The named-branch control succeeded and removing the branch property caused refusal.
- **R4-F2** — `fix_verified` — Unbound substring and prefix selections failed. The wrapper refused a missing DLL, the bound Windows case passed, and the unfiltered unbound and intentional deferral cases reported NOT APPLICABLE.
- **R4-F3** — `still_open` — The child’s source selects the correct mode and the report exposes it. The required ordinary child-to-report observation is still missing.
- **R4-F4** — `fix_verified` — Removing the measurement-file destination eliminated the overwrite. The fixture matched the returned digest to the ZIP and checksum file. R5-F3 covers the separate return-shape defect.
- **R4-F5** — `fix_verified` — test-domain encloses binding changes and private-copy preparation in its cleanup scope. The same-process digest refusal restored all four seed values and left the private-directory count unchanged. R5-F4 concerns the outer fixture.
- **R4-F6** — `fix_verified` — The v0.0.188 historical record has null branch and source-state fields, sourceStateAvailable false, and the release tag in its dedicated field.

#### Evidence assessment

- **Inspected:** Controller inputs, applicable repository instructions, the full feature diff, the round-five correction delta and all eleven pending findings.; Production project reference resolution, build and package writers, both receipt paths, standalone validation, loader admission and staging, deployment records, runner selection and evidence readers.
- **Executed verification:** The controller receipt and matching log record a 3.8-second focused gate on a7d293a533. Production compilation verified UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1. The selected loader assertion passed, 1 of 3,189 registered tests.; The archived Build-Local console at 49be3fd0e2 records 3,188 distinct passes and one deferred loader case, followed by one exact candidate compile, a pass on that output and one strict ZIP validation. The packaged DLL SHA-256 is 3507c7554dc3a82bff7684616ab02ac8b0681c1ced4fdcdb7dda54319b5570d1, its MVID is ac6e8b46-3e64-4251-bf96-14cd3e784869, and the ZIP SHA-256 is 71347c1273b1a212f3af38d43231010fa7c40acc7794880c1a80cbfc104aa84b.; Archived outputs at that producer record 59/59 admission cases on a separate temporary-clone package, 15/15 MSBuild cases, 7/7 loader-selection cases and 102/102 offline loader-safety checks. Static metadata resolution on the frozen development DLL covered 23 types and 67 members across all four loader profiles.; Read-only review checks matched the mission and gate-log hashes, all 24 indexed evidence blobs, the producer fingerprint and the digest of passed test names. Git shows 17 unshipped documentation or evidence files changed between the package producer and H.; An in-memory check against the archived selection log confirmed that replacing one PASS with a second copy of the loader deferral still satisfies the reader’s count predicates.
- **Source inspection only:** The ordinary receipt’s completion checks, post-swap rollback ordering, package command’s success-stream contents and outer fixture cleanup were traced in source.; The ordinary admission mode is correctly selected in source. Its published fixture exercises the mode helper and a handwritten summary.
- **Required gaps:** R1-F4 needs a producer-bound check and a real changed-input packaging control.; R4-F3 needs an ordinary Build-Local admission-child result carried through the report.; R5-F1 through R5-F4 need focused checks for late staging rollback, distinct test outcomes, measurement output cardinality and fixture environment restoration.
- **Limitations:** The controller withheld the mixed foreground transcript. Only the focused gate has a controller-run receipt on H. The broader runs are documented by committed outputs at their named producer. This review used read-only inspection and did not rerun builds, packaging or native workflows.; Native behavior on 0.31.1, 0.32.4 and owner 0.33.0 remains NOT RUN for this package because another enrollment used the disposable copy. The 0.33.4 row lacks a reviewed harness profile. No combined-stack run was recorded.; The .NET 8 evidence covers the isolated loader case. The complete portable runner has a documented pre-existing Newtonsoft.Json dependency issue.; The full git diff whitespace check reports 69 trailing-whitespace lines in captured console tables. The check passed for scripts, source, tests and edited guides.

#### Summary

- **Delivered behavior:** TRE declares UMM 0.31.1, verifies the pinned UMM and Harmony12 compiler inputs, and checks the packaged DLL’s actual assembly references. The frozen development DLL resolved statically against 0.31.1, 0.32.4, owner 0.33.0 and 0.33.4.
- **Scope reviewed:** Final correction review of TRE PR #220 at the fixed commit, including all eleven pending findings and the directly affected build, admission, staging and evidence consumers.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** The owner retains the decision on draft PR #220 after the blocking findings close and on scheduling any later guarded native qualification.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: none.

Codex CLI `0.170.0`, thread `01a20f21-bf8c-71f2-b9e4-7b9f8a88ceb7`, duration 769.3s, response sha256 `d02bfc5e14c8625ca6199c917ad9211d858ed669f1efd59901bfb5eaf8f96440`, request `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r05-cdfd83`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "a7d293a5339ec00c3a2ccde27cee309cefb803b2",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The focused gate passed on the review commit, and the frozen development package has the expected loader references. Two correctness gaps still affect artifact provenance and staging. An ordinary Build-Local receipt can describe changed package inputs as clean, and a late staging failure can leave the previous Mods tree deleted. The review also found defects in test-evidence counting and two PowerShell interfaces. Nine of the eleven prior findings are verified.\n\n## Continuation for Claude\n\nBind the ordinary build receipt to the inputs actually consumed. Exercise the real packaging step with a changed shipped file in a temporary checkout, then check admission after restoring the file. Use an offline ordinary Build-Local receipt to exercise the admission child and pass its returned identity through the report. Keep the previous Mods tree available through the last staging check and exercise a late failure on a disposable target.\n\nRequire one distinct outcome per selected test, return a single measurement object from package.ps1, and preserve the caller\u2019s environment across the entire selection fixture. Run the affected positive and negative checks and the controller\u2019s focused gate within the existing enrollment budget. Keep the corrections within TRE build, admission and validation tooling. The documented native blockers and the other six repositories do not require work in this review.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt and matching log record a 3.8-second focused gate on a7d293a533. Production compilation verified UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1. The selected loader assertion passed, 1 of 3,189 registered tests.",
      "The archived Build-Local console at 49be3fd0e2 records 3,188 distinct passes and one deferred loader case, followed by one exact candidate compile, a pass on that output and one strict ZIP validation. The packaged DLL SHA-256 is 3507c7554dc3a82bff7684616ab02ac8b0681c1ced4fdcdb7dda54319b5570d1, its MVID is ac6e8b46-3e64-4251-bf96-14cd3e784869, and the ZIP SHA-256 is 71347c1273b1a212f3af38d43231010fa7c40acc7794880c1a80cbfc104aa84b.",
      "Archived outputs at that producer record 59/59 admission cases on a separate temporary-clone package, 15/15 MSBuild cases, 7/7 loader-selection cases and 102/102 offline loader-safety checks. Static metadata resolution on the frozen development DLL covered 23 types and 67 members across all four loader profiles.",
      "Read-only review checks matched the mission and gate-log hashes, all 24 indexed evidence blobs, the producer fingerprint and the digest of passed test names. Git shows 17 unshipped documentation or evidence files changed between the package producer and H.",
      "An in-memory check against the archived selection log confirmed that replacing one PASS with a second copy of the loader deferral still satisfies the reader\u2019s count predicates."
    ],
    "inspected": [
      "Controller inputs, applicable repository instructions, the full feature diff, the round-five correction delta and all eleven pending findings.",
      "Production project reference resolution, build and package writers, both receipt paths, standalone validation, loader admission and staging, deployment records, runner selection and evidence readers."
    ],
    "limitations": [
      "The controller withheld the mixed foreground transcript. Only the focused gate has a controller-run receipt on H. The broader runs are documented by committed outputs at their named producer. This review used read-only inspection and did not rerun builds, packaging or native workflows.",
      "Native behavior on 0.31.1, 0.32.4 and owner 0.33.0 remains NOT RUN for this package because another enrollment used the disposable copy. The 0.33.4 row lacks a reviewed harness profile. No combined-stack run was recorded.",
      "The .NET 8 evidence covers the isolated loader case. The complete portable runner has a documented pre-existing Newtonsoft.Json dependency issue.",
      "The full git diff whitespace check reports 69 trailing-whitespace lines in captured console tables. The check passed for scripts, source, tests and edited guides."
    ],
    "required_gaps": [
      "R1-F4 needs a producer-bound check and a real changed-input packaging control.",
      "R4-F3 needs an ordinary Build-Local admission-child result carried through the report.",
      "R5-F1 through R5-F4 need focused checks for late staging rollback, distinct test outcomes, measurement output cardinality and fixture environment restoration."
    ],
    "source_inspection_only": [
      "The ordinary receipt\u2019s completion checks, post-swap rollback ordering, package command\u2019s success-stream contents and outer fixture cleanup were traced in source.",
      "The ordinary admission mode is correctly selected in source. Its published fixture exercises the mode helper and a handwritten summary."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [],
      "actual": "Build-Local records sourceStateSha256 at line 99. At completion it compares the commit for both routes, but rechecks the source fingerprint only for development. The ordinary manifest writes the initial fingerprint at line 575. A README edit made before package.ps1 copies it can therefore enter the ZIP under a clean fingerprint. Restoring the README after the build makes the current-tree comparison pass. The new fixture changes the receipt on an already clean ZIP and never packages the edited README.",
      "blocking_rationale": "An ordinary package can be admitted as the product of a clean commit while carrying different shipped content.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A receipt accepted for current staging binds the producing commit to the material inputs consumed by the compiler and packager.",
      "id": "R1-F4",
      "location": {
        "file": "scripts/Build-Local.ps1",
        "line": 500
      },
      "priority": "P1",
      "requested_correction": "Verify the consumed material inputs for ordinary builds as well as development builds. Preserve reuse after unshipped review-note changes. Update the compatibility guide to describe what the negative fixture actually exercises.",
      "requested_evidence": "In a temporary checkout, change a shipped input after the initial fingerprint and exercise the real packaging boundary. Show that receipt creation or subsequent admission refuses the altered content, including after the checkout is restored. Retain the clean and unshipped-note controls.",
      "title": "Bind the ordinary receipt to the inputs that entered the package",
      "trigger": "A shipped file changes after an ordinary Build-Local run takes its initial fingerprint."
    },
    {
      "acceptance_ids": [],
      "actual": "The fixture runs the admission child for development and historical packages. For the ordinary route it calls Get-KmgAdmissionMode directly, then supplies a handwritten summary containing build-local. Neither observation exercises the ordinary manifest reader through the child.",
      "blocking_rationale": "The requested integration observation is missing for the specific route whose identity was previously recorded incorrectly.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "An offline ordinary Build-Local admission returns build-local from the child and preserves that identity through the report.",
      "id": "R4-F3",
      "location": {
        "file": "scripts/Test-CandidateAdmission.ps1",
        "line": 489
      },
      "priority": "P2",
      "requested_correction": "Exercise the ordinary admission path with an explicitly identified offline fixture and use its returned identity as the report input.",
      "requested_evidence": "Capture the child\u2019s successful exit, build-local mode, producer and package digest, and the corresponding report projection. No release or native transaction is needed.",
      "title": "Exercise the ordinary admission child through reporting",
      "trigger": "The evidence is used to qualify an admission with neither a development purpose nor a historical release version."
    },
    {
      "acceptance_ids": [],
      "actual": "After the rename, line 300 deletes the previous Mods tree. Lines 305\u2013310 then hash the staged product, check its digest, read Info.json and hash the harness. A read failure or the explicit digest-mismatch exception reports staging failure after the rollback copy is gone.",
      "blocking_rationale": "A failed staging check can leave the shared disposable copy changed and its previous target unavailable for restoration.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The previous Mods tree can be restored until every check required for staging success has completed.",
      "id": "R5-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 300
      },
      "priority": "P2",
      "requested_correction": "Perform fallible measurements on the prepared tree where possible. Retain the previous tree through any required destination checks and restore it on their failure.",
      "requested_evidence": "On a disposable target, force a final staged-file verification error and show that the previous sentinel contents are back at Mods. Retain the successful swap control.",
      "title": "Keep the rollback copy through final staging verification",
      "trigger": "A final staged-file check fails after the prepared directory has replaced Mods."
    },
    {
      "acceptance_ids": [],
      "actual": "The reader checks uniqueness only among PASS names, then adds every NOT APPLICABLE row to the selected count. Two loader deferrals can account for a missing test. The parser also discards the reason, accepting an ordinary unbound skip as a Build-Local deferral. Receipt readback omits the recorded notApplicable list from its comparison.",
      "blocking_rationale": "A development receipt can validate incomplete or contradictory test coverage.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each selected test has one distinct outcome. The loader case may be deferred once for the exact build, and the receipt agrees with the log.",
      "id": "R5-F2",
      "location": {
        "file": "scripts/DevelopmentCandidate.Common.ps1",
        "line": 160
      },
      "priority": "P2",
      "requested_correction": "Check uniqueness across passed and deferred outcomes, distinguish an intentional deferral from an unbound run, and compare any recorded notApplicable field with the parsed log. Preserve valid earlier receipts.",
      "requested_evidence": "Show refusal for a duplicate deferral replacing another pass, conflicting outcomes for the same case, and an unbound loader skip supplied as development-build evidence. Keep a valid deferred run followed by the exact-DLL pass.",
      "title": "Count each selected test exactly once across all outcomes",
      "trigger": "A selected-test log contains a repeated loader deferral or comes from an unbound direct run."
    },
    {
      "acceptance_ids": [],
      "actual": "create_deterministic_package.py prints the ZIP path. Its stdout flows through package.ps1 before the new measurement object, so assigning the command\u2019s output produces an array. Build-Local filters that array, and the fixture selects its last element, masking the public return shape.",
      "blocking_rationale": "A caller consuming or serializing the documented structured result receives a mixture of a path string and an object.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The opt-in measurement call emits one object containing the package path, digest and measured DLL identity.",
      "id": "R5-F3",
      "location": {
        "file": "scripts/package.ps1",
        "line": 354
      },
      "priority": "P2",
      "requested_correction": "Consume or redirect the ZIP writer\u2019s informational stdout while preserving its exit check. Return the documented value on the success stream.",
      "requested_evidence": "Capture the entire output of a successful -PassThruMeasurement call and assert a count of one, the object\u2019s fields and agreement with the on-disk checksum. Verify the ordinary path output separately.",
      "title": "Return a single package measurement on the success stream",
      "trigger": "A caller assigns the result of package.ps1 -PassThruMeasurement without filtering it."
    },
    {
      "acceptance_ids": [],
      "actual": "The fixture overwrites KMG_TEST_FILTER and removes it at lines 44, 72 and 82. Its final case seeds the four loader bindings and then clears them at line 85. It verifies that test-domain restored the fixture\u2019s seed values, but discards the values its own caller had supplied.",
      "blocking_rationale": "Running the new validation entry point can silently broaden the caller\u2019s next test run or erase its production-DLL binding.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The selection fixture restores the caller\u2019s filter and loader bindings on every exit.",
      "id": "R5-F4",
      "location": {
        "file": "scripts/Test-LoaderCaseSelection.ps1",
        "line": 41
      },
      "priority": "P2",
      "requested_correction": "Save all five incoming process variables before the first fixture mutation and restore them in an outer cleanup scope.",
      "requested_evidence": "Invoke the fixture in-process with pre-existing values and confirm restoration after success and a representative failure.",
      "title": "Restore the selection fixture\u2019s caller environment",
      "trigger": "Test-LoaderCaseSelection.ps1 runs in an existing PowerShell session with a filter or loader binding."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F3",
      "rationale": "Admission independently checks each recorded compiler field. The MVID-only mismatch was refused, and current and earlier-format controls were admitted."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F4",
      "rationale": "Both readers reject a fingerprint inconsistent with a clean producer, and the guide now distinguishes shipped documents. The ordinary writer can still stamp later-modified package inputs with its initial clean fingerprint. The fixture never produced a ZIP containing the modified input."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F3",
      "rationale": "The prepared product is compared file for file with the archive. Both requested pre-swap refusals preserved the sentinel and ZIP, and the genuine tree staged successfully. R5-F1 covers the separate failure path after the swap."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F3",
      "rationale": "The complete-suite Build-Local console reaches packaging. It records the deferred case, one exact candidate compile, the assertion passing on that output and one strict ZIP validation. Compiler and packaged identities agree."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F5",
      "rationale": "The ancestor check includes explicit directories. The standalone validator refused the directory below Info.json and accepted the ordinary directory control."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R4-F1",
      "rationale": "The detached receipt preserved an empty branch through deployment record construction. The named-branch control succeeded and removing the branch property caused refusal."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R4-F2",
      "rationale": "Unbound substring and prefix selections failed. The wrapper refused a missing DLL, the bound Windows case passed, and the unfiltered unbound and intentional deferral cases reported NOT APPLICABLE."
    },
    {
      "assessment": "still_open",
      "finding_id": "R4-F3",
      "rationale": "The child\u2019s source selects the correct mode and the report exposes it. The required ordinary child-to-report observation is still missing."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R4-F4",
      "rationale": "Removing the measurement-file destination eliminated the overwrite. The fixture matched the returned digest to the ZIP and checksum file. R5-F3 covers the separate return-shape defect."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R4-F5",
      "rationale": "test-domain encloses binding changes and private-copy preparation in its cleanup scope. The same-process digest refusal restored all four seed values and left the private-directory count unchanged. R5-F4 concerns the outer fixture."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R4-F6",
      "rationale": "The v0.0.188 historical record has null branch and source-state fields, sourceStateAvailable false, and the release tag in its dedicated field."
    }
  ],
  "request_id": "kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r05-cdfd83",
  "round": 5,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "TRE declares UMM 0.31.1, verifies the pinned UMM and Harmony12 compiler inputs, and checks the packaged DLL\u2019s actual assembly references. The frozen development DLL resolved statically against 0.31.1, 0.32.4, owner 0.33.0 and 0.33.4.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "The owner retains the decision on draft PR #220 after the blocking findings close and on scheduling any later guarded native qualification."
    ],
    "scope_reviewed": "Final correction review of TRE PR #220 at the fixed commit, including all eleven pending findings and the directly affected build, admission, staging and evidence consumers.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
