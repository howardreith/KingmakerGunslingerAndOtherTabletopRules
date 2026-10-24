# Cleanup 2 large-creature lane

2026-10-21. Assigned IDs: ART-02 Griffon, ART-05 Ettin, ART-06 Elephant,
ART-09 Brachiosaurus, ART-10 Gorgon. This is a development checkpoint; none of
these assets is declared owner-approved. The integrator owns combined production
compilation, domain tests, manifests, native transactions and PR state.

| ID | Reproduced/diagnosed | Change/disposition | Technical evidence | Native evidence | Owner acceptance |
| --- | --- | --- | --- | --- | --- |
| ART-02 | Exact baseline render shows needle feathers, disconnected-looking coarse shoulder surfaces, long narrow talons, rod/ball tail; 75 baseline parts are already outward | Continuous eagle/lion skin, broad scapular bridges with coverts, closed feather fans/vanes, shorter rooted talons, curved tail/swept tuft, blended paint | Export audit, unchanged 34-bone set, exact-byte stills inspected, deterministic rebuild | UNVERIFIED: travel/turn/attack/implemented transitions | PENDING |
| ART-05 | Source confirms eight runtime spheres and lateral Head parenting; no matched native reproduction | Original sculpted head/collar skin, neck/skull blend, sibling skull pivot and rotation follower; owned mesh/material teardown | 2,011 vertices/3,986 triangles, deterministic generated C#, synthetic authored turn/pitch/reaction previews inspected | UNVERIFIED: native fit and motion, multi-instance/despawn/reconstruction/material lifecycle | PENDING |
| ART-06 | Exact baseline has all 13 closed parts inward; culling render reproduces dark missing exteriors and coarse zigzag limbs | One opaque outward body skin, broad columnar legs/round pads/toenails, thin large pinnae, integrated trunk/tusks, thin tail, painted hide | All 20 final components closed/outward, 38-bone set unchanged, deterministic mesh/paint, front/side/party/rear/below inspection | UNVERIFIED: settled opacity under native effects, movement, camera culling, native Mastodon control | PENDING |
| ART-09 | Exact baseline has all 9 closed parts inward; primitive shoulder/neck/limb joins and short thick donor tail | Continuous painted body/neck/limbs, small head, balancing tapered tail; existing stiff Head-driven neck retained | All 3 final components closed/outward, 29-bone set unchanged, deterministic mesh/paint, synthetic exact-byte inspection | UNVERIFIED: native deformation/contact/camera/selection, both route assignments | PENDING |
| ART-10 | No specific owner defect; 52 baseline components are closed/outward and synthetic front/side/party views show plated bovine, paired horns and cloven hooves | PRESERVED: no evidence justifies an unobserved redesign; faceting and seams still need native review | Fresh exact-byte audit/render only; exported bytes unchanged | UNVERIFIED: actual travel/contact, plate response and existing breath mouth origin | PENDING |

## Provenance and scope

The retained `cleanup2-*-input.json` inputs are the exact original project mesh
exports from the current lane base, not native geometry or bind captures. They
make authoring reproducible without inventing absent Sprint 25/27 census data.
Each model source describes its regeneration command. Existing shared authoring
helpers were read/imported but not changed. No mechanical size, statistics,
attacks, breath rules, blueprint GUIDs, stable asset paths or native donor assets
were changed in this lane. No runtime was launched by this worker.

The first anatomy pass was inspected and refined again: Elephant's original
zigzag legs became rounded columns/pads and the ears became thin facing fans;
Griffon's scapular roots, foretalons and tail received actual shape changes.
Further proportion/texture calibration should be driven by native observation.
The Brachiosaurus's retained stiff-neck adaptation is an explicit limitation,
not a claim of a newly articulated native sauropod neck.

## Exact final identities

