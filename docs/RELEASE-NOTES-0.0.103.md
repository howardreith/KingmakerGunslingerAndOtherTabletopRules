# Kingmaker Gunslinger 0.0.103

This release corrects three bounded defects without changing firearm balance,
summon rosters, spell quantities, summon stat blocks, or the accepted Acadamae
Graduate and Cord of Stubborn Resolve rules. Its standalone archive is
`KingmakerGunslinger-0.0.103-overhaul-summon-menu-fatigue-escalation.zip`.

Overhaul Firearm no longer waits for a hidden 60-second `GameTime` interval.
Its ordinary full-round command promptly revalidates and commits the same
equipped empty Wrecked firearm to Broken, consumes exactly one Firearm Repair
Kit, preserves ammunition, and leaves world time unchanged. Combat, changed
equipment, missing resources, and ambiguous firearms still fail closed.

Expanded Summoning's exact native PC action-bar variant menu now measures the
rendered runtime list and active canvas-safe rectangle. Lists that fit keep
their native presentation. Larger lists choose the better side, clamp to the
canvas, and use one bounded scrolling viewport without deleting, reordering,
or cloning native options. Geometry is resolution-, aspect-, safe-area-, and
UI-scale-aware, and third-party additions are measured from the runtime list.

Exact canonical Fatigued applications now follow the deterministic native
condition table: fresh targets become Fatigued, already-Fatigued targets become
one Exhausted fact, and Exhausted targets are never duplicated or downgraded.
The coordinator acts only after Kingmaker accepts the incoming buff, preserves
duration/context and immunity behavior, and is exact-GUID scoped. Acadamae
Graduate uses this shared path. Cord substitution still resolves once per
accepted incoming effect with its established damage and 1-HP floor.

The complete dependency-free regression suite contains 1,278 passing tests.
The source commit does not include game assemblies, Unity assemblies,
`CraftMagicItems.dll` or other optional-mod binaries, saves, raw runtime logs,
credentials, or generated packages. The
existing firearm SoundBank and all project blueprint identities are retained.
The unchanged SoundBank SHA-256 is
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
The exact automated and supervised evidence boundary is recorded in the
0.0.103 qualification report.
