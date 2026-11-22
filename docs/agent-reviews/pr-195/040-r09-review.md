<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r09-review part=1/1 sha256=d58541eea26fd83009fa564cf2cc55d746a0ff14f18c091727ee17da76e5096f -->
### Codex GPT-6.2 Astra — review — round 9 — candidate `db63a941a6`

**Decision:** `changes_required` — The focused gate passed, and the native records support the spear repair, normal pricing, disabled-publication reload and selected summon. The loader-compat runner can still lose a session’s recovery journal, replay a completed registry restore, or release ownership after the host becomes busy. The failed-startup tooltip observations also predate the required shared runtime lease.

#### Findings

**R8-F1** · P1 · defect · blocks readiness — Recovery can restore from a stale or unleased journal
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:331`; acceptance: 4.2
- Trigger: Two callers read the same pending journal and one finishes before the other acquires ownership, or recovery encounters an older journal created without a shared lease.
- Expected: Only the caller holding exclusive ownership of the current, still-pending session may import its registry backup.
- Actual: Invoke-LcRecovery reads the journal before acquiring its handle. A delayed caller can recreate the released lock, accept the matching run ID and purpose from a Completed lease, and restore from its stale journal. A pre-round-7 journal without a lease ID skips the handle acquisition entirely.
- Why it blocks / does not: A recovery invocation can overwrite current game settings after another caller has already completed the transaction.
- Requested correction: Serialize recovery and re-read the current journal and lease under that ownership before any registry write. Treat completed sessions as cleanup-only. Acquire equivalent ownership for a supported unleased legacy journal or refuse recovery without mutating the host.
- Requested evidence: Pause a second recovery after its initial read, let the first finish, then resume the second. Show one restore in total and preservation of any newer session. Also exercise a pre-round-7 journal with a competing caller.

**R9-F1** · P1 · defect · blocks readiness — Session starts and phase updates can destroy the recovery journal
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:120`; acceptance: 4.2
- Trigger: Two loader-compat invocations pass admission together, or the process stops during a journal write.
- Expected: One caller claims the journal atomically, and every progress update leaves a complete recoverable record.
- Actual: Admission checks Test-Path and then writes the global journal with WriteAllText. Two callers can pass the check. The loser can overwrite the winner’s journal and delete it when shared-lock acquisition fails. Later phase updates also truncate the sole journal in place, leaving recovery unable to parse it if a write is interrupted.
- Why it blocks / does not: The runner can lose the only record linking an active transaction to its registry backup and remaining locks.
- Requested correction: Create the initial journal with atomic no-overwrite semantics, replace subsequent versions atomically, and verify ownership before removing it during rollback or completion.
- Requested evidence: Interleave two disposable session starts and show that the losing caller preserves the winner’s journal. Interrupt a later journal update and recover from the last complete record.

**R8-F2** · P2 · defect · blocks readiness — Partial acquisition can still leave a lock without usable recovery state
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:141`; acceptance: 4.2
- Trigger: The shared lock is created but its lease state cannot be written, or the process exits before private-lock acquisition is journaled.
- Expected: Every acquired lock is either rolled back by its owner or identifiable from durable state after interruption.
- Actual: Enter-KmgRuntimeLease deliberately retains compatibility.lock if lease initialization fails. When that call throws, the caller has no $scope and deletes the session journal at line 141. A stop after Enter-LcLock creates the private lock but before privateLock=true is persisted likewise leaves a matching lock that recovery skips.
- Why it blocks / does not: A setup failure can still strand a lock that -RecoverOnly cannot clear, preventing subsequent coordinated sessions.
- Requested correction: Bind the intended lease identity to the journal before partial acquisition can strand it. Retain the record on an ambiguous acquisition failure and reconcile the actual matching private lock during recovery.
- Requested evidence: Inject a lease-state write failure after shared-lock creation and an interruption immediately after private-lock creation. Show safe recovery and successful subsequent acquisition without manual deletion.

**R8-F4** · P2 · defect · blocks readiness — A successful registry import can be replayed after interruption
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:219`; acceptance: 4.2
- Trigger: The runner exits between applying the registry backup and recording success, or recovery converts a completed round-7 transaction.
- Expected: Recovery recognizes an already-applied registry snapshot and completes only the outstanding work.
- Actual: The registry import runs at line 212. Its completion flag is persisted at line 219. Recovery of the intervening restoring phase calls the import again. Legacy conversion also initializes registryRestored to false even when the round-7 lease already records Completed after a successful restore.
- Why it blocks / does not: Replaying the delete-and-import operation can erase settings written after the first successful restoration.
- Requested correction: Reconcile an interrupted restore against the verified intake before deciding whether another import is needed. Honor a completed legacy lease and retain an explicit unresolved state if a safe decision cannot be made.
- Requested evidence: Stop a fixture after the restore callback applies its change and before the restored flag is saved. Also recover a round-7 journal with a Completed lease. Both cases should finish cleanup without a second import.

