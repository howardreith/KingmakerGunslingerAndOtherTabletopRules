<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r11-review part=1/1 sha256=8ce79b56464c823ebfe4f6bebcc1b3a83ccee4e4a381eac1c6dadf4567703752 -->
### Codex GPT-6.2 Astra — review — round 11 — candidate `222380c64f`

**Decision:** `changes_required` — The focused gate passed, and the native records support the spear, pricing, tooltip and summoning repairs on the tested configurations. The loader-compat recovery code still permits registry writes without shared ownership or contrary to the owner’s selected action. It can also certify settings it did not restore. These defects block the shared-host safety requirement in mission section 4.2.

#### Findings

**R11-F1** · P1 · defect · blocks readiness — An accepted spelling of KeepCurrent can import the backup
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:383`; acceptance: 4.2
- Trigger: The owner invokes RecoverOnly with -RegistryResolution keepcurrent on a pending transaction.
- Expected: Every accepted spelling of KeepCurrent preserves the live settings and performs zero imports.
- Actual: ValidateSet accepts keepcurrent and passes that exact string through. Invoke-LcRegistryStep tests it with -ceq 'KeepCurrent'. On a pending restore with an intact backup, execution then reaches the automatic delete-and-import path.
- Why it blocks / does not: A valid command asking to preserve the owner’s current settings can overwrite them.
- Requested correction: Normalize the option when binding it or use the same case-insensitive comparison as ValidateSet throughout dispatch.
- Requested evidence: Exercise lowercase and mixed-case KeepCurrent through recovery of a pending fixture. Verify zero imports, unchanged live values and the owner-kept-current result.

**R8-F2** · P1 · defect · blocks readiness — Interrupted legacy recovery can proceed without acquiring the shared lease
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:612`; acceptance: 4.2
- Trigger: Recovery stops after saving its intended lease ID and before publishing compatibility.lock.
- Expected: A recovery with outstanding registry work acquires its recorded shared lock and publishes durable lease state before acting.
- Actual: At recovery-lease-intended the converted journal has a leaseRunId and sharedLock=false. On restart, line 612 skips acquisition because sharedLock is false. Completion can then run with a null lease. Even uninterrupted conversion of an unleased journal constructs a lease with State=null, so deferral cannot publish the claimed RecoveryRequired lease state.
- Why it blocks / does not: A legacy registry import can run while another lab is free to acquire the shared host.
- Requested correction: Complete an interrupted lease acquisition from its recorded intent. Require the matching live lock handle before registry work and persist the recovery lease when an owner decision is outstanding.
- Requested evidence: Interrupt a legacy recovery at recovery-lease-intended, before the lock exists. On retry, verify that a competing acquisition is refused during import and that a deferred decision retains the shared lock and a RecoveryRequired lease.

**R11-F2** · P1 · defect · blocks readiness — Retry can overwrite settings changed by a peer during deferral
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:413`; acceptance: 4.2
- Trigger: A peer blocks an import after the snapshot, changes the live settings and exits before recovery.
- Expected: Recovery compares the live settings with the retained snapshot and backup before overwriting a state another session may have changed.
- Actual: When a peer appears during the pre-import export, the runner sets registryRestore back to pending. On retry, the pending branch replaces the saved preRestore snapshot with a new export and imports unconditionally. A settings change made by the peer after the first export is lost.
- Why it blocks / does not: Waiting for the peer to exit avoids a simultaneous write but still erases that peer’s later settings.
- Requested correction: Retain the first snapshot across a withheld import and reconcile against it on retry. Preserve unrecognized current settings until the owner resolves them.
- Requested evidence: Extend the peer-during-snapshot fixture so the peer changes a value before leaving. Ordinary recovery must preserve it and retain ownership. Keep the existing control where the settings stay the same and one import completes.

**R10-F5** · P1 · defect · blocks readiness — Reconciliation can use different backup bytes from those it verified
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:393`; acceptance: 4.2
- Trigger: The backup changes while the live registry is being exported for reconciliation.
- Expected: Comparison and import consume the same verified backup bytes throughout the operation.
- Actual: Recovery hashes the backup, exports the live key, then reopens the backup path for comparison. If the backup is replaced with the live values during export, Resolve-LcInterruptedRestore returns applied. That branch settles immediately and bypasses the second digest check. The import path also reopens the filename after verification.
- Why it blocks / does not: The runner can certify a restore that never occurred or import bytes other than those captured at intake.
- Requested correction: Hold the verified backup stable through reconciliation and import, using a read handle that prevents replacement or an equivalently protected verified copy.
- Requested evidence: Use the existing export hook to attempt replacement after the first digest check. Show that the unrestored live state cannot be certified as applied and that only the original verified bytes can be imported.

