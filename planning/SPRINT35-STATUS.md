# Sprint 35 status — Barbed Devil and Bone Devil

Last updated: 2026-10-30. Active product branch:
`codex/expanded-summoning-sprint35-barbed-bone-devils` at reviewed master
`aee691127b9ade4bbfde55247d54652acf85957c`.

Sprint 34 is closed at v0.0.172. Its accepted creatures, assets, adaptations,
campaigns, and artifacts are inputs only. This file is the single current
Sprint 35 checkpoint; historical records are not current instructions.

## Compact requirement-to-proof map

| Required outcome | Existing implementation / evidence | Demonstrated gap | Smallest patch | Selected proof |
|---|---|---|---|---|
| Append-only registration of Barbed Devil and Bone Devil, initially hidden | Current catalog has 126 creatures, Monster 104 entries / 582 placements, Nature 105 / 588; the identity ledger and hidden-publication catalog are append-only | Neither creature, their dependencies, nor their five routes exists | Allocate the exact unit/weapon/ability/effect/resource/route identities once; register both roots with only their five Monster routes suppressed | Focused ledger/catalog tests, hidden bootstrap, and dependency/type-resolution inventory |
| Five untemplated Monster routes only | Existing generator supplies one, 1d3, and 1d4+1 placement semantics without reordering old entries | Barbed VIII/IX and Bone VII/VIII/IX are absent | Append Barbed VIII one + IX 1d3 and Bone VII one + VIII 1d3 + IX 1d4+1 | One grouped public-route run records each command, actor, quantity, tier, duration, ownership, and cleanup; final derived totals must be 128 creatures, Monster 106 / 587, Nature 105 / 588, 1,175 generated placements + 29 wrappers = 1,204 visible choices |
| Barbed printed combat profile and defenses | Sprint 34 profile, natural-weapon, feat, DR/SR, reach, action/resource, and ordinary Cleave machinery are reusable by contract | Existing Infernal34 traits encode DR 5/good-or-silver and the released creatures' profiles; no Barbed exact weapons/profile | Add a Barbed-specific profile using shared helpers only where exact: Medium, two primary claws, DR 10/good, SR 22, printed saves/abilities/feats | Focused profile calculation plus native inventory/attack/modifier/defense observations in both combat modes |
| Barbed retaliation, claw fear, grab, and impale remain four distinct consumers | Exact-weapon reach policy; source/target-owned grapple link store; natural 1/20, size, immunity, limb, target, reload-reset, and maintain contracts already exist | No incoming-attack retaliation, claw-only fear, Medium-cap grab, or exclusive impale rider; recursion and independent-owner isolation are unproved | Add exact-event/exact-owner predicates. Retaliation keys the eligible incoming attack and rejects generated damage; claw fear keys a wounding outgoing claw; grab reuses the existing link; a successful later maintain uses impale instead of replaying claw/default grapple damage | Native normal-melee positive, real reach-weapon negative, self-generated-damage nonrecursion, two-owner isolation, claw/nonclaw fear, initial grab, later impale, failed/immune/oversize negatives, and reload-safe release |
| Barbed legal SLAs with real costs, including exactly two Scorching Ray rays | Existing native-backed spell cloning, ray targeting, ability resources, and AI action ownership | Exact Barbed list/resources are not wired; nested summoning and travel must remain absent | Reuse supported native actions; specialize Scorching Ray to two rays at CL 12; give daily uses per summoning and at-will buttons normal action costs; omit forbidden teleport and summoning | Native command/action/resource deltas, two attributed ray attacks, selected spell outcomes, AI useful/futile controls, fresh-load resource preservation |
| Bone printed profile, four equal-role natural attacks, defenses, and effective flight representation | Existing outsider profiles and ordinary-map Airborne representation | No Large 10-foot-reach Bone profile; donor-edition secondary penalties would be wrong | Add Large bite/claw/claw/sting at printed equal attack bonus, DR 10/good, SR 20, speed 40 with the established Airborne single-speed representation for fly 60 | Focused attack math and native full-attack limb/bonus/damage/contact inventory; movement/reach observation and representation disclosure |
| Bone sting poison is injury-gated and independently timed | Existing native-backed injury poison graph and wound gate support exact weapon, positive final damage, finite exposures, stat damage, and cure saves | No sting carrier with Fortitude DC 20, 1d3 Strength, once/round for 6 rounds, cure 2 consecutive saves | Clone the proven graph deeply for the exact sting and initialize all dice/context operands; do not attach summon-source cleanup to inflicted poison or erase ability damage | Miss/nonwound/other-limb negatives; sting wound positive; native DC, cadence, Strength damage, finite course, two-save cure, source expiry, dispel distinction, save/load |
| Bone fear is a 5-foot aura with native eligibility and source memory | Existing source-attributed fear terminal buffs and area membership/lifetime helpers; Kyton gaze is explicitly inapplicable | No continuous aura consumer or repeat-save contract for Bone | Build a source-owned 5-foot aura applying native frightened for 1d6 rounds on failed DC 19 Will save with source-specific repeat handling; preserve native immunity/eligibility and do not make the area enemy-only | Actual membership enter/stay/leave, immunity, success/failure and no frame-by-frame retries, two-source isolation, source cleanup, both combat modes |
| Bone regular and quickened self-invisibility stay distinct | Existing native ordinary attack-breaking invisibility, self targeting, resource, and action builders | No standard at-will action and no separate swift-action three-use resource; toggle/load-reset risks unproved | Create two self-only commands sharing the ordinary invisibility effect: standard at-will and swift 3/summoning; suppress recast while active in AI only | Native action economy, attack breaks effect, active-effect AI negative, standard/quickened distinction, 3-to-0 spend, no reset by mode/toggle/load |
| Bone offensive control includes Dimensional Anchor and faithful Wall of Ice | Native spell cloning and source-owned area lifetimes exist. Installed 2.1.7b reflection shows `AreaEffectsController.Spawn`, `AreaEffectView.m_NavmeshCut`, standing-unit corpulence obstacles, and `ObstaclePathfinder`; Assembly-CSharp SHA-256 is `3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB` | No retained production graph proves selectable wall geometry, temporary navigation obstruction, attackable HP/breach, residual cold, or restored navigation; finding visual ice alone is insufficient | First prototype a private, source-owned planar wall from temporary sections: area views carry real Navmesh cuts; paired attackable section entities carry HP/destruction and spawn residual cold on breach. Reject this route if the early native run does not prove real obstruction and restoration | One hidden native cast before AI expansion: legal placement, formation save/SR case, movement/targeting interaction distinguished, section damage/breach, plane residual damage, source expiry/removal, and restored navigation |
| Two recognizable creature-specific bodies and two real icons | Sprint 34 measured rig, mesh/albedo loader, receipts, and icon exporter are reusable; original Sprint 34 bodies remain protected | No Barbed/Bone assets exist | Add new original paintings/provenance and new Sprint 35 geometry: a spiny Barbed silhouette and a skeletal/insectile Bone silhouette with a visible segmented stinging tail; reuse rig/clips without retaining an unrelated visible body | Export/hash validation; native selection/movement/attack/contact/visibility/fallback captures inspected; exact private objects separated from immutable caches |
| Ownership, cleanup, disabled-mode safety, and persistence | `InfernalEffectLifetime34`, `InfernalSourceLifetime34`, `OutsiderLifetimeBoundary`, source-specific helpers, owned-item policies, and accepted grapple reload reset | New aura/wall/resources/markers/private view objects have no identities or termination policy | Give every active grant/area/wall/private object an exact owner and idempotent cleanup. Keep poison and applied ability damage independently timed. Register identities while disabled without allocating views/resources or starting mechanics | Source death/expiry/disable, destroyed-actor cleanup, no orphan wall/aura/grapple, disabled-module load, mixed ON and OFF fresh-process persistence envelopes (six processes total) |
| Useful native AI without changing player-visible rules | Existing exact-owner AI/range/action/resource observers and established admission/scoring patterns | No creature actions or wall placement scoring | Add small creature-specific actions; wall admission requires legal useful placement, a viable allied route, and no redundant wall; do not change collision/victim rules or add an arbitrary wall limit | RTWP and turn-based substantive outcomes, unsafe/futile/redundant negatives, fallback to attacks, exact owner attribution |
| One coherent final candidate, release, public verification, and exact restoration | Current validation policy and Sprint34 qualified release tooling define focused gates, exact candidate identity, promotion, refusal, and rollback | No Sprint 35 selector/reports or release inputs; v0.0.173 availability must be rechecked at integration | Add the smallest Sprint 35 selector/validator and exact two-branch publisher allowlist entries only if required; do not copy a release system | Focused development gates, independent review, one final source/build/package boundary, grouped mandatory native proof, product PR/merge, next-free stable release from exact ZIP, public download/manifest/hash/MVID lineage, intake restoration, separate records PR/merge |

