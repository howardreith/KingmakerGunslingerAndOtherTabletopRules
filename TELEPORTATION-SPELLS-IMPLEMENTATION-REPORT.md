# Contextual world-map teleportation implementation report

Status: IN PROGRESS. Native desktop and gamepad contextual casting have
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
- `35616afa59c9e4045317d92e727d6392a5e77d1f`: native Travel/Escape, live source counts, stable reopened layout and scrolling across frames.
- `ba32ac2d6e7a916aedbdf61b4dea2ff093d3c746`: native mishap life-state settlement, real associated-pet qualification and corrected world/resource fingerprints.
- `cdee670b2879431972f4a9baa70f249e15db8803`: native gamepad destination rows/navigation, shared native confirmation, rendered-text and module-hook qualification.
- `68e1e8a8111d68335ec25f8bc34a25f684520a77`: native spellbook rows/descriptions/preparation, action-bar exclusion and exact deferred UI cleanup.
- `5b1168f45edf4c8979e6dbbb5536344523faf61d`: actual native level-up choices, preview cancellation and same-artifact UI/resource/save regressions.

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
706 unique stable IDs and 611 observed main-map anchors. The special-point audit
qualifies 22 actual arrivals and preserves 19 native campaign exclusions; no
unconditional point-specific deny was justified. Other campaign/map contexts and
full campaign persistence remain open. Exact scope and evidence follow below.

## Validation checkpoints

- Current domain suite: 1,490 passing cases, including 4,096 module settings round
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
members for one mishap and by 1 then 2 for repeated mishaps. Each successful cast reported a single real resource expenditure and no save
writes. Its anonymous fingerprint comparisons were later found incomplete; the
corrected serializer, negative controls and superseding runs are recorded below.
No pet/mount was present in this working save.

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

## Native mishap life state and snapshot correction

The first pet probe exposed a real production defect. All three original party
members and the three associated native pets had views but were outside
`Game.State.AwakeUnits`. RuleDealDamage changed HP damage, while the synchronous
mishap loop still read cached Conscious state. Thus a pet lethally damaged by the
first mishap incorrectly received the next packet. The same delay hid native
unconsciousness. The rejected run is
`20260908T0724492651938Z-55ced266ed42469d95beb2f72b0238ff`, directory
`20260908T0724492566466Z-disposable-teleportation-travelers`, FAIL. Cleanup and
save-write guards passed. The initial dead control also revived during the
confirmation wait through native out-of-combat recovery; the corrected fixture
establishes and records that control immediately before the actual commitment.

Production now exposes only the native UnitLifeController protected per-unit
boundary through a narrow subclass. It checks native life state before a damage
packet and settles native unconsciousness/death after it, before another reroll.
It neither registers a controller nor changes native thresholds, ferocity,
regeneration, difficulty, HP floors or recovery. Living travelers must have their
native views for a possible mishap; unknown support omits the Teleport source and
fails pre-spend revalidation. Unconscious living units remain included; dead
units receive no further packet. Material-effect tracking begins before native
life/damage operations, preserving the no-refund-after-effect rule.

The probe also exposed global JSON defaults that reduced some anonymous
fingerprints/logs to an object identifier. Early PASS results are retained as run
history, but those anonymous comparisons are not accepted as protected-state,
resource-fingerprint or native-action-content proof. Typed slot-delta checks and
explicit roster/reference checks were independent. Production and fixture
fingerprints now use an isolated serializer with an explicit ordinary contract
resolver, without changing the game's save serializer. The corrected runtime
checks deliberately change world time, traveled miles, point flags and visit time;
each change must be rejected and exact restoration accepted. A real temporary
prepared expenditure must change its resource fingerprint. Structured cast logs
must retain action, world, resource, event and result fields.

Corrected installed-profile traveler run
`20260908T0743051211896Z-0b845aad9e574a77bd53746466e5827e`, directory
`20260908T0743051121851Z-disposable-teleportation-travelers`, passed 15 assertions.
It created three native leopard pets through SpawnUnit/SetMaster and an unowned
cross-scene control. The first mishap dealt 2 to each living traveler and killed
the fragile pet. The second dealt 1 to the surviving travelers and skipped that
pet. An already dead pet and the nontraveler took zero. A separate cast put a
living pet into native Unconscious state; it received both packets. Both casts
spent exactly one real preparation and preserved the canonical token/roster and
protected world state. All temporary entities, ownership, original HP/life/damage
attribution, book state, prefab identity, map data, ledger and time were restored
under intact save-write sentinels.

`teleportation-travelers.json` holds structured evidence. The production log check
found all six full cast records and zero teleportation errors. Native off-scene
death visuals emitted four bounded inactive-view coroutine warnings in this
fixture; life events and rules damage completed. No view was activated or moved
to suppress those native presentation warnings. Native difficulty recovery after
the synchronous result remains untouched. Permanent death under other difficulty
settings and mod-provided mount representations are not separately qualified.

