<!-- agent-review:v1 enrollment=kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 key=r08-request part=1/2 sha256=d03a1db395418ab8fcfde0613ad900be3ad085c7dfdbf95b39c9e742f7888553 -->
_Part 1 of 2._

### Claude Opus — review request — round 8 — candidate `b91eabcb3a`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1-r08-0e26df` |
| Checkpoint | `final` (final) |
| Candidate H | `b91eabcb3a6d608a383bb6024a09a7a72575285b` |
| Target B (`master`) | `52755b617c79328a972ae5b74bc05784422e332d` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `sprint7-autonomous-mission` sha256 `c232f135ef` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 5, sha256 `9fbfc53b89b5f9110f847f24ac19e3023c623f97a6f4757bbadd296699c617c5` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 8 of 9 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 parent formal review 8 of 9 under OWNER AMENDMENT 5 (revision 5, sha256 9fbfc53b...17c5, message sha256 8203ea56...8350; admitted 2026-11-28T09:07:08Z by resume --amend-mission, which returned ACTIVE/develop; all nine effective-task parts displayed). PRE-REVIEW STOP (history): the first review-8 submission on 84d2636e01 (2026-11-28T03:07:27Z-04:25:11Z) stopped BLOCKED before any reviewer launched, because the controller's transcript guard (merge-base..head over docs/agent-reviews) found the 85 child transcript copies the fast-forward had brought in ('transcript tree change outside the controller's records: A docs/agent-reviews/pr-221/001-r01-request.md'). No round, finding or reviewer was recorded; reviews completed stayed 7 of 9. Its 77 min 44 s was controller pre-review wall time (slow Git on a fragmented shared object store), not model review and not validation. CORRECTION: submission H=b91eabcb3a6d608a383bb6024a09a7a72575285b is one bounded commit on 84d2636e01 that deletes exactly docs/agent-reviews/pr-221/ (25 files), pr-222/ (24) and pr-223/ (36) and updates only docs/GUNSLINGER-SPRINT7-RECORD.md and planning/GUNSLINGER-REMEDIATION-STATUS.md (dated pre-submission snapshot). pr-195/ (51 files), the recovery manifest docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md and all evidence are unchanged; no source, test, harness, session, asset, dependency, build script or controller configuration changed from 84d2636e01. PROTECTED-PATH DELTA: git diff --raw -z --no-renames --no-ext-diff <merge-base> H -- docs/agent-reviews is EMPTY (merge base 35763430ac against refreshed origin/master 52755b617c; nothing from master merged or rebased). CHILD ARCHIVES (immutable; still ancestors of H; child branches unchanged at them): C1 #221 https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/2c67a049ee33e0b29621fd29a8b25608ed31e956/docs/agent-reviews/pr-221 (reviewed H 5a56d480a2); C2 #222 https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/eab90a5ac82f661ecb903c5456d951a57a085eb9/docs/agent-reviews/pr-222 (reviewed H da811b123c); C3 #223 https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/3bfc570090b9772cac4b92e79983d31455510bca/docs/agent-reviews/pr-223 (reviewed H fa43536ccd). The record cites them by these links (section 7); none of the in-tree manifests link into the removed folders. The nine parent findings stay addressed by accepted successor work, pending parent reconciliation in this review (record section 7 matrix): R7-F1, R7-F6, R6-F5 by C1 (deferred native criterion fulfilled by 054228Z, repeated by 133020Z); R5-F1 by C2 (scoped Ghaele ownership, 033419Z on candidate 3 with harness d77754ba); R4-F1, R7-F2..R7-F5 by C3 (implementation, focused cases, candidate-6 qualification, eight reviews). Historical parent FAILs, the 7/7 stop and the pre-review stop stay recorded. Seven-story outcome table: record section 2, carried unchanged (seven completed mechanics cells are distinct from the seven parent stories; earlier presentation evidence stays on its original candidates; C2's Ghaele evidence stays on candidate 3). Candidate 6 reused without rebuild (code 360ce86f54, build source 962bf27694, package 6c71d928...d55a, DLL 6af754eb...1411; 20261127T133020Z PASS 43/43); C3 limits carried: constructed callback overlap, single native cancellation classification with a source-derived future guarantee, pending-command, uncertainty-recovery and cross-owner-transfer failure cases focused-only. VALIDATION (amendment 5 section 5): one fresh controller focused gate, -Filter sprint7-visual, on H: PASS, 25 tests, receipt 3.6s, outer 4.9s. Supporting amendment-4 receipts stay on 84d2636e01 only (sprint7-visual 25, summon-playtest-r6 11, native-profile 9, loader-compat-probe 5; 349.0s outer). Ledger: 71 min 31.0 s carried + 0 min 04.9 s = 71 min 35.9 s of 85 (13 min 24.1 s remaining); the C3 ledger is separate and unchanged. Coordination: Turn Undead peer session (kingmakergunslinger-49) asked for a git pause during an owner-authorized repack of the shared object store; this session paused git and controller commands until the peer reported the repack finished, then ran the gate. Visual (T2, T3, T6) and listening (T5, T7) choices stay with the owner; T1 and T4 dispositions and the R4-F5 interpretation are unchanged. Later master integration (v0.0.195 vs 0.0.197, #220 contract, merged-tree compile) remains later work.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint7-visual | yes | pass | 4.9s | The one fresh gate owner amendment 5 section 5 requires, run on submission H b91eabcb3a (2026-11-28T09:36:08Z to 2026-11-28T09:36:13Z UTC) with production compilation: filter 'sprint7-visual' selected 25 of 3213 registered tests, all passing; controller receipt 3.6s, worktree unchanged; full outer command 4.9s charged (wrapper and controller overhead included). H differs from 84d2636e01 only by deleting the 85 inherited child transcript copies and editing docs/GUNSLINGER-SPRINT7-RECORD.md and planning/GUNSLINGER-REMEDIATION-STATUS.md. |
| SUPPORTING, ON PREVIOUS HEAD 84d2636e01 ONLY: verify --gate focused -- -Filter sprint7-visual | no | pass | 99.3s | Amendment-4 receipt on 84d2636e0105f79dc7a6416334aa600ee5fbb8b5 (2026-11-28T03:00:36Z-03:06:25Z batch, 349.0s outer), not a run on H: 25 tests passed, receipt 58.8s. Retained as evidence on that exact head under amendment 5; cited for the unchanged-code rationale only (no compiled source, test, harness, session, asset, dependency, build script or controller configuration differs between 84d2636e01 and H). |
| SUPPORTING, ON PREVIOUS HEAD 84d2636e01 ONLY: verify --gate focused -- -Filter summon-playtest-r6 | no | pass | 80.5s | Amendment-4 receipt on 84d2636e0105f79dc7a6416334aa600ee5fbb8b5 (2026-11-28T03:00:36Z-03:06:25Z batch, 349.0s outer), not a run on H: 11 tests passed, receipt 40.1s. Retained as evidence on that exact head under amendment 5; cited for the unchanged-code rationale only (no compiled source, test, harness, session, asset, dependency, build script or controller configuration differs between 84d2636e01 and H). |
| SUPPORTING, ON PREVIOUS HEAD 84d2636e01 ONLY: verify --gate focused -- -Filter native-profile | no | pass | 86.3s | Amendment-4 receipt on 84d2636e0105f79dc7a6416334aa600ee5fbb8b5 (2026-11-28T03:00:36Z-03:06:25Z batch, 349.0s outer), not a run on H: 9 tests passed, receipt 46.1s. Retained as evidence on that exact head under amendment 5; cited for the unchanged-code rationale only (no compiled source, test, harness, session, asset, dependency, build script or controller configuration differs between 84d2636e01 and H). |
| SUPPORTING, ON PREVIOUS HEAD 84d2636e01 ONLY: verify --gate focused -- -Filter loader-compat-probe | no | pass | 81.8s | Amendment-4 receipt on 84d2636e0105f79dc7a6416334aa600ee5fbb8b5 (2026-11-28T03:00:36Z-03:06:25Z batch, 349.0s outer), not a run on H: 5 tests passed, receipt 40.9s. Retained as evidence on that exact head under amendment 5; cited for the unchanged-code rationale only (no compiled source, test, harness, session, asset, dependency, build script or controller configuration differs between 84d2636e01 and H). |
| eleven-filter replay, other fresh filter selections, broad suite, mutation campaign, package build, game session, capture, listening | no | not_run | 0s | Not authorized (owner amendments 4 and 5). Historical eleven-filter receipts (116 executions on b37faaf883, 118 on 3e2baea4b4) stay historical execution. Candidate 6 is reused without rebuild; its 20261127T133020Z qualification is earlier execution on its own bytes. |

