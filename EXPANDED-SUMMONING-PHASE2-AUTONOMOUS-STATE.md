# Expanded Summoning Phase 2 autonomous state

This is the live resume record for the owner's Phase 2 Sprints 9-21 mission in
`Kingmaker_Expanded_Summoning_Phase2_Codex_Starter_Packet.zip`. That order
supersedes the Phase 1 handoff's historical prohibition on starting Sprint 9.

## Current position

- Tranche: 2A, Sprints 9-13. Sprint 9 is internally qualified; current
  sprint: 10, Stirge and Giant Wasp.
- Branch: `codex/expanded-summoning-phase2a-sprints9-13` in
  `.worktrees/expanded-summoning-phase2a`.
- Accepted dependency: `master` / `origin/master` at
  `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`, the merged PR #24 Favored
  Class integration. The owner packet's expected `2943a02d` is its ancestor.
- Latest pushed source checkpoint before Wasp poison:
  `9f011014e9f0b574a8c10fff14e75088b4569818`; latest pushed branch head
  before this checkpoint: `0c1b5e24bef0cbdf2a4bd7d392917e3de408712d`.
  Branch head at intake: `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`;
  first pushed intake checkpoint: `359e346ecb6a094552f096869ecb9c81a2d291bf`.
- Sprint 9 Eagle and Dire Bat passed the internal technical gate. The 14
  preserved Bat placements are published; all 813 generated roots and 29
  native wrappers passed the guarded player path. Original skinned bird/bat
  visuals, Bat icon, Bat 40-foot imprecise blindsense, direct/quantity casts,
  RTWP/turn-based native combat, impact contact, open-floor and obstructed
  room-opening travel, visual lifecycle, save/load/expiry, module-disabled
  safety, donor controls and exact installation restoration are qualified.
  Sprint 10 is in progress. Giant Wasp is registered at SM IV and SNA IV,
  and its sting-delivered poison has a guarded mechanical PASS. All twelve
  logical placements remain suppressed. Stirge is registered at SNA I-IX
  with a hidden Tiny unit and zero-damage touch carrier; all nine placements
  remain suppressed. Neither creature is published.
- HumanReview: NOT_PERFORMED_NONBLOCKING. Sprint 9 internal engineering,
  fidelity, visual and evidence/restoration reviews passed with the bounded
  limitations below; tranche and later-sprint reviews remain pending.
- Blockers: none established. The Sprint 10 rules-source access gate is
  resolved by the owner's standing network authorization; exact Stirge and
  Giant Wasp baseline rules are cited in
  `EXPANDED-SUMMONING-PHASE2-BLOCKERS.md`.

## Verified intake and baseline

- The owner packet's mission, charter sections 1-5, Sprint 9-21 sections,
  Appendix A rows, roster workbook sheets, and summoning guide were read.
  Repository Phase 1 handoff, state, report, journal, program state, generated
  roster and fidelity/traceability records, icon guide/index/catalog, Pteranodon
  source/runtime pipeline, and guarded runtime instructions were inspected.
- Accepted master has 81 unique creatures, SM 74 base entries / 414 logical
  placements, SNA 71 / 399, total 813. Dire Bat's 14 placements remain
  registered and hidden; Eagle and Dire Bat share the native Giant Eagle donor.
- The accepted master uses local-development version `0.0.140`. No public Phase
  2 version or release has been chosen.
- Clean `scripts/Build-Local.ps1` on accepted `2ce70e4`: repository wrapper
  PASS; all 1,918 domain cases PASS; exact-reference Release build PASS; strict
  standalone UMM package PASS. Local package SHA-256
  `28691a8db147312409b87d4e044664355721a3f8b0437a318d10947a6707b836`;
  DLL SHA-256
  `08ddda2c7fff06e7124b45596194dd0371f06ab7e1c38d12595f416bab1e25f0`.
  These are baseline hashes, not Phase 2 candidate hashes.
- Guarded `observe-expanded-summoning-native-donors` on `2ce70e4` / version
  `0.0.140`: PASS, evidence
  `20260927T0251122942654Z-observe-expanded-summoning-native-donors`.
  Restoration record
  `20260927T0253210207995Z-observe-expanded-summoning-native-donors.json`:
  live tree 136 files and SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
  both before and after. The guard launched through Steam App ID 640820.
- The audit found native `Blindsight` GUID
  `236ec7f226d3d784884f066aa4be1570`, but its 60-foot **blindsight** and
  blindness immunity are too strong for Dire Bat blindsense. Native
  `Blindsense` component behavior is proven by the existing Nereid scenario;
  Sprint 9 requires a bounded, creature-owned sense fact and live verification.
- Source discrepancy: Appendix A's generated row labels Dire Bat `S10`, while
  the detailed charter, generated traceability ledger, and controlling Phase 2
  mission all place it in Sprint 9. Follow Sprint 9.

## Sprint 9 mechanical checkpoint in progress

- One new append-only identity, `KMG.Summoning.Natural.DireBat.Blindsense` /
  `5dcc039bc9674208a51e4babcd8a30ee` (`BlueprintFeature`), is registered
  in all module states and attached only to Dire Bat. It carries the native
  `Blindsense` component with `Blindsight=false` and range 40 feet. The native
  Blindsight feature was rejected because it also grants blindness immunity.
- The guarded `disposable-expanded-summoning` scenario asserts the exact
  feature and native live sense part on every Bat cast, and absence from
  Eagle, Pteranodon, and Roc. Commit
  `b32c00f0ad9999f8d469363f12694729e5f5d300` passed on 2026-09-27:
  evidence `20260927T0336278524107Z-disposable-expanded-summoning`, result
  `expanded-summoning-dire-bat-blindsense` PASS (`definition=True;bat=2/2;birds=13/13`),
  177/177 casts, 230 spawned, exact per-cast cleanup. The guard used Steam
  App ID 640820 and `KMG_AUTOMATION_WORKING`. This validates the bounded
  sense on spawned units, not completed Bat presentation or publication.
- Restoration record
  `20260927T0339582427042Z-disposable-expanded-summoning.json`: 136 live
  files, SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
  both before and after. Kingmaker process absent after the run. Runtime
  package SHA-256 `1e79c904f38724657a768e4e8664e89302dc788ae8aed547147c48f53e6babe1`;
  DLL SHA-256 `166360c55c0456a897bde37ad36f8a20a1e95b415830ab8ee83cef790a7986ec`.
- The new focused test was observed failing before implementation. Current
  `scripts/Build-Local.ps1`: repository wrapper PASS, domain 1,919/1,919
  PASS, exact-reference clean Release build PASS, strict UMM package PASS.
  Package SHA-256 `ac939b2bfe88c24cc2d20f1f4429d0b91701d6e6c4f08e0457a96d963e2c7af2`;
  DLL SHA-256 `a1a5021b6552fb77cce9ca77b3f8f81e6a143952e7d2962032db95b8e1a601ef`.
  These are source-qualified hashes; no game correctness claim follows.
- The original Dire Bat icon source is integrated at
  `assets-source/original-icons/expanded-summoning/sources/dire-bat.png`
  (SHA-256 `CC1B58F8802B5795198F37548786E84CA133F084C38CF9AB3167E17866607510`).
  Its 128px export and 29 consumer symbols are in the 92-icon manifests.
  Owner visual approval remains separate.

## Sprint 9 Dire Bat visual checkpoint, 2026-09-27

