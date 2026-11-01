# Sprint 36 — RELEASED v0.0.174; STOP before Sprint 37

[Stable release](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.174) ·
[Product PR #88](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/88) ·
[Public manifest](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/download/v0.0.174/release-manifest.json)

Sprint 36 is complete. Babau and Hezrou are released through seven untemplated
Summon Monster routes. Sprint 35 remains closed. No Nature's Ally entries,
template children, Vrock, Sprint 37, DATA, Mounted Combat, Buff Planner,
permanent deployment or historical release replacement was included.

- Tested/promoted source: `fc394d6281f9d70aa81c03d02df01f95a8927454`
- Source fingerprint: `6c95541a43ec7eb9d8e562af40d3bd11c687478a7911f07a0dc42f427a707198`
- Product merge: `bbe19b55e985b18823695c6d9fa269f0dcb9407c`
- Reviewed base, preserved as first merge parent: `e58bb50ab6a835cad266ca2abe95efddba911058`
- Annotated tag object/target: `f72e8c39badc1eebb0409256d7bec51a6cb2823a` → product merge
- Public ZIP SHA-256: `b7211829a85ca2fc372970fd311f8f5f66606564d5c34afa08fb0bb7f2a10304`
- DLL SHA-256: `7599b7f4088ec828b50c37db269dbab95d195b45eb110dfc4abc50c831002173`
- DLL MVID: `796c99df-951b-42d0-b99c-25dd1d4fe338`
- Public manifest SHA-256: `64bdf22b7f873e5c285ecae4671b4a515e97df50bb88c4682fe5f21cee7654ec`
- Public checksums SHA-256: `fc406f5b5a6c855f5c0fbd5f5915d6656b8be39d7144852905533eb60d8f7aff`

The release is stable, non-draft, non-prerelease and Latest. The actual public
ZIP, manifest and checksum were downloaded from GitHub. The 446-entry ZIP
contains the expected Info, one DLL, Babau/Hezrou icons and both original-body
asset sets. Downloaded ZIP/DLL/MVID, release asset digests, checksum line,
annotated tag, PR parents, master head and public manifest all match the exact
qualified identities above. Publication rebuilt nothing.

## Delivered scope

| Creature | Public progression | Delivered behavior |
|---|---|---|
| Babau | SM V one; VI 1d3; VII–IX 1d4+1 | Distinct natural claw/claw/bite and nontransferable-longspear iterative/bite modes; 5-foot natural and 10-foot spear reach; native sneak qualification; printed defenses/feats; constant See Invisibility; useful at-will CL 7 Dispel Magic with native checks and attack fallback; exact-owner Protective Slime against natural/unarmed attackers. |
| Hezrou | SM VIII one; IX 1d3 | 10-foot bite/claw/claw routine; rider-free Grab and maintain path; Power Attack and private Cleave/Great Cleave; ally-inclusive source-owned stench sickness; separate held-victim nausea cadence/recovery; supported CL 13 Chaos Hammer, Unholy Blight and one-use Blasphemy with alignment/SR/save/collateral/resource behavior and melee fallback. |

Both creatures have distinct original rigged bodies, real painted 128px icons,
functional native selection/movement/contacts and exact private ownership.
Existing summon identities, wrappers, donors and ordering remain unchanged.

Final source-derived public surface:

| Surface | Count |
|---|---:|
| Unique project creatures | 130 |
| Monster entries / placements | 108 / 594 |
| Nature's Ally entries / placements | 105 / 588 |
| Generated placements | 1,182 |
| Retained wrappers | 29 |
| Visible choices | 1,211 |

## Final requirements map

Printed PF1 behavior, installed Kingmaker 2.1.7b behavior and owner-authorized
adaptations remain distinct.

| required behavior | existing implementation/native consumer | actual gap | smallest patch | selected proof |
|---|---|---|---|---|
| Two creatures and exactly seven Monster-only roots | Append-only manifest, visibility filter and native summon graphs | Babau/Hezrou identities and dependencies were absent | Allocate once, register dependency-complete, expose only Babau V–IX and Hezrou VIII–IX | Source-derived roster plus final-byte public run of all seven parents/children, quantities, costs, CL20 durations and cleanup |
| Babau modes, reach, sneak, Slime and useful Dispel | S34 weapon modes/reach/equipment ownership; S35 contact attribution; native sneak and Dispel | Donor constants and Barbed Defense semantics do not apply | Exact Babau weapons/mode policy; natural/unarmed-only Slime; CL7 native Dispel scorer/cooldown | Core run `20261101T1551511295635Z-cd0b366196794cd1bad3f9a9749fc9ad` and combat run `20261101T1801020940289Z-44baad15544a44f6af6a116191e9aa23` on unchanged named production consumers |
| Hezrou Grab/Cleave, distinct stench and held nausea, supported SLAs | Released Grab/maintain, private Cleave, S35 area/source lifetime and alignment spell consumers | Hezrou owner/profile, separate condition semantics and caller parameters were absent | Exact-owner Hezrou carrier without Impale/constrict/swallow; source-specific sickness/nausea; CL13 spell parameters/resources | Core run above; RTWP/TB run `20261101T1646550891510Z-ed9bf6b858ee44fca4fb3c4e537e4e00`; corrected combat run above |
| Distinct bodies/icons and all public routes | Original measured rigs, attachment lifecycle and icon manifest | No installed Babau/Hezrou body exists | Two distinct original bodies and two cataloged real icons | Final-byte public run `20261101T2233317800754Z-121cf76d753f45f0b28329f081c45c02` |
| Mixed ON/OFF persistence and highest-risk installed integration | Supported three-process Working envelope and installed-stack restoration | New owned state, finite use and body reconstruction needed coverage | Extend only Sprint 36 persistence readers/dispatch | Six fresh final-candidate processes; exact Working/catalog and installation restoration |
| One stable exact-byte release | Full Sprint gate, reproducibility and guarded publisher | Sprint 36 binding/records absent | Named selector, strict preflight, exact retained ZIP promotion | 2,785/2,785 gate, identical repro, 22 publisher refusals, preflight PASS, public download and lineage verification |

## Explicit adaptations and limitations

On 2026-11-01 the owner authorized exactly three omissions, disclosed in the
release notes and implemented without substitutes:

- Protective Slime does not damage manufactured weapons or ammunition because
  Kingmaker 2.1.7b exposes no faithful item-instance hardness/HP/broken/repair
  consumer. Manufactured attacks do not damage the wielder instead.
- Babau omits Darkness. Constant See Invisibility remains.
- Hezrou omits Gaseous Form.

Self-teleportation, planar travel and nested summoning remain excluded by the
Summon Monster contract. No other required mechanic is omitted. No owner
aesthetic approval is claimed; fine texture/anatomy/choreography refinement is
nonblocking visual backlog.

## Qualification and attribution

The complete source/build/package boundary passed **2,785/2,785** tests in
503.5 seconds. Supported exact-reference reproducibility produced the identical
ZIP, DLL and MVID while preserving the primary artifact.

| Mandatory row | Verdict | Exact execution |
|---|---|---|
| Profiles and registration | PASS, final candidate | ON prepare `20261101T2148388134047Z-5d42592daa844a8d89d88120d79d9713` |
| Babau combat/acid and core Hezrou source behavior | REUSED_PASS, unchanged production dependencies | Core `20261101T1551511295635Z-cd0b366196794cd1bad3f9a9749fc9ad` (117.6s) |
| Hezrou Cleave/control in RTWP and turn-based | REUSED_PASS, post-correction unchanged production dependencies | `20261101T1646550891510Z-ed9bf6b858ee44fca4fb3c4e537e4e00` (166.8s) |
| Both modes, ordinary Grab, SLA collateral/resources/fallback and teardown | REUSED_PASS, corrected unchanged production dependencies | `20261101T1801020940289Z-44baad15544a44f6af6a116191e9aa23` (157.7s) |
| Presentation and all seven public routes | PASS, final candidate | `20261101T2233317800754Z-121cf76d753f45f0b28329f081c45c02` (93.0s) |
| Mixed ON persistence | PASS, three fresh final-candidate processes | `20261101T2148388134047Z-5d42592daa844a8d89d88120d79d9713`; `20261101T2155330379926Z-be0d9e2d3e3443aea92593d430816176`; `20261101T2202188747625Z-b36421989dc042da9b4e1bd7a320ab64` |
| Mixed OFF persistence | PASS, three fresh final-candidate processes | `20261101T2210090886768Z-b2b4479479c740abb4a709b56629932e`; `20261101T2217003504060Z-d84187f866c54bfab5a8906c81b607e5`; `20261101T2223445470812Z-16b7a178ad314d99bceea950e4808d67` |
| Highest-risk installed integration | PASS, final candidate | Combined with ON prepare above |
| Complete gate and exact reproducibility | PASS | Source `fc394d6281f9d70aa81c03d02df01f95a8927454`; ZIP `b7211829a85ca2fc372970fd311f8f5f66606564d5c34afa08fb0bb7f2a10304` |

The six fresh persistence processes consumed 1,236.0 seconds of native result
time; the final public process consumed 93.0 seconds. Earlier failed raw results
remain under their original runtime-evidence identities. They were classified
as product, fixture, reader, environment or unmet-precondition findings; no
unchanged failure was relabeled or deleted. The final product change after the
last mechanics proofs affected only explicitly requalified public/persistence
and reporting/observer boundaries; reused rows are limited by exact Git diffs
over their named production paths.

## Publication, restoration and stop condition

The guarded publisher passed 22 refusal tests and a real preflight before PR
#88 merged. Two publication attempts safely refused before tagging because the
external ledger lacked publisher-only reporting metadata; those local promotion
directories are retained. The successful attempt created the tag and one stable
GitHub release from the exact retained qualified ZIP.

Post-download restoration verification proves:

- the actual 968-entry human Mods installation exactly matches the preserved
  pre-mutation Sprint 36 intake;
- all 118 protected saves and metadata match intake;
- `KMG_AUTOMATION_WORKING` SHA-256 is
  `650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`;
- immutable baseline SHA-256 is
  `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`;
- Kingmaker is exited, the runtime lease is absent and the product worktree is clean.

The exact machine-local proof map is recorded in
[SPRINT36-RELEASE-EVIDENCE.json](SPRINT36-RELEASE-EVIDENCE.json). Product and
release work are complete. This records-only follow-up performs no rebuild or
native campaign. **STOP before Sprint 37.**
