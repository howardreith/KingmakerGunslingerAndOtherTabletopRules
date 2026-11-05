# Summoned creature visual overhaul: active checkpoint

## Current handoff: code corrections compiled; extra native allowance pending

The second-review camera defect is fixed. Appearance collection is metadata
only, matched views and Lillend casting use the ordinary party camera, and
the Default lease remains authoritative over manual clock pulses. Model and
albedo exports are unchanged from reviewed `b2f65d53`; all twenty exact hashes
were rechecked. No speculative art or scale-logic revision was made. The three
previous defects remain closed, with the earlier OFF evidence credited.

Candidate `162d089e2636` produced sharp inspected indoor and daylight images,
successful Nalfeshnee movement and ON reconstruction, and successful movement
for six more revised creatures. Its longer phases exposed additional observer
setup failures; they remain failures, as described in the following section.
The Wall did form and alter actual movement, but its full acceptance is open.

Compiled follow-up source: `fa457ca668cfed7affd4347a18a98a40070fc012` (includes
`caf42be67556292aaeae9c880679aed4a7d4b8e4`). It removes the hostile-witness
overlap, uses supported/settled Wall witnesses, clears witness occupancy before
bare-grid inspection, supports the actual Recast backend, records grid cleanup
diagnostics, keeps the humanoid reference near the movement lane for ordinary
vision, and captures the formed Wall before later checks. Strict physical
damage and walkability-restoration predicates remain. No material, fog,
brightness or depth-of-field override is supplied. These corrections are
**unpackaged and native NOT RUN**; no mechanical Wall repair is claimed.

### Exact current native results

All four runs used candidate `162d089e2636`, the exact ZIP/DLL identities in
the next section, and restored installed v0.0.140, native settings and every
protected save byte/length/mtime exactly. No save was loaded or mutated.

| Phase | Status / assertion total | Runtime-result SHA-256 |
| --- | --- | --- |
| indoor | PASS / 4 | `ac319c288f287d4e4670c69ed82b6558f174f9ce3a5323f6470d827a3cc1fbba` |
| daylight | PASS / 11 | `c608ca658f2882b144b8b9498a850b35180f9c44855b83865da3da02460e023f` |
| motion | FAIL / 33 | `8bd55768c6dbe2e641e3cd09f6eb850ac2a15f5a0be33ce9810872ad8a619a0c` |
| Wall | FAIL / 17 | `615c066745059ed752566fd139ede37e2318d6e978bb03aaefe91b9c16abe3ed` |

The motion failure remains Lillend's interrupted/unstarted supplied command;
translation alone is not success. Its later frame also reaches the ordinary
fog edge and cannot establish casting/deformation quality. Glaive/Kyton pairs
were not reached. The Wall run proves original 36 HP/720-second creation,
baseline/live mover and detour, controlled native fire destruction and returned
Recast route. Physical breach and exact grid checks failed; expiry/tail were
not reached. Witness/graph corrections require native confirmation. Selection
surfaces/rays and open-lane movement do not prove comfortable crowded clicking
or confined-space movement; those remain manual playtest observations.

### Current validation accounting and next action

Measured seconds: initial focused compile/28 tests 36.35; one development build
681.87; indoor 171.57; daylight 163.13; motion 201.07; Wall 212.41; narrowed
setup compile/26 tests 29.48; Wall capture compile/18 tests 22.51; final reference
helper compile 16.56 FAIL (missing namespace, retained); corrected compile/10
tests 21.34 PASS. Report QA/text checks conservatively reserve 66 seconds.
**This continuation: 27.04/30 minutes. Prior 26.80 and 28.10 retained;
cumulative 81.94 minutes.** Authoring/investigation time is separate.

The source corrections are concrete and compiled. A new candidate build plus
remaining motion/Wall runs exceed the 2.96-minute balance. An explicit request
for **20 additional validation minutes is pending** under validation policy
section 7. No dependent build/launch is authorized by elapsed time. If granted,
freeze this corrected source, build once, and select only corrected motion and
Wall checks. Indoor/daylight and OFF remain credited to their exact candidates
unless affected evidence must be refreshed. No release/full matrix is selected.

