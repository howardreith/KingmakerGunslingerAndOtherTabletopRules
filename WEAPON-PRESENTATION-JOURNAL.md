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

## 2026-08-21 - semantic-frame and handgun calibration checkpoint

Published implementation commit
`e2aba9d24cebbf38aadc236044c84f641a69534c`
(`fix(presentation): validate frames and calibrate handguns`) adds one shared
authoring/runtime semantic-frame contract and applies it to every production
firearm, branched-spear prefab, and Eastern held prefab. The contract requires
identity equipment roots; unique direct-child markers; finite, positive,
non-reflected hierarchy scales; a nondegenerate forward/secondary basis;
correct tip/butt polarity; plausible semantic length; support-hand interval
and envelope placement; and correspondence between semantic ends and actual
mesh-backed renderer bounds. Basis rotation and grip translation use the
documented equations rather than an unexplained Euler value.

The family builders now author `WeaponUp`, `HeadUp`, or `BladeNormal` as
appropriate. Firearms also author an explicit `WeaponForward`, which keeps the
barrel axis distinct from a muzzle that is vertically offset from the grip.
Held and stored long-gun and branched-spear prefabs must be distinct and must
not repeat one incompatible visible-child transform. Runtime bundle loading
repeats the semantic and renderer-bound checks and fails closed to the existing
native fallback on invalid assets. The diagnostic code remains request-gated.

Measured handgun corrections:

- Service Pistol source forward is `-Z`, source up is `+Y`, and its physical
  grip is source `(0,0,0.68)`. The solved donor frame is forward `+Z`, up `+Y`,
  visible position `(0,0,0.1632)`, Euler `(0,180,0)`, and scale `0.24`. The
  real renderer endpoints are root `Z=-0.0768..+0.4032`; the stale `+0.264`
  muzzle and unexplained `180`-degree roll were rejected.
- Duelist's Rebuttal and The Last Word retain their distinct project-owned
  geometry and identity transforms. Their authored `+Z/+Y` frames and
  `-0.075..+0.264` semantic ends now pass the same full contract.
- Revolver source forward is physical `+X`, source up is `+Y`, and the grip is
  derived from the `Grip_LP` component bounds at
  `(-8.889382,7.50916529,2.68602586)`. The basis solution maps it to donor
  `+Z/+Y`, yielding Euler `(0,-90,0)` and grip-derived translation. Its live
  root renderer endpoints are `-0.0356628..+0.279631257` metres. The muzzle's
  vertical offset is retained; projectile data was not moved to compensate for
  visible geometry.

Deterministic Unity 2018.4.10f1 qualification:

- Final logs:
  `artifacts/weapon-presentation/semantic-frame/unity-firearms-determinism.log`,
  `unity-spear-determinism.log`, and `unity-eastern-determinism.log`.
- Two independently restaged ForceRebuild passes produced byte-identical
  tracked/output bundles.
- Firearms: `4D1F51362D7EF74A7D4DF3001783DD91BA001FDBD23F5F041987C1B6D4E5961D`
  (17,978,495 bytes).
- Branched spear:
  `0BC67C89D08806B0B67FF074AE983FC1E2CDF6E6618CC10901E66C01B7A725FA`
  (127,202 bytes).
- Eastern weapons:
  `7AF99FAA8C63BA91DBAF9BC5295E1629A8E090288A9ED86753D835CCAF3C3C33`
  (311,289 bytes).
- Deliberately retained phase boundaries: current spear held geometry still
  reports forward `-Y`, and Eastern source frames still report `+Z/+Y` rather
  than the measured native donor frames. The new contract exposes these facts;
  it does not mislabel them as calibrated.

Source/package gates:

- Repository/version validation: PASS for version `0.0.88`.
- Complete clean Release domain suite: PASS, 1,164 tests / 0 failures.
- Clean Release build/package, exact-reference compilation, output validation,
  SoundBank validation, and strict package validation: PASS.
- `Build-Local.ps1`: PASS, then repeated identically by each guarded dirty-tree
  launch.
- Standalone and local-runtime package SHA-256:
  `32BAA60B1427EF9880DC986A8030D2E83EBEC42465079C8CA4E2E823232ABF13`
  (22,192,628 bytes).
- DLL SHA-256:
  `E6240EB8B5BE1B93FF7400F4EDCFD9479C7C795F8AB9F90AAF1F21D2A51BF421`
  (3,491,840 bytes; MVID `9abfc536-78c0-4725-8b91-f9e569e11eb2`).

Guarded Steam App ID 640820 results:

- `weapon-presentation-evidence`: PASS, 9/9 assertions at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0034448996480Z-weapon-presentation-evidence/`.
  It materialized all 22 production variants plus 6 donor controls in stored
  and held-idle states: 56/56 presentations, 56 PNG/JSON pairs, 224 labelled
  views, no blank/low-density sheets, and exact no-save cleanup. Result SHA-256:
  `583124356871E62E52570E1CD6AB2A3C2823F4EBCB84DA7E307C87A67553C8DD`.
- `disposable-firearm-visual-rigs`: PASS, 65/65 assertions at
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0040087523551Z-disposable-firearm-visual-rigs/`.
  It loaded the production bundle, instantiated every production firearm
  variant, verified exact animation style, native left-hand IK, separate
  long-gun back prefabs, short-gun sheath policy, one cloned projectile per
  firearm, cleanup, and the new basis-derived service Pistol transform. Result
  SHA-256:
  `3CB1A7D9C3BD34150655B2CAE85EA3F19796EA678760DEA8733470B70A63E10C`.
