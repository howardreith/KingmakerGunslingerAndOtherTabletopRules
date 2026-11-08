# Summoned Creature Manual Playtest Repair — R4 status

**R4 COMPLETE FOR REVIEW (finishing checkpoint, 2026-11-08).** MEPH-01..04
are technically established. Owner aesthetic approval is a separate, pending
judgment. Nothing was merged, released or installed.

- **MEPH-01/02:** damage repaired (unchanged since the first checkpoint).
- **MEPH-03:** Salt's art replaced after inspecting real native art.
- **Charter ally-safety:** repaired on the four owned native-backed
  breaths.
- **MEPH-04:** the Pyrotechnics flash was captured while alive.
- **One restoration exception:** UMM `Params.xml` did not return to its
  intake hash. Exact restoration of that file could not be established
  (recorded below).

## Evidence wording (active; supersedes broader phrasing below)

- **Air:** loaded slashing profile, owned grant and live AI-action
  reference. No Air cast was executed.
- **Earth:** an executed cast (first checkpoint) produced bludgeoning damage
  on the intended target. That assertion did not establish the absence of
  stray-target damage.
- **Water:** an executed cast produced acid damage, launched the acid-cone
  projectile, and produced no stray damage events in that fixture. The
  screenshot shows the green impact, not the cone in flight.
- **Brains:** the fixture's brains were inactive. An AI-action reference is
  configuration, not an observed autonomous cast.
- **Saves:** every damage observation used controlled failed saves (seeded
  native d20 on the disposable hostile). Natural save outcomes and save
  scaling are not qualified.
- **Dice:** the loaded 1d12 configuration of the four native breaths is
  preserved. Whether it comes from the game data or another enabled mod
  remains unresolved.
- **Fire and the six variants:** loaded configuration and description
  windows only; none was cast.

## Finishing checkpoint (continues the R4 ledger)

- **Intake recheck:** identical to the first R4 intake. Branch head
  `9aa73c4711`; PR #114 open as a draft.
- **Salt icon (MEPH-03):**
  - Native art was inspected before choosing. A read-only pure-Python reader
    (no installs) extracted the 64×64 native textures from
    `Kingmaker_Data/sharedassets1.assets`. The decode was verified against
    in-game renders (Burning Hands, Fragmentation, Sandstorm).
  - About 20 candidates were compared:
    - Ice Blast, Blizzard, Sleet and white-dragon art read as cold.
    - Clashing Rocks and Rock Throw read as boulders.
    - Spike Growth reads as plant thorns; Mud Blast as mud.
    - Horrid Wilting is a withered face.
    - Wind Wall is too close to Dust's Sandstorm.
    - Glitterdust is Salt's own spell-like ability.
  - Chosen: **Metal Blast** (`MetalBlastAbility` `665cfd37…`), a burst of
    pale jagged shards that reads as cutting crystalline grit. It is
    distinct from Dust (golden sand), Earth (rock cluster) and Glitterdust
    (sparkling silhouette).
  - The ten-unique-art assertion still holds and was not narrowed. The
    other nine assignments are unchanged.
- **Charter ally-safety (MEPH audit):**
  - The inherited native breath effects deal damage to every creature in the
    cone. The native breath AI actions' only target consideration is
    `AttackTargetsPriority` (no friend avoidance). So the four owned
    native-backed breaths did **not** guarantee ally safety.
  - Repair: the repository's existing scoped mechanism. The six variants'
    `Conditional(ContextConditionIsEnemy)` effect gate is now a shared
    `MephitBreathEnemiesOnly` helper (same object graph and comment) and is
    applied to the four owned breaths.
  - Native donors and AI are untouched. Dice, recharge, save scaling,
    sickness and action cost are unchanged.
  - The breath text now states that only the mephit's enemies are affected.
  - All ten loaded breaths carry the gate (`loaded-breath-matrix`).
- **Pyrotechnics (MEPH-04):**
  - The cast loop now evaluates the spawn trigger after each yielded frame
    and before the completion break, with a single-fire guard.
  - The exact instance from the executed cast is tracked. Liveness is
    sampled at the frame the screenshot is requested, not while the file is
    written. Retirement afterwards is required.
- **Narrow finishing phase** `summon-playtest-r4-finish`:
  - configuration checks for all ten;
  - fresh Salt, Water, Earth (as the ally) and Magma;
  - the Salt description, one ally-safety case and Pyrotechnics.
  - The original phase is unchanged apart from the shared loop fix and gate
    check.
