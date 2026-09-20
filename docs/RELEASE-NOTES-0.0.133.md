# Release Notes 0.0.133 — Word of Recall Favored Class Selection

Version: `0.0.133-word-of-recall-favored-class`

## Summary

An Aasimar (or Human) Oracle using the Favored Class mod could see Word of
Recall among the sixth-level favored-class bonus spell choices but could
never actually pick it. This release repairs that pick gate for the favored class route.

## Root cause

The Favored Class per-level bonus-spell features are native
`BlueprintParametrizedFeature` clones bound to the live Call of the Wild
Oracle class spell list. Since 0.0.126 the final-live reconciliation
publishes the canonical Word of Recall into that list and clears the
native filtered cache, so the level-up extraction correctly **lists** the
spell. However, `BlueprintParametrizedFeature.CanSelect` admits a pick
only when the exact Feature+Param item exists in `get_Items()` — the
`m_CachedItems` array rebuilt from `BlueprintParameterVariants`, a
load-time snapshot that never contained the reconciled spell. The visible
choice therefore always failed the native pick gate.

Runtime evidence on the owner's installed profile:
`fcb-level6-full-items-recall ... items=2966;recall=0;variants=2966;variantRecall=0`
while the genuine extraction listed the canonical ability exactly once.

## Repair

`TeleportationFinalLiveReconciler` now also reconciles the optional
Favored Class integration: after the existing Oracle list pass, it
resolves the installed `FavoredOracleOracleSpellListBonusSpellFeatureSelection`
by exact identity and structure (eight per-level LearnSpell features bound
to this Oracle class and this exact class list), merges the canonical Word
of Recall into the `BlueprintParameterVariants` of exactly the per-level
feature whose shared list level already contains it, and clears that
feature's `m_CachedItems`. The pass runs twice idempotently, preserves all
foreign snapshot entries, rolls back on failure, and remains a safe no-op
when Favored Class or Call of the Wild is absent, the TeleportationSpells
module is off, or the structure differs. After the repair the same
observation reads `items=2967;recall=1;variants=2967;variantRecall=1`.

## Runtime qualification

The guarded `disposable-teleportation-level-up` scenario was extended with
a genuine favored-class phase on fresh request-local native Oracles:
Aasimar route (favor Oracle, earn the half-spell credit, open the real
award, see canonical Word of Recall exactly once among 24 native
candidates, pick it, cancel, reopen, commit), the shared Human route, the
no-credit control, the below-prerequisite control at Oracle 12, the
already-known duplicate control, native serialization persistence of the
committed parametrized selection, world-map casting of the learned spell,
and full restoration. The full scenario passed **66/66 assertions** with
zero exceptions on the owner's installed mod stack, alongside all
pre-existing ordinary-learning, scroll-eligibility and level-up controls.

## Compatibility

- No spell, scroll, or selector GUID changes; no new blueprint identities.
- The deliberately restricted Ganzi enchantment favored-class variant is
  untouched.
- Favored Class and Call of the Wild remain optional load-time
  integrations; Gunslinger never modifies or redistributes their binaries.
- Ordinary Oracle learning, scroll eligibility and native Cleric 6 /
  Druid 8 publication are unchanged (regression suites PASS). No Craft
  Magic Items contract changed and production keeps no static
  `CraftMagicItems.dll` dependency. No
  Craft Magic Items contract changed ( references
  are untouched).

## Corrective pass

After owner review, the variants publication was rebuilt as an owned array
transaction (exact assigned-array retention, prior cache capture, correct
rollback, foreign-mutation refusal, idempotent no-op on an already-correct
array — four behavioral regressions demonstrated failing against the
shipped sequence), the per-level target resolution now fully validates
identity, parent relationship, selection settings and the actual
LearnSpellParametrized grant configuration with distinct absence,
malformed and ambiguity diagnostics, the acceptance fixture asserts the
installed at-level ordinary allowance plus exactly one favored-class
grant, and a real authorized disposable save-file persistence acceptance
now proves the learned spell, granting feature and parameter, award
accounting and one strategic cast through a fresh-process reload. Every
qualification scenario was re-run on the final binary; see the per-run
mapping in Z-WORD-OF-RECALL-FAVORED-CLASS-CORRECTIVE-PASS.md. A second
review pass additionally placed publication itself inside the failure
contract (cache-writer and write-then-throw failures roll back; restoration
failures preserve both exceptions), validated the per-level child's
selection-side contract (specific level, zero penalty, class-spell-level
prerequisite) separately from the grant component, replaced the allowance
assumption with a matched native control after verifying from the native
reader that the installed table is cumulative (the 0.0.129 zero-choice
record was correct; the observed extras are mystery bonus spells), and
hardened the owned-save destructive cleanup with receipt-hash
revalidation, preservation of changed or replaced output, and filesystem
regressions.

A final script-only pass placed the persistence driver's cleanup
finalization under per-stage protection: every remaining stage runs
independently, a live game process prohibits mutating stages while
disposal and reporting continue, primary and stage failures accumulate
without losing their causes, the final record reports accurate stage
outcomes before any aggregate propagates, and preserved owned output is
never excused or deleted to satisfy the catalog assertion. Twelve
filesystem regressions run through the actual finalization path, and the
corrected driver was re-qualified by a fresh native persistence
transaction.

## Boundaries

- The owner authorized publication of this release on 2026-09-20 after
  accepting the corrective-pass review; the release ships the owner
  authorized finalization-protected driver qualified natively.
- The full compatibility-profile matrix was not rerun for this release;
  the focused qualification ran on the live installed owner profile with
  Favored Class and Call of the Wild enabled
  (`compatibilityRuntimeQualificationPending` remains true).
- Persistence is proven both through the guarded disposable save-file
  fresh-process reload with a strategic cast (`exact=True`) and through
  the native save-format serialization
  round-trip of the committed unit (preview rebuild); a dedicated
  save-file reload campaign was not run.

- Historical suite checkpoints of 1,251, 1,288 and 1,325 cases remain

## Artifact identity

- Build label: Kingmaker Gunslinger 0.0.133
- Installable archive: `KingmakerGunslinger-0.0.133-word-of-recall-favored-class.zip`
- Qualified firearm SoundBank SHA-256 unchanged:
  0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18
