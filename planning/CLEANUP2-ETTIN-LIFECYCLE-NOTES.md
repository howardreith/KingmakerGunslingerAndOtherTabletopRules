# Cleanup 2 Ettin lifecycle observer

New source only: `RuntimeTestRunner.SummonCleanup2EttinLifecycle.cs`.
Entry: `ReviewSummonCleanup2EttinLifecycle(fixture, Spellbook book)`.
Future exact phase: `ettin-lifecycle`; registration, compile entry, guarded
fixture selection and native execution belong to the integrator. No existing
partial, catalog, project, script, game state, save or Git state was changed by
this source lane. Compilation and native results remain pending.

Frozen source SHA-256:
`9166db3bc10841634d71c48846a6142023aabe16951fcce70442ae9aae2d0945`.
No new assembly dependency is introduced beyond existing Assembly-CSharp and
Unity Core/animation references. The integrator needs only this new compile
entry, exact phase/guard admission and dispatch with the already prepared book.

## Exact route and private objects

The observer derives the actual own-tier single from the current catalog and
the existing canonical parent/slot cast helpers. Ettin currently publishes
**SNA V**, not VII. The exact unit is
`2481bc73abd64ef0ae581b48438b5404`, using the retained Hill Giant prefab
`635d6dee1c7831b41aa987bdbeff14bc`. Two independent real canonical casts supply
the initial actors; a third real cast supplies the recreated actor. No direct
unit/debug spawn supplies a lifecycle actor.

`EttinSecondHeadVisual` creates one `KMG_Ettin_SecondHead_Root` under the
original `Head` bone's neck parent, one `..._SkullControl`, one authored mesh
`KMG_Ettin_AuthoredHead`, a two-entry neck/skull palette, and four original
materials. Its `CaptureOwnedResources` returns its exact root/private mesh and
the union of those original materials with currently assigned renderer
materials, including native controller-created instances. The observer holds
those actual object references and their captured identity/type/name; it
never substitutes a scene-wide count for ownership.

Each live actor must have one project root, one original native head, one
project skull, one authored head renderer, four assigned material slots and
live exact ownership. The initial pair's private objects must be disjoint.
The sibling must retain its actual view, head mesh, assigned material
references and captured original/controller material objects during the
other actor's expiry and the cast-again cycle.

## Native poses and no-orbit measurement

The actual installed `AttackHandInfo` constructor accepts the real hand slot;
`CreateAnimationHandleForAttack` selects its native main/off/special action
and weapon animation style. The observer executes those native handles for
both real Ettin hand slots. It issues no UnitAttack, attack rule, hit, damage
or fabricated combat result. Finishing an owned handle uses the existing
qualified animation completion helper.

A request-only observer component samples in `LateUpdate` at execution order
32000, after default-order `EttinHeadPoseDriver.LateUpdate`. The actual installed
Unity CoreModule exposes `DefaultExecutionOrder(int)`. This avoids comparing
the current source head with a project skull awaiting its same-frame follow.
The component is attached only to the exact owned project head root and is
disabled/destroyed or destroyed with that root during teardown.

Samples contain scalar original-head/skull/neck world points, both head
positions in the same neck basis, their offset and rotation-follow error.
The no-orbit predicate requires at least twelve retained samples and positive
original-head rotation relative to the neck (>0.1 degree), fixed neck-local
offset within 0.005 local units and rotation-follow error within 0.5 degree.
An unchanged head pose cannot prove absence of orbit. No source/project head
transform is assigned by the observer; no mesh vertices/indices/weights or
native texture pixels are exported. Image aesthetics and actual attack
resolution remain separate evidence.

## Expiry, sibling isolation and release

Only a unit already in the exact fixture-created list with positive caster
provenance can have its single exact native `SummonedUnitBuff` expired.
`ExpireSprint16OwnedBuff` is the established RTWP fixture seam: it sets only
that owned marker's deadline to just before the native clock, then invokes
native `BuffCollection.UpdateNextEvent/Tick`. This is an explicit targeted
deadline stimulus, not proof of waiting the full printed duration. No direct
buff removal or fixture entity destruction supplies the expiry assertion.

The first actor must become logically destroyed, lose its view and leave its
holding state within 1800 updates/18 seconds. Exact resource references then
receive a separate native entity-destroyer/Unity drain bounded to 180 updates
or six seconds. Surviving references are reported as unresolved release after
that bound, not an invented claim of a permanent leak. No forced object
destruction makes a failed resource assertion pass. The sibling's exact head
and materials must remain live and unchanged.

The third canonical cast must produce one fresh independent head; the expired
objects stay destroyed and the two live actors each retain one project head.
Both remaining native markers are subsequently expired and their exact
resources observed through the same bounded drain. The whole phase has a
120-second wall bound. Finally blocks stop only owned handles/observers and
commands, remove unique capture hooks, restore prior capture/pause fields,
and leave every positively created actor registered for shared teardown on
failure. No human actor or unproven captured unit becomes cleanup authority.

## Reconstruction and cold-load limit

Actual 2.1.7b `EntityDataBase.AttachToViewOnLoad(view)` assigns an existing
view and may copy its UniqueId, or calls `CreateViewForData` for a missing
view, then assigns `View.Data`. That data setter invokes `OnDataAttached`,
`OnViewAttached` and view activation. It is a native load lifecycle surface;
the current runtime harness has no qualified ordinary-unit detach/rebuild
fixture proving how to retire the prior view and preserve all owner state.
The observer therefore does not call it, invoke protected OnDataAttached as
fake hydration, or clone a prefab and label it reload.

View reconstruction is explicitly **NOT EXERCISED** by this phase. The
qualified `giant-humanoids-ii` Working persistence path remains the separate
required cold-load proof. This observer makes no save write and cannot close
that remaining ART-05 acceptance item by itself.
