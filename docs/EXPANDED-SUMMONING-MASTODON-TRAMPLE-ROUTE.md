# Mastodon Trample: normal-play invocation route (RISK-06 / S5 T6)

Status: **investigation accepted (principal review, 2026-11-20); the approved owned Mastodon Trample AI action is
implemented and qualified natively in real-time and turn-based combat (session `20261120T162705Z`, 28 of 28).
Returned for review; S5 T6 and Sprint 5 stay open.** Not merged, not released.

- **Branch:** `codex/risk06-mastodon-trample-route`, draft PR #191, stacked on PR #190 (accepted S5 T5 review head
  `c2867a3aa1`).
- **Epic story:** S5 T6, inventory record RISK-06: "Normal-play access to commanded-only Trample is not established".
- **Implementation:** `09c1f5925e` (action, brain, ledger query, observer), `203b605c10` (unavailable-ability guard).
  Qualified candidate `203b605c108c` (below).
- **Investigation evidence (unchanged):** session `20261120T140731Z-owner-0.33.0-risk06-trample` on candidate
  `921a952978ce`, PASS 12 of 12; two earlier sessions of `bc92642e1907` kept (below).

## Owner decisions (principal review, 2026-11-20)

| Question | Decision |
|---|---|
| Add a Mastodon-owned Trample AI action? | Yes |
| Keep "not threatened" and "target out of reach"? | Yes, initially: Trample is an approach maneuver |
| Additional restriction | Preserve any legal ordinary melee attack against another enemy already within reach |
| Prefer paths through several enemies? | No optimization; incidental contacts use the existing Trample rules |
| Direct summon control, other tramplers | No |

## Implementation (demonstrated below, not only proposed)

- **Ownership.** `KMG.Summoning.Special.Mastodon.Brain` (`7e9e643401714494adf441ffd267ed5a`) is a private copy of the
  native "Dumb Monster Brain" (`5abc8884c6f15204c8604cb01a2efbab`): its components deep-cloned and its actions
  unchanged and in order (`AttackAiAction`, `866ffa6c34000cd4a86fb1671f86c7d8`, base score 1), followed by
  `KMG.Summoning.Special.Mastodon.TrampleAi` (`4d88e6d656064c36a8df3f0e8df1f39f`, type `UngulateTrampleAi`, base score
  5). Only `KMG.Summoning.Unit.Mastodon` uses the private brain; the shared native brain is not modified (1,577 units
  use it). Both identities are appended to `blueprints/blueprints.json` (append-only; development-candidate validation:
  2 appended). Built by `ExpandedSummoningSpecialBuilder.ConfigureMastodonTrampleAi` after the existing Mastodon
  Trample; no other trampler changes.
- **Action.** `UngulateTrampleAi` (`src/KingmakerGunslinger/Summoning/UngulateTrampleAi.cs`) is a
  `BlueprintAiCastSpell` with target type Enemy. It holds no state. Its command is the ordinary native
  `UnitUseAbility.CreateCastCommand(trample, point)`, so the action budget, the overrun delivery, the path checker, the
  contact rule, the attack-of-opportunity or Reflex branch, the round ledger and ordinary cancellation are unchanged.
- **Conservative predicate** (`MastodonTrampleAiPolicy`, pure; `UngulateTrampleAiRuntime`, native reads), enforced in
  `IsAvailable` and `IsTargetValid`. Trample is offered only when every condition holds:
  - the Mastodon is in combat, can act and move, AI movement is not forbidden;
  - it is not engaged (no enemy threatens it: native `UnitCombatState.IsEngaged`);
  - in turn-based combat it has its full round (native `HasFullRoundAction`);
  - no living enemy is within the native reach of any of its melee weapons (`UnitEngagementExtension.IsReach`: bodies,
    weapon reach and line of sight), so any ordinary melee attack available without movement keeps precedence; an
    exception in that test counts as "in reach";
  - the target is a living enemy, not marked unreachable by the native AI, out of reach, smaller than the Mastodon
    (existing size rule), and the round ledger shows it neither claimed nor the round halted;
  - the Trample ability is natively available.
- **Read-only ledger query.** `ContextActionUngulateTrample.ReadLedger` reads the existing ledger's `HasClaim` and
  `IsHalted` for the current round. It never creates a ledger and never claims or halts; the contact action remains
  the only owner of a claim. No parallel AI ledger.
