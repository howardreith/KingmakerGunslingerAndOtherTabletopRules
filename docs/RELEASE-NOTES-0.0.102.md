# Kingmaker Gunslinger 0.0.102

This release fixes four narrowly scoped Gunslinger integration defects while
preserving the established firearm, archetype, acquisition, and optional-mod
contracts. Its standalone archive is
`KingmakerGunslinger-0.0.102-starter-bokken-combat-log-acadamae-toggle.zip`.

Characters that commit their exact first Gunslinger class level now receive the
same production starter package as first-level character creation. Base
Gunslinger, Pistolero, Mysterious Stranger, and other default-policy
archetypes receive one Pistol; Musket Master receives one Musket. The firearm
starts in ordinary condition, is bound to its exact owner as a battered
starter, and is accompanied by 20 Black Powder, 20 Lead Ball, and one
Gunsmith's Kit. A durable per-unit receipt and exact inventory transaction make
level-up, respec, repeated callbacks, save/load, and unrelated shared-inventory
firearms idempotent.

Bokken now carries the complete early firearm-supply catalog: 100 Black Powder,
100 Lead Ball, 100 Paper Cartridge, five Repair Kits, two Overhaul Kits, and one
exact production Gunsmith's Kit. Exact KMG-owned firearm-supply rows are
removed from Oleg without disturbing native inventory or component order.
Both native table shapes retain transactional publication and rollback.

Routine KMG mechanical feedback no longer uses Kingmaker's warning overlay.
Firearm armor-class selection, firearm condition changes, Acadamae Graduate,
Bodyguard, Cord of Stubborn Resolve, Shield Other, and named Elven Branched
Spear outcomes use concise Combat-channel entries through the native
`BattleLogView`. Detailed identities and traces remain in the structured mod
log, and a feedback failure never changes an already committed mechanic.

Acadamae Graduate now treats the exact native activatable ability's current
`IsOn` state as authoritative whenever presentation and a new command are
evaluated. A lingering hidden marker cannot accelerate a command created after
the mode is turned off. Commands genuinely armed while the mode was on retain
their existing snapshot and resolve exactly one Fortitude save after a
successful cast.

The dependency-free regression suite contains 1,251 passing tests. Guarded
Steam runtime qualification uses only request-scoped scenarios and does not
write a save for the disposable fixtures. Thirteen final-candidate scenarios
passed. They cover starter creation/preview/commit/respec boundaries, the exact
Bokken and Oleg tables, firearm AC and condition feedback, and the Acadamae
OFF/ON/OFF player-command lifecycle. The save-free main-menu fixtures prove
the exact native combat-log call, one attempt per event, concise content, and
failure isolation; visible in-game combat-log placement remains a manual UI
check. Fresh-process Acadamae ON/OFF persistence likewise remains a manual
save/load check because this qualification intentionally performed no save
mutation.

The accepted Craft Magic Items 2.1.0 integration and its optional reflection
boundary remain unchanged. `CraftMagicItems.dll`, game assemblies, saves, raw
runtime logs, and generated packages are not included in the source commit.
The qualified firearm SoundBank remains byte-identical at SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
