# Cleanup 2 arthropod, hand and Stegosaurus authoring checkpoint

Date: 2026-10-21. Scope: ART-01, ART-03, ART-04 and ART-07. These are authored
asset repairs on the integrator's `codex/*` worktree. No game launch, package,
runtime/C# helper, compile list, manifest, stable catalog identity, gameplay
rule, save or Git mutation was performed by this lane.

Implementation and offline verification are complete for review. Native
visual/mechanical evidence and Howie's aesthetic acceptance are **pending**.
The integrator owns current-installed intake, matched public v0.0.163 native
baseline/candidate staging, actual canonical casts and package identities.

## Reproduction, changes and remaining checks

| ID / published route | Observed authored baseline | Actual repair | Focused evidence | Native work / owner status |
| --- | --- | --- | --- | --- |
| ART-01 Giant Crab / SNA III plus higher quantities sharing `giant-crab` | Exact shipped-byte studio renders show a broad shell with very thin raised legs, round separate palms and small spike fingers. | New low broad closed shell, eight connected robust segmented leg surfaces, two asymmetric continuous open chelae with flattened palms and rooted eyes. Original mottled warm-brown chitin, rim and dark tips. No whole-view scaling. | Closed/wound/outward, all eight complete driver chains, both complete seven-driver chela chains, no abdomen/chelicera geometry, weights and seams pass. Exact exports reproduce twice. Synthetic stride/claw probes inspected. | Native ordinary zoom, both sides/three-quarter, actual travel/turn/claw contact pending. Forward-walking spider donor limitation retained. Owner review pending. |
| ART-03 Giant Stag Beetle / SNA IV and higher quantities sharing `giant-stag-beetle` | Shipped-byte renders show wedge-like thick legs and coarse raised cases. | Distinct head/pronotum/abdomen, curved antler jaws with inner tines, paired convex mahogany wing cases, compact clubbed antennae, six slimmer jointed legs. | Six connected full leg chains; fourth pair absent; two jaw components; closed outward surfaces, normals/weights/seams pass. Exact mesh/paint exports reproduce twice. Synthetic jaw/stride probes inspected. | Native contact, leg/jaw motion, turn and current unchanged view-scale policy pending. Fire Beetle and ants untouched. Owner review pending. |
| ART-04 Girallon / SNA V and higher quantities sharing `girallon` | Shipped-byte renders show palms and digits as separate capped components. Baseline fails the new connected upper-hand driver contract; strict topology audit also finds inconsistent directed edges in the older repaired export. | Four closed arm-through-fingertip surfaces, palm webbing, blended joints, buried shoulder roots, proper UV seams with identical weights/normals. Upper digits articulate; lower hands are authored simplified shapes on forearm-twist driver to avoid applying the donor's upper knuckle pivots to offset fingers. Existing original albedo retained. | Two connected arm/hand surfaces per side; upper hands include all thumb/three-finger terminal controls; all 56 reviewed drivers retained. Equal seam weights guarantee seams cannot tear under any driver transform. Closed topology/normals/weights pass. Final export reproduces twice. Three synthetic wrist/shoulder/digit/hit probes inspected. | Native near/far hand idle, walking, turning, strike/hit and extreme wrist/shoulder motion pending. Simplified lower hands and shared two driver-chain limitation are explicit. Owner review pending. |
| ART-07 Stegosaurus / SNA VI and higher quantities sharing `stegosaurus` | Exact shipped-byte side render reproduces the nearly spherical radius-.86 overlapping torso, obscured legs and poorly separated head/neck. | One elongated elliptical trunk with four integrated weighted legs, low narrow neck/small head, high hips, muscular narrowing six-joint tail, nine rooted staggered plates and four spikes. New original mottled olive skin with bands/light belly. | One connected surface carries all limbs/neck/head/tail controls, plates/spikes are closed rigid parts; all surfaces outward and normals/weights/seams pass. Exact mesh/paint exports reproduce twice. Synthetic stride and tail sweep inspected. | Native side/rear, walking/tail contact, camera/selection usability pending. Triceratops untouched. Owner review pending. |

Canonical publication identity was read from the existing catalog; no route
assignment was changed. Native ability resolution and multiplicity remain the
integrator's check. No domain/offline PASS here establishes actual game motion,
opacity, contact or owner approval.

## Exact asset identities

| Creature | Mesh SHA-256 | Albedo SHA-256 |
| --- | --- | --- |
| Giant Crab | `34833285f367258ed5d00127c72d86bb96897df94459740d3c9fd800e720893e` | `1182b139b3a8dc049eaa68d7db8d9bcf865d02e321af2e8d13b46688866409ce` |
| Giant Stag Beetle | `2ecf222413eb857051b70df8b5fd61592a939108c7661ccdfb96f20fbcea794d` | `967d506b96019914fd40363cc80b21b0ce08f80662ef96ef259f5e0a8caecde4` |
| Girallon | `e5c4e59878ff6aae7b704f8fb49925e849f348817fc160e97a1f76907ee683cb` | `21e970e238983c79971709e05d562c3b04b5534a2019eeb55b491ff31fc0dfe6` (unchanged) |
| Stegosaurus | `4ea887c98387d90d546e5512cf862cd47cb5694b0bbb1f99431c83c1a12c95ef` | `27ef9768948c6f28a2836a28cfa45e9be03220b8f590d15af656212e540511bf` |

