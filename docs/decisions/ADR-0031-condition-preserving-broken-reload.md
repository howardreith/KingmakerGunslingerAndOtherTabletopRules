# ADR-0031: Permit condition-preserving reload of a Broken firearm

## Status

Accepted for `0.0.24.1-s24-broken-reload-repair`.

## Context

Sprint 24 introduced the bounded second-misfire transition:

```text
loaded / Broken -> discharge -> empty / Broken -> misfire damage -> empty / Wrecked
```

The 0.0.24 Kingmaker test proved the first Normal-to-Broken transition, then exposed that the player-facing reload ability remained unavailable while the Test Musket was empty/Broken. This prevented the required Broken-to-Wrecked runtime proof.

The canonical pure `FirearmStateMachine.Load` already permits `Broken` and rejects only `Wrecked`. The stale restriction existed independently in both `ReloadTestMusketRuntime.Evaluate` and `FirearmReloadTransactionService.GetRejection`. `FirearmReloadResult` also validated successful reloads as Normal-only.

## Decision

Align the runtime adapter and transaction with the canonical state machine:

- an empty Normal or Broken exact firearm may reload;
- the loaded result must preserve the exact pre-reload condition;
- one Black Powder Charge and one Lead Ball are consumed atomically;
- a loaded Normal or loaded Broken firearm remains `AlreadyLoaded`;
- Wrecked remains rejected before mutation; and
- no reload path may change Broken to Normal.

The legacy internal `FirearmReloadStatus.Broken` enum member is retained to avoid unnecessary binary churn, but the repaired transaction no longer emits it.

## Consequences

The Sprint 24 second-misfire test is now reachable through ordinary player-facing reload behavior. Reloading a Broken firearm is not a repair mechanic: it remains Broken and is still eligible to become Wrecked on the next detected misfire.

The item-owned inert `BlueprintWeaponEnchantment` state carrier remains authoritative. The rejected `ItemEntityWeapon.UniqueId` vault is not revived. Wrecked firearms remain unable to reload or fire successfully.
