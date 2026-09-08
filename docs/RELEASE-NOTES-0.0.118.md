# Kingmaker Gunslinger 0.0.118

`0.0.118-contextual-world-map-teleportation`

Install **KingmakerGunslinger-0.0.118-contextual-world-map-teleportation.zip**
through Unity Mod Manager and restart the game. Existing module settings are
preserved. The new World-Map Teleportation Spells module defaults ON.

## Contextual travel spells

Select a previously visited destination on the world map while the party is
stationary. If an active party caster has a usable spell, its action appears
beside the native destination actions, with caster and available-use count.
With no usable spell, the normal destination interaction is unchanged. Native
Travel remains first/default; dismissing a panel or confirmation spends nothing.

- **Teleport**: Wizard/Sorcerer 5, Travel domain 5 where its native list exists.
  Confirmation shows ordinary visits, familiarity and exact d100 odds. Failed
  arrivals use another legal visited point; mishaps damage the traveling party
  and reroll. A cast spends one real spellbook use, including rules-level failures.
- **Greater Teleport**: Wizard/Sorcerer 7, Travel domain 7 where present. Also
  known as Teleport Without Error; arrives exactly at the selected point.
- **Word of Recall**: Cleric 6, Druid 8. Appears only on Oleg's world-map point
  before the capital is established, then only on the capital's point.

Magical travel relocates the canonical party token without entering a local
area, traversing a route or advancing world-map time. The spellbook, native
level-up and preparation paths remain the means of obtaining real casts.
Existing characters receive no automatic grant; spontaneous casters obtain
these spells through normal level-up or respecialization. Items and scrolls
are outside this feature. Both desktop and controller point panels are supported.

## Retained content

Includes the published 0.0.117 Elemental Races stabilization, its 12 heritages,
11 feats and 19 published alternate racial traits, plus prior Gunslinger and
Brown-Fur fixes. All 1,869 published blueprint entries retain their exact
identities; only three strategic spell abilities are appended.

## Evidence and owner testing

The owner authorized publication now and will test teleportation in an existing
high-level game. This decision supersedes the original requirement to finish
every compatibility and persistence gate before release. It does not turn
untested behavior into a passing result.

The combined deterministic suite passes **1,550** cases. Clean Release and strict
135-file package validation pass, together with 279 launcher preflight checks.
The combined-source casting, spellbook and controller runs pass 114 assertions. Native development
evidence includes real prepared/spontaneous expenditure, no-spell vanilla
interaction, normal Travel, cancellation, on/off/similar arrival, repeated
mishaps, associated pets, controller panels, level-up/spellbook presentation,
22 special-point casts, delayed-reveal protection and all 26 module boundaries.
Exact final build, package, runtime IDs and hashes are recorded in the
[implementation report](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/codex/contextual-world-map-teleportation/TELEPORTATION-SPELLS-IMPLEMENTATION-REPORT.md).

Full campaign disk persistence, the complete isolated save-backed compatibility
matrix, Arms and Armor, and UMM 0.32.4 remain unqualified. Current runtime testing
uses UMM 0.33.0.0. Standalone and Toggle Custom Soundpacks 1.0.0 startup and spell
publication passed on a development artifact; the working save's Craft Magic
Items data prevents using it for profiles without that mod. Native owner-graph
serialization and level-up reconstruction passed, but are narrower than a full
campaign save/reload. No claim of complete compatibility is made.

For player testing, follow the [short smoke-test guide](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/v0.0.118/docs/TELEPORTATION-SPELLS-SMOKE-TEST.md).

The firearm SoundBank retains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items integration remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test and 1,325-test
checkpoints remain historical evidence; the current suite contains 1,550 cases.
