# Natural-roll misfire detection

## Sprint 23 foundation retained by Sprint 24

Version 0.0.23 established classification and miss enforcement for the final main natural d20 of a successfully discharged exact firearm. Version 0.0.24 retains this layer unchanged and delegates item-owned condition consequences to `FIREARM-MISFIRE-CONDITION-TRANSITIONS.md`.

For the diagnostic Test Musket:

```text
misfire range: natural 1-2
ordinary range: natural 3-20
```

A misfire can only change an ordinary Kingmaker success to failure:

```text
finalSuccess = nativeSuccess && naturalD20 > misfireValue
```

The loaded-round transaction has already completed before this rule runs. Therefore a detected misfire still consumes exactly one round from the exact firing item and consumes no additional Black Powder Charge or Lead Ball from shared inventory.

## Exact Kingmaker 2.1.7b contracts

Private-reference inspection establishes the main attack sequence:

```text
RulebookEvent.Dice.D20
RuleAttackRoll.set_Roll(RulebookEvent.RollEntry)
RuleAttackRoll.get_Roll()
RollEntry.op_Implicit(RollEntry)
RuleAttackRoll.IsSuccessRoll(int)
```

The implementation uses two exact, fail-closed Harmony targets:

```text
private void RuleAttackRoll.set_Roll(RulebookEvent.RollEntry value)
public bool RuleAttackRoll.IsSuccessRoll(int d20)
```

The setter prefix observes the final `RollEntry.Value` selected by Kingmaker. When a development-only forced roll is queued, it replaces that value and the last history entry only for the registered exact-firearm attack. The `IsSuccessRoll` postfix preserves Kingmaker's native boolean above the threshold and forces it to `false` within the threshold.

The critical-confirmation roll uses a different property, `CriticalConfirmationRoll`, and is not a force-roll target.

## Eligibility and isolation

Misfire context is registered only after all of the following have succeeded:

1. the attack exposes exactly one Gunslinger firearm marker;
2. the concrete runtime weapon item is resolved;
3. item-owned firearm state is read without conflict;
4. the state permits discharge; and
5. the exact item commits the one-round transition.

This ordering provides the isolation guarantee:

- a native Heavy Crossbow has no exact firearm marker;
- an empty firearm is rejected before context registration;
- a Wrecked firearm is rejected before context registration; and
- a duplicate attack callback cannot commit a second round or register a second context.

Those paths cannot consume the process-local forced-roll slot.

## Attacks without a natural d20

Kingmaker can complete an attack before assigning the main `Roll`, including auto-hit handling or an earlier concealment/miss-chance termination. The attack may already have discharged its round, but there is no natural d20 to classify.

At the `RuleAttackRoll.OnTrigger` postfix, the short-lived context is removed. If no main roll was observed, diagnostics increment `noNaturalRoll`, and a pending forced roll remains queued for the next eligible exact-firearm attack that actually reaches `set_Roll`.

## Diagnostics

The Unity Mod Manager panel reports:

```text
eligible
naturalRolls
ordinary
misfires
forcedApplied
duplicateAssignments
duplicateEvaluations
noNaturalRoll
faults
pendingForcedRoll
last
```

The deterministic controls queue natural 1, 2, 3, or 20 and provide cancellation. The queue is process-local, single-slot, and replacement-based. It is not serialized and is not a global dice patch.

## Sprint 24 composition

The natural-roll decision remains pure:

```text
finalSuccess = nativeSuccess && naturalD20 > misfireValue
```

Version 0.0.24 consumes that immutable decision only after the exact item has committed its loaded-to-empty discharge. A detected misfire then applies Normal → Broken or Broken → Wrecked through the accepted item-token repository. See `FIREARM-MISFIRE-CONDITION-TRANSITIONS.md`.

Explosion, area or splash damage, gameplay repair, automatic reloads, Rapid Reload, scatter behavior, additional firearms, and Gunslinger class mechanics remain out of scope.
