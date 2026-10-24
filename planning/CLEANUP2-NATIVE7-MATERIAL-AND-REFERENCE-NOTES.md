# Cleanup 2: native7 material and native8 reference corrections

Source checkpoint, 2026-10-22. Owner visual approval, current-byte native
material/fade behavior, giant measurement/calibration and shared-donor native
controls remain **pending**. No game, build, package, installation, save or Git
operation was performed by this lane. Historical evidence retains its original
failure/status and identities.

## Ettin: current native evidence and bounded paint change

Inspected actual native7 actor `b1cfebd5-befe-46a4-aade-a66c26c6935a`
`idle-12-heads--40.png`, `idle-12-body-75.png`, and
`main-hand-24-heads--105.png` under
`20261022T0226040010370Z-disposable-expanded-summoning-cleanup2-native-review`.
Source `a04e204bc01ae57bbdf121e5556943cb7b4b2cf7`, DLL SHA-256
`c574da54b7ef161f75d649096415da588bc58ae71baa406542775839f7c05c83`, MVID
`5e5d2cdb-ffc6-420b-91dd-1f1aff98f645`.

The negative-Z facing repair is visible and the shorter neck meets the collar.
The face remains a pale cream/yellow simplified mask beside the native warm-tan
head. Native metadata still shows the intended PF/Gamma shader, white Tint,
original 512-square ARGB32 albedo and 2-square RGBA32 parameter texture, null
BumpMap and exact UV scale/offset. There is no lost-texture/controller-reset
diagnosis. Native7 still has 72 PASS/2 FAIL for the absent native off-hand pose
handles; its two head-follow rows cover only about 4.2/4.4 degrees of motion,
with zero rotation-follow error and about 3.0/3.3e-6 neck-local offset drift.
These limited observations and old-byte resource release are not new-byte
motion, cold hydration or owner acceptance.

Only the original **skin paint quadrant** changes: base RGB
`(.465,.405,.315)` becomes `(.36,.26,.18)`, with stronger authored forehead/
socket, nasolabial, cheek and beard variation in this head's own UV frame.
This is a concrete native-calibration starting point, not a promised color
match. All 65,536 skin pixels change; all three other regions (shadow/hair/eye)
are byte-identical. The entire packed geometry/normal/weight/UV/index payload
is byte-identical to native7: 5,024 vertices, 9,992 triangles, two drivers,
four material slots, negative-Z facing and `(.44,-.035,-.02)` offset retained.
No other creature geometry/material, GUID or mechanics change.

## Ettin: exact native controller and fade baseline

