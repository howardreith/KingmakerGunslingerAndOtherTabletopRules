<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 key=r04-review part=1/1 sha256=d24efc3497c2ee5aca964bda2bdf3f9b94056a7419fcacd8beecc071940f4c0d -->
### Codex GPT-6.2 Astra — review — round 4 — candidate `2f975db075`

**Decision:** `changes_required` — All seven pending findings have verified fixes, and all four Focused gates passed on the candidate. C1 still characterizes a cue that completed before its first stop attempt and a window containing samples the collector could never have taken. The remaining findings concern the same evaluator’s treatment of recorded times, stop causes, counts and missing evidence.

## Continuation for Claude

Preserve the four-review stop on the existing C1 enrollment. Further C1 work requires owner direction. If authorized, reconcile the retirement-frame observations with the product’s skip-on-completion rule, apply only the time bounds each collector generation establishes, and distinguish an impossible or unavailable observation from an observed stop. The structured findings give the controls and expected outcomes. Exercise them through the existing Focused domain case using both 224949Z representations and a Round6-specific control. Preserve the historical verdicts, frozen parent and shared ledger. C2 requires a stable C1 handoff. C3 owns native execution of the changed evaluator.

#### Findings

**R4-F1** · P1 · defect · blocks readiness — Reject a first stop attributed after the cue already completed
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:323`; acceptance: R7-F6
- Trigger: On a 224949Z copy, copy cue 622’s settled endUtc into afterOtherRetired.other.endUtc. Keep the retirement-frame stop fields null, otherStopAttempts at zero and the settled record unchanged.
- Expected: Once a frame has observed EndUtc with zero attempts, SummonCastCueLifecycle.Retire skips that posting. Its attempt count cannot subsequently rise to one. Any stop or end already present in the retirement frame must also precede or coincide with the first subsequent sample timestamp.
- Actual: Advances accepts the matching end, and StopAgrees accepts the zero-attempt frame and the later successful attempt independently. RetiredByOwnId then credits the settled stop. The row characterizes. The same path also accepts a stop already present in the retirement frame at 22:51:56.4333170Z although the subsequent first window sample was stamped at 22:51:56.4303174Z.
- Why it blocks / does not: The evaluator can credit retirement with stopping a cue that the product would skip as already complete.
- Requested correction: Reconcile the retirement-frame completion with the change in attempt count and with the first subsequent sample. Preserve a legitimate completion following a stop that was already attempted.
- Requested evidence: On both 224949Z shapes, refuse an end recorded at zero attempts followed by a settled first stop. Also refuse a complete retirement-frame stop whose timestamp is later than the first sample. Retain the original positive row.

**R4-F2** · P1 · defect · blocks readiness — Reject samples that require re-entering an expired window
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:400`; acceptance: R7-F1, R7-F6
- Trigger: Keep the 224949Z retirement time and 12-second duration. Set the penultimate and final sample timestamps to 22:52:08.4300000Z and 22:52:08.4500000Z. Both follow the 22:52:08.4293166Z deadline.
- Expected: A sample timestamp can straddle its own loop guard. Once that timestamp is past the deadline, the collector cannot enter another iteration and emit a further sample.
- Actual: The sample loop checks time order and the retirement lower bound. Both altered samples pass, remain before the disposal stop at 22:52:08.4634895Z and contribute to a characterized verdict.
- Why it blocks / does not: An impossible sequence of periodic reads can establish playback behavior within the declared observation window.
- Requested correction: Use the Round7 deadline to reject a later iteration when a preceding sample was already stamped past that deadline. Preserve the separate timing of the final sample and terminal callback list.
- Requested evidence: Exercise the two late samples on both 224949Z shapes. Retain a control where only the final sample straddles its guard and the existing control for a callback first seen in the terminal list.

