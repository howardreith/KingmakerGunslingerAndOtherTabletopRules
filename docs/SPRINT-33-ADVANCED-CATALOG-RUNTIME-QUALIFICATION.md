# Sprint 33 advanced catalog runtime qualification

## Qualified build

- Commit: `41f299a`
- Version: `0.0.33`
- Package SHA-256: `6a8386e782f47726c38be60cda52e5e9b335d943a3650426cf6263c5deb51cf2`
- DLL SHA-256: `ba0f28e9197e1fb6949de9e829a3ccd60b65d865cea2962b6a06528ee87b4a64`

## Guarded evidence

Exact-assembly `mod-load-smoke` passed:

- `20260801T1433034839705Z-56c2363396894593961542057943f189`

The expanded `production-firearm-catalog` scenario then passed twice from
independent fresh Steam App ID 640820 processes against the exact named
`KMG_AUTOMATION_WORKING` save:

- `20260801T1434411929092Z-b69f2b13cc1f4a03945624f83ff3c5b9`
- `20260801T1436113008241Z-3c3f36a8807e4ed3869826afd13a5543`

Both feature runs proved five exact production entries and ten distinct custom
item/type blueprints, including Advanced Rifle and six-chamber Advanced
Revolver stats, marker isolation from native Heavy Crossbow, and continued
fail-closed Blunderbuss availability. Both correlated the same working-save
fingerprint and observed no save-writing API.

This qualifies advanced blueprint registration and catalog mechanics. It does
not yet qualify live batch reload, repeated revolver discharge, or save/restart
round-count behavior; those require the dedicated capacity scenario.
