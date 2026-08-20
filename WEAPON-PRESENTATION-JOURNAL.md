# Weapon Presentation Calibration Journal

## 2026-08-20 — startup and baseline qualification

- Read the repository agent contract and inspected the clean `master`
  worktree, origin, and recent history.
- Fetched `origin` and fast-forwarded `master` without rewriting history.
  Local `master` and `origin/master` were both
  `7af4375238b2492857a131eefdf909b38a000a05`, exactly the authored minimum
  baseline.
- Created `codex/weapon-presentation-calibration` from that origin commit. No
  mission commit exists yet.
- Inspected current firearm, spear, and Eastern bundle builders; runtime asset
  loaders; blueprint presentation copying; variant mappings; calibration tools;
  tests; guarded runtime harness; and relevant historical visual-repair work.
  All named historical branches are ancestors of current master, so current
  post-bugfix source remains authoritative.

Baseline validation outcomes:

- `./scripts/validate-repository.ps1`: PASS.
- `./scripts/test-domain.ps1 -Configuration Release -Clean`: PASS,
  1,162 tests and 0 failures.
- `./scripts/build.ps1 -Configuration Release -Clean -Package`: PASS,
  including repository validation, all 1,162 domain tests, Release compilation,
  build-output validation, sound-bank validation, and strict package validation.
- `./scripts/validate-package.ps1 -PackagePath
  ./artifacts/packages/KingmakerGunslinger-0.0.88-urban-barbarian.zip`: PASS.
- `./scripts/Build-Local.ps1`: PASS, including exact-reference validation and
  generation of the guarded local-runtime package.

Baseline artifact identities:

- Project version: `0.0.88` (`0.0.88-overnight-gunslinger-bugfixes`).
- Standalone package:
  `artifacts/packages/KingmakerGunslinger-0.0.88-urban-barbarian.zip`.
- Standalone package SHA-256:
  `6D7B39013F7FB97006332C0DC8C5F1196F3BB8E6B7F446A9681E575BD5874466`.
- Local-runtime package:
  `artifacts/local-runtime/0.0.88/KingmakerGunslinger-0.0.88-local-runtime.zip`.
- Local-runtime package SHA-256:
  `396EFC6872771F306BA4BB6812DCC1FD1E174D0E638DBEC1304628CD9469075A`.
- Installed DLL SHA-256:
  `8D096D478E0F38B68A0C982A1DC4DC90DD6B02697CDD391E7D55C26ED29C9FFD`.
- Installed DLL MVID: `bd200eda-1acd-4f3a-b2bf-3a6eaa56043b`.
- Firearm bundle SHA-256:
  `050197BA87F71B7C8D5D4FF056D4FF7CF0C9CCD1DBBD8FB23E748FCE6492C35C`.
- Spear bundle SHA-256:
  `33EB89C74EC4AE7CDA5A8155224A449233904B74CB59FC453C24AE022EE3CB2A`.
- Eastern bundle SHA-256:
  `079AA2E44E313291C144BD830D302782310274B11375204F9CE8FF6481EF3041`.
- Deployment manifest:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260820T2136093830388Z/deployment.json`.
- Recoverable prior-live backup:
  `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260820T2136060672078Z`.

Guarded baseline runtime outcomes through Steam App ID 640820:

- `working-save-smoke`: PASS at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2137012516471Z-working-save-smoke/runtime-result.json`.
- `disposable-firearm-visual-rigs`: PASS at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2139121725602Z-disposable-firearm-visual-rigs/runtime-result.json`.
- `observe-native-firearm-rig-contracts`: FAIL only because the observer still
  expects all five profiles to be `NativeFallback`; it independently reports
  both native donor controls and all five current `AutonomousCandidate` custom
  capabilities as valid. Evidence:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2141154020918Z-observe-native-firearm-rig-contracts/runtime-result.json`.
- `observe-elven-branched-spear-contracts`: PASS at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2143164620170Z-observe-elven-branched-spear-contracts/runtime-result.json`.
- First `observe-eastern-weapon-contracts` produced all contract assertions and
  a late game-side PASS after the outer 120-second orchestration timeout. A
  clean retry with `-TimeoutSeconds 300` passed at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2148107345614Z-observe-eastern-weapon-contracts/runtime-result.json`.
