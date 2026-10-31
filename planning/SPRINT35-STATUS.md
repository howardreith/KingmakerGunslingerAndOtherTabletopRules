# Sprint 35 status — Barbed Devil and Bone Devil

Last updated: 2026-10-31. Active product branch:
`codex/expanded-summoning-sprint35-barbed-bone-devils` at reviewed master
`aee691127b9ade4bbfde55247d54652acf85957c`.

Sprint 34 is closed at v0.0.172. Its accepted creatures, assets, adaptations,
campaigns, and artifacts are inputs only. This file is the single current
Sprint 35 checkpoint; historical records are not current instructions.

## Compact requirement-to-proof map

| Required outcome | Existing implementation / evidence | Demonstrated gap | Smallest patch | Selected proof |
|---|---|---|---|---|
| Append-only registration of Barbed Devil and Bone Devil, initially hidden | Hidden registration and qualification are complete: 128 creatures, Monster 106 / 587, Nature 105 / 588, with both roots and 47 private dependencies resolving | Final public candidate and exact five-route native proof remain | Remove only the two suppression keys; preserve every allocated identity and ordering | Focused ledger/catalog tests plus the frozen-candidate bootstrap and grouped public-route run |
| Five untemplated Monster routes only | The catalog now exposes Barbed VIII/IX and Bone VII/VIII/IX with one, 1d3, and 1d4+1 semantics; derived totals are 1,175 generated placements + 29 wrappers = 1,204 visible choices | The new public-route observer has compiled but has not yet run natively | Keep the exact five routes public together; add no SNA or template variants | One grouped public-route run records each command, actor, quantity, tier, duration, ownership, icon and cleanup |
| Barbed printed combat profile and defenses | Sprint 34 profile, natural-weapon, feat, DR/SR, reach, action/resource, and ordinary Cleave machinery are reusable by contract | Existing Infernal34 traits encode DR 5/good-or-silver and the released creatures' profiles; no Barbed exact weapons/profile | Add a Barbed-specific profile using shared helpers only where exact: Medium, two primary claws, DR 10/good, SR 22, printed saves/abilities/feats | Focused profile calculation plus native inventory/attack/modifier/defense observations in both combat modes |
| Barbed retaliation, claw fear, grab, and impale remain four distinct consumers | Exact-weapon reach policy; source/target-owned grapple link store; natural 1/20, size, immunity, limb, target, reload-reset, and maintain contracts already exist. Exact candidate `1ba1d5af` now passes two-owner retaliation, claw fear, oversize/immunity grab negatives, a real failed maintain with exact-owner release, reacquisition, and one exclusive Impale | Reload-safe reset and fresh-process absence remain for the selected persistence envelope; the behavior group itself is closed on the current hidden source | Retain the exact-event/exact-owner production predicates and the bounded release cleanup only for the exact Barbed/Impale owner, exact target, and absent reciprocal target link | Reuse native PASS `20261031T0029267333965Z-bf97d85fd26643959cb5e767b26e56c1`; rerun only changed persistence/public-route consumers on the final candidate |
| Barbed legal SLAs with real costs, including exactly two Scorching Ray rays | Hold Person and exact two-ray Scorching Ray use installed spell graphs. Unholy Blight reuses the qualified all-creature alignment-burst consumer with live DC 18, one per-summoning resource, independent 1d4 sickness, and ally-safe AI admission without changing victim eligibility. The exact current candidate produced two separately attributed native ranged-touch hits and two `4d8` damage events | Installed census has no named Major Image, Produce Flame, Pyrotechnics, or Order's Wrath graph. Major Image needs a broad illusion consumer; Produce Flame needs its touch/thrown-flame contract; the existing project Pyrotechnics adaptation is not faithful; Order's Wrath geometry remains unproved | Owner decision 2026-10-31: omit those four unsupported buttons, disclose each limitation, and add no substitute; keep teleport/summoning precisely absent and retain the faithful supported SLAs | Reuse current two-ray native PASS plus the earlier qualified Unholy group; fresh-load resource preservation remains in the mixed persistence envelope |
| Bone printed profile, four equal-role natural attacks, defenses, and effective flight representation | Existing outsider profiles and ordinary-map Airborne representation | No Large 10-foot-reach Bone profile; donor-edition secondary penalties would be wrong | Add Large bite/claw/claw/sting at printed equal attack bonus, DR 10/good, SR 20, speed 40 with the established Airborne single-speed representation for fly 60 | Focused attack math and native full-attack limb/bonus/damage/contact inventory; movement/reach observation and representation disclosure |
| Bone sting poison is injury-gated and independently timed | Existing native-backed injury poison graph and wound gate support exact weapon, positive final damage, finite exposures, stat damage, and cure saves | No sting carrier with Fortitude DC 20, 1d3 Strength, once/round for 6 rounds, cure 2 consecutive saves | Clone the proven graph deeply for the exact sting and initialize all dice/context operands; do not attach summon-source cleanup to inflicted poison or erase ability damage | Miss/nonwound/other-limb negatives; sting wound positive; native DC, cadence, Strength damage, finite course, two-save cure, source expiry, dispel distinction, save/load |
| Bone fear is a 5-foot aura with native eligibility and source memory | Existing source-attributed fear terminal buffs and area membership/lifetime helpers; Kyton gaze is explicitly inapplicable | No continuous aura consumer or repeat-save contract for Bone | Build a source-owned 5-foot aura applying native frightened for 1d6 rounds on failed DC 19 Will save with source-specific repeat handling; preserve native immunity/eligibility and do not make the area enemy-only | Actual membership enter/stay/leave, immunity, success/failure and no frame-by-frame retries, two-source isolation, source cleanup, both combat modes |
| Bone regular and quickened self-invisibility stay distinct | Existing native ordinary attack-breaking invisibility, self targeting, resource, and action builders; both generated buttons already share that native graph | Standard/quickened native outcomes and toggle/load-reset remain unproved; audit found both AI actions lacked their active-effect guard | Bind both exact-owner AI actions to the native ordinary invisibility buff so admission refuses redundant recasts while the effect is active; actual targeting, duration, action and resource rules remain unchanged | Native action economy, attack breaks effect, active-effect AI negative, standard/quickened distinction, 3-to-0 spend, no reset by mode/toggle/load |
| Bone offensive control and faithful Wall of Ice | Immutable native facts from `20261031T1250109528231Z-be0cb7d758594a26ba18906a60c3a460` prove cast/orientation, 22-node obstruction, real physical half damage, full fire breach, residual cold/steam, adjacent SR/Reflex disruption, and exact navigation restoration; the bounded reader verdict is PASS while the original observer FAIL remains | The corrected sampling must run fresh on the frozen public candidate. Player-drawn multi-section geometry, hemisphere, Strength breaking, and line-of-effect blocking remain unsupported and explicitly unclaimed; Dimensional Anchor is omitted | Keep the approved Wall-only GridGraph adapter and one supported grounded 10-foot planar section per cast; no substitute control or broader terrain framework | Fresh frozen-candidate early run plus mixed persistence and AI/fallback proof |
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
  The five Sprint 35 placements were the only suppressed routes; publication
  now removes exactly those two creature keys without reallocating identities.
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
- The owner selected the bounded Wall of Ice adaptation. Hidden native run
  `20261031T0934466325823Z-6025f197cc1c4588baad34f4d3173e2b` proved one
  private section obstructs the actual movement GridGraph and that retirement
  restores every captured node and the original route with zero live claims.
  Production now adds the supported grounded 10-foot planar placement,
  occupied-space rejection, adjacent SR/Reflex disruption, attackable 36-HP
  section, auto-hit attacks, non-fire object halving, full fire damage,
  remaining-duration breach cold, ten-minute native fog after lethal fire,
  exact source cleanup, and bounded AI admission. The selected 10/10 focused
  group and production compilation pass. These changed consumers still require
  the next hidden native run; no public-route, AI, or persistence verdict is
  inferred from focused validation.
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
- Commit `fcbf2922` produced ZIP
  `1c7485504673a00f39b66e7a1d767aea2f7dbe883f52b0e25d84c4f6b4e815df`
  (DLL
  `615884a9c47bd4e99ad430ce181c129e0d030aae58bdea11880f594245d4c524`,
  MVID `812bd289-6784-4a92-a4f3-8dfff98749a3`, source state
  `ee588825d092d2f31fd11431c373dfbc440b83ee00b3ef63f170b77d7ca27e20`);
  its exact build is preserved under manifest
  `585D3230DF5C52A476570981D413DFF035A9E4852A188EFD1BE3D6629A13C86E`.
  Guarded run
  `20261030T2147147935627Z-a492adbf17b34170ac220b3a9fc2820f`
  passed all ten native assertions and all six Barbed phase cells in 164.780
  seconds. Two owned aura areas existed immediately after actor disposal and
  both retired naturally after two Unity frames; the terminal count was zero.
  The outer wrapper restored the original 136-file live tree to exact SHA-256
  `216a9dc2b8e95cd644ba3cadc69a638463c25e60f40a11f8d4b2065c69d5aaf3`.
  Raw PASS evidence, both snapshots, deployment and restoration are preserved
  under manifest
  `E047405DAB353C322D48F3D25F4345C99BE0947CA4640133973B2491E9F3BFD7`.
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
- The post-Unholy audit found one bounded production admission defect: Bone's
  standard and quickened Invisibility AI actions did not name the ordinary
  attack-breaking invisibility buff as their active-effect guard. Both now use
  exact native buff `525f980cb29bc2240b93e953974cb325`; spell targeting,
  duration, action economy and the three-use resource are unchanged. The same
  audit confirmed that installed `BlueprintAbilityAreaEffect` has no
  `AffectAllies` switch: allies are ordinary members and `AffectEnemies=true`
  adds enemies, matching prior native all-creature area observations. Bone's
  fear logic itself does not filter by faction, so no enemy-only adaptation was
  introduced. A new `sprint35-infernal-barbed-combat` phase now observes the
  remaining Barbed sequencing with real commands: two independent retaliation
  owners, wounding-claw fear, exact mind/maneuver/size controls, natural
  grab/maintain impale, and the explicit two-ray SLA. Its reacquisition waits
  for reciprocal native grapple cleanup and preserves every natural refusal.
  The final observer audit also reused the established exact target-escape
  streams: one attributed Mobility path or one native CMB/maneuver path may
  explain a released hold and permit a bounded retry; unrelated target events
  or unattributed hold loss still fail. Production compilation and the selected
  Sprint 35/correction group pass 8/8 (25.4 s / 1.3 s); runtime orchestration
  passes 271 assertions and guarded
  preflight passes 1,173 checks. Earlier compile-only attempts stopped on
  missing observer namespace imports, and the first orchestration check exposed
  the existing one-line phase-declaration parser contract; both were corrected
  without gameplay changes. No package, deployment, save or game process was
  selected by this checkpoint. The changed observer and Bone AI require a new
  source-bound package and affected native runs.