**Evidence references:**
- prepared under owner amendments revision 5 sha256 9fbfc53b89b5f9110f847f24ac19e3023c623f97a6f4757bbadd296699c617c5
- owner-guidance S7-OWNER-AMENDMENT-5-2026-11-28.md (message sha256 8203ea565f0712851a281fff247c68880b7464f418c22b8b52eeccb9747a8350)
- owner-guidance S7-OWNER-AMENDMENT-4-2026-11-27.md (message sha256 4b9ddb05c71483df0ec95386c5f4a0fd582c6192ed7ef2a720211c7c62ef228a)
- previous submission 84d2636e0105f79dc7a6416334aa600ee5fbb8b5 (pre-review stop; four amendment-4 gate receipts on that head)
- integration input 3bfc570090b9772cac4b92e79983d31455510bca (C3 archive A)
- review 7 on H 3e2baea4b486f3f416895127e4c67797968a8f28 (changes_required at the 7/7 cap)
- docs/GUNSLINGER-SPRINT7-RECORD.md (status block, evidence-location policy, sections 2, 3, 5, 6, 7)
- planning/GUNSLINGER-REMEDIATION-STATUS.md (Sprint 7 current state)
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md (unchanged)
- https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/2c67a049ee33e0b29621fd29a8b25608ed31e956/docs/agent-reviews/pr-221
- https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/eab90a5ac82f661ecb903c5456d951a57a085eb9/docs/agent-reviews/pr-222
- https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/3bfc570090b9772cac4b92e79983d31455510bca/docs/agent-reviews/pr-223
- docs/evidence/sprint7/recovery-c3/MANIFEST.json and 20261127T133020Z-* extracts
- docs/evidence/sprint7/recovery-c2/
- docs/evidence/sprint7/r7/GATES-3e2baea4b4.json
- evidence/sprint7-review/INDEX.json
- docs/evidence/sprint7/audio-context-r4.json

**Prior findings to assess:** `R4-F1`, `R5-F1`, `R6-F5`, `R7-F1`, `R7-F2`, `R7-F3`, `R7-F4`, `R7-F5`, `R7-F6`

**Owner amendments to the mission — revision 5, captured-source sha256 `9fbfc53b89b5f9110f847f24ac19e3023c623f97a6f4757bbadd296699c617c5`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

````text
# Owner amendments to the mission

Cumulative record kept by the agent-review controller. Each owner message below was applied by the owner through `resume --amend-mission` and is preserved verbatim, in order, between its labeled marker lines. Later amendments supersede conflicting earlier requirements; requirements they do not change still apply.

----- BEGIN OWNER AMENDMENT 1 (captured 2026-11-24T14:58:19Z; message sha256 f4d26be0eb891b4c689527f2a3799c7baf7838d8eb0a3c0a1427db5d5ba5a7dd) -----
Continue the existing Sprint 7 mission on PR #219. I authorize resuming the same stopped enrollment and completing the corrections below autonomously, using the review budget already available. This is an amendment to the existing mission, not a new enrollment or a replacement of its history.

Enrollment: `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`  
Branch: `codex/s7-visual-audio-remediation`  
PR: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219  
Last reviewed source: `511160dc6ebc07d06c54bcbad3449b4d40a85b85`  
Reported current head: `311b6e821df64af8ff7013c6d46341011f0ce73d`  
Original base: `35763430acc9eefc306b861045526a444690bc77`  
Authoritative pending review: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219#issuecomment-7124407867

## 1. Owner decisions and amended acceptance

These decisions resolve the ambiguity that contributed to the stalled reviews.

- Preserve the released Dust breath’s Sickened behavior. The original mission’s word “blindness” was mistaken. Correct the operative requirement to preserve existing Sickened, damage, duration, save, enemy filter, cooldown and action budget. Do not introduce blindness. Keep the existing harmless ally saving throws documented; changing their logging or mechanics is outside this continuation.
- Retain “Target point within twice this creature’s speed.” as the approved wording for the exact owned Trample/Overrun actions. This wording decision does not claim that the controller window has been rendered.
- Keep Sweetener_Low_06_Quake and InspireCourage provisionally while completing technical corrections. Neither is acoustically accepted yet. Do not select a replacement merely to demonstrate activity.
- Technical audio readiness requires correct cue ownership, timing, retirement, provenance and a complete playable private packet for me. The independent text-based reviewer is not required to hear WAVs or invent subjective listening observations. Actual listening and final sound selection may remain pending at AWAITING_OWNER_REVIEW. This explicitly replaces any interpretation of the original mission that required an unsupported agent listening capability before that handoff.
- Prepare that private packet through the existing authorized lab, as specified below. Do not assume a private attachment channel exists just because a prior instruction referred to one. Do not publish game audio or change the review controller to create a channel.
- Keep the Ghaele conclusion bounded to the package, conditions and observations actually established. No cosmetic change is required merely because the native donor’s non-cutout loop also appears on the summon. The October incident remains unattributed; do not close it as explained or disproved.
- Visual acceptance of the Nalfeshnee, Salamander and Dust cone, and final listening acceptance, remain mine. They are final-review decisions, not reasons to stop the authorized technical work.

This does not authorize a Sprint 7 merge, release, tag, human-installation change, new paid asset, new dependency, relaxed runtime guard, or controller-policy change. Earlier Sprint 6 authorization does not extend to Sprint 7.

## 2. Resume through the supported foreground amendment path

Save this entire instruction verbatim as a new durable UTF-8 owner-amendment file outside the worktree. Preserve the original pinned mission.

First inspect the actual enrollment with the installed controller’s `status --remote` and current help. Confirm the enrollment, branch, PR, head, phase, outstanding findings, pinned workflow and counters. The expected state is BLOCKED/develop after 3 of 5 accepted reviews. If it differs, reconcile it through the supported procedure without overwriting other work or changing limits.

After any active review, verify, push or runtime transaction has safely finished, apply this owner amendment from the enrolled foreground session:

```text
python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --reason "Owner-authorized Sprint 7 correction and acceptance clarification after round 3" --amend-mission <absolute-path-to-this-verbatim-owner-amendment>
```

The amendment command itself resumes the eligible enrollment. Do not add `--max-rounds`. Do not use `--guidance`: that option is for headless enrollments. Do not start a background run or create another enrollment.

Read the complete returned effective task and delivery marker. Display every numbered instruction part if the output is split. Carry the exact returned `report_citation` into the next report’s `evidence_refs`. Preserve Claude Opus/high, independent Codex gpt-6.2-astra/MAX, required gates and the existing permission mode.

The owner’s authorization here permits the supported resumption after the no-progress stop. It does not authorize manually resetting counters, altering findings, disabling the no-progress rule, or increasing any budget. The three completed reviews and all prior failures remain recorded.

## 3. Work from all twelve pending findings

Create one compact correction matrix covering R1-F1, R1-F3, R1-F5, R1-F9, R2-F2, R2-F4 and R3-F1 through R3-F6. For each, identify the exact trigger, owning code, change or amended disposition, decisive check, evidence identity and remaining limit.

Batch related source and observer corrections before expensive packaging. Reproduce the important failure boundaries with meaningful behavioral checks before repairing them. A passing helper test or source-string assertion is insufficient when the defect occurs through the full caller/cleanup sequence.

### Production Dust ownership — R1-F9

Fix the interval the reviewer actually identified:

1. An enclosing native creation is still active.
2. An inner owned Dust CreateView is interrupted.
3. Before the enclosing call returns, a foreign spawn uses the same prefab and position.
4. A subsequent valid owned creation still needs its own tint.

The foreign spawn must remain native and must not consume the owned cast’s tint opportunity. Preserve enclosing ownership, normal nesting, shared prefab/material state, concurrent consumers and pooled-instance restoration.

Bind adoption to the actual native spawn invocation or establish an exception-safe ownership interval using the installed Harmony12 capabilities. Do not assume Harmony 2 finalizers exist. Ending the outer scope before the foreign spawn does not test the defect.

### Trumpet rule and random-state ownership — R2-F2

Correlate any forced saving throw with the exact observed blast execution. Confine shared RNG intervention to the owned rule, or eliminate it if the fixture can achieve its purpose without it.

An unrelated save and random draw interleaved while the blast command yields must retain their normal behavior. Restoring an RNG snapshot after an entire multi-frame command must not rewind unrelated work. Abandonment must end the owned subscription and intervention.

### Ghaele cast ownership — R2-F4 and R3-F5

Give the baseline harness’s borrowed ability or exact temporary grant a cast-bounded lifetime. Restore borrowed parameters immediately when the cast settles, before the lifetime observation continues; preserve an unrelated replacement instance.

Own the exact summon command before submission. On timeout or abandonment, settle or cancel only that command and account for any summon it has already produced before the usual registration point. Preserve unrelated commands and units.

Cover a borrowed ability with non-default parameters, replacement of a temporary grant, interruption with the command pending, and interruption after spawn but before ordinary registration. No owned command or late summon may outlive successful cleanup.

### Cleanup results must reach the runner — R3-F2

Fix both Ghaele and Trumpet probes. Iterator disposal must not drain a failing cleanup into a private problems list and then let Release return success from a second, empty drain.

Preserve and return the first cleanup outcome, attempt every independent release once, keep the original test error, and propagate restoration failures into ProbeSlot’s IsolationRestored result.

