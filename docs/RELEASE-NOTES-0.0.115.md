# Kingmaker Gunslinger 0.0.115 — Elemental Heritages

Candidate package: `KingmakerGunslinger-0.0.115-elemental-heritages.zip`

This release expands the existing Elemental Races module. Ifrit, Oread, Sylph,
and Undine each receive one obligatory three-choice heritage selection:
General plus two alternate heritages. These are not new top-level races. The
four parent race blueprints and all save-bearing 0.0.114 General provider GUIDs
remain unchanged.

Legacy 0.0.114 characters have no heritage marker. The reconciler treats that
absence as General, retains the existing racial stats and providers, and does
not restore a spent racial SLA use. Alternate heritages apply only their exact
net stat delta and reconcile one affinity and one SLA provider.

The alternate choices are Lavasoul and Sunsoul, Gemsoul and Ironsoul,
Smokesoul and Stormsoul, and Mistsoul and Rimesoul. Their player-facing spell
names match the actual granted ability. The approved Owlcat substitutions are
Firebelly for Burning Sands, Flare Burst for Sun Metal, Expeditious Retreat for
Blurred Movement, and Blur for Obscuring Mist. Unerring Weapon and Chill Touch
use narrow project-owned native-rule implementations; the deviation matrix
records the precise mechanics and the frightened-for-panicked engine
adaptation.

All heritage identities register even when `elemental-races` is disabled. The
setting controls selector publication only, preserving existing characters'
race, facts, resources, and appearance. No favored-class bonus, new module
toggle, native race-enum member, outsider-type rewrite, custom mesh, copied
asset, or third-party runtime dependency is added.

Release A qualification is in progress. The 0.0.114 runtime and compatibility
records remain historical evidence and are not claimed as 0.0.115 results.
Nothing in this candidate note asserts a release PASS, merge, tag, or public
release.

Dedicated guarded player-command testing now passes all alternate heritage
SLAs. It verifies cancellation, exact one-use commitment, zero-use blocking,
ordinary-rest recovery, donor-native effects, Unerring Weapon's exact-item
critical-confirmation bonus, and Chill Touch's living/undead branches with
persistent per-level charges. The Chill Touch integration declares explicit
ordering before Call of the Wild's broader sticky-touch prefix; no optional
assembly is linked or required. Save-backed migration, respec, visual, and
compatibility gates remain pending.

The candidate preserves the previously qualified firearm SoundBank byte
identity `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items support remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test
overhaul and 1,325-test fatigue-authority baselines remain historical, while this candidate's current
dependency-free suite contains 1,407 registered cases.