**R10-F2** · P1 · defect · blocks readiness — Cleanup discards an unresolved registry result from the preceding runner
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:429`; acceptance: 4.2, 6
- Trigger: The schema-3 runner stopped after marking an inconclusive restore completed and before removing its locks.
- Expected: An explicitly unresolved registry result retains its recovery record until restoration or an owner decision.
- Actual: The schema-3 predecessor could persist registryRestore='unresolved: …' and phase='lease-completed' while both locks still existed. Conversion preserves that phase. Complete-LcTransaction skips the registry step and removes the journal and locks. The invocation reports failure, but a later owner resolution finds no journal.
- Why it blocks / does not: A supported predecessor record can lose its recovery path while owner settings are still unresolved.
- Requested correction: During migration, distinguish settled registry outcomes from the predecessor’s premature Completed marker. Preserve an outstanding registry decision under the matching ownership.
- Requested evidence: Start from a schema-3 lease-completed journal with unresolved registry status and its matching locks. Verify that ordinary recovery retains it and that the owner can subsequently settle that same transaction.

**R8-F1** · P1 · defect · blocks readiness — Recovery checks one journal read and acts on another
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:592`; acceptance: 4.2
- Trigger: A live session finishes and a replacement session starts between the two reads under the recovery guard.
- Expected: The journal acted upon is the same snapshot whose identity was compared with the caller’s original observation.
- Actual: Under the guard, Get-LcJournalIdentity reads one file and Read-LcJournalFile opens the path again. The guard serializes recovery callers only. Normal completion and startup can replace A with B between those reads. The later ownership check then compares B with itself.
- Why it blocks / does not: A recovery started for A can restore or release B if B’s owner has also exited.
- Requested correction: Read the journal once under the guard and derive both its identity and the recovery object from those bytes. Apply the same rule to legacy digest identities.
- Requested evidence: Pause between identity verification and journal selection while A finishes normally and B starts and is interrupted. Verify that the caller leaves B’s journal, locks and registry untouched.

**R11-F3** · P1 · defect · blocks readiness — An owner’s resolution is not bound to the session that requested it
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:372`; acceptance: 4.2
- Trigger: The interrupted session changes between the owner reviewing a deferral and issuing the separate resolution command.
- Expected: An owner resolution identifies the interrupted transaction for which the decision was requested.
- Actual: The printed owner-resolution command carries only Import or KeepCurrent. If A is settled and B becomes pending before the owner runs that command, both identity reads see B and the decision is applied to B.
- Why it blocks / does not: An explicit decision about one session’s settings can authorize a destructive action on another session.
- Requested correction: Include the transaction identity in the resolution instruction and verify it before honoring Import or KeepCurrent.
- Requested evidence: Obtain a resolution instruction for A, settle A, then leave B pending. Applying A’s instruction must preserve B’s settings and ownership.

**R10-F3** · P2 · defect · blocks readiness — The final backup read still separates the host check from the registry write
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:416`; acceptance: 4.2
- Trigger: A peer appears during the second SHA-256 calculation.
- Expected: The last host check follows the potentially slow backup verification and immediately precedes the destructive operation.
- Actual: The export-time peer checks work. A full backup hash still runs after Get-LcHostConflicts and before Restore. A peer that starts during that read is first detected after the registry mutation.
- Why it blocks / does not: Registry deletion and import can run while a peer game or sibling transaction is active.
- Requested correction: Verify and hold the restore source first, then check host conflicts immediately before invoking Restore.
- Requested evidence: Introduce a fixture peer during the final backup verification. Verify zero imports and retained ownership while it remains active.

