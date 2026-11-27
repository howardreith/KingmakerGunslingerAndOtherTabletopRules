<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r08-request part=1/1 sha256=cf0bc224394fa4228f79863a5b359b1b56f9bb7a0eb8a07cde0832fe9fe34d7e -->
### Claude Opus — review request — round 8 — candidate `680f691c5e`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r08-e72b8e` |
| Checkpoint | `final` (final) |
| Candidate H | `680f691c5e62ab6ed79ef070dd76764491642ca7` |
| Target B (`master`) | `a804d4bfc6fb53e3ea5842fef8d8766c5bc79e94` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `kingmaker-umm-0311-mission-20261123` sha256 `f7d990f396` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 4, sha256 `732aeb371fbbc7ee4aa71c2ef5bd848874af88607e27ab0babcb73e7395ce936` |
| Developer observed | foreground session `e178a93b-ca53-4b6b-a4d2-a357d48eaa77`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 8 of 9 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Prepared under owner amendments revision 4 sha256 732aeb371fbbc7ee4aa71c2ef5bd848874af88607e27ab0babcb73e7395ce936 under owner amendment umm-0311-final-two-20261126-02. Round-9 corrections for Tabletop Rules Expansion PR #220: R1-F4, R7-F1 and R7-F2 as one batch. Code and tests c67881f3fb; 680f691c5e adds evidence and documentation only.

