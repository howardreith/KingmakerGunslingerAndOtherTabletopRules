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

Scorching Weapons is now mechanically active. Its Swift command snapshots up
to two distinct currently held manufactured weapons carrying Kingmaker's
native Metal subcategory and applies a one-round enchantment owned by those
exact items. Unequipping or swapping does not transfer the benefit. Qualifying
attacks deal exactly 1 additional fire damage once, ordinary fire resistance
applies, and an existing fire-damage weapon effect suppresses the project
packet. The feat grants one +2 racial save modifier against fire attacks and
native Light-descriptor spells.

Inner Flame now replaces those values with 1d6 fire damage and +4 total rather
than stacking base and improved benefits. Kingmaker has no ordinary
player-facing grapple state, so the printed grapple clause remains an honest
no-op. Kingmaker 2.1.7b also has no `SpellDescriptor.Light`; the implementation
uses an immutable isolated-runtime-audited catalog of seven exact native Spell
identities and excludes racial spell-like abilities from that Light-spell
branch.

Blazing Aura is a Free action on the Ifrit's turn while Scorching Weapons is
active. Its one-round aura deals one native 1d6 fire packet when any adjacent
other creature, including an ally, begins its turn. Firesight uses native
Dazzled immunity and sees through only explicitly classified fire or smoke
concealment; it does not suppress fog, Blur, displacement, invisibility,
blindness, darkness, or Mirror Image.

Airy Step adds one +2 racial saving-throw modifier against native Electricity
effects, direct electricity-damage reasons, and an immutable isolated-runtime
catalog of eleven exact native Air effects. Overlapping predicates and parent
variants apply once. Wings of Air replaces this with +4 total. Cloud Gazer
ignores concealment only from exact native Obscuring Mist and explicit
project-owned fog/mist/cloud effects; all unrelated concealment remains
effective. Inner Breath blocks the exact native respiration-required
poisonous-swamp-gas pair and explicit project-owned respiration effects. It
does not grant blanket poison, gas, or cloud immunity.

Hydraulic Maneuver extends an active racial Hydraulic Push with four
per-use choices: Bull Rush, Disarm, Trip, and Kingmaker's genuine native Dirty
Trick (blind). Each path uses total character level plus the current best of
Intelligence, Wisdom, and Charisma through native `RuleCombatManeuver`, and
spends the same racial use only after the command is accepted. Kingmaker has
no native player-facing Dirty Trick (dazzle), so that printed option is
explicitly omitted rather than simulated.

Triton Portal is a full-round SpellLike point-target command that shares the
racial Hydraulic Push use. It invokes the exact native summon graph for 1d3
Small Water Elementals, with ordinary summon duration, allied non-hostile
faction, source linkage, death, and cleanup behavior. It is independent of
the `expanded-summoning` selector module. Dolphins, sharks, and electric eels
remain omitted under the approved native-water-elemental adaptation.

The required catalog is Elemental Strike; Scorching Weapons, Inner Flame,
Blazing Aura, and Firesight for Ifrits; Airy Step, Wings of Air, Cloud Gazer,
and Inner Breath for Sylphs; and Hydraulic Maneuver and Triton Portal for
Undines. Oreads qualify for Elemental Strike. No favored-class content is
included.

Dedicated guarded run
`20260904T2000378983332Z-b0699acd82da4d378c3abdded3983858` passed all
16 Elemental Strike/Wings assertions. Isolated guarded run
`20260904T2222242573484Z-6e4985f6214a4ffeba5512e353f884f3` passed all
12 Scorching Weapons/Inner Flame assertions. Run
`20260905T0137340360592Z-e5da1d69116a4fd1837b7ae385ed7bd9`
passed all 14 Blazing Aura/Firesight assertions, and isolated KMG-only run
`20260905T0258431754839Z-f395da4f5be54cbdad4e980f477f2791`
passed all 18 Airy Step/Cloud Gazer/Inner Breath assertions. KMG-only run
`20260905T0550526363250Z-a4c7158ae8e74168b36082c6c6e6e3a0`
passed all 13 Hydraulic Maneuver/Triton Portal assertions. Every final run had
zero runtime-result warnings, touched no save, and cleaned up its disposable
state exactly. This checkpoint is independently buildable but is not a
Release B PASS. Save-backed persistence, compatibility profiles, and final
release hashes remain pending. Release A 0.0.115 evidence remains historical
and is not relabelled as 0.0.116 evidence.

The candidate preserves the previously qualified firearm SoundBank byte
identity `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items support remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test
overhaul and 1,325-test fatigue-authority baselines remain historical, while
this candidate's current dependency-free suite contains 1,408 registered
cases.
