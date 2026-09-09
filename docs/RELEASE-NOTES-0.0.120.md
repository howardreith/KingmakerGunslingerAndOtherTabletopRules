# Kingmaker Gunslinger 0.0.120

Informational version: **0.0.120-elemental-races-completion**.
The owner authorized commit, merge, push and public release, and waived the final
repetitive testing matrix. This is a **partial Elemental Races completion release**.

Nereid Fascination is now available for all three Undine heritages in the existing
SLA replacement selector: retain-base, Acid Breath, Nereid Fascination, Ooze Breath.
The alternatives are mutually exclusive and replace the active heritage's SLA.
The available inventory is four parent races, twelve heritages, eleven racial
feats and twenty alternate racial traits. No new race, module, schema or
favored-class content is added.

Nereid is a standard supernatural activation, once per ordinary rest. Other
humanoids within 20 feet, including allies, attempt Will against
10 + floor(total level / 2) + current Charisma modifier. Duration is
max(1, floor(total level / 2)) rounds. The caster is excluded, and native line of
effect applies. It uses no spell slots, spell resistance, arcane spell failure or
elemental affinity bonuses. Re-entry keeps the original expiration; a successful
save or broken fascination cannot be reapplied by that activation's area tick.
Perceived obvious threats interrupt it; hostile approach permits a save. Shake
Free uses a standard action at touch range. **Sound alone does not trigger threat
interruption**; this native sensory limitation is disclosed in the ability text.

Treacherous Earth remains hidden. Its native difficult-terrain effect and ground
checker are implemented, and the focused installed-stack run passed 68 assertions
covering native action/resource commitment, movement costs, ground/floor controls,
overlap, immunities and cleanup. Native Oread Player creation/respec and complete
save integration are still unqualified. Automatic approval review rejected
ordinary publication without that evidence. Hidden registration is containment,
not completion of the requested selectable feature.

The implemented Treacherous ground policy follows the owner's delegated rules
judgment: any valid walkable ground within touch reach, including wood and worked
stone, with actual ground contact, line of effect and an unblocked local path.
This intentionally broadens the tabletop earth/unworked-stone/sand restriction.
No sound-surface grid, texture-name inference or permanent navigation mutation is used.

Current development-artifact evidence includes 1,557 domain/reflection tests,
464 runtime preflight checks, Nereid core (152 assertions), ordinary selector/
replacement integration (6,586 assertions), and the focused Treacherous run (68).
These belong to their exact recorded development DLLs. The earlier native Player,
resource, fresh-process persistence, module OFF/ON, v0.0.117/deferred-marker and
pinned v0.0.114 checks remain historical evidence. Remaining final-artifact
qualification was waived; it is not reported as PASS. Exact references and
artifact identities are in the [completion ledger](../ELEMENTAL-RACES-COMPLETION.md).

The accepted Heritage phase, separate populated ZFavoredClass background Trait
roots, Helpful transaction, visual retention and native respec expenditure paths
are retained. Old ignored Nereid markers retire conservatively while preserving
the existing heritage SLA and spent resources; old Treacherous markers remain
inert. Save-bearing identities remain registered with Elemental Races OFF.
The manifest contains 1,883 identities: 1,881 active and two reserved.

Released 0.0.119 World-Map Teleportation behavior and its stated qualification
limits are retained. Public 0.0.117, 0.0.118 and 0.0.119 tags/assets are untouched.
All artwork, firearm audio and other shipped assets are preserved. The unchanged
firearm SoundBank SHA-256 is
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
No proprietary references or third-party runtime dependencies are packaged;
Craft Magic Items remains reflection-only; CraftMagicItems.dll is not linked or packaged. The inherited 1,288-test overhaul and
1,325-test fatigue-authority checkpoints remain historical evidence.

Owner checks in a disposable character/save:

- Select Nereid on the desired Undine heritage; check retain-base/back-navigation,
  ordinary background Traits and final-review completion.
- Check the aura's ally exposure, one-use display, threat interruption and stated
  hearing limitation. Rest should restore one use; a canceled activation spends none.
- Treacherous is unavailable in ordinary selection. Its remaining task is native
  Player/save integration qualification, followed by a separately approved publication.

Installable archive: `KingmakerGunslinger-0.0.120-elemental-races-completion.zip`.
