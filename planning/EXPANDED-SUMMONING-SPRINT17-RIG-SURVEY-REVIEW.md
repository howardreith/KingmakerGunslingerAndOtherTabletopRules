# Sprint 17 bounded native-rig research

## Current disposition, 2026-10-06 UTC

RESEARCH SOURCE CHECKPOINT; Sprint 17 NOT QUALIFIED. The containing commit
descends normally from pushed Sprint 16 closure evidence
`b27b6aea04d4e1e2b151339fca36407ba45bb8a9`. PR #26/laptop remains the sole
active line. No DATA source imported. Sprints 14-16 remain complete.

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
- Exact-head repository/full suite/clean build/strict package: PENDING.
- Guarded metadata survey: NOT RUN.

The first frozen source `dc3aff789dbe747a49c5727c41b88deb810d0451` stopped
in repository validation: the pinned development test count still said 2020
after adding the one new policy test (actual 2021). No build artifact or game
launch occurred. The narrow follow-up updates both the validator and current
development metadata to 2021, preserving the immutable public-release count.
The exact gate must run on that new committed source, not reuse this failed
attempt. Machine-local log: `artifacts/sprint17-dc3aff78-exact-gate.log`.
The complete repository wrapper passes after the pin correction; log
`artifacts/sprint17-count-repair-validation.log`. Full exact gate remains next.

## Primary-rule intake

The [Venomous Snake](https://aonprd.com/MonsterDisplay.aspx?ItemName=Venomous%20Snake),
[Constrictor Snake](https://aonprd.com/MonsterDisplay.aspx?ItemName=Constrictor%20Snake)
and [Salamander](https://aonprd.com/MonsterDisplay.aspx?ItemName=Salamander)
records were checked against the existing Sprint 17 contracts. Viper here is
the Medium venomous snake, not the Tiny familiar. Bite damage remains 1d4-1.
Land skill rows needing implementation/live breakdown include Viper
Perception/Stealth/Acrobatics +9 and Constrictor +12/+11/+15 respectively.
Aquatic/climbing-only consumers remain outside the land-use scope.

Salamander retains its existing identity/placements and requires a separate
hybrid/weapon seam: manufactured spear, secondary tail, differing reach and
outgoing heat (not invented incoming thorns). Its older native grab graph,
unprinted Weapon Focus and missing printed feats/skills need focused review.
No implementation or adaptation is accepted merely by this research note.

Accepted engine limitations remain unchanged. HumanReview:
NOT_PERFORMED_NONBLOCKING. Phase 2C remains authorized but deferred until
Phase 2B owner acceptance; this mission stops after Phase 2B closure.