Superseding regressions with the corrected comparisons:

- Casting: `20260908T0748191029883Z-d82a6141e3e84987958b3e1f4a29c030`, directory
  `20260908T0748190940075Z-disposable-teleportation-casting`, all 42 assertions PASS.
- Native interaction: `20260908T0751392109622Z-8c5bb2bbda2b44c0a07d7874e37a1120`,
  directory `20260908T0751392014182Z-disposable-teleportation-interaction`, all 29
  assertions PASS, with original native action contents/callbacks and actual
  resource fingerprints compared. Two ordinary routes each started exactly once.
- Resources: `20260908T0756240277406Z-8c30ba0c637f4155b1e40492f52e816e`, directory
  `20260908T0756240167503Z-disposable-teleportation-resources`, all 19
  assertions PASS with the corrected book fingerprints.

All named runs launched through Steam App ID 640820 and proved zero save writes.
Current clean Release/package log:
`artifacts/teleportation/build-travelers-checkpoint.log`; explicit domain log:
`artifacts/teleportation/domain-travelers.log`. All 1,475 domain cases and strict
package validation passed. Runtime preflight passed 208 checks plus map metadata;
compatibility parameter/settings checks passed 4,120. Working-save regression
`20260908T0806109909820Z-967379eaf9a2461ead657bdc5a2a100a`, directory
`20260908T0806109819970Z-working-save-smoke`, passed all eleven assertions with
zero save writes. No release promotion is implied.

## Complete module boundary checkpoint

Qualified source: `ba32ac2d6e7a916aedbdf61b4dea2ff093d3c746`. All 26 required configurations passed 845 structured assertions through guarded Steam App ID 640820 launches.

The exact expected set was checked: all ON, all OFF, each module alone ON, and each module alone OFF. Each run verified real spell-list publication/rollback, actual destination/familiarity Harmony hooks, and unrelated module decisions. These runs precede the gamepad adapter; its additional hooks require separate qualification.

Settings restored byte-for-byte, SHA-256 `a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`. Machine-local aggregate: `artifacts/teleportation/module-boundary-results.json`. Launch/build/package log: `artifacts/teleportation/runtime-module-boundary-matrix.log`.