- The first attempted immutable reuse correctly stopped before launch because
  that path requires a clean Git tree. The permitted dirty-source path rebuilt,
  validated, redeployed, and then passed. Deployment manifests are
  `20260821T0034448475634Z/deployment.json` and
  `20260821T0040086863012Z/deployment.json`.

Direct visual review of the four-view contact sheets shows the service Pistol
and Revolver now originate at the dominant hand instead of spanning the
pelvis. Duelist and Last Word remain distinct and plausibly grip-centred. On
this default Medium male, the service Pistol and Revolver stored weapon models
hang muzzle-down at the front-belt slot without severe torso penetration.
This is only held-idle/stored evidence. Combat-ready, firing, dual wield,
reload, movement, transitions, female, Small, and Enlarged acceptance remain
open and are not inferred from the static sheets. The profile label `hidden`
continues to mean no inherited belt/sheath prefab; Kingmaker still displays
the held weapon model in its native stored attachment, so true hidden behavior
has not been claimed.

## 2026-08-21 - guarded long-gun motion diagnostic checkpoint

Published commit `0e9c2902b255f0e091093e10f03655965d441123`
(`feat(presentation): add guarded long-gun motion evidence`) adds the autonomous
`weapon-presentation-motion-evidence` scenario. It is restricted to
`KMG_AUTOMATION_WORKING`, launches through Steam App ID 640820, creates a
request-local Medium male actor and immortal hostile target, and never calls a
save API. The exact case list is Musket.Service, Blunderbuss.Service,
Rifle.Service, and the native Heavy Crossbow control. Each case captures a
combat-ready sheet and fixed native-attack updates
`1/4/8/12/18/24/36/60/96`, with front, right-side, rear, and
front-right-three-quarter views.

Read-only inspection of the installed Kingmaker command implementation showed
that `UnitCommand.Start` rejects a command unless `IsUnitEnoughClose` is true,
while `UnitAttack.Init` derives `ApproachRadius` from the selected weapon and
target. The final fixture therefore searches only navmesh-backed target
positions and requires the native `CanStart`, `IsUnitEnoughClose`, target-state,
line-of-sight, and approach-radius contract before starting. It uses a
single-attack command and performs one guarded explicit command tick only after
the live animation reaches `IsActed`; this prevents contact-sheet capture work
from starving the normal action callback. The PASS gate requires the acted
animation event for the native control as well as the three firearms.

The instrumentation failures were retained as engineering history rather than
treated as presentation evidence:

- `20260821T0116250424801Z`: the first fixture did not establish the native
  command start contract and could not settle removal.
- `20260821T0123062470123Z`: a blind explicit start left commands interrupted;
  its motion and firing assertions failed.
- `20260821T0151032534857Z`: navmesh readiness produced real animation for all
  four cases; Blunderbuss and Rifle fired, but Musket's acted animation was
  starved before its action callback.
- `20260821T0159502682905Z`: the guarded single-attack tick proved the strategy;
  all assertions passed. A final source-review tightening then made
  `AnimationActedObserved` part of the PASS condition.

The final qualifying run is:

`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0210452969596Z-weapon-presentation-motion-evidence/`

