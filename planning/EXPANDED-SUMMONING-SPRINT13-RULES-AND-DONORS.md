# Sprint 13 rules and donor record

Primary sources read in full 2026-10-01:

- Wolverine: https://legacy.aonprd.com/bestiary/wolverine.html
- Poison Frog: https://legacy.aonprd.com/bestiary/frog.html (heading "Poison Frog")
- Shadow Mastiff: https://legacy.aonprd.com/bestiary3/shadowMastiff.html

Appendix A assignments for this sprint: Wolverine SM 3 / SNA 3; Shadow Mastiff
SM 6 only; Poisonous Frog SM 1 / SNA 1.

## 1. Wolverine - printed block versus the current profile

Printed: N Medium animal, 3d8+9, AC 14 touch 12 flat-footed 12 (+2 Dex, +2
natural), Fort +5 Ref +5 Will +2, Speed 30 ft. (burrow 10 ft., climb 10 ft.),
**2 claws +4 (1d6+2), bite +4 (1d4+2)**, Special Attacks rage, Str 15 Dex 15
Con 15 Int 2 Wis 12 Cha 10, Base Atk +2, CMB +4, CMD 16 (20 vs. trip), Feats
Skill Focus (Perception) and Toughness, Skills Climb +10 Perception +10.

> Rage (Ex) A wolverine that takes damage in combat flies into a rage on its
> next turn, clawing and biting madly until either it or its opponent is dead.
> It gains +4 to Strength, +4 to Constitution, and -2 to AC. The creature
> cannot end its rage voluntarily.

Current profile (`ExpandedSummoningNaturalProfiles.cs`):
`PS("wolverine", "Wolverine", "Animal", 3, "Medium", 15, 15, 15, 2, 12, 10, 30, 2, "Claw1d6", A("Claw1d6"), A("Bite1d4"), A("SkillFocusPerception", "Toughness"), ...)`

| Item | Printed | Current | Verdict |
| --- | --- | --- | --- |
| Size, HD, ability scores, speed, natural armor | Medium, 3, 15/15/15/2/12/10, 30 ft., +2 | same | exact |
| Feats | Skill Focus (Perception), Toughness | same | exact |
| Trip defence | CMD 16 (20 vs. trip) | `TripDefenseFourLegs` absent | **defect** - the printed block carries the quadruped +4, and the Dog, Dire Rat and Hyena rows all encode it |
| Claws | 2 claws +4 (1d6+2) | primary `Claw1d6` + additional `Claw1d6` | exact |
| Bite | bite **+4** (1d4**+2**) | `Bite1d4` placed in `AdditionalSecondaryWeapons` | **defect**, re-verified 2026-10-01 against both the source and the project's own semantics. Printed BAB +2 and Strength 15 give +4 with full +2 damage, which is a primary natural attack; a secondary one would be -1 to hit and +1 damage. The semantics were calibrated against an accepted creature rather than assumed: the Crocodile's printed `tail slap +0 (1d12+2)` is a genuine secondary - BAB 2 + Str 4 - 1 size - 5 = +0, half Strength damage - and the project correctly places it in `AdditionalSecondaryWeapons`. So that list does mean a PF1 secondary attack, and the Wolverine's bite does not belong in it. Move it to `AdditionalWeapons`. |
| Rage | Ex, see quote | omitted, recorded as a deviation | **must implement** - Appendix A asks for "summon-local rage" and D-02 forbids counting the creature complete without the mechanic that justifies it. The recorded omission is not an accepted deviation. |
| Burrow, climb | burrow 10 ft., climb 10 ft. | omitted, disclosed | keep the disclosed single-speed adaptation |

### Rage implementation contract

- Trigger: the wolverine **takes damage in combat**. Not a save, not a hit that
  deals zero damage - actual positive damage, as with the Sprint 12 bite rider.
- Onset: **its next turn**, not immediately. The buff must be scheduled rather
  than applied on the damage event, so the printed one-turn delay is visible.
- Effect: +4 Strength, +4 Constitution, **-2 AC**. The AC penalty is part of the
  rule and must not be dropped; a rage that is pure upside is stronger than the
  stat block.
- Cannot be ended voluntarily: no player toggle to switch it off.
- Ends when "either it or its opponent is dead", and in Kingmaker terms also
  when the summon expires, is dismissed, dies, or the module is disabled.
