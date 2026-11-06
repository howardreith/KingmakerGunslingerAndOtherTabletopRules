# Bone Devil Sprint 3 — corrected acceptance complete (development candidate; draft PR #103, unmerged)

**Base:** branch `codex/bone-devil-sprint3`, from master `dc609a10e6`
(PR #101 merged; `v0.0.178` published).

**Scope:** two gameplay corrections:
- Infernal Wound bleed on Wall of Ice sections
- natural reach versus the installed ProperFlanking2 reach dead zone

**Status:**
- The corrected Sprint 3 acceptance is **complete** on one exact candidate,
  run **s3c** (`baf796c4d6`): PASS, 156/156 assertions and 155/155 phase checks.
- This is a development candidate only. It is not merged, released or
  release-qualified.

## Change separation

| Kind | Commits | Files |
| --- | --- | --- |
| Production | `c1fe8b0711` | `Summoning/InfernalRules35Policy.cs`, `Summoning/InfernalWound34.cs`, `Blueprints/ExpandedSummoningSpecialBuilder.Infernal35.cs` |
| Domain regressions | `c1fe8b0711`, `baf796c4d6` | `InfernalRules35Tests.cs`, `InfernalAdjacentRoutineVerdict35Tests.cs`, `Program.cs` |
| Observer / fixture | `c1fe8b0711` (strict physical verdict, first adjacency check), `cd23ceef6c`, `717b460c1a`, `baf796c4d6` | `RuntimeTesting/*` only |
| Documentation | `b3b9f824a4` and this record | `planning/BONE-DEVIL-SPRINT3.md` |

Production has not changed since `c1fe8b0711`: `git diff c1fe8b0711 HEAD`
outside `RuntimeTesting` is empty except the csproj line for the new
observer verdict.

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

**Native** (`wall-physical-destruction`). The check is strict: the killing
blow must be a weapon hit, with no secondary (bleed) row and no
attacker-owned effect on the section.

| Run | Glaive hits | Weapon damage | Killing blow | Bleed rows | Attacker effects on section |
| --- | --- | --- | --- | --- | --- |
| s3a | 6 | 39 | weapon hit | 0 | none |
| s3b | 7 | — | weapon hit | 0 | none |
| **s3c** | **6** | **38** | **weapon hit** | **0** | **none** (`noInfernalWoundOnSection` true) |

In s3c every damage packet was object-halved and every roll an automatic
hit. The hit-point sequence destroyed the section, which left its owned cold
residual for the remaining duration and no steam.

These destruction checks exercise the **Bone Devil's** Wall section only. No
native Ice Devil Wall section was attacked. The Ice Devil exclusion is
covered by the domain regression alone, because `sprint40-*` phases run only
on HOWARD-LAPTOP.

Before this fix, close12 recorded a killing blow from the bleed, and close27
recorded the wound effect on the section.

## 2. Natural reach is legal beside the Bone Devil under ProperFlanking2

**Installed contract**, read from the decompiled assemblies:
- `BlueprintWeaponType.AttackRange` returns `max(2 ft, raw − 4 ft)` for raw
  ranges up to 10 ft.
- `UnitDescriptor.GetWeaponRange` adds the unit's native `ReachRange` stat.
- **ProperFlanking2** patches `UnitCommand.GetTargetPoint` and
  `UnitEngagementExtension.IsReach`, guarded by the setting
  `reach_weapons_dead_zone`. It treats a melee weapon as a reach weapon when
  its native **type** range exceeds `GameConsts.MinWeaponRange` (2 ft). Such a
  weapon cannot strike inside `(size reach + 1) ft × 0.5` beyond both bodies;
  the mod moves the approach point away instead.
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
- Total reach is unchanged:
  - unit weapon range is 9 ft;
  - the native approach radius is 4.5432 m in int3, s3a and s3c;
  - the tail-lane distance check still passes.
- ProperFlanking2 is neither disabled nor overridden, and manufactured reach
  weapons keep their dead zone.

**Regression:** `expanded-summoning.sprint35-bone-natural-reach-not-reach-weapon`.

### Corrected adjacent-routine acceptance

The first check (`c1fe8b0711`) accepted a finished command with one natural
hit, so s3b's partial routine passed. It is replaced by
`InfernalAdjacentRoutineVerdict35` over the recorded receipt, which passes
only when all of the following hold:

- **Profile and plan.** The profile names bite, claw and sting, and the
  command's native plan, from the trace's command-end row, is exactly bite,
  claw, claw, sting.
- **Every slot resolves.** Each planned slot appears as one attack row for
  the exact owner, exact target and exact command. Any missing, extra or
  foreign row fails. A miss counts as a resolved attack.
- **Native terminal boundary.** The command ends `Success`, or with the
  installed stack's exhausted-routine `Interrupt` (the
  `Sprint40NativeExhaustedAttack` contract). For the interrupt, a
  command-end row for this command and actor must show:
  - attack index 4;
  - no next planned attack;
  - an interruption raised from `UnitAttack.OnTick`.
