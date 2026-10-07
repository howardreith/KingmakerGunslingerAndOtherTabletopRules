# Sprint 17 snake native-load readiness repair

Status: SOURCE PASS; corrected runtime NOT RUN; Sprint 17 NOT QUALIFIED.
Source parent: 16f61e19d42c8fab918ba1a263ec72f9952990f2.
Laptop PR #26 only. DATA salvage-only / ZERO PORTS.

## Observed fixture defect

[Exact diagnostic artifact and evidence](EXPANDED-SUMMONING-SPRINT17-SNAKE-LOAD-BOUNDARY-EVIDENCE.json).

The unchanged 16f61e19 artifact passed smoke 11/11 and prepare 13/13.
Cleanup failed 10/12; absence was correctly NOT RUN. The same mandatory reset
assertion was observed at the wrong native lifecycle boundary:

| Cleanup event | Native frame | Observation |
| --- | ---: | --- |
| Scenes loaded | 4229 | Loaded Constrictor has initiator/Hold/control locks; subscribed safeguard; unit in native pool. |
| Reset assertion | 4229 | Fixture inspects the same state before the load-complete callback. |
| Fixture completion | 4234 | Four units already destroyed in memory; no load-complete/reset callback recorded. |

Prepare independently proves the real callback is later: scenes-loaded at
4221 saw the previous four saved fixtures; load-complete/reset at 4238 saw
their replacements, because prepare had already retired and respawned them.
The subscribed safeguard returned normally on the new not-yet-held units.
No relevant native safeguard error was recorded. This rules out missing
native-pool membership/subscription for the measured units; it does NOT yet
qualify production reset of loaded held units at its actual completion boundary.

Failure classification: fixture ordering; dependent fail-closed cleanup save
rejection. No environment/restoration failure. Do not change production
grapple behavior to accommodate an assertion that ran before its callback.

## Bounded correction

Before any unit snapshot, stale retirement, creation or inspection, each of
the three snake persistence stages must observe the existing safeguard's
native load-complete callback RETURN for the exact state/area whose scenes-
loaded callback was observed. New loads invalidate previous readiness, even
when references repeat; null/foreign/preceding-frame completion is rejected.
A 600-frame bounded observation budget fails closed without fixture unit
mutation or save if native completion is missing. This is event readiness,
not a fixed delay or a wait for the hold state to happen to look clean.

The request-local trace records the readiness boundary. Its own read errors
also prevent readiness. No event is raised, reset invoked, part/buff/condition
removed, clock advanced or relationship reconstructed by the readiness gate.
Production cleanup, mechanics, assets and suppression are unchanged.
The original strict reset predicate is unchanged. One new mandatory native-
boundary assertion makes next-candidate totals prepare 14, cleanup 13,
absence 6; previous 16f diagnostic counts remain 13/12/5.

Behavior tests cover missing/unpaired callbacks, scenes-visible-but-incomplete,
wrong state/area, nulls, preceding frames, exact completion, stale completion
after another load, and same-frame genuine completion without an invented
minimum delay. Existing exhaustive reset-predicate tests remain.

## Source gates and next exact gate

272 focused PASS; 2074 complete unfiltered PASS (81.4s).
Repository/static/icon/manifest, clean 14-reference Release, deterministic
package and strict 320-member validation PASS; complete source gate 179.6s.
Dirty-source artifact not deployed. Policy-push, then repeat every exact-head
prelaunch gate and the complete affected smoke/prepare/cleanup/absence
protocol on one immutable package. Native reset must now pass in reality;
an observed callback alone cannot qualify it.

## Restoration and saved fixtures

Batch artifacts/laptop-runtime-20261007T0256127798172Z.
Snapshot 20261007T0256135125979Z restored at 2026-10-07T03:06:24.8492046Z:
136 files / Info 0.0.117 /
tree 216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3.
Lease Completed/recovery=false/released; no Kingmaker/shared runtime lock/
worktree staging. Exact installation restoration verified.

One authorized native prepare save; cleanup wrote zero. Four new marked
fixtures remain saved: Viper40e1f272, Constrictor5afa284f, venom-target2cfa95a4,
hold-target0c9ddc6d (full IDs in the evidence). Existing native prepare must
retire only those exact receipt-owned units. No save-file access/surgery,
protected-baseline load/write or absence claim.

Then remaining lifecycle/UI/private 32 routes, separate Salamander, complete
Sprint 17 hidden/publication and Phase 2B closure. Stop for owner review.
Phase 2C authorized but deferred. HumanReview: NOT_PERFORMED_NONBLOCKING.
