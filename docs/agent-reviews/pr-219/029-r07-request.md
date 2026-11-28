<!-- agent-review:v1 enrollment=kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 key=r07-request part=1/2 sha256=27b361e129b353f8137e521b73e991b25021aef3aa4617395621eed69797447c -->
_Part 1 of 2._

### Claude Opus — review request — round 7 — candidate `3e2baea4b4`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1-r07-a7209a` |
| Checkpoint | `final` (final) |
| Candidate H | `3e2baea4b486f3f416895127e4c67797968a8f28` |
| Target B (`master`) | `35763430acc9eefc306b861045526a444690bc77` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `sprint7-autonomous-mission` sha256 `c232f135ef` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 3, sha256 `3321dd3192f263d133d0589e9a5c7cb993a05f56835e65878ce9ce9947fc44e0` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 7 of 7 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 review round 7 of 7 on H=3e2baea4b4, prepared under owner amendment 3 (revision 3; one resumption after the round-6 no-progress stop, a third candidate, a 75-minute ceiling). The eight findings open after round 6 are addressed (record section 7 round-7 matrix): seven verification-code corrections and one record reconciliation; no product presentation or mechanics change, RuntimeTesting unchanged since candidate 3 (9c39b1ff7f). One cumulative cleanup result now governs the effects phase (cell admission and final restoration) and ProbeSlot. Native on candidate 3: 223809Z and 224949Z effects 35/36 each with every new isolation check passing; Ghaele 224649Z lifecycle/visibility PASS. Retained FAILs: s7-anky-callback-loss-characterized in 223809Z (no cue overlap in the declared search) and in 224949Z, whose collector ran its whole live path but whose in-run verdict misread String-typed null fields in memory; the same unchanged evaluator over the recorded row characterizes. That evaluator defect needs a fourth candidate to pass in game and is reported as the remaining limitation of R6-F4, not hidden. R4-F5 stays accepted (round 6); the 164749Z cause stays unknown. Validation 65m42s of 75 (round 7: 25m58s plus 40.1s of required gates). Owner-only: visual T2/T3/T6, listening T5/T7 (private packet r5), merge/release. Controller rendering and the released outdoor Ghaele unexecuted; October incident unattributed.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint7-visual | yes | pass | 3.7s | 22 tests on H. Includes effects-cell-sequence-isolation (R4-F1/R6-F2 through the phase Sequence: disposal, live/unreadable/failed-release refusal, recovery never erasing a failure, hook case and zero-hook control), ghaele-early-release-failures-reach-slot (R6-F1 through ProbeSlot/Poll/Release/unwind), impact-cue-playback-verdicts (one settled evaluator over the retained 202648Z/200320Z rows, terminal-read mutations, the round-7 rows and the in-memory null-typing reproduction), ghaele-lighting-and-loan (R5-F3 producer-to-verdict for missing/hidden/visible; R5-F1 witness correlation) and ghaele-attribution (retained lighting rows: fader-presence-unrecorded). |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r3 | yes | pass | 3.4s | 5 tests on H. R3 Trample/Overrun rows unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r4 | yes | pass | 3.3s | 4 tests on H. Mephit breath identities unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r6 | yes | pass | 3.3s | 11 tests on H. Cast cue lifecycle. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r7 | yes | pass | 3.6s | 2 tests on H. R7 loader contracts. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint22- | yes | pass | 3.7s | 11 tests on H. Ankylosaurus tail control and impact bound. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint5- | yes | pass | 3.5s | 4 tests on H. Sprint 5 mephit pack. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter salamander | yes | pass | 3.4s | 42 tests on H. Salamander contracts. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter native-admission | yes | pass | 3.1s | 3 tests on H. Existing review-phase admission. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter native-profile | yes | pass | 3.4s | 9 tests on H. Native profile policy. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter loader-compat-probe | yes | pass | 3.2s | 5 tests on H. Harness probe lifecycle. |
| Build-LoaderCompatHarness.ps1 (round-7 harness source; twice: uncommitted, then the 9c39b1ff7f source) | no | pass | 1.2s + 1.3s | Harness ee7742a493eb3879cffa30c69879592a51da60a3b7205a902832d8e4395ea373 (built from the 9c39b1ff7f source). Counted in the owner allowance. |
| agent_review.py verify --gate focused (uncommitted tree) | no | fail | 1.7s | Refused: gates run on a committed candidate. Counted in the owner allowance. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, three runs) | no | pass | 20.8s + 1.7s + 5.5s | 22 tests each; the third after the test-only round-7 row cases. Counted in the owner allowance. |
| Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual (candidate 3, 9c39b1ff7f) | no | pass | 7m2s | Package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413, DLL 5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3. Third and last authorized candidate (owner amendment 3). No product change outside RuntimeTesting/; RuntimeTesting is unchanged from 9c39b1ff7f to H (records and tests only after it). |
| Invoke-LoaderCompatSession.ps1 s7-candidate-effects.json on 9c39b1ff7f (harness ee7742a4) | no | fail | 8m0s | Session 20261124T223809Z, retained: 35 of 36 structured checks, r6 16/16. Passed: s7-anky-callback-abandoned-unwound, s7-dust-abandoned-unwound, s7-dust-early-exit-released, s7-dust-cell-conditions-released, s7-anky-concurrent-retirement, s7-effects-owned-state-restored (phase isolation restored; all five cells admitted). FAIL: s7-anky-callback-loss-characterized, not-both-playing (no two cues confirmed playing at once within the declared 16 s search; 607 ended 13.6043, 641 posted 13.6023). |
| Invoke-LoaderCompatSession.ps1 s7-ghaele.json (lifecycle, visibility; harness ee7742a4) on 9c39b1ff7f | no | pass | 2m15s | Session 20261124T224649Z: lifecycle incl. the post-disposal late-result negative control (late-owned-summon, ordered, ability-data match, unit owned and destroyed, witness unsubscribed) and the positive pending case (null); visibility: Ghaele present-visible valid, donor present-hidden fader-hidden. The full 120 s released/current lifetimes were not repeated (owner amendment 3). |
| Invoke-LoaderCompatSession.ps1 s7-candidate-effects.json on 9c39b1ff7f (one rerun, same package) | no | fail | 8m9s | Session 20261124T224949Z, retained: 35 of 36, r6 16/16, same isolation passes. The callback-loss collector reached overlap and ran its whole live path; its in-run verdict survivor-stopped-by-other-retirement is an evaluator defect (in-memory null strings are String-typed JValues, which the null test misreads); the same evaluator over the recorded row gives characterized. Rerun chosen once because 223809Z never reached the collector's live path; recorded as such. |

