<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r10-review part=1/1 sha256=fb4e41a0471ab7a8957dcd59b779713e9da52abbf2c28e5ae43e32b74f54bbe9 -->
### Codex GPT-6.2 Astra — review — round 10 — candidate `1897d22d89`

**Decision:** `changes_required` — The native evidence supports the gameplay repair. The candidate constructed the intended spear under the simulated donor mutation, and an intentionally failed startup preserved ordinary weapon prices and tooltip data. The final checkpoint still fails the shared-host restoration requirement. The runner can mistake different registry contents for a restored snapshot, discard an unresolved recovery, or return success after a recorded integrity failure.

#### Findings

**R8-F1** · P1 · defect · blocks readiness — A delayed recovery can act on a newer interrupted session
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:436`; acceptance: 4.2
- Trigger: Session B replaces session A while a recovery caller for A is paused before acquiring the guard.
- Expected: A delayed recovery leaves a replacement transaction for its own recovery invocation.
- Actual: The first transaction ID is used only in a message. After acquiring the recovery guard, the caller adopts whichever journal it reads. The newer-session fixture refuses because B's owner is still alive. If that owner has exited, the same caller can restore B and remove its journal.
- Why it blocks / does not: A recovery requested for one session can write another session's settings and consume its recovery record.
- Requested correction: Compare the identity observed before waiting with the identity under the guard, before acquiring a lock or changing state. Use a stable identity for legacy journals as well.
- Requested evidence: Pause a recovery for A, finish A, create B and let B's owner exit. Resume A's caller and show that B's registry, journal and lock are untouched.

**R8-F2** · P2 · defect · blocks readiness — Partial legacy acquisition can still strand the shared lock
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:447`; acceptance: 4.2
- Trigger: Recovery stops while acquiring ownership for an unleased journal, or encounters a partially acquired schema-2 lease.
- Expected: Recovery can identify each lock it acquires from durable state, including during conversion of an older journal.
- Actual: Legacy recovery creates compatibility.lock with a fresh run ID before saving that ID to the converted journal. An interruption or failed journal write leaves a lock the next recovery classifies as foreign. A schema-2 acquisition interrupted after lease creation but before saving its run ID also loses the matching-lease lookup present in the previous runner.
- Why it blocks / does not: A supported interrupted session can block every subsequent shared runtime transaction and require manual lock removal.
- Requested correction: Persist the intended recovery lease ID before acquiring the shared lock. Reconcile an existing schema-2 lease only when its identity and session ownership can be established. Ensure interrupted lock-file creation also leaves an identifiable claim.
- Requested evidence: Interrupt legacy recovery between lock creation and journal publication, and exercise a schema-2 journal beside its matching partially acquired lease. Show recovery and subsequent acquisition while preserving a genuinely foreign lock.

**R8-F4** · P1 · defect · blocks readiness — An unleased legacy journal can replay a completed restore
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:383`; acceptance: 4.2
- Trigger: The older runner exits after importing its registry backup and before deleting its journal.
- Expected: Recovery skips an already-applied restore and preserves an explicit unresolved result when the older record cannot establish what happened.
- Actual: Conversion assigns pending to a pre-round-7 journal containing only sessionId and registryBackup. That runner imported the backup before removing the journal. If it stopped between those operations, the new runner treats the previous restore as outstanding and imports the backup again.
- Why it blocks / does not: The repeated delete-and-import can erase settings changed after the first successful restoration.
- Requested correction: Treat a legacy journal without a durable restore phase as ambiguous. Reconcile what can be verified and require resolution before overwriting different current settings.
- Requested evidence: Recover a pre-round-7 journal left after its import, once with the backup still applied and once after a later settings change. Show zero duplicate imports and preservation of the later settings.

**R9-F1** · P2 · defect · blocks readiness — The initial journal claim can be left unreadable
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:97`; acceptance: 4.2
- Trigger: The process stops or the write fails after CreateNew succeeds but before the initial JSON is durable.
- Expected: The first published journal is both exclusive and complete.
- Actual: New-LcJournal creates the final journal path and then writes and flushes its JSON. An interruption between those operations leaves an empty or truncated file. Preflight refuses another start and recovery cannot parse the claim. Atomic replacement protects later updates only.
- Why it blocks / does not: An interruption before any game or lease operation can disable both session startup and guarded recovery.
- Requested correction: Write and flush the initial record privately, then publish it with a same-volume atomic operation that refuses to replace an existing claim.
- Requested evidence: Interrupt the initial write before publication and interleave two starters. Show that the next caller finds either no claim or a complete winning journal.

