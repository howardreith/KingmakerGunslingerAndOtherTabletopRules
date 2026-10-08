# Sprint 17 bounded native spear-carrier audit

Status: exact 291059aa RESEARCH ONLY PASS; Sprint 17 NOT QUALIFIED.

Exact 291059aa: 237 focused / 2039 unfiltered tests (88.3s), complete gate
183.2s; 491 preflight / 81 orchestration / 17 provenance, persistence 11/3/19,
crowd 5/7, clean exact Release / strict 318-member package PASS. Fresh Steam
smoke 11/11 and metadata survey 15/15 PASS. RESEARCH ONLY; Sprint 17 remains
NOT QUALIFIED.

All three archived spear/longspear prefabs reference the native human
MyAnimationSet (24 actions), with PiercingTwoHanded clips
Human_2H_spear_attack_01 / _02 (1.46666718 / 1.40000057 seconds).
Command-act events are at 0.620427966 / 0.734528542 seconds. Native references
and actor membership are unchanged. This is metadata, not actual playback,
grip, contact or a safe Salamander binding. No action adopted or NPC spawned.

Each prefab has 177 unique bone paths but a combined duplicate bone palette
(2252 / 1776 / 2507 entries) and a zero-bone cape renderer. Cape bones do not
prove an articulated tail. These human clips cannot simply replace the
incompatible 39-bone Lizardfolk action. The latest body570 gate remains 33/35
FAIL before hybrid attachment; all 12 body attempts and prior censuses remain.

Actual snapshot 1326496740021Z restored at 13:33:27.2332485 UTC:
136 files / Info 0.0.117 / exact tree. Journal
97A08A2FD8C08DB595ED36637633F9156F5567724068489DAD7C4FF2AF6FF207;
spear census 3AF2CBC78820708586547C36952FF76BF9921823EDEA78ED7BA1243F4DAC2687.
Lease Completed/recoveryRequired=false/released; no game/lock/staging/save write.

Next: bounded exact human-prefab binding/native spear-handling proof on a
request-local project-owned Salamander prototype; no native NPC facts/loot,
human-to-Lizardfolk clip transplant or assumed cape/tail compatibility.
Keep independent snake work moving if a bounded hybrid seam is unavailable.
Sprints 14–16 complete; 976+29=1005 visible choices; ZERO DATA ports.
Phase 2C remains authorized but deferred until Phase 2B owner acceptance.

[Exact artifact, request, result and restoration hashes](EXPANDED-SUMMONING-SPRINT17-SPEAR-CARRIER-EVIDENCE.json).

## Detailed human bind-palette follow-up — exact a904 diagnostic PASS

The follow-up is now closed: smoke11/survey16 PASS, but17 duplicate
weapon-storage bind groups disagree. No rig was adopted. See the
[current review](EXPANDED-SUMMONING-SPRINT17-HUMAN-BIND-REVIEW.md).

The containing source checkpoint adds private bind metadata for only the
already observed Guard Captain prefab `ced3729f4b4abab4da4ef63d8489f857`.
It groups the combined renderer's palette by actual Transform reference and
records every contributing slot, finite/invertible status, first bind frame,
and maximum duplicate-matrix difference. It never deduplicates by name,
changes a native asset, creates a campaign NPC or adopts a rig.

The sixteenth survey assertion requires the observed 1776-slot / 177-transform
structure and complete finite, invertible measurement. Agreement is a separate
recorded finding at tolerance 0.00001, not silently made true or interpreted
as contact/animation proof. Disagreement must prevent use as an authoring bind.
Raw coordinates remain private evidence; no native geometry or curve export.

Focused 238/238, 2040 registered, static validator and incremental exact
compile PASS. The first focused test used decimal-derived floats with an
incorrect exact-equality expectation; its failure log is retained. Binary-exact
test inputs corrected the fixture only. Product comparison was unchanged.
Exact a904 then passed all2040 tests, complete178s gate and full prelaunch;
fresh-Steam smoke11/survey16 PASS, actual snapshot restored14:14:02UTC.

A bounded offline alternative audit found the historical Unity2018 editor
activation failure in
`docs/EXPANDED-SUMMONING-PTERANODON-CUSTOM-ASSET-BUILD.md` and its archived log.
Both installed executable versions are2018.4.10.10503941; no editor launch,
activation attempt, credential access or installation was performed.
Unity's [2018.4 SetCurve documentation](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/AnimationClip.SetCurve.html)
limits runtime curve authoring to legacy clips; non-legacy authoring is
editor-only. No runtime flag workaround or animation-system replacement is
being attempted. The native-human binding route remains under investigation;
this historical toolchain issue is not a new phase-wide blocker.

## Source scope

The closed 8022fee2 census proves that none of the eleven recorded Lizardfolk
prefabs has a PiercingTwoHanded main-hand action. Do not repeat that search or
adopt a slashing/one-handed action under a different label. The rejected
conditional Lizardfolk adapter remains inactive; no production view changed.

The archived native census is
`20261002T1440160968919Z-observe-expanded-summoning-native-donors/native-donor-audit.json`,
SHA-256 `0D5FD0A8CFD36AB8D07F99D838CA2C01D2B994CF1EBC0CDCA26DD922FD034D18`.
It identifies three distinct actual spear/longspear prefabs:

| Source | Blueprint | Prefab | Primary weapon |
| --- | --- | --- | --- |
| GoblinShamanBoss | 8421b6137d7765947958973526b5249b | 520c43197dcb8c848a632675c7aa3f27 | LongshankBane / Spear / 928723c8d5238cb409b16e4d077a03d0 |
| OwlbearAttack_CapitalGuardCaptain1 | 86dc43534645e234eb35431131e3b669 | ced3729f4b4abab4da4ef63d8489f857 | StandardLongspear / f28f6031c2908d84d945865a80f67177 |
| RiverPirate_Bard | 063e8f0e64d9b8d41a6a60bf5f13145c | 6e8f58e9489bcb747beb203f72e807a2 | MasterworkLongspear / 9f1545b033149e6429cc9c29354fd9f1 |

The research scenario adds one fifteenth assertion and a separate metadata
file, `sprint17-native-spear-actions.json`. Blueprint, prefab, weapon, category
and absent offhand must match exactly. No NPC is instantiated, no manager
initialized and no animation adopted. Missing detached managers/sets remain
explicitly unknown rather than fabricated human playback. Existing eleven
Lizardfolk observations retain their stronger already-established manager/set
requirements. All native references and actor membership must remain unchanged.

The source test exercises every identity field, cross-donor substitution,
offhand rejection and nested-array ownership. Focused gate: 237/237 PASS,
2039 registered. Exact committed full prelaunch/build/package and fresh-Steam
smoke11/survey15 with lease-first actual-snapshot restoration all passed.

This is bounded donor research, not a new hybrid framework or a gameplay gate.
Equipment and structural metadata cannot by themselves qualify a rig, native
clip, two-palm grip, spear/tail contact, movement or cleanup. Preserve all prior
failed body/census attempts. Sprints14–16 remain complete; 976+29=1005 visible.
HumanReview: NOT_PERFORMED_NONBLOCKING.
