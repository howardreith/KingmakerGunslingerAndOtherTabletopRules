# Contextual world-map teleportation implementation report

Status: IN PROGRESS. The native desktop contextual casting checkpoint has
structured runtime evidence; complete feature and compatibility qualification
remain unfinished. Release metadata remains 0.0.116.

## Base and scope

Clean base: `58d9511082af30f1a4ec88c1238ae7ae2b3651c2`, verified against freshly
fetched `origin/master` before source edits. The expected
`6874dc15a27ded132456dbdd480f47c794543a05` is an ancestor; all eleven intervening
commits, including published 0.0.115 and 0.0.116 work, were inspected and retained.
See `docs/TELEPORTATION-NATIVE-FORENSICS.md` for the reconciliation. Branch:
`codex/contextual-world-map-teleportation`. No merge or history rewrite occurred.

Final qualified source SHA: pending completion of all required gates.

Pushed coherent checkpoints:

- `0cd3eca4684bfa181746fca23d849319b745bd15`: pure teleportation policies and guarded native inventory.
- `11540f8efe6301262d92330e9f3a93b077e3584d`: compatibility profile package identity and exact restoration regression.
- `d0b746d8542655665eb4842c90cb104644c16a36`: contextual actions, alternate outcomes, one-use transactions and mishaps.
- `b919c267bcadc4f005a5777ea969778d84e64c57`: guarded native world-map panel observation.
- `87782a8a8c742aa91f43b663e2d88ac138518ea0`: default-ON twelfth module and schema-11 contracts.
- `15e7a644b1e11dc473fc4fc2d461ae935d139961`: real strategic spell blueprints and isolated native-list transactions.
- `321c0fe884e8a43604a794e017d2cd382ac89806`: save-owned ordinary-arrival observer and guarded familiarity qualification.
- `8eede43c0dfb3b8bfb3bbe5656b905cb2add6155`: real prepared/spontaneous source enumeration and proven native resource expenditure.
- `53b373575fb71a80d7da69849b028f2d5a34a33b`: exact current world-map destination and capital-state composition.
- `2f0b6bfadfca93269bc2255e98ae69bf818976f7`: native desktop rows/confirmation, canonical real casts and protected relocation.

