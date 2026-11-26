<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 key=r05-request part=1/1 sha256=2dabbed8e6cd7841a26587f790e887ea0a27cced6d1005afbc6c46d1da9dfab3 -->
### Claude Opus — review request — round 5 — candidate `da811b123c`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r05-25f1b7` |
| Checkpoint | `final` (final) |
| Candidate H | `da811b123c5720ad2d174db17142d8f4ef8fa5d6` |
| Target B (`codex/s7-recovery-c1-cue-evaluator`) | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Merge base M | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Mission | `s7-recovery-c2-mission` sha256 `f54c407bac` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 2, sha256 `c94d3ad031dd6b5f781374cccd1d6c198b3819061749b2680f9c2134810ac2ee` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 5 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C2, review 5 of 5 under OWNER C2 AMENDMENT 2 (revision 2), on H=da811b123c, PR #222 targeting C1's branch at its archive A 2c67a049ee. Round 4 verified R3-F2 and R3-F3 and left two domain-evidence gaps; round 5 closes both in the existing focused test without changing the harness: the creation queue is bound to the shared cast-release boundary with a detector control (R3-F1), and the registered foreign control's teardown request is ordered after a teardown-only sentinel so a reconciliation request would be visible (R4-F1). Production harness unchanged since d631d283aa (harness d77754bad437); its native lifecycle session 20261126T033419Z PASS on the unchanged candidate-3 package is reused as amendment 2 allows. All earlier verified fixes preserved. No product package (0 of 2). Recovery validation 14 min 38.6 s of 45. C3 retains the combined product qualification.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter sprint7-visual | yes | pass | 4.7s | 23 tests on H, production compilation; receipt 2.7s, worktree unchanged. Includes the round-5 detector control and the request-order assertions in sprint7-visual.ghaele-late-units-reach-slot. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter loader-compat-probe | yes | pass | 3.8s | 5 tests on H; receipt 1.9s. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, round-5 source) | no | pass | 3.8s | 23 tests; test binary rebuilt after the edit (timestamps checked). |
| Native session reuse: 20261126T033419Z-owner-0.33.0-s7-ghaele-lifecycle | no | pass | 0s (no new run) | Round 5 changed only tests/ and docs/: git diff d631d283aa..H outside docs/ and tests/ is empty, so tools/LoaderCompatHarness and scripts/compatibility/loader-compat/sessions/s7-ghaele-lifecycle.json are byte-identical to the source of harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. As C2 amendment 2 section 6 allows, the round-4 session PASS on candidate 3 (package 49f8b88c...6413, DLL 5969f596...e2c3; restoration, owner files and registry verified) qualifies the same harness bytes; no new native session was run. |
| Shared recovery ledger | no | pass | 14m38.6s | Cumulative recovery validation after these gates: 14 min 38.6 s of 45, remaining 30 min 21.4 s (the committed ledger stops at 14 min 30.1 s, before these controller gates: 8.5s batch wall, receipts 4.6s). No failed validation attempt in C2. Product candidates 0 of 2. C3 estimate about 25 minutes. |

**Evidence references:**
- prepared under owner amendments revision 2 sha256 c94d3ad031dd6b5f781374cccd1d6c198b3819061749b2680f9c2134810ac2ee
- docs/missions/sprint7/recovery/S7-RECOVERY-C2-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- docs/evidence/sprint7/recovery-c2/MANIFEST.json
- docs/evidence/sprint7/recovery-c2/20261126T033419Z-owner-0.33.0-s7-ghaele-lifecycle-01-ghaele.json
- tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs
- tools/LoaderCompatHarness/TrumpetCueVerdict.cs
- tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs

**Prior findings to assess:** `R3-F1`, `R4-F1`

**Owner amendments to the mission — revision 2, captured-source sha256 `c94d3ad031dd6b5f781374cccd1d6c198b3819061749b2680f9c2134810ac2ee`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

````text
# Owner amendments to the mission