- The project-owned Bat generator reuses the accepted Pteranodon bind-frame
  parser, 46 reviewed bone names, instance-local material/mesh swap, and
  same-frame donor fallback. Its private measured 72-bone rig and Blender
  preview files remain ignored; the shipped JSON contains original geometry,
  weights, UVs, structural bone names, and an albedo hash, not donor transforms.
  The editable generator/painting/review scripts are retained. Regenerating
  the Pteranodon with the same input produced byte-for-byte mesh SHA-256
  `CC80E28589F5644C86E870C672CD8F188312920F7092CACBD6CB3DFF4007DE3C`,
  matching the protected asset. Bat mesh SHA-256
  `3C488CD4838812C12946410B9BBA3283917F9B005904D4B237FEDAAC508F0819`;
  painted albedo SHA-256
  `C84011DA7D1CA446191632ED1330892ED320EDF253F1AB6DCDB17E76DF21E335`.
- Repository validation PASS; 1,920/1,920 full domain tests PASS; clean
  exact-reference Release build PASS; strict 253-file installable package
  PASS. Candidate package SHA-256
  `6dc7d121dfa74d41e73eded069dd5306f77ef4d47a17b528e7d7e671f3cbcf5d`;
  DLL SHA-256 `0806e60aee39367cb1c6c111ab8c51989c726eccb501c6854f3d80289d27f75a`.
- Guarded Steam App ID 640820 run on the disposable working save PASS:
  `20260927T0412311065438Z-disposable-expanded-summoning`. It completed
  177/177 casts; `expanded-summoning-dire-bat-visual-attached` PASS (2/2);
  sense PASS (2/2, bird controls 13/13); Eagle/Roc donor isolation PASS
  (4/4); Pteranodon attachment/crowding/repeated lifecycle PASS (9 views,
  11 total Bat/Pteranodon patch outcomes). This proves attachment and
  instance isolation on live spawned views, not motion, impact, navigation,
  save/load, icon, or human visual acceptance.
- Restoration record
  `20260927T0416020406571Z-disposable-expanded-summoning.json` verifies the
  136-file live mod tree had SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
  before and after. Kingmaker exited. Dire Bat remains hidden in 14 placements.

## Sprint 9 Eagle visual checkpoint, 2026-09-27

- The original feathered Eagle uses the same private measured bind frame and
  shared validated mesh parser/instance-local renderer swap. Its geometry has
  1,696 vertices, 46 weighted bone names and three or fewer influences per
  vertex; the private Blender 4.5.10 LTS preview and FBX stay ignored. Shipped
  Eagle mesh SHA-256
  `A45D0CD203C9742E6DAA64B86BCF642906AE08B41A4B8285B51D33050A760753`;
  original albedo SHA-256
  `A17C7891AEEBB4B879D7D53C43F39920AFF11E375070DA4743DA951F16C08F47`.
  The focused Eagle asset test failed with a missing mesh before staging.
- Repository validation PASS, all 1,921 domain tests PASS, clean
  exact-reference Release build PASS, strict 255-file package PASS. Runtime
  candidate package SHA-256
  `45F0DB76FAF98A6CF51590A507251E4D233E61A2E30204C774140DA62860A7EE`;
  DLL SHA-256 `2E811EC9D965D1B29AB35199B8A2FFAA028600BFF340617A996D866AC4F23229`.
- Guarded Steam cast run
  `20260927T0443251559393Z-disposable-expanded-summoning` PASS: 177 casts,
  Eagle visual 2/2, Bat visual 2/2, Roc donor isolation 2/2, Pteranodon
  repeated lifecycle 5 views/9 flying patch outcomes. The prior run
  `20260927T0435314466298Z` failed only because its assertion counted an
  additional Eagle summoned in a later mechanical contract; the counter was
  narrowed to the coverage loop and the full run rerun PASS.
- Guarded visual-contract run
  `20260927T0446552603378Z-disposable-expanded-summoning-visual-contracts`
  PASS: 81/81 selection/navigation, locomotion, attack animation and
  hit/death checks; Eagle live height 1.3596102 below Medium humanoid
  1.92633152, max bound 3.06158566. Its live-camera silhouette is supporting
  art evidence only, not proof of animation or impact alignment.
- Guarded visual lifecycle retry
  `20260927T0510593955723Z-disposable-expanded-summoning-visual-lifecycle`
  PASS with a 600-second timeout; restoration record
  `20260927T0513506556485Z-disposable-expanded-summoning-visual-lifecycle.json`
  verified the 136-file baseline SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  The first lifecycle game result was PASS but its 120-second launcher wait
  timed out, so that first attempt is not counted as qualified.
- Player-path game result
  `20260927T0452347967456Z-disposable-expanded-summoning-player-path` was
  PASS, but Kingmaker did not exit within the launcher/wrapper teardown window;
  the batch record `20260927T0506085646355Z-disposable-expanded-summoning.json`
  correctly marks restoration FAILED. After Kingmaker exited on its own, the
  exact 258-file deployed tree SHA-256
  `D9C779DC7807EAA6556CD632BC55575E98615F6B9FC14BB0317C474F63991304`
  was verified, then `Restore-Live-Mod.ps1` restored the snapshot at
  `20260927T0440037530378Z`. The live tree was verified at the original
  136-file SHA above, with no game process or compatibility lock. This
  player-path run is **PASS with teardown fault**, not a clean qualification;
  retry after the Bat icon/publication change with a longer guarded lease.

## Sprint 9 Bat publication checkpoint, 2026-09-27

- Preserved identities: 14 Bat placements (seven each SM and SNA, tiers III-IX)
  published with the original `dire-bat.png` icon. The catalog remains 81
  project-owned units and 813 logical abilities; no new unit/ability GUIDs.
- Repository validation, 1,922/1,922 domain tests, clean exact-reference
  Release build and strict 256-file package PASS. Final local package SHA-256
  `ad2e43bc0354acc25e7af8615000d02e94ed25ef1e80cde83aafba46f366c043`;
  DLL SHA-256
  `02374e67217b07e5cdd772e07e35bb457d734ccce1c658eb7e7123d55e4e151d`.
- Guarded cast `20260927T0551063528146Z` PASS; player path
  `20260927T0554347181180Z` PASS 813/813 generated roots and 29/29 native
  wrappers. Their shared wrapper restoration record
  `20260927T0608158839124Z` confirms the 136-file pre-run hash below.
- The first inventory run `20260927T0612429996748Z` FAIL exposed stale
  exact-fact and allowed-reference maps for the new Bat sense, while its menu
  order/count/icon/placement checks passed. The exact audit correction then
  passed all 48 inventory assertions in
  `20260927T0623527812622Z-observe-expanded-summoning-inventory`, including
  813 visible placements, distinct non-null project icons and zero prohibited
  references. Wrapper restoration record `20260927T0627357893276Z` confirms
  the live tree returned to
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
- Publication is source and guarded-runtime qualified. Sprint 9 final
  qualification and owner visual approval remain open.

## Sprint 9 flying save/load checkpoint, 2026-09-27

- Published-candidate visual-contract, view-lifecycle and rules runs PASS:
  `20260927T0634285980681Z`, `20260927T0637208226656Z`,
  `20260927T0640079411062Z`. Wrapper restoration record
  `20260927T0643575276053Z` confirms the original live tree.
- The persistence fixture now includes one Eagle and one Dire Bat, with exact
  private renderer attachment checks. Repository validation, 1,923/1,923
  domain tests, clean Release build and strict 256-file package PASS. Local
  package SHA-256
  `21c06b3c03c90e4d56ac18f4707e18d1b05b476e1d3e7d527b9d3308eaaa8305`;
  DLL SHA-256
  `9d909f7a2ebb8741f5c6bbfa0abe8168f8b3d42b564df5ae7e34db2220d664c9`.