- **Lifetime and life.** The attacker still has its summon lifetime and is
  alive at the end.
- **ProperFlanking2 state.** The assembly is loaded, and the UMM entry is
  loaded and active from that same assembly. Its live
  `Main.settings.reach_weapons_dead_zone` (read by reflection) is true, and
  its postfixes on both `GetTargetPoint` and `IsReach` are installed.
- **Geometry.** The target is inside the dead zone and threatened, and
  neither unit travels 0.25 m or more.

**Regressions** (domain, linked verdict source):
- `sprint35-adjacent-routine-complete-passes`: Success, exhausted `Interrupt`
  and any slot order pass.
- `sprint35-adjacent-routine-misses-resolve`: misses are not missing attacks.
- `sprint35-adjacent-routine-partial-fails`: these fail:
  - the s3b shape;
  - a foreign interrupt caller;
  - a next planned attack still present;
  - completion evidence from another command or another actor;
  - lifetime loss;
  - an unfinished command or another result.
- `sprint35-adjacent-routine-missing-evidence-fails`: these fail:
  - a missing profile, plan or slot;
  - a missing attack row, or an extra, wrong-target or wrong-command row;
  - a missing command identity;
  - missing completion evidence.
- `sprint35-adjacent-routine-preconditions-fail`: these fail:
  - ProperFlanking2 absent, inactive, disabled, unread or unpatched;
  - a target outside the dead zone or not threatened;
  - a unit that moved, or travel that was not recorded.

### Fixture

- **Attacker:** a fresh source Bone Devil, summoned in the two-Wall scenario.
- **Witness:** the fixture's own hostile, reset first. It is not a summon
  (so it has no lifetime), its brain is off and it has 100,000 hit points.
  Before this correction the witness was the faction-switched Glaive summon,
  which has its own lifetime.
- **Fear preparation:** the established per-source fear-aura exposure beside
  the source, then only the resulting fright is cleared.
- **Placement:** one placement beside the attacker on a native node, then
  six settle frames. Nothing is written during the command: no position,
  no roll, no deadline.
- **Summoner search:** the widened search is kept. A Wall cast without a
  summoner spot inside the 11.7 m follow distance now fails setup at once,
  with the search evidence, instead of casting.

### s3c native evidence (`bone-adjacent-natural-attack`, verdict failure: none)

**Actors before the command** (game second 43318.335):