- `disposable-elven-branched-spear-combat`: PASS at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2150547350589Z-disposable-elven-branched-spear-combat/runtime-result.json`.
- `disposable-eastern-weapons-combat`: PASS at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2152440037492Z-disposable-eastern-weapons-combat/runtime-result.json`.
- `disposable-production-firearm-switching`: PASS at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2154423464217Z-disposable-production-firearm-switching/runtime-result.json`.

No credentials, entitlement, cloud-conflict, purchase, update, or security
dialog was observed. These runs establish baseline structure and protected
mechanics; they do not establish cosmetic acceptance because no state-labelled
character screenshots were captured.

## Current engineering findings

- Firearm roots are identity transformed and visible corrections live on the
  `Visual` child, as required. Long guns have grip/muzzle/butt/support markers,
  but no `WeaponUp` secondary axis. Service Pistol and Revolver still use
  unexplained legacy transforms and lack source semantic markers. Rifle uses
  hard-coded source points. Handgun stored models are deliberately hidden;
  Musket and Blunderbuss have independent back prefabs; Rifle is hidden.
- The firearm native-contract observer contains a stale
  `production-readiness-remains-fallback` expectation that contradicts current
  `AutonomousCandidate` profiles.
- The spear generator proves the physical central head points toward source
  `+Z` and the blade thickness/normal is along source `Y`. Current held building
  uses `Quaternion.Euler(90,0,0)`, mapping physical `+Z` to root `-Y`. Current
  stored building uses the opposite X rotation before back roll. The polarity
  mismatch is a supported defect hypothesis, but the native Longspear control
  must choose the correct target sign before repair.
- Eastern source models deliberately place grip at zero and physical tip along
  `+Z`; their cutting edge lies toward local `-X`, spine toward `+X`, and blade
  surface normal is local `Y`. Current prefabs use identity rotation and expose
  no `BladeNormal`. They also publish only held prefabs and unintentionally keep
  donor stored/sheath presentation.

Next work is the semantic-frame/native-donor checkpoint and guarded baseline
visual capture. No cosmetic acceptance claim has been made yet.

## 2026-08-20 — guarded visual-baseline instrumentation

Added the request-gated `weapon-presentation-evidence` scenario. It uses the
proven autonomous working-save loader, creates one disposable default Medium
humanoid in the loaded area, equips the real registered production item for
each of the 22 exact visual variants, and drives the native
`UnitViewHandsEquipment` lifecycle across game updates. `ForceSwitch(false)`
and `ForceSwitch(true)` capture stored and genuinely in-hand states separately;
`GetWeaponModel(false)` identifies the exact active primary-hand visual. The
scenario never invokes a save API and restores exact party/global unit
snapshots.

The fixture writes one PNG contact sheet and one JSON record for both `stored`
and `held-idle` per variant. Each sheet contains front, right-side, rear, and
front-right three-quarter views. JSON records bind the image hash to the item
symbol/GUID, effective held/belt/sheath models, exact hierarchy path, model
transform, semantic anchors, body/weapon bounds, overlap diagnostics, state,
and loaded build identity. Empty-handed body renderer references are frozen
before any item is equipped so inherited sheath or auxiliary equipment bounds
cannot distort camera framing.

Focused and full qualification while developing the fixture:

- Repository/version validation: PASS on every build iteration.
- Complete Release domain suite: PASS, 1,163 tests and 0 failures.
- Release compilation: PASS after the final lifecycle/framing change.
- Runtime preflight: PASS, 112 checks.
- The stale native-firearm readiness assertion was corrected to the published
  five `AutonomousCandidate` profiles without weakening either native donor
  control. Repaired observer PASS:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2220444335411Z-observe-native-firearm-rig-contracts/runtime-result.json`.

Guarded Steam iterations failed closed for specific fixture defects (request
timeout classification, pre-load dispatch, synchronous model lookup, stored
versus held mislabelling, low-density abort, and renderer-rebuild timing). Each
was narrowed with structured evidence and replaced by an explicit lifecycle
contract. The final unchanged-asset baseline is PASS at:

`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2307109303617Z-weapon-presentation-evidence/`

Final baseline assertions:

