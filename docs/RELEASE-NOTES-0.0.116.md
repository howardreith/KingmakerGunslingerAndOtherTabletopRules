# Kingmaker Gunslinger 0.0.116 — Elemental Feats

Proposed package: `KingmakerGunslinger-0.0.116-elemental-feats.zip`.

Release B remains in progress. The current checkpoint fixes and registers the
complete 25-identity save-bearing blueprint inventory, binds the eleven feat
prerequisites to exact project races and providers, and adds additive,
deterministic, idempotent, exact-GUID-aware publication to the universal and
Fighter combat-feat selectors under the existing `elemental-races` setting.

Elemental Strike is now mechanically active: its Swift command applies a
one-round buff and qualifying weapon attacks add one exact flat packet keyed to
the parent race (fire, acid, electricity, or cold), scaling from +1 to +5 at
the published total-character-level breakpoints. It does not affect spells or
unrelated damage and does not double-apply when the same damage event is
revisited.

Wings of Air now uses Kingmaker's exact base draconic-flight abstraction while
the Sylph wears no armor or light armor: +3 Dodge AC against melee attacks,
difficult-terrain condition immunity, and immunity to Ground-descriptor buffs.
It grants no ranged AC, prone/trip immunity, blanket ground-spell immunity, or
unrestricted three-dimensional navigation. Medium or heavy armor suppresses
the benefit, and legal armor removal restores it immediately without rest or
respec. No optional-mod flight component, custom mesh, or persistent VFX is
copied.

The required catalog is Elemental Strike; Scorching Weapons, Inner Flame,
Blazing Aura, and Firesight for Ifrits; Airy Step, Wings of Air, Cloud Gazer,
and Inner Breath for Sylphs; and Hydraulic Maneuver and Triton Portal for
Undines. Oreads qualify for Elemental Strike. No favored-class content is
included.

Dedicated guarded run
`20260904T2000378983332Z-b0699acd82da4d378c3abdded3983858` passed all
16 Elemental Strike/Wings assertions with no warnings and exact disposable-unit
cleanup. This checkpoint is independently buildable but is not a Release B
PASS. The other nine feat mechanics, save-backed persistence, compatibility
profiles, and final release hashes remain pending. Release A 0.0.115 evidence
remains historical and is not relabelled as 0.0.116 evidence.

The candidate preserves the previously qualified firearm SoundBank byte
identity `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items support remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test
overhaul and 1,325-test fatigue-authority baselines remain historical, while
this candidate's current dependency-free suite contains 1,408 registered
cases.