It passed all six assertions. All four commands were installed, start-ready,
started, running, and observed with both a live animation handle and its acted
event. Musket, Blunderbuss, and Rifle each recorded exactly one discharge, no
fault, and zero loaded rounds afterward. The run captured 40 PNG/JSON pairs,
160 labelled views, no blank sheets, exact party/global-unit cleanup, loaded
version `0.0.88`, and automatic game exit. Result SHA-256 is
`6955A29800592B07B0008051F7990F9E2D3B581C77DCD9D066E220B531B0F374`;
motion-index SHA-256 is
`B3D2348D4F6EA3AE2E782D533F63F1E3B590E41C4C1C68524AA0E03316AC80E4`.
Deployment manifest:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260821T0210452354655Z/deployment.json`.

Direct review of the final contact sheets establishes, only for the default
Medium male fixture:

- Musket remains low and transverse through the pelvis/lower torso in
  combat-ready and attack poses; its hands cluster near the dominant-hand end
  instead of occupying a plausible shoulder/fore-end relationship.
- Blunderbuss spans the waist in combat-ready and the upper torso/neck during
  its acted attack; the visible support-hand relationship is implausible.
- Rifle crosses the hips in combat-ready, then places its stock/receiver over
  the upper torso while the barrel exits across the opposite side during its
  acted attack.
- Native Heavy Crossbow provides the control: it lifts along the shoulder line
  during its acted attack rather than remaining transverse through the body.

This is defect reproduction, not calibration acceptance. Reload, locomotion,
turning, storage transitions, female, Small, and Enlarged evidence remain open.
Repository validation and the complete Release domain suite pass at 1,164/1,164.
Runtime preflight passes 115 checks after one documented post-build artifact
timestamp race. Clean Release compilation, strict standalone package
validation, and `Build-Local.ps1` pass. Standalone and local-runtime package
SHA-256 are
`8A01BB7E6B1952AEEA14A9675987EEB481BB7BFBA9F73E3807D322757F01A1D7`;
DLL SHA-256 is
`2B7EC67E836D7290E71AE62D5621172F1231FC88340B23D2CE3118275158382E`.

## 2026-08-21 - calibrated long-gun held and stored checkpoint

Published implementation commit
`b672406ebbb8af8340d723d074ff8a69bd0ffe25`
(`fix(presentation): calibrate long-gun held and stored rigs`) replaces the
legacy Musket/Blunderbuss yaw guesses and Rifle source transform with one
deterministic, family-specific source-authoring pipeline. It does not rotate
an equipment root or alter projectile semantics. Each production long gun now
has an identity equipment root, a calibrated visible child, renderer-bound
butt/muzzle polarity, a measured trigger-wrist grip, an explicit fore-end
support target, and non-collinear `WeaponForward`/`WeaponUp` markers. Rifle now
has an independent `RifleBelt` prefab rather than inheriting the held transform
or relying on a hidden stored policy.

The physical source measurements are recorded as original `+X` butt-to-muzzle
and `+Z` stock/receiver-up. The generated canonical frame is `+Z` forward and
`+Y` up. The visible held layer maps that complete frame to the live native
Heavy Crossbow donor basis measured as Euler
`(81.58254, 6.878487, 255.457428)`; all root and semantic locators remain
identity-root children. Stored prefabs independently use the measured native
stored position `(-0.227002054, -0.0360002033, 0.111000687)`, Euler
`(29.35143, 112.346809, 16.69746)`, and renderer-center anchor. The Musket
support station is `0.374 m` forward of its grip, selected from the native
control contact result rather than an unexplained Euler adjustment.

The derivative generator was executed twice with identical outputs. Exact
source FBX SHA-256 values are:

- Musket: `C5E2EA93E903782BF3110E50C1D6677C4E7C109248651495192D8B6063F73A0A`.
- Blunderbuss: `45DD00FD88D7CE1B66690E1A1B6FFE732A343F3C728D84B4FF8956F1F4F4197C`.
- Rifle: `9D9288D04DEED70A6CA7AA321A2107B0F482431A082A1E2EDF4B50CB14742072`.

Two Unity 2018.4.10f1 builds also reproduced byte-identically. The final
`kingmakergunslinger.firearms` bundle is 18,172,758 bytes with SHA-256
`5FA2D053EDC75B8BC7F64C296CE8A4EBB166B4A9C956C0CCFE7278E5ABFCB49E`.
Build logs are
`C:/Dev/KingmakerGunslingerLab/unity-asset-build/weapon-presentation-long-guns-musket-support-build.log`
and
`C:/Dev/KingmakerGunslingerLab/unity-asset-build/weapon-presentation-long-guns-musket-support-repro.log`.

Final source qualification passed repository validation, all 1,164 Release
domain tests, clean Release compilation, output validation, SoundBank checks,
strict package validation, and `Build-Local.ps1`. The exact deployed local
runtime package is
`artifacts/local-runtime/0.0.88/KingmakerGunslinger-0.0.88-local-runtime.zip`
with SHA-256
`EBE81ABDF3879FCE501A9E9FB2AE71E214765274040F4165020AFDC21577FB2C`.
Its DLL SHA-256 is
`AB69C222DCEF85D3DC819E3138C99B3D47808B72F299E1F0E860710C98D02BDA`.

Final exact-package guarded Steam evidence is:

- Static held/stored matrix:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0413290534687Z-weapon-presentation-evidence/`.
  It passed 9/9 assertions with 56 PNG/JSON pairs and 224 labelled views;
  result SHA-256 is
  `80AFD853265E79EFE58DCA43EDEA1BC77CC9A99DE4781599A483C18D64FFC974`.