| | Attacker | Witness |
| --- | --- | --- |
| Unit | Bone Devil `9e952b11-0f6a-45ed-8624-a1dfbf46b70a` | `cd8a8b6d-1cc3-416b-96ce-1932d16f6e5d` |
| Blueprint | `74652de839a94feba3c2fb67b536437c` | `4391e8b9afbb0cf43aeba700c089f56d` (`KMG_Runtime_Sprint32Turn_HostileTarget`) |
| Faction | Summoned | private hostile faction |
| Size | Large, corpulence 0.9 | Medium, corpulence 0.5 |
| Hit points | 155/155 | 100,000/100,000 |
| Summon lifetime | 115.0 s remaining | none (not a summon) |
| Fog / can act | not in fog / yes | not in fog / yes |

- **Hostility:** each is an enemy of the other.
- **Reset:** the witness reset was "clean".
- **Fear preparation:** the witness was not immune before exposure and was
  immune after it. After clearing, neither the witness nor the source was
  frightened.

**Geometry:**
- Centre distance 1.555 m and edge gap 0.155 m, against the dead-zone bound
  of 2.924 m.
- ProperFlanking2's size reach for this Bone Devil is 9 ft, giving a dead
  range of 1.524 m.
- `IsReach` is true.
- Each natural weapon has a 2-ft native type range, a 9-ft unit weapon range,
  and is not a reach weapon to ProperFlanking2.

**Command:**
- Native `UnitAttack`, reference −1484866304.
- `IsSingleAttack` is false and `ForceFullAttack` is true.
- The native plan was bite `a54ab763…`, claw `d0f2530d…`, claw `d0f2530d…`,
  sting `e3419532…`.

| Slot | Game s | Weapon role | Result | HP damage | Range |
| --- | --- | --- | --- | --- | --- |
| 1 | 43320.020 | bite | hit | 6 | 9 ft |
| 2 | 43321.368 | claw | hit | 6 | 9 ft |
| 3 | 43322.960 | claw | hit | 7 | 9 ft |
| 4 | 43324.534 | sting | hit | 14 | 9 ft |

- **Count:** 4 attacks (bite 1, claw 2, sting 1), with 4 hits and 0 misses.
- **Terminal:**
  - the result was `Interrupt`, with `nativeExhaustedInterrupt` true;
  - the command-end at 43325.344 shows attack index 4 and no next planned
    attack;
  - the caller was `UnitAttack.OnTick`;
  - the act rows show indices 1–4 in order.
- **Afterwards:** both units travelled 0.0 m, and the Bone Devil was alive
  and still had its summon lifetime.

**ProperFlanking2 configuration**, read in-process during s3c:
- **Assembly:** `ProperFlanking20`. The loaded copy is
  `Mods\ProperFlanking2\ProperFlanking20.dll.42708.cache`, MVID
  `980f03cb-a29f-4ed1-9903-086f5ab23328`, SHA-256 `b12baf53…8a2666`.
- **UMM entry:** `ProperFlanking2` 1.2.3-2.1. It is loaded, active, has its
  assembly, has no load error, and its assembly is the loaded one.
- **Setting:** the live `ProperFlanking20.Main+Settings.reach_weapons_dead_zone`
  is **true**. `settings.json` (SHA-256 `6e473006…a595da`) also reads true.
- **Postfixes owned by ProperFlanking2** (all observed):
  - `UnitCommand.GetTargetPoint`;
  - `UnitEngagementExtension.IsReach` (3- and 4-argument overloads);
  - `ForceAttackOfOpportunity`;
  - `RuleCalculateCMD.OnTrigger`;
  - `LoadDictionary`.
- Nothing about the mod or its setting was altered.

### Earlier runs, reclassified

The receipts are immutable. This is a reader-only re-reading of their raw
`InfernalEvents34` command traces; the old verdict was "pass" in both.

