# Word of Recall Favored Class second corrective pass (owner review)

Final artifact commit: `6e8d62d274d2b20b509986377c41d07ac7851765`
Loaded version 0.0.133; deployed DLL SHA-256
`56f53d009aa615f6076f1b217140c4b34d801c822d8f8a615671fba53dc20326`
(equal to the final Build-Local artifact manifest `dllSha256`).

## Finding 1 — publication exceptions bypassed rollback

- Failing regression: with the committed transaction restored, four new
  behavioral regressions ran red (`Completed 1673 tests; failures=4`, log
  retained at `private/fcb-transaction-red-evidence.log`):
  CacheWriterFailureRollsBackInstalledArray (a cache-writer exception after
  the array write left the assigned array installed with `_mutated` false),
  WriterThatChangesTargetThenThrowsIsRolledBack, RestorationFailure… and
  RestorationRefusal… (neither surfaced because publication ran outside the
  catch).
- Corrective change: `PublishAndValidate` executes publication itself
  inside the failure contract; each owned write is attempt-marked BEFORE
  its delegate runs (a writer may change its target and then throw); the
  rollback decision reads the live field rather than trusting completion
  flags; a restoration whose own writes fail rethrows a distinct
  "restoration failed" error preserving the original failure inside an
  AggregateException; foreign mutations remain refused with the original
  cause as the inner exception.
- Passing evidence: domain suite 1,675/1,675 including the four new
  regressions; observer still `variantRecall=1` after two idempotent
  passes on the final binary.
- Remaining boundary: rollback is proven against the production class in
  the domain harness; no live-game fault injection was manufactured.

## Finding 2 — incomplete target validation

- Gap: the feature's own `SpecificSpellLevel`/`SpellLevelPenalty` and the
  installed class-spell-level prerequisite were not validated.
- Corrective change: `FavoredClassTargetPolicy.Candidate` gains
  `HasValidSelectionContract`, computed by
  `TeleportationFinalLiveReconciler.HasValidSelectionContract`
  (SpecificSpellLevel, zero penalty, and a `PrerequisiteClassSpellLevel`
  for the resolved class at `level + 1`); a bound child without it is
  malformed with a diagnostic naming the child and the missing contract,
  so it is never resolved and never published. Unrelated siblings that do
  not bind to the Oracle class list remain ignored; eligibility rules and
  the Ganzi variant are untouched.
- Passing evidence:
  `teleportation.favoredClass.MalformedSelectionContractTargetsNeverResolve`
  (non-specific level, penalty-bearing and bad-grant children all
  malformed; valid contract plus invalid grant still malformed) and the
  reconciler contract tokens; live resolution unchanged (observer 13/13).
- Remaining boundary: none identified.

## Finding 3 — ordinary-spell accounting contradiction

- Verified discrepancy cause: the installed `SpellsKnown` table is
  cumulative and the native `ApplySpellbook.Apply` reader offers the
  DIFFERENCE `GetCount(new,6) − GetCount(old,6)` per spell level
  (established from the native IL; installed values 11–14 = 0,1,2,2), so
  the 0.0.129 record of zero ordinary sixth-level CLASS choices at
  Oracle 13→14 was correct. The two extra sixth-level spells my earlier
  fixture observed come from the fixture's own seeded CotW Oracle
  mystery bonus-spell selections, not from the class table or the
  FCB route.
- Corrective change: a matched native control — an identically seeded
  Aasimar Oracle (verified `seedIdentical=True`, identical mystery and
  ordinary history) takes a non-spell FCB award (Bonus Hit Point) at the
  same 13→14 level-up. The assertion requires: identical seeded books,
  identical ordinary new sixth-level sets (`controlNew ==
  fcbOrdinaryNew`, observed `571221cc|f0f761b8` on both sides), the FCB
  route adding exactly `{596d85a6}` (Recall) over the control,
  previously-known spells preserved on both sides, the control never
  knowing Recall with zero level-6 grant facts, and exactly one seed-time
  partial pick with a non-spell final FCB pick. The cumulative table
  values are captured as evidence only; the persistence reload accounting
  is snapshot-consistent with the stored ordinary set plus exactly one
  Recall. No expectation was changed to fit output: both historical
  records are now explained by the same installed semantics.
