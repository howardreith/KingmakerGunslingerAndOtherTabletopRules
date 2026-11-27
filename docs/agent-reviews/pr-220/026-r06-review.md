<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r06-review part=1/1 sha256=9bcd57d4fb65b8298b37e87c0995187811b4e3216efbee5a81cb8b15eb1a06c1 -->
### Codex GPT-6.2 Astra — review — round 6 — candidate `a38e38ec53`

**Decision:** `changes_required` — The focused gate passed and five prior fixes are verified. R1-F4 remains open because a restored working tree can produce a clean receipt for a ZIP containing an edited file. The review also found a staging cleanup failure after rollback data is deleted, five incorrect evidence hashes, and a four-loader audit that covers an earlier DLL.

Continuation for Claude

Bind ordinary receipt creation or admission to the shipped bytes that entered the ZIP. Exercise the README case with the source restored before the receipt check. Keep the previous Mods tree available through fallible private-copy cleanup and exercise that failure on a disposable directory. Correct the evidence index from the committed blobs, recover the readable build exit records, and bind the existing four-profile static audit to the admissible frozen package selected for handoff. Preserve the earlier 62/63 result and the later focused successes under their producing commits.

Use retained artifacts and narrow offline checks. Run the required focused gate on the corrected commit. Check the remaining OWNER AMENDMENT 2 allowance before starting validation and report a budget blocker if the required work cannot fit. Native gameplay, combined-stack testing, merge and release remain outside this continuation.

#### Findings

**R1-F4** · P1 · defect · blocks readiness — Bind the ordinary receipt to the inputs that entered the package
- Location: `scripts/Build-Local.ps1:504`; acceptance: —
- Trigger: A shipped README changes after the starting fingerprint, enters staging, and is restored before Build-Local's final fingerprint.
- Expected: A receipt accepted for staging binds the producing commit to the material inputs actually consumed.
- Actual: Build-Local compares the final working-tree fingerprint with its starting value. Both are clean in this sequence, while the ZIP retains the edited README. The new fixture calls the check while the README is still dirty, restores it afterward, then observes a missing development receipt on a separately assembled ZIP. The compatibility guide also still attributes a real modified-input package to the round-five fixture, which changed only the receipt.
- Why it blocks / does not: Ordinary admission can accept shipped content under a clean producer identity that does not describe it.
- Requested correction: Use the existing package inputs to verify the shipped content against its producer, or consume immutable verified inputs. Correct the compatibility guide's attribution of the round-five and round-six checks.
- Requested evidence: Package an edited README, restore the source before the receipt decision, and show refusal for the packaged content through the ordinary writer or reader. Retain the clean and unshipped-review-note controls.

**R6-F1** · P2 · defect · blocks readiness — Make the evidence index reproducible from the committed files
- Location: `docs/evidence/umm-0311/INDEX.md:20`; acceptance: —
- Trigger: A reviewer follows the index instructions and hashes the evidence with git cat-file blob.
- Expected: Each exported SHA-256 identifies the named Git blob, and the build exports preserve readable exit status.
- Actual: Five rows contain CRLF digests for files committed with LF. For ordinary-admission-r7.txt the indexed value starts 7a3545d0, while the Git blob hashes to 5082915454f4ea530c57098b6fe41c91ef561c3f4c2eee6b96ed64353263100b. The other affected rows are candidate-admission-r6.json, loader-case-selection-r7.json and both selection-restore-r7 text records. The two build logs also contain doubled carriage returns and misdecoded exit trailers.
- Why it blocks / does not: The documented verification command rejects evidence used to close the pending findings. Accurate artifact identities are required for handoff.
- Requested correction: Calculate exported digests from Git blob bytes. Distinguish original captures from intermediate converted files and correct or annotate the mixed-encoding exit trailers using the retained data.
- Requested evidence: Verify every indexed digest against the corrected commit and show the recovered exit 1 and exit 0 records. The existing captures suffice for this correction.