## Current checkpoint

- Intake/master and the sparse product worktree are pinned. The append-only
  allocator has registered the two unit roots, their dependency-complete
  private graphs, and all five routes: foundation 2,637; active identities
  7,655; reserved 3; total 7,658. The catalog derives 128 creatures, Monster
  106 / 587, Nature 105 / 588, and 1,175 generated placements plus 29 wrappers.
  Only the five Sprint 35 placements remain suppressed.
- PF1 rules have been frozen separately in `planning/SPRINT35-RULES.md`.
- The two original icon paintings and prompt/provenance records are checked in
  under the established medallion family. Exported icon SHA-256 values are
  `15ef2ae55a1037359f63fcf9d9086bd5399d81a3ae0bbe0061bda891be89da34`
  (Barbed) and
  `d542bd9c5ffc52615c786cccafb5b3169ab3b2298599c6a895803e223c01d612`
  (Bone). The sparse-safe exporter refreshed both post-allocation consumer
  receipts and retained strict full-export verification.
- The hidden production candidate compiles against the installed Kingmaker
  2.1.7b references. The focused `expanded-summoning.sprint35-*` domain group
  passes 4/4, covering route suppression, predicate separation, live modifiers,
  and exact sting injury admission. Native behavior is not inferred from this.
- Wall of Ice has a measured installed candidate path, not yet a PASS. The first
  native viability run will instantiate the hidden unit directly (without menu
  publication) and is the admission gate for expanding its AI/persistence.
