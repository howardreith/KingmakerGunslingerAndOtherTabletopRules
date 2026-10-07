# Sprint 17 snake load-boundary trace

Status: SOURCE PASS; runtime NOT RUN; full Sprint 17 NOT QUALIFIED.
Source parent: 15070179e84d799d476ce90a7b205d0e950ec52d.
Laptop PR #26 only; DATA salvage-only / ZERO PORTS.

## Why this observation changes

The exact a5875a45 diagnostic established a Constrictor owner-side orphan:
native initiator, Hold buff, CantAct and CantMove remain; prey is free, Grab
exists, project links are empty, and no appearance lock remains. The strict
reset assertion has failed twice. No wait increase or assertion waiver.

Pinned native assembly
3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB
was inspected offline without executing native methods or exporting assets.
Its Game.OnAreaLoaded dispatches scenes-loaded and scene/activation events.
The separate AreaLoadingComplete coroutine yields once before dispatching
IAreaLoadingStagesHandler.OnAreaLoadingComplete. EntityPool.All returns its
underlying HashSet. UnitEntityData.Remove delegates to UnitDescriptor.Remove;
native grapple-part OnRemove removes its owned buff/control conditions.
These facts motivate a live ordering/input census; they do not prove that the
fixture observed too early, that a unit was missing, or that cleanup threw.

Private audit script/output hashes:
- Inspect-Sprint17-NativeLoadReset.ps1: 54685bdf0e8f95cd6a9ac60388da89927fbbcbe652cc2df8862fa70790ae9db7
- sprint17-native-load-reset-audit.txt: 17081160efe05c909c94624693d43b8cd6e1d54e85e51e0dc5b56992281831ff
- Inspect-Sprint17-NativeLoadResetParts.ps1: c0b43446cc423ff0bc8546cdfdb31144f8718c95bcc011ba7d456b055c965067
- sprint17-native-load-reset-parts-audit.txt: 14ac62cbfc0349057829966cf3e1a12be71fe51a08958b9727b4c5ea795dc6e5
Private IL and reference assemblies are not committed.

## Bounded observer

Only the existing three persistence scenarios with exact scope "snakes" arm
this observer, after the guarded request has been accepted and claimed.
One domain test exercises the closed scenario/scope cross-product.

Read-only Harmony prefixes/postfixes observe the project's scenes-loaded,
load-complete and ResetLoadedGrapples methods. Records contain native frame/
time, subscription status, exact reset input IDs, State.Units.All membership,
party HoldingState membership and each KMG unit's relationship/control state.
The existing assertion takes a contemporaneous snapshot. Up to 32 records;
overflow or read errors fail observation. Relevant native safeguard errors
are captured without suppressing them.

No event/reset is invoked, return value changed, original skipped, save
authority expanded, part/buff/condition removed, clock advanced, unit
collection mutated, or relationship reconstructed. No production behavior,
assets, routes or suppression changes. The strict existing reset predicate
and original 13/12/5 assertion counts remain unchanged.
Hooks and log subscription are removed at scenario completion, including
failure/timeout completion; only these exact observer methods are unpatched.
The trace is written atomically to its authorized request evidence directory.

## Gates and next action

Initial full suite: 2073/2073 PASS (83.2s); the first Release attempt rejected
a missing explicit compile-list entry for the new observer. That source-build
failure is retained and was never deployed. The entry was added; isolated
exact-reference compile PASS. Complete repeated source gate PASS: 271 focused, 2073 unfiltered (81.2s),
repository/static/icon/manifest, clean 14-reference Release and strict
320-member package; complete gate 176.5s. Dirty output was not deployed.

Next: policy-push the source-qualified checkpoint, run every exact-head
prelaunch gate, freeze the immutable package and execute the entire affected
smoke/prepare/verify-cleanup/verify-absent protocol. Interpret actual callback
order/input/output before any production correction. Do not call reset from
the fixture to obtain a pass.

Working save still contains four marked a587 fixture units. Existing
ownership-checked native prepare retires only those; no save-file access or
protected-baseline load/write. Latest actual installation snapshot
20261007T0204316627378Z was exactly restored at 2026-10-07T02:14:37.9430184Z
(136 files, Info 0.0.117, tree
216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3).
No current runtime transaction.

Then remaining snake lifecycle/UI/private 32 routes, separate Salamander,
full Sprint 17 hidden/publication and Phase 2B closure. Stop for owner review.
Phase 2C authorized but deferred. HumanReview: NOT_PERFORMED_NONBLOCKING.
