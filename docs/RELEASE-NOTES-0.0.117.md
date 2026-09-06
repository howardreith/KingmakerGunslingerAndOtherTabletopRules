# Kingmaker Gunslinger 0.0.117

Informational version: `0.0.117-elemental-traits`

Proposed local package:
`KingmakerGunslinger-0.0.117-elemental-traits.zip`

## Status

Release C remains in progress. This checkpoint includes the deterministic
replacement framework and the first eight passive mechanics. It is not a
Release C PASS and must not be published.

## Implemented in this checkpoint

- 62 fixed manifest identities: ten slot selections, ten retain-base markers,
  21 visible alternate-trait markers, and 21 separate hidden providers.
- Three semantic replacement slots: Energy Resistance, Elemental Affinity,
  and Racial Spell-Like Ability.
- Exact legal-combination and overlap rejection policy for all four races and
  all twelve heritages, including marker activation/deactivation order and
  reconstruction.
- Race-owned obligatory selections with no additional top-level races, module
  toggle, settings-schema revision, or dynamically generated GUID.
- Project-owned reconciliation that preserves remembered heritage-SLA amounts
  and never removes native or foreign facts.
- Wildfire Heart, Brazen Flame, Forge-Hardened, Granite Skin, Like the Wind,
  Secretive, Thunderous Resilience, and Whispering Wind, using native
  stat/resistance components and narrow saving-throw/melee rules.
- Scorching Weapons' nonstacking check now also handles later-acquired
  Brazen Flame. Only its own exact damage packet is removed.
- Acadamae's self-fatigue save carries the existing native fatigue context;
  actual command regressions preserve its DC, action, resource, and cleanup.

## Still required

The incremental framework passes its 1,413-test suite, clean build, strict
package, 17,483 focused runtime assertions, KMG-only/combined ON/OFF profiles,
and four-process retain-base persistence (43 assertions). See the
[exact framework ledger](ELEMENTAL-RACES-0.0.117-FRAMEWORK-QUALIFICATION.md).

The first eight mechanics pass 1,415 tests, clean build/package, and eight
guarded processes (13,397 assertions, zero warnings, three exact profile
restorations). See the
[passive-mechanics ledger](ELEMENTAL-RACES-0.0.117-PASSIVE-MECHANICS-CHECKPOINT.md).
Thirteen mechanics, trait-bearing module-OFF/ON persistence, final
compatibility/module matrices, full release qualification, and final
documentation remain pending.
Release A and Release B evidence is
retained as historical checkpoint evidence and is not relabelled as Release C
proof.

No merge, tag, public release, or committed package is authorized by this
checkpoint.

## Inherited package guarantees

The candidate preserves the qualified firearm SoundBank byte identity
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items support remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test
overhaul and 1,325-test fatigue-authority baselines remain historical, while
this candidate's current dependency-free suite contains 1,415 registered
cases.