- Exact candidate `f1cd6a13aefccab0ef368d761dbe2559ed2d2fc2`
  passed repository admission, the selected 8/8 group, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `ff80fd04b358cdc9c9d6ecfb3e54b0817a1954a6e41314822beb0bdae550b0e5`;
  ZIP `0b8a1f202e8245a8200262c258345ae6c4edbfd6dc3bc561d1024018a1fed08d`;
  DLL `614f46ebce6d9715535fb9f14d51b62eec0d28d08fa1fdf78b23f834c8e74ff9`;
  MVID `1e38c8f2-dc20-4913-9d10-e0f63780bfe5`. Its 162-file exact
  build is preserved on `E:` under manifest
  `AB141F090C7236C363D32A2A4B64D4B8121BAF4DDDFCA1BD9A3268737A827E1B`.
  Guarded run `20261030T2220157734150Z-b9ed104766764c1bb4dbafe4de9b0f56`
  failed its first Barbed mapping cell: the two claws and two-ray SLA were
  present, but `SummonGrabComponent.Find` returned null. The raw result proves
  the active Barbed grab was intentionally installed on a hidden
  `BlueprintFeature`, while the shared lookup searched only buff facts; that
  also denied later-round maintain/Impale access and is a production defect,
  not an observer mismatch. The bounded correction preserves the existing
  buff-first route and then searches active hydrated feature facts. The mapping
  observer now requires reference identity with that active feature component.
  The live tree restored to exact intake hash
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no Kingmaker process remains. Candidate, snapshots, deployment, raw FAIL and
  restoration are preserved as 309 verified files on `E:` under manifest
  `21049FBCCF1D4AC00F34A64010FEAA9CDD4FEB424854671EB18A26FA54F7A9FD`.
  The failed bytes will not be relaunched; the exact feature-fact lookup is the
  only causal production change before the next candidate. Production
  compilation passed in 29.0 seconds and the selected Sprint 35/correction
  group passed 8/8 in 5.1 seconds.
- Exact candidate `615c1cbbab67905fe820310a950ebfd0e83360bc`
  passed repository admission, the selected 8/8 group, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `dbe59ecc8332c86e3ccbc42dc652c4cc971495d1da17676628b051a4e8769c46`;
  ZIP `a931880524302b611fce00ae6a1f1c1f70b246137ea20a9ed1accfc6a782dda7`;
  DLL `2693622ec41395c64f1d7fbfd4b939fa4a22bc40678600898ff987adf45b7e9b`;
  MVID `47f27d17-88a1-4ea2-8d18-e11e7ad6bb8b`. Its 162-file exact
  build is preserved on `E:` under manifest
  `4E92F0DC6BFF4B7218CDB3945146EE4A31796CBFBCB2DA524442323F4C1F67CB`.
  Guarded run `20261030T2253208108579Z-ba0ad9c616cc4d4bbf81e356923c4303`
  closed the feature-backed grab mapping and passed both independent-owner
  Barbed Defense cells, claw fear, oversize rejection and maneuver-immunity
  rejection. The first eligible real claw then dealt 18 HP damage and won its
  one real grapple check (17 + 222 versus CMD -99); both reciprocal grapple
  buffs were active and the command had acted, but the native attack command
  retired as `Interrupt` after that successful state change. The observer
  incorrectly required terminal `Success` before it would inspect those real
  hold facts, so it reported a natural-refusal failure. This is an observer
  defect, not a gameplay failure. The bounded correction accepts acted
  `Success` or native `Interrupt` only while still requiring exactly one real
  wounding claw, one successful attributed maneuver, the exact held target,
  both reciprocal grapple parts and buffs, the establishing primary claw,
  exact normal command cost, and no premature Impale. The run restored the
  live mod tree to exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no Kingmaker process remains and no save was staged. Candidate, snapshots,
  raw FAIL and restoration are preserved as 309 verified files on `E:` under
  manifest
  `E499871960B2A5FC53316733A23266F59E0DE657138F41C6EFDDB0D2077A9E6F`.
  The unchanged bytes will not be relaunched; the changed observer requires a
  new source-bound candidate and one affected native rerun. Production
  compilation and the selected Sprint 35/correction group pass 8/8 after the
  observer repair (25.2 s / 5.8 s).
- Exact candidate `ae78425ca04167f351f5a7386cf02d340162d531`
  passed development admission, selected 8/8 tests, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `f75b483ebc63e8a3baf8a18b89a224343dc6f0da5194195551a3bcbf5db47329`;
  ZIP `1444fb12f4535f749323d43f0087c1fd4fe224d97cd58a16431b7bfb379ebd77`;
  DLL `776ce96417001067bdd1e105ac7fe346dca5bfdc48f8aa8543d180d02a836904`;
  MVID `7b2d0077-102f-4a53-8a77-3504f991d403`. Its 162-file exact
  build (86,484,925 bytes) is preserved on `E:` under manifest
  `754A189C779C83533DCA2E9D2A2E8AD3ADCBBBD093477F98F681E3CA4E7DEB95`.
  The first guarded invocation refused before deployment because the
  reproducible domain-validator binary had been removed during C: headroom
  cleanup; the live tree never changed and its exact restoration record was
  retained. Rebuilding only that dependency-free validator restored the
  supported preflight without changing the candidate.
- Guarded run `20261030T2310220358354Z-7010964255a44c228fb863de82b18ab3`
  closed the prior observer mismatch: the real acted/retired claw established
  reciprocal hold state, and the later round made exactly one successful
  grapple check plus one direct `3d8+9` Impale with no replayed claw, fear save
  or default grapple damage. It then stopped before Scorching Ray because the
  observer manually called the production link-release helper and demanded
  every initiator-side native controller object disappear within five seconds.
  That teardown is not a player command or a missing grapple result: the
  shared release contract deliberately removes the target side and leaves the
  initiator side to the native grapple controller or owned-actor disposal.
  The bounded observer repair retains the already-settled second Barbed and
  Barbazu from the independent-owner cell, casts the unrelated ray between
  that clean pair, and lets ordinary owned-actor disposal close the proved
  hold at final cleanup. It changes no gameplay code and prevents the fixture
  from acting as a second release mechanic. The native run restored the live
  tree to exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no Kingmaker process remains and no save was staged. Candidate, snapshots,
  deployment, raw FAIL and restoration are preserved as 309 verified files
  on `E:` under manifest
  `E1E9C8B0BC7CDB3EEA53C17323F0C57AEC517EE2C0504AD84F7B09F9A18B11D1`.
  The unchanged bytes will not be relaunched; the observer-only source change
  requires a new exact candidate and one affected native rerun. Production
  compilation and the selected Sprint 35/correction group pass 8/8 after the
  isolation repair (26.0 s / 1.4 s).
- Exact candidate `cfa5319b0c50eae4b211c2ca67070f547a3c4c90`
  passed development admission, selected 8/8 tests, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `46030a7d0c8fed1fce326cd4619d1db39e738a74f43c2c233a7983b80c93fd7d`;
  ZIP `5f53cf5954f3991007ed760270aa1fc19f8e584367c8795fceb87db7afd473c6`;
  DLL `fb9de234993600c119c5cb9ef08e98aeaec550ce4204c3eeb4c4cd83a4d15297`;
  MVID `bb81de03-83e6-4318-8cfc-9d60e079b9d3`. Its 162-file exact
  build (86,490,539 bytes) is preserved on `E:` under manifest
  `6772D7786DABE025C24AFA144F6F5BD6FAA41741369A7FC3438029FECB3AE0FE`.
  Guarded run `20261030T2325472972946Z-141ce8e0e4e84457a092432c0544c7c8`
  again closed the grapple/Impale rows, then ran the isolated real Scorching
  Ray command. Both configured rays made separately attributed native ranged
  touch attacks (natural 18 hit, natural 1 miss), but the hit correctly dealt
  no damage because the observer had selected the retained Barbazu as its
  target and devils are fire-immune. The command, two-projectile delivery,
  CL12/spell-level-2 parameters and exact standard-action cost were otherwise
  present. This is an ineligible observer target, not a gameplay defect. The
  smallest correction disposes the Barbazu after its contact cells, retains
  the second Barbed as ray owner, and targets the existing non-fire-immune
  hostile while the independent original Barbed hold remains live. No roll,
  damage or immunity is forced or removed. The live tree restored to exact
  intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no Kingmaker process remains and no save was staged. Candidate, snapshots,
  deployment, raw FAIL and restoration are preserved as 309 verified files
  on `E:` under manifest
  `12B1D6EC69D20C3363A196BE3439FF08F965EF244A4039BC8340313B357CD611`.
  The unchanged bytes will not be relaunched; the observer-only target repair
  requires a new exact candidate and one affected native rerun. Production
  compilation and the selected Sprint 35/correction group pass 8/8 after the
  target repair (26.3 s / 4.9 s).
- Exact candidate `0734ca9bbe544d1ac1fa3dc2d11dd9397f25657a`
  passed development admission, selected 8/8 tests, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `4667f7818dd4ba11680f00ddd85c74ac6485c297c997694d5a77ec5129098b72`;
  ZIP `24fd39271cad718f6a7d27d9d15b459bffa0c08df40063fc39c386317f207aaf`;
  DLL `ff466db0f02f3d2037ad016ec5a1b759f5cbe366d34e26dccfa34ca12a6901df`;
  MVID `4e7decd3-4e4e-4ac0-9ace-43d27b907797`. Its 162-file exact
  build (86,490,489 bytes) is preserved on `E:` under manifest
  `D2AD920F2156DC8997C878B1F6144049A101F43928C994657C6F0A68B8C9293A`.
  Guarded run `20261030T2341017982842Z-b7ffcfba3cbf49918ab7d026e6bb2122`
  again passed mapping, both independent Barbed Defense owners, claw fear,
  oversize and maneuver-immunity controls, then established the exact native
  hold. Its later real grapple check rolled a natural 1: production correctly
  recorded product failure, applied no Impale, replayed no claw or fear, and
  removed the reciprocal target hold, but the full release predicate remained
  false after 159 frames. This matches retained Kingmaker 2.1.7b evidence:
  non-multi release can leave the owner's `UnitPartGrappleInitiator`, hold buff
  and control conditions after the target side is free, blocking useful later
  action and reacquisition. The shared hold callback still must not remove its
  own initiator part re-entrantly. The bounded production correction therefore
  schedules one main-loop cleanup only for the exact Barbed/Impale owner,
  exact still-named target and absent reciprocal target link; a different
  owner, different target or already reacquired reciprocal hold is untouched.
  The observer now records every release operand. The accepted shared reload
  reset and all other grapplers remain unchanged. The live tree restored to
  exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no Kingmaker process remains and no save was staged. Candidate, snapshots,
  deployment, raw FAIL and restoration are preserved as 309 verified files
  (130,226,705 bytes) on `E:` under manifest
  `510BDE561B89D0FB5969107F23DAA27FF88F65DBC1E3953254BC0058F61578A8`.
  These unchanged bytes will not be relaunched; the exact-owner scheduled
  cleanup requires a new source-bound candidate and one affected native rerun.
  The first focused compile stopped only because two new diagnostic fields used
  null-conditional access on native value-type `UnitReference`; explicit
  part-null checks corrected that observer-only syntax. Production compilation
  then passed in 25.5 seconds and the selected Sprint 35/correction group passed
  8/8 in 5.1 seconds, including the four exact-owner cleanup admission cases.
