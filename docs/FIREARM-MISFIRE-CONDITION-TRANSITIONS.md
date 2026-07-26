# Firearm misfire condition transitions

## Sprint 24 boundary

Version 0.0.24 retains Sprint 23's exact main-roll detection and adds only the item-owned condition damage caused by a detected firearm misfire:

```text
Normal -> Broken
Broken -> Wrecked
```

The transition is evaluated after the exact firearm's loaded round has already been discharged. Both the pre-transition and post-transition states therefore remain empty:

```text
rounds=0
ammunition=<none>
```

Sprint 24 does not add explosion, splash, area, wielder-damage, repair, Quick Clear, or additional firearm behavior.

### 0.0.24.1 broken-reload repair

The first live 0.0.24 test proved `Normal -> Broken` but exposed a stale Sprint 21 restriction in the player-facing reload adapter and transaction. Although the canonical state machine already allowed Broken loading, the action rejected the empty/Broken item and made the second transition unreachable.

Version 0.0.24.1 aligns the reload path with the state machine:

```text
empty / Broken -> loaded / Broken
```

The operation consumes exactly one Black Powder Charge and one Lead Ball, preserves the exact item's Broken condition, and leaves Wrecked reload blocked. It adds no repair or condition reduction.

## Ordering

The runtime sequence is intentionally split into independent authoritative steps:

1. `FirearmDischargeRuntime` validates exactly one firearm marker on the concrete runtime weapon.
2. The item-owned repository commits `loaded -> empty` for that exact item.
3. Only a verified `Fired` result registers a short-lived `RuleAttackRoll` misfire context.
4. The exact private `RuleAttackRoll.set_Roll(RollEntry)` hook observes or applies the final natural d20.
5. The exact public `RuleAttackRoll.IsSuccessRoll(int)` hook classifies the roll.
6. A first detected misfire evaluation applies one bounded condition transition through the same exact runtime item and repository identity.
7. The final attack result remains a miss.

This ordering prevents condition damage from creating or preserving a loaded round and keeps attack-time inventory consumption at zero.

## Pure policy

`FirearmMisfireConditionService` accepts:

- one immutable `FirearmMisfireDecision`; and
- the exact item's verified empty post-discharge `FirearmState`.

It rejects loaded and Wrecked inputs because they cannot represent an eligible successfully discharged attack. For an ordinary roll it returns the same state. For a detected misfire it delegates to the existing canonical state-machine transition:

```text
FirearmStateMachine.ApplyMisfireDamage(postDischargeState)
```

The immutable `FirearmMisfireConditionDecision` verifies that:

- ordinary rolls have no state change;
- a misfire never has transition `None`;
- `NormalToBroken` has exactly Normal before and Broken after;
- `BrokenToWrecked` has exactly Broken before and Wrecked after; and
- both states are empty.

## Exact-item commit

The short-lived attack context retains the exact runtime weapon object that successfully discharged, the empty post-discharge state, and the repository identity observed at discharge.

A condition transition uses:

```text
FirearmRuntimeState.Service.Transition(exactItem, transition)
```

The transition callback rejects any intervening state change. After commit, the runtime verifies:

- the committed state equals the pure decision's expected post-state; and
- the repository identity still matches the discharged item.

No display name, blueprint category, equipment slot, owner, object hash, or inventory position is used as persistent identity. The runtime-rejected `ItemEntityWeapon.UniqueId` vault is not revived.

## At-most-once behavior

One `RuleAttackRoll` object receives one short-lived context. Atomic gates separately protect:

- main-roll assignment; and
- natural-roll evaluation.

Only the first evaluation may commit condition damage. A duplicate `IsSuccessRoll` callback still enforces the already-determined misfire miss but increments a duplicate diagnostic instead of repeating the state mutation.

## Persistence

Condition damage is stored by the existing item-owned inert `BlueprintWeaponEnchantment` token repository:

```text
empty / Normal   -> absence of a state token
empty / Broken   -> KMG broken-empty token
empty / Wrecked  -> KMG wrecked token
```

The existing weapon-only `ItemEntity.ApplyEnchantments()` reconciliation guard remains responsible for preserving an unambiguous item-owned token through native enchantment refresh. Version 0.0.24 adds no new save carrier.

## Diagnostics

The UMM panel retains the Sprint 23 counters and adds:

```text
normalToBroken
brokenToWrecked
```

The last natural-roll record includes:

```text
conditionTransition
conditionBefore
conditionAfter
stateBefore
stateAfter
```

A successful forced-1/2 misfire from Normal should therefore end with `normalToBroken +1`; a successful forced-1/2 misfire from Broken should end with `brokenToWrecked +1`. Faults, duplicate assignments, and duplicate evaluations must remain zero in the normal smoke path.

## Carried-forward gate

The user explicitly approved entering Sprint 24 before every formal Sprint 23 control was separately captured. The 0.0.24 smoke test therefore carries forward and rechecks:

- native Heavy Crossbow queue isolation;
- empty-firearm queue isolation;
- Wrecked-firearm queue isolation;
- `noNaturalRoll` queue preservation;
- ordinary 3/20 counter evidence;
- post-misfire quicksave and full save/exit/restart/load; and
- zero relevant runtime faults.