All invocations/logs/exact timers and `validation-ledger.json` are under
`artifacts/summon-overhaul-final`. The prepared guarded launcher is `run-native.ps1`
there; it preserves the prior installation/settings/save envelope. All four
native transactions used one exact phase, same candidate/purpose and fresh
process. The build used `scripts/Build-Local.ps1 -DevelopmentPurpose
summon-outsider-review-corrections -DevelopmentTestFilter
'expanded-summoning.native-profile;expanded-summoning.sna-naming-scale;expanded-summoning.overhaul;expanded-summoning.sprint35;expanded-summoning.sprint34'
-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`.

Illustrated review with 17 actual native frames, retained failure details,
unchanged-model comparisons and owner reference images:
`artifacts/summon-overhaul-final/illustrated-review/Summoned-Creature-Acceptance-Review.html`.
SHA-256 `6c516bfa0b4a3bd8521af0dc65d9d8c1591bf39a6badf916f052e4abd786e241`.
Browser layout inspected. Native captures and packages stay outside Git.
Owner aesthetic approval and full mission acceptance remain pending; PR #96
stays draft. Earlier records below retain their original provenance.

## Second-review native progress and narrowed follow-up

Candidate `162d089e2636c5a5ab81e9d677c2c8a9a909f3ed` passed exact build/package
validation. ZIP SHA-256 `2eb94b4d3569aa5eb190bff739ff02cbc782e08dc17addc289a7b2594493f16f`;
DLL SHA-256 `57b7b53e24c38ee4620423b4c069c2e03d15051f45c68f497b93496ef373aaf3`;
MVID `d97a7ee3-a4ec-4474-b641-860277e1d281`. All twenty export identities
remain unchanged. New evidence under the assigned lab's runtime-evidence:

- `20261105T0505593606969Z-disposable-expanded-summoning-sprint32-native-review`:
  indoor PASS 4 assertions; ordinary-camera image inspected sharp in the dim
  original hall. Appearance JSON is metadata-only and produces no overhead render.
- `20261105T0509182581276Z-disposable-expanded-summoning-sprint32-native-review`:
  daylight PASS 11 assertions; sharp image inspected, Nalfeshnee native move
  started/acted/finished Success, ON reconstruction retained 2.10 scale.
- `20261105T0512270408795Z-disposable-expanded-summoning-sprint32-native-review`:
  motion FAIL at Lillend after Glabrezu/Vrock/Babau/Hezrou/Barbed/Bone successful
  movement. Lillend translated but its supplied command did not start; lane
  receipt says `anchorGap=0`. Unlike established presentation code, the generic
  observer had not relocated its hostile witness from the surveyed origin.
  Casting and infernal pair/contact sequence were not reached.
- `20261105T0517083304769Z-disposable-expanded-summoning-sprint32-native-review`:
  Wall FAIL at strict viability after actual formation (original 36 HP/720s),
  successful baseline/live mover with a real detour, controlled native fire
  destruction and restored Recast route. Physical attack never started; old
  graph-index equality and final grid snapshot also failed. Expiry/tail not reached.

Every transaction restored original installation/settings/save bytes and mtime
exactly. No save was loaded. The three previously closed defects remain closed;
module OFF is credited to its prior candidate and was not repeated.

Follow-up observers now compile: move the owned hostile clear of the generic
movement lane; use native floor support instead of nearest-node projections
for Wall witnesses; settle physical/adjacent/path observers; clear disposable
witnesses before bare-grid restoration checks; accept both actual supported
path backends, with native Recast-cut admission required for Recast. Keep the
strict physical hit/halving and exact walkability restoration assertions and
add restoration diagnostics. These changes have not run natively. They do not
constitute a claimed gameplay repair. A new candidate is required.

Measured seconds so far: preflight 36.35, candidate build 681.87, indoor 171.57,
daylight 163.13, motion 201.07, Wall 212.41, follow-up compile/checks 29.48.
Logs/exact timers are in `artifacts/summon-overhaul-final`. About 25 minutes of
the new ordinary 30-minute envelope are used before report QA. Another roughly
11-minute build plus remaining native observations exceeds the balance; seek
an explicit additional allocation under validation policy section 7 before
starting it. Preserve both earlier ledgers. PR remains draft, acceptance open.


## Current second-review continuation: source frozen for prerequisites

