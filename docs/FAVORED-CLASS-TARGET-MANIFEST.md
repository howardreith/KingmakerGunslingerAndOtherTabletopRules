# Favored Class target manifests

Exact targets of the selected-target favored-class counters: what each one
scales, what stays at the actual class level, and what is not offered. The
revelation and performance tables are generated from the committed manifests
(`FavoredClassRevelationManifest`, `FavoredClassPerformanceManifest`) and the
live scope evidence of candidate `6db62e57d` (run
`20260924T0549454842857Z-disposable-favored-class-oracle-revelations`). The
audits behind them (private, not committed) read the installed Call of the Wild
1.14.4c-2.1 and Kingmaker 2.1.7b blueprints.

A counter never grants a feature, ability, revelation or power early, never
satisfies a level prerequisite, and never changes spell slots, BAB or saves.
Owner decisions are listed in `FAVORED-CLASS-BLOCKERS.md`.

## I06/S04 — selected oracle revelation (1/6 per revelation, uncapped)

Offered to Ifrit and Sylph Oracles for a revelation they already have (any of
its selectable features; the ten Dragon colours are one target). One counter
per revelation: 3 full and 17 partial ranks. The effective level adds the
earned steps where the revelation itself reads oracle level:

- **A — ranks.** Call of the Wild's Oracle engine rank configs (the sum of Oracle
  and Demon Hunter Inquisitor levels) evaluated in the revelation's own ability,
  buff, area or feature context get the steps added to their base value, after
  Call of the Wild's own postfix and before the rank's progression and limits.
- **B — uses.** A resource whose maximum scales with oracle level gets exactly
  its native maximum at the effective level (clamped as natively).
- **C — parameters.** The engine's caster level for the revelation's own
  abilities rises by the steps and its half-level spell level by the resulting
  difference, so save DCs and caster-level-based durations and dice follow.

Held back (still the actual oracle level): breakpoint tables, tier selections
the audit does not implement, the explicitly held reads below, single-level
extra uses and every level gate. Level-gated abilities are scaled once the
revelation actually grants them. Read points reached from two targets are
withheld from both; a target with no read point is not published.