Runtime results live under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/<directory>/runtime-result.json`.

| Configuration | Run ID | Directory | Assertions |
| --- | --- | --- | --- |
| All ON | `20260908T0815415736194Z-3b60b100eacf462797a13c6ae78d6608` | `20260908T0815415616195Z-observe-feature-module-settings` | 33 PASS |
| Only teleportationSpells OFF | `20260908T0817374509820Z-9de2031877764baeb08372acf8441100` | `20260908T0817374469805Z-observe-feature-module-settings` | 33 PASS |
| Only elementalRaces OFF | `20260908T0819339602175Z-ec68c5a5e9ca442b90bbd7ada1f4d33b` | `20260908T0819339562186Z-observe-feature-module-settings` | 33 PASS |
| Only protectionFromAlignmentControlImmunity OFF | `20260908T0821302048418Z-08257842553e4102a525980eacf6b89c` | `20260908T0821302018389Z-observe-feature-module-settings` | 33 PASS |
| Only bodyguardFeats OFF | `20260908T0823268216126Z-18a83612f9c44f19b457d10c84f2ebe2` | `20260908T0823268176116Z-observe-feature-module-settings` | 33 PASS |
| Only urbanBarbarian OFF | `20260908T0825231531057Z-49b304583531441e93bef44f65f7addb` | `20260908T0825231531057Z-observe-feature-module-settings` | 33 PASS |
| Only brownFurTransmuter OFF | `20260908T0827205570114Z-f2969d616c2e4727a1277c1bb53d316a` | `20260908T0827205570114Z-observe-feature-module-settings` | 33 PASS |
| Only easternWeapons OFF | `20260908T0829178179571Z-2f3fe46f599a454baae03cfa02cb680e` | `20260908T0829178164602Z-observe-feature-module-settings` | 33 PASS |
| Only elvenBranchedSpears OFF | `20260908T0831137624285Z-fb18394dd5244a249e5263749a372045` | `20260908T0831137584252Z-observe-feature-module-settings` | 33 PASS |
| Only expandedSummoning OFF | `20260908T0833095032941Z-515a2b6143bc453abd18b00518f965f0` | `20260908T0833095032941Z-observe-feature-module-settings` | 33 PASS |
| Only shieldOther OFF | `20260908T0835058062108Z-720a252c8aea4f4689de2101092702dd` | `20260908T0835058051611Z-observe-feature-module-settings` | 33 PASS |
| Only acadamaeGraduate OFF | `20260908T0837022379660Z-f4091c0dd3a84201aeab3a75cddeb922` | `20260908T0837022379660Z-observe-feature-module-settings` | 33 PASS |
| Only gunslinger ON | `20260908T0838591362410Z-eb50421bfa654944b4656436a381713f` | `20260908T0838591332419Z-observe-feature-module-settings` | 33 PASS |
| Only gunslinger OFF | `20260908T0840548541026Z-5e878cc479164ebeb986470bf0373cfb` | `20260908T0840548541026Z-observe-feature-module-settings` | 32 PASS |
| Only acadamaeGraduate ON | `20260908T0842512435461Z-657dae61888c4d38be6add80baf6d2b4` | `20260908T0842512405465Z-observe-feature-module-settings` | 32 PASS |
| Only shieldOther ON | `20260908T0844471399790Z-b3c2fcd52b364125a302fc0833943f37` | `20260908T0844471399790Z-observe-feature-module-settings` | 32 PASS |
| Only expandedSummoning ON | `20260908T0846436149390Z-ebda76c8e7994b82802171cd9516a8f1` | `20260908T0846436109450Z-observe-feature-module-settings` | 32 PASS |
| Only elvenBranchedSpears ON | `20260908T0848395260009Z-18c22f6d0b414c049e6276aafa48f007` | `20260908T0848395260009Z-observe-feature-module-settings` | 32 PASS |
| Only easternWeapons ON | `20260908T0850359675650Z-800c3a9d8c3b4149b476c72b968d2dab` | `20260908T0850359675650Z-observe-feature-module-settings` | 32 PASS |
| Only brownFurTransmuter ON | `20260908T0852324791144Z-3f78b524b69942adb0538abdd64d3139` | `20260908T0852324791144Z-observe-feature-module-settings` | 32 PASS |
| Only urbanBarbarian ON | `20260908T0854285982206Z-7e7c0f4320544059a73b5bba1697731a` | `20260908T0854285982206Z-observe-feature-module-settings` | 32 PASS |
| Only bodyguardFeats ON | `20260908T0856251153884Z-d26aa990c77145119d529354a8d2e67f` | `20260908T0856251153884Z-observe-feature-module-settings` | 32 PASS |
| Only protectionFromAlignmentControlImmunity ON | `20260908T0858217890667Z-c7567cc3354a47aebd2085676d3ee22f` | `20260908T0858217890667Z-observe-feature-module-settings` | 32 PASS |
| Only elementalRaces ON | `20260908T0900178203755Z-5006331c470148d784d526b1d29f81cd` | `20260908T0900178203755Z-observe-feature-module-settings` | 32 PASS |
| Only teleportationSpells ON | `20260908T0902140842296Z-16e359d8470b4e27aa846b2aa66c397c` | `20260908T0902140842296Z-observe-feature-module-settings` | 32 PASS |
| All OFF | `20260908T0904097232535Z-1fcecd2180ee4521a00bd40b7235529c` | `20260908T0904097152777Z-observe-feature-module-settings` | 32 PASS |

## Native gamepad destination qualification

The gamepad adapter appends native ConsoleButton rows after destination
composition and registers them with the existing navigation collection. Native
Travel, other native controls, their order/default and their input layers remain
intact. The shared native confirmation binds the same physical spellbook and
uses the desktop transaction/outcome/relocation implementation.

The final guarded gamepad run passed 39 assertions. It covers no-source,
unvisited, closed and current-point omission; native Travel exactly once before
and after augmentation; six distinct source rows; six reopen cycles; native
cancel and modal replacement ownership; live counts and removal of an exhausted
selected row from navigation; real prepared Teleport, spontaneous Greater
Teleport and pre/post-capital Recall; twelve-source scrolling; exact cleanup;
and zero observed native/mod UI exceptions. Recall appears first only on its
required stable destination. All four completed casts use actual contextual
rows and native confirmation controls. No save write occurred.

TextMeshPro evidence includes every rendered character, numeric percentage,
line break, source count and confirmation label. The native font's uppercase /
small-caps styling is accepted without changing the text. The actual modal uses
ScreenSpaceOverlay, so geometry is measured through its owning canvas rather
than the world-map UI camera. Native fade/rotation must finish; the text and
buttons must fit on screen without truncation or overflow. This is structured
UI evidence, not screenshot inference or a review of every display resolution.

| Scenario/configuration | Assertions | Exact run ID | Result directory |
|---|---:|---|---|
| Native gamepad interaction/casting | 39 PASS | `20260908T1010179638483Z-9fc7ced7478444cf954b1bf08ce52cc1` | `20260908T1010179548235Z-disposable-teleportation-gamepad` |
| Desktop contextual casting | 42 PASS | `20260908T1014214159153Z-9a69a30993ed40c788bc296626ba20c7` | `20260908T1014214079156Z-disposable-teleportation-casting` |
| Desktop interaction | 29 PASS | `20260908T1018389758730Z-7424c79a9121415384eae22539d144d8` | `20260908T1018389608496Z-disposable-teleportation-interaction` |
| Working save | 11 PASS | `20260908T1020106360919Z-795b2fcd0c7d4f39b8cd0ce28ae03e7e` | `20260908T1020106360919Z-working-save-smoke` |
| All modules ON | 34 PASS | `20260908T1021358972525Z-595267a8e06449e7ac21581616b787c8` | `20260908T1021358972525Z-observe-feature-module-settings` |
| Only Teleportation OFF | 34 PASS | `20260908T1022507762681Z-b926a0f844934c46b4aca52592bd744c` | `20260908T1022507762681Z-observe-feature-module-settings` |
| Only Teleportation ON | 33 PASS | `20260908T1024059211546Z-5bdd420157864e94921b3f500a172d77` | `20260908T1024059211546Z-observe-feature-module-settings` |
| All modules OFF | 33 PASS | `20260908T1025217015010Z-d5cfb7f6835c4407a94d48db8ddea8bb` | `20260908T1025217015010Z-observe-feature-module-settings` |

All directories above are beneath
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/` and contain
`runtime-result.json`. The gamepad detail is `teleportation-gamepad.json`;
regressions retain their scenario-specific artifacts and copied native logs.
The seven regressions passed 216 assertions, for 255 assertions including the
new gamepad run. They reuse deployment
`deployments/20260908T1010179218231Z/deployment.json` and the same immutable local
runtime package. Native logs contain no Teleportation errors. Gamepad logs and
the request-local exception observer contain zero native UI exceptions.

