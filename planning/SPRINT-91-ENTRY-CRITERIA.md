# Sprint 91 battered firearm deed use gates

Every deed that uses an equipped firearm must evaluate the shared effective
battered condition. Pistol-Whip, Lightning Reload, and the custom Dead Shot
discharge/misfire pipeline therefore consume `EffectiveCondition` while all
state transitions remain commits against the exact actual item.

Quick Clear is deliberately different: it is maintenance that removes an
actual misfire-origin Broken state. It continues to inspect and repair the
repository condition, and cannot erase an ownership-only effective overlay.

The focused source contract, complete domain/reflection suite, repository
validation, clean Release build, and strict package validation are required
before commit; guarded exact-commit mod load is the runtime gate.
