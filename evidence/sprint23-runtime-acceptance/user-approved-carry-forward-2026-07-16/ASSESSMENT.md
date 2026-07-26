# Sprint 23 runtime evidence and user-approved carry-forward

**Assessment date:** 2026-07-16  
**Build observed:** 0.0.23-s23-natural-roll-misfire  
**Entry decision:** Sprint 24 approved by the user with explicit carry-forward of unobserved controls

## Observed evidence

The supplied diagnostics and combat cards establish the core natural-roll boundary:

- forced natural 1 entered the configured 1-2 misfire range and ended in a miss;
- forced natural 2 entered the configured 1-2 misfire range and converted a native success to a final miss;
- the forced-2 combat card showed roll 2 plus 11 against AC 9, proving the final miss was not an ordinary armor-class failure;
- forced natural 3 retained Kingmaker's ordinary hit behavior;
- forced natural 20 retained Kingmaker's ordinary critical behavior;
- the diagnostic misfire records reported `conditionTransition=none`, as required by Sprint 23;
- observed duplicate-assignment, duplicate-evaluation, and misfire-fault counters remained zero in the captured panels; and
- captured fired states were empty after the shot.

The strongest forced-2 diagnostic reads, in substance:

```text
naturalD20=2
misfireRange=1-2
nativeSuccess=True
misfired=True
finalSuccess=False
forced=True
conditionTransition=none
```

## Controls not claimed as separately observed

The supplied screenshots do not by themselves complete every formal item in `planning/SPRINT-24-ENTRY-CRITERIA.md`. In particular, this assessment does not claim standalone proof of:

- a genuine native Heavy Crossbow preserving a pending forced roll;
- an empty Test Musket preserving a pending forced roll;
- a Wrecked Test Musket preserving a pending forced roll;
- an eligible `noNaturalRoll` completion preserving a pending forced roll;
- a post-misfire full save, complete process exit, restart, and reload; or
- a single final panel after all ordinary 3/20 checks showing every counter and fault value.

## User-approved decision

After the distinction above was explained, the user directed:

> I think we can assume those are all working OK. Please proceed to the next sprint and we can test those along with the next material.

Sprint 24 therefore proceeds as a deliberate combined-gate exception. The unobserved Sprint 23 controls are not marked passed; they are carried into `SMOKE-TEST-GUIDE-0.0.24.md` alongside the new exact-item Normal → Broken and Broken → Wrecked transitions.

## Engineering consequence

No Sprint 23 repair was indicated by the observed core behavior. Sprint 24 remains bounded to item-owned condition transitions. Sprint 25 must remain blocked until the complete combined 0.0.24 runtime gate passes.