**R6-F2** · P2 · evidence_gap · blocks readiness — Bind the four-loader static assessment to the current package
- Location: `docs/UMM-LOADER-COMPATIBILITY.md:21`; acceptance: —
- Trigger: The round-five static audit is used for final acceptance after the corrected build produces a different packaged DLL.
- Expected: The static comparison for the current candidate covers the same frozen packaged DLL on all four agreed profiles.
- Actual: api-audit.json measures DLL 3507c755 from 49be3fd0e2. The later validated b16ce7a47c package contains DLL f34146a3. The guide still calls the earlier freeze current, although material build and packaging scripts have changed. The focused gate checks assembly references and supplies no member-resolution results for the later DLL.
- Why it blocks / does not: The four loader profiles have valid historical results, but the required static assessment does not identify an artifact admissible as current at this checkpoint.
- Requested correction: Use the existing metadata audit on the retained admissible package selected for handoff, or supply an existing audit for those exact bytes. Update the current-candidate table and preserve the 49be3fd0e2 result under its original identity.
- Requested evidence: Provide the producer commit, ZIP SHA-256, extracted DLL SHA-256 and MVID alongside the results for 0.31.1, 0.32.4, owner 0.33.0 and 0.33.4.

**R6-F3** · P2 · defect · blocks readiness — Retain the previous Mods tree through private-copy cleanup
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:317`; acceptance: —
- Trigger: The destination DLL check succeeds, but Windows refuses deletion of the private admitted ZIP.
- Expected: A cleanup failure reported by the staging call leaves the previous target available for restoration.
- Actual: Line 317 removes the previous Mods tree. The finally block at line 321 then recursively deletes the admission directory with terminating errors enabled. A read handle that permits hashing but denies deletion lets destination verification pass and makes cleanup throw. The caller receives a staging error after the replacement is in place and the previous tree is gone.
- Why it blocks / does not: A failure inside the staging call can leave the shared disposable target changed with no rollback copy.
- Requested correction: Complete private-copy cleanup after its last read and before discarding the previous tree. Keep cleanup failure within the existing rollback scope.
- Requested evidence: On a disposable directory, hold the admitted ZIP open for reading without delete sharing. Confirm that cleanup failure leaves the previous sentinel at Mods. Keep the existing successful-swap control.

#### Prior finding assessments

- **R1-F4** — `fix_rejected` — The recorded refusal is valid while the README remains edited. Restoring it before the final fingerprint leaves the packaged edit undetected. The fixture restores it only after the check.
- **R4-F3** — `fix_verified` — The explicitly identified ordinary-format fixture reached the real child. Its exit 0, build-local mode, b16ce7a47c producer and a0541197 ZIP digest agree with the Get-LcReportHead projection.
- **R5-F1** — `fix_verified` — The previous tree survives the destination digest check. The injected verification failure restored the sentinel and the genuine swap passed. R6-F3 covers the later private-copy cleanup failure.
- **R5-F2** — `fix_verified` — The reader enforces unique names across passes and deferrals, requires the exact Build-Local reason and compares a recorded deferral list with the log. The 98/0 contract run includes the negative cases, and the earlier valid receipt agrees with its log.
- **R5-F3** — `fix_verified` — The ZIP writer's informational output goes to the host. The fixture captured one measurement object and one ordinary path on their respective success streams, and the measured digest matched both ZIP and checksum.
- **R5-F4** — `fix_verified` — The outer finally restores the incoming filter and four loader bindings. The records show all five original values after success, a failed check and interruption while the fixture's filter was active.

#### Evidence assessment

- **Inspected:** The controller's mission, both owner amendments, checkpoint report, transcript notice, prior-finding ledger, gate receipt, changed-file list and M..H feature diff.; AGENTS.md, the validation policy, the agent-review workflow, the production project and pin checks, and the build, packaging, receipt, admission, staging and report paths affected by the corrections.; The compatibility guide, evidence index, static audit and profile identities, both new build logs, admission records, selection records and their producing commits.
- **Executed verification:** The controller receipt records production compilation at H with UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1. The named loader test passed, with one of 3,189 tests selected, zero failures and a 4.2-second gate.; At b16ce7a47c, the exported Build-Local log records strict validation of ZIP a0541197a979f18b9f7b99172e7ca4314c1be78a52d98699f1fb8b09a88d53b0 containing DLL f34146a3ab02d2d0582d5eba327fbf9080c9b3b162b7f0ac0be91c0410bfc5b0. The ordinary admission child exited 0 and the report projected its mode, producer commit and ZIP digest.; The 42e9da86d6 admission suite passed 62 of 63 cases. Its destination-verification rollback, genuine swap, package return-shape and changed-README checks passed. Its ordinary child-output extraction case failed. The later focused record covers that case.; The b16ce7a47c build log records 98 passing development-candidate contract checks and zero failures. Independent readback of the earlier complete-suite log found 3,188 distinct passes and one exact loader deferral, matching the receipt's pass-name digest and deferral list.; The selection fixture at b16ce7a47c passed 7/7 from the TRE checkout. Its driver recorded restoration of all five seeded settings after success and interruption. A separate failed run from another working directory also restored all five.; I verified the mission and amendment digests, confirmed that only evidence files differ between b16ce7a47c and H, and hashed all 31 indexed Git blobs. Twenty-six exported digests matched. The five mismatches all reproduce with CRLF bytes.
- **Source inspection only:** The final source-state check reads the working tree. The ordinary admission path verifies the ZIP digest and DLL identity but does not compare the packaged README with its producer.; Set-LcGameCopyMods deletes its rollback directory before Remove-LcAdmittedCopy runs. The caller treats an exception from that cleanup as a staging failure.; The material-input comparison includes Build-Local.ps1 and package.ps1. Both differ between the audited 49be3fd0e2 freeze and H.
- **Required gaps:** An ordinary receipt or admission check that rejects altered packaged content after the source has already been restored.; A disposable staging check showing that a private-copy cleanup failure preserves the previous Mods tree.; Reproducible exported hashes and readable exit records for the new evidence.; Four-profile static results bound to the admissible frozen package selected for this checkpoint.
- **Limitations:** The foreground developer transcript is withheld. The controller gate receipt and committed offline outputs provide the execution evidence. This read-only review performed no build or runtime transaction.; No output survived the 467-second attempt, so its cause is unknown. The later failed build records a running Kingmaker process. The two exported build-log trailers decode to exit 1 for that refusal and exit 0 for the successful build.; Native gameplay and combined-stack testing were NOT RUN under OWNER AMENDMENT 2. The four prior profile results establish static API resolution for DLL 3507c755 only.

#### Summary

- **Delivered behavior:** TRE declares UMM 0.31.1, compiles against UnityModManager 0.31.1.0 and Harmony12 1.2.0.1, and checks the packaged DLL's references. Ordinary admission reporting, selected-test accounting, package measurement output, destination-verification rollback and caller-setting restoration have focused evidence.
- **Scope reviewed:** Final correction review of TRE PR #220 at the fixed candidate, under the original repository mission and OWNER AMENDMENTS 1 and 2.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** The owner decides the disposition of draft PR #220 after the required corrections and independent handoff.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: revision 2, sha256 `d08e405955f822368d58f57523fad79873f181d777aeedf8168981e04446ccf4`.

Codex CLI `0.170.0`, thread `01a218f3-c70d-7562-803b-ada99953f6a7`, duration 736.9s, response sha256 `9f1c1dbd709d4adc1ff12b478fee0610c0b4b8cfbaa8524143ed60f4a470dd24`, request `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r06-9c4457`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "a38e38ec5357f5abdfdd585a64e63aae459ee95d",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The focused gate passed and five prior fixes are verified. R1-F4 remains open because a restored working tree can produce a clean receipt for a ZIP containing an edited file. The review also found a staging cleanup failure after rollback data is deleted, five incorrect evidence hashes, and a four-loader audit that covers an earlier DLL.\n\nContinuation for Claude\n\nBind ordinary receipt creation or admission to the shipped bytes that entered the ZIP. Exercise the README case with the source restored before the receipt check. Keep the previous Mods tree available through fallible private-copy cleanup and exercise that failure on a disposable directory. Correct the evidence index from the committed blobs, recover the readable build exit records, and bind the existing four-profile static audit to the admissible frozen package selected for handoff. Preserve the earlier 62/63 result and the later focused successes under their producing commits.\n\nUse retained artifacts and narrow offline checks. Run the required focused gate on the corrected commit. Check the remaining OWNER AMENDMENT 2 allowance before starting validation and report a budget blocker if the required work cannot fit. Native gameplay, combined-stack testing, merge and release remain outside this continuation.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt records production compilation at H with UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1. The named loader test passed, with one of 3,189 tests selected, zero failures and a 4.2-second gate.",
      "At b16ce7a47c, the exported Build-Local log records strict validation of ZIP a0541197a979f18b9f7b99172e7ca4314c1be78a52d98699f1fb8b09a88d53b0 containing DLL f34146a3ab02d2d0582d5eba327fbf9080c9b3b162b7f0ac0be91c0410bfc5b0. The ordinary admission child exited 0 and the report projected its mode, producer commit and ZIP digest.",
      "The 42e9da86d6 admission suite passed 62 of 63 cases. Its destination-verification rollback, genuine swap, package return-shape and changed-README checks passed. Its ordinary child-output extraction case failed. The later focused record covers that case.",
      "The b16ce7a47c build log records 98 passing development-candidate contract checks and zero failures. Independent readback of the earlier complete-suite log found 3,188 distinct passes and one exact loader deferral, matching the receipt's pass-name digest and deferral list.",
      "The selection fixture at b16ce7a47c passed 7/7 from the TRE checkout. Its driver recorded restoration of all five seeded settings after success and interruption. A separate failed run from another working directory also restored all five.",
      "I verified the mission and amendment digests, confirmed that only evidence files differ between b16ce7a47c and H, and hashed all 31 indexed Git blobs. Twenty-six exported digests matched. The five mismatches all reproduce with CRLF bytes."
    ],
    "inspected": [
      "The controller's mission, both owner amendments, checkpoint report, transcript notice, prior-finding ledger, gate receipt, changed-file list and M..H feature diff.",
      "AGENTS.md, the validation policy, the agent-review workflow, the production project and pin checks, and the build, packaging, receipt, admission, staging and report paths affected by the corrections.",
      "The compatibility guide, evidence index, static audit and profile identities, both new build logs, admission records, selection records and their producing commits."
    ],
    "limitations": [
      "The foreground developer transcript is withheld. The controller gate receipt and committed offline outputs provide the execution evidence. This read-only review performed no build or runtime transaction.",
      "No output survived the 467-second attempt, so its cause is unknown. The later failed build records a running Kingmaker process. The two exported build-log trailers decode to exit 1 for that refusal and exit 0 for the successful build.",
      "Native gameplay and combined-stack testing were NOT RUN under OWNER AMENDMENT 2. The four prior profile results establish static API resolution for DLL 3507c755 only."
    ],
    "required_gaps": [
      "An ordinary receipt or admission check that rejects altered packaged content after the source has already been restored.",
      "A disposable staging check showing that a private-copy cleanup failure preserves the previous Mods tree.",
      "Reproducible exported hashes and readable exit records for the new evidence.",
      "Four-profile static results bound to the admissible frozen package selected for this checkpoint."
    ],
    "source_inspection_only": [
      "The final source-state check reads the working tree. The ordinary admission path verifies the ZIP digest and DLL identity but does not compare the packaged README with its producer.",
      "Set-LcGameCopyMods deletes its rollback directory before Remove-LcAdmittedCopy runs. The caller treats an exception from that cleanup as a staging failure.",
      "The material-input comparison includes Build-Local.ps1 and package.ps1. Both differ between the audited 49be3fd0e2 freeze and H."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [],
      "actual": "Build-Local compares the final working-tree fingerprint with its starting value. Both are clean in this sequence, while the ZIP retains the edited README. The new fixture calls the check while the README is still dirty, restores it afterward, then observes a missing development receipt on a separately assembled ZIP. The compatibility guide also still attributes a real modified-input package to the round-five fixture, which changed only the receipt.",
      "blocking_rationale": "Ordinary admission can accept shipped content under a clean producer identity that does not describe it.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A receipt accepted for staging binds the producing commit to the material inputs actually consumed.",
      "id": "R1-F4",
      "location": {
        "file": "scripts/Build-Local.ps1",
        "line": 504
      },
      "priority": "P1",
      "requested_correction": "Use the existing package inputs to verify the shipped content against its producer, or consume immutable verified inputs. Correct the compatibility guide's attribution of the round-five and round-six checks.",
      "requested_evidence": "Package an edited README, restore the source before the receipt decision, and show refusal for the packaged content through the ordinary writer or reader. Retain the clean and unshipped-review-note controls.",
      "title": "Bind the ordinary receipt to the inputs that entered the package",
      "trigger": "A shipped README changes after the starting fingerprint, enters staging, and is restored before Build-Local's final fingerprint."
    },
    {
      "acceptance_ids": [],
      "actual": "Five rows contain CRLF digests for files committed with LF. For ordinary-admission-r7.txt the indexed value starts 7a3545d0, while the Git blob hashes to 5082915454f4ea530c57098b6fe41c91ef561c3f4c2eee6b96ed64353263100b. The other affected rows are candidate-admission-r6.json, loader-case-selection-r7.json and both selection-restore-r7 text records. The two build logs also contain doubled carriage returns and misdecoded exit trailers.",
      "blocking_rationale": "The documented verification command rejects evidence used to close the pending findings. Accurate artifact identities are required for handoff.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each exported SHA-256 identifies the named Git blob, and the build exports preserve readable exit status.",
      "id": "R6-F1",
      "location": {
        "file": "docs/evidence/umm-0311/INDEX.md",
        "line": 20
      },
      "priority": "P2",
      "requested_correction": "Calculate exported digests from Git blob bytes. Distinguish original captures from intermediate converted files and correct or annotate the mixed-encoding exit trailers using the retained data.",
      "requested_evidence": "Verify every indexed digest against the corrected commit and show the recovered exit 1 and exit 0 records. The existing captures suffice for this correction.",
      "title": "Make the evidence index reproducible from the committed files",
      "trigger": "A reviewer follows the index instructions and hashes the evidence with git cat-file blob."
    },
    {
      "acceptance_ids": [],
      "actual": "api-audit.json measures DLL 3507c755 from 49be3fd0e2. The later validated b16ce7a47c package contains DLL f34146a3. The guide still calls the earlier freeze current, although material build and packaging scripts have changed. The focused gate checks assembly references and supplies no member-resolution results for the later DLL.",
      "blocking_rationale": "The four loader profiles have valid historical results, but the required static assessment does not identify an artifact admissible as current at this checkpoint.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "The static comparison for the current candidate covers the same frozen packaged DLL on all four agreed profiles.",
      "id": "R6-F2",
      "location": {
        "file": "docs/UMM-LOADER-COMPATIBILITY.md",
        "line": 21
      },
      "priority": "P2",
      "requested_correction": "Use the existing metadata audit on the retained admissible package selected for handoff, or supply an existing audit for those exact bytes. Update the current-candidate table and preserve the 49be3fd0e2 result under its original identity.",
      "requested_evidence": "Provide the producer commit, ZIP SHA-256, extracted DLL SHA-256 and MVID alongside the results for 0.31.1, 0.32.4, owner 0.33.0 and 0.33.4.",
      "title": "Bind the four-loader static assessment to the current package",
      "trigger": "The round-five static audit is used for final acceptance after the corrected build produces a different packaged DLL."
    },
    {
      "acceptance_ids": [],
      "actual": "Line 317 removes the previous Mods tree. The finally block at line 321 then recursively deletes the admission directory with terminating errors enabled. A read handle that permits hashing but denies deletion lets destination verification pass and makes cleanup throw. The caller receives a staging error after the replacement is in place and the previous tree is gone.",
      "blocking_rationale": "A failure inside the staging call can leave the shared disposable target changed with no rollback copy.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A cleanup failure reported by the staging call leaves the previous target available for restoration.",
      "id": "R6-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 317
      },
      "priority": "P2",
      "requested_correction": "Complete private-copy cleanup after its last read and before discarding the previous tree. Keep cleanup failure within the existing rollback scope.",
      "requested_evidence": "On a disposable directory, hold the admitted ZIP open for reading without delete sharing. Confirm that cleanup failure leaves the previous sentinel at Mods. Keep the existing successful-swap control.",
      "title": "Retain the previous Mods tree through private-copy cleanup",
      "trigger": "The destination DLL check succeeds, but Windows refuses deletion of the private admitted ZIP."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F4",
      "rationale": "The recorded refusal is valid while the README remains edited. Restoring it before the final fingerprint leaves the packaged edit undetected. The fixture restores it only after the check."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R4-F3",
      "rationale": "The explicitly identified ordinary-format fixture reached the real child. Its exit 0, build-local mode, b16ce7a47c producer and a0541197 ZIP digest agree with the Get-LcReportHead projection."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R5-F1",
      "rationale": "The previous tree survives the destination digest check. The injected verification failure restored the sentinel and the genuine swap passed. R6-F3 covers the later private-copy cleanup failure."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R5-F2",
      "rationale": "The reader enforces unique names across passes and deferrals, requires the exact Build-Local reason and compares a recorded deferral list with the log. The 98/0 contract run includes the negative cases, and the earlier valid receipt agrees with its log."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R5-F3",
      "rationale": "The ZIP writer's informational output goes to the host. The fixture captured one measurement object and one ordinary path on their respective success streams, and the measured digest matched both ZIP and checksum."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R5-F4",
      "rationale": "The outer finally restores the incoming filter and four loader bindings. The records show all five original values after success, a failed check and interruption while the fixture's filter was active."
    }
  ],
  "request_id": "kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r06-9c4457",
  "round": 6,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "TRE declares UMM 0.31.1, compiles against UnityModManager 0.31.1.0 and Harmony12 1.2.0.1, and checks the packaged DLL's references. Ordinary admission reporting, selected-test accounting, package measurement output, destination-verification rollback and caller-setting restoration have focused evidence.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "The owner decides the disposition of draft PR #220 after the required corrections and independent handoff."
    ],
    "scope_reviewed": "Final correction review of TRE PR #220 at the fixed candidate, under the original repository mission and OWNER AMENDMENTS 1 and 2.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