| Run | Old verdict | Raw trace | Corrected classification |
| --- | --- | --- | --- |
| s3a | pass (finished, 4 hits) | Plan bite, claw, claw, sting. 4 attacks against target `0700e0e7…` under reference 490795008. `Success`. Command-end at index 4, no next planned attack. | The four-slot routine credit is **retained**. The run recorded only assembly presence for ProperFlanking2 (no setting, entry or patch state) and no lifetime evidence, so it is supporting evidence, not acceptance. |
| s3b | pass (finished, 2 hits) | 2 attacks (bite, claw). Command-end `Interrupt` at index 2 with the next attack still planned, 7 ms after the Bone Devil's summon lifetime ended. | **Partial**: 2 legal adjacent hits before expiry. Fails the corrected verdict (`planned-slot-unresolved`). |

The no-bleed results stay credited to their own runs (s3a, s3b, s3c above).

## Runs

| Run | Executable | Result | Cause (classification) | Build / native s |
| --- | --- | --- | --- | --- |
| s3a | `c1fe8b0711` | FAIL 111 | Both Sprint 3 checks pass. Then source B's Wall cast was interrupted three times: no clear summoner spot existed behind its caster beside Wall A, so native summon follow pulled it back (fixture). The observer then hit a null section (observer). | 345.24 / 287.70 |
| s3b | `cd23ceef6c` | FAIL 97 | Physical check passes; adjacency only partial (see above). The original Bone Devil's native summon lifetime ended (game second 43320.667) 7 ms before the adjacency command's interrupt, because earlier steps ran about 12 s longer; source cleanup then found no lifetime (fixture timing). | 347.01 / 242.13 |
| **s3c** | **`baf796c4d6`** | **PASS 156/156** | All 155 phase checks pass. Each of the five shared-scenario Wall casts (turn boundary and both two-Wall orders) found a summoner spot 3.0–3.5 m from its caster and formed on its first attempt. | 346.48 / 351.42 |

**s3c identities:**
- **Commit:** `baf796c4d6a0d9ea63e09bd2dd366569f2c1465b`.
- **Package:** `KingmakerGunslinger-0.0.178-development-summon-outsider-review-corrections-baf796c4d6a0.zip`,
  SHA-256 `381d6fd67b198f0c0cd2a6511d04833c06d3fe0c822f040178df2be3cfd19d40`.
- **Loaded module:** MVID `c7326e80-bd7e-48a1-b0f1-bf0994732410`, SHA-256
  `b44e7fdae7f5e77101c1de62e26967d82dd9ce9fbbb1647cf21e55a08307f9c0`, game
  2.1.7b.
- **Run:** id `20261106T2024014308405Z-640be39a6877468fb2d2736daddc7925`.
- **Evidence:** `runtime-evidence/20261106T2024014134885Z-disposable-expanded-summoning-sprint32-native-review`
  (machine-local; not committed).
- **Restoration record:** `runtime-evidence/expanded-summoning-restoration/20261106T2029180325190Z-disposable-expanded-summoning-sprint32-native-review.json`.

**Phase coverage in s3c.** All of these passed:
- the physical, fire and controlled-expiry Wall lifecycles, with navigation
  and the live/restored native mover for each;
- the steam presentation and consumer, and steam pool reuse;
- the two-Wall shared geometry, formation in both orders, survivor
  preservation and baseline restoration;
- the turn boundary;
- source retirement cleanup;
- the tail lane at 5 ft beyond the bodies (distance 3.324 m, approach radius
  4.5432 m) and tail contact (plan of 4 completed).

**Restoration.** Every run restored the intake exactly. For s3c:
- the installed version `0.0.140` was read at intake and again afterwards;
- settings were restored;
- protected saves are unchanged;
- the live tree was restored (`sha256=5C3E4AD4…CB9BFBB`);
- `KMG_AUTOMATION_BASELINE` was untouched.

No host restriction was bypassed.

**Ledger.**
- **Before this continuation:** ≥ 21.30 min (3 gates 56.0 s, 2 builds
  692.24 s, 2 guarded runs 529.83 s).
- **This continuation (30-minute allowance):** 11.89 min, made up of:
  - 1 focused gate, 15.43 s (filter `expanded-summoning.sprint35-`, 28 cases,
    0 failures, production compiled);
  - 1 build, 346.48 s;
  - 1 guarded run, 351.42 s.
