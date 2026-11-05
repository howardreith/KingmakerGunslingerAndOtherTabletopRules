# Summoned creature visual overhaul: active checkpoint

## PR #96 review correction continuation

Owner-authorized additional validation allowance: 30 minutes beyond the retained
26.80-minute initial ledger below; planning target 15 minutes. PR remains draft.
This selects focused review corrections and native acceptance, not release qualification.

The reviewed source is `3611f921ddeb4c2af65a7f095e9b140527bea26c`.
Reproduced atlas crossings: Nalfeshnee 545 and Bone Devil 176 triangles. The
corrected remesher assigns a whole face to one atlas tile and splits UV seams
while retaining identical smooth normals and weights. All nine affected atlas
exports now have zero crossings or seam normal/weight mismatches. A deliberately
poisoned triangle proves the checker rejects valid-corner/interior-crossing UVs.
The Kyton link basis is perpendicular with a stable vertical fallback: all 64
hanging links have a 0.027 minimum opening semi-axis versus 0.010 tube radius;
four owned regions retain their runtime ownership. The five retained scale
entries now reject module-OFF before touching or marking a view.

Art refinements strengthen Hezrou jaw/brow and planted legs, Glabrezu head and
separated four-arm silhouette, Bone Devil skeletal joints/pelvis/feet/membranes,
and Kyton clothing/asymmetric chain arrangement. Nalfeshnee retains the heavy
head/trunk with larger hands, tapered calves and planted toes. Vrock retains
feather masses with less regular primaries. Babau, Barbed Devil, Bearded Devil
and Lillend receive scoped anatomical transitions. Protected Satyr, Shadow Demon
and Invisible Stalker exports are unchanged. Offline inspection is not native
acceptance or owner aesthetic approval.

Pre-candidate focused production compilation and 78 selected domain tests PASS
(26.52 seconds). Exact OFF-admission positives/14 negatives and existing 285
orchestration assertions PASS. Three chain regression tests PASS. These checks
and asset checks conservatively reserve 0.60 minute of the new allowance.

Plan: freeze this source/asset/observer batch; build one immutable development
candidate with exact updated pins; use four guarded fresh processes for indoor,
daylight/motions/colliders/module-ON reconstruction, Wall formation/obstruction/
destruction/original-deadline controlled expiry, then actual module-OFF view
reconstruction. Native statuses remain NOT RUN until receipts exist. Source
contains observations, not an unproven Wall mechanical repair. Preserve actual
installed intake, all save bytes/metadata and native settings transactionally.
Expected build 7–8 minutes; native work reserved approximately 15–20 minutes.


Owner mission: `C:/Dev/KingmakerGunslingerLab/Codex mission for summoned creature visual overhaul.md`.
Branch: `codex/summon-outsider-visual-overhaul`. Baseline fetched from approved
origin: `ffb1f8bf2c0014ee6bb5465103252195f75e449b`; latest published release
independently checked as v0.0.177, ZIP SHA-256
`47ef6ee375164ab367ee8e8a7ecf178e60aa7344e3983127938abb18957014e4`.
The actual local installation reports v0.0.140. It is protected intake, not a
license to leave a development build installed. No release, merge or next
roster sprint is selected.

**Status: reviewable development work; mission acceptance incomplete.**
Technical validation, native visual review and owner aesthetic approval remain
separate. The owner located all twelve references in
`C:/Dev/KingmakerBuffPlannerLab/handoff`, with `ChatGPT Image Nov 4, 2026,
08_51_46 PM.png` through `08_52_31 PM.png` names. Each image was viewed and
matched to its visible creature name/combat log in mission order. The report
embeds the supplied images in expandable panels alongside the explicitly
labeled offline v0.0.177/revised export comparisons. Raw inputs stay unchanged
and out of Git. The Stalker outline, successful dark bat-winged Shadow Demon,
detached Kyton arcs and Lillend garment concerns were confirmed from these
references; final native states still require observation.

## Current per-creature disposition