- Exact production matrix: PASS, 44 records / 22 variants.
- Native materialization: PASS, 44/44 stored and held models.
- Contact sheets: PASS, 44 PNG/JSON pairs, 176 views, 89 scenario files.
- Render visibility: PASS, zero blank and zero low-density sheets.
- State labels: PASS, exactly 22 `stored` and 22 `held-idle` records.
- Cleanup: PASS in one update; exact party/global-unit snapshots restored.
- Loaded version: PASS, `0.0.88`.
- Runtime DLL SHA-256:
  `36B28263DA564418C1421F02847891BAD0C2C7A8B50F17A1CE9BB63E8C95CADA`.
- Local-runtime package SHA-256:
  `28BC3B5298510D4B4E72A050194E24AADF858DF451A945CE13B0B39DC274C35E`.
- Deployment manifest:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260820T2307108673780Z/deployment.json`.

Direct baseline observations on the default Medium male fixture:

- Service/Duelist/LastWord pistols and the Revolver are in the primary hand but
  sit behind or across the pelvis; the barrel/grip silhouette is visibly
  implausible. Their nominally hidden stored policy does not prevent the held
  model from appearing in front-belt slots.
- Musket and Rifle held-idle models cross the legs/torso at a low diagonal;
  their support hands remain at the actor's side in this idle state. Musket and
  Blunderbuss stored models run nearly horizontally through the shoulder line.
- All three branched spears materialize in hand and on the back. Held idle uses
  one low dominant-hand attachment with the shaft crossing behind the body;
  stored presentation crosses the upper back. Attack polarity, two-hand fit,
  and branch roll still require combat/thrust evidence.
- All twelve Eastern variants materialize distinctly. Wakizashi and Katana
  held blades are close to edge-on from the front and cross behind the legs;
  Nodachi exposes a different apparent blade plane. Stored models inherit donor
  mounting rather than a custom independently calibrated stored prefab.

These images establish baseline defects, not acceptance. This fixture does
not yet claim combat-ready, attack/fire/thrust, reload, locomotion, transition,
female, Small, or Enlarged correctness.

## 2026-08-20 - baseline evidence checkpoint qualification

The evidence harness and repaired native-firearm readiness expectation were
committed as `baa426f491ad7a63a9a2dc52c7236e5f4c4b5afd`
(`test(presentation): add guarded baseline visual evidence`) and published to
`origin/codex/weapon-presentation-calibration` with an ordinary non-force push.

Exact post-runtime clean qualification for that source:

- Runtime scenario preflight: PASS, 112 checks.
- Repository/version validation: PASS for version `0.0.88`.
- Complete clean Release domain suite: PASS, 1,163 tests / 0 failures.
- Clean Release build and standalone package creation: PASS.
- Explicit standalone UMM package validation: PASS.
- `Build-Local.ps1`: PASS; it performed no deployment.
- Release package:
  `artifacts/packages/KingmakerGunslinger-0.0.88-urban-barbarian.zip`.
- Release package SHA-256:
  `28BC3B5298510D4B4E72A050194E24AADF858DF451A945CE13B0B39DC274C35E`.
- Local-runtime package:
  `artifacts/local-runtime/0.0.88/KingmakerGunslinger-0.0.88-local-runtime.zip`.
- Local-runtime package SHA-256:
  `28BC3B5298510D4B4E72A050194E24AADF858DF451A945CE13B0B39DC274C35E`.
- DLL SHA-256:
  `36B28263DA564418C1421F02847891BAD0C2C7A8B50F17A1CE9BB63E8C95CADA`.

The clean local-runtime package is byte-identical to the package deployed for
the successful visual-baseline run. Runtime evidence therefore corresponds to
the exact committed source, not an unqualified intermediate build.

## 2026-08-20 - live native-donor frame capture

Expanded the guarded visual-evidence scenario without changing any production
asset transform. In addition to the exact 22 production variants, the scenario
now resolves six exact native donor types and a deterministic native item whose
effective held model is the donor model: Light Crossbow, Heavy Crossbow,
Longspear, Scimitar, Bastard Sword, and Greatsword. Each control is materialized
through the same real equipment lifecycle in both stored and held-idle states.

The records now include presentation role, model-local renderer bounds and
major/minor axes, plus model-local positions and axes for native IK, weapon
center, trail, surface, and warhead locators. The successful guarded Steam run
is:

`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260820T2345261164438Z-weapon-presentation-evidence/`

Exact results:

- Native controls: PASS, 12 records / 6 controls.
- Native held/stored mesh-local geometry: PASS, 6/6 invariant at an explicit
  component tolerance of `0.00001`; bounds come from `Mesh.bounds` or
  `SkinnedMeshRenderer.localBounds`, never reconstructed world AABBs.
- Full production/control materialization: PASS, 56/56.
- Evidence output: PASS, 56 PNG/JSON pairs / 224 labelled views / 113 files.
- Blank or low-density contact sheets: 0 / 0.
- Cleanup: PASS; no save interaction and exact request-local snapshots restored.
- Steam route: App ID 640820.
- Runtime DLL SHA-256:
  `EA0774E877274437ED63CA94E55FB8FA50CE183C0DBBE566BD2E13EFE8A2617E`.
- Guarded local-runtime package SHA-256:
  `24DF7A916343948A8515FE699B14B367914D97FD8D062D1007B9D34212AB098A`.
- Deployment manifest:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260820T2345260622459Z/deployment.json`.