- Combat-ready/attack matrix:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0416419128426Z-weapon-presentation-motion-evidence/`.
  It passed 6/6 assertions with 40 PNG/JSON pairs and 160 labelled views;
  every native command ran and reached its acted event, every firearm fired
  once with no fault and zero rounds afterward, and request-local cleanup was
  exact. Result SHA-256 is
  `72B7D8088F0A98A59293B66E6AB8ED85E4D5F7F37F16B17E9044EB63EF807813`;
  motion-index SHA-256 is
  `6AB9264D2666C7FCC715CF0EDAF2663018D626FC3E9CF44D82C3D094121C8BAB`.

Across all ten ready/attack samples per weapon, left-hand-to-support-target
distance was Musket `0.118081..0.143733 m` (average `0.131895 m`),
Blunderbuss `0.107004..0.141727 m` (average `0.125596 m`), Rifle
`0.115776..0.144329 m` (average `0.133205 m`), and native Heavy Crossbow
`0.118313..0.144278 m` (average `0.132578 m`). Direct review of the front,
right-side, rear, and front-right-three-quarter sheets confirms physical
muzzles/bell lead toward the target, stocks approach the shoulder plausibly,
support hands remain at the fore-end rather than the muzzle, and no severe or
persistent torso traversal remains in the captured states. The independent
stored models are diagonal and acceptable on the same fixture; the
Blunderbuss remains visually bulky but does not show severe persistent
clipping.

Both final launches used the guarded request path through Steam App ID 640820,
encountered no credential, entitlement, cloud, or security dialog, made no
save call, exited automatically, and did not touch `KMG_AUTOMATION_BASELINE`.
The runtime identity records the previously published source commit `e530a8ac`
because the exact package was built from the qualified dirty worktree; the
package, DLL, bundle, source-FBX, and evidence hashes above bind the tested
content now committed at `b672406e`.

Acceptance is deliberately bounded to held-idle, stored, combat-ready, and
sampled acted-fire states on the default Medium male. Reload, locomotion,
turning, equip/unequip transitions, armor/cloak, female, Small, and Enlarged
coverage remain open and are not inferred from these captures.

## 2026-08-21 - guarded branched-spear thrust diagnostic checkpoint

Published commit `4e66d30afb1030849f7dcedb61669f84d79bf7bb`
(`test(presentation): capture branched-spear thrust frames`) adds the guarded
`weapon-presentation-spear-motion-evidence` scenario for Classic, Thorn,
Crown, and native Longspear. The request is restricted to
`KMG_AUTOMATION_WORKING`, uses the disposable request-local combat pair, never
calls a save API, and launches through Steam App ID 640820. Each case records
combat-ready plus nine native-attack samples from four labelled views.

The instrumentation resolves custom physical endpoints from renderer-bound
`Tip`/`Butt` markers and native endpoints from the actual
`TH_LongspearKnight1` renderer's longitudinal positive-`Y` head. It records
target-relative endpoint projections, head-face normal, dominant-hand grip
distance, and support-hand target distance. A source-review tightening first
failed at
`20260821T0443200507649Z-weapon-presentation-spear-motion-evidence` because
Classic update 1 preceded the acted event and was therefore not yet a thrust.
The final assertion correctly retains every setup sample but gates the thrust
claim on all acted samples leading with the physical tip.

Final evidence is
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0448131191263Z-weapon-presentation-spear-motion-evidence/`.
It passes 6/6 with 40 PNG/JSON pairs, 160 views, all four native commands
acted, 14/14 acted endpoint samples tip-leading, exact cleanup, version
`0.0.88`, and automatic exit. Result/index SHA-256 are
`4CC5BA985C01D4E6960C5711859486206C12AE6406BD0FF6BD7D4787E099D664`
and `BAE8D79ED48118737FA938D09262F01EB578F6DAEE855CE294A62A74EAF3FE76`.
The exact package/DLL SHA-256 values are
`F65B2036F42435D41865A63D997CBE5F65404ACEF9EDAFA00F6BCF08D62CEEE6`
and `4969DC1CC84D6B699C8CF89F4AC8832603264691447FF0210C795AB45F86CC65`.

The current physical spearhead polarity is therefore accepted for the
captured thrust states and must not be reversed. A follow-up raw-record audit
corrected the initial grip interpretation: the native control has no `Grip`
marker, so its displayed `0.000000 m` was a PowerShell null conversion rather
than a contact measurement. Every custom `Grip` is exactly coincident with
`R_WeaponBone`; the custom `R_Hand` radial shaft offset (`0.105720 m`) closely
matches the native hand-to-physical-shaft-axis offset (about `0.1036 m`). The
weapon-bone grip anchor is therefore accepted and must not be translated.

The remaining objective defects are:

- support-hand-to-target averages are Classic `0.280264 m`, Thorn
  `0.287448 m`, Crown `0.279584 m`, versus native `0.127318 m`;
- V5 static sheets retain a near-horizontal shoulder-spanning custom back
  mount while native Longspear uses a diagonal, near-vertical stored mount;
- branch roll and clipping are captured but remain open pending the
  mesh-grounded source-frame and donor-basis calibration.

Repository validation, all 1,164 Release domain tests, clean Release build,
strict package validation, and guarded Steam runtime qualification passed.
`KMG_AUTOMATION_BASELINE` was untouched and no unsafe dialog appeared.

## 2026-08-21 - calibrated branched-spear held and stored checkpoint

Published commit `a1d45c630502d873debf89ac56562568739c5d58`
(`fix(presentation): calibrate branched-spear held and stored rigs`) replaces
the measured V7 defects without changing the V7-proven physical polarity or
weapon-bone grip. The project-owned generator now emits mesh-grounded source
markers for grip, support, physical tip, physical butt, head-face normal, and
renderer center. It proves the central leaf and butt cap own the evaluated mesh
extremes, branches remain behind the physical tip, grip/support remain inside
the shaft, and all rendered scales are positive identity.

The six-prefab Unity builder maps source +Z physical-forward and +Y head-normal
to separately measured native Longspear held and stored bases. Translation is
solved from the held grip or stored renderer-center anchor; equipment roots
remain identity. The measured native support station is `0.593016 m`, exposed
through held-only `EquipmentOffsets.IkTargetLeftHand`; stored prefabs cannot
drive hand IK. Native visual parameters are cloned and only custom held/Belt
models are replaced, preserving donor animation, trail, sound, slot, timing,
and blueprint identity.

Two clean Blender 4.5.10 runs produced byte-identical FBXs and normalized PNGs
plus identical schema-3 semantic reports after excluding only Blender's
documented `.blend` session-container hash. SHA-256 values are:

- generator: `F9977A854176D047D0B0DF4C32C960CF96DD0E99BD4BAE608E1E7E5B3750274F`;
- Classic FBX: `A7FE4DEE53B18D1778D994F8B24A349B22C000E87660934B882D239A0F807E3A`;
- Thorn FBX: `3EC09E5A662991944F5B41E01852A8ABCB3A040506481D3789E1E3F94C0F430B`;
- Crown FBX: `EA5B392F95AADA371185188021A8935C26621C8CE20C485014C215ADE4BA9443`;
- unchanged runtime icon: `A4CAA5FED242BEE645AD4F9D1E5F201C372EDE4A066254EE6BD4003A6538AF99`.

