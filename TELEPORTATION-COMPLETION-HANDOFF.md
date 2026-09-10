# Teleportation completion handoff (Z mission, 2026-09-10)

This is the §10 final handoff for the five completion gates in
`Z-TELEPORTATION-MISSION.md`. Progress detail lives in
`Z-TELEPORTATION-STATE.md`; this file is the curated handoff summary.

## Demonstrated causes and fixes

1. **Broken arrows after teleport.** The direction arrows are
   `CompassDirectionLabel` widgets rebuilt only by the native
   `OnPawnMovementStopped` pawn event. Relocation raised no events, leaving
   stale origin arrows. Fix: `TeleportationOutcomeWorld.Relocate` raises the
   exact native notification pair around `SetCurrentPosition` +
   `UpdatePawnPosition` (no `OpenOutgoingEdges`, so the no-reveal contract
   holds; stop-time exploration cannot fire because the arrival sets a
   location position).
2. **Specialist slot rejection.** The favorite slot accepts only spells in
   the school special list (`Spellbook.GetSpecialSpells`), attached by the
   school feature. Fix: Teleport (5) and Greater Teleport (7) are published
   into verified `WizardConjurationSpellList`
   (`69a6eba12bc77ea4191f573d63c9df12`) through the existing reversible
   publication. Existing specialists re-derive on load; no respec.
3. **Overflowing/ambiguous destination menu.** Fix: compact two-line rows
   (full spell name over caster · cost) measured inside the native content
   width, and the natively available settlement button is relabeled
   "Settlement Teleport" while spell rows coexist, restored byte-for-byte on
   dismissal. Callbacks, ownership, eligibility untouched.

## Scrolls (Gate 4)

Items (canonical spell associations, CopyScroll intact):
`KMG_ScrollOfTeleport` (1125 gp, CL 9, level 5),
`KMG_ScrollOfGreaterTeleport` (2275 gp, CL 13, level 7),
`KMG_ScrollOfWordOfRecall` (1650 gp, CL 11, level 6).
Vendor stock (verified shared tables, one grant per table):
Zarcie `ArcaneScrollsVendorTableI` `5450d563aab78134196ee9a932e88671`
(5 Teleport + 3 Greater Teleport); Arsinoe/Jhod family
`C11_JhodVendorTable` `afa2c7f292b8e1c4d9c835f0e8047dd3` (5 Word of Recall).
Saved inventories migrate via a module-gated `VendorLogic.BeginTrading`
sweep with save-owned grant markers: natively stocked targets only record
the marker, bought-out stock never refills, and each aliased family
receives exactly one batch. Native shared-table diff also stocks new
materializations. Scroll sources enumerate the shared party inventory once;
readers are spell-knowing casters or trained Use Magic Device characters;
activation consumes exactly one scroll and no spell slot; inventory Use
outside the flow prompts to select a world-map destination.

## Player instructions

See README "Teleportation completion update" and the smoke-test section
"Teleportation completion smoke test" (specialist preparation, purchase,
copy, activation, arrows, compact menu).

## Structured acceptance evidence (all PASS, zero save writes)

- `disposable-teleportation-arrows` 8/8 (twice): arrows rebuilt at each
  arrival, first arrow through the real handler, off-target arrival bound
  to the ACTUAL point, stationary waiting adds no mileage/time.
- `disposable-teleportation-specialist` 9/9 (twice): universalist negative,
  favorite rejection of other schools, favorite preparation via native UI,
  mixed counting, rest, world-map spend of exactly the favorite use.
- `disposable-teleportation-coexistence` 26/26: compact rows fit inner
  width, Settlement Teleport relabel + restore, foreign/native coexistence.
- `disposable-teleportation-scrolls` 18/18 (twice): shared stock counted
  once, UMD reader row, native cancellation, exactly-one scroll cast with
  rebuilt arrows, migration rules, and the integrated chain — real gold
  purchase (`VendorLogic.BeginTrading/AddForBuy/Deal`), native copy
  (`CopyScroll.CanCopy/DoCopy/RemoveItem`) into a fresh book, Conjuration
  favorite preparation, cast, first arrow.
- Regressions: destinations 68/68, casting 44/44, interaction 29/29,
  travelers/gamepad/disabled PASS; observer (scrolls + vendor stock) PASS.
- Persistence: four fresh-process phases A/B/C/D 12/12/8/3 (transaction
  `20260910T1830094932100Z_9c3533f46399407994e5ad1f57d80217`) including
  scroll grant markers, module OFF/ON, reload arrows, protected saves.
- Workflow at every commit: clean Release build, repository validation,
  1,561 domain tests, 464-check scenario preflight.

## Final artifact

- Branch `codex/z-teleportation-completion` (no merge/release performed).
- Qualified candidate commit `d634ad1d`; documentation commits follow.
- Package `artifacts/local-runtime/0.0.121/KingmakerGunslinger-0.0.121-local-runtime.zip`
  sha256 `33a23dbb70273d56fa4c0da868ec4c7399528876a4952bfb79bab216afa7d8f0`.
- Installed DLL sha256 `9ad27a2d7ac4b58667f4d6c6552ccc515ed0e10b46d8676f32bd67aef809238a`,
  MVID `9c872887-6969-408c-ab5f-7e841c02f73f` (== deployed == qualification).
- Rollback backup `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260910T1829167066166Z`.
- Owner `FeatureModules.json` preserved/restored; all pre-existing saves
  hash-protected in every run.

## Known limitations

- Purchase and copy are driven through the exact native action boundaries
  (`VendorLogic.Deal`, `CopyScroll.DoCopy/RemoveItem`), not pointer-driven
  shop/spellbook screens; structured evidence only, no screenshots.
- Crafted-scroll variants (Craft Magic Items) and non-16:10 aspect ratios
  were not separately exercised; both use the same item/table/UI contracts.
- Cleric/Druid Word of Recall activation eligibility follows the published
  class lists (Cleric 6 / Druid 8) and is covered by list publication
  checks, not a separate divine caster scenario.
