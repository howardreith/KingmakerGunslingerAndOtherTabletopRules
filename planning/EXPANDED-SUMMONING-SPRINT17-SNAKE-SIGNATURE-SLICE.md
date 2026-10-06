# Sprint 17 closed snake signature rules slice

Status: exact dc1208d2 runtime FAIL61/62; phase-aware observer NOT RUN.
Laptop PR26 only; failed candidate dc1208d2572d254f6cf3c1506f19503215e76808.
Sprints14–16 complete.32 snake roots remain withheld;1005 visible choices.
No DATA ports, version change, publication or new adaptation.

## Latest runtime and separate native save phases

[Exact Attempt3 evidence](EXPANDED-SUMMONING-SPRINT17-SNAKE-SIGNATURE-ATTEMPT3-EVIDENCE.json).
Exactdc12 full2051 tests85.5s/complete181.7s/strict320/all prelaunch PASS;
smoke11/11,signature61/62 FAIL. Six damage events, live ticks6 and removal with
zero seventh/duplicate damage passed. Five saves were buff-owned, not six.
All38 profile/body and13 Constrictor checks pass; result remains FAIL.

Native audit explains the initial boundary: ContextActionSavingThrow executes
the injury gate before applying poison; RuleSavingThrow/RulebookEvent constructors
and MechanicsContext.TriggerRule leave its Reason null. BuffPoisonStatDamage
OnFactActivate deals exposure1 and increments ticks without a second save.
OnNewRound makes the five later buff-owned saves/damage events.
Private audit log SHA26dce5ebb45d8ccf4238a2e1b803c619229464b990b2435486f89980781de8d0.

The new observer null-guards Reason and captures the initial Fortitude save
only while the exact synchronous owned wounding bite is in Rulebook.CurrentContext.
It records that save separately from five exact venom/owner saves; all must be
DC13 and failed for this seeded cadence cell. Six native damage events and
their actual1..2 damage/pre-difficulty1..2 bonus remain mandatory, plus live
counter6, final absence and zero exhaustion/replay events. No production fix,
extra wait or waiver. Two behavior tests cover each phase and reject duplicates.
251 focused,2053 unfiltered86.8s,complete repository/static/icon/manifest,
clean14-reference Release/deterministic strict320 package183.6s,142 orchestration
PASS. Initial source gate rejected stale2051 development metadata; updated only
that count to2053; immutable1992 published record unchanged. Logs
`artifacts/sprint17-signatures-native-phases-dirty-gate-2.log` and preceding
failed metadata log retained. New exact artifact next; repair runtime NOT RUN.

Actual snapshot1923327993552Z restored19:30:35.0087296UTC:136/.117/exact tree;
lease Completed/recovery=false/released, no game/lock/staging/save write.

## Attempt2 retained: changed from disposed counters to live events

[Exact Attempt2 evidence](EXPANDED-SUMMONING-SPRINT17-SNAKE-SIGNATURE-ATTEMPT2-EVIDENCE.json).
Exactccba full2051 tests84.0s/complete gate180.7s/strict320 and all prelaunch
PASS. Steam smoke11/11, signature61/62 FAIL; all62 ran. All13 Constrictor
assertions now pass, including an actually established holder's death and
native controller cleanup.

The remaining failure is the same cadence assertion's post-disposal inspection.
Six native exposures passed; the exhausted callback removed poison with no
seventh or duplicate damage. Removal discards the component array, so reading
its old counter afterward returned-1. This is not a production poison failure,
but the recorded result remains FAIL.

After two failures of that assertion, observation strategy changes: capture
the last LIVE counter, and observe RuleDealStatDamage/RuleSavingThrow events
while they occur. The observer is read-only, filtered to the exact owned target
and venom fact, and unsubscribed in finally. Require six1d2 stat-damage events
and six native saves, no further event/damage at exhaustion or replay, and
actual victim-buff absence. No disposed component inspection, increased wait,
production mutation or waived requirement.

Actual snapshot1857178703604Z restored19:04:19.5799121UTC:136/.117/exact tree;
lease Completed/recovery=false/released, no game/shared lock/staging/save write.
Event-observer repair source gate PASS:249 focused,2051 unfiltered83.0s;
complete repository/static/icon/manifest, clean14-reference Release and
deterministic strict320 package178.8s;142 orchestration checks.
Log: `artifacts/sprint17-signatures-event-observer-dirty-gate.log`.
This repair became the separately recorded dc1208d2 Attempt3 above.

## Attempt1 retained and first boundary repair

[Exact failed artifact/results/restoration](EXPANDED-SUMMONING-SPRINT17-SNAKE-SIGNATURE-ATTEMPT1-EVIDENCE.json).
Complete source/prelaunch PASS:249 focused/2051 full84.2s, complete gate181.9s,
clean14-reference Release/strict320,502 preflight,142 orchestration and all
ancillary tests. Fresh Steam smoke11/11; signature60/62, all62 executed.
All38 inherited profile/body and22/24 signature assertions passed.

