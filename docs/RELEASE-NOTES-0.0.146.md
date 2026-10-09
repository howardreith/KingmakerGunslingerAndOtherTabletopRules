# 0.0.146-expanded-summoning-phase2b-checkpoint

Kingmaker Gunslinger 0.0.146 retains the qualified native Wwise firearm bank:
SHA256 `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
No firearm audio asset is changed by this release.
Existing `CraftMagicItems.dll` compatibility is inherited; no new crafting
integration or full Craft Magic Items profile qualification is claimed.
Historical content and fatigue-authority records retain their 1,288 and 1,325 test
counts; neither describes this candidate's larger current suite.

This is the owner-authorized numbered stable-release scope, not an alpha or
prerelease. Publication requires every exact integration and runtime gate;
candidate and release evidence are recorded separately from this package.

This checkpoint combines accepted master v0.0.145 (including v0.0.142 traits) with the qualified Phase2A
and Phase2B summoning tree; PR25/26 are evidence, not direct merge targets.
The installation artifact is
`KingmakerGunslinger-0.0.146-expanded-summoning-phase2b-checkpoint.zip`.

## Included content

The previously unreleased Sprint 12 Dire Rat, Dog, Hyena and Goblin Dog; Sprint
13 Wolverine, Shadow Mastiff and Poisonous Frog; Sprint 14 Fire Beetle, Giant
Ant Worker and Giant Ant Soldier; Sprint 15 Giant Ant Drone and Giant Stag Beetle;
Sprint 16 Crocodile overhaul and Dire Crocodile; Sprint 17 Viper, Constrictor
Snake and Salamander. Earlier released summoning remains available. There are
1008 published generated placements and 29 retained native wrappers, 1037 visible
choices, no withheld placements. Direct, 1d3 and 1d4+1 routes retain distinct
parent-local identities, duration, template and spell-slot contracts.

Salamander is INCLUDED, not merely hidden with modified old routes. One
owner-authorized observation-only correction passed on exact db1da016:
smoke11/11 and affected profile/view73/73. Raw weapon reach inputs5/10,
computed engine ranges2/6 and body reach5 were independently observed. No
production profile, reach, AI, animation, geometry or balance changed to make
the observer pass. Other mechanics/view/crowd/persistence cells retain47e8c121
evidence and all five public roots retain996c5fe7 publication evidence.

All v0.0.142 Fiery Glare, Stoic Dignity, Aerial Observer and Whiteout traits,
optional qualified Favored Class integration, firearm descriptions and Model D
merchant behavior remain intact. Existing merchant inventories are not rewritten.

The released v0.0.144 weapon findability/recovery implementation and v0.0.145
Heirloom Nodachi icon assignment remain unchanged. Legacy Cheetah/Lion/Tiger
and six Mephit variants now obey the Expanded Summoning module's restart-bound
active state before allocating or mutating any view resource. Save-compatible
profiles remain registered while the module is OFF; native visuals remain native.

## Honest limits

- `PASSIVE_CREATURE_SENSES_UNMODELED`: Scent, Darkvision and Low-light Vision
  have no faithful Kingmaker carrier. No substitute blindsight/range inflation.
- `ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`: active grab/hold/swallow,
  engulf and Stirge attachment links are session-scoped and reset cleanly;
  no relationship re-establishment is promised.
- `SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`: Dire Crocodile and Purple Worm
  have no attackable interior AC/HP/cut-free path. Size, initial bite, later
  damage, native escape, exact ownership and cleanup remain required.
- `SWALLOW_ELIGIBLE_TARGET_ELSE_DEATH_ROLL`: Dire Crocodile's later maintain
  deterministically swallows smaller legal prey, otherwise uses Death Roll on
  same-size eligible prey. One check never resolves both; not tabletop choice.

HumanReview: NOT_PERFORMED_NONBLOCKING. Technical qualification is not owner
artwork, tooltip or merchant visual approval. Native donors' actual animation
sets are retained; nonexistent native animations are not invented.

## Qualification and installation

Integration ledger: [exact imported tips and conflicts](../planning/RELEASE-0.0.146-INTEGRATION-LEDGER.md).
Exact integration source/build/package, exhaustive player path, whole-roster
ON/OFF persistence, five-profile compatibility and v0.0.142/144/145 coexistence
qualification is recorded with its tested-candidate provenance in the companion
release manifest and planning/RELEASE-0.0.146-EVIDENCE.json. Package notes are
frozen before runtime qualification; no source-only PASS is gameplay qualification.

Back up saves before updates. Install only the standalone UMM ZIP. Keep the mod
installed in campaigns that have used its content; disabling a content module
retains registered save identities but is not an uninstall-safe-save guarantee.
The owner's actual pre-run installation is restored, not permanently updated.

## Workflow reset

After v0.0.146: STOP. Future work branches from latest released master, one
charter sprint per branch/PR/release, no stacked campaign and no autonomous
next sprint without a new owner mission. Focused edit-loop tests; one immutable
sprint candidate; at most one product and one observation-only correction;
global matrices once at release close. Sprint 18 was not started; Phase2C and
Sprint22 are outside this mission.
