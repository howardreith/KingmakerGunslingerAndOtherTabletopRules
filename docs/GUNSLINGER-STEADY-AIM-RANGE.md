# Steady Aim: the armed shot is resolved on the attack roll (NEW-03)

Status: **development repair on a review branch. Not merged, not released.** Branch
`codex/new03-steady-aim-attack-roll`, stacked on draft PR #169 at its reviewed head `7fee25403d`. The published
v0.0.191 does not contain it. Nothing here is a release qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: finding NEW-03, recorded while DEF-02 was
worked (`docs/GUNSLINGER-EFFECT-FREE-BUFF-LINKS.md`, finding 2). Current status is kept in
`planning/GUNSLINGER-REMEDIATION-STATUS.md`.

## What was wrong

Steady Aim armed, and then did nothing. No shot consumed the armed state and no shot gained its 10 feet; the state
ran out after one round.

The armed buff carried a handler on `RuleAttackWithWeapon` that did its work in `OnEventAboutToTrigger` and read
`evt.AttackRoll`. Kingmaker 2.1.7b creates that roll inside the rule's own trigger:

```
Rulebook.TriggerEventInternal(evt)
    RulebookEventBus.OnEventAboutToTrigger(evt)      <- the old handler ran here; AttackRoll is null
    evt.OnTrigger(context)
        RuleAttackWithWeapon.OnTrigger
            AttackRoll = Rulebook.Trigger(new RuleAttackRoll(...))   <- assigned only once the roll is resolved
    RulebookEventBus.OnEventDidTrigger(evt)
```

The handler returned on the null roll every time. The property is assigned from the return value of the inner
trigger, so the weapon attack rule never exposes a roll that is still unresolved: before its trigger there is none,
and after it the armor class has already been chosen. No callback on that rule can change the roll's range.

## The change

`SteadyAimRuntime.BeforeAttackRoll` resolves the shot from the mod's existing prefix on `RuleAttackRoll.OnTrigger`
(`RuleAttackRollFirearmPatch.Prefix`), which already runs the firearm steps in a fixed order:

```
FirearmDischargeRuntime.BeforeAttackRoll     decides whether this roll fires a loaded round
SteadyAimRuntime.BeforeAttackRoll            new
DeadeyeRuntime.BeforeAttackRoll              reads the roll's effective range
FirearmArmorClassRuntime.BeforeAttackRoll    captures the roll's range for armor-class selection
... RuleAttackRoll.OnTrigger                 triggers RuleCalculateAC, then rolls
```

Why this place and not another:

- The real `RuleAttackRoll` exists here, and `RuleCalculateAC` is triggered later, inside the method the prefix
  precedes. The bonus is registered before either consumer reads it.
- It has to follow the discharge step. That step is what marks a roll as an actual shot (a loaded round fired through
  the ordinary path). Dead Shot's probe and delivery rolls, scatter deliveries and an empty chamber are not marked.
  Steady Aim uses the same mark Deadeye uses, so a computational or auxiliary roll cannot consume the state.
- A handler moved to `RuleAttackRoll` on the buff would run in the event bus, before the prefix and therefore before
  the discharge step. It would be early enough for range and too early to know whether the roll is a shot. Changing
  the handler's event type alone would not have been correct.

A qualifying shot removes the armed state once. If the deed's grit requirement still holds it registers 10 feet in
the existing per-roll range context (`EffectiveFirearmRangeRuntime`), which is keyed to that one attack roll; the
weapon definition is not touched. A roll that has already been resolved is ignored if it is observed again. Any fault
leaves the shot without a bonus and is logged.

The decision itself is `SteadyAimPolicy.ResolveShot`, which has no engine types and is covered by the domain tests.

The buff keeps its `SteadyAimAttackHandler` component, now a plain holder of the grit references; it no longer
subscribes to a rule. The buff's GUID, name, text, duration (6 seconds), stacking and effect links are unchanged, and
so are the ability, its move action, its availability rule and its cost of no grit.

## Contract, as implemented

| Rule | Where it is decided |
|---|---|
| Arming is a move action, spends no grit, needs positive grit (selected True Grit exempt), unavailable while armed | `SteadyAimAbilityLogic`, unchanged |
| Armed state lasts 6 seconds unless used | `SteadyAimAbilityLogic.Deliver`, unchanged |
| Qualifying shot: exact firearm, one marker, two-handed kind (Musket, direct Blunderbuss, Rifle), not scatter | `SteadyAimPolicy.IsQualifyingShot`, unchanged |
| Only a roll that fired a loaded round is a shot | `FirearmMisfireRuntime.IsEligibleAttack`, as for Deadeye |
| A qualifying shot consumes the state once, hit or miss | `SteadyAimPolicy.ResolveShot`, `SteadyAimRuntime` |
| Exactly 10 feet for that roll, only while the grit requirement holds | `SteadyAimPolicy.ResolveShot` |
| No grit is spent by arming or firing | nothing in the path spends grit |

## Focused tests

