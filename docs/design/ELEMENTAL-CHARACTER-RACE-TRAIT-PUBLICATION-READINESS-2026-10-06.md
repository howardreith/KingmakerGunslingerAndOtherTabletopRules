# Elemental character race trait publication readiness — 2026-10-06

Disposition: **BLOCKED-ONLY-ON-ORIGINAL-ICONS**.
TraitsPublished: false.

This is a dormant publication contract for Fiery Glare, Stoic Dignity, Aerial
Observer and Whiteout. It does not replace any qualified mechanics, publish a
trait or register a visible blueprint, icon consumer or localization string.
The four foundation implementations and specialized runtime evidence remain
historical qualification of their exact artifacts, not qualification of this
new readiness DLL.

## Identity and graph authority

[The publication plan](../design/ELEMENTAL-CHARACTER-RACE-TRAIT-PUBLICATION-PLAN-2026-10-06.json)
lists all 11 proposed stable symbols/GUIDs. The compiled semantic authority is
`ElementalCharacterTraitCatalog`. These IDs are deliberately absent from the
live blueprint manifest. They must never be regenerated, recycled or replaced
by guarded fixture GUIDs. All hidden production facts that can persist get
stable identities too.

There are four visible rank-one character trait features, one visible Fiery
Glare toggle, and six hidden graph nodes. Features alone enter the selection.
No hidden provider, activation/recipient buff or area is independently selectable.
The catalog and returned arrays are immutable snapshots.

The dormant factory preflights every planned ID against the actual library
before allocation. A conflicting canonical/native/foreign identity rejects the
whole graph; it never adopts or overwrites a lookalike. Exact canonical race
references are required, not matching names or copied GUIDs. The supported host
contract is `ZFavoredClass.NewMechanics.PrerequisiteRace`, derived from native
`Prerequisite`, with exact `race: BlueprintRace`. The installed Kingmaker
assembly itself has no class named `PrerequisiteRace`. Each feature also has
native `PrerequisiteNoFeature(self)`, both in GroupType.All.

| Trait | Exact race | Granted graph |
| --- | --- | --- |
| Fiery Glare | KMG Ifrit | Feature → activatable → hidden buff → native Take10ForSuccess(CheckIntimidate) |
| Stoic Dignity | KMG Oread | Feature → hidden holder provider → moving 10-foot ally area → hidden recipient |
| Aerial Observer | KMG Sylph | Feature → hidden conditional Perception provider using exact KMG Wings of Air |
| Whiteout | KMG Undine | Feature → hidden weather provider with its exact stable blueprint backlink → existing dormant narrow attack adapter |

Fiery Glare retains the qualified off-by-default, immediate/free/no-resource
native toggle contract, combat availability and success-only take-10 behavior.
The future visible toggle is allowed on the action bar; only this dormant
output changes `ActionBarAutoFillIgnored` to false. Feature/toggle share one
qualified original icon and the exact staged feature description keys.

Stoic retains the qualified self Trait/ally Morale save component, holder
consciousness, exact same-effect/source-lineage suppression, ambiguity-grants
fallback, ally-only 10 feet, self-exclusion, rule replay and native stacking.
The area/recipient/self component references are the qualified graph factory's
objects. Aerial verifies canonical Wings of Air and its complete qualified
native gameplay composition; foreign wings, height and generic flight remain
excluded. Whiteout gets its stable ID before any use, never calls the random
fixture factory, and uses the unchanged qualified weather component and exact
provider backlink. The attack patch, 1–10 d100 boundary, sequential 28% probability,
outdoors actual Rain/Snow Light+ rule, IgnoreConcealment and authorized Seeking
behavior are unchanged.

## Asset admission and dormant reachability

The only native graph constructor is uncalled `CreateDetached`. Its first
operation reads the real immutable original-asset evidence catalog and requires
all four originals. That catalog is currently empty. Hypothetical test receipts
cannot be passed to this native factory. It has no registration method or
bootstrap caller.

After admission, the future factory would require supported Favored Class,
enable_traits, enabled Elemental Races, four complete valid graphs, conflict-free
IDs and four exact qualified 128px original sprites. It can stage unresolved
name/description key objects and text, but does not register localization or
blueprints and cannot publish a selector. Detached object cleanup refuses to
destroy any object subsequently registered; a future native transaction must
remove exact registry entries before detached disposal.

No new entry is added to the live icon catalog, OwnedIconAssignments, blueprint
manifest, setting, race grant, feature selection, README or release list.
No image or substitute exists in the package.

## Late publication and transaction

The sole target is supported Favored Class `racial_traits`:
`331ed3c4a988415785f71a37b826d0f1` / `RacialTrait`.
Its exact inspected contract leaves Features empty and consumes AllFeatures.
Do not change Features, publish into Adopted, replace a racial ability or create
another selection UI.

`ElementalCharacterTraitPublicationTransaction<T>` is a dormant plan over the
existing unchanged `HelpfulPublicationTransaction`. It:

1. Requires all host/module/traits/graph/original-asset conditions and the exact
   target before any action.
2. Validates the complete four-feature batch, every planned/native identity and
   foreign catalog uniqueness before any registration.
3. Rechecks host, identities and current target immediately before commit.
4. Executes supplied reversible native-registry/localization steps, then appends
   the four exact canonical features in catalog order to AllFeatures.
