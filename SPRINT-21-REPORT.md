# Sprint 21 report — full-round Test Musket reload

## Result

Sprint 21 is complete as an exact-reference compiled Unity Mod Manager smoke-test package.

It connects the Sprint 20 ammunition stacks to the runtime-proven item-owned firearm-state carrier without changing either persistence representation or the Test Musket's stable blueprint IDs.

## Delivered

- Activated `KMG.Test.ReloadAbility` (`19e24b74331f437282077ce58e739d0f`).
- Registered eleven custom blueprints transactionally.
- Added a localized, personal, extraordinary, full-round `Reload Test Musket` ability.
- Granted/restored it through Firearm Proficiency.
- Resolved exactly one equipped Test Musket by exact item blueprint identity.
- Added read-only availability evaluation for state and inventory.
- Added a cross-resource transaction that consumes one Black Powder Charge and one Lead Ball and writes one loaded round to that exact Test Musket.
- Added exact post-write verification and separate firearm/inventory rollback diagnostics.
- Added UMM readiness, runtime-counter, and immediate-transaction controls.
- Added 21 reload tests, bringing the exact .NET Framework 4.7 suite to 419 cases.

## Build evidence

The final mod is compiled against the user-exported Kingmaker 2.1.7b, Unity Mod Manager 0.32.4, Harmony 1.2, Unity, and Newtonsoft reference assemblies. Compilation targets .NET Framework 4.7 and C# 7.3 with warnings treated as errors.

The dependency-free suite is compiled against the official .NET Framework 4.7 reference surface and executed three times. All 419 tests pass in each run and the output is byte-identical.

## Runtime qualification required

The exact-reference compile proves API compatibility, not in-game action scheduling. The smoke test must still prove:

- ability restoration on an upgraded save;
- action-bar/ability visibility;
- full-round timing;
- delivery-time resource mutation;
- cancellation safety;
- save/restart persistence of the loaded state and remaining ammunition.

## Out of scope

Firing still behaves like a Heavy Crossbow attack and does not require or consume the loaded round. Empty-fire prevention and shot consumption are Sprint 22.
