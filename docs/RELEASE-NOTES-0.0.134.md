# Release Notes 0.0.134 — Magic Circle

Version: `0.0.134-magic-circle-alignment-spells`
Build label: Kingmaker Gunslinger 0.0.134.

The qualified firearm SoundBank is unchanged, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

The owner authorized finalization, merge to master, push and a new full public
release after accepting PR #19 and its review hardening. This release preserves
the Word of Recall Favored Class repair from 0.0.133.

## Spells and settings

Magic Circle against Evil, Good, Law and Chaos are separate level-3 abjurations.
A standard-action touch makes the chosen creature the bearer of a moving
ten-foot emanation for ten minutes per original caster level. Everyone covered,
including the bearer and enemies, receives native +2 deflection AC and +2
resistance saves against the matching alignment. Normal typed stacking applies.

The `magic-circle-spells` content module uses the existing startup/restart
settings model. The existing `protection-from-alignment-control-immunity` setting
is the single authority for added protection against **new qualifying control**.
Circles use the same catalog and originating-controller alignment as individual
and communal Protection. Existing control stays active. Disabling the enhancement
retains ordinary circle defenses and adjusts descriptions.

Cleric, Sorcerer/Wizard and Inquisitor receive all four spells. Paladin receives
against Evil and Chaos. Verified optional lists and the exact third-level Oracle
Favored Class choice preserve native restrictions and one spell per selection.
Each associated scroll is level 3, CL5, 375 gp, with one charge, native scribing
and finite stock at established suppliers. Each cast consumes one slot or charge.

The original caster owns duration, metamagic and dispel attribution; the bearer
owns position. Each area owns its recipient contributions independently.
Overlaps, standalone Protection and stronger equipment bonuses coexist. Bearer
death ends its circle, as approved by the owner; unconsciousness and caster death
do not end a living bearer's circle. Recipient buffs last only while covered and
are not independently dispellable. Save hydration preserves original deadlines
even when content publication is disabled; new casts remain disabled.

## Review hardening and integration

Persistence requests carry an immutable preparation binding. The native session
checks exact fixture, save and artifact identities before cleanup or save-write
authorization. Settings overrides retain shared ownership through process exit
and restoration, preserve exact backup bytes, and reject foreign changes.
Standalone leave-open runtime success hands off a retained lease without turning
native PASS into an error. Guarded completion/recovery requires the exact owner,
recorded identities and exited processes; inherited leases stay parent-owned.

Integration retains both Magic Circle and Word of Recall request guards and
their distinct favored-class features. No approved art, native spell identity,
unrelated settings or established Protection targeting/levels/durations changed.
Craft Magic Items compatibility is unchanged; there is no static
`CraftMagicItems.dll` dependency. Historical domain checkpoints of 1,251, 1,288
and 1,325 cases remain archived under their original releases.

## Qualification and boundaries

The complete 1,678-case domain suite, repository validation, clean Release,
strict package gates and focused orchestration regressions are required for this
release. Guarded native qualification uses Steam App 640820 and the disposable
`KMG_AUTOMATION_WORKING`, with exact DLL/package identities recorded separately
in the release evidence. Historical PR evidence remains attributed to its tested
binary; the final release receives its own focused checks and deterministic
publication verification. See the release manifest for the published commit and
package/DLL SHA-256 values, and `reports/magic-circle/` for curated evidence.

The four original paintings retain owner approval. All twenty visible consumers
were previously inspected in native desktop UI with both control configurations;
artwork and assignments are unchanged. Gamepad UI is unqualified. Reduced mod
profiles cannot load the existing Working fixture because of a previously
recorded native post-load failure; no reduced-profile save repair was attempted.
The full compatibility matrix, uninstall and downgrade safety remain unclaimed.

Deferred mechanics remain absent: new saves or morale bonuses against existing
control; removing, suppressing or resuming control; summoned-creature contact,
exclusion, spell-resistance and breach barriers; inward circles, planar binding
and trapping diagrams; blanket descriptor immunity; silver-dust bookkeeping.

## Installation

Close Kingmaker, back up the current mod folder and preserve FeatureModules.json,
then install `KingmakerGunslinger-0.0.134-magic-circle-alignment-spells.zip` through
Unity Mod Manager. Verify SHA256SUMS.txt and start with disposable saves. Rollback
restores the prior folder/settings backup while the game is closed and uses a
save from before the new content; removing registered blueprints from an existing
save has not been qualified. Qualification deployment does not replace the
owner's normal-play installation.