**R4-F3** · P2 · defect · blocks readiness — Keep the Round6 timeline mark distinct from the loop start
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:311`; acceptance: R7-F6
- Trigger: Construct a Round6Second sequence with local retiredAt at 20:29:02.3250000Z, its subsequent phase mark at 20:29:02.3261873Z, and disposal after the 12-second loop with a stop at 20:29:14.3258000Z. These are example producer timings, not a claim about the unrecorded start of 202648Z.
- Expected: Round6 calculates its deadline from retiredAt, which it reads before emitting the timeline mark. A stop can follow the real deadline while preceding mark plus duration.
- Actual: Settled adds the declared duration to the supplied Round6 phase mark and uses that value as a proven lower bound. It returns survivor-stop-inside-zero-stop-interval for the constructed valid ordering.
- Why it blocks / does not: This introduces a false refusal for a supported historical generation by assigning the exact loop-start meaning to a different clock read.
- Requested correction: Use the exact retirement-plus-duration bound for Round7. For Round6, use bounds established by its recorded postings and samples and preserve uncertainty about the unrecorded loop start.
- Requested evidence: Add a Round6Second control with distinguishable local-start and phase-mark reads and a disposal stop between their derived deadlines. Preserve the recorded 202648Z and 200320Z results and the Round7 inside-window refusal.

**R4-F4** · P2 · defect · blocks readiness — Do not diagnose reversed observation order from equal UTC readings
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:454`; acceptance: R7-F1, R7-F6
- Trigger: Construct a collector-shaped row with zero periodic callback counts and a terminal callback, survivor retirement mark and later disposal request all stamped in the same clock tick after the window.
- Expected: The collector copies the callback list before disposal. Two ordered reads can share a UTC timestamp. The retained 224949Z row itself shows identical times for the separately read retirement stamp and timeline mark.
- Actual: The comparison at line 454 reports callback-after-terminal-capture on equality. The adjacent disposal check likewise treats equality with the earliest possible zero-count read as proof that the stop happened inside the interval.
- Why it blocks / does not: The evaluator requires a positive clock gap between ordered operations that can share a timestamp and reports a contradiction the readings do not establish.
- Requested correction: Use the known observation order when timestamps tie. Reserve a reversed-order verdict for a strict inversion and identify uncertainty wherever the available observations cannot settle the order.
- Requested evidence: Exercise a terminal callback and subsequent disposal stop sharing a clock tick on both row shapes, alongside the strictly post-disposal callback and strictly inside-window stop controls.

**R4-F5** · P2 · defect · blocks readiness — Apply the one-attempt limit to the pre-disposal survivor counts
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:340`; acceptance: R7-F6
- Trigger: Set afterOtherRetired.survivorStopAttempts to 2 on a 224949Z copy. Independently set otherRetirementSettled.survivorStopAttemptsThroughWindow to 2.
- Expected: Retire skips a record after its first attempt. A cumulative count of two for one selected posting is inconsistent wherever the collector reads it.
- Actual: Both pre-disposal branches report survivor-stopped-by-other-retirement for every positive Int32. The same count on survivorAfterDisposal is correctly diagnosed as stop-attempts-impossible.
- Why it blocks / does not: The recorded verdict attributes a stop to the other retirement using a count the producer cannot emit for that posting.
- Requested correction: Apply the per-posting one-attempt limit before interpreting either pre-disposal survivor count as evidence of a stop.
- Requested evidence: Exercise counts of zero, one and two at both pre-disposal locations on the two row shapes. Preserve the existing negative and oversized count controls.

**R4-F6** · P2 · defect · blocks readiness — Keep an unavailable settled completion distinct from a recorded null
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:334`; acceptance: R7-F1, R7-F6
- Trigger: Remove otherRetirementSettled.other.endUtc from 224949Z, replace it with a non-date value on a second copy, and compare both with the existing explicit-null control.
- Expected: An explicitly recorded null establishes that no completion had been recorded. An omitted or malformed settled end leaves the completion observation unavailable and must retain that distinction in the verdict.
- Actual: When the retirement-frame end was null, Advances skips the later field. Utc then maps all three settled inputs to the same nullable value, and RetiredByOwnId produces other-not-stopped-by-own-id for each.
- Why it blocks / does not: The current reader loses the required distinction between an observation of no completion and unavailable completion evidence on the cue whose retirement it judges.
- Requested correction: Read the required settled end with the same missing, unreadable and explicit-null handling used for the other lifecycle snapshots before evaluating the successful stop and completion timestamps.
- Requested evidence: Compare omitted, malformed, explicitly null and valid settled endUtc on both 224949Z shapes. Preserve the separate observation that the stop request succeeded.