- Summon-local: the buff belongs to that one wolverine. It must never land on
  the owner, the caster, another summon, or an unrelated unit, and must not
  survive on anything after the wolverine is gone.
- Save/load: per the accepted engine limitation the project does not rebuild
  session relationships on reload, but a rage buff is ordinary creature state,
  not a grapple link. It must either persist correctly or reset cleanly with no
  stuck +4/+4/-2 and no orphaned modifier.

## 2. Poisonous Frog - printed block versus the current profile

Printed: N Tiny animal, 1d8, AC 13 touch 13 flat-footed 12 (+1 Dex, +2 size -
**no natural armor**), Fort +2 Ref +3 Will -1, Speed 10 ft. (swim 20 ft.),
**bite +3 (1 plus poison)**, Space 2-1/2 ft. Reach 0 ft., Str 2 Dex 12 Con 11
Int 1 Wis 9 Cha 10, Base Atk +0, CMB -1, CMD 5 (9 vs. trip), Feats **Weapon
Finesse**, Poison (Ex) injury, save Fort DC 10, frequency 1/round for 6 rounds,
effect 1d2 Con damage, cure 1 save.

Current profile:
`P("poisonous-frog", "Poisonous Frog", "Animal", 1, "Tiny", 2, 12, 11, 1, 9, 10, 10, 0, "Bite1d3", ..., A("WeaponFinesse", "TripDefenseFourLegs", "PoisonFrog"), ...)`

| Item | Printed | Current | Verdict |
| --- | --- | --- | --- |
| Size, HD, ability scores, speed, natural armor | Tiny, 1, 2/12/11/1/9/10, 10 ft., 0 | same | exact |
| Weapon Finesse | yes | yes | exact |
| Trip defence | CMD 5 (9 vs. trip) | `TripDefenseFourLegs` | exact |
| Poison | Fort DC 10, 1/round for 6 rounds, 1d2 Con, cure 1 save | `PoisonFrog` on the native Constitution-scaled graph | exact, already disclosed; DC 10 = 10 + Con 11 modifier 0 + half of 1 HD |
| Bite damage | **1** (a flat point) | `Bite1d3` | **defect** - 1d3 averages 2 and can roll 3. Exactly fixable: `DiceType` has a `One` member and the project mints its own natural weapons, so the printed flat point is one roll of `DiceType.One`. See 5.1. |
| Visual | Tiny frog | Giant Poisonous Frog proxy | **must replace** - Appendix A asks for "a true Tiny frog visual"; the charter forbids counting a proxy as ideal |
| Swim | swim 20 ft. | omitted | keep the disclosed single-speed adaptation |

## 3. Shadow Mastiff - new creature

Printed: NE Medium outsider (evil, extraplanar), 6d10+18, AC 18 touch 12
flat-footed 16 (+2 Dex, +6 natural), Fort +8 Ref +7 Will +5, Defensive
Abilities shadow blend, Speed 50 ft., **bite +10 (1d8+4 plus trip), tail slap
+5 (1d6+2)**, Special Attacks bay, Str 19 Dex 15 Con 17 Int 4 Wis 12 Cha 13,
Base Atk +6, CMB +10, CMD 22 (26 vs. trip), Feats Improved Initiative, Iron
Will, Power Attack, Skills Perception +10 Stealth +11 Survival +10, Languages
Common (cannot speak).

> Bay (Su) When a shadow mastiff howls or barks, all creatures within a
> 300-foot spread except evil outsiders must succeed at a DC 16 Will save or
> become panicked for 1d4 rounds. This is a sonic, mind-affecting fear effect.
> A creature that successfully saves cannot be affected by the same mastiff's
> bay for 24 hours. ... The save DC is Charisma-based and includes a +2 racial
> bonus.

> Shadow Blend (Su) In any condition of illumination other than full daylight, a
> shadow mastiff disappears into the shadows, giving it concealment (50% miss
> chance). Artificial illumination, even a light or continual flame spell, does
> not negate this ability; a daylight spell, however, does. A shadow mastiff
> can suspend or resume this ability as a free action.

Derived facts to encode:
- Attack placement: bite +10 = BAB 6 + Str 4 → **primary**, with trip. Tail
  slap +5 = 6 + 4 - 5 → **secondary**, and 1d6+2 is half Strength, confirming
  it. So bite is the primary natural weapon and the tail slap goes in the
  secondary limb list.
