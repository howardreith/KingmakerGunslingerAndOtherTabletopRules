# ADR-0015: Read-only marker-scoped firearm combat tracing

- **Status:** Accepted for Sprint 8; runtime signature corrected by ADR-0028; callback-signature detail superseded by ADR-0028
- **Date:** 2026-07-12

> **0.0.22.1 correction:** Kingmaker 2.1.7b declares these callbacks as `void OnTrigger(RulebookEventContext)`. The original zero-argument assumption caused the patches to be skipped. ADR-0028 supersedes only that target-signature detail; the marker-scoped, read-only tracing design remains accepted.

## Context

Touch-AC and misfire mechanics depend on the exact order and contents of Kingmaker rule events. Implementing them from remembered APIs would risk modifying the wrong AC calculation, using an attack total as a natural d20, or applying firearm behavior to all Heavy Crossbows.

## Decision

Install disabled-by-default prefix/postfix observers on `RuleAttackWithWeapon`, `RuleAttackRoll`, and `RuleCalculateAC`. The original Sprint 8 text assumed zero-argument callbacks; exact Kingmaker 2.1.7b inspection later established `void OnTrigger(RulebookEventContext)`, now enforced by ADR-0028.

A root trace begins only when the concrete weapon's exact `BlueprintWeaponType` has exactly one `FirearmDefinitionComponent`. Nested callbacks join by event identity and callback nesting. The runtime adapter copies values immediately into immutable diagnostic snapshots and retains no Kingmaker or Unity objects.

Dynamic target resolution uses Harmony's `Prepare()` and `TargetMethod()` convention. Missing or ambiguous targets skip only the optional observer. Runtime contract inspection remains the build/test gate.

## Rejected alternatives

### Patch AC immediately

Rejected because the event order, final AC member, and nested callback behavior have not been observed in the target game installation.

### Identify firearms by `WeaponCategory.HeavyCrossbow`

Rejected because the category is an engine/animation adapter and would misclassify native Heavy Crossbows.

### Subscribe globally and filter after logging

Rejected because it would create noisy logs and unnecessary work for every non-firearm attack.

### Store event objects until the attack completes

Rejected because rule events and unit/item objects have engine-owned lifetimes. Integer identities and copied values are sufficient for correlation.

### Use only a postfix

Rejected because before/after values are needed to determine which event performs the decisive mutation and to distinguish input from resolved output.

## Consequences

- Sprint 8 changes no gameplay.
- A real trace is required before Sprint 9's AC mutation is considered runtime-proven.
- Some fields may be `<unavailable>` and require an adapter update.
- Verbose tracing can produce several log lines per firearm attack and therefore remains opt-in.