Both failures are fixture scheduling assumptions supported by the exact
native assembly audit, not demonstrated production defects. Native poison
increments its sixth exposure before removing at the next exhausted callback.
The repaired assertion still requires exactly six1d2 events and now explicitly
proves no seventh/duplicate damage and removal at that native boundary.
The owner-death case never established its second grapple: the preceding
synchronous lethal-prey case had not allowed UnitGrappleController to clear
the initiator part. The repair invokes that native per-unit callback only for
the two owned actors, records before/after initiator state, and strengthens
cleanup to require both parts and both buffs absent. It does not directly
remove the part or change production. All62 acceptance identities remain.

Snapshot1831551306710Z restored18:38:54.4691641UTC:136 files,Info0.0.117,
tree216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3.
Lease Completed/recovery=false/released; no game/shared lock/staging/save write.
Failed package, results, driver, extraction and audit remain preserved.
Repair source gate PASS:249 focused;2051 full85.1s; complete repository/static/
icon/manifest, clean14-reference Release/deterministic strict320 package181.3s;
142 orchestration. Log `artifacts/sprint17-signatures-boundary-repair-dirty-gate.log`.
Commit/push NOT QUALIFIED, then all exact-head prelaunch checks and the complete
affected smoke/signature batch. Dirty diagnostic package must not be deployed.

## Closed scope

New request `disposable-expanded-summoning-snake-signatures` accepts only
`saveName=KMG_AUTOMATION_WORKING`. Unknown parameters, creature/prefab
selectors, protected saves and save-writing input are rejected. The previously
qualified38-check profile/body request is unchanged. The new request adds
24 signature assertions to those38, with46 ordered metadata rows total.
An exception or reduced assertion set is failure, not partial qualification.

These are seeded native RULE cases, not real command/AI qualification.
Only newly created fixture actors are modified. No shared blueprint, party,
original unit, global clock, visibility or rule-result mutation is permitted.
Native creation/destruction, saved environment and original-census cleanup
remain mandatory. There is no save-write stage or save-file access.

Viper: native owned Constitution1d2/six-exposure/one-Fortitude-cure poison;
actual wounding/missing/zero-damage bites and wounding non-bite rejection;
live Constitution14/18/8 and DC13/15/10; exactly one application with source
identity; native initial/later exposure scheduling, duplicate/early tick
rejection, expiry and cure; poison remains victim-owned with unchanged DC
after exact source destruction. Destruction is not claimed as a death test.
The inert target's Fortitude/Constitution inputs and seeded rolls are disclosed,
restored in finally blocks, and do not qualify ordinary combat difficulty.

Constrictor: bite-only, hit-only and same-size-or-smaller grab; one actual
bite/grapple/initial constrict; no application-frame maintain; one later
native held-round maintain with bite plus constrict, no second attack and
no replay; live Strength17/21/7 yields1d4+4/+7/-2; native growth yields1d6+10;
restoration yields1d4+4. Exact source, physical form and event counts are read
from RuleDealDamage. Terminal cases settle actual native life-controller death,
then prove lethal-prey/owner cleanup and dead/destroyed-prey rule rejection.
The isolated grapple fixture uses BAB100 solely for deterministic sequencing;
printed attack bonuses are independently checked by the unchanged profile slice.

## Collector and prelaunch

The separate collector requires all62 exact named PASS assertions, all22
inherited profile/body rows and24 exact ordered signature rows with Boolean
verdicts. It cannot accept profile-only evidence, duplicates, malformed/nested
root arrays, foreign names, missing rows or string-valued truth. The closed38
collector remains unchanged.142 orchestration assertions PASS.

249 focused/2051 registered tests PASS. First complete source attempt passed
all2051 tests85.1s but correctly failed compile because the new file was not
listed in the project. The project entry and missing enum imports were fixed
through focused compilation; retained logs distinguish those source failures
from runtime, which has not occurred. The stale2050 development-count guard
was updated to2051; immutable published-version1992 evidence is unchanged.

Corrected precommit gate PASS:2051 unfiltered tests84.5s; complete repository/
static/icon/manifest, clean14-reference Release and deterministic strict320
package183.9s. Standalone request preflight502 PASS; orchestration142,
provenance17, persistence11/3/19, crowd5/7 PASS. Logs
`artifacts/sprint17-snake-signatures-dirty-gate-3.log`,
`artifacts/sprint17-signatures-precommit-preflight.log` and ancillary logs
remain machine-local. Dirty diagnostic packages are not runtime candidates.

Historical initial next action: commit/push this NOT QUALIFIED checkpoint, then repeat exact-head gates
and preserve immutable hashes. Run
fresh Steam640820 smoke and this closed request under one lease acquired before
installation observation/snapshot/deployment. Restore the actual snapshot;
require no game/shared lock/staging and unchanged working-save fingerprints.
Do not deploy a dirty diagnostic package.

This slice cannot close Sprint17. Actual RTWP/turn-based manual and AI commands,
measured contacts, full lifecycle, persistence/crowds/UI/routes and separate
Salamander remain required before the full hidden/publication/tranche gates.
Phase2C authorized but deferred until Phase2B owner acceptance.
HumanReview: NOT_PERFORMED_NONBLOCKING.
