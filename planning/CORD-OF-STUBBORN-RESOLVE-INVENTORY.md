# Cord of Stubborn Resolve Inventory

Status: PASS

Exact assembly findings (2026-08-09): installed Kingmaker exposes `UnitCondition.Fatigued` and `.Exhausted` plus `SpellDescriptor.Fatigue` and `.Exhausted`. CotW uses both descriptor immunities and direct condition APIs and patches `RuleApplyBuff` in its shadow-spell mechanics; a single source-spell hook would therefore be incomplete. Exact `UnitState.AddCondition(UnitCondition, Buff)` IL increments authoritative condition state and covers both buff-owned and direct sources while retaining the source buff for exact nested-source classification.

Native nonlethal classification: NOT APPLICABLE — authorized fallback selected. The installed game initializes and serializes `CharacterStats.DamageNonLethal` and exposes read-only descriptor/unit accessors, but exhaustive named-type/member and IL-reference inspection found no native rule event, write path, damage-kind flag, combat-log path, healing interaction, or unconsciousness application path. A dormant accumulator is not a usable native damage system. The implementation therefore uses one native d6 through self-originated untyped `RuleDealDamage`, with `MinHPAfterDamage = 1`, and describes it truthfully as a nonlethal-equivalent adaptation.

Required item: belt, 15,000 gp, 1 lb., caster level 8, moderate transmutation, native +2 enhancement Constitution, exact fatigue/exhaustion substitution while equipped, and exactly one capital-vendor row when its module is ON.

| Contract | Status | Evidence sought |
|---|---|---|
| Native +2 Constitution belt donor | IN PROGRESS | Guarded installed graph `20260809T1344106768806Z-mod-load-smoke`: unique internal `BeltOfConstitution2`, cost 4,000; inherited enchantment identity/equip delta still needs focused observer |
| Belt slot/cost/weight metadata | PASS | Exact donor concrete belt type retained; explicit cost 15,000, weight 1 lb., nonstackable clone |
| Fatigued application paths | IN PROGRESS | Exact convergence at `UnitState.AddCondition`; focused native-source runtime cases pending |
| Exhausted application paths | IN PROGRESS | Exact convergence plus source-bound downgrade bypass/duplicate suppression; runtime pending |
| Equipped exact-item detection | PASS | Exact `UnitBody.Belt.Item.Blueprint` reference; inventory-only instances cannot match |
| Native nonlethal support | NOT APPLICABLE | Dormant stat only; no usable native application semantics found |
| Authorized capped fallback | IN PROGRESS | Native `RuleRollDice` d6 capped to `HPLeft - 1`, then self-source/target flat untyped `RuleDealDamage`, DR ignored; final runtime/log proof pending |
| Capital merchant | PASS | Bounded graph retained `SmithVendorTable` `7de959347266092448d8a72089ef9778`: exact established-capital blacksmith owners `CapitalOwlbearAttack_Blacksmith` and `VerdelBlacksmith`; no more precise general always-available owner/table was proven, so the mission-authorized fallback applies |

Final qualification: standalone Cord runtime passes 7/7 with native belt equip/unequip, Constitution 10 to 12, inventory-only control, fatigue substitution, exhaustion-to-fatigue downgrade, one-roll isolation, two-unit isolation, re-equip cleanup, and the 1 HP floor. Integrated Acadamae runs prove the exact canonical Fatigued buff boundary, one d6 result, no retained inert buff, and ordinary fatigue after unequip. The capital observer proves one count-one `SmithVendorTable` row, exact owner graph, preservation, idempotence, and guarded rollback.

The fallback implementation uses `RuleRollDice(1d6)` and a self-originated untyped `RuleDealDamage` capped to `HPLeft - 1`; it never claims native nonlethal accounting. Player-facing text and notification call it nonlethal-equivalent.

Next concrete action: preserve this inventory as exact-contract evidence for final 0.0.75 qualification.

## Issue 12 acquisition supersession

Cord of Stubborn Resolve (`c4b804d9ebf941b4842b0a461a2b6b6d`) is no longer recurring Capital Smith stock. It is published exactly once to `RichHuman_treasure_chest_2` (`e2add2e7254305b40aa1b9ae60ed2be0`) in Capital Square Village beside the native Belt of Constitution +2. The exact snapshot transaction removes stale Cord vendor rows and rolls back both loot and vendor mutations on bootstrap failure.

Guarded runtime `20260820T0859287762363Z-observe-capital-cord-vendor` passed. Existing already-materialized vendor/container state is not claimed to refresh.