Owner requests completion of the remaining observer/native items and will
perform the final manual review. Reviewed HEAD is `b2f65d530a80ecedc94ecbf9c36f9e97e11f325a`.
The three prior defects stay closed. All model exports, scale logic and
protected designs stay fixed. Appearance collection is now metadata-only;
Lillend casting uses the settled ordinary party camera. Wall witnesses wait
for three stable native frames and require ground support before moving.
The prior Default clock-ownership correction is retained.

This new review task selects an ordinary focused validation envelope, target
15 minutes / ceiling 30; both earlier ledgers (26.80 and 28.10) are retained.
Preflight production compilation and 28 selected tests PASS in 36.35 seconds.
Plan one immutable candidate build (~11 minutes), then inspect indoor capture
and the separate Nalfeshnee daylight movement/ON-reconstruction prerequisite
before selecting remaining motion and Wall processes (~14–17 minutes total).
The exact motion phase is separately admitted to avoid running the full
sequence before its prerequisites are reviewed. Module OFF is not repeated.
Native results for this candidate are NOT RUN until recorded below. No
release/merge/full compatibility campaign is selected.


## Latest review correction outcome: mission acceptance remains open

The three demonstrated defects and ten further art revisions are implemented
in `1dda3428e72ff926684402590f0fafc6fd75ddd4`. PR #96 stays draft. Owner
aesthetic approval remains pending. Older notes and the 26.80-minute ledger
below are historical evidence, not current acceptance.

One immutable development candidate served all four fresh native processes:

- Purpose `summon-outsider-review-corrections`, version `0.0.177`.
- ZIP: `artifacts/development-candidates/summon-outsider-review-corrections/1dda3428e72f/KingmakerGunslinger-0.0.177-development-summon-outsider-review-corrections-1dda3428e72f.zip`.
- ZIP SHA-256 `bb9de770a6465286b86942471203e151340909ede3a5fcbd09f42414dc9ee24d`.
- DLL SHA-256 `87ac3cc5bf6757442931df488ea2ee0e10cf41dc02a2d989813e2691cec43596`.
- DLL MVID `17fd2bcf-ef78-499e-95cc-ed14c2dd1a00`.
- Source fingerprint `6b163067c2238c94bef9d8ab22b45100474d28eb3df07617ac4e2977a5adc3ca`.

### Actual native observations

Directories are under the assigned lab's `runtime-evidence`; timestamp suffix
is `-disposable-expanded-summoning-sprint32-native-review` in every row.

| Phase / timestamp | Result | Meaning and limits |
| --- | --- | --- |
| indoor / `20261105T0326114862116Z` | PASS, 4 assertions | Nalfeshnee custom body/albedo and root/original scale 2.10 observed; actual collider rays hit. Image blurred, so no indoor material/art acceptance. |
| Wall / `20261105T0329350156477Z` | FAIL, 9 assertions total | Default admitted; nested setup reset its wait flag and the outer poller requested a forbidden manual pulse. Stopped before casting. No formation, obstruction, destruction or expiry result. |
| daylight / `20261105T0334347226844Z` | FAIL, 9 assertions total | Nalfeshnee body/collider/reference captured. Move finished `Interrupt`, `started=false`, `acted=false`, without horizontal travel. Later bodies/motions and ON reconstruction not reached. Image blurred. |
| module OFF / `20261105T0340447645602Z` | PASS, 16 assertions | All five identities retain donor visuals and root/original scale 1.0. Same-identity Nalfeshnee native view reconstruction retains 1.0. No save serialization/cold-load claim. |

Native result SHA-256, in row order:
`b2a75a6702318d85911a646dcc29871326063c98ca830ea0d8c42547ed022be6`,
`c67de1a082f5880bb440d823edde49377fa1784b26969b7cdef214b4597826f2`,
`9d79fe4370593ebe5567b3782b89b7e10baf946f42402a7c56508cec3efcc7bd`,
`d7d50e442017d3f3b6e0df0ef666e699137d3448f78125cc4541b0c967104e22`.

