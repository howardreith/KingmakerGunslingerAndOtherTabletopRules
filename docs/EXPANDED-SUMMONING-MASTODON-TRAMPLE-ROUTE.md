# Mastodon Trample: normal-play invocation route (RISK-06 / S5 T6)

Status: **investigation complete; no normal-play route exists. An owned AI action is proposed below for review; nothing
is implemented and no action priority has changed.** Not merged, not released.

- **Branch:** `codex/risk06-mastodon-trample-route`, draft PR #191, stacked on PR #190 (accepted S5 T5 review head
  `c2867a3aa1`).
- **Epic story:** S5 T6, inventory record RISK-06: "Normal-play access to commanded-only Trample is not established".
- **Gameplay source:** unchanged. This branch adds only an investigation observer, its phase and session, and records.
- **Evidence:** session `20261120T140731Z-owner-0.33.0-risk06-trample` on candidate `921a952978ce`, PASS 12 of 12; two earlier
  sessions of `bc92642e1907` kept (below).

## The ability and its owner (traced)

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

## Routes considered

| Route | Result | Basis |
|---|---|---|
| Native AI (brain actions) | **None found.** The configured brain holds only `AttackAiAction`. Among the 112,518 loaded blueprints (game, Call of the Wild, Races Unleashed, product) no `BlueprintAiCastSpell` names this Trample in its `Ability` field and no brain holds such an action. The scan searched those direct references only; it did not inspect every possible indirect command producer or custom AI implementation | Native census in session (below); `UnitBrain` builds its actions only from `Blueprint.Brain.Actions` |
| Direct player control, real time | **None.** A summon's `IsDirectlyControllable` comes from its spawn action, false for every Mastodon row; native selection and ground commands use only directly controllable units | Native census; 2.1.7b `UnitEntityData.IsDirectlyControllable`, `ClickGroundHandler` |
| Direct player control, turn-based | **None.** `TurnController` starts a unit that is not directly controllable in `Acting` (AI), never `Preparing` (player input) | 2.1.7b `TurnController` |
| A plain native AI cast action | **Not viable as is.** `BlueprintAiCastSpell.GetTargetType` returns Self for an ability that targets neither enemies nor friends, so it would aim Trample at the Mastodon, which Trample cannot target; its `Locators` are fixed scene objects | 2.1.7b `BlueprintAiCastSpell` |
| Existing qualification | **Commanded only.** R5 `mastodon-native-trample-command-mastodon-rtwp` and the Sprint 23 driver build a `UnitUseAbility` themselves, run it and tick the command, animation and movement with brains inactive | `Sprint23NativeTrampleCommand` |

The Mastodon is in `Player.ControllableCharacters` (observed), but that list is not a control route: every selection
and command path also requires `IsDirectlyControllable`.

## Native observation: a normal summon in combat

Phase `summon-risk06-trample-route` (`Sprint39TrampleRoutePhase`), session file
`scripts/compatibility/loader-compat/sessions/risk06-trample.json`, observer `RuntimeTestRunner.Risk06TrampleRoute.cs`.
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

### Observed (session `20261120T140731Z`, candidate `921a952978ce`)

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

### Candidates and sessions