| Key | Revelation | Mystery | Audit families | Found (build 14) | Rank reads | Resources | Parameter abilities | Held back |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| AgingTouch | Aging Touch | Time | A, B | A, B, C | 2 | 1 | 1 | - |
| RewindTime | Rewind Time | Time | B | B | 0 | 1 | 0 | - |
| SpeedOrSlowTime | Speed or Slow Time | Time | B, C | B, C | 0 | 1 | 2 | - |
| TimeFlicker | Time Flicker | Time | B | B | 0 | 1 | 0 | the displacement gained at 7th level |
| TimeHop | Time Hop | Time | B | B, C | 0 | 1 | 2 | - |
| TimeSight | Time Sight | Time | B | B | 0 | 1 | 0 | the foresight step at 18th level (held-breakpoint-table) |
| EraseFromTime | Erase From Time | Time | A, C | A, C | 1 | 0 | 1 | the extra daily use at 11th level (held-threshold-resource) |
| BloodOfHeroes | Blood of Heroes | Ancestor | A, B | A, B | 1 | 1 | 0 | - |
| PhantomTouch | Phantom Touch | Ancestor | A | A | 1 | 0 | 0 | - |
| SpiritOfTheWarrior | Spirit of the Warrior | Ancestor | B | B | 0 | 1 | 0 | the base attack bonus while possessed (held-read) |
| SpiritShield | Spirit Shield | Ancestor | A, B | A, B | 1 | 1 | 0 | the ranged miss chance gained at 13th level (held-tier) |
| StormOfSouls | Storm of Souls | Ancestor | A, B, C | A, B, C | 2 | 1 | 1 | - |
| SpiritWalk | Spirit Walk | Ancestor | B | B | 0 | 1 | 0 | - |
| AncestralWeapon | Ancestral Weapon | Ancestor | B | B | 0 | 1 | 0 | - |
| FireBreath | Fire Breath | Flame | A, B, C | A, B, C | 1 | 1 | 1 | - |
| Firestorm | Firestorm | Flame | A, C | A, C | 1 | 0 | 1 | - |
| FormOfFlame | Form of Flame | Flame | C | C | 0 | 0 | 4 | the elemental body forms gained at 9th, 11th and 13th level |
| HeatAura | Heat Aura | Flame | A, B, C | A, B, C | 1 | 1 | 1 | - |
| TouchOfFlame | Touch of Flame | Flame | A | A | 1 | 0 | 0 | the flaming weapon gained at 11th level |
| Battlecry | Battlecry | Battle | B | B | 0 | 1 | 0 | the +2 bonus at 10th level (held-read) |
| BattleCombatHealer | Combat Healer | Battle | B | B | 0 | 1 | 0 | - |
| IronSkin | Iron Skin | Battle | C | C | 0 | 0 | 1 | the extra daily use at 15th level (held-threshold-resource) |
| SurprisingCharge | Surprising Charge | Battle | B | B | 0 | 1 | 0 | - |
| Channel | Channel | Life | A, C | A, C | 24 | 0 | 21 | - |
| LifeCombatHealer | Combat Healer | Life | B | B | 0 | 1 | 0 | - |
| EnergyBody | Energy Body | Life | A, B | A, B | 2 | 1 | 0 | - |
| LifeLink | Life Link | Life | B | B | 0 | 1 | 0 | - |
| SpiritBoost | Spirit Boost | Life | A | A | 1 | 0 | 0 | - |
| AirBarrier | Air Barrier | Wind | A, B | A, B | 1 | 1 | 0 | the ranged miss chance gained at 13th level (held-tier) |
| Invisibility | Invisibility | Wind | B | B | 0 | 1 | 0 | greater invisibility at 9th level |
| LightningBreath | Lightning Breath | Wind | A, B, C | A, B, C | 1 | 1 | 1 | - |
| Thunderburst | Thunderburst | Wind | A, B, C | A, B, C | 1 | 1 | 1 | - |
| TouchOfElectricity | Touch of Electricity | Wind | A | A | 1 | 0 | 0 | the shock weapon gained at 11th level |
| PresenceOfDragons | Presence of Dragons | Dragon | B, C | B, C | 0 | 1 | 1 | - |
| ScaledToughness | Scaled Toughness | Dragon | A | A | 1 | 0 | 0 | the extra daily use at 13th level (held-threshold-resource) |
| BreathWeapon | Breath Weapon | Dragon | A, B, C | A, B, C | 20 | 10 | 20 | the final breath weapon at 20th level |
| FormOfTheDragon | Form of the Dragon | Dragon | A | A, C | 40 | 0 | 40 | the forms gained at 15th and 19th level |
| Blizzard | Blizzard | Waves | A, C | A, C | 1 | 0 | 1 | - |
| IceArmor | Ice Armor | Waves | A, B | A, B | 1 | 1 | 0 | the damage reduction gained at 13th level (held-tier) |
| WaterForm | Water Form | Waves | C | C | 0 | 0 | 4 | the elemental body forms gained at 9th, 11th and 13th level |
| WintryTouch | Wintry Touch | Waves | A | A | 1 | 0 | 0 | the frost weapon gained at 11th level |
| PunitiveTransformation | Punitive Transformation | Waves | A, C | A, C | 1 | 0 | 1 | - |
| ErosionTouch | Erosion Touch | Nature | A, B | A, B, C | 1 | 1 | 1 | - |
| LifeLich | Life Lich | Nature | B, C | B, C | 0 | 1 | 1 | - |
| FormOfTheBeast | Form of the Beast | Nature | C | C | 0 | 0 | 8 | the forms gained at 9th, 11th and 13th level |
| GiftOfClawAndHorn | Gift of Claw and Horn | Nature | A | A | 6 | 0 | 0 | the second natural weapon gained at 11th level |
| ArmorOfBones | Armor of Bones | Bones | A, B | A, B | 1 | 1 | 0 | the damage reduction gained at 13th level (held-tier) |
| BleedingWounds | Bleeding Wounds | Bones | A | A | 1 | 0 | 0 | - |
| DeathsTouch | Death's Touch | Bones | A | A | 1 | 0 | 0 | - |
| RaiseTheDead | Raise the Dead | Bones | A | A, C | 1 | 0 | 2 | the extra daily use at 10th level and the ability gained at 15th level (held-threshold-resource) |
| SoulSiphon | Soul Siphon | Bones | A, B | A, B | 1 | 1 | 0 | - |
| UndeadServitude | Undead Servitude | Bones | C | C | 0 | 0 | 1 | - |