**R10-F1** · P2 · defect · blocks readiness — Linguistic equality can certify distinct registry string values as identical
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:339`; acceptance: 4.2
- Trigger: Two exports differ in the UTF-16 representation of a string value.
- Expected: Registry string data must compare equal ordinally before recovery certifies exact restoration.
- Actual: The canonical form now preserves key associations and continuation order. Its final comparison uses PowerShell -ceq, which uses linguistic equality. On this host a value containing U+00E9 compared equal to one containing U+0065 U+0301, although their lengths and ordinal contents differed.
- Why it blocks / does not: Recovery can classify different stored string data as the already-applied backup.
- Requested correction: Use ordinal equality for both canonical comparisons and ordinal ordering for canonical value entries.
- Requested evidence: Compare exports containing the two Unicode spellings in the same value. Verify that recovery preserves the distinct live value as unresolved when it matches neither recorded state.

**R9-F3** · P2 · defect · blocks readiness — A waiting recovery caller can turn another caller’s failed result into success
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:586`; acceptance: 4.2, 6
- Trigger: One recovery observes the journal and another completes a failed integrity check and removes it before the first acquires the guard.
- Expected: A caller that observed a specific transaction reports its recorded verdict, or reports that the verdict could not be verified.
- Actual: When another caller finishes cleanup after the first journal read, the missing-journal branch returns Succeeded=true without consulting the completed lease. The other caller may have recorded changed or unreadable owner files and exited 1.
- Why it blocks / does not: Two callers recovering the same failed transaction can return opposite exit codes to automation.
- Requested correction: Retain the observed lease identity and read its durable verdict on this path. Return a nonzero result if that verdict is failed or unavailable.
- Requested evidence: Use the existing paused-recovery fixture with a changed manifest. Verify that both recovery commands return nonzero, cleanup completes once and the registry is imported at most once.

**R11-F4** · P3 · optional_polish · non-blocking — The healthy-profile unit fixture still models the earlier 17-check profile
- Location: `tests/KingmakerGunslinger.DomainTests/SpearBootstrapTooltipRepairTests.cs:191`; acceptance: 4.1.2
- Trigger: The focused gate runs HealthyProfilePassesWithoutFindings.
- Expected: The shared healthy-profile fixture represents the current set of predicates.
- Actual: The common HealthyProfile fixture omits subcategory-list and asserts 17 checks. The current product evaluates 18.
- Why it blocks / does not: The returned-list predicate has separate focused and native coverage.
- Requested correction: Include subcategory-list in the common fixture and update the cases that currently append it or expect a 17-check message.
- Requested evidence: The existing focused filter passes with the current profile represented.

#### Prior finding assessments

- **R10-F1** — `fix_rejected` — The swapped-subkey and multiline fixtures now distinguish their inputs. The remaining linguistic comparison still equates distinct stored value data, as confirmed in PowerShell 5.1.
- **R10-F2** — `fix_rejected` — A new schema-4 export failure retains ownership and can be retried. A supported schema-3 journal with an explicitly unresolved registry result can still lose its journal during cleanup.
- **R10-F3** — `fix_rejected` — The fixture catches peers introduced during export. The final hash remains between the host check and Restore, leaving a further observable interval before mutation.
- **R10-F4** — `fix_verified` — Recovery clears cached integrity for uncommitted transactions. The passing verified-boundary fixture changes the manifest during the interruption, records changed on recovery and performs no second import.
- **R10-F5** — `fix_rejected` — The intake digest rejects replacements present when recovery begins. Reconciliation subsequently reopens the path and can settle applied using replacement bytes without checking their digest.
- **R8-F1** — `fix_rejected` — The supplied fixture correctly refuses a replacement made before the guard is acquired. A second open under that guard can still select a different journal from the one whose identity passed.
- **R8-F2** — `fix_rejected` — Atomic lock publication and matching schema-2 orphan adoption cover the tested cases. An interruption one step earlier in legacy conversion leaves durable lease intent, and the retry can proceed without acquiring that lease.
- **R8-F4** — `fix_verified` — An unleased legacy journal with no restore evidence now defers. The fixtures preserve both an already-applied backup and later settings with zero automatic imports. KeepCurrent settles the recorded choice.
- **R9-F1** — `fix_verified` — Publish-LcClaim writes and flushes a private file before the same-directory no-overwrite move. The fixtures cover interruption before publication and preservation of the winning journal and locks.
- **R9-F3** — `fix_rejected` — The child-process cases now propagate verified, changed and unreadable cleanup verdicts correctly. A caller whose observed journal disappears during the wait still returns unconditional success.