- **Final path.** The native delivery follows a straight segment from the Mastodon to the point. The planner puts the
  point on the line through the target's centre, beyond it by both bodies and 0.5 m, takes the nearest walkable navmesh
  node, and then validates the adjusted segment itself:
  - its length is above the Mastodon's body and at most the ability's own double-move range (no approach before the
    delivery starts; also within the path checker's limit);
  - it crosses the target's contact envelope ahead of the start: closest approach at most half of (both bodies + 1 ft),
    which keeps contact inside the native 45-degree test;
  - it ends beyond the target with the Mastodon's body clear of it, and the endpoint is not inside any other living body;
  - the navmesh trace reaches the endpoint, line of sight holds, and the native `AbilityData.CanTarget(point)` and
    `IsAvailable` accept it.
  A point that snapped beside or short of the target is refused. The plan is recomputed when the command is created;
  if it no longer holds, no Trample is cast and the ordinary attack command is issued instead. Once dispatched, native
  cancellation applies.
- **Unavailable-ability guard (`203b605c10`).** The native selector treats a cast action whose ability is unavailable as
  "expended" and removes it from the unit's available actions, which the game restores only at creation or rest. The
  action therefore refuses in `IsAvailable` whenever the ability is unavailable, before that check runs. Found while
  reviewing the native selector before qualification; no session ran on the earlier build.

## Implementation qualification (session `20261120T162705Z`, candidate `203b605c108c`)

Phase `summon-risk06-trample-ai` (`Sprint39TrampleAiPhase`, replacing the investigation phase), session file
`scripts/compatibility/loader-compat/sessions/risk06-trample-ai.json`, observer `RuntimeTestRunner.Risk06TrampleRoute.cs`.
Every cell creates its Mastodon with the disclosed own-tier fixture helper (Summon Nature's Ally VII single execution;
see the investigation section for what that helper does). During combat the observer issues the Mastodon no command,
ticks no AI and supplies no action: the game's own controllers drive it. Disclosed fixture steps per cell: a Medium
private hostile (100,000 hit points, inactive brain) and the Mastodon placed on a geometry the product's own read-only
planner accepts (start 6.74 m centre to centre; Mastodon body 2.0 m, reach 2.74 m, combat speed 4.06 m/s; planned
path 9.48 m of at most 24.38 m); party actors quieted; ordinary native combat entry; idle fixture turns passed in
turn-based mode; and the arrangement named per cell. Every assertion is affirmative: each cell states the behaviour it
requires and fails otherwise.

| Cell | Arrangement | Required | Observed |
|---|---|---|---|
| Ownership | — | Private brain = native actions + owned action; shared brain unchanged | Exact; one AI action names the Trample; one brain holds it; one unit uses the brain |
| Route inventory | — | Every independently expected Mastodon output resolves, spawns only the Mastodon, not directly controllable; unreadable graphs listed | 32 expected: 6 canonical menu placements (each offered by its native parent menu), 6 retained celestial/fiendish executions (offered by no menu), 20 alternate identities across 7 families (each offered by its family menu). 0 unresolved, 0 unreadable, 0 wrong unit or control flag |
| Positive, real time | Eligible approach | Owned Trample first, executed; same-round suppression; later-round reuse | First command: the owned Trample (ledger unclaimed at start), Success; Reflex DC 29 failed, damage. Returned to the start in the claimed round: the AI issued an ordinary attack approach, no Trample, for the rest of that round. Returned again in the next round: a second owned Trample (ledger unclaimed at start), Success, second save and damage. 2 Trample commands, both from the owned action, 0 cutscene commands |
| Positive, turn-based | Eligible approach | Owned Trample first, full round spent; later-round reuse | Round 1: owned Trample, Success (Reflex passed, half damage); afterwards neither a full round nor a standard action remained. Returned to the start during an idle turn; round 2: owned Trample again, Success (Reflex failed). No other command started between them (see note) |
| Another enemy in reach, real time and turn-based | Unarmed second hostile within the Mastodon's reach (not threatening it) | Trample never selected; ordinary attack on the enemy in reach | Refusal `MeleeTargetInReach`; 0 Trample issued; `AttackAiAction` attacks on the second hostile (2 completed in real time, 1 in its turn) |
| Threatened, real time | Armed adjacent second hostile (native melee weapon, 1.83 m reach) | Trample never selected; ordinary attack | Engaged; refusal `Threatened`; 0 Trample issued; 2 attacks on that enemy |
| Larger target, real time | Target set to the Mastodon's size | Trample never selected; ordinary attack | Refusal `TargetNotSmaller`; 0 Trample issued; 2 attacks on the target |
| No full round, turn-based | Native Staggered condition on the Mastodon | Trample never selected; ordinary attack | Refusal `NoFullRoundAction` (the native ability also reports unavailable); 0 Trample issued; one attack after a move |
| Occupied endpoint, real time | The summoner placed on the planned endpoint | No Trample cast; ordinary attack | Refusal `EndpointOccupied`; 0 Trample issued; 2 attacks on the target |
| Death mid-Trample, real time | The Mastodon killed (`GameHelper.KillUnit`) once the owned Trample had started and acted, before any contact | Command stops; no contact; no claim left | Command ended Interrupt; no save, attack of opportunity or damage; ledger shows no claim; no unfinished command |
| Shared brain after cells | — | Native brain unchanged | Still exactly `AttackAiAction` |

In every cell the owned action was still in the Mastodon's available actions at the end (the unavailable-ability
guard held, including under Staggered), and every unit the cell brought into combat left it before the turn-based
setting was restored (each `-restored` check PASS). Pointer driver: 0 failed reads, 0 displacements.