- Guarded working-save prepare `20260927T0655003623332Z`, fresh-load cleanup
  `20260927T0659022955480Z` and final absence
  `20260927T0703025930540Z` each PASS 14/14. The original Eagle and Bat
  meshes were bound before save and after reload, then both were absent after
  native expiry. Each writing stage had exactly one authorized SaveRoutine;
  final absence had none. Wrapper record `20260927T0705405909889Z` confirms
  the same pre-run live tree hash.
- Actual doorway travel, impact contact, RTWP/turn-based and module-disabled
  qualification remain open. The existing visual-contract probe is an
  animation/view contract, not proof of movement through geometry.

## Sprint 9 flying travel checkpoint, 2026-09-27

- Added a guarded review assertion that requires the summoned Eagle and Dire
  Bat to accept a native `UnitMoveTo`, clear their native appearance buff,
  advance at least 0.75 m toward a reachable same-graph destination, finish
  within 2 m, and show nonzero movement-agent velocity. The fixture uses a
  party-area navmesh linecast and does not inject a forced path. It restores
  temporary pause/awake state and dismisses the summons. Animation captures
  alone cannot pass this assertion.
- The first attempts showed zero travel before appearance readiness. A
  temporary forced-path probe later measured travel, but could not attribute
  it to the command. With that probe removed, the final guarded result
  `20260927T0911579435138Z-working-save-expanded-summoning-creature-review`
  was PASS: Eagle traveled 7.943 m toward its target, minimum target gap
  0.18 m, peak velocity 8.124; Dire Bat traveled 6.195 m toward its target,
  minimum gap 1.26 m, peak velocity 4.064. Both native move commands were
  accepted and `CanStart` was true. `IsStarted` was not sampled true in that
  run, so this is evidence of live open-floor travel, not a claim about the
  precise command lifecycle or doorway behavior.
- Repository validation, 1,924/1,924 domain tests, clean exact-reference
  Release build and strict local package validation passed. Package SHA-256
  `35216de8c114f046fd89a4341a7e960270dbc9405c449a911db0fddcc467eb95`;
  DLL SHA-256
  `278ca237fbdece7b3b68ba45e1b73763002f3c4ec5d376880a8277955684a729`.
  The wrapper restored the original 136-file live tree with SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
  in restoration record `20260927T0916296796822Z`.
- Doorway navigation, actual attack contact and impact alignment, both combat
  modes, module-disabled safety and owner visual approval remain open.

## Sprint 9 flying combat-mode checkpoint, 2026-09-27

- Extended the existing guarded working-save summon activation fixture with
  only `flightCreature=eagle|dire-bat` on the turn-based and RTWP controls.
  The typed-save launcher, preflight and on-disk request serializer each
  allowlist that exact field. The first successful control run silently
  omitted it during serialization and summoned Dog; it is excluded from
  Eagle evidence. The corrected request files carry the exact creature key.
- Corrected guarded own-tier Quickened runs passed for Eagle turn-based
  (`20260927T1000134559074Z`, six exact-hostile weapon rules), Dire Bat
  turn-based (`20260927T1007223013777Z`, two), Dire Bat RTWP
  (`20260927T1010235825673Z`, one) and Eagle RTWP
  (`20260927T1017190134756Z`, one). Each exact species assertion passed.
  The turn-based fixture observed native summon turns and commands; RTWP
  observed native AI command, appearance clearing, no turn order and no
  `CurrentTurn`. This proves active Quickened combat in both modes. It does
  not prove visual contact or attack-impact alignment.
- Repository validation, 1,925/1,925 domain tests, clean exact-reference
  Release build and strict package validation passed. Package SHA-256
  `49ac8b3815ffa6be1265feb55b0baaba8ce6c725db791699361421aee8cfd9e6`;
  DLL SHA-256
  `28bef843f8454360f0620dab7a8e7c514438d17969ee74a0c858a2e045130a48`.
  Both batches restored the original 136-file live tree SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
  the final records are `20260927T1013217931852Z` and
  `20260927T1020168331178Z`.
- A guarded Roc negative-control party-camera review
  (`20260927T0928318357033Z`) passed and showed the same cyan wing
  occlusion silhouette where a native donor body crosses the corridor wall.
  This makes the cyan overlay an engine/camera occlusion effect rather than
  evidence of a Bat-only material swap failure; doorway geometry and
  normal-camera visual acceptance remain open.

## Sprint 9 disabled publication checkpoint, 2026-09-27

- The broad `observe-feature-module-settings` run
  `20260927T1030110774186Z` returned FAIL on unrelated Brown Fur and
  teleportation-scroll assertions. Its Expanded Summoning publication gate
  passed, but the overall run is excluded as qualification. A new guarded,
  read-only `observe-expanded-summoning-module-boundary` request accepts one
  exact Boolean and compares only the active module state and the eighteen
  native summon parents, plus loaded mod version.
- `gunslinger-only` compatibility transaction
  `compat-20260927T104207Z-b887ecd1ad31` ran the narrow scenario via Steam;
  result `20260927T1046174341970Z-observe-expanded-summoning-module-boundary`
  is PASS. With the module disabled, expected and observed were exactly
  `publishedParents=0;placements=0;nativeOptions=0;preservation=0;unclassified=0;placementsExact=True;nativeVariants=46`.
  Version `0.0.140` passed. Transaction status is `Restored`,
  `restorationVerified=True`, and the original FeatureModules.json SHA-256
  `A06601C52F1B98AC54EED309F7415677A3C55FE4C51DAA2556DDE5206C687F17`
  was restored byte-for-byte. The launcher restored the original live mod tree.
- Repository validation, 1,926/1,926 domain tests, clean Release build and
  strict package validation passed. Package SHA-256
  `bb615ca3479d4aae79950b0fe0a56c6ad463cdb3946eb1c4f82ee5f1e6c4e64b`;
  DLL SHA-256
  `955ec69de75f0a4035dba5332d91712bcbde442a98650366fc38c008114f9797`.
  This proves the disabled publication boundary at mod load. It does not yet
  establish safe loading of an existing Eagle/Bat summon with the module off.

## Sprint 9 save-backed module-off checkpoint, 2026-09-27

- The first `gunslinger-only` off-load attempt
  `20260927T1108473787599Z` timed out before `Player.PostLoad`. Its game log
  shows the known missing Craft Magic Items blueprint dependency when that
  profile removes the mod referenced by the working save. The prepared save
  was not altered by the failed load; the compatibility transaction restored.
  This is excluded from summon mechanics evidence.
- The mission runtime wrapper now permits one exact
  `working-save-expanded-summoning-verify-cleanup` request to stage
  `expanded-summoning=false` after snapshotting the live KMG tree. It changes
  only that JSON Boolean, retains the original installed mod graph (including
  Craft Magic Items), and restores the exact original tree in `finally`.
  Focused orchestration tests passed 46 assertions, including malformed,
  absent, duplicate and already-disabled setting rejection.
- An initial full-mod off load `20260927T1130267838290Z` loaded and cleaned
  all 16 fixture summons with one working-save write, but returned FAIL only
  because the existing visual assertions still expected custom meshes with
  the module disabled. The asset runtime intentionally leaves a visible native
  donor in that state. The assertions now demand an enabled, nonempty native
  donor renderer and no project visual for the three saved flyers.
- The corrected guarded trio passed: prepare
  `20260927T1143486900326Z` (16 exact summons, one authorized write),
  module-off fresh-load/cleanup `20260927T1151452804617Z` (14/14 assertions,
  16 exact summons, zero published placements, original Pteranodon/Eagle/Bat
  identities on visible 72-bone native donors, session hold safely released,
  one authorized write), and final enabled-module absence
  `20260927T1159482067877Z` (14/14, zero KMG summons and zero save writes).
  Exact off-run restoration record `20260927T1155391539509Z` and final record
  `20260927T1202300608552Z` both verify the original 136-file live tree SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  The original FeatureModules.json SHA-256
  `A06601C52F1B98AC54EED309F7415677A3C55FE4C51DAA2556DDE5206C687F17`
  is contained in the verified restored tree.
