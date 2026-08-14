# Eastern Weapons journal

## 2026-08-14 - repository gate

- Read the governing repository, architecture, runtime, working-save, and Elven
  Branched Spear implementation/qualification documents before source changes.
- Confirmed repository root
  `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger` and upstream
  `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Ran `git fetch --all --prune`.
- Confirmed local `master` and `origin/master` are identical at
  `4ffd15b09992bd9cee9d330eee0a650ad2c94661`, with divergence `0 0` and a
  clean worktree.
- Confirmed active version `0.0.79` and the complete landed Elven Branched Spear
  implementation. Per explicit user correction, the authoritative static child
  title is **Weapon Proficiency (Elven Branched Spear)**; new Eastern exotic
  children will follow the same structure.
- Repository validation passed.
- Complete clean Release domain/reflection suite passed: `1033/1033`.
- Clean exact-reference Release build and build-output validation passed.
- Created `codex/eastern-weapons` directly from the verified `origin/master`.
- No runtime launch, save access, package deployment, optional-mod transaction,
  or source implementation occurred during the gate.

## Next

Inspect the landed spear/custom-category, bootstrap, module, commerce,
runtime-testing, compatibility, packaging, Blender, and Unity implementations.
Then add and run the save-free `observe-eastern-weapon-contracts` investigation
before selecting production donors, native enchantments, effect hooks, category
values, or campaign targets.

## 2026-08-14 - investigation observer source

- Added `observe-eastern-weapon-contracts` as an autonomous, save-free guarded
  runtime scenario.
- The observer inventories all required weapon donor families, approved native
  enchantment families, installed proficiency/grip/combat/size rule types,
  compatible selectors, vendor tables, campaign loot, and direct owners.
- Added focused source guards that prohibit save, inventory, selector, loot, and
  blueprint mutation from the observer.
- Production category values, GUIDs, donors, enchantment references, prices, and
  campaign placements remain intentionally unselected pending live evidence.
- Repository validation passed; the complete suite passed `1034/1034`; the
  clean Release build, build-output validation, deterministic package creation,
  and strict standalone validation passed for the observer checkpoint.

## 2026-08-14 - observer timeout refinement

- The first guarded save-free run
  `20260814T1104529826303Z-b2fcb4605fd84bbabd97ad2bf6af9aa2` did not produce a
  result before the 120-second orchestration timeout. No save interaction
  occurred. The request was accepted, but the first runner update arrived only
  after the locally active compatibility stack completed its long blueprint
  initialization window.
- Narrowed the observer's expensive discovery predicates to component type
  names, restricted direct-reference scans to plausible campaign owners, and
  added phase timing events. No production choice or mutation was introduced.
- The refinement passed repository validation, `1034/1034` tests, clean Release
  build, deterministic packaging, and strict package validation.
- The mandatory guarded push helper rejected the exact authorized branch
  `codex/eastern-weapons` because its external allowlist contains only
  `codex/eastern-weapon`. No direct push or branch rename was used.

## 2026-08-14 - installed contract observation PASS

- Guarded save-free run
  `20260814T1110588439047Z-7f131097a8ca48ac916f675e77b57c47`
  passed on exact source commit `41b7687079f380a044ffed3a0bf0d3dac771228e`.
- Loaded version, DLL hash, and MVID matched the deployed candidate. No save
  interaction occurred and the guarded process exited after completion.
- Recorded exact identities for eight native weapon donors and sixteen approved
  native enchantment contracts, including Brilliant Energy's native Undead and
  Construct exclusions and Speed's native Haste-marked extra attack.
- Mighty Cleaving, Impact/size, member-level coup-de-grace, category collision,
  and bastard-sword grip authority require one narrower follow-up inventory;
  no production identity or donor has been chosen yet.
- Extended the same save-free observer with numeric category occupancy, every
  installed weapon-type group contract, alternate mechanic blueprint names,
  and loaded CLR types selected by declared member names. The follow-up source
  passed validation, `1034/1034` tests, clean build, deterministic packaging,
  and strict package validation.

## 2026-08-14 - targeted mechanic observation PASS

- Guarded save-free run
  `20260814T1119161920060Z-d07fac81ae644db0ac092e1fa3cfa3fe`
  passed on source commit `34f3093118ef028242f39e3f63e497a9c16a7580`.
- Loaded version `0.0.79`, DLL SHA-256
  `57B42B4F18FC05614AC7078564CB2D0A83536480A1C97CF3BBA1DA771FD32A7E`,
  and MVID `5337c8ba-2d31-4c60-a39f-34017ce40339` were recorded. No save
  interaction occurred.
- Proved category values `0x004B4D48` through `0x004B4D4A` are unoccupied
  across the live 136-weapon-type graph.
- Selected `ItemEntityWeapon.HoldInTwoHands` as the shared exact authority for
  katana proficiency and Moonlit Crossing. Selected
  `RuleCalculateWeaponStats.IncreaseWeaponSize` for Unfixed Form and exact
  `RuleAttackRoll` hit/confirmed-critical state for weapon-hit effects.
- No native Mighty Cleaving or Impact/Lead Blades enchantment contract exists
  in the installed graph. The native Cleave feats are not substitutes.
- Deadly is deferred because the installed coup-de-grace action graph exposes
  no reliable virtual-damage-only Fortitude DC rule hook.
- The guarded push helper continues to reject the exact authorized branch
  because its external allowlist contains only the singular branch spelling.

## 2026-08-14 - reusable category and sixth-module foundation

- Added the independent default-on `eastern-weapons` module, settings schema 5
  migration, immutable active/pending state, publication gates, UI label, exact
  six-Boolean guarded request contract, and 64-state matrix transaction.
- Added a data-driven custom-category definition and collision registry plus the
  three locked Eastern category profiles and twelve generic catalog records.
- Stable category values are `0x004B4D48` Wakizashi, `0x004B4D49` Katana, and
  `0x004B4D4A` Nodachi. Generic prices use base cost, base plus 300 gp
  masterwork, doubled base cost for cold iron, and base plus 2,300 gp for +1.
- Repository validation, all `1037/1037` domain/reflection tests, clean Release
  build, build-output validation, deterministic package creation, and strict
  package validation passed.
- The first six-module live run exposed a stale inherited spear observer
  expectation: the accepted merged path has one Exotic `AllFeatures` reference
  and two Finesse Training references, not four static references. No spear
  source or saved identity was changed; the observer was corrected to three.
- Corrected all-ON run
  `20260814T1137520825950Z-1974f02c68834bcb8c08805a3724c2cd`
  passed. Transactional Eastern-OFF run
  `20260814T1140244851767Z-123a6d83d9634e0f9f0de39a479164ff`
  passed and restored settings bytes with SHA-256
  `2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.
