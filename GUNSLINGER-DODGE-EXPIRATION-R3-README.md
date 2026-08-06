# Gunslinger's Dodge expiration R3

R2 proved that the native ability action successfully applies the custom buff and
its +2 Dodge modifier, but the live game's `BuffCollection` does not remove this
particular dynamically registered buff when its displayed deadline elapses.

R3 keeps the native `ContextActionApplyBuff` path and adds a narrowly scoped
Harmony postfix on `BuffCollection.Tick`. The postfix does nothing unless that
collection contains the exact Gunslinger's Dodge blueprint. Once the buff's
already-established `EndTime` has elapsed, it calls `RemoveFact` so the normal
`OnTurnOff` path removes the AC modifier and condition icon.

The in-game UMM panel must say:

```text
Kingmaker Gunslinger - 0.0.67 DODGE-EXPIRATION-R3
```

It also reports:

```text
Dodge expiration guard: observations=...; expiredRemovals=...; faults=...; lastTimeLeftMs=...
```

After one successful use and expiration, `expiredRemovals` should be at least 1,
`faults` should remain 0, the condition should be gone, and AC should be back to
its starting value.