| Creature | Substantive change or preservation | Native review | Owner approval |
| --- | --- | --- | --- |
| Nalfeshnee | Continuous heavy trunk/thorax, boar face/tusks, broad hands/feet, closed small membranes; 2.10 display step | NOT RUN | pending |
| Glabrezu | High supporting thorax, dominant larger pincers, forward smaller arms, connected skin; 2.20 display step | NOT RUN | pending |
| Vrock | Overlapping flattened vanes/covert row, coherent thorax, skin/beak/plumage values; 1.45 display step | NOT RUN | pending |
| Babau | Lean connected anatomy, ribs/iliac relief, cheeks/inset eyes, more readable dark skin | NOT RUN | pending |
| Hezrou | High broad thorax/mandible, restrained eyes, shallow mouth/teeth, varied olive hide; 1.45 display step | NOT RUN | pending |
| Barbed Devil | Connected body, 34 rooted shoulder/back/limb thorns and visual tail | NOT RUN | pending |
| Bone Devil | Ivory skull/ribs/pelvis, neck bridge, thin wing plates, rooted stinger; 1.45 display step | NOT RUN | pending |
| Bearded Devil | Connected soldier anatomy, face and jaw-rooted beard; native glaive retained | NOT RUN | pending |
| Chain Devil | Fitted body chains/leather/cuffs, four cuff-rooted drapes, regenerated owned vertex receipts | NOT RUN | pending |
| Lillend | Fitted closed bodice/peplum, blended fabric weights, refined shoulders/arms/neck/face | NOT RUN | pending |
| Invisible Stalker | Preserve exported bytes after offline geometry/source inspection; its special material path is intentionally airy | invisible/selected/revealed NOT RUN | pending |
| Shadow Demon | Preserve exported bytes, bat-winged identity and intentionally dark native material path | transparency/motion NOT RUN | pending |
| Satyr | Protected accepted appearance: no source, export, binding, material or scale-entry changes | regression observation NOT RUN | prior owner acceptance preserved |

Glabrezu's smaller arms and Vrock/Nalfeshnee wings retain chest ownership on
the measured two-arm carrier. They do not gain independent articulation or
attacks. Glaive grip, chains through movement/turn/contact/removal and two-owner
isolation, Bone tail contact, Lillend equipment/spell poses, view reconstruction,
matched indoor/outdoor lineups and mixed-group performance remain native review
requirements until specifically recorded below.

## Darkness diagnosis and bounded authoring repair

The exact baseline Nalfeshnee audit found 64/64 closed, consistently outward
components and no normals opposing their triangles. An exact-byte studio
render reproduces dark surfaces and visible primitive seams. Another blanket
winding flip is unsupported and was not applied. Source paint is very dark
red/brown (body .252/.132/.092 before its modulation); much anatomy was separate
rigid tube/ellipsoid shells, including ribbed tube membranes.

The correction is opt-in original authoring: voxel union and relaxation of
selected organic atlas regions, existing outward checks, nearest-surface UV/
weight transfer and blended maximum-two influences. Discrete teeth, horns,
feathers, chains and wings retain intentional ownership. Revised paint lifts
surface values with local creature-specific separation. No shared shader,
global brightness, emission or original material asset is changed. This proves
an offline authoring improvement, **not** a complete native lighting root cause.
The existing native appearance observer now records settled material properties,
parameter texture bindings, local keywords, transform determinant, actual
renderer bounds and a live image for the affected outsider identities.

`outsider_surface.py` is imported only by the revised outsider generators.
The locally retained geometry-free capture has SHA-256
`6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d`.
Running the unchanged Nalfeshnee generator with it regenerated the released
position/normal/UV/index/weight blob and albedo manifest byte-for-byte. The
historical capture hash `727645...ad7a4` is preserved; loaders admit exactly
these two reviewed receipts, while new exports truthfully name their input.
No native mesh, pixels or animation curves were exported or committed.

## Scale and attachment diagnosis

