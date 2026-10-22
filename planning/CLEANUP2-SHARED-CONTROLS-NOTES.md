# Cleanup 2 shared donor controls

## Scope and integration

New partial: `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2SharedControls.cs`.
Entry: `ReviewSummonCleanup2SharedControls(fixture, book)`.
Exact future phase: `shared-controls`, save-free fixture and automatic exit only.
The integrator owns admission, dispatch, project registration, compilation and native execution.
No existing source, save, runtime installation, build or Git operation was changed by this task.

## Exact controls

- Native Mastodon: existing `GiantBodyVisualPolicy.DonorGuid` = `028cc6f46e7998f46855a33ffde89567`, with exact native prefab `fe9f25fe62e0f3b49a13910a996470b2`. `ArmoredDinosaurVisualPolicy.TriceratopsDonorGuid` must agree. The same registered blueprint object is required from the live library. Its prefab is loaded read-only before either actor is created.
- Native creation reuses the qualified Sprint27 `EntityCreator.SpawnUnit(native, caster.Position, Quaternion.identity, fixture.Scene)` path. Only its exact returned fresh blueprint-owned instance, absent from intake/party/exact-start snapshots and distinct from other fixture actors, is registered. Actual scene, state and view-data identity are then required. This direct native control is not represented as a canonical spell summon.
- Released Triceratops uses `Cleanup2GiantRoute` to derive the actual SNA own-tier single parent/child and unit, then `Cleanup2CastGiant` for the prepared-slot/native command/cast/spawn path. The caster's fixture-granted spellbook access and synchronously advanced cast are explicit limitations. No tier or GUID was guessed.
- Both controls wait for native fade/appearance/dissolve using `Cleanup2WaitAppearance`. No appearance buff removal, shader forcing, stat changes, trample setup, hit/act stimulus, movement command or Working-save access is added.

## Evidence and ownership

The observer captures the native prefab's original skin/mesh/material/bone/action-set references and selected scalar identities before creation. The native Mastodon must use those same original mesh references and action set, without an authored-body receipt. The released Triceratops must have its exact private authored mesh/material receipt and retain the native original donor mesh and action-set identity.

Three native front/side/rear PNGs are taken for each control. Native Mastodon remains alive through Triceratops attachment while the observer compares its actual live references, native stats/resources and root scale with its earlier state. It is then destroyed before the Triceratops pictures so the two large co-located controls cannot mask each other. The disposable Human caster can still appear in the scene; image pixels require review and no occlusion-free or visual-approval assertion is made by capture counts.

Before/after checks include the exact source blueprint/prefab-link/loaded-view identity, source and actor meshes/materials/bone palettes/root bones, enabled state, action-set/list/action/clip identities, selected material properties, root local scale, native size/Corpulence, six abilities, HP/damage, AC/BAB/speed/saves, actual resource blueprint amounts and primary/secondary weapon GUIDs. Native dissolve progress is excluded from immutable material comparison and is independently required to be settled for every image. No native mesh vertices/indices/weights, bind matrices or texture pixels are exported.

Hooks and capture collections/counters are restored by exact request ownership. Only positively created actors are disposed. Failure after a successful canonical spawn still leaves the existing cast helper's exact positive actor registered for shared fixture teardown. Exact captured Triceratops resources get a bounded Unity destruction drain; a surviving reference is unresolved release, never a global leak claim. No global object scan is used.

## Bounds and limitations

The phase has a 120-second wall bound checked during waits and before each of six synchronous renders; existing appearance waits also retain their own frame/time bounds. Exact resource teardown drains at most 120 Unity frames/3 seconds within that phase budget. Safe cleanup runs even after a timeout.

Appearance can use the existing qualified synthetic fixture clock. Its pulse count and game-seconds advancement are recorded; idle image waits suspend those pulses. No new frame driver is added. This targeted control does not exercise travel, actual attack/hit, trample, native combat timing, cold-load/persistence, selection clicks or a full 117-frame art packet. Those are not inferred from its stills.

## Source verification

Read the current validation policy, the prior Sprint25 Triceratops control, the qualified Sprint27 direct native donor control, current fixture creation/destruction and shared cast/appearance helpers. Read-only reflection against actual installed Kingmaker 2.1.7b verified `UnitAbilityResourceCollection.Enumerable`/`GetResourceAmount`, `HandSlot.MaybeWeapon`, and native `AnimationSet.Actions`/`AnimationActionBase.Clips` contracts. This task did not compile, run domain tests or launch native gameplay; the integrator must do focused compilation and this exact native phase on the new candidate. Existing native PASS records do not apply to this new observer.
