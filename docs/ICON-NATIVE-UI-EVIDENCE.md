# Native icon screen evidence

`NativeIconScreenEvidence` captures the real game framebuffer using the installed
Unity `ScreenCapture.CaptureScreenshot(string)` API. It runs only inside the
existing guarded creator, spellbook and learning scenarios with automatic exit enabled.
It does not construct substitute screens, edit pixels, send input, or authorize
any save operation. The mission's [current state](../planning/ICON-OVERHAUL-STATE.md)
records whether a particular artifact has actually passed these checks.

The creator pauses its existing state machine at race, heritage, feature,
rendered selection and final-review checkpoints. Each capture retains the exact
controller/unit ownership through completion. Metadata records active rows,
parameters, sprites and active TMP text/font/overflow observations. The historical
`visibleIconRows` field uses `activeInHierarchy`, which can include rows outside a
scroll viewport. It does not by itself establish that a row appears in the PNG.
The exact disposable Gunslinger cases prefer already legal, rendered Weapon Focus
Pistol (Ifrit), Musket (Oread), Blunderbuss (Sylph), or Rapid Reload (Undine).
This changes only the test fixture's choice; eligibility and production menus
remain unchanged. Other creator scenarios retain their existing choice policy.
The capture also checks actual Rapid Reload row data and the active TMP glyph;
constructor-only evidence previously missed the static menu's raw feature path.
The exact Gunslinger case additionally reveals existing Weapon Focus P/M/B and
Nodachi rows through their own native `ScrollRect`. The already allowlisted Fighter
case first learns its exotic proficiency through the native feat choices, then
chooses Weapon Focus with its other feat: Wakizashi for Ifrit, Katana for Oread,
and Elven Branched Spear for Sylph/Undine, when the matching module is enabled.
The capture requires the exact proficiency in the native preview; it never grants
facts directly. Untrained exotic categories are correctly absent from the native
Weapon Focus list. The viewport helper preserves
the normalized position and velocity, waits for layout, verifies vertical row
bounds, and restores scrolling before the creator continues. Target parameter/
category identities accompany those captures. No selection or eligibility is changed.

The supported creator routes are `disposable-elemental-character-creation-baseline`,
`disposable-elemental-character-creation-case`,
`working-save-elemental-character-creation`, and
`working-save-elemental-character-creation-regression`. The existing working
regression with `allocation=roll` uses the native CustomCompanion blueprint and
records `nativeMercenaryFixture`; the source baseline/case uses the ordinary
default character. Use those recorded identities when labeling screenshots.
The original controller cancellation/restoration and save-write guards remain
mandatory. In-memory fixture completion is not a disk persistence test.

`disposable-teleportation-spellbook-ui` holds each real spell description and
prepared-spell view while capturing. Its six physical book/spell pairings cover
Teleport, Greater Teleport and Word of Recall. Original spellbooks, resources,
action bars, selection, party, area, time and settings must still be restored by
the existing fixture. Local casting remains disabled for the strategic spells.

`disposable-teleportation-level-up` now captures the already qualified native
Wizard/Sorcerer learning rows through the same viewport hold. Its existing isolated
preview, exact row identity, native prerequisites and cancel/cleanup assertions
remain mandatory. The capture never commits a level or writes a save. This
extension passed its current-artifact preview/cancel qualification in the
[viewport record](../reports/icon-overhaul/NATIVE-VIEWPORT-QUALIFICATION.json).

Use the existing runtime orchestrator through Steam App ID 640820, the exact
validated package/deployment manifests and current version. For example, supply
`-Scenario disposable-elemental-character-creation-case` with
`-Parameters @{race='Ifrit';class='Gunslinger';allocation='point-buy'}` and
`-ExitAfterCompletion:$true`. The two working creator routes and spellbook route
also require `-SaveName KMG_AUTOMATION_WORKING` through their documented guarded
load. New save creation or writes require separate owner authorization.

