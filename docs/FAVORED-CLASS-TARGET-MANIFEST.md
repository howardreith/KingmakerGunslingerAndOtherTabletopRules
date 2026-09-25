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
A revelation, bloodline power or performance counts as owned when the
character has it or chooses it in the same level-up: the native level-up
replays its picks in priority order, and the host's reward selection comes
before the bloodline, revelation and power selections, so the owned-target
check also counts a later pick of the level-up being replayed.
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

Level thresholds and gates (review finding 2, charter 8.10): the
revelation's own steps, tiers, breakpoint tables and single-level extra uses
follow the effective level, and so do its own native level gates
(`AddFeatureOnClassLevel`, including the before/after pairs that swap a form):
an invested revelation gains the abilities and forms it grants at later oracle
levels earlier, and loses them again when the investment is removed. Nothing
else is granted: no other revelation, no revelation choice, no class feature,
spell, slot, BAB or save. A target whose effect cannot be implemented without
breaking a charter rule is excluded instead of published partially. Read points
and gates reached from two targets are withheld from both, and a target that
loses a gate that way is withheld whole; a target with no read point is not
published.

| Key | Revelation | Mystery | Audit families | Found (build 14) | Rank reads | Resources | Parameter abilities | Effective-level thresholds and gates |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| AgingTouch | Aging Touch | Time | A, B | A, B, C | 2 | 1 | 1 | - |
| RewindTime | Rewind Time | Time | B | B | 0 | 1 | 0 | - |
| SpeedOrSlowTime | Speed or Slow Time | Time | B, C | B, C | 0 | 1 | 2 | - |
| TimeFlicker | Time Flicker | Time | B | B | 0 | 1 | 0 | the displacement gained at 7th level (gate) |
| TimeHop | Time Hop | Time | B | B, C | 0 | 1 | 2 | - |
| TimeSight | Time Sight | Time | B | B | 0 | 1 | 0 | the foresight step at 18th level (breakpoint table) |
| EraseFromTime | Erase From Time | Time | A, C | A, C | 1 | 0 | 1 | the extra daily use at 11th level (resource threshold) |
| BloodOfHeroes | Blood of Heroes | Ancestor | A, B | A, B | 1 | 1 | 0 | - |
| PhantomTouch | Phantom Touch | Ancestor | A | A | 1 | 0 | 0 | - |
| SpiritOfTheWarrior | Spirit of the Warrior | Ancestor | B | B | 0 | 1 | 0 | EXCLUDED, not published: its possession sets the base attack bonus from oracle level, which a counter must never raise |
| SpiritShield | Spirit Shield | Ancestor | A, B | A, B | 1 | 1 | 0 | the ranged miss chance gained at 13th level (tier) |
| StormOfSouls | Storm of Souls | Ancestor | A, B, C | A, B, C | 2 | 1 | 1 | - |
| SpiritWalk | Spirit Walk | Ancestor | B | B | 0 | 1 | 0 | - |
| AncestralWeapon | Ancestral Weapon | Ancestor | B | B | 0 | 1 | 0 | - |
| FireBreath | Fire Breath | Flame | A, B, C | A, B, C | 1 | 1 | 1 | - |
| Firestorm | Firestorm | Flame | A, C | A, C | 1 | 0 | 1 | - |
| FormOfFlame | Form of Flame | Flame | C | C | 0 | 0 | 4 | the elemental body forms gained at 9th, 11th and 13th level (gate) |
| HeatAura | Heat Aura | Flame | A, B, C | A, B, C | 1 | 1 | 1 | - |
| TouchOfFlame | Touch of Flame | Flame | A | A | 1 | 0 | 0 | the flaming weapon gained at 11th level (gate) |
| Battlecry | Battlecry | Battle | B | B | 0 | 1 | 0 | the +2 bonus at 10th level (step) |
| BattleCombatHealer | Combat Healer | Battle | B | B | 0 | 1 | 0 | - |
| IronSkin | Iron Skin | Battle | C | C | 0 | 0 | 1 | the extra daily use at 15th level (resource threshold) |
| SurprisingCharge | Surprising Charge | Battle | B | B | 0 | 1 | 0 | - |
| Channel | Channel | Life | A, C | A, C | 24 | 0 | 21 | - |
| LifeCombatHealer | Combat Healer | Life | B | B | 0 | 1 | 0 | - |
| EnergyBody | Energy Body | Life | A, B | A, B | 2 | 1 | 0 | - |
| LifeLink | Life Link | Life | B | B | 0 | 1 | 0 | - |
| SpiritBoost | Spirit Boost | Life | A | A | 1 | 0 | 0 | - |
| AirBarrier | Air Barrier | Wind | A, B | A, B | 1 | 1 | 0 | the ranged miss chance gained at 13th level (tier) |
| Invisibility | Invisibility | Wind | B | B | 0 | 1 | 0 | greater invisibility at 9th level (gate) |
| LightningBreath | Lightning Breath | Wind | A, B, C | A, B, C | 1 | 1 | 1 | - |
| Thunderburst | Thunderburst | Wind | A, B, C | A, B, C | 1 | 1 | 1 | - |
| TouchOfElectricity | Touch of Electricity | Wind | A | A | 1 | 0 | 0 | the shock weapon gained at 11th level (gate) |
| PresenceOfDragons | Presence of Dragons | Dragon | B, C | B, C | 0 | 1 | 1 | - |
| ScaledToughness | Scaled Toughness | Dragon | A | A | 1 | 0 | 0 | the extra daily use at 13th level (resource threshold) |
| BreathWeapon | Breath Weapon | Dragon | A, B, C | A, B, C | 20 | 10 | 20 | the final breath weapon at 20th level (gate) |
| FormOfTheDragon | Form of the Dragon | Dragon | A | A, C | 40 | 0 | 40 | the forms gained at 15th and 19th level (gate) |
| Blizzard | Blizzard | Waves | A, C | A, C | 1 | 0 | 1 | - |
| IceArmor | Ice Armor | Waves | A, B | A, B | 1 | 1 | 0 | the damage reduction gained at 13th level (tier) |
| WaterForm | Water Form | Waves | C | C | 0 | 0 | 4 | the elemental body forms gained at 9th, 11th and 13th level (gate) |
| WintryTouch | Wintry Touch | Waves | A | A | 1 | 0 | 0 | the frost weapon gained at 11th level (gate) |
| PunitiveTransformation | Punitive Transformation | Waves | A, C | A, C | 1 | 0 | 1 | - |
| ErosionTouch | Erosion Touch | Nature | A, B | A, B, C | 1 | 1 | 1 | - |
| LifeLich | Life Lich | Nature | B, C | B, C | 0 | 1 | 1 | - |
| FormOfTheBeast | Form of the Beast | Nature | C | C | 0 | 0 | 8 | the forms gained at 9th, 11th and 13th level (gate) |
| GiftOfClawAndHorn | Gift of Claw and Horn | Nature | A | A | 6 | 0 | 0 | the second natural weapon gained at 11th level (gate) |
| ArmorOfBones | Armor of Bones | Bones | A, B | A, B | 1 | 1 | 0 | the damage reduction gained at 13th level (tier) |
| BleedingWounds | Bleeding Wounds | Bones | A | A | 1 | 0 | 0 | - |
| DeathsTouch | Death's Touch | Bones | A | A | 1 | 0 | 0 | - |
| RaiseTheDead | Raise the Dead | Bones | A | A, C | 1 | 0 | 2 | the extra daily use at 10th level (resource threshold) and the ability gained at 15th level (gate) |
| SoulSiphon | Soul Siphon | Bones | A, B | A, B | 1 | 1 | 0 | - |
| UndeadServitude | Undead Servitude | Bones | C | C | 0 | 0 | 1 | - |

