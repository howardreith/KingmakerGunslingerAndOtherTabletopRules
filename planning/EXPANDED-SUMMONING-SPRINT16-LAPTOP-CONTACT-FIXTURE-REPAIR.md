# Laptop Sprint 16 contact and fixture correction

Status: **NOT QUALIFIED**. Parent evidence checkpoint
`5da53f84d42079b02586c2db0252652dac4d7dbe`; failed runtime source
`bc36f97612020d6d680144bcb6ebc01bebe11216`.
All failures, immutable hashes and exact restoration remain in the
[laptop batch review](EXPANDED-SUMMONING-SPRINT16-LAPTOP-NATIVE-FACTION-REVIEW.md).
No side-branch source imported; the separate salvage audit remains read-only.

## Demonstrated defects and bounded changes

1. Dire AI attacks have legal 15-foot reach, but the original contact pose was
   capped at 0.25 m. Native AI attacks passed while weighted jaw/tail surfaces
   missed by up to 2.58 m after adjustment (bite gap before adjustment 2.828 m).
   Only Dire's view-local approach now eases to the target surface, capped at
   **3.048 m / 10 feet**, half its printed 20-foot footprint. Ordinary Crocodile's
   original cap/envelope is unchanged. Both still require the exact attached
   owned visual/weapon graph and native attack event. No unit position, reach,
   collision, target transform, animation clock or contact criterion changes.
   Weighted-world contact tolerance remains 0.25 m; unknown creatures get zero.
   The old 0.25 m Dire test encoded an engineering assumption contradicted by
   native reach evidence, not an owner adaptation or accepted contact waiver.
   New behavior checks cover measured gaps, smooth return, overrun cap and
   native Monitor Lizard refusal. Live pose quality/contact remains unqualified.

2. The failed manual-swallow row had a rider but zero issued manual attacks;
   its deliberately unavailable Sprint remained queued. Require a completed
   rejection and a non-opportunity rule event while the actual issued UnitAttack
   is executing before the manual drill can complete or pass. Record per-event
   command provenance and persist each completed cell immediately. Incidental
   events remain visible, not silently counted as requested commands.
   A disposable held-target's existing maneuver-immunity fixture stays active
   during Sprint/rejection setup and is removed immediately before the native
   attack command. No immunity remains for bite/hold/maintain. AI cells retain
   native decisions; no AI attack is injected. The prior inference that the
   incidental event might be an AoO is not promoted to proven fact.

3. Combat teardown now destroys its owned fixture before restoring the mode,
   then restores native group/selection as well as its existing clock/pause/
   awake census. The native UI drill likewise restores selection through
   SelectUnit/MultiSelect and the captured clock before strict equality checks.
   Sheet open/close still uses native APIs; no false visibility assertion or
   weakened restoration tolerance. This addresses the observed changed
   selection and 40 ms clock drift, not installation restoration (which passed).

4. Final UI/lifecycle work now owns a temporary RTWP scope and restores the exact
   original mode/group/selection/clock/pause afterward. Actual RTWP/TB commands
   remain in the separate full eighteen-cell matrix. Native BuffCollection.Tick
   uses turn-start time and an owner/caster current-turn gate in TB; off-turn
   synchronous expiry is invalid. The old laptop fixture did not record enough
   context to prove which gate skipped its ten failed lifecycle cases.
   The corrected expiry helper fails closed in native TB, verifies exact buff
   ownership, records pre/assigned deadlines and the scheduler clock, then uses
   native UpdateNextEvent/Tick. It does not directly remove buffs or claim a
   forced removal as expiry. New runtime rows expose whether the exact buff is
   still attached after that native tick. Fresh persistence continues using the
   same guarded helper and must pass again.

## Focused checks and next gate

Focused **217/217** of 2019 registered PASS; incremental exact-reference Release
compile PASS. The one added registered test exercises the actual manual-drill
completion policy across all boolean states and the 89/90-frame boundary.
Existing bone-policy tests now cover each creature's bounded contact envelope.
This count is derived from the new laptop test, not imported archive test pins.
No visible icon/asset/GUID/version/publication change.

The development metadata independently follows the latest owner order:
active laptop branch, sprints 9-21 authorized in principle, Phase 2C true and
deferred until owner acceptance, current mission stops after Sprint 17/Phase 2B.
PR #26's top notice has been corrected; PR #27 stays archived draft evidence.

Commit/policy-push this coherent correction. On that exact committed head run
full unfiltered suite, repository/static/icon/manifest, runtime preflight,
orchestration, persistence round-trip/rejections, crowd requests, clean exact
Release, deterministic package and strict standalone validation. Then acquire
the runtime lease BEFORE live observation/snapshot and run one immutable six-
scenario fresh-Steam batch. Record all identities/results and restore the exact
actual snapshot. No source PASS or historical partial PASS qualifies these bytes.

Dire remains withheld; Sprint 17 not started. All four accepted decisions and
HumanReview: NOT_PERFORMED_NONBLOCKING remain unchanged. No permanent deployment.