The helper waits across rendered frames for a completed PNG with matching
framebuffer dimensions, records its SHA-256 and writes `native-ui-screens.json`
beside the run's result. No existing capture is overwritten. Loss of ownership,
changed dimensions or an incomplete capture fails the scenario and retains its
incomplete status. Each filename is local to one exact run/artifact.

The capture helper closes UMM's startup overlay through its normal
`UI.ToggleWindow(false)` API and restores the original window state during
cleanup. It requires the plain mod list (both settings indexes `-1`) and an
empty UMM game-script callback list before closing. It invokes no settings-save
operation, changes no window preference, and rejects an unexpected reopening.
Capture waits for the native blocking Canvas to disappear across real frames.
The window state and restoration evidence accompany the screenshot manifest.
The first experimental creator captures were occluded by UMM; they remain
diagnostic evidence and do not qualify native icon appearance.
During each capture the native `TooltipsController.SetTemporaryCooldown()` API
clears incidental hover tips while preserving explicit description windows. The
helper allows real rendering to settle and holds that short cooldown until the
PNG completes. Normal hover resumes after the native delay. No mouse movement,
input, tooltip preference or persistent setting is changed.

Inspect the actual PNGs for scale, clipping, overlays, framing and readability.
Structured UI objects establish mechanical identity; screenshots support visual
judgment; the owner's image approval is a separate decision. A file/hash check
alone establishes neither beauty nor correct visible placement. Raw screenshots,
native reference material and runtime dumps remain outside Git. Curate run IDs,
hashes, observations and review decisions in the mission report.

Run `tools/validate_native_icon_screens.py --evidence <run>/native-ui-screens.json
--build-manifest <exact-package>.build-local.json` to check the paired runtime
result, loaded-build identity, capture MVID, completed frames, PNG hashes and
dimensions, restored overlay state and explicit row viewport/restoration metadata.
Its nineteen corruption fixtures run in
repository validation. This check reports provenance only; inspect the images.

These hooks do not yet cover strategic
destination controls, racial action/variant menus or visible buffs. Track those
consumers separately rather than treating creator/spellbook captures as complete
mission coverage.

The [0.0.128 technical checkpoint](../reports/icon-overhaul/NATIVE-128-QUALIFICATION.json)
records nine passing guarded runs, 116 native screenshots and exact restoration.
Inspected Rapid Reload rows use native P/M/B; selected/sheet data construction is
qualified separately from the later rendered fact-slot checkpoint below. Mercenary heritage
targets are clear, but some right-hand progression content retains a hover panel.
An empty creator doll in these fixtures is not appearance qualification. The three
strategic descriptions were inspected without target occlusion. Owner approval and
the remaining native surfaces are still pending.

The subsequent [viewport checkpoint](../reports/icon-overhaul/NATIVE-VIEWPORT-QUALIFICATION.json)
records six passing runs, 86 assertions, 138 original screenshots and verified
136-file restoration. Its ten weapon targets include native P/M/B, NO, WK, KA and
EB. The inspected WK glyph has no overflow/truncation at 1280x720; no clipping
correction was needed at that scale. All four learning rows are clear. Its first
fixture assumption failed because untrained exotics are correctly filtered;
the successful Fighter cases learned proficiency through normal feat choices.

Actual selected facts are qualified separately from constructor data. The native
Total list passes real Feature objects through `SetData(IUIDataProvider)`; the
character sheet uses `SetFeature(Feature)`. The exact P/M/B adapter invokes native
SetIcon/TMP while preserving facts, parameters, ranks, names, blueprint fallbacks,
font, border/mask sequences and all unrelated rows.

The actual Total list has a disabled PreferredSize fitter with content height
862.03 and preferred height 1,461. Only a firearm-containing list in that exact
state temporarily enables vertical fitting with horizontal fitting unconstrained.
It restores original settings on hide/refill/disable. It never assigns a fixed
height or moves individual rows. Metadata and negative tests require the actual
native before/after modes, extent and viewport. Scroll restoration compares enabled
axes and real content coordinates; an inactive horizontal normalized value can
change from rounding despite unchanged geometry.