- Repository validation, 1,927/1,927 domain tests, clean Release build and
  strict package validation passed. Package SHA-256
  `f214756442d18ef94e3da0bd98c8c783c619836c327a18e4cf0b52e865576014`;
  DLL SHA-256
  `1476c5acb2d748f64e391fccdca65dd6a9bd042602fd2fa69f1f3d9109799d2a`.
  This closes the save-backed module-disabled safety gate for the current
  Sprint 9 fixture. Doorway navigation and visual attack-impact alignment
  remain open; owner visual approval remains separate.

## Sprint 9 attack-anchor diagnostic checkpoint, 2026-09-27

- The own-tier Quickened combat fixture recorded Eagle/Bat jaw, head and foot
  bone distances at native weapon rules. The first selector took the hostile's
  `L_WeaponMarker` renderer, not its body; those geometry values are excluded
  from visual alignment qualification. The exact-target combat results remain
  valid.
- Guarded Steam Eagle result `20260927T1219166168032Z` passed with six
  exact-target attacks. Jaw distances at two bites were 2.007 and 2.003 m;
  nearest-foot distances at four claws were 1.996-2.109 m to that weapon
  marker. Visual contact remains unresolved despite valid attack rules.
- Guarded Steam Dire Bat result `20260927T1226258599811Z` passed with two
  exact-target bites; jaw distances to the same weapon marker were 0.454 and
  0.436 m. No Eagle-versus-Bat contact conclusion follows from these values.
- Both runs passed repository validation, 1,927/1,927 domain tests, clean
  Release and strict package validation. Package SHA-256:
  `666e32b5c8f3b7bffddce3dead39170c46be6269ebfa663f6a3d35eac990d2e9`;
  DLL SHA-256: `17df838f3ef184dc8cfb8b8f8d8fae68c091e30a6e5136b043568f1867b8410e`.
  Restoration records `20260927T1222257744521Z` and
  `20260927T1229286678632Z` verify the original 136-file live-tree SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  The portable .NET 8 fallback could not restore offline framework packages;
  the repository's standard offline MSBuild suite passed.

Guarded Eagle repeat `20260927T1240150768739Z` passed native combat and
restored the original tree in record `20260927T1243233634861Z`. Its baked
surface probe revealed the marker selection explicitly: `targetRenderer=
L_WeaponMarker`. The distances to that marker are also excluded. Repository
validation, 1,927 domain cases, clean Release, strict package passed; package
SHA-256 `5928202557b68b5afe7f832dadacaf5211d6187e462cd1295bca49f79fd53329`.

The corrected body-renderer selector passed repository validation,
1,927/1,927 domain tests, clean Release and strict package validation.
Package SHA-256 `e82b3444df52066691ca9be20ce316d65c92dbfa3f77acd9132cf410acd0c5bc`;
DLL SHA-256 `58903da59c883993f919c63f2174d54e22190265801811522c06cb71148ff714`.
Guarded Eagle result `20260927T1252224741767Z` passed six exact-target
weapon rules and measured the hostile's `Character` skinned body renderer
(1,819 bones, about 1.54 x 1.96 x 1.31 m). Beak-weighted vertices were
1.271-1.292 m from its bounds at two bites; nearest talon-weighted vertices
were 1.297-1.330 m away at four claws. Guarded Dire Bat result
`20260927T1259362389907Z` passed two exact-target bites against the same
body contract; beak-weighted vertices intersected its bounds at both (0 m).
Restoration records `20260927T1255324566498Z` and
`20260927T1302410049391Z` verify the original 136-file live tree SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
The first baked Eagle probe applied its 0.30 view scale twice. Calibration
run `20260927T1314477538739Z` passed native combat and showed
`viewForwardDot=1`, but its doubly scaled bounds (about 0.31 x 0.16 x 0.58 m)
did not match the live rendered presentation; restoration record
`20260927T1317535013252Z` verifies the original tree. Those weighted
distances are excluded. The scale-corrected probe passed repository
validation, 1,927/1,927 domain tests, clean Release and strict package
validation (package SHA-256
`d8c66bed8222d7c2fab82cc258a96462c65f029818e3a3f5a1ee524e84d95932`,
DLL SHA-256 `8f134eb67fabe938afb0200a959bb7a3361b4268d7fdede01323c980515e4c1f`).
Guarded Eagle result `20260927T1326061196128Z` passed six exact-target
weapon rules. The beak/head-weighted surface was 0.739-0.798 m from the
hostile body at two bites; nearest talon-weighted surface was 0.864-0.906 m
away at four claws. Baked bounds were consistent with a Small rendered bird,
and view facing dot was 1. Restoration record `20260927T1329140002404Z`
verifies the original 136-file tree hash above. Eagle attack-impact alignment
is a measured open defect. Bat bite contact passed its bounded fixture;
doorway traversal remains open.

Next: fix Eagle's instance-local attack presentation without changing combat
reach or shared donor assets, prove contact, then qualify doorway traversal.
Sprints 10-21 remain planned; this checkpoint is not Sprint 9 completion.

## Sprint 9 Eagle attack presentation checkpoint, 2026-09-27

An Eagle-only component now offsets its attached skeleton root by at most
0.95 m during native attacks, with bounded approach, impact hold and return.
The entity, view root, movement agent, selection and mechanical reach stay
unchanged; Bat and native donor views have no component. The component restores
its offset on disable/destruction and the donor visual remains the attach
fallback. Focused policy/source tests, repository validation, 1,928/1,928
domain tests, clean Release and strict package validation passed. Package
SHA-256 `2eeb725842c9e3329dce8967b5f22031aab0f1d580cae2e655a7bab0f2796b69`;
DLL SHA-256 `a38afad258f86e4fd59bb0bf9915260f58ee8db210a22ca7a7694a5f6154c4ca`.

Guarded Steam turn-based Eagle result `20260927T1345424022344Z` passed six
exact-hostile native attacks. At the weapon events the beak/head weighted
surface was 0-0.144 m from the hostile body for bites, and the nearest talon
surface was 0-0.226 m away for claws. The lunge reported 0.95 m at each
impact. The original 136-file live tree was restored in
`20260927T1348486221199Z` to SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Guarded working-save Eagle/Bat creature review
`20260927T1353291620814Z` passed live renderer, native travel and cleanup
for both: Eagle travelled 7.945 m to within 0.176 m of destination; Bat
travelled 6.255 m to within 1.2 m. Both had four intact party-camera renders
and zero surviving reviewed summons after dismissal. The original tree was
restored in `20260927T1358060993442Z` to the same hash. These renders do not
establish the visual appearance of the live combat lunge; measured attack
contact is the mechanical visual-geometry evidence. RTWP contact was
separately qualified by guarded result
`20260927T1404176347439Z`: one exact-hostile native bite, 0.106 m
head-weighted surface gap to the hostile body, 0.95 m visual offset, facing
dot 1. Restoration record `20260927T1407214071366Z` returned the original
tree to the same hash. Doorway traversal remains open. HumanReview:
NOT_PERFORMED_NONBLOCKING.

## Sprint 9 doorway and internal closeout, 2026-09-27

- Native working-save scene/navmesh surveys `20260927T1423188619526Z`
  and `20260927T1435573645955Z` identified closed named doors in different
  path areas and a connected adjacent room across an obstructed direct line.
  A first destination beyond another blocked boundary failed in
  `20260927T1503558530369Z`: the Eagle moved 15.57 m but finished 8.688 m
  short. That result is excluded from doorway qualification. The wrapper
  restored the original installation in each survey and failed fixture run.