**Evidence references:**
- prepared under owner amendments revision 3 sha256 3321dd3192f263d133d0589e9a5c7cb993a05f56835e65878ce9ce9947fc44e0
- docs/GUNSLINGER-SPRINT7-RECORD.md
- docs/evidence/sprint7/r7/MANIFEST.json
- docs/evidence/sprint7/r6/MANIFEST.json
- docs/evidence/sprint7/r5/MANIFEST.json
- docs/evidence/sprint7/r4/MANIFEST.json
- evidence/sprint7-review/INDEX.json
- docs/evidence/sprint7/audio-context-r4.json
- planning/GUNSLINGER-REMEDIATION-STATUS.md
- KNOWN-ISSUES.md

**Prior findings to assess:** `R4-F1`, `R5-F1`, `R5-F3`, `R6-F1`, `R6-F2`, `R6-F3`, `R6-F4`, `R6-F5`

**Owner amendments to the mission — revision 3, captured-source sha256 `3321dd3192f263d133d0589e9a5c7cb993a05f56835e65878ce9ce9947fc44e0`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

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

### R6-F4 — qualify the collector actually delivered

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
- Update any active Trumpet wording that still suggests seeded or forced dice: the accepted current probe applies `AutoPass` to its

---

<!-- agent-review:v1 enrollment=kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 key=r07-request part=2/2 sha256=2d957b0e6fdeb5fb206a231f8186fb4cbaf844883067a3f232c8c701cda67e87 -->
_Part 2 of 2._

 correlated save while leaving dice native.
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
````

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