- **Sprint 3 total:** ≥ 33.19 min.
- **Cumulative:** ≥ 648.38 min.
- All figures are lower bounds; decompilation and analysis are not counted.

## Census (source only; not changed here)

These summons express reach or ordinary range through raw 9–10-ft
**natural** types, which ProperFlanking2 treats as reach weapons:
- **Small and Medium:** Lemure claw, Dretch, Hound and Lantern Archon, Astral
  Deva slam, Barbed Devil claw, Sprint 36 bodies. The dead zone is about
  0.15 m beyond the bodies.
- **Larger:** Lillend tail; Ankylosaurus, Stegosaurus and Brachiosaurus tails;
  Salamander tail.

**Ice Devil (source finding):**
- Its bite and tail use raw `ReachFeet = 10`
  (`IceDevilRules40Policy.ReachFeet`), and its ordinary spear uses that same
  range (`ExpandedSummoningSpecialBuilder.IceDevil40.cs`).
- The builder states the spear is the native SPEAR type, "not a reach
  longspear".
- By the installed getter, raw 10 ft is a 6-ft native type range. That
  exceeds 2 ft, so ProperFlanking2 would treat all three as reach weapons.
- This is a source reading only. Native Ice Devil behaviour needs the
  designated host (HOWARD-LAPTOP) and is not claimed here. Those weapons are
  unchanged.

Manufactured raw 10-ft reach weapons (Glaives, Giant flails) are correct
reach weapons.

## Next sprint

Classify every published summon weapon into three separate categories:
- **Natural reach** (bite, claw, slam, tail, sting): should be a 2-ft native
  type plus `ReachRange`, so adjacent squares stay legal under the dead zone.
- **Ordinary manufactured weapons** whose source range is long only to carry
  reach (e.g. the Ice Devil spear): must not become ProperFlanking2 reach
  weapons.
- **Genuine reach weapons** (glaive, longspear, whip, Giant flail): keep the
  dead zone.

Ice Devil native validation needs HOWARD-LAPTOP.

## Follow-up note (Sprint 4, 2026-11-06): corrections to the guidance above

The Sprint 3 results, the s3a/s3b/s3c runs and their identities above are
unchanged. These parts of the census and next-sprint guidance were wrong.

1. **"The dead zone is about 0.15 m beyond the bodies"** (Small and Medium).
   ProperFlanking2's dead range is `(size reach + 1) ft x 0.5`. For
   Small/Medium that is **0.914 m** beyond both bodies, not 0.15 m.
2. **"Manufactured raw 10-ft weapons (Glaives, Giant flails) are correct
   reach weapons"**, and the guidance listing **whip** and **Giant flail**
   among genuine reach weapons. Only the glaive is a reach weapon there.
   - The Ettin's flails are ordinary flails (Pathfinder: flail and heavy
     flail are not reach weapons).
   - The giants' greatclubs, greatswords and morningstars are ordinary
     weapons wielded with creature reach.
   - The whip is a reach weapon with its own adjacent-foe exception, and
     the roster has no whip.
3. **"Natural reach should be a 2-ft native type plus ReachRange"** as a
   universal recipe. An owner-wide Reach bonus also moves ranged weapons
   and cannot express unequal per-attack reach. Sprint 4 instead keeps each
   row's live range with an exact-owner, exact-item correction
   (`SummonReachRange4`), and keeps the Bone Devil's accepted Reach-stat
   representation.
4. **The census list was incomplete.** The installed-blueprint census found
   45 false reach-weapon rows on 26 creatures, including the Hill, Stone,
   Fire, Cloud and Storm Giants, Glabrezu, Nalfeshnee, Vrock, Hezrou,
   Babau, Kyton and the Bearded Devil claw.

See `planning/SUMMON-REACH-SPRINT4.md`.