- Narrowed Eagle result `20260927T1516313231221Z` and Dire Bat result
  `20260927T1524129109534Z` each passed native movement to within 0.068 m
  and 0.066 m of the adjacent-room node, respectively, with cross-frame
  travel, native velocity and zero surviving reviewed summons after cleanup.
- The final stricter, combined fresh-process result
  `20260927T1536450257132Z` passed 12/12 assertions. The same-area
  endpoints had a blocked direct native line; native `UnitMoveTo` crossed
  the measured opening. Eagle travelled 12.357 m and finished 0.021 m from
  destination; Dire Bat travelled 12.360 m and finished 0.024 m away. Both
  moves started and finished, both renderers were intact in four captures,
  and both cleanup counts were zero. Restoration record
  `20260927T1541351948729Z` verifies the original 136-file live tree at
  SHA-256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  Repository validation, 1,928/1,928 domain tests, clean Release and strict
  package validation passed on the final source; package SHA-256
  `c329103885331df5e9f81346bc33af8a2fd0d473f42a4744b1eb0deedbd0b56b`,
  DLL SHA-256 `c952c065a25553c17940d0ff932837736836c03faa24d159520af453ae64f578`.
- Engineering/runtime-safety review: the doorway fixture uses only the
  named disposable working save, guards the scene landmark and navmesh
  endpoints, issues native movement without a forced path, restores pause
  and awake-unit state, dismisses each summon, writes no save and restores
  the live mod tree. The Eagle lunge changes only its private skeleton root.
- Rules/roster review: Eagle keeps its accepted Small three-attack role;
  Dire Bat uses its preserved identities, Large presentation and bounded
  imprecise blindsense. No attack reach, hit chance, spell tier, quantity or
  balance decision changed in the doorway/contact work.
- Visual/player review: original feather and membrane models, material
  controller, intact party-camera frames, live body-contact measurements,
  ordinary and obstructed movement, hit/death/expiry and native donor
  controls were inspected. Cyan wall occlusion also appeared on native Roc
  and is treated as the game's silhouette overlay, not mechanical contact
  evidence. HumanReview: NOT_PERFORMED_NONBLOCKING; aesthetic approval remains
  with the owner.
- Evidence/restoration review: invalid weapon-marker and doubly scaled
  contact samples and the overlong doorway route are explicitly excluded;
  the corrected body/weighted-surface and final native movement records are
  the acceptance basis. This fixture proves one actual room opening in the
  working save, not every map geometry or a general flight pathing guarantee.

Sprint 9 internal technical status: PASS. Next authorized item: Sprint 10
Stirge and Giant Wasp. No tranche candidate or PR-ready claim yet.

## Sprint 10 intake: native flying-vermin and signature-mechanic audit

- The charter and ideal-roster workbook list Stirge only at SNA I and Giant
  Wasp at SM IV/SNA IV. Their signature roles are attach/blood drain and
  flying poison strike, respectively. Neither is in the current creature
  catalog or native-donor catalog.
- I extended the existing metadata-only live blueprint audit to search
  `wasp`, `stirge`, `mosquito`, `blood`, `attach`, and `drain`. This changes
  only the diagnostic query; no unit or mechanic has been published. The
  focused intake test, repository validator, 1,929/1,929 domain cases,
  clean Release build and strict package validation passed. Candidate
  package SHA-256: `16866C3F109BD2A99ED642A2FC6F5A6146EFF807DF9A0B058AE35CBB7F727F94`.
- Guarded fresh-process `observe-expanded-summoning-native-donors` result
  `20260927T1601519083199Z` passed. Its 306 selected unit blueprints
  contain no name matching Stirge, Wasp or mosquito. The 2,462 selected
  facts and 328 selected abilities contain no matching attach/blood-drain
  identity; of 366 selected buffs, the only matching identity is generic
  `ConstitutionDrain` (`8081f8edc11aa29478ded59d1ca3d194`). This
  rules out a named native creature/attach shortcut, not every possible
  component-level implementation. No game-owned assets were exported.
  Restoration record `20260927T1603474656325Z` verifies the original
  136-file live tree at SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
- The supplied charter, roster workbook, and summoning guide give tiers
  and tactical roles, but no numeric Stirge/Wasp stat block or poison
  progression. The charter cites external spell-list URLs; repository
  machine-safety instructions prohibit network access without explicit
  authorization. I am checking the local rules/engine seams before
  declaring whether a source-material decision is required. Sprint 10 is
  not qualified and no claim of creature correctness is made.
- The follow-up guarded metadata audit `20260927T1629242113136Z` widened
  the search to fly, beetle, mantis, insect and vargouille rigs and read the
  native Giant Spider poison buff component graph. Among selected units,
  only six Giant Flytrap names matched these terms; no separate native
  flying insect rig was found. The native `BuffPoisonStatDamage` graph has
  Strength, 1d6, six round ticks and two successful saves, so it cannot be
  reused unaltered for Giant Wasp's Dexterity 1d2, six total exposures and
  one-save cure. Repository validation, 1,929/1,929 domain tests, clean
  Release and strict package checks passed; package SHA-256
  `034706D6AB5C20F1E9AA65A75ECFF8C17031B2A499AD275BDB9FBF034DC8F93C`.
  Restoration record `20260927T1631336749721Z` verifies the original
  136-file live mod tree at SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
  This is design evidence, not Wasp poison qualification.

## Sprint 10 Giant Wasp original visual checkpoint, 2026-09-27

Pushed commit `9933c40e354155ca9a66610e30d310c7a2eab96b` adds
project-owned procedural Wasp mesh and albedo, their deterministic scripts
and provenance, package integration, and the established flying renderer's
instance-local swap. The Wasp creature remains unregistered and unpublished;
no poison, attack or live view claim is made by this checkpoint. The private
Blender/FBX products stay in ignored local evidence because they encode the
measured donor rig. Top and side renders were reviewed locally; the revised
waist connection is coherent at close inspection, but party-camera and motion
review of a summoned Wasp remain outstanding.

Repository validation, all 1,929 domain cases, exact-reference Release and
strict 258-file package validation passed. The final dirty-tree candidate
had source-state SHA-256
`8fa680d4443937cca382f4203b8ea2fd5225d622c3edf511c51856bfe44d4c61`,
package SHA-256
`89f75a7c4ec22d3f657e42643f242a459ea31f0f64f18f66908fc2041987a75d`,
and DLL SHA-256
`b4750c30d0e69b809755040ff8acdde0f2dd32cfc511f45025761eb90d3ba4a0`.
Guarded runtime result `20260927T1710239929046Z` explicitly reported
`giant-wasp-original-asset-loader: visual:published` and PASS. Restoration
record `20260927T1712231807937Z` verified the exact original 136-file live
tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

## Sprint 10 suppressed Wasp registration checkpoint, 2026-09-27

Pushed source commit `9f011014e9f0b574a8c10fff14e75088b4569818`
appends 26 exact identities after the preserved 2,461-entry ledger: one
Giant Wasp unit, 24 logical/template ability identities and one 1d8 sting.
The manifest is 2,487 entries, 2,485 active and two reserved. The roster is
82 registered units, 75 SM entries / 420 placements and 72 SNA entries /
405 placements. Wasp's twelve placements remain suppressed, leaving the
player-visible 813 unchanged. Its dedicated sting is registered but poison
is not implemented; no Wasp cast, combat or view qualification is claimed.

