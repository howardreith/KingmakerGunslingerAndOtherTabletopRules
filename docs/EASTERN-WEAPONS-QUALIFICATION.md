# Eastern Weapons qualification

This is the cumulative qualification ledger for release `0.0.80-eastern-weapons`.
PASS is recorded only for an observed completed surface; pending work remains
explicit.

## Current qualified candidate

- Repository validation: PASS.
- Dependency-free domain/reflection suite: PASS, `1046/1046`.
- Clean exact-reference Release build and build-output validation: PASS.
- Strict standalone package validation: PASS.
- Package: `KingmakerGunslinger-0.0.80-eastern-weapons.zip`.
- DLL SHA-256: `183DC903A64959C76A1A5063665E496A2E3FF2DF2BB575C99DA90092A3DEF8B4`.
- DLL MVID: `34061378-e413-4359-a96d-7952301e0efd`.
- Package SHA-256: `7D89041D27EBCE75BD397769D2681D2F9C9513D3BC409213B4DC3BC648FA5D70`.
- Eastern bundle SHA-256:
  `39884FF681EE553DE957E36E01B350AB926A452F994C4E8D33015D57D4EAD1EC`.

## Runtime results

- Focused Eastern combat: PASS,
  `20260814T1513175535242Z-6d95393a15c44b96a168cb21132fee19`.
- Elven Branched Spear combat regression: PASS,
  `20260814T1515370965718Z-017e199cb77b4af891868cec2d3a840b`.
- Vendor/loot observer, module ON: PASS,
  `20260814T1531432806171Z-d6638ced8af7472fabeb9b65f2c233c7`.
- Vendor/loot observer, Eastern module OFF: PASS,
  `20260814T1534024888613Z-29a2ce31f1db4aa7bedec9c2c14e6047`.
- Working-save prepare: PASS,
  `20260814T1546412271086Z-ec58de23ddad45c4b9c57c2594065f28`.
- Module-disabled working-save verification and cleanup: PASS,
  `20260814T1551228284204Z-e4d26f6cabb740c2a9dbbacb4785055e`.
- Final working-save absence verification: PASS,
  `20260814T1554167770731Z-9695a053916645e3b3cc4a44ceae8c29`.
- Loaded identity matches the built and installed DLL: PASS.
- Mods-tree restoration for both transactions: PASS.
- Save interaction: exactly two correlated writes to `KMG_AUTOMATION_WORKING`;
  the protected baseline remained distinct and untouched.

The focused Eastern run proves exact family/catalog identity, corrected static
proficiency names, Wakizashi/Katana/Nodachi proficiency behavior, Katana grip
authority, native Wakizashi finesse, all five bespoke effects, capstone
effective-bonus/Speed references, and complete fixture cleanup.

## Pending final qualification

- Expanded live fighter-group, generic-selector, all-named-property, capstone
  attack-count, and transformation controls.
- Call of the Wild, Arms and Armor, negative-control, and maximum combined
  compatibility profiles.
- All 64 feature-module states.
- Canonical non-mutating working-save smoke.
- Final sealed package identity comparison.
- Human subjective visual acceptance.

## Working-save persistence

The three guarded fresh-process phases passed with exact object-reference
correlation to `KMG_AUTOMATION_WORKING` and an independently resolved, distinct
protected baseline descriptor:

- Prepare refused pre-existing fixtures, then added one instance of every one
  of the 30 item blueprints and the exact Wakizashi/Katana Exotic Weapon
  Proficiency facts to deterministic owners. Every item retained its exact
  blueprint, family type, and stable category. One correlated native save write
  occurred.
- With Eastern Weapons disabled, all 30 items and both facts deserialized
  exactly once, while public selector, campaign, commerce, loot, and custom
  presentation publication was absent. Cleanup removed only those exact items
  and facts and performed one correlated native save write.
- After settings restoration, a third fresh process observed zero feature-owned
  items and facts and performed zero save writes.

No parameterized chosen-weapon fixture was persisted. The installed generic
selectors manufacture parameterized feature instances dynamically and expose no
ordinary stable blueprint identity suitable for safe save-backed test ownership;
the mission permits omission where no safe ordinary fact construction exists.
The settings file was restored byte-for-byte to SHA-256
`2E53FA0A09C56662434F6EA548FF5EBCF91F5AAF293D668248221239A1308655`.
