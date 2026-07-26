# 0.0.24 Sprint 24 runtime failure assessment

Date: 2026-07-16

## Supplied result

The user forced a misfire from a loaded/Normal Test Musket. Version 0.0.24 correctly consumed the round and committed the first condition transition:

```text
naturalD20=1
misfired=True
conditionTransition=NormalToBroken
conditionBefore=Normal
conditionAfter=Broken
stateAfter=[rounds=0; ammunition=<none>; condition=Broken]
normalToBroken=1
faults=0
```

The resulting empty/Broken Test Musket could not activate Reload Test Musket. This prevented the required loaded/Broken setup and therefore blocked the `BrokenToWrecked` runtime criterion.

## Gate decision

Sprint 24 does not pass. Sprint 25 remains blocked. The project stays on a bounded Sprint 24 repair branch.

## Root cause

The pure `FirearmStateMachine.Load` correctly permits Broken firearms and rejects only Wrecked firearms. Two older Sprint 21 restrictions remained in the runtime reload path:

1. `ReloadTestMusketRuntime.Evaluate` rejected `FirearmCondition.Broken` before exposing the ability as available.
2. `FirearmReloadTransactionService.GetRejection` independently returned `FirearmReloadStatus.Broken` before mutation.

`FirearmReloadResult` also validated successful reloads as Normal-only, so simply removing the availability check would have caused the transaction result to fail after mutation.

## Bounded repair

Version `0.0.24.1-s24-broken-reload-repair`:

- permits empty/Normal and empty/Broken reload;
- preserves Normal or Broken condition in the successful loaded state;
- consumes exactly one Black Powder Charge and one Lead Ball;
- retains Wrecked rejection;
- retains exact-item state-token writes and rollback; and
- adds no repair, Quick Clear, explosion, damage, Rapid Reload, or new content.
