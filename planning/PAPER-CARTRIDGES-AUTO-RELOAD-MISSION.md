# Paper Cartridges, Ammunition Selection, and Free-Action Full Attacks

Status: active autonomous mission (Revision 2, 2026-08-08).

## Authority and baseline

- Work only on Paper Cartridges and the directly required reload, full-attack,
  Lightning Reload, misfire, crafting, vendor, persistence, presentation, and
  qualification integration.
- Base is clean remote `master` commit
  `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`, containing archetype ancestor
  `1c570bd4211d69c5c29f6af46a870146adb1645b`.
- Work only on `codex/paper-cartridges-auto-reload`; never merge, rebase, force
  push, raw-push, or edit/bypass the approved push helper.
- Baseline authority is version `0.0.73` / informational version
  `0.0.73-pistolero-musket-master`, 242 registered blueprints and 243 ledger
  identities (242 active plus one reserved). Target `0.0.74` /
  `0.0.74-paper-cartridges-auto-reload` unless the actual base advances.
- Preserve all merged rare-firearm, Reliable, Seeking, Smith/BTSL stock, fixed
  campaign loot, Pistolero, Musket Master, Mysterious Stranger, proficiency,
  audio, persistence, and optional-compatibility behavior.

## Fixed feature contract

- Add one stackable inert inventory item, **Paper Cartridge**, cost 12 gp,
  weight zero unless the installed contract requires an engine-safe equivalent,
  zero resale, with project-owned/fallback art and truthful text.
- A cartridge contains powder and bullet or pellets, replaces one Black Powder
  Charge plus one Lead Ball, loads exactly one chamber, reduces reload by one
  step, and adds +1 effective misfire to that exact loaded shot only.
- Stable loaded ID: `kmg.ammunition.paper-cartridge`. Compatibility is driven by
  resolved canonical firearm definition: `FirearmEra.Early` and kind Pistol,
  Musket, or Blunderbuss. This includes mundane, +1, and all named magic clones.
  Rifle and Revolver reject paper mode. No other ammunition feature is in scope.
- A distinct loaded identity survives equipment changes and reconstruction until
  fired/removed and works for direct Blunderbuss fire and Scatter Shot. It never
  changes damage, range, touch AC, critical behavior, projectile count, or cone.

## Selection and reload contract

- Keep the existing Reload Firearm ability as the sole manual/right-click native
  auto-use control. Its text must explain left-click manual reload, right-click
  auto reload, Paper mode source selection, and no fallback.
- Add one per-unit, visible, project-owned **Use Paper Cartridges** activatable,
  off by default, free to toggle, no resource, backed by exact hidden state.
  Grant it exactly once alongside Reload from full, one-handed scoped, and
  two-handed scoped proficiency. It remains active at zero stock, never leaks
  between units, and affects future reloads only.
- Mode on requires compatible firearm plus cartridges. It never consumes or
  falls back to loose ammunition. Mode off retains powder-plus-ball behavior.
- One immutable profile/catalog owns IDs, inventory source, exact blueprints,
  compatibility, reduction, modifier, display name, and rejection reason.
- One authoritative pure reload plan binds unit, exact item, definition,
  condition, selected profile/source, requested/loadable rounds, normal action,
  Lightning legality/action, availability, and rejection. Presentation,
  command construction, delivery, native empty-attack continuation, full attack,
  diagnostics, and tests consume this plan.
- Reduction order: base; qualifying Fast Musket; exact matching Rapid Reload;
  Paper Cartridge; clamp Free. Required results: Pistol loose Standard, Rapid
  loose Move, paper Move, Rapid+paper Free; Musket/Blunderbuss loose Full-round,
  Rapid loose Standard, paper Standard, Rapid+paper Move; Fast Musket loose
  Standard, Rapid loose Move, paper Move, Rapid+paper Free. Advanced firearms
  retain established non-paper behavior.
- Atomic reload supports loose (remove powder and ball, load Lead Ball identity)
  and paper (remove one cartridge, load paper identity). Validate before writes;
  after any write restore exact firearm state, all three inventory counts, and
  any Lightning marker on exception or verification failure. Never mix loaded
  identities or substitute a source after planning.

## Persistence and misfire contract

- Append tokens `kmg.state.v1.loaded-normal.paper-cartridge` and
  `kmg.state.v1.broken-loaded.paper-cartridge` with new inert enchantment
  blueprints/GUIDs. Preserve every old token ID/GUID/meaning, including absence
  as Empty/Normal and all Lead Ball, broken, and wrecked semantics.
- Extend codec, catalog, bootstrap, manifest, validators, reconciliation,
  corruption diagnostics, tooltips, and tests. State DTO string contract stays.
- Token replacement must preserve exact Enhancement, Reliable, Seeking, and Fey
  Bane static enchantment arrays through all state/reconciliation/transfer paths.
- Extend the single `EffectiveFirearmMisfirePolicy`; order is base, existing
  condition/training, ammunition (+0 lead/+1 paper), exact-weapon Reliable, one
  final clamp 0..20. Threshold zero remains truthful.