#### Evidence assessment

- **Inspected:** The pinned mission, repository instructions, validation policy, review workflow, complete feature diff, developer report, CLI transcript, gate receipt and ten pending ledger entries.; Spear construction and selector publication, bootstrap rollback, battered-origin inspection, Expanded Summoning publication and menu hooks, and the native pricing, persistence and sorcerer probes.; The schema-4 loader transaction, its predecessor journal formats, shared lease helpers, recovery dispatch, fixture assertions and investigation record.
- **Executed verification:** The controller receipt and matching log SHA-256 confirm production compilation and 30 of 30 selected elven-branched-spear tests passing at 222380c64f81 in 2.0 seconds. The transcript confirms the guarded push and clean worktree.; The developer transcript records 99 passing offline safety checks at the final commit. The empty-journal RecoverOnly command also completed successfully.; The transcript confirms S10 and B10 on runner commit 731f7d883a. The final commit changes only the investigation record. S10 loaded product DLL 6b2696c7…7049f from package 253fbe43…43518. B10 loaded the released DLL 0eff2af3…ab96f. Both sessions recorded an applied registry restore and verified owner-file integrity across 95 saves.; S10 completed startup with the simulated 15-foot, Polearms donor. Its invalid-owned-profile launch emitted one fighter-group diagnostic and one unavailable-token warning. Each of four ordinary weapons completed 25 price queries and 25 tooltip fills with matching values of 1, 4, 5 and 13 gp and zero throws. B10 reproduced the original generic startup failure.; Two read-only PowerShell 5.1 checks confirmed the operator behavior used in R11-F1 and R10-F1. ValidateSet accepted lowercase keepcurrent and preserved its casing. Case-sensitive PowerShell equality treated two ordinally distinct Unicode registry value strings as equal.
- **Source inspection only:** Git comparison found no product, domain-test or native-harness source changes between the frozen native package revision 4e0ac475aabc and the reviewed head.; The transaction interleavings in the findings were traced through the candidate and predecessor code. The supplied 99-check fixture covers neighboring cases but does not exercise the specific remaining paths.
- **Required gaps:** Show that recovery holds the correct shared lease, acts on the intended transaction and honors every accepted spelling of the owner’s decision.; Show that reconciliation uses the verified backup bytes, preserves later peer changes and retains unresolved predecessor transactions for recovery.; Show exact registry-value comparison, a host check after the last backup read, and consistent failure verdicts for overlapping recovery callers.
- **Limitations:** The review used the read-only snapshot. Builds, game launches, registry operations and transaction fixtures were not rerun by the reviewer.; The reporter’s raw log and complete 43-mod environment are unavailable. The simulated donor change establishes a failure mechanism. The reporter’s exact failing predicate and external cause remain unknown.; The earlier P7 and R7/R8 persistence and reporter-configuration evidence is historical. Its product source and harness are unchanged at this candidate. The current transcript directly covers S10 and B10.; Live runs exercised uncontended completion. Interruption and competing-owner coverage comes from disposable fixtures.

#### Summary