`.\scripts\Invoke-KmgGate.ps1 -Filter 'archetype-;deadeye;true-grit;firearm-armor;loader-compat-probe'`: production
compiled; 49 of 3,001 registered tests selected, 0 failures (18 s). Three are new, in `ArchetypeFoundationTests`:

- `archetype-steady-aim.shot-resolution`: exactly 10 feet for Musket, direct Blunderbuss and Rifle; the state is
  consumed with no bonus when the grit requirement no longer holds; a pistol, a non-firearm, two markers, an unknown
  kind, a scatter delivery and a roll that fired no round each leave the state and gain nothing; an unarmed shot
  gains nothing; a bonus without consumption cannot be represented.
- `archetype-steady-aim.grit-requirement`: through the production True Grit service, available with positive grit
  at no cost, unavailable with none, available with none when True Grit is selected.
- `archetype-steady-aim.range-reaches-deadeye-and-armor-class`: at 45 feet with a musket, the resolved bonus makes
  armor-class selection choose touch and makes Deadeye read the first increment; the three cases without a bonus
  leave ordinary armor class and Deadeye's second increment; the musket's own increment stays 40.

These exercise production policy code. They cannot see the event order, which is why the order is checked in the
game.

## Native evidence

One session, `scripts/compatibility/loader-compat/sessions/steady-aim-range.json`, in the disposable game copy with
the fixture save `KMCS_AFTER_NORMAL_012_MODONLY.zks`, loader profile `owner-0.33.0`, real-time play.

Fixture: a Musket Master levelled by the level-up controller (grit 5) with a musket, 45.0 ft from a hostile target
on a straight walkable line. The target's ordinary armor class is 18 and its touch armor class 10 (flat-footed
values the same), it cannot move and has hit points to spare. The game's AI is off for the three disposable units.
Steady Aim is armed through the game's ability command. Shots go through the game's attack command
(`UnitAttack`), whose own `RuleAttackWithWeapon` and attack roll are the ones recorded; three cases that have to
resolve at once use the weapon attack rule alone and are marked so.

**The distance the game used.** The attack command walks the shooter into the musket's 40 ft range, which the game
measures between the two units' edges. The product's armor-class rule measures between their centres
(`UnitEntityData.DistanceTo`). From the 45 ft start every shot by the shooter was resolved at **43.2 ft**.

Matched pair, same harness bytes:

| Shot | Before the repair (`20261115T234729Z`) | With the repair (`20261115T234533Z`) |
|---|---|---|
| Control, not armed, attack command | 43.2 ft, no bonus, ordinary AC 18, "Normal AC (43.2 ft.)" | the same |
| Armed, natural 19, attack command | no bonus, **ordinary AC 18**, state still on the shooter, 0 removal notices | **10 ft, touch AC 10**, "Touch AC (43.2 ft.)", hit, state removed during the roll, 1 removal notice, grit 5 to 5 |
| Next shot, not armed, attack command | ordinary AC 18 | no bonus, ordinary AC 18, "Normal AC (43.2 ft.)" |
| Armed, natural 3, attack command | no bonus, ordinary AC 18, state kept | 10 ft, touch AC 10, **miss**, recorded by the product as completed without a misfire, state removed once, grit unchanged |
| Pistol shot while armed (rule) | state kept | state kept, no bonus on the pistol roll |
| Musket shot after it, attack command | no bonus, ordinary AC 18, state kept | 10 ft, touch AC 10, state removed once |
| Another gunslinger's musket shot while the first is armed (rule, 45.2 ft) | no bonus, ordinary AC 18 | no bonus, ordinary AC 18, the first gunslinger still armed |
| Deadeye armed as well (rule) | Deadeye authorized touch AC itself ("Touch AC (Deadeye)"); Steady Aim kept | Deadeye read the lengthened first increment and authorized nothing (1 not authorized, 0 authorized, 0 faults); 10 ft, touch AC 10 by range; both states removed |
| Grit spent to 0 after arming, attack command | no bonus, ordinary AC 18, state kept | state removed once, **no bonus, ordinary AC 18**, grit 0 to 0 |
| Armed and left alone | ended after 6.03 s | ended after 6.02 s, grit 5 to 5 |

In every armed case arming raised one buff-added notice, changed no grit, and left the ability reading as
unavailable while armed. With no grit the ability read as unavailable. These two are the game's availability check;
no command was attempted in either state. Every recorded roll through the attack command was that command's own
(`UnitAttack.LastAttackRule.AttackRoll`), one roll per command, and the musket's own range read 40 ft throughout.

Before the repair the step fails by design on the armed cases only. With the repair the step passes.

### What was covered where

| Case | Native, attack command | Native, weapon attack rule | Focused tests |
|---|---|---|---|
| Exactly 10 ft on a qualifying shot | yes | yes | yes |
| Consumed on a hit | yes | | decision has no outcome input |
| Consumed on a miss without misfire | yes | | |
| No bonus on the next shot | yes | | yes |
| One-handed firearm keeps the state | | yes | yes |
| Non-firearm, ambiguous or unknown firearm, scatter delivery, roll that fired no round | | | yes |
| No grit: unavailable | availability check | | yes |
| Grit lost after arming | yes | | yes |
| Selected True Grit exemption | | | yes |
| Unused state ends after one round | yes | | |
| Another actor | | yes | |
| Deadeye reads the corrected range | | yes | yes (data flow only) |
| Armor class selected after the bonus | yes | yes | yes (data flow only) |

