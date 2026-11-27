<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r07-review part=1/1 sha256=48c20b9c4334c5725718ee059b856ef1c844dfee24c5773b68701f3f5d300eb4 -->
### Codex GPT-6.2 Astra — review — round 7 — candidate `ffdd7fa7a5`

**Decision:** `changes_required` — The focused gate passed, and the four-loader audit matches the packaged DLL. Three issues block handoff. The receipt check skips shipped files whose Git and ZIP paths differ, it can inspect a different ZIP from the one whose digest it records, and the retained package lacks the files its admission reader requires.

## Continuation for Claude

Use the existing package copy rules to map ZIP entries to their producer paths. Compare their checkout bytes on the same archive whose SHA-256 is bound to the receipt. Exercise the remapped-file and archive-replacement cases alongside the existing README controls. Keep the final freeze at its receipt-bound location with its validation summary and selected-test log. Verify admission there after fixture cleanup, audit that packaged DLL on the four agreed profiles, and run the required focused gate on the submitted commit. Keep the historical captures and audits under their original identities. Use offline fixtures within the existing validation allowance and review cap.

#### Findings

**R1-F4** · P1 · defect · blocks readiness — Include remapped files in the producer comparison
- Location: `scripts/ReferenceProvenance.Common.ps1:374`; acceptance: —
- Trigger: A tracked icon changes for the copy into build output and its source is restored before the receipt decision.
- Expected: Each repository-sourced ZIP member resolves to its canonical source path and matches that commit's checkout bytes.
- Actual: Build-Local copies assets/game/icons to assets/icons. The new helper skips every entry whose ZIP-relative name is absent from the Git tree. For example, black-powder.png is tracked at the source path and checked by name in package validation, so altered pixels can pass the 159-file producer comparison. The same ordinal lookup skips an altered readme.md even though the Windows allowlist accepts that spelling.
- Why it blocks / does not: OWNER AMENDMENT 3 requires the receipt to verify the repository-sourced content actually shipped. A clean producer can still receive credit for different packaged bytes.
- Requested correction: Use the existing source-to-package mappings for copied files. Resolve accepted filename spellings to the canonical Git path or refuse them explicitly. Preserve the current checkout-byte comparison.
- Requested evidence: Package a changed remapped file, restore its source before the receipt decision, and show ordinary refusal naming the file. Cover a case-variant edited README and retain the clean-package and unshipped-note controls.

**R7-F1** · P2 · defect · blocks readiness — Bind the content check to the receipt's ZIP digest
- Location: `scripts/ReferenceProvenance.Common.ps1:368`; acceptance: —
- Trigger: The ZIP at the measured path is replaced between its digest check and the new member comparison.
- Expected: The ZIP digest recorded in the receipt and the member bytes compared with Git come from the same archive.
- Actual: Build-Local checks the measured ZIP digest at line 490. The content helper later reopens the path and receives no expected digest. A replacement clean ZIP can therefore pass the content check while the writer records the earlier ZIP's SHA-256. Both staging admission readers make the same separate reads.
- Why it blocks / does not: A successful file comparison can certify an archive different from the one identified by the receipt.
- Requested correction: Pass the expected package digest into the comparison. Hash and inspect members through one held read handle, or perform both checks on an existing verified private copy. Apply the binding in the writer and both staging readers.
- Requested evidence: Use the existing clean and edited fixture ZIPs to show that replacement after digest measurement is refused and that the matching archive succeeds.

**R7-F2** · P2 · defect · blocks readiness — Retain the files needed to admit the audited package
- Location: `docs/evidence/umm-0311/focused-r8-driver.ps1.txt:64`; acceptance: —
- Trigger: The owner consumes the frozen package after the focused driver removes its disposable clone.
- Expected: The final admissible freeze selected under OWNER AMENDMENT 3 remains consumable at the location identified for handoff.
- Actual: The driver copies only the ZIP, checksum and receipt to frozen-candidate. It runs admission in the original clone, then removes that clone in finally. The retained receipt names the original absolute package path. The reader also requires development-validation.json and selected-domain-tests.log with their recorded hashes. Neither accompanies the retained copy.
- Why it blocks / does not: The documented handoff cannot pass the existing development reader after cleanup. The static results remain valid for the bytes audited.
- Requested correction: Retain the final producer's canonical candidate layout and required evidence, or identify an existing complete retained layout. Preserve the exported driver as the record of this run and keep the reader's identity guards intact.
- Requested evidence: After fixture cleanup, run the development receipt reader with the correct purpose and RequireCurrentMaterialInputs at the retained location. Record the returned producer, ZIP digest, DLL digest and MVID alongside the matching audit.

