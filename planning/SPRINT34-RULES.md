# Sprint 34 PF1 contract

Sources: adult Bestiary entries for [Barbazu](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Bearded+Devil+(Barbazu))
and [Kyton](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Kyton),
cross-checked with [legacy devil](https://legacy.aonprd.com/bestiary/devil.html)
and [legacy Kyton](https://legacy.aonprd.com/bestiary/kyton.html).
[Summon Monster](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon+Monster+5)
and the actual ideal catalog select SM V, no SNA, no templates. No nested summons,
teleportation, campaign scripts, XP or transferable equipment.

## Printed baseline and derivation

| Field | Bearded Devil | Kyton |
|---|---|---|
| Type | LE Medium outsider; devil, evil, extraplanar, lawful | LE Medium outsider; evil, extraplanar, kyton, lawful (not devil) |
| HD / BAB / HP | 6d10 / 6 / 33 racial + 6 * Con modifier = 57 | 8d10 / 8 / 44 racial + 8 * Con modifier = 60 |
| Str/Dex/Con/Int/Wis/Cha | 19/15/19/6/12/10 | 15/17/14/11/12/12 |
| Saves | Fort 5+4=9; Ref 5+2=7; Will 2+1=3 | Fort 6+2=8; Ref 6+3=9; Will 2+1=3 |
| AC | 10+2 Dex+7 natural=19 | 10+3 Dex+4 armor+4 natural=21 |
| Defenses | DR 5/good or silver; fire/poison immune; acid/cold resistance 10; SR 16 | DR 5/silver or good; cold immune; SR 17; regeneration 2, bypass good weapons/spells or silver weapons |
| Movement / reach | 40 ft.; claws 5 ft., glaive 10 ft. | 30 ft.; body 5 ft., chains 10 ft. |
| Routine | Glaive +11/+6, 1d10+6 OR two claws +10, 1d6+4 | Four chains +11, 2d4+2 |
| Derivation | BAB6+Str4+glaive Focus1; two-handed damage 1.5 * positive Str (full negative); claws ordinary Str | BAB8+Str2+chain Focus1; damage ordinary Str |
| Feats | Improved Initiative, Power Attack, Weapon Focus (glaive) | Alertness, Blind-Fight, Improved Initiative, Weapon Focus (chain) |

Live bonuses must use native signed ability modifiers, buffs, feats and weapon
mode; never bake final donor totals. Chain Armor is a typed +4 armor bonus,
without wearable-armor penalties. Preserve named unsupported skills as reference
values, without reallocating their ranks or granting substitute bonuses.
Passive/world senses stay within the established scope.
Named unsupported references: Barbazu Climb +13 and Sense Motive +6; Kyton
Climb +13, Craft (blacksmithing) +11 and Escape Artist +14. No broad Athletics,
Persuasion or crafting substitute is granted. Controlled-chain climbing has no
world-chain consumer under the approved owned-chain policy. Existing Mobility
handles Kyton Acrobatics +14; Intimidate has its own native check consumer.

Barbazu's both-claw, same-opponent condition causes separate 1d8+2 beard damage.
Disease exposure follows its injury contract: Fort DC17 (Con-based),
1d4-day onset, daily 1d4 Strength damage, three consecutive saves to cure.
No combat-round acceleration. Damaging glaive attacks cause bleed2.
Heal DC17 stops it; spell healing needs CL DC16 or fails. Successful spell
healing works and stops bleed. Repeated wounds do not multiply same-type ticks.

Kyton's gaze has 30-foot range, Will DC15 (Cha-based), shaken1d3 rounds and
mind-affecting/fear descriptors. Its printed control operation spends a standard
action on up to four chains within20ft, extends them up to15ft and adds barbs.
A possessed chain allows Will DC15; successful resistance prevents another
attempt for24hours or until possession changes. Controlled-chain climbing
uses normal speed without checks.

## Installed contract evidence

Read-only reflection of installed Assembly-CSharp confirms:
`RuleHealDamage.OnTrigger` calculates healing using its nullable Modifier,
then changes target Damage and notifies healing consumers. Blocking HP healing
alone is insufficient until every removal/action consumer has been traced.
`AddEffectRegeneration` exposes Heal, energy/alignment/material bypass arrays,
active state and native round/death handling; inspect actual graph and outcomes.
The installed `StatType` enum has no Heal skill. A broad type scan reports a
HashSet transparency loader exception, so it is not exhaustive absence proof.

Local scripts/IL/results remain under `artifacts/sprint34-intake/`.
Native type signatures are research, never native-dispatch PASS.

## Approved action and ownership adaptations (2026-10-29)

| Operation | Printed rule / native possibility | Approved policy |
|---|---|---|
| Ordinary routine | Four chains; ordinary native single/full attack commands | Keep one or four attacks, 10ft chain reach, normal costs |
| Control | Standard action; at most four existing chains in20ft | Control the summon's four already-owned chain items; no attack on activation |
| Extension | Up to15ft extra length; no global body reach | Chain weapons alone may reach25ft while controlled |
| Later attacks | Chains attack with Kyton effectiveness; print does not settle a native command cadence | Reuse the same weapons and attack commands/costs; no independent or extra routine |
| Ownership | Printed possession save and retry boundary | No foreign/player inventory control or general environmental simulation; owner approved this restriction |
| Retirement | Charter requires summon/summoner area boundary | Exact-source control ends on expiry/death/dismissal/area departure; sibling control remains |

The owner approved the owned-chain policy above and a targeted standard-action
gaze: 30-foot range, live Charisma DC (15 at baseline), shaken 1d3 rounds,
native fear/mind-affecting immunity and sight checks. Players may select eligible
allies; AI selects enemies. This replaces the passive gaze/avert-eyes cycle.

For absent Heal access, the owner instructed: "Don't worry about implementing
the heal skill." No new Heal skill or Lore(Nature) replacement action is selected.
The printed Heal DC17 cure remains a documented unavailable native skill option;
the actual spell-healing CL16 check and successful cure remain required.

Source cleanup removes only that source's ongoing effects. Disease timing
remains days, and its source lifetime remains summon-safe. Successful spell
healing stops bleed under the printed wound contract; rejected healing must
neither restore HP nor accidentally invoke ordinary heal-removal consumers.

Installed inspection also found `RemovedByHeal.OnEventDidTrigger` removes its
buff without checking `RuleHealDamage.Value`; `AddHealTrigger` does check a
positive value. A zero modifier alone is therefore insufficient. The retained
Filth Fever blueprint graph saves on NewRound, so it cannot be cloned unchanged
for Devil Chills' day-scale onset/frequency. These are implementation findings,
not native gameplay acceptance.

Installed regeneration IL selects positive final damage and reads alignment
from every BaseDamage, while silver selection is limited to PhysicalDamage.
Installed OnNewRound restores regeneration after only3 seconds. It also does
not read a good spell's source descriptor. The Kyton-only subclass reimplements
the two actual native callback interfaces, retaining native healing, difficulty
and death-prevention ownership. Positive good/silver injury suppresses the
following round; the subsequent callback restores native healing. A good native
spell/SLA descriptor also qualifies. Serialized suppression survives cold load.
Compiled interface maps confirm both callbacks dispatch to the subclass; this
is not gameplay proof. Development mechanics04 subsequently observed native
following-round, good-spell and fatal-injury behavior. Cold hydration and final
qualification remain required. Other creatures' regeneration is unchanged.
CotW's bleed donor uses its shared strongest-bleed selector; its private clone
keeps that consumer. Source removal reselects remaining live bleed facts because
the installed selector does not do this itself. Stock bleed uses its actual
native direct HP action only when the inspected graph resolves. Its donor's
activation-pruning family identifies stronger ordinary bleeds; the wound keeps
its own marker/healing gate but yields its tick while one is active. CotW native
removal reselects remaining facts on that exact patient. Stock03 proved native
spell rejection/closure and same-type owned ticks, but exposed coexistence
stacking. Fresh stock/CotW coexistence, resumption and source-isolation proof is
required for this repair; effects01 did not observe the new coexistence cells.
