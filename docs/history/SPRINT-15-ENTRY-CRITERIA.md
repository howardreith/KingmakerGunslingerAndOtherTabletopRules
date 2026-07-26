# Sprint 15 entry criteria and branch decision

## Gate

Sprint 15 becomes the powder-and-bullet inventory sprint only after a real compiled Sprint 14 UMM package produces a persistence **GO** decision.

Required evidence:

- successful installed-assembly runtime-contract report;
- 327 dependency-free tests passing;
- clean Debug and Release builds;
- generated UMM ZIP and SHA-256;
- complete Sprint 14 persistence lifecycle matrix;
- stable item identity across save/load, restart, transfer, stash, rest, area transition, and sale/repurchase;
- distinct identity after duplication;
- no identity/state transfer after deletion;
- successful Sprint 13 direct-reference migration;
- successful Sprint 12 token migration;
- conflict and malformed-data cases fail closed;
- no state or pricing effect on native Heavy Crossbows.

## GO branch — bounded Sprint 15 scope

After GO, Sprint 15 may implement:

- stackable Black Powder Charge blueprint;
- stackable Lead Ball blueprint;
- immutable ammunition definition and compatibility query;
- inventory lookup and atomic consume service;
- development controls to grant/remove ammunition;
- no reload ability yet;
- no firing consumption yet;
- no alchemical ammunition yet.

## NO-GO branch

If any Critical matrix row fails, Sprint 15 remains a persistence sprint. It must:

1. preserve all stable blueprint IDs;
2. preserve readable Sprint 12 token data;
3. preserve readable Sprint 13 direct-reference records;
4. preserve readable Sprint 14 identity records;
5. document the exact failed lifecycle transition;
6. select the next carrier only from observed Kingmaker runtime behavior;
7. repeat the full matrix before ammunition work.

## Explicit prohibitions

A NO-GO branch must not silently switch to:

- character buffs;
- weapon blueprint keys;
- display names;
- owners or equipment slots;
- inventory indices;
- runtime reference hashes;
- a generated item GUID without a proven item-serialization hook;
- a global sidecar save not transactionally coupled to Kingmaker saves.