The exact working Gunslinger regression with rolled allocation holds each of its
three registered disposable mercenaries for the real Abilities sheet. Normal legal
choices cover Weapon Focus P/M/B, or Rapid Reload P/M/B for Undine. Existing owned
creator cleanup first restores the original global backend and presenter, because
native hide clears only the visible backend. The sheet restores the previous
character/section, group, selection, party, area, time and pause context before
the existing item/money/membership rollback. The remote actor is never enrolled
in the active party and no save is written. Use `-TimeoutSeconds 900` for these
three-character captures; the creator deadline uses this field, not CompletionTimeout.

The [selected-fact qualification](../reports/icon-overhaul/NATIVE-FACT-SLOT-QUALIFICATION.json)
records nine PASS runs, 185 assertions, 255 original PNGs and exact 136-file
installation restoration. Six actual sheets and their Total rows preserve native
P/M/B and all controls, with ten sheet restoration checks per actor. Both module-OFF
boundaries, dependent-feat data, ordinary creator paths, learning and working-save
smoke pass on the same DLL. Earlier offscreen, backend and timeout attempts remain
FAIL in the journal; partial correct glyphs never qualify a failed run.

All six current sheets, current Total P/WK controls and four learning targets were
inspected clear at 1280x720. Generated names overlap the header and ordinary creator
dolls are empty; neither is appearance qualification. Remote abilities are
deactivated and do not qualify an active action bar. Higher dependent roots have
native data coverage, not blanket rendered-sheet evidence. Full racial/action/buff,
strategic-control coverage remains separate, as does owner approval.

The qualified scroll extension adds inventory and item-description captures to the
existing `disposable-teleportation-spellbook-ui` scenario. It requires initial
absence of all three strategic scrolls, creates one exact identified entity per
spell, opens the real native inventory and uses its measured virtual slots to
scroll. Actual ItemSlot sprites and TooltipTrigger item references must match
the canonical scroll/spell identity, with non-vacuous unchanged item controls.
OpenDescriptionWindow initializes tooltip data from the actual slot; validate
both the collected object and TooltipData.Item after this native call. The native
item-description window must show the same scroll name/icon.
No equip, use, copy, purchase or save operation is added. Normal native window
closure precedes exact removal of the three additions and restoration of item
references/order/counts/indices/identification/charges, gold, filter preferences
and sheet/group context. The outer spellbook fixture still enforces its full
world/resource/UI cleanup and zero-exception guards. The [scroll inventory qualification](../reports/icon-overhaul/NATIVE-SCROLL-INVENTORY-QUALIFICATION.json)
records two PASS runs, 49 assertions, 16 original screenshots and exact 136-file
restoration. All six scroll targets were inspected clear at 1280x720. Each of
the 15 item/UI cleanup predicates passed, with no save writes. The first tooltip
lifecycle mistake remains a documented ERROR; partial inventory success was not
qualification. Two focused corruption cases bring capture validation to sixteen.
Native merchant views and final owner UI approval remain separate.


The qualified merchant extension adds native merchant rows within that same
fixture. A detached, unregistered merchant owns a private stock collection with
three canonical scrolls and one ordinary item control. It has no shared vendor
table and cannot identify the player's existing inventory. Native VendorUI
HandleTradeStarted/HandleTradeExit must bind and close the actual Game.Vendor and
Store; purchase/sale baskets stay empty, and no Deal, use or save is invoked.
The fixture restores original vendor references, filters, group and UI collections
after native close clears the virtual rows. Native Collection retains its last
reference, so the fixture restores that scalar only when its exact owned binding
and empty rows are verified, before disposing its own stock/actor. Outer inventory rollback also restores
native display indices. The [merchant qualification](../reports/icon-overhaul/NATIVE-SCROLL-MERCHANT-QUALIFICATION.json)
records two PASS runs, 53 assertions and 19 original captures on the same DLL.
All three merchant targets were inspected clear at 1280x720. Twelve merchant
cleanup predicates, fifteen inventory predicates and outer fixture restoration
passed with zero save writes. All 136 original installation files/settings were
independently verified restored. The seventeenth corruption case requires real
merchant ownership and empty trade evidence. Owner approval remains separate.