**R9-F3** · P2 · defect · blocks readiness — Cleanup-only recovery can turn a failed transaction into exit 0
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:483`; acceptance: 4.2, 6
- Trigger: A session records failed integrity and stops after completing its lease but before removing its locks or journal.
- Expected: Finishing lock cleanup preserves the recorded transaction verdict in the command's exit code.
- Actual: The cleanup-only branch sets Succeeded from Outcome == 'completed' and bypasses Test-LcTransactionSucceeded. A journal with changed or unresolved integrity can therefore make -RecoverOnly exit 0. The child-process tests call the exit-code helper directly and miss this branch.
- Why it blocks / does not: Automation can advance after a known protected-file or registry failure.
- Requested correction: Evaluate the recorded registry and integrity results on cleanup-only recovery too. Represent unavailable legacy integrity evidence explicitly.
- Requested evidence: Interrupt after lease-completed with a changed or unreadable manifest. Exercise the recovery command's dispatch in a disposable fixture and capture its nonzero exit after cleanup. Keep a verified clean control.

**R10-F1** · P1 · defect · blocks readiness — Registry comparison loses the association between keys and values
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:240`; acceptance: 4.2
- Trigger: An interrupted restore is reconciled against exports containing multiple subkeys or multiline values.
- Expected: Registry equality preserves each key path, value name, type and ordered data.
- Actual: Get-LcRegistryFingerprint sorts all nonblank export lines together. Swapping two same-named values between subkeys leaves that fingerprint identical. Recovery can declare the wrong settings applied or decide that a different state is safe to overwrite.
- Why it blocks / does not: The comparison can certify exact restoration for a demonstrably different registry tree.
- Requested correction: Use an exact export comparison or canonicalize complete values within their registry keys, preserving continuation order.
- Requested evidence: Exercise swapped subkey values and reordered data in a multiline binary value. Both must compare unequal, and recovery must preserve an unrecognized current state.

**R10-F2** · P1 · defect · blocks readiness — An inconclusive registry restore releases ownership and drops its journal
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:312`; acceptance: 4.2, 6
- Trigger: Recovery cannot export or identify the current registry after an interrupted import.
- Expected: An inconclusive registry restore retains its backup, recovery record and ownership until restoration or explicit owner resolution.
- Actual: Resolve-LcInterruptedRestore returns an unresolved string when its registry export fails. Complete-LcTransaction records that result as phase restored, writes a Completed lease with recoveryRequired false, and removes the journal. A second recovery reports that there is no interrupted session.
- Why it blocks / does not: A temporary read failure or an interruption between registry deletion and import can leave owner settings unrestored while allowing another session to start.
- Requested correction: Keep unresolved registry reconciliation in RecoveryRequired. Preserve the evidence needed to retry a transient failure and distinguish a missing key from an unreadable export.
- Requested evidence: Make the reconciliation export fail once, then succeed. Verify that the first attempt returns failure and retains ownership, and that the later attempt can resolve the same transaction safely.

**R10-F3** · P1 · defect · blocks readiness — A peer can appear during the registry export and still be overwritten
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:298`; acceptance: 4.2
- Trigger: A game or sibling transaction starts after the initial host check while the pre-import snapshot is being taken.
- Expected: The runner confirms the host is free immediately before each destructive registry restore.
- Actual: The runner checks host conflicts, then exports the registry, hashes it and flushes a journal update before calling Restore. The reconciliation path also exports before deciding to import. A peer that starts during this work is first detected after the registry write.
- Why it blocks / does not: Deferring lock release after verification cannot undo overwriting settings while another game or sibling lab owns the host.
- Requested correction: Recheck host conflicts after snapshot or reconciliation work and before either Restore call. Persist a truthful deferred state if the import was withheld.
- Requested evidence: Have the registry-export fixture introduce a peer in both normal completion and recovery. Verify zero imports while the peer remains and retention of the journal and ownership.

