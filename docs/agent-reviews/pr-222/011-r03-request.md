<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 key=r03-request part=1/1 sha256=ad89b8b702287e4247befedd1b320c84e210b18b7305f1517d4eeb661f1d3941 -->
### Claude Opus — review request — round 3 — candidate `89c7229e20`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r03-ec7375` |
| Checkpoint | `final` (final) |
| Candidate H | `89c7229e208a498e71caa91e7e4225e867c0d5d8` |
| Target B (`codex/s7-recovery-c1-cue-evaluator`) | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Merge base M | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Mission | `s7-recovery-c2-mission` sha256 `f54c407bac` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 1, sha256 `d62882e3355874e552829475113faf7afcc304a36c387d04ca29850cd08d4626` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 3 of 3 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C2, review 3 of 3 under OWNER C2 AMENDMENT 1 (late-unit cleanup; revision 1), on H=89c7229e20, PR #222 targeting C1's branch at its archive A 2c67a049ee (C1 accepted by the owner for its implementation/domain scope; recorded in the manifest). One coherent correction in the external harness: witness observations stay available to final cleanup and are reconciled again there (R2-F1); unknown ownership is computed by reference after positive correlation and known controls (R2-F2); one shared final release sequence is used by GhaeleLifetimeProbe and its domain fixture (R1-F7); records corrected (R2-F3, and the omitted R2-F1 in the earlier handoff). The read-only witness, one destruction request per reference, the conclusive Destroyed-mark settlement, the single terminal snapshot and the pre-fallback verdict condition are preserved; no global destroyer tick. Evidence: amendment cases A, B and C as behavioral domain gates through the real ProbeSlot and the production finalizer; native short lifecycle session 20261126T024816Z PASS on the unchanged candidate-3 package with harness 4180621b2ade, restoration confirmed. No product package (0 of 2). Recovery validation 12 min 6.5 s of 45. C3 retains the combined product qualification.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter sprint7-visual | yes | pass | 4.7s | 23 tests on H, production compilation; receipt 2.8s, worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter loader-compat-probe | yes | pass | 4.0s | 5 tests on H; receipt 2.0s. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, amendment-1 source) | no | pass | 4.0s | 23 tests; test binary rebuilt after the test edit (timestamps checked). |
| Build-LoaderCompatHarness.ps1 (amendment-1 source 56e359f2cc) | no | pass | 1.3s | Harness 4180621b2ade02140bb461aff586ba041d9ae318c60764828b36575aaeafdb61. tools/LoaderCompatHarness and tests are unchanged from 56e359f2cc to H (the later commit is records only). Two pre-existing CS0472 warnings in BarbedCrabProbes.cs. |
| Invoke-LoaderCompatSession.ps1 s7-ghaele-lifecycle.json on owner-0.33.0 with the existing candidate-3 package and harness 4180621b | no | pass | 2m12s | Session 20261126T024816Z (window 02:48Z-02:50Z agreed with the other Kingmaker session and closed by message): candidate 3 source 9c39b1ff7f, package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413, DLL 5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3; git 56e359f2cc; staged harness 4180621b2ade02140bb461aff586ba041d9ae318c60764828b36575aaeafdb61. Lifecycle step only: late-case verdict late-owned-summon, lateUnitDestroyRequestedByReconciliation true, exactly one request for late unit cfec1089-f7c4-4f23-8a52-0e601a4fcb46 in the cumulative cleanupAttempted, lateUnitDestroyed true; foreign controls alive and unrequested after settlement (late and spawned cases), foreignPreserved true in the pending case; the three witnesses' "final reconciliation" actions and "disposable units settled" ran in the slot's final release; unsettledAfterWitness, unsettledAtEnd and unsettledAtFinalRelease empty; cleanupFailures empty; isolationRestored true; transaction completed, owner files verified, registry restored. 005206Z stays tied to harness bf5d74a1 from 44fd90a8d1. Neither run executes C1's changed evaluator (old product package). The failed-unsubscribe and slot-abandonment cases are domain-only by design (no EventBus fault manufactured in game). |
| Shared recovery ledger | no | pass | 12m6.5s | Cumulative recovery validation after these gates: 12 min 6.5 s of 45, remaining 32 min 53.5 s (the committed ledger stops at 11 min 57.8 s, before these controller gates: 8.7s batch wall, receipts 4.8s). C2 round 3 spent 2 min 26.0 s; no failed attempt in C2. Product candidates 0 of 2. Reserve: C3 still needs one combined Build-Local candidate and the affected native effects qualification (estimated about 18 minutes plus build and gates). |

**Evidence references:**
- prepared under owner amendments revision 1 sha256 d62882e3355874e552829475113faf7afcc304a36c387d04ca29850cd08d4626
- docs/missions/sprint7/recovery/S7-RECOVERY-C2-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- docs/evidence/sprint7/recovery-c2/MANIFEST.json
- docs/evidence/sprint7/recovery-c2/20261126T024816Z-owner-0.33.0-s7-ghaele-lifecycle-01-ghaele.json
- tools/LoaderCompatHarness/TrumpetCueVerdict.cs
- tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs
- tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs

**Prior findings to assess:** `R1-F7`, `R2-F1`, `R2-F2`, `R2-F3`

**Owner amendments to the mission — revision 1, captured-source sha256 `d62882e3355874e552829475113faf7afcc304a36c387d04ca29850cd08d4626`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

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
````

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