- Ordinary fire, Dead Shot, and Scatter capture pre-discharge ammunition and use
  this same exact-weapon policy. Preserve duplicate guards, condition transitions,
  training, Expert Loading, Strangers Fortune, explosion scheduling, grit/True
  Grit, one committed non-misfire sound, and no normal report/sound on rejection
  or misfire. Dead Shot uses one captured threshold for all probes. Scatter keeps
  all-roll-misfire aggregation and triple explosion; its cone never repeats as a
  native multi-cone full attack.

## Full attack and Lightning Reload contract

- Native empty-attack auto-use retains exact item/target/pending-command binding,
  interruption and stale-context safeguards, delivery revalidation, and no empty
  projectile; it consumes the selected plan.
- Inline reload requires native full attack, completed prior attack, next attack,
  same item, legal target, empty non-Wrecked state, exact Reload Firearm native
  auto-use, available plan, and either a genuinely Free normal reload or one
  currently legal unused genuinely Free Lightning Reload. Commit once or end the
  remaining sequence without consuming/attacking. Prefer normal Free without
  spending Lightning. Cover Haste/Rapid Shot/iteratives/switch/death/retargeting.
- Lightning Reload remains one chamber and once per round, with positive grit and
  True Grit preserved. Loose/no Rapid is Swift; loose/matching Rapid is Free;
  paper/compatible/in-stock is Free; paper absent/incompatible rejects without
  fallback. Rapid+paper does not grant a second use. Displayed/runtime/command
  action must agree. Marker is unit-local, success-only, exactly rolled back,
  and reset by the native round callback.

## Crafting, acquisition, and presentation contract

- Add **Craft Paper Cartridges**: existing kit, conscious/able/out of combat,
  shared exact once-per-rest marker with basic ammunition, cost 120 gp, output
  exactly 20, no skill check. Basic recipe remains unchanged. Money, item count,
  and marker commit atomically and restore exactly. Update Gunsmithing text.
- Extend existing bounded vendor normalization. Publish 200 to exact capital
  `SmithVendorTable` `7de959347266092448d8a72089ef9778` and 200 to each installed
  project-handled BTSL table. Include paper in desired stock and project-owned
  normalization universe; exact repeats are no-ops; normalize only project
  stale/duplicate/wrong counts; preserve unrelated entries/order and exact
  rollback snapshots; refuse rollback after foreign mutation.
- Never restore stock to Jhod `afa2c7f292b8e1c4d9c835f0e8047dd3`; preserve
  mundane/+1 roster, advanced/named exclusions, and all five fixed rare-firearm
  loot relationships. Do not put paper in named loot or starting ammunition.
- Investigate Bokken through bounded exact installed blueprint/runtime evidence.
  Publish 100 only if one unique table and safe transaction are proven; otherwise
  document evidence-backed defer without blocking the core feature.
- Update all names/descriptions/icons/help, recursive presentation publication,
  logs, build label, changelog, architecture, matrices, resume/blockers, and a
  short human visual/player checklist. Do not globally mutate donor blueprints.

## Qualification and stopping contract

- Each narrow source phase requires focused tests, repository validation, full
  dependency-free domain suite, clean exact-reference Release build, output and
  SoundBank validation, deterministic package creation/strict validation,
  scenario preflight, diff check, and tracked/generated/binary/save/secret audit.
- Runtime proof must use the guarded Steam App ID 640820 harness, exact-build
  `mod-load-smoke` before feature sequences, request-local disposable fixtures,
  structured evidence, and no UI automation/OCR/direct executable/save writes.
- Implement and pass disposable reload, native full-attack, misfire, scatter,
  Lightning Reload, crafting/vendor, and comprehensive scenarios. Final requires
  two consecutive comprehensive PASSes from independent fresh processes.
- Final compatibility order: standalone, Arms & Armor, Toggle Custom Soundpacks,
  existing combined profile, then one bounded Call of the Wild sequence. Restore
  every compatibility transaction exactly and retain CotW classification absent
  its existing human gate.
- Run two eligible canonical working-save smokes unless inherited blockers make
  the smoke itself invalid. Never alter `KMG_AUTOMATION_BASELINE`; only explicitly
  named `KMG_AUTOMATION_WORKING` is allowed by the guarded procedure.
- Inherited detached Gunslinger's Dodge and Targeting Torso defects remain out of
  scope: preserve/report, prove paper slices independently, and continue.
- Failures require structured-evidence diagnosis, narrow repair, regression test,
  complete applicable gates, coherent commit/push, and fresh-process retry.
- Stop only for the work order's genuine hard stops: irreconcilable unsafe base;
  exact-branch helper rejection; genuinely absent mandatory proprietary contract;
  proven unsafe/unimplementable core; save/data/system/credential risk; forbidden
  Steam/account UI; unresolved authoritative contradiction; or finite completion.
  Before a non-completion stop, preserve a clean pushed tree and write
  `HUMAN-INPUT-REQUIRED.md` with evidence, smallest question, answer, and resume.
- Every coherent commit is pushed only by the approved policy helper and remote
  equality is verified. Completion requires all source, deterministic, package,
  runtime, compatibility, evidence, documentation, cleanliness, and publication
  conditions; a build, checkpoint, commit, or one runtime PASS is never enough.

