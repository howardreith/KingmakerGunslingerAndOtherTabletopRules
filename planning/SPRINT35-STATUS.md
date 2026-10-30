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
| Barbed legal SLAs with real costs, including exactly two Scorching Ray rays | Hold Person and exact two-ray Scorching Ray use installed spell graphs. Unholy Blight now reuses the qualified all-creature alignment-burst consumer with live DC 18, one per-summoning resource, independent 1d4 sickness, and ally-safe AI admission without changing victim eligibility | Installed census has no named Major Image, Produce Flame, Pyrotechnics, Order's Wrath, or Unholy Blight graph. Major Image needs a broad illusion consumer; Produce Flame needs its touch/thrown-flame contract; the existing project Pyrotechnics adaptation is not faithful; Order's Wrath geometry remains unproved | Keep teleport/summoning precisely absent. Retain the bounded Unholy implementation; do not add the four unsupported buttons until their exact consumer exists or the owner accepts a specific bounded adaptation | Focused Unholy policy tests plus native command/action/resource, good-outsider/good/neutral/evil outcomes, SR/save/sickness/source attribution, ally-safety/futile controls, two attributed ray attacks, and fresh-load resource preservation |
| Bone printed profile, four equal-role natural attacks, defenses, and effective flight representation | Existing outsider profiles and ordinary-map Airborne representation | No Large 10-foot-reach Bone profile; donor-edition secondary penalties would be wrong | Add Large bite/claw/claw/sting at printed equal attack bonus, DR 10/good, SR 20, speed 40 with the established Airborne single-speed representation for fly 60 | Focused attack math and native full-attack limb/bonus/damage/contact inventory; movement/reach observation and representation disclosure |
| Bone sting poison is injury-gated and independently timed | Existing native-backed injury poison graph and wound gate support exact weapon, positive final damage, finite exposures, stat damage, and cure saves | No sting carrier with Fortitude DC 20, 1d3 Strength, once/round for 6 rounds, cure 2 consecutive saves | Clone the proven graph deeply for the exact sting and initialize all dice/context operands; do not attach summon-source cleanup to inflicted poison or erase ability damage | Miss/nonwound/other-limb negatives; sting wound positive; native DC, cadence, Strength damage, finite course, two-save cure, source expiry, dispel distinction, save/load |
| Bone fear is a 5-foot aura with native eligibility and source memory | Existing source-attributed fear terminal buffs and area membership/lifetime helpers; Kyton gaze is explicitly inapplicable | No continuous aura consumer or repeat-save contract for Bone | Build a source-owned 5-foot aura applying native frightened for 1d6 rounds on failed DC 19 Will save with source-specific repeat handling; preserve native immunity/eligibility and do not make the area enemy-only | Actual membership enter/stay/leave, immunity, success/failure and no frame-by-frame retries, two-source isolation, source cleanup, both combat modes |
| Bone regular and quickened self-invisibility stay distinct | Existing native ordinary attack-breaking invisibility, self targeting, resource, and action builders | No standard at-will action and no separate swift-action three-use resource; toggle/load-reset risks unproved | Create two self-only commands sharing the ordinary invisibility effect: standard at-will and swift 3/summoning; suppress recast while active in AI only | Native action economy, attack breaks effect, active-effect AI negative, standard/quickened distinction, 3-to-0 spend, no reset by mode/toggle/load |
| Bone offensive control includes Dimensional Anchor and faithful Wall of Ice | Source-owned area lifetimes exist. Installed 2.1.7b has dynamic `NavmeshCut`; `5623824c` proved the exact cut is registered and reloaded by the active installed `TileHandlerHelper` | The native census exposes no named Dimensional Anchor graph or teleport-blocking condition. For Wall, the helper updates `RecastGraph`, but the actual six-metre movement path uses the separate `GridGraph`; 16 samples retained the identical two-point `PathClear` route. Selectable geometry, formation save/SR, breach/residual cold, and LoS/targeting also remain open | Owner decision required for a wall-specific GridGraph update/restore driver (or a precisely bounded alternative) and, separately, any new Dimensional Anchor blocking consumer. Do not create general navigation/teleport frameworks or silently ship inert substitutes | After a Wall decision, first prove one hidden native cast changes actual movement and removal restores it; only then expand legal placement, formation save/SR, breach/residual damage, source expiry, AI, and persistence. Anchor requires a real blocked travel attempt before admission |
| Two recognizable creature-specific bodies and two real icons | Sprint 34 measured rig, mesh/albedo loader, receipts, and icon exporter are reusable; original Sprint 34 bodies remain protected. Sprint 35 now has hashed original Barbed and Bone exports plus two existing real icons | Native attachment, movement, selection, sting contact, fallback, and cleanup are not yet observed on the new bodies | Reuse the accepted per-view swap/receipt path under exact Sprint 35 identities; Bone alone receives a tail-region deformation driven by its exact native sting event | Export/hash validation passes; the early native observer now records distinct caches/views and real sting contact. Final selection/movement/visibility/fallback captures and owned-resource cleanup remain required |
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
- Two original model families are now authored without changing the closed
  Sprint 34 assets. Barbed's broad red horned body has twenty authored rigid
  thorns (mesh SHA-256
  `890456e59f973eb0383197a9eed4ee50628b5314114f315acb15321326e63a28`);
  Bone's narrow jointed body has two wing-strut silhouettes and one isolated
  six-segment stinging tail (mesh SHA-256
  `b94c51a20680a5f4d747f3c8acc16e417a96278cee8b01c3398e5225792a4c40`).
  Their albedos are `2614201dd25653c2b86f719ce48bab479ecf8151103a068ae468335ef3a3ac8d`
  and `98cea77b72fa5540df7127f29444110dded90233d951fd04b6d66ac28f053c77`.
  Agent inspection finds the static previews distinct and recognizable; it is
  not native presentation proof or owner aesthetic approval. The accepted
  measured-rig loader now has exact Sprint 35 identities, disabled-module
  refusal, per-view resources, rollback, and release cleanup. Bone alone adds
  a visual-only native-sting tail deformation; it cannot create a command,
  reach, hit, or damage. The early observer records both distinct attached
  sources and the actual sting contact gap before admitting presentation.
  The generated weapon/provenance audit and closed build/package inventories
  include the four exact new exports. Offline body decoding, albedo hashes,
  winding, tail vertex isolation, exact identities, production compilation,
  and the selected Sprint 35/correction group pass 7/7. Two preceding Focused
  attempts stopped before a test verdict on the legacy production and test
  project source lists respectively; each exact missing include was repaired.
  A changed source-bound native candidate is still required.