**R7-F3** · P3 · optional_polish · non-blocking — Make the locked-copy fixture identify the rollback boundary
- Location: `scripts/Test-CandidateAdmission.ps1:501`; acceptance: —
- Trigger: A future staging error occurs before the swap while the fixture holds the admitted ZIP open.
- Expected: The rollback assertion distinguishes a restored previous tree from one that was never moved.
- Actual: The fixture labels every caught exception CLEANUP REFUSED and checks the final sentinel. An earlier refusal also leaves that sentinel in place, and the fallback cleanup can emit the same locked-file error.
- Why it blocks / does not: The current implementation places cleanup after destination verification, and the retained run reports the expected sharing violation. A more specific assertion would distinguish future regressions.
- Requested correction: Record successful verification of the destination DLL in the locked-copy case and require the removal error to identify the admitted ZIP.
- Requested evidence: The focused fixture records destination verification, the cleanup refusal and the restored sentinel in order.

#### Prior finding assessments

- **R1-F4** — `fix_rejected` — The requested canonical README reproduction now reaches the ordinary reader after restoration and fails for the packaged edit. Both controls pass, and the guide corrects its earlier attribution. The same producer-binding obligation remains open for shipped Git files omitted by the path lookup.
- **R6-F1** — `fix_verified` — All 37 exported digests match their committed blobs. The index distinguishes original captures, converted intermediates and committed bytes. The exit trailers are readable, and the original 467-second failure remains of unknown cause. The later host-guard refusal has its own captured error and exit status.
- **R6-F2** — `fix_verified` — The audit matches the c0b54ba4ad package receipt's DLL digest and MVID. It records resolution for 0.31.1, 0.32.4, owner 0.33.0 and 0.33.4. The 49be3fd0e2 audit retains its original identity. R7-F2 covers the separate loss of the admission files after this run.
- **R6-F3** — `fix_verified` — Private-copy removal now executes inside the rollback scope while the previous Mods tree is available. The locked-ZIP fixture recorded the sharing violation, the original sentinel text at Mods, no replacement product there and successful cleanup after handle release. The successful-swap control also passed.

#### Evidence assessment

- **Inspected:** The effective TRE mission, all three owner amendments, repository instructions, pending findings, developer report and controller evidence.; The 71-file feature diff and the corrections since a38e38ec53. The controller's diff matches the Git diff byte for byte. Only documentation and evidence changed between producer c0b54ba4ad and H.; The package copy rules, source comparison, ordinary and development receipt readers, staging rollback, focused driver and retained evidence.
- **Executed verification:** The controller receipt and matching log SHA-256 confirm the focused gate at ffdd7fa7a5. Production compiled against UMM 0.31.1.0 and Harmony12 1.2.0.1. The named loader-reference test selected one of 3,189 tests and passed in a 5.0-second gate.; The retained c0b54ba4ad build log records exit 0 after 441 seconds and strict package validation. The focused fixture record reports 11/11, including ordinary-reader refusal of the packaged README edit after source restoration, both clean controls, the locked-ZIP rollback and the successful swap.; Independently verified all 37 indexed exported SHA-256 values against Git blob bytes at H. The two corrected logs end in readable exit 1 and exit 0 records. The checked-out fixture digest also matches the focused record.; Cross-checked the build receipt, focused record and static audit. The ZIP SHA-256 is d86d9a7cdd4a66f763b8ee77f8c1e22c67e54c0a8feb844af35e29d6ec12caa1. All four profiles resolved 23 types and 67 members on DLL SHA-256 71dfdd7231e8b5858aa27c36d0c0022c3a1a421c9621081a46e807e3bfe9c281, MVID d055b414-77a8-4373-a5a2-46af8a140424.
- **Source inspection only:** Git lists 298 tracked files under assets/game/icons and none under the shipped assets/icons prefix. Traced the missing comparisons through Build-Local, package.ps1 and validate-package.ps1.; Traced the separate ZIP opens in receipt writing and admission, and compared the focused driver's retention step with the development reader's path and evidence requirements. These failure paths were not executed during review.
- **Required gaps:** Ordinary receipt refusal for changed repository content shipped under a different path, including filename spellings accepted by the Windows validator.; Evidence that the member comparison and receipt digest identify the same archive when the package path changes.; Successful admission of the final retained freeze from its supported location after disposable fixture cleanup.
- **Limitations:** The controller withheld the mixed-project foreground transcript. Verification beyond its gate receipt used the committed run outputs and source. The machine-local ZIP and original captures were outside the review snapshot.; The earlier admission suite remains 62/63 at 42e9da86d6. The later ordinary-admission and caller-setting checks belong to b16ce7a47c. Native and combined-stack checks are recorded as NOT RUN.

