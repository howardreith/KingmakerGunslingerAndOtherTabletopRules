# Elven Branched Spear qualification

## Scope and revision

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`
- Upstream project: `KingmakerGunslingerAndOtherTabletopRules`
- Base: `6357b8cb27b92f6974ff61409c7aaffb7f2c3cdc`
- Feature branch: `codex/elven-branched-spear`
- Final mechanics/runtime-test revision: `f8dbcb98485c385b7365e206ec150bb2ded9aa0a`
- Module: `elven-branched-spears`, label **Elven Branched Spears**, default ON
- Category: `0x004b4d47`; one family for all 12 items

## Delivered profile

The medium base weapon is 20 gp, 10 lb., 1d8 piercing, 20/x3,
two-handed, exotic, Spears-group, six-foot engine reach, non-thrown, and
finesse-compatible. Elven Weapon Familiarity grants the exact category through
the native feature; blanket Martial proficiency does not. The family includes
mundane, masterwork, cold iron, masterwork cold iron, +1, +1 cold iron,
Boughkeeper, Thornstep, Moonlit Fork, Viper's Reach, Briar-Crowned Spear, and
Spear of the First Branch. Brace is neither implemented nor claimed.

The +2 movement-AoO attack modifier is established only while the exact
`UnitAttackOfOpportunity` constructed inside `UnitCombatState.Disengage` runs.
The rule also requires `RuleAttackWithWeapon.IsAttackOfOpportunity` and the
equipped spear family. Ordinary attacks and directly queued nonmovement AoOs
remain unmodified. The combat log source is the inherent weapon enchantment.

## Selectors and Dexterity

Exactly one spear option is published in Exotic Weapon Proficiency, Rogue
Finesse Training, Weapon Focus, Greater Weapon Focus, Improved Critical,
Weapon Specialization, Greater Weapon Specialization, Sword Saint Chosen
Weapon, and Weapon Mastery. The weapon type is in the native Spears fighter
group. Fencing Grace and Slashing Grace expose zero spear options.

Final isolated Call of the Wild run
`20260814T0332236294874Z-disposable-elven-branched-spear-combat` passed 17/17
assertions with zero warnings. It proved Strength baseline, Weapon Finesse
attack only, native Finesse Training on all 12 items, Agile alone, Agile plus
Training without double damage, two equivalent category facts without double
damage, native Longspear switching, feature removal/reselection, Fighter's
Finesse, Trained Grace, and Dervish Dance exclusion. The exact optional profile
was Call of the Wild 1.14.4c-2.1, DLL SHA-256
`4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`;
transaction `compat-20260814T033132Z-c013c61d4afe` restored the prior Mods tree.

## Mechanics and named items

The same final combat run exercised real attack, damage, AoO, sneak-damage,
save, buff, and command events for all six named items. It observed:

- Boughkeeper: one +1 Dodge AC buff after an AoO hit; no miss/ordinary trigger;
- Thornstep: one -10 Speed effect after a movement AoO; no nonmovement trigger;
- Moonlit Fork: native Agile and native cold iron together;
- Viper's Reach: one -2 Reflex effect only after 15 applied sneak damage;
- Briar-Crowned: one generated -5 attack, one native AoO consumed, no recursion;
- First Branch: DC 15 in the fixture, native Entangled on failure, -10 Speed on
  success, one combined round marker, and no secondary-damage recursion.

All refreshable effects remained count one and all request-local units, facts,
items, commands, memory, and presentation objects were cleaned up.

## Campaign and module publication

The placement manifest records four verified vendor tables and four fixed-loot
tables. Module-ON publication contains exactly 24 count-one vendor rows and four
count-one fixed-loot rows. Module OFF contains zero new acquisition rows and
zero new selector entries while retaining all 12 item identities and the native
familiarity category.

All 32 five-module states passed guarded fresh-launch runtime qualification from
commit `ce33383fc209c04aa1412410a2bacec2cc3d68dd`. Evidence begins at
`20260814T0148218696367Z-observe-feature-module-settings` (all ON) and ends at
`20260814T0300217767370Z-observe-feature-module-settings` (all OFF). A
deterministic audit found 32 results, 32 unique masks, all PASS, no duplicates,
and restored settings SHA-256
`dc76b429302838c52895d1901ac7488bc58e9d18a01b8e584968497cdb30c50c`.
One intervening launch exited before scenario execution because Steam could not
load `steam_api64`; its exact `off-off-on-on-off` mask passed on immediate
guarded retry and the incomplete launch is not counted as qualification.

## Save safety

The clean-commit three-phase sequence passed:

- prepare: `20260814T0322279668156Z-working-save-elven-branched-spear-prepare`;
- module-OFF verify/cleanup:
  `20260814T0325232210401Z-working-save-elven-branched-spear-verify-cleanup`;
- final no-write absence:
  `20260814T0328087214273Z-working-save-elven-branched-spear-verify-absent`.

The module-OFF load deserialized exactly 12 items, one shared category, and the
exact selected Finesse Training child. Cleanup removed all 13 references and
performed one correlated write to `KMG_AUTOMATION_WORKING`; the final load
observed zero items, zero feature facts, and zero writes. The protected baseline
was never selected or written. Final canonical general smoke
`20260814T0334532807442Z-working-save-smoke` also passed from `f8dbcb9`.

## Assets and packaging

The project-owned original mesh is generated by the checked-in Blender script:
900 triangles, 15 mesh objects, original `.blend`, deterministic FBX, original
icon, and a dedicated Unity 2018.4.10f1 bundle. Bundle SHA-256 is
`3AB56092F363AA96C627287095E2CA549EEA7ED50D39C73BCD943646BFBE0EBE`.
The final combat run selected the validated custom prefab. Missing, corrupt, or
rejected bundle/prefab/material/transform data retains the native Longspear
presentation without changing mechanics or saved blueprint identity.

The final clean Release gate passed repository validation, all 1,028
dependency-free tests, build-output validation, package generation, and strict
standalone UMM package validation. The qualified package path is
`artifacts/packages/KingmakerGunslinger-0.0.78-expanded-summoning.zip`; SHA-256
after release-document reconciliation is
`ECD7EF9D1F1434D0BB5A4D689D152BB3851C2157854229FF70A25AFF0F88DE45`.

## Remaining limitation

Deterministic mesh, material, transform, anchor, prefab, icon, fallback, equip,
attack, and cleanup checks pass. Human aesthetic acceptance across body types,
armor silhouettes, size-changing effects, and every animation remains
visual-only review; it does not block or disable mechanics. No Brace or
pseudo-Brace system exists.