| Creature | Export vertices / triangles | Closed outward components | Front-normal agreement |
| --- | --- | --- | --- |
| Crab | 8,521 / 11,516 | 15 | 100% |
| Stag Beetle | 10,092 / 14,804 | 15 | 100% |
| Girallon | 8,293 / 16,180 | 44 | 99.969% |
| Stegosaurus | 7,775 / 11,436 | 17 | 100% |

The small Girallon normal disagreement affects four triangles in the retained
pelvis/trunk and one triangle driven by the right upper-arm control. Every
component is closed consistently outward. This small smooth-normal limitation
is retained explicitly for native inspection, not counted as visual approval.

## Reproducible commands and evidence

Toolchain: `C:/Dev/.toolchains/blender-4.5.10-windows-x64/blender.exe`, Blender
4.5.10 LTS (`6dc0b208d1b5`). Bundled Python 3.11 at
`4.5/python/bin/python.exe`. No new tool installation was needed.

Builders, each with `--capture`, `--albedo`, `--mesh-data`, `--report` and
`--blend-out`, are documented with complete command lines in each species'
`SOURCE.md`:

- `sprint21-arthropods/build_giant_crab.py` uses the private Sprint 14 Giant
  Spider frame. The Sprint 21 independent census identified the same donor;
  actual input SHA `60074cc2edb939cd4ab3e49961eec470866f94096d40e97f490dedae60faa19b`
  is recorded instead of substituting the older Sprint 21 provenance hash.
- `sprint14-insects/build_giant_stag_beetle.py` uses that same measured frame.
- `sprint19-fourarmed/generate_fourarmed.py --kind girallon` uses private
  Sprint 18 primate capture SHA
  `328443db6c9b77651ab64212c7a654284c722c9ddeb6277ce66d289d72f6a4a5`.
- `sprint23-armored-dinosaurs/build_stegosaurus.py` uses private Sprint 22
  Hodag capture SHA
  `33ce3ad77a73d752d53aeae7de9effdc3b3b20a94fc8b2949d511d697f72a1c1`.

The prior Crab/Stag/Stegosaurus generators refuse accidental regeneration of
superseded anatomy and point at their replacements. The shared
`continuous-bodies/` library is unchanged. New local
`sprint21-arthropods/cleanup2_authored_parts.py` reuses it for union/export,
painting and weight interpolation. New synthetic pose JSONs are editable
authoring parameters and contain no native animation curves.

```text
python assets-source/original-models/sprint21-arthropods/verify_cleanup2_bodies.py --root . --out artifacts/cleanup2-arthropod-hands/geometry-verification.json
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/continuous-bodies/render_mesh_data.py -- --mesh <absolute mesh.json> --out-dir <private output> --name <key> --up <y|z> --forward=<-y|-z|+z> --views party,side,rear,front --size 1000
blender -b --factory-startup --python-exit-code 1 --python assets-source/original-models/continuous-bodies/review_continuous_body.py -- --source <private source.blend> --out <private output> --name <key> --up <y|z> --forward=<-y|-z|+z> --views party,front,rear,side --poses <species stress-pose JSON> --size 1000
```

The first coherent four-builder authoring batch measured 5.62s Crab, 6.57s
Stag Beetle, 5.49s Stegosaurus and 17.31s Girallon. A later Girallon-only repair
removed lower-finger orbiting discovered by the synthetic stress render;
its final export was independently repeated and matched exactly. Those later
individual timings were not separately instrumented. Seven final regenerated
files compare equal between independent processes (four meshes, three new
paintings); Girallon painting is the existing unchanged input.

Final focused verifier: **4 creatures PASS, 0 FAIL**, with per-assertion
anatomy/topology/normal/weight/seam/albedo checks. Baseline Girallon connected
hand reproduction: **expected FAIL**. The baseline also fails the stricter
directed-edge topology check; that is an old export defect, not a candidate
failure. Production compilation/domain contract integration remain owned by
the integrator, including intentional migration of the old four pinned
`RepairedBodyTests` asset identities.

Machine-local evidence under this worktree, deliberately uncommitted:

- `artifacts/cleanup2-arthropod-hands/baseline/`: exact prechange mesh/albedo
  copies and full-body shipped-byte party/side/rear/front studio renders.
- `artifacts/cleanup2-arthropod-hands/candidate/`: final Blender sources,
  reports/build logs, exact-payload stills and `*-stress/` synthetic probes.
- `artifacts/cleanup2-arthropod-hands/repeat/`: independent export bytes,
  sources and logs proving deterministic regeneration.
- `artifacts/cleanup2-arthropod-hands/geometry-verification.json`: final
  four-creature machine-readable authored-byte result.

The studio/synthetic images are **not native game captures**. They support
review of anatomy and weighting and must not enter a native acceptance row as
PASS. Native package/DLL identities are intentionally unset here.
