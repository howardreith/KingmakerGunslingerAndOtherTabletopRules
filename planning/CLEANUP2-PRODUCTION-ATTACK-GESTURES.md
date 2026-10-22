# Original Crab/Brachiosaurus attack presentation checkpoint

## Scope and status

Original private render-palette gestures are authored for the registered summoned
Giant Crab and Brachiosaurus. Native attack animation lookup still returns null
for Claw/Tail. No Bite/Slam/Gore alias, animation handle, supplied act, attack
rule, roll, weapon, action cost, range, source skeleton, unit root or collider
is written by these helpers. Owner art approval and actual native qualification
remain pending. This supersedes no retained failure or earlier timing evidence.

The earlier Crab diagnostic alias acted at native command t=.6670539 rather
than the missing-handle ScheduleAct(1) fallback. It remains a separate request
experiment and is not used here. The Brachio v1/v2/v3/v4 synthetic probes remain
preserved under artifacts/cleanup2-tail-prototype; they are not native evidence.

## Implementation and ownership

- `SummonAttackGesturePolicy.cs`: exact registered unit/prefab/source/weapon
  identities, independent positive ownership gates and original bounded curves.
  Crab covers its 25 published sources; Brachio covers its 11 canonical/adapter
  summon abilities, excluding Trample/Overrun. Curves wind up over the observed
  native one-second schedule, reach the authored strike only at the real native
  OnAction boundary, then recover over .5 seconds of native GameTime. Crab uses
  primary claw/left named pedipalp palette and additional primary claw/right
  palette; it does not claim independent finger closure or a mechanical hand.
- `SummonAttackGestureVisual.cs`: Transform-only mirror paths under one owned
  carrier, no component/Animator/collider cloning. Only selected project skin
  palette slots and that view's private project mesh bindposes change. Native
  rootBone and source transforms remain literal original references.
- `ExpandedSummoningPteranodonViewPatch.cs`: one attachment field; extend the
  existing exact controller/driven-material receipt from Crab to Brachio; attach
  the dormant original gesture after material-controller adoption; release the
  gesture first in existing Revert. Shared material-controller/default handling
  is unchanged. Failure refuses gesture while retaining the attached body.
- Native ScheduleAct postfix only observes the actual stored delay/deadline.
  Native UnitAttack.OnAction prefix only observes the existing act. Neither
  changes arguments, return values or command state. Ordinary native nonnull
  animation handles, module OFF, charge/cutscene/overridden commands, foreign
  unit/source/caster/view/palette/material/controller refreshes refuse admission.
  A captured exact native donor action-list/manager reference also gates motion.
- Visibility/fog/dissolve use the same existing renderer and native materials.
  Temporary component disable cancels art, retains a neutral private palette and
  allows later enable; real teardown conditionally restores exact owned slots,
  private bind entries and culling bounds and destroys its owned transforms.
  Foreign mesh/material replacements are not adopted or destroyed. Teardown
  unlinks only exact owned pivot references even if the mesh was replaced.

For a selected actual source bind inverse B, authored renderer-space pivot p,
let d=B*p. Mirror the current source TRS. Insert Pivot at d and Recenter at -d;
parent cloned source descendants beneath Recenter. Bind the selected palette
slot to Pivot with private bind `T(-d)*B`. At zero gesture, for every arbitrary
current source matrix M, `M*T(d)*T(-d)*B == M*B`; descendant paths likewise
cancel. This avoids substituting old captured bind scalars for current native
animation. During art only Pivot rotation changes. Tail0/pelvis stay literal
source slots; Tail1..4 use authored centerline pivots. Crab offsets are zero and
only its two pedipalp chains receive the original poses.

Activation requires synchronous actual current-pose BakeMesh equality at zero
gesture (maximum renderer-local error <=.0002). A before/after source/root and
native collider geometry/matrix probe is available to the native observer.
Temporary meshes and vertex arrays stay in memory and are disposed; only
aggregate errors, counts, identities and transform points are exported.
Unknown collider geometry refuses the proof. Capsule/sphere/box/mesh and actual
installed CharacterController geometry surfaces are measured. Expanded renderer
bounds are culling-only; camera framing uses a separate actual current own-skin
aggregate. Neither is anatomy, collision or range evidence.

## Brachiosaurus original asset repair

New reproducible generator step:
`assets-source/original-models/sprint25-giant-bodies/repair_brachiosaurus_tail_weights.py`.
The existing original body generator calls it after Brachio export only.
It reads project-authored geometry only, asserts the known 605 tail vertices,
70 pelvis junction triangles/40 tail boundary vertices, pins those boundary
vertices to Tail0 and smoothly redistributes Tail1..4 weights along the authored
long tail. Exactly 594 tail weight rows changed. Positions, normals, UVs,
triangles and every non-tail weight row were asserted byte-identical. Geometry
blob prefix SHA256: `aedefa3ca984a16c911974e8bb2800de21ce31bdf148189cb60a170586bfa7e1`.
Runtime verifies the v4 junction is Tail0-pinned before installing this rig.

- Previous mesh JSON SHA256: `9bd237aad9b1a99f25162e4a109814b5f72fb9da6167d2f000e88f333e24a974`.
- Final deterministic UTF-8/LF mesh JSON SHA256: `cf919c077be4a9cc11a128e0e67f78fc7e1566514349671846660e5c0ca40db5`.
- Albedo unchanged: `198b61966762624adc3ffdfb5975fe08bfc7ad4589fc6e0f318e7bbb06d30776`.

