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
- RUNTIME-QUALIFIED artifact (built from commit `d8887b3e` by the guarded
  orchestrator; installed == package == build exact-match guard PASS):
  package `artifacts/local-runtime/0.0.125/KingmakerGunslinger-0.0.125-local-runtime.zip`
  sha256 `6c5790d6f6d950d3388a8d24ea5feaf279b7ce2bc3127a2a5ae11c629c6b4c05`
  DLL sha256 `b4bc1513c754b73bb1abdbcdba452628c8120dfeee6c5cbbe9a6413362b039fd`
  deployment manifest
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260911T1745465853449Z/deployment.json`
  (owner FeatureModules.json preserved; rollback backup
  `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260911T1745436555052Z`).

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

- 2026-09-11 guarded `disposable-teleportation-coexistence` PASS on the exact
  final artifact (run `20260911T1745466343449Z-ccc05e0ebcab483189c47ab2263b7866`,
  evidence `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260911T1745466213435Z-disposable-teleportation-coexistence/`,
  Steam App 640820, `KMG_AUTOMATION_WORKING`, loaded mod 0.0.125, 28/28
  assertions PASS, zero save writes, zero UI exceptions). Key geometry
  (structured, canvas world/local units at the automated fixture's
  resolution; canvas scale 0.8125):
  - native settlement button 175.0 local wide → widened to 269.35 local =
    218.9 world = label preferred 202.63 + native padding 16.25 (symmetric
    ≈8.1/side) — exactly the policy target;
  - widened extent −107.82..111.07 world, inside the Travel/Cancel region
    −160.49..162.12 (compact-rows-fit unchanged: rows width 387.16, native
    extent identical to the qualified 0.0.124 run — no indirect expansion);
  - governing mechanism recorded: no parent layout group, point-centered
    anchors (0.5/0.5) — the settled rect size carries the width; the
    natively present LayoutElement (count 1 before/during/after) had its
    values driven and restored;
  - dismissal restores exactly 175.0 with the byte-for-byte label; a second
    open cycle reproduces exactly 269.4 with neighbor geometry
    byte-identical and rows unchanged (no cumulative growth); native
    callback count 1 throughout; travel happened only through the explicit
    native invoke gate; no resource spend from layout/reopening.
- 2026-09-11 runtime scenario preflight 471 PASS + teleportation world-map
  guarded metadata check PASS (post-bump orchestrator contracts).

## NOT RUN / limitations

- Visual acceptance at the owner's resolution/UI scale remains pending owner
  review: the automated proof is structured geometry (the numbers above),
  not an eyeball; no after screenshot was captured by the permitted workflow
  (the scenario records structured evidence; screenshots remain optional
  supporting evidence per the runtime-testing contract). The machine's
  display settings were not modified autonomously; the automated run's
  display resolution was not recorded in evidence.
- No release published, no merge; owner review required.