Measured donor facts, expressed in each instantiated model's local space:

- Both crossbow controls are longitudinal on `+Z`, use local `+Y` as the stable
  secondary/up axis, and expose their support-hand IK targets on the fore-end.
- Native Longspear is longitudinal on `+Y`: its warhead is at local
  `Y=0.9053`, trail/head locators are near `Y=0.982`, and its left-hand target
  is behind the root at `Y=-0.1680`. Its physical head therefore establishes
  donor forward `+Y`, disproving the current authored branched-spear mapping to
  `-Y`.
- Scimitar, Bastard Sword, and Greatsword blades are longitudinal on `+Y`.
  Their blade surfaces occupy the local `YZ` plane, so local `+X` is the
  donor's blade-normal axis. The scimitar tip curves toward local `-Z`, which
  also supplies an edge-side convention rather than only an unsigned plane.
- Native held and stored attachment transforms are different for every control.
  Held and stored presentation therefore require separate calibration; copying
  one visible-child transform between those states would be unsupported.

These are donor-frame measurements, not cosmetic acceptance. The next source
checkpoint will encode complete authored semantic frames and validate their
basis, polarity, scale, support interval, renderer endpoints, and identity-root
contract before applying family-specific transforms.

Instrumentation was itself fail-closed. The first donor run at
`20260820T2329318465808Z` captured valid images and locators, but code review
found that its local bounds were reconstructed from world-axis-aligned bounds,
which made attachment rotation inflate the result. That run is not used for
mesh bounds. A corrected run at `20260820T2340576016289Z` then rejected exact
string equality for harmless floating-point deltas no larger than roughly
`0.0000017`. The final run above records numeric components and applies the
documented `0.00001` tolerance; it passes all six controls. No production asset
was changed during these iterations.

Post-runtime clean qualification for the exact diagnostic source:

- Runtime scenario preflight: PASS, 112 checks. One initial preflight invocation
  was invalidated by concurrently running repository validation while its
  artifact fingerprint assertion was active; the isolated rerun passed and the
  unsupported fixture performed no build, deployment, launch, or evidence
  creation. An immediate post-runtime invocation later caught one lingering
  artifact timestamp update; a direct before/after unsupported-scenario probe
  showed zero artifact delta and zero CIM/launch calls, and the quiescent rerun
  again passed all 112 checks.
- Repository/version validation: PASS for `0.0.88`.
- Complete clean Release domain suite: PASS, 1,163 tests / 0 failures.
- Clean Release build/package: PASS, including another complete domain run,
  output validation, sound-bank validation, and strict package validation.
- Explicit standalone package validation: PASS.
- `Build-Local.ps1`: PASS; no deployment performed.
- Clean standalone and local-runtime package SHA-256:
  `24DF7A916343948A8515FE699B14B367914D97FD8D062D1007B9D34212AB098A`.
- Clean DLL SHA-256:
  `EA0774E877274437ED63CA94E55FB8FA50CE183C0DBBE566BD2E13EFE8A2617E`.

The clean package and DLL exactly match the guarded runtime identities above.
The source/test checkpoint was committed as
`07c11236d2047af63fc6aeccfb51be99b06fe708`
(`test(presentation): capture native donor frames`) and published to the
identically named origin feature branch with an ordinary non-force push.