- The hidden production candidate compiles against the installed Kingmaker
  2.1.7b references. The focused `expanded-summoning.sprint35-*` domain group
  plus the selected correction control passes 7/7, covering route suppression,
  predicate separation, live modifiers, exact sting injury admission, the
  private wall donor, and original-body/tail export contracts. Native behavior
  is not inferred from this.
- Wall of Ice is stopped at a measured gameplay-adaptation decision. Hidden
  native runs instantiated the unit directly without menu publication, proved
  the section/cut/lifetime path, and then proved that the installed dynamic-cut
  consumer cannot obstruct the graph actually used by unit movement. No AI or
  persistence expansion is admitted until the owner selects the bounded route
  recorded in the requirement map.
- Independent SLA work continued without changing the failed wall candidate.
  The latest installed spell census contains no named graph for Major Image,
  Produce Flame, Pyrotechnics, Order's Wrath, Unholy Blight, Dimensional
  Anchor, or Wall of Ice. Unholy Blight nevertheless has an exact bounded path
  through the already-qualified Astral alignment-burst consumer: the new
  implementation keeps all-creature targeting and per-target SR, uses a real
  Will save and live Charisma DC, distinguishes good outsiders/good/neutral/
  evil, applies an independently timed native Sickened effect, spends one
  per-summoning resource, and changes only AI admission to avoid non-evil
  allies. Production compilation and the selected Sprint 35/correction group
  pass 8/8. The remaining named gaps are recorded in the rules contract and
  have not been replaced by generic spell buttons.
