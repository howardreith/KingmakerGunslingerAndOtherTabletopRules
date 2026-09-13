# Native icon screen evidence

`NativeIconScreenEvidence` captures the real game framebuffer using the installed
Unity `ScreenCapture.CaptureScreenshot(string)` API. It runs only inside the
existing guarded creator and spellbook scenarios with automatic exit enabled.
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
dimensions, and restored overlay state. Its seven corruption fixtures run in
repository validation. This check reports provenance only; inspect the images.

These hooks do not yet cover native inventory/merchant scroll views, strategic
destination controls, racial action/variant menus or visible buffs. Track those
consumers separately rather than treating creator/spellbook captures as complete
mission coverage.

The [0.0.128 technical checkpoint](../reports/icon-overhaul/NATIVE-128-QUALIFICATION.json)
records nine passing guarded runs, 116 native screenshots and exact restoration.
Inspected Rapid Reload rows use native P/M/B; selected/sheet data construction is
qualified separately from its remaining rendered-screen review. Mercenary heritage
targets are clear, but some right-hand progression content retains a hover panel.
An empty creator doll in these fixtures is not appearance qualification. The three
strategic descriptions were inspected without target occlusion. Owner approval and
the remaining native surfaces are still pending.
