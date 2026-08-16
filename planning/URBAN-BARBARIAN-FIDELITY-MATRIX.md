# Urban Barbarian fidelity matrix

Status: **IMPLEMENTATION AND FOCUSED GAMEPLAY CHECKPOINT PASS; final
persistence, compatibility-profile, and boundary qualification pending**.

| Surface | Tabletop requirement | Kingmaker implementation/adaptation | Qualification |
| --- | --- | --- | --- |
| Parent | Barbarian archetype | Native Barbarian `f7d7eb166b3dd594fb330d085df41853`; additive archetype publication | Exact identity qualified in both profiles |
| Proficiency | Lose medium armor only | Archetype replacement retains simple, martial, light armor, and non-tower shields | Fast/source and focused fixture PASS |
| Skills removed | Handle Animal, Knowledge (nature), Survival | Remove consolidated Lore (Nature) from the archetype only | Authorized adaptation |
| Skills added | Diplomacy, Knowledge (local), Knowledge (nobility), Linguistics, Profession | Add Knowledge (World); retain applicable native Athletics, Mobility, Perception, Persuasion; no Profession substitute | Authorized adaptation |
| Class skill safety | Archetype-specific | Never mutate native Barbarian `ClassSkills` | Fast/source and focused fixture PASS |
| Crowd Control attack | +1 adjacent to 2+ enemies | +1 untyped attack through owner-scoped attack rule event | Focused fixture PASS |
| Crowd Control AC | +1 dodge adjacent to 2+ enemies | +1 dodge AC through the defender/target-scoped `RuleCalculateAC` event. Exact `OnTrigger` IL reads `RulebookTargetEvent.Target.Stats.AC`; the synchronous temporary modifier refreshes native AC before rule resolution. | Focused fixture PASS |
| Crowd adjacency | Adjacent hostile crowd | Live 0.0.83 observation proved `UnitEntityData.DistanceTo` reports center-to-center distance for the request-local units. The implementation converts it to native edge distance with `max(0, DistanceTo - owner.Corpulence - candidate.Corpulence)`, then applies 1.524 m plus float tolerance; weapon reach is never read. | Corrected large-corpulence and reach-independent fixture PASS |
| Active enemies | Current hostile active creatures | Re-evaluate current `IsEnemy` at each rule event; require in-game, not destroyed/detached, turned on, and conscious. Dead/unconscious/destroyed/detached/turned-off are excluded. Summons count normally. Charmed/faction-changed units follow current hostility. No separate native `Untargetable` condition exists; a conscious active hostile under a target-selection-only restriction still counts. | Native group-hostility, unconscious, dead, destroyed, and live-threshold fixture PASS |
| Crowd movement | Movement not impeded by crowds | Intentional no-op: no precise crowd-movement subsystem exists; no difficult-terrain/freedom/speed/AoO approximation | Adaptation locked |
| Crowd influence | Intimidate to influence crowds | Intentional no-op: no precise crowd-influence subsystem exists; no global Persuasion/Intimidate bonus | Adaptation locked |
| Rage pool | +4; +6 Greater; +8 Mighty | Morale modifiers on actual Strength/Dexterity/Constitution values; tier derives from exact native Greater/Mighty facts | Architecture locked |
| Allocations | Full or split in +2 increments | Deterministic current-tier vectors: 6, 10, 15 | Fast/source and focused fixture PASS |
| Selection | Player-controlled | Compact nested selector; free, persistent, locked while raging, unmistakable selected state. A primitive serialized `UnitPartControlledRageSelection` is authoritative; stable selection facts are synchronized carriers only. | Mechanical contract PASS; corrected save/restart carrier pending; final legibility is human acceptance |
| Tier state | Preserve legal selection per tier | Independent +4/+6/+8 persisted state; default full Strength when each tier first unlocks; lower-tier values are retained without mapping | Fast/source transition and serialization policy PASS; guarded rerun pending |
| Native offensive Rage | None | Owner-scoped buff substitution removes exact melee damage, thrown damage, and melee attack components | Exact components classified |
| Native defensive Rage | No Will bonus, no AC penalty | Owner-scoped clone removes exact Will and AC components | Exact components classified |
| Native temporary HP | None | Owner-scoped clone removes exact temporary-HP component | Exact component classified |
| Skills while raging | Permit Int/Dex/Cha skills | Native final Rage buff has no separate skill-lock component; no new skill lock is introduced | Exact final graph inspected |
| Spellcasting | Otherwise normal Rage | Retain exact `ForbidSpellCasting` component with magic items allowed | Final graph and focused fixture PASS |
| Lifecycle | Normal Rage | Retain native feature, activatable, resource/fact, activation actions, shared value, descriptor, fatigue actions, cancellation, and Tireless fact | Focused resource/fatigue/Tireless fixture PASS |
| Rage powers | Normal integration | Retain native Rage feature/activatable plus `UnitCondition.BarbarianRage` action and Rage descriptor; no recommended powers granted automatically | Native focused mechanics PASS; final CotW profile pending |
| Rage equivalence | Feats/items/prerequisites recognize Rage | Retain exact native Rage feature `247939...`; substitute only buff application for Urban owners | Architecture locked |
| Constitution HP | Genuine Constitution modifier | Native morale ability modifier; explicit anti-heal/duplication/reconciliation policy; active-buff load reads the authoritative persisted allocation and re-synchronizes modifiers | Focused low-HP/repeated-cycle fixture PASS; corrected guarded save/load pending |
| CotW absent | Urban fully usable | Core available; interoperability N/A | Pending focused profile |
| CotW supported | Urban fully usable | Core available; marker/action behavior qualified; bridge only if proven necessary | Pending focused profile |
| CotW unknown/ambiguous | Urban remains usable | Core available; optional bridge disabled/unqualified with exact diagnostic | Fast structural simulations PASS |
| Module OFF | Hide new selection, preserve owners | Blueprints stay registered; remove only Urban publication; existing facts/progression function | Fast publication PASS; guarded existing-owner persistence pending |

