# Risk register and bounded spikes

Scores use likelihood and impact from 1–5. Priority is their product.

| Risk | L | I | Priority | Retirement experiment | Gate |
|---|---:|---:|---:|---|---|
| Per-item state cannot be serialized safely | 4 | 5 | 20 | Two identical muskets retain different state through restart, transfer, stash, vendor, and respec paths | Sprints 12–14; still open |
| Engine item identity or custom UnitPart records do not survive reconstruction | 4 | 5 | 20 | Complete the Sprint 14 identity lifecycle and migration matrix on a compiled build | Sprint 14 gate |
| Identity records become orphaned or transfer to the wrong item | 3 | 5 | 15 | Measure save size, sale/repurchase, duplication, deletion, and identity reuse across repeated restarts | Sprint 14 gate |
| Current UMM/Harmony compatibility differs from Call of the Wild era | 3 | 5 | 15 | Compile/load a no-op Harmony12 mod on the installed UMM and capture assembly identities | Sprints 2–3; runtime open |
| Borrowed weapon category leaks crossbow mechanics | 4 | 4 | 16 | Enumerate category consumers and test feat/UI behavior | Sprints 5–7; runtime open |
| Full attacks queue shots before reload validation | 4 | 5 | 20 | Instrument standard, iterative, Rapid Shot, and Haste attacks | Sprints 23–24 |
| Inventory consumption duplicates or loses ammunition | 3 | 5 | 15 | Atomic reload tests under interruption, save/load, stack split, and transfer | Sprint 15 or later after persistence GO |
| Touch-AC modification bypasses defenses or uses wrong distance | 3 | 5 | 15 | Instrument AC pipeline at several ranges with cover and flat-footed context | Sprints 8–9; runtime open |
| Misfire processing occurs too late or more than once | 3 | 4 | 12 | Force natural rolls and trace event delivery | Sprints 16–18 |
| Custom class registration conflicts with other class mods | 3 | 4 | 12 | Integrity report and compatibility run with Call of the Wild and respec mods | Sprints 28 and 70 |
| Final storefront assemblies differ | 2 | 4 | 8 | Hash supported installations and centralize native references | Sprint 2 onward |
| Models or animations cannot be added cleanly | 4 | 2 | 8 | Optional model-loading spike with crossbow fallback | Sprint 69 |
| Rules text or assets are distributed under wrong terms | 2 | 5 | 10 | License manifest and release audit; no copied assets in core | Sprint 67 onward |
| Removing the mod corrupts saves containing custom types | 5 | 4 | 20 | Explicit uninstall warning, backup procedure, and migration diagnostics | Sprint 14 gate and Sprint 71 |
| Blueprint GUID collision or reassignment | 2 | 5 | 10 | Manifest validator, collision checks, and permanent reservations | Sprints 2–4 onward |

## Spike rules

A spike succeeds when it replaces uncertainty with a recorded engine fact, even if the desired approach fails.

Every diagnostic sprint must produce:

- the exact environment fingerprint;
- a minimal reproduction;
- relevant logs;
- the observed event order or serialized representation;
- the decision taken;
- a bounded follow-up task.

A spike must not quietly grow into an unrelated feature subsystem.

## Immediate critical path

```text
Foundation bootstrap
  -> verified native blueprint candidates
  -> Test Musket ordinary attack
  -> touch AC
  -> exact per-item runtime state
  -> durable save/load and migration gate
  -> ammunition transaction
  -> misfire and repair
  -> iterative attack/reload
  -> class content
```

The class remains downstream of the two highest-risk engine questions: item persistence and queued iterative attacks.
