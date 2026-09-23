# Pteranodon native-contract audit (charter Sprint 2)

Status: **exact installed type metadata and shipped-catalog facts captured;
live donor prefab inspection pending.** This document records reflection-only
member metadata and curated facts. No proprietary assembly, decompiled source,
extracted mesh, texture, rig, or animation is committed.

Sprint 2 replaces the Pteranodon's borrowed body with an original pterosaur
visual and proves the minimum reusable skinned-creature pipeline. Its accepted
mechanics, identity and placements must not change.

## 1. What Pteranodon actually is today

From the shipped catalogs at release `0.0.136`:

| Fact | Value | Source |
|---|---|---|
| Creature key | `pteranodon` | `ExpandedSummoningCatalog` |
| Spell placement | Summon Monster IV / Summon Nature's Ally IV | `ExpandedSummoningCatalog` |
| Alignment template | Celestial/fiendish, neutral caster chooses | `templated: true` |
| View policy label | `Roc` | `ExpandedSummoningCatalog` |
| Donor blueprint | `406c1e1af5400ac4881e330502ccbd9e` | `ExpandedSummoningDonorCatalog` |
| Donor role flag | `0` - visual/body donor, not a dedicated summon | `ExpandedSummoningDonorCatalog` |
| View scale multiplier | `0.82` | `SummonViewScaleCatalog` |
| Stat profile | Animal, CR 5, Large, STR 16 DEX 19 CON 15 INT 2 WIS 15 CHA 12, natural AC 2, Bite 2d6, fly 50 | `ExpandedSummoningNaturalProfiles` |
| Facts | Airborne, Dodge, Improved Initiative, Skill Focus (Perception) | `ExpandedSummoningNaturalProfiles` |

The charter and the shipped roster call this a "Roc" proxy, but that is the
*view policy label*, not the donor. The donor blueprint is
`CR3_GiantEagleStandard`.

### 1.1 What the guide says this creature is for

The summoning guide (reference R1) rates Pteranodon the air pick at its tier:
it values the ten-foot reach for attacks of opportunity, and the fact that
"everything stacks on that main attack" - a single bite of 2d6 plus modifiers -
and elsewhere calls multiple Pteranodons the best single-target aerial
attacker available at that level.

This corroborates the shipped profile rather than changing it: Large size
carries the ten-foot reach, and `Bite2d6` is the stacked main attack. The
consequence for this sprint is an acceptance emphasis, not a stat change. The
replacement visual must land its bite impact at the correct time and at the
correct point at Large reach, because that single attack is the creature's
entire tactical identity. A pterosaur that reads well standing still but
whose bite connects late, or visually short of its reach, would fail the
charter's signature-role requirement even with every number intact.

### 1.2 The donor is shared by four creatures

This is the governing constraint on the whole sprint:

| Creature | Donor | Flag |
|---|---|---|
| `eagle` | `406c1e1af5400ac4881e330502ccbd9e` | 0 |
| `dire-bat` | `406c1e1af5400ac4881e330502ccbd9e` | 0 |
| `pteranodon` | `406c1e1af5400ac4881e330502ccbd9e` | 0 |
| `roc` | `406c1e1af5400ac4881e330502ccbd9e` | 0 |

Any change to the shared prefab, its skeleton, materials, mesh, or animator
would silently alter Eagle, Dire Bat and Roc as well. Every visual change must
therefore be **instance-local and reversible**, and Eagle / Dire Bat / Roc are
the natural negative controls the sprint's acceptance scenarios require.

## 2. Exact installed view contract

Reflection-only inspection of the installed 2.1.7b `Assembly-CSharp.dll`
(resolved against the installed `Kingmaker_Data/Managed`, 9,293 types
recovered) establishes the seams below.

### 2.1 `Kingmaker.View.UnitEntityView` (base `EntityViewBase`)

Relevant readable surface:

- `Animator Animator` - the ordinary Unity animator driving the creature.
- `Character CharacterAvatar` - the humanoid doll system. Creature units that
  are not equipment-bearing humanoids do not use it; this must be confirmed as
  null for the donor before any binding strategy is chosen.
- `UnitAnimationManager AnimationManager`, private `m_AnimatorManager`.
- Scale surface: private `m_Scale`, `m_OriginalScale`, public `OriginalScale`,
  `DoNotAdjustScale`, `DisableSizeScaling`, static `ReduceSizeScaleStep`.
- Collision/selection surface: private `m_SoftCollider` (`CapsuleCollider`),
  `m_CoreCollider` (`MeshCollider`), `m_Highlighter` (`UnitMultiHighlight`),
  `m_CenterTorso`, `m_Corpulence`, `m_RenderersAndCollidersAreUpdated`.
- `ParticlesSnapMap ParticlesSnapMap`, `UnitHitFxManager HitFxManager` -
  impact and hit-effect anchoring.
- `CameraOrientedBoundsSize` / `CameraOrientedCoreBoundsSize` - the bounds the
  camera and selection use.

### 2.2 `Kingmaker.Visual.CharacterSystem.Skeleton` (base `ScriptableObject`)

- Fields `Bones`, `CharacterFxBonesMap`, `RaceBoneHierarchyObject`,
  `CloakScaleFudge`, `RagdollSkeletonOverride`, private `m_BonesByName`,
  `m_IsDirty`; property `BonesByName`; methods `CopyHierarchy`,
  `SetSkeletonDirty`, `IsDirty`, `ResetDirty`.
- Nested `Skeleton+Bone`: fields `Name`, `Scale`, `Offset`, `ApplyOffset`;
  properties `NameHash`, `Transform`, `Rigidbody`; method `NeedsReapply`.

`RaceBoneHierarchyObject` and `CloakScaleFudge` mark this as the **humanoid
Character-doll** skeleton, not a general creature rig. It is recorded here so a
later sprint does not mistake it for the creature binding seam. Bone-name
matching against this type is explicitly **not** evidence that a creature rig
is compatible.

## 3. Established instance-local precedent

`ExpandedSummoningViewScalePatch` is the shipped pattern this sprint extends:

- Harmony postfix on `UnitEntityView.OnDataAttached`.
- Keyed on `__instance.EntityData.Blueprint.name`, so only the intended summon
  identity is affected and the shared donor prefab is never touched.
- `ConditionalWeakTable` idempotence marker, so a replayed attach cannot apply
  the change twice and nothing is retained after the instance dies.
- Validates the pre-change transform and throws rather than writing a
  non-finite or non-positive value.

The custom-visual attachment must follow exactly this shape: instance-scoped,
idempotent, validated before mutation, and reversible on failure.

## 4. Confirmed asset toolchain

| Component | Version | Location |
|---|---|---|
| Game runtime | Unity `2018.4.10.10503941` | installed Kingmaker |
| Bundle editor | Unity `2018.4.10f1` | `C:/Program Files/Unity/Editor/Unity.exe` |
| Bundle project | - | `C:/Dev/KingmakerGunslingerLab/unity-asset-build/KingmakerGunslinger-2018.4.10f1` |
| Mesh authoring | Blender `4.5.10 LTS` | on PATH |

Unity `6000.5.6f1` is also installed via Hub. It is **not** bundle-compatible
with a 2018.4 runtime and must not be used.

Three shipped bundles already prove the deterministic export path
(`kingmakergunslinger.easternweapons`, `.elvenbranchedspear`, `.firearms`),
each with editable Blender source, a build report, and a reproduced bundle
hash. All three are **static props**. A skinned creature mesh bound to an
existing skeleton is new ground for this repository, and that is the genuine
technical risk this sprint exists to retire.

## 5. Live donor observation — answered