Cumulative record kept by the agent-review controller. Each owner message below was applied by the owner through `resume --amend-mission` and is preserved verbatim, in order, between its labeled marker lines. Later amendments supersede conflicting earlier requirements; requirements they do not change still apply.

----- BEGIN OWNER AMENDMENT 1 (captured 2026-11-26T02:43:00Z; message sha256 6acde0306dbe04787f541d68c064c6ef6420ac532f149abbbc056b967fd18774) -----
SPRINT 7 RECOVERY — C2 AMENDMENT 1: LATE-UNIT CLEANUP  
Owner instruction, 2026-11-25

I accept C1 for its amended implementation/domain scope and authorize one cohesive C2 correction pass followed by one additional formal review on its existing enrollment. Increase C2’s total review cap from two to three. This does not increase the shared validation allowance or authorize another product candidate for C2.

The published C2 round-2 review contains three blocking findings. The handoff omitted R2-F1, a P1. Correct that omission. R2-F1, R2-F2 and R1-F7 all remain required; none is waived by this amendment.

## 1. Preserve identities and dependencies

C1, PR #221:

- Reviewed H: `5a56d480a2827e0ddd3918e30d32a07c7554e112`.
- Archive A: `2c67a049ee33e0b29621fd29a8b25608ed31e956`.
- Acceptance covers implementation/domain behavior only. C3 still owns native execution of its revised evaluator.
- Record this owner acceptance in the descendant recovery manifest. Keep C1’s branch and archive frozen.

C2, PR #222:

- Branch: `codex/s7-recovery-c2-ghaele-late-units`.
- Target: `codex/s7-recovery-c1-cue-evaluator`, at the C1 archive above.
- Enrollment: `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08`.
- Reviewed H: `525a27751b95fd51f3080b2a7865719532fff298`.
- Reported head: `b055db4fbdec9c7fd22bb096941fe3cdabd1971b`, including the records-only stop.
- Review: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/222#issuecomment-7179452273.

Verify current remote identities and controller state before proceeding. Preserve concurrent work if anything has moved.

Keep parent #219 frozen at `0542b0592a668aad0c62c66d5f2dc862460818fe` and BLOCKED at 7/7. Preserve all original findings, verdicts, amendments and artifact identities. C3 has not started and may begin only after C2 has a current scoped handoff.

## 2. Register the amendment through the existing controller

Save this complete instruction verbatim outside the worktree as:

`C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C2-AMENDMENT-1-2026-11-25.md`

Use the installed controller, `C:/Dev/agent-review/agent_review.py`. Read its current help and instructions.

After checking `status --remote`, apply this document with `resume --amend-mission` on the exact C2 enrollment. Then use a separate `resume` command with `--max-rounds 3` and a reason citing this owner authorization. Do not combine amendment capture and the cap change in one command.

The expected command sequence is:

```text
python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --reason "Owner-authorized C2 cleanup correction" --amend-mission C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C2-AMENDMENT-1-2026-11-25.md

python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --reason "Owner authorizes one additional C2 review, total cap three" --max-rounds 3
```

Amendment capture may leave the enrollment blocked at its previous cap until the separate cap-change command. Read `status --instructions` and every returned instruction part. Record the actual amendment revision and digest.

Do not edit controller state, reset counters, create a replacement enrollment, or change global workflow policy.

## 3. Repair the final cleanup contract coherently

Keep the witness read-only. Its caller owns reconciliation and destruction.

**R2-F1 — late observations after failed early unsubscribe:**

Keep each relevant witness’s observations available through final cleanup, including when early unsubscribe throws and delivery remains active. Reconcile arrivals received after the earlier reconciliation before taking final settlement. Adopt positively correlated units and request destruction through the existing per-reference, one-request guard.

Retain the original unsubscribe failure. Refusing the next probe does not substitute for accounting for units the current witness already observed. Do not clear observations, suppress delivery, or discard a failure to obtain a clean result.

**R2-F2 — uncertainty resolved by positive attribution:**