- Outsider hit dice (d10) and the evil/extraplanar subtype. Summon Monster VI
  only, and **not** Celestial/Fiendish templated, like the other project
  outsiders (`hell-hound`, `erinyes-devil`).
- Bay DC 16 is Charisma-based with a +2 racial bonus: 10 + 3 (half of 6 HD) +
  1 (Cha 13) + 2 racial = 16. Encode the components, not the literal 16, so a
  buffed Charisma moves the DC as the engine expects.
- Panicked, not frightened or shaken. Sonic and mind-affecting fear
  descriptors, so native fear and sonic immunities apply for free.
- The 24-hour per-mastiff/per-target immunity after a successful save is the
  rule's own bound and must be per mastiff, like the Stirge's
  once-per-victim disease check.

### The ally-aware bay policy the charter asks for

The printed bay catches **all** creatures in a 300-foot spread except evil
outsiders, which in an ordinary Kingmaker encounter includes the summoner's own
party. The charter's Definition of Done forbids "friendly-fire loops" and
"repeated friendly-fire effects" from constrained caster AI, and the Sprint 13
boundary asks for bay that is "useful without repeatedly harming allies or
stalling AI".

The resolution that keeps the rule exact is to make bay a **player-activated
ability on the summoned mastiff** and to keep the mastiff's own AI from ever
choosing it. Kingmaker summons are player-controllable, so the printed
300-foot spread, the DC, the panic duration and the ally exposure all stay
exactly as written, and the decision to accept the friendly-fire cost belongs
to the player rather than to an AI that would spam it. No AI loop is possible
because the AI never selects it, and the rule's own 24-hour immunity bounds
repeat use. This is a bounded AI-priority decision inside the charter's
delegated scope, not a change to the rule, and it must be recorded as such
rather than as a weakening of bay.

Shadow blend's free-action suspend/resume is also a player control; the default
state is active, which is what a summoner wants.

## 4. Donor and visual plan

- Wolverine currently borrows the Worg view. Appendix A asks for a wolverine
  visual. Needs an original mesh on the audited canid donor rig, like the
  Sprint 12 quadrupeds.
- Shadow Mastiff is a new creature with no view. A canid donor rig plus an
  original dark-coated mesh with the printed long spiked tail.
- Poisonous Frog borrows the Giant Poisonous Frog view at Tiny scale. Needs a
  true Tiny frog silhouette.

## 5. Native seams, resolved against the installed assembly

Every item below was read out of the exact private build reference
`Assembly-CSharp.dll` in
`private/extracted-references/KingmakerGunslinger-private-build-references/Managed`,
by reflection over its types and, where the answer depended on control flow,
by decoding the method IL and resolving its metadata tokens. These are
assembly facts rather than name guesses, and each one is restated as a runtime
assertion when the ability that needs it is built.

### 5.1 The Poison Frog's flat 1 damage needs no adaptation

`Kingmaker.RuleSystem.DiceType` has an explicit `One = 1` member alongside
`Zero = 0`, and `DiceFormula` exposes a static `One`. The project already mints
its own natural weapons rather than borrowing native ones -
`ExpandedSummoningNaturalBuilder.ConfigureWeapon(donor, target, symbol, rolls,
dice)` is how `Bite1d4` and `Bite1d3` exist at all, and the Stirge proboscis is
already minted at `DiceType.Zero`. So the printed `bite +3 (1 plus poison)` is
expressible exactly, as one roll of `DiceType.One`. The open item that expected
to settle for "the smallest available die as a disclosed adaptation" is closed
in favour of the printed value; no adaptation is recorded because none is
needed.

### 5.2 Panicked: the engine has one flee state, and it is Frightened

`Kingmaker.UnitLogic.UnitCondition` has `Shaken`, `Frightened` and `Cowering`
but no `Panicked`, and `BlueprintRoot.SystemMechanics` correspondingly offers
`ShakenBuff`, `FrightenedBuff` and `CoweringBuff` and no panicked buff.
`SpellDescriptor` likewise has `Shaken` and `Frightened` and no panicked
member.

What the engine does have is `Kingmaker.Controllers.Units.UnitFearController`,
and decoding it settles how fear actually runs. `ShouldTickOnUnit` is

    base.ShouldTickOnUnit(unit) &&
        (unit.Descriptor.State.IsPanicked ||
         unit.Descriptor.State.HasCondition(UnitCondition.Frightened))