- A separate `sprint35-infernal-barbed` phase now observes the bounded Unholy
  consumer without entering the known Wall failure. It records one real
  standard-action command, exact 1-to-0 resource debit, per-target SR/save/
  damage and duration rolls, good-outsider/good/neutral/evil outcomes,
  independent Sickened contexts, source-loss lifetime, and owned cleanup. The
  observer distinguishes the damage roll from the separate 1d4 duration roll
  and snapshots the shared parameter sentinel before casting. Production
  compilation and the selected 8/8 group pass; exact phase admission/refusal
  passes the 269-assertion orchestration suite and 1,170-check runtime
  preflight suite. Commit `975b0827` produced the immutable development ZIP
  `eda7c8d10abdcb549ef801d0656dee3dcedeedb3a689ce6e128d0081b219b2d9`
  (DLL
  `d4af5b8132b1b41ccd67ddfa7763ac7009a6ab32c5c9055fa515ad78cdac974a`,
  MVID `d8b79a5b-43e1-4699-88dd-3efe71c61dd0`, source state
  `b6eea1e534c087bdd6c1edca4c32c4c601903a4f46847c28d3c775d2eef6d5a7`).
  Its 162-file exact build is preserved on `E:` under manifest
  `69D7AE654F927BBC761279416843A0BC3BEB0B7A724382CBEDFED96377DF42FE`.
  One preflight correctly stopped before deployment because reclaimed build
  scratch included the package manifest validator; the selected validator was
  rebuilt without changing the package. A second attempt deployed and restored
  without launch because the phase name was passed as the scenario rather than
  as the already-supported Sprint32 `reviewPhase`; its no-launch record is
  preserved under manifest
  `3C74E85B16A02003393BC621222BB7DDB8ADC4C17141FB3610F04E6716684DB3`.
  Correctly dispatched run
  `20261030T2122272270728Z-c1bd26453e3b4009b6caac22905ecc01`
  proved the Barbed body itself settled, then failed before casting because the
  observer also required its mechanics-only Astral Deva SR control to have a
  renderer. The accepted Holy Smite observer does not make control rendering a
  spell precondition. The bounded correction retains native appearance for the
  new Barbed owner only; production gameplay is unchanged. Raw FAIL, both
  snapshots, deployment and exact restoration are preserved under manifest
  `46CA2987A54DF82FF3F2EAF5FC572FEB11E875036587DD7AEAF2DC53BC79C807`.
  The live tree was restored each time to exact SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`;
  no save, process, lease or compatibility lock remains.
- Commit `25ba4c56` produced the corrected development ZIP
  `842e38a6be3b1892a6e2137e4844bd420089ab37b3f18dabd05e21c9dd4bb7e5`
  (DLL
  `30af0aac80f53c3765ce3cd0f2bef02575694df58bc3a0d9e8d6cc8209175582`,
  MVID `ef684007-6e09-4daf-a0c1-e88ba1e87de9`, source state
  `4236daf2e3c6e36e0f5c549ff1a4b4c68d8c0b95d5f9083abb0a593d3ecd27fa`);
  its exact build is preserved under manifest
  `D23629A52827FEF714201BC9948F62FC88E3F8069287A4078B5F31341E0A5C18`.
  Guarded run
  `20261030T2136055112917Z-813c475c4c42414d90548720ccd35027`
  passed the real mapping, isolated footprint, standard-action command,
  one-to-zero resource debit, per-target SR/save/damage/duration, good ordinary
  5d8, neutral quarter damage, evil exclusions, native Sickened attribution,
  shared-parameter isolation, source-loss survival, and natural expiry cells.
  Cleanup then sampled one source-owned area one frame after all four actors
  were destroyed. Accepted Astral observers wait for owned-area retirement;
  this new observer had waited only for actors. The bounded correction now
  waits up to five seconds for both actors and areas, records immediate/final
  area counts, and still fails a persistent orphan. The raw FAIL and exact
  restoration are preserved under manifest
  `CBAA98D7F0BC3280D08B952E591C6DCE265A0CE19276B7CBA9DC585A6E9A6379`;
  the live tree again restored to exact intake and no save was selected.
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
- Commit `98446fcb` produced the corrected hidden candidate (ZIP
  `2a0ed7ba7283f82c2894eca0eca463a8b5f3ad15d5d5896abd702814d323105d`;
  DLL `0bca56e6165d191c0d703ca58b5f8766210991550b4da56adaa405eff5842342`;
  MVID `e8d35dee-ab2d-43f4-adfb-369dae3262bc`). Guarded run
  `20261030T1718409168790Z-de58fa3ea0c84d848fa8d15dcb5bfa1a` again failed
  before assertions, now after the new AI shell was admitted: registration
  indexed every `BlueprintUnit` through the public creature-root donor map,
  but the private Wall of Ice section is intentionally not a creature root.
  The exact `KeyNotFoundException` is retained in the native logs; no save was
  staged, and restoration again returned the live mod tree to pre-run SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`.
  The bounded repair declares only that private section's shell donor as the
  Bone Devil donor already loaded by the catalog; unknown private unit shells
  now fail with an explicit diagnostic. A focused policy regression covers
  the positive mapping and public/unknown/null negatives. The first focused
  compile attempt stopped on C-drive headroom before a test verdict; the exact
  failed 984 candidate was copied to `E:` and verified as 168 files before its
  C-drive copy and regenerated build intermediates were removed (archive
  manifest SHA-256
  `81b9aa5b44bad3b1386043963fee732b68abf04b749ff96f40d724f6ea1b7951`).
  With verified headroom restored, production compilation and the selected
  Sprint 35/planned-type regression group pass 6/6. The unchanged failed
  candidate will not be relaunched; a new source-bound candidate is required.
