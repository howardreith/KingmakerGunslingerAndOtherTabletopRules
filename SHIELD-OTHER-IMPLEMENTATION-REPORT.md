# Shield Other Implementation Report

Status: REGRESSION REPAIR SOURCE-QUALIFIED; RUNTIME QUALIFICATION PENDING

Release: 0.0.77

Frozen base: `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57`

Branch: `codex/shield-other-spell`

## Delivered implementation

### 2026-08-11 casting and UI repair

The original 0.0.77 blueprint incorrectly replaced the donor's non-null empty
material-component data with `null`. Kingmaker and CotW dereference that object
while evaluating spontaneous spell availability, producing a continuous
exception storm in action-bar slot updates. This made the cast icon inert and
destabilized the spellbook/sidebar UI. The repaired blueprint owns an empty
`MaterialComponentData`, which represents no required item without violating
the engine invariant. The runtime scenario now evaluates the same native
`AbilityData` availability path.

The player-facing platinum-ring note was removed. Shield Other now uses its own
project-owned icon for both the ability and target buff rather than the Shield
of Faith donor icon.

- Independent, default-enabled, restart-required `Shield Other` module. Schema
  2 migrates schema-1 files enabled, defaults missing fields enabled, uses safe
  malformed-file recovery, and atomically saves immutable-process pending UI.
- Stable identities:
  - `KMG.Spells.ShieldOther.Ability` / `6a8c4c1d2fbe4d6a9a724988c1348401`
  - `KMG.Spells.ShieldOther.TargetBuff` / `7bd92e3c44ad42e7b523ee8ed7afc602`
- Ledger: 255 identities, 254 active registrations, one reserved. Registration
  is constant across all eight module combinations and independent of
  publication.
- Level-2 Abjuration spell: standard action, native close range, allied
  non-self harmless targeting, and one hour per caster level. The buff grants
  +1 deflection AC and +1 resistance to all saves. The paired 50-gp platinum
  ring focus is abstracted because Kingmaker has no safe paired-focus inventory
  convention.
- Transactional level-2 publication to Cleric, Paladin, Inquisitor, Community,
  and Protection. Final-live structural discovery supports unambiguous CotW
  Oracle, Warpriest, and Psychic lists without a compile-time CotW reference.
- Native `MechanicsContext` persists caster, subject, and caster level. Recast
  replaces one subject's prior link; one caster can link multiple subjects;
  bonuses do not self-stack.
- Links end on expiration, dispel/removal, missing or dead endpoints, different
  areas, or distance beyond caster-level-scaled close range. Validity is checked
  periodically and immediately before transfer.

## Damage interception

Exact Kingmaker 2.1.7b IL established
`RuleDealDamage.OnTrigger -> DamageBundle.CalculateDamage ->
RuleDealDamage.ApplyDifficultyModifiers -> RuleDealDamage.DealHitPointsDamage`
as the finalized-HP boundary. Interception occurs at `DealHitPointsDamage`
before target HP mutation and downstream death, concentration, or on-damage
observation. Kingmaker 2.1.7b has no native `RedirectionTarget` or
`RedirectedPercent` equivalent.

For finalized HP damage `D`, the subject receives `floor(D / 2)` and the caster
receives the remainder: 1 becomes 0/1 and 3 becomes 1/2. The caster share avoids
a second defense evaluation, uses normal temporary-HP/lethal HP application,
has a dedicated log message, and is protected by an exception-safe thread-local
guard. Original riders execute once; transfer cannot recurse or create another
attack, save, critical, precision, poison, bleed, trip, or source rider.
Non-HP effects, ability damage/drain, negative levels, death effects, and
Constitution-derived maximum-HP changes are excluded.

## Qualification completed before final repetition

- Version-aware validation and deterministic suite: 981/981 PASS.
- Clean exact-reference Release build and strict standalone package: PASS.
- All eight standalone module combinations: PASS with 254 registrations and
  isolated publication surfaces.
- Expanded disposable scenario: PASS for damage types, mitigation, immunity,
  temporary HP, lethal transfer, recursion, lifecycle, and cleanup.
- CotW x2 and highest-risk combined x2 passed on exact release source with
  exact Mods restoration after every transaction.
- Guarded two-launch `KMG_AUTOMATION_WORKING` persistence: PASS. Fresh load
  reconstructed caster, subject, and CL 5, proved 3 HP -> 1/2, restored HP,
  removed the link, and saved clean. The protected baseline remained unchanged.

## Current release artifacts

- Package: `artifacts/packages/KingmakerGunslinger-0.0.77-complete-maintenance-loop-smoke-test.zip`
- Package SHA-256: `b1b79a8c57b44e13fc2ed7343c2ad75bf32df575a7ede1a89dfca0bd76b6e646`
- Release DLL SHA-256: `602a4f689b7fa41684dbc402bc4f223ce16e8e50691f0a1369e162cb5f3d8b06`
- Runtime package SHA-256: `36bc4ea0d92685a1d81b366fa218c1ad22d30d47e94bf68269358ff337bc05c7`

## Final release evidence

- Release-source SHA: `80d468ee22835d6c258041cd63f2cd288ef0401e`.
- Eight-module matrix PASS IDs: `20260810T1627445514271Z`,
  `20260810T1630083823184Z`, `20260810T1632305619801Z`,
  `20260810T1634384073420Z`, `20260810T1636470659096Z`,
  `20260810T1638554577488Z`, `20260810T1641029948281Z`, and
  `20260810T1643115666580Z`, all with suffix
  `-observe-feature-module-settings`. Settings restored exactly to SHA-256
  `a08c80d5e877d4f6b5deea1247a9764bb48c08031b90da91a6e24d953e465ba6`.
- Standalone x2 PASS: `20260810T1645354769017Z-disposable-shield-other` and
  `20260810T1647523323239Z-disposable-shield-other`.
- CotW 1.14.4c-2.1 x2 PASS/restored:
  `compat-20260810T164943Z-046b85f47e64` and
  `compat-20260810T165221Z-e48789eb06ec`.
- Highest-risk x2 PASS/restored: `compat-20260810T165454Z-9ceec19dd586`
  and `compat-20260810T165726Z-dbba50e40dcd`.
- Exact combined DLL hashes: CotW
  `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`;
  Arms and Armor 1.0.10
  `cec7c177819f8f68adac4cb24df5834c862d0930d77305655ac3195097e33733`;
  Toggle Custom Soundpacks 1.0.1
  `a2582533dfdff82d1ece3ec51d931d72d7c8aac9a1302c219fcd8fca070c9434`.

## Adaptations and limitations

- The paired 50-gp platinum-ring focus is abstracted; no material is consumed.
- Native buff dismissal/removal is used; no bespoke multi-target dismissal UI.
- No unambiguous Friendship or Martyr final-live structure was found, so
  neither was guessed or mutated.
- Optional CotW publication requires one unambiguous structural match and
  otherwise fails closed without affecting other modules.
- No merge or pull request was created.