where the condition operand is `ldc.i4.s 13` and `UnitCondition.Frightened` is
13. `TickOnUnit` then, for such a unit, interrupts every command, picks a point
away from the averaged direction of its remembered enemies along a partly
random node choice (`GetPointForMove`), and runs a `UnitMoveTo` to it,
re-interrupting each tick; when no enemy has been remembered for
`GameConsts.UnitPanickedCooldownDuration` (3 seconds) it clears
`UnitState.IsPanicked` and releases the unit.

So `IsPanicked` is the controller's own latch rather than an authorable state,
and the single authorable lever is `UnitCondition.Frightened`, applied by
`Kingmaker.UnitLogic.FactLogic.AddCondition`. Behaviourally the engine's
Frightened already delivers what PF1 assigns to panicked: forced flight away
from the threat at run speed, along a random path, with no other action
possible. The PF1 ladder's remaining distinctions between frightened and
panicked - dropping held items, and cowering when cornered - are separate
mechanics, and the engine can express both
(`SystemMechanics.DisarmMainHandBuff` and `DisarmOffHandBuff`, and
`UnitCondition.Cowering`), so neither is an engine barrier. Whether Bay should
carry them is a rules question to settle in the implementation record, not one
the engine decides.

Bay therefore applies a project-owned buff for its printed `1d4` rounds whose
`AddCondition` imposes `UnitCondition.Frightened`, so the native fear
controller drives the flight, and whose descriptors are `Sonic`,
`MindAffecting` and `Fear` - all three present in `SpellDescriptor`
(`Sonic = 1073741824`, `MindAffecting = 16`, `Fear = 32`) - so native sonic,
mind-affecting and fear immunities apply without being re-implemented.

### 5.3 Shadow Blend: the concealment is exact, the illumination clause is not

`Kingmaker.UnitLogic.FactLogic.AddConcealment` exists, and
`Kingmaker.Enums.Concealment` is `None, Partial, Total`. The printed ability
grants "concealment (50% miss chance)", which is `Concealment.Total`; `Partial`
is the 20% grade. The effect itself is therefore exact.

The precondition is not. The installed assembly has no mechanics-layer
illumination model at all: no illumination or light-level type outside the
rendering namespaces, no enum carrying bright/dim/dark grades, and no
`SpellDescriptor.Light` or `.Darkness` member. There is no ambient light to
read, so "any condition of illumination other than full daylight" has no
literal predicate.

Two pieces of the clause survive exactly, and the implementation uses both
rather than quietly leaving the ability always on:

- "a daylight spell, however, does [negate it]" is expressible against an
  already-audited identity: native Daylight is
  `2b877386976817a429002e8bb10bb3fc`, in this project's own audited native
  light-spell census in `ElementalFeatPolicy`.
- "other than full daylight" has one honest in-engine reading. `Game.TimeOfDay`
  is a real accessor returning `Kingmaker.AreaLogic.TimeOfDay`
  (`Morning, Day, Evening, Night`), and `BlueprintArea.IsSingleLightScene`
  marks an area whose lighting does not follow the sun, which is how the engine
  distinguishes interiors and dungeons. "Full daylight" is read as
  `TimeOfDay.Day` in an area that is not a single light scene.

That is a bounded adaptation of a precondition, recorded here as such. It is
deliberately not called an accepted deviation: the ability, its concealment
grade, its free-action suspend and resume, and its daylight negation are all
implemented as printed.

### 5.4 Rage: a creature-scoped buff, not the barbarian's

`Kingmaker.UnitLogic.FactLogic.AddStatBonus` carries `Stat`, `Value` and a
`ModifierDescriptor`; `StatType` has `Strength`, `Constitution` and `AC`, and
`ModifierDescriptor` has both `Morale` and `Penalty`. The printed +4 Strength,
+4 Constitution and -2 AC are therefore three ordinary components on one buff,
with the AC term carried as a penalty rather than dropped.

The open question of whether to reuse a native barbarian rage is answered no.
`UnitCondition.BarbarianRage` exists, and imposing it would expose the creature
to class machinery it has no business in - rage powers, rage rounds, and the
fatigue that follows a barbarian's rage - none of which the wolverine's stat
block has. A creature-scoped buff built the way the Sprint 12 allergy buff is
built keeps the mechanic local to the one animal, which is what "summon-local
rage" in Appendix A asks for.