- **Focused gate:** 16 selected (summon-playtest-r4 ×4, summon-playtest-r3
  ×5, Sprint 5 ×4, sprint32 admission, identities-exact, sprint18
  append-only): 16/16, 20 s. The guarded admission accepts
  `summon-playtest-r4-finish` and refuses an unknown phase. No identities or
  ledger entries changed.
- **Finishing candidate** (one build, explicit `-ReferenceBundleDir`, 16
  selected, 394 s; the nested wrapper ran only its `selected-behavior`
  self-check):
  - Source `15f9f68a976a66b1051d5e36f8203b371bf338c1`.
  - Package
    `KingmakerGunslinger-0.0.179-development-summon-playtest-r4-15f9f68a976a.zip`
    SHA-256 `a5c1d4b727f44f477f389ecf4070706d07962cb7a68630a660add875b78cd555`.
  - DLL `2928e00484a0f1b410dea9c9ca20d4a76953e46f558aa374f544b7ca8528dc25`,
    loaded MVID `175dc344-f882-4f83-a2ed-a35047de98f7`.
  - Observer: the R4 finishing phase in that DLL.
- **One guarded run** `20261108T0513132129516Z-1ef754bc478b40c8adb2f805e5df5cef`
  (133 s; 5 PNGs local and private): **17 PASS, 0 FAIL**.
  - Configuration: `loaded-breath-matrix` (all ten, including the enemies
    gate; Salt `MetalBlast`), `native-donors-untouched`,
    `pyrotechnics-flash-configured`, `live-breath-facts`.
  - `render-salt-mephit-breath`: passed.
  - `ally-safety-water-breath`:
    - Water at 2.6 m from the hostile; ally at 2.75 m, 19° off the aim.
    - The cone delivered to both the ally and the hostile. The ally had no
      damage event (damage 0 → 0); the hostile took Energy:Acid 3 under a
      controlled failed save.
  - `pyrotechnics-cancel-no-flash`: passed.
  - `pyrotechnics-flash-captured-live`:
    - The flash spawned at frame 1485 on the caster. The trigger fired after
      a yield (frame 1486).
    - The during image was requested at frame 1500 with the exact instance
      alive, and completed at 1505.
    - The instance retired by itself at frame 1591.
  - `pyrotechnics-one-flash`: applied once, resource 1 → 0, one instance.
  - Plus admission, native Default enter/restore, `owned-ui-restored`,
    fixture and profile restore.
- **Images inspected:**
  - `000` Salt Mephit Breath window: Metal Blast shards icon, "1d4 slashing
    … touches only enemies", no `<null>`.
  - `001` the Water Mephit breathing a green acid spray. The hostile and ally
    are at the frame edge, so the protection rests on the
    delivery/damage records.
  - `002` before: no flash.
  - `003` during: a bright white-blue burst of rays with a golden runic
    triangle around the magma mephit, under the "Pyrotechnics" float. It is
    at the right edge of the view and partly cropped. **Visual acceptance:
    established.**
  - `004` after: the flash is gone and nothing lingers.
- **Restoration:**
  - Live tree back to `5C3E4AD4…` (0.0.140); record
    `expanded-summoning-restoration/20261108T0514516642030Z-…`.
  - 66 settings restored; 95 protected saves unchanged (baseline
    `CC7CBB0D…`); the intake readout is byte-identical.
  - `owned-ui-restored`: no errors.
  - No process; last lease `Completed`, no recovery required.
  - **Exception (narrowed 2026-11-08):** the KMG installation, game
    settings and protected saves were restored, and the enabled-mod profile
    is unchanged (all 16).
  - UMM `Params.xml` did **not** return to its intake hash: `c51c869c…`
    (only this 16-character prefix was recorded) became `e44f4495…`.
  - A comparison against an older UMM backup (`runtime-backups/umm-params`,
    2026-09-26), not the intake bytes, found only a `<LastUpdateCheck>`
    difference. The current file carries a timestamp from this run's game
    start (00:14:01). Exact restoration of the file could not be
    established.
  - The R4 runner did not snapshot `Params.xml`, and no backup matching the
    intake hash exists. The file was not rewritten, and no timestamp was
    invented.
  - R5 adds byte-exact `Params.xml` protection to the guarded runner and
    uses the current file as its intake.
- **Spend:**
  - This continuation: gate 20 s, build 394 s, run 133 s = **547 s**.
  - R4 ledger total **1,135 s** of the 1,800 s ceiling (588 s from the first
    checkpoint). R3 ledgers are unchanged.

### Owner retest checklist

1. Salt Mephit breath: the Metal Blast shard icon reads as salt/crystal grit.
2. Pyrotechnics shows one brief radiant flash with a runic triangle, and
   nothing lingers.