The four focused module runs verify actual desktop/gamepad/familiarity hook
counts 4/4/2 when enabled and 0/0/0 when disabled, plus real publication and exact
rollback. The original settings bytes were restored after every configuration;
the independently checked final SHA-256 is
`a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`.
The prior full 26-state/845-assertion matrix remains evidence for ba32ac2;
these four runs qualify the new module hook boundary, not a new full 26-state run.

The complete 1,482-case domain suite, clean Release build, build-output and
strict installable-package checks pass. The relevant local logs are
`artifacts/teleportation/domain-gamepad-extended.log` and
`artifacts/teleportation/build-gamepad-canvas-fields.log`.
The scenario preflight passes 208 checks plus teleportation metadata validation;
compatibility parameter/settings validation passes 4,120 checks. Generated
packages, proprietary inspection output and raw runtime artifacts are uncommitted.

### Rejected gamepad probes and corrections

- `20260908T0912419437069Z-fe953e125eb749049cdfe34df6866906`
  (`20260908T0912419357133Z-disposable-teleportation-gamepad`) stopped before
  fixture casting: the console map existed but only the desktop modal was loaded.
- `20260908T0923268173371Z-5e17835d386049e0b5200f91e23a9f6e`
  (`20260908T0923268083372Z-disposable-teleportation-gamepad`) passed 25 cast/input
  assertions, but subsequent native-log review found 112 local-UI null references
  and console map initialization observer errors. It is not UI-stability proof.
  Controller mode had changed while desktop local-area controls were still live.
- `20260908T0947531104480Z-3592e8882fa14a47ae07b522630b498d`
  (`20260908T0947531004541Z-disposable-teleportation-gamepad`) used a fully loaded
  desktop world map before native console UI reloading. It proved zero exceptions
  and restored state, but failed three case-sensitive rendered-text assertions.
- `20260908T0955417846206Z-df215b0d815e40cb8bba5ec4a03661f5`
  (`20260908T0955417756218Z-disposable-teleportation-gamepad`) retained those
  failures while recording the exact native uppercase/small-caps text and layout.
- `20260908T1001455915923Z-3d662cd0ddeb439c8033200ff9932bae`
  (`20260908T1001455831596Z-disposable-teleportation-gamepad`) exposed an incorrect
  measurement camera: UIUtility.IsTransformInScreen assumes Game.UI.UICamera,
  while this native modal belongs to a separate overlay canvas. The final run
  above uses the correct canvas and waits for native animation completion.

Fixture setup now follows native loading contracts: load the desktop world map,
dispose/unload the desktop loading UI, load/initialize the console loading UI,
and invoke the exact native same-area Game.LoadArea overload with no autosave
and no forced unload. SceneLoader owns UI scene replacement. It leaves no old
local-area controls running under gamepad mode. Before mandatory process exit,
all books/map/ledger changes are restored, native UI contexts are disposed and
controller mode is restored. No input event is synthesized or published.

## Native spellbook and action-bar qualification

