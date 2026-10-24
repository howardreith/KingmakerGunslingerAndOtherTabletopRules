# Cleanup iteration 2: giant presentation

Acceptance ID: ART-08. Source base `a35a3277a9c5e2c111475523b868c2cf08422e05`.
New-source lane only; the integrator owns palette registration, project/test
registration, native observations and the six-family lineup.

| Implementation / technical verification | Native evidence | Owner acceptance |
| --- | --- | --- |
| Exact giant/provenance policy and measured-height lifecycle patch implemented. Integrator reports production compilation and all four giant cases PASS within 49 focused tests; actual invocation/duration are recorded by integration. Active height calibration intentionally absent. | Fresh read-only donor census observed, with numeric bind-joint data below. No candidate lineup or measured human comparison yet. Height refinement remains incomplete. | Pending |

## Existing tint audit

Stone and Fire already register separate visual variants in
`ExpandedSummoningSpecialBuilder.Giants.cs`. The existing
`ExpandedSummoningVisualVariantPatch` clones each instance's renderer materials,
applies tint, reinitializes the native material controller so it reads the
replacement materials, and records ownership. Its destruction path restores the
renderers and destroys both its original clones and controller-created material
instances. No shared donor material is recolored by that seam.

Historical `planning/SPRINT28-STATUS.md` records settled native observations on
RunId `20261020T0550345217861Z-fb5e711df25a493b90c9eb69b86cb991`: Stone and Fire
each bound three private materials, and the two skinned body instances retained
the exact `_TintColor` values `(0.660, 0.680, 0.690)` and `(0.860, 0.450, 0.330)`.
That evidence does not approve the colors aesthetically and is not relabeled as
execution on this candidate. It does show why a new tint-overwrite patch is not
justified solely by the owner's weak-color report. Existing materials and
controller logic are preserved until current evidence identifies a failure.

Cloud and Storm lacked distinct palette registration. The integrator will add
their palette variants through the existing ownership/reinitialization API;
the new giant patch allocates no material, renderer, mesh or texture. Tint can
only modulate the underlying donor texture: a pale tint alone does not prove
that a brown donor now reads as powder-blue Cloud skin. Actual skin/hair/armor
and silhouette inspection remains required.

## Exact scope

`GiantSummonPresentationPolicy` authorizes these released unit identities:

| Family | Unit GUID | Physical art reference |
| --- | --- | --- |
| Hill | `58770207fd8c4084a0d9addbac591616` | 10 ft |
| Stone | `956501547dd54479b329a315aab07a05` | 12 ft |
| Frost | `590cd3d5e76fdc649a5f97bc984cd3c4` (retained native) | 15 ft |
| Fire | `65481592185641c99c502e21ba9a5fa2` | 14 ft provisional art target within 12–16 ft reference |
| Cloud | `ff4696c6336141a3b2236cb0a22c86f3` | 18 ft |
| Storm | `4b1143140c614a0aaaa1118c8a44c9ae` | 21 ft |

Frost additionally requires the actual `SummonedUnitBuff.Context.SourceAbility`
GUID to be one of the exact released canonical/alternate Frost wrapper GUIDs.
The explicit symbol/GUID map is in the policy and is checked against the
current blueprint manifest. A matching name, KMG prefix, arbitrary summon,
missing context, multiple summoned-unit buffs, or ordinary native Frost enemy
does not authorize mutation. Other native Hill/Frost donor consumers are not
included. Module-off resolution is inert.

The buff-context read follows the existing accepted summon-glow engine seam.
The attach hook retries after the exact native summoned-unit buff's
`SpawnParticleEffect`, because a fresh view may attach before provenance exists.
Cold reconstruction also checks the currently attached buff. Actual source GUID
and timing still require current native observation; unknown provenance remains
unmodified.

## Measured height policy and current limitation

No donor multiplier is guessed or activated. The calibration dictionary starts
empty, and owned giant views explicitly report `height=missing-calibration`
through `GiantSummonPresentationPatch.Describe`. This is incomplete visual
refinement, not a PASS or an invisible scale adjustment.

To activate a profile, the integrator must supply actual rest-pose giant and
human heights on the same game-unit basis, the human reference height in feet,
the exact prefab asset ID, original view root scale and evidence identity.
The uniform visual factor is:

`(giant reference feet / human reference feet) * (measured human height / measured giant height)`.

