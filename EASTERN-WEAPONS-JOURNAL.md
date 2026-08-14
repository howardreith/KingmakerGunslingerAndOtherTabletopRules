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