The guarded `disposable-teleportation-spellbook-ui` scenario passed **31 of 31**
assertions on run `20260908T1116293771376Z-9db2a72926a64a2aaba36bc40c295800`.
Evidence directory:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260908T1116293681361Z-disposable-teleportation-spellbook-ui`.
Read `runtime-result.json`, `teleportation-spellbook-ui.json`, and the copied
`output_log-spellbook-ui.txt`.

This uses the actual local-area native service window, class/level toggles,
spell rows and pagination. Wizard and Sorcerer Teleport 5 / Greater Teleport 7,
Cleric Word of Recall 6 and Druid Word of Recall 8 all display the correct real
book, spell, icon and complete native description. Native row preparation
allocates real unavailable preparations and displays their actual slots; native
rest makes the preparations ready. All strategic spells remain unavailable in
the local area and absent from the native metamagic selection. After a native
selection refresh, ordinary Dimension Door reaches the actual action bar while
all strategic spells remain absent, including after preparation/rest callbacks.

Native service-window close/reopen works. The captured closed surface is
inactive, alpha 0, blocks no input, and returns to the fixture's native Pause
mode with its action bar active. The scenario restores the exact original
spellbooks/resources/stats, action-bar array and history references/contents,
selection, party, positions, area, time and pause state. Deferred native refresh
is observed after restoration. The UI exception observer records zero exceptions
from fixture setup through cleanup, and save-write sentinels record zero writes.
Four ZFavoredClass `KeyNotFoundException` messages occur during mod startup,
before this fixture; they are not hidden or treated as compatibility PASS.

Same-artifact working-save regression:
`20260908T1119401911470Z-fd2dafbefbba4ea987af7857b8e42855`, **11 of 11 PASS**,
directory `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260908T1119401795087Z-working-save-smoke`.
Both runs use deployment
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260908T1116293351376Z/deployment.json`.
The exact protected working-save fingerprint checks pass; no save is modified.

Rejected first probe:
`20260908T1107377752219Z-0f67a057eddb4f6aa1fe8a9b0a2a9b66`, directory
`20260908T1107377632211Z-disposable-teleportation-spellbook-ui`, failed two of
31 assertions. Its early native positive control had not refreshed because
fixture `Spellbook.AddKnown(..., isCopy: true)` emits no learn event. Its close
assertion incorrectly expected Default mode while the fixture deliberately used
native Pause; an animated child flag was also sampled before it settled. The
corrected fixture reselects the owner through native selection events and checks
the native service-window input surface/HUD, recording actual visibility and
alpha. Both probes had zero UI exceptions and exact cleanup; the first is not a
qualification PASS. Production spell or UI behavior required no change.

The complete 1,482-case domain suite, clean Release build and strict UMM package
validation passed (`artifacts/teleportation/build-spellbook-ui-refresh.log`).
Runtime preflight passed 208 checks; compatibility/settings parameter checks
passed 4,120. This qualifies the native desktop spellbook and action-bar paths in
the installed profile. Level-up selection is covered by the following checkpoint; other compatibility
profiles and full campaign familiarity disk persistence remain open gates.

## Native level-up selection qualification

`disposable-teleportation-level-up` passed **18 of 18** assertions in guarded
Steam run `20260908T1207363385307Z-e204a1b706bf41b6891074349d6bc571`.
Evidence directory:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260908T1207363299683Z-disposable-teleportation-level-up`.
Read `runtime-result.json`, `teleportation-level-up.json` and copied
`output_log-level-up.txt`.

Actual native Wizard and Sorcerer spell selection rows expose Teleport at 5 and
Greater Teleport at 7. The fixture supplies real books immediately below each
native caster-level threshold and temporarily supplies XP eligibility, while
the original character remains level 2. Native class, feature and skill rules
unlock the Spells phase; selection counts and phase flags are never overwritten.
The actual enabled row learns the exact spell only in the isolated native
preview. Escape and the owned native cancellation dialog close each UI; exact
preview cancellation disposes that clone/thread. Original known spells,
resources, levels, class/feature references, XP, UI settings/selection, party,
positions, time and pause remain unchanged. The native inactive warmup backend
and presenter Unit reference are preserved. No save write occurs.

There are zero native/mod exceptions during fixture setup through cleanup.
The same four pre-fixture ZFavoredClass startup exceptions remain in the full
log and are not a compatibility PASS. No completed character advancement or
campaign disk save/reload is claimed by this preview fixture.

Same-artifact regressions, all PASS with protected saves and exact cleanup:

| Scenario | Assertions | Run ID | Directory under the lab runtime-evidence root |
|---|---:|---|---|
| Native spellbook/action bar | 31 | `20260908T1210416696946Z-fde966bb32004196abeb2d42cfcaff29` | `20260908T1210416596942Z-disposable-teleportation-spellbook-ui` |
| Native cast resources | 19 | `20260908T1212152093227Z-811c49b39de842efa75f4f368911cb80` | `20260908T1212152083223Z-disposable-teleportation-resources` |
| Working save | 11 | `20260908T1213478289669Z-9f25ccb87448473880e9630ecbb6aab0` | `20260908T1213478289669Z-working-save-smoke` |

All four runs use deployment
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260908T1207362949656Z/deployment.json`,
DLL SHA-256 `8709efc3ff5446770a6d3425f0601ea386a5f1d84ea7dad855cc591a14d37dde`.
The guarded artifact passed 1,482 domain cases and strict UMM package validation;
runtime preflight passes 208 checks and compatibility/settings parameters 4,120.

Rejected exploratory runs (not qualification PASS):

- `20260908T1149415773399Z-28822031d57f477683d29ae66d91fbfe`, directory
  `20260908T1149415693618Z-disposable-teleportation-level-up`, stopped before
  changes at an overly broad existing-backend prerequisite.