The weight refinement may change native animated tail deformation relative to
the older weights; zero-offset proof compares the **new actual project skin**
under actual native current TRS against the same skin with owned rig neutral.
It does not claim old/new weighted animation equality. The source native donor
mesh/bindposes/weights remain untouched.

The first Windows write had 117 CRLF line endings and provisional raw hash
`03c6935761c8d4afae9cd2027dff78505d7eb6b18e5d1d4f4cd8078a1d732db9`.
The writer now explicitly uses UTF-8 with LF, matching repository checkout
normalization. Regeneration changed zero weight rows and preserved the decoded
geometry prefix hash above; the scratch rerun proved final LF byte idempotence.
Use only the final LF hash for the current protected package pin.

## Native observer integration (root-owned dispatch/build)

New partial `RuntimeTestRunner.SummonCleanup2ProductionAttacks.cs` exposes
`ReviewSummonCleanup2ProductionAttacks(fixture, Spellbook book)`.
Exact future phase `production-attacks`, save-free mansion RTWP fixture,
automatic exit, existing qualified .05 native clock, 240-second phase ceiling,
35-second/3000-frame bound per command. No Working/human-save access.

The observer casts actual canonical SNA III Crab and SNA VII Brachiosaurus
through parent/prepared slot/native RuleCast/spawn capture, settles appearance,
and uses the existing exact disposable hostile. It reuses the measured native
range/LOS/graph/party-safe lane helper (root admits this exact phase alongside
crab-attack). Single and full ordinary commands run with exact actor-only render
suppression OFF baseline, then gesture ON. No global module toggle or mechanical
input is changed. The fixture's Wizard grant is explicitly not class access.

It records the actual schedule/index/AttackHandInfo/weapon instance and real
RuleAttackWithWeapon callback association, native null handles, one native cost,
natural rolls, actual act deadline, corpse avoidance, exact weapon/stat/size/
corpulence/agent-clearance/summon-end-time invariance and protected native prefab
assets. Native OnAction invokes Rulebook before incrementing its attack index,
so the after-rule callback must still match the exact planned slot. Duplicate
or mismatched chains fail. Baseline/ON stored delays must both be exactly 1s;
actual act latency comparison tolerates one .05s fixture quantum, never early
act. This is not natural Default-mode or turn-based timing qualification.

Current-pose zero-offset proof, finite positive actual skin displacement,
unaffected weight-row displacement <=.0002 and same-frame source/root/collider
invariance are sampled during the actual command. ON captures three real native
camera poses per actual attack (windup/act/recovery) with unique names. No Hit
action is invented. Enemy contact, whole-tail reach and owner art approval stay
separate. Source resources and all exact private transforms/palettes are checked
through live release; shared positive fixture teardown owns actor disposal.

Required production Compile entries: the two new Summoning files and new native
observer partial. Domain links the new pure policy and new
`SummonAttackGesturePolicyTests.cs`; named cases are
`ExactSourcesAndEveryOwnershipGate`, `TailSourceCensusMatchesPublishedSummonAdapters`,
`OriginalCurvesAreFiniteBoundedAndReturnToZero`. Existing Crab pure identity
policy, SharedControls read-only receipt/stats helpers, ScalePrototype mechanics
scalar helper, current Review route/cast/appearance/shape helpers and qualified
Sprint24 observer/Sprint26 command trace/FirearmReviewCosts are transitive
dependencies. No shared dispatcher, catalog, project, test runner, runtime
script, package pin or existing Crab diagnostic was edited by this agent.

## Checks performed here / limits

- Existing AGENTS and current validation policy read. Installed 2.1.7b IL audited
  from Assembly-CSharp SHA `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`,
  MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`: ScheduleAct `0600279F`
  stores deadline from TimeSinceStart; TryStartNextAttack `06002682` retains
  missing-handle literal delay1; OnAction `06002681` triggers the actual rule
  before incrementing index; native Tick `060027A7` alone supplies act. Public
  property contracts and actual Unity CharacterController surface were checked
  by reflection against installed references. No proprietary arrays exported.
  Start `060027A5` calls `set_IsStarted(true)` at IL0091, then virtual OnStart
  at IL0097. UnitAttack.OnStart `0600267E` rebuilds attacks at IL0007, then
  TryStartNextAttack(true) at IL000E. Thus the ScheduleAct postfix sees started
  true and the final native attack-info list, rather than prestart handles.
  PlannedAttack `06002670` returns m_AllAttacks[m_AttackIndex]; GetAttackIndex
  `06002675` returns that same field. TryStartNextAttack's null-handle branch
  calls ClearAnimation at IL0081 and ScheduleAct(1) at IL008C. No initial
  unstarted Init/InitAttacks pass is admitted by this production observer.
- Weights-only repair command used Blender 4.5.10's embedded Python, completed
  successfully (~1s tool invocation). A scratch-only rerun proved byte-identical
  idempotence without touching the tracked asset a second time.
- `artifacts/cleanup2-tail-prototype/verify-pivot-cancellation.py` performed a
  SYNTHETIC algebra check of 1,000 four-bone arbitrary current affine hierarchies
  with nonuniform source scales: maximum matrix cancellation error
  `2.2737367544323206e-13`; ~1s invocation. This is not actual native pose proof.
- Source inspection found no command/rule/animation-act writes. No build,
  C# test execution, native run, game launch, Git mutation or commit performed
  by this agent. Root owns focused compile/tests and guarded native qualification.
- Candidate remains reviewable development work. All live outcome and art
  acceptance claims require the new observer's actual evidence and pixel review.
