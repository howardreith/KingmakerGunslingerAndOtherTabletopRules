# Word of Recall Oracle Favored-Class Selection — Mission State

Branch: `codex/z-word-of-recall-favored-class`
Base: master `525625df` (release 0.0.132)
Version: 0.0.133-word-of-recall-favored-class (candidate)

## Owner request

An Aasimar Oracle can earn additional spells known through the Favored
Class mod, but Word of Recall is missing from the sixth-level favored-class
bonus spell choices. Make the genuine Favored Class bonus-spell mechanism
offer and teach canonical Word of Recall (Aasimar and shared Human routes).

## Baseline recorded (2026-09-19/20)

- Source: master `525625df`, version 0.0.132, clean tree.
- Installed: Call of the Wild 1.14.4c-2.1, Favored Class (ZFavoredClass)
  1.3.1, plus the owner's live mod stack; owner FeatureModules.json
  preserved exactly across deployments
  (`a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`).

## Static analysis (installed-assembly IL, private dumps)

- ZFavoredClass 1.3.1 ordinary Oracle route: per-level clones of the
  vanilla `MysticTheurgeInquisitorLevelParametrized1`
  (`bcd757ac2aeef3c49b77e5af4e510956`) store
  `SpellList = custom_spell_list ?? spellbook.SpellList` on the feature
  AND its replaced LearnSpellParametrized (verified live: both reference
  `f305174b73f64783a8379238a14c3283:OracleSpellList`, the same object as
  the Oracle class spellbook list).
- Verified installed identities: selection
  `FavoredOracleOracleSpellListBonusSpellFeatureSelection`
  `9ba3858327354e2093613efb9de198d7`, level-6 feature
  `7249760f01784ea997afaa9c433c2e68`, partial
  `b19759026d6508b9022f1edb4ec4b31f`, class selection
  `c6f18fa1194d0bfb35e1913983b8da98`, favored-class progression
  `52ee82659e040ed231ed36c8e5457e38` (picked through the vanilla
  BasicFeatsProgression `FavoredClassSelection`
  `27947ef789544982a437200c3189c59a`); Ganzi variant deliberately separate
  and untouched.
- Native desktop and console level-up pickers list candidates through
  `ExtractSelectionItems(before, preview)` → (LearnSpell type)
  `ExtractItemsFromSpellList(preview)` reading the feature's shared list
  `SpellsFiltered` minus already-known spells.
- **`BlueprintParametrizedFeature.CanSelect` admits a pick only when the
  exact Feature+Param item exists in `get_Items()` → `GetFullSelectionItems`
  → `m_CachedItems`, rebuilt from `BlueprintParameterVariants` — a
  load-time snapshot array, not the live spell list.** The UI click and the
  controller path both pass through this gate
  (`LevelUpController.SelectFeature → SelectFeature.Check → CanSelect`).
- `LearnSpellParametrized.OnFactActivate` grants through
  `DemandSpellbook(SpellcasterClass).AddKnown(6, spell)` with
  `SpecificSpellLevel` (component list unused on that branch).

## Runtime root cause (owner profile, guarded runs)

- `observe-word-of-recall-favored-class` BEFORE (run
  `20260920T0114458977400Z`): every structural link sound — feature list
  identity = live class list, Recall present exactly once at level 6,
  caches clean, level-7 prerequisite correct, not DLC-locked — but
  `fcb-level6-full-items-recall ... items=2966;recall=0;variants=2966;
  variantRecall=0`: the pick-gate snapshot never contained the reconciled
  spell. The 0.0.126 list reconciliation made Word of Recall **visible**
  in the sixth-level choices while the native pick gate silently refused
  it — exactly the owner's experience.
- `disposable-teleportation-level-up` BEFORE repair (run
  `20260920T0104067653489Z`): the genuine extraction listed
  `KMG_WordofRecall_Ability` exactly once among 24 native candidates, and
  the native parametrized pick failed at `SelectFeature.Check/CanSelect`.

## Repair

`TeleportationFinalLiveReconciler.ReconcileFavoredClass` (after the
existing list passes, twice idempotently): resolve the installed selection
by exact identity + structure (parametrized LearnSpell children bound to
this Oracle class and this exact class list), merge the canonical Word of
Recall into `BlueprintParameterVariants` of exactly the per-level feature
whose shared list level already contains it, and clear that feature's
`m_CachedItems`. Rollback restores the exact prior array; absent or
structurally different integration is a safe no-op; module gate and
duplicate fail-closed behavior unchanged.

## AFTER evidence (guarded runs, Steam App 640820, owner mod stack)

- Graph (run `20260920T0121263183172Z-observe-word-of-recall-favored-class`):
  **PASS 13/13**, `fcb-level6-full-items-recall
  items=2967;recall=1;variants=2967;variantRecall=1`.
- Full level-up acceptance (run
  `20260920T0129005692756Z-disposable-teleportation-level-up`): **PASS
  66/66, zero exceptions** through the genuine route on fresh request-local
  native Oracles:
  - Aasimar: favored Oracle selected natively; one partial pick completes
    the installed half-spell credit; no-credit control (award absent from
    the first selection's candidates); award → level-6 → Recall among 24
    native candidates exactly once; preview-only learning; cancellation
    consumes nothing; commit teaches canonical Recall once at Oracle 6
    with one grant fact, no extra ordinary choice; duplicate control
    (already-known excluded); native save-format serialization round-trip
    preserves the committed parametrized selection; world-map cast spends
    exactly one sixth-level spontaneous slot to the sanctuary.
  - Human: shared route equivalent (12→13 below-prerequisite control:
    award open but level-6 gate closed at max spell level 6; 13→14 commit
    teaches Recall once).
  - Cleanup: party, cross-scene, inventory, money and save-write guard
    restored; all 50 pre-existing ordinary-learning/level-up assertions
    PASS unchanged.

## Source gates

- New domain regressions: five
  `teleportation.favoredClass.*` cases (merge once/idempotent/fail-closed,
  reconciler contract, scenario contract, observer contract); full suite
  1,662/1,662 PASS.
- `tools/validate_word_of_recall_favored_class133.py` chained on the
  complete inherited validator chain; repository validation PASS.
- Clean exact-reference Release build and strict package validation:
  `KingmakerGunslinger-0.0.133-local-runtime.zip`.

## Honest boundaries

- `publicReleaseAuthorized` false pending owner approval; no public
  release, no merge, no branch publication beyond the guarded checkpoint
  push.
- Compatibility matrix not rerun; focused qualification ran on the live
  owner profile with Favored Class and Call of the Wild enabled
  (`compatibilityRuntimeQualificationPending` true).
- Persistence proven through the native save-format serialization
  round-trip of the committed unit (preview rebuild); a dedicated
  save-file reload campaign was not run.
- Owner-specific archetype/mystery/curse progression was not reproduced;
  the ordinary Aasimar and Human routes are qualified.