| Product file | SHA-256 |
| --- | --- |
| `assets/sprint25-giant-bodies/elephant-mesh.json` | `5afe5464331a083c0de63d7b4bee4a1266d71376fb8360f7d96b0bf7a4f13028` |
| `assets/sprint25-giant-bodies/elephant-albedo.png` | `5a973962a26dbf38208b66feeb5ffd154d469ac69b25137ae487b9d6767f2919` |
| `assets/sprint25-giant-bodies/brachiosaurus-mesh.json` | `9bd237aad9b1a99f25162e4a109814b5f72fb9da6167d2f000e88f333e24a974` |
| `assets/sprint25-giant-bodies/brachiosaurus-albedo.png` | `198b61966762624adc3ffdfb5975fe08bfc7ad4589fc6e0f318e7bbb06d30776` |
| `assets/sprint27-griffon/griffon-mesh.json` | `e3db5d71b8f4c2bf4647bbcfdcea1b2438f4a060ad3d6c1ddc3ec6649af3eab9` |
| `assets/sprint27-griffon/griffon-albedo.png` | `1ccd2ade87b48b1162910270395c32c294c7e84c20c3f961b8e720b40eddd124` |
| `src/KingmakerGunslinger/Summoning/EttinAuthoredHeadMesh.cs` | `a6a6beb2f631c256bf45c1660e367a4b1d4b00ac00c6286f0eaedf2c5c7cd372` |
| `src/KingmakerGunslinger/Summoning/EttinSecondHeadVisual.cs` | `fffdd78b7fd75dc67974a011ab3c36646071d1b0ee791a346d46d411f2930c92` |
| Preserved `assets/ungulates/gorgon-mesh.json` | `0f3c8ce10c4c0d50650ecb5a042985d1b242977716c5b7a0286754d370fdc508` |
| Preserved `assets/ungulates/gorgon-albedo.png` | `4e374e18cf44af6e90f7bda29fb0124d03a124aa5443bc4334c2d0f8caf13b7c` |

Authoring script SHA-256: giant bodies
`4e12c1163bac522595a5fac1779ee57271a770edfd0e1d6838c9866cea94f725`;
Griffon `557bb99f179cb310f2a927ed790044b7e41e50168acfa758ace276e2040bc1b4`;
Ettin `9e57df21f0207424d008e027637a68dc2b301919f939dcc0b4d5beabb5226fc2`.
Original project input SHA-256: Elephant
`ed1e0dffc817a7ebda0453c4125b07386e1b03d309ebfc84d1e2b8c027ed8087`;
Brachiosaurus `ce826df01509d0333e50491077a376a8e985f4bf8d735c7c2814aa16d434e18a`;
Griffon `4694292d3cc275590fddade1e68169968884185573cc172455daf8b875e048ac`.

## Focused commands and actual results

Installed tools: `C:/Dev/.toolchains/blender-4.5.10-windows-x64/blender.exe`
and its `4.5/python/bin/python.exe`. All final authoring runs use
`PYTHONHASHSEED=0`, `-b --factory-startup --python-exit-code 1` and absolute
output paths. Each source `SOURCE.md` contains its exact CLI shape.

- Final clean authoring passes (Elephant, Brachiosaurus, Griffon, Ettin in that
  order): **20.271 s** to product outputs; **18.596 s** to separate reproduction
  outputs. SHA-256 comparison: **7/7 identical** (six mesh/albedo files and the
  generated Ettin source). `.blend` metadata is not an export identity check.
- `python assets-source/original-models/continuous-bodies/audit_mesh_orientation.py
  <four baseline meshes>`: **0.184 s** process duration. Elephant 0/13 and
  Brachiosaurus 0/9 outward; Griffon 75/75 and Gorgon 52/52 outward. All closed.
- The local `artifacts/cleanup2-large/check_final.py` calls that same audit and
  validates finite normalized weights, unchanged bone sets and actual albedo
  hashes. **0.489 s** internal duration: **4/4 asset reports pass**, zero bad
  weights, all final components closed/consistent/outward. Final vertices /
  triangles: Elephant 9,470/13,936; Brachiosaurus 8,302/12,836; Griffon
  11,231/13,852; preserved Gorgon 1,744/3,280.
- `blender ... -P assets-source/original-models/continuous-bodies/render_mesh_data.py
  -- --mesh <absolute exported JSON> --out-dir <absolute local evidence>
  --name <creature> --up y --forward=<direction> --views side,party,front,rear,below
  --size 720`: exact shipped bytes, backface culling, **about 2.2 s per five-view
  render**. `-z` is forward for Elephant/Brachiosaurus; `+z` for Griffon/Gorgon.
  Visually inspected frames are evidence about synthetic geometry/presentation.