Measured indoor Nalfeshnee renderer AABB: 4.270 × 5.112 × 5.234 m;
soft collider 4.924 × 5.032 × 4.957 m; core collider 3.967 × 1.480 × 3.967 m.
Native reference body: 0.871 × 1.949 × 0.708 m. Both collider rays hit.
Selection geometry **is enlarged**; preserved size enum/corpulence does not
establish unchanged selection geometry. Movement clearance/UI usability remain
unaccepted. Reconstruction used native `AttachToViewOnLoad(null)`, not a disk load.

All four transactions exited and restored installed v0.0.140, native settings
and protected save bytes/lengths/mtime exactly. Original live-tree fingerprint
`23d713cd130b43a1fbbd970006d2eda73083a6fa3a9867a14e0b78cb846b6b59`.
Restoration receipts: `artifacts/summon-outsider-overhaul/review-corrections/native-review`.
No save was loaded or mutated. Raw evidence/private inputs remain outside Git.

### Follow-up observer corrections: compiled, native unrun

`45c90e2ac519b6c56f9d35d79457863be6edf393` changes observers only. Polling
respects a Default lease independently of nested waiting flags. Movement uses
the existing infernal observer's three stable native frames and ground-support
check. Review images use the native party camera after twelve presentation
frames instead of immediate artificial overhead framing; brightness,
depth-of-field and materials are not forced. Production compilation and 10
focused neighbors pass. These changes are **not in the tested ZIP and have
not run natively**. They are proposed observer corrections, not proven
material/movement repairs. No Wall mechanic was changed without a demonstrated
mechanical failure path.

### Additional validation ledger

| Invocation | Seconds | Result |
| --- | ---: | --- |
| Pre-candidate focused compile/78 tests, atlas/link/admission/orchestration | 36.00 reserved | PASS; measured gate 26.52s |
| One immutable development build | 669.08 | PASS; 78/78, exact references and strict package pins |
| Indoor process including restoration | 189.11 | structural PASS; image insufficient |
| Wall process including restoration | 158.04 | FAIL before cast |
| Daylight process including restoration | 320.70 | FAIL at first move |
| Module-OFF process including restoration | 220.71 | PASS |
| Observer correction compile/10 tests | 26.32 | PASS; native unrun |
| Report audit/layout and final text checks | 66.00 reserved | conservative reserve |

**Additional total 28.10/30 minutes; previous 26.80 retained; combined 54.90.**
Model authoring time is separate. Remaining 1.90 minutes cannot cover another
approximately 11-minute build plus missing native checks. Current validation
policy §7 prevents selecting that over-ceiling continuation.

Build command: `scripts/Build-Local.ps1 -DevelopmentPurpose
summon-outsider-review-corrections -DevelopmentTestFilter
'expanded-summoning.sprint3;expanded-summoning.overhaul;expanded-summoning.native-profile;expanded-summoning.sna-naming-scale;expanded-summoning.sprint16-view-scale;expanded-summoning.sprint17-snake-production-body-identity;mission.combat-log-publication'
-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`.
Each native invocation used `artifacts/summon-overhaul-baseline/run-review-corrections-native.ps1`
with the exact ZIP/version and one phase, ordered indoor, Wall, daylight, OFF.
Final gate: `scripts/Invoke-KmgGate.ps1 -Filter
'expanded-summoning.native-profile;expanded-summoning.sna-naming-scale'`.
Logs, exact timers and `continuation-validation-ledger.json` remain under
`artifacts/summon-overhaul-review`.

Illustrated document:
`artifacts/summon-overhaul-review/illustrated-review/Summoned-Creature-Visual-Review.html`.
Document SHA-256: `add3eb6c01d6504a84893c73c430b1daefb91582ac10f81cf309b9722a941ded`.
It compares reviewed PR exports at `3611f921ddeb` with corrected exports,
includes all twelve owner references, and embeds actual native evidence with
failure/blur limits. Technical validation, native observation and owner
aesthetic approval remain separate. Earlier artifacts/receipts are preserved.

Required continuation needs a new allowance: freeze the observer corrections
into a new candidate; first establish sharp ordinary-camera indoor/daylight
images and grounded movement, then finish remaining bodies/motions,
two-Kyton ownership and ON reconstruction. Run the corrected Wall observer
through legal formation, actual obstruction, destruction and original-deadline
controlled expiry before choosing any mechanical repair. Keep the 10-foot
plane/36 HP/720 seconds and installation/save safeguards. Full compatibility,
release and merge remain deferred.

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