**R9-F2** · P2 · defect · blocks readiness — The host can become busy during integrity verification and still be released
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Common.ps1:227`; acceptance: 4.2
- Trigger: A peer appears after the initial safe-boundary check and remains active through the final comparison.
- Expected: The runner confirms the host is still available when it commits the final verification and releases ownership.
- Actual: Complete-LcTransaction checks host conflicts before restoration, then hashes the protected files, records Completed and releases the locks without another host check. The owner-file comparison can take long enough for a game or sibling transaction to start.
- Why it blocks / does not: The lease can certify a safe completion and release the host while a conflicting transaction is active.
- Requested correction: Recheck the configured locks and game processes after the protected-file comparison and before completion. Preserve the recorded registry restore and defer release if contention appeared during verification.
- Requested evidence: Have a disposable manifest callback introduce a peer process or sibling lock during verification. Show retained ownership while the peer remains and eventual completion without another registry import.

**R9-F3** · P2 · defect · blocks readiness — A failed integrity check can return a successful command exit
- Location: `scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1:382`; acceptance: 4.2, 6
- Trigger: The session body finishes normally but teardown defers, or recovery finds changed or unreadable protected-file intake.
- Expected: The command preserves its evidence and signals a failing process result for failed integrity or deferred recovery.
- Actual: The main command writes integrity-failed and reaches the end of the script normally. The recovery command also returns a success string when the integrity result is changed or unresolved. A caller using the process exit code receives success in these cases.
- Why it blocks / does not: Automation can advance past a session whose restoration or protected-file integrity still needs attention.
- Requested correction: Propagate the final unsuccessful transaction result after writing the summary and completing safe cleanup. Apply the same result semantics to -RecoverOnly.
- Requested evidence: Capture process exit codes and recorded outcomes for a clean fixture, deferred restoration, and a changed or unreadable manifest. Only the clean completion should return success.

**R9-F4** · P2 · evidence_gap · blocks readiness — The historical failed-startup tooltip runs predate shared runtime ownership
- Location: `docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md:413`; acceptance: 3.A.3, 4.1.1, 4.1.3, 4.2
- Trigger: Final acceptance relies on the simulated donor and failed-startup pricing results in section 4.2.
- Expected: The reproduced donor condition and failed-startup price and tooltip checks have native records from the required guarded transaction.
- Actual: The investigation identifies B and all simulated sessions through S6 as private-lock-only runs. The later leased I7/I8 failures check diagnostics and selectors. R7/R8 query prices after successful startup. None of those leased sessions exercises ordinary tooltip pricing after failed startup or the original 15 ft/Polearms type-donor case.
- Why it blocks / does not: The historical observations establish mechanical behavior, but the mission also requires those native checks to use the shared runtime lease.
- Requested correction: Supply existing shared-lease records for those cases, or use the corrected runner for the baseline control and the two affected candidate launches, simulated-donor-mutation and invalid-owned-profile. Reuse the frozen packages.
- Requested evidence: Record the loaded DLL, shared lease, donor and control-weapon observations, per-weapon query and tooltip results, warning count, and final restoration for the targeted launches.

**R9-F5** · P3 · optional_polish · non-blocking — The environment summary attributes an in-process check to F8
- Location: `docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md:407`; acceptance: 4.2, 6
- Trigger: The owner reads the common environment description alongside the F8 setup-failure record.
- Expected: The environment summary attributes in-process identity checks to sessions that launched the game.
- Actual: Section 4.2 says every session verified in-process loader and product identity. F8 stopped while copying a missing fixture save, before any game process launched.
- Why it blocks / does not: F8’s individual entry accurately records the setup error and restoration.
- Requested correction: Qualify the common identity statement and describe F8 as host-side setup and restoration evidence.
- Requested evidence: Check the revised sentence against the F8 CLI report. No additional execution is needed.

#### Prior finding assessments

- **R7-F1** — `fix_verified` — Get-LcHostConflicts now blocks restoration in both finalization and recovery when a sibling lock is already present. The fixture keeps that lock present with an empty game list, verifies zero restores and retained ownership, then verifies one restore after release. R9-F2 concerns a peer arriving later during verification.
- **R8-F1** — `fix_rejected` — The live-owner and held-handle fixtures pass. A delayed caller can still act on a journal read before it gained ownership, and an unleased legacy recovery reaches restoration without taking the shared handle.
- **R8-F2** — `fix_rejected` — The missing-ZIP refusal and F8 confirm cleanup for the original input error and for failure after both locks were acquired. An error inside lease acquisition or an interruption before recording the private lock can still strand ownership.
- **R8-F3** — `fix_verified` — The runner persists the intake manifests and their digests beside the lease. It records the comparison before releasing shared ownership. The fixtures cover a refused competing lease, a changed manifest and an unreadable manifest.
- **R8-F4** — `fix_rejected` — The five fixtures demonstrate recovery after their named phase writes. They skip the interval after the registry change and before its phase write, and the legacy fixture starts from an active lease. Both remaining paths can replay a successful restore.
- **R8-F5** — `fix_verified` — Section 4.2 now limits the private-lock-only statement to sessions before round 7 and identifies the four Completed round-7 leases for I7, P7, X7 and R7.

#### Evidence assessment

- **Inspected:** The pinned mission, repository review and validation directives, 30-file feature diff, developer report, CLI transcript, focused receipt and prior-findings ledger.; Spear construction and selector publication, named and progression item validation, battered-origin inspection and strict ownership paths, Expanded Summoning publication and menu hooks, native probes and session definitions.; The session runner, shared lease implementation, journal acquisition and recovery, registry restore, owner-file comparison, safety fixtures and committed investigation record.
- **Executed verification:** Read-only Git and hash checks confirmed H, the mission SHA-256, and identical src trees at product commit 6d6a295b64, package commit 4e0ac475aabc and H. The gate log SHA-256 matches its controller receipt.; The controller’s focused gate compiled production and passed 30 of 30 selected elven-branched-spear tests at H in 2.3 seconds. The transcript also confirms the guarded push and clean worktree.; The developer transcript confirms 74 passing offline safety checks at H. A missing ZIP was refused before acquisition. The deliberate F8 setup failure exited with code 1 and recorded registry restoration, 95 unchanged owner saves, unchanged installation, a Completed lease and a free shared lock.; On UMM 0.31.1.0, I8 passed both induced item-damage diagnostic launches. R8 passed the reporter-settings and startup launches with Call of the Wild, CraftMagicItems, KingmakerBugfixes and RacesUnleashed. The startup session checks normal prices, the visible creature menu, the selected Fire Beetle ability and the spawned unit. Both sessions used DLL 6b2696c70988db4ea1ed42ce015b99226a4de1fdd1727c569bf35eefd5e7049f and reported restored ownership.; Earlier controller assessments corroborate the simulated donor and failed-startup tooltip observations on their original packages. P7 on the reused product package confirmed a fresh-process reload with Gunslinger disabled, a 1d8 spear, the battered pistol’s saved owner and 22 gp price, and native control prices.
- **Source inspection only:** The pricing change is confined to read-only inspection. Bind, owner resolution and malformed-token checks retain their strict paths. The summoning menu hook checks for a published parent before applying the expanded menu.; The journal is read before recovery ownership is acquired, written in place during acquisition and teardown, and removed by pathname. The 74 passing fixtures do not exercise all of the resulting interruption and concurrency cases.
- **Required gaps:** Prove that admission, partial acquisition, concurrent recovery and interrupted registry restoration preserve the owning session’s record and restore settings at most once.; Demonstrate that a peer appearing during final verification defers release and that the command reports unsuccessful integrity or recovery through a failing exit status.; Supply shared-lease native evidence for the reproduced Longspear condition and for repeated ordinary price and tooltip queries after deliberately failed startup.
- **Limitations:** Review access was read-only. Native observations are attributed to the supplied CLI records and earlier controller assessments. The competing-caller and interruption findings below come from source inspection.; The reporter’s raw log and full 43-mod configuration were unavailable. The four-mod setup did not reproduce the original startup failure. The donor mutation and invalid profiles establish specific failure mechanisms.; The observed failed-startup spell was the native unexpanded Summon Monster I. A menu opening with zero visible rows remains unconfirmed.

#### Summary

- **Delivered behavior:** The owned spear receives its intended mechanical fields and validates its effective profile after registration. Failed bootstrap takes a narrow native-price path for ordinary weapon inspection. The partial reporter stack passed the selected Fire Beetle summon, and the fresh-process reload retained the battered pistol’s owner and price with Gunslinger disabled.
- **Scope reviewed:** The complete final mission at db63a941a65b3ccf0a6ecbf6cfdab8c4b4461c04, including the reused product artifact, native evidence, shared-host transaction safety and all six pending findings.
- **Pending later-phase work:** none
- **Unresolved questions:** Which spear predicate failed in the reporter’s exact environment, and what changed it, remain unknown.
- **Remaining owner review:** Player-facing presentation acceptance and any eventual merge or release decision remain with the owner.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a20080-61e8-7a03-8c92-c46c33ed1136`, duration 689.0s, response sha256 `785c26e2020c4b1df30c4719b1ed9cad39b76c352382a4cc14b1eefe1fd28d26`, request `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r09-c9052b`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "db63a941a65b3ccf0a6ecbf6cfdab8c4b4461c04",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The focused gate passed, and the native records support the spear repair, normal pricing, disabled-publication reload and selected summon. The loader-compat runner can still lose a session\u2019s recovery journal, replay a completed registry restore, or release ownership after the host becomes busy. The failed-startup tooltip observations also predate the required shared runtime lease.",
  "evidence_assessment": {
    "executed_verification": [
      "Read-only Git and hash checks confirmed H, the mission SHA-256, and identical src trees at product commit 6d6a295b64, package commit 4e0ac475aabc and H. The gate log SHA-256 matches its controller receipt.",
      "The controller\u2019s focused gate compiled production and passed 30 of 30 selected elven-branched-spear tests at H in 2.3 seconds. The transcript also confirms the guarded push and clean worktree.",
      "The developer transcript confirms 74 passing offline safety checks at H. A missing ZIP was refused before acquisition. The deliberate F8 setup failure exited with code 1 and recorded registry restoration, 95 unchanged owner saves, unchanged installation, a Completed lease and a free shared lock.",
      "On UMM 0.31.1.0, I8 passed both induced item-damage diagnostic launches. R8 passed the reporter-settings and startup launches with Call of the Wild, CraftMagicItems, KingmakerBugfixes and RacesUnleashed. The startup session checks normal prices, the visible creature menu, the selected Fire Beetle ability and the spawned unit. Both sessions used DLL 6b2696c70988db4ea1ed42ce015b99226a4de1fdd1727c569bf35eefd5e7049f and reported restored ownership.",
      "Earlier controller assessments corroborate the simulated donor and failed-startup tooltip observations on their original packages. P7 on the reused product package confirmed a fresh-process reload with Gunslinger disabled, a 1d8 spear, the battered pistol\u2019s saved owner and 22 gp price, and native control prices."
    ],
    "inspected": [
      "The pinned mission, repository review and validation directives, 30-file feature diff, developer report, CLI transcript, focused receipt and prior-findings ledger.",
      "Spear construction and selector publication, named and progression item validation, battered-origin inspection and strict ownership paths, Expanded Summoning publication and menu hooks, native probes and session definitions.",
      "The session runner, shared lease implementation, journal acquisition and recovery, registry restore, owner-file comparison, safety fixtures and committed investigation record."
    ],
    "limitations": [
      "Review access was read-only. Native observations are attributed to the supplied CLI records and earlier controller assessments. The competing-caller and interruption findings below come from source inspection.",
      "The reporter\u2019s raw log and full 43-mod configuration were unavailable. The four-mod setup did not reproduce the original startup failure. The donor mutation and invalid profiles establish specific failure mechanisms.",
      "The observed failed-startup spell was the native unexpanded Summon Monster I. A menu opening with zero visible rows remains unconfirmed."
    ],
    "required_gaps": [
      "Prove that admission, partial acquisition, concurrent recovery and interrupted registry restoration preserve the owning session\u2019s record and restore settings at most once.",
      "Demonstrate that a peer appearing during final verification defers release and that the command reports unsuccessful integrity or recovery through a failing exit status.",
      "Supply shared-lease native evidence for the reproduced Longspear condition and for repeated ordinary price and tooltip queries after deliberately failed startup."
    ],
    "source_inspection_only": [
      "The pricing change is confined to read-only inspection. Bind, owner resolution and malformed-token checks retain their strict paths. The summoning menu hook checks for a published parent before applying the expanded menu.",
      "The journal is read before recovery ownership is acquired, written in place during acquisition and teardown, and removed by pathname. The 74 passing fixtures do not exercise all of the resulting interruption and concurrency cases."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Invoke-LcRecovery reads the journal before acquiring its handle. A delayed caller can recreate the released lock, accept the matching run ID and purpose from a Completed lease, and restore from its stale journal. A pre-round-7 journal without a lease ID skips the handle acquisition entirely.",
      "blocking_rationale": "A recovery invocation can overwrite current game settings after another caller has already completed the transaction.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Only the caller holding exclusive ownership of the current, still-pending session may import its registry backup.",
      "id": "R8-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 331
      },
      "priority": "P1",
      "requested_correction": "Serialize recovery and re-read the current journal and lease under that ownership before any registry write. Treat completed sessions as cleanup-only. Acquire equivalent ownership for a supported unleased legacy journal or refuse recovery without mutating the host.",
      "requested_evidence": "Pause a second recovery after its initial read, let the first finish, then resume the second. Show one restore in total and preservation of any newer session. Also exercise a pre-round-7 journal with a competing caller.",
      "title": "Recovery can restore from a stale or unleased journal",
      "trigger": "Two callers read the same pending journal and one finishes before the other acquires ownership, or recovery encounters an older journal created without a shared lease."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Admission checks Test-Path and then writes the global journal with WriteAllText. Two callers can pass the check. The loser can overwrite the winner\u2019s journal and delete it when shared-lock acquisition fails. Later phase updates also truncate the sole journal in place, leaving recovery unable to parse it if a write is interrupted.",
      "blocking_rationale": "The runner can lose the only record linking an active transaction to its registry backup and remaining locks.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "One caller claims the journal atomically, and every progress update leaves a complete recoverable record.",
      "id": "R9-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 120
      },
      "priority": "P1",
      "requested_correction": "Create the initial journal with atomic no-overwrite semantics, replace subsequent versions atomically, and verify ownership before removing it during rollback or completion.",
      "requested_evidence": "Interleave two disposable session starts and show that the losing caller preserves the winner\u2019s journal. Interrupt a later journal update and recover from the last complete record.",
      "title": "Session starts and phase updates can destroy the recovery journal",
      "trigger": "Two loader-compat invocations pass admission together, or the process stops during a journal write."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Enter-KmgRuntimeLease deliberately retains compatibility.lock if lease initialization fails. When that call throws, the caller has no $scope and deletes the session journal at line 141. A stop after Enter-LcLock creates the private lock but before privateLock=true is persisted likewise leaves a matching lock that recovery skips.",
      "blocking_rationale": "A setup failure can still strand a lock that -RecoverOnly cannot clear, preventing subsequent coordinated sessions.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every acquired lock is either rolled back by its owner or identifiable from durable state after interruption.",
      "id": "R8-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 141
      },
      "priority": "P2",
      "requested_correction": "Bind the intended lease identity to the journal before partial acquisition can strand it. Retain the record on an ambiguous acquisition failure and reconcile the actual matching private lock during recovery.",
      "requested_evidence": "Inject a lease-state write failure after shared-lock creation and an interruption immediately after private-lock creation. Show safe recovery and successful subsequent acquisition without manual deletion.",
      "title": "Partial acquisition can still leave a lock without usable recovery state",
      "trigger": "The shared lock is created but its lease state cannot be written, or the process exits before private-lock acquisition is journaled."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "The registry import runs at line 212. Its completion flag is persisted at line 219. Recovery of the intervening restoring phase calls the import again. Legacy conversion also initializes registryRestored to false even when the round-7 lease already records Completed after a successful restore.",
      "blocking_rationale": "Replaying the delete-and-import operation can erase settings written after the first successful restoration.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Recovery recognizes an already-applied registry snapshot and completes only the outstanding work.",
      "id": "R8-F4",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 219
      },
      "priority": "P2",
      "requested_correction": "Reconcile an interrupted restore against the verified intake before deciding whether another import is needed. Honor a completed legacy lease and retain an explicit unresolved state if a safe decision cannot be made.",
      "requested_evidence": "Stop a fixture after the restore callback applies its change and before the restored flag is saved. Also recover a round-7 journal with a Completed lease. Both cases should finish cleanup without a second import.",
      "title": "A successful registry import can be replayed after interruption",
      "trigger": "The runner exits between applying the registry backup and recording success, or recovery converts a completed round-7 transaction."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Complete-LcTransaction checks host conflicts before restoration, then hashes the protected files, records Completed and releases the locks without another host check. The owner-file comparison can take long enough for a game or sibling transaction to start.",
      "blocking_rationale": "The lease can certify a safe completion and release the host while a conflicting transaction is active.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The runner confirms the host is still available when it commits the final verification and releases ownership.",
      "id": "R9-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Common.ps1",
        "line": 227
      },
      "priority": "P2",
      "requested_correction": "Recheck the configured locks and game processes after the protected-file comparison and before completion. Preserve the recorded registry restore and defer release if contention appeared during verification.",
      "requested_evidence": "Have a disposable manifest callback introduce a peer process or sibling lock during verification. Show retained ownership while the peer remains and eventual completion without another registry import.",
      "title": "The host can become busy during integrity verification and still be released",
      "trigger": "A peer appears after the initial safe-boundary check and remains active through the final comparison."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "The main command writes integrity-failed and reaches the end of the script normally. The recovery command also returns a success string when the integrity result is changed or unresolved. A caller using the process exit code receives success in these cases.",
      "blocking_rationale": "Automation can advance past a session whose restoration or protected-file integrity still needs attention.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The command preserves its evidence and signals a failing process result for failed integrity or deferred recovery.",
      "id": "R9-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1",
        "line": 382
      },
      "priority": "P2",
      "requested_correction": "Propagate the final unsuccessful transaction result after writing the summary and completing safe cleanup. Apply the same result semantics to -RecoverOnly.",
      "requested_evidence": "Capture process exit codes and recorded outcomes for a clean fixture, deferred restoration, and a changed or unreadable manifest. Only the clean completion should return success.",
      "title": "A failed integrity check can return a successful command exit",
      "trigger": "The session body finishes normally but teardown defers, or recovery finds changed or unreadable protected-file intake."
    },
    {
      "acceptance_ids": [
        "3.A.3",
        "4.1.1",
        "4.1.3",
        "4.2"
      ],
      "actual": "The investigation identifies B and all simulated sessions through S6 as private-lock-only runs. The later leased I7/I8 failures check diagnostics and selectors. R7/R8 query prices after successful startup. None of those leased sessions exercises ordinary tooltip pricing after failed startup or the original 15 ft/Polearms type-donor case.",
      "blocking_rationale": "The historical observations establish mechanical behavior, but the mission also requires those native checks to use the shared runtime lease.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "The reproduced donor condition and failed-startup price and tooltip checks have native records from the required guarded transaction.",
      "id": "R9-F4",
      "location": {
        "file": "docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md",
        "line": 413
      },
      "priority": "P2",
      "requested_correction": "Supply existing shared-lease records for those cases, or use the corrected runner for the baseline control and the two affected candidate launches, simulated-donor-mutation and invalid-owned-profile. Reuse the frozen packages.",
      "requested_evidence": "Record the loaded DLL, shared lease, donor and control-weapon observations, per-weapon query and tooltip results, warning count, and final restoration for the targeted launches.",
      "title": "The historical failed-startup tooltip runs predate shared runtime ownership",
      "trigger": "Final acceptance relies on the simulated donor and failed-startup pricing results in section 4.2."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "Section 4.2 says every session verified in-process loader and product identity. F8 stopped while copying a missing fixture save, before any game process launched.",
      "blocking_rationale": "F8\u2019s individual entry accurately records the setup error and restoration.",
      "blocks_readiness": false,
      "category": "optional_polish",
      "expected": "The environment summary attributes in-process identity checks to sessions that launched the game.",
      "id": "R9-F5",
      "location": {
        "file": "docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md",
        "line": 407
      },
      "priority": "P3",
      "requested_correction": "Qualify the common identity statement and describe F8 as host-side setup and restoration evidence.",
      "requested_evidence": "Check the revised sentence against the F8 CLI report. No additional execution is needed.",
      "title": "The environment summary attributes an in-process check to F8",
      "trigger": "The owner reads the common environment description alongside the F8 setup-failure record."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R7-F1",
      "rationale": "Get-LcHostConflicts now blocks restoration in both finalization and recovery when a sibling lock is already present. The fixture keeps that lock present with an empty game list, verifies zero restores and retained ownership, then verifies one restore after release. R9-F2 concerns a peer arriving later during verification."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F1",
      "rationale": "The live-owner and held-handle fixtures pass. A delayed caller can still act on a journal read before it gained ownership, and an unleased legacy recovery reaches restoration without taking the shared handle."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F2",
      "rationale": "The missing-ZIP refusal and F8 confirm cleanup for the original input error and for failure after both locks were acquired. An error inside lease acquisition or an interruption before recording the private lock can still strand ownership."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R8-F3",
      "rationale": "The runner persists the intake manifests and their digests beside the lease. It records the comparison before releasing shared ownership. The fixtures cover a refused competing lease, a changed manifest and an unreadable manifest."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F4",
      "rationale": "The five fixtures demonstrate recovery after their named phase writes. They skip the interval after the registry change and before its phase write, and the legacy fixture starts from an active lease. Both remaining paths can replay a successful restore."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R8-F5",
      "rationale": "Section 4.2 now limits the private-lock-only statement to sessions before round 7 and identifies the four Completed round-7 leases for I7, P7, X7 and R7."
    }
  ],
  "request_id": "kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r09-c9052b",
  "round": 9,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The owned spear receives its intended mechanical fields and validates its effective profile after registration. Failed bootstrap takes a narrow native-price path for ordinary weapon inspection. The partial reporter stack passed the selected Fire Beetle summon, and the fresh-process reload retained the battered pistol\u2019s owner and price with Gunslinger disabled.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Player-facing presentation acceptance and any eventual merge or release decision remain with the owner."
    ],
    "scope_reviewed": "The complete final mission at db63a941a65b3ccf0a6ecbf6cfdab8c4b4461c04, including the reused product artifact, native evidence, shared-host transaction safety and all six pending findings.",
    "unresolved_questions": [
      "Which spear predicate failed in the reporter\u2019s exact environment, and what changed it, remain unknown."
    ]
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
