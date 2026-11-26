# Mission: Sprint 7 recovery C2 — Ghaele late-unit ownership (R5-F1)

Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules
Branch: `codex/s7-recovery-c2-ghaele-late-units`, branched from and targeting C1's branch
`codex/s7-recovery-c1-cue-evaluator` at C1's archive commit A (`2c67a049ee33e0b29621fd29a8b25608ed31e956`), which is frozen while C2 is under review.
Authority: the owner instruction "SPRINT 7 RECOVERY — THREE SCOPED REPAIR PRS" (2026-11-25), reproduced verbatim below and
stored at `<lab>/owner-guidance/S7-RECOVERY-THREE-REPAIR-PRS-2026-11-25.md`; C1 amendments 1 and 2 (2026-11-25,
`<lab>/owner-guidance/S7-RECOVERY-C1-AMENDMENT-1-2026-11-25.md` and `...-AMENDMENT-2-2026-11-25.md`) direct proceeding to
C2 after C1's handoff and keep C2's two-review limit and the shared allowance. C1 reached AWAITING_OWNER_REVIEW with
reviewed H `5a56d480a2827e0ddd3918e30d32a07c7554e112` and archive A `2c67a049ee33e0b29621fd29a8b25608ed31e956`.
Parent: PR #219 frozen at `0542b0592a668aad0c62c66d5f2dc862460818fe`; enrollment BLOCKED at 7/7. Round-7 parent review:
https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219#issuecomment-7147252673
Recovery manifest: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md`.

## Scope of this child

- Inherited finding: **R5-F1** of the parent (an unaccounted late Ghaele must fail slot isolation), as section 4 of the
  owner instruction defines it. Preserve the verified post-disposal delivery and visibility fixes.
- Acceptance boundary: the external harness correction and its qualification. The witness stays read-only; its caller
  reconciles every positively correlated unit with the existing destruction owner on every release path, observes
  settlement, and records still-present, unreadable or unknown-ownership units as named cleanup failures in the outcome
  ProbeSlot reads. Behavioral regressions run through the actual ProbeSlot / caller / cleanup / admission path.
- Native: one short lifecycle session (`s7-ghaele-lifecycle`, lifecycle step only) on the existing candidate-3 package
  (`49f8b88c…6413`, DLL `5969f596…e2c3`; the product contract the harness uses is unchanged) with an exactly identified
  rebuilt harness. No product package, no full Ghaele expiry, combat or visual qualification.
- Out of scope: the parent's other findings (C1, C3), gameplay, presentation, balance, assets and release.
- Budget: this child's validation commands count against the one shared 45-minute recovery allowance; it builds no
  product candidate.
- Review: one foreground enrollment, `--plan single`, `--target codex/s7-recovery-c1-cue-evaluator`, `--max-rounds 2`.
  A further mandatory stop is preserved and reported, never recreated.

---

SPRINT 7 RECOVERY — THREE SCOPED REPAIR PRS

I authorize a change in how the remaining Sprint 7 work is delivered. Keep the existing broad PR and enrollment stopped, and complete the remaining repairs through the three small, dependent PRs below. Work autonomously through their scoped review loops. Return the resulting reviewable stack and integration handoff.

This instruction supersedes the original requirement for one Sprint 7 branch, enrollment and PR only for these named repairs. It does not erase any finding, reset the original enrollment, authorize another broad Sprint 7 enrollment, or grant merge/release authority.

## 1. Freeze the parent and preserve evidence

Parent: PR #219, `codex/s7-visual-audio-remediation`.  
Enrollment: `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`.  
Reported frozen head: `0542b0592a668aad0c62c66d5f2dc862460818fe`.  
Round-7 reviewed H: `3e2baea4b486f3f416895127e4c67797968a8f28`.  
Candidate 3 source: `9c39b1ff7f95c995ead0ecdfca2e679e62506825`.  
Package: `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413`.  
DLL: `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3`.  
Qualifying round-7 harness: `ee7742a493eb3879cffa30c69879592a51da60a3b7205a902832d8e4395ea373`.

Read current repository/workflow instructions and verify these identities remotely. If another actor has changed the intended base, reconcile that fact without overwriting their work or inventing equivalence.

Do not resume, cancel, close, retarget, merge or rewrite #219. Leave its enrollment BLOCKED at 7/7. Keep all original findings, amendments, sessions, FAILs, budgets and package identities. No archive A exists for that parent.

Create a concise recovery manifest mapping every remaining parent finding to exactly one repair PR, its scope, evidence, dependencies and remaining integration gate. Link the actual round-7 review. Parent findings remain historically open until explicitly reconciled; a new child’s PASS is not a retrospective parent PASS.

Keep accepted R4-F5, the bounded Ghaele scope, controller-admission disposition, approved wording, released Sickened mechanics, and all other accepted story evidence. Visual and listening acceptance remain pending. No gameplay, presentation, balance, asset or release expansion is authorized.

## 2. Use three narrow dependent PRs

Use one dedicated `codex/*` branch/worktree, immutable scoped mission and foreground controller enrollment per repair. Each mission must include this authority and identify its inherited finding IDs and acceptance boundary. Use the installed controller’s supported `--target` option so each PR shows only its incremental change.

Sequence:

- C1 targets the frozen #219 branch.
- After C1 reaches its scoped handoff and its archive commit is stable, C2 branches from and targets C1.
- After C2 reaches its scoped handoff, C3 branches from and targets C2.

Freeze prerequisite branches while descendants are under review. Do not force-push or rewrite earlier handoffs. These PRs depend on #219; do not describe them as independently mergeable to master.

Use the existing developer and independent reviewer roles. Enroll each with `--plan single`, `--mode foreground`, its exact `--target`, and `--max-rounds 2`. Read current help/instructions and use actual returned enrollment identities. The two-review limit is per named correction PR; no substitute enrollments or extra repair PRs may be created to evade a stop.

Review each PR against its own declared scope and the affected callers. Do not re-open the seven-story implementation campaign. New relevant defects introduced by a correction remain in that correction’s scope.

## 3. C1 — cue observation/evaluation contract

Assigned findings: **R7-F1 and R7-F6**.

Deliver a correct, strict evaluator over the actual collector’s observations, with focused behavioral tests. This child has an explicitly limited code/domain acceptance boundary: native execution of its changed compiled evaluator remains pending C3’s named combined qualification. State that prominently in its PR, report and handoff.

Required behavior:

- Separate absent/unreadable required properties from explicitly recorded null values.
- Treat explicit null equivalently in the collector-shaped in-memory `JObject` and the serialized/reparsed form, including String-typed `JValue` instances whose underlying value is null.
- Preserve real stop requests, completion callbacks and native playback queries as separate evidence.
- Bind selected survivor and other IDs to the corresponding settled and terminal records. Require valid, distinct IDs and reject contradictory identities.
- Consume the collector’s explicit survivor stop count through the window. An omitted required count is unknown, not zero.
- Preserve the terminal playback rules already accepted.
- Use collector-specific interpretation for older rows that did not emit newer fields; do not invent missing evidence or revoke an already accepted bounded historical disposition.

Use the immutable `224949Z` row and a collector-shaped live counterpart. Both should characterize its observed behavior after the repair. Missing required properties, actual stop/end observations, substituted cue IDs, and a nonzero survivor stop count must prevent characterization.

Replace the active behavioral expectation that reproduces the incorrect live verdict with the corrected expectation. Preserve the original recorded FAIL as history. Do not obtain equality by serializing the live object merely to hide the representation bug.

No product package or game run is needed for this child’s explicitly code/domain handoff. C3 must qualify the delivered evaluator in the game before the combined correction is called natively qualified.

## 4. C2 — Ghaele late-unit ownership

Assigned finding: **R5-F1**. Preserve the verified post-disposal delivery and visibility fixes.

The witness remains read-only. Its caller must reconcile every positively correlated result with the existing destruction owner, including an additional result observed after cast-owner disposal.

Request destruction once per owned unit and establish settlement. A returned request, or a request already marked attempted, does not establish disappearance. Any known alive, unreadable or unaccounted owned unit must enter the cumulative cleanup failure consumed by `ProbeSlot`. Reconcile during abandonment/final release as well.

Keep foreign controls untouched. Do not guess ownership of an unattributed unit; uncertainty must prevent clean isolation.

Test through the actual `ProbeSlot`/caller/cleanup/admission path:

- Destruction returns but the late unit remains present.
- An additional positively correlated unit arrives.
- Each owned unit receives one request.
- Independent cleanup continues.
- Foreign control survives.
- Unresolved ownership is reported by identity and the next probe does not start.

This is an external-harness change. Reuse candidate 3 where compatible with the unchanged product contract, with an exactly identified rebuilt harness. Run only the short affected lifecycle check; do not repeat full Ghaele expiry, combat or visual qualification. A fourth mod package is not necessary for this child alone.

## 5. C3 — complete effects ownership and combined native qualification

Assigned findings: **R4-F1 and R7-F2 through R7-F5**. This child also owns the combined native qualification of C1 and the final correction stack.

Keep this ownership work together. Repair the existing complete acquisition-to-admission contract rather than adding separate local booleans for each review example. Reuse existing structures; no new library or general-purpose resource framework.

Required invariants:

- Register an acquisition attempt before it can partially succeed and throw.
- Preserve the exact current execution before interrupting a command can clear its `ExecutionProcess` field.
- Attempt each independent command/resource release even when another throws.
- Distinguish a destruction request from its result, and preserve a named failed request without issuing it twice.
- Publish complete child outcomes through ordinary completion, exceptions and iterator disposal.
- Unexpected setup, execution and release exceptions must reach the affected runner’s sticky isolation failure, even if the visible resource later disappears.
- Unknown or unreadable owned hooks, processes or units cannot count as restored.
- The guard that admits later cells and the final phase verdict must consume the same complete result.

Cover each remaining finding:

- **R4-F1:** a deliberate injected fault plus an independent command-release failure. Later repair of the intended residual must not erase or exempt the independent failure.
- **R7-F2:** ordinary Dust disposal after the execution begins but before effect delivery. Retain the process before interruption; one failing interruption must not skip another owned command.
- **R7-F3:** partial callback/projectile hook installation followed by failed or unreadable rollback. Constructor failure and a null returned object cannot imply zero acquisition. Preserve setup and rollback errors and an explicit hook readback.
- **R7-F4:** either direct cue-source retirement throws. Preserve one request, the original error, independent cleanup and failed phase isolation.
- **R7-F5:** abandonment of the Dust recovery driver while its execution wait is pending. Attempt repair of the exact injected condition on every exit, preserving foreign effects, the original child failure and any failed repair.

Simplification is authorized: destructive failure controls may end their scenario after demonstrating failed isolation and refusal of later work, instead of recovering into ordinary acceptance cells in the same fixture. The outer owner must still attempt cleanup of its own intervention and complete transactional restoration. Process exit is not a substitute for that required cleanup attempt.

Do not add a separate game launch for every theoretical combination. Settle pure failure aggregation, representation, identity and caller-admission behavior through focused tests of the real orchestration with controlled boundaries. Use native checks for the engine-owned process/hook boundaries and final delivered collector/evaluator.

Before packaging, inspect and test the whole affected path once: acquisition, partial acquisition, early release, normal release, abandonment, recovery if retained, next-cell admission and final restoration.

Then build the combined candidate and qualify the affected native paths. Use controlled, declared scheduling of valid producer events to reach the two-source cue observation, retaining real playing IDs and production-handler ownership. Do not rely on repeated uncoordinated attacks eventually overlapping. Keep constructed setup distinct from ordinary-play evidence.

C3 must show that the final live evaluator and its persisted-row interpretation agree on the same identified run. Historical failed verdicts remain unchanged. It must also establish effects isolation through the actual enclosing runner and retain the independently qualified Ghaele correction at its own package/harness identity.

## 6. Records and evidence

R6-F5’s requested corrections appear present at the frozen head. Confirm the record and planning row from existing receipts and record the disposition in the recovery manifest; no build or game run for that correction.

Preserve:

- `65m42s` spent in rounds 6–7.
- 118 selected-test executions for round 7.
- `40.1s` gate-wrapper wall time versus `19.1s` inside receipts.
- Harness suffix `a373`.
- All original native FAILs and accepted interpretations.
- The accepted R4-F5 disposition and unknown historical `164749Z` cause.

Keep source, package/DLL, harness, collector, reader and fixture identities distinct. A child archive commit is not the product source that ran. A passing regression that characterizes an existing defect is not evidence that the defect was repaired.

Re-use the existing model/desktop/Trumpet evidence and private listening packet. Do not collect new taste evidence, broaden loader/provider coverage, seek new fixtures or re-investigate the October incident.

## 7. One shared recovery budget

This mission authorizes up to **45 minutes of new validation-command wall time across all three repair PRs**, including failed attempts, builds, focused checks and game sessions. It is one aggregate allowance, not 45 minutes per PR. The old unused `9m18s` is not added to it. Keep every earlier expenditure visible separately and in the cumulative ledger.

Target **one combined new product candidate** after the code corrections are settled. At most **two new product candidates** are authorized across this entire recovery, allowing one contingency for a concrete failure found during affected native qualification. No other package builds are authorized. Reuse unchanged artifacts.

Model/source review and implementation time are outside the validation-command clock, as before. Safe in-progress restoration must always complete. Keep global workflow limits and `VALIDATION-POLICY.md` unchanged.

Use relevant named Focused gates and production compilation; honor any project-required gates. Avoid unrelated full suites. On the final combined source, run the required focused gates needed to preserve the Sprint 7 handoff and its affected neighbors.

Before each expensive build/run, update the shared remaining allowance and estimate the necessary remaining work. A refused, failed or timed-out attempt still counts. Do not multiply allowances by opening another enrollment.

If any child exhausts its review cap, hits a mandatory stop, or cannot establish its declared acceptance, preserve that state and report the concrete blocker. Do not recreate it as a fresh enrollment or raise limits automatically.

## 8. Scoped handoffs and final delivery

For each child, follow the supported review/finalize/`status --remote` sequence. Confirm `AWAITING_OWNER_REVIEW` and `handoff_current=true` for its own declared scope before starting a dependent child. Preserve reviewed H and archive A separately.

C1’s handoff explicitly covers implementation/domain behavior; C3 owns its deferred native integration check. C2 owns its external-harness qualification. C3’s handoff covers effects ownership and the combined native correction. A code-scope handoff is not a claim of complete native qualification. Record fulfillment of C1’s deferred check in C3 and the recovery manifest; do not amend the frozen C1 branch and invalidate its descendants.

Return:

- The three PRs and their dependency order.
- Each enrollment, target/base, reviewed H, archive A and exact handoff scope.
- A complete original-finding-to-repair/evidence map.
- Combined candidate and harness identities.
- The aggregate validation spend, original failures and restoration receipts.
- Any remaining owner visual/listening choices.
- A concrete integration plan for the reviewed corrections and #219.

Do not merge these PRs into their prerequisites, advance #219’s frozen branch, resume its 7/7 enrollment, create a replacement whole-sprint enrollment, or declare the parent ready. Final integration and any parent review resumption remain a separate owner decision over this concrete reviewed stack. Nothing here authorizes merge, release, tag or human-installation changes.
