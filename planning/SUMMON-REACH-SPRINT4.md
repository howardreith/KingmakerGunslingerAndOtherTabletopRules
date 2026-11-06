# Summon reach Sprint 4 — corrected and natively accepted (development work)

**Base:** stacked on PR #103 (`codex/bone-devil-sprint3`, accepted head
`75c39a693f`, tested source `baf796c4d6`). #103 merges first, then this
branch.

**Scope:** published summon weapons whose type made ProperFlanking2 impose a
reach-weapon dead zone.

**Status:** complete. Accepted executable candidate **`a9e0ac398d`**, run
**s4g**: PASS, 41/41 assertions and 34/34 Sprint 4 cells.

**Representative coverage.** Native evidence comes from representatives,
not an individual native pass for every creature. The mapping is below.
Ice Devil native acceptance is **host-blocked**: `sprint40-*` phases run
only on HOWARD-LAPTOP, and this host is DATA.

## Installed contract

Read from the decompiled Kingmaker 2.1.7b and ProperFlanking2 1.2.3-2.1:

- **Type range.** `BlueprintWeaponType.AttackRange` is `max(2, raw - 4)`
  ft for raw ≤ 10, and the raw value above that.
- **Reach range.** The Reach stat is the size modifier (Small/Medium 5,
  Large 9, Huge 12, Gargantuan 15, Colossal 18) plus `AddStatBonus` Reach.
  `ReachRange` is `max(Reach - 5, 0)`.
- **Live range.** `ItemEntityWeapon.AttackRange` is
  `UnitDescriptor.GetWeaponRange` = type range + `ReachRange`, plus every
  exact-item `get_AttackRange` postfix.
- **Native threat.** `IsReach` is true when the edge gap is less than the
  live range and the attacker has line of sight. `UnitAttack` approaches to
  its planned attack's own range.
- **ProperFlanking2.** A melee weapon whose **type** range exceeds
  `GameConsts.MinWeaponRange` (2 ft) is a reach weapon. It refuses targets
  within `(size reach + 1) ft x 0.5` beyond both bodies (Small/Medium
  0.914 m, Large 1.524 m, Huge 1.981 m), in both `IsReach` and the approach
  point. The setting `reach_weapons_dead_zone` is on, and was read live in
  every run.

## Rules basis

Primary sources (legacy.aonprd.com Core Rulebook, Bestiary):

- **Reach weapons.** "Glaives, guisarmes, lances, longspears, ranseurs,
  and whips are reach weapons." "You use a reach weapon to strike opponents
  10 feet away, but you can't use it against an adjacent foe."
- **Natural attacks.** They "can be made against any creature within your
  reach." Larger creatures' natural reach threatens adjacent squares.
- **Ordinary weapons.** The flail, heavy flail and spear do not have the
  reach feature. The Ettin's two flails, the Ice Devil's spear, and the
  giants' greatclubs, greatswords and morningstars are ordinary weapons
  wielded with creature reach.
- **Per-attack extensions.** The Salamander has "Reach 5 ft. (10 ft. with
  tail)", and the Kyton "Reach 5 ft. (10 ft. with chains)". These are
  per-attack natural reach, not reach weapons. The Kyton text does not
  classify the chains; this sprint treats them as natural, as the project
  models them.
- **Whip.** There is no whip in the roster. The whip's adjacent-foe
  exception was only available as a summary and was not verified verbatim.

## Representation

The s4c census of installed blueprints found 47 melee rows with a type
range above 2 ft:

- **45 false classifications.** Natural attacks and ordinary manufactured
  weapons that carried creature reach in the type's raw range.
- **2 genuine reach weapons.** The Bearded Devil glaive and the Babau
  longspear. They are unchanged and keep their dead zone.

**Production** (`f74c8f664c`):

- Each of the 45 types stores the ordinary raw 5 ft (2 ft native, not a
  ProperFlanking2 reach weapon).
- `SummonReachRange4` adds back exactly what the type lost. It is an
  exact-owner, exact-item `ItemEntityWeapon.get_AttackRange` correction, the
  pattern the released Griffon, Gorgon/Bulette, Elephant, Triceratops,
  Barbazu and Babau corrections already use.
- So **every row's live range is unchanged**, and each attack keeps its own
  range: the Salamander spear stays 2 ft and its tail 6 ft.
- The table and its arithmetic live in `SummonReachRepresentation4`.

**Why not a reach stat.** An owner-wide Reach bonus (the Bone Devil
pattern) would also move ranged weapons (giant rocks, bows, javelins) and
cannot express unequal per-attack reach. The exact-item correction changes
nothing else.

**Existing corrections:**

- **Huge giants.** The Cloud and Storm Giants keep their absolute 15-ft
  correction (`SummonSprint29HugeMeleeReach`); only their stored raw range
  changed.
- **Kyton chain.** It keeps its existing +4, and +15 more while it controls
  its chains (`InfernalWeaponReach34`). The new +4 is additive.

**Unchanged:** stable GUIDs, damage, attack counts, critical properties,
ProperFlanking2 and its settings. Builders' own guards were updated to
expect the native minimum.

**Consequence (rules-aligned).** Barbed Defense exempts manufactured melee
weapons whose type range exceeds 2 ft, which models "weapons with
exceptional reach do not endanger their users." Ordinary weapons wielded
with creature reach (Giant greatclubs, Ettin flails, Archon greatswords)
were wrongly exempt and are no longer. The glaive and longspear stay
exempt. This is covered by a regression.

## Open owner questions (recorded, not changed)

1. **Glabrezu and Nalfeshnee** live reach is 22 ft against a printed 15 ft.
   Sprint 38 recorded this and Sprint 4 preserves it.
2. **Medium natural attacks** (Lemure, Dretch, archons, devils) use a 5-ft
   live range for a printed 5-ft reach, where native Kingmaker Medium
   creatures use 2 ft. This is the project's adaptation, preserved.
3. **Salamander tail** live range is 6 ft (the Sprint 17 accepted mapping)
   against a printed 10 ft. Preserved.

## Regressions

**Command:**
`scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.summon-reach4;expanded-summoning.sprint35;expanded-summoning.sprint32-native-admission;expanded-summoning.sprint17-observer'`

**Result:** 43 tests, 0 failures, production compiled. The Sprint 22 rules
were also run once with this set (54 tests).

**`summon-reach4-*` behaviour cases:**

- Live range is preserved and the dead zone is gone, for all 45 rows.
- At the adjacent distance the target is legal after (refused before);
  just inside is legal; just beyond is rejected.
- Genuine reach weapons keep their dead zone and their farther attack.
- Mixed and unequal per-attack routines keep their differences.
- Shared items (giant slam, greatclub, Ettin flails) are corrected per
  owner, and other owners are untouched.
- The correction is exact and stateless, with no accumulation and no
  stacking on absolute corrections.
- The Barbed Defense boundary holds.
- The generalized adjacent-routine verdict passes complete and exhausted
  routines and evidenced substitutions, and rejects partial routines,
  dropped slots, foreign interrupts, lifetime loss, a foreign target,
  ProperFlanking2 disabled, a non-adjacent target, movement, a terminal
  grab missing any piece of evidence, and substitutions that lack the
  installed toggle, are charge-only or inactive, are doubled, sit on a
  weapon slot, double-count with a missing slot, or fall outside the plan.

The Sprint 3 Bone Devil verdict and its regressions are unchanged.

## Native acceptance — s4g (accepted)

**Identity:**
- **Commit:** `a9e0ac398d46603fa855e1786c7779000702b345`.
- **Package:** `KingmakerGunslinger-0.0.178-development-summon-outsider-review-corrections-a9e0ac398d46.zip`,
  SHA-256 `17e2e255e720b9545edc98d83b33088793695d1937f667364713bca08a60374d`.
- **Loaded module:** MVID `0251abfd-18ef-4860-adc0-7a436cee50ff`, SHA-256
  `0b1d652727772449809b966e661e2959cc583f6d0823c323a540597914177e2b`, game
  2.1.7b.
- **Run:** `20261106T2253054955548Z-0d58f3f471f241d3870a0ca218cc376e`.
- **Evidence:** `runtime-evidence/20261106T2253054705557Z-disposable-expanded-summoning-sprint32-native-review/s4-summon-reach.json`
  (machine-local).
- **Restoration record:** `runtime-evidence/expanded-summoning-restoration/20261106T2255131099009Z-disposable-expanded-summoning-sprint32-native-review.json`.

**Cells:**

- **ProperFlanking2.** The assembly is loaded; the UMM entry
  `ProperFlanking2` 1.2.3-2.1 is active; the live
  `reach_weapons_dead_zone` is true; and its postfixes are installed on
  `UnitCommand.GetTargetPoint` and both `IsReach` overloads.
- **Census after.** Every converted row is raw 5 / type 2, and the only
  remaining ProperFlanking2 reach weapons are the glaive and the longspear.
- **Threat probes (24 representatives).**
  - Each wielded weapon was probed at an edge gap of 0.15 m, 0.1 m inside
    its own live range, and 0.1 m beyond it.
  - The observed native `IsReach` equals the installed contract in every
    sample.
  - Converted rows keep their pre-Sprint-4 live range, are legal adjacent
    and inside, and are rejected beyond.
  - The glaive is refused adjacent and legal at 10 ft.