Compute residual unknowns by unit reference after accounting for positive correlation and explicitly identified foreign controls. If the same reference was first unattributed and later positively matched before release, it is no longer an unknown owner.

Keep fixture-teardown ownership separate from cast attribution. A unit registered only for fixture teardown remains unattributed to the watched cast. Unknown cast ownership must not itself authorize destruction. Preserve genuine earlier cleanup failures.

**R1-F7 — exercise the actual final sequence:**

Use one small final-release sequence shared by `GhaeleLifetimeProbe` and the domain fixture, or execute the production finalizer directly with controlled native boundaries. The test must not construct its own parallel final action array.

After iterator unwinding and the existing held-resource release, the production sequence must:

1. Reconcile the latest observations from all relevant witnesses.
2. Request remaining independently owned disposable units.
3. Take one final terminal snapshot and use that same snapshot for the evidence row and cumulative cleanup result.
4. Attempt independent cleanup even when another action fails.
5. Return the cumulative outcome to the real `ProbeSlot`, preserving the original timeout or probe error.

A request does not prove destruction. Pending, unreadable or unaccounted units remain named residuals and prevent clean admission. Do not restore the global destroyer tick or force unrelated engine work during abandonment.

Reuse existing helpers and project conventions. Add no library or general resource framework. Do not change gameplay, presentation, assets or C1’s accepted evaluator.

## 4. Focused behavioral evidence

Use the real `ProbeSlot`, caller and shared production finalizer. Control only the native boundary operations needed for deterministic failure cases.

Cover these cases together before formal review:

**A. Failed early unsubscribe with later delivery:**

- Early unsubscribe throws before detaching; the earlier reconciliation has already run.
- Deliver an additional positively correlated unit and an unknown unit before slot termination.
- Establish one destruction request and a terminal result or named residual for the correlated unit.
- Establish a named uncertainty failure for the unknown, retention of the unsubscribe error, foreign survival without destruction requests, and refusal of the next `Begin`.

**B. Unattributed, then positively correlated, same reference:**

- Deliver both observations for the same object before release.
- Establish one request, successful settlement, no residual unknown and clean admission when no independent failure exists.
- Keep the registered-but-unattributed refusal case and the untouched foreign control.

**C. Abandonment before ordinary settlement:**

- Call the actual `ProbeSlot` abandonment path.
- Establish that the production final row and cleanup outcome contain the same unresolved unit.
- Preserve the original timeout, run independent cleanup and refuse the next `Begin`.
- Include a terminal reader that changes on a subsequent read so the test demonstrates that evidence and verdict consume one snapshot.

These are behavioral gates. Source-text presence checks and copied test cleanup arrays are insufficient. No mutation-testing framework or separate broken commit is required.

Run the applicable controller-required focused gates and production compilation. Inspect the affected normal, early-release and abandonment callers together. Avoid unrelated suites and new native fault campaigns.

## 5. Native qualification and artifact identity

Rebuild the external harness only. Reuse candidate 3:

- Source: `9c39b1ff7f95c995ead0ecdfca2e679e62506825`.
- Package SHA-256: `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413`.
- Product DLL SHA-256: `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3`.

Run only the short `s7-ghaele-lifecycle` session on `owner-0.33.0` with the precisely identified rebuilt harness. Retain the pre-fallback reconciliation request assertion, exactly one request, terminal destruction, foreign controls alive and unrequested after settlement, and final cleanup outcome.

The failed-unsubscribe and enclosing-`ProbeSlot`-abandonment cases belong in the focused boundary-controlled tests. Do not manufacture an actual EventBus failure in the game solely to repeat them.

Keep `005206Z` tied to harness `bf5d74a1…63cd` from `44fd90a8d1`. Its PASS covers the earlier normal path; the new run qualifies the new harness. Neither run executes C1’s changed evaluator on the old product package.

Preserve all failed attempts and restoration receipts. Continue game-window coordination and use only the disposable installation.

## 6. Records and budget

