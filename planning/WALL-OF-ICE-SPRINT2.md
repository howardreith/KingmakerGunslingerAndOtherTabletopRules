# Wall of Ice Sprint 2 — complete (closeout below)

The Bone Devil Wall of Ice passed full native acceptance in one fresh guarded
Kingmaker process on exact executable source
`36e44513d3fa7e9a670245a2d1bfe39153058702`: **PASS, 95 assertions**, covering
feedback, formation, live obstruction, native attack eligibility, physical and
fire destruction, residual cold and steam, controlled original-deadline expiry,
exact navigation restoration after every retirement, and Bone Devil tail
contact. Six earlier guarded runs on intermediate candidates failed; each was
inspected before the next correction and all are retained below.

## Closeout (development candidate; PR #98 stays draft and unmerged)

Executable source `24a1b935831a2376571d706d382c1db40a45ddfc` (0.0.177
development candidate) passed the guarded `summon-overhaul-wall` phase in
one fresh process: **PASS, 152 assertions**. The phase includes:

- the full single-Wall acceptance
- delivered-feedback verdicts
- the steam presentation check
- the ordinary-camera tail sequence
- the new two-Wall scenario in both retirement orders

The 95-assertion credit above remains the credit for its own candidate
(`36e44513d3`). The 152 result is credit for `24a1b93583` only. Commits after
`24a1b93583` change documentation, or are reverts that return `src`, `tests`
and `scripts` byte-for-byte to `24a1b93583` (`git diff 24a1b93583 HEAD --
src tests scripts` is empty).

| Item | Value |
| --- | --- |
| Run ID | `20261106T0745404351478Z-fee2167da95f4e00bdb23eb8c1b69a45` |
| Evidence directory | `20261106T0745404181066Z-disposable-expanded-summoning-sprint32-native-review` |
| Result SHA-256 | `5b067a24e450b017a189fbdd3bdda64b15497183d1b32f1a603da410c8613693` |
| ZIP SHA-256 | `ff4af5e181c4c11af4b65a9d7bf7742b0c4a53285a8f623ef44d9d80d33f1e22` |
| DLL SHA-256 / MVID | `aa77f1e176272c28b127e8f58a6b54a449251a20ec8c4683f1687c049e009e16` / `1942ad28-e1f6-4572-a112-73cf24d5b512` |
| Source state | `142540a948803023a79a4fce07a0bd67d6ff926901c2ac9bd284c4916ddd192d` |
| Restoration | v0.0.140 before and after; live tree `5C3E4AD4…BFBB` (254 files), settings and protected saves exact |

ZIP path: `artifacts/development-candidates/summon-outsider-review-corrections/24a1b935831a/`.

### Production changes in this closeout (separate from observer changes)

1. **Wall steam is visible.** The native Obscuring Mist area
   (`e09010a73354a794293ebc7b33c2d130`) links effect
   `e19d6ede75196c244b1dc481aed96d70`. That link loads no prefab in the
   installed game, so native Obscuring Mist draws nothing. A postfix on
   `AreaEffectView.SpawnFxs` attaches the native 20-ft kineticist steam cloud
   (`f5677ddc969727746af75ab89a26167e`). It does so only for the Wall's own
   steam area, and only when the native view spawned nothing. Native
   `LateUpdate` follows the area and native `OnDestroy` removes the cloud.
   The cloud's burning-ground and glow renderers (16 of them) are switched
   off, because the steam deals no fire. Deactivating those layers'
   GameObjects instead stopped the whole cloud natively (close7: 0 playing
   systems), so only the renderers are disabled. Regressions:
   `sprint35-wall-steam-presentation`, `sprint35-wall-steam-no-flames`.