**Note (turn-based).** After each turn-based Trample, and after the Staggered cell's attack, the log shows an ordinary
`UnitAttack` from `AttackAiAction` ending Success in the same frame without ever starting (`IsStarted` false). No swing
or damage follows: the target's damage events equal its Trample contacts (2 and 2). The same pattern appears in the
investigation sessions; it is recorded as observed, not interpreted further.

**What this demonstrates:** with the conservative predicate, a summoned Mastodon's own native AI now selects and
executes its existing Trample as an approach maneuver in both modes, and keeps the ordinary attack whenever an enemy
is in reach, it is threatened, the target is not smaller, it lacks a full round or the final path is invalid.
Planning never claims the ledger; a claimed target is not trampled again in the same round; a later round reuses it;
death stops the command without contact.

## Investigation (accepted, principal review 2026-11-20)

The sections below record the investigation as accepted; they describe the product before the implementation.

### The ability and its owner (traced)

| Item | Finding |
|---|---|
| Ability | `KMG.Summoning.Special.Mastodon.Trample` (`a1f52aace5124004b76e2351c7023566`, `KMG_Summoning_Special_Mastodon_Trample`), added by Repair R5 MECH-02 |
| Builder | `ExpandedSummoningSpecialBuilder.ConfigureUngulateTrample` (shared with the Aurochs, Bison, Giant Stag Beetle, Woolly Rhinoceros, Triceratops, Brachiosaurus and Elephant tramplers) |
| Shape | A field copy of the native `OverrunAbility` (`1a3b471ecea51f7439a946b23577fd70`): Extraordinary, range double move, **point target only** (no self, friend or enemy target), Standard action made full-round; `AbilityCustomOverrun` (auto-success, every contact) running `ContextActionUngulateTrample`; `AbilityIsFullRoundInTurnBased`; `UngulateTramplePathChecker` (farther than the body, at most twice the current speed) |
| Contact rule | `ContextActionUngulateTrample`: enemy smaller than the Mastodon, once per target per round (round ledger), a target with a legal melee attack of opportunity makes it at -4 (stopping the trample if the Mastodon cannot continue, otherwise full damage and no save), otherwise Reflex DC 29 for half; 2d8+18 from live Strength and hit dice; ordinary command interruption |
| Grant | `KMG.Summoning.Unit.Mastodon` (`e129ffc2768d47c5bee61bb99b0c8703`) `AddFacts`; one grant |
| Routes that spawn the unit | Twelve canonical registered execution graphs, representing six menu placements: the three Summon Monster VII to IX canonical placements (single, 1d3, 1d4+1), the six retained celestial and fiendish executions the builder creates for them, and the three Summon Nature's Ally VII to IX canonical placements. The publisher adds only the six canonical placements to the menus; the six retained executions are unpublished (which is not the same as their blueprint `Hidden` flag). Every one of the twelve sets `IsDirectlyControllable` false. The census scanned the canonical registration dictionary only: separately registered alternate-family children were not scanned. Their source contracts carry the same control setting (the control flag is part of signature validation and unit substitution preserves it); that is a source-derived conclusion, not a per-alternate observation |
| Brain | The native "Dumb Monster Brain" (`5abc8884c6f15204c8604cb01a2efbab`), set by `ExpandedSummoningNaturalBuilder` for natural summons; its only action is `AttackAiAction` (`BlueprintAiAttack`) |
| `SummonPlaytestR5Policy` | Declares the identity only (`MastodonTrampleSymbol`, `NewIdentities`); no AI or control route |