- `20260908T1159123042357Z-5b808abda8c94db7b9587f2faf79de56`, directory
  `20260908T1159122956740Z-disposable-teleportation-level-up`, identified the
  closed UI's native main-menu warmup reference. Exact IL proved its lifecycle;
  the corrected fixture preserves that inactive object without cancelling it.
- `20260908T1203200766808Z-6b98667eceb34b21b35c72975d82f059`, directory
  `20260908T1203200646834Z-disposable-teleportation-level-up`, passed both Wizard
  cases but retained a Sorcerer row across a native deferred refresh. Its bound
  BlueprintAbility was cleared before the fixture read it. Exact compiled IL
  identified the failing fixture read; re-resolving the current native widget
  across frames fixed the probe. Cleanup remained exact and save writes zero.

Production spell publication or player level-up behavior required no change.
Final checkpoint validation also passed the explicit Release domain command
(1,482 tests), clean Release build, repository/build-output checks and strict
installable-package validation. Logs are
`artifacts/teleportation/domain-level-up-checkpoint.log` and
`artifacts/teleportation/build-level-up-checkpoint.log`. The separately built
Release package remains a qualification candidate; its DLL SHA-256 is
`b263ece7fb6a6b1fb34597eb340824a41d8286332240a292b76884acef7198c2`.
It is not substituted for the exact deployed runtime artifact identified above.

## Deferred exploration and special-point qualification

A guarded special-point cast exposed a real native deferred exploration effect:
arrival at `07b9f4001ac3eed4da444c36653c584f` let LocationRevealController change
nearby hidden point `312bf36ac8bc4c74cb0969908c876cce` LastPerceptionRolled from
0 to 12 on subsequent frames. Immediate relocation snapshots alone missed this.
A versioned save-owned arrival boundary now suppresses that one native controller
while the party remains at its magical arrival. Ordinary Travel releases it,
including malformed saved state. OFF installs no prefix. No point-specific blanket
ban, native route replacement, or perception/reveal rollback is used.

Final audit run `20260908T1330037600506Z-cc0ea3d5a53a4123a156a651546ca8c3` passed **68 of 68**,
with zero fixture exceptions, zero save writes and exact cleanup. Evidence:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260908T1330037500523Z-disposable-teleportation-destinations`,
`runtime-result.json`, `teleportation-destinations.json` and copied
`output_log-destinations-recovery-control.txt`. It audits 611 current points;
41 special/type-representative points yield 22 actual native-confirmed Greater
Teleport casts and 19 native campaign exclusions. All five stable point types,
all ten current book events, Oleg/capital and six component points are covered.
Each cast spends one real seventh-level slot, retains native controls and protects
all captured world/party state across deferred frames. Native Travel restores the
actual hidden-point perception check, twice, without spending spell slots.
The [curated point audit](docs/TELEPORTATION-MAP-POINT-ARRIVAL-AUDIT.csv) records exact
IDs and results. Other campaign restriction states and other map scenes remain
outside this proof. No unconditional deny-catalog entry was justified.

All following runs use deployment
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260908T1330037170527Z/deployment.json`,
DLL SHA-256 `5faa368ab26a0db6f07bb26ee144256ea076733caa914b6c3791b1b10492a475`,
package SHA-256 `ebf8ebf0cf5c888d87306e64ddd47b1ebad7807bd454261a96c14298ba050982`.
The artifact passed 1,490 domain cases, clean Release build and strict package
validation. Runtime preflight passes 208 checks and compatibility/settings
parameters 4,120. Log: `artifacts/teleportation/build-exploration-recovery-control.log`.

Seven focused same-artifact regressions passed **164 assertions**:

| Scenario | Assertions | Run ID | Directory under runtime-evidence |
|---|---:|---|---|
| casting | 42 | `20260908T1332488279913Z-4396ce68fb7941f1b5ec051dd99caf6d` | `20260908T1332488179916Z-disposable-teleportation-casting` |
| interaction | 29 | `20260908T1334279588379Z-e2865e0c71b148fea80b350d52eff7fe` | `20260908T1334279578363Z-disposable-teleportation-interaction` |
| gamepad | 39 | `20260908T1336031772247Z-5a93c01e9f9f496abb538ebdbbbfd584` | `20260908T1336031762237Z-disposable-teleportation-gamepad` |
| travelers | 15 | `20260908T1337535092280Z-3c850ceeb7b141c2a9f3dbc53042371e` | `20260908T1337535082277Z-disposable-teleportation-travelers` |
| resources | 19 | `20260908T1339283460896Z-7f9c508249214600918505adb936ac2c` | `20260908T1339283450871Z-disposable-teleportation-resources` |
| familiarity | 9 | `20260908T1341005910988Z-c22b34cefdd043599baab9468db9395b` | `20260908T1341005901011Z-disposable-teleportation-familiarity` |
| working-save-smoke | 11 | `20260908T1342325654068Z-60f7266a522f4d4da5ddcc600885f3ad` | `20260908T1342325644062Z-working-save-smoke` |

