# Cleanup 2 giant render scale prototype

## Experimental scope

`RuntimeTesting/Cleanup2GiantRenderScalePrototype.cs` is an additive,
request-owned experiment. There is no production registration, default
activation, height calibration, or accepted giant appearance change. The root
integrator owns project inclusion, a future bounded native phase and builds.

The helper admits only an enabled, automatic-exit Cleanup 2 native review
request with the single parameter `reviewPhase=scale-prototype`. The actor must
occur exactly once by reference in the caller's positive request-owned actor
list, differ from the request caster, and carry exactly one native summoned
buff whose context identifies that exact caster and a source ability. These
checks supplement the existing guarded request/lease/transaction admission;
they do not authorize another runtime entry path.

The proposed factor is finite and uniform, between 0.1 and 10. This is an
experimental input bound, not a calibrated giant height or balance decision.

## Native input and reason for this experiment

Preserved cloth probe directory:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261022T0009584048373Z-disposable-expanded-summoning-cleanup2-native-review`.

Run ID: `20261022T0009584213879Z-7132ad2085c046bfa7a765a14ebaa538`.

| Artifact | SHA-256 |
| --- | --- |
| `runtime-result.json` | `8d9e47edb64f325049867a920670d0d40944a9e348ef9ead7062bbf29d2ce011` |
| `summon-cleanup2-observations.json` | `fa1388e6bc0cc27db12d1c615f3707bbb72ba1e2120c6675d3009d38a18bf562` |

That run remains FAIL. On the exact native Frost cape, enabled Cloth had 292
coefficients, 92 pinned coefficients, 27 renderer bones, five capsule references
and no sphere pairs. The observed mesh has 354 vertices; a direct particle to
mesh index map was not proved. The coefficient fingerprint was
`1caf9c99f5b5755f` and maximum allowed distance was 1.

| Stage | Maximum raw Cloth displacement | Maximum raw BakeMesh displacement | Native source-bone angle change |
| --- | ---: | ---: | ---: |
| visible | 2.525731 | 2.1441853 | 179.618332 degrees |
| renderer disabled | 0 | 2.72853112 | 177.830978 degrees |
| restored | 2.10844731 | 2.56679416 | 179.241989 degrees |
| shadows only | 2.53140116 | 2.71925 | 178.877625 degrees |

Renderer suppression froze the native Cloth samples while source animation
and BakeMesh continued changing. ShadowsOnly retained positive Cloth motion.
Neither path produced a consistent tested coordinate hypothesis. The proposed
helper consequently creates an owned solver instead of treating a BakeMesh
array as native simulated cloth, or assuming a welded-particle index mapping.

## Verified installed API and remaining collision unknown

The installed UnityPlayer file version is `2018.4.10.10503941`.

| Reference | Identity |
| --- | --- |
| `UnityEngine.ClothModule.dll` MVID | `7c2e9a3d-365c-4884-b034-cb11e7467628` |
| ClothModule SHA-256 | `390e7edea7c242f764c4645c8582cf02bfdb3d2d86f51d4ea8d8b91ac5e57e91` |
| `UnityEngine.CoreModule.dll` MVID | `bd5ffe06-494e-4588-a068-c8443cc48c47` |

Reflection-only inspection verified writable supported Cloth scalar/vector
settings, coefficients, capsule and sphere arrays, virtual-particle index and
weight methods, and self/intercollision index methods. The helper copies the
exact non-obsolete scalar list, coefficient arrays and these particle settings
to the owned solver, then requires an exact round trip. Deprecated
`selfCollision` is an error-level obsolete unsupported property, and the old
`solverFrequency` and `useContinuousCollision` aliases are not used in place of
their supported fields. Original Cloth fields are read only.

The actual installed Renderer/SkinnedMeshRenderer properties and indexed and
unindexed material-property-block methods used by the helper were also
verified by reflection. Native `WeaponSnap.LateUpdate()` copies its snap target
position/rotation and has no DefaultExecutionOrder attribute. The helper uses
order 10010, matching the repository's established visual attachment order
after the default-order native snap; a native observer must still verify the
actual weapon pose and execution timing.

Unity 2018.4 documents explicit capsule arrays (at most 32), and cloth response
that does not apply forces back to the world. These contracts support an owned
visual solver with a bounded explicit collision list; they do not establish
that a **disabled** capsule is consumed by the native Cloth solver.
[Cloth manual](https://docs.unity3d.com/2018.4/Documentation/Manual/class-Cloth.html),
[capsule array API](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Cloth-capsuleColliders.html).

The collider documentation says disabling a collider prevents collider-to-
collider interactions. It does not resolve the separate Cloth-array question.
The helper therefore always reports disabled-capsule collision response as
`UNKNOWN_REQUIRES_NATIVE_CONTACT_PROOF`; it never enables an owned capsule as
a fallback. [Collider enabled API](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Collider-enabled.html).

## Implementation and ownership

Before creating any Unity object or mutating source fields, a managed plan
preflights the complete view: bounded transform/renderer/collider counts,
finite positive transforms, renderer mesh/material and bone ownership, every
Cloth component, supported native collision geometry and proposed coefficient
arithmetic. Missing/foreign palettes, unsupported active renderer types,
nonempty sphere arrays and unsupported coefficient sentinel values refuse the
whole view. There is no partial scale fallback.

The owned root starts inactive beneath the unchanged native view root and
holds the uniform factor. It contains transform-only mirrors with prefixed
index names, preserving source hierarchy and local poses without copying
Animators, unit components, locators or native behaviours. Ordinary non-Cloth
skin palettes/root bones point to the mirrors; their meshes and materials
remain borrowed and their source transforms remain unchanged. Their culling
bounds account for the proposed visual scale.

Each native Cloth renderer gets an owned skinned-renderer/Cloth pair borrowing
its mesh and materials and binding to the mirrored palette. The original cape
renderer is suppressed; its original Cloth component, bones and configuration
are untouched. The new solver receives copied native settings. Explicit
`ProposedClothDistances` multipliers apply only to copied `maxDistance` and
`collisionSphereDistance` values. All other scalar settings, including self-
collision distances, are copied exactly. Neither multiplier is inferred from
the render factor, and neither choice is accepted calibration.

Only the native Cloth's capsule references are mirrored. Each owned capsule is
created under an inactive owned object, set disabled before that object or
the owned root can become active, and given the same local center, radius,
height and direction. No native collider property or transform is assigned.
Static meshes receive owned MeshFilter/MeshRenderer proxies borrowing mesh and
materials; original static renderers are conditionally suppressed. Renderer
settings and material property blocks are copied to owned renderers. No mesh,
material, texture, animation or model geometry is cloned or exported.

LateUpdate checks source identity before mirroring poses, and checks exact
same-frame native transform and collider matrices after its writes. Native
collider geometry, flags, material identity and census must remain exact.
The report includes intake/current geometry, intake matrices, matrices before
and after the last prototype write, aggregate invariance predicates and the
owned disabled-capsule inventory. Naturally animated native collider poses are
allowed to change between frames; the helper must not change them within its
own synchronous operation.

Foreign palettes/materials, hierarchy refresh, actor/view reconstruction,
native Cloth configuration changes and enabled/replaced owned capsules refuse
continuation. Disposal conditionally restores only the exact palette/root/
bounds or suppression fields still owned, including a commit interrupted
between field setters. It records foreign-state restoration issues and
destroys only its owned root and components. Borrowed assets and native
components are never destruction targets.

## Entry contract for the future bounded observer

```csharp
var proposal = new Cleanup2GiantRenderScalePrototype.ProposedClothDistances(
    maximumDistanceMultiplier, collisionSphereDistanceMultiplier, proposalIdentity);
