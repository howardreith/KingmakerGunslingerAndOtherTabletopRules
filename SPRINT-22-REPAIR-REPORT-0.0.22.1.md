# Sprint 22 repair report — 0.0.22.1 attack-hook binding and weapon-only reconciliation

## Outcome

Version 0.0.22 did **not** satisfy the Sprint 23 entry criteria. Quicksave no longer removed the loaded-state token, but an ordinary Test Musket attack left the exact item loaded, the reload ability stayed unavailable, and the attack-enforcement diagnostics remained at zero observations. The same evidence showed repeated state-token reconciliation faults on `ItemEntityShield`.

The project therefore remains on Sprint 22. Version 0.0.22.1 is a bounded repair smoke-test candidate; no misfire implementation has been added.

## Runtime result entering the repair

The supplied 0.0.22 evidence established:

- Reload still completed and wrote one loaded round to the exact Test Musket.
- Save/quicksave no longer removed the loaded token.
- Kingmaker completed an ordinary attack through its native weapon pipeline.
- The firearm stayed at `rounds=1` after that attack.
- `Firearm attack enforcement` remained `observed=0; fired=0; emptyRejected=0; wreckedRejected=0`.
- `State-token native reconciliation` reached `faults=10`, with the last fault reporting that `ItemEntityShield` exposed no readable enumerable enchantment collection.

The complete criterion-by-criterion assessment and original screenshots are under `evidence/sprint22-repair/`.

## Root cause 1 — wrong rule-event method contract

The exact private Kingmaker 2.1.7b assembly declares these methods:

```text
System.Void Kingmaker.RuleSystem.Rules.RuleAttackRoll.OnTrigger(
    Kingmaker.RuleSystem.RulebookEventContext context)

System.Void Kingmaker.RuleSystem.Rules.RuleAttackWithWeapon.OnTrigger(
    Kingmaker.RuleSystem.RulebookEventContext context)

System.Void Kingmaker.RuleSystem.Rules.RuleCalculateAC.OnTrigger(
    Kingmaker.RuleSystem.RulebookEventContext context)
```

The 0.0.22 resolver required a zero-argument `OnTrigger`. It therefore found zero candidates and instructed Harmony to skip all three optional patches. This directly explains why the loaded-round callback was never observed and also means the intended touch-AC and combat-trace hooks were not attached at those targets.

The repair accepts exactly one method only when all of these conditions hold:

- declared on the intended type;
- instance method;
- non-generic;
- name exactly `OnTrigger`;
- return type exactly `System.Void`; and
- exactly one parameter whose type is exactly `Kingmaker.RuleSystem.RulebookEventContext`.

It does not fall back to a plausible overload.

## Root cause 2 — base patch inspected non-weapons

`Kingmaker.Items.ItemEntity.ApplyEnchantments()` is the intended exact zero-argument native reconciliation target. Because it is a base method, however, Harmony invokes the prefix for non-weapon item subclasses as well. The 0.0.22 prefix unconditionally attempted to enumerate firearm-state tokens, including on `ItemEntityShield`, which has no compatible weapon-enchantment collection for that adapter.

The repair casts the runtime instance to `ItemEntityWeapon` before token inspection. Non-weapons receive an empty Harmony invocation state, native `ApplyEnchantments()` runs unchanged, and the postfix exits without firearm reflection. Weapon reconciliation behavior is otherwise unchanged.

## Files changed for runtime behavior

- `src/KingmakerGunslinger/Diagnostics/RuleEventPatchContract.cs`
- `src/KingmakerGunslinger/Diagnostics/RuleEventPatchTarget.cs`
- `src/KingmakerGunslinger/Firearms/FirearmStateTokenReconciliationPatch.cs`

The corresponding project files, dependency-free regression harness, version metadata, and runtime-contract inspection script were updated to keep the repaired contract executable and prevent the zero-argument assumption from being reintroduced.

## Scope deliberately preserved

The repair does not change:

- the exact `FirearmDefinitionComponent` marker requirement;
- item-owned inert `BlueprintWeaponEnchantment` state tokens;
- the accepted absence-means-Empty/Normal encoding;
- atomic Black Powder Charge plus Lead Ball consumption during reload;
- the full-round reload ability;
- attack-time inventory isolation;
- loaded/empty/Broken/Wrecked discharge decisions;
- duplicate event-instance protection; or
- native Heavy Crossbow exclusion.

The rejected `ItemEntityWeapon.UniqueId` vault approach remains rejected.

## Verification

Final qualification for the packaged candidate:

```text
Source invariant validation:             PASS
Exact private-reference Release compile: PASS
Warnings as errors:                      enabled
Deterministic compile comparison:         PASS — two same-output-path DLLs byte-identical
Declared/executed tests:                  455 x 3
Test failures:                            0
Repeated test output:                     byte-identical
Standalone UMM package validation:        PASS
Private compiler references redistributed: no
```

Authoritative hashes:

```text
KingmakerGunslinger.dll
  36f46686856d2f0b04bf91ddd9665b47d9c6676de96af428bd0d697ec32fa463

KingmakerGunslinger-0.0.22.1-attack-hook-repair-smoke-test.zip
  83423ad78a5e62dd4e329c07c1f5ddb20b9bd49567690e5bab6f9495e7263541

455-test repeated stdout
  5e004b6f22411a12e4d4706b4b7c21822e43ffbfb3a47b1d57310429339d9148
```

The standalone UMM ZIP exists and passed the eight-entry, one-binary allowlist, so the artifact is **READY FOR KINGMAKER — SPRINT 22 REPAIR SMOKE TEST**. This label means compile/package readiness only; it does not claim live runtime acceptance. Full machine-readable qualification is in `BUILD-QUALIFICATION.json`, `VALIDATION-RESULTS-S22-REPAIR.txt`, and `evidence/sprint22-repair/`.

## Runtime acceptance still required

Version 0.0.22.1 is not proof that the repair works in Kingmaker. The next live run must establish at minimum:

1. loaded state survives quicksave with reconciliation `conflicts=0; faults=0`;
2. one loaded Test Musket attack increments attack observation and fired counters and changes the exact item to `rounds=0`;
3. reload becomes available again without consuming additional inventory ammunition;
4. a second empty attack increments `emptyRejected` and is forced to miss;
5. the `ItemEntityShield` reconciliation fault does not recur; and
6. an actual native Heavy Crossbow remains outside firearm enforcement.

The full bounded procedure is `SMOKE-TEST-GUIDE-0.0.22.1.md`. Sprint 23 remains blocked until all criteria in `planning/SPRINT-23-ENTRY-CRITERIA.md` are satisfied without KMG or Harmony faults.
