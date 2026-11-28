<!-- agent-review:v1 enrollment=kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 key=r06-request part=1/1 sha256=41b60ef54f931374424d3c19c2de8e833e31348055c336308e6b6f2efe90f36d -->
### Claude Opus — review request — round 6 — candidate `b37faaf883`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1-r06-2e9004` |
| Checkpoint | `final` (final) |
| Candidate H | `b37faaf883cf71d086a1fb140c3ea7eb3c98966a` |
| Target B (`master`) | `35763430acc9eefc306b861045526a444690bc77` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `sprint7-autonomous-mission` sha256 `c232f135ef` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 2, sha256 `26409cd879f04212588a92b4182df1defcc0380bb25b8b00c7a97ee4747e3ab4` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 6 of 7 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 review round 6 of 7 on H=b37faaf883, prepared under owner amendment 2 (revision 2; cap extended to seven by the owner). All eleven findings open after round 5 addressed; matrix in record section 7. Two record reconciliations (R1-F1, R5-F6), eight observer/harness corrections (R2-F2, R4-F1, R4-F2, R5-F1..R5-F5), and R4-F5 given the conditional owner disposition (amendment 2 section 4) on a synthetic callback-loss characterization; no product change outside RuntimeTesting since 091446a079. Native: Trumpet 201133Z and Ghaele 201333Z PASS on candidate 1; effects 200320Z and 202648Z 33/34 + r6 16/16 with s7-anky-callback-loss-characterized FAIL in-run (observer selection, then observer timing), both retained; the 202648Z records re-evaluated offline characterize the disposition; the head's settled read is not natively exercised (both authorized candidates used). Validation 39 min 4 s of the 60-minute exception (38 min 25 s before the 39 s controller gates). Owner-only: visual T2/T3/T6, listening T5/T7 (private packet r5), merge/release. Controller rendering unexecuted; released outdoor/bright Ghaele unexecuted; October incident unattributed.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint7-visual | yes | pass | 4s | 20 tests. Includes trumpet-owned-save, probe-slot-destroys-each-unit-once (real ProbeSlot -> Release -> iterator finally -> remaining resources with deferred destruction), impact-cue-playback-verdicts (with the offline re-evaluation of the retained round-6 records), ghaele-attribution (R5-F2 negatives; complete retained 182757Z/183206Z datasets re-evaluated) and ghaele-lighting-and-loan (R5-F3 negatives, pending and combat verdicts). |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r3 | yes | pass | 3s | 5 tests. R3 Trample/Overrun rows unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r4 | yes | pass | 3s | 4 tests. Mephit breath identities unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r6 | yes | pass | 4s | 11 tests. Cast cue lifecycle. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r7 | yes | pass | 4s | 2 tests. R7 loader contracts. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint22- | yes | pass | 4s | 11 tests. Ankylosaurus tail control and impact bound. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint5- | yes | pass | 3s | 4 tests. Sprint 5 mephit pack. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter salamander | yes | pass | 4s | 42 tests. Salamander contracts. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter native-admission | yes | pass | 3s | 3 tests. Existing review-phase admission. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter native-profile | yes | pass | 3s | 9 tests. Native profile policy. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter loader-compat-probe | yes | pass | 4s | 5 tests. Harness probe lifecycle. |
| Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual (candidate 1, f3bfab1303) | no | pass | 7m23s | Package a6ac5d79f2d794ea5f9707c003885160c0392b0f6f1036b49e806de3a59d0017, DLL 270f7aa4d897274808c4523e654a0c468f354cac37c250f60a30eefc7975c355. Harness 9240a3d633a801c86775eb678d7be2818190e7f10cb9b5e39e8485f62cdb08a0 (built from f3bfab1303; one failed 2 s compile before it). No product change outside RuntimeTesting/ since 091446a079. |
| Invoke-LoaderCompatSession.ps1 s7-candidate-effects.json on f3bfab1303 | no | fail | 8m12s | Session 20261124T200320Z, retained: 33 of 34 structured checks pass (s7-dust-abandoned-unwound, s7-dust-early-exit-released, s7-dust-cell-conditions-released, s7-anky-concurrent-retirement with the per-ID trace verdict, all others), r6 16/16; s7-anky-callback-loss-characterized FAIL (observer: latched a cue that had already ended; not-both-playing). |
| Invoke-LoaderCompatSession.ps1 s7-trumpet-reload.json (harness 9240a3d6) on f3bfab1303 | no | pass | 2m1s | Session 20261124T201133Z: foreign d20 dispatched directly inside the owned save before its native roll 8 = control 8 (not pre-rolled); owned die 17 = control 17, passed through AutoPass; following draw = control; 49 owned saves passed with natural dice; interleave untouched; reload (160 natural, no replay, fresh 324); abandonment with one destruction request. |
| Invoke-LoaderCompatSession.ps1 s7-ghaele.json (harness 9240a3d6) on f3bfab1303 | no | pass | 4m11s | Session 20261124T201333Z: pending abandonment (command pending and loan held immediately before disposal, separate witness saw no owned/unattributed summon, foreign kept, witness unsubscribed; negative control on the real owned spawn: late-owned-summon); combat (no LeaveCombat, owned attacks left 0, target one request, independent control preserved in combat, no unowned engagement, witness held then settled, native exit 3.04 s after control release); attribution 11/11; lighting rows valid with faderVisible. Released package not re-run (pure reader changes re-evaluated on retained 182757Z/183206Z). |
| Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual (candidate 2, 740d94a995) | no | pass | 7m15s | Package e0eda5b9ee7bde2b6c6bacb5f6167dc531cc0c470091182665fc5b8b0c55b7a7, DLL f145ed84917be449c1ca994b838644f0a270d262a69553dc15f786535c9a36cb. Changes only the callback-loss selection in RuntimeTesting. Second and last authorized candidate. |
| Invoke-LoaderCompatSession.ps1 s7-candidate-effects.json on 740d94a995 | no | fail | 8m16s | Session 20261124T202648Z, retained: 33 of 34 structured checks pass, r6 16/16; s7-anky-callback-loss-characterized FAIL in-run (observer: read the other source stop in the retirement frame; other-not-stopped-by-own-id). Its retained records re-evaluated offline with the settled predicate: characterized (record 4.5). |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, four runs) | no | pass | 18s (compile fail) + 4s + 17s + 24s | Counted in the owner validation allowance. |

**Evidence references:**
- prepared under owner amendments revision 2 sha256 26409cd879f04212588a92b4182df1defcc0380bb25b8b00c7a97ee4747e3ab4
- docs/GUNSLINGER-SPRINT7-RECORD.md
- docs/evidence/sprint7/r6/MANIFEST.json
- docs/evidence/sprint7/r5/MANIFEST.json
- docs/evidence/sprint7/r4/MANIFEST.json
- evidence/sprint7-review/INDEX.json
- docs/evidence/sprint7/audio-context-r4.json
- planning/GUNSLINGER-REMEDIATION-STATUS.md
- KNOWN-ISSUES.md

**Prior findings to assess:** `R1-F1`, `R2-F2`, `R4-F1`, `R4-F2`, `R4-F5`, `R5-F1`, `R5-F2`, `R5-F3`, `R5-F4`, `R5-F5`, `R5-F6`

**Owner amendments to the mission — revision 2, captured-source sha256 `26409cd879f04212588a92b4182df1defcc0380bb25b8b00c7a97ee4747e3ab4`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

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
````

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