- **Adjacent full attacks.**
  - **Setup:** fresh summons with about 120 s of lifetime against the
    durable fixture hostile (100,000 HP, no lifetime).
  - **Geometry:** edge gap 0.15 m; both units travelled 0.0 m.
  - **Expected routine:** derived independently from the body and
    Pathfinder full-attack rules (base-attack iteratives for a manufactured
    primary, one attack per natural limb).
  - **Integrity:** every slot is tied to the exact attacker, target and
    command. No roll, position, eligibility or plan was written.

| Representative | Mechanism | Expected routine | Native result |
| --- | --- | --- | --- |
| Lemure | Medium natural | claw x2 | 2 resolved (0 hit, 2 miss), Success |
| Hezrou | Large natural | bite, claw x2 | 3 hits, Success |
| Hill Giant | Large ordinary manufactured | greatclub x2 (BAB 7) | 2 hits, Success |
| Salamander | mixed manufactured/natural, unequal reach | spear x2 (BAB 8), tail | 3 resolved (1 hit, 2 miss), Success |
| Storm Giant | Huge manufactured, existing absolute override | greatsword x3 (BAB 14) | slot 1 by native Improved Sunder substitution, slots 2-3 hits; Success |
| Cloud Giant | same override path | morningstar x3 | 3 hits, Success |

**Storm Giant substitution.**
- **Option before the command:** ProperFlanking2's default-on
  `SunderActionToggleBuff` (AttackReplacementWithCombatManeuver, SunderArmor,
  not charge-only) was active.
- **Slot 1:** at act index 1 there is a standalone roll (no outer weapon
  rule) against the exact target with the planned greatsword. No weapon
  attack occurred at that instant, and the target gained `SunderArmorBuff`.
- **Slots 2 and 3:** greatsword hits with their own distinct rolls.
- **End:** the command ended `Success` at index 3.

The feats and ProperFlanking2's configuration are unchanged.

**Coverage map.**

| Changed mechanism | Native representative |
| --- | --- |
| Small/Medium natural (raw 9 to 5, +3) | Lemure, Dretch, Barbed Devil, Babau (probes); Lemure (routine) |
| Medium ordinary manufactured (raw 9) | Hound Archon greatsword, Astral Deva warhammer (probes) |
| Natural per-attack extension (raw 10 to 5, +4) | Salamander tail (probe, mixed routine); Kyton chain over its existing +4 (probe) |
| Larger creature natural (raw 10, Large) | Hezrou (probe, routine), Vrock, Lillend tail (probes) |
| Larger ordinary manufactured (raw 10, Large) | Hill and Fire Giant, Ettin flails, Lillend longsword (probes); Hill Giant (routine) |
| Huge natural without an item correction (raw 15 to 5, +13) | Glabrezu (probe) |
| Huge manufactured on the absolute override (raw 15 to 5) | Storm Giant (probe, routine), Cloud Giant (routine) |
| Dinosaur tails with chassis Reach bonuses (raw 9) | Ankylosaurus, Brachiosaurus (probes) |
| Genuine reach control | Bearded Devil glaive (probe); Babau longspear (probe) |
| Unaffected controls and existing corrections | Bone Devil (Sprint 3), Griffon, Elephant, Wolf, Dire Bear (probes) |

No native run covers these rows; they are covered by the same mechanism
and the domain regressions:
- Stone Giant (it shares Hill Giant items);
- Trumpet Archon;
- Nalfeshnee;
- Stegosaurus;
- Ice Devil (host-blocked).

## Runs (Sprint 4 ledger)

| Run | Source | Result | Cause / classification | Build s | Native s |
| --- | --- | --- | --- | --- | --- |
| s4a | `a04650d861` | refused before game start | launcher allowlist derives phases from `Sprint32..40*Phase` constants; the constant was `Sprint4...` (observer) | 387.86 | 37.40 |
| s4b | `5c458bf62b` | build aborted | a file was written into the worktree during the build; "source state changed" (process) | 367.57 | - |
| s4c | `5c458bf62b` | FAIL (probes); census PASS | **before-fix census** (134 creatures, 218 rows); probe geometry used 3D distance at 0.1 m (observer) | 380.47 | 125.70 |
| s4d | `dfa798e34a` | FAIL | first fixed candidate; probe verdict misread null errors (observer; reader-only re-evaluation PASS 24/24); Salamander terminal grab; Storm Giant slot unexplained | 377.79 | 154.73 |
| s4e | `d2c45477d4` | FAIL (Storm Giant only) | diagnostic; first slot a standalone d20-0 roll | 365.96 | 155.58 |
| s4f | `02bf9bcab3` | FAIL (Storm Giant only) | diagnostic; identified ProperFlanking2's Improved Sunder substitution; Cloud Giant PASS | 368.41 | 160.78 |
| **s4g** | **`a9e0ac398d`** | **PASS 41/41** | **accepted** | 391.83 | 164.95 |

**Diagnostic versus accepted.** s4a-s4f are failed or diagnostic runs,
retained as records. s4g alone is the accepted run. Production has been
identical since `f74c8f664c` in s4d-s4g.

**Focused gates** (9 runs): 143.89 s, including one compile failure
(11.72 s).

**Total:** **3582.92 s = 59.72 min** of the 90-minute allowance, a lower
bound with analysis not counted. Sprint 3 ended at >= 648.38 min
cumulative; the cumulative is now **>= 708.10 min**.

**Restoration.** Every transaction that deployed (s4a, s4c-s4g) restored
the intake exactly:
- the installed version 0.0.140 was read before and after;
- settings were restored;
- protected saves are unchanged;
- the live tree was restored (`sha256=5C3E4AD4...CB9BFBB`);
- `KMG_AUTOMATION_BASELINE` was untouched.

s4b never deployed. No host restriction was bypassed.

## Change separation

| Kind | Commits |
| --- | --- |
| Production + its regressions | `f74c8f664c` |
| Observer / fixture / verdict | `a04650d861`, `5c458bf62b`, `dfa798e34a`, `d2c45477d4`, `02bf9bcab3`, `a9e0ac398d`, `a332b1bd61` |
| Documentation | this record and the Sprint 3 follow-up note |

`a332b1bd61` follows the accepted executable. It is a pure verdict
hardening: each substituted slot must be unique and must not sit on a
credited weapon attack. A reader-only re-evaluation of the s4g raw
evidence passes all six routines; the old and new verdicts are retained.
The released Glabrezu and Sprint 17 Salamander observers follow the new
type representation, and were not rerun natively this sprint.

## Follow-up (not completion requirements)

- **Ice Devil** native acceptance on HOWARD-LAPTOP: the `sprint40-*`
  phases, including the adjacent spear, bite and tail.
- **Owner decisions** on the three open questions above.
- **Stone Giant, Trumpet Archon, Nalfeshnee and Stegosaurus** native probes,
  if individual evidence is wanted.

## Complete census (s4c, installed blueprints, before the fix)

**Sources:**
- **"Raw -> type" and "computed unit ft"** were read in-process from the
  installed blueprints; unit range = type range + ReachRange, before
  item-level postfixes.
- **"Status"** comes from source analysis and the Sprint 4 table.
- **Live values after the fix** come from the representative s4g probes
  above.

