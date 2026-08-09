# Cord of Stubborn Resolve Inventory

Status: IN PROGRESS

Exact assembly findings (2026-08-09): installed Kingmaker exposes `UnitCondition.Fatigued` and `.Exhausted` plus `SpellDescriptor.Fatigue` and `.Exhausted`. CotW uses both descriptor immunities and direct condition APIs and patches `RuleApplyBuff` in its shadow-spell mechanics; a single source-spell hook would therefore be incomplete. Supporting source uses `StatType.DamageNonLethal`, which is evidence that native nonlethal accounting may exist, but exact installed damage/event/serialization semantics remain to be proven before classification.

Required item: belt, 15,000 gp, 1 lb., caster level 8, moderate transmutation, native +2 enhancement Constitution, exact fatigue/exhaustion substitution while equipped, and exactly one capital-vendor row when its module is ON.

| Contract | Status | Evidence sought |
|---|---|---|
| Native +2 Constitution belt donor | IN PROGRESS | Guarded installed graph `20260809T1344106768806Z-mod-load-smoke`: unique internal `BeltOfConstitution2`, cost 4,000; exact GUID/equip delta still pending |
| Belt slot/cost/weight metadata | TODO | BlueprintItemEquipment contract |
| Fatigued application paths | TODO | Buff, condition, RuleApplyBuff, context action, direct AddBuff/AddCondition |
| Exhausted application paths | TODO | Same, including nested fatigue behavior |
| Equipped exact-item detection | TODO | Item enchantment/fact lifecycle |
| Native nonlethal support | TODO | Damage types, accumulators, serialization, healing, logs |
| Authorized capped fallback | TODO | Use only if native support conclusively absent |
| Capital merchant | IN PROGRESS | SmithVendorTable `7de959...` proven fallback; exact main-merchant graph pending |

Next concrete action: inspect native item donors, damage/unit-state assemblies and representative fatigue/exhaustion blueprints.
