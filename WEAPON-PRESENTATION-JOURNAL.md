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