- Ettin `review_continuous_body.py --source <ettin-head-source.blend> ...
  --up y --forward=+z --poses <authoring-poses.json>`: **2.225 s** final ten-view
  render (initial preview 1.611 s). Authored turn/pitch/reaction poses retain a connected collar; these
  controls are not a measured donor rig or native animation.
- `git diff --check` for owned production/asset paths: **PASS** (exit 0).

Machine-local evidence lives under this worktree's ignored
`artifacts/cleanup2-large/`: `baseline-assets`, `baseline`, `candidate`, `audit`,
`repro-assets`, `repro-work`, `final-asset-receipt.json`. It is not staged for
commit or packaged. No editor `.blend`, raw PNG evidence or Python cache belongs
in the tracked change.

## Integration and deferred checks

Add `Summoning/EttinAuthoredHeadMesh.cs` to the production compile list. The
existing Sprint 29 reader checks the `attached;heads=2;` prefix, which remains.
The full current marker is
`attached;heads=2;original=1;project=1;authored=1;continuous-neck=1;orbit=0`;
these fields describe construction/configuration, not native visual acceptance.
The old primitive `parts=8` field is removed. `CaptureOwnedResources` now
includes the owned Mesh and still collects renderer-instantiated materials.
The Griffon metadata now correctly records up to four influences per vertex.

Combined production compilation/domain tests and product/DLL/package identities
are owned by the integrator. Native before/after comparisons, actual canonical
casts/route assignments, real travel/turn/attack/contact, two-instance teardown,
reconstruction, material/opacity changes over time, camera culling and controls
are pending. No omitted native check is reported as PASS.

## Mission-owned current-stack native wrapper

The later independent harness task adds only three new script files:
`Invoke-SummonCleanup2CurrentStack.ps1`,
`SummonCleanup2CurrentStack.Common.ps1`, and
`Test-SummonCleanup2CurrentStack.ps1`. Existing shared runtime scripts are
unchanged. The old Sprint 22 exact-eleven-mod guard is retained; this new entry
point independently inventories the actual current identifiable stack instead.

The public wrapper admits only the fixed creature-review scenario, exact
`KMG_AUTOMATION_WORKING`, unique typed mission creature keys, an explicit package,
expected version, and either `summon-cleanup-iteration2` or
`summon-cleanup-iteration2-baseline`. Both purposes require the existing strict
development receipt/package reader and exact clean source/commit binding.
A downloaded release ZIP without the required development receipt is refused.
No profile, arbitrary scenario/parameters, install-path override, force-kill,
dirty-source allowance or fixture-mode switch is exposed.

It reuses the existing compatibility transaction to snapshot/restore all Mods
and the managed sound bank, copying current peer mods, root-level Mods files
and actual current FeatureModules settings into staging. It records the actual
IDs/versions/per-mod manifests and complete original manifest. All existing save
files, including Working and the immutable baseline, are protected by the
existing OS read leases and exact hash/metadata checks. Filename intake rejects
missing/nonunique Working or baseline; the qualified native loader must still
prove the captured descriptor identity. No save is created, written or repaired.

Deployment and the existing qualified save-loader/creature-review launch share
the compatibility lease and explicit package/deployment identity. A fresh child
must exit before independent settings/save and installation restoration. A
foreign original-backup mutation or ownership sentinel/lease is refused before
overwriting it. Preflight settings changes are retained as foreign evidence;
settings are restored only after an owned native launch attempt. If Kingmaker
remains alive past the bounded exit wait, no unsafe restoration occurs: the
receipt records `WAITING_FOR_NATIVE_EXIT`, and ownership/recovery remains open.
Failures and independent restoration errors are retained in `installed-stack.json`.

Example for the integrator, after the matching clean-source development build:

```powershell
.\scripts\Invoke-SummonCleanup2CurrentStack.ps1 -PackagePath <validated-development-package> -ExpectedVersion 0.0.163 -DevelopmentPurpose summon-cleanup-iteration2 -SaveName KMG_AUTOMATION_WORKING -CreatureKeys @('elephant','brachiosaurus') -Confirm:$false
```