Repository validation, all 1,932 domain cases, clean Release, and strict
258-file package validation passed. Package SHA-256:
`357A4CF091445BC826A6AFA6FB4D1C190A9E3B18AA12AC557E2781BA84BB405C`;
DLL SHA-256:
`E38C4F208E614DB05B234C05CA3AF48FC2E04CFD3504A48B648E19D327F3574D`.
Guarded inventory `20260927T1804369885102Z` passed all 48 live assertions:
82 units, 1,290 abilities, exact menu order and quantity counts, zero
missing published icons, distinct visible category icons and unchanged
foreign choices. Its 420-second window was required because the first run
completed after the default 120-second result window and found that the
observer incorrectly required a suppressed Wasp icon. The corrected run
passed. Restoration `20260927T1808371924124Z` verified the original
136-file tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

Next: add Wasp's exact Dexterity poison, then qualify attack cadence,
vermin immunities, view motion/contact, lifecycle, direct and quantity
casts before publishing. Stirge still requires attach, blood drain, escape
and cleanup. Continue Sprint 10 before Sprint 11.

## Standing boundaries and next action

`OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`.
Active holds and mouth occupancy return cleanly released on reload. Do not
implement hold re-establishment or call this grapple persistence.

Next: implement and qualify Sprint 10 Stirge and Giant Wasp from the cited
Pathfinder 1e rules, then continue through
the remaining authorized Phase 2 work. The Bat's preserved placements
are published after its model, blindsense, icon and guarded live cast gate.
Continue through A, B and C without an intermediate owner
decision. Open three stacked PRs as the tranche gates pass. No merge, release,
permanent deployment, or Sprint 22.

## Sprint 10 Wasp poison checkpoint, 2026-09-27

The dedicated Wasp poison feature and venom buff are append-only active
identities `7f023db3e8db404880511ea354f8a4fa` and
`9eda79a310ab49f7b1e59defe30b5579`. The manifest is now 2,489 total,
2,487 active and two reserved. A cloned native Spider poison lifecycle is
scoped to the Wasp sting: Fortitude, Dexterity 1d2, six total exposures and
one successful save to cure. A Wasp-only action sets the 4-HD, Con-based
DC including the stat block's +2 racial bonus on the poison context before
the native save; that same context is retained by the buff. The native
Spider icon is provisional only while every Wasp placement remains hidden;
it must receive its own icon disposition before publication.

The final guarded `disposable-expanded-summoning` run
`20260927T1912579609072Z` passed 21/21 assertions, including 179/179
structural casts, Wasp visual attachment 2/2 and Wasp poison: sting hit,
DC 18, initial Dexterity damage 1, a failed round save with poison active,
and removal after a successful save. The next round's rolled 1 produced
zero integer stat damage under the working save's `DamageToParty=0.8`
enemy-damage scale. Earlier runs
`20260927T1841288896478Z`, `20260927T1853108579217Z`, and
`20260927T1903352617466Z` are excluded as qualification passes; they
exposed DC, visual-rig expectation and one-tick assertion defects that were
corrected. Final restoration
`20260927T1916362556609Z-disposable-expanded-summoning.json` verified the
original 136-file live tree and SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

Repository PowerShell validation, 1,933/1,933 full domain cases, clean
Release build and strict installable package validation passed after the
final source change. Clean package SHA-256:
`5E7AC1E0DE325AD4F531AB09BCDF8DA8A24E843E6263CE27427B51C3308B5A90`;
clean DLL SHA-256:
`A4742CA4E7A845E1AA9C38B62066E9BAA2BEE79B3717C1C15845F27B3A6C2AEF`.
The guarded exact-reference runtime package was
`0AC6AD7F5819687FA47DD3F45253BF81FC2CF2228400C65FB4873DD59287FC58`;
its DLL was
`0DCDF4AD275BC052BDB67827E8A0C603620C0129AFF627E216866736C53514C1`.
No full Wasp qualification or publication is claimed. Next: Wasp motion,
attack-contact, immunities, quantity, cleanup and save/load checks, then
Stirge attach and blood drain. Continue Sprint 10.

## Sprint 10 Wasp guarded movement and visual diagnostic, 2026-09-27

The suppressed Wasp is now admitted only to its guarded working-save creature
review. The final run `20260927T1955333075608Z` passed native 12.345 m planar
movement, a connected doorway crossing, four lit/intact camera captures and
zero surviving summons. Its attack capture is animation-only, not a live
strike. A fifth diagnostic frame temporarily hid and restored the one
auxiliary `AttackLine` renderer; cyan silhouettes persisted, including one
at a distant wall. Native occlusion display is an inference, not proven.
The crowded frames do not qualify visual clarity or target contact.
Installation restoration `20260927T1959253902969Z` verified the original
136-file tree, SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

Repository validation, 1,934/1,934 domain tests, clean Release and strict
package validation passed. Package SHA-256:
`5AE609E8C383588C294DE9CFB9CB00113B6C823AA548CD94C748AE9B9309C01A`;
DLL SHA-256:
`7C21C3FC8A31E2D078FF56DC2E3873D989326626767471DE5A641D71256F5BEA`.
Wasp remains suppressed. Next: real target-contact and cadence fixture,
immunities and quantity/lifecycle checks, then Stirge.

## Sprint 10 Wasp quantity checkpoint, 2026-09-27

The disposable mechanical fixture now exercises 1d3 and 1d4+1 Wasp casts
in both SM and SNA under suppression, in addition to all own-tier singles.
The final guarded run `20260927T2015561300256Z` passed 22/22 assertions:
183/183 native commands, all four Wasp quantity variants with legal counts
and exact Wasp blueprint identity, 14/14 private visual attachments and
exact per-cast cleanup. The original 136-file installation was restored at
SHA-256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
record `20260927T2019330163598Z-disposable-expanded-summoning.json`.
Repository validation, 1,935/1,935 domain cases, clean Release and strict
package validation passed. Package SHA-256:
`E30157BF6100441F6D0278B14FCB89F28292AF26C7A935BD71DB982581124BB4`;
DLL SHA-256:
`A9E03627FDFEDD4562C88257C4842CCDFE0D99252D1D705FB592DC68D40DDD33`.
Wasp remains suppressed. Next: prove vermin mind-affecting immunity,
real strike cadence/contact and safe lifecycle, then Stirge.

## Sprint 10 Wasp Vermin immunity checkpoint, 2026-09-27

The final guarded `disposable-expanded-summoning` result
`20260927T2115391949167Z` passed 23/23 assertions. A paired native
RuleApplyBuff probe found the Wasp immune/ineligible to a mind-affecting
confusion buff and the human control nonimmune/eligible. Neither target
received the buff through this fixture; only the native rule decision is
qualified. The Wasp carries native VerminType and its immunity components,
but its cloned donor species marker still reads `EagleGiant` and needs
correction. Three failed diagnostics are excluded from qualification.
Restoration `20260927T2119178826656Z-disposable-expanded-summoning.json`
verified the original 136-file live tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,936/1,936 domain cases, clean Release and strict
package checks passed. Package SHA-256:
`CD34E6E571BF39D15F5A848C59E5FA708038DB4FDFE9681CBBAD240200FEFE93`;
DLL SHA-256:
`FCEFCCB9387D4E2A096856FE9DF3C2CDF5E26F219C8D1ADF5F256774679951CB`.
Wasp stays suppressed. Next: species identity, real strike cadence/contact,
safe lifecycle, icon and visual review; then Stirge.

## Sprint 10 owned Wasp species type, 2026-09-27

The Wasp now uses append-only `BlueprintUnitType` identity
`682c4c25e772495e882fc2cacddc0c38`, replacing the inherited
`EagleGiant` inspectable type only on the Wasp. It has a Giant Wasp name
and Lore (Nature) category. Its image is deliberately null under menu
suppression; final type-image and summon-icon disposition remain open.
The manifest is 2,490 total, 2,488 active and two reserved.