Correct the missing R2-F1 in the status and finding map. Verify the existing R2-F3 records correction against both C2 session receipts; no native run is needed for that wording.

Shared recovery validation already spent: **9m40.5s of 45m**.  
Remaining: **35m19.5s**.  
New product candidates: **0 of the existing maximum of 2**.

Keep this one cumulative ledger across C1–C3, including failed and refused attempts and wrapper time. Keep the earlier parent `65m42s` separate. No allowance is reset or extended.

Before expensive validation, account for the remaining C3 combined build and affected native qualification. Model review and implementation time retain their existing treatment. Restoration must always finish.

## 7. Handoff and autonomous continuation

Use the one additional C2 review after the coherent correction and focused evidence are complete. Report each blocking finding normally; do not relabel unresolved defects as optional or claim acceptance from this amendment.

If C2 receives `ready_for_owner_review`, finalize it and confirm `status --remote` reports `AWAITING_OWNER_REVIEW` with `handoff_current=true`. Preserve reviewed H and archive A separately, then freeze C2.

Proceed directly to the already-authorized C3 from C2’s stable archive, under C3’s original mission, two-review cap, shared time allowance and product-candidate ceiling. No extra owner pause is required between a valid C2 handoff and C3. C3 still owns effects isolation and live-versus-persisted evaluator agreement on the same identified combined run.

If C2 reaches another mandatory stop or cannot satisfy its scope within the allowance, preserve the stop and report the exact remaining blocker. Do not increase limits or create another enrollment automatically.

Return the reviewed stack, identities, finding map, evidence scopes, spend and restoration receipts. Keep visual and listening acceptance pending. Parent resumption, integration, merge, release, tags and human-installation changes remain outside this authorization.
----- END OWNER AMENDMENT 1 -----

----- BEGIN OWNER AMENDMENT 2 (captured 2026-11-26T03:29:00Z; message sha256 51ab2989ed42cdc1d67b1518c1e16e4fae9ac7fe604b5022ffcc79fd1d541e2e) -----
SPRINT 7 RECOVERY — C2 AMENDMENT 2: CAST RELEASE AND SHARED ATTRIBUTION  
Owner instruction, 2026-11-25

I authorize continued work on the existing C2 enrollment for the corrections below, with up to two additional formal reviews: increase its total cap from three to five. Target completion in round 4; round 5 is available for a concrete correction identified by that review. Do not use another round after a current handoff is achieved.

The shared validation allowance remains 45 minutes across C1–C3. No additional product candidate is authorized for C2. This amendment does not waive either blocking finding or reopen the accepted Sprint 7 presentation work.

## 1. Preserve the stack and authority

C2 is draft PR #222, branch `codex/s7-recovery-c2-ghaele-late-units`, targeting `codex/s7-recovery-c1-cue-evaluator`.

- Enrollment: `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08`.
- Last reviewed H: `89c7229e208a498e71caa91e7e4225e867c0d5d8`.
- Reported branch head: `0446ec2c02458783194d2c316e7cc96f656e01cc`.
- Round-3 review: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/222#issuecomment-7181367940.

C1 remains owner-accepted for implementation/domain scope and frozen at reviewed H `5a56d480a2827e0ddd3918e30d32a07c7554e112` and archive A `2c67a049ee33e0b29621fd29a8b25608ed31e956`. Its native evaluator check remains assigned to C3.

Parent #219 stays frozen at `0542b0592a668aad0c62c66d5f2dc862460818fe`, with its enrollment BLOCKED at 7/7. C3 may start only after C2 has a current scoped handoff.

Verify remote identities and controller state. Preserve concurrent work if anything moved. Keep every prior finding, verdict, session, artifact identity and amendment. Do not reopen verified fixes without evidence of a regression.

## 2. Apply this amendment through the controller

Save this entire instruction verbatim outside the worktree:

`C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C2-AMENDMENT-2-2026-11-25.md`

Use the installed `C:/Dev/agent-review/agent_review.py` and its current instructions. Check `status --remote`, then apply the amendment and cap change as separate commands:

```text
python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --reason "Owner authorizes C2 cast-release and shared-attribution correction" --amend-mission C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C2-AMENDMENT-2-2026-11-25.md

python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --reason "Owner authorizes up to two additional C2 reviews, total cap five" --max-rounds 5
```

Amendment capture may retain the old cap stop until the separate cap-change command. Read and display `status --instructions` and every returned instruction part in the active foreground session; saving them to a file alone is insufficient. Record the actual amendment revision and digest.

Do not edit controller state, reset counters, replace the enrollment, or change global workflow policy. Existing mandatory stops remain effective.

## 3. R3-F1: release only the exact owned summon

Remove the unconditional `Game.Instance.EntityCreator.Tick()` from the held “unregistered summons” release in `CastOwned`. It currently runs even before the callback checks whether this cast has a summon.

Use the retained `handle.Summoned` reference, the existing registered-reference exclusion and the per-reference destruction-request guard. Preserve the command and ability-loan cleanup.

Do not substitute a global destroyer tick, queue drain, frame pump or equivalent work affecting unrelated entities. A missing owned reference does not authorize running the creation queue to discover one.

An owned summon may remain pending or unreadable after its request. Keep that reference in settlement accounting, record a named residual, and refuse clean admission when appropriate. Do not treat absence from `State.Units` as proof of destruction.

The creator tick in ordinary forward cast progression is a separate call. This finding authorizes correcting the cleanup path and its directly affected callers; it does not authorize deleting every creator tick or requalifying unrelated creation code.

The focused regression must reach the held cast-release operation used by production through `ProbeSlot.Abandon`. A test-only parallel release sequence is insufficient. If needed, share the small actual held operation between production and the fixture using the existing harness structures.

Use a participating controlled creation-queue boundary: invoking it would register the foreign queued entity. Assert that synchronous abandonment leaves that foreign entry pending and unrequested, while the exact owned summon receives one request and a terminal result or named residual. Preserve the original timeout and independent releases.

Include the local no-owned-summon control, since the current tick is unconditional. Preserve a conclusive-destruction control and a pending/unreadable control. The requirement concerns what cleanup itself does; ordinary later engine frames may legitimately process the foreign queue.

## 4. R3-F2: classify ownership across the probe before recording uncertainty

Keep witnesses read-only and retain their observation history. Give reconciliation access to the positive cast matches and explicit control identities from all relevant witnesses in this probe, compared by object reference.

Use that shared attribution information before any applicable `RecordUncertain` call can commit a false failure, including early release, held cleanup during iterator unwind, and final reconciliation. A union introduced only after held callbacks have already recorded failures is insufficient.

Resolve each witness’s unattributed references using the positive matches and explicit controls already available across the probe. Request positively correlated units through the existing one-request owner. Keep genuinely unknown references named.

Keep cast attribution distinct from fixture teardown registration. `units.Owns`, matching blueprint, shared caster or similar names are not substitutes for positive cast attribution or an explicit control identity.

Preserve genuine earlier unsubscribe, destruction and isolation failures. Do not delete errors, clear witness histories or suppress delivery to make the report clean. Keep the shared final terminal snapshot used by both the evidence row and `ProbeSlot`’s cumulative cleanup outcome.

Use a small probe-local shared view within the existing implementation. Add no process-global registry, library or general ownership framework.

Focused evidence must use the actual shared caller/finalizer and `ProbeSlot` with two witnesses:

- Witness A remains subscribed after its early unsubscribe fails.
- A later summon is unattributed to A and positively matched to B.
- A foreign control observed by A is explicitly identified by B.
- A separate reference remains genuinely unknown.

Establish one request for the correlated summon, no false unknown for either identified reference, preservation of the foreign control during reconciliation, a named failure for the genuine unknown, retention of the unsubscribe error, and refusal of the next `Begin`.