### Routes considered

| Route | Result | Basis |
|---|---|---|
| Native AI (brain actions) | **None found.** The configured brain holds only `AttackAiAction`. Among the 112,518 loaded blueprints (game, Call of the Wild, Races Unleashed, product) no `BlueprintAiCastSpell` names this Trample in its `Ability` field and no brain holds such an action. The scan searched those direct references only; it did not inspect every possible indirect command producer or custom AI implementation | Native census in session (below); `UnitBrain` builds its actions only from `Blueprint.Brain.Actions` |
| Direct player control, real time | **None.** A summon's `IsDirectlyControllable` comes from its spawn action, false for every Mastodon row; native selection and ground commands use only directly controllable units | Native census; 2.1.7b `UnitEntityData.IsDirectlyControllable`, `ClickGroundHandler` |
| Direct player control, turn-based | **None.** `TurnController` starts a unit that is not directly controllable in `Acting` (AI), never `Preparing` (player input) | 2.1.7b `TurnController` |
| A plain native AI cast action | **Not viable as is.** `BlueprintAiCastSpell.GetTargetType` returns Self for an ability that targets neither enemies nor friends, so it would aim Trample at the Mastodon, which Trample cannot target; its `Locators` are fixed scene objects | 2.1.7b `BlueprintAiCastSpell` |
| Existing qualification | **Commanded only.** R5 `mastodon-native-trample-command-mastodon-rtwp` and the Sprint 23 driver build a `UnitUseAbility` themselves, run it and tick the command, animation and movement with brains inactive | `Sprint23NativeTrampleCommand` |

The Mastodon is in `Player.ControllableCharacters` (observed), but that list is not a control route: every selection
and command path also requires `IsDirectlyControllable`.

### Native observation: a normal summon in combat

Phase `summon-risk06-trample-route` (`Sprint39TrampleRoutePhase`), session file
`scripts/compatibility/loader-compat/sessions/risk06-trample.json` (at `921a952978`; phase and file retired in `09c1f5925e`, replaced by `risk06-trample-ai.json`), observer `RuntimeTestRunner.Risk06TrampleRoute.cs`.
This is native summoned-unit combat following disclosed fixture creation. The Mastodon is created by the accelerated
own-tier fixture helper from the Summon Nature's Ally VII single execution: the helper temporarily grants the summoner
that execution, supplies its spell parameters, creates a cutscene command that ignores cooldown, starts and ticks the
execution directly, and removes the summon's appearance buff. This does not establish normal menu selection, the
source slot's debit or the summoning cast's action expenditure. In the owned outdoor native Default fixture, with the
game's own controllers driving it, the Mastodon then fights a Medium private hostile (100,000 hit points, inactive
brain) standing on a surveyed legal Trample path. During that combat the observer issues the Mastodon no Trample
command, ticks no AI and supplies it no action; every command the Mastodon starts is recorded. Disclosed fixture steps:
the summon's creation as above, the hostile's size and placement, ordinary native combat entry (memory and
`JoinCombat`), passing idle fixture turns in turn-based mode.

#### Observed (session `20261120T140731Z`, candidate `921a952978ce`)

**Configured routes (census, live):** twelve canonical registered execution graphs (six menu placements and six
retained celestial and fiendish executions) spawn `KMG_Summoning_Unit_Mastodon`, each with `IsDirectlyControllable`
false; alternate-family children were not scanned. One Trample grant (Standard action, full round); brain "Dumb Monster
Brain" with exactly `AttackAiAction`; among 112,518 loaded blueprints no AI cast action names the Trample in its
`Ability` field and no brain holds one (direct references only).

