# Sprint 17 bounded native-rig research

## Current disposition, 2026-10-06 UTC

Exact corrected research **a173da3a396b3c7f1bddef3bee92c476150d8639**
passes the complete prelaunch gate: 219 focused, 2021 unfiltered (84.5 seconds),
486 preflight, 68 orchestration, persistence/crowd request tests, full repository/
static/icon/manifest, clean exact Release and deterministic strict 312-member
package. Fresh Steam smoke **11/11** and three-target survey **11/11** PASS.
No production creature, registration, asset, icon or gameplay change.
Sprint 17 remains NOT QUALIFIED; Sprints 14-16 remain complete and published.

Actual snapshot `20261006T0239169703799Z` restored exactly at
`2026-10-06T02:45:49.8363590Z`: 136 files / Info 0.0.117 / tree
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Lease Completed/recoveryRequired=false; no game/shared lock/staging or save
write. Owned-only native cleanup restores 955 original unit references,
three party references and exact area membership. [Both attempts' exact
hashes and request identities](EXPANDED-SUMMONING-SPRINT17-FALLBACK-RESEARCH-EVIDENCE.json).

## Corrected three-target research findings

- Water Elemental: seven skins at renderer/view scale 0.06. Body and
  bubblegum stripe use `PF/StandardDynamic`; five other water skins use
  `PF/Particles`. The body has spine/head/jaw but no long continuous tail
  chain. The original lower-coil prototype is rigid. This is a motion risk,
  not evidence that the body's shader is intrinsically unusable.
- Purple Worm: two skins, `Purple_Worm` and `Purple_Worm_Stones`, each with
  40 complete bones, renderer/view scale approximately one. Both use
  `PF/StandardDynamic`. `Hips_Joints` through `Body02` ... `Body014` to `Head`
  form a continuous chain. Upper/lower/left/right jaw branches are separate.
  Original snake geometry can be compared on that chain while omitting the
  radial side petals, horn and stone geometry. Stones must be suppressed only
  on exact new snake identities; native Purple Worm remains unchanged.
  This is **bind metadata**, not live ground, locomotion or jaw-contact proof.
- Salamander: the existing Lizardfolk body has 39 bones and an additional
  19-bone armor skin. Native static club/shield meshes remain a genuine visual
  defect. A separate original hybrid/weapon seam is mandatory; its identity
  and placements remain fixed. No spear-contact qualification is inferred
  from a manufactured-weapon animation action being present.
- Native action metadata now distinguishes a missing clip enumeration from
  an exposed empty collection. Prone/CastSpell expose missing enumerations
  on these views. Null does not mean no animation capability. No native
  vertices, UVs, texture pixels, animation curves or proprietary assets were
  exported or redistributed.

Private original Purple Worm-chain snakes and a separate Salamander hybrid
body are now authored. [Source, offline findings and exact private hashes](../assets-source/original-models/sprint17-serpents/SOURCE.md).
Four water, four continuous-chain and six hybrid authoring tests PASS; 74 new
framed review panels. Offline review corrected clipping/lighting, detached
fingers, the waist seam and dorsal/belly texture orientation. No prototype
enters the package and Salamander's weapon is not implemented by this body.
Next: bounded live donor motion/ground/jaw acceptance and actual Salamander
spear/tail seam, then printed profiles/signatures and one stable hidden
qualification candidate. Donor measurements and offline tests are not gameplay
or visual lifecycle qualification.

## Preserved private prototypes and failed b2893abe research

Sprint 17 remains NOT QUALIFIED. After pushed evidence e5e79fe9, two private
original snake prototypes pass four Blender behavior checks and have 40
offline review panels. [Source, hashes and remaining problems](../assets-source/original-models/sprint17-serpents/SOURCE.md).
No prototype enters the package. The water frame has no continuous tail chain;
the lower coil is rigid, so a native Purple Worm fallback comparison is
necessary before choosing a production donor. Its exact recorded native pair
is `bf2216f48b3f4d24c9c502007649340d` /
`130f0866af3249a4e817ec7e6e9ecd89`, from the October 2 native-donor audit
`20261002T1440160968919Z-observe-expanded-summoning-native-donors`.

The fixed research allowlist now contains those three existing summons, not
arbitrary request keys. The source-only extension also records renderer/view
scale and material shader names/queue/slot flags plus native animation action
types/counts, never native art data or animation curves. It does not alter
Purple Worm gameplay. Exact cross-donor substitution rejections are tested.
Exact b2893abe passes 219 focused / 2021 full (82.7 seconds), 486 exact-head
preflight, 68 orchestration, persistence/crowd requests, repository/static/
icon/manifest, clean exact Release and strict 312-member package. Fresh Steam
smoke passes 11/11. The extended survey **FAILS, 8/9**, before writing any
metadata: a present native animation action returns a **null Clips collection**,
and the new diagnostic called LINQ Count on it. Six exact donor/view checks
pass; fixture cleanup restores 955 original unit references, three party
references and exact area membership. This is a **fixture observation defect**,
not a production mechanics failure or an accepted omission.