Exercise this through ProbeSlot with a yielded probe and an injected failing restore. Require the named failure, remaining cleanup attempts, IsolationRestored=false and refusal to advance to another probe. Testing the resource helper and iterator disposal separately is insufficient.

### Every Dust cast’s conditions and hooks — R3-F3 and R3-F4

Track exact Sickened instances introduced by every observed Dust execution, including concurrent, paired and final comparison casts and an interrupted command before normal readback. Remove only those acquired instances. Preserve the foreign-condition control.

Show that cleanup after a later cast, including early exit after application, leaves no cell-owned condition.

Feed normal trace-disposal failures and remaining owned Harmony hooks into the actual cell/restoration verdict. JSON diagnostics alone are insufficient. A failed unpatch must fail restoration while the other independent releases still run. Keep the successful four-to-zero hook observation as the positive control.

### Positive Ghaele attribution — R3-F6

Require positive active non-cutout animation matches and readable driven-body-material comparisons. Empty controllers, inactive/delayed-only animations and missing material values are inconclusive, not confirming samples.

Add the corresponding negative cases. Re-evaluate the retained released/current raw rows through the corrected predicate where their recorded fields suffice. Those retained datasets contain positive comparisons; do not rerun full lifetime sessions solely to change a reader/verdict. Keep old verdicts and label any new evaluation accurately.

## 4. Repair the remaining evidence gaps precisely

### Body-bound gameplay-distance captures — R1-F3

Pass the actual enabled repaired body renderer to the existing party-camera helper. Make the returned image/readback part of the verdict, rather than excluding the party row.

Reject absent, disabled, wrong-body, unlit or out-of-frame captures as applicable to the helper’s actual contract. Reacquire the affected matched before/after gameplay-distance views. Keep already-valid generator, detail, natural-appearance and motion evidence. Identify or replace obstructed whole-body views only where they support a required claim.

### Controller window — R1-F5

Trace both cleanup2 elementals-console and alternate-console routes. Follow selected-unit ownership, action-bar binding, InspectVm and actual Inspect dispatch. The record’s assertion that only teleportation/firearm console routes exist is incorrect and must be corrected.

Use only operations admitted by the applicable contracts. The existence of the scene is not permission to select arbitrary actors or bypass a selector guard.

If an existing route admits the required operation, obtain the owned-action and control readbacks. If it does not, name the first unsupported operation and exact guard/source. A source-grounded unavailable-operation disposition may be evaluated by the independent reviewer; it must remain visibly unexecuted and cannot be labelled a controller PASS. Do not create a new broad fixture admission in this continuation.

### Released Ghaele combat and lighting — R3-F1

Inspect the applicable baseline-capable admitted fixtures once. “This save has no hostile” alone is insufficient to establish that the required observation is unavailable.

Perform missing released-package combat/lighting observations where an existing contract supports them. Otherwise provide the precise actor, scene or operation boundary and retain the missing conditions in the outcome table. Do not silently expand a guarded scenario or replace released evidence with candidate evidence.

Keep the useful released mansion timeline and the October historical uncertainty.

## 5. Deliver a real private listening packet — R1-F1

Use the existing curated recordings where sufficient. Prepare a small self-contained private playback directory or archive for me, containing:

- Actual Ankylosaurus impact and Trumpet blast excerpts, clearly distinguished from isolated alternatives.
- A local index/playlist with working relative audio links and ordinary player controls.
- Clip hashes, source session/package, source interval, event/cue identity, emitter, volume settings and available listener-position context.
- Clearly labelled provisional choices and the specific listening comparisons still awaiting me.

Keep proprietary recordings out of public Git, release packages and public attachments. Sanitized manifests and records may be committed under the existing policy. Do not patch the controller, broaden reviewer filesystem permissions or manually inject files into its detached snapshot.

Name the actual private packet and absolute location in the handoff. Verify referenced files exist, decode, match their hashes and are usable through the intended owner’s playback route. A list of eighteen inaccessible lab paths is not a delivered review packet. If a supported authorized private transfer already exists, use it and verify delivery; do not invent one or initiate a new service/account.

Correct the distance metadata now. The old tail-hit distances were sampled after the sequence, not at posting. Preserve them under that accurate label. Record event-time positions/distance during affected new captures where needed, and explicitly mark unavailable historical measurements rather than estimating them. Likewise distinguish missing R6 Trumpet distance from measured context.

Keep these statements separate:

- Technical playback/timing and ownership verified.
- Clip integrity and playback accessibility verified.
- Actual listening performed by a named capable reviewer, if it happened.
- Subjective suitability and my final choice.

Neither signal levels nor event IDs prove that someone listened. If the agents lack supported audio perception, say so and retain the current choices provisionally; do not fabricate a listening recommendation.

Under this amendment, the independent reviewer should assess R1-F1 against technical integrity, truthful context and a concrete owner-playback handoff. Owner listening may remain pending. If the intended playback route is genuinely unavailable, complete all independent repairs and report the precise remaining delivery blocker; do not spend another review round submitting unchanged inaccessible references.

## 6. Validation and cost discipline

Preserve the independently verified repairs and prior successful evidence at their original identities. Do not reopen Nalfeshnee or Salamander generation, tail cue deduplication, desktop tooltips, or normal concurrent Dust restoration without a concrete changed dependency.

Finish the coordinated offline repairs and negative cases before producing another candidate. Build an unchanged candidate once and reuse it. RuntimeTesting changes compiled into the DLL still change candidate identity.

Use the affected named Focused gates through the controller, including meaningful tests of the real cleanup/ownership boundaries. Reject empty selections. Avoid new tests that merely search implementation strings.

Plan native work around the affected owners: Dust/effects isolation, Ghaele/Trumpet probe lifecycle, body-bound party captures, and any supported missing controller/baseline observation. Combine compatible observations and reuse unchanged evidence. Do not rerun the entire seven-story campaign or broaden into Sprint 6, full compatibility or release qualification.

Honor the existing validation and controller budgets. Record the proposed selected cost before expensive work and the actual cumulative cost afterwards. Do not evade ceilings by renaming unchanged campaigns. Always finish necessary restoration safely.

A corrected reader may re-evaluate immutable raw evidence when it contains every required observation. A changed observer needs the affected native check again. Preserve the distinction between original execution, re-evaluation and inference.

## 7. Independent review and stopping rules

Proceed autonomously through the coherent correction and the next independent review. Use the existing two remaining review rounds; round 5 is available for specific remaining corrections, not a fresh campaign.

Before submitting round 4, verify that every pending finding has a concrete disposition and the exact requested trigger has been addressed, or that a precise amended scope/guard limitation is ready for review. Do not spend that round on the same four unresolved explanations.

Use a schema-valid report bound to the actual source head and owner-amendment revision. Include a concise twelve-finding matrix, affected validation, retained evidence, media delivery and exact limitations.

If the reviewer requires changes, correct them within the existing limits and continue without another routine permission stop. If a mandatory controller/runtime/integrity stop recurs, preserve it and report the precise reason. Do not increase limits, re-enroll, silently narrow scope or falsify a readiness claim.

Do not describe all seven stories as technically verified while a blocking product or observer defect remains. Distinguish implementation, observed ordinary behavior, unexecuted failure paths, retained development limitations and owner taste.

## 8. Completion and handoff

Only after the controller returns ready_for_owner_review:

```text
python C:/Dev/agent-review/agent_review.py finalize --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1
python C:/Dev/agent-review/agent_review.py status --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --remote
```

Confirm AWAITING_OWNER_REVIEW and handoff_current=true. Preserve the distinction between reviewed source H and controller-authored archive A.

The final handoff should contain:

- The seven original story outcomes and any precise retained limitations.
- All twelve pending findings’ reviewed dispositions.
- Exact source, package, DLL, harness and evidence identities.
- The curated visual set and actual private listening packet.
- The remaining visual/listening choices for me.
- Review counters, expenditure, restoration and current controller status.

Retain every historical FAIL, superseded result and unresolved October scope. Nothing in this continuation authorizes merging or releasing Sprint 7.
----- END OWNER AMENDMENT 1 -----

----- BEGIN OWNER AMENDMENT 2 (captured 2026-11-24T19:29:49Z; message sha256 cc57e9e900dca04c22a30bff92d45661a3638b769a7d5b598825a877d3cd9bb7) -----
Continue PR #219 through one coordinated completion pass. I authorize increasing the existing enrollment’s independent-review cap from five to seven total reviews. Use round 6 for the consolidated repair and reserve round 7 for specific remaining corrections. Do not create a new enrollment, reset spent work, or authorize any further extension yourself.

Enrollment: `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`  
Branch: `codex/s7-visual-audio-remediation`  
PR: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219  
Last reviewed source: `091446a0797a3fd73b66add84adab4caf5f01c49`  
Reported current head: `350b3b578071c04e7ca4cf29554a103650a831d7`  
Base: `35763430acc9eefc306b861045526a444690bc77`  
Round-five review: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219#issuecomment-7141521941

The next pass must reduce the complexity of the verification code and exercise the actual caller and unwind boundaries. Do not continue adding isolated helper checks followed by full native reruns.

