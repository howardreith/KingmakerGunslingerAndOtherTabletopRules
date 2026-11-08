# Summoned Creature Manual Playtest Repair — R4 status

**R4 PARTIAL (2026-11-08).** MEPH-01 and MEPH-02 are complete for review on
executed native evidence:

- Air, Earth and Water deal their required damage through owned breaths.
- Water deals acid packets through the native acid cone.
- Earth deals bludgeoning packets.

Two R4 items remain:

- **Salt Mephit breath icon (MEPH-03).** The art chosen
  (`FragmentationForm`) renders as a blue figure on a glowing circle in the
  inspected native window. It does not read as salt or grit.
- **Pyrotechnics flash (MEPH-04).** The flash is established natively:
  exactly one native blinding-burst instance is spawned per executed cast, on
  the caster, and it retires on its own after about 3.1 s. No framebuffer
  frame was captured while it was visible.

Owner aesthetic approval is separate and pending. Nothing was merged,
released or installed.

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
| Salt | `8c8fe6eb…` | `0082eea8…` | slashing | slashing | Physical: Slashing | pale cold cone | Fragmentation | no damage defect; **icon defect remains** | loaded; description screen (misleading art) |
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

- The native-backed breaths keep native targeting, so they also hit allies
  in the cone. The six variants stay enemy-only.
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

## Remaining R4 gap (precise)

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
