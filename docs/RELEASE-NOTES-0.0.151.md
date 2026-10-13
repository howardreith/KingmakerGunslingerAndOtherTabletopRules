# v0.0.151 — Ankylosaurus Completion

`0.0.151-expanded-summoning-sprint22`

Kingmaker Gunslinger 0.0.151 installable asset:
`KingmakerGunslinger-0.0.151-expanded-summoning-sprint22.zip`.
The firearm audio bank remains byte-identical, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Existing integration with `CraftMagicItems.dll` is retained without redistributing
the third-party assembly or claiming a fresh whole-series compatibility run.
Historical content and fatigue-authority records retain their 1,288 and 1,325
test counts; neither describes this candidate's current suite.

The existing Ankylosaurus gains an original low, broad armored body with a
small head, plated back and terminal tail club, replacing its Hodag appearance.
Its unit and all ten summon roots keep their published GUIDs, names and levels.
Summon Monster V and Summon Nature's Ally V remain single; VI remains 1d3;
VII–IX remain 1d4+1. Monster template children and Nature's Ally alignment
behavior remain intact. The catalog retains 1,063 placements and 29 wrappers.

Stun (Ex) makes one Fortitude save per resolved tail hit. A failed ordinary
save dazes for one round; a failed confirmed critical save stuns for 1d4 rounds
instead. DC is 10 + half racial Hit Dice + current Strength modifier (23 at
10 HD and Strength 27). Fortitude is the owner's explicit interpretation of
the primary paragraph's unspecified save type. A valid hit whose damage is
absorbed still qualifies. Separate native daze and stun immunity contracts
apply. Finite conditions belong to the victim and retain their native expiry.

The single tail retains 1.5× positive Strength damage, with Huge 3d6 dice.
Improved Bull Rush, Weapon Focus (tail), four-leg trip defense and exact
Perception ranks use native carriers. Power Attack remains a separate native
toggle. Difficulty modifiers remain part of gameplay and are named in native
comparisons.

Accepted limitations remain PASSIVE_CREATURE_SENSES_UNMODELED for low-light
vision and scent, and ORDINARY_MAP_LAND_USE_SCOPE. Improved Overrun remains
omitted: no concrete standalone native feat or qualified summon command
consumer was found; the native maneuver enum alone does not supply an action.
No general overrun system or substitute senses are added. The donor has no
native Hit animation. Owner aesthetic approval is
NOT_PERFORMED_NONBLOCKING.

Install the standalone UMM ZIP using the established installation procedure.
Optional compatibility mods remain optional. This patch does not change
Triceratops, Stegosaurus, animal companions or mounted combat. Sprint 23 has
not started.

The qualified artifact was built from
`d42e33ccc5b282daad594836b1defd4f680990e5`. ZIP SHA-256 is
`5d4efe07770fba24fecb8d7dda6933e483ccef31f1ef0d0e35849b6d5c27bf67`;
DLL SHA-256 is
`97fe266abbe23d4527eac4804efb3c3e48a692528e859adcf8cde137fe87bc0f`;
DLL MVID is `abfbf93f-60e4-44ac-9ac8-e21568962b05`. The complete suite passed
2,582/2,582, followed by fresh exact-package turn-based, RTWP standalone,
high-risk combined, and enabled/disabled persistence qualification. The human
installation, original registry settings and unrelated saves were restored
exactly after testing.

## Release verification

[v0.0.151](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.151)
was published as stable Latest at 2026-10-13 06:19:26Z. PR #39 merged at
`30f41dfb76a8264fdfd3238c9cba05a4c131f3bf`; the annotated tag peels to
that commit. The published ZIP was downloaded again, retained SHA-256
`5d4efe07770fba24fecb8d7dda6933e483ccef31f1ef0d0e35849b6d5c27bf67`,
and passed strict standalone UMM package validation. The published manifest
records tested source `d42e33cc...`, qualification reporting commit
`67080d02...`, exact-artifact promotion and no publication rebuild.
The post-publication audit again matched the original human installation,
registry and unrelated saves exactly.

Owner aesthetic approval remains `NOT_PERFORMED_NONBLOCKING`. Sprint 22 is
RELEASED/STOP; Sprint 23 has not started.