Guarded result `20260927T2152154541742Z` passed 23/23 assertions.
The live type matched, native `VerminType` granted `VerminImmunities`, and
RuleApplyBuff marked the Wasp immune/ineligible versus a human
nonimmune/eligible. Neither target received the fixture's control buff.
Diagnostic `20260927T2140542227252Z` is excluded because its extra direct
component-mask condition failed; it did not show an immunity regression.
Restoration `20260927T2155531149844Z-disposable-expanded-summoning.json`
verified the original 136-file tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,937 full domain cases, clean Release and strict
package validation passed. Package SHA-256:
`5DE73D3947814762CFBA25DE56F817D60D4C3FCC1B1B11A4DADF8439B623CB72`;
DLL SHA-256:
`89DB9B4A6336E26719C42425D004061A79B145CFC9FE03AC017ABF9F5FB2E7E4`.
Wasp stays hidden; next are real strike contact,
cadence, lifecycle, art, then Stirge.

## Sprint 10 Wasp two-mode strike cadence, 2026-09-27

The bounded hidden-Wasp flight fixture now requires two native sting rules
on the exact hostile in both modes. Turn-based
`20260927T2217134616132Z` and RTWP `20260927T2220168994981Z` passed;
RTWP needed 145 wait frames. Baked weighted Tail/stinger surface distances
to the target's rendered body bounds were 0.643–0.654 m in turn-based and
0.743/2.04 m in RTWP. These prove real attack cadence but fail visual
contact. Keep all twelve Wasp placements suppressed. The batch's single
restoration `20260927T2223192246293Z-summon-same-turn-activation.json`
returned the original 136-file tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,938/1,938 domain cases, clean Release and strict
package validation passed. Package SHA-256:
`D1DA510EB2504FB597F6B804FE6C99FBF05F6EF309B1FDF3E3720D26066029F0`;
DLL SHA-256:
`404653BC60B5FC371FB56156E64C3F5C027A924C7D5A56E8C9FF99FDF2C31281`.
Next: view-target contact, clipping, lifecycle, icon and visual clarity;
then Stirge.

## Sprint 10 Wasp real-impact camera diagnosis, 2026-09-27

Final guarded turn-based `20260927T2251029714353Z` and RTWP
`20260927T2254096344270Z` both passed exact two-sting mechanics and each
produced two party-camera and two overhead PNGs at native impact. The
party camera has a doorway wall in front of the Wasp; the temporary overhead
camera is restored after every capture. Overhead frames show the Wasp's
abdomen/stinger pointed away from the target, including the intact second
RTWP strike. The first RTWP frame was partly dissolved. Visual strike
contact is a real defect; Wasp remains suppressed. Shared restoration
`20260927T2257092555149Z-summon-same-turn-activation.json` returned the
original 136-file live tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,938 full domain cases, clean Release and strict
package checks passed. Package SHA-256:
`1F1EB4B50AD94D59F0E767002891BB569111E2A11D663D23BE61A1A2A6910954`;
DLL SHA-256:
`E6E757498CC15D6ED7CC71F4EFEE8F17C89487C5CB3DD457D7AB9DA74EC924CD`.
Next: a reversible target-directed tail-pose probe, then contact/lifecycle,
icon and visual clarity, then Stirge.

## Sprint 10 Wasp request-local tail probe, 2026-09-27

Guarded turn-based `20260927T2310527552766Z` and RTWP
`20260927T2314172123373Z` each passed two exact-target native stings.
A request-local probe rotated only the Wasp Tail bone, baked/measured the
mesh, captured an overhead image and restored the exact native rotation in
`finally`. The selected tip moved from 2.04/2.049 m to inside the target
bounds in turn-based. RTWP gaps changed 2.137 to 0.012 m and 3.51 to
1.198 m. The same-frame renders did not show a convincing changed pose;
the second RTWP strike remained geometrically short. No production tail
animation is claimed. All twelve Wasp placements stay suppressed.
Shared restoration `20260927T2317241914821Z-summon-same-turn-activation.json`
verified the original 136-file tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,938/1,938 domain cases, clean Release and strict
package checks passed. Package SHA-256:
`F2C33C487DE6A6DA4034899159CEF58CF2017E935331642942BF9A94CDC601B1`;
DLL SHA-256:
`1156B6674026687DA24C4FD1B76C4BF91181A7199506FCC31C5C000E4BF9903A`.
Next: frame-persistent visual contact strategy, Wasp lifecycle/art review,
then Stirge.

## Sprint 10 Wasp two-frame tail diagnosis, 2026-09-27

The request-local `WaspTailAimFrameProbe` held an aimed Tail transform
through two rendered frames, captured overhead, and restored it on finish,
disable or destruction. Guarded turn-based `20260927T2331283868029Z` and
RTWP `20260927T2334367843649Z` both passed two native exact-target stings.
The delayed pose had a baked tip gap of 0/0 m in turn-based, 0/1.193 m in
RTWP. Delayed images show changed geometry, but the doorway clips the Wasp
body and wings, impact effects obscure the target, and the second RTWP
strike still visibly lacks contact. This is no production animation or
visual PASS. All Wasp choices stay suppressed. Shared restoration
`20260927T2337441522077Z-summon-same-turn-activation.json` returned the
original 136-file installation at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,938/1,938 domain cases, clean Release and strict
package validation passed. Package SHA-256:
`857C4A165E57FCDB4D28F41E2F555CC82101B142DB9857F1223B11BD24E0E26B`;
DLL SHA-256:
`23B7891F480AB812A7348791CC184E3BFAEA8D6972761ADCB4A6677FFBE28CD3`.
Next: Stirge attach/blood-drain implementation, then revisit Wasp visual
contact in an unobstructed fixture and with a revised rig.

## Sprint 10 Stirge rules boundary, 2026-09-27