var prototype = Cleanup2GiantRenderScalePrototype.Take(
    request, actor, requestCaster, exactPositiveOwnedActors, uniformFactor, proposal);
try
{
    // Advance only the existing qualified native clock/animation path.
    // Describe() returns structural evidence; OwnedCloths supplies actual
    // owned Cloth references for a separate bounded motion/contact observer.
}
finally { prototype.Dispose(); }
```

`Synchronize()` is available for explicit same-frame observation, while normal
pose following occurs in LateUpdate. The future harness must not treat a
manual synchronization call as proof of after-WeaponSnap scheduling. It must
retain native particles/motion/contact evidence and actual framebuffer review.

## Validation status and blockers

This lane performed scalar native-evidence inspection, installed API/IL checks,
official Unity 2018.4 documentation checks and source safety review. It did not
build, launch the game, register a phase, change project files or mutate Git.
The new source has not been production-compiled or natively executed yet.

Unresolved: disabled-capsule Cloth collision consumption; exact copied solver
behaviour on this runtime; distance/gravity/inertia treatment at another visual
scale; native highlight/buff/visibility integration; weapon timing and visual
contact; foreign model refresh handling in a real run; and six-family height
calibration/visual acceptance. Reports retain explicit UNKNOWN/EXPERIMENTAL
status for these questions. Source feasibility is not ART-08 acceptance.