**R4-F7** · P2 · defect · blocks readiness — Check the recorded cause before crediting a stop to retirement
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:549`; acceptance: R7-F6
- Trigger: Change only otherRetirementSettled.other.stopReason on 224949Z from view-destroyed to an empty string or manual-preview-stop.
- Expected: The production playback component supplies entity-retired or view-destroyed when it calls Retire. An empty or explicitly unrelated cause supplies no support for attributing the request to that retirement.
- Actual: StopAgrees tests whether the reason field is set. Unset treats both strings as set, so the settled stop is accepted and the row characterizes.
- Why it blocks / does not: The evaluator can credit source retirement even when the consumed stop record explicitly gives a different cause.
- Requested correction: Validate the recorded cause when crediting a successful request to source retirement. Preserve both production retirement reasons and the separate failed-call outcome.
- Requested evidence: On both 224949Z shapes, refuse a settled request with an empty or explicitly unrelated cause. Retain valid source-retirement and failed-call controls.

#### Prior finding assessments

- **R2-F3** — `fix_verified` — Round7 now uses its recorded retirement time and declared duration to protect the gap after the final sample. The passing two-shape regression refuses the 22:52:08.3500000Z disposal stop, accepts the actual 08.4634895Z disposal stop and permits a terminal callback at 08.3500000Z. R4-F3 concerns the separate Round6 timestamp source.
- **R3-F1** — `fix_verified` — The tests reject the callback before cue 622 posted, the strictly post-disposal callback, retirement before the selected posting and a sample before retirement. Source inspection confirms the distinct Round6First arming order. R4-F4 concerns the newly introduced equality case.
- **R3-F2** — `fix_verified` — StopAgrees reconciles a recorded request and its same-frame count at both reported locations. The tests refuse a successful request with zero attempts and distinguish a failed call with no request timestamp. R4-F1 concerns a transition between two individually consistent snapshots.
- **R3-F3** — `fix_verified` — Utc rejects the time-only string, a zone-less collector string and an Unspecified DateTime. The passing assertions recognize the complete round-trip UTC string and its reparsed date token as the same instant.
- **R3-F4** — `fix_verified` — SourceRecord now validates the immutable posting fields of positive entries before filtering by ID. Posting rejects an empty event. The two-shape tests cover the incomplete 513 record, the empty selected event and a legitimate unrelated ID-zero post.
- **R3-F5** — `fix_verified` — The candidate-bound sprint7-visual log names the run, historical verdict, historical evaluator or its absence, current reader and current result for both generations.
- **R3-F6** — `fix_verified` — The manifest leads with the amended four-review allowance and labels the prior 2/2 stop as historical. It retains the stop reason, branch head bb670b13f4 and reviewed head e4c9ed9c5e.

#### Evidence assessment

- **Inspected:** Verified the detached HEAD, merge base, mission hash and OWNER AMENDMENT 1 hash against the fixed inputs. Read the repository instructions, full feature diff, round-four correction, developer report, transcript notice and prior-findings ledger.; Compared Settled, CueRow and their callers with S7CallbackLoss, SummonCastCuePlayback and SummonCastCueLifecycle.Retire. Checked the historical collectors at f3bfab1303 and 740d94a995 against their retained rows.; Checked the recovery manifest, frozen parent record, planning row and historical gate extract. The extract totals 118 selections and 19.1s inside receipts. The records distinguish the 40.1s wrapper batch, 65m42s parent expenditure and qualifying harness suffix a373.
- **Executed verification:** I verified the four controller log hashes against their receipts. Each receipt identifies the unchanged candidate 2f975db0754d94cf12ddc289c76363a67dde3148, successful production compilation and exit code 0. sprint7-visual passed 22 tests in 2.3s, summon-playtest-r6 passed 11 in 2.1s, sprint22- passed 11 in 2.4s and native-profile passed 9 in 1.9s. That is 53 test executions and 8.7s inside the receipts.; The sprint7-visual log records PASS for impact-cue-playback-verdicts. Its assertions cover the pending correction controls on the persisted row and the live counterpart built through CueRow with direct String-typed nulls.; The log identifies both the historical verdict and current reader for all four runs. CallbackLossRound6 characterizes 202648Z and returns not-both-playing for 200320Z. CallbackLossRound7 characterizes 224949Z and returns not-both-playing for 223809Z. The recorded 224949Z verdict remains survivor-stopped-by-other-retirement.
- **Source inspection only:** Traced the new controls through the candidate’s actual verdict branches using the retained posting IDs, timestamps and counts. Compared the claimed transitions with the product’s retirement guard and the collector’s loop and snapshot order.; Verified from historical source that both Round6 collectors read a local retirement time, then emit a separately timed phase mark, and calculate the loop deadline from the local value.
- **Required gaps:** The retirement verdict must reconcile an earlier completion with a later first attempt, reject a conflicting stop cause and distinguish a missing settled completion from an observed null.; Window and disposal checks must honor the order the loop can produce, the timestamp each generation recorded and the possibility of equal UTC readings.; The one-attempt limit must apply before a survivor count is reported as an observed stop. Focused behavioral evidence for the new controls is outstanding.
- **Limitations:** The new counterexamples are source traces. I did not execute them or run a build in the read-only snapshot.; The controller withheld the mixed foreground transcript. The four committed-candidate gate results are independently corroborated. The local attempts and wrapper wall times come from the developer ledger and report, which report 3m47.3s spent, 41m12.7s remaining and zero of two recovery product candidates used.; C1 covers implementation and domain behavior. Its changed evaluator has no new in-game observation or runtime restoration transaction. C3 owns the combined native check.

#### Summary

- **Delivered behavior:** The production collector and Round7 reader share an entry point. The retained 224949Z row and its collector-shaped live counterpart characterize in domain tests, and the historical in-game FAIL is preserved. All seven prior corrections are verified. The new findings prevent a C1 handoff.
- **Scope reviewed:** Final C1 implementation and domain review for parent R7-F1 and R7-F6, plus the R6-F5 records disposition, against the original mission and OWNER AMENDMENT 1.
- **Pending later-phase work:** C2’s Ghaele ownership repair and short external-harness qualification depend on a stable C1 handoff.; C3 owns effects isolation and combined native qualification, including live and persisted agreement on the same identified run.
- **Unresolved questions:** none
- **Remaining owner review:** OWNER AMENDMENT 1 authorized four C1 reviews. The fourth has been used. Any further C1 authority is an owner decision.; T2/T3/T6 visual acceptance, T5/T7 listening choices and eventual integration of the dependent stack with frozen PR #219 remain owner decisions.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: revision 1, sha256 `dd15fc443f359318f58ec5308b471656fb45cddc078da65ca77818a9979e6ebb`.

Codex CLI `0.170.0`, thread `01a21374-3b31-7530-9f98-d65f4b117cde`, duration 1029.5s, response sha256 `003c6f32d0d29f9f419ad5ff69300ee511b30570d3cb5c22ec2a0278931a001d`, request `kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34-r04-eb080a`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "2f975db0754d94cf12ddc289c76363a67dde3148",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "All seven pending findings have verified fixes, and all four Focused gates passed on the candidate. C1 still characterizes a cue that completed before its first stop attempt and a window containing samples the collector could never have taken. The remaining findings concern the same evaluator\u2019s treatment of recorded times, stop causes, counts and missing evidence.\n\n## Continuation for Claude\n\nPreserve the four-review stop on the existing C1 enrollment. Further C1 work requires owner direction. If authorized, reconcile the retirement-frame observations with the product\u2019s skip-on-completion rule, apply only the time bounds each collector generation establishes, and distinguish an impossible or unavailable observation from an observed stop. The structured findings give the controls and expected outcomes. Exercise them through the existing Focused domain case using both 224949Z representations and a Round6-specific control. Preserve the historical verdicts, frozen parent and shared ledger. C2 requires a stable C1 handoff. C3 owns native execution of the changed evaluator.",
  "evidence_assessment": {
    "executed_verification": [
      "I verified the four controller log hashes against their receipts. Each receipt identifies the unchanged candidate 2f975db0754d94cf12ddc289c76363a67dde3148, successful production compilation and exit code 0. sprint7-visual passed 22 tests in 2.3s, summon-playtest-r6 passed 11 in 2.1s, sprint22- passed 11 in 2.4s and native-profile passed 9 in 1.9s. That is 53 test executions and 8.7s inside the receipts.",
      "The sprint7-visual log records PASS for impact-cue-playback-verdicts. Its assertions cover the pending correction controls on the persisted row and the live counterpart built through CueRow with direct String-typed nulls.",
      "The log identifies both the historical verdict and current reader for all four runs. CallbackLossRound6 characterizes 202648Z and returns not-both-playing for 200320Z. CallbackLossRound7 characterizes 224949Z and returns not-both-playing for 223809Z. The recorded 224949Z verdict remains survivor-stopped-by-other-retirement."
    ],
    "inspected": [
      "Verified the detached HEAD, merge base, mission hash and OWNER AMENDMENT 1 hash against the fixed inputs. Read the repository instructions, full feature diff, round-four correction, developer report, transcript notice and prior-findings ledger.",
      "Compared Settled, CueRow and their callers with S7CallbackLoss, SummonCastCuePlayback and SummonCastCueLifecycle.Retire. Checked the historical collectors at f3bfab1303 and 740d94a995 against their retained rows.",
      "Checked the recovery manifest, frozen parent record, planning row and historical gate extract. The extract totals 118 selections and 19.1s inside receipts. The records distinguish the 40.1s wrapper batch, 65m42s parent expenditure and qualifying harness suffix a373."
    ],
    "limitations": [
      "The new counterexamples are source traces. I did not execute them or run a build in the read-only snapshot.",
      "The controller withheld the mixed foreground transcript. The four committed-candidate gate results are independently corroborated. The local attempts and wrapper wall times come from the developer ledger and report, which report 3m47.3s spent, 41m12.7s remaining and zero of two recovery product candidates used.",
      "C1 covers implementation and domain behavior. Its changed evaluator has no new in-game observation or runtime restoration transaction. C3 owns the combined native check."
    ],
    "required_gaps": [
      "The retirement verdict must reconcile an earlier completion with a later first attempt, reject a conflicting stop cause and distinguish a missing settled completion from an observed null.",
      "Window and disposal checks must honor the order the loop can produce, the timestamp each generation recorded and the possibility of equal UTC readings.",
      "The one-attempt limit must apply before a survivor count is reported as an observed stop. Focused behavioral evidence for the new controls is outstanding."
    ],
    "source_inspection_only": [
      "Traced the new controls through the candidate\u2019s actual verdict branches using the retained posting IDs, timestamps and counts. Compared the claimed transitions with the product\u2019s retirement guard and the collector\u2019s loop and snapshot order.",
      "Verified from historical source that both Round6 collectors read a local retirement time, then emit a separately timed phase mark, and calculate the loop deadline from the local value."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "R7-F6"
      ],
      "actual": "Advances accepts the matching end, and StopAgrees accepts the zero-attempt frame and the later successful attempt independently. RetiredByOwnId then credits the settled stop. The row characterizes. The same path also accepts a stop already present in the retirement frame at 22:51:56.4333170Z although the subsequent first window sample was stamped at 22:51:56.4303174Z.",
      "blocking_rationale": "The evaluator can credit retirement with stopping a cue that the product would skip as already complete.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Once a frame has observed EndUtc with zero attempts, SummonCastCueLifecycle.Retire skips that posting. Its attempt count cannot subsequently rise to one. Any stop or end already present in the retirement frame must also precede or coincide with the first subsequent sample timestamp.",
      "id": "R4-F1",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 323
      },
      "priority": "P1",
      "requested_correction": "Reconcile the retirement-frame completion with the change in attempt count and with the first subsequent sample. Preserve a legitimate completion following a stop that was already attempted.",
      "requested_evidence": "On both 224949Z shapes, refuse an end recorded at zero attempts followed by a settled first stop. Also refuse a complete retirement-frame stop whose timestamp is later than the first sample. Retain the original positive row.",
      "title": "Reject a first stop attributed after the cue already completed",
      "trigger": "On a 224949Z copy, copy cue 622\u2019s settled endUtc into afterOtherRetired.other.endUtc. Keep the retirement-frame stop fields null, otherStopAttempts at zero and the settled record unchanged."
    },
    {
      "acceptance_ids": [
        "R7-F1",
        "R7-F6"
      ],
      "actual": "The sample loop checks time order and the retirement lower bound. Both altered samples pass, remain before the disposal stop at 22:52:08.4634895Z and contribute to a characterized verdict.",
      "blocking_rationale": "An impossible sequence of periodic reads can establish playback behavior within the declared observation window.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A sample timestamp can straddle its own loop guard. Once that timestamp is past the deadline, the collector cannot enter another iteration and emit a further sample.",
      "id": "R4-F2",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 400
      },
      "priority": "P1",
      "requested_correction": "Use the Round7 deadline to reject a later iteration when a preceding sample was already stamped past that deadline. Preserve the separate timing of the final sample and terminal callback list.",
      "requested_evidence": "Exercise the two late samples on both 224949Z shapes. Retain a control where only the final sample straddles its guard and the existing control for a callback first seen in the terminal list.",
      "title": "Reject samples that require re-entering an expired window",
      "trigger": "Keep the 224949Z retirement time and 12-second duration. Set the penultimate and final sample timestamps to 22:52:08.4300000Z and 22:52:08.4500000Z. Both follow the 22:52:08.4293166Z deadline."
    },
    {
      "acceptance_ids": [
        "R7-F6"
      ],
      "actual": "Settled adds the declared duration to the supplied Round6 phase mark and uses that value as a proven lower bound. It returns survivor-stop-inside-zero-stop-interval for the constructed valid ordering.",
      "blocking_rationale": "This introduces a false refusal for a supported historical generation by assigning the exact loop-start meaning to a different clock read.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Round6 calculates its deadline from retiredAt, which it reads before emitting the timeline mark. A stop can follow the real deadline while preceding mark plus duration.",
      "id": "R4-F3",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 311
      },
      "priority": "P2",
      "requested_correction": "Use the exact retirement-plus-duration bound for Round7. For Round6, use bounds established by its recorded postings and samples and preserve uncertainty about the unrecorded loop start.",
      "requested_evidence": "Add a Round6Second control with distinguishable local-start and phase-mark reads and a disposal stop between their derived deadlines. Preserve the recorded 202648Z and 200320Z results and the Round7 inside-window refusal.",
      "title": "Keep the Round6 timeline mark distinct from the loop start",
      "trigger": "Construct a Round6Second sequence with local retiredAt at 20:29:02.3250000Z, its subsequent phase mark at 20:29:02.3261873Z, and disposal after the 12-second loop with a stop at 20:29:14.3258000Z. These are example producer timings, not a claim about the unrecorded start of 202648Z."
    },
    {
      "acceptance_ids": [
        "R7-F1",
        "R7-F6"
      ],
      "actual": "The comparison at line 454 reports callback-after-terminal-capture on equality. The adjacent disposal check likewise treats equality with the earliest possible zero-count read as proof that the stop happened inside the interval.",
      "blocking_rationale": "The evaluator requires a positive clock gap between ordered operations that can share a timestamp and reports a contradiction the readings do not establish.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The collector copies the callback list before disposal. Two ordered reads can share a UTC timestamp. The retained 224949Z row itself shows identical times for the separately read retirement stamp and timeline mark.",
      "id": "R4-F4",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 454
      },
      "priority": "P2",
      "requested_correction": "Use the known observation order when timestamps tie. Reserve a reversed-order verdict for a strict inversion and identify uncertainty wherever the available observations cannot settle the order.",
      "requested_evidence": "Exercise a terminal callback and subsequent disposal stop sharing a clock tick on both row shapes, alongside the strictly post-disposal callback and strictly inside-window stop controls.",
      "title": "Do not diagnose reversed observation order from equal UTC readings",
      "trigger": "Construct a collector-shaped row with zero periodic callback counts and a terminal callback, survivor retirement mark and later disposal request all stamped in the same clock tick after the window."
    },
    {
      "acceptance_ids": [
        "R7-F6"
      ],
      "actual": "Both pre-disposal branches report survivor-stopped-by-other-retirement for every positive Int32. The same count on survivorAfterDisposal is correctly diagnosed as stop-attempts-impossible.",
      "blocking_rationale": "The recorded verdict attributes a stop to the other retirement using a count the producer cannot emit for that posting.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Retire skips a record after its first attempt. A cumulative count of two for one selected posting is inconsistent wherever the collector reads it.",
      "id": "R4-F5",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 340
      },
      "priority": "P2",
      "requested_correction": "Apply the per-posting one-attempt limit before interpreting either pre-disposal survivor count as evidence of a stop.",
      "requested_evidence": "Exercise counts of zero, one and two at both pre-disposal locations on the two row shapes. Preserve the existing negative and oversized count controls.",
      "title": "Apply the one-attempt limit to the pre-disposal survivor counts",
      "trigger": "Set afterOtherRetired.survivorStopAttempts to 2 on a 224949Z copy. Independently set otherRetirementSettled.survivorStopAttemptsThroughWindow to 2."
    },
    {
      "acceptance_ids": [
        "R7-F1",
        "R7-F6"
      ],
      "actual": "When the retirement-frame end was null, Advances skips the later field. Utc then maps all three settled inputs to the same nullable value, and RetiredByOwnId produces other-not-stopped-by-own-id for each.",
      "blocking_rationale": "The current reader loses the required distinction between an observation of no completion and unavailable completion evidence on the cue whose retirement it judges.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An explicitly recorded null establishes that no completion had been recorded. An omitted or malformed settled end leaves the completion observation unavailable and must retain that distinction in the verdict.",
      "id": "R4-F6",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 334
      },
      "priority": "P2",
      "requested_correction": "Read the required settled end with the same missing, unreadable and explicit-null handling used for the other lifecycle snapshots before evaluating the successful stop and completion timestamps.",
      "requested_evidence": "Compare omitted, malformed, explicitly null and valid settled endUtc on both 224949Z shapes. Preserve the separate observation that the stop request succeeded.",
      "title": "Keep an unavailable settled completion distinct from a recorded null",
      "trigger": "Remove otherRetirementSettled.other.endUtc from 224949Z, replace it with a non-date value on a second copy, and compare both with the existing explicit-null control."
    },
    {
      "acceptance_ids": [
        "R7-F6"
      ],
      "actual": "StopAgrees tests whether the reason field is set. Unset treats both strings as set, so the settled stop is accepted and the row characterizes.",
      "blocking_rationale": "The evaluator can credit source retirement even when the consumed stop record explicitly gives a different cause.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The production playback component supplies entity-retired or view-destroyed when it calls Retire. An empty or explicitly unrelated cause supplies no support for attributing the request to that retirement.",
      "id": "R4-F7",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 549
      },
      "priority": "P2",
      "requested_correction": "Validate the recorded cause when crediting a successful request to source retirement. Preserve both production retirement reasons and the separate failed-call outcome.",
      "requested_evidence": "On both 224949Z shapes, refuse a settled request with an empty or explicitly unrelated cause. Retain valid source-retirement and failed-call controls.",
      "title": "Check the recorded cause before crediting a stop to retirement",
      "trigger": "Change only otherRetirementSettled.other.stopReason on 224949Z from view-destroyed to an empty string or manual-preview-stop."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F3",
      "rationale": "Round7 now uses its recorded retirement time and declared duration to protect the gap after the final sample. The passing two-shape regression refuses the 22:52:08.3500000Z disposal stop, accepts the actual 08.4634895Z disposal stop and permits a terminal callback at 08.3500000Z. R4-F3 concerns the separate Round6 timestamp source."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F1",
      "rationale": "The tests reject the callback before cue 622 posted, the strictly post-disposal callback, retirement before the selected posting and a sample before retirement. Source inspection confirms the distinct Round6First arming order. R4-F4 concerns the newly introduced equality case."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F2",
      "rationale": "StopAgrees reconciles a recorded request and its same-frame count at both reported locations. The tests refuse a successful request with zero attempts and distinguish a failed call with no request timestamp. R4-F1 concerns a transition between two individually consistent snapshots."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F3",
      "rationale": "Utc rejects the time-only string, a zone-less collector string and an Unspecified DateTime. The passing assertions recognize the complete round-trip UTC string and its reparsed date token as the same instant."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F4",
      "rationale": "SourceRecord now validates the immutable posting fields of positive entries before filtering by ID. Posting rejects an empty event. The two-shape tests cover the incomplete 513 record, the empty selected event and a legitimate unrelated ID-zero post."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F5",
      "rationale": "The candidate-bound sprint7-visual log names the run, historical verdict, historical evaluator or its absence, current reader and current result for both generations."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F6",
      "rationale": "The manifest leads with the amended four-review allowance and labels the prior 2/2 stop as historical. It retains the stop reason, branch head bb670b13f4 and reviewed head e4c9ed9c5e."
    }
  ],
  "request_id": "kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34-r04-eb080a",
  "round": 4,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The production collector and Round7 reader share an entry point. The retained 224949Z row and its collector-shaped live counterpart characterize in domain tests, and the historical in-game FAIL is preserved. All seven prior corrections are verified. The new findings prevent a C1 handoff.",
    "pending_later_phase_work": [
      "C2\u2019s Ghaele ownership repair and short external-harness qualification depend on a stable C1 handoff.",
      "C3 owns effects isolation and combined native qualification, including live and persisted agreement on the same identified run."
    ],
    "remaining_owner_review": [
      "OWNER AMENDMENT 1 authorized four C1 reviews. The fourth has been used. Any further C1 authority is an owner decision.",
      "T2/T3/T6 visual acceptance, T5/T7 listening choices and eventual integration of the dependent stack with frozen PR #219 remain owner decisions."
    ],
    "scope_reviewed": "Final C1 implementation and domain review for parent R7-F1 and R7-F6, plus the R6-F5 records disposition, against the original mission and OWNER AMENDMENT 1.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
