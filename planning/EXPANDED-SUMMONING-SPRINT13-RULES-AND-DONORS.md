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
| Bite damage | **1** (a flat point) | `Bite1d3` | **open** - 1d3 averages 2 and can roll 3. Check whether the installed library has a 1-point or 1d2 natural bite; if it does, use it, and if it does not, record the smallest available die as a disclosed adaptation with the reason. |
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

## 5. Open items to resolve during implementation

1. The smallest available native natural-bite damage die, for the Poison Frog's
   printed flat 1 damage.
2. The exact native identities for: panicked, the sonic descriptor, a
   concealment/miss-chance component, and a rage-style stat buff with an AC
   penalty. All must come from an audited exact seam, not a name search.
3. Whether the native `Rage` or barbarian rage buff can be reused without
   dragging in class resources; if not, a creature-scoped buff built the way
   the Sprint 12 allergy buff is built.