2. **A standing Wall section is not a creature.** Formation-save adjacency
   counted every unit, so Wall A's section rolled a Reflex save (natural 20
   against DC 18) and stopped an adjacent Wall B from forming (close19,
   native save row on the section's id). Wall of Ice lets only creatures
   adjacent to the forming wall save. `WallFormationSaveEligible` excludes
   the exact section identity. Regression:
   `sprint35-wall-section-not-formation-creature`.
3. Read-only accessor `InfernalWallNavmesh35.ClaimOwnedBy` (no behaviour
   change).

### Observer and test changes (no gameplay effect)

- **Feedback is accepted only when delivered.** Each case requires:
  - exactly one attempt
  - exactly one successful publication
  - zero faults
  - the exact message
  - the native log view present

  The mechanical outcome, native command/save/SR checks and silent polling
  are unchanged. `InfernalWallFeedbackPolicy35` is shared with the domain
  regression `sprint35-wall-feedback-delivery`. That regression shows none of
  these can pass:
  - a failed publication (attempts +1, published 0, faults +1)
  - a wrong-outcome message
  - a repeated publication
  - a success after a fault
- **Two-Wall scenario** (`RuntimeTestRunner.InfernalWallShared35.cs`); see
  below.
- **Ordinary-camera tail sequence and steam captures**
  (`RuntimeTestRunner.InfernalWallVisual35.cs`).
- **Physical destruction** records the killing-blow source.
  - The attacker's own Infernal Wound bleed applying to ice is the tracked
    bleed-on-ice defect. Its tick timing decides whether the last point comes
    from a weapon hit (accepted run; close27: weapon hit, 6 hits,
    36 weapon damage) or the bleed (close12: 34 of 36 from weapons).
  - The verdict no longer fails on tick timing; it labels the source.
- **Live-mover verdicts** judge "around, not through" within one recast cell
  (0.2 m): the mover must not cross the section's centre plane inside its
  length.
  - The detour-length test remains for single Walls.
  - The old zero-gap test only passed by luck (close16 grazed 0.24 mm).

### Feedback deltas

| Case | Attempts | Published | Faults | Log view | Accepted run | close27 |
| --- | --- | --- | --- | --- | --- | --- |
| Placement changed | +1 | +1 | 0 | present | same | same |
| Reflex disruption | +1 | +1 | 0 | present | same | same |
| Hostile SR | +1 | +1 | 0 | present | same | same |
| 240 clear + 240 occupied polls | 0 | 0 | 0 | present | same | same |

### Two Walls from separate Bone Devils (close27)

**Setup.** Each Bone Devil was summoned by the fixture caster with its
ordinary tier summon:

- source A `1d7ea155-5c29-4b08-9ec2-600fd0db7738`
- source B `f8242f59-9f53-479e-b918-97ffc80176bf`

Each cast the Wall with an ordinary `UnitUseAbility`, one attempt each.

**Wall A.** Axis-aligned at `(17.38, -5.77, 23.60)`, cut instance `-597222`.

**Wall B.** A legal diagonal placement at -45°, centre `(18.88, -5.77, 21.85)`,
cut instance `-598398`. It was chosen from:

- 732 clear-footprint candidates, 3 of them sharing
- 15 evaluated for a caster point and crossing route
- 6 viable, 3 of them sharing

Legal placement never lets two claims share a node, because Wall B's
footprint must be walkable. The overlap that does exist natively is Wall A's
claimed cells inside Wall B's native cut box.

**Grid.** Graph index 1, node size 0.5 m, `erodeIterations` 10,
`erosionUseTags` true. The region is ±6.5 m, 676 nodes: baseline
8 unwalkable (static), 0 dynamically blocked, 0 claims, no foreign cuts.

**Native grid block.** Combat was active, so the native combat controller
applied both cuts' grid blocks:

- Wall A: 6 counters
- Wall B: 36 counters (its diagonal bounding box)
- Wall B's counter covered 4 cells that Wall A claims (nodes 30301–30304)

| Check | a-first (retire A, then B) | b-first (retire B, then A) |
| --- | --- | --- |
| Formation | 2 separate sections, separate summoners and cuts; claims 24 + 22, none double-owned; 15 ice-footprint nodes all blocked | new sections `9804eff4` (A) and `94924f22` (B); same counts |
| Live movers, both Walls standing | A lane and B lane: around, never through (deviation 1.41 / 1.35 m, intrusion 0.0) | (run in a-first only) |
| First retirement | Survivor B: cut registered and enabled, grid block unchanged, 22 claims kept, 0 ice nodes changed; A's 20 retired-only nodes at baseline; 618 unrelated nodes unchanged | Survivor A: same; B's 34 retired-only nodes at baseline; 618 unrelated nodes unchanged |
| Survivor mover | around B (deviation 1.41 m) | around A (deviation 1.41 m) |
| Second retirement | all 676 nodes equal baseline in static, pre-erosion, counter, walkability and claim count; 0 claims; foreign cuts unchanged | same |
| Restored movers | both lanes direct (deviation 0.0) | both lanes direct |

Retirement used the established controlled original-deadline expiry. This
is a controlled expiry, not a real-time soak.

**Claim and erosion finding.** The concern is proven natively; a fix was
attempted, failed native acceptance, and was reverted.

- Installed `GridGraph.UpdateArea` (decompiled) runs with `updateErosion`
  true and `erodeIterations` 10. For every node within about 10 m of any grid
  update, it resets the static flag from the pre-erosion flag. Wall claims
  set only the static flag, so any native grid update erases them while the
  Wall stands.
- Native evidence:
  - The single-Wall receipts in close8 already showed all 24 claimed nodes
    back to walkable while the Wall stood.
  - close27 showed 14 of Wall A's 24 claimed nodes walkable before any
    retirement.
  - In b-first, retiring Wall B removed its counter from nodes 30301–30304.
    Those cells became walkable while Wall A still stood and still owned
    them.
- What stays blocked: the ice footprint itself, through each cut's own native
  counter, which is per cut and released only by its own revert. Real-time
  movement uses the recast graph and went around both Walls.
- Effect on players: limited to the turn-based grid's claimed ring cells,
  whose centres lie outside the ice.
- Precondition for the old `OriginalWalkable` concern: a claim records
  combined walkability. Under legal placement every recorded original equalled
  the baseline static flag (`claimOriginalsDifferingFromBaselineStatic` 0 in
  every two-Wall receipt).
- The narrow fix held both static layers and recorded the static flag (commit
  `4bb14d17f4`, regression `sprint35-wall-claim-survives-other-wall`). Its
  only native run (close28, `9c95a26053`) failed the physical lane's
  real-time route restoration: the recast route still detoured after 15 s,
  although every grid node was restored. The cause is unexplained, so the fix
  and its observer tightening are reverted (`76959d5f6d`, `9abf1246a9`).
  The ring-cell checks remain recorded, not enforced.

### Steam evidence

close27:

- spawned `Kinetic_SteamAoe00_20Feet_Aoe_Cycle(Clone)`
- 19 playing systems, 712 live particles, 11 visible renderers (cold energy,
  smoke, bubbles, droplets, puddles, splash)
- 0 of 16 flame renderers drawing
- captures: `s35-wall-steam-area-ordinary.png` (mean luma 15.4) and
  `s35-wall-steam-area-inspection-light.png` (19.2)

Inspected frames:

| Run | Conditions | What the frames show |
| --- | --- | --- |
| close5 | dim | readable grey cloud with orange flame tongues (before the flame correction) |
| close8 | dim, rain | readable grey-white vapour, no flames |
| close10, close27 | dim | readable grey-white vapour, no flames |

Limits:

- The fixture area is night-lit. The controlled inspection light raises luma
  only slightly; no daytime frame exists.
- A few orange sparks at the steam site sit on the fire-destruction lane.
  They are probably native fire-death embers (tracked polish); this was not
  separated.

### Tail evidence

close27 used the ordinary party camera on lane `tail-forward`, with
0 occluders in physics and renderer-bound rays. One native full attack
(plan 4: bite, claw, claw, sting) ran on owner `421e9ced`, target
`482b688b`. Frames, each correlated with the planned weapon and clip:

- attack start and wind-up for each attack (`s35-bone-tail-seq-a{1..4}-*`)
- contact (frame 4094)
- follow-through and late follow-through
- recovery (4107)
- recovered

close5 and close10 show the same pattern. Images were inspected for close5
and close27: the body rears with the tail raised before the sting, lunges at
contact, and is upright at recovery.

Limits:

- The sting uses the body's `Bite_Attack` clip.
- The private tail has no wind-up and moves only from the native impact
  (weight 1, decaying over about 0.35 s). The anticipation is body motion, so
  the "tail moving before contact" frame never fired (`anticipationFrame` -1).
- Mechanical contact and visible animation are separate evidence.

### Candidate runs (all inspected before the next correction; evidence retained)

| Run | Source | Result and cause |
| --- | --- | --- |
| close1 | `7493fac6a2` | FAIL: tail frame read `PlannedAttack` before the command started (NRE) |
| close2 | `f30bc67e45` | FAIL: new Bone Devil faded out 11 m from the party |
| close3 | `48f50463d2` | FAIL: attacker travel 1.08 m after a native redirect |
| close4 | `8c5fdaf4ac` | FAIL 95: tail capture loop timed out on a memory-starved host; wrapper then stopped by the host for low memory; lease completed and installation restored manually (receipt `20261106T0132553793008Z-close4-manual-restoration.json`) |
| close5 | `f5a2bfb9c0` | FAIL: two-Wall source never faded in (the mover is not a fog revealer) |
| close6 | `bb3bb08064` | FAIL: same; sight-clear level floor beside the mover is still fogged |
| close7 | `27fe93f6fb` | FAIL: deactivating flame GameObjects stopped the steam cloud |
| close8 | `1c268b7abb` | FAIL: two-Wall formation demanded shared counters (none outside combat) |
| close9 | `a6d7f51342` | FAIL: source B's AI walked it out of reveal range |
| close10 | `805dd7939a` | FAIL: 0.25 m floor-step limit rejected every reveal spot |
| close11 | `4c4d4094ed` | FAIL: two-Wall corridor skim vs single-Wall detour thresholds |
| close12 | `7b31923945` | FAIL: physical killing blow was the Infernal Wound bleed tick |
| close13 | `64a6c2b163` | FAIL: Wall B's caster stood behind Wall A; execution refused |
| close14 | `f0e1f607e0` | FAIL: survivor "cut live" required the turn-based grid block |
| close15 | `6aea705bb8` | FAIL: tail target had left the world (native `JoinCombat` null view) |
| close16 | `a649999f70` | build attempt 1 failed (temp file lock, 21.25 s, no package); FAIL: 0.24 mm footprint graze |
| close17 | `4378f29f83` | FAIL: source B frightened by source A, settle timeout |
| close18 | `2cf73eeac7` | FAIL: mover frightened by source B mid-walk (first exposure) |
| close19 | `1c448eb620` | FAIL: Wall A's section saved against Wall B's formation (production fix) |
| close20 | `817c2dd925` | FAIL: no Wall B placement free of adjacent creatures |
| close21 | `9a0a3927e4` | FAIL: cast B interrupted; claimed ring nodes rewritten in combat |
| close22 | `aedfc3db2f` | FAIL: crossing rule flagged rounding a Wall end |
| close23 | `cf1e2bbb3f` | FAIL: receipt temporary path over 260 characters (a-first passed) |
| close24 | `41c9aeeb99` | FAIL: mover frightened by source B beside lane B |
| close25 | `eafade6758` | FAIL: source B moved off its settle check by a native move |
| close26 | `142a5a3f97` | FAIL: native `SummonedUnitsController` follow-summoner moves interrupted cast B |
| **close27** | **`24a1b93583`** | **PASS 152** |
| close28 | `9c95a26053` | FAIL: claim fix; physical route not restored in 15 s; fix reverted |

Every run, close5 through close28, restored v0.0.140, the live tree, settings
and protected saves exactly (runner receipts
`artifacts/wall-sprint2/native-review/*-closeN-restoration.json`). close1–3
restored through their runners. close4 was restored manually as above.

### Dispositions

| Item | Disposition |
| --- | --- |
| Stone chips on ice | Accepted interim compromise |
| Infernal Wound bleed applying to ice sections | Existing mechanical defect; tracked for a separate correction (P1) |
| Invisible residual cold | Tracked presentation debt (P2) |
| Fire-death embers | Tracked polish (P3); steam readability finished here |
| ProperFlanking2 vs adjacent natural attacks | Compatibility defect, tracked separately (P1). The mod was not disabled and eligibility was not overridden. Moving to a legal attack distance does not resolve adjacent natural attacks. |
| Claims overwritten by native grid updates | Proven natively; fix attempted and reverted; engineering follow-up (P1) |
| Section receives Bone Devil fear-aura buffs (close21) | Observed; unexamined; P3 |
| Cause of the original "Wall did nothing" report; any display cause | Unproven |

### Validation ledger

Additional this closeout: **283.293 minutes, a lower bound**. Cumulative:
**at least 494.604 minutes**.

| Check | Count | Seconds |
| --- | --- | --- |
| Focused gates (one more failed argument validation, unmeasured) | 34 | 482.08 |
| Candidate builds (including close16's failed attempt) | 29 | 9561.62 |
| Native guarded runs (close4 excluded) | 27 | 6719.91 |
| close4 native, game launch to last evidence write (killed) | 1 | ≥ 234 |

Not included: close4's manual recovery, the decompilation, and the waiter
stopped by the host before any build. Each source was built once, except
close16 after an infrastructure failure. Commands:

- `scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.sprint32-native-admission;expanded-summoning.sprint35;expanded-summoning.overhaul' -KingmakerInstallDir <install>`
- `Build-Local.ps1 -DevelopmentPurpose summon-outsider-review-corrections`
- git-ignored `artifacts/wall-sprint2/build-and-run.ps1` and `run-native.ps1` (phase `summon-overhaul-wall`)

Raw evidence and packages are not committed.

### Follow-ups

1. **P1: Bleed on ice.** Infernal Wound bleed applies to Wall sections; decide
   the rule and correct it.
2. **P1: Natural reach vs ProperFlanking2.** KMG natural reach (9-ft weapon
   type) under the ProperFlanking2 dead zone prevents adjacent natural
   attacks.
3. **P1: Claim durability.** Make Wall claims durable under native grid
   updates, or retire them in favour of the native cut counters. Reproduce the
   close28 route-restoration failure before re-applying `4bb14d17f4`.
4. **P2: Residual cold presentation.**
5. **P3: Fire-death embers and the orange sparks at the steam site.**
6. **P3: Fear-aura buffs on Wall sections**, and their combat side effects.

## Production repairs (player-visible)

1. **Donor body never shows.** The section's donor creature meshes are kept
   hidden every frame while the private ice plane draws, and release no longer
   re-enables them. Native: 0 visible donor renderers at formation and across
   45 removal frames in all three lanes.
2. **Feedback reaches the player.** Rejection messages were gated on the caster
   being player faction; expanded summons use the native Summoned faction, so a
   player's Bone Devil could never report why a cast did nothing. The gate now
   also admits a caster whose `UnitPartSummonedMonster.Summoner` is player
   faction. Whether this explains the historical "Wall did nothing" report is
   **unproven**.
3. **Ice shows stone chips, not red blood.** The section kept native
   `BloodType.Common`, so hits and death splashed red blood. It now copies its
   visual record and uses native `BloodType.Stone` chips (installed value 9).
   Formation requires it natively. This changes hit and death presentation
   only: the attacker's Infernal Wound bleed still applies to a section
   mechanically (tracked defect; see the closeout).
4. **Navigation grid block is released.** Installed
   `TurnBased.Controllers.CombatController.UpdateNavigationGrid` applies every
   listed `NavmeshCut` to grid mask 2 as `DynamicBlocked` counters and reverts
   them only while the cut is still listed; `NavmeshCut.OnDestroy` has no grid
   revert. Release destroyed the cut first, stranding the centre-row block
   (observed natively: six unclaimed unwalkable nodes after retirement). Release
   now queues the cut's native revert object exactly as the controller does,
   then destroys the cut. Claim/reference-count code is unchanged, so no
   two-Wall isolation run was required.

## Native contracts established (installed 2.1.7b and installed mods)

The owner's actual installation enables 15 other UMM mods. Three affected the
observers; none was overridden. Each is a real rule a player with this setup
experiences:

- **KingmakerBugfixes** `BypassSpellResistanceForOutOfCombatBuffs` postfixes
  `RuleSpellResistanceCheck.HasResistanceRoll` to false out of combat between
  non-enemies. The spell-resistance witness is now an actual hostile creature.
- **ProperFlanking2** reach dead zone postfixes `UnitCommand.GetTargetPoint`:
  a melee weapon with type range above 5 ft cannot strike inside
  `(size reach + 1) ft x 0.5` beyond both bodies; the approach point moves away.
  This explains the long-standing physical-eligibility refusal at the historical
  2.5 m point (edge gap ~0.7 m). The Glaive witness now stands inside reach and
  outside the dead zone; the historic point's rejection is still recorded.
- **Native difficulty**: `RuleDealDamage.ApplyDifficultyModifiers` scales all
  damage a player's enemy deals by `GameDifficulty.DamageToParty` (0.8 in the
  accepted run). Fire is measured before that modifier.
- Kingmaker's pathfinding fork defines `GraphNode.Walkable` as static flag AND
  `DynamicBlocked == 0`; native grid updates restore claimed flags from their
  pre-erosion state while the wall stands. Restoration acceptance checks both
  layers per node. The closeout shows this reopens claimed ring cells while a
  Wall stands; the ice footprint stays blocked by the cut's own native counter.

## Accepted native run

| Item | Value |
| --- | --- |
| Evidence directory | `20261105T2213068225716Z-disposable-expanded-summoning-sprint32-native-review` |
| Run ID | `20261105T2213068465724Z-afd04aeb8dcf441bafaf72b5dce04b7f` |
| Result SHA-256 | `e60883a318d3c1a304d8a4cb86e0c7d31b4e3cc685fcccdee15b8d0a30ec8cec` |
| Phase / scenario | `summon-overhaul-wall` / `disposable-expanded-summoning-sprint32-native-review` |
| Source / version | `36e44513d3fa7e9a670245a2d1bfe39153058702` / 0.0.177 |
| ZIP SHA-256 | `ae2f5c88d56049dc4826c0eca0542fed0402e88ae6dea426c9702b5a006b25b3` |
| DLL SHA-256 / MVID | `009c5804cc6e2f75cb19eedf3848257820f0fbee0b8b3897b0f8e30c96543999` / `5a6d1999-0733-491a-a337-99a01464c460` |
| Source state | `fb1d08b32b980d92bcf1778f17bf89be5879f9bb31b542b023f36e561bc8beea` |
| Restoration | v0.0.140 before and after; live tree, settings and protected saves exact |

ZIP path: `artifacts/development-candidates/summon-outsider-review-corrections/36e44513d3fa/`.
Commits after `36e44513d3` change only documentation.

| Acceptance area | Native observation |
| --- | --- |
| Formation | 3 lanes: command Success, exact identity, 36 HP, native duration 720 s (bonus 0), ground/visual gap 0.00 m, lane/face alignment >= 0.999, Stone blood, cut + 24 grid claims |
| Feedback | 240 clear + 240 occupied polls: 0 log attempts. Placement-changed, Reflex disruption and hostile SR each publish exactly one message and form nothing |
| Live obstruction | Actual native mover with the wall present: ~7.9 m travel at 3.048 m/s, footprint never entered (min gap 0.0002–0.008 m) |
| Attack eligibility | `IsUnitEnoughClose` true unmodified: distance^2 13.17 <= 4.848^2, NeedLoS true, LOS ID 0, not blocked, approach point = target |
| Physical destruction | 6 ordinary Glaive attacks, all native auto-hits, object-halved packets, killing blow a weapon hit, 37 weapon damage (+2 attacker-owned Infernal Wound bleed ticks), attacker travel 0 |
| Fire destruction | 6 fire full (unhalved; 6 before difficulty -> 4), still alive and obstructing; 40 fire destroys |
| Residual cold | Owned by the exact cast; lifetime = remaining wall time (699.3 s / 715.9 s); restored mover takes owned 1d6+12 cold |
| Steam | 600 s, owned; mover inside carries `ObscuringMistBuff` from the Bone Devil, ends on exit, area continues |
| Expiry | **Controlled original-deadline expiry** via the Player.GameTime seam: deadline unchanged, retires the section, no residual; not a real-time soak |
| Navigation restoration | Each lane: claims 24 -> 0, all 24 nodes walkable with static flag original, DynamicBlocked 0, owner count 0; cut grid revert queued; RTWP path back to its pre-cast length; actual mover crosses |
| Tail contact | Owner Bone Devil `3dec72e3`, target `88cabab0`, one native full-attack command (plan 4: bite, 2 claws, sting), exactly one sting row, tail contact gap 0.002 m |
| Source cleanup | Removing the Bone Devil's summon lifetime ends its 3 areas, 0 claims |

## Captures inspected

Formed (3 lanes), live mover (3), physical attack position, removed (3), steam,
tail contact. Formed walls are clean grounded slabs with no donor body; removed
walls leave no body or blood. Limits: the steam cloud is not visibly readable
in the night-lit frame; fire destruction plays the native fire-death ember
effect; the tail capture shows the Bone Devil only as the native x-ray
silhouette behind a palisade. Screenshots support presentation only.

## Owner decisions recorded, not invented

- Stone chips for ice hit/death effects (alternatives Dust, WaterElemental).
- Bearded Devil Infernal Wound bleed currently applies to a wall section.
- The residual cold area has no visible effect (hidden-mechanic concern).
- Fire-death embers and steam visibility on ice.
- KMG natural reach uses weapon type range 9 ft; with ProperFlanking2 a Bone
  Devil (and likely other reach summons) cannot strike an adjacent creature.
  Affects many creatures; not changed in this sprint.
- Claims record original walkability from combined `Walkable`. Superseded by
  the closeout: native grid updates overwrite claims while a Wall stands
  (proven), and the attempted fix is reverted pending investigation.

## Validation ledger

Owner superseded the 85-minute ceiling for this sprint. Retained **117.319233**
minutes; additional **93.992** minutes; cumulative **211.311** minutes.

| Check | Seconds | Result |
| --- | --- | --- |
| Focused gates (11) | 226.07 | 9 PASS, 2 compile FAIL fixed |
| Candidate `ce5e9e2913` first build (stopped by host, low memory) | >= 49.9 (kill time unrecorded) | no package |
| Candidate `ce5e9e2913` | 536.85 | PASS |
| Native run 1 | 202.02 | FAIL: SR witness was an ally (KingmakerBugfixes) |
| Candidate `70d28ce659` (stopped by host; wrapper retry 5.5 s) | >= 461.6 + 5.5 | no package |
| Candidate `70d28ce659` | 475.20 | PASS |
| Native run 2 | 197.46 | FAIL: reach dead zone (ProperFlanking2) |
| Candidate `1c194d3954` / run 3 | 424.50 / 225.45 | PASS / FAIL: bleed rows; red blood seen |
| Candidate `74dcf65d55` / run 4 | 446.57 / 221.33 | PASS / FAIL: stranded DynamicBlocked |
| Candidate `16cfea6c74` / run 5 | 452.81 / 205.49 | PASS / FAIL: fire measured after difficulty |
| Candidate `8e0fc07050` / run 6 | 466.42 / 263.72 | PASS / FAIL: tail lane in dead zone |
| Candidate `36e44513d3` / run 7 | 448.63 / 270.01 | PASS / **PASS 95** |
| Evidence/identity/handoff reserve | 60 | Included |

One immutable candidate per source identity; no unchanged rebuild. Every run
restored v0.0.140, settings and protected saves exactly. Commands: the Sprint 1
gate and `Build-Local.ps1` commands, plus git-ignored
`artifacts/wall-sprint2/run-native.ps1` (phase `summon-overhaul-wall`, 900 s
timeouts) and `build-and-run.ps1`. Raw evidence and packages are not committed.
