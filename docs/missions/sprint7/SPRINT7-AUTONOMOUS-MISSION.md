# Mission: complete all of Tabletop Rules Expansion Sprint 7 through the development process

Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules  
Host entry point: C:\Dev  
Scope: S7 T1–T7 / REP-04 and VIS-01–VIS-06  
Predecessor: the separately authorized Sprint 6 merge and release  
Delivery: one coherent draft PR, independent review/correction loop, then final owner review

## 1. Authority, objective, and autonomy

After the already-authorized Sprint 6 merge/release has completed, carry out the ENTIRETY of Sprint 7 of the original remediation epic using the installed development process. This replaces the earlier instruction to implement only Nalfeshnee and stop after each manually reviewed checkpoint.

Work through all seven stories: investigate, recover prerequisites, implement appropriate repairs, make reversible asset/cue selections, validate, collect presentation evidence, update records, obtain independent Codex review, and automatically address its findings. Do not stop after finishing a story merely to ask whether to continue. Do not ask Howie to relay ordinary reviewer findings or approve routine implementation choices.

Use one foreground Claude Opus/high developer session, one enrollment, one dedicated worktree/branch, and one draft PR covering Sprint 7. Organize implementation into coherent internal batches and commits. The single plan’s final scope is all seven stories, not whichever story finishes first.

Treat final owner visual/listening judgment as part of the final review, not a reason to stop before preparing and implementing concrete candidates. Exercise judgment and make a recommendation. Technical qualification, agent recommendations, and actual owner acceptance remain separate.

A genuine missing input or supported-tool limitation blocks only the work that depends on it while other authorized work continues. Complete useful independent work before reporting a task-level blocker, subject to the controller’s mandatory stops and runtime protections. Do not manufacture a PASS, invent an approval, or call a required unexecuted check optional to reach a ready status.

The separate authorization to merge/release Sprint 6 remains valid. This mission starts after that work; it does not authorize a Sprint 7 merge, release, tag, or human-installation change. End Sprint 7 at the process’s verified owner-review handoff, or accurately report its genuine BLOCKED/PAUSED state.

## 2. Establish the post-release starting point

Before editing Sprint 7 product files:

1. Read the Sprint 6 release handoff and refresh the actual repository, branch, PR, release, and worktree state.
2. Verify that the intended Sprint 6 changes have been integrated and their release completed. Use the real merge/release record, commit ancestry, published tag/package identity, and applicable release verification/restoration receipts. Do not infer completion from the instruction to release or from an unrelated latest tag.
3. The reviewed Sprint 6 chain was #198, #200, #203–#216. Use its actual final dispositions rather than assuming every original branch tip or PR state is unchanged.
4. Record the actual post-release master SHA, release tag, package/DLL identity, and any material environment changes. Do not hardcode the next version or use the former v0.0.193 development stack as the Sprint 7 base.
5. If the release task is still active, keep its work and runtime transaction separate; wait through the existing task mechanism. Do not start a second release or duplicate native writer. If its completion is blocked, identify that predecessor dependency rather than declaring Sprint 7 started on an assumed base.
6. Start a clean codex/* Sprint 7 worktree from the actual post-release origin/master. Preserve unrelated changes and other active missions. Do not reset, clean, force-push, rewrite history, or reuse a worktree claimed by another enrollment.

Read current AGENTS.md, docs/VALIDATION-POLICY.md, docs/AGENT-REVIEW-WORKFLOW.md, relevant guarded-runtime/save procedures, the original Gunslinger_Remediation_Epic_2026-11-15.docx, the current remediation register, KNOWN-ISSUES.md, and Sprint 6 disposition. Locate the epic in its documented lab location; this mission also supplies all seven story contracts below.

Carry Sprint 6’s accepted scopes, historical FAILs, original artifact identities, REP-05/06 blocks, G5 block, and other retained limits forward. They are not additional Sprint 7 acceptance requirements. Preserve the just-released fixes, current loader support contract, stable identities, saves, and compatibility. Coordinate with the separate UMM work without importing an unreviewed branch or altering the installed loader.

## 3. Enroll in the actual development process

Use the installed, pinned controller:

C:/Dev/agent-review/agent_review.py

Read the installed operating guide and project configuration, plus the maintained references:

- C:/Dev/development-process/README.md
- C:/Dev/development-process/agent_review/README.md
- C:/Dev/development-process/docs/GITHUB-AGENT-REVIEW-PROTOCOL.md
- The installed kingmaker-gunslinger project configuration and developer-report schema

Use installed --help, list, check-project, and the appropriate preflight. Verify rather than assume availability or command syntax. Inspect any existing Sprint 7 enrollment before creating another. Resume an existing matching enrollment through the supported procedure, preserving its findings and budgets; do not enroll twice.

Current maintained policy is Claude Opus/high for the main developer and independent Codex gpt-6.2-astra/MAX for review. Foreground is the ordinary interactive mode. Some older repository guidance names reviewer high or presents headless startup first; do not use it to downgrade the maintained policy. Verify the installed authority, session provenance, model/effort, and permitted permission mode. Do not change transcripts, use bypassPermissions, or silently install/activate a different controller to make a mismatch pass.

Save this complete mission VERBATIM, for example as docs/missions/sprint7/SPRINT7-AUTONOMOUS-MISSION.md. Do not replace it with a shortened paraphrase. Commit the mission/setup and push through the required repository route. Let the controller create or attach the draft PR.

The documented enrollment shape is:

python C:/Dev/agent-review/agent_review.py enroll --project kingmaker-gunslinger --worktree <actual-worktree> --plan single --mission <verbatim-mission-file> --mode foreground

Use project kingmaker-gunslinger (ID kmg), target master, and a dedicated codex/* branch. This mission authorizes that enrollment and its normal automatic review/correction cycle. It does not request a separate headless developer or a new workflow implementation.

After enrollment or supported resumption, run:

python C:/Dev/agent-review/agent_review.py status --enrollment <id> --instructions

Verify that foreground_session.problems is empty, read the complete effective task, and display every part if the controller splits it. Use the report bindings/citations it requires. Keep the mission immutable after enrollment; later actual owner scope changes go through the supported verbatim amendment mechanism on the same enrollment. Reviewer continuations cannot alter scope, permissions, models, required gates, or budgets.

The controller owns PR workflow comments, review requests/results, finding dispositions, candidate identity, gate receipts, transcript archives, and final readiness. Do not manually imitate those records, enroll a second review bot, or request human reviewers.

## 4. Plan all seven stories before costly validation

Create a compact Sprint 7 working matrix with exactly these seven original rows. For each, record the current baseline, exact owner/source, intended result, prerequisites, implementation status, focused checks, native observations, evidence identity, and remaining owner judgment.

Suggested execution order: baseline diagnosis/capture for T1/T4/T7; offline T2/T3 repairs and asset preparation; T4/T5/T6/T7 implementation; then coordinated candidate validation and presentation capture. Adapt the order to real dependencies. Continue independent stories when one is blocked. Parallel read-only/offline work is useful where supported; maintain one product writer and serialized runtime leases.

Do not interpret historical labels such as optional, deferred, or tolerated for an earlier release as permission to skip a story now selected by this mission. Do not expand into Sprint 8 animation refinements or Sprint 9’s broad integrated qualification.

## 5. S7 T1 / REP-04 — attribute and repair premature Ghaele dissolve

Read planning/EXPANDED-SUMMONING-VISUAL-REFINEMENT-STATE.md and the relevant summon/material/runtime code. The historical record observed increasing dissolve and a scripted movement attempt that did not start. The later Light Ray repair did not establish a body-visibility fix.

On the actual baseline, ordinarily summon a fresh Ghaele and observe idle, movement, combat, and elapsed lifetime in representative bright and dim presentation. Use a meaningful donor or sibling control. Tie captures to elapsed game time, remaining summon lifetime, relevant material properties, and actual actions.

Distinguish normal summon appearance and genuine expiry from premature dissolution. If the failure recurs, identify the exact property writer, shared material instance, animation/controller, or lifetime owner before changing it. Repair only the owned contribution. Preserve ordinary despawn, spell duration, and donor/sibling state; do not disable dissolve globally or hide the cause with a blanket every-frame reset.

Verify actual ordinary movement displacement. Keep the old failed scripted-command observation separate unless ordinary movement reproduces it. A command issued is not movement evidence.

After repair, exercise a second summon and a donor/sibling control to check material isolation and resource cleanup. Observe visibility through the intended lifetime and genuine expiry; accelerated expiry is only supplemental evidence, not a substitute for an elapsed-lifetime claim.

A bounded current non-reproduction must name its conditions and retain the historical attribution as unresolved. It is not a repair or specific disproof merely because one run passes. If attribution remains unavailable, preserve the evidence and exact unresolved scope, complete other stories, and report the actual blocker/disposition.

## 6. S7 T2 / VIS-01 — repair Nalfeshnee wing-root topology

Primary sources:

- assets-source/original-models/sprint39-nalfeshnee/generate_sprint39_nalfeshnee.py
- Its SOURCE.md and associated inputs
- planning/summon-playtest-repair/R7-STATUS.md
- The decoded-mesh audit, existing R7 tests, and current runtime loader

The reviewed R7 baseline had 12,402 vertices / 14,392 triangles and 48 zero-area triangles, 24 per side, where membrane grids collapse at the wrist. It also reported 56 directed edges used more than once. Verify the actual post-release baseline before editing.

Historical identity checks:

- Mesh SHA-256: d34e6c194d5b6c8c798a1ead77b9248e48e44721fccb2491eccee79aa1199aaf
- Albedo SHA-256: d9ab401ba01429ff64c8b05cdfaeac67b29b2d078a0f0c5d35e5002179868e7b
- Geometry-free rig capture: 6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d
- Existing pinned authoring pipeline: Blender 4.5.10, PYTHONHASHSEED=0

Repair the deterministic source topology, including coherent membrane/rim construction, rather than deleting arbitrary triangle indices from the shipped blob. Preserve R7 anatomy, silhouette, palette, UV/material behavior, valid skin weights, and rig mappings. The wings follow the upper torso: retain articulatedWings=false; no new wing animation, scale, reach, collision, or gameplay change.

Prefer a local generator/input change. If a shared helper genuinely must change, identify and verify its concretely affected consumers without starting a general generator rewrite.

Add or extend meaningful decoded-export validation that fails on the known baseline degenerates and passes on the repaired export. Check for new holes, inappropriate repeated edges, flipped visible surfaces, isolated unused vertices, invalid indices/data/weights, and rig/material violations. Respect legitimate UV seams. Do not hide defects by loosening tolerance or asserting an arbitrary new triangle count.

The existing orientation audit does not explicitly reject zero-area triangles; its normal agreement can count a zero normal as nonnegative. tools/validate_sprint39.py is the historical Pistol-Whip validator, not this asset’s mesh validator.

Regenerate through the existing pipeline, record input/source/toolchain/export hashes, and confirm determinism on fixed inputs. Inspect bound and deformed geometry and both roots during native idle, walk, attack, and turns, at useful detail and normal gameplay distance.

Use current R7 as the before image. The older R7 observer’s pre-R7 mesh beginning 1caa91e1 is not the VIS-01 baseline. Preserve historical evidence while adapting supported capture infrastructure. Require unobstructed roots after appearance fade-in settles.

## 7. S7 T3 / VIS-02 — repair the exact Salamander source

Use the current human/tail model, not the earlier Lizardfolk prototype:

- assets-source/original-models/sprint17-serpents/generate_human_salamander.py
- assets/sprint17-serpents/salamander-human-mesh.json
- src/KingmakerGunslinger/Summoning/SalamanderHumanVisualAttachment.cs
- The serpentine SOURCE.md, human bind evidence, current binding tests, and visual-refinement record

The documented problem affects 19 of 12,924 normal corners around a waist pleat and left-hand fingers. The waist pleat requires geometry work; flipping normals alone cannot make every incident face agree.

Perform one bounded recovery of the exact private authoring/bind input using named lab locations and provenance records. Verify capture identity and baseline reproduction. If an authorized supported guarded recapture is available, use the actual procedure and establish equivalence; do not pretend a new capture has an old hash or relax identity checks to accept it. Do not substitute a different donor/body or import native geometry.

Repair the waist topology and affected weights, inspect the fingers separately, and regenerate normals through the existing pipeline. Use a decoded-mesh regression exposing the original defect and checking no cracks, collapsed fingers, new seams, invalid weights, or other geometry regressions.

Inspect native idle, walking, attacks, and turns, including seams, fingers, spear grips and torso/tail attachment. Preserve the existing material initialization fix, spear attacks, owned tail slap, and current traveling tail gait. Changing serpentine locomotion belongs to Sprint 8.

If the exact prerequisite cannot be admitted or reproduced, identify the missing input and affected claims precisely, preserve the story’s blocked portion, and continue other work. Keep private captures, blends, and raw evidence out of Git/packages.

## 8. S7 T4 / VIS-03 — correct owned Trample/Overrun presentation

Inspect SummonActionTooltipPolicy, exact owned action identities, installed BlueprintAbility.GetTarget/AbilityHeader paths, and planning/summon-playtest-repair/R3-STATUS.md.

Reproduce the misleading custom-range/school text on actual owned desktop and relevant controller target/description windows. Use the narrowest supported presentation override to state truthful movement targeting and omit inapplicable school text.

Preserve the earlier duration, saving-throw and icon repairs; target legality, path rules, range, ability type, action cost and mechanics must remain unchanged. Do not assign a fictional school or mechanical range or globally change localization to improve these two windows.

Verify real rendered, populated windows, with an unrelated native ability and a suitable foreign/unowned control. Blueprint strings or direct formatting-method output alone do not prove UI behavior. Reuse the corrected R3 observation infrastructure: its earlier missing TooltipData.Type caused empty windows and misleading results.

If no safe supported scoped override exists after bounded investigation, prepare the concrete presentation proposal, exact native boundary, and demonstrated alternatives as a pending limitation decision. Do not silently close the story or stop the other work.

## 9. S7 T5 / VIS-04 — Ankylosaurus impact audio

Inspect AnkylosaurusTailControl, AnkylosaurusTailImpactVisual, AnkylosaurusTailImpactPolicy, and the proven R6 owned-cue path. The older deferral does not remove this story from the current mission.

Choose a suitable supported installed cue or properly sourced owned cue through the existing audio pipeline. Asset selection and implementation are authorized preparation: make a reversible candidate choice and a recommendation rather than stopping to ask for an initial preference.

Define the qualifying resolved impact from the actual current rule contract. Emit one owned cue for the exact owner/weapon/target/event, preserving existing native-event deduplication. Do not post audio per cosmetic LateUpdate frame or accidentally depend on a later saving throw.

Preserve attacks, damage, saves, action cost, control/knockback behavior, and animation. Demonstrate one cue on a qualifying impact, none on a miss, pre-impact cancellation, invalid target or foreign owner/weapon, and no duplicate playback when the same native strike is observed more than once.

Reuse exact playback ownership and retirement. Exercise concurrent sources so retiring one does not stop the other. Check natural completion and early source/view loss. Record ordinary-volume gameplay audio and distance context for final listening review.

Use the supported Wwise/owned-audio pipeline. Verify actual bank/event identities rather than inventing event names or introducing a parallel audio library/backend.

## 10. S7 T6 / VIS-05 — Dust Mephit breath

Inspect MephitBreathPolicy, ExpandedSummoningSpecialBuilder.MephitBreathR4, and the actual Dust-owned breath publication/delivery path. Locate the current owner rather than assuming the native-backed rows configure Dust.

Select or author a properly sourced dust effect with a readable forward cone, appropriate color/opacity and visible targets. Implement the recommended reversible candidate without an upfront asset-choice pause.

Bind through exact Dust ownership: an owned visual/projectile or safely cloned effect/material instance. Do not mutate a shared native prefab, projectile or material. The existing cold-cone mapping is shared by multiple damage types and mephits; changing that generic mapping to dust would affect other consumers.

Preserve actual area/orientation, damage type/dice, blindness, save, enemy filter, target rules, cooldown and action budget. Include appropriate native donor/other-consumer controls; a narrow Dust repair does not require replaying every mephit unless a shared dependency actually changes.

Capture real committed casts at normal speed. Demonstrate correspondence between the visible forward footprint and actual affected area, exactly one bounded effect per cast, ordinary completion, repeated casts, pre-execution cancellation, and source loss while active. Verify owned cleanup without disturbing a concurrent source/donor.

Observe mechanics separately from particles. A dust-colored effect alone does not prove damage, conditions, saving throws or targeting are preserved. Provide curated native before/after evidence for final visual acceptance.

## 11. S7 T7 / VIS-06 — resolve Trumpet Archon’s provisional cue

Read R6-STATUS.md and inspect the current Trumpet Blast builder, SummonAbilityCastCue, SummonCastCueLifecycle and associated policy. The current InspireCourage substitute already plays. This story concerns suitability and disposition, not reopening the repaired silence defect.

Record the current cue in the actual blast scenario, identify its provider/bank/event, and compare a small useful set of supported installed or properly sourced alternatives. Make a recommended reversible selection. If the current cue is best, retain it with a concrete listening recommendation; that is a proposed disposition, not a code repair or invented owner acceptance.

Preserve the repaired once-per-execution and owned-retirement path, all combat mechanics, and donor audio. Check one cue despite multiple affected targets, no cue on pre-execution cancellation, natural completion, source loss, and no replay/leak across the relevant reload boundary.

A returned stop request is not proof of audio completion. Use actual playback identities and native completion evidence. If a shared audio helper changes, verify the concretely affected Vrock neighbor rather than relabeling an old PASS.

Return comparable current/recommended samples with volume, distance and provenance. Do not infer audibility or suitability from a nonzero event ID or measured signal alone.

## 12. Common asset, presentation, and runtime requirements

Prefer supported installed assets or project-authored assets and existing pipelines. Preserve provenance and redistribution boundaries. Do not purchase assets, add libraries, extract proprietary geometry/audio into the repository, or publish private inputs. A supported installed event reference differs from redistributing its source audio.

When a visual/audio choice is needed, prepare a small concrete comparison, choose a candidate using the story’s criteria, implement it reversibly, and continue. Owner visual/listening acceptance remains pending final review. A genuinely incompatible product choice should be isolated with an actionable recommendation after completing useful independent work.

Keep raw evidence in the approved lab location. Deliver a curated review set through the established review mechanism: useful before/after images or short clips, ordinary gameplay framing plus necessary detail, and comparable audio samples. Name file paths/hashes and make the evidence accessible to the intended review. Commit only permitted sanitized records, provenance and authored exports, not raw saves/logs, packages or private native inputs.

Record actual motions, elapsed game time, appearance/expiry state, lighting and camera context. Do not count an issued command, hidden/obstructed model, fade-in frame, particle preview or empty tooltip as the intended observation. Disclose unmatched conditions.

If loopback audio is used, note that the existing capture script records the whole default output-device mix, not an isolated game process. Keep unrelated audio out where possible without persistent human-device changes. Separate playback/timing assertions, signal measurements, actual listening and subjective suitability.

Use only admitted disposable fixtures, guarded runtime requests, current package checks, leases, and transactional staging/restoration. Preserve the immutable baseline and human installation/saves. Respect active peer runtime locks and finish restoration before yielding. Do not force-stop another transaction or change loader/profile/fixture admission rules to get a run through.

## 13. Validation and evidence economy

Treat this as seven stories organized into bounded development batches, not an unlimited campaign. Honor the repository’s current planning/validation limits and the controller’s cumulative limits. Record both per-batch and total expenditure. Do not evade a cap by renaming batches, re-enrolling, or rerunning unchanged campaigns.

Reproduce suspected defects once under matched conditions. Keep failures with exact identities and causes; unexplained failures remain unexplained. Use focused behavior/data tests and affected production compilation. Avoid source-string tests, decorative tests, empty selections, and unnecessary libraries or mocks.

Batch related offline corrections before expensive packaging. Build each unchanged candidate once and reuse its verified package. Combine compatible native observations when it reduces work without compromising identity or ownership controls. A change in exported assets changes product bytes even when C# is unchanged.

Track source H, package/DLL/export hashes, harness, fixture, game, loader, providers, mode, commands, durations and restoration separately. Bind each PASS to what actually ran. A later documentation head or archive commit is not a newly tested package. Reuse prior evidence only with its scope and dependency reasoning intact.

Before final review, resolve concrete interactions among Sprint 7 changes using a bounded selection on the final candidate. Check shared material/audio/UI/asset-loader dependencies that actually changed. Do not turn this into a rerun of Sprint 6, a full roster census, arbitrary mod matrices, or Sprint 9 release qualification.

## 14. Automatic review and correction loop

Use the installed controller’s documented foreground cycle:

1. Complete the mission’s available implementation/evidence work, commit a coherent clean candidate, and push through the required guarded route.
2. Run the required controller verify gates on that exact candidate. For this project the required gate is focused. Select meaningful named filters for changed behavior and dependencies.
3. The current configured argument pattern admits a single -Filter name. Use separate verify calls for separate filters; do not pass semicolon lists or change the gate configuration. Supplemental native/asset evidence is recorded separately and does not replace the required receipt.
4. Submit a schema-valid developer report bound to the mission/instruction revision and exact candidate. State the seven-story scope, findings dispositions, evidence and remaining owner-only judgments accurately.
5. Run review, read the complete structured result and Continuation for Claude, and implement the coordinated correction batch automatically. Give brief progress updates without waiting for Howie to relay or approve ordinary corrections.
6. Repeat affected checks and review within the same enrollment. Address every pending finding. A rebuttal needs evidence and independent review; a required gap cannot become a harmless deferral.

Documented command shapes, to verify against the installed help:

python C:/Dev/agent-review/agent_review.py verify --enrollment <id> --gate focused -- -Filter <named-filter>

python C:/Dev/agent-review/agent_review.py review --enrollment <id> --report <schema-valid-report.json>

The single plan is final for the WHOLE mission. Do not seek ready_for_owner_review after only T2 or another subset. If a real required blocker remains after independent work is exhausted, report it honestly rather than submitting a fabricated ready report.

Respect installed round, attempt, invocation, no-progress and time budgets. The maintained project currently specifies five review rounds per checkpoint, three attempts per round, thirty developer invocations and a three-round no-progress limit; verify actual pinned values. Do not increase limits, reset the enrollment, change models/permissions, reinstall/activate the workflow, or edit required validation policy to bypass a stop.

Only after ready_for_owner_review, run:

python C:/Dev/agent-review/agent_review.py finalize --enrollment <id>

python C:/Dev/agent-review/agent_review.py status --enrollment <id> --remote

Observe completion. Report AWAITING_OWNER_REVIEW only if the controller returns it and handoff_current is true. Distinguish reviewed source H from its controller-authored archive A. If the status is BLOCKED or PAUSED, report that exact state/reason and counters.

## 15. Completion and final handoff

Maintain a focused Sprint 7 record and the current remediation/known-issue state without rewriting historical observations. Account for exactly S7 T1–T7 and their seven original record IDs. Separate technical implementation, source inference, automated checks, native evidence, proposed retained limitation, owner aesthetic/listening decision, merge and release.

For a technically complete handoff, each story must have its required implemented or specifically supported disposition and applicable evidence. Owner-only final taste may remain pending. An unavailable required source/native observation, unresolved defect, or non-attributed historical report must retain its actual status; it cannot be hidden under owner taste or called repaired.

Return one final report in this conversation containing:

- Enrollment, draft PR, branch/base, actual post-Sprint-6 release baseline, reviewed H and archive A.
- A seven-row outcome table: original problem, result, evidence, technical status, remaining owner decision or precise blocker.
- A concise explanation of each repair or supported no-change disposition and what players will see/hear.
- The curated visual/audio review set, clearly labeled before/after and with recommendations.
- Exact candidate and relevant asset/harness/fixture identities, commands, timings, verdicts and restoration results.
- Independent review rounds, finding dispositions and controller expenditure/status.
- Unexecuted cases and proposed limitations without converting them to PASS.
- Any final visual/listening selections, missing prerequisites or publication decision Howie must make.

Do not report success merely because a worker was started, code compiled, one reviewer result passed, or a PR exists. Finish the automatic correction loop and verify the real handoff. Do not merge or release Sprint 7 under the earlier Sprint 6 authorization.

Begin with predecessor/process intake, then proceed through all authorized Sprint 7 work without per-story manual approval stops.