Installed Assembly-CSharp SHA-256 is
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`,
MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, matching the inspected local IL.
`UnitEntityView.GetSizeScale` returns 1 when current size equals original
blueprint size; it does not compare a Huge creature to its Medium donor rig.
The original outsider bodies are about humanoid height in authored units.
`UnitEntityView.Update` also writes `localScale = m_OriginalScale * m_Scale`
each frame, erasing a one-time local transform adjustment.

Five named display corrections therefore update only each live view's native
scale baseline once, with the existing weak-table idempotence guard. The
catalog's unrelated entries and original bounds remain unchanged. Their
approximate intended body heights are 4.1/4.3 units for Nalfeshnee/Glabrezu,
2.7-2.9 for the three revised Large outsiders. These are source-derived display
targets; matched native humanoid/Large/Huge bounds, selection/collision,
navigation, attack contact and reconstruction stability remain unverified.
Correct original Huge/Large mechanics, reach and movement rules are unchanged.
Lillend keeps the same serpent/wing footprint and 1.0 display step; its revised
upper bound is 4 mm lower, with X/Z bounds and original tail rest/slap unchanged.

Kyton's old authored origins sat roughly .121 units from the palm; the new
.032-unit roots sit inside modeled wrist cuffs. Four disjoint, fully palm-owned
attack regions and their native weapon timing remain. Inspection did not
demonstrate faulty runtime skin-space math, so that math was preserved.

## Validation and artifact record

Baseline attempts and genuine tooling failures are retained in the separate
[Wall investigation](SUMMON-OVERHAUL-WALL-OF-ICE.md). They consumed 661.01 s,
plus approximately four seconds of exact-current-reference export. No baseline
native process was launched. No historical PASS was relabeled.

Changed-worktree focused gate: **PASS**, production compilation 15.3 s,
68/68 selected tests (2,810 registered), 24.959 s total. Command:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.sprint3;expanded-summoning.sna-naming-scale;expanded-summoning.sprint16-view-scale;expanded-summoning.sprint17-snake-production-body-identity;mission.combat-log-publication'
```

Lillend export checks passed 3/3 in 6.456 s; all 132 parts closed/outward.
Other changed meshes receive the existing orientation audit during report
generation. Exact final mesh/albedo hashes, triangle/vertex counts and audit
results are written in the local `review-manifest.json`.

The first final development build at `3781d9bdf4ba` ran 441.604 s: selected
68/68 tests, exact-reference production compilation, supply-icon/native
companion checks and package assembly passed; strict packaging then correctly
rejected the old Lillend export pins. The missed packaging pins for all ten
revised models were updated to the reviewed exported bytes, retaining the
preserved shadow-outsider and unrelated pins. The failed archive/log remains
in its original location without a successful development receipt. Narrow
positive validation and a valid-JSON tampered-Lillend negative fixture are
recorded separately before the corrected candidate build. No package from
this failed attempt is deployed or presented as a qualified candidate.

Corrected immutable development candidate: **PASS**, 450.449 s, source
`0ecccbf2c601c33ce250ac285bca1d3a529accb1`. The command was the existing
`scripts/Build-Local.ps1 -DevelopmentPurpose summon-outsider-overhaul
-DevelopmentTestFilter '<same focused filter above>' -ReferenceBundleDir
artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`.
It ran the development repository wrapper, 68/68 selected tests (7.4 s runner),
exact-reference production compilation, supply-icon checks, 37 native-contract
companion assertions, archive assembly and strict standalone validation. The
archive has a successful development receipt, `releaseQualification=false`.

| Candidate identity | Value |
| --- | --- |
| ZIP | `artifacts/development-candidates/summon-outsider-overhaul/0ecccbf2c601/KingmakerGunslinger-0.0.177-development-summon-outsider-overhaul-0ecccbf2c601.zip` |
| ZIP SHA-256 | `56bef143018617b63943b5316019e47edacd8ba35220c5c5afa14dadfd540238` |
| DLL SHA-256 | `cf24da87b7d4f3718e52ba724506bb3392d52794709edb02b96fd4c5ce847250` |
| DLL MVID | `ecfa329f-954c-4361-b89b-9530e9a12470` |
| source-state SHA-256 | `da26f56855a57fc1249bc3b4981ccb9164285cdd323e3463a750fe220dedd822` |
| receipt | the ZIP path plus `.development-receipt.json` |
| build log / wall duration | `artifacts/summon-overhaul/corrected-candidate-build.log` / `corrected-candidate-duration.txt` |

