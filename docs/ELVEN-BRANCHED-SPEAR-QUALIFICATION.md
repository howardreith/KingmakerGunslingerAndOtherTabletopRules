# Elven Branched Spear qualification

## Scope and revision

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`
- Upstream project: `KingmakerGunslingerAndOtherTabletopRules`
- Base: `6357b8cb27b92f6974ff61409c7aaffb7f2c3cdc`
- Accepted pre-amendment checkpoint:
  `f5136b12ce91ec2f56d5c7bf8dcb52129418ec5d`
- Feature branch and draft PR: `codex/elven-branched-spear`, PR #3
- Final artifact source:
  `9a24147b717b2502442d48d5f2026becdaba4e8d`
- Module: `elven-branched-spears`, label **Elven Branched Spears**, default ON
- Category: `0x004b4d47`; one family for all 12 items

## First-playtest repair

The human playtest accepted feat availability, Weapon Finesse, native Rogue
Finesse Training damage, Piranha Strike, reach, combat, custom-model shape,
tested-character clipping, and hand/body alignment. The repair preserves those
contracts.

The raw category leak was repaired at the central
`StatsStrings.GetText(WeaponCategory)` boundary for only the owned stable value.
Parameterized children now publish native `FeatureUIData` with display name
**Elven Branched Spear**, acronym `EB`, no bespoke icon, and the native glyph
path. Exotic Weapon Proficiency shares the native parent/donor icon. Rogue
Finesse Training is exactly **Finesse Training (Elven Branched Spear)** and uses
the spear inventory icon. Native and optional categories are untouched.

Guarded run
`20260814T0444110998835Z-disposable-elven-branched-spear-combat` passed 18/18
and observed a human-readable prerequisite, native EWP icon, exact Rogue name,
spear Rogue icon, seven `EB` native-glyph selector rows, and no raw category or
firearm icon leakage. The same run re-proved Dexterity and combat behavior.

The final EWP follow-up was qualified with Call of the Wild active at
`20260814T1009047959759Z-disposable-elven-branched-spear-combat`. It passed
18/18 and observed the exact title **Proficiency (Elven Branched Spear)**, the
human-readable prerequisite, one EWP option, and merged selector indexes
`Elven Curve Blade=5` / `Elven Branched Spear=6`. Because Kingmaker renders
this native list in reverse, the spear appears immediately above Elven Curve
Blade. The serialized catalog legitimately has no Call of the Wild donor
anchor and remains a singular append; the merged catalog owns the exact
relative ordering.

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
Weapon, and Weapon Mastery. Their presentation follows each native selector's
own icon architecture rather than one spear-specific policy. The weapon type is in the native Spears fighter
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

The placement manifest records four verified base-campaign vendor tables, four
verified BTSL vendor tables, and four fixed-loot tables. The BTSL addition is
the six generic tiers only: one count-one row on each installed table, 24 rows
total. Guarded run
`20260814T0454174378820Z-observe-vendor-table-contracts` resolved both
standalone and both campaign tables, observed every row exactly once, preserved
all native rows and the existing 48 firearm rows, and passed. Module OFF adds no
acquisition or selector rows while retaining all 12 item identities and the
native-familiarity category. Focused ON/OFF runs
`20260814T0457017452463Z-observe-feature-module-settings` and
`20260814T0459247497589Z-observe-feature-module-settings` passed with exact
settings restoration.

All 32 five-module states passed guarded fresh-launch runtime qualification from
the final artifact source. Evidence begins at
`20260814T0512562299220Z-observe-feature-module-settings` (all ON) and ends at
`20260814T0623367488297Z-observe-feature-module-settings` (all OFF). A
deterministic audit found 32 results, 32 unique masks, one commit, one loaded
version (`0.0.79`), all PASS, no duplicates, and restored settings SHA-256
`dc76b429302838c52895d1901ac7488bc58e9d18a01b8e584968497cdb30c50c`.

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
The final non-mutating 0.0.79 smoke passed at
`20260814T0509599402694Z-working-save-smoke`; it positively correlated
`KMG_AUTOMATION_WORKING`, loaded through Steam, requested no save write, and
left the accepted three-phase spear persistence evidence intact.

## Assets and packaging

The project-owned original mesh is generated by the checked-in Blender script:
900 triangles, 15 mesh objects, original `.blend`, deterministic FBX, original
icon, and a dedicated Unity 2018.4.10f1 bundle. Bundle SHA-256 is
`3AB56092F363AA96C627287095E2CA549EEA7ED50D39C73BCD943646BFBE0EBE`.
The final combat run selected the validated custom prefab. Missing, corrupt, or
rejected bundle/prefab/material/transform data retains the native Longspear
presentation without changing mechanics or saved blueprint identity.

The stale Expanded Summoning package statement in the pre-amendment report was
not a spear artifact: package entry points and active version pins still used
the prior `0.0.78-expanded-summoning` identity. They now use assembly version
`0.0.79`, informational version `0.0.79-elven-branched-spear`, and exact archive
`KingmakerGunslinger-0.0.79-elven-branched-spear.zip`.

Final artifact identity:

- source commit: `9a24147b717b2502442d48d5f2026becdaba4e8d`, clean;
- assembly version: `0.0.79.0`;
- informational version: `0.0.79-elven-branched-spear`;
- DLL MVID: `2be784db-21c8-4bba-9590-214583811eb5`;
- DLL SHA-256:
  `70279B4C764E0E9FCCDF0CB56B886B5250421772EBFA6C46C79B90E41FC46FC4`;
- package SHA-256:
  `ECD656AF16C507192D2638F374743B46BE606B349EE365C15B07E0EBFC2FB3CE`;
- bundle SHA-256:
  `3AB56092F363AA96C627287095E2CA549EEA7ED50D39C73BCD943646BFBE0EBE`;
- installed DLL SHA-256: identical to the built DLL; and
- final exact runtime-loaded version: `0.0.79`.

Repository validation, all 1,033 dependency-free tests, clean exact-reference
Release build, build-output validation, package creation, and explicit strict
standalone validation passed. The 125-file archive contains the exact DLL,
dedicated spear bundle, 128px spear icon, `Info.json`, blueprint ledger/schema,
README, changelog, and all other allowlisted assets. Player-facing localization
and feature-module schema/defaults are compiled in the exact DLL. Mutable
`FeatureModules.json` is intentionally not packaged, so deployment preserves
the user's byte-exact settings; the final matrix restored hash
`DC76B429302838C52895D1901AC7488BC58E9D18A01B8E584968497CDB30C50C`.

The final installed Call of the Wild run passed 18/18 at
`20260814T1009047959759Z-disposable-elven-branched-spear-combat`. Its loaded
commit was `9a24147b717b2502442d48d5f2026becdaba4e8d`, and its cached, live, built, and
packaged DLL hashes are identical.

## Remaining limitation

Deterministic mesh, material, transform, anchor, prefab, icon, fallback, equip,
attack, and cleanup checks pass. The first human playtest accepted the model,
tested fit, and lack of observed clipping. All variants share one weapon type
and fit-proven prefab, so optional per-item material differentiation is deferred
rather than risking the accepted rig with a type split or renderer mutation.
Broader body-type, armor, size-changing, animation, and repaired-UI visual
acceptance remains human review. It does not block mechanics. No Brace or
pseudo-Brace system exists.