- Exact candidate `7efbea90d19e625b4b120298ed21615ab9b4ace2`
  passed development admission, selected 8/8 tests, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `7a2c375d6af6f683ac7a6d80f7d63507b7147c9c5988e28e952adb00de2e2d33`;
  ZIP `d3f1dc4cdc7633587d65c24a1d18df128bd509f7dcd777e4556304f9e438104e`;
  DLL `9704a6c95c514ed0c625626c3a2eec962d550b412a39f5afde23d4741957cda8`;
  MVID `219773e5-e505-4b2c-bb12-a1861c0b8c70`. Its 162-file exact
  build (86,500,642 bytes) is preserved on `E:` under manifest
  `C9565A0A3EF92BAA59BE310178C91BF67D0CCC2711DFCC7DC4A823AAE800F4FB`.
  Guarded run `20261030T2358491460573Z-92ed8a8c6b0b4d67bf4d17807578e454`
  passed every Barbed behavior row reached: real hold establishment; one later
  natural roll of 3 producing exactly one successful grapple check and one
  direct `3d8+9` Impale with no replayed claw/fear/default grapple damage; and
  the isolated real Scorching Ray command making two native ranged-touch hits
  and two separately attributed `4d8` damage events (17 and 18 damage) while
  the independent original hold remained live. The sole FAIL was final
  observer teardown: both owned actors were already destroyed and outside game
  state, owned areas were zero and every hostile fixture modifier was restored,
  but the observer required every owner-side native part/buff object on the
  already destroyed `UnitEntityData` to be null. The aggregate did not record
  whether the still-live hostile was free. The bounded observer correction
  records the disposed owner's residual objects but gates success on the live
  target having no exact owner grapple part, no source-attributed grappled buff
  and no reciprocal hold. The next run also uses the already-supported native
  maneuver-immunity condition only for the first later maintain check, then
  removes it immediately; this lawful fixture precondition guarantees the new
  exact-owner scheduled release is observed before an ordinary bounded
  reacquisition and successful Impale, without seeding a roll or changing
  production balance. Prior natural-1 raw verdict remains preserved. The live
  tree restored to exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no Kingmaker process remains and no save was staged. Candidate, snapshots,
  deployment, raw FAIL and restoration are preserved as 309 verified files
  (130,275,352 bytes) on `E:` under manifest
  `4B9BA5DC5F714127DFE8F1781644D659458881AADEE050D5565BC0398BDA6BD6`.
  These unchanged bytes will not be relaunched; the observer-only proof and
  disposal correction require a new source-bound candidate. Production
  compilation passed in 26.2 seconds and the selected Sprint 35/correction
  group passed 8/8 in 4.8 seconds after that bounded observer change.
- Exact candidate `8fee8c8324cc80ef1fba5776bcc45f13732d0f7d`
  passed development admission, selected 8/8 tests, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `fd9963e7dcc67888a9ab558c468a5179a41be028ffb6efe1238e6adaeee73ced`;
  ZIP `d7441e87c2098f225a6d95abecee24a8ea54622a8e102b01ee7635bf270bc63d`;
  DLL `fcf78d44ac19b8e449ef042be6b17604c97d12e3b5ee660a1688eb4b3a43b020`;
  MVID `4d94fca4-f30c-4608-b1c1-86400bff1aeb`. Its 162-file exact
  build (86,510,861 bytes) is preserved on `E:` under manifest
  `A0C2521EC8220E8212FAF0E4214001DD72DCEC0B24E5AC4C0557F90F59F92CDE`.
  Guarded run `20261031T0014239226386Z-c4b3570eecf24049ab95e300130a9d35`
  established the real reciprocal hold, then demonstrated that native
  maneuver immunity is not a failed maintain verdict in Kingmaker 2.1.7b:
  the owner-to-target event recorded roll/CMB/CMD zero yet native and project
  success, while no Impale occurred and the hold nevertheless released. The
  new release diagnostics proved every operand clear within two frames
  (initiator, target part, establishing limb, both buffs and reciprocal held
  target), but the observer correctly retained FAIL because that invalidation
  route is not the required failed grapple check. The smallest correction
  replaces only that fixture precondition with temporary high target CMD.
  This keeps the actual natural d20 and native CMB/CMD event; any non-20 is a
  genuine product failure, after which the CMD is restored before exact-owner
  cleanup, reacquisition and later successful Impale. A target-owned escape
  cannot substitute for the selected high-CMD proof. No production code or
  balance changes. The live tree restored to exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no Kingmaker process remains and no save was staged. Candidate, snapshots,
  deployment, raw FAIL and restoration are preserved as 309 verified files
  (130,229,320 bytes) on `E:` under manifest
  `53A99ABF88E241A40096550B6A2400294BFAC373B21877A4CCBD097ED32ACBFC`.
  These unchanged bytes will not be relaunched; the observer-only high-CMD
  proof requires a new source-bound candidate. Production compilation passed
  in 26.0 seconds and the selected Sprint 35/correction group passed 8/8 in
  5.0 seconds after that observer-only change.
- Exact candidate `1ba1d5afaddea3793d79d77ec6a7f4e73747d518`
  passed development admission, selected 8/8 tests, exact-reference
  compilation, strict 440-file packaging and package validation: source state
  `2baddeb2d9c18ef8929eaa3895939a5987a69e889b7134f9091f812983ba1ac4`;
  ZIP `fb14b89159ebd3c7bc3e7fab64f8e70922c41a1ae06cb3eec23e459e53a16554`;
  DLL `2d4c9f0856415e0fa377f0d55b2645944a0d11d766e9c60ba3616ccffb15858b`;
  MVID `554746f9-fea0-4200-99d2-0c54f6c8f968`. Its 162-file exact
  build (86,511,041 bytes) is preserved on `E:` under manifest
  `B0246938761B3F142F147C022D8288CA9354EFE6F345762061C22374C6EEA29A`.
  Guarded run `20261031T0029267333965Z-bf97d85fd26643959cb5e767b26e56c1`
  passed all 24 assertions and closed the hidden Barbed behavior group. The
  first later maintain retained a natural roll of 12 and native CMB 227, used
  temporary target CMD 999, recorded native/product failure, produced no
  Impale or replayed claw/fear, and cleared the exact reciprocal hold in two
  frames. Fixture CMD then restored. The next bounded attempt naturally
  established the hold (14 + 222 versus CMD -99), maintained it (12 + 227
  versus CMD -101), and applied exactly one direct `3d8+9` Impale with no
  replay. An independent Barbed then made exactly two separately attributed
  native ranged-touch hits and two `4d8` damage events while the original
  hold remained live. Final cleanup destroyed both owned actors, left the live
  hostile with no target-side hold/buff/reciprocal link, removed every owned
  area, restored every fixture modifier, restored the empty native profile,
  and attempted no save operation. The wrapper restored the live mod tree to
  exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  Candidate, 29-file runtime record, deployment, both snapshots and restoration
  are preserved as 309 verified source files (130,326,260 bytes) on `E:` under
  manifest
  `58F1D0BFB34C2E21082182F61671D1F986366D1C5E59487DE4BB2CBD5038F722`.
  Barbed behavior is now closed on the hidden current source; its route and
  fresh-process lifecycle rows remain for the selected final grouped campaign.
- The hidden Bone group then exposed four bounded observer/fixture assumptions
  before reaching its spell rows. Runs
  `20261031T0058029406866Z-8c656420208f4c4ca4ace16b1a5f1a44`,
  `20261031T0112083719130Z-0fad3c24d50044e1b30ba473c874dcad`,
  `20261031T0123557317856Z-f66fb97e1a5b4c088344d86922b2f6c9`, and
  `20261031T0142299782664Z-da5c22984bec4c819fb21d90d8c7fb81`
  respectively corrected the aura's real all-creature precondition, restored
  the summoner anchor before native lane selection, recorded stat-damage
  `Bonus` after the native roll, and matched Kingmaker's truncating difficulty
  scale. Each raw FAIL, deployment, snapshots and restoration remains under
  its named `failed-bone-native-*` archive on `E:`; their manifest SHA-256s are
  `DBCB2EF7AFAD20C8BA6DDF0978E550559675935CE5BD3D1E34B110707BAE2123`,
  `8654DDFEBD26CB26406ADDF80AE53E05770084A1E118C2F68CF87914CDCD413B`,
  `099B7BE8F211E469D0CA67E825FB2C26628F4A4A3643A8302D69C1FAF80E69BA`,
  and `04C94A99DE63309E72BF5AE34290FE1A8C5EF2F79CCCCA230D943DE5E445A710`.
  These are retained failures, not retroactive PASS records.
- Exact candidate `eb9cbf4af266ce256ae5b165727c8f321bb42b2b`
  (source state
  `57082cdd1675587b1cd3cd22449bf7dc72e86875a6232e318380514f58aadf72`,
  ZIP `45c728f7b615602aa4ef6fa96145b237670ce2d404dbe7bee8cd74c6cc34ae4a`,
  DLL `34f4e47c712443d3492c2ee4d9c8eac6a36351d56e8a5691494f1d0a1d786e4a`,
  MVID `430fe348-dce7-4be1-9e2f-b0d862557392`) passed the aura, real four-attack
  routine, wounding-sting poison, native cadence and two-save cure, then run
  `20261031T0156018470556Z-c3e0b076e3dd4b7892f8d9d0e3472e4c`
  proved its disposable target did not initially attack the Summoned faction.
  The exact build remains under
  `successful-development-candidate-eb9cbf4a-exact-build`; the failed native
  transaction is preserved under manifest
  `C0FF1415B3EEAA63896CE3784316F65CDC4C6AC2C1E524D1AD298FF1C984B740`.
  Extending only the request-owned private faction to Summoned, with exact
  restoration, established reciprocal hostility on the next candidate.
- Exact candidate `b9f755726bff4ece850a6c7322655f3cb451536e`
  (source state
  `1683ffc0d7cc330e780b2204ead9332467cabf50a603ed9da9e8b5f1f552bc98`,
  ZIP `4af94c3cf2c8895affb4fe8e83f4ba1de169d7d58bb4d921a78b887f68e0924e`,
  DLL `e9a4e12a0dadfaa9b4f24a890b1e3600326d67445a3cddf0177ebca9bb51976b`,
  MVID `fbca79ca-eb67-4dc3-941d-196f1f8f2811`) again passed all Bone rows through
  poison cure and proved both hostility directions, yet real bite completion
  in run
  `20261031T0213584995495Z-b0a9a48b27d54b37ac63554539f09c21`
  left the cloned native Invisibility effect active. The live tree restored to
  intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  its 303-file copy-only archive is verified by manifest
  `9E243F3A2FD269C5C811FC2EC2ABF97FAE9726239D1309B7B3F27B65D5948B4A`,
  and every C: original remains. The bounded correction appends one Bone-only
  buff identity, deep-clones the installed ordinary effect, explicitly keeps
  `NotDispellAfterOffensiveAction=false`, and retargets exactly one native
  apply action in each standard/quickened ability. Both AI guards now name the
  owned effect. The observer records that component flag and both actors'
  `IsInGame` state. The focused production compile and selected Sprint 35
  group pass after this change (26.1 s / 5.0 s); the initial sandboxed
  invocation stopped on generated-artifact access before producing a verdict,
  while the approved identical invocation completed. The first attempted
  `Build-Local.ps1` call omitted the required development-purpose arguments;
  the unchanged Sprint 34 release guard refused the changed product before
  compilation or packaging. The corrected development call then exposed the
  actual missing dependency contract: the Python foundation plan had not yet
  mirrored the appended identity. The repository allocator appended and
  activated exactly
  `KMG.Summoning.Infernal.BoneDevil.InvisibilityBuff` as GUID
  `14899cf428314b63afbad5bf3cea99eb`; validation now derives 2,638 foundation
  rows, 7,656 active and three retained reserved identities, with 4,021
  alternate-source adapters. The post-allocation focused production compile
  and selected Sprint 35/correction group pass (26.1 s / 5.2 s). Neither
  refused build produced a package, DLL candidate or deployment. A new exact
  candidate and only the affected Bone native group are next; Wall of Ice
  still requires the explicit bounded GridGraph gameplay decision above.