The packaging-pin reproduction passed on the retained archive, and a second
archive containing a valid-JSON Lillend mesh with changed bytes was refused by
the exact hash check: positive exit 0, negative exit 1, 19.715 s combined.
No failed archive received a successful receipt. The original failed verdict,
the corrected-reader result and the new candidate result are distinct.

**Cumulative instrumented validation: 1,608.19 s (26.80 min)**, including
baseline prerequisite/failure attempts, approximately four seconds of reference
export, the 24.959 s focused gate, Lillend's 6.456 s checks, the 19.715 s pin
reproduction, the 441.604 s failed candidate and the 450.449 s corrected one.
Authoring/render iterations were separate; their uninstrumented review time
does not supply native qualification. The existing early35/core39 native
sequence is expected to take another 6-10 min and would exceed the owner's
30-minute validation ceiling, so it was **not launched**. No scene, save,
installation, native game setting or Steam authentication was mutated by this
work. The actual installed Info still reads 0.0.140 and no Kingmaker process is
running at handoff.

All ten revised exports have closed/outward/consistent components. The
manifest retains normal/triangle-corner disagreement statistics (including the
older chain/tail cases); this is not proof of settled native shading. Final
darkness diagnosis, matched indoor/outdoor size and material comparisons,
motion/contact/second-owner/reconstruction/mixed-group evidence and every
native disposition in the table remain open. Wall's actual formation,
obstruction, destruction/expiry cleanup and original 720 s expiry demonstration
remain open. The feedback change does not close the functional defect.
Full suite, compatibility, persistence, release reproducibility and qualification
remain deferred by policy.

## Regeneration and illustrated review

Run the respective existing Blender 4.5.10 generator with the measured capture,
the exact asset basenames, and private report/blend/preview paths. The revised
sources keep their normal `--kind`, `--capture`, `--albedo`, `--mesh-data`,
`--report`, `--blend-out`, `--preview` interfaces. Lillend keeps its existing
human-bind capture and direct `generate_lillend.py` pipeline; see its README.

```powershell
& '<Blender bundled Python>' scripts/Export-SummonOutsiderVisualReview.py --blender '<Blender executable>' --before-ref ffb1f8bf2c0014ee6bb5465103252195f75e449b --owner-reference-dir C:/Dev/KingmakerBuffPlannerLab/handoff
```

Review document: `artifacts/summon-overhaul/review/Summoned-Creature-Visual-Review.html`.
It embeds images of the actual before/after exports, explicitly labeled as
offline previews, and an owner checklist. Raw machine-local captures, exported
private inputs, models' blend scenes, report images and packages stay ignored.
Shape references are credited in the report to Roman Kurbanov's verified WotR
Vrock and Babau production-art pages; no third-party artwork is embedded.

Draft PR: [#96](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/96).
The branch was pushed through the approved policy script. The report's cover
and model-card layout were checked with an isolated headless browser; all
reference images and representative exact-export front/party views were inspected,
with side views used for attachment questions. Its embedded game
references use scaled JPEG review copies with raw source hashes retained in a
local manifest; the original supplied images are unchanged.

Next action, after a further native validation allocation is selected, is the
existing save-free early35 phase and a representative Nalfeshnee/core39
material comparison, followed by the other exact affected phases. The prepared
unrun driver is `artifacts/summon-overhaul-baseline/run-final-native-review.ps1`.
Keep the candidate unchanged and satisfy its exact source/root receipt guards:
use source `0ecccbf2c601` clean in its original root on a separate codex branch,
preserving this PR branch and its records, rather than attributing the binary
to this documentation-only handoff HEAD. Do not rebuild merely to stamp docs.
An incomplete native handoff is not mission acceptance or owner art approval.
