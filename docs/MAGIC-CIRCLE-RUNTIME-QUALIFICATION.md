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
The original owner-approved export pixels are unchanged. The first native desktop checkpoint passed 56 assertions and visual inspection
(see `reports/magic-circle/NATIVE-UI-AND-LEARNING.json`); later candidates still
require impacted checks, including startup-description accuracy.

The following narrowly scoped persistence scenarios use four named non-party
actors (`KMG_RUNTIME_MAGIC_CIRCLE_SAVED_CasterA`, `CasterB`, `Bearer`, `Recipient`)
of the registered native DefaultPlayerCharacter blueprint. No shared blueprint
is modified. The two Sorcerers cast on the bearer through native commands;
their spellbooks, spent slots, original contexts and deadlines are saved. The
current fixture casts each alignment twice (eight carriers), retains a real
held-touch charge, and buys four scroll stacks from one canonical supplier that
must be absent from both native stock caches and both grant ledgers. The merchant
is one of the same four named actors. Native purchases leave alternating partial
and bought-out stocks (3, 0, 3, 0); real merchant revisits test no-refill behavior
across the startup combinations.

A test-only receipt on that exact fixture actor records original inventory,
gold and grant state plus the purchased stacks and counts. Native items expose
no persistent UniqueId, so initial absence of all four scroll blueprints and an
exact complete inventory/slot/count/charge snapshot bound ownership. No campaign
items are removed, no existing supplier stock is replaced, and no item tokens or
cross-file duplicate ItemEntity references are introduced. New default-actor
starter items are removed by exact instance before saving. Cleanup refuses a
changed inventory/gold/ledger boundary, removes only its verified purchased
stacks and previously absent supplier, restores original gold and ledgers, then
performs the single authorized working save. A fresh absent phase compares that
original state again. Never discard a failed fixture by broad blueprint removal.

| Scenario | Authorized operation |
| --- | --- |
| `working-save-magic-circle-prepare` | Require no fixture; native casts; arm and perform exactly one save on the captured working descriptor. |
| `working-save-magic-circle-verify` | Fresh load; inspect original contexts/known spells/ownership and actual defense/control rules; no writes. |
| `working-save-magic-circle-scene` | Fresh load; temporarily stage the four exact fixture actors as traveling party references using the existing Elemental fixture convention; native Game.ReloadArea(), then the existing native world-map entry and a return to the captured original area through installed LoadArea (all AutoSaveMode.None, no save descriptor); observe real scene replacement and original deadlines on every leg; restore only fixture positions after native arrival formation; remove only those temporary references; no writes. |
| `working-save-magic-circle-cleanup` | Fresh load; verify then remove only the four exact named actors and their areas/contributions; exactly one guarded working save. |
| `working-save-magic-circle-absent` | Fresh load; verify fixture and effects are absent; no writes. |

Prepare/cleanup require all pre-write checks to pass before arming the existing
exact-descriptor sentinel. Ambiguity fails closed. A failed phase never silently
prepares over an existing fixture. Resume with verify or cleanup after diagnosis.
Snapshots in `magic-circle-persistence.json` must be compared across fresh
launches for exact actor IDs, carrier deadlines, caster levels, metamagic, known
spells, spent slots and pre-existing control, held charge, purchased inventory, gold, finite stock and grant state. Area instances may reconstruct;
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

The disposable mechanics scenario also creates one exact native leopard and
uses native reciprocal pet ownership, then stages an isolated pair of actual
walkable points separated by native scene obstruction. It observes native area
membership before/after crossing that obstruction. No production distance scan
is added. Native Paladin progression supplies class entitlement and alignment
restrictions; real preparation/casting must preserve those gates.


The saved-market receipt uses the existing Teleportation shared:<table-id>
grant identity, and validates both complete ledgers before inventory or stock
mutation. A failed cleanup never saves partial changes. Native game version is
recorded directly from installed GameVersion.Cached in feature evidence.

The mechanics scenario reads the final installed optional primary books,
inherited Wizard/Witch books and Abjuration implement lists. It verifies
legitimate level-three entries, original class/book ownership, duplicate counts,
Antipaladin restrictions and the Unlettered negative case. It never invokes the
optional publisher to create its expected result; absent optional identities
remain absent. Named present/absent compatibility profiles restore original
files/settings through the existing guarded profile wrapper.


The reduced-stack working-save load failed in native Player.PostLoad at the
main-character UniqueId lookup, before any Circle scenario code. This same
pre-existing limitation is documented in ELEMENTAL-RACES-CHARACTER-CREATION-
STABILIZATION-HANDOFF.md. Do not repair or rewrite that save for compatibility.

The separate registered disposable-magic-circle-profile request reuses
ElementalNativeProfileCreatorFixture: an initially empty native Player/world,
one request-owned native character and scene services, detached native menu
subscription, and exact outer restoration. It requires automatic exit, rejects
all save names/parameters and catalog/load timeouts, and guards the five native
save mutation boundaries plus LoadRoutine before registration. It reuses the
same CircleFamily native cast commands, real failed Will-save/control checks,
ordinary typed rules and slot costs, then inspects final installed class/list
publication. It creates no expected buff, mocked rule result or substitute
control catalog. This bounded profile result makes no terrain, native UI,
scene travel or disk-hydration claim; those remain in the real working-save
scenarios under the original eleven-mod installed stack. The named
`gunslinger-qualified-combined` profile has only three mods and is a different
profile; it also failed the Working-save prerequisite before Circle execution.

The first save-free attempt stopped after successful real control/touch casts:
native Game.DoStartMode rejects Pause without an active gameplay mode. It restored the exact host
with zero save/load attempts. The fixture now uses the existing summon-test
convention of one synchronous native GameMode token and its native mode-count
registration (ElementalNativeTurnScope.SetFirearmPaused), verifies paused state,
and restores both exact original structures before host disposal. It never changes rule outcomes,
terrain, production aura membership or campaign modes.

Native menu-host evidence additionally showed that UnitMoveController returns
before spatial registration when no Astar navigation graph is loaded, and
EntityDestructionController only traverses registered area/cross-scene states.
Profile actors now use native CrossSceneState (including actual permanent
removal), and their initial static positions are registered once in the native
InteractiveObjectGrid. Area membership, control and typed rules are untouched.
This fixture does not claim movement, navigation or terrain qualification.

The completed final sequence, exact candidate hashes and assertion mapping are
in [the final qualification record](../reports/magic-circle/FINAL-CANDIDATE-QUALIFICATION.json).
Twelve saved-world runs passed, including both enhancement configurations and
all four content/control persistence combinations. Exactly two Working writes
prepared and removed the fixture; a final fresh load proved absence.
[The handoff](MAGIC-CIRCLE-HANDOFF.md) records the scoped profile limitations and
verified restoration of the original normal-play installation.