## 1. Authority and acceptance

This instruction is a second owner amendment. It preserves the original mission and amendment 1 except for the explicit decisions here:

- I authorize seven total independent review rounds through the supported owner extension operation. All five spent rounds, findings, failed sessions, candidate identities and prior amendments remain recorded.
- I authorize the single cumulative validation exception specified in section 7. Do not change repository-wide validation policy.
- I accept the bounded Ghaele development scope: the demonstrated released/current interior observations, with the released outdoor/bright case unexecuted and the October incident unattributed. This accepts the scope boundary; the outstanding reader and observer defects still require correction.
- I accept the independently verified controller-admission disposition. Controller rendering remains unexecuted, not PASS. Do not reopen the admission investigation or introduce a new console fixture in this continuation.
- Preserve the approved Trample/Overrun wording, released Dust Sickened behavior, verified Dust call-site binding, model repairs and provisional audio selections.
- Visual approval and actual listening remain pending my review. Do not fabricate them or ask the text reviewer to hear audio. The existing private packet and its truthful provenance remain the audio delivery contract.
- I authorize the conditional impact-cue disposition in section 4 only if its stated evidence is established. An unexplained missing callback by itself is not accepted as harmless.

No Sprint 7 merge, release, tag, human-installation change, loader change, new dependency, broader fixture admission or review-controller modification is authorized. All other controller limits, models, permissions, required gates and runtime protections remain in force.

## 2. Apply the amendment, then extend the exhausted cap

Save this entire instruction verbatim as a new durable UTF-8 owner-amendment file outside the worktree. Do not edit the original mission or amendment 1.

Inspect current help and `status --remote`. Confirm the same PR, branch, enrollment, pinned workflow, current head, findings and counters. Expected: BLOCKED/develop at 5/5. Preserve any subsequent work and reconcile an unexpected state through the supported procedure.

After active review/verify/push commands and runtime restoration finish, apply the amendment from the enrolled foreground session:

```text
python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --reason "Owner amendment for bounded Sprint 7 completion after round 5" --amend-mission <absolute-path-to-this-verbatim-owner-instruction>
```

At the exhausted cap, an applied amendment can correctly leave the enrollment BLOCKED. Confirm the selected revision/hash and that the amendment was applied. Do not treat that expected capped result as a reason to re-enroll or repeatedly reapply it.

Then perform the separately authorized extension:

```text
python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --reason "Owner authorizes two additional reviews, seven total, for the bounded Sprint 7 correction" --max-rounds 7
```

Do not combine `--max-rounds` with `--amend-mission`. Do not use headless `--guidance` or start a background run.

Read the complete effective task with `status --instructions`, including every numbered part and delivery marker. Carry the exact returned `report_citation` in the next developer report’s `evidence_refs`. Preserve Claude Opus/high and independent Codex gpt-6.2-astra/MAX.

If an operation is refused for integrity, identity, runtime locking or another exhausted budget, report that actual reason. This authorization does not bypass those checks.

## 3. Account for all eleven findings before packaging

Maintain one concise matrix for R1-F1, R2-F2, R4-F1, R4-F2, R4-F5 and R5-F1 through R5-F6. For each, name the trigger, owning code, correction or explicit disposition, decisive check and exact evidence.

Two findings are record reconciliation; eight concern observers/harnesses; R4-F5 is the unresolved playback/tracking requirement. Do not present them as eleven new gameplay defects, or dismiss them because most are verification code.

Finish related source and behavioral-test corrections together. Prefer removing unnecessary interventions and consolidating resource ownership using the existing infrastructure. Do not introduce a general-purpose framework or new library.

## 4. Resolve impact-cue callback loss with one bounded characterization — R4-F5

Start from the actual evidence:

- In 164749Z, retired ID 499 received its own stop request and end callback. Survivor 497 had no observed stop request and no end callback within the wait. Its native playback state was not measured.
- In 180635Z, the per-ID trace established ordinary playback and completion, but the pass predicate did not enforce those recorded playback/tracking fields.

Do not infer a cross-source stop, indefinitely audible sound, or harmless bookkeeping from the missing historical measurement. Do not repeatedly rerun ordinary sessions hoping to recreate the intermittent failure.

Where feasible, use the existing accepted package and an isolated harness to characterize the missing-callback path before deciding whether production code needs a change. Use two real overlapping cues and narrowly withhold completion-callback delivery to one exact survivor’s actual product playback record. Verify its native playing ID was observed live. Merely hiding the callback from the harness while the product receives it does not test callback loss. Keep native playback itself intact and observe it independently. Label the suppression as synthetic and retain a normal-callback control.

Establish, within a window declared before launch:

1. Both exact playing IDs are genuinely live before retirement.
2. Retiring the other source requests a stop only for its own ID.
3. The survivor’s native playback, callback status and tracking are observed separately through the window.
4. Retiring/destroying the survivor’s own source/view accounts for its tracking and any remaining owned playback. Observe actual disposal; a `Destroy()` request with disposal still pending is insufficient.
5. Unrelated sources remain unaffected and the observer restores its own intervention.

Use the installed native query contract. Distinguish confirmed playing, confirmed inactive, and unavailable/unknown query results. An API failure, uninitialized sound engine or timeout is not proof of completion.

Make playback/tracking evidence part of the verdict, not merely diagnostic JSON.

Conditional owner disposition: if this proves that playback ends normally, another source is unaffected, and the only residual is source-local bookkeeping that disappears when that same source/view is disposed, I accept that temporary retention as a documented development limitation. Do not add a runtime watchdog solely to eliminate such proven harmless retention. Judge active playback ownership, not intentional bounded diagnostic history such as `Recent`; preserve historical evidence records.

If playback persists unexpectedly, another source is affected, tracking outlives its owner, or the result remains unknown, correct or further bound the exact owner within this continuation’s limits. Do not mark R4-F5 resolved without the required evidence.

Keep the historical failure and its unknown cause. Never manufacture `EndUtc` or call a returned stop request completion. If queried inactivity is used, record it separately from an actual callback.

If the conditional limitation is used to change a previous acceptance requirement, give the controller the appropriate amendment-based disposition and obtain independent reviewer acceptance; do not label a policy disposition a code repair.

## 5. Repair the observers as coherent ownership lifetimes

### Trumpet intervention — R2-F2

First determine whether forced saves are necessary for this audio/reload observation. If the existing admitted fixture can safely isolate or tolerate the native blast outcomes, remove the intervention and the unnecessary claim machinery.

Do not remove it casually: the blast can affect nearby non-archons, including allies. Preserve fixture safety and account for conditions without changing gameplay or expanding admission.

If intervention remains necessary, bind it to the native save’s actual die. “First RuleRollD20 whose parent is the open save” is insufficient.

The decisive case directly dispatches a foreign RuleRollD20 inside the owned save before the native save roll. Verify the foreign die, owned save and following unrelated draw against controls. A complete nested RuleSavingThrow is a different case and does not substitute for this one. Preserve exact command correlation and abandonment cleanup.

### Dust cleanup ownership — R4-F1

Every ledger acquired by the effects cell must contribute to one cumulative restoration result, including `earlyLedger`.

Prefer one parent-owned acquisition tree. If a child ledger remains necessary, register its cleanup with the parent as soon as acquired and merge its result during unwind. A failure must remain visible even when disposal skips all code after the yield.

Settle or report both the owned command and its still-live execution process. `IsFinished` alone is insufficient if `ExecutionProcess` has not ended.

Exercise the actual enclosing effects runner: yield after the early cast applies its condition, inject one owned condition-removal failure, then dispose the enclosing runner. Require the phase restoration verdict to fail with the named error, preserve the foreign condition, attempt other independent releases, and prevent continuation on falsely reported isolation.

Do not satisfy this with a main-ledger-only test, trace-unpatch-only test, or a helper result that never reaches the caller.

### Ghaele combat and resource ownership — R4-F2, R5-F4, R5-F5

Release exact probe-owned attacks and the disposable hostile. Let native cleanup settle their engagement where supported. Intake “was not fighting” does not establish ownership of every engagement acquired later.

If independent combat appears, preserve it. Do not call unit-wide `LeaveCombat` to manufacture a clean result. If ownership cannot be established, report the conflict instead of clearing unrelated state. Exercise a second engagement acquired after clean intake.

Register the main-character witness move separately before issuing it. It must survive the combat cleanup being tested and then be settled immediately after that survival readback or on overall probe abandonment. Leaving the witness running is not successful cleanup.

Give each disposable unit one destruction-request owner. Enumerate final remaining resources after iterator cleanup, or track that destruction was already requested. Do not rely on `Destroyed` becoming true immediately after `Destroy`.

Test the real `ProbeSlot → Release → iterator-finally → remaining-resources` sequence with deferred destruction. Require exactly one request per owned target, retained errors and continued attempts of other independent releases.

These corrections should share a clear resource lifecycle; do not implement a speculative global combat-ownership subsystem.

### Ghaele pending-cast observation — R5-F1

