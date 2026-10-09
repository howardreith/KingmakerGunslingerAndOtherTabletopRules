# Sprint 17 snake native-load readiness repair

Status: exact a429da50 native snake persistence PASS; full Sprint 17 NOT QUALIFIED.
Source parent: 16f61e19d42c8fab918ba1a263ec72f9952990f2.
Laptop PR #26 only. DATA salvage-only / ZERO PORTS.

## Corrected exact runtime result

[Exact artifact, requests, callback trace and restoration](EXPANDED-SUMMONING-SPRINT17-SNAKE-NATIVE-BOUNDARY-PASS-EVIDENCE.json).

Candidate a429da506ebfbbf5c243faa0da163394d32012a2 passes smoke11/11,
prepare14/14, cleanup13/13 and fresh absence6/6 in four fresh Steam640820
processes on one immutable package. Exact prelaunch:272 focused,2074 full
(81.8s), complete178.3s,14-reference Release/strict320,508preflight,
168orchestration,17provenance,persistence11/6/3/56,crowd8/11,launcher13/10 PASS.

Cleanup scenes-loaded4235 still contains the saved Constrictor's native
initiator, Hold buff and CantAct/CantMove. At4252 the existing production
reset removes each; its load-complete callback returns, then the unchanged
strict assertion verifies all four units free and grab retained. No direct
reset, predicate waiver, production repair or reconstructed relationship.

Prepare safely retires the previous four marked units and12 private resources.
Venom source/DC13/counters/2 Constitution damage survive load; one native
later exposure adds2 and a native save cures. Cleanup destroys all four new
units and12 resources, preserving the three unrelated units/party. Exactly
one native prepare write and one native cleanup write; fresh absence0.
The authorized working save is normalized without any save-file access.

Snapshot20261007T0323408975413Z restored2026-10-07T03:37:14.0902498Z:
136files/Info0.0.117/tree216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3.
Lease Completed/recovery=false/released; no game/shared runtime lock/worktree
staging. This qualifies enabled-module snake persistence only. Full Sprint17,
module-disabled matrix, lifecycle/UI/routes, Salamander and publication remain
open. Previous failures and their saved-fixture states below are historical.

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

## Historical pre-candidate source gates

272 focused PASS; 2074 complete unfiltered PASS (81.4s).
Repository/static/icon/manifest, clean 14-reference Release, deterministic
package and strict 320-member validation PASS; complete source gate 179.6s.
Dirty-source artifact not deployed. Policy-push, then repeat every exact-head
prelaunch gate and the complete affected smoke/prepare/cleanup/absence
protocol on one immutable package. Native reset must now pass in reality;
an observed callback alone cannot qualify it.

## Historical failed diagnostic restoration and saved fixtures

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
