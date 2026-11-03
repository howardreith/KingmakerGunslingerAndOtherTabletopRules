# Sprint 38 rules and engine contract — Glabrezu

This contract keeps four things separate: printed Pathfinder First Edition
requirements, the installed Kingmaker 2.1.7b surface, already accepted project
adaptations, and Sprint 38 decisions proposed for native proof. Source entries:
[Glabrezu](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Glabrezu),
[Demon](https://legacy.aonprd.com/bestiary/demon.html),
[Summon Monster](https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html),
[Power Word Stun](https://legacy.aonprd.com/coreRulebook/spells/powerWordStun.html),
[Reverse Gravity](https://legacy.aonprd.com/coreRulebook/spells/reverseGravity.html),
[Universal Monster Rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html).
These sources do not prove installed behavior.

Charter authority is the Sprint 38 row of the Expanded Summoning Implementation
Charter v1.0 (page 27). **The charter document itself is not present in this
repository or anywhere on this machine**; the Sprint 38 requirements used here
are the ones the mission quotes, and the roster/traceability ledgers
(`EXPANDED-SUMMONING-IDEAL-ROSTER.md`, `EXPANDED-SUMMONING-CHARTER-TRACEABILITY.md`)
agree with them: S38, phase 4, "Glabrezu", roster weight 4, one creature,
Summon Monster 9, no Nature's Ally, asset state `None`.

## Charter boundary as quoted by the mission

- A faithful Glabrezu body and routine; safe caster/bruiser AI.
- Power Word Stun with valid thresholds.
- A bounded, Kingmaker-compatible Reverse Gravity, with **no global
  physics/movement patch**.
- Wish and material-component abilities are outside scope.
- Unrelated humanoid/spider-rig shortcuts are explicitly rejected for this
  creature.

## Printed profile transcribed from the linked entry

CE Huge outsider (chaotic, demon, evil, extraplanar), CR 13, XP 25,600.
12d10+120 = 186 hp. Init +0; Perception +26; darkvision 60 ft., *true seeing*.
AC 28, touch 8, flat-footed 28 (+20 natural, -2 size). Fort +18, Ref +4,
Will +11. DR 10/good; immune electricity, poison; resist acid 10, cold 10,
fire 10; SR 24. Speed 40 ft. Melee 2 pincers +20 (2d8+10/19-20), 2 claws +20
(1d6+10), bite +20 (1d8+10). Space 15 ft., reach 15 ft. Special attacks rend
(2 pincers, 2d8+15). Str 31, Dex 11, Con 31, Int 16, Wis 16, Cha 20.
Base Atk +12; CMB +24; CMD 34. Feats Cleave, Great Cleave, Improved Critical
(pincer), Persuasive, Power Attack, Vital Strike. Skills Bluff +28,
Diplomacy +22, Intimidate +22, Knowledge (history) +18, Knowledge (local) +18,
Perception +26, Sense Motive +18, Stealth +7, Use Magic Device +17; racial +8
Bluff, +8 Perception. Languages Abyssal, Celestial, Draconic; telepathy 100 ft.

Spell-Like Abilities, CL 14th:
constant *true seeing*; at will *chaos hammer* (DC 19), *confusion* (DC 19),
*dispel magic*, *mirror image*, *reverse gravity* (DC 22), *greater teleport*
(self plus 50 lbs. of objects only), *veil* (self only), *unholy blight*;
1/day *power word stun*, summon (level 4, 1 glabrezu 20% or 1d2 vrocks 50%);
1/month *wish* (granted to a mortal humanoid only).

### Derived rather than hard-coded

* **Attack bonus.** +20 is `BAB 12 + Str 10 - 2 size`, with every one of the
  five natural attacks a full-Strength primary. The printed damage bonus is
  the plain +10, not +15, on all five limbs.
* **Rend.** 2d8+15 is `2d8 + 1.5 x live Strength bonus`, which is what the
  engine's own `RendFeature` computes. Nothing pins +15.
* **Every spell-like DC.** `10 + spell level + Charisma bonus` reproduces both
  printed DCs exactly: chaos hammer and confusion (level 4) give 19, reverse
  gravity (level 7) gives 22. Veil (level 6) gives 21 and unholy blight
  (level 4) gives 19 by the same formula; the entry prints no DC for either.
  So DCs come from the live Charisma through `ContextCalculateAbilityParams`,
  exactly as the released `ConfigureOwnedSpell35` already does, and are not
  constants.
* **Caster level 14 is not the hit dice.** The creature has 12 racial HD.
  `RuleCalculateAbilityParams` derives a level from the unit and will report
  12; the project's `ContextCalculateAbilityParams` puts 14 on the applied
  effect's own context. Both appear in receipts; they are different
  quantities and neither is a defect. This is the same split Sprint 37
  recorded for the Vrock's CL 12 against 9 HD.

### Every printed save derives; there is no anomaly

An earlier revision of this document claimed a PF1 outsider has three good
saves, concluded that the printed Reflex +4 therefore could not be derived,
and recorded it as a published stat-block anomaly carried as printed. **That
premise was wrong.** The outsider type grants **two** good saves, and which
two is per-creature rather than fixed. This entry's good saves are Fortitude
and Will, with a **poor Reflex**, and on the standard progressions at 12 hit
dice - good `2 + HD/2` = 8, poor `HD/3` = 4 - all three printed totals derive
exactly:

| Save | Base | Ability | Printed |
|---|---:|---:|---:|
| Fortitude | 8 (good) | +10 Con | **+18** |
| Reflex | 4 (poor) | +0 Dex | **+4** |
| Will | 8 (good) | +3 Wis | **+11** |

The values shipped were always the right ones; only the explanation was
wrong, and the assertion premised on three good saves is gone. The exact
racial-profile mapping expresses this creature's own good/poor selection and
is not a new balance adaptation, so nothing compensates for native difficulty
and no unrelated outsider is touched. Source for the type:
[Outsider](https://legacy.aonprd.com/bestiary/creatureTypes.html).

## Creature rules freeze

| Rule surface | Print | Installed engine | Existing accepted adaptation | Sprint 38 decision |
|---|---|---|---|---|
| Chassis | CE Huge outsider (chaotic, demon, evil, extraplanar), 12 HD, 186 hp, AC 28 incl. +20 natural, Fort +18/Ref +4/Will +11, DR 10/good, immune electricity and poison, resist acid/cold/fire 10, SR 24, space/reach 15 ft. | `InfernalProfile36`, `OutsiderLevels`, `SummonGiantRacialProfile`, `InfernalDefenses36` (`AddDamageResistancePhysical`, `AddStatBonus` natural armour, `AddSpellResistance`, energy resistance) and the native outsider/evil/chaotic/extraplanar and electricity/poison immunity facts are all released | Generated summon profiles own their stats; native difficulty modifiers stay separate from a printed-profile comparison | Reuse the released Sprint 36/37 demon chassis helpers unchanged with Glabrezu numbers and `coldIronOrGood: false` (DR 10/good only). Huge footprint with 15-ft space and reach. No gameplay value is bent to satisfy an observer |
| Movement | Speed 40 ft., no fly speed | Kingmaker exposes exactly one movement speed | Released creatures carry one effective speed | Land 40 ft. **This creature has no printed fly speed, so there is no dual-mode omission to disclose** — unlike the Vrock. No `Airborne` fact |
| Melee routine | Five primary natural attacks at +20: two pincers 2d8+10 (19-20), two claws 1d6+10, bite 1d8+10 | `Sprint29Weapon` immutable natural-weapon clones, `NaturalBody` primary plus additional limbs, the released full-Strength-primary correction, `SummonGiantEquipmentOwnership` exact-owner protection | Project profiles own attack roles rather than copying a donor; no imported secondary penalties | Three exact project weapons (pincer 2d8 19-20, claw 1d6, bite 1d8) in one body of **bite primary plus pincer, pincer, claw, claw as additional limbs in that order**; every limb a full-Strength primary. The limb order is load-bearing, not cosmetic — see the rend row. Native ordinary/single/full/charge/AoO commands keep their real action costs. No Pounce, no Grab, no imported cast-plus-full-attack economy, no attack-scheduling framework |
| Feats | Cleave, Great Cleave, Improved Critical (pincer), Persuasive, Power Attack, Vital Strike | `Cleave` `d809b6c4ff2aaff4fa70d712a70f7d7b`, `GreatCleave` `cc9c862ef2e03af4f89be5088851ea35`, `PowerAttack` `9972f33f977fc724c838e59641b2fca5` and `Persuasive` `86d93a5891d299d4983bdc6ef3987afd` are already in the project's GUID map or released builders. Native per-weapon `ImprovedCriticalBite/Claw/Gore` exist but **there is no native pincer weapon category**, so no native Improved Critical (pincer) feature can exist. Plain Vital Strike's identity is not recorded anywhere in this project (only Improved Vital Strike `52913092cd018da47845f36e6fbe240f`, from Sprint 29) | `SummonGiantCleaveDelivery` is the released private ordinary Cleave path, and Sprint 28/29/36 already grant the native Great Cleave feature beside it | Grant the four exact native feats that exist, reuse the released private Cleave path, and take Great Cleave from the released native feature. **Improved Critical (pincer) is delivered as the pincer weapon's own 19-20 critical range**, which is the printed effect, rather than by inventing a feat for a weapon category the game does not have; this is named as an adaptation, not hidden in the weapon. Plain Vital Strike's identity is resolved by the Sprint 38 input probe before it is wired, never guessed |
| **Rend (Ex)** | Rend (2 pincers, 2d8+15). Universal Monster Rules: triggered when two specified attacks hit the same target in the same round; **usable only once per round** | `Kingmaker.Designers.Mechanics.Facts.RendFeature` owns the damage (dice + 1.5 x live Strength as one event). The engine's own command-level gate keys on hand slot, cannot express equal primaries, and never checks that both hits landed on one creature | `DireApeRendGate` is the released project gate: it decides only *whether this attack is the rend*, keyed on an exact weapon blueprint and a `RendClawCount`, tracked per live unit in a non-serialized weak table, reset on command change, on `OnNewRound` and on `OnTurnOff`. Released at two claws (Dire Ape) and four claws (Girallon) | **A Glabrezu-local gate, because the released one cannot express the printed limit.** `DireApeRendGate` delegates to `ClawRendTracker`, whose `Follow()` calls `Reset()` whenever the attack command changes — clearing the emitted flag along with the claw bookkeeping. Its limit is therefore per *command*: a second qualifying pair in the same round rends again, and `OnNewRound` only clears state a command change would have cleared anyway. An earlier revision of this row claimed the command reset supplied the per-round rule; it does not. `GlabrezuRendGate38` keeps the **round allowance as its own state, with no attack-command operand at all**, so nothing a command does can refill it. The victim bookkeeping is round-scoped too, which is what the printed universal rule says — two named attacks on the same opponent *in the same round*, once per round — so a rend is correctly **allowed** when the two pincer hits land in different commands inside one round (an attack of opportunity then a full attack) and refused for the rest of that round once it has happened; a command-scoped pairing would have quietly under-triggered there. The round boundary is the installed one and covers both modes: `ITickEachRound.OnNewRound` is raised by the base engine's `UnitTicksController` every six seconds of game time outside turn-based combat and by the turn controller inside it. Only an armed attack that actually hit spends the allowance, so a miss leaves it intact and a duplicated event grants nothing. Limb identity is unchanged: `SummonLimbs.Classify` must report `Additional` with index `< 2`, so the pincers are additional limbs 0 and 1 — hence the fixed body order above — and the claws at 2 and 3 carry a different weapon blueprint. The released Dire Ape and Girallon keep `DireApeRendGate` and their behaviour **unchanged**, and a domain test asserts the released tracker's command-change reopening directly so the reason for the split is evidence rather than prose |
| Ordinary actions | — | Native standard attack, full attack, charge, AoO | Released summons keep these as distinct native commands | Unchanged. No five-attack standard action, no duplicate Strength or critical multiplication, no artificial Pounce, no extra action, no damage-trigger recursion. Power Attack and size/Strength effects are observed in their real weapon and event roles |
| **Power Word Stun (1/day)** | Enchantment (compulsion) [mind-affecting]; standard action; close range (25 ft. + 5 ft./2 levels = 60 ft. at CL 14); one creature with 150 hp or less; **no saving throw**; **spell resistance yes**; stuns for 4d4 rounds at 50 or fewer current hp, 2d4 at 51-100, 1d4 at 101-150; a creature with 151 or more hp is unaffected; hearing the word is not required | Native `UnitCondition.Stunned`, the native `Stunned` buff `09d39b38bb7c6014394b6daced9bacd3` recorded by the Sprint 37 audit, `AddCondition`, `SpellDescriptor.Stun`, native condition immunity and `UnitPartSpellResistance`. **Whether a native Power Word Stun blueprint exists is an open question** — see "What the retained dumps do and do not prove" | Sprint 29's `Sprint29LimitedNativeSpell` is the released once-per-summoning owned-spell-with-`BlueprintAbilityResource` pattern; Sprint 37's screech is the released native-stun carrier with live, derived parameters | Resolved by the input probe first. If a faithful native graph exists it is cloned through `ConfigureOwnedSpell35`/`Sprint29LimitedNativeSpell`; otherwise the ability is assembled from the native stun carrier and a current-hp duration selector. Either way: one independent single-use `BlueprintAbilityResource` under the project's once-per-summoning convention, standard action, close range, **no saving throw**, spell resistance applies, mind-affecting/compulsion descriptors set so native immunity refuses it, duration chosen from the recipient's **current** hit points at resolution, 151-or-more-hp unaffected, natural expiry, and reload restores the remaining state **without refilling the resource**. The Vrock's area screech is explicitly **not** cloned: this is a single-target, no-save, SR-yes ability and shares none of that shape. No fixed duration, no Fortitude save and no repeating end-of-turn save is imported from another game |
| **Reverse gravity (at will, DC 22)** | Transmutation; standard action; medium range (100 ft. + 10 ft./level = 240 ft. at CL 14); area up to one 10-ft. cube/level, shapeable; duration 1 round/level (dismissible); **no spell resistance**; saving throw none, see text. Unattached creatures and objects fall upward, reaching the area's top within one round; contact with an overhead surface deals damage as an equivalent downward fall; those reaching the top unobstructed hover there until it ends, then descend. A creature with a secure handhold may attempt a **Reflex save** to secure itself. Flight or levitation counteracts the upward motion | See "Reverse Gravity: what the engine actually has". In summary: the engine has **no gravity or levitation concept at all** — zero occurrences of `gravit` or `levitat` in the whole 103 MB `Assembly-CSharp` IL — but it does ship a complete, persisted, bounded vertical-capture framework in `AreaEffectPit` + `UnitPartInPit` + `UnitInPitController` | The released Wall of Ice path (`ConfigureWallAbility35`) is the project's point-targeted, medium-range, exact-owner area pattern. Wall of Ice itself is not a substitute for this effect | **Omitted by owner decision of 2026-11-03, and disclosed.** A bounded version was shown to be reachable and was offered with its exact contract; the owner chose omission, which is a charter change rather than a deferral under the charter. Nothing stands in for it and the `AreaEffectPit` framework is not used. See the decision record below |
| Chaos hammer, unholy blight (at will) | Level 4, DC 19 each from the shared formula | Both already consumed by the released Sprint 36 infernal builder | `ConfigureOwnedSpell35` with Charisma-derived DC and CL | Reuse the released graphs at CL 14 with live Charisma DCs. At-will, no resource |
| Confusion, dispel magic, mirror image (at will) | Level 4 / 3 / 2 | Native `MirrorImage` `3e4ab69ada402d145a5e0ad3ad4b8564` (named by the Sprint 37 audit); dispel magic is released in the Sprint 36 Babau; confusion eligibility exists natively | Sprint 37 gave the Vrock a **one-use** Mirror Image because the Vrock's entry prints 1/day | **At will, with no resource.** The Vrock's single-use resource is explicitly *not* inherited: this entry prints these three as at-will, and a previous creature's frequency is not this creature's. The released Babau dispel selector — which distinguishes a beneficial from a harmful effect from `BlueprintBuff.Harmful` and the caster's relationship to the owner — is reused unchanged and its fix is **not** reopened |
| At-will AI restraint | — | Native brain, `Consideration`, memory/LOS | Released self-buff AI admission refuses while the effect is already active | At-will does not mean recasting every tick. Already-active, useful-target and redundant-area checks go through the released consumers. **An AI restraint must not impose a hidden daily resource or change what the player can command**; the two are asserted separately. Area spells keep their real collateral — safe AI is not invented party immunity |
| **Power Word Stun AI target selection** | One creature, chosen by the caster | `BlueprintAiAction` carries **two** consideration lists, and the engine's own tooltip distinguishes them: `ActorConsiderations` are "Considerations that only depend on deciding unit", while `TargetConsiderations` are scored per candidate through `AiAction.ScoreTarget`, which multiplies each into that candidate's `DecisionContext.CurrentScore` — so a zero removes the candidate rather than discouraging it | The released Vrock screech attaches its eligibility to `ActorConsiderations`, which is right for an *area* ability where the actor-level question is the whole radius | Suitability sits on **both** paths, and the per-candidate one is the load-bearing half. An actor-only check creates a real selection gap: the action is admitted because enemy A is eligible, the native selector picks enemy B, and the single daily use is spent on a creature the printed spell cannot touch. `GlabrezuStunTargetEligibility38` is attached to `TargetConsiderations` and decides the actual candidate; `GlabrezuStunEligibility38` remains a cheap actor precheck. Both call one shared predicate so they cannot disagree. Target protection uses the released `SummonDescriptorImmunity` pair rather than `UnitPartSpellResistance.IsImmune` alone, because that part answers only for `SpellImmunityToSpellDescriptor` and a creature protected solely by a `BuffDescriptorImmunity` would otherwise read as usable. **Numerical spell resistance is deliberately not consulted during scoring** — it is a roll the engine makes at resolution, and a synthetic roll here would be a second, wrong adjudication. Manual availability and the native effect, save, immunity and resource semantics are unchanged |
| Constant *true seeing* | Printed as a constant spell-like ability | Not yet resolved; this project has never recorded a true seeing identity anywhere | The summon sanitization policy's `ExpensiveMaterialComponent` hazard is the charter's material-component boundary. PF1 true seeing has a 250 gp ointment component **as a spell**; as a spell-like ability it has no components at all, and `ConfigureOwnedSpell35` already clears `MaterialComponent` | Disposition decided on the probe's live evidence, then recorded. If a faithful native consumer exists it is applied as a permanent self-effect on the summon. If it does not, it is named as unimplemented. **No unimplemented sense is claimed and no forbidden use is silently granted** |
| *Veil* (self only) | Level 6 illusion (glamer), self only in this entry | Not yet resolved | Released creatures record abilities with no supported summon consumer | Scope decided early from the probe. A self-only disguise with no mechanical effect in this game is recorded as unrepresented rather than faked. No new illusion framework |
| *Greater teleport*, summon, *wish* | At-will greater teleport (self + 50 lbs.); 1/day summon; 1/month wish granted to a mortal | Installed travel and summon graphs exist but are excluded for summons | `SummonUnitSanitizationPolicy` already forbids `TeleportationOrPlanarTravel`, `CreatureSummoningOrConjuration`, `QuestOrStory` and `ExpensiveMaterialComponent` | Omitted precisely through that existing policy. No new blanket spell-like policy, no wish, no story-bargain system, no nested summon |
| Skills and senses | Bluff +28, Diplomacy +22, Intimidate +22, Knowledge (history/local) +18, Perception +26, Sense Motive +18, Stealth +7, Use Magic Device +17; racial +8 Bluff and +8 Perception; darkvision 60 ft.; telepathy 100 ft. | `SummonGiantRacialProfile` carries Perception/Stealth/Mobility ranks; `AddStatBonus` carries racial bonuses | Released profiles carry only skills with real summon consumers and record the rest | Carry the racial +8 Perception and the Perception and Stealth ranks through the released consumers. Bluff, Diplomacy, Intimidate, both Knowledges, Sense Motive, Use Magic Device, telepathy and the passive senses are recorded as unrepresented, consistent with every released sprint. No new skill framework |

## Public surface

Exactly one project creature, stable key `glabrezu`, **one** untemplated Summon
Monster route and no Nature's Ally entry, template child or variant:

| Tier | Quantity |
|---|---|
| IX | One |

Source-derived expectation if no unrelated catalog change occurs:

| Surface | Reviewed baseline | Sprint 38 expected |
|---|---:|---:|
| Unique project creatures | 131 | 132 |
| Monster entries / placements | 109 / 597 | 110 / 598 |
| Nature's Ally entries / placements | 105 / 588 | 105 / 588 |
| Generated placements | 1,185 | 1,186 |
| Retained wrappers | 29 | 29 |
| Visible choices | 1,214 | 1,215 |

The two pins that move are `ExpandedSummoningIdentityCatalog.LogicalAbilityCount`
and `SummonVisibilityCatalog.RegisteredLogicalPlacementCount`, both currently
1,185. Totals are derived from source; aggregate pins are not scattered across
old sprint tests.

## Demon identity

`SummonDemonIdentityCatalog` is the one authoritative set. The Glabrezu's own
printed type line is **CE Huge outsider (chaotic, demon, evil, extraplanar)**,
so it is the next actual demon member: six keys become seven
(`dretch`, `shadow-demon`, `succubus`, `babau`, `hezrou`, `vrock`, `glabrezu`),
the ten non-demon outsider exclusions are preserved unchanged, and
`Validate()`'s six-member pin moves to seven. Membership is resolved by
registered key and blueprint identity, never by alignment, visual donor,
faction or name matching.

The affected integration is the Vrock's Stunning Screech exemption: the new
Glabrezu must be exempt while an appropriate **non-demon control** remains
eligible and is actually asked to save. Existing members keep their
source-level identity controls. The Screech, Spores and Babau campaigns are
not replayed, and the recorded third-party/campaign-demon interoperability
limitation is unchanged — no compatibility-taxonomy project.

## Requirement map

| Requirement | Existing consumer | Actual gap | Smallest change | Selected proof |
|---|---|---|---|---|
| One creature, one Monster-only root at tier IX | Append-only catalog, donor catalog, visibility filter, generated summon graphs | No `glabrezu` identity or dependency exists | One catalog row at Monster tier 9, one donor row, one visibility route assertion, the identity specs, and the two 1,185 pins | Source-derived roster counts plus a final-byte public run of the one parent, its quantity, cost, CL 20 duration and cleanup |
| Huge demon chassis, five full-Strength primaries, Huge footprint, feats | Sprint 36/37 demon chassis helpers, `Sprint29Weapon`, `NaturalBody`, released full-Strength correction, private Cleave, native Great Cleave/Power Attack/Persuasive | Glabrezu numbers, a three-weapon five-limb body, the pincer crit range and plain Vital Strike's identity are absent | One new builder partial reusing the released helpers | Hidden profile/body run plus native attack-mode observations in both combat modes |
| Pincer-only rend, once per round, live modifiers | `DireApeRendGate` plus native `RendFeature` | No Glabrezu rend feature or pincer weapon exists | One rend configuration with `Claw` = pincer and `RendClawCount` = 2, and the fixed limb order | Native run: two pincer hits on one victim produce exactly one rend packet with live Strength and real attribution; a claw or bite pair does not; a second command in the same round does not |
| Power Word Stun: thresholds, no save, SR, real action and resource | Native stun carrier, `Sprint29LimitedNativeSpell`, `AbilityResourceLogic` | The ability, its resource, the hp-bucket selector and the native identity are all absent | One ability on one single-use resource plus one current-hp duration selector | Native run: real cast and action debit, actual recipient, hp bucket at resolution, native immunity and SR refusals, the 151-or-more-hp no-effect case, natural expiry, and reload with no refill. Boundary policy tested compactly with native representatives per outcome, not every hp value by every mode by every profile |
| At-will chaos hammer, unholy blight, confusion, dispel magic, mirror image with AI restraint | Released Sprint 36 graphs, released Babau dispel selector, released self-buff AI admission | Five owned at-will abilities and their admission wiring are absent | One builder section reusing the released helpers, with no resources | Native run: real casts, no resource debit, no recast while satisfied, useful fallback, real collateral, manual availability unchanged |
| Constant true seeing and veil dispositions | None yet | Neither identity nor disposition is recorded | Either one released permanent self-effect or one recorded omission, decided on probe evidence | The probe receipt plus, if implemented, a live observation of the actual native consumer |
| Reverse Gravity | — | No gravity concept exists in the engine | **None. Omitted by owner decision of 2026-11-03** | No implementation and therefore no gameplay evidence. The disclosure itself is verified in the release notes and the status record |
| New Vrock exemption with a non-demon control | `SummonDemonIdentityCatalog`, released screech exemption and consideration | The Glabrezu is not in the set; the six-member pin blocks it | One key, the updated pin, and the affected six-member expectations | One targeted integration run: Glabrezu exempt, non-demon control asked and stunned |
| Distinct original four-armed body and real icon | Project-owned Blender 4.5 procedural body pipeline (`generate_sprint37_vrock.py` and its shared modules) and the project-owned procedural icon renderer (`render_creature_icon.py` into `New-ExpandedSummoningIcons.ps1`) | No Glabrezu geometry, albedo or icon exists; the icon count pin is 145 | One new body generator and one new icon creature in the existing renderer, both reusing the measured rig capture | Hash/provenance records, the strict package inventory, and the native presentation run identifying the actual packaged mesh |
| ON/OFF persistence | Released three-process Working envelope | The new resource, stun state and any area state need coverage | Extend only the Sprint 38 persistence reader/dispatch | Six fresh final-candidate processes with exact Working and installation restoration |
| One stable exact-byte release | Full Sprint gate, reproducibility, guarded publisher | Sprint 38 binding and records are absent | A named selector and strict preflight | Complete gate, identical reproduction, publisher refusals, preflight, public download and lineage verification |

## What the retained dumps do and do not prove

Three installed mods keep blueprint dumps
(`Mods/CallOfTheWild/loaded_blueprints.txt` and two smaller ones). They looked
like a blueprint census that could settle identities without a guarded launch.
**They cannot.** Three known-native GUIDs — Mirror Image
`3e4ab69ada402d145a5e0ad3ad4b8564`, Heroism `5ab0d42fb68c9e34abae4921822b9d63`
and the `Stunned` buff `09d39b38bb7c6014394b6daced9bacd3`, all three named by
the Sprint 37 audit — appear **nowhere** in those 23,135 rows. The dumps list
only each mod's own additions.

So the following are recorded as **UNKNOWN**, not as absences, and go to the
input probe: native Power Word Stun, Reverse Gravity, Create Pit and its pit
visual settings, True Seeing, Veil, Confusion, Chaos Hammer, Unholy Blight,
Dispel Magic, plain Vital Strike, and any native Glabrezu unit. In particular,
the four `...PowerWordStun` rows in those dumps are CallOfTheWild's own
blueprints and are **not** evidence that the installed game ships one; and the
zero `gravit` hits in the same file are evidence about CallOfTheWild, not about
Kingmaker.

## Reverse Gravity: what the engine actually has

This section is engine-code evidence from the retained 103 MB
`Assembly-CSharp.il` disassembly in `artifacts/inspection/bodyguard-native`,
read offline with no guarded transaction spent. It is authoritative about what
component types exist. It says nothing about which blueprints ship.

### There is no gravity and no levitation

Across the entire assembly there are **zero** occurrences of `gravit` and
**zero** of `levitat`, in any casing — no component, rule, controller, part,
enum member or field. Positive controls on the same file in the same way:
`Stunned` appears 36 times, and the `AbilityCustom*` family enumerates as
`Animation`, `AnimationByBuff`, `Charge`, `Cleave`, `DimensionDoor`,
`DimensionDoorTargets`, `DweomerLeap`, `FlashStep`, `Fly`, `Logic`,
`MeleeAttack`, `Overrun`, `TongueGrab`. So the printed spell has no direct
consumer, and the printed "flight or levitation counteracts it" clause has no
levitation consumer at all.

That rules out a faithful implementation. It does **not** rule out a bounded
one, and missing a specialized class name does not prove all bounded options
impossible.

### There is a complete bounded vertical-capture framework

`Kingmaker.UnitLogic.Abilities.Components.AreaEffects.AreaEffectPit`
(extends `AbilityAreaEffectLogic`) is Kingmaker's Create Pit implementation,
and it is far more general than its name:

```
VisualSettings              BlueprintAreaEffectPitVisualSettings  [ValidateNotNull]
ClimbDC                     ContextValue
OnFallAction                ActionList
EveryRoundAction            ActionList
OnEndedActionForUnitsInside ActionList
MaxUnitSize                 Size
ImmunityFacts               BlueprintUnitFact[]
```

`Kingmaker.UnitLogic.Parts.UnitPartInPit` drives a ten-state machine —
`ReadyToEvade, Evading, ReadyToStepOnEdge, OnEdge, ReadyToFallInto,
FallingInto, ReadyToFallOnFloor, OnFloor, ReadyToClimbOut, ClimbingOut` — and
**persists**: `m_AreaEffectId`, `m_AreaEffectBlueprint`, `m_Captured`,
`m_Disappeared` and `m_Hidden` all carry `[JsonProperty]`, so capture survives
a save and load.

`Kingmaker.Controllers.Units.UnitInPitController` decides entry. Decompiled
from the IL, at state `ReadyToEvade`:

```
immune = ImmunityFacts.Any(f => UnitHelper.HasFact(unit.Descriptor, f))
evade  = unit.Descriptor.State.Size > settings.MaxUnitSize ? true : ReflexCheck(part)
State  = (evade || immune) ? Evading : ReadyToFallInto
```

and `ReflexCheck` is:

```
if (caster != null && !owner.IsEnemy(caster) && owner.GroupId != caster.GroupId
    && !owner.Faction.Neutral) return true;            // auto-evade
if (owner.State.IsHelpless || owner.State.HasCondition(UnitCondition.ForceMove))
    return false;                                       // auto-fail
rule = new RuleSavingThrow(owner, SavingThrowType.Reflex, context.Params.DC);
if (part.IsOnEdge) rule.AddTemporaryModifier(+2 to SaveReflex);
return Rulebook.Trigger(rule).IsPassed;
```

`SavingThrowType.Reflex` is enum value 2 and `UnitCondition.ForceMove` is 27,
both confirmed from their own enum declarations.

So the native framework already supplies, with no patch: a point-anchored
finite-duration area; **a Reflex save against the ability's own DC** to avoid
capture; a +2 edge bonus; automatic evasion above a size cap and for an
enumerated immunity-fact set; automatic failure when helpless or already being
force-moved; damage on transit (`OnFallAction`); a per-round hook; a distinct
action for those still held when the effect **ends**
(`OnEndedActionForUnitsInside`, which the component runs only for units in the
four captured states); a climb-out difficulty; and persisted capture state.

### The motion is actually reversible

`BlueprintAreaEffectPitVisualSettings` is a plain `BlueprintScriptableObject`
this project can construct, with `DepthMeters` (default 2.5),
`HoleEdgeMeters` (default 1.7), nullable `UnitDisappearFx`/`UnitAppearFx`
GameObjects and four `AnimationCurve`s (`FallXZCurve`, `FallYCurve`,
`ClimbXZCurve`, `ClimbYCurve`). The vertical position is computed as

```
unit.FlyHeight = (-yCurve.Evaluate(t)) * settings.DepthMeters * torsoScale.x
```

so a **negative** `DepthMeters` makes `FlyHeight` positive and the captured
unit **rises** instead of sinking. Release already restores the ground state:
the controller calls `ReleaseUnit()`, `ShowAndAppearUnit()`,
`set_FlyHeight(0)`, restores `TargetPosition` and removes the part.

That is genuine upward displacement produced by the engine's own controller.
It is not a view-only lift: capture removes the unit from ordinary play, and it
is not a stun either.

### What a bounded version still cannot match

`AreaEffectShape` has exactly three members — `Cylinder`, `Wall`, `AllArea`.
There is no cube and no shapeable multi-cube region, so the printed "up to one
10-ft. cube per level, shapeable" cannot be expressed; a cylinder is the only
honest choice. There is no ceiling or overhead-surface detection anywhere, so
"contact with an overhead surface" has to become a chosen lift height with the
matching printed falling damage. The printed effect also has no climb-out
escape, while the native framework always offers one.

### Owner decision, 2026-11-03: Reverse Gravity is omitted and disclosed

The bounded contract above was put to the owner as one concrete decision, with
the exact feasible effect, the severity options and every difference from print
stated up front. **The owner selected omission.**

Reverse Gravity is therefore **not represented** in this creature, and is
disclosed in the release notes beside greater teleport, nested summon and wish.
Nothing stands in for it: no stun, no view-only lift, no pit, no repurposed
area effect, no damage substitute. The `AreaEffectPit` framework is **not**
used by this sprint, and no project-owned pit visual-settings blueprint is
created.

This is recorded as a **charter change, not a deferral under the charter.**
The option the owner chose said so explicitly: the Sprint 38 charter row
selects a bounded adaptation rather than permitting deferral, so unlike the
Vrock's Dance of Ruin — which the Sprint 37 charter row itself allowed to be
deferred — this omission overrides a charter requirement. It is the owner's
call to make and it was made with that conflict in front of them.

The feasibility evidence above is retained rather than deleted. It is the
reason the decision was informed, it establishes that the omission is a
balance and fidelity choice rather than an engineering dead end, and it is the
starting point if the owner ever revisits it.

A second question confirmed that the engine-limit disclosures (cylinder
instead of shapeable cubes, a fixed lift height, creatures and not objects, an
exact-identity flight exemption, the engine's inherited auto-evade, no new
VFX) were acceptable to disclose and ship. With omission selected there is no
bounded effect for those particular disclosures to attach to, so that answer
is recorded as confirming the project's disclosure standard and **not** as
authorizing a bounded Reverse Gravity.