Exercise held unwind as well as the final reconciliation boundary, so a final-only patch cannot pass. The result must not depend on which witness is reconciled first. Keep the fixture-registration-only unknown control and the pending-settlement behavior.

Distinguish preservation during witness reconciliation from later authorized fixture teardown; a control owned independently by the fixture can be removed during that teardown.

## 5. R3-F3 and focused verification

Make the local test-helper correction while editing the tests: require an actual `JArray` at each asserted settlement field. An explicit empty array is clean evidence; a missing or incorrectly shaped field must not become an empty list.

R3-F3 remains nonblocking test polish. It does not independently justify another native run or block an otherwise satisfactory scoped handoff.

Before packaging the harness, inspect the whole affected path once: acquisition, exact cast release, overlapping witnesses, early unsubscribe failure, iterator unwind, final settlement and next-probe admission. Preserve the verified fixes from round 3.

Run the two applicable focused gates, `sprint7-visual` and `loader-compat-probe`, with the required production compilation. Retain meaningful existing cases. No broad suite, arbitrary failure matrix or new cleanup framework is requested.

## 6. Native qualification and evidence identity

Build the external harness only. Reuse candidate 3:

- Source: `9c39b1ff7f95c995ead0ecdfca2e679e62506825`.
- Package SHA-256: `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413`.
- Product DLL SHA-256: `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3`.

Plan one short `s7-ghaele-lifecycle` qualification on `owner-0.33.0` after the coherent correction. Preserve the existing assertions for reconciliation before fallback, one request, terminal destruction, foreign controls after settlement, final reconciliations and cleanup.

The queued-foreign-creation and failed-unsubscribe controls belong in the focused tests with controlled native boundaries. Do not deliberately damage the game’s event bus or manipulate unrelated live queues solely to repeat them natively.

Keep `20261126T024816Z` tied to source `56e359f2cc536f6f0434507cf465760df85c2e44` and harness `4180621b2ade02140bb461aff586ba041d9ae318c60764828b36575aaeafdb61`. Preserve its PASS at its actual scope. Do not relabel it as execution of the new correction.

A later records-only or domain-test-only correction does not by itself require another native session when the exercised harness and session bytes remain unchanged. Requalify affected behavior if executable harness code changes.

Keep restoration, registry and owner-file checks, disposable-installation isolation and game-window coordination. Preserve all failed or refused attempts.

## 7. Budget and continuation

Recovery validation spent: **12m06.5s of 45m**.  
Remaining: **32m53.5s**.  
New recovery product candidates: **0 of the existing maximum of 2**.

Do not reset or extend these allowances. Keep the parent’s earlier `65m42s` separate. Count validation-command wrapper time and failed/refused validation attempts under the existing policy. Controller refusals that launch no validation are documented separately.

C3 currently needs roughly 25 minutes for its combined build, focused gates and effects qualification. Use that estimate when planning C2’s short run and any necessary rerun; update it from actual work. It is an estimate, not a guarantee or a new separate allowance.

Review and implementation time retain their existing treatment. The extra review authority does not authorize optional game sessions or redundant candidate builds. Finish restoration even if a limit is reached.

After C2 receives `ready_for_owner_review`, finalize and confirm `status --remote` reports `AWAITING_OWNER_REVIEW` with `handoff_current=true`. Preserve H and A separately and freeze C2.

Then proceed directly to C3 from C2’s stable archive under its original mission, two-review cap, shared validation allowance and product-candidate limit. No extra owner pause is required. C3 still owns effects isolation and agreement between the revised live evaluator and the persisted row from the same identified run.

If another mandatory stop occurs or the required remaining validation cannot fit, preserve that state and report the concrete blocker. Do not increase limits or create a substitute enrollment automatically.

Return the reviewed stack, exact identities, complete finding map, evidence scopes, cumulative spend and restoration receipts. Visual/listening acceptance remains pending. Nothing here authorizes advancing or resuming parent #219, integration, merge, release, tags or human-installation changes.
----- END OWNER AMENDMENT 2 -----
````

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
