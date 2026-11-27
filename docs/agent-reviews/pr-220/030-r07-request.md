<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r07-request part=1/1 sha256=2bfd31860cdfa5cb58e44207097cb70fb119e069b0147e8d9514bcf85679e56e -->
### Claude Opus — review request — round 7 — candidate `ffdd7fa7a5`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6-r07-c9fb20` |
| Checkpoint | `final` (final) |
| Candidate H | `ffdd7fa7a5a4db10741cb0def47e0885c8e96835` |
| Target B (`master`) | `329ad1b0738b0060f180df8931d41b3c7fb00430` |
| Merge base M | `35763430acc9eefc306b861045526a444690bc77` |
| Mission | `kingmaker-umm-0311-mission-20261123` sha256 `f7d990f396` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 3, sha256 `9097a57f4916be5ded05516c07c171aa348a98820c6c4fb272d692a68282a69e` |
| Developer observed | foreground session `e178a93b-ca53-4b6b-a4d2-a357d48eaa77`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 7 of 8 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Prepared under owner amendments revision 3 sha256 9097a57f4916be5ded05516c07c171aa348a98820c6c4fb272d692a68282a69e. Round-7 corrections for Tabletop Rules Expansion PR #220 under owner amendment 3: R1-F4, R6-F1, R6-F2, R6-F3 as one batch. Code and tests c0b54ba4ad; ffdd7fa7a5 adds evidence and documentation only.

R1-F4: Assert-KmgPackagedRepositoryFilesMatchCommit (scripts/ReferenceProvenance.Common.ps1) compares every packaged file tracked at the producing commit with that commit's file as checked out, taken from git archive of the commit (the repository's attributes and line-ending configuration, no other normalization; the working tree is not read). Build-Local runs it before writing any receipt from a clean start; Read-KmgBuildLocalManifest runs it for staging admission on both the ordinary and development routes. R6-F3: Set-LcGameCopyMods removes the admitted private copy after the destination check and before discarding the previous Mods, inside the rollback; the restoration is verified before a failure is reported, and Remove-LcAdmittedCopy reports a removal that did not happen. R6-F1: the round-7 index rows now carry the original capture digest, name the prefix-replaced intermediate, and give the committed Git blob digest; the two build logs were re-exported from their raw captures with readable exit trailers. R6-F2: the four-profile static audit ran on the DLL inside the round-8 frozen candidate. The 62/63 admission run at 42e9da86d6 remains not an overall pass; the earlier focused successes stay attributed to b16ce7a47c. The original 467-second failure remains of unknown cause. Native runtime NOT RUN.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 --gate focused -- -Filter loader.production-dll-references-pinned-loader | yes | pass | 5.0s | Controller receipt gate-focused-ffdd7fa7a5a4-d88e49a1250-a32501bde64-20261126T234932698989Z.log on ffdd7fa7a5: production compiled with the pinned loader references, the named loader test selected. |
| Focused driver Invoke-KmgFocusedR8.ps1 on c0b54ba4ad (exported as focused-r8-driver.ps1.txt) | no | pass | 441s build plus fixtures | docs/evidence/umm-0311/focused-r8.json (driver docs/evidence/umm-0311/focused-r8-driver.ps1.txt: one Build-Local development build of c0b54ba4ad in a temporary detached clone, then the committed Test-CandidateAdmission regions focused-packaged-content and focused-tree-probe run unmodified on that frozen candidate; 11 of 11 passed). Complete build output: docs/evidence/umm-0311/development-build-r8.txt. The whole admission suite was not rerun (the new cases are in it; their committed code ran through the driver). |
| LoaderApiAudit on the round-8 frozen candidate's DLL (four profiles) | no | pass | 1s | docs/evidence/umm-0311/api-audit-r8.json: producer c0b54ba4adac0ad0cc101cd44b434cef7c07a33f, ZIP KingmakerGunslinger-0.0.195-development-umm-0311-admission-c0b54ba4adac.zip SHA-256 d86d9a7cdd4a66f763b8ee77f8c1e22c67e54c0a8feb844af35e29d6ec12caa1, DLL SHA-256 71dfdd7231e8b5858aa27c36d0c0022c3a1a421c9621081a46e807e3bfe9c281, MVID d055b414-77a8-4373-a5a2-46af8a140424; 0.31.1 RESOLVED; 0.32.4 RESOLVED; owner-0.33.0 RESOLVED; 0.33.4 RESOLVED (23 types, 67 members). Static metadata resolution, not a game run. |
| Packaged-content helper probes on the published v0.0.195 ZIP against its tag commit ef0e1fe15d (uncommitted batch) | no | pass | 78s | Genuine release ZIP: 159 tracked files compared, all equal; the same ZIP with an edited README entry: refused naming README.md. Recorded in the mission ledger only (a first per-file version took 72 s and was replaced by the git archive version). |
| Focused driver attempts 1-3 on c0b54ba4ad (no candidate) | no | fail | 848s | Counted failures: (1) 344 s - the driver passed a stale default private reference bundle (no Managed/UniRx.dll), Build-Local refused in 5 s after the clone (docs/evidence/umm-0311/development-build-r8-attempt1.txt); (2) 190 s - the repository source-only tests refused because another session started Kingmaker during the build; (3) 314 s - the driver ran from a working directory outside the repository, and QualifiedRelease.Common.ps1 calls git rev-parse without -C, so Test-Sprint34Release failed; diagnosed with a 27-second isolated run of that test before the fourth attempt. None is a product failure; logs (2) and (3) are retained in the fleet record. |
| Native checks | no | not_run | 0 | Not authorized. |

**Evidence references:**
- prepared under owner amendments revision 3 sha256 9097a57f4916be5ded05516c07c171aa348a98820c6c4fb272d692a68282a69e
- docs/evidence/umm-0311/INDEX.md
- docs/evidence/umm-0311/focused-r8.json
- docs/evidence/umm-0311/development-build-r8.txt
- docs/evidence/umm-0311/api-audit-r8.json
- docs/UMM-LOADER-COMPATIBILITY.md

**Prior findings to assess:** `R1-F4`, `R6-F1`, `R6-F2`, `R6-F3`

**Owner amendments to the mission — revision 3, captured-source sha256 `9097a57f4916be5ded05516c07c171aa348a98820c6c4fb272d692a68282a69e`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

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
```

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
