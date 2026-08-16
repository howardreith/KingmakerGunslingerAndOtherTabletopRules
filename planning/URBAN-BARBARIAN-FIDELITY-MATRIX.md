# Urban Barbarian fidelity matrix

Status: **DESIGN GATE OPEN — native Rage inventory pending**.

| Surface | Tabletop requirement | Kingmaker implementation/adaptation | Qualification |
| --- | --- | --- | --- |
| Parent | Barbarian archetype | Native Barbarian class; additive archetype publication | Pending exact identity |
| Proficiency | Lose medium armor only | Archetype replacement retains simple, martial, light armor, and non-tower shields | Pending inventory and fixture |
| Skills removed | Handle Animal, Knowledge (nature), Survival | Remove consolidated Lore (Nature) from the archetype only | Authorized adaptation |
| Skills added | Diplomacy, Knowledge (local), Knowledge (nobility), Linguistics, Profession | Add Knowledge (World); retain applicable native Athletics, Mobility, Perception, Persuasion; no Profession substitute | Authorized adaptation |
| Class skill safety | Archetype-specific | Never mutate native Barbarian `ClassSkills` | Required source/runtime proof |
| Crowd Control attack | +1 adjacent to 2+ enemies | +1 untyped attack through owner-scoped attack rule event | Pending |
| Crowd Control AC | +1 dodge adjacent to 2+ enemies | +1 dodge AC through owner-scoped AC rule event | Pending |
| Crowd adjacency | Adjacent hostile crowd | Native edge-to-edge five-foot distance with corpulence; weapon reach irrelevant | Pending native contract |
| Active enemies | Current hostile active creatures | Native hostility plus life/target state; exact dead, unconscious, destroyed, untargetable, summoned, charmed, and faction-changed treatment must be recorded | Pending inventory/runtime |
| Crowd movement | Movement not impeded by crowds | Intentional no-op unless an exact crowd-movement subsystem is found; no difficult-terrain/freedom/speed/AoO approximation | Pending subsystem inspection |
| Crowd influence | Intimidate to influence crowds | Intentional no-op unless an exact crowd-influence subsystem is found; no global Persuasion/Intimidate bonus | Pending subsystem inspection |
| Rage pool | +4; +6 Greater; +8 Mighty | Morale bonuses to actual Strength/Dexterity/Constitution scores | Pending architecture gate |
| Allocations | Full or split in +2 increments | Deterministic current-tier vectors: 6, 10, 15 | Pending implementation |
| Selection | Player-controlled | Compact nested selector; free, persistent, locked while raging, unmistakable selected state | Pending implementation/runtime legibility |
| Tier state | Preserve legal selection per tier | Independent +4/+6/+8 state; default full Strength when each tier first unlocks | Pending |
| Native offensive Rage | None | Suppress ordinary native attack and damage bonuses for Urban owner only | Pending exact component classification |
| Native defensive Rage | No Will bonus, no AC penalty | Suppress exact ordinary components for Urban owner only | Pending exact component classification |
| Native temporary HP | None | Suppress exact native temporary-HP benefit | Pending exact component classification |
| Skills while raging | Permit Int/Dex/Cha skills | Remove only exact skill restriction while preserving concentration/spell restriction | Pending exact component classification |
| Spellcasting | Otherwise normal Rage | Preserve exact native concentration/spellcasting prohibition | Pending inventory/runtime |
| Lifecycle | Normal Rage | Preserve resource, live counter, per-round spend, cancel, fatigue, Tireless Rage, unconscious/end handling | Pending inventory/runtime |
| Rage powers | Normal integration | Preserve native and qualified CotW-added rage powers; no recommended powers granted automatically | Pending bridge decision |
| Rage equivalence | Feats/items/prerequisites recognize Rage | Prefer native identity with owner-scoped substitution; otherwise explicit exact bridge | Pending architecture gate |
| Constitution HP | Genuine Constitution modifier | Native morale ability modifier; explicit anti-heal/duplication/reconciliation policy | Pending implementation/runtime |
| CotW absent | Urban fully usable | Core available; interoperability N/A | Pending focused profile |
| CotW supported | Urban fully usable | Core available; marker/action behavior qualified; bridge only if proven necessary | Pending focused profile |
| CotW unknown/ambiguous | Urban remains usable | Core available; optional bridge disabled/unqualified with exact diagnostic | Pending fast/runtime tests |
| Module OFF | Hide new selection, preserve owners | Blueprints stay registered; remove only Urban publication; existing facts/progression function | Pending persistence scenario |

## Rage component classification ledger

The authoritative rows will be populated from the guarded no-CotW and
CotW-present inventory before production Rage code is written.

| Final Rage-buff component identity | Assembly/profile | Category | Retain/replace | Evidence |
| --- | --- | --- | --- | --- |
| Pending guarded inventory | Native / no-CotW | Pending | Pending | Pending |
| Pending guarded inventory | Final graph / CotW | Pending | Pending | Pending |

## Intentional no-op decision gate

No exact Kingmaker crowd-movement or crowd-influence subsystem has yet been
identified in repository source. The guarded native inventory and managed API
inspection must confirm that absence. If confirmed, both tabletop clauses are
intentional no-ops and will be repeated in README, known adaptations, runtime
matrix, and human acceptance material.