Installed 2.1.7b `Assembly-CSharp.dll` SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`, MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, establishes:

- `StandardMaterialController.ReinitMaterials` token `06001180` calls native
  color/rim/dissolve/petrification default restoration, then `Awake`, which
  recensuses child renderers and caches each driven material's baseline.
- `DissolveAnimationController.Update` token `060011A3` drives the controller's
  current Materials list; `RevertToDefaults` token `060011A4` restores the
  per-material cached keyword, texture/UV, dissolve, width, color/HDR, cutout
  and emission values. It does not infer a baseline from the current live fade.
- The material list/cache and nested MaterialSettings type are internal. Their
  exact fields are read via the existing material-list helper and strict
  reflection of `m_MaterialSettings`/named field types; inaccessible native
  APIs are not referenced as public C# types.

`EttinSecondHeadVisual` now requires one exact source controller containing the
audited Hill material and its cached defaults. Each head-owned material uses
that cached native fade state, including render tag/queue, before the same
controller is reinitialized to discover the new renderer. It then requires
all four assigned materials to belong to that controller and cache the full
same source fade baseline. No hardcoded `_Dissolve=0`, new animation, forced
visibility or fade-time override is added. Native dissolve FX texture references
are borrowed only at runtime, never copied/exported/destroyed. Surface albedo/
parameter maps remain original; the verified roughness/emission/metal and flat
normal contract stays unchanged.

Release first removes the owned root, refreshes the same per-view controller
to exclude its renderer, then releases the exact owned mesh/materials/textures.
Controller-refresh failure is retained in an explicit receipt while private
resource cleanup still completes. `ObserveMaterialControl(view)` exposes
controller/material IDs, membership, live/cached dissolve, full cached-source
baseline agreement and cleanup outcome. Root must add this observation to the
selected native initial/late/release packets before claiming fade/lifecycle
success. Existing original resource capture includes both textures and proven
renderer-created clones. The Pteranodon shared helpers were not edited;
arthropod ownership was consulted and confirmed no overlapping helper changes.

## Shared donor: loaded asset is not an initialized view

Native8 shared controls failed before spawning either actor at the cached view
manager guard. The installed `UnitEntityView.AnimationManager` getter (token
`0600182A`) reads `m_AnimatorManager` or the CharacterAvatar manager cache.
The base `AnimationManager.AnimationSet` getter (token `06001518`) silently
returns HumanAnimationSet when serialized `m_AnimationSet` is null. Neither
is proof of an exact uninitialized Mastodon prefab's serialized action set.
`AnimationSet.Actions` itself is a direct backing-field return.

`RuntimeTestRunner.SummonCleanup2SharedControls.cs` now records a bounded child
UnitAnimationManager census on the exact blueprint/prefab, with component paths,
identities, cached view-manager identity, exact serialized set and action count.
Exactly one component with a nonnull serialized set and 1..128 actions is
required; at most eight candidate diagnostics are emitted. Prefab receipts use
that raw serialized set directly. No fallback, Awake, Init, playable setup or
prefab write occurs. Live owned actor receipts additionally require the
initialized getter to resolve the same exact component. Mesh/material/bone/
action reference and source-prefab protection checks remain. A fresh native
census/control run is required; ambiguous/missing components still refuse.

## Giant Human CPU source: preserve the native upload lifetime

Native8 mansion run `20261022T0306080437716Z-3c95a294375a480da66fefb55c26358f`
in `20261022T0306080247725Z-disposable-expanded-summoning-cleanup2-native-review`
qualified its ground plane (226 candidates, selected 21 supports, grade about
2.87e-12, residual 3.58e-7), but failed 38/2 at the human art anchor. Exact
renderer `Renderer_Character_Diffuse_Cutout`, mesh `Character` had 0 source
positions, 1,759 baked/weight records and 1,819 bones/binds with no null entries.
No calibration was applied. The failed reference is retained.

Installed `Character.BuildMesh` token `0600142A` creates the current `Character`
mesh from the filtered per-material BodyPart dictionary, CombineMeshes with
identity transforms, remapped weights/binds and atlas UVs. At `IL_02E1/02E2`
it executes literal `true`/`Mesh.UploadMeshData(Boolean)`, discarding CPU
vertices. Installed Unity CoreModule SHA-256
`3a76df7f709d465e3273502e08edbffb536b1c2f78c3a132b8668e59fddd2803`
checks `canAccess` in the vertex getter; the inaccessible branch logs and
returns an empty array. Its Mesh API has no modern AcquireReadOnlyMeshData or
GetVertexBuffer escape hatch. The UMA-named hierarchy does not establish a
retained UMA CPU mesh: this is Kingmaker's CharacterSystem implementation.

NEW `Cleanup2CharacterCpuReference.cs` supplies an **inactive until explicitly
leased** observation, not a production calibration:

1. `Begin(request, exactRequestOwnedHumanBlueprint)` requires the exact Cleanup2
   scenario, automatic exit, one exact giant-lineup/scale-prototype parameter,
   run ID, a distinct native DefaultPlayerCharacter clone/Human race, installed
   module identity and an exclusive lease. Arm it **before** SpawnUnit builds
   the request Human; it never rebuilds an existing actor to repair missing data.
2. A request-installed BuildMesh-only transpiler requires exactly one literal-
   true native upload. It observes the actual stack Mesh and Character just
   before upload, then calls the original Mesh.UploadMeshData with its unchanged
   argument exactly once. Foreign characters continue through the native call.
3. Only a Character whose live view/data references the exact permitted cloned
   blueprint is captured. Ordered CPU positions/weights/binds remain transient
   in memory. A postfix ties the same mesh to one actual assigned renderer and
   its live palette. Bounds: 32 captures, 16 renderers, 100,000 positions,
   8,192 binds; invalid/nonfinite/ambiguous data records failure and is not used.
4. `TryGetExactSource(view, renderer, out positions, out receipt)` requires the
   same owner/view/unit/renderer/mesh, exact bone references and bit-identical
   weight/bind records. It returns only the current matched positions in memory;
   JSON contains scalar counts, instance identities, ordered-record digest and
   failures. The ordinary weighted-bone/BakeMesh convention check must still run.
5. Dispose removes only its own Harmony lease and clears transient arrays.
   An early build before ownership is observable is a missing-reference failure,
   not authority to adopt a later arbitrary mesh or force a rebuild.

Root integration: arm after the fixture Blueprint clone is prepared and before
its SpawnUnit, dispose in the outer finally. In shared measurement, use the
helper only when `source.Length==0`, attach its scalar receipt, retain the strict
rejection on false, and preserve every subsequent influence/bind/normalization/
convention/Cloth/contact/FX gate. Root owns these shared-file/compile-list changes.
No renderer exclusion, culling-bound anatomy, old skeleton, assumed convention,
production scale activation or cold reconstruction is authorized by this helper.
Missing transactional/current-16-mod cold prerequisites remain a refusal.

## Focused checks and exact current art identities

Blender 4.5.10 LTS `6dc0b208d1b5`, same commands as the preceding forward notes,
with absolute work directories `artifacts/cleanup2-ettin/native7-warm-final`
and `native7-warm-repeat`: final generation exit 0 **10.6112 s**, independent
repeat exit 0 **5.9840 s**, identical generated C# bytes. Exact packed audit
exit 0 **0.6386 s**, same complete topology/normal/UV/weight checks: closed skin,
zero degenerates, 9,992/9,992 outward averaged normals. Paint-region byte audit
exit 0 **0.2472 s** confirms geometry and all three non-skin regions exact.
Shared-donor posed-frame front/side/rear plus synthetic turn/pitch previews exit
0 **3.8811 s**. Final front/side/rear were opened: warmer original skin and
authored facial variation are visible. Workbench remains darker/glossier than
PF and is not native lighting or owner acceptance.

| Art identity | SHA-256 |
| --- | --- |
| Geometry (unchanged) | `747aec079b36e424b13af5e4d4cf002a9810cda13da5a2e81b5d3e2175183eb8` |
| Original albedo | `64de9ab9d66678e16cf8b296d75a1ef4b75d8f3b3bb585ce7136535b43f0ae5c` |
| Generated C# | `6afd07161a9af5d8eea474ca6439b0d690235df640c7dc05bcec0c69467fdeaa` |
| Generator | `f7fcce849536143dc88001d21d224523d307a8735418262c8b1c31b5f15f400d` |

NEW `scripts/Test-SummonCleanup2CharacterCpuReference.ps1 -ModAssembly <compiled
DLL>` executes actual production request guards, the rewrite's accepted and
missing/ambiguous/changed-upload cases and branch-label preservation, exact
float/weight/bind comparisons, installed upload/serialized-field contracts and
actual cached native fade scalar reading/type refusal. It never installs a
Harmony hook or invokes Unity object construction/mesh upload. PowerShell
parse passed; **execution is deferred to root's production compilation**.
This lane performed no build and makes no no-game behavior PASS claim yet.
Live ownership/capture/adoption and all native gates require the qualified
new artifact. Raw native data, proprietary bytes, private QA helpers, textures,
`.blend`, JSON and images remain local ignored artifacts.

## Subsequent fixture/measurement and Ettin observer integration

Root directed integration after the preceding safety/roster checkpoint compiled.
The new integration itself has **not been built or run by this lane**.

- `ExpandedSummoningCorrectionFixture` owns the CPU lease. Only the exact
  permitted giant request assigns the exact registered Human BlueprintRace to
  its own DefaultPlayerCharacter clone (the native pre-chargen default has no
  race), then arms the lease before SpawnUnit. Native root data remains intact;
  later descriptor/race-fact verification is unchanged. Normal teardown disposes
  the lease before caster destruction, retaining its scalar receipt. A partial
  constructor failure cleans only the fixture's positive references and releases
  the lease locally because the caller never received that fixture. Both
  construction and cleanup errors are retained if both fail.
- Shared measurement records `sharedMeshCpuReadable` and avoids intentionally
  calling the installed invalid-read getter on an unreadable mesh. Only an empty
  native CPU source under the exact request may use `TryGetExactSource`.
  Raw empty count, selected source mode and full scalar capture receipt remain
  visible. Missing/ambiguous/failed/foreign capture still reaches the original
  strict rejection; all influence, finite, normalization and weighted-bone/
  BakeMesh convention checks remain unchanged. No dispatcher change was made.
- Ettin's normal LateUpdate packet checks all four assigned owned materials,
  both exact original texture slots, the source controller's membership/full
  cached baseline and same-frame source/head dissolve (maximum error .0001).
  Native expiry sampling uses the existing late observer without setting a fade
  value or animation. Positive proof requires at least two samples, an active
  native dissolve animation and actual source-value change greater than .0001;
  no observed fade is failure/unknown. Captured release receipts must report
  `released`, zero assigned head materials and native controller refresh, in
  addition to exact private resource destruction and sibling isolation.
- A retained managed CWT view key can report its final controller-cleanup
  receipt after Unity destruction; no destroyed native view is reinitialized.
- Existing `giant-humanoids-ii` persistence now adds one exact Ettin prepare/
  cold-verify attachment/material assertion. It reads the actual view and never
  invokes attach/rebuild: one authored root/skull, native Head/Neck relationship,
  two drivers, four owned PF materials, both exact original textures, private
  mesh identity and exact controller/native-fade agreement are required ON;
  zero project head roots/resources are required OFF. Runtime observations do
  not alter the saved profile or stable receipt schema. The cold-verify label
  becomes evidence only when the guarded fresh-load scenario actually runs.

The independent route audit before any new cold wrapper found the old exact-
eleven-mod installed-stack guard and explicit Cleanup2 refusal, while the
current creature-review wrapper's protected catalog locks **all** saves with
FileShare.Read, including Working. It cannot serve as a save-writing prepare
transaction. The existing planning functions explicitly set live/runtime use
false. A separate qualified retained-owner current-16-mod transaction, exact
original Working bytes and path/length/all timestamps/attributes restoration,
protection for every other save, fresh processes and durable recovery remain
required. Root/menu lane owns that separate work; this observer does not remove
an old refusal or grant a live launch. No current cold-hydration claim is made.