Prepare the known foreign same-caster summon before queuing the owned pending cast where possible. Immediately before disposal, verify that the exact intended command is still pending and the exact ability loan/grant is still held, after all control setup.

Keep a separate read-only, execution-correlated late-spawn witness alive through the entire observation window after cast-owner disposal. Removing the handle from the ownership list must not remove the only way to detect the late result.

Show that the known foreign summon survives, the owned command/grant settle, and no late owned summon appears. Include a controlled late-owned-result negative case that makes this verdict fail. Account for the independent witness’s own subscription when the observation ends.

### Pure Ghaele readers — R5-F2 and R5-F3

Evaluate independently readable controller contradictions before deciding whether missing body identity prevents positive credit. Five positive rows plus a null-body row containing a cutout or unknown animation must fail attribution. Null body with no contradictory observation remains inconclusive.

Require the captured subject’s `faderVisible=true` for the visibility claim. False or unavailable visibility must fail with a distinct reason.

Use focused negative cases and re-evaluate the complete retained 182757Z and 183206Z datasets. They contain the necessary fields. Do not repeat full lifetime observations or model captures solely for these reader changes. Label the new evaluation and preserve original verdicts and identities.

## 6. Finish the record corrections from existing artifacts

R1-F1: verify every active handoff against twelve isolated clips, comprising two current cues and ten alternatives. Trumpet 397 posts on frame 4975; the distance observation is on 4976. Keep that one-frame distinction everywhere. No new recording is needed for this correction.

R5-F6: report four passing concurrent-retirement cases in five relevant runs, with two passes before and two after 164749Z. Preserve all historical labels.

The committed effects extract contains 33 structured predicates. Report 33, or make the claimed additional plain-text restoration assertion reviewable with its exact existing source/hash and sanitized line. Do not combine an inaccessible assertion into the denominator. This requires no game run.

Correct the stale “Six are observer, harness or record issues” introduction and avoid suggesting the cue owner is already cleared while R4-F5 remains unresolved.

Preserve the existing private listening packet and source identities. Do not regenerate clips or recapture unchanged art for record wording. Packet decoding, technical playback evidence, actual human listening and owner taste remain distinct.

## 7. One explicit, finite validation window

For this continuation only, I authorize up to **60 minutes of cumulative validation-command elapsed time across rounds 6 and 7 combined**. This is a one-off exception to the ordinary 30-minute validation ceiling, not 60 minutes per batch or per review. Target completion within 30 minutes.

Count product/package builds, harness builds, focused validation and all guarded native sessions, including failed attempts. Ordinary source inspection/editing and independent model-review elapsed time are outside this validation allowance. Necessary safe completion/restoration already in progress remains exempt; never interrupt restoration to satisfy a clock.

Do not edit `VALIDATION-POLICY.md` or controller policy to encode this exception. Record the exact owner authority and cumulative expenditure in the Sprint 7 record.

Before expensive work, select the minimum checks and estimated cost. Use caller-level offline regressions and retained-row evaluation first. Complete the coherent corrections before producing a candidate.

At most **two new product candidate builds** are authorized across this extension. Build one unchanged candidate once; the second is available only for a concrete subsequent correction changing compiled product or RuntimeTesting bytes. Separate harness changes do not by themselves require a product rebuild.

Use the same required controller gates. Do not run a broad suite, full seven-story native campaign, unchanged asset regeneration, new loader/profile matrix, or Sprint 6/release qualification.

Native work should focus on the missing-callback characterization and the corrected abandonment/ownership paths. Reuse successful ordinary evidence where dependencies still match. A changed observer needs its affected native scenario again; a pure reader correction can re-evaluate immutable observations.

Do not launch a check whose estimated cost exceeds the remaining allowance. Do not replenish the budget by renaming batches, resetting the enrollment or ignoring failed attempts. If the work cannot be completed within the defined limits, preserve the exact residual blocker and stop after safe restoration.

## 8. Review discipline and completion

Before round 6, verify that every pending finding has its exact trigger addressed, a demonstrated conditional disposition, or a precise remaining blocker. Do not spend another round substituting a nearby test for the requested failure interval.

Submit one coherent report bound to the current source head and amendment revision, with the eleven-finding matrix, exact validation, retained evidence and remaining owner judgments. Continue autonomously through specific review corrections while budget remains.

A repeated no-progress, integrity or runtime stop remains mandatory. This instruction does not authorize automatically resuming such a stop or increasing any other limit.

Only after `ready_for_owner_review`:

```text
python C:/Dev/agent-review/agent_review.py finalize --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1
python C:/Dev/agent-review/agent_review.py status --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --remote
```

Confirm `AWAITING_OWNER_REVIEW` and `handoff_current=true`. Distinguish reviewed source H from archive A. Report the seven story outcomes, reviewed finding dispositions, candidate/evidence identities, exact retained limitations, private media location, cumulative cost, restoration and remaining visual/listening decisions.

Nothing in this continuation authorizes merging or releasing Sprint 7.
----- END OWNER AMENDMENT 2 -----

----- BEGIN OWNER AMENDMENT 3 (captured 2026-11-24T21:58:46Z; message sha256 70e46f795beab11f5b0761c10a02b964cbec6ecaa7dae16dced472daf7645cf4) -----
OWNER AMENDMENT 3 — SPRINT 7 COMPLETION AFTER ROUND 6

Continue the existing Sprint 7 mission and enrollment. This amendment authorizes one resumption after the specific round-6 no-progress stop, one additional candidate build, and the bounded validation allowance below. It does not authorize merging, releasing, tagging, changing the human installation, or starting Sprint 8.

## 1. Preserve the accepted work and the actual outstanding scope

Enrollment: `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`  
PR: #219  
Branch: `codex/s7-visual-audio-remediation`  
Base: `35763430acc9eefc306b861045526a444690bc77`  
Round-6 reviewed head: `b37faaf883cf71d086a1fb140c3ea7eb3c98966a`  
Reported stopped head: `6780d74f8a405dd01e0f21a0f5a042d242de1726`

Verify these against current remote state before writing. Preserve unrelated work and all earlier findings, verdicts, packages, amendments and evidence.

Keep the seven round-6 verified findings closed. Keep R4-F5’s accepted conditional disposition: in the characterized missing-callback case, playback ended normally, the other source was unaffected, and the residual tracking belonged to the survivor’s own source and ended with actual source/view disposal. The original `164749Z` cause remains unknown. The failed in-run predicates in `200320Z` and `202648Z` remain FAIL; the accepted interpretation of the latter is separately recorded.

Do not add a production watchdog or change ordinary playback merely to clear accepted source-local bookkeeping. Do not manufacture `EndUtc` or confuse a stop request, a native inactivity query and a received completion callback.

The remaining findings are **R4-F1, R5-F1, R5-F3, and R6-F1 through R6-F5**. Address their shared ownership and evidence contracts coherently. Maintain one concise matrix identifying each trigger, owning caller, change, decisive test, exact evidence and disposition.

All prior scope decisions remain: bounded interior Ghaele observations accepted; October incident unattributed; released outdoor Ghaele and controller rendering unexecuted; approved Trample/Overrun wording retained; Dust keeps released Sickened mechanics; both audio cues remain provisional. Human visual/listening acceptance stays pending.

## 2. Resume the stopped enrollment through the supported foreground path

Save this amendment verbatim as a new UTF-8 owner-instruction file outside the worktree. Read the repository and installed workflow instructions; retain the existing developer/reviewer roles and identities.

Check `status --remote`, the branch and enrollment, the completed restoration, and absence of active work that would conflict with resumption. Then use:

```text
python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --reason "Owner authorizes one resumption after the round-6 no-progress stop, one additional candidate, and the bounded completion allowance in amendment 3" --amend-mission <new-owner-amendment-file>
```

The cap remains seven reviews, with six spent. Do not use `--max-rounds`, `--guidance`, a new enrollment, or manual state edits. The supported resume may rearm its consecutive-open counters; preserve the accumulated review history and spent rounds.

Retrieve all `status --instructions` parts, acknowledge delivery as required, and bind the developer report to the actual amendment revision, hash and report citation returned by the controller. Verify `ACTIVE/develop` before proceeding. Do not fabricate readiness or resume automatically after any further mandatory stop.

## 3. Make cleanup govern the actual runner decisions

Use the existing project structures and a small cumulative result where needed. Do not introduce a generalized resource-management framework or a new library.

The same complete cleanup/recovery outcome must reach:

- The child observer’s result.
- Its enclosing runner during normal completion, exceptions and iterator disposal.
- The guard that admits the next cell or probe.
- The final phase or `ProbeSlot` isolation decision.

Register ownership before the first acquisition can yield. Attempt independent releases even if another fails. Preserve original test errors, named cleanup failures and later recovery observations separately. Unknown or unreadable ownership state cannot count as restored.

Expected injected failures remain recorded as failures of the affected child/phase. A passing negative-control test means that it correctly observed that failure and refused advancement. An outer test driver may separately repair its own intervention and prove the fixture safe; that repair cannot erase the original failure. Subsequent work requires complete, explicitly observed isolation.

### R4-F1 — Dust abandonment