- The first development build from `d344d6b5c048d32fa087c29b55ad84cbbf4ba310`
  passed the exact append-only development validator (`7,659` total identities,
  `74` appended after v0.0.172), then stopped in repository validation because
  the canonical icon catalog still pinned the pre-append ledger hash. It
  produced no package or deployable DLL candidate; the partial validation
  directory remains in its immutable commit/purpose location. The correction
  changes no image, consumer, art count or approval: it updates only the
  catalog's exact registry SHA-256 to
  `70bf21168b253683d351c0d83fff54ed6ef1a7c12cbe51a65614ed9ea4ce1534`
  and its already-present Sprint 35 authorization from 73/46 to the actual
  74 appended identities/47 private dependencies. Read-only icon validation
  and all 30 corruption/positive tests pass; the independent summoning
  manifest again derives 2,638 foundation rows and 4,021 adapters. A fresh
  commit is required before the single reusable development package is built.
- Exact candidate `6042822cfa46573fc74e7f87fc0c226f808047de`
  passed development admission, the selected 9/9 Sprint 35/correction tests,
  exact-reference compilation, strict packaging and package validation:
  source state
  `7e61ca8fcbcca800a7adcc8583c0a27729c48eba1ab07cabcf8b9fee7f5c8d97`;
  ZIP `47ce8c74c39c7c602e9b31b13f3222cf8a1ed8fa6c71b1d1031f1531cdaee86f`;
  DLL `336f246c2cbeb2bc51eb4c09025301b1cb6044c5901c0aa9ef8e77f9033f6a2c`;
  MVID `667daddc-cb11-46eb-bd2d-a45b4101f123`. Its 1,236-file exact build
  (518,756,208 bytes) is preserved on `E:` under manifest
  `93FC963B33E6F432AE5502F8E4750F8B0379236A449D7E446ECD2D14DAAAC579`.
  The first native preflight refused before deployment because the archived
  workspace test output was absent; its vacuous restoration record is
  `20261031T0238183780336Z-disposable-expanded-summoning-sprint32-native-review.json`.
  Restoring the exact 54 test files from the archive satisfied that supported
  preflight without changing source or candidate bytes.
- Guarded run
  `20261031T0239099154616Z-dd16a6ed79f14544bcbd97860cfa2c92`
  then proved the Bone-only owned invisibility mapping, ordinary
  attack-breaking flag and reciprocal private hostility before an observer
  precondition failed. The second aura source had no invisibility command or
  owned invisibility buff in the complete event trace, but it was summoned at
  the caster position after that caster had deliberately been parked beyond
  the presented combat lane. Its healthy native state (`CanAct`/`CanMove` and
  no appearance buff) remained hidden by `EntityFader`, so the shared visible
  appearance helper timed out before the behavior rows. This is not evidence
  against the invisibility correction or the model. The observer-only repair
  moves each disposable extra aura source beside the already-visible first
  Bone before waiting for its native fade, then moves it to the intended aura
  position; it never writes renderer or fader state. The wrapper restored the
  live tree to exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no game process remains and no save was staged. Candidate, raw FAIL,
  deployment, both backups and restoration are preserved as 303 verified
  files (62,384,155 bytes) on `E:` under manifest
  `DED36EED3A736DF471E18481999C1C48D8C5198590D5C2E6FA73C88F0BEC9759`;
  every C: original remains. The unchanged candidate will not be relaunched.
  The first focused invocation was denied only while writing generated
  `artifacts/` in the sandbox and produced no verdict; the approved identical
  invocation passed production compilation in 25.9 seconds and all 9 selected
  Sprint 35/correction tests in 4.8 seconds. A new source-bound package and one
  affected Bone rerun are next. Wall of Ice remains stopped on the explicit
  bounded GridGraph gameplay decision.
- Exact candidate `869d1e649548a5ef842ff5d2861acf9fcd479e9c`
  passed development admission, selected 9/9 tests, exact-reference
  compilation, strict packaging and package validation: source state
  `cac7df4b3d6c1ba6de012312f30c638a563a4a044b130a4b7952ed35f6022b02`;
  ZIP `f6084ade6b8faf257839d8284d868024d115d47d3e3d1aa9b1bf1498c4df376b`;
  DLL `f68ac8ca42b048c5dd72ae7b1ab15f5bc7a63f7df8b185f672a2382e9a0dd777`;
  MVID `e1e3a803-6ff6-4744-af64-91755b2ed9a8`. The successful exact build is
  preserved on `E:` under manifest
  `49BE7261554357B61296772CE570722DA85F80142EAABA4AA65B11625C135439`.
  Two earlier attempts on the same source were preserved separately: one ran
  out of C: space while creating the deterministic ZIP (manifest
  `85B3B810595E4EE454419B2083D34DAB430634616F089BE438C38F3EB845E892`),
  and one created the same final ZIP before validation exhausted default C:
  temporary space (manifest
  `AB2B97FF7B3782DC0C667304D455E5E27504F320D037DCC3242E527D171F8DA3`).
  The successful invocation used the supported exact E: reference root and a
  process-scoped E: temporary directory; neither changes candidate bytes.
- Guarded run `20261031T0324061320699Z-964c0340b0b64272b9b9d79a20998cfe`
  passed native appearance, private hostility, fear-aura isolation, the
  other-limb poison negative, and the real wounding-sting poison row. Its sole
  behavior failure was observer accounting at the two-save cure boundary: a
  natural 20 made the scheduled cadence exposure the first successful native
  Fortitude save, the following native save also succeeded, and the poison
  correctly disappeared, but the observer reset its trace index between those
  saves and demanded two more. The observer now evaluates the consecutive
  save sequence across that boundary and records both subsets; production
  poison code and balance are unchanged. The wrapper restored the live tree
  to exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no game process or lease remains. Candidate, raw FAIL, deployment, both
  backups and restoration are preserved as 303 verified files (62,515,392
  bytes) on E: under manifest
  `455DF326397E1D3A06A788A763692AAAAF6B01A91A4F033FA50F8AF1EB6FC606`.
  To recover build headroom without removing evidence, the nine exact
  current-sprint runtime-backup directories were NTFS-compressed; all 1,224
  before/after SHA-256 comparisons matched and C: recovered 431.5 MiB. The
  unchanged candidate will not be relaunched. A new source-bound package and
  only the affected Bone group are next; Wall of Ice remains decision-blocked.
- The observer correction's first focused pass compiled production and passed
  all eight Sprint 35 tests, then exposed a stale neighboring append-only
  aggregate: `SummoningCurrentSurface` ended at Sprint 34 and did not include
  Sprint 35's 2 units, 5 roots and 47 private dependencies. The bounded repair
  adds one named `InfernalRules35.AppendedCatalogIdentityCount` (54), derives
  the legacy aggregate from it, and verifies the actual current identity
  catalog in the existing Sprint 35 route test. It changes no GUID, ledger row,
  publication state or gameplay. The identical focused filter then passed 9/9:
  production compilation 48.1 seconds and selected domain tests 8.9 seconds.
  The compiler helper created by each completed gate was stopped after its
  work, releasing its pagefile commitment without touching Steam or another
  process. Reproducible focused-build scratch was hash-archived before removal:
  the failed-neighbor pass under manifest
  `FE23A032536577C9E5B1803BD574C1CB196E41BBB369957125563267B16D92E3`
  and the final passing `bin`/`obj` under manifest
  `027D96729EE97214D046519F7312FEA642EFC7DB243D89A07F9A354A5575CD6E`.
  Retained test output remains available for guarded preflight. The next action
  is one new exact Bone candidate from the coherent source, not a rerun of
  `869d1e64`.
- Exact development candidate
  `738196b68a19b4f529c63a26ac8b53a73d0dd321` passed development admission,
  selected 9/9 Sprint 35/correction tests, exact-reference compilation, strict
  packaging and package validation: source state
  `c47dbe7a99bd7a4a5563a837d1039e128481151f7abdc042ab90f1ef59fd9428`;
  ZIP `c85521922efc89390b141fd0c3f0355b86cf25a1ec5b08dde4267b00aec8170d`;
  DLL `a79348a6d5675b03c3c6f37b6f00d855ae23e00ca74745dd2d7295a449beab9c`;
  MVID `275b5454-7cda-4e5a-83bd-cacfaf1853e7`. Its 1,270-file exact build
  (435,047,760 bytes) is preserved on E: under manifest
  `F4FDB6F80E8D180D446EE54393B04CE13EF0A6BBA64BA79F5278DA888F779117`.
  An earlier attempt exhausted C: during package asset copy and produced no
  package or receipt; that 1,273-file envelope is preserved under manifest
  `B5A2121F04C4490077BB9CFCAEDE6D076E69207DEDEF2DA0991995DAD0F988BE`.
  The successful retry followed byte verification and removal only of exact C:
  duplicates already present in the named E: archives; runtime evidence and
  backups were not removed.
- Guarded run
  `20261031T0411208849757Z-e8e55c3f8fed4dd69cf259ca7e6d36d2`
  passed native appearance, private hostility, Bone mapping, fear-aura
  isolation, the other-limb poison negative, wounding-sting poison, and the
  corrected two-save cadence/cure row. A real standard Invisibility command
  then applied the owned effect, but a real enemy bite that hit for 13 damage
  did not remove it; the command completed successfully and the observer timed
  out on the still-active buff. Installed 2.1.7b IL confirms that native
  `BuffInvisibility` is the offensive-action subscriber and carries mutable
  `Fact`/event-listener backing state. The bounded production fix creates only
  that component fresh for the owned buff while retaining deep clones of the
  donor's other components; rules, abilities, resources and identities are
  unchanged. The focused production compile passed in 53.5 seconds and all
  eight directly selected Sprint 35 tests passed in 9.1 seconds. The wrapper
  restored the live tree to exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no game process remains and no save was staged. Candidate, raw FAIL,
  deployment, both backups and restoration are preserved as 303 verified
  files (62,556,899 bytes) on E: under manifest
  `875E5DCDB399EEF1524EDFB304D8BB0C4FC7E72D596E55CD97102A3165863996`.
  The unchanged candidate will not be relaunched. One new source-bound package
  and only the affected Bone group are next; Wall of Ice remains stopped on the
  explicit bounded GridGraph gameplay decision.