Focused no-game command:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SummonCleanup2CurrentStack.ps1
```

Final execution: **110 checks, 0 failures, 11.9769312 s**. Actual orchestration,
development receipt reader/source-binding, compatibility stage/restore, runtime
lease ownership, save read leases and independent restoration run against bounded
fixture files. External package-content validation/compiler provenance, Git
queries, registry opening, process checks, deployment and native execution are
fixtures. This tests orchestration control flow; it is not native qualification.
The production wrapper continues to use the existing strict package validator.

Cases include both named purposes; a sixteen-mod current intake and root files;
missing/duplicate IDs; missing/ambiguous Working/baseline; purpose, version,
commit, dirty/source/hash/identity refusal; unsupported public parameters;
child failure/wrong receipt; settings failure without skipped installation
restoration; foreign sentinel/lease/original-backup refusal with independent
settings restoration; foreign prelaunch settings without overwrite; save-write
denial/new-save refusal; and a still-live child without unsafe cleanup. Final
evidence is local under
`artifacts/cleanup2-currentstack-tests/ec16cdee01a042759429bba5ee4ed75f/`.
Earlier 74-check and 104-check runs remain in their original artifact directories.
All three new scripts parse without errors; `git diff --check` passes.

Exact script SHA-256 identities:

| Script | SHA-256 |
| --- | --- |
| `Invoke-SummonCleanup2CurrentStack.ps1` | `88c3daa180cbce75b2132945b7b0cf28399929701a2cbb178ea0d9101f99b7c0` |
| `SummonCleanup2CurrentStack.Common.ps1` | `e695ca39fe484cbb4a51ef8cc36e3b6c5036e31fc7c4a8934685e17492c52574` |
| `Test-SummonCleanup2CurrentStack.ps1` | `fe364e72f2fdd5507e71e12e6629170d63f6e2bddeb90eff8f7fe38cda82a4cd` |

No game, build or Git mutation was performed for this harness task. Actual native
baseline/candidate runs and review remain the integrator's responsibility.

## Cleanup 2 save-free phase plumbing

A subsequent integration task unions the exact
`disposable-expanded-summoning-cleanup2-native-review` scenario into four existing
PowerShell surfaces: `RuntimeAutomation.Common.ps1`,
`ExpandedSummoningOrchestration.Common.ps1`,
`compatibility/Invoke-KingmakerCompatibilityProfile.ps1`, and
`Test-RuntimeScenarioPreflight.ps1`. It retains every existing scenario/phase.
The new scenario has save-free mod-load metadata, requires automatic exit and
exactly one case-sensitive string `reviewPhase` key whose value is `giant-lineup`
or `menus`, and serializes only that key. No generic authorization is broadened.
Compatibility profiles forward the same validated parameters, use the exact
current-run receipt/restoration boundary, and identify evidence as
`cleanup2-<profile>-<phase>`.

Focused command:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RuntimeScenarioPreflight.ps1
```

Result: **786 checks PASS, exit 0, 27.3316924 seconds**. Both phases are admitted
and round-trip with exactly one parameter. Missing, broad, unknown, wrong-case
key/value, nonstring, extra/save parameters and nonautomatic exit are refused.
The check also confirms the existing C#/PowerShell scenario union and old
preflight contracts. All four scripts parse with zero errors and their focused
diff check passes. No game, build or Git mutation was performed. Raw log:
`artifacts/cleanup2-large/cleanup2-runtime-preflight.log` (local only).

| Script | SHA-256 at this checkpoint |
| --- | --- |
| `RuntimeAutomation.Common.ps1` | `3d751d2ba623dbd0e56e9d3c0886b41cca7f16d51369b04c804751a7e3ae9bc4` |
| `ExpandedSummoningOrchestration.Common.ps1` | `c54808be762211f9fc2819103d349b2b9d01221695a60313d66bce215ff5d7aa` |
| `compatibility/Invoke-KingmakerCompatibilityProfile.ps1` | `71d00fe803d2363e0d829d56d11fc24972bf8ad12dc2be435a1a31c2af5fee23` |
| `Test-RuntimeScenarioPreflight.ps1` | `2e665e2961212135962b76acd0e05dd383d02f09f466e5fcc3d305867748ef1e` |

Static inspection of the separately reported Build-Local outer exit 1 did not
establish a cause. Its development path writes a receipt before consumer
read-back validation; a receipt file alone therefore does not prove the final
success path completed. The final success messages and original outer command
are needed to distinguish a read-back failure from outer-shell status handling.
Build-Local was not modified or rerun by this worker.
