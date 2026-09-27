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
- Current Sprint 9 sub-item: Dire Bat-only imprecise blindsense and original
  skinned visual. Both source/build-qualified and guarded disposable-cast PASS.
  Eagle visual work, Bat icon integration and publication remain planned.
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
- A generated original Dire Bat icon source is temporarily held under ignored
  `artifacts/phase2/dire-bat-icon-source.png` (SHA-256
  `CC1B58F8802B5795198F37548786E84CA133F084C38CF9AB3167E17866607510`).
  It is not integrated or published. Restore it to the approved source family
  when the Bat is ready; do not mistake the bitmap for owner visual approval.

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

## Standing boundaries and next action

`OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`.
Active holds and mouth occupancy return cleanly released on reload. Do not
implement hold re-establishment or call this grapple persistence.

Next: create and qualify a true Eagle silhouette on the shared flying rig,
then integrate the Bat icon and qualify the full Sprint 9
publication and player path.
Do not unhide Dire Bat until its model, blindsense, icon, mechanics, and live
qualification pass. Continue through A, B and C without an intermediate owner
decision. Open three stacked PRs as the tranche gates pass. No merge, release,
permanent deployment, or Sprint 22.
