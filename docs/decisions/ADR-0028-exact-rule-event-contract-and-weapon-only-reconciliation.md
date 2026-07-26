# ADR-0028: Bind exact rule-event callbacks and inspect reconciliation only on weapons

- **Status:** Accepted for the Sprint 22 repair
- **Date:** 2026-07-14

## Context

Version 0.0.22 preserved a loaded Test Musket through the supplied quicksave test, but an ordinary attack left the exact item loaded and `Firearm attack enforcement` remained at zero observations. The same runtime evidence showed repeated `MissingMemberException` faults while native token reconciliation inspected `Kingmaker.Items.ItemEntityShield`.

Exact inspection of the private Kingmaker 2.1.7b `Assembly-CSharp.dll` established:

- `RuleAttackWithWeapon.OnTrigger`, `RuleAttackRoll.OnTrigger`, and `RuleCalculateAC.OnTrigger` each declare `void OnTrigger(Kingmaker.RuleSystem.RulebookEventContext)`;
- version 0.0.22 searched for zero-argument methods and therefore selected no target; and
- `ItemEntity.ApplyEnchantments()` is a zero-argument base method invoked for non-weapon item subclasses as well as weapons.

The attack pipeline itself was not the failure. The Harmony target contract and the reconciliation boundary were wrong.

## Decision

Resolve each intended rule-event target only when the declaring type exposes exactly one method satisfying all of these conditions:

```text
name:          OnTrigger
instance:      yes
static:        no
generic:       no
return type:   System.Void
parameters:    exactly one
parameter 0:   Kingmaker.RuleSystem.RulebookEventContext
```

Missing or ambiguous contracts skip the affected patch and produce a structured warning rather than binding a plausible overload.

At the `ItemEntity.ApplyEnchantments()` prefix, cast `__instance` to `ItemEntityWeapon` before calling firearm-state reconciliation. Non-weapons receive an empty invocation state and proceed through native Kingmaker logic unchanged. The postfix exits immediately for that empty state.

Keep the existing item-owned inert `BlueprintWeaponEnchantment` state carrier, exact firearm marker, loaded/empty/Broken/Wrecked decisions, duplicate-event gate, and reload transaction unchanged.

## Rejected alternatives

### Revive the `ItemEntityWeapon.UniqueId` vault

Rejected. The installed Kingmaker runtime disproved the required member contract, and the handoff explicitly forbids returning to that design.

### Match any method named `OnTrigger`

Rejected because an overload or future runtime change could silently bind the wrong method and mutate combat incorrectly.

### Patch a different attack method because the native shot completed

Rejected because the diagnostics proved the intended callback never attached. The smallest evidence-driven repair is to bind the exact installed method, not replace Kingmaker's weapon pipeline.

### Catch and ignore non-weapon reflection faults

Rejected because non-weapons are outside the firearm-state domain. They should not enter token inspection at all.

## Consequences

- Loaded-round enforcement, touch-AC integration, and optional combat tracing can attach to the installed Kingmaker 2.1.7b rule-event methods.
- Shield and other non-weapon `ItemEntity` refreshes cannot invoke firearm-token reflection.
- The repair remains fail closed if the installed method contract changes or becomes ambiguous.
- Nine dependency-free reflection-contract tests prevent the zero-argument assumption from returning.
- The Windows runtime-contract inspector now checks the same one-argument signature.
- Sprint 23 remains blocked until the repaired candidate passes the full in-game acceptance matrix, including native Heavy Crossbow isolation and zero KMG/Harmony faults.