- Candidate `fca44a54` passed strict packaging (ZIP
  `842374e4270f3172e41a919ee7bef12163f1b7a81cc23967d6544bdd63f7c750`;
  DLL `cc86261c49ad279df4667acfde7d3b0aa8b8b495c3dd2cd9dbdec92c25e08ef7`;
  MVID `e4608d19-a585-4f5e-919c-522c4320f40f`). An initial wrapper invocation
  refused before deployment because its generated manifest-validator binary
  had been removed with build intermediates; it created and exactly restored
  only the outer snapshot. After rebuilding that selected validator, guarded
  run `20261030T1743495604303Z-4232af98f3484ef18a9d9517225b3dd4`
  registered all Sprint 35 identities, including the private wall section,
  then failed before assertions on `BlueprintItemWeapon.m_CriticalRollEdge`,
  a field that does not exist on that installed type. Rollback removed 3,007
  owned registrations and the wrapper restored live SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`.
  The incorrect mutation also duplicated Improved Critical: the private claw
  type must keep its native base edge 20 while the granted feat supplies the
  printed live 19-20 range. The failed candidate is preserved on `E:` as 168
  verified files; archive manifest SHA-256
  `c2c4af68f121909156d3091b905f8320b4f0240aacde1c83f95004bd3d4cb5e4`.
- Mandatory qualification, publication, release, verification, and restoration
  are all pending.
- Development build `8958f247` passed repository validation, the selected 7/7
  domain group, exact-reference compilation, and build-output validation, then
  failed closed before a package was produced: its deterministic package
  inventory still counted the closed Sprint 34 set and expected 436 files,
  while the four declared Sprint 35 exports correctly staged 440. The bounded
  repair adds only `$infernal35Files.Count` to that existing formula. This
  incomplete attempt is not a candidate and will not be deployed or relaunched.
- Candidate `6d673532` passed the same selected 7/7 group, exact-reference
  compilation, strict 440-file packaging, and package validation (ZIP
  `ce92a2574abc62f8f84b2f0b4527c431f0262c45be5a5982ed574d0477e9e58a`;
  DLL `193ee6b79aca8d0f510bbcaa9ed8b1f7d713b3053d050f47486d38ed6df39959`;
  MVID `5a9dff4f-a775-4d94-af7b-1121976a4a4e`). Guarded run
  `20261030T1816329347148Z-90186ea4257e4812baee633cce231997`
  failed before assertions after registering the Sprint 35 identities:
  `ConfigureBarbedScorchingRay35` attempted to register the same description
  localization key a second time with different text. Blueprint rollback
  removed all 3,007 owned registrations, no save was staged, and the wrapper
  restored the live mod tree to exact pre-run SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`.
  The candidate is retained on `E:` as 168 hash-matched files with archive
  manifest SHA-256
  `ae801332431ed9e252cb5a99d8ed8505f22e8278c65ad14ffbfa43c05bdd7497`.
  The repair makes the shared owned-spell builder accept the specialized
  description so each localization key is created exactly once. After
  reclaiming only reproducible build/staging scratch, production compilation
  and the selected Sprint 35/correction group pass 7/7. The failed bytes will
  not be relaunched; a changed source-bound candidate is required.