**Live summon (both modes):** `IsDirectlyControllable` false, `UnitPartSummonedMonster.IsDirectlyControllable` false,
not in the party, not player faction; present in `Player.ControllableCharacters`; live brain "Dumb Monster Brain" with
`AttackAiAction` only, active at spawn. Trample was legal at the start (native `CanTarget` and `IsAvailable` on a
surveyed point beyond the Medium enemy, 3.0 m away) and still available at the end.

| Mode | Window | Commands the Mastodon started | Trample commands | Trample saves on the target | Damage events |
|---|---|---|---|---|---|
| Real time | 18 game seconds (3 rounds) | 3 `UnitAttack` from `AttackAiAction` (Standard), the first two finished Success, the third running at the window's end | 0 | 0 | 5 |
| Turn-based | rounds 1 to 3: three owner turns began, two completed | 2 `UnitAttack` from `AttackAiAction`, one per completed turn (each reported an Interrupt end and a Success end in the same frame) | 0 | 0 | 3 |

In both modes every unit the cell brought into combat (the Mastodon, the hostile, the summoner and the anchor) left
combat before the turn-based setting was restored; the game was not paused afterwards and the native Default lease
released exactly.

**Conclusion (as accepted, 2026-11-20):** no supported normal-play Trample route was found in the inspected
configuration. The observed canonical summon is not directly controllable, and its configured live brain supplies only
ordinary attacks. Supported alternate adapters preserve the validated native control settings through the traced
contracts. The Trample qualified in R5 and Sprint 23 was reached only by a harness command. The 12 of 12 session
completes the investigation; it does not show that Trample is now accessible.

#### Candidates and sessions (investigation)

All sessions: disposable game copy (`game-copies/loader-compat`, UMM 0.33.0 owner profile), the candidate with Call of
the Wild 1.14.4c-2.1 and Races Unleashed 1.0.11, harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`,
save-free, pointer owned by a driver that re-parks it every 50 ms.

| Candidate | Identity | Session | Result |
|---|---|---|---|
| `bc92642e1907` | package `667845a1aa33aeb3e3aed19ceb5153c431820e7a5640b774b6854c1cbf992df5`, DLL `599ae7c076170a0d71e317e547ca934f39b13fd52d8fd50a7d9ed39ff300b0bd`, MVID `65846e7c-27df-4ad8-a8f5-8f11ce861041` | `20261120T135147Z-owner-0.33.0-risk06-trample` | **FAIL**: every RISK-06 cell passed with the findings above; then the native Default lease release refused a foreign Pause mode (stack 2). The pointer driver recorded 1,102 failed reads and 180 displacements, none logged inside the phase window |
| `bc92642e1907` (same bytes) | as above | `20261120T135607Z-owner-0.33.0-risk06-trample` | **FAIL**, identical refusal, with no failed read and no displacement: one matched rerun to classify the first failure. The refusal is the observer's: its teardown left the summoner and anchor in combat, and native turn-based disable pauses a party still in combat. Same Trample findings |
| `921a952978ce` | package `aac4ad516040b2fe52124227f80ccd8e24763f40dd8a2645e85a78daab5e247c`, DLL `387bc9ba2f2ef021ef8a324caf3934af4f261e752f7f5ef61ddc9f0b90595c54`, MVID `275940af-e6f0-47a8-887c-9dd9b52161ec` | `20261120T140731Z-owner-0.33.0-risk06-trample` | **PASS, 12 of 12 in game.** Teardown corrected; findings unchanged; no failed read or displacement |

### The investigation's proposal

The investigation proposed an owned, conservative Trample AI action and asked the owner four questions. The owner's
decisions are recorded above; the approved action is described under "Implementation". The proposal differed in one
point the review corrected: "the selected enemy is out of reach" did not by itself preserve an ordinary attack against
another enemy already in reach, so the implementation refuses Trample whenever any legal ordinary melee target is in
reach.

## Candidates and sessions (implementation)

Same lane as the investigation: disposable game copy, UMM 0.33.0 owner profile, Call of the Wild 1.14.4c-2.1 and Races
Unleashed 1.0.11, harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`, save-free, pointer
re-parked every 50 ms.