All **26** current module boundaries passed **897 assertions**. Actual
Teleportation desktop/gamepad/arrival/exploration hook counts are 4/4/2/1 ON and
0/0/0/0 OFF. Each transaction restores exact original settings bytes; original
SHA-256 is `a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`.
These startup/publication boundaries do not substitute for other mod profiles.

| Configuration (catalog order) | Assertions | Run ID | Directory under runtime-evidence |
|---|---:|---|---|
| on-on-on-on-on-on-on-on-on-on-on-on | 35 | `20260908T1344008733699Z-139e43e24b6c49b39cd858655cf0842c` | `20260908T1344008723720Z-observe-feature-module-settings` |
| on-on-on-on-on-on-on-on-on-on-on-off | 35 | `20260908T1345189914526Z-5a6227730754438fb33413b349b8e519` | `20260908T1345189904556Z-observe-feature-module-settings` |
| on-on-on-on-on-on-on-on-on-on-off-on | 35 | `20260908T1346351706243Z-fd3e691577a342d4a77d57cf09c689b7` | `20260908T1346351696240Z-observe-feature-module-settings` |
| on-on-on-on-on-on-on-on-on-off-on-on | 35 | `20260908T1347525373601Z-03dfdaae426c4d829d7649d5d5bd915d` | `20260908T1347525363627Z-observe-feature-module-settings` |
| on-on-on-on-on-on-on-on-off-on-on-on | 35 | `20260908T1349097110828Z-19ad7a641f0646aca712a7d77acaff67` | `20260908T1349097100821Z-observe-feature-module-settings` |
| on-on-on-on-on-on-on-off-on-on-on-on | 35 | `20260908T1350270593275Z-d0e882584a224893ba2b753b30daf07e` | `20260908T1350270583240Z-observe-feature-module-settings` |
| on-on-on-on-on-on-off-on-on-on-on-on | 35 | `20260908T1351441113813Z-c340d0a94dd244dab8375fbb384e2b36` | `20260908T1351441103819Z-observe-feature-module-settings` |
| on-on-on-on-on-off-on-on-on-on-on-on | 35 | `20260908T1353014240528Z-0d5340aa8e0244039211f7e23c6b13e0` | `20260908T1353014230526Z-observe-feature-module-settings` |
| on-on-on-on-off-on-on-on-on-on-on-on | 35 | `20260908T1354178190348Z-535d581169aa49ed8c3ebae33b83e1cf` | `20260908T1354178180373Z-observe-feature-module-settings` |
| on-on-on-off-on-on-on-on-on-on-on-on | 35 | `20260908T1355353560391Z-49bab445f4994cdfaeeb3e4c365b8682` | `20260908T1355353550392Z-observe-feature-module-settings` |
| on-on-off-on-on-on-on-on-on-on-on-on | 35 | `20260908T1356521660013Z-65b36357e8c643e583052915ac393a44` | `20260908T1356521650011Z-observe-feature-module-settings` |
| on-off-on-on-on-on-on-on-on-on-on-on | 35 | `20260908T1358088768439Z-b52fcd52cbbb41428f384ac9bf8d88d3` | `20260908T1358088758432Z-observe-feature-module-settings` |
| on-off-off-off-off-off-off-off-off-off-off-off | 35 | `20260908T1359262472980Z-ed00acb6cd5c47e2904a46a15c4b968f` | `20260908T1359262462982Z-observe-feature-module-settings` |
| off-on-on-on-on-on-on-on-on-on-on-on | 34 | `20260908T1400429780919Z-10225aea68834dd583ae0800ed535a21` | `20260908T1400429770930Z-observe-feature-module-settings` |
| off-on-off-off-off-off-off-off-off-off-off-off | 34 | `20260908T1401598166208Z-6af6d5897a8149c79de919ae36ed2e4e` | `20260908T1401598160043Z-observe-feature-module-settings` |
| off-off-on-off-off-off-off-off-off-off-off-off | 34 | `20260908T1403163434716Z-0164b9ef4d41448aa23d89ebd9b5ffe2` | `20260908T1403163424709Z-observe-feature-module-settings` |
| off-off-off-on-off-off-off-off-off-off-off-off | 34 | `20260908T1404342721720Z-6ba3589ab884449a9de77eab7b740fd5` | `20260908T1404342711728Z-observe-feature-module-settings` |
| off-off-off-off-on-off-off-off-off-off-off-off | 34 | `20260908T1405514200586Z-62c0b358fcdf439697669242817e157c` | `20260908T1405514190496Z-observe-feature-module-settings` |
| off-off-off-off-off-on-off-off-off-off-off-off | 34 | `20260908T1407077332014Z-717fef89b425406db9f0d64ef05add45` | `20260908T1407077322023Z-observe-feature-module-settings` |
| off-off-off-off-off-off-on-off-off-off-off-off | 34 | `20260908T1408249093394Z-640942fd62bf4f948bd1993aaa87f634` | `20260908T1408249073381Z-observe-feature-module-settings` |
| off-off-off-off-off-off-off-on-off-off-off-off | 34 | `20260908T1409420499989Z-7b97f2f84a2047f7884a4036e3a62b65` | `20260908T1409420489998Z-observe-feature-module-settings` |
| off-off-off-off-off-off-off-off-on-off-off-off | 34 | `20260908T1410589268541Z-082dab0d8d7e4731b3f0e51981158db0` | `20260908T1410589258539Z-observe-feature-module-settings` |
| off-off-off-off-off-off-off-off-off-on-off-off | 34 | `20260908T1412154444841Z-e2e3bac5a96d495cb0560a7a43b72e30` | `20260908T1412154434842Z-observe-feature-module-settings` |
| off-off-off-off-off-off-off-off-off-off-on-off | 34 | `20260908T1413319334092Z-5bdf60f0b53442af9371f4e827585fcf` | `20260908T1413319324108Z-observe-feature-module-settings` |
| off-off-off-off-off-off-off-off-off-off-off-on | 34 | `20260908T1414498150630Z-8bc790ad7fba49938e9141d8c4127215` | `20260908T1414498145408Z-observe-feature-module-settings` |
| off-off-off-off-off-off-off-off-off-off-off-off | 34 | `20260908T1416070697182Z-bf0af4ffc2e14f0f96db3b08a557c615` | `20260908T1416070687182Z-observe-feature-module-settings` |