- Candidate `6df32ad8` passed strict packaging (ZIP
  `b17238ce2490a41e7e4d7e972d846a908c96bf2cf949d24efc02549e07114551`;
  DLL `eff9bf5be478cc41d7ab8157941f3bf3831eed26dd97ab38e8758d4a4db5c43d`;
  MVID `517cc042-d437-4225-9de7-2a60c61bed06`). Guarded run
  `20261030T1838360037546Z-ac19654a47c247f0b9878069c816f802`
  closed the prior registration defect and produced real PASS rows for hidden
  bootstrap, both distinct attached original bodies, ordinary-melee Barbed
  Defense, the manufactured reach-glaive exception, nonrecursive retaliation,
  and Bone's exact native sting/tail contact. The wall was not cast: the
  observer's setup demanded a ten-meter chord around one isolated node even
  though the accepted local graph/LoS search in the same run found 52 legal
  combat lanes. The runtime result arrived just after the outer timeout,
  classified the failure as that ineligible fixture setup, restored its world
  fixture, and exited without save interaction; no owner process remains. The
  wrapper restored the live mod tree to exact pre-run SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`.
  The package/receipt/test evidence and exact compile archive are preserved on
  `E:` under manifests
  `71323674f1ed70dc94b46e2ccbed6aaf05b65a2767d92412b2cf0c324e29504b`
  and
  `3bc624dbec6000035180b69d0bbb3354661e33b34d5cc75c79795cf2600128d1`;
  both raw installation backups are preserved there under manifest
  `ac5f40c82b72dc393a2156ef7bda3c136ce34eacace6fd3cae9c92eb4203dcee`.
  The bounded observer repair now searches the already-proven party-local
  graph, requiring same graph/area, a direct native trace, legal off-lane cast
  point, LoS and party clearance; request-owned summons are parked off the
  measured lane so they cannot supply the claimed obstruction. Production wall
  behavior is unchanged. Production compilation and the selected group pass
  7/7; the changed observer requires a new native run.
- Commit `5bafcb4c` first reached the exact compile and selected 7/7 test
  boundary twice, but deterministic package creation failed closed both times
  when C: exhausted its remaining space. Neither incomplete package was staged.
  Their complete candidate directories are preserved separately on `E:` under
  verified manifests
  `2c950023920d39406af60d4ae186e5bd2fb869ae21aa35c70a7fd908e39b2780`
  and
  `0e71dcee059685a8bc3baccf806ba1c1edd497c8e8fe8aaa3ac77e42c8b7d80a`.
  Ten raw live-mod backups belonging to the preceding Sprint 35 guarded runs
  were also copied and verified file-for-file on `E:` before their redundant
  C: copies were removed (1,360 files; manifest SHA-256
  `9bf9296d4ffc0af7e1625fc0f9a2daad1e7044460a23b68959f3c96c0d216a92`).
  No historical or unrelated backup was selected. With 886 MB of measured
  headroom, the same source then passed strict package validation as ZIP
  `3ada24d636dfb4b3b13d73d0384087c268a1d60f45ce357c0024dbc69d377812`;
  DLL
  `ae4f6384600e193feaba4e2675766bd46aed67a9d27be6209116a002826ca017`;
  MVID `b75e84f1-f461-4e8d-8fb4-54bda6780d72`; source-state SHA-256
  `8ed8ce2c2b5f7fc7ba904bea5b94e6a25ea93c00346c75cab330a2f3e84e5586`.
  Its exact-build tree is preserved on `E:` as 162 verified files under
  manifest
  `22b03e70582479ef029dd9841808a7739e801f47b0a2342e67422b1169ec79c2`;
  the runnable package, receipt, selected-test log, and validator remain on C:.
- Guarded run
  `20261030T1916306416621Z-ff24788552c9409c9618eac1f8b4b759`
  retained the hidden bootstrap/profile/body and Barbed ordinary/reach contact
  PASS rows, then stopped before the wall cast on a chance-sensitive observer
  verdict. Bone's one real four-attack native plan emitted all four exact
  weapon events and the exact sting advanced its private authored tail
  (`contactsAfter=1`, `contactGap=0.001763626`, no forced rolls). The command
  reported `Success` at its final native act boundary, then retired as
  `Interrupt` on the following `UnitAttack.OnTick`; the old cell incorrectly
  required the later mutable result. This is an observer defect, not a sting or
  body failure. The bounded repair reuses the accepted RTWP completion rule
  already used by the Lillend fixture: final-act success plus the exact command
  reference and a complete real attack plan. It additionally requires exactly
  one sting row before accepting the already-measured tail contact; an
  incomplete plan remains a failure. The Focused production compilation and
  selected Sprint 35/correction group pass 7/7 after this change. The failed
  candidate will not be relaunched. The wrapper staged no save, received its
  structured FAIL in 214.190 seconds, exited the game, and restored the live
  mod tree to exact pre-run SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`;
  restoration record
  `20261030T1920382640955Z-disposable-expanded-summoning-sprint32-native-review.json`.
