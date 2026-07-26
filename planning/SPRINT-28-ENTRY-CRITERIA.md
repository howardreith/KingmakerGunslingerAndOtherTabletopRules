# Sprint 28 entry criteria — player-facing firearm recovery delivery

Sprint 28 may begin only after the exact 0.0.27 standalone package proves in Kingmaker that:

- a second-misfire explosion still leaves the exact firing item present, empty, and Wrecked rather than automatically deleting or replacing it;
- two blueprint-identical Test Muskets remain distinguishable by exact runtime reference and repository identity;
- the development-only overhaul accepts only an exact equipped Wrecked firearm;
- overhaul changes only that item from empty/Wrecked to empty/Broken;
- repository identity and runtime reference hash are unchanged;
- repository revision increases exactly once;
- no Black Powder Charge, Lead Ball, weapon item, or other inventory resource is created or consumed;
- the second Test Musket remains unchanged;
- arming and cancelling the destructive cleanup control removes nothing;
- the overhauled empty/Broken state survives quicksave and full save/exit/restart/load;
- ordinary Broken reload remains available and preserves Broken condition; and
- repository, token reconciliation, attack, misfire, burst, reload, AC, bootstrap, Harmony, and lifecycle-probe faults remain zero.

## Bounded Sprint 28 scope after a pass

Sprint 28 may choose and implement one minimal player-facing recovery delivery for the already-qualified same-item state path.

The sprint must:

- preserve the exact item and item-owned token;
- keep Wrecked-to-Broken overhaul distinct from Broken-to-Normal ordinary repair;
- define explicit availability, action timing, resource/cost, and failure behavior;
- target exactly one selected or equipped firearm and fail closed on ambiguity;
- use no automatic item replacement or `ItemEntity.Dispose` shortcut;
- add deterministic diagnostics and dependency-free policy tests; and
- remain compatible with Kingmaker 2.1.7b, UMM 0.32.4, Harmony 1.2.0.1, .NET Framework 4.7, and C# 7.3.

If a safe player-facing selection, cost, or action-delivery contract cannot be established unambiguously, Sprint 28 must remain research/documentation only.

Sprint 28 must not add Quick Clear, Gunslinger class progression, grit, deeds, Rapid Reload, scatter triple damage, magical firearms, additional firearm blueprints, custom assets, vendors, crafting systems, or enemy firearm AI.
