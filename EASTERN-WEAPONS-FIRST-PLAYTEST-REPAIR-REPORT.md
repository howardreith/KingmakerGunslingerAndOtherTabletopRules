# Eastern Weapons First Human Playtest Repair Report

Status: implementation in progress; corrected candidate not yet ready for the
second human visual review.

## Human findings

- Accepted: Nodachi in Weapon Focus; Katana and Wakizashi under Exotic Weapon
  Proficiency; Improved Critical category-glyph presentation.
- Repaired and automated: Call of the Wild Focused Weapon eligibility and
  mechanical damage-die behavior for all four KMG custom categories.
- Pending repair: seven diagonal inventory icons; thinner curved single-edged
  family meshes; Wakizashi forward stance; Katana/Nodachi edge and animation
  calibration; all-30 instantiated-prefab audit.
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

## Remaining work

The deterministic Blender and Unity asset repair, expanded all-item visual
observer, full source/package/runtime regression, compatibility profiles,
three-phase persistence test, 64-state matrix, working-save smoke, final hash
seal, PR update, and targeted human recheck checklist remain open.