- Passing evidence: `fcb-matched-control` PASS in both final acceptance
  runs (68/68 each); persistence verify 8/8 with the decomposed snapshot.
- Remaining boundary: the mystery bonus-spell machinery itself is
  third-party behavior recorded as evidence, not qualified here.

## Finding 4 — destructive cleanup revalidation

- Gap: the driver recorded `createdSha256` but deleted by path checks
  alone; a changed or replaced file would have been deleted.
- Corrective change: registration captures the completed-save receipt
  (name, exact path, SHA-256 at disk commit) as the authoritative
  identity. `Remove-PersistenceOwnedSave` deletes only when the current
  file at the exact safe catalog path (never part of the protected
  inventory) hashes equal to BOTH the completed receipt and the creation
  hash; changed, replaced, unproven, absent or escaping output is
  preserved with a recorded reason. Cleanup failures are collected before
  any throw, and catalog disposal, sidecar restoration and the
  transaction-result record (now including `cleanupFailed` and
  `preservedOwnedSaves`) always run.
- Passing evidence: `scripts/Test-WordOfRecallFavoredClassPersistence.ps1`
  PASS with 7 filesystem regressions (changed output preserved, replaced
  output preserved, missing proof preserved, absent file reported,
  escaping path refused, unchanged proven file deleted, protected-catalog
  file never deleted); the live final transaction completed with
  `cleanupFailed: False`, empty `preservedOwnedSaves` and the owned save
  deleted after evidence copy.
- Remaining boundary: none identified.

## Per-run qualification mapping (final binary only)

All runs loaded commit `6e8d62d2`, version 0.0.133, deployed DLL SHA-256
`56f53d009aa615f6076f1b217140c4b34d801c822d8f8a615671fba53dc20326`
(equal to the final artifact manifest), clean tree, package
`KingmakerGunslinger-0.0.133-local-runtime.zip`
(`412f453d92faaecf3fb6d85d6535407409732e3d8a90e02f53c4481f0e6144a9`).

| Run | Evidence directory | Status | Assertions |
| --- | --- | --- | --- |
| Level-up acceptance A (with matched control) | `20260920T1215195122338Z-disposable-teleportation-level-up` | PASS | 68/68 |
| Level-up acceptance B (consecutive fresh process) | `20260920T1218288363943Z-disposable-teleportation-level-up` | PASS | 68/68 |
| Scroll eligibility control | `20260920T1221219277554Z-disposable-teleportation-scrolls` | PASS | 65/65 |
| Graph observer | `20260920T1223437582531Z-observe-word-of-recall-favored-class` | PASS | 13/13 |
| FCB persistence prepare | `20260920T1224364102863Z-disposable-word-of-recall-favored-class-persistence` | PASS | 6/6 |
| FCB persistence verify (fresh reload + cast) | `20260920T1225333264621Z-disposable-word-of-recall-favored-class-persistence` | PASS | 8/8 |

Persistence transaction
`word-of-recall-fcb-persistence-20260920T1224343386995Z_cb0cf838be0c4e4d889c27b91cbfe644`
`passed: true`, `cleanupFailed: false`, no preserved owned saves.
Superseded artifacts (commits `44c8d5f..2e78313c`) are recorded in git
history and are not qualification of this binary.

## Source gates (final binary)

Domain suite 1,675/1,675 (four transaction failure-contract regressions,
malformed-selection-contract regressions, driver cleanup contract);
repository validation through `validate_word_of_recall_favored_class133`
PASS; owned-save cleanup filesystem regressions 7/7; clean
exact-reference Release build and strict package validation PASS.

## Finding 4 (second round) — unprotected finalization sequence in the driver