Actual leased snapshot `20261006T0222526525316Z` is exactly restored at
`2026-10-06T02:29:30.3453602Z`: 136 files / Info 0.0.117 / tree
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Lease Completed/recoveryRequired=false; no game/shared lock/staging; no save
write. [Exact failed artifact, request and restoration hashes](EXPANDED-SUMMONING-SPRINT17-FALLBACK-RESEARCH-EVIDENCE.json).

The narrow correction preserves null as **no exposed enumeration**, distinct
from an exposed empty list or a list containing null entries. It does not
pretend missing metadata means zero native animation capability. Null, empty,
all-null and mixed regression cases pass through the same counting policy;
219 focused / incremental Release PASS. Its exact a173da3a complete gate and
new smoke/survey batch subsequently pass as recorded above. The failed b289
attempt remains failed, not retroactively qualified.

## Previous exact two-target research, f507278b

RESEARCH PASS ONLY; Sprint 17 NOT QUALIFIED. Exact research candidate
`f507278bbebe5894130dc6ec51978580bc2c43d0` passed all prelaunch gates and
fresh Steam smoke **11/11**, metadata survey **8/8**. Actual leased snapshot
restored exactly. [Artifact/request/result/restoration hashes](EXPANDED-SUMMONING-SPRINT17-RIG-SURVEY-EVIDENCE.json).
It descends normally from Sprint 16 closure evidence b27b6aea. PR #26/laptop
remains the sole active line. No DATA source imported. Sprints 14-16 remain complete.

Only guarded research code, its fixed policy/schema tests and current-state
documentation change. No new summon, identity, asset, icon, mechanic or save
state; no publication or count change (976 + 29 = 1005 visible).

## Why this survey is necessary

The tranche census included optional-mod blueprints despite its historical
"native units" label. `SerpentineWaterElementalEidolonUnit`
(`6f5db07e89834a01aa9b7e7aed6cd407`) carries Eidolon-specific components and
must not become a new dependency. The archived inventory
`20261002T1321056753532Z-observe-expanded-summoning-inventory/runtime-result.json`
also records the exact same prefab on the existing **native Medium Water
Elemental**. Thus the proposed bind-frame research can use a native carrier.
This corrects provenance, not rig suitability: live frames must still prove
whether an original limbless snake can use it.

| Fixed target | Native blueprint | Native prefab |
| --- | --- | --- |
| Medium Water Elemental | `62a3e860e6e72e6499c38bb8b2fe303e` | `dc296683c2a3d2648afa516aeb030fb8` |
| Salamander's Lizardfolk donor | `e8276e28b2234a745900fed80670bfdb` | `9b1744531a4428e44aa9837ca984513a` |

The new `disposable-expanded-summoning-serpentine-survey` accepts only the
guarded native `KMG_AUTOMATION_WORKING` load, never an arbitrary asset/key or
protected baseline. It verifies these native blueprint/view pairs, casts the
two existing own-tier project summons, disables only their disposable AI,
and permits 60 native frames (45-second bound) before private metadata capture.
It does not remove appearance buffs, force visibility or alter animations.

Evidence contains renderer/component names, counts, bounds and bone-local
bind positions/rotations, optionally Unity controller clip names/durations.
No native vertices, triangles, UVs, textures or animation curves are exported
or committed. Multiple skinned renderers are a finding, not an assumed error;
each actual skin must have a complete nonempty bind frame. Exact owned-only
native destruction and original unit/party/area references are checked.

This proves only source provenance and available bind metadata, not visual
fidelity, contacts, normal AI, gameplay, persistence or publication. It makes
no save write. The next transaction must acquire the runtime lease before
observing/snapshotting live files, pin one exact committed package, run fresh
Steam App ID 640820 smoke/survey processes and restore that actual snapshot.

## Source checks before freezing

- Focused Expanded Summoning: **219/219**, 2021 registered, 2.8 seconds.
- Incremental Release compile: PASS (not an exact-reference candidate).
- Runtime scenario preflight: **486** PASS, run alone before artifact writers.
- Orchestration: **68** assertions PASS.
- Persistence: 11 wiring checks, 3 exact scopes, 19 rejection cases PASS.
- Crowd: 5 exact request round trips, 7 rejection cases PASS.
- Exact-head repository/static/icon/manifest and full suite: **2021/2021**,
  unfiltered, 90.2 seconds.
- Clean exact-reference Release and strict deterministic package: PASS,
  312 members, version unchanged at 0.0.141.
- Guarded metadata survey: **8/8 PASS**, smoke **11/11 PASS**, two fresh
  Steam App ID 640820 processes, one immutable package.

