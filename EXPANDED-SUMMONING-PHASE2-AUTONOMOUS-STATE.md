# Expanded Summoning Phase 2 autonomous state

This is the live resume record for the owner's Phase 2 Sprints 9-21 mission in
`Kingmaker_Expanded_Summoning_Phase2_Codex_Starter_Packet.zip`. That order
supersedes the Phase 1 handoff's historical prohibition on starting Sprint 9.

## Current position

- Tranche: 2A, Sprints 9-13. Current sprint: 9, Eagle and Dire Bat.
- Branch: `codex/expanded-summoning-phase2a-sprints9-13` in
  `.worktrees/expanded-summoning-phase2a`.
- Accepted dependency: `master` / `origin/master` at
  `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`, the merged PR #24 Favored
  Class integration. The owner packet's expected `2943a02d` is its ancestor.
- Branch head at intake: `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`;
  first pushed intake checkpoint: `359e346ecb6a094552f096869ecb9c81a2d291bf`.
- Current Sprint 9 sub-item: Dire Bat blindsense, original Bat/Eagle skinned
  visuals and Bat icon. The 14 preserved Bat placements are now published:
  all 813 generated roots and 29 native wrappers passed the guarded native
  player path; the 18-parent inventory/menu/icon audit passed after an exact
  Bat feature audit correction. Remaining Sprint 9 gates are explicit
  motion/contact, both combat modes, save/load and module-disabled safety.
  No Phase 2 creature has been claimed complete.
- HumanReview: NOT_PERFORMED_NONBLOCKING. Internal Phase 2 reviews: pending.
- Blockers: none established. A difficult rig is not by itself a blocker.

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

## Standing boundaries and next action

`OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`.
Active holds and mouth occupancy return cleanly released on reload. Do not
implement hold re-establishment or call this grapple persistence.

Next: finish Sprint 9 motion, impact, RTWP, turn-based, save/load and
module-disabled gates, then advance to Sprint 10. The Bat's preserved placements
are published only after its model, blindsense, icon and guarded live cast gate.
Continue through A, B and C without an intermediate owner
decision. Open three stacked PRs as the tranche gates pass. No merge, release,
permanent deployment, or Sprint 22.
