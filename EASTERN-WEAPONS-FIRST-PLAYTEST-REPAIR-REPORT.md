# Eastern Weapons First Human Playtest Repair Report

Status: implementation and structural qualification in progress; corrected
candidate assets are built, with the second subjective human review pending.

## Human findings

- Accepted: Nodachi in Weapon Focus; Katana and Wakizashi under Exotic Weapon
  Proficiency; Improved Critical category-glyph presentation.
- Repaired and automated: Call of the Wild Focused Weapon eligibility and
  mechanical damage-die behavior for all four KMG custom categories.
- Repaired and automated: seven measured diagonal inventory icons; thinner
  curved asymmetric single-edged family meshes; Wakizashi forward visual donor;
  Nodachi two-handed sword visual donor; family-level item override
  normalization; all-30 instantiated-prefab audit.
- Pending human recheck: every subjective silhouette, stance, attack-edge, and
  icon-taste check. Structural automation will not be reported as subjective
  acceptance.

## Focused Weapon

KMG publishes ordinary persistent feature children into CotW's exact merged
selection and reuses CotW's own Weapon Focus prerequisite and weapon-damage-die
component. The optional selection is never added to the unconditional custom
parameterized-selector path. CotW absence leaves inert persistent identities
for save safety and performs no optional selector lookup.

Guarded observers passed standalone and CotW profiles. The final save-free CotW
combat run passed four individual matching Weapon Focus controls, a no-match
control, a multiple-match control, actual 2d8 high-level damage-die replacement
for all four categories, and request-local cleanup. Exact evidence is recorded
in `docs/EASTERN-WEAPONS-IMPLEMENTATION-EVIDENCE.md`.

## Asset repair

- Icons: all six Eastern icons and the Elven Branched Spear icon are exact
  128x128 transparent RGBA, measured at 42 degrees above horizontal with the tip
  upper-right and butt lower-left.
- Geometry: the three blade planforms are asymmetric, curved single edges with
  a distinct cutting-edge material, restrained tips, oval tsuba, and wrapped
  elongated grips. Length and identity-root contracts are unchanged.
- Visual donors: Wakizashi uses Scimitar
  `d9fbec4637d71bd4ebc977628de3daf3`, Katana uses Bastard Sword
  `d2fe2c5516b56f04da1d5ea51ae3ddfe`, and Nodachi uses Greatsword
  `5f824fbb0766a3543bbd6ae50248688f`. Only presentation fields are inherited.
- Family normalization: live reflection proves this installed
  `BlueprintItemWeapon` has no item-level `m_VisualParameters` field. The
  scenario records that each item's public effective visual equals its family
  type, plus prefab identity, donor
  contract, materials, enchantment overlays, and cleanup for all 30 items.
- Eastern bundle: 147,724 bytes,
  `F58801B7B34514B06577EA9CE36F2F3FC0A79A6F157113EA227251BFE2A15B43`.
- Spear preservation: accepted FBX `8A79...474B` and bundle `3AB5...0EBE`
  remain byte-identical; only its deterministic icon sources changed.

## Targeted human recheck

- generic and +1 Katana silhouette;
- generic and +1 Wakizashi silhouette and forward main/offhand grip;
- generic and +1 Nodachi silhouette;
- representative named item from each family and Heaven's Measure;
- Katana one-handed and two-handed;
- Nodachi idle and combat;
- cutting-edge direction during attacks and trail origin;
- all seven diagonal icons;
- Focused Weapon four matching Weapon Focus controls and no-focus control;
- Improved Critical regression.

## Remaining qualification

Full runtime regression, compatibility profiles, three-phase persistence,
64-state matrix, working-save smoke, final artifact hash seal, PR update, and
the targeted subjective human recheck remain open until their evidence is
recorded below.