Every checkpoint was pushed using the owner's exact policy wrapper. Draft
[pull request #10](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/10)
is open with an evidence-based in-progress summary. The final qualified source
commit list remains pending implementation.

## Implementation and evidence

Pure policies cover the exact d100 table, familiarity thresholds and strict
versioned ledger encoding, centralized point eligibility, exact recall IDs,
positive source availability, deterministic source ordering/deduplication,
alternate distance/bucket selection, idempotent single-use commitment, technical
compensation guards, and repeated mishaps with a defensive cap. Domain coverage
does not establish actual native spell expenditure or relocation.

The native desktop destination presenter is `GlobalMapMessageBox.OnLocationSelect`.
Its existing vertical destination layout and native Accept/Hide/circle buttons
were inspected through guarded structured evidence. Additional spell actions
now append to this panel. The native Accept operation, button order/labels and
callbacks remain unchanged. A no-source selection creates no spell rows or
confirmation. The guarded casting checkpoint below supplies evidence.

The three manifest identities are:

| Spell | Symbol | Stable ID |
|---|---|---|
| Teleport | `KMG.Spells.Teleport.Ability` | `82e3fb1dce1647b58d3b7169c8520af0` |
| Greater Teleport | `KMG.Spells.GreaterTeleport.Ability` | `73d19adfe18743e0a2a3a21abf4af5f3` |
| Word of Recall | `KMG.Spells.WordOfRecall.Ability` | `596d85a666204d6ea5c0188e53f4b4de` |

The publication slice adds Conjuration/standard presentation,
empty non-null material data, no local targets/effects, no metamagic, and no
action-bar autofill. It publishes Wizard 5/7, optional exact Travel domain 5/7,
Cleric 6, and Druid 8 through a separate reversible native-list transaction.
Repeated publication preserves already valid native list instances; rollback
restores exact prior list and filtered-cache references. It never grants spells
to characters. Late `SpellListComponent` metadata is allowed by exact native type;
other extra components are rejected.

Native `Spellbook.Spend` handles prepared groups and spontaneous slots. Native
`RestoreSpontaneousSlots` and captured prepared-slot availability restoration
are implemented in the production cast-source/resource adapter. Guarded native
resource probes and actual contextual transactions are recorded below.

The settlement-circle wrapper calls `TeleportParty`, which opens outgoing edges.
That wrapper would violate the no-reveal contract. The narrower native
`SetCurrentPosition(new MapPosition(point))` plus `UpdatePawnPosition()` path was
traced and exercised by the actual contextual cast checkpoint below. The shared
native token moves without entering an area or traversing a route.

`UnitPartTeleportFamiliarity` now owns the versioned ledger on the canonical main
character. `Player.OnAreaLoaded` performs idempotent legacy migration. Save/load
callbacks do not rewrite the payload; disabled modules install no familiarity
hooks. A narrow `MoveAlongEdge` observer captures native initialized progress and
completed route boundaries without planning or changing movement. It handles
intermediate points, final arrivals, revisits, partial-edge starts, native reveal
stops and duplicate callbacks. The native `RevealPath` flag is not a visited-state
flag: both native constructors initialize it true, even for known roads.

Guarded native movement, live no-op save/load callbacks, and ownerless UnitPart
payload serialization have passed. Full campaign owner-graph disk save/reload
qualification remains outstanding; serialization alone is not that proof.

The [point audit](docs/TELEPORTATION-MAP-POINT-AUDIT.md) and companion CSV retain
706 unique stable IDs and 611 observed main-map anchors. Point-specific explicit
exclusions and complete campaign arrival qualification remain open. The production
current-state adapter and isolated Recall-state fixture are qualified below.

## Validation checkpoints

- Current domain suite: 1,470 passing cases, including 4,096 module settings round
  trips and 4,096 publication-intent combinations. The guarded catalog generates
  26 boundary configurations for 12 modules.
- Clean Release build and strict installable-package validation passed for the
  publication candidate. Final artifact qualification remains in progress.
- Native standalone inventory: `20260907T2336260492141Z-78c2851fd60a41468e664a68683e50a8`,
  directory `20260907T2336260376893Z-observe-teleportation-native-contracts`, PASS;
  compatibility transaction `compat-20260907T233547Z-878395056426` restored exact bytes.
- Native panel: `20260908T0028468837443Z-700a1cbb6ead4159812161c32be81f08`, directory
  `20260908T0028468636122Z-observe-teleportation-world-map`, five assertions PASS.
- Twelve-module working-save regression: `20260908T0044326727368Z-252ae5a1786547be8ab5ee7ea16f3d2d`,
  directory `20260908T0044326571115Z-working-save-smoke`, eleven assertions PASS.
- Publication/list rollback with installed mods: `20260908T0121349574829Z-c5b927ca303441749237d48dd72e9151`,
  directory `20260908T0121349454809Z-observe-teleportation-native-contracts`, seven assertions PASS.

All listed directories are under the machine-local project runtime-evidence root.
Raw artifacts, saves, proprietary assemblies, and packages are not committed.
Earlier rejected probes and their diagnoses are retained in the native-forensics
notes; two publication probes correctly failed the initially overstrict component
contract before exact inert native list metadata was identified.

Additional publication qualification:

- Standalone ON: `20260908T0125261375669Z-778971707ad141d0a3b7afab95922da6`,
  directory `20260908T0125261285579Z-observe-teleportation-native-contracts`, seven
  assertions PASS; exact restoration `compat-20260908T012436Z-1388860e92dc`.
- Teleportation OFF / other eleven ON: `20260908T0129144377215Z-2b8748198eca415fbcca4e2006bd264f`,
  directory `20260908T0129144220691Z-observe-feature-module-settings`, 31 assertions
  PASS with exact settings restoration and no OFF-mode publication fixture.
- Publication working-save regression: `20260908T0133209151824Z-e0fd19bcdadc456dbe3a1b1ea77bc991`,
  directory `20260908T0133208995428Z-working-save-smoke`, eleven assertions PASS.

## Familiarity checkpoint

- Installed-profile run `20260908T0223050324619Z-eb7985f6f51d4f098d522387a20eb2ee`,
  directory `20260908T0223050168190Z-disposable-teleportation-familiarity`, nine
  assertions PASS. Native panel Accept created each two-edge ordinary route;
  native movement crossed the intermediate and destination points exactly once
  on three trips. Revealed edges were revisited in both directions. Fixture
  placement, selection, cancellation and canonical placement alone added no
  arrival; exact tracked fields/payload were restored with no save write.
- This fixture supplies a request-local native time step and invokes native
  `MapMovementController.Tick`; it does not qualify spell casting, magical travel
  invariants, or a complete disk save/reload.
- Earlier runs `20260908T0210049014005Z-a6db54375d6842de871763945981aa0a`
  and `20260908T0215131100646Z-e3345b3289c2419c9e5c3b014a9fe56f` were ERROR:
  a fixture wrongly required `RevealPath=false` on known roads. Native constructor
  inspection and structured preview/actual route evidence corrected that assumption.
- Run `20260908T0219294157924Z-f773af6e672649e89dde88a37aec5da8` passed all
  three native travel legs but was ERROR overall: generic JSON serialization
  followed the live UnitPart Owner into Unity data. The corrected carrier probe
  keeps the live owner graph out of its scope; disk persistence stays pending.
- The compatibility wrapper now preserves schema 11 and all twelve module keys.
  Its actual parameter-validation and settings-construction code passed 4,120
  focused checks (4,096 settings combinations plus absent/mistyped keys).

- Standalone arrival run `20260908T0232569730289Z-b4cdb0b1cfee4e8b944bac6a69901fd9`,
  directory `20260908T0232569670314Z-disposable-teleportation-familiarity`, TIMEOUT
  before the feature fixture: native `Player.PostLoad` could not locate the main
  character in the cross-scene entity state. Catalog identity and load correlation
  passed; load completion did not. No save was changed. Profile transaction
  `compat-20260908T023211Z-f488f858a5a5` verified exact restoration. A valid standalone
  save-backed fixture remains to be established; this is not a passing feature run.
- OFF hook-audit run `20260908T0243251891594Z-cc8b7de8808c47b89db97e9f1358dc9f`,
  directory `20260908T0243251771556Z-observe-feature-module-settings`, ERROR before
  assertions: the installed Harmony12 bridge throws when `GetPatchInfo` receives
  an unpatched method. The probe now checks the actual patched-method registry
  before requesting details; zero installed hooks must still be proven. Exact
  settings restoration passed.

- Corrected OFF run `20260908T0250396138426Z-1b12dee6c0c04cd1977f7dcd49b0bac7`,
  directory `20260908T0250396018424Z-observe-feature-module-settings`, 32 assertions
  PASS: no familiarity hooks installed, no spell publication, exact settings bytes
  restored. The bridge workaround observes the registry; it does not suppress an
  error in production movement or infer absence from the module flag.
- Standalone startup run `20260908T0253148546708Z-b7f27f81de57400b93fcb15c9439d023`,
  directory `20260908T0253148446200Z-observe-teleportation-native-contracts`, eight
  assertions PASS, including exactly two installed familiarity hooks. Profile
  transaction `compat-20260908T025227Z-ff5836da6505` restored exact bytes. This
  save-free result does not resolve the standalone working-save load failure.
- The corrected familiarity candidate passed all 1,439 domain cases, a clean
  Release build, and strict installable-package validation. Local logs:
  `artifacts/teleportation/domain-familiarity-bridge.log` and
  `artifacts/teleportation/build-familiarity-bridge.log`.

- Restored installed-profile working-save regression
  `20260908T0255386128427Z-bf16bca1f4aa4a3ca198fa01d923d29f`, directory
  `20260908T0255386008390Z-working-save-smoke`, eleven assertions PASS. This
  confirms the guarded named save loads with the restored installed profile.
- Runtime preflight passed all 208 checks plus the world-map metadata check;
  the compatibility module parameter/settings regression passed all 4,120 checks.

## Real spell-resource checkpoint

`TeleportationSpellbookAdapter` reads current active-party sources, never cached
class possession. Prepared uses are exact current native instances, including
reciprocal opposition-slot groups; spontaneous sources require actual known data
and a positive native correct-level pool. Sources retain caster/book identity,
current counts and labels and re-resolve when selected later.

`TeleportationNativeCastResource` captures all native resource levels and exact
collection/slot/ability/link identities. It calls native `Spellbook.Spend`,
requires its success, proves exactly one selected use changed, and guards duplicate
calls. Exact prepared-field or native spontaneous restoration is allowed once
only after an attributable one-use change. Another operation's resource change
cannot be refunded by this lease. The request-local transaction still owns the
separate pre-effect/after-effect and rules-failure refund decisions.

Initial installed-profile run
`20260908T0318149717580Z-e4d810cac736401d9836f64429241bb7`, directory
`20260908T0318149620713Z-disposable-teleportation-resources`, passed 17 assertions.
Seven real sources covered two owners, three native spellbook types, opposition
and ordinary preparations, fifth/seventh-level spontaneous slots, exact spend,
duplicate suppression, exact restoration and stale/exhausted omission. Original
books/resources/attributes, party, time and map were restored; no save write.
This predates the additional attribution and special-known controls; no
contextual casting is qualified by this probe.

- Attribution run `20260908T0327477925922Z-7b95d23ec68642499170889059dbc317`,
  directory `20260908T0327477805944Z-disposable-teleportation-resources`, 18
  assertions PASS, including refusal to refund another operation's native debit.
- Attribution-build working-save regression
  `20260908T0332593382632Z-b752783d7c6a46e4a33906d50f45a7ff`, directory
  `20260908T0332593301534Z-working-save-smoke`, eleven assertions PASS.
- Final resource checkpoint run
  `20260908T0342259125609Z-337c4bc0fcbd45f6ae14803d9563b042`, directory
  `20260908T0342259005448Z-disposable-teleportation-resources`, 19 assertions PASS.
  This adds an exact native special-only known instance, which spends one real
  spontaneous slot through the same adapter. Both common and special known lists
  are checked during discovery, capture and expenditure. Fixture cleanup and
  save-write sentinels passed.
- All 1,449 domain cases, clean Release build and strict package validation passed
  for the final resource checkpoint (`artifacts/teleportation/build-resources-special.log`).
  Runtime preflight remains 208 passing checks plus map metadata; the compatibility
  parameter/settings regression remains 4,120 passing checks.

These resource runs used the installed profile. Standalone save-backed resources,
full required compatibility profiles and all contextual casting gates remain open.

## Current map and destination composition checkpoint

`TeleportationWorldMapAdapter` reads the current native campaign, loaded static
map scene, anchored canonical pawn, mode stack, loading queue/screen, combat,
dialogue/scheduled dialogue, cutscene pool, kingdom modes, encounter state, travel
command and request occupancy. It reads existing location records through
`GlobalMapState.Locations.TryGetValue`; point composition creates no records or
ledger entries and performs no route calculation. Native `LocationRestriction`
checks and closure state supplement exact registered blueprint/current anchor,
reveal, visit, point kind and origin checks.

The base-game capital region is `caacbcf9f6d6561459f526e584ded703`. Its prebuilt
settlement exists before region claim, so Recall establishment follows that exact
native region's claimed state. After claim, the settlement must belong to that
region and resolve the exact capital point. Unknown state or an invalid established
anchor never falls back to Oleg's. An absent kingdom is the pre-capital case.

Guarded installed-profile run
`20260908T0421564890455Z-9fc529962db34a83958398ca5b84c1e9`, directory
`20260908T0421564779636Z-disposable-teleportation-context`, passed 14 assertions.
Six actual Greater/Teleport source rows covered two casters and distinct native
books; Recall appeared only at its resolved point before/after capital claim.
Current, closed, hidden, unvisited and pending-relocation controls omitted actions.
Reading all 611 current points and repeatedly composing sources changed no native
map records, familiarity, spell resources or time. The temporary capital state,
original absent kingdom, map placement and real books were restored with no save
write. This qualifies production action composition, not rendered contextual rows,
confirmation, native normal-travel continuation or a completed magical cast.

The prologue save has no kingdom. The fixture uses native KingdomState's exact
private deserialization constructor, then assigns only the newly constructed
object's Regions collection with a native prebuilt RegionState. It briefly binds
that object in the guarded fixture and restores the original reference before
exit. Native region-claim/founding actions and the notifying public kingdom
constructor are never used. No fixture entry point is available in normal play.

Rejected probes are retained truthfully:

- `20260908T0410123244107Z-4af31b8155f34375ae74482c97def1cc`, directory
  `20260908T0410123164095Z-disposable-teleportation-context`: ERROR before fixture
  mutation because the initial probe incorrectly expected an existing kingdom.
- `20260908T0416458142761Z-984e0307ba574bed915837f1375b74f5`, directory
  `20260908T0416458022745Z-disposable-teleportation-context`: ERROR after proving
  the native UI scene (`UI_Globalmap_Scene`) differs from the map scene
  (`Globalmap`). The gate now uses the loaded area's native static scene; exact
  cleanup and no-save-write assertions passed even in that rejected run.

All 1,454 domain cases, repository validation, clean Release build and strict
package validation passed (`artifacts/teleportation/build-context-static-scene.log`).
The compatibility parameter/settings regression passed all 4,120 checks, and
runtime preflight passed 208 checks plus the guarded map metadata check.
Final clean Release/package log: `artifacts/teleportation/build-context-checkpoint.log`.
The explicit domain command also passed (`artifacts/teleportation/domain-context.log`).
Working-save regression `20260908T0430569217853Z-ef80210c30b6463a86bb0d90477a39ff`,
directory `20260908T0430569137709Z-working-save-smoke`, passed all eleven assertions.
The current package remains 0.0.116; no release promotion is implied.

## Native desktop contextual casting checkpoint

The manual module-gated hooks append positive source rows after native
`GlobalMapMessageBox.FillDialogInfoLocation`. Native Accept, Hide, Enter and
circle actions retain their existing callbacks and presentation. There is no
raw pointer hook, normal-travel interception, persistent travel window or free
cast entry. The owned inactive button clone clears persistent events and its
donor localization component, then uses current localized source labels.

The selected row closes the native point panel and opens its native dialog
confirmation. A request owns its exact source/destination and an idempotent
transaction. Confirmation re-reads source availability and all current map
conditions. Cancellation, stale selection and repeated callbacks cannot spend
another slot. Rules results, including failures, use the one committed resource.

Native graph ranking uses the existing QuickGraph graph and native weight
function. D100/D10 production rolls use RulebookEvent.Dice. Each mishap sends
its one d10 value through RuleDealDamage for every living canonical traveler,
without a hit-point floor. Native relocation compares structured before/after
party/pet identities, canonical token, world time, weariness, travel/encounter
state, all point/edge discovery records, perception IDs and the saved ledger.

Run `20260908T0508534702414Z-e3ef7871730244eda49763aa3789b9db`, directory
`20260908T0508534612400Z-disposable-teleportation-casting`, was rejected: cloned
LocalizedUIText.Awake reset every row to Accept and inherited ignoreLayout left
rows overlapping. Exact cast mechanics passed but this was an overall FAIL.

Corrected run `20260908T0518061328136Z-dcef820240d34772a501292ba833475e`, directory
`20260908T0518061237339Z-disposable-teleportation-casting`, passed 37 assertions.
The two native Wizard sources and one Sorcerer source produced six unambiguous
rows, with original native actions unchanged. It proved Cancel, repeated menu
selection, exact Teleport, spontaneous seventh-level Greater Teleport, pre/post
capital Recall, graph-ranked off-target/similar outcomes, repeated confirmation,
stale destination and one/repeated mishaps. Damage rose by 1 in all three living
members for one mishap and by 1 then 2 for repeated mishaps. Every successful
cast proved protected state unchanged, a single real resource expenditure,
exact cleanup and no save writes. No pet/mount was present in this working save.

The local result artifact is `teleportation-casting.json` within each named
runtime directory. Build/package log:
`artifacts/teleportation/build-casting-mishap.log`; all 1,467 domain cases passed.
The strengthened run `20260908T0531179887178Z-e33c3e7fe3fe46a781e7a99c5883f0d7`,
directory `20260908T0531179797184Z-disposable-teleportation-casting`, passed all
41 assertions, adding no-alternate rules failure (slot remains spent), exact
pre-effect compensation, stale caster cancellation and canonical non-injected
dice (native d100 = 19).

Final casting checkpoint run
`20260908T0542206317921Z-5180c9651e2840f1bbe49631dddaa2ef`, directory
`20260908T0542206207909Z-disposable-teleportation-casting`, passed all 42 assertions.
It additionally replaced the selected physical spellbook with a real new native
book having the same caster, blueprint and available preparation: the open
confirmation canceled with both books unspent. Confirmation binds the opening
Player/Map/Rules and physical book references while re-resolving the valid use.
The fixture restored the exact original book dictionary entry and all state.

Module OFF run `20260908T0535060528821Z-be76dbc835524646b26a0df3844267ea`, directory
`20260908T0535060408993Z-observe-feature-module-settings`, passed 33 assertions.
The actual Harmony registry contained zero familiarity and zero destination hooks;
the three spells were absent from their player lists. The settings transaction
restored exact bytes, SHA-256
`a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`.

Working-save regression `20260908T0545391191224Z-94ee28fffc1445669dd335920b10d484`,
directory `20260908T0545391101186Z-working-save-smoke`, passed all eleven assertions.
All these requests launched through Steam App ID 640820 and preserved the save
write sentinels. Current clean Release/package log:
`artifacts/teleportation/build-casting-book-identity.log`; explicit domain log:
`artifacts/teleportation/domain-casting.log`. Runtime preflight passed 208 checks
plus the map metadata check; compatibility parameter/settings checks passed 4,120.
No generated package, runtime payload or campaign save is committed.

## Native interaction across frames

`disposable-teleportation-interaction` now uses actual Unity updates, native
CameraRig scrolling, the original Travel button, the native Escape stack and
native dialog buttons. It counts IPawnMovementHandler start/stop events, verifies
unchanged native button callbacks/order, and restores all fixture state before
closing save-write sentinels. A long source list uses a native ScrollRect within
the space above/below the selected point. A fresh native-only layout measurement
prevents previous rows from shrinking the next menu's viewport.

- Final interaction run `20260908T0645599302678Z-3fbdcbc3efcc4ef3a07e1687ff737d38`,
  directory `20260908T0645599222679Z-disposable-teleportation-interaction`, passed
  29 assertions. Both the no-source and augmented native Travel actions emitted
  exactly one start and completed the ordinary two-edge route. Escape, force
  closure, stale destination, native dialog replacement, unvisited/current-point
  omission, live count changes, exhausted-row removal and a real delayed
  confirmation passed. Eight reopens retained a 213-unit source viewport. Twelve
  real sources used 426 units of content within a 370.1854-unit viewport; the
  panel remained on screen and native scrolling reached the final source.
- Full casting regression `20260908T0651138625548Z-e62f45df4e7247b9b97325b3cf24ffc8`,
  directory `20260908T0651138505575Z-disposable-teleportation-casting`, passed all
  42 assertions after the layout change. Its fixture now centers distant selected
  points through the native camera API before invoking native point selection.
- Earlier interaction run `20260908T0616354540124Z-120ea8c2089b477f9c3b9135e7e37aed`,
  directory `20260908T0616354420112Z-disposable-teleportation-interaction`, was FAIL:
  the probe assumed native HandleOpen replaced an already shown dialog. IL and
  runtime establish that it returns unchanged. The corrected probe uses native
  ForceClose followed by native HandleOpen and proves the old request cancels
  without closing the unrelated replacement.
- Run `20260908T0628168568585Z-c60e43921e9642eba455525480a0b21e`, directory
  `20260908T0628168448610Z-disposable-teleportation-interaction`, was ERROR: twenty
  frames did not guarantee the native fade had completed at uncapped FPS, and the
  third companion did not satisfy the assumed absent-book prerequisite. The
  probe now waits for observable native alpha and uses temporary real native
  Druid books on the two established fixture owners for the long-list case.
  These unusual fixture preparations are removed before completion; production
  publication and character acquisition remain unchanged.
- The corrected intermediate run
  `20260908T0637242006253Z-1d3f0bdea7de4987b5ab067c320ea295`, directory
  `20260908T0637241916142Z-disposable-teleportation-interaction`, passed 28
  assertions before the additional viewport-stability check.

All these runs launched through Steam App ID 640820. Fixture restoration and
zero save writes were proven, including the rejected probes. Artifacts are
`teleportation-interaction.json` and `teleportation-casting.json` in the named
machine-local directories. Explicit domain log:
`artifacts/teleportation/domain-interaction-final.log`; clean Release/package log:
`artifacts/teleportation/build-interaction-checkpoint.log`. All 1,470 domain cases
and strict package checks passed. Runtime preflight passed 208 checks plus the
map metadata check, and compatibility parameter/settings checks passed 4,120.
Working-save regression `20260908T0703527193851Z-6454ae9f84a34c5aa1daee5627084490`,
directory `20260908T0703527103850Z-working-save-smoke`, passed all eleven assertions
with zero save writes. Complete compatibility qualification is separate.

## Remaining qualification and constraints

Full campaign familiarity disk save/reload, actual associated pet/mount fixtures,
unconscious/dead traveler cases, remaining UI lifecycle/input boundaries,
gamepad augmentation/navigation, complete 26-state module boundaries, and all
required compatibility profiles remain incomplete. The current UI qualification
uses structured native button invocation and measured on-screen geometry across
frames; it is not a presentation review of every camera position/resolution.

The installed host is UMM 0.33.0.0; the requested 0.32.4 host has not been qualified.
Required Arms and Armor and Toggle Custom Soundpacks references were absent from
the configured reference folder, installed Mods, and inspected project backups.
Owner questions about those references, UMM qualification, and the next unused
release version remain pending. Existing 0.0.115/0.0.116 releases will be preserved.

These checkpoints do not establish complete feature, final compatibility or
release readiness.