- Product commit `004cb6fa835a8d24636f054dff1a2289e5bedcd6` contains the
  fresh native `BuffInvisibility` subscriber fix, is pushed normally on the
  exact Sprint 35 product branch, and has a clean worktree. Its Focused gate
  passed production compilation in 53.5 seconds and the direct Sprint 35
  selection 8/8; the subsequent development admission selected the intended
  Sprint 35 plus correction-ledger group and passed 9/9. Exact-reference
  compilation produced a compile candidate, but packaging stopped at the
  first asset copy with `There is not enough space on the disk`; it produced
  no final development ZIP or receipt and is not a native-test candidate. The
  584-file incomplete envelope (256,948,907 bytes) is preserved copy-only on
  `E:` under manifest
  `855725C4A8BEB59F0B5BFEC6487335B5BED8D4CCE52AC8F1BBEFD0F14403AC`.
  Only that incomplete commit directory and its reproducible `bin`, `obj`, and
  staging scratch were removed from C: after hash verification. The unchanged
  package attempt will not be repeated under the same storage conditions.
- Three retained but reproducible C: trees that would provide the smallest
  measured packaging headroom are now also preserved copy-only on `E:` before
  any deletion decision: `artifacts/deploy-staging`, development candidate
  `738196b68a19`, and `artifacts/tests`. Their combined 500-file archive
  (193,865,872 bytes) is verified by manifest
  `142F1FF58A11BB2F587ABDAAC447BB04B03544B742DF3992EC9245FFA476FD12`.
  Nothing in those retained trees has been deleted; explicit storage approval
  is pending. The laptop remains at exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`,
  with no game process, lease, transaction, or staged save.
- The mission-selected independent read-only review has so far found the
  Sprint 35 lifetime split coherent. The source-owned aura and active commands
  retire with the exact summon; poison, Strength damage, frightened duration,
  and source-specific fear memory do not carry the Sprint 34 source-removal
  component. The real Bone observer already records an allied summoner inside
  the five-foot area and therefore proves the aura is not enemy-only despite
  its harmful-area metadata. Original model instances remain per-view private,
  and disabled-module registration does not load their resources. Package
  validators pin all four new model exports. The same review confirms that no
  Sprint 35 mixed ON/OFF persistence scenarios or Sprint 35 qualified-release
  selector exist yet; both remain mandatory final-candidate work. The push
  wrapper already contains only the two exact authorized Sprint 35 branches,
  so it needs no allowlist edit. No persistence or publisher expansion will be
  frozen around the known-incomplete Wall candidate.
- Exact candidate `ecdbb7ee6fc53bc801a087b46181d63c11b1faaa`
  packaged and validated successfully before two guarded Bone-only attempts:
  source state
  `29377aaa19bd435b0633e125d3286634cf0e0952e41bd48c4f009ec27a8cbd66`,
  ZIP `e5e6f0e0c14556eaaa7f7158c8b3317336615d89d61192b00bf3ae55eb65a5b3`,
  DLL `105c50448bed5f97daf134b84aa8c2ed9035ea9d769603af01a8e3bd7e53f269`,
  MVID `3b195405-095c-4e5d-acd1-37a29d6876ae`. Run
  `20261031T0502069514965Z-ca181a64432d42b89f8dc4bde4dd026b`
  stopped before fixture entry on cold-start menu readiness after the installed
  compatibility catalog alone took 91.9 seconds; no save interaction or Sprint
  35 mechanic occurred. The one evidence-backed retry enlarged only startup
  allowance. Run
  `20261031T0508174943855Z-99bbe4daaa224fd984429eeb2ddb3f73`
  then passed private hostility, appearance, mapping, allied/hostile two-source
  fear aura, non-sting poison negative, the complete four-limb attack plan,
  wounding-sting poison, cadence and two-save cure. Standard Invisibility was
  applied by its real command, and the subsequent real bite hit for 6 damage
  with native `UnitAttack` result `Success`, but the owned effect remained.
  Both attempts restored the live 136-file intake exactly to SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no process, lease or staged save remains.
- The second failure disproves the earlier unverified theory that copying the
  donor component's runtime `Fact`/listener fields was the cause: historical
  installed graph evidence records the native blueprint component with null
  `Fact`, `IsListeningEvents=false`, and serialized stealth bonus 20. The fresh
  component also dropped that serialized field, but the prior complete clone
  failed the same removal. The next candidate therefore adds observation only:
  live component/fact/owner/listener identity plus the installed global and
  exact-owner offensive-action dispatch. It waits for the successful command,
  records the failed removal cell, and stops; it does not invoke the handler or
  supply production behavior. Focused production compilation and all eight
  Sprint 35 tests pass. The shared compiler server is disabled for this laptop
  envelope after two measured attempts showed it retaining about 160 MiB of C:
  backing space; compiled bytes and test semantics are unchanged.
- All storage recovery preserved exact bytes before removal. The incomplete
  diagnostic attempts and their changed partial test/build outputs are under
  `E:\KingmakerGunslingerLab\s35-private-references\invisibility-diagnostic-gate-ecdbb7ee-incomplete`;
  primary bin/obj manifest SHA-256
  `8D9219C365A8BE2DBC5917D717FEBF2AD7EDAC7E2B7D994A6A373975D26C6058`,
  current-candidate manifest
  `57FB8CC5DCC20B6D7AE2CF0E4CC04D10882629029B94EEFA423A41B4B6E7D1ED`.
  The final cleanup reverified 891 current files before removing only
  reproducible `bin`, `obj`, `deploy-staging`, `tests`, and old development
  candidate trees. Source, native evidence, saves and unique artifacts were
  not removed.
- Diagnostic commit `d2b5b8eeef107c0c64a39dbf1c87e5cc9ceef76d`
  passed development admission and the intended 9/9 focused domain selection.
  Its exact compiler produced compile-candidate ZIP SHA-256
  `74FA3247C446A3D690D331910F5683FE2448B57347937768417D4919D654E549`,
  but the outer package copy stopped at the armored-dinosaur assets with
  `There is not enough space on the disk`; no final development package or
  receipt exists, so the guarded runtime path cannot admit that partial
  attempt. Its 1,050-file, 350,104,357-byte envelope is preserved on `E:` under
  manifest SHA-256
  `96397525E9CA62AC898DE3CD05D7406409C85D577080A1076017DE3B249462A0`
  before removal of only the verified C: `bin`, `obj`, `tests`, `staging`, and
  incomplete development-candidate copies. To create measured headroom without
  touching Sprint 34 or any frozen/local-runtime artifact, only the closed
  `s24r155` worktree's reproducible `bin`, `obj`, `tests`, `staging`, and
  `deploy-staging` copies were separately preserved on `E:` (1,111 files,
  247,994,518 bytes; manifest SHA-256
  `B9E783EA079FB858CC9FEE8041361A6B9C960D127F6CB95576600CDDAE9D94AE`)
  and then removed from C:. Source, its local-runtime archive, packages and
  evidence remain untouched. This raised measured C: headroom to 960,954,368
  bytes. Because `Build-Local.ps1` has no supported partial-package/receipt
  resume and the first run produced no admissible candidate, one unchanged
  source-bound completion is required under the larger envelope. Expected
  result: identical compiler identity plus a validated package/receipt, which
  closes only the changed-observer admission needed for the narrow Bone event
  dispatch diagnosis; it does not broaden mechanic qualification.
- Source-bound diagnostic candidate `ff40f69e232a9247ee04a200dc9606cf0b2edb88`
  completed the supported development path: source state
  `4795201c40f2eb84d4d2258f502d69d4b2054d86b75861545dd6644e5524a21b`,
  ZIP `dbee716c3000b4ed434e2d430aaaeb72b56626005c61f67ed6db46b3739da0c0`,
  DLL `6d4f6d7205bf7d2318e911a3d21eee947da59b2c8297a9d1c02abb91b925fbda`,
  MVID `0c0f585b-843b-4a73-ba84-86ec29c26972`, and the intended 9/9
  selected tests. Its 1,068-file exact build and reproducible build/test/staging
  envelope (364,397,748 bytes) is preserved on `E:` under manifest SHA-256
  `D08DF495549938E25360FA73B4F8FD9DCC344C61988B900D0769E6E1244C487C`;
  the admitted ZIP, checksum, receipt and selected logs remain at their receipt
  paths. A first guarded dispatch was refused before deployment because its
  archived domain-test validator was not present; intake stayed exactly
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  Restoring the exact 54-file validator tree from that archive closed only that
  package-admission prerequisite.
- Guarded run `20261031T0559168539811Z-9006344faf9e4ed8b387b5113dbe066f`
  produced a complete immutable native FAIL after the launcher had already
  reached its 120-second result-wait boundary. All prior Bone rows again passed,
  including real hostility, both appearances, allied/hostile two-source fear,
  non-sting poison negative, bite/claw/claw/sting plan, wounding-sting poison,
  cadence and two-save cure. Standard Invisibility cast successfully, both AI
  actions refused redundant recasts, and a real bite hit the exact hostile for
  7 damage. Immediately before and after that hit the live native
  `BuffInvisibility` reported `listening=true`, exact Fact/Buff/Owner identity,
  `NotDispellAfterOffensiveAction=false`, active/turned-on/non-disposed state;
  nevertheless both global and exact-owner offensive-action observations were
  empty and the effect remained. This disproves component hydration/listener
  identity as the cause and demonstrates that the installed
  `OffensiveActionsController` did not raise its global or actor-local event for
  this real attack in the actual first-area contract. The observer supplied no
  event or removal. Evidence is under
  `20261031T0559167699797Z-disposable-expanded-summoning-sprint32-native-review`.
  The outer wrapper restored the 136-file installation exactly to intake and
  released its compatibility lock after the game exited.
- That timeout exposed a separate host cleanup defect: the wrapper released its
  own stale shared lock but retained the generic `RecoveryRequired` journal, so
  the printed completion command then failed while reading the absent lock.
  The bounded repair in `RuntimeCoordination.Common.ps1` permits only an exact
  `RecoveryRequired` journal, after both recorded processes and every Kingmaker
  process are gone, to atomically recreate its own lock with the existing
  `CreateNew` owner primitive; active, intentional leave-open, racing and
  foreign owners remain refused, and `WhatIf` creates nothing. The isolated
  lifecycle test now reproduces that ordering. Validation and exact completion
  of retained run
  `runtime-20261031T055914Z-bc09689669aa4f519da72cf8be6d0746` are the next
  actions before any further native dispatch. No manual coordination-file
  creation or deletion has been performed.
- Host cleanup validation is complete. The actual launcher/coordination fixture
  passed 41 assertions, including present-lock leave-open, automatic exit,
  interruption, timeout, foreign/busy ownership, inherited ownership, process
  identity reuse, competing recovery, missing-lock `WhatIf`, and atomic exact
  missing-lock recovery. It used the untouched released v0.0.172 ZIP SHA-256
  `EF82D2028C636A00EAEBF2536FE6123921D1551716EA4A6CD9D6FFA75AC47D86`
  only as its disposable external package boundary; no real Steam/game process
  or installation was touched. The passing 465-file fixture is preserved on
  `E:` under manifest SHA-256
  `3D2E6D84BA70D1FCC2A3CC79E2C6FBEF3089E3C10EA7E5984DDB410BE9B7013C`;
  the earlier dirty-tree/missing-candidate fixture records are separately
  preserved under manifest
  `87C9E7851D546D7C565B6D71D7FB1A1A4DB6668558AFC1AEA4087AC390E42925`.
  Retained real lease
  `runtime-20261031T055914Z-bc09689669aa4f519da72cf8be6d0746`
  then completed through that validated path with native outcome unchanged.
  Its journal is `Completed`, `recoveryRequired=false`; no Kingmaker process or
  compatibility lock remains, and the live 136-file installation is still
  exact intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
- The conclusive Bone trace makes the correction smaller than an engine or
  controller patch. Bone's private ordinary-Invisibility buff now retains the
  complete deep-cloned native `BuffInvisibility` (including its serialized
  stealth bonus) for ordinary offensive spell/ability dispatch and appends one
  exact-owner `RuleAttackRoll` fallback. A live hostile attack attempt, hit or
  miss, removes only that active private fact; foreign owners, inactive facts,
  actors outside the active game and non-hostile targets are rejected. The
  fallback does not raise or synthesize an offensive-action event. Focused
  validation compiled production in 46.9 seconds and passed the exact Sprint 35
  selection 8/8 in 9.2 seconds; full, package and native claims remain deferred.
  The next native rerun is justified only by this causal production change:
  the same real Bone bite should now consume the effect through the measured
  attack-roll boundary while the previously passing fear, attack-plan, poison,
  cadence and resource rows remain unchanged. Only that affected Bone group is
  selected; Wall of Ice remains stopped on the explicit gameplay decision.
- Exact candidate `24b955bdb22edfae8516e5e463d2d35063eaf9dc`
  completed the supported development path with source state
  `619156c1330a255240a6b53d2b1b87e19143791cbd153fbd092bec0a8b8be83f`,
  ZIP `9b95a8a3d8d0b678c539c08eda5ccd463b829a531329d7bb17edd1ea8668536e`,
  DLL `62ea58a4b758f4d4f8367aaf6aefefba8183301e5bd32d28cfb73402251045bc`
  and MVID `fe53c788-e70a-4425-8226-3fccf96c3ac1`; its selected tests passed
  9/9 and strict package validation passed. Its 1,067-file exact/reproducible
  envelope (364,406,336 bytes) is preserved on E: under manifest SHA-256
  `9253160D1D6A12EDBBBA48D54A1A9B8C82C1E24D60EA71157896A67C6A3F914B`.
  Native run `20261031T0636450086522Z-08e88814ed8646b9b74bc0aa621e078b`
  proves the attack-breaking correction: standard Invisibility and all three
  quickened uses were applied by real commands, refused active-effect recasts,
  and were each removed by a real hostile bite; every attack-plan, fear and
  poison row also passed. The only failed gameplay row found a separate AI
  admission defect after the third quickened use: the real resource was zero
  and `AbilityData.IsAvailable` was false, but `InfernalSpellAi35.IsAvailable`
  still returned true because the native base action did not inspect the
  granted ability's exhausted resource. The exact-owner wrapper now requires
  the owner's real granted `AbilityData` to be available before deferring to
  the native base action. This preserves active-effect refusal and also applies
  the same finite-use rule to the other Sprint 35 SLA buttons. The next affected
  Bone rerun is justified only by that causal AI change: all four invisibility
  removals should remain true and final quickened AI availability should become
  false. The prior FAIL is retained, the live 136-file installation was restored
  exactly to SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`,
  and the unchanged failed candidate will not be relaunched.