The first frozen source `dc3aff789dbe747a49c5727c41b88deb810d0451` stopped
in repository validation: the pinned development test count still said 2020
after adding the one new policy test (actual 2021). No build artifact or game
launch occurred. The narrow follow-up updates both the validator and current
development metadata to 2021, preserving the immutable public-release count.
The exact gate must run on that new committed source, not reuse this failed
attempt. Machine-local log: `artifacts/sprint17-dc3aff78-exact-gate.log`.
The complete repository wrapper passes after the pin correction; log
`artifacts/sprint17-count-repair-validation.log`. New exact f507 full gate
passes; log `artifacts/sprint17-f507278b-exact-gate.log`.

## Live findings and exact restoration

Run `20261006T0142363709333Z-disposable-expanded-summoning-serpentine-survey`
confirms both native blueprint/view pairs and their existing public summons.
At 60 native frames:

- Water Elemental: **seven** skins. `body` has 43 complete bones rooted at
  `LowerTorso`, including a spine chain, neck/head and `Jaw_M`, plus arms and
  water-spiral branches. Six further skins carry the water surface/effects.
  This is not a ready-made snake, nor a single-renderer swap. The bounded
  prototype must use the torso/head/jaw without arm geometry, suppress every
  exact-owned water skin atomically and retain native fallback/cleanup.
- Lizardfolk: `_lizardman001` has **39** bones rooted at `Torso_Lower`, with
  manufactured-weapon arm/hand chains, jaw and four tail joints. A second
  `_ammunition04` skin has 19 bones. The prefab still shows native
  `WP_ShieldLightDamaged` and `lizardman_club` meshes despite the existing
  Salamander spear blueprint. Those are an explicit S17 visual defect to
  replace through the separate owned humanoid/weapon seam, not evidence of
  correct spear handling.
- No Unity controller clip list was exposed; the views carry Owlcat's native
  `UnitAnimationManager`. No animation curves/assets were exported.
- Owned-only destruction restored all **955** original unit references, all
  **3** party references and every original area membership. No save write.

The earlier S16 pre-candidate note's Purple Worm recommendation and Tiny/Small
Viper premise were hypotheses, not qualified contracts. Viper is **Medium**;
this measured native water-body/jaw chain is the first bounded snake prototype
candidate. It still needs an offline and live visual slice; the research PASS
does not accept a compromised shape or authorize a global animation rewrite.
Salamander remains a separate seam and keeps its existing identity/placements.

Lease `runtime-20261006T013907Z-c7cd7841df2344e3999cf79923710591` was acquired
before live observation. Actual snapshot `20261006T0139078842255Z` restored
at `2026-10-06T01:45:45.9953210Z`: **136 files**, Info **0.0.117**, tree
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Lease Completed/recoveryRequired=false; zero game processes, no shared runtime
lock or staging. Immutable ZIP/sidecar/driver/extraction and raw private frames
remain in `artifacts/laptop-runtime-20261006T0139071137036Z` and the named
evidence directory. Prior native working-save cleanup remains unmodified.

Next: bounded original mesh/binding prototype, exact profile/signature work,
offline pose review and one stable hidden Sprint 17 qualification candidate.
No new creature is registered or published by this checkpoint.

## Primary-rule intake

The [Venomous Snake](https://aonprd.com/MonsterDisplay.aspx?ItemName=Venomous%20Snake),
[Constrictor Snake](https://aonprd.com/MonsterDisplay.aspx?ItemName=Constrictor%20Snake)
and [Salamander](https://aonprd.com/MonsterDisplay.aspx?ItemName=Salamander)
records were checked against the existing Sprint 17 contracts. Viper here is
the Medium venomous snake, not the Tiny familiar. Bite damage remains 1d4-1.
Land skill rows needing implementation/live breakdown include Viper
Perception/Stealth/Acrobatics +9 and Constrictor +12/+11/+15 respectively.
Aquatic/climbing-only consumers remain outside the land-use scope.

The [universal Grab rule](https://aonprd.com/UMR.aspx?ItemName=Grab) permits
same-size or smaller prey. The existing project's delta-zero policy is correct
for Constrictor Snake and Salamander; do not introduce a one-category-smaller
restriction. [Constrict](https://aonprd.com/UMR.aspx?ItemName=Constrict) is
additional damage on a successful grapple check, including the establishing
check, and uses the creature's printed damage. These are verified source rules,
not new balance adaptations.

Salamander retains its existing identity/placements and requires a separate
hybrid/weapon seam: manufactured spear, secondary tail, differing reach and
outgoing heat (not invented incoming thorns). Its older native grab graph,
unprinted Weapon Focus and missing printed feats/skills need focused review.
No implementation or adaptation is accepted merely by this research note.

Accepted engine limitations remain unchanged. HumanReview:
NOT_PERFORMED_NONBLOCKING. Phase 2C remains authorized but deferred until
Phase 2B owner acceptance; this mission stops after Phase 2B closure.