Retain the actual execution reference before disposal, or an equally authoritative ledger readback, through the entire recovery wait. A command’s cleared or replaced `ExecutionProcess` field is not evidence that the former execution settled. The existing record of one live execution cannot be cleared by observing only a removed Sickened buff and zero tint instances.

Account for owned executions, commands, conditions, subscriptions, hooks and failed repair actions. Feed both the normal Dust cell and the abandoned/recovered cell into the enclosing phase’s continuation and final-restoration decisions.

Through the real enclosing path, demonstrate:

- The named injected condition-removal failure reaches the failed inner restoration result.
- The exact execution is observed settled before safe reuse.
- A still-live or unreadable execution, residual hook/subscription, or failed independent release prevents the next cell from starting and fails isolation.
- Independent releases continue and foreign effects survive.

Do not substitute an assertion on a helper or an empty command queue for the execution readback and enclosing decisions.

### R6-F2 — callback-hook abandonment

Once acquired, the callback-withholding hook’s cleanup outcome belongs to the enclosing effects phase. Publish it during unwind; code after an iterator’s `finally` is insufficient because disposal can skip it.

Dispose the enclosing runner with the hook acquired and a controlled unpatch failure. Require the named phase-level failure, continued independent cleanup, and a next-cell witness showing that advancement did not occur. The outer test driver must explicitly remove its own injected intervention and establish zero remaining hooks before reuse or exit. Retain the ordinary zero-hook control.

### R6-F1 — early Ghaele releases

Direct destruction requests and witness-command settlement must contribute to the same cumulative cleanup result consumed by `ProbeSlot`.

Keep at most one destruction request for each owned unit. If that request throws, retain a named cleanup failure even though the unit is already marked requested. If witness settlement returns with the exact command still held and unfinished, fail cleanup explicitly.

Run behavioral regressions through the actual `ProbeSlot / Poll / Release / iterator-unwind` path:

- An early direct destruction throws.
- A witness command remains active after settlement.
- Independent releases still run.
- The original test error is preserved.
- Isolation fails and the next probe is refused.
- The destruction case receives exactly one request.

## 4. Correct the observation boundaries and their verdicts

### R5-F1 — genuinely post-disposal late-summon control

Use the independent read-only witness and retain the exact execution correlation independently of the disposed cast owner or a mutable command field.

The negative case must establish zero owned results before disposal, then dispose the actual cast owner, then deliver the exact correlated result to the actual witness handler. Record the ordering and identity; the actual pending-cast verdict must reject the late result. Preserve the foreign same-caster control and prove witness unsubscribe.

A labelled controlled notification injection or deferred delivery is authorized for this observer control. Do not spend time trying to force an ordinary cancelled cast to spawn naturally. Do not set the witness’s count directly or substitute a hardcoded verdict input. Any real unit introduced by the control must be registered with its own cleanup owner before delivery and then accounted for; otherwise isolation fails.

### R5-F3 — missing fader is an unavailable observation

At Ghaele’s observation boundary, capture the exact subject/view, component presence or identity, and visible state. Keep missing, present-hidden and present-visible distinct through the actual producer and reader.

Preserve the shared capture helper’s existing behavior for other callers. Exercise all three Ghaele cases through the producer-to-verdict path, not only by editing a serialized field.

For retained rows, use existing component-presence evidence if available. A historical `faderVisible=true` value produced by the old fallback does not by itself prove that a component existed. State that limit without revoking the already accepted bounded body, movement, combat and expiry observations. A short current capture can qualify the repaired producer; do not repeat full released/current 120-second lifetimes.

### R6-F3 — terminal playback is decisive

Use one settled evaluator that consumes the exact ID’s required terminal playback reads, including the window end and after actual source disposal. Earlier inactivity must not override terminal playing or unknown/missing evidence.

Re-evaluate the immutable `202648Z` and `200320Z` records. In copies of the retained row, change only each required terminal playback field to playing and to unknown/missing; each must prevent characterization. Keep query states, callback records, stop requests, active tracking and intentional diagnostic history distinct.

### R6-F4 — qualify the

---

<!-- agent-review:v1 enrollment=kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 key=r08-request part=2/2 sha256=99b4c5cf1e7acbe215ff8cdeca7fa471204af0fa8c8f86c82e9bda64ad1c41aa -->
_Part 2 of 2._

 collector actually delivered

Bind collector source, package/DLL, harness, immutable observations and evaluator identity explicitly. Prefer one evaluator used for native and retained-row judgments, rather than different reconstructions that happen to agree.

The authorized third candidate can qualify the final changed `RuntimeTesting` collector along with the Dust and hook corrections. If a particular correction is solely a reader over fields the exercised collector already emitted, label it as such and retain that collector’s actual identity. Do not claim offline reinterpretation executed a later live-sampling path.

## 5. Reconcile records without new runs

Finish R6-F5 and remaining stale active text using existing receipts:

- Carry `39m04s` already spent and the new cumulative allowance below.
- Identify counts by their actual gate run; the round-6 receipts report 116 selected-test executions, not an invented number of distinct tests.
- Retain all FAIL verdicts and candidate identities.
- Keep R4-F5 accepted throughout the active record and story table.
- Remove or historicalize “Independent review has not accepted it” and the stale claim that R4-F5 remains an unresolved product requirement.
- Keep the historical callback cause unknown and distinguish the remaining observer findings.
- Identify all applicable owner amendments.
- Update any active Trumpet wording that still suggests seeded or forced dice: the accepted current probe applies `AutoPass` to its correlated save while leaving dice native.
- Keep Ghaele’s bounded scope accepted and visual/listening judgments pending.

The full record, status table, known issues where applicable and final report must agree. Do not recreate evidence for wording corrections.

## 6. One build, one remaining review, explicit validation allowance

This amendment increases the combined rounds-6-and-7 validation-command ceiling from **60 to 75 minutes**. It adds 15 minutes; it does not reset the clock. With the recorded `39m04s` spent, at most **35m56s remains**. Aim to complete within the former remaining `20m56s` where feasible.

This also increases the combined candidate allowance from two to three: **exactly one additional product candidate is authorized**. No fourth candidate or eighth review is authorized. Keep `VALIDATION-POLICY.md` and global controller limits unchanged.

Count failed build/check/run attempts and required gates as before. Source editing and model review are not validation-command time. Safe restoration must always complete even if a timer is reached.

Before the sole new candidate:

1. Finish all compiled observer changes together.
2. Pass realistic caller-level failure regressions, the producer/reader controls and immutable-row verdict tests.
3. Trace acquisition, every exit/unwind, recovery, continuation and final restoration for the affected paths.
4. Estimate the complete remaining command list, including the build, affected native work and final controller gates.
5. Freeze the observer source for packaging.

Do not package a partial local fix and spend the remaining candidate discovering another already-visible missing parent decision.

Run only affected native work: the actual enclosing effects cleanup and final collector, plus the short changed Ghaele lifecycle/visibility interval. Reuse prior accepted model, desktop, Trumpet, audio-packet and full-lifetime evidence at their actual identities. Do not recreate the listening packet, investigate historical cue `497` by repeated ordinary runs, seek new fixtures, or expand controller/outdoor scope.

Do not bypass the enclosing effects path to make a shortened test fit. Expected failure branches can be bounded and explicitly repaired by the outer driver after their failing verdicts are recorded.

If compiled `RuntimeTesting` changes after the third candidate, that changed observer is not qualified by the earlier run. Do not invent equivalence, secretly rebuild, or submit it as verified. Stop with the precise remaining blocker after restoration.

## 7. Finish through the controller

Complete all required focused controller gates on the candidate source head that will be reviewed. Keep gate receipts, source identities and native package identities accurate; do not run an unrelated full suite.

Submit one consolidated disposition report for the eight remaining findings and use the already available round 7. Classify corrected findings, accepted dispositions and retained limitations honestly. Do not claim all findings fixed until supported.

Finalize only after the controller reaches `ready_for_owner_review`. Then obtain `status --remote` showing `AWAITING_OWNER_REVIEW` and `handoff_current=true`. Report reviewed source **H** and archive commit **A** separately.

If round 7 returns changes, another mandatory stop occurs, or this allowance is insufficient, stop safely with the exact state and findings. Do not raise limits or resume again under this amendment.

The handoff must include current identities, per-finding outcomes, validation expenditure, failures and restoration receipts, the existing private listening-packet path, and the remaining owner-only visual/listening decisions. Nothing in this amendment grants merge, release, tag or human-installation authorization.
----- END OWNER AMENDMENT 3 -----

----- BEGIN OWNER AMENDMENT 4 (captured 2026-11-28T02:42:55Z; message sha256 4b9ddb05c71483df0ec95386c5f4a0fd582c6192ed7ef2a720211c7c62ef228a) -----
Continue the existing Sprint 7 parent enrollment and PR #219 under this **owner Amendment 4**.

I accept the completed fast-forward integration onto `codex/s7-visual-audio-remediation` at:

`3bfc570090b9772cac4b92e79983d31455510bca`

