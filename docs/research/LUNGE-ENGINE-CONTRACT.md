# Lunge installed-engine contract — 2026-10-05

Mission base: `6a4dc1b26350c1045171b42099ef2ac5584e303d`.

Classification: **BLOCKED-NO-SEPARABLE-SEAM**, specifically the frozen RTwP
turn-timing contract. Attack and threat ranges themselves are separable.
No production Lunge state, toggle, buff, feature, patch or acquisition is added.
This is an evidence-backed intake gate, not gameplay qualification or a claim
that a differently specified future RTwP adaptation is impossible.

## Exact installed authority

Installed `Assembly-CSharp.dll`:

- MVID: `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.
- SHA-256: `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
- Evidence: local, ignored `artifacts/inspection/bodyguard-native/Assembly-CSharp.il`,
  generated with the already installed ILDASM `/text /nobar /utf8`.
- Exact method bodies and member metadata were inspected; no proprietary assembly
  or raw disassembly is committed. Reflection's whole-assembly type enumeration
  left six unloaded dependency types; the complete IL, rather than that partial
  enumeration, is the authority for absent flight/turn interfaces.

## Reach, command and execution map

| Responsibility | Exact installed contract | Observed ordering and implication |
|---|---|---|
| Target faction legality | `UnitEntityData.CanAttack(UnitEntityData): bool`, 33 IL bytes | Checks neutral faction or native group enemy relation; no distance mutation needed |
| Construct command | `UnitAttack.CreateAttackCommand(UnitEntityData, UnitEntityData)` and `UnitAttack.Init` | Ordinary attack commands are separate from abilities and `UnitAttackOfOpportunity` |
| Final selected weapon range | `ItemEntityWeapon.AttackRange: Feet`, getter 34 bytes | Calls wielder `UnitDescriptor.GetWeaponRange(BlueprintItemWeapon)`; unwielded fallback uses exact weapon blueprint range |
| Lawful reach stat | `UnitDescriptor.GetWeaponRange`, 35 bytes; `CharacterStats.ReachRange`, 25 bytes | Adds blueprint attack range and native reach-stat contribution `max(Reach.ModifiedValue - 5, 0)` feet |
| Selected full-attack hand | `AttackHandInfo(WeaponSlot, int, int)`, 103 bytes | Caches that exact wielded weapon's already calculated metres in `WeaponRange`; additional limbs use the same hand representation |
| Approach/execution radius | private `UnitAttack.GetApproachRadius(UnitEntityData): float`, 130 bytes | Adds both live views' corpulence, then planned attack's cached range; falls back to executor's first actual weapon only when no planned attack exists |
| Range refresh | private `UnitAttack.UpdateTarget(): bool`, 533 bytes | Recomputes `ApproachRadius` before target life and `IsUnitEnoughClose` checks; retargeting stays native |
| Geometry/LOS | `UnitCommand.IsUnitCloseEnough(Vector3, Vector3, Vector3, float, bool, int): bool`, 38 bytes | Uses squared mechanics distance against radius, then native LOS; native command approach/pathing consumes its approach radius |
| Full-attack continuation | `UnitAttack.TryStartNextAttack(bool): bool`, 162 bytes | Subsequent attacks run `UpdateTarget`, select the next planned hand, and start animation or schedule native act |
| Real attack execution | `UnitAttack.OnAction(): ResultType`, 168 bytes; `TriggerAttackRule(AttackHandInfo)` | Uses native attack rule, advances attack index, and retains subsequent full-attack entries |
| Native threat | `UnitEngagementExtension.GetThreatHand`, 166 bytes; `IsReach` overloads, 64/75 bytes; `IsEngage`, 59 bytes | Separately reads native `ItemEntityWeapon.AttackRange` plus both corpulences, LOS, ability to act and concealment |
| AoO | `UnitCombatState.AttackOfOpportunity` and separate `UnitAttackOfOpportunity : UnitCommand` | Does not inherit `UnitAttack` or its private approach calculation; native threat and AoO must remain untouched |

A conditional postfix on **only** `UnitAttack.GetApproachRadius`, using the exact
planned melee hand and per-owner state, is a plausible future +`5.Feet()` seam.
It would preserve native weapon, natural, reach-stat and corpulence calculations
without mutating any blueprint, global reach or threat function. It is **not
installed or runtime-qualified here**. This map does not claim that all command,
pathing, full-attack and AoO assertions pass in game.

Ranged/firearm/ray/spell/ability paths would require exclusion before changing a
radius. Combat maneuvers often arrive through `UnitUseAbility`/context actions,
not this command's exact hand/radius contract. They remain excluded and
unqualified; no autonomous maneuver adaptation is chosen.