Families found beyond the audit (all Oracle-engine parameters of the
revelation's own abilities): Aging Touch, Time Hop and Erosion Touch (caster
level only; no save), Form of the Dragon (the forms' own parameters) and Raise
the Dead (the 15th-level abilities, scaled once gained). Recorded for owner review.

Not offered (audit dispositions): threshold-only design decisions (Temporal
Celerity, War Sight, Cinder Dance, Molten Skin, Spark Skin, Icy Skin, Weapon
Mastery, Fluid Nature, Freezing Spells, Dragon Senses, Draconic Resistances,
Spirit of Nature, Near Death, Resist Life); unsupported branches (Maneuver
Mastery, Enhanced Cures, Vortex Spells, Animal Companion); no oracle-level
scaling (Sacred Council, Burning Magic, Skill at Arms, Healing Hands, Safe
Curing, Wings of Air, Dragon Magic, Wings of the Dragon, Friend to Animals,
Nature's Whispers); Dual-Cursed Fortune (eligibility decision) and Misfortune;
Final Revelations (capstones).

## I08/S06 — selected bloodline power (1/6 per power, max +2)

| Key | Row | Power feature | Ability | Scales |
| --- | --- | --- | --- | --- |
| FireRay | I08 | `ce0889b5c1b392e48baf1e004d1efd67` | `1b4989258e5964149a909e47c72b7f67` | damage bonus rank |
| FireBlast | I08 | `3022a5066a5604a498dd289b37dfd8aa` | `b2d1d39cd406e0f4185c52fecc73c3b5` | dice, DC, caster level checks |
| AirRay | S06 | `acf668c24dfbcdd499276eaf1881486e` | `4729c2ac98d02004fb440d17f7786e28` | damage bonus rank |
| AirBlast | S06 | `553d9802d5d9de04b941b55cb47d3096` | `6d005cc9c3ad3f24e8769aad2fbfdf3f` | dice, DC, caster level checks |

Not targets: Elemental Movement, Body and Arcana and Ray uses (no level
scaling); Elemental Resistance's 9th-level step and Blast's extra uses
(thresholds, owner decision); Primal Elemental bloodlines (owner decision);
efreeti and djinni bloodlines (absent from the game).

## O01 — selected bardic performance range (+5 feet per investment, max +30 feet)

Offered to Oread Bards for a performance they already have. One full-only
counter per performance (6 ranks). Only the casting bard's own instance of the
performance's cylinder widens (on spawn and when a save loads); the shared
blueprint, other performers of the same area, other performances and the visual
ring are unchanged.

| Key | Performance | Feature | Areas | Provider |
| --- | --- | --- | --- | --- |
| InspireCourage | Inspire Courage | `acb4df34b25ca9043a6aba1a4c92bc69` | `5d4308fa344af0243b2dd3b1e500b2cc` | Kingmaker |
| InspireCompetence | Inspire Competence | `6d3fcfab6d935754c918eb0e004b5ef7` | `c08bd33a377d5014a81be94e33ec8ce4` | Kingmaker |
| Fascinate | Fascinate | `ddaec3a5845bc7d4191792529b687d65` | `a4fc1c0798359974e99e1d790935501d` | Kingmaker |
| DirgeOfDoom | Dirge of Doom | `1d48ab2bded57a74dad8af3da07d313a` | `4a15b95f8e173dc4fb56924fe5598dcf` | Kingmaker |
| InspireGreatness | Inspire Greatness | `9ae0f32c72f8df84dab023d1b34641dc` | `23ddd38738bd1d84595f3cdbb8512873` | Kingmaker |
| FrighteningTune | Frightening Tune | `cfd8940869a304f4aa9077415f93febe` | `55c526a79761a3c48a3cc974a09bfef7` | Kingmaker |
| InspireHeroics | Inspire Heroics | `199d6fa0de149d044a8ab622a542cc79` | `1be964f750eea8748a76e92744746efb` | Kingmaker |
| InciteRage | Incite Rage | `35ac4bd7990fa0842bfc22e80665c2f9` | `8426523287601104085d71d410a6fc42`, `9c423eacfb7bb9f408757e651607e125`, `d63dce0f272ba2d4aa13000470398d63` | Kingmaker |
| StormCall | Storm Call | `161db4d6c4a1f4640ab52c762e15c1af` | `85c1ea0021ce2714f8559fb618bf7ff6` | Kingmaker |
| FireDance | Fire Dance | `3c10a0069e7f110499d2e810f4861a6e` | `0bd2c3ff0012e6b468497461448174c7` | Kingmaker |
| SongOfFieryGaze | Song of Fiery Gaze | `edf5697b6ddc42fca14d20a03affd475` | `b556833f0a0a45738863a02c78323fed` | Call of the Wild |
| Satire | Satire | `867e67a274d94c44bf6859b810745b1d` | `b1125eb8eae649bdb441f22e3c088535` | Call of the Wild |
| Mockery | Mockery | `71a3c675a44d4a8a89c9a0840cb1d92a` | `eeb9c36c16be45dda7604b6120d1ab88` | Call of the Wild |
| GloriousEpic | Glorious Epic | `d78e50c8ec9c436c82d3be6a028b4572` | `0d961603708c4db3abf178e26d32fb1b` | Call of the Wild |
| Scandal | Scandal | `88d2e41984ea4c68968197b44ec2f445` | `164dba1be13048eab380b302e4f25b7e` | Call of the Wild |

Not targets (owner decisions): Soothing Performance (burst), Deadly Performance
(targeted), Thunder Call (which range), Dance of the Dead, Call of the Wild
masterpieces and Discordant Voice. Range-free: Archaeologist's Luck, Dance of 23
Steps, move/swift/lingering performance features.

## O06/O07/O08 — auras and pets (1/4, uncapped)

- **O06:** the native Aura of Courage (fear) and Aura of Resolve (charm) ally
  buffs read the paladin's earned steps inside their own Morale bonus; not
  offered to an archetype that replaces both auras (Divine Hunter).
- **O07:** natural armor on the ranger's current animal companion; not offered
  to an archetype that replaces Hunter's Bond (Flamewarden, Freebooter,
  Stormwalker).
- **O08:** natural armor on Call of the Wild's eidolon of the summoner.

