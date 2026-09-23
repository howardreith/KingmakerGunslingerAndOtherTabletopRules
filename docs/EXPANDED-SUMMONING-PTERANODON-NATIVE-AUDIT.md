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

### 1.1 The donor is shared by four creatures

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

## 5. Open questions that require the live donor

None of the following may be answered by name matching or assumption; the
charter forbids treating a Roc-to-pterosaur adaptation as proven:

1. Is `CharacterAvatar` null for the donor, confirming a plain prefab with
   `SkinnedMeshRenderer` plus `Animator` rather than a Character doll?
2. The exact renderer set, their `rootBone`, `bones[]` order and bind poses.
3. The exact bone hierarchy and names available to bind an original mesh to.
4. The animator controller, its clips, and the attack/impact events that drive
   bite timing.
5. Which transforms `ParticlesSnapMap` and `UnitHitFxManager` anchor to.
6. The collider and `CameraOrientedBoundsSize` values that own selection,
   targeting and footprint, so the replacement preserves them.

These require the guarded runtime observer path, following the precedent in
`docs/FIREARM-NATIVE-RIG-FORENSICS.md` and the
`observe-native-firearm-rig-contracts` scenario. Until they are answered from
the running game, no mesh should be authored: authoring against a guessed
skeleton is exactly the "bone-name matching is not proof" failure the charter
names.

## 6. Disposition

The seam exists, the toolchain is exact and installed, and the instance-local
pattern is already shipped and tested. Sprint 2 is **feasible and not
blocked**. Its remaining work is live donor inspection, original mesh and
texture authoring, a 2018.4.10f1 bundle, the runtime loader with validated
fallback, and live camera/motion acceptance in both RTWP and turn-based play.

Nothing in Sprint 2 is complete. No Pteranodon visual has changed, and the
accepted Roc-policy proxy remains in place as the approved fallback candidate.
