# Version 0.0.22 runtime-result assessment

## Decision

**Sprint 23 entry: BLOCKED. Remain on the Sprint 22 repair branch.**

Version 0.0.22 fixed the original quicksave regression, but it did not enforce loaded-round consumption during attacks. The supplied screenshots also show a new reconciliation fault on non-weapon items. Natural-roll misfire work must not begin until the repaired attack hook and weapon-only reconciliation path pass in Kingmaker.

## Evidence supplied

The four original screenshots are preserved byte-for-byte under `runtime-screenshots/`.

| Screenshot | SHA-256 | Relevant observation |
|---|---|---|
| `01-empty-ready-to-reload.png` | `ebcb4ba75ec3c44ca53a86ecbb6fbec10464444f3af610b7415932b5376d7844` | Empty/Normal Test Musket is recognized and reload readiness is available. |
| `02-loaded-after-quicksave.png` | `ef23645bf52e3b272226c197a49a0f2f21acbe591ba5690e876b513b42c562d6` | Reload completed; the item reports `rounds=1`; the token was observed and preserved with zero faults at that point. |
| `03-native-heavy-crossbow-pipeline-attack.png` | `8e63e998216606b8005ac7ba13032d4cc1a06adea01e69b4b899326613aea529` | Kingmaker completed an ordinary Heavy-Crossbow-presented weapon attack through its native combat pipeline. |
| `04-post-attack-loaded-state-and-shield-faults.png` | `b3e9fbd6816dfdebd6001fdf11ca6d7a814e2145fc266f472dc3f85773d8c828` | After the attack the Test Musket still reports `rounds=1`; reload remains unavailable; attack enforcement remains `observed=0; fired=0`; reconciliation reports ten faults, last on `ItemEntityShield`. |

## Sprint 23 gate assessment

| Blocking criterion | Result | Evidence |
|---|---|---|
| Loaded Test Musket remains loaded immediately after quicksave | **Pass** | The loaded token remained present and reload stayed unavailable. |
| Reconciliation has no unresolved conflict or fault | **Fail** | `faults=10`; last fault is a `MissingMemberException` while inspecting `Kingmaker.Items.ItemEntityShield`. |
| Loaded state survives save, complete process exit, restart, and reload | Not established by this result | The supplied result confirms saving/quicksave no longer unloads in-process, but does not establish this exact build's full process-restart path. |
| Loaded Test Musket attacks through the native weapon pipeline | **Pass** | The combat log shows the ordinary Heavy Crossbow presentation completing an attack and damage. |
| The attack consumes exactly one loaded round from the firing item | **Fail** | The exact item remains at `rounds=1` after the attack. |
| Hit or miss does not change one-round consumption | Not testable after the failure | The observed hit consumed zero rounds. |
| Second attack while empty is forced to miss | **Fail / unreachable** | The first attack never made the weapon empty. |
| Empty-fire attempt leaves inventory ammunition unchanged | Not established | No valid empty-fire enforcement occurred. |
| Loaded Broken firearm discharges once and remains Broken | Not tested | Blocked by the first loaded-shot failure. |
| Wrecked Test Musket is forced to miss | Not tested | Blocked by the attack-hook failure. |
| Duplicate callbacks cannot consume twice | Not runtime-qualified | The attack callback was never observed. |
| Native Heavy Crossbow remains unaffected | Not tested as a negative control | The screenshot shows the Test Musket's borrowed presentation, not a confirmed native Heavy Crossbow item. |
| No bootstrap, token, attack-enforcement, or Harmony fault | **Fail** | Reconciliation faults are explicit; attack enforcement was never attached to a live roll. |

## Exact-reference diagnosis

Inspection of the supplied private `Assembly-CSharp.dll` established two independent contract errors in 0.0.22:

1. `RuleAttackRoll.OnTrigger`, `RuleAttackWithWeapon.OnTrigger`, and `RuleCalculateAC.OnTrigger` each have the exact installed signature `void OnTrigger(Kingmaker.RuleSystem.RulebookEventContext)`. The 0.0.22 resolver required zero parameters, selected no target, and caused Harmony to skip the attack-enforcement, touch-AC, and combat-trace patches.
2. `ItemEntity.ApplyEnchantments()` is correctly a zero-argument base method, but it runs for every `ItemEntity` subtype. The 0.0.22 prefix attempted firearm-token reflection on non-weapons, including shields, which explains the observed `ItemEntityShield` faults.

Machine-readable method metadata and source hashes are in `exact-runtime-contracts.json`.

## Bounded repair

Version 0.0.22.1 makes only the evidence-driven Sprint 22 corrections:

- bind exactly one non-static, non-generic, `void OnTrigger(RulebookEventContext)` declared on each intended rule-event type;
- let only `ItemEntityWeapon` instances enter firearm state-token inspection around `ItemEntity.ApplyEnchantments()`; and
- retain the existing fail-closed discharge, exact firearm marker, per-item token state, inventory boundary, and native Heavy Crossbow isolation logic unchanged.

No natural-roll reading, forced-roll diagnostic, misfire detection, condition transition, explosion, or other Sprint 23 behavior is present.