Families found beyond the audit (all Oracle-engine parameters of the
revelation's own abilities): Aging Touch, Time Hop and Erosion Touch (caster
level only; no save), Form of the Dragon (the forms' own parameters) and Raise
the Dead (the 15th-level abilities, scaled once gained). Recorded for owner review.

Not offered (audit dispositions): explicitly deferred threshold-only
revelations (Temporal Celerity, War Sight, Cinder Dance, Molten Skin, Spark
Skin, Icy Skin, Weapon Mastery, Fluid Nature, Freezing Spells, Dragon Senses,
Draconic Resistances, Spirit of Nature, Near Death, Resist Life): their level
thresholds follow the same gate and rank rules, but publishing them needs new
owned target identities that this candidate's identity ledger does not contain;
unsupported branches (Maneuver Mastery, Enhanced Cures, Vortex Spells, Animal Companion); no oracle-level
scaling (Sacred Council, Burning Magic, Skill at Arms, Healing Hands, Safe
Curing, Wings of Air, Dragon Magic, Wings of the Dragon, Friend to Animals,
Nature's Whispers); Dual-Cursed Fortune (eligibility decision) and Misfortune;
Final Revelations (capstones).

## I08/S06 — selected bloodline power (1/6 per power, max +2)

| Key | Row | Power feature | Ability | Scales |
| --- | --- | --- | --- | --- |
| FireRay | I08 | `ce0889b5c1b392e48baf1e004d1efd67` | `1b4989258e5964149a909e47c72b7f67` | damage bonus rank |
| FireBlast | I08 | `3022a5066a5604a498dd289b37dfd8aa` | `b2d1d39cd406e0f4185c52fecc73c3b5` | dice, DC, caster level checks, extra daily uses at 17th and 20th level |
| AirRay | S06 | `acf668c24dfbcdd499276eaf1881486e` | `4729c2ac98d02004fb440d17f7786e28` | damage bonus rank |
| AirBlast | S06 | `553d9802d5d9de04b941b55cb47d3096` | `6d005cc9c3ad3f24e8769aad2fbfdf3f` | dice, DC, caster level checks, extra daily uses at 17th and 20th level |

Elemental Blast's extra daily uses are granted natively by the bloodline
progression at 17th and 20th level (`...ElementalBlastExtraUse`, +1 each); a
Blast counter adds exactly the extra uses between the real and the effective
level of the owner's eligible bloodline progression (read from that
progression's own level entries, so the Seeker and Crossblooded copies follow
their own tables), at most two steps.

Not targets: Elemental Movement, Body and Arcana, and Elemental Ray's uses (no
level scaling); Primal Elemental bloodlines (not fire or air elemental); efreeti
and djinni bloodlines (absent from the game). Explicitly deferred: Elemental
Resistance (its 9th-level step from resistance 10 to 20 is implementable with
the same threshold rule, but publishing it needs a new owned target identity,
which this candidate's identity ledger does not contain).

## O01 - selected bardic performance range (+5 feet per investment, max +30 feet)

Offered to Oread Bards for a performance they already have: one full-only
counter per performance (6 ranks, +30 feet). The manifest is classified by
mechanics (`FavoredClassPerformanceManifest`): a published target is a
maintained performance whose effect is a persistent cylinder area around the
bard. 14 targets are published, 2 registered counters are never published and
14 bardic entries are not targets.

Exact read points, all owner-local (only the bard who casts that instance):

- **Actual range:** an `AreaEffectView.InitAtRuntime` postfix
  (`FavoredClassPerformanceRangePatch`) sets that one view's
  `ScriptZoneCylinder.Radius` from the native `BlueprintAbilityAreaEffect.Size`
  plus 5 feet per earned step, on spawn and again when a save recreates the view
  (`AreaEffectEntityData.CreateViewForData`). Membership is the native
  `Shape.Contains` test on that cylinder. The shared blueprint `Size` is never
  written, and other performers of the same area keep the native radius.
- **Visible boundary:** the same postfix scales that instance's spawned ring
  (`AreaEffectView.m_SpawnedFx`) horizontally by owner radius / native radius
  (`FavoredClassPerformanceRing`). A ring that the native attach spawns later
  (an `AreaEffectView.SpawnFxs` postfix: a save load whose owner view was not
  ready) receives the same scale once. A `GameObjectsPool.Release` prefix
  restores the exact previous scales before the pooled effect is reused.
- **Displayed range:** `Fact.SelectUIData` and
  `MechanicActionBarSlotActivableAbility.GetDescription` postfixes
  (`FavoredClassPerformanceTextPatch`) show the owner's range in that owner's
  feature and toggle descriptions only (`FavoredClassPerformanceText`).
- **Eligibility:** `PrerequisiteFavoredClassOwnsAny` on the performance feature.
  Publication withholds the unpublished counters (`excluded-target:`).

| Key | Performance | Feature | Toggles | Areas | Native radius | Ring (`Fx.AssetId`) | Provider |
| --- | --- | --- | --- | --- | ---: | --- | --- |
| InspireCourage | Inspire Courage | `acb4df34b25ca9043a6aba1a4c92bc69` | `5250fe10c377fdb49be449dfe050ba70` | `5d4308fa344af0243b2dd3b1e500b2cc` | 50 ft | `2f93a2909cb766f4d961aee34a3c84c2` | Kingmaker |
| InspireCompetence | Inspire Competence | `6d3fcfab6d935754c918eb0e004b5ef7` | `430ab3bb57f2cfc46b7b3a68afd4f74e` | `c08bd33a377d5014a81be94e33ec8ce4` | 30 ft | `79665f3d500fdf44083feccf4cbfc00a` | Kingmaker |
| Fascinate | Fascinate | `ddaec3a5845bc7d4191792529b687d65` | `993908ad3fb81f34ba0ed168b7c61f58` | `a4fc1c0798359974e99e1d790935501d` | 30 ft | `725b02acb7286094688c0d5da974dcdc` | Kingmaker |
| DirgeOfDoom | Dirge of Doom | `1d48ab2bded57a74dad8af3da07d313a` | `d99d63f84e180d44e8f92b9a832c609d` | `4a15b95f8e173dc4fb56924fe5598dcf` | 30 ft | `20caf000cd4c3434da00a74f4a49dccc` | Kingmaker |
| InspireGreatness | Inspire Greatness | `9ae0f32c72f8df84dab023d1b34641dc` | `be36959e44ac33641ba9e0204f3d227b` | `23ddd38738bd1d84595f3cdbb8512873` | 30 ft | `3a0228650295f6a40bc335385a929a07` | Kingmaker |
| FrighteningTune | Frightening Tune | `cfd8940869a304f4aa9077415f93febe` | `ad8a93dfa2db7ac4e85133b5e4f14a5f` | `55c526a79761a3c48a3cc974a09bfef7` | 30 ft | `20caf000cd4c3434da00a74f4a49dccc` | Kingmaker |
| InspireHeroics | Inspire Heroics | `199d6fa0de149d044a8ab622a542cc79` | `a4ce06371f09f504fa86fcf6d0e021e4` | `1be964f750eea8748a76e92744746efb` | 30 ft | `79665f3d500fdf44083feccf4cbfc00a` | Kingmaker |
| InciteRage | Incite Rage | `35ac4bd7990fa0842bfc22e80665c2f9` | `dbd7c54ba43e1d54592e037d63117f7b`, `b1d8fdffd132bfd428a8045b7b8b363c`, `32d247b6e6b65794ab47fc372c444a96` | `8426523287601104085d71d410a6fc42`, `9c423eacfb7bb9f408757e651607e125`, `d63dce0f272ba2d4aa13000470398d63` | 30 ft | `20caf000cd4c3434da00a74f4a49dccc` | Kingmaker |
| FireDance | Fire Dance | `3c10a0069e7f110499d2e810f4861a6e` | `1b28d456a5b1b4744a1d87cf24309ad1` | `0bd2c3ff0012e6b468497461448174c7` | 30 ft | `79665f3d500fdf44083feccf4cbfc00a` | Kingmaker |
| SongOfFieryGaze | Song of Fiery Gaze | `edf5697b6ddc42fca14d20a03affd475` | `6f528fdd236b464795546db489d10f3b` | `b556833f0a0a45738863a02c78323fed` | 30 ft | `79665f3d500fdf44083feccf4cbfc00a` | Call of the Wild |
| Satire | Satire | `867e67a274d94c44bf6859b810745b1d` | `c23fed3a6e4b4caa82199a847d1b3fa5` | `b1125eb8eae649bdb441f22e3c088535` | 50 ft | `2f93a2909cb766f4d961aee34a3c84c2` | Call of the Wild |
| GloriousEpic | Glorious Epic | `d78e50c8ec9c436c82d3be6a028b4572` | `5fa0caff7bbe47399af61d16fb9620ab` | `0d961603708c4db3abf178e26d32fb1b` | 30 ft | `20caf000cd4c3434da00a74f4a49dccc` | Call of the Wild |
| Scandal | Scandal | `88d2e41984ea4c68968197b44ec2f445` | `a13ad8cc3fc545278b41d652aa3c1ca9` | `164dba1be13048eab380b302e4f25b7e` | 50 ft | `5d4308fa344af0243b2dd3b1e500b2cc` (links an area GUID: no ring spawns for any bard) | Call of the Wild |
| DanceOfTheDead | Dance of the Dead | `92d80172888643328f1638a4293fb3d8` | `06c88e8ce5be4235bb61d0d1c2655655` | `86e88e1394694fa6953e1bb82c76bc40` | 50 ft | `baa268c6db5723b4fa43c1b65f99bf0f` | Call of the Wild |

Registered but never published (no truthful display exists):

| Key | Performance | Feature | Areas | Reason |
| --- | --- | --- | --- | --- |
| StormCall | Storm Call | `161db4d6c4a1f4640ab52c762e15c1af` | `85c1ea0021ce2714f8559fb618bf7ff6` | its native description promises bolts on enemies within 50 feet, but its native area is 30 feet; no owner range can be displayed truthfully without rewriting the native text |
| Mockery | Mockery | `71a3c675a44d4a8a89c9a0840cb1d92a` | `eeb9c36c16be45dda7604b6120d1ab88` | its description is a single selected target with no range, while Call of the Wild implements a 30-foot area on every creature; widening that area would change a target-count rule, not a range |

Not targets (classified by mechanics):

| Entry | Feature | Classification |
| --- | --- | --- |
| Soothing Performance | `546698146e02d1e4ea00581a3ea7fe58` | instantaneous: a one-shot mass cure burst, not a maintained performance area |
| Deadly Performance | `a6e13797b0a20d2458a086a8a511fd8c` | instantaneous: a one-shot single-target ability |
| Thunder Call | `5ebb5d1f76b602d44818a21e9b6e31b8` | instantaneous: a one-shot burst ability |
| Archaeologist's Luck | `03bf87dd753cd4f48a47eaf0ea6da9fe` | personal: a self buff with no range |
| Blazing Rondo | `6e1f8dd4e17b41808e9f49e5a71dd9fc` | masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance |
| Banshee's Requiem | `22602cf4d9954ba2ae0223bcc27ff744` | masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance |
| Symphony of the Elysian Heart | `3dc71cb4cbaf467f8ba5c4dbab401603` | masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance |
| Clamor of the Heavens | `4f1a14d4d9314fd49a42ff2cef1b542b` | masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance |
| Triple Time | `b6e876fbe9a84eaaa80dc64c077dea49` | masterpiece and instantaneous |
| Dance of 23 Steps | `a8059b931829424c902777b70cd6fa84` | masterpiece and personal |
| Discordant Voice | `8064adc641c74e4cb821ce048ecd83a2` | inert: a feat that adds damage to other performances, not a performance |
| Bardic Performance (Move Action) | `36931765983e96d4bb07ce7844cd897e` | inert: action economy only |
| Bardic Performance (Swift Action) | `fd4ec50bc895a614194df6b9232004b9` | inert: action economy only |
| Lingering Performance | `17239b298065efc459cffe2220ecb559` | inert: duration only |

## O06/O07/O08 — auras and pets (1/4, uncapped)

- **O06:** the native Aura of Courage (fear) and Aura of Resolve (charm) ally
  buffs read the paladin's earned steps inside their own Morale bonus; not
  offered to an archetype that replaces both auras (Divine Hunter).
- **O07:** natural armor on the ranger's current animal companion; not offered
  to an archetype that replaces Hunter's Bond (Flamewarden, Freebooter,
  Stormwalker).
- **O08:** natural armor on Call of the Wild's eidolon of the summoner.