| Creature | Unit GUID | Size / ReachRange | Item (GUID) | Type GUID | Kind | Raw → type ft | Computed unit ft | PF2 reach weapon (before) | Shared consumers | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| air-mephit | `9de31a332f814795b7258bd1d3813678` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| ankylosaurus | `10c80d5cfa594332bd3e5127799e426f` | Huge / 10 | KMG_Summoning_Natural_Tail3d6 (`15394605e1664a51bce4b50f38a7603a`) | `7243c9cd0f654b9080e7b31112ddbf18` | nat melee | 9 → 5 | 15 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 15 → 15 ft |
| ape | `c53c3e23097e4f25a411c50ce2868c60` | Large / 4 | KMG_Summoning_Natural_Slam1d6 (`2a445954332645149e9586fb6ac8e86d`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| astral-deva | `2814e9aa55e34dda8f3322d84a5fe5e2` | Medium / 0 | KMG_Summoning_OutsiderCelestial_AstralDeva_Warhammer1d8 (`d9d766472a9d4c168555e48a59f39e9b`) | `248408477486430e9b633067c67b8f7c` | mfg melee | 9 → 5 | 5 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| astral-deva | `2814e9aa55e34dda8f3322d84a5fe5e2` | Medium / 0 | KMG_Summoning_OutsiderCelestial_AstralDeva_Slam1d8 (`1e3e03e0a8094a838077e3e3839d6dff`) | `f28265c2e8094f9286d3c0752c5cb615` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| astral-deva | `2814e9aa55e34dda8f3322d84a5fe5e2` | Medium / 0 | TouchItem (`bb337517547de1a4189518d404ec49d4`) | `ba9cd5a303b089f458aa9f07aa10ea10` | mfg melee | 5 → 2 | 2 | no | bralani-azata | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| aurochs | `40bf3f1e2b094e83ba2ab498c3d5d603` | Large / 0 | GoreTwoHandedLarge1d8 (`73ed4e955295e62469fe471f1d49d9ef`) | `904e948c3e4c6084e90613bdaf1469c6` | nat melee | 5 → 2 | 2 | no | boar | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| babau | `1512476cf6674704b6b0bcc3b67b48d9` | Medium / 0 | KMG_Summoning_Infernal_Babau_Bite (`eb97ebcb9cf241a2a930d366173b6295`) | `e807a76b44f74ab1861bd23474407df4` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| babau | `1512476cf6674704b6b0bcc3b67b48d9` | Medium / 0 | KMG_Summoning_Infernal_Babau_Claw (`56f9470894df4fe7b6e5cf57610191fb`) | `e412dd1fe56a4cc9a5ead5a8bd523706` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| babau | `1512476cf6674704b6b0bcc3b67b48d9` | Medium / 0 | KMG_Summoning_Infernal_Babau_Longspear (`979ea8542538458184a4ae084c8d7f73`) | `81a9bb54753c40f08b36e5d1fa056172` | mfg melee | 10 → 6 | 6 | yes | — | UNCHANGED: genuine reach weapon (longspear); existing +4 (InfernalWeaponReach36) → 10 ft; dead zone kept |
| barbed-devil | `a8ed4e16ec4b48a5bdfdc47f7f1b9cd0` | Medium / 0 | KMG_Summoning_Infernal_BarbedDevil_Claw (`c4743cbf5fb7435eb7cedb8f103d6ee5`) | `dd87db14497b41f09f87a1e2427f1fee` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| barbed-devil | `a8ed4e16ec4b48a5bdfdc47f7f1b9cd0` | Medium / 0 | RayItem (`f6ef95b1f7bb52b408a5b345a330ffe8`) | `1d39a22f206840e40b2255fc0175b8d0` | mfg ranged | 50 → 50 | 50 | no | bebelith,fire-mephit,giant-spider,ooze-mephit,water-mephit | UNCHANGED: ranged, not a dead-zone defect |
| bearded-devil | `182032f380314a7581d0016c443945a2` | Medium / 0 | KMG_Summoning_Infernal_BeardedDevil_Glaive (`804dc1aecc9f43b9bcad0c48547c8b28`) | `bf1d7fd267a84f1bbd364f68037c1e6c` | mfg melee | 10 → 6 | 6 | yes | — | UNCHANGED: genuine reach weapon (glaive); existing +4 (InfernalWeaponReach34) → 10 ft; dead zone kept |
| bearded-devil | `182032f380314a7581d0016c443945a2` | Medium / 0 | KMG_Summoning_Infernal_BeardedDevil_Claw (`5e14ac33c7d1442ab29285bb3901b66f`) | `a916da7d267f483f8b29156094f25de4` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| bebelith | `9d7d36e71ea141258509b8a32557577e` | Huge / 7 | KMG_Summoning_Special_Bebelith_Claw (`85971d6300dd41a0a62b0dd92a570045`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bebelith | `9d7d36e71ea141258509b8a32557577e` | Huge / 7 | BiteHuge2d6 (`d2f99947db522e24293a7ec4eded453f`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 9 | no | pteranodon | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bebelith | `9d7d36e71ea141258509b8a32557577e` | Huge / 7 | RayItem (`f6ef95b1f7bb52b408a5b345a330ffe8`) | `1d39a22f206840e40b2255fc0175b8d0` | mfg ranged | 50 → 50 | 57 | no | barbed-devil,fire-mephit,giant-spider,ooze-mephit,water-mephit | UNCHANGED: ranged, not a dead-zone defect |
| bison | `d1f0439382454d908f2086dda33eafcc` | Large / 0 | GoreTwoHandedLarge2d6 (`d1f80b5c5c73cc84db7854774850b08c`) | `904e948c3e4c6084e90613bdaf1469c6` | nat melee | 5 → 2 | 2 | no | dire-boar,rhinoceros | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| boar | `d98699ea265441a09c5b9b51769ef7ce` | Medium / 0 | GoreTwoHandedLarge1d8 (`73ed4e955295e62469fe471f1d49d9ef`) | `904e948c3e4c6084e90613bdaf1469c6` | nat melee | 5 → 2 | 2 | no | aurochs | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bone-devil | `74652de839a94feba3c2fb67b536437c` | Large / 7 | KMG_Summoning_Infernal_BoneDevil_Bite (`a54ab76372484c088cfe028fe7854694`) | `89d4277562a34373882fc8ea2c31922b` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bone-devil | `74652de839a94feba3c2fb67b536437c` | Large / 7 | KMG_Summoning_Infernal_BoneDevil_Claw (`d0f2530de475451a815b1861c618776d`) | `b052e61eb9544424a080c139d0ef73f6` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bone-devil | `74652de839a94feba3c2fb67b536437c` | Large / 7 | KMG_Summoning_Infernal_BoneDevil_Sting (`e341953286ac453a8adc1747671e025d`) | `baa74fc8abf643b3b80c8b49c467dfa6` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| brachiosaurus | `9093aef39ecc416092ef68655a7e56fb` | Gargantuan / 15 | KMG_Summoning_Natural_Brachiosaurus_Tail4d6 (`13b51070d6f84a15bfed98a04c98fa30`) | `5aaf157b2f5b489390f241bd9ec2decf` | nat melee | 9 → 5 | 20 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 20 → 20 ft |
| bralani-azata | `fb640167f05541f2963de3d31e217d11` | Medium / 0 | TouchItem (`bb337517547de1a4189518d404ec49d4`) | `ba9cd5a303b089f458aa9f07aa10ea10` | mfg melee | 5 → 2 | 2 | no | astral-deva | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bralani-azata | `fb640167f05541f2963de3d31e217d11` | Medium / 0 | WeaponEmptyHand (`20375b5a0c9243d45966bd72c690ab74`) | `a8b38f6b734daa44087ec0ec2e80c1cd` | nat melee | 5 → 2 | 2 | no | erinyes-devil,ghaele-azata | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bralani-azata | `fb640167f05541f2963de3d31e217d11` | Medium / 0 | CompositeLongbowBralaniAzata (`0f0b7863d48f76b49b0c5b2691d5f566`) | `1ac79088a7e5dde46966636a3ac71c35` | mfg ranged | 50 → 50 | 50 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| bralani-azata | `fb640167f05541f2963de3d31e217d11` | Medium / 0 | ScimitarPlus1 (`7bef1327d4cb31249823d9fcb24af332`) | `d9fbec4637d71bd4ebc977628de3daf3` | mfg melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| bulette | `f84a0babcc864630bae225b2019d6bba` | Huge / 3 | KMG_Summoning_Natural_Bulette_Bite2d8 (`f7801f4601554a478920e839ec40d9a4`) | `8b42eea07bc7444ca874e76a55571f6d` | nat melee | 5 → 2 | 5 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Bulette +5 (Sprint26NaturalReachPatch) |
| bulette | `f84a0babcc864630bae225b2019d6bba` | Huge / 3 | KMG_Summoning_Natural_Bulette_Claw2d6 (`d70dfa263eff44f2a547c9f47be0f87e`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 5 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Bulette +5 (Sprint26NaturalReachPatch) |
| cheetah | `6eb29e76792c41c5ab9e3812fe4b66e0` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| cheetah | `6eb29e76792c41c5ab9e3812fe4b66e0` | Medium / 0 | Claw1d3 (`800092a2b9a743b48ae8aeeb5d243dcc`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | leopard | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| cloud-giant | `ff4696c6336141a3b2236cb0a22c86f3` | Huge / 7 | KMG_Summoning_Giant_Sprint29_CloudMorningstar4d6 (`aba8243c429b4e0a938120a20db285ca`) | `b853a77431c842469ab23754a9f917e9` | mfg melee | 15 → 15 | 22 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; absolute 15 kept (SummonSprint29HugeMeleeReach); live 15 → 15 ft |
| cloud-giant | `ff4696c6336141a3b2236cb0a22c86f3` | Huge / 7 | KMG_Summoning_Giant_CloudSlam2d6 (`bb9d1aa6dad74a56ba71b887a31172bb`) | `353f9844e14a449e8177d8a85035509f` | nat melee | 15 → 15 | 22 | yes | — | FIXED (Natural): raw → 5, type 2; absolute 15 kept (SummonSprint29HugeMeleeReach); live 15 → 15 ft |
| cloud-giant | `ff4696c6336141a3b2236cb0a22c86f3` | Huge / 7 | KMG_Summoning_Giant_CloudRock2d6 (`0cb0e2a58ac94475babd99b484e9e47a`) | `c876085795ff450da6418c7fdfca106b` | nat ranged | 140 → 140 | 147 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| constrictor-snake | `f1a2eadf588e4c3b9fb670724d706364` | Medium / 0 | KMG_Summoning_Natural_Bite1d4 (`0e3e5a52e7824ae5ba52f9f5cef30752`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | dire-rat,dog,eagle,fire-beetle,viper,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| crocodile | `ed3fa562802b418ab062a7da622874da` | Large / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | dire-bat,dire-bear,dire-lion,dire-wolf,lion,monitor-lizard,shadow-mastiff | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| crocodile | `ed3fa562802b418ab062a7da622874da` | Large / 0 | KMG_Summoning_Natural_Tail1d12 (`d7ec01bae32a4d9086214f156ce52ecd`) | `4ce435468ebd4364997da8cbd0c66133` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| cyclops | `a85dfadbca3340f4aad99c1cc36e40b3` | Large / 4 | StandardGreataxe (`6efea466862f014469cec6c3f2b85cb7`) | `e8059a8eac62cd74f9171d748a5ae428` | mfg melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| deinonychus | `10a13f87e3664e30b642a3c59cc8c700` | Medium / 0 | KMG_Summoning_Natural_Deinonychus_Talon1d8 (`7bb056a081a4444b804e52508d91f6c8`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| deinonychus | `10a13f87e3664e30b642a3c59cc8c700` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| deinonychus | `10a13f87e3664e30b642a3c59cc8c700` | Medium / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-ape | `5482b49785a3492aa7d29a4ee575cd66` | Large / 4 | KMG_Summoning_Natural_DireApe_Claw1d4 (`bedf467ee92646858b8f0a041bdabcb0`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-ape | `5482b49785a3492aa7d29a4ee575cd66` | Large / 4 | KMG_Summoning_Natural_DireApe_Bite1d6 (`fd2c965ffc764d67b0505cc0da2cc594`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-bat | `d867cb795b5640219a8362661f447697` | Large / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | crocodile,dire-bear,dire-lion,dire-wolf,lion,monitor-lizard,shadow-mastiff | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-bear | `11b15a81cea1498babfdb57af1b53c41` | Large / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | crocodile,dire-bat,dire-lion,dire-wolf,lion,monitor-lizard,shadow-mastiff | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-bear | `11b15a81cea1498babfdb57af1b53c41` | Large / 0 | ClawLarge1d6 (`c76f72a862d168d44838206524366e1c`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | dire-lion,grizzly-bear,owlbear,shadow-demon,succubus,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-boar | `1710475cee544d9d858b05b24fb3ad4c` | Large / 0 | GoreTwoHandedLarge2d6 (`d1f80b5c5c73cc84db7854774850b08c`) | `904e948c3e4c6084e90613bdaf1469c6` | nat melee | 5 → 2 | 2 | no | bison,rhinoceros | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-crocodile | `95bd522bb3a444d49986ba12b7822f10` | Gargantuan / 10 | KMG_Summoning_Natural_Bite3d6 (`2fda8520f73c4431b02db582e16430b7`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 12 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-crocodile | `95bd522bb3a444d49986ba12b7822f10` | Gargantuan / 10 | KMG_Summoning_Natural_Tail4d8 (`99fd2a7c2e0e422d8c3792b4f882e37e`) | `4ce435468ebd4364997da8cbd0c66133` | nat melee | 5 → 2 | 12 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-lion | `56c64aa6765a4c37a1b30c0c5b31427b` | Large / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | crocodile,dire-bat,dire-bear,dire-wolf,lion,monitor-lizard,shadow-mastiff | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-lion | `56c64aa6765a4c37a1b30c0c5b31427b` | Large / 0 | ClawLarge1d6 (`c76f72a862d168d44838206524366e1c`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | dire-bear,grizzly-bear,owlbear,shadow-demon,succubus,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-rat | `e7e43c80489a48e1b562fe6ac11c33ee` | Small / 0 | KMG_Summoning_Natural_Bite1d4 (`0e3e5a52e7824ae5ba52f9f5cef30752`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | constrictor-snake,dog,eagle,fire-beetle,viper,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-tiger | `d15ee151c2274f9f86ab523f111bc3af` | Large / 0 | BiteLarge2d6 (`48647a4517e6512419e937f7a617ea5c`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | tiger | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-tiger | `d15ee151c2274f9f86ab523f111bc3af` | Large / 0 | ClawLarge2d4 (`8afc47748d00b3e4a8aff2787d9ee350`) | `493243bca1fb64f4dbdc56023fc1b0af` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dire-wolf | `d2e7b46ea8994f7085063abac3775142` | Large / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | crocodile,dire-bat,dire-bear,dire-lion,lion,monitor-lizard,shadow-mastiff | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dog | `e8c90cb29374455cb6301e4fa7d1f837` | Small / 0 | KMG_Summoning_Natural_Bite1d4 (`0e3e5a52e7824ae5ba52f9f5cef30752`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | constrictor-snake,dire-rat,eagle,fire-beetle,viper,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| dretch | `717d3fdec4284e42b21efe83bc03eb25` | Small / 0 | KMG_Summoning_OutsiderCelestial_Dretch_Claw1d4 (`ae9975afa8884bd98da9f8788b365acc`) | `758fe1a261934b60ba5445af1efc2008` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| dretch | `717d3fdec4284e42b21efe83bc03eb25` | Small / 0 | KMG_Summoning_OutsiderCelestial_Dretch_Bite1d4 (`266db5aebcae435a86956cd545e7429d`) | `77a01737e1b34fbd9d5c46b007980cd5` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| dust-mephit | `30f6615badf14d21b41d6468d5c93ac3` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| eagle | `7383db28c1d74dce98533ddc257a2e3c` | Small / 0 | KMG_Summoning_Natural_Bite1d4 (`0e3e5a52e7824ae5ba52f9f5cef30752`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | constrictor-snake,dire-rat,dog,fire-beetle,viper,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| eagle | `7383db28c1d74dce98533ddc257a2e3c` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| earth-mephit | `2ddb201a43134893bb626aba291a83eb` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| elder-air-elemental | `1243adb4aecc4eb5ae54b2b54cfe1dec` | Huge / 7 | Unarmed1d3 (`e5ea5042194712040a3b2944e6944d10`) | `fcca8e6b85d19b14786ba1ab553e23ad` | nat melee | 5 → 2 | 9 | no | greater-air-elemental,huge-air-elemental,large-air-elemental,medium-air-elemental,small-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| elder-air-elemental | `1243adb4aecc4eb5ae54b2b54cfe1dec` | Huge / 7 | AirElementalSlam_Greater (`bdb489e16c1c839449420aa8c2c8fbc5`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | greater-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| elder-earth-elemental | `2bdfdec402384e689351bad9a0d2d8af` | Huge / 7 | EarthElementalSlam_Greater (`e4779dd883b250941861760b80d1d744`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | greater-earth-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| elder-fire-elemental | `20116595e2554c758a29b3b32553f0e5` | Huge / 7 | FireElementalSlam_Greater (`9212a9d45b8eea140bf49fc4ec5d0791`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | greater-fire-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| elder-water-elemental | `54acc1934a55479ca43e826ed6e7d14c` | Huge / 7 | WaterElementalSlam_Elder (`d18a41599d0a9f64eb891e8e4e99c54f`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| elephant | `9dd8544097234f05bcd35d400d91b510` | Huge / 7 | MastodonGore (`de42c58801037b84c9d992634ddd7220`) | `352b738b4ebfd554b909852e7f3b690a` | nat melee | 5 → 2 | 9 | no | mastodon,woolly-rhinoceros | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Elephant absolute 10 (ElephantNaturalReachPatch) |
| elephant | `9dd8544097234f05bcd35d400d91b510` | Huge / 7 | MastodonSlam (`c2ce7bc3559b2024ea91ddf5bb321f0a`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | mastodon | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Elephant absolute 10 (ElephantNaturalReachPatch) |
| erinyes-devil | `bf08d0f66ffc4ed78dd40e5f7e68ae9d` | Medium / 0 | WeaponEmptyHand (`20375b5a0c9243d45966bd72c690ab74`) | `a8b38f6b734daa44087ec0ec2e80c1cd` | nat melee | 5 → 2 | 2 | no | bralani-azata,ghaele-azata | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| erinyes-devil | `bf08d0f66ffc4ed78dd40e5f7e68ae9d` | Medium / 0 | CompositeLongbowFlamingPlus1 (`a5463ff3013db0d409b7cf9dc10ff606`) | `1ac79088a7e5dde46966636a3ac71c35` | mfg ranged | 50 → 50 | 50 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| erinyes-devil | `bf08d0f66ffc4ed78dd40e5f7e68ae9d` | Medium / 0 | LongswordPlus1 (`03d706655c07d804cb9d5a5583f9aec5`) | `d56c44bc9eb10204c8b386a02c7eed21` | mfg melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| ettin | `2481bc73abd64ef0ae581b48438b5404` | Large / 4 | KMG_Summoning_Giant_Sprint29_EttinMainFlail2d6 (`940159c86d414820889994ca9c7fea42`) | `31e2af71f723426499d4dc4874332823` | mfg melee | 10 → 6 | 10 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| ettin | `2481bc73abd64ef0ae581b48438b5404` | Large / 4 | KMG_Summoning_Giant_Sprint29_EttinMainJavelin1d8 (`c4299b0281144e528037a3cb7c31611e`) | `589604bc6e654533b3d78b4bc66ff3cd` | mfg ranged | 30 → 30 | 34 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| ettin | `2481bc73abd64ef0ae581b48438b5404` | Large / 4 | KMG_Summoning_Giant_Sprint29_EttinOffFlail2d6 (`35fa914ef66a4b69991e088215bf5854`) | `31e2af71f723426499d4dc4874332823` | mfg melee | 10 → 6 | 10 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| ettin | `2481bc73abd64ef0ae581b48438b5404` | Large / 4 | KMG_Summoning_Giant_Sprint29_EttinOffJavelin1d8 (`d857fc4ebda548439fa6b06a0cba02f2`) | `589604bc6e654533b3d78b4bc66ff3cd` | mfg ranged | 30 → 30 | 34 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| fire-beetle | `714b04102eae4cd2be200a8f09f04bc3` | Small / 0 | KMG_Summoning_Natural_Bite1d4 (`0e3e5a52e7824ae5ba52f9f5cef30752`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | constrictor-snake,dire-rat,dog,eagle,viper,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| fire-giant | `65481592185641c99c502e21ba9a5fa2` | Large / 4 | KMG_Summoning_Giant_Greatsword3d6 (`d33330230bc444a98c7f7b0b81778f1d`) | `41cce4f929b44a57a68dc6c02b037fc0` | mfg melee | 10 → 6 | 10 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| fire-giant | `65481592185641c99c502e21ba9a5fa2` | Large / 4 | KMG_Summoning_Giant_Slam1d8 (`fe074f0010824f9cb2ab6864469e1852`) | `a51e01183f844ce58be689fc075aa716` | nat melee | 10 → 6 | 10 | yes | hill-giant,stone-giant | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| fire-giant | `65481592185641c99c502e21ba9a5fa2` | Large / 4 | KMG_Summoning_Giant_FireRock1d8 (`80f42e4d54104df681f39637dfc43f25`) | `28981505774b459dbf0e9871a9bd2dd3` | nat ranged | 120 → 120 | 124 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| fire-mephit | `a841652d2c7242d6849a9b74982710f8` | Small / 0 | RayItem (`f6ef95b1f7bb52b408a5b345a330ffe8`) | `1d39a22f206840e40b2255fc0175b8d0` | mfg ranged | 50 → 50 | 50 | no | barbed-devil,bebelith,giant-spider,ooze-mephit,water-mephit | UNCHANGED: ranged, not a dead-zone defect |
| fire-mephit | `a841652d2c7242d6849a9b74982710f8` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| ghaele-azata | `e7eccabdc8cd474d86475586792c03c7` | Medium / 0 | WeaponEmptyHand (`20375b5a0c9243d45966bd72c690ab74`) | `a8b38f6b734daa44087ec0ec2e80c1cd` | nat melee | 5 → 2 | 2 | no | bralani-azata,erinyes-devil | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| ghaele-azata | `e7eccabdc8cd474d86475586792c03c7` | Medium / 0 | LongswordGhaeleAzata (`e08e3e308e08a734c9cc6e80902e0efd`) | `d56c44bc9eb10204c8b386a02c7eed21` | mfg melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-ant-drone | `9387ef0ca1cd403a985893a85a6c7806` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-ant-drone | `9387ef0ca1cd403a985893a85a6c7806` | Medium / 0 | KMG_Summoning_Natural_AntSting1d4 (`685b63071d864fff9ccc25f2d64b8755`) | `34c90ab7a0d6e05449f081c122ca219f` | nat melee | 5 → 2 | 2 | no | giant-ant-soldier | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-ant-soldier | `ef2cd2abd3254cb2a90952253add918a` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-ant-soldier | `ef2cd2abd3254cb2a90952253add918a` | Medium / 0 | KMG_Summoning_Natural_AntSting1d4 (`685b63071d864fff9ccc25f2d64b8755`) | `34c90ab7a0d6e05449f081c122ca219f` | nat melee | 5 → 2 | 2 | no | giant-ant-drone | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-ant-worker | `9db94712f7b540679b8ff650dbaf746a` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-centipede | `baf9e8f829e9410db8f3d200bb62a2c6` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-crab | `5965d242ffe1409d98e38f2793543aad` | Medium / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-flytrap | `287490e71bed46bd92adef60489ecb4c` | Huge / 7 | BiteLarge1d8 (`ec35ef997ed5a984280e1a6d87ae80a8`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 9 | no | shadow-demon | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-frog | `ca53b4bba8f04497acf3d36440a6e942` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-spider,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-scorpion | `4b5ec54e8e7a4e57b838541006affd3d` | Large / 4 | KMG_Summoning_Natural_GiantScorpion_Claw1d6 (`6e24ee48cc0545f3ba19b364c3b74181`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-scorpion | `4b5ec54e8e7a4e57b838541006affd3d` | Large / 4 | KMG_Summoning_Natural_GiantScorpion_Sting1d6 (`6dce74d083d64612b5753022b4d8921a`) | `34c90ab7a0d6e05449f081c122ca219f` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-spider | `4a3cd49e751448c8b8836485b262fdf1` | Medium / 0 | RayItem (`f6ef95b1f7bb52b408a5b345a330ffe8`) | `1d39a22f206840e40b2255fc0175b8d0` | mfg ranged | 50 → 50 | 50 | no | barbed-devil,bebelith,fire-mephit,ooze-mephit,water-mephit | UNCHANGED: ranged, not a dead-zone defect |
| giant-spider | `4a3cd49e751448c8b8836485b262fdf1` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,goblin-dog,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-stag-beetle | `fc294cc691284525814324ac5bc5bf75` | Large / 0 | KMG_Summoning_Natural_Bite2d8 (`c19d1025fe2b47769c93a3b76d0c052c`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | roc | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| giant-wasp | `156a063aaec7449992d2021d53341085` | Large / 4 | KMG_Summoning_Natural_WaspSting1d8 (`0006090d43334fe1becaed56d09987b3`) | `34c90ab7a0d6e05449f081c122ca219f` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| girallon | `b9187f2a07a74c84b07d5a34280753a6` | Large / 4 | KMG_Summoning_Natural_Girallon_Claw1d4 (`909529bb5abf4ef695f24c4f96f3bb0e`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| girallon | `b9187f2a07a74c84b07d5a34280753a6` | Large / 4 | KMG_Summoning_Natural_Girallon_Bite1d6 (`d14e299c7630424e987bc5251d1a167c`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| glabrezu | `3824e7589ae548f79a3eb8369f50359f` | Huge / 7 | KMG_Summoning_Demon_Glabrezu_Bite (`d44165bbdcad4ebf8ae6f3b5454e86cf`) | `e17d62a9c3b64bfab677d576d94c63f7` | nat melee | 15 → 15 | 22 | yes | — | FIXED (Natural): raw → 5, type 2; +13 SummonReachRange4; live 22 → 22 ft |
| glabrezu | `3824e7589ae548f79a3eb8369f50359f` | Huge / 7 | KMG_Summoning_Demon_Glabrezu_Pincer (`7af8a097a3594b53b8b40c05881146db`) | `2097b105f5bd4d6eb8c0a7c5c9e6ac49` | nat melee | 15 → 15 | 22 | yes | — | FIXED (Natural): raw → 5, type 2; +13 SummonReachRange4; live 22 → 22 ft |
| glabrezu | `3824e7589ae548f79a3eb8369f50359f` | Huge / 7 | KMG_Summoning_Demon_Glabrezu_Claw (`d984a1fbac854b0bb1b0f1e0ca70a606`) | `21e5bddd0d7c49d79a3d2985293c550d` | nat melee | 15 → 15 | 22 | yes | — | FIXED (Natural): raw → 5, type 2; +13 SummonReachRange4; live 22 → 22 ft |
| goblin-dog | `f1584066792a436fa3a5ba0b3731b481` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,grizzly-bear,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| gorgon | `ce2fc58942b54e558208ea67d595cb7b` | Large / 0 | KMG_Summoning_Natural_Gorgon_Gore2d8 (`6fa1a7d22d31434b85708d7ba7735649`) | `904e948c3e4c6084e90613bdaf1469c6` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Gorgon +3 (Sprint26NaturalReachPatch) |
| gorgon | `ce2fc58942b54e558208ea67d595cb7b` | Large / 0 | KMG_Summoning_Natural_Gorgon_Hoof1d6 (`fab6e52557c5426e8e09fbd714443c2c`) | `ad298a0ee3ca1ba419d0c973d1a905f2` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Gorgon +3 (Sprint26NaturalReachPatch) |
| greater-air-elemental | `c35e1370ca28417098d6a3b9835b0ba5` | Huge / 7 | Unarmed1d3 (`e5ea5042194712040a3b2944e6944d10`) | `fcca8e6b85d19b14786ba1ab553e23ad` | nat melee | 5 → 2 | 9 | no | elder-air-elemental,huge-air-elemental,large-air-elemental,medium-air-elemental,small-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| greater-air-elemental | `c35e1370ca28417098d6a3b9835b0ba5` | Huge / 7 | AirElementalSlam_Greater (`bdb489e16c1c839449420aa8c2c8fbc5`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | elder-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| greater-earth-elemental | `17c4fcc106df43af980fcea7a0877e04` | Huge / 7 | EarthElementalSlam_Greater (`e4779dd883b250941861760b80d1d744`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | elder-earth-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| greater-fire-elemental | `7c7c5326601c4b2e8237c33969c47914` | Huge / 7 | FireElementalSlam_Greater (`9212a9d45b8eea140bf49fc4ec5d0791`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | elder-fire-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| greater-water-elemental | `72d45b40b6764ab090a3dff639aa678a` | Huge / 7 | WaterElementalSlam_Greater (`70935407fb448aa45a0f1ba6781f8926`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| griffon | `4b55d1f8da5e4a3ea23f9a95191f8737` | Large / 0 | KMG_Summoning_Natural_Griffon_Bite1d6 (`67549c5f756c495a8592ab27a019fec8`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Griffon +3 (GriffonReachPatch) |
| griffon | `4b55d1f8da5e4a3ea23f9a95191f8737` | Large / 0 | KMG_Summoning_Natural_Griffon_Talon1d6 (`130a92b8f9d64a46908cf05258fbb2b0`) | `493243bca1fb64f4dbdc56023fc1b0af` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Griffon +3 (GriffonReachPatch) |
| griffon | `4b55d1f8da5e4a3ea23f9a95191f8737` | Large / 0 | KMG_Summoning_Natural_Griffon_Rake1d4 (`4807a234c116416988cd388e5a923776`) | `493243bca1fb64f4dbdc56023fc1b0af` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Griffon +3 (GriffonReachPatch) |
| grizzly-bear | `f7370368039e41ba8f88e8218cfb39d0` | Large / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,hyena,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| grizzly-bear | `f7370368039e41ba8f88e8218cfb39d0` | Large / 0 | ClawLarge1d6 (`c76f72a862d168d44838206524366e1c`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | dire-bear,dire-lion,owlbear,shadow-demon,succubus,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| hell-hound | `f8c0664ec7204643be3ab7a1ac16a391` | Medium / 0 | HellhouneBite (`711fded1182d7564895dd4f1d4ddb728`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| hezrou | `0c97e35a3e8e4d2db937a23775ea13ac` | Large / 4 | KMG_Summoning_Infernal_Hezrou_Bite (`26fb64d30ac242bc9200d582b74dd9c0`) | `da9a7793ee894d4892e1e889e14eca3c` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| hezrou | `0c97e35a3e8e4d2db937a23775ea13ac` | Large / 4 | KMG_Summoning_Infernal_Hezrou_Claw (`bb889c692fad42fd87857b71dc75fdbc`) | `18a7ac2d117146f6836670937df9bb04` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| hill-giant | `58770207fd8c4084a0d9addbac591616` | Large / 4 | KMG_Summoning_Giant_Greatclub2d8 (`2b821e28b95d40f197f5fb847008b93f`) | `81f343d50f774520936111cd95d4d677` | mfg melee | 10 → 6 | 10 | yes | stone-giant | FIXED (OrdinaryManufactured): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| hill-giant | `58770207fd8c4084a0d9addbac591616` | Large / 4 | KMG_Summoning_Giant_Slam1d8 (`fe074f0010824f9cb2ab6864469e1852`) | `a51e01183f844ce58be689fc075aa716` | nat melee | 10 → 6 | 10 | yes | fire-giant,stone-giant | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| hill-giant | `58770207fd8c4084a0d9addbac591616` | Large / 4 | KMG_Summoning_Giant_HillRock1d8 (`a32cbf513a024dd5b91f140bc5dfa91d`) | `6925ed3e113540649e9f220154d846e6` | nat ranged | 120 → 120 | 124 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| horse | `629fa53bf40d4b4b9828f57cf81194e9` | Large / 0 | Hoof1d4 (`b0e472a49ff2a294f93faa3ab757a4a5`) | `ad298a0ee3ca1ba419d0c973d1a905f2` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| hound-archon | `4685038956fc47c7b18d5acf1d7d8d7b` | Medium / 0 | KMG_Summoning_OutsiderCelestial_HoundArchon_Slam1d4 (`ca5f1f56746b4473b3fefdf1e3edbb5e`) | `55d2b80ecbd64ae5a9d203c34aadbbc4` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| hound-archon | `4685038956fc47c7b18d5acf1d7d8d7b` | Medium / 0 | KMG_Summoning_OutsiderCelestial_HoundArchon_Greatsword2d6 (`99fa49115f9c48228e13ca9f2879e6e0`) | `d099d40bb7ae46d68d1ffde41d8e56e6` | mfg melee | 9 → 5 | 5 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| hound-archon | `4685038956fc47c7b18d5acf1d7d8d7b` | Medium / 0 | KMG_Summoning_OutsiderCelestial_HoundArchon_Bite1d8 (`b7c358cbe55a45fda087f4fefd968122`) | `270965b8a99d42f58ce68fe83ec59f24` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| huge-air-elemental | `f6b8223178ca4ec7b9b8aac61f1fd0f8` | Huge / 7 | Unarmed1d3 (`e5ea5042194712040a3b2944e6944d10`) | `fcca8e6b85d19b14786ba1ab553e23ad` | nat melee | 5 → 2 | 9 | no | elder-air-elemental,greater-air-elemental,large-air-elemental,medium-air-elemental,small-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| huge-air-elemental | `f6b8223178ca4ec7b9b8aac61f1fd0f8` | Huge / 7 | AirElementalSlam_Huge (`22f246aaa4ca3f246a70e546c50c8c94`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| huge-earth-elemental | `f8ba7baab64e42b2951d52e227a20b4f` | Huge / 7 | EarthElementalSlam_Huge (`1f0e80f7b172bab4f9f485335c25ea12`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| huge-fire-elemental | `fe3b06c44a2d4d36be8f549b14042f73` | Huge / 7 | FireElementalSlam_Huge (`1ae868cfefc2dbf4d8fa2efe217b2f5c`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| huge-water-elemental | `974d58335c9149b687140d09ef8e47a2` | Huge / 7 | WaterElementalSlam_Huge (`fbd1b237e60c68145afa01f80966f597`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| hyena | `ec12fab8be5c412d8ee824d15a6621d0` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,leopard,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| ice-devil | `6b5b789a08584fab84a00e7da42e1d2c` | Large / 4 | KMG_Summoning_Infernal_IceDevil_Spear (`a401bfb28e394f06a2fce8b89e91ebd8`) | `573c1e9bffb2470293c276999f94a408` | mfg melee | 10 → 6 | 10 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft; native acceptance HOST-BLOCKED (HOWARD-LAPTOP) |
| ice-devil | `6b5b789a08584fab84a00e7da42e1d2c` | Large / 4 | KMG_Summoning_Infernal_IceDevil_Bite (`d324d22032f14392aa44aad0472cc0de`) | `92dada2bf76947eca2cee0dc494afdfc` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft; native acceptance HOST-BLOCKED (HOWARD-LAPTOP) |
| ice-devil | `6b5b789a08584fab84a00e7da42e1d2c` | Large / 4 | KMG_Summoning_Infernal_IceDevil_Tail (`7f6ab83598e047649f3f47ae813ff8b0`) | `30a4a845d51a4d91b2cfdeee35d50bee` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft; native acceptance HOST-BLOCKED (HOWARD-LAPTOP) |
| ice-mephit | `b37cdd78c8cc43be8799e9ffd6953ba2` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| invisible-stalker | `3cd7b4f2c65b4d35929dd4d969dfaa41` | Medium / 0 | AirElementalSlam_Large (`72aa06bd4e7a8fa4db8a20d1b5f1a103`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 2 | no | large-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| kyton-chain-devil | `96045f5c18ef4f4396a6c466a2be1a92` | Medium / 0 | KMG_Summoning_Infernal_KytonChainDevil_Chain (`00d62f48e15e4b8ca19f480fd8dd07b1`) | `aa856db3834e472db6d8190b0081d38f` | nat melee | 10 → 6 | 6 | yes | — | FIXED (NaturalExtended): raw → 5, type 2; +4 SummonReachRange4 (+4 existing InfernalWeaponReach34); live 10 → 10 ft |
| lantern-archon | `02f8e9c6c91549deaded9ef667399449` | Small / 0 | (no weapon referenced) | | | | | | | VERIFIED: no weapon on the blueprint |
| large-air-elemental | `6fce34709ee54f5a85fd75ea5c6d30be` | Large / 4 | Unarmed1d3 (`e5ea5042194712040a3b2944e6944d10`) | `fcca8e6b85d19b14786ba1ab553e23ad` | nat melee | 5 → 2 | 6 | no | elder-air-elemental,greater-air-elemental,huge-air-elemental,medium-air-elemental,small-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| large-air-elemental | `6fce34709ee54f5a85fd75ea5c6d30be` | Large / 4 | AirElementalSlam_Large (`72aa06bd4e7a8fa4db8a20d1b5f1a103`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 6 | no | invisible-stalker | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| large-earth-elemental | `4fee8d80c1944b1f96d905aaf15c61b3` | Large / 4 | EarthElementalSlam_Large (`178879ca4f7d75a4f9a3fc544b40d10f`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| large-fire-elemental | `e9371f3a9bdd4f8780db58cdefb95292` | Large / 4 | FireElementalSlam_Large (`1ac38b9b7d51dca4292a78e82a3aa59f`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| large-water-elemental | `2c6483c5025d4dcba441cc4c805fa784` | Large / 4 | WaterElementalSlam_Large (`f250818c2263494478470c9992dc138d`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| lemure | `80fd27e965b943b5b307a8c0a45cfb96` | Medium / 0 | KMG_Summoning_OutsiderCelestial_Lemure_Claw1d4 (`780c5819371c4b5b88d72d12fc43a9f2`) | `dfcd9e2b2dd14e1c81b052aaf21fac96` | nat melee | 9 → 5 | 5 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| leopard | `f36aad1d463444c69950f219457b894f` | Medium / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,owlbear | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| leopard | `f36aad1d463444c69950f219457b894f` | Medium / 0 | Claw1d3 (`800092a2b9a743b48ae8aeeb5d243dcc`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | cheetah | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| lillend-azata | `71a8adf916b4409680023e853d28583b` | Large / 4 | KMG_Summoning_FeyCelestial_LillendAzata_Longsword (`f2a606d19b0240abb62255da5e4e1973`) | `7e77d8b4a4284e04a5efb45a99036ed7` | mfg melee | 10 → 6 | 10 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| lillend-azata | `71a8adf916b4409680023e853d28583b` | Large / 4 | KMG_Summoning_FeyCelestial_LillendAzata_Tail (`50d93d8a75d34b3787183363f7ec25c9`) | `9afa3017d7794fb1b477e2236dbb0568` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| lion | `14c7cbb0f32f4e30bbefaeaccba10269` | Large / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | crocodile,dire-bat,dire-bear,dire-lion,dire-wolf,monitor-lizard,shadow-mastiff | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| lion | `14c7cbb0f32f4e30bbefaeaccba10269` | Large / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,magma-mephit,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| magma-mephit | `ce9551f1413d4fdcb59941734d4546b1` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,ooze-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| mastodon | `e129ffc2768d47c5bee61bb99b0c8703` | Huge / 7 | MastodonGore (`de42c58801037b84c9d992634ddd7220`) | `352b738b4ebfd554b909852e7f3b690a` | nat melee | 5 → 2 | 9 | no | elephant,woolly-rhinoceros | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| mastodon | `e129ffc2768d47c5bee61bb99b0c8703` | Huge / 7 | MastodonSlam (`c2ce7bc3559b2024ea91ddf5bb321f0a`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 9 | no | elephant | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| medium-air-elemental | `d8e45b6e036b4723a0fc2c1c074fcaea` | Medium / 0 | Unarmed1d3 (`e5ea5042194712040a3b2944e6944d10`) | `fcca8e6b85d19b14786ba1ab553e23ad` | nat melee | 5 → 2 | 2 | no | elder-air-elemental,greater-air-elemental,huge-air-elemental,large-air-elemental,small-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| medium-air-elemental | `d8e45b6e036b4723a0fc2c1c074fcaea` | Medium / 0 | AirElementalSlam_Medium (`bfbe379b358adde4ebf599aadbec8fbc`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| medium-earth-elemental | `fc989a8d68e24250bb613b8c7ac20238` | Medium / 0 | EarthElementalSlam_Medium (`bbdf5d550dc406640a77c5d2a05244ca`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| medium-fire-elemental | `c75e8614029540dda8a985c7cba4ac00` | Medium / 0 | FireElementalSlam_Medium (`e7de2f7234f90dd4d85c1c5bdd904cb4`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| medium-water-elemental | `d1889a6ad861465cacd64508669ef7da` | Medium / 0 | WaterElementalSlam_Medium (`d9fc638aa709e204587b032c4ab129d1`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| monitor-lizard | `10b148d216364b509cd2c665a47b2950` | Medium / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | crocodile,dire-bat,dire-bear,dire-lion,dire-wolf,lion,shadow-mastiff | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| nalfeshnee | `9930f78084f0fe78d1de849aed58b753` | Huge / 7 | KMG_Summoning_Demon_Nalfeshnee_Bite (`e061d98c5542b5625a83daaef4c86076`) | `a642301b1d2cea009d2bfeb726b60a87` | nat melee | 15 → 15 | 22 | yes | — | FIXED (Natural): raw → 5, type 2; +13 SummonReachRange4; live 22 → 22 ft |
| nalfeshnee | `9930f78084f0fe78d1de849aed58b753` | Huge / 7 | KMG_Summoning_Demon_Nalfeshnee_Claw (`4038f0c02ba5614ee0aa706053631d55`) | `a542beeff43d04f2c676070581fd7865` | nat melee | 15 → 15 | 22 | yes | — | FIXED (Natural): raw → 5, type 2; +13 SummonReachRange4; live 22 → 22 ft |
| ooze-mephit | `7cbf19a6c83f4b8e849d17b8a54e6d51` | Small / 0 | RayItem (`f6ef95b1f7bb52b408a5b345a330ffe8`) | `1d39a22f206840e40b2255fc0175b8d0` | mfg ranged | 50 → 50 | 50 | no | barbed-devil,bebelith,fire-mephit,giant-spider,water-mephit | UNCHANGED: ranged, not a dead-zone defect |
| ooze-mephit | `7cbf19a6c83f4b8e849d17b8a54e6d51` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,salt-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| owlbear | `7bde6f94222c47f687ed68c752cb24cb` | Large / 0 | Bite1d6 (`a000716f88c969c499a535dadcf09286`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | cheetah,deinonychus,giant-ant-drone,giant-ant-soldier,giant-ant-worker,giant-centipede,giant-frog,giant-spider,goblin-dog,grizzly-bear,hyena,leopard | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| owlbear | `7bde6f94222c47f687ed68c752cb24cb` | Large / 0 | ClawLarge1d6 (`c76f72a862d168d44838206524366e1c`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | dire-bear,dire-lion,grizzly-bear,shadow-demon,succubus,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| pixie | `396881ade24e4ddba188dc2e7ff481f9` | Small / 0 | KMG_Summoning_Special_Pixie_SleepBow (`0a1d8ac4be724595aa952471a9491975`) | `18c39948e14e4abcbb4f029372ea91ff` | nat ranged | 50 → 50 | 50 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| poisonous-frog | `e1a8e5e206154dd48b6aca1d4262e8e7` | Tiny / 0 | KMG_Summoning_Natural_Bite1 (`f120128f4b7c4fbb8a3b19756bdb6300`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| pony | `67a661d7b39e44bc811ad9ff59120110` | Medium / 0 | SmallHoof1d3 (`085547b82eded104ba7e1870dd0563bf`) | `ad298a0ee3ca1ba419d0c973d1a905f2` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| pteranodon | `c9a94142c9164ab793f7a06ae3fdcf56` | Large / 4 | BiteHuge2d6 (`d2f99947db522e24293a7ec4eded453f`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 6 | no | bebelith | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| purple-worm | `8739fe771e5b4f3fa3acbb10dd969892` | Gargantuan / 10 | PurpleWormBite (`7e4b9b41a9358264d9e3c69c183ca0a2`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 12 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| purple-worm | `8739fe771e5b4f3fa3acbb10dd969892` | Gargantuan / 10 | PurpleWormSting (`287cd06241fdaf8408410b226f744093`) | `34c90ab7a0d6e05449f081c122ca219f` | nat melee | 5 → 2 | 12 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| rhinoceros | `e0d2c0f0589e467792aff319fdb2b6f9` | Large / 0 | GoreTwoHandedLarge2d6 (`d1f80b5c5c73cc84db7854774850b08c`) | `904e948c3e4c6084e90613bdaf1469c6` | nat melee | 5 → 2 | 2 | no | bison,dire-boar | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| roc | `439e955cb0fd41daafd0478d3641615a` | Gargantuan / 10 | KMG_Summoning_Natural_Bite2d8 (`c19d1025fe2b47769c93a3b76d0c052c`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 12 | no | giant-stag-beetle | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| roc | `439e955cb0fd41daafd0478d3641615a` | Gargantuan / 10 | KMG_Summoning_Natural_Talon2d6 (`8a3741a7598147baa08de552565635ad`) | `493243bca1fb64f4dbdc56023fc1b0af` | nat melee | 5 → 2 | 12 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| salamander | `f8fb103168d74b4c93182437e5d2b4e4` | Medium / 0 | KMG_Summoning_Special_Salamander_Spear (`99394d453c6f425f84d4b92f7a8deea0`) | `7393ad3dde0c4385b73189b44ecb758f` | mfg melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| salamander | `f8fb103168d74b4c93182437e5d2b4e4` | Medium / 0 | KMG_Summoning_Special_Salamander_Tail (`93e097b8d3db42d3a37656502899e1a9`) | `c023eeab7aca4f9bbc0709dfb6485f1e` | nat melee | 10 → 6 | 6 | yes | — | FIXED (NaturalExtended): raw → 5, type 2; +4 SummonReachRange4; live 6 → 6 ft |
| salt-mephit | `8c8fe6eb536b450a9a2c10b8cbef9fbe` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,steam-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| satyr | `b0dab767cb2948ca9f918bf7d7c3dbb4` | Medium / 0 | KMG_Summoning_FeyCelestial_Satyr_Dagger (`25698949bdea41d2bcbe13ad8783aa4d`) | `b2ba7766959344fb8536d4a3f7281441` | mfg melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| satyr | `b0dab767cb2948ca9f918bf7d7c3dbb4` | Medium / 0 | KMG_Summoning_FeyCelestial_Satyr_Horns (`fe9b4722730744dfadbf0ae3b10c07f9`) | `261b62148fec4ebf909792ab4976683c` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| satyr | `b0dab767cb2948ca9f918bf7d7c3dbb4` | Medium / 0 | StandardShortbow (`6d40d84e239bdf345b349ff52e3c00a9`) | `99ce02fb54639b5439d07c99c55b8542` | mfg ranged | 40 → 40 | 40 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| shadow-demon | `627c1841a5eb4e32b1a94c0f43ec8a60` | Medium / 0 | ClawLarge1d6 (`c76f72a862d168d44838206524366e1c`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | dire-bear,dire-lion,grizzly-bear,owlbear,succubus,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| shadow-demon | `627c1841a5eb4e32b1a94c0f43ec8a60` | Medium / 0 | BiteLarge1d8 (`ec35ef997ed5a984280e1a6d87ae80a8`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | giant-flytrap | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| shadow-mastiff | `c0c955211eda4de18df0dcb4870eede2` | Medium / 0 | BiteDragonLarge1d8 (`c988aa874d11ff84d873508ddc9b928f`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | crocodile,dire-bat,dire-bear,dire-lion,dire-wolf,lion,monitor-lizard | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| shadow-mastiff | `c0c955211eda4de18df0dcb4870eede2` | Medium / 0 | KMG_Summoning_Natural_Tail1d6 (`396d94b237eb449e9ecae03a3f5bd720`) | `4ce435468ebd4364997da8cbd0c66133` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| shambling-mound | `c0680a67c42f4d17b3b55c011a8f9c56` | Large / 4 | SlamGargantuan2d6 (`27eee74857c42db499b3a6b20cfa6211`) | `f18cbcb39a1b35643a8d129b1ec4e716` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| small-air-elemental | `5184af966fa84c6ebf42f6f941d5d533` | Small / 0 | Unarmed1d3 (`e5ea5042194712040a3b2944e6944d10`) | `fcca8e6b85d19b14786ba1ab553e23ad` | nat melee | 5 → 2 | 2 | no | elder-air-elemental,greater-air-elemental,huge-air-elemental,large-air-elemental,medium-air-elemental | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| small-air-elemental | `5184af966fa84c6ebf42f6f941d5d533` | Small / 0 | AirElementalSlam_Small (`df3c845e099dc4746a70b05628443efa`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| small-earth-elemental | `1830865b016a486db1de7f99e8b9f997` | Small / 0 | EarthElementalSlam_Small (`f217709b11a0f8b439e5d658842e587c`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| small-fire-elemental | `6a95de017d0949708fd32548207f8195` | Small / 0 | FireElementalSlam_Small (`b3b7ca8bb05b8e14c9e7e5cc7e78de8c`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| small-water-elemental | `daa6a4f74317475da5a42c83082b7cdd` | Small / 0 | WaterElementalSlam_Small (`db22244b1a3811944ac1452092195f0a`) | `b6a50705eb5dfab4fafd577020f49c5e` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| steam-mephit | `047f68ee53554d8ab1702de189ecd85d` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,water-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| stegosaurus | `39cab5be4dc148339f4ce8b0157bd716` | Huge / 10 | KMG_Summoning_Natural_Stegosaurus_Tail4d6 (`96b5e8692e3949bcba63b57507a5075f`) | `deb29b6a26c049a39daf3ec6cb3f77f1` | nat melee | 9 → 5 | 15 | yes | — | FIXED (Natural): raw → 5, type 2; +3 SummonReachRange4; live 15 → 15 ft |
| stirge | `c49f994ea2a840d98108b2848781774c` | Tiny / 0 | KMG_Summoning_Natural_StirgeTouch (`ebf16ae835ea40c7afaaf90277c013bf`) | `ba9cd5a303b089f458aa9f07aa10ea10` | mfg melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| stone-giant | `956501547dd54479b329a315aab07a05` | Large / 4 | KMG_Summoning_Giant_Greatclub2d8 (`2b821e28b95d40f197f5fb847008b93f`) | `81f343d50f774520936111cd95d4d677` | mfg melee | 10 → 6 | 10 | yes | hill-giant | FIXED (OrdinaryManufactured): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| stone-giant | `956501547dd54479b329a315aab07a05` | Large / 4 | KMG_Summoning_Giant_Slam1d8 (`fe074f0010824f9cb2ab6864469e1852`) | `a51e01183f844ce58be689fc075aa716` | nat melee | 10 → 6 | 10 | yes | fire-giant,hill-giant | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| stone-giant | `956501547dd54479b329a315aab07a05` | Large / 4 | KMG_Summoning_Giant_StoneRock1d8 (`20c088bbd36c4e43a7b26acde82345a1`) | `c711974fd2f8479a885e34c0b3711880` | nat ranged | 180 → 180 | 184 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| storm-giant | `4b1143140c614a0aaaa1118c8a44c9ae` | Huge / 7 | KMG_Summoning_Giant_Sprint29_StormGreatsword4d6 (`240530e94b9647f09ca18dcb2e70b15a`) | `51f064c26ee848b3b89dbbcbc4dbb3fb` | mfg melee | 15 → 15 | 22 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; absolute 15 kept (SummonSprint29HugeMeleeReach); live 15 → 15 ft |
| storm-giant | `4b1143140c614a0aaaa1118c8a44c9ae` | Huge / 7 | KMG_Summoning_Giant_Sprint29_StormSlam2d6 (`12bff2b66e674023a75057d7face1ca6`) | `31e80b8a20b64680a214ce0847858250` | nat melee | 15 → 15 | 22 | yes | — | FIXED (Natural): raw → 5, type 2; absolute 15 kept (SummonSprint29HugeMeleeReach); live 15 → 15 ft |
| storm-giant | `4b1143140c614a0aaaa1118c8a44c9ae` | Huge / 7 | KMG_Summoning_Giant_Sprint29_StormCompositeBow3d6 (`fec864af23094a3cab1eecdbbf7355fe`) | `a7151fef3287423994c2f4821c88c455` | mfg ranged | 50 → 50 | 57 | no | — | UNCHANGED: ranged, not a dead-zone defect |
| succubus | `0c908145873f4b67a188397ca5f46da1` | Medium / 0 | ClawLarge1d6 (`c76f72a862d168d44838206524366e1c`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | dire-bear,dire-lion,grizzly-bear,owlbear,shadow-demon,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| tiger | `5182125341e34ae98491eaf991bc0ef3` | Large / 4 | BiteLarge2d6 (`48647a4517e6512419e937f7a617ea5c`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 6 | no | dire-tiger | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| tiger | `5182125341e34ae98491eaf991bc0ef3` | Large / 4 | KMG_Summoning_Natural_Claw1d8 (`422318f491354d37aba69dece0004a11`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 6 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| triceratops | `ce540cd4b3b74861a4a91458d742a692` | Huge / 10 | KMG_Summoning_Natural_Triceratops_Gore2d10 (`ffe6a2d9dbe4451abf1855addda1af15`) | `352b738b4ebfd554b909852e7f3b690a` | nat melee | 5 → 2 | 12 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon; existing Triceratops exact gore correction (TriceratopsGoreReachPatch) |
| trumpet-archon | `b6359294f13a4adab626d14c5ededd50` | Medium / 0 | KMG_Summoning_OutsiderCelestial_TrumpetArchon_Greatsword2d6 (`1182d9ef39c443f5b4ffda85ae1e57b2`) | `d3e0c0058deb480eb1dad4b6d5352fdf` | mfg melee | 9 → 5 | 5 | yes | — | FIXED (OrdinaryManufactured): raw → 5, type 2; +3 SummonReachRange4; live 5 → 5 ft |
| tyrannosaurus | `04b1a75a76544a9581ee832045f8227a` | Gargantuan / 15 | KMG_Summoning_Natural_Tyrannosaurus_Bite4d6 (`cfb6ea9c9c3b4405bb001f00159e8370`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 17 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| viper | `d8be82543ab64dc988c33e9f13608bad` | Medium / 0 | KMG_Summoning_Natural_Bite1d4 (`0e3e5a52e7824ae5ba52f9f5cef30752`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | constrictor-snake,dire-rat,dog,eagle,fire-beetle,wolverine | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| vrock | `5f4aa097fc3e4fa4a627ce5d5be6509e` | Large / 4 | KMG_Summoning_Demon_Vrock_Bite (`3f2836b9353f4b1b9e722b98070feaf4`) | `3690df4ef25345f0a3f75c9ec56f2de8` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| vrock | `5f4aa097fc3e4fa4a627ce5d5be6509e` | Large / 4 | KMG_Summoning_Demon_Vrock_Claw (`ded2d13a407e41088ac0845f257f6560`) | `553c473e642b4ea78905276cdd7a14e1` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| vrock | `5f4aa097fc3e4fa4a627ce5d5be6509e` | Large / 4 | KMG_Summoning_Demon_Vrock_Talon (`77976b057df64b959866ddf0e82a257a`) | `95108263c6c54e78a6ae5e92df53fd72` | nat melee | 10 → 6 | 10 | yes | — | FIXED (Natural): raw → 5, type 2; +4 SummonReachRange4; live 10 → 10 ft |
| water-mephit | `556264c328834ffcbd0dddd86a93ac2b` | Small / 0 | RayItem (`f6ef95b1f7bb52b408a5b345a330ffe8`) | `1d39a22f206840e40b2255fc0175b8d0` | mfg ranged | 50 → 50 | 50 | no | barbed-devil,bebelith,fire-mephit,giant-spider,ooze-mephit | UNCHANGED: ranged, not a dead-zone defect |
| water-mephit | `556264c328834ffcbd0dddd86a93ac2b` | Small / 0 | Claw1d4 (`118fdd03e569a66459ab01a20af6811a`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | air-mephit,deinonychus,dust-mephit,eagle,earth-mephit,fire-mephit,giant-crab,ice-mephit,lion,magma-mephit,ooze-mephit,salt-mephit,steam-mephit | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| wolf | `48d7740de99b4040bad6b6493a91f42b` | Medium / 0 | BiteDragon1d6 (`f3ff6972c32f22e4ba4c85c3982a03cf`) | `12a8a3a89e62d6b4fbc09ecdc187a828` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| wolverine | `f640d3e77d7d4a0d8de3351129cd7148` | Medium / 0 | ClawLarge1d6 (`c76f72a862d168d44838206524366e1c`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | dire-bear,dire-lion,grizzly-bear,owlbear,shadow-demon,succubus | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| wolverine | `f640d3e77d7d4a0d8de3351129cd7148` | Medium / 0 | KMG_Summoning_Natural_Bite1d4 (`0e3e5a52e7824ae5ba52f9f5cef30752`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | constrictor-snake,dire-rat,dog,eagle,fire-beetle,viper | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| woolly-rhinoceros | `8c4a8e045ca844a8bd42f614707b8e74` | Large / 0 | MastodonGore (`de42c58801037b84c9d992634ddd7220`) | `352b738b4ebfd554b909852e7f3b690a` | nat melee | 5 → 2 | 2 | no | elephant,mastodon | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| xill | `440bd760511f4bdd9515ff59a0c5eb33` | Medium / 0 | KMG_Summoning_Special_Xill_Bite1d3 (`e4027a9a27af49d198a6c350ed78ffa6`) | `952e30e6cb40b454789a9db6e5f6dd09` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
| xill | `440bd760511f4bdd9515ff59a0c5eb33` | Medium / 0 | KMG_Summoning_Special_Xill_Claw1d4 (`edbecbcb598646309f2b1ebb286fd5c8`) | `d4f7aee36efe0b54e810c9d3407b6ab3` | nat melee | 5 → 2 | 2 | no | — | VERIFIED UNCHANGED: type ≤ 2 ft, not a PF2 reach weapon |