## Rage component classification ledger

| Final Rage-buff component identity | Assembly/profile | Category | Retain/replace | Evidence |
| --- | --- | --- | --- | --- |
| `[0] TemporaryHitPointsPerLevel` | Native / both | ordinary Unchained Rage benefit | Remove from Urban clone | 1 HP/level using Rage rank |
| `[1] BuffParticleEffectPlay` | Native / both | presentation | Retain | activation/deactivation/round FX |
| `[2] AddFactContextActions` | Native / both | activation/resource/fatigue lifecycle and Rage marker | Retain exact action graph | Provides ordinary end/fatigue handling and `UnitCondition.BarbarianRage` |
| `[3] ContextRankConfig` | Native / both | native rage-power/tier integration | Retain | feature-list rank uses Greater/Mighty facts; CotW final graph also includes Bloodrage tier facts |
| `[4] AddContextStatBonus(AC,-2,None)` | Native / both | ordinary Unchained Rage benefit | Remove from Urban clone | exact AC penalty |
| `[5] AddContextStatBonus(SaveWill,+rank,None)` | Native / both | ordinary Unchained Rage benefit | Remove from Urban clone | exact Will bonus |
| `[6] ContextCalculateSharedValue(Duration)` | Native / both | activation/lifecycle support | Retain | shared duration value |
| `[7] WeaponAttackTypeDamageBonus(Melee,+rank,None)` | Native / both | ordinary Unchained Rage benefit | Remove from Urban clone | melee damage |
| `[8] ForbidSpellCasting(ForbidMagicItems=false)` | Native / both | concentration/spell restriction | Retain | ordinary spellcasting prohibition; magic items unaffected |
| `[9] SpellDescriptorComponent(Rage)` | Native / both | native/CotW rage-power marker integration | Retain | exact Rage descriptor identity |
| `[10] WeaponGroupDamageBonus(Thrown,+rank,None)` | Native / both | ordinary Unchained Rage benefit | Remove from Urban clone | thrown damage |
| `[11] AttackTypeAttackBonus(Melee,+rank,None)` | Native / both | ordinary Unchained Rage benefit | Remove from Urban clone | melee attack |
| `[12..15] FeatureReplacement` | CotW-present only | CotW Bloodrager/Urban Bloodrager routing | Do not copy to Urban clone | target only CotW Bloodrage and whole-stat Urban Bloodrager buffs; not Urban Barbarian behavior |

## Intentional no-op decision gate

Managed API and repository inspection found no exact Kingmaker crowd-movement
or crowd-influence subsystem. Both clauses are therefore intentional no-ops and
must be repeated in README, known adaptations, runtime matrix, and human
acceptance material.
