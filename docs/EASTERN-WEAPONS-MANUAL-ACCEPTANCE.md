# Eastern Weapons manual visual acceptance

## Human aesthetic acceptance — 2026-08-21

- Reviewer: Howie Reith, project owner
- Qualified version: `0.0.89`
- Branch: `codex/weapon-presentation-calibration`
- Runtime-qualified source commit: `96d17e1bfaa1be2d2afa2e6758e4472a8a973f3f`
- Result: **HUMAN ACCEPTED FOR MERGE**

The project owner reviewed the deployed weapon-presentation build in game and
accepted the Eastern weapons visually. The finished presentation was judged
nearly perfect and comparable in quality to Owlcat's native work.

The katana handhold remains slightly offset. This is explicitly accepted as a
minor, native-comparable cosmetic imperfection rather than a blocker. No further
katana transform, grip, blade-plane, animation, held-model, or stored-model
adjustment is requested for `0.0.89`.

The authoritative human acceptance record for the complete firearm,
elven-branched-spear, and Eastern-weapon mission is:

`docs/WEAPON-PRESENTATION-HUMAN-ACCEPTANCE.md`

## Objective qualification retained

The 2026-08-21 calibration has guarded in-game held, stored, ready, attack,
movement, transition, body, and loadout evidence for all 12 Eastern variants and
three native controls. That evidence accepts the objective frame, grip,
cutting-edge polarity, variant identity, independent stored presentation,
donor preservation, and severe-clipping checks.

Human acceptance does not relabel every automated matrix cell as personally
observed. The precise automated-versus-observed scope remains recorded in
`planning/WEAPON-PRESENTATION-MATRIX.md` and
`docs/EASTERN-WEAPONS-VISUAL-CALIBRATION.md`.

## Optional expanded regression checklist

The cases below are retained for future regression passes and are not release
gates for the human-accepted `0.0.89` presentation:

- Compare generic and enchanted silhouettes within each Eastern weapon family.
- Recheck Wakizashi in main hand, offhand, and two-weapon full attacks.
- Recheck Katana in one-hand and two-hand presentation, including shield or
  offhand changes.
- Recheck Nodachi through idle, movement, full attack, critical, death, and
  weapon-set transitions.
- Recheck robes, light armor, bulky armor, male and female rigs, Small and
  Medium races, Enlarge Person, Reduce Person, Legendary Proportions, and
  weapon-retaining polymorph forms.
- Recheck trail origin, dropped-item rendering, inventory DollRoom visibility,
  material integrity, floor drag, hand separation, support-hand drift, and
  extreme transient clipping.
- Recheck inventory icons, Focused Weapon exposure, and Improved Critical
  presentation under their separate UI and feature acceptance scopes.

Mechanical acceptance remains tracked separately. This human acceptance changes
no source, assets, bundles, version, gameplay behavior, or artifact identity.