This derives the result from measured donor dimensions. It neither assumes
that all native rigs have equal height nor treats animated spawn bounds as
calibration. Fire's 14 ft target is an unapplied midpoint for review, not a
claim that PF1e specifies a unique Fire height. Native usability and the
human-plus-six-giant lineup may require an explicitly recorded art adjustment.

The patch captures each view's unmodified root scale. Repeated attachment
recognizes the prior applied target; a native reset to the original baseline
can reapply the same absolute target without compounding. An unrelated scale
change is preserved and reported. View replacement gets a new record;
destruction/view reuse restores only a transform still matching this patch's
owned result. The patch writes only `transform.localScale`; unit Size, reach,
stats, costs, equipment, AI and blueprints are not written. **Root scaling does
change native selection collider dimensions**; it must remain inert until a
verified visual-only target replaces this seam.

## Focused checks and outstanding native work

`Cleanup2GiantPresentationTests` covers exact manifest/source identity mapping,
native Frost negative provenance and module-off cases, synthetic measured
height arithmetic, quantity-independent transform idempotence, original-baseline
reapplication, foreign rescaling, missing/wrong/invalid measurements, and absent
evidence identity. The synthetic 3.0/1.8/6.0 fixtures are calculation tests only;
they are not native Hill/Frost measurements. The integrator reports four giant
cases PASS as part of the 49 focused cases and production compilation. Its
compile corrected `BuffCollection` enumeration to `foreach`, matching the
actual installed collection surface; no LINQ contract is assumed for it.

Still required: common-frame Hill/Frost body rest-height measurement; a measured
human control; selected calibration and source/evidence
identities; normal-zoom human/Hill/Stone/Frost/Fire/Cloud/Storm lineup; skin,
hair/clothing/armor and silhouette differentiation; height/ground contact while
walking, turning and attacking; native tint stability; retained Frost provenance
positive/negative controls; simultaneous instances; despawn/recreation and
reattachment/reload without compounding scale or material leaks. Parent/donor
controls and gameplay statistics must remain unchanged.

No fresh game process was launched and no save or installed mod was changed by
this lane. No native or owner aesthetic acceptance is asserted.

## Current donor census observations

The integrator's source `638eafa` guarded read-only
`observe-expanded-summoning-sprint28-native-inputs` run completed PASS in
135.3 s, with exact installation/settings restoration and unchanged saves
reported by integration. RunId:
`20261021T2240252686756Z-observe-expanded-summoning-sprint28-native-inputs`.
Raw `s28-native-inputs.json` SHA-256:
`33f8e7cb0ec065073d1cc82e269b21935752f7354090ae27d3c54a6e30ce37bf`.
This is baseline detached metadata, not candidate visual qualification.

| Actual control | Prefab / renderer | Root local scale | Head Y | Mean Toe0 Y | Head-to-toe joint span |
| --- | --- | --- | --- | --- | --- |
| Hill donor `61dd71e874b16f742a0f783314105553` | `635d6dee1c7831b41aa987bdbeff14bc` / `hill_giant_1`, 53 bones | `(1,1,1)` | 3.298406 | 0.100757 | 3.197649 |
| Retained Frost `590cd3d5e76fdc649a5f97bc984cd3c4` | `df0f3fbceb3605d489c0dec8fa5c7d1e` / `Frozen_body`, 48 bones | `(1,1,1)` | 3.298406 | 0.1007571395 | 3.1976488605 |

These positions come from inverse mesh bind poses in each renderer's local
frame. The donor head/toe spans are essentially identical, but a head joint is
not the crown and a toe joint is not the sole. The capture helper also omits
renderer-to-view scale/rotation and contains no human control. Therefore these
values do not yet establish actual game-world body height and no calibration
was registered from them.

The reported body `SkinnedMeshRenderer.localBounds` heights are 3.84926963 for
Hill and 3.93093824 for Frost. They are culling bounds, not guaranteed rest-body
extents. Hill outfit height 3.44172049 and Frost cape height 2.77928686 further
illustrate why arbitrary renderer bounds should not stand in for anatomy.
The 14 named census candidates were Hill variants; Stone/Fire continue using
their deliberately selected Hill/Frost donor families. A bounded common-frame
measurement must now add renderer transforms, crown/sole convention and the
human reference before measured normalization can be enabled.