- Exact finite-resource candidate `f503f871b4e6c4296c669c32ea40c81d5bdf6cdb`
  completed development admission with source state
  `58e06e19758c67e38ce191171dd9165cce7e5b9cb4bac2b1c2fb7cb03dd7320f`,
  ZIP `9d992bfcbfa6fffe150bb3905a5633026622f52c4570ed30a5c17ab4bd829a07`,
  DLL `543de24b2651c41dc6cf5d5f2b0fa8cbc85062f72a53b0e5e82878b727feb9b1`
  and MVID `ffed1c76-f6a5-44b6-a9dc-97fa053fc506`; selected tests passed
  9/9 and strict package validation passed. Its 1,067-file, 364,414,946-byte
  exact/reproducible envelope is preserved under manifest SHA-256
  `2B787642E6C7ABC59C827266CA67AE07ADFBDD9B39EAFEF30B24F1FB433B4E35`.
  Native run `20261031T0659029746015Z-2f67a1f246f848a9b4dd02acbdffad89`
  stopped before invisibility on an unrelated chance-sensitive observer
  assumption. The allied summoner naturally failed its DC19 aura save, but the
  observer checked the failed-save source marker only after later hostile and
  second-source setup. That marker deliberately shares the rolled 1d6-round
  frightened lifetime, so a short natural result may expire before the late
  check; earlier successful raw rows retained two-, four- and five-round
  results long enough. Production rules and rolls are unchanged. The observer
  now snapshots JSON for memory/frightened immediately after the allied save,
  separately records later presence or lawful expiry, and retains no destroyed
  Unity fact. One changed-observer Bone run is required; the unchanged
  `f503f871` FAIL will not be relaunched. The live installation again restored
  exactly to the same 136-file intake SHA-256 above.
- Changed-observer candidate `c075b41a01cd4a10ef8f7127b6dc765bfb66bcaa`
  completed development admission with source state
  `bab9a4bf4b781ba7a5204ecad7063f0f3ee9108816c7985b539901e9ac759804`,
  ZIP `acbcc20cab1b92b09cd898276306d2c246c92a97fd8c127962ecd5b8792e49c6`,
  DLL `43a23e39218d6f2973dab209c8a5b1c251b430045ac70f56025acd2c8da6fe06`
  and MVID `12ffa9bf-6f5c-4b0b-b9c5-3b855fddfa32`; selected tests passed
  9/9 and strict package validation passed. Its exact-build scratch is
  preserved on E: as 162 files / 86,709,784 bytes under manifest SHA-256
  `5D24471660A279377720BFB5E1E1D4F9AF3678FB56529D151421AD1649D14494`.
  The first archive copy used a literal wildcard and therefore copied zero
  files; verification stopped before source removal. The confirmed-empty
  target was removed and the corrected copy was fully hash-verified before
  only the reproducible C: exact-build directory was removed.
- Guarded run `20261031T0718039668633Z-70f7868af8784bb9ac4c7f379d4c2d3a`
  under evidence directory
  `20261031T0718039262813Z-disposable-expanded-summoning-sprint32-native-review`
  proves both targeted corrections. The allied fear result was captured at
  exposure with exact source memory/frightened, and the later check may expire
  lawfully. Standard Invisibility and all three quickened casts again broke on
  real hostile bites; quickened resources reached 3 -> 2 -> 1 -> 0, native
  `AbilityData.IsAvailable` and the exact-owner AI action were both false at
  zero. Every earlier attack-plan, sting-only poison, cadence and cure row also
  passed.
- The run's remaining FAIL is an observer filter defect, not a production
  poison-lifetime failure. After destroying the exact Bone source, raw trace at
  game second 43303.373 records the next DC20 Fortitude save on the same victim
  (`owner=null`, natural 19, failed under the fixture modifier) and a subsequent
  Strength-damage event whose fact is the exact private poison. Its pre-scale
  roll was 1 and the installed 0.8 hostile damage scale truncated actual damage
  to zero. The old lookup required `Context.MaybeCaster` to resolve the destroyed
  source and therefore reported the surviving buff and both source-less native
  events as absent. The corrected observer requires exact Buff-instance
  continuity, DC20, and the original caster ID retained by the existing
  serialized `MechanicsContext.m_CasterReference`; it separately records the
  expected null live resolver and native event attribution. Fixture cleanup now
  also matches that retained source ID. No gameplay code or balance changed.
  The first focused compile caught and retained one local value-type access
  error (`UnitReference` cannot use `?.`); direct access fixed it. The final
  focused gate compiled production in 46.8 seconds and passed 8/8 Sprint 35
  tests in 2.7 seconds. Full/package/native claims remain deferred. One changed-
  observer Bone run is required; the unchanged `c075b41a` FAIL will not be
  relaunched. The outer wrapper restored the live 136-file installation exactly
  to intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
- Source-bound candidate `6de8e4e2b6f3d77aa5875f8cf54a215aab06fb28`
  completed development admission with source state
  `4632babf2aa9dd769dc60b5052716caf70d5920002af04ae7050ce5e3ef13657`,
  ZIP `0debe989c2813bcc13cd619f9415e68e723eaf190f38f51fb3909c32918c99e8`,
  DLL `1c7ff009ca78f36e29878f6554961b13f8b7d1dea85aff465b99de491a041049`
  and MVID `ecb00cd9-d674-4a24-be70-19e83d947ee4`; selected tests passed
  9/9 and strict package validation passed. Its 162-file / 86,713,971-byte
  exact-build scratch is preserved on E: under manifest SHA-256
  `704BC9406FC33241AF6374DDA927D64E2F3A2D5FFDD1DAD8EF38FD772F8B4224`.
- Guarded run `20261031T0746048989268Z-8a8d4b58a5424eb590bd7e2d2032cb0d`
  under evidence directory
  `20261031T0746048510164Z-disposable-expanded-summoning-sprint32-native-review`
  stopped at the earlier aura row, before reaching the corrected poison row.
  The allied summoner entered the first Bone's actual five-foot area, failed a
  natural DC19 save and immediately received exact-source memory/frightened.
  The observer then fixture-teleported that already-established source into a
  new lane. No hostile exposure from that first area followed in the bounded
  12-second observation, while the second Bone was settled at the first
  source's already-proven position and did naturally expose the hostile. This
  is an ineligible first-source area-membership setup, not a failed save or
  production aura defect. The smallest correction keeps the first source at
  its proven position, restores the allied recipient, begins tracing before
  placement, and moves the hostile into that same established area. Sibling
  sources likewise settle at the proven point. Later attack/poison work still
  obtains its separate native combat lane. No production mechanic or roll is
  changed. Focused validation compiled production in 47.3 seconds and passed
  8/8 Sprint 35 tests in 2.6 seconds. The unchanged `6de8e4e2` FAIL will not be
  relaunched. The wrapper restored the live 136-file installation exactly to
  intake SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
- Headroom handling remains byte-preserving. The five reproducible focused
  output trees (`bin`, `obj`, `tests`, `staging`, `deploy-staging`) were copied
  and per-file hash-verified on E: before their C: copies were removed: 1,351
  files / 435,499,400 bytes, manifest SHA-256
  `E5D7CFECADE8EB7A49661C6EB1A8E07BC44E9D59329289A3F204966F6EE62149`.
  The four closed Bone development-candidate directories through `c075b41a`
  were likewise preserved as 24 files / 270,903,855 bytes under manifest
  `08D0CBC34EE0ED15B8E375F43D1CE0ADD320B2E171B9B7D76A8B1A62E0C7FC3A`
  before only those C: copies were removed. Their ZIPs, checksums, receipts and
  logs remain available at the recorded E: archive; raw native evidence and
  runtime backups were never in either removal set.