**R10-F4** · P2 · defect · blocks readiness — Recovery can commit a stale owner-file integrity result
- Location: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1:315`; acceptance: 4.2, 6
- Trigger: The runner exits after saving its integrity comparison and before committing lease completion.
- Expected: Recovery verifies protected files against the recorded intake before committing an unfinished lease.
- Actual: Once phase verified is journaled, recovery skips Test-LcOwnerIntegrity whenever the cached integrity object exists. If a protected file changes while the runner is down and the host is free when recovery resumes, the older verified result is committed to the lease.
- Why it blocks / does not: The final record can report unchanged owner saves or installation files despite a detectable change before completion.
- Requested correction: Refresh the owner-file comparison when another process resumes a verified but uncommitted transaction. Preserve the durable registry restore result.
- Requested evidence: Stop at the verified fixture boundary, change one fixture manifest, then recover. The result must record the change and fail without a second registry import.

**R10-F5** · P2 · defect · blocks readiness — The registry backup is not bound to its captured contents
- Location: `scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1:173`; acceptance: 4.2
- Trigger: The backup changes between initial export and finalization or a later recovery invocation.
- Expected: The bytes used to restore owner settings are verified against the session's intake before the registry is changed.
- Actual: The journal records only the path to game-settings.reg. The original export has no recorded digest. Both normal restore and recovery trust the bytes later found at that path, although the pre-restore export and owner manifests have digest checks.
- Why it blocks / does not: A replaced or damaged backup can be imported as the owner's original settings after the live key has been deleted.
- Requested correction: Bind the registry backup to a digest at intake and verify the restore source before comparison or delete-and-import. Retain recovery state if that identity cannot be verified.
- Requested evidence: Replace the fixture backup with a different valid export after intake. Show that completion and recovery refuse it before any registry mutation.

#### Prior finding assessments

- **R8-F1** — `fix_rejected` — The guard serializes callers and the live-owner fixture passes. The first-read identity is never compared with the replacement journal, so the protection ends when the newer owner exits.
- **R8-F2** — `fix_rejected` — Normal schema-3 admission now records its lease identity before acquisition, and the tested lease-write and private-lock failures recover. The legacy recovery path still acquires a shared lock before durably recording its ID.
- **R8-F4** — `fix_rejected` — The tested schema-3 import interruption and Completed round-7 lease avoid a second import. An unleased journal from the earlier supported runner still defaults to pending even if that runner already restored the registry.
- **R9-F1** — `fix_rejected` — Concurrent starters now have one winner, and File.Replace protects subsequent updates. The initial claim still exposes the final journal path before a complete record exists.
- **R9-F2** — `fix_verified` — The post-comparison host check handles the reported interleaving. The passing fixture introduces peer PID 5151 during manifest verification, retains both locks and the applied restore, then completes after the peer leaves without reimporting.
- **R9-F3** — `fix_rejected` — The main session and ordinary recovery propagate failures. The cleanup-only branch independently computes success and can override a recorded failed result.
- **R9-F4** — `fix_verified` — The CLI transcript confirms leased B9 and both targeted S9 launches on the pinned packages. Their records cover the donor mutation, control weapon, repeated price and tooltip queries, diagnostic counts and final restoration.
- **R9-F5** — `fix_verified` — Section 4.2 now limits the in-process identity claim to sessions that launched Kingmaker. Its description of F8 matches the recorded missing-fixture failure during setup.

#### Evidence assessment

- **Inspected:** The fixed mission, repository validation and review instructions, 32-file feature diff, investigation record, prior findings ledger, developer transcript and gate receipt.; Spear construction and validation, selector publication and rollback, battered-origin inspection and binding, Expanded Summoning publication and menu hooks, and the native pricing, sorcerer and persistence probes.; The loader-compat entry point, transaction and safety fixtures, shared lease helpers, and earlier runner versions that produced the supported legacy journals.
- **Executed verification:** The controller receipt records production compilation and 30 of 30 selected elven-branched-spear tests passing on 1897d22d897d, with exit 0 and an unchanged worktree in 2.2 seconds. The CLI transcript confirms the guarded push and 87 passing offline safety checks.; Leased baseline B9 ran the published v0.0.192 DLL 0eff2af36f15 under UMM 0.31.1.0. With the simulated 15 ft, Polearms donor, startup failed. Longspear and Longsword each threw the missing-token exception on all five price queries and all five tooltip fills.; Leased S9 ran product DLL 6b2696c70988 from package 253fbe43188d. The mutated-donor launch passed its owned-profile and unchanged-control checks. The invalid-owned-profile launch emitted one fighter-group diagnostic and one unavailable-state warning. Each of four ordinary weapons completed 25 price queries and 25 tooltip fills without throwing, at 1, 4, 5 and 13 gp respectively. The native single-spell summon also passed.; B9 and S9 recorded completed ownership, registry restoration applied and owner-file integrity verified across 95 saves. Their runner revision was 2c84b833a8. Git confirms that only the investigation document changed between that revision and the review candidate.; A read-only, in-memory comparison using the candidate's registry line-sorting expression produced identical fingerprints for two different exports with values exchanged between subkeys.
- **Source inspection only:** The product source tree at the candidate matches both 6d6a295b64 and the packaged revision 4e0ac475aa. The pricing exemption is confined to inspection. Strict battered-item binding and owner lookup remain in use.; The recorded reporter-configuration session exercises native spell selection, visible creature rows, the targeting handler, the submitted cast and the resulting Fire Beetle. The persistence fixture checks the loaded spear and battered owner with Gunslinger publication disabled.; The interruption and recovery defects below follow from the candidate's control flow. The registry fingerprint collision was also checked independently in memory.
- **Required gaps:** The transaction needs focused fixture evidence for preservation of a newer journal, interruption during initial claims, and recovery of legacy ownership and restore state.; Registry recovery needs evidence that it distinguishes different key contents, verifies the intake backup, withholds imports while a peer is active, and retains recoverable state after an inconclusive comparison.; The recovery entry point needs failing exit-code evidence for unsuccessful cleanup-only results and a fresh owner-file comparison after interruption before lease completion.
- **Limitations:** The review used the read-only snapshot. Native observations are attributed to their recorded runs. The current transcript covers B9 and S9. Earlier review records cover the reporter-configuration summon and fresh-process persistence checks.; The reporter's raw log and full 43-mod installation remain unavailable. The simulated donor mutation establishes a failure mechanism. The reporter's exact failing predicate and external interaction remain unknown.; The failure control exercised Kingmaker's unexpanded single spell. A creature menu that opened empty was never observed. Contention and interruption coverage comes from offline fixtures.

#### Summary

- **Delivered behavior:** The owned spear receives and validates its fixed mechanical profile. A genuine profile failure remains visible, and ordinary pricing and tooltip inspection continue to work after rollback. Earlier native records cover a visible seven-choice Summon Monster I menu, a successful Fire Beetle cast, and a fresh-process reload retaining the battered owner and 22 gp price with Gunslinger disabled.
- **Scope reviewed:** The complete final mission for PR #195, including gameplay, save compatibility, native evidence, the focused gate, and the shared-host runner used for verification.
- **Pending later-phase work:** none
- **Unresolved questions:** Which spear predicate failed in the reporter's exact 43-mod installation, and what changed it?
- **Remaining owner review:** Assess player-facing menu and tooltip presentation and the applicability of the partial-mod configuration to the original report.; Retain the merge and release decision after the blocking restoration findings are resolved.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a2009b-9072-76d0-bc40-480a3ae1bfe6`, duration 803.3s, response sha256 `5b2206db851489265231460ef9440b28687e85a029e7bc51d1c03fea4d67ca0d`, request `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r10-ae6a54`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "1897d22d897d123e1205a8ff4030fba3eb828165",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "The native evidence supports the gameplay repair. The candidate constructed the intended spear under the simulated donor mutation, and an intentionally failed startup preserved ordinary weapon prices and tooltip data. The final checkpoint still fails the shared-host restoration requirement. The runner can mistake different registry contents for a restored snapshot, discard an unresolved recovery, or return success after a recorded integrity failure.",
  "evidence_assessment": {
    "executed_verification": [
      "The controller receipt records production compilation and 30 of 30 selected elven-branched-spear tests passing on 1897d22d897d, with exit 0 and an unchanged worktree in 2.2 seconds. The CLI transcript confirms the guarded push and 87 passing offline safety checks.",
      "Leased baseline B9 ran the published v0.0.192 DLL 0eff2af36f15 under UMM 0.31.1.0. With the simulated 15 ft, Polearms donor, startup failed. Longspear and Longsword each threw the missing-token exception on all five price queries and all five tooltip fills.",
      "Leased S9 ran product DLL 6b2696c70988 from package 253fbe43188d. The mutated-donor launch passed its owned-profile and unchanged-control checks. The invalid-owned-profile launch emitted one fighter-group diagnostic and one unavailable-state warning. Each of four ordinary weapons completed 25 price queries and 25 tooltip fills without throwing, at 1, 4, 5 and 13 gp respectively. The native single-spell summon also passed.",
      "B9 and S9 recorded completed ownership, registry restoration applied and owner-file integrity verified across 95 saves. Their runner revision was 2c84b833a8. Git confirms that only the investigation document changed between that revision and the review candidate.",
      "A read-only, in-memory comparison using the candidate's registry line-sorting expression produced identical fingerprints for two different exports with values exchanged between subkeys."
    ],
    "inspected": [
      "The fixed mission, repository validation and review instructions, 32-file feature diff, investigation record, prior findings ledger, developer transcript and gate receipt.",
      "Spear construction and validation, selector publication and rollback, battered-origin inspection and binding, Expanded Summoning publication and menu hooks, and the native pricing, sorcerer and persistence probes.",
      "The loader-compat entry point, transaction and safety fixtures, shared lease helpers, and earlier runner versions that produced the supported legacy journals."
    ],
    "limitations": [
      "The review used the read-only snapshot. Native observations are attributed to their recorded runs. The current transcript covers B9 and S9. Earlier review records cover the reporter-configuration summon and fresh-process persistence checks.",
      "The reporter's raw log and full 43-mod installation remain unavailable. The simulated donor mutation establishes a failure mechanism. The reporter's exact failing predicate and external interaction remain unknown.",
      "The failure control exercised Kingmaker's unexpanded single spell. A creature menu that opened empty was never observed. Contention and interruption coverage comes from offline fixtures."
    ],
    "required_gaps": [
      "The transaction needs focused fixture evidence for preservation of a newer journal, interruption during initial claims, and recovery of legacy ownership and restore state.",
      "Registry recovery needs evidence that it distinguishes different key contents, verifies the intake backup, withholds imports while a peer is active, and retains recoverable state after an inconclusive comparison.",
      "The recovery entry point needs failing exit-code evidence for unsuccessful cleanup-only results and a fresh owner-file comparison after interruption before lease completion."
    ],
    "source_inspection_only": [
      "The product source tree at the candidate matches both 6d6a295b64 and the packaged revision 4e0ac475aa. The pricing exemption is confined to inspection. Strict battered-item binding and owner lookup remain in use.",
      "The recorded reporter-configuration session exercises native spell selection, visible creature rows, the targeting handler, the submitted cast and the resulting Fire Beetle. The persistence fixture checks the loaded spear and battered owner with Gunslinger publication disabled.",
      "The interruption and recovery defects below follow from the candidate's control flow. The registry fingerprint collision was also checked independently in memory."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "The first transaction ID is used only in a message. After acquiring the recovery guard, the caller adopts whichever journal it reads. The newer-session fixture refuses because B's owner is still alive. If that owner has exited, the same caller can restore B and remove its journal.",
      "blocking_rationale": "A recovery requested for one session can write another session's settings and consume its recovery record.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A delayed recovery leaves a replacement transaction for its own recovery invocation.",
      "id": "R8-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 436
      },
      "priority": "P1",
      "requested_correction": "Compare the identity observed before waiting with the identity under the guard, before acquiring a lock or changing state. Use a stable identity for legacy journals as well.",
      "requested_evidence": "Pause a recovery for A, finish A, create B and let B's owner exit. Resume A's caller and show that B's registry, journal and lock are untouched.",
      "title": "A delayed recovery can act on a newer interrupted session",
      "trigger": "Session B replaces session A while a recovery caller for A is paused before acquiring the guard."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Legacy recovery creates compatibility.lock with a fresh run ID before saving that ID to the converted journal. An interruption or failed journal write leaves a lock the next recovery classifies as foreign. A schema-2 acquisition interrupted after lease creation but before saving its run ID also loses the matching-lease lookup present in the previous runner.",
      "blocking_rationale": "A supported interrupted session can block every subsequent shared runtime transaction and require manual lock removal.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Recovery can identify each lock it acquires from durable state, including during conversion of an older journal.",
      "id": "R8-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 447
      },
      "priority": "P2",
      "requested_correction": "Persist the intended recovery lease ID before acquiring the shared lock. Reconcile an existing schema-2 lease only when its identity and session ownership can be established. Ensure interrupted lock-file creation also leaves an identifiable claim.",
      "requested_evidence": "Interrupt legacy recovery between lock creation and journal publication, and exercise a schema-2 journal beside its matching partially acquired lease. Show recovery and subsequent acquisition while preserving a genuinely foreign lock.",
      "title": "Partial legacy acquisition can still strand the shared lock",
      "trigger": "Recovery stops while acquiring ownership for an unleased journal, or encounters a partially acquired schema-2 lease."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Conversion assigns pending to a pre-round-7 journal containing only sessionId and registryBackup. That runner imported the backup before removing the journal. If it stopped between those operations, the new runner treats the previous restore as outstanding and imports the backup again.",
      "blocking_rationale": "The repeated delete-and-import can erase settings changed after the first successful restoration.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Recovery skips an already-applied restore and preserves an explicit unresolved result when the older record cannot establish what happened.",
      "id": "R8-F4",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 383
      },
      "priority": "P1",
      "requested_correction": "Treat a legacy journal without a durable restore phase as ambiguous. Reconcile what can be verified and require resolution before overwriting different current settings.",
      "requested_evidence": "Recover a pre-round-7 journal left after its import, once with the backup still applied and once after a later settings change. Show zero duplicate imports and preservation of the later settings.",
      "title": "An unleased legacy journal can replay a completed restore",
      "trigger": "The older runner exits after importing its registry backup and before deleting its journal."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "New-LcJournal creates the final journal path and then writes and flushes its JSON. An interruption between those operations leaves an empty or truncated file. Preflight refuses another start and recovery cannot parse the claim. Atomic replacement protects later updates only.",
      "blocking_rationale": "An interruption before any game or lease operation can disable both session startup and guarded recovery.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The first published journal is both exclusive and complete.",
      "id": "R9-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 97
      },
      "priority": "P2",
      "requested_correction": "Write and flush the initial record privately, then publish it with a same-volume atomic operation that refuses to replace an existing claim.",
      "requested_evidence": "Interrupt the initial write before publication and interleave two starters. Show that the next caller finds either no claim or a complete winning journal.",
      "title": "The initial journal claim can be left unreadable",
      "trigger": "The process stops or the write fails after CreateNew succeeds but before the initial JSON is durable."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "The cleanup-only branch sets Succeeded from Outcome == 'completed' and bypasses Test-LcTransactionSucceeded. A journal with changed or unresolved integrity can therefore make -RecoverOnly exit 0. The child-process tests call the exit-code helper directly and miss this branch.",
      "blocking_rationale": "Automation can advance after a known protected-file or registry failure.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Finishing lock cleanup preserves the recorded transaction verdict in the command's exit code.",
      "id": "R9-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 483
      },
      "priority": "P2",
      "requested_correction": "Evaluate the recorded registry and integrity results on cleanup-only recovery too. Represent unavailable legacy integrity evidence explicitly.",
      "requested_evidence": "Interrupt after lease-completed with a changed or unreadable manifest. Exercise the recovery command's dispatch in a disposable fixture and capture its nonzero exit after cleanup. Keep a verified clean control.",
      "title": "Cleanup-only recovery can turn a failed transaction into exit 0",
      "trigger": "A session records failed integrity and stops after completing its lease but before removing its locks or journal."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "Get-LcRegistryFingerprint sorts all nonblank export lines together. Swapping two same-named values between subkeys leaves that fingerprint identical. Recovery can declare the wrong settings applied or decide that a different state is safe to overwrite.",
      "blocking_rationale": "The comparison can certify exact restoration for a demonstrably different registry tree.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Registry equality preserves each key path, value name, type and ordered data.",
      "id": "R10-F1",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 240
      },
      "priority": "P1",
      "requested_correction": "Use an exact export comparison or canonicalize complete values within their registry keys, preserving continuation order.",
      "requested_evidence": "Exercise swapped subkey values and reordered data in a multiline binary value. Both must compare unequal, and recovery must preserve an unrecognized current state.",
      "title": "Registry comparison loses the association between keys and values",
      "trigger": "An interrupted restore is reconciled against exports containing multiple subkeys or multiline values."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "Resolve-LcInterruptedRestore returns an unresolved string when its registry export fails. Complete-LcTransaction records that result as phase restored, writes a Completed lease with recoveryRequired false, and removes the journal. A second recovery reports that there is no interrupted session.",
      "blocking_rationale": "A temporary read failure or an interruption between registry deletion and import can leave owner settings unrestored while allowing another session to start.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An inconclusive registry restore retains its backup, recovery record and ownership until restoration or explicit owner resolution.",
      "id": "R10-F2",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 312
      },
      "priority": "P1",
      "requested_correction": "Keep unresolved registry reconciliation in RecoveryRequired. Preserve the evidence needed to retry a transient failure and distinguish a missing key from an unreadable export.",
      "requested_evidence": "Make the reconciliation export fail once, then succeed. Verify that the first attempt returns failure and retains ownership, and that the later attempt can resolve the same transaction safely.",
      "title": "An inconclusive registry restore releases ownership and drops its journal",
      "trigger": "Recovery cannot export or identify the current registry after an interrupted import."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "The runner checks host conflicts, then exports the registry, hashes it and flushes a journal update before calling Restore. The reconciliation path also exports before deciding to import. A peer that starts during this work is first detected after the registry write.",
      "blocking_rationale": "Deferring lock release after verification cannot undo overwriting settings while another game or sibling lab owns the host.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The runner confirms the host is free immediately before each destructive registry restore.",
      "id": "R10-F3",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 298
      },
      "priority": "P1",
      "requested_correction": "Recheck host conflicts after snapshot or reconciliation work and before either Restore call. Persist a truthful deferred state if the import was withheld.",
      "requested_evidence": "Have the registry-export fixture introduce a peer in both normal completion and recovery. Verify zero imports while the peer remains and retention of the journal and ownership.",
      "title": "A peer can appear during the registry export and still be overwritten",
      "trigger": "A game or sibling transaction starts after the initial host check while the pre-import snapshot is being taken."
    },
    {
      "acceptance_ids": [
        "4.2",
        "6"
      ],
      "actual": "Once phase verified is journaled, recovery skips Test-LcOwnerIntegrity whenever the cached integrity object exists. If a protected file changes while the runner is down and the host is free when recovery resumes, the older verified result is committed to the lease.",
      "blocking_rationale": "The final record can report unchanged owner saves or installation files despite a detectable change before completion.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Recovery verifies protected files against the recorded intake before committing an unfinished lease.",
      "id": "R10-F4",
      "location": {
        "file": "scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1",
        "line": 315
      },
      "priority": "P2",
      "requested_correction": "Refresh the owner-file comparison when another process resumes a verified but uncommitted transaction. Preserve the durable registry restore result.",
      "requested_evidence": "Stop at the verified fixture boundary, change one fixture manifest, then recover. The result must record the change and fail without a second registry import.",
      "title": "Recovery can commit a stale owner-file integrity result",
      "trigger": "The runner exits after saving its integrity comparison and before committing lease completion."
    },
    {
      "acceptance_ids": [
        "4.2"
      ],
      "actual": "The journal records only the path to game-settings.reg. The original export has no recorded digest. Both normal restore and recovery trust the bytes later found at that path, although the pre-restore export and owner manifests have digest checks.",
      "blocking_rationale": "A replaced or damaged backup can be imported as the owner's original settings after the live key has been deleted.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The bytes used to restore owner settings are verified against the session's intake before the registry is changed.",
      "id": "R10-F5",
      "location": {
        "file": "scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1",
        "line": 173
      },
      "priority": "P2",
      "requested_correction": "Bind the registry backup to a digest at intake and verify the restore source before comparison or delete-and-import. Retain recovery state if that identity cannot be verified.",
      "requested_evidence": "Replace the fixture backup with a different valid export after intake. Show that completion and recovery refuse it before any registry mutation.",
      "title": "The registry backup is not bound to its captured contents",
      "trigger": "The backup changes between initial export and finalization or a later recovery invocation."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F1",
      "rationale": "The guard serializes callers and the live-owner fixture passes. The first-read identity is never compared with the replacement journal, so the protection ends when the newer owner exits."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F2",
      "rationale": "Normal schema-3 admission now records its lease identity before acquisition, and the tested lease-write and private-lock failures recover. The legacy recovery path still acquires a shared lock before durably recording its ID."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R8-F4",
      "rationale": "The tested schema-3 import interruption and Completed round-7 lease avoid a second import. An unleased journal from the earlier supported runner still defaults to pending even if that runner already restored the registry."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R9-F1",
      "rationale": "Concurrent starters now have one winner, and File.Replace protects subsequent updates. The initial claim still exposes the final journal path before a complete record exists."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R9-F2",
      "rationale": "The post-comparison host check handles the reported interleaving. The passing fixture introduces peer PID 5151 during manifest verification, retains both locks and the applied restore, then completes after the peer leaves without reimporting."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R9-F3",
      "rationale": "The main session and ordinary recovery propagate failures. The cleanup-only branch independently computes success and can override a recorded failed result."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R9-F4",
      "rationale": "The CLI transcript confirms leased B9 and both targeted S9 launches on the pinned packages. Their records cover the donor mutation, control weapon, repeated price and tooltip queries, diagnostic counts and final restoration."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R9-F5",
      "rationale": "Section 4.2 now limits the in-process identity claim to sessions that launched Kingmaker. Its description of F8 matches the recorded missing-fixture failure during setup."
    }
  ],
  "request_id": "kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r10-ae6a54",
  "round": 10,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The owned spear receives and validates its fixed mechanical profile. A genuine profile failure remains visible, and ordinary pricing and tooltip inspection continue to work after rollback. Earlier native records cover a visible seven-choice Summon Monster I menu, a successful Fire Beetle cast, and a fresh-process reload retaining the battered owner and 22 gp price with Gunslinger disabled.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "Assess player-facing menu and tooltip presentation and the applicability of the partial-mod configuration to the original report.",
      "Retain the merge and release decision after the blocking restoration findings are resolved."
    ],
    "scope_reviewed": "The complete final mission for PR #195, including gameplay, save compatibility, native evidence, the focused gate, and the shared-host runner used for verification.",
    "unresolved_questions": [
      "Which spear predicate failed in the reporter's exact 43-mod installation, and what changed it?"
    ]
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