Rejected and superseded probes are retained truthfully:

- `20260908T1229326505641Z-1ce931b6ab584e448c53eb486d63d8e9`, directory
  `20260908T1229326410251Z-disposable-teleportation-destinations`, ERROR after four
  casts when the fifth changed deferred map state. No save writes; exact cleanup.
- `20260908T1235137817318Z-7c1a929babc7498c87e46f28fdc5d7ee`, directory
  `20260908T1235137707368Z-disposable-teleportation-destinations`, ERROR; narrowed
  differences proved the exact post-commit perception change, with no selection
  difference. This was a production defect and required the saved-arrival guard.
- `20260908T1257287107457Z-52c784f01e1d4050a00ac28ad9613fac`, directory
  `20260908T1257286997479Z-disposable-teleportation-destinations`, ERROR after all 22
  cast invariants passed: the ordinary-travel control lacked a revealed route.
- `20260908T1309026483770Z-583312525794498b890011181056c449`, directory
  `20260908T1309026403790Z-disposable-teleportation-destinations`, passed 67 assertions
  after the adjacent-edge control correction. The 68-assertion run supersedes it
  and adds native recovery from a malformed saved boundary.
- `20260908T1323274762788Z-ec412aec35574be388a64dd51cee8a54`, directory
  `20260908T1323274642770Z-disposable-teleportation-destinations`, ERROR after 66
  passing checks: the fixture tried to start its second Travel from the native
  paused command's null stationary position. Native IL proved Stop's lifecycle;
  a zero-distance fixture reset corrected the probe. No production change was needed.

All destination probes restored tracked state, recorded zero fixture exceptions
and made no save writes. The four known ZFavoredClass startup exceptions occur
before these fixtures and remain recorded; they are not a compatibility PASS.
A preliminary batch was stopped at the outer monitor while its guarded traveler
fixture completed its own cleanup and automatic exit. The complete current batch
above is the qualification evidence; interrupted monitoring is not substituted.

Final checkpoint checks also passed the explicit Release domain command
(`artifacts/teleportation/domain-exploration-recovery-checkpoint.log`) and clean
Release/build-output/repository/strict-package checks
(`artifacts/teleportation/build-exploration-final-checkpoint.log`). The separately
built Release candidate DLL SHA-256 is
`1cb3251989f9a566e62e2fca94df909027c18afc4c7308b30aa1c4ed93339cc3`;
package SHA-256 is
`96883bec0bd9809d67972fc95c9063bbc26f10b633ee8d453b02c318075c03b8`.
This candidate is not substituted for the exact deployed runtime artifact above.

## Remaining qualification and constraints

Full campaign familiarity disk save/reload, mod-provided mount qualification,
other campaign restriction states/map scenes and all required compatibility
profiles remain incomplete. This checkpoint passed the complete 26-state module
matrix. The current UI qualification
uses structured native button invocation and measured on-screen geometry across
frames; it is not a presentation review of every camera position/resolution.

The installed host is UMM 0.33.0.0; the requested 0.32.4 host has not been qualified.
Required Arms and Armor and Toggle Custom Soundpacks references were absent from
the configured reference folder, installed Mods, and inspected project backups.
Owner questions about those references, UMM qualification, and the next unused
release version remain pending. Existing 0.0.115/0.0.116 releases will be preserved.

These checkpoints do not establish complete feature, final compatibility or
release readiness.
