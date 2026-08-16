# Urban Barbarian runtime matrix

Status: **INVENTORY OBSERVER SOURCE QUALIFIED — guarded profile runs pending**.

## Immutable identity fields

Every run must record and agree on:

- source commit and branch;
- package version and SHA-256;
- built and installed DLL SHA-256 and DLL MVID;
- package-manifest path/hash and deployment-manifest path/hash;
- Kingmaker game build;
- feature-module settings bytes/hash;
- CotW presence, version, DLL SHA-256/MVID, and settings bytes/hash when relevant;
- exact disposable save or request-local fixture identity; and
- runtime-result, runtime-evidence, and orchestration hashes.

Build, test, package, validate, back up, and deploy exactly once for the final
immutable candidate commit. Every scenario then reuses the exact installed
artifact. Settings restoration is byte-for-byte and mandatory on success,
failure, timeout, or interruption.

## Architecture inventory gate

| Profile | Scenario | Required result |
| --- | --- | --- |
| CotW present, current settings | `observe-urban-barbarian-rage-inventory` | Exact finalized Barbarian/Rage graph, component semantics, CotW marker and representative added powers |
| CotW absent | `observe-urban-barbarian-rage-inventory` through isolated compatibility transaction | Exact native-only graph; Urban core not yet required for research checkpoint |

These read-only, save-free launches precede production Rage implementation.

## Focused Urban scenarios

The final scenario set must consolidate compatible assertions into the smallest
trustworthy launch family while covering:

1. Publication and presentation: ON exactly once, OFF absent for new selection,
   native/CotW archetype order preserved, no duplicate identity, exact level-1
   replacement rows, proficiency, class skills, selector readability/state.
2. Ordinary Controlled Rage: three full +4 choices and representative +2/+2,
   exact stats, no native benefit leakage, allowed skills, preserved lifecycle.
3. Greater Rage: full +6, representative +4/+2, +2/+2/+2, current tier only.
4. Mighty Rage: full +8, representative +6/+2, +4/+4, +4/+2/+2, current tier only.
5. Constitution/HP: full health, damage, low HP, end, repeated toggle, Tireless,
   inactive and safely supported active save/load, level-up, no exploit.
6. Crowd Control: zero/one/two/three, movement in/out, death, hostility change,
   large unit, reach independence, melee/ranged attack, exact attack/dodge type.
7. Rage integration: passive and activated native rage powers, representative
   Rage feat/item, fatigue/Tireless, safe forced end/unconsciousness, supported
   CotW marker/power, no base-Barbarian regression.
8. Persistence: tier selection, inactive/active state where safe, module-OFF
   existing owner, restart idempotence, no duplicate registration.
9. Compatibility profiles: CotW absent, supported normal, supported balance;
   unknown/ambiguous behavior is primarily a deterministic fast-test simulation.

## Authoritative eight-module boundary

The generic catalog derives exactly `2N + 2 = 18` states for `N = 8`:

- all eight ON;
- all eight OFF;
- each of Gunslinger, Acadamae Graduate, Shield Other, Expanded Summoning,
  Elven Branched Spears, Eastern Weapons, Brown-Fur Transmuter, and Urban
  Barbarian ON alone; and
- each of those eight OFF while all seven others are ON.

There is no numeric `Boundary16` or `Boundary18` mode. The legacy
`Boundary14` compatibility alias may remain deprecated, but the authoritative
controller and evidence use the active catalog and generic `-Boundary` logic.

All 256 configurations belong in dependency-free domain/source tests only.
CotW profiles are focused scenarios and are not multiplied across the boundary.

## Human boundary

After focused profiles, persistence, and all 18 boundary states pass on the
same artifact, install that unchanged candidate and create
`URBAN-BARBARIAN-ACCEPTANCE-CHECKLIST.md`. Stop with it installed. Human visual
and play judgment covers name/description/icon/progression, skills/proficiency,
Crowd Control tooltip and visible behavior, selector legibility and selected
state, legal allocations, Rage counter, leakage absence, Constitution/HP,
native/CotW rage powers, UMM status, ON/OFF owner behavior, and duplication.