- Candidate `0d2e1b7d` passed strict 440-file packaging (ZIP
  `fee807b1d830b61921737a6f606ac054b51fb274a8f12e3bb72431e349e0ba70`;
  DLL `3343eb712dc889c725566e0c02f2712eb60c4674f3f16036245782a11aec274b`;
  MVID `861ccc46-a86a-4746-91f5-bfc6683053f4`; source state
  `324b7b9912a582dedfd189e5e35d6d8f4999d96eb83e22fc5117296fa77adeef`).
  Guarded run
  `20261030T1935260140953Z-0d9ae262e4a841d388c1e61400fb286f`
  retained the hidden bootstrap/profile/body and Barbed contact PASS controls
  and closed the Bone RTWP tail cell: the exact four-row native plan contained
  exactly one sting, reached final-act `Success`, then retired normally after
  the completed plan. The first real Wall of Ice command was targetable and
  completed `Success` on a measured six-metre native path, but produced no
  section; pathing consequently remained unchanged. Installed 2.1.7b IL gives
  the causal contract: `AbilityEffectRunAction.Apply` (165 bytes) returns at
  IL `003c` when `HasSavingThrow` is true and `TargetWrapper.Unit` is null;
  `get_HasSavingThrow` (10 bytes) is true for any enum value greater than
  `Unknown`. The formation graph incorrectly put a Reflex save on its point
  target, so its spawn action never ran. The bounded repair uses no outer save
  for formation and records/rejects a future recurrence in the native row;
  formation contact save/SR remains a separate unimplemented unit consumer,
  not something this repair claims. No save was staged; restoration returned
  the live mod tree to exact SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`.
  The two run-owned live-mod backups are preserved on `E:` as 272 verified
  files (62,186,054 bytes), manifest SHA-256
  `50cd8eff20d69a1ffc0b7c65f6be05da85175b5517d7d8815ee729685b4d6774`;
  only their redundant C: copies were removed. The repair's Focused production
  compilation passed in 25.6 seconds and the selected Sprint 35/correction
  group passed 7/7 in 5.6 seconds; candidate packaging and the affected native
  rerun remain pending on changed source.
- Commit `7b3caea8` passed strict 440-file packaging (ZIP
  `22795cd708236abe058ba6129f55db7f16a15ecc7bf60feaf9837035c27fc311`;
  DLL `7b3df787e001314d244de606018dec910b099111f7ac7731ddf9c312a304558d`;
  MVID `67b7d7a0-5f84-48eb-be51-c3b782b3a765`; source state
  `f1f22071be01a9b709eefb12fadbad03bca099794cb950369a65d4a3005d19c6`).
  Guarded run
  `20261030T1955576328355Z-025820c6613f4fe5bfd104ff684fb9bb`
  closed the point-target formation defect: the outer save is `Unknown`, the
  real command completed `Success`, and exact section
  `5d25c68c-f3d4-42dd-93c4-8ac4deda9495` spawned at lane centre with blueprint
  `cc2c011c696e4d3e846815c60245465a`, 36 HP and its native summoned-lifetime
  buff, then retired when that buff was removed. The measured six-metre path
  remained the identical two-point `PathClear` route before, during and after;
  LoS also remained unchanged. This disproves the prototype assumption that a
  standing unit itself is a navigation obstacle; it does not disprove the
  installed dynamic-cut path. The exact build is preserved on `E:` as 162
  verified files under manifest
  `A9FF8D461232C2654CFD27C11AF8254F4D4988F7980E2F5A3F3B522F6488F743`;
  the package evidence is archived there under manifest
  `41E35CF831B6BEACF685C096D84761303FC3BB28BCDCAC92C4DF891AB2855D13`.
  The run-owned backup is likewise preserved as 440 verified files under
  manifest
  `7A5D7F304C518DF2D3D7CFAA4B55BC3A20EB8C07C039A6C152EB6D3A462EB0FF`.
  Outer transaction `compat-20261030T195248Z-b47e043d977d` is `Restored` with
  `restorationVerified=true`; it staged no save, left no owner process or lock,
  and removed its temporary original/staged trees.
- Installed `Pathfinding.NavmeshCut` is a public native `MonoBehaviour` with a
  rectangle mesh type, measured size/height/rotation fields, `ForceUpdate`,
  update/revert graph objects, and `OnDestroy` removal. The bounded changed
  candidate now adds one such cut only to the exact private section view; the
  section unit remains the serialized HP/lifetime authority and recreates its
  view-only cut through `UnitEntityView.OnDataAttached`. The observer records
  exact owner and configuration, passively polls for the native graph change,
  destroys the section through its existing lifetime, and passively polls for
  restoration; it never calls a graph update. An initial Focused compile
  stopped before tests on the one missing `Bootstrap.ModContext` import. With
  that exact import added, production compilation passed in 26.1 seconds and
  the selected Sprint 35/correction group passed 7/7 in 5.0 seconds. A new
  source-bound package and one changed wall viability run are required; the
  unchanged `7b3caea8` failure will not be relaunched.
- The first exact `4961ff41` package attempt passed its exact-reference compile
  and stopped at deterministic ZIP creation with `No space left on device`; no
  package was completed or staged. Its 166-file failed candidate is preserved
  on `E:` under manifest
  `67E62089116E1B4797E1A20504978302D77D6054099AEFB22461D970035E62B2`.
  After removing only that verified duplicate plus reproducible bin/obj/staging,
  measured C: headroom was 949 MB. The unchanged retry passed repository
  validation, the selected 7/7 group, exact-reference compilation, strict
  440-file packaging and package validation: commit
  `4961ff41a3021064234f9f62fd04ff82a7ed4677`; source state
  `1e319711de9c28a91dd2d554203f98a853b0d2e826c27e43b92d0c04ba581cdf`;
  ZIP `fbc7568ab7dc72f7ba32a1a6579dda903bd40abb5e22dce018505ae9e4782664`;
  DLL `7b3e042f124253a27059a41e1cfcacf1e39dc1ac05eeed2076d25f7e70a31041`;
  MVID `e2bb9280-711e-4f62-b5e1-a166d9a40ee5`. Its exact build is preserved as
  162 verified files under manifest
  `463D1FFAA29B717A1D3A040718FC4601BA832FE452A7C9898218896EE18D5898`.
- Guarded run
  `20261030T2026107412038Z-226813357ab640ad97927348ec12f50e` proved the
  exact section owns one registered rectangle cut (`3.05 x 0.75`, height
  `3.05`), but every sample from frame 5,616 through 5,766 retained the exact
  two-point 6.0080795-metre `PathClear` route. The section again retired through
  its native summon buff; LoS did not change and navigation was only vacuously
  identical afterward. This is a real production failure, not a missing cut or
  observer race. Transaction `compat-20261030T202336Z-4aa56b421609` is
  `Restored` with `restorationVerified=true`; its original/staged temporary
  trees and lock are absent, no Kingmaker process remains, and no save was
  staged. The exact package plus 440-file run backup are preserved on `E:`
  under manifest
  `204FCAFAB2F1A6009FAC01B1132E4AA40C0F9EB0666782746748419F45FD0A05`;
  only verified C: duplicates were removed.
- Installed 2.1.7b IL now narrows the missing consumer. `NavmeshCut` only
  registers geometry. `TileHandlerHelper.Update` calls its public `ForceUpdate`
  to reload changed recast tiles; the separate `CombatController` grid path
  calls `GetUpdateObjectForGrid`, but only inside active, unpaused turn-based
  combat. The next bounded candidate therefore asks an already-present native
  `TileHandlerHelper` to apply the exact cut once, and after cut destruction
  queues its old bounds, asks the same helper to restore once. It neither
  creates a helper nor directly creates/applies graph update objects. The
  observer adds graph type, helper inventory, cut bounds/registration and the
  production update route. If this installed consumer is absent or still
  inert, the wall path will stop for an explicit gameplay-adaptation decision;
  it will not grow into a general navmesh subsystem. Production compilation
  passed in 25.9 seconds and the selected Sprint 35/correction group passed
  7/7 in 4.8 seconds; the changed source requires a new exact candidate.
- Commit `5623824ce7cac04fbb218cfaf5c1dc15ba035c5e` passed the selected 7/7
  group, exact-reference production compilation, strict 440-file packaging,
  and package validation: source state
  `d6506846ca7ec11d0c4628a03aca5f295662fc3a1acbb7855bb615ad386a696d`;
  ZIP `e132a26f783a3760760e26c432d501a0ca31dc5ca5a99e7ac20b6d0f69051286`;
  DLL `bfba38ca7c564cdab4e22a97059463b1f318a81dc17985954d1ee5f5bdc488d8`;
  MVID `cc953185-2724-4e23-82f8-0391691d244c`. Its 162-file exact build is
  preserved on `E:` under manifest
  `64DC030C2E58832F4B189653295C5A7061EE15E24B1D85A80AF2638CEC208500`.
  Guarded run `20261030T2047515415213Z-cbaf2e5307a84d509afd225da2334914`
  found the active installed helper (`NavMesh`, enabled/active, 0.1-second
  interval), registered the exact `3.05 x 0.75 x 3.05` rectangle cut, and made
  one production recast-apply request. `recastUpdatePending` became false, so
  the native consumer processed the cut. The actual path nevertheless used
  `Pathfinding.GridGraph` index 1 while the cut/helper operated on the separate
  `Pathfinding.RecastGraph` index 0; all 16 samples retained the exact two-point
  6.0080795-metre `PathClear` route. LoS was unchanged. The section retired and
  the world fixture restored, but navigation restoration was only vacuous
  because obstruction never occurred. This is the final bounded supported-path
  feasibility attempt, not an observer race or an absent-helper failure.
  Transaction `compat-20261030T204518Z-46bf1673be4c` is `Restored` with
  `restorationVerified=true` at `2026-10-30T20:51:05.8499527Z`; temporary
  original/staged trees and the compatibility lock are absent, no Kingmaker
  process remains, and no save was staged. The exact live-mod backup and
  transaction package are preserved on `E:` under manifest
  `8243FBB53A4BFB3E9507C440FDBA4D6F9015580778A2CDC35ABBD535DBC4694D`;
  only their hash-verified redundant C: copies were removed. Wall work now
  requires the explicit bounded gameplay decision above; these bytes will not
  be relaunched unchanged.