3. The Water breath's acid look and the other nine icons are acceptable.
4. Air/Earth/Fire/Water breaths no longer harm the mephit's allies.
5. Optional: 1d12 versus PF1 1d8 on the four native breaths.

---

# First R4 checkpoint (historical; evidence wording corrected above)

## Starting state and branch

- R3 accepted (PR #113 merged as `5ad3e88767`, reviewed head `a61310c5b5`).
- Branch `codex/summon-playtest-r4-mephit` from master `5ad3e88767`; draft PR
  against master.
- R3 evidence stays with its own identities:
  - UI-01: `57a8e659b1e6`, with the undetected Auto End Turn qualification.
  - UI-02..06: `31e61aa73a01`, 23 PASS / 0 FAIL.
  - R3 product code: `221cadc7a67e`.
- R1 cuts and the accepted R2 behavior are untouched: the fear fix, and the
  retained native surprise-round arrival.

## DATA intake (recorded; restored exactly)

- Installation: 0.0.140; DLL `00EF67570D30E4FF…`; live tree
  `5C3E4AD499DC5116…` (254 files).
- Mods: 16 mod directories. The UMM `Params.xml` (`c51c869cd5161d89…`)
  enables all 16: BagOfTricks, BetterVendors, CallOfTheWild, CheatMenu,
  CraftMagicItems, EddicKingmakerRespec, KingmakerBuffPlanner,
  KingmakerBugfixes, KingmakerDiceRoller, KingmakerGunslinger,
  KingmakerLastAzlantiPreserver, ProperFlanking2, RacesUnleashed, SkipIntro,
  TweakOrTreat, ZFavoredClass.
- Settings: 66 values.
- Saves: 95 protected save files (464,971,512 bytes);
  KMG_AUTOMATION_BASELINE `CC7CBB0D08581873…`.
- Reference bundle: manifest `7F1EDE2B…`.
- Runtime: no Kingmaker process; last lease `Completed`, no recovery
  required.

## Ten-breath matrix (actual vs required)

"Before R4" is the installed native configuration. For the four
native-backed rows it comes from the 2.1.4 blueprint dump and is confirmed in
the loaded game by the `native-donors-untouched` observation. For the six
variants it is the project source, unchanged. "After R4" is the loaded
configuration in candidate `a1e6c264193f`.

| Mephit | KMG unit | Owned breath (GUID) | Required | Before R4 | After R4 (loaded) | Cone | Icon (existing art) | Disposition | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Air | `9de31a33…` (native-backed) | `02ef2120…` | slashing | native `1f084387…`: **electricity** | Physical: Slashing | pale cold cone (unchanged) | Air Blast (wind) | **repaired** (damage, icon) | loaded configuration; description screen |
| Earth | `2ddb201a…` (native-backed) | `69687d6e…` | bludgeoning | native `fa5ee5f4…`: **acid**, acid cone | Physical: Bludgeoning | acid → pale cold cone | Earth Blast (rocks) | **repaired** (damage, cone, icon) | loaded; **executed**: 1 RuleDealDamage, bludgeoning only; description screen |
| Fire | `a841652d…` (native-backed) | `4405eaee…` | fire | native `ab0616be…`: fire | Energy: Fire | fire cone (unchanged) | red-dragon Fire Breath | no damage defect; icon repaired through an owned clone | loaded; description screen |
| Water | `556264c3…` (native-backed) | `c5e19388…` | acid | native `a54cd279…`: **electricity** (text said cold; cold cone) | Energy: Acid | cold → **acid cone** | black-dragon Acid Breath | **repaired** (damage, cone, text, icon) | loaded; **executed**: 1 RuleDealDamage, acid only; native AcidCone15Feet00 launched; inspected acid-green impact |
| Dust | `30f6615b…` | `9bf56be0…` | slashing | slashing | Physical: Slashing | pale cold cone (tolerated) | Sandstorm Blast | no damage defect; icon repaired | loaded; description screen |
| Ice | `b37cdd78…` | `fe4016ec…` | cold | cold | Energy: Cold | cold cone | white-dragon Cold Breath | no damage defect; icon repaired | loaded; description screen |
| Magma | `ce9551f1…` | `512be3e3…` | fire | fire | Energy: Fire | fire cone | Magma Blast | no damage defect; icon repaired | loaded; description screen |
| Ooze | `7cbf19a6…` | `3d5cd0f7…` | acid | acid (Reflex negates damage and sickness, preserved) | Energy: Acid | acid cone | Corrosive Touch (slime) | no damage defect; icon repaired | loaded; description screen |
| Salt | `8c8fe6eb…` | `0082eea8…` | slashing | slashing | Physical: Slashing | pale cold cone | Fragmentation → **Metal Blast** (finishing) | no damage defect; icon repaired in the finishing checkpoint | loaded; description screens (first: misleading art; finishing: Metal Blast) |
| Steam | `047f68ee…` | `146c6326…` | fire | fire | Energy: Fire | fire cone | Steam Blast | no damage defect; icon repaired | loaded; description screen |

Every row is granted through `BlueprintUnit.AddFacts`: exactly one owned
breath and no native breath. The unit's live brain has exactly one cast
action referencing the owned breath and none referencing a native breath.
The loaded native donors are unchanged: they still grant and cast their own
breaths (air/water electricity, earth acid, fire fire), keep Burning Hands,
and keep their native brains.

Preservation-context discrepancy, not changed: the loaded native
Air/Earth/Fire/Water breaths roll **1d12**, where PF1 and the 2.1.4 dump
have 1d8. The owned clones copy the loaded dice, and their text states 1d12.
Whether the source is the installed 2.1.7b data or an enabled mod is not
established. Variant dice (1d4; magma 1d8) are unchanged.

## Repairs

- **Owned native-backed breaths**
  (`ExpandedSummoningSpecialBuilder.MephitBreathR4.cs`):
  - The four KMG mephits receive the shared native breath through their own
    `AddFacts` array, and the shared native brain casts it.
  - Each gets an owned clone (same delivery, cone length, Reflex-half save,
    dice, targeting and action cost) with the required damage, the matching
    native 15-ft cone and its own art.
  - The redirect touches only that unit's `AddFacts` entry, plus an owned
    copy of the native brain whose one breath action is replaced.
  - It fails closed on any other grant shape, a shared array, or a brain
    that is not the donor's.
  - Native breaths, AI actions, brains and donor units are never written.
- **Explicit damage kinds** (`MephitBreathPolicy`): slashing and
  bludgeoning are physical; fire, cold and acid are energy. An unknown kind
  (for example electricity) is refused rather than falling back to acid. The
  variants' existing profiles are unchanged and must agree with the required
  table.
- **Icons:** native semantic reuse for all ten. The parent summon icons and
  creature portraits are unchanged.
- **Pyrotechnics flash** (MEPH-04):
  - The native Flare Burst `Blinding00_10Feet_Aoe` (`bb95a617…`) spawn
    component is cloned onto the owned Pyrotechnics ability, retimed to the
    single non-target `OnApplyEffect` spawn and anchored on the caster.
  - Per the decompiled `AbilityExecutionProcess`, this spawns once per
    executed cast (after the target loop), never for a queued or cancelled
    command, and regardless of saves.
  - Action cost, resource use, targeting, blindness, duration and ally
    safety are unchanged. Magma Form stays withdrawn.
- **Identities:** 12 append-only: Air/Earth/Fire/Water `Breath`,
  `BreathAi` and `Brain` (Air `02ef2120…`/`7c26375d…`/`be62a10d…`, Earth
  `69687d6e…`/`94b6806c…`/`b1abc63e…`, Fire `4405eaee…`/`957e5b72…`/
  `2c63914a…`, Water `c5e19388…`/`1557b0b2…`/`3a1f1bd2…`). The ledger, the
  manifest tool and the icon-catalog registry hash are updated.

Preserved, recorded:

- First checkpoint only: the native-backed breaths kept native targeting
  and hit allies in the cone. This was **repaired in the finishing
  checkpoint** under the charter's ally-safe rule; all ten are now
  enemy-only.
- A Water/Earth/Air/Fire mephit saved before R4 keeps its native breath fact
  and AI action until it expires. No load fixer was added.
- Dust's ice-like cone is tolerated. Earth now uses the same pale cone as
  the other physical breaths; its mid-cone look was not captured.

## Validation (R4 ledger: target 900 s, ceiling 1,800 s; R3 ledgers unchanged)

- **Focused gate** (76 selected real tests: summon-playtest-r4 ×4, r3,
  Sprint 5, sprint32 admission, and every ledger and identity-count test
  touching the appended identities):
  - one production compile error (a shadowed local), 13 s;
  - then 76/76, 22 s;
  - after a trace hardening edit, 76/76 again, 19 s.
  - The guarded PowerShell admission accepts `summon-playtest-r4-mephit`
    and refuses an unknown phase.
- **Candidate** (one build, explicit `-ReferenceBundleDir`, 76 selected,
  390 s; the nested wrapper ran only its own `selected-behavior`
  self-check):
  - Source `a1e6c264193f57669e8d87df36023abc8cade857`.
  - Package
    `KingmakerGunslinger-0.0.179-development-summon-playtest-r4-a1e6c264193f.zip`
    SHA-256 `69d293d754a4a258ef570d7cb9dc36efffafa9acfee349d67ea75c75e0681527`.
  - DLL `b7fbc23ae0605dea5003277d395633850b35278ef5d122db3bd3f39f77701935`,
    loaded MVID `d4cfcc9f-4314-4e19-ad50-0cbb454c12b0`.
  - Observer: the R4 phase as compiled into that DLL.
- **One guarded run**
  `20261108T0411432421160Z-5ba82967f8c345f682a8bc5c049004ca` (144 s; evidence
  and 14 PNGs local and private): **26 PASS, 0 FAIL**.
  - Configuration: `loaded-breath-matrix`, `native-donors-untouched`,
    `pyrotechnics-flash-configured`, `live-breath-facts` (ten fresh
    summons).
  - `render-*` ×10: each native ability window built as
    `Type = Ability, Ability = live fact`, owned, no unrelated modal.
  - `water-acid-executed`: water mephit → hostile, owned breath; one event,
    packet Energy:Acid 3; native AcidCone15Feet00 launched; no stray
    targets; forced failed Reflex (DC 13, seeded d20).
  - `earth-bludgeoning-executed`: one event, packet Physical:Bludgeoning 3.
  - `pyrotechnics-cancel-no-flash`: a queued command cancelled before start
    gave no flash, no application, resource 1 → 1.
  - `pyrotechnics-one-flash`: applied once; resource 1 → 0; exactly one
    `Blinding00_10Feet_Aoe(Clone)` spawned on the caster (frame 1423), not
    parented to it, alive about 3.1 s, retired by itself (frame 1525).
    Hostile blinded, caster not.
  - Plus admission, native Default enter/restore, `owned-ui-restored`,
    fixture and profile restore.
- **Images inspected** (`native-ui-000`..`013`, 1920×1080, normal UI scale):
  - `000`–`009`: the ten breath windows. Name, own icon, Target "All
    creatures within a 15 foot cone", Spell Resistance, descriptor (energy
    breaths) and Casting Time. The text states the actual damage (1d12 for
    native-backed). No `<null>`.
  - Icons read correctly for Air (wind), Earth (rocks), Fire (red dragon),
    Water (green acid dragon), Dust (sand swirl), Ice (frost dragon), Magma
    (magma burst), Ooze (slime hand) and Steam (steam cloud). **Salt** shows
    a blue figure on a glowing circle: misleading.
  - `010`: Water impact (enlarged crop) is a green acid burst with "Water
    Mephit Breath" and 3. `011`: Earth impact with 3.
  - `012`/`013`: Pyrotechnics before and after, no flash present.
  - **No during frame:** the spawn landed on the cast's final frame, and the
    observer's trigger check ran only before each yield (observer ordering
    defect).
