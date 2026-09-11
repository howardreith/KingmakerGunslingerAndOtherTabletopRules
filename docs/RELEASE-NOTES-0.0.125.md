# Release notes — 0.0.125-settlement-button-width

Owner follow-up mission: one cosmetic fix. The owner screenshot showed the
relabeled native **Settlement Teleport** button extending beyond the
narrow native button behind it; the owner otherwise accepts the
teleportation implementation. This release widens that existing native control so the full
label fits — nothing else in the popup changes.

## What changed

1. **Settlement Teleport button width.** While spell rows coexist with the
   native settlement-teleport control, the relabeling path now also widens
   the very same native button: the target width is the label's preferred
   width at the native font size plus the control's own native padding,
   never narrower than the native control and never wider than the settled
   native action region (the parchment content region Travel and Cancel
   already occupy). The button keeps its exact label wording, font,
   styling, artwork, height, row centering, native callback, eligibility,
   and resource behavior. The visible background/border and the clickable
   area expand together because the native control's own rect — not a text
   rectangle — is what widens, under the layout constraints that already
   govern it (a LayoutElement content-width constraint is set when a parent
   layout controls the width, the settled rect size when it does not).
2. **Exact restoration.** Every layout value the widening touches is
   recorded per panel instance — the settled rect width plus the
   LayoutElement values, including whether the element was added solely for
   this adjustment — and restored alongside the original label, both
   restored byte-for-byte, when the mod augmentation withdraws; a layout
   component added solely for the adjustment is removed. Opening, closing, changing destinations, and
   reopening cannot accumulate adjustments or duplicate components: each
   cycle measures from the pristine native width the previous dismissal
   restored, and a second open cycle reproduces the same settled width.
3. **Unchanged on purpose.** No parchment enlargement, no Travel/Cancel
   change, no spell-row resizing (their settled-extent measurement still
   sees the pristine native width), no font shrinking, truncation,
   wrapping, or rewording, no cloned or replaced action, no shared-prefab
   or localization-asset change, no specialist-slot, spell-transaction,
   confirmation, arrival-notification, or movement-arrow change, and the
   direct Greater Teleport behavior keeps its no second confirmation
   settlement.

## Verification summary

- 1,576 deterministic domain tests PASS (two new: settlement button width
  policy — label preferred width plus native padding, grow-only, region
  cap — and its fail-closed unproven-geometry contract).
- Clean Release build and repository validation PASS at the release
  identity.
- The guarded `disposable-teleportation-coexistence` scenario now asserts
  the widened geometry structurally: the settled button covers the label's
  preferred width plus its native padding, stays centered inside the
  native action region, leaves every neighboring native action and the
  appended rows at exactly their settled extents, restores the exact
  native width and layout components on dismissal, and reproduces the
  identical width on a second open cycle — with the native callback
  intact and no travel or resource spend from layout or reopening.
- Visual acceptance at the owner's resolution/UI scale remains pending
  owner review: the automated proof is structured geometry, not an
  eyeball.

## Upgrade

Install **KingmakerGunslinger-0.0.125-settlement-button-width.zip**
through Unity Mod Manager and restart the game. Existing module settings and
saves are preserved; nothing about this change persists in saves.

## Retained baselines

This Kingmaker Gunslinger 0.0.125 release carries the retained qualification
counts forward: the inherited Gunslinger-fixes baseline of 1,288 tests, the
fatigue-authority baseline of 1,325 tests, and the current deterministic suite
of 1,576 tests all pass.

The installable archive is
`KingmakerGunslinger-0.0.125-settlement-button-width.zip`. The qualified
firearm SoundBank is retained unchanged: `KMG_Firearms.bnk` SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Foreign assemblies — `CraftMagicItems.dll` remains an externally installed
optional mod and is never bundled.
