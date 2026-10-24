# Summon Cleanup Iteration 2 — v0.0.164 RELEASED / STOP

**PR #67 is MERGED and v0.0.164 is PUBLISHED.** On 2026-10-23 the owner
approved the Cleanup 2 artwork ("The artwork looks great") and authorized the
release once the Pro review's checks were clear: the release validator and
Sprint-level gate, merging PR #67, publishing through `Publish-Release.ps1`
and checking the public download. The release changes art, giant presentation
and summon menus, and no identity or route. The source-derived public equation
is unchanged from v0.0.163: **1,127 generated placements + 29 retained
wrappers = 1,156 visible choices**.

- **Merge.** PR #67 merged normally as
  `23952275bb8bbe5a69a8e64a961dd978093a2ec6` (tree `4a8f1791…`). Between the
  gated candidate `dcfffcd602` and the merge, only the unpackaged record
  `06fb138d9f` changed.
- **Gates on the candidate.** The Sprint-level gate passed 2,739 / 2,739,
  including the v0.0.164 release validator, the exact-reference build and
  strict package validation (package `eb60f199…64ae`). The guarded visual
  contracts passed 15/15 and the creature review 32/32 on that package. The
  first candidate `9a123f9675` failed visual contracts 14/1. The cause was the
  measurement, not the art: it read the Brachiosaurus attack gesture's
  deliberate culling envelope (25.035 m) as the body. `7c300008e1` measures the
  gesture-owned skin at its current pose (9.131 m) and keeps every limit. See the
  [release candidate gates](planning/CLEANUP2-PR67-REVIEW-FOLLOWUP.md#v00164-release-candidate-october-23-24).
- **Publication.** `Publish-Release.ps1` ran once from the clean, pushed main
  checkout (`master` fast-forwarded to the merge) with the cleanup2 reference
  bundle, in 624 s. Its build passed 2,739 / 2,739 domain tests, the
  release-mode wrapper with `validate_expanded_summoning_cleanup2_release164.py`
  and strict package validation, and both clean builds produced identical
  bytes.

[Stable v0.0.164](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.164)
is published (2026-10-24T03:47:11Z) and Latest, titled "Tabletop Added Rules
v0.0.164". Annotated tag object `ef6467ee42e084661e0e10dd99637f7e5bf1188e`
peels to the merge commit. The downloaded public assets match GitHub's
digests:

- `KingmakerGunslinger-0.0.164-expanded-summoning-cleanup2.zip`:
  `0a24dd9f8998e43234d64138c481b1969f1f6c671f5bebb2d724f9eb0df0a812`
  (400 files; DLL
  `61c3800017f7c6829310ecd4c63b8d319a0d6946d020be924697e66b2adb5940`,
  MVID `1a40087f-d91a-4952-979d-413d58198578`, assembly version 0.0.164.0);
- `SHA256SUMS.txt`:
  `b4b6b55a2c043f28d32931faff05669899b9b23687e73dedfc1f133f3196642f`;
- `release-manifest.json`:
  `22191e974f1fef021ad6c03e1069457dabcad359dbd5c346ca0ed8f7de1df49d`.

The downloaded ZIP passed strict package validation. Its DLL embeds the merge
commit and `0.0.164-expanded-summoning-cleanup2`. These bytes differ from the
gated candidate because the build stamps the commit and the PDB path. Disassembly
shows the same code: 399 of 400 package entries are identical, and the DLL's IL
differs only in the commit stamp, MVID, image base and the layout addresses of
188 compiler static-data fields, each holding identical bytes. The publisher's
own run qualified the released bytes, and the native evidence stays bound to
the candidate. Nothing was deployed permanently: every guarded run restored the
live mod exactly (254 files, `5C3E4AD4…BFBB`), all 95 saves remain, and
`KMG_AUTOMATION_WORKING` is unchanged.

**Still open (owner decisions, not release blockers):**
- The level-up preview folds a single elemental row behind "Elementals (1)".
- Generated descriptions read "Summons X through the native summon lifecycle."
- Feral Hunter and Preservationist PC menu hovers were last exercised on
  review43; the owner chose not to rerun menu-adapters on the final bytes.
- Owner notes on the Brachiosaurus: the torso reads as a broad dome and the
  tail is short.

See [the cleanup state](planning/EXPANDED-SUMMONING-CLEANUP-ITERATION2-STATE.md)
and [the release notes](docs/RELEASE-NOTES-0.0.164.md). STOP: no further
Cleanup 2 work is scheduled.

# Sprint 29 — RELEASED / STOP

Ettin, Cloud Giant and Storm Giant shipped in stable [v0.0.163](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.163) from [product PR #64](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/64), normal merge `c8c7054b3472e433ecb4bc74ae70a961620655ee`, exact tested source `a4c409c7cad4fdab5c83a6205255e52c29212662`. The 2,700/2,700 Sprint gate, 16 fresh exact-package native reviews, six fresh Working processes, publisher preflight and dry run, and actual public-asset download checks passed. All eight SNA roots are live: 1,127 generated roots plus 29 wrappers = 1,156 visible choices. The downloaded ZIP SHA-256 is `82a4d09d6394d47d3adc6a36bb5d7ee55460fe12793382133779b52d47172df4`; DLL SHA-256 `a7e03aeea0e65dafffffc45bf29f4dbb5a3795d83da947e4effa749d6b946b51`; MVID `d30f4d47-6f91-4ff7-ad58-0f87c56388aa`.

The final full protected intake and original Working bytes/metadata are restored exactly; no owned game process, task, lease, staging or permanent development deployment remains. See the [coverage map](docs/SPRINT29-COVERAGE.md) and [release evidence](planning/SPRINT29-RELEASE-EVIDENCE.json) for the qualified RunIds, asset identities, explicit owner scope decisions and visual backlog. Dated development failures below retain their original verdicts. No owner aesthetic approval is claimed. STOP before Sprint 30.

## Dated development history

Exact `c24a7c1b` passed fresh guarded owner-expiration cases: Cloud's CL16 mist and fog areas kept their finite native durations after the owning summon expired; Storm's Call state and commands ended with its owner while the sibling kept Freedom of Movement and independent lightning uses. The first exact Working-save prepare on `c24a7c1b` **failed**: Ettin's two dormant javelins and Storm's dormant bow were lootable, so no save was written. Its original Working bytes/metadata and full intake were restored; the parent's final comparison also threw from a scratch helper stdout/string bug, then was repaired. Exact `0a578514` passed guarded ON prepare with 19/19 native assertions, but ON cleanup failed: eight Ettin second-head renderer material instances survived the destroyed unit/view and were unresolved to the resource observer. No cleanup save or absent run followed. The parent restored original Working bytes/metadata and all protected intake parts. The exact material ownership/teardown repair is qualified by the `dd676afe` ON/OFF runs above. These earlier failures remain dated development evidence.

Exact `2187c8ed` passed the fresh negative and positive RTWP AI review on a guarded development package. Cloud recognized a request-owned Ettin as an ally near the hostile and chose a resolved attack instead of obscuring that ally; Storm recognized electricity immunity and attacked without consuming lightning resources. Positive controls retained Ettin's four attacks, safe Cloud fog, Storm Chain and Call activation, and two separate autonomous granted Call bolts with real saves and damage. Exact `dcca52ef` passed two fresh guarded turn-based phases: 11/11 Awesome Blow open/blocked assertions and 12/12 native AI assertions across Ettin, Cloud and Storm. The `16441221` cost-comparator and `b5978dde` fixture teardown failures remain dated and nonqualifying. Every transaction restored actual protected intake exactly. Cold-load hydration, affected controls, public routes and release remain.

The `4105be52` Awesome Blow native task exited PASS and restored the laptop, but its frame trace exposed a 34.53 m open push for a 3.048 m request. The PASS predicate lacked an upper bound and is **nonqualifying**. Exact `ae770c32` correctly failed its bounded observer: native Bull Rush queued a 42.0624 m margin-dependent chunk before the private 10-foot chunk. Exact `3718fd94` bounded the open push but failed blocked collision/prone. Exact `37312bd2` passed 15/15 native core RTWP predicates. Exact `f7a19f9f` passed 24/24 fresh seven-case RTWP predicates: bounded open and blocked movement, separate victim collision 1d6, real slam damage, prone, failed maneuver, immune, same-size and incorporeal exclusions, and exact native chunk cancellation without a delayed collision. Each native command spent a standard action. The owner approved documenting the installed engine limitation for the obstacle object's own printed 1d6: inspected static navmesh walls have no damageable HP consumer. The victim's collision damage remains implemented. Turn-based Awesome Blow is separately qualified on its recorded artifact. Every transaction restored actual laptop intake exactly.

Cloud Giant, Storm Giant and Ettin are hidden development candidates on branch `codex/expanded-summoning-sprint29-cloud-storm-ettin`. Native profiles, equipment modes, resolved RTWP attacks and turn-based commands passed on their recorded development artifacts. Exact `b8966336` passed 10/10 direct Cloud fog predicates; exact `32f8be8b` passed 13/13 direct Storm magic predicates after the recorded CL/DC repair. Earlier autonomous AI failures and repairs remain dated in the evidence map. Exact `b6dd3752` passed 13/13 fresh RTWP AI predicates: Ettin chose four flail attacks; Cloud chose safe CL16 Fog Cloud; Storm chose Chain Lightning, then activated CL15 Call Lightning and used its granted bolt with a real DC15 save, damage and finite-state debit. No fixture attack or forced roll was injected. The `c81a9941`, `0ec04e74` and `6307228a` item failures remain dated. Exact `ff42de09` passed 14/14 fresh native item predicates: Ettin quantity siblings owned five unique items each; Cloud and Storm equipment, transfer refusal, death/expiry no-loot and exact party inventory passed. Exact `e4ea70d1` passed 13/13 native disarm predicates, including a real two-attack offhand-only Ettin command and Cloud/Storm slam fallback with no item resupply. The first `ad90de97` Awesome Blow native run failed its open-displacement/prone observation after a real successful maneuver and slam hit; the source now normalizes the native push direction and samples the force-move outcome during the action, pending native recheck. All transactions restored actual laptop intake exactly. Negative fog/target cases, later bolt cadence, turn-based AI, Awesome Blow collision, cold-load hydration, affected controls, public routes and release remain. The source-derived public equation is **1,119 generated placements + 29 retained wrappers = 1,148 visible choices**; all eight new SNA roots remain suppressed. See [the current contract](planning/EXPANDED-SUMMONING-SPRINT29-CONTRACT.md) and [evidence map](planning/SPRINT29-EVIDENCE.json). Historical Sprint 28 release evidence remains below.

# Sprint 28 — RELEASED / STOP

Hill, Stone and Fire Giants shipped in stable [v0.0.162](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.162) from product [PR #62](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/62), merge `f590aa33ced091506510981598eab184bcb293a9`, tested source `b0f5b4bac9d9c78ab30b7a32697d3960b0890ef6`. The exact downloaded ZIP SHA-256 is `4d806ea4d235db95a945ee93b0af4a47c0b92f50ecd9ee44ddb98b6b021c2f12`; DLL SHA-256 `04b7e0cafeb4f03ec6a6e2ec640bfac5aca88b0eea2b55c802da63d3bb13b6b2`; MVID `0ac0af6e-bad3-41d0-a2bd-73fe4bc5e508`.

All eleven SNA-only roots are public. Nineteen fresh exact-candidate native RunIds and the full Sprint gate passed; the final machine and original Working save match intake exactly. See [current status](planning/SPRINT28-STATUS.md), [release coverage](docs/SPRINT28-COVERAGE.md), and [release evidence map](planning/SPRINT28-RELEASE-EVIDENCE.json). The only owner-approved gameplay omission is named Climb for these three and Fire Craft; a future focused pit/climbing review is backlog. Rough body/color/animation polish is backlog, with no owner aesthetic approval claimed. Sprint 29 was not started. STOP after this final records merge.

# Sprint 27 historical release

Griffon shipped in [v0.0.161](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.161) from product [PR #60](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/60), merge 05d75068e2f49a914c5cbeb2499944ff1298273c. The exact qualified source is 63ef0fa4845c7143daa6aa9a1505c059ef61096a and the downloaded ZIP SHA-256 is 7ae697de7b743ba58772a219e105d372a854461007cb4edd12952e11898129de. All six SNA-only roots are public; the source-derived total is 1,108 generated roots + 29 retained wrappers = 1,137 visible choices.

The [current coverage](docs/SPRINT27-COVERAGE.md) and [release evidence map](planning/SPRINT27-RELEASE-EVIDENCE.json) record the full gate, fresh native commands, retained postprocess failure/recovery, reuse audit, public downloaded assets, and exact machine/Working-save restoration. [Development evidence](planning/SPRINT27-EVIDENCE.json) remains historical. HumanReview is NOT_PERFORMED_NONBLOCKING; documented coarse wing/anatomy/transition polish is visual debt.

The final audit found no owned game, task, lease, active compatibility sentinel or staged Mods directory. All 118 saves, including the protected baseline and restored Working fixture, were exact. Sprint 26 remains closed. That historical stop closed Sprint 27; the current Sprint 28 authority and state appear above.
