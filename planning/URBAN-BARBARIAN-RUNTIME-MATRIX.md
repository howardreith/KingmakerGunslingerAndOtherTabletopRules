# Urban Barbarian runtime matrix

Status: **ARCHITECTURE INVENTORY 2/2 PASS; final immutable gameplay,
persistence, compatibility, and 18-state boundary execution pending**.

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
| CotW present, balance fixes (`1.14.4c-2.1`) | `observe-urban-barbarian-rage-inventory` | PASS; run `20260816T1328091838409Z...`; 952 records; CotW DLL SHA-256 `4EBF8E...15`; MVID `8caab254-aacf-4811-8093-44b9184e6e53`; settings SHA-256 `24CC3F...F6E8` |
| CotW absent | Same scenario through transaction `urban-rage-absent-20260816T133054Z-14e8246e` | PASS; run `20260816T1331159196672Z...`; 130 records; exact restoration verified |

These read-only, save-free launches used research candidate commit
`8edb2cb57206b359e8b67ee8eacbb4df6e98a67a`, package SHA-256
`ca6a49e65d744c5291cd640a423d30063d351fb7410e81964cee1b6a46a755e1`,
DLL SHA-256 `6e5425f6a243ae9b1258804b8cee5d5de9519e2ce3834147cb87fe4c13d2baaa`,
and MVID `0eb8cbfb-987c-4ff2-a123-f56caa9f03c3`. They authorize production
implementation but are not final-candidate gameplay qualification.

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

The final fixture allocation is:

- `disposable-urban-barbarian-focused`: one save-free live-unit launch for
  progression, allocation families, leakage, HP cycles, native lifecycle, and
  Crowd Control rule events;
- `observe-urban-barbarian-rage-inventory`: one read-only launch per required
  CotW profile for final graph, optional-status, marker, and rage-power evidence;
- `working-save-urban-barbarian-prepare` followed by
  `working-save-urban-barbarian-off-verify-cleanup`: the exact guarded active-
  Rage save/load and existing-owner module-OFF transaction; and
- generic `Invoke-FeatureModuleRuntimeMatrix.ps1 -Boundary`: the authoritative
  18 publication boundary launches, with no CotW Cartesian multiplication.

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