| Candidate | Identity | Session | Result |
|---|---|---|---|
| `09c1f5925efb` | package `2bda9cf320a21367531340c040d042fb5caf3757a85953b4f8d7de3e9bc75e28`, DLL `76df4af5a856976717cfb33a1cd73bc4cd53418193621841708a0b72b3d9e574` | none | **Superseded before any run**: built, then the unavailable-ability hazard was found in the native selector; never staged or launched |
| `203b605c108c` | package `7391dc81475df8a01bd4d0ec929d4006f72d6c08f926280e7e17bf14458e1c48`, DLL `d900c900ed8a04d2a4150e2716d8e2c7882719fdf61c6cf764d189ad95a97fc7`, MVID `5b0eb1b8-17f5-4062-bbd0-c99aada6b56d` | `20261120T162705Z-owner-0.33.0-risk06-trample-ai` | **PASS, 28 of 28 in game** (244 s); no failed pointer read or displacement |

## Checkpoint record

### Focused checks

| Check | Result |
|---|---|
| `Invoke-KmgGate` (production compiled), filter `risk06-mastodon-trample-ai;expanded-summoning.sprint32-native-admission;summon-playtest-r5;native-profile;identity-catalog;correction-ledger`, at `09c1f5925e` and `203b605c10` | 23 of 23 each |
| New domain tests `risk06-mastodon-trample-ai.*` | 5 of 5: actor predicate (including another enemy in reach without threat), target predicate, planning never claims (repeated reads, duplicate suppression, later-round reuse, halt), final segment (through, beside, short, away, too long, too short, lateral offset either side of half the envelope), brain composition |
| Broader focused run (`expanded-summoning;summon-playtest;native-profile;primate;alternate;manifest;icon` plus the new tests) on the product and test source of `09c1f5925e`, before the observer rewrite | 831 of 831 |
| Complete domain suite on the `09c1f5925e` source (run twice) | 3,147 of 3,148. The one failure is pre-existing and outside this story: `model-d.ItemIdentityEconomicsEnchantmentsAndCraftingUnchanged` pins `src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs` to its 0.0.142 hash, and RISK-01 (`0c0a211e62`, S4 T5, already in the accepted S5 T5 base) changed that file; the file is byte-identical to its committed version here. Not changed by this branch; recorded for the owner |
| Candidate development filter (the gate filter plus `loader-compat`) | 28 of 28 (3,148 registered); strict standalone package validation PASS |
| `tools/expanded_summoning_manifest.py --allocate --activate` | PASS; 2 identities appended (7,935 total); roster ledger current |
| `tools/validate_icon_catalog.py`; `tools/validate_development_candidate.py --purpose risk06-trample-ai` | PASS; registry receipt refreshed; 2 appended, no released row changed |
| `Test-KmgSprint32ReviewPhaseScope` | `summon-risk06-trample-ai` admitted; the retired `summon-risk06-trample-route` and an unknown phase refused |
| `git diff --check` | clean |

Investigation checks (unchanged): gate 17 of 17 at `bc92642e19` and `921a952978`; candidate filters 22 of 22.

## Limits

- **Geometry.** One surveyed geometry, reused by every cell (a single Medium enemy 6.74 m away on open ground).
  Groups of enemies, other target sizes than Medium and Huge, slopes, doors and narrow terrain were not observed.
  Paths through several enemies are not optimized (owner decision); incidental contacts follow the existing rules but
  were not observed here.
- **Creation route.** Every cell's Mastodon came from the accelerated own-tier fixture helper (the Summon Nature's Ally
  VII single execution). Menu selection, slot debit and the summoning cast's action expenditure are not established;
  the other outputs share the unit and its brain (route inventory) but were not cast.
- **Not separately observed:** a stale plan between command creation and start (the plan is recomputed at creation and
  the real-time selector re-plans while the command waits; covered by code and domain tests only); expiry of the summon
  (death was observed); a stopping attack of opportunity during an AI-issued Trample (the AoO branch and ledger halt are
  the existing, previously qualified rule; the halt is read by the planner and covered by domain tests); the
  threatened case in turn-based mode.
- **Fixture targets.** The hostiles have inactive brains and the primary target made no attacks of opportunity (no
  threatening weapon); the Reflex branch was observed, the attack-of-opportunity branch was not.
- **Saves.** Summons saved before this change keep their saved brain actions (attack only) until re-summoned; this
  follows the native save of `UnitBrain.Actions` and was not observed.
- **Scope.** Mastodon only; the other tramplers (Aurochs, Bison, Giant Stag Beetle, Woolly Rhinoceros, Triceratops,
  Brachiosaurus, Elephant) keep their existing behaviour and were not examined. The owner installation and its 15-mod
  stack were not used.
