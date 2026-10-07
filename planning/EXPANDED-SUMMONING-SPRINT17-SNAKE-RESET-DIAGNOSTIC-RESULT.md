# Sprint17 snake loaded hold-reset diagnostic result

Status: diagnostic complete; gameplay NOT QUALIFIED.
Exact candidate a5875a45305f14c8036b637d44e835532ac0f084.
[Exact artifact, request/result hashes, native observations and restoration](EXPANDED-SUMMONING-SPRINT17-SNAKE-RESET-DIAGNOSTIC-EVIDENCE.json).
Laptop PR26 only; DATA salvage-only / ZERO PORTS.

## Concrete failure

Smoke11/11 and prepare13/13 PASS; cleanup10/12 FAIL; absence NOT RUN.
The same strict reset assertion fails a second time, now with every operand
observed. No wait increase, reset call, changed predicate or gameplay change.

Constrictor34163738-3e5b-4d62-9f5e-8adc5f795e29 retains:
- Native UnitPartGrappleInitiator targetingcc007436-bd8d-41d0-8eab-9935628ef60a.
- Permanent KMG_Summoning_Special_Grapple_Hold (5af4e99c8e5744a6b978412b126d25f0),
  whose source is the Constrictor itself.
- CantAct and CantMove; CanAct/CanMove are false.

Its project link part exists but has0links. Its Grab component is present.
The hold-target has no native target part, held buff or control lock.
Viper and venom-target are also free. Every native appearance-lock flag is
false. Thus missing Grab and transient appearance lock are not explanations.
The owner is orphaned at this observed load boundary. The accepted session-
scoped reset policy remains mandatory; no re-establishment.

## Source trace boundary, not yet a root-cause claim

Main.Load subscribes SummonGrappleAreaSafeguard before the runtime runner.
OnAreaLoadingComplete calls ResetLoadedGrapples(Game.Instance.State.Units.All),
then Sweep(false). The former explicitly removes each selected KMG owner's
initiator and Hold buff. The latter examines party members for holds.
The persistence fixture uses a broader state/party-HoldingState collection.
Which units the real callback saw and when it ran are not recorded yet.

SummonHoldComponent.HeldTarget requires a reciprocal native target part;
for the measured orphan it returns null. OnNewRound immediately returns for
null target. Merely increasing waits is therefore not a justified correction.
Trace production collection coverage and callback delivery/order; use a
bounded read-only census if necessary, then fix the demonstrated miss.
Do not call ResetLoadedGrapples from the fixture to manufacture a pass.

## Saved fixture and restoration

Prepare safely recognized/retired the previous4receipt-owned units and their
12owned resources. It created four new fixtures, established native venom/hold,
and made one native working-save write (two stashed areas). No save-file access.

Reload preserved exact venom DC13/tick1/saves0/2Con damage/source and views/
skills. One later exposure and one native successful-save cure passed.
Cleanup natively destroyed all4units/12resources and preserved unrelated3units,
but correctly refused its native save because the reset assertion failed.
The write failure is dependent. FOUR NEW MARKED FIXTURES REMAIN SAVED.
No absence claim or protected-baseline load/write. Existing native prepare
has now proved safe stale-fixture recovery; retain that exact ownership guard.

Actual snapshot20261007T0204316627378Z restored2026-10-07T02:14:37.9430184Z:
136files/Info0.0.117/tree216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3.
Lease Completed/recovery=false/released; no game/shared runtime lock/worktree
staging. No environment/restoration failure. Extraction and all artifacts kept.
Installation is restored; working-save cleanup is outstanding.

Exact prelaunch270focused/2072full83.0s/fullgate178.3s,508preflight,
168orchestration,17provenance,persistence11/6/3/56,crowd8/11,
actual-launcher13/10,clean14-reference Release/strict320 PASS.
Three fresh Steam640820 processes. Earlier ec direct14/crowd18 and983
rules62/commands51 qualifications remain exact-artifact bounded.

## Continuation

Preserve/policy-push this NOT QUALIFIED checkpoint. Trace the load safeguard,
apply only an evidence-supported bounded correction, retain strict reset
assertions and add relevant shared-seam regressions if production changes.
Repeat complete affected guarded persistence with exact restoration.
Then remaining lifecycle/UI/private32routes, separate Salamander, fullSprint17
hidden/publication and Phase2B closure.32 roots stay hidden.
HumanReview: NOT_PERFORMED_NONBLOCKING. Phase2C authorized but deferred;
stop after Phase2B owner review.