The racial feat extension is qualified. Within the exact disposable
Gunslinger creator case, the existing general-feat selector's real ShowAll toggle
reveals the native list without selecting feats or changing prerequisites. Each
case captures its race-specific targets; native ShowAll may also display
wrong-race entries as unavailable.
Four race cases cover eleven identities. Each target records the actual native
Image, complete TMP title, eligibility, Toggle and native availability markers,
plus nonempty ordinary icon controls. Both native scroll component types are
supported. The fixture restores the filter through its native toggle, waits for
layout, then restores exact scroll position/velocity and checks original preview,
facts and selection. A required result assertion rejects missing target coverage
or restoration. Two corruption tests reject wrong art, missing or altered state,
wrong race/filter and vacuous controls. Disabled rows remain valid native states;
these observations do not qualify feat mechanics or owner visual approval.

The [racial feat qualification](../reports/icon-overhaul/NATIVE-RACIAL-FEAT-QUALIFICATION.json)
records four creator PASS runs plus same-artifact working-save smoke: 59 assertions
and fourteen target captures covering all eleven racial feats. Every target was
inspected clear at 1280x720 with its complete title. Inner Flame, Blazing Aura,
Wings of Air, Inner Breath and Triton Portal retain their observed unavailable
states where prerequisites are unmet. Each creator's exact filter, native scroll,
selection and preview facts were restored, final review was reached and canceled,
and the original installation was restored. These are actual UI objects and
native images; action/variant/buff surfaces and owner approval remain separate.


The qualified bounded extension captures the exact existing
`disposable-teleportation-interaction` destination panel. Six native prepared and
spontaneous Teleport/Greater Teleport buttons retain their real source keys,
two-line CompactRow labels, native font/background, ordinary controls and
resources/familiarity. Production refreshes action snapshots each frame, so
ownership follows actual Buttons and stable source keys/text. Capture uses the
existing native viewport helper and restores its scroll. No source is selected
or cast by the capture; the original interaction scenario then resumes its
existing behavior and complete rollback. New capture exceptions fail the run.
The [strategic control qualification](../reports/icon-overhaul/NATIVE-STRATEGIC-CONTROL-QUALIFICATION.json)
records two PASS runs, 53 assertions and six inspected 1280x720 captures. Every
reopen measured exactly 454 layout units after native readiness; native source
resources, original controls, full outer cleanup and same-artifact smoke passed.
The failed initial timing probe remains separately recorded. All 136 original
installation files/settings were restored. The shared renderer also supplies scroll
and Word of Recall text controls without an icon assignment; that is a source
audit, not separate native screenshot evidence for those choices. Their painted
spellbook/item/description/merchant surfaces remain separately qualified.


The [v0.0.129 integration](../reports/icon-overhaul/NATIVE-129-INTEGRATION-QUALIFICATION.json)
retains both upstream camera stabilization and native panel fade/layout readiness.
Five guarded runs pass 208 assertions and yield 30 inspected native captures,
including the newly available normal Oracle Recall learning row. The capture
validator binds exact class/spell/level combinations and exact stage identities;
wrong Oracle mappings, levels, previews and unavailable rows fail. Its twenty-first
focused case covers this integration. A fresh full build reproduces the tested
DLL/package exactly after that Python validation update. Original installation
restoration is independently verified. Historical reports keep their stated
artifact hashes. All target images are clear; incidental hover panels and the
Wizard fixture's stale central level header are explicitly limited in the report.
Owner approval remains separate from these checks.

