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
- Branch head at intake: `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`;
  first pushed intake checkpoint: `359e346ecb6a094552f096869ecb9c81a2d291bf`.
- Sprint 9 Eagle and Dire Bat passed the internal technical gate. The 14
  preserved Bat placements are published; all 813 generated roots and 29
  native wrappers passed the guarded player path. Original skinned bird/bat
  visuals, Bat icon, Bat 40-foot imprecise blindsense, direct/quantity casts,
  RTWP/turn-based native combat, impact contact, open-floor and obstructed
  room-opening travel, visual lifecycle, save/load/expiry, module-disabled
  safety, donor controls and exact installation restoration are qualified.
  Sprint 10 intake is under way; no Stirge or Giant Wasp creature is published.
- HumanReview: NOT_PERFORMED_NONBLOCKING. Sprint 9 internal engineering,
  fidelity, visual and evidence/restoration reviews passed with the bounded
  limitations below; tranche and later-sprint reviews remain pending.
- Blocker: exact numeric Stirge/Giant Wasp rules are absent from supplied
  local materials, and `AGENTS.md` requires explicit authorization before
  consulting public rules pages over the network. The narrow source-access
  decision and evidence are in `EXPANDED-SUMMONING-PHASE2-BLOCKERS.md`.

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

## Standing boundaries and next action

`OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`.
Active holds and mouth occupancy return cleanly released on reload. Do not
implement hold re-establishment or call this grapple persistence.

Next: after the exact Stirge/Wasp rules source is authorized or supplied,
implement and qualify Sprint 10 Stirge and Giant Wasp, then continue through
the remaining authorized Phase 2 work. The Bat's preserved placements
are published after its model, blindsense, icon and guarded live cast gate.
Continue through A, B and C without an intermediate owner
decision. Open three stacked PRs as the tranche gates pass. No merge, release,
permanent deployment, or Sprint 22.
