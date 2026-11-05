# Wall of Ice Sprint 2 — complete

The Bone Devil Wall of Ice passed full native acceptance in one fresh guarded
Kingmaker process on exact executable source
`36e44513d3fa7e9a670245a2d1bfe39153058702`: **PASS, 95 assertions**, covering
feedback, formation, live obstruction, native attack eligibility, physical and
fire destruction, residual cold and steam, controlled original-deadline expiry,
exact navigation restoration after every retirement, and Bone Devil tail
contact. Six earlier guarded runs on intermediate candidates failed; each was
inspected before the next correction and all are retained below.

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
3. **Ice does not bleed.** The section kept native `BloodType.Common`, so hits
   and death splashed red blood. It now copies its visual record and uses native
   `BloodType.Stone` chips (installed value 9). Formation requires it natively.
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
  layers per node.

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
- Latent: claims record original walkability from combined `Walkable`; if the
  cut's grid block ever lands before the claim, a node could be restored as
  blocked. Not observed; fixing it touches claim code.

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