5. Preserves all foreign references/order, treats existing exact own entries
   idempotently, and rejects a foreign object carrying an owned identity.
6. Rolls selection back first, then owned registry/copy steps in reverse order,
   including an apply operation that threw after partially changing state.
7. Preserves later foreign selection additions on rollback. If selection rollback
   fails, it retains registry/copy resolution and reports incomplete rollback
   instead of creating dangling selector references.

Future native adapters must preflight all library keys/types/components and
copy-key ownership, supply exact idempotent rollback, and re-resolve current
host/settings/identities at commit. They must never overwrite foreign blueprints
or text. The plan contains no live native registration adapter or bootstrap;
those are connected only in the all-four asset-qualified publication pass.
This is not a live publication qualification claim.

## Save, respec and module contract

Stable visible and hidden IDs become save API only after deliberate registration.
No ordinary save can acquire any new ID in this mission. Guarded fixture factories
continue to use request-local random identities; they are not production restore
identities.

Acquisition removal and mechanical deactivation are different operations.
Traits disabled or host absent/incompatible removes only owned late-selection
entries; registered stable identities must remain resolvable for saved facts.
Module disabled removes/deactivates owned providers and leaves stable identity
resolution intact. It must not delete saved facts, unregister save APIs or rewrite
saves. Re-enabling may rebuild an active feature's owned provider once.

`ElementalCharacterTraitOwnedGrant` is new dormant graph glue. Its private
`[JsonProperty] Fact _ownedFact` records only the grant owned by this feature.
On activation/turn-on it reuses a live exact recorded fact with the canonical
blueprint in the correct owner collection. Stale/disposed/missing entries are
replaced; respec, removal and turn-off remove only that exact recorded fact.
It never sweeps by blueprint or removes foreign grants.

Provider buffs are added with explicit holder/self MechanicsContext. Native
AddFacts passes null context and would not supply Stoic's attached-area
MaybeCaster link, so it is not used to grant these providers. The qualified
AddAreaEffect/recipient lifecycle handles owned area and recipient cleanup.
Activatable grants use the native fact collection. No static owner dictionary,
global scan, frame polling, save-writing API or migration is introduced.

The policy and source contracts are deterministic-tested and the new glue
compiles against exact installed references. Actual feature grant/removal,
save/load reconstruction, toggle restoration, module transitions and respec
still require guarded player-publication runtime qualification. A writable
persistence fixture is not authorized here; no save write is made or claimed.

## Exact original-art intake

The current guide, reference index, active icon catalog, production manifest and
protected assignments remain unchanged authorities. All four concepts are
missing:

| Concept | Original source | Canonical export | Qualified production record |
| --- | --- | --- | --- |
| fiery-glare | production/sources/fiery-glare.png | production/exports/fiery-glare.png | absent |
| stoic-dignity | production/sources/stoic-dignity.png | production/exports/stoic-dignity.png | absent |
| aerial-observer | production/sources/aerial-observer.png | production/exports/aerial-observer.png | absent |
| whiteout | production/sources/whiteout.png | production/exports/whiteout.png | absent |

Here production is
`assets-source/original-icons/icon-overhaul-v2/production/`.
Runtime exports belong at `assets/game/icons/<slug>.png` and install at
`assets/icons/<slug>.png`. Source dimensions/hash/format, export dimensions/hash,
approved export hash and owner review evidence are currently unavailable.

Four production brief **drafts** are stored at the established
`production/briefs/<slug>.json` paths. They have `draft:true`, null source hash,
null tool and `visualStatus:not-authored`. They are neither provenance records
nor approval. The active catalog has no concept/consumer/production record for
these traits. No art, copied file or active icon assignment was created.

Each future original uses painted-magical / project-painted-128, an actual
original PNG plus a decoded 128×128 8-bit noninterlaced RGBA export, its real
source/tool/prompt/reference provenance, distinct hashes, exact source/export
paths, production record and catalog assetAuthority, reviewed export hash,
review-group and protected-assignment checks. The groups are ifrit-traits,
oread-traits, sylph-traits and undine-traits. The five planned visible consumers
are explicit in the publication plan; Fiery feature/toggle share their one concept.
Hidden providers/buffs/area have no independent icon.

The production drafts give the required eye/flame, calm stone face, wind-borne
perception and precipitation-protection compositions. Approved project painting
references are style references only. No text, letters, monogram, generic flight
speed, magical fog, procedural substitute, donor or existing-icon copy is
permitted. Human visual approval is distinct from asset/build validation.

`python tools/inspect_elemental_character_trait_icons.py --expect blocked`
reports the actual four-concept intake without writing files or publishing.
It delegates active catalog/protected validation to the unchanged established
validator and rejects drafts, wrong provenance, malformed exports, stale owner
review, copied originals and incorrect visible-consumer mappings. Its ready
mode verifies the eventual complete asset/catalog/consumer candidate; it does
not register features. The production compiled receipt catalog can be populated
only by a later reviewed original-art intake, never by passing arbitrary test
metadata at runtime.

## Remaining owner action

1. Create the four original icons through an authorized art workflow.
2. Obtain owner visual approval of their exact exports.
3. Rerun this branch's asset gate and publication qualification, including
   stable native registration/localization/selection adapters, character
   creation, wrong-race/duplicate/respec/module controls, foundation mechanics,
   authorized persistence or its explicit fixture blocker, and final UI review.
