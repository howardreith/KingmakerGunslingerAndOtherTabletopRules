# KingmakerGunslinger 0.0.61 First Playtest Repair Report

## Result

The functional, usability, and 2D presentation repairs are qualified on branch
`codex/first-playtest-repair`. Qualified baseline `0decb91` is an ancestor.
The only incomplete checkpoint is the legally sourced 3D firearm mesh described
in `planning/FIREARM-ASSET-INPUT-REQUIRED.md`.

## Defects, causes, and repairs

1. Repeated Light Crossbow feature icons: a global native fallback was assigned
   to player-facing facts. Replaced by project-original category/deed icons.
2. Overlong progression: implementation grants and individual deed carriers
   were exposed. Deeds are grouped at levels 1/7/11/15/19 and 19 helper facts
   are hidden; runtime top-level visible entries are now 27 (the old exposed
   graph contained 75 visible facts; the current full reachable graph is 82
   because it also includes the new feat choices).
3. No firearm Weapon Focus: native `WeaponCategory` parameterization cannot
   truthfully represent the five custom firearm identities. One grouped custom
   selection now provides Pistol, Musket, Blunderbuss, Rifle, and Revolver
   choices, with firearm proficiency, BAB +1, combat-feat publication, and an
   exact-kind +1 attack marker. Crossbows are unaffected.
4. No Rapid Reload: one repeatable grouped selection now supplies the same five
   exact-kind choices and drives the PnP action reductions. Native crossbow
   Rapid Reload is unchanged.
5-7. Pistol/ammunition icons and crossbow doll: original pistol, lead-ball, and
   powder icons replace temporary native fallbacks. The doll still uses the
   superseded crossbow mesh because no licensed local firearm model exists.
8-9. Literal `<null>` tooltips: exposed abilities lacked fields Kingmaker renders
   directly. Deadeye and Gunslinger's Dodge now use accurate `Until triggered`
   and `None` metadata; a general exposed-blueprint validator reports zero
   incomplete or literal-null tooltip fields.
10. One-count ammunition: native class starting items grant 1/1 and the exact
   level-one finalization patch adds 19/19 atomically. Runtime proves one pistol
   plus one stack each of 20 powder and 20 balls.
11-12. Pistol reload rejection: availability incorrectly required a full-round
   profile. Exact equipped-item resolution now selects one unambiguous firearm,
   derives its action from era/handedness/kind and matching Rapid Reload, and
   uses the existing rollback-safe ammunition/state transaction.
13. Automatic reload: the player receives one Reload Firearm variants parent.
   Hidden action implementations expose free/move/standard/full-round command
   types without four action-bar entries. Native right-click autocast selects
   the available child, queues at most one command, respects existing commands,
   and uses the same production transaction as manual reload.
14. ADR-0007 presentation fallback: superseded for production. Icons are fixed;
   the mesh portion remains the narrow external-input item.

## Reload matrix

| Firearm | Normal | Matching Rapid Reload |
| --- | --- | --- |
| Early one-handed (Pistol) | Standard | Move |
| Early two-handed (Musket/Blunderbuss) | Full-round | Standard |
| Advanced one-/two-handed (Revolver/Rifle) | Move | Free |

Reload fills the supported remaining capacity, consumes one compatible powder
and ball per round exactly once, rejects full/invalid/ambiguous states with a
specific reason, and rolls back inventory and state together on failure.

## Principal files changed

- `src/KingmakerGunslinger/Blueprints/Firearms/` and `Blueprints/Playtest/`
  for production firearm, feat, icon, tooltip, reload, and starting-item wiring.
- `src/KingmakerGunslinger/Domain/Firearms/ReloadActionEconomy.cs` for the PnP
  action matrix.
- `src/KingmakerGunslinger/RuntimeTesting/` for presentation, autocast, catalog,
  starting-stack, and working-save evidence.
- `assets/source/icons/` and `assets/game/icons/` for editable and exported art.
- `tools/validate_playtest61.py` and focused PowerShell/domain tests for source,
  package, tooltip, icon, action, feat, and safety contracts.
- `THIRD-PARTY-ASSETS.md`, `CHANGELOG.md`, and this report for provenance and
  qualification state.

## Artwork provenance

All 0.0.61 replacement icons are original project artwork generated for this
project from original prompts and exported as transparent 128x128 PNG files.
No third-party or compiled game/mod art was copied. Exact provenance is in
`THIRD-PARTY-ASSETS.md`. The runtime package resolves the named project icon
files; no prohibited Light/Heavy Crossbow or Diamond Dust fallback remains on
the audited production items/principal features.

## Qualification evidence

- Deterministic suite: 858/858 PASS (the pre-existing 854 plus four focused
  playtest-reload cases), with repeated top-level repository validation.
- Clean Release build and strict 28-file standalone UMM package validation:
  PASS.
- `20260803T0110347924965Z-gunslinger-starting-items`: PASS, exact 1/20/20.
- `20260803T0046109874070Z-disposable-reload-autocast`: PASS, exact native
  variant selection, one transaction, full/no-ammo retry suppression.
- `20260803T0112171221352Z-observe-gunslinger-presentation`: PASS, 27 top-level
  visible entries, 19 hidden helpers, zero incomplete visible facts/tooltips.
- `20260803T0114090225027Z-generic-firearm-actions`: PASS.
- `20260803T0115497054842Z-advanced-capacity`: PASS.
- `20260803T0120125540858Z-production-firearm-catalog`: PASS for all five exact
  production identities and native-crossbow isolation.
- `20260803T0034253550707Z-mod-load-smoke`: PASS.
- `20260803T0036120442708Z-observe-gunslinger-presentation`: earlier PASS used
  while repairing generalized tooltip metadata.

All cited working-save scenarios used the exact `KMG_AUTOMATION_WORKING`
receiver-bound path, excluded `KMG_AUTOMATION_BASELINE`, invoked no save-writing
API, and retained stable post-load fingerprints. Existing 0.0.60 working-save
loading is therefore compatible; no save schema, stable blueprint GUID,
firearm identity, or item-state identity changed.

## Package

- Path: `artifacts/local-runtime/0.0.61/KingmakerGunslinger-0.0.61-local-runtime.zip`
- Package SHA-256: `4f8bd050602d1a521f9a2a937eeb79b921b56856fe21f69dd5badb76214a82b4`
- DLL SHA-256: `118b90792f67b73aa2c22de6adea5ff806448908dd6af5143c16729c8b08aa4e`
- Functional qualification commit before this report: `92de6640d9806595e4023dcd71c7e99c4021e5f1`

Hashes and final commit are refreshed after the documentation checkpoint and
final clean rebuild.

## One-time supervised visual acceptance checklist

Install the final package and perform one visual inspection only:

- confirm class/proficiency/Gunsmithing/Grit/Deeds and deed icons are distinct
  and readable;
- confirm Pistol, Lead Ball, and Black Powder no longer resemble crossbow or
  Diamond Dust fallbacks;
- confirm Deadeye and Gunslinger's Dodge show no literal `<null>` fields;
- confirm the progression is grouped and choices remain visible;
- record the firearm doll mesh as pending until the licensed asset contract is
  fulfilled. Do not accept the current crossbow mesh as completion.

## Known limitation

The only known mission limitation is the missing legally sourced Early Pistol
3D model and Kingmaker asset bundle. Exact accepted input and import procedure
are documented in `planning/FIREARM-ASSET-INPUT-REQUIRED.md`.
