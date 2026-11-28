# Sprint 7 master integration: development checkpoint mission

This is a new, separate integration task. It does not replace, resume or extend the completed Sprint 7 parent
enrollment (PR #219), and it does not reset any of the parent's or children's counters or ledgers.

- **Endpoint.** A reviewed integration development checkpoint: the accepted Sprint 7 parent source combined with current
  `master`, with production compilation and focused validation. Packaged/native qualification and release are later
  boundaries and are not part of this mission.
- **Review ceiling.** At most four formal reviews (`--max-rounds 4`), with the normal independent reviewer and permission
  controls. The ceiling is not expanded automatically.
- **Integration record.** [`S7-MASTER-INTEGRATION-RECORD.md`](S7-MASTER-INTEGRATION-RECORD.md) in this folder is the
  concise account the owner asked for (§6 below).
- **Prior evidence.** The accepted parent and child reviews are prior evidence, cited by immutable links in the record.
  This mission does not ask the reviewer to repeat the earlier Sprint 7 investigations. The reviewer assesses the combined
  source, the conflict resolutions, build and test registration, and integration-specific risks.

The contract is the owner's authorization of 2026-11-28, reproduced verbatim below. It is saved outside the repository at
`C:\Dev\KingmakerGunslingerLab\owner-guidance\S7-MASTER-INTEGRATION-AUTHORIZATION-2026-11-28.md`
(SHA-256 `910a81a8cf58a7cadad3228e9964d5c09a6de08483deee6c307ffa51119e7406`).

---

I accept the completed technical scope of Sprint 7 parent PR #219 at:

- Reviewed H: `3ebdda17dac096156974a118474649070b68521c`
- Archive A: `d682b6bd4e443e685257a12eb24b00878f9c4114`

Preserve the completed parent enrollment and these identities. This acceptance does not grant visual or listening approval, permission to merge into master, or permission to release.

The next authorized task is a separate integration development checkpoint against current master.

### 1. Freeze the completed parent

Leave PR #219, its branch, its controller enrollment, and the accepted child branches and archives unchanged.

Do not resume the parent, amend its mission again, request review 10, or make another parent records commit.

Retain the final parent ledger of 71 min 40.3 s out of 85 minutes. Keep the C3 ledger separate and unchanged. Neither remaining balance becomes the budget for this new integration task.

Acknowledge R9-F1 as an accepted non-blocking handoff clarification. Do not mark it formally fixed or closed. Include this clarification in the owner-facing integration account:

“The restriction in the parent record’s §3 concerns the model, tooltip and Dust dependency comparison. Session 20261124T050957Z also contains supplementary T1 Ghaele gh/ghd observations and six indexed stills. Those remain evidence on their original candidate. They do not establish the missing released outdoor/bright observation or qualify the later Dust spawn-ownership repair.”

### 2. Establish a separate integration task

Use a new dedicated worktree and `codex/*` integration branch, preserving all existing worktrees and refs.

Use the supported foreground workflow to create a separate mission/enrollment and draft PR against master. This is new integration scope, not a replacement for the completed parent or a reset of its counters.

I authorize creating and pushing that integration branch, opening the draft PR, and the controller’s normal review and archive operations for this task.

The mission endpoint is a reviewed integration development checkpoint: the accepted Sprint 7 source combined with current master, with compilation and focused validation. Packaged/native qualification and release are later boundaries.

Allow at most four formal reviews for this integration checkpoint. Keep the normal independent reviewer and permission controls. Do not expand the review ceiling automatically.

Read the current repository instructions, validation policy, building instructions, and installed workflow before selecting commands.

### 3. Integrate the reviewed source

Refresh master and record its actual starting SHA. The presently reviewed target is:

`52755b617c79328a972ae5b74bc05784422e332d`

Start the integration branch from the actual current master. If master has advanced, inspect and record the additional changes and their integration implications.

I authorize merging the accepted parent reviewed source commit:

`3ebdda17dac096156974a118474649070b68521c`

into the new integration branch only.

Use that reviewed source commit as the integration input. Preserve archive A as the immutable audit record; do not import its 43 parent transcript files merely to carry the review history into the new enrollment.

Preserve history. Do not rebase, squash, force-push, reset, or modify any accepted parent or child ref.

Before spending gate time, compute the integration candidate’s actual merge base and inspect its net change under `docs/agent-reviews/`. The new enrollment must not inherit foreign transcript additions or modify master’s existing archive records.

Use immutable links to parent archive A and the accepted child archives for the audit trail. Do not copy those records into another directory or modify the controller’s archive inventory or enforcement.

### 4. Reconcile both sides’ contracts

Inspect the actual overlap rather than relying on a clean textual merge.

At the currently known inputs, the four shared modified paths are:

- `scripts/validate-package.ps1`
- `src/KingmakerGunslinger/KingmakerGunslinger.csproj`
- `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj`
- `tests/KingmakerGunslinger.DomainTests/Program.cs`

Preserve all accepted Sprint 7 product code, assets, compile items and registered tests.

Preserve master’s current version metadata. Do not downgrade 0.0.197 to the parent’s historical 0.0.195, and do not invent a release-version increment for this checkpoint.

Preserve #220’s pinned loader references, compiler reference enforcement, strict archive-entry validation, packaged-DLL/receipt identity checks, embedded source stamp, and development-candidate admission rules.

Resolve actual merge and compilation issues within the integration scope. Do not change accepted gameplay behavior, provisional audio choices, visual assets, blueprint identities or save contracts as incidental cleanup.

If integration reveals a substantive new defect requiring a separate gameplay or observer repair, identify the precise problem and its affected dependency. Do not silently turn this task into a replay of the original Sprint 7 mission.

### 5. Focused validation

Use a new, separately reported validation ledger for this integration checkpoint.

Apply the repository’s ordinary 15-minute planning target and 30-minute ceiling. Charge full outer command time. Preserve safe completion and restoration obligations if any unexpected transaction is already in progress.

On the final clean integration submission, run the configured focused gate selections needed to establish:

- Sprint 7 production compilation and the `sprint7-visual` cases.
- The merged production DLL’s pinned loader contract through `loader.production-dll-references-pinned-loader`.

Evaluate `native-profile`, `loader-compat-probe`, and `summon-playtest-r6` against the actual merged runner and dependency changes. Run them when they cover an identified integration risk, and explain that connection. Do not replay all eleven historical selections or a broad matrix by default.

Reject zero-test selections. Preserve failures and distinguish baseline failures from integration regressions. Add a test only for a concrete behavioral risk introduced or exposed by an actual correction.

Use the normal controller receipt mechanism and push wrapper. Keep required gate receipts bound to the submitted integration head. Do not relabel any parent or child receipt as execution on the merged tree.

No new package build, deployment, game session, screenshot capture, audio capture, listening campaign or compatibility matrix is authorized for this development checkpoint.

### 6. Review the integration at its actual scope

Prepare one concise integration record identifying:

- Starting master SHA.
- Accepted parent H and archive A.
- Integration head and any reconciliation commits.
- Actual overlapping paths and how both contracts were preserved.
- Protected transcript-path preflight.
- Focused commands, counts, results, receipts and outer durations.
- The inherited evidence and its original identities.
- Remaining owner decisions.
- The later package/native qualification boundary.

Keep the completed parent’s dated records and archived reviews intact. Cite them rather than rewriting them as if they were produced on the integration candidate.

The independent reviewer should assess the combined source, conflict resolutions, build/test registration and integration-specific risks. Supply accepted parent and child reviews as prior evidence. Do not present this mission as a request to repeat all earlier Sprint 7 investigations.

Correct integration findings within this scope and the four-review limit. Once the integration development checkpoint is ready, perform normal finalization and remote handoff verification for the new enrollment.

### 7. Preserve the package and native boundaries

Candidate 6 remains the accepted historical mechanics package. It does not qualify the merged tree.

The later packaged integration boundary must build a fresh candidate from the actual merged source and use #220’s strict validation and admission. An unchanged candidate should then be reused under its verified identity.

For this checkpoint, report packaging and native execution as not run. Do not describe successful compilation or focused tests as native qualification.

Carry forward the existing limitations:

- Candidate 6’s seven mechanics cells are distinct from the seven parent stories.
- Earlier presentation evidence stays on its producing candidates.
- C2’s Ghaele lifecycle evidence stays on candidate 3.
- Constructed callback overlap, source-derived cancellation guarantees, and focused-only transitions retain their stated scope.
- The October Ghaele incident remains unattributed.
- Accepted controller-rendering and other unexecuted dispositions remain unchanged.

### 8. Owner decisions and delivery

T2/T3/T6 visual acceptance and T5/T7 listening choices remain pending with me. Keep the provisional cues unchanged.

Include direct immutable links to the existing visual sets and the exact existing private listening-packet location. Reuse the current material; do not regenerate it to make a newer packet.

Do not merge into master, close or retarget the accepted PRs, tag, release, perform repository maintenance, or touch the human installation.

Finish with the new draft PR, exact integrated and reviewed/archive heads, focused validation results and ledger, the integration decisions made, any remaining findings, and a clear statement of what is still required before a packaged/native integration qualification or release.