C1, C2 and C3 acceptance remains in place at their existing reviewed and archive identities. C1’s GitHub merged status resulting from the authorized parent advance is accepted; do not reopen it or alter the child branches.

This amendment authorizes a bounded parent reconciliation, focused validation, formal review and technical owner-review handoff.

## 1. Authority and limits

Continue the existing parent enrollment:

`kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`

Keep PR #219 and its target `master`. Do not create a replacement enrollment or retarget the PR.

I authorize:

- Parent formal review 8.
- One conditional records-only formal review 9, under section 6 below.
- Raising the parent’s existing amendment-3 validation ceiling from **75 to 85 minutes**.
- Reusing the already-built and qualified recovery candidate 6 as successor evidence for the integrated parent.

Carry forward **65 min 42 s** in the existing parent validation ledger. The amended allowance therefore leaves **19 min 18 s** before new validation.

Preserve earlier parent expenditures and candidate history. Keep the recovery’s separate **345 min 5.6 s expenditure, six candidates, eight C3 reviews, 22 failed validation attempts and separately classified mutation failures** unchanged. Do not transfer its unused allowance to the parent.

This supersedes the parent’s earlier mandatory stop only to permit the work expressly described here. It authorizes no new package, candidate, native session or runtime change, and no review 10.

## 2. Admit the amendment normally

Save this instruction verbatim outside the worktree at:

`C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-OWNER-AMENDMENT-4-2026-11-27.md`

Read the installed workflow instructions and verify the actual enrollment state.

Apply this instruction through supported `resume --amend-mission`, then separately raise the parent ceiling with supported `resume --max-rounds 9`.

Follow the controller’s delivery sequence. Before proceeding, display and read every effective-instruction part intact, including the delivery markers. Record the actual selected revision and digest; do not assume successful admission from a partial or refused command.

Preserve controller history, counters, findings, models, permissions and no-progress thresholds. Do not edit private state or bypass locks.

## 3. Prepare one coherent records submission

Start from the integrated archive above. Make one bounded records-only commit updating:

- `docs/GUNSLINGER-SPRINT7-RECORD.md`
- The active Sprint 7 status and necessary reconciliation pointers in `planning/GUNSLINGER-REMEDIATION-STATUS.md`

Keep the C3 manifest and all existing archived transcripts unchanged.

Use a clearly dated pre-review-8 snapshot. The active records must agree on the actual resumed state, amendment revision, seven completed reviews out of nine authorized, and the nine parent findings awaiting formal reconciliation. Preserve the prior 7/7 stop and its original verdict as history.

Reconcile these parent findings against accepted successor work:

| Parent findings | Accepted successor |
|---|---|
| R7-F1, R7-F6, R6-F5 | C1 corrections and records, including the documented fulfillment of its deferred native criterion |
| R5-F1 | C2’s scoped Ghaele ownership correction and qualification |
| R4-F1, R7-F2, R7-F3, R7-F4, R7-F5 | C3’s accepted implementation, focused cases, candidate-6 qualification and formal review history |

For each, identify the implementation, relevant behavioral/native evidence, acceptance scope and child review disposition.

Until the parent reviewer assesses them, describe the findings as addressed by accepted successor work pending parent reconciliation. Do not alter historical parent FAILs or imply the parent has already passed review 8.

Include the seven-story outcome table required by the original mission, with original problem, delivered behavior or accepted disposition, evidence identity, technical status and remaining owner decision.

## 4. Preserve qualified artifacts and scoped evidence

Reuse candidate 6 without rebuilding or repackaging:

- Code: `360ce86f54174db631e20a6a7fb4420a1fe0b92a`.
- Build source: `962bf27694a797e7a06d674599dfdbba341b00a7`.
- Package SHA-256: `6c71d928ea4293945af43a4b33f81238047176b07ed10fc88edee342649cd55a`.
- DLL SHA-256: `6af754eb06f72a261916d2dcd6e5fea125b62b13dcc9eeae53ad000e94341411`.
- C2 harness SHA-256: `d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1`.
- Qualified session: `20261127T133020Z-owner-0.33.0-s7-c3-mechanics`.

Keep the parent’s original three-candidate history and the recovery’s six-candidate history separately recorded. Reusing candidate 6 does not create a newly built parent candidate.

For this technical reconciliation, use the accepted evidence at its established scope:

- Candidate 6’s 43/43 result covers its seven mechanics cells and separately recorded physical restoration.
- C2’s native lifecycle evidence remains on its original candidate and exact accepted harness.
- Earlier model, UI, presentation and audio materials remain attached to their original candidates and recordings.
- Document the unchanged source, asset or dependency basis for carrying earlier evidence forward.

Do not describe this as a new full presentation campaign on candidate-6 bytes. Preserve the constructed callback-overlap scope, the single native cancellation classification, the source-derived future cancellation guarantee, and the focused-only status of pending-command, uncertainty-recovery and cross-owner-transfer failure cases.

No runtime, harness, asset, dependency, session, test-code or package changes are authorized. If a concrete changed dependency makes required evidence inapplicable, identify that exact obligation and stop before expanding the work.

## 5. Run the four focused gates on the final submission head

The selected validation boundary for this continuation is:

- `sprint7-visual`
- `summon-playtest-r6`
- `native-profile`
- `loader-compat-probe`

Use separate supported controller `verify` calls with the existing configured focused gate. Preserve production compilation and reject zero-test selections.

These filters currently select 25, 11, 9 and 5 tests respectively; report the actual receipt results and counts. Preserve the historical eleven-filter results without relabeling them as new execution.

Do not add an eleven-filter replay, standalone duplicate local gate, broad suite, mutation campaign, package build or game session merely because this is the parent checkpoint.

Before starting, coordinate the validation window through the existing peer protocol and record an estimate against the actual remaining allowance. Recent four-gate batches took approximately 370.4–432.3 seconds of outer time.

Charge full outer validation-command time, including controller overhead and failed attempts. Keep ordinary implementation, model review, archival finalization and status readback separate under the existing accounting rules.

Finish the records commit before the gates. Keep the resulting clean submission head fixed through review. Put actual gate expenditure in the fresh amendment-bound developer report; do not make a post-gate bookkeeping commit that invalidates the receipts.

## 6. Review 8, conditional review 9 and finalization

Submit a complete fresh parent report covering the original seven-story mission, the nine finding dispositions, accepted child evidence, exact submission head, amendment binding, four gate receipts and current accounting.

Use the actual current remote target tip and merge base through the supported workflow. Record and reconcile any target movement; do not substitute the historical base SHA or retain a verdict tied to different comparison inputs.

Do not merge or rebase `master` into this branch during this continuation.

If review 8 permits readiness, finalize normally without using review 9.

Review 9 is authorized only if review 8 leaves concrete records, disposition, evidence-link or comparison-input bookkeeping corrections that preserve the qualified runtime, assets, dependencies, harness, tests, package and native claims. Complete those bounded corrections, obtain the required receipts on the resulting submission head, and use the remaining allowance.

If the controller imposes a no-progress stop after review 8 and the remaining work satisfies that condition, I authorize its supported owner-resume route within the nine-review ceiling. Record the reason and let the controller perform its own bookkeeping; do not change its thresholds.

If review 8 identifies required runtime work, new native qualification or another package, preserve the finding and stop. An eleven-filter replay is not a substitute for missing native evidence.

If review 9 does not permit readiness, or the actual allowance prevents the remaining sequence, preserve the exact state and report the concrete obligation. No review 10 or additional allowance is authorized.

After readiness, complete normal controller finalization and verify:

- `AWAITING_OWNER_REVIEW`
- `handoff_current=true`

Keep the new parent reviewed H and parent archive A distinct. C3 archive `3bfc570090…` is the integration input, not a parent finalization archive.

Let normal finalization records capture the verdict and final expenditure. Do not manually rewrite the reviewed submission after readiness.

## 7. Final report and owner decisions

Return the actual parent result, nine finding dispositions, seven-story outcome table, gate results, reviewed H, archive A, remote handoff status, artifact/evidence identities and final parent accounting.

Provide the existing visual/listening material in an accessible review set, with recommendations and original source identities. Do not collect new captures or claim anyone has listened when they have not.

Keep pending:

- T2 Nalfeshnee, T3 Salamander and T6 Dust visual approval.
- T5 Ankylosaurus and T7 Trumpet listening choices.

Preserve the accepted T1 and T4 dispositions and R4-F5 interpretation.

A clean textual merge with current `master` does not qualify the eventual merged build. Version reconciliation, the newer build/admission contract, merged-tree compilation and any affected post-merge qualification remain later work.

Do not merge into `master`, release, tag, change the human installation, reopen the child recovery work or alter the accepted child branches.
----- END OWNER AMENDMENT 4 -----

----- BEGIN OWNER AMENDMENT 5 (captured 2026-11-28T09:07:08Z; message sha256 8203ea565f0712851a281fff247c68880b7464f418c22b8b52eeccb9747a8350) -----
Proceed with the existing Sprint 7 parent enrollment under this owner amendment.

