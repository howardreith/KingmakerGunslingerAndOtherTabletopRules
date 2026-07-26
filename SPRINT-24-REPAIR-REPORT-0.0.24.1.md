# Sprint 24 repair report — 0.0.24.1 condition-preserving Broken reload

## Decision

The supplied 0.0.24 Kingmaker result failed the Sprint 25 entry gate. The forced natural-roll misfire correctly changed the exact Test Musket from loaded/Normal to empty/Broken, but the empty/Broken item could not reload. That made the intended Broken → Wrecked path unreachable through normal play.

Sprint 25 remains blocked. Version 0.0.24.1 is a bounded Sprint 24 repair only.

## Root cause

The canonical firearm state machine already permitted Broken firearms to load while retaining their condition. Three reload-layer checks still encoded Sprint 21's temporary Normal-only restriction:

- the player-facing reload-availability adapter rejected `condition=Broken`;
- the atomic reload transaction returned the `Broken` rejection status before attempting the canonical load; and
- the immutable success result accepted only an empty/Normal → loaded/Normal shape.

This was an internal policy mismatch, not a token-persistence or misfire-transition failure.

## Repair

Version `0.0.24.1-s24-broken-reload-repair`:

- permits an empty Broken Test Musket to reload;
- preserves `condition=Broken` in the loaded result;
- consumes exactly one Black Powder Charge and one Lead Ball after all preconditions pass;
- treats a loaded/Broken firearm as already loaded;
- retains Wrecked rejection before mutation;
- retains exact-item writes, post-write verification, and rollback; and
- adds no repair gameplay, Quick Clear, explosion, wielder damage, Rapid Reload, new firearm, or class behavior.

The item-owned inert `BlueprintWeaponEnchantment` token remains authoritative. The runtime-rejected `ItemEntityWeapon.UniqueId` vault remains unused.

## Tests added

The dependency-free suite now proves:

- an empty/Broken firearm reloads successfully and remains Broken;
- a loaded/Broken firearm is rejected as already loaded without consuming components;
- a successful Broken reload must preserve condition rather than silently repairing the item; and
- existing Wrecked rejection, atomic consumption, rollback, and misfire-condition tests continue to pass.

## Runtime evidence that prompted the repair

The original 0.0.24 screenshots and assessment are preserved under:

```text
evidence/sprint24-repair/runtime-failure-2026-07-16/
```

They show `conditionTransition=NormalToBroken`, `rounds=0`, `condition=Broken`, and zero natural-roll misfire faults immediately before the user-observed reload failure.

## Qualification

**READY FOR KINGMAKER — Sprint 24 Broken-reload repair smoke test**

The non-runtime qualification gate passed:

- exact Kingmaker 2.1.7b private-reference Release compilation;
- .NET Framework 4.7, C# 7.3, AnyCPU;
- warnings as errors and deterministic compiler mode;
- two same-output-path compiles with byte-identical DLL and PDB outputs;
- **503 tests × 3 runs, 0 failures**, with byte-identical output across all runs;
- strict standalone UMM package validation: 8 entries and exactly one project-owned binary; and
- no private Kingmaker, Unity, UMM, Harmony, or Newtonsoft assemblies redistributed.

Authoritative qualification hashes:

```text
KingmakerGunslinger.dll
674e732edd0a27727bb27eb950749b8f72428addd00e9c8996160decd88f11b4

KingmakerGunslinger.pdb
de034a38f6b3443f10e408ccf063d9ab7f6eec6806a316d1f1e99555eb623aa0

Repeated test output
cf1f9b44687a228abf972eb6196e66fb1bee1343f75e62015faaabe0bebe3bc3

Standalone UMM ZIP
5f3a9caf325339132dc1d03482af0b48a2292fce9dfa6a3548f4c5d6b307fa7f
```

## Runtime status

The exact 0.0.24.1 standalone package still requires the explicit smoke test in `SMOKE-TEST-GUIDE-0.0.24.1.md`. The decisive sequence is empty/Broken → reload → loaded/Broken → forced natural-roll misfire → empty/Wrecked.

Sprint 25 remains blocked until this package proves condition-preserving Broken reload, Broken → Wrecked, Wrecked reload/attack rejection, persistence, exact-item isolation, and zero relevant faults.