For the printed one-turn delay the engine offers two round-boundary seams,
`Kingmaker.PubSubSystem.IUnitNewCombatRoundHandler.HandleNewCombatRound(unit)`
and `Kingmaker.Controllers.Units.ITickEachRound.OnNewRound()`. The damage
trigger reuses the Sprint 12 rider's proven shape, a
`RuleTargetLogicComponent<RuleDealDamage>` that fires only on actual positive
damage.

### 5.5 Bay's 24-hour immunity is keyed to an identity, not a reference

The printed bound is per mastiff and per target, and it lasts 24 hours - far
longer than a summon does. A successful save therefore grants the victim a
project-owned immunity buff carrying the mastiff's `UniqueId` rather than a
live reference to it, precisely because the window deliberately outlives the
creature that opened it. This is the same lesson the Sprint 12 disease lifetime
contract records: a rider whose duration exceeds its source's must not hold the
source.

## 5.6 What the Shadow Mastiff build actually chose

The creature has no native Kingmaker equivalent, so it is constructed the way
the project's other outsiders are: outsider hit dice on the native outsider
class, the evil subtype and the project's extraplanar marker, and no
Celestial/Fiendish template. The Worg supplies the rig and bite animation; its
stat line supplies nothing.

- **Attack placement comes from the printed numbers, not the donor's limbs.**
  Bite +10 is base attack 6 plus Strength 4 with full Strength damage, so it is
  the primary natural weapon; tail slap +5 is 6 + 4 - 5 and 1d6+2 is half
  Strength, so it is the only secondary limb. The 1d6 tail slap needed a
  project-owned identity, minted from the native animated tail with the printed
  dice, because no native 1d6 tail existed.
- **The bay DC is stored as its components.** 10 + half of 6 hit dice +
  Charisma 13 + 2 racial derives to the printed 16, and the derivation is what
  is asserted, so a buffed or drained Charisma moves the DC the way the engine
  expects instead of the number being frozen.
- **Bay keeps the printed spread and bounds the decision instead.** Its target
  type is `Any`, not `Enemy`, because the printed rule catches every creature in
  300 feet except evil outsiders and that includes the summoner's own party;
  sparing allies would be a change to the rule. What is bounded is who chooses
  to use it: bay is a player-activated standard action on the summoned mastiff
  and nothing wires it into a brain, so no AI can select it, no friendly-fire
  loop is possible, and the rule's own 24-hour immunity bounds repeat use. A
  test asserts that no `BlueprintAiCastSpell` appears in the bay builder.
- **The 24-hour immunity is matched by caster identity.** Comparing the stored
  marker's caster to the baying mastiff is exactly right in all three cases
  that can arise: the same living mastiff matches and is blocked, a different
  mastiff does not match and may bay as printed, and a mastiff that has expired
  resolves to no caster and could never bay again anyway. No ledger is needed.
- **Shadow blend rides the native concealment entry and a suppression gate.**
  `AddConcealment` at `Concealment.Total` is the printed 50% miss chance, and a
  `BuffLogic` suppresses the buff when the printed negations hold, so the
  engine adds and removes the entry rather than this project doing it by hand.
  The `Blur` concealment descriptor is deliberate: the ability is not
  invisibility, so `TargetIsInvisible` would wrongly let See Invisibility
  defeat it, and it is not fog, so `Fog` would wrongly tie it to weather.
  The condition is re-read on activation and at each round boundary, which is
  faithful rather than approximate because everything it depends on - the time
  of day, the loaded area, and whether a daylight effect is present - changes
  on a scale far coarser than a round.
- **Registration precedes qualification.** The creature's four placements are
  suppressed, so the published surface stays at 900 generated and 929 total
  choices while its mechanics, visual identity and lifecycle are qualified.
  Its identities are allocated once and never move.

## 6. Open items to resolve during implementation

All four of the previously open items are resolved above. What remains is
runtime confirmation rather than research:

1. That a 300-foot spread resolves against a real loaded area, and how many
   units it actually reaches there.
2. That the native fear controller does drive a summoned mastiff's victims, and
   releases them cleanly when the printed `1d4` rounds end.
3. That the rage buff's +4/+4/-2 appears and clears with no stuck modifier,
   including across the summon's expiry and a save/load.