- **Delivered behavior:** The owned spear receives its intended profile and produces bounded diagnostics for invalid values. Ordinary weapons retain native prices and tooltip values after failed initialization. Earlier leased runs on the same product cover the expanded creature menu and selected Fire Beetle summon, plus fresh-process persistence of the released spear and the 22 gp battered pistol with Gunslinger disabled.
- **Scope reviewed:** Complete final mission for draft PR #195, including gameplay changes, native evidence, validation provenance and the loader-compat transaction used for shared-host testing.
- **Pending later-phase work:** none
- **Unresolved questions:** Which predicate failed in the reporter’s exact environment, and what changed it, remain unknown. The available four-mod subset did not reproduce that failure on the released package.
- **Remaining owner review:** After the recovery defects are corrected, the owner can judge the player-facing presentation and decide whether to merge or release.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a200b8-4abe-7462-96e9-6a56c23be98d`, duration 760.9s, response sha256 `a893d9c647ead25a522183852a73e076a1e4ecb63239ce8f0539ff9bc42f3fd2`, request `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r11-6fc6e6`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "222380c64f81853cbc5ef40caef5665defb7b2d6",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The focused gate passed, and the native records support the spear, pricing, tooltip and summoning repairs on the tested configurations. The loader-compat recovery code still permits registry writes without shared ownership or contrary to the owner\u2019s selected action. It can also certify settings it did not restore. These defects block the shared-host safety requirement in mission section 4.2.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt and matching log SHA-256 confirm production compilation and 30 of 30 selected elven-branched-spear tests passing at 222380c64f81 in 2.0 seconds. The transcript confirms the guarded push and clean worktree.",
      "The developer transcript records 99 passing offline safety checks at the final commit. The empty-journal RecoverOnly command also completed successfully.",
      "The transcript confirms S10 and B10 on runner commit 731f7d883a. The final commit changes only the investigation record. S10 loaded product DLL 6b2696c7\u20267049f from package 253fbe43\u202643518. B10 loaded the released DLL 0eff2af3\u2026ab96f. Both sessions recorded an applied registry restore and verified owner-file integrity across 95 saves.",
      "S10 completed startup with the simulated 15-foot, Polearms donor. Its invalid-owned-profile launch emitted one fighter-group diagnostic and one unavailable-token warning. Each of four ordinary weapons completed 25 price queries and 25 tooltip fills with matching values of 1, 4, 5 and 13 gp and zero throws. B10 reproduced the original generic startup failure.",
      "Two read-only PowerShell 5.1 checks confirmed the operator behavior used in R11-F1 and R10-F1. ValidateSet accepted lowercase keepcurrent and preserved its casing. Case-sensitive PowerShell equality treated two ordinally distinct Unicode registry value strings as equal."
    ],
    "inspected": [
      "The pinned mission, repository instructions, validation policy, review workflow, complete feature diff, developer report, CLI transcript, gate receipt and ten pending ledger entries.",
      "Spear construction and selector publication, bootstrap rollback, battered-origin inspection, Expanded Summoning publication and menu hooks, and the native pricing, persistence and sorcerer probes.",
      "The schema-4 loader transaction, its predecessor journal formats, shared lease helpers, recovery dispatch, fixture assertions and investigation record."
    ],
    "limitations": [
      "The review used the read-only snapshot. Builds, game launches, registry operations and transaction fixtures were not rerun by the reviewer.",
      "The reporter\u2019s raw log and complete 43-mod environment are unavailable. The simulated donor change establishes a failure mechanism. The reporter\u2019s exact failing predicate and external cause remain unknown.",
      "The earlier P7 and R7/R8 persistence and reporter-configuration evidence is historical. Its product source and harness are unchanged at this candidate. The current transcript directly covers S10 and B10.",
      "Live runs exercised uncontended completion. Interruption and competing-owner coverage comes from disposable fixtures."
    ],
    "required_gaps": [
      "Show that recovery holds the correct shared lease, acts on the intended transaction and honors every accepted spelling of the owner\u2019s decision.",
      "Show that reconciliation uses the verified backup bytes, preserves later peer changes and retains unresolved predecessor transactions for recovery.",
      "Show exact registry-value comparison, a host check after the last backup read, and consistent failure verdicts for overlapping recovery callers."
    ],
    "source_inspection_only": [
      "Git comparison found no product, domain-test or native-harness source changes between the frozen native package revision 4e0ac475aabc and the reviewed head.",
      "The transaction interleavings in the findings were traced through the candidate and predecessor code. The supplied 99-check fixture covers neighboring cases but does not exercise the specific remaining paths."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "ValidateSet accepts keepcurrent and passes that exact string through. Invoke-LcRegistryStep tests it with -ceq 'KeepCurrent'. On a pending restore with an intact backup, execution then reaches the automatic delete-and-import path.",
      "blocking_rationale": "A valid command asking to preserve the owner\u2019s current settings can overwrite them.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every accepted spelling of KeepCurrent preserves the live settings and performs zero imports.",
      "id": "R11-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 383
      },
      "priority": "P1",
      "requested_correction": "Normalize the option when binding it or use the same case-insensitive comparison as ValidateSet throughout dispatch.",
      "requested_evidence": "Exercise lowercase and mixed-case KeepCurrent through recovery of a pending fixture. Verify zero imports, unchanged live values and the owner-kept-current result.",
      "title": "An accepted spelling of KeepCurrent can import the backup",
      "trigger": "The owner invokes RecoverOnly with -RegistryResolution keepcurrent on a pending transaction."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "At recovery-lease-intended the converted journal has a leaseRunId and sharedLock=false. On restart, line 612 skips acquisition because sharedLock is false. Completion can then run with a null lease. Even uninterrupted conversion of an unleased journal constructs a lease with State=null, so deferral cannot publish the claimed RecoveryRequired lease state.",
      "blocking_rationale": "A legacy registry import can run while another lab is free to acquire the shared host.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A recovery with outstanding registry work acquires its recorded shared lock and publishes durable lease state before acting.",
      "id": "R8-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 612
      },
      "priority": "P1",
      "requested_correction": "Complete an interrupted lease acquisition from its recorded intent. Require the matching live lock handle before registry work and persist the recovery lease when an owner decision is outstanding.",
      "requested_evidence": "Interrupt a legacy recovery at recovery-lease-intended, before the lock exists. On retry, verify that a competing acquisition is refused during import and that a deferred decision retains the shared lock and a RecoveryRequired lease.",
      "title": "Interrupted legacy recovery can proceed without acquiring the shared lease",
      "trigger": "Recovery stops after saving its intended lease ID and before publishing compatibility.lock."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "When a peer appears during the pre-import export, the runner sets registryRestore back to pending. On retry, the pending branch replaces the saved preRestore snapshot with a new export and imports unconditionally. A settings change made by the peer after the first export is lost.",
      "blocking_rationale": "Waiting for the peer to exit avoids a simultaneous write but still erases that peer\u2019s later settings.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Recovery compares the live settings with the retained snapshot and backup before overwriting a state another session may have changed.",
      "id": "R11-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 413
      },
      "priority": "P1",
      "requested_correction": "Retain the first snapshot across a withheld import and reconcile against it on retry. Preserve unrecognized current settings until the owner resolves them.",
      "requested_evidence": "Extend the peer-during-snapshot fixture so the peer changes a value before leaving. Ordinary recovery must preserve it and retain ownership. Keep the existing control where the settings stay the same and one import completes.",
      "title": "Retry can overwrite settings changed by a peer during deferral",
      "trigger": "A peer blocks an import after the snapshot, changes the live settings and exits before recovery."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Recovery hashes the backup, exports the live key, then reopens the backup path for comparison. If the backup is replaced with the live values during export, Resolve-LcInterruptedRestore returns applied. That branch settles immediately and bypasses the second digest check. The import path also reopens the filename after verification.",
      "blocking_rationale": "The runner can certify a restore that never occurred or import bytes other than those captured at intake.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Comparison and import consume the same verified backup bytes throughout the operation.",
      "id": "R10-F5",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 393
      },
      "priority": "P1",
      "requested_correction": "Hold the verified backup stable through reconciliation and import, using a read handle that prevents replacement or an equivalently protected verified copy.",
      "requested_evidence": "Use the existing export hook to attempt replacement after the first digest check. Show that the unrestored live state cannot be certified as applied and that only the original verified bytes can be imported.",
      "title": "Reconciliation can use different backup bytes from those it verified",
      "trigger": "The backup changes while the live registry is being exported for reconciliation."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "The schema-3 predecessor could persist registryRestore='unresolved: \u2026' and phase='lease-completed' while both locks still existed. Conversion preserves that phase. Complete-LcTransaction skips the registry step and removes the journal and locks. The invocation reports failure, but a later owner resolution finds no journal.",
      "blocking_rationale": "A supported predecessor record can lose its recovery path while owner settings are still unresolved.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An explicitly unresolved registry result retains its recovery record until restoration or an owner decision.",
      "id": "R10-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 429
      },
      "priority": "P1",
      "requested_correction": "During migration, distinguish settled registry outcomes from the predecessor\u2019s premature Completed marker. Preserve an outstanding registry decision under the matching ownership.",
      "requested_evidence": "Start from a schema-3 lease-completed journal with unresolved registry status and its matching locks. Verify that ordinary recovery retains it and that the owner can subsequently settle that same transaction.",
      "title": "Cleanup discards an unresolved registry result from the preceding runner",
      "trigger": "The schema-3 runner stopped after marking an inconclusive restore completed and before removing its locks."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Under the guard, Get-LcJournalIdentity reads one file and Read-LcJournalFile opens the path again. The guard serializes recovery callers only. Normal completion and startup can replace A with B between those reads. The later ownership check then compares B with itself.",
      "blocking_rationale": "A recovery started for A can restore or release B if B\u2019s owner has also exited.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The journal acted upon is the same snapshot whose identity was compared with the caller\u2019s original observation.",
      "id": "R8-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 592
      },
      "priority": "P1",
      "requested_correction": "Read the journal once under the guard and derive both its identity and the recovery object from those bytes. Apply the same rule to legacy digest identities.",
      "requested_evidence": "Pause between identity verification and journal selection while A finishes normally and B starts and is interrupted. Verify that the caller leaves B\u2019s journal, locks and registry untouched.",
      "title": "Recovery checks one journal read and acts on another",
      "trigger": "A live session finishes and a replacement session starts between the two reads under the recovery guard."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "The printed owner-resolution command carries only Import or KeepCurrent. If A is settled and B becomes pending before the owner runs that command, both identity reads see B and the decision is applied to B.",
      "blocking_rationale": "An explicit decision about one session\u2019s settings can authorize a destructive action on another session.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An owner resolution identifies the interrupted transaction for which the decision was requested.",
      "id": "R11-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 372
      },
      "priority": "P1",
      "requested_correction": "Include the transaction identity in the resolution instruction and verify it before honoring Import or KeepCurrent.",
      "requested_evidence": "Obtain a resolution instruction for A, settle A, then leave B pending. Applying A\u2019s instruction must preserve B\u2019s settings and ownership.",
      "title": "An owner\u2019s resolution is not bound to the session that requested it",
      "trigger": "The interrupted session changes between the owner reviewing a deferral and issuing the separate resolution command."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "The export-time peer checks work. A full backup hash still runs after Get-LcHostConflicts and before Restore. A peer that starts during that read is first detected after the registry mutation.",
      "blocking_rationale": "Registry deletion and import can run while a peer game or sibling transaction is active.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The last host check follows the potentially slow backup verification and immediately precedes the destructive operation.",
      "id": "R10-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 416
      },
      "priority": "P2",
      "requested_correction": "Verify and hold the restore source first, then check host conflicts immediately before invoking Restore.",
      "requested_evidence": "Introduce a fixture peer during the final backup verification. Verify zero imports and retained ownership while it remains active.",
      "title": "The final backup read still separates the host check from the registry write",
      "trigger": "A peer appears during the second SHA-256 calculation."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "The canonical form now preserves key associations and continuation order. Its final comparison uses PowerShell -ceq, which uses linguistic equality. On this host a value containing U+00E9 compared equal to one containing U+0065 U+0301, although their lengths and ordinal contents differed.",
      "blocking_rationale": "Recovery can classify different stored string data as the already-applied backup.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Registry string data must compare equal ordinally before recovery certifies exact restoration.",
      "id": "R10-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 339
      },
      "priority": "P2",
      "requested_correction": "Use ordinal equality for both canonical comparisons and ordinal ordering for canonical value entries.",
      "requested_evidence": "Compare exports containing the two Unicode spellings in the same value. Verify that recovery preserves the distinct live value as unresolved when it matches neither recorded state.",
      "title": "Linguistic equality can certify distinct registry string values as identical",
      "trigger": "Two exports differ in the UTF-16 representation of a string value."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "When another caller finishes cleanup after the first journal read, the missing-journal branch returns Succeeded=true without consulting the completed lease. The other caller may have recorded changed or unreadable owner files and exited 1.",
      "blocking_rationale": "Two callers recovering the same failed transaction can return opposite exit codes to automation.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A caller that observed a specific transaction reports its recorded verdict, or reports that the verdict could not be verified.",
      "id": "R9-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 586
      },
      "priority": "P2",
      "requested_correction": "Retain the observed lease identity and read its durable verdict on this path. Return a nonzero result if that verdict is failed or unavailable.",
      "requested_evidence": "Use the existing paused-recovery fixture with a changed manifest. Verify that both recovery commands return nonzero, cleanup completes once and the registry is imported at most once.",
      "title": "A waiting recovery caller can turn another caller\u2019s failed result into success",
      "trigger": "One recovery observes the journal and another completes a failed integrity check and removes it before the first acquires the guard."
    },
    {
      "acceptance_ids": [
        "4.1.2"
      ],
      "actual": "The common HealthyProfile fixture omits subcategory-list and asserts 17 checks. The current product evaluates 18.",
      "blocking_rationale": "The returned-list predicate has separate focused and native coverage.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The shared healthy-profile fixture represents the current set of predicates.",
      "id": "R11-F4",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/SpearBootstrapTooltipRepairTests.cs",
        "line": 191
      },
      "priority": "P3",
      "requested_correction": "Include subcategory-list in the common fixture and update the cases that currently append it or expect a 17-check message.",
      "requested_evidence": "The existing focused filter passes with the current profile represented.",
      "title": "The healthy-profile unit fixture still models the earlier 17-check profile",
      "trigger": "The focused gate runs HealthyProfilePassesWithoutFindings."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_rejected",
      "finding_id": "R10-F1",
      "rationale": "The swapped-subkey and multiline fixtures now distinguish their inputs. The remaining linguistic comparison still equates distinct stored value data, as confirmed in PowerShell 5.1."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R10-F2",
      "rationale": "A new schema-4 export failure retains ownership and can be retried. A supported schema-3 journal with an explicitly unresolved registry result can still lose its journal during cleanup."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R10-F3",
      "rationale": "The fixture catches peers introduced during export. The final hash remains between the host check and Restore, leaving a further observable interval before mutation."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R10-F4",
      "rationale": "Recovery clears cached integrity for uncommitted transactions. The passing verified-boundary fixture changes the manifest during the interruption, records changed on recovery and performs no second import."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R10-F5",
      "rationale": "The intake digest rejects replacements present when recovery begins. Reconciliation subsequently reopens the path and can settle applied using replacement bytes without checking their digest."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F1",
      "rationale": "The supplied fixture correctly refuses a replacement made before the guard is acquired. A second open under that guard can still select a different journal from the one whose identity passed."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F2",
      "rationale": "Atomic lock publication and matching schema-2 orphan adoption cover the tested cases. An interruption one step earlier in legacy conversion leaves durable lease intent, and the retry can proceed without acquiring that lease."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R8-F4",
      "rationale": "An unleased legacy journal with no restore evidence now defers. The fixtures preserve both an already-applied backup and later settings with zero automatic imports. KeepCurrent settles the recorded choice."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R9-F1",
      "rationale": "Publish-LcClaim writes and flushes a private file before the same-directory no-overwrite move. The fixtures cover interruption before publication and preservation of the winning journal and locks."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R9-F3",
      "rationale": "The child-process cases now propagate verified, changed and unreadable cleanup verdicts correctly. A caller whose observed journal disappears during the wait still returns unconditional success."
    }
  ],
  "request_id": "kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r11-6fc6e6",
  "round": 11,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The owned spear receives its intended profile and produces bounded diagnostics for invalid values. Ordinary weapons retain native prices and tooltip values after failed initialization. Earlier leased runs on the same product cover the expanded creature menu and selected Fire Beetle summon, plus fresh-process persistence of the released spear and the 22 gp battered pistol with Gunslinger disabled.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "After the recovery defects are corrected, the owner can judge the player-facing presentation and decide whether to merge or release."
    ],
    "scope_reviewed": "Complete final mission for draft PR #195, including gameplay changes, native evidence, validation provenance and the loader-compat transaction used for shared-host testing.",
    "unresolved_questions": [
      "Which predicate failed in the reporter\u2019s exact environment, and what changed it, remain unknown. The available four-mod subset did not reproduce that failure on the released package."
    ]
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
