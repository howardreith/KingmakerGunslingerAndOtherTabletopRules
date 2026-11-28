<!-- agent-review:v1 enrollment=kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 key=r04-request part=1/1 sha256=bec05e3b4942ede1f89a3b1337b02fcce79f8cfeef442f94396746d3deb15717 -->
### Claude Opus — review request — round 4 — candidate `6095fa4a75`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1-r04-9232c7` |
| Checkpoint | `final` (final) |
| Candidate H | `6095fa4a750f92223550ef014a6759ad63678dbf` |
| Target B (`master`) | `35763430acc9eefc306b861045526a444690bc77` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `sprint7-autonomous-mission` sha256 `c232f135ef` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 1, sha256 `59be8e3f36003b7dbf52994ac43a695ee5292aad949f820f01d391f3052031e0` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 4 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 review round 4 on H=6095fa4a75, prepared under owner amendment 1 (revision 1). All twelve pending findings addressed; matrix in record section 7. Product change: Dust tint adoption bound to CreateView's own cast-effect call (transpiler), tracker removed. Everything else is observer, harness, test and record work. Native: bc67438f3b fx 31/31 + r6 16/16 with loopback audio; a07716e3ab models 2/2, Trumpet reload PASS, Ghaele lifecycle and timeline with combat and lighting PASS on released v0.0.195 and the candidate. Retained failures with causes: 154136Z, 154929Z, 155639Z, 155845Z, 161344Z, 164749Z (incl. one lost native end callback for a surviving Ankylosaurus cue, cause unknown). Private listening packet delivered in the lab (no agent listening; choices provisional). Controller mode unexecuted (precise first unsupported operation). Released outdoor/bright Ghaele has no admitted route. October incident unattributed. Product code outside RuntimeTesting/ identical from 1ac4324617 to H. Owner-only judgments in record section 6.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint7-visual | yes | pass | 3.1s | 16 tests. 16 tests: VIS-01/02 decoded exports; VIS-03 identities/text; VIS-04 qualification; VIS-05 identity, tint and call-site ownership through the reviewer interval (inner owned creation interrupted, foreign same-prefab same-point spawn before the enclosing call returns, later owned spawn tinted once); admission and screen capture; observer ownership; Trumpet reload verdict and abandonment; cleanup failure reaching ProbeSlot with a yielded probe and injected failing restore; blast-owned roll scope with interleaved unrelated draws and abandonment; positive-only Ghaele attribution with negative cases and re-evaluation of the retained round-3 rows (11/11 positive each, body identity not recorded); condition ledger, trace release verdict and party-capture verdict. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r3 | yes | pass | 2.0s | 5 tests. R3 Trample/Overrun rows unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r4 | yes | pass | 1.9s | 4 tests. Mephit breath identities and shared mapping unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r6 | yes | pass | 1.8s | 11 tests. Cast cue lifecycle (R6RunPassive now records cue-post listener distance). |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter summon-playtest-r7 | yes | pass | 2.1s | 2 tests. R7 loader contracts on the repaired export. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint22- | yes | pass | 2.7s | 11 tests. Ankylosaurus tail control and impact bound. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter sprint5- | yes | pass | 2.4s | 4 tests. Sprint 5 mephit pack. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter salamander | yes | pass | 2.3s | 42 tests. Salamander contracts with the new export. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter native-admission | yes | pass | 2.2s | 3 tests. Existing review-phase admission. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter native-profile | yes | pass | 2.2s | 9 tests. Native profile policy. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1 --gate focused -- -Filter loader-compat-probe | yes | pass | 2.1s | 5 tests. Harness probe lifecycle (ProbeSlot, CleanupReport). |
| Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual (1ac4324617, a07716e3ab, 2e2737f8c6, bc67438f3b) | no | pass | 7-8 min each | Strict package validation, no deployment. Final bc67438f3b: package 7adde59cce585da3543a849c010ba186dbbca3407c8a67bfb27df6c5f0d2d894, DLL aeb633f7d82ecb7d81a188dd80da98a2ea279723d958b8b7ad41939c04829fd2; a07716e3ab: 4dc92312.../c26b7aa0...; product code outside RuntimeTesting/ identical from 1ac4324617 to H. Harness 71b0c6f03de48fce27d3e6d8ef384769b524e09b3c47f21b740b1bfdb7475701. |
| Invoke-LoaderCompatSession.ps1 s7-candidate-effects.json on bc67438f3b with loopback audio | no | pass | 7m16s | Session 20261124T170720Z: fx 31/31 (incl. s7-dust-nested-interruption, s7-dust-early-exit-released, s7-dust-cell-conditions-released, s7-dust-trace-released, s7-effects-owned-state-restored), r6 16/16. Retained failures with observer causes: 20261124T154136Z, 20261124T161344Z, 20261124T164749Z (record section 3). |
| Invoke-LoaderCompatSession.ps1 s7-models.json on a07716e3ab | no | pass | 4m56s | Session 20261124T162110Z: models bright (incl. s7-observer-rollback with the failed normal disposal) and dim; party views valid on all 8 states. Retained failure 20261124T154929Z (observer). |
| Invoke-LoaderCompatSession.ps1 s7-trumpet-reload.json (harness 71b0c6f0) on a07716e3ab | no | pass | 2m13s | Session 20261124T162605Z: reload (saved 159 natural, no replay, fresh 323), blast-owned seeding (25 saves), interleaved unrelated save/draw equal to control, no rewind, abandonment (expected step timeout) and abandon-check. |
| Invoke-LoaderCompatSession.ps1 s7-ghaele.json (harness 71b0c6f0) on released v0.0.195 and on a07716e3ab | no | pass | 4m16s / 4m19s | Sessions 20261124T162819Z (released 456d504d...) and 20261124T163235Z: lifecycle (borrowed non-default, replaced grant, abandoned pending, abandoned after spawn, bystander command) and timeline with combat (3 hits) and idle/combat lighting; attribution 11/11 positive with body identity. Retained failures 20261124T155639Z/155845Z (observer bystander read). |
| Offline reproductions before repair (sprint7-visual temporary cases on the round-3 code) | no | fail | seconds | Expected failures reproducing the defects, not gates: R1-F9 foreignTints=1 on the round-3 tracker; R3-F2 isolationRestored=true after a failed unsubscribe with the round-3 Release pattern. Lab files runtime-evidence/s7/r4-offline/*-before-repair.txt. |
| Private listening packet build and verification (make_packet.py; Windows SoundPlayer.Load) | no | pass | seconds | 18 clips copied with matching SHA-256, decoded as 16-bit PCM, loaded by the Windows WAV decoder, every index/playlist link resolves, zip testzip clean (sha256 b2fddccd...17d0). Browser playback not exercised. No listening performed by any agent. |

**Evidence references:**
- prepared under owner amendments revision 1 sha256 59be8e3f36003b7dbf52994ac43a695ee5292aad949f820f01d391f3052031e0
- docs/GUNSLINGER-SPRINT7-RECORD.md
- evidence/sprint7-review/INDEX.json
- docs/evidence/sprint7/r4/MANIFEST.json
- docs/evidence/sprint7/audio-context-r4.json
- docs/evidence/sprint7/audio-context.json
- docs/evidence/sprint7/exports.json
- planning/GUNSLINGER-REMEDIATION-STATUS.md
- KNOWN-ISSUES.md

**Prior findings to assess:** `R1-F1`, `R1-F3`, `R1-F5`, `R1-F9`, `R2-F2`, `R2-F4`, `R3-F1`, `R3-F2`, `R3-F3`, `R3-F4`, `R3-F5`, `R3-F6`

**Owner amendments to the mission — revision 1, captured-source sha256 `59be8e3f36003b7dbf52994ac43a695ee5292aad949f820f01d391f3052031e0`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

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
````

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