The [Pathfinder 1e Stirge stat block](https://legacy.aonprd.com/bestiary/stirge.html)
grounds a touch-hit attachment, a +8
racial bonus to maintain a grapple after attachment, one Constitution damage
at the end of each attached turn, detachment after four actual damage, and
10% disease exposure per Stirge. A small `StirgeAttachPolicy` now encodes
the touch/meal boundary and distinguishes requested from actual ability
damage so immune prey cannot advance the meal. It is not wired to a unit,
turn event, grapple/escape or disease lifecycle. Stirge is still unregistered
and unpublished; no gameplay qualification is claimed. Repository validation,
1,939/1,939 domain tests, clean Release and strict package validation passed.
Guarded working-save smoke `20260927T2353348639066Z` passed, and restoration
`20260927T2356206464811Z-working-save-smoke.json` returned the exact
original 136-file installed mod tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Package SHA-256:
`F67E0861D80C882E6CF94AEE24FC415101F72A08ED819B33DDBDF2A84BD3E55A`;
DLL SHA-256:
`6A8A39F0F7F5DD0CE8096555630AD012EA1EA44A502CC4E7A9279A0C630C1853`.
Next: wire and qualify Stirge's dedicated live attachment lifecycle.

## Sprint 10 hidden Stirge registration, 2026-09-28

The append-only ledger now has 2,501 identities, including a hidden Stirge
unit, nine Nature's Ally placements, and a zero-damage touch carrier. All
nine Stirge and twelve Wasp placements remain suppressed; the published
surface stays at 813 generated choices. The Stirge's touch-AC override,
attachment, end-turn blood drain, disease, cleanup, original visual and icon
are still absent. The temporary Eagle donor is a rig candidate, not an
approved Stirge visual. The current weapon audit records the carrier as a
summoning-only exclusion; no player-visible icon or weapon assignment moved.

Repository validation, 1,940/1,940 domain tests, clean exact-reference
Release build and strict standalone package validation passed. Guarded
working-save smoke `20260928T0029440999424Z` passed. The first inventory
run `20260928T0037477029671Z` found the expected 83 units and 834 logical
abilities but exposed a stale safety-audit exception for the existing owned
Wasp poison fact; it is excluded. After an exact fact-name exception, the
repeat `20260928T0052069434655Z` passed 48/48 assertions: 813 visible
placements, zero shared donor components and zero prohibited references.
Restoration `20260928T0055527257092Z` returned the original 136-file live
mod tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Final package SHA-256:
`44512D7EF942D2D19CD470FF2CB70899D522537E789DCEEE24A243DDF26A138A`;
DLL SHA-256:
`587969E44A60626CD87AD8098FDFA1804E27410B9071A9E6C705B0DCA839EA18`.
Next: implement and prove Stirge's dedicated touch/attachment lifecycle,
then qualify its original visual and return to Wasp impact contact.

## Sprint 10 native Stirge touch routing, 2026-09-28

The hidden `StirgeTouch` weapon now clones the installed Shocking Grasp
delivery's native held-touch weapon rather than the bite weapon. Registration
fails closed unless that donor reports `AttackType.Touch`. A new live
inventory assertion requires the Stirge unit's primary hand to reference
that exact weapon with `AttackType.Touch` and zero base dice. Guarded
inventory `20260928T0113159269094Z` and the exact final wording candidate
`20260928T0125544188038Z` each passed 49/49; the latter observed
`type=Touch;dice=0;primary=True`. Restoration
`20260928T0129418890710Z` returned the original 136-file installation
at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
The final source passed repository validation, 1,940/1,940 domain tests,
clean exact-reference Release and strict standalone package checks.
Package SHA-256:
`6F4B2A312DFAF2600222DDAE9CA88F7E9503BA266FE3272D2D0B372EAEDAFFAB`;
DLL SHA-256:
`A66F95442A6641AFF4763177A77E06C8F6FD0BCDE73A10B2DBFC0E9B09F7704F`.
This proves blueprint attack-type routing, not a direct native hit, HP-damage
absence or the attach/drain lifecycle. All nine Stirge choices remain hidden.

## Sprint 10 direct Stirge touch hit, 2026-09-28

The guarded disposable combat fixture summoned the hidden Stirge through its
own Nature's Ally I command and triggered its primary `RuleAttackWithWeapon`
against an armored hostile. The actual attack roll used `AttackType.Touch`,
resolved AC 6 rather than ordinary melee AC 14, hit, and changed HP damage
from 0 to 0. The scenario `20260928T0142219533559Z` passed; restoration
`20260928T0146022240699Z` returned the original 136-file installed tree at
SHA-256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,940 domain tests, clean Release and strict package
checks passed. Package SHA-256:
`03579CB0E9B57AC0E60BF9759267493D72C0EC247DC33D0742F140576D2EA473`;
DLL SHA-256:
`05B7CF3AE20D2E71978EFEC4F52579CBEF4389139741063E91375D54008BB2F8`.
Attachment, blood drain, escape, cleanup, original Stirge visual and icon
remain open, and all nine choices stay hidden.

## Sprint 10 hidden Stirge attachment and first drain, 2026-09-28

Two append-only buff identities now carry Stirge's attack trait and attached
hold. A successful native primary touch hit establishes reciprocal native
grapple parts without a second maneuver. The holder loses Dexterity to AC;
the target receives the shared held state. The dedicated hold component
requests one Constitution damage each round, tracks actual rather than
requested damage, and releases at four actual points or prey death. The
existing area-transition safeguard covers the same native parts. Disease,
multi-round and interruption behavior still require implementation or proof.

Guarded disposable combat `20260928T0232376506044Z` passed the touch,
attachment and first-drain assertions: touch AC 6 versus ordinary AC 14,
no HP damage, reciprocal link, holder/target buffs, lost Dexterity to AC,
one actual Constitution damage (0 to 1), cumulative meal 1 and retained
link. Explicit release cleared both parts and buffs. An earlier attachment
run `20260928T0220384662621Z` passed touch, reciprocal link and cleanup
before the drain assertion was added. Both restored the original 136-file
installation; final restoration `20260928T0236160392850Z` recorded SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Repository validation, 1,940 domain tests, clean Release and strict package
checks passed. Package SHA-256:
`D10EC168E99A24568DDDFDFADF50EA09C2FF60B178C5212637205CD33D84C28F`;
DLL SHA-256:
`FADEBD91075C5BE4042152A168FD0242D8061B207FF49930634B73BEB7375D05`.
All nine Stirge choices remain hidden; this is not full Sprint 10 acceptance.

## Sprint 10 four-point Stirge meal, 2026-09-28

Guarded disposable combat `20260928T0247164550495Z` passed the complete
four-tick meal: actual Constitution damage progressed 0 to 1 to 2 to 3 to 4,
the reciprocal native link remained through point three, and point four
automatically removed both grapple parts, holder/target buffs, and Stirge's
lost-Dexterity condition. The fixture's fallback release was not needed for
that automatic-cleanup assertion. Repository validation, 1,940 domain tests,
clean Release and strict package checks passed. Restoration
`20260928T0250550544603Z` returned the original 136-file installed tree at
SHA-256 `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Package SHA-256:
`822970FA0AB43765AFA6AA0F872DB8C8A3A458E9120F1C6581ECEC2EC59DB087`;
DLL SHA-256:
`E19F45CC51D54C23889E35594EA3D502CEFF825F3AA8E49277050EB9A0812EAB`.
Independent escape, death, dismissal, expiry, area transition, disease,
save/load, original visual and icon still need qualification or implementation.
Stirge stays unpublished.

## Sprint 10 Stirge break-free and area leave, 2026-09-28

Guarded disposable combat `20260928T0304163413890Z` passed two interruption
paths after the full meal. A new touch hit reattached; the victim's native
`UnitHelper.TryBreakFree` succeeded, and the fixture performed the same
target-part removal as the native grapple controller. The Stirge hold then
cleared without another Constitution drain. Another reattachment was
released by `SummonGrappleAreaSafeguard.Sweep(true)` with exactly one target
swept and no remaining parts, buffs or lost-Dexterity state. This proves
the native rule decision and cleanup path; it does not observe the controller's
real-time scheduling. Repository validation, 1,940 domain tests, clean Release
and strict package checks passed. Restoration `20260928T0307556510922Z`
returned the original 136-file tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Package SHA-256:
`AB0BC4A2E926DD21D3792429AE58478B2296DB4F54820877EA9C7D9FDB58CB08`;
DLL SHA-256:
`52B0F26DEDA2B1088507E5548928975813C4B8E99D684154C81301DA0C8D7DD3`.
Death, dismissal, expiry, disease, save/load, original visual and icon remain
open, and Stirge remains unpublished.

## Sprint 10 attached-summon teardown, 2026-09-28

Guarded disposable combat `20260928T0332419433992Z` passed teardown of a
newly attached Stirge: after the native queued entity-destroyer tick, the
summon was destroyed, its victim's target part and held state were gone,
and no further HP or Constitution damage occurred. An earlier immediate
diagnostic `20260928T0320014996249Z` failed because
`UnitEntityData.Destroy()` had only queued teardown; it is excluded from
qualification. Repository validation, 1,940 domain tests, clean Release and
strict package checks passed. Restoration `20260928T0336376497673Z`
returned the original 136-file tree at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Package SHA-256:
`AB5609EC38EA31228145B5C7259880979D1200B8E8FA39BFCCDB549F5C8981A1`;
DLL SHA-256:
`037600735E5E74F368F4DCDD0FD7A33CE08B2D338E3BE4ED8BDB721947E3545D`.
Actual summon timer expiry, prey death, disease, save/load, original visual
and icon still require qualification or implementation. Stirge remains hidden.
