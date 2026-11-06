# Bone Devil Sprint 3 — in progress (development candidate; draft PR)

**Base:** branch `codex/bone-devil-sprint3`, from master `dc609a10e6`
(PR #101 merged; `v0.0.178` published).

**Scope:** two gameplay corrections:
- Infernal Wound bleed on Wall of Ice sections
- natural reach versus the installed ProperFlanking2 reach dead zone

**Status:**
- Both corrections passed their native checks twice (s3a, s3b).
- The full Wall phase has not yet completed on one candidate. Both failures
  were fixture causes in later, unrelated steps, and both are corrected.
- One more guarded run (~11 min) is needed, and it exceeds the
  validation policy's 30-minute ordinary ceiling, so it awaits owner
  authorization.

## 1. Infernal Wound never bleeds a Wall section

**Rule.** Pathfinder bleed is a condition a creature takes at the start of
its turn. A Wall of Ice section is an object (the spell gives it hit points
per inch of ice), not a creature. This is the same basis already accepted for
formation saves.

**Production.**
- The Bearded Devil's Glaive no longer applies its Infernal Wound marker to
  either species' Wall section. `InfernalRules35.InfernalWoundExcludesTarget`
  feeds the existing `excludedUnitType` gate of
  `SummonInjuryDiseasePolicy.ShouldResolve`.
- Glaive weapon damage still applies, and creatures keep the printed wound.

**Regression:** `expanded-summoning.sprint35-wall-section-no-infernal-wound`.

**Native** (`wall-physical-destruction`, now strict: weapon-hit killing blow,
no bleed row, no attacker-owned effect on the section):

| Run | Glaive hits | Weapon damage | Killing blow | Bleed rows | Attacker effects on section |
| --- | --- | --- | --- | --- | --- |
| s3a | 6 | 39 | weapon hit | 0 | none |
| s3b | 7 | — | weapon hit | 0 | none |

Before this, close12 recorded a killing blow from the bleed, and close27
recorded the wound effect on the section.

## 2. Natural reach is legal beside the Bone Devil under ProperFlanking2

**Installed contract**, read from the decompiled assemblies:
- `BlueprintWeaponType.AttackRange` returns `max(2 ft, raw − 4 ft)` for raw
  ranges up to 10 ft.
- `UnitDescriptor.GetWeaponRange` adds the unit's native `ReachRange` stat.
- **ProperFlanking2** (`Patch_UnitCommand__GetTargetPoint`,
  `Patch_UnitEngagementExtension__IsReach`, setting `reach_weapons_dead_zone`
  true) treats a melee weapon as a reach weapon when its native **type** range
  exceeds `GameConsts.MinWeaponRange` (2 ft). Such a weapon cannot strike
  inside `(size reach + 1) ft × 0.5` beyond both bodies; the mod moves the
  approach point away instead.
- That is correct for a manufactured reach weapon. Natural reach, however,
  threatens adjacent squares.

**Why the Bone Devil failed.** Its bite, claws and sting used raw 9-ft
natural types (5 ft native), so the mod classed them as reach weapons.

**Production.**
- The natural types are ordinary raw 5-ft weapons (2 ft native, not reach
  weapons).
- The Bone Devil's 10-ft natural reach is expressed through native
  `ReachRange`: +3 ft (UntypedStackable) on its private Traits feature, with no
  new identity.
- Total reach is unchanged. The native approach radius is 4.5432 m both
  before (int3) and after (s3a) the change, and the tail-lane distance check
  still passes.
- ProperFlanking2 is neither disabled nor overridden, and manufactured reach
  weapons keep their dead zone.

**Regression:** `expanded-summoning.sprint35-bone-natural-reach-not-reach-weapon`.

**Native** (`bone-adjacent-natural-attack`). With ProperFlanking2 loaded, a
creature stood adjacent to the Large Bone Devil: edge gap 0.15 m, centre
distance 1.95 m against the mod's dead-zone bound of 3.32 m. In both runs the
native types were 2 ft, unit weapon range 9 ft, `IsReach` true, and the Bone
Devil travelled 0.0 m.

| Run | Full attack | Natural attacks | Hits |
| --- | --- | --- | --- |
| s3a | completed | 4 | 4 |
| s3b | — | 2 | 2 |

In s3b the command was interrupted by the Bone Devil's lifetime ending (see
below).

## Runs

| Run | Executable | Result | Cause (classification) | Build / native s |
| --- | --- | --- | --- | --- |
| s3a | `c1fe8b0711` | FAIL 111 | Both Sprint 3 checks pass. Then source B's Wall cast was interrupted three times: no clear summoner spot existed behind its caster beside Wall A, so native summon follow pulled it back (fixture); the observer then hit a null section (observer). | 345.24 / 287.70 |
| s3b | `cd23ceef6c` | FAIL 97 | Both Sprint 3 checks pass. The original Bone Devil's native summon lifetime ended (game second 43320.667) 7 ms before the adjacency command's interrupt, because earlier steps ran about 12 s longer; source cleanup then found no lifetime (fixture timing). | 347.01 / 242.13 |

**Corrections, observer only:**
- After s3a: a wider summoner spot search around each Wall caster, and a
  null-safe formation receipt (`cd23ceef6c`).
- After s3b: the adjacency check moved into the two-Wall scenario, using a
  freshly summoned source Bone Devil after the established fear-aura exposure
  (`717b460c1a`).

**Restoration.** Every run restored the intake exactly: mod 0.0.140 read
before and after, settings restored, protected saves unchanged, live tree
restored.

**Ledger.**
- **Sprint 3:** ≥ 21.30 min (3 gates 56.0 s, 2 builds 692.24 s, 2 guarded
  runs 529.83 s).
- **Cumulative:** ≥ 636.49 min.
- Both are lower bounds; decompilation and analysis are not counted.

## Remaining

1. **One guarded `summon-overhaul-wall` run** of the current head, needed for
   a whole-phase pass of both corrections together with the unchanged Wall
   acceptance. It exceeds the 30-minute ceiling and awaits authorization.
2. **Census follow-up**, not changed here. These summons express reach or
   ordinary range through raw 9–10-ft **natural** types, which ProperFlanking2
   also treats as reach weapons:
   - **Small and Medium:** Lemure claw, Dretch, Hound and Lantern Archon,
     Astral Deva slam, Barbed Devil claw, Sprint 36 bodies. The dead zone is
     about 0.15 m beyond the bodies.
   - **Larger:** Lillend tail; Ankylosaurus, Stegosaurus and Brachiosaurus
     tails; Salamander tail.

   Manufactured raw 10-ft weapons (Glaives, Giant flails) are correct reach
   weapons.
3. **Ice Devil natural weapons:** not assessed for the same pattern.
   `sprint40-*` phases run only on HOWARD-LAPTOP.