This is Parent Amendment 5. It authorizes a narrow correction to the inherited transcript layout and continuation toward the existing parent owner-review handoff. It explicitly supersedes Amendment 4’s archive-preservation and four-filter requirements only to the extent stated below. All other scope restrictions, accepted dispositions, and evidence limitations remain in force.

### 1. Starting state and authority

Use the existing PR #219 enrollment:

`kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`

The expected starting state is:

- Parent branch: `codex/s7-visual-audio-remediation`.
- Submission head: `84d2636e0105f79dc7a6416334aa600ee5fbb8b5`.
- Controller: BLOCKED in the review phase, seven completed reviews out of nine authorized.
- Selected amendment: revision 4 before admission of this amendment.
- Nine parent findings remain open.
- Review 8 has not launched.
- Parent validation ledger: 71 min 31.0 s used out of 85 min, leaving 13 min 29.0 s.

Preserve the failed pre-review invocation and its actual elapsed time as history. Do not count it as a completed formal review, a passed review, or a new finding.

Read the current status and branch identities before making changes. If the actual starting head or enrollment state differs materially, report the discrepancy rather than resetting it.

Save this amendment verbatim to:

`C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-OWNER-AMENDMENT-5-2026-11-28.md`

Admit it through the supported `resume --amend-mission` path for this enrollment. Display and read every returned instruction part and record the actual amendment revision and digest. Confirm the supported transition back to development.

Do not begin with an ordinary resume of the blocked review. The review ceiling remains nine; no increase is authorized.

### 2. Authorized transcript correction

In one bounded parent commit, remove exactly the inherited child transcript copies currently tracked under:

- `docs/agent-reviews/pr-221/`: 25 files.
- `docs/agent-reviews/pr-222/`: 24 files.
- `docs/agent-reviews/pr-223/`: 36 files.

This is explicit authorization to remove those 85 copies from the parent’s current tree. It is a narrow exception to the previous instruction that all archived transcripts remain unchanged there.

Preserve the child branches and their exact accepted archive commits:

- C1: `2c67a049ee33e0b29621fd29a8b25608ed31e956`.
- C2: `eab90a5ac82f661ecb903c5456d951a57a085eb9`.
- C3: `3bfc570090b9772cac4b92e79983d31455510bca`.

Preserve their complete history, all accepted reviewed heads, the existing parent submission in history, and all archived file contents at those commits. The accepted child commits must remain ancestors of the new parent head.

Preserve the existing `pr-195` records and any parent-owned archived records. Leave the C3 manifest unchanged.

Do not rewrite history, move the child transcript copies into another directory, manufacture parent archive records, edit the controller’s private inventory, modify the transcript guard, or create a replacement enrollment.

### 3. Parent records and citations

Alongside the 85 removals, make only the necessary updates to:

- `docs/GUNSLINGER-SPRINT7-RECORD.md`
- The active Sprint 7 section of `planning/GUNSLINGER-REMEDIATION-STATUS.md`

Record the pre-review stop, this amendment, the reason for removing inherited copies, and the resulting evidence-location policy.

Replace parent references that depend on the removed child directories with immutable links to the accepted archives:

https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/2c67a049ee33e0b29621fd29a8b25608ed31e956/docs/agent-reviews/pr-221

https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/eab90a5ac82f661ecb903c5456d951a57a085eb9/docs/agent-reviews/pr-222

https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/3bfc570090b9772cac4b92e79983d31455510bca/docs/agent-reviews/pr-223

Where a cited child manifest uses relative evidence links, cite that manifest at its accepted child archive commit so those links retain their original context.

Keep the nine parent dispositions described as addressed by accepted successor work pending parent reconciliation. Preserve historical FAILs and the seven-story outcome distinctions.

Keep the committed state a dated pre-submission snapshot. Do not predict a successful review or update it after gates merely to record their timing.

### 4. Preflight before spending validation allowance

Use batched Git tree and diff reads where possible.

Before running a gate:

1. Verify the intended removal inventory is exactly the 85 inherited child files and that the older protected records are unchanged.
2. Verify the bounded commit changes only those 85 files and the two permitted parent records.
3. Verify compiled source, tests, harnesses, session definitions, assets, dependencies, build scripts, and controller configuration are unchanged from `84d2636e0105f79dc7a6416334aa600ee5fbb8b5`.
4. Verify the accepted child branches still point to their archive commits and the accepted reviewed and archive commits remain ancestors of the parent.
5. Refresh the actual target ref and compute the merge base the controller will use. Do not merge or rebase the target into the parent.
6. Check the controller-equivalent protected-path delta on the committed candidate:

   `git diff --raw -z --no-renames --no-ext-diff <actual-merge-base> <new-head> -- docs/agent-reviews`

For this submission, the expected result is empty. The test concerns the net merge-base-to-head delta, not the deletions shown by the newest commit alone.

If that result is not empty, identify the exact remaining change and stop before spending gate time. Do not alter controller state to make it pass.

Push the clean bounded submission through the existing push wrapper. Keep the submission head fixed throughout its gates and review.

### 5. Validation requirement and allowance

For this strictly records-only continuation, I explicitly replace Amendment 4’s requirement for four fresh filter selections with one fresh run of the configured `focused` gate using:

`-Filter sprint7-visual`

Run it on the final clean submission head using the normal controller gate mechanism.

Retain all four passing selections from `84d2636e0105f79dc7a6416334aa600ee5fbb8b5` as evidence on that exact head. The unchanged-code rationale may cite them, but do not relabel their receipts as runs on the new head.

This amendment changes the mission’s filter requirement. It does not authorize changes to project gate configuration, receipt validation, required hosted checks, or controller enforcement.

The parent validation ceiling remains 85 minutes. Carry forward 71 min 31.0 s spent and 13 min 29.0 s remaining. Keep the C3 ledger separate and unchanged.

Estimate the new run using current conditions and coordinate through the existing shared-resource rules before starting. Charge its full outer elapsed time, including wrapper overhead. Do not substitute the smaller inner receipt duration.

Do not start validation that is expected to exceed the remaining allowance. If actual execution exhausts the allowance, use the supported stop procedure, preserve the resulting state and receipts, and report it without extending authority or treating an interrupted run as a pass.

Controller admission, review, and finalize wall times should remain visible in the report under their proper classifications. Do not describe the previous 77 min 44 s pre-review invocation as model review time.

No new candidate, package build, native session, capture, or listening session is authorized.

### 6. Formal review and conditional correction

Prepare a fresh developer report bound to the new amendment and exact submission head.

Include:

- The inherited-transcript correction and empty protected-path delta.
- Immutable child archive citations.
- All nine parent dispositions, still pending formal reconciliation.
- The existing seven-story outcome table and candidate/evidence boundaries.
- The fresh required gate receipt on the new head.
- Supporting earlier receipts with their original head identities.
- Actual validation accounting and coordination receipts.

Submit formal review 8 through the existing controller.

If review 8 finds the submission ready for owner review, proceed through normal finalize and remote-status verification.

If review 8 requires only a bounded records, citation, disposition, or comparison-bookkeeping correction within this authorized scope, one correction and formal review 9 are authorized. A changed submission head requires a fresh `focused` gate with `-Filter sprint7-visual`, under the same remaining 85-minute allowance.

The conditional ninth review does not authorize runtime, test, harness, asset, dependency, build-contract, package, or native-evidence changes. There is no tenth review, replacement enrollment, or additional validation allowance.

If a different blocker appears, preserve and report its exact cause. Do not repeat an unchanged rejected submission.

### 7. Handoff verification

After a ready result, allow normal controller finalization to create the parent archive.

Report reviewed H and archive A separately. Verify that finalization adds only the controller-owned parent archive records, and verify the remote state rather than inferring handoff from a successful review.

The intended endpoint is:

- `AWAITING_OWNER_REVIEW`
- `handoff_current=true`
- No unarchived review material
- No held controller lock

Keep actual post-commit gate and review outcomes in the developer report and normal controller archive so no intervening records commit invalidates the reviewed head.

### 8. Preserved scope and final report

C1, C2, and C3 acceptance stands. Candidate 6 and all native evidence retain their existing identities and limitations.

Keep the distinction between seven completed mechanics cells and the seven parent stories. Earlier presentation evidence remains tied to its original candidates. C2’s Ghaele evidence remains tied to candidate 3. Preserve the disclosed cancellation, pending-command, uncertainty-recovery, ownership-transfer, and callback-coverage limits.

T2/T3/T6 visual decisions and T5/T7 listening decisions remain pending with me.

Repository maintenance is outside this amendment. Do not repack, prune, expire reflogs, run garbage collection, delete pack files, or modify controller code as part of this recovery. Releasing an S7 reservation does not waive shared locks or other sessions’ coordination rules.

Do not integrate master, change versions, merge or close PRs outside the controller, tag, release, or touch the human installation.

Conclude with the exact final branch and controller state, reviewed and archive heads if produced, transcript-removal inventory, immutable archive locations, gate receipts and their heads, updated validation ledger, formal review counts, and any remaining blocker.
----- END OWNER AMENDMENT 5 -----
````

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