- Both runs loaded the exact local candidate with DLL SHA-256
  `5b74019e31f732eb08c56e5b615cbaf094c8fc120650b5eef78c32097cfe5048`
  and MVID `01a66571-5cbf-4c21-a386-74fda2042386`. No save was accessed.

## 2026-08-14 - stable generic weapon catalog

- Registered three production weapon types and all twelve generic items under
  every module state. Each family shares one stable category and weapon type
  across mundane, masterwork, cold-iron, and +1 forms.
- Wakizashi uses the exact locked light 1d6, 18-20/x2,
  piercing-or-slashing profile; Katana uses the locked versatile one-handed
  1d8, 18-20/x2 slashing profile; Nodachi uses the locked two-handed 1d10,
  18-20/x2 slashing-or-piercing profile. None has reach, thrown, or brace
  behavior.
- Masterwork and +1 reuse exact native enchantment blueprints. Cold iron uses
  the native item-level physical material field. Prices follow the documented
  base, 300 gp masterwork, doubled cold-iron, and +1 formula.
- Repository validation passed; the complete suite passed `1038/1038`; clean
  Release build, build-output validation, deterministic packaging, and strict
  standalone validation passed.
- All-ON run
  `20260814T1155410119533Z-cf305c900e5344d686ed42d62969399a`
  and Eastern-OFF run
  `20260814T1158109553961Z-ab0aed33e9304c618125dc1cfd1230cf`
  passed. The latter kept all 15 identities registered while suppressing
  Eastern presentation, then restored settings bytes exactly.
- The candidate DLL SHA-256 was
  `3946018FA2E1FCD1F19B13595D309391973D9404AB34F6E9DE09C47E9490760F`,
  MVID `6917d74c-2525-4677-b485-e5c36addf5e7`; package SHA-256 was
  `DD4C3C72641EEE50F42CA21B7C4D225D026BD666309DE5D3B2F2F498FDD6160F`.
  No save was accessed.
