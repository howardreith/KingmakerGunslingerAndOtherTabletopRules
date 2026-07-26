# Sprint 28 report — player-facing same-item overhaul

## Result

**READY FOR KINGMAKER — Sprint 28 player-facing overhaul smoke test.**

Version: `0.0.28-s28-player-facing-overhaul`

Runtime acceptance remains pending for the exact standalone package. Sprint 29 remains blocked until the complete 0.0.28 smoke-test gate passes.

## Entry decision

Sprint 27 was accepted from the supplied Kingmaker evidence. The exact Wrecked Test Musket remained present, two blueprint-identical muskets retained independent state, the development-only same-item overhaul preserved repository identity and runtime reference while advancing one revision, the second item remained unchanged, the item could be wrecked again, the destructive cleanup displayed a separate warning/confirmation flow, and saving produced no reported state fault.

## Implemented vertical slice

Sprint 28 adds the first player-facing recovery delivery:

```text
one exact equipped empty/Wrecked Test Musket
+ one Firearm Repair Kit
+ completed full-round Overhaul Test Musket ability
→ the same exact item becomes empty/Broken
```

The implementation includes:

- one stable stackable Firearm Repair Kit blueprint;
- one stable full-round personal extraordinary Overhaul Test Musket ability;
- Firearm Proficiency granting both Reload and Overhaul;
- fail-closed exact equipped-item selection;
- Wrecked-only availability and explicit missing-kit rejection;
- delivery-time mutation so pre-delivery interruption consumes nothing;
- a verified cross-resource transaction consuming exactly one kit;
- best-effort exact state and inventory rollback after mutation-time failures;
- exact repository identity, runtime reference, state, and revision evidence;
- process-local readiness, success, rejection, and fault diagnostics; and
- development controls for deterministic repair-kit setup and immediate transaction inspection.

Overhaul creates no ammunition, consumes no Black Powder Charge or Lead Ball, does not remove or replace the weapon, and stops at Broken. Ordinary Broken-to-Normal repair remains separate.

The item-owned inert `BlueprintWeaponEnchantment` token remains authoritative. The rejected `ItemEntityWeapon.UniqueId` vault was not revived.

## Qualification

- Exact Kingmaker 2.1.7b private-reference Release compile: PASS.
- .NET Framework 4.7 / C# 7.3 / AnyCPU: PASS.
- Warnings as errors: PASS.
- Same output path compile runs: 2.
- DLL comparison: byte-identical.
- PDB comparison: byte-identical.
- 569 tests × 3 runs, 0 failures.
- Repeated test output: byte-identical.
- Strict standalone UMM package: 8 entries.
- Packaged binaries: exactly one project-owned `KingmakerGunslinger.dll`.
- Private Kingmaker, Unity, UMM, Harmony, Newtonsoft, compiler, and framework binaries redistributed: none.

Authoritative hashes:

```text
KingmakerGunslinger.dll
9b589fddfe931092d0aff298c1648a71df03e27d9aa24a8f79b24a4f2993bd0b

KingmakerGunslinger.pdb
c9ac0ea9fb381af5b3acb8ae31afe8e5195141e40eeb0a456f8a09797bfca91c

Repeated test output
cb7a23830a7c7d2240020b72cc087adba7737298e708487835c424f94757501d

Standalone UMM ZIP
8f1907c53085d4dc2592d21120d0a05eaef0745d295cf35fd65634098f2d5022
```

## Runtime gate

The detailed guide is `SMOKE-TEST-GUIDE-0.0.28.md`. The blocking live proofs are:

1. Reload and Overhaul abilities are granted.
2. Missing-kit, Normal, Broken, and ambiguous-target requests fail without mutation.
3. Interrupting the full-round command before delivery consumes nothing and leaves Wrecked state unchanged.
4. Completed delivery consumes exactly one kit and changes only the exact item to empty/Broken.
5. Repository identity and runtime reference are unchanged; revision increases exactly once.
6. Powder, Lead Balls, weapon count, and a second Test Musket remain unchanged.
7. The exact empty/Broken state survives quicksave and full save/exit/restart/load.
8. All relevant fault, conflict, and duplicate-application counters remain zero.

## Delivery cadence decision

Sprints 18–28 retired unsafe runtime unknowns one at a time. The core contracts are now sufficiently mature to stop treating every small behavior as its own sprint. Sprints 29–38 are mapped as larger player-visible vertical slices in `planning/ROADMAP-SPRINTS-29-38.md`, with the risk-based acceleration policy in `planning/ACCELERATION-PLAN.md`.