- Stabilized-fear candidate `5190828793764dd00a344731fbe2b1999e6841c9`
  completed the supported development boundary with source state
  `2c65cedba88a7824ff7d4c72b9169d80cd09f24ad54d1dfd1574ad08a6cb6757`,
  ZIP `35ee2d00663e8496cdef9dc8bb822ddc0efc2f59127e866ea3efd3ede4db6706`,
  DLL `991f6596c44e4e8739abcd7d2b7390b606ee0f47a63f58dd3e1c14e5e6195bda`
  and MVID `4932b28a-539c-418e-b34a-3030f87003a3`. Development admission,
  strict packaging and the selected Sprint 35/correction group passed 9/9.
  Its exact-build scratch was copied and verified by relative path, length and
  SHA-256 before only that C: scratch tree was removed: 162 files /
  86,713,715 bytes, archive manifest SHA-256
  `DBCEF8FDA666D8AE0CB5A61643F0B5F067F06600C538C900ADA6CABDC1E1B45D`.
- Guarded run `20261031T0805455466497Z-19d0a504f1944ee9ae17045d4e156b0e`
  reached and passed the previously failing Bone fear row, including the first
  source's allied and hostile DC19 exposures and source-specific memory. It
  also passed private hostility, mapping, four-limb native lane, non-sting
  poison negative, sting poison, cadence/two-save cure, standard invisibility,
  and all three quickened uses/resource exhaustion. The original scenario and
  poison-source-independence verdicts remain **FAIL**; immutable
  `runtime-result.json` SHA-256 is
  `9672F55285FC9CD18298B19B07FD21CDD4E5581165F7DBA181076248505500B9`.
- A bounded reader-only re-evaluation of that immutable poison row is **PASS**
  for the source-lifetime requirement, without changing either original
  verdict. The row contains every required observation: the exact source is
  destroyed; the same Buff instance remains at DC20; its live caster resolves
  null while its serialized caster ID remains the destroyed Bone's ID; exactly
  one source-less DC20 repeat save occurs; the failed save produces exactly one
  exact-poison Strength event (`1` pre-scale, native `0.8` scale, truncated
  damage `0`); victim damage therefore remains `5` as the event reports; and
  the source-owned aura count is zero. The aggregate observer nevertheless
  emitted false. Because this is the third distinct Bone observer/setup stop,
  no unchanged or immediate diagnostic relaunch is selected. The observer now
  snapshots each conjunct once and emits named subpredicates so a later frozen-
  candidate run cannot repeat an opaque verdict; production mechanics are
  unchanged. Focused validation compiled production in 46.8 seconds and passed
  the selected 9/9 Sprint 35/correction tests in 3.0 seconds; package and native
  checks are deferred to the later frozen candidate. The orchestrator restored
  the live 136-file tree exactly to
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no save interaction occurred.
- Owner decision 2026-10-31 approves the recommended bounded rules contract.
  Barbed omits Major Image, Produce Flame, Pyrotechnics, and Order's Wrath;
  Bone omits Major Image and Dimensional Anchor. Each remains an explicit
  release limitation with no substitute button or new broad framework. Wall of
  Ice may use a Wall-only temporary GridGraph adapter: an A* work item captures
  the exact prior walkability of intersecting nodes, overlapping live sections
  reference-count their claims, and the last owner restores each exact value on
  breach, expiry, source removal, module disable, load/view teardown, or area
  teardown. The adapter claims movement obstruction only, never line of effect.
- The first implementation remains hidden and changes no route or public spell
  surface. The private serialized section still owns HP and duration; its
  recreated view owns only derived navigation claims. It retains the measured
  native `NavmeshCut` path, then selects the nearest actual `GridGraph`, queues
  footprint claims/flood fill through the installed A* work-item API, and
  queues exact restoration idempotently. The early observer requires the same
  graph used by the measured path, a positive changed-node count, changed
  movement, restored path and node values, and zero residual claims. Focused
  validation compiled production in 44.8 seconds and passed the selected 9/9
  Sprint 35/correction tests in 9.6 seconds. Package and native feasibility are
  not yet claimed; one source-bound early Wall run is next.
- Hidden development candidate `70eaed6e6282bebc31ad75f797843f468ac71fba`
  completed source-bound admission with source state
  `299daabab9e06811a2a7f812164f7e8e48e1ab4034af4bf07d65676598e730a3`,
  ZIP `24de584e2eed17c84de87f11bce5b02a5ff0fefb99cef10db9790e734a443623`,
  DLL `0332a15aa2faebbcd2f2b5fe8684bb85184b4d008ec8183a6c073587bb8000ed`
  and MVID `69a651c6-b213-46ca-ab8f-093b5b40d67a`; selected tests passed
  9/9 and strict package validation passed. Its 162-file / 86,748,038-byte
  exact-build scratch is preserved on E: under manifest SHA-256
  `9349233FDAF697013EF676DC0EDE2DE1BB473572CC5C8D55C0CEE79BB8C75B2E`.
  The first archive command was safely refused because a literal wildcard
  copied nothing; the second refused the resulting zero-length manifest. The
  final transaction verified every source/destination path, length and SHA-256
  before removing only the redundant C: exact-build directory.
- Guarded run `20261031T0905102628398Z-70002a764b304663b3411c72ce63969c`
  under evidence directory
  `20261031T0905102141533Z-disposable-expanded-summoning-sprint32-native-review`
  timed out in the outer supervisor before the game flushed its result, so its
  orchestration verdict remains ERROR and no Wall verdict is claimed. The
  later immutable native result stopped at hidden bootstrap: all five routes
  were present and hidden and all 47 registered private values were non-null,
  but the observer still pinned the pre-correction count of 46. The append-only
  Bone ordinary-invisibility buff is the legitimate forty-seventh identity.
  No Wall cast occurred. The fixture references/clock and save guard restored;
  no save interaction occurred. The outer wrapper restored the live 136-file
  tree exactly to
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  The owned game process later wrote its retained FAIL, initiated automatic
  exit, and is now absent. The corrected observer derives the exact prefix
  symbol/type set from the append-only identity catalog and records missing,
  unexpected and wrong-type entries instead of pinning another stale count.
  Focused validation compiled production in 58.6 seconds and passed the
  selected 9/9 Sprint 35/correction tests in 5.5 seconds. The observer DLL
  changed, so the late raw FAIL is retained and a new source-bound early run is
  required; the unchanged Wall production implementation is not yet proven.
- Corrected hidden candidate `12cc945620f2bc69846ad3c3153e434fa7a0de85`
  completed admission with source state
  `d8be5bf3036d4ef56233f437cfb2a247701157453aa4cb839c12c55b06c94f88`,
  ZIP `febfee434a2a5e4c2629ba7855b18f5959cf794ec10ca4c285e5003544f7831f`,
  DLL `8f073cb560d35da28facea8051d4f9726ad17ee2085cb0f0425d9f1ed01918fa`
  and MVID `47bd27dd-93d0-439d-81b2-5d2eafc4b9f8`; its 162-file /
  86,760,550-byte exact build is preserved on E: under manifest SHA-256
  `51D39F6BA8B6708FE0DABBFA26DAA7C17DBC69DF34D649A227CB0AEA23C0992C`.