Two forced Unity 2018.4.10f1 builds produced the identical 127,369-byte bundle
SHA-256 `A59DC61CE246A7F5931F22494C4C52CE39C6E96312F3448FB9138A0AC0D7DC9B`.
Repository validation, all 1,164 Release domain tests, clean exact-reference
Release build, build-output and SoundBank checks, and strict package validation
passed. The tested 0.0.88 package/DLL SHA-256 values are
`97B2F5FF735F7BF141740652F7FED392F1CC6A3267D3D3C070041DC280BD4E45`
and `DFEB9E71B034448F735EF00492CCD143AFBE3F63E09C015D6EE5598AAA638682`;
DLL MVID is `6be90c7b-b38d-46b5-baf8-01f59e0aba68`.

Three guarded Steam runtime layers pass:

- save-free semantic/combat qualification at
  `20260821T0517404957120Z-disposable-elven-branched-spear-combat` (24/24;
  result SHA-256
  `9BA7F08F144ABFD4DA95BD479444DA60E7963187205A1DA19294F817D01CC6C3`);
- held/stored static evidence at
  `20260821T0520508017635Z-weapon-presentation-evidence` (9/9; 56
  PNG/JSON pairs; 224 views; result/index SHA-256
  `80BE05F0D94040446163D53DC434C9B59E62594CD4269F5D0294E60B92F48AC2` /
  `22D045AFA893D5A34A611C50297842D95F37F3A77E3A90CF03D1A3C83645ECF2`);
- combat-ready/thrust evidence at
  `20260821T0525081495864Z-weapon-presentation-spear-motion-evidence` (6/6;
  40 PNG/JSON pairs; 160 views; result/index SHA-256
  `CFB570DC4A726DE0182DDEB6A8F834B23282CB8066AA83DDB9ACB21A8F159CA8` /
  `E1D0096EA98CD9CEC0CE553C323FF9E0C4EC3017355AF528A5C7780F9D0A2CE9`).

All 40 motion records and all 15 acted records lead with the renderer-grounded
physical spearhead. Left-hand-to-support-target ranges/averages are Classic
`0.101770..0.181390 m` / `0.130179 m`, Thorn
`0.107490..0.146483 m` / `0.123882 m`, Crown
`0.108711..0.157084 m` / `0.124882 m`, and native Longspear
`0.107776..0.174014 m` / `0.126062 m`. Weapon-bone-to-grip error is zero in
every custom frame. Direct front, side, rear, and three-quarter review accepts
all three distinct variants in held idle, stored, combat-ready, and sampled
thrust states on the default Medium male: branch roll is consistent with the
native control, both hands remain plausibly on the shaft, the physical head
leads, and no severe persistent clipping remains.

Acceptance is bounded to those states and fixture. Movement/turning,
equip/unequip transitions, armor/cloak, female, Small, and Enlarged coverage
remain ordinary final-matrix work. Both save-backed runs named only
`KMG_AUTOMATION_WORKING`; the save-free run used no save; no run called a save
API or touched `KMG_AUTOMATION_BASELINE`.

## 2026-08-21 - calibrated Eastern held and stored checkpoint

Published implementation commit
`8aeef5e7fb2ef976e7ca5cbe82ba44d50b01401b`
(`fix(presentation): calibrate eastern held and stored rigs`) replaces the
baseline family-wide sideways-roll and shared-stored-transform defects for all
12 production Eastern variants. The change is presentation-only: it does not
alter a category, item identity, enhancement, proficiency, grip rule, damage,
range, attack timing, or any other gameplay field.

The project-owned Blender generator now emits a schema-3, renderer-grounded
physical frame for every FBX. `KMG_Grip`, physical `KMG_Tip`, physical
`KMG_Butt`, `KMG_BladeNormal`, `KMG_Edge`, and `KMG_Stored` are checked against
evaluated mesh geometry. Source forward is grip-to-tip `+Z`, blade normal is
`+Y`, and the cutting-edge side is `-X`; Nodachi additionally places
`KMG_Support` on the butt/pommel side of the grip. These semantics expose the
full roll-resolving frame rather than merely proving a longitudinal vector.

The Unity 2018.4.10f1 builder corrects the FBX reflection and solves the full
source basis onto independently measured native held and stored frames:
Scimitar for Wakizashi, Bastard Sword for Katana, and Greatsword for Nodachi.
Equipment roots remain identity. Held translation lands the source grip on the
donor grip; stored translation lands a separate `StoredMount` on the donor's
measured renderer-center anchor. The output has exactly 24 prefabs, one held
and one `Stored` prefab for each production variant. Only held Nodachi prefabs
carry a left-hand target, at the native Greatsword `-0.169 m` butt-side
station.

Runtime publication is transactional: every held/stored pair must validate
before any custom presentation is exposed. `WeaponVisualParameters` is cloned
and only `m_WeaponModel` and `m_WeaponBeltModel` change. Animation style,
trails, sounds, slots, sheath, timing, and all other donor fields remain exact;
native donor blueprints are not mutated.

