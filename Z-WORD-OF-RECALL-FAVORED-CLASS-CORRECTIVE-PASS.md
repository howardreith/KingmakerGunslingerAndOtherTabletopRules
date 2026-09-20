# Word of Recall Favored Class corrective pass (owner review R1-R4)

## R1 — variants publication transaction

- Failing regression (red demonstration): with the exact shipped 0.0.133
  publication/rollback sequence temporarily installed in the extracted
  transaction class, four behavioral regressions failed — second-pass
  replacement of the exact assigned array, already-correct array
  replacement plus needless cache invalidation, missing rollback after a
  failed validation, and missing foreign-mutation refusal
  (`Completed 1669 tests; failures=4`; log retained at
  `C:\Dev\KingmakerGunslingerLab\private\fcb-variants-red-evidence.log`).
- Corrective change: the publication executes through
  `FavoredClassVariantsTransaction` (production class exercised by the
  domain suite): it retains the exact assigned array, captures the prior
  variants array and the prior item-cache value before mutation, restores
  exactly the owned state when validation fails after assignment, refuses
  to overwrite a genuine foreign mutation while preserving the original
  failure as the inner exception, and returns the same instance (no
  write, no cache invalidation) for an already-correct array.
- Passing evidence: domain suite 1,669 PASS including the seven new
  behavioral cases; runtime observer still `variantRecall=1` after two
  idempotent passes.
- Boundary: rollback is proven against the production class in the domain
  harness; a live in-game induced failure was not manufactured.

## R2 — complete target validation

- Demonstrated gap: the shipped resolver took the first structurally
  matching child and treated a present-but-wrong-type identity as
  absence.
- Corrective change: `ResolveFavoredClassLevelFeature` validates the
  exact selection identity and type, the parent relationship, selection
  settings, the shared-list membership gate, every child's structural
  binding and its actual `LearnSpellParametrized` grant configuration,
  with no first-match privilege (two qualifying children are ambiguous).
  Distinct outcomes and diagnostics: `favored-class.absent` (info),
  `favored-class.malformed` / `favored-class.ambiguous` (failures),
  `favored-class.published` / `unchanged` / `failed`. The installed
  prerequisite and restricted selections are only ever read.
- Passing evidence:
  `teleportation.favoredClass.ResolutionDistinguishesAbsentMalformedAndAmbiguous`
  plus the reconciler contract tests; live resolution unchanged
  (observer 13/13).
- Boundary: none identified.

## R3 — strengthened tests without feature expansion

- Demonstrated gap: prior regressions tested the List-based merge helper
  and source tokens; the committed ordinary-choice check repeated
  `ExtraSelected.Length == 0`.
- Corrective change: behavioral regressions exercise the production
  transaction and the game-free `FavoredClassTargetPolicy`; the committed
  fixture asserts the installed at-level allowance plus exactly one FCB
  grant with distinct new entries and no unfilled ordinary slot. The new
  invariant immediately caught the cumulative-delta misreading
  (`newSixth=3;ordinaryNew=2;allowanceDelta=0`) which was corrected to
  the installed at-level semantics.
- Passing evidence: `fcb-aasimar-committed` observed
  `newSixth=3;ordinaryNew=2;allowanceAtFourteenth=2;extra=0` PASS in both
  final acceptance runs; non-invariant values are captures only.
- Boundary: none.

## R4 — real persistence acceptance

- Change: guarded two-phase scenario
  `disposable-word-of-recall-favored-class-persistence` plus driver
  `scripts/Invoke-WordOfRecallFavoredClassPersistence.ps1`; prepare
  commits the genuine award and writes one new native manual save through
  the guarded disposable lease with disk payload/header verification and
  a receipt; verify runs in a fresh process, reloads exactly that save
  (provenance: prior PASS receipt, dll hash, distinct process, save
  hash) and asserts Recall at Oracle 6, the granting parametrized feature
  with its Recall parameter, award accounting, and one strategic cast
  (`exact=True`) with the save-write guard intact. Protected saves and
  settings are preserved; only the proven transaction-owned save is
  deleted afterwards.
- Passing evidence: transaction
  `word-of-recall-fcb-persistence-20260920T0401079546815Z_c87ca63ff44b44e69bdbe11fdde271f6`
  `passed: true`; prepare 6/6, verify 8/8.
- Boundary: the preview round-trip remains supplementary coverage.

## Per-run qualification mapping (final binary)

All runs loaded commit `2e78313c`, version 0.0.133, deployed DLL SHA-256
`4f0cf61017cfd473bfb73eaac855db6eb1c85ea87a64399ab42a5832b92c6134`
(equal to the final clean Build-Local DLL hash), clean tree, package
`KingmakerGunslinger-0.0.133-local-runtime.zip`
(`9883514c10f6ca44549f757ceb2b5b00ef023ddf36c87802a10631164f74a4fe`).

| Run | Evidence directory | Status | Assertions |
| --- | --- | --- | --- |
| Graph observer | `20260920T0400172443116Z-observe-word-of-recall-favored-class` | PASS | 13/13 |
| FCB persistence prepare | `20260920T0401102916481Z-disposable-word-of-recall-favored-class-persistence` | PASS | 6/6 |
| FCB persistence verify (fresh reload + cast) | `20260920T0402070769267Z-disposable-word-of-recall-favored-class-persistence` | PASS | 8/8 |
| Level-up acceptance A | `20260920T0405558044290Z-disposable-teleportation-level-up` | PASS | 67/67 |
| Level-up acceptance B (consecutive fresh process) | `20260920T0408505863354Z-disposable-teleportation-level-up` | PASS | 67/67 |
| Scroll eligibility control | `20260920T0411433479073Z-disposable-teleportation-scrolls` | PASS | 65/65 |

Superseded artifacts (not qualification of the final binary): the
0.0.133-candidate runs at commits `44c8d5f..632531cb` (66/66, 67/67 and
65/65 on those binaries, recorded in git history) — the final binary
re-ran every scenario above.

## Source gates (final binary)

Domain suite 1,669/1,669; repository validation through
`tools/validate_word_of_recall_favored_class133.py` PASS; clean
exact-reference Release build and strict package validation PASS.

## Additional boundary note

Invalid guarded requests leave the game process alive by design; two
stuck processes from rejected persistence requests during development
were terminated manually before the qualifying runs.