## Turn and attack-attempt map

Turn-based mode has a genuine native owner and discrete boundary:

- `TurnBased.Controllers.CombatController.CurrentTurn` points to a
  `TurnController` with exact `Unit` and `Status`.
- `TurnController.Prepare(): void`, 622 bytes, clears native action cooldowns,
  calls `UnitCombatState.OnNewRound`, raises `IUnitNewCombatRoundHandler`, ticks
  the unit's `ITickEachRound` fact components, and raises
  `ITurnBasedModeHandler.HandleTurnStarted(UnitEntityData)`.
- Private `TurnController.End(): void`, 62 bytes, commits native cooldowns,
  marks `TurnStatus.Ended` and interrupts owned commands. Querying current owner
  plus status could deny extended range after end without changing threat.

RTwP exposes **two different clocks**, not the same discrete turn contract:

1. `UnitCombatCooldownsController.TickOnUnit(UnitEntityData)`, 303 bytes,
   remembers whether `Cooldown.StandardAction > 0`, decrements native action
   cooldowns, and calls `OnNewRound`/raises `IUnitNewCombatRoundHandler` only
   when a previously positive Standard cooldown reaches zero. Idle/no-attack
   activation at zero cannot produce a later transition from this route.
2. `UnitTicksController.TickOnUnit(UnitEntityData)`, 83 bytes, increments
   `UnitEntityData.TimeToNextRoundTick` by native delta and subtracts six seconds
   at the boundary; private `TickNextRound`, 59 bytes, ticks AI and
   `ITickEachRound` fact components. This periodic mechanic clock has no test
   for a running attack command or Standard cooldown and is independent of
   attack commitment. In native turn-based combat this route returns early.

`UnitCombatState.OnNewRound(): void`, 15 bytes, resets `HitThisRound` and
`ExecutedAttackNumber`. That execution counter is not a before-attempt ledger.
It cannot prove canceled or invalidated committed commands.

For attack commitment, `UnitCommand.Start(): void`, 174 bytes, first rejects a
removed target or insufficient distance, then marks `IsStarted`, calls virtual
`OnStart`, and raises `IUnitCommandStartHandler`. This is too late for a command
canceled after acceptance while approaching. `UnitCommands.Run` has a 10-byte
public wrapper and a 740-byte private implementation: it initializes/restricts
and installs the command, then raises `IUnitRunCommandHandler`. Its merge-return
and queue paths have different event timing. `IUnitWantRunCommandHandler` is a
pre-acceptance hook. Neither a skill/attack rule nor a target preview substitutes
for a demonstrated exact accepted-attempt boundary across all those paths.

## Why the combined gate is blocked

The frozen contract simultaneously requires all melee subattacks throughout one
own turn, end-of-turn reach expiry, next-own-turn AC expiry, no-attack penalty
commitment, and lockout after canceled/invalidated accepted attack attempts.

- Using Standard cooldown completion alone has no next boundary for activation
  followed by no attack and cannot independently identify an end-of-turn interval.
- Using only periodic six-second fact ticks can end the effect during a native
  full-attack command: its timer is independent of that command's start and end.
- Using command end as turn end excludes later legal commands within the same
  native periodic round; holding until the next command instead retains state
  through idle time and lacks the frozen no-attack boundary.
- Mixing the two clocks requires defining new precedence and a new meaning of
  RTwP "own turn", including the full attack that straddles a periodic tick.
  That is an additional behavioral adaptation, not evidence that the frozen
  interpretations are preserved. A private timer anchored to toggle activation
  would likewise invent a turn boundary.

No exact native route was found that resolves this combined timing/commitment
contract under the permitted range-only patch surface. A broad combat-turn,
command, movement or threat rewrite is forbidden. The mission's hard-stop rule
therefore applies **before production binding**. No turn model is presented as
real RTwP qualification, and no abbreviated two-mode runtime PASS is claimed.

## AC and publication boundary

`ModifierDescriptor.Penalty` exists (native value `0x24`), as do the native AC
stat and `ModifiableValueArmorClass.Touch`, `FlatFooted` and
`FlatFootedTouch`. `AllowedForTouch` (31 bytes) and `AllowedForFlatFooted`
(21 bytes) admit negative modifiers before excluding positive armor/shield or
Dodge categories. This supports a future real -2 penalty on derived AC; stacking,
expiry, unconsciousness/death and duplicate activation remain runtime obligations.
No AC modifier is installed in this mission.

Future BAB +6 prerequisite construction, feature graph, icons and selection
publication remain deferred. There is no Lunge identity, icon, localization,
setting, race/feat grant or ordinary save-visible acquisition path.
