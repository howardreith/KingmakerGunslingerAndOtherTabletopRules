# Sprint 22 completion report

## Result

Sprint 22 is complete as an exact-reference compiled Kingmaker smoke-test milestone.

```text
Version:                    0.0.22-s22-loaded-round-enforcement
Target game:                Pathfinder: Kingmaker 2.1.7b
Unity Mod Manager:          0.32.4
Harmony:                    1.2.0.1
Target framework:           .NET Framework 4.7
Language version:           C# 7.3
Configuration:              Release / AnyCPU
Warnings treated as errors: yes
Exact-reference compile:    passed
Compiler stderr:            empty
Declared/executed tests:    446
Test runs:                  3
Failures:                   0
Repeated output:            byte-identical
Kingmaker status:           ready for loaded-round and quicksave smoke testing
```

The standalone package is:

```text
KingmakerGunslinger-0.0.22-loaded-round-smoke-test.zip
SHA-256: 5a6fd105e78c0a8850b6988c4d1ee0edb7b4f6460d61a30a14db17d43d0447cd
```

## Runtime evidence consumed

Sprint 21 was tested in Kingmaker and established that:

- `Reload Test Musket` appeared and executed as a full-round action;
- delivery consumed exactly one Black Powder Charge and one Lead Ball;
- the exact equipped Test Musket became loaded;
- reload became unavailable while the gun was loaded; and
- quicksave immediately made the firearm appear empty again.

The quicksave behavior was accepted as a blocking state-carrier defect rather than an ability-availability display issue.

## Quicksave root cause

Inspection of the exact Kingmaker 2.1.7b `ItemEntity.ApplyEnchantments()` method showed that native item reconciliation removes a dynamic enchantment when its `ParentContext` is null and its blueprint is not one of the item's built-in enchantments.

The Sprint 21 loaded-state token matched that condition. Quicksave or an associated equipment refresh invoked reconciliation, removed the token, and caused the firearm to decode as canonical empty/Normal state.

## Implemented durability repair

Sprint 22 now:

1. gives newly created state-token enchantments a `MechanicsContext` when the item exposes a current wielder or owner;
2. patches the exact zero-argument `ItemEntity.ApplyEnchantments()` method;
3. records only known firearm-state token IDs before native reconciliation;
4. lets Kingmaker run its native method normally;
5. verifies the token set afterward; and
6. restores only the exact, unambiguous case where one known token existed before and none existed afterward.

Duplicate, changed, foreign, or otherwise ambiguous token sets are reported as conflicts. They are not silently normalized.

## Loaded-round attack enforcement

Sprint 22 also closes the first basic firing loop for the Test Musket.

At the start of an exact marked firearm's `RuleAttackRoll`:

- Loaded / Normal consumes one round and allows the native attack to proceed.
- Loaded / Broken consumes one round, retains Broken, and allows the native attack to proceed.
- Empty / Normal is forced to miss.
- Empty / Broken is forced to miss.
- Wrecked is forced to miss.
- A firearm state read or write fault fails closed by forcing the marked firearm attack to miss.

Forced misses clear `AutoHit` before setting `AutoMiss`, because Kingmaker evaluates the auto-hit path first.

A weak reference-identity event gate prevents duplicate callbacks for the same `RuleAttackRoll` object from consuming more than one round.

Firing never consumes inventory powder or Lead Balls. Those components were already consumed by reload.

## Native-weapon isolation

The firing rule still identifies a firearm by exactly one `FirearmDefinitionComponent` on the concrete weapon type. The borrowed Heavy Crossbow category, model, icon, and name are insufficient.

A native Heavy Crossbow must therefore remain outside the firearm state and discharge systems.

## Blueprint ledger

Sprint 22 adds no blueprint IDs.

```text
Stable IDs: 12
Active:     11
Reserved:    1
Manifest SHA-256:
af56f2a35bd05e055e8feee1e996d37abcb20b76efb90762784f31ccfe933337
```

## Executed tests

Sprint 22 adds 27 pure tests:

- 13 discharge-decision/result cases;
- 4 reference-event-gate cases; and
- 10 native token-reconciliation cases.

The complete exact .NET Framework 4.7 suite ran three times:

```text
Completed 446 tests; failures=0.
Completed 446 tests; failures=0.
Completed 446 tests; failures=0.
```

All three stdout files have SHA-256:

```text
209f26caf959cc7a88015de5a98452a9d594a40962b982800b9a9e89cfdeb401
```

The test executable SHA-256 is:

```text
736f2b37cbeb1629442228c7c3564d7f2c964eac36ee6d806d00b316f00b31eb
```

## Exact Kingmaker compile

The full mod compiled against the private Kingmaker, Unity, Unity Mod Manager, Harmony, and Newtonsoft reference set supplied for Kingmaker 2.1.7b.

```text
Compiler exit code: 0
Compiler stderr:    empty
Mod DLL SHA-256:
25f8e1a25cf7871591f5dc3778b0732ebb5f03edec7b79c62775d05bf07b1660
```

No private reference assembly is included in the install ZIP.

## Runtime claims still pending

The source and binary have not yet proved in Kingmaker that:

- quicksave preserves or restores the loaded token;
- manual save leaves the currently equipped firearm loaded in memory;
- a loaded ordinary attack consumes exactly one round;
- a second empty ordinary attack is forced to miss;
- a loaded Broken firearm discharges and remains Broken;
- a Wrecked firearm is forced to miss;
- a native Heavy Crossbow is unaffected; or
- attack enforcement composes correctly with every other combat mod.

Those are the acceptance cases in `SMOKE-TEST-GUIDE-0.0.22.md`.

## Explicitly deferred

Sprint 22 does not add natural-roll misfires, automatic Broken/Wrecked transitions, explosions, firearm repair gameplay, iterative automatic reloads, Rapid Reload, custom firearm assets, vendors, crafting, or the Gunslinger class.