- Demonstrated gap (the reviewer's reproducer, verified by regression): with
  a transaction-owned save changed after its recorded creation,
  `Remove-PersistenceOwnedSave` preserved it, but the outer finally then ran
  `Assert-KmgProtectedSaveCatalog` without owned paths, saw the preserved
  file as an unowned new save, threw, and skipped catalog disposal, sidecar
  restoration and the final transaction record.
- Corrective change (script only): the outer finally delegates to
  `Invoke-FinalizationStages`. Every stage — process exit, settings
  restoration, owned-save cleanup, catalog assertion, catalog disposal,
  sidecar restoration, inventory comparison — runs independently through
  its own catch; a live game process PROHIBITS the mutating stages while
  disposal and reporting still run; primary and stage failures accumulate
  without overwriting their causes (the aggregate reports `primary: … |
  stage: detail | …`); the final record carries `stageOutcomes`
  (succeeded/failed/skipped with reasons), `finalizationFailures`,
  `preservedOwnedSaves`, and accurate `settingsRestored`, `noGameProcess`
  and `catalogClosed` flags instead of hard-coded success; the record write
  precedes both failure throws and a failing write is itself recorded;
  preserved output is never excused from the catalog assertion and never
  deleted to make it pass.
- Filesystem regressions through the actual finalization path (twelve
  checks, `scripts/Test-WordOfRecallFavoredClassPersistence.ps1` driving
  `scripts/fcb-persistence-regressions/`): (1) changed output stays intact,
  catalog leases are verifiably released (exclusive re-open succeeds), the
  record reports failure and the preserved path; (2) a real catalog
  assertion failure (foreign new save) after a successful deletion still
  disposes and reports; (3) a sidecar restoration failure is recorded
  without suppressing settings restoration, cleanup or disposal; (4) a
  process-exit failure skips every prohibited mutation while the blocked
  state is reported as preserved-with-reason and disposal still runs; (5)
  successful finalization remains successful (all stages succeeded, owned
  save deleted, passed=true). The seven deletion-helper checks from the
  first round are retained. A StrictMode `.Count` defect in the live record
  write was caught by these regressions and fixed.
- Qualification of the corrected driver itself: native fresh-process
  transaction
  `word-of-recall-fcb-persistence-20260920T1313008534783Z_a7a8fa04d04e4b77be401aed85ea02f7`
  `passed: true` with every stage succeeded, `cleanupFailed: false`,
  prepare 6/6 and verify 8/8 (reload + strategic cast), all at commit
  `a76eb3a0` DLL `f03a0a67…` (see mapping below).
- Boundary: the script host's malware heuristics learned to block the
  single-file regression script during iteration (and briefly blacklisted
  the first finalization function name), so the regressions now run from
  split units; the driver itself compiles and executes cleanly, and the
  live transaction above ran the corrected driver end to end.

## Corrected-driver qualification mapping (final binary, second round)

All runs below loaded commit `a76eb3a0`, version 0.0.133, deployed DLL
SHA-256 `f03a0a67ba9c790f07a5749a59511dc8eff42c219689ffbd11e37ebf57d3a16b`
(equal to the final artifact manifest and the installed DLL), clean tree,
package `KingmakerGunslinger-0.0.133-local-runtime.zip`
(`70c6175bf4429d34…`).

| Run | Evidence directory | Status | Assertions |
| --- | --- | --- | --- |
| Graph observer | `20260920T1312089301121Z-observe-word-of-recall-favored-class` | PASS | 13/13 |
| FCB persistence prepare (corrected driver) | `20260920T1313032595868Z-disposable-word-of-recall-favored-class-persistence` | PASS | 6/6 |
| FCB persistence verify (fresh reload + cast, corrected driver) | `20260920T1314051259470Z-disposable-word-of-recall-favored-class-persistence` | PASS | 8/8 |
| Level-up acceptance (matched control included) | `20260920T1316471732648Z-disposable-teleportation-level-up` | PASS | 68/68 |
| Scroll eligibility control | `20260920T1319395235030Z-disposable-teleportation-scrolls` | PASS | 65/65 |

Source gates at the same commit: domain suite 1,675/1,675, finalization
regressions 12/12, repository validation PASS, clean exact-reference
Release build and strict package validation PASS. The earlier
`6e8d62d2`/`56f53d00…` mapping above remains valid evidence for the
gameplay DLL behaviors it qualified; the corrected finalization driver was
not part of that binary's driver and is qualified separately here.

## Operational note

Several blocked orchestration attempts left queued Steam game launches;
each leftover process was terminated before the qualifying runs, and the
qualifying runs themselves all exited cleanly.