#### Summary

- **Delivered behavior:** TRE declares UMM 0.31.1, compiles against the pinned loader, and has a four-profile static audit on one identified packaged DLL. The evidence index and requested private-copy rollback are verified. Package provenance and retained admission still need correction.
- **Scope reviewed:** Final correction review for TRE PR #220 under mission amendment revision 3, covering the four pending findings and the package, receipt and staging paths affected by their corrections.
- **Pending later-phase work:** none
- **Unresolved questions:** none
- **Remaining owner review:** Owner acceptance of draft PR #220 after the required corrections.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: revision 3, sha256 `9097a57f4916be5ded05516c07c171aa348a98820c6c4fb272d692a68282a69e`.

Codex CLI `0.170.0`, thread `01a21a44-a263-7930-995e-2e0b59c2ca3c`, duration 738.0s, response sha256 `47b04e0d2fced29a0b6a063f97257d4efbbac5c35997fdddff9b862e19078185`, request `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r07-c9fb20`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "ffdd7fa7a5a4db10741cb0def47e0885c8e96835",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The focused gate passed, and the four-loader audit matches the packaged DLL. Three issues block handoff. The receipt check skips shipped files whose Git and ZIP paths differ, it can inspect a different ZIP from the one whose digest it records, and the retained package lacks the files its admission reader requires.\n\n## Continuation for Claude\n\nUse the existing package copy rules to map ZIP entries to their producer paths. Compare their checkout bytes on the same archive whose SHA-256 is bound to the receipt. Exercise the remapped-file and archive-replacement cases alongside the existing README controls. Keep the final freeze at its receipt-bound location with its validation summary and selected-test log. Verify admission there after fixture cleanup, audit that packaged DLL on the four agreed profiles, and run the required focused gate on the submitted commit. Keep the historical captures and audits under their original identities. Use offline fixtures within the existing validation allowance and review cap.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt and matching log SHA-256 confirm the focused gate at ffdd7fa7a5. Production compiled against UMM 0.31.1.0 and Harmony12 1.2.0.1. The named loader-reference test selected one of 3,189 tests and passed in a 5.0-second gate.",
      "The retained c0b54ba4ad build log records exit 0 after 441 seconds and strict package validation. The focused fixture record reports 11/11, including ordinary-reader refusal of the packaged README edit after source restoration, both clean controls, the locked-ZIP rollback and the successful swap.",
      "Independently verified all 37 indexed exported SHA-256 values against Git blob bytes at H. The two corrected logs end in readable exit 1 and exit 0 records. The checked-out fixture digest also matches the focused record.",
      "Cross-checked the build receipt, focused record and static audit. The ZIP SHA-256 is d86d9a7cdd4a66f763b8ee77f8c1e22c67e54c0a8feb844af35e29d6ec12caa1. All four profiles resolved 23 types and 67 members on DLL SHA-256 71dfdd7231e8b5858aa27c36d0c0022c3a1a421c9621081a46e807e3bfe9c281, MVID d055b414-77a8-4373-a5a2-46af8a140424."
    ],
    "inspected": [
      "The effective TRE mission, all three owner amendments, repository instructions, pending findings, developer report and controller evidence.",
      "The 71-file feature diff and the corrections since a38e38ec53. The controller's diff matches the Git diff byte for byte. Only documentation and evidence changed between producer c0b54ba4ad and H.",
      "The package copy rules, source comparison, ordinary and development receipt readers, staging rollback, focused driver and retained evidence."
    ],
    "limitations": [
      "The controller withheld the mixed-project foreground transcript. Verification beyond its gate receipt used the committed run outputs and source. The machine-local ZIP and original captures were outside the review snapshot.",
      "The earlier admission suite remains 62/63 at 42e9da86d6. The later ordinary-admission and caller-setting checks belong to b16ce7a47c. Native and combined-stack checks are recorded as NOT RUN."
    ],
    "required_gaps": [
      "Ordinary receipt refusal for changed repository content shipped under a different path, including filename spellings accepted by the Windows validator.",
      "Evidence that the member comparison and receipt digest identify the same archive when the package path changes.",
      "Successful admission of the final retained freeze from its supported location after disposable fixture cleanup."
    ],
    "source_inspection_only": [
      "Git lists 298 tracked files under assets/game/icons and none under the shipped assets/icons prefix. Traced the missing comparisons through Build-Local, package.ps1 and validate-package.ps1.",
      "Traced the separate ZIP opens in receipt writing and admission, and compared the focused driver's retention step with the development reader's path and evidence requirements. These failure paths were not executed during review."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [],
      "actual": "Build-Local copies assets/game/icons to assets/icons. The new helper skips every entry whose ZIP-relative name is absent from the Git tree. For example, black-powder.png is tracked at the source path and checked by name in package validation, so altered pixels can pass the 159-file producer comparison. The same ordinal lookup skips an altered readme.md even though the Windows allowlist accepts that spelling.",
      "blocking_rationale": "OWNER AMENDMENT 3 requires the receipt to verify the repository-sourced content actually shipped. A clean producer can still receive credit for different packaged bytes.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each repository-sourced ZIP member resolves to its canonical source path and matches that commit's checkout bytes.",
      "id": "R1-F4",
      "location": {
        "file": "scripts/ReferenceProvenance.Common.ps1",
        "line": 374
      },
      "priority": "P1",
      "requested_correction": "Use the existing source-to-package mappings for copied files. Resolve accepted filename spellings to the canonical Git path or refuse them explicitly. Preserve the current checkout-byte comparison.",
      "requested_evidence": "Package a changed remapped file, restore its source before the receipt decision, and show ordinary refusal naming the file. Cover a case-variant edited README and retain the clean-package and unshipped-note controls.",
      "title": "Include remapped files in the producer comparison",
      "trigger": "A tracked icon changes for the copy into build output and its source is restored before the receipt decision."
    },
    {
      "acceptance_ids": [],
      "actual": "Build-Local checks the measured ZIP digest at line 490. The content helper later reopens the path and receives no expected digest. A replacement clean ZIP can therefore pass the content check while the writer records the earlier ZIP's SHA-256. Both staging admission readers make the same separate reads.",
      "blocking_rationale": "A successful file comparison can certify an archive different from the one identified by the receipt.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The ZIP digest recorded in the receipt and the member bytes compared with Git come from the same archive.",
      "id": "R7-F1",
      "location": {
        "file": "scripts/ReferenceProvenance.Common.ps1",
        "line": 368
      },
      "priority": "P2",
      "requested_correction": "Pass the expected package digest into the comparison. Hash and inspect members through one held read handle, or perform both checks on an existing verified private copy. Apply the binding in the writer and both staging readers.",
      "requested_evidence": "Use the existing clean and edited fixture ZIPs to show that replacement after digest measurement is refused and that the matching archive succeeds.",
      "title": "Bind the content check to the receipt's ZIP digest",
      "trigger": "The ZIP at the measured path is replaced between its digest check and the new member comparison."
    },
    {
      "acceptance_ids": [],
      "actual": "The driver copies only the ZIP, checksum and receipt to frozen-candidate. It runs admission in the original clone, then removes that clone in finally. The retained receipt names the original absolute package path. The reader also requires development-validation.json and selected-domain-tests.log with their recorded hashes. Neither accompanies the retained copy.",
      "blocking_rationale": "The documented handoff cannot pass the existing development reader after cleanup. The static results remain valid for the bytes audited.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The final admissible freeze selected under OWNER AMENDMENT 3 remains consumable at the location identified for handoff.",
      "id": "R7-F2",
      "location": {
        "file": "docs/evidence/umm-0311/focused-r8-driver.ps1.txt",
        "line": 64
      },
      "priority": "P2",
      "requested_correction": "Retain the final producer's canonical candidate layout and required evidence, or identify an existing complete retained layout. Preserve the exported driver as the record of this run and keep the reader's identity guards intact.",
      "requested_evidence": "After fixture cleanup, run the development receipt reader with the correct purpose and RequireCurrentMaterialInputs at the retained location. Record the returned producer, ZIP digest, DLL digest and MVID alongside the matching audit.",
      "title": "Retain the files needed to admit the audited package",
      "trigger": "The owner consumes the frozen package after the focused driver removes its disposable clone."
    },
    {
      "acceptance_ids": [],
      "actual": "The fixture labels every caught exception CLEANUP REFUSED and checks the final sentinel. An earlier refusal also leaves that sentinel in place, and the fallback cleanup can emit the same locked-file error.",
      "blocking_rationale": "The current implementation places cleanup after destination verification, and the retained run reports the expected sharing violation. A more specific assertion would distinguish future regressions.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The rollback assertion distinguishes a restored previous tree from one that was never moved.",
      "id": "R7-F3",
      "location": {
        "file": "scripts/Test-CandidateAdmission.ps1",
        "line": 501
      },
      "priority": "P3",
      "requested_correction": "Record successful verification of the destination DLL in the locked-copy case and require the removal error to identify the admitted ZIP.",
      "requested_evidence": "The focused fixture records destination verification, the cleanup refusal and the restored sentinel in order.",
      "title": "Make the locked-copy fixture identify the rollback boundary",
      "trigger": "A future staging error occurs before the swap while the fixture holds the admitted ZIP open."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F4",
      "rationale": "The requested canonical README reproduction now reaches the ordinary reader after restoration and fails for the packaged edit. Both controls pass, and the guide corrects its earlier attribution. The same producer-binding obligation remains open for shipped Git files omitted by the path lookup."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R6-F1",
      "rationale": "All 37 exported digests match their committed blobs. The index distinguishes original captures, converted intermediates and committed bytes. The exit trailers are readable, and the original 467-second failure remains of unknown cause. The later host-guard refusal has its own captured error and exit status."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R6-F2",
      "rationale": "The audit matches the c0b54ba4ad package receipt's DLL digest and MVID. It records resolution for 0.31.1, 0.32.4, owner 0.33.0 and 0.33.4. The 49be3fd0e2 audit retains its original identity. R7-F2 covers the separate loss of the admission files after this run."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R6-F3",
      "rationale": "Private-copy removal now executes inside the rollback scope while the previous Mods tree is available. The locked-ZIP fixture recorded the sharing violation, the original sentinel text at Mods, no replacement product there and successful cleanup after handle release. The successful-swap control also passed."
    }
  ],
  "request_id": "kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r07-c9fb20",
  "round": 7,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "TRE declares UMM 0.31.1, compiles against the pinned loader, and has a four-profile static audit on one identified packaged DLL. The evidence index and requested private-copy rollback are verified. Package provenance and retained admission still need correction.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Owner acceptance of draft PR #220 after the required corrections."
    ],
    "scope_reviewed": "Final correction review for TRE PR #220 under mission amendment revision 3, covering the four pending findings and the package, receipt and staging paths affected by their corrections.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