### Things seen in the fixture that are not Steady Aim

- From the second roll on, the game takes its 4 for shooting into a melee off every attack roll (attack bonus 5 on
  the first roll, 1 afterwards, on both builds; the rolls record that penalty by name). The game applies it when a
  unit friendly to the shooter is engaged with the target within 10 ft, so something from the save joined the fight
  around the target; which unit was not recorded. It does not affect which armor class is selected.
- The target reads as flat-footed on some rolls and not on others. Its flat-footed values equal its ordinary ones.
- **Finding NEW-07.** Because the command's range is edge to edge and the product's first increment is centre to
  centre, an unarmed musket shot ordered from the edge of its range against a Medium target is resolved at about
  43 ft: past the 40 ft increment, against ordinary armor class. Observed on both builds in the control shot. Not
  changed here; recorded as a question for the firearm-rules work.

## Identities

| Item | Identity |
|---|---|
| Product commit | `28836aaaea` (the only product change after `7fee25403d`) |
| Candidate | built once at `2ee86e0f6a6e`, 410 s; package SHA-256 `5e8e03e7a5b3bb1ef9651a7cb98aa48141a0fa3cb5fd4d2e5c041123ca916dca`, DLL `706bf89133807cf431670234f9c56b6db8d107b2d53d3931b1f384cbf00c6d3e`; 50 of 3,001 tests selected by the build, 0 failures |
| Pre-fix reference | PR #169's candidate `96dbf78fac0c`, package `3484df4947e0dff0ddc6a525ef2a3b52a256c3772a3182bb8df920cd2452596b`, DLL `ed64e2a4de9264eab9911e1a9212fd89c9ca7fe3304c527eb918cb6f863757fa`, not rebuilt |
| Session file | `steady-aim-range.json`, SHA-256 `274607b606357c1debfc83606db3d002f34384b94bc414d0b2352e566424fb37` |
| Harness for the matched pair | commit `1a657c8e4a`, DLL `6d125cf2e6d5f6310b4f1bccb983c03aac59fa52e47605daafc26a328483845c` |

All four runs, in order:

| Run | Product | Harness | Result |
|---|---|---|---|
| `20261115T232920Z` | pre-fix `96dbf78fac0c` | working tree before `2ee86e0f6a`, DLL `dd832dbe...` | FAIL. The defect showed in the first armed shot (43.3 ft, ordinary AC, state kept). The rest of this run is not usable: the target was knocked out by the first hit and changed armor class, and the game's AI issued attacks of its own for the shooter, which displaced two of the probe's commands |
| `20261115T234128Z` | candidate | `2ee86e0f6a`, DLL `c32cf94e...` | FAIL on one fixture step: the unarmed "next shot" was ordered while the previous attack command was still in the unit's slot, was absorbed by it and never ran. Every Steady Aim expectation in the run was met |
| `20261115T234533Z` | candidate | `1a657c8e4a`, DLL `6d125cf2...` | **PASS** |
| `20261115T234729Z` | pre-fix `96dbf78fac0c` | same DLL | FAIL by design, armed cases only |

Every session reported the owner's 95 save files and installation unchanged and the registry restored. Every probe
released everything it owned (subscription, forced roll, pause, three units, three blueprint clones) with no cleanup
failure and isolation restored.

Validation time for this cut: about 16 minutes (gate 18 s, harness compiles about 20 s, candidate build 410 s, four
launches 135 + 131 + 109 + 129 s). That is over the 15-minute planning target and inside the 30-minute ceiling; the
two extra launches were the probe's own faults.

## Limits

- Real-time play only. Turn-based mode, controller, save and load with the state armed, and other mod stacks were not
  run.
- The native shots used a musket. Direct Blunderbuss and Rifle qualification is covered by the focused tests only.
- Scatter delivery, Dead Shot rolls, an empty chamber, a non-firearm attack and ambiguous or unknown firearm
  identities were not exercised in the game for Steady Aim; the pipeline's shot mark that excludes them is the one
  Deadeye already relies on.
- The selected True Grit exemption was not exercised in the game.
- No command was attempted while armed or with no grit; those are availability readings.
- The published v0.0.191 was not run with this probe. From source its handler is the same as the pre-fix
  reference's.
- The Deadeye check is limited to Deadeye receiving the range. Deadeye's pricing and re-arm rule are unchanged and
  were not judged; with both armed, Deadeye's state is used up by the shot while authorizing nothing.
- A repeated observation of one attack roll was not produced in the game; the guard is the same reference gate the
  discharge step uses.
- The earlier `effect-free-buff-lifecycles` session has its own `steady-aim` step (the general armed-state script, at
  10 ft). It was not rerun on this candidate, and that session's description still says the step fails.
- The complete domain suite was not run for this cut.