- **Restoration (exact):**
  - Live tree back to `5C3E4AD4…` (0.0.140); record
    `expanded-summoning-restoration/20261108T0413333742525Z-…`.
  - 66 settings restored; 95 protected save files unchanged (baseline
    `CC7CBB0D…`).
  - The post-run intake readout is byte-identical to intake; `Params.xml`
    unchanged.
  - `owned-ui-restored`: no errors.
  - No process; last lease `Completed`, no recovery required.
- **Spend:** gates 54 s, build 390 s, run 144 s, for **588 s (≈ 9.8 min)**
  of the 15-minute target. Image inspection and restoration are excluded.

## Remaining R4 gap (resolved in the finishing checkpoint above)

1. **Salt Mephit breath icon:** the `FragmentationEarthBlastAbility` art is
   not grit or salt. Replace it with existing art that reads as salt or
   crystal grit, distinct from Dust's Sandstorm and Salt's own Glitterdust,
   and confirm it in a native description capture.
2. **Pyrotechnics flash frame:** an observer-only fix. Evaluate the spawn
   trigger after each frame as well (and before leaving the cast loop), so
   the capture runs while the native flash instance is alive. Keep the
   execution, spawn and retirement records.

## Owner retest checklist

1. Water Mephit breath looks acidic (green) and its tooltip says acid.
2. Earth Mephit breath: the pale cone and rock icon are acceptable for
   bludgeoning grit.
3. All ten breath icons are acceptable; Salt is a known defect.
4. Pyrotechnics shows one brief blinding flash, and nothing lingers.
5. Optional: decide on the native 1d12 breath dice versus PF1 1d8.
6. Optional: whether native-backed breaths should become enemy-only like
   the variants.