- Guarded early run `20261031T0934466325823Z-6025f197cc1c4588baad34f4d3173e2b`
  under evidence directory
  `20261031T0934465734697Z-disposable-expanded-summoning-sprint32-native-review`
  passed all seven selected cells. The catalog-derived bootstrap found exactly
  47 expected/actual registrations with no missing, unexpected or wrong-type
  values, while all five routes remained hidden. The actual unit path used
  GridGraph 1; the Wall work item considered 32 nodes and claimed/changed 14.
  The route changed from two points / 6.0080795 m to six points / 7.1193864 m.
  Section retirement restored every captured walkability value, the original
  two-point route, and zero active claims. Line of sight did not change and is
  not claimed. Runtime-result SHA-256 is
  `6C4ACBEFE7793CEE7684D9A8F7FCCDAA6210EE499B9F836F28448A7159B4DAEB`;
  focused receipt SHA-256 is
  `146B06C0E295555AD85F19F2896CE6FF918CD204F1CB5ECE377655B421B61362`.
  The fixture and native empty world restored, no save interaction occurred,
  the live tree restored exactly to
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`,
  and no Kingmaker process remains. Feasibility is closed; formation geometry,
  Reflex/SR disruption, breach/residual cold, AI and persistence remain open.
- Changed hidden candidate `160c282a4352f9321a5190eb9e1ebb75aebaa092`
  completed source-bound development admission and strict packaging with source
  state `3c72e72a3e6018d096fa0149d0c3bcb4d0b933b3603473224146a3a2d5dcf919`,
  ZIP `8d5fae13fb8da2a01a092a5c9f3095b29089636a78ac6451522de5bf406ba18a`,
  DLL `ac54ccb8f79a235a20b5a411e4076e4a21c587c4904fb6872d598159f5a1199d`
  and MVID `cd4e2b01-6481-4d0e-aceb-f18f6844e159`. Its selected Sprint 35/
  correction group passed 10/10 of 2,776 registered tests. The exact build and
  reproducible outputs were copied and hash-verified on E: as 1,061 files /
  350,208,393 bytes under manifest SHA-256
  `D74F3F831C3D53A447D2480116C99C2F83CC4D685892421029EC2A12651C349C`.
- The first guarded invocation for that candidate was safely refused before
  deployment because the production package-validator executable had been
  archived with reproducible test output. Its exact 54-file / 17,696,768-byte
  directory was restored and hash-verified from the E: archive. A following
  run `20261031T1020362913589Z-41723678fae14377b07879542af56146`
  retained raw FAIL SHA-256
  `7B560F4B74BCE631CE92C345FB690938DD59F4C0FC041082432A5619116A15A1`:
  the runner's first main-thread update arrived after about 206 seconds, beyond
  the request's 180-second startup limit, so no fixture event occurred. The
  causal request limit was raised to the documented 600-second native budget.
- Widened-startup run
  `20261031T1026147333330Z-0ad5ddc599074adf8ce2679158bcdb8a` reached the
  selected phase and retained raw FAIL SHA-256
  `B250840B47823E03874787388289E6759C2345EEFDAA7DF97395E0A3FC3C3B90`.
  Hidden bootstrap (47/47), both exact profiles, both original bodies and the
  private Bone tail passed. The unchanged Barbed contact replay then exhausted
  eight lawful native misses against AC 26: `6+8`, `13+6`, `9+8`, `17+8`,
  `4+10`, `13+8`, `11+10`, and `2+8`; Wall was not reached. This is an
  ineligible chance precondition, not a mechanics verdict. The redundant replay
  is removed from the changed Wall observer, which cites the unchanged exact
  contact/reach PASS `20261031T0029267333965Z-bf97d85fd26643959cb5e767b26e56c1`
  and explicitly equips the real Bearded glaive witness before Wall. Both
  guarded runs restored the 136-file live tree exactly to intake
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no save interaction occurred and no Kingmaker process remains.
- Wall-scoped candidate `400bf48daf4b4ce3eb5f9f60e8ac5428ff7a0bb3`
  completed development admission and strict packaging with source state
  `20aef7fd622445087a256cecbdafea8bb558121846bef71ddc4c9e0dd5a7e129`,
  ZIP `a08486c1b2d80ed19274e7131338189222e0cdd9851f708c3e1ea74405f21a6c`,
  DLL `673580dd69adcabb668964950F46DF8DD2F82FAD61321D74B4EB0EF5AA0441FD`
  and MVID `34fabee1-1533-4971-97da-70ddd645d7fe`. Its exact build and reproducible
  outputs are preserved on E: as 1,014 files / 347,039,031 bytes under manifest
  SHA-256 `1427DB10501A85B074523687D0BFC034FC2FC688486337583D3EC414EA4FAACA`.
- Guarded run `20261031T1046434335583Z-3ce1155614764c65b704040bb4d75cf9`
  retained raw FAIL SHA-256
  `85A2006453ECF8FD28BE67D1922F10E9C8CA3FDA1C3AA53150FE158EAC1444CA`.
  It proved the changed cast reaches native success, creates one correctly
  oriented visible 36-HP section with two donor renderers suppressed, claims
  and changes 22 actual GridGraph nodes, and changes the real movement route;
  line of sight remained unchanged and is still unclaimed. The physical witness
  was the Bearded Devil just used as Bone's full-attack target and its attack
  command retired before start. The fixture's direct fire packet used a zero
  dice formula plus `PreRolledValue`; the installed damage consumer reported an
  unhalved fire packet with zero bonus and zero damage. The section therefore
  never breached, so residual/fog, formation retry and restoration could not
  occur. The correction moves Wall before the tail/full-attack observation,
  admits only a live acting witness, settles it for one frame, and initializes
  the explicit zero-dice fire packet with native `AddBonus(1000)`. Production
  Wall rules are unchanged. The wrapper restored the exact intake and no save
  interaction or Kingmaker process remains.
- Observer candidate `32aec478e14b50311cb44f22ef599d11edcfe442`
  completed source-bound development admission and strict packaging with source
  state `a96cf4b27625ef5bfcd3e6e336fc9a6c35b36df3d81551b9e8e84e31f8e7753e`,
  ZIP `6f6a0d48887e00231c35ee65fbac8c61df81325335f2bf69e89f60a4585636b4`,
  DLL `abc35f474482b87e01bb7a1f4c35f8fad391fbe70d4dce912b0a968c863e679a`,
  and MVID `7fd06f43-4f48-4601-95f9-7dee7c66e0c4`; its selected group passed
  10/10 of 2,776 tests. Two unchanged attempts stopped first before receipt
  creation and then during strict validation because C: lacked the measured
  package/extraction envelope. Their exact owned outputs and all older Sprint
  35 development candidates were hash-verified on E: before C: scratch removal.
  The successful exact-build/bin/staging set is preserved as 1,007 files /
  332,511,538 bytes under manifest SHA-256
  `78D0F178F0E25A5912BC64AA45A133D93909EC1938F81906037D2CB7095E5766`.
- Guarded run `20261031T1138284216733Z-e31293970ac1436f8ad95f9fa377b385`
  under evidence directory
  `20261031T1138283611589Z-disposable-expanded-summoning-sprint32-native-review`
  retained raw FAIL SHA-256
  `BA077DC4260D069B8C4D4A090460111EDBAF49BF2110BB8D62A9DF63C2111573`.
  The real cast again succeeded, the visible oriented 36-HP section claimed and
  changed 22 GridGraph nodes, and the route changed. The Bearded witness was
  alive, acting, enemy, in range, and `CanStart`, but the native action
  controller interrupted its attack before start because the observer had not
  entered the attacker and inert section into a real combat lane. The explicit
  fire packet was unhalved and carried bonus 1,000, but the retasked summon
  source resolved zero damage; no breach meant no residual, formation retry, or
  restoration verdict. The bounded observer repair now reuses the qualified
  wake/combat/controller setup, uses the independent fixture hostile and
  deterministic positive native fire dice, records raw result/immunity/final
  values, and waits out Bone's standard-action cooldown before formation.
  Production Wall code remains unchanged. Focused production compilation and
  the selected 10/10 group pass. Early evidence SHA-256 is
  `DFF9B26D60557EA24F89DC9AF4642A8800304CE9A49DE6CA0B3E799E2DC91B3B`;
  restoration SHA-256 is
  `6191F724B2BECE8557CAE5874531AAC75FFB1EC2919E839F70408D515FB3BB13`.
  The live 136-file tree restored to exact intake, no save was touched, and no
  Kingmaker process remains. A changed source-bound candidate is required.
- Observer candidate `2ce9230af7ec312b35b7380dee164f59b5733fe6`
  completed source-bound development admission and strict packaging with source
  state `a3f32ce663df99db51a740298755083aa738f4b9a312e0789f919d7522357632`,
  ZIP `53c2f57f61d44b5137a7d06492c98165422a169197f5bac6b3e89ae6d521000e`,
  DLL `7c1012dc8d68ce10c2a339ca7f52cedd7cc87da7ed435ce1e3ffe14f0a704807`
  and MVID `762b63bf-ca9d-4f40-a8e1-a50057c4cdbe`; its selected group passed
  10/10 of 2,776 tests. The exact build/bin/staging set is preserved on E: as
  1,007 files under manifest SHA-256
  `1E2D0EFD9182B3175AB857A9161617DD2EAE7B176A0F2156B3953BE02DB67BCF`.
- Guarded run `20261031T1213586726270Z-d1157c1f467b49268983c4967c7927c9`
  under evidence directory
  `20261031T1213584342608Z-disposable-expanded-summoning-sprint32-native-review`
  retained FAIL. Hidden bootstrap, profiles and bodies passed; the actual cast
  again produced the visible oriented 36-HP section, changed 22 GridGraph nodes
  and changed the real route. Both the real weapon command and independent
  100d1 fire rule stopped before an attack roll or calculated damage
  (`ResultDamage` was null). Installed `RuleDealDamage.OnTrigger` IL shows this
  exact early return precedes `RulePrepareDamage` and is limited to native
  untargetable/green-NPC admission (the bundle was nonempty). The observer had
  not waited for the section's native summoned-appearance gate. Because the
  undamaged section remained at the same point, legal-placement correctly made
  all later formation attempts unavailable; no production formation verdict is
  implied. The next observer waits bounded native frames until the exact
  appearance buff is absent and `IsUntargetable` is false, and records both
  actors' faction/`CanAttack`/green-NPC predicates before the unchanged real
  weapon and fire consumers. Expected consequence: native damage calculation,
  breach cold/steam, GridGraph release, and the now-unoccupied formation retry
  become observable in the same run. Production Wall code remains unchanged.
  Focused validation compiled production in 46.9 seconds and passed the selected
  10/10 Sprint 35/correction tests in 2.7 seconds.
  The wrapper restored the 136-file live tree exactly to
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  no save was touched and no Kingmaker process remains.
- Targetability candidate `8e14c69079fa5c1388a7e4697c73a4df73357e71`
  completed source-bound admission and strict packaging with source state
  `322fe1b674d492c176f45d260ceb99c623fc62a745ee5ec9de5f79f666247f28`,
  ZIP `df2beb5b5e2dde710fe0db93e0ca86c21c18d792ff0935d6354e1fde86c10866`,
  DLL `08f0664544459a50cb104e084cb7a3f850a7384701564038ddbfa7f76c97a0f5`
  and MVID `859679b5-45ce-40eb-94cd-4bc8ca2c5ffc`; selected tests passed
  10/10 of 2,776. Its exact build/bin/staging set is preserved on E: as
  1,007 files under manifest SHA-256
  `B3581EE4E9304B87577231B0DFFA71A6F1DCF5C3CB3582262CE52D982B788D2A`.
- Guarded run `20261031T1250109528231Z-be0cb7d758594a26ba18906a60c3a460`
  under evidence directory
  `20261031T1250109049711Z-disposable-expanded-summoning-sprint32-native-review`
  retains its original aggregate **FAIL** and runtime-result SHA-256
  `03ED6280228A4B1427DA84120498E7CC83E7339D79BC7B5B1FF7CD8CA92FB7E0`.
  The changed precondition was proven: the native appearance buff was present,
  cleared after 43 frames / 1.431 game seconds, and the section was then
  targetable; both hostile sources could attack it and neither green-NPC guard
  applied. The real Glaive command started/acted/finished Success, auto-hit with
  d20 0, and dealt 7 damage from a native 1d10+12 packet marked half. The
  independent 100d1 fire rule then dealt 80 full damage (packet final 100),
  spawned the owned residual-cold and steam areas, released all 22 GridGraph
  claims, restored every captured node and restored the exact four-point /
  8.101601839 m route. Movement changed while the wall lived; line of sight did
  not and remains unclaimed.
- A bounded reader-only re-evaluation of immutable early evidence SHA-256
  `46430868EF1919E98A3D43F770BD086E3F679FA601A8508AA0F35140B3CDD00C`
  is **PASS** for Wall viability, without changing the original verdict. The raw
  trace contains the exact missing observations: the breach area's first entry
  performed one native SR check and dealt 16 cold damage from Bone to the fire
  witness, and each legal retry performed a native SR check followed by a
  successful DC18 `SaveReflex`; no section formed. The compiled observer sampled
  residual damage one frame before that event and compared the recorded
  `StatType` text to `SavingThrowType.Reflex`, so its arrays were empty despite
  the immutable rows. The observer now waits for that exact damage row and
  matches `StatType.SaveReflex`; no gameplay rule, roll or state is supplied.
  These corrected observations will run fresh at the later frozen public
  candidate boundary; no immediate repeat of already complete native facts is
  selected. Restoration SHA-256 is
  `AD9DD9F017C640EC153C36866CA81BBD5463401BC11AFF9967BEE1CA01C4BCE4`;
  the live 136-file tree restored exactly to intake, no save was touched, and
  no Kingmaker process remains.
- Publication candidate `cae13627` removes only the `barbed-devil` and
  `bone-devil` visibility keys. The append-only catalog still derives 128
  creatures, Monster 106 / 587, Nature 105 / 588, 1,175 generated roots and
  29 wrappers, now all 1,204 visible. A new grouped public observer covers the
  exact two-plus-three routes, real icons, native parent/child commands, slot
  debit, quantity, CL20 duration, base alignment and exact cleanup. Production
  compiled in 45.0 seconds and the selected Sprint 35/correction group passed
  10/10 of 2,776 tests in 9.3 seconds after the frozen-catalog guard was
  correctly changed from two hidden keys to zero plus the exact public route
  sets. No mechanics, dependency identity, old route or menu order changed.
- The existing targeted Working-save fixture now admits one exact
  `infernal-reach-ii` scope containing only Barbed and Bone. Its prepare leg
  spends Barbed's 1/day Unholy Blight and all three Bone quickened-invisibility
  uses, applies the exact DC20 finite Strength poison to a private recipient,
  and forms a real Wall section through Bone's production command. Cold ON
  requires the poison/configuration, zero resources, serialized section and
  recreated GridGraph claims; cold OFF requires the same independently timed
  poison and spent resources but no wall, body allocations, tail object or
  navigation claim. Source cleanup requires the wall to retire while the
  poison retains its serialized source ID with no live caster; fixture cleanup
  then removes only its owned state and saves fresh absence. The six-process
  ON/OFF orchestration is unchanged. Production compiled and the selected
  Sprint 35/correction group passed 10/10; the compiled request parser admitted
  the exact scope and passed 33 admission/refusal cases without creating
  evidence, deployment, game or save state. Native persistence remains pending
  on the frozen candidate.