- The required laptop intake is captured before staging: 968 current Mod-tree
  entries, 118 protected-save entries, 83 registry settings, unchanged Working
  SHA-256 `650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`,
  and sound-bank SHA-256
  `0e9f88c562f4f937a8941ace0f241bb31a7ed56b46fbca549c98f764392edf18`.
  The automatically discovered August private-reference bundle was incomplete
  and was not substituted silently. A fresh private 2.1.7b bundle on `E:`
  contains all 16 required files and matches every installed assembly; the old
  bundle is untouched. The first strict development-package attempt correctly
  refused dirty source. Commit `ca21defc` then passed the Sprint 35 development
  validator, the selected Python groups, and both protected legacy-icon gates;
  its package attempt stopped before compilation when the sparse worktree lacked
  hash-pinned catalog inputs. Those exact read-only inputs were materialized,
  and the current registry/delegated-manifest hashes are now bound in the icon
  catalog. The resulting `a8d9ecd9` development ZIP passed strict package
  validation (ZIP SHA-256
  `acba82e56db4710612d2804bf3842663fae02d91edf99b3626f9dfc792f8463f`;
  DLL SHA-256
  `3656cc61cca85e7c6b89c378ca6d65e56c6eed30f16bf5ee4c8abb935a4490b3`)
  but has not been staged. Pre-dispatch review found and repaired two host-only
  admission gaps: the PowerShell phase parser omitted declared Sprint 35 phases,
  and the restoration wrapper could not predeploy/reuse an explicit development
  candidate. The bounded orchestration suite now passes 267 assertions and the
  focused Sprint 35 domain group remains 4/4.
- Commit `dc2785da` produced the first source-bound native candidate (ZIP
  `f308bf9de78abb234794d30a395e31621c88936de57581b384bf7efd2cec9033`,
  DLL `48396a276a7465531f12251dbd0305d66c6b718789dc391813960dff6a576df6`,
  MVID `20e722ee-ec32-4fa8-bd63-8c2ae1a98f84`). The guarded early run
  `20261030T1629300029849Z-36d380d435214b86bcabdef80f9ad7b5` failed before
  assertions because registration rejected the one new planned shell type,
  `InfernalSpellAi35`. It staged no save, exited normally, and restoration
  returned the live mod tree to its exact pre-run SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`.
  The failed candidate is retained byte-for-byte on `E:` with verified archive
  manifest SHA-256
  `0d0dba0f8ef60bc6da94dec16362cc980adaf597543d2a2cec693e9ca7dcc0db`.
  The smallest repair adds that exact shell factory; production compilation,
  the four Sprint 35 policy tests, and the existing all-planned-types factory
  regression now pass 5/5. A changed source-bound candidate is required before
  the next native launch; the failed bytes will not be relaunched.
- Mandatory qualification, publication, release, verification, and restoration
  are all pending.
