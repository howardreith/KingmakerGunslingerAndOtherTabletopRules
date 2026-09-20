# Magic Circle guarded qualification

This feature uses the existing `-kmgRuntimeTestRequest` launcher through Steam
640820. Only the exact `KMG_AUTOMATION_WORKING` descriptor may be loaded or
written. Never modify `KMG_AUTOMATION_BASELINE`. Current version comes from
`Info.json`; package, deployment, loaded DLL hash/MVID and profile must correlate.

`disposable-magic-circle-evil` exercises real cast commands and native areas,
typed combat/save rules, control applications, movement, overlaps, dispelling,
Extend, expiration, unconsciousness, original caster death and bearer death.
Its actors and cloned blueprints are request-local and never saved.

`disposable-magic-circle-ui` uses the same exact working-save load and mandatory
exit. It stages a native Sorcerer 5, selects one Circle in the real level-up
presenter, cancels once, then commits the ordinary level-6 choice. It verifies
that only the selected variant is learned and casts that learned spell. A
request-owned native Wizard book then exposes all four separate prepared spells,
held touch, timed carrier/proximity buff rows and scroll inventory rows. Native
framebuffer captures support visual inspection; structured native state proves
learning, preparation, resource use, identity and cleanup. It restores the
original party, selection, UI settings, inventory, money, entities and clock.
There are no save writes, campaign level-ups, synthetic UI rows or new settings.
The original owner-approved export pixels are unchanged. Native UI acceptance
remains pending until this scenario and visual inspection have passed.

The following narrowly scoped persistence scenarios use four named non-party
actors (`KMG_RUNTIME_MAGIC_CIRCLE_SAVED_CasterA`, `CasterB`, `Bearer`, `Recipient`)
of the registered native DefaultPlayerCharacter blueprint. No shared blueprint
is modified. The two Sorcerers cast on the bearer through native commands;
their spellbooks, spent slots, original contexts and deadlines are saved.

| Scenario | Authorized operation |
| --- | --- |
| `working-save-magic-circle-prepare` | Require no fixture; native casts; arm and perform exactly one save on the captured working descriptor. |
| `working-save-magic-circle-verify` | Fresh load; inspect original contexts/known spells/ownership and actual defense/control rules; no writes. |
| `working-save-magic-circle-scene` | Fresh load; temporarily stage the four exact fixture actors as traveling party references using the existing Elemental fixture convention; one native `Game.ReloadArea()` (no autosave); observe real scene replacement and original deadlines; remove only those temporary references; no writes. |
| `working-save-magic-circle-cleanup` | Fresh load; verify then remove only the four exact named actors and their areas/contributions; exactly one guarded working save. |
| `working-save-magic-circle-absent` | Fresh load; verify fixture and effects are absent; no writes. |

Prepare/cleanup require all pre-write checks to pass before arming the existing
exact-descriptor sentinel. Ambiguity fails closed. A failed phase never silently
prepares over an existing fixture. Resume with verify or cleanup after diagnosis.
Snapshots in `magic-circle-persistence.json` must be compared across fresh
launches for exact actor IDs, carrier deadlines, caster levels, metamagic, known
spells, spent slots and pre-existing control. Area instances may reconstruct;
each live carrier must have exactly one area and each recipient exactly one
contribution per area. Merely producing snapshots is not a persistence PASS.

Run verification in all four content/enhancement startup combinations, changing
only `magic-circle-spells` and `protection-from-alignment-control-immunity` in
the existing settings document and restoring its original bytes afterward.
Never change settings while the game is running. Content OFF retains stable
hydration identities and original active lifetimes but disables new casts and
publication. Only the existing enhancement controls added control protection.
All other settings must survive these operations exactly.

Owner-approved adaptation: bearer death ends the circle through native ordinary
buff cleanup. Unconsciousness and original caster death do not end a circle on a
living bearer. Recipient contributions are derivative and not independently
dispellable; targeted carrier or successful native area dispel ends that cast.
Scene unloading is temporary and does not authorize deleting the timed carrier.

These scenarios do not prove uninstall safety, learning/scroll acquisition,
all optional profiles, art placement, or any untested row in the feature matrix.

Scene-fixture diagnostic: the starting-area native reload discarded all four
non-party fixture actors, so that attempt failed rather than accepting missing
actors as circle cleanup. Fresh-process hydration had passed separately. The
scene scenario now stages exact traveling references only in memory, records
the native area's exclusion flag and holding-state types, and checks restoration
of the original party and remote-companion identities. It does not recreate a
carrier, area, spellbook or expected effect after the native reload.
