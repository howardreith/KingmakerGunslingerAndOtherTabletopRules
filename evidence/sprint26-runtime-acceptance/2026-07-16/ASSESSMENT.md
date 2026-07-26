# Sprint 26 runtime acceptance assessment — 2026-07-16

Package evaluated: `0.0.26-s26-misfire-burst`

## Decision

Sprint 26 is accepted for Sprint 27 entry.

The supplied Kingmaker evidence proves the bounded native burst behavior:

- the first Normal-to-Broken misfire did not apply a burst;
- an empty Broken Test Musket reloaded while remaining Broken;
- the second Broken-to-Wrecked misfire queried the configured five-foot native burst once;
- the native query produced two candidates and the deterministic plan applied exactly two unique targets;
- the nearby Assassin and exact wielder each received one independent native Reflex DC 12 save and one damage event;
- the exact wielder was included once;
- the exact firing item ended empty/Wrecked;
- query, target, attack, misfire, reload, AC, trace, and token diagnostics showed zero relevant faults;
- duplicate native query observation was deduplicated rather than delivered twice; and
- save behavior remained healthy.

The diagnostic evidence includes:

```text
queries=1
queryCandidates=2
plannedTargets=2
targetAttempts=2
targetApplied=2
targetRejected=0
targetDuplicates=1
targetFaults=0
faults=0
```

## Item-isolation clarification

The post-burst inventory screenshot still showed two Test Musket icons. Later screenshots showed no visible firearms and a state-carrier removal count increase. Wrecking does not remove an item or increment the repository removal count; the evidence is consistent with the destructive development cleanup control being invoked after the burst, not with the burst mutating or deleting both blueprint-identical firearms.

The user subsequently confirmed that the item-isolation concern was resolved and explicitly approved Sprint 27. The clean two-item post-burst retest was accepted by user observation rather than preserved as an additional screenshot. Sprint 27 therefore treats the gate as passed while also replacing the one-click destructive cleanup control with a two-step arm/confirm flow.

## Carry-forward boundaries

- Item-owned inert `BlueprintWeaponEnchantment` tokens remain authoritative.
- The rejected `ItemEntityWeapon.UniqueId` vault remains prohibited.
- Process-local diagnostic counters are not persistence state.
- Automatic firearm destruction and player-facing repair remain unimplemented at Sprint 27 entry.
