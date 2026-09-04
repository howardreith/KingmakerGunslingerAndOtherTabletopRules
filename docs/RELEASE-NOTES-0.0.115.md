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

Release A qualification PASS is established locally. The complete 1,407-case
domain/reflection suite, clean Release build, strict package validation,
dedicated blueprint/mechanics/SLA runs, 24-fixture persistence transaction,
exact 0.0.114 migration sequence, native death/resurrection and
polymorph/return transitions, and all six installed compatibility profiles
passed. Historical 0.0.114 results remain identified separately and are not
relabelled as 0.0.115 evidence.

Dedicated guarded player-command testing now passes all alternate heritage
SLAs. It verifies cancellation, exact one-use commitment, zero-use blocking,
ordinary-rest recovery, donor-native effects, Unerring Weapon's exact-item
critical-confirmation bonus, and Chill Touch's living/undead branches with
persistent per-level charges. The Chill Touch integration declares explicit
ordering before Call of the Wild's broader sticky-touch prefix; no optional
assembly is linked or required. Save-backed native respec, module-OFF/ON,
ordinary rest, level-up, exact legacy migration, visual state transitions, and
fresh-process cleanup all passed.

Compatibility passed KMG alone, KMG + Call of the Wild, KMG + Races
Unleashed, KMG + Favored Class (without any new favored-class behavior), the
minimum valid Tweak or Treat stack, and the highest-risk installed combined
stack. Every profile also passed with Elemental Races OFF: all identities
remained registered and the four races were absent from the top-level selector.
All twelve transactions restored the original 968-entry mod tree and relevant
settings exactly. Visual Adjustments was absent and is therefore NOT-RUN.

This is a qualified local milestone, not a publication claim. The required
push wrapper currently refuses the user-mandated feature branch because that
branch is outside its external allowlist. Nothing was merged, tagged, or
publicly released, and the candidate ZIP is not tracked.

The candidate preserves the previously qualified firearm SoundBank byte
identity `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items support remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test
overhaul and 1,325-test fatigue-authority baselines remain historical, while this candidate's current
dependency-free suite contains 1,407 registered cases.