Scenario `observe-summon-pteranodon-view-contracts`, run through the guarded
harness against the installed game at 0.0.136. **Status PASS, 8/8 assertions.**
Evidence:
`runtime-evidence/20260923T1258015475577Z-observe-summon-pteranodon-view-contracts`.

The donor prefab was instantiated inactive and 10,000 units below the play
area, measured, and destroyed in a `finally`. The shipped Pteranodon identity
was re-checked as SM 4 / SNA 4 and alignment-templated afterwards. No
blueprint, unit, inventory, campaign or save state was touched.

### 5.1 It is bindable

| Fact | Observed |
|---|---|
| Donor blueprint / prefab | `CR3_GiantEagleStandard` / `GiantEagle` |
| `CharacterAvatar` | **null** - a plain skinned prefab, not a humanoid Character doll |
| Skinned renderers | **1** (`eagle_boss1`, mesh `eagle_boss1`) |
| Root bone | **`LowerTorso`** |
| Bones / bind poses | **72 / 72** - consistent |
| Materials / shader | 1 / `PF/StandardDynamic` |
| Plain `MeshRenderer`s | 0 |
| Child transforms | 194 |
| Renderer bounds | 10.205 x 4.532 x 3.585 |
| Corpulence | 1.6 |
| Soft / core collider | `CapsuleCollider` / `MeshCollider` |

One renderer, one material, a named root bone and matching bind poses is the
straightforward case: an original mesh can be skinned to this rig and swapped
on the instance without rebuilding anything.

### 5.2 Where animation comes from

`UnitEntityView.Animator` is **null** on a detached prefab, and
`m_AnimatorManager` is null too - both bind when the view attaches to a unit.
The Animator itself lives on a child object, **`GiantEagleBoss_Body_RIG_02`**.

An earlier revision of this scenario asserted that an Animator must be present
and failed. The assertion was wrong, not the game. Sprint 2 must therefore
bind its mesh to the bones under that child rig and let the native animator
keep driving them, rather than expecting an Animator on the view root.

### 5.3 The rig — 72 bones

Spine and head: `LowerTorso`, `UpperTorso`, `Neck`, `Head`, `Jaw` (+`_end`).

Tail: `Tail`, `Tail_L`, `Tail_R` (each +`_end`).

Legs, mirrored L/R: `Leg0_Upper` → `Leg0_Lower` → `Foot0` → four toes
`Finger_1..4` (each `_1`, `_2`, `_2_end`).

Wings, mirrored L/R: `Arm_Upper` → `Arm_Lower` → `Palm` → six feather chains
`Feather_1..6` (each +`_end`).

### 5.4 The one real modelling constraint

**This is a feathered bird rig, and a Pteranodon is not a bird.** The wing
chain terminates in six discrete feather bones per side. A pterosaur carries a
single membrane stretched from an elongated fourth finger to the body, not six
quills.

The rig is still usable - the six feather bones can drive the membrane's
trailing edge and the `Palm` → `Feather_1` chain can stand in for the
elongated finger - but the mesh has to be authored for that from the start.
Discovering this after a mesh existed would have meant rebuilding it, which is
exactly why the charter requires the observation before the art.

The legs are a better fit: four toes with two joints each suits a pterosaur
foot without adaptation.

## 6. Disposition

The seam is proven, not hypothesised. The donor is a single-renderer,
single-material, 72-bone skinned prefab with consistent bind poses and a child
Animator; the toolchain is exact and installed; and the instance-local
mutation pattern is already shipped and tested. Sprint 2 is **feasible and not
blocked**.

Remaining work: author an original pterosaur mesh and textures skinned to the
72 bones listed above, build a deterministic Unity 2018.4.10f1 bundle,
implement safe loading with validated instance-local binding and an approved
native fallback, and qualify it live in RTWP and turn-based play with
Eagle, Dire Bat and Roc as donor-sharing negative controls.

Nothing in Sprint 2 is complete. No Pteranodon visual has changed, and the
accepted proxy remains in place as the approved fallback candidate.