The first static runtime attempt at
`20260821T0629124884888Z-weapon-presentation-evidence` failed closed on the
first Nodachi held recreation. Structured evidence isolated the exception to
native Greatsword sheath reattachment: Kingmaker's `EquipmentOffsets.GetOffsets`
enumerates `m_SlotOffsets` for a sheath slot, while a runtime-added component
left that serialized collection null. The correction initializes an empty
`EquipmentOffsets.Offsets[0]`, expressing that the calibrated custom root has
no slot-specific correction. It does not clear the preserved native sheath,
copy model-specific donor offsets, move the root, or affect held IK.

After the correction, repository validation and all 1,164 Release domain tests
passed. Runtime preflight passed 121/121 after one immediately post-build
artifact-timestamp race was rerun standalone. Clean exact-reference Release
compilation, output and SoundBank checks, strict standalone package validation,
and `Build-Local.ps1` all passed. Two forced Unity builds produced the same
365,592-byte bundle with SHA-256
`AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B`.

The first corrected static run completed its structured PASS and all 56 sheets
seconds after the generic 120-second wrapper deadline. The game exited normally,
but that recoverable orchestration race is not used as final evidence. The
source was committed and rebuilt cleanly, producing package SHA-256
`0AC692C8D3F5EFC8D7A15968BBA8B791C6F4885D8A17156B8F8AFF2695927A5B`
and DLL SHA-256
`CCF8F81C0025762CD52835A6949848652C255F45EC7B895B083ABA4AD368B8FB`;
DLL MVID is `3e3d7594-5eab-4c58-b739-0e9e04e5326f`. Deployment manifest
`20260821T0655065885306Z/deployment.json` binds that artifact to published
commit `8aeef5e7`.

Final clean commit-bound guarded Steam evidence is:

- `20260821T0655066469058Z-weapon-presentation-evidence`: PASS 9/9;
  56 exact held/stored PNG/JSON pairs, 224 labelled views, six native controls,
  no blank or low-density sheet, no save call, exact cleanup. Result/index
  SHA-256 are
  `57582D42D5893709EA97B29BB6DD1B881661AA923E70FDD51D6C06D224D32AFD` /
  `05ADD4CD0C2BA202BE20089548C41B2D383A64175CBB55ADB8F3DC839B7E336D`.
- `20260821T0657502514655Z-weapon-presentation-eastern-motion-evidence`: PASS
  6/6; all 12 production variants plus Scimitar, Bastard Sword, and Greatsword
  controls in combat-ready and nine fixed attack updates; 150 PNG/JSON pairs,
  600 views, all 15 native commands acted, all physical blade frames
  nondegenerate/orthogonal with correct cutting-edge polarity, no blank sheet,
  no save call, exact cleanup. Result/index SHA-256 are
  `242062B3D515D1FD0697DC235285E2ADC674EFD3FB05C14BBECF524D256837A1` /
  `31266732D4D8D96B0085F6185A7379BB301BB9D4695A0C5EA29FD8B441A50084`.
- `20260821T0701587480686Z-disposable-eastern-weapons-combat`: PASS 21/21;
  all 30 item blueprints resolve their exact held/stored pair and distinct
  variant, every pair preserves the donor contract and CuttingEdge materials,
  and the complete protected mechanics/cleanup fixture passes. Result SHA-256
  is `0327284F9E9516B870E23FCA1C8021FD81B4F7CAF8DFF3E206CA7097416F0EE5`.

Direct review covered every production variant in held idle, stored, and at
least one acted frame from front, right side, rear, and front-right three-
quarter views. Wakizashi blades are no longer sideways in the light-blade
attack; Katana blade planes track Bastard Sword; Nodachi planes track
Greatsword, with both hands plausibly on the handle throughout the sampled
swing. Nodachi support-hand distance averages are `0.077418..0.093722 m`
across variants versus native Greatsword `0.093011 m`. Physical tips lead the
grips, cutting edges retain canonical polarity, each stored model uses its own
calibration, variant silhouettes/palettes remain distinct, and no severe
persistent clipping is visible in captured states.

Acceptance is intentionally bounded to held-idle, stored, combat-ready, and
sampled attacks on the default Medium male. Locomotion/turning, equip/unequip
transitions, armor/cloak, female, Small, and Enlarged remain ordinary final-
matrix work. Every save-backed run named only `KMG_AUTOMATION_WORKING`; no run
called a save API or touched `KMG_AUTOMATION_BASELINE`.

## 2026-08-21 - guarded all-family transition and locomotion checkpoint

The request-gated `weapon-presentation-transition-motion-evidence` scenario
now instantiates all 22 production variants plus the six exact native controls.
It captures four explicit states for each case: an active MainHandEquip native
coroutine, navmesh-backed movement, a native 90-degree body-relative turn, and
an active MainHandUnequip native coroutine. Every state produces a PNG/JSON
pair with front, right-side, rear, and front-right-three-quarter views.

Initial implementation attempts exposed three distinct engine layers rather
than a presentation defect. `UnitMoveTo.Start()` accepting a command did not
prove that the movement agent advanced; explicitly ticking a forced path was
rejected; and actual `UnitCombatState.JoinCombat` polluted
`Game.Player.IsInCombat`, enabling turn-based movement gating across cases.
Read-only native inspection showed that `UnitViewHandsEquipment` maintains an
independent `m_ShoudBeInCombat` transition guard. The final fixture calls that
view transition path directly, verifies the matching native animation clips and
coroutine, and never joins actor or player combat. Movement uses native
`UnitMoveTo`, a same-area `ForcedPath`, nonzero `MovementAgent` velocity, and
measured rig-bound displacement. Turning uses `ForceLookAt` and fails unless
the held model follows at least 60 degrees. Each movement record also fails
closed if actor combat, player combat, or turn-based combat is unexpectedly
active.

