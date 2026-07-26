# ADR-0029: Detect firearm misfires at exact main-roll assignment and success evaluation

- **Status:** Accepted for Sprint 23
- **Date:** 2026-07-15

## Context

The 0.0.22.1 runtime gate proved that exact marked firearms can persist item-owned state, reload atomically, consume one loaded round, reject empty and Wrecked attacks, preserve Broken condition through discharge, and exclude a native Heavy Crossbow.

Sprint 23 requires the smallest natural-d20 slice. Exact inspection of Kingmaker 2.1.7b shows that `RuleAttackRoll.OnTrigger` obtains `RulebookEvent.Dice.D20`, assigns it through the private `Roll` setter, then immediately passes the implicit integer value to public `IsSuccessRoll(int)`. Critical confirmation uses a different property and calculation.

A global dice patch would be unnecessarily broad. A transpiler would be fragile. Applying condition damage at the same time would make a failed live test ambiguous between roll detection, miss enforcement, and persistence mutation.

## Decision

After loaded-round enforcement successfully commits one round for an exact firearm, register that specific `RuleAttackRoll` object in a short-lived weak context.

Patch exactly:

```text
private void RuleAttackRoll.set_Roll(RulebookEvent.RollEntry value)
public bool RuleAttackRoll.IsSuccessRoll(int d20)
```

At `set_Roll`:

- observe the final main natural d20;
- optionally replace it with the single queued development-only value;
- preserve unrelated dice and critical-confirmation rolls; and
- consume the queue only for the registered exact-firearm attack.

At `IsSuccessRoll`:

- require the argument to match the observed final main d20;
- classify `naturalD20 <= firearm.MisfireValue` as a misfire;
- preserve Kingmaker's ordinary boolean above the threshold; and
- force only a misfire result to `false`.

Remove the short-lived context when `RuleAttackRoll.OnTrigger` completes. If no main roll was assigned, report that fact and preserve a pending forced roll.

Do not mutate firearm condition in Sprint 23.

## Rejected alternatives

### Patch the global d20 generator

Rejected because it could alter initiative, saves, skills, enemy attacks, native weapons, and other unrelated rolls. It would also make exact firearm isolation difficult to prove.

### Transpile `RuleAttackRoll.OnTrigger`

Rejected because the exact setter and public evaluator provide narrower stable interception points and executable reflection contracts.

### Infer misfire from the combat-log result

Rejected because the log is downstream presentation and does not expose a reliable final natural d20 boundary.

### Apply condition transitions immediately

Rejected for this sprint. Separating classification from item-state mutation makes the runtime evidence diagnostic: natural-roll binding and forced-miss behavior can be accepted before adding Normal → Broken and Broken → Wrecked transitions.

### Consume a queued roll on any firearm attack command

Rejected. Empty and Wrecked attempts do not reach a valid natural firearm roll, and native Heavy Crossbows are outside the firearm domain. The queue must survive those controls.

## Consequences

- Test Musket natural 1-2 can be proven deterministically without changing the global dice subsystem.
- Native Heavy Crossbows, empty firearms, and Wrecked firearms cannot consume the forced-roll queue.
- Misfires consume the already-fired round exactly once because discharge remains the earlier authoritative transaction.
- Attacks completing before main-roll assignment have no misfire classification and preserve the queue; diagnostics expose this through `noNaturalRoll`.
- Exact patch-contract tests fail closed if the private setter or public evaluator shape changes.
- Sprint 24 remains bounded to condition transitions after 0.0.23 passes its live gate.