The next extension holds the first existing remote mercenary in each exact
`working-save-elemental-character-creation-regression` Gunslinger/roll race
case. Thirteen painted buff identities are divided across the four races.
Native `FactCollection.ActiveByDefault` must already be false and the mercenary
must remain outside the active world before any addition. Its descriptor keeps
its exact original enabled state; native remote registration leaves that true.
Its native BuffCollection receives owned inactive facts and one ordinary Bless
control; neither lifecycle flag nor effect activation is changed. The real character sheet supplies icons, titles,
native dimming and timers. Capture proves exact real Buff references, sprites,
unclipped labels, native control and unchanged original facts/units/area effects.
The fixture uses the actual buff section-group membership and native IsShowed,
and calls SetDirty before Refresh so same-owner cached rows are rebound.
Buff rows must already be visible: their explicit observation mode calls no
scroll setters and checks native bounds, actual content position, velocity and
axes. Fitting-content normalized ratios are recorded only for diagnosis.
Cleanup removes only owned additions and clears their retained pooled-row
references. Native sheet/viewport and outer creator restoration remain required.
The [native buff qualification](../reports/icon-overhaul/NATIVE-RACIAL-BUFF-QUALIFICATION.json)
records five PASS runs, 63 assertions and thirteen inspected target identities
on one artifact, with exact creator/installation restoration and no save writes.
This qualifies the native inactive buff presentation only; active effects, action
bars and variant menus remain distinct. Native Show finishes at alpha 1 here;
the fixture retains the observed native presentation and imposes no dimming.

The qualified glyph extension reads final native TMP mesh vertices and exact
character sequences. In native Overflow mode a label can exceed its nominal
text or unmasked layout-row rectangle while remaining fully visible. Those
containment results and the raw overflow flag are diagnostics. Complete glyph
emission, containment in every real clipping mask, separation from every other
visible buff row, and icon/timer separation are required independently. Missing
mask or neighboring-row evidence fails closed. No text, font or layout is changed.

## Pending native racial action extension

The locally preserved, unqualified action draft extends the guarded
Gunslinger/roll elemental creator case. It binds its first
request-owned remote mercenary to the native ability-group element after the
character sheet restores. It covers 26 parents, ten native variants, one
activatable and two held-touch deliveries (39 consumers). It preserves the
original party selection and action-bar manager owner. When the creator leaves
an empty or multiple selection and hidden unbound bar, the fixture uses the normal native
MultiSelect callback on the existing party leader, then restores the original
selection and waits for the native hide animation. It never forces manager
ownership or visibility. Native inactive slot caches may warm through selection;
no request-owned fact or owner remains in pooled UI. Existing native facts
are retained; only absent facts are added to already-inactive collections.
Native group and variant buttons must already be available. No ability is cast,
activatable switched on, resource granted or native disabled state overridden.

Held-touch presentation uses the native UnitPartTouch.Init with the exact
source, delivery and execution parameters; it never calls the sticky-touch
casting action. Chill Touch also requires its real request-owned retention
part. Existing charges cannot be replaced. Captures retain the native border,
counts, availability and dimming, with an ordinary Fighting Defensively control.
Actual icon screen corners, canvas camera and clipping masks establish visible
placement; no layout change is made. The validator independently checks exact
consumer/parent identities and geometry.

Every request preserves original actor facts, activation, resources, part
references, commands, damage, positions, party, selection, inventory, money,
time and area. Native pooled slots release only request-owned references;
original group contents and UnitUISettings restore, including deferred updates.
This extension is not yet runtime qualified. Inactive native presentation does
not establish combat behavior or owner visual approval. Save writes remain
forbidden by the existing guarded request.

The racial action fixture owns an initially absent, empty native
`UnitPartAbilityModifiers` cache because the native action-type getter ensures
it while rendering. It never adds a modifier entry, and removes only its exact
empty cache after releasing native UI references. Existing cache identity and
all original part membership must survive cleanup. This rule is part of the
pending action qualification; it does not change gameplay action costs.
