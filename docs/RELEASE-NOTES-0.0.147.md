# 0.0.147-expanded-summoning-sprint18

Kingmaker Gunslinger 0.0.147 is the Expanded Summoning Sprint 18 candidate:
the Ape and the Dire Ape. It retains the qualified native Wwise firearm bank,
SHA256 `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`, and
changes no firearm audio asset. The installation artifact is
`KingmakerGunslinger-0.0.147-expanded-summoning-sprint18.zip`. Existing
`CraftMagicItems.dll` compatibility is inherited; no new crafting integration
is claimed. Historical content and fatigue-authority records retain their 1,288
and 1,325 test counts; neither describes this candidate's larger current suite.

It is built on the released v0.0.146 master, commit
`da782d2297d6cf361ac30a5e0ea07348d3b7f7d7`, and changes nothing that release
qualified.

**This candidate is NOT runtime qualified.** Both new creatures are registered
and withheld: no player can select either, and the visible summon surface is
exactly what v0.0.146 published. The sections below describe what the source
contains, not what has been proved in game.

## What Sprint 18 registers

| | Summon Monster | Summon Nature's Ally | Roots |
| --- | ---: | ---: | ---: |
| Ape | III | III | 7 + 7 = 14 |
| Dire Ape | IV | IV | 6 + 6 = 12 |

Both follow the existing celestial/fiendish template policy and the charter's
quantity propagation: single at their own tier, 1d3 one tier higher, 1d4+1
above that. Registered generated placements rise from 1008 to 1034, all 26 new
roots are suppressed, and the published surface stays at 1008 generated plus 29
retained native wrappers - 1037 visible choices, unchanged.

Identities are allocated once, now, and never move: publication is the removal
of two suppression keys. The blueprint ledger grows from 2922 to 2980 entries
by append only.

## Printed profiles

**Ape**, N Large animal, 3d8+6 (19 hp), AC 14/11/12, Fort +7 Ref +5 Will +2,
speed 30 ft., two slams +3 (1d6+2), Space 10 ft., Reach 10 ft., Str 15 Dex 15
Con 14 Int 2 Wis 12 Cha 7, BAB +2, CMB +5, CMD 17, Great Fortitude, Skill Focus
(Perception). Exactly two primary slams: no bite, no claw, no rend, and no
chest-thump fear or roar, none of which the stat block prints.

**Dire Ape**, N Large animal, 4d8+12 (30 hp), AC 15/11/13, Fort +7 Ref +6
Will +4, speed 30 ft., bite +6 (1d6+4) and two claws +6 (1d4+4), Space 10 ft.,
Reach 10 ft., rend 1d4+6, Str 19 Dex 15 Con 16 Int 2 Wis 12 Cha 7, BAB +3,
CMB +8, CMD 20, Iron Will, Skill Focus (Perception). All three attacks are
primary, at the same bonus and the whole Strength modifier.

## Rend

Rend resolves once after both claws hit the exact same creature in one attack
sequence. It does not fire from one claw, from claws that hit different
creatures, from a bite and a claw, across turns or commands, or from a replayed
rule event, and no sequence rends more than once.

The damage is the engine's own: Kingmaker's `RendFeature` deals the feature's
dice plus one and a half times the live Strength modifier as a single damage
event, which is the printed 1d4+6 for an unmodified Dire Ape and follows a
buffed, enlarged or weakened one exactly.

Only the decision is this project's. The engine's command-level rend gate fires
when a secondary-hand attack follows a primary-hand hit, which identifies a rend
by which slot a limb is kept in rather than by which limb it is: it cannot
express a bite plus two equal primary claws, it would rend from a bite-then-claw
pair, and it never checks that both hits landed on one creature. A bounded
Dire-Ape-owned gate replaces that one decision and owns no damage, no dice and
no rule. Because the same engine gate also selects the rend attack animation
variant, that variant is not played; the rend is still a visible damage event.
This is recorded as a presentation deviation and nothing is invented to replace
it.

No grab, pin or armor-targeting behaviour is implemented. The flavour text's
remark that dire apes may grapple difficult foes is not a printed special
attack.

## Honest omissions

- `ORDINARY_MAP_LAND_USE_SCOPE`: Kingmaker exposes one movement speed, so the
  30-foot ground speed is used and the 30-foot climb speed is omitted. The
  printed Climb +14 and +16, and the +8 racial climb bonus inside them, have no
  faithful ordinary-map consumer and are omitted rather than substituted.
  Mobility is not raised to stand in for Climb and Athletics is not raised to
  simulate the racial bonus. Each ape's remaining printed ranks are allocated
  exactly - one Mobility and one Perception for the Ape, one each of Mobility,
  Perception and Stealth for the Dire Ape - and the rank that bought Climb is
  not reallocated.
- `PASSIVE_CREATURE_SENSES_UNMODELED`: low-light vision and scent are omitted on
  both apes under the owner's accepted limitation. Nothing is substituted for
  them - no blindsense, no vision-range override - and no record claims they
  work.

## Visuals

Both creatures ship two new original project-owned 128x128 icons, rendered
procedurally with the project's own Blender creature-icon tool. The Ape is a
dark knuckle-walking silverback; the Dire Ape is a heavier red-brown
gigantopithecus with bared canines and clawed hands.

**The original creature bodies are NOT authored.** Both apes currently ride the
Owlbear's Large bipedal donor rig and are recorded as borrowed-body visual
proxies. The installed library carries no primate unit type at all, so neither
ape can ever clone a native primate and each owns its own inspectable type. The
bounded read-only primate donor census that the original meshes depend on has
not run. This is the main reason both creatures are withheld.

`HumanReview: NOT_PERFORMED_NONBLOCKING.`

## Uninstall

To uninstall, remove the mod directory, exactly as before. Withheld creatures register no
player-visible choice, so a save made with this build loads without them exactly
as it loads without any other unreleased content.
