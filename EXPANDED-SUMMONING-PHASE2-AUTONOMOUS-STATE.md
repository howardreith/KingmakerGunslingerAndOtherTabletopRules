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
- Current Sprint 9 sub-item: Dire Bat-only imprecise blindsense feature at
  40 feet. Source/build-qualified, pending guarded in-game assertion. Eagle
  and Bat visual work, icon integration, and Bat publication remain planned.
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
- The guarded `disposable-expanded-summoning` scenario now asserts the
  exact feature and native live sense part on every Bat cast, and absence from
  Eagle, Pteranodon, and Roc. It has not yet been run on this candidate.
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

## Standing boundaries and next action

`OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`.
Active holds and mouth occupancy return cleanly released on reload. Do not
implement hold re-establishment or call this grapple persistence.

Next: commit and push the source-qualified blinded-sense candidate, then run
guarded `disposable-expanded-summoning` through the Steam harness on the clean
candidate and inspect its sense assertion and restoration. Continue with an
instance-local Eagle/Bat visual extension with native fallback.
Do not unhide Dire Bat until its model, blindsense, icon, mechanics, and live
qualification pass. Continue through A, B and C without an intermediate owner
decision. Open three stacked PRs as the tranche gates pass. No merge, release,
permanent deployment, or Sprint 22.
