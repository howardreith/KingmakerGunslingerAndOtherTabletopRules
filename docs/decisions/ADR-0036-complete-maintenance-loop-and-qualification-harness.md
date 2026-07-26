# ADR-0036: Complete the staged maintenance loop and add deterministic qualification automation

## Status

Accepted for Sprint 29 runtime qualification.

## Context

Sprint 28 proved a player-facing full-round Wrecked-to-Broken Overhaul that consumed one Firearm Repair Kit only on completed delivery and preserved the exact item. The firearm still required a separate ordinary Broken-to-Normal action before it could return to a fully functional Normal state.

Repeated runtime qualification also required long manual setup sequences. The core item, transaction, action-delivery, and persistence contracts were already proven, so retaining one-button-at-a-time regression setup was no longer efficient.

## Decision

Add a separate personal extraordinary full-round `Repair Test Musket` ability granted by Firearm Proficiency.

Repair accepts exactly one equipped empty/Broken Test Musket and one Firearm Repair Kit. Completed delivery consumes exactly one kit and changes the same exact item to empty/Normal. It does not replace the item, create ammunition, consume powder or Lead Balls, repair Wrecked directly, or load the firearm.

Retain the staged sequence:

```text
Wrecked --Overhaul + kit--> Broken --Repair + kit--> Normal --Reload + ammunition--> Loaded
```

Add a deterministic process-local maintenance qualification fixture and pure PASS/FAIL evaluator. The fixture records one exact target, one independent second item, resources, revisions, completion counters, faults, and duplicate counters. The evaluator recognizes fixture, Overhaul, Repair, and Reload checkpoints.

Also expose a one-command immediate diagnostic runner for fast transaction-level regression. Keep manual action-bar tests for full-round timing and interruption.

## Consequences

Positive consequences:

- the player can complete the full recovery loop without development-only condition mutation;
- Overhaul and ordinary Repair retain distinct costs and state boundaries;
- cancelled actions remain non-mutating because transactions start only at delivery;
- exact-item identity and second-item isolation remain explicit;
- cross-resource failure attempts to restore both state and inventory;
- regression setup and result review become substantially faster; and
- Sprint 30 can generalize a proven three-action maintenance model rather than invent it while adding new weapons.

Costs and limitations:

- actions remain Test-Musket-specific for one more sprint;
- the Repair Kit uses placeholder presentation and development distribution;
- Repair currently has no skill check or campaign-economy integration;
- the immediate runner bypasses action economy and cannot replace manual interruption tests; and
- the qualification baseline is process-local and intentionally invalid after restart.

## Rejected alternatives

### Let Overhaul repair directly to Normal

Rejected because it collapses two recovery stages, weakens future Gunsmithing and Quick Clear design space, and contradicts the runtime-qualified Wrecked-to-Broken boundary.

### Let Reload repair Broken firearms

Rejected because reload must preserve condition and consume only ammunition components.

### Consume the kit when the command is selected

Rejected because interrupted full-round actions would lose resources before delivery.

### Replace the firearm with a Normal copy

Rejected because it discards exact-item identity and complicates rollback and persistence.

### Make the automated runner the only acceptance test

Rejected because immediate diagnostic delivery cannot prove Kingmaker's real full-round command timing, interruption, action-bar presentation, or animation.
