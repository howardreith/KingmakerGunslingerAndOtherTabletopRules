# Z settlement-teleport button width — durable state

Companion mission record for the single owner-requested cosmetic fix: the
relabeled **Settlement Teleport** destination button extended beyond the
narrow native button behind it (owner screenshot). The owner accepts the rest
of the teleportation implementation; this mission widens that one existing
native control and nothing else.

## Repository / branch / artifact

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`
- Branch: `codex/z-settlement-teleport-button-width` (from
  `677ee31a` = master `398d2b7c` + the 0.0.124 release record)
- Active version: `0.0.125-settlement-button-width`
- Candidate package
  `artifacts/local-runtime/0.0.125/KingmakerGunslinger-0.0.125-local-runtime.zip`
  package sha256 `f65fd629714db7195d9f0c2c162603435aa572a71eecdbef32369237a4272bd2`
  DLL sha256 `ef5b05ff687ab8babdc8440d40cff2a3dbf5f79b0b9271b176a9ae0f0ee40da3`

## Change (exactly one cosmetic adjustment)

`WorldMapPointSpellActionRuntime.RelabelSettlementControl` — the path that
already relabels the native settlement-teleport button found through its
serialized `OnTeleportPressed` callback — now also widens that same control
per instance:

- Target width from `TeleportContextLayoutPolicy.SettlementButtonWidth`
  (domain-tested): the label's **preferred width at the native font size**
  plus the control's **own settled native padding** (pristine button width
  minus pristine label width, both captured before the longer text can
  disturb a content-driven label rect), **grow-only**, capped by the
  **settled native action region** (the world extent of the dialog's other
  active native action buttons — Travel/Cancel — with the appended rows and
  the settlement control itself excluded), converted into the button's local
  space.
- Applied under the constraints that actually govern the control: a
  `LayoutElement` content-width constraint (added if none exists, recorded
  either way) drives the width when a parent layout controls it, and the
  settled rect size is set in the same adjustment for the non-controlled
  case; one immediate layout rebuild settles the dialog.
- Verified on the settled geometry: the widened control must sit inside the
  native action region AND cover the label's preferred width plus the native
  padding. Any failure reverts every touched value and keeps only the
  already-accepted relabel (logged
  `destination.settlement-width-reverted`); geometry never partially
  overlaps.
- Restoration: `RestoreSettlementControl` returns the exact recorded rect
  width and `LayoutElement` values and destroys a component added solely for
  this adjustment, alongside the byte-for-byte label restore.
- No accumulation: every cycle measures from the pristine width the previous
  dismissal restored; `WidenedSettlement` is per-panel and guards duplicate
  application. Row sizing in `TeleportDestinationRows.Create` still measures
  the pristine native extent because widening always happens after `Create`
  and is always restored before the next `Clear`.
- Untouched: label wording, font, styling, artwork, height, centering,
  native callback, eligibility, navigation, resources, parchment size,
  Travel/Cancel, spell rows, all other gameplay systems. No new libraries.

## Guarded scenario coverage (coexistence gates extended)

The desktop `disposable-teleportation-coexistence` Gate 3 now asserts, in
addition to the existing label/callback gates:

- `settlement-label-coexists`: widened (grow-only), settled world extent
  covers label preferred width + native padding, centered inside the native
  action region, every neighboring native action's world extent unchanged,
  appended rows width unchanged, at most one layout component added, native
  callback intact.
- `settlement-label-restored`: exact label, exact native width, exact layout
  component count.
- `settlement-width-reopen`: a second open cycle (settlement control active)
  reproduces the same settled width with neighbors and rows unchanged —
  proves no cumulative growth.
- `settlement-width-restored-again`: the second cycle restores the exact
  native geometry again.
- `settlement-geometry` capture: width/original width/label preferred/native
  padding/extent/region/rows width/parent layout type/anchors/element count
  recorded as structured evidence.

## Validation completed (static)

- Repository validation PASS at 0.0.125 (new
  `tools/validate_settlement_button125.py` chain; older validators'
  version lattice updated).
- Full domain suite: 1,576 tests PASS (two new settlement-width policy
  tests).
- Clean exact-reference Release build + strict package validation PASS
  (Build-Local): both the standalone
  `KingmakerGunslinger-0.0.125-settlement-button-width.zip` and the local
  runtime package validated.

## Runtime evidence

- PENDING: guarded `disposable-teleportation-coexistence` run on the
  candidate artifact via Steam App 640820 / `KMG_AUTOMATION_WORKING`.

## NOT RUN / limitations

- Visual acceptance at the owner's resolution/UI scale remains pending owner
  review; the automated proof is structured geometry, not an eyeball. The
  machine's display settings are not modified autonomously.
- No release published, no merge; owner review required.
