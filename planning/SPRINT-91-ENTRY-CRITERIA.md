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

Exact source commit `8d2e5b6` passed guarded Steam mod load as
`20260802T1412373661267Z-mod-load-smoke`. The rebuilt package/DLL hashes were
`7c416c6465308c3c326b7f53938b131f640fa43dc908271a4a3ba0ee92054588` and
`6ac5c28106a57c322349285c001faf59a7ce9798e78cdbe22a190338bbccaadd`.