R1-F4: Assert-KmgPackagedRepositoryFilesMatchCommit resolves every ZIP member under the mod folder to the file tracked at the producing commit that produced it - the same path, or the source of the build's copy rule for that packaged path, read from the producing commit's project file (Content/None items with a Link and CopyToOutputDirectory: assets/game/icons/*.png ships as assets/icons/*.png; unsupported rule forms are refused) - matched without regard to letter case and resolved to Git's spelling (two tracked spellings are refused as ambiguous). Only the compiled assembly is a generated member; any other member resolving to no tracked producer file is refused, never treated as generated. The checkout-byte comparison (git archive of the commit) is unchanged and now covers 457 members of the clean package instead of 159.

R7-F1: the helper takes the receipt's expected package SHA-256; it opens the ZIP once without write or delete sharing, hashes it on that handle, refuses a mismatch, and reads the members through the same handle. Build-Local passes its measured digest; both staging readers pass the receipt's packageSha256.

R7-F2: the focused driver now builds the one final candidate in a retained producer clone and never removes it; negative and destructive fixtures run in a separate disposable clone of the same commit with the producer's build output. After the fixture cleanup attempt, the real development reader Read-KmgBuildLocalManifest -DevelopmentPurpose umm-0311-admission -RequireCurrentMaterialInputs at the retained location <dev>\KingmakerGunslingerLab\worktrees\umm-0311-final-candidate-c67881f3fb\producer (producer clone clean at the producing commit) returned producer c67881f3fb8892cae09cdf4cc4f370170f20afc8, ZIP d8d14600c8c0fa0c0abc117cba1c1f0cb8fe446983d1bd23e2ddf5625bb29929, DLL 17a42879a57dba9914cebd67b3855ad95a94ef64775a8c2f66886ef5f03c808a, MVID 5efdffb9-c4ac-4a3e-91f7-089a12991064; the four-loader audit is of that same DLL. The disposable fixture area could not be removed (Windows denied deleting a Git pack index that the fixture clone shares by hard link with the lab repository's object store); it was left in place and does not contain the retained candidate. Verified R6-F1..R6-F3 are preserved. The 62/63 admission run remains not an overall pass. Native runtime NOT RUN.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 --gate focused -- -Filter loader.production-dll-references-pinned-loader | yes | pass | 55.1s | Controller receipt gate-focused-680f691c5e62-d88e49a1250-a32501bde64-20261127T023814854369Z.log on 680f691c5e: production compiled with the pinned loader references, the named loader test selected. |
| Focused driver Invoke-KmgFocusedR9.ps1 on c67881f3fb (exported as focused-r9-driver.ps1.txt) | no | fail | 764s (build 530s) | docs/evidence/umm-0311/focused-r9.json (driver docs/evidence/umm-0311/focused-r9-driver.ps1.txt, completion step docs/evidence/umm-0311/focused-r9-completion.ps1.txt; 17 of 18 cases passed, the failing case being the disposable fixture-area removal). One Build-Local development build in the retained producer clone after recorded pre-build checks (working directory, Assert-KmgReferenceBundleMatchesInstall on the selected bundle, no Kingmaker process); complete output docs/evidence/umm-0311/development-build-r9.txt. All 16 driver cases passed; the driver exited 1 because its own removal of the disposable fixture area failed, so its post-cleanup steps ran in the completion step. The whole admission suite was not rerun. |
| Completion step Complete-KmgFocusedR9.ps1 (exported as focused-r9-completion.ps1.txt) | no | fail | 12s | Retried the fixture-area removal once (read-only attributes cleared): denied again on multi-pack-index; remnant preserved. Then the real development reader Read-KmgBuildLocalManifest -DevelopmentPurpose umm-0311-admission -RequireCurrentMaterialInputs at the retained location <dev>\KingmakerGunslingerLab\worktrees\umm-0311-final-candidate-c67881f3fb\producer (producer clone clean at the producing commit) returned producer c67881f3fb8892cae09cdf4cc4f370170f20afc8, ZIP d8d14600c8c0fa0c0abc117cba1c1f0cb8fe446983d1bd23e2ddf5625bb29929, DLL 17a42879a57dba9914cebd67b3855ad95a94ef64775a8c2f66886ef5f03c808a, MVID 5efdffb9-c4ac-4a3e-91f7-089a12991064 (pass; recorded as its own case). Retained layout with digests in focused-r9.json. |
| LoaderApiAudit on the retained candidate's DLL (four profiles) | no | pass | 2s | docs/evidence/umm-0311/api-audit-r9.json: DLL 17a42879a57dba9914cebd67b3855ad95a94ef64775a8c2f66886ef5f03c808a (MVID 5efdffb9-c4ac-4a3e-91f7-089a12991064) extracted from the retained ZIP d8d14600c8c0fa0c0abc117cba1c1f0cb8fe446983d1bd23e2ddf5625bb29929: 0.31.1 RESOLVED; 0.32.4 RESOLVED; owner-0.33.0 RESOLVED; 0.33.4 RESOLVED (23 types, 67 members). Static metadata resolution, not a game run. |
| Lightweight ZIP checks before the build (docs/evidence/umm-0311/zip-probes-r9.txt) | no | pass | 73s | Corrected helper (uncommitted batch, identical to c67881f3fb) on the round-8 frozen ZIP d86d9a7c against c0b54ba4ad: clean 457 compared; remapped icon edit, case-variant README edit, unmapped member and member outside the mod folder refused; unedited case variants pass; archive replaced after digest measurement refused both ways; matching archive passes. |
| Dry run of the committed focused-packaged-content region (docs/evidence/umm-0311/packaged-content-dry-run-r9.txt) | no | pass | 124s | Disposable clone of c67881f3fb with this checkout's older build output and a stand-in receipt, run to catch fixture errors before the single build: writer-check, packaging and digest cases behaved as designed; the ordinary-reader cases stopped at the DLL source-stamp check, so this run is not evidence for them. |
| Native checks | no | not_run | 0 | Not authorized. |

**Evidence references:**
- prepared under owner amendments revision 4 sha256 732aeb371fbbc7ee4aa71c2ef5bd848874af88607e27ab0babcb73e7395ce936
- docs/evidence/umm-0311/INDEX.md
- docs/evidence/umm-0311/focused-r9.json
- docs/evidence/umm-0311/development-build-r9.txt
- docs/evidence/umm-0311/development-receipt-r9.json
- docs/evidence/umm-0311/api-audit-r9.json
- docs/UMM-LOADER-COMPATIBILITY.md

**Prior findings to assess:** `R1-F4`, `R7-F1`, `R7-F2`, `R7-F3`

**Owner amendments to the mission — revision 4, captured-source sha256 `732aeb371fbbc7ee4aa71c2ef5bd848874af88607e27ab0babcb73e7395ce936`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

```text
# Owner amendments to the mission

Cumulative record kept by the agent-review controller. Each owner message below was applied by the owner through `resume --amend-mission` and is preserved verbatim, in order, between its labeled marker lines. Later amendments supersede conflicting earlier requirements; requirements they do not change still apply.

----- BEGIN OWNER AMENDMENT 1 (captured 2026-11-26T13:58:04Z; message sha256 c3d3998ac1dc60dec6597a2f22d10b3d1bff078dc72b8377a0c4590f72b77b11) -----
PHASE B — RESUME THE FOUR PAUSED REPOSITORIES

Once Phase A has completed, or a repository has reached a concrete blocker
within its authorization, proceed with the remaining authorized work.
One blocked repository must not stop independent progress in the others.
Report any incomplete Phase A publication explicitly.

Resume the existing enrollments and PRs:

TRE:
- Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules
- PR: #220
- Last recorded head: a7d293a533
- Reviews used: 5.
- New TOTAL review cap: 8.

Buff:
- Repository: howardreith/KingmakerBuffPlanner
- PR: #26
- Last recorded head: e636c2c563
- Reviews used: 5.
- New TOTAL review cap: 8.

Waking:
- Repository: howardreith/KingmakerWakingRune
- PR: #12
- Last recorded head: 5432227c86
- Reviews used: 5.
- New TOTAL review cap: 8.

Skip:
- Repository: howardreith/KingmakerSkipCinematic
- PR: #3
- Last recorded head: 45f5c206a2
- Reviews used: 7.
- New TOTAL review cap: 10.

This authorizes three additional review submissions per repository.
The caps above are absolute totals, not additional-round values.

I also explicitly authorize one supported resumption of each existing
no-progress-blocked enrollment under this assignment. Use the controller's
documented resume mechanism. Preserve completed rounds, findings, evidence,
and history; do not manually erase counters or create replacement
enrollments to escape previous stops.

Start from the recorded unresolved findings:

- TRE: R1-F4, R4-F3, R5-F1, R5-F2, R5-F3, R5-F4.
- Buff: R1-F3, R4-F1, R4-F4, R5-F1, R5-F2, R5-F3.
- Waking: R1-F1, R4-F1, R4-F2, R4-F3, R4-F4, R5-F1, R5-F2.
  R5-F3 remains optional.
- Skip: R5-F3, R6-F1, R6-F2, R6-F4, R7-F1, R7-F2, R7-F3, R7-F4.

Read the actual finding text and current mission records before fixing.
Do not reconstruct requirements from finding titles alone.

Batch coherent corrections before submitting a review. Use focused,
realistic reproductions first, then the required candidate gate.
Make sure a regression test reaches the behavior it claims to test;
an earlier unrelated refusal is not evidence that the target behavior works.

Stop each repository at AWAITING_OWNER_REVIEW as soon as its required
findings and checks are satisfied. Do not merge or release these four.

SCOPE FREEZE

The authorized correction scope remains:

- Declare UMM 0.31.1 consistently.
- Compile against the agreed pinned 0.31.1 baseline through the relevant
  supported build routes.
- Perform the agreed static checks for the specific higher loader versions.
- Keep package, receipt, and evidence claims accurate.
- Correct concrete regressions introduced by this mission.
- Complete the integration and release work expressly authorized above.

No new gameplay features, general build-system redesign, broad MSBuild
interpreter, budget framework, evidence framework, or native-test campaign.

Prefer the smallest adequate correction using existing infrastructure.
Simplifying or removing unnecessary mission-added machinery is acceptable
when it preserves the required behavior and truthful evidence.

Previously accepted optional findings and deferrals remain nonblocking.

New findings must identify a concrete defect within this frozen scope,
including a direct regression from the current corrections or integration.
General hardening ideas and unrelated cleanup go into follow-up notes.

A demonstrated data-loss defect, broken rollback, incorrect release
artifact, or false evidence claim introduced by this work remains a real
blocker. Fix it narrowly or remove the unnecessary offending path; do not
waive it merely to obtain a ready verdict.

Communicate this scope to the controller/reviewer through the existing
supported mechanism. Do not fabricate approval or weaken required gates.

VALIDATION ALLOWANCE AND ACCOUNTING

I authorize up to 30 minutes of NEW validation work per repository for
this assignment. This is a ceiling, not a target.

For Mounted, explicitly declare a separately budgeted integration/release
campaign with a 30-minute allowance using the existing supported mechanism.
Keep the completed correction campaign's ledger unchanged.

For the other repositories, record the authorization through their existing
accounting or allowance mechanism. Apply any required allowance adjustment
once, with this assignment as its source, preserving prior spending and
history. Do not introduce new metering infrastructure.

Count all validation work under this assignment, including:
- Direct reproductions and focused tests.
- Failed runs and reruns.
- Builds and required gates.
- Packaging and release qualification.

Direct invocation does not make a run uncounted. Where no automated ledger
exists, record durations in the existing mission status record.

Inspect nested commands so a focused check does not unexpectedly launch a
broad campaign. Reuse valid results for unchanged inputs when permitted.

Do not start a known run that cannot fit the remaining allowance.
Complete necessary cleanup safely and record actual costs.

No automatic extensions beyond the stated review and validation allowances.
If a repository reaches a real stop, report its precise blocker and continue
other independent authorized work.
----- END OWNER AMENDMENT 1 -----

----- BEGIN OWNER AMENDMENT 2 (captured 2026-11-26T16:27:25Z; message sha256 93783515c5613c7950d2eb9979b3b35bbe9a870c39e801f38d2851453a622d52) -----
PHASE B — SPECIFIC CORRECTIONS

I authorize one supported resumption of the CURRENT blocked state in each
of TRE, Buff, Waking, and Skip. Preserve the same enrollments, PRs,
completed rounds, findings, evidence, and spending.

Review caps are absolute totals:
- TRE #220: retain cap 8; reported 5 used.
- Buff #26: retain cap 8; reported 6 used.
- Waking #12: retain cap 8; reported 6 used.
- Skip #3: increase cap from 10 to 12; reported 10 used.

Do not automatically resume a subsequent no-progress stop or extend
these caps again.

TRE #220

Starting reported head: b16ce7a47c.

Finish the missing evidence for the corrections already committed:
- R4-F3: ordinary admission through the real child and report projection,
  using the corrected JSON handling.
- R5-F4: restoration of all five incoming caller settings after success
  and a representative failure.

Preserve the correctly attributed results already obtained for R1-F4,
R5-F1, R5-F2, and R5-F3. Do not describe the prior 62/63 admission run as
an overall pass.

Before another build, inspect any surviving output from the failed
467-second attempt. If it is unavailable, say so explicitly.

Capture complete build stdout/stderr and exit status from the start of
the next attempt. Diagnose a failure before spending another broad run.

Use a retained, correctly identified frozen candidate where the existing
workflow permits. Run the missing focused cases and required gate;
do not repeat the entire admission suite merely to exercise one case
unless a concrete dependency or required gate makes that necessary.

Export the relevant results, retain their producing identities, and
submit the next review after the required evidence is complete.

Buff #26

Starting reported head: 45427841.

Correct these four required findings as one coherent batch:

- R5-F1: protect the previous release BEFORE moving it. Permit cleanup
  only after confirmed promotion or restoration. Preserve and identify
  the recovery location if restoration has not completed.

  Exercise interruption/unwind immediately after the backup move on a
  disposable target, and retain the occupied-destination control.

- R6-F1: use Windows-compatible case-insensitive matching for the
  existing applicable production configuration paths.

  Retain acceptance of unrelated test-project user settings. Do not
  broaden the scan to unrelated files.

- R4-F1: use two ZIPs that EACH pass the standalone validator before
  demonstrating deterministic-build disagreement.

  Keep the required timestamps. Use a valid differing ZIP representation,
  such as a supported compression difference, under the disposable
  fixture's cleanup root.

  Capture both validator successes, distinct package digests, the actual
  inter-build refusal, and unchanged previous-release hashes.
  Correct the interpretation of the old evidence without altering it.

- R6-F2: run the existing four-loader static audit on the packaged DLL
  from the corrected freeze. Bind its results to that ZIP, DLL hash,
  MVID, and source. Preserve the old audit as historical evidence.

R6-F3 remains optional and nonblocking.

Waking #12

Starting reported head: 495635d7.

Correct these three required findings as one coherent batch:

- R1-F1: include actual authored compiler inputs in provenance checks
  independently of Git ignore rules, and use Windows-compatible matching
  for selected artwork.

  Exercise ignored mixed-case artwork and an ignored authored .cs file
  that the production project really compiles.

  Preserve narrow existing generated-file exceptions and documentation
  controls. Do not build a new input-inventory framework.

- R5-F1: identify the physical build-owned publication directory through
  the existing identity/path machinery, including alternate spellings.

  Resolve-Path alone does not establish equivalence for the reported
  Windows short-path alias.

  A build-owned ZIP must use the publication lock and must be refused if
  its required receipt is absent, including after an interrupted
  publisher releases the lock. Preserve the supported handling of
  genuinely external packages.

- R4-F4: restore recon correctly for an interrupted legacy reinstall
  when the replacement directory already exists but recon.flag does not.

  Use the journal's current reconEnabled field or its legacy recon field
  as appropriate for interrupted recovery. Preserve normal reinstall
  behavior. Exercise both Boolean values in the existing disposable
  recovery fixture.

Keep the accepted R5-F3 deferral and optional R6-F1 nonblocking.

Skip #3

Starting reported head: 4f3ef4aa.

Correct the path bug and close the two specific evidence gaps together:

- R10-F1: resolve a relative ProductZip through PowerShell's literal
  filesystem-path semantics BEFORE passing an absolute path to .NET
  File.Copy.

  A bare Path.GetFullPath(relativePath) can still use the wrong process
  working directory and is not an adequate correction.

- R8-F1: use the already-built clean PRODUCTION ZIP for the positive
  handoff through the real copy/validation/extraction helpers.

  Combine this with the relative-path regression: place a different
  same-named ZIP in the process working directory and demonstrate that
  the caller-selected production ZIP is used.

  Compare the admitted ZIP and extracted DLL identities to the genuine
  production package's manifest.

- R9-F3: exercise the actual runner setup/cleanup path after successful
  validation and handoff, followed by a later setup failure.

  Verify removal of owned temporary copies and extraction directories.
  A missing-validator failure occurs too early; cleanup invoked only
  by the fixture's own finally does not prove the runner's cleanup.

  Use disposable filesystem targets and make the fixture incapable of
  reaching a native game transaction.

Preserve the removal of unnecessary mission-added native-runner machinery.
Do not reintroduce it to implement these tests.

VALIDATION ALLOWANCES

Read the current records and apply these one-time changes:

- TRE: add 20 minutes to the release-and-resume assignment allowance,
  increasing its total from 30 to 50 minutes.
- Waking: add 10 minutes, increasing its total from 30 to 40 minutes.
- Buff: retain the existing 30-minute allowance.
- Skip: retain the existing 30-minute allowance.
- Mounted: retain its existing 30-minute integration/release allowance.

Preserve all prior spending, including failed runs and direct tests.
Do not reset ledgers or borrow unused time from completed repositories.

Count all new validation: reproductions, direct tests, builds, gates,
packaging, and reruns. Use existing accounting; add no budget tooling.

Capture logs before launching expensive work. Serialize expensive
validation on the shared Windows host to avoid avoidable contention.
Batch corrections before candidate qualification.

Use focused behavior checks first. Broaden only for a concrete remaining
risk or an existing required gate. Reuse unchanged evidence only with
accurate source and artifact attribution.

Do not start a known run that cannot fit the remaining allowance.
Finish necessary cleanup safely and record actual costs.

BOUNDARIES AND COMPLETION

The previous scope freeze remains in force:
- UMM 0.31.1 minimum and pinned compile baseline.
- Agreed static loader checks.
- Accurate packages, receipts, and evidence.
- Direct regressions introduced by this mission.
- Mounted's already-authorized integration and alpha release.

No new gameplay features, general build-system redesign, controller
redesign, evidence framework, or native-test campaign.

Native gameplay and combined-stack testing remain NOT RUN.
No installation, game launch, save changes, live UMM changes, use of
another enrollment's game copy, or Nexus publication.

Accepted optional findings and deferrals remain nonblocking.
New required findings must identify a concrete defect within the frozen
scope. Record unrelated improvements as follow-up.

Stop TRE, Buff, Waking, and Skip at AWAITING_OWNER_REVIEW.
They are not authorized for merge or release in this continuation.

One blocked repository must not prevent independent progress elsewhere.

Update FLEET-STATUS and mission memory with:
- Dice and Preserver completed.
- Mounted's actual review, merge, and publication outcome.
- Each remaining PR's source/archive head and controller state.
- Findings resolved or still required.
- Validation producing commits, results, and actual spending.
- Any exact permission rejection or other blocker.

Preserve all history and do not mark the seven-repository mission complete
while required work remains.
----- END OWNER AMENDMENT 2 -----

----- BEGIN OWNER AMENDMENT 3 (captured 2026-11-26T22:35:35Z; message sha256 4af6c8f544fb1abbb0f1add1afcd70bf298ba8fa97c93571a435b0a6167a6243) -----
TRE — FIX THE FOUR SPECIFIC REMAINING FINDINGS

Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules
PR: #220
Reported current head: a38e38ec53
Rounds used: 6; retain TOTAL cap 8.

I authorize one supported resumption of the CURRENT no-progress block.
Keep the same enrollment, PR, completed rounds, findings, and history.

Add 30 minutes to this assignment's validation allowance, increasing its
total from 50 to 80 minutes. Preserve all prior spending.

Address R1-F4 and R6-F1 through R6-F3 as one coherent correction batch.

R1-F4 — packaged inputs:

The existing start/end working-tree fingerprint is insufficient.

Use the existing packaging inputs and producer identity to verify the
actual repository-sourced content shipped in the ZIP, or consume already
available immutable verified inputs. Keep the implementation narrowly
within the existing package/receipt helpers.

Account correctly for the repository's existing byte and checkout
conventions. Do not introduce arbitrary normalization that conceals
different packaged content.

The negative fixture must:
1. Start from a clean producer.
2. Put an edited README into the real package.
3. Restore the working README BEFORE the receipt decision.
4. Demonstrate refusal through the ordinary receipt writer or reader
   because the packaged content does not match its claimed producer.

Retain the clean-package and unshipped-review-note controls.
Do not repeat a working-tree-only check as the proof.

Correct the compatibility guide's attribution of the earlier fixtures
without changing their original outputs.

R6-F1 — evidence records:

Compute the exported hashes from the final committed Git blobs, as the
index's own verification instructions require.

Verify every indexed digest. Preserve distinctions between raw captures,
converted/exported files, and committed bytes.

Correct or annotate the mixed-encoding exit trailers from retained data.
Preserve the original captures and failed-run history.

Keep the original 467-second failure classified as unknown unless
independent retained evidence establishes its cause. Describe the later
observed host-guard refusal separately.

Use the existing captures for this work; do not rerun tests merely to
repair their index hashes or export encoding.

R6-F2 — current static audit:

Select the final admissible frozen package after the necessary code
corrections. Use the existing auditor on that packaged DLL.

Record its producer, ZIP SHA-256, DLL SHA-256, MVID, and results for:
0.31.1, 0.32.4, owner 0.33.0, and 0.33.4.

Retain historical audits under their original identities.
Do not transfer an old DLL's PASS to different bytes.

R6-F3 — cleanup and rollback:

Complete the fallible private-copy cleanup inside the existing rollback
scope, before discarding the previous Mods tree.

Do not report successful restoration without verifying it, or suppress a
cleanup failure merely to obtain a pass.

Use the requested disposable-directory fixture: hold the admitted ZIP
open for reading without delete sharing, allow destination verification,
and demonstrate that the cleanup failure leaves the previous sentinel
restored at Mods. Retain the successful-swap control.

After these corrections, run focused reproductions and the required gate
on the corrected candidate, then submit the next review.

Preserve the earlier 62/63 admission result as NOT an overall pass and
keep later focused successes attributed to their actual producers.

Stop at AWAITING_OWNER_REVIEW. No TRE merge or release is authorized yet.

VALIDATION COORDINATION

Use one foreground coordinator to launch expensive validation for this
assignment. Other workers may inspect or prepare changes, but must return
them to the coordinator for builds and gates.

Do not overlap this mission's expensive builds, suites, or gates.

Use the existing host guards and coordination mechanisms. Check required
host conditions before expensive work. If another session's game or
transaction blocks a guarded operation, wait for a suitable window or
continue independent nonconflicting work.

Do not stop another session's game/process, bypass its lock, or create a
new host-coordination framework.

Capture stdout/stderr and exit status from the start of each expensive
invocation. Preserve useful failure output before removing temporary work.

Count all validation spending, including failed runs, direct reproductions,
builds, packaging, and gates. Read live records before applying each
one-time allowance adjustment.

Reuse correctly identified unchanged artifacts where the existing workflow
permits. Build a necessary final candidate once and reuse it for its
applicable checks.

No automatic review-cap increases, further no-progress resumptions, or
additional time extensions beyond this instruction.

COMMON BOUNDARIES

Keep the frozen mission scope:
- UMM 0.31.1 minimum and pinned compile baseline.
- Agreed static loader checks.
- Correct packages, receipts, evidence, and direct mission regressions.
- The merges and Buff release expressly authorized here.

No new gameplay features, general build-system redesign, controller
redesign, evidence framework, or native-test campaign.

Memory Leak Fix remains excluded.
No installation, game launch, save changes, live UMM changes, use of another
enrollment's game copy, or Nexus publication.

Accepted optional findings and deferrals remain nonblocking.
Use normal branch protections, permissions, and publishing controls.
Do not force-push, rewrite history, overwrite published tags/assets, or
work around permission denials.

A blocked repository must not stop independent authorized work elsewhere.

----- END OWNER AMENDMENT 3 -----

----- BEGIN OWNER AMENDMENT 4 (captured 2026-11-27T01:46:18Z; message sha256 5e16e096f23642f9a12071a5c6f5f368db4c87bf95a05843385bf9877fb672c6) -----
NEW OWNER AMENDMENT
ID: umm-0311-final-two-20261126-02

NEW BOUNDED AUTHORIZATION

TRE #220:
- Reported current head: ffdd7fa7a5.
- Reviews used: 7.
- New TOTAL review cap: 9.
- Add 20 validation minutes, increasing this assignment's allowance
  from 80 to 100 minutes.

Waking #12:
- Reported current head: 79f1687.
- Reviews used: 8.
- New TOTAL review cap: 10.
- Add 30 validation minutes, increasing this assignment's allowance
  from 55 to 85 minutes.

These are maximum allowances, not work that must be consumed.

I authorize supported resumption of the current blocked enrollments.
This amendment also covers one further supported continuation per repo,
if needed to address the listed findings or direct regressions from their
correction, within the two-submission limit and validation allowance.

Preserve every stop, review, finding, counter and spending record.
Use documented controller operations; do not edit state or disable guards.
Apply mission amendments and cap changes separately where required.

No further extension beyond these limits is authorized.

TARGET OUTCOME

Bring both existing PRs to AWAITING_OWNER_REVIEW.
Do not merge or release TRE or Waking in this continuation.

Use one coherent correction batch per repo. Do not build a separate full
production candidate for each finding.

TRE — THREE REQUIRED FINDINGS

R1-F4: complete the existing package-to-producer comparison.

- Use the source-to-package copy mappings already present in the build
  and packaging code.
- Cover repository files shipped under different paths, including
  assets/game/icons → assets/icons.
- Resolve accepted filename spellings to their canonical source paths,
  or explicitly refuse unsupported spellings.
- Do not classify a member as generated merely because its ZIP path
  does not appear verbatim in Git.
- Preserve the existing checkout-byte comparison and the separate
  controls for actual generated outputs.

Exercise:
- A clean package.
- A changed remapped icon, with its source restored before the receipt
  decision.
- An edited case-variant README.
- The existing unshipped-review-note control.

The actual ordinary writer or reader must refuse the altered packaged
content and identify the affected file.

R7-F1: bind content inspection to the receipt's expected ZIP digest.

- Pass the expected SHA-256 into the content comparison.
- Hash and inspect members through the same held read handle, or use
  an existing verified private copy that establishes both identities.
- Apply this in the writer and both staging admission readers.
- Do not rely on separate reads of a mutable filename.

Reuse the clean and edited ZIP fixtures to show:
- A matching archive succeeds.
- Replacing the pathname's archive after the earlier digest measurement
  cannot make content inspection approve a different ZIP.

R7-F2: retain one complete, admissible final candidate.

- Correct the driver's retention arrangement BEFORE the final build.
- Keep the final producer worktree and canonical candidate layout at
  the location bound by its receipt.
- Include every required validation summary, selected-test log and
  other sidecar, with their recorded identities.
- Keep destructive and negative fixture variants in a separate
  disposable area.
- Do not rewrite receipt paths or weaken RequireCurrentMaterialInputs
  to make a relocated or incomplete copy appear admissible.

After fixture cleanup, run the real development reader at the retained
location with the correct purpose and RequireCurrentMaterialInputs.

Record its returned producer, ZIP digest, DLL digest and MVID.
The four-loader audit must identify that same retained DLL.

TRE execution order:

1. Batch mapping, digest-binding, call-site and retention corrections.
2. Run lightweight focused ZIP checks.
3. Verify working directory, current reference-bundle identity and
   required host conditions.
4. Build one complete final candidate.
5. Run the affected checks using that candidate.
6. Clean only disposable fixture variants.
7. Demonstrate admission at the retained canonical location.
8. Audit that DLL on the four agreed profiles.
9. Verify exported evidence digests and run the required focused gate.
10. Submit the review.

Preserve the verified R6-F1, R6-F2 and R6-F3 corrections.
Refresh evidence only where changed inputs require it.
R7-F3 remains optional and nonblocking.

HOST AND DRIVER DISCIPLINE

One foreground coordinator launches expensive validation for this mission.
Other workers may prepare changes, but must return them for scheduled
builds and gates.

Before an expensive invocation:
- Set and verify the intended repository working directory.
- Verify the selected reference bundle using existing checks.
- Check the existing host requirements.
- Ensure the final candidate will survive fixture cleanup.
- Start complete stdout/stderr and exit-status capture.

Do not overlap this mission's expensive validations.
Do not repeatedly launch builds while another session makes the host
unavailable. Continue independent nonconflicting work where possible.

Do not stop another session's game/process, bypass a host guard, or build
new host-coordination infrastructure.

Count all validation spending, including failed runs and direct probes.
Preserve prior charges and distinguish waiting from actual validation.

SCOPE AND STOP CONDITIONS

Keep the existing frozen scope:
- UMM 0.31.1 minimum and pinned compile baseline.
- Agreed static loader checks.
- Correct packages, receipts and evidence.
- The listed defects and direct regressions from their correction.

No new gameplay work, general build-system or provenance redesign,
controller changes, new libraries, or native-test campaign.

Native gameplay and combined-stack testing remain NOT RUN for this mission.
No installation, game launch, save changes, live UMM changes, use of another
enrollment's game copy, or Nexus publication.
Memory Leak Fix remains excluded.

Accepted optional findings and deferrals remain nonblocking.
Do not reopen the five completed repositories.

Stop each remaining repo at AWAITING_OWNER_REVIEW.
Stop at the stated review or validation limits if unresolved.
A blocker in one repo must not prevent independent authorized work in
the other.

Update FLEET-STATUS and mission memory with:
- The five completed repositories.
- This new amendment ID and its one-time allowance applications.
- TRE and Waking's actual source/archive heads and controller states.
- Resolved and remaining required findings.
- Producing commits, candidate locations, artifact identities, checks
  and actual spending.
- Any exact host, permission or cleanup blocker.

Do not mark the seven-repository mission complete while required work
remains.
----- END OWNER AMENDMENT 4 -----
```

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