Repository validation passed; all 1,164 Release domain tests passed; clean
Release compilation and packaging passed; strict package validation passed;
and runtime preflight passed 124 checks after one known immediately-post-build
timestamp race was rerun. The guarded Steam App ID 640820 run is:

`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0837326051191Z-weapon-presentation-transition-motion-evidence/`

Its structured result passes 7/7 assertions in 204,192 ms: 28 exact cases,
112 PNG/JSON pairs, 448 labelled views, all native equip/unequip transitions
active and matched, every movement command accepted with nonzero velocity and
`0.367545..0.372974 m` displacement, every body-relative turn
`89.99999..90.00001` degrees, no zero-pixel sheet, exact request-local cleanup,
no save call, and loaded version `0.0.88`. Result/index SHA-256 are
`6F877A4ADA88F7D49CD4745514F2BF6D705B14FECB1E64668863CA2F52B2CF8B` /
`0A25959DCEB32254ACF609F6D7127575913AEEF7B109F67F7C2015E41A22D2F1`.
The 22,418,038-byte local-runtime package SHA-256 is
`9F08E75EACAB8FFB4A7CDEC4A49F7CD1A3F77E9B01A4A00A9EFCE229085E47DB`;
DLL SHA-256 is
`F6E7934CAB20D0C86B5C42D01AD0C0D30FABB4BAFE8299A5454BAAEC3039D5DE`.

The generic wrapper reached its deadline while the same responsive Kingmaker
process was still traversing the large matrix, so `orchestration.json` records
`ERROR` and deliberately leaves PID 18488 running. The guarded scenario then
completed normally, atomically wrote the PASS result above, and auto-exited.
The dirty-source run is valid engineering evidence bound by the exact
package/DLL/evidence hashes. Published commit
`897ec7359cc4d8f9ea1260c04ecccc93c164ce39` then received a clean exact-commit
rerun at:

`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260821T0901591703709Z-weapon-presentation-transition-motion-evidence/`

That run passed the same 7/7 structural assertions in 202,568 ms with 112
PNG/JSON pairs and 448 views. Result/index SHA-256 are
`7C32187C6148C438E5B40C18BA2C55E7154433D1EFA8D62176AAC345F70EBCA4` /
`F6AF2475F8AD5706A61CEEDB2EF69B17C389553ADFD84EB7D238AEE979B1652F`.
Its 22,418,029-byte package SHA-256 is
`93133DDD1B62E13C40A99318E8A1928D951162A79901C12D89FD9F5375E012FF`;
DLL SHA-256 is
`2F8B3F1A230FC7F1F9B7B024A59665CEC0A5AC3E64B53884C7FAB8B990EE7F03`.

Direct review accepts handguns, long guns, and branched spears in the captured
movement, turn, and transition states. It also proved that all held models
follow actor rotation rather than remaining world-space pinned. However, rear
views objectively exposed a separate Eastern stored-presentation defect that
the structural assertions did not yet cover: inherited donor scabbards floated
detached from custom Katana Reed, Katana Regal, and Nodachi Titan actors. The
clearest reproductions are
`15-katana-reed-turned-right-default-medium.png`,
`15-katana-reed-unequip-transition-default-medium.png`,
`16-katana-regal-turned-right-default-medium.png`, and
`20-nodachi-titan-turned-right-default-medium.png`. Native Scimitar, Bastard
Sword, and Greatsword controls retained correctly attached scabbards. Eastern
transition acceptance from the earlier review is therefore superseded by this
`OBS-DEFECT`; non-Eastern V12 acceptance remains valid.

## 2026-08-21 - Eastern clone-only sheath replacement

All 12 Eastern variants already provide a separately calibrated, complete
stored prefab. Retaining the native donor sheath on the same custom visual clone
duplicated that role and allowed Kingmaker to recreate the donor scabbard at an
incompatible attachment during held and transition states. The narrow repair
sets `m_WeaponSheathModel` to null only on each custom clone after exact held and
stored validation. Native donor blueprints retain their non-null sheaths. Every
other `WeaponVisualParameters` field remains equal to the donor, including
animation, trails, sounds, attachment slots, and timing. No mesh transform,
equipment root, gameplay field, item identity, projectile, or native blueprint
is changed.

Focused contracts now require clone-only sheath replacement, preserved
unreplaced donor fields, and an explicit live transition assertion. Repository
validation passed; all 1,164 Release domain tests passed; clean Release
compilation/package creation and strict package validation passed. Runtime
preflight's known immediately-post-build timestamp negative case failed once,
then the standalone rerun passed all 124 checks. The first two attempts to reuse
an installed artifact failed closed before launch because the invocation first
lacked an exact package path and then had a dirty Git state; the normal guarded
dirty-source workflow was used instead.

The exact post-repair dirty-source runtime artifact is 22,418,707 bytes,
SHA-256
`1C64964A70861C742948164D2FE9DBBE325172E6064215CC837AA304B78C3232`;
DLL SHA-256 is
`7DC0261D8DDAFCCF9AB68091B128099A4F7196FC266A63647A72C01C8F6D40CD`.
Guarded Steam App ID 640820 results are:

- `20260821T0916061387506Z-disposable-eastern-weapons-combat`: PASS 21/21;
  all 30 item presentations report `sheathModel=<null>` while each native
  family donor reports its exact non-null sheath; protected combat mechanics
  and cleanup pass. Result SHA-256 is
  `B8566D16AACF4F78808145B5694A1C5A039BE79F8CC7EF5D5D683F03E2F5FB40`.
- `20260821T0918521143567Z-weapon-presentation-transition-motion-evidence`:
  PASS 8/8; 48/48 custom Eastern records are sheath-free, 12/12 native Eastern
  control records retain a sheath, 112 PNG/JSON pairs contain 448 labelled
  views, every transition/movement/turn assertion passes, and cleanup is exact.
  Result/index SHA-256 are
  `DE63A46EDBB4DC68BBAAB6901A8A584003BD314F6262C76EEFDD25457CA4C353` /
  `82B862659418D2AA2F2B201E737CC8FD99C72B3A9BB7316197CC6ABC00598660`.
- `20260821T0925383218065Z-weapon-presentation-evidence`: PASS 9/9; all 22
  production variants plus six controls materialize in stored and held-idle
  states, producing 56 PNG/JSON pairs and 224 labelled views with no blank or
  low-density sheet and exact cleanup. Result/index SHA-256 are
  `D6A2E2F45AED132ABBFFA5469DEB06798521F57376660E14092756E2CC359CF2` /
  `25ADAF37BD7951B289626D5A3C6576D9324A8BE4B259017E6D830657961736CE`.

Direct before/after review of the exact reproductions above and every Eastern
turned-right sheet confirms that no detached donor scabbard remains on any of
the 12 custom variants. Review of all 12 post-repair stored sheets confirms each
independent custom stored model remains visible and plausibly attached. Native
Scimitar, Bastard Sword, and Greatsword controls still render their own
scabbards. This accepts Eastern held-idle, stored, movement, turning, and
equip/unequip transitions on the default Medium male.

The repair was committed and published as
`754ae076de0c02b5dd1e62691ba5905aa363432c` (`fix(presentation): replace
detached eastern donor sheaths`). Before commit, repository validation, all
1,164 Release domain tests, a clean Release/package build, strict package
validation, and runtime preflight 124/124 passed. The first clean guarded
invocation failed safely before launch when the sandbox denied the external
backup directory; the identical command was rerun with the required permission
and then used the approved Steam App ID 640820 path. This was an orchestration
permission failure, not a game or presentation result.

The clean exact-commit runtime artifact is 22,418,712 bytes, SHA-256
`82BFCA3C009BC6BCA8DC0CC23E0B89985153B727AB7848D64EE520C8BA12C3AE`.
The 3,568,128-byte DLL SHA-256 is
`80465348626E6B07570D357FBC89FE1C977E10E88290364B50D135B853C4421F`;
MVID is `5a93d383-fb11-4e66-8bcd-fdd46f8137ef`. The deployment manifest is
`runtime-evidence/deployments/20260821T0942300524847Z/deployment.json`,
SHA-256
`FA4C3B9EB2A707610A3BA424EDE31E50D9C757ABFB566C5578E82FD8CAD3C83F`.
The two screenshot runs reused and verified that exact installed artifact.

Clean guarded results are:

- `20260821T0942301027834Z-disposable-eastern-weapons-combat`: PASS 21/21 in
  90,652 ms; all 30 custom item presentations are sheath-free, each native
  family donor retains its exact non-null sheath, protected combat mechanics
  and cleanup pass. Result/runtime-evidence SHA-256 are
  `C311F1DD7FA4E82230F5183AF7BE3E12883A2A65DF549A4065EBE8A1580BBDAA` /
  `E43B7D92DAD70A405AE7C3F4A97140B5F70DBAD4A79D8AD3ACB7FB0EF2C84C70`.
- `20260821T0944317567220Z-weapon-presentation-transition-motion-evidence`:
  PASS 8/8 in 202,111 ms; custom sheath 48/48 null, native control sheath
  12/12 retained, 112 PNG/JSON pairs, 448 views, exact cleanup. Result/index
  SHA-256 are
  `A1A5E9FD2B952201A5D3C6D8C2E34D5B4AB200BE00FBD2FE8B6E907648D6B435` /
  `66D5E896557E81F1909D08DEC03C42FA73391D05374B6AAA3D7E517B93DBC912`.
- `20260821T0948158393773Z-weapon-presentation-evidence`: PASS 9/9 in
  129,484 ms; 56 exact held/stored PNG/JSON pairs, 224 views, no blank or
  low-density sheet, exact cleanup. Result/index SHA-256 are
  `15F2C61FD4F58471254733E493567A176F8B2795E6F19C721B27F50F9C7CD37D` /
  `3DB3CC5D77DBC850250E9F562DF52E5F1ED89E2B8D9196C2304C03B4D9C7F1E5`.

Direct clean-run review of all 12 Eastern turned-right sheets, all 12 custom
stored sheets, the exact before/after reproductions, and all three stored native
donor controls confirms the same acceptance. The dirty-source runs remain
diagnostic history; the clean exact-commit results above are authoritative.

Mission acceptance remains bounded to the recorded default Medium male states.
Firearm reload, handgun ready/fire and valid dual wield, armor/cloak
interaction, female, Small, and Enlarged coverage remain open. No run called a
save API or touched `KMG_AUTOMATION_BASELINE`.