All sessions: disposable game copy (`game-copies/loader-compat`, UMM 0.33.0 owner profile), the candidate with Call of
the Wild 1.14.4c-2.1 and Races Unleashed 1.0.11, harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`,
save-free, pointer owned by a driver that re-parks it every 50 ms.

| Candidate | Identity | Session | Result |
|---|---|---|---|
| `bc92642e1907` | package `667845a1aa33aeb3e3aed19ceb5153c431820e7a5640b774b6854c1cbf992df5`, DLL `599ae7c076170a0d71e317e547ca934f39b13fd52d8fd50a7d9ed39ff300b0bd`, MVID `65846e7c-27df-4ad8-a8f5-8f11ce861041` | `20261120T135147Z-owner-0.33.0-risk06-trample` | **FAIL**: every RISK-06 cell passed with the findings above; then the native Default lease release refused a foreign Pause mode (stack 2). The pointer driver recorded 1,102 failed reads and 180 displacements, none logged inside the phase window |
| `bc92642e1907` (same bytes) | as above | `20261120T135607Z-owner-0.33.0-risk06-trample` | **FAIL**, identical refusal, with no failed read and no displacement: one matched rerun to classify the first failure. The refusal is the observer's: its teardown left the summoner and anchor in combat, and native turn-based disable pauses a party still in combat. Same Trample findings |
| `921a952978ce` | package `aac4ad516040b2fe52124227f80ccd8e24763f40dd8a2645e85a78daab5e247c`, DLL `387bc9ba2f2ef021ef8a324caf3934af4f261e752f7f5ef61ddc9f0b90595c54`, MVID `275940af-e6f0-47a8-887c-9dd9b52161ec` | `20261120T140731Z-owner-0.33.0-risk06-trample` | **PASS, 12 of 12 in game.** Teardown corrected; findings unchanged; no failed read or displacement |

## Proposal for review: an owned Trample AI action (not implemented)

Because no route exists, a normal summoned Mastodon never tramples. The concrete proposal:

1. **Action.** One owned `BlueprintAiCastSpell` subclass (`UngulateTrampleAi`, precedent `KytonGazeAi34`,
   `InfernalSpellAi35`), naming the existing Mastodon Trample, added to a private copy of the Mastodon's brain after
   the unchanged `AttackAiAction` (the same pattern as the Crocodilian Sprint brain). Its command is the ordinary native
   `UnitUseAbility.CreateCastCommand(trample, point)`, so the native action budget (full round; in turn-based
   `AbilityIsFullRoundInTurnBased`), movement, the overrun delivery, `UngulateTramplePathChecker`, the contact action,
   the attack-of-opportunity or Reflex branch, the once-per-target round ledger and ordinary interruption are all
   unchanged. No damage, save or ledger code changes.
2. **Target point.** Target type Enemy (the native Self default cannot work). For the chosen enemy the action computes
   one point beyond it on the line from the Mastodon (enemy edge plus the Mastodon's body), clamped to a walkable
   navmesh node, and uses it only if a navmesh trace reaches it, line of sight holds, the native
   `AbilityData.CanTarget(point)` and `IsAvailable` accept it, and the distance is within the path checker's twice-speed
   limit.
3. **Conservative predicate (for review before any priority change).** Trample is offered only when all hold:
   - the Mastodon is in combat, not engaged (no enemy currently threatens it), and in turn-based mode has its full
     round available;
   - the chosen enemy is out of the Mastodon's melee reach, so the alternative would be to move rather than to make a
     full attack;
   - the enemy is eligible under the existing size rule (smaller than the Mastodon) and not already claimed in the
     current round's ledger;
   - the path above is legal.
   Otherwise the ordinary `AttackAiAction` decides as today. The predicate is enforced in the action's `IsAvailable`
   and `IsTargetValid`, so the action exists only while it holds; its static `BaseScore` would then sit above
   `AttackAiAction`'s. An `IsEngagedConsideration` (precedent: Crocodilian Sprint) can express "not engaged".
4. **Not proposed:** direct summon control; other tramplers (the same action could later serve them, each by its own
   decision); changes to the Trample rule; a Stampede AI.
5. **Qualification, if approved:** native real-time and turn-based observations of a normal summon: invocation without
   a harness command; full-round cost and movement; a legal path and its contacts; an ordinary attack when no legal path
   exists (engaged, enemy in reach, larger enemy, blocked path); interruption; no duplicate damage to a target in a
   round; later-round reuse; no lingering state after death or expiry. Domain tests for the predicate and the point
   computation.

Open decisions for the owner: whether to add the action at all; the predicate's conditions (in particular "not
engaged" and "enemy out of reach"), which make Trample rare in melee; whether a trample through several enemies should
be preferred over a single one.

## Checkpoint record

### Focused checks

| Check | Result |
|---|---|
| `Invoke-KmgGate -Filter 'expanded-summoning.sprint32-native-admission;summon-playtest-r5;native-profile'` (production compiled), at `bc92642e19` and `921a952978` | 17 of 17 each |
| Each candidate's development filter (the above plus `loader-compat`) | 22 of 22 (3,143 registered); strict standalone package validation PASS |
| `Test-KmgSprint32ReviewPhaseScope` | `summon-risk06-trample-route` and `summon-pol06-cotw` admitted; an unknown phase refused |
| `git diff --check` | clean |

## Limits

- One geometry per mode (a single Medium enemy about 3 m away on a legal path); larger groups, other sizes and terrain
  were not observed. The finding does not depend on geometry: no AI action or control route can issue Trample.
- Only the Summon Nature's Ally VII single row was cast; the other eleven rows share the unit and spawn flags (census).
- The owner installation and its 15-mod stack were not used; a third-party mod that makes summons controllable or
  edits their brains would be a different, unsupported route.
- The other tramplers (Aurochs, Bison, Giant Stag Beetle, Woolly Rhinoceros, Triceratops, Brachiosaurus, Elephant)
  share the Trample carrier; their brains and routes were not examined or observed here.
